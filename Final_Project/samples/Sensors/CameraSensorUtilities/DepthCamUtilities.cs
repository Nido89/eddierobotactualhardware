//------------------------------------------------------------------------------
//  <copyright file="DepthCamUtilities.cs" company="Microsoft Corporation">
//      Copyright (C) Microsoft Corporation.  All rights reserved.
//  </copyright>
//------------------------------------------------------------------------------
namespace Microsoft.Robotics.Common
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using Microsoft.Robotics.PhysicalModel;
    using Microsoft.Robotics.Services.DepthCamSensor;

    /// <summary>
    /// Depth image utilities
    /// </summary>
    public static class DepthImageUtilities
    {
        /// <summary>
        /// Threshold defining what constitutes a large gradient change.
        /// </summary>
        private const int LargeGradientChangeInMm = 500;

        /// <summary>
        /// Three meters in mm.
        /// </summary>
        private const int ThreeMetersInMm = 3000;

        /// <summary>
        /// Class for tracking spans of no reading in the horizontal depth profile
        /// </summary>
        private class NoDepthReadingSpan
        {
            /// <summary>
            /// Gets index of the first no-reading in the span
            /// </summary>
            public int StartIndex { get; private set; }

            /// <summary>
            /// Gets the number of no readings in this span
            /// </summary>
            public int Count { get; private set; }

            /// <summary>
            /// Construct a NoDepthReadingSpan
            /// </summary>
            /// <param name="startIndex">Index of the first no-reading</param>
            /// <param name="count">Number of no readings in this span</param>
            public NoDepthReadingSpan(int startIndex, int count)
            {
                this.StartIndex = startIndex;
                this.Count = count;
            }
        }

        /// <summary>
        /// Downsamples depth image using the row and column scaling steps
        /// </summary>
        /// <param name="depthCamState">Depth cam sensor state with original image buffer</param>
        /// <param name="targetDepthImage">Target buffer for downsampled image</param>
        /// <param name="columnStep">Column sampling interval</param>
        /// <param name="rowStep">Row sampling interval</param>
        public static void DownSampleDepthImage(
            DepthCamSensorState depthCamState,
            short[] targetDepthImage,
            int columnStep,
            int rowStep)
        {
            DownSampleDepthImageInternal(depthCamState, targetDepthImage, columnStep, rowStep);
        }

        /// <summary>
        /// Downsamples depth image using the row and column scaling steps
        /// </summary>
        /// <param name="depthCamState">Depth cam sensor state with original image buffer</param>
        /// <param name="targetDepthImage">Target buffer for downsampled image</param>
        /// <param name="columnStep">Column sampling interval</param>
        /// <param name="rowStep">Row sampling interval</param>
        private static unsafe void DownSampleDepthImageInternal(
            DepthCamSensorState depthCamState,
            short[] targetDepthImage,
            int columnStep,
            int rowStep)
        {
            int downSampledWidth = depthCamState.DepthImageSize.Width / columnStep;
            fixed (short* depthImagePointer = &depthCamState.DepthImage[0])
            {
                fixed (short* depthImageTargetPointer = &targetDepthImage[0])
                {
                    for (int y = 0; y < depthCamState.DepthImageSize.Height; y += rowStep)
                    {
                        for (int x = 0; x < depthCamState.DepthImageSize.Width; x += columnStep)
                        {
                            int targetOffset = ((y / rowStep) * downSampledWidth) + (x / columnStep);
                            int sourceOffset = (y * depthCamState.DepthImageSize.Width) + x;
                            depthImageTargetPointer[targetOffset] = depthImagePointer[sourceOffset];
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Computes a horizontal depth profile as an array of byte values
        /// </summary>
        /// <param name="depthData">Depth image</param>
        /// <param name="imageSize">Depth image size</param>
        /// <param name="noReadingDepthValue">Value indicating depth is not valid</param>
        /// <param name="floorDepthValue">Value to use to designate floor</param>
        /// <param name="deadZoneColumnsFromImageRightEdge">Zero or more pixel columns on right edge of depth image to ignore</param>
        /// <param name="minDepth">Minimum depth value in millimeters</param>
        /// <param name="maxDepth">Maximum depth value in millimeters</param>
        /// <param name="numberOfBins">Number of bins in the profile</param>
        /// <returns>The horizontal profile as byte array</returns>
        public static byte[] CalculateHorizontalDepthProfileAsByteArray(
            short[] depthData,
            Size imageSize,
            short noReadingDepthValue,
            short floorDepthValue,
            int deadZoneColumnsFromImageRightEdge,
            int minDepth,
            int maxDepth,
            int numberOfBins)
        {
            return CalculateHorizontalProfile(
                depthData,
                imageSize,
                noReadingDepthValue,
                floorDepthValue,
                deadZoneColumnsFromImageRightEdge,
                minDepth,
                maxDepth,
                numberOfBins,
                null);
        }

        /// <summary>
        /// Computes a horizontal depth profile as an array of short values
        /// </summary>
        /// <param name="depthData">Depth image</param>
        /// <param name="imageWidth">Depth image width</param>
        /// <param name="imageHeight">Depth image height</param>
        /// <param name="noReadingDepthValue">Value indicating depth is not valid</param>
        /// <param name="floorDepthValue">Value to use to designate floor</param>
        /// <param name="deadZoneColumnsFromImageRightEdge">Zero or more pixel columns on right edge of depth image to ignore</param>
        /// <param name="minDepth">Minimum depth value in millimeters</param>
        /// <param name="maxDepth">Maximum depth value in millimeters</param>
        /// <param name="numberOfBins">Number of bins in the profile</param>
        /// <returns>The horizontal profile as byte array</returns>
        public static short[] CalculateHorizontalDepthProfileAsShortArray(
            short[] depthData,
            int imageWidth,
            int imageHeight,
            short noReadingDepthValue,
            short floorDepthValue,
            int deadZoneColumnsFromImageRightEdge,
            int minDepth,
            int maxDepth,
            int numberOfBins)
        {
            var depthProfile = new short[imageWidth];
            CalculateHorizontalProfile(
                depthData,
                new Size(imageWidth, imageHeight),
                noReadingDepthValue,
                floorDepthValue,
                deadZoneColumnsFromImageRightEdge,
                minDepth,
                maxDepth,
                numberOfBins,
                depthProfile);
            return depthProfile;
        }
        
        /// <summary>
        /// Computes a horizontal depth profile
        /// </summary>
        /// <param name="depthData">Depth image</param>
        /// <param name="imageSize">Depth image size</param>
        /// <param name="noReadingDepthValue">Value indicating depth is not valid</param>
        /// <param name="floorDepthValue">Value to use to designate floor</param>
        /// <param name="deadZoneAtRightOfImage">Zero or more pixel columns on right edge of depth image to ignore</param>
        /// <param name="minDepth">Minimum depth value in millimeters</param>
        /// <param name="maxDepth">Maximum depth value in millimeters</param>
        /// <param name="numberOfBins">Number of bins in the profile</param>
        /// <param name="depthHorizontalProfile">Dpeth profile as short array. If array is null, parameter is ignored</param>
        /// <returns>The horizontal profile as byte array</returns>
        private static unsafe byte[] CalculateHorizontalProfile(
            short[] depthData,
            Size imageSize,
            short noReadingDepthValue,
            short floorDepthValue,
            int deadZoneAtRightOfImage,
            int minDepth,
            int maxDepth,
            int numberOfBins,
            short[] depthHorizontalProfile)
        {
            if (numberOfBins <= 0)
            {
                return null;
            }

            if (depthData == null)
            {
                throw new ArgumentException("Depth Data cannot be null.");
            }

            // will do all calculations with a full version of the profile and then downscale.
            if (depthHorizontalProfile == null)
            {
                depthHorizontalProfile = new short[numberOfBins];
            }

            int width = imageSize.Width;
            int height = imageSize.Height;
            int downscaleDivider = imageSize.Width / numberOfBins;

            // initialize to max Depth
            for (int i = 0; i < numberOfBins; i++)
            {
                depthHorizontalProfile[i] = short.MaxValue;
            }

            fixed (short* depthDataOffset = depthData)
            {
                for (int x = 0; x < width - deadZoneAtRightOfImage; x++)
                {
                    for (int y = height - 1; y >= 0; y--)
                    {
                        short depth = *(depthDataOffset + (y * imageSize.Width) + x);

                        // merge this reading with the others if it is not a non-reading
                        if (depth != (short)noReadingDepthValue)
                        {
                            depthHorizontalProfile[x / downscaleDivider] = Math.Min(depth, depthHorizontalProfile[x / downscaleDivider]);
                        }
                    }
                }
            }

            // downscale the whole profile to create a byte version.
            byte[] horizontalProfileBytes = new byte[width];
            for (int i = 0; i < width; i++)
            {
                horizontalProfileBytes[i] = (byte)((byte.MaxValue * depthHorizontalProfile[i]) / maxDepth);
            }

            return horizontalProfileBytes;
        }

        /// <summary>
        /// Calculate the width of the observed open space.
        /// </summary>
        /// <param name="depthImageWidth">Width of the depth image</param>
        /// <param name="depthImageHeight">Height of the depth image</param>
        /// <param name="pixelColumnCountAtRightImageEdgeToIgnore">Number of pixel columns to ignore on the right edge of the image</param>
        /// <param name="invProjMatrix">Inverse projection matrix</param>
        /// <param name="horizontalDepthProfileArr">Horizontal depth profile</param>
        /// <param name="halfScreenHeight">Half screen height</param>
        /// <param name="robotWidthSquared">Width of the robot squared</param>
        /// <param name="distanceThresholdForOpenSpaceMillimeters">Depth value considered as obstacle threshold</param>
        /// <param name="smallestProjectedWidthSquared">The smallest projected width squared</param>
        /// <param name="bestStartIndex">The best starting index</param>
        /// <param name="avgDepthOfOpening">Average depth of the opening</param>
        /// <param name="nearObstacleIndex">Sum of indices at which an obstacle was observed</param>
        /// <param name="nearObstacleCount">Number of pixels at which an obstacle was observed</param>
        /// <param name="bestWidthInPixels">Width of the best open space in pixels</param>
        public static unsafe void CalculateWidthOfOpenSpaceFromDepthHorizontalProfile(
            int depthImageWidth,
            int depthImageHeight,
            int pixelColumnCountAtRightImageEdgeToIgnore,
            ref Matrix invProjMatrix,
            short[] horizontalDepthProfileArr,
            int halfScreenHeight,
            float robotWidthSquared,
            int distanceThresholdForOpenSpaceMillimeters,
            out float smallestProjectedWidthSquared,
            out int bestStartIndex,
            out int avgDepthOfOpening,
            out double nearObstacleIndex,
            out double nearObstacleCount,
            out int bestWidthInPixels)
        {
            int startIndex = 0, width = 0;
            smallestProjectedWidthSquared = 0;
            bestStartIndex = 0;
            avgDepthOfOpening = 0;
            nearObstacleIndex = 0;
            nearObstacleCount = 0;
            bestWidthInPixels = 0;

            // latest depthcams report no reading values at far right edge of image ( last N pixel columns at 320)
            int deadZonePixeCount = pixelColumnCountAtRightImageEdgeToIgnore;

            // specific init value not important, just has ot be large enough to not appear as obstacle
            int startDepthOfOpening = 10000,
                endDepthOfOpening = 10000;

            int horizontalDepthProfileLength = horizontalDepthProfileArr.Length;
            const short DefaultDepthForInitialization = 4000;

            int minGradientIndex = 0,
                bestMinGradientIndex = 0,
                maxGradientIndex = horizontalDepthProfileLength - 1,
                bestMaxGradientIndex = horizontalDepthProfileLength - 1,
                depthOfMinGradientIndex = DefaultDepthForInitialization,
                bestDepthOfMinGradientIndex = DefaultDepthForInitialization,
                depthOfMaxGradientIndex = DefaultDepthForInitialization,
                bestDepthOfMaxGradientIndex = DefaultDepthForInitialization;

            fixed (short* horizontalDepthProfile = horizontalDepthProfileArr)
            {
                float realWidthSquared = 0;

                // analyze depth summary to find open spaces with no obstacles
                for (int i = 0; i < horizontalDepthProfileArr.Length - deadZonePixeCount; i++)
                {
                    if (horizontalDepthProfile[i] <
                        distanceThresholdForOpenSpaceMillimeters)
                    {
                        nearObstacleIndex += i;
                        nearObstacleCount++;
                    }

                    // look for sharp changes in depth
                    if (minGradientIndex == 0)
                    {
                        if (LargeNegativeGradientChange(horizontalDepthProfile, i, horizontalDepthProfileLength))
                        {
                            minGradientIndex = i;
                            depthOfMinGradientIndex = horizontalDepthProfile[i];
                        }
                    }
                    else if (LargePositiveGradientChange(horizontalDepthProfile, i))
                    {
                        if (i > maxGradientIndex || maxGradientIndex == horizontalDepthProfileLength - 1)
                        {
                            maxGradientIndex = i;
                            depthOfMaxGradientIndex = horizontalDepthProfile[i];
                        }
                    }

                    // anything at threshold consider an obstacle
                    if (horizontalDepthProfile[i] <
                        distanceThresholdForOpenSpaceMillimeters)
                    {
                        endDepthOfOpening = horizontalDepthProfile[i];
                        startDepthOfOpening = horizontalDepthProfile[bestStartIndex];

                        realWidthSquared = ComputeWidthSquaredInWorldSpace(
                            depthImageWidth,
                            depthImageHeight,
                            invProjMatrix,
                            halfScreenHeight,
                            startDepthOfOpening,
                            endDepthOfOpening,
                            startIndex,
                            startIndex + width);

                        if ((realWidthSquared > robotWidthSquared || (startIndex == 0))
                            && width > bestWidthInPixels)
                        {
                            smallestProjectedWidthSquared = realWidthSquared;
                            bestStartIndex = startIndex;
                            bestWidthInPixels = width;
                            bestMinGradientIndex = minGradientIndex;
                            bestMaxGradientIndex = maxGradientIndex;
                            bestDepthOfMinGradientIndex = depthOfMinGradientIndex;
                            bestDepthOfMaxGradientIndex = depthOfMaxGradientIndex;
                        }

                        startIndex = i + 1;
                        width = 0;

                        minGradientIndex = 0;
                        maxGradientIndex = horizontalDepthProfileLength - 1;

                        continue;
                    }

                    width++;
                }

                realWidthSquared = ComputeWidthSquaredInWorldSpace(
                    depthImageWidth,
                    depthImageHeight,
                    invProjMatrix,
                    halfScreenHeight,
                    startDepthOfOpening,
                    horizontalDepthProfile[horizontalDepthProfileArr.Length - 1 - deadZonePixeCount],
                    startIndex,
                    startIndex + width);

                if (width > bestWidthInPixels)
                {
                    smallestProjectedWidthSquared = realWidthSquared;
                    bestStartIndex = startIndex;
                    bestWidthInPixels = width;
                    bestMinGradientIndex = minGradientIndex;
                    bestMaxGradientIndex = maxGradientIndex;
                    bestDepthOfMinGradientIndex = depthOfMinGradientIndex;
                    bestDepthOfMaxGradientIndex = depthOfMaxGradientIndex;
                }

                // if the large gradient opening is greater than the robot width, take that
                if (bestMinGradientIndex > bestStartIndex &&

                    // we add 1 because the inclusive length between two elements in an array
                    //  is their difference + 1 
                    bestMaxGradientIndex + 1 < bestStartIndex + bestWidthInPixels)
                {
                    realWidthSquared = ComputeWidthSquaredInWorldSpace(
                        depthImageWidth,
                        depthImageHeight,
                        invProjMatrix,
                        halfScreenHeight,
                        bestDepthOfMinGradientIndex,
                        bestDepthOfMaxGradientIndex,
                        bestMinGradientIndex,
                        bestMaxGradientIndex);

                    if (realWidthSquared > robotWidthSquared)
                    {
                        smallestProjectedWidthSquared = realWidthSquared;
                        bestStartIndex = bestMinGradientIndex;

                        // we add 1 because the inclusive length between two elements in an array
                        //  is their difference + 1 
                        bestWidthInPixels = (bestMaxGradientIndex - bestMinGradientIndex) + 1;
                    }
                }

                if (bestWidthInPixels > 0)
                {
                    for (int i = bestStartIndex; i < bestStartIndex + bestWidthInPixels; i++)
                    {
                        avgDepthOfOpening += horizontalDepthProfileArr[i];
                    }

                    avgDepthOfOpening /= bestWidthInPixels;
                }
                else
                {
                    avgDepthOfOpening = 0;
                }
            }
        }

        /// <summary>
        /// Computes squared width in meters. Assumes indices are for horizontal profile that is same length as depth image width
        /// </summary>
        /// <param name="depthImageWidth">Width of the depth image</param>
        /// <param name="depthImageHeight">Height of the depth image</param>
        /// <param name="invProjMatrix">Inverse projection matrix</param>
        /// <param name="halfScreenHeight">Half-height of the image</param>
        /// <param name="startDepthOfOpening">Starting depth of the opening</param>
        /// <param name="endDepthOfOpening">Ending depth of the opening</param>
        /// <param name="startIndexOfOpening">Starting index of the opening</param>
        /// <param name="endIndexOfOpening">Ending index of the opening</param>
        /// <returns>Returns the computed width in meters</returns>
        private static float ComputeWidthSquaredInWorldSpace(
            int depthImageWidth,
            int depthImageHeight,
            Matrix invProjMatrix,
            int halfScreenHeight,
            int startDepthOfOpening,
            int endDepthOfOpening,
            int startIndexOfOpening,
            int endIndexOfOpening)
        {
            // determine the *projected* 3d width of the opening based on the start/end depths
            int minDepthOfOpening = Math.Min(startDepthOfOpening, endDepthOfOpening);

            var v = MathUtilities.ConvertPixelSpaceToViewSpace(
                startIndexOfOpening,
                halfScreenHeight,
                minDepthOfOpening,
                invProjMatrix,
                depthImageWidth,
                depthImageHeight);

            var w = MathUtilities.ConvertPixelSpaceToViewSpace(
                endIndexOfOpening,
                halfScreenHeight,
                minDepthOfOpening,
                invProjMatrix,
                depthImageWidth,
                depthImageHeight);

            var d = v - w;
            return Vector3.Dot(d, d);
        }

        /// <summary>
        /// Scan the depth profile for a large negative gradient.
        /// </summary>
        /// <param name="depthProfile">The horizontal depth profile</param>
        /// <param name="i">The index to be checked</param>
        /// <param name="depthProfileLength">Length of the depth profile</param>
        /// <returns>True if a large negative gradient occurs at the given index</returns>
        private static unsafe bool LargeNegativeGradientChange(short* depthProfile, int i, int depthProfileLength)
        {
            if (i != depthProfileLength - 1 &&
                depthProfile[i] < ThreeMetersInMm &&
                depthProfile[i + 1] > ThreeMetersInMm)
            {
                var gradient = depthProfile[i + 1] - depthProfile[i];
                if (gradient > LargeGradientChangeInMm)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Scan the depth profile for a large positive gradient.
        /// </summary>
        /// <param name="depthProfile">The horizontal depth profile</param>
        /// <param name="i">The index to be checked</param>
        /// <returns>True if a large positive gradient occurs at the given index</returns>
        private static unsafe bool LargePositiveGradientChange(short* depthProfile, int i)
        {
            if (i != 0 && depthProfile[i - 1] > ThreeMetersInMm && depthProfile[i] < ThreeMetersInMm)
            {
                var gradient = depthProfile[i - 1] - depthProfile[i];
                if (gradient > LargeGradientChangeInMm)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Fuse the depth profile with the current sonar readings.
        /// </summary>
        /// <param name="minValidDepthCamReadingMillineters">Minimum depth value returned by depth camera</param>
        /// <param name="horizontalDepthProfile">The horizontal depth profile.</param>
        /// <param name="indexInProfileForLeftSonar">Index into depth profile to fuse left sonar value</param>
        /// <param name="leftSonarReading">Left sonar reading in meters</param>
        /// <param name="indexInProfileForRightSonar">Index into depth profile to fuse right sonar value</param>
        /// <param name="rightSonarReading">Right sonar reading in meters</param>
        public static unsafe void FuseDepthProfileWithSonarReadings(
            int minValidDepthCamReadingMillineters,
            short[] horizontalDepthProfile,
            int indexInProfileForLeftSonar,
            double leftSonarReading,
            int indexInProfileForRightSonar,
            double rightSonarReading)
        {
            // convert meters to millimeters so we can fuse with depth profile, which is millimeters
            var leftSonarValue = 1000 *
                leftSonarReading;
            var rightSonarValue = 1000 *
                rightSonarReading;

            // the sonars have a large FOV. Left sonar will cover the left 
            // half of our FOV, right the right half.
            // If both sonars fire, we will mark the middle third of the FOV.

            // The fusion logic for sonar:
            // If sonar reading is less the depthcam near range, we will use 
            // sonar reading over any other
            // if sonar reading is further than near range but less than near detection threshold
            // and both sonars are firing 
            // (so object is in the middle) *and* IR and depth report nothing
            // we will again use the sonar, since this is most likely glass            

            fixed (short* horizontalDepthPointer = horizontalDepthProfile)
            {
                for (int k = 0; k < horizontalDepthProfile.Length; k++)
                {
                    FuseDepthProfileWithSonarNearValues(
                        minValidDepthCamReadingMillineters,
                        leftSonarValue,
                        rightSonarValue,
                        indexInProfileForLeftSonar,
                       indexInProfileForRightSonar,
                        horizontalDepthPointer,
                        k);
                }
            }
        }

        /// <summary>
        /// Fuse the depth profile with the current IR readings
        /// </summary>
        /// <param name="depthCamFov">The depth camera field of view</param>
        /// <param name="minValidDepthCamReadingMillineters">Minimum depth value returned by depth camera</param>
        /// <param name="horizontalDepthProfile">The horizontal depth profile</param>
        /// <param name="sensorOrientationsInRadians">Array of sensor orientation values on the XZ (horizontal) plane</param>
        /// <param name="sensorReadingsInMeters">Array of sensor readings</param>
        /// <param name="sensorIndex">Sensor array start index</param>
        /// <param name="sensorCount">Sensor count</param>
        public static void FuseDepthProfilesWithIrReadings(
            double depthCamFov,
            int minValidDepthCamReadingMillineters,
            short[] horizontalDepthProfile,
            double[] sensorOrientationsInRadians,
            double[] sensorReadingsInMeters,
            int sensorIndex,
            int sensorCount)
        {
            if (sensorReadingsInMeters == null)
            {
                return;
            }

            double nearDistanceThresholdForIrReadings =
                (double)minValidDepthCamReadingMillineters / 1000.0;

            // the front IR sensors will contribute directly to the horizontal profile that captures the depthcam FOV
            // even if the FOV is narrower than what IR array covers, we make all our decisions on it so this will be
            // a conservative approach
            for (int i = sensorIndex; i < sensorCount + sensorIndex; i++)
            {
                var angle = sensorOrientationsInRadians[i];
                if (Math.Abs(angle) > depthCamFov / 2)
                {
                    continue;
                }

                var irReading = sensorReadingsInMeters[i];
                if (irReading >= nearDistanceThresholdForIrReadings)
                {
                    // ignore IR readings beyond Kinect camera near field.
                    continue;
                }

                // within depthcam FOV. Update several entries with the same IR reading so
                // obstacle does not appear too skinny
                int repeatCount = horizontalDepthProfile.Length / 4;
                int index = 0;

                if (angle == 0)
                {
                    // we re enforce the reading from the middle sensor since its of
                    // higher importance
                    repeatCount *= 2;
                    index = (horizontalDepthProfile.Length / 2) - 1 + (repeatCount / 2);
                }
                else if (angle < 0)
                {
                    index = repeatCount - 1;
                }
                else
                {
                    index = horizontalDepthProfile.Length - 1;
                }

                for (int k = 0; k < repeatCount; k++)
                {
                    double normalizedIrDepth = irReading;
                    var irDepth = (short)(normalizedIrDepth * 1000);
                    horizontalDepthProfile[index - k] = irDepth;
                }
            }
        }

        /// <summary>
        /// Fuse the horizontal depth profile with near sonar values.
        /// </summary>
        /// <param name="minValidDepthCamReadingMillineters">Minimum depth value returned by depth camera</param>
        /// <param name="leftSonarValue">The left sonar reading</param>
        /// <param name="rightSonarValue">The right sonar reading</param>
        /// <param name="leftMiddleIndex">Index corresponding to the center of FOV for the left sonar</param>
        /// <param name="rightMiddleIndex">Index corresponding to the center of FOV for the right sonar</param>
        /// <param name="horizontalDepthPointer">The horizontal depth profile</param>
        /// <param name="horizontalProfileIndex">The horizontal index of interest</param>
        private static unsafe void FuseDepthProfileWithSonarNearValues(
            int minValidDepthCamReadingMillineters,
            double leftSonarValue,
            double rightSonarValue,
            int leftMiddleIndex,
            int rightMiddleIndex,
            short* horizontalDepthPointer,
            int horizontalProfileIndex)
        {
            if ((horizontalProfileIndex > leftMiddleIndex && horizontalProfileIndex <= rightMiddleIndex) &&
                leftSonarValue < (short)minValidDepthCamReadingMillineters &&
                rightSonarValue < (short)minValidDepthCamReadingMillineters)
            {
                // there is something very close and directly infront of us. 
                // We dont care if IR or depth do not see it. It has very small
                // chance of being a false positive since at this distance, its 
                // tall enough to be seen
                double sonarDepth = Math.Min(leftSonarValue, rightSonarValue);
                horizontalDepthPointer[horizontalProfileIndex] = (short)sonarDepth;
            }
            else if ((horizontalProfileIndex < leftMiddleIndex) &&
                leftSonarValue < (short)minValidDepthCamReadingMillineters)
            {
                // there is something very close and to the leftof us. 
                double sonarDepth = leftSonarValue;
                horizontalDepthPointer[horizontalProfileIndex] = (short)sonarDepth;
            }
            else if ((horizontalProfileIndex >= rightMiddleIndex) &&
                rightSonarValue < (short)minValidDepthCamReadingMillineters)
            {
                // there is something very close and to the right of us. 
                double sonarDepth = rightSonarValue;
                horizontalDepthPointer[horizontalProfileIndex] = (short)sonarDepth;
            }
        }

        /// <summary>
        /// Generates a vector field of view space vectors with 1M z value
        /// </summary>
        /// <param name="width">Width of the vector field to generate</param>
        /// <param name="height">Height of the vector field to generate</param>
        /// <param name="invProjMatrix">Inverse projection matrix</param>
        /// <returns>A vector field of width times height elements</returns>
        public static Vector3[] GenerateVectorField(int width, int height, Matrix invProjMatrix)
        {
            var field = new Vector3[width * height];
            var depthInMillimeters = 1000;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    field[(y * width) + x] = MathUtilities.ConvertPixelSpaceToViewSpace(
                        x,
                        y,
                        depthInMillimeters,
                        invProjMatrix,
                        width,
                        height);
                }
            }

            return field;
        }

        /// <summary>
        /// Computes the depth values of a given floor and ceiling in pixel space
        /// </summary>
        /// <param name="cameraPose">The camera pose</param>
        /// <param name="floorThreshold">The floor threshold</param>
        /// <param name="ceilingThreshold">The ceiling threshold</param>
        /// <param name="invProjectionMatrix">The inverse projection matrix</param>
        /// <param name="floorCeilingMinDepths">The computed min depths of floor and ceiling</param>
        /// <param name="floorCeilingMaxDepths">The computed max depths of floor and ceiling</param>
        /// <param name="width">Width of the image</param>
        /// <param name="height">Height of the image</param>
        /// <param name="depthRange">Depth range of the camera</param>
        public static void ComputeCeilingAndFloorDepthsInPixelSpace(
            Pose cameraPose,
            float floorThreshold,
            float ceilingThreshold,
            Microsoft.Robotics.PhysicalModel.Matrix invProjectionMatrix,
            ref short[] floorCeilingMinDepths,
            ref short[] floorCeilingMaxDepths,
            int width,
            int height,
            float depthRange)
        {
            floorCeilingMinDepths = new short[width * height];
            floorCeilingMaxDepths = new short[width * height];

            ComputeCeilingAndFloorDepthsInPixelSpaceInternal(
                ref cameraPose,
                floorThreshold,
                ceilingThreshold,
                ref invProjectionMatrix,
                floorCeilingMinDepths,
                floorCeilingMaxDepths,
                width,
                height,
                depthRange);
        }

        /// <summary>
        /// Compute ceiling and floor depths in pixel space
        /// </summary>
        /// <param name="cameraPose">Pose of the camera</param>
        /// <param name="floorThreshold">Threshold for the floor</param>
        /// <param name="ceilingThreshold">Threshold for the ceiling</param>
        /// <param name="invProjectionMatrix">Inverse camera projection matrix</param>
        /// <param name="floorCeilingMinDepths">Floor and ceiling min depths</param>
        /// <param name="floorCeilingMaxDepths">Floor and ceiling max depths</param>
        /// <param name="width">Widht of the image</param>
        /// <param name="height">Height of the image</param>
        /// <param name="depthRange">Range of the depthcam</param>
        private static void ComputeCeilingAndFloorDepthsInPixelSpaceInternal(
            ref Pose cameraPose,
            float floorThreshold,
            float ceilingThreshold,
            ref Matrix invProjectionMatrix,
            short[] floorCeilingMinDepths,
            short[] floorCeilingMaxDepths,
            int width,
            int height,
            float depthRange)
        {
            float aX, bX, aY, bY;
            MathUtilities.ComputeTranslationScaleTransform(width, height, out aX, out bX, out aY, out bY);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // convert x,y,0 to view space (vx, vy, 0)
                    Vector3 viewVector = MathUtilities.ConvertPixelSpaceToViewSpace(
                        x,
                        y,
                        1,
                        invProjectionMatrix,
                        width,
                        aX,
                        bX,
                        aY,
                        bY);

                    viewVector = Vector3.Normalize(viewVector);

                    floorCeilingMaxDepths[(y * width) + x] =
                        floorCeilingMinDepths[(y * width) + x] = short.MaxValue;

                    short maxDepthMillimeters = (short)(depthRange * 1000);

                    // can't intersect with ceiling if not pointing upwards
                    if (viewVector.Y > 0)
                    {
                        float lengthOfVectorIntersectingCeiling = ceilingThreshold / viewVector.Y;
                        var vectorIntersectingCeiling = Vector3.Scale(Vector3.Normalize(viewVector), lengthOfVectorIntersectingCeiling);
                        var d = maxDepthMillimeters * vectorIntersectingCeiling.Z / depthRange;

                        floorCeilingMaxDepths[(y * width) + x] = (short)Math.Min(Math.Max(0, d), maxDepthMillimeters);
                    }

                    // can't intersect with floor if not pointing downwards
                    if (viewVector.Y < 0)
                    {
                        float lengthOfVectorIntersectingFloor = -cameraPose.Position.Y / viewVector.Y;
                        var vectorIntersectingFloor = Vector3.Scale(Vector3.Normalize(viewVector), lengthOfVectorIntersectingFloor);

                        // normalize from [0, depthRange] -> [0, maxDepth]
                        var d = maxDepthMillimeters * vectorIntersectingFloor.Z / depthRange;

                        floorCeilingMaxDepths[(y * width) + x] = (short)Math.Min(Math.Max(0, d), maxDepthMillimeters);

                        float lengthOfVectorIntersectingBelowFloor = (floorThreshold - cameraPose.Position.Y) / viewVector.Y;
                        var vectorIntersectingBelowFloor = Vector3.Scale(Vector3.Normalize(viewVector), lengthOfVectorIntersectingBelowFloor);

                        // normalize from [0, depthRange] -> [0, 254]
                        d = maxDepthMillimeters * vectorIntersectingBelowFloor.Z / depthRange;
                        floorCeilingMinDepths[(y * width) + x] = (short)Math.Min(Math.Max(0, d), maxDepthMillimeters);
                    }
                }
            }
        }

        /// <summary>
        /// Converts any depth value to 254 (255 is reserved), if at that distance, its above a certain height or below ground plane.
        /// This method is much faster than passing an inverse projection matrix
        /// </summary>
        /// <param name="floorHoleAsObstacleDepthThresholdAlongDepthAxisInMillimeters">
        /// Threshold on depth beyond floor plane for treating a hole in the floor plane as an obstacle </param>
        /// <param name="floorDetectionMarginInMillimeters">Margin for obstacles above floor</param>
        /// <param name="preFilterNoReadingDepthValue">Depth value from sensor indicating no reading was available</param>
        /// <param name="postFilterNoReadingDepthValue">Depth value to use, to replace sensor no reading values</param>
        /// <param name="floorDepthValue">Depth value to use to indicate floor</param>
        /// <param name="minValidDepthMillimeters">Mimimum valid depth in millimeters</param>
        /// <param name="maxValidDepthMillimeters">Maximum valid depth in millimeters</param>
        /// <param name="depthData">The depth image</param>
        /// <param name="floorCeilingDepths">The cached floor and ceiling depths</param>
        /// <param name="numberOfFloorPixels">Number of floor pixels detected</param>
        public static unsafe void FilterOutGroundAndAboveRobotValues(
            int floorHoleAsObstacleDepthThresholdAlongDepthAxisInMillimeters,
            int floorDetectionMarginInMillimeters,
            short preFilterNoReadingDepthValue,
            short postFilterNoReadingDepthValue,
            short floorDepthValue,
            short minValidDepthMillimeters,
            short maxValidDepthMillimeters,
            short[] depthData,
            short[] floorCeilingDepths,
            out int numberOfFloorPixels)
        {
            numberOfFloorPixels = 0;
            for (int i = 0; i < depthData.Length; ++i)
            {
                short sd = depthData[i];

                int floorDepth = floorCeilingDepths[i];

                if (sd == preFilterNoReadingDepthValue || sd == postFilterNoReadingDepthValue)
                {
                    depthData[i] = postFilterNoReadingDepthValue;
                }
                else if (sd < (int)maxValidDepthMillimeters &&
                    sd >= floorDepth + floorHoleAsObstacleDepthThresholdAlongDepthAxisInMillimeters)
                {
                    // for close depressions in the floor plane, treat them as obstacles
                    int min = minValidDepthMillimeters;
                    depthData[i] = (short)(floorDepth - (min * 2));
                }
                else if (sd >= floorDepth - floorDetectionMarginInMillimeters)
                {
                    // make the floor appear far away (not an obstacle)
                    depthData[i] = maxValidDepthMillimeters;
                    numberOfFloorPixels++;
                }
            }
        }
    }
}
