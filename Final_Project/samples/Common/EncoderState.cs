//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: EncoderState.cs $ $Revision: 17 $
//-----------------------------------------------------------------------

using Microsoft.Dss.Core.Attributes;

using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Microsoft.Robotics.Services.Encoder
{
    /// <summary>
    /// Update TickCount Request
    /// </summary>
    [DataContract]
    public class UpdateTickCountRequest
    {
        private DateTime _timeStamp;

        /// <summary>
        /// Time Stamp
        /// </summary>
        [DataMemberConstructor]
        [DataMember(XmlOmitDefaultValue = true)]
        [Browsable(false)]
        [Description("Identifies the timestamp of the latest reading.")]
        public DateTime TimeStamp
        {
            get { return _timeStamp; }
            set { _timeStamp = value; }
        }
        private int _count;

        /// <summary>
        /// Count
        /// </summary>
        [DataMemberConstructor]
        [DataMember]
        [Browsable (false)]
        [Description("Identifies the tick count of the latest reading.")]
        public int Count
        {
            get { return _count; }
            set { _count = value; }
        }
    }

    /// <summary>
    /// Definition of a wheel encoder
    /// </summary>
    [DataContract]
    [Description("The state of the encoder.")]
    public class EncoderState
    {
        private DateTime _timeStamp;

        private int _ticksSinceReset;
        private double _currentAngle;
        private int _currentReading;
        private int _ticksPerRevolution;
        private int _hardwareIdentifier;

        /// <summary>
        /// Timestamp of this sample
        /// </summary>
        [DataMember(XmlOmitDefaultValue = true)]
        [Browsable(false)]
        [Description("Identifies the timestamp of the encoder reading.")]
        public DateTime TimeStamp
        {
            get { return _timeStamp; }
            set { _timeStamp = value; }
        }

        /// <summary>
        /// Number of increments since last reset
        /// </summary>
        [DataMember]
        [Browsable(false)]
        [Description("Specifies the number of increments since last reset.")]
        public int TicksSinceReset
        {
            get { return this._ticksSinceReset; }
            set { this._ticksSinceReset = value; }
        }

        /// <summary>
        /// Current reading in terms of angle, in radians.
        /// </summary>
        /// <remarks>Valid if TicksPerRevolution set</remarks>
        [DataMember]
        [Browsable(false)]
        [Description("Indicates the current angle reading (in radians).")]
        public double CurrentAngle
        {
            get { return _currentAngle; }
            set { _currentAngle = value; }
        }

        /// <summary>
        /// Current reading, in ticks
        /// </summary>
        [DataMember]
        [Browsable(false)]
        [Description("Indicates the current encoder reading (in ticks).")]
        public int CurrentReading
        {
            get { return _currentReading; }
            set { _currentReading = value; }
        }

        /// <summary>
        /// Number of ticks per axle revolution
        /// </summary>
        [DataMember]
        [DataMemberConstructor(Order = 2)]
        [Description("Indicates the number of ticks per axle revolution.")]
        public int TicksPerRevolution
        {
            get { return this._ticksPerRevolution; }
            set { this._ticksPerRevolution = value; }
        }

        /// <summary>
        /// Hardware port identifier
        /// </summary>
        [DataMember]
        [DataMemberConstructor(Order = 1)]
        [Description("Identifies the hardware port.")]
        public int HardwareIdentifier
        {
            get { return _hardwareIdentifier; }
            set { _hardwareIdentifier = value; }
        }

    }

    /// <summary>
    /// Reset the TicksSinceReset counter
    /// </summary>
    [DataContract]
    public class ResetCounter
    {

    }

}
