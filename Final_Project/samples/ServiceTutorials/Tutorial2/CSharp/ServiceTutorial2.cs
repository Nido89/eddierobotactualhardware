//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: ServiceTutorial2.cs $ $Revision: 6 $
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

using serviceTutorial2 = RoboticsServiceTutorial2;


namespace RoboticsServiceTutorial2
{

    /// <summary>
    /// Implementation class for ServiceTutorial2
    /// </summary>
    [DisplayName("(User) Service Tutorial 2: Updating State")]
    [Description("Demonstrates how to update the state of a service.")]
    [Contract(Contract.Identifier)]
    [DssServiceDescription("http://msdn.microsoft.com/library/bb483062.aspx")]
    public class ServiceTutorial2Service : DsspServiceBase
    {
        #region CODECLIP 01
        /// <summary>
        /// Service State
        /// </summary>
        [ServiceState]
        private ServiceTutorial2State _state = new ServiceTutorial2State();
        #endregion

        /// <summary>
        /// Main Port
        /// </summary>
        [ServicePort("/serviceTutorial2", AllowMultipleInstances=false)]
        private ServiceTutorial2Operations _mainPort = new ServiceTutorial2Operations();

        #region CODECLIP 03-1
        private Port<DateTime> _timerPort = new Port<DateTime>();
        #endregion

        /// <summary>
        /// Default Service Constructor
        /// </summary>
        public ServiceTutorial2Service(DsspServiceCreationPort creationPort) :
                base(creationPort)
        {
        }

        #region CODECLIP 03-2
        /// <summary>
        /// Service Start
        /// </summary>
        protected override void Start()
        {
            base.Start();

            // Kick off the timer (with no delay) and start a receiver for it
            _timerPort.Post(DateTime.Now);
            Activate(Arbiter.Receive(true, _timerPort, TimerHandler));
        }
        #endregion

        #region CODECLIP 03-3
        /// <summary>
        /// Timer Handler
        /// </summary>
        /// <param name="signal">Not used</param>
        void TimerHandler(DateTime signal)
        {
            // Post a message to ourselves.
            // Do not modify the state here because this handler is
            // not part of the main interleave and therefore does not
            // run exclusively.
            _mainPort.Post(new IncrementTick());

            // Set the timer for the next tick
            Activate(
                Arbiter.Receive(false, TimeoutPort(1000),
                    delegate(DateTime time)
                    {
                        _timerPort.Post(time);
                    }
                )
            );
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

        #region CODECLIP 02
        /// <summary>
        /// Increment Tick Handler
        /// </summary>
        /// <param name="incrementTick">Not used</param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public IEnumerator<ITask> IncrementTickHandler(IncrementTick incrementTick)
        {
            // Only update the state here because this is an Exclusive handler
            _state.Ticks++;
            LogInfo("Tick: " + _state.Ticks);
            incrementTick.ResponsePort.Post(DefaultUpdateResponseType.Instance);
            yield break;
        }
        #endregion
    }

}
