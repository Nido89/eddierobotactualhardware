//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: RoboticsTutorial2Types.cs $ $Revision: 12 $
//-----------------------------------------------------------------------

using Microsoft.Ccr.Core;
using Microsoft.Dss.Core.Attributes;
using Microsoft.Dss.ServiceModel.Dssp;

using System;
using System.Collections.Generic;
using W3C.Soap;

using tutorial2 = Microsoft.Robotics.Services.RoboticsTutorial2;

namespace Microsoft.Robotics.Services.RoboticsTutorial2
{

    public static class Contract
    {
        public const string Identifier = "http://schemas.microsoft.com/robotics/2006/06/roboticstutorial2.user.html";
    }

    #region CODECLIP 03-1
    [DataContract]
    public class RoboticsTutorial2State
    {
        [DataMember]
        public bool MotorOn;
    }
    #endregion

    [ServicePort]
    public class RoboticsTutorial2Operations : PortSet<DsspDefaultLookup, DsspDefaultDrop, Get, Replace>
    {
    }

    public class Get : Get<GetRequestType, PortSet<RoboticsTutorial2State, Fault>>
    {
    }

    public class Replace : Replace<RoboticsTutorial2State, PortSet<DefaultReplaceResponseType, Fault>>
    {
    }
}
