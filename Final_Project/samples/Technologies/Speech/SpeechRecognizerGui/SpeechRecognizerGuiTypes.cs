//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: SpeechRecognizerGuiTypes.cs $ $Revision: 2 $
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using W3C.Soap;

using Microsoft.Ccr.Core;
using Microsoft.Dss.Core.Attributes;
using Microsoft.Dss.Core.DsspHttp;
using Microsoft.Dss.Core.Utilities;
using Microsoft.Dss.ServiceModel.Dssp;

using sr = Microsoft.Robotics.Technologies.Speech.SpeechRecognizer.Proxy;
using speechrecognizergui = Microsoft.Robotics.Technologies.Speech.SpeechRecognizerGui;

namespace Microsoft.Robotics.Technologies.Speech.SpeechRecognizerGui
{
    /// <summary>
    /// SpeechRecognizerGui Contract class
    /// </summary>
    public sealed class Contract
    {
        /// <summary>
        /// The Dss Service contract
        /// </summary>
        [DataMember]
        public const String Identifier = "http://schemas.microsoft.com/robotics/2008/03/speechrecognizergui.user.html";
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
        private List<EventListEntry> _events;
        /// <summary>
        /// List of events
        /// </summary>
        [DataMember]
        public List<EventListEntry> Events
        {
            get { return _events; }
            set { _events = value; }
        }
    }
}
