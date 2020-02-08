//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: XInputGamepad.cs $ $Revision: 7 $
//-----------------------------------------------------------------------
using System;
using System.Net;
using System.Collections.Generic;
using System.ComponentModel;
using System.Security.Permissions;

using Microsoft.Ccr.Core;
using Microsoft.Dss.Core;
using Microsoft.Dss.Core.Attributes;
using Microsoft.Dss.Core.DsspHttp;
using Microsoft.Dss.ServiceModel.Dssp;
using Microsoft.Dss.ServiceModel.DsspServiceBase;
using sm = Microsoft.Dss.Services.SubscriptionManager;

using xinput = Microsoft.Xna.Framework.Input;
using xna = Microsoft.Xna.Framework;
using W3C.Soap;

namespace Microsoft.Robotics.Services.Sample.XInputGamepad
{
    /// <summary>
    /// Provides access to an Xbox 360 controller such as a gamepad.
    /// </summary>
    [DisplayName("(User) XInput Controller")]
    [Contract(Contract.Identifier)]
    [DssServiceDescription("http://msdn.microsoft.com/library/dd145260.aspx")]
    public class XInputGamepadService : DsspServiceBase
    {
        [ServiceState(StateTransform = "Microsoft.Robotics.Services.Sample.XInputGamepad.GamePad.user.xslt")]
        [InitialStatePartner(Optional = true)]
        private XInputGamepadState _state;

        [ServicePort("/xinputgamepad", AllowMultipleInstances=true)]
        private XInputGamepadOperations _mainPort = new XInputGamepadOperations();

        [Partner("SubMgr", Contract = sm.Contract.Identifier, CreationPolicy = PartnerCreationPolicy.CreateAlways, Optional = false)]
        sm.SubscriptionManagerPort _submgr = new sm.SubscriptionManagerPort();

        /// <summary>
        /// Default Service Constructor
        /// </summary>
        public XInputGamepadService(DsspServiceCreationPort creationPort) :
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
                _state = new XInputGamepadState();
            }
            base.Start();

            // start the timer
            Spawn(DateTime.Now, TimerHandler);
        }

        void TimerHandler(DateTime signal)
        {
            _mainPort.Post(new Poll(new PollRequest()));
            Activate(
                Arbiter.Receive(false, TimeoutPort(50), TimerHandler)
            );
        }

        /// <summary>
        /// Polls the controller to update the current state.\n If the state has changed, then appropriate notifications are sent.
        /// </summary>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public virtual IEnumerator<ITask> PollHandler(Poll poll)
        {
            DateTime timeStamp = DateTime.Now;
            xinput.GamePadState gamepad = _state.GetState();

            if (_state.Update(timeStamp, gamepad))
            {
                if (_state.Controller.Update(gamepad))
                {
                    SendNotification<ControllerChanged>(_submgr, _state.Controller);
                }

                if (_state.DPad.Update(timeStamp, gamepad.DPad))
                {
                    SendNotification<DPadChanged>(_submgr, _state.DPad);
                }

                if (_state.Buttons.Update(timeStamp, gamepad.Buttons))
                {
                    SendNotification<ButtonsChanged>(_submgr, _state.Buttons);
                }

                if (_state.Triggers.Update(timeStamp, gamepad.Triggers))
                {
                    SendNotification<TriggersChanged>(_submgr, _state.Triggers);
                }

                if (_state.Thumbsticks.Update(timeStamp, gamepad.ThumbSticks))
                {
                    SendNotification<ThumbsticksChanged>(_submgr, _state.Thumbsticks);
                }
            }

            poll.ResponsePort.Post(DefaultSubmitResponseType.Instance);
            yield break;
        }

        /// <summary>
        /// Replace Handler
        /// </summary>
        /// <param name="replace"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public virtual IEnumerator<ITask> ReplaceHandler(Replace replace)
        {
            _state = replace.Body;
            replace.ResponsePort.Post(DefaultReplaceResponseType.Instance);

            SendNotification(_submgr, replace);
            yield break;
        }

        /// <summary>
        /// Handles subscribe requests.
        /// </summary>
        /// <param name="subscribe"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public virtual IEnumerator<ITask> SubscribeHandler(Subscribe subscribe)
        {
            SubscribeRequestType request = subscribe.Body;

            yield return Arbiter.Choice(SubscribeHelper(_submgr, request, subscribe.ResponsePort),
                delegate(SuccessResult success)
                {
                    SendNotificationToTarget<Replace>(request.Subscriber, _submgr, _state);
                },
                delegate(Exception failure){}
            );
        }

        /// <summary>
        /// This handler is called when a controller is plugged in or removed.
        /// </summary>
        /// <param name="controllerChanged"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public virtual IEnumerator<ITask> ControllerChangedHandler(ControllerChanged controllerChanged)
        {
            Controller controller = controllerChanged.Body;

            if (controller.IsValid)
            {
                _state.Controller.TimeStamp = DateTime.Now;
                _state.Controller.PlayerIndex = controller.PlayerIndex;
                _state.Controller.Update(_state.GetState());

                SendNotification<ControllerChanged>(_submgr, _state.Controller);
            }
            else
            {
                controllerChanged.ResponsePort.Post(
                    Fault.FromCodeSubcodeReason(
                        FaultCodes.Receiver,
                        DsspFaultCodes.UnknownEntry,
                        "Bad Controller"
                    )
                );
            }

            yield break;
        }

        /// <summary>
        /// Handler to set the controller vibration.
        /// </summary>
        /// <param name="setVibration"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public virtual IEnumerator<ITask> SetVibrationHandler(SetVibration setVibration)
        {
            _state.SetVibration(setVibration.Body);
            setVibration.ResponsePort.Post(DefaultUpdateResponseType.Instance);

            SendNotification(_submgr, setVibration);
            yield break;
        }

        /// <summary>
        /// Returns information about all connected controllers.
        /// </summary>
        /// <param name="queryControllers"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public virtual IEnumerator<ITask> QueryControllersHandler(QueryControllers queryControllers)
        {
            QueryControllersResponse response = new QueryControllersResponse();

            for (PlayerIndex player = PlayerIndex.One; player <= PlayerIndex.Four; player++)
            {
                xinput.GamePadCapabilities caps = xinput.GamePad.GetCapabilities((xna.PlayerIndex)player);

                if (caps.IsConnected)
                {
                    ControllerCaps controller = new ControllerCaps();
                    controller.PlayerIndex = player;
                    controller.IsConnected = true;
                    controller.ControllerType = (ControllerType)caps.GamePadType;

                    response.Controllers.Add(controller);
                }
            }

            queryControllers.ResponsePort.Post(response);

            yield break;
        }
    }
}
