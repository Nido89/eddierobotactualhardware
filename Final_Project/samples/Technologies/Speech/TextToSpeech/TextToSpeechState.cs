//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: TextToSpeechState.cs $ $Revision: 11 $
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Net;
using System.Security.Permissions;
using System.Speech.Synthesis;

using Microsoft.Ccr.Core;
using Microsoft.Dss.Core.Attributes;
using Microsoft.Dss.Core.DsspHttp;
using Microsoft.Dss.ServiceModel.DsspServiceBase;
using Microsoft.Dss.ServiceModel.Dssp;

using W3C.Soap;

namespace Microsoft.Robotics.Technologies.Speech.TextToSpeech
{
    /// <summary>
    /// Text To Speech Service state
    /// </summary>
    [DataContract]
    public class TextToSpeechState
    {
        /// <summary>
        /// The default value for the volume 
        /// </summary>
        internal const int DefaultVolume = 100;

        SpeechSynthesizer _synthesizer;
        /// <summary>
        /// Speech synthesizer object
        /// </summary>
        internal SpeechSynthesizer Synthesizer
        {
            get { return _synthesizer; }
            set { _synthesizer = value; }
        }

        List<string> _voices;
        /// <summary>
        /// List of available voices
        /// </summary>
        [DataMember]
        [DisplayName("(User) List of available voices")]
        [Description ("Identifies the set of voices available.")]
        public List<string> Voices
        {
            get { return _voices; }
            set { _voices = value; }
        }

        string _voice;
        /// <summary>
        /// Voice which will say text
        /// </summary>
        [DataMember]
        [DisplayName("(User) Voice which will say text")]
        [Description("Specifies the specific voice used.")]
        public string Voice
        {
            get { return _voice; }
            set { _voice = value; }
        }

        int _volume;
        /// <summary>
        /// Volume of voice (range: 0 to 100)
        /// </summary>
        [DataMember]
        [DisplayName("(User) Voice volume")]
        [Description("Specifies the speaking output volume.\n(0 to 100)")]
        public int Volume
        {
            get { return _volume; }
            set { _volume = value; }
        }

        int _rate;
        /// <summary>
        /// Rate of speech (range: -10 to 10)
        /// </summary>
        [DataMember]
        [DisplayName("(User) Speech rate")]
        [Description("Specifies the speaking rate.\n(-10 to +10)")]
        public int Rate
        {
            get { return _rate; }
            set { _rate = value; }
        }

        string _speechText;
        /// <summary>
        /// Text to be spoken
        /// </summary>
        [DataMember]
        [DisplayName("(User) Text to be spoken")]
        [Description("Specifies the text to be spoken.")]
        public string SpeechText
        {
            get { return _speechText; }
            set { _speechText = value; }
        }

        bool _disableAudioOutput;
        /// <summary>
        /// Output audio
        /// </summary>
        [DataMember]
        [DisplayName("(User) Disable audio output")]
        [Description("Whether audio output shall be disabled or not.")]
        public bool DisableAudioOutput
        {
            get { return _disableAudioOutput; }
            set { _disableAudioOutput = value; }
        }
    }

    /// <summary>
    /// Text to be spoken asynchronously
    /// </summary>
    [DataContract]
    public class SayTextRequest
    {
        /// <summary>
        /// Default Constructor
        /// </summary>
        public SayTextRequest()
        {
        }

        /// <summary>
        /// Constructor to initialize members
        /// </summary>
        /// <param name="speechText">Text to be spoken</param>
        public SayTextRequest(string speechText)
        {
            _speechText = speechText;
        }

        string _speechText;
        /// <summary>
        /// Text to be spoken
        /// </summary>
        [DataMember]
        [Description("Specifies the text to be spoken.")]
        public string SpeechText
        {
            get { return _speechText; }
            set { _speechText = value; }
        }
    }


    /// <summary>
    /// Text to be spoken synchronously
    /// </summary>
    [DataContract]
    public class SayTextSynchRequest
    {
        string _speechText;
        /// <summary>
        /// Text to be spoken
        /// </summary>
        [DataMember]
        [Description("Specifies the text to be spoken.")]
        public string SpeechText
        {
            get { return _speechText; }
            set { _speechText = value; }
        }
    }

    /// <summary>
    /// Request body for the SetRate message
    /// </summary>
    /// <see cref="SetRate"/>
    [DataContract]
    public class SetRateRequest
    {
        int _rate;
        /// <summary>
        /// The new speech rate.
        /// This must be between -10 and 10.
        /// </summary>
        [DataMember]
        [Description("Specifies the speaking rate.\n(-10 to +10)")]
        public int Rate
        {
            get { return _rate; }
            set { _rate = value; }
        }
    }

    /// <summary>
    /// Request body for the SetVolume message
    /// </summary>
    /// <see cref="SetVolume"/>
    [DataContract]
    public class SetVolumeRequest
    {
        int _volume;
        /// <summary>
        /// The new speech volume.
        /// This must be between 0 and 100.
        /// </summary>
        [DataMember]
        [Description("Specifies the speaking output volume.\n(0 to 100)")]
        public int Volume
        {
            get { return _volume; }
            set { _volume = value; }
        }
    }

    /// <summary>
    /// Request body for the SetVoice message
    /// </summary>
    /// <see cref="SetVoice"/>
    [DataContract]
    public class SetVoiceRequest
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public SetVoiceRequest()
        {
        }

        /// <summary>
        /// Constructor to initialize member.
        /// </summary>
        /// <param name="voice">The name of the voice to use</param>
        public SetVoiceRequest(string voice)
        {
            _voice = voice;
        }

        string _voice;
        /// <summary>
        /// The new voice to use.
        /// This must be in the list in the state.
        /// </summary>
        [DataMember]
        [Description("Identifies the specific voice used.")]
        public string Voice
        {
            get { return _voice; }
            set { _voice = value; }
        }
    }

    /// <summary>
    /// Viseme notification
    /// </summary>
    [DataContract]
    public class VisemeNotification
    {
        int _viseme;
        /// <summary>
        /// Current viseme
        /// </summary>
        [DataMember]
        [Description("Current viseme.")]
        public int Viseme
        {
            get { return _viseme; }
            set { _viseme = value; }
        }

        int _nextViseme;
        /// <summary>
        /// Next viseme
        /// </summary>
        [DataMember]
        [Description("Next viseme.")]
        public int NextViseme
        {
            get { return _nextViseme; }
            set { _nextViseme = value; }
        }

        TimeSpan _audioPosition;
        /// <summary>
        /// Audio position
        /// </summary>
        internal TimeSpan AudioPosition
        {
            get { return _audioPosition; }
            set { _audioPosition = value; }
        }

        /// <summary>
        /// Audio position in ticks. Workaround using ticks because
        /// System.TimeSpan itself cannot be XML serialized
        /// </summary>
        [DataMember]
        [Description("Audio position in ticks.")]
        public long AudioPositionInTicks
        {
            get { return _audioPosition.Ticks; }
            set { _audioPosition = new TimeSpan(value); }
        }
        
        TimeSpan _duration;
        /// <summary>
        /// Duration
        /// </summary>
        internal TimeSpan Duration
        {
            get { return _duration; }
            set { _duration = value; }
        }

        /// <summary>
        /// Duration in ticks. Workaround using ticks because
        /// System.TimeSpan itself cannot be XML serialized
        /// </summary>
        [DataMember]
        [Description("Duration in ticks.")]
        public long DurationInTicks
        {
            get { return _duration.Ticks; }
            set { _duration = new TimeSpan(value); }
        }

        SynthesizerEmphasis _emphasis;
        /// <summary>
        /// Emphasis
        /// </summary>
        [DataMember]
        [Description("Emphasis")]
        public SynthesizerEmphasis Emphasis
        {
            get { return _emphasis; }
            set { _emphasis = value; }
        }
    }
}
