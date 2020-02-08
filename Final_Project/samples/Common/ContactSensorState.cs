//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: ContactSensorState.cs $ $Revision: 17 $
//-----------------------------------------------------------------------

using Microsoft.Dss.Core.Attributes;
using Microsoft.Robotics.PhysicalModel;

using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Microsoft.Robotics.Services.ContactSensor
{
    /// <summary>
    /// Definition of a Binary Switch Sensor
    /// </summary>
    [DataContract]
    [Description("The state of the contact sensor.")]
    public class ContactSensor
    {
        private string _name;
        private int _hardwareIdentifier;
        private DateTime _timeStamp;
        private bool _pressed;
        private Pose _pose;

        /// <summary>
        /// Descriptive identifier string for this sensor (or bank of sensors)
        /// </summary>
        [DataMember]
        [DataMemberConstructor(Order = 2)]
        [Description("Specifies the descriptive identifier for this sensor (or set of sensors).")]
        public string Name
        {
            get { return this._name; }
            set { this._name = value; }
        }

        /// <summary>
        /// Descriptive identifier number for this sensor
        /// </summary>
        [DataMember]
        [DataMemberConstructor(Order = 1)]
        [Description("Identifies the hardware port for the sensor.")]
        public int HardwareIdentifier
        {
            get { return this._hardwareIdentifier; }
            set { this._hardwareIdentifier = value; }
        }

        /// <summary>
        /// Last time sensor was updated
        /// </summary>
        [DataMember(XmlOmitDefaultValue = true)]
        [Browsable(false)]
        [Description("Identifies the timestamp for the sensor update.")]
        public DateTime TimeStamp
        {
            get { return this._timeStamp; }
            set { this._timeStamp = value; }
        }

        /// <summary>
        /// The state of the binary sensor
        /// </summary>
        [DataMember]
        [Browsable(false)]
        [Description("Identifies the state of the sensor.")]
        public bool Pressed
        {
            get { return this._pressed; }
            set { this._pressed = value; }
        }

        /// <summary>
        /// Position and orientation
        /// </summary>
        [DataMember]
        [Description("The position and orientation of the sensor.")]
        public Pose Pose
        {
            get { return _pose; }
            set { _pose = value; }
        }
    }

    /// <summary>
    /// A list of binary sensors
    /// </summary>
    [DataContract]
    [Description("The state of the set (array) of contact sensors.")]
    public class ContactSensorArrayState
    {

        private List<ContactSensor> _sensors;

        /// <summary>
        /// The list of sensors
        /// </summary>
        [DataMember]
        [Description("The set of contact sensors.")]
        public List<ContactSensor> Sensors
        {
            get { return this._sensors; }
            set { this._sensors = value; }
        }
    }
}
