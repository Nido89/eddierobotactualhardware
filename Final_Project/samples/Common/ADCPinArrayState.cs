//------------------------------------------------------------------------------
//  <copyright file="ADCPinArrayState.cs" company="Microsoft Corporation">
//      Copyright (C) Microsoft Corporation.  All rights reserved.
//  </copyright>
//------------------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//------------------------------------------------------------------------------

namespace Microsoft.Robotics.Services.ADCPinArray
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
    public class ADCPinState
    {
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
        [Description("Board location identifier for the pin.")]
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
        /// Gets or sets an arbitrary pin value (millivolts, PWM, TTL, etc)
        /// </summary>
        [DataMember]
        [Description("Value of the pin.")]
        public double PinValue
        {
            get;
            set;
        }
    }

    /// <summary>
    /// List of GPIO pins
    /// </summary>
    [DataContract]
    public class ADCPinArrayState
    {
        /// <summary>
        /// Gets or sets the list of pins
        /// </summary>
        [DataMember]
        [Description("The set of ADC pins.")]
        public List<ADCPinState> Pins
        {
            get;
            set;
        }
    }
}
