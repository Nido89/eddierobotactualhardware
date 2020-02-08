//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: ServiceTutorial5.cs $ $Revision: 8 $
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

using serviceTutorial5 = RoboticsServiceTutorial5;
#region CODECLIP 02-1
using rst4 = RoboticsServiceTutorial4.Proxy;
#endregion

namespace RoboticsServiceTutorial5
{

    /// <summary>
    /// Implementation class for ServiceTutorial5
    /// </summary>
    [ResourceGroup("Service tutorial 5")]
    [DisplayName("(User) Service Tutorial 5: Subscribing to another Service")]
    [Description("This service shows how to subscribe to another service.")]
    [Contract(Contract.Identifier)]
    [ActivationSettings(ShareDispatcher=true,ExecutionUnitsPerDispatcher=0)]
    [DssServiceDescription("http://msdn.microsoft.com/library/bb483061.aspx")]
    public class ServiceTutorial5Service : DsspServiceBase
    {
        /// <summary>
        /// Service State
        /// </summary>
        [ServiceState]
        private ServiceTutorial5State _state = new ServiceTutorial5State();

        /// <summary>
        /// Main Port
        /// </summary>
        [ServicePort("/serviceTutorial5", AllowMultipleInstances=false)]
        private ServiceTutorial5Operations _mainPort = new ServiceTutorial5Operations();

        #region CODECLIP 02-2
        // Partner with ServiceTutorial4 and refer to it by the name Clock
        [Partner("Clock", Contract = rst4.Contract.Identifier, CreationPolicy = PartnerCreationPolicy.UseExistingOrCreate)]
        rst4.ServiceTutorial4Operations _clockPort = new rst4.ServiceTutorial4Operations();
        rst4.ServiceTutorial4Operations _clockNotify = new rst4.ServiceTutorial4Operations();
        #endregion

        /// <summary>
        /// Default Service Constructor
        /// </summary>
        #region CODECLIP 02-4
        public ServiceTutorial5Service(DsspServiceCreationPort creationPort) :
                base(creationPort)
        {
            PartnerEnumerationTimeout = TimeSpan.FromSeconds(10);
        }
        #endregion

        /// <summary>
        /// Service Start
        /// </summary>
        #region CODECLIP 03-1
        protected override void Start()
        {
            base.Start();

            // Add the handlers for notifications from ServiceTutorial4.
            // This is necessary because these handlers do not handle
            // operations in this service, so you cannot mark them with
            // the ServiceHandler attribute.
            MainPortInterleave.CombineWith(
                new Interleave(
                    new TeardownReceiverGroup(),
                    new ExclusiveReceiverGroup(),
                    new ConcurrentReceiverGroup(
                        Arbiter.Receive<rst4.IncrementTick>(true, _clockNotify, NotifyTickHandler),
                        Arbiter.Receive<rst4.Replace>(true, _clockNotify, NotifyReplaceHandler)
                    ))
            );

            #region CODECLIP 02-3
            _clockPort.Subscribe(_clockNotify);
            #endregion
        }
        #endregion

        /// <summary>
        /// Replace Handler
        /// </summary>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public IEnumerator<ITask> ReplaceHandler(Replace replace)
        {
            _state = replace.Body;
            replace.ResponsePort.Post(DefaultReplaceResponseType.Instance);
            yield break;
        }

        #region CODECLIP 04-4
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public IEnumerator<ITask> IncrementTickHandler(rst4.IncrementTick incrementTick)
        {
            _state.TickCount++;
            incrementTick.ResponsePort.Post(DefaultUpdateResponseType.Instance);
            yield break;
        }

        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public IEnumerator<ITask> SetTickCountHandler(SetTickCount setTickCount)
        {
            _state.TickCount = setTickCount.Body.TickCount;
            setTickCount.ResponsePort.Post(DefaultUpdateResponseType.Instance);
            yield break;
        }
        #endregion

        #region CODECLIP 04-5
        private void NotifyTickHandler(rst4.IncrementTick incrementTick)
        {
            LogInfo("Got Tick");
            _mainPort.Post(new rst4.IncrementTick(incrementTick.Body));
        }

        private void NotifyReplaceHandler(rst4.Replace replace)
        {
            LogInfo("Tick Count: " + replace.Body.Ticks);
            _mainPort.Post(new SetTickCount(replace.Body.Ticks));
        }
        #endregion
    }

}
