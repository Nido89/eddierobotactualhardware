//------------------------------------------------------------------------------
//  <copyright file="RawViewerTypes.cs" company="Microsoft Corporation">
//      Copyright (C) Microsoft Corporation.  All rights reserved.
//  </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Robotics.Tools.DssLogAnalyzerPlugins.RawViewer
{
    using Microsoft.Ccr.Core;
    using Microsoft.Dss.Core.Attributes;
    using Microsoft.Dss.ServiceModel.Dssp;
    using W3C.Soap;

    /// <summary>
    /// RawViewer contract class
    /// </summary>
    public sealed class Contract
    {
        /// <summary>
        /// DSS contract identifer for RawViewer
        /// </summary>
        [DataMember]
        public const string Identifier = "http://schemas.microsoft.com/robotics/2010/01/rawviewer.user.html";
    }

    /// <summary>
    /// RawViewer state
    /// </summary>
    [DataContract]
    public class RawViewerState
    {
    }

    /// <summary>
    /// RawViewer main operations port
    /// </summary>
    [ServicePort]
    public class RawViewerOperations : PortSet<DsspDefaultLookup, DsspDefaultDrop, Get, Subscribe>
    {
    }

    /// <summary>
    /// RawViewer get operation
    /// </summary>
    public class Get : Get<GetRequestType, PortSet<RawViewerState, Fault>>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Get"/> class.
        /// </summary>
        public Get()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Get"/> class.
        /// </summary>
        /// <param name="body">The request message body</param>
        public Get(GetRequestType body)
            : base(body)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Get"/> class.
        /// </summary>
        /// <param name="body">The request message body</param>
        /// <param name="responsePort">The response port for the request</param>
        public Get(GetRequestType body, PortSet<RawViewerState, Fault> responsePort)
            : base(body, responsePort)
        {
        }
    }

    /// <summary>
    /// RawViewer subscribe operation
    /// </summary>
    public class Subscribe : Subscribe<SubscribeRequestType, PortSet<SubscribeResponseType, Fault>>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Subscribe"/> class.
        /// </summary>
        public Subscribe()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Subscribe"/> class.
        /// </summary>
        /// <param name="body">The request message body</param>
        public Subscribe(SubscribeRequestType body)
            : base(body)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Subscribe"/> class.
        /// </summary>
        /// <param name="body">The request message body</param>
        /// <param name="responsePort">The response port for the request</param>
        public Subscribe(SubscribeRequestType body, PortSet<SubscribeResponseType, Fault> responsePort)
            : base(body, responsePort)
        {
        }
    }
}
