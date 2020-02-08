//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: ServiceTutorial6.cs $ $Revision: 10 $
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
using W3C.Soap;

using ServiceTutorial6 = RoboticsServiceTutorial6;
using rst4 = RoboticsServiceTutorial4.Proxy;

namespace RoboticsServiceTutorial6
{

    /// <summary>
    /// Implementation class for ServiceTutorial6
    /// </summary>
    [DisplayName("(User) Service Tutorial 6: Retrieving State and Displaying it Using an XML Transform")]
    [Description("How to expose and transform the state of a service.")]
    [DssServiceDescription("http://msdn.microsoft.com/library/bb483058.aspx")]
    [Contract(Contract.Identifier)]
    public class ServiceTutorial6Service : DsspServiceBase
    {
        /// <summary>
        /// Service State
        /// </summary>
        #region CODECLIP 03-1
        // Declare the service state and also the XSLT Transform for displaying it
        [ServiceState(StateTransform = "RoboticsServiceTutorial6.ServiceTutorial6.user.xslt")]
        private ServiceTutorial6State _state = new ServiceTutorial6State();
        #endregion

        /// <summary>
        /// Main Port
        /// </summary>
        [ServicePort("/ServiceTutorial6", AllowMultipleInstances=false)]
        private ServiceTutorial6Operations _mainPort = new ServiceTutorial6Operations();

        [Partner("Clock", Contract = rst4.Contract.Identifier, CreationPolicy = PartnerCreationPolicy.UseExistingOrCreate)]
        rst4.ServiceTutorial4Operations _clockPort = new rst4.ServiceTutorial4Operations();
        rst4.ServiceTutorial4Operations _clockNotify = new rst4.ServiceTutorial4Operations();

        /// <summary>
        /// Default Service Constructor
        /// </summary>
        public ServiceTutorial6Service(DsspServiceCreationPort creationPort) :
                base(creationPort)
        {
        }

        /// <summary>
        /// Service Start
        /// </summary>
        protected override void Start()
        {
            base.Start();
            Activate<ITask>(
                Arbiter.Receive<rst4.IncrementTick>(true, _clockNotify, NotifyTickHandler),
                Arbiter.Receive<rst4.Replace>(true, _clockNotify, NotifyReplaceHandler)
            );

            #region CODECLIP 01-2
            SpawnIterator(OnStartup);
            #endregion
        }

        #region CODECLIP 01-3
        private IEnumerator<ITask> OnStartup()
        {
            rst4.Get get;
            yield return _clockPort.Get(GetRequestType.Instance, out get);
            rst4.ServiceTutorial4State state = get.ResponsePort;
            if (state == null)
            {
                LogError("Unable to Get state from ServiceTutorial4", (Fault)get.ResponsePort);
            }
        #endregion

        #region CODECLIP 01-4
            else // if (state != null)
            {
                ServiceTutorial6State initState = new ServiceTutorial6State();
                initState.InitialTicks = state.Ticks;

                PartnerType partner = FindPartner("Clock");
                if (partner != null)
                {
                    initState.Clock = partner.Service;
                }

                Replace replace = new Replace();
                replace.Body = initState;

                _mainPort.Post(replace);
            }
        #endregion

        #region CODECLIP 01-5
            rst4.Subscribe subscribe;
            yield return _clockPort.Subscribe(_clockNotify, out subscribe);
            Fault fault = subscribe.ResponsePort;
            if (fault != null)
            {
                LogError("Unable to subscribe to ServiceTutorial4", fault);
            }
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
    }

}
