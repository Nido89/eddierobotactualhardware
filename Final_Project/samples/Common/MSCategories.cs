//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: MSCategories.cs $ $Revision: 13 $
//-----------------------------------------------------------------------

using Microsoft.Ccr.Core;
using Microsoft.Dss.Core.Attributes;
using dssp = Microsoft.Dss.ServiceModel.Dssp;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Xml.Serialization;

using W3C.Soap;

namespace Microsoft.Robotics.Services
{
    /// <summary>
    /// MSCategories Contract
    /// </summary>
    public static class Contract
    {
        /// The Unique Contract Identifier for the MSCategories Contract
        public const string Identifier = "http://schemas.microsoft.com/robotics/mscategories.html";
    }

    /// <summary>
    /// Microsoft Robotics published categories.
    /// These categories are available for use by any Dss Service.
    /// </summary>
    [DataContract]
    public sealed class MicrosoftCategories
    {
        /// <summary>
        /// Indicates that your service is a type of Distance Measuring Device.
        /// </summary>
        [DataMember]
        [Description("Indicates that the service is a type of Distance Measuring Device.")]
        public const string DistanceMeasurement = "http://schemas.microsoft.com/categories/robotics/distancemeasurement.html";

        /// <summary>
        /// Indicates that your service is a robotics service.
        /// </summary>
        [DataMember]
        [Description("Indicates that the service is a robotics service.")]
        public const string Robotics = "http://schemas.microsoft.com/categories/robotics/robotics.html";

        /// <summary>
        /// Indicates that your service is a type of sensor.
        /// </summary>
        [DataMember]
        [Description("Indicates that the service is a type of sensor.")]
        public const string Sensor = "http://schemas.microsoft.com/categories/robotics/sensor.html";

        /// <summary>
        /// Indicates that your service is a type of actuator.
        /// </summary>
        [DataMember]
        [Description("Indicates that the service is a type of actuator.")]
        public const string Actuator = "http://schemas.microsoft.com/categories/robotics/actuator.html";

        /// <summary>
        /// Indicates that your service provides simulation of another Dss service.
        /// </summary>
        [DataMember]
        [Description("Indicates that the service provides simulation of another service.")]
        public const string Simulation = "http://schemas.microsoft.com/categories/robotics/simulation.html";

        /// <summary>
        /// Indicates this is a core Dss service
        /// </summary>
        [DataMember]
        [Description("Indicates that the service is a core DSS service.")]
        public const string DssInfrastructure = "http://schemas.microsoft.com/categories/dss/infrastructure.html";

        /// <summary>
        /// Indicates that the service is an internal service which should not be started directly.
        /// </summary>
        [DataMember]
        [Description("Indicates that the service is an internal service which should not be started directly.")]
        public const string Internal = "http://schemas.microsoft.com/categories/robotics/internal.html";

    }
}
