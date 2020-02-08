//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: SimulatedWebcamTypes.cs $ $Revision: 8 $
//-----------------------------------------------------------------------
using Microsoft.Ccr.Core;
using Microsoft.Dss.Core.Attributes;
using Microsoft.Dss.Core.DsspHttp;
using Microsoft.Dss.ServiceModel.Dssp;

using System;
using System.Collections.Generic;
using W3C.Soap;

namespace Microsoft.Robotics.Services.Simulation.Sensors.SimulatedWebcam
{

    /// <summary>
    /// SimulatedWebcam Contract
    /// </summary>
    public static class Contract
    {
        /// The Unique Contract Identifier for the SimulatedWebcam service
        public const String Identifier = "http://schemas.microsoft.com/2006/09/simulatedwebcam.user.html";
    }
}
