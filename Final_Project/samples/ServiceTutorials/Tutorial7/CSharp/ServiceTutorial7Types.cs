//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: ServiceTutorial7Types.cs $ $Revision: 6 $
//-----------------------------------------------------------------------

using Microsoft.Ccr.Core;
using Microsoft.Dss.Core.Attributes;
using Microsoft.Dss.Core.DsspHttp;
using Microsoft.Dss.ServiceModel.Dssp;

using System;
using System.Collections.Generic;
using W3C.Soap;

using servicetutorial7 = RoboticsServiceTutorial7;
using rst4 = RoboticsServiceTutorial4.Proxy;


namespace RoboticsServiceTutorial7
{

    /// <summary>
    /// ServiceTutorial7 Contract class
    /// </summary>
    public static class Contract
    {
        public const string Identifier = "http://schemas.microsoft.com/2006/06/servicetutorial7.user.html";
    }

    /// <summary>
    /// The ServiceTutorial7 State
    /// </summary>
    #region CODECLIP 03-1
    [DataContract]
    public class ServiceTutorial7State
    {
        private List<string> _clocks = new List<string>();

        [DataMember(IsRequired = true)]
        public List<string> Clocks
        {
            get { return _clocks; }
            set { _clocks = value; }
        }

        private List<TickCount> _tickCounts = new List<TickCount>();

        [DataMember(IsRequired = true)]
        public List<TickCount> TickCounts
        {
            get { return _tickCounts; }
            set { _tickCounts = value; }
        }
    }

    [DataContract]
    public class TickCount
    {
        public TickCount()
        {
        }

        public TickCount(int initial, string name)
        {
            _initial = initial;
            _count = initial;
            _name = name;
        }

        private string _name;
        [DataMember]
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        private int _count;
        [DataMember]
        public int Count
        {
            get { return _count; }
            set { _count = value; }
        }

        private int _initial;

        public int Initial
        {
            get { return _initial; }
            set { _initial = value; }
        }
    }
    #endregion

    /// <summary>
    /// ServiceTutorial7 Main Operations Port
    /// </summary>
    #region CODECLIP 03-2
    [ServicePort]
    public class ServiceTutorial7Operations : PortSet<
        DsspDefaultLookup,
        DsspDefaultDrop,
        Get,
        HttpGet,
        Replace,
        IncrementTick,
        SetTickCount>
    {
    }
    #endregion

    /// <summary>
    /// ServiceTutorial7 Get Operation
    /// </summary>
    public class Get : Get<GetRequestType, PortSet<ServiceTutorial7State, Fault>>
    {
        public Get()
        {
        }

        public Get(Microsoft.Dss.ServiceModel.Dssp.GetRequestType body) :
                base(body)
        {
        }

        public Get(Microsoft.Dss.ServiceModel.Dssp.GetRequestType body, Microsoft.Ccr.Core.PortSet<ServiceTutorial7State,W3C.Soap.Fault> responsePort) :
                base(body, responsePort)
        {
        }
    }

    public class Replace : Replace<ServiceTutorial7State, PortSet<DefaultReplaceResponseType, Fault>>
    {
    }

    #region CODECLIP 03-4
    public class SetTickCount : Update<TickCount, PortSet<DefaultUpdateResponseType, Fault>>
    {
        public SetTickCount()
        {
        }

        public SetTickCount(int tickCount, string source)
            : base(new TickCount(tickCount, source))
        {
        }
    }
    #endregion

    #region CODECLIP 03-3
    public class IncrementTick : Update<IncrementTickRequest, PortSet<DefaultUpdateResponseType, Fault>>
    {
        public IncrementTick()
        {
        }

        public IncrementTick(string source)
            : base(new IncrementTickRequest(source))
        {
        }
    }

    [DataContract]
    [DataMemberConstructor]
    public class IncrementTickRequest
    {
        public IncrementTickRequest()
        {
        }

        public IncrementTickRequest(string name)
        {
            _name = name;
        }

        private string _name;
        [DataMember]
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }
    }
    #endregion

}
