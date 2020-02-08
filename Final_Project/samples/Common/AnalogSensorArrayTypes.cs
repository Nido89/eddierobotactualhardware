//------------------------------------------------------------------------------
//  <copyright file="AnalogSensorArrayTypes.cs" company="Microsoft Corporation">
//      Copyright (C) Microsoft Corporation.  All rights reserved.
//  </copyright>
//------------------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//------------------------------------------------------------------------------

namespace Microsoft.Robotics.Services.AnalogSensorArray
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;

    using Microsoft.Ccr.Core;
    using Microsoft.Dss.Core.Attributes;
    using Microsoft.Dss.Core.DsspHttp;
    using Microsoft.Dss.ServiceModel.Dssp;
    using Microsoft.Robotics.PhysicalModel;
    
    using W3C.Soap;

    /// <summary>
    /// Analog Sensor Contract
    /// </summary>
    [DisplayName("(User) Generic Analog Sensor Array")]
    [Description("Provides access to an analog sensor array.")]
    [DssServiceDescription("http://msdn.microsoft.com/library/dd145245.aspx")]
    public static class Contract
    {
        /// The Unique Contract Identifier
        public const string Identifier = "http://schemas.microsoft.com/robotics/2006/06/analogsensorarray.html";
    }

    /// <summary>
    /// Analog Sensor Operations Port
    /// </summary>
    [ServicePort]
    public class AnalogSensorOperations : PortSet<DsspDefaultLookup,
                                                  DsspDefaultDrop,
                                                  Get,
                                                  HttpGet,
                                                  Replace,
                                                  ReliableSubscribe,
                                                  Subscribe>
    {
    }

    /// <summary>
    /// Get Operation
    /// </summary>
    [Description("Gets an analog sensor array's current state.\n This includes the analog sensor state for each of the sensors in the array.")]
    public class Get : Get<GetRequestType, PortSet<AnalogSensorArrayState, Fault>> 
    { 
    }
    
    /// <summary>
    /// Replace Operation
    /// </summary>
    [Description("Changes (or indicates a change to) an analog sensor array's entire state.")]
    [DisplayName("(User) AnalogSensorsReplace")]
    public class Replace : Replace<AnalogSensorArrayState, PortSet<DefaultReplaceResponseType, Fault>>
    { 
    }
    
    /// <summary>
    /// Subscribe Operation
    /// </summary>
    public class Subscribe : Subscribe<SubscribeRequestType, PortSet<SubscribeResponseType, Fault>>
    { 
    }
    
    /// <summary>
    /// ReliableSubscribe Operation
    /// </summary>
    public class ReliableSubscribe : Subscribe<ReliableSubscribeRequestType, DsspResponsePort<SubscribeResponseType>, AnalogSensorOperations>
    { 
    }
}
