//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: MotorTypes.cs $ $Revision: 18 $
//-----------------------------------------------------------------------

using Microsoft.Ccr.Core;
using Microsoft.Dss.Core.Attributes;
using Microsoft.Dss.Core.DsspHttp;
using Microsoft.Dss.ServiceModel.Dssp;

using System;
using System.Net;
using System.Collections.Generic;
using System.ComponentModel;

using W3C.Soap;

namespace Microsoft.Robotics.Services.Motor
{

    /// <summary>
    /// Dss Motor Contract
    /// </summary>
    [DisplayName("(User) Generic Motor")]
    [Description("Provides access to a motor.")]
    [DssServiceDescription("http://msdn.microsoft.com/library/dd145255.aspx")]
    public static class Contract
    {
        /// <summary>
        /// Motor contract
        /// </summary>
        public const string Identifier = "http://schemas.microsoft.com/robotics/2006/05/motor.html";
    }

    /// <summary>
    /// Motor Operations Port
    /// </summary>
    [ServicePort]
    public class MotorOperations : PortSet<
        DsspDefaultLookup,
        DsspDefaultDrop,
        Get,
        HttpGet,
        Replace,
        SetMotorPower>
    {
    }

    /// <summary>
    /// Operation Retrieve Motor State
    /// </summary>
    [Description("Gets the motor's current state.")]
    public class Get : Get<GetRequestType, PortSet<MotorState, Fault>>
    {
    }

    /// <summary>
    /// Operation Replace: Configures the motor
    /// </summary>
    [Description("Changes (or indicates a change to) the motor's entire state.")]
    [DisplayName("(User) MotorReplace")]
    public class Replace : Replace<MotorState, PortSet<DefaultReplaceResponseType, Fault>>
    {
    }

    /// <summary>
    /// Sets (or indicates a change to) the motor's power setting.
    /// </summary>
    [Description("Sets (or indicates a change to) the motor's power setting.")]
    public class SetMotorPower : Update<SetMotorPowerRequest, PortSet<DefaultUpdateResponseType, Fault>>
    {
        /// <summary>
        /// Sets (or indicates a change to) the motor's power setting.
        /// </summary>
        public SetMotorPower() { }

        /// <summary>
        /// Sets (or indicates a change to) the motor's power setting.
        /// </summary>
        /// <param name="targetPower"></param>
        public SetMotorPower(double targetPower)
        {
            this.Body = new SetMotorPowerRequest(targetPower);
        }
    }
}
