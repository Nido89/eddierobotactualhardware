//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: ServiceTutorial6Types.cs $ $Revision: 5 $
//-----------------------------------------------------------------------

using Microsoft.Ccr.Core;
using Microsoft.Dss.Core.Attributes;
using Microsoft.Dss.Core.DsspHttp;
using Microsoft.Dss.ServiceModel.Dssp;

using System;
using System.Collections.Generic;
using W3C.Soap;

using ServiceTutorial6 = RoboticsServiceTutorial6;
using rst4 = RoboticsServiceTutorial4.Proxy;


namespace RoboticsServiceTutorial6
{

    /// <summary>
    /// ServiceTutorial6 Contract class
    /// </summary>
    public sealed class Contract
    {
        /// <summary>
        /// The Dss Service contract
        /// </summary>
        public const String Identifier = "http://schemas.microsoft.com/2006/06/servicetutorial6.user.html";
    }

    /// <summary>
    /// The ServiceTutorial6 State
    /// </summary>
    [DataContract]
    public class ServiceTutorial6State
    {
        #region CODECLIP 01-1
        private string _clock;

        [DataMember]
        public string Clock
        {
            get { return _clock; }
            set { _clock = value; }
        }

        private int _initialTicks;

        [DataMember]
        public int InitialTicks
        {
            get { return _initialTicks; }
            set { _initialTicks = value; }
        }
        #endregion

        private int _tickCount;

        [DataMember]
        public int TickCount
        {
            get { return _tickCount; }
            set { _tickCount = value; }
        }
    }

    /// <summary>
    /// ServiceTutorial6 Main Operations Port
    /// </summary>
    [ServicePort]
    public class ServiceTutorial6Operations : PortSet<
        DsspDefaultLookup,
        DsspDefaultDrop,
        Get,
        HttpGet,
        Replace,
        rst4.IncrementTick,
        SetTickCount>
    {
    }

    /// <summary>
    /// ServiceTutorial6 Get Operation
    /// </summary>
    public class Get : Get<GetRequestType, PortSet<ServiceTutorial6State, Fault>>
    {
        /// <summary>
        /// ServiceTutorial6 Get Operation
        /// </summary>
        public Get()
        {
        }

        /// <summary>
        /// ServiceTutorial6 Get Operation
        /// </summary>
        public Get(Microsoft.Dss.ServiceModel.Dssp.GetRequestType body) :
                base(body)
        {
        }

        /// <summary>
        /// ServiceTutorial6 Get Operation
        /// </summary>
        public Get(Microsoft.Dss.ServiceModel.Dssp.GetRequestType body, Microsoft.Ccr.Core.PortSet<ServiceTutorial6State,W3C.Soap.Fault> responsePort) :
                base(body, responsePort)
        {
        }
    }

    public class Replace : Replace<ServiceTutorial6State, PortSet<DefaultReplaceResponseType, Fault>>
    {
    }

    public class SetTickCount : Update<SetTickCountRequest, PortSet<DefaultUpdateResponseType, Fault>>
    {
        public SetTickCount()
        {
        }

        public SetTickCount(int tickCount)
            : base(new SetTickCountRequest(tickCount))
        {
        }
    }

    [DataContract]
    [DataMemberConstructor]
    public class SetTickCountRequest
    {
        public SetTickCountRequest()
        {
        }

        public SetTickCountRequest(int tickCount)
        {
            _tickCount = tickCount;
        }

        private int _tickCount;

        [DataMember]
        public int TickCount
        {
            get { return _tickCount;}
            set { _tickCount = value;}
        }
    }

}
