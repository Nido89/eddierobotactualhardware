//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: SonarTypes.cs $ $Revision: 20 $
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

namespace Microsoft.Robotics.Services.Sonar
{
    /// <summary>
    /// Sonar Contract
    /// </summary>
    [DisplayName("(User) Generic Sonar")]
    [Description("Provides access to a sonar sensor.")]
    [DssServiceDescription("http://msdn.microsoft.com/library/dd145256.aspx")]
    public static class Contract
    {
        /// The Unique Contract Identifier for the Sonar service
        public const string Identifier = "http://schemas.microsoft.com/robotics/2006/06/sonar.html";
    }

    /// <summary>
    /// Sonar Service Operations Port
    /// </summary>
    [ServicePort]
    public class SonarOperations : PortSet<
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
    /// Sonar Service Get Operation
    /// </summary>
    [Description("Gets the sonar's current state.")]
    public class Get : Get<GetRequestType, PortSet<SonarState, Fault>> { }
    /// <summary>
    /// Sonar Service Replace Operation
    /// </summary>
    [Description("Indicates a change to the sonar sensor readings.")]
    [DisplayName("(User) SonarUpdate")]
    public class Replace : Replace<SonarState, PortSet<DefaultReplaceResponseType, Fault>> { }
    /// <summary>
    /// Sonar Service Subscribe Operation
    /// </summary>
    [Description("Subscribe to SonarUpdate readings.")]
    public class Subscribe : Subscribe<SubscribeRequestType, PortSet<SubscribeResponseType, Fault>> { }
    /// <summary>
    /// Sonar Service ReliableSubscribe Operation
    /// </summary>
    [Description("Subscribe to SonarUpdate readings.")]
    public class ReliableSubscribe : Subscribe<ReliableSubscribeRequestType, DsspResponsePort<SubscribeResponseType>, SonarOperations> { }
}
