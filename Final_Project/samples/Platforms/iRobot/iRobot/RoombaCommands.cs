//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: RoombaCommands.cs $ $Revision: 21 $
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using W3C.Soap;

using Microsoft.Ccr.Core;
using Microsoft.Dss.Core.Attributes;
using Microsoft.Dss.ServiceModel.Dssp;
using System.ComponentModel;

using roomba = Microsoft.Robotics.Services.IRobot.Roomba;
using Microsoft.Robotics.Services.IRobot.Create;

namespace Microsoft.Robotics.Services.IRobot.Roomba
{
    #region Roomba Enumerations

    /// <summary>
    /// Roomba Commands
    /// </summary>
    [DataContract]
    [Description("Identifies the set of command codes.")]
    public enum RoombaCommandCode : byte
    {
        /// <summary>
        /// No Command specified
        /// </summary>
        None = 255,

        /// <summary>
        /// All Sensor Data Return Packet
        /// </summary>
        ReturnAllRoomba = 0,
        /// <summary>
        /// Sensors Return Packet
        /// </summary>
        ReturnBumpsCliffsAndWalls = 1,
        /// <summary>
        /// Pose Return Packet
        /// </summary>
        ReturnPose = 2,
        /// <summary>
        /// Power Return Packet
        /// </summary>
        ReturnPower = 3,
        /// <summary>
        /// Power Return Packet
        /// </summary>
        ReturnCliffDetail = 4,
        /// <summary>
        /// Telemetry Return Packet
        /// </summary>
        ReturnTelemetry = 5,
        /// <summary>
        /// All Create Sensor data
        /// </summary>
        ReturnAllCreate = 6,
        /// <summary>
        /// Results of the QueryList
        /// </summary>
        ReturnQueryList = 7,

        // ***********************************************************/
        // Firmware Commands

        /// <summary>
        /// Soft Reset of the iRobot
        /// </summary>
        OsmoReset = 0x07,

        /// <summary>
        /// Request the Firmware Date (Roomba Command Code 0x08)
        /// </summary>
        FirmwareDate = 0x08,

        // ***********************************************************/

        /// <summary>
        /// Results from define script command
        /// </summary>
        ReturnDefineScript = 9,

        /// <summary>
        /// Results from show script command
        /// </summary>
        ReturnScript = 10,


        // ***********************************************************/
        //  Response packets with a header byte.

        /// <summary>
        /// Return Firmware Date
        /// </summary>
        ReturnFirmwareDate = 18,

        /// <summary>
        /// Results from stream notifications
        /// </summary>
        ReturnStream = 19,

        // ***********************************************************/

        /// <summary>
        /// Place Roomba in Passive Mode
        /// </summary>
        Start = 128,
        /// <summary>
        /// Set Roomba internal Baud Rate
        /// </summary>
        Baud = 129,
        /// <summary>
        /// Place Roomba from Passive to Safe Mode
        /// </summary>
        Control = 130,
        /// <summary>
        /// Place Roomba from Full to Safe Mode
        /// </summary>
        Safe = 131,
        /// <summary>
        /// Place Roomba from Safe to Full Mode
        /// </summary>
        Full = 132,
        /// <summary>
        /// Place Roomba in Sleep Mode
        /// <remarks>from Safe or Full Mode</remarks>
        /// </summary>
        Power = 133,
        /// <summary>
        /// Start Spot Cleaning
        /// <remarks>From Safe or Full Mode</remarks>
        /// </summary>
        Spot = 134,
        /// <summary>
        /// Start Cleaning Cycle
        /// For Create, seeks to cover the entire room
        /// <remarks>From Safe or Full Mode</remarks>
        /// </summary>
        Clean = 135,
        /// <summary>
        /// Start Maximum time Cleaning Cycle
        /// <remarks>From Safe or Full Mode</remarks>
        /// </summary>
        Max = 136,
        /// <summary>
        /// Start an iRobot Create Demo
        /// </summary>
        Demo = 136,
        /// <summary>
        /// Control Roomba's Wheels
        /// <remarks>From Safe or Full Mode</remarks>
        /// </summary>
        Drive = 137,
        /// <summary>
        /// Control Roomba's cleaning motors
        /// <remarks>From Safe or Full Mode</remarks>
        /// </summary>
        Motors = 138,
        /// <summary>
        /// Set Roomba's display LEDs.
        /// <remarks>From Safe or Full Mode</remarks>
        /// </summary>
        Leds = 139,
        /// <summary>
        /// Define a Song to be played later
        /// <remarks>From Passive, Safe or Full Mode</remarks>
        /// </summary>
        DefineSong = 140,
        /// <summary>
        /// Play a Song
        /// <remarks>From Safe or Full Mode</remarks>
        /// </summary>
        PlaySong = 141,
        /// <summary>
        /// Request Sensor Data
        /// <remarks>From Passive, Safe or Full Mode</remarks>
        /// </summary>
        Sensors = 142,
        /// <summary>
        /// Force Seeking Dock
        /// <remarks>Valid from any mode</remarks>
        /// </summary>
        ForceSeekingDock = 143,

        //*******************************************
        // iRobot Create commands
        //*******************************************
        /// <summary>
        /// PWM Low Side Drivers
        /// </summary>
        PWMLowSideDrivers = 144,

        /// This command lets you control the forward and backward motion of Create's drive
        /// wheels independently.  A positive velocity makes that wheel drive forward,
        /// while a negative velocity makes it drive backward.
        DriveDirect = 145,

        /// <summary>
        /// Digital Outputs
        /// </summary>
        DigitalOutputs = 147,

        /// <summary>
        /// Starts a continuous stream of data packets.
        /// The list of packets requested is sent every 15 ms,
        /// which is the rate Create uses to update data.
        /// </summary>
        Stream = 148,
        /// <summary>
        /// Query for a list of sensor packets.
        /// The result is returned once.
        /// </summary>
        QueryList = 149,
        /// <summary>
        /// Pause or Resume notifications without clearing the list of requested packets.
        /// </summary>
        StreamPauseResume = 150,
        /// <summary>
        /// Send IR
        /// </summary>
        SendIR = 151,
        /// <summary>
        /// Define a script
        /// </summary>
        DefineScript = 152,
        /// <summary>
        /// Play a script
        /// </summary>
        PlayScript = 153,
        /// <summary>
        /// Show a script
        /// </summary>
        ShowScript = 154,
        /// <summary>
        /// Wait specified time
        /// </summary>
        WaitTime = 155,
        /// <summary>
        /// Wait until distance is driven
        /// </summary>
        WaitDistance = 156,
        /// <summary>
        /// Wait until angle is turned
        /// </summary>
        WaitAngle = 157,
        /// <summary>
        /// Wait for an event to occur
        /// </summary>
        WaitEvent = 158,
    }

    /// <summary>
    /// Roomba Query Packets
    /// </summary>
    [DataContract]
    [Description("Identifies the type of sensor data requested.")]
    public enum RoombaQueryType : byte
    {
        /// <summary>
        /// All Sensor Data Return Packet
        /// </summary>
        ReturnAll = 0,
        /// <summary>
        /// Sensors Return Packet
        /// </summary>
        ReturnSensors = 1,
        /// <summary>
        /// Pose Return Packet
        /// </summary>
        ReturnPose = 2,
        /// <summary>
        /// Power Return Packet
        /// </summary>
        ReturnPower = 3,

        // *********************************************************
        // iRobot Create specific query types
        // *********************************************************

        /// <summary>
        /// Power Return Packet
        /// </summary>
        ReturnCliffDetail = 4,
        /// <summary>
        /// Telemetry Return Packet
        /// </summary>
        ReturnTelemetry = 5,
        /// <summary>
        /// All Create Sensor data
        /// </summary>
        ReturnAllCreate = 6,

        // *********************************************************
    }

    /// <summary>
    /// Connection Type
    /// </summary>
    [DataContract]
    [Description("Identifies how the service host/PC is connected to the robot.")]
    public enum iRobotConnectionType
    {
        /// <summary>
        /// Unconfigured Roomba
        /// </summary>
        [Description("Not Configured")]
        NotConfigured = 0,
        /// <summary>
        /// RoombaDevTools Bluetooth connection
        /// </summary>
        [Description("RoombaDevTools 'Rootooth' Bluetooth connection")]
        RooTooth = 1,
        /// <summary>
        /// Serial Port connection designed for the iRobot Roomba 7-pin connector
        /// <remarks>Supports Roomba Wakeup mapping RTS to Roomba Device Detect Pin-5</remarks>
        /// </summary>
        [Description("Serial Port connection designed for the iRobot Roomba 7-pin connector (supports Wakeup)")]
        RoombaSerialPort = 2,
        /// <summary>
        /// Serial Port connection designed for the iRobot Create 7-pin connector
        /// </summary>
        [Description("Serial Port connection designed for the iRobot Create 7-pin connector")]
        CreateSerialPort = 3,
        /// <summary>
        /// Element Direct Bluetooth Adapter Module (BAM!)
        /// </summary>
        [Description("Element Direct Bluetooth Adapter Module (BAM!)")]
        BluetoothAdapterModule = 4,
    }

    /// <summary>
    /// Roomba Operation Modes
    /// </summary>
    [DataContract]
    [Description ("Indicates the current operating mode.")]
    public enum RoombaMode
    {
        /// <summary>
        /// Roomba is in Sleep Mode
        /// </summary>
        Off = 0,
        /// <summary>
        /// Roomba is in Passive Mode
        /// </summary>
        Passive = 1,
        /// <summary>
        /// Roomba is in Safe Mode
        /// </summary>
        Safe = 2,
        /// <summary>
        /// Roomba is in Full Mode
        /// </summary>
        Full = 3,


        /// <summary>
        /// The iRobot Create/Roomba has not been initialized.
        /// This happens when the service is started or the
        /// serial port is configured.
        /// </summary>
        Uninitialized = -1,

        /// <summary>
        /// Return Code indicates the mode is not specified
        /// and should not be updated.
        /// </summary>
        NotSpecified = -2,

        /// <summary>
        /// The iRobot Create/Roomba service is shutting down.
        /// </summary>
        Shutdown = -3,

    }

    /// <summary>
    /// Type of sensor data requested
    /// </summary>
    [DataContract]
    [Description("Identifies the type of sensor data requested.")]
    public enum RoombaReturnPacketCode
    {
        /// <summary>
        /// Retrieve All Data
        /// </summary>
        All = 0,
        /// <summary>
        /// Retrieve Sensor Packet
        /// </summary>
        Sensors = 1,
        /// <summary>
        /// Retrieve Pose Packet
        /// </summary>
        Pose = 2,
        /// <summary>
        /// Retrieve Power Packet
        /// </summary>
        Power = 3
    }

    /// <summary>
    /// Internal Roomba Baud Rate
    /// <remarks>When using Bluetooth connection,
    /// this setting may not be correct</remarks>
    /// </summary>
    [DataContract]
    [Description("Identifies the code to set the baud rate.")]
    public enum RoombaBaudCode
    {
        /// <summary>
        /// 300 Baud
        /// </summary>
        Baud300 = 0,
        /// <summary>
        /// 600 Baud
        /// </summary>
        Baud600 = 1,
        /// <summary>
        /// 1200 Baud
        /// </summary>
        Baud1200 = 2,
        /// <summary>
        /// 2400 Baud
        /// </summary>
        Baud2400 = 3,
        /// <summary>
        /// 4800 Baud
        /// </summary>
        Baud4800 = 4,
        /// <summary>
        /// 9600 Baud
        /// </summary>
        Baud9600 = 5,
        /// <summary>
        /// 14400 Baud
        /// </summary>
        Baud14400 = 6,
        /// <summary>
        /// 19200 Baud
        /// </summary>
        Baud19200 = 7,
        /// <summary>
        /// 28800 Baud
        /// </summary>
        Baud28800 = 8,
        /// <summary>
        /// 38400 Baud
        /// </summary>
        Baud38400 = 9,
        /// <summary>
        /// 57600 Baud
        /// </summary>
        Baud57600 = 10,
        /// <summary>
        /// 115200 Baud
        /// </summary>
        Baud115200 = 11,
    }

    /// <summary>
    /// Roomba Cleaning Motors
    /// </summary>
    [DataContract]
    [Flags]
    [Description("Identifies the flags (bit settings) for accessing the cleaning motors.")]
    public enum RoombaMotorBits
    {
        /// <summary>
        /// Side Brush Motor
        /// </summary>
        SideBrush = 0x01,
        /// <summary>
        /// Vacuum Motor
        /// </summary>
        Vacuum = 0x02,
        /// <summary>
        /// Main Brush Motor
        /// </summary>
        MainBrush = 0x04,
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
    /// iRobot Leds
    /// </summary>
    [DataContract]
    [Flags]
    [Description("Identifies the flags (bit settings) for accessing the robot's LEDs.")]
    public enum RoombaLedBits
    {
        /// <summary>
        /// Off
        /// </summary>
        Off = 0x00,
        /// <summary>
        /// Dirt Detect
        /// </summary>
        DirtDetect = 0x01,
        /// <summary>
        /// Max Led
        /// </summary>
        Max = 0x02,
        /// <summary>
        /// Create center "Play" Button
        /// </summary>
        CreatePlay = 0x02,
        /// <summary>
        /// Clean Led
        /// </summary>
        Clean = 0x04,
        /// <summary>
        /// Spot Led
        /// </summary>
        Spot = 0x08,
        /// <summary>
        /// Create right "Advance" button
        /// </summary>
        CreateAdvance = 0x08,
        /// <summary>
        /// Status Led - Red
        /// </summary>
        StatusRed = 0x10,
        /// <summary>
        /// Status Led - Green
        /// </summary>
        StatusGreen = 0x20,
        /// <summary>
        /// Status Led - Amber
        /// </summary>
        StatusAmber = 0x30,
        /// <summary>
        /// Unused Bit 7
        /// </summary>
        Bit7 = 0x40,
        /// <summary>
        /// Unused Bit 8
        /// </summary>
        Bit8 = 0x80,
    }

    /// <summary>
    /// Bumps and Wheel Drops
    /// </summary>
    [DataContract]
    [Flags]
    [Description("Identifies the flags (bit settings) for accessing the bump and wheel drop sensors.")]
    public enum BumpsWheeldrops
    {
        /// <summary>
        /// Right Bump
        /// </summary>
        BumpRight = 0x01,
        /// <summary>
        /// Left Bump
        /// </summary>
        BumpLeft = 0x02,
        /// <summary>
        /// Right Wheel Drop
        /// </summary>
        WheelDropRight = 0x04,
        /// <summary>
        /// Left Wheel Drop
        /// </summary>
        WheelDropLeft = 0x08,
        /// <summary>
        /// Caster Wheel Drop
        /// </summary>
        WheelDropCaster = 0x10,
        /// <summary>
        /// Unused Bit 6
        /// </summary>
        Bit6 = 0x20,
        /// <summary>
        /// Unused Bit 7
        /// </summary>
        Bit7 = 0x40,
        /// <summary>
        /// Unused Bit 8
        /// </summary>
        Bit8 = 0x80,
    }

    /// <summary>
    /// Motor Stalled
    /// </summary>
    [DataContract]
    [Flags]
    [Description("Identifies the flags (bit settings) for accessing the motor stall state.")]
    public enum MotorOvercurrents
    {
        /// <summary>
        /// Side Brush Stalled
        /// </summary>
        SideBrush = 0x01,
        /// <summary>
        /// Vacuum Plugged
        /// </summary>
        Vacuum = 0x02,
        /// <summary>
        /// Main Brush Stalled
        /// </summary>
        MainBrush = 0x04,
        /// <summary>
        /// Right Wheel Blocked
        /// </summary>
        DriveRight = 0x08,
        /// <summary>
        /// Left Wheel Blocked
        /// </summary>
        DriveLeft = 0x10,
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
    /// Roomba Physical Buttons
    /// </summary>
    [DataContract]
    [Flags]
    [Description("Identifies the flags (bit settings) for accessing the Roomba's buttons.")]
    public enum ButtonsRoomba
    {
        /// <summary>
        /// No Roomba Buttons pressed
        /// </summary>
        Off = 0x00,
        /// <summary>
        /// Roomba Max Button
        /// </summary>
        Max = 0x01,
        /// <summary>
        /// Roomba Clean Button
        /// </summary>
        Clean = 0x02,
        /// <summary>
        /// Roomba Spot Button
        /// </summary>
        Spot = 0x04,
        /// <summary>
        /// Roomba Power Button
        /// </summary>
        Power = 0x08,
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
    /// Charging State
    /// </summary>
    [DataContract]
    [Description("Identifies the flags (bit settings) for acessing the charging state.")]
    public enum ChargingState
    {
        /// <summary>
        /// Not Charging
        /// </summary>
        NotCharging = 0,
        /// <summary>
        /// Charging Recovery
        /// </summary>
        ChargingRecovery = 1,
        /// <summary>
        /// Charging
        /// </summary>
        Charging = 2,
        /// <summary>
        /// Trickle Charging
        /// </summary>
        TrickleCharging = 3,
        /// <summary>
        /// Waiting
        /// </summary>
        Waiting = 4,
        /// <summary>
        /// Charging Error
        /// </summary>
        ChargingError = 5,

        /// <summary>
        /// No response was received from the robot
        /// </summary>
        NoResponse = 255,
    }

    /// <summary>
    /// Roomba Remote IR Codes
    /// </summary>
    [DataContract]
    [Flags]
    [Description("Identifies the flags (bit settings) for accessing the remote control IR codes.")]
    public enum RemoteIR
    {
        /// <summary>
        /// Bit 1 (0x01)
        /// </summary>
        Code1 = 0x01,
        /// <summary>
        /// Bit 2 (0x02)
        /// </summary>
        Code2 = 0x02,
        /// <summary>
        /// Bit 3 (0x04)
        /// </summary>
        Code3 = 0x04,
        /// <summary>
        /// Bit 4 (0x08)
        /// </summary>
        Code4 = 0x08,
        /// <summary>
        /// Bit 5 (0x10)
        /// </summary>
        Code5 = 0x10,
        /// <summary>
        /// Bit 6 (0x20)
        /// </summary>
        Code6 = 0x20,
        /// <summary>
        /// Bit 7 (0x40)
        /// </summary>
        Code7 = 0x40,
        /// <summary>
        /// Bit 8 (0x80)
        /// </summary>
        Code8 = 0x80,
        /// <summary>
        /// Remote "Left"
        /// </summary>
        RemoteLeft = 0x81,
        /// <summary>
        /// Remote "Forward"
        /// </summary>
        RemoteForward = 0x82,
        /// <summary>
        /// Remote "Right"
        /// </summary>
        RemoteRight = 0x83,
        /// <summary>
        /// Remote "Spot"
        /// </summary>
        RemoteSpot = 0x84,
        /// <summary>
        /// Remote "Max"
        /// </summary>
        RemoteMax = 0x85,
        /// <summary>
        /// Small?
        /// </summary>
        RemoteSmall = 0x86,
        /// <summary>
        /// Medium??
        /// </summary>
        RemoteMedium = 0x87,
        /// <summary>
        /// Remote "Clean"
        /// </summary>
        RemoteClean = 0x88,
        /// <summary>
        /// Remote Pause
        /// </summary>
        RemoteStop = 0x89,
        /// <summary>
        /// Remote "Power"
        /// </summary>
        RemotePower = 0x8A,
        /// <summary>
        /// Remote "Forward" + "Left"
        /// </summary>
        RemoteArcForwardLeft = 0x8B,
        /// <summary>
        /// Remote "Forward" + "Right"
        /// </summary>
        RemoteArcForwardRight = 0x8C,
        /// <summary>
        /// Remote "DriveStop" ???
        /// </summary>
        RemoteDriveStop = 0x8D,
        /// <summary>
        /// Remote "Download"
        /// </summary>
        RemoteDownload = 0x8E,
        /// <summary>
        /// Remote "SeekDock"
        /// </summary>
        RemoteSeekDock = 0x8F,
        /// <summary>
        /// Just out of sight of the Buoys
        /// </summary>
        DockFuzzy = 0xF0,
        /// <summary>
        /// The Dock is near
        /// </summary>
        DockNear = 0xF2,
        /// <summary>
        /// The Dock Green Buoy is visible
        /// <remarks>Left of center</remarks>
        /// </summary>
        DockGreen = 0xF4,
        /// <summary>
        /// The Dock Red Buoy is visible
        /// <remarks>Right of center</remarks>
        /// </summary>
        DockRed = 0xF8,
        /// <summary>
        /// Both Dock Buoy's are visible
        /// <remarks>At 90 degrees to base</remarks>
        /// </summary>
        DockGreenRed = 0xFC,
        /// <summary>
        /// The Dock Red Buoy is visible and the Dock is near
        /// </summary>
        DockRedNear = 0xFA,
        /// <summary>
        /// The Dock Green Buoy is visible and the Dock is near
        /// </summary>
        DockGreenNear = 0xF6,
        /// <summary>
        /// The Dock Red and Green Buoy's are visible and the Dock is near
        /// </summary>
        DockRedGreenNear = 0xFE,
        /// <summary>
        /// No IR is being received
        /// </summary>
        NoIR = 0xFF,
    }

    /// <summary>
    /// The table of note frequencies
    /// as defined by Roomba
    /// </summary>
    [DataContract]
    [Description("Identifies note frequencies supported.")]
    public enum RoombaFrequency
    {
        /// <summary>
        /// Quiet
        /// </summary>
        Rest,
        /// <summary>
        /// G Hz 49.0
        /// </summary>
        G_Hz_49p0 = 31,
        /// <summary>
        /// GSharp Hz 51.0
        /// </summary>
        GSharp_Hz_51p0 = 32,
        /// <summary>
        /// A Hz 55.0
        /// </summary>
        A_Hz_55p0 = 33,
        /// <summary>
        /// ASharp Hz 58.3
        /// </summary>
        ASharp_Hz_58p3 = 34,
        /// <summary>
        /// B Hz 61.7
        /// </summary>
        B_Hz_61p7 = 35,
        /// <summary>
        /// C Hz 65.4
        /// </summary>
        C_Hz_65p4 = 36,
        /// <summary>
        /// CSharp Hz 69.3
        /// </summary>
        CSharp_Hz_69p3 = 37,
        /// <summary>
        /// D Hz 73.4
        /// </summary>
        D_Hz_73p4 = 38,
        /// <summary>
        /// DSharp Hz 77.8
        /// </summary>
        DSharp_Hz_77p8 = 39,
        /// <summary>
        /// E Hz 82.4
        /// </summary>
        E_Hz_82p4 = 40,
        /// <summary>
        /// F Hz 87.3
        /// </summary>
        F_Hz_87p3 = 41,
        /// <summary>
        /// FSharp Hz 92.5
        /// </summary>
        FSharp_Hz_92p5 = 42,
        /// <summary>
        /// G Hz 98.0
        /// </summary>
        G_Hz_98p0 = 43,
        /// <summary>
        /// GSharp Hz 103.8
        /// </summary>
        GSharp_Hz_103p8 = 44,
        /// <summary>
        /// A Hz 110.0
        /// </summary>
        A_Hz_110p0 = 45,
        /// <summary>
        /// ASharp Hz 116.5
        /// </summary>
        ASharp_Hz_116p5 = 46,
        /// <summary>
        /// B Hz 123.5
        /// </summary>
        B_Hz_123p5 = 47,
        /// <summary>
        /// C Hz 130.8
        /// </summary>
        C_Hz_130p8 = 48,
        /// <summary>
        /// CSharp Hz 138.6
        /// </summary>
        CSharp_Hz_138p6 = 49,
        /// <summary>
        /// D Hz 146.8
        /// </summary>
        D_Hz_146p8 = 50,
        /// <summary>
        /// DSharp Hz 155.6
        /// </summary>
        DSharp_Hz_155p6 = 51,
        /// <summary>
        /// E Hz 164.8
        /// </summary>
        E_Hz_164p8 = 52,
        /// <summary>
        /// F Hz 174.6
        /// </summary>
        F_Hz_174p6 = 53,
        /// <summary>
        /// FSharp Hz 185.0
        /// </summary>
        FSharp_Hz_185p0 = 54,
        /// <summary>
        /// G Hz 196.0
        /// </summary>
        G_Hz_196p0 = 55,
        /// <summary>
        /// GSharp Hz 207.7
        /// </summary>
        GSharp_Hz_207p7 = 56,
        /// <summary>
        /// A Hz 220.0
        /// </summary>
        A_Hz_220p0 = 57,
        /// <summary>
        /// ASharp Hz 233.1
        /// </summary>
        ASharp_Hz_233p1 = 58,
        /// <summary>
        /// B Hz 246.9
        /// </summary>
        B_Hz_246p9 = 59,
        /// <summary>
        /// C Hz 261.6
        /// </summary>
        C_Hz_261p6 = 60,
        /// <summary>
        /// CSharp Hz 277.2
        /// </summary>
        CSharp_Hz_277p2 = 61,
        /// <summary>
        /// D Hz 293.7
        /// </summary>
        D_Hz_293p7 = 62,
        /// <summary>
        /// DSharp Hz 311.1
        /// </summary>
        DSharp_Hz_311p1 = 63,
        /// <summary>
        /// E Hz 329.6
        /// </summary>
        E_Hz_329p6 = 64,
        /// <summary>
        /// F Hz 349.2
        /// </summary>
        F_Hz_349p2 = 65,
        /// <summary>
        /// FSharp Hz 370.0
        /// </summary>
        FSharp_Hz_370p0 = 66,
        /// <summary>
        /// G Hz 392.0
        /// </summary>
        G_Hz_392p0 = 67,
        /// <summary>
        /// GSharp Hz 415.3
        /// </summary>
        GSharp_Hz_415p3 = 68,
        /// <summary>
        /// A Hz 440.0
        /// </summary>
        A_Hz_440p0 = 69,
        /// <summary>
        /// ASharp Hz 466.2
        /// </summary>
        ASharp_Hz_466p2 = 70,
        /// <summary>
        /// B Hz 493.9
        /// </summary>
        B_Hz_493p9 = 71,
        /// <summary>
        /// C Hz 523.3
        /// </summary>
        C_Hz_523p3 = 72,
        /// <summary>
        /// CSharp Hz 554.4
        /// </summary>
        CSharp_Hz_554p4 = 73,
        /// <summary>
        /// D Hz 587.3
        /// </summary>
        D_Hz_587p3 = 74,
        /// <summary>
        /// DSharp Hz 622.3
        /// </summary>
        DSharp_Hz_622p3 = 75,
        /// <summary>
        /// E Hz 659.3
        /// </summary>
        E_Hz_659p3 = 76,
        /// <summary>
        /// F Hz 698.5
        /// </summary>
        F_Hz_698p5 = 77,
        /// <summary>
        /// FSharp Hz 740.0
        /// </summary>
        FSharp_Hz_740p0 = 78,
        /// <summary>
        /// G Hz 784.0
        /// </summary>
        G_Hz_784p0 = 79,
        /// <summary>
        /// GSharp Hz 830.6
        /// </summary>
        GSharp_Hz_830p6 = 80,
        /// <summary>
        /// A Hz 880.0
        /// </summary>
        A_Hz_880p0 = 81,
        /// <summary>
        /// ASharp Hz 932.3
        /// </summary>
        ASharp_Hz_932p3 = 82,
        /// <summary>
        /// B Hz 987.8
        /// </summary>
        B_Hz_987p8 = 83,
        /// <summary>
        /// C Hz 1046.5
        /// </summary>
        C_Hz_1046p5 = 84,
        /// <summary>
        /// CSharp Hz 1108.7
        /// </summary>
        CSharp_Hz_1108p7 = 85,
        /// <summary>
        /// D Hz 1174.7
        /// </summary>
        D_Hz_1174p7 = 86,
        /// <summary>
        /// DSharp Hz 1244.5
        /// </summary>
        DSharp_Hz_1244p5 = 87,
        /// <summary>
        /// E Hz 1318.5
        /// </summary>
        E_Hz_1318p5 = 88,
        /// <summary>
        /// F Hz 1396.9
        /// </summary>
        F_Hz_1396p9 = 89,
        /// <summary>
        /// FSharp Hz 1480.0
        /// </summary>
        FSharp_Hz_1480p0 = 90,
        /// <summary>
        /// G Hz 1568.0
        /// </summary>
        G_Hz_1568p0 = 91,
        /// <summary>
        /// GSharp Hz 1661.2
        /// </summary>
        GSharp_Hz_1661p2 = 92,
        /// <summary>
        /// A Hz 1760.0
        /// </summary>
        A_Hz_1760p0 = 93,
        /// <summary>
        /// ASharp Hz 1864.7
        /// </summary>
        ASharp_Hz_1864p7 = 94,
        /// <summary>
        /// B Hz 1975.5
        /// </summary>
        B_Hz_1975p5 = 95,
        /// <summary>
        /// C Hz 2093.0
        /// </summary>
        C_Hz_2093p0 = 96,
        /// <summary>
        /// CSharp Hz 2217.5
        /// </summary>
        CSharp_Hz_2217p5 = 97,
        /// <summary>
        /// D Hz 2349.3
        /// </summary>
        D_Hz_2349p3 = 98,
        /// <summary>
        /// DSharp Hz 2489.0
        /// </summary>
        DSharp_Hz_2489p0 = 99,
        /// <summary>
        /// E Hz 2637.0
        /// </summary>
        E_Hz_2637p0 = 100,
        /// <summary>
        /// F Hz 2793.8
        /// </summary>
        F_Hz_2793p8 = 101,
        /// <summary>
        /// FSharp Hz 2960.0
        /// </summary>
        FSharp_Hz_2960p0 = 102,
        /// <summary>
        /// G Hz 3136.0
        /// </summary>
        G_Hz_3136p0 = 103,
        /// <summary>
        /// GSharp Hz 3322.4
        /// </summary>
        GSharp_Hz_3322p4 = 104,
        /// <summary>
        /// A Hz 3520.0
        /// </summary>
        A_Hz_3520p0 = 105,
        /// <summary>
        /// ASharp Hz 3729.3
        /// </summary>
        ASharp_Hz_3729p3 = 106,
        /// <summary>
        /// B Hz 3951.1
        /// </summary>
        B_Hz_3951p1 = 107,
        /// <summary>
        /// C Hz 4186.0
        /// </summary>
        C_Hz_4186p0 = 108,
        /// <summary>
        /// CSharp Hz 4434.9
        /// </summary>
        CSharp_Hz_4434p9 = 109,
        /// <summary>
        /// D Hz 4698.6
        /// </summary>
        D_Hz_4698p6 = 110,
        /// <summary>
        /// DSharp Hz 4978.0
        /// </summary>
        DSharp_Hz_4978p0 = 111,
        /// <summary>
        /// E Hz 5274.0
        /// </summary>
        E_Hz_5274p0 = 112,
        /// <summary>
        /// F Hz 5587.7
        /// </summary>
        F_Hz_5587p7 = 113,
        /// <summary>
        /// FSharp Hz 5919.9
        /// </summary>
        FSharp_Hz_5919p9 = 114,
        /// <summary>
        /// G Hz 6271.9
        /// </summary>
        G_Hz_6271p9 = 115,
        /// <summary>
        /// GSharp Hz 6644.9
        /// </summary>
        GSharp_Hz_6644p9 = 116,
        /// <summary>
        /// A Hz 7040.0
        /// </summary>
        A_Hz_7040p0 = 117,
        /// <summary>
        /// ASharp Hz 7458.6
        /// </summary>
        ASharp_Hz_7458p6 = 118,
        /// <summary>
        /// B Hz 7902.1
        /// </summary>
        B_Hz_7902p1 = 119,
        /// <summary>
        /// C Hz 8372.0
        /// </summary>
        C_Hz_8372p0 = 120,
        /// <summary>
        /// CSharp Hz 8869.8
        /// </summary>
        CSharp_Hz_8869p8 = 121,
        /// <summary>
        /// D Hz 9397.3
        /// </summary>
        D_Hz_9397p3 = 122,
        /// <summary>
        /// DSharp Hz 9956.1
        /// </summary>
        DSharp_Hz_9956p1 = 123,
        /// <summary>
        /// E Hz 10548.1
        /// </summary>
        E_Hz_10548p1 = 124,
        /// <summary>
        /// F Hz 11175.3
        /// </summary>
        F_Hz_11175p3 = 125,
        /// <summary>
        /// FSharp Hz 11839.8
        /// </summary>
        FSharp_Hz_11839p8 = 126,
        /// <summary>
        /// G Hz 12543.9
        /// </summary>
        G_Hz_12543p9 = 127,

    }

    /// <summary>
    /// Defines a note with a tone and duration.
    /// </summary>
    [DataContract]
    [Description("Specifies a song note (using tone and duration).")]
    public class RoombaNote
    {
        private RoombaFrequency _tone;
        private int _duration;

        /// <summary>
        /// Defines a note with a tone and duration.
        /// </summary>
        public RoombaNote() { }

        /// <summary>
        /// Defines a note with a tone and duration.
        /// </summary>
        /// <param name="tone"></param>
        /// <param name="duration"></param>
        public RoombaNote(RoombaFrequency tone, int duration)
        {
            this._tone = tone;
            this._duration = duration;
        }


        /// <summary>
        /// The note tone or frequency.
        /// </summary>
        [DataMember, DataMemberConstructor(Order=1)]
        [Description("Specifies the note's tone (frequency).")]
        public RoombaFrequency Tone
        {
            get
            {
                try
                {
                    return _tone;
                }
                catch
                {
                    return RoombaFrequency.Rest;
                }
            }
            set { _tone = value; }
        }

        /// <summary>
        /// Duration in 1/64 second increments
        /// <remarks>Range 0 - 255</remarks>
        /// </summary>
        [DataMember, DataMemberConstructor(Order = 2)]
        [Description("The note's duration (in 1/64 second increments).\n(Range = 0 - 255)")]
        public int Duration
        {
            get { return _duration; }
            set { _duration = value; }
        }
    }

    /// <summary>
    /// The model of iRobot Roomba or Create
    /// </summary>
    [DataContract]
    public enum IRobotModel
    {
        /// <summary>
        /// The type of iRobot has not been specified.
        /// </summary>
        NotSpecified = 0,
        /// <summary>
        /// iRobot Roomba
        /// </summary>
        Roomba = 1,
        /// <summary>
        /// iRobot Create
        /// </summary>
        Create = 2,
    }

    #endregion

    #region Roomba Commands
    /// <summary>
    /// The base Roomba command format in which all commands inherit from
    /// </summary>
    [DataContract]
    public class RoombaCommand
    {
        private byte[] _data;
        private bool _iRobotCreate;
        private RoombaMode _maintainMode;

        /// <summary>
        /// The Roomba Command
        /// </summary>
        public readonly byte Command;

        /// <summary>
        /// Command Data
        /// </summary>
        public virtual byte[] Data
        {
            get { return _data; }
            set { _data = value; }
        }

        /// <summary>
        /// When set to Passive, Safe, or Full, this mode will be maintained,
        /// even after automatic mode changes and additional commands.
        /// </summary>
        public RoombaMode MaintainMode
        {
            get { return _maintainMode; }
            set { _maintainMode = value; }
        }

        /// <summary>
        /// Identifies this as a command specific to the iRobot Create.
        /// </summary>
        public virtual bool iRobotCreate
        {
            get { return _iRobotCreate; }
            set { _iRobotCreate = value; }
        }

        /// <summary>
        /// Default Constructor
        /// </summary>
        public RoombaCommand() { }

        /// <summary>
        /// Command Constructor
        /// </summary>
        /// <param name="command"></param>
        public RoombaCommand(RoombaCommandCode command)
        {
            this.Command = (byte)command;
            InitializeRoombaCommand();
        }

        /// <summary>
        /// Command Constructor
        /// </summary>
        /// <param name="command"></param>
        /// <param name="iRobotCreate">Is this command specific to the iRobot Create?</param>
        public RoombaCommand(RoombaCommandCode command, bool iRobotCreate)
        {
            this._iRobotCreate = iRobotCreate;
            this.Command = (byte)command;
            InitializeRoombaCommand();
        }

        /// <summary>
        /// Command Constructor with data initialization
        /// </summary>
        /// <param name="command"></param>
        /// <param name="data"></param>
        public RoombaCommand(RoombaCommandCode command, byte[] data)
        {
            this._iRobotCreate = false;
            this.Command = (byte)command;
            this.Data = data;
        }


        /// <summary>
        /// Initialize the Data array based on the command type.
        /// </summary>
        private void InitializeRoombaCommand()
        {
            int dataLength = IRobotUtility.PacketDataSize(this.RoombaCommandCode, this._iRobotCreate);
            if (dataLength > 0)
                this._data = new byte[dataLength];
            else
                this._data = null;
        }

        /// <summary>
        /// Convert internal data to a command packet.
        /// </summary>
        /// <returns></returns>
        public byte[] GetPacket()
        {
            int length = (this.Data == null) ? 0 : this.Data.Length;
            byte[] packetBuffer = new byte[length + 1];

            // Set up the command packet
            packetBuffer[0] = this.Command;
            if (length > 0)
                System.Buffer.BlockCopy(this.Data, 0, packetBuffer, 1, length);
            return packetBuffer;
        }

        /// <summary>
        /// Does this command elicit a response from the iRobot?
        /// </summary>
        /// <returns>The size of the return packet, or 0 for none</returns>
        public virtual int ExpectedResponseBytes()
        {
            return 0;
        }

        /// <summary>
        /// How long should we wait for a response from this command?
        /// </summary>
        /// <returns>The command timeout in ms (0=default)</returns>
        internal int CmdTimeoutMs;
        internal DateTime CmdStarted;
        internal DateTime CmdExpiration;

        /// <summary>
        /// Returns the Roomba Command Code
        /// </summary>
        /// <returns></returns>
        public RoombaCommandCode RoombaCommandCode
        {
            get { return (RoombaCommandCode)this.Command; }
        }


    }

    /// <summary>
    /// Retrieve the firmware date from a Roomba or Create.
    /// </summary>
    [DataContract]
    public class CmdFirmwareDate : RoombaCommand
    {
        /// <summary>
        /// Retrieve the firmware date from a Roomba or Create.
        /// </summary>
        public CmdFirmwareDate() : base(RoombaCommandCode.FirmwareDate) { }

        /// <summary>
        /// The string representation for this command.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("FirmwareDate()");
        }

        /// <summary>
        /// The number of bytes returned by the GetFirmwareDate command
        /// </summary>
        /// <returns></returns>
        public override int ExpectedResponseBytes()
        {
            return 7;
        }

    }

    /// <summary>
    /// Set the iRobot Mode.
    /// </summary>
    [DataContract]
    [Description("Specifies the robot's command mode.")]
    public class CmdSetMode
    {
        private RoombaMode _roombaMode;

        /// <summary>
        /// Set the iRobot Mode
        /// </summary>
        public CmdSetMode() { }

        /// <summary>
        /// Set the iRobot Mode
        /// </summary>
        public CmdSetMode(RoombaMode roombaMode, bool maintainMode)
        {
            this.RoombaMode = roombaMode;
            this.MaintainMode = maintainMode;
        }

        /// <summary>
        /// The iRobot Mode
        /// </summary>
        [DataMember, DataMemberConstructor(Order=1)]
        [Description("Specifies the operational mode.")]
        public RoombaMode RoombaMode
        {
            get { return _roombaMode; }
            set { _roombaMode = value; }
        }

        /// <summary>
        /// When set to Passive, Safe, or Full, this mode will be maintained,
        /// even after automatic mode changes and additional commands.
        /// </summary>
        [DataMember, DataMemberConstructor(Order = 2)]
        [Description("Specifies the maintain mode.")]
        public bool MaintainMode;

        /// <summary>
        /// The string representation for this command.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("SetMode(RoombaMode={0},MaintainMode={1})", this.RoombaMode, this.MaintainMode);
        }

    }

    #region Internal Mode Change Commands


    /// <summary>
    /// Start the Roomba SCI.
    /// <remarks>Internal: Use CmdSetMode from outside of this service</remarks>
    /// <remarks>The Start command must be sent before any
    /// other SCI commands. This command puts the SCI in
    /// passive mode.</remarks>
    /// </summary>
    [DataContract]
    public class InternalCmdStart : RoombaCommand
    {
        /// <summary>
        /// Enable Passive Mode
        /// </summary>
        public InternalCmdStart() : base(RoombaCommandCode.Start) { }

        /// <summary>
        /// Enable Passive Mode and optionally maintain this mode.
        /// </summary>
        /// <param name="maintainPassiveMode"></param>
        public InternalCmdStart(bool maintainPassiveMode)
            : base(RoombaCommandCode.Start)
        {
            if (maintainPassiveMode)
                MaintainMode = RoombaMode.Passive;
        }

        /// <summary>
        /// The string representation for this command.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("Start()");
        }

    }



    /// <summary>
    /// Enables user control of the Roomba.
    /// <remarks>Internal: Use CmdSetMode from outside of this service</remarks>
    /// <remarks>This command must be sent after the start command and
    /// before any control commands are sent to the SCI. </remarks>
    /// </summary>
    [DataContract]
    public class InternalCmdControl : RoombaCommand
    {
        /// <summary>
        /// Enable Safe Mode
        /// </summary>
        public InternalCmdControl() : base(RoombaCommandCode.Control) { }

        /// <summary>
        /// Enable Safe Mode and optionally maintain this mode.
        /// </summary>
        /// <param name="maintainSafeMode"></param>
        public InternalCmdControl(bool maintainSafeMode)
            : base(RoombaCommandCode.Control)
        {
            if (maintainSafeMode)
                MaintainMode = RoombaMode.Safe;
        }

        /// <summary>
        /// The string representation for this command.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("Control()");
        }

    }

    /// <summary>
    /// Puts the Roomba in Safe mode.
    /// <remarks>Internal: Use CmdSetMode from outside of this service</remarks>
    /// <remarks>Note: In order to go from passive mode to safe mode,
    /// use the CmdControl command.</remarks>
    /// </summary>
    [DataContract]
    public class InternalCmdSafe : RoombaCommand
    {
        /// <summary>
        /// Enable Safe Mode
        /// </summary>
        public InternalCmdSafe() : base(RoombaCommandCode.Safe) { }

        /// <summary>
        /// Enable Safe Mode and optionally maintain this mode.
        /// </summary>
        /// <param name="maintainSafeMode"></param>
        public InternalCmdSafe(bool maintainSafeMode)
            : base(RoombaCommandCode.Safe)
        {
            if (maintainSafeMode)
                MaintainMode = RoombaMode.Safe;
        }

        /// <summary>
        /// The string representation for this command.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("Safe()");
        }

    }

    /// <summary>
    /// Enables unrestricted control of Roomba and turns
    /// off safety features.
    /// <remarks>Internal: Use CmdSetMode from outside of this service</remarks>
    /// <remarks>This command puts the SCI in full mode.</remarks>
    /// </summary>
    [DataContract]
    public class InternalCmdFull : RoombaCommand
    {
        /// <summary>
        /// Enable Full Mode
        /// </summary>
        public InternalCmdFull() : base(RoombaCommandCode.Full) { }

        /// <summary>
        /// Enable Full Mode and optionally maintain this mode.
        /// </summary>
        /// <param name="maintainFullMode"></param>
        public InternalCmdFull(bool maintainFullMode)
            : base(RoombaCommandCode.Full)
        {
            if (maintainFullMode)
                MaintainMode = RoombaMode.Full;
        }

        /// <summary>
        /// The string representation for this command.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("Full()");
        }

    }

    /// <summary>
    /// Puts Roomba to sleep, the same as a normal "power" button press.
    /// <remarks>Internal: Use CmdSetMode from outside of this service</remarks>
    /// <remarks>The Device Detect line must be held low for 500 ms to
    /// wake up Roomba from sleep. This command puts the SCI in
    /// passive mode.
    /// </remarks>
    /// </summary>
    [DataContract]
    public class InternalCmdPower : RoombaCommand
    {
        /// <summary>
        /// Default Constructor
        /// </summary>
        public InternalCmdPower() : base(RoombaCommandCode.Power) { }

        /// <summary>
        /// The string representation for this command.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("Power()");
        }

    }


    /// <summary>
    /// Resets Roomba to sleep mode, the same as a normal "power" button press.
    /// <remarks>
    /// In most cases, you can power cycle the Create robot to reset it, 
    /// either with the power button or by transmitting two low-to-high transitions 
    /// on the power toggle input line on the Cargo Bay Connector (see the Open 
    /// Interface manual for details). </remarks>
    /// <remarks>
    /// Always wait at least one second between powering the robot off and powering 
    /// it on again to ensure a complete reset.</remarks>
    /// <remarks>
    /// However, if the robot is on the charger but not charging because it is in 
    /// Safe or Full mode, this doesn�t work. Instead, use this procedure:</remarks>
    /// <remarks>
    /// 1. Send opcode "7". This is not an official OI opcode, rather it is an 
    /// opcode used by Osmo (a firmware updating device) to initiate a soft reset 
    /// of the robot and force it to run its bootloader.</remarks>
    /// <remarks>
    /// 2. The robot resets. Wait 3 seconds for the bootloader to complete. 
    /// Do NOT send any opcodes while the bootloader is running.</remarks>
    /// <remarks>
    /// 3. The robot should start charging. Note that the robot spews some 
    /// battery-related text if it is charging and not in OI mode. Ignore this text.</remarks>
    /// <remarks>
    /// 4. Send a Start command to get back into the OI (and stop the spew).</remarks>
    /// </summary>
    [DataContract]
    public class InternalCmdReset : RoombaCommand
    {
        /// <summary>
        /// Default Constructor
        /// </summary>
        public InternalCmdReset() : base(RoombaCommandCode.OsmoReset, true) 
        {
            this.CmdTimeoutMs = 3000;
        }

        /// <summary>
        /// The string representation for this command.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("OsmoReset()");
        }

    }


    #endregion

    /// <summary>
    /// Starts a spot cleaning cycle, the same as a normal "spot" button press.
    /// <remarks>This command puts the SCI in passive mode.</remarks>
    /// </summary>
    [DataContract]
    public class CmdSpot : RoombaCommand
    {
        /// <summary>
        /// Default Constructor
        /// </summary>
        public CmdSpot() : base(RoombaCommandCode.Spot) { }

        /// <summary>
        /// The string representation for this command.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("Spot()");
        }

    }

    /// <summary>
    /// Starts a normal cleaning cycle, the same as a normal "clean" button press.
    /// <remarks>This command puts the SCI in passive mode.</remarks>
    /// </summary>
    [DataContract]
    public class CmdClean : RoombaCommand
    {
        /// <summary>
        /// Default Constructor
        /// </summary>
        public CmdClean() : base(RoombaCommandCode.Clean) { }

        /// <summary>
        /// The string representation for this command.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("Clean()");
        }

    }

    /// <summary>
    /// Starts a maximum time cleaning cycle, the same as a normal "max" button press.
    /// <remarks>This command puts the SCI in passive mode.</remarks>
    /// </summary>
    [DataContract]
    public class CmdMax : RoombaCommand
    {
        /// <summary>
        /// Default Constructor
        /// </summary>
        public CmdMax() : base(RoombaCommandCode.Max) { }

        /// <summary>
        /// The string representation for this command.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("Max()");
        }

    }

    /// <summary>
    /// Controls Roomba's drive wheels.
    /// <remarks>The command takes four data
    /// bytes, which are interpreted as two 16 bit signed values using
    /// twos-complement. The first two bytes specify the average velocity
    /// of the drive wheels in millimeters per second (mm/s), with the
    /// high byte sent first. The next two bytes specify the radius, in
    /// millimeters, at which Roomba should turn. The longer radii make
    /// Roomba drive straighter; shorter radii make it turn more. A Drive
    /// command with a positive velocity and a positive radius will make
    /// Roomba drive forward while turning toward the left. A negative
    /// radius will make it turn toward the right. Special cases for the
    /// radius make Roomba turn in place or drive straight, as specified
    /// below. </remarks>
    /// </summary>
    [DataContract]
    [Description("Specifies a drive command.")]
    public class CmdDrive : RoombaCommand
    {
        /// <summary>
        /// Default Constructor
        /// </summary>
        public CmdDrive() : base(RoombaCommandCode.Drive) { }

        /// <summary>
        /// Initialization constructor
        /// </summary>
        /// <param name="velocity">mm/s (-500 to +500)</param>
        /// <param name="radius">Radius in mm (-2000 to +2000, 32768)
        /// 1 = counter clockwise when velocity greate than 0 and clockwise when velocity less than 0.
        /// -1 = counter clockwise when velocity less than 0 and clockwise when velocity greater than 0.
        /// 32768 = drive straight ahead</param>
        public CmdDrive(int velocity, int radius)
            : base(RoombaCommandCode.Drive)
        {
            this.Velocity = velocity;
            this.Radius = radius;
        }

        /// <summary>
        /// Velocity (-500 to +500 mm/s)
        /// </summary>
        [DataMember]
        [DataMemberConstructor(Order = 1)]
        [Description ("Specifies the speed setting (mm/sec).\n(Range = -500 to +500)")]
        public int Velocity
        {
            get { return ByteArray.BigEndianGetShort(this.Data, 0); }
            set { ByteArray.BigEndianSetShort(this.Data, 0, value); }
        }

        /// <summary>
        /// Radius (-2000 to +2000 mm, 32768 straight)
        /// </summary>
        [DataMember]
        [DataMemberConstructor(Order = 2)]
        [Description ("Specifies the radius setting (mm).\n(Range = -2000 to +2000; 32768 drives straight ahead)")]
        public int Radius
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
            return string.Format("Drive(Velocity={0},Radius={1})", this.Velocity, this.Radius);
        }
    }


    /// <summary>
    /// Controls Roomba�s cleaning motors.
    /// <remarks>The state of each motor is specified by one bit
    /// in the data byte. </remarks>
    /// </summary>
    [DataContract]
    [Description("Specifies cleaning motors command.")]
    public class CmdMotors : RoombaCommand
    {
        /// <summary>
        /// Default Constructor
        /// </summary>
        public CmdMotors() : base(RoombaCommandCode.Motors) { }

        /// <summary>
        /// Initialization Constructor
        /// </summary>
        public CmdMotors(RoombaMotorBits motorBits)
            : base(RoombaCommandCode.Motors)
        {
            this.Motors = motorBits;
        }

        /// <summary>
        /// Motors
        /// </summary>
        [DataMember, DataMemberConstructor]
        [Description ("Identifies the set of motors.")]
        public RoombaMotorBits Motors
        {
            get { return (RoombaMotorBits)Data[0]; }
            set { Data[0] = (byte)value; }
        }

        /// <summary>
        /// The string representation for this command.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("Motors(Motors={0})", this.Motors);
        }

    }


    /// <summary>
    /// Controls Roomba�s LEDs.
    /// <remarks>The state of each of the spot, clean,
    /// max, and dirt detect LEDs is specified by one bit in the first data
    /// byte. The color of the status LED is specified by two bits in the
    /// first data byte. The power LED is specified by two data bytes, one
    /// for the color and one for the intensity. </remarks>
    /// </summary>
    [DataContract]
    [Description("Specifies an LED command.")]
    public class CmdLeds : RoombaCommand
    {
        /// <summary>
        /// Default Constructor
        /// </summary>
        public CmdLeds() : base(RoombaCommandCode.Leds) { }

        /// <summary>
        /// Initialization Constructor
        /// </summary>
        public CmdLeds(RoombaLedBits ledBits, int powerColor, int powerIntensity)
            : base(RoombaCommandCode.Leds)
        {
            this.Leds = ledBits;
            this.PowerColor = powerColor;
            this.PowerIntensity = powerIntensity;
        }

        /// <summary>
        /// iRobot LEDs
        /// </summary>
        [DataMember, DataMemberConstructor(Order = 1)]
        [Description("Identifies the set of LEDs.")]
        public RoombaLedBits Leds
        {
            get { return (RoombaLedBits)Data[0]; }
            set { Data[0] = (byte)value; }
        }

        /// <summary>
        /// Power Color
        /// </summary>
        [DataMember, DataMemberConstructor(Order = 2)]
        [Description ("Specifies the color for the LED.")]
        public int PowerColor
        {
            get { return (int)Data[1]; }
            set { Data[1] = (byte)value; }
        }

        /// <summary>
        /// Power Intensity
        /// </summary>
        [DataMember, DataMemberConstructor(Order = 3)]
        [Description ("Specifies the intensity setting for the LED.")]
        public int PowerIntensity
        {
            get { return (int)Data[2]; }
            set { Data[2] = (byte)value; }
        }

        /// <summary>
        /// The string representation for this command.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("Leds(Leds={0},PowerColor={1},PowerIntensity={2})", this.Leds, this.PowerColor, this.PowerIntensity);
        }

    }

    /// <summary>
    /// Specifies a song to the SCI to be played later.
    /// <remarks>Each song is
    /// associated with a song number which the Play command uses
    /// to select the song to play. Users can specify up to 16 songs
    /// with up to 16 notes per song. Each note is specified by a note
    /// number using MIDI note definitions and a duration specified
    /// in fractions of a second. The number of data bytes varies
    /// depending on the length of the song specified. A one note song
    /// is specified by four data bytes. For each additional note, two data
    /// bytes must be added. </remarks>
    /// </summary>
    [DataContract]
    [Description("Specifies a song definition.")]
    public class CmdDefineSong : RoombaCommand
    {
        private List<RoombaNote> _notes = new List<RoombaNote>();
        private int _songNumber = 1;

        /// <summary>
        /// Default Constructor
        /// </summary>
        public CmdDefineSong() : base(RoombaCommandCode.DefineSong) { }

        /// <summary>
        /// Defines a Song.
        /// </summary>
        public CmdDefineSong(int songNumber)
            : base(RoombaCommandCode.DefineSong)
        {
            this.SongNumber = songNumber;
        }

        /// <summary>
        /// Defines a Song.
        /// <remarks>Song Length is no longer necessary</remarks>
        /// </summary>
        [Obsolete("Song length is no longer necessary. Use CmdSong(songNumber).")]
        public CmdDefineSong(int songNumber, int songLength)
            : base(RoombaCommandCode.DefineSong)
        {
            this.SongNumber = songNumber;
        }

        /// <summary>
        /// Defines a Song.
        /// </summary>
        public CmdDefineSong(int songNumber, List<RoombaNote> notes)
            : base(RoombaCommandCode.DefineSong)
        {
            this.SongNumber = songNumber;
            this.Notes = notes;
        }

        /// <summary>
        /// The predefined song number (1-16)
        /// </summary>
        [DataMember, DataMemberConstructor(Order = 1)]
        [Description("Specifies the number for a pre-defined song (1-16).")]
        public int SongNumber
        {
            get { return _songNumber; }
            set
            {
                if (value < 1)
                    _songNumber = 1;
                else if (value > 16)
                    _songNumber = 16;
                else
                    _songNumber = value;
            }
        }

        /// <summary>
        /// A list of notes which compose a short song.
        /// </summary>
        [DataMember, DataMemberConstructor(Order = 2)]
        [Description("Specifies a set of notes for a song.")]
        public List<RoombaNote> Notes
        {
            get { return _notes; }
            set
            {
                if (value == null)
                    _notes.Clear();
                else
                    _notes = value;

                // Truncate the song to 16 notes.
                if (_notes.Count > 16)
                    _notes.RemoveRange(16, _notes.Count - 16);
            }
        }



        /// <summary>
        /// Override Data so we can check to see if
        /// the Notes list has been modified since it was created.
        /// </summary>
        public override byte[] Data
        {
            get
            {
                if (base.Data == null || base.Data.Length != (2 + (this.SongLength * 2)))
                {
                    base.Data = new byte[2 + (this.SongLength * 2)];
                    int ix = 0;
                    base.Data[ix++] = (byte)(this.SongNumber - 1);
                    base.Data[ix++] = (byte)this.SongLength;
                    foreach (RoombaNote note in this._notes)
                    {
                        base.Data[ix++] = (byte)note.Tone;
                        base.Data[ix++] = (byte)note.Duration;
                    }
                }
                return base.Data;
            }
            set
            {
                base.Data = value;
                this._notes.Clear();
                if (base.Data != null && base.Data.Length > 2)
                {
                    _songNumber = (int)(base.Data[0] + 1);
                    int songLength = (int)base.Data[1];
                    if ((2 + (songLength * 2)) == base.Data.Length)
                    {
                        for (int ix = 0; ix < songLength; ix++)
                        {
                            RoombaFrequency tone = (RoombaFrequency)base.Data[2+(ix*2)];
                            int duration = (int)base.Data[3 + (ix*2)];
                            _notes.Add(new RoombaNote(tone, duration));
                        }
                    }
                }
            }
        }



        #region Note Helper Functions


        /// <summary>
        /// Set the (1-16)th Note Tone and Duration
        /// </summary>
        /// <param name="ix">1-16</param>
        /// <param name="tone">RoombaFrequency</param>
        /// <param name="duration">n/64th seconds</param>
        public void SetNote(int ix, RoombaFrequency tone, int duration)
        {
            if (ix > 0 && ix <= 16)
            {
                while (_notes.Count < ix)
                    _notes.Add(new RoombaNote(RoombaFrequency.Rest, 1));

                _notes[ix - 1].Tone = tone;
                _notes[ix - 1].Duration = duration;
            }
        }

        /// <summary>
        /// The number of notes in this song (1-16)
        /// </summary>
        public int SongLength
        {
            get
            {
                if (_notes.Count > 16)
                    return 16;
                return _notes.Count;
            }
        }

        #endregion

        /// <summary>
        /// The string representation for this command.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("DefineSong(...)");
        }

    }


    /// <summary>
    /// Plays one of 16 songs, as specified by an earlier Song command.
    /// <remarks>If the requested song has not been specified yet,
    /// the Play command does nothing. </remarks>
    /// </summary>
    [DataContract]
    [Description("Specifies a play song command.")]
    public class CmdPlaySong : RoombaCommand
    {
        /// <summary>
        /// Plays a song
        /// </summary>
        public CmdPlaySong() : base(RoombaCommandCode.PlaySong) { }

        /// <summary>
        /// Plays the specified song (1-16)
        /// </summary>
        public CmdPlaySong(int songNumber)
            : base(RoombaCommandCode.PlaySong)
        {
            this.SongNumber = songNumber;
        }

        /// <summary>
        /// The predefined song number (1-16)
        /// </summary>
        [DataMember, DataMemberConstructor]
        [Description("Specifies the number for a pre-defined song (1-16).")]
        public int SongNumber
        {
            get { return (int)(Data[0]+1); }
            set { Data[0] = (byte)(value - 1); }
        }

        /// <summary>
        /// The string representation for this command.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("PlaySong(SongNumber={0})", this.SongNumber);
        }

    }

    /// <summary>
    /// Requests the SCI to send a packet of sensor data bytes.
    /// </summary>
    [DataContract]
    [Description("Specifies a send sensor data retrieval command.")]
    public class CmdSensors : RoombaCommand
    {
        int _returnPacketSize = 0;

        /// <summary>
        /// Default Constructor
        /// </summary>
        public CmdSensors() : base(RoombaCommandCode.Sensors) { }

        /// <summary>
        /// Initialization Constructor
        /// </summary>
        public CmdSensors(RoombaCommandCode sensorPacket)
            : base(RoombaCommandCode.Sensors)
        {
            this.SensorPacket = sensorPacket;
        }

        /// <summary>
        /// Initialization Constructor
        /// </summary>
        public CmdSensors(RoombaQueryType queryType)
            : base(RoombaCommandCode.Sensors)
        {
            this.SensorPacketCode = (int)queryType;
        }

        /// <summary>
        /// Initialization Constructor
        /// </summary>
        public CmdSensors(CreateSensorPacket queryType)
            : base(RoombaCommandCode.Sensors)
        {
            this.SensorPacketCode = (int)queryType;
        }

        /// <summary>
        /// CmdSensors expects data back from the iRobot
        /// </summary>
        /// <returns></returns>
        public override int ExpectedResponseBytes()
        {
            return _returnPacketSize;
        }

        /// <summary>
        /// The Sensor Packet Code to retrieve
        /// <remarks>
        /// Roomba (0-3)
        /// Create (0-42)
        /// </remarks>
        /// </summary>
        [Description("Specifies a Sensor Packet code to retrieve.")]
        public int SensorPacketCode
        {
            get { return (int)Data[0]; }
            set
            {
                if (value < 0 || value > 42)
                    throw new ArgumentOutOfRangeException("Invalid Sensor Packet");

                Data[0] = (byte)value;
                _returnPacketSize = IRobotUtility.ReturnPacketSize(this.CreateSensorPacket);
            }
        }

        /// <summary>
        /// The Create Sensor Packet to retrieve
        /// </summary>
        [DataMember, DataMemberConstructor]
        public CreateSensorPacket CreateSensorPacket
        {
            get { return (CreateSensorPacket)Data[0]; }
            set
            {
                if ((int)value < 0 || (int)value > 42)
                    throw new ArgumentOutOfRangeException("Invalid Sensor Packet");

                Data[0] = (byte)value;
                _returnPacketSize = IRobotUtility.ReturnPacketSize(this.CreateSensorPacket);
            }
        }

        /// <summary>
        /// The Roomba Sensor Packet to retrieve
        /// </summary>
        public RoombaCommandCode SensorPacket
        {
            get { return (RoombaCommandCode)Data[0]; }
            set
            {
                if ((int)value < 0 || (int)value > 3)
                    throw new ArgumentOutOfRangeException("The sensor packet is not valid.");

                Data[0] = (byte)value;
                _returnPacketSize = IRobotUtility.ReturnPacketSize(this.CreateSensorPacket);
            }
        }

        /// <summary>
        /// The string representation for this command.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("Sensors(CreateSensorPacket={0})", this.CreateSensorPacket);
        }

    }

    /// <summary>
    /// Turns on force-seeking-dock mode.
    /// <remarks>Causes the robot to immediately attempt to dock during
    /// its cleaning cycle if it encounters the docking beams from the
    /// Home Base. (Note, however, that if the robot was not active in a
    /// clean, spot or max cycle it will not attempt to execute the docking.)
    /// Normally the robot attempts to dock only if the cleaning cycle has
    /// completed or the battery is nearing depletion. This command can be
    /// sent anytime, but the mode will be canceled if the robot turns off,
    /// begins charging, or is commanded into SCI safe or full modes.</remarks>
    /// </summary>
    [DataContract]
    [Description("Specifies a Force Seeking Dock command.")]
    public class CmdForceSeekingDock : RoombaCommand
    {
        /// <summary>
        /// Default Constructor
        /// </summary>
        public CmdForceSeekingDock() : base(RoombaCommandCode.ForceSeekingDock) { }

        /// <summary>
        /// The string representation for this command.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("ForceSeekingDock()");
        }

    }

    #endregion

    #region Roomba Response Packets

    /// <summary>
    /// Acknowledges the receipt of a Roomba command
    /// </summary>
    [DataContract]
    public class RoombaCommandReceived : RoombaReturnPacket
    {
        private static RoombaCommandReceived defaultInstanceOff = new RoombaCommandReceived(RoombaMode.Uninitialized);
        private static RoombaCommandReceived defaultInstanceSleep = new RoombaCommandReceived(RoombaMode.Off);
        private static RoombaCommandReceived defaultInstancePassive = new RoombaCommandReceived(RoombaMode.Passive);
        private static RoombaCommandReceived defaultInstanceSafe = new RoombaCommandReceived(RoombaMode.Safe);
        private static RoombaCommandReceived defaultInstanceFull = new RoombaCommandReceived(RoombaMode.Full);
        private static RoombaCommandReceived defaultInstanceShutdown = new RoombaCommandReceived(RoombaMode.Shutdown);

        /// <summary>
        /// Default Instance of RoombaCommandReceived
        /// </summary>
        public static RoombaCommandReceived Instance(RoombaMode roombaMode)
        {
            switch (roombaMode)
            {
                case RoombaMode.Safe:
                    return defaultInstanceSafe;
                case RoombaMode.Passive:
                    return defaultInstancePassive;
                case RoombaMode.Full:
                    return defaultInstanceFull;
                case RoombaMode.Off:
                    return defaultInstanceSleep;
                case RoombaMode.Shutdown:
                    return defaultInstanceShutdown;
                default:
                    return defaultInstanceOff;
            }
        }

        /// <summary>
        /// Default command received
        /// </summary>
        public RoombaCommandReceived()
        {
            this.RoombaMode = RoombaMode.Off;
        }

        /// <summary>
        /// Roomba Command Received with Mode
        /// </summary>
        /// <param name="roombaMode"></param>
        public RoombaCommandReceived(RoombaMode roombaMode)
        {
            this.RoombaMode = roombaMode;
        }

        /// <summary>
        /// Roomba Command Received with Command Code
        /// </summary>
        /// <param name="roombaCommandCode"></param>
        public RoombaCommandReceived(RoombaCommandCode roombaCommandCode)
        : base(roombaCommandCode)
        {
        }

        /// <summary>
        /// The Roomba Mode after the command is completed
        /// </summary>
        [DataMember]
        [DataMemberConstructor]
        public override RoombaMode RoombaMode
        {
            get
            {
                return base.RoombaMode;
            }
            set
            {
                base.RoombaMode = value;
            }
        }

        /// <summary>
        /// All RoombaCommandReceived packets are valid.
        /// </summary>
        public override bool ValidPacket
        {
            get
            {
                return true;
            }
        }
    }

    /// <summary>
    /// The standard return package in which all return messages inherit from
    /// </summary>
    [DataContract]
    public class RoombaReturnPacket : RoombaCommand
    {
        private roomba.RoombaMode _roombaMode;
        private DateTime _timestamp;

        /// <summary>
        /// Default Constructor
        /// </summary>
        public RoombaReturnPacket() { }

        /// <summary>
        /// Initialization Constructor
        /// </summary>
        public RoombaReturnPacket(RoombaCommandCode command)
            : base(command)
        {
            this.RoombaMode = RoombaMode.NotSpecified;
        }

        /// <summary>
        /// Initialization Constructor
        /// </summary>
        public RoombaReturnPacket(RoombaCommandCode command, roomba.RoombaMode roombaMode)
            : base(command)
        {
            this.RoombaMode = roombaMode;
        }

        /// <summary>
        /// The current mode of the Roomba after the command
        /// </summary>
        [DataMember]
        [Description("Indicates the current Roomba mode.")]
        public virtual roomba.RoombaMode RoombaMode
        {
            get { return _roombaMode; }
            set { _roombaMode = value; }
        }

        /// <summary>
        /// Is the current packet valid?
        /// </summary>
        public virtual bool ValidPacket
        {
            get { return false; }
        }

        /// <summary>
        /// The timestamp when the data was received.
        /// </summary>
        [DataMember]
        [Description("Indicates the time the data was received (ms).")]
        public DateTime Timestamp
        {
            get { return _timestamp; }
            set { _timestamp = value; }
        }


    }


    /// <summary>
    /// Returns the Firmware Date.
    /// </summary>
    [DataContract]
    public class ReturnFirmwareDate: RoombaReturnPacket
    {
        /// <summary>
        /// Returns the Firmware Date.
        /// </summary>
        public ReturnFirmwareDate()
            : base(RoombaCommandCode.ReturnFirmwareDate)
        {
            this.Timestamp = DateTime.Now;
        }

        /// <summary>
        /// Returns the Firmware Date.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="timestamp"></param>
        public ReturnFirmwareDate(byte[] data, DateTime timestamp)
            : base(RoombaCommandCode.ReturnFirmwareDate)
        {
            this.Data = data;
            this.Timestamp = timestamp;
            if (ValidPacket)
            {
                this.FirmwareDate = new DateTime(2000 + base.Data[1], base.Data[2], base.Data[3], base.Data[4], base.Data[5], 0);
            }
        }

        /// <summary>
        /// Valid Return Firmware Date Packet
        /// </summary>
        public override bool ValidPacket
        {
            get
            {
                return IRobotUtility.FindFirmwarePacket(this.Data, 0, this.Data.Length) == 0;
            }
        }

        /// <summary>
        /// The firmware Date
        /// </summary>
        [DataMember]
        [Description("Identifies the robot's firmware date.")]
        public DateTime FirmwareDate;
    }

    /// <summary>
    /// Notification of Roomba Mode changes
    /// </summary>
    [DataContract]
    public class ReturnMode
    {
        /// <summary>
        /// Notification of Roomba Mode changes
        /// </summary>
        public ReturnMode() { }

        /// <summary>
        /// Notification of Roomba Mode changes
        /// </summary>
        /// <param name="roombaMode"></param>
        public ReturnMode(roomba.RoombaMode roombaMode) { this.RoombaMode = roombaMode; }

        /// <summary>
        /// Notification of Roomba Mode changes
        /// </summary>
        /// <param name="roombaMode"></param>
        /// <param name="maintainMode"></param>
        public ReturnMode(roomba.RoombaMode roombaMode, RoombaMode maintainMode)
        {
            this.RoombaMode = roombaMode;
            this.MaintainMode = maintainMode;
        }

        /// <summary>
        /// Notification of Roomba Mode changes
        /// </summary>
        /// <param name="roombaMode"></param>
        /// <param name="maintainMode"></param>
        /// <param name="iRobotModel"></param>
        /// <param name="firmwareDate"></param>
        public ReturnMode(roomba.RoombaMode roombaMode, RoombaMode maintainMode, IRobotModel iRobotModel, DateTime firmwareDate)
        {
            this.RoombaMode = roombaMode;
            this.MaintainMode = maintainMode;
            this.IRobotModel = iRobotModel;
        }

        /// <summary>
        /// Validate the RoombaMode
        /// </summary>
        public bool ValidPacket
        {
            get
            {
                return ((int)this.RoombaMode >= (int)RoombaMode.Shutdown && (int)this.RoombaMode <= (int)RoombaMode.Full);
            }
        }

        /// <summary>
        /// The current mode of the Roomba
        /// </summary>
        [DataMember]
        [Description("Identifies the current operating mode.")]
        public roomba.RoombaMode RoombaMode;

        /// <summary>
        /// The iRobot hardware model
        /// </summary>
        [DataMember]
        [Description("Identifies the iRobot robot model.")]
        public roomba.IRobotModel IRobotModel;

        /// <summary>
        /// When set to Passive, Safe, or Full, this mode will be maintained with
        /// each subsequent command and also involuntary mode changes.
        /// <remarks>
        /// Off = Ignore for this command
        /// NotSpecified = Maintain Mode is Not Specified (disabled)
        /// </remarks>
        /// </summary>
        [DataMember]
        [Description("Identifies the mode which will be maintained after each command is completed.\n(Off = Ignore,  NotSpecified = Disabled)")]
        public roomba.RoombaMode MaintainMode;

        /// <summary>
        /// The Firmware Date
        /// </summary>
        [DataMember(IsRequired = false)]
        [Description("Identifies the robot's firmware date.")]
        public DateTime FirmwareDate;
    }

    /// <summary>
    /// Notification of All Sensors.
    /// Roomba: Sensors, Pose, Power
    /// Create: Sensors, Pose, Power, CliffDetail, and Telemetry
    /// </summary>
    [DataContract]
    public class ReturnAll : RoombaReturnPacket
    {
        private ReturnPose _returnPose = new ReturnPose();
        private ReturnPower _returnPower = new ReturnPower();
        private ReturnSensors _returnSensors = new ReturnSensors();
        private ReturnCliffDetail _returnCliffDetail = null;
        private ReturnTelemetry _returnTelemetry = null;

        /// <summary>
        /// Default Constructor
        /// </summary>
        public ReturnAll() : base(RoombaCommandCode.ReturnAllCreate) { }

        /// <summary>
        /// Initialization Constructor
        /// </summary>
        /// <param name="roombaMode"></param>
        public ReturnAll(RoombaMode roombaMode) : base(RoombaCommandCode.ReturnAllCreate, roombaMode) { }

        /// <summary>
        /// Is the current packet valid?
        /// </summary>
        public override bool ValidPacket
        {
            get
            {
                if (this._returnCliffDetail != null && !this._returnCliffDetail.ValidPacket)
                    return false;

                if (this._returnTelemetry != null && !this._returnTelemetry.ValidPacket)
                    return false;

                // If all sub-packets are empty, the entire ReturnAll is invalid
                if (this._returnSensors == null
                    && this._returnPose == null
                    && this._returnPower == null
                    && this._returnTelemetry == null
                    && this._returnCliffDetail == null)
                    return false;

                if (this._returnSensors != null && !this._returnSensors.ValidPacket)
                    return false;

                if (this._returnPose != null && !this._returnPose.ValidPacket)
                    return false;

                if (this._returnPower != null && !this._returnPower.ValidPacket)
                    return false;

                return true;
            }
        }

        /// <summary>
        /// Set the RoombaMode on the ReturnAll packet and all sub-packets.
        /// </summary>
        [DataMember]
        [Description("Indicates the current Roomba value.")]
        public override RoombaMode RoombaMode
        {
            get { return base.RoombaMode; }
            set
            {
                base.RoombaMode = value;
                if (this.Sensors != null)
                    this.Sensors.RoombaMode = value;
                if (this.Pose != null)
                    this.Pose.RoombaMode = value;
                if (this.Power != null)
                    this.Power.RoombaMode = value;
                if (this.CliffDetail != null)
                    this.CliffDetail.RoombaMode = value;
                if (this.Telemetry != null)
                    this.Telemetry.RoombaMode = value;
            }
        }


        /// <summary>
        /// Roomba Sensors
        /// </summary>
        /// <returns></returns>
        [DataMember(IsRequired = false)]
        [Description("Identifies the set of sensors.")]
        public ReturnSensors Sensors
        {
            get { return _returnSensors; }
            set
            {
                _returnSensors = value;
                if (this.Data != null)
                    ByteArray.CopyTo(this.Data, value.Data, 0, 0, 10);
            }
        }

        /// <summary>
        /// Roomba Pose
        /// </summary>
        /// <returns></returns>
        [DataMember(IsRequired=false)]
        [Description("Identifies the position and orientation of the robot.")]
        public ReturnPose Pose
        {
            get { return _returnPose; }
            set
            {
                _returnPose = value;
                if (this.Data != null)
                    ByteArray.CopyTo(this.Data, value.Data, 0, 10, 6);
            }
        }

        /// <summary>
        /// Roomba Power
        /// </summary>
        /// <returns></returns>
        [DataMember(IsRequired = false)]
        [Description("Indicates the current power setting.")]
        public ReturnPower Power
        {
            get { return _returnPower; }
            set
            {
                _returnPower = value;
                if (this.Data != null)
                    ByteArray.CopyTo(this.Data, value.Data, 0, 16, 10);
            }
        }

        /// <summary>
        /// iRobot Create Cliff Detail
        /// </summary>
        [DataMember(IsRequired = false)]
        public ReturnCliffDetail CliffDetail
        {
            get { return _returnCliffDetail; }
            set
            {
                _returnCliffDetail = value;
                if (this.Data != null && this.Data.Length >= 52)
                    ByteArray.CopyTo(this.Data, value.Data, 26, 0, 14);
            }
        }

        /// <summary>
        /// iRobot Create Telemetry
        /// </summary>
        [DataMember(IsRequired = false)]
        public ReturnTelemetry Telemetry
        {
            get { return _returnTelemetry; }
            set
            {
                _returnTelemetry = value;
                if (this.Data != null && this.Data.Length >= 52)
                    ByteArray.CopyTo(this.Data, value.Data, 40, 0, 12);
            }
        }

    }

    /// <summary>
    /// Roomba Sensor Notifications
    /// </summary>
    [DataContract]
    public class ReturnSensors : RoombaReturnPacket
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


                if ((this.Data[0] >= 0x20)  // bumps and wheel drops
                    || (this.Data[1] > 1)   // wall
                    || (this.Data[2] > 1)   // CliffLeft
                    || (this.Data[3] > 1)   // CliffFrontLeft
                    || (this.Data[4] > 1)   // CliffFrontRight
                    || (this.Data[5] > 1)   // CliffRight
                    || (this.Data[6] > 1)   // VirtualWall
                    || (this.Data[7] >= 0x20) // MotorOvercurrents
                    )
                    return false;

                return true;
            }
        }

        /// <summary>
        /// Default Constructor
        /// </summary>
        public ReturnSensors() : base(RoombaCommandCode.ReturnBumpsCliffsAndWalls) { }

        /// <summary>
        /// Initialization Constructor
        /// </summary>
        public ReturnSensors(RoombaMode roombaMode, byte[] data, DateTime timestamp)
            : base(RoombaCommandCode.ReturnBumpsCliffsAndWalls, roombaMode)
        {
            this.Data = data;
            this.Timestamp = timestamp;
        }

        /// <summary>
        /// Initialization Constructor
        /// </summary>
        public ReturnSensors(RoombaMode roombaMode)
            : base(RoombaCommandCode.ReturnBumpsCliffsAndWalls, roombaMode){}

        /// <summary>
        /// The state of the bump (0 = no bump, 1 = bump) and wheeldrop
        /// sensors (0 = wheel up, 1 = wheel dropped) are sent as individual
        /// bits.
        /// <remarks>Some robots do not report the three wheel drops separately. Instead,
        /// if any of the three wheels drops, all three wheel-drop bits will be set. You
        /// can tell which kind of robot you have by examining the serial number
        /// inside the battery compartment. Wheel drops are separate only if there is
        /// an �E� in the serial number.</remarks>
        /// </summary>
        [DataMember]
        [Description ("Identifies the state of the bump and wheel drop sensors.")]
        public BumpsWheeldrops BumpsWheeldrops
        {
            get { return (BumpsWheeldrops)Data[0]; }
            set { Data[0] = (byte)value; }
        }

        /// <summary>
        /// The state of the wall sensor.
        /// </summary>
        [DataMember]
        [Description ("Identifies the state of the wall sensor.")]
        public bool Wall
        {
            get { return (Data[1] == 0x01); }
            set { Data[1] = (byte)((value) ? 1 : 0); }
        }

        /// <summary>
        /// The state of the cliff sensor on the left side of Roomba.
        /// </summary>
        [DataMember]
        [Description ("Identifies the state of the left side cliff sensor.")]
        public bool CliffLeft
        {
            get { return (Data[2] == 0x01); }
            set { Data[2] = (byte)((value) ? 1 : 0); }
        }

        /// <summary>
        /// The state of the cliff sensor on the front left side of Roomba.
        /// </summary>
        [DataMember]
        [Description ("Identifies the state of the front left cliff sensor.")]
        public bool CliffFrontLeft
        {
            get { return (Data[3] == 0x01); }
            set { Data[3] = (byte)((value) ? 1 : 0); }
        }

        /// <summary>
        /// The state of the cliff sensor on the front right side of Roomba.
        /// </summary>
        [DataMember]
        [Description ("Identifies the state of the front right cliff sensor.")]
        public bool CliffFrontRight
        {
            get { return (Data[4] == 0x01); }
            set { Data[4] = (byte)((value) ? 1 : 0); }
        }

        /// <summary>
        /// The state of the cliff sensor on the right side of Roomba.
        /// </summary>
        [DataMember]
        [Description ("Identifies the state of the right side cliff sensor.")]
        public bool CliffRight
        {
            get { return (Data[5] == 0x01); }
            set { Data[5] = (byte)((value) ? 1 : 0); }
        }

        /// <summary>
        /// The state of the virtual wall detector.
        /// </summary>
        [DataMember]
        [Description ("Identifies the virtual wall detector state.")]
        public bool VirtualWall
        {
            get { return (Data[6] == 0x01); }
            set { Data[6] = (byte)((value) ? 1 : 0); }
        }

        /// <summary>
        /// The state of the five motors� overcurrent sensors.
        /// </summary>
        [DataMember]
        [Description ("Identifies the state of the five motors' overcurrent sensors.")]
        public MotorOvercurrents MotorOvercurrents
        {
            get { return (MotorOvercurrents)Data[7]; }
            set { Data[7] = (byte)value; }
        }

        /// <summary>
        /// The current dirt detection level of the left side dirt detector
        /// is sent as a one byte value. A value of 0 indicates no dirt is
        /// detected. Higher values indicate higher levels of dirt detected.
        /// </summary>
        [DataMember]
        [Description ("Identifies the level of the left dirt detection sensor.\n(0 = no dirt detected)")]
        public int DirtDetectorLeft
        {
            get { return (int)Data[8]; }
            set { Data[8] = (byte)value; }
        }

        /// <summary>
        /// The current dirt detection level of the right side dirt detector
        /// is sent as a one byte value. A value of 0 indicates no dirt is
        /// detected. Higher values indicate higher levels of dirt detected.
        /// <remarks>Some robots don�t have a right dirt detector. You can tell by removing
        /// the brushes. The dirt detectors are metallic disks. For robots with no right
        /// dirt detector this byte is always 0.</remarks>
        /// </summary>
        [DataMember]
        [Description("Identifies the level of the right dirt detection sensor.\n(0 = no dirt detected)")]
        public int DirtDetectorRight
        {
            get { return (int)Data[9]; }
            set { Data[9] = (byte)value; }
        }
    }


    /// <summary>
    /// Roomba Pose Notifications
    /// </summary>
    [DataContract]
    public class ReturnPose : RoombaReturnPacket
    {
        /// <summary>
        /// Is the current packet valid?
        /// </summary>
        public override bool ValidPacket
        {
            get
            {
                if (this.Data == null || this.Data.Length < 6)
                    return false;

                if (this.Data[1] >= 0x10)  // Buttons
                    return false;

                return true;
            }
        }


        /// <summary>
        /// Default Constructor
        /// </summary>
        public ReturnPose() : base(RoombaCommandCode.ReturnPose) { }

        /// <summary>
        /// Initialization Constructor
        /// </summary>
        public ReturnPose(RoombaMode roombaMode, byte[] data)
            : base(RoombaCommandCode.ReturnPose, roombaMode)
        {
            this.Data = data;
        }

        /// <summary>
        /// Initialization Constructor
        /// </summary>
        /// <param name="roombaMode"></param>
        public ReturnPose(RoombaMode roombaMode)
            : base(RoombaCommandCode.ReturnPose, roombaMode) { }

        /// <summary>
        /// The command number of the infrared command currently
        /// being received by Roomba. A value of 255 indicates that no
        /// remote control command is being received. See Roomba remote
        /// control documentation for a description of the command values.
        /// <remarks>Range: 0 � 255 (with some values unused)</remarks>
        /// </summary>
        [DataMember]
        [Description ("Identifies the command number received from the infrared remote control; range 0 - 255.\n  255 = no infrared remote control command received.")]
        public RemoteIR RemoteControlCommand
        {
            get
            {
                try
                {
                    return (RemoteIR)Data[0];
                }
                catch
                {
                    return RemoteIR.NoIR;
                }
            }
            set
            {
                Data[0] = (byte)value;
            }
        }

        /// <summary>
        /// The state of the Roomba buttons.
        /// </summary>
        /// <remarks>Roomba and Create share the same storage for Buttons, but the values are interpreted slightly different.</remarks>
        [DataMember]
        [Description ("Identifies the state of the Roomba command buttons.")]
        public ButtonsRoomba ButtonsRoomba
        {
            get { return (ButtonsRoomba)Data[1]; }
            set { Data[1] = (byte)value; }
        }

        /// <summary>
        /// The state of the Create buttons.
        /// </summary>
        /// <remarks>Roomba and Create share the same storage for Buttons, but the values are interpreted slightly different.</remarks>
        [DataMember]
        [Description("Identifies the state of the Create command buttons.")]
        public ButtonsCreate ButtonsCreate
        {
            get { return (ButtonsCreate)Data[1]; }
            set { Data[1] = (byte)value; }
        }

        /// <summary>
        /// The distance that Roomba has traveled in millimeters since the
        /// distance it was last requested. This is the same as the sum of
        /// the distance traveled by both wheels divided by two. Positive
        /// values indicate travel in the forward direction; negative in the
        /// reverse direction. If the value is not polled frequently enough, it
        /// will be capped at its minimum or maximum.
        /// </summary>
        [DataMember]
        [Description ("Identifies the distance traveled (in mm) since last requested.\n(Right wheel distance + Left wheel distance)/2\nPositive values = forward, negative values = reverse")]
        public int Distance
        {
            get { return ByteArray.BigEndianGetShort(this.Data, 2); }
            set { ByteArray.BigEndianSetShort(this.Data, 2, value); }
        }

        /// <summary>
        /// The angle that Roomba has turned through since the angle was
        /// last requested. The angle is expressed as the difference in
        /// the distance traveled by Roomba�s two wheels in millimeters,
        /// specifically the right wheel distance minus the left wheel
        /// distance, divided by two.
        /// <example>This makes counter-clockwise angles
        /// positive and clockwise angles negative. This can be used to
        /// directly calculate the angle that Roomba has turned through
        /// since the last request. Since the distance between Roomba�s
        /// wheels is 258mm, the equations for calculating the angles in
        /// familiar units are:
        /// Angle in radians = (2 * difference) / 258
        /// Angle in degrees = (360 * difference) / (258 * Pi).
        /// </example>
        /// <remarks>If the value is not polled frequently enough, it will
        /// be capped at its minimum or maximum. Reported angle and
        /// distance may not be accurate. Roomba measures these by detecting
        /// its wheel revolutions. If for example, the wheels slip on the
        /// floor, the reported angle of distance will be greater than the
        /// actual angle or distance.</remarks>
        /// </summary>
        [DataMember]
        [Description ("Identifies the most recent angle (in mm) since last requested.\n(Right wheel distance - Left wheel distance)/2\nPositive values = counter-clockwise, negative values = clockwise")]
        public int Angle
        {
            get { return ByteArray.BigEndianGetShort(this.Data, 4); }
            set { ByteArray.BigEndianSetShort(this.Data, 4, value); }
        }

    }

    /// <summary>
    /// Roomba Power Notifications
    /// </summary>
    [DataContract]
    public class ReturnPower : RoombaReturnPacket
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


                if ((this.Data[0] > 5)          // Charging State
                    || (this.Voltage > 21000)
                    || (this.Current < -50000)
                    || (this.Current > 50000)
                    || (this.Charge < -50000)
                    || (this.Charge > 50000)
                    || (this.Capacity < -50000)
                    || (this.Capacity > 50000)
                    )
                    return false;

                return true;
            }
        }

        /// <summary>
        /// Default Constructor
        /// </summary>
        public ReturnPower() : base(RoombaCommandCode.ReturnPower) { }

        /// <summary>
        /// Initialization Constructor
        /// </summary>
        public ReturnPower(RoombaMode roombaMode, byte[] data)
            : base(RoombaCommandCode.ReturnPower, roombaMode)
        {
            this.Data = data;
        }

        /// <summary>
        /// Initialization Constructor
        /// </summary>
        public ReturnPower(RoombaMode roombaMode)
            : base(RoombaCommandCode.ReturnPower, roombaMode) { }

        /// <summary>
        /// A code indicating the current charging state of Roomba.
        /// </summary>
        [DataMember]
        [Description ("Indicates the battery charging state.")]
        public ChargingState ChargingState
        {
            get
            {
                try
                {
                    return (ChargingState)Data[0];
                }
                catch
                {
                    return ChargingState.NoResponse;
                }
            }
            set
            {
                try
                {
                    ChargingState newValue = value;
                    Data[0] = (byte)(newValue);
                }
                catch
                {
                    Data[0] = (byte)ChargingState.NoResponse;
                }
            }
        }

        /// <summary>
        /// The voltage of Roomba's battery in millivolts (mV)
        /// </summary>
        [DataMember]
        [Description ("Indicates the current voltage of the battery (in mV).")]
        public int Voltage
        {
            get { return ByteArray.BigEndianGetUShort(this.Data, 1); }
            set { ByteArray.BigEndianSetUShort(this.Data, 1, value); }
        }

        /// <summary>
        /// The current in milliamps (mA) flowing into or out of Roomba�s
        /// battery. Negative currents indicate current is flowing out of the
        /// battery, as during normal running. Positive currents indicate
        /// current is flowing into the battery, as during charging.
        /// </summary>
        [DataMember]
        [Description ("Indicates the current flowing in or out the battery (mV).")]
        public int Current
        {
            get { return ByteArray.BigEndianGetShort(this.Data, 3); }
            set { ByteArray.BigEndianSetShort(this.Data, 3, value); }
        }

        /// <summary>
        /// The temperature of Roomba�s battery in degrees Celsius.
        /// </summary>
        [DataMember]
        [Description ("Indicates the current temperature of the battery (degrees Celsius).")]
        public int Temperature
        {
            get
            {
                if (Data[5] >= 128)
                    return (int)Data[5] - 256;
                return (int)Data[5];
            }
            set
            {
                if (value < 0)
                    Data[5] = (byte)(256 + value);
                else
                    Data[5] = (byte)value;
            }
        }


        /// <summary>
        /// The current charge of Roomba�s battery in milliamp-hours (mAh).
        /// The charge value decreases as the battery is depleted during
        /// running and increases when the battery is charged.
        /// </summary>
        [DataMember]
        [Description ("Indicates the current charge of the battery (mAh).")]
        public int Charge
        {
            get { return ByteArray.BigEndianGetUShort(this.Data, 6); }
            set { ByteArray.BigEndianSetUShort(this.Data, 6, value); }
        }


        /// <summary>
        /// The estimated charge capacity of Roomba�s battery. When the
        /// Charge value reaches the Capacity value, the battery is fully
        /// charged.
        /// </summary>
        [DataMember]
        [Description ("Identifies the estimated charge capacity of the battery.")]
        public int Capacity
        {
            get { return ByteArray.BigEndianGetUShort(this.Data, 8); }
            set { ByteArray.BigEndianSetUShort(this.Data, 8, value); }
        }
    }

    /// <summary>
    /// RoombaCommandReceived Notification
    /// </summary>
    public class RoombaCommandReceivedNotification : Insert<RoombaCommandReceived, DsspResponsePort<DefaultInsertResponseType>>
    {
        /// <summary>
        /// Default Constructor
        /// </summary>
        /// <param name="data"></param>
        public RoombaCommandReceivedNotification(RoombaCommandReceived data) { this.Body = data; }
    }

    #endregion

    /// <summary>
    /// Byte Array static methods
    /// </summary>
    public static class ByteArray
    {

        #region Helper Methods

        /// <summary>
        /// Do the two byte arrays contain identical data?
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public static bool IsEqual(byte[] first, byte[] second)
        {
            if (first == null && second == null)
                return true;
            if (first == null || second == null)
                return false;
            if (first.Length != second.Length)
                return false;

            for (int ix = 0; ix < first.Length; ix++)
            {
                if (first[ix] != second[ix])
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Set a signed 2-byte short integer
        /// <remarks>Big-Endian encoding</remarks>
        /// </summary>
        /// <param name="data"></param>
        /// <param name="startPosition"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool BigEndianSetShort(byte[] data, int startPosition, int value)
        {
            // Do we have room in the buffer?
            if (data == null)
                throw new ArgumentNullException("data");

            if (startPosition > data.Length - 2)
                throw new ArgumentOutOfRangeException("startPosition");

            data[startPosition + 1] = (byte)(value & 0xFF); //LSB is second!
            data[startPosition] = (byte)(value >> 8);       //high byte is first!
            return true;
        }

        /// <summary>
        /// Get a signed 2-byte short integer
        /// </summary>
        /// <param name="data"></param>
        /// <param name="startPosition"></param>
        /// <returns></returns>
        public static int BigEndianGetShort(byte[] data, int startPosition)
        {
            // Do we have room in the buffer?
            if (data == null)
                throw new ArgumentNullException("data");

            if (startPosition > data.Length - 2)
                throw new ArgumentOutOfRangeException("startPosition");

            int uValue = (((int)data[startPosition]) * 0x100) + data[startPosition + 1]; //MSB is first
            if (uValue >= 32768)
                uValue -= 65536;
            return uValue;
        }


        /// <summary>
        /// get an unsigned Big-Endian 2-byte short integer
        /// <remarks>HighByte/LowByte order</remarks>
        /// </summary>
        /// <param name="data"></param>
        /// <param name="startPosition"></param>
        /// <returns></returns>
        public static int BigEndianGetUShort(byte[] data, int startPosition)
        {
            // Do we have room in the buffer?
            if (data == null)
                throw new ArgumentNullException("data");

            if (startPosition > data.Length - 2)
                throw new ArgumentOutOfRangeException("startPosition");

            //high byte is first!
            return ((data[startPosition] * 0x100) + data[startPosition + 1]);
        }

        /// <summary>
        /// Set an unsigned 2-byte Big-Endian unsigned short integer
        /// </summary>
        /// <param name="data"></param>
        /// <param name="startPosition"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool BigEndianSetUShort(byte[] data, int startPosition, int value)
        {
            return BigEndianSetShort(data, startPosition, value);
        }

        /// <summary>
        /// Copy a portion of an array to another array
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="source"></param>
        /// <param name="sourceStart"></param>
        /// <param name="destinationStart"></param>
        /// <param name="length"></param>
        public static void CopyTo(byte[] destination, byte[] source, int sourceStart, int destinationStart, int length)
        {
            if (source == null)
                return;

            const string errPrefix = "Error in ByteArray.CopyTo(): ";
            if (sourceStart < 0)
                throw new ArgumentOutOfRangeException(errPrefix + "sourceStart must be >= 0");

            if (destinationStart < 0)
                throw new ArgumentOutOfRangeException(errPrefix + "destinationStart must be >= 0");

            if (source.Length < (sourceStart + length))
                throw new ArgumentOutOfRangeException(errPrefix + "Source array is too short");

            if (destination == null)
                destination = new byte[destinationStart + length];

            if (destination.Length < (destinationStart + length))
                throw new ArgumentOutOfRangeException(errPrefix + "Destination array is too short");

            System.Buffer.BlockCopy(source, sourceStart, destination, destinationStart, length);
        }

        /// <summary>
        /// Combine two byte arrays
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public static byte[] Combine(byte[] first, byte[] second)
        {
            if ((first == null || first.Length == 0) && (second == null || second.Length == 0))
                return null;
            if (first == null || first.Length == 0)
                return second;
            if (second == null || second.Length == 0)
                return first;

            byte[] combined = new byte[first.Length + second.Length];
            CopyTo(combined, first, 0, 0, first.Length);
            CopyTo(combined, second, 0, first.Length, second.Length);
            return combined;
        }

        /// <summary>
        /// Return a sub-array
        /// </summary>
        /// <param name="data"></param>
        /// <param name="start"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static byte[] SubArray(byte[] data, int start, int length)
        {
            byte[] sub = new byte[length];
            if (data != null)
                System.Buffer.BlockCopy(data, start, sub, 0, Math.Min(length, data.Length));
            return sub;
        }
        #endregion
    }
}
