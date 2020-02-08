//------------------------------------------------------------------------------
//  <copyright file="GpioPinArrayState.cs" company="Microsoft Corporation">
//      Copyright (C) Microsoft Corporation.  All rights reserved.
//  </copyright>
//------------------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//------------------------------------------------------------------------------

namespace Microsoft.Robotics.Services.GpioPinArray
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using Microsoft.Ccr.Core;
    using Microsoft.Dss.Core.Attributes;
    using Microsoft.Dss.Core.DsspHttp;
    using Microsoft.Dss.ServiceModel.Dssp;
    using Microsoft.Robotics.PhysicalModel;
    using W3C.Soap;
    
    /// <summary>
    /// Pin State
    /// </summary>
    [DataContract]
    [Description("The state of a pin.")]
    public class GpioPinState
    {
        /// <summary>
        /// The I/O direction of a pin
        /// </summary>
        [DataContract]
        public enum GpioPinDirection
        {
            /// <summary>
            /// Input pin
            /// </summary>
            In,

            /// <summary>
            /// Output pin
            /// </summary>
            Out,
        }

        /// <summary>
        /// The signal state of a pin
        /// </summary>
        [DataContract]
        public enum GpioPinSignal
        {
            /// <summary>
            /// Voltage driven high
            /// </summary>
            High,

            /// <summary>
            /// Pulled to ground
            /// </summary>
            Low,
        }

        /// <summary>
        /// Gets or sets timestamp of this sample
        /// </summary>
        [DataMember(XmlOmitDefaultValue = true)]
        [Browsable(false)]
        [Description("Indicates the timestamp of the pin state.")]
        public DateTime TimeStamp
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets pin location identifier
        /// </summary>
        [DataMember]
        [Description("Location identifier for the pin.")]
        public int Number
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets pin name
        /// </summary>
        [DataMember]
        [Description("Friendly identifier for the pin.")]
        public string Name
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets pin signal
        /// </summary>
        [DataMember]
        [Description("Voltage state of the pin.")]
        public GpioPinSignal PinState
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets pin direction
        /// </summary>
        [DataMember]
        [Description("Data direction for the pin.")]
        public GpioPinDirection PinDirection
        {
            get;
            set;
        }
    }

    /// <summary>
    /// List of GPIO pins
    /// </summary>
    [DataContract]
    public class GpioPinArrayState
    {
        /// <summary>
        /// Gets or sets the list of pins
        /// </summary>
        [DataMember]
        [Description("The set of GPIO pins.")]
        public List<GpioPinState> Pins
        {
            get;
            set;
        }
    }
}
