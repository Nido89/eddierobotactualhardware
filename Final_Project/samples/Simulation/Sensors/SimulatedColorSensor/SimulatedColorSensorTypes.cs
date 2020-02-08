//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: SimulatedColorSensorTypes.cs $ $Revision: 4 $
//-----------------------------------------------------------------------
using Microsoft.Dss.Core.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Dss.ServiceModel.Dssp;
using Microsoft.Ccr.Core;
using W3C.Soap;

namespace Microsoft.Robotics.Services.Simulation.Sensors.ColorSensor
{
    /// <summary>
    /// SimulatedColorSensor Contract
    /// </summary>
    public static class Contract
    {
        /// The Unique Contract Identifier for the SimulatedPhotoCell service
        public const string Identifier = "http://schemas.microsoft.com/2008/11/simcolorsensor.user.html";
    }

    /// <summary>
    /// ColorSensor state
    /// </summary>
    [DataContract]
    public class ColorSensorState
    {
        /// <summary>
        /// Average red value of the color sensor on a [0,1] scale
        /// </summary>
        [DataMember]
        [Browsable(false)]
        public double NormalizedAverageRed { get; set; }

        /// <summary>
        /// Average green value of the color sensor on a [0,1] scale
        /// </summary>
        [DataMember]
        [Browsable(false)]
        public double NormalizedAverageGreen { get; set; }

        /// <summary>
        /// Average blue value of the color sensor on a [0,1] scale
        /// </summary>
        [DataMember]
        [Browsable(false)]
        public double NormalizedAverageBlue { get; set; }

        /// <summary>
        /// Timestamp of this sample
        /// </summary>
        [DataMember(Order = -1, XmlOmitDefaultValue = true)]
        [Browsable(false)]
        [Description("Indicates the timestamp of the sensor reading.")]
        [DefaultValue(typeof(DateTime), "0001-01-01T00:00:00")]
        public DateTime TimeStamp { get; set; }
    }

    /// <summary>
    /// ColorSensor main operations port
    /// </summary>
    [ServicePort]
    public class ColorSensorOperations : PortSet<DsspDefaultLookup, DsspDefaultDrop, Get, Subscribe, Replace, ReliableSubscribe>
    {
    }

    /// <summary>
    /// ColorSensor get operation
    /// </summary>
    public class Get : Get<GetRequestType, PortSet<ColorSensorState, Fault>>
    {
    }

    /// <summary>
    /// ColorSensor replace operation
    /// </summary>
    public class Replace : Replace<ColorSensorState, PortSet<DefaultReplaceResponseType, Fault>>
    {
    }

    /// <summary>
    /// ColorSensor subscribe operation
    /// </summary>
    public class Subscribe : Subscribe<SubscribeRequestType, PortSet<SubscribeResponseType, Fault>> { }

    /// <summary>
    /// ColorSensor reliable subscribe operation
    /// </summary>
    public class ReliableSubscribe : Subscribe<ReliableSubscribeRequestType, DsspResponsePort<SubscribeResponseType>, ColorSensorOperations> { }
}
