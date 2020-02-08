//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: SimulatedBumperTypes.cs $ $Revision: 11 $
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.Ccr.Core;
using soap = W3C.Soap;
using dssp = Microsoft.Dss.ServiceModel.Dssp;
using contactsensor = Microsoft.Robotics.Services.ContactSensor.Proxy;

namespace Microsoft.Robotics.Services.Simulation.Sensors.Bumper
{
    /// <summary>
    /// SimulatedBumperService Contract
    /// </summary>
    public static class Contract
    {
        /// <summary>
        /// SimulatedBumperService unique contract identifier 
        /// </summary>
        public const string Identifier = "http://schemas.microsoft.com/robotics/simulation/services/2006/05/simulatedbumper.user.html";
    }
}
