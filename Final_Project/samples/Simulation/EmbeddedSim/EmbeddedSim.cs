//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
// 
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: EmbeddedSim.cs $ $Revision: 7 $
//-----------------------------------------------------------------------

using Microsoft.Ccr.Core;
using Microsoft.Ccr.Adapters.WinForms;
using Microsoft.Dss.Core;
using Microsoft.Dss.Core.Attributes;
using Microsoft.Dss.ServiceModel.Dssp;
using Microsoft.Dss.ServiceModel.DsspServiceBase;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Xml;
using W3C.Soap;

using xnaTypes = Microsoft.Xna.Framework;
using Microsoft.Robotics.Simulation;
using Microsoft.Robotics.Simulation.Engine;
using engineproxy = Microsoft.Robotics.Simulation.Engine.Proxy;
using Microsoft.Robotics.Simulation.Physics;
using Microsoft.Robotics.PhysicalModel;

namespace Microsoft.Simulation.Embedded
{
    /// <summary>
    /// Embedded Sim implementation class
    /// </summary>
    [DisplayName("(User) EmbeddedSim")]
    [Description("Example of a Service that uses the Simulator in headless mode.")]
    [Contract(Contract.Identifier)]
    public class EmbeddedSimService : DsspServiceBase
    {
        EmbeddedSimUI _embeddedSimUI = null;

        SimulationEnginePort _notificationTarget;
        CameraEntity _observer;

        SimulatorConfiguration _defaultConfig = null;

        // This port receives events from the user interface
        FromWinformEvents _fromWinformPort = new FromWinformEvents();

        /// <summary>
        /// _state
        /// </summary>
        private EmbeddedSimState _state = new EmbeddedSimState();
        /// <summary>
        /// _main Port
        /// </summary>
        [ServicePort("/embeddedsim", AllowMultipleInstances = false)]
        private EmbeddedSimOperations _mainPort = new EmbeddedSimOperations();
        /// <summary>
        /// Default Service Constructor
        /// </summary>
        public EmbeddedSimService(DsspServiceCreationPort creationPort) :
            base(creationPort)
        {
        }
        /// <summary>
        /// Service Start
        /// </summary>
        protected override void Start()
        {
            base.Start();
            _notificationTarget = new SimulationEnginePort();
            EntitySubscribeRequestType req = new EntitySubscribeRequestType();
            req.Name = "MainCamera";
            SimulationEngine.GlobalInstancePort.Subscribe(req, _notificationTarget);

            // Set the Simulator camera view and resolution
            UpdateCameraView view = new UpdateCameraView(new CameraView());
            view.Body.EyePosition = new Vector3(1, 5, 1);
            view.Body.LookAtPoint = new Vector3(0, 0, 0);
            view.Body.XResolution = 640;
            view.Body.YResolution = 480;
            SimulationEngine.GlobalInstancePort.Post(view);

            // get the current simulator configuration
            _defaultConfig = new SimulatorConfiguration(true);

            Activate(Arbiter.Choice(SimulationEngine.GlobalInstancePort.Query(_defaultConfig),
                delegate(SimulatorConfiguration config) 
                {
                    _defaultConfig = config;
                    if(_embeddedSimUI != null)
                        WinFormsServicePort.FormInvoke(delegate() { _embeddedSimUI.SetHeadless(config.Headless); }); 
                },
                delegate(W3C.Soap.Fault fault)
                {
                }
            ));

            // Add the winform message handler to the interleave
            Activate(Arbiter.Interleave(
                new TeardownReceiverGroup(),
                new ExclusiveReceiverGroup
                (
                    Arbiter.Receive<InsertSimulationEntity>(false, _notificationTarget, InsertEntityNotificationHandlerFirstTime),
                    Arbiter.Receive<FromWinformMsg>(true, _fromWinformPort, OnWinformMessageHandler)
                ),
                new ConcurrentReceiverGroup()
            ));

            // Create the user interface form
            WinFormsServicePort.Post(new RunForm(CreateForm));
        }

        void InsertEntityNotificationHandlerFirstTime(InsertSimulationEntity ins)
        {
            InsertEntityNotificationHandler(ins);
            Activate(Arbiter.Interleave(
                    new TeardownReceiverGroup
                    (
                    ),
                    new ExclusiveReceiverGroup(
                        Arbiter.Receive<FromWinformMsg>(true, _fromWinformPort, OnWinformMessageHandler),
                        Arbiter.Receive<InsertSimulationEntity>(true, _notificationTarget, InsertEntityNotificationHandler),
                        Arbiter.Receive<DeleteSimulationEntity>(true, _notificationTarget, DeleteEntityNotificationHandler)
                    ),
                    new ConcurrentReceiverGroup()
                ));
        }

        void InsertEntityNotificationHandler(InsertSimulationEntity ins)
        {
            _observer = (CameraEntity)ins.Body;
            // kick of polling
            Activate(new Task(() => UpdateCameraImage(DateTime.UtcNow)));
        }

        void DeleteEntityNotificationHandler(DeleteSimulationEntity del)
        {
            _observer = null;
        }

        void UpdateCameraImage(DateTime dt)
        {
            try
            {
                var resultPort = new PortSet<int[], Exception>();
                _observer.CaptureScene(resultPort);
                Activate(
                    resultPort.Choice(
                        (data) =>
                        {
                            var bmp = _observer.CreateBitmapFromArray(data);
                            WinFormsServicePort.Post(new FormInvoke(delegate() { _embeddedSimUI.SetCameraImage(bmp); }));
                        },
                        (ex) => { }));
                
            }
            finally
            {
                if (this.ServicePhase == ServiceRuntimePhase.Started)
                {
                    Activate(TimeoutPort(50).Receive(UpdateCameraImage));
                }
            }
        }


        // Create the UI form
        System.Windows.Forms.Form CreateForm()
        {
            return new EmbeddedSimUI(_fromWinformPort);
        }

        // process messages from the UI Form
        void OnWinformMessageHandler(FromWinformMsg msg)
        {
            switch (msg.Command)
            {
                case FromWinformMsg.MsgEnum.Loaded:
                    // the windows form is ready to go
                    _embeddedSimUI = (EmbeddedSimUI)msg.Object;
                    WinFormsServicePort.FormInvoke(delegate() {_embeddedSimUI.SetHeadless(_defaultConfig.Headless); });
                    break;

                case FromWinformMsg.MsgEnum.Drag:
                    if (_observer != null)
                    {
                        Vector2 drag = (Vector2)msg.Object;
                        xnaTypes.Vector3 view = _observer.LookAt - _observer.Location;
                        view.Normalize();
                        xnaTypes.Vector3 up = new xnaTypes.Vector3(0, 1, 0);
                        float dot = xnaTypes.Vector3.Dot(view, up);
                        if (Math.Abs(dot) > 0.99)
                        {
                            up += new xnaTypes.Vector3(0.1f, 0, 0);
                            up.Normalize();
                        }
                        xnaTypes.Vector3 right = xnaTypes.Vector3.Cross(view, up);
                        view = xnaTypes.Vector3.Multiply(view, 10f);
                        view = xnaTypes.Vector3.Transform(view, xnaTypes.Matrix.CreateFromAxisAngle(up, (float)(-drag.X * Math.PI / 500)));
                        view = xnaTypes.Vector3.Transform(view, xnaTypes.Matrix.CreateFromAxisAngle(right, (float)(-drag.Y * Math.PI / 500)));

                        _observer.LookAt = _observer.Location + view;
                    }
                    break;

                case FromWinformMsg.MsgEnum.Zoom:
                    if (_observer != null)
                    {
                        Vector2 drag = (Vector2)msg.Object;
                        xnaTypes.Vector3 view = _observer.LookAt - _observer.Location;
                        view.Normalize();
                        view = xnaTypes.Vector3.Multiply(view, drag.X * 0.1f);
                        _observer.LookAt += view;
                        _observer.Location += view;
                    }
                    break;

            }
        }

        /// <summary>
        /// Get Handler
        /// </summary>
        /// <param name="get"></param>
        /// <returns></returns>
        public virtual IEnumerator<ITask> GetHandler(Get get)
        {
            get.ResponsePort.Post(_state);
            yield break;
        }

    }
}
