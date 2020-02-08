//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: BumperTypes.cs $ $Revision: 5 $
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.Dss.Core.Attributes;



namespace Microsoft.Robotics.Services.IRobot.Roomba.Bumper
{
    /// <summary>
    /// Bumper Contract
    /// </summary>
    public static class Contract
    {
        /// <summary>
        /// The Unique Contract Identifier for the Bumper service
        /// </summary>
        public const String Identifier = "http://schemas.microsoft.com/robotics/2006/12/irobot/roomba/bumper.user.html";
    }

    /// <summary>
    /// iRobot Sensors
    /// </summary>
    [DataContract]
    public enum RoombaBumpers
    {
        /// <summary>
        /// Left Bumper Contact Sensor
        /// </summary>
        LeftBumper = 1,

        /// <summary>
        /// Right Bumper Contact Sensor
        /// </summary>
        RightBumper = 2,
        
        /// <summary>
        /// Wall Sensor
        /// </summary>
        Wall = 3,
        
        /// <summary>
        /// Virtual Wall Sensor
        /// </summary>
        VirtualWall = 4,
        
        /// <summary>
        /// Left Cliff Sensor
        /// </summary>
        CliffLeft = 5,
        
        /// <summary>
        /// Front Left Cliff Sensor
        /// </summary>
        CliffFrontLeft = 6,
        
        /// <summary>
        /// Right Front Cliff Sensor
        /// </summary>
        CliffFrontRight = 7,
        
        /// <summary>
        /// Right Cliff Sensor
        /// </summary>
        CliffRight = 8,
        
        /// <summary>
        /// Left Wheel Drop Sensor
        /// </summary>
        WheelDropLeft = 9,
        
        /// <summary>
        /// Right Wheel Drop Sensor
        /// </summary>
        WheelDropRight = 10,
        
        /// <summary>
        /// Rear Wheel Drop Sensor
        /// </summary>
        WheelDropRear = 11,
    }
}
