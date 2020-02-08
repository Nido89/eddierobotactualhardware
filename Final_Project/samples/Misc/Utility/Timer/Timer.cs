//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: Timer.cs $ $Revision: 19 $
//-----------------------------------------------------------------------
using Microsoft.Ccr.Core;
using Microsoft.Dss.Core;
using Microsoft.Dss.Core.Attributes;
using Microsoft.Dss.ServiceModel.Dssp;
using Microsoft.Dss.ServiceModel.DsspServiceBase;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Security.Permissions;
using xml = System.Xml;
using sm = Microsoft.Dss.Services.SubscriptionManager;
using System.Text;
using System.Globalization;


namespace Microsoft.Robotics.Services.Sample.Timer
{
    /// <summary>
    /// TimerService - Time delay operations
    /// </summary>
    [DisplayName("(User) Timer")]
    [Description("Provides simple timer support.")]
    [Contract(Contract.Identifier)]
    [DssServiceDescription("http://msdn.microsoft.com/library/dd126875.aspx")]
    public class TimerService : DsspServiceBase
    {
        const int _tickInterval = 100;
        const int _tickFilter = 10;
        const int _waitLimit = 60000;

        [ServiceState]
        private TimerState _state = new TimerState();

        [ServicePort("/timer", AllowMultipleInstances = true)]
        private TimerOperations _mainPort = new TimerOperations();

        Port<DateTime> _timerPort = new Port<DateTime>();

        [Partner("SubMgr", Contract = sm.Contract.Identifier, CreationPolicy = PartnerCreationPolicy.CreateAlways, Optional = false)]
        sm.SubscriptionManagerPort _subMgr = new sm.SubscriptionManagerPort();

        /// <summary>
        /// Default Service Constructor
        /// </summary>
        public TimerService(DsspServiceCreationPort creationPort) :
                base(creationPort)
        {
        }

        /// <summary>
        /// Service Start
        /// </summary>
        protected override void Start()
        {
            base.Start();

            _timerPort.Post(DateTime.Now);
            Activate(
                Arbiter.Receive(true, _timerPort, TimerHandler)
            );
        }

        void TimerHandler(DateTime signal)
        {
            Activate(
                Arbiter.Receive(false, TimeoutPort(_tickInterval),
                    delegate(DateTime time)
                    {
                        _timerPort.Post(time);
                    }
                )
            );

            _mainPort.Post(new Tick(new TickRequest()));
        }

        /// <summary>
        /// SubscribeHandler - Process subscription requests
        /// </summary>
        /// <param name="subscribe"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public virtual IEnumerator<ITask> SubscribeHandler(Subscribe subscribe)
        {
            SubscribeHelper(_subMgr, subscribe.Body, subscribe.ResponsePort);
            yield break;
        }

        /// <summary>
        /// SetTimerHandler - Set a timer
        /// </summary>
        /// <param name="setTimer"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public virtual IEnumerator<ITask> SetTimerHandler(SetTimer setTimer)
        {
            MessageTimestamp timeStamp = setTimer.GetHeader<MessageTimestamp>();
            DateTime start;

            if (timeStamp == null)
            {
                start = DateTime.Now;
            }
            else
            {
                start = timeStamp.Value;
            }

            StringBuilder log = new StringBuilder();

            if (_state.Timeout > 0 && _state.Expires > DateTime.Now)
            {
                log.AppendFormat(
                    CultureInfo.InvariantCulture,
                    "Override previous timer of {0}ms, expected at {1}.",
                    _state.Timeout,
                    _state.Expires
                );
#if URT_MINCLR
                log.Append("\n");
#else
                log.AppendLine();
#endif
            }

            if (setTimer.Body.Interval > 0)
            {
                _state.Timeout = setTimer.Body.Interval;
                _state.Expires = start.AddMilliseconds(_state.Timeout);

                log.AppendFormat(
                    CultureInfo.InvariantCulture,
                    "Set new timer of {0}ms, expected at {1}.",
                    _state.Timeout,
                    _state.Expires
                );
            }
            else
            {
                _state.Timeout = 0;
                _state.Expires = DateTime.MaxValue;

                log.Append("Timer stopped");
            }
            LogVerbose(log.ToString());

            setTimer.ResponsePort.Post(DefaultUpdateResponseType.Instance);

            SendNotification(_subMgr, setTimer);
            yield break;
        }

        /// <summary>
        /// TickHandler - Send notification on tick count
        /// </summary>
        /// <param name="tick"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public virtual IEnumerator<ITask> TickHandler(Tick tick)
        {
            if (_state.Timeout > 0 &&
                _state.Expires <= DateTime.Now.AddMilliseconds(_tickInterval / 2.0))
            {
                LogVerbose("Firing Timer due at " + _state.Expires);

                FireTimerRequest request = new FireTimerRequest();
                request.Interval = _state.Timeout;
                request.Fired = DateTime.Now;

                _mainPort.Post(new FireTimer(request));

                _state.Expires = DateTime.MaxValue;
                _state.Timeout = 0;
            }
            _state.Ticks = (_state.Ticks + 1) % _tickFilter;
            if (_state.Ticks == 0)
            {
                SendNotification(_subMgr, tick);
            }
            yield break;
        }

        /// <summary>
        /// FireTimerHandler - Send notifiation when a timer expires
        /// </summary>
        /// <param name="fireTimer"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public virtual IEnumerator<ITask> FireTimerHandler(FireTimer fireTimer)
        {
            SendNotification(_subMgr, fireTimer);
            yield break;
        }

        /// <summary>
        /// WaitHandler - Wait for specified time before responding
        /// </summary>
        /// <param name="wait"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public virtual IEnumerator<ITask> WaitHandler(Wait wait)
        {
            if (wait.Body.Interval > _waitLimit)
            {
                string msg = string.Format(System.Globalization.CultureInfo.InvariantCulture,
                    "Attempt to wait for {0}ms, which is too long (max {1}ms)",
                    wait.Body.Interval,
                    _waitLimit);

                LogError(msg);

                wait.ResponsePort.Post(
                    W3C.Soap.Fault.FromCodeSubcodeReason(
                        W3C.Soap.FaultCodes.Receiver,
                        DsspFaultCodes.OperationFailed,
                        msg
                    )
                );
            }
            else
            {
                LogVerbose("Waiting for " + wait.Body.Interval + "ms");
                Activate(
                    Arbiter.Receive(false, TimeoutPort(wait.Body.Interval),
                        delegate(DateTime signal)
                        {
                            wait.ResponsePort.Post(DefaultSubmitResponseType.Instance);
                        }
                    )
                );
            }
            yield break;
        }

        /// <summary>
        /// TimeHandler - Return the current date and time
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public virtual IEnumerator<ITask> TimeHandler(Time time)
        {
            time.ResponsePort.Post(new TimeResponse(DateTime.Now));
            yield break;
        }
    }
}
