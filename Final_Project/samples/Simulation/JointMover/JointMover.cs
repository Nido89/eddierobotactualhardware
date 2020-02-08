//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: JointMover.cs $ $Revision: 11 $
//-----------------------------------------------------------------------

using Microsoft.Ccr.Core;
using Microsoft.Dss.Core;
using Microsoft.Dss.Core.Attributes;
using Microsoft.Dss.ServiceModel.Dssp;
using Microsoft.Dss.ServiceModel.DsspServiceBase;
using submgr = Microsoft.Dss.Services.SubscriptionManager;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;

using simtypes = Microsoft.Robotics.Simulation;
using sim = Microsoft.Robotics.Simulation;
using simengine = Microsoft.Robotics.Simulation.Engine;
using Physics = Microsoft.Robotics.Simulation.Physics;
using System.ComponentModel;
using Microsoft.Dss.Core.DsspHttp;
using Microsoft.Robotics.PhysicalModel;
using Microsoft.Robotics.Simulation.Physics;
using W3C.Soap;
using Microsoft.Ccr.Adapters.WinForms;

namespace ProMRDS.Simulation.JointMover
{
    [DisplayName("(User) Joint Mover")]
    [Description("Allows joints in a sim entity to be manipulated.")]
    [DssCategory(simtypes.PublishedCategories.SimulationService)]
    [Contract(Contract.Identifier)]
    public class JointMoverService : DsspServiceBase
    {
        #region Simulation Variables
        simengine.SimulationEnginePort _simEngine;
        simengine.VisualEntity _entity;
        simengine.SimulationEnginePort _notificationTarget;
        #endregion

        // This port receives events from the user interface
        FromWinformEvents _fromWinformPort = new FromWinformEvents();
        SimulatedBipedMoverUI _simulatedBipedMoverUI = null;

        [Partner("SubMgr", Contract = submgr.Contract.Identifier, CreationPolicy = PartnerCreationPolicy.CreateAlways)]
        submgr.SubscriptionManagerPort _subMgrPort = new submgr.SubscriptionManagerPort();
        string _subMgrUri = string.Empty;

        [InitialStatePartner(Optional = true)]
        private JointMoverState _state = new JointMoverState();

        [ServicePort("/SimulatedJointMover", AllowMultipleInstances = true)]
        private JointMoverOperations _mainPort = new JointMoverOperations();
        public JointMoverService(DsspServiceCreationPort creationPort) :
            base(creationPort)
        {
        }

        protected override void Start()
        {
            _simEngine = simengine.SimulationEngine.GlobalInstancePort;
            _notificationTarget = new simengine.SimulationEnginePort();

            if (_state == null)
                CreateDefaultState();

            // PartnerType.Service is the entity instance name. 
            _simEngine.Subscribe(ServiceInfo.PartnerList, _notificationTarget);

            // don't start listening to DSSP operations, other than drop, until notification of entity
            Activate(new Interleave(
                new TeardownReceiverGroup
                (
                    Arbiter.Receive<simengine.InsertSimulationEntity>(false, _notificationTarget, InsertEntityNotificationHandlerFirstTime),
                    Arbiter.Receive<DsspDefaultDrop>(false, _mainPort, DefaultDropHandler)
                ),
                new ExclusiveReceiverGroup
                (
                    Arbiter.Receive<FromWinformMsg>(true, _fromWinformPort, OnWinformMessageHandler)
                ),
                new ConcurrentReceiverGroup()
            ));

            // Create the user interface form
            WinFormsServicePort.Post(new Microsoft.Ccr.Adapters.WinForms.RunForm(CreateForm));

            Activate(Arbiter.Receive(false, TimeoutPort(5000), dateTime => SpawnIterator(RefreshListIterator)));
        }

        // Create the UI form
        System.Windows.Forms.Form CreateForm()
        {
            return new SimulatedBipedMoverUI(_fromWinformPort);
        }

        // process messages from the UI Form
        void OnWinformMessageHandler(FromWinformMsg msg)
        {
            switch (msg.Command)
            {
                case FromWinformMsg.MsgEnum.Loaded:
                    // the windows form is ready to go
                    _simulatedBipedMoverUI = (SimulatedBipedMoverUI)msg.Object;
                    break;

                case FromWinformMsg.MsgEnum.MoveJoint:
                    MoveJoint((MoveJoint)msg.Object);
                    break;

                case FromWinformMsg.MsgEnum.Suspend:
                    Task<simengine.VisualEntity, bool> deferredTask =
                        new Task<simengine.VisualEntity, bool>(_entity, (bool)msg.Object, SuspendBipedInternal);
                    _entity.DeferredTaskQueue.Post(deferredTask);
                    break;

                case FromWinformMsg.MsgEnum.ChangeEntity:
                    {
                        string entityName = (string)msg.Object;
                        if (string.IsNullOrEmpty(entityName) == false)
                        {
                            if (_entity == null || _entity.State.Name != entityName || _state.Joints==null || _state.Joints.Count == 0)
                            {
                                DeleteEntityInternal();
                                simengine.EntitySubscribeRequestType req = new Microsoft.Robotics.Simulation.Engine.EntitySubscribeRequestType();
                                req.Name = entityName;
                                _simEngine.Subscribe(req, _notificationTarget);
                            }
                        }
                        break;
                    }

                case FromWinformMsg.MsgEnum.RefreshList:
                    {
                        SpawnIterator(RefreshListIterator);
                        break;
                    }
            }
        }

        IEnumerator<ITask> RefreshListIterator()
        {
            var getOrFault = _simEngine.Get();
            yield return getOrFault.Choice();
            Fault ex = (Fault)getOrFault;
            if (ex != null)
            {
                LogError(ex);
                yield break;
            }

            var simState = (sim.SimulationState)getOrFault;
            WinFormsServicePort.FormInvoke(() =>
            {
                foreach (var entity in simState.Entities)
                {
                    if (entity is simengine.GlobalJointEntity)
                    {
                        _simulatedBipedMoverUI.AddEntityName(entity.State.Name, _state.Joints==null || _state.Joints.Count == 0);
                    }
                }
            });

        }

        void MoveJoint(MoveJoint move)
        {
            DOFDesc dof = _state.Joints[move.Name];
            JointDesc desc = dof.Description;
            Vector3 targetVelocity = new Vector3(0, 0, 0);

            switch (dof.Type)
            {
                case DOFType.Twist:
                    desc.TwistAngle = (float)move.Angle;
                    targetVelocity = new Vector3((float)-move.Angle, 0, 0);
                    break;
                case DOFType.Swing1:
                    desc.Swing1Angle = (float)move.Angle;
                    targetVelocity = new Vector3(0, (float)-move.Angle, 0);
                    break;
                case DOFType.Swing2:
                    desc.Swing2Angle = (float)move.Angle;
                    targetVelocity = new Vector3(0, 0, (float)-move.Angle);
                    break;
                case DOFType.X:
                    desc.X = (float)move.Angle;
                    targetVelocity = new Vector3((float)-move.Angle, 0, 0);
                    break;
                case DOFType.Y:
                    desc.Y = (float)move.Angle;
                    targetVelocity = new Vector3(0, (float)-move.Angle, 0);
                    break;
                case DOFType.Z:
                    desc.Z = (float)move.Angle;
                    targetVelocity = new Vector3(0, 0, (float)-move.Angle);
                    break;
            }

            PhysicsJoint thisJoint = null;
            if (desc.Joint != null)
                thisJoint = (PhysicsJoint)desc.Joint;
            else
                thisJoint = (PhysicsJoint)desc.JointEntity.ParentJoint;

            if (IsVelocityDrive(thisJoint.State, dof.Type))
            {
                _entity.DeferredTaskQueue.Post(new Task(() => SetDriveInternal(thisJoint, targetVelocity)));
            }
            else
            {
                Task<Physics.PhysicsJoint, Quaternion, Vector3> deferredTask =
                    new Task<Physics.PhysicsJoint, Quaternion, Vector3>(thisJoint, desc.JointOrientation, desc.JointPosition, SetDriveInternal);
                _entity.DeferredTaskQueue.Post(deferredTask);
            }
        }

        void SetDriveInternal(Physics.PhysicsJoint joint, Quaternion orientation, Vector3 position)
        {
            if (joint.State.Angular != null)
                joint.SetAngularDriveOrientation(orientation);
            if (joint.State.Linear != null)
                joint.SetLinearDrivePosition(position);
        }
        void SetDriveInternal(Physics.PhysicsJoint joint, Vector3 targetVelocity)
        {
            if (joint.State.Angular != null)
                joint.SetAngularDriveVelocity(targetVelocity);
            if (joint.State.Linear != null)
                joint.SetAngularDriveVelocity(targetVelocity);
        }


        void SuspendBipedInternal(simengine.VisualEntity entity, bool suspend)
        {
            if (suspend)
            {
                entity.PhysicsEntity.IsKinematic = true;
                entity.State.Flags |= Microsoft.Robotics.Simulation.Physics.EntitySimulationModifiers.Kinematic;
                MoveBipedPose(new Vector3(0, 0.3f, 0), entity);
            }
            else
            {
                entity.State.Flags &= ~Microsoft.Robotics.Simulation.Physics.EntitySimulationModifiers.Kinematic;
                MoveBipedPose(new Vector3(0, -0.28f, 0), entity);
                Activate(Arbiter.Receive(false, TimeoutPort(200), SetDynamic));
            }
        }

        void SetDynamic(DateTime now)
        {
            _entity.PhysicsEntity.IsKinematic = false;
        }

        void MoveBipedPose(Vector3 offset, simengine.VisualEntity entity)
        {
            entity.PhysicsEntity.SetPose(new Pose(offset + entity.State.Pose.Position, entity.State.Pose.Orientation));
            foreach (simengine.VisualEntity child in entity.Children)
                MoveBipedPose(offset, child);
        }

        void CreateDefaultState()
        {
            if (_state == null)
            {
                _state = new JointMoverState();
                _state.Joints = new Dictionary<string, DOFDesc>();
            }
        }

        void InsertEntityNotificationHandlerFirstTime(simengine.InsertSimulationEntity ins)
        {
            InsertEntityNotificationHandler(ins);

            base.Start();

            // Listen on the main port for requests and call the appropriate handler.
            MainPortInterleave.CombineWith(
                new Interleave(
                    new TeardownReceiverGroup(),
                    new ExclusiveReceiverGroup(
                        Arbiter.Receive<simengine.InsertSimulationEntity>(true, _notificationTarget, InsertEntityNotificationHandler),
                        Arbiter.Receive<simengine.DeleteSimulationEntity>(true, _notificationTarget, DeleteEntityNotificationHandler),
                        Arbiter.Receive<FromWinformMsg>(true, _fromWinformPort, OnWinformMessageHandler)
                    ),
                    new ConcurrentReceiverGroup()
                )
            );
        }

        void InsertEntityNotificationHandler(simengine.InsertSimulationEntity ins)
        {
            _entity = (simengine.VisualEntity)ins.Body;
            _entity.ServiceContract = Contract.Identifier;

            // traverse the entities associated with this entity and build a list of joints
            _state.Joints.Clear();
            Dictionary<string, simengine.VisualEntity> entities = new Dictionary<string, simengine.VisualEntity>();
            FindJointsAndEntities(_entity, entities);

            // add a slider for each joint to the UI
            WinFormsServicePort.FormInvoke(
                delegate()
                {
                    _simulatedBipedMoverUI.AddEntityName(_entity.State.Name, _state.Joints == null || _state.Joints.Count == 0);
                    _simulatedBipedMoverUI.AddSliders(_state.Joints);
                }
            );
        }

        private void FindJointsAndEntities(simengine.VisualEntity thisEntity, Dictionary<string, simengine.VisualEntity> visited)
        {

            if (thisEntity == null)
                return;

            if (visited.ContainsKey(thisEntity.State.Name))
                return;
            else
                visited.Add(thisEntity.State.Name, thisEntity);

            // process the children first
            if (thisEntity is simengine.VisualEntity)
            {
                foreach (simengine.VisualEntity Child in ((simengine.VisualEntity)thisEntity).Children)
                    FindJointsAndEntities(Child, visited);
            }


            if (thisEntity.ParentJoint != null)
                AddJoint(thisEntity.ParentJoint, thisEntity);

            // search this entity for other entities or joints
            Type thisEntityType = thisEntity.GetType();
            System.Reflection.FieldInfo[] fields = thisEntityType.GetFields(
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.NonPublic);
            foreach (FieldInfo field in fields)
            {
                // search for joints
                if ((field.FieldType == typeof(Joint)) || (field.FieldType.IsSubclassOf(typeof(Joint))))
                {
                    Joint thisJoint = (Joint)field.GetValue(thisEntity);
                    if (thisJoint != null)
                        AddJoint(thisJoint, thisEntity);
                }
                else if (field.FieldType == typeof(List<Joint>))
                {
                    List<Joint> jointList = (List<Joint>)field.GetValue(thisEntity);
                    if (jointList != null)
                    {
                        foreach (Joint thisJoint in jointList)
                            AddJoint(thisJoint, thisEntity);
                    }
                }
                else if (field.FieldType == typeof(Joint[]))
                {
                    Joint[] jointArray = (Joint[])field.GetValue(thisEntity);
                    if (jointArray != null)
                    {
                        foreach (Joint thisJoint in jointArray)
                            AddJoint(thisJoint, thisEntity);
                    }
                }

                // search for entities
                if ((field.FieldType == typeof(simtypes.Entity)) || (field.FieldType.IsSubclassOf(typeof(simtypes.Entity))))
                    FindJointsAndEntities((simengine.VisualEntity)field.GetValue(thisEntity), visited);
                else if (field.FieldType.IsGenericType)
                {
                    Type[] parms = field.FieldType.GetGenericArguments();
                    if (parms.Length == 1)
                    {
                        if ((parms[0] == typeof(simtypes.Entity)) || (parms[0].IsSubclassOf(typeof(simtypes.Entity))))
                            foreach (simengine.VisualEntity someEntity in (System.Collections.ICollection)field.GetValue(thisEntity))
                                FindJointsAndEntities(someEntity, visited);
                    }
                }
            }
        }

        void AddJoint(Joint joint, simengine.VisualEntity entity)
        {
            if (!(joint is PhysicsJoint))
                return;

            if (!_state.Joints.ContainsKey(joint.State.Name))
            {
                JointDesc description = new JointDesc(joint.State.Name, entity, joint as PhysicsJoint);
                string[] DOFNames = joint.State.Name.Split(';');
                int which = 0;
                if (joint.State.Angular != null)
                {
                    if (joint.State.Angular.TwistMode != JointDOFMode.Locked && IsNotUselessDrive(joint.State.Angular.TwistDrive))
                        AddDOF(DOFNames, which++, description, DOFType.Twist);
                    if (joint.State.Angular.Swing1Mode != JointDOFMode.Locked && IsNotUselessDrive(joint.State.Angular.SwingDrive))
                        AddDOF(DOFNames, which++, description, DOFType.Swing1);
                    if (joint.State.Angular.Swing2Mode != JointDOFMode.Locked && IsNotUselessDrive(joint.State.Angular.SlerpDrive))
                        AddDOF(DOFNames, which++, description, DOFType.Swing2);
                }
                if (joint.State.Linear != null)
                {
                    if (joint.State.Linear.XMotionMode != JointDOFMode.Locked && IsNotUselessDrive(joint.State.Linear.XDrive))
                        AddDOF(DOFNames, which++, description, DOFType.X);
                    if (joint.State.Linear.YMotionMode != JointDOFMode.Locked && IsNotUselessDrive(joint.State.Linear.YDrive))
                        AddDOF(DOFNames, which++, description, DOFType.Y);
                    if (joint.State.Linear.ZMotionMode != JointDOFMode.Locked && IsNotUselessDrive(joint.State.Linear.ZDrive))
                        AddDOF(DOFNames, which++, description, DOFType.Z);
                }
            }
        }

        private bool IsNotUselessDrive(JointDriveProperties jointDriveProperties)
        {
            if (jointDriveProperties==null || (jointDriveProperties.Mode == JointDriveMode.Position &&
                jointDriveProperties.Spring!=null && jointDriveProperties.Spring.SpringCoefficient == 0))
                return false;
            else
                return true;
        }


        void AddDOF(string[] DOFNames, int which, JointDesc desc, DOFType type)
        {
            bool isVelocityDrive = false;
            float defaultDriveValue = 0.0f;

            float min = -180, max = 180;
            if (which < DOFNames.Length)
            {
                string[] subNames = DOFNames[which].Split('|');
                try
                {
                    if (subNames.Length > 1)
                        min = Single.Parse(subNames[1]);
                    if (subNames.Length > 2)
                        max = Single.Parse(subNames[2]);
                }
                catch
                {
                }
                if (IsVelocityDrive(desc.Joint.State, type))
                {
                    float angularVelocityLength = 0; 
                    if (Math.Abs(desc.Joint.State.Angular.DriveTargetVelocity.X) > Math.Abs(angularVelocityLength))
                        angularVelocityLength = desc.Joint.State.Angular.DriveTargetVelocity.X;

                    if (Math.Abs(desc.Joint.State.Angular.DriveTargetVelocity.Y) > Math.Abs(angularVelocityLength))
                        angularVelocityLength = desc.Joint.State.Angular.DriveTargetVelocity.Y;

                    if (Math.Abs(desc.Joint.State.Angular.DriveTargetVelocity.Z) > Math.Abs(angularVelocityLength))
                        angularVelocityLength = desc.Joint.State.Angular.DriveTargetVelocity.Z;

                    if (Math.Abs(angularVelocityLength) < float.Epsilon)
                    {
                        min = -2.0f;
                        max = 2.0f;
                    }
                    else
                    {
                        min = Math.Min(-angularVelocityLength, angularVelocityLength);
                        max = Math.Max(-angularVelocityLength, angularVelocityLength);
                        defaultDriveValue = -angularVelocityLength;
                    }
                    isVelocityDrive = true;
                }
                _state.Joints.Add(subNames[0], new DOFDesc(subNames[0], desc, type, min, max, isVelocityDrive, defaultDriveValue));
            }
            else
            {
                string name = DOFNames[0];
                switch (which)
                {
                    case 0: name += " Twist"; break;
                    case 1: name += " Swing1"; break;
                    case 2: name += " Swing2"; break;
                    case 3: name += " X"; min = -2; max = 2; break;
                    case 4: name += " Y"; min = -2; max = 2; break;
                    case 5: name += " Z"; min = -2; max = 2; break;
                }
                _state.Joints.Add(name, new DOFDesc(name, desc, type, min, max, isVelocityDrive, defaultDriveValue));
            }
        }

        private bool IsVelocityDrive(JointProperties jointProperties, DOFType dof)
        {
            switch(dof)
            {
                case DOFType.Twist:
                    if (jointProperties.Angular != null && jointProperties.Angular.TwistDrive != null &&
                       jointProperties.Angular.TwistDrive.Mode == JointDriveMode.Velocity)
                        return true;
                    break;

                case DOFType.Swing1:
                case DOFType.Swing2:
                    if (jointProperties.Angular != null && jointProperties.Angular.SwingDrive != null &&
                        jointProperties.Angular.SwingDrive.Mode == JointDriveMode.Velocity)
                        return true;
                    break;

                case DOFType.X:
                    if (jointProperties.Linear != null && jointProperties.Linear.XDrive != null &&
                            jointProperties.Linear.XDrive.Mode == JointDriveMode.Velocity)
                        return true;
                    break;

                case DOFType.Y:
                    if (jointProperties.Linear != null && jointProperties.Linear.YDrive != null &&
                                jointProperties.Linear.YDrive.Mode == JointDriveMode.Velocity)
                        return true;
                    break;

                case DOFType.Z:
                    if (jointProperties.Linear != null && jointProperties.Linear.ZDrive != null &&
                                jointProperties.Linear.ZDrive.Mode == JointDriveMode.Velocity)
                        return true;
                    break;
            }

            return false;
        }

        void DeleteEntityNotificationHandler(simengine.DeleteSimulationEntity del)
        {
            DeleteEntityInternal();
        }

        void DeleteEntityInternal()
        {
            _entity = null;
            // add a slider for each joint to the UI
            WinFormsServicePort.FormInvoke(
                delegate()
                {
                    _simulatedBipedMoverUI.ClearNames();
                    _simulatedBipedMoverUI.ClearSliders();
                    Activate(Arbiter.Receive(false, TimeoutPort(5000), dateTime => SpawnIterator(RefreshListIterator)));
                }
            );
        }


        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public IEnumerator<ITask> GetHandler(HttpGet get)
        {
            get.ResponsePort.Post(new HttpResponseType(_state));
            yield break;
        }

        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public IEnumerator<ITask> GetHandler(Get get)
        {
            get.ResponsePort.Post(_state);
            yield break;
        }
    }
}
