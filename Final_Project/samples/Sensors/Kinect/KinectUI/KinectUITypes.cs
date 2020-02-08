//------------------------------------------------------------------------------
//  <copyright file="KinectUITypes.cs" company="Microsoft Corporation">
//      Copyright (C) Microsoft Corporation.  All rights reserved.
//  </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Dss.Services.Samples.KinectUI
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using Microsoft.Ccr.Core;
    using Microsoft.Dss.Core.Attributes;
    using Microsoft.Dss.ServiceModel.Dssp;
    using Microsoft.Dss.ServiceModel.DsspServiceBase;
    using W3C.Soap;

    /// <summary>
    /// KinectUI contract class
    /// </summary>
    public sealed class Contract
    {
        /// <summary>
        /// DSS contract identifer for KinectUI
        /// </summary>
        [DataMember]
        public const string Identifier = "http://schemas.microsoft.com/2008/08/kinectui.user.html";
    }

    /// <summary>
    /// KinectUI state
    /// </summary>
    [DataContract]
    public class KinectUIState
    {
    }

    /// <summary>
    /// KinectUI main operations port
    /// </summary>
    [ServicePort]
    public class KinectUIOperations : PortSet<DsspDefaultLookup, DsspDefaultDrop, Get>
    {
    }

    /// <summary>
    /// KinectUI get operation
    /// </summary>
    public class Get : Get<GetRequestType, PortSet<KinectUIState, Fault>>
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
        /// <param name="body">The request message body</param>
        public Get(GetRequestType body)
            : base(body)
        {
        }

        /// <summary>
        /// Creates a new instance of Get
        /// </summary>
        /// <param name="body">The request message body</param>
        /// <param name="responsePort">The response port for the request</param>
        public Get(GetRequestType body, PortSet<KinectUIState, Fault> responsePort)
            : base(body, responsePort)
        {
        }
    }
}