//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: SimulatedBumper.cs $ $Revision: 30 $
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.Ccr.Core;
using Microsoft.Dss.Core;
using Microsoft.Dss.Core.Attributes;
using System.Security.Permissions;
using svcbase = Microsoft.Dss.ServiceModel.DsspServiceBase;
using dssp = Microsoft.Dss.ServiceModel.Dssp;
using contactsensor = Microsoft.Robotics.Services.ContactSensor.Proxy;
using submgr = Microsoft.Dss.Services.SubscriptionManager;
using simtypes = Microsoft.Robotics.Simulation;
using simengine = Microsoft.Robotics.Simulation.Engine;
using physics = Microsoft.Robotics.Simulation.Physics;
using System.ComponentModel;
using Microsoft.Dss.Core.DsspHttp;
using System.Net;
namespace Microsoft.Robotics.Services.Simulation.Sensors.Bumper
{
    /// <summary>
    /// Provides access to a simulated contact sensor array used as bumpers.\n(Uses the Generic Contacts contract.
    /// </summary>
    [DisplayName("(User) Simulated Generic Contact Sensors")]
    [AlternateContract(contactsensor.Contract.Identifier)]
    [Contract(Contract.Identifier)]
    public class SimulatedBumperService : svcbase.DsspServiceBase
    {
        simengine.BumperArrayEntity _entity;
        simengine.SimulationEnginePort _notificationTarget;
        contactsensor.ContactSensorArrayState _state = new contactsensor.ContactSensorArrayState();
        Port<physics.EntityContactNotification> _contactNotificationPort = new Port<physics.EntityContactNotification>();

        Dictionary<physics.Shape, contactsensor.ContactSensor> _bumperShapeToSensorTable;

        [ServicePort("/SimulatedBumper", AllowMultipleInstances = true)]
        contactsensor.ContactSensorArrayOperations _mainPort = new contactsensor.ContactSensorArrayOperations();

        [Partner("SubMgr",
            Contract = submgr.Contract.Identifier,
            CreationPolicy = PartnerCreationPolicy.CreateAlways)]
        submgr.SubscriptionManagerPort _subMgrPort = new submgr.SubscriptionManagerPort();

        /// <summary>
        /// SimulatedBumperService constructor that takes a PortSet to notify when it is created
        /// </summary>
        /// <param name="creationPort"></param>
        public SimulatedBumperService(dssp.DsspServiceCreationPort creationPort) :
                base(creationPort)
        {
        }

        /// <summary>
        /// Start initializes SimulatedBumperService and listens for drop messages 
        /// </summary>
        protected override void Start()
        {
            // Find the Bumper entity we are supposed to be hooked up with
            // Detailed explanation on how simulation services compose with the simulation entities
            // can be found in SimulationTutorial2 and also in the InitializeSimulation() method
            // of other simulation service (like the SimulatedDifferentialDrive)

            _notificationTarget = new simengine.SimulationEnginePort();

            // PartnerType.Service is the entity instance name
            simengine.SimulationEngine.GlobalInstancePort.Subscribe(
                ServiceInfo.PartnerList,
                _notificationTarget);

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

        private void CreateDefaultState()
        {
            // default state has no contact sensors
            _state.Sensors = new List<contactsensor.ContactSensor>();
            _bumperShapeToSensorTable = new Dictionary<physics.Shape, contactsensor.ContactSensor>();
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
            _entity = (simengine.BumperArrayEntity)ins.Body;
            _entity.ServiceContract = Contract.Identifier;

            // reinitialize the state
            CreateDefaultState();

            contactsensor.ContactSensor cs = null;

            // The simulation bumper service uses a simple heuristic to assign contact sensors, to physics shapes:
            // Half the sensors go infront, the other half in the rear. Assume front sensors are first in the list
            // In the for loop below we create a lookup table that matches simulation shapes with sensors. When
            // the physics engine notifies us with a contact, it provides the shape that came into contact. We need to
            // translate that to sensor. In the future we might add an object Context field to shapes to make this easier
            for (int i = 0; i < _entity.Shapes.Length; i++)
            {
                cs = new contactsensor.ContactSensor();
                cs.Name = _entity.Shapes[i].State.Name;
                cs.HardwareIdentifier = i;
                _state.Sensors.Add(cs);
                _bumperShapeToSensorTable.Add((physics.BoxShape)_entity.Shapes[i], cs);
            }

            // subscribe to bumper simulation entity for contact notifications
            _entity.Subscribe(_contactNotificationPort);
            // Activate a handler on the notification port, it will run when contacts occur in simulation
            Activate(Arbiter.Receive(false, _contactNotificationPort, PhysicsContactNotificationHandler));
        }

        void PhysicsContactNotificationHandler(physics.EntityContactNotification contact)
        {
            foreach (physics.ShapeContact sc in contact.ShapeContacts)
            {
                // look up shape involved in collision and check its one of the bumper shapes
                contactsensor.ContactSensor s;
                if (!_bumperShapeToSensorTable.TryGetValue(sc.LocalShape, out s))
                    continue;
                if (contact.Stage == physics.ContactNotificationStage.Started)
                    s.Pressed = true;
                else if (contact.Stage == physics.ContactNotificationStage.Finished)
                    s.Pressed = false;
                s.TimeStamp = DateTime.Now;
                // notification for individual sensor
                _subMgrPort.Post(new submgr.Submit(s, dssp.DsspActions.UpdateRequest));
            }

            // send notification
            _subMgrPort.Post(new submgr.Submit(_state, dssp.DsspActions.ReplaceRequest));

            // reactivate notification handler
            Activate(Arbiter.Receive(false, _contactNotificationPort, PhysicsContactNotificationHandler));
        }

        /// <summary>
        /// Get SimulatedBumperService state
        /// </summary>
        /// <param name="get"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public IEnumerator<ITask> GetHandler(contactsensor.Get get)
        {
            get.ResponsePort.Post(_state);
            yield break;
        }

        /// <summary>
        /// Get SimulatedBumperService state
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
        /// Handler for replace messages
        /// </summary>
        /// <param name="replace"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public IEnumerator<ITask> ReplaceHandler(contactsensor.Replace replace)
        {
            _state = replace.Body;
            replace.ResponsePort.Post(dssp.DefaultReplaceResponseType.Instance);
            _subMgrPort.Post(new submgr.Submit(_state, dssp.DsspActions.ReplaceRequest));
            yield break;
        }

        /// <summary>
        /// Handler for subscribe messages
        /// </summary>
        /// <param name="subscribe"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public IEnumerator<ITask> SubscribeHandler(contactsensor.Subscribe subscribe)
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
        /// Handler for subscribe messages
        /// </summary>
        /// <param name="subscribe"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public IEnumerator<ITask> ReliableSubscribeHandler(contactsensor.ReliableSubscribe subscribe)
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
