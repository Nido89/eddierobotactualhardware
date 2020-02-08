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
using user = Microsoft.Robotics.Services.Sensors.Kinect.Proxy;
using dialog = Microsoft.Robotics.Services.Sample.Dialog.Proxy;
using flexdialog = Microsoft.Robotics.Services.Sample.FlexDialog.Proxy;
using drive = Microsoft.Robotics.Services.Drive.Proxy;
using timer = Microsoft.Robotics.Services.Sample.Timer.Proxy;

namespace Robotics.FollowMe.Diagram
{
    [DisplayName("FollowMe")]
    [Description("A user defined activity.")]
    [dssa.Contract(Contract.Identifier)]
    public class DiagramService : dssm.DsspServiceBase
    {
        // Service state
        [dssa.InitialStatePartner(Optional = true)]
        private DiagramState _state;

        // Service operations port
        [dssa.ServicePort("/FollowMe", AllowMultipleInstances = true)]
        private DiagramOperations _mainPort = new DiagramOperations();

        #region Partner services

        [dssa.Partner("SubMgr", Contract = submgr.Contract.Identifier, CreationPolicy = dssa.PartnerCreationPolicy.CreateAlways)]
        private submgr.SubscriptionManagerPort _subMgr = new submgr.SubscriptionManagerPort();

        // Partner: Timer, Contract: http://schemas.microsoft.com/robotics/2006/08/timer.html
        [dssa.Partner("Timer", Contract = timer.Contract.Identifier, CreationPolicy = dssa.PartnerCreationPolicy.UsePartnerListEntry)]
        timer.TimerOperations _timerPort = new timer.TimerOperations();
        timer.TimerOperations _timerNotify = new timer.TimerOperations();

        // Partner: UserKinect, Contract: http://schemas.microsoft.com/robotics/2011/08/kinect.user.html
        [dssa.Partner("UserKinect", Contract = user.Contract.Identifier, CreationPolicy = dssa.PartnerCreationPolicy.UsePartnerListEntry)]
        user.KinectOperations _userKinectPort = new user.KinectOperations();

        // Partner: SimpleDialog, Contract: http://schemas.microsoft.com/robotics/2006/08/dialog.html
        [dssa.Partner("SimpleDialog", Contract = dialog.Contract.Identifier, CreationPolicy = dssa.PartnerCreationPolicy.UsePartnerListEntry)]
        dialog.DialogOperations _simpleDialogPort = new dialog.DialogOperations();

        // Partner: FlexibleDialog, Contract: http://schemas.microsoft.com/robotics/2007/08/flexdialog.html
        [dssa.Partner("FlexibleDialog", Contract = flexdialog.Contract.Identifier, CreationPolicy = dssa.PartnerCreationPolicy.UsePartnerListEntry)]
        flexdialog.FlexDialogOperations _flexibleDialogPort = new flexdialog.FlexDialogOperations();

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
                _timerPort.Subscribe(_timerNotify,                
                    typeof(Microsoft.Robotics.Services.Sample.Timer.Proxy.FireTimer)
                ),
                EmptyHandler,
                delegate(soap.Fault f)
                {
                    fault = f;
                }
            );

            if (fault != null)
            {
                LogError("Failed to subscribe to program.TimerService.Timer", fault);
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
                ccr.Arbiter.ReceiveWithIterator<Microsoft.Robotics.Services.Sample.Timer.Proxy.FireTimer>(true, _timerNotify, TimerFireTimerHandler)
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

        IEnumerator<ccr.ITask> TimerFireTimerHandler(Microsoft.Robotics.Services.Sample.Timer.Proxy.FireTimer message)
        {
            OnTimerFireTimerHandler handler = new OnTimerFireTimerHandler(this, Environment.TaskQueue);
            return handler.RunHandler(message);
        }

        #endregion

        #region OnTimerFireTimerHandler class

        class OnTimerFireTimerHandler : HandlerBase
        {
            ///////////////////////////////////////////////////////////////////
            // __use__.snippet0.snippet.noop - [(__use__.snippet0.snippet.expr1 - __use__.snippet0.snippet.join)(__use__.snippet0.snippet.expr0 - &__use__.snippet0.snippet.join)(__use__.snippet0.snippet.expr - &__use__.snippet0.snippet.join)] - __use__.snippet0.call - __use__.snippet0.call.iftype
            // [(__use__100.snippet0.snippet.noop - __use__100.snippet0.snippet.expr - __use__100.snippet0.snippet.join)] - __use__100.snippet0.call - __use__100.snippet0.call.iftype
            // [(__use__01.snippet0.snippet.noop - __use__01.snippet0.snippet.expr - __use__01.snippet0.snippet.join)] - __use__01.snippet0.call - __use__01.snippet0.call.iftype
            // expr0
            // if if{&join}else{&join6}
            // join
            // &element
            // expr00
            // if0 if{&join0}else{&join6}
            // join0
            // &element
            // expr000
            // if00 if{&join1}else{&join6}
            // join1
            // &element
            // expr0000
            // if000 if{&join2}else{&join6}
            // join2
            // &element
            // expr00000
            // if0000 if{&join3}else{&join6}
            // join3
            // &element
            // expr00001
            // if0001 if{&join4}else{join6}
            // __use__3.snippet.snippet.noop - [(__use__3.snippet.snippet.expr0 - __use__3.snippet.snippet.join)(__use__3.snippet.snippet.expr - &__use__3.snippet.snippet.join)] - __use__3.snippet.call - __use__3.snippet.call.iftype
            // join4
            // element
            // expr1 - &join5
            // expr2 - join5
            // if1 if{__use__33.snippet.snippet.noop - [(__use__33.snippet.snippet.expr0 - __use__33.snippet.snippet.join)(__use__33.snippet.snippet.expr - &__use__33.snippet.snippet.join)] - __use__33.snippet.call - __use__33.snippet.call.iftype}elseif{__use__31.snippet.snippet.noop - [(__use__31.snippet.snippet.expr0 - __use__31.snippet.snippet.join)(__use__31.snippet.snippet.expr - &__use__31.snippet.snippet.join)] - __use__31.snippet.call - __use__31.snippet.call.iftype}elseif{__use__30.snippet.snippet.noop - [(__use__30.snippet.snippet.expr0 - __use__30.snippet.snippet.join)(__use__30.snippet.snippet.expr - &__use__30.snippet.snippet.join)] - __use__30.snippet.call - __use__30.snippet.call.iftype}elseif{__use__32.snippet.snippet.noop - [(__use__32.snippet.snippet.expr0 - __use__32.snippet.snippet.join)(__use__32.snippet.snippet.expr - &__use__32.snippet.snippet.join)] - __use__32.snippet.call - __use__32.snippet.call.iftype}else{__use__34.snippet.snippet.noop - [(__use__34.snippet.snippet.expr0 - __use__34.snippet.snippet.join)(__use__34.snippet.snippet.expr - &__use__34.snippet.snippet.join)] - __use__34.snippet.call - __use__34.snippet.call.iftype}
            // __use__1.snippet.snippet.noop - [(__use__1.snippet.snippet.expr2 - __use__1.snippet.snippet.join)(__use__1.snippet.snippet.expr1 - &__use__1.snippet.snippet.join)(__use__1.snippet.snippet.expr0 - &__use__1.snippet.snippet.join)(__use__1.snippet.snippet.expr - &__use__1.snippet.snippet.join)] - __use__1.snippet.call - __use__1.snippet.call.iftype
            ///////////////////////////////////////////////////////////////////

            public OnTimerFireTimerHandler(DiagramService service, ccr.DispatcherQueue queue)
                : base(service, queue)
            {

                // Activate merge handlers
                Activate(ccr.Arbiter.Receive(true, _mergeAlpha, _mergeAlphaHandler));

                RegisterAndActivateJoins();
            }

            ///////////////////////////////////////////////////////////////////
            // __use__.snippet0.snippet.noop - [(__use__.snippet0.snippet.expr1 - __use__.snippet0.snippet.join)(__use__.snippet0.snippet.expr0 - &__use__.snippet0.snippet.join)(__use__.snippet0.snippet.expr - &__use__.snippet0.snippet.join)] - __use__.snippet0.call - __use__.snippet0.call.iftype
            ///////////////////////////////////////////////////////////////////

            public IEnumerator<ccr.ITask> RunHandler(Microsoft.Robotics.Services.Sample.Timer.Proxy.FireTimer message)
            {
                Increment();

                user.QueryRawFrameRequest request = new user.QueryRawFrameRequest();
                request.IncludeSkeletons = true;
                request.IncludeVideo = false;
                request.IncludeDepth = false;

                Increment();
                Activate(
                    ccr.Arbiter.Choice(
                        UserKinectPort.QueryRawFrame(request),
                        OnQueryRawFrameSuccess,
                        OnQueryRawFrameFault
                    )
                );

                Decrement();

                yield return WaitUntilComplete();
            }

            ///////////////////////////////////////////////////////////////////
            // element
            ///////////////////////////////////////////////////////////////////

            void _joinAlphaHandler(object[] args)
            {
                JoinAlpha message = new JoinAlpha(args);

                Increment();
                _mergeAlpha.Post(message);

                Decrement(args.Length);
            }

            ///////////////////////////////////////////////////////////////////
            // __use__1.snippet.snippet.noop - [(__use__1.snippet.snippet.expr2 - __use__1.snippet.snippet.join)(__use__1.snippet.snippet.expr1 - &__use__1.snippet.snippet.join)(__use__1.snippet.snippet.expr0 - &__use__1.snippet.snippet.join)(__use__1.snippet.snippet.expr - &__use__1.snippet.snippet.join)] - __use__1.snippet.call - __use__1.snippet.call.iftype
            // if1 if{__use__33.snippet.snippet.noop - [(__use__33.snippet.snippet.expr0 - __use__33.snippet.snippet.join)(__use__33.snippet.snippet.expr - &__use__33.snippet.snippet.join)] - __use__33.snippet.call - __use__33.snippet.call.iftype}elseif{__use__31.snippet.snippet.noop - [(__use__31.snippet.snippet.expr0 - __use__31.snippet.snippet.join)(__use__31.snippet.snippet.expr - &__use__31.snippet.snippet.join)] - __use__31.snippet.call - __use__31.snippet.call.iftype}elseif{__use__30.snippet.snippet.noop - [(__use__30.snippet.snippet.expr0 - __use__30.snippet.snippet.join)(__use__30.snippet.snippet.expr - &__use__30.snippet.snippet.join)] - __use__30.snippet.call - __use__30.snippet.call.iftype}elseif{__use__32.snippet.snippet.noop - [(__use__32.snippet.snippet.expr0 - __use__32.snippet.snippet.join)(__use__32.snippet.snippet.expr - &__use__32.snippet.snippet.join)] - __use__32.snippet.call - __use__32.snippet.call.iftype}else{__use__34.snippet.snippet.noop - [(__use__34.snippet.snippet.expr0 - __use__34.snippet.snippet.join)(__use__34.snippet.snippet.expr - &__use__34.snippet.snippet.join)] - __use__34.snippet.call - __use__34.snippet.call.iftype}
            ///////////////////////////////////////////////////////////////////

            void _joinBetaHandler(object[] args)
            {
                JoinBeta message = new JoinBeta(args);
                flexdialog.FlexControl request = new flexdialog.FlexControl();
                request.Value = ((@"X:" + message.X) + @"  Z:") + message.Z;
                request.ControlType = Microsoft.Robotics.Services.Sample.FlexDialog.Proxy.FlexControlType.TextBox;
                request.Id = @"Position";
                FlexibleDialogPort.UpdateControl(request);

                if (message.X > 0.5D)
                {
                    drive.SetDrivePowerRequest requestA = new drive.SetDrivePowerRequest();
                    requestA.RightWheelPower = (double)0.1F;
                    requestA.LeftWheelPower = (double)-0.1F;

                    Increment();
                    Activate(
                        ccr.Arbiter.Choice(
                            Parallax2011ReferencePlatformIoControllerPort.SetDrivePower(requestA),
                            OnSetDrivePowerSuccess,
                            delegate(soap.Fault fault)
                            {
                                base.FaultHandler(fault, @"Parallax2011ReferencePlatformIoControllerPort.SetDrivePower(requestA)");
                                Decrement();
                            }
                        )
                    );
                }
                else if (message.X < -0.5D)
                {
                    drive.SetDrivePowerRequest requestB = new drive.SetDrivePowerRequest();
                    requestB.RightWheelPower = (double)-0.1F;
                    requestB.LeftWheelPower = (double)0.1F;

                    Increment();
                    Activate(
                        ccr.Arbiter.Choice(
                            Parallax2011ReferencePlatformIoControllerPort.SetDrivePower(requestB),
                            OnSetDrivePower1Success,
                            delegate(soap.Fault fault)
                            {
                                base.FaultHandler(fault, @"Parallax2011ReferencePlatformIoControllerPort.SetDrivePower(requestB)");
                                Decrement();
                            }
                        )
                    );
                }
                else if (message.Z > 1D)
                {
                    drive.SetDrivePowerRequest requestC = new drive.SetDrivePowerRequest();
                    requestC.RightWheelPower = (double)0.1F;
                    requestC.LeftWheelPower = (double)0.1F;

                    Increment();
                    Activate(
                        ccr.Arbiter.Choice(
                            Parallax2011ReferencePlatformIoControllerPort.SetDrivePower(requestC),
                            OnSetDrivePower2Success,
                            delegate(soap.Fault fault)
                            {
                                base.FaultHandler(fault, @"Parallax2011ReferencePlatformIoControllerPort.SetDrivePower(requestC)");
                                Decrement();
                            }
                        )
                    );
                }
                else if (message.Z < 1.5D)
                {
                    drive.SetDrivePowerRequest requestD = new drive.SetDrivePowerRequest();
                    requestD.RightWheelPower = (double)-0.1F;
                    requestD.LeftWheelPower = (double)-0.1F;

                    Increment();
                    Activate(
                        ccr.Arbiter.Choice(
                            Parallax2011ReferencePlatformIoControllerPort.SetDrivePower(requestD),
                            OnSetDrivePower3Success,
                            delegate(soap.Fault fault)
                            {
                                base.FaultHandler(fault, @"Parallax2011ReferencePlatformIoControllerPort.SetDrivePower(requestD)");
                                Decrement();
                            }
                        )
                    );
                }
                else
                {
                    drive.SetDrivePowerRequest requestE = new drive.SetDrivePowerRequest();
                    requestE.RightWheelPower = (double)0;
                    requestE.LeftWheelPower = (double)0;

                    Increment();
                    Activate(
                        ccr.Arbiter.Choice(
                            Parallax2011ReferencePlatformIoControllerPort.SetDrivePower(requestE),
                            OnSetDrivePower4Success,
                            delegate(soap.Fault fault)
                            {
                                base.FaultHandler(fault, @"Parallax2011ReferencePlatformIoControllerPort.SetDrivePower(requestE)");
                                Decrement();
                            }
                        )
                    );
                }

                Decrement(args.Length);
            }

            ///////////////////////////////////////////////////////////////////
            // __use__3.snippet.snippet.noop - [(__use__3.snippet.snippet.expr0 - __use__3.snippet.snippet.join)(__use__3.snippet.snippet.expr - &__use__3.snippet.snippet.join)] - __use__3.snippet.call - __use__3.snippet.call.iftype
            ///////////////////////////////////////////////////////////////////

            void _joinGammaHandler(object[] args)
            {
                JoinGamma message = new JoinGamma(args);
                drive.SetDrivePowerRequest request = new drive.SetDrivePowerRequest();
                request.RightWheelPower = (double)0.1F;
                request.LeftWheelPower = (double)-0.1F;
                Parallax2011ReferencePlatformIoControllerPort.SetDrivePower(request);

                Decrement(args.Length);
            }

            ///////////////////////////////////////////////////////////////////
            ///////////////////////////////////////////////////////////////////

            void _secondJoinAlphaHandler(object[] args)
            {
                JoinAlpha message = new JoinAlpha(args);
                Decrement(args.Length);
            }

            ///////////////////////////////////////////////////////////////////
            ///////////////////////////////////////////////////////////////////

            void _thirdJoinAlphaHandler(object[] args)
            {
                JoinAlpha message = new JoinAlpha(args);
                Decrement(args.Length);
            }

            ///////////////////////////////////////////////////////////////////
            ///////////////////////////////////////////////////////////////////

            void _fourthJoinAlphaHandler(object[] args)
            {
                JoinAlpha message = new JoinAlpha(args);
                Decrement(args.Length);
            }

            ///////////////////////////////////////////////////////////////////
            ///////////////////////////////////////////////////////////////////

            void _fifthJoinAlphaHandler(object[] args)
            {
                JoinAlpha message = new JoinAlpha(args);
                Decrement(args.Length);
            }

            ///////////////////////////////////////////////////////////////////
            // &element
            ///////////////////////////////////////////////////////////////////

            void _sixthJoinAlphaHandler(object[] args)
            {
                JoinAlpha message = new JoinAlpha(args);

                Increment();
                _mergeAlpha.Post(message);

                Decrement(args.Length);
            }

            void OnQueryRawFrameSuccess(user.GetRawFrameResponse response)
            {

                Increment();
                _joinAlphaPorts[1].Post(response.RawFrames.RawSkeletonFrameData.SkeletonData[5]);

                if (response.RawFrames.RawSkeletonFrameData.SkeletonData[5].TrackingState == Microsoft.Kinect.SkeletonTrackingState.Tracked)
                {
                    Increment();
                    _joinAlphaPorts[0].Post(response.RawFrames.RawSkeletonFrameData.SkeletonData[5]);
                }
                else
                {
                    Increment();
                    _joinGammaPorts[5].Post(response.RawFrames.RawSkeletonFrameData.SkeletonData[5]);
                }

                Increment();
                _secondJoinAlphaPorts[1].Post(response.RawFrames.RawSkeletonFrameData.SkeletonData[4]);

                if (response.RawFrames.RawSkeletonFrameData.SkeletonData[4].TrackingState == Microsoft.Kinect.SkeletonTrackingState.Tracked)
                {
                    Increment();
                    _secondJoinAlphaPorts[0].Post(response.RawFrames.RawSkeletonFrameData.SkeletonData[4]);
                }
                else
                {
                    Increment();
                    _joinGammaPorts[4].Post(response.RawFrames.RawSkeletonFrameData.SkeletonData[4]);
                }

                Increment();
                _thirdJoinAlphaPorts[1].Post(response.RawFrames.RawSkeletonFrameData.SkeletonData[3]);

                if (response.RawFrames.RawSkeletonFrameData.SkeletonData[3].TrackingState == Microsoft.Kinect.SkeletonTrackingState.Tracked)
                {
                    Increment();
                    _thirdJoinAlphaPorts[0].Post(response.RawFrames.RawSkeletonFrameData.SkeletonData[3]);
                }
                else
                {
                    Increment();
                    _joinGammaPorts[3].Post(response.RawFrames.RawSkeletonFrameData.SkeletonData[3]);
                }

                Increment();
                _fourthJoinAlphaPorts[1].Post(response.RawFrames.RawSkeletonFrameData.SkeletonData[2]);

                if (response.RawFrames.RawSkeletonFrameData.SkeletonData[2].TrackingState == Microsoft.Kinect.SkeletonTrackingState.Tracked)
                {
                    Increment();
                    _fourthJoinAlphaPorts[0].Post(response.RawFrames.RawSkeletonFrameData.SkeletonData[2]);
                }
                else
                {
                    Increment();
                    _joinGammaPorts[2].Post(response.RawFrames.RawSkeletonFrameData.SkeletonData[2]);
                }

                Increment();
                _fifthJoinAlphaPorts[1].Post(response.RawFrames.RawSkeletonFrameData.SkeletonData[1]);

                if (response.RawFrames.RawSkeletonFrameData.SkeletonData[1].TrackingState == Microsoft.Kinect.SkeletonTrackingState.Tracked)
                {
                    Increment();
                    _fifthJoinAlphaPorts[0].Post(response.RawFrames.RawSkeletonFrameData.SkeletonData[1]);
                }
                else
                {
                    Increment();
                    _joinGammaPorts[1].Post(response.RawFrames.RawSkeletonFrameData.SkeletonData[1]);
                }

                timer.SetTimerRequest request = new timer.SetTimerRequest();
                request.Interval = 300;
                TimerPort.SetTimer(request);

                Increment();
                _sixthJoinAlphaPorts[1].Post(response.RawFrames.RawSkeletonFrameData.SkeletonData[0]);

                if (response.RawFrames.RawSkeletonFrameData.SkeletonData[0].TrackingState == Microsoft.Kinect.SkeletonTrackingState.Tracked)
                {
                    Increment();
                    _sixthJoinAlphaPorts[0].Post(response.RawFrames.RawSkeletonFrameData.SkeletonData[0]);
                }
                else
                {
                    Increment();
                    _joinGammaPorts[0].Post(response.RawFrames.RawSkeletonFrameData.SkeletonData[0]);
                }

                Decrement();
            }

            void OnQueryRawFrameFault(soap.Fault response)
            {
                dialog.AlertRequest request = new dialog.AlertRequest();
                request.Message = Stringize(response.Code);
                SimpleDialogPort.Alert(request);

                Decrement();
            }

            void OnSetDrivePowerSuccess(dssp.DefaultUpdateResponseType response)
            {
                Decrement();
            }

            void OnSetDrivePower1Success(dssp.DefaultUpdateResponseType response)
            {
                Decrement();
            }

            void OnSetDrivePower2Success(dssp.DefaultUpdateResponseType response)
            {
                Decrement();
            }

            void OnSetDrivePower3Success(dssp.DefaultUpdateResponseType response)
            {
                Decrement();
            }

            void OnSetDrivePower4Success(dssp.DefaultUpdateResponseType response)
            {
                Decrement();
            }

            ccr.Port<JoinAlpha> _mergeAlpha = new ccr.Port<JoinAlpha>();

            ///////////////////////////////////////////////////////////////////
            // expr2 - join5
            // expr1 - &join5
            ///////////////////////////////////////////////////////////////////

            void _mergeAlphaHandler(JoinAlpha message)
            {
                Increment();
                _joinBetaPorts[1].Post(message.SkeletonData.Position.Z);

                Increment();
                _joinBetaPorts[0].Post(message.SkeletonData.Position.X);

                Decrement();
            }

            class JoinDelta
            {
                public string IncludeDepth;
                public string IncludeVideo;
                public string IncludeSkeletons;

                public JoinDelta()
                {
                }

                public JoinDelta(object[] args)
                {
                    IncludeDepth = args[0].ToString();
                    IncludeVideo = args[1].ToString();
                    IncludeSkeletons = args[2].ToString();
                }
            }

            ccr.Port<object>[] _joinDeltaPorts = new ccr.Port<object>[3]{
                new ccr.Port<object>(),
                new ccr.Port<object>(),
                new ccr.Port<object>()
            };

            class JoinEpsilon
            {
                public string Message;

                public JoinEpsilon()
                {
                }

                public JoinEpsilon(object[] args)
                {
                    Message = args[0].ToString();
                }
            }

            ccr.Port<object>[] _joinEpsilonPorts = new ccr.Port<object>[1]{
                new ccr.Port<object>()
            };

            class JoinAlpha
            {
                public Microsoft.Kinect.Skeleton TrackingState;
                public Microsoft.Kinect.Skeleton SkeletonData;

                public JoinAlpha()
                {
                }

                public JoinAlpha(object[] args)
                {
                    TrackingState = (Microsoft.Kinect.Skeleton)args[0];
                    SkeletonData = (Microsoft.Kinect.Skeleton)args[1];
                }
            }

            ccr.Port<object>[] _joinAlphaPorts = new ccr.Port<object>[2]{
                new ccr.Port<object>(),
                new ccr.Port<object>()
            };

            class JoinBeta
            {
                public float X;
                public float Z;

                public JoinBeta()
                {
                }

                public JoinBeta(object[] args)
                {
                    X = (float)args[0];
                    Z = (float)args[1];
                }
            }

            ccr.Port<object>[] _joinBetaPorts = new ccr.Port<object>[2]{
                new ccr.Port<object>(),
                new ccr.Port<object>()
            };

            class JoinZeta
            {
                public string Id;
                public string ControlType;
                public string Text;
                public string Value;

                public JoinZeta()
                {
                }

                public JoinZeta(object[] args)
                {
                    Id = args[0].ToString();
                    ControlType = args[1].ToString();
                    Text = args[2].ToString();
                    Value = args[3].ToString();
                }
            }

            ccr.Port<object>[] _joinZetaPorts = new ccr.Port<object>[4]{
                new ccr.Port<object>(),
                new ccr.Port<object>(),
                new ccr.Port<object>(),
                new ccr.Port<object>()
            };

            class JoinEta
            {
                public string LeftWheelPower;
                public string RightWheelPower;

                public JoinEta()
                {
                }

                public JoinEta(object[] args)
                {
                    LeftWheelPower = args[0].ToString();
                    RightWheelPower = args[1].ToString();
                }
            }

            ccr.Port<object>[] _joinEtaPorts = new ccr.Port<object>[2]{
                new ccr.Port<object>(),
                new ccr.Port<object>()
            };

            class JoinTheta
            {
                public string LeftWheelPower;
                public string RightWheelPower;

                public JoinTheta()
                {
                }

                public JoinTheta(object[] args)
                {
                    LeftWheelPower = args[0].ToString();
                    RightWheelPower = args[1].ToString();
                }
            }

            ccr.Port<object>[] _joinThetaPorts = new ccr.Port<object>[2]{
                new ccr.Port<object>(),
                new ccr.Port<object>()
            };

            ccr.Port<object>[] _secondJoinThetaPorts = new ccr.Port<object>[2]{
                new ccr.Port<object>(),
                new ccr.Port<object>()
            };

            ccr.Port<object>[] _thirdJoinThetaPorts = new ccr.Port<object>[2]{
                new ccr.Port<object>(),
                new ccr.Port<object>()
            };

            ccr.Port<object>[] _fourthJoinThetaPorts = new ccr.Port<object>[2]{
                new ccr.Port<object>(),
                new ccr.Port<object>()
            };

            class JoinGamma
            {
                public Microsoft.Kinect.Skeleton msg;
                public Microsoft.Kinect.Skeleton msg0;
                public Microsoft.Kinect.Skeleton msg1;
                public Microsoft.Kinect.Skeleton msg2;
                public Microsoft.Kinect.Skeleton msg3;
                public Microsoft.Kinect.Skeleton msg4;

                public JoinGamma()
                {
                }

                public JoinGamma(object[] args)
                {
                    msg = (Microsoft.Kinect.Skeleton)args[0];
                    msg0 = (Microsoft.Kinect.Skeleton)args[1];
                    msg1 = (Microsoft.Kinect.Skeleton)args[2];
                    msg2 = (Microsoft.Kinect.Skeleton)args[3];
                    msg3 = (Microsoft.Kinect.Skeleton)args[4];
                    msg4 = (Microsoft.Kinect.Skeleton)args[5];
                }
            }

            ccr.Port<object>[] _joinGammaPorts = new ccr.Port<object>[6]{
                new ccr.Port<object>(),
                new ccr.Port<object>(),
                new ccr.Port<object>(),
                new ccr.Port<object>(),
                new ccr.Port<object>(),
                new ccr.Port<object>()
            };

            ccr.Port<object>[] _fifthJoinThetaPorts = new ccr.Port<object>[2]{
                new ccr.Port<object>(),
                new ccr.Port<object>()
            };

            ccr.Port<object>[] _secondJoinAlphaPorts = new ccr.Port<object>[2]{
                new ccr.Port<object>(),
                new ccr.Port<object>()
            };

            ccr.Port<object>[] _thirdJoinAlphaPorts = new ccr.Port<object>[2]{
                new ccr.Port<object>(),
                new ccr.Port<object>()
            };

            ccr.Port<object>[] _fourthJoinAlphaPorts = new ccr.Port<object>[2]{
                new ccr.Port<object>(),
                new ccr.Port<object>()
            };

            ccr.Port<object>[] _fifthJoinAlphaPorts = new ccr.Port<object>[2]{
                new ccr.Port<object>(),
                new ccr.Port<object>()
            };

            class JoinIota
            {
                public string Interval;

                public JoinIota()
                {
                }

                public JoinIota(object[] args)
                {
                    Interval = args[0].ToString();
                }
            }

            ccr.Port<object>[] _joinIotaPorts = new ccr.Port<object>[1]{
                new ccr.Port<object>()
            };

            ccr.Port<object>[] _sixthJoinAlphaPorts = new ccr.Port<object>[2]{
                new ccr.Port<object>(),
                new ccr.Port<object>()
            };

            void RegisterAndActivateJoins()
            {
                // Register joins with base class to manage active count for incomplete joins.
                base.RegisterJoin(_joinAlphaPorts);
                base.RegisterJoin(_joinBetaPorts);
                base.RegisterJoin(_joinGammaPorts);
                base.RegisterJoin(_secondJoinAlphaPorts);
                base.RegisterJoin(_thirdJoinAlphaPorts);
                base.RegisterJoin(_fourthJoinAlphaPorts);
                base.RegisterJoin(_fifthJoinAlphaPorts);
                base.RegisterJoin(_sixthJoinAlphaPorts);
                // Activate Join handlers
                Activate(ccr.Arbiter.MultiplePortReceive(true, _joinAlphaPorts, _joinAlphaHandler));
                Activate(ccr.Arbiter.MultiplePortReceive(true, _joinBetaPorts, _joinBetaHandler));
                Activate(ccr.Arbiter.MultiplePortReceive(true, _joinGammaPorts, _joinGammaHandler));
                Activate(ccr.Arbiter.MultiplePortReceive(true, _secondJoinAlphaPorts, _secondJoinAlphaHandler));
                Activate(ccr.Arbiter.MultiplePortReceive(true, _thirdJoinAlphaPorts, _thirdJoinAlphaHandler));
                Activate(ccr.Arbiter.MultiplePortReceive(true, _fourthJoinAlphaPorts, _fourthJoinAlphaHandler));
                Activate(ccr.Arbiter.MultiplePortReceive(true, _fifthJoinAlphaPorts, _fifthJoinAlphaHandler));
                Activate(ccr.Arbiter.MultiplePortReceive(true, _sixthJoinAlphaPorts, _sixthJoinAlphaHandler));
            }
        }

        #endregion

        #region StartHandler class

        class StartHandler : HandlerBase
        {
            ///////////////////////////////////////////////////////////////////
            // program.activity.Start+start
            // [(expr - __use__0.snippet0.snippet.noop - __use__0.snippet0.snippet.expr - __use__0.snippet0.snippet.join)] - __use__0.snippet0.call - __use__0.snippet0.call.iftype
            // [(expr3 - __use__35.snippet0.snippet.noop - __use__35.snippet0.snippet.expr - __use__35.snippet0.snippet.join)] - __use__35.snippet0.call - __use__35.snippet0.call.iftype
            // snippet.element
            ///////////////////////////////////////////////////////////////////

            public StartHandler(DiagramService service, ccr.DispatcherQueue queue)
                : base(service, queue)
            {
            }

            ///////////////////////////////////////////////////////////////////
            // program.activity.Start+start
            // snippet.element
            // [(expr3 - __use__35.snippet0.snippet.noop - __use__35.snippet0.snippet.expr - __use__35.snippet0.snippet.join)] - __use__35.snippet0.call - __use__35.snippet0.call.iftype
            // [(expr - __use__0.snippet0.snippet.noop - __use__0.snippet0.snippet.expr - __use__0.snippet0.snippet.join)] - __use__0.snippet0.call - __use__0.snippet0.call.iftype
            ///////////////////////////////////////////////////////////////////

            public IEnumerator<ccr.ITask> RunHandler()
            {
                Increment();

                drive.EnableDriveRequest request = new drive.EnableDriveRequest();
                request.Enable = true;
                Parallax2011ReferencePlatformIoControllerPort.EnableDrive(request);

                timer.SetTimerRequest requestA = new timer.SetTimerRequest();
                requestA.Interval = 5000;
                TimerPort.SetTimer(requestA);

                Decrement();

                yield return WaitUntilComplete();
            }

            class JoinAlpha
            {
                public string Enable;

                public JoinAlpha()
                {
                }

                public JoinAlpha(object[] args)
                {
                    Enable = args[0].ToString();
                }
            }

            ccr.Port<object>[] _joinAlphaPorts = new ccr.Port<object>[1]{
                new ccr.Port<object>()
            };

            class JoinBeta
            {
                public string Interval;

                public JoinBeta()
                {
                }

                public JoinBeta(object[] args)
                {
                    Interval = args[0].ToString();
                }
            }

            ccr.Port<object>[] _joinBetaPorts = new ccr.Port<object>[1]{
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

            protected timer.TimerOperations TimerPort
            {
                get { return _service._timerPort; }
            }

            protected user.KinectOperations UserKinectPort
            {
                get { return _service._userKinectPort; }
            }

            protected dialog.DialogOperations SimpleDialogPort
            {
                get { return _service._simpleDialogPort; }
            }

            protected flexdialog.FlexDialogOperations FlexibleDialogPort
            {
                get { return _service._flexibleDialogPort; }
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
