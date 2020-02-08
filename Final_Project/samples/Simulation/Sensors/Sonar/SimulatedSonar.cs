//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: SimulatedSonar.cs $ $Revision: 10 $
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.Ccr.Core;
using Microsoft.Dss.Core;
using Microsoft.Dss.Core.Attributes;
using permissions = System.Security.Permissions;
using svcbase = Microsoft.Dss.ServiceModel.DsspServiceBase;
using dssp = Microsoft.Dss.ServiceModel.Dssp;
using submgr = Microsoft.Dss.Services.SubscriptionManager;
using simtypes = Microsoft.Robotics.Simulation;
using simengine = Microsoft.Robotics.Simulation.Engine;
using physics = Microsoft.Robotics.Simulation.Physics;
using Microsoft.Robotics.PhysicalModel;
using System.ComponentModel;
using Microsoft.Dss.Core.DsspHttp;
using System.Net;
using sonar = Microsoft.Robotics.Services.Sonar;

namespace Microsoft.Robotics.Services.Simulation.Sensors.Sonar
{
    /// <summary>
    /// Models a sonar sensor using physics raycasting to determine impact points
    /// </summary>
    [DataContract]
    public class SonarEntity : Microsoft.Robotics.Simulation.Engine.LaserRangeFinderEntity
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public SonarEntity() { }

        /// <summary>
        /// Initialization constructor
        /// </summary>
        /// <param name="localPose"></param>
        public SonarEntity(Pose localPose) : base(localPose) { }
    }

    /// <summary>
    /// Provides access to a simulated Sonar contract
    /// using physics raycasting and the SonarEntity
    /// </summary>
    [DisplayName("(User) Simulated Sonar")]
    [Description("Provides access to a simulated sonar sensor.\n(Uses the Sonar contract.)")]
    [AlternateContract(sonar.Contract.Identifier)]
    [Contract(Contract.Identifier)]
    public class SimulatedSonarService : svcbase.DsspServiceBase
    {
        #region Simulation Variables
        simengine.SonarEntity _entity;
        simengine.SimulationEnginePort _notificationTarget;
        physics.PhysicsEngine _physicsEngine;
        Port<physics.RaycastResult> _raycastResults = new Port<physics.RaycastResult>();
        #endregion

        private sonar.SonarState _state = new sonar.SonarState();

        [ServicePort("/SimulatedSonar", AllowMultipleInstances=true)]
        private sonar.SonarOperations _mainPort = new sonar.SonarOperations();

        [Partner("SubMgr", Contract = submgr.Contract.Identifier, CreationPolicy = PartnerCreationPolicy.CreateAlways)]
        submgr.SubscriptionManagerPort _subMgrPort = new submgr.SubscriptionManagerPort();

        /// <summary>
        /// SimulatedSonarService constructor that takes a PortSet to notify when the service is created
        /// </summary>
        /// <param name="creationPort"></param>
        public SimulatedSonarService(dssp.DsspServiceCreationPort creationPort) :
                base(creationPort)
        {
        }

        /// <summary>
        /// Start initializes SimulatedSonarService and listens for drop messages
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

        const float SONAR_RANGE = 2.0f;
        void CreateDefaultState()
        {
            _state = new sonar.SonarState
            {
                AngularRange = 30,
                AngularResolution = 0.5f,
                DistanceMeasurement = SONAR_RANGE,
                DistanceMeasurements = null,
                HardwareIdentifier = 0,
                MaxDistance = SONAR_RANGE,
                Pose = new Pose(),
                TimeStamp = DateTime.Now
            };
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
            _entity = (simengine.SonarEntity)ins.Body;
            _entity.ServiceContract = Contract.Identifier;

            CreateDefaultState();

        }

        /// <summary>
        /// Get the SimulatedSonar state
        /// </summary>
        /// <param name="get"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public IEnumerator<ITask> GetHandler(sonar.Get get)
        {
            _state.DistanceMeasurement = _entity.LatestReading;
            get.ResponsePort.Post(_state);
            yield break;
        }

        /// <summary>
        /// Get the SimulatedSonar state
        /// </summary>
        /// <param name="get"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public IEnumerator<ITask> GetHandler(HttpGet get)
        {
            _state.DistanceMeasurement = _entity.LatestReading;
            get.ResponsePort.Post(new HttpResponseType(HttpStatusCode.OK, _state));
            yield break;
        }

        /// <summary>
        /// Processes a replace message
        /// </summary>
        /// <param name="replace"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public IEnumerator<ITask> ReplaceHandler(sonar.Replace replace)
        {
            _state = replace.Body;
            if (replace.ResponsePort != null)
                replace.ResponsePort.Post(dssp.DefaultReplaceResponseType.Instance);

            // issue notification
            _subMgrPort.Post(new submgr.Submit(_state, dssp.DsspActions.ReplaceRequest));
            yield break;
        }

        /// <summary>
        /// Subscribe to SimulatedSonar service
        /// </summary>
        /// <param name="subscribe"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public IEnumerator<ITask> SubscribeHandler(sonar.Subscribe subscribe)
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
        /// Subscribe to SimulatedSonar service
        /// </summary>
        /// <param name="subscribe"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public IEnumerator<ITask> ReliableSubscribeHandler(sonar.ReliableSubscribe subscribe)
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
