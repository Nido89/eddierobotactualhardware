//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: RoboticsTutorial4Types.cs $ $Revision: 17 $
//-----------------------------------------------------------------------
using Microsoft.Ccr.Core;
using Microsoft.Dss.Core.Attributes;
using Microsoft.Dss.ServiceModel.Dssp;

using System;
using System.Collections.Generic;
using W3C.Soap;

using tutorial4 = Microsoft.Robotics.Services.RoboticsTutorial4;

namespace Microsoft.Robotics.Services.RoboticsTutorial4
{

    public static class Contract
    {
        public const string Identifier = "http://schemas.microsoft.com/robotics/2006/06/roboticstutorial4.user.html";
    }

    [DataContract]
    public class RoboticsTutorial4State
    {
        private bool _motorEnabled;

        [DataMember]
        public bool MotorEnabled
        {
            get { return _motorEnabled; }
            set { _motorEnabled = value; }
        }

    }

    #region CODECLIP 01-1
    [ServicePort]
    public class RoboticsTutorial4Operations : PortSet<
        DsspDefaultLookup,
        DsspDefaultDrop,
        Get,
        Replace,
        Stop,
        Forward,
        Backward,
        TurnLeft,
        TurnRight>
    {
    }
    #endregion

    public class Get : Get<GetRequestType, PortSet<RoboticsTutorial4State, Fault>>
    {
    }

    public class Replace : Replace<RoboticsTutorial4State, PortSet<DefaultReplaceResponseType, Fault>>
    {
        public Replace()
        {
        }

        public Replace(RoboticsTutorial4State body)
            : base(body)
        {
        }
    }

    #region CODECLIP 01-2
    public class Stop : Submit<StopRequest, PortSet<DefaultSubmitResponseType, Fault>>
    {
        public Stop()
            : base(new StopRequest())
        {
        }
    }

    [DataContract]
    public class StopRequest { }
    #endregion

    public class Forward : Submit<ForwardRequest, PortSet<DefaultSubmitResponseType, Fault>>
    {
        public Forward()
            : base(new ForwardRequest())
        {
        }
    }

    [DataContract]
    public class ForwardRequest { }

    public class Backward : Submit<BackwardRequest, PortSet<DefaultSubmitResponseType, Fault>>
    {
        public Backward()
            : base(new BackwardRequest())
        {
        }
    }

    [DataContract]
    public class BackwardRequest { }

    public class TurnLeft : Submit<TurnLeftRequest, PortSet<DefaultSubmitResponseType, Fault>>
    {
        public TurnLeft()
            : base(new TurnLeftRequest())
        {
        }
    }

    [DataContract]
    public class TurnLeftRequest { }

    public class TurnRight : Submit<TurnRightRequest, PortSet<DefaultSubmitResponseType, Fault>>
    {
        public TurnRight()
            : base(new TurnRightRequest())
        {
        }
    }

    [DataContract]
    public class TurnRightRequest { }
}
