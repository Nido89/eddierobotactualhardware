//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: SimpleDashboardState.cs $ $Revision: 3 $
//-----------------------------------------------------------------------

using Microsoft.Dss.Core.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;

using drive = Microsoft.Robotics.Services.Drive.Proxy;
using sicklrf = Microsoft.Robotics.Services.Sensors.SickLRF.Proxy;

namespace Microsoft.Robotics.Services.SimpleDashboard
{
    /// <summary>
    /// SimpleDashboard StateType
    /// </summary>
    [DataContract]
    public class SimpleDashboardState
    {
        /// <summary>
        /// Log - Logging is turned on if true
        /// </summary>
        [DataMember]
        [Description ("Specifies whether to log messages.")]
        public bool Log;

        /// <summary>
        /// LogFile - The name of the log file
        /// </summary>
        [DataMember]
        [Description("Specifies the filename to log the data to.")]
        public string LogFile;
        
        /// <summary>
        /// Adjusts the sensitivity for driving forwards/backwards
        /// </summary>
        [DataMember]
        [Description("Specifies the a scaling factor for forward and backward motion.")]
        public double TranslateScaleFactor;

        /// <summary>
        /// Adjusts the sensitivity for rotating
        /// </summary>
        [DataMember]
        [Description("Specifies the scaling factor for rotation.")]
        public double RotateScaleFactor;
    }
}
