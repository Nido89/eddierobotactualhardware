//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: SimpleSimulatedRobot.cs $ $Revision: 5 $
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

using Microsoft.Robotics.Simulation.Engine;
using Microsoft.Robotics.PhysicalModel;
using xna = Microsoft.Xna.Framework.Graphics;
using Microsoft.Robotics.Simulation.Physics;
using drive = Microsoft.Robotics.Services.Simulation.Drive.Proxy;
using simwebcam = Microsoft.Robotics.Services.Simulation.Sensors.SimulatedWebcam.Proxy;
using System.Runtime.InteropServices;

namespace Robotics.SimpleSimulatedRobot
{
    #region Basic Simulation Service Template
    [Contract(Contract.Identifier)]
    [DisplayName("(User) Simple Simulated Robot")]
    [Description("SimpleSimulatedRobot demonstrates how to create a custom entity that allows for easily adding various sensors")]
    class SimpleSimulatedRobotService : DsspServiceBase
    {
        /// <summary>
        /// Service state
        /// </summary>
        [ServiceState]
        SimpleSimulatedRobotState _state = new SimpleSimulatedRobotState();

        /// <summary>
        /// Main service port
        /// </summary>
        [ServicePort("/SimpleSimulatedRobot", AllowMultipleInstances = true)]
        SimpleSimulatedRobotOperations _mainPort = new SimpleSimulatedRobotOperations();

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
        public SimpleSimulatedRobotService(DsspServiceCreationPort creationPort)
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


    #region Motor Base
    /// <summary>
    /// MotorBase is an implementation of the differential drive entity. 
    /// </summary>
    [DataContract]
    public class MotorBase : DifferentialDriveEntity
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public MotorBase() { }

        /// <summary>
        /// Initialization constructor
        /// </summary>
        /// <param name="initialPos"></param>
        public MotorBase(Vector3 initialPos)
        {
            MASS = 9;
            CHASSIS_DIMENSIONS = new Vector3(0.393f, 0.18f, 0.40f);
            CHASSIS_CLEARANCE = 0.05f;
            FRONT_WHEEL_RADIUS = 0.08f;
            CASTER_WHEEL_RADIUS = 0.025f; // = CHASSIS_CLEARANCE / 2; // to keep things simple we make caster a bit bigger
            FRONT_WHEEL_WIDTH = 4.74f;  //not used
            CASTER_WHEEL_WIDTH = 0.02f; //not used
            FRONT_AXLE_DEPTH_OFFSET = -0.05f; // distance of the axle from the center of robot

            base.State.Name = "MotorBaseWithThreeWheels";
            base.State.MassDensity.Mass = MASS;
            base.State.Pose.Position = initialPos;

            // reference point for all shapes is the projection of
            // the center of mass onto the ground plane
            // (basically the spot under the center of mass, at Y = 0, or ground level)

            // chassis position
            BoxShapeProperties motorBaseDesc = new BoxShapeProperties("chassis", MASS,
                new Pose(new Vector3(
                0, // Chassis center is also the robot center, so use zero for the X axis offset
                CHASSIS_CLEARANCE + CHASSIS_DIMENSIONS.Y / 2, // chassis is off the ground and its center is DIM.Y/2 above the clearance
                0)), // no offset in the z/depth axis, since again, its center is the robot center
                CHASSIS_DIMENSIONS);

            motorBaseDesc.Material = new MaterialProperties("high friction", 0.0f, 1.0f, 20.0f);
            motorBaseDesc.Name = "Chassis";
            ChassisShape = new BoxShape(motorBaseDesc);

            // rear wheel is also called the caster
            CASTER_WHEEL_POSITION = new Vector3(0, // center of chassis
                CASTER_WHEEL_RADIUS, // distance from ground
                CHASSIS_DIMENSIONS.Z / 2 - CASTER_WHEEL_RADIUS); // all the way at the back of the robot

            // NOTE: right/left is from the perspective of the robot, looking forward

            FRONT_WHEEL_MASS = 0.10f;

            RIGHT_FRONT_WHEEL_POSITION = new Vector3(
                CHASSIS_DIMENSIONS.X / 2 + 0.01f - 0.05f,// left of center
                FRONT_WHEEL_RADIUS,// distance from ground of axle
                FRONT_AXLE_DEPTH_OFFSET); // distance from center, on the z-axis

            LEFT_FRONT_WHEEL_POSITION = new Vector3(
                -CHASSIS_DIMENSIONS.X / 2 - 0.01f + 0.05f,// right of center
                FRONT_WHEEL_RADIUS,// distance from ground of axle
                FRONT_AXLE_DEPTH_OFFSET); // distance from center, on the z-axis

            MotorTorqueScaling = 20;

            ConstructStateMembers();
        }
    }
    #endregion

    /// <summary>
    /// MotorBaseWithSensors is an implementation of the differential drive entity
    /// that automatically starts the simulated differential drive service
    /// </summary>
    [DataContract]
    public class MotorBaseWithDrive : MotorBase
    {
        public MotorBaseWithDrive() { }

        public MotorBaseWithDrive(Vector3 position) 
            : base(position) 
        {
        }

        public override void Initialize(xna.GraphicsDevice device, PhysicsEngine physicsEngine)
        {
            base.ServiceContract = drive.Contract.Identifier;
            base.Initialize(device, physicsEngine);
        }
    }

    /// <summary>
    /// SimulatedWebcamEntity is a CameraEntity that automatically starts the simulated
    /// web cam service
    /// </summary>
    [DataContract]
    public class SimulatedWebcamEntity : CameraEntity
    {
        public SimulatedWebcamEntity() { }
        
        /// <summary>
        /// Defaults to a 320x240 web cam with a 90 degree FOV
        /// </summary>
        /// <param name="viewSizeX"></param>
        /// <param name="viewSizeY"></param>
        /// <param name="halfViewAngle"></param>
        public SimulatedWebcamEntity(
            Vector3 initialPosition,
            [DefaultParameterValue(320)] int viewSizeX,
            [DefaultParameterValue(240)] int viewSizeY,
            [DefaultParameterValue((float)Math.PI / 4.0f)] float halfViewAngle 
            )
            : base(viewSizeX, viewSizeY, halfViewAngle)
        {
            State.Pose.Position = initialPosition;
        }

        public override void Initialize(xna.GraphicsDevice device, PhysicsEngine physicsEngine)
        {
            ServiceContract = simwebcam.Contract.Identifier;
            IsRealTimeCamera = true;

            base.Initialize(device, physicsEngine);
        }
    }
}


