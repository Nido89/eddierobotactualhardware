//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: RoboticsTutorial1Types.cs $ $Revision: 12 $
//-----------------------------------------------------------------------

using Microsoft.Ccr.Core;
using Microsoft.Dss.Core.Attributes;
using Microsoft.Dss.ServiceModel.Dssp;

using System;
using System.Collections.Generic;
using W3C.Soap;

using tutorial1 = Microsoft.Robotics.Services.RoboticsTutorial1;

namespace Microsoft.Robotics.Services.RoboticsTutorial1
{

    public static class Contract
    {
        public const string Identifier = "http://schemas.microsoft.com/robotics/tutorials/2006/06/roboticstutorial1.user.html";
    }

    [DataContract]
    public class RoboticsTutorial1State
    {
    }

    [ServicePort]
    public class Tutorial1Operations : PortSet<DsspDefaultLookup, DsspDefaultDrop, Get>
    {
    }

    public class Get : Get<GetRequestType, PortSet<RoboticsTutorial1State, Fault>>
    {
    }
}
