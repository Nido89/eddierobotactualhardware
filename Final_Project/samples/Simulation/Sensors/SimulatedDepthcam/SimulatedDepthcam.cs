//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: SimulatedDepthcam.cs $ $Revision: 24 $
//-----------------------------------------------------------------------

namespace Microsoft.Robotics.Services.Simulation.Sensors.DepthCamera
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing;
    using System.Threading;
    using Microsoft.Ccr.Core;
    using Microsoft.Dss.Core.Attributes;
    using Microsoft.Dss.Core.DsspHttp;
    using Microsoft.Dss.Core.DsspHttpUtilities;
    using Microsoft.Robotics.Services.DepthCamSensor;
    using depthcam = Microsoft.Robotics.Services.DepthCamSensor;
    using dssp = Microsoft.Dss.ServiceModel.Dssp;
    using physics = Microsoft.Robotics.Simulation.Physics;
    using simengine = Microsoft.Robotics.Simulation.Engine;
    using simtypes = Microsoft.Robotics.Simulation;
    using submgr = Microsoft.Dss.Services.SubscriptionManager;
    using svcbase = Microsoft.Dss.ServiceModel.DsspServiceBase;
    using webcam = Microsoft.Robotics.Services.WebCam.Proxy;
    using xnagrfx = Microsoft.Xna.Framework.Graphics;


    /// <summary>
    /// Provides access to a simulated Depth Camera contract
    /// using physics raycasting and the LaserRangeFinderEntity
    /// </summary>
    [DisplayName("(User) Simulated Depth Camera")]
    [Description("Provides access to a simulated depth camera.\n(Uses the generic Depth Camera contract.)")]
    [AlternateContract(depthcam.Contract.Identifier)]
    [Contract(Contract.Identifier)]
    public partial class SimulatedDepthCameraService : svcbase.DsspServiceBase
    {
        #region Simulation Variables
        simengine.DepthCameraEntity _entity;
        simengine.SimulationEnginePort _notificationTarget;
        physics.PhysicsEngine _physicsEngine;
        Port<simengine.DepthCameraEntity.DepthCameraResult> _raycastResults = 
            new Port<simengine.DepthCameraEntity.DepthCameraResult>();
        #endregion

        /// <summary>
        /// The DepthCam sensor state
        /// </summary>
        [ServiceState, InitialStatePartner(Optional = true, ServiceUri = "config/simulateddepthcamera.config.xml")]
        private depthcam.DepthCamSensorState _state = new depthcam.DepthCamSensorState();

        /// <summary>
        /// The main port
        /// </summary>
        [ServicePort("/SimulatedDepthCam", AllowMultipleInstances = true)]
        private depthcam.DepthCamSensorOperationsPort _mainPort = new depthcam.DepthCamSensorOperationsPort();

        /// <summary>
        /// The SubscriptionManager Partner
        /// </summary>
        [Partner("SubMgr", Contract = submgr.Contract.Identifier, CreationPolicy = PartnerCreationPolicy.CreateAlways)]
        private submgr.SubscriptionManagerPort _subMgrPort = new submgr.SubscriptionManagerPort();

        /// <summary>
        /// The WebCam partner
        /// </summary>
        [Partner(
            "WebCam", 
            Contract = webcam.Contract.Identifier, 
            CreationPolicy = PartnerCreationPolicy.UsePartnerListEntry, 
            Optional = true)]
        private webcam.WebCamOperations webcamPort = new webcam.WebCamOperations();

        /// <summary>
        /// Depth Camera transform used to render the depth cam on a web page.
        /// </summary>
        [EmbeddedResource("Microsoft.Robotics.Services.Simulation.Sensors.DepthCamera.SimulatedDepthcam.user.xslt")]
        private string xslttransform = null;

        /// <summary>
        /// Dssp Http Utilities port used as a parameter for the http helper methods.
        /// </summary>
        private DsspHttpUtilitiesPort utilitiesPort = new DsspHttpUtilitiesPort();

        /// <summary>
        /// SimulatedDepthCameraService constructor that takes a PortSet to notify when the service is created
        /// </summary>
        /// <param name="creationPort"></param>
        public SimulatedDepthCameraService(dssp.DsspServiceCreationPort creationPort) :
                base(creationPort)
        {
        }

        /// <summary>
        /// Start initializes SimulatedDepthcamService and listens for drop messages
        /// </summary>
        protected override void Start()
        {
            const double KinectMaxValidDepthMm = 4000.0;
            const double KinectMinValidDepthMm = 800.0;

            if (this._state == null)
            {
                // no config file found, initialize default state according to KinectReservedSampleValues
                this._state = new DepthCamSensorState();
                this._state.MaximumRange = KinectMaxValidDepthMm / 1000.0;
                this._state.MinimumRange = KinectMinValidDepthMm / 1000.0;
                this._state.FurtherThanMaxDepthValue = (short)4095.0;
                this._state.NearerThanMinDepthValue = (short)0.0;

                this.SaveState(this._state);
            }

            this._physicsEngine = physics.PhysicsEngine.GlobalInstance;
            _notificationTarget = new simengine.SimulationEnginePort();

            utilitiesPort = DsspHttpUtilitiesService.Create(Environment);

            // PartnerType.Service is the entity instance name.
            simengine.SimulationEngine.GlobalInstancePort.Subscribe(ServiceInfo.PartnerList, _notificationTarget);


            // dont start listening to DSSP operations, other than drop, until notification of entity
            Activate(new Interleave(
                new TeardownReceiverGroup
                (
                    Arbiter.Receive<simengine.InsertSimulationEntity>(false, _notificationTarget, InsertEntityNotificationHandlerFirstTime),
                    Arbiter.Receive<dssp.DsspDefaultDrop>(false, _mainPort, DefaultDropHandler)
                ),
                new ExclusiveReceiverGroup(),
                new ConcurrentReceiverGroup()
            ));

        }

        void DeleteEntityNotificationHandler(simengine.DeleteSimulationEntity del)
        {
            _entity = null;
        }

        void InsertEntityNotificationHandlerFirstTime(simengine.InsertSimulationEntity ins)
        {
            InsertEntityNotificationHandler(ins);
            base.Start();
            MainPortInterleave.CombineWith(
                new Interleave(
                    new TeardownReceiverGroup(),
                    new ExclusiveReceiverGroup(
                        Arbiter.Receive<simengine.InsertSimulationEntity>(true, _notificationTarget, InsertEntityNotificationHandler),
                        Arbiter.Receive<simengine.DeleteSimulationEntity>(true, _notificationTarget, DeleteEntityNotificationHandler)
                    ),
                    new ConcurrentReceiverGroup()
                )
            );
        }

        void InsertEntityNotificationHandler(simengine.InsertSimulationEntity ins)
        {
            _entity = (simengine.DepthCameraEntity)ins.Body;
            _entity.ServiceContract = Contract.Identifier;

            try
            {
                _entity.Register(_raycastResults);
            }
            catch (Exception ex)
            {
                LogError(ex);
            }

            if (_rayCastQueue == null)
            {
                _rayCastQueue = new DispatcherQueue(_entity.EntityState.Name + "depthNotifications",
                    TaskQueue.Dispatcher,
                    TaskExecutionPolicy.ConstrainQueueDepthDiscardTasks,
                    1);
                // attach handler to raycast results port
                _rayCastQueue.Enqueue(Arbiter.ReceiveWithIterator(false, _raycastResults, RaycastResultsHandler));
            }           
        }

        DispatcherQueue _rayCastQueue;
        double _lastRaycastUpdate;
        private IEnumerator<ITask> RaycastResultsHandler(simengine.DepthCameraEntity.DepthCameraResult result)
        {
            try
            {
                var now = Microsoft.Robotics.Common.Utilities.ElapsedSecondsSinceStart;
                if (now - _lastRaycastUpdate < ((double)_entity.UpdateInterval / 1000.0))
                {
                    // dont update more than 20 times a second
                    yield break;
                }
                _lastRaycastUpdate = now;

                if (result.Data == null)
                {
                    yield break;
                }

                var latestResults = new depthcam.DepthCamSensorState();
                latestResults.MaximumRange = this._state.MaximumRange;
                latestResults.MinimumRange = this._state.MinimumRange;
                latestResults.FurtherThanMaxDepthValue = this._state.FurtherThanMaxDepthValue;
                latestResults.NearerThanMinDepthValue = this._state.NearerThanMinDepthValue;
                var depthImage = new short[result.Data.Length];
                latestResults.DepthImage = depthImage;
                latestResults.DepthImageSize = new Size(result.Width, result.Height);

                if (_entity != null)
                {
                    latestResults.FieldOfView = (float)(_entity.FieldOfView * Math.PI / 180.0f);
                }

                short maxDepthMM = (short)(this._state.MaximumRange * 1000);
                short minDepthMM = (short)(this._state.MinimumRange * 1000);

                for (int i = 0; i < result.Data.Length; i++)
                {
                    var s = (short)(result.Data[i] & 0xFF);
                    var depth = (short)((s * (short)maxDepthMM) / byte.MaxValue);
                    // The camera's far plane is already set at MaxDepth so no pixels will be further than 
                    // that. To compensate for that, we relax the test from '>' to '>=' so that the  
                    // 'further-than' pixel value can be set. This enables similar behavior to Kinect where 
                    // too-near and too-far pixels are both set to zero. 
                    if (depth >= maxDepthMM)
                    {
                        // this if branch is redundant if the shader sets the depth limit but its defense in depth.
                        depthImage[i] = this._state.FurtherThanMaxDepthValue;
                    }
                    else if (depth < minDepthMM)
                    {
                        depthImage[i] = this._state.NearerThanMinDepthValue;
                    }
                    else
                    {
                        depthImage[i] = depth;
                    }                    
                }

                byte[] rgbImage = null;

                if (webcamPort != null)
                {
                    var stateOrFault = webcamPort.QueryFrame();
                    yield return stateOrFault.Choice(
                        response =>
                        {
                            rgbImage = response.Frame;
                        },
                        fault => LogError(fault));
                }

                if (rgbImage != null)
                {
                    latestResults.VisibleImage = rgbImage;
                }

                latestResults.Pose = _state.Pose;
                latestResults.TimeStamp = DateTime.Now;
                // send replace message to self
                var replace = new depthcam.Replace();
                // for perf reasons dont set response port, we are just talking to ourself anyway
                replace.ResponsePort = null;
                replace.Body = latestResults;
                _mainPort.Post(replace);
            }
            finally
            {
                _raycastResults.Clear();
                _rayCastQueue.Enqueue(Arbiter.ReceiveWithIterator(false, _raycastResults, RaycastResultsHandler));
            }
        }

        /// <summary>
        /// Get the SimulatedDepthcam state
        /// </summary>
        /// <param name="get"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public IEnumerator<ITask> GetHandler(depthcam.Get get)
        {
            get.ResponsePort.Post(_state);
            yield break;
        }

        /// <summary>
        /// Processes a replace message
        /// </summary>
        /// <param name="replace"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public IEnumerator<ITask> ReplaceHandler(depthcam.Replace replace)
        {
            _state = replace.Body;
            if (replace.ResponsePort != null)
            {
                replace.ResponsePort.Post(dssp.DefaultReplaceResponseType.Instance);
            }

            // issue notification
            _subMgrPort.Post(new submgr.Submit(_state, dssp.DsspActions.ReplaceRequest));
            yield break;
        }

        /// <summary>
        /// Subscribe to SimulatedDepthcam service
        /// </summary>
        /// <param name="subscribe"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public IEnumerator<ITask> SubscribeHandler(depthcam.Subscribe subscribe)
        {
            yield return Arbiter.Choice(
                SubscribeHelper(_subMgrPort, subscribe.Body, subscribe.ResponsePort),
                delegate(SuccessResult success)
                {
                    _subMgrPort.Post(new submgr.Submit(
                        subscribe.Body.Subscriber, dssp.DsspActions.ReplaceRequest, _state, null));
                },
                delegate(Exception ex) { LogError(ex); }
            );
        }

        /// <summary>
        /// Http Get handler 
        /// </summary>
        /// <param name="get">Http Get Request</param>
        [ServiceHandler]
        public void HttpGetHandler(HttpGet get)
        {
            DepthCamSensorHttpUtilities.HttpGetHelper(get, this._state, this.xslttransform);
        }

        /// <summary>
        /// Http Query Handler
        /// </summary>
        /// <param name="query">Htt Query Request</param>
        /// <returns>CCR Task Chunk</returns>
        [ServiceHandler]
        public IEnumerator<ITask> HttpQueryHandler(HttpQuery query)
        {
            return DepthCamSensorHttpUtilities.HttpQueryHelper(
                query, this._state, this.xslttransform, this.utilitiesPort, this._state.DepthImageSize.Width, this._state.DepthImageSize.Height);
        }
    }
}
