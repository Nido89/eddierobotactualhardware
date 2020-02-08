//------------------------------------------------------------------------------
//  <copyright file="Kinect.cs" company="Microsoft Corporation">
//      Copyright (C) Microsoft Corporation.  All rights reserved.
//  </copyright>
//------------------------------------------------------------------------------
namespace Microsoft.Robotics.Services.Sensors.Kinect
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Net;
    using System.Threading;
    using Microsoft.Ccr.Core;
    using Microsoft.Dss.Core.Attributes;
    using Microsoft.Dss.Core.DsspHttp;
    using Microsoft.Dss.Core.DsspHttpUtilities;
    using Microsoft.Dss.ServiceModel.Dssp;
    using Microsoft.Dss.ServiceModel.DsspServiceBase;
    using Microsoft.Kinect;
    using W3C.Soap;
    using submgr = Microsoft.Dss.Services.SubscriptionManager;
    
    /// <summary>
    /// Container class to store processed visible and depth images for updating Alternates
    /// </summary>
    internal class ProcessedImageArrays
    {
        /// <summary>
        /// WebCam image - flipped to match the orientation of the depth image
        /// </summary>
        public byte[] VisibleImage;

        /// <summary>
        /// Depth image (in mm)
        /// </summary>
        public short[] DepthImage;
    }
    
    /// <summary>
    /// Kinect camera service implementation
    /// </summary>
    [Contract(Contract.Identifier)]
    [DisplayName("(User) Kinect")]
    [Description("Represents a Kinect depth camera")]
    public partial class KinectService : DsspServiceBase
    {
        /// <summary>
        /// Internal (exclusive) port for reading Kinect for the purpose of updating Alternates
        /// </summary>
        private readonly Port<bool> alternateContractUpdatePort;

        /// <summary>
        /// Internal port for update ticks to AlternateContractUpdatePort and maintaining target 
        /// requested FPS
        /// </summary>
        private readonly Port<DateTime> scheduleNextFrameReadPort;

        /// <summary>
        /// Query type constant
        /// </summary>
        private const string QueryTypeString = "type";

        /// <summary>
        /// Query string used when constructing uri for Rgb stream.
        /// </summary>
        private const string RgbQueryString = "rgb";

        /// <summary>
        /// Query string used when constructing uri for depth  stream.
        /// </summary>
        private const string DepthQueryString = "depth";

        /// <summary>
        /// Query string used when constructing uri for depth 8 bits and RGB combined stream.
        /// </summary>
        private const string DepthPlusRgb = "depthplusrgb";

        /// <summary>
        /// When polling Kinect sensor - we want frame reading functions to succeed no matter how 
        /// long it takes
        /// So we define this const as sort of 'infinite'
        /// </summary>
        private const int AVeryLargeNumberOfMilliseconds = 1000 * 1000;

        /// <summary>
        /// Kinect Sensor must be kept at class level
        /// </summary>
        private KinectSensor kinectSensor;

        /// <summary>
        /// Cached responce to "GetRawFrames" to avoid high frequency allocations/deallocations of 
        /// this expensive structure
        /// </summary>
        private GetRawFrameResponse cachedRawFrameQueryResponse;

        /// <summary>
        /// Last Raw frames
        /// </summary>
        private RawKinectFrames cachedRawFrames = new RawKinectFrames();

        /// <summary>
        /// Used to keep requested FPS
        /// </summary>
        private double lastFrameReadTime = 0.0;

        /// <summary>        
        /// Cached arrays where we'll store processed depth cam and web cam alternate data
        /// </summary>
        private ProcessedImageArrays cachedProcessedFrames;
        
        /// <summary>
        /// Service state
        /// </summary>
        [ServiceState, InitialStatePartner(Optional = true, ServiceUri = "Kinect.user.config.xml")]
        private KinectState state = new KinectState();

        /// <summary>
        /// Main Operations port
        /// </summary>
        [ServicePort("/kinect", AllowMultipleInstances = false)]
        private KinectOperations mainPort = new KinectOperations();

        /// <summary>
        /// Subscribtion manager port
        /// </summary>
        [SubscriptionManagerPartner]
        private submgr.SubscriptionManagerPort submgrPort = new submgr.SubscriptionManagerPort();

        /// <summary>
        /// XSLT transform for viewing web/depth cam alternates
        /// </summary>
        [EmbeddedResource("Microsoft.Robotics.Services.Sensors.Kinect.Kinect.user.xslt")]
        private string transform = string.Empty;

        /// <summary>
        /// We use this to make sure we don't flood logs
        /// </summary>
        private bool alternateUpdateErrorReportedAtLeastOnce = false;

        /// <summary>
        /// Delegate declarations for behaviors that will be used to implement state-specific 
        /// functionality
        /// </summary>
        private delegate void TrivialDelegate();

        /// <summary>
        /// Delegate declaration for webcam alternate         
        /// </summary>
        private delegate void ProcessVisualImageForWebCamAlternateDelegate(
            KinectFrameInfo frame,
            byte[] rawColorData,
            byte[] visibleImage);

        /// <summary>
        /// Delegate declaration for depthcam alternate         
        /// </summary>        
        private delegate void ProcessDepthDataForDepthCamAlternateDelegate(
            KinectFrameInfo frame,
            short[] rawDepthData,
            short[] depthImage);

        /// <summary>
        /// Delegate declaration for read color frame        
        /// </summary>        
        private delegate void ReadColorFrameDelegate(
            RawKinectFrames rawFrames,
            ColorImageStream kinectStream);

        /// <summary>
        /// Delegate declaration for read depth frame        
        /// </summary>        
        private delegate void ReadDepthFrameDelegate(
            RawKinectFrames rawFrames,
            DepthImageStream kinectStream);

        /// <summary>
        /// Delegate declaration for read skeleton frame        
        /// </summary>                
        private delegate void ReadSkeletonDelegate(
            RawKinectFrames rawFrames,
            SkeletonStream kinectStream);

        /// <summary>
        /// Delegate declaration for depthcam alternate
        /// </summary> 
        private delegate void UpdateDepthCamAlternateDelegate(byte[] webCamData, short[] depthData);

        /// <summary>
        /// Delegate declaration for webcam alternate
        /// </summary> 
        private delegate void UpdateWebCamAlternateDelegate(byte[] imageData);

        // To perform actual Kinect reading, initialization, polling, etc - we invoke below routines 
        // (ending with *Behavior) by default all behaviors are set to NoOp - we'll wire them up 
        // (if needed) to do  useful, configuration-appropriate stuff  at startup. We do this to avoid 
        // riddling the mainline code with too  many duplicate condition checks throughout. 
        // I.e. if webcam alternate was not configured to be updated during polling - we'll leave 
        // ReadVideoFrameForAlternateBehavior empty, but may still wire up  ReadVideoFrameOnDemandBehavior 
        // to read Kinect. This way, we don't pay the penalty of needless video frame processing  
        // and the code remains readable. 
        private TrivialDelegate InitializeVideoStreamBehavior = delegate { };

        private TrivialDelegate InitializeDepthStreamBehavior = delegate { };

        private TrivialDelegate InitializeSkeletalEngineBehavior = delegate { };

        private TrivialDelegate InitializeWebCamAlternateBehavior = delegate { };

        private TrivialDelegate InitializeDepthCamAlternateBehavior = delegate { };

        private ProcessVisualImageForWebCamAlternateDelegate ProcessVisualImageForWebCamAlternateBehavior =
            delegate(KinectFrameInfo frame, byte[] rawColorData, byte[] visibleImage) { };

        private ProcessDepthDataForDepthCamAlternateDelegate ProcessDepthDataForDepthCamAlternateBehavior =
            delegate(KinectFrameInfo frame, short[] rawDepthData, short[] depthImage) { };

        private TrivialDelegate StartPollingBehavior = delegate { };

        private ReadColorFrameDelegate ReadColorFrameOnDemandBehavior =
            delegate(RawKinectFrames rawFrames, ColorImageStream kinectStream) { };

        private ReadDepthFrameDelegate ReadDepthFrameOnDemandBehavior =
            delegate(RawKinectFrames rawFrames, DepthImageStream kinectStream) { };

        private ReadSkeletonDelegate ReadSkeletalFrameBehavior =
            delegate(RawKinectFrames rawFrames, SkeletonStream kinectStream) { };

        private ReadColorFrameDelegate ReadColorFrameForAlternateBehavior =
            delegate(RawKinectFrames rawFrames, ColorImageStream kinectStream) { };

        private ReadDepthFrameDelegate ReadDepthFrameForAlternateBehavior =
            delegate(RawKinectFrames rawFrames, DepthImageStream kinectStream) { };

        private UpdateDepthCamAlternateDelegate UpdateDepthCamAlternateBehavior =
            delegate(byte[] webCamData, short[] depthData) { };

        private UpdateWebCamAlternateDelegate UpdateWebCamAlternateBehavior = 
            delegate(byte[] imageData) { };

        /// <summary>
        /// Initializes a new instance of the KinectService class
        /// </summary>
        /// <param name="creationPort">Service creation port</param>        
        public KinectService(DsspServiceCreationPort creationPort)
            : base(creationPort)
        {
            this.alternateContractUpdatePort = new Port<bool>();
            this.scheduleNextFrameReadPort = new Port<DateTime>();
        }

        /// <summary>
        /// Service start method
        /// </summary>
        protected override void Start()
        {
            if (this.state == null)
            {
                this.state = new KinectState();
                this.state.FrameRate = KinectState.MaxFrameRate;
                this.state.DeviceID = 0;
                this.state.UseColor = true;
                this.state.UseDepth = true;
                this.state.UseSkeletalTracking = true;
                this.state.ColorImageFormat = ColorImageFormat.RgbResolution640x480Fps30; 
                this.state.DepthImageFormat = DepthImageFormat.Resolution320x240Fps30;                
                this.state.DepthStreamRange = DepthRange.Default;                
                this.state.TransformSmooth = false;
                this.state.IsDepthServiceUpdateEnabled = true;
                this.state.IsWebCamServiceUpdateEnabled = true;
                
                this.SaveState(this.state);
            }

            this.panTiltState = InitialPanTiltState();

            this.cachedProcessedFrames = new ProcessedImageArrays();
            this.cachedRawFrameQueryResponse = new GetRawFrameResponse();
            this.cachedRawFrameQueryResponse.RawFrames = new RawKinectFrames();

            this.WireUpStateSpecificBehaviors();

            SpawnIterator(this.InitializeKinect);            
        }
        
        /// <summary>
        /// A factory-like method that hooks up proper image reading and processing routines depending on startup 
        /// config parameters w/o this - the rest of the code would be riddled with if-elses and difficult to read. 
        /// So, we centralize all of those if-elses here in one spot.
        /// </summary>
        private void WireUpStateSpecificBehaviors() 
        {
            this.WireUpVideoProcessingBehaviors();

            this.WireUpDepthProcessingBehaviors();

            this.WireUpSkeletalTrackingBehaviors();
        }

        /// <summary>
        /// Wire up the skeletal tracking behaviors
        /// </summary>
        private void WireUpSkeletalTrackingBehaviors()
        {
            if (this.state.UseSkeletalTracking)
            {
                this.ReadSkeletalFrameBehavior = this.DoReadSkeletalFrame;
                this.InitializeSkeletalEngineBehavior = this.DoEnableSkeletalStream;
            }
        }

        /// <summary>
        /// Wire up the depth processing behaviors
        /// </summary>
        private void WireUpDepthProcessingBehaviors()
        {
            if (this.state.UseDepth)         
            {
                this.ReadDepthFrameOnDemandBehavior = this.DoReadDepthFrame;
                this.InitializeDepthStreamBehavior = this.DoOpenDepthStream;

                if (this.state.IsDepthServiceUpdateEnabled)
                {
                    this.ReadDepthFrameForAlternateBehavior = this.DoReadDepthFrame;
                    this.InitializeDepthCamAlternateBehavior = this.DoInitializeDepthCamAlternate;
                    this.StartPollingBehavior = this.DoPollKinectCamera;
                    this.UpdateDepthCamAlternateBehavior = this.DoUpdateDepthCamAlternate;
                    this.ProcessDepthDataForDepthCamAlternateBehavior = 
                        this.DoProcessDepthAndPlayerIndexDataForDepthCamAlternate;                
                }
            }
        }

        /// <summary>
        /// Wire up the video processing behaviors
        /// </summary>
        private void WireUpVideoProcessingBehaviors()
        {
            if (this.state.UseColor)
            {
                this.InitializeVideoStreamBehavior = this.DoOpenVideoStream;
                this.ReadColorFrameOnDemandBehavior = this.DoReadColorFrame;

                if (this.state.IsWebCamServiceUpdateEnabled)
                {
                    this.ReadColorFrameForAlternateBehavior = this.DoReadColorFrame;
                    this.InitializeWebCamAlternateBehavior = this.DoInitializeWebcamAlternate;
                    this.StartPollingBehavior = this.DoPollKinectCamera;
                    this.UpdateWebCamAlternateBehavior = this.DoUpdateWebCamAlternate;

                    if (this.state.ColorImageFormat == ColorImageFormat.RawYuvResolution640x480Fps15)
                    {
                        this.ProcessVisualImageForWebCamAlternateBehavior = 
                            this.DoProcessVisualImageForWebCamAlternateYUVRaw;
                    }
                    else
                    {
                        this.ProcessVisualImageForWebCamAlternateBehavior = 
                            this.DoProcessVisualImageForWebCamAlternateRgbYuv;
                    }
                }
            }
        }

        /// <summary>
        /// Helper function to initialize Kinect camera.
        /// </summary>
        /// <returns>CCR Iterator</returns>
        protected IEnumerator<ITask> InitializeKinect()
        {
            var finishedInitialization = new SuccessFailurePort();

            ThreadPool.QueueUserWorkItem(
                param =>
                {
                    try
                    {
                        this.InitializeKinectDevice();

                        finishedInitialization.Post(SuccessResult.Instance);
                    }
                    catch (Exception e)
                    {
                        LogError(e);
                        finishedInitialization.Post(new Exception(Resources.InitializeFailed));
                    }
                });

            yield return finishedInitialization.Choice(
                success =>
                {
                    base.Start();

                    this.StartPollingBehavior();

                    // Merge the internal update ports into the main interleave
                    MainPortInterleave.CombineWith(
                        new Interleave(
                            new ExclusiveReceiverGroup(
                                Arbiter.Receive(
                                true, 
                                this.alternateContractUpdatePort, 
                                this.OnUpdateAlternates)),
                            new ConcurrentReceiverGroup(
                                Arbiter.Receive(
                                true,
                                this.scheduleNextFrameReadPort, 
                                this.OnScheduleNextFrameRead))));

                    LogInfo(Resources.Initialized);
                },
                fault =>
                {
                    LogError(fault);
                    base.StartFailed();
                });
        }

        /// <summary>
        /// Initializes Kinect device
        /// </summary>
        private void InitializeKinectDevice()
        {
            this.kinectSensor = KinectSensor.KinectSensors[this.state.DeviceID];

            this.kinectSensor.Start(); 

            this.SetKinectTiltAngle((int)this.state.TiltDegrees);

            this.InitializeVideoStreamBehavior();

            this.InitializeDepthStreamBehavior();

            this.InitializeWebCamAlternateBehavior();

            this.InitializeDepthCamAlternateBehavior();
                        
            this.InitializeSkeletalEngineBehavior();
        }

        /// <summary>
        /// Initiate polling
        /// </summary>
        protected void DoPollKinectCamera()
        {
            this.scheduleNextFrameReadPort.Post(DateTime.Today);
        }

        /// <summary>
        /// Read frames for alternates 
        /// </summary>
        /// <param name="rawFrames">Raw Kinect Frames</param>
        /// <param name="kinectSensorParam">Kinect Sensor Parameters</param>
        private void ReadKinectFramesForAlternates(RawKinectFrames rawFrames, KinectSensor kinectSensorParam)
        {
            this.ReadColorFrameForAlternateBehavior(
                rawFrames, 
                kinectSensorParam.ColorStream);

            this.ReadDepthFrameForAlternateBehavior(
                rawFrames, 
                kinectSensorParam.DepthStream);

            this.lastFrameReadTime = Common.Utilities.ElapsedSecondsSinceStart;
        }

        /// <summary>
        /// Read kinect frames when requested via GetFrame
        /// </summary>
        /// <param name="rawFrames">Raw Kinect Frames</param>
        /// <param name="kinectSensorParam">Kinect Sensor</param>
        /// <param name="includeDepth">Inclue depth?</param>
        /// <param name="includeVideo">Inclue video?</param>
        /// <param name="includeSkeletons">Inclue skeletons?</param>
        private void ReadKinectFramesOnDemand(
            RawKinectFrames rawFrames, 
            KinectSensor kinectSensorParam,
            bool includeDepth, 
            bool includeVideo, 
            bool includeSkeletons)
        {
            if (includeVideo)
            {
                this.ReadColorFrameOnDemandBehavior(
                    rawFrames,
                    kinectSensorParam.ColorStream);
            }

            if (includeDepth)
            {
                this.ReadDepthFrameOnDemandBehavior(
                    rawFrames,
                    kinectSensorParam.DepthStream);
            }            

            if (includeSkeletons)
            {
                this.ReadSkeletalFrameBehavior(rawFrames, kinectSensorParam.SkeletonStream);
            }

            this.lastFrameReadTime = Common.Utilities.ElapsedSecondsSinceStart;
        }

        /// <summary>
        /// DoReadColorFrame  Frame Read Behavior
        /// </summary>
        /// <param name="rawFrames">Raw frames</param>
        /// <param name="kinectStream">Kinect stream</param>                        
        private void DoReadColorFrame(RawKinectFrames rawFrames, ColorImageStream kinectStream)
        {
            ColorImageFrame kinectFrame = kinectStream.OpenNextFrame(AVeryLargeNumberOfMilliseconds);

            if (kinectFrame != null)
            {
                rawFrames.RawColorFrameInfo = new KinectFrameInfo(kinectFrame);
                rawFrames.RawColorFrameData = new byte[kinectFrame.PixelDataLength];
                kinectFrame.CopyPixelDataTo(rawFrames.RawColorFrameData);
            }            
        }

        /// <summary>
        /// DoReadDepthFrame  Frame Read Behavior
        /// </summary>
        /// <param name="rawFrames">Raw frames</param>
        /// <param name="kinectStream">Kinect stream</param>
        private void DoReadDepthFrame(RawKinectFrames rawFrames, DepthImageStream kinectStream)
        {
            DepthImageFrame kinectFrame = kinectStream.OpenNextFrame(AVeryLargeNumberOfMilliseconds);

            if (kinectFrame != null)
            {
                rawFrames.RawDepthFrameInfo = new KinectFrameInfo(kinectFrame);
                rawFrames.RawDepthFrameData = new short[kinectFrame.PixelDataLength];
                kinectFrame.CopyPixelDataTo(rawFrames.RawDepthFrameData);                
            }
        }
        
        /// <summary>
        /// Read skeletal frame if needed
        /// </summary>
        /// <param name="rawFrames">Raw frames</param>
        /// <param name="skeletonStream">Skeleton stream</param>
        private void DoReadSkeletalFrame(RawKinectFrames rawFrames, SkeletonStream skeletonStream)
        {
            SkeletonFrame skeletonFrame = skeletonStream.OpenNextFrame(AVeryLargeNumberOfMilliseconds);
                
            if (skeletonFrame != null)
            {
                rawFrames.RawSkeletonFrameData = new SkeletonDataFrame(skeletonFrame);
            }            
        }
        
        /// <summary>
        /// OnSubscribe - Handle subscription messages
        /// </summary>
        /// <param name="subscribe">Subscribe request</param>        
        [ServiceHandler]
        public void OnSubscribe(Subscribe subscribe)
        {
            SubscribeHelper(this.submgrPort, subscribe.Body, subscribe.ResponsePort);
        }

        /// <summary>
        /// OnSetFrameRate - Set the frame rate at which WebCamSensor and DepthCamSensor 
        /// are updated. 
        /// </summary>
        /// <param name="update">SetFrameRate update request</param>  
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public void OnSetFrameRate(SetFrameRate update)
        {
            var rate = update.Body.FrameRate;

            if (rate > KinectState.MaxFrameRate)
            {
                update.ResponsePort.Post(
                    Fault.FromCodeSubcodeReason(
                        FaultCodes.Sender,
                        DsspFaultCodes.OperationFailed,
                        Resources.FrameRateOutOfRange));
            }
            else
            {                
               this.state.FrameRate = rate;
                update.ResponsePort.Post(DefaultUpdateResponseType.Instance);
                SendNotification(this.submgrPort, update);                
            }
        }
                
        /// <summary>
        /// Http Get handler 
        /// </summary>
        /// <param name="get">Http Get Request</param>
        [ServiceHandler]
        public void HttpGetHandler(HttpGet get)
        {            
            HttpResponseType rsp = new HttpResponseType(HttpStatusCode.OK, this.state, this.transform);
            get.ResponsePort.Post(rsp);            
        }

        /// <summary>
        /// Get handler. We need explicit implementation to read actual tilt value 
        /// which may have changed (i.e. due to robot vibration)
        /// from what it was set in the config
        /// </summary>
        /// <param name="get">Get Request</param>
        [ServiceHandler]
        public void GetHandler(Get get)
        {
            this.state.TiltDegrees = this.kinectSensor.ElevationAngle;
            
            get.ResponsePort.Post(this.state);
        }

        /// <summary>
        /// Http Query Handler
        /// </summary>
        /// <param name="query">Htt Query Request</param>
        /// <returns>CCR Task Chunk</returns>        
        [ServiceHandler]
        public IEnumerator<ITask> HttpQueryHandler(HttpQuery query)
        {
            return this.OnDepthcamHttpQuery(query);            
        }

        /// <summary>
        /// UpdateTiltHandler - Change the Kinect tilt.
        /// </summary>
        /// <param name="update">Update tilt request</param>          
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public void UpdateTiltHandler(UpdateTilt update)
        {            
            var tilt = update.Body.Tilt;

            // need constants
            if (tilt > (short) kinectSensor.MaxElevationAngle ||
                tilt < (short) kinectSensor.MinElevationAngle)
            {
                update.ResponsePort.Post(
                    Fault.FromCodeSubcodeReason(
                        FaultCodes.Sender,
                        DsspFaultCodes.OperationFailed,
                        Resources.InvalidTiltAngle));
            }
            else
            {
                this.SetKinectTiltAngle((int)tilt);
                this.UpdateTiltAngle(this.state.TiltDegrees);
                update.ResponsePort.Post(DefaultUpdateResponseType.Instance);
                SendNotification(this.submgrPort, update);                
            }
        }

        /// <summary>
        /// UpdateSkelatalSmoothingHandler - change skeletal smoothing parameters.
        /// </summary>
        /// <param name="update">Update Skeletal Smoothing request</param>          
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public void UpdateSkeletalSmoothingHandler(UpdateSkeletalSmoothing update)
        {
            this.state.TransformSmooth = update.Body.TransfrormSmooth;
            this.state.SkeletalEngineTransformSmoothParameters =
                update.Body.SkeletalEngineTransformSmoothParameters;
            
            if (this.state.TransformSmooth)
            {
                this.kinectSensor.SkeletonStream.Enable(this.state.SkeletalEngineTransformSmoothParameters);
            }
            else
            {
                this.kinectSensor.SkeletonStream.Enable();
            }

            update.ResponsePort.Post(DefaultUpdateResponseType.Instance);
            SendNotification(this.submgrPort, update);            
        }

        /// <summary>
        /// Sets the tilt angle on Kinect camera
        /// </summary>
        /// <param name="newAngle">New tilt angle</param>
        private void SetKinectTiltAngle(int newAngle)
        {
            this.kinectSensor.ElevationAngle = newAngle;
            this.state.TiltDegrees = (double)newAngle;
        }

        /// <summary>
        /// Returns x and y coordinates in the color image that correspond 
        /// to a given depth point
        /// </summary>
        /// <param name="query">DepthToColorImage request</param>          
        [ServiceHandler]
        public void DepthToColorImage(DepthToColorImage query)
        {
            var response = new DepthToColorResponse();

            
            ColorImagePoint imagePoint;
            DepthImagePoint depthPoint = new DepthImagePoint();

            depthPoint.X = query.Body.X;
            depthPoint.Y = query.Body.Y;
            depthPoint.Depth = (int)query.Body.Depth;

            imagePoint = this.kinectSensor.CoordinateMapper.MapDepthPointToColorPoint(
                this.kinectSensor.DepthStream.Format,
                depthPoint,
                this.kinectSensor.ColorStream.Format);

            response.X = imagePoint.X;
            response.Y = imagePoint.Y;

            query.ResponsePort.Post(response);            
        }

        /// <summary>
        /// SkeletonToDepthImage Handler 
        /// </summary>
        /// <param name="query">SkeletonToDepthImage request</param>          
        [ServiceHandler]
        public void SkeletonToDepthImage(SkeletonToDepthImage query)
        {
            var response = new SkeletonToDepthImageResponse();
            
            response.DepthPoint = this.kinectSensor.CoordinateMapper.MapSkeletonPointToDepthPoint(
                            query.Body.SkeletonVector,
                            this.kinectSensor.DepthStream.Format);
            
            query.ResponsePort.Post(response);
        }

        /// <summary>
        /// SkeletonToColorImage Handler 
        /// </summary>
        /// <param name="query">SkeletonToColorImage request</param>          
        [ServiceHandler]
        public void SkeletonToColorImage(SkeletonToColorImage query)
        {
            var response = new SkeletonToColorImageResponse();

            ColorImagePoint colorPoint = this.kinectSensor.CoordinateMapper.MapSkeletonPointToColorPoint(
                                            query.Body.SkeletonVector,
                                            this.kinectSensor.ColorStream.Format);

            response.X = colorPoint.X;
            response.Y = colorPoint.Y;            

            query.ResponsePort.Post(response);
        }

        /// <summary>
        /// DepthImageToSkeleton Handler 
        /// </summary>
        /// <param name="query">DepthImageToSkeleton request</param>          
        [ServiceHandler]
        public void DepthImageToSkeleton(DepthImageToSkeleton query)
        {
            var response = new DepthImageToSkeletonResponse();

            DepthImagePoint depthPoint = new DepthImagePoint();

            depthPoint.X = query.Body.X;
            depthPoint.Y = query.Body.Y;
            depthPoint.Depth = query.Body.Depth;

            response.SkeletonVector = this.kinectSensor.CoordinateMapper.MapDepthPointToSkeletonPoint(
                this.kinectSensor.DepthStream.Format,
                depthPoint);

            query.ResponsePort.Post(response);
        }  

        /// <summary>
        /// GetDepthProperties Handler 
        /// </summary>
        /// <param name="query">GetDepthProperties request</param>          
        [ServiceHandler]
        public void GetDepthProperties(GetDepthProperties query)
        {
            var response = new GetDepthPropertiesResponse();

            response.MaxDepthValue = this.kinectSensor.DepthStream.MaxDepth;
            response.MinDepthValue = this.kinectSensor.DepthStream.MinDepth;
            response.TooFarDepthValue = this.kinectSensor.DepthStream.TooFarDepth;
            response.TooNearDepthValue = this.kinectSensor.DepthStream.TooNearDepth;
            response.UnknownDepthValue = this.kinectSensor.DepthStream.UnknownDepth;

            if (response.MaxDepthValue == 0)
            {
                // Kinect doesn't correctly return MaxDepth/MinDepth in "Default" mode
                response.MaxDepthValue = 4000;
                response.MinDepthValue = 800;
            }

            query.ResponsePort.Post(response);            
        }
        
        /// <summary>
        /// HandleHttpPost
        /// Handles the HttpPost message generated when a browser sends a form
        /// post to the service main port.
        /// </summary>
        /// <param name="post">The Dssp operation.</param>        
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public void HandleHttpPost(HttpPost post)
        {
            var request = post.GetHeader<HttpPostRequestData>();

            if (request != null &&
                request.TranslatedOperation != null)
            {
                if (request.TranslatedOperation is UpdateTilt)
                {
                    var tilt = (UpdateTilt)request.TranslatedOperation;
                    this.UpdateTiltHandler(tilt);
                }                                   
                else if (request.TranslatedOperation is SetFrameRate)
                {
                    var fps = (SetFrameRate)request.TranslatedOperation;
                    this.OnSetFrameRate(fps);
                }  
            }
            else
            {
                throw new InvalidOperationException();
            }

            post.ResponsePort.Post(new HttpResponseType(
                HttpStatusCode.OK, this.state, this.transform));        
        }

        /// <summary>
        /// GetRawFrame Handler. We pre-alloc CachedRawFrameQueryResponse for perf reasons, as 
        /// this is expected to be a high frequency 
        /// call, and we'd like to avoid puting more load on garbage collector
        /// </summary>
        /// <param name="query">GetRawFrame request</param>          
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public void GetRawFrame(QueryRawFrame query)
        {
            this.ReadKinectFramesOnDemand(
                this.cachedRawFrames, 
                this.kinectSensor, 
                query.Body.IncludeDepth, 
                query.Body.IncludeVideo, 
                query.Body.IncludeSkeletons);

            // we won't pass back data unless explicitly requested, even if there is a cached
            // data for the frame
            this.cachedRawFrameQueryResponse.RawFrames.RawDepthFrameData =
                query.Body.IncludeDepth ? this.cachedRawFrames.RawDepthFrameData : null;

            this.cachedRawFrameQueryResponse.RawFrames.RawDepthFrameInfo =
                query.Body.IncludeDepth ? this.cachedRawFrames.RawDepthFrameInfo : null;

            this.cachedRawFrameQueryResponse.RawFrames.RawColorFrameData =
                query.Body.IncludeVideo ? this.cachedRawFrames.RawColorFrameData : null;

            this.cachedRawFrameQueryResponse.RawFrames.RawColorFrameInfo =
                query.Body.IncludeVideo ? this.cachedRawFrames.RawColorFrameInfo : null;

            this.cachedRawFrameQueryResponse.RawFrames.RawSkeletonFrameData =
                query.Body.IncludeSkeletons ? this.cachedRawFrames.RawSkeletonFrameData : null;

            query.ResponsePort.Post(this.cachedRawFrameQueryResponse);
        }

        /// <summary>
        /// Issue an actual KinectSensor poll when time comes. There is a ping-pong between those two
        /// ports (and subsequently OnUpdateAlternates and OnScheduleNextFrameRead callbacks). 
        /// They activate each other when they are done. This method was chosen to ensure that we 
        /// a) maintain roughly the FPS requested b) don't issue next poll requests unless previous 
        /// frame has been completed c) perform reading on a callback that's on main interleave's 
        /// exclusive list which is necessary to do since while polling is going on - a Get request 
        /// may be issued - and since we keep only one cached deep copy of raw Kinect frame - 
        /// we might invalidate that data while its being passed back to consumer
        /// </summary>
        /// <param name="dummyValueThatsNeverUsed">The parameter is not used.</param>
        private void OnScheduleNextFrameRead(DateTime dummyValueThatsNeverUsed)
        {
            double targetDelayBetweenFrameReads = 1.0 / this.state.FrameRate;
            
            double elapsedTimeSinceLastFrameRead =
                Common.Utilities.ElapsedSecondsSinceStart - this.lastFrameReadTime;

            double differenceBetweenTargetAndActualDelaysBetweenPolls = 
                targetDelayBetweenFrameReads - elapsedTimeSinceLastFrameRead;

            // if we are close to our goal frequency - don't delay - frame rate is not an exact 
            // science and by delaying  when there is very little time left until next read if due 
            // - we will skew effective frame rate towards lower value of half frame (~0.015) is 
            // chosen roughtly as a half max frame rate delay (30 fps)
            double gracePeriod = (1.0 / KinectState.MaxFrameRate) / 2; 

            if (differenceBetweenTargetAndActualDelaysBetweenPolls > gracePeriod)
            {
                // again - we don't want to wait for exact amount of time left until the next frame is due
                // otherwise we'll end up reducing the effective frame rate beyond what's required. 
                int millisecondsToSleep = 
                    (int)(1000.00 * (differenceBetweenTargetAndActualDelaysBetweenPolls - (gracePeriod / 2)));

                // recurse back - not time yet to schedule another read.
                Activate(TimeoutPort(millisecondsToSleep).Receive(this.OnScheduleNextFrameRead));
            }
            else
            {
                // ping pong to OnUpdateAlternates
                this.alternateContractUpdatePort.Post(true);
            }
        }

        /// <summary>
        /// Update alternates with processed images for video and depth
        /// </summary>
        /// <param name="dummyValue">The parameter is not used.</param>
        private void OnUpdateAlternates(bool dummyValue)
        {
            try
            {
                this.ReadFramesForAlternateContracts();

                this.UpdateWebCamAlternateBehavior(this.cachedProcessedFrames.VisibleImage);

                this.UpdateDepthCamAlternateBehavior(
                    this.cachedProcessedFrames.VisibleImage, this.cachedProcessedFrames.DepthImage);
            }
            catch (Exception e)
            {
                if (!this.alternateUpdateErrorReportedAtLeastOnce)
                {
                    this.alternateUpdateErrorReportedAtLeastOnce = true;
                    LogError(new Exception(Resources.UpdateAlternateError, e));
                }
            }

            // ping pong to OnScheduleNextFrameRead
            this.scheduleNextFrameReadPort.Post(DateTime.Today);
        }

        /// <summary>
        /// Opens Kinect color stream
        /// </summary>
        private void DoOpenVideoStream()
        {
            this.kinectSensor.ColorStream.Enable(this.state.ColorImageFormat); 
        }

        /// <summary>
        /// Opens Kinect depth stream
        /// </summary>
        private void DoOpenDepthStream()
        {            
            this.kinectSensor.DepthStream.Range = this.state.DepthStreamRange;
            this.kinectSensor.DepthStream.Enable(this.state.DepthImageFormat); 
        }

        /// <summary>
        /// Enables skeletal stream
        /// </summary>
        private void DoEnableSkeletalStream()
        {
            if (this.state.TransformSmooth)
            {
                this.kinectSensor.SkeletonStream.Enable(this.state.SkeletalEngineTransformSmoothParameters);
            }
            else
            {
                this.kinectSensor.SkeletonStream.Enable();
            }
        }

        /// <summary>
        /// Reads depth and video frames for the sole purpose of updating alternate contracts
        /// </summary>
        private void ReadFramesForAlternateContracts() 
        {
            this.ReadKinectFramesForAlternates(this.cachedRawFrames, this.kinectSensor);
            
            this.ProcessVisualImageForWebCamAlternateBehavior(
                this.cachedRawFrames.RawColorFrameInfo, this.cachedRawFrames.RawColorFrameData, this.cachedProcessedFrames.VisibleImage);

            this.ProcessDepthDataForDepthCamAlternateBehavior(
                this.cachedRawFrames.RawDepthFrameInfo, this.cachedRawFrames.RawDepthFrameData, this.cachedProcessedFrames.DepthImage);            
        }
    }
}
