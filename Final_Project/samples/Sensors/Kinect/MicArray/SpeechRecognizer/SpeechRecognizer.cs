//------------------------------------------------------------------------------
//  <copyright file="SpeechRecognizer.cs" company="Microsoft Corporation">
//      Copyright (C) Microsoft Corporation.  All rights reserved.
//  </copyright>
//------------------------------------------------------------------------------


namespace Microsoft.Robotics.Services.Sensors.Kinect.MicArraySpeechRecognizer
{

    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using System.Xml;
    using Microsoft.Ccr.Core;
    using Microsoft.Dss.Core;
    using Microsoft.Dss.Core.Attributes;
    using Microsoft.Dss.Core.DsspHttp;
    using Microsoft.Dss.ServiceModel.Dssp;
    using Microsoft.Dss.ServiceModel.DsspServiceBase;
    using Microsoft.Kinect;
    using Microsoft.Speech.Recognition;
    using W3C.Soap;

    using mnt = Microsoft.Dss.Services.MountService;
    using saf = Microsoft.Speech.AudioFormat;
    using sr = Microsoft.Speech.Recognition;
    using submgr = Microsoft.Dss.Services.SubscriptionManager;


    /// <summary>
    /// SpeechRecognizer service - Recognizes spoken commands from a defined grammar
    /// </summary>
    [DisplayName("(User) KinectMicArraySpeechRecognizer")]
    [Description("Recognizes speech and the direction its coming from, using Kinect MicArray and Microsoft.Speech framework,"
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
            + "/MicArraySpeechRecognizer.user.config.xml")]
        private SpeechRecognizerState state = new SpeechRecognizerState();
        
        /// <summary>
        /// The service's main port
        /// </summary>
        [ServicePort("/micarrayspeechrecognizer", AllowMultipleInstances = false)]
        private SpeechRecognizerOperations _mainPort = new SpeechRecognizerOperations();

        /// <summary>
        /// Subscription manager port for handling speech recognition event subscriptions
        /// </summary>
        [Partner("SubMgr", Contract = submgr.Contract.Identifier, CreationPolicy = PartnerCreationPolicy.CreateAlways)]
        private submgr.SubscriptionManagerPort subMgrPort = new submgr.SubscriptionManagerPort();

        /// <summary>
        /// Block size with which files are read from the mount service
        /// </summary>
        private const int MountServiceReadBlockSize = 32768;

        /// <summary>
        /// Success/failure port LoadGrammar waits on and the speech recognition engine
        /// callback posts to
        /// </summary>
        private SuccessFailurePort loadGrammarResponsePort = new SuccessFailurePort();
        private Grammar grammarToLoad;

        /// <summary>
        /// Kinect Audio source and stream to work with Kinect MicArray. Sound shource locaiton, confidence, etc will be read into
        /// _state upon detection of speech events 
        /// callback posts to
        /// </summary>
        private KinectAudioSource kinectAudioSource;
        private Stream kinectAudioStream;

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
                if (this.state == null)
                {
                    this.state = new SpeechRecognizerState();
                }

                InitializeKinectAudio();
                
                // Set up speech recognition engine
                if (this.state.IgnoreAudioInput)
                {
                    this.state.Recognizer.SetInputToNull();
                }
                else
                {
                    SetRecognizerInputToKinectMicArray();
                }                 

                // Register handlers for speech and beam events
                this.state.Recognizer.RecognizerUpdateReached += RecognizerUpdateReachedHandler;
                this.state.Recognizer.LoadGrammarCompleted += LoadGrammarCompletedHandler;
                this.state.Recognizer.SpeechDetected += SpeechDetectedHandler;
                this.state.Recognizer.SpeechRecognized += SpeechRecognizedHandler;
                this.state.Recognizer.SpeechRecognitionRejected += SpeechRecognitionRejectedHandler;
                this.kinectAudioSource.BeamAngleChanged += BeamChangedHandler;
            }
            catch (Exception exception)
            {
                // Fatal exception during startup, shutdown service
                LogError(LogGroups.Activation, exception);

                if (null != this.state.Recognizer)
                {
                    this.state.Recognizer.Dispose();
                }

                DefaultDropHandler(new DsspDefaultDrop());

                return;
            }

            SpawnIterator(LoadGrammarOnStartup);
        }
                
        /// <summary>
        /// State Initializer
        /// </summary>
        public void InitializeKinectAudio()
        {            
            KinectSensor sensor = (from sensorToCheck in KinectSensor.KinectSensors where sensorToCheck.Status == KinectStatus.Connected select sensorToCheck).FirstOrDefault();

            if (null == sensor) 
            {
                // Sensor not connected, bail                
                throw new InvalidOperationException("Can not find Kinect sensor, make sure Kinect is connected");
            }

            this.kinectAudioSource = sensor.AudioSource;
            this.kinectAudioSource.AutomaticGainControlEnabled = false; //Important to turn this off for speech recognition (Kinect SDK recommendaiton)
            sensor.Start();
            this.kinectAudioStream = this.kinectAudioSource.Start();

            Func<RecognizerInfo, bool> matchingFunc = r =>
            {
                string value;
                r.AdditionalInfo.TryGetValue("Kinect", out value);
                return "True".Equals(value, StringComparison.InvariantCultureIgnoreCase) && "en-US".Equals(r.Culture.Name, StringComparison.InvariantCultureIgnoreCase);
            };

            sr.RecognizerInfo ri = SpeechRecognitionEngine.InstalledRecognizers().Where(matchingFunc).FirstOrDefault();

            if (null == ri) 
            {
                // If Speech recognizer is not detected, we have no choice but bail
                throw new InvalidOperationException("Can not find en-US speech recognizer");
            }

            this.state.Recognizer = new sr.SpeechRecognitionEngine(ri.Id);
        }

        /// <summary>
        /// Kinect MicArray helper
        /// </summary>
        public void SetRecognizerInputToKinectMicArray()
        {
            this.state.Recognizer.SetInputToAudioStream(this.kinectAudioStream,
                                                  new saf.SpeechAudioFormatInfo(
                                                      saf.EncodingFormat.Pcm, 16000, 16, 1,
                                                      32000, 2, null));
        }   

        /// <summary>
        /// Loads grammar on service startup
        /// </summary>
        /// <returns></returns>
        private IEnumerator<ITask> LoadGrammarOnStartup()
        {
            // Determine whether we have a valid grammar to load
            bool loadGrammar = false;
            switch (this.state.GrammarType)
            {
                case GrammarType.DictionaryStyle:
                    loadGrammar = this.state.DictionaryGrammar.Count > 0;
                    break;
                case GrammarType.Srgs:
                    loadGrammar = !string.IsNullOrEmpty(this.state.SrgsFileLocation);
                    break;
            }

            // Load grammar
            if (loadGrammar)
            {
                SuccessFailurePort loadGrammarPort = new SuccessFailurePort();
                LoadGrammarRequest loadRequest = new LoadGrammarRequest();
                loadRequest.GrammarType = this.state.GrammarType;
                loadRequest.DictionaryGrammar = this.state.DictionaryGrammar;
                loadRequest.SrgsFileLocation = this.state.SrgsFileLocation;
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
            this.state.Recognizer.Dispose();
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
            if (this.state.GrammarType != GrammarType.DictionaryStyle)
            {
                // Since we are switching grammar mode make sure there exists a valid,
                // empty dictionary
                if (this.state.DictionaryGrammar == null)
                {
                    this.state.DictionaryGrammar = new Dictionary<string, string>();
                }
                else
                {
                    this.state.DictionaryGrammar.Clear();
                }
            }

            #region Set up load grammar request and load grammar
            // Set up load grammar request
            LoadGrammarRequest loadRequest = new LoadGrammarRequest();
            loadRequest.GrammarType = GrammarType.DictionaryStyle;
            loadRequest.DictionaryGrammar = new Dictionary<string,string>(state.DictionaryGrammar);
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

            SaveState(this.state);

            // Notify subscribers of state change
            Replace replace = new Replace();
            replace.Body = this.state;
            SendNotification<Replace>(subMgrPort, replace);

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
            if (this.state.GrammarType != GrammarType.DictionaryStyle)
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
            loadRequest.DictionaryGrammar = new Dictionary<string,string>(state.DictionaryGrammar);
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

            SaveState(this.state);

            // Notify subscribers of state change
            Replace replace = new Replace();
            replace.Body = this.state;
            SendNotification<Replace>(this.subMgrPort, replace);

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
            if (this.state.GrammarType != GrammarType.DictionaryStyle)
            {
                // Since we are switching grammar mode make sure there exists a valid,
                // empty dictionary
                if (this.state.DictionaryGrammar == null)
                {
                    this.state.DictionaryGrammar = new Dictionary<string, string>();
                }
                else
                {
                    this.state.DictionaryGrammar.Clear();
                }
            }

            #region Set up load grammar request and load grammar
            // Set up load grammar request
            LoadGrammarRequest loadRequest = new LoadGrammarRequest();
            loadRequest.GrammarType = GrammarType.DictionaryStyle;
            loadRequest.DictionaryGrammar = new Dictionary<string,string>(state.DictionaryGrammar);
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

            SaveState(this.state);

            // Notify subscribers of state change
            Replace replace = new Replace();
            replace.Body = this.state;
            SendNotification<Replace>(this.subMgrPort, replace);

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
            if (this.state.GrammarType != GrammarType.DictionaryStyle)
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
            loadRequest.DictionaryGrammar = new Dictionary<string,string>(state.DictionaryGrammar);
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

            SaveState(this.state);

            // Notify subscribers of state change
            Replace replace = new Replace();
            replace.Body = this.state;
            SendNotification<Replace>(this.subMgrPort, replace);

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

            SaveState(this.state);

            // Notify subscribers of state change
            Replace replace = new Replace();
            replace.Body = this.state;
            SendNotification<Replace>(this.subMgrPort, replace);

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

            this.state.Recognizer.EmulateRecognizeAsync(speech.Body.Text);
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
            if (newState.IgnoreAudioInput != state.IgnoreAudioInput)
            {
                state.IgnoreAudioInput = newState.IgnoreAudioInput;
                if (state.IgnoreAudioInput)
                {
                    // Stop engine and switch to ignoring audio input
                    state.Recognizer.RecognizeAsyncCancel();
                    state.Recognizer.SetInputToNull();
                }
                else
                {
                    SetRecognizerInputToKinectMicArray();

                    // Because old state ignored audio input the engine is stopped, now that
                    // we switched to listening to audio input the engine needs to be started
                    if (state.Recognizer.Grammars.Count > 0)
                    {
                        state.Recognizer.RecognizeAsync(RecognizeMode.Multiple);
                    }

                }
            }
            #endregion

            SaveState(this.state);

            // Notify subscribers of state change
            replace.Body = this.state;
            SendNotification<Replace>(this.subMgrPort, replace);

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
                this.subMgrPort,
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

            SendNotificationToTarget<Replace>(request.Subscriber, this.subMgrPort, this.state);
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
        /// Beam changed operation (not supported, just used for
        /// notifications to subscribers)
        /// </summary>
        /// <param name="beamChanged"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public IEnumerator<ITask> BeamChangedHandler(BeamDirectionChanged beamChanged)
        {
            PostActionNotSupported(beamChanged);
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

            ReadKinectMicArrayInfo(notification);

            msg.Body = notification;
            SendNotification(subMgrPort, msg);
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
            PopulateCommonSpeechNotificationInformation(result, notification);
            notification.Confidence = result.Confidence;
            notification.Text = result.Text;
            notification.Semantics = new RecognizedSemanticValue(null, result.Semantics);
            
            msg.Body = notification;
            SendNotification(subMgrPort, msg);
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

            PopulateCommonSpeechNotificationInformation(eventArgs.Result, notification);

            msg.Body = notification;
            SendNotification(subMgrPort, msg);
        }

        /// <summary>
        /// Speech events have some common fields that are filled out the same way for speech recognized/rejected
        /// notificaitons - this is a helper method that populated common data in those notifications
        /// </summary>
        /// <param name="result"></param>
        /// <param name="notification"></param>
        private void PopulateCommonSpeechNotificationInformation(RecognitionResult result, SpeechAudioInformation notification)
        {
            if (result.Audio != null)
            {
                ReadKinectMicArrayInfo(notification);

                // if speech has been recognized - we want to use on Recognition result's start time - and not the one
                // populated by ReadKinectMicArrayInfo, which sets it to 'Now'
                notification.StartTime = result.Audio.StartTime;
                notification.Duration = result.Audio.Duration;                
            }
            else
            {
                // If the engine's audio input is set to null no audio information is available
                notification.StartTime = new DateTime(0);
                notification.Duration = new TimeSpan(0);
                notification.Angle = 0.0;
                notification.DirectionConfidence = 0.0;
            }
        }

        /// <summary>
        /// Beam direction changed event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        private void BeamChangedHandler(object sender, BeamAngleChangedEventArgs eventArgs)
        {
            BeamDirectionChanged msg = new BeamDirectionChanged();

            BeamDirectionChangedNotification notification = new BeamDirectionChangedNotification();

            ReadKinectMicArrayInfo(notification);

            msg.Body = notification;
            SendNotification(subMgrPort, msg);
        }

        private void ReadKinectMicArrayInfo(SpeechInformation notification)
        {
            notification.StartTime = DateTime.Now;

            this.state.KinectAudioBeamInformation.Angle = this.kinectAudioSource.SoundSourceAngle;
            this.state.KinectAudioBeamInformation.Confidence = this.kinectAudioSource.SoundSourceAngleConfidence;

            notification.Angle = this.state.KinectAudioBeamInformation.Angle;
            notification.DirectionConfidence = this.state.KinectAudioBeamInformation.Confidence;
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
                    grammarToLoad = GrammarUtilities.BuildSrgsGrammar((MemoryStream)fileReaderPort);
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
                    grammarToLoad = GrammarUtilities.BuildDictionaryGrammar(request.DictionaryGrammar);
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
            state.Recognizer.RequestRecognizerUpdate();
            state.Recognizer.RecognizeAsyncCancel();
            yield return (Choice)loadGrammarResponsePort;

            Exception loadGrammarException = (Exception)loadGrammarResponsePort;
            if (loadGrammarException != null)
            {
                LogWarning(loadGrammarException);
                response.Post(loadGrammarException);
                yield break;
            }

            // Empty response port
            SuccessResult loadGrammarSuccess = (SuccessResult)loadGrammarResponsePort;

            // Start engine again
            if (state.Recognizer.Grammars.Count > 0 && !state.IgnoreAudioInput)
            {
                state.Recognizer.RecognizeAsync(RecognizeMode.Multiple);
            }
            #endregion

            // Store information about the new grammar in the service's state
            this.state.Grammar = grammarToLoad;
            this.state.GrammarType = request.GrammarType;
  
            if (request.GrammarType == GrammarType.Srgs)
            {
                this.state.SrgsFileLocation = request.SrgsFileLocation;
                this.state.DictionaryGrammar = null;
            }
            else
            {
                this.state.DictionaryGrammar = request.DictionaryGrammar;
                this.state.SrgsFileLocation = null;
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
            if (this.state.Grammar != null)
            {
                try
                {
                    this.state.Recognizer.UnloadGrammar(this.state.Grammar);
                }
                catch (Exception) {}
            }

            if (grammarToLoad != null)
            {
                // Load new grammar asynchronously
                try
                {
                    this.state.Recognizer.LoadGrammarAsync(grammarToLoad);
                }
                catch (Exception exception)
                {
                    LogWarning(exception);
                    loadGrammarResponsePort.Post(exception);
                }
            }
            else
            {
                // Did not need to load a new grammar, done here
                loadGrammarResponsePort.Post(SuccessResult.Instance);
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
                if (this.state.Grammar != null && this.state.Grammar != grammarToLoad)
                {
                    this.grammarToLoad = this.state.Grammar;
                    this.state.Recognizer.RequestRecognizerUpdate();
                    return;
                }
                else
                {
                    this.loadGrammarResponsePort.Post(args.Error);
                    return;
                }
            }

            if (args.Cancelled)
            {
                OperationCanceledException ex =
                    new OperationCanceledException("Loading grammar was canceled.");
                LogWarning(ex);
                this.loadGrammarResponsePort.Post(ex);
                return;
            }

            this.loadGrammarResponsePort.Post(SuccessResult.Instance);
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
