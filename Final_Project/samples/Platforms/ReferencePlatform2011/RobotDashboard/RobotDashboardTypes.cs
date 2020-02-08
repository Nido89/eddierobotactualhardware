//-----------------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples
//
//  <copyright file="RobotDashboardTypes.cs" company="Microsoft Corporation">
//      Copyright (C) Microsoft Corporation.  All rights reserved.
//  </copyright>
//
//  $File: RobotDashboardTypes.cs $ $Revision: 1 $
//-----------------------------------------------------------------------------

namespace Microsoft.Robotics.Services.RobotDashboard
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using Microsoft.Ccr.Core;
    using Microsoft.Dss.Core.Attributes;
    using Microsoft.Dss.ServiceModel.Dssp;
    using Microsoft.Dss.ServiceModel.DsspServiceBase;
    using W3C.Soap;

    /// <summary>
    /// The number of Proximity Sensors
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// The number of proximity sensors
        /// </summary>
        public const int IRSensorCount = 3;

        /// <summary>
        /// The number of sonar sensors
        /// </summary>
        public const int SonarSensorCount = 2;
    }

    /// <summary>
    /// RobotDashboard contract class
    /// </summary>
    public sealed class Contract
    {
        /// <summary>
        /// DSS contract identifer for RobotDashboard
        /// </summary>
        [DataMember]
        public const string Identifier = "http://schemas.microsoft.com/robotics/2011/07/robotdashboard.user.html";
    }

    /// <summary>
    /// Names of the Proximity Sensors
    /// </summary>
    public enum IrSensorNames
    {
        /// <summary>
        /// Left IR Sensor
        /// </summary>
        IrLeft = 0,

        /// <summary>
        /// Center IR Sensor
        /// </summary>
        IrCenter = 1,

        /// <summary>
        /// Right IR Sensor
        /// </summary>
        IrRight = 2
    }

    /// <summary>
    /// Names of the Sensor Sensors
    /// </summary>
    public enum SonarSensorNames
    {
        /// <summary>
        /// Left Sonar Sensor
        /// </summary>
        SonarLeft = 0,

        /// <summary>
        /// Right Sonar Sensor
        /// </summary>
        SonarRight = 1,
    }

    /// <summary>
    /// RobotDashboard state
    /// </summary>
    [DataContract]
    public class RobotDashboardState
    {
        /// <summary>
        /// The current Tilt Angle in degrees
        /// </summary>
        [DataMember]
        public double TiltAngle;

        /// <summary>
        /// Option settings from the GUI
        /// </summary>
        [DataMember]
        public GUIOptions Options;
    }

    /// <summary>
    /// GUIOptions -- Properties "bag" for option settings
    /// </summary>
    [DataContract]
    [DisplayName("(User) GUI Option Settings")]
    public class GUIOptions
    {
        /// <summary>
        /// Initial X position of the Window in screen coords
        /// </summary>
        [DataMember]
        public int WindowStartX;

        /// <summary>
        /// Initial Y position of the Window in screen coords
        /// </summary>
        [DataMember]
        public int WindowStartY;

        /// <summary>
        /// Initial X position of the Depthcam Window in screen coords
        /// </summary>
        [DataMember]
        public int DepthcamWindowStartX;

        /// <summary>
        /// Initial Y position of the Depthcam Window in screen coords
        /// </summary>
        [DataMember]
        public int DepthcamWindowStartY;

        /// <summary>
        /// Width of the Depthcam Window in screen coords
        /// </summary>
        [DataMember]
        public int DepthcamWindowWidth;

        /// <summary>
        /// Height of the Depthcam Window in screen coords
        /// </summary>
        [DataMember]
        public int DepthcamWindowHeight;

        /// <summary>
        /// Initial X position of the Webcam Window in screen coords
        /// </summary>
        [DataMember]
        public int WebcamWindowStartX;

        /// <summary>
        /// Initial Y position of the Webcam Window in screen coords
        /// </summary>
        [DataMember]
        public int WebcamWindowStartY;

        /// <summary>
        /// Width of the Webcam Window in screen coords
        /// </summary>
        [DataMember]
        public int WebcamWindowWidth;

        /// <summary>
        /// Height of the Webcam Window in screen coords
        /// </summary>
        [DataMember]
        public int WebcamWindowHeight;

        // Dead Zone parameters
        // The "deadzone" is a region where the movement of the
        // joystick has no effect. The implementation here snaps
        // the x or y coordinate to zero when it is within the
        // DeadZoneX or Y range. This allows the robot to drive
        // dead ahead, or to rotate perfectly. The old code did
        // not have an exact center and so the wheel power could
        // never be balanced!
        // This amounts to only a few pixels on the screen (because
        // the "yoke" range is +/- 1000). If you set it too high,
        // you won't get any movement! However, you can set it to
        // zero if you don't want this feature.

        /// <summary>
        /// X range at center of joystick that is "dead"
        /// </summary>
        [DataMember]
        public double DeadZoneX;

        /// <summary>
        /// Y range at center of joystick that is "dead"
        /// </summary>
        [DataMember]
        public double DeadZoneY;

        // Scale Factors
        // Because different robots have different drive
        // characteristics, and their users have different reaction
        // times and/or preferences, there are now two parameters
        // that affect the scaling of the drive power. The Translate
        // Scale Factor adjusts the forward/backward take-up rate,
        // i.e. moving the joystick forwards or backwards (which is
        // up or down on the "yoke" on the screen).
        // The Rotate Scale Factor adjusts the rate for the the
        // left/right (side to side) movements to control the speed
        // of rotation.
        // Usually you want different scale factors for these two
        // types of motions because it is hard to control turns if
        // the robot spins at the same speed that it uses to drives
        // forwards.
        // For the Reference Platform the recommended values are:
        // 0.7 for Translate and
        // 0.4 for Rotate.
        //
        // NOTE: The results of the motor power calculations are
        // limited to +/- 1000, which translates to +/- 1.0 when
        // sent to the differentialdrive service. This means that
        // if you set Translate to 2.0 for instance, then it will
        // max out halfway between the center and the maximum
        // (top) of the joystick travel. This makes the speed much
        // more responsive to joystick movements. Conversely, you
        // can set a value of 0.5 and then the robot will only ever
        // reach half of its possible drive speed (approximately).

        /// <summary>
        /// Adjusts the sensitivity for driving forwards/backwards
        /// </summary>
        [DataMember]
        public double TranslateScaleFactor;

        /// <summary>
        /// Adjusts the sensitivity for rotating
        /// </summary>
        [DataMember]
        public double RotateScaleFactor;

        /// <summary>
        /// Update interval for simulated camera -- OBSOLETE!
        /// </summary>
        [DataMember]
        public int CameraInterval;
    }

    /// <summary>
    /// RobotDashboard main operations port
    /// </summary>
    [ServicePort]
    public class RobotDashboardOperations : PortSet<DsspDefaultLookup, DsspDefaultDrop, Get, Subscribe>
    {
    }

    /// <summary>
    /// RobotDashboard get operation
    /// </summary>
    public class Get : Get<GetRequestType, PortSet<RobotDashboardState, Fault>>
    {
        /// <summary>
        /// Creates a new instance of Get
        /// </summary>
        public Get()
        {
        }

        /// <summary>
        /// Creates a new instance of Get
        /// </summary>
        /// <param name="body">The request message body</param>
        public Get(GetRequestType body)
            : base(body)
        {
        }

        /// <summary>
        /// Creates a new instance of Get
        /// </summary>
        /// <param name="body">The request message body</param>
        /// <param name="responsePort">The response port for the request</param>
        public Get(GetRequestType body, PortSet<RobotDashboardState, Fault> responsePort)
            : base(body, responsePort)
        {
        }
    }

    /// <summary>
    /// RobotDashboard subscribe operation
    /// </summary>
    public class Subscribe : Subscribe<SubscribeRequestType, PortSet<SubscribeResponseType, Fault>>
    {
        /// <summary>
        /// Creates a new instance of Subscribe
        /// </summary>
        public Subscribe()
        {
        }

        /// <summary>
        /// Creates a new instance of Subscribe
        /// </summary>
        /// <param name="body">The request message body</param>
        public Subscribe(SubscribeRequestType body)
            : base(body)
        {
        }

        /// <summary>
        /// Creates a new instance of Subscribe
        /// </summary>
        /// <param name="body">The request message body</param>
        /// <param name="responsePort">The response port for the request</param>
        public Subscribe(SubscribeRequestType body, PortSet<SubscribeResponseType, Fault> responsePort)
            : base(body, responsePort)
        {
        }
    }
}

