//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: SimulatedDifferentialDrive.cs $ $Revision: 54 $
//-----------------------------------------------------------------------

using Microsoft.Ccr.Core;
using Microsoft.Dss.Core;
using Microsoft.Dss.Core.Attributes;
using Microsoft.Dss.ServiceModel.Dssp;
using Microsoft.Dss.ServiceModel.DsspServiceBase;
using submgr = Microsoft.Dss.Services.SubscriptionManager;
using System;
using System.Collections.Generic;
using System.Xml;
using diffdrive = Microsoft.Robotics.Services.Drive.Proxy;
using W3C.Soap;

using simtypes = Microsoft.Robotics.Simulation;
using simengine = Microsoft.Robotics.Simulation.Engine;
using physics = Microsoft.Robotics.Simulation.Physics;
using System.ComponentModel;
using Microsoft.Dss.Core.DsspHttp;

namespace Microsoft.Robotics.Services.Simulation.Drive
{
    /// <summary>
    /// Provides access to a simulated differential drive service.\n(Uses the Generic Differential Drive contract.
    /// </summary>
    [DisplayName("(User) Simulated Generic Differential Drive")]
    [AlternateContract(diffdrive.Contract.Identifier)]
    [DssCategory(simtypes.PublishedCategories.SimulationService)]
    [Contract(Contract.Identifier)]
    [DssServiceDescription("http://msdn.microsoft.com/library/cc998469.aspx")]
    public class SimulatedDifferentialDriveService : Microsoft.Dss.ServiceModel.DsspServiceBase.DsspServiceBase
    {
        #region Simulation Variables
        simengine.SimulationEnginePort _simEngine;
        simengine.DifferentialDriveEntity _entity;
        simengine.SimulationEnginePort _notificationTarget;
        #endregion

        [Partner("SubMgr", Contract = submgr.Contract.Identifier, CreationPolicy = PartnerCreationPolicy.CreateAlways)]
        submgr.SubscriptionManagerPort _subMgrPort = new submgr.SubscriptionManagerPort();
        string _subMgrUri = string.Empty;

        [InitialStatePartner(Optional = true)]
        private diffdrive.DriveDifferentialTwoWheelState _state = new diffdrive.DriveDifferentialTwoWheelState();

        [ServicePort("/SimulatedDifferentialDrive", AllowMultipleInstances=true)]
        private diffdrive.DriveOperations _mainPort = new diffdrive.DriveOperations();

        /// <summary>
        /// SimulatedDifferentialDriveService constructor that takes a PortSet to
        /// notify when the service is created
        /// </summary>
        /// <param name="creationPort"></param>
        public SimulatedDifferentialDriveService(DsspServiceCreationPort creationPort) :
                base(creationPort)
        {
        }

        /// <summary>
        /// Start initializes service state and listens for drop messages
        /// </summary>
        protected override void Start()
        {
            // Find our simulation entity that represents the "hardware" or real-world service.
            // To hook up with simulation entities we do the following steps
            // 1) have a manifest or some other service create us, specifying a partner named SimulationEntity
            // 2) in the simulation service (us) issue a subscribe to the simulation engine looking for
            //    an instance of that simulation entity. We use the Entity.State.Name for the match so it must be
            //    exactly the same. See SimulationTutorial2 for the creation process
            // 3) Listen for a notification telling us the entity is available
            // 4) cache reference to entity and communicate with it issuing low level commands.

            _simEngine = simengine.SimulationEngine.GlobalInstancePort;
            _notificationTarget = new simengine.SimulationEnginePort();

            if (_state == null)
                CreateDefaultState();

            // enabled by default
            _state.IsEnabled = true;  
                
            // PartnerType.Service is the entity instance name.
            _simEngine.Subscribe(ServiceInfo.PartnerList, _notificationTarget);

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

        void CreateDefaultState()
        {
            _state = new diffdrive.DriveDifferentialTwoWheelState();
            _state.LeftWheel = new Microsoft.Robotics.Services.Motor.Proxy.WheeledMotorState();
            _state.RightWheel = new Microsoft.Robotics.Services.Motor.Proxy.WheeledMotorState();
            _state.LeftWheel.MotorState = new Microsoft.Robotics.Services.Motor.Proxy.MotorState();
            _state.RightWheel.MotorState = new Microsoft.Robotics.Services.Motor.Proxy.MotorState();
        }

        void UpdateStateFromSimulation()
        {
            if (_entity != null)
            {
                _state.TimeStamp = DateTime.Now;
                _state.LeftWheel.MotorState.CurrentPower = _entity.LeftWheel.Wheel.MotorTorque;
                _state.RightWheel.MotorState.CurrentPower = _entity.RightWheel.Wheel.MotorTorque;
                _state.IsEnabled = _entity.IsEnabled;
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
                        Arbiter.Receive<simengine.DeleteSimulationEntity>(true, _notificationTarget, DeleteEntityNotificationHandler)
                    ),
                    new ConcurrentReceiverGroup()
                )
            );
        }

        void InsertEntityNotificationHandler(simengine.InsertSimulationEntity ins)
        {
            _entity = (simengine.DifferentialDriveEntity)ins.Body;
            _entity.ServiceContract = Contract.Identifier;

            // create default state based on the physics entity
            if(_entity.ChassisShape != null)
                _state.DistanceBetweenWheels = _entity.ChassisShape.BoxState.Dimensions.X;

            _state.LeftWheel.MotorState.PowerScalingFactor = _entity.MotorTorqueScaling;
            _state.RightWheel.MotorState.PowerScalingFactor = _entity.MotorTorqueScaling;
            //SpawnIterator(TestDriveDistanceAndRotateDegrees);
        }

        void DeleteEntityNotificationHandler(simengine.DeleteSimulationEntity del)
        {
            _entity = null;
        }

        /// <summary>
        /// Get handler retrieves service state
        /// </summary>
        /// <param name="get"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public IEnumerator<ITask> GetHandler(HttpGet get)
        {
            UpdateStateFromSimulation();
            get.ResponsePort.Post(new HttpResponseType(_state));
            yield break;
        }

        /// <summary>
        /// Get handler retrieves service state
        /// </summary>
        /// <param name="get"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public IEnumerator<ITask> GetHandler(diffdrive.Get get)
        {
            UpdateStateFromSimulation();
            get.ResponsePort.Post(_state);
            yield break;
        }

        #region Subscribe Handling
        /// <summary>
        /// Subscribe to Differential Drive service
        /// </summary>
        /// <param name="subscribe"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public IEnumerator<ITask> SubscribeHandler(diffdrive.Subscribe subscribe)
        {
            Activate(Arbiter.Choice(
                SubscribeHelper(_subMgrPort, subscribe.Body, subscribe.ResponsePort),
                delegate(SuccessResult success)
                {
                    _subMgrPort.Post(new submgr.Submit(
                        subscribe.Body.Subscriber, DsspActions.UpdateRequest, _state, null));
                },
                delegate(Exception ex) { LogError(ex); }
            ));

            yield break;
        }

        /// <summary>
        /// Subscribe to Differential Drive service
        /// </summary>
        /// <param name="subscribe"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public IEnumerator<ITask> ReliableSubscribeHandler(diffdrive.ReliableSubscribe subscribe)
        {
            Activate(Arbiter.Choice(
                SubscribeHelper(_subMgrPort, subscribe.Body, subscribe.ResponsePort),
                delegate(SuccessResult success)
                {
                    _subMgrPort.Post(new submgr.Submit(
                        subscribe.Body.Subscriber, DsspActions.UpdateRequest, _state, null));
                },
                delegate(Exception ex) { LogError(ex); }
            ));
            yield break;
        }
        #endregion

        /// <summary>
        /// Handler for drive request
        /// </summary>
        /// <param name="driveDistance"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public IEnumerator<ITask> DriveDistanceHandler(diffdrive.DriveDistance driveDistance)
        {
            if (_entity == null)
                throw new InvalidOperationException("Simulation entity not registered with service");

            if (!_state.IsEnabled)
            {
                driveDistance.ResponsePort.Post(Fault.FromException(new Exception("Drive is not enabled.")));
                LogError("DriveDistance request to disabled drive.");
                yield break;
            }

            if ((driveDistance.Body.Power > 1.0f) || (driveDistance.Body.Power < -1.0f))
            {
                // invalid drive power
                driveDistance.ResponsePort.Post(Fault.FromException(new Exception("Invalid Power parameter.")));
                LogError("Invalid Power parameter in DriveDistanceHandler."); 
                yield break;
            }

            _state.DriveDistanceStage = driveDistance.Body.DriveDistanceStage;
            if (driveDistance.Body.DriveDistanceStage == diffdrive.DriveStage.InitialRequest)
            {
                Port<simengine.OperationResult> entityResponse = new Port<simengine.OperationResult>();
                Activate(Arbiter.Receive<simengine.OperationResult>(false, entityResponse, delegate(simengine.OperationResult result)
                {
                    // post a message to ourselves indicating that the drive distance has completed
                    diffdrive.DriveDistanceRequest req = new diffdrive.DriveDistanceRequest(0, 0);
                    switch (result)
                    {
                        case simengine.OperationResult.Error:
                            req.DriveDistanceStage = diffdrive.DriveStage.Canceled;
                            break;
                        case simengine.OperationResult.Canceled:
                            req.DriveDistanceStage = diffdrive.DriveStage.Canceled;
                            break;
                        case simengine.OperationResult.Completed:
                            req.DriveDistanceStage = diffdrive.DriveStage.Completed;
                            break;
                    }
                    _mainPort.Post(new diffdrive.DriveDistance(req));
                }));

                _entity.DriveDistance((float)driveDistance.Body.Distance, (float)driveDistance.Body.Power, entityResponse);

                diffdrive.DriveDistanceRequest req2 = new diffdrive.DriveDistanceRequest(0, 0);
                req2.DriveDistanceStage = diffdrive.DriveStage.Started;
                _mainPort.Post(new diffdrive.DriveDistance(req2));
            }
            else
            {
                base.SendNotification(_subMgrPort, driveDistance);
            }
            driveDistance.ResponsePort.Post(DefaultUpdateResponseType.Instance);
            yield break;
        }

        /// <summary>
        /// Handler for rotate request
        /// </summary>
        /// <param name="rotate"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public IEnumerator<ITask> RotateHandler(diffdrive.RotateDegrees rotate)
        {
            if (_entity == null)
                throw new InvalidOperationException("Simulation entity not registered with service");

            if (!_state.IsEnabled)
            {
                rotate.ResponsePort.Post(Fault.FromException(new Exception("Drive is not enabled.")));
                LogError("RotateDegrees request to disabled drive.");
                yield break;
            }

            _state.RotateDegreesStage = rotate.Body.RotateDegreesStage;
            if (rotate.Body.RotateDegreesStage == diffdrive.DriveStage.InitialRequest)
            {
                Port<simengine.OperationResult> entityResponse = new Port<simengine.OperationResult>();
                Activate(Arbiter.Receive<simengine.OperationResult>(false, entityResponse, delegate(simengine.OperationResult result)
                {
                    // post a message to ourselves indicating that the drive distance has completed
                    diffdrive.RotateDegreesRequest req = new diffdrive.RotateDegreesRequest(0, 0);
                    switch (result)
                    {
                        case simengine.OperationResult.Error:
                            req.RotateDegreesStage = diffdrive.DriveStage.Canceled;
                            break;
                        case simengine.OperationResult.Canceled:
                            req.RotateDegreesStage = diffdrive.DriveStage.Canceled;
                            break;
                        case simengine.OperationResult.Completed:
                            req.RotateDegreesStage = diffdrive.DriveStage.Completed;
                            break;
                    }
                    _mainPort.Post(new diffdrive.RotateDegrees(req));
                }));

                _entity.RotateDegrees((float)rotate.Body.Degrees, (float)rotate.Body.Power, entityResponse);

                diffdrive.RotateDegreesRequest req2 = new diffdrive.RotateDegreesRequest(0, 0);
                req2.RotateDegreesStage = diffdrive.DriveStage.Started;
                _mainPort.Post(new diffdrive.RotateDegrees(req2));
            }
            else
            {
                base.SendNotification(_subMgrPort, rotate);
            }
            rotate.ResponsePort.Post(DefaultUpdateResponseType.Instance);
            yield break;
        }

        /// <summary>
        /// Handler for setting the drive power
        /// </summary>
        /// <param name="setPower"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public IEnumerator<ITask> SetPowerHandler(diffdrive.SetDrivePower setPower)
        {
            if (_entity == null)
                throw new InvalidOperationException("Simulation entity not registered with service");

            if (!_state.IsEnabled)
            {
                setPower.ResponsePort.Post(Fault.FromException(new Exception("Drive is not enabled.")));
                LogError("SetPower request to disabled drive.");
                yield break;
            }

            if ((setPower.Body.LeftWheelPower > 1.0f) || (setPower.Body.LeftWheelPower < -1.0f) ||
                (setPower.Body.RightWheelPower > 1.0f) || (setPower.Body.RightWheelPower < -1.0f))
            {
                // invalid drive power
                setPower.ResponsePort.Post(Fault.FromException(new Exception("Invalid Power parameter.")));
                LogError("Invalid Power parameter in SetPowerHandler.");
                yield break;
            }


            // Call simulation entity method for setting wheel torque
            _entity.SetMotorTorque(
                (float)(setPower.Body.LeftWheelPower),
                (float)(setPower.Body.RightWheelPower));

            UpdateStateFromSimulation();
            setPower.ResponsePort.Post(DefaultUpdateResponseType.Instance);

            // send update notification for entire state
            _subMgrPort.Post(new submgr.Submit(_state, DsspActions.UpdateRequest));
            yield break;
        }

        /// <summary>
        /// Handler for setting the drive speed
        /// </summary>
        /// <param name="setSpeed"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public IEnumerator<ITask> SetSpeedHandler(diffdrive.SetDriveSpeed setSpeed)
        {
            if (_entity == null)
                throw new InvalidOperationException("Simulation entity not registered with service");

            if (!_state.IsEnabled)
            {
                setSpeed.ResponsePort.Post(Fault.FromException(new Exception("Drive is not enabled.")));
                LogError("SetSpeed request to disabled drive.");
                yield break;
            }

            _entity.SetVelocity(
                (float)setSpeed.Body.LeftWheelSpeed,
                (float)setSpeed.Body.RightWheelSpeed);

            UpdateStateFromSimulation();
            setSpeed.ResponsePort.Post(DefaultUpdateResponseType.Instance);

            // send update notification for entire state
            _subMgrPort.Post(new submgr.Submit(_state, DsspActions.UpdateRequest));
            yield break;
        }

        /// <summary>
        /// Handler for enabling or disabling the drive
        /// </summary>
        /// <param name="enable"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public IEnumerator<ITask> EnableHandler(diffdrive.EnableDrive enable)
        {
            if (_entity == null)
                throw new InvalidOperationException("Simulation entity not registered with service");

            _state.IsEnabled = enable.Body.Enable;
            _entity.IsEnabled = _state.IsEnabled;

            UpdateStateFromSimulation();
            enable.ResponsePort.Post(DefaultUpdateResponseType.Instance);

            // send update for entire state
            _subMgrPort.Post(new submgr.Submit(_state, DsspActions.UpdateRequest));
            yield break;
        }

        /// <summary>
        /// Handler when the drive receives an all stop message
        /// </summary>
        /// <param name="estop"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public IEnumerator<ITask> AllStopHandler(diffdrive.AllStop estop)
        {
            if (_entity == null)
                throw new InvalidOperationException("Simulation entity not registered with service");

            _entity.SetMotorTorque(0,0);
            _entity.SetVelocity(0);

            // AllStop disables the drive
            _entity.IsEnabled = false;

            UpdateStateFromSimulation();
            estop.ResponsePort.Post(DefaultUpdateResponseType.Instance);

            // send update for entire state
            _subMgrPort.Post(new submgr.Submit(_state, DsspActions.UpdateRequest));
            yield break;
        }

        /// <summary>
        /// Resets the encoder tick count to zero on both wheels
        /// Not implemented in this service.
        /// </summary>
        /// <param name="reset">Request message</param>
        [ServiceHandler]
        public void EncoderResetHandler(diffdrive.ResetEncoders reset)
        {
            throw new NotImplementedException();
        }

        // Test the DriveDistance and RotateDegrees messages
        IEnumerator<ITask> TestDriveDistanceAndRotateDegrees()
        {
            yield return Arbiter.Choice(
                _mainPort.RotateDegrees(90, 0.2),
                delegate(DefaultUpdateResponseType response) { },
                delegate(Fault f) { LogInfo(LogGroups.Console, "RotateDegrees Fault"); }
            );

            yield return Arbiter.Choice(
                _mainPort.RotateDegrees(-90, 0.2),
                delegate(DefaultUpdateResponseType response) { },
                delegate(Fault f) { LogInfo(LogGroups.Console, "RotateDegrees Fault"); }
            );
        }
    }
}
