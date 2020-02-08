//------------------------------------------------------------------------------
//  <copyright file="SpeechRecognizerState.cs" company="Microsoft Corporation">
//      Copyright (C) Microsoft Corporation.  All rights reserved.
//  </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Robotics.Services.Sensors.Kinect.MicArraySpeechRecognizer
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Text;
    using Microsoft.Dss.Core.Attributes;

    using sr = Microsoft.Speech.Recognition;

    /// <summary>
    /// Speech recognizer service state
    /// </summary>
    [DataContract]
    public class SpeechRecognizerState
    {
        
        /// <summary>
        /// Speech recognizer state constructor
        /// </summary>
        public SpeechRecognizerState()
        {
            this.dictionaryGrammar = new Dictionary<string, string>();
            this.grammarType = GrammarType.DictionaryStyle;
            this.kinectAudioBeamInformation = new AudioBeamInformation();
        }            

        private AudioBeamInformation kinectAudioBeamInformation;
        /// <summary>
        /// Beam information captured at the time of beam change event
        /// </summary>
        internal AudioBeamInformation KinectAudioBeamInformation
        {
            get { return this.kinectAudioBeamInformation; }
            set { this.kinectAudioBeamInformation = value; }
        }

        private sr.SpeechRecognitionEngine recognizer;
        /// <summary>
        /// Speech recognition engine
        /// </summary>
        internal sr.SpeechRecognitionEngine Recognizer
        {
            get { return this.recognizer; }
            set { this.recognizer = value; }
        }

        private bool ignoreAudioInput;
        /// <summary>
        /// Whether input from the default audio input device shall be ignored
        /// </summary>
        [DataMember]
        public bool IgnoreAudioInput
        {
            get { return this.ignoreAudioInput; }
            set { this.ignoreAudioInput = value; }
        }

        private GrammarType grammarType;
        /// <summary>
        /// Type of grammar the recognizer uses
        /// </summary>
        [DataMember]
        [Description("Specifies the type of grammar that is used by the recognizer.")]
        public GrammarType GrammarType
        {
            get { return this.grammarType; }
            set { this.grammarType = value; }
        }

        private Dictionary<string, string> dictionaryGrammar;
        /// <summary>
        /// Dictionary entries of the dictionary-style grammar, where an entry's key is the
        /// recognizable phrase and its value is the phrase's meaning (semantic value).
        /// </summary>
        [DataMember]
        [Description("Dictionary entries of the dictionary-style grammar.")]
        public Dictionary<string, string> DictionaryGrammar
        {
            get { return this.dictionaryGrammar; }
            set { this.dictionaryGrammar = value; }
        }

        private string srgsFileLocation;
        /// <summary>
        /// Location of SRGS (Speech Recognition Grammar Specification) file that shall be
        /// used as a grammar.
        /// </summary>
        /// <remarks>
        /// SRGS grammars require Microsoft Windows Vista and will not work with Microsoft Windows XP/Server 2003.
        /// </remarks>
        [DataMember]
        [Description("Location of SRGS (Speech Recognition Grammar Specification) file that shall be used as a grammar.")]
        public string SrgsFileLocation
        {
            get { return this.srgsFileLocation; }
            set { this.srgsFileLocation = value; }
        }

        private sr.Grammar grammar;
        /// <summary>
        /// The grammar object used in the recognition engine
        /// </summary>
        internal sr.Grammar Grammar
        {
            get { return this.grammar; }
            set { this.grammar = value; }
        }
    }

    /// <summary>
    /// Insert grammar request
    /// </summary>
    [DataContract]
    public class InsertGrammarRequest : ModifyGrammarDictionaryRequest
    {
    }

    /// <summary>
    /// Update grammar request
    /// </summary>
    [DataContract]
    public class UpdateGrammarRequest : ModifyGrammarDictionaryRequest
    {
    }

    /// <summary>
    /// Update or insert grammar request
    /// </summary>
    [DataContract]
    public class UpsertGrammarRequest : ModifyGrammarDictionaryRequest
    {
    }

    /// <summary>
    /// Delete grammar request
    /// </summary>
    [DataContract]
    public class DeleteGrammarRequest : ModifyGrammarDictionaryRequest
    {
    }

    /// <summary>
    /// Modify grammar dictionary request
    /// </summary>
    [DataContract]
    public class ModifyGrammarDictionaryRequest
    {
        private Dictionary<string, string> dictionaryGrammar;
        /// <summary>
        /// Dictionary grammar that shall be used to modify the current dictionary
        /// grammar.
        /// </summary>
        [DataMember]
        [Description("Dictionary grammar that shall be used to modify the current dictionary grammar.")]
        public Dictionary<string, string> DictionaryGrammar
        {
            get { return this.dictionaryGrammar; }
            set { this.dictionaryGrammar = value; }
        }
    }

    /// <summary>
    /// Set SRGS grammar file request
    /// </summary>
    [DataContract]
    public class SetSrgsGrammarFileRequest
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public SetSrgsGrammarFileRequest()
        {
        }

        private string fileLocation;
        /// <summary>
        /// Location of the SRGS grammar file inside the DSS node's /store directory
        /// </summary>
        [DataMember]
        public String FileLocation
        {
            get { return this.fileLocation; }
            set { this.fileLocation = value; }
        }
    }

    /// <summary>
    /// Emulate recognize request
    /// </summary>
    [DataContract]
    public class EmulateRecognizeRequest
    {
        private string text;
        /// <summary>
        /// Text of speech that shall be emulated
        /// </summary>
        [DataMember]
        public String Text
        {
            get { return this.text; }
            set { this.text = value; }
        }
    }

    /// <summary>
    /// Notification that speech has been detected by the speech recognition engine
    /// </summary>
    [DataContract]
    public class SpeechDetectedNotification : SpeechInformation
    {
    }

    /// <summary>
    /// Notification that speech has been recognized by the speech recognition engine
    /// </summary>
    [DataContract]
    public class SpeechRecognizedNotification : SpeechAudioInformation
    {
        private float confidence;
        /// <summary>
        /// Measure of certainty for a recognized phrase returned by the recognition engine
        /// </summary>
        [DataMember]
        [Description("Measure of certainty for a recognized phrase returned by the recognition engine.")]
        public float Confidence
        {
            get { return this.confidence; }
            set { this.confidence = value; }
        }

        private string text;
        /// <summary>
        /// Normalized text obtained by the recognition engine from audio input
        /// </summary>
        [DataMember]
        [Description("Normalized text obtained by the recognition engine from audio input.")]
        public string Text
        {
            get { return this.text; }
            set { this.text = value; }
        }

        private RecognizedSemanticValue semantics;
        /// <summary>
        /// Semantics of the normalized text obtained by the recognition engine from audio input
        /// </summary>
        [DataMember]
        [Description("Semantics of the normalized text obtained by the recognition engine from audio input.")]
        public RecognizedSemanticValue Semantics
        {
            get { return this.semantics; }
            set { this.semantics = value; }
        }
    }

    /// <summary>
    /// Notification that speech has been rejected by the speech recognition engine
    /// </summary>
    [DataContract]
    public class SpeechRecognitionRejectedNotification : SpeechAudioInformation
    {
    }

    /// <summary>
    /// Notification that beam direction has changed
    /// </summary>
    [DataContract]
    public class BeamDirectionChangedNotification : SpeechInformation
    {
    }      

    /// <summary>
    /// Information on the audio input on which speech recognition was performed
    /// </summary>
    [DataContract]
    public class SpeechAudioInformation : SpeechInformation
    {
        private TimeSpan duration;

        /// <summary>
        /// Duration of speech
        /// </summary>
        internal TimeSpan Duration
        {
            get { return this.duration; }
            set { this.duration = value; }
        }

        /// <summary>
        /// Duration of speech in ticks. Workaround using ticks because
        /// System.TimeSpan itself cannot be XML serialized
        /// </summary>
        [DataMember]
        [Description("Duration of speech in ticks.")]
        public long DurationInTicks
        {
            get { return this.duration.Ticks; }
            set { this.duration = new TimeSpan(value); }
        }
    }


    /// <summary>
    /// Information on detected, recognized or rejected speech
    /// </summary>
    [DataContract]
    public class SpeechInformation
    {
        private DateTime startTime;

        /// <summary>
        /// Time at which speech started
        /// </summary>
        [DataMember]
        [Description("Time at which speech started.")]
        public DateTime StartTime
        {
            get { return this.startTime; }
            set { this.startTime = value; }
        }

        private AudioBeamInformation beamInfo = new AudioBeamInformation();

        /// <summary>
        /// Beam angle in radians
        /// </summary>
        [DataMember]
        [Description("Directional angle of the beam in radians")]
        public double Angle
        {
            get { return this.beamInfo.Angle; }
            set { this.beamInfo.Angle = value; }
        }
        
        /// <summary>
        /// Directional angle confidence
        /// </summary>
        [DataMember]
        [Description("Directional angle confidence (fuzzy boolean)")]
        public double DirectionConfidence
        {
            get { return this.beamInfo.Confidence; }
            set { this.beamInfo.Confidence = value; }
        }
    }
    

    /// <summary>
    /// Information on the beam that saw a direciton change
    /// </summary>
    [DataContract]
    public class AudioBeamInformation
    {
        private double angle;

        /// <summary>
        /// Beam angle in radians
        /// </summary>
        [DataMember]
        [Description("New directional angle of the beam in radians")]
        internal double Angle
        {
            get { return this.angle; }
            set { this.angle = value; }
        }

        private double confidence;

        /// <summary>
        /// Directional angle confidence
        /// </summary>
        [DataMember]
        [Description("Directional angle confidence (fuzzy boolean)")]
        internal double Confidence
        {
            get { return this.confidence; }
            set { this.confidence = value; }
        }
    }
}
