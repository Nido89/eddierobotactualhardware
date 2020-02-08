//------------------------------------------------------------------------------
//  <copyright file="Encoders.cs" company="Microsoft Corporation">
//      Copyright (C) Microsoft Corporation.  All rights reserved.
//  </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Robotics.ReferencePlatform.Parallax2011ReferencePlatformIoController
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Ccr.Core;
    using Microsoft.Dss.Core.Attributes;
    using Microsoft.Dss.ServiceModel.Dssp;
    using Microsoft.Dss.ServiceModel.DsspServiceBase;

    using board = ParallaxControlBoard;
    using drive = Microsoft.Robotics.Services.Drive;
    using serialcomservice = Microsoft.Robotics.Services.SerialComService.Proxy;
    using soap = W3C.Soap;

    /// <summary>
    /// Main class for the service
    /// </summary>
    public partial class Parallax2011ReferencePlatformIoControllerService : DsspServiceBase
    {
        /// <summary>
        /// Performs the string parsing from FW on current speed and updates wheel speeds
        /// </summary>
        /// <param name="fwSpeedString">The speed values (L/R) returned from the FW</param>
        private void UpdateSpeedStateFromFWString(string fwSpeedString)
        {
            try
            {
                string[] values = null;
                
                fwSpeedString = fwSpeedString.Trim();

                values = fwSpeedString.Split(board.ParameterDelimeterStrings, StringSplitOptions.RemoveEmptyEntries);
                if (values.Length != board.EncoderCount)
                {
                    LogError(string.Format("FW ERROR: Unexpected data returned for speed values: {0}", fwSpeedString));                    
                    return;
                }

                this.state.DriveState.LeftWheel.WheelSpeed = Convert.ToInt16(values[(int)Sides.Left], 16) / this.encoderTicksPerMeter[(int)Sides.Left];
                this.state.DriveState.RightWheel.WheelSpeed = Convert.ToInt16(values[(int)Sides.Right], 16) / this.encoderTicksPerMeter[(int)Sides.Right];
            }
            catch (Exception e)
            {
                LogError(string.Format("Failed to parse speed value from FW: {0}", fwSpeedString));
                LogError(e);
                return;
            }
        }

        /// <summary>
        /// Performs string parsing from FW and updates all state values
        /// </summary>
        /// <param name="fwEncoderString">The encoder values (L/R) returned from the FW</param>
        private void UpdateEncoderStatesFromFWString(string fwEncoderString)
        {
            string[] rlvals;

            try
            {
                fwEncoderString = fwEncoderString.Trim(); // remove the trailing CR

                rlvals = fwEncoderString.Split(board.ParameterDelimeterStrings, StringSplitOptions.RemoveEmptyEntries);
                if (rlvals.Length != board.EncoderCount)
                {
                    LogError(string.Format("FW ERROR: Unexpected data returned for encoder values: {0}", fwEncoderString));
                    return;
                }

                this.state.DriveState.LeftWheel.EncoderState.TicksSinceReset =
                this.state.DriveState.LeftWheel.EncoderState.CurrentReading = Convert.ToInt32(rlvals[(int)Sides.Left], 16);

                this.state.DriveState.RightWheel.EncoderState.TicksSinceReset =
                this.state.DriveState.RightWheel.EncoderState.CurrentReading = Convert.ToInt32(rlvals[(int)Sides.Right], 16);

                this.state.DriveState.LeftWheel.EncoderState.TimeStamp =
                this.state.DriveState.RightWheel.EncoderState.TimeStamp = DateTime.Now;
            }
            catch (Exception e)
            {
                LogError(string.Format("Failed to parse encoder value from FW: {0}", fwEncoderString));
                LogError(e);
                return;
            }
        }

        /// <summary>
        /// Handles right wheel Reset requests on alternate port
        /// resets the encoder tick count to zero on both wheels
        /// </summary>
        /// <param name="reset">Request message</param>
        [ServiceHandler(PortFieldName = DrivePortName)]
        public void EncoderResetHandler(drive.ResetEncoders reset)
        {
            SpawnIterator<drive.ResetEncoders>(reset, this.InternalEncoderReset);
        }

        /// <summary>
        /// Performs a reset on both encoders in response to 
        /// a reset (update) on either encoder.
        /// </summary>
        /// <param name="reset">An encoder Reset operation type</param>
        /// <returns>An IEnumerator of type ITask</returns>
        private IEnumerator<ITask> InternalEncoderReset(drive.ResetEncoders reset)
        {
            serialcomservice.SendAndGetRequest sg = new serialcomservice.SendAndGetRequest();
            sg.Timeout = this.state.DefaultResponsePause;
            sg.Terminator = board.PacketTerminatorString;
            sg.Data = new serialcomservice.Packet(board.CreatePacket<byte>(board.ResetEncoderTicksString));

            PortSet<serialcomservice.Packet, soap.Fault> resultPort = this.serialCOMServicePort.SendAndGet(sg);
            yield return resultPort.Choice();

            soap.Fault f = (soap.Fault)resultPort;
            if (f != null)
            {
                reset.ResponsePort.Post(f);
                yield break;
            }

            serialcomservice.Packet p = (serialcomservice.Packet)resultPort;
            if (this.HasFWError(p))
            {
                f = soap.Fault.FromCodeSubcodeReason(
                                                     soap.FaultCodes.Receiver,
                                                     DsspFaultCodes.OperationFailed,
                                                     "FW ERROR: Failed to reset encoder tick count!");
                reset.ResponsePort.Post(f);
                yield break;
            }

            // update both the encoder partner state as well as the brick state
            this.state.DriveState.LeftWheel.EncoderState.TicksSinceReset =
            this.state.DriveState.RightWheel.EncoderState.TicksSinceReset = 0;

            this.state.DriveState.LeftWheel.EncoderState.CurrentAngle =
            this.state.DriveState.RightWheel.EncoderState.CurrentAngle = 0;

            this.state.DriveState.LeftWheel.EncoderState.CurrentReading =
            this.state.DriveState.RightWheel.EncoderState.CurrentReading = 0;

            this.state.DriveState.LeftWheel.EncoderState.TimeStamp =
            this.state.DriveState.RightWheel.EncoderState.TimeStamp = DateTime.Now;

            // Notify any brick subscribers as well as any drive subscribers
            this.SendNotification<Replace>(this.submgrPort, new Replace());
            this.SendNotification<drive.ResetEncoders>(this.submgrDrivePort, new drive.ResetEncoders());

            reset.ResponsePort.Post(DefaultUpdateResponseType.Instance);
        }
    }
}