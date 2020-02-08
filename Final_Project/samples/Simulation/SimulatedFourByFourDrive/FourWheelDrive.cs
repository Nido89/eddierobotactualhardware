//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: FourWheelDrive.cs $ $Revision: 6 $
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

using Microsoft.Dss.Core.Attributes;
using Microsoft.Robotics.PhysicalModel;
using Microsoft.Robotics.Simulation;
using Microsoft.Robotics.Simulation.Engine;
using Microsoft.Robotics.Simulation.Physics;

using xna = Microsoft.Xna.Framework;
using xnagrfx = Microsoft.Xna.Framework.Graphics;
using xnaprof = Microsoft.Robotics.Simulation.MeshLoader;

namespace Microsoft.Robotics.Services.Samples.SimulatedFourByFourDrive
{
    /// <summary>
    /// Base 4x4 entity class
    /// </summary>
    [DataContract]
    [Editor(typeof(GenericObjectEditor), typeof(System.Drawing.Design.UITypeEditor))]
    [Browsable(false)] // prevent from being displayed in NewEntity dialog
    public class FourWheelDriveEntity : VisualEntity
    {
        #region State
        /// <summary>
        /// Chassis mass in kilograms
        /// </summary>
        public float Mass;

        /// <summary>
        /// Chassis dimensions
        /// </summary>
        protected Vector3 ChassisDimensions;
        /// <summary>
        /// Left front wheel position
        /// </summary>
        protected Vector3 LeftFrontWheelPosition;
        /// <summary>
        /// Right front wheel position
        /// </summary>
        protected Vector3 RightFrontWheelPosition;
        /// <summary>
        /// Left rear wheel position
        /// </summary>
        protected Vector3 LeftRearWheelPosition;
        /// <summary>
        /// Right rear wheel position
        /// </summary>
        protected Vector3 RightRearWheelPosition;
        /// <summary>
        /// Distance from ground of chassis
        /// </summary>
        protected float ChassisClearance;
        /// <summary>
        /// Mass of front wheels
        /// </summary>
        protected float FrontWheelMass;
        /// <summary>
        /// Radius of front wheels
        /// </summary>
        protected float FrontWheelRadius;
        /// <summary>
        /// Front wheels width
        /// </summary>
        protected float FrontWheelWidth;
        /// <summary>
        /// Mass of front wheels
        /// </summary>
        protected float RearWheelMass;
        /// <summary>
        /// Radius of front wheels
        /// </summary>
        protected float RearWheelRadius;
        /// <summary>
        /// Front wheels width
        /// </summary>
        protected float RearWheelWidth;
        /// <summary>
        /// distance of the axle from the center of robot
        /// </summary>
        protected float FrontAxleDepthOffset;
        /// <summary>
        /// distance of the axle from the center of robot
        /// </summary>
        protected float RearAxleDepthOffset;

        string _frontWheelMesh;
        /// <summary>
        /// Mesh file to use for front wheels
        /// </summary>
        [DataMember]
        public string FrontWheelMesh
        {
            get { return _frontWheelMesh; }
            set { _frontWheelMesh = value; }
        }

        string _rearWheelMesh;
        /// <summary>
        /// Mesh file to use for rear wheels
        /// </summary>
        [DataMember]
        public string RearWheelMesh
        {
            get { return _rearWheelMesh; }
            set { _rearWheelMesh = value; }
        }

        bool _isEnabled;
        /// <summary>
        /// True if the drive mechanism is enabled.
        /// </summary>
        [DataMember]
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set { _isEnabled = value; }
        }

        float _motorTorqueScaling;
        /// <summary>
        /// Scaling factor to apply to motor torgue requests.
        /// </summary>
        [DataMember]
        public float MotorTorqueScaling
        {
            get { return _motorTorqueScaling; }
            set { _motorTorqueScaling = value; }
        }
        
        WheelEntity _rightFrontWheel;
        /// <summary>
        /// Right front wheel child entity.
        /// </summary>
        [DataMember]
        public WheelEntity RightFrontWheel
        {
            get { return _rightFrontWheel; }
            set { _rightFrontWheel = value; }
        }

        WheelEntity _leftFrontWheel;
        /// <summary>
        /// Left front wheel child entity.
        /// </summary>
        [DataMember]
        public WheelEntity LeftFrontWheel
        {
            get { return _leftFrontWheel; }
            set { _leftFrontWheel = value; }
        }

        private WheelEntity _rightRearWheel;
        /// <summary>
        /// Right rear wheel child entity.
        /// </summary>
        [DataMember]
        public WheelEntity RightRearWheel
        {
            get { return _rightRearWheel; }
            set { _rightRearWheel = value; }
        }

        private WheelEntity _leftRearWheel;
        /// <summary>
        /// Left rear wheel child entity.
        /// </summary>
        [DataMember]
        public WheelEntity LeftRearWheel
        {
            get { return _leftRearWheel; }
            set { _leftRearWheel = value; }
        }
	

        List<BoxShape> _chassisShape;
        /// <summary>
        /// Chassis physics shapes.
        /// </summary>
        [DataMember]
        public List<BoxShape> ChassisShape
        {
            get { return _chassisShape; }
            set { _chassisShape = value; }
        }

        private float _distanceBetweenWheels;
        /// <summary>
        /// Distance between wheels on the same axle
        /// </summary>
        [DataMember]
        public float DistanceBetweenWheels
        {
            get { return _distanceBetweenWheels; }
            set { _distanceBetweenWheels = value; }
        }

        private float _wheelBase;
        /// <summary>
        /// Distance between the front and rear axles
        /// </summary>
        [DataMember]
        public float WheelBase
        {
            get { return _wheelBase; }
            set { _wheelBase = value; }
        }

        private float _suspensionTravel;
        /// <summary>
        /// Maximum suspension travel in the -Y axis direction
        /// </summary>
        [DataMember]
        public float SuspensionTravel
        {
            get { return _suspensionTravel; }
            set { _suspensionTravel = value; }
        }
	

        #endregion

        /// <summary>
        /// Default constructor
        /// </summary>
        public FourWheelDriveEntity() { }

        /// <summary>
        /// Initialization
        /// </summary>
        /// <param name="device"></param>
        /// <param name="physicsEngine"></param>
        public override void Initialize(xnagrfx.GraphicsDevice device, PhysicsEngine physicsEngine)
        {
            try
            {
                InitError = string.Empty;

                ProgrammaticallyBuildModel(device, physicsEngine);

                _leftFrontWheel.Parent = this;
                _rightFrontWheel.Parent = this;
                _leftRearWheel.Parent = this;
                _rightRearWheel.Parent = this;

                _leftFrontWheel.Initialize(device, physicsEngine);
                _rightFrontWheel.Initialize(device, PhysicsEngine);
                _leftRearWheel.Initialize(device, PhysicsEngine);
                _rightRearWheel.Initialize(device, PhysicsEngine);

                base.Initialize(device, physicsEngine);
            }
            catch (Exception ex)
            {
                // clean up
                if (PhysicsEntity != null)
                {
                    PhysicsEngine.DeleteEntity(PhysicsEntity);
                }

                HasBeenInitialized = false;
                InitError = ex.ToString();
            }
        }

        /// <summary>
        /// Builds the simulated robotic entity using local fields for position size, orientation
        /// </summary>
        /// <param name="device"></param>
        /// <param name="physicsEngine"></param>
        public void ProgrammaticallyBuildModel(xnagrfx.GraphicsDevice device, PhysicsEngine physicsEngine)
        {
            if (_chassisShape != null)
            {
                foreach (BoxShape shape in _chassisShape)
                {
                    base.State.PhysicsPrimitives.Add(shape);
                }
            }

            base.CreateAndInsertPhysicsEntity(physicsEngine);

            // increase physics fidelity
            base.PhysicsEntity.SolverIterationCount = 64;

            // if we were created from xml the wheel entities would already be instantiated
            if (_leftFrontWheel != null &&
                _rightFrontWheel != null &&
                _leftRearWheel != null &&
                _rightRearWheel != null)
            {
                return;
            }

            // front left wheel
            WheelShapeProperties w = new WheelShapeProperties("front left wheel", FrontWheelMass, FrontWheelRadius);
            w.Flags |= WheelShapeBehavior.OverrideAxleSpeed;
            w.InnerRadius = 0.7f * w.Radius;
            w.LocalPose = new Pose(LeftFrontWheelPosition);
            w.SuspensionTravel = _suspensionTravel;
            w.Suspension = new SpringProperties(10, 5, 0.5f);

            _leftFrontWheel = new WheelEntity(w);
            _leftFrontWheel.State.Name = State.Name + ":" + "Left front wheel";
            _leftFrontWheel.State.Assets.Mesh = _frontWheelMesh;
            _leftFrontWheel.Parent = this;

            // front right wheel
            w = new WheelShapeProperties("front right wheel", FrontWheelMass, FrontWheelRadius);
            w.Flags |= WheelShapeBehavior.OverrideAxleSpeed;
            w.InnerRadius = 0.7f * w.Radius;
            w.LocalPose = new Pose(RightFrontWheelPosition);
            w.SuspensionTravel = _suspensionTravel;
            w.Suspension = new SpringProperties(10, 5, 0.5f);
            _rightFrontWheel = new WheelEntity(w);
            _rightFrontWheel.State.Name = State.Name + ":" + "Right front wheel";
            _rightFrontWheel.State.Assets.Mesh = _frontWheelMesh;
            _rightFrontWheel.Parent = this;

            // rear left wheel
            w = new WheelShapeProperties("rear left wheel", RearWheelMass, RearWheelRadius);
            w.Flags |= WheelShapeBehavior.OverrideAxleSpeed;
            w.InnerRadius = 0.7f * w.Radius;
            w.LocalPose = new Pose(LeftRearWheelPosition);
            w.SuspensionTravel = _suspensionTravel;
            w.Suspension = new SpringProperties(10, 5, 0.5f);
            _leftRearWheel = new WheelEntity(w);
            _leftRearWheel.State.Name = State.Name + ":" + "Left rear wheel";
            _leftRearWheel.State.Assets.Mesh = _rearWheelMesh;
            _leftRearWheel.Parent = this;

            // front right wheel
            w = new WheelShapeProperties("rear right wheel", RearWheelMass, RearWheelRadius);
            w.Flags |= WheelShapeBehavior.OverrideAxleSpeed;
            w.InnerRadius = 0.7f * w.Radius;
            w.LocalPose = new Pose(RightRearWheelPosition);
            w.SuspensionTravel = _suspensionTravel;
            w.Suspension = new SpringProperties(10, 5, 0.5f);
            _rightRearWheel = new WheelEntity(w);
            _rightRearWheel.State.Name = State.Name + ":" + "Right rear wheel";
            _rightRearWheel.State.Assets.Mesh = _rearWheelMesh;
            _rightRearWheel.Parent = this;
        }

        /// <summary>
        /// Special dispose to handle embedded entities 
        /// </summary>
        public override void Dispose()
        {
            if (_leftFrontWheel != null)
            {
                _leftFrontWheel.Dispose();
            }

            if (_rightFrontWheel != null)
            {
                _rightFrontWheel.Dispose();
            }

            if (_leftRearWheel != null)
            {
                _leftRearWheel.Dispose();
            }

            if (_rightRearWheel != null)
            {
                _rightRearWheel.Dispose();
            }

            base.Dispose();
        }


        /// <summary>
        /// Updates pose for our entity. We override default implementation
        /// since we control our own rendering when no file mesh is supplied, which means
        /// we dont need world transform updates
        /// </summary>
        /// <param name="update"></param>
        public override void Update(FrameUpdate update)
        {
            float leftFront = _leftFrontWheel.Wheel.AxleSpeed + _leftFrontTargetVelocity;
            float rightFront = _rightFrontWheel.Wheel.AxleSpeed + _rightFrontTargetVelocity;
            float leftRear = _leftRearWheel.Wheel.AxleSpeed + _leftRearTargetVelocity;
            float rightRear = _rightRearWheel.Wheel.AxleSpeed + _rightRearTargetVelocity;

            float timeStep = (float)update.ElapsedTime;
            ModulateWheelSpeed(leftFront, _leftFrontWheel, timeStep);
            ModulateWheelSpeed(rightFront, _rightFrontWheel, timeStep);
            ModulateWheelSpeed(leftRear, _leftRearWheel, timeStep);
            ModulateWheelSpeed(rightRear, _rightRearWheel, timeStep);

            // update state for us and all the shapes that make up the rigid body
            PhysicsEntity.UpdateState(true);

            // update entities in fields
            _leftFrontWheel.Update(update);
            _rightFrontWheel.Update(update);
            _leftRearWheel.Update(update);
            _rightRearWheel.Update(update);

            // sim engine will update children
            base.Update(update);
        }

        private void AllStop()
        {
            _rightFrontWheel.Wheel.AxleSpeed = 0;
            _leftFrontWheel.Wheel.AxleSpeed = 0;

            _leftFrontTargetVelocity = 0;
            _rightFrontTargetVelocity = 0;

            _rightRearWheel.Wheel.AxleSpeed = 0;
            _leftFrontWheel.Wheel.AxleSpeed = 0;

            _leftRearTargetVelocity = 0;
            _rightRearTargetVelocity = 0;
        }

        private void ModulateWheelSpeed(float speed, WheelEntity wheel, float timeStep)
        {
            if (Math.Abs(speed) > 0.1)
            {
                if (speed > 0)
                {
                    wheel.Wheel.AxleSpeed -= SpeedDelta * timeStep;
                }
                else
                {
                    wheel.Wheel.AxleSpeed += SpeedDelta * timeStep;
                }
            }
        }

        /// <summary>
        /// Renders 4x4 body and all of its four wheels
        /// </summary>
        /// <param name="renderMode"></param>
        /// <param name="transforms"></param>
        /// <param name="currentCamera"></param>
        public override void Render(RenderMode renderMode, MatrixTransforms transforms, CameraEntity currentCamera)
        {
            _leftFrontWheel.Render(renderMode, transforms, currentCamera);
            _rightFrontWheel.Render(renderMode, transforms, currentCamera);
            _leftRearWheel.Render(renderMode, transforms, currentCamera);
            _rightRearWheel.Render(renderMode, transforms, currentCamera);

            base.Render(renderMode, transforms, currentCamera);
        }

        #region Motor Base Control

        const float SpeedDelta = 5.0f;
        /// <summary>
        /// The rotation in radians about the Y axis the 4x4 is oriented
        /// </summary>
        public float CurrentHeading
        {
            get
            {
                // return the axis angle of the quaternion
                xna.Vector3 euler = UIMath.QuaternionToEuler(State.Pose.Orientation);
                return xna.MathHelper.ToRadians(euler.Y); // heading is the rotation about the Y axis.
            }
        }

        /// <summary>
        /// Specifies the motor torque for the wheels of the 4x4
        /// </summary>
        /// <param name="leftWheel">Motor torque for both left wheels</param>
        /// <param name="rightWheel">Motor torque for both right wheels</param>
        public void SetMotorTorque(float leftWheel, float rightWheel)
        {
            SetMotorTorque(leftWheel, rightWheel, leftWheel, rightWheel);
        }

        /// <summary>
        /// Specifies the motor torque for the wheels of the 4x4
        /// </summary>
        /// <param name="frontLeftWheel">Motor torque for front left wheel</param>
        /// <param name="frontRightWheel">Motor torque for front right wheel</param>
        /// <param name="rearLeftWheel">Motor torque for rear left wheel</param>
        /// <param name="rearRightWheel">Motor torque for rear right wheel</param>
        public void SetMotorTorque(float frontLeftWheel, float frontRightWheel, float rearLeftWheel, float rearRightWheel)
        {
            SetAxleVelocity(
                frontLeftWheel * _motorTorqueScaling,
                frontRightWheel * _motorTorqueScaling,
                rearLeftWheel * _motorTorqueScaling,
                rearRightWheel * _motorTorqueScaling
            );
        }

        float _leftFrontTargetVelocity;
        float _rightFrontTargetVelocity;
        float _leftRearTargetVelocity;
        float _rightRearTargetVelocity;

        /// <summary>
        /// Specifies the wheel velocity for all wheels in the 4x4
        /// </summary>
        /// <param name="value"></param>
        public void SetVelocity(float value)
        {
            SetVelocity(value, value, value, value);
        }

        /// <summary>
        /// Specifies the wheel velocity for the wheels in the 4x4
        /// </summary>
        /// <param name="left">Velocity for both left wheels</param>
        /// <param name="right">Velocity for both right wheels</param>
        public void SetVelocity(float left, float right)
        {
            SetVelocity(left, right, left, right);
        }

        /// <summary>
        /// Specifies the wheel velocity for the wheels in the 4x4
        /// </summary>
        /// <param name="leftFront">Velocity for the front left wheel</param>
        /// <param name="rightFront">Velocity for the front right wheel</param>
        /// <param name="leftRear">Velocity for the rear left wheel</param>
        /// <param name="rightRear">Velocity for the rear right wheel</param>
        public void SetVelocity(float leftFront, float rightFront, float leftRear, float rightRear)
        {
            if (_leftFrontWheel == null ||
                _rightFrontWheel == null ||
                _leftRearWheel == null ||
                _rightRearWheel == null)
            {
                return;
            }

            leftFront = ValidateWheelVelocity(leftFront);
            rightFront = ValidateWheelVelocity(rightFront);
            leftRear = ValidateWheelVelocity(leftRear);
            rightRear = ValidateWheelVelocity(rightRear);

            // v is in m/sec - convert to an axle speed
            //  2Pi(V/2PiR) = V/R
            SetAxleVelocity(
                leftFront / _leftFrontWheel.Wheel.State.Radius,
                rightFront / _rightFrontWheel.Wheel.State.Radius,
                leftRear / _leftRearWheel.Wheel.State.Radius,
                rightRear / _rightRearWheel.Wheel.State.Radius
            );
        }

        private void SetAxleVelocity(float left, float right)
        {
            SetAxleVelocity(left, right, left, right);
        }

        private void SetAxleVelocity(float leftFront, float rightFront, float leftRear, float rightRear)
        {
            _leftFrontTargetVelocity = leftFront;
            _rightFrontTargetVelocity = rightFront;
            _leftRearTargetVelocity = leftRear;
            _rightRearTargetVelocity = rightRear;
        }

        /// <summary>
        /// Manually sets angles of the four wheels
        /// </summary>
        /// <param name="leftFront"></param>
        /// <param name="rightFront"></param>
        /// <param name="leftRear"></param>
        /// <param name="rightRear"></param>
        public void SetWheelAngles(float leftFront, float rightFront, float leftRear, float rightRear)
        {
            if (_leftFrontWheel == null ||
                _rightFrontWheel == null ||
                _leftRearWheel == null ||
                _rightRearWheel == null)
            {
                return;
            }

            leftFront = ValidateWheelAngle(leftFront);
            rightFront = ValidateWheelAngle(rightFront);
            leftRear = ValidateWheelAngle(leftRear);
            rightRear = ValidateWheelAngle(rightRear);

            _leftFrontWheel.Wheel.SteerAngle = leftFront;
            _rightFrontWheel.Wheel.SteerAngle = rightFront;
            _leftRearWheel.Wheel.SteerAngle = leftRear;
            _rightRearWheel.Wheel.SteerAngle = rightRear;

        }

        /// <summary>
        /// Specifies the maximum angle (in radians) a wheel can turn
        /// </summary>
        protected float MaxSteerAngle = (float)(Math.PI / 4);

        private float ValidateWheelAngle(float angle)
        {
            if (angle > MaxSteerAngle)
            {
                return MaxSteerAngle;
            }
            else if (angle < -MaxSteerAngle)
            {
                return -MaxSteerAngle;
            }
            else
            {
                return angle;
            }
        }

        const float MaxVelocity = 20.0f;
        const float MinVelocity = -MaxVelocity;

        float ValidateWheelVelocity(float value)
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
    }

    /// <summary>
    /// SimpleFourByFour is a simple 4x4 entity derived from FourWheelDriveEntity
    /// </summary>
    [DataContract]
    public class SimpleFourByFour : FourWheelDriveEntity
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public SimpleFourByFour() 
        { 
        }

        /// <summary>
        /// Initialization constructor
        /// </summary>
        /// <param name="initialPos"></param>
        public SimpleFourByFour(Vector3 initialPos)
        {
            Mass = 400;

            // the default settings are pulled out of thin air
            ChassisDimensions = new Vector3(1.5f, 0.5f, 2f);
            ChassisClearance = 0.2f;
            FrontWheelRadius = 0.3f;
            RearWheelRadius = 0.3f;
            FrontWheelWidth = 0.3f; // not used?
            RearWheelWidth = 0.3f; // not used?
            FrontAxleDepthOffset = -0.7f; // distance of the axle from the center of robot
            RearAxleDepthOffset = 0.7f; // distance of the axle from the center of the robot
            WheelBase = RearAxleDepthOffset - FrontAxleDepthOffset;
            DistanceBetweenWheels = ChassisDimensions.X;

            base.State.Name = "MotorBaseWithFourWheels";
            base.State.MassDensity.Mass = Mass;
            base.State.Pose.Position = initialPos;

            // reference point for all shapes is the projection of
            // the center of mass onto the ground plane 
            // (basically the spot under the center of mass, at Y = 0, or ground level)

            // chassis position
            BoxShapeProperties motorBaseDesc = new BoxShapeProperties(
                "chassis", 
                Mass,
                new Pose(
                    new Vector3(
                    0, // Chassis center is also the robot center, so use zero for the X axis offset
                    ChassisClearance + ChassisDimensions.Y / 2, // chassis is off the ground and its center is DIM.Y/2 above the clearance
                    0)
                ), // no offset in the z/depth axis, since again, its center is the robot center
                ChassisDimensions);

            motorBaseDesc.Material = new MaterialProperties("high friction", 0.0f, 1.0f, 20.0f);
            motorBaseDesc.Name = "Chassis";
            ChassisShape = new List<BoxShape>();
            ChassisShape.Add(new BoxShape(motorBaseDesc));

            

            // NOTE: right/left is from the perspective of the robot, looking forward

            FrontWheelMass = 10;
            RearWheelMass = 10;

            RightFrontWheelPosition = new Vector3(
                (ChassisDimensions.X  + FrontWheelWidth) / 2,// left of center
                FrontWheelRadius,// distance from ground of axle
                FrontAxleDepthOffset
            ); // distance from center, on the z-axis

            LeftFrontWheelPosition = new Vector3(
                -(ChassisDimensions.X + FrontWheelWidth) / 2,// left of center
                FrontWheelRadius,// distance from ground of axle
                FrontAxleDepthOffset
            ); // distance from center, on the z-axis


            RightRearWheelPosition = new Vector3(
                (ChassisDimensions.X + RearWheelWidth) / 2,// left of center
                RearWheelRadius,// distance from ground of axle
                RearAxleDepthOffset
            ); // distance from center, on the z-axis

            LeftRearWheelPosition = new Vector3(
                -(ChassisDimensions.X + RearWheelWidth) / 2,// left of center
                RearWheelRadius,// distance from ground of axle
                RearAxleDepthOffset
            ); // distance from center, on the z-axis

            MotorTorqueScaling = 20;
        }
    }

    /// <summary>
    /// Sample4x4Vehicle is the entity used in this sample
    /// </summary>
    [DataContract]
    public class Sample4x4Vehicle : FourWheelDriveEntity
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public Sample4x4Vehicle()
        {
        }

        /// <summary>
        /// Initialization constructor
        /// </summary>
        /// <param name="initialPos"></param>
        public Sample4x4Vehicle(Vector3 initialPos)
        {
            Mass = 1200;

            ChassisDimensions = new Vector3(1.61f, 0.58f, 2.89f);
            ChassisClearance = 0.43f;
            FrontWheelRadius = 0.35f;
            RearWheelRadius = 0.35f;
            FrontWheelWidth = 0.17f; // not used?
            RearWheelWidth = 0.17f; // not used?
            FrontAxleDepthOffset = -1.08f; // distance of the axle from the center of robot
            RearAxleDepthOffset = 0.83f; // distance of the axle from the center of the robot
            WheelBase = RearAxleDepthOffset - FrontAxleDepthOffset;
            DistanceBetweenWheels = ChassisDimensions.X;
            SuspensionTravel = 0.2f;
            

            FrontWheelMesh = "4x4Wheel.obj";
            RearWheelMesh = "4x4Wheel.obj";

            State.Assets.Mesh = "4x4Body.obj";

            base.State.Name = "MotorBaseWithFourWheels";
            base.State.MassDensity.Mass = Mass;
            base.State.Pose.Position = initialPos;

            // reference point for all shapes is the projection of
            // the center of mass onto the ground plane 
            // (basically the spot under the center of mass, at Y = 0, or ground level)

            // chassis position
            BoxShapeProperties chassis = new BoxShapeProperties(
                "chassis",
                Mass * 5 / 6,
                new Pose(
                    new Vector3(
                        0, 
                        ChassisClearance + ChassisDimensions.Y / 2, 
                        0
                    )
                ), 
                ChassisDimensions
            );
            chassis.Material = new MaterialProperties("high friction", 0.0f, 1.0f, 20.0f);

            float proportion = 2f/3f;

            BoxShapeProperties cabin = new BoxShapeProperties(
                "cabin",
                Mass / 6,
                new Pose(
                    new Vector3(
                        0,
                        ChassisClearance + 3 * ChassisDimensions.Y / 2,
                        ChassisDimensions.Z * (1 - proportion) / 2
                    )
                ),
                new Vector3(
                    ChassisDimensions.X,
                    ChassisDimensions.Y,
                    ChassisDimensions.Z * proportion
                )
            );
            cabin.Material = chassis.Material;

            ChassisShape = new List<BoxShape>();
            ChassisShape.Add(new BoxShape(chassis));
            ChassisShape.Add(new BoxShape(cabin));
                                

            // NOTE: right/left is from the perspective of the robot, looking forward

            FrontWheelMass = 10;
            RearWheelMass = 10;

            RightFrontWheelPosition = new Vector3(
                (ChassisDimensions.X + 0.001f) / 2,// left of center
                FrontWheelRadius,// distance from ground of axle
                FrontAxleDepthOffset
            ); // distance from center, on the z-axis

            LeftFrontWheelPosition = new Vector3(
                -(ChassisDimensions.X + 0.001f) / 2,// left of center
                FrontWheelRadius,// distance from ground of axle
                FrontAxleDepthOffset
            ); // distance from center, on the z-axis


            RightRearWheelPosition = new Vector3(
                (ChassisDimensions.X + 0.001f) / 2,// left of center
                RearWheelRadius,// distance from ground of axle
                RearAxleDepthOffset
            ); // distance from center, on the z-axis

            LeftRearWheelPosition = new Vector3(
                -(ChassisDimensions.X + 0.001f) / 2,// left of center
                RearWheelRadius,// distance from ground of axle
                RearAxleDepthOffset
            ); // distance from center, on the z-axis

            MotorTorqueScaling = 20;
        }
    }

}
