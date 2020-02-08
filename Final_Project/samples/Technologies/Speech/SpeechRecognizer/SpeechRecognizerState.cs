//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: SpeechRecognizerState.cs $ $Revision: 10 $
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using sr = System.Speech.Recognition;

using Microsoft.Dss.Core.Attributes;

namespace Microsoft.Robotics.Technologies.Speech.SpeechRecognizer
{
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
            _recognizer = new sr.SpeechRecognitionEngine();
            _dictionaryGrammar = new Dictionary<string, string>();
            _grammarType = GrammarType.DictionaryStyle;
        }

        private sr.SpeechRecognitionEngine _recognizer;
        /// <summary>
        /// Speech recognition engine
        /// </summary>
        internal sr.SpeechRecognitionEngine Recognizer
        {
            get { return _recognizer; }
            set { _recognizer = value; }
        }

        private bool _ignoreAudioInput;
        /// <summary>
        /// Whether input from the default audio input device shall be ignored
        /// </summary>
        [DataMember]
        public bool IgnoreAudioInput
        {
            get { return _ignoreAudioInput; }
            set { _ignoreAudioInput = value; }
        }

        private GrammarType _grammarType;
        /// <summary>
        /// Type of grammar the recognizer uses
        /// </summary>
        [DataMember]
        [Description("Specifies the type of grammar that is used by the recognizer.")]
        public GrammarType GrammarType
        {
            get { return _grammarType; }
            set { _grammarType = value; }
        }

        private Dictionary<string, string> _dictionaryGrammar;
        /// <summary>
        /// Dictionary entries of the dictionary-style grammar, where an entry's key is the
        /// recognizable phrase and its value is the phrase's meaning (semantic value).
        /// </summary>
        [DataMember]
        [Description("Dictionary entries of the dictionary-style grammar.")]
        public Dictionary<string, string> DictionaryGrammar
        {
            get { return _dictionaryGrammar; }
            set { _dictionaryGrammar = value; }
        }

        private string _srgsFileLocation;
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
            get { return _srgsFileLocation; }
            set { _srgsFileLocation = value; }
        }

        private sr.Grammar _grammar;
        /// <summary>
        /// The grammar object used in the recognition engine
        /// </summary>
        internal sr.Grammar Grammar
        {
            get { return _grammar; }
            set { _grammar = value; }
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
        private Dictionary<string, string> _dictionaryGrammar;
        /// <summary>
        /// Dictionary grammar that shall be used to modify the current dictionary
        /// grammar.
        /// </summary>
        [DataMember]
        [Description("Dictionary grammar that shall be used to modify the current dictionary grammar.")]
        public Dictionary<string, string> DictionaryGrammar
        {
            get { return _dictionaryGrammar; }
            set { _dictionaryGrammar = value; }
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

        private string _fileLocation;
        /// <summary>
        /// Location of the SRGS grammar file inside the DSS node's /store directory
        /// </summary>
        [DataMember]
        public String FileLocation
        {
            get { return _fileLocation; }
            set { _fileLocation = value; }
        }
    }

    /// <summary>
    /// Emulate recognize request
    /// </summary>
    [DataContract]
    public class EmulateRecognizeRequest
    {
        private string _text;
        /// <summary>
        /// Text of speech that shall be emulated
        /// </summary>
        [DataMember]
        public String Text
        {
            get { return _text; }
            set { _text = value; }
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
        private float _confidence;
        /// <summary>
        /// Measure of certainty for a recognized phrase returned by the recognition engine
        /// </summary>
        [DataMember]
        [Description("Measure of certainty for a recognized phrase returned by the recognition engine.")]
        public float Confidence
        {
            get { return _confidence; }
            set { _confidence = value; }
        }

        private string _text;
        /// <summary>
        /// Normalized text obtained by the recognition engine from audio input
        /// </summary>
        [DataMember]
        [Description("Normalized text obtained by the recognition engine from audio input.")]
        public string Text
        {
            get { return _text; }
            set { _text = value; }
        }

        private RecognizedSemanticValue _semantics;
        /// <summary>
        /// Semantics of the normalized text obtained by the recognition engine from audio input
        /// </summary>
        [DataMember]
        [Description("Semantics of the normalized text obtained by the recognition engine from audio input.")]
        public RecognizedSemanticValue Semantics
        {
            get { return _semantics; }
            set { _semantics = value; }
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
    /// Information on the audio input on which speech recognition was performed
    /// </summary>
    [DataContract]
    public class SpeechAudioInformation : SpeechInformation
    {
        private TimeSpan _duration;
        /// <summary>
        /// Duration of speech
        /// </summary>
        internal TimeSpan Duration
        {
            get { return _duration; }
            set { _duration = value; }
        }

        /// <summary>
        /// Duration of speech in ticks. Workaround using ticks because
        /// System.TimeSpan itself cannot be XML serialized
        /// </summary>
        [DataMember]
        [Description("Duration of speech in ticks.")]
        public long DurationInTicks
        {
            get { return _duration.Ticks; }
            set { _duration = new TimeSpan(value); }
        }
    }

    /// <summary>
    /// Information on detected, recognized or rejected speech
    /// </summary>
    [DataContract]
    public class SpeechInformation
    {
        private DateTime _startTime;
        /// <summary>
        /// Time at which speech started
        /// </summary>
        [DataMember]
        [Description("Time at which speech started.")]
        public DateTime StartTime
        {
            get { return _startTime; }
            set { _startTime = value; }
        }
    }
}
