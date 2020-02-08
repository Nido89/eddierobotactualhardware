//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: ArticulatedArmState.cs $ $Revision: 10 $
//-----------------------------------------------------------------------

using Microsoft.Dss.Core.Attributes;
using Microsoft.Robotics.PhysicalModel;

using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Microsoft.Robotics.Services.ArticulatedArm
{
    /// <summary>
    /// Articulated Arm State
    /// </summary>
    [DataContract]
    [Description("The state of the articulated arm.")]
    public class ArticulatedArmState
    {
        List<Joint> _joints;

        /// <summary>
        /// Joints
        /// </summary>
        [DataMember]
        [Description("The set of joints.")]
        public List<Joint> Joints
        {
            get { return _joints; }
            set { _joints = value; }
        }

        Pose _endEffectorPose;

        /// <summary>
        /// End Effector Pose
        /// </summary>
        [DataMember]
        [Description("Identifies the position and orientation for the end effector.")]
        public Pose EndEffectorPose
        {
          get { return _endEffectorPose; }
          set { _endEffectorPose = value; }
        }
    }
}