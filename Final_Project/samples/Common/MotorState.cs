//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: MotorState.cs $ $Revision: 19 $
//-----------------------------------------------------------------------

using Microsoft.Dss.Core.Attributes;
using Microsoft.Robotics.PhysicalModel;

using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Microsoft.Robotics.Services.Motor
{
    /// <summary>
    /// State Definition for the Motor Service
    /// </summary>
    [DataContract]
    [DataMemberConstructor]
    public class MotorState
    {
        private string _name;
        private int _hardwareIdentifier;
        private double _currentPower = 0.0;
        private double _powerScalingFactor = 1.0;
        private bool _reversePolarity = false;
        private Pose _pose;

        /// <summary>
        /// Descriptive Identifier for this motor.
        /// </summary>
        [DataMember]
        [Description("Specifies the descriptive identifier for the motor.")]
        public string Name
        {
            get { return this._name; }
            set { this._name = value; }
        }

        /// <summary>
        /// Hardware port identifier, if applicable
        /// </summary>
        [DataMember]
        [Description("Identifies the hardware port of the motor.")]
        public int HardwareIdentifier
        {
            get { return _hardwareIdentifier; }
            set { _hardwareIdentifier = value; }
        }

        /// <summary>
        /// The current motor power. Range is -1.0 to 1.0
        /// </summary>
        [DataMember]
        [DataMemberConstructor(Order=-1)] //exclude from constructor
        [Description("Indicates the current motor power; range is -1.0 to 1.0.")]
        [Browsable(false)]
        public double CurrentPower
        {
            get { return this._currentPower; }
            set { this._currentPower = value; }
        }

        /// <summary>
        /// Power scaling factor, multipled by CurrentPower
        /// </summary>
        [DataMember]
        [Description("Indicates the multiplier applied to CurrentPower to set the power for a motor.")]
        public double PowerScalingFactor
        {
            get { return _powerScalingFactor; }
            set { _powerScalingFactor = value; }
        }

        /// <summary>
        /// Reverses the direction of the motor
        /// </summary>
        [DataMember]
        [Description("Indicates the direction (polarity) of the motor.\n(Setting this to true reverses the motor.)")]
        public bool ReversePolarity
        {
            get { return _reversePolarity; }
            set { _reversePolarity = value; }
        }

        /// <summary>
        /// Position and orientation
        /// </summary>
        [DataMember]
        [Description("The position and orientation of the motor.")]
        public Pose Pose
        {
            get { return _pose; }
            set { _pose = value; }
        }
    }

    /// <summary>
    /// Wheeled Motor Definition
    /// </summary>
    [DataContract]
    [Description("The state of the motor.")]
    public class WheeledMotorState
    {
        private string _name;
        private MotorState _motorState;
        private double _radius;
        private double _gearRatio;
        private Microsoft.Robotics.Services.Encoder.EncoderState _encoderState;
        private double _wheelSpeed;

        /// <summary>
        /// Default constructor
        /// </summary>
        public WheeledMotorState() { }

        /// <summary>
        /// Initialization constructor
        /// </summary>
        /// <param name="name"></param>
        /// <param name="radius"></param>
        public WheeledMotorState(string name, double radius)
        {
            _name = name;
            _radius = radius;
            MotorState = new MotorState();
            EncoderState = new Microsoft.Robotics.Services.Encoder.EncoderState();
        }

        /// <summary>
        /// Speed of the wheel in m/sec.
        /// </summary>
        [DataMember]
        [Description ("Identifies the speed setting of the wheel (in m/sec).")]
        [Browsable(false)]
        public double WheelSpeed
        {
            get { return _wheelSpeed; }
            set { _wheelSpeed = value; }
        }

        /// <summary>
        /// Descriptive Identifier for this wheel.
        /// </summary>
        [DataMember]
        [Description("Specifies the descriptive identifier for this wheel.")]
        public string Name
        {
            get { return this._name; }
            set { this._name = value; }
        }

        /// <summary>
        /// Motor State
        /// </summary>
        [DataMember]
        [Browsable(false)]
        [Description("Identifies the state of the motor.")]
        public MotorState MotorState
        {
            get { return this._motorState; }
            set { this._motorState = value; }
        }

        /// <summary>
        /// The wheel radius, in meters
        /// </summary>
        [DataMember]
        [Description("Specifies the radius of the motor's attached wheel (in meters).")]
        public double Radius
        {
            get { return this._radius; }
            set { this._radius = value; }
        }

        /// <summary>
        /// The gear ratio (motor / wheel)
        /// <remarks>A number less than 1 indicates speed reduction.
        /// Example: 1/5 the wheel rotates 5 times slower than the motor.</remarks>
        /// </summary>
        [DataMember]
        [Description("Specifies the gear ratio (motor/wheel).\nA number less than 1 indicates speed reduction.")]
        public double GearRatio
        {
            get { return this._gearRatio; }
            set { this._gearRatio = value; }
        }

        /// <summary>
        /// Encoder State
        /// </summary>
        [DataMember]
        [Browsable(false)]
        [Description("The state of a motor's encoder.")]
        public Microsoft.Robotics.Services.Encoder.EncoderState EncoderState
        {
            get { return this._encoderState; }
            set { this._encoderState = value; }
        }

    }

    /// <summary>
    /// Update the target power of a motor
    /// </summary>
    [DataContract]
    [DataMemberConstructor]
    public class SetMotorPowerRequest
    {
        private double _targetPower;
        /// <summary>
        /// The target motor power. Range is -1.0 to 1.0
        /// </summary>
        [DataMember]
        [Description("Specifies the target power for the motor. (-1.0 to 1.0)")]
        public double TargetPower
        {
            get { return this._targetPower; }
            set { this._targetPower = value; }
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public SetMotorPowerRequest() { }

        /// <summary>
        /// initialization constructor
        /// </summary>
        /// <param name="targetPower"></param>
        public SetMotorPowerRequest(double targetPower)
        {
            this._targetPower = targetPower;
        }
    }

    /// <summary>
    /// Update the target wheelspeed of a wheel
    /// </summary>
    [DataContract]
    public class SetWheelSpeedRequest
    {
        private double _targetWheelSpeed;

        /// <summary>
        /// Target wheel speed in m/sec.
        /// </summary>
        [DataMember]
        [Description("Specifies the target speed of the wheel.")]
        public double TargetWheelSpeed
        {
            get { return _targetWheelSpeed; }
            set { _targetWheelSpeed = value; }
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public SetWheelSpeedRequest() { }

        /// <summary>
        /// Initialization constructor
        /// </summary>
        /// <param name="targetWheelSpeed">Target wheel speed in meters/sec</param>
        public SetWheelSpeedRequest(double targetWheelSpeed)
        {
        }
    }
}
