//------------------------------------------------------------------------------
//  <copyright file="GpioPinArrayTypes.cs" company="Microsoft Corporation">
//      Copyright (C) Microsoft Corporation.  All rights reserved.
//  </copyright>
//------------------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//------------------------------------------------------------------------------

namespace Microsoft.Robotics.Services.GpioPinArray
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
    [DisplayName("(User) General Purpose IO Pin Array")]
    [Description("Provides access to an array of GPIO pins.")]
    [DssServiceDescription("http://msdn.microsoft.com/robotics")] 
    public static class Contract
    {
        /// <summary>
        /// The Unique Contract Identifier
        /// </summary>
        public const string Identifier = "http://schemas.microsoft.com/robotics/2011/08/gpiopinarray.html";
    }

    /// <summary>
    /// GPIO Pin Array Operations Port
    /// </summary>
    [ServicePort]
    public class GpioPinArrayOperations : PortSet<DsspDefaultLookup,
                                                  DsspDefaultDrop,
                                                  Get,
                                                  HttpGet,
                                                  Replace,
                                                  ReliableSubscribe,
                                                  Subscribe,
                                                  SetPin>
    {
    }

    /// <summary>
    /// Get Operation
    /// </summary>
    [Description("Gets a GPIO Pin array's current state.\n This includes the pin state for each of the pins in the array.")]
    public class Get : Get<GetRequestType, PortSet<GpioPinArrayState, Fault>> 
    { 
    }

    /// <summary>
    /// Replace Operation
    /// </summary>
    [Description("Changes (or indicates a change to) a GPIO Pin array's entire state.")]
    [DisplayName("(User) GpioPinArrayReplace")]
    public class Replace : Replace<GpioPinArrayState, PortSet<DefaultReplaceResponseType, Fault>> 
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
    public class ReliableSubscribe : Subscribe<ReliableSubscribeRequestType, DsspResponsePort<SubscribeResponseType>, GpioPinArrayOperations>
    { 
    }

    /// <summary>
    /// SetPin Operation, used for modifying the state of a single pin
    /// </summary>
    public class SetPin : Update<SetPinRequestType, PortSet<DefaultUpdateResponseType, Fault>>
    {
    }

    /// <summary>
    /// Change a specific pin state using it's name and/or number
    /// </summary>
    [DataContract]
    [DataMemberConstructor]
    public class SetPinRequestType
    {
        /// <summary>
        /// The individual pin state
        /// </summary>
        [DataMember]
        public GpioPinState PinState;
    }
}
