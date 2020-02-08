//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: CustomSimulatedEntities.cs $ $Revision: 6 $
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
using engine = Microsoft.Robotics.Simulation.Engine;

#region Using Statements
using Microsoft.Robotics.Simulation.Engine;
using Microsoft.Robotics.PhysicalModel;
using xna = Microsoft.Xna.Framework.Graphics;
using Microsoft.Robotics.Simulation.Physics;

using simlrf = Microsoft.Robotics.Services.Simulation.Sensors.LaserRangeFinder.Proxy;
using simsonar = Microsoft.Robotics.Services.Simulation.Sensors.Sonar.Proxy;
using siminfrared = Microsoft.Robotics.Services.Simulation.Sensors.Infrared.Proxy;
using simcolorsensor = Microsoft.Robotics.Services.Simulation.Sensors.ColorSensor.Proxy;
using simbrightnesssensor = Microsoft.Robotics.Services.Simulation.Sensors.BrightnessSensor.Proxy;
using simcompass = Microsoft.Robotics.Services.Simulation.Sensors.Compass.Proxy;
using simgps = Microsoft.Robotics.Services.Simulation.Sensors.Compass.Proxy;
using Microsoft.Dss.Services.Constructor;
using System.Runtime.InteropServices;
#endregion

namespace Robotics.CustomSimulatedEntities
{
    #region Basic Simulation Service Template
    [Contract(Contract.Identifier)]
    [DisplayName("(User) Custom Simulated Entities")]
    [Description("Defines several more custom sensor entities")]
    class CustomSimulatedEntitiesService : DsspServiceBase
    {
        /// <summary>
        /// Service state
        /// </summary>
        [ServiceState]
        CustomSimulatedEntitiesState _state = new CustomSimulatedEntitiesState();

        /// <summary>
        /// Main service port
        /// </summary>
        [ServicePort("/CustomSimulatedEntities", AllowMultipleInstances = true)]
        CustomSimulatedEntitiesOperations _mainPort = new CustomSimulatedEntitiesOperations();

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
        public CustomSimulatedEntitiesService(DsspServiceCreationPort creationPort)
            : base(creationPort)
        {
        }

        /// <summary>
        /// Service start
        /// </summary>
        protected override void Start()
        {

            // 
            // Add service specific initialization here
            // 

            base.Start();
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
    #endregion

    #region SimulatedLRFEntity
    /// <summary>
    /// SimulatedLRFEntity is a LaserRangeFinderEntity that automatically starts the simulated 
    /// LRF service
    /// </summary>
    [DataContract]
    public class SimulatedLRFEntity : LaserRangeFinderEntity
    {
        public SimulatedLRFEntity() { }
        public SimulatedLRFEntity(Pose localPose) : base(localPose) { }

        public override void Initialize(xna.GraphicsDevice device, PhysicsEngine physicsEngine)
        {
            ServiceContract = simlrf.Contract.Identifier;

            base.Initialize(device, physicsEngine);
        }
    }
    #endregion

    #region Additional Custom Entities

    #region SimulatedSonarEntity
    /// <summary>
    /// SimulatedSonarEntity is a SonarEntity that automatically starts the simulated
    /// sonar service
    /// </summary>
    [DataContract]
    public class SimulatedSonarEntity : engine.SonarEntity
    {
        public SimulatedSonarEntity() { }
        public SimulatedSonarEntity(Pose localPose) : base(localPose) { }

        public override void Initialize(xna.GraphicsDevice device, PhysicsEngine physicsEngine)
        {
            ServiceContract = simsonar.Contract.Identifier;

            base.Initialize(device, physicsEngine);
        }
    }
    #endregion

    #region SimulatedIREntity
    /// <summary>
    /// SimulatedIREntity is an IREntity that automatically starts the simulated
    /// IR service
    /// </summary>
    [DataContract]
    public class SimulatedIREntity : engine.IREntity
    {
        public SimulatedIREntity() { }
        public SimulatedIREntity(Pose localPose) : base(localPose) { }

        public override void Initialize(xna.GraphicsDevice device, PhysicsEngine physicsEngine)
        {
            ServiceContract = siminfrared.Contract.Identifier;

            base.Initialize(device, physicsEngine);
        }
    }
    #endregion

    #region SimulatedColorSensorEntity
    /// <summary>
    /// SimulatedColorSensorEntity is an CameraEntity that automatically starts the simulated
    /// color sensor service
    /// </summary>
    [DataContract]
    public class SimulatedColorSensorEntity : CameraEntity
    {
        public SimulatedColorSensorEntity() { }

        public SimulatedColorSensorEntity([DefaultParameterValue(32)] int viewSizeX,
            [DefaultParameterValue(32)] int viewSizeY, [DefaultParameterValue(2.0f * (float)Math.PI / 180.0f)] float halfViewAngle)
            : base(viewSizeX, viewSizeY, halfViewAngle)
        {
        }

        public override void Initialize(xna.GraphicsDevice device, PhysicsEngine physicsEngine)
        {
            IsRealTimeCamera = true;
            ServiceContract = simcolorsensor.Contract.Identifier;

            base.Initialize(device, physicsEngine);
        }
    }
    #endregion

    #region SimulatedBrightnessSensorEntity
    /// <summary>
    /// SimulatedBrightnessSensorEntity is an CameraEntity that automatically starts the 
    /// simulated brightness sensor service
    /// </summary>
    [DataContract]
    public class SimulatedBrightnessSensorEntity : CameraEntity
    {
        public SimulatedBrightnessSensorEntity() { }

        public SimulatedBrightnessSensorEntity([DefaultParameterValue(32)] int viewSizeX,
            [DefaultParameterValue(32)] int viewSizeY, [DefaultParameterValue(2.0f * (float)Math.PI / 180.0f)] float halfViewAngle)
            : base(viewSizeX, viewSizeY, halfViewAngle)
        {
        }

        public override void Initialize(xna.GraphicsDevice device, PhysicsEngine physicsEngine)
        {
            IsRealTimeCamera = true;
            ServiceContract = simbrightnesssensor.Contract.Identifier;

            base.Initialize(device, physicsEngine);
        }
    }
    #endregion

    #region SimulatedCompassEntity
    /// <summary>
    /// SimulatedCompassEntity is a VisualEntity that automatically starts the 
    /// simulated compass sensor service
    /// </summary>
    [DataContract]
    public class SimulatedCompassEntity : VisualEntity
    {
        public override void Initialize(xna.GraphicsDevice device, PhysicsEngine physicsEngine)
        {
            ServiceContract = simcompass.Contract.Identifier;

            base.Initialize(device, physicsEngine);
        }
    }
    #endregion

    #region SimulatedGPSEntity
    /// <summary>
    /// SimulatedGPSEntity is a VisualEntity that automatically starts the 
    /// simulated GPS sensor service
    /// </summary>
    [DataContract]
    public class SimulatedGPSEntity : VisualEntity
    {
        public override void Initialize(xna.GraphicsDevice device, PhysicsEngine physicsEngine)
        {
            ServiceContract = simgps.Contract.Identifier;

            base.Initialize(device, physicsEngine);
        }
    }
    #endregion

    #endregion
}


