//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: SimulatedGPSTypes.cs $ $Revision: 3 $
//-----------------------------------------------------------------------
using Microsoft.Dss.Core.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Dss.ServiceModel.Dssp;
using Microsoft.Ccr.Core;
using W3C.Soap;

namespace Microsoft.Robotics.Services.Simulation.Sensors.GPSSensor
{
    /// <summary>
    /// SimulatedGPSSensor Contract
    /// </summary>
    public static class Contract
    {
        /// The Unique Contract Identifier for the SimulatedPhotoCell service
        public const string Identifier = "http://schemas.microsoft.com/2008/11/simgpssensor.user.html";
    }

    /// <summary>
    /// GPSSensor state
    /// </summary>
    [DataContract]
    public class GPSSensorState
    {
        /// <summary>
        /// X component
        /// </summary>
        [DataMember]
        [Browsable(false)]
        public double X { get; set; }

        /// <summary>
        /// Y component
        /// </summary>
        [DataMember]
        [Browsable(false)]
        public double Y { get; set; }

        /// <summary>
        /// Z component
        /// </summary>
        [DataMember]
        [Browsable(false)]
        public double Z { get; set; }

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
    /// GPSSensor main operations port
    /// </summary>
    [ServicePort]
    public class GPSSensorOperations : PortSet<DsspDefaultLookup, DsspDefaultDrop, Get, Subscribe, Replace, ReliableSubscribe>
    {
    }

    /// <summary>
    /// GPSSensor get operation
    /// </summary>
    public class Get : Get<GetRequestType, PortSet<GPSSensorState, Fault>>
    {
    }

    /// <summary>
    /// GPSSensor replace operation
    /// </summary>
    public class Replace : Replace<GPSSensorState, PortSet<DefaultReplaceResponseType, Fault>>
    {
    }

    /// <summary>
    /// GPSSensor subscribe operation
    /// </summary>
    public class Subscribe : Subscribe<SubscribeRequestType, PortSet<SubscribeResponseType, Fault>> { }

    /// <summary>
    /// GPSSensor reliable subscribe operation
    /// </summary>
    public class ReliableSubscribe : Subscribe<ReliableSubscribeRequestType, DsspResponsePort<SubscribeResponseType>, GPSSensorOperations> { }
}
