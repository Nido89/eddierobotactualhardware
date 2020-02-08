//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: ServiceTutorial3Types.cs $ $Revision: 5 $
//-----------------------------------------------------------------------

using Microsoft.Ccr.Core;
using Microsoft.Dss.Core.Attributes;
using Microsoft.Dss.Core.DsspHttp;
using Microsoft.Dss.ServiceModel.Dssp;

using System;
using System.Collections.Generic;
using W3C.Soap;

using serviceTutorial3 = RoboticsServiceTutorial3;


namespace RoboticsServiceTutorial3
{

    /// <summary>
    /// ServiceTutorial3 Contract class
    /// </summary>
    public sealed class Contract
    {
        /// <summary>
        /// The Dss Service contract
        /// </summary>
        public const String Identifier = "http://schemas.microsoft.com/2006/06/servicetutorial3.user.html";
    }

    /// <summary>
    /// The ServiceTutorial3 State
    /// </summary>
    [DataContract]
    public class ServiceTutorial3State
    {
        private string _member = "This is my State!";

        [DataMember]
        public string Member
        {
            get { return _member; }
            set { _member = value; }
        }

        private int _ticks;

        [DataMember]
        public int Ticks
        {
            get { return _ticks; }
            set { _ticks = value; }
        }
    }

    /// <summary>
    /// ServiceTutorial3 Main Operations Port
    /// </summary>
    [ServicePort]
    public class ServiceTutorial3Operations : PortSet<DsspDefaultLookup, DsspDefaultDrop, Get, HttpGet, Replace, IncrementTick>
    {
    }

    /// <summary>
    /// ServiceTutorial3 Get Operation
    /// </summary>
    public class Get : Get<GetRequestType, PortSet<ServiceTutorial3State, Fault>>
    {
        /// <summary>
        /// ServiceTutorial3 Get Operation
        /// </summary>
        public Get()
        {
        }

        /// <summary>
        /// ServiceTutorial3 Get Operation
        /// </summary>
        public Get(Microsoft.Dss.ServiceModel.Dssp.GetRequestType body) :
                base(body)
        {
        }

        /// <summary>
        /// ServiceTutorial3 Get Operation
        /// </summary>
        public Get(Microsoft.Dss.ServiceModel.Dssp.GetRequestType body, Microsoft.Ccr.Core.PortSet<ServiceTutorial3State,W3C.Soap.Fault> responsePort) :
                base(body, responsePort)
        {
        }
    }

    public class Replace : Replace<ServiceTutorial3State, PortSet<DefaultReplaceResponseType, Fault>>
    {
    }

    public class IncrementTick : Update<IncrementTickRequest, PortSet<DefaultUpdateResponseType, Fault>>
    {
        public IncrementTick()
            : base(new IncrementTickRequest())
        {
        }
    }

    [DataContract]
    public class IncrementTickRequest
    {
    }

}
