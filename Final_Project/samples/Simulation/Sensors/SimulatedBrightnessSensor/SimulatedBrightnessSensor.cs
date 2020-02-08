//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: SimulatedBrightnessSensor.cs $ $Revision: 8 $
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

namespace Microsoft.Robotics.Services.Simulation.Sensors.BrightnessSensor
{

    /// <summary>
    /// SimPhotoCell Service
    /// Simulates a photocell brightness sensor.
    /// </summary>
    [DisplayName("(User) Simulated PhotoCell Brightness Sensor")]
    [Description("Provides a photocell interface to the Camera Entity")]
    [Contract(Contract.Identifier)]
    [AlternateContract("http://schemas.microsoft.com/robotics/2006/06/analogsensor.html")]
    public class SimulatedBrightnessCell : DsspServiceBase
    {
        #region Simulation Variables
        simengine.CameraEntity _entity;
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
        [ServicePort("/simPhotoCell", AllowMultipleInstances = true)]
        private pxanalogsensor.AnalogSensorOperations _mainPort = new Microsoft.Robotics.Services.AnalogSensor.Proxy.AnalogSensorOperations();

        [Partner("SubMgr", Contract = submgr.Contract.Identifier, CreationPolicy = PartnerCreationPolicy.CreateAlways)]
        private submgr.SubscriptionManagerPort _submgrPort = new submgr.SubscriptionManagerPort();

        /// <summary>
        /// Default Service Constructor
        /// </summary>
        public SimulatedBrightnessCell(DsspServiceCreationPort creationPort) :
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

        float _previousBrightness = 0;

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
                    // get the image from the CameraEntity
                    PortSet<int[], Exception> result = new PortSet<int[], Exception>();
                    _entity.CaptureScene(result);
                    double brightness = 0;
                    yield return Arbiter.Choice(result,
                        delegate(int[] data)
                        {
                            for (int i = 0; i < data.Length; i++)
			                {
                                int c = data[i];
                                int r = (0x00FF0000 & c) >> 16;
                                int g = (0x0000FF00 & c) >> 8;
                                int b = (0x000000FF & c);
                			    brightness += (r * 0.3) + (g * 0.59) + (b * 0.11);
			                }
                            // calculate the average brightness, scale it to (0-1) range
                            brightness = brightness / (data.Length * 255.0);
                        },
                        delegate(Exception ex)
                        {
                        }
                    );
                    if (Math.Abs(brightness - _previousBrightness) > 0.005)
                    {
                        // send notification of state change
                        _state.RawMeasurement = brightness;
                        _state.NormalizedMeasurement = _state.RawMeasurement / _state.RawMeasurementRange;
                        _state.TimeStamp = DateTime.Now;
                        base.SendNotification<Replace>(_submgrPort, _state);
                        _previousBrightness = (float)brightness;
                    }
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
            _entity = (simengine.CameraEntity)ins.Body;
            _entity.ServiceContract = Contract.Identifier;

            CreateDefaultState();
        }

        private void CreateDefaultState()
        {
            _state.HardwareIdentifier = 0;
            _state.NormalizedMeasurement = 0;
            _state.Pose = new Microsoft.Robotics.PhysicalModel.Proxy.Pose();
            _state.RawMeasurement = 0;
            _state.RawMeasurementRange = 1.0f;
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
