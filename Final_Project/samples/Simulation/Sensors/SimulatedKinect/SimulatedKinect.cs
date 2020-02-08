// -----------------------------------------------------------------------
// <copyright file="SimulatedKinect.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Robotics.Services.Simulation.Sensors.SimulatedKinect
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.Linq;
    using System.Net;
    using System.Runtime.InteropServices;

    using Microsoft.Ccr.Core;
    using Microsoft.Dss.Core.Attributes;
    using Microsoft.Dss.Core.DsspHttp;
    using Microsoft.Dss.Core.DsspHttpUtilities;
    using Microsoft.Dss.ServiceModel.Dssp;
    using Microsoft.Dss.ServiceModel.DsspServiceBase;
    using Microsoft.Kinect;
    using Microsoft.Robotics.Services.DepthCamSensor;
    using Microsoft.Robotics.Services.Sensors.Kinect;
    using Microsoft.Robotics.Services.WebCamSensor;
    using Microsoft.Robotics.Simulation.Engine;
    using W3C.Soap;

    using kinect = Microsoft.Robotics.Services.Sensors.Kinect.Proxy;
    using simengine = Microsoft.Robotics.Simulation.Engine;
    using submgr = Microsoft.Dss.Services.SubscriptionManager;
    using xna = Xna.Framework;

    /// <summary>
    /// The SimulatedKinect service
    /// </summary>
    [DisplayName("(User) Simulated Kinect")]
    [Description("Provides access to a simulated Kinect.")]
    [Contract(Contract.Identifier)]
    [AlternateContract(kinect.Contract.Identifier)]
    public partial class SimulatedKinect : DsspServiceBase
    {
        /// <summary>
        /// The maximum frames per second that Kinect can deliver
        /// </summary>
        private const int MaxFramesPerSec = 30;

        /// <summary>
        /// The kinect state state
        /// </summary>
        [ServiceState, InitialStatePartner(Optional = true, ServiceUri = "SimulatedKinect.user.config.xml")]
        private kinect.KinectState state = new kinect.KinectState();

        /// <summary>
        /// The kinect operations port
        /// </summary>
        [ServicePort("/simulatedkinect", AllowMultipleInstances = true)]
        private kinect.KinectOperations kinectOps = new kinect.KinectOperations();

        /// <summary>
        /// The kinect subscription manager
        /// </summary>
        [SubscriptionManagerPartner]
        private submgr.SubscriptionManagerPort kinectSubmgrPort = new submgr.SubscriptionManagerPort();

        /// <summary>
        /// Depth Camera transform used to render the depth cam on a web page.
        /// </summary>
        [EmbeddedResource("Microsoft.Robotics.Services.Simulation.Sensors.SimulatedKinect.SimulatedKinect.user.xslt")]
        private string xslttransform = null;

        /// <summary>
        /// The kinect entity
        /// </summary>
        private KinectEntity kinectEntity;

        //// The following variables are used to simulate the rate at which device
        //// can serve data.

        /// <summary>
        /// The poll interval
        /// </summary>
        private TimeSpan pollInterval;

        /// <summary>
        /// The poll Port
        /// </summary>
        private Port<DateTime> pollPort = new Port<DateTime>();

        /// <summary>
        /// The pendingFrameResponses
        /// </summary>
        private List<kinect.QueryRawFrame> pendingFrameRequests = new List<kinect.QueryRawFrame>();

        /// <summary>
        /// The frameNumber
        /// </summary>
        private int frameNumber;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="SimulatedKinect"/> class.
        /// </summary>
        /// <param name="creationPort">The creation port.</param>
        public SimulatedKinect(DsspServiceCreationPort creationPort) : base(creationPort)
        {
        }

        /// <summary>
        /// Gets the handler.
        /// </summary>
        /// <param name="get">The get request.</param>
        [ServiceHandler]
        public void GetHandler(kinect.Get get)
        {
            get.ResponsePort.Post(this.state);
        }

        /// <summary>
        /// Subscribe the kinect service.
        /// </summary>
        /// <param name="subscribe">The subscription request.</param>
        [ServiceHandler]
        public void SubscribeHandler(kinect.Subscribe subscribe)
        {
            SubscribeHelper(this.kinectSubmgrPort, subscribe.Body, subscribe.ResponsePort);
        }

        /// <summary>
        /// Sets the frame rate handler.
        /// </summary>
        /// <param name="update">The update.</param>
        [ServiceHandler]
        public void SetFrameRateHandler(kinect.SetFrameRate update)
        {
            var rate = update.Body.FrameRate;

            if (rate < 1 || rate > kinect.KinectState.MaxFrameRate)
            {
                update.ResponsePort.Post(
                    Fault.FromCodeSubcodeReason(
                        FaultCodes.Receiver,
                        DsspFaultCodes.OperationFailed,
                        Resources.FrameRateOutOfRange));
            }
            else
            {
                this.pollInterval = TimeSpan.FromMilliseconds(1000.0 / rate);
                TaskQueue.EnqueueTimer(this.pollInterval, this.pollPort);

                this.kinectEntity.WebCam.UpdateInterval = (int)this.pollInterval.TotalMilliseconds;
                this.kinectEntity.DepthCam.UpdateInterval = (int)this.pollInterval.TotalMilliseconds;

                this.state.FrameRate = rate;
                update.ResponsePort.Post(DefaultUpdateResponseType.Instance);
                SendNotification(this.kinectSubmgrPort, update);
            }
        }

        /// <summary>
        /// Http GET handler.
        /// </summary>
        /// <param name="get">The get request</param>
        [ServiceHandler]
        public void HttpGetHandler(Microsoft.Dss.Core.DsspHttp.HttpGet get)
        {
            var rsp = new HttpResponseType(HttpStatusCode.OK, this.state, this.xslttransform);
            get.ResponsePort.Post(rsp);
        }

        /// <summary>
        /// Implements the Kinect HttpQuery Operation
        /// </summary>
        /// <param name="query">The HttpQuery Operation</param>
        [ServiceHandler]
        public void KinectServiceHttpQueryHandler(HttpQuery query)
        {
            this.RedirectHttpRequest(Contract.Identifier, this.depthCamPartner, query.Body.Context, query.ResponsePort);
        }

        /// <summary>
        /// Implements the Kinect UpdateTilt Operation
        /// </summary>
        /// <param name="update">The UpdateTilt Operation</param>
        [ServiceHandler]
        public void KinectServiceUpdateTiltHandler(kinect.UpdateTilt update)
        {
            var tilt = update.Body.Tilt;

            // need constants
            if (tilt > (short)kinect.KinectReservedSampleValues.MaximumTiltAngle ||
                tilt < (short)kinect.KinectReservedSampleValues.MinimumTiltAngle)
            {
                update.ResponsePort.Post(
                    Fault.FromCodeSubcodeReason(
                        FaultCodes.Sender,
                        DsspFaultCodes.OperationFailed,
                        Resources.InvalidTiltAngle));
            }
            else
            {
                var rotation = this.kinectEntity.Rotation;
                this.kinectEntity.DepthCam.Rotation = new xna.Vector3((float)tilt, rotation.Y, rotation.Z);
                this.kinectEntity.WebCam.Rotation = new xna.Vector3((float)tilt, rotation.Y, rotation.Z);

                this.state.TiltDegrees = tilt;
                this.UpdateTiltAngle(tilt);
                update.ResponsePort.Post(DefaultUpdateResponseType.Instance);
                SendNotification(this.kinectSubmgrPort, update);
            }
        }

        /// <summary>
        /// Implements the Kinect QueryPixelMapping
        /// </summary>
        /// <param name="query">The QueryPixelMapping</param>
        [ServiceHandler]
        public void KinectServiceQueryPixelMappingHandler(kinect.DepthToColorImage query)
        {
            var response = new kinect.DepthToColorResponse { X = query.Body.X, Y = query.Body.Y };

            query.ResponsePort.Post(response);
        }

        /// <summary>
        /// Queries the service for the latest frame.
        /// </summary>
        /// <param name="query">The query.</param>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public void KinectServiceQueryRawFrameHandler(kinect.QueryRawFrame query)
        {
            // Defer this request until the next frame is 'ready'.
            this.pendingFrameRequests.Add(query);
        }

        /// <summary>
        /// Implements the Kinect HttpPost Operation
        /// </summary>
        /// <param name="post">The HttpPost Operation</param>
        [ServiceHandler]
        public void KinectServiceHttpPostHandler(Microsoft.Dss.Core.DsspHttp.HttpPost post)
        {
            var request = post.GetHeader<HttpPostRequestData>();

            if (request != null &&
                request.TranslatedOperation != null)
            {
                if (request.TranslatedOperation is kinect.UpdateTilt)
                {
                    var tilt = (kinect.UpdateTilt)request.TranslatedOperation;
                    this.KinectServiceUpdateTiltHandler(tilt);
                }
            }
            else
            {
                throw new InvalidOperationException();
            }

            post.ResponsePort.Post(new HttpResponseType(
                HttpStatusCode.OK,
                this.state,
                this.xslttransform));
        }

        /// <summary>
        /// Skeleton to depth image handler.
        /// </summary>
        /// <param name="request">The request.</param>
        [ServiceHandler]
        public void SkeletonToDepthImageHandler(kinect.SkeletonToDepthImage request)
        {
            throw new NotImplementedException("SkeletonToDepthImage");
        }

        /// <summary>
        /// Depth image to skeleton handler.
        /// </summary>
        /// <param name="request">The request.</param>
        [ServiceHandler]
        public void DepthImageToSkeletonHandler(kinect.DepthImageToSkeleton request)
        {
            throw new NotImplementedException("DepthImageToSkeleton");
        }

        /// <summary>
        /// Skeletal smoothing handler.
        /// </summary>
        /// <param name="request">The request.</param>
        [ServiceHandler]
        public void UpdateSkeletalSmoothingHandler(kinect.UpdateSkeletalSmoothing request)
        {
            throw new NotImplementedException("UpdateSkeletalSmoothing");
        }

        /// <summary>
        /// Initialize the simulated kinect camera
        /// </summary>
        protected override void Start()
        {
            SpawnIterator(this.Initialize);
        }

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        /// <returns>Standard ccr iterator</returns>
        private IEnumerator<ITask> Initialize()
        {
            var located = new PortSet<VisualEntity, Fault>();
            SpawnIterator(located, this.LocateCameraEntity);

            yield return located.Choice();

            var entity = (VisualEntity)located;
            if (entity == null)
            {
                LogError("Kinect entity not found");
                StartFailed();
                yield break;
            }

            this.kinectEntity = (KinectEntity)entity;
            if (this.state == null)
            {
                this.state = new kinect.KinectState
                    {                        
                        DepthImageFormat = DepthImageFormat.Resolution320x240Fps30,
                        FrameRate = MaxFramesPerSec,
                        IsDepthServiceUpdateEnabled = true,
                        IsWebCamServiceUpdateEnabled = true,
                        UseColor = true,
                        UseDepth = true,
                        UseSkeletalTracking = false,                        
                        ColorImageFormat = ColorImageFormat.RgbResolution640x480Fps30
                    };

                SaveState(this.state);
            }

            this.panTiltState = InitialPanTiltState();

            base.Start();

            MainPortInterleave.CombineWith(
                new Interleave(
                    new ExclusiveReceiverGroup(
                        Arbiter.ReceiveWithIterator(true, this.pollPort, this.DrainPendingRequests)),
                    new ConcurrentReceiverGroup()));

            this.kinectOps.Post(new kinect.SetFrameRate(new kinect.SetFrameRateRequest(this.state.FrameRate)));
        }

        /// <summary>
        /// Locates the camera entity.
        /// </summary>
        /// <param name="responsePort">The response port.</param>
        /// <returns>Standard ccr iterator</returns>
        private IEnumerator<ITask> LocateCameraEntity(PortSet<VisualEntity, Fault> responsePort)
        {
            // Subscribe for it in the simulation engine
            var simenginePort = simengine.SimulationEngine.GlobalInstancePort;
            var notificationPort = new simengine.SimulationEnginePort();
            var subscribeResponse = simenginePort.Subscribe(ServiceInfo.PartnerList, notificationPort);
            var subscribeFailure = (Fault)subscribeResponse;
            if (subscribeFailure != null)
            {
                responsePort.Post(subscribeFailure);
                yield break;
            }

            var insertPort = notificationPort.P6;
            yield return insertPort.Receive(inserted => responsePort.Post(inserted.Body));

            yield break;
        }

        /// <summary>
        /// Drains the pending requests.
        /// </summary>
        /// <param name="timestamp">The timestamp.</param>
        /// <returns>Standard ccr iterator</returns>
        private IEnumerator<ITask> DrainPendingRequests(DateTime timestamp)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                // No pending request - exit early
                if (this.pendingFrameRequests.Count == 0)
                {
                    yield break;
                }

                // Fetch the depth and video in parallel as required. Do once each for any pending requests.
                var taskCount = 0;
                var donePort = new Port<EmptyValue>();
                var rawFrames = new RawKinectFrames();
                
                if (this.pendingFrameRequests.Any(p => p.Body.IncludeDepth))
                {
                    Arbiter.ExecuteToCompletion(
                        TaskQueue, 
                        new IterativeTask<DepthImageFormat, RawKinectFrames>(
                            this.state.DepthImageFormat,
                            rawFrames, 
                            this.BuildRawDepthFrame), 
                        donePort);

                    ++taskCount;
                }

                if (this.pendingFrameRequests.Any(p => p.Body.IncludeVideo))
                {
                    Arbiter.ExecuteToCompletion(
                        TaskQueue, 
                        new IterativeTask<ColorImageFormat, RawKinectFrames>(
                            this.state.ColorImageFormat, 
                            rawFrames, 
                            this.BuildRawVideoFrame), 
                        donePort);

                    ++taskCount;
                }

                if (taskCount > 0)
                {
                    // Wait for the operations to complete
                    yield return Arbiter.MultipleItemReceive(false, donePort, taskCount, EmptyHandler);
                }

                // Stamp the returned frames
                var currentFrame = ++this.frameNumber;
                if (rawFrames.RawDepthFrameData != null)
                {
                    rawFrames.RawDepthFrameInfo.FrameNumber = currentFrame;
                    rawFrames.RawDepthFrameInfo.Timestamp = timestamp.Ticks;
                }

                if (rawFrames.RawColorFrameData != null)
                {
                    rawFrames.RawColorFrameInfo.FrameNumber = currentFrame;
                    rawFrames.RawColorFrameInfo.Timestamp = timestamp.Ticks;
                }
                
                // Return the requested frames to each pending request.
                foreach (var request in this.pendingFrameRequests)
                {
                    var response = new RawKinectFrames
                        {
                            RawDepthFrameInfo = request.Body.IncludeDepth ? rawFrames.RawDepthFrameInfo : null,
                            RawDepthFrameData = request.Body.IncludeDepth ? rawFrames.RawDepthFrameData : null,
                            RawColorFrameInfo = request.Body.IncludeVideo ? rawFrames.RawColorFrameInfo : null,
                            RawColorFrameData = request.Body.IncludeVideo ? rawFrames.RawColorFrameData : null, 
                        };
                    request.ResponsePort.Post(new kinect.GetRawFrameResponse { RawFrames = response });
                }
            }
            finally
            {
                this.pendingFrameRequests.Clear();

                var timeToNextFrame = this.pollInterval - stopwatch.Elapsed;
                if (timeToNextFrame <= TimeSpan.Zero)
                {
                    this.pollPort.Post(DateTime.Now);
                }
                else
                {
                    TaskQueue.EnqueueTimer(timeToNextFrame, this.pollPort);
                }
            }
        }

        /// <summary>
        /// Builds the video based data.
        /// </summary>
        /// <param name="imageFormat">Image format.</param>
        /// <param name="rawFrames">The raw frames.</param>
        /// <returns>
        /// Standard ccr iterator
        /// </returns>
        private IEnumerator<ITask> BuildRawVideoFrame(ColorImageFormat imageFormat, RawKinectFrames rawFrames)
        {
            var webCamResponse = this.webCamPartner.Get();
            yield return webCamResponse.Choice();

            var webCamSensorState = (WebCamSensorState)webCamResponse;
            if (webCamSensorState == null)
            {
                yield break;
            }

            var frameInfo = new KinectFrameInfo
                {
                    Timestamp = webCamSensorState.TimeStamp.Ticks,                    
                    Width = webCamSensorState.Width,
                    Height = webCamSensorState.Height,
                    BytesPerPixel = 4
                };

            var source = webCamSensorState.Data;

            // Move from 24rgb to 32rgb
            var target = new byte[(source.Length * 4) / 3];
            for (int i = 0, j = 0; i < source.Length && j < target.Length; i += 3, j += 4)
            {
                target[j] = source[i];
                target[j + 1] = source[i + 1];
                target[j + 2] = source[i + 2];
            }

            rawFrames.RawColorFrameData = target;

            rawFrames.RawColorFrameInfo = frameInfo;
        }

        /// <summary>
        /// Builds the raw depth frame.
        /// </summary>
        /// <param name="depthFormat">Depth image format.</param>
        /// <param name="rawFrames">The raw frames.</param>
        /// <returns>
        /// Standard ccr iterator
        /// </returns>
        private IEnumerator<ITask> BuildRawDepthFrame(DepthImageFormat depthFormat, RawKinectFrames rawFrames)
        {
            var depthCamResponse = this.depthCamPartner.Get();
            yield return depthCamResponse.Choice();

            var depthCamSensorState = (DepthCamSensorState)depthCamResponse;
            if (depthCamSensorState == null)
            {
                yield break;
            }

            var frameInfo = new KinectFrameInfo
                {
                    Timestamp = depthCamSensorState.TimeStamp.Ticks,
                    Width = depthCamSensorState.DepthImageSize.Width,
                    Height = depthCamSensorState.DepthImageSize.Height,
                    BytesPerPixel = 2 
                };

            var target = new short[depthCamSensorState.DepthImage.Length];
            
            // Raw depth data includes 3 bits for player information            
            for (int i = 0; i < target.Length; i++)
            {
                target[i] = (short)(depthCamSensorState.DepthImage[i] << 3);
            }

            rawFrames.RawDepthFrameData = target;
            
            rawFrames.RawDepthFrameInfo = frameInfo;
        }
        
        /// <summary>
        /// Forwards a subscription to a partner
        /// </summary>
        /// <param name="partner">The partner.</param>
        /// <param name="request">The request.</param>
        /// <param name="responsePort">The response port.</param>
        private void ForwardSubscription(
            IPort partner, 
            SubscribeRequestType request, 
            PortSet<SubscribeResponseType, Fault> responsePort)
        {
            var lookup = new DsspDefaultLookup();
            partner.PostUnknownType(lookup);
            this.Activate(
                lookup.ResponsePort.Choice(
                    svcinfo =>
                        {
                            var submgrInfo = svcinfo.PartnerList.Find(p => p.Contract == submgr.Contract.Identifier);
                            var subMgrOps = ServiceForwarder<submgr.SubscriptionManagerPort>(submgrInfo.Service);
                            var insert = new submgr.InsertSubscription(request) { ResponsePort = responsePort };
                            subMgrOps.Post(insert);
                        },
                    responsePort.Post));
        }

        /// <summary>
        /// Redirects the HTTP request from an alternate contract to a partner
        /// </summary>
        /// <param name="contract">The contract.</param>
        /// <param name="partner">The partner.</param>
        /// <param name="context">The context.</param>
        /// <param name="responsePort">The response port.</param>
        private void RedirectHttpRequest(
            string contract,
            IPort partner, 
            HttpListenerContext context,
            PortSet<HttpResponseType, Fault> responsePort)
        {
            var alternate = AlternateContractServiceInfo.Find(s => s.Contract == contract) ?? ServiceInfo;
            var basePath = alternate.HttpServiceAlias.AbsolutePath;
            var requestedPath = context.Request.Url.PathAndQuery;
            var pathSuffix = requestedPath.Substring(basePath.Length);

            var lookup = new DsspDefaultLookup();
            partner.PostUnknownType(lookup);
            this.Activate(
                lookup.ResponsePort.Choice(
                    svcinfo =>
                        {
                            var redirectPath = svcinfo.HttpServiceAlias.AbsolutePath + pathSuffix;
                            context.Response.Redirect(redirectPath);
                            context.Response.Close();
                            responsePort.Post(new HttpResponseType());
                        },
                    responsePort.Post));
        }
    }
}
