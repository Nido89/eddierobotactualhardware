//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: EntityUI.cs $ $Revision: 4 $
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Ccr.Core;
using Microsoft.Dss.Core.Attributes;
using Microsoft.Dss.ServiceModel.Dssp;
using Microsoft.Dss.ServiceModel.DsspServiceBase;
using W3C.Soap;
using submgr = Microsoft.Dss.Services.SubscriptionManager;
using engine = Microsoft.Robotics.Simulation.Engine.Proxy;
using simulatedwebcam = Microsoft.Robotics.Services.Simulation.Sensors.SimulatedWebcam.Proxy;
using webcam = Microsoft.Robotics.Services.WebCam.Proxy;
using simulateddrive = Microsoft.Robotics.Services.Simulation.Drive.Proxy;
using drive = Microsoft.Robotics.Services.Drive.Proxy;
using Microsoft.Dss.Core;
using System.Drawing;
using Microsoft.Ccr.Adapters.WinForms;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using Microsoft.Robotics.Simulation.Engine;

#region Service Using Statements

using laserrangefinder = Microsoft.Robotics.Services.Simulation.Sensors.LaserRangeFinder.Proxy;
using simsonar = Microsoft.Robotics.Services.Simulation.Sensors.Sonar.Proxy;
using sicklrf = Microsoft.Robotics.Services.Sensors.SickLRF.Proxy;
using infrared = Microsoft.Robotics.Services.Simulation.Sensors.Infrared.Proxy;
using colorsensor = Microsoft.Robotics.Services.Simulation.Sensors.ColorSensor.Proxy;
using brightnesssensor = Microsoft.Robotics.Services.Simulation.Sensors.BrightnessSensor.Proxy;
using compass = Microsoft.Robotics.Services.Simulation.Sensors.Compass.Proxy;
using analogsensor = Microsoft.Robotics.Services.AnalogSensor.Proxy;
using sonar = Microsoft.Robotics.Services.Sonar.Proxy;
using System.IO;
using System.Reflection;
#endregion

namespace Robotics.EntityUI
{
    [Contract(Contract.Identifier)]
    [DisplayName("(User) Entity UI")]
    [Description("Basic UI that allows adding entities for the simulation tutorials")]
    class EntityUIService : DsspServiceBase
    {
        /// <summary>
        /// Service state
        /// </summary>
        [ServiceState]
        EntityUIState _state = new EntityUIState();

        /// <summary>
        /// Main service port
        /// </summary>
        [ServicePort("/EntityUI", AllowMultipleInstances = true)]
        EntityUIOperations _mainPort = new EntityUIOperations();

        [SubscriptionManagerPartner]
        submgr.SubscriptionManagerPort _submgrPort = new submgr.SubscriptionManagerPort();

        /// <summary>
        /// SimulationEngine partner
        /// </summary>
        [Partner("SimulationEngine", Contract = engine.Contract.Identifier, CreationPolicy = PartnerCreationPolicy.UseExistingOrCreate)]
        engine.SimulationEnginePort _simulationEnginePort = new engine.SimulationEnginePort();
        engine.SimulationEnginePort _simulationEngineNotify = new engine.SimulationEnginePort();

        /// <summary>
        /// Service constructor
        /// </summary>
        public EntityUIService(DsspServiceCreationPort creationPort)
            : base(creationPort)
        {
        }

        EntityUIForm _entityWinForm;

        /// <summary>
        /// Service start
        /// </summary>
        protected override void Start()
        {

            // 
            // Add service specific initialization here
            // 

            base.Start();


            WinFormsServicePort.Post(new RunForm(() =>
                {
                    _entityWinForm = new EntityUIForm(TaskQueue);
                    _entityWinForm.Show();
                    _entityWinForm.TopMost = true;
                    _entityWinForm.TopMost = false;
                    return _entityWinForm;
                }));

            
        }

        /// <summary>
        /// Handles Subscribe messages
        /// </summary>
        /// <param name="subscribe">the subscribe request</param>
        [ServiceHandler]
        public void SubscribeHandler(Subscribe subscribe)
        {
            SubscribeHelper(_submgrPort, subscribe.Body, subscribe.ResponsePort);
        }
    }
}


