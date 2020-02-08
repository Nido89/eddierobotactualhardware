//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: ContactSensorTypes.cs $ $Revision: 26 $
//-----------------------------------------------------------------------

using Microsoft.Ccr.Core;
using Microsoft.Dss.Core.Attributes;
using Microsoft.Dss.Core.DsspHttp;
using Microsoft.Dss.ServiceModel.Dssp;

using System;
using System.Net;
using System.Collections.Generic;
using System.ComponentModel;

using W3C.Soap;

namespace Microsoft.Robotics.Services.ContactSensor
{
    /// <summary>
    /// The Dss Contract Definition
    /// </summary>
    [DisplayName("(User) Generic Contact Sensors")]
    [Description("Provides access to contact sensors.")]
    [DssServiceDescription("http://msdn.microsoft.com/library/dd145249.aspx")]
    public static class Contract
    {
        /// <summary>
        /// Contract Identifier
        /// </summary>
        public const string Identifier = "http://schemas.microsoft.com/2006/06/contactsensor.html";
    }

    /// <summary>
    /// ContactSensorArray Port
    /// </summary>
    [ServicePort]
    public class ContactSensorArrayOperations : PortSet<
        DsspDefaultLookup,
        DsspDefaultDrop,
        Get,
        HttpGet,
        Replace,
        Update,
        ReliableSubscribe,
        Subscribe>
    {
    }

    /// <summary>
    /// Operation Get: Gets the state
    /// </summary>
    [Description("Gets a set of contact sensors' current state.")]
    public class Get : Get<GetRequestType, PortSet<ContactSensorArrayState, Fault>>
    {
    }

    /// <summary>
    /// Operation Replace: Configures bumper collection
    /// </summary>
    [Description("Configures (or indicates a change to) the state of a set (array) of contact sensors.")]
    [DisplayName("(User) ContactSensorsReplace")]
    public class Replace : Replace<ContactSensorArrayState, PortSet<DefaultReplaceResponseType, Fault>>
    {
    }

    /// <summary>
    /// Operation Update: Notification of a sensor change
    /// </summary>
    [Description("Indicates an update to a contact sensor's state.")]
    [DisplayName("(User) ContactSensorUpdate")]
    public class Update : Update<ContactSensor, PortSet<DefaultUpdateResponseType, Fault>>
    {
    }

    /// <summary>
    /// Operation Subscribe to bumper
    /// </summary>
    public class Subscribe : Subscribe<SubscribeRequestType, PortSet<SubscribeResponseType, Fault>>
    {
    }

    /// <summary>
    /// Operation Subscribe to bumper
    /// </summary>
    public class ReliableSubscribe : Subscribe<ReliableSubscribeRequestType, PortSet<SubscribeResponseType, Fault>>
    {
    }
}
