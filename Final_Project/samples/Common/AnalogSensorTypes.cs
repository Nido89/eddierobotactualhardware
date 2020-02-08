//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: AnalogSensorTypes.cs $ $Revision: 18 $
//-----------------------------------------------------------------------

using Microsoft.Ccr.Core;
using Microsoft.Dss.Core.Attributes;
using Microsoft.Dss.Core.DsspHttp;
using Microsoft.Dss.ServiceModel.Dssp;

using System;
using System.Collections.Generic;
using System.ComponentModel;

using W3C.Soap;

namespace Microsoft.Robotics.Services.AnalogSensor
{
    /// <summary>
    /// Analog Sensor Contract
    /// </summary>
    [DisplayName("(User) Generic Analog Sensor")]
    [Description("Provides access to an analog sensor.")]
    [DssServiceDescription("http://msdn.microsoft.com/library/dd145244.aspx")]
    public static class Contract
    {
        /// The Unique Contract Identifier
        public const string Identifier = "http://schemas.microsoft.com/robotics/2006/06/analogsensor.html";
    }

    /// <summary>
    /// Analog Sensor Operations Port
    /// </summary>
    [ServicePort]
    public class AnalogSensorOperations : PortSet
    {
        /// <summary>
        /// Analog Sensor Operations Port
        /// </summary>
        public AnalogSensorOperations()
            : base(
            typeof(DsspDefaultLookup),
            typeof(DsspDefaultDrop),
            typeof(Get),
            typeof(HttpGet),
            typeof(Replace),
            typeof(ReliableSubscribe),
            typeof(Subscribe))
        {
        }
    }

    /// <summary>
    /// Get Operation
    /// </summary>
    [Description("Gets an analog sensor's current state.")]
    public class Get : Get<GetRequestType, PortSet<AnalogSensorState, Fault>> { }
    /// <summary>
    /// Replace Operation
    /// </summary>
    [Description("Indicates a change to an analog sensor's state.")]
    [DisplayName("(User) AnalogSensorUpdate")]
    public class Replace : Replace<AnalogSensorState, PortSet<DefaultReplaceResponseType, Fault>> { }

    /// <summary>
    /// Subscribe Operation
    /// </summary>
    public class Subscribe : Subscribe<SubscribeRequestType, PortSet<SubscribeResponseType, Fault>> { }
    /// <summary>
    /// ReliableSubscribe Operation
    /// </summary>
    public class ReliableSubscribe : Subscribe<ReliableSubscribeRequestType, DsspResponsePort<SubscribeResponseType>, AnalogSensorOperations> { }
}
