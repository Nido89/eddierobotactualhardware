//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: SimulatedCompass.cs $ $Revision: 8 $
//-----------------------------------------------------------------------
using Microsoft.Ccr.Core;
using Microsoft.Dss.Core;
using Microsoft.Dss.Core.Attributes;
using Microsoft.Dss.ServiceModel.Dssp;
using Microsoft.Dss.ServiceModel.DsspServiceBase;
using System;
using System.ComponentModel;
using System.Collections.Generic;
using Microsoft.Robotics.PhysicalModel;
using System.Drawing;

using pxanalogsensor = Microsoft.Robotics.Services.AnalogSensor.Proxy;
using xml = System.Xml;
using simengine = Microsoft.Robotics.Simulation.Engine;
using dssp = Microsoft.Dss.ServiceModel.Dssp;
using submgr = Microsoft.Dss.Services.SubscriptionManager;
using Microsoft.Robotics.Services.AnalogSensor.Proxy;

namespace Microsoft.Robotics.Services.Simulation.Sensors.Compass
{

    /// <summary>
    /// SimPhotoCell Service
    /// Simulates a compass sensor.
    /// </summary>
    [DisplayName("(User) Simulated Compass Sensor")]
    [Description("Provides a service to determine an entity's orientation with respect to North (+Z axis)")]
    [Contract(Contract.Identifier)]
    [AlternateContract("http://schemas.microsoft.com/robotics/2006/06/analogsensor.html")]
    public class SimulatedCompass : DsspServiceBase
    {
        #region Simulation Variables
        simengine.VisualEntity _entity;
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
        [ServicePort("/SimulatedCompass", AllowMultipleInstances = true)]
        private pxanalogsensor.AnalogSensorOperations _mainPort = new Microsoft.Robotics.Services.AnalogSensor.Proxy.AnalogSensorOperations();

        [Partner("SubMgr", Contract = submgr.Contract.Identifier, CreationPolicy = PartnerCreationPolicy.CreateAlways)]
        private submgr.SubscriptionManagerPort _submgrPort = new submgr.SubscriptionManagerPort();

        /// <summary>
        /// Default Service Constructor
        /// </summary>
        public SimulatedCompass(DsspServiceCreationPort creationPort) :
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
                    double rotationFromPlusZDegrees = _entity.RotationAnglesInWorldSpace.Y;
                    double rotationFromPlusZ = Math.PI * rotationFromPlusZDegrees / 180.0;

                    var rotationFromMinusZ = -rotationFromPlusZ; 

                    _state.RawMeasurement = rotationFromMinusZ; //[-PI, PI]
                    _state.NormalizedMeasurement = (rotationFromMinusZ + Math.PI) / (2 * Math.PI); // [0,1]
                    _state.TimeStamp = DateTime.Now;
                    base.SendNotification<Replace>(_submgrPort, _state);
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
            _entity = (simengine.VisualEntity)ins.Body;
            _entity.ServiceContract = Contract.Identifier;

            CreateDefaultState();
        }

        private void CreateDefaultState()
        {
            _state.HardwareIdentifier = 0;
            _state.NormalizedMeasurement = 0;
            _state.Pose = new Microsoft.Robotics.PhysicalModel.Proxy.Pose();
            _state.RawMeasurement = 0;
            _state.RawMeasurementRange = 2 * Math.PI;
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
            // the state is updated automatically
            get.ResponsePort.Post(_state);
            yield break;
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
