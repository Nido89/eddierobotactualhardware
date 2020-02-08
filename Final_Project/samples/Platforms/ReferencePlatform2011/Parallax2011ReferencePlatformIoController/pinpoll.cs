//------------------------------------------------------------------------------
//  <copyright file="pinpoll.cs" company="Microsoft Corporation">
//      Copyright (C) Microsoft Corporation.  All rights reserved.
//  </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Robotics.ReferencePlatform.Parallax2011ReferencePlatformIoController
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    using Microsoft.Ccr.Core;
    using Microsoft.Dss.ServiceModel.DsspServiceBase;

    using board = ParallaxControlBoard;
    using serialcomservice = Microsoft.Robotics.Services.SerialComService.Proxy;
    using soap = W3C.Soap;

    /// <summary>
    /// Main class for the service
    /// </summary>
    public partial class Parallax2011ReferencePlatformIoControllerService : DsspServiceBase
    {
        /// <summary>
        /// Keep the FW watchdog alive by polling the pin and encoder values
        /// </summary>
        private Port<DateTime> pinPollingPort = new Port<DateTime>();

        /// <summary>
        /// Keep the FW watchdog alive by continuously retrieving pin and encoder values
        /// </summary>
        /// <param name="dt">A instance of type DateTime</param>
        /// <returns>A instance of IEnumerator of type ITask</returns>
        private IEnumerator<ITask> PollPins(DateTime dt)
        {
            try
            {
                serialcomservice.SendAndGetRequest sg = new serialcomservice.SendAndGetRequest();
                sg.Timeout = this.state.DefaultResponsePause;
                sg.Terminator = board.PacketTerminatorString;
                sg.Data = new serialcomservice.Packet(board.CreatePacket<byte>(board.GetADCValuesString));
                var resultPort = this.serialCOMServicePort.SendAndGet(sg);
                yield return resultPort.Choice();

                soap.Fault f = (soap.Fault)resultPort;
                if (f != null)
                {
                    LogError(string.Format("Failed to send command: {0}", Encoding.ASCII.GetString(sg.Data.Message)));
                    yield break;
                }

                serialcomservice.Packet p = (serialcomservice.Packet)resultPort;
                if (this.HasFWError(p))
                {
                    LogError("Error returned reading ADC values from FW!");
                    yield break;
                }

                // Stash the analog pin values into a string
                string analogValues = Encoding.ASCII.GetString(p.Message);

                // Retrieve the digital pin values
                sg.Data = new serialcomservice.Packet(board.CreatePacket<byte>(board.GetDigitalValuesString));
                resultPort = this.serialCOMServicePort.SendAndGet(sg);
                yield return resultPort.Choice();

                f = (soap.Fault)resultPort;
                if (f != null)
                {
                    LogError(string.Format("Failed to send command: {0}", Encoding.ASCII.GetString(sg.Data.Message)));
                    yield break;
                }

                p = (serialcomservice.Packet)resultPort;
                if (this.HasFWError(p))
                {
                    LogError("Error returned reading digital pin values from FW!");
                    yield break;
                }

                // Stash the digital pin values into a string
                string digitalValues = Encoding.ASCII.GetString(p.Message);

                // Perform all the parsing
                this.UpdatePinStatesFromFWString(analogValues, digitalValues);

                // Reset speed to 0 since it is currently unknown as opposed to having the last value be stuck
                this.state.DriveState.LeftWheel.WheelSpeed = 0;
                this.state.DriveState.RightWheel.WheelSpeed = 0;

                // Retrieve both wheel speeds
                sg.Data = new serialcomservice.Packet(board.CreatePacket<byte>(board.GetCurrentSpeedString));
                resultPort = this.serialCOMServicePort.SendAndGet(sg);
                yield return resultPort.Choice();

                p = (serialcomservice.Packet)resultPort;
                if (this.HasFWError(p))
                {
                    LogError("Error received reading speed values from FW!");
                    yield break;
                }

                f = (soap.Fault)resultPort;
                if (f != null)
                {
                    LogError(string.Format("Failed to send command: {0}", Encoding.ASCII.GetString(sg.Data.Message)));
                    yield break;
                }

                this.UpdateSpeedStateFromFWString(Encoding.ASCII.GetString(p.Message));

                // Retrieve both wheel encoder values
                sg.Data = new serialcomservice.Packet(board.CreatePacket<byte>(board.GetEncoderTicksString));
                resultPort = this.serialCOMServicePort.SendAndGet(sg);
                yield return resultPort.Choice();

                f = (soap.Fault)resultPort;
                if (f != null)
                {
                    LogError(string.Format("Failed to send command: {0}", Encoding.ASCII.GetString(sg.Data.Message)));
                    yield break;
                }

                p = (serialcomservice.Packet)resultPort;
                if (this.HasFWError(p))
                {
                    LogError("Error received reading encoder values from FW!");
                    yield break;
                }

                // Reflect the returned bytes into encoder and motor states
                this.UpdateEncoderStatesFromFWString(Encoding.ASCII.GetString(p.Message));
            }
            finally
            {
                // Ensure we haven't been droppped
                if (ServicePhase == ServiceRuntimePhase.Started)
                {
                    // Issue another polling request
                    Activate(TimeoutPort(this.state.PinPollingInterval).Receive(this.pinPollingPort.Post));
                } 
            }
        }
                                                                                                            
        /// <summary>
        /// Converts the raw string data returned from FW into a state change
        /// </summary>
        /// <param name="analogValues">FW returned string containing analog pin values</param>
        /// <param name="digitalValues">FW returned string containing digital pin values</param>
        private void UpdatePinStatesFromFWString(string analogValues, string digitalValues)
        {
            string[] values;
            double rawVoltage;
            int rawVal;

            try
            {
                if (!string.IsNullOrWhiteSpace(analogValues) && !string.IsNullOrWhiteSpace(digitalValues))
                {
                    int currentPinIndex = 0;
                    analogValues = analogValues.Trim(); // remove the trailing CR
                    values = analogValues.Split(board.ParameterDelimeterStrings, StringSplitOptions.RemoveEmptyEntries);

                    for (int x = 0; x < values.Length; x++)
                    {
                        // Reference platform has 7 externally available analog pins
                        // and an internal analog pin that reports battery voltage
                        if (x < board.ADCPinCount)
                        {
                            rawVoltage = Convert.ToInt32(values[x], 16) * board.ADCVoltageMultiplier;
                            this.state.AdcState.Pins[currentPinIndex].PinValue = rawVoltage;
                            this.state.AdcState.Pins[currentPinIndex].TimeStamp = DateTime.Now;
                        }
                        else if (x == board.ADCPinCount) 
                        {
                            // battery voltage is reported on the last ADC pin
                            rawVoltage = Convert.ToInt32(values[currentPinIndex], 16) * board.ADCVoltageMultiplier * board.BatteryVoltageDivider;
                        }

                        currentPinIndex++;
                    }

                    digitalValues = digitalValues.Trim();
                    values = digitalValues.Split(board.ParameterDelimeterStrings, StringSplitOptions.RemoveEmptyEntries);
                    
                    for (int x = 0; x < values.Length; x++)
                    {
                        // Reference platform has 10 pins dedicated to digital sensors (Ping Sonar)
                        // but these same pins can also be configured as GPIO
                        if (x < board.DigitalPinCount)
                        {
                            rawVal = Convert.ToInt32(values[x], 16);
                            this.state.AdcState.Pins[currentPinIndex].PinValue = rawVal;
                            this.state.AdcState.Pins[currentPinIndex].TimeStamp = DateTime.Now;
                        }

                        currentPinIndex++;
                    }
                }
            }
            catch (Exception e)
            {
                LogError("Invalid or unparseable pin value returned from FW!", e);
            }
        }
    }
}