//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: SimulatedLBR3Arm.cs $ $Revision: 31 $
//-----------------------------------------------------------------------

using Microsoft.Ccr.Core;
using Microsoft.Dss.Core;
using Microsoft.Dss.Core.Attributes;
using Microsoft.Dss.ServiceModel.Dssp;
using Microsoft.Dss.ServiceModel.DsspServiceBase;
using System;
using System.Collections.Generic;
using System.Security.Permissions;
using xml = System.Xml;
using submgr = Microsoft.Dss.Services.SubscriptionManager;
using arm = Microsoft.Robotics.Services.ArticulatedArm;
using simengine = Microsoft.Robotics.Simulation.Engine;
using Microsoft.Robotics.Simulation.Physics;
using Microsoft.Robotics.PhysicalModel;
using System.ComponentModel;


namespace Microsoft.Robotics.Services.Simulation.LBR3Arm
{
    /// <summary>
    /// Simulated KUKA LBR3 Robotic Arm Service
    /// </summary>
    [DisplayName("(User) Simulated KUKA LBR3 Robotic Arm")]
    [Description("Provides access for controlling the joints on a simulated KUKA LBR3 Arm.\n(Uses the Generic Articulated Arm contract.)")]
    [AlternateContract(arm.Contract.Identifier)]
    [Contract(Contract.Identifier)]
    public class SimulatedLBR3ArmService : DsspServiceBase
    {
        #region Simulation Variables
        simengine.KukaLBR3Entity _entity;
        simengine.SimulationEnginePort _notificationTarget;
        #endregion

        [Partner("SubMgr", Contract = submgr.Contract.Identifier, CreationPolicy = PartnerCreationPolicy.CreateAlways)]
        private submgr.SubscriptionManagerPort _submgrPort = new submgr.SubscriptionManagerPort();

        [ServiceState]
        private arm.ArticulatedArmState _state = new arm.ArticulatedArmState();
        Dictionary<string, Joint> _jointLookup;

        [ServicePort("/SimulatedLBR3Arm", AllowMultipleInstances=true)]
        private arm.ArticulatedArmOperations _mainPort = new arm.ArticulatedArmOperations();
        /// <summary>
        /// Default Service Constructor
        /// </summary>
        public SimulatedLBR3ArmService(DsspServiceCreationPort creationPort) :
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
                    Arbiter.Receive<DsspDefaultDrop>(false, _mainPort, DefaultDropHandler)
                ),
                new ExclusiveReceiverGroup(),
                new ConcurrentReceiverGroup()
            ));
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
            _entity = (simengine.KukaLBR3Entity)ins.Body;
            _entity.ServiceContract = Contract.Identifier;

            _state = new arm.ArticulatedArmState();

            // use the entity state as our state
            _state.Joints = _entity.Joints;

            // create dictionary for quick lookup of joints from name
            _jointLookup = new Dictionary<string, Joint>();
            foreach (Joint j in _state.Joints)
                _jointLookup.Add(j.State.Name, j);
        }

        void DeleteEntityNotificationHandler(simengine.DeleteSimulationEntity del)
        {
            _entity = null;
        }

        /// <summary>
        /// Get LBR3 arm state
        /// </summary>
        /// <param name="get"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public IEnumerator<ITask> GetHandler(arm.Get get)
        {
            get.ResponsePort.Post(_state);
            yield break;
        }

        /// <summary>
        /// Subscribe to LBR3 arm service
        /// </summary>
        /// <param name="subscribe"></param>
        /// <returns></returns>
        public IEnumerator<ITask> SubscribeHandler(arm.Subscribe subscribe)
        {
            SubscribeRequestType request = subscribe.Body;
            SubscribeHelper(_submgrPort, request, subscribe.ResponsePort);
            yield break;
        }

        /// <summary>
        /// Informs the LBR3 arm to move to a desired pose
        /// </summary>
        /// <param name="update"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public IEnumerator<ITask> SetJointTargetPose(arm.SetJointTargetPose update)
        {
            Joint j = _jointLookup[update.Body.JointName];
            _entity.SetJointTargetOrientation(j, update.Body.TargetOrientation);
            update.ResponsePort.Post(DefaultUpdateResponseType.Instance);

            // send an end effector update notification as well
            base.SendNotification(_submgrPort, update);
            yield break;
        }

        /// <summary>
        /// Informs the LBR3 arm to move to a desired velocity
        /// </summary>
        /// <param name="update"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public IEnumerator<ITask> SetJointTargetVelocity(arm.SetJointTargetVelocity update)
        {
            Joint j = _jointLookup[update.Body.JointName];
            _entity.SetJointTargetVelocity(j, update.Body.TargetVelocity);
            update.ResponsePort.Post(DefaultUpdateResponseType.Instance);

            // send an end effector update notification as well
            base.SendNotification(_submgrPort, update);

            yield break;
        }

        /// <summary>
        /// This is called when the LBR3 service is dropped
        /// </summary>
        /// <param name="drop"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Teardown)]
        public IEnumerator<ITask> DropHandler(DsspDefaultDrop drop)
        {
            base.DefaultDropHandler(drop);
            yield break;
        }
    }
}
