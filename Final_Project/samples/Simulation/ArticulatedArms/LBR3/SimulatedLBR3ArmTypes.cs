//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: SimulatedLBR3ArmTypes.cs $ $Revision: 11 $
//-----------------------------------------------------------------------

using Microsoft.Ccr.Core;
using Microsoft.Dss.ServiceModel.Dssp;
using System;


namespace Microsoft.Robotics.Services.Simulation.LBR3Arm
{
    /// <summary>
    /// KUKA LBR3 Arm contract 
    /// </summary>
    public static class Contract
    {
        /// <summary>
        /// The unique contract identifier for the LBR3 Arm service
        /// </summary>
        public const string Identifier = "http://schemas.microsoft.com/robotics/simulation/services/2006/07/simulatedlbr3arm.user.html";
    }
}
