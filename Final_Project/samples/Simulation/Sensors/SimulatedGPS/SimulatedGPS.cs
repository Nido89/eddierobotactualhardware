//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: SimulatedGPS.cs $ $Revision: 6 $
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

using xml = System.Xml;
using simengine = Microsoft.Robotics.Simulation.Engine;
using dssp = Microsoft.Dss.ServiceModel.Dssp;
using submgr = Microsoft.Dss.Services.SubscriptionManager;

namespace Microsoft.Robotics.Services.Simulation.Sensors.GPSSensor
{

    /// <summary>
    /// SimPhotoCell Service
    /// Simulates a photocell brightness sensor.
    /// </summary>
    [DisplayName("(User) Simulated GPS Sensor")]
    [Description("Provides a simulated GPS sensor")]
    [Contract(Contract.Identifier)]
    public class SimulatedGPSSensor : DsspServiceBase
    {
        #region Simulation Variables
        simengine.VisualEntity _entity;
        simengine.SimulationEnginePort _notificationTarget;
        #endregion
        /// <summary>
        /// _state
        /// </summary>
        [ServiceState]
        private GPSSensorState _state = new GPSSensorState();
        /// <summary>
        /// _main Port
        /// </summary>
        [ServicePort("/simPhotoCell", AllowMultipleInstances = true)]
        private GPSSensorOperations _mainPort = new GPSSensorOperations();

        [Partner("SubMgr", Contract = submgr.Contract.Identifier, CreationPolicy = PartnerCreationPolicy.CreateAlways)]
        private submgr.SubscriptionManagerPort _submgrPort = new submgr.SubscriptionManagerPort();

        /// <summary>
        /// Default Service Constructor
        /// </summary>
        public SimulatedGPSSensor(DsspServiceCreationPort creationPort) :
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
                    try
                    {
                        // get the image from the CameraEntity
                        PortSet<int[], Exception> result = new PortSet<int[], Exception>();

                        _state.X = _entity.State.Pose.Position.X;
                        _state.Y = _entity.State.Pose.Position.Y;
                        _state.Z = _entity.State.Pose.Position.Z;
                        _state.TimeStamp = DateTime.Now;
                        base.SendNotification<Replace>(_submgrPort, _state);
                    }
                    catch
                    {
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
            _state.X = 0;
            _state.Y = 0;
            _state.Z = 0;
            _state.TimeStamp = DateTime.Now;
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
        public virtual IEnumerator<ITask> GetHandler(Get get)
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
        public virtual IEnumerator<ITask> ReplaceHandler(Replace replace)
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
        public virtual IEnumerator<ITask> ReliableSubscribeHandler(ReliableSubscribe subscribe)
        {
            yield break;
        }
        /// <summary>
        /// Subscribe Handler
        /// </summary>
        /// <param name="subscribe"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public virtual IEnumerator<ITask> SubscribeHandler(Subscribe subscribe)
        {
            SubscribeHelper(_submgrPort, subscribe.Body, subscribe.ResponsePort);
            yield break;
        }
    }
}
