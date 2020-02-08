//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: BatteryTypes.cs $ $Revision: 19 $
//-----------------------------------------------------------------------

using Microsoft.Ccr.Core;
using Microsoft.Dss.Core.Attributes;
using Microsoft.Dss.Core.DsspHttp;
using Microsoft.Dss.ServiceModel.Dssp;

using System;
using System.Collections.Generic;
using System.ComponentModel;

using W3C.Soap;

namespace Microsoft.Robotics.Services.Battery
{

    /// <summary>
    /// Battery Contract
    /// </summary>
    [DisplayName("(User) Generic Battery")]
    [Description("Provides access to a battery.")]
    [DssServiceDescription("http://msdn.microsoft.com/library/dd145247.aspx")]
    public static class Contract
    {
        /// The Unique Contract Identifier for the Battery service
        public const string Identifier = "http://schemas.microsoft.com/2006/06/battery.html";
    }

    /// <summary>
    /// Battery Operations Port
    /// </summary>
    [ServicePort]
    public class BatteryOperations : PortSet<
        DsspDefaultLookup,
        DsspDefaultDrop,
        Get,
        HttpGet,
        Replace,
        Subscribe,
        SetCriticalLevel>
    {
    }

    /// <summary>
    /// Get Operation
    /// </summary>
    [Description("Get the battery's current state.")]
    public class Get : Get<GetRequestType, PortSet<BatteryState, Fault>> { }

    /// <summary>
    /// Replace Operation
    /// </summary>
    [Description("Indicates an update of the battery's state, \nor sets the entire battery state.")]
    [DisplayName("(User) BatteryUpdate")]
    public class Replace : Replace<BatteryState, PortSet<DefaultReplaceResponseType, Fault>> { }

    /// <summary>
    /// SetCriticalLevel Operation
    /// </summary>
    [Description("indicates the battery has fallen below the critical level, \nor sets the battery's critical level.")]
    [DisplayName("(User) CriticalLevelUpdate")]
    public class SetCriticalLevel : Update<UpdateCriticalBattery, PortSet<DefaultUpdateResponseType, Fault>> { }

    /// <summary>
    /// Subscribe Operation
    /// </summary>
    [Description("Subscribe to BatteryUpdate and CriticalLevelUpdate notifications")]
    public class Subscribe : Subscribe<SubscribeRequestType, PortSet<SubscribeResponseType, Fault>> { }
}
