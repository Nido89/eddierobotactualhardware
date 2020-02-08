//  <copyright file="SingleAxisJointController.cs" company="Microsoft Corporation">
//      Copyright (C) Microsoft Corporation.  All rights reserved.
//  </copyright>
//------------------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//------------------------------------------------------------------------------

namespace Microsoft.Robotics.Services.SingleAxisJoint
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;

    using Microsoft.Ccr.Core;
    using Microsoft.Dss.Core.Attributes;
    using Microsoft.Dss.Core.DsspHttp;
    using Microsoft.Dss.ServiceModel.Dssp;
    using Microsoft.Robotics.PhysicalModel;

    using W3C.Soap;

    /// <summary>
    /// Possible states of the joints.
    /// </summary>
    [FlagsAttribute]
    [DataContract]
    public enum JointStates
    {
        /// <summary>
        /// Joint is busy.
        /// </summary>
        Busy = 1,

        /// <summary>
        /// Joint is actively applying holding torque
        /// </summary>
        HoldingTorqueApplied = 2,

        /// <summary>
        /// Joint is in the process of finding it's index position.
        /// </summary>
        FindingIndex = 4,

        /// <summary>
        /// Last joint find index command was succesful.
        /// </summary>
        FoundIndex = 8
    }

    /// <summary>
    /// Contains commanded elements of a joint.
    /// </summary>
    [DataContract]
    public class JointCommand : ICloneable
    {
        /// <summary>
        /// Gets or sets the target angle of the most recent command.
        /// </summary>
        [DataMember]
        public double TargetAngleInRadians { get; set; }

        /// <summary>
        /// Gets or sets target acceleration of most recent command.
        /// </summary>
        [DataMember]
        public double TargetAccelerationInRadiansPerSecondSecond { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether WaitForCompletion was set in most recently issued command.
        /// </summary>
        [DataMember]
        public bool IsWaitingForCompletion { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the joint has been commanded resist positional disturbances.
        /// </summary>
        [DataMember]
        public bool IsPositionHoldingEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the desired maximum speed of the most recent command.
        /// of the joint.
        /// </summary>
        [DataMember]
        public double MaximumSpeedInRadiansPerSecond { get; set; }

        #region ICloneable Members

        /// <summary>
        /// Create a clone of this JointCommand instance.
        /// </summary>
        /// <returns>A clone of this JointCommand instance.</returns>
        public object Clone()
        {
            var newJointCommand = new JointCommand();
            newJointCommand.TargetAngleInRadians = this.TargetAngleInRadians;
            newJointCommand.TargetAccelerationInRadiansPerSecondSecond = this.TargetAccelerationInRadiansPerSecondSecond;
            newJointCommand.IsWaitingForCompletion = this.IsWaitingForCompletion;
            newJointCommand.IsPositionHoldingEnabled = this.IsPositionHoldingEnabled;
            newJointCommand.MaximumSpeedInRadiansPerSecond = this.MaximumSpeedInRadiansPerSecond;

            return newJointCommand;
        }

        #endregion
    }

    /// <summary>
    /// Contains feedback elements of a joint
    /// </summary>
    [DataContract]
    public class JointFeedback
    {
        /// <summary>
        /// Gets or sets a value indicating whether the joint
        /// is being driven by an active trajectory.
        /// </summary>
        public bool IsTrajectoryActive { get; set; }

        /// <summary>
        /// Gets or sets the number
        /// of the keyframe that the joint is being driven towards.
        /// </summary>
        public int NextTrajectoryKeyFrame { get; set; }

        /// <summary>
        /// Gets or sets current rotation around axis, in radians
        /// </summary>
        [DataMember]
        public double RotationAngleInRadians { get; set; }

        /// <summary>
        /// Gets or sets the current speed of the joint in (radians/second)
        /// </summary>
        [DataMember]
        public double AngularVelocityInRadianPerSecond { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the joint has achieved it's reference position (within the precision of the physical joint)
        /// </summary>
        [DataMember]
        public bool IsConverged { get; set; }

        /// <summary>
        /// Gets or sets flags indicating the state of the joint.
        /// </summary>
        [DataMember]
        public JointStates StateFlags { get; set; }

        /// <summary>
        /// Gets or sets time that the state of the joint was last updated.
        /// </summary>
        [DataMember]
        public double TimeStamp { get; set; }
    }

    /// <summary>
    /// Contains informational elements of a joint.
    /// </summary>
    [DataContract]
    public class JointInformation
    {
        /// <summary>
        /// System-wide index for this joint.
        /// </summary>
        public int JointIndex { get; set; }

        /// <summary>
        /// Gets or sets global coordinate and orientation of where joint attachs to the base. This should be considered constant and
        /// is not affected by joint rotation around its axis.
        /// Axis of rotation is the x-axis
        /// </summary>
        [DataMember]
        public Pose BaseAttachPose { get; set; }

        /// <summary>
        /// Gets or sets maximum allowed rotation angle
        /// </summary>
        [DataMember]
        public double MaximumRotationAngleInRadians { get; set; }

        /// <summary>
        /// Gets or sets minimum allowed rotation angle
        /// </summary>
        [DataMember]
        public double MinimumRotationAngleInRadians { get; set; }
    }

    /// <summary>
    /// SingleAxisJoint state
    /// </summary>
    [DataContract]
    public class SingleAxisJointState
    {
        /// <summary>
        /// Gets or sets joint commands
        /// </summary>
        [DataMember]
        public JointCommand JointCommand { get; set; }

        /// <summary>
        /// Gets or sets joint feedback
        /// </summary>
        [DataMember]
        public JointFeedback JointFeedback { get; set; }

        /// <summary>
        /// Gets or sets joint information
        /// </summary>
        [DataMember]
        public JointInformation JointInformation { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether motion is disabled due to hardware state.
        /// </summary>
        [DataMember]
        public bool IsJointMotionBlocked { get; set; }
    }

    /// <summary>
    /// Represents a key-frame time and position for a single joint.
    /// </summary>
    [DataContract]
    [DataMemberConstructor]
    public class KeyFrame
    {
        /// <summary>
        /// Gets or sets time of the key-frame in seconds.
        /// </summary>
        [DataMember]
        public int TimeInMs { get; set; }

        /// <summary>
        /// Gets or sets pose of the joint at the key-frame.
        /// </summary>
        [DataMember]
        public Pose2D Pose { get; set; }
    }

    /// <summary>
    /// Represents the pose of 2 dimensional (max) actuators as either
    /// a distance and rotation around a single axis or a rotation around
    /// two axes.
    /// </summary>
    [DataContract]
    [DataMemberConstructor]
    public class Pose2D
    {
        /// <summary>
        /// Specifies the distance from the reference pose in millimeters.
        /// </summary>
        [DataMember]
        public double Distance;

        /// <summary>
        /// Specifies orientation relative to reference pose.  For single axis joints, rotation
        /// is around Y.
        /// </summary>
        [DataMember]
        public Vector2 Rotation;
    }

    /// <summary>
    /// Request for initiating a trajectory on a single joint
    /// </summary>
    [DataContract]
    [DataMemberConstructor]
    public class StartSingleJointTrajectoryRequest
    {
        /// <summary>
        /// Gets or sets a value indicating whether the trajectory should be appended
        /// to the current trajectory.
        /// If true, this trajectory will be appended to the trajectory currently
        /// in progress (if any).   If false, this trajectory will immediately interrupt
        /// any trajectory currently in progress for this joint and start executing the
        /// new trajectory.  Has no effect if no trajectory is currently being executed
        /// for this joint.
        /// </summary>
        [DataMember]
        public bool AppendToInProgressTrajectory { get; set; }

        /// <summary>
        /// Gets or sets a list of key-frames that comprise the trajectory.  Times must be monotonically
        /// increasing and positions must be withing limits for the current joint.
        /// Positions for head tilt and pan are absolute values in milliradians, and 
        /// positions for the two drive wheels are realtive values in millimeters.
        /// </summary>
        [DataMember]
        public List<KeyFrame> KeyFrames { get; set; }

        /// <summary>
        /// Gets or sets the time stamp.
        /// </summary>
        [DataMember]
        public double? TimeStamp { get; set; }
    }

    /// <summary>
    /// Contains index and trajectory information for a single joint's trajectory.
    /// Used to form trajectories containing multiple joints.
    /// </summary>
    [DataMemberConstructor]
    [DataContract]
    public class MultiJointTrajectoryElement
    {
        /// <summary>
        /// The system wide index for the joint.
        /// </summary>
        [DataMember]
        public int JointIndex { get; set; }

        /// <summary>
        /// The trajectory for the joint to follow.
        /// </summary>
        [DataMember]
        public StartSingleJointTrajectoryRequest Trajectory { get; set; }
    }

    /// <summary>
    /// Request for starting simultaneous trajectories on multiple joints.
    /// </summary>
    [DataContract]
    [DataMemberConstructor]
    public class StartMultiJointTrajectoryRequest
    {
        /// <summary>
        /// Gets or sets a list of single joint trajectories to start simultaneously.
        /// No two trajectories may be for the same joint.
        /// </summary>
        [DataMember]
        public List<MultiJointTrajectoryElement> TrajectoryList { get; set; }

        /// <summary>
        /// Gets or sets the timestamp.
        /// </summary>
        [DataMember]
        public double? TimeStamp { get; set; }
    }

    /// <summary>
    /// Request type for setting PD gains of a joint.
    /// </summary>
    [DataContract]
    [DataMemberConstructor]
    public class SetPDGainsRequest
    {
        /// <summary>
        /// Gets or sets the proportional gain.
        /// </summary>
        [DataMember]
        public int ProportionalGain { get; set; }

        /// <summary>
        /// Gets or sets the derivative gain.
        /// </summary>
        [DataMember]
        public int DerivativeGain { get; set; }

        /// <summary>
        /// Gets or sets the frequency of the control loop.
        /// </summary>
        [DataMember]
        public int Frequency { get; set; }

        /// <summary>
        /// Gets or sets the time stamp.
        /// </summary>
        [DataMember]
        public double? TimeStamp { get; set; }
    }

    /// <summary>
    /// Request for rotating a single axis.
    /// </summary>
    [DataContract]
    [DataMemberConstructor]
    public class RotateSingleAxisRequest
    {
        /// <summary>
        /// Initializes a new instance of the RotateSingleAxisRequest class.
        /// </summary>
        public RotateSingleAxisRequest()
        {
            this.IsMotionCompletionRequiredForResponse = false;
            this.IsRelative = false;
        }

        /// <summary>
        /// Gets or sets a value indicating whether rotation is relative to current position, otherwise its absolute angle in radians
        /// </summary>
        [DataMember]
        public bool IsRelative { get; set; }

        /// <summary>
        /// Gets or sets the amount to rotate (in radians)
        /// </summary>
        [DataMember]
        public double TargetRotationAngleInRadians { get; set; }
        
        /// <summary>
        /// Gets or sets the desired maximum rotation speed for movement.  This is the target steady state speed
        /// for the duration of the movement command and should always be positive (direction of movement is determined
        /// by the position portion of the command.  If 0, then default speed is used.
        /// </summary>
        [DataMember]
        public double MaxSpeedInRadiansPerSecond { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a response will be posted to the messages response port when the movement has converged on
        /// its target or it has been prempted by another movement command.  If false, a response is posted as soon
        /// as the message is succesfully sent.
        /// </summary>
        [DataMember]
        public bool IsMotionCompletionRequiredForResponse { get; set; }

        /// <summary>
        /// Gets or sets desired acceleration.  0 results in default acceleration.
        /// </summary>
        [DataMember]
        public double TargetAccelerationInRadiansPerSecondSecond { get; set; }

        /// <summary>
        /// Gets or sets time stamp for request.
        /// </summary>
        [DataMember]
        public double? TimeStamp { get; set; }
    }

    /// <summary>
    /// Contains index and trajectory information for a single joint's rotation.
    /// Used to form trajectories containing multiple joints.
    /// </summary>
    [DataMemberConstructor]
    [DataContract]
    public class RotateMultipleAxisElement
    {
        /// <summary>
        /// The system wide index for the joint.
        /// </summary>
        [DataMember]
        public int JointIndex { get; set; }

        /// <summary>
        /// The trajectory for the joint to follow.
        /// </summary>
        [DataMember]
        public RotateSingleAxisRequest Trajectory { get; set; }
    }

    /// <summary>
    /// Request for rotating multiple axes simultaneously.
    /// </summary>
    [DataContract]
    [DataMemberConstructor]
    public class RotateMultipleAxisRequest
    {
        /// <summary>
        /// Initializes a new instance of the RotateMultipleAxisRequest class.
        /// </summary>
        public RotateMultipleAxisRequest()
        {
        }

        /// <summary>
        /// Initializes a new instance of the RotateMultipleAxisRequest class.
        /// </summary>
        /// <param name="rotateRequestList">A list of single axis rotation requests</param>
        public RotateMultipleAxisRequest(List<RotateMultipleAxisElement> rotateRequestList)
        {
            this.RotateRequestList = rotateRequestList;
        }

        /// <summary>
        /// Gets or sets list of RotateSingleAxisRequests
        /// </summary>
        [DataMember]
        public List<RotateMultipleAxisElement> RotateRequestList { get; set; }

        /// <summary>
        /// Gets or sets time stamp for request for entire RotateMultipleAxisRequest.
        /// Time stamps in requests in RotateRequestList are ignored.
        /// </summary>
        [DataMember]
        public double? TimeStamp { get; set; }
    }

    /// <summary>
    /// Message body for <c>UpdateJointMotionBlockRequest</c> request.
    /// </summary>
    [DataContract]
    [DataMemberConstructor]
    public class UpdateJointMotionBlockRequest
    {
        /// <summary>
        /// Initializes a new instance of the UpdateJointMotionBlockRequest class.
        /// </summary>
        public UpdateJointMotionBlockRequest()
        {
        }

        /// <summary>
        /// Initializes a new instance of the UpdateJointMotionBlockRequest class.
        /// </summary>
        /// <param name="isMotionBlocked">Indicates whether a hardware stop is active. </param>
        public UpdateJointMotionBlockRequest(bool isMotionBlocked)
        {
            this.IsMotionBlocked = isMotionBlocked;
        }

        /// <summary>
        /// Gets or sets a value indicating whether a hardware stop is active.
        /// </summary>
        [DataMember]
        public bool IsMotionBlocked { get; set; }
    }

    /// <summary>
    /// Message body for <c>SetHoldingTorque</c> request.
    /// </summary>
    [DataContract]
    [DataMemberConstructor]
    public class SetHoldingTorqueRequest
    {
        /// <summary>
        /// Gets or sets a value indicating whether the holding torque of the joint is enabled.
        /// </summary>
        [DataMember]
        public bool IsEnabled { get; set; }

        /// <summary>
        /// Gets or sets time stamp for request.
        /// </summary>
        [DataMember]
        public double? TimeStamp { get; set; }
    }
}
