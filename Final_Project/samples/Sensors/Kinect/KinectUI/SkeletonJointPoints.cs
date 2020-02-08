//------------------------------------------------------------------------------
//  <copyright file="SkeletonJointPoints.cs" company="Microsoft Corporation">
//      Copyright (C) Microsoft Corporation.  All rights reserved.
//  </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Dss.Services.Samples.KinectUI
{
    using System;
    using System.Collections.Generic;
    using System.Windows;

    using kinect = Microsoft.Kinect;

    /// <summary>
    /// Joint information used in visualizing the skeletons
    /// </summary>
    public class VisualizableJoint
    {
        /// <summary>
        /// Gets or sets the tracking state. Is one of the following - Tracked, Inferred, Not Tracked
        /// </summary>
        public kinect.JointTrackingState TrackingState { get; set; }

        /// <summary>
        /// Visualizable coordinates of the joint
        /// </summary>
        public Point JointCoordiantes = new Point();
    }

    /// <summary>
    /// Joint coordinates that will be used by UI to draw the seletons
    /// </summary>
    public class VisualizableSkeletonInformation
    {
        /// <summary>
        /// Gets or sets the skeleton quality
        /// </summary>
        public string SkeletonQuality { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether or not this skeleton is tracked by Kinect
        /// </summary>
        public bool IsSkeletonActive { get; set; }

        /// <summary>
        /// Pre-allocated joint points
        /// </summary>
        public Dictionary<kinect.JointType, VisualizableJoint> JointPoints = new Dictionary<kinect.JointType, VisualizableJoint>() 
        { 
            { kinect.JointType.AnkleLeft, new VisualizableJoint() },
            { kinect.JointType.AnkleRight, new VisualizableJoint() },
            { kinect.JointType.ElbowLeft, new VisualizableJoint() },
            { kinect.JointType.ElbowRight, new VisualizableJoint() },
            { kinect.JointType.FootLeft, new VisualizableJoint() },
            { kinect.JointType.FootRight, new VisualizableJoint() },
            { kinect.JointType.HandLeft, new VisualizableJoint() },
            { kinect.JointType.HandRight, new VisualizableJoint() },
            { kinect.JointType.Head, new VisualizableJoint() },
            { kinect.JointType.HipCenter, new VisualizableJoint() },
            { kinect.JointType.HipLeft, new VisualizableJoint() },
            { kinect.JointType.HipRight, new VisualizableJoint() },
            { kinect.JointType.KneeLeft, new VisualizableJoint() },
            { kinect.JointType.KneeRight, new VisualizableJoint() },
            { kinect.JointType.ShoulderCenter, new VisualizableJoint() },
            { kinect.JointType.ShoulderLeft, new VisualizableJoint() },
            { kinect.JointType.ShoulderRight, new VisualizableJoint() },
            { kinect.JointType.Spine, new VisualizableJoint() },
            { kinect.JointType.WristLeft, new VisualizableJoint() },
            { kinect.JointType.WristRight, new VisualizableJoint() },
        };
    }
}
