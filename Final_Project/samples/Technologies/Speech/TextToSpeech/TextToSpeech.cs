//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: TextToSpeech.cs $ $Revision: 26 $
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Net;
using System.Security.Permissions;
using System.Speech.Synthesis;
using W3C.Soap;

using Microsoft.Ccr.Core;
using Microsoft.Dss.Core;
using Microsoft.Dss.Core.Attributes;
using Microsoft.Dss.Core.DsspHttp;
using Microsoft.Dss.Core.DsspHttpUtilities;
using Microsoft.Dss.ServiceModel.Dssp;
using Microsoft.Dss.ServiceModel.DsspServiceBase;

using submgr = Microsoft.Dss.Services.SubscriptionManager;

namespace Microsoft.Robotics.Technologies.Speech.TextToSpeech
{
    /// <summary>
    /// Main text to speech implementation class
    /// </summary>
    [DisplayName("(User) Text to Speech (TTS)")]
    [Description("Converts text to speech using the .NET speech synthesis framework.")]
    [Contract(Contract.Identifier)]
    [DssServiceDescription("http://msdn.microsoft.com/library/dd145258.aspx")]
    public class TextToSpeech : DsspServiceBase
    {
        /// <summary>
        /// Service state
        /// </summary>
        // Note that the transform is declared here on the State
        [ServiceState(StateTransform = "Microsoft.Robotics.Technologies.Speech.TextToSpeech.TextToSpeech.user.xslt")]
        [InitialStatePartner(Optional = true, ServiceUri = ServicePaths.Store + "/TextToSpeech.config.xml")]
        TextToSpeechState _state = new TextToSpeechState();

        /// <summary>
        /// Main port
        /// </summary>
        [ServicePort("/TextToSpeech")]
        SpeechTextOperations _mainPort = new SpeechTextOperations();

        /// <summary>
        /// Synchronous speech synthesis port
        /// </summary>
        Port<SayTextSynch> _sayTextSynchPort = new Port<SayTextSynch>();

        /// <summary>
        /// Subscription manager port
        /// </summary>
        [Partner("SubMgr", Contract = submgr.Contract.Identifier, CreationPolicy = PartnerCreationPolicy.CreateAlways)]
        submgr.SubscriptionManagerPort _subMgrPort = new submgr.SubscriptionManagerPort();

        /// <summary>
        /// Needed for HTTP post processing
        /// </summary>
        DsspHttpUtilitiesPort _utilities = new DsspHttpUtilitiesPort();

        Dictionary<Prompt, SayTextSynch> _sayTextSynchDictionary = new Dictionary<Prompt,SayTextSynch>();

        /// <summary>
        /// Default constructor
        /// </summary>
        public TextToSpeech(DsspServiceCreationPort creationPort) :
                base(creationPort)
        {
        }

        /// <summary>
        /// The startup method for the text to speech service
        /// </summary>
        protected override void Start()
        {
            if (_state == null)
            {
                _state = new TextToSpeechState();
                _state.Volume = TextToSpeechState.DefaultVolume;
            }

            InitTextToSpeech();

            _utilities = DsspHttpUtilitiesService.Create(Environment);

            // Set up internal port
            Activate(
                Arbiter.Receive<SayTextSynch>(true, _sayTextSynchPort, InternalSayTextSynchHandler)
            );

            base.Start();
        }

        /// <summary>
        /// Initializes the text to speech service
        /// </summary>
        void InitTextToSpeech()
        {
            // Create speech synthesizer
            _state.Synthesizer = new SpeechSynthesizer();

            // Load list of installed and enabled voices
            _state.Voices = new List<string>();
            foreach (InstalledVoice voice in _state.Synthesizer.GetInstalledVoices())
            {
                if (voice.Enabled)
                {
                    _state.Voices.Add(voice.VoiceInfo.Name);
                }
            }

            // Set voice
            try
            {
                SetVoice(_state.Voice);
            }
            catch (Exception)
            {
                if (_state.Voices.Count > 0)
                {
                    // Default voice
                    SetVoice(_state.Voices[0]);
                }
                else
                {
                    // No voice installed
                    _state.Voice = null;
                }
            }

            // Set volume
            try
            {
                SetVolume(_state.Volume);
            }
            catch (Exception)
            {
                // Default volume
                SetVolume(TextToSpeechState.DefaultVolume);
            }

            // Set rate
            try
            {
                SetRate(_state.Rate);
            }
            catch (Exception)
            {
                // Default rate
                SetRate(0);
            }

            if (_state.DisableAudioOutput)
            {
                _state.Synthesizer.SetOutputToNull();
            }

            // Register synthesizer events
            _state.Synthesizer.VisemeReached += VisemeReachedHandler;
            _state.Synthesizer.SpeakCompleted += SpeakCompletedHandler;
        }

        /// <summary>
        /// Drop handler
        /// </summary>
        /// <param name="drop"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Teardown)]
        public IEnumerator<ITask> DropHandler(DsspDefaultDrop drop)
        {
            base.DefaultDropHandler(drop);

            _state.Synthesizer.Dispose();
            yield break;
        }

        /// <summary>
        /// Handles the DSSP Replace message.
        /// </summary>
        /// <param name="replace">The new settings for the service</param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public IEnumerator<ITask> ReplaceHandler(Replace replace)
        {
            // Use try/catch to respond with meaningful error in case of an exception
            try
            {
                SetRate(replace.Body.Rate);
                SetVolume(replace.Body.Volume);
                SetVoice(replace.Body.Voice);
            }
            catch (ArgumentException e)
            {
                replace.ResponsePort.Post(
                    Fault.FromCodeSubcodeReason(
                        W3C.Soap.FaultCodes.Receiver,
                        DsspFaultCodes.OperationFailed,
                        e.Message
                    )
                );
                yield break;
            }

            DisableAudioOutput(replace.Body.DisableAudioOutput);

            SayText say = new SayText();
            say.Body.SpeechText = replace.Body.SpeechText;
            SpawnIterator(say, SayTextHandler);

            yield return (Choice)say.ResponsePort;
            Fault fault = (Fault)say.ResponsePort;
            if (fault != null)
            {
                replace.ResponsePort.Post(fault);
                yield break;
            }

            SaveState(_state);

            // Notify subscribers of state change
            Replace subscriberReplace = new Replace();
            subscriberReplace.Body = _state;
            SendNotification<Replace>(_subMgrPort, subscriberReplace);

            replace.ResponsePort.Post(DefaultReplaceResponseType.Instance);
        }

        /// <summary>
        /// Say the text
        /// </summary>
        /// <param name="sayText">The SayText message</param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public IEnumerator<ITask> SayTextHandler(SayText sayText)
        {
            _state.SpeechText = sayText.Body.SpeechText;
            _state.Synthesizer.SpeakAsync(_state.SpeechText);

            sayText.ResponsePort.Post(DefaultUpdateResponseType.Instance);
            yield break;
        }

        /// <summary>
        /// Say the text synchronously
        /// </summary>
        /// <param name="sayText">The SayTextSynch message</param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public IEnumerator<ITask> SayTextSynchHandler(SayTextSynch sayText)
        {
            _state.SpeechText = sayText.Body.SpeechText;

            // Post synchronous request into non-exclusive port, thus
            // making sure the service is not locked while text is being spoken
            _sayTextSynchPort.Post(sayText);
            yield break;
        }

        /// <summary>
        /// Handles the <see cref="SetRate"/> message.
        /// Sets the speech rate.
        /// </summary>
        /// <param name="setRate">The speech rate to set</param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public IEnumerator<ITask> SetRateHandler(SetRate setRate)
        {
            // Use try/catch to respond with meaningful error in case of an exception
            try
            {
                SetRate(setRate.Body.Rate);
            }
            catch (ArgumentException e)
            {
                setRate.ResponsePort.Post(
                    Fault.FromCodeSubcodeReason(
                        W3C.Soap.FaultCodes.Receiver,
                        DsspFaultCodes.OperationFailed,
                        e.Message
                    )
                );
                yield break;
            }

            SaveState(_state);

            // Notify subscribers of state change
            Replace replace = new Replace();
            replace.Body = _state;
            SendNotification<Replace>(_subMgrPort, replace);

            setRate.ResponsePort.Post(DefaultUpdateResponseType.Instance);
            yield break;
        }

        /// <summary>
        /// Handles the <see cref="SetVolume"/> message.
        /// Sets the speech volume.
        /// </summary>
        /// <param name="setVolume">The volume to set</param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public IEnumerator<ITask> SetVolumeHandler(SetVolume setVolume)
        {
            // Use try/catch to respond with meaningful error in case of an exception
            try
            {
                SetVolume(setVolume.Body.Volume);
            }
            catch (ArgumentException e)
            {
                setVolume.ResponsePort.Post(
                    Fault.FromCodeSubcodeReason(
                        W3C.Soap.FaultCodes.Receiver,
                        DsspFaultCodes.OperationFailed,
                        e.Message
                    )
                );
                yield break;
            }

            SaveState(_state);

            // Notify subscribers of state change
            Replace replace = new Replace();
            replace.Body = _state;
            SendNotification<Replace>(_subMgrPort, replace);

            setVolume.ResponsePort.Post(DefaultUpdateResponseType.Instance);
            yield break;
        }

        /// <summary>
        /// Handles the <see cref="SetVoice"/> message.
        /// Sets the voice to use for speech.
        /// </summary>
        /// <param name="setVoice"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public IEnumerator<ITask> SetVoiceHandler(SetVoice setVoice)
        {
            // Use try/catch to respond with meaningful error in case of an exception
            try
            {
                SetVoice(setVoice.Body.Voice);
            }
            catch (ArgumentException e)
            {
                setVoice.ResponsePort.Post(
                    Fault.FromCodeSubcodeReason(
                        W3C.Soap.FaultCodes.Receiver,
                        DsspFaultCodes.OperationFailed,
                        e.Message
                    )
                );
                yield break;
            }

            SaveState(_state);

            // Notify subscribers of state change
            Replace replace = new Replace();
            replace.Body = _state;
            SendNotification<Replace>(_subMgrPort, replace);

            setVoice.ResponsePort.Post(DefaultUpdateResponseType.Instance);
            yield break;
        }

        /// <summary>
        /// Handles the <see cref="HttpPost"/> message.
        /// </summary>
        /// <param name="httpPost"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public IEnumerator<ITask> HttpPostHandler(HttpPost httpPost)
        {
            ReadFormData readForm = new ReadFormData(httpPost);
            _utilities.Post(readForm);

            PortSet<NameValueCollection, Exception> readFormResponsePort = readForm.ResultPort;
            yield return (Choice)readFormResponsePort;
            Exception exception = (Exception)readFormResponsePort;
            if (exception != null)
            {
                LogError(exception);
                httpPost.ResponsePort.Post(Fault.FromException(exception));
                yield break;
            }

            NameValueCollection collection = (NameValueCollection)readFormResponsePort;
            string voice = collection["Voice"];
            string rate = collection["Rate"];
            string volume = collection["Volume"];
            string speechText = collection["SpeechText"];

            PortSet<DefaultUpdateResponseType, Fault> responsePort = null;

            if (!string.IsNullOrEmpty(voice))
            {
                SetVoice setVoice = new SetVoice(new SetVoiceRequest(voice));
                SpawnIterator(setVoice, SetVoiceHandler);

                responsePort = setVoice.ResponsePort;
            }
            else if (!string.IsNullOrEmpty(rate))
            {
                int rateInt;
                if (int.TryParse(rate, out rateInt))
                {
                    SetRate setRate = new SetRate();
                    setRate.Body.Rate = rateInt;
                    SpawnIterator(setRate, SetRateHandler);

                    responsePort = setRate.ResponsePort;
                }
            }
            else if (!string.IsNullOrEmpty(volume))
            {
                int volumeInt;

                if (int.TryParse(volume, out volumeInt))
                {
                    SetVolume setVolume = new SetVolume();
                    setVolume.Body.Volume = volumeInt;
                    SpawnIterator(setVolume, SetVolumeHandler);

                    responsePort = setVolume.ResponsePort;
                }
            }
            else if (!string.IsNullOrEmpty(speechText))
            {
                SayText sayText = new SayText(new SayTextRequest(speechText));
                SpawnIterator(sayText, SayTextHandler);

                responsePort = sayText.ResponsePort;
            }

            if (responsePort != null)
            {
                yield return (Choice)responsePort;
                Fault fault = (Fault)responsePort;
                if (fault != null)
                {
                    LogError("Unable to perform post operation", fault);
                    httpPost.ResponsePort.Post(fault);
                    yield break;
                }
            }

            httpPost.ResponsePort.Post(new HttpResponseType(
                HttpStatusCode.OK,
                _state,
                base.StateTransformPath     // Use the transform declared on the State
            ));
            yield break;
        }

        /// <summary>
        /// Subscription handler
        /// </summary>
        /// <param name="subscribe">Subscription message</param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public IEnumerator<ITask> SubscribeHandler(Subscribe subscribe)
        {
            SubscribeRequestType request = subscribe.Body;
            SuccessFailurePort subscribePort = SubscribeHelper(
                _subMgrPort,
                request,
                subscribe.ResponsePort
            );

            yield return (Choice)subscribePort;
            Exception exception = (Exception)subscribePort;
            if (exception != null)
            {
                LogError("Subscribe failed", exception);
                yield break;
            }

            SendNotificationToTarget<Replace>(request.Subscriber, _subMgrPort, _state);
            yield break;
        }

        /// <summary>
        /// Viseme notify operation (not supported, just used for notifications to
        /// subscribers)
        /// </summary>
        /// <param name="notify"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public IEnumerator<ITask> VisemeNotifyHandler(VisemeNotify notify)
        {
            PostActionNotSupported(notify);
            yield break;
        }

        /// <summary>
        /// Synchronous speak handler
        /// </summary>
        /// <param name="sayText"></param>
        void InternalSayTextSynchHandler(SayTextSynch sayText)
        {
            // Speek text asynchronously but add SayTextSynch request to dictionary
            // such that the SpeakCompletedHandler callback can post into the response
            // port on completion
            Prompt prompt = _state.Synthesizer.SpeakAsync(sayText.Body.SpeechText);
            lock (_sayTextSynchDictionary)
            {
                _sayTextSynchDictionary.Add(prompt, sayText);
            }
        }

        /// <summary>
        /// VisemeReached handler, called by speech synthesizer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void VisemeReachedHandler(object sender, VisemeReachedEventArgs args)
        {
            if (args.Error != null || args.Cancelled)
            {
                return;
            }

            VisemeNotification notification = new VisemeNotification();
            notification.AudioPosition = args.AudioPosition;
            notification.Duration = args.Duration;
            notification.Emphasis = args.Emphasis;
            notification.NextViseme = args.NextViseme;
            notification.Viseme = args.Viseme;

            VisemeNotify notify = new VisemeNotify();
            notify.Body = notification;

            SendNotification<VisemeNotify>(_subMgrPort, notify);
        }

        /// <summary>
        /// SeakCompleted handler, called by speech synthesizer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void SpeakCompletedHandler(object sender, SpeakCompletedEventArgs args)
        {
            Fault fault = null;
            if (args.Error != null)
            {
                string errorStr = "Asynchronous speech synthesis failed: "
                    + args.Error.ToString();

                LogWarning(LogGroups.Console, errorStr);

                fault = Fault.FromCodeSubcodeReason(
                    FaultCodes.Receiver,
                    DsspFaultCodes.OperationFailed,
                    errorStr
                );
            }

            SayTextSynch sayText;
            lock (_sayTextSynchDictionary)
            {
                if (_sayTextSynchDictionary.TryGetValue(args.Prompt, out sayText))
                {
                    _sayTextSynchDictionary.Remove(args.Prompt);

                    if (fault != null)
                    {
                        sayText.ResponsePort.Post(fault);
                    }
                    else
                    {
                        sayText.ResponsePort.Post(DefaultUpdateResponseType.Instance);
                    }
                }
                else
                {
                    if (fault != null)
                    {
                        FaultNotify notifyMsg = new FaultNotify();
                        notifyMsg.Body = fault;
                        SendNotification<FaultNotify>(_subMgrPort, notifyMsg);
                    }
                }
            }
        }

        /// <summary>
        /// Posts an ActionNotSupported DSSP fault code to the response port of the passed in
        /// DSSP operation
        /// </summary>
        /// <param name="operation"></param>
        static void PostActionNotSupported(DsspOperation operation)
        {
            operation.ResponsePort.TryPostUnknownType(Fault.FromCodeSubcode(
                W3C.Soap.FaultCodes.Sender,
                DsspFaultCodes.ActionNotSupported)
            );
        }

        /// <summary>
        /// Sets the rate if it is in a valid range. Throws exception otherwise.
        /// </summary>
        /// <param name="rate">The rate to set</param>
        void SetRate(int rate)
        {
            if (rate < -10 || rate > 10)
            {
                throw new ArgumentException("Rate out of range; must be between -10 and 10");
            }

            _state.Synthesizer.Rate = rate;
            _state.Rate = rate;
        }

        /// <summary>
        /// Sets the volume if it is in a valid range. Throws exception otherwise.
        /// </summary>
        /// <param name="volume">The volume to set</param>
        void SetVolume(int volume)
        {
            if (volume < 0 || volume > 100)
            {
                throw new ArgumentException("Volume out of range; must be between 0 and 100");
            }

            _state.Synthesizer.Volume = volume;
            _state.Volume = volume;
        }

        /// <summary>
        /// Sets the voice if it is installed and enabled. Throws exception otherwise.
        /// </summary>
        /// <param name="voice">The voice to set</param>
        void SetVoice(string voice)
        {
            if (!_state.Voices.Contains(voice))
            {
                throw new ArgumentException("Voice not installed or enabled");
            }

            _state.Synthesizer.SelectVoice(voice);
            _state.Voice = voice;
        }

        /// <summary>
        /// Enables or disables audio output
        /// </summary>
        /// <param name="disable">Whether to disable audio output or not</param>
        void DisableAudioOutput(bool disable)
        {
            if (disable != _state.DisableAudioOutput)
            {
                if (disable)
                {
                    _state.Synthesizer.SetOutputToNull();
                }
                else
                {
                    _state.Synthesizer.SetOutputToDefaultAudioDevice();
                }

                _state.DisableAudioOutput = disable;
            }
        }
    }
}
