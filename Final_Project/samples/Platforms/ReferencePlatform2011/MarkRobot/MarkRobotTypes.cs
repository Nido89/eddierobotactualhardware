//------------------------------------------------------------------------------
//  <copyright file="MarkRobotTypes.cs" company="Microsoft Corporation">
//      Copyright (C) Microsoft Corporation.  All rights reserved.
//  </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Robotics.ReferencePlatform.MarkRobot
{
    using System;
    using System.Collections.Generic;
    
    using Microsoft.Ccr.Core;
    using Microsoft.Dss.Core.Attributes;
    using Microsoft.Dss.Core.DsspHttp;
    using Microsoft.Dss.ServiceModel.Dssp;
    using W3C.Soap;

    using battery = Microsoft.Robotics.Services.Battery;
    using drive = Microsoft.Robotics.Services.Drive;
    using ir = Microsoft.Robotics.Services.InfraredSensorArray;
    using sonar = Microsoft.Robotics.Services.SonarSensorArray;

    /// <summary>
    /// MarkRobot contract class
    /// </summary>
    public sealed class Contract
    {
        /// <summary>
        /// DSS contract identifer for MarkRobot
        /// </summary>
        [DataMember]
        public const string Identifier = "http://schemas.microsoft.com/2011/11/markrobot.user.html";
    }

    /// <summary>
    /// MarkRobot state
    /// </summary>
    [DataContract]
    public class MarkRobotState
    {
        /// <summary>
        /// Timestamp information for the service state
        /// </summary>
        [DataMember]
        public DateTime LastStartTime = DateTime.Now;

        /// <summary>
        /// Time in MS between retrieving sensor values
        /// </summary>
        [DataMember]
        public int SensorPollingInterval = 100;

        /// <summary>
        /// ADC pin containing the battery voltage
        /// </summary>
        [DataMember]
        public int BatteryVoltagePinIndex = 7;

        /// <summary>
        /// ADC pin containing the battery voltage
        /// </summary>
        [DataMember]
        public double BatteryVoltageDivider = 3.21;

        /// <summary>
        /// Part of raw to normalized conversion formula
        /// </summary>
        [DataMember]
        public double InfraredRawValueDivisorScalar = 22;

        /// <summary>
        /// Ration for IR sensor voltage function
        /// </summary>
        [DataMember]
        public double InfraredDistanceExponent = -1.20;

        /// <summary>
        /// Conversion of echo time to centimeters
        /// </summary>
        [DataMember]
        public double SonarTimeValueMultiplier = 0.1088928;

        /// <summary>
        /// Alternate contract state data for drive
        /// </summary>
        [DataMember]
        public drive.DriveDifferentialTwoWheelState DriveState = new drive.DriveDifferentialTwoWheelState();

        /// <summary>
        /// Alternate contract state data for IR sensors
        /// </summary>
        [DataMember]
        public ir.InfraredSensorArrayState InfraredSensorState = new ir.InfraredSensorArrayState();

        /// <summary>
        /// Alternate contract state data for Sonar sensors
        /// </summary>
        [DataMember]
        public sonar.SonarSensorArrayState SonarSensorState = new sonar.SonarSensorArrayState();

        /// <summary>
        /// Alternate contract state data for Battery
        /// </summary>
        [DataMember]
        public battery.BatteryState BatteryState = new battery.BatteryState();
    }

    /// <summary>
    /// MarkRobot main operations port
    /// </summary>
    [ServicePort]
    public class MarkRobotOperations : PortSet<DsspDefaultLookup, DsspDefaultDrop, Get, HttpGet, Subscribe>
    {
    }

    /// <summary>
    /// MarkRobotState get operation
    /// Boilerplate interface definition, no code required
    /// </summary>
    public class Get : Get<GetRequestType, PortSet<MarkRobotState, Fault>>
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
        public Get(GetRequestType body) : base(body) 
        {
        }

        /// <summary>
        /// Creates a new instance of Get
        /// </summary>
        /// <param name="body">The request message body</param>
        /// <param name="responsePort">The response port for the request</param>
        public Get(GetRequestType body, PortSet<MarkRobotState, Fault> responsePort)
            : base(body, responsePort) 
        {
        }
    }

    /// <summary>
    /// MarkRobotState Replace operation
    /// </summary>
    public class Replace : Replace<MarkRobotState, PortSet<DefaultReplaceResponseType, Fault>>
    {
        /// <summary>
        /// Default no-param ctor
        /// </summary>
        public Replace() 
        {
        }

        /// <summary>
        /// Service State-based ctor
        /// </summary>
        /// <param name="state">Service State</param>
        public Replace(MarkRobotState state)
            : base(state) 
        {
        }

        /// <summary>
        /// State and Port ctor
        /// </summary>
        /// <param name="state">Service State</param>
        /// <param name="responsePort">Response Port</param>
        public Replace(MarkRobotState state, PortSet<DefaultReplaceResponseType, Fault> responsePort)
            : base(state, responsePort)
        { 
        }
    }
    
    /// <summary>
    /// MarkRobot subscribe operation
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
        public Subscribe(SubscribeRequestType body) : base(body)
        {
        }

        /// <summary>
        /// Creates a new instance of Subscribe
        /// </summary>
        /// <param name="body">The request message body</param>
        /// <param name="responsePort">The response port for the request</param>
        public Subscribe(SubscribeRequestType body, PortSet<SubscribeResponseType, Fault> responsePort) : base(body, responsePort)
        {
        }
    }
}
