//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: CreateCommands.cs $ $Revision: 26 $
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Text;
using W3C.Soap;

using Microsoft.Ccr.Core;
using Microsoft.Dss.Core.Attributes;
using Microsoft.Dss.ServiceModel.Dssp;
using Microsoft.Robotics.Services.IRobot.Roomba;

using roomba = Microsoft.Robotics.Services.IRobot.Roomba;
using stream = Microsoft.Robotics.Services.DssStream.Proxy;
using istream = Microsoft.Robotics.Services.IRobot.DssStream;
using ports = System.IO.Ports;

namespace Microsoft.Robotics.Services.IRobot.Create
{
    #region Create Enumerations

    /// <summary>
    /// Starts a Built-in demo on the iRobot Create.
    /// </summary>
    [DataContract]
    [Description ("Specifies a Create built-in demo script.")]
    public enum DemoMode
    {
        /// <summary>
        /// Stops the current demo
        /// </summary>
        AbortCurrentDemo = -1,
        /// <summary>
        /// Attempt to cover the entire room
        /// </summary>
        Cover = 0,
        /// <summary>
        /// Cover the entire room, seeking the dock
        /// </summary>
        CoverAndSeekDock = 1,
        /// <summary>
        /// Cover an area around the starting position by
        /// spiraling outward, then inward.
        /// </summary>
        SpotCover = 2,
        /// <summary>
        /// Search for a wall and then travel
        /// around the circumference of the room.
        /// </summary>
        Mouse = 3,
        /// <summary>
        /// Drive continuously in a figure 8 pattern.
        /// </summary>
        DriveFigureEight = 4,
        /// <summary>
        /// Drive forward when pushed from behind.  If Create
        /// hits an obstacle, it drives away from the obstacle.
        /// </summary>
        Wimp = 5,
        /// <summary>
        /// Create drives toward an iRobot Virtual Wall as long
        /// as the back and sides of the virtual wall receiver
        /// are blinded by black electrical tape.
        ///
        /// A Virtual Wall emits infrared signals that Create
        /// sees with its Omnidirectional Infrared Receiver,
        /// located on top of the bumper.
        ///
        /// If you want Create to home in on a Virtual Wall,
        /// cover all but a small opening in the front of the
        /// infrared receiver with black electrical tape.
        ///
        /// Create spins to locate a virtual wall, then drives
        /// toward it.  Once Create hits the wall or another
        /// obstacle, it stops.
        /// </summary>
        Home = 6,
        /// <summary>
        /// Idential to the Home demo, except Create drives
        /// into multiple virtual walls by bumping into one,
        /// turning around, driving to the next virtual wall,
        /// bumping into it and turning around to bump into
        /// the next virtual wall.
        /// </summary>
        Tag = 7,
        /// <summary>
        /// Create plays the notes of Pachelbel's Canon in
        /// sequence when cliff sensors are activated.
        /// </summary>
        Pachelbel = 8,
        /// <summary>
        /// Create plays a note of a chord for each of its four cliff sensors.  Select the chord using the bumper, as follows:
        /// No bumper: G major
        /// Right or left bumper: D major 7
        /// both bumpers (center): C major
        /// </summary>
        Banjo = 9,
    }

    /// <summary>
    /// Create Sensor Packets
    /// </summary>
    [DataContract]
    [Description ("Identifies a Sensor Packet (notification) to be returned.\n(Supported only for Create.)")]
    public enum CreateSensorPacket
    {
        /***************************************************/
        /* Combined Packets                                */
        /***************************************************/

        /// <summary>
        /// All Roomba Sensors, Pose and Power Packets
        /// <remarks>26 bytes</remarks>
        /// </summary>
        AllRoomba = 0,
        /// <summary>
        /// Roomba Bumps, Cliffs, and Walls Return Packet
        /// <remarks>10 bytes</remarks>
        /// </summary>
        AllBumpsCliffsAndWalls = 1,
        /// <summary>
        /// Pose Return Packet
        /// <remarks>6 bytes</remarks>
        /// </summary>
        AllPose = 2,
        /// <summary>
        /// Power Return Packet
        /// <remarks>10 bytes</remarks>
        /// </summary>
        AllPower = 3,
        /// <summary>
        /// Cliff Return Packet
        /// <remarks>14 bytes</remarks>
        /// </summary>
        AllCliffDetail = 4,
        /// <summary>
        /// Telemetry Return Packet
        /// <remarks>12 bytes</remarks>
        /// </summary>
        AllTelemetry = 5,
        /// <summary>
        /// All Create Sensor data
        /// <remarks>52 bytes</remarks>
        /// </summary>
        AllCreate = 6,


        /***************************************************/
        /* Individual sensors                              */
        /***************************************************/

        /// <summary>
        /// Bumps And Wheel Drops
        /// </summary>
        BumpsWheelDrops = 7,
        /// <summary>
        /// Wall
        /// </summary>
        Wall = 8,
        /// <summary>
        /// Cliff Left
        /// </summary>
        CliffLeft = 9,
        /// <summary>
        /// Cliff Front Left
        /// </summary>
        CliffFrontLeft = 10,
        /// <summary>
        /// Cliff Front Right
        /// </summary>
        CliffFrontRight = 11,
        /// <summary>
        /// Cliff Right
        /// </summary>
        CliffRight = 12,
        /// <summary>
        /// Virtual Wall
        /// </summary>
        VirtualWall = 13,
        /// <summary>
        /// Low Side Driver And Wheel Overcurrents
        /// </summary>
        MotorOvercurrents = 14,
        /// <summary>
        /// Unused15
        /// </summary>
        Unused15 = 15,
        /// <summary>
        /// Unused16
        /// </summary>
        Unused16 = 16,
        /// <summary>
        /// Infrared
        /// </summary>
        Infrared = 17,
        /// <summary>
        /// Buttons
        /// </summary>
        Buttons = 18,
        /// <summary>
        /// Distance
        /// </summary>
        Distance = 19,
        /// <summary>
        /// Angle
        /// </summary>
        Angle = 20,
        /// <summary>
        /// Charging State
        /// </summary>
        ChargingState = 21,
        /// <summary>
        /// Voltage
        /// </summary>
        Voltage = 22,
        /// <summary>
        /// Current
        /// </summary>
        Current = 23,
        /// <summary>
        /// Battery Temperature
        /// </summary>
        BatteryTemperature = 24,
        /// <summary>
        /// Battery Charge
        /// </summary>
        BatteryCharge = 25,
        /// <summary>
        /// Battery Capacity
        /// </summary>
        BatteryCapacity = 26,
        /// <summary>
        /// Wall Signal
        /// </summary>
        WallSignal = 27,
        /// <summary>
        /// Cliff Left Signal
        /// </summary>
        CliffLeftSignal = 28,
        /// <summary>
        /// Cliff Front Left Signal
        /// </summary>
        CliffFrontLeftSignal = 29,
        /// <summary>
        /// Cliff Front Right Signal
        /// </summary>
        CliffFrontRightSignal = 30,
        /// <summary>
        /// Cliff Right Signal
        /// </summary>
        CliffRightSignal = 31,
        /// <summary>
        /// Cargo Bay Digital Inputs
        /// </summary>
        CargoBayDigitalInputs = 32,
        /// <summary>
        /// Cargo Bay Analog Signal
        /// </summary>
        CargoBayAnalogSignal = 33,
        /// <summary>
        /// Charging Sources Available
        /// </summary>
        ChargingSourcesAvailable = 34,
        /// <summary>
        /// OI Mode
        /// </summary>
        OIMode = 35,
        /// <summary>
        /// Song Number
        /// </summary>
        SongNumber = 36,
        /// <summary>
        /// Song Playing
        /// </summary>
        SongPlaying = 37,
        /// <summary>
        /// Number Of Stream Packets
        /// </summary>
        NumberOfStreamPackets = 38,
        /// <summary>
        /// Requested Velocity
        /// </summary>
        RequestedVelocity = 39,
        /// <summary>
        /// Requested Radius
        /// </summary>
        RequestedRadius = 40,
        /// <summary>
        /// Requested Right Velocity
        /// </summary>
        RequestedRightVelocity = 41,
        /// <summary>
        /// Requested Left Velocity
        /// </summary>
        RequestedLeftVelocity = 42,
    }

    /// <summary>
    /// Cargo Bay Digital Inputs
    /// </summary>
    [DataContract]
    [Description("Identifies the flags (bit settings) for accessing the Create digital inputs.")]
    [Flags]
    public enum CargoBayDigitalInputs
    {
        /// <summary>
        /// Digital Input 0
        /// </summary>
        Pin17 = 0x01,

        /// <summary>
        /// Digital Input 1
        /// </summary>
        Pin5 = 0x02,

        /// <summary>
        /// Digital Input 2
        /// </summary>
        Pin18 = 0x04,

        /// <summary>
        /// Digital Input 3
        /// </summary>
        Pin6 = 0x08,

        /// <summary>
        /// Baud Rate Change
        /// </summary>
        Pin15 = 0x10,

        /// <summary>
        /// Not Applicable
        /// </summary>
        NA6 = 0x20,

        /// <summary>
        /// Not Applicable
        /// </summary>
        NA7 = 0x40,

        /// <summary>
        /// Not Applicable
        /// </summary>
        NA8 = 0x80,

    }

    /// <summary>
    /// Charging Sources Available
    /// </summary>
    [DataContract]
    [Description("Identifies the flags (bit settings) for accessing the robot's charging source.")]
    [Flags]
    public enum ChargingSourcesAvailable
    {
        /// <summary>
        /// Internal Charger
        /// </summary>
        InternalCharger = 0x01,

        /// <summary>
        /// Home Base
        /// </summary>
        HomeBase = 0x02,

        /// <summary>
        /// Unsupported Charging Source
        /// </summary>
        UnsupportedBit3 = 0x04,

        /// <summary>
        /// Unsupported Charging Source
        /// </summary>
        UnsupportedBit4 = 0x08,

        /// <summary>
        /// Not Applicable
        /// </summary>
        NA5 = 0x10,

        /// <summary>
        /// Not Applicable
        /// </summary>
        NA6 = 0x20,

        /// <summary>
        /// Not Applicable
        /// </summary>
        NA7 = 0x40,

        /// <summary>
        /// Not Applicable
        /// </summary>
        NA8 = 0x80,
    }

    /// <summary>
    /// Create Physical Buttons
    /// </summary>
    [DataContract]
    [Flags]
    [Description("Identifies the flags (bit settings) for accessing the Create physical buttons.")]
    public enum ButtonsCreate
    {
        /// <summary>
        /// No Buttons Pressed
        /// </summary>
        Off = 0x00,
        /// <summary>
        /// Create Play Button
        /// </summary>
        Play = 0x01,
        /// <summary>
        /// Not Defined Flag 0x02
        /// </summary>
        Bit2 = 0x02,
        /// <summary>
        /// Create Advance Button
        /// </summary>
        Advance = 0x04,
        /// <summary>
        /// Not Defined Flag 0x08
        /// </summary>
        Bit4 = 0x08,
        /// <summary>
        /// Not Defined Flag 0x10
        /// </summary>
        Bit5 = 0x10,
        /// <summary>
        /// Not Defined Flag 0x20
        /// </summary>
        Bit6 = 0x20,
        /// <summary>
        /// Not Defined Flag 0x40
        /// </summary>
        Bit7 = 0x40,
        /// <summary>
        /// Not Defined Flag 0x80
        /// </summary>
        Bit8 = 0x80,

    }


    /// <summary>
    /// Create Scripting Wait Event
    /// </summary>
    [DataContract]
    [Description("Identifies the value for a Create Scripting Wait event.")]
    public enum WaitEvent
    {
        /// <summary>
        /// No Event
        /// </summary>
        None = 0,
        /// <summary>
        /// Wheel Drop
        /// </summary>
        WheelDrop = 1,
        /// <summary>
        /// No Wheel Drop
        /// </summary>
        NoWheelDrop = 255,
        /// <summary>
        /// Front Wheel Drop
        /// </summary>
        FrontWheelDrop = 2,
        /// <summary>
        /// No Front Wheel Drop
        /// </summary>
        NoFrontWheelDrop = 254,
        /// <summary>
        /// Left Wheel Drop
        /// </summary>
        LeftWheelDrop = 3,
        /// <summary>
        /// No Left Wheel Drop
        /// </summary>
        NoLeftWheelDrop = 253,
        /// <summary>
        /// Right Wheel Drop
        /// </summary>
        RightWheelDrop = 4,
        /// <summary>
        /// No Right Wheel Drop
        /// </summary>
        NoRightWheelDrop = 252,
        /// <summary>
        /// Bump
        /// </summary>
        Bump = 5,
        /// <summary>
        /// No Bump
        /// </summary>
        NoBump = 251,
        /// <summary>
        /// Left Bump
        /// </summary>
        LeftBump = 6,
        /// <summary>
        /// No Left Bump
        /// </summary>
        NoLeftBump = 250,
        /// <summary>
        /// Right Bump
        /// </summary>
        RightBump = 7,
        /// <summary>
        /// No Right Bump
        /// </summary>
        NoRightBump = 249,
        /// <summary>
        /// Virtual Wall
        /// </summary>
        VirtualWall = 8,
        /// <summary>
        /// No Virtual Wall
        /// </summary>
        NoVirtualWall = 248,
        /// <summary>
        /// Wall
        /// </summary>
        Wall = 9,
        /// <summary>
        /// No Wall
        /// </summary>
        NoWall = 247,
        /// <summary>
        /// Cliff
        /// </summary>
        Cliff = 10,
        /// <summary>
        /// No Cliff
        /// </summary>
        NoCliff = 246,
        /// <summary>
        /// Left Cliff
        /// </summary>
        LeftCliff = 11,
        /// <summary>
        /// No Left Cliff
        /// </summary>
        NoLeftCliff = 245,
        /// <summary>
        /// Front Left Cliff
        /// </summary>
        FrontLeftCliff = 12,
        /// <summary>
        /// No Front Left Cliff
        /// </summary>
        NoFrontLeftCliff = 244,
        /// <summary>
        /// Front Right Cliff
        /// </summary>
        FrontRightCliff = 13,
        /// <summary>
        /// No Front Right Cliff
        /// </summary>
        NoFrontRightCliff = 243,
        /// <summary>
        /// Right Cliff
        /// </summary>
        RightCliff = 14,
        /// <summary>
        /// No Right Cliff
        /// </summary>
        NoRightCliff = 242,
        /// <summary>
        /// Home Base
        /// </summary>
        HomeBase = 15,
        /// <summary>
        /// No Home Base
        /// </summary>
        NoHomeBase = 241,
        /// <summary>
        /// Advance Button
        /// </summary>
        AdvanceButton = 16,
        /// <summary>
        /// No Advance Button
        /// </summary>
        NoAdvanceButton = 240,
        /// <summary>
        /// Play Button
        /// </summary>
        PlayButton = 17,
        /// <summary>
        /// No Play Button
        /// </summary>
        NoPlayButton = 239,
        /// <summary>
        /// Digital Input 0
        /// </summary>
        DigitalInput0 = 18,
        /// <summary>
        /// No Digital Input 0
        /// </summary>
        NoDigitalInput0 = 238,
        /// <summary>
        /// Digital Input 1
        /// </summary>
        DigitalInput1 = 19,
        /// <summary>
        /// No Digital Input 1
        /// </summary>
        NoDigitalInput1 = 237,
        /// <summary>
        /// Digital Input 2
        /// </summary>
        DigitalInput2 = 20,
        /// <summary>
        /// No Digital Input 2
        /// </summary>
        NoDigitalInput2 = 236,
        /// <summary>
        /// Digital Input 3
        /// </summary>
        DigitalInput3 = 21,
        /// <summary>
        /// No Digital Input 3
        /// </summary>
        NoDigitalInput3 = 235,
        /// <summary>
        /// Mode is Passive
        /// </summary>
        ModePassive = 22,
        /// <summary>
        /// Mode is not Passive
        /// </summary>
        NoModePassive = 234,
    }

    #endregion

    #region Create Commands

    /// <summary>
    /// Starts a Demo.
    /// <remarks>Works only with the iRobot Create.</remarks>
    /// </summary>
    [DataContract]
    [DataMemberConstructor]
    public class CmdDemo : RoombaCommand
    {
        /// <summary>
        /// Start a Demo
        /// </summary>
        public CmdDemo() : base(RoombaCommandCode.Demo, true)
        {
            this.DemoMode = DemoMode.AbortCurrentDemo;

            // Moving to passive mode.  Make sure we don't maintain safe or full mode.
            this.MaintainMode = RoombaMode.NotSpecified;
        }

        /// <summary>
        /// Start the specified demo.
        /// </summary>
        /// <param name="demoMode"></param>
        public CmdDemo(DemoMode demoMode) :
            base(RoombaCommandCode.Demo, true)
        {
            this.DemoMode = demoMode;

            // Moving to passive mode.  Make sure we don't maintain safe or full mode.
            this.MaintainMode = RoombaMode.NotSpecified;
        }

        /// <summary>
        /// The iRobot Create Demo Mode.
        /// </summary>
        [DataMember, DataMemberConstructor(Order = 1)]
        [Description("Specifies the Create demo mode.")]
        public DemoMode DemoMode
        {
            get { return (DemoMode)this.Data[0]; }
            set { this.Data[0] = (byte)value; }
        }

        /// <summary>
        /// The string representation for this command.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("Demo(DemoMode={0})", this.DemoMode);
        }

    }

    /// <summary>
    /// This command lets you control the three low side drivers
    /// with variable power. With each data byte, you specify the
    /// PWM duty cycle for the low side driver (max 128). For
    /// example, if you want to control a driver with 25% of battery
    /// voltage, choose a duty cycle of 128 * 25% = 32.
    /// <remarks>Available in Safe or Full mode</remarks>
    /// </summary>
    [DataContract]
    public class CmdPWMLowSideDrivers : RoombaCommand
    {

        /// <summary>
        /// Control the three low side drivers with variable power.
        /// </summary>
        public CmdPWMLowSideDrivers() : base(RoombaCommandCode.PWMLowSideDrivers, true) { }

        /// <summary>
        /// Control the three low side drivers with variable power.
        /// </summary>
        /// <param name="digitalOut0">duty cycle (0 to 128)</param>
        /// <param name="digitalOut1">duty cycle (0 to 128)</param>
        /// <param name="digitalOut2">duty cycle (0 to 128)</param>
        public CmdPWMLowSideDrivers(int digitalOut0, int digitalOut1, int digitalOut2)
            : base(RoombaCommandCode.PWMLowSideDrivers, true)
        {
            this.DigitalOut0 = digitalOut0;
            this.DigitalOut1 = digitalOut1;
            this.DigitalOut2 = digitalOut2;
        }

        /// <summary>
        /// Digital Out 0 (Pin 19)
        /// </summary>
        [DataMember]
        [DataMemberConstructor(Order = 1)]
        [Description("Identifies the digital output port 0 (Pin 19).")]
        public int DigitalOut0
        {
            get { return (int)this.Data[2];}
            set { this.Data[2] = (byte)value; }
        }

        /// <summary>
        /// Digital Out 1 (Pin 7)
        /// </summary>
        [DataMember]
        [DataMemberConstructor(Order = 2)]
        [Description("Identifies the digital output port 1 (Pin 7).")]
        public int DigitalOut1
        {
            get { return (int)this.Data[1]; }
            set { this.Data[1] = (byte)value; }
        }

        /// <summary>
        /// Digital Out 2 (Pin 20)
        /// </summary>
        [DataMember]
        [DataMemberConstructor(Order = 3)]
        [Description("Identifies the digital output port 2 (Pin 20).")]
        public int DigitalOut2
        {
            get { return (int)this.Data[0]; }
            set { this.Data[0] = (byte)value; }
        }

        /// <summary>
        /// The string representation for this command.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("PWMLowSideDrivers(DigitalOut0={0},DigitalOut1={1},DigitalOut2={2})", this.DigitalOut0, this.DigitalOut1, this.DigitalOut2);
        }

    }


    /// <summary>
    /// This command lets you control the forward and backward motion of Create's drive
    /// wheels independently.  A positive velocity makes that wheel drive forward,
    /// while a negative velocity makes it drive backward.
    /// </summary>
    [DataContract]
    public class CmdDriveDirect : RoombaCommand
    {

        /// <summary>
        /// Control the forward and backward motion of Create's drive wheels.
        /// </summary>
        public CmdDriveDirect() : base(RoombaCommandCode.DriveDirect, true) { }

        /// <summary>
        /// Control the forward and backward motion of Create's drive wheels.
        /// </summary>
        /// <param name="rightVelocity">mm/s (-500 to +500)</param>
        /// <param name="leftVelocity">mm/s (-500 to +500)</param>
        public CmdDriveDirect(int rightVelocity, int leftVelocity)
            : base(RoombaCommandCode.DriveDirect, true)
        {
            this.RightVelocity = rightVelocity;
            this.LeftVelocity = leftVelocity;
        }

        /// <summary>
        /// Right Velocity (-500 to +500 mm/s)
        /// </summary>
        [DataMember]
        [Description("Specifies the right wheel velocity.\n(-500 to +500 mm/s)")]
        [DataMemberConstructor(Order = 1)]
        public int RightVelocity
        {
            get { return ByteArray.BigEndianGetShort(this.Data, 0); }
            set { ByteArray.BigEndianSetShort(this.Data, 0, value); }
        }

        /// <summary>
        /// Left Velocity (-500 to +500 mm/s)
        /// </summary>
        [DataMember]
        [Description("Specifies the left wheel velocity.\n(-500 to +500 mm/s)")]
        [DataMemberConstructor(Order = 2)]
        public int LeftVelocity
        {
            get { return ByteArray.BigEndianGetShort(this.Data, 2); }
            set { ByteArray.BigEndianSetShort(this.Data, 2, value); }
        }

        /// <summary>
        /// The string representation for this command.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("DriveDirect(RightVelocity={0},LeftVelocity={1})", this.RightVelocity, this.LeftVelocity);
        }
    }


    /// <summary>
    /// This command controls the state of the 3 digital output
    /// pins on the 25 pin Cargo Bay Connector. The digital outputs
    /// can provide up to 20 mA of current.
    /// </summary>
    [DataContract]
    [DataMemberConstructor]
    public class CmdDigitalOutputs : RoombaCommand
    {
        /// <summary>
        /// Controls the state of the 3 digital output pins.
        /// </summary>
        public CmdDigitalOutputs()
            : base(RoombaCommandCode.DigitalOutputs, true)
        {
            this.Data = new byte[1];
        }

        /// <summary>
        /// Controls the state of the 3 digital output pins.
        /// </summary>
        /// <param name="digitalOut0">on = 5v</param>
        /// <param name="digitalOut1">on = 5v</param>
        /// <param name="digitalOut2">on = 5v</param>
        public CmdDigitalOutputs(bool digitalOut0, bool digitalOut1, bool digitalOut2)
            : base(RoombaCommandCode.DigitalOutputs, true)
        {
            this.Data = new byte[1];
            DigitalOut0 = digitalOut0;
            DigitalOut1 = digitalOut1;
            DigitalOut2 = digitalOut2;
        }

        /// <summary>
        /// Digital Output 0 (pin 19)
        /// </summary>
        [DataMember, DataMemberConstructor(Order=1)]
        [Description("Identifies digital output 0 (pin 19).")]
        public bool DigitalOut0
        {
            get
            {
                // return the value of bit-1
                return (this.Data == null) ? false : ((this.Data[0] & 0x01) == 0x01);
            }
            set
            {
                // Mask bit-1, then bitwise or bit-1 with this flag
                this.Data[0] = (byte)((this.Data[0] & (byte)0xFE) | (value ? 0x01 : 0x00));
            }
        }

        /// <summary>
        /// Digital Output 1 (pin 7)
        /// </summary>
        [DataMember, DataMemberConstructor(Order=2)]
        [Description("Identifies digital output 1 (pin 7).")]
        public bool DigitalOut1
        {
            get
            {
                // return the value of bit-2
                return (this.Data == null) ? false : ((this.Data[0] & 0x02) == 0x02);
            }
            set
            {
                // Mask bit-2, then bitwise or bit-2 with this flag
                this.Data[0] = (byte)((this.Data[0] & 0xFD) | (value ? 0x02 : 0x00));
            }
        }

        /// <summary>
        /// Digital Output 2 (pin 20)
        /// </summary>
        [DataMember, DataMemberConstructor(Order=1)]
        [Description("Identifies digital output 2 (pin 20).")]
        public bool DigitalOut2
        {
            get
            {
                // return the value of bit-3
                return (this.Data == null) ? false : ((this.Data[0] & 0x04) == 0x04);
            }
            set
            {
                // Mask bit-3, then bitwise or bit-3 with this flag
                this.Data[0] = (byte)((this.Data[0] & 0xFB) | (value ? 0x04 : 0x00));
            }
        }

        /// <summary>
        /// The string representation for this command.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("DigitalOutputs(DigitalOut0={0},DigitalOut1={1},DigitalOut2={2})", this.DigitalOut0, this.DigitalOut1, this.DigitalOut2);
        }

    }


    /// <summary>
    /// Request a stream of sensor data.
    /// Works only with the iRobot Create.
    /// </summary>
    [DataContract]
    [DataMemberConstructor]
    public class CmdStream : RoombaCommand
    {
        private int _count;

        /// <summary>
        /// Request a stream of data.
        /// </summary>
        public CmdStream()
            : base(RoombaCommandCode.Stream, true)
        {
            _count = 0;
        }

        /// <summary>
        /// Request a stream of data.
        /// </summary>
        /// <param name="queryList"></param>
        public CmdStream(List<CreateSensorPacket> queryList)
            : base(RoombaCommandCode.Stream, true)
        {
            _count = queryList.Count;
            this.Data = new byte[_count + 1];
            this.Data[0] = (byte)_count;
            for (int ix = 0; ix < _count; ix++)
                this.Data[ix + 1] = (byte)queryList[ix];

        }

        /// <summary>
        /// Retrieve the list of stream data
        /// </summary>
        /// <returns></returns>
        public List<CreateSensorPacket> GetList()
        {
            List<CreateSensorPacket> sensorList = new List<CreateSensorPacket>();
            if (this.Data == null || this.Data.Length != (this.Data[0] + 1))
                return sensorList;
            int count = this.Data[0];
            for (int ix = 1; ix <= count; ix++)
            {
                CreateSensorPacket sensor = (CreateSensorPacket)this.Data[ix];
                sensorList.Add(sensor);
            }
            return sensorList;
        }

        /// <summary>
        /// The string representation for this command.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("Stream(SensorList={0})", IRobotUtility.SensorListToString(this.GetList()));
        }

    }

    /// <summary>
    /// Query for a list of sensors.
    /// Works only with the iRobot Create.
    /// </summary>
    [DataContract]
    [DataMemberConstructor]
    public class CmdQueryList : RoombaCommand
    {
        private int _count;
        private int _returnPacketSize;
        private readonly List<CreateSensorPacket> _queryList = null;

        /// <summary>
        /// Query for a list of sensors.
        /// </summary>
        public CmdQueryList()
            : base(RoombaCommandCode.QueryList, true)
        {
            _count = 0;
            _returnPacketSize = 0;
        }

        /// <summary>
        /// Query for a list of sensors.
        /// </summary>
        /// <param name="queryList"></param>
        public CmdQueryList(List<CreateSensorPacket> queryList)
            : base(RoombaCommandCode.QueryList, true)
        {
            _queryList = queryList;
            _count = queryList.Count;
            this.Data = new byte[_count+1];
            this.Data[0] = (byte)_count;
            for (int ix = 0; ix < _count; ix++)
                this.Data[ix + 1] = (byte)queryList[ix];
            _returnPacketSize = IRobotUtility.ReturnListSize(queryList);
        }

        /// <summary>
        /// CmdQueryList expects data back from the iRobot
        /// </summary>
        /// <returns></returns>
        public override int ExpectedResponseBytes()
        {
            return _returnPacketSize;
        }

        /// <summary>
        /// The initialized query list
        /// </summary>
        /// <returns></returns>
        public List<CreateSensorPacket> QueryList()
        {
            return _queryList;
        }

        /// <summary>
        /// The string representation for this command.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("QueryList(SensorList={0})", IRobotUtility.SensorListToString(this.QueryList()));
        }

    }


    /// <summary>
    /// Pause/Resume a stream of sensor data.
    /// Works only with the iRobot Create.
    /// </summary>
    [DataContract]
    [DataMemberConstructor]
    public class CmdStreamPauseResume : RoombaCommand
    {
        /// <summary>
        /// Pause or Resume Stream Data.
        /// </summary>
        public CmdStreamPauseResume()
            : base(RoombaCommandCode.StreamPauseResume, true)
        {
            this.Data = new byte[1];
        }

        /// <summary>
        /// Pause or Resume Stream Data.
        /// </summary>
        /// <param name="streamActive"></param>
        public CmdStreamPauseResume(bool streamActive)
            : base(RoombaCommandCode.StreamPauseResume, true)
        {
            this.Data = new byte[1];
            this.Data[0] = (byte)(streamActive ? 1: 0);
        }

        /// <summary>
        /// The requested stream state
        /// </summary>
        [DataMember]
        [Description("Identifies if data streaming is active.")]
        public bool StreamActive
        {
            get { return (this.Data == null) ? false : (this.Data[0] == 1 ? true : false); }
            set{ this.Data[0] = (byte)(value ? 1 : 0); }
        }

        /// <summary>
        /// The string representation for this command.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("StreamPauseResume(StreamActive={0})", this.StreamActive);
        }

    }

    /// <summary>
    /// This command sends the requested byte out of low side
    /// driver 1 (pin 23 on the Cargo Bay Connector), using the
    /// format expected by iRobot Create�s IR receiver. You must
    /// use a preload resistor (suggested value: 100 ohms) in
    /// parallel with the IR LED and its resistor in order turn it on.
    ///
    /// Works only with the iRobot Create.
    /// <remarks>Valid Range: 0 - 255</remarks>
    /// </summary>
    [DataContract]
    [DataMemberConstructor]
    public class CmdSendIR : RoombaCommand
    {
        /// <summary>
        /// Sends an IR Command out pin 23 on the Cargo Bay Connector.
        /// </summary>
        public CmdSendIR()
            : base(RoombaCommandCode.SendIR, true)
        {
            this.Data = new byte[1];
        }

        /// <summary>
        /// Sends an IR Command out pin 23 on the Cargo Bay Connector.
        /// </summary>
        /// <param name="irCode"></param>
        public CmdSendIR(RemoteIR irCode)
            : base(RoombaCommandCode.SendIR, true)
        {
            this.Data = new byte[1];
            this.Data[0] = (byte)irCode;
        }

        /// <summary>
        /// The requested IR Code
        /// </summary>
        [DataMember, DataMemberConstructor(Order=1)]
        [Description("Identifies an IR code.")]
        public RemoteIR IRCode
        {
            get { return (RemoteIR)this.Data[0]; }
            set { this.Data[0] = (byte)value; }
        }

        /// <summary>
        /// The string representation for this command.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("SendIR(IRCode={0})", this.IRCode);
        }

    }


    /// <summary>
    /// Define an iRobot Create Script.
    /// This command defines a script to be played later. A script
    /// consists of OI commands and can be up to 100 bytes long.
    /// There is no flow control, but �wait� commands (see below)
    /// cause Create to hold its current state until the specified
    /// event is detected.
    /// Works only with the iRobot Create.
    /// </summary>
    [DataContract]
    [DataMemberConstructor]
    [Description("Defines an iRobot Create Script.\nUse the iRobot Create Scripting Engine.")]
    public class CmdDefineScript : RoombaCommand
    {
        private int _count;
        private List<RoombaCommand> _scriptedCommands;
        private int _lastCommandCount = -1;
        private int _scriptResponseBytes = 0;

        /// <summary>
        /// Define an iRobot Create Script.
        /// </summary>
        public CmdDefineScript()
            : base(RoombaCommandCode.DefineScript, true)
        {
            _count = 0;
            _scriptedCommands = new List<RoombaCommand>();
        }

        /// <summary>
        /// Define an iRobot Create Script.
        /// </summary>
        /// <param name="scriptedCommands"></param>
        public CmdDefineScript(List<RoombaCommand> scriptedCommands)
            : base(RoombaCommandCode.DefineScript, true)
        {
            _scriptedCommands = new List<RoombaCommand>();
            SetScriptedCommands(scriptedCommands);
        }

        /// <summary>
        /// Set the scripted commands
        /// </summary>
        /// <param name="scriptedCommands"></param>
        private void SetScriptedCommands(List<RoombaCommand> scriptedCommands)
        {
            List<RoombaCommand> cmds = new List<RoombaCommand>();
            byte[] data = new byte[1];
            int count = 0;
            int playResponseBytes = 0;

            foreach (RoombaCommand cmd in scriptedCommands)
            {
                byte[] nextData = cmd.GetPacket();
                if (count + nextData.Length <= 100)
                {
                    count += nextData.Length;
                    data = ByteArray.Combine(data, nextData);
                    playResponseBytes += cmd.ExpectedResponseBytes();
                    cmds.Add(cmd);
                }
            }
            data[0] = (byte)count;

            // Assign to class members after
            // we are done validating the data.
            base.Data = data;
            this._scriptedCommands = cmds;
            this._count = count;
            this._lastCommandCount = cmds.Count;
            this._scriptResponseBytes = playResponseBytes;
        }

        /// <summary>
        /// The Commands which will be executed in order.
        /// </summary>
        public List<RoombaCommand> ScriptedCommands
        {
            get { return _scriptedCommands; }
            set { SetScriptedCommands(value); }
        }

        /// <summary>
        /// The count of script bytes
        /// </summary>
        public int Count
        {
            get { return _count; }
        }

        /// <summary>
        /// Returns the number of response bytes expected
        /// when the script is played.
        /// </summary>
        public int ScriptResponseBytes
        {
            get { return _scriptResponseBytes; }
        }

        /// <summary>
        /// If the number of items in the list has changed,
        /// Data is recalulated before being retrieved.
        /// </summary>
        public override byte[] Data
        {
            get
            {
                if (_lastCommandCount != _scriptedCommands.Count && _scriptedCommands.Count > 0)
                    SetScriptedCommands(_scriptedCommands);

                return base.Data;
            }
            set
            {
                ExtractScriptCommands(value);
            }
        }

        /// <summary>
        /// The script payload.  Use the iRobot Create Scripting Engine.
        /// </summary>
        [DataMember]
        [Description("Identifies the script payload.\nUse the iRobot Create Scripting Engine.")]
        public byte[] PacketData
        {
            get { return base.Data; }
            set
            {
                ExtractScriptCommands(value);
            }
        }

        private void ExtractScriptCommands(byte[] value)
        {
            base.Data = value;
            _scriptedCommands.Clear();
            roomba.RoombaCommand cmd;
            _count = this.Data[0];
            if ((_count + 1) == this.Data.Length)
            {
                int ix = 1;
                while (ix < _count)
                {
                    ix = IRobotUtility.ReturnIRobotCommand(value, ix, out cmd);
                    if (cmd != null)
                        _scriptedCommands.Add(cmd);
                }
            }
            _lastCommandCount = _scriptedCommands.Count;
        }

        /// <summary>
        /// The string representation for this command.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("DefineScript(...)");
        }

    }


    /// <summary>
    /// Play the previously defined iRobot Create script.
    /// This command loads a previously defined OI script into the
    /// serial input queue for playback.
    /// </summary>
    [DataContract]
    [DataMemberConstructor]
    public class CmdPlayScript : RoombaCommand
    {
        private int _scriptResponseBytes = 0;

        /// <summary>
        /// Play the previously defined iRobot Create script.
        /// </summary>
        public CmdPlayScript()
            : base(RoombaCommandCode.PlayScript, true) { }

        /// <summary>
        /// Play the previously defined iRobot Create script.
        /// </summary>
        public CmdPlayScript(int scriptResponseBytes)
            : base(RoombaCommandCode.PlayScript, true)
        {
            _scriptResponseBytes = scriptResponseBytes;
        }

        /// <summary>
        /// Play the previously defined iRobot Create script.
        /// </summary>
        public CmdPlayScript(int scriptResponseBytes, int timeoutMs)
            : base(RoombaCommandCode.PlayScript, true)
        {
            _scriptResponseBytes = scriptResponseBytes;
            base.CmdTimeoutMs = timeoutMs;
        }

        /// <summary>
        /// The script being played may return data.
        /// This needs to be passed in to CmdPlayScript(expectedResponseBytes),
        /// or by setting ScriptResponseBytes.
        /// </summary>
        /// <returns></returns>
        public override int ExpectedResponseBytes()
        {
            return _scriptResponseBytes;
        }

        /// <summary>
        /// The expected number of response bytes returned when the script is played.
        /// </summary>
        [DataMember, DataMemberConstructor(Order = 1)]
        [Description("Indicates the expected number of response bytes returned when a script is played.")]
        public int ScriptResponseBytes
        {
            get { return _scriptResponseBytes; }
            set { _scriptResponseBytes = value; }
        }

        /// <summary>
        /// The number of ms to wait before cancelling the script.
        /// <remarks>0 - Do not cancel</remarks>
        /// </summary>
        [DataMember, DataMemberConstructor(Order = 2)]
        [Description("Indicates the number of milliseconds to wait before cancelling the script.")]
        public int TimeoutMs
        {
            get { return base.CmdTimeoutMs; }
            set { base.CmdTimeoutMs = value; }
        }

        /// <summary>
        /// The string representation for this command.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("PlayScript(ScriptResponseBytes={0},TimeoutMs={1})", this.ScriptResponseBytes, this.TimeoutMs);
        }

    }


    /// <summary>
    /// Show the previously defined iRobot Create Script.
    /// This command returns the values of a previously stored
    /// script, starting with the number of bytes in the script and
    /// followed by the script�s commands and data bytes. It first
    /// halts the sensor stream, if one has been started with a
    /// Stream or Pause/Resume Stream command. To restart the
    /// stream, send Pause/Resume Stream (opcode 150).
    /// </summary>
    [DataContract]
    [DataMemberConstructor]
    public class CmdShowScript : RoombaCommand
    {
        /// <summary>
        /// Show the previously defined iRobot Create Script.
        /// </summary>
        public CmdShowScript()
            : base(RoombaCommandCode.ShowScript, true) { }

        /// <summary>
        /// The string representation for this command.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("ShowScript()");
        }

    }


    /// <summary>
    /// This command causes Create to wait for the specified time.
    /// During this time, Create�s state does not change, nor does
    /// it react to any inputs, serial or otherwise.
    ///
    /// Works only with the iRobot Create.
    /// <remarks>The SCI may be in any mode to accept this
    /// command. This command does not change the mode.</remarks>
    /// <remarks>Valid Range: 0.0 - 25.5</remarks>
    /// </summary>
    [DataContract]
    [DataMemberConstructor]
    public class CmdWaitTime : RoombaCommand
    {
        /// <summary>
        /// Causes Create to wait for the specified time.
        /// </summary>
        public CmdWaitTime()
            : base(RoombaCommandCode.WaitTime, true)
        {
            this.Data = new byte[1];
        }

        /// <summary>
        /// Causes Create to wait for the specified time.
        /// </summary>
        /// <param name="seconds">0.0 - 25.5</param>
        public CmdWaitTime(double seconds)
            : base(RoombaCommandCode.WaitTime, true)
        {
            this.Data = new byte[1];
            this.Seconds = seconds;
        }

        /// <summary>
        /// The number of seconds to wait.
        /// <remarks>0 - 25.5 seconds in 1/10 second increments with a 15ms resolution</remarks>
        /// </summary>
        [DataMember]
        [Description("Specifies the number of seconds to wait.\n(0 - 25.5 seconds in 1/10 second increments.)")]
        public double Seconds
        {
            get
            {
                return ((double)this.Data[0]) / 10.0;
            }
            set
            {
                if (value > 25.5)
                    this.Data[0] = 255;
                else if (value < 0)
                    this.Data[0] = 0;
                else
                    this.Data[0] = (byte)(Math.Round(value * 10.0, 0));
            }
        }

        /// <summary>
        /// The string representation for this command.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("WaitTime(Seconds={0})", this.Seconds);
        }
    }


    /// <summary>
    /// This command causes iRobot Create to wait until it has
    /// traveled the specified distance in mm. When Create travels
    /// forward, the distance is incremented. When Create travels
    /// backward, the distance is decremented. If the wheels
    /// are passively rotated in either direction, the distance is
    /// incremented. Until Create travels the specified distance,
    /// its state does not change, nor does it react to any inputs,
    /// serial or otherwise.
    ///
    /// Works only with the iRobot Create.
    /// <remarks>The SCI may be in any mode to accept this
    /// command. This command does not change the mode.</remarks>
    /// <remarks>Valid Range: -32767 - 32768</remarks>
    /// </summary>
    [DataContract]
    [DataMemberConstructor]
    public class CmdWaitDistance : RoombaCommand
    {
        /// <summary>
        /// Causes Create to wait until the wheels travel
        /// the specified distance in mm.
        /// </summary>
        public CmdWaitDistance()
            : base(RoombaCommandCode.WaitDistance, true)
        {
            this.Data = new byte[2];
        }

        /// <summary>
        /// Causes Create to wait until the wheels travel
        /// the specified distance in mm.
        /// </summary>
        /// <param name="distance"></param>
        public CmdWaitDistance(int distance)
            : base(RoombaCommandCode.WaitDistance, true)
        {
            this.Data = new byte[2];
            this.Distance = distance;
        }

        /// <summary>
        /// The distance to wait (mm).
        /// <remarks>-32767 - 32768</remarks>
        /// </summary>
        [DataMember]
        [Description("Specifies the distance to wait (mm).")]
        public int Distance
        {
            get { return ByteArray.BigEndianGetShort(this.Data, 0); }
            set { ByteArray.BigEndianSetShort(this.Data, 0, value); }
        }

        /// <summary>
        /// The string representation for this command.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("WaitDistance(Distance={0})", this.Distance);
        }
    }


    /// <summary>
    /// This command causes Create to wait until it has rotated
    /// through specified angle in degrees. When Create turns
    /// counterclockwise, the angle is incremented. When Create
    /// turns clockwise, the angle is decremented. Until Create
    /// turns through the specified angle, its state does not change,
    /// nor does it react to any inputs, serial or otherwise.
    ///
    /// Works only with the iRobot Create.
    /// <remarks>The SCI may be in any mode to accept this
    /// command. This command does not change the mode.</remarks>
    /// <remarks>This command resets the angle variable that is
    /// returned in Sensors</remarks>
    /// <remarks>Valid Range: -32767 - 32768</remarks>
    /// </summary>
    [DataContract]
    [DataMemberConstructor]
    public class CmdWaitAngle : RoombaCommand
    {
        /// <summary>
        /// Causes Create to wait until the wheels rotate through the specified angle in degrees.
        /// </summary>
        public CmdWaitAngle()
            : base(RoombaCommandCode.WaitAngle, true)
        {
            this.Data = new byte[2];
        }

        /// <summary>
        /// Causes Create to wait until the wheels rotate through the specified angle in degrees.
        /// </summary>
        /// <param name="angle"></param>
        public CmdWaitAngle(int angle)
            : base(RoombaCommandCode.WaitAngle, true)
        {
            this.Data = new byte[2];
            this.Angle = angle;
        }

        /// <summary>
        /// The angle to wait (degrees).
        /// <remarks>-32767 - 32768</remarks>
        /// </summary>
        [DataMember]
        [Description("Specifies the angle to wait (degrees).")]
        public int Angle
        {
            get { return ByteArray.BigEndianGetShort(this.Data, 0); }
            set { ByteArray.BigEndianSetShort(this.Data, 0, value); }
        }

        /// <summary>
        /// The string representation for this command.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("WaitAngle(Angle={0})", this.Angle);
        }

    }


    /// <summary>
    /// This command causes Create to wait until it detects the
    /// specified event. Until the specified event is detected,
    /// Create�s state does not change, nor does it react to any
    /// inputs, serial or otherwise.
    ///
    /// Works only with the iRobot Create.
    /// <remarks>The SCI may be in any mode to accept this
    /// command. This command does not change the mode.</remarks>
    /// <remarks>Valid Range: 0.0 - 25.5</remarks>
    /// </summary>
    [DataContract]
    [DataMemberConstructor]
    public class CmdWaitEvent : RoombaCommand
    {
        /// <summary>
        /// Causes Create to wait until the specified event occurs.
        /// </summary>
        public CmdWaitEvent()
            : base(RoombaCommandCode.WaitEvent, true)
        {
            this.Data = new byte[1];
        }

        /// <summary>
        /// Causes Create to wait until the specified event occurs.
        /// </summary>
        /// <param name="waitEvent"></param>
        public CmdWaitEvent(WaitEvent waitEvent)
            : base(RoombaCommandCode.WaitEvent, true)
        {
            this.Data = new byte[1];
            this.WaitEvent = waitEvent;
        }

        /// <summary>
        /// The Event to wait for.
        /// </summary>
        [DataMember]
        [Description("Specifies the Create event to wait for.")]
        public WaitEvent WaitEvent
        {
            get { return (WaitEvent)this.Data[0]; }
            set { this.Data[0] = (byte)(value); }
        }

        /// <summary>
        /// The string representation for this command.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("WaitEvent(WaitEvent={0})", this.WaitEvent);
        }
    }

    #endregion

    #region Create Response Packets

    /// <summary>
    /// An individual Sensor Value
    /// </summary>
    [DataContract]
    public class SensorValue
    {
        /// <summary>
        /// An individual Sensor Value
        /// </summary>
        public SensorValue()
        {
        }

        /// <summary>
        /// An individual Sensor Value
        /// </summary>
        public SensorValue(CreateSensorPacket sensor, int value)
        {
            this.Sensor = sensor;
            this.Value = value;
        }

        /// <summary>
        /// The type of Sensor
        /// </summary>
        [DataMember]
        [Description("Identifies a sensor.")]
        public CreateSensorPacket Sensor;

        /// <summary>
        /// The Sensor Value
        /// </summary>
        [DataMember]
        [Description("Identifies the sensor's value.")]
        public int Value;
    }

    /// <summary>
    /// Create CliffDetail Results
    /// </summary>
    [DataContract]
    public class ReturnCliffDetail : RoombaReturnPacket
    {
        /// <summary>
        /// Is the current packet valid?
        /// </summary>
        public override bool ValidPacket
        {
            get
            {
                if (this.Data == null || this.Data.Length < 10)
                    return false;

                if ((this.WallSignal > 4095)
                    || (this.CliffLeftSignal > 4095)
                    || (this.CliffFrontLeftSignal > 4095)
                    || (this.CliffFrontRightSignal > 4095)
                    || (this.CliffRightSignal > 4095)
                    || ((byte)this.UserDigitalInputs > 31)
                    || (this.UserAnalogInput > 1023)
                    || ((byte)this.ChargingSourcesAvailable > 15)
                    )
                    return false;

                return true;
            }
        }

        /// <summary>
        /// Default Constructor
        /// </summary>
        public ReturnCliffDetail() : base(RoombaCommandCode.ReturnCliffDetail) { }

        /// <summary>
        /// Initialization Constructor
        /// </summary>
        public ReturnCliffDetail(RoombaMode roombaMode, byte[] data)
            : base(RoombaCommandCode.ReturnCliffDetail, roombaMode)
        {
            this.Data = data;
        }

        /// <summary>
        /// Initialization Constructor
        /// </summary>
        public ReturnCliffDetail(RoombaMode roombaMode)
            : base(RoombaCommandCode.ReturnCliffDetail, roombaMode) { }

        /// <summary>
        /// Wall Signal
        /// <remarks>0-4095</remarks>
        /// </summary>
        [DataMember]
        [Description("Identifies the wall sensor.")]
        public int WallSignal
        {
            get { return ByteArray.BigEndianGetUShort(this.Data, 0); }
            set { ByteArray.BigEndianSetUShort(this.Data, 0, value); }
        }


        /// <summary>
        /// Cliff Left Signal
        /// <remarks>0-4095</remarks>
        /// </summary>
        [DataMember]
        [Description("Identifies the left side cliff sensor.")]
        public int CliffLeftSignal
        {
            get { return ByteArray.BigEndianGetUShort(this.Data, 2); }
            set { ByteArray.BigEndianSetUShort(this.Data, 2, value); }
        }

        /// <summary>
        /// Cliff Front Left Signal
        /// <remarks>0-4095</remarks>
        /// </summary>
        [DataMember]
        [Description("Identifies the left front cliff sensor.")]
        public int CliffFrontLeftSignal
        {
            get { return ByteArray.BigEndianGetUShort(this.Data, 4); }
            set { ByteArray.BigEndianSetUShort(this.Data, 4, value); }
        }

        /// <summary>
        /// Cliff Front Right Signal
        /// <remarks>0-4095</remarks>
        /// </summary>
        [DataMember]
        [Description("Identifies the right front cliff sensor.")]
        public int CliffFrontRightSignal
        {
            get { return ByteArray.BigEndianGetUShort(this.Data, 6); }
            set { ByteArray.BigEndianSetUShort(this.Data, 6, value); }
        }


        /// <summary>
        /// Cliff Right Signal
        /// <remarks>0-4095</remarks>
        /// </summary>
        [DataMember]
        [Description("Identifies the right side cliff sensor.")]
        public int CliffRightSignal
        {
            get { return ByteArray.BigEndianGetUShort(this.Data, 8); }
            set { ByteArray.BigEndianSetUShort(this.Data, 8, value); }
        }

        /// <summary>
        /// User Digital Inputs
        /// <remarks>0-31</remarks>
        /// </summary>
        [DataMember]
        [Description("Identifies the digital inputs on the Create's Cargo Bay connector.")]
        public CargoBayDigitalInputs UserDigitalInputs
        {
            get { return (CargoBayDigitalInputs)this.Data[10]; }
            set { this.Data[10] = (byte)value;}
        }

        /// <summary>
        /// User Analog Input
        /// <remarks>0-1023</remarks>
        /// </summary>
        [DataMember]
        [Description("Identifies the analog inputs on the Create's Cargo Bay connector.")]
        public int UserAnalogInput
        {
            get { return ByteArray.BigEndianGetUShort(this.Data, 11); }
            set { ByteArray.BigEndianSetUShort(this.Data, 11, value); }
        }

        /// <summary>
        /// Charging Sources Available
        /// <remarks>Valid Bits: 0-3</remarks>
        /// </summary>
        [DataMember]
        [Description("Identifies the charging sources available.")]
        public ChargingSourcesAvailable ChargingSourcesAvailable
        {
            get { return (ChargingSourcesAvailable)this.Data[13]; }
            set { this.Data[13] = (byte)value; }
        }


    }

    /// <summary>
    /// Create Telemetry Results
    /// </summary>
    [DataContract]
    public class ReturnTelemetry : RoombaReturnPacket
    {
        /// <summary>
        /// Is the current packet valid?
        /// </summary>
        public override bool ValidPacket
        {
            get
            {
                if (this.Data == null || this.Data.Length < 12)
                    return false;

                if (((byte)this.OIMode > 3)
                    || (this.SongNumber > 15)
                    || (this.Data[2] > 1)       // Song Playing
                    || (this.NumberOfStreamPackets > 43)
                    || (this.RequestedVelocity < -500)
                    || (this.RequestedVelocity > 500)
                    || (this.RequestedRightVelocity < -500)
                    || (this.RequestedRightVelocity > 500)
                    || (this.RequestedLeftVelocity < -500)
                    || (this.RequestedLeftVelocity > 500)
                    )
                    return false;

                return true;
            }
        }

        /// <summary>
        /// Default Constructor
        /// </summary>
        public ReturnTelemetry() : base(RoombaCommandCode.ReturnTelemetry)
        {
            this.OIMode = RoombaMode.Uninitialized;
            this.RoombaMode = RoombaMode.Uninitialized;
        }

        /// <summary>
        /// Initialization Constructor
        /// </summary>
        public ReturnTelemetry(byte[] data)
            : base(RoombaCommandCode.ReturnTelemetry)
        {
            this.Data = data;
            if (this.ValidPacket)
                this.RoombaMode = this.OIMode;
            else
            {
                this.OIMode = RoombaMode.Uninitialized;
            }
        }

        /// <summary>
        /// Initialization Constructor
        /// </summary>
        public ReturnTelemetry(RoombaMode roombaMode)
            : base(RoombaCommandCode.ReturnTelemetry, roombaMode)
        {
            this.OIMode = RoombaMode.Uninitialized;
        }

        /// <summary>
        /// Open Interface Mode.
        /// A code indicating the current state of iRobot Create.
        /// </summary>
        [DataMember]
        [Description("Identifies the current Open Interface mode.")]
        public RoombaMode OIMode
        {
            get
            {
                try
                {
                    return IRobotService.GetRoombaMode(Data[0]);
                }
                catch
                {
                    return RoombaMode.Off;
                }
            }
            set
            {
                try
                {
                    RoombaMode newValue = value;
                    Data[0] = (byte)(sbyte)(newValue);
                }
                catch
                {
                    Data[0] = (byte)RoombaMode.Off;
                }
            }
        }

        /// <summary>
        /// Song Number
        /// </summary>
        /// <remarks>Range 1-16</remarks>
        [DataMember]
        [Description("Identifies the number for a pre-defined song.\n(1 - 16).")]
        public int SongNumber
        {
            get { return ((int)this.Data[1])+1; }
            set { this.Data[1] = (byte)(value-1); }
        }

        /// <summary>
        /// Song Playing
        /// </summary>
        [DataMember]
        [Description("Identifies if the song is currently playing.")]
        public bool SongPlaying
        {
            get { return (this.Data[2] == 1); }
            set { this.Data[2] = (byte)(value ? 1 : 0); }
        }

        /// <summary>
        /// Number of Stream Packets
        /// </summary>
        [DataMember]
        [Description("Identifies the number of stream packets.")]
        public int NumberOfStreamPackets
        {
            get { return (int)this.Data[3]; }
            set { this.Data[3] = (byte)value; }
        }

        /// <summary>
        /// The velocity most recently requested with a Drive command.
        /// </summary>
        [DataMember]
        [Description("Identifies the most recently requested Drive velocity.")]
        public int RequestedVelocity
        {
            get { return ByteArray.BigEndianGetShort(this.Data, 4); }
            set { ByteArray.BigEndianSetShort(this.Data, 4, value); }
        }

        /// <summary>
        /// The radius most recently requested with a Drive command.
        /// </summary>
        [Description("Identifies the most recently requested Drive radius.")]
        [DataMember]
        public int RequestedRadius
        {
            get { return ByteArray.BigEndianGetShort(this.Data, 6); }
            set { ByteArray.BigEndianSetShort(this.Data, 6, value); }
        }

        /// <summary>
        /// The right wheel velocity most recently requested with a Drive Direct command.
        /// </summary>
        [DataMember]
        [Description("Identifies the most recently requested Drive Direct right wheel velocity.")]
        public int RequestedRightVelocity
        {
            get { return ByteArray.BigEndianGetShort(this.Data, 8); }
            set { ByteArray.BigEndianSetShort(this.Data, 8, value); }
        }


        /// <summary>
        /// The left wheel velocity most recently requested with a Drive Direct command.
        /// </summary>
        [DataMember]
        [Description("Identifies the most recently requested Drive Direct left wheel velocity.")]
        public int RequestedLeftVelocity
        {
            get { return ByteArray.BigEndianGetShort(this.Data, 10); }
            set { ByteArray.BigEndianSetShort(this.Data, 10, value); }
        }


    }


    /// <summary>
    /// Create QueryList Results
    /// </summary>
    [DataContract]
    public class ReturnQueryList : RoombaReturnPacket
    {
        private CmdQueryList _cmdQueryList = null;
        private Dictionary<CreateSensorPacket, int> _namedValues = null;

        /// <summary>
        /// Is the current packet valid?
        /// </summary>
        public override bool ValidPacket
        {
            get
            {
                if (this.Data == null || this.Data.Length != _cmdQueryList.ExpectedResponseBytes())
                    return false;


                return true;
            }
        }

        /// <summary>
        /// Default Constructor
        /// </summary>
        public ReturnQueryList() : base(RoombaCommandCode.ReturnQueryList) { }

        /// <summary>
        /// Initialization Constructor
        /// </summary>
        public ReturnQueryList(byte[] data, CmdQueryList cmdQueryList)
            : base(RoombaCommandCode.ReturnQueryList, RoombaMode.NotSpecified)
        {
            this.Data = data;
            this._cmdQueryList = cmdQueryList;
        }


        /// <summary>
        /// Parse the query list packet into
        /// a list of named values
        /// </summary>
        /// <returns></returns>
        public Dictionary<CreateSensorPacket, int> NamedValues(RoombaState currentState)
        {
            if (_namedValues == null)
                ParseNamedValues(currentState);

            return _namedValues;
        }

        /// <summary>
        /// Parse named values from the query list return data.
        /// </summary>
        /// <param name="currentState"></param>
        private void ParseNamedValues(RoombaState currentState)
        {
            if (_namedValues == null)
                _namedValues = new Dictionary<CreateSensorPacket, int>();
            else
                _namedValues.Clear();

            if (!this.ValidPacket)
            {
                return;
            }

            int ix = 0;
            foreach (CreateSensorPacket code in this._cmdQueryList.QueryList())
            {
                ix = IRobotUtility.ProcessSensorCode(code, this.Data, ix, currentState, this.Timestamp, ref _namedValues);
            }
        }
    }


    /// <summary>
    /// Create Stream Results
    /// </summary>
    [DataContract]
    public class ReturnStream : RoombaReturnPacket
    {
        private Dictionary<CreateSensorPacket, int> _namedValues = null;

        /// <summary>
        /// Is the current packet valid?
        /// </summary>
        public override bool ValidPacket
        {
            get
            {
                if (this.Data == null || this.Data.Length < 3)
                    return false;
                return 0 < IRobotUtility.ValidateNotification(this.Data, 0);
            }
        }

        /// <summary>
        /// Default Constructor
        /// </summary>
        public ReturnStream() : base(RoombaCommandCode.ReturnStream) { }


        /// <summary>
        /// Initialization Constructor
        /// </summary>
        /// <param name="data"></param>
        /// <param name="timestamp"></param>
        public ReturnStream(byte[] data, DateTime timestamp)
            : base(RoombaCommandCode.ReturnStream, RoombaMode.NotSpecified)
        {
            this.Data = data;
            this.Timestamp = timestamp;
        }

        /// <summary>
        /// The stream packet data
        /// </summary>
        [DataMember]
        [Description("Identifies the stream packet data.")]
        public byte[] StreamPacket
        {
            get { return this.Data; }
            set
            {
                this.Data = value;
                _namedValues = null;
            }
        }

        /// <summary>
        /// The Named Values which have changed.
        /// </summary>
        [DataMember]
        [Description("Identifies the named values that have changed.")]
        public List<SensorValue> NamedValues
        {
            get
            {
                List<SensorValue> list = new List<SensorValue>();
                if (_namedValues != null)
                {
                    foreach (CreateSensorPacket code in _namedValues.Keys)
                        list.Add(new SensorValue(code, _namedValues[code]));
                }
                return list;
            }
            set
            {
                if (_namedValues == null)
                    _namedValues = new Dictionary<CreateSensorPacket, int>();
                else
                    _namedValues.Clear();

                if (value != null)
                {
                    foreach (SensorValue sensor in value)
                        _namedValues.Add(sensor.Sensor, sensor.Value);
                }
            }
        }

        /// <summary>
        /// Parse the notification packet into
        /// a list of named values
        /// </summary>
        /// <returns></returns>
        public Dictionary<CreateSensorPacket, int> RetrieveChangedValues(RoombaState currentState)
        {
            if (_namedValues == null)
                ParseNamedValues(currentState);

            return _namedValues;
        }



        #region Unpack Stream Data



        /// <summary>
        /// Parse a notification packet.
        /// </summary>
        /// <param name="currentState"></param>
        private void ParseNamedValues(RoombaState currentState)
        {
            if (_namedValues == null)
                _namedValues = new Dictionary<CreateSensorPacket, int>();
            else
                _namedValues.Clear();

            if (!this.ValidPacket)
            {
                return;
            }

            int countBytes = this.Data[1];
            int ix = 2;
            while (ix < (this.Data.Length - 1))
            {
                CreateSensorPacket code = (CreateSensorPacket)this.Data[ix++];
                ix = IRobotUtility.ProcessSensorCode(code, this.Data, ix, currentState, this.Timestamp, ref _namedValues);
            }
        }


        #endregion

    }


    /// <summary>
    /// Create Script Results
    /// </summary>
    [DataContract]
    public class ReturnScript : RoombaReturnPacket
    {

        /// <summary>
        /// Is the current packet valid?
        /// </summary>
        public override bool ValidPacket
        {
            get
            {
                if (this.Data == null || this.Data.Length < 3)
                    return false;
                return true;
            }
        }

        /// <summary>
        /// Default Constructor
        /// </summary>
        public ReturnScript() : base(RoombaCommandCode.ReturnScript) { }


        /// <summary>
        /// Initialization Constructor
        /// </summary>
        /// <param name="data"></param>
        /// <param name="timestamp"></param>
        public ReturnScript(byte[] data, DateTime timestamp)
            : base(RoombaCommandCode.ReturnScript, RoombaMode.NotSpecified)
        {
            this.Data = data;
            this.Timestamp = timestamp;
        }

        /// <summary>
        /// The stream packet data
        /// </summary>
        [DataMember]
        [Description("Identifies the stream packet data.")]
        public byte[] ScriptPacket
        {
            get { return this.Data; }
            set
            {
                this.Data = value;
            }
        }

    }

    /// <summary>
    /// The Define Script response
    /// </summary>
    [DataContract]
    public class ReturnDefineScript : RoombaReturnPacket
    {

        /// <summary>
        /// The Define Script response
        /// </summary>
        public ReturnDefineScript() : base(RoombaCommandCode.ReturnDefineScript) { }

        /// <summary>
        /// The Define Script response
        /// </summary>
        public ReturnDefineScript(int scriptResponseBytes):
            base(RoombaCommandCode.ReturnDefineScript)
        {
            this.ScriptResponseBytes = scriptResponseBytes;
        }

        /// <summary>
        /// The expected number of response bytes returned when the script is played.
        /// </summary>
        [DataMember]
        [Description("The expected number of response bytes returned when the script is played.")]
        public int ScriptResponseBytes = 0;


        /// <summary>
        /// Identify this as a valid packet
        /// </summary>
        public override bool ValidPacket
        {
            get { return true; }
        }

    }


    #endregion

    #region Create Specific Operation Types
    // There is no operation type for CmdStream.  This functionality
    // is exposed through the service configuration.


    /// <summary>
    /// Starts a Demo on the iRobot Create.
    /// </summary>
    [DisplayName("(User) CreateDemo")]
    [Description("Starts a demo script on the iRobot Create.")]
    public class CreateDemo : Update<CmdDemo, StandardResponse>
    {
        /// <summary>
        /// Starts a Demo on the iRobot Create.
        /// </summary>
        public CreateDemo()
        {
            this.Body = new CmdDemo();
        }

        /// <summary>
        /// Starts a Demo on the iRobot Create.
        /// </summary>
        /// <param name="demoMode"></param>
        public CreateDemo(DemoMode demoMode)
        {
            this.Body = new CmdDemo(demoMode);
        }
    }


    /// <summary>
    /// This command lets you control the three low side drivers
    /// with variable power. With each data byte, you specify the
    /// PWM duty cycle for the low side driver (max 128). For
    /// example, if you want to control a driver with 25% of battery
    /// voltage, choose a duty cycle of 128 * 25% = 32.
    /// </summary>
    [DisplayName("(User) CreatePWMLowSideDrivers")]
    [Description("Controls the three low side drivers with variable power.")]
    public class CreatePWMLowSideDrivers : Update<CmdPWMLowSideDrivers, StandardResponse>
    {
        /// <summary>
        /// Control the three low side drivers with variable power.
        /// </summary>
        public CreatePWMLowSideDrivers()
        {
            this.Body = new CmdPWMLowSideDrivers();
        }

        /// <summary>
        /// Control the three low side drivers with variable power.
        /// </summary>
        /// <param name="digitalOut0">duty cycle (0 to 128)</param>
        /// <param name="digitalOut1">duty cycle (0 to 128)</param>
        /// <param name="digitalOut2">duty cycle (0 to 128)</param>
        public CreatePWMLowSideDrivers(int digitalOut0, int digitalOut1, int digitalOut2)
        {
            this.Body = new CmdPWMLowSideDrivers(digitalOut0, digitalOut1, digitalOut2);
        }
    }


    /// <summary>
    /// Controls Create's drive wheels.
    /// <remarks>This command lets you control the forward and backward
    /// motion of Create�s drive wheels independently. It takes
    /// four data bytes, which are interpreted as two 16-bit signed
    /// values using two�s complement. The first two bytes specify
    /// the velocity of the right wheel in millimeters per second
    /// (mm/s), with the high byte sent first. The next two bytes
    /// specify the velocity of the left wheel, in the same format.
    /// A positive velocity makes that wheel drive forward, while a
    /// negative velocity makes it drive backward.</remarks>
    /// </summary>
    [DisplayName("(User) CreateDriveDirect")]
    [Description("Controls the forward and backward motion of Create's wheels.")]
    public class CreateDriveDirect : Update<CmdDriveDirect, StandardResponse>
    {
        /// <summary>
        /// Control the forward and backward motion of Create's drive wheels.
        /// </summary>
        public CreateDriveDirect()
        {
            this.Body = new CmdDriveDirect();
        }

        /// <summary>
        /// Control the forward and backward motion of Create's drive wheels.
        /// </summary>
        /// <param name="rightVelocity">mm/s (-500 to +500)</param>
        /// <param name="leftVelocity">mm/s (-500 to +500)</param>
        public CreateDriveDirect(int rightVelocity, int leftVelocity)
        {
            this.Body = new CmdDriveDirect(rightVelocity, leftVelocity);
        }
    }


    /// <summary>
    /// This command controls the state of the 3 digital output
    /// pins on the 25 pin Cargo Bay Connector. The digital outputs
    /// can provide up to 20 mA of current.
    /// </summary>
    [DisplayName("(User) CreateDigitalOutputs")]
    [Description("Controls the state of the 3 digital output pins.")]
    public class CreateDigitalOutputs : Update<CmdDigitalOutputs, StandardResponse>
    {
        /// <summary>
        /// Controls the state of the 3 digital output pins.
        /// </summary>
        public CreateDigitalOutputs()
        {
            this.Body = new CmdDigitalOutputs();
        }

        /// <summary>
        /// Controls the state of the 3 digital output pins.
        /// </summary>
        /// <param name="digitalOut0">on = 5v</param>
        /// <param name="digitalOut1">on = 5v</param>
        /// <param name="digitalOut2">on = 5v</param>
        public CreateDigitalOutputs(bool digitalOut0, bool digitalOut1, bool digitalOut2)
        {
            this.Body = new CmdDigitalOutputs(digitalOut0, digitalOut1, digitalOut2);
        }
    }


    /// <summary>
    /// Request a stream of data.
    /// </summary>
    [DisplayName("(User) CreateStream")]
    [Description("Requests a stream of data.")]
    public class CreateStream : Update<CmdStream, StandardResponse>
    {
        /// <summary>
        /// Request a stream of data.
        /// </summary>
        public CreateStream()
        {
            this.Body = new CmdStream();
        }

        /// <summary>
        /// Request a stream of data.
        /// </summary>
        /// <param name="queryList"></param>
        public CreateStream(List<CreateSensorPacket> queryList)
        {
            this.Body = new CmdStream(queryList);
        }
    }


    /// <summary>
    /// Query for a list of sensors.
    /// </summary>
    [DisplayName("(User) CreateQueryList")]
    [Description("Queries for a list of sensors.")]
    public class CreateQueryList : Update<CmdQueryList, PortSet<ReturnQueryList, Fault>>
    {
        /// <summary>
        /// Query for a list of sensors.
        /// </summary>
        public CreateQueryList()
        {
            this.Body = new CmdQueryList();
        }

        /// <summary>
        /// Query for a list of sensors.
        /// </summary>
        /// <param name="queryList"></param>
        public CreateQueryList(List<CreateSensorPacket> queryList)
        {
            this.Body = new CmdQueryList(queryList);
        }

    }


    /// <summary>
    /// Pause or Resume Stream Data.
    /// </summary>
    [DisplayName("(User) CreateStreamPauseResume")]
    [Description("Pauses or resumes data streaming.")]
    public class CreateStreamPauseResume : Update<CmdStreamPauseResume, StandardResponse>
    {
        /// <summary>
        /// Pause or Resume Stream Data.
        /// </summary>
        public CreateStreamPauseResume()
        {
            this.Body = new CmdStreamPauseResume();
        }

        /// <summary>
        /// Pause or Resume Stream Data.
        /// </summary>
        /// <param name="streamActive"></param>
        public CreateStreamPauseResume(bool streamActive)
        {
            this.Body = new CmdStreamPauseResume(streamActive);
        }
    }


    /// <summary>
    /// Sends an IR Command out pin 23 on the Cargo Bay Connector.
    /// </summary>
    [DisplayName("(User) CreateSendIR")]
    [Description("Sends an IR command.\n(Uses Cargo Bay Connector pin 23.")]
    public class CreateSendIR : Update<CmdSendIR, StandardResponse>
    {
        /// <summary>
        /// Sends an IR Command out pin 23 on the Cargo Bay Connector.
        /// </summary>
        public CreateSendIR()
        {
            this.Body = new CmdSendIR();
        }

        /// <summary>
        /// Sends an IR Command out pin 23 on the Cargo Bay Connector.
        /// </summary>
        /// <param name="irCode"></param>
        public CreateSendIR(RemoteIR irCode)
        {
            this.Body = new CmdSendIR(irCode);
        }
    }


    /// <summary>
    /// Define an iRobot Create Script.
    /// </summary>
    [DisplayName("(User) CreateDefineScript")]
    [Description("Defines an iRobot Create Script.")]
    public class CreateDefineScript : Update<CmdDefineScript, PortSet<ReturnDefineScript, Fault>>
    {
        /// <summary>
        /// Define an iRobot Create Script.
        /// </summary>
        public CreateDefineScript()
        {
            this.Body = new CmdDefineScript();
        }

        /// <summary>
        /// Define an iRobot Create Script.
        /// </summary>
        /// <param name="scriptedCommands"></param>
        public CreateDefineScript(List<RoombaCommand> scriptedCommands)
        {
            this.Body = new CmdDefineScript(scriptedCommands);
        }
    }


    /// <summary>
    /// Play the previously defined iRobot Create script.
    /// </summary>
    [DisplayName("(User) CreatePlayScript")]
    [Description("Plays the previously defined iRobot Create script.")]
    public class CreatePlayScript : Update<CmdPlayScript, StandardResponse>
    {
        /// <summary>
        /// Play the previously defined iRobot Create script.
        /// </summary>
        public CreatePlayScript()
        {
            this.Body = new CmdPlayScript();
        }
    }

    /// <summary>
    /// Show the previously defined iRobot Create Script.
    /// </summary>
    [DisplayName("(User) CreateShowScript")]
    [Description("Shows the previously defined iRobot Create Script.")]
    public class CreateShowScript : Update<CmdShowScript, PortSet<ReturnScript, Fault>>
    {
        /// <summary>
        /// Show the previously defined iRobot Create Script.
        /// </summary>
        public CreateShowScript()
        {
            this.Body = new CmdShowScript();
        }

    }


    /// <summary>
    /// Causes Create to wait for the specified time.
    /// </summary>
    [DisplayName("(User) CreateWaitTime")]
    [Description("Causes the Create to wait for the specified time.\n(0.0 - 25.5 seconds)")]
    public class CreateWaitTime : Update<CmdWaitTime, StandardResponse>
    {
        /// <summary>
        /// Causes Create to wait for the specified time.
        /// </summary>
        public CreateWaitTime()
        {
            this.Body = new CmdWaitTime();
        }

        /// <summary>
        /// Causes Create to wait for the specified time.
        /// </summary>
        /// <param name="seconds">0.0 - 25.5</param>
        public CreateWaitTime(double seconds)
        {
            this.Body = new CmdWaitTime(seconds);
        }
    }


    /// <summary>
    /// Causes Create to wait for the specified distance.
    /// </summary>
    [DisplayName("(User) CreateWaitDistance")]
    [Description("Causes the Create to wait until the wheels travel the specified distance in mm.")]
    public class CreateWaitDistance : Update<CmdWaitDistance, StandardResponse>
    {
        /// <summary>
        /// Causes Create to wait until the wheels travel the specified distance in mm.
        /// </summary>
        public CreateWaitDistance()
        {
            this.Body = new CmdWaitDistance();
        }

        /// <summary>
        /// Causes Create to wait until the wheels travel the specified distance in mm.
        /// </summary>
        /// <param name="distance">-32767 - 32768 mm</param>
        public CreateWaitDistance(int distance)
        {
            this.Body = new CmdWaitDistance(distance);
        }
    }


    /// <summary>
    /// Causes Create to wait until the wheels rotate through the specified angle in degrees.
    /// </summary>
    [DisplayName("(User) CreateWaitAngle")]
    [Description("Causes the Create to wait until the wheels rotate through the specified angle in degrees.")]
    public class CreateWaitAngle : Update<CmdWaitAngle, StandardResponse>
    {
        /// <summary>
        /// Causes Create to wait until the wheels rotate through the specified angle in degrees.
        /// </summary>
        public CreateWaitAngle()
        {
            this.Body = new CmdWaitAngle();
        }

        /// <summary>
        /// Causes Create to wait until the wheels rotate through the specified angle in degrees.
        /// </summary>
        /// <param name="angle">-32767 - 32768 degrees</param>
        public CreateWaitAngle(int angle)
        {
            this.Body = new CmdWaitAngle(angle);
        }
    }


    /// <summary>
    /// Causes Create to wait until the specified event occurs.
    /// </summary>
    [DisplayName("(User) CreateWaitEvent")]
    [Description("Causes the Create to wait until the specified event occurs.")]
    public class CreateWaitEvent : Update<CmdWaitEvent, StandardResponse>
    {
        /// <summary>
        /// Causes Create to wait until the specified event occurs.
        /// </summary>
        public CreateWaitEvent()
        {
            this.Body = new CmdWaitEvent();
        }

        /// <summary>
        /// Causes Create to wait until the specified event occurs.
        /// </summary>
        /// <param name="waitEvent"></param>
        public CreateWaitEvent(WaitEvent waitEvent)
        {
            this.Body = new CmdWaitEvent(waitEvent);
        }
    }

    #endregion

    #region Create Utilities
    /// <summary>
    /// Create Utilities
    /// </summary>
    public static class IRobotUtility
    {
        /// <summary>
        /// Convert Left and Right Normalized Power to a Roomba Drive command
        /// </summary>
        /// <param name="leftPower"></param>
        /// <param name="rightPower"></param>
        /// <returns></returns>
        public static roomba.CmdDrive ConvertToDrive(double leftPower, double rightPower)
        {
            double absLeft = Math.Abs(leftPower);
            double absRight = Math.Abs(rightPower);

            roomba.CmdDrive drive = new roomba.CmdDrive();
            int velocity = 0;   // -500 to +500
            int radius = 2000; // -2000 to +2000

            // if right wheel is faster forward, positive radius turns to the left
            int spinSign = (rightPower > leftPower) ? 1 : -1;

            // set velocity sign based on sign of the faster wheel.
            int velocitySign;
            if (absLeft > absRight)
                velocitySign = Math.Sign(leftPower);
            else
                velocitySign = Math.Sign(rightPower);

            if (absLeft != 0 || absRight != 0)
            {
                if (leftPower == 0.0 || rightPower == 0.0 || Math.Sign(leftPower) == Math.Sign(rightPower))
                {
                    velocity = (int)((rightPower + leftPower) * 250);

                    if (absLeft == 0 || absRight == 0)
                    {
                        // radius is 258 mm
                        radius = spinSign * (int)roomba.Contract.iRobotWheelBase;
                    }
                    else
                    {
                        // radius 0 = 2000mm

                        if (rightPower == leftPower)
                        {
                            // Roomba special value for straight ahead
                            radius = 32768;
                        }
                        else
                        {
                            // Calculate the number of seconds it will take to travel in a full circle.
                            // 360 = seconds * (360 * ((right - left) * 500) / (258 * pi)
                            double secondsToCircle = 3.2421236185046666220934479715444 / Math.Abs(absRight - absLeft);

                            // circumference = ((leftmm + rightmm) / 2) * secondsToCircle;
                            // radius = (circumference / PI / 2)
                            radius = velocitySign * spinSign * Math.Max(1, Math.Min(2000, (int)((absLeft + absRight) * 125.0 * secondsToCircle / Math.PI)));
                        }

                    }
                }
                else // left and right are travelling in different directions (spin)
                {

                    velocity = velocitySign * (int)((absLeft + absRight) * 250);

                    // an exact spin
                    if (absLeft == absRight)
                    {
                        // 1 = counter clockwise when velocity > 0 and clockwise when velocity < 0.
                        radius = 1; // ;
                    }
                    else
                    {
                        // -.999, +1.0  radius = 1;
                        // -.001, +.002 radius = 1
                        // -.001, +1.0  radius = 120;
                        radius = spinSign * (int)Math.Max(1.0, (Math.Min(1.0, (Math.Abs(absLeft - absRight) * 120))));
                    }
                }
            }

            drive.Velocity = Math.Max(Math.Min(velocity, 500), -500);
            drive.Radius = radius;
            if ((drive.Radius < -2000 || drive.Radius > 2000) && (drive.Radius != 32768))
                drive.Radius = 32768;

            return drive;
        }

        /// <summary>
        /// Validate a notification packet.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="start">0 based start of the data within data buffer</param>
        /// <returns>the size of the notification packet or 0</returns>
        public static int ValidateNotification(byte[] data, int start)
        {
            if ((data == null) || ((data.Length - start) < 4) || data[start] != 19)
                return 0;

            int checksum = 0;
            int bytes = data[start + 1];

            // check the end of the buffer
            if (data.Length < (bytes + 3 + start))
                return 0;

            for (int ix = 0; ix <= bytes + 2; ix++)
            {
                checksum += data[ix];
            }
            if ((checksum % 0x100) != 0)
                return 0;

            return bytes + 3;
        }

        /// <summary>
        /// Retrieve a Roomba Command from its packet form
        /// </summary>
        /// <param name="data"></param>
        /// <param name="ix"></param>
        /// <param name="cmd"></param>
        /// <returns></returns>
        public static int ReturnIRobotCommand(byte[] data, int ix, out RoombaCommand cmd)
        {
            if (data == null || data.Length < (ix + 1))
            {
                cmd = null;
                return ix + 1;
            }

            RoombaCommandCode code = (RoombaCommandCode)data[ix];
            int dataLength = IRobotUtility.PacketDataSize(code, true);
            switch (code)
            {
                case RoombaCommandCode.Baud:
                    cmd = null;
                    break;
                case RoombaCommandCode.Clean:
                    cmd = new roomba.CmdClean();
                    break;
                case RoombaCommandCode.Control:
                    cmd = new roomba.InternalCmdControl();
                    break;
                case RoombaCommandCode.DefineSong:
                    cmd = new roomba.CmdDefineSong();
                    break;
                case RoombaCommandCode.Demo:  // Same as RoombaCommandCode.Max, but for Create.
                    cmd = new CmdDemo();
                    break;
                case RoombaCommandCode.DigitalOutputs:
                    cmd = new CmdDigitalOutputs();
                    break;
                case RoombaCommandCode.Drive:
                    cmd = new roomba.CmdDrive();
                    break;
                case RoombaCommandCode.DriveDirect:
                    cmd = new CmdDriveDirect();
                    break;
                case RoombaCommandCode.FirmwareDate:
                    cmd = new CmdFirmwareDate();
                    break;
                case RoombaCommandCode.ForceSeekingDock:
                    cmd = new roomba.CmdForceSeekingDock();
                    break;
                case RoombaCommandCode.Full:
                    cmd = new roomba.InternalCmdFull();
                    break;
                case RoombaCommandCode.Leds:
                    cmd = new roomba.CmdLeds();
                    break;
                case RoombaCommandCode.Motors:
                    cmd = new roomba.CmdMotors();
                    break;
                case RoombaCommandCode.StreamPauseResume:
                    cmd = new CmdStreamPauseResume();
                    break;
                case RoombaCommandCode.OsmoReset:
                    cmd = new roomba.InternalCmdReset();
                    break;
                case RoombaCommandCode.PlaySong:
                    cmd = new roomba.CmdPlaySong();
                    break;
                case RoombaCommandCode.PlayScript:
                    cmd = new CmdPlayScript();
                    break;
                case RoombaCommandCode.Power:
                    cmd = new roomba.InternalCmdPower();
                    break;
                case RoombaCommandCode.PWMLowSideDrivers:
                    cmd = new CmdPWMLowSideDrivers();
                    break;
                case RoombaCommandCode.QueryList:
                    cmd = new CmdQueryList();
                    break;
                case RoombaCommandCode.Safe:
                    cmd = new roomba.InternalCmdSafe();
                    break;
                case RoombaCommandCode.DefineScript:
                    cmd = new CmdDefineScript();
                    break;
                case RoombaCommandCode.SendIR:
                    cmd = new CmdSendIR();
                    break;
                case RoombaCommandCode.Sensors:
                    cmd = new CmdSensors();
                    break;
                case RoombaCommandCode.ShowScript:
                    cmd = new CmdShowScript();
                    break;
                case RoombaCommandCode.Spot:
                    cmd = new roomba.CmdSpot();
                    break;
                case RoombaCommandCode.Start:
                    cmd = new roomba.InternalCmdStart();
                    break;
                case RoombaCommandCode.Stream:
                    cmd = new CmdStream();
                    break;
                case RoombaCommandCode.WaitAngle:
                    cmd = new CmdWaitAngle();
                    break;
                case RoombaCommandCode.WaitDistance:
                    cmd = new CmdWaitDistance();
                    break;
                case RoombaCommandCode.WaitEvent:
                    cmd = new CmdWaitEvent();
                    break;
                case RoombaCommandCode.WaitTime:
                    cmd = new CmdWaitTime();
                    break;
                default:
                    cmd = null;
                    break;
            }
            if (cmd != null && dataLength > 0)
                cmd.Data = ByteArray.SubArray(data, ix + 1, dataLength);

            return ix + 1 + dataLength;
        }

        #region Define Songs

        /// <summary>
        /// Define a Song
        /// </summary>
        /// <param name="songId"></param>
        /// <returns></returns>
        public static CmdDefineSong DefinePlayfulSong(int songId)
        {
            // Define a song
            CmdDefineSong song = new CmdDefineSong(songId);
            song.Notes.Add(new RoombaNote(RoombaFrequency.B_Hz_493p9, 10));
            song.Notes.Add(new RoombaNote(RoombaFrequency.B_Hz_987p8, 10));
            song.Notes.Add(new RoombaNote(RoombaFrequency.FSharp_Hz_740p0, 10));
            song.Notes.Add(new RoombaNote(RoombaFrequency.DSharp_Hz_622p3, 10));
            song.Notes.Add(new RoombaNote(RoombaFrequency.B_Hz_987p8, 10));
            song.Notes.Add(new RoombaNote(RoombaFrequency.FSharp_Hz_740p0, 10));
            song.Notes.Add(new RoombaNote(RoombaFrequency.DSharp_Hz_622p3, 20));
            song.Notes.Add(new RoombaNote(RoombaFrequency.C_Hz_523p3, 10));
            song.Notes.Add(new RoombaNote(RoombaFrequency.C_Hz_1046p5, 10));
            song.Notes.Add(new RoombaNote(RoombaFrequency.G_Hz_784p0, 10));
            song.Notes.Add(new RoombaNote(RoombaFrequency.E_Hz_659p3, 10));
            song.Notes.Add(new RoombaNote(RoombaFrequency.C_Hz_1046p5, 10));
            song.Notes.Add(new RoombaNote(RoombaFrequency.G_Hz_784p0, 10));
            song.Notes.Add(new RoombaNote(RoombaFrequency.E_Hz_659p3, 20));
            song.Notes.Add(new RoombaNote(RoombaFrequency.B_Hz_987p8, 10));
            song.Notes.Add(new RoombaNote(RoombaFrequency.Rest, 10));
            return song;
        }

        /// <summary>
        /// Define a Song
        /// </summary>
        /// <param name="songId"></param>
        /// <returns></returns>
        public static CmdDefineSong DefineSimpleSong(int songId)
        {
            // Define a song
            CmdDefineSong song = new CmdDefineSong(songId);
            song.SetNote(1, RoombaFrequency.B_Hz_493p9, 20);
            song.SetNote(2, RoombaFrequency.B_Hz_987p8, 10);
            return song;
        }

        /// <summary>
        /// Define two ascending notes
        /// </summary>
        /// <param name="songId"></param>
        /// <returns></returns>
        public static CmdDefineSong DefineAscendingSong(int songId)
        {
            // Define a song
            List<RoombaNote> notes = new List<RoombaNote>();
            notes.Add(new RoombaNote(RoombaFrequency.B_Hz_493p9, 20));
            notes.Add(new RoombaNote(RoombaFrequency.B_Hz_987p8, 10));
            return new CmdDefineSong(songId, notes);
        }

        /// <summary>
        /// Define two descending notes
        /// </summary>
        /// <param name="songId"></param>
        /// <returns></returns>
        public static CmdDefineSong DefineDecendingSong(int songId)
        {
            // Define a song
            List<RoombaNote> notes = new List<RoombaNote>();
            notes.Add(new RoombaNote(RoombaFrequency.B_Hz_987p8, 10));
            notes.Add(new RoombaNote(RoombaFrequency.B_Hz_493p9, 20));
            return new CmdDefineSong(songId, notes);
        }



        #endregion

        /// <summary>
        /// Adjust the newPose Distance and Angle
        /// to be cumulative based on priorPose.
        /// </summary>
        /// <param name="newPose"></param>
        /// <param name="priorPose"></param>
        internal static void AdjustDistanceAndAngle(Microsoft.Robotics.Services.IRobot.Roomba.ReturnPose newPose, Microsoft.Robotics.Services.IRobot.Roomba.ReturnPose priorPose)
        {
            newPose.Angle += priorPose.Angle;
            newPose.Distance += priorPose.Distance;
        }

        /// <summary>
        /// Returns the string representation of a List of CreateSensorPacket's.
        /// </summary>
        /// <param name="sensorList"></param>
        /// <returns></returns>
        internal static string SensorListToString(List<CreateSensorPacket> sensorList)
        {
            StringBuilder sensors = new StringBuilder("[");
            if (sensorList != null && sensorList.Count > 0)
            {
                foreach (CreateSensorPacket sensor in sensorList)
                {
                    sensors.Append(sensor.ToString());
                    sensors.Append(',');
                }
                sensors.Length--;
            }
            sensors.Append(']');
            return sensors.ToString();
        }

        /// <summary>
        /// Find a firmware packet in the specified buffer
        /// </summary>
        /// <param name="data">the data buffer</param>
        /// <param name="start">starting index of the buffer</param>
        /// <param name="end">index which is one past the last byte of the buffer</param>
        /// <returns>The index of the start of the firmware packet or -1</returns>
        internal static int FindFirmwarePacket(byte[] data, int start, int end)
        {
            if (data == null || data.Length < 7 || (end - start) < 7)
                return -1;

            for (; (start + 7) <= end; start++)
            {
                if (data[start] != 0x12)
                    continue;

                int xorSum = 0;
                for (int ix = 1; ix < 6; ix++)
                {
                    xorSum = xorSum ^ (int)data[start + ix];
                }
                if (xorSum == (int)data[start + 6])
                    return start;
            }

            return -1;
        }

        /// <summary>
        /// The size of the specified list of return packets
        /// </summary>
        /// <param name="packetList"></param>
        /// <returns></returns>
        internal static int ReturnListSize(List<CreateSensorPacket> packetList)
        {
            int count = 0;
            foreach (CreateSensorPacket packet in packetList)
            {
                count += ReturnPacketSize(packet);
            }
            return count;
        }

        /// <summary>
        /// The Notification size for a list of sensors.
        /// </summary>
        /// <param name="packetList"></param>
        /// <returns></returns>
        internal static int ReturnNotificationSize(List<CreateSensorPacket> packetList)
        {
            if (packetList == null)
                return 0;

            return 2 + packetList.Count + ReturnListSize(packetList);
        }

        /// <summary>
        /// The size of the specified return packet
        /// </summary>
        /// <param name="sensor"></param>
        /// <returns></returns>
        internal static int ReturnPacketSize(CreateSensorPacket sensor)
        {
            switch (sensor)
            {
                case CreateSensorPacket.AllRoomba:
                    return 26;
                case CreateSensorPacket.AllBumpsCliffsAndWalls:
                    return 10;
                case CreateSensorPacket.AllPose:
                    return 6;
                case CreateSensorPacket.AllPower:
                    return 10;
                case CreateSensorPacket.AllCliffDetail:
                    return 14;
                case CreateSensorPacket.AllTelemetry:
                    return 12;
                case CreateSensorPacket.AllCreate:
                    return 52;
                case CreateSensorPacket.BumpsWheelDrops:
                case CreateSensorPacket.Wall:
                case CreateSensorPacket.CliffLeft:
                case CreateSensorPacket.CliffFrontLeft:
                case CreateSensorPacket.CliffFrontRight:
                case CreateSensorPacket.CliffRight:
                case CreateSensorPacket.VirtualWall:
                case CreateSensorPacket.MotorOvercurrents:
                case CreateSensorPacket.Unused15:
                case CreateSensorPacket.Unused16:
                case CreateSensorPacket.Infrared:
                case CreateSensorPacket.Buttons:
                    return 1;
                case CreateSensorPacket.Distance:
                case CreateSensorPacket.Angle:
                    return 2;
                case CreateSensorPacket.ChargingState:
                    return 1;
                case CreateSensorPacket.Voltage:
                case CreateSensorPacket.Current:
                    return 2;
                case CreateSensorPacket.BatteryTemperature:
                    return 1;
                case CreateSensorPacket.BatteryCharge:
                case CreateSensorPacket.BatteryCapacity:
                case CreateSensorPacket.WallSignal:
                case CreateSensorPacket.CliffLeftSignal:
                case CreateSensorPacket.CliffFrontLeftSignal:
                case CreateSensorPacket.CliffFrontRightSignal:
                case CreateSensorPacket.CliffRightSignal:
                    return 2;
                case CreateSensorPacket.CargoBayDigitalInputs:
                    return 1;
                case CreateSensorPacket.CargoBayAnalogSignal:
                    return 2;
                case CreateSensorPacket.ChargingSourcesAvailable:
                case CreateSensorPacket.OIMode:
                case CreateSensorPacket.SongNumber:
                case CreateSensorPacket.SongPlaying:
                case CreateSensorPacket.NumberOfStreamPackets:
                    return 1;
                case CreateSensorPacket.RequestedVelocity:
                case CreateSensorPacket.RequestedRadius:
                case CreateSensorPacket.RequestedRightVelocity:
                case CreateSensorPacket.RequestedLeftVelocity:
                    return 2;
            }
            return 0;
        }

        #region Unpack stream/query sensor data

        /// <summary>
        /// Retrieve the current sensor code from data at ix.
        /// Advance ix to the next byte after the current data.
        /// </summary>
        /// <param name="code"></param>
        /// <param name="data"></param>
        /// <param name="ix"></param>
        /// <param name="currentState"></param>
        /// <param name="timestamp"></param>
        /// <param name="namedValues"></param>
        /// <returns>ix</returns>
        internal static int ProcessSensorCode(CreateSensorPacket code, byte[] data, int ix, RoombaState currentState, DateTime timestamp, ref Dictionary<CreateSensorPacket, int> namedValues)
        {
            switch (code)
            {
                // multi-part packages
                case CreateSensorPacket.AllCliffDetail:
                    ix = UnpackCliffDetail(currentState, data, ix, ref namedValues);
                    break;
                case CreateSensorPacket.AllCreate:
                    ix = UnpackSensors(currentState, data, ix, timestamp, ref namedValues);
                    ix = UnpackPose(currentState, data, ix, ref namedValues);
                    ix = UnpackPower(currentState, data, ix, ref namedValues);
                    ix = UnpackCliffDetail(currentState, data, ix, ref namedValues);
                    ix = UnpackTelemetry(currentState, data, ix, ref namedValues);
                    break;
                case CreateSensorPacket.AllPose:
                    ix = UnpackPose(currentState, data, ix, ref namedValues);
                    break;
                case CreateSensorPacket.AllPower:
                    ix = UnpackPower(currentState, data, ix, ref namedValues);
                    break;
                case CreateSensorPacket.AllRoomba:
                    ix = UnpackSensors(currentState, data, ix, timestamp, ref namedValues);
                    ix = UnpackPose(currentState, data, ix, ref namedValues);
                    ix = UnpackPower(currentState, data, ix, ref namedValues);
                    break;
                case CreateSensorPacket.AllBumpsCliffsAndWalls:
                    ix = UnpackSensors(currentState, data, ix, timestamp, ref namedValues);
                    break;
                case CreateSensorPacket.AllTelemetry:
                    ix = UnpackTelemetry(currentState, data, ix, ref namedValues);
                    break;

                // ***************************************************************************
                // Unsigned 2 byte integers.
                case CreateSensorPacket.Voltage:
                    ix = GetDiffUShort(code, currentState.Power.Voltage, data, ix, ref namedValues);
                    break;
                case CreateSensorPacket.BatteryCharge:
                    ix = GetDiffUShort(code, currentState.Power.Charge, data, ix, ref namedValues);
                    break;
                case CreateSensorPacket.BatteryCapacity:
                    ix = GetDiffUShort(code, currentState.Power.Capacity, data, ix, ref namedValues);
                    break;
                case CreateSensorPacket.WallSignal:
                    ix = GetDiffUShort(code, currentState.CliffDetail.WallSignal, data, ix, ref namedValues);
                    break;
                case CreateSensorPacket.CliffLeftSignal:
                    ix = GetDiffUShort(code, currentState.CliffDetail.CliffLeftSignal, data, ix, ref namedValues);
                    break;
                case CreateSensorPacket.CliffFrontLeftSignal:
                    ix = GetDiffUShort(code, currentState.CliffDetail.CliffFrontLeftSignal, data, ix, ref namedValues);
                    break;
                case CreateSensorPacket.CliffFrontRightSignal:
                    ix = GetDiffUShort(code, currentState.CliffDetail.CliffFrontRightSignal, data, ix, ref namedValues);
                    break;
                case CreateSensorPacket.CliffRightSignal:
                    ix = GetDiffUShort(code, currentState.CliffDetail.CliffRightSignal, data, ix, ref namedValues);
                    break;
                case CreateSensorPacket.CargoBayAnalogSignal:
                    ix = GetDiffUShort(code, currentState.CliffDetail.UserAnalogInput, data, ix, ref namedValues);
                    break;

                // ***************************************************************************
                // Signed 2 byte integers.
                case CreateSensorPacket.Distance:
                    // Distance is cumulative.  Compare to 0 instead of the prior value.
                    ix = GetDiffShort(code, 0, data, ix, ref namedValues);
                    break;
                case CreateSensorPacket.Angle:
                    // Angle is cumulative.  Compare to 0 instead of the prior value.
                    ix = GetDiffShort(code, 0, data, ix, ref namedValues);
                    break;
                case CreateSensorPacket.Current:
                    ix = GetDiffShort(code, currentState.Power.Current, data, ix, ref namedValues);
                    break;
                case CreateSensorPacket.RequestedVelocity:
                    ix = GetDiffShort(code, currentState.Telemetry.RequestedVelocity, data, ix, ref namedValues);
                    break;
                case CreateSensorPacket.RequestedRadius:
                    ix = GetDiffShort(code, currentState.Telemetry.RequestedRadius, data, ix, ref namedValues);
                    break;
                case CreateSensorPacket.RequestedRightVelocity:
                    ix = GetDiffShort(code, currentState.Telemetry.RequestedRightVelocity, data, ix, ref namedValues);
                    break;
                case CreateSensorPacket.RequestedLeftVelocity:
                    ix = GetDiffShort(code, currentState.Telemetry.RequestedLeftVelocity, data, ix, ref namedValues);
                    break;

                // ***************************************************************************
                // Single byte sensor data
                case CreateSensorPacket.BumpsWheelDrops:
                    ix = GetDiffByte(code, (int)currentState.Sensors.BumpsWheeldrops, data, ix, ref namedValues);
                    break;
                case CreateSensorPacket.Wall:
                    ix = GetDiffByte(code, currentState.Sensors.Wall ? 1 : 0, data, ix, ref namedValues);
                    break;
                case CreateSensorPacket.CliffLeft:
                    ix = GetDiffByte(code, currentState.Sensors.CliffLeft ? 1 : 0, data, ix, ref namedValues);
                    break;
                case CreateSensorPacket.CliffFrontLeft:
                    ix = GetDiffByte(code, currentState.Sensors.CliffFrontLeft ? 1 : 0, data, ix, ref namedValues);
                    break;
                case CreateSensorPacket.CliffFrontRight:
                    ix = GetDiffByte(code, currentState.Sensors.CliffFrontRight ? 1 : 0, data, ix, ref namedValues);
                    break;
                case CreateSensorPacket.CliffRight:
                    ix = GetDiffByte(code, currentState.Sensors.CliffRight ? 1 : 0, data, ix, ref namedValues);
                    break;
                case CreateSensorPacket.VirtualWall:
                    ix = GetDiffByte(code, currentState.Sensors.VirtualWall ? 1 : 0, data, ix, ref namedValues);
                    break;
                case CreateSensorPacket.MotorOvercurrents:
                    ix = GetDiffByte(code, (int)currentState.Sensors.MotorOvercurrents, data, ix, ref namedValues);
                    break;
                case CreateSensorPacket.Unused15:
                    ix = GetDiffByte(code, currentState.Sensors.DirtDetectorLeft, data, ix, ref namedValues);
                    break;
                case CreateSensorPacket.Unused16:
                    ix = GetDiffByte(code, currentState.Sensors.DirtDetectorRight, data, ix, ref namedValues);
                    break;

                case CreateSensorPacket.Infrared:
                    ix = GetDiffByte(code, (int)currentState.Pose.RemoteControlCommand, data, ix, ref namedValues);
                    break;
                case CreateSensorPacket.Buttons:
                    ix = GetDiffByte(code, (int)currentState.Pose.ButtonsRoomba, data, ix, ref namedValues);
                    break;

                case CreateSensorPacket.ChargingState:
                    ix = GetDiffByte(code, (int)currentState.Power.ChargingState, data, ix, ref namedValues);
                    break;
                case CreateSensorPacket.BatteryTemperature:
                    // convert int to signed byte before comparison.
                    int temp = (currentState.Power.Temperature < 0) ? (256 + currentState.Power.Temperature) : currentState.Power.Temperature;
                    ix = GetDiffByte(code, temp, data, ix, ref namedValues);
                    break;

                case CreateSensorPacket.CargoBayDigitalInputs:
                    ix = GetDiffByte(code, (int)currentState.CliffDetail.UserDigitalInputs, data, ix, ref namedValues);
                    break;
                case CreateSensorPacket.ChargingSourcesAvailable:
                    ix = GetDiffByte(code, (int)currentState.CliffDetail.ChargingSourcesAvailable, data, ix, ref namedValues);
                    break;

                case CreateSensorPacket.OIMode:
                    ix = GetDiffByte(code, (int)currentState.Telemetry.OIMode, data, ix, ref namedValues);
                    break;
                case CreateSensorPacket.SongNumber:
                    ix = GetDiffByte(code, (int)currentState.Telemetry.SongNumber, data, ix, ref namedValues);
                    break;
                case CreateSensorPacket.SongPlaying:
                    ix = GetDiffByte(code, currentState.Telemetry.SongPlaying ? 1 : 0, data, ix, ref namedValues);
                    break;
                case CreateSensorPacket.NumberOfStreamPackets:
                    ix = GetDiffByte(code, (int)currentState.Telemetry.NumberOfStreamPackets, data, ix, ref namedValues);
                    break;

                default:
                    break;
            }
            return ix;
        }

        #region private helpers to parse sensor data
        /// <summary>
        /// Retrieve an unsigned byte, compare to the original value
        /// and Add to our list only if the value has changed.
        /// </summary>
        /// <param name="code"></param>
        /// <param name="data"></param>
        /// <param name="originalValue"></param>
        /// <param name="ix"></param>
        /// <param name="namedValues"></param>
        /// <returns>ix</returns>
        private static int GetDiffByte(CreateSensorPacket code, int originalValue, byte[] data, int ix, ref Dictionary<CreateSensorPacket, int> namedValues)
        {
            int newValue = data[ix];
            if (originalValue != newValue)
                namedValues[code] = newValue;
            return ix + 1;
        }

        /// <summary>
        /// Retrieve a 2-byte unsigned integer, compare to the original value
        /// and Add to our list only if the value has changed.
        /// </summary>
        /// <param name="code"></param>
        /// <param name="data"></param>
        /// <param name="originalValue"></param>
        /// <param name="ix"></param>
        /// <param name="namedValues"></param>
        /// <returns>ix</returns>
        private static int GetDiffUShort(CreateSensorPacket code, int originalValue, byte[] data, int ix, ref Dictionary<CreateSensorPacket, int> namedValues)
        {
            int newValue = ByteArray.BigEndianGetUShort(data, ix);
            if (originalValue != newValue)
                namedValues[code] = newValue;
            return ix + 2;
        }

        /// <summary>
        /// Retrieve a 2-byte integer, compare to the original value
        /// and Add to our list only if the value has changed.
        /// </summary>
        /// <param name="code"></param>
        /// <param name="data"></param>
        /// <param name="originalValue"></param>
        /// <param name="ix"></param>
        /// <param name="namedValues"></param>
        /// <returns>ix</returns>
        private static int GetDiffShort(CreateSensorPacket code, int originalValue, byte[] data, int ix, ref Dictionary<CreateSensorPacket, int> namedValues)
        {
            int newValue = ByteArray.BigEndianGetShort(data, ix);
            if (originalValue != newValue)
                namedValues[code] = newValue;
            return ix + 2;
        }

        /// <summary>
        /// Unpack Pose (6 bytes starting at data[ix])
        /// </summary>
        /// <param name="currentState"></param>
        /// <param name="data"></param>
        /// <param name="ix"></param>
        /// <param name="namedValues"></param>
        /// <returns></returns>
        private static int UnpackPose(RoombaState currentState, byte[] data, int ix, ref Dictionary<CreateSensorPacket, int> namedValues)
        {
            ReturnPose poseDetail = new ReturnPose(RoombaMode.NotSpecified, ByteArray.SubArray(data, ix, 6));
            if (poseDetail.ValidPacket)
            {
                if (poseDetail.Angle != 0)
                    namedValues[CreateSensorPacket.Angle] = poseDetail.Angle;
                if (currentState.Pose.ButtonsRoomba != poseDetail.ButtonsRoomba)
                    namedValues[CreateSensorPacket.Buttons] = (int)poseDetail.ButtonsRoomba;
                if (poseDetail.Distance != 0)
                    namedValues[CreateSensorPacket.Distance] = poseDetail.Distance;
                if (currentState.Pose.RemoteControlCommand != poseDetail.RemoteControlCommand)
                    namedValues[CreateSensorPacket.Infrared] = (int)poseDetail.RemoteControlCommand;
            }
            ix += 6;
            return ix;
        }

        /// <summary>
        /// Unpack Power (10 bytes starting at data[ix])
        /// </summary>
        /// <param name="currentState"></param>
        /// <param name="data"></param>
        /// <param name="ix"></param>
        /// <param name="namedValues"></param>
        /// <returns></returns>
        private static int UnpackPower(RoombaState currentState, byte[] data, int ix, ref Dictionary<CreateSensorPacket, int> namedValues)
        {
            ReturnPower powerDetail = new ReturnPower(RoombaMode.NotSpecified, ByteArray.SubArray(data, ix, 10));
            if (powerDetail.ValidPacket)
            {
                if (currentState.Power.Capacity != powerDetail.Capacity)
                    namedValues[CreateSensorPacket.BatteryCapacity] = powerDetail.Capacity;
                if (currentState.Power.Charge != powerDetail.Charge)
                    namedValues[CreateSensorPacket.BatteryCharge] = powerDetail.Charge;
                if (currentState.Power.ChargingState != powerDetail.ChargingState)
                    namedValues[CreateSensorPacket.ChargingState] = (int)powerDetail.ChargingState;
                if (currentState.Power.Current != powerDetail.Current)
                    namedValues[CreateSensorPacket.Current] = powerDetail.Current;
                if (currentState.Power.Temperature != powerDetail.Temperature)
                    namedValues[CreateSensorPacket.BatteryTemperature] = powerDetail.Temperature;
                if (currentState.Power.Voltage != powerDetail.Voltage)
                    namedValues[CreateSensorPacket.Voltage] = powerDetail.Voltage;
            }
            ix += 10;
            return ix;
        }

        /// <summary>
        /// Unpack Telemetry (12 bytes starting at data[ix])
        /// </summary>
        /// <param name="currentState"></param>
        /// <param name="data"></param>
        /// <param name="ix"></param>
        /// <param name="namedValues"></param>
        /// <returns></returns>
        private static int UnpackTelemetry(RoombaState currentState, byte[] data, int ix, ref Dictionary<CreateSensorPacket, int> namedValues)
        {
            ReturnTelemetry telemetryDetail = new ReturnTelemetry(ByteArray.SubArray(data, ix, 12));
            if (telemetryDetail.ValidPacket)
            {
                if (currentState.Telemetry.NumberOfStreamPackets != telemetryDetail.NumberOfStreamPackets)
                    namedValues[CreateSensorPacket.NumberOfStreamPackets] = telemetryDetail.NumberOfStreamPackets;
                if (currentState.Telemetry.OIMode != telemetryDetail.OIMode)
                    namedValues[CreateSensorPacket.OIMode] = (int)telemetryDetail.OIMode;
                if (currentState.Telemetry.RequestedLeftVelocity != telemetryDetail.RequestedLeftVelocity)
                    namedValues[CreateSensorPacket.RequestedLeftVelocity] = telemetryDetail.RequestedLeftVelocity;
                if (currentState.Telemetry.RequestedRadius != telemetryDetail.RequestedRadius)
                    namedValues[CreateSensorPacket.RequestedRadius] = telemetryDetail.RequestedRadius;
                if (currentState.Telemetry.RequestedRightVelocity != telemetryDetail.RequestedRightVelocity)
                    namedValues[CreateSensorPacket.RequestedRightVelocity] = telemetryDetail.RequestedRightVelocity;
                if (currentState.Telemetry.RequestedVelocity != telemetryDetail.RequestedVelocity)
                    namedValues[CreateSensorPacket.RequestedVelocity] = telemetryDetail.RequestedVelocity;
                if (currentState.Telemetry.SongNumber != telemetryDetail.SongNumber)
                    namedValues[CreateSensorPacket.SongNumber] = telemetryDetail.SongNumber;
                if (currentState.Telemetry.SongPlaying != telemetryDetail.SongPlaying)
                    namedValues[CreateSensorPacket.SongPlaying] = telemetryDetail.SongPlaying ? 1 : 0;
            }
            ix += 12;
            return ix;
        }

        /// <summary>
        /// Unpack Sensors (10 bytes starting at data[ix])
        /// </summary>
        /// <param name="currentState"></param>
        /// <param name="data"></param>
        /// <param name="ix"></param>
        /// <param name="timestamp"></param>
        /// <param name="namedValues"></param>
        /// <returns></returns>
        private static int UnpackSensors(RoombaState currentState, byte[] data, int ix, DateTime timestamp, ref Dictionary<CreateSensorPacket, int> namedValues)
        {
            if (data == null || data.Length < (ix + 10))
                return ix;

            ReturnSensors sensorsDetail = new ReturnSensors(RoombaMode.NotSpecified, ByteArray.SubArray(data, ix, 10), timestamp);
            if (sensorsDetail.ValidPacket)
            {
                if (currentState.Sensors.BumpsWheeldrops != sensorsDetail.BumpsWheeldrops)
                    namedValues[CreateSensorPacket.BumpsWheelDrops] = (int)sensorsDetail.BumpsWheeldrops;
                if (currentState.Sensors.CliffFrontLeft != sensorsDetail.CliffFrontLeft)
                    namedValues[CreateSensorPacket.CliffFrontLeft] = sensorsDetail.CliffFrontLeft ? 1 : 0;
                if (currentState.Sensors.CliffFrontRight != sensorsDetail.CliffFrontRight)
                    namedValues[CreateSensorPacket.CliffFrontRight] = sensorsDetail.CliffFrontRight ? 1 : 0;
                if (currentState.Sensors.CliffLeft != sensorsDetail.CliffLeft)
                    namedValues[CreateSensorPacket.CliffLeft] = sensorsDetail.CliffLeft ? 1 : 0;
                if (currentState.Sensors.CliffRight != sensorsDetail.CliffRight)
                    namedValues[CreateSensorPacket.CliffRight] = sensorsDetail.CliffRight ? 1 : 0;
                if (currentState.Sensors.DirtDetectorLeft != sensorsDetail.DirtDetectorLeft)
                    namedValues[CreateSensorPacket.Unused15] = sensorsDetail.DirtDetectorLeft;
                if (currentState.Sensors.DirtDetectorRight != sensorsDetail.DirtDetectorRight)
                    namedValues[CreateSensorPacket.Unused16] = sensorsDetail.DirtDetectorRight;
                if (currentState.Sensors.MotorOvercurrents != sensorsDetail.MotorOvercurrents)
                    namedValues[CreateSensorPacket.MotorOvercurrents] = (int)sensorsDetail.MotorOvercurrents;
                if (currentState.Sensors.VirtualWall != sensorsDetail.VirtualWall)
                    namedValues[CreateSensorPacket.VirtualWall] = sensorsDetail.VirtualWall ? 1 : 0;
                if (currentState.Sensors.Wall != sensorsDetail.Wall)
                    namedValues[CreateSensorPacket.Wall] = sensorsDetail.Wall ? 1 : 0;
            }
            ix += 10;
            return ix;
        }

        /// <summary>
        /// Unpack Cliff Details (14 bytes starting at data[ix])
        /// </summary>
        /// <param name="currentState"></param>
        /// <param name="data"></param>
        /// <param name="ix"></param>
        /// <param name="namedValues"></param>
        /// <returns>The updated index</returns>
        private static int UnpackCliffDetail(RoombaState currentState, byte[] data, int ix, ref Dictionary<CreateSensorPacket, int> namedValues)
        {
            if (data == null || data.Length < (ix + 14))
                return ix;

            ReturnCliffDetail cliffDetail = new ReturnCliffDetail(RoombaMode.NotSpecified, ByteArray.SubArray(data, ix, 14));
            if (cliffDetail.ValidPacket)
            {
                if (currentState.CliffDetail.ChargingSourcesAvailable != cliffDetail.ChargingSourcesAvailable)
                    namedValues[CreateSensorPacket.ChargingSourcesAvailable] = (int)cliffDetail.ChargingSourcesAvailable;
                if (currentState.CliffDetail.CliffFrontLeftSignal != cliffDetail.CliffFrontLeftSignal)
                    namedValues[CreateSensorPacket.CliffFrontLeftSignal] = cliffDetail.CliffFrontLeftSignal;
                if (currentState.CliffDetail.CliffFrontRightSignal != cliffDetail.CliffFrontRightSignal)
                    namedValues[CreateSensorPacket.CliffFrontRightSignal] = cliffDetail.CliffFrontRightSignal;
                if (currentState.CliffDetail.CliffLeftSignal != cliffDetail.CliffLeftSignal)
                    namedValues[CreateSensorPacket.CliffLeftSignal] = cliffDetail.CliffLeftSignal;
                if (currentState.CliffDetail.CliffRightSignal != cliffDetail.CliffRightSignal)
                    namedValues[CreateSensorPacket.CliffRightSignal] = cliffDetail.CliffRightSignal;
                if (currentState.CliffDetail.UserAnalogInput != cliffDetail.UserAnalogInput)
                    namedValues[CreateSensorPacket.CargoBayAnalogSignal] = cliffDetail.UserAnalogInput;
                if (currentState.CliffDetail.UserDigitalInputs != cliffDetail.UserDigitalInputs)
                    namedValues[CreateSensorPacket.CargoBayDigitalInputs] = (int)cliffDetail.UserDigitalInputs;
                if (currentState.CliffDetail.WallSignal != cliffDetail.WallSignal)
                    namedValues[CreateSensorPacket.WallSignal] = cliffDetail.WallSignal;
            }
            ix += 14;
            return ix;
        }

        #endregion

        #endregion


        #region Stream Helpers

        /// <summary>
        /// Parse the iRobot stream configuration and update the internal iRobotConnection.
        /// </summary>
        /// <returns>true when a Serial Port is configured</returns>
        internal static bool ParseConfiguration(stream.StreamState state, ref istream.iRobotConnection iRobotConnection)
        {
            if (state == null)
            {
                state = new stream.StreamState();
            }
            if (state.Configurations == null)
            {
                state.Configurations = new List<stream.NameValuePair>();
                state.Configurations.Add(new stream.NameValuePair("SerialPort", "0"));
                state.Configurations.Add(new stream.NameValuePair("BaudRate", "57600"));
                state.Configurations.Add(new stream.NameValuePair("ConnectionType", "RS232"));
            }
            if (iRobotConnection == null)
                iRobotConnection = new istream.iRobotConnection();

            state.Initialized = false;

            iRobotConnection.BaudRate = GetConfigurationNumber(state.Configurations, "BaudRate", iRobotConnection.BaudRate);
            iRobotConnection.DataBits = GetConfigurationNumber(state.Configurations, "DataBits", iRobotConnection.DataBits);
            iRobotConnection.Encoding = GetConfigurationEnum<Encoding>(state.Configurations, "Encoding", iRobotConnection.Encoding);
            iRobotConnection.Parity = GetConfigurationEnum<ports.Parity>(state.Configurations, "Parity", iRobotConnection.Parity);
            iRobotConnection.StopBits = GetConfigurationEnum<ports.StopBits>(state.Configurations, "StopBits", iRobotConnection.StopBits);
            iRobotConnection.ConnectionType = GetConfigurationEnum<roomba.iRobotConnectionType>(state.Configurations, "ConnectionType", iRobotConnection.ConnectionType);

            string portName = string.Empty;

            // If the SerialPort was specified, this overrides all others
            int serialPort = GetConfigurationNumber(state.Configurations, "SerialPort", 0);
            if (serialPort > 0)
                portName = "COM" + serialPort.ToString();
            else
            {
                // Try to get the "PortName" parameter, or stick with the original
                portName = GetConfiguration(state.Configurations, "PortName", iRobotConnection.PortName);

                // If we still dont have a portName, try the Identifier
                if (string.IsNullOrEmpty(portName) && state.Identifier > 0)
                {
                    portName = "COM" + state.Identifier.ToString();
                    SetNameValue(state.Configurations, "SerialPort", state.Identifier.ToString());
                }
            }

            if (iRobotConnection.BaudRate == 0)
                iRobotConnection.BaudRate = 57600;

            // Now update the configuration
            if (!string.IsNullOrEmpty(portName))
            {
                iRobotConnection.PortName = portName;
                return true;
            }


            return false;
        }

        /// <summary>
        /// Get an enum configuration
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="configurations"></param>
        /// <param name="name"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        private static T GetConfigurationEnum<T>(List<stream.NameValuePair> configurations, string name, T defaultValue)
        {
            string textValue = GetConfiguration(configurations, name, string.Empty);
            if (!string.IsNullOrEmpty(textValue))
            {
                object o = null;
                try
                {
                    if (typeof(T).IsEnum)
                        o = Enum.Parse(typeof(T), textValue, true);
                    else
                        throw new ArgumentException("Type " + typeof(T).ToString() + " is invalid for GetConfigurationEnum()");

                    if (o != null)
                        return (T)o;
                }
                catch { }
            }

            return defaultValue;
        }

        /// <summary>
        /// Get a named configuration parameter
        /// </summary>
        /// <param name="configurations"></param>
        /// <param name="name"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        private static int GetConfigurationNumber(List<stream.NameValuePair> configurations, string name, int defaultValue)
        {
            try
            {
                return int.Parse(GetConfiguration(configurations, name, defaultValue.ToString()));
            }
            catch (System.FormatException) { }
            catch (System.ArgumentNullException) { }

            return defaultValue;
        }

        /// <summary>
        /// Get a named configuration parameter
        /// </summary>
        /// <param name="configurations"></param>
        /// <param name="name"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        private static string GetConfiguration(List<stream.NameValuePair> configurations, string name, string defaultValue)
        {
            if (configurations != null)
            {
                try
                {
                    string parmValue = FindNameValue(configurations, name);
                    if (!string.IsNullOrEmpty(parmValue))
                        return parmValue;
                }
                catch { }
            }
            return defaultValue;
        }

        /// <summary>
        /// Find the value of a name/value pair
        /// </summary>
        /// <param name="list"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        private static string FindNameValue(List<stream.NameValuePair> list, string name)
        {
            stream.NameValuePair match = FindNameValuePair(list, name);
            if (match == null)
                return string.Empty;

            return match.Value;
        }

        /// <summary>
        /// Find a NameValuePair in the specified list
        /// </summary>
        /// <param name="list"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        private static stream.NameValuePair FindNameValuePair(List<stream.NameValuePair> list, string name)
        {
            return list.Find(
                delegate(stream.NameValuePair pair)
                {
                    return pair.Name == name;
                });
        }

        /// <summary>
        /// Add or set the value of a name/value pair
        /// </summary>
        /// <param name="list"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        private static void SetNameValue(List<stream.NameValuePair> list, string name, string value)
        {
            stream.NameValuePair pair = FindNameValuePair(list, name);
            if (pair == null)
                pair = new stream.NameValuePair(name, value);
            else
                pair.Value = value;
        }


        /// <summary>
        /// Get the default Baud Rate for the specified
        /// Roomba connection type.
        /// <remarks>
        /// A baudRate of 0 will return the default baud rate
        /// for the specified connection type.
        /// Otherwise, baudRate will be returned.</remarks>
        /// </summary>
        /// <param name="iRobotConnectionType"></param>
        /// <param name="baudRate"></param>
        /// <returns></returns>
        internal static int GetDefaultBaudRate(iRobotConnectionType iRobotConnectionType, int baudRate)
        {
            if (baudRate <= 0)
            {
                switch (iRobotConnectionType)
                {
                    case iRobotConnectionType.RooTooth:
                        baudRate = 115200;
                        break;
                    default:
                        baudRate = 57600;
                        break;
                }
            }
            return baudRate;
        }

        #endregion

        #region iRobot State Helpers
        /// <summary>
        /// Make sure the specified state is valid.
        /// </summary>
        /// <param name="state"></param>
        /// <param name="initialize">Initialize the connection</param>
        /// <returns></returns>
        public static RoombaState ValidateState(ref RoombaState state, bool initialize)
        {
            if (state == null)
            {
                state = new RoombaState();
#if NET_CF
                state.ConnectionType = iRobotConnectionType.CreateSerialPort;
                state.IRobotModel = IRobotModel.Create;
                state.Name = "AutoGenerated iRobot Create";
                state.PollingInterval = 250;
                state.SerialPort = 1;
                state.WaitForConnect = false;
#else
                // Default connection is wired RS232
                state.ConnectionType = iRobotConnectionType.RoombaSerialPort;
                state.IRobotModel = IRobotModel.NotSpecified;
#endif
            }
            else if (state.IRobotModel == IRobotModel.Create)
            {
                if (state.CliffDetail == null)
                    state.CliffDetail = new ReturnCliffDetail();

                if (state.Telemetry == null)
                {
                    state.Telemetry = new ReturnTelemetry();
                    state.Telemetry.OIMode = state.Mode;
                }

                if (state.CreateNotifications == null)
                {
                    state.CreateNotifications = new List<CreateSensorPacket>();
                    state.CreateNotifications.Add(CreateSensorPacket.OIMode);
                    state.CreateNotifications.Add(CreateSensorPacket.AllBumpsCliffsAndWalls);
                    state.CreateNotifications.Add(CreateSensorPacket.Buttons);
                }
            }

            if (state.Pose == null)
                state.Pose = new ReturnPose();
            if (state.Power == null)
                state.Power = new ReturnPower();
            if (state.Sensors == null)
                state.Sensors = new ReturnSensors();

            // Give the robot a name.
            if (string.IsNullOrEmpty(state.Name))
                state.Name = "Sunny";

            // If we define one or more sensors, then we must define the OIMode,
            // otherwise our mode changing logic will fail.
            if (state.CreateNotifications != null && state.CreateNotifications.Count > 0)
            {
                bool foundOIMode = false;
                foreach (CreateSensorPacket sensor in state.CreateNotifications)
                {
                    if (sensor == CreateSensorPacket.AllCreate
                        || sensor == CreateSensorPacket.AllTelemetry
                        || sensor == CreateSensorPacket.OIMode)
                        foundOIMode = true;
                }
                if (!foundOIMode)
                    state.CreateNotifications.Add(CreateSensorPacket.OIMode);
            }

            bool song1Defined = false;
            bool song2Defined = false;
            if (state.SongDefinitions == null)
            {
                state.SongDefinitions = new List<CmdDefineSong>();
            }
            else
            {
                foreach (CmdDefineSong cmdDefineSong in state.SongDefinitions)
                {
                    if (cmdDefineSong.SongNumber == 1)
                        song1Defined = true;
                    if (cmdDefineSong.SongNumber == 2)
                        song2Defined = true;
                }
            }
            CmdDefineSong song;
            if (!song1Defined)
            {
                song = DefineAscendingSong(1);
                state.SongDefinitions.Add(song);
            }

            if (!song2Defined)
            {
                song = DefineDecendingSong(2);
                state.SongDefinitions.Add(song);
            }


            if (initialize)
            {
                if (state.Telemetry != null)
                    state.Telemetry.OIMode = RoombaMode.Uninitialized;

                state.FirmwareDate = DateTime.MinValue;
                state.Mode = RoombaMode.Uninitialized;
            }

            return state;
        }

        /// <summary>
        /// Is the Roomba Configuration valid?
        /// </summary>
        /// <param name="_state"></param>
        /// <returns></returns>
        internal static bool ValidState(RoombaState _state)
        {
            return (_state != null && _state.SerialPort > 0);
        }


        /// <summary>
        /// Return the name of the iRobot mdoel
        /// <remarks>use the generic "iRobot" when the model is unknown.</remarks>
        /// </summary>
        /// <returns></returns>
        internal static string iRobotModelName(RoombaState state)
        {
            if (state == null || state.IRobotModel == IRobotModel.NotSpecified)
                return "iRobot";

            return state.IRobotModel.ToString();
        }


        /// <summary>
        /// Valid state for command?
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="state"></param>
        /// <param name="newMode"></param>
        /// <returns>true when the RoombaMode is valid</returns>
        internal static bool ValidStateForCommand(RoombaCommand cmd, RoombaState state, out RoombaMode newMode)
        {
            // iRobot Create has one variation from Roomba
            if (state.IRobotModel == IRobotModel.Create)
            {
                if (((RoombaCommandCode)cmd.Command) == RoombaCommandCode.Demo)
                {
                    if (state.Mode == RoombaMode.Off)
                    {
                        newMode = RoombaMode.Passive;
                        return false;
                    }

                    // On Create, Demo mode is valid in Passive, Safe & Full modes.
                    newMode = state.Mode;
                    return true;
                }
            }

            switch ((RoombaCommandCode)cmd.Command)
            {
                // Allow Any
                case RoombaCommandCode.OsmoReset:
                    newMode = RoombaMode.Off;
                    return true;

                // Allow Passive,safe,full
                case RoombaCommandCode.Baud:
                case RoombaCommandCode.DefineSong:
                case RoombaCommandCode.FirmwareDate:
                case RoombaCommandCode.Sensors:
                    if (state.Mode == RoombaMode.Off)
                    {
                        newMode = RoombaMode.Passive;
                        return false;
                    }
                    break;
                // Allow Passive
                case RoombaCommandCode.Control:
                case RoombaCommandCode.ForceSeekingDock:
                    if (state.Mode != RoombaMode.Passive)
                    {
                        newMode = RoombaMode.Passive;
                        return false;
                    }
                    break;

                // Custom
                case RoombaCommandCode.Safe:
                    // Create: Allow Passive, Safe, or Full
                    if (state.IRobotModel == IRobotModel.Create)
                    {
                        newMode = state.Mode;
                        return true;
                    }

                    // Roomba: Allow Full or safe
                    if (state.Mode != RoombaMode.Safe && state.Mode != RoombaMode.Full)
                    {
                        newMode = RoombaMode.Full;
                        return false;
                    }
                    break;

                // Allow Safe or full
                case RoombaCommandCode.Full:
                case RoombaCommandCode.Power:       // Supported only by Roomba
                case RoombaCommandCode.Spot:
                case RoombaCommandCode.Clean:
                case RoombaCommandCode.Max:
                case RoombaCommandCode.Drive:
                case RoombaCommandCode.Motors:      // same as Create Low side drivers
                case RoombaCommandCode.Leds:
                case RoombaCommandCode.PlaySong:
                case RoombaCommandCode.DigitalOutputs:
                case RoombaCommandCode.PWMLowSideDrivers:
                case RoombaCommandCode.SendIR:
                case RoombaCommandCode.DriveDirect:
                    if (state.Mode != RoombaMode.Safe && state.Mode != RoombaMode.Full)
                    {
                        newMode = RoombaMode.Safe;
                        return false;
                    }
                    break;

            }
            newMode = state.Mode;
            return true;

        }

        /// <summary>
        /// Determine the Command necessary to place the Roomba into the specified mode.
        /// <remarks>Returns null if no command is necessary</remarks>
        /// </summary>
        /// <param name="newMode"></param>
        /// <param name="state"></param>
        /// <returns>A Command which will change the mode, or null</returns>
        internal static RoombaCommand SetModeCommand(RoombaMode newMode, RoombaState state)
        {
            // Can't execute a command if the Roomba fell asleep.
            if ((state.Mode == RoombaMode.Off) && (newMode != RoombaMode.Off))
                return null;

            // Already in the correct state
            if (state.Mode == newMode)
                return null;

            if (newMode == RoombaMode.Passive && (state.Mode != RoombaMode.Passive))
            {
                // Move to Passive Mode
                return new InternalCmdStart();
            }

            if (newMode == RoombaMode.Safe && (state.Mode != RoombaMode.Safe))
            {
                if (state.Mode == RoombaMode.Full || state.IRobotModel == IRobotModel.Create)
                {
                    return new InternalCmdSafe();
                }
                else if (state.Mode == RoombaMode.Passive)
                {
                    return new InternalCmdControl();
                }

                return null;
            }

            if (newMode == RoombaMode.Full && (state.Mode != RoombaMode.Full))
            {
                if (state.Mode == RoombaMode.Passive)
                {
                    return new InternalCmdControl();
                }
                // If moving from Passive to Full,
                // this will be executed on the second call to SetModeCommand()
                if (state.Mode == RoombaMode.Safe)
                    return new InternalCmdFull();
            }

            if (newMode == RoombaMode.Shutdown)
            {
                return new InternalCmdStart();
            }

            return null;
        }


        /// <summary>
        /// Update the mode to reflect Create/Roomba state after the specified command is executed.
        /// </summary>
        /// <param name="roombaCommandCode"></param>
        /// <param name="priorMode"></param>
        /// <param name="iRobotModel"></param>
        /// <param name="newMode"></param>
        /// <returns>true when the mode will change</returns>
        internal static bool GetChangedMode(RoombaCommandCode roombaCommandCode, RoombaMode priorMode, IRobotModel iRobotModel, out RoombaMode newMode)
        {
            newMode = priorMode;
            bool isCreate = (iRobotModel == IRobotModel.Create);

            switch (roombaCommandCode)
            {
                case RoombaCommandCode.Start:
                    newMode = RoombaMode.Passive;
                    break;

                case RoombaCommandCode.Control:
                    if (priorMode == RoombaMode.Passive)
                        newMode = RoombaMode.Safe;
                    break;

                case RoombaCommandCode.Safe:
                    if (isCreate || priorMode == RoombaMode.Full)
                        newMode = RoombaMode.Safe;
                    break;

                case RoombaCommandCode.Full:
                    if (isCreate || priorMode == RoombaMode.Safe)
                        newMode = RoombaMode.Full;
                    break;

                case RoombaCommandCode.Power:       // Supported by Roomba
                case RoombaCommandCode.OsmoReset:   // Supported by Create
                    newMode = RoombaMode.Off;
                    break;

                case RoombaCommandCode.Spot:
                case RoombaCommandCode.Clean:   // same as Create "Cover" command
                case RoombaCommandCode.Max:     // same as Create "Demo" command
                case RoombaCommandCode.ForceSeekingDock:
                    if (isCreate || priorMode == RoombaMode.Safe || priorMode == RoombaMode.Full)
                        newMode = RoombaMode.Passive;
                    break;
            }

            // Return true when mode is changed.
            return (priorMode != newMode);
        }


        /// <summary>
        /// Returns the Data size of the packet for the specified command.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="isCreate">Is this an iRoomba Create?</param>
        /// <returns></returns>
        internal static int PacketDataSize(RoombaQueryType command, bool isCreate)
        {
            return PacketDataSize((RoombaCommandCode)command, isCreate);
        }

        /// <summary>
        /// Returns the Data size of the packet for the specified command.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="isCreate">Is this an iRoomba Create?</param>
        /// <returns></returns>
        internal static int PacketDataSize(RoombaCommandCode command, bool isCreate)
        {
            switch (command)
            {
                case RoombaCommandCode.ReturnAllRoomba:
                    return 26;
                case RoombaCommandCode.ReturnBumpsCliffsAndWalls:
                    return 10;
                case RoombaCommandCode.ReturnPose:
                    return 6;
                case RoombaCommandCode.ReturnPower:
                    return 10;
                case RoombaCommandCode.ReturnCliffDetail:
                    return 14;
                case RoombaCommandCode.ReturnFirmwareDate:
                    return 7;
                case RoombaCommandCode.ReturnTelemetry:
                    return 12;

                case RoombaCommandCode.Baud:
                    return 1;
                case RoombaCommandCode.Drive:
                    return 4;
                case RoombaCommandCode.Motors:  // same as Create LowSideDrivers
                    return 1;
                case RoombaCommandCode.Leds:
                    return 3;
                case RoombaCommandCode.DefineSong:
                    return 2; // the fixed size of the song header.
                case RoombaCommandCode.PlaySong:
                    return 1;
                case RoombaCommandCode.Sensors:
                    return 1;
                case RoombaCommandCode.Max: // same as: RoombaCommandCode.Demo
                    if (isCreate)
                        return 1;           // RoombaCommandCode.Demo
                    else
                        return 0;           // RoombaCommandCode.Max
                case RoombaCommandCode.PWMLowSideDrivers:
                    return 3;
                case RoombaCommandCode.DriveDirect:
                    return 4;
                case RoombaCommandCode.DigitalOutputs:
                    return 1;
                case RoombaCommandCode.Stream:
                case RoombaCommandCode.QueryList:
                case RoombaCommandCode.DefineScript:
                    return 1; // the fixed size of the querylist header.

                case RoombaCommandCode.StreamPauseResume:
                case RoombaCommandCode.SendIR:
                case RoombaCommandCode.WaitTime:
                case RoombaCommandCode.WaitEvent:
                    return 1;
                case RoombaCommandCode.WaitDistance:
                case RoombaCommandCode.WaitAngle:
                    return 2;
                default:
                    return 0;
            }
        }





        #endregion
    }

    #endregion
}
