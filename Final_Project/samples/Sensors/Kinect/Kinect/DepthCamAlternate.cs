//------------------------------------------------------------------------------
//  <copyright file="DepthCamAlternate.cs" company="Microsoft Corporation">
//      Copyright (C) Microsoft Corporation.  All rights reserved.
//  </copyright>
//------------------------------------------------------------------------------
namespace Microsoft.Robotics.Services.Sensors.Kinect
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Net;
    using System.Net.Mime;
    using System.Runtime.InteropServices;

    using Microsoft.Ccr.Core;
    using Microsoft.Dss.Core.Attributes;
    using Microsoft.Dss.Core.DsspHttp;
    using Microsoft.Dss.Core.DsspHttpUtilities;
    using Microsoft.Dss.ServiceModel.Dssp;
    using Microsoft.Dss.ServiceModel.DsspServiceBase;
    using Microsoft.Kinect;

    using W3C.Soap;

    using depth = Microsoft.Robotics.Services.DepthCamSensor;
    using pm = Microsoft.Robotics.PhysicalModel;
    using submgr = Microsoft.Dss.Services.SubscriptionManager;

    /// <summary>
    /// Kinect camera service implementation
    /// </summary>    
    public partial class KinectService : DsspServiceBase
    {
        /// <summary>
        /// Depth cam port name
        /// </summary>
        private const string DepthCamPort = "depthCamPort";

        /// <summary>
        /// Depth cam port
        /// </summary>
        [AlternateServicePort("/kinectdepthcamsensor", AlternateContract = depth.Contract.Identifier)]
        private depth.DepthCamSensorOperationsPort depthCamPort = new depth.DepthCamSensorOperationsPort();

        /// <summary>
        /// Main state of the depth cam
        /// </summary>
        private depth.DepthCamSensorState depthCamState;
        
        /// <summary>
        /// Sub mgr port
        /// </summary>
        [SubscriptionManagerPartner("DepthSubMgr")]
        private submgr.SubscriptionManagerPort depthSubMgr = new submgr.SubscriptionManagerPort();

        /// <summary>
        /// Initialize depthcam state.
        /// </summary>        
        private void DoInitializeDepthCamAlternate()
        {
            this.cachedProcessedFrames.DepthImage =
                new short[this.kinectSensor.DepthStream.FrameWidth * this.kinectSensor.DepthStream.FrameHeight];

            this.utilitiesPort = DsspHttpUtilitiesService.Create(Environment);

            this.depthCamState = new depth.DepthCamSensorState
            {                
                FieldOfView = KinectCameraConstants.HorizontalFieldOfViewRadians,
                
                ImageMode = depth.DepthCamSensorImageMode.Rgb,
                
                DepthImageSize =
                    new Size(
                        this.kinectSensor.DepthStream.FrameWidth,
                        this.kinectSensor.DepthStream.FrameHeight),

                MaximumRange = KinectCameraConstants.MaximumRangeMeters,

                TimeStamp = DateTime.MinValue,

                Pose = new Microsoft.Robotics.PhysicalModel.Pose(
                    new Microsoft.Robotics.PhysicalModel.Vector3(0, 0, 0),
                    new Microsoft.Robotics.PhysicalModel.Quaternion()),

                DepthImage =
                    new short[this.kinectSensor.DepthStream.FrameWidth * this.kinectSensor.DepthStream.FrameHeight]
            };            
        }

        /// <summary>
        /// Get handler for depthcam state
        /// </summary>
        /// <param name="get">Self explanatory</param>
        [ServiceHandler(PortFieldName = DepthCamPort)]
        public void OnGet(depth.Get get)
        {
            get.ResponsePort.Post(this.depthCamState);
        }

        /// <summary>
        /// Replace handler for depthcam state
        /// </summary>
        /// <param name="replace">Self explanatory</param>        
        [ServiceHandler(PortFieldName = DepthCamPort)]
        public void OnReplace(depth.Replace replace)
        {
            // Can't actually send a replace to this service, but it's here
            // for symmetry with replace events that are raised as the device
            // signals changes to it.
            replace.ResponsePort.Post(
                Fault.FromCodeSubcode(
                    FaultCodes.Receiver,
                    DsspFaultCodes.MessageNotSupported));
        }

        /// <summary>
        /// Subscribe handler
        /// </summary>
        /// <param name="subscribe">Self explanatory</param>
        [ServiceHandler(PortFieldName = DepthCamPort)]
        public void OnSubscribe(depth.Subscribe subscribe)
        {
            SubscribeHelper(this.depthSubMgr, subscribe, subscribe.ResponsePort);
        }

        #region Handle HTTP

        /// <summary>
        /// HttpGet handler for depth camera
        /// </summary>
        /// <param name="get">Self explanatory</param>        
        [ServiceHandler(PortFieldName = DepthCamPort)]
        public void OnDepthcamHttpGet(HttpGet get)
        {
            depth.DepthCamSensorHttpUtilities.HttpGetHelper(get, this.depthCamState, this.transform);
        }

        /// <summary>
        /// Handles http query request.
        /// </summary>
        /// <param name="query">The http query</param>
        /// <returns>CCR Task Chunk</returns>
        [ServiceHandler(PortFieldName = DepthCamPort)]
        public IEnumerator<ITask> OnDepthcamHttpQuery(HttpQuery query)
        {
            int videoWidth  = 0;
            int videoHeight = 0;

            if (null != this.kinectSensor.ColorStream) 
            {
                videoWidth = this.kinectSensor.ColorStream.FrameWidth;
                videoHeight = this.kinectSensor.ColorStream.FrameHeight;
            }

            return depth.DepthCamSensorHttpUtilities.HttpQueryHelper(
                query,
                this.depthCamState,
                this.transform,
                this.utilitiesPort,
                videoWidth,
                videoHeight);
        }

        #endregion

        /// <summary>
        /// Populates depth camera with depth data from Kinect. Also adds an optional 
        /// visible image of the same dimensions that may be used for 
        /// debugging. 
        /// </summary>
        /// <param name="webCamData">Processsed webcam data</param>
        /// <param name="depthData">Processed Depth data</param>
        private void DoUpdateDepthCamAlternate(byte[] webCamData, short[] depthData)
        {
            this.depthCamState.TimeStamp = DateTime.UtcNow;
            this.depthCamState.DepthImageSize =
                new Size(this.kinectSensor.DepthStream.FrameWidth, this.kinectSensor.DepthStream.FrameHeight);
            this.depthCamState.DepthImage = depthData;
            this.depthCamState.VisibleImage = webCamData;
            this.depthCamState.ImageMode = depth.DepthCamSensorImageMode.Rgb;

            SendNotification(this.depthSubMgr, new depth.Replace { Body = this.depthCamState });
        }
                
        /// <summary>
        /// Converts depth + player data returned by Kinect sensor to depth 'image' - a matrix of 
        /// shorts with distances in mm. The depth frame returned from the Kinect is mirrored.
        /// We not only have to do bitshifts, but also flip the image.
        /// </summary>        
        /// <param name="frameInfo">Frame info</param>
        /// <param name="rawDepthData">Raw frame data</param>        
        /// <param name="depthImage">Procesed frame</param>
        private void DoProcessDepthAndPlayerIndexDataForDepthCamAlternate( 
            KinectFrameInfo frameInfo, 
            short[] rawDepthData,
            short[] depthImage)
        {
            int height = frameInfo.Height;
            int width = frameInfo.Width;

            for (int y = 0; y < height; ++y)
            {
                // we pre-calc those for perf reasons (profiler-verified to have a significant effect)
                int yTImesWidth = y * width;
                int yTImesWidthPlusWidth = (y * width) + width;

                for (int x = 0; x < width; ++x)
                {
                    int i = yTImesWidth + x;
                    int j = yTImesWidthPlusWidth - x - 1;
                                        
                    depthImage[i] = (short)(rawDepthData[j] >> 3);
                }
            }            
        }         
    }
}
