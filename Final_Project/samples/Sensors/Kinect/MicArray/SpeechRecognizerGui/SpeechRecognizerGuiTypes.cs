//------------------------------------------------------------------------------
//  <copyright file="SpeechRecognizerGuiTypes.cs" company="Microsoft Corporation">
//      Copyright (C) Microsoft Corporation.  All rights reserved.
//  </copyright>
//------------------------------------------------------------------------------
namespace Microsoft.Robotics.Services.Sensors.Kinect.MicArraySpeechRecognizerGui
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Ccr.Core;
    using Microsoft.Dss.Core.Attributes;
    using Microsoft.Dss.Core.DsspHttp;
    using Microsoft.Dss.Core.Utilities;
    using Microsoft.Dss.ServiceModel.Dssp;
    using W3C.Soap;

    using sr = Microsoft.Robotics.Services.Sensors.Kinect.MicArraySpeechRecognizer.Proxy;

    /// <summary>
    /// SpeechRecognizerGui Contract class
    /// </summary>
    public sealed class Contract
    {
        /// <summary>
        /// The Dss Service contract
        /// </summary>
        [DataMember]
        public const string Identifier = "http://schemas.microsoft.com/robotics/2011/06/micarrayspeechrecognizergui.user.html";                                          
    }
    
    /// <summary>
    /// SpeechRecognizerGui Main Operations Port
    /// </summary>
    [ServicePort]
    public class SpeechRecognizerGuiOperations : PortSet<
        DsspDefaultLookup,
        DsspDefaultDrop,
        Get,
        EventsQuery,
        SpeechRecognizerStateQuery,
        HttpGet,
        HttpQuery,
        HttpPost>
    {
    }
    
    /// <summary>
    /// SpeechRecognizerGui Get Operation
    /// </summary>
    public class Get : Get<GetRequestType, DsspResponsePort<SpeechRecognizerGuiState>>
    {
        
        /// <summary>
        /// SpeechRecognizerGui Get Operation
        /// </summary>
        public Get()
        {
        }
        
        /// <summary>
        /// SpeechRecognizerGui Get Operation
        /// </summary>
        public Get(GetRequestType body) : base(body)
        {
        }
        
        /// <summary>
        /// SpeechRecognizerGui Get Operation
        /// </summary>
        public Get(GetRequestType body, DsspResponsePort<SpeechRecognizerGuiState> responsePort) : 
                base(body, responsePort)
        {
        }
    }

    /// <summary>
    /// Events query operation
    /// </summary>
    public class EventsQuery : Query<EventsQueryRequestType, DsspResponsePort<EventsQueryResponse>>
    {
    }

    /// <summary>
    /// Speech recognizer state query operation
    /// </summary>
    public class SpeechRecognizerStateQuery : Query<
        SpeechRecognizerStateQueryRequestType,
        DsspResponsePort<sr.SpeechRecognizerState>>
    {
    }

    /// <summary>
    /// Events query response
    /// </summary>
    [DataContract]
    public class EventsQueryResponse
    {
        private List<EventListEntry> events;
        /// <summary>
        /// List of events
        /// </summary>
        [DataMember]
        public List<EventListEntry> Events
        {
            get { return this.events; }
            set { this.events = value; }
        }
    }
}
