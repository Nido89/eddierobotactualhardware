//------------------------------------------------------------------------------
//  <copyright file="ReferencePlatform2011.cs" company="Microsoft Corporation">
//      Copyright (C) Microsoft Corporation.  All rights reserved.
//  </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Robotics.Services.Simulation.ReferencePlatform2011
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using Microsoft.Ccr.Core;
    using Microsoft.Dss.Core.Attributes;
    using Microsoft.Dss.ServiceModel.Dssp;
    using Microsoft.Dss.ServiceModel.DsspServiceBase;
    using Microsoft.Robotics.PhysicalModel;
    using Microsoft.Robotics.Simulation.Physics;
    using diffdrive = Microsoft.Robotics.Services.Drive;
    using engine = Microsoft.Robotics.Simulation.Engine;
    using submgr = Microsoft.Dss.Services.SubscriptionManager;
    using xna = Microsoft.Xna.Framework;

    #region Reference Platform
    /// <summary>
    /// Simulated 2011 Reference platform service
    /// </summary>
    [Contract(Contract.Identifier)]
    [AlternateContract(diffdrive.Contract.Identifier)]
    [DisplayName("(User) Simulated Reference Platform Robot 2011")]
    [Description("Simulated Reference Platform 2011 for Robotics Developer Studio")]
    public partial class ReferencePlatform2011Service : DsspServiceBase
    {
        /// <summary>
        /// Service state
        /// </summary>
        [ServiceState]
        [InitialStatePartner(Optional = false)]
        private ReferencePlatform2011State state = new ReferencePlatform2011State();

        /// <summary>
        /// Main service port
        /// </summary>
        [ServicePort("/ReferencePlatform2011Entity", AllowMultipleInstances = true)]
        private ReferencePlatform2011Operations mainPort = new ReferencePlatform2011Operations();

        /// <summary>
        /// Subscription manager port
        /// </summary>
        [SubscriptionManagerPartner]
        private submgr.SubscriptionManagerPort submgrPort = new submgr.SubscriptionManagerPort();

        /// <summary>
        /// Port used for the simulation engine
        /// </summary>
        private engine.SimulationEnginePort simulationEngineNotify = new engine.SimulationEnginePort();

        /// <summary>
        /// Initializes a new instance of the <see cref="ReferencePlatform2011Service"/> class.
        /// </summary>
        /// <param name="creationPort">The creation port.</param>
        public ReferencePlatform2011Service(DsspServiceCreationPort creationPort)
            : base(creationPort)
        {
        }

        /// <summary>
        /// Handles Subscribe messages
        /// </summary>
        /// <param name="subscribe">The subscribe request</param>
        [ServiceHandler]
        public void SubscribeHandler(Subscribe subscribe)
        {
            SubscribeHelper(this.submgrPort, subscribe.Body, subscribe.ResponsePort);
        }

        /// <summary>
        /// Service start
        /// </summary>
        protected override void Start()
        {
            if (this.state == null)
            {
                this.state = new ReferencePlatform2011State
                    {
                        InitialPosition = new Vector3(4f, 0.01f, 8.3f), EntityName = "ReferencePlatform2011", 
                    };
                this.CreateDefaultDriveState();
            }

            TaskQueue.Enqueue(new IterativeTask(this.InsertEntityAndStartDrive));
        }

        /// <summary>
        /// Configure sensors
        /// </summary>
        /// <returns>A CCR iterator</returns>
        private IEnumerator<ITask> InsertEntityAndStartDrive()
        {
            // Start the sim drive first
            this.StartSimDrive();

            var robot = new ReferencePlatform2011Entity(this.state.InitialPosition)
                { State = { Name = this.state.EntityName } };
            var insert = new engine.InsertSimulationEntity(robot);
            engine.SimulationEngine.GlobalInstancePort.Post(insert);
            yield return insert.ResponsePort.Choice();

            DefaultInsertResponseType response = insert.ResponsePort;
            if (response == null)
            {
                yield break;
            }

            // dont start listening to DSSP operations, other than drop, until notification of entity
            Activate(new Interleave(
                new TeardownReceiverGroup(
                    Arbiter.Receive<engine.InsertSimulationEntity>(
                        false, this.simulationEngineNotify, this.InsertEntityNotificationHandlerFirstTime),
                    Arbiter.Receive<DsspDefaultDrop>(false, this.mainPort, DefaultDropHandler),
                    Arbiter.Receive<DsspDefaultDrop>(false, this.drivePort, DefaultDropHandler)),
                new ExclusiveReceiverGroup(),
                new ConcurrentReceiverGroup()));

            var sub = new engine.SubscribeForSimulationEntity
                {
                    Body = { Name = this.state.EntityName },
                    NotificationPort = this.simulationEngineNotify
                };
            engine.SimulationEngine.GlobalInstancePort.Post(sub);
        }

        /// <summary>
        /// Inserts the entity first time.
        /// </summary>
        /// <param name="ins">The entity.</param>
        private void InsertEntityNotificationHandlerFirstTime(engine.InsertSimulationEntity ins)
        {
            this.driveEntity = ins.Body as ReferencePlatform2011Entity;

            this.driveEntity.ServiceContract = Contract.Identifier;

            // create default state based on the physics entity
            if (this.driveEntity.ChassisShape != null)
            {
                this.state.DriveState.DistanceBetweenWheels = this.driveEntity.ChassisShape.BoxState.Dimensions.X;
            }

            this.state.DriveState.LeftWheel.MotorState.PowerScalingFactor = this.driveEntity.MotorTorqueScaling;
            this.state.DriveState.RightWheel.MotorState.PowerScalingFactor = this.driveEntity.MotorTorqueScaling;

            base.Start();
        }
    }
    #endregion

    #region Motor Base
    /// <summary>
    /// Reference Platform variant of the motor base entity. It just specifies different physical properties in
    /// its custom constructor, otherwise uses the base class as is.
    /// </summary>
    [DataContract]
    public class ReferencePlatform2011Entity : engine.VisualEntity
    {
        /// <summary>
        /// The Speed Delta
        /// </summary>
        private const float SpeedDelta = 0.5f;

        /// <summary>
        /// The acceptable rotation error
        /// </summary>
        private const float AcceptableRotationError = 0.005f;
        
        /// <summary>
        /// The average kernel
        /// </summary>
        private const int AverageKernel = 6;

        /// <summary>
        /// The threshhold for deceleration
        /// </summary>
        private const int DecelerateThreshold = 6;

        /// <summary>
        /// Conveinence constant
        /// </summary>
        private const float TwoPI = (float)(2 * Math.PI);
        
        /// <summary>
        /// The maximum velocity
        /// </summary>
        private const float MaxVelocity = 20.0f;

        /// <summary>
        /// The minimum velocity
        /// </summary>
        private const float MinVelocity = -MaxVelocity;

        /// <summary>
        /// Thickness of platform
        /// </summary>
        protected const float ChassisThickness = 0.009525f;

        /// <summary>
        /// Depth from ground of the lowest platform
        /// </summary>
        protected const float ChassisDepthOffset = 0.03f;

        /// <summary>
        /// Depth from ground of the sensor platform
        /// </summary>
        protected const float ChassisSecondDepthOffset = 0.0978408f;

        /// <summary>
        /// Distance above platform that sensor point of origin floats
        /// </summary>
        protected const float SensorBoxHeightOffBase = 0.03f;

        #region State
        /// <summary>
        /// Chassis mass in kilograms
        /// </summary>
        private float mass;

        /// <summary>
        /// Chassis dimensions
        /// </summary>
        protected Vector3 chassisDimensions;
        
        /// <summary>
        /// Left front wheel position
        /// </summary>
        protected Vector3 leftFrontWheelPosition;
        
        /// <summary>
        /// Right front wheel position
        /// </summary>
        protected Vector3 rightFrontWheelPosition;
        
        /// <summary>
        /// Caster wheel position
        /// </summary>
        protected Vector3 casterWheelPosition;

        /// <summary>
        /// Distance from ground of chassis
        /// </summary>
        protected float chassisClearance;
        
        /// <summary>
        /// Mass of front wheels
        /// </summary>
        protected float frontWheelMass;
        
        /// <summary>
        /// Radius of front wheels
        /// </summary>
        protected float frontWheelRadius;
        
        /// <summary>
        /// Caster wheel radius
        /// </summary>
        protected float chassisWheelRadius;
        
        /// <summary>
        /// Front wheels width
        /// </summary>
        protected float frontWheelWidth;
        
        /// <summary>
        /// Caster wheel width
        /// </summary>
        protected float casterWheelWidth;
        
        /// <summary>
        /// Distance of the axle from the center of robot
        /// </summary>
        protected float frontAxleDepthOffset;

        /// <summary>
        /// The mesh to be used for the wheels
        /// </summary>
        private string wheelMesh;

        /// <summary>
        /// Gets or sets the mesh file for front wheels
        /// </summary>
        [Description("Gets or sets the mesh file for front wheels")]
        public string WheelMesh
        {
            get { return this.wheelMesh; }
            set { this.wheelMesh = value; }
        }

        /// <summary>
        /// True if drive mechanism is enabled
        /// </summary>
        private bool isEnabled;

        /// <summary>
        /// Gets or sets a value indicating whether the drive mechanism is enabled
        /// </summary>
        [DataMember]
        [Description("Gets or sets a value indicating whether the drive mechanism is enabled.")]
        public bool IsEnabled
        {
            get { return this.isEnabled; }
            set { this.isEnabled = value; }
        }

        /// <summary>
        /// Scaling factor to apply to motor torque requests
        /// </summary>
        private float motorTorqueScaling;

        /// <summary>
        /// Gets or sets the scaling factor to apply to motor torque requests
        /// </summary>
        [DataMember]
        [Description("Scaling factor to apply to motor torgue requests.")]
        public float MotorTorqueScaling
        {
            get { return this.motorTorqueScaling; }
            set { this.motorTorqueScaling = value; }
        }

        /// <summary>
        /// Right wheel child entity
        /// </summary>
        private engine.WheelEntity rightWheel;

        /// <summary>
        /// Gets or sets the right wheel child entity
        /// </summary>
        [DataMember]
        [Description("Right wheel child entity.")]
        public engine.WheelEntity RightWheel
        {
            get { return this.rightWheel; }
            set { this.rightWheel = value; }
        }
        
        /// <summary>
        /// Left wheel child entity
        /// </summary>
        private engine.WheelEntity leftWheel;

        /// <summary>
        /// Gets or sets the left wheel child entity
        /// </summary>
        [DataMember]
        [Description("Left wheel child entity.")]
        public engine.WheelEntity LeftWheel
        {
            get { return this.leftWheel; }
            set { this.leftWheel = value; }
        }

        /// <summary>
        /// The chassisShape
        /// </summary>
        private BoxShape chassisShape;

        /// <summary>
        /// Gets or sets the chassis physics shapes
        /// </summary>
        [DataMember]
        [Description("Chassis physics shapes.")]
        public BoxShape ChassisShape
        {
            get { return this.chassisShape; }
            set { this.chassisShape = value; }
        }
        
        /// <summary>
        /// Reference to the casterWheelShape
        /// </summary>
        private SphereShape casterWheelShape;

        /// <summary>
        /// Gets or sets the caster wheel physics shape
        /// </summary>
        [DataMember]
        [Description("Caster wheel physics shape.")]
        public SphereShape CasterWheelShape
        {
            get { return this.casterWheelShape; }
            set { this.casterWheelShape = value; }
        }

        /// <summary>
        /// Rotate Degrees Angle Threshold
        /// </summary>
        private double rotateDegreesAngleThreshold = 0.2f;

        /// <summary>
        /// Gets or sets the threshold, in radians, for stopping rotation
        /// </summary>
        [DataMember]
        [Description("Threshold for stopping scheduled rotation")]
        public double RotateDegreesAngleThreshold
        {
            get { return this.rotateDegreesAngleThreshold; }
            set { this.rotateDegreesAngleThreshold = value; }
        }
        
        /// <summary>
        /// Our starting pose
        /// </summary>
        private Pose startPoseForDriveDistance;

        /// <summary>
        /// The distance we must travel
        /// </summary>
        private double distanceToTravel;
        
        /// <summary>
        /// The port used for Drive distance requests
        /// </summary>
        private Port<engine.OperationResult> driveDistancePort = null;

        /// <summary>
        /// A queue of progress points
        /// </summary>
        private Queue<double> progressPoints = new Queue<double>();
        
        /// <summary>
        /// The time we began out drive distance command
        /// </summary>
        private DateTime startTime;

        /// <summary>
        /// Our target rotation
        /// </summary>
        private double targetRotation = double.MaxValue;
        
        /// <summary>
        /// Our current rotation
        /// </summary>
        private double currentRotation = 0;
        
        /// <summary>
        /// Last frames heading
        /// </summary>
        private double previousHeading = 0;
        
        /// <summary>
        /// The port used for rotate degrees
        /// </summary>
        private Port<engine.OperationResult> rotateDegreesPort = null;

        /// <summary>
        /// The timeout for DriveDistance and RotateDegrees commands in seconds.
        /// </summary>
        private float timeoutSeconds = 30.0f;

        /// <summary>
        /// Gets the current heading, in radians, of robot base
        /// </summary>
        public float CurrentHeading
        {
            get
            {
                // return the axis angle of the quaternion
                xna.Vector3 euler = UIMath.QuaternionToEuler(this.State.Pose.Orientation);
                return xna.MathHelper.ToRadians(euler.Y); // heading is the rotation about the Y axis.
            }
        }

        /// <summary>
        /// The current target velocity of the left wheel
        /// </summary>
        private float leftTargetVelocity;
        
        /// <summary>
        /// The current target velocity of the right wheel
        /// </summary>
        private float rightTargetVelocity;

        /// <summary>
        /// Physics share of the front wheel
        /// </summary>
        private SphereShape frontWheelShape;

        /// <summary>
        /// Gets or sets the Front wheel physics shape
        /// </summary>
        [DataMember]
        [Description("Front wheel physics shape.")]
        public SphereShape FrontWheelShape
        {
            get { return this.frontWheelShape; }
            set { this.frontWheelShape = value; }
        }

        /// <summary>
        /// Physics share of the rear wheel
        /// </summary>
        private SphereShape rearWheelShape;

        /// <summary>
        /// Gets or sets the Rear wheel physics shape
        /// </summary>
        [DataMember]
        [Description("rear wheel physics shape.")]
        public SphereShape RearWheelShape
        {
            get { return this.rearWheelShape; }
            set { this.rearWheelShape = value; }
        }

        /// <summary>
        /// The kinect entity
        /// </summary>
        private engine.KinectEntity kinect;

        /// <summary>
        /// Gets or sets the Kinect entity
        /// </summary>
        [DataMember]
        [Description("Kinect entity.")]
        public engine.KinectEntity Kinect
        {
            get { return this.kinect; }
            set { this.kinect = value; }
        }

        /// <summary>
        /// The left sonar
        /// </summary>
        private engine.SonarEntity sonarLeft;

        /// <summary>
        /// Gets or sets the Left Sonar Entity
        /// </summary>
        [DataMember]
        [Description("Left Sonar entity.")]
        public engine.SonarEntity LeftSonar
        {
            get { return this.sonarLeft; }
            set { this.sonarLeft = value; }
        }

        /// <summary>
        /// The right sonar
        /// </summary>
        private engine.SonarEntity sonarRight;

        /// <summary>
        /// Gets or sets the Right Sonar Entity
        /// </summary>
        [DataMember]
        [Description("Right Sonar entity.")]
        public engine.SonarEntity RightSonar
        {
            get { return this.sonarRight; }
            set { this.sonarRight = value; }
        }

        /// <summary>
        /// The front left IR
        /// </summary>
        private engine.IREntity irFrontLeft;

        /// <summary>
        /// Gets or sets the left IR entity
        /// </summary>
        [DataMember]
        [Description("IR entity.")]
        public engine.IREntity FrontLeftIR
        {
            get { return this.irFrontLeft; }
            set { this.irFrontLeft = value; }
        }

        /// <summary>
        /// The middle IR
        /// </summary>
        private engine.IREntity irFrontMiddle;

        /// <summary>
        /// Gets or sets the middle IR entity
        /// </summary>
        [DataMember]
        [Description("IR entity.")]
        public engine.IREntity FrontMiddleIR
        {
            get { return this.irFrontMiddle; }
            set { this.irFrontMiddle = value; }
        }

        /// <summary>
        /// The front right IR
        /// </summary>
        private engine.IREntity irFrontRight;

        /// <summary>
        /// Gets or sets the right IR entity
        /// </summary>
        [DataMember]
        [Description("IR entity.")]
        public engine.IREntity FrontRightIR
        {
            get { return this.irFrontRight; }
            set { this.irFrontRight = value; }
        }
        #endregion

        /// <summary>
        /// Port used for EntityContactNotification
        /// </summary>
        private Port<EntityContactNotification> notifications = new Port<EntityContactNotification>();

        /// <summary>
        /// Body strut positions
        /// </summary>
        protected Vector3[] bodyStrutPositions = new Vector3[] 
        { 
            new Vector3(0.1651f,  0.0f,  0.09398f),
            new Vector3(-0.1651f, 0.0f,  0.09398f),
            new Vector3(0.1651f,  0.0f, -0.09398f),
            new Vector3(-0.1651f, 0.0f, -0.09398f)
        };

        /// <summary>
        /// Body strut dimensions
        /// </summary>
        protected Vector3 bodyStrutDimension = new Vector3(0.0127f, 0.136525f, 0.0127f);

        /// <summary>
        /// Kinect strut positions
        /// </summary>
        protected Vector3[] kinectStrutPositions = new Vector3[] 
        { 
            new Vector3(0.0762f, 0.0f,  0.153289f),
            new Vector3(-0.0762f, 0.0f,  0.153289f),
        };

        /// <summary>
        /// Kinect strut dimensions
        /// </summary>
        protected Vector3 kinectStrutDimension = new Vector3(0.0127f, 0.3048f, 0.0127f);

        /// <summary>
        /// Kinect platform dimensions
        /// </summary>
        protected Vector3 kinectPlatformDimension = new Vector3(0.2032f, ChassisThickness, 0.0762f);

        /// <summary>
        /// Kinect platform position
        /// </summary>
        protected Vector3 kinectPlatformPosition = new Vector3(0.0f, 0.0f, 0.153289f);
        
        /// <summary>
        /// Sensor box dimensions
        /// </summary>
        protected Vector3 sensorBoxDimension = new Vector3(0.05f, 0.05f, 0.05f);
        
        /// <summary>
        /// Left IR position
        /// </summary>
        protected Vector3 leftIRBoxPosition = new Vector3(-0.17f, ChassisSecondDepthOffset + ChassisThickness + SensorBoxHeightOffBase, -0.1f);
        
        /// <summary>
        /// Middle IR position
        /// </summary>
        protected Vector3 middleIRBoxPosition = new Vector3(0.0f, ChassisSecondDepthOffset + ChassisThickness + SensorBoxHeightOffBase, -0.185f);
        
        /// <summary>
        /// Right IR position
        /// </summary>
        protected Vector3 rightIRBoxPosition = new Vector3(0.17f, ChassisSecondDepthOffset + ChassisThickness + SensorBoxHeightOffBase, -0.1f);

        /// <summary>
        /// Left sonar position
        /// </summary>
        protected Vector3 leftSonarBoxPosition = new Vector3(-0.09f, ChassisSecondDepthOffset + ChassisThickness + SensorBoxHeightOffBase, -0.175f);

        /// <summary>
        /// Right sonar position
        /// </summary>
        protected Vector3 rightSonarBoxPosition = new Vector3(0.09f, ChassisSecondDepthOffset + ChassisThickness + SensorBoxHeightOffBase, -0.175f);

        /// <summary>
        /// Sonars are angled out 30 degrees left of center. 
        /// </summary>
        protected const double LeftSonarAngleOutRadians = 30 / (180 / Math.PI);

        /// <summary>
        /// Left sonar orientation. 
        /// </summary>
        protected Quaternion leftSonarOrientation = Quaternion.FromAxisAngle(0f, 1f, 0f, (float)LeftSonarAngleOutRadians);

        /// <summary>
        /// Sonars are angled out 30 degrees right of center. 
        /// </summary>
        protected const double RightSonarAngleOutRadians = -30 / (180 / Math.PI);

        /// <summary>
        /// Right sonar orientation. 
        /// </summary>
        protected Quaternion rightSonarOrientation = Quaternion.FromAxisAngle(0f, 1f, 0f, (float)RightSonarAngleOutRadians);

        /// <summary>
        /// Default constructor, used for creating the entity from an XML description
        /// </summary>
        public ReferencePlatform2011Entity() 
        {
            this.InitializePhysicalAttributes();
        }

        /// <summary>
        /// Custom constructor for building model from hardcoded values. Used to create entity programmatically
        /// </summary>
        /// <param name="initialPos">The position to the place the reference platform at</param>
        public ReferencePlatform2011Entity(Vector3 initialPos)
        {
            this.State.Pose.Position = initialPos;
            this.InitializePhysicalAttributes();
        }

        /// <summary>
        /// Initializes the entity
        /// </summary>
        /// <param name="device">The graphics device</param>
        /// <param name="physicsEngine">The physics engine</param>
        public override void Initialize(xna.Graphics.GraphicsDevice device, PhysicsEngine physicsEngine)
        {
            try
            {
                this.InitError = string.Empty;
                this.ProgrammaticallyBuildModel(device, physicsEngine);

                // Set the parent entity for the wheel entities, clear any local rotation
                // on the wheel shape so that the wheel contact is always in the -Y direction.
                this.leftWheel.Parent = this;
                this.leftWheel.Wheel.State.LocalPose.Orientation = new Quaternion(0, 0, 0, 1);
                this.rightWheel.Parent = this;
                this.rightWheel.Wheel.State.LocalPose.Orientation = new Quaternion(0, 0, 0, 1);
                this.leftWheel.Initialize(device, physicsEngine);
                this.rightWheel.Initialize(device, PhysicsEngine);

                base.Initialize(device, physicsEngine);

                this.isEnabled = true;
            }
            catch (Exception ex)
            {
                // clean up
                if (this.PhysicsEntity != null)
                {
                    this.PhysicsEngine.DeleteEntity(PhysicsEntity);
                }

                this.HasBeenInitialized = false;
                this.InitError = ex.ToString();
            }
        }
        
        /// <summary>
        /// Sets the various dimensions of our physical components
        /// </summary>
        private void InitializePhysicalAttributes()
        {
            this.mass = 9.0f; // kg  (around 20 pounds)

            this.chassisDimensions = new Vector3(
                0.315f, // meters wide
                ChassisThickness,  // meters high
                0.315f); // meters long

            this.frontWheelMass = 0.01f;
            this.frontWheelRadius = 0.0799846f;
            this.chassisClearance = ChassisDepthOffset;
            this.chassisWheelRadius = 0.0377952f;
            this.frontWheelWidth = 0.03175f;
            this.casterWheelWidth = 0.05715f; // not currently used, but dim is accurate
            this.frontAxleDepthOffset = 0.0f; // distance of the axle from the center of robot

            this.State.MassDensity.Mass = this.mass;
            this.State.MassDensity.CenterOfMass = new Pose(new Vector3(0, this.chassisClearance, 0));

            // reference point for all shapes is the projection of
            // the center of mass onto the ground plane
            // (basically the spot under the center of mass, at Y = 0, or ground level)
            // NOTE: right/left is from the perspective of the robot, looking forward
            // NOTE: X = width of robot (right to left), Y = height, Z = length

            // chassis position
            BoxShapeProperties baseDesc = new BoxShapeProperties(
                "Create Body",
                this.mass,
                new Pose(
                    new Vector3(
                0, // Chassis center is also the robot center, so use zero for the X axis offset
                this.chassisClearance, // chassis is off the ground
                0.0f)), // minor offset in the z/depth axis
                this.chassisDimensions);

            this.ChassisShape = new BoxShape(baseDesc);

            // rear wheel is also called the caster
            this.casterWheelPosition = new Vector3(
                0, // center of chassis
                this.chassisWheelRadius, // distance from ground
                this.chassisDimensions.Z / 2); // at the rear of the robot

            this.rightFrontWheelPosition = new Vector3(
                +(this.chassisDimensions.X / 2) - (this.frontWheelWidth / 2) + 0.01f, // left of center
                this.frontWheelRadius, // distance from ground of axle
                this.frontAxleDepthOffset); // distance from center, on the z-axis

            this.leftFrontWheelPosition = new Vector3(
                -(this.chassisDimensions.X / 2) + (this.frontWheelWidth / 2) - 0.01f, // right of center
                this.frontWheelRadius, // distance from ground of axle
                this.frontAxleDepthOffset); // distance from center, on the z-axis

            this.MotorTorqueScaling = 20;

            this.MeshScale = new Vector3(0.0254f, 0.0254f, 0.0254f);

            this.ConstructStateMembers();

            // Add the wheel meshes separately
            this.LeftWheel.EntityState.Assets.Mesh = "Left_Tire.obj";
            this.LeftWheel.MeshScale = new Vector3(0.0254f, 0.0254f, 0.0254f);
            this.LeftWheel.MeshTranslation = new Vector3(this.chassisDimensions.X / 2, -this.frontWheelRadius, 0);
            this.RightWheel.EntityState.Assets.Mesh = "Right_Tire.obj";
            this.RightWheel.MeshScale = new Vector3(0.0254f, 0.0254f, 0.0254f);

            // Override the 180 degree rotation (that the Diff Drive applies) because we have separate wheel meshes
            this.RightWheel.MeshRotation = new Vector3(0, 0, 0);
            this.RightWheel.MeshTranslation = new Vector3(-this.chassisDimensions.X / 2, -this.frontWheelRadius, 0);
        }

        /// <summary>
        /// Self describes the reference platform
        /// </summary>
        /// <param name="device">The graphics device</param>
        /// <param name="physicsEngine">The physics engine</param>
        public void ProgrammaticallyBuildModel(Microsoft.Xna.Framework.Graphics.GraphicsDevice device, PhysicsEngine physicsEngine)
        {
            // add a front wheel
            Vector3 frontWheelPosition = new Vector3(
                0, // center of chassis
                this.chassisWheelRadius, // distance from ground
                (-this.chassisDimensions.Z / 2) + 0.000f); // at the front of the robot

            if (this.frontWheelShape == null)
            {
                this.frontWheelShape = new SphereShape(
                    new SphereShapeProperties(
                        "front wheel", 
                        0.001f,
                    new Pose(frontWheelPosition),
                    this.chassisWheelRadius));
                this.frontWheelShape.State.Name = this.EntityState.Name + "FrontWheel";

                // a fixed caster wheel has high friction when moving laterally, but low friction when it moves along the
                // body axis its aligned with. We use anisotropic friction to model this
                this.frontWheelShape.State.Material = new MaterialProperties("small friction with anisotropy", 0.5f, 0.5f, 1);
                this.frontWheelShape.State.Material.Advanced = new MaterialAdvancedProperties();
                this.frontWheelShape.State.Material.Advanced.AnisotropicDynamicFriction = 0.3f;
                this.frontWheelShape.State.Material.Advanced.AnisotropicStaticFriction = 0.4f;
                this.frontWheelShape.State.Material.Advanced.AnisotropyDirection = new Vector3(0, 0, 1);
            }

            State.PhysicsPrimitives.Add(this.frontWheelShape);

            // add a rear wheel
            Vector3 rearWheelPosition = new Vector3(
                0, // center of chassis
                this.chassisWheelRadius, // distance from ground
                (this.chassisDimensions.Z / 2) + 0.000f); // at the back of the robot

            if (this.rearWheelShape == null)
            {
                this.rearWheelShape = new SphereShape(
                    new SphereShapeProperties(
                        "rear wheel", 
                        0.001f,
                        new Pose(rearWheelPosition),
                        this.chassisWheelRadius));
                this.rearWheelShape.State.Name = this.EntityState.Name + "RearWheel";

                // a fixed caster wheel has high friction when moving laterally, but low friction when it moves along the
                // body axis its aligned with. We use anisotropic friction to model this
                this.rearWheelShape.State.Material = new MaterialProperties("small friction with anisotropy", 0.5f, 0.5f, 1);
                this.rearWheelShape.State.Material.Advanced = new MaterialAdvancedProperties();
                this.rearWheelShape.State.Material.Advanced.AnisotropicDynamicFriction = 0.3f;
                this.rearWheelShape.State.Material.Advanced.AnisotropicStaticFriction = 0.4f;
                this.rearWheelShape.State.Material.Advanced.AnisotropyDirection = new Vector3(0, 0, 1);
            }

            this.State.PhysicsPrimitives.Add(this.rearWheelShape);

            float accumulatedHeight = this.chassisDimensions.Y + ChassisSecondDepthOffset + (this.bodyStrutDimension.Y / 2.0f);

            foreach (Vector3 strutPosition in this.bodyStrutPositions)
            {
                BoxShapeProperties strutDesc = new BoxShapeProperties(
                    "strut", 
                    0.001f,
                    new Pose(
                        new Vector3(
                        strutPosition.X,
                        strutPosition.Y + accumulatedHeight,
                        strutPosition.Z)),
                    this.bodyStrutDimension);
                BoxShape strutShape = new BoxShape(strutDesc);
                State.PhysicsPrimitives.Add(strutShape);
            }

            accumulatedHeight += this.bodyStrutDimension.Y / 2.0f;

            if (this.irFrontLeft == null)
            {
                this.irFrontLeft = new engine.IREntity(new Pose(this.leftIRBoxPosition));
                this.irFrontLeft.EntityState.Name = this.EntityState.Name + "FrontLeftIR";
                InsertEntity(this.irFrontLeft);
            }

            if (this.irFrontRight == null)
            {
                this.irFrontRight = new engine.IREntity(new Pose(this.rightIRBoxPosition));
                this.irFrontRight.EntityState.Name = this.EntityState.Name + "FrontRightIR";
                this.InsertEntity(this.irFrontRight);
            }

            if (this.irFrontMiddle == null)
            {
                this.irFrontMiddle = new engine.IREntity(new Pose(this.middleIRBoxPosition));
                this.irFrontMiddle.EntityState.Name = this.EntityState.Name + "FrontMiddleIR";
                this.InsertEntity(this.irFrontMiddle);
            }

            if (this.sonarLeft == null)
            {
                this.sonarLeft = new engine.SonarEntity(
                    new Pose(
                        this.leftSonarBoxPosition,
                        this.leftSonarOrientation));
                this.sonarLeft.EntityState.Name = this.EntityState.Name + "LeftSonar";
                this.InsertEntity(this.sonarLeft);
            }

            if (this.sonarRight == null)
            {
                this.sonarRight = new engine.SonarEntity(
                    new Pose(
                        this.rightSonarBoxPosition,
                        this.rightSonarOrientation));
                this.sonarRight.EntityState.Name = this.EntityState.Name + "RightSonar";
                this.InsertEntity(this.sonarRight);
            }

            BoxShapeProperties topDesc = 
                new BoxShapeProperties(
                    "Top",
                    this.mass,
                    new Pose(
                        new Vector3(
                        0,
                        accumulatedHeight, // chassis is off the ground
                        0.0f)), // minor offset in the z/depth axis
                    this.chassisDimensions);

            BoxShape topShape = new BoxShape(topDesc);
            this.State.PhysicsPrimitives.Add(topShape);

            foreach (Vector3 kinectStrut in this.kinectStrutPositions)
            {
                BoxShapeProperties strutDesc = 
                    new BoxShapeProperties(
                        "kstrut", 
                        0.001f,
                    new Pose(
                        new Vector3(
                        kinectStrut.X,
                        kinectStrut.Y + accumulatedHeight + (this.kinectStrutDimension.Y / 2.0f),
                        kinectStrut.Z)),
                    this.kinectStrutDimension);
                BoxShape strutShape = new BoxShape(strutDesc);
                this.State.PhysicsPrimitives.Add(strutShape);
            }

            accumulatedHeight += this.kinectStrutDimension.Y;

            BoxShapeProperties kinectPlatformDesc = 
                new BoxShapeProperties(
                    "kplat", 
                    0.001f,
                    new Pose(
                        new Vector3(
                            this.kinectPlatformPosition.X,
                            this.kinectPlatformPosition.Y + accumulatedHeight + (this.kinectPlatformDimension.Y / 2.0f),
                            this.kinectPlatformPosition.Z)),
                    this.kinectPlatformDimension);
            BoxShape kinectPlatformShape = new BoxShape(kinectPlatformDesc);
            this.State.PhysicsPrimitives.Add(kinectPlatformShape);

            accumulatedHeight += this.kinectPlatformDimension.Y;

            if (this.Kinect == null)
            {
                this.Kinect = new engine.KinectEntity(new Vector3(this.kinectPlatformPosition.X, accumulatedHeight, this.kinectPlatformPosition.Z), this.EntityState.Name);
                this.InsertEntity(this.Kinect);
            }

            // Add the various meshes for this platform
            this.Meshes.Add(engine.SimulationEngine.ResourceCache.CreateMeshFromFile(device, "LowerDeck.obj"));
            this.Meshes.Add(engine.SimulationEngine.ResourceCache.CreateMeshFromFile(device, "UpperDeck.obj"));
            this.Meshes.Add(engine.SimulationEngine.ResourceCache.CreateMeshFromFile(device, "KinectStand.obj"));
            this.Meshes.Add(engine.SimulationEngine.ResourceCache.CreateMeshFromFile(device, "Laptop.obj"));
            this.Meshes.Add(engine.SimulationEngine.ResourceCache.CreateMeshFromFile(device, "Casters_Turned180.obj"));

            this.CreateAndInsertPhysicsEntity(physicsEngine);
        }

        /// <summary>
        /// Call this in the non-default constructor on an entity derived from
        /// DifferentialDriveEntity after setting CASTER_WHEEL_POSITION and 
        /// CASTER_WHEEL_RADIUS
        /// </summary>
        protected void ConstructStateMembers()
        {
            this.ConstructCasterWheelShape();
            this.ConstructWheels();
        }

        /// <summary>
        /// Constructs the wheel components
        /// </summary>
        protected void ConstructWheels()
        {
            // front left wheel
            WheelShapeProperties w = new WheelShapeProperties("front left wheel", this.frontWheelMass, this.frontWheelRadius);

            // Set this flag on both wheels if you want to use axle speed instead of torque
            w.Flags |= WheelShapeBehavior.OverrideAxleSpeed;
            w.InnerRadius = 0.7f * w.Radius;
            w.LocalPose = new Pose(this.leftFrontWheelPosition);
            this.leftWheel = new engine.WheelEntity(w);
            this.leftWheel.State.Name = this.EntityState.Name + "LeftWheel";
            this.leftWheel.State.Assets.Mesh = this.wheelMesh;
            this.leftWheel.Parent = this;

            // front right wheel
            w = new WheelShapeProperties("front right wheel", this.frontWheelMass, this.frontWheelRadius);
            w.Flags |= WheelShapeBehavior.OverrideAxleSpeed;
            w.InnerRadius = 0.7f * w.Radius;
            w.LocalPose = new Pose(this.rightFrontWheelPosition);
            this.rightWheel = new engine.WheelEntity(w);
            this.rightWheel.State.Name = State.Name + "RightWheel";
            this.rightWheel.State.Assets.Mesh = this.wheelMesh;
            this.rightWheel.MeshRotation = new Vector3(0, 180, 0);   // flip the wheel mesh
            this.rightWheel.Parent = this;
        }

        /// <summary>
        /// Constructs the caster wheel shape
        /// </summary>
        private void ConstructCasterWheelShape()
        {
            // add caster wheel as a basic sphere shape
            this.CasterWheelShape = new SphereShape(
                new SphereShapeProperties(
                    "rear wheel", 
                    0.001f,
                    new Pose(this.casterWheelPosition),
                    this.chassisWheelRadius));
            this.CasterWheelShape.State.Name = this.EntityState.Name + "CasterWheel";

            // a fixed caster wheel has high friction when moving laterely, but low friction when it moves along the
            // body axis its aligned with. We use anisotropic friction to model this
            this.CasterWheelShape.State.Material = new MaterialProperties("small friction with anisotropy", 0.5f, 0.5f, 1);
            this.CasterWheelShape.State.Material.Advanced = new MaterialAdvancedProperties();
            this.CasterWheelShape.State.Material.Advanced.AnisotropicDynamicFriction = 0.3f;
            this.CasterWheelShape.State.Material.Advanced.AnisotropicStaticFriction = 0.4f;
            this.CasterWheelShape.State.Material.Advanced.AnisotropyDirection = new Vector3(0, 0, 1);
        }

        /// <summary>
        /// Special dispose to handle embedded entities
        /// </summary>
        public override void Dispose()
        {
            if (this.leftWheel != null)
            {
                this.leftWheel.Dispose();
            }

            if (this.rightWheel != null)
            {
                this.rightWheel.Dispose();
            }

            base.Dispose();
        }

        /// <summary>
        /// Updates pose for our entity. We override default implementation
        /// since we control our own rendering when no file mesh is supplied, which means
        /// we dont need world transform updates
        /// </summary>
        /// <param name="update">The frame update message</param>
        public override void Update(engine.FrameUpdate update)
        {
            // update state for us and all the shapes that make up the rigid body
            this.PhysicsEntity.UpdateState(true);

            if (this.distanceToTravel > 0)
            {
                // DriveDistance update
                double currentDistance = Vector3.Length(this.State.Pose.Position - this.startPoseForDriveDistance.Position);
                if (currentDistance >= this.distanceToTravel)
                {
                    this.rightWheel.Wheel.AxleSpeed = 0;
                    this.leftWheel.Wheel.AxleSpeed = 0;
                    this.leftTargetVelocity = 0;
                    this.rightTargetVelocity = 0;
                    this.distanceToTravel = 0;

                    // now that we're finished, post a response
                    if (this.driveDistancePort != null)
                    {
                        Port<engine.OperationResult> tmp = this.driveDistancePort;
                        this.driveDistancePort = null;
                        tmp.Post(engine.OperationResult.Completed);
                    }
                }
                else if ((this.timeoutSeconds != 0) && (DateTime.Now - this.startTime).TotalSeconds > this.timeoutSeconds)
                {
                    if (this.driveDistancePort != null)
                    {
                        Port<engine.OperationResult> tmp = this.driveDistancePort;
                        this.driveDistancePort = null;
                        tmp.Post(engine.OperationResult.Error);
                    }
                }
                else
                {
                    // need to drive further, check to see if we should slow down
                    if (this.progressPoints.Count >= AverageKernel)
                    {
                        double distanceRemaining = this.distanceToTravel - currentDistance;
                        double framesToCompletion = Math.Abs(distanceRemaining * AverageKernel / (currentDistance - this.progressPoints.Dequeue()));
                        if (framesToCompletion < DecelerateThreshold)
                        {
                            if (Math.Abs(this.leftTargetVelocity) > 0.1)
                            {
                                this.leftTargetVelocity *= 0.5f;
                                this.rightTargetVelocity *= 0.5f;
                            }

                            this.progressPoints.Clear();
                        }
                    }

                    this.progressPoints.Enqueue(currentDistance);
                }
            }
            else if (this.targetRotation != double.MaxValue)
            {
                // RotateDegrees update
                float currentHeading = this.CurrentHeading;
                double angleDelta = currentHeading - this.previousHeading;

                while (angleDelta > Math.PI)
                {
                    angleDelta -= TwoPI;
                }

                while (angleDelta <= -Math.PI)
                {
                    angleDelta += TwoPI;
                }

                this.currentRotation += angleDelta;
                this.previousHeading = currentHeading;  // for next frame

                float angleError;
                if (this.targetRotation < 0)
                {
                    angleError = (float)(this.currentRotation - this.targetRotation);
                }
                else
                {
                    angleError = (float)(this.targetRotation - this.currentRotation);
                }

                if (angleError < AcceptableRotationError)
                {
                    // current heading is within acceptableError or has overshot
                    // end the rotation
                    this.targetRotation = double.MaxValue;
                    this.rightWheel.Wheel.AxleSpeed = 0;
                    this.leftWheel.Wheel.AxleSpeed = 0;
                    this.leftTargetVelocity = 0;
                    this.rightTargetVelocity = 0;

                    // now that we're finished, post a response
                    if (this.rotateDegreesPort != null)
                    {
                        Port<engine.OperationResult> tmp = this.rotateDegreesPort;
                        this.rotateDegreesPort = null;
                        tmp.Post(engine.OperationResult.Completed);
                    }
                }
                else if ((this.timeoutSeconds != 0) && (DateTime.Now - this.startTime).TotalSeconds > this.timeoutSeconds)
                {
                    if (this.rotateDegreesPort != null)
                    {
                        Port<engine.OperationResult> tmp = this.rotateDegreesPort;
                        this.rotateDegreesPort = null;
                        tmp.Post(engine.OperationResult.Error);
                    }
                }
                else
                {
                    if (angleDelta != 0)
                    {
                        // need to turn more, check to see if we should slow down
                        if (this.progressPoints.Count >= AverageKernel)
                        {
                            double framesToCompletion = Math.Abs(angleError * AverageKernel / (this.currentRotation - this.progressPoints.Dequeue()));
                            if (framesToCompletion < DecelerateThreshold)
                            {
                                if (Math.Abs(this.leftTargetVelocity) > 0.1)
                                {
                                    this.leftTargetVelocity *= 0.5f;
                                }

                                if (Math.Abs(this.rightTargetVelocity) > 0.1)
                                {
                                    this.rightTargetVelocity *= 0.5f;
                                }

                                this.progressPoints.Clear();
                            }
                        }

                        this.progressPoints.Enqueue(this.currentRotation);
                    }
                }
            }

            float left = this.leftWheel.Wheel.AxleSpeed + this.leftTargetVelocity;
            float right = this.rightWheel.Wheel.AxleSpeed + this.rightTargetVelocity;

            if (Math.Abs(left) > 0.1)
            {
                if (left > 0)
                {
                    this.leftWheel.Wheel.AxleSpeed -= SpeedDelta;
                }
                else
                {
                    this.leftWheel.Wheel.AxleSpeed += SpeedDelta;
                }
            }

            if (Math.Abs(right) > 0.1)
            {
                if (right > 0)
                {
                    this.rightWheel.Wheel.AxleSpeed -= SpeedDelta;
                }
                else
                {
                    this.rightWheel.Wheel.AxleSpeed += SpeedDelta;
                }
            }

            // if the drive is not enabled, it should not be moving.
            if (!this.isEnabled)
            {
                this.leftWheel.Wheel.AxleSpeed = 0;
                this.rightWheel.Wheel.AxleSpeed = 0;

                // cancel any pending operations
                if ((this.driveDistancePort != null) || (this.rotateDegreesPort != null))
                {
                    this.ResetRotationAndDistance();
                }
            }

            // update entities in fields
            this.leftWheel.Update(update);
            this.rightWheel.Update(update);

            // sim engine will update children
            base.Update(update);
        }

        /// <summary>
        /// Render entities stored as fields
        /// </summary>
        /// <param name="renderMode">The render mode</param>
        /// <param name="transforms">The transforms</param>
        /// <param name="currentCamera">The current camera</param>
        public override void Render(RenderMode renderMode, engine.MatrixTransforms transforms, engine.CameraEntity currentCamera)
        {
            var entityEffect = this.leftWheel.Effect;
            if (currentCamera.LensEffect != null)
            {
                this.leftWheel.Effect = currentCamera.LensEffect;
                this.rightWheel.Effect = currentCamera.LensEffect;
            }

            this.leftWheel.Render(renderMode, transforms, currentCamera);
            this.rightWheel.Render(renderMode, transforms, currentCamera);

            this.leftWheel.Effect = entityEffect;
            this.rightWheel.Effect = entityEffect;

            base.Render(renderMode, transforms, currentCamera);
        }

        #region Motor Base Control

        /// <summary>
        /// Applies constant power to both wheels, driving the motor base for a fixed distance, in the current direction
        /// </summary>
        /// <param name="distance">Distance to travel, in meters</param>
        /// <param name="power">Normalized power (torque) value for both wheels</param>
        /// <param name="responsePort">A port to report the result of the request, success or exception</param>
        public void DriveDistance(float distance, float power, Port<engine.OperationResult> responsePort)
        {
            if (!this.isEnabled)
            {
                responsePort.Post(engine.OperationResult.Error);
                return;
            }

            // reset any drivedistance or rotate degrees commands that haven't completed
            this.ResetRotationAndDistance();

            // keep track of the response port for when we complete the request
            this.driveDistancePort = responsePort;

            // handle negative distances
            if (distance < 0)
            {
                distance = -distance;
                power = -power;
            }

            this.startPoseForDriveDistance = this.State.Pose;
            this.distanceToTravel = distance;
            this.SetAxleVelocity(power * this.motorTorqueScaling, power * this.motorTorqueScaling);
            this.startTime = DateTime.Now;
        }

        /// <summary>
        /// Gets or sets the timeout for DriveDistance and RotateDegrees commands in seconds.
        /// </summary>
        [DataMember]
        public float TimeoutSeconds
        {
            get { return this.timeoutSeconds; }
            set { this.timeoutSeconds = value; }
        }

        /// <summary>
        /// Applies constant power to each wheel (but of inverse polarity), rotating the motor base 
        /// through the given rotation.
        /// </summary>
        /// <param name="degrees">Rotation around Y axis, in degrees.</param>
        /// <param name="power">Normalized power (torque) value for both wheels</param>
        /// <param name="responsePort">A port to report the result of the request, success or exception</param>
        public void RotateDegrees(float degrees, float power, Port<engine.OperationResult> responsePort)
        {
            if (!this.isEnabled)
            {
                responsePort.Post(engine.OperationResult.Error);
                return;
            }

            // reset any drivedistance or rotate degrees commands that haven't completed
            this.ResetRotationAndDistance();

            // keep track of the response port for when we complete the request
            this.rotateDegreesPort = responsePort;

            this.targetRotation = xna.MathHelper.ToRadians(degrees);
            this.currentRotation = 0;
            this.previousHeading = this.CurrentHeading;

            if (degrees < 0)
            {
                this.SetAxleVelocity(power * this.motorTorqueScaling, -power * this.motorTorqueScaling);
            }
            else
            {
                this.SetAxleVelocity(-power * this.motorTorqueScaling, power * this.motorTorqueScaling);
            }

            this.startTime = DateTime.Now;
        }

        /// <summary>
        /// When a direct update to motor torque or wheel velocity occurs
        /// we abandon any current DriveDistance or RotateDegrees commands
        /// </summary>
        /// <summary>
        /// When a direct update to motor torque or wheel velocity occurs
        /// we abandon any current DriveDistance or RotateDegrees commands
        /// </summary>
        private void ResetRotationAndDistance()
        {
            this.progressPoints.Clear();
            this.distanceToTravel = 0;
            this.targetRotation = double.MaxValue;

            if (this.driveDistancePort != null)
            {
                this.driveDistancePort.Post(engine.OperationResult.Canceled);
                this.driveDistancePort = null;
            }

            if (this.rotateDegreesPort != null)
            {
                this.rotateDegreesPort.Post(engine.OperationResult.Canceled);
                this.rotateDegreesPort = null;
            }
        }

        /// <summary>
        /// Sets motor torque on the active wheels
        /// </summary>
        /// <param name="leftWheelTorque">The left wheel torque</param>
        /// <param name="rightWheelTorque">The right wheel torque</param>
        public void SetMotorTorque(float leftWheelTorque, float rightWheelTorque)
        {
            if (this.leftWheel == null || this.rightWheel == null)
            {
                return;
            }

            // convert to velocity and call SetVelocity
            this.SetVelocity(
                leftWheelTorque * this.motorTorqueScaling * this.leftWheel.Wheel.State.Radius,
                rightWheelTorque * this.motorTorqueScaling * this.rightWheel.Wheel.State.Radius);
        }

        /// <summary>
        /// Sets angular velocity (radians/sec) on both wheels
        /// </summary>
        /// <param name="value">The new velocity</param>
        public void SetVelocity(float value)
        {
            this.ResetRotationAndDistance();
            this.SetVelocity(value, value);
        }

        /// <summary>
        /// Sets angular velocity on the wheels
        /// </summary>
        /// <param name="left">Velocity for left wheel</param>
        /// <param name="right">Velocity for right wheel</param>
        public void SetVelocity(float left, float right)
        {
            this.ResetRotationAndDistance();
            if (this.leftWheel == null || this.rightWheel == null)
            {
                return;
            }

            left = this.ValidateWheelVelocity(left);
            right = this.ValidateWheelVelocity(right);

            // v is in m/sec - convert to an axle speed
            //  2Pi(V/2PiR) = V/R
            this.SetAxleVelocity(
                left / this.leftWheel.Wheel.State.Radius,
                right / this.rightWheel.Wheel.State.Radius);
        }

        /// <summary>
        /// Sets axle velocity
        /// </summary>
        /// <param name="left">The left axle velocity</param>
        /// <param name="right">The right axle velocity</param>
        private void SetAxleVelocity(float left, float right)
        {
            // if not enabled, don't move.
            if (!this.isEnabled)
            {
                left = right = 0;
            }

            this.leftTargetVelocity = left;
            this.rightTargetVelocity = right;
        }

        /// <summary>
        /// Validates a velocity
        /// </summary>
        /// <param name="value">Velocity to validate</param>
        /// <returns>The clamped value</returns>
        private float ValidateWheelVelocity(float value)
        {
            if (value > MaxVelocity)
            {
                return MaxVelocity;
            }

            if (value < MinVelocity)
            {
                return MinVelocity;
            }

            return value;
        }
        #endregion
        
        /// <summary>
        /// Adds a notification port to the list of subscriptions that get notified when the bumper shapes
        /// collide in the physics world
        /// </summary>
        /// <param name="notificationTarget">The target of the notification</param>
        public void Subscribe(Port<EntityContactNotification> notificationTarget)
        {
            PhysicsEntity.SubscribeForContacts(notificationTarget);
        }
    }
    #endregion
}