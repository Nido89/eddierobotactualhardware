//------------------------------------------------------------------------------
//  <copyright file="ADCPinArrayTypes.cs" company="Microsoft Corporation">
//      Copyright (C) Microsoft Corporation.  All rights reserved.
//  </copyright>
//------------------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//------------------------------------------------------------------------------

namespace Microsoft.Robotics.Services.ADCPinArray
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
    [DisplayName("(User) Analog-Digital Pin Array")]
    [Description("Provides access to an array of ADC pins.")]
    [DssServiceDescription("http://msdn.microsoft.com/robotics")] 
    public static class Contract
    {
        /// <summary>
        /// The Unique Contract Identifier
        /// </summary>
        public const string Identifier = "http://schemas.microsoft.com/robotics/2011/10/adcpinarray.html";
    }

    /// <summary>
    /// ADC Pin Array Operations Port
    /// </summary>
    [ServicePort]
    public class ADCPinArrayOperations : PortSet<DsspDefaultLookup,
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
    [Description("Gets a ADC Pin array's current state.\n This includes the pin state for each of the pins in the array.")]
    public class Get : Get<GetRequestType, PortSet<ADCPinArrayState, Fault>> 
    { 
    }

    /// <summary>
    /// Replace Operation
    /// </summary>
    [Description("Changes (or indicates a change to) an ADC Pin array's entire state.")]
    [DisplayName("(User) ADCPinArrayReplace")]
    public class Replace : Replace<ADCPinArrayState, PortSet<DefaultReplaceResponseType, Fault>> 
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
    public class ReliableSubscribe : Subscribe<ReliableSubscribeRequestType, DsspResponsePort<SubscribeResponseType>, ADCPinArrayOperations>
    { 
    }
}
