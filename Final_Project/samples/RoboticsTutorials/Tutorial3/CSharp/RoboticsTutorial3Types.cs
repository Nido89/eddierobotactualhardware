//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: RoboticsTutorial3Types.cs $ $Revision: 13 $
//-----------------------------------------------------------------------

using Microsoft.Ccr.Core;
using Microsoft.Dss.Core.Attributes;
using Microsoft.Dss.ServiceModel.Dssp;
using System;
using System.Collections.Generic;

using W3C.Soap;
using tutorial3 = Microsoft.Robotics.Services.RoboticsTutorial3;

namespace Microsoft.Robotics.Services.RoboticsTutorial3
{

    public static class Contract
    {
        public const string Identifier = "http://schemas.microsoft.com/robotics/2006/06/roboticstutorial3.user.html";
    }

    [ServicePort]
    public class RoboticsTutorial3Operations : PortSet<DsspDefaultLookup, DsspDefaultDrop>
    {
    }
}
