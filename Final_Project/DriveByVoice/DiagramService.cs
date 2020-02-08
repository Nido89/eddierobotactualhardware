///////////////////////////////////////////////////////////////////////////////
// Activity: program.activity
// Diagram service implementation
///////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml;

using ccr = Microsoft.Ccr.Core;
using dss = Microsoft.Dss.Core;
using dssa = Microsoft.Dss.Core.Attributes;
using dssh = Microsoft.Dss.Core.DsspHttp;
using dssm = Microsoft.Dss.ServiceModel.DsspServiceBase;
using dssp = Microsoft.Dss.ServiceModel.Dssp;
using soap = W3C.Soap;

using submgr = Microsoft.Dss.Services.SubscriptionManager;
using texttospeech = Microsoft.Robotics.Technologies.Speech.TextToSpeech.Proxy;
using drive = Microsoft.Robotics.Services.Drive.Proxy;
using speechrecognizer = Microsoft.Robotics.Technologies.Speech.SpeechRecognizer.Proxy;

namespace Robotics.DriveByVoice.Diagram
{
    [DisplayName("DriveByVoice")]
    [Description("A user defined activity.")]
    [dssa.Contract(Contract.Identifier)]
    public class DiagramService : dssm.DsspServiceBase
    {
        // Service state
        [dssa.InitialStatePartner(Optional = true)]
        private DiagramState _state;

        // Service operations port
        [dssa.ServicePort("/DriveByVoice", AllowMultipleInstances = true)]
        private DiagramOperations _mainPort = new DiagramOperations();

        #region Partner services

        [dssa.Partner("SubMgr", Contract = submgr.Contract.Identifier, CreationPolicy = dssa.PartnerCreationPolicy.CreateAlways)]
        private submgr.SubscriptionManagerPort _subMgr = new submgr.SubscriptionManagerPort();

        // Partner: SpeechRecognizer, Contract: http://schemas.microsoft.com/robotics/2008/02/speechrecognizer.html
        [dssa.Partner("SpeechRecognizer", Contract = speechrecognizer.Contract.Identifier, CreationPolicy = dssa.PartnerCreationPolicy.UsePartnerListEntry)]
        speechrecognizer.SpeechRecognizerOperations _speechRecognizerPort = new speechrecognizer.SpeechRecognizerOperations();
        speechrecognizer.SpeechRecognizerOperations _speechRecognizerNotify = new speechrecognizer.SpeechRecognizerOperations();

        // Partner: TexttoSpeechTTS, Contract: http://schemas.microsoft.com/2006/05/texttospeech.html
        [dssa.Partner("TexttoSpeechTTS", Contract = texttospeech.Contract.Identifier, CreationPolicy = dssa.PartnerCreationPolicy.UsePartnerListEntry)]
        texttospeech.SpeechTextOperations _texttoSpeechTTSPort = new texttospeech.SpeechTextOperations();

        // Partner: Parallax2011ReferencePlatformIoController, Contract: http://schemas.microsoft.com/robotics/2006/05/drive.html
        [dssa.Partner("Parallax2011ReferencePlatformIoController", Contract = drive.Contract.Identifier, CreationPolicy = dssa.PartnerCreationPolicy.UsePartnerListEntry)]
        drive.DriveOperations _parallax2011ReferencePlatformIoControllerPort = new drive.DriveOperations();

        #endregion

        public DiagramService(dssp.DsspServiceCreationPort creationPort)
            : base(creationPort)
        {
        }

        protected override void Start()
        {
            // If there was no initial state partner, then the service state will be null.
            if (_state == null)
            {
                // The state MUST be created before the service starts processing messages.
                _state = new DiagramState();
            }

            // The rest of the start process requires the ability to wait for responses from
            // services and from the start handler (if any). So execution now proceeds in an
            // iterator function.
            SpawnIterator(DoStart);
        }

        private IEnumerator<ccr.ITask> DoStart()
        {
            soap.Fault fault = null;

            // Subscribe to partners

            yield return ccr.Arbiter.Choice(
                _speechRecognizerPort.Subscribe(_speechRecognizerNotify,                
                    typeof(Microsoft.Robotics.Technologies.Speech.SpeechRecognizer.Proxy.SpeechRecognized),
                    typeof(Microsoft.Robotics.Technologies.Speech.SpeechRecognizer.Proxy.SpeechRecognitionRejected)
                ),
                EmptyHandler,
                delegate(soap.Fault f)
                {
                    fault = f;
                }
            );

            if (fault != null)
            {
                LogError("Failed to subscribe to program.SpeechRecognizer.SpeechRecognizer", fault);
                StartFailed();
                yield break;
            }

            // Start the RunHandler, this represents the parts of the diagram that
            // are are not run in the context of an operation or notification.
            StartHandler start = new StartHandler(this, Environment.TaskQueue);
            SpawnIterator(start.RunHandler);
            // Wait until the RunHandler has completed.
            yield return ccr.Arbiter.Receive(false, start.Complete, EmptyHandler);

            // Start operation handlers and insert into directory service.
            StartHandlers();

            // Add notifications to the main interleave
            base.MainPortInterleave.CombineWith(
                new ccr.Interleave(
                    new ccr.ExclusiveReceiverGroup(
                    ),
                    new ccr.ConcurrentReceiverGroup(
                    )
                )
            );

            // Activate independent tasks
            Activate<ccr.ITask>(
                ccr.Arbiter.ReceiveWithIterator<Microsoft.Robotics.Technologies.Speech.SpeechRecognizer.Proxy.SpeechRecognized>(true, _speechRecognizerNotify, SpeechRecognizerSpeechRecognizedHandler),
                ccr.Arbiter.ReceiveWithIterator<Microsoft.Robotics.Technologies.Speech.SpeechRecognizer.Proxy.SpeechRecognitionRejected>(true, _speechRecognizerNotify, SpeechRecognizerSpeechRecognitionRejectedHandler)
            );

            yield break;
        }

        private void StartHandlers()
        {
            // Activate message handlers for this service and insert into the directory.
            base.Start();
        }

        #region Standard DSS message handlers

        [dssa.ServiceHandler(dssa.ServiceHandlerBehavior.Concurrent)]
        public virtual IEnumerator<ccr.ITask> GetHandler(Get get)
        {
            get.ResponsePort.Post(_state);
            yield break;
        }

        [dssa.ServiceHandler(dssa.ServiceHandlerBehavior.Concurrent)]
        public virtual IEnumerator<ccr.ITask> HttpGetHandler(dssh.HttpGet httpGet)
        {
            httpGet.ResponsePort.Post(new dssh.HttpResponseType(_state));
            yield break;
        }

        [dssa.ServiceHandler(dssa.ServiceHandlerBehavior.Exclusive)]
        public virtual IEnumerator<ccr.ITask> ReplaceHandler(Replace replace)
        {
            _state = replace.Body;

            replace.ResponsePort.Post(dssp.DefaultReplaceResponseType.Instance);
            base.SendNotification<Replace>(_subMgr, replace);

            yield break;
        }

        [dssa.ServiceHandler(dssa.ServiceHandlerBehavior.Concurrent)]
        public virtual IEnumerator<ccr.ITask> SubscribeHandler(Subscribe subscribe)
        {
            dssp.SubscribeRequestType request = subscribe.Body;

            yield return ccr.Arbiter.Choice(
                SubscribeHelper(_subMgr, request, subscribe.ResponsePort),
                delegate(ccr.SuccessResult success)
                {
                    base.SendNotificationToTarget<Replace>(request.Subscriber, _subMgr, _state);
                },
                delegate(Exception e) { }
            );
        }

        #endregion

        #region Custom message handlers

        [dssa.ServiceHandler(dssa.ServiceHandlerBehavior.Independent)]
        public virtual IEnumerator<ccr.ITask> ActionHandler(Action message)
        {
            // Empty handler. Respond with default response type.
            message.ResponsePort.Post(new ActionResponse());
            yield break;
        }

        #endregion

        #region Notification message handlers

        IEnumerator<ccr.ITask> SpeechRecognizerSpeechRecognizedHandler(Microsoft.Robotics.Technologies.Speech.SpeechRecognizer.Proxy.SpeechRecognized message)
        {
            OnSpeechRecognizerSpeechRecognizedHandler handler = new OnSpeechRecognizerSpeechRecognizedHandler(this, Environment.TaskQueue);
            return handler.RunHandler(message);
        }

        IEnumerator<ccr.ITask> SpeechRecognizerSpeechRecognitionRejectedHandler(Microsoft.Robotics.Technologies.Speech.SpeechRecognizer.Proxy.SpeechRecognitionRejected message)
        {
            OnSpeechRecognizerSpeechRecognitionRejectedHandler handler = new OnSpeechRecognizerSpeechRecognitionRejectedHandler(this, Environment.TaskQueue);
            return handler.RunHandler(message);
        }

        #endregion

        #region OnSpeechRecognizerSpeechRecognizedHandler class

        class OnSpeechRecognizerSpeechRecognizedHandler : HandlerBase
        {
            ///////////////////////////////////////////////////////////////////
            // if if{(join1) (expr0)}else{&element}
            // if1 if{expr1 - &element0}elseif{&join1}else{&element}
            // expr00 - if0 if{expr2 - element0}elseif{expr3 - &element0}elseif{[(expr5 - join)(expr4 - &join)] - element1}elseif{[(expr50 - join0)(expr40 - &join0)] - &element1}else{element}
            // [(__use__3.snippet.snippet.noop - __use__3.snippet.snippet.expr - __use__3.snippet.snippet.join)] - __use__3.snippet.call - __use__3.snippet.call.iftype
            // __use__50.snippet.snippet.noop - [(__use__50.snippet.snippet.expr0 - __use__50.snippet.snippet.join)(__use__50.snippet.snippet.expr - &__use__50.snippet.snippet.join)] - __use__50.snippet.call - __use__50.snippet.call.iftype
            // __use__5.snippet0.snippet.noop - [(__use__5.snippet0.snippet.expr0 - __use__5.snippet0.snippet.join)(__use__5.snippet0.snippet.expr - &__use__5.snippet0.snippet.join)] - __use__5.snippet0.call - __use__5.snippet0.call.iftype
            ///////////////////////////////////////////////////////////////////

            public OnSpeechRecognizerSpeechRecognizedHandler(DiagramService service, ccr.DispatcherQueue queue)
                : base(service, queue)
            {

                // Activate merge handlers
                Activate(ccr.Arbiter.Receive(true, _mergeAlpha, _mergeAlphaHandler));
                Activate(ccr.Arbiter.Receive(true, _mergeBeta, _mergeBetaHandler));
                Activate(ccr.Arbiter.Receive(true, _mergeGamma, _mergeGammaHandler));

                RegisterAndActivateJoins();
            }

            ///////////////////////////////////////////////////////////////////
            // if if{(join1) (expr0)}else{&element}
            // if1 if{expr1 - &element0}elseif{&join1}else{&element}
            ///////////////////////////////////////////////////////////////////

            public IEnumerator<ccr.ITask> RunHandler(Microsoft.Robotics.Technologies.Speech.SpeechRecognizer.Proxy.SpeechRecognized message)
            {
                Increment();

                if (message.Body.Confidence > 0.7D)
                {
                    Increment();
                    _joinAlphaPorts[1].Post(message.Body);

                    if (message.Body.Semantics.Children[@"TypeOfMoving"].ValueString == @"Stop")
                    {

                        Increment();
                        _mergeGamma.Post(FloatCast(0D));
                    }
                    else if (message.Body.Semantics.Children[@"TypeOfMoving"].ValueString == @"Move")
                    {
                        Increment();
                        _joinAlphaPorts[0].Post(message.Body.Semantics.Children[@"TypeOfMoving"].ValueString);
                    }
                    else
                    {

                        Increment();
                        _mergeAlpha.Post(message.Body.Semantics.Children[@"TypeOfMoving"].ValueString);
                    }
                }
                else
                {

                    Increment();
                    _mergeAlpha.Post(message.Body);
                }

                Decrement();

                yield return WaitUntilComplete();
            }

            ccr.Port<object> _mergeAlpha = new ccr.Port<object>();

            ///////////////////////////////////////////////////////////////////
            // [(__use__3.snippet.snippet.noop - __use__3.snippet.snippet.expr - __use__3.snippet.snippet.join)] - __use__3.snippet.call - __use__3.snippet.call.iftype
            ///////////////////////////////////////////////////////////////////

            void _mergeAlphaHandler(object message)
            {
                texttospeech.SayTextRequest request = new texttospeech.SayTextRequest();
                request.SpeechText = @"I did not understand you.";
                TexttoSpeechTTSPort.SayText(request);

                Decrement();
            }

            ccr.Port<double> _mergeGamma = new ccr.Port<double>();

            ///////////////////////////////////////////////////////////////////
            // __use__5.snippet0.snippet.noop - [(__use__5.snippet0.snippet.expr0 - __use__5.snippet0.snippet.join)(__use__5.snippet0.snippet.expr - &__use__5.snippet0.snippet.join)] - __use__5.snippet0.call - __use__5.snippet0.call.iftype
            ///////////////////////////////////////////////////////////////////

            void _mergeGammaHandler(double message)
            {
                drive.SetDrivePowerRequest request = new drive.SetDrivePowerRequest();
                request.RightWheelPower = message;
                request.LeftWheelPower = message;
                Parallax2011ReferencePlatformIoControllerPort.SetDrivePower(request);

                Decrement();
            }

            ///////////////////////////////////////////////////////////////////
            // expr00 - if0 if{expr2 - element0}elseif{expr3 - &element0}elseif{[(expr5 - join)(expr4 - &join)] - element1}elseif{[(expr50 - join0)(expr40 - &join0)] - &element1}else{element}
            ///////////////////////////////////////////////////////////////////

            void _joinAlphaHandler(object[] args)
            {
                JoinAlpha message = new JoinAlpha(args);

                if (message.Recognition.Semantics.Children[@"MovingDirection"].ValueString == @"Forward")
                {

                    Increment();
                    _mergeGamma.Post(FloatCast(0.5D));
                }
                else if (message.Recognition.Semantics.Children[@"MovingDirection"].ValueString == @"Backward")
                {

                    Increment();
                    _mergeGamma.Post(FloatCast(-0.5D));
                }
                else if (message.Recognition.Semantics.Children[@"MovingDirection"].ValueString == @"Left")
                {
                    JoinBeta a = new JoinBeta();
                    a.Right = FloatCast(0.15D);
                    a.Left = FloatCast(-0.15D);

                    Increment();
                    _mergeBeta.Post(a);
                }
                else if (message.Recognition.Semantics.Children[@"MovingDirection"].ValueString == @"Right")
                {
                    JoinBeta b = new JoinBeta();
                    b.Right = FloatCast(-0.15D);
                    b.Left = FloatCast(0.15D);

                    Increment();
                    _mergeBeta.Post(b);
                }
                else
                {

                    Increment();
                    _mergeAlpha.Post(message.Recognition.Semantics.Children[@"MovingDirection"].ValueString);
                }

                Decrement(args.Length);
            }

            ccr.Port<JoinBeta> _mergeBeta = new ccr.Port<JoinBeta>();

            ///////////////////////////////////////////////////////////////////
            // __use__50.snippet.snippet.noop - [(__use__50.snippet.snippet.expr0 - __use__50.snippet.snippet.join)(__use__50.snippet.snippet.expr - &__use__50.snippet.snippet.join)] - __use__50.snippet.call - __use__50.snippet.call.iftype
            ///////////////////////////////////////////////////////////////////

            void _mergeBetaHandler(JoinBeta message)
            {
                drive.SetDrivePowerRequest request = new drive.SetDrivePowerRequest();
                request.RightWheelPower = message.Right;
                request.LeftWheelPower = message.Left;
                Parallax2011ReferencePlatformIoControllerPort.SetDrivePower(request);

                Decrement();
            }

            class JoinGamma
            {
                public string SpeechText;

                public JoinGamma()
                {
                }

                public JoinGamma(object[] args)
                {
                    SpeechText = args[0].ToString();
                }
            }

            ccr.Port<object>[] _joinGammaPorts = new ccr.Port<object>[1]{
                new ccr.Port<object>()
            };

            class JoinAlpha
            {
                public string TypeOfMoving;
                public Microsoft.Robotics.Technologies.Speech.SpeechRecognizer.Proxy.SpeechRecognizedNotification Recognition;

                public JoinAlpha()
                {
                }

                public JoinAlpha(object[] args)
                {
                    TypeOfMoving = (string)args[0];
                    Recognition = (Microsoft.Robotics.Technologies.Speech.SpeechRecognizer.Proxy.SpeechRecognizedNotification)args[1];
                }
            }

            ccr.Port<object>[] _joinAlphaPorts = new ccr.Port<object>[2]{
                new ccr.Port<object>(),
                new ccr.Port<object>()
            };

            class JoinBeta
            {
                public double Left;
                public double Right;

                public JoinBeta()
                {
                }

                public JoinBeta(object[] args)
                {
                    Left = (double)args[0];
                    Right = (double)args[1];
                }
            }

            ccr.Port<object>[] _joinBetaPorts = new ccr.Port<object>[2]{
                new ccr.Port<object>(),
                new ccr.Port<object>()
            };

            class JoinDelta
            {
                public string LeftWheelPower;
                public string RightWheelPower;

                public JoinDelta()
                {
                }

                public JoinDelta(object[] args)
                {
                    LeftWheelPower = args[0].ToString();
                    RightWheelPower = args[1].ToString();
                }
            }

            ccr.Port<object>[] _joinDeltaPorts = new ccr.Port<object>[2]{
                new ccr.Port<object>(),
                new ccr.Port<object>()
            };

            ccr.Port<object>[] _secondJoinBetaPorts = new ccr.Port<object>[2]{
                new ccr.Port<object>(),
                new ccr.Port<object>()
            };

            ccr.Port<object>[] _secondJoinDeltaPorts = new ccr.Port<object>[2]{
                new ccr.Port<object>(),
                new ccr.Port<object>()
            };

            void RegisterAndActivateJoins()
            {
                // Register joins with base class to manage active count for incomplete joins.
                base.RegisterJoin(_joinAlphaPorts);
                // Activate Join handlers
                Activate(ccr.Arbiter.MultiplePortReceive(true, _joinAlphaPorts, _joinAlphaHandler));
            }
        }

        #endregion

        #region OnSpeechRecognizerSpeechRecognitionRejectedHandler class

        class OnSpeechRecognizerSpeechRecognitionRejectedHandler : HandlerBase
        {
            ///////////////////////////////////////////////////////////////////
            // element
            // __use__3.snippet.snippet.noop - __use__3.snippet.snippet.expr - __use__3.snippet.snippet.join
            // __use__3.snippet.call - __use__3.snippet.call.iftype
            ///////////////////////////////////////////////////////////////////

            public OnSpeechRecognizerSpeechRecognitionRejectedHandler(DiagramService service, ccr.DispatcherQueue queue)
                : base(service, queue)
            {

                // Activate merge handlers
                Activate(ccr.Arbiter.Receive(true, _mergeAlpha, _mergeAlphaHandler));

                RegisterAndActivateJoins();
            }

            ///////////////////////////////////////////////////////////////////
            // element
            ///////////////////////////////////////////////////////////////////

            public IEnumerator<ccr.ITask> RunHandler(Microsoft.Robotics.Technologies.Speech.SpeechRecognizer.Proxy.SpeechRecognitionRejected message)
            {
                Increment();

                Increment();
                _mergeAlpha.Post(message.Body);

                Decrement();

                yield return WaitUntilComplete();
            }

            ccr.Port<object> _mergeAlpha = new ccr.Port<object>();

            ///////////////////////////////////////////////////////////////////
            // __use__3.snippet.snippet.noop - __use__3.snippet.snippet.expr - __use__3.snippet.snippet.join
            ///////////////////////////////////////////////////////////////////

            void _mergeAlphaHandler(object message)
            {
                Increment();
                _joinAlphaPorts[0].Post(@"I did not understand you.");

                Decrement();
            }

            ///////////////////////////////////////////////////////////////////
            // __use__3.snippet.call - __use__3.snippet.call.iftype
            ///////////////////////////////////////////////////////////////////

            void _joinAlphaHandler(object[] args)
            {
                JoinAlpha message = new JoinAlpha(args);
                TexttoSpeechTTSPort.SayText((texttospeech.SayTextRequest)message);

                Decrement(args.Length);
            }

            class JoinAlpha
            {
                public string SpeechText;

                public JoinAlpha()
                {
                }

                public JoinAlpha(object[] args)
                {
                    SpeechText = (string)args[0];
                }

                public static explicit operator texttospeech.SayTextRequest(JoinAlpha join)
                {
                    texttospeech.SayTextRequest request = new texttospeech.SayTextRequest();

                    request.SpeechText = join.SpeechText;
                    return request;
                }
            }

            ccr.Port<object>[] _joinAlphaPorts = new ccr.Port<object>[1]{
                new ccr.Port<object>()
            };

            void RegisterAndActivateJoins()
            {
                // Register joins with base class to manage active count for incomplete joins.
                base.RegisterJoin(_joinAlphaPorts);
                // Activate Join handlers
                Activate(ccr.Arbiter.MultiplePortReceive(true, _joinAlphaPorts, _joinAlphaHandler));
            }
        }

        #endregion

        #region StartHandler class

        class StartHandler : HandlerBase
        {
            ///////////////////////////////////////////////////////////////////
            // program.activity.Start+start
            // [(expr - __use__.snippet0.snippet.noop - __use__.snippet0.snippet.expr - __use__.snippet0.snippet.join)] - __use__.snippet0.call - __use__.snippet0.call.iftype
            // [(__use__2.snippet0.snippet.noop - __use__2.snippet0.snippet.expr - __use__2.snippet0.snippet.join)] - __use__2.snippet0.call - __use__2.snippet0.call.iftype
            // [(__use__0.snippet0.snippet.noop - __use__0.snippet0.snippet.expr - __use__0.snippet0.snippet.join)] - __use__0.snippet0.call - __use__0.snippet0.call.iftype
            // snippet.element
            ///////////////////////////////////////////////////////////////////

            public StartHandler(DiagramService service, ccr.DispatcherQueue queue)
                : base(service, queue)
            {
            }

            ///////////////////////////////////////////////////////////////////
            // program.activity.Start+start
            // snippet.element
            // [(expr - __use__.snippet0.snippet.noop - __use__.snippet0.snippet.expr - __use__.snippet0.snippet.join)] - __use__.snippet0.call - __use__.snippet0.call.iftype
            ///////////////////////////////////////////////////////////////////

            public IEnumerator<ccr.ITask> RunHandler()
            {
                Increment();

                speechrecognizer.SetSrgsGrammarFileRequest request = new speechrecognizer.SetSrgsGrammarFileRequest();
                request.FileLocation = @"/samples/VPLExamples/Speech/DriveByVoice/MoveCommands.grxml";

                Increment();
                Activate(
                    ccr.Arbiter.Choice(
                        SpeechRecognizerPort.SetSrgsGrammarFile(request),
                        OnSetSrgsGrammarFileSuccess,
                        OnSetSrgsGrammarFileFault
                    )
                );

                Decrement();

                yield return WaitUntilComplete();
            }

            void OnSetSrgsGrammarFileSuccess(dssp.DefaultUpdateResponseType response)
            {
                texttospeech.SayTextRequest request = new texttospeech.SayTextRequest();
                request.SpeechText = @"I successfully loaded the speech grammar. What can I do for you now?";
                TexttoSpeechTTSPort.SayText(request);

                Decrement();
            }

            void OnSetSrgsGrammarFileFault(soap.Fault response)
            {
                texttospeech.SayTextRequest request = new texttospeech.SayTextRequest();
                request.SpeechText = @"Could not load the grammar file. The error is as follows: " + response.Reason[response.Reason.Length - 1].Value;
                TexttoSpeechTTSPort.SayText(request);

                Decrement();
            }

            class JoinAlpha
            {
                public string FileLocation;

                public JoinAlpha()
                {
                }

                public JoinAlpha(object[] args)
                {
                    FileLocation = args[0].ToString();
                }
            }

            ccr.Port<object>[] _joinAlphaPorts = new ccr.Port<object>[1]{
                new ccr.Port<object>()
            };

            class JoinBeta
            {
                public string SpeechText;

                public JoinBeta()
                {
                }

                public JoinBeta(object[] args)
                {
                    SpeechText = args[0].ToString();
                }
            }

            ccr.Port<object>[] _joinBetaPorts = new ccr.Port<object>[1]{
                new ccr.Port<object>()
            };

            ccr.Port<object>[] _secondJoinBetaPorts = new ccr.Port<object>[1]{
                new ccr.Port<object>()
            };
        }

        #endregion

        #region Handler utility base class

        class HandlerBase : ccr.CcrServiceBase
        {
            ccr.Port<ccr.EmptyValue> _complete = new ccr.Port<ccr.EmptyValue>();
            ccr.Port<ccr.EmptyValue> _shutdown = new ccr.Port<ccr.EmptyValue>();
            List<ccr.Port<object>[]> _joins = new List<ccr.Port<object>[]>();
            DiagramService _service;
            int _count;

            public HandlerBase(DiagramService service, ccr.DispatcherQueue queue)
                : base(queue)
            {
                _service = service;
            }

            protected DiagramState State
            {
                get { return _service._state; }
            }

            bool _stateChanged;

            public bool StateChanged
            {
                get { return _stateChanged; }
                set { _stateChanged = value; }
            }

            protected speechrecognizer.SpeechRecognizerOperations SpeechRecognizerPort
            {
                get { return _service._speechRecognizerPort; }
            }

            protected texttospeech.SpeechTextOperations TexttoSpeechTTSPort
            {
                get { return _service._texttoSpeechTTSPort; }
            }

            protected drive.DriveOperations Parallax2011ReferencePlatformIoControllerPort
            {
                get { return _service._parallax2011ReferencePlatformIoControllerPort; }
            }

            public ccr.Port<ccr.EmptyValue> Complete
            {
                get { return _complete; }
            }

            protected void Increment()
            {
                System.Threading.Interlocked.Increment(ref _count);
            }

            protected void Decrement()
            {
                int offset = 0;

                foreach (ccr.Port<object>[] join in _joins)
                {
                    bool complete = true;
                    int joinCount = 0;

                    foreach (ccr.Port<object> sink in join)
                    {
                        if (sink.ItemCount != 0)
                        {
                            joinCount += sink.ItemCount;
                        }
                        else
                        {
                            complete = false;
                        }
                    }
                    if (complete == false)
                    {
                        offset += joinCount;
                    }
                }

                if (System.Threading.Interlocked.Decrement(ref _count) <= offset)
                {
                    _shutdown.Post(ccr.EmptyValue.SharedInstance);
                    _complete.Post(ccr.EmptyValue.SharedInstance);
                }
            }

            protected void Decrement(int count)
            {
                for(int i = 0; i < count; i++)
                {
                    Decrement();
                }
            }

            protected void RegisterJoin(ccr.Port<object>[] join)
            {
                _joins.Add(join);
            }

            protected ccr.ITask WaitUntilComplete()
            {
                return ccr.Arbiter.Receive(false, _shutdown, NullDelegate);
            }

            protected static void NullDelegate(ccr.EmptyValue token)
            {
            }

            protected void SendNotification<T>(object notification)
                where T : dssp.DsspOperation, new()
            {
                _service.SendNotification<T>(_service._subMgr, notification);
            }

            protected void FaultHandler(soap.Fault fault, string msg)
            {
                _service.LogError(null, msg, fault);
            }

            protected static string Stringize(object obj)
            {
                if (obj == null)
                {
                    return string.Empty;
                }
                else
                {
                    return obj.ToString();
                }
            }

            protected void UnhandledResponse(W3C.Soap.Fault fault)
            {
                _service.LogError("Unhandled fault response from partner service", fault);
                Decrement();
            }

            protected void UnhandledResponse<T>(T response)
            {
                Decrement();
            }

            #region Type Cast Functions

            protected static bool BoolCast(object obj)
            {
                if (obj != null && obj is IConvertible)
                {
                    try
                    {
                        return ((IConvertible)obj).ToBoolean(System.Globalization.CultureInfo.InvariantCulture);
                    }
                    catch (FormatException)
                    {
                    }
                }
                return false;
            }

            protected static byte ByteCast(object obj)
            {
                if (obj != null && obj is IConvertible)
                {
                    try
                    {
                        return ((IConvertible)obj).ToByte(System.Globalization.CultureInfo.InvariantCulture);
                    }
                    catch (FormatException)
                    {
                    }
                }
                return 0;
            }

            protected static char CharCast(object obj)
            {
                if (obj != null && obj is IConvertible)
                {
                    try
                    {
                        return ((IConvertible)obj).ToChar(System.Globalization.CultureInfo.InvariantCulture);
                    }
                    catch (FormatException)
                    {
                    }
                }
                return '\0';
            }

            protected static decimal DecimalCast(object obj)
            {
                if (obj != null && obj is IConvertible)
                {
                    try
                    {
                        return ((IConvertible)obj).ToDecimal(System.Globalization.CultureInfo.InvariantCulture);
                    }
                    catch (FormatException)
                    {
                    }
                }
                return 0;
            }

            protected static double DoubleCast(object obj)
            {
                if (obj != null && obj is IConvertible)
                {
                    try
                    {
                        return ((IConvertible)obj).ToDouble(System.Globalization.CultureInfo.InvariantCulture);
                    }
                    catch (FormatException)
                    {
                    }
                }
                return 0;
            }

            protected static float FloatCast(object obj)
            {
                if (obj != null && obj is IConvertible)
                {
                    try
                    {
                        return ((IConvertible)obj).ToSingle(System.Globalization.CultureInfo.InvariantCulture);
                    }
                    catch (FormatException)
                    {
                    }
                }
                return 0;
            }

            protected static int IntCast(object obj)
            {
                if (obj != null && obj is IConvertible)
                {
                    try
                    {
                        return ((IConvertible)obj).ToInt32(System.Globalization.CultureInfo.InvariantCulture);
                    }
                    catch (FormatException)
                    {
                    }
                }
                return 0;
            }

            protected static long LongCast(object obj)
            {
                if (obj != null && obj is IConvertible)
                {
                    try
                    {
                        return ((IConvertible)obj).ToInt64(System.Globalization.CultureInfo.InvariantCulture);
                    }
                    catch (FormatException)
                    {
                    }
                }
                return 0;
            }

            protected static sbyte SByteCast(object obj)
            {
                if (obj != null && obj is IConvertible)
                {
                    try
                    {
                        return ((IConvertible)obj).ToSByte(System.Globalization.CultureInfo.InvariantCulture);
                    }
                    catch (FormatException)
                    {
                    }
                }
                return 0;
            }

            protected static short ShortCast(object obj)
            {
                if (obj != null && obj is IConvertible)
                {
                    try
                    {
                        return ((IConvertible)obj).ToInt16(System.Globalization.CultureInfo.InvariantCulture);
                    }
                    catch (FormatException)
                    {
                    }
                }
                return 0;
            }

            protected static uint UIntCast(object obj)
            {
                if (obj != null && obj is IConvertible)
                {
                    try
                    {
                        return ((IConvertible)obj).ToUInt32(System.Globalization.CultureInfo.InvariantCulture);
                    }
                    catch (FormatException)
                    {
                    }
                }
                return 0;
            }

            protected static ulong ULongCast(object obj)
            {
                if (obj != null && obj is IConvertible)
                {
                    try
                    {
                        return ((IConvertible)obj).ToUInt64(System.Globalization.CultureInfo.InvariantCulture);
                    }
                    catch (FormatException)
                    {
                    }
                }
                return 0;
            }

            protected static ushort UShortCast(object obj)
            {
                if (obj != null && obj is IConvertible)
                {
                    try
                    {
                        return ((IConvertible)obj).ToUInt16(System.Globalization.CultureInfo.InvariantCulture);
                    }
                    catch (FormatException)
                    {
                    }
                }
                return 0;
            }

            #endregion

            #region List Functions

            protected List<T> ListAdd<T>(List<T> list, T item)
            {
                List<T> output = new List<T>(list);
                output.Add(item);
                return output;
            }

            protected List<T> ListConcat<T>(List<T> head, List<T> tail)
            {
                List<T> output = new List<T>(head);
                output.AddRange(tail);
                return output;
            }

            protected int ListIndex<T>(List<T> list, T item)
            {
                return list.IndexOf(item);
            }

            protected List<T> ListRemove<T>(List<T> list, int index)
            {
                List<T> output = new List<T>(list);
                output.RemoveAt(index);
                return output;
            }

            protected List<T> ListReverse<T>(List<T> list)
            {
                List<T> output = new List<T>(list);
                output.Reverse();
                return output;
            }

            protected List<T> ListSort<T>(List<T> list)
            {
                List<T> output = new List<T>(list);
                output.Sort();
                return output;
            }

            protected List<T> ListInsert<T>(List<T> list, T item, int index)
            {
                List<T> output = new List<T>(list);
                output.Insert(index, item);
                return output;
            }

            #endregion
        }

        #endregion
    }
}
