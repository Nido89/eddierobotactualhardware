//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: SpeechRecognizer.cs $ $Revision: 15 $
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Speech.Recognition;
using System.Xml;
using W3C.Soap;

using Microsoft.Ccr.Core;
using Microsoft.Dss.Core;
using Microsoft.Dss.Core.Attributes;
using Microsoft.Dss.Core.DsspHttp;
using Microsoft.Dss.ServiceModel.Dssp;
using Microsoft.Dss.ServiceModel.DsspServiceBase;
using mnt = Microsoft.Dss.Services.MountService;
using submgr = Microsoft.Dss.Services.SubscriptionManager;

namespace Microsoft.Robotics.Technologies.Speech.SpeechRecognizer
{
    
    /// <summary>
    /// SpeechRecognizer service - Recognizes spoken commands from a defined grammar
    /// </summary>
    [DisplayName("(User) SpeechRecognizer")]
    [Description("Recognizes speech, using the .NET speech recognition framework,"
        + " and turns it into text and/or semantic values.")]
    [Contract(Contract.Identifier)]
    [DssServiceDescription("http://msdn.microsoft.com/library/dd145257.aspx")]
    public class SpeechRecognizer : DsspServiceBase
    {
        
        /// <summary>
        /// Speech recognition service state
        /// </summary>
        [ServiceState]
        [InitialStatePartner(Optional = true, ServiceUri = ServicePaths.Store
            + "/SpeechRecognizer.user.config.xml")]
        private SpeechRecognizerState _state = new SpeechRecognizerState();
        
        /// <summary>
        /// The service's main port
        /// </summary>
        [ServicePort("/speechrecognizer", AllowMultipleInstances = false)]
        private SpeechRecognizerOperations _mainPort = new SpeechRecognizerOperations();

        /// <summary>
        /// Subscription manager port for handling speech recognition event subscriptions
        /// </summary>
        [Partner("SubMgr", Contract = submgr.Contract.Identifier, CreationPolicy = PartnerCreationPolicy.CreateAlways)]
        private submgr.SubscriptionManagerPort _subMgrPort = new submgr.SubscriptionManagerPort();

        /// <summary>
        /// Block size with which files are read from the mount service
        /// </summary>
        private const int MountServiceReadBlockSize = 32768;

        /// <summary>
        /// Success/failure port LoadGrammar waits on and the speech recognition engine
        /// callback posts to
        /// </summary>
        private SuccessFailurePort _loadGrammarResponsePort = new SuccessFailurePort();
        private Grammar _grammarToLoad;

        /// <summary>
        /// Default service constructor
        /// </summary>
        public SpeechRecognizer(DsspServiceCreationPort creationPort) : 
                base(creationPort)
        {
        }
        
        /// <summary>
        /// Starts service
        /// </summary>
        protected override void Start()
        {
            try
            {
                if (_state == null)
                {
                    _state = new SpeechRecognizerState();
                }

                // Set up speech recognition engine
                if (_state.IgnoreAudioInput)
                {
                    _state.Recognizer.SetInputToNull();
                }
                else
                {
                    _state.Recognizer.SetInputToDefaultAudioDevice();
                }

                // Register event handlers for speech recognition
                _state.Recognizer.RecognizerUpdateReached += RecognizerUpdateReachedHandler;
                _state.Recognizer.LoadGrammarCompleted += LoadGrammarCompletedHandler;
                _state.Recognizer.SpeechDetected += SpeechDetectedHandler;
                _state.Recognizer.SpeechRecognized += SpeechRecognizedHandler;
                _state.Recognizer.SpeechRecognitionRejected += SpeechRecognitionRejectedHandler;
            }
            catch (Exception exception)
            {
                // Fatal exception during startup, shutdown service
                LogError(LogGroups.Activation, exception);
                _state.Recognizer.Dispose();
                DefaultDropHandler(new DsspDefaultDrop());
                return;
            }

            SpawnIterator(LoadGrammarOnStartup);
        }

        /// <summary>
        /// Loads grammar on service startup
        /// </summary>
        /// <returns></returns>
        private IEnumerator<ITask> LoadGrammarOnStartup()
        {
            // Determine whether we have a valid grammar to load
            bool loadGrammar = false;
            switch (_state.GrammarType)
            {
                case GrammarType.DictionaryStyle:
                    loadGrammar = _state.DictionaryGrammar.Count > 0;
                    break;
                case GrammarType.Srgs:
                    loadGrammar = !string.IsNullOrEmpty(_state.SrgsFileLocation);
                    break;
            }

            // Load grammar
            if (loadGrammar)
            {
                SuccessFailurePort loadGrammarPort = new SuccessFailurePort();
                LoadGrammarRequest loadRequest = new LoadGrammarRequest();
                loadRequest.GrammarType = _state.GrammarType;
                loadRequest.DictionaryGrammar = _state.DictionaryGrammar;
                loadRequest.SrgsFileLocation = _state.SrgsFileLocation;
                SpawnIterator<LoadGrammarRequest, SuccessFailurePort>(
                    loadRequest,
                    loadGrammarPort,
                    LoadGrammar
                );

                yield return (Choice)loadGrammarPort;
                Exception exception = (Exception)loadGrammarPort;
                if (exception != null)
                {
                    LogWarning(exception);
                }
            }

            StartService();
            yield break;
        }

        /// <summary>
        /// Wrapper around base.Start() call; needed because base.Start() cannot
        /// be called from inside an iterator
        /// </summary>
        private void StartService()
        {
            base.Start();
        }

        /// <summary>
        /// Drop handler
        /// </summary>
        /// <param name="drop"></param>
        /// <returns></returns>
        public IEnumerator<ITask> DropHandler(DsspDefaultDrop drop)
        {
            base.DefaultDropHandler(drop);
            // Stop & dispose recognition engine
            _state.Recognizer.Dispose();
            yield break;
        }

        /// <summary>
        /// Insert grammar entry handler
        /// </summary>
        /// <param name="insert"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public IEnumerator<ITask> InsertGrammarEntryHandler(InsertGrammarEntry insert)
        {
            if (_state.GrammarType != GrammarType.DictionaryStyle)
            {
                // Since we are switching grammar mode make sure there exists a valid,
                // empty dictionary
                if (_state.DictionaryGrammar == null)
                {
                    _state.DictionaryGrammar = new Dictionary<string, string>();
                }
                else
                {
                    _state.DictionaryGrammar.Clear();
                }
            }

            #region Set up load grammar request and load grammar
            // Set up load grammar request
            LoadGrammarRequest loadRequest = new LoadGrammarRequest();
            loadRequest.GrammarType = GrammarType.DictionaryStyle;
            loadRequest.DictionaryGrammar = new Dictionary<string,string>(_state.DictionaryGrammar);
            try
            {
                GrammarUtilities.InsertDictionary(loadRequest.DictionaryGrammar, insert.Body.DictionaryGrammar);
            }
            catch (Exception ex)
            {
                LogInfo(ex);
                insert.ResponsePort.Post(Fault.FromCodeSubcodeReason(
                    FaultCodes.Receiver,
                    DsspFaultCodes.OperationFailed,
                    ex.Message
                ));
                yield break;
            }

            // Load new grammar
            SuccessFailurePort loadGrammarPort = new SuccessFailurePort();
            SpawnIterator<LoadGrammarRequest, SuccessFailurePort>(
                loadRequest,
                loadGrammarPort,
                LoadGrammar
            );

            // Check loading outcome
            yield return (Choice)loadGrammarPort;
            Exception exception = (Exception)loadGrammarPort;
            if (exception != null)
            {
                LogWarning(exception);
                insert.ResponsePort.Post(Fault.FromCodeSubcodeReason(
                    W3C.Soap.FaultCodes.Receiver,
                    DsspFaultCodes.OperationFailed,
                    exception.Message
                ));
                yield break;
            }
            #endregion

            SaveState(_state);

            // Notify subscribers of state change
            Replace replace = new Replace();
            replace.Body = _state;
            SendNotification<Replace>(_subMgrPort, replace);

            insert.ResponsePort.Post(DefaultInsertResponseType.Instance);
        }

        /// <summary>
        /// Update handler
        /// </summary>
        /// <param name="update"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public IEnumerator<ITask> UpdateGrammarEntryHandler(UpdateGrammarEntry update)
        {
            // Make sure current grammar type is dictionary-style
            if (_state.GrammarType != GrammarType.DictionaryStyle)
            {
                Fault fault = Fault.FromCodeSubcodeReason(
                    FaultCodes.Receiver,
                    DsspFaultCodes.OperationFailed,
                    "Cannot update grammar dictionary because grammar"
                    + " currently in use is not DictionaryStyle."
                );
                LogInfo(fault.ToException());
                update.ResponsePort.Post(fault);
                yield break;
            }

            #region Set up load grammar request and load grammar
            // Set up load grammar request
            LoadGrammarRequest loadRequest = new LoadGrammarRequest();
            loadRequest.GrammarType = GrammarType.DictionaryStyle;
            loadRequest.DictionaryGrammar = new Dictionary<string,string>(_state.DictionaryGrammar);
            GrammarUtilities.UpdateDictionary(loadRequest.DictionaryGrammar, update.Body.DictionaryGrammar);

            // Load new grammar
            SuccessFailurePort loadGrammarPort = new SuccessFailurePort();
            SpawnIterator<LoadGrammarRequest, SuccessFailurePort>(
                loadRequest,
                loadGrammarPort,
                LoadGrammar
            );

            // Check loading outcome
            yield return (Choice)loadGrammarPort;
            Exception exception = (Exception)loadGrammarPort;
            if (exception != null)
            {
                LogWarning(exception);
                update.ResponsePort.Post(Fault.FromCodeSubcodeReason(
                    FaultCodes.Receiver,
                    DsspFaultCodes.OperationFailed,
                    exception.Message
                ));
                yield break;
            }
            #endregion

            SaveState(_state);

            // Notify subscribers of state change
            Replace replace = new Replace();
            replace.Body = _state;
            SendNotification<Replace>(_subMgrPort, replace);

            update.ResponsePort.Post(DefaultUpdateResponseType.Instance);
        }

        /// <summary>
        /// Upsert handler
        /// </summary>
        /// <param name="upsert"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public IEnumerator<ITask> UpsertGrammarEntryHandler(UpsertGrammarEntry upsert)
        {
            if (_state.GrammarType != GrammarType.DictionaryStyle)
            {
                // Since we are switching grammar mode make sure there exists a valid,
                // empty dictionary
                if (_state.DictionaryGrammar == null)
                {
                    _state.DictionaryGrammar = new Dictionary<string, string>();
                }
                else
                {
                    _state.DictionaryGrammar.Clear();
                }
            }

            #region Set up load grammar request and load grammar
            // Set up load grammar request
            LoadGrammarRequest loadRequest = new LoadGrammarRequest();
            loadRequest.GrammarType = GrammarType.DictionaryStyle;
            loadRequest.DictionaryGrammar = new Dictionary<string,string>(_state.DictionaryGrammar);
            GrammarUtilities.UpsertDictionary(loadRequest.DictionaryGrammar, upsert.Body.DictionaryGrammar);

            // Load new grammar
            SuccessFailurePort loadGrammarPort = new SuccessFailurePort();
            SpawnIterator<LoadGrammarRequest, SuccessFailurePort>(
                loadRequest,
                loadGrammarPort,
                LoadGrammar
            );

            // Check loading outcome
            yield return (Choice)loadGrammarPort;
            Exception exception = (Exception)loadGrammarPort;
            if (exception != null)
            {
                LogWarning(exception);
                upsert.ResponsePort.Post(Fault.FromCodeSubcodeReason(
                    FaultCodes.Receiver,
                    DsspFaultCodes.OperationFailed,
                    exception.Message
                ));
                yield break;
            }
            #endregion

            SaveState(_state);

            // Notify subscribers of state change
            Replace replace = new Replace();
            replace.Body = _state;
            SendNotification<Replace>(_subMgrPort, replace);

            upsert.ResponsePort.Post(DefaultUpsertResponseType.Instance);
        }

        /// <summary>
        /// Delete handler
        /// </summary>
        /// <param name="delete"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public IEnumerator<ITask> DeleteGrammarEntryHandler(DeleteGrammarEntry delete)
        {
            // Make sure current grammar type is dictionary-style
            if (_state.GrammarType != GrammarType.DictionaryStyle)
            {
                Fault fault = Fault.FromCodeSubcodeReason(
                    FaultCodes.Receiver,
                    DsspFaultCodes.OperationFailed,
                    "Cannot delete entries from grammar dictionary because grammar"
                    + "currently in use is not DictionaryStyle."
                );
                LogInfo(fault.ToException());
                delete.ResponsePort.Post(fault);
                yield break;
            }

            #region Set up load grammar request and load grammar
            // Set up load grammar request
            LoadGrammarRequest loadRequest = new LoadGrammarRequest();
            loadRequest.GrammarType = GrammarType.DictionaryStyle;
            loadRequest.DictionaryGrammar = new Dictionary<string,string>(_state.DictionaryGrammar);
            GrammarUtilities.DeleteDictionary(loadRequest.DictionaryGrammar, delete.Body.DictionaryGrammar);

            // Load new grammar
            SuccessFailurePort loadGrammarPort = new SuccessFailurePort();
            SpawnIterator<LoadGrammarRequest, SuccessFailurePort>(
                loadRequest,
                loadGrammarPort,
                LoadGrammar
            );

            // Check loading outcome
            yield return (Choice)loadGrammarPort;
            Exception exception = (Exception)loadGrammarPort;
            if (exception != null)
            {
                LogWarning(exception);
                delete.ResponsePort.Post(Fault.FromCodeSubcodeReason(
                    FaultCodes.Receiver,
                    DsspFaultCodes.OperationFailed,
                    exception.Message
                ));
                yield break;
            }
            #endregion

            SaveState(_state);

            // Notify subscribers of state change
            Replace replace = new Replace();
            replace.Body = _state;
            SendNotification<Replace>(_subMgrPort, replace);

            delete.ResponsePort.Post(DefaultDeleteResponseType.Instance);
        }

        /// <summary>
        /// Set SRGS grammar file handler
        /// </summary>
        /// <param name="setSrgsGrammarFile"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public IEnumerator<ITask> SetSrgsGrammarFileHandler(SetSrgsGrammarFile setSrgsGrammarFile)
        {
            #region Set up load grammar request and load grammar
            // Set up load grammar request
            LoadGrammarRequest loadRequest = new LoadGrammarRequest();
            loadRequest.GrammarType = GrammarType.Srgs;
            loadRequest.SrgsFileLocation = setSrgsGrammarFile.Body.FileLocation;

            // Load new grammar
            SuccessFailurePort loadGrammarPort = new SuccessFailurePort();
            SpawnIterator<LoadGrammarRequest, SuccessFailurePort>(
                loadRequest,
                loadGrammarPort,
                LoadGrammar
            );

            // Check loading outcome
            yield return (Choice)loadGrammarPort;
            Exception exception = (Exception)loadGrammarPort;
            if (exception != null)
            {
                LogWarning(exception);
                setSrgsGrammarFile.ResponsePort.Post(Fault.FromCodeSubcodeReason(
                    FaultCodes.Receiver,
                    DsspFaultCodes.OperationFailed,
                    exception.Message
                ));
                yield break;
            }
            #endregion

            SaveState(_state);

            // Notify subscribers of state change
            Replace replace = new Replace();
            replace.Body = _state;
            SendNotification<Replace>(_subMgrPort, replace);

            setSrgsGrammarFile.ResponsePort.Post(DefaultUpdateResponseType.Instance);
        }

        /// <summary>
        /// Eemulate recognize handler
        /// </summary>
        /// <param name="speech"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public IEnumerator<ITask> EmulateRecognizeHandler(EmulateRecognize speech)
        {
            if (string.IsNullOrEmpty(speech.Body.Text))
            {
                Fault fault = Fault.FromCodeSubcodeReason(
                    FaultCodes.Sender,
                    DsspFaultCodes.OperationFailed,
                    "Text of speech to be emulated cannot be empty"
                );
                LogInfo(fault.ToException());
                speech.ResponsePort.Post(fault);
                yield break;
            }

            _state.Recognizer.EmulateRecognizeAsync(speech.Body.Text);
            speech.ResponsePort.Post(DefaultUpdateResponseType.Instance);
            yield break;
        }

        /// <summary>
        /// Replace handler
        /// </summary>
        /// <param name="replace"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public IEnumerator<ITask> ReplaceHandler(Replace replace)
        {
            SpeechRecognizerState newState = replace.Body;

            #region Set up load grammar request, load grammar and on success store new grammar in state
            // Set up load grammar request
            LoadGrammarRequest loadRequest = new LoadGrammarRequest();
            loadRequest.GrammarType = newState.GrammarType;
            loadRequest.SrgsFileLocation = newState.SrgsFileLocation;
            loadRequest.DictionaryGrammar = newState.DictionaryGrammar;

            // Load new grammar
            SuccessFailurePort loadGrammarPort = new SuccessFailurePort();
            SpawnIterator<LoadGrammarRequest, SuccessFailurePort>(
                loadRequest,
                loadGrammarPort,
                LoadGrammar
            );

            // Check loading outcome
            yield return (Choice)loadGrammarPort;
            Exception exception = (Exception)loadGrammarPort;
            if (exception != null)
            {
                LogWarning(exception);
                replace.ResponsePort.Post(Fault.FromCodeSubcodeReason(
                    W3C.Soap.FaultCodes.Receiver,
                    DsspFaultCodes.OperationFailed,
                    exception.Message
                ));
                yield break;
            }
            #endregion

            #region Check new state's IgnoreAudioInput flag and start recognition engine if necessary
            if (newState.IgnoreAudioInput != _state.IgnoreAudioInput)
            {
                _state.IgnoreAudioInput = newState.IgnoreAudioInput;
                if (_state.IgnoreAudioInput)
                {
                    // Stop engine and switch to ignoring audio input
                    _state.Recognizer.RecognizeAsyncCancel();
                    _state.Recognizer.SetInputToNull();
                }
                else
                {
                    _state.Recognizer.SetInputToDefaultAudioDevice();

                    // Because old state ignored audio input the engine is stopped, now that
                    // we switched to listening to audio input the engine needs to be started
                    if (_state.Recognizer.Grammars.Count > 0)
                    {
                        _state.Recognizer.RecognizeAsync(RecognizeMode.Multiple);
                    }

                }
            }
            #endregion

            SaveState(_state);

            // Notify subscribers of state change
            replace.Body = _state;
            SendNotification<Replace>(_subMgrPort, replace);

            replace.ResponsePort.Post(DefaultReplaceResponseType.Instance);
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
        /// Speech detected operation (not supported, just used for notifications to subscribers)
        /// </summary>
        /// <param name="detected"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public IEnumerator<ITask> SpeechDetectedHandler(SpeechDetected detected)
        {
            PostActionNotSupported(detected);
            yield break;
        }

        /// <summary>
        /// Speech recognized operation (not supported, just used for notifications to subscribers)
        /// </summary>
        /// <param name="recognized"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public IEnumerator<ITask> SpeechRecognizedHandler(SpeechRecognized recognized)
        {
            PostActionNotSupported(recognized);
            yield break;
        }

        /// <summary>
        /// Speech recognition rejected operation (not supported, just used for
        /// notifications to subscribers)
        /// </summary>
        /// <param name="rejected"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public IEnumerator<ITask> SpeechDetectedHandler(SpeechRecognitionRejected rejected)
        {
            PostActionNotSupported(rejected);
            yield break;
        }

        /// <summary>
        /// Speech detected event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        private void SpeechDetectedHandler(object sender, SpeechDetectedEventArgs eventArgs)
        {
            SpeechDetected msg = new SpeechDetected();

            SpeechDetectedNotification notification = new SpeechDetectedNotification();
            notification.StartTime = DateTime.Now;

            msg.Body = notification;
            SendNotification(_subMgrPort, msg);
        }

        /// <summary>
        /// Speech recognized event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        private void SpeechRecognizedHandler(object sender, SpeechRecognizedEventArgs eventArgs)
        {
            RecognitionResult result = eventArgs.Result;
            SpeechRecognized msg = new SpeechRecognized();

            SpeechRecognizedNotification notification = new SpeechRecognizedNotification();
            if (result.Audio != null)
            {
                notification.StartTime = result.Audio.StartTime;
                notification.Duration = result.Audio.Duration;
            }
            else
            {
                // If the engine's audio input is set to null no audio information is available
                notification.StartTime = new DateTime(0);
                notification.Duration = new TimeSpan(0);
            }
            notification.Confidence = result.Confidence;
            notification.Text = result.Text;
            notification.Semantics = new RecognizedSemanticValue(null, result.Semantics);
            
            msg.Body = notification;
            SendNotification(_subMgrPort, msg);
        }

        /// <summary>
        /// Speech recognition rejected event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        private void SpeechRecognitionRejectedHandler(object sender, SpeechRecognitionRejectedEventArgs eventArgs)
        {
            SpeechRecognitionRejected msg = new SpeechRecognitionRejected();

            SpeechRecognitionRejectedNotification notification = new SpeechRecognitionRejectedNotification();
            if (eventArgs.Result.Audio != null)
            {
                notification.StartTime = eventArgs.Result.Audio.StartTime;
                notification.Duration = eventArgs.Result.Audio.Duration;
            }
            else
            {
                // If the engine's audio input is set to null no audio information is available
                notification.StartTime = new DateTime(0);
                notification.Duration = new TimeSpan(0);
            }


            msg.Body = notification;
            SendNotification(_subMgrPort, msg);
        }

        /// <summary>
        /// Loads a grammar into the speech recognition engine; MUST run exclusive
        /// </summary>
        /// <param name="request">Request that contains the new grammar to load</param>
        /// <param name="response">Response port</param>
        /// <returns></returns>
        private IEnumerator<ITask> LoadGrammar(LoadGrammarRequest request, SuccessFailurePort response)
        {
            #region Build grammar
            // Build grammar
            if (request.GrammarType == GrammarType.Srgs)
            {
                // Load SRGS grammar file
                FileReaderPort fileReaderPort = new FileReaderPort();
                yield return new IterativeTask(delegate
                    {
                        return ReadFileFromMountService(request.SrgsFileLocation, fileReaderPort);
                    });

                Exception fileReaderException = (Exception)fileReaderPort;
                if (fileReaderException != null)
                {
                    LogWarning(fileReaderException);
                    response.Post(fileReaderException);
                    yield break;
                }

                try
                {
                    _grammarToLoad = GrammarUtilities.BuildSrgsGrammar((MemoryStream)fileReaderPort);
                }
                catch (Exception ex)
                {
                    LogWarning(ex);
                    response.Post(ex);
                    yield break;
                }
            }
            else
            {
                // Build dictionary-style grammar
                try
                {
                    _grammarToLoad = GrammarUtilities.BuildDictionaryGrammar(request.DictionaryGrammar);
                }
                catch (Exception ex)
                {
                    LogWarning(ex);
                    response.Post(ex);
                    yield break;
                }
            }
            #endregion

            #region Load grammar and start engine if necessary
            // Request chance to update recognition engine and cancel current recognition
            // operation
            _state.Recognizer.RequestRecognizerUpdate();
            _state.Recognizer.RecognizeAsyncCancel();
            yield return (Choice)_loadGrammarResponsePort;

            Exception loadGrammarException = (Exception)_loadGrammarResponsePort;
            if (loadGrammarException != null)
            {
                LogWarning(loadGrammarException);
                response.Post(loadGrammarException);
                yield break;
            }

            // Empty response port
            SuccessResult loadGrammarSuccess = (SuccessResult)_loadGrammarResponsePort;

            // Start engine again
            if (_state.Recognizer.Grammars.Count > 0 && !_state.IgnoreAudioInput)
            {
                _state.Recognizer.RecognizeAsync(RecognizeMode.Multiple);
            }
            #endregion

            // Store information about the new grammar in the service's state
            _state.Grammar = _grammarToLoad;
            _state.GrammarType = request.GrammarType;
  
            if (request.GrammarType == GrammarType.Srgs)
            {
                _state.SrgsFileLocation = request.SrgsFileLocation;
                _state.DictionaryGrammar = null;
            }
            else
            {
                _state.DictionaryGrammar = request.DictionaryGrammar;
                _state.SrgsFileLocation = null;
            }

            response.Post(SuccessResult.Instance);
        }

        /// <summary>
        /// On speech recognizer ready for update callback, called by recognition engine
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void RecognizerUpdateReachedHandler(object sender, RecognizerUpdateReachedEventArgs args)
        {
            // Unload currently loaded grammar
            if (_state.Grammar != null)
            {
                try
                {
                    _state.Recognizer.UnloadGrammar(_state.Grammar);
                }
                catch (Exception) {}
            }

            if (_grammarToLoad != null)
            {
                // Load new grammar asynchronously
                try
                {
                    _state.Recognizer.LoadGrammarAsync(_grammarToLoad);
                }
                catch (Exception exception)
                {
                    LogWarning(exception);
                    _loadGrammarResponsePort.Post(exception);
                }
            }
            else
            {
                // Did not need to load a new grammar, done here
                _loadGrammarResponsePort.Post(SuccessResult.Instance);
            }
        }

        /// <summary>
        /// On grammar loaded callback, called by recognition engine
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void LoadGrammarCompletedHandler(object sender, LoadGrammarCompletedEventArgs args)
        {
            if (args.Error != null)
            {
                LogWarning(args.Error);

                // Try reloading the old grammar (if it exists and this is not
                // already an attempt to load the old grammar)
                if (_state.Grammar != null && _state.Grammar != _grammarToLoad)
                {
                    _grammarToLoad = _state.Grammar;
                    _state.Recognizer.RequestRecognizerUpdate();
                    return;
                }
                else
                {
                    _loadGrammarResponsePort.Post(args.Error);
                    return;
                }
            }

            if (args.Cancelled)
            {
                OperationCanceledException ex =
                    new OperationCanceledException("Loading grammar was canceled.");
                LogWarning(ex);
                _loadGrammarResponsePort.Post(ex);
                return;
            }

            _loadGrammarResponsePort.Post(SuccessResult.Instance);
        }

        /// <summary>
        /// Reads a binary file from the mount service and writes it into a memory stream
        /// </summary>
        /// <param name="filename">Relative path to the file on the mount service</param>
        /// <param name="readReaderPort">Response port</param>
        /// <returns></returns>
        private IEnumerator<ITask> ReadFileFromMountService(string filename, FileReaderPort readReaderPort)
        {
            if (string.IsNullOrEmpty(filename))
            {
                Exception exception = new ArgumentException(
                    "Cannot read file from mount service. No filename specified"
                );
                LogWarning(exception);
                readReaderPort.Post(exception);
                yield break;
            }

            // Construct URI to file
            string fileUri = "http://localhost" + ServicePaths.MountPoint;
            if (!filename.StartsWith("/"))
            {
                fileUri += "/";
            }
            fileUri += filename;

            // Establish channel with mount service
            mnt.MountServiceOperations mountPort = ServiceForwarder<mnt.MountServiceOperations>(fileUri);

            // Set up byte query
            mnt.QueryBytesRequest queryBytesRequest = new mnt.QueryBytesRequest();
            mnt.QueryBytes queryBytes = new mnt.QueryBytes(queryBytesRequest);
            queryBytesRequest.Offset = 0;
            queryBytesRequest.Length = MountServiceReadBlockSize;

            // Read file in blocks from mount service
            MemoryStream memoryStream = new MemoryStream();
            int bytesRead = (int)queryBytesRequest.Length;
            while (bytesRead == queryBytesRequest.Length)
            {
                mountPort.Post(queryBytes);
                yield return (Choice)queryBytes.ResponsePort;

                Fault fault = (Fault)queryBytes.ResponsePort;
                if (fault != null)
                {
                    LogWarning(fault.ToException());
                    readReaderPort.Post(fault.ToException());
                    yield break;
                }

                mnt.QueryBytesResponse byteQueryResponse = (mnt.QueryBytesResponse)queryBytes.ResponsePort;

                bytesRead = byteQueryResponse.Data.Length;
                memoryStream.Write(byteQueryResponse.Data, 0, bytesRead);
                queryBytesRequest.Offset += bytesRead;
            }
            memoryStream.Position = 0;

            readReaderPort.Post(memoryStream);
        }

        /// <summary>
        /// Posts an ActionNotSupported DSSP fault code to the response port of the passed in
        /// DSSP operation
        /// </summary>
        /// <param name="operation"></param>
        private static void PostActionNotSupported(DsspOperation operation)
        {
            operation.ResponsePort.TryPostUnknownType(Fault.FromCodeSubcode(
                W3C.Soap.FaultCodes.Sender,
                DsspFaultCodes.ActionNotSupported)
            );
        }

        /// <summary>
        /// File reader response port
        /// </summary>
        private class FileReaderPort : PortSet<MemoryStream, Exception>
        {
        }
    }
}
