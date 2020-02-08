//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: AnalogSensorState.cs $ $Revision: 17 $
//-----------------------------------------------------------------------

using Microsoft.Dss.Core.Attributes;
using Microsoft.Robotics.PhysicalModel;

using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Microsoft.Robotics.Services.AnalogSensor
{
    /// <summary>
    /// Analog Sensor State
    /// </summary>
    [DataContract]
    [Description ("The state of the analog sensor.")]
    public class AnalogSensorState
    {
        private DateTime _timeStamp;
        private int _hardwareIdentifier;
        private double _rawMeasurement;
        private double _rawMeasurementRange;
        private double _normalizedMeasurement;
        private Pose _pose;

        /// <summary>
        /// Timestamp of this sample
        /// </summary>
        [DataMember(XmlOmitDefaultValue = true)]
        [Browsable (false)]
        [Description("Indicates the timestamp of the sensor reading.")]
        public DateTime TimeStamp
        {
            get { return _timeStamp; }
            set { _timeStamp = value; }
        }

        /// <summary>
        /// Hardware port identifier
        /// </summary>
        [DataMember]
        [Description("Identifies the hardware port for the sensor.")]
        public int HardwareIdentifier
        {
            get { return _hardwareIdentifier; }
            set { _hardwareIdentifier = value; }
        }

        /// <summary>
        /// The analog raw measurement.
        /// </summary>
        [DataMember]
        [Browsable(false)]
        [Description("Provides the raw input value from the sensor.")]
        public double RawMeasurement
        {
            get { return this._rawMeasurement; }
            set { this._rawMeasurement = value; }
        }

        /// <summary>
        /// Normalized measurement relative to the RawMeasurementRange property. This value is in the range from 0 to 1.
        /// </summary>
        [DataMember]
        [Browsable(false)]
        [Description("Provides the normalized input value (relative to the RawMeasurementRange property).\nThis value is in the range from 0 to 1.")]
        public double NormalizedMeasurement
        {
            get { return _normalizedMeasurement; }
            set { _normalizedMeasurement = value; }
        }

        /// <summary>
        /// This is the upper bound for the raw measurement values
        /// </summary>
        [DataMember]
        [Description("This is the upper bound for the raw measurement values.")]
        public double RawMeasurementRange
        {
            get { return _rawMeasurementRange; }
            set { _rawMeasurementRange = value; }
        }

        /// <summary>
        /// Position and orientation
        /// </summary>
        [DataMember]
        [Description("Specifies the position and orientation of the sensor.")]
        public Pose Pose
        {
            get { return _pose; }
            set { _pose = value; }
        }
    }
}
