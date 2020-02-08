//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: ServiceTutorial2Types.cs $ $Revision: 4 $
//-----------------------------------------------------------------------

using Microsoft.Ccr.Core;
using Microsoft.Dss.Core.Attributes;
using Microsoft.Dss.Core.DsspHttp;
using Microsoft.Dss.ServiceModel.Dssp;

using System;
using System.Collections.Generic;
using W3C.Soap;

using serviceTutorial2 = RoboticsServiceTutorial2;


namespace RoboticsServiceTutorial2
{

    /// <summary>
    /// ServiceTutorial2 Contract class
    /// </summary>
    public sealed class Contract
    {
        /// <summary>
        /// The Dss Service contract
        /// </summary>
        public const String Identifier = "http://schemas.microsoft.com/2006/06/servicetutorial2.user.html";
    }

    /// <summary>
    /// The ServiceTutorial2 State
    /// </summary>
    [DataContract]
    public class ServiceTutorial2State
    {
        private string _member = "This is my State!";

        [DataMember]
        public string Member
        {
            get { return _member; }
            set { _member = value; }
        }

        #region CODECLIP 01-1
        private int _ticks;

        [DataMember]
        public int Ticks
        {
            get { return _ticks; }
            set { _ticks = value; }
        }
        #endregion
    }

    #region CODECLIP 01-3
    /// <summary>
    /// ServiceTutorial2 Main Operations Port
    /// </summary>
    [ServicePort]
    public class ServiceTutorial2Operations : PortSet<DsspDefaultLookup, DsspDefaultDrop, Get, HttpGet, Replace, IncrementTick>
    {
    }
    #endregion

    /// <summary>
    /// ServiceTutorial2 Get Operation
    /// </summary>
    public class Get : Get<GetRequestType, PortSet<ServiceTutorial2State, Fault>>
    {
        /// <summary>
        /// ServiceTutorial2 Get Operation
        /// </summary>
        public Get()
        {
        }

        /// <summary>
        /// ServiceTutorial2 Get Operation
        /// </summary>
        public Get(Microsoft.Dss.ServiceModel.Dssp.GetRequestType body) :
                base(body)
        {
        }

        /// <summary>
        /// ServiceTutorial2 Get Operation
        /// </summary>
        public Get(Microsoft.Dss.ServiceModel.Dssp.GetRequestType body, Microsoft.Ccr.Core.PortSet<ServiceTutorial2State,W3C.Soap.Fault> responsePort) :
                base(body, responsePort)
        {
        }
    }

    public class Replace : Replace<ServiceTutorial2State, PortSet<DefaultReplaceResponseType, Fault>>
    {
    }

    #region CODECLIP 01-2
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
    #endregion

}
