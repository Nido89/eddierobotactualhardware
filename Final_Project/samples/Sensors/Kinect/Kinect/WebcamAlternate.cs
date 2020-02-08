//------------------------------------------------------------------------------
//  <copyright file="WebcamAlternate.cs" company="Microsoft Corporation">
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
    using pm = Microsoft.Robotics.PhysicalModel.Proxy;
    using submgr = Microsoft.Dss.Services.SubscriptionManager;
    using webcam = Microsoft.Robotics.Services.WebCamSensor;

    /// <summary>
    /// Kinect camera service implementation
    /// </summary>   
    public partial class KinectService : DsspServiceBase
    {
        /// <summary>
        /// Web cam port name
        /// </summary>
        private const string WebCamPort = "webCamPort";

        /// <summary>
        /// Need this structure for atomic operations moving bytes
        /// </summary>
        private struct ThreeByteStruct 
        {
            /// <summary>
            /// Default ctor which we need to silence compiler (which complains about unused variables)
            /// </summary>
            /// <param name="init">Dummy init value</param>
            private ThreeByteStruct(byte init) 
            {
                this.a = this.b = this.c = init;
            }

            /// <summary>
            /// Placeholders for three bytes
            /// </summary>
            private byte a, b, c;
        }

        /// <summary>
        /// Web cam port
        /// </summary>
        [AlternateServicePort("/kinectwebcamsensor", AlternateContract = webcam.Contract.Identifier)]
        private webcam.WebCamSensorOperations webCamPort = new webcam.WebCamSensorOperations();

        /// <summary>
        /// Main webcam state
        /// </summary>
        private webcam.WebCamSensorState webCamState;

        /// <summary>
        /// XSLT for the wecam alternate
        /// </summary>
        [EmbeddedResource("Microsoft.Robotics.Services.Sensors.Kinect.WebCam.user.xslt")]
        private string webCamTransform = string.Empty;

        /// <summary>
        /// Helper classes for handling HTTP request
        /// </summary>
        private DsspHttpUtilitiesPort utilitiesPort;

        /// <summary>
        /// Initialize webcam state.
        /// </summary>
        private void DoInitializeWebcamAlternate()
        {
            this.cachedProcessedFrames.VisibleImage =
                new byte[this.kinectSensor.ColorStream.FrameWidth * this.kinectSensor.ColorStream.FrameHeight 
                            * 3];

            this.webCamState = new webcam.WebCamSensorState
            {
                DeviceName = "Kinect",
                Width = this.kinectSensor.ColorStream.FrameWidth,
                Height = this.kinectSensor.ColorStream.FrameHeight
            };

            this.utilitiesPort = DsspHttpUtilitiesService.Create(Environment);            
        }

        /// <summary>
        /// Sub mgr port
        /// </summary>
        [SubscriptionManagerPartner("WebCamSubMgr")]
        private submgr.SubscriptionManagerPort webCamSubMgr = new submgr.SubscriptionManagerPort();

        /// <summary>
        /// Get handler for web camera state
        /// </summary>
        /// <param name="get">Get operation</param>
        [ServiceHandler(PortFieldName = WebCamPort)]
        public void OnGet(webcam.Get get)
        {
            get.ResponsePort.Post(this.webCamState);
        }

        /// <summary>
        /// Replace handler for web camera state
        /// </summary>
        /// <param name="replace">Replace operation</param>
        [ServiceHandler(PortFieldName = WebCamPort)]
        public void OnReplace(webcam.Replace replace)
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
        /// <param name="subscribe">Subscribe operation</param>
        [ServiceHandler(PortFieldName = WebCamPort)]
        public void OnSubscribe(webcam.Subscribe subscribe)
        {
            SubscribeHelper(this.webCamSubMgr, subscribe, subscribe.ResponsePort);
        }

        #region Handle HTTP

        /// <summary>
        /// HttpGet handler for web camera
        /// </summary>
        /// <param name="get">HttpGet operation</param>        
        [ServiceHandler(PortFieldName = WebCamPort)]
        public void OnWebcamHttpGet(HttpGet get)
        {
            this.webCamPort.HttpGetHelper(get, this.webCamState, this.webCamTransform);
        }
      
        /// <summary>
        /// Web cam http query handler
        /// </summary>
        /// <param name="query">HttpQuery operation</param>
        /// <returns>CCR Iterator</returns>
        [ServiceHandler(PortFieldName = WebCamPort)]
        public IEnumerator<ITask> OnWebcamHttpQuery(HttpQuery query)
        {
            return this.webCamPort.HttpQueryHelper(
                query,
                this.webCamState,
                this.webCamTransform,
                this.utilitiesPort);
        }
        #endregion

        /// <summary>
        /// Update webcam alternate state
        /// </summary>
        /// <param name="imageData">Image Data</param>
        private void DoUpdateWebCamAlternate(byte[] imageData)
        {
            this.webCamState.TimeStamp = DateTime.UtcNow;
            this.webCamState.Width = this.kinectSensor.ColorStream.FrameWidth;
            this.webCamState.Height = this.kinectSensor.ColorStream.FrameHeight;

            this.webCamState.Stride = imageData.Length / this.kinectSensor.ColorStream.FrameHeight;
            this.webCamState.Data = imageData;

            SendNotification(this.webCamSubMgr, new webcam.Replace(this.webCamState));
        }

        /// <summary>
        /// Non YUV-raw coversion
        /// </summary>
        /// <param name="frameInfo">Frame info</param>
        /// <param name="rawColorData">Raw frame data</param>
        /// <param name="visibleImage">Processed visible image</param>
        private unsafe void DoProcessVisualImageForWebCamAlternateRgbYuv(KinectFrameInfo frameInfo, byte[] rawColorData, byte[] visibleImage)
        {
            int height = frameInfo.Height;
            int width = frameInfo.Width;

            // The following fixed statement pins the location of
            // the src and dst objects in memory so that they will
            // not be moved by garbage collection (perf)       
            fixed (byte* ptrSrc = rawColorData, ptrDest = visibleImage)
            {
                // we need to cut the 4th byte to conform to 24 bit color image format expected by 
                // cameras we also need to flip image along the vertical axis to make sure depth and 
                // rgb images have same orientation (raw buffers are oppositely oriented)

                for (int y = 0; y < height; ++y)
                {
                    // we pre-calc those for perf reasons (profiler-verified to have a significant effect)
                    int yTImesWidth = y * width;
                    int yTImesWidthPlusWidth = (y * width) + width;

                    for (int x = 0; x < width; ++x)
                    {
                        int i = (yTImesWidth + x) * 3;
                        int j = (yTImesWidthPlusWidth - x - 1) * 4;

                        *(ThreeByteStruct*)(ptrDest + i) = *(ThreeByteStruct*)(ptrSrc + j);
                    }
                }
            }
        }

        /// <summary>
        /// YUV-RAW conversion. YUV Raw has 2 bytes per pixel, not 4 like it is the case with 
        /// RGB and YUV
        /// </summary>        
        /// <param name="frameInfo">Frame info</param>
        /// <param name="rawColorData">Raw frame data</param>        
        /// <param name="resultingImage">Processed visible image</param>
        private unsafe void DoProcessVisualImageForWebCamAlternateYUVRaw(
            KinectFrameInfo frameInfo, 
            byte[] rawColorData, 
            byte[] resultingImage)
        {
            int height = frameInfo.Height;
            int width = frameInfo.Width;

            // The following fixed statement pins the location of
            // the src and dst objects in memory so that they will
            // not be moved by garbage collection (perf)       
            fixed (byte* ptrSrc = rawColorData, ptrDest = resultingImage)
            {
                for (int y = 0; y < height; ++y)
                {
                    // we pre-calc those for perf reasons (profiler-verified to have a significant effect)
                    int yTImesWidth = y * width;
                    int yTImesWidthPlusWidth = (y * width) + width;

                    for (int x = 0; x < width; ++x)
                    {
                        int i = (yTImesWidth + x) * 3;
                        int j = (yTImesWidthPlusWidth - x - 1) * 2;

                        *(short*)(ptrDest + i) = *(short*)(ptrSrc + j);
                    }
                }
            }
        } 
    }
}