//------------------------------------------------------------------------------
//  <copyright file="SonarSensorArrayState.cs" company="Microsoft Corporation">
//      Copyright (C) Microsoft Corporation.  All rights reserved.
//  </copyright>
//------------------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//------------------------------------------------------------------------------

namespace Microsoft.Robotics.Services.SonarSensorArray
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
    /// List of sensors
    /// </summary>
    [DataContract]
    public class SonarSensorArrayState
    {
        /// <summary>
        /// Gets or sets the list of sensors
        /// </summary>
        [DataMember]
        public List<Microsoft.Robotics.Services.Sonar.SonarState> Sensors
        {
            get;
            set;
        }
    }
}