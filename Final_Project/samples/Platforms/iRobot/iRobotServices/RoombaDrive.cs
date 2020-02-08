//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: RoombaDrive.cs $ $Revision: 30 $
//-----------------------------------------------------------------------
using Microsoft.Ccr.Core;
using Microsoft.Dss.Core;
using Microsoft.Dss.Core.Attributes;
using Microsoft.Dss.ServiceModel.Dssp;
using Microsoft.Dss.ServiceModel.DsspServiceBase;
using System;
using System.Collections.Generic;
using System.Security.Permissions;
using W3C.Soap;
using Microsoft.Dss.Core.DsspHttp;
using System.ComponentModel;
using System.Collections.Specialized;
using Microsoft.Dss.Core.DsspHttpUtilities;
using System.Net;

using dssphttp = Microsoft.Dss.Core.DsspHttp;
using submgr = Microsoft.Dss.Services.SubscriptionManager;

using drive = Microsoft.Robotics.Services.Drive.Proxy;
using encoder = Microsoft.Robotics.Services.Encoder.Proxy;
using motor = Microsoft.Robotics.Services.Motor.Proxy;

using roomba = Microsoft.Robotics.Services.IRobot.Roomba;
using create = Microsoft.Robotics.Services.IRobot.Create;
using sensorupdates = Microsoft.Robotics.Services.IRobot.SensorUpdates;
using lite = Microsoft.Robotics.Services.IRobot.Lite;

namespace Microsoft.Robotics.Services.IRobot.Roomba.Drive
{

    /// <summary>
    /// Roomba Drive Service
    /// </summary>
    [Contract(Contract.Identifier)]
    [AlternateContract(drive.Contract.Identifier)]
    [DisplayName("(User) iRobot® Generic Drive")]
    [Description("Provides access to an iRobot Roomba or Create's differential motor drive.\n(Uses the Generic Differential Drive contract.)\n(Partner with the 'iRobot® Create / Roomba' service.)")]
    public class DriveService : DsspServiceBase
    {
        /// <summary>
        /// main Port
        /// </summary>
        [ServicePort("/irobot/drive", AllowMultipleInstances = true)]
        private drive.DriveOperations _mainPort = new Microsoft.Robotics.Services.Drive.Proxy.DriveOperations();

        private Port<drive.SetDrivePower> _internalDrivePowerPort = new Port<drive.SetDrivePower>();

        // This is the internal drive port for excuting the drive operations:
        //  driveDistance, and rotateDegrees.
        private PortSet<drive.DriveDistance, drive.RotateDegrees> _internalDriveOperationsPort = new PortSet<drive.DriveDistance, drive.RotateDegrees>();

        // Port used for canceling a driveDistance or RotateDegrees operation.
        private Port<drive.CancelPendingDriveOperation> _internalDriveCancalOperationPort = new Port<drive.CancelPendingDriveOperation>();

        DsspHttpUtilitiesPort _utilities = new DsspHttpUtilitiesPort();

        /// <summary>
        /// The subscription manager which keeps track of subscriptions to our iRobot Drive data
        /// </summary>
        [Partner("SubMgr", Contract = submgr.Contract.Identifier, CreationPolicy = PartnerCreationPolicy.CreateAlways)]
        submgr.SubscriptionManagerPort _subMgrPort = new submgr.SubscriptionManagerPort();

        [Partner("iRobotUpdates", Contract = lite.Contract.Identifier,
            CreationPolicy = PartnerCreationPolicy.UseExisting,
            Optional = false)]
        lite.IRobotLiteOperations _iRobotLitePort = new lite.IRobotLiteOperations();

        [Partner("Create", Contract = create.Contract.Identifier,
            CreationPolicy = PartnerCreationPolicy.UseExisting,
            Optional = true)]
        create.CreateOperations _createPort = new create.CreateOperations();

        lite.IRobotLiteOperations _iRobotSensorUpdates = new lite.IRobotLiteOperations();

        private drive.DriveDifferentialTwoWheelState _state = new drive.DriveDifferentialTwoWheelState();

        // xslt in the resources
        private const string _transform = ServicePaths.EmbeddedResources + "/RoombaServices/RoombaDrive.user.xslt";

        private RoombaDriveState _driveState;

        /// <summary>
        /// Default Service Constructor
        /// </summary>
        public DriveService(DsspServiceCreationPort creationPort) :
                base(creationPort)
        {
        }

        /// <summary>
        /// Service Start
        /// </summary>
        protected override void Start()
        {
            InitState();

            // Listen on the main port for requests and call the appropriate handler.
            Interleave mainInterleave = ActivateDsspOperationHandlers();

            // Subscribe to the iRobot sensors
            _iRobotLitePort.Subscribe(_iRobotSensorUpdates);

            _utilities = DsspHttpUtilitiesService.Create(Environment);

            mainInterleave.CombineWith(new Interleave(
                new ExclusiveReceiverGroup(
                    ),
                new ConcurrentReceiverGroup(
                    Arbiter.Receive<sensorupdates.UpdateAll>(true, _iRobotSensorUpdates, ReturnAllHandler),
                    Arbiter.Receive<sensorupdates.UpdateMode>(true, _iRobotSensorUpdates, UpdateModeHandler),

                    Arbiter.Receive<sensorupdates.UpdateAll>(true, _iRobotSensorUpdates, EmptyHandler),
                    Arbiter.Receive<sensorupdates.UpdateBumpsCliffsAndWalls>(true, _iRobotSensorUpdates, EmptyHandler),
                    Arbiter.Receive<sensorupdates.UpdatePose>(true, _iRobotSensorUpdates, EmptyHandler),
                    Arbiter.Receive<sensorupdates.UpdatePower>(true, _iRobotSensorUpdates, EmptyHandler),
                    Arbiter.Receive<sensorupdates.UpdateMode>(true, _iRobotSensorUpdates, EmptyHandler),
                    Arbiter.Receive<sensorupdates.UpdateCliffDetail>(true, _iRobotSensorUpdates, EmptyHandler),
                    Arbiter.Receive<sensorupdates.UpdateTelemetry>(true, _iRobotSensorUpdates, EmptyHandler),

                    Arbiter.Receive<sensorupdates.UpdateNotifications>(true, _iRobotSensorUpdates, EmptyHandler)
                )));

            // Wait one time for an InternalDrivePower command
            Activate(Arbiter.ReceiveWithIterator<drive.SetDrivePower>(false, _internalDrivePowerPort, InternalDrivePowerHandler));


            // Interleave to manage internal drive operations (driveDistance and RotateDegrees)
            Activate(
                new Interleave(
                    new ExclusiveReceiverGroup(
                    Arbiter.ReceiveWithIteratorFromPortSet<drive.DriveDistance>(true, _internalDriveOperationsPort, InternalDriveDistanceHandler),
                    Arbiter.ReceiveWithIteratorFromPortSet<drive.RotateDegrees>(true, _internalDriveOperationsPort, InternalRotateDegreesHandler)
                    ),
                    new ConcurrentReceiverGroup())
                   );

            // Publish the service to the local service Directory
            DirectoryInsert();

            // display HTTP service Uri
            LogInfo(LogGroups.Console, "Service uri: ");

        }

        #region Request Handlers


        /// <summary>
        /// Get Handler
        /// </summary>
        /// <param name="get"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public virtual IEnumerator<ITask> GetHandler(drive.Get get)
        {
            get.ResponsePort.Post(_state);
            yield break;
        }


        /// <summary>
        /// HttpGet
        /// </summary>
        /// <param name="httpGet"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public virtual IEnumerator<ITask> HttpGetHandler(Microsoft.Dss.Core.DsspHttp.HttpGet httpGet)
        {
            httpGet.ResponsePort.Post(new HttpResponseType(_state));
            yield break;
        }


        /// <summary>
        /// HttpPost Handler
        /// </summary>
        /// <param name="httpPost"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public virtual IEnumerator<ITask> HttpPostHandler(dssphttp.HttpPost httpPost)
        {
            try
            {
                Fault fault = null;
                NameValueCollection collection = null;

                ReadFormData readForm = new ReadFormData(httpPost);
                _utilities.Post(readForm);

                yield return Arbiter.Choice(
                    readForm.ResultPort,
                    delegate(NameValueCollection success)
                    {
                        collection = success;
                    },
                    delegate(Exception e)
                    {
                        fault = Fault.FromException(e);
                        LogError(e);
                    }
                );

                if (fault != null)
                {
                    yield break;
                }


            }
            finally
            {
                httpPost.ResponsePort.Post(new HttpResponseType(
                    HttpStatusCode.OK,
                    _state,
                    _transform
                ));
            }

        }


        /// <summary>
        /// Update Handler
        /// </summary>
        /// <param name="update"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public virtual IEnumerator<ITask> UpdateHandler(drive.Update update)
        {
            _state = update.Body;
            update.ResponsePort.Post(DefaultUpdateResponseType.Instance);
            yield break;
        }

        /// <summary>
        /// Subscribe Handler
        /// </summary>
        /// <param name="subscribe"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public virtual IEnumerator<ITask> SubscribeHandler(drive.Subscribe subscribe)
        {
            SubscribeHelper(_subMgrPort, subscribe.Body, subscribe.ResponsePort);
            yield break;
        }


        /// <summary>
        /// ReliableSubscribe Handler
        /// </summary>
        /// <param name="subscribe"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public virtual IEnumerator<ITask> ReliableSubscribeHandler(drive.ReliableSubscribe subscribe)
        {
            SubscribeHelper(_subMgrPort, subscribe.Body, subscribe.ResponsePort);
            yield break;
        }


        /// <summary>
        /// EnableDrive Handler
        /// </summary>
        /// <param name="update"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public virtual IEnumerator<ITask> EnableDriveHandler(drive.EnableDrive update)
        {
            _state.IsEnabled = update.Body.Enable;
            _state.TimeStamp = DateTime.Now;

            if (_state.IsEnabled == false)
            {
                // check for pending operations
                if (_driveState._internalPendingDriveOperation != drive.DriveRequestOperation.NotSpecified)
                {
                    drive.CancelPendingDriveOperation cancelPendingOp = new drive.CancelPendingDriveOperation();
                    _internalDriveCancalOperationPort.Post(cancelPendingOp);
                    // do not wait for a response from a cancel operation
                }
            }

            SendNotification<drive.Update>(_subMgrPort, _state);
            update.ResponsePort.Post(DefaultUpdateResponseType.Instance);
            yield break;
        }


        /// <summary>
        /// SetDrivePower Handler
        /// </summary>
        /// <param name="update"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public virtual IEnumerator<ITask> SetDrivePowerHandler(drive.SetDrivePower update)
        {
            // Verified drive is enabled and acknowledge request or fault
            if (ValidateDriveEnabledAndRespondHelper(update.ResponsePort))
            {
                _internalDrivePowerPort.Post(update);
            }
            yield break;
        }


        /// <summary>
        /// Process the most recent Drive Power command
        /// When complete, self activate for the next internal command
        /// </summary>
        /// <param name="update"></param>
        /// <returns></returns>
        public virtual IEnumerator<ITask> InternalDrivePowerHandler(drive.SetDrivePower update)
        {
            // Take a snapshot of the number of pending commands at the time
            // we entered this routine.
            // This will prevent a livelock which can occur if we try to
            // process the queue until it is empty, but the inbound queue
            // is growing at the same rate as we are pulling off the queue.
            int pendingCommands = _internalDrivePowerPort.ItemCount;

            // If newer commands have been issued,
            // respond success to the older command and
            // move to the newer one.
            drive.SetDrivePower newerUpdate;
            while (pendingCommands > 0)
            {
                if (_internalDrivePowerPort.Test(out newerUpdate))
                {
                    update.ResponsePort.Post(DefaultUpdateResponseType.Instance);
                    update = newerUpdate;
                }
                pendingCommands--;
            }

            double leftPower = update.Body.LeftWheelPower;
            double rightPower = update.Body.RightWheelPower;

            // check for pending drive operation, and cancel it.
            if (_driveState._internalPendingDriveOperation != drive.DriveRequestOperation.NotSpecified)
            {
                drive.CancelPendingDriveOperation cancelPendingOp = new drive.CancelPendingDriveOperation();
                _internalDriveCancalOperationPort.Post(cancelPendingOp);
                // do not wait for a response from a cancel operation
             }


            PortSet<roomba.RoombaCommandReceived, Fault> roombaResponse = DriveIRobot(leftPower, rightPower);

            yield return Arbiter.Choice(
                Arbiter.Receive<roomba.RoombaCommandReceived>(false, roombaResponse,
                    delegate(roomba.RoombaCommandReceived ok)
                    {
                        update.ResponsePort.Post(DefaultUpdateResponseType.Instance);
                    }),
                Arbiter.Receive<Fault>(false, roombaResponse,
                    delegate(Fault fault)
                    {
                        LogError(fault);
                        update.ResponsePort.Post(fault);
                    }),
                Arbiter.Receive<DateTime>(false, TimeoutPort(3000),
                    delegate(DateTime timeout)
                    {
                        LogError("Timeout sending Drive command");
                    }));

            // Wait one time for the next InternalDrivePower command
            Activate(Arbiter.ReceiveWithIterator(false, _internalDrivePowerPort, InternalDrivePowerHandler));

            yield break;
        }


        /// <summary>
        /// SetDriveSpeed Handler (meters/second)
        /// </summary>
        /// <param name="update"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public virtual IEnumerator<ITask> SetDriveSpeedHandler(drive.SetDriveSpeed update)
        {
            double leftPower = Math.Max(-1.0, Math.Min(1.0, update.Body.LeftWheelSpeed * 2.0));
            double rightPower = Math.Max(-1.0, Math.Min(1.0, update.Body.RightWheelSpeed * 2.0));

            // check for pending drive operation, and cancel it.
            if (_driveState._internalPendingDriveOperation != drive.DriveRequestOperation.NotSpecified)
            {
                drive.CancelPendingDriveOperation cancelPendingOp = new drive.CancelPendingDriveOperation();
                _internalDriveCancalOperationPort.Post(cancelPendingOp);
                // do not wait for a response from a cancel operation
            }

#if DEBUG
            LogVerbose(LogGroups.Console, "Roomba SDP: L:" + update.Body.LeftWheelSpeed.ToString() + " R:" + update.Body.RightWheelSpeed.ToString() + " |");
#endif

            PortSet<DefaultUpdateResponseType, Fault> responsePort = _mainPort.SetDrivePower(new drive.SetDrivePowerRequest(leftPower, rightPower));
            yield return Arbiter.Choice(
                Arbiter.Receive<DefaultUpdateResponseType>(false, responsePort,
                    delegate(DefaultUpdateResponseType rsp)
                    {
                        update.ResponsePort.Post(rsp);
                    }),
                Arbiter.Receive<Fault>(false, responsePort,
                    delegate(Fault fault)
                    {
                        update.ResponsePort.Post(fault);
                    }));

            yield break;
        }

        /// <summary>
        /// AllStop Handler
        /// </summary>
        /// <param name="update"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public virtual IEnumerator<ITask> AllStopHandler(drive.AllStop update)
        {
            if (_driveState.robotModel == roomba.IRobotModel.NotSpecified)
            {
                update.ResponsePort.Post(
                    Fault.FromException(
                        new InvalidOperationException("Unable to process the All Stop command on iRobot " + _driveState.robotModel.ToString())));
                yield break;
            }

            // The drive state is changed now, so any pending drive operations that is about to be
            // started are aborted.
            _state.IsEnabled = false;

            // disable drive.
            drive.EnableDrive disableDrive = new drive.EnableDrive();


            // check for pending operations
            if (_driveState._internalPendingDriveOperation != drive.DriveRequestOperation.NotSpecified)
            {
                drive.CancelPendingDriveOperation cancelPendingOp = new drive.CancelPendingDriveOperation();
                _internalDriveCancalOperationPort.Post(cancelPendingOp);
                // do not wait for a response from a cancel operation
                // proceed to shutdown power to the drive.
            }

            _driveState.driveCommandInProgress = false;

            StandardResponse roombaResponse = _iRobotLitePort.DriveDirect(0, 0);
            yield return Arbiter.Choice(
                Arbiter.Receive<roomba.RoombaCommandReceived>(false, roombaResponse,
                    delegate(roomba.RoombaCommandReceived ok)
                    { update.ResponsePort.Post(DefaultUpdateResponseType.Instance); }),
                Arbiter.Receive<Fault>(false, roombaResponse,
                    delegate(Fault fault) { update.ResponsePort.Post(fault); })
             );

            disableDrive.Body.Enable = false;
            _mainPort.Post(disableDrive);
            SendNotification(_subMgrPort, update);
            yield break;
        }


        /// <summary>
        /// DriveDistance Handler
        /// </summary>
        /// <param name="driveUpdate"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public IEnumerator<ITask> DriveDistanceHandler(drive.DriveDistance driveUpdate)
        {
            // Verified drive is enabled and acknowledge request or fault
            if (ValidateDriveEnabledAndRespondHelper(driveUpdate.ResponsePort))
            {

                // check for pending drive operations
                if (_driveState._internalPendingDriveOperation != drive.DriveRequestOperation.NotSpecified)
                {
                    // cancel pending drive operation
                    drive.CancelPendingDriveOperation cancelPendingOp = new drive.CancelPendingDriveOperation();
                    cancelPendingOp.TimeSpan = TimeSpan.FromMilliseconds(500);
                    _internalDriveCancalOperationPort.Post(cancelPendingOp);

                    // wait for cancel operation
                    yield return cancelPendingOp.ResponsePort.Choice();
                }
                _internalDriveOperationsPort.Post(driveUpdate);
            }
            yield break;
        }

        /// <summary>
        /// RotateDegrees Handler
        /// </summary>
        /// <param name="driveUpdate"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public virtual IEnumerator<ITask> RotateDegreesHandler(drive.RotateDegrees driveUpdate)
        {

            // Verified drive is enabled and acknowledge request or fault
            if (ValidateDriveEnabledAndRespondHelper(driveUpdate.ResponsePort))
            {
                // check for pending operations
                if (_driveState._internalPendingDriveOperation != drive.DriveRequestOperation.NotSpecified)
                {
                    // cancel pending drive operation
                    drive.CancelPendingDriveOperation cancelPendingOp = new drive.CancelPendingDriveOperation();
                    cancelPendingOp.TimeSpan = TimeSpan.FromMilliseconds(500);
                    _internalDriveCancalOperationPort.Post(cancelPendingOp);

                    // wait for cancel operation
                    yield return cancelPendingOp.ResponsePort.Choice();
                }
                _internalDriveOperationsPort.Post(driveUpdate);
            }
            
            yield break;
        }

        /// <summary>
        /// Resets the encoder tick count to zero on both wheels.
        /// Not implemented in this service.
        /// </summary>
        /// <param name="reset">Request message</param>
        [ServiceHandler]
        public void EncoderResetHandler(drive.ResetEncoders reset)
        {
            throw new NotImplementedException();
        }

        private bool ValidateDriveEnabledAndRespondHelper(PortSet<DefaultUpdateResponseType, Fault> responsePort)
        {
            Fault fault = null;

            // Acknowledge request or fault
            if (_state.IsEnabled == false)
            {
                fault = Fault.FromException(new InvalidOperationException("Attempting to process a drive operation, but the differential drive is not enabled"));
                responsePort.Post(fault);
            }
            
            return _state.IsEnabled;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Initialize Roomba State
        /// </summary>
        private void InitState()
        {
            if (_state == null)
            {
                _state = new drive.DriveDifferentialTwoWheelState();
            }

            if (_state.DistanceBetweenWheels <= 0)
                _state.DistanceBetweenWheels = IRobot.Roomba.Contract.iRobotWheelBase;

            if (_state.LeftWheel == null)
                _state.LeftWheel = new motor.WheeledMotorState();
            if (_state.LeftWheel.MotorState == null)
                _state.LeftWheel.MotorState = new motor.MotorState();
            if (_state.LeftWheel.MotorState.HardwareIdentifier == 0)
                _state.LeftWheel.MotorState.HardwareIdentifier = 1;
            if (string.IsNullOrEmpty(_state.LeftWheel.MotorState.Name))
                _state.LeftWheel.MotorState.Name = "Left Motor";
            if (_state.LeftWheel.MotorState.PowerScalingFactor == 0.0)
                _state.LeftWheel.MotorState.PowerScalingFactor = 1.0;


            if (_state.RightWheel == null)
                _state.RightWheel = new motor.WheeledMotorState();
            if (_state.RightWheel.MotorState == null)
                _state.RightWheel.MotorState = new motor.MotorState();
            if (_state.RightWheel.MotorState.HardwareIdentifier == 0)
                _state.RightWheel.MotorState.HardwareIdentifier = 2;
            if (string.IsNullOrEmpty(_state.RightWheel.MotorState.Name))
                _state.RightWheel.MotorState.Name = "Right Motor";
            if (_state.RightWheel.MotorState.PowerScalingFactor == 0.0)
                _state.RightWheel.MotorState.PowerScalingFactor = 1.0;

            if (_state.LeftWheel.EncoderState == null)
                _state.LeftWheel.EncoderState = new encoder.EncoderState();

            if (_state.RightWheel.EncoderState == null)
                _state.RightWheel.EncoderState = new encoder.EncoderState();

            _state.IsEnabled = true;
            _state.TimeStamp = DateTime.Now;

            _driveState._internalPendingDriveOperation = drive.DriveRequestOperation.NotSpecified;

            SaveState(_state);
        }

        /// <summary>
        /// Drive the iRobot
        /// </summary>
        /// <param name="leftPower"></param>
        /// <param name="rightPower"></param>
        /// <returns></returns>
        private PortSet<roomba.RoombaCommandReceived, Fault> DriveIRobot(double leftPower, double rightPower)
        {
            return _iRobotLitePort.DriveDirect((int)(rightPower * 500.0), (int)(leftPower * 500.0));
        }


        #endregion

        #region Roomba Sensors Notifications Handlers

        /// <summary>
        /// Handle Update Mode Notifications
        /// </summary>
        /// <param name="updateMode"></param>
        public void UpdateModeHandler(sensorupdates.UpdateMode updateMode)
        {
            if ((updateMode.Body.IRobotModel != roomba.IRobotModel.NotSpecified)
                && (_driveState.robotModel != updateMode.Body.IRobotModel))
            {
                _driveState.robotModel = updateMode.Body.IRobotModel;
            }
        }

        /// <summary>
        /// Handle Return All Notifications
        /// </summary>
        /// <param name="notify"></param>
        public void ReturnAllHandler(sensorupdates.UpdateAll notify)
        {
            if (notify.Body.Sensors.Wall)
                LogVerbose(LogGroups.Console, "*** Wall! ***");

            if ((notify.Body.Sensors.BumpsWheeldrops & roomba.BumpsWheeldrops.BumpLeft) == roomba.BumpsWheeldrops.BumpLeft)
                LogVerbose(LogGroups.Console, "*** Bump Left! ***");

            if ((notify.Body.Sensors.BumpsWheeldrops & roomba.BumpsWheeldrops.BumpRight) == roomba.BumpsWheeldrops.BumpRight)
                LogVerbose(LogGroups.Console, "*** Bump Right! ***");

            if ((notify.Body.Sensors.BumpsWheeldrops & roomba.BumpsWheeldrops.WheelDropCaster) == roomba.BumpsWheeldrops.WheelDropCaster)
                LogVerbose(LogGroups.Console, "*** Wheel Drop Caster! ***");

            if ((notify.Body.Sensors.BumpsWheeldrops & roomba.BumpsWheeldrops.WheelDropLeft) == roomba.BumpsWheeldrops.WheelDropLeft)
                LogVerbose(LogGroups.Console, "*** Wheel Drop Left! ***");

            if ((notify.Body.Sensors.BumpsWheeldrops & roomba.BumpsWheeldrops.WheelDropRight) == roomba.BumpsWheeldrops.WheelDropRight)
                LogVerbose(LogGroups.Console, "*** Wheel Drop Right! ***");

            if ((notify.Body.Pose.ButtonsRoomba & roomba.ButtonsRoomba.Max) == roomba.ButtonsRoomba.Max)
                LogVerbose(LogGroups.Console, "*** Max button pressed! ***");

            if (notify.Body.Sensors.CliffFrontLeft)
                LogVerbose(LogGroups.Console, "*** Cliff Front Left! ***");
            if (notify.Body.Sensors.CliffFrontRight)
                LogVerbose(LogGroups.Console, "*** Cliff Front Right! ***");
            if (notify.Body.Sensors.CliffLeft)
                LogVerbose(LogGroups.Console, "*** Cliff Left! ***");
            if (notify.Body.Sensors.CliffRight)
                LogVerbose(LogGroups.Console, "*** Cliff Right! ***");

            if ((notify.Body.Pose.ButtonsRoomba & roomba.ButtonsRoomba.Power) == roomba.ButtonsRoomba.Power)
            {
                LogVerbose(LogGroups.Console, "*** Power button pressed! ***");
            }

            if ((notify.Body.Pose.ButtonsRoomba & roomba.ButtonsRoomba.Clean) == roomba.ButtonsRoomba.Clean)
                LogVerbose(LogGroups.Console, "*** Clean button pressed! ***");

            if ((notify.Body.Pose.ButtonsRoomba & roomba.ButtonsRoomba.Spot) == roomba.ButtonsRoomba.Spot)
                LogVerbose(LogGroups.Console, "*** Spot button pressed! ***");

        }

        #endregion

        #region Build and Play Create Scripts

        /// <summary>
        /// Create and Play a Script
        /// </summary>
        /// <param name="cmdList">The list of scripted commands</param>
        /// <param name="responsePort">The Response port</param>
        /// <param name="successResponse">The Success Response</param>
        /// <returns></returns>
        private IEnumerator<ITask> CreateAndPlayScript(List<roomba.RoombaCommand> cmdList, IPortSet responsePort, object successResponse)
        {
            bool maintainMode = false;
            RoombaMode startingMode = RoombaMode.NotSpecified;

            bool abort = false;
            create.CmdDefineScript cmdscript = new create.CmdDefineScript(cmdList);
            PortSet<create.ReturnDefineScript, Fault> defineResponsePort = _createPort.CreateDefineScript(cmdscript);
            yield return Arbiter.Choice(defineResponsePort,
                delegate(create.ReturnDefineScript success)
                {
                    if (success.MaintainMode == RoombaMode.Passive || success.MaintainMode == RoombaMode.Safe || success.MaintainMode == RoombaMode.Full)
                    {
                        maintainMode = true;
                        startingMode = success.MaintainMode;
                    }
                    else
                    {
                        startingMode = success.RoombaMode;
                    }
                },
                delegate(Fault fault)
                {
                    LogError(fault);
                    responsePort.PostUnknownType(fault);
                    abort = true;
                });

            if (abort)
                yield break;

            PortSet<RoombaCommandReceived,Fault> playResponsePort = _createPort.CreatePlayScript(new create.CmdPlayScript(cmdscript.ScriptResponseBytes, 3000));
            yield return Arbiter.Choice(playResponsePort,
                delegate(roomba.RoombaCommandReceived success) { },
                delegate(Fault fault)
                {
                    LogError(fault);
                    responsePort.PostUnknownType(fault);
                    abort = true;
                });

            if (abort)
            {
                // Abort means the script was cancelled.  The Create may still be moving.
                // Stop the wheels!
                LogVerbose(LogGroups.Console, "Drive Script was cancelled.  Stopping wheels.");
                yield return Arbiter.Choice(_createPort.CreateDriveDirect(new create.CmdDriveDirect(0, 0)),
                    delegate(roomba.RoombaCommandReceived success) { },
                    delegate(Fault fault) { });


                // Now reset the mode to our starting mode.
                CmdSetMode cmdSetMode = new CmdSetMode(startingMode, maintainMode);
                yield return Arbiter.Choice(_iRobotLitePort.SetMode(cmdSetMode),
                    delegate(roomba.RoombaCommandReceived success) { },
                    delegate(Fault fault) { });

                yield break;
            }

            responsePort.PostUnknownType(successResponse);
            yield break;
        }

        #endregion

        #region Internal Drive Handlers
        /// <summary>
        /// Internal drive distance operation handler
        /// </summary>
        /// <param name="driveDistance"></param>
        /// <returns></returns>
        public virtual IEnumerator<ITask> InternalDriveDistanceHandler(drive.DriveDistance driveDistance)
        {
            switch (driveDistance.Body.DriveDistanceStage)
            {
                case drive.DriveStage.InitialRequest:
                    _driveState._internalPendingDriveOperation = drive.DriveRequestOperation.DriveDistance;
                    SpawnIterator<drive.DriveDistance>(driveDistance, InternalDriveDistanceImpl);
                    break;

                case drive.DriveStage.Started:
                    SendNotification<drive.DriveDistance>(_subMgrPort, driveDistance.Body);
                    break;

                case drive.DriveStage.Completed:
                    _driveState._internalPendingDriveOperation = drive.DriveRequestOperation.NotSpecified;
                    SendNotification<drive.DriveDistance>(_subMgrPort, driveDistance.Body);
                    break;

                case drive.DriveStage.Canceled:
                    _driveState._internalPendingDriveOperation = drive.DriveRequestOperation.NotSpecified;
                    SendNotification<drive.DriveDistance>(_subMgrPort, driveDistance.Body);
                    break;

            }

            yield break;
        }

        /// <summary>
        /// Internal rotate degrees handler
        /// </summary>
        /// <param name="rotateDegrees"></param>
        /// <returns></returns>
        public virtual IEnumerator<ITask> InternalRotateDegreesHandler(drive.RotateDegrees rotateDegrees)
        {
            switch (rotateDegrees.Body.RotateDegreesStage)
            {
                case drive.DriveStage.InitialRequest:
                    _driveState._internalPendingDriveOperation = drive.DriveRequestOperation.RotateDegrees;
                    SpawnIterator<drive.RotateDegrees>(rotateDegrees, InternalRotateDegreesImpl);
                    break;

                case drive.DriveStage.Started:
                    SendNotification<drive.RotateDegrees>(_subMgrPort, rotateDegrees.Body);
                    break;

                case drive.DriveStage.Completed:
                    _driveState._internalPendingDriveOperation = drive.DriveRequestOperation.NotSpecified;
                    SendNotification<drive.RotateDegrees>(_subMgrPort, rotateDegrees.Body);
                    break;

                case drive.DriveStage.Canceled:
                    _driveState._internalPendingDriveOperation = drive.DriveRequestOperation.NotSpecified;
                    SendNotification<drive.RotateDegrees>(_subMgrPort, rotateDegrees.Body);
                    break;
            }

            yield break;
        }

        /// <summary>
        /// Internal DriveDistance Implementation
        /// </summary>
        /// <param name="update"></param>
        /// <returns></returns>
        public virtual IEnumerator<ITask> InternalDriveDistanceImpl(drive.DriveDistance update)
        {
            Fault saveFault = null;
            drive.CancelPendingDriveOperation _ic = null;

            // ****************************************************************************
            // The iRobot Create Script is inherently unstable because if a wait
            // condition is not fulfilled, there is NO WAY to regain control of the robot.

            bool useCreateScript = (_createPort != null)
                && (_driveState.robotModel == roomba.IRobotModel.Create);

            // until this issue is resolved, we will not use it.
            useCreateScript = false;
            // ****************************************************************************

            if (_driveState.robotModel == roomba.IRobotModel.NotSpecified)
                throw new InvalidOperationException("DriveDistance is invalid for iRobot Model: " + _driveState.robotModel.ToString() + ".");
            

            int power = (int)(update.Body.Power * 500.0);

            // iRobot measures distance in mm.
            int distance = (int)(update.Body.Distance * 1000.0);

            // To drive the Create & Roomba backwards, it velocity vector must be negative.
            // (the values for both the power and distance parameters must be negative).
            if (power != 0 && Math.Sign(power) < 0)
            {
                distance = (distance != 0 && Math.Sign(distance) > 0) ? distance * (-1) : distance;
            }

            _driveState.pendingDriveOperation = drive.DriveRequestOperation.DriveDistance;
            // notify subscribers of drive distance start
            update.Body.DriveDistanceStage = drive.DriveStage.Started;
            _internalDriveOperationsPort.Post(update);

            if (useCreateScript)
            {
                List<roomba.RoombaCommand> cmdList = new List<roomba.RoombaCommand>();
                cmdList.Add(new create.CmdDriveDirect(power, power));
                cmdList.Add(new create.CmdWaitDistance(distance));
                cmdList.Add(new create.CmdDriveDirect(0, 0));
                cmdList.Add(new roomba.CmdSensors(create.CreateSensorPacket.AllRoomba));

                // ***************************************
                // Do not place any code below this line
                // ***************************************
                SpawnIterator<List<roomba.RoombaCommand>, IPortSet, object>(
                    cmdList,                                // The list of commands
                    update.ResponsePort,                    // where to send the response
                    DefaultUpdateResponseType.Instance,     // a successful response
                    CreateAndPlayScript);                   // Create and play this script.

                // CreateAndPlayScript is an asyncronous iterator and
                // will respond to the current command when it is finished.

                yield break;
                // ***************************************
            }
            // Start driving, watch sensors until we have driven the proper distance.
            bool abort = false;
            int startingDistance = 0;


            // confirm that drive is still enabled.
            if (_state.IsEnabled != true)
            {
                // if drive is not enabled, cancel operation.
                update.Body.DriveDistanceStage = drive.DriveStage.Canceled;
                _internalDriveOperationsPort.Post(update);
                yield break;
            }

            GetSensorsResponse getSensorsResponse = _iRobotLitePort.GetSensors(new CmdSensors(RoombaCommandCode.ReturnPose));
            yield return Arbiter.Choice(
                Arbiter.Receive<ReturnPose>(false, getSensorsResponse,
                    delegate(ReturnPose returnPose)
                    {
                        startingDistance = returnPose.Distance;
                    }),
                Arbiter.Receive<Fault>(false, getSensorsResponse,
                    delegate(Fault fault)
                    {
                        saveFault = fault;
                        abort = true;
                    }),
                Arbiter.Receive<drive.CancelPendingDriveOperation>(false, _internalDriveCancalOperationPort,
                   delegate(drive.CancelPendingDriveOperation ic) { abort = true; _ic = ic; })
             );

            if (abort)
            {
                update.Body.DriveDistanceStage = drive.DriveStage.Canceled;
                _internalDriveOperationsPort.Post(update);

                yield break;
            }

            // Start driving

            StandardResponse response = _iRobotLitePort.DriveDirect(power, power);
            yield return Arbiter.Choice(
                Arbiter.Receive<RoombaCommandReceived>(false, response,
                    delegate(RoombaCommandReceived ok) { }),
                Arbiter.Receive<Fault>(false, getSensorsResponse,
                    delegate(Fault fault)
                    {
                        saveFault = fault;
                        abort = true;
                    }),
                Arbiter.Receive<drive.CancelPendingDriveOperation>(false, _internalDriveCancalOperationPort,
                    delegate(drive.CancelPendingDriveOperation ic) { abort = true; _ic = ic; })
            );

            if (!abort)
            {
                // Wait until we have driven for the proper amount of time.
                // ms = (distance) / (mm/sec * 1000)
                int ms = (distance * 1000 / power);
               
                yield return Arbiter.Choice(
                    Arbiter.Receive<DateTime>(false, TimeoutPort(ms), delegate(DateTime done) { }),
                    Arbiter.Receive<drive.CancelPendingDriveOperation>(false, _internalDriveCancalOperationPort, delegate(drive.CancelPendingDriveOperation ic) { abort = true; _ic = ic; })
                    );
            }

            // stop
            yield return Arbiter.Choice(_iRobotLitePort.DriveDirect(0, 0),
                delegate(RoombaCommandReceived ok) { },
                delegate(Fault fault) { });

            if (abort)
            {
                _ic.ResponsePort.Post(DefaultUpdateResponseType.Instance);
                update.Body.DriveDistanceStage = drive.DriveStage.Canceled;
                _internalDriveOperationsPort.Post(update);

                yield break;
            }

            int result = 0;
            getSensorsResponse = _iRobotLitePort.GetSensors(new CmdSensors(RoombaCommandCode.ReturnPose));
            yield return Arbiter.Choice(
                Arbiter.Receive<ReturnPose>(false, getSensorsResponse,
                    delegate(ReturnPose returnPose) { result = startingDistance - returnPose.Distance; }),
                Arbiter.Receive<Fault>(false, getSensorsResponse, delegate(Fault fault) { abort = true; })
            );

            LogInfo(LogGroups.Console, string.Format("Completed DriveDistance {0} mm", result));

            // notify subscribers of drive distance complete
            update.Body.DriveDistanceStage = drive.DriveStage.Completed;
            _internalDriveOperationsPort.Post(update);

            yield break;
        }


        /// <summary>
        /// Internal RotateDegrees Handler
        /// </summary>
        /// <param name="update"></param>
        /// <returns></returns>
        public virtual IEnumerator<ITask> InternalRotateDegreesImpl(drive.RotateDegrees update)
        {
            Fault saveFault = null;
            drive.CancelPendingDriveOperation _ic = null;

            // ****************************************************************************
            // The iRobot Create Script is inherently unstable because if a wait
            // condition is not fulfilled, there is NO WAY to regain control of the robot.

            bool useCreateScript = (_createPort != null)
                && (_driveState.robotModel == roomba.IRobotModel.Create);

            // until this issue is resolved, we will not use it.
            useCreateScript = false;
            // ****************************************************************************

            if (_driveState.robotModel == roomba.IRobotModel.NotSpecified)
                throw new InvalidOperationException("RotateDegrees is invalid for iRobot Model: " + _driveState.robotModel.ToString() + ".");

            _driveState.pendingDriveOperation = drive.DriveRequestOperation.RotateDegrees;

            int power = (int)(Math.Abs(update.Body.Power) * 500.0);
            int degrees = (int)update.Body.Degrees;
            if (degrees < 0)
                power *= -1;

            // notify subscribers of drive distance start
            update.Body.RotateDegreesStage = drive.DriveStage.Started;
            _internalDriveOperationsPort.Post(update);

            if (useCreateScript)
            {
                List<roomba.RoombaCommand> cmdList = new List<roomba.RoombaCommand>();
                cmdList.Add(new create.CmdDriveDirect(power, -power));
                cmdList.Add(new create.CmdWaitAngle(degrees));
                cmdList.Add(new create.CmdDriveDirect(0, 0));
                cmdList.Add(new roomba.CmdSensors(create.CreateSensorPacket.AllRoomba));

                // ***************************************
                // Do not place any code below this line
                // ***************************************
                SpawnIterator<List<roomba.RoombaCommand>, IPortSet, object>(
                    cmdList,                                // The list of commands
                    update.ResponsePort,                    // where to send the response
                    DefaultUpdateResponseType.Instance,     // a successful response
                    CreateAndPlayScript);                   // Create and play this script.

                // CreateAndPlayScript is an asyncronous iterator and
                // will respond to the current command when it is finished.

                yield break;
                // ***************************************
            }

            bool abort = false;
            int startingAngle = 0;
            GetSensorsResponse getSensorsResponse = _iRobotLitePort.GetSensors(new CmdSensors(RoombaCommandCode.ReturnPose));
            yield return Arbiter.Choice(
                Arbiter.Receive<ReturnPose>(false, getSensorsResponse,
                    delegate(ReturnPose returnPose)
                    {
                        startingAngle = returnPose.Angle;
                    }),
                Arbiter.Receive<Fault>(false, getSensorsResponse,
                    delegate(Fault fault)
                    {
                        saveFault = fault;
                        abort = true;
                    }),
                Arbiter.Receive<drive.CancelPendingDriveOperation>(false, _internalDriveCancalOperationPort, 
                                delegate(drive.CancelPendingDriveOperation ic) { abort = true; _ic = ic; })    
            );

            if (abort)
            {
                update.Body.RotateDegreesStage = drive.DriveStage.Canceled;
                _internalDriveOperationsPort.Post(update);
                yield break;
            }

            // Start driving

            StandardResponse response = _iRobotLitePort.DriveDirect(power, -power);
            yield return Arbiter.Choice(
                Arbiter.Receive<RoombaCommandReceived>(false, response,
                    delegate(RoombaCommandReceived ok) { }),
                Arbiter.Receive<Fault>(false, getSensorsResponse,
                    delegate(Fault fault)
                    {
                        saveFault = fault;
                        abort = true;
                    }),
                Arbiter.Receive<drive.CancelPendingDriveOperation>(false, _internalDriveCancalOperationPort,
                    delegate(drive.CancelPendingDriveOperation ic) { abort = true; _ic = ic; })
                    );

            if (!abort)
            {
                // Wait until we have rotated for the proper amount of time.
                // ms = (circumference mm) * (degrees / 360) / (mm/sec * 1000)
                int ms = (int)((Math.PI * roomba.Contract.iRobotWheelBase)
                    * (update.Body.Degrees / 360.0)
                    * 1000.0 / (double)power);

                yield return Arbiter.Choice(
                    Arbiter.Receive<DateTime>(false, TimeoutPort(ms), 
                                delegate(DateTime done) { }),
                    Arbiter.Receive<drive.CancelPendingDriveOperation>(false, _internalDriveCancalOperationPort, 
                                delegate(drive.CancelPendingDriveOperation ic) { abort = true; _ic = ic; })
                    );
            }

            yield return Arbiter.Choice(_iRobotLitePort.DriveDirect(0, 0),
                delegate(RoombaCommandReceived ok) { },
                delegate(Fault fault) { });

            if (abort)
            {
                _ic.ResponsePort.Post(DefaultUpdateResponseType.Instance);
                update.Body.RotateDegreesStage = drive.DriveStage.Canceled;
                _internalDriveOperationsPort.Post(update);

                yield break;
            }

            int result = 0;
            getSensorsResponse = _iRobotLitePort.GetSensors(new CmdSensors(RoombaCommandCode.ReturnPose));
            yield return Arbiter.Choice(
                Arbiter.Receive<ReturnPose>(false, getSensorsResponse,
                    delegate(ReturnPose returnPose)
                    {
                        result = startingAngle - returnPose.Angle;
                    }),
                Arbiter.Receive<Fault>(false, getSensorsResponse, delegate(Fault fault) { abort = true; }));

            LogInfo(LogGroups.Console, string.Format("Completed rotation {0} degrees", result));

            // notify subscribers of drive distance start
            update.Body.RotateDegreesStage = drive.DriveStage.Completed;
            _internalDriveOperationsPort.Post(update);

            yield break;
        }
        #endregion

    }
}
