//------------------------------------------------------------------------------
//  <copyright file="SonarSensorArrayTypes.cs" company="Microsoft Corporation">
//      Copyright (C) Microsoft Corporation.  All rights reserved.
//  </copyright>
//------------------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//------------------------------------------------------------------------------

namespace Microsoft.Robotics.Services.SonarSensorArray
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
    /// Sonar Sensor Contract
    /// </summary>
    public static class Contract
    {
        /// <summary>
        /// The Unique Contract Identifier
        /// </summary>
        public const string Identifier = "http://schemas.microsoft.com/robotics/2011/09/sonarsensorarray.html";
    }

    /// <summary>
    /// Sonar Sensor Operations Port
    /// </summary>
    [ServicePort]
    public class SonarSensorOperations : PortSet<DsspDefaultLookup,
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
    public class Get : Get<GetRequestType, PortSet<SonarSensorArrayState, Fault>> 
    { 
    }

    /// <summary>
    /// Replace Operation
    /// </summary>
    public class Replace : Replace<SonarSensorArrayState, PortSet<DefaultReplaceResponseType, Fault>> 
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
    public class ReliableSubscribe : Subscribe<ReliableSubscribeRequestType, DsspResponsePort<SubscribeResponseType>, SonarSensorOperations> 
    { 
    }
}
