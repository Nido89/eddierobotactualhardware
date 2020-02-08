//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: SimulatedCompassTypes.cs $ $Revision: 3 $
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Ccr.Core;
using Microsoft.Dss.Core.Attributes;
using Microsoft.Dss.ServiceModel.Dssp;
using Microsoft.Dss.ServiceModel.DsspServiceBase;
using W3C.Soap;


namespace Microsoft.Robotics.Services.Simulation.Sensors.Compass
{
    /// <summary>
    /// SimulatedCompassService contract class
    /// </summary>
    public sealed class Contract
    {
        /// <summary>
        /// DSS contract identifer for SimulatedCompassService
        /// </summary>
        [DataMember]
        public const string Identifier = "http://schemas.microsoft.com/2008/11/simulatedcompass.user.html";
    }
}


