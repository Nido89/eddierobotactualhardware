//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: TextToSpeechTypes.cs $ $Revision: 13 $
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel;

using Microsoft.Ccr.Core;
using Microsoft.Dss.Core.Attributes;
using Microsoft.Dss.Core.DsspHttp;
using Microsoft.Dss.ServiceModel.Dssp;

using W3C.Soap;


namespace Microsoft.Robotics.Technologies.Speech.TextToSpeech
{
    /// <summary>
    /// The text to speech contract definition
    /// </summary>
    public static class Contract
    {
        /// <summary>
        /// the text to speech contract
        /// </summary>
        public const string Identifier = "http://schemas.microsoft.com/2006/05/texttospeech.user.html";
    }

    /// <summary>
    /// the text to speech operations port
    /// </summary>
    [ServicePort]
    public class SpeechTextOperations : PortSet<
        DsspDefaultLookup,
        DsspDefaultDrop,
        Get,
        Replace,
        SayText,
        SayTextSynch,
        SetRate,
        SetVolume,
        SetVoice,
        HttpGet,
        HttpPost,
        Subscribe,
        VisemeNotify,
        FaultNotify>
    { }

    /// <summary>
    /// returns the text to speech state
    /// </summary>
    [DisplayName("(User) Get")]
    [Description("Gets the current state of the text-to-speech service.")]
    public class Get : Get<GetRequestType, PortSet<TextToSpeechState, Fault>>
    {
    }

    /// <summary>
    /// Changes all writable text to speech settings
    /// </summary>
    [DisplayName("(User) Replace")]
    [Description("Sets the current state of the text-to-speech service.\nThis can be used to set the rate, volume, voice and text to say all in one message.")]
    public class Replace : Replace<TextToSpeechState, PortSet<DefaultReplaceResponseType, Fault>>
    {
    }

    /// <summary>
    /// Will say the text asynchronously
    /// </summary>
    [DisplayName("(User) SayText")]
    [Description("Sets the text to be spoken asynchronously.")]
    public class SayText : Update<SayTextRequest, PortSet<DefaultUpdateResponseType, Fault>>
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public SayText()
        {
        }

        /// <summary>
        /// Constructor to initialize request
        /// </summary>
        /// <param name="body"></param>
        public SayText(SayTextRequest body)
            : base(body)
        {
        }
    }

    /// <summary>
    /// Will say the text (blocking)
    /// </summary>
    [DisplayName("(User) SayTextSynchronous")]
    [Description("Sets the text to be spoken synchronously.")]
    public class SayTextSynch : Update<SayTextSynchRequest, PortSet<DefaultUpdateResponseType, Fault>>
    {
    }

    /// <summary>
    /// Set the speech rate
    /// </summary>
    [DisplayName("(User) SetRate")]
    [Description("Sets the rate at which the text is spoken.\nAcceptable values are between -10 and 10.")]
    public class SetRate : Update<SetRateRequest, PortSet<DefaultUpdateResponseType, Fault>>
    {
    }

    /// <summary>
    /// Set the speech volume
    /// </summary>
    [DisplayName("(User) SetVolume")]
    [Description("Sets the volume at which the text is spoken.\nAcceptable values are between 0 and 100.")]
    public class SetVolume : Update<SetVolumeRequest, PortSet<DefaultUpdateResponseType, Fault>>
    {
    }

    /// <summary>
    /// Set the speech voice
    /// </summary>
    [DisplayName("(User) SetVoice")]
    [Description("Sets the voice which is used to say the text.")]
    public class SetVoice : Update<SetVoiceRequest, PortSet<DefaultUpdateResponseType, Fault>>
    {
        /// <summary>
        /// Default Constructor
        /// </summary>
        public SetVoice()
        {
        }

        /// <summary>
        /// Constructor to initialize request
        /// </summary>
        /// <param name="body"></param>
        public SetVoice(SetVoiceRequest body)
            : base(body)
        {
        }
    }

    /// <summary>
    /// Subscribe operation
    /// </summary>
    [DisplayName("(User) Subscribe")]
    [Description("Subscribe operation.")]
    public class Subscribe : Subscribe<SubscribeRequestType, PortSet<SubscribeResponseType, Fault>>
    {
    }

    /// <summary>
    /// Viseme notificaiton
    /// </summary>
    [DisplayName("(User) VisemeNotify")]
    [Description("Viseme notification.")]
    public class VisemeNotify : Update<VisemeNotification, PortSet<DefaultUpdateResponseType, Fault>>
    {
    }

    /// <summary>
    /// Fault notificaiton
    /// </summary>
    [DisplayName("(User) FaultNotify")]
    [Description("Fault notification.")]
    public class FaultNotify : Update<Fault, PortSet<DefaultUpdateResponseType, Fault>>
    {
    }
}
