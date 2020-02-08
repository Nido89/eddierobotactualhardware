//------------------------------------------------------------------------------
//  <copyright file="ParallaxControlBoard.cs" company="Microsoft Corporation">
//      Copyright (C) Microsoft Corporation.  All rights reserved.
//  </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Robotics.ReferencePlatform.Parallax2011ReferencePlatformIoController
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Text;

    /// <summary>
    /// Represents the FW and corresponding physical properties of the Parallax Robot Controller Board
    /// </summary>
    internal class ParallaxControlBoard
    {
        /// <summary> Number of potential GPIO pins available </summary>
        public const int GPIOPinCount = 10;

        /// <summary> Number of ADC pins available </summary>
        public const int ADCPinCount = 8;

        /// <summary> FW returns values representing 1/819.2v for ADC pins </summary>
        public const double ADCVoltageMultiplier = 1 / 819.2;

        /// <summary> FW returns a battery voltage divided </summary>
        public const double BatteryVoltageDivider = 3.21;

        /// <summary> Number of potential digital pins available. </summary>
        public const int DigitalPinCount = 10;

        /// <summary> Number of switchable power ports </summary>
        public const int AuxiliaryPowerRelayCount = 3;

        /// <summary> COM port buffer size </summary>
        public const int ParallaxMaxBuffer = 256;

        /// <summary> Parallax drive speed/direction values go from -128 (full reverse) to 127 (full forward) </summary>
        public const sbyte HBridgeFullStop = 0;

        /// <summary> Reference platform has an encoder for each wheel </summary>
        public const int EncoderCount = 2;

        /// <summary> 
        /// Min value motor speed/direction for Parallax HB25 
        /// Note: Even though the range accepted by the FW is min(sbyte), keeping symmetry with 
        /// the maximum value makes the conversion function simpler with no functional loss
        /// </summary>
        public const int HBridgeReverseMax = -255; 

        /// <summary> Max value motor speed/direction for Parallax HB25 </summary>
        public const int HBridgeForwardMax = 255;

        /// <summary> 3.3v Solid State Relay is located on GPIO pin 11 </summary>
        public const byte Relay33vPinNumber = 16;

        /// <summary> 5v Solid State Relay is located on GPIO pin 12 </summary>
        public const byte Relay5vPinNumber = 17;

        /// <summary> 12v Solid State Relay is located on GPIO pin 13 </summary>
        public const byte Relay12vPinNumber = 18;

        /// <summary> Positive response from FW </summary>
        public static readonly byte PacketTerminatorByte = Convert.ToByte('\r');

        /// <summary> Packet terminator as string </summary>
        public const string PacketTerminatorString = "\r";

        /// <summary> Delimiting character in all commands that take parameters </summary>
        public static readonly byte ParameterDelimiterByte = Convert.ToByte(' ');

        /// <summary> Delimiting character in all commands that take parameters </summary>
        public static readonly string[] ParameterDelimeterStrings = { " " };

        /// <summary> Send a series of 3 carriage returns to reset the FW COM buffer</summary>
        public static readonly byte[] FlushBuffers = Encoding.ASCII.GetBytes("\r\r\r");

        /// <summary> Response from FW in the case of a problem </summary>
        public const string Error = "ERROR";

        /// <summary> Default wheel radius in meters </summary>
        public const double DefaultWheelRadius = 0.0762;

        /// <summary> Default ticks per revolution </summary>
        public const int DefaultTicksPerRevolution = 36;

        #region FWCommandStrings

        ////////////////////////////////////////////////////////////
        // The current FW commands
        ////////////////////////////////////////////////////////////

        /// <summary> Returns 16 bits representing the FW version</summary>
        public const string GetVersionString = "VER";

        /// <summary> Sets specific GPIO pins to output using a 20 bit mask</summary>
        public const string SetGPIODirectionOutString = "OUT";

        /// <summary> Sets specific GPIO pins to input using a 20 bit mask</summary>
        public const string SetGPIODirectionInString = "IN";

        /// <summary> Sets specific GPIO output pins to high using a 20 bit mask</summary>
        public const string SetGPIOStateHighString = "HIGH";

        /// <summary> Sets specific GPIO output pins to low using a 20 bit mask</summary>
        public const string SetGPIOStateLowString = "LOW";

        /// <summary> Returns 20 bits representing the high/low state of all GPIO pins</summary>
        public const string GetGPIOStatesString = "READ";

        /// <summary> Returns the ADC values as 8 separate 12 bit words </summary>
        public const string GetADCValuesString = "ADC";

        /// <summary> Returns the digital (PING) values as 10 separate 16 bit words </summary>
        public const string GetDigitalValuesString = "PING";

        /// <summary> Sets drive power using 8 bits (signed) for each wheel, -128 is full reverse, 127 is full forward</summary>
        public const string SetTravelPowerString = "GO";

        /// <summary> Sets drive speed using 16 bits (signed) for each wheel, ? is full reverse, ? is full forward</summary>
        public const string SetTravelVelocityString = "GOSPD";

        /// <summary> Sets drive distance and speed using 16 bits (signed) and 16 bits (signed), respectively </summary>
        public const string SetTravelDistanceString = "TRVL";

        /// <summary> Sets a gradual slow down to stop distance using 16 bits</summary>
        public const string SetStoppingDistanceString = "STOP";

        /// <summary> Sends a rotate in place degrees and speed using 16 bits and 16 bits, respectively</summary>
        public const string SetRotateInPlaceString = "TURN";

        /// <summary> Returns the current speed as 8 bits </summary>
        public const string GetCurrentSpeedString = "SPD";

        /// <summary> Returns the current heading, relative to start, as 16 bits (signed) </summary>
        public const string GetCurrentHeadingString = "HEAD";

        /// <summary> Returns right and left encoder ticks as a pair of signed 16 bit values </summary>
        public const string GetEncoderTicksString = "DIST";

        /// <summary> Zeros out the internal registers where encoder ticks are accumulated </summary>
        public const string ResetEncoderTicksString = "RST";

        /// <summary> Sets a velocity ramping value for drive system </summary>
        public const string SetRampingValueString = "ACC";

        /// <summary> Send a full stop </summary>
        public const string SetFullStopString = "STOP 00";
        #endregion

        /// <summary> Associative lookups for command string -> packet bytes </summary>
        private static Dictionary<string, byte[]> commandSet = new Dictionary<string, byte[]>();

        /// <summary>
        /// Populate the command set container
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1810", Justification = "Need to add items to dictionary")]
        static ParallaxControlBoard()
        {
            commandSet.Add(GetVersionString, Encoding.ASCII.GetBytes(GetVersionString));
            commandSet.Add(SetGPIODirectionOutString, Encoding.ASCII.GetBytes(SetGPIODirectionOutString));
            commandSet.Add(SetGPIODirectionInString, Encoding.ASCII.GetBytes(SetGPIODirectionInString));
            commandSet.Add(SetGPIOStateHighString, Encoding.ASCII.GetBytes(SetGPIOStateHighString));
            commandSet.Add(SetGPIOStateLowString, Encoding.ASCII.GetBytes(SetGPIOStateLowString));
            commandSet.Add(GetGPIOStatesString, Encoding.ASCII.GetBytes(GetGPIOStatesString));
            commandSet.Add(GetADCValuesString, Encoding.ASCII.GetBytes(GetADCValuesString));
            commandSet.Add(GetDigitalValuesString, Encoding.ASCII.GetBytes(GetDigitalValuesString));
            commandSet.Add(SetTravelPowerString, Encoding.ASCII.GetBytes(SetTravelPowerString));
            commandSet.Add(SetTravelDistanceString, Encoding.ASCII.GetBytes(SetTravelDistanceString));
            commandSet.Add(SetTravelVelocityString, Encoding.ASCII.GetBytes(SetTravelVelocityString));
            commandSet.Add(SetStoppingDistanceString, Encoding.ASCII.GetBytes(SetStoppingDistanceString));
            commandSet.Add(SetRotateInPlaceString, Encoding.ASCII.GetBytes(SetRotateInPlaceString));
            commandSet.Add(GetCurrentSpeedString, Encoding.ASCII.GetBytes(GetCurrentSpeedString));
            commandSet.Add(GetCurrentHeadingString, Encoding.ASCII.GetBytes(GetCurrentHeadingString));
            commandSet.Add(GetEncoderTicksString, Encoding.ASCII.GetBytes(GetEncoderTicksString));
            commandSet.Add(ResetEncoderTicksString, Encoding.ASCII.GetBytes(ResetEncoderTicksString));
            commandSet.Add(SetFullStopString, Encoding.ASCII.GetBytes(SetFullStopString));
            commandSet.Add(SetRampingValueString, Encoding.ASCII.GetBytes(SetRampingValueString));
        }

        /// <summary>
        /// Returns the bytes for a command
        /// </summary>
        /// <param name="cmd">FW command string</param>
        /// <returns>Byte array representation of command string</returns>
        public static byte[] GetCommandBytes(string cmd)
        {
            return commandSet[cmd];
        }

        /// <summary>
        /// Creates a packet given a command and arguments
        /// </summary>
        /// <typeparam name="T">Type of argument</typeparam>
        /// <param name="cmd">Command string</param>
        /// <param name="args">Variable length argument list</param>
        /// <returns>Byte array representation of command and arguments</returns>
        public static byte[] CreatePacket<T>(string cmd, params T[] args)
        {
            List<byte> ret = new List<byte>(GetCommandBytes(cmd));
            ret.Add(ParameterDelimiterByte);

            foreach (T t in args)
            {
                string s = string.Format("{0:X}", t);
                ret.AddRange(Encoding.ASCII.GetBytes(s));
                ret.Add(ParameterDelimiterByte);
            }

            // clobber the trailing ParameterDelimeter with a PacketTerminator
            ret[ret.Count - 1] = PacketTerminatorByte;
            return ret.ToArray();
        }

        /// <summary>
        /// Creates a packet given a command and two dissimarly typed arguments
        /// </summary>
        /// <typeparam name="T0">Type of first argument</typeparam>
        /// <typeparam name="T1">Type of second argument</typeparam>
        /// <param name="cmd">Command string</param>
        /// <param name="arg0">First argument</param>
        /// <param name="arg1">Second argument</param>
        /// <returns>Byte array representation of command and arguments</returns>
        public static byte[] CreatePacket<T0, T1>(string cmd, T0 arg0, T1 arg1)
        {
            List<byte> ret = new List<byte>(GetCommandBytes(cmd));
            ret.Add(ParameterDelimiterByte);

            string s = string.Format("{0:X}", arg0);
            ret.AddRange(Encoding.ASCII.GetBytes(s));
            ret.Add(ParameterDelimiterByte);

            s = string.Format("{0:X}", arg1);
            ret.AddRange(Encoding.ASCII.GetBytes(s));

            ret.Add(PacketTerminatorByte);
            return ret.ToArray();
        }

        /// <summary>
        /// Creates a packet given a command and three dissimarly typed arguments
        /// </summary>
        /// <typeparam name="T0">Type of first argument</typeparam>
        /// <typeparam name="T1">Type of second argument</typeparam>
        /// <typeparam name="T2">Type of third argument</typeparam>
        /// <param name="cmd">Command string</param>
        /// <param name="arg0">First argument</param>
        /// <param name="arg1">Second argument</param>
        /// <param name="arg2">Third argument</param>
        /// <returns>Byte array representation of command and arguments</returns>
        public static byte[] CreatePacket<T0, T1, T2>(string cmd, T0 arg0, T1 arg1, T2 arg2)
        {
            List<byte> ret = new List<byte>(GetCommandBytes(cmd));
            ret.Add(ParameterDelimiterByte);

            string s = string.Format("{0:X}", arg0);
            ret.AddRange(Encoding.ASCII.GetBytes(s));
            ret.Add(ParameterDelimiterByte);

            s = string.Format("{0:X}", arg1);
            ret.AddRange(Encoding.ASCII.GetBytes(s));
            ret.Add(ParameterDelimiterByte);

            s = string.Format("{0:X}", arg2);
            ret.AddRange(Encoding.ASCII.GetBytes(s));

            ret.Add(PacketTerminatorByte);
            return ret.ToArray();
        }
    }
}
