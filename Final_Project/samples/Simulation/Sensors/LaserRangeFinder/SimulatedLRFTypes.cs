//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: SimulatedLRFTypes.cs $ $Revision: 14 $
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.Ccr.Core;
using soap = W3C.Soap;
using dssp = Microsoft.Dss.ServiceModel.Dssp;
using sicklrf = Microsoft.Robotics.Services.Sensors.SickLRF.Proxy;

namespace Microsoft.Robotics.Services.Simulation.Sensors.LaserRangeFinder
{
    /// <summary>
    /// LaserRangeFinder Contract
    /// </summary>
    public static class Contract
    {
        /// <summary>
        /// LaserRangeFinder unique contract identifier 
        /// </summary>
        public const string Identifier = "http://schemas.microsoft.com/robotics/simulation/services/2006/05/simulatedlrf.user.html";
    }

}
