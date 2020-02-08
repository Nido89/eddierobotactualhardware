//------------------------------------------------------------------------------
//  <copyright file="InfraredState.cs" company="Microsoft Corporation">
//      Copyright (C) Microsoft Corporation.  All rights reserved.
//  </copyright>
//------------------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//------------------------------------------------------------------------------

using Microsoft.Dss.Core.Attributes;
using Microsoft.Robotics.PhysicalModel;

using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Microsoft.Robotics.Services.Infrared
{
    /// <summary>
    /// Infrared State
    /// </summary>
    [DataContract]
    [Description("The state of the IR sensor.")]
    public class InfraredState
    {
        /// <summary>
        /// Timestamp of this sample
        /// </summary>
        [DataMember(XmlOmitDefaultValue = true)]
        [Browsable(false)]
        [Description("Identifies the timestamp for the reading of the sensor.")]
        public DateTime TimeStamp
        {
            get;
            set;
        }

        /// <summary>
        /// Hardware port identifier
        /// </summary>
        [DataMember]
        [Description("Identifies the hardware ID of the sensor.")]
        public int HardwareIdentifier
        {
            get;
            set;
        }

        /// <summary>
        /// Hardware type identifier
        /// </summary>
        [DataMember]
        [Description("Identifies the model or type of the sensor.")]
        public string ManufacturerIdentifier
        {
            get;
            set;
        }

        /// <summary>
        /// Min distance sensor can read in meters
        /// </summary>
        [DataMember]
        [Description("Specifies the minimum distance the sensor can read in meters.")]
        public double MinDistance
        {
            get;
            set;
        }

        /// <summary>
        /// Max distance sensor can read
        /// </summary>
        [DataMember]
        [Description("Specifies the maximum distance the sensor can read in meters.")]
        public double MaxDistance
        {
            get;
            set;
        }

        /// <summary>
        /// The distance reading in meters.
        /// </summary>
        [DataMember]
        [Browsable(false)]
        [Description("Identifies the distance reading of the sensor in meters.")]
        public double DistanceMeasurement
        {
            get;
            set;
        }

        /// <summary>
        /// Position and orientation
        /// </summary>
        [DataMember]
        [Description("The position and orientation of the sensor.")]
        public Pose Pose
        {
            get;
            set;
        }
    }
}
