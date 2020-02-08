//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: ServiceTutorial5Types.cs $ $Revision: 5 $
//-----------------------------------------------------------------------

using Microsoft.Ccr.Core;
using Microsoft.Dss.Core.Attributes;
using Microsoft.Dss.Core.DsspHttp;
using Microsoft.Dss.ServiceModel.Dssp;

using System;
using System.Collections.Generic;
using W3C.Soap;

using serviceTutorial5 = RoboticsServiceTutorial5;
using rst4 = RoboticsServiceTutorial4.Proxy;


namespace RoboticsServiceTutorial5
{

    /// <summary>
    /// ServiceTutorial5 Contract class
    /// </summary>
    public sealed class Contract
    {
        /// <summary>
        /// The Dss Service contract
        /// </summary>
        public const String Identifier = "http://schemas.microsoft.com/2006/06/servicetutorial5.user.html";
    }

    /// <summary>
    /// The ServiceTutorial5 State
    /// </summary>
    [DataContract]
    public class ServiceTutorial5State
    {
        #region CODECLIP 04-1
        private int _tickCount;

        [DataMember]
        public int TickCount
        {
            get { return _tickCount; }
            set { _tickCount = value; }
        }
        #endregion
    }

    #region CODECLIP 04-2
    /// <summary>
    /// ServiceTutorial5 Main Operations Port
    /// </summary>
    [ServicePort]
    public class ServiceTutorial5Operations : PortSet<
        DsspDefaultLookup,
        DsspDefaultDrop,
        Get,
        HttpGet,
        Replace,
        rst4.IncrementTick,
        SetTickCount>
    {
    }
    #endregion

    /// <summary>
    /// ServiceTutorial5 Get Operation
    /// </summary>
    public class Get : Get<GetRequestType, PortSet<ServiceTutorial5State, Fault>>
    {
        /// <summary>
        /// ServiceTutorial5 Get Operation
        /// </summary>
        public Get()
        {
        }

        /// <summary>
        /// ServiceTutorial5 Get Operation
        /// </summary>
        public Get(Microsoft.Dss.ServiceModel.Dssp.GetRequestType body) :
                base(body)
        {
        }

        /// <summary>
        /// ServiceTutorial5 Get Operation
        /// </summary>
        public Get(Microsoft.Dss.ServiceModel.Dssp.GetRequestType body, Microsoft.Ccr.Core.PortSet<ServiceTutorial5State,W3C.Soap.Fault> responsePort) :
                base(body, responsePort)
        {
        }
    }

    public class Replace : Replace<ServiceTutorial5State, PortSet<DefaultReplaceResponseType, Fault>>
    {
    }

    #region CODECLIP 04-3
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
    #endregion
}
