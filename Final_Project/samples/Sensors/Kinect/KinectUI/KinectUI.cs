//------------------------------------------------------------------------------
//  <copyright file="KinectUI.cs" company="Microsoft Corporation">
//      Copyright (C) Microsoft Corporation.  All rights reserved.
//  </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Dss.Services.Samples.KinectUI
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Threading;
    using System.Windows;
    using Microsoft.Ccr.Core;
    using Microsoft.Dss.Core.Attributes;
    using Microsoft.Dss.ServiceModel.Dssp;
    using Microsoft.Dss.ServiceModel.DsspServiceBase;
    using W3C.Soap;

    using ccrwpf = Microsoft.Ccr.Adapters.Wpf;
    using common = Microsoft.Robotics.Common;
    using kinect = Microsoft.Robotics.Services.Sensors.Kinect;
    using kinectProxy = Microsoft.Robotics.Services.Sensors.Kinect.Proxy;
    using mskinect = Microsoft.Kinect;

    /// <summary>
    /// KinectUIService - Example of using Kinect service
    /// </summary>
    [Contract(Contract.Identifier)]
    [DisplayName("(User) WPF UI for Kinect")]
    [Description("Sample WPF user interface for the Kinect service")]
    internal class KinectUIService : DsspServiceBase
    {     
        /// <summary>
        /// UI will use this port to post operations to
        /// </summary>
        [ServicePort("/KinectUI", AllowMultipleInstances = false)]
        private KinectUIOperations mainPort = new KinectUIOperations();

        /// <summary>
        /// Used to guage frequency of reading the state (which is much lower than that of reading frames)
        /// </summary>
        private double lastStateReadTime = 0;

        /// <summary>
        /// We don't want to flood logs with same errors
        /// </summary>
        private bool frameQueryFailed = false;

        /// <summary>
        /// We don't want to flood logs with same errors
        /// </summary>
        private bool tiltPollFailed = false;

        /// <summary>
        /// Gets or sets a value indicating whether to render depth information
        /// </summary>
        public bool IncludeDepth { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to render video information
        /// </summary>
        public bool IncludeVideo { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to render skeleton information
        /// </summary>
        public bool IncludeSkeletons { get; set; }
                
        /// <summary>
        /// Kinect partner service
        /// </summary>
        [Partner("Kinect", Contract = kinectProxy.Contract.Identifier, CreationPolicy = PartnerCreationPolicy.UseExistingOrCreate)]
        private kinectProxy.KinectOperations kinectPort = new kinectProxy.KinectOperations();

        /// <summary>
        /// Frame Pre Processor
        /// </summary>
        private FramePreProcessor frameProcessor;

        /// <summary>
        /// WPF service port
        /// </summary>
        private ccrwpf.WpfServicePort wpfServicePort;

        /// <summary>
        /// Main UI window to do "Invokes" on when data is ready for visualization
        /// </summary>
        private KinectUI userInterface;

        /// <summary>
        /// By default we will query all 3 frame types
        /// </summary>
        /// <param name="creationPort">Creation port</param>
        public KinectUIService(DsspServiceCreationPort creationPort)
            : base(creationPort)
        {
            // by default, lets show all the good stuff
            this.IncludeDepth = true;
            this.IncludeVideo = true;
            this.IncludeSkeletons = true;
        }

        /// <summary>
        /// Service start
        /// </summary>
        protected override void Start()
        {
            SpawnIterator(this.Initialize);
        }

        /// <summary>
        /// Initialize Kinect UI serice
        /// </summary>
        /// <returns>CCR Iterator</returns>
        private IEnumerator<ITask> Initialize()
        {
            this.frameProcessor = new FramePreProcessor(this.kinectPort);

            // create WPF adapter
            this.wpfServicePort = ccrwpf.WpfAdapter.Create(TaskQueue);

            var runWindow = this.wpfServicePort.RunWindow(() => new KinectUI(this));
            yield return (Choice)runWindow;

            var exception = (Exception)runWindow;
            if (exception != null)
            {
                LogError(exception);
                StartFailed();
                yield break;
            }

            // need double cast because WPF adapter doesn't know about derived window types
            this.userInterface = (Window)runWindow as KinectUI;

            yield return this.kinectPort.Get().Choice(
                kinectState =>
                {
                    this.UpdateState(kinectState);
                },
                failure =>
                {
                    LogError(failure);                    
                });

            yield return this.kinectPort.GetDepthProperties().Choice(
                GetDepthProperties =>
                {
                    KinectUI.MaxValidDepth = GetDepthProperties.MaxDepthValue;
                },
                failure =>
                {
                    LogError(failure);                  
                });            
            
            base.Start();

            SpawnIterator(this.ReadKinectLoop);
        }

        /// <summary>
        /// Main read loop
        /// Read raw frame from Kinect service, then process it asynchronously, then request UI update
        /// </summary>
        /// <returns>A standard CCR iterator.</returns>
        private IEnumerator<ITask> ReadKinectLoop()
        {
            while (true)
            {
                kinectProxy.QueryRawFrameRequest frameRequest = new kinectProxy.QueryRawFrameRequest();
                frameRequest.IncludeDepth = this.IncludeDepth;
                frameRequest.IncludeVideo = this.IncludeVideo;
                frameRequest.IncludeSkeletons = this.IncludeSkeletons;

                if (!this.IncludeDepth && !this.IncludeVideo && !this.IncludeSkeletons)
                {
                    // poll 5 times a sec if user for some reason deselected all image options (this would turn
                    // into a busy loop then)
                    yield return TimeoutPort(200).Receive();
                }

                kinect.RawKinectFrames rawFrames = null;

                // poll depth camera
                yield return this.kinectPort.QueryRawFrame(frameRequest).Choice(
                    rawFrameResponse =>
                    {
                        rawFrames = rawFrameResponse.RawFrames;
                    },
                    failure =>
                    {
                        if (!this.frameQueryFailed)
                        {
                            this.frameQueryFailed = true;
                            LogError(failure);
                        }
                    });

                this.frameProcessor.SetRawFrame(rawFrames);

                if (null != rawFrames.RawSkeletonFrameData)
                {
                    yield return new IterativeTask(this.frameProcessor.ProcessSkeletons);
                }

                this.UpdateUI(this.frameProcessor);

                // poll state at low frequency to see if tilt has shifted (may happen on an actual robot due to shaking)
                if (common.Utilities.ElapsedSecondsSinceStart - this.lastStateReadTime > 1)
                {
                    yield return this.kinectPort.Get().Choice(
                        kinectState =>
                        {
                            this.UpdateState(kinectState);
                        },
                        failure =>
                        {
                            if (!this.tiltPollFailed)
                            {
                                this.tiltPollFailed = true;
                                LogError(failure);
                            }
                        });

                    this.lastStateReadTime = common.Utilities.ElapsedSecondsSinceStart;                    
                }
            }
        }

        /// <summary>
        /// Update the UI for each frame
        /// </summary>
        /// <param name="processedFrames">Processed Frames</param>
        private void UpdateUI(FramePreProcessor processedFrames)
        {
            this.wpfServicePort.Invoke(() => this.userInterface.DrawFrame(processedFrames));
        }

        /// <summary>
        /// Update the kinect state on the UI
        /// </summary>
        /// <param name="kinectState">Kinect State</param>
        private void UpdateState(kinectProxy.KinectState kinectState)
        {
            this.wpfServicePort.Invoke(() => this.userInterface.UpdateState(kinectState));
        }

        /// <summary>
        /// Update the tilt angle on the kinect
        /// </summary>
        /// <param name="degrees">Angle in degrees</param>
        internal void UpdateTilt(int degrees)
        {
            kinectProxy.UpdateTiltRequest request = new kinectProxy.UpdateTiltRequest();
            request.Tilt = degrees;

            Activate(
                Arbiter.Choice(
                    this.kinectPort.UpdateTilt(request),
                    success =>
                    {
                        // nothing to do
                    },
                    fault =>
                    {
                        // the fault handler is outside the WPF dispatcher
                        // to perfom any UI related operation we need to go through the WPF adapter

                        // show an error message
                        this.wpfServicePort.Invoke(() => this.userInterface.ShowFault(fault));
                    }));
        }

        /// <summary>
        /// Send a request to Kinect service to update smoothing parameters
        /// </summary>
        /// <param name="transformSmooth">A value indicating whether to apply transform smooth</param>
        /// <param name="smoothing">The amount of smoothing to be applied</param>
        /// <param name="correction">The amount of correction to be applied</param>
        /// <param name="prediction">The amount of prediction to be made</param>
        /// <param name="jitterRadius">The radius for jitter processing</param>
        /// <param name="maxDeviationRadius">Maximum deviation radius</param>
        internal void UpdateSkeletalSmoothing(
            bool transformSmooth,
            float smoothing,
            float correction,
            float prediction,
            float jitterRadius,
            float maxDeviationRadius)
        {
            mskinect.TransformSmoothParameters newSmoothParams = new mskinect.TransformSmoothParameters();
            newSmoothParams.Correction = correction;
            newSmoothParams.JitterRadius = jitterRadius;
            newSmoothParams.MaxDeviationRadius = maxDeviationRadius;
            newSmoothParams.Prediction = prediction;
            newSmoothParams.Smoothing = smoothing;

            kinectProxy.UpdateSkeletalSmoothingRequest request = new kinectProxy.UpdateSkeletalSmoothingRequest();
            request.TransfrormSmooth = transformSmooth;
            request.SkeletalEngineTransformSmoothParameters = newSmoothParams;

            Activate(
                Arbiter.Choice(
                    this.kinectPort.UpdateSkeletalSmoothing(request),
                    success =>
                    {
                        // nothing to do
                    },
                    fault =>
                    {
                        // the fault handler is outside the WPF dispatcher
                        // to perfom any UI related operation we need to go through the WPF adapter

                        // show an error message
                        this.wpfServicePort.Invoke(() => this.userInterface.ShowFault(fault));
                    }));
        }
    }
}