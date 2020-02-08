//------------------------------------------------------------------------------
//  <copyright file="SpeechRecognizerGuiState.cs" company="Microsoft Corporation">
//      Copyright (C) Microsoft Corporation.  All rights reserved.
//  </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Robotics.Services.Sensors.Kinect.MicArraySpeechRecognizerGui
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;

    using Microsoft.Dss.Core.Attributes;
    using Microsoft.Dss.Core.Utilities;
    using sr = Microsoft.Robotics.Services.Sensors.Kinect.MicArraySpeechRecognizer.Proxy;

    /// <summary>
    /// The SpeechRecognizerGui state
    /// </summary>
    [DataContract]
    public class SpeechRecognizerGuiState
    {
        private sr.SpeechRecognizerState speechRecognizerState;

        /// <summary>
        /// The speech recognizer's state
        /// </summary>
        [DataMember]
        [Description("The speech recognizer's state.")]
        public sr.SpeechRecognizerState SpeechRecognizerState
        {
            get { return this.speechRecognizerState; }
            set { this.speechRecognizerState = value; }
        }

        private List<EventListEntry> speechEvents = new List<EventListEntry>();

        /// <summary>
        /// Past speech events received from speech recognizer
        /// </summary>
        [DataMember]
        [Description("Past speech events received from speech recognizer.")]
        public List<EventListEntry> SpeechEvents
        {
            get { return this.speechEvents; }
            set { this.speechEvents = value; }
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
            this.SpeechDetected = content;
            this.Timestamp = DateTime.Now.Ticks;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="content">Recognized notification event</param>
        public EventListEntry(sr.SpeechRecognizedNotification content)
        {
            this.SpeechRecognized = content;
            this.Timestamp = DateTime.Now.Ticks;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="content">Rejected notification event</param>
        public EventListEntry(sr.SpeechRecognitionRejectedNotification content)
        {
            this.RecognitionRejected = content;
            this.Timestamp = DateTime.Now.Ticks;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="content">Beam changed notification event</param>
        public EventListEntry(sr.BeamDirectionChangedNotification content)
        {
            this.BeamDirectionChanged = content;
            this.Timestamp = DateTime.Now.Ticks;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="content">Info on the recognized speech</param>
        public EventListEntry(sr.SpeechInformation content)
        {
            this.SpeechInformation = content;
            this.Timestamp = DateTime.Now.Ticks;
        }

        private long timestamp;

        /// <summary>
        /// Timestamp when the event occurred
        /// </summary>
        [DataMember]
        public long Timestamp
        {
            get { return this.timestamp; }
            set { this.timestamp = value; }
        }

        private sr.SpeechDetectedNotification speechDetected;

        /// <summary>
        /// Speech Detected Notification
        /// </summary>
        [DataMember]
        public sr.SpeechDetectedNotification SpeechDetected
        {
            get { return this.speechDetected; }
            set { this.speechDetected = value; }
        }

        private sr.SpeechRecognizedNotification speechRecognized;

        /// <summary>
        /// Speech Recognized Notification
        /// </summary>
        [DataMember]
        public sr.SpeechRecognizedNotification SpeechRecognized
        {
            get { return this.speechRecognized; }
            set { this.speechRecognized = value; }
        }

        private sr.SpeechRecognitionRejectedNotification recognitionRejected;

        /// <summary>
        /// Speech Rejected Notification
        /// </summary>
        [DataMember]
        public sr.SpeechRecognitionRejectedNotification RecognitionRejected
        {
            get { return this.recognitionRejected; }
            set { this.recognitionRejected = value; }
        }

        private sr.BeamDirectionChangedNotification beamDirectionChanged;

        /// <summary>
        /// Speech Detected Notification
        /// </summary>
        [DataMember]
        public sr.BeamDirectionChangedNotification BeamDirectionChanged
        {
            get { return this.beamDirectionChanged; }
            set { this.beamDirectionChanged = value; }
        }

        private sr.SpeechInformation speechInformation;

        /// <summary>
        /// Speech Information
        /// </summary>
        [DataMember]
        public sr.SpeechInformation SpeechInformation
        {
            get { return this.speechInformation; }
            set { this.speechInformation = value; }
        }
    }
    #endregion

    /// <summary>
    /// Events query request type
    /// </summary>
    [DataContract]
    public class EventsQueryRequestType
    {
        private long newerThanTimestamp;

        /// <summary>
        /// Newer Than Timestamp
        /// </summary>
        [DataMember]
        public long NewerThanTimestamp
        {
            get { return this.newerThanTimestamp; }
            set { this.newerThanTimestamp = value; }
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
        private static HttpPostSuccess instance;

        /// <summary>
        /// Default response instance
        /// </summary>
        public static HttpPostSuccess Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new HttpPostSuccess();
                }

                return instance;
            }
        }
    }
}
