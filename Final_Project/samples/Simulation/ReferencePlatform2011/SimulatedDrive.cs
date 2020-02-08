//------------------------------------------------------------------------------
//  <copyright file="SimulatedDrive.cs" company="Microsoft Corporation">
//      Copyright (C) Microsoft Corporation.  All rights reserved.
//  </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Robotics.Services.Simulation.ReferencePlatform2011
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Ccr.Core;
    using Microsoft.Dss.Core.Attributes;
    using Microsoft.Dss.Core.DsspHttp;
    using Microsoft.Dss.ServiceModel.Dssp;
    using Microsoft.Robotics.Simulation.Engine;
    using W3C.Soap;
    using diffdrive = Microsoft.Robotics.Services.Drive;
    using encoder = Microsoft.Robotics.Services.Encoder;
    using engine = Microsoft.Robotics.Simulation.Engine;
    using motor = Microsoft.Robotics.Services.Motor;
    using submgr = Microsoft.Dss.Services.SubscriptionManager;

    #region Reference Platform
    /// <summary>
    /// Reference platform service
    /// </summary>
    public partial class ReferencePlatform2011Service
    {
        /// <summary>
        /// The constant string used for specifying the alternate service port 
        /// </summary>
        private const string DrivePortName = "drivePort";

        /// <summary>
        /// The Differential Drive Entity
        /// </summary>
        private ReferencePlatform2011Entity driveEntity;

        /// <summary>
        /// The subscription manager
        /// </summary>
        [SubscriptionManagerPartner("SubMgr")]
        private submgr.SubscriptionManagerPort subMgrPort = new submgr.SubscriptionManagerPort();

        /// <summary>
        /// The drive operation port
        /// </summary>
        [AlternateServicePort("/SimulatedDifferentialDrive", AllowMultipleInstances = true,
            AlternateContract = diffdrive.Contract.Identifier)]
        private diffdrive.DriveOperations drivePort = new diffdrive.DriveOperations();

        /// <summary>
        /// The encoder value of the left wheel at last reset
        /// </summary>
        private int lastResetLeftWheelEncoderValue;

        /// <summary>
        /// The encoder value of the right wheel at last reset
        /// </summary>
        private int lastResetRightWheelEncoderValue;

        /// <summary>
        /// Get handler retrieves service state
        /// </summary>
        /// <param name="get">The Get request</param>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent, PortFieldName = DrivePortName)]
        public void DriveHttpGetHandler(HttpGet get)
        {
            this.UpdateStateFromSimulation();
            get.ResponsePort.Post(new HttpResponseType(this.state.DriveState));
        }

        /// <summary>
        /// Get handler retrieves service state
        /// </summary>
        /// <param name="get">The Get request</param>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent, PortFieldName = DrivePortName)]
        public void DriveGetHandler(diffdrive.Get get)
        {
            this.UpdateStateFromSimulation();
            get.ResponsePort.Post(this.state.DriveState);
        }

        #region Subscribe Handling
        /// <summary>
        /// Subscribe to Differential Drive service
        /// </summary>
        /// <param name="subscribe">The subscribe request</param>
        /// <returns>Standard ccr iterator.</returns>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent, PortFieldName = DrivePortName)]
        public IEnumerator<ITask> DriveSubscribeHandler(diffdrive.Subscribe subscribe)
        {
            yield return Arbiter.Choice(
                SubscribeHelper(this.subMgrPort, subscribe.Body, subscribe.ResponsePort),
                success =>
                this.subMgrPort.Post(
                    new submgr.Submit(subscribe.Body.Subscriber, DsspActions.UpdateRequest, this.state.DriveState, null)),
                LogError);
        }

        /// <summary>
        /// Subscribe to Differential Drive service
        /// </summary>
        /// <param name="subscribe">The subscribe request</param>
        /// <returns>Standard ccr iterator.</returns>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent, PortFieldName = DrivePortName)]
        public IEnumerator<ITask> DriveReliableSubscribeHandler(diffdrive.ReliableSubscribe subscribe)
        {
            yield return Arbiter.Choice(
                SubscribeHelper(this.subMgrPort, subscribe.Body, subscribe.ResponsePort),
                success =>
                this.subMgrPort.Post(
                    new submgr.Submit(subscribe.Body.Subscriber, DsspActions.UpdateRequest, this.state.DriveState, null)),
                LogError);
        }
        #endregion

        /// <summary>
        /// ResetsEncoders handler.
        /// </summary>
        /// <param name="resetEncoders">The reset encoders request</param>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive, PortFieldName = DrivePortName)]
        public void ResetEncodersHandler(diffdrive.ResetEncoders resetEncoders)
        {
            this.lastResetLeftWheelEncoderValue += this.state.DriveState.LeftWheel.EncoderState.CurrentReading;
            this.lastResetRightWheelEncoderValue += this.state.DriveState.RightWheel.EncoderState.CurrentReading;
            resetEncoders.ResponsePort.Post(DefaultUpdateResponseType.Instance);
        }

        /// <summary>
        /// Handler for drive request
        /// </summary>
        /// <param name="driveDistance">The DriveDistance request</param>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive, PortFieldName = DrivePortName)]
        public void DriveDistanceHandler(diffdrive.DriveDistance driveDistance)
        {
            if (this.driveEntity == null)
            {
                throw new InvalidOperationException("Simulation entity not registered with service");
            }

            if (!this.state.DriveState.IsEnabled)
            {
                driveDistance.ResponsePort.Post(Fault.FromException(new Exception("Drive is not enabled.")));
                LogError("DriveDistance request to disabled drive.");
                return;
            }

            if ((driveDistance.Body.Power > 1.0f) || (driveDistance.Body.Power < -1.0f))
            {
                // invalid drive power
                driveDistance.ResponsePort.Post(Fault.FromException(new Exception("Invalid Power parameter.")));
                LogError("Invalid Power parameter in DriveDistanceHandler."); 
                return;
            }

            this.state.DriveState.DriveDistanceStage = driveDistance.Body.DriveDistanceStage;
            if (driveDistance.Body.DriveDistanceStage == diffdrive.DriveStage.InitialRequest)
            {
                var entityResponse = new Port<engine.OperationResult>();
                Activate(
                    Arbiter.Receive(
                        false,
                        entityResponse,
                        result =>
                            {
                                // post a message to ourselves indicating that the drive distance has completed
                                var req = new diffdrive.DriveDistanceRequest(0, 0);
                                switch (result)
                                {
                                    case engine.OperationResult.Error:
                                        req.DriveDistanceStage = diffdrive.DriveStage.Canceled;
                                        break;
                                    case engine.OperationResult.Canceled:
                                        req.DriveDistanceStage = diffdrive.DriveStage.Canceled;
                                        break;
                                    case engine.OperationResult.Completed:
                                        req.DriveDistanceStage = diffdrive.DriveStage.Completed;
                                        break;
                                }

                                this.drivePort.Post(new diffdrive.DriveDistance(req));
                            }));

                this.driveEntity.DriveDistance(
                    (float)driveDistance.Body.Distance, (float)driveDistance.Body.Power, entityResponse);

                var req2 = new diffdrive.DriveDistanceRequest(0, 0)
                    { DriveDistanceStage = diffdrive.DriveStage.Started };
                this.drivePort.Post(new diffdrive.DriveDistance(req2));
            }
            else
            {
                SendNotification(this.subMgrPort, driveDistance);
            }

            driveDistance.ResponsePort.Post(DefaultUpdateResponseType.Instance);
        }

        /// <summary>
        /// Handler for rotate request
        /// </summary>
        /// <param name="rotate">The RotateDegrees request</param>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive, PortFieldName = DrivePortName)]
        public void DriveRotateHandler(diffdrive.RotateDegrees rotate)
        {
            if (this.driveEntity == null)
            {
                throw new InvalidOperationException("Simulation entity not registered with service");
            }

            if (!this.state.DriveState.IsEnabled)
            {
                rotate.ResponsePort.Post(Fault.FromException(new Exception("Drive is not enabled.")));
                LogError("RotateDegrees request to disabled drive.");
                return;
            }

            this.state.DriveState.RotateDegreesStage = rotate.Body.RotateDegreesStage;
            if (rotate.Body.RotateDegreesStage == diffdrive.DriveStage.InitialRequest)
            {
                var entityResponse = new Port<engine.OperationResult>();
                Activate(
                    Arbiter.Receive(
                        false,
                        entityResponse,
                        result =>
                            {
                                // post a message to ourselves indicating that the drive distance has completed
                                var req = new diffdrive.RotateDegreesRequest(0, 0);
                                switch (result)
                                {
                                    case engine.OperationResult.Error:
                                        req.RotateDegreesStage = diffdrive.DriveStage.Canceled;
                                        break;
                                    case engine.OperationResult.Canceled:
                                        req.RotateDegreesStage = diffdrive.DriveStage.Canceled;
                                        break;
                                    case engine.OperationResult.Completed:
                                        req.RotateDegreesStage = diffdrive.DriveStage.Completed;
                                        break;
                                }

                                this.drivePort.Post(new diffdrive.RotateDegrees(req));
                            }));

                this.driveEntity.RotateDegrees((float)rotate.Body.Degrees, (float)rotate.Body.Power, entityResponse);

                var req2 = new diffdrive.RotateDegreesRequest(0, 0)
                    { RotateDegreesStage = diffdrive.DriveStage.Started };
                this.drivePort.Post(new diffdrive.RotateDegrees(req2));
            }
            else
            {
                SendNotification(this.subMgrPort, rotate);
            }

            rotate.ResponsePort.Post(DefaultUpdateResponseType.Instance);
        }

        /// <summary>
        /// Handler for setting the drive power
        /// </summary>
        /// <param name="setPower">The SetDrivePower request</param>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive, PortFieldName = DrivePortName)]
        public void DriveSetPowerHandler(diffdrive.SetDrivePower setPower)
        {
            if (this.driveEntity == null)
            {
                throw new InvalidOperationException("Simulation entity not registered with service");
            }

            if (!this.state.DriveState.IsEnabled)
            {
                setPower.ResponsePort.Post(Fault.FromException(new Exception("Drive is not enabled.")));
                LogError("SetPower request to disabled drive.");
                return;
            }

            if ((setPower.Body.LeftWheelPower > 1.0f) || (setPower.Body.LeftWheelPower < -1.0f) ||
                (setPower.Body.RightWheelPower > 1.0f) || (setPower.Body.RightWheelPower < -1.0f))
            {
                // invalid drive power
                setPower.ResponsePort.Post(Fault.FromException(new Exception("Invalid Power parameter.")));
                LogError("Invalid Power parameter in SetPowerHandler.");
                return;
            }

            // Call simulation entity method for setting wheel torque
            this.driveEntity.SetMotorTorque((float)setPower.Body.LeftWheelPower, (float)setPower.Body.RightWheelPower);

            this.UpdateStateFromSimulation();
            setPower.ResponsePort.Post(DefaultUpdateResponseType.Instance);

            // send update notification for entire state
            this.subMgrPort.Post(new submgr.Submit(this.state.DriveState, DsspActions.UpdateRequest));
        }

        /// <summary>
        /// Handler for setting the drive speed
        /// </summary>
        /// <param name="setSpeed">The SetSpeed request</param>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive, PortFieldName = DrivePortName)]
        public void DriveSetSpeedHandler(diffdrive.SetDriveSpeed setSpeed)
        {
            if (this.driveEntity == null)
            {
                throw new InvalidOperationException("Simulation entity not registered with service");
            }

            if (!this.state.DriveState.IsEnabled)
            {
                setSpeed.ResponsePort.Post(Fault.FromException(new Exception("Drive is not enabled.")));
                LogError("SetSpeed request to disabled drive.");
                return;
            }

            this.driveEntity.SetVelocity((float)setSpeed.Body.LeftWheelSpeed, (float)setSpeed.Body.RightWheelSpeed);

            this.UpdateStateFromSimulation();
            setSpeed.ResponsePort.Post(DefaultUpdateResponseType.Instance);

            // send update notification for entire state
            this.subMgrPort.Post(new submgr.Submit(this.state.DriveState, DsspActions.UpdateRequest));
        }

        /// <summary>
        /// Handler for enabling or disabling the drive
        /// </summary>
        /// <param name="enable">The enable message</param>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent, PortFieldName = DrivePortName)]
        public void DriveEnableHandler(diffdrive.EnableDrive enable)
        {
            if (this.driveEntity == null)
            {
                throw new InvalidOperationException("Simulation entity not registered with service");
            }

            this.state.DriveState.IsEnabled = enable.Body.Enable;
            this.driveEntity.IsEnabled = this.state.DriveState.IsEnabled;

            this.UpdateStateFromSimulation();
            enable.ResponsePort.Post(DefaultUpdateResponseType.Instance);

            // send update for entire state
            this.subMgrPort.Post(new submgr.Submit(this.state.DriveState, DsspActions.UpdateRequest));
        }

        /// <summary>
        /// Handler when the drive receives an all stop message
        /// </summary>
        /// <param name="estop">The stop message</param>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive, PortFieldName = DrivePortName)]
        public void DriveAllStopHandler(diffdrive.AllStop estop)
        {
            if (this.driveEntity == null)
            {
                throw new InvalidOperationException("Simulation entity not registered with service");
            }

            this.driveEntity.SetMotorTorque(0, 0);
            this.driveEntity.SetVelocity(0);

            // AllStop disables the drive
            this.driveEntity.IsEnabled = false;

            this.UpdateStateFromSimulation();
            estop.ResponsePort.Post(DefaultUpdateResponseType.Instance);

            // send update for entire state
            this.subMgrPort.Post(new submgr.Submit(this.state.DriveState, DsspActions.UpdateRequest));
        }

        /// <summary>
        /// Start initializes service state and listens for drop messages
        /// </summary>
        protected void StartSimDrive()
        {
            if (this.state.DriveState == null)
            {
                this.CreateDefaultDriveState();
            }

            // enabled by default
            this.state.DriveState.IsEnabled = true;
        }

        /// <summary>
        /// Rotations to ticks.
        /// </summary>
        /// <param name="wheel">The wheel entity.</param>
        /// <returns>The number of rotations converted to ticks</returns>
        private static int RotationsToTicks(WheelEntity wheel)
        {
            const double MetersPerEncoderTick = 0.01328;

            return (int)(wheel.Rotations * 2 * Math.PI * wheel.Wheel.State.Radius / MetersPerEncoderTick);
        }

        /// <summary>
        /// Creates the default state of the drive.
        /// </summary>
        private void CreateDefaultDriveState()
        {
            this.state.DriveState = new diffdrive.DriveDifferentialTwoWheelState
                {
                    LeftWheel = new motor.WheeledMotorState 
                    { 
                        MotorState = new motor.MotorState(),
                        EncoderState = new encoder.EncoderState() 
                    },
                    RightWheel = new motor.WheeledMotorState
                    {
                        MotorState = new motor.MotorState(),
                        EncoderState = new encoder.EncoderState()
                    },
                };
        }

        /// <summary>
        /// Updates the state from simulation.
        /// </summary>
        private void UpdateStateFromSimulation()
        {
            if (this.driveEntity != null)
            {
                this.state.DriveState.TimeStamp = DateTime.Now;

                // Reverse out the encoder ticks
                this.state.DriveState.LeftWheel.EncoderState.TimeStamp = this.state.DriveState.TimeStamp;
                this.state.DriveState.LeftWheel.EncoderState.CurrentReading =
                    RotationsToTicks(this.driveEntity.LeftWheel) - this.lastResetLeftWheelEncoderValue;
                this.state.DriveState.RightWheel.EncoderState.CurrentReading =
                    RotationsToTicks(this.driveEntity.RightWheel) - this.lastResetRightWheelEncoderValue;

                // Compute the wheel speeds
                this.state.DriveState.LeftWheel.WheelSpeed = -this.driveEntity.LeftWheel.Wheel.AxleSpeed
                                                             * this.driveEntity.LeftWheel.Wheel.State.Radius;
                this.state.DriveState.RightWheel.WheelSpeed = -this.driveEntity.RightWheel.Wheel.AxleSpeed
                                                             * this.driveEntity.RightWheel.Wheel.State.Radius;

                // Compute the power
                this.state.DriveState.LeftWheel.MotorState.CurrentPower = this.driveEntity.LeftWheel.Wheel.MotorTorque;
                this.state.DriveState.RightWheel.MotorState.CurrentPower = this.driveEntity.RightWheel.Wheel.MotorTorque;
                this.state.DriveState.IsEnabled = this.driveEntity.IsEnabled;
            }
        }
    }
    #endregion
}