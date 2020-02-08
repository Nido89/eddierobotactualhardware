//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: JoystickFormTypes.cs $ $Revision: 3 $
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel;

using Microsoft.Ccr.Core;
using Microsoft.Dss.Core.Attributes;
using Microsoft.Dss.ServiceModel.Dssp;

using W3C.Soap;

namespace Microsoft.Robotics.Services.Sample.JoystickForm
{
    /// <summary>
    /// Joystick Form contract
    /// </summary>
    public sealed class Contract
    {
        /// <summary>
        /// Contract Identifier
        /// </summary>
        [DataMember]
        public const String Identifier = "http://schemas.microsoft.com/robotics/2007/08/joystickform.user.html";
    }
}
