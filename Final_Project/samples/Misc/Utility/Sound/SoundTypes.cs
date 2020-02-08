//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: SoundTypes.cs $ $Revision: 13 $
//-----------------------------------------------------------------------
#if !URT_MINCLR
using Microsoft.Ccr.Core;
using Microsoft.Dss.Core.Attributes;
using Microsoft.Dss.ServiceModel.Dssp;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using W3C.Soap;


namespace Microsoft.Robotics.Services.Sample.Sound
{
    /// <summary>
    /// Sound Service contract
    /// </summary>
    public static class Contract
    {
        /// The Unique Contract Identifier for the Sound service
        public const string Identifier = "http://schemas.microsoft.com/robotics/2006/08/sound.user.html";
    }

    /// <summary>
    ///  Sound Service Operations PortSet
    /// </summary>
    [ServicePort]
    public class SoundOperations : PortSet<DsspDefaultLookup, Play, Asterisk, Beep, Exclamation, Hand, Question>
    {
    }

    /// <summary>
    /// PlayRequest - Play a specified sound file
    /// </summary>
    [DataContract]
    public class PlayRequest
    {
        private string _filename;
        /// <summary>
        /// Filename - Name of the WAV (sound) file to play
        /// </summary>
        [DataMember]
        [Description("Specifies the filename of the sound file.")]
        public string Filename
        {
            get { return _filename; }
            set { _filename = value; }
        }

        private bool _synchronous;
        /// <summary>
        /// Synchronous - True means do not respond until finished
        /// </summary>
        [DataMember]
        [Description("Indicates whether to play the file synchronously (true).")]
        public bool Synchronous
        {
            get { return _synchronous; }
            set { _synchronous = value; }
        }
    }

    /// <summary>
    /// PlaySound - Operation
    /// </summary>
    [DisplayName("(User) PlaySound")]
    [Description("Plays the specified .wav file.")]
    public class Play : Submit<PlayRequest, PortSet<DefaultSubmitResponseType, Fault>>
    {
    }

    /// <summary>
    /// AsteriskRequest - Play the Windows "Asterisk" sound
    /// </summary>
    [DataContract]
    public class AsteriskRequest
    {
        private bool _synchronous;
        /// <summary>
        /// Synchronous - True means do not respond until finished
        /// </summary>
        [DataMember]
        [Description("Indicates whether to play the sound synchronously (true).")]
        public bool Synchronous
        {
            get { return _synchronous; }
            set { _synchronous = value; }
        }
    }

    /// <summary>
    /// Asterisk - Operation
    /// </summary>
    [DisplayName("(User) AsteriskSound")]
    [Description("Plays system sound 'Asterisk'.")]
    public class Asterisk : Submit<AsteriskRequest, PortSet<DefaultSubmitResponseType, Fault>>
    {
    }

    /// <summary>
    /// BeepRequest - Play a Beep
    /// </summary>
    [DataContract]
    public class BeepRequest
    {
        private bool _synchronous;
        /// <summary>
        /// Synchronous - True means do not respond until finished
        /// </summary>
        [DataMember]
        [Description("Indicates whether to play the sound synchronously (true).")]
        public bool Synchronous
        {
            get { return _synchronous; }
            set { _synchronous = value; }
        }
    }

    /// <summary>
    /// Beep - Operation
    /// </summary>
    [DisplayName("(User) Beep")]
    [Description("Plays system 'Beep' sound.")]
    public class Beep : Submit<BeepRequest, PortSet<DefaultSubmitResponseType, Fault>>
    {
    }

    /// <summary>
    /// ExclamationRequest - Play the Windows "Exclamation" sound
    /// </summary>
    [DataContract]
    public class ExclamationRequest
    {
        private bool _synchronous;
        /// <summary>
        /// Synchronous - True means do not respond until finished
        /// </summary>
        [DataMember]
        [Description("Indicates whether to play the sound synchronously (true).")]
        public bool Synchronous
        {
            get { return _synchronous; }
            set { _synchronous = value; }
        }
    }

    /// <summary>
    /// Exclamation - Operation
    /// </summary>
    [DisplayName("(User) ExclamationSound")]
    [Description("Plays system 'Exclamation' sound.")]
    public class Exclamation : Submit<ExclamationRequest, PortSet<DefaultSubmitResponseType, Fault>>
    {
    }

    /// <summary>
    /// HandRequest - Play the Windows "Hand" sound
    /// </summary>
    [DataContract]
    public class HandRequest
    {
        private bool _synchronous;
        /// <summary>
        /// Synchronous - True means do not respond until finished
        /// </summary>
        [DataMember]
        [Description("Indicates whether to play the sound synchronously (true).")]
        public bool Synchronous
        {
            get { return _synchronous; }
            set { _synchronous = value; }
        }
    }

    /// <summary>
    /// Hand - Operation
    /// </summary>
    [DisplayName("(User) CriticalStopSound")]
    [Description("Plays system 'Critical Stop' sound.")]
    public class Hand : Submit<HandRequest, PortSet<DefaultSubmitResponseType, Fault>>
    {
    }

    /// <summary>
    /// QuestionRequest - Play the Windows "Question" sound
    /// </summary>
    [DataContract]
    public class QuestionRequest
    {
        private bool _synchronous;
        /// <summary>
        /// Synchronous - True means do not respond until finished
        /// </summary>
        [DataMember]
        [Description("Indicates whether to play the sound synchronously (true).")]
        public bool Synchronous
        {
            get { return _synchronous; }
            set { _synchronous = value; }
        }
    }

    /// <summary>
    /// Question - Operation
    /// </summary>
    [DisplayName("(User) QuestionSound")]
    [Description("Plays system 'Question' sound.")]
    public class Question : Submit<QuestionRequest, PortSet<DefaultSubmitResponseType, Fault>>
    {
    }

}
#endif