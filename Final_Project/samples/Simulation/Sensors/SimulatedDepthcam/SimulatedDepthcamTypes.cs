//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: SimulatedDepthcamTypes.cs $ $Revision: 2 $
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.Ccr.Core;
using soap = W3C.Soap;
using dssp = Microsoft.Dss.ServiceModel.Dssp;
using depthcam = Microsoft.Robotics.Services.DepthCamSensor;

namespace Microsoft.Robotics.Services.Simulation.Sensors.DepthCamera
{
    /// <summary>
    /// Contract
    /// </summary>
    public static class Contract
    {
        /// <summary>
        /// unique contract identifier 
        /// </summary>
        public const string Identifier = "http://schemas.microsoft.com/robotics/simulation/services/2011/01/simulateddepthcam.user.html";
    }

}
