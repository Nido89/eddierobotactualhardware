//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: SimulatedLRF.cs $ $Revision: 31 $
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.Ccr.Core;
using Microsoft.Dss.Core;
using Microsoft.Dss.Core.Attributes;
using permissions = System.Security.Permissions;
using svcbase = Microsoft.Dss.ServiceModel.DsspServiceBase;
using dssp = Microsoft.Dss.ServiceModel.Dssp;
using sicklrf = Microsoft.Robotics.Services.Sensors.SickLRF.Proxy;
using submgr = Microsoft.Dss.Services.SubscriptionManager;
using simtypes = Microsoft.Robotics.Simulation;
using simengine = Microsoft.Robotics.Simulation.Engine;
using physics = Microsoft.Robotics.Simulation.Physics;
using Microsoft.Robotics.PhysicalModel;
using System.ComponentModel;
using Microsoft.Dss.Core.DsspHttp;
using System.Net;

namespace Microsoft.Robotics.Services.Simulation.Sensors.LaserRangeFinder
{
    /// <summary>
    /// Provides access to a simulated Laser Range Finder contract
    /// using physics raycasting and the LaserRangeFinderEntity
    /// </summary>
    [DisplayName("(User) Simulated Laser Range Finder")]
    [Description("Provides access to a simulated laser range finder.\n(Uses the Sick Laser Range Finder contract.)")]
    [AlternateContract(sicklrf.Contract.Identifier)]
    [Contract(Contract.Identifier)]
    [DssServiceDescription("http://msdn.microsoft.com/library/cc998493.aspx")]
    public class SimulatedLRFService : svcbase.DsspServiceBase
    {
        #region Simulation Variables
        simengine.LaserRangeFinderEntity _entity;
        simengine.SimulationEnginePort _notificationTarget;
        physics.PhysicsEngine _physicsEngine;
        Port<physics.RaycastResult> _raycastResults = new Port<physics.RaycastResult>();
        #endregion

        private sicklrf.State _state = new sicklrf.State();

        [ServicePort("/SimulatedLRF", AllowMultipleInstances=true)]
        private sicklrf.SickLRFOperations _mainPort = new sicklrf.SickLRFOperations();

        [Partner("SubMgr", Contract = submgr.Contract.Identifier, CreationPolicy = PartnerCreationPolicy.CreateAlways)]
        submgr.SubscriptionManagerPort _subMgrPort = new submgr.SubscriptionManagerPort();

        /// <summary>
        /// SimulatedLRFService constructor that takes a PortSet to notify when the service is created
        /// </summary>
        /// <param name="creationPort"></param>
        public SimulatedLRFService(dssp.DsspServiceCreationPort creationPort) :
                base(creationPort)
        {
        }

        /// <summary>
        /// Start initializes SimulatedLRFService and listens for drop messages
        /// </summary>
        protected override void Start()
        {
            _physicsEngine = physics.PhysicsEngine.GlobalInstance;
            _notificationTarget = new simengine.SimulationEnginePort();

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

        const float LASER_RANGE = 8f;
        void CreateDefaultState()
        {
            _state.Units = sicklrf.Units.Millimeters;
            _state.AngularRange = 180;
            _state.AngularResolution = 0.5f;
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
            _entity = (simengine.LaserRangeFinderEntity)ins.Body;
            _entity.ServiceContract = Contract.Identifier;

            CreateDefaultState();

            physics.RaycastProperties raycastProperties = new physics.RaycastProperties();
            raycastProperties.StartAngle = -_state.AngularRange / 2.0f;
            raycastProperties.EndAngle = _state.AngularRange / 2.0f;
            raycastProperties.AngleIncrement = (float)_state.AngularResolution;
            raycastProperties.Range = LASER_RANGE;
            raycastProperties.OriginPose = new Pose();

            _entity.RaycastProperties = raycastProperties;
            try
            {
                _entity.Register(_raycastResults);
            }
            catch (Exception ex)
            {
                LogError(ex);
            }

            // attach handler to raycast results port
            Activate(Arbiter.Receive(true, _raycastResults, RaycastResultsHandler));
        }

        private void RaycastResultsHandler(physics.RaycastResult result)
        {
            // we just receive ray cast information from physics. Currently we just use
            // the distance measurement for each impact point reported. However, our simulation
            // engine also provides you the material properties so you can decide here to simulate
            // scattering, reflections, noise etc.

            sicklrf.State latestResults = new sicklrf.State();
            latestResults.DistanceMeasurements = new int[result.SampleCount+1];
            int initValue = (int)(LASER_RANGE * 1000f);
            for (int i = 0; i < (result.SampleCount + 1); i++)
                latestResults.DistanceMeasurements[i] = initValue;

            foreach (physics.RaycastImpactPoint pt in result.ImpactPoints)
            {
                // the distance to the impact has been pre-calculted from the origin
                // and it's in the fourth element of the vector
                latestResults.DistanceMeasurements[pt.ReadingIndex] = (int)(pt.Position.W * 1000f);
            }

            latestResults.AngularRange = (int)Math.Abs(_entity.RaycastProperties.EndAngle - _entity.RaycastProperties.StartAngle);
            latestResults.AngularResolution = _entity.RaycastProperties.AngleIncrement;
            latestResults.Units = sicklrf.Units.Millimeters;
            latestResults.LinkState = "Measurement received";
            latestResults.TimeStamp = DateTime.Now;

            // send replace message to self
            sicklrf.Replace replace = new sicklrf.Replace();
            // for perf reasons dont set response port, we are just talking to ourself anyway
            replace.ResponsePort = null;
            replace.Body = latestResults;
            _mainPort.Post(replace);
        }

        /// <summary>
        /// Get the SimulatedLRF state
        /// </summary>
        /// <param name="get"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public IEnumerator<ITask> GetHandler(sicklrf.Get get)
        {
            get.ResponsePort.Post(_state);
            yield break;
        }

        /// <summary>
        /// Get the SimulatedLRF state
        /// </summary>
        /// <param name="get"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public IEnumerator<ITask> GetHandler(HttpGet get)
        {
            get.ResponsePort.Post(new HttpResponseType(HttpStatusCode.OK, _state));
            yield break;
        }

        /// <summary>
        /// Processes a replace message
        /// </summary>
        /// <param name="replace"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public IEnumerator<ITask> ReplaceHandler(sicklrf.Replace replace)
        {
            _state = replace.Body;
            if (replace.ResponsePort != null)
                replace.ResponsePort.Post(dssp.DefaultReplaceResponseType.Instance);

            // issue notification
            _subMgrPort.Post(new submgr.Submit(_state, dssp.DsspActions.ReplaceRequest));
            yield break;
        }

        /// <summary>
        /// Subscribe to SimulatedLRF service
        /// </summary>
        /// <param name="subscribe"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public IEnumerator<ITask> SubscribeHandler(sicklrf.Subscribe subscribe)
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
        /// Subscribe to SimulatedLRF service
        /// </summary>
        /// <param name="subscribe"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public IEnumerator<ITask> ReliableSubscribeHandler(sicklrf.ReliableSubscribe subscribe)
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
    }
}
