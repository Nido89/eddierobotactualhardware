//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: SimulatedIR.cs $ $Revision: 8 $
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
using Microsoft.Dss.ServiceModel.DsspServiceBase;
using Microsoft.Robotics.Services.AnalogSensor.Proxy;
using Microsoft.Dss.ServiceModel.Dssp;
using pxanalogsensor = Microsoft.Robotics.Services.AnalogSensor.Proxy;
using Microsoft.Robotics.Simulation.Physics;


namespace Microsoft.Robotics.Services.Simulation.Sensors.Infrared
{
    /// <summary>
    /// SimulatedIR Service
    /// Simulates an IR distance sensor.
    /// </summary>
    [DisplayName("(User) Simulated IR Distance Sensor")]
    [Description("Provides access to the simulated IR entity.")]
    [Contract(Contract.Identifier)]
    [AlternateContract(pxanalogsensor.Contract.Identifier)]
    public class SimulatedIRService : DsspServiceBase
    {
        #region Simulation Variables
        simengine.IREntity _entity;
        simengine.SimulationEnginePort _notificationTarget;
        #endregion
        /// <summary>
        /// _state
        /// </summary>
        [ServiceState]
        private pxanalogsensor.AnalogSensorState _state = new Microsoft.Robotics.Services.AnalogSensor.Proxy.AnalogSensorState();
        /// <summary>
        /// _main Port
        /// </summary>
        [ServicePort("/simulatedir", AllowMultipleInstances = true)]
        private pxanalogsensor.AnalogSensorOperations _mainPort = new Microsoft.Robotics.Services.AnalogSensor.Proxy.AnalogSensorOperations();

        [Partner("SubMgr", Contract = submgr.Contract.Identifier, CreationPolicy = PartnerCreationPolicy.CreateAlways)]
        private submgr.SubscriptionManagerPort _submgrPort = new submgr.SubscriptionManagerPort();

        /// <summary>
        /// Default Service Constructor
        /// </summary>
        public SimulatedIRService(DsspServiceCreationPort creationPort) :
            base(creationPort)
        {
        }
        /// <summary>
        /// Service Start
        /// </summary>
        protected override void Start()
        {
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

            // start notification method
            SpawnIterator<DateTime>(DateTime.Now, CheckForStateChange);
        }

        double _previousDistance = 0;

        /// <summary>
        /// Check the hardware value 5 times per second and send a notification if it has changed.
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns></returns>
        private IEnumerator<ITask> CheckForStateChange(DateTime timeout)
        {
            while (true)
            {
                if (_entity != null)
                {
                    if (_entity.LatestReading != _previousDistance)
                    {
                        // send notification of state change
                        UpdateState();
                        base.SendNotification<Replace>(_submgrPort, _state);
                    }
                    _previousDistance = _entity.LatestReading;
                }
                yield return Arbiter.Receive(false, TimeoutPort(200), delegate { });
            }
        }

        /// <summary>
        /// Called the first time the simulation engine tells us about our entity.  Activate
        /// the other handlers and insert ourselves into the service directory.
        /// </summary>
        /// <param name="ins"></param>
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
            _entity = (simengine.IREntity)ins.Body;
            _entity.ServiceContract = Contract.Identifier;

            CreateDefaultState();
        }

        private void CreateDefaultState()
        {
            _state.HardwareIdentifier = 0;
            _state.NormalizedMeasurement = 0;
            _state.Pose = new Microsoft.Robotics.PhysicalModel.Proxy.Pose();
            _state.RawMeasurement = 0;
            _state.RawMeasurementRange = _entity.MaximumRange;
        }

        void DeleteEntityNotificationHandler(simengine.DeleteSimulationEntity del)
        {
            _entity = null;
        }

        /// <summary>
        /// Get Handler
        /// </summary>
        /// <param name="get"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public virtual IEnumerator<ITask> GetHandler(pxanalogsensor.Get get)
        {
            UpdateState();
            get.ResponsePort.Post(_state);
            yield break;
        }

        void UpdateState()
        {
            if (_entity != null)
            {
                // update our state from the entity
                _state.RawMeasurement = _entity.LatestReading;
                _state.NormalizedMeasurement = _state.RawMeasurement / _state.RawMeasurementRange;
                _state.TimeStamp = DateTime.Now;
            }
        }

        /// <summary>
        /// Replace Handler
        /// </summary>
        /// <param name="replace"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public virtual IEnumerator<ITask> ReplaceHandler(pxanalogsensor.Replace replace)
        {
            _state = replace.Body;
            replace.ResponsePort.Post(DefaultReplaceResponseType.Instance);
            yield break;
        }
        /// <summary>
        /// ReliableSubscribe Handler
        /// </summary>
        /// <param name="subscribe"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public virtual IEnumerator<ITask> ReliableSubscribeHandler(pxanalogsensor.ReliableSubscribe subscribe)
        {
            yield break;
        }
        /// <summary>
        /// Subscribe Handler
        /// </summary>
        /// <param name="subscribe"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public virtual IEnumerator<ITask> SubscribeHandler(pxanalogsensor.Subscribe subscribe)
        {
            SubscribeHelper(_submgrPort, subscribe.Body, subscribe.ResponsePort);
            yield break;
        }
    }
}
