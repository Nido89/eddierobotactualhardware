//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: ServiceTutorial4.cs $ $Revision: 10 $
//-----------------------------------------------------------------------

using Microsoft.Ccr.Core;
using Microsoft.Dss.Core;
using Microsoft.Dss.Core.Attributes;
using Microsoft.Dss.Core.DsspHttp;
using Microsoft.Dss.ServiceModel.Dssp;
using Microsoft.Dss.ServiceModel.DsspServiceBase;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml;

using serviceTutorial4 = RoboticsServiceTutorial4;
#region CODECLIP 02-1
using submgr = Microsoft.Dss.Services.SubscriptionManager;
#endregion


namespace RoboticsServiceTutorial4
{

    /// <summary>
    /// Implementation class for ServiceTutorial4
    /// </summary>
    [DisplayName("(User) Service Tutorial 4: Supporting Subscriptions")]
    [Description("Demonstrates a service that supports subscriptions")]
    [Contract(Contract.Identifier)]
    [DssServiceDescription("http://msdn.microsoft.com/library/bb483060.aspx")]
    public class ServiceTutorial4Service : DsspServiceBase
    {
        /// <summary>
        /// Service State
        /// </summary>
        [ServiceState]
        [InitialStatePartner(Optional = true, ServiceUri = ServicePaths.Store + "/ServiceTutorial4.xml")]
        private ServiceTutorial4State _state = new ServiceTutorial4State();

        /// <summary>
        /// Main Port
        /// </summary>
        [ServicePort("/serviceTutorial4", AllowMultipleInstances=false)]
        private ServiceTutorial4Operations _mainPort = new ServiceTutorial4Operations();

        #region CODECLIP 02-2
        // Declare and create a Subscription Manager to handle the subscriptions
        [SubscriptionManagerPartner]
        private submgr.SubscriptionManagerPort _submgrPort = new submgr.SubscriptionManagerPort();
        #endregion

        /// <summary>
        /// Default Service Constructor
        /// </summary>
        public ServiceTutorial4Service(DsspServiceCreationPort creationPort) :
                base(creationPort)
        {
        }

        /// <summary>
        /// Service Start
        /// </summary>
        protected override void Start()
        {
            if (_state == null)
            {
                _state = new ServiceTutorial4State();
            }

            base.Start();
        }

        /// <summary>
        /// Replace Handler
        /// </summary>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public IEnumerator<ITask> ReplaceHandler(Replace replace)
        {
            _state = replace.Body;

            #region CODECLIP 04-1
            // Echo the Replace message to all subscribers
            base.SendNotification(_submgrPort, replace);
            #endregion

            replace.ResponsePort.Post(DefaultReplaceResponseType.Instance);
            yield break;
        }

        [ServiceHandler(ServiceHandlerBehavior.Exclusive, Interval=1)]
        public IEnumerator<ITask> IncrementTickHandler(IncrementTick incrementTick)
        {
            _state.Ticks++;
            LogInfo("Tick: " + _state.Ticks);

            #region CODECLIP 04-2
            // Notify all subscribers.
            // The incrementTick message does not contain the tick count.
            base.SendNotification(_submgrPort, incrementTick);
            #endregion

            if (_state.Ticks % 10 == 0)
            {
                LogInfo("Store State");
                yield return Arbiter.Choice(
                    base.SaveState(_state),
                    delegate(DefaultReplaceResponseType success) { },
                    delegate(W3C.Soap.Fault fault)
                    {
                        LogError(null, "Unable to store state", fault);
                    }
                );
            }

            incrementTick.ResponsePort.Post(DefaultUpdateResponseType.Instance);
            yield break;
        }

        #region CODECLIP 05-1
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public IEnumerator<ITask> SubscribeHandler(Subscribe subscribe)
        {
            SubscribeRequestType request = subscribe.Body;
            LogInfo("Subscribe request from: " + request.Subscriber);

            // Use the Subscription Manager to handle the subscribers
            yield return Arbiter.Choice(
                SubscribeHelper(_submgrPort, request, subscribe.ResponsePort),
                delegate(SuccessResult success)
                {
                    // Send a notification on successful subscription so that the
                    // subscriber can initialize its own state
                    base.SendNotificationToTarget<Replace>(request.Subscriber, _submgrPort, _state);
                },
                delegate(Exception e)
                {
                    LogError(null, "Subscribe failed", e);
                }
            );

            yield break;
        }
        #endregion
    }

}
