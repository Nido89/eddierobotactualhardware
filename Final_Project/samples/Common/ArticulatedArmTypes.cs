//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: ArticulatedArmTypes.cs $ $Revision: 25 $
//-----------------------------------------------------------------------

using Microsoft.Ccr.Core;
using Microsoft.Dss.Core.Attributes;
using Microsoft.Dss.Core.DsspHttp;
using Microsoft.Dss.ServiceModel.Dssp;
using Microsoft.Robotics.PhysicalModel;

using System;
using System.Collections.Generic;
using System.ComponentModel;

using W3C.Soap;

namespace Microsoft.Robotics.Services.ArticulatedArm
{
    /// <summary>
    /// The Dss Contract Definition
    /// </summary>
    [DisplayName("(User) Generic Articulated Arm")]
    [Description("Provides access to an articulated arm.")]
    [DssServiceDescription("http://msdn.microsoft.com/library/dd145246.aspx")]
    public static class Contract
    {
        /// <summary>
        /// Contract Identifier
        /// </summary>
        public const string Identifier = "http://schemas.microsoft.com/2006/06/articulatedarm.html";
    }

    /// <summary>
    /// ArticulatedArm Port
    /// </summary>
    [ServicePort]
    public class ArticulatedArmOperations : PortSet
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public ArticulatedArmOperations() : base(
            typeof(DsspDefaultLookup),
            typeof(DsspDefaultDrop),
            typeof(Get),
            typeof(HttpGet),
            typeof(Replace),
            typeof(SetJointTargetPose),
            typeof(SetJointTargetVelocity),
            typeof(SetEndEffectorPose),
            typeof(GetEndEffectorPose),
            typeof(ReliableSubscribe),
            typeof(Subscribe))
        {
        }

        /// <summary>
        /// Implicit conversion
        /// </summary>
        public static implicit operator Port<DsspDefaultLookup>(ArticulatedArmOperations portSet)
        {
            if (portSet == null) return null;
            return (Port<DsspDefaultLookup>)portSet[typeof(DsspDefaultLookup)];
        }
        /// <summary>
        /// Implicit conversion
        /// </summary>
        public static implicit operator Port<DsspDefaultDrop>(ArticulatedArmOperations portSet)
        {
            if (portSet == null) return null;
            return (Port<DsspDefaultDrop>)portSet[typeof(DsspDefaultDrop)];
        }
        /// <summary>
        /// Implicit conversion
        /// </summary>
        public static implicit operator Port<Get>(ArticulatedArmOperations portSet)
        {
            if (portSet == null) return null;
            return (Port<Get>)portSet[typeof(Get)];
        }
        /// <summary>
        /// Implicit conversion
        /// </summary>
        public static implicit operator Port<Replace>(ArticulatedArmOperations portSet)
        {
            if (portSet == null) return null;
            return (Port<Replace>)portSet[typeof(Replace)];
        }
        /// <summary>
        /// Implicit conversion
        /// </summary>
        public static implicit operator Port<SetJointTargetPose>(ArticulatedArmOperations portSet)
        {
            if (portSet == null) return null;
            return (Port<SetJointTargetPose>)portSet[typeof(SetJointTargetPose)];
        }
        /// <summary>
        /// Implicit conversion
        /// </summary>
        public static implicit operator Port<SetJointTargetVelocity>(ArticulatedArmOperations portSet)
        {
            if (portSet == null) return null;
            return (Port<SetJointTargetVelocity>)portSet[typeof(SetJointTargetVelocity)];
        }
        /// <summary>
        /// Implicit conversion
        /// </summary>
        public static implicit operator Port<SetEndEffectorPose>(ArticulatedArmOperations portSet)
        {
            if (portSet == null) return null;
            return (Port<SetEndEffectorPose>)portSet[typeof(SetEndEffectorPose)];
        }
        /// <summary>
        /// Implicit conversion
        /// </summary>
        public static implicit operator Port<GetEndEffectorPose>(ArticulatedArmOperations portSet)
        {
            if (portSet == null) return null;
            return (Port<GetEndEffectorPose>)portSet[typeof(GetEndEffectorPose)];
        }
        /// <summary>
        /// Implicit conversion
        /// </summary>
        public static implicit operator Port<Subscribe>(ArticulatedArmOperations portSet)
        {
            if (portSet == null) return null;
            return (Port<Subscribe>)portSet[typeof(Subscribe)];
        }
        /// <summary>
        /// Implicit conversion
        /// </summary>
        public static implicit operator Port<ReliableSubscribe>(ArticulatedArmOperations portSet)
        {
            if (portSet == null) return null;
            return (Port<ReliableSubscribe>)portSet[typeof(ReliableSubscribe)];
        }

    }

    /// <summary>
    /// Operation Get: Gets the state
    /// </summary>
    [Description("Gets an arm's current state.")]
    public class Get : Get<GetRequestType, PortSet<ArticulatedArmState, Fault>>
    {
    }

    /// <summary>
    /// Operation Replace: Configures arm
    /// </summary>
    [Description("Changes (or indicates a change to) an arm's entire state.")]
    [DisplayName("(User) ArticulatedArmReplace")]
    public class Replace : Replace<ArticulatedArmState, PortSet<DefaultReplaceResponseType, Fault>>
    {
    }

    /// <summary>
    /// Set Joint Target Pose Request
    /// </summary>
    [DataMemberConstructor]
    [DataContract]
    public class SetJointTargetPoseRequest
    {
        string _jointName;

        /// <summary>
        /// Joint Name
        /// </summary>
        [DataMember]
        [Description("Specifies the descriptive identifier for a joint.")]
        public string JointName
        {
            get { return _jointName; }
            set { _jointName = value; }
        }

        Vector3 _targetPosition;

        /// <summary>
        /// Target Position
        /// </summary>
        [DataMember]
        [Description("Specifies the ending position for a joint.")]
        public Vector3 TargetPosition
        {
            get { return _targetPosition; }
            set { _targetPosition = value; }
        }

        AxisAngle _targetOrientation;

        /// <summary>
        /// Target Orientation
        /// </summary>
        [DataMember]
        [Description("Specifies the ending orientation for a joint.")]
        public AxisAngle TargetOrientation
        {
            get { return _targetOrientation; }
            set { _targetOrientation = value; }
        }
    }

    /// <summary>
    /// Set Joint Target Velocity Request
    /// </summary>
    [DataMemberConstructor]
    [DataContract]
    public class SetJointTargetVelocityRequest
    {
        string _jointName;

        /// <summary>
        /// Joint Name
        /// </summary>
        [DataMember]
        [Description("Specifies the descriptive identifier for the joint.")]
        public string JointName
        {
            get { return _jointName; }
            set { _jointName = value; }
        }

        Vector3 _targetVelocity;

        /// <summary>
        /// Target Velocity
        /// </summary>
        [DataMember]
        [Description("Specifies the ending velocity for a joint.")]
        public Vector3 TargetVelocity
        {
            get { return _targetVelocity; }
            set { _targetVelocity = value; }
        }
    }

    /// <summary>
    /// Operation SetJointTargetPosition: Set target position and/or orientation for joint
    /// </summary>
    [Description("Sets (or indicates a change to) a joint's target position and/or orientation.")]
    public class SetJointTargetPose : Update<SetJointTargetPoseRequest, PortSet<DefaultUpdateResponseType, Fault>>
    {
    }

    /// <summary>
    /// Operation SetJointTargetVelocity: Sets target linear or angular velocity depending on joint type
    /// </summary>
    [Description("Sets (or indicates a change to) a joint's target linear or angular velocity.")]
    public class SetJointTargetVelocity : Update<SetJointTargetVelocityRequest, PortSet<DefaultUpdateResponseType, Fault>>
    {
    }

    /// <summary>
    /// Queries end effector Pose
    /// </summary>
    [Description("Gets an end effector's current position.")]
    public class GetEndEffectorPose : Query<GetEndEffectorPoseRequest, PortSet<GetEndEffectorPoseResponse, Fault>>
    {
    }

    /// <summary>
    /// Sets the end effector Pose
    /// </summary>
    [Description("Sets (or indicates a change to) an end effector's position and/or orientation.")]
    public class SetEndEffectorPose : Update<SetEndEffectorPoseRequest, PortSet<DefaultUpdateResponseType, Fault>>
    {
    }

    /// <summary>
    /// Requests end effector Pose
    /// </summary>
    [DataContract]
    [Description("Gets the position and orientation of the end effector.")]
    public class GetEndEffectorPoseRequest
    {
    }

    /// <summary>
    /// End Effector Pose
    /// </summary>
    [DataContract]
    public class GetEndEffectorPoseResponse
    {
        private Pose _endEffectorPose;

        /// <summary>
        /// Pose
        /// </summary>
        [DataMember]
        [Description("Specifies the position and orientation of the end effector.")]
        public Pose EndEffectorPose
        {
            get { return _endEffectorPose; }
            set { _endEffectorPose = value; }
        }
    }

    /// <summary>
    /// Sets the end effector joint state
    /// </summary>
    [DataContract]
    public class SetEndEffectorPoseRequest
    {
        private Pose _endEffectorPose;

        /// <summary>
        /// Pose
        /// </summary>
        [DataMember]
        [Description("Specifies the position and orientation of the end effector.")]
        public Pose EndEffectorPose
        {
            get { return _endEffectorPose; }
            set { _endEffectorPose = value; }
        }
    }

    /// <summary>
    /// Operation Subscribe to bumper
    /// </summary>
    public class Subscribe : Subscribe<SubscribeRequestType, PortSet<SubscribeResponseType, Fault>>
    {
    }

    /// <summary>
    /// Operation Subscribe to bumper
    /// </summary>
    public class ReliableSubscribe : Subscribe<ReliableSubscribeRequestType, PortSet<SubscribeResponseType, Fault>>
    {
    }
}
