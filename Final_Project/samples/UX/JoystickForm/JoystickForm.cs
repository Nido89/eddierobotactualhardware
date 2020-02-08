//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: JoystickForm.cs $ $Revision: 5 $
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;

using Microsoft.Ccr.Core;
using Microsoft.Ccr.Adapters.WinForms;

using Microsoft.Dss.Core;
using Microsoft.Dss.Core.Attributes;
using Microsoft.Dss.ServiceModel.Dssp;
using Microsoft.Dss.ServiceModel.DsspServiceBase;

using W3C.Soap;

using sm = Microsoft.Dss.Services.SubscriptionManager;
using gc = Microsoft.Robotics.Services.GameController.Proxy;


namespace Microsoft.Robotics.Services.Sample.JoystickForm
{
    /// <summary>
    /// JoystickFormService - Displays a form that acts as a joystick
    /// </summary>
    [Contract(Contract.Identifier)]
    [AlternateContract(gc.Contract.Identifier)]
    [DisplayName("(User) Desktop Joystick")]
    [Description("Desktop Joystick Service")]
    [DssServiceDescription("http://msdn.microsoft.com/library/dd145250.aspx")]
    public class JoystickFormService : DsspServiceBase
    {
        [ServiceState]
        private gc.GameControllerState _state = new gc.GameControllerState();

        [ServicePort("/joystickform", AllowMultipleInstances = true)]
        private gc.GameControllerOperations _mainPort = new gc.GameControllerOperations();

        [Partner("SubMgr", Contract = sm.Contract.Identifier, CreationPolicy = PartnerCreationPolicy.CreateAlways, Optional = false)]
        private sm.SubscriptionManagerPort _subMgr = new sm.SubscriptionManagerPort();

        const string _productGuid = "{6ECC4ACE-470A-4e1b-9512-C9AF59BF2F63}";

        static int _instanceCount;

        int _thisInstance;

        /// <summary>
        /// Constructor for service
        /// </summary>
        /// <param name="creationPort"></param>
        public JoystickFormService(DsspServiceCreationPort creationPort) : 
                base(creationPort)
        {
            _thisInstance = System.Threading.Interlocked.Increment(ref _instanceCount);
        }

        /// <summary>
        /// Service Start
        /// </summary>
        protected override void Start()
        {
            DateTime now = DateTime.UtcNow;

            _state.Controller.TimeStamp = now;
            _state.Controller.Current = true;
            _state.Controller.Instance = Guid.NewGuid();
            _state.Controller.InstanceName = "Desktop Joystick - " + _thisInstance;
            _state.Controller.Product = new Guid(_productGuid);
            _state.Controller.ProductName = "Desktop Joystick";

            _state.Axes.TimeStamp = now;

            _state.Buttons.TimeStamp = now;
            _state.Buttons.Pressed.AddRange(new bool[10]);

            WinFormsServicePort.Post(new RunForm(StartForm));

			base.Start();
        }

        Form StartForm()
        {
            gc.GameControllerOperations fwdPort = ServiceForwarder<gc.GameControllerOperations>(ServiceInfo.Service);

            return new JoystickUI(fwdPort);
        }

        /// <summary>
        /// ReplaceHandler
        /// </summary>
        /// <param name="replace"></param>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public void ReplaceHandler(gc.Replace replace)
        {
            _state = replace.Body;
            replace.ResponsePort.Post(DefaultReplaceResponseType.Instance);
            SendNotification(_subMgr, replace);
        }

        /// <summary>
        /// PollHandler - NOT IMPLEMENTED
        /// </summary>
        /// <param name="submit"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public virtual IEnumerator<ITask> PollHandler(gc.Poll submit)
        {
            // TODO: Implement Submit operations here.
            throw new NotImplementedException("TODO: Implement Submit operations here.");
        }

        /// <summary>
        /// ChangeControllerHandler - NOT SUPPORTED
        /// </summary>
        /// <param name="update"></param>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public void ChangeControllerHandler(gc.ChangeController update)
        {
            update.ResponsePort.Post(
                Fault.FromCodeSubcodeReason(
                    FaultCodes.Receiver,
                    DsspFaultCodes.ActionNotSupported,
                    "The Joystick Form Service does not support changing controllers"
                )
            );
        }

        /// <summary>
        /// UpdateAxesHandler
        /// </summary>
        /// <param name="update"></param>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public void UpdateAxesHandler(gc.UpdateAxes update)
        {
            _state.Axes = update.Body;
            update.ResponsePort.Post(DefaultUpdateResponseType.Instance);
            SendNotification(_subMgr, update);
        }

        /// <summary>
        /// UpdateButtonsHandler
        /// </summary>
        /// <param name="update"></param>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public void UpdateButtonsHandler(gc.UpdateButtons update)
        {
            _state.Buttons = update.Body;
            update.ResponsePort.Post(DefaultUpdateResponseType.Instance);
            SendNotification(_subMgr, update);
        }

        /// <summary>
        /// UpdatePovHatsHandler
        /// </summary>
        /// <param name="update"></param>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public void UpdatePovHatsHandler(gc.UpdatePovHats update)
        {
            _state.PovHats = update.Body;
            update.ResponsePort.Post(DefaultUpdateResponseType.Instance);
            SendNotification(_subMgr, update);
        }

        /// <summary>
        /// UpdateSlidersHandler
        /// </summary>
        /// <param name="update"></param>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public void UpdateSlidersHandler(gc.UpdateSliders update)
        {
            _state.Sliders = update.Body;
            update.ResponsePort.Post(DefaultUpdateResponseType.Instance);
            SendNotification(_subMgr, update);
        }

        /// <summary>
        /// SubscribeHandler
        /// </summary>
        /// <param name="subscribe"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public virtual IEnumerator<ITask> SubscribeHandler(gc.Subscribe subscribe)
        {
            base.SubscribeHelper(_subMgr, subscribe.Body, subscribe.ResponsePort);
            yield break;
        }

        /// <summary>
        /// GetControllersHandler
        /// </summary>
        /// <param name="query"></param>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public void GetControllersHandler(gc.GetControllers query)
        {
            gc.GetControllersResponse response = new gc.GetControllersResponse();
            response.Controllers = new List<gc.Controller>();
            response.Controllers.Add(_state.Controller);

            query.ResponsePort.Post(response);
        }
    }
}
