//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: SonarState.cs $ $Revision: 17 $
//-----------------------------------------------------------------------

using Microsoft.Dss.Core.Attributes;
using Microsoft.Robotics.PhysicalModel;

using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Microsoft.Robotics.Services.Sonar
{

    /// <summary>
    /// Sonar State
    /// </summary>
    [DataContract]
    [Description("The state of the sonar sensor.")]
    public class SonarState
    {
        private DateTime _timeStamp;

        private int _hardwareIdentifier;
        private double _distanceMeasurement;

        private double _angularRange;
        private double _maxDistance;

        private double _angularResolution;
        private Pose _pose;
        private double[] _distanceMeasurements;


        /// <summary>
        /// Timestamp of this sample
        /// </summary>
        [DataMember(XmlOmitDefaultValue = true)]
        [Browsable(false)]
        [Description("Identifies the timestamp for the reading of the sonar sensor.")]
        public DateTime TimeStamp
        {
            get { return _timeStamp; }
            set { _timeStamp = value; }
        }

        /// <summary>
        /// Hardware port identifier
        /// </summary>
        [DataMember]
        [Description("Identifies the hardware port of the sonar sensor.")]
        public int HardwareIdentifier
        {
            get { return _hardwareIdentifier; }
            set { _hardwareIdentifier = value; }
        }

        /// <summary>
        /// Max distance sensor can read in meters
        /// </summary>
        [DataMember]
        [Description("Specifies the maximum distance the sensor can read in meters.")]
        public double MaxDistance
        {
            get { return _maxDistance; }
            set { _maxDistance = value; }
        }

        /// <summary>
        /// The distance reading in meters.
        /// </summary>
        [DataMember]
        [Browsable(false)]
        [Description("Identifies the distance reading of the sonar in meters.")]
        public double DistanceMeasurement
        {
            get { return this._distanceMeasurement; }
            set { this._distanceMeasurement = value; }
        }

        /// <summary>
        /// Angular range of the measurement.
        /// </summary>
        [DataMember]
        [Description("Specifies the sonar's scanning angle.")]
        public double AngularRange
        {
            get { return this._angularRange; }
            set { this._angularRange = value; }
        }

        /// <summary>
        /// Resolution of the raycasting
        /// </summary>
        [DataMember]
        [Description("Specifies the size of smallest detectable feature (in radians).")]
        public double AngularResolution
        {
            get { return this._angularResolution; }
            set { this._angularResolution = value; }
        }

        /// <summary>
        /// Position and orientation
        /// </summary>
        [DataMember]
        [Description("The position and orientation of the sonar sensor.")]
        public Pose Pose
        {
            get { return _pose; }
            set { _pose = value; }
        }

        /// <summary>
        /// Array of distance readings.
        /// <remarks>NOTE: This is just a discretization of the sensor's cone for raycasting</remarks>
        /// </summary>
        [DataMember]
        [Browsable(false)]
        [Description("Identifies the set of distance readings.")]
        public double[] DistanceMeasurements
        {
            get { return this._distanceMeasurements; }
            set { this._distanceMeasurements = value; }
        }
    }
}
