//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: ServiceTutorial3.cs $ $Revision: 8 $
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

using serviceTutorial3 = RoboticsServiceTutorial3;


namespace RoboticsServiceTutorial3
{

    /// <summary>
    /// Implementation class for ServiceTutorial3
    /// </summary>
    [DisplayName("(User) Service Tutorial 3: Persisting State")]
    [Description("This service shows how to persist the service state to disk.")]
    [Contract(Contract.Identifier)]
    [DssServiceDescription("http://msdn.microsoft.com/library/bb483063.aspx")]
    public class ServiceTutorial3Service : DsspServiceBase
    {
        #region CODECLIP 01-1
        /// <summary>
        /// Service State
        /// </summary>
        [ServiceState]
        [InitialStatePartner(Optional = true, ServiceUri = ServicePaths.Store + "/ServiceTutorial3.xml")]
        private ServiceTutorial3State _state = new ServiceTutorial3State();
        #endregion

        /// <summary>
        /// Main Port
        /// </summary>
        [ServicePort("/serviceTutorial3", AllowMultipleInstances=false)]
        private ServiceTutorial3Operations _mainPort = new ServiceTutorial3Operations();

        private Port<DateTime> _timerPort = new Port<DateTime>();

        /// <summary>
        /// Default Service Constructor
        /// </summary>
        public ServiceTutorial3Service(DsspServiceCreationPort creationPort) :
                base(creationPort)
        {
        }

        /// <summary>
        /// Service Start
        /// </summary>
        protected override void Start()
        {
            #region CODECLIP 01-2
            // The initial state is optional, so we must be prepared to
            // create a new state if there is none
            if (_state == null)
            {
                _state = new ServiceTutorial3State();
            }
            #endregion

            base.Start();

            _timerPort.Post(DateTime.Now);
            Activate(Arbiter.Receive(true, _timerPort, TimerHandler));
        }

        void TimerHandler(DateTime signal)
        {
            _mainPort.Post(new IncrementTick());

            Activate(
                Arbiter.Receive(false, TimeoutPort(1000),
                    delegate(DateTime time)
                    {
                        _timerPort.Post(time);
                    }
                )
            );
        }

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
        public IEnumerator<ITask> IncrementTickHandler(IncrementTick incrementTick)
        {
            _state.Ticks++;
            LogInfo("Tick: " + _state.Ticks);

            // Save the state every 10 ticks
            if (_state.Ticks % 10 == 0)
            {
                LogInfo("Store State");
                #region CODECLIP 03-1
                // SaveState() sends a message so the process is not complete
                // until a response is received. It is good practise to check
                // for a Fault whenever you send a message.
                yield return Arbiter.Choice(
                    base.SaveState(_state),
                    delegate(DefaultReplaceResponseType success) { },
                    delegate(W3C.Soap.Fault fault)
                    {
                        LogError(null, "Unable to store state", fault);
                    }
                );
                #endregion
            }

            incrementTick.ResponsePort.Post(DefaultUpdateResponseType.Instance);
            yield break;
        }
    }

}
