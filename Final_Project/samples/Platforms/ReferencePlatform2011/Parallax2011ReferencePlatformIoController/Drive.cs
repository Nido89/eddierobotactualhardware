//------------------------------------------------------------------------------
//  <copyright file="Drive.cs" company="Microsoft Corporation">
//      Copyright (C) Microsoft Corporation.  All rights reserved.
//  </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Robotics.ReferencePlatform.Parallax2011ReferencePlatformIoController
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Text;
    
    using Microsoft.Ccr.Core;
    using Microsoft.Dss.Core.Attributes;
    using Microsoft.Dss.Core.DsspHttp;
    using Microsoft.Dss.ServiceModel.Dssp;
    using Microsoft.Dss.ServiceModel.DsspServiceBase;

    using board = ParallaxControlBoard;
    using drive = Microsoft.Robotics.Services.Drive;
    using serialcomservice = Microsoft.Robotics.Services.SerialComService.Proxy;
    using soap = W3C.Soap;
    using submgr = Microsoft.Dss.Services.SubscriptionManager;

    /// <summary>
    /// Main service class
    /// </summary>
    public partial class Parallax2011ReferencePlatformIoControllerService : DsspServiceBase
    {
        /// <summary> Generic Differential Drive values go from -1 (full reverse) to 1 (full forward) </summary>
        private const double GDDriveFullStop = 0;

        /// <summary> Min value motor speed/direction for Generic Differential Drive </summary>
        private const double GDDriveMin = -1;

        /// <summary> Max value motor speed/direction for Generic Differential Drive </summary>
        private const double GDDriveMax = 1;

        /// <summary> Scaling factor for converting [-1 to 1] to [1000 to 2000] in drive commands </summary>
        private const double DrivePowerScale = (board.HBridgeForwardMax - board.HBridgeReverseMax) / (GDDriveMax - GDDriveMin);

        /// <summary>
        /// Wheel and Motor Positions
        /// </summary>
        private enum Sides
        {
            /// <summary>
            /// Robot's left side
            /// </summary>
            Left,

            /// <summary>
            /// Robot's right side
            /// </summary>
            Right
        }

        /// <summary>
        /// Human-readable wheel and motor positions
        /// </summary>
        private string[] wheelNames = { "Left Wheel", "Right Wheel" }; 

        /// <summary>
        /// Drive Port Identifier used in attributes
        /// </summary>
        private const string DrivePortName = "drivePort";

        /// <summary>
        /// Encoder ticks per meter for each wheel
        /// </summary>
        private double[] encoderTicksPerMeter = { 1, 1 };

        /// <summary>
        /// Alternate contract service port
        /// </summary>
        [AlternateServicePort(AlternateContract = drive.Contract.Identifier)]
        private drive.DriveOperations drivePort = new drive.DriveOperations();

        /// <summary>
        /// Drive service subscription port
        /// </summary>
        [SubscriptionManagerPartner("Drive")]
        private submgr.SubscriptionManagerPort submgrDrivePort = new submgr.SubscriptionManagerPort();

        /// <summary>
        /// Handles Get requests on alternate port: Drive
        /// </summary>
        /// <param name="get">Request message</param>
        [ServiceHandler(PortFieldName = DrivePortName)]
        public void DriveGetHandler(drive.Get get)
        {
            this.state.DriveState.TimeStamp = DateTime.Now;
            get.ResponsePort.Post(this.state.DriveState);
        }

        /// <summary>
        /// Handles HttpGet requests on alternate port: Drive
        /// </summary>
        /// <param name="httpget">Request message</param>
        [ServiceHandler(PortFieldName = DrivePortName)]
        public void DriveHttpGetHandler(Microsoft.Dss.Core.DsspHttp.HttpGet httpget)
        {
            this.state.DriveState.TimeStamp = DateTime.Now;
            HttpResponseType resp = new HttpResponseType(HttpStatusCode.OK, this.state.DriveState);
            httpget.ResponsePort.Post(resp);
        }

        /// <summary>
        /// Handles EnableDrive requests on alternate port: Drive
        /// </summary>
        /// <param name="enabledrive">Request message</param>
        /// <returns>An IEnumerator object of type ITask</returns>
        [ServiceHandler(PortFieldName = DrivePortName)]
        public IEnumerator<ITask> DriveEnableDriveHandler(drive.EnableDrive enabledrive)
        {
            this.state.DriveState.TimeStamp = DateTime.Now;
            this.state.DriveState.IsEnabled = enabledrive.Body.Enable;

            // The caller's intent is to disable the drive, so we need to set motor power to zero
            if (!this.state.DriveState.IsEnabled)
            {
                serialcomservice.SendAndGetRequest sg = new serialcomservice.SendAndGetRequest();
                sg.Timeout = this.state.DefaultResponsePause;
                sg.Data = new serialcomservice.Packet(board.CreatePacket<byte>(board.SetFullStopString));

                var resultPort = this.serialCOMServicePort.SendAndGet(sg);
                yield return resultPort.Choice();

                soap.Fault f = (soap.Fault)resultPort;
                if (f != null)
                {
                    LogError(string.Format("Failed to send command: {0}", board.SetFullStopString));
                    enabledrive.ResponsePort.Post(f);
                    yield break;
                }

                if (this.HasFWError((serialcomservice.Packet)resultPort))
                {
                    f = soap.Fault.FromCodeSubcodeReason(soap.FaultCodes.Receiver, DsspFaultCodes.OperationFailed, "Error received from FW!");
                    enabledrive.ResponsePort.Post(f);
                    yield break;
                }

                // Reflect into state
                this.state.DriveState.LeftWheel.MotorState.CurrentPower = GDDriveFullStop;
                this.state.DriveState.RightWheel.MotorState.CurrentPower = GDDriveFullStop;
                this.state.DriveState.LeftWheel.WheelSpeed = 0;
                this.state.DriveState.RightWheel.WheelSpeed = 0;
            }

            enabledrive.ResponsePort.Post(DefaultUpdateResponseType.Instance);
        }

        /// <summary>
        /// Handles SetDrivePower requests on alternate port: Drive
        /// </summary>
        /// <param name="setdrivepower">Request message</param>
        /// <returns>An IEnumerator object of type ITask</returns>
        [ServiceHandler(PortFieldName = DrivePortName, QueueDepthLimit = 1)]
        public IEnumerator<ITask> DriveSetDrivePowerHandler(drive.SetDrivePower setdrivepower)
        {
            if (!this.state.DriveState.IsEnabled)
            {
                setdrivepower.ResponsePort.Post(soap.Fault.FromCodeSubcodeReason(
                                                                                 soap.FaultCodes.Receiver,
                                                                                 DsspFaultCodes.OperationFailed,
                                                                                 "Drive not enabled!"));
                yield break;
            }

            this.state.DriveState.TimeStamp = DateTime.Now;

            sbyte leftDrivePower = (sbyte)(board.HBridgeReverseMax + 
                                      ((setdrivepower.Body.LeftWheelPower - GDDriveMin) 
                                      * DrivePowerScale));

            sbyte rightDrivePower = (sbyte)(board.HBridgeReverseMax + 
                                       ((setdrivepower.Body.RightWheelPower - GDDriveMin) 
                                       * DrivePowerScale));

            byte[] cmdpacket = board.CreatePacket<sbyte>(board.SetTravelPowerString, leftDrivePower, rightDrivePower);

            serialcomservice.SendAndGetRequest sg = new serialcomservice.SendAndGetRequest();
            sg.Timeout = this.state.DefaultResponsePause;
            sg.Data = new serialcomservice.Packet(cmdpacket);

            var resultPort = this.serialCOMServicePort.SendAndGet(sg);
            yield return resultPort.Choice();

            soap.Fault f = (soap.Fault)resultPort;
            if (f != null)
            {
                LogError(string.Format("Failed to send command: {0}", Encoding.ASCII.GetString(sg.Data.Message)));
                setdrivepower.ResponsePort.Post(f);
                yield break;
            }

            if (this.HasFWError((serialcomservice.Packet)resultPort))
            {
                f = soap.Fault.FromCodeSubcodeReason(soap.FaultCodes.Receiver, DsspFaultCodes.OperationFailed, "Error received from FW!");
                setdrivepower.ResponsePort.Post(f);
                yield break;
            }

            this.state.DriveState.LeftWheel.MotorState.CurrentPower = setdrivepower.Body.LeftWheelPower;
            this.state.DriveState.RightWheel.MotorState.CurrentPower = setdrivepower.Body.RightWheelPower;
            setdrivepower.ResponsePort.Post(DefaultUpdateResponseType.Instance);
        }

        /// <summary>
        /// Handles AllStop requests on alternate port: Drive
        /// </summary>
        /// <param name="allstop">Request message</param>
        /// <returns>An IEnumerator object of type ITask</returns>
        [ServiceHandler(PortFieldName = DrivePortName)]
        public IEnumerator<ITask> DriveAllStopHandler(drive.AllStop allstop)
        {
            this.state.DriveState.TimeStamp = DateTime.Now;

            serialcomservice.SendAndGetRequest sg = new serialcomservice.SendAndGetRequest();
            sg.Timeout = this.state.DefaultResponsePause;
            sg.Data = new serialcomservice.Packet(board.CreatePacket<sbyte>(board.SetFullStopString));

            var resultPort = this.serialCOMServicePort.SendAndGet(sg);
            yield return resultPort.Choice();

            soap.Fault f = (soap.Fault)resultPort;
            if (f != null)
            {
                LogError(string.Format("Failed to send command: {0}", Encoding.ASCII.GetString(sg.Data.Message)));
                allstop.ResponsePort.Post(f);
                yield break;
            }

            if (this.HasFWError((serialcomservice.Packet)resultPort))
            {
                f = soap.Fault.FromCodeSubcodeReason(soap.FaultCodes.Receiver, DsspFaultCodes.OperationFailed, "Error received from FW!");
                allstop.ResponsePort.Post(f);
                yield break;
            }

            // Reflect into our state
            this.state.DriveState.LeftWheel.MotorState.CurrentPower = GDDriveFullStop;
            this.state.DriveState.RightWheel.MotorState.CurrentPower = GDDriveFullStop;
            this.state.DriveState.LeftWheel.WheelSpeed = 0;
            this.state.DriveState.RightWheel.WheelSpeed = 0;
            this.state.DriveState.IsEnabled = false;
            allstop.ResponsePort.Post(DefaultUpdateResponseType.Instance);
        }

        /// <summary>
        /// Handles RotateDegrees requests on alternate port: Drive
        /// Rotates in place, N degrees, and P power
        /// </summary>
        /// <param name="rotatedegrees">Request message</param>
        /// <returns>An IEnumerator object of type ITask</returns>
        [ServiceHandler(PortFieldName = DrivePortName, QueueDepthLimit = 1)]
        public IEnumerator<ITask> DriveRotateDegreesHandler(drive.RotateDegrees rotatedegrees)
        {
            if (!this.state.DriveState.IsEnabled)
            {
                rotatedegrees.ResponsePort.Post(soap.Fault.FromCodeSubcodeReason(
                                                                                 soap.FaultCodes.Receiver, 
                                                                                 DsspFaultCodes.OperationFailed, 
                                                                                 "Drive not enabled!"));
                yield break;
            }

            this.state.DriveState.TimeStamp = DateTime.Now;

            short pwr = (short)(board.HBridgeReverseMax +
                                      ((rotatedegrees.Body.Power - GDDriveMin)
                                      * DrivePowerScale));
            short deg = (short)(this.encoderTicksPerMeter[(int)Sides.Left] * rotatedegrees.Body.Degrees);

            byte[] cmdpacket = board.CreatePacket<short, short>(board.SetRotateInPlaceString, deg, pwr);

            serialcomservice.SendAndGetRequest sg = new serialcomservice.SendAndGetRequest();
            sg.Timeout = this.state.DefaultResponsePause;
            sg.Data = new serialcomservice.Packet(cmdpacket);

            var resultPort = this.serialCOMServicePort.SendAndGet(sg);
            yield return resultPort.Choice();

            soap.Fault f = (soap.Fault)resultPort;
            if (f != null)
            {
                LogError(string.Format("Failed to send command: {0}", Encoding.ASCII.GetString(sg.Data.Message)));
                rotatedegrees.ResponsePort.Post(f);
                yield break;
            }

            if (this.HasFWError((serialcomservice.Packet)resultPort))
            {
                f = soap.Fault.FromCodeSubcodeReason(
                                                     soap.FaultCodes.Receiver,
                                                     DsspFaultCodes.OperationFailed,
                                                     "Error received from FW!");
                rotatedegrees.ResponsePort.Post(f);
                yield break;
            }

            this.state.DriveState.LeftWheel.MotorState.CurrentPower = rotatedegrees.Body.Power;
            this.state.DriveState.RightWheel.MotorState.CurrentPower = rotatedegrees.Body.Power;
            rotatedegrees.ResponsePort.Post(DefaultUpdateResponseType.Instance);
        }

        /// <summary>
        /// Handles DriveDistance requests on alternate port: Drive
        /// </summary>
        /// <param name="drivedistance">Request message</param>
        /// <returns>An IEnumerator object of type ITask</returns>
        [ServiceHandler(PortFieldName = DrivePortName, QueueDepthLimit = 1)]
        public IEnumerator<ITask> DriveDriveDistanceHandler(drive.DriveDistance drivedistance)
        {
            if (!this.state.DriveState.IsEnabled)
            {
                drivedistance.ResponsePort.Post(soap.Fault.FromCodeSubcodeReason(
                                                                                 soap.FaultCodes.Receiver,
                                                                                 DsspFaultCodes.OperationFailed,
                                                                                 "Drive not enabled!"));
                yield break;
            }

            this.state.DriveState.TimeStamp = DateTime.Now;

            short pwr = (short)(board.HBridgeReverseMax +
                                      ((drivedistance.Body.Power - GDDriveMin)
                                      * DrivePowerScale));

            short dist = (short)(this.encoderTicksPerMeter[(int)Sides.Left] * drivedistance.Body.Distance);

            byte[] cmdpacket = board.CreatePacket<short, short>(board.SetTravelDistanceString, dist, pwr);

            serialcomservice.SendAndGetRequest sg = new serialcomservice.SendAndGetRequest();
            sg.Timeout = this.state.DefaultResponsePause;
            sg.Data = new serialcomservice.Packet(cmdpacket);

            var resultPort = this.serialCOMServicePort.SendAndGet(sg);
            yield return resultPort.Choice();

            soap.Fault f = (soap.Fault)resultPort;
            if (f != null)
            {
                LogError(string.Format("Failed to send command: {0}", Encoding.ASCII.GetString(sg.Data.Message)));
                drivedistance.ResponsePort.Post(f);
                yield break;
            }

            if (this.HasFWError((serialcomservice.Packet)resultPort))
            {
                f = soap.Fault.FromCodeSubcodeReason(
                                                     soap.FaultCodes.Receiver, 
                                                     DsspFaultCodes.OperationFailed, 
                                                     "Error received from FW!");
                drivedistance.ResponsePort.Post(f);
                yield break;
            }

            this.state.DriveState.LeftWheel.MotorState.CurrentPower = drivedistance.Body.Power;
            this.state.DriveState.RightWheel.MotorState.CurrentPower = drivedistance.Body.Power;
            this.state.DriveState.DriveDistanceStage = drive.DriveStage.Completed;
            drivedistance.ResponsePort.Post(DefaultUpdateResponseType.Instance);
        }

        /// <summary>
        /// Handles SetDriveSpeed requests on alternate port: Drive
        /// </summary>
        /// <param name="setdrivespeed">Request message</param>
        /// <returns>An IEnumerator object of type ITask</returns>
        [ServiceHandler(PortFieldName = DrivePortName, QueueDepthLimit = 1)]
        public IEnumerator<ITask> DriveSetDriveSpeedHandler(drive.SetDriveSpeed setdrivespeed)
        {
            if (!this.state.DriveState.IsEnabled)
            {
                setdrivespeed.ResponsePort.Post(soap.Fault.FromCodeSubcodeReason(
                                                                                 soap.FaultCodes.Receiver,
                                                                                 DsspFaultCodes.OperationFailed,
                                                                                 "Drive not enabled!"));
                yield break;
            }

            this.state.DriveState.TimeStamp = DateTime.Now;

            short leftWheelSpeed = (short)(this.encoderTicksPerMeter[(int)Sides.Left] * setdrivespeed.Body.LeftWheelSpeed);
            short rightWheelSpeed = (short)(this.encoderTicksPerMeter[(int)Sides.Right] * setdrivespeed.Body.RightWheelSpeed);

            byte[] cmdpacket = board.CreatePacket<short>(board.SetTravelVelocityString, leftWheelSpeed, rightWheelSpeed);

            serialcomservice.SendAndGetRequest sg = new serialcomservice.SendAndGetRequest();
            sg.Timeout = this.state.DefaultResponsePause;
            sg.Data = new serialcomservice.Packet(cmdpacket);

            var resultPort = this.serialCOMServicePort.SendAndGet(sg);
            yield return resultPort.Choice();

            soap.Fault f = (soap.Fault)resultPort;
            if (f != null)
            {
                LogError(string.Format("Failed to send command: {0}", Encoding.ASCII.GetString(sg.Data.Message)));
                setdrivespeed.ResponsePort.Post(f);
                yield break;
            }

            if (this.HasFWError((serialcomservice.Packet)resultPort))
            {
                f = soap.Fault.FromCodeSubcodeReason(soap.FaultCodes.Receiver, DsspFaultCodes.OperationFailed, "Error received from FW!");
                setdrivespeed.ResponsePort.Post(f);
                yield break;
            }

            this.state.DriveState.LeftWheel.WheelSpeed = setdrivespeed.Body.LeftWheelSpeed;
            this.state.DriveState.RightWheel.WheelSpeed = setdrivespeed.Body.RightWheelSpeed;
            setdrivespeed.ResponsePort.Post(DefaultUpdateResponseType.Instance);
        }

        #region NOTIMPLS
        /// <summary>
        /// Handles HttpPost requests on alternate port: Drive
        /// </summary>
        /// <param name="httppost">Request message</param>
        [ServiceHandler(PortFieldName = DrivePortName)]
        public void DriveHttpPostHandler(Microsoft.Dss.Core.DsspHttp.HttpPost httppost)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Handles Update requests on alternate port: Drive
        /// </summary>
        /// <param name="update">Request message</param>
        [ServiceHandler(PortFieldName = DrivePortName)]
        public void DriveUpdateHandler(drive.Update update)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region SubscriptionHandlers
        /// <summary>
        /// Handles ReliableSubscribe requests on alternate port Drive
        /// </summary>
        /// <param name="reliablesubscribe">Request message</param>
        [ServiceHandler(PortFieldName = DrivePortName)]
        public void DriveReliableSubscribeHandler(drive.ReliableSubscribe reliablesubscribe)
        {
            SubscribeHelper(this.submgrDrivePort, reliablesubscribe.Body, reliablesubscribe.ResponsePort);
        }

        /// <summary>
        /// Handles Subscribe requests on alternate port Drive
        /// </summary>
        /// <param name="subscribe">Request message</param>
        [ServiceHandler(PortFieldName = DrivePortName)]
        public void DriveSubscribeHandler(drive.Subscribe subscribe)
        {
            SubscribeHelper(this.submgrDrivePort, subscribe.Body, subscribe.ResponsePort);
        }
        #endregion
    }
}