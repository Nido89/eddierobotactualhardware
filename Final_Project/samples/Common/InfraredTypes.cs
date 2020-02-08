//------------------------------------------------------------------------------
//  <copyright file="InfraredTypes.cs" company="Microsoft Corporation">
//      Copyright (C) Microsoft Corporation.  All rights reserved.
//  </copyright>
//------------------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//------------------------------------------------------------------------------

using Microsoft.Ccr.Core;
using Microsoft.Dss.Core.Attributes;
using Microsoft.Dss.Core.DsspHttp;
using Microsoft.Dss.ServiceModel.Dssp;

using System;
using System.Net;
using System.Collections.Generic;
using System.ComponentModel;

using W3C.Soap;

namespace Microsoft.Robotics.Services.Infrared
{
    /// <summary>
    /// Infrared Contract
    /// </summary>
    [DisplayName("(User) Generic Infrared Distance Sensor")]
    [Description("Provides access to an IR sensor.")]
    [DssServiceDescription("http://msdn.microsoft.com/robotics")]
    public static class Contract
    {
        /// The Unique Contract Identifier for the IR service
        public const string Identifier = "http://schemas.microsoft.com/robotics/2011/10/infrared.html";
    }

    /// <summary>
    /// Infrared Service Operations Port
    /// </summary>
    [ServicePort]
    public class InfraredOperations : PortSet<
        DsspDefaultLookup,
        DsspDefaultDrop,
        Get,
        HttpGet,
        Replace,
        ReliableSubscribe,
        Subscribe>
    {
    }

    /// <summary>
    /// Infrared Service Get Operation
    /// </summary>
    [Description("Gets the IR sensor current state.")]
    public class Get : Get<GetRequestType, PortSet<InfraredState, Fault>> 
    { 
    }
    
    /// <summary>
    /// Infrared Service Replace Operation
    /// </summary>
    [Description("Indicates a change to the IR sensor readings.")]
    [DisplayName("(User) InfraredUpdate")]
    public class Replace : Replace<InfraredState, PortSet<DefaultReplaceResponseType, Fault>> 
    { 
    }

    /// <summary>
    /// Infrared Service Subscribe Operation
    /// </summary>
    [Description("Subscribe to IR sensor update readings.")]
    public class Subscribe : Subscribe<SubscribeRequestType, PortSet<SubscribeResponseType, Fault>> 
    { 
    }
    
    /// <summary>
    /// Infrared Service ReliableSubscribe Operation
    /// </summary>
    [Description("Subscribe to IR sensor update readings.")]
    public class ReliableSubscribe : Subscribe<ReliableSubscribeRequestType, DsspResponsePort<SubscribeResponseType>, InfraredOperations> 
    { 
    }
}
