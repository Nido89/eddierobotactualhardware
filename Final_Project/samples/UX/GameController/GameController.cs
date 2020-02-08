//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: GameController.cs $ $Revision: 7 $
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Security.Permissions;

using Microsoft.Ccr.Core;
using Microsoft.Dss.Core;
using Microsoft.Dss.Core.Attributes;
using Microsoft.Dss.ServiceModel.Dssp;
using Microsoft.Dss.ServiceModel.DsspServiceBase;

using sm = Microsoft.Dss.Services.SubscriptionManager;
using W3C.Soap;



namespace Microsoft.Robotics.Services.GameController
{

    /// <summary>
    /// Provides access to a DirectInput game controller such as a joystick or gamepad.
    /// </summary>
    [DisplayName("(User) Game Controller")]
    [Contract(Contract.Identifier)]
    [DssServiceDescription("http://msdn.microsoft.com/library/dd145253.aspx")]
    public class GameControllerService : DsspServiceBase
    {
        [ServiceState]
        [InitialStatePartner(Optional = true)]
        private GameControllerState _state;

        [ServicePort("/gamecontroller", AllowMultipleInstances=true)]
        private GameControllerOperations _mainPort = new GameControllerOperations();

        [Partner("SubMgr", Contract = sm.Contract.Identifier, CreationPolicy = PartnerCreationPolicy.CreateAlways, Optional = false)]
        sm.SubscriptionManagerPort _subMgr = new sm.SubscriptionManagerPort();

        /// <summary>
        /// Default Service Constructor
        /// </summary>
        public GameControllerService(DsspServiceCreationPort creationPort) :
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
                _state = new GameControllerState();
            }
            base.Start();

            // post a replace message to ourself, this causes the correct initialization
            Replace replace = new Replace(_state);
            _mainPort.Post(replace);

            // start the timer
            Spawn(DateTime.Now, TimerHandler);
        }

        void TimerHandler(DateTime signal)
        {
            try
            {
                Poll poll = new Poll(new PollRequest());
                _mainPort.Post(poll);
            }
            finally
            {
                Activate(
                    Arbiter.Receive(false, TimeoutPort(50), TimerHandler)
                );
            }
        }

        /// <summary>
        /// Handles the replace message by replacing the state of the service
        /// </summary>
        /// <param name="replace"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public virtual IEnumerator<ITask> ReplaceHandler(Replace replace)
        {
            _state.Controller.Dispose();

            _state = replace.Body;

            _state.Controller.FindInstance();

            replace.ResponsePort.Post(DefaultReplaceResponseType.Instance);
            yield break;
        }

        /// <summary>
        /// Handles the Poll message by updating the state of the contollers and sending
        /// appropriate notifications.
        /// </summary>
        /// <param name="poll"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public virtual IEnumerator<ITask> PollHandler(Poll poll)
        {
            Substate updated = _state.Update(DateTime.Now);

            if ((updated & Substate.Axes) != Substate.None)
            {
                SendNotification<UpdateAxes>(_subMgr, _state.Axes);
            }
            if ((updated & Substate.Buttons) != Substate.None)
            {
                SendNotification<UpdateButtons>(_subMgr, _state.Buttons);
            }
            if ((updated & Substate.PovHats) != Substate.None)
            {
                SendNotification<UpdatePovHats>(_subMgr, _state.PovHats);
            }
            if ((updated & Substate.Sliders) != Substate.None)
            {
                SendNotification<UpdateSliders>(_subMgr, _state.Sliders);
            }

            poll.ResponsePort.Post(DefaultSubmitResponseType.Instance);
            yield break;
        }

        /// <summary>
        /// Handles a subscribe request.
        /// </summary>
        /// <param name="subscribe"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public virtual IEnumerator<ITask> SubscribeHandler(Subscribe subscribe)
        {
            SubscribeRequestType request = subscribe.Body;

            yield return Arbiter.Choice(
                SubscribeHelper(_subMgr, request, subscribe.ResponsePort),
                delegate(SuccessResult success)
                {
                    SendNotificationToTarget<Replace>(request.Subscriber, _subMgr, _state);
                },
                delegate(Exception failure) { }
            );
        }

        /// <summary>
        /// Handles a request to change the current controller.
        /// </summary>
        /// <param name="changeController"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public virtual IEnumerator<ITask> ChangeControllerHandler(ChangeController changeController)
        {
            Controller newController = changeController.Body;
            if (newController.FindInstance())
            {
                _state.Controller.Dispose();

                _state.Controller = newController;

                changeController.ResponsePort.Post(DefaultUpdateResponseType.Instance);

                SendNotification<ChangeController>(_subMgr, _state.Controller);
            }
            else
            {
                changeController.ResponsePort.Post(Fault.FromCodeSubcodeReason(
                    W3C.Soap.FaultCodes.Receiver,
                    DsspFaultCodes.OperationFailed,
                    Resources.ControllerNotFound
                    )
                );
            }
            yield break;
        }

        /// <summary>
        /// Handles a request to update the controller axes state.
        /// </summary>
        /// <param name="update"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public virtual IEnumerator<ITask> UpdateAxesHandler(UpdateAxes update)
        {
            ActionNotSupported(update.ResponsePort);
            yield break;
        }

        /// <summary>
        /// Handles a request to update the controller button state.
        /// </summary>
        /// <param name="update"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public virtual IEnumerator<ITask> UpdateButtonsHandler(UpdateButtons update)
        {
            ActionNotSupported(update.ResponsePort);
            yield break;
        }

        /// <summary>
        /// Handles a request to update the Point Of View Hats state.
        /// </summary>
        /// <param name="update"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public virtual IEnumerator<ITask> UpdatePovHatsHandler(UpdatePovHats update)
        {
            ActionNotSupported(update.ResponsePort);
            yield break;
        }

        /// <summary>
        /// Handles a request to update the sliders state.
        /// </summary>
        /// <param name="update"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public virtual IEnumerator<ITask> UpdateSlidersHandler(UpdateSliders update)
        {
            ActionNotSupported(update.ResponsePort);
            yield break;
        }

        private void ActionNotSupported(PortSet<DefaultUpdateResponseType, W3C.Soap.Fault> responsePort)
        {
            responsePort.Post(Fault.FromCodeSubcodeReason(
                W3C.Soap.FaultCodes.Receiver,
                DsspFaultCodes.ActionNotSupported,
                Resources.NotModifiable
                )
            );
        }

        /// <summary>
        /// Handles a request to return the current controllers.
        /// </summary>
        /// <param name="getControllers"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public virtual IEnumerator<ITask> GetControllersHandler(GetControllers getControllers)
        {
            GetControllersResponse response = new GetControllersResponse();
            response.Controllers.AddRange(Controller.Attached);

            foreach (Controller controller in response.Controllers)
            {
                controller.Current = (controller.Instance == _state.Controller.Instance);
            }

            getControllers.ResponsePort.Post(response);
            yield break;
        }
    }
}
