//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: SimulatedSonarTypes.cs $ $Revision: 3 $
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.Ccr.Core;
using soap = W3C.Soap;
using dssp = Microsoft.Dss.ServiceModel.Dssp;

namespace Microsoft.Robotics.Services.Simulation.Sensors.Sonar
{
    /// <summary>
    /// LaserRangeFinder Contract
    /// </summary>
    public static class Contract
    {
        /// <summary>
        /// LaserRangeFinder unique contract identifier 
        /// </summary>
        public const string Identifier = "http://schemas.microsoft.com/robotics/simulation/services/2006/05/simulatedsonar.user.html";
    }

}
