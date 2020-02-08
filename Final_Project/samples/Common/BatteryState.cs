//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: BatteryState.cs $ $Revision: 16 $
//-----------------------------------------------------------------------

using Microsoft.Dss.Core.Attributes;
using Microsoft.Robotics.PhysicalModel;

using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Microsoft.Robotics.Services.Battery
{
    /// <summary>
    /// Battery Notification
    /// </summary>
    [DataContract]
    public class BatteryNotification
    {
        private int _maxBatteryPower;
        private double _percentBatteryPower;
        private Pose _pose;

        /// <summary>
        /// Max Battery Power
        /// </summary>
        [DataMember]
        [Description("Indicates the maximum battery level setting.")]
        public int MaxBatteryPower
        {
            get { return this._maxBatteryPower; }
            set { this._maxBatteryPower = value; }
        }

        /// <summary>
        /// Percent Battery Power
        /// </summary>
        [DataMember]
        [Description("Indicates the percentage battery power setting.")]
        public double PercentBatteryPower
        {
            get { return this._percentBatteryPower; }
            set { this._percentBatteryPower = value; }
        }

        /// <summary>
        /// Position and orientation
        /// </summary>
        [DataMember]
        [Description("Specifies the position and orientation of the battery.")]
        public Pose Pose
        {
            get { return _pose; }
            set { _pose = value; }
        }

        /// <summary>
        /// Battery Notification Constructor
        /// </summary>
        public BatteryNotification() { }

        /// <summary>
        /// Battery Notification Initialization Constructor
        /// </summary>
        /// <param name="max"></param>
        /// <param name="percent"></param>
        public BatteryNotification(int max, double percent)
        {
            _maxBatteryPower = max;
            _percentBatteryPower = percent;
        }
    }

    /// <summary>
    /// Battery State
    /// </summary>
    [DataContract]
    [Description("The battery's state.")]
    public class BatteryState
    {
        private double _maxBatteryPower;
        private double _percentBatteryPower;
        private double _percentCriticalBattery;

        /// <summary>
        /// Full battery power
        /// </summary>
        [DataMember]
        [Description("Identifies the power setting at which the battery is fully charged. (Default = 1.00)")]
        public double MaxBatteryPower
        {
            get { return this._maxBatteryPower; }
            set { this._maxBatteryPower = value; }
        }

        /// <summary>
        /// Percentage of remaining battery power
        /// between 0 and 1
        /// </summary>
        [DataMember]
        [Browsable(false)]
        [Description("Indicates the percentage of battery power remaining. (0% - 100%)")]
        public double PercentBatteryPower
        {
            get { return this._percentBatteryPower; }
            set { this._percentBatteryPower = value; }
        }

        /// <summary>
        /// Percent Critical Battery
        /// </summary>
        [DataMember]
        [Description("Indicates the percentage of the battery level at which operation may be impaired. (0% - 100%)")]
        public double PercentCriticalBattery
        {
            get { return this._percentCriticalBattery; }
            set { this._percentCriticalBattery = value; }
        }
    }

    /// <summary>
    /// Update Critical Battery
    /// </summary>
    [DataContract]
    public class UpdateCriticalBattery
    {
        private double _percentCriticalBattery;

        /// <summary>
        /// Percent Critical Battery
        /// </summary>
        [DataMember]
        public double PercentCriticalBattery
        {
            get { return this._percentCriticalBattery; }
            set { this._percentCriticalBattery = value; }
        }

    }
}
