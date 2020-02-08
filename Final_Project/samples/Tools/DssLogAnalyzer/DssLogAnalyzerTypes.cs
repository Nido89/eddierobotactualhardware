//------------------------------------------------------------------------------
//  <copyright file="DssLogAnalyzerTypes.cs" company="Microsoft Corporation">
//      Copyright (C) Microsoft Corporation.  All rights reserved.
//  </copyright>
//------------------------------------------------------------------------------
namespace Microsoft.Robotics.Tools.DssLogAnalyzer
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Ccr.Core;
    using Microsoft.Dss.Core.Attributes;
    using Microsoft.Dss.ServiceModel.Dssp;
    using W3C.Soap;

    /// <summary>
    /// Plugin interface
    /// </summary>
    public interface IDssLogAnalyzerPluginUi
    {
        /// <summary>
        /// Interface to Update the UI from data
        /// </summary>
        /// <param name="envelopes">List of data envelopes</param>
        void UpdateUiFromEnvelopes(List<Envelope> envelopes);
    }

    /// <summary>
    /// Plugin service interface
    /// </summary>
    public interface IDssLogAnalyzerPluginService
    {
        /// <summary>
        /// ITask enumerator interface for processing loaded log files
        /// </summary>
        /// <param name="loaded">Which log files have been loaded</param>
        /// <returns>ITask enumerator</returns>
        IEnumerator<ITask> LogFilesLoaded(LogFilesLoaded loaded);

        /// <summary>
        /// ITask enumerator interface for processing selected envelopes
        /// </summary>
        /// <param name="selected">Envelopes selected</param>
        /// <returns>ITask enumerator</returns>
        IEnumerator<ITask> EnvelopesSelected(EnvelopesSelected selected);
    }

    /// <summary>
    /// Data envelope header
    /// </summary>
    [DataContract]
    public class EnvelopeHeader
    {
        /// <summary>
        /// Gets or sets which action this header is
        /// </summary>
        [DataMember]
        public string Action { get; set; }

        /// <summary>
        /// Gets or sets from URL
        /// </summary>
        [DataMember]
        public string From { get; set; }

        /// <summary>
        /// Gets or sets Message ID
        /// </summary>
        [DataMember]
        public Guid MessageId { get; set; }

        /// <summary>
        /// Gets or sets To URL
        /// </summary>
        [DataMember]
        public string To { get; set; }

        /// <summary>
        /// Gets or sets the timestamp
        /// </summary>
        [DataMember]
        public DateTime TimeStamp { get; set; }
    }

    /// <summary>
    /// Data Envelope
    /// </summary>
    [DataContract]
    public class Envelope
    {
        /// <summary>
        /// Gets or sets the frame offset
        /// </summary>
        [DataMember]
        public long Offset { get; set; }
        
        /// <summary>
        /// Gets or sets envelope filename
        /// </summary>
        [DataMember]
        public string Filename { get; set; }

        /// <summary>
        /// Gets or sets envelope header
        /// </summary>
        [DataMember]
        public EnvelopeHeader Header { get; set; }
    }

    /// <summary>
    /// DssLogAnalyzer contract class
    /// </summary>
    public sealed class Contract
    {
        /// <summary>
        /// DSS contract identifer for DssLogAnalyzer
        /// </summary>
        [DataMember]
        public const string Identifier = "http://schemas.microsoft.com/robotics/2009/11/dssloganalyzer.user.html";
    }

    /// <summary>
    /// A list of envelopes that all originate from the same file
    /// </summary>
    [DataContract]
    public class EnvelopeList
    {
        /// <summary>
        /// Gets or sets envelope list
        /// </summary>
        /// <value>
        /// The envelopes.
        /// </value>
        [DataMember]
        public List<Envelope> Envelopes { get; set; }
    }

    /// <summary>
    /// DssLogAnalyzer state
    /// </summary>
    [DataContract]
    public class DssLogAnalyzerState
    {
        /// <summary>
        /// Gets or sets currently selected envelope
        /// </summary>
        [DataMember]
        public EnvelopeList CurrentSelectedEnvelopes;

        /// <summary>
        /// Gets or sets a value indicating whether envelope list
        /// </summary>
        [DataMember]
        public List<EnvelopeList> Envelopes { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether root log files folder
        /// </summary>
        [DataMember]
        public string LogFilesFolder { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the list of log files per directory
        /// </summary>
        [DataMember]
        public Dictionary<string, List<EnvelopeList>> LogFileEnvelopes { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the UI should be displayed
        /// </summary>
        /// <value>
        ///   <c>true</c> if headless; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool Headless { get; set; }
    }

    /// <summary>
    /// DssLogAnalyzer main operations port
    /// </summary>
    [ServicePort]
    public class DssLogAnalyzerOperations : PortSet<DsspDefaultLookup, DsspDefaultDrop, Get, Subscribe, 
        LogFilesLoaded, EnvelopesSelected>
    {
    }

    /// <summary>
    /// DssLogAnalyzer get operation
    /// </summary>
    public class Get : Get<GetRequestType, PortSet<DssLogAnalyzerState, Fault>>
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
        public Get(GetRequestType body, PortSet<DssLogAnalyzerState, Fault> responsePort)
            : base(body, responsePort)
        {
        }
    }

    /// <summary>
    /// DssLogAnalyzer subscribe operation
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

    /// <summary>
    /// Log files loaded request
    /// </summary>
    [DataContract]
    public class LogFilesLoadedRequest
    {
    }

    /// <summary>
    /// Log files loaded update request
    /// </summary>
    public class LogFilesLoaded : Update<LogFilesLoadedRequest, DsspResponsePort<DefaultUpdateResponseType>>
    {
    }

    /// <summary>
    /// Envelope selected request
    /// </summary>
    [DataContract]
    public class EnvelopesSelectedRequest
    {
    }

    /// <summary>
    /// Envelope selected update request
    /// </summary>
    public class EnvelopesSelected : Update<EnvelopesSelectedRequest, DsspResponsePort<DefaultUpdateResponseType>>
    {
    }
}
