//------------------------------------------------------------------------------
//  <copyright file="utilities.cs" company="Microsoft Corporation">
//      Copyright (C) Microsoft Corporation.  All rights reserved.
//  </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Robotics.Common
{
    using System;
    using System.Diagnostics;
    using Microsoft.Robotics.PhysicalModel;
    using pm = Microsoft.Robotics.PhysicalModel;
    using resources = Microsoft.Robotics.Properties.Resources; 

    /// <summary>
    /// Math utilities
    /// </summary>
    public static class MathUtilities
    {
        /// <summary>
        /// Converts a quaternion to an Euler rotation vector in Radians
        /// </summary>
        /// <param name="quat">The quaternion to convert to euler angles</param>
        /// <returns>Vector with x=roll, y=heading, z = pitch</returns>
        public static Vector3 QuaternionToEulerRadians(pm.Quaternion quat)
        {
            double qx = quat.X;
            double qy = quat.Y;
            double qz = quat.Z;
            double qw = quat.W;
            double heading = Math.Atan2((2 * qy * qw) - (2 * qx * qz), 1 - (2 * qy * qy) - (2 * qz * qz));
            double sin = (2 * qx * qy) + (2 * qz * qw);

            // precision problems can rarely move this value slightly out of the valid range -1 <= sin <= 1
            // clamp it
            if (sin < -1)
            {
                sin = -1;
            }
            else if (sin > 1)
            {
                sin = 1;
            }

            double pitch = Math.Asin(sin);
            double roll = Math.Atan2((2 * qx * qw) - (2 * qy * qz), 1 - (2 * qx * qx) - (2 * qz * qz));

            return new Vector3((float)roll, (float)heading, (float)pitch);
        }

        /// <summary>
        /// Converts an Euler rotation vector in Radians to a quaternion
        /// </summary>
        /// <param name="value">Vector with x=roll, y=heading, z = pitch</param>
        /// <returns>A quaternion representing the euler angles in value</returns>
        public static pm.Quaternion EulerRadiansToQuaternion(Vector3 value)
        {
            // prevent conversion errors (gimbal lock) by disallowing 0 degree values
            if (value.X == 0)
            {
                value.X = 0.00001f;
            }

            if (value.Y == 0)
            {
                value.Y = 0.00001f;
            }

            if (value.Z == 0)
            {
                value.Z = 0.00001f;
            }

            double heading = value.Y;
            double pitch = value.Z;
            double roll = value.X;
            double c1 = Math.Cos(heading / 2);
            double c2 = Math.Cos(pitch / 2);
            double c3 = Math.Cos(roll / 2);
            double s1 = Math.Sin(heading / 2);
            double s2 = Math.Sin(pitch / 2);
            double s3 = Math.Sin(roll / 2);
            Microsoft.Robotics.PhysicalModel.Quaternion quat = new Microsoft.Robotics.PhysicalModel.Quaternion();
            quat.W = (float)((c1 * c2 * c3) - (s1 * s2 * s3));
            quat.X = (float)((s1 * s2 * c3) + (c1 * c2 * s3));
            quat.Y = (float)((s1 * c2 * c3) + (c1 * s2 * s3));
            quat.Z = (float)((c1 * s2 * c3) - (s1 * c2 * s3));

            return quat;
        }

        /// <summary>
        /// Return the angle difference between the two headings using the first argument as the basis
        /// (heading delta is relative to the first heading. Negative delta is counter clockwise from first heading
        /// </summary>
        /// <param name="firstHeading">The first angle in radians</param>
        /// <param name="secondHeading">The second angle in radians</param>
        /// <returns>The difference between the angles in degrees</returns>
        public static double CalculateDifferenceInHeadings(double firstHeading, double secondHeading)
        {
            const double TwoPi = Math.PI * 2.0;
            double delta = 0;
            firstHeading = firstHeading % TwoPi;
            secondHeading = secondHeading % TwoPi;
            if (firstHeading < 0)
            {
                firstHeading += TwoPi;
            }

            if (secondHeading < 0)
            {
                secondHeading += TwoPi;
            }

            if (firstHeading > secondHeading)
            {
                if (firstHeading - secondHeading > Math.PI)
                {
                    delta = -(firstHeading - TwoPi) + secondHeading;
                }
                else
                {
                    delta = secondHeading - firstHeading;
                }
            }
            else
            {
                if (secondHeading - firstHeading > Math.PI)
                {
                    delta = (secondHeading - TwoPi) - firstHeading;
                }
                else
                {
                    delta = secondHeading - firstHeading;
                }
            }

            return delta;
        }

        /// <summary>
        /// Calcualte moments 
        /// </summary>
        /// <param name="normalizeRange">True to normalize moments, false otherwise</param>
        /// <param name="projection">Array to compute moments from</param>
        /// <param name="mean">Computed mean</param>
        /// <param name="count">Number of values used to compute the mean</param>
        /// <param name="moments">Returns the computed moments</param>
        public static void CalculateMoments(
            bool normalizeRange,
            int[] projection,
            double mean,
            int count,
            out double[] moments)
        {
            moments = new double[4];

            double square = 0.0;
            double offset = -mean;
            double standardDev = 0.0;
            double skew = 0.0;
            double kurtosis = 0.0;

            for (int x = 0; x < projection.Length; x++, offset++)
            {
                if (projection[x] > 0)
                {
                    square = offset * offset * projection[x];
                    standardDev += square;
                    skew += offset * square;
                    kurtosis += offset * skew;
                }
            }

            standardDev = Math.Sqrt(standardDev / count);
            if (standardDev != 0)
            {
                skew = skew / (count * standardDev * standardDev * standardDev);
                kurtosis = kurtosis / (count * standardDev * standardDev * standardDev * standardDev);
            }

            double factor = normalizeRange == true ? projection.Length : 1;

            moments[0] = mean / factor;
            moments[1] = standardDev / factor;
            moments[2] = skew / factor;
            moments[3] = kurtosis / factor;
        }

        /// <summary>
        /// Transforms a vector using the supplied pose
        /// </summary>
        /// <param name="pose">Pose to apply</param>
        /// <param name="position">Image feature position</param>
        /// <returns>Vector after transformation is applied</returns>
        public static Vector3 TransformVector(PhysicalModel.Pose? pose, PhysicalModel.Vector3 position)
        {
            if (pose.HasValue)
            {
                var rotatedV = PhysicalModel.Quaternion.Rotate(
                    pose.Value.Orientation,
                    position);
                position = new PhysicalModel.Vector3(
                    rotatedV.X + pose.Value.Position.X,
                    rotatedV.Y + pose.Value.Position.Y,
                    rotatedV.Z + pose.Value.Position.Z);
            }

            return position;
        }

        /// <summary>
        /// Converts degress to a two element cartesian unit vector (origin to point on a unit circle)
        /// </summary>
        /// <param name="compassReading">Compass reading to convert to a byte vector</param>
        /// <returns>The byte vector representation of the compass</returns>
        public static byte[] DegreesToCartesianUnitVector(double compassReading)
        {
            var vector = new byte[2];
            double y = Math.Sin((compassReading / 180.0) * Math.PI);
            double x = Math.Cos((compassReading / 180.0) * Math.PI);

            // convert to byte, mapping [-1,1] to [0,255], with 0 being 128 in the byte range
            // first convert [-1,1] to [0,1]
            y = (y + 1) / 2;
            x = (x + 1) / 2;
            vector[0] = (byte)(x * byte.MaxValue);
            vector[1] = (byte)(y * byte.MaxValue);

            return vector;
        }

        /// <summary>
        /// Converts two element cartesian unit vector (origin to point on a unit circle) to a heading in degrees
        /// </summary>
        /// <param name="vector">Byte vector representing cartesian 2D vector in [1,1]</param>
        /// <returns>Compass reading in degrees</returns>
        public static double CartesianUnitVectorToHeadingDegrees(byte[] vector)
        {
            var x = (double)vector[0] / (double)byte.MaxValue;
            var y = (double)vector[1] / (double)byte.MaxValue;

            x *= 2;
            y *= 2;

            x -= 1;
            y -= 1;

            x = Math.Atan2(y, x);

            return (x * 180) / Math.PI;
        }

        /// <summary>
        /// Converts a coordinate in screen space to a vector in view space
        /// </summary>
        /// <param name="x">X-coordinate with range [0, screenWidth-1]</param>
        /// <param name="y">Y-coordinate with range [0, screenHeight-1]</param>
        /// <param name="z">Z-coordinate with range [0, maxValidDepth] in mm</param>
        /// <param name="invProjectionMatrix">Inverse projection matrix of the camera</param>
        /// <param name="screenWidth">Width of the camera view plane in pixel space</param>
        /// <param name="screenHeight">Height of the camera view plane in pixel space</param>
        /// <returns>Viewspace vector</returns>
        public static Vector3 ConvertPixelSpaceToViewSpace(
            int x, 
            int y, 
            int z,
            Microsoft.Robotics.PhysicalModel.Matrix invProjectionMatrix, 
            int screenWidth, 
            int screenHeight)
        {
            float matrixAx, matrixBx, matrixAy, matrixBy;
            ComputeTranslationScaleTransform(screenWidth, screenHeight, out matrixAx, out matrixBx, out matrixAy, out matrixBy);

            return ConvertPixelSpaceToViewSpace(x, y, z, invProjectionMatrix, screenWidth, matrixAx, matrixBx, matrixAy, matrixBy);
        }

        /// <summary>
        /// Converts a coordinate in screen space to a vector in view space in millimeters
        /// Allows for working in the native resolution of the depth camera with less conversions
        /// </summary>
        /// <param name="x">X-coordinate with range [0, screenWidth-1]</param>
        /// <param name="y">Y-coordinate with range [0, screenHeight-1]</param>
        /// <param name="z">Z-coordinate with range [0, maxValidDepth] in mm</param>
        /// <param name="invProjectionMatrix">Inverse projection matrix of the camera</param>
        /// <param name="screenWidth">Width of the camera view plane in pixel space</param>
        /// <param name="screenHeight">Height of the camera view plane in pixel space</param>
        /// <returns>Viewspace vector with depth in millimeters</returns>
        public static Vector3 ConvertPixelSpaceToViewSpaceInMillimeters(
            int x, 
            int y, 
            int z,
            Microsoft.Robotics.PhysicalModel.Matrix invProjectionMatrix, 
            int screenWidth, 
            int screenHeight)
        {
            var positionInMeters = ConvertPixelSpaceToViewSpace(x, y, z, invProjectionMatrix, screenWidth, screenHeight);

            // multiply by 1000 to convert to millimeters
            return Vector3.Scale(positionInMeters, 1000.0f);
        }

        /// <summary>
        /// Converts a vector in view space to a vector in screen space
        /// </summary>
        /// <param name="viewSpaceVector">View space vector</param>
        /// <param name="projectionMatrix">Projection matrix</param>
        /// <param name="screenWidth">Screen width</param>
        /// <param name="screenHeight">Screen height</param>
        /// <returns>Pixel space vector</returns>
        public static Vector3 ConvertViewSpaceToPixelSpaceWithDepthInMillimeters(
            Vector3 viewSpaceVector,
            Microsoft.Robotics.PhysicalModel.Matrix projectionMatrix,
            int screenWidth, 
            int screenHeight)
        {
            var screenSpacePoint = ConvertViewSpaceToPixelSpace(ref viewSpaceVector, ref projectionMatrix, screenWidth, screenHeight);

            // we return 'z' in millimeters as that is the unit z is returned from the camera
            return new Vector3(
                screenSpacePoint.X, 
                screenSpacePoint.Y,
                viewSpaceVector.Z * 1000.0f);
        }

        /// <summary>
        /// Converts a vector in view space to a vector in screen space
        /// </summary>
        /// <param name="viewSpaceVector">View space vector</param>
        /// <param name="projectionMatrix">Projection matrix</param>
        /// <param name="screenWidth">Screen width</param>
        /// <param name="screenHeight">Screen height</param>
        /// <returns>Pixel space vector</returns>
        private static Vector3 ConvertViewSpaceToPixelSpace(
            ref Vector3 viewSpaceVector,
            ref Microsoft.Robotics.PhysicalModel.Matrix projectionMatrix,
            int screenWidth,
            int screenHeight)
        {
            Vector4 ptW = TransformVector(
                new Vector4(viewSpaceVector.X, viewSpaceVector.Y, viewSpaceVector.Z, 1.0f),
                projectionMatrix);

            // project to screen space
            var screenSpacePoint = new Vector3(ptW.X / ptW.W, ptW.Y / ptW.W, ptW.Z / ptW.W);

            float w = (float)screenWidth, h = (float)screenHeight;
            float matrixAx = w * 0.5f, matrixBx = (w * 0.5f) - 0.5f;
            float matrixAy = -h * 0.5f, matrixBy = (h * 0.5f) - 0.5f;

            screenSpacePoint.X = (matrixAx * screenSpacePoint.X) + matrixBx + 0.5f;
            screenSpacePoint.Y = (matrixAy * screenSpacePoint.Y) + matrixBy + 0.5f;
            return screenSpacePoint;
        }

        /// <summary>
        /// Compute a ST matrix that maps [(0,0), (W,H)] => [(-1,-1), (1,1)]
        /// </summary>
        /// <param name="width">The width of the image</param>
        /// <param name="height">The height of the image</param>
        /// <param name="matrixAx">Element at first row, first column</param>
        /// <param name="matrixBx">Element at first row, second column</param>
        /// <param name="matrixAy">Element at second row, first column</param>
        /// <param name="matrixBy">Element at second row, second column</param>
        public static void ComputeTranslationScaleTransform(int width, int height, out float matrixAx, out float matrixBx, out float matrixAy, out float matrixBy)
        {
            matrixAx = 2.0f / width;
            matrixBx = (0.5f * matrixAx) - 1;

            matrixAy = -2.0f / height;
            matrixBy = (0.5f * matrixAy) + 1;
        }

        /// <summary>
        /// Transform a vector from screen to view space using provided ST matrix to map to NDC space
        /// </summary>
        /// <param name="x">X coordinate in pixel space</param>
        /// <param name="y">Y coordinate in pixel space</param>
        /// <param name="depthValueInMillimeters">Depth value in pixel space</param>
        /// <param name="invProjectionMatrix">Inverse of the camera's projection matrix</param>
        /// <param name="width">Width of the image in pixels</param>
        /// <param name="matrixAx">Element at first row, first column</param>
        /// <param name="matrixBx">Element at first row, second column</param>
        /// <param name="matrixAy">Element at second row, first column</param>
        /// <param name="matrixBy">Element at second row, second column</param>
        /// <returns>Position in 3d space with respect to the camera</returns>
        public static Vector3 ConvertPixelSpaceToViewSpace(
            int x, 
            int y, 
            int depthValueInMillimeters,
            Matrix invProjectionMatrix, 
            int width, 
            float matrixAx, 
            float matrixBx, 
            float matrixAy, 
            float matrixBy)
        {
            var view3 = ConvertPixelSpaceToViewSpace(
                x, 
                y, 
                invProjectionMatrix, 
                width, 
                matrixAx,
                matrixBx, 
                matrixAy, 
                matrixBy);

            // map from millimeters to meters
            float depthInMeters = depthValueInMillimeters * (1 / 1000.0f);

            // normalize such that view3.Z=1
            // we need to negate view3.Z since XNA (unlike DirectX) uses a right handed coordinate system with -Z into the screen
            view3.Z = -view3.Z;

            view3.X = (view3.X * depthInMeters) / view3.Z;
            view3.Y = (view3.Y * depthInMeters) / view3.Z;
            view3.Z = depthInMeters;
            return view3;
        }

        /// <summary>
        /// Transform a vector from screen to view space using provided ST matrix to map to NDC space
        /// </summary>
        /// <param name="x">X coordinate in pixel space</param>
        /// <param name="y">Y coordinate in pixel space</param>
        /// <param name="invProjectionMatrix">Inverse of the camera's projection matrix</param>
        /// <param name="width">Width of the image in pixels</param>
        /// <param name="matrixAx">Element at first row, first column</param>
        /// <param name="matrixBx">Element at first row, second column</param>
        /// <param name="matrixAy">Element at second row, first column</param>
        /// <param name="matrixBy">Element at second row, second column</param>
        /// <returns>Position in 3d space with respect to the camera</returns>
        internal static Vector3 ConvertPixelSpaceToViewSpace(
            int x, 
            int y, 
            Matrix invProjectionMatrix,
            int width, 
            float matrixAx, 
            float matrixBx, 
            float matrixAy, 
            float matrixBy)
        {
            float nx = (matrixAx * (x - 0.5f)) + matrixBx;
            float ny = (matrixAy * (y - 0.5f)) + matrixBy;

            // map pixel back to NDC space
            Vector4 pndc = new Vector4(nx, ny, 0, 1);

            // map from NDC to view 
            Vector4 pview = TransformVector(pndc, invProjectionMatrix);

            float invW = 1.0f / pview.W;
            return new Vector3(pview.X * invW, pview.Y * invW, pview.Z * invW);
        }

        /// <summary>
        /// Transforms vector
        /// </summary>
        /// <param name="vector">Vector to transform</param>
        /// <param name="matrix">Matrix with affine transform</param>
        /// <returns>Transformed vector</returns>
        internal static Vector4 TransformVector(Vector4 vector, Matrix matrix)
        {
            Vector4 t = new Vector4();
            t.X = (vector.X * matrix.M11) + (vector.Y * matrix.M21) + (vector.Z * matrix.M31) + (vector.W * matrix.M41);
            t.Y = (vector.X * matrix.M12) + (vector.Y * matrix.M22) + (vector.Z * matrix.M32) + (vector.W * matrix.M42);
            t.Z = (vector.X * matrix.M13) + (vector.Y * matrix.M23) + (vector.Z * matrix.M33) + (vector.W * matrix.M43);
            t.W = (vector.X * matrix.M14) + (vector.Y * matrix.M24) + (vector.Z * matrix.M34) + (vector.W * matrix.M44);
            return t;
        }

        /// <summary>
        /// Computes projection matrix
        /// </summary>
        /// <param name="horizontalFovInRadians">Camera horizonal field of view</param>
        /// <param name="widthInPixels">Camera image width</param>
        /// <param name="heightInPixels">Camera image height</param>
        /// <param name="maximumRange">Depth maximum range</param>
        /// <returns>Projection matrix</returns>
        public static pm.Matrix ComputeProjectionMatrix(float horizontalFovInRadians, int widthInPixels, int heightInPixels, double maximumRange)
        {
            // all units are meters
            const float ToMeters = 1.0f / 1000.0f;

            // for reference: pixelSizeInViewSpace = 0.102766666f * toMeters; 

            float nearPlaneDistance = 120.0f * ToMeters;
            float farPlaneDistance = (float)maximumRange;

            float aspectRatio = (float)widthInPixels / (float)heightInPixels;

            var projMatrix = CreatePerspectiveFieldOfView(horizontalFovInRadians, aspectRatio, nearPlaneDistance, farPlaneDistance);

            return projMatrix;
        }

        #region project matrix internal methods
        /// <summary>
        /// Compute projection matrix given a field of view
        /// </summary>
        /// <param name="fieldOfView">Camera horizontal FOV</param>
        /// <param name="aspectRatioWidthToHeight">Image aspect ratio</param>
        /// <param name="nearPlaneDistance">Near plane distance</param>
        /// <param name="farPlaneDistance">Far plane distance</param>
        /// <returns>Perspective matrix</returns>
        private static pm.Matrix CreatePerspectiveFieldOfView(float fieldOfView, float aspectRatioWidthToHeight, float nearPlaneDistance, float farPlaneDistance)
        {
            pm.Matrix matrix;
            if ((fieldOfView <= 0f) || (fieldOfView >= 3.141593f))
            {
                throw new ArgumentException(resources.FieldOfViewOutOfRange);
            }

            if (nearPlaneDistance <= 0f)
            {
                throw new ArgumentException(resources.NearPlaneDistanceLessThanZero);
            }

            if (farPlaneDistance <= 0f)
            {
                throw new ArgumentException(resources.FarPlaneDistanceLessThanZero);
            }

            if (nearPlaneDistance >= farPlaneDistance)
            {
                throw new ArgumentException(resources.NearPlaneGreaterThanOrEqualToFarPlaneDistance);
            }

            float focalLength = 1f / ((float)Math.Tan((double)(fieldOfView * 0.5f)));

            matrix.M11 = focalLength;
            matrix.M12 = matrix.M13 = matrix.M14 = 0f;
            matrix.M22 = focalLength * aspectRatioWidthToHeight;
            matrix.M21 = matrix.M23 = matrix.M24 = 0f;
            matrix.M31 = matrix.M32 = 0f;
            matrix.M33 = -(farPlaneDistance + nearPlaneDistance) / (farPlaneDistance - nearPlaneDistance);
            matrix.M34 = -1f;
            matrix.M41 = matrix.M42 = matrix.M44 = 0f;
            matrix.M43 = -(2f * farPlaneDistance * nearPlaneDistance) / (farPlaneDistance - nearPlaneDistance); 
            return matrix;
        }

        /// <summary>
        /// Matrix inversion from reflector since I don't feel like rewriting myself at the moment
        /// </summary>
        /// <param name="matrix">Matrix to invert</param>
        /// <returns>Inverted matrix</returns>
        public static pm.Matrix Invert(pm.Matrix matrix)
        {
            return InvertInternal(ref matrix);
        }

        /// <summary>
        /// Inverts matrix
        /// </summary>
        /// <param name="matrix">Matrix to invert</param>
        /// <returns>Inverted matrix</returns>
        private static Microsoft.Robotics.PhysicalModel.Matrix InvertInternal(ref pm.Matrix matrix)
        {
            pm.Matrix matrix2;
            float num5 = matrix.M11;
            float num4 = matrix.M12;
            float num3 = matrix.M13;
            float num2 = matrix.M14;
            float num9 = matrix.M21;
            float num8 = matrix.M22;
            float num7 = matrix.M23;
            float num6 = matrix.M24;
            float num17 = matrix.M31;
            float num16 = matrix.M32;
            float num15 = matrix.M33;
            float num14 = matrix.M34;
            float num13 = matrix.M41;
            float num12 = matrix.M42;
            float num11 = matrix.M43;
            float num10 = matrix.M44;
            float num23 = (num15 * num10) - (num14 * num11);
            float num22 = (num16 * num10) - (num14 * num12);
            float num21 = (num16 * num11) - (num15 * num12);
            float num20 = (num17 * num10) - (num14 * num13);
            float num19 = (num17 * num11) - (num15 * num13);
            float num18 = (num17 * num12) - (num16 * num13);
            float num39 = ((num8 * num23) - (num7 * num22)) + (num6 * num21);
            float num38 = -(((num9 * num23) - (num7 * num20)) + (num6 * num19));
            float num37 = ((num9 * num22) - (num8 * num20)) + (num6 * num18);
            float num36 = -(((num9 * num21) - (num8 * num19)) + (num7 * num18));
            float num = 1f / ((((num5 * num39) + (num4 * num38)) + (num3 * num37)) + (num2 * num36));
            matrix2.M11 = num39 * num;
            matrix2.M21 = num38 * num;
            matrix2.M31 = num37 * num;
            matrix2.M41 = num36 * num;
            matrix2.M12 = -(((num4 * num23) - (num3 * num22)) + (num2 * num21)) * num;
            matrix2.M22 = (((num5 * num23) - (num3 * num20)) + (num2 * num19)) * num;
            matrix2.M32 = -(((num5 * num22) - (num4 * num20)) + (num2 * num18)) * num;
            matrix2.M42 = (((num5 * num21) - (num4 * num19)) + (num3 * num18)) * num;
            float num35 = (num7 * num10) - (num6 * num11);
            float num34 = (num8 * num10) - (num6 * num12);
            float num33 = (num8 * num11) - (num7 * num12);
            float num32 = (num9 * num10) - (num6 * num13);
            float num31 = (num9 * num11) - (num7 * num13);
            float num30 = (num9 * num12) - (num8 * num13);
            matrix2.M13 = (((num4 * num35) - (num3 * num34)) + (num2 * num33)) * num;
            matrix2.M23 = -(((num5 * num35) - (num3 * num32)) + (num2 * num31)) * num;
            matrix2.M33 = (((num5 * num34) - (num4 * num32)) + (num2 * num30)) * num;
            matrix2.M43 = -(((num5 * num33) - (num4 * num31)) + (num3 * num30)) * num;
            float num29 = (num7 * num14) - (num6 * num15);
            float num28 = (num8 * num14) - (num6 * num16);
            float num27 = (num8 * num15) - (num7 * num16);
            float num26 = (num9 * num14) - (num6 * num17);
            float num25 = (num9 * num15) - (num7 * num17);
            float num24 = (num9 * num16) - (num8 * num17);
            matrix2.M14 = -(((num4 * num29) - (num3 * num28)) + (num2 * num27)) * num;
            matrix2.M24 = (((num5 * num29) - (num3 * num26)) + (num2 * num25)) * num;
            matrix2.M34 = -(((num5 * num28) - (num4 * num26)) + (num2 * num24)) * num;
            matrix2.M44 = (((num5 * num27) - (num4 * num25)) + (num3 * num24)) * num;
            return matrix2;
        }
        #endregion 
    }

    /// <summary>
    /// Helper functions
    /// </summary>
    public static class Utilities
    {
        /// <summary>
        /// Stop watch instance
        /// </summary>
        private static Stopwatch watch = Stopwatch.StartNew();

        /// <summary>
        /// Gets elapsed time, in seconds since class initialization
        /// </summary>
        public static double ElapsedSecondsSinceStart
        {
            get
            {
                return watch.Elapsed.TotalSeconds;
            }
        }

        /// <summary>
        /// Gets elapsed time, in milliseconds since class initialization
        /// </summary>
        public static double ElapsedMilliseconds
        {
            get
            {
                return watch.Elapsed.TotalMilliseconds;
            }
        }

        /// <summary>
        /// This helper method is used to extract the service absolute path from
        /// the service identifier.
        /// </summary>
        /// <param name="serviceIdentifier">Service identifier</param>
        /// <returns>Service absolute path</returns>
        public static string GetServiceAbsolutePath(string serviceIdentifier)
        {
            var path = serviceIdentifier;

            if (string.IsNullOrEmpty(serviceIdentifier) == false)
            {
                try
                {
                    // Only interested in the service absolute path
                    var uri = new Uri(serviceIdentifier);
                    path = uri.AbsolutePath;
                }
                catch
                {
                    // Simply return what pass in to this method
                }
            }

            return path;
        }
    }
}
