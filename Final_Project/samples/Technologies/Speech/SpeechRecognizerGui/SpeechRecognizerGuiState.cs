//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: SpeechRecognizerGuiState.cs $ $Revision: 3 $
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel;

using Microsoft.Dss.Core.Attributes;
using Microsoft.Dss.Core.Utilities;
using sr = Microsoft.Robotics.Technologies.Speech.SpeechRecognizer.Proxy;

namespace Microsoft.Robotics.Technologies.Speech.SpeechRecognizerGui
{

    /// <summary>
    /// The SpeechRecognizerGui state
    /// </summary>
    [DataContract]
    public class SpeechRecognizerGuiState
    {
        private sr.SpeechRecognizerState _speechRecognizerState;
        /// <summary>
        /// The speech recognizer's state
        /// </summary>
        [DataMember]
        [Description("The speech recognizer's state.")]
        public sr.SpeechRecognizerState SpeechRecognizerState
        {
            get { return _speechRecognizerState; }
            set { _speechRecognizerState = value; }
        }

        private List<EventListEntry> _speechEvents = new List<EventListEntry>();
        /// <summary>
        /// Past speech events received from speech recognizer
        /// </summary>
        [DataMember]
        [Description("Past speech events received from speech recognizer.")]
        public List<EventListEntry> SpeechEvents
        {
            get { return _speechEvents; }
            set { _speechEvents = value; }
        }

    }

    #region EventListEntry class, needed to serialize a list of mixed types
    /// <summary>
    /// EventListEntry - A list of events that have occurred
    /// </summary>
    [DataContract]
    public class EventListEntry
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public EventListEntry()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="content">Detected notification event</param>
        public EventListEntry(sr.SpeechDetectedNotification content)
        {
            SpeechDetected = content;
            Timestamp = DateTime.Now.Ticks;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="content">Recognized notification event</param>
        public EventListEntry(sr.SpeechRecognizedNotification content)
        {
            SpeechRecognized = content;
            Timestamp = DateTime.Now.Ticks;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="content">Rejected notification event</param>
        public EventListEntry(sr.SpeechRecognitionRejectedNotification content)
        {
            RecognitionRejected = content;
            Timestamp = DateTime.Now.Ticks;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="content">Info on the recognized speech</param>
        public EventListEntry(sr.SpeechInformation content)
        {
            SpeechInformation = content;
            Timestamp = DateTime.Now.Ticks;
        }

        private long _timestamp;
        /// <summary>
        /// Timestamp when the event occurred
        /// </summary>
        [DataMember]
        public long Timestamp
        {
            get { return _timestamp; }
            set { _timestamp = value; }
        }

        private sr.SpeechDetectedNotification _speechDetected;
        /// <summary>
        /// Speech Detected Notification
        /// </summary>
        [DataMember]
        public sr.SpeechDetectedNotification SpeechDetected
        {
            get { return _speechDetected; }
            set { _speechDetected = value; }
        }

        private sr.SpeechRecognizedNotification _speechRecognized;
        /// <summary>
        /// Speech Recognized Notification
        /// </summary>
        [DataMember]
        public sr.SpeechRecognizedNotification SpeechRecognized
        {
            get { return _speechRecognized; }
            set { _speechRecognized = value; }
        }

        private sr.SpeechRecognitionRejectedNotification _recognitionRejected;
        /// <summary>
        /// Speech Rejected Notification
        /// </summary>
        [DataMember]
        public sr.SpeechRecognitionRejectedNotification RecognitionRejected
        {
            get { return _recognitionRejected; }
            set { _recognitionRejected = value; }
        }

        private sr.SpeechInformation _speechInformation;
        /// <summary>
        /// Speech Information
        /// </summary>
        [DataMember]
        public sr.SpeechInformation SpeechInformation
        {
            get { return _speechInformation; }
            set { _speechInformation = value; }
        }
    }
    #endregion

    /// <summary>
    /// Events query request type
    /// </summary>
    [DataContract]
    public class EventsQueryRequestType
    {
        private long _newerThanTimestamp;
        /// <summary>
        /// Newer Than Timestamp
        /// </summary>
        [DataMember]
        public long NewerThanTimestamp
        {
            get { return _newerThanTimestamp; }
            set { _newerThanTimestamp = value; }
        }
    }

    /// <summary>
    /// SpeechRecognizer state query request type
    /// </summary>
    [DataContract]
    public class SpeechRecognizerStateQueryRequestType
    {
    }

    /// <summary>
    /// HTTP POST success type
    /// </summary>
    [DataContract]
    public class HttpPostSuccess
    {
        private static HttpPostSuccess _instance;

        /// <summary>
        /// Default response instance
        /// </summary>
        public static HttpPostSuccess Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new HttpPostSuccess();
                }

                return _instance;
            }
        }
    }
}
