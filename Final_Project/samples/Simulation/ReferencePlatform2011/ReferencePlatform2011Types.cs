//------------------------------------------------------------------------------
//  <copyright file="ReferencePlatform2011Types.cs" company="Microsoft Corporation">
//      Copyright (C) Microsoft Corporation.  All rights reserved.
//  </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Robotics.Services.Simulation.ReferencePlatform2011
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using Microsoft.Ccr.Core;
    using Microsoft.Dss.Core.Attributes;
    using Microsoft.Dss.ServiceModel.Dssp;
    using Microsoft.Dss.ServiceModel.DsspServiceBase;
    using Microsoft.Robotics.PhysicalModel;
    using W3C.Soap;
    using drive = Microsoft.Robotics.Services.Drive;

    /// <summary>
    /// ReferencePlatform2011 contract class
    /// </summary>
    public sealed class Contract
    {
        /// <summary>
        /// DSS contract identifier for ReferencePlatform2011
        /// </summary>
        [DataMember]
        public const string Identifier = "http://schemas.microsoft.com/2009/03/simulatedreferenceplatform2011.user.html";
    }

    /// <summary>
    /// ReferencePlatform2011 state
    /// </summary>
    [DataContract]
    public class ReferencePlatform2011State
    {
        /// <summary>
        /// Gets or sets the initial position the robot should be placed in sim
        /// </summary>
        [DataMember]
        public Vector3 InitialPosition { get; set; }

        /// <summary>
        /// Gets or sets the name to be used for this entity
        /// </summary>
        [DataMember]
        public string EntityName { get; set; }

        /// <summary>
        /// Gets or sets the differential drive state
        /// </summary>
        [DataMember]
        public drive.DriveDifferentialTwoWheelState DriveState { get; set; }
    }

    /// <summary>
    /// ReferencePlatform2011 main operations port
    /// </summary>
    [ServicePort]
    public class ReferencePlatform2011Operations : PortSet<DsspDefaultLookup, DsspDefaultDrop, Get, Subscribe>
    {
    }

    /// <summary>
    /// ReferencePlatform2011 get operation
    /// </summary>
    public class Get : Get<GetRequestType, PortSet<ReferencePlatform2011State, Fault>>
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
        public Get(GetRequestType body, PortSet<ReferencePlatform2011State, Fault> responsePort)
            : base(body, responsePort)
        {
        }
    }

    /// <summary>
    /// ReferencePlatform2011 subscribe operation
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
        /// <param name="body">The request message body</param>
        public Subscribe(SubscribeRequestType body)
            : base(body)
        {
        }

        /// <summary>
        /// Creates a new instance of Subscribe
        /// </summary>
        /// <param name="body">The request message body</param>
        /// <param name="responsePort">The response port for the request</param>
        public Subscribe(SubscribeRequestType body, PortSet<SubscribeResponseType, Fault> responsePort)
            : base(body, responsePort)
        {
        }
    }
}
