//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: MultipleSimulatedSensorsTypes.cs $ $Revision: 4 $
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Ccr.Core;
using Microsoft.Dss.Core.Attributes;
using Microsoft.Dss.ServiceModel.Dssp;
using Microsoft.Dss.ServiceModel.DsspServiceBase;
using W3C.Soap;

namespace Robotics.MultipleSimulatedSensors
{
    /// <summary>
    /// MultipleSimulatedSensors contract class
    /// </summary>
    public sealed class Contract
    {
        /// <summary>
        /// DSS contract identifer for MultipleSimulatedSensors
        /// </summary>
        [DataMember]
        public const string Identifier = "http://schemas.microsoft.com/2009/09/multiplesimulatedsensors.user.html";
    }

    /// <summary>
    /// MultipleSimulatedSensors state
    /// </summary>
    [DataContract]
    public class MultipleSimulatedSensorsState
    {
    }

    /// <summary>
    /// MultipleSimulatedSensors main operations port
    /// </summary>
    [ServicePort]
    public class MultipleSimulatedSensorsOperations : PortSet<DsspDefaultLookup, DsspDefaultDrop, Get, Subscribe>
    {
    }

    /// <summary>
    /// MultipleSimulatedSensors get operation
    /// </summary>
    public class Get : Get<GetRequestType, PortSet<MultipleSimulatedSensorsState, Fault>>
    {
        /// <summary>
        /// Creates a new instance of Get
        /// </summary>
        public Get()
        {
        }

        /// <summary>
        /// Creates a new instance of Get
        /// </summary>
        /// <param name="body">the request message body</param>
        public Get(GetRequestType body)
            : base(body)
        {
        }

        /// <summary>
        /// Creates a new instance of Get
        /// </summary>
        /// <param name="body">the request message body</param>
        /// <param name="responsePort">the response port for the request</param>
        public Get(GetRequestType body, PortSet<MultipleSimulatedSensorsState, Fault> responsePort)
            : base(body, responsePort)
        {
        }
    }

    /// <summary>
    /// MultipleSimulatedSensors subscribe operation
    /// </summary>
    public class Subscribe : Subscribe<SubscribeRequestType, PortSet<SubscribeResponseType, Fault>>
    {
        /// <summary>
        /// Creates a new instance of Subscribe
        /// </summary>
        public Subscribe()
        {
        }

        /// <summary>
        /// Creates a new instance of Subscribe
        /// </summary>
        /// <param name="body">the request message body</param>
        public Subscribe(SubscribeRequestType body)
            : base(body)
        {
        }

        /// <summary>
        /// Creates a new instance of Subscribe
        /// </summary>
        /// <param name="body">the request message body</param>
        /// <param name="responsePort">the response port for the request</param>
        public Subscribe(SubscribeRequestType body, PortSet<SubscribeResponseType, Fault> responsePort)
            : base(body, responsePort)
        {
        }
    }
}


