//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: SimulatedDifferentialDriveTypes.cs $ $Revision: 11 $
//-----------------------------------------------------------------------

using Microsoft.Ccr.Core;
using Microsoft.Dss.Core.Attributes;
using Microsoft.Dss.ServiceModel.Dssp;
using System;
using System.Collections.Generic;

using W3C.Soap;
using diffdrive = Microsoft.Robotics.Services.Drive.Proxy;

namespace Microsoft.Robotics.Services.Simulation.Drive
{
    /// <summary>
    /// SimulatedDifferentialDrive Contract
    /// </summary>
    public static class Contract
    {
        /// <summary>
        /// Unique SimulatedDifferentialDrive contract identifier
        /// </summary>
        public const string Identifier = "http://schemas.microsoft.com/robotics/simulation/services/2006/05/simulateddifferentialdrive.user.html";
    }

}
