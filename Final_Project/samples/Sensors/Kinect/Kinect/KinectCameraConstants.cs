//------------------------------------------------------------------------------
//  <copyright file="KinectCameraConstants.cs" company="Microsoft Corporation">
//      Copyright (C) Microsoft Corporation.  All rights reserved.
//  </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Robotics.Services.Sensors.Kinect
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using common = Microsoft.Robotics.Common;
    using pm = Microsoft.Robotics.PhysicalModel;

    /// <summary>
    /// Static class containing kinect camera constants
    /// </summary>
    public static class KinectCameraConstants
    {
        /// <summary>
        /// Depth Range In Meters
        /// </summary>
        private const double DepthRangeInMeters = 4.0; // Kinect device advertized range limit

        /// <summary>
        /// Gets the horizontal fov of the depth camera in radians
        /// </summary>
        /// <returns>Horizontal fov in radians</returns>
        public const double HorizontalFieldOfViewRadians = 58.51f * Math.PI / 180.0f;

        /// <summary>
        /// Percentage of image width that is not usable, at the far right of the image
        /// </summary>
        public const double PercentImageColumnsAtRightEdgeNotActive = 0.015;

        /// <summary>
        /// Default Inverse Projection Matrix
        /// </summary>
        private static pm.Matrix defaultInverseProjectionMatrix =
            common.MathUtilities.Invert(common.MathUtilities.ComputeProjectionMatrix(
                (float)HorizontalFieldOfViewRadians, 320, 240, DepthRangeInMeters));        

        /// <summary>
        /// Gets the default inverse projection matrix
        /// </summary>
        /// <returns>Default inverse projection matrix</returns>
        public static pm.Matrix InverseProjectionMatrix
        {
            get
            {
                return defaultInverseProjectionMatrix;
            }
        }

        /// <summary>
        /// Gets the maximum range of the depth camera
        /// </summary>
        /// <returns>Depthcam range in meters</returns>
        public static double MaximumRangeMeters
        {
            get
            {
                return DepthRangeInMeters;
            }
        }
    }
}
