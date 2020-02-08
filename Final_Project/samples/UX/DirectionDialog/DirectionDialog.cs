//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: DirectionDialog.cs $ $Revision: 7 $
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
using directiondialog = Microsoft.Robotics.Services.Sample.DirectionDialog;
using xml = System.Xml;
using sm = Microsoft.Dss.Services.SubscriptionManager;
using soap = W3C.Soap;
using dssp = Microsoft.Dss.ServiceModel.Dssp;
using Microsoft.Ccr.Adapters.WinForms;
using W3C.Soap;


namespace Microsoft.Robotics.Services.Sample.DirectionDialog
{
    /// <summary>
    /// Direction Dialog service - Provides four direction arrows (plus Stop) for controlling a robot
    /// </summary>
    [DisplayName("(User) Direction Dialog")]
    [Description("Displays a Windows dialog with 5 buttons on it for direction control.")]
    [Contract(Contract.Identifier)]
    [DssServiceDescription("http://msdn.microsoft.com/library/dd145251.aspx")]
    public class DirectionDialogService : DsspServiceBase
    {
        [ServiceState]
        private DirectionDialogState _state = new DirectionDialogState();

        [ServicePort("/directiondialog", AllowMultipleInstances=true)]
        private DirectionDialogOperations _mainPort = new DirectionDialogOperations();

        [Partner("SubMgr", Contract = sm.Contract.Identifier, CreationPolicy = PartnerCreationPolicy.CreateAlways, Optional = false)]
        sm.SubscriptionManagerPort _submgr = new sm.SubscriptionManagerPort();

        /// <summary>
        /// Default Service Constructor
        /// </summary>
        public DirectionDialogService(DsspServiceCreationPort creationPort) :
                base(creationPort)
        {
        }

        #region Start
        /// <summary>
        /// Service Start
        /// </summary>
        protected override void Start()
        {
            _state.Buttons.Add(new Button(ButtonDirection.Forwards));
            _state.Buttons.Add(new Button(ButtonDirection.Backwards));
            _state.Buttons.Add(new Button(ButtonDirection.Left));
            _state.Buttons.Add(new Button(ButtonDirection.Right));
            _state.Buttons.Add(new Button(ButtonDirection.Stop));

            base.Start();

            WinFormsServicePort.Post(
                new RunForm(
                    delegate()
                    {
                        return new Direction(
                            ServiceForwarder<DirectionDialogOperations>(ServiceInfo.Service)
                        );
                    }
                )
            );
        }
        #endregion

        /// <summary>
        /// Replace Handler
        /// </summary>
        /// <param name="replace"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public virtual IEnumerator<ITask> ReplaceHandler(Replace replace)
        {
            foreach (Button curr in replace.Body.Buttons)
            {
                Button button = _state.Buttons.Find(
                    delegate(Button test)
                    {
                        return test.Name == curr.Name;
                    }
                );

                if (button != null)
                {
                    button.Pressed = curr.Pressed;
                }
            }

            replace.ResponsePort.Post(DefaultReplaceResponseType.Instance);
            SendNotification<Replace>(_submgr, _state);
            yield break;
        }

        #region Button Press Handler
        /// <summary>
        /// ButtonPressHandler - Reacts to button presses
        /// </summary>
        /// <param name="buttonPress">Button press info sent from the Form</param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public virtual IEnumerator<ITask> ButtonPressHandler(ButtonPress buttonPress)
        {
            Button button = _state.Buttons.Find(
                delegate(Button test)
                {
                    return test.Name == buttonPress.Body.Name;
                }
            );
            if (button == null)
            {
                buttonPress.ResponsePort.Post(Fault.FromCodeSubcodeReason(
                    soap.FaultCodes.Receiver,
                    DsspFaultCodes.UnknownEntry,
                    "No such button: " + buttonPress.Body.Name
                    )
                );
            }
            else
            {
                button.Pressed = true;
                buttonPress.ResponsePort.Post(DefaultUpdateResponseType.Instance);
                SendNotification(_submgr, buttonPress);
            }
            yield break;
        }
        #endregion

        #region Button Release Handler
        /// <summary>
        /// ButtonReleaseHandler - Reacts to button releases
        /// </summary>
        /// <param name="buttonRelease">Button release info sent from the Form</param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public virtual IEnumerator<ITask> ButtonReleaseHandler(ButtonRelease buttonRelease)
        {
            Button button = _state.Buttons.Find(
                delegate(Button test)
                {
                    return test.Name == buttonRelease.Body.Name;
                }
            );
            if (button == null)
            {
                buttonRelease.ResponsePort.Post(Fault.FromCodeSubcodeReason(
                    soap.FaultCodes.Receiver,
                    DsspFaultCodes.UnknownEntry,
                    "No such button: " + buttonRelease.Body.Name
                    )
                );
            }
            else
            {
                button.Pressed = false;
                buttonRelease.ResponsePort.Post(DefaultUpdateResponseType.Instance);
                SendNotification(_submgr, buttonRelease);
            }
            yield break;
        }
        #endregion

        #region Subscribe Handler
        /// <summary>
        /// SubscribeHandler - Accepts subscription requests
        /// </summary>
        /// <param name="subscribe">Subscribe message</param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public virtual IEnumerator<ITask> SubscribeHandler(Subscribe subscribe)
        {
            SubscribeHelper(_submgr, subscribe.Body, subscribe.ResponsePort);
            yield break;
        }
        #endregion

    }
}
