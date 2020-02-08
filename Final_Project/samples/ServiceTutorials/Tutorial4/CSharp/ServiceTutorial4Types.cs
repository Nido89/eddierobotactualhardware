//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: ServiceTutorial4Types.cs $ $Revision: 4 $
//-----------------------------------------------------------------------

using Microsoft.Ccr.Core;
using Microsoft.Dss.Core.Attributes;
using Microsoft.Dss.Core.DsspHttp;
using Microsoft.Dss.ServiceModel.Dssp;

using System;
using System.Collections.Generic;
using W3C.Soap;

using serviceTutorial4 = RoboticsServiceTutorial4;


namespace RoboticsServiceTutorial4
{

    /// <summary>
    /// ServiceTutorial4 Contract class
    /// </summary>
    public sealed class Contract
    {
        /// <summary>
        /// The Dss Service contract
        /// </summary>
        public const String Identifier = "http://schemas.microsoft.com/2006/06/servicetutorial4.user.html";
    }

    /// <summary>
    /// The ServiceTutorial4 State
    /// </summary>
    [DataContract]
    public class ServiceTutorial4State
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

    #region CODECLIP 01-2
    /// <summary>
    /// ServiceTutorial4 Main Operations Port
    /// </summary>
    [ServicePort]
    public class ServiceTutorial4Operations : PortSet<
        DsspDefaultLookup,
        DsspDefaultDrop,
        Get,
        HttpGet,
        Replace,
        IncrementTick,
        Subscribe>
    {
    }
    #endregion

    /// <summary>
    /// ServiceTutorial4 Get Operation
    /// </summary>
    public class Get : Get<GetRequestType, PortSet<ServiceTutorial4State, Fault>>
    {
        /// <summary>
        /// ServiceTutorial4 Get Operation
        /// </summary>
        public Get()
        {
        }

        /// <summary>
        /// ServiceTutorial4 Get Operation
        /// </summary>
        public Get(Microsoft.Dss.ServiceModel.Dssp.GetRequestType body) :
                base(body)
        {
        }

        /// <summary>
        /// ServiceTutorial4 Get Operation
        /// </summary>
        public Get(Microsoft.Dss.ServiceModel.Dssp.GetRequestType body, Microsoft.Ccr.Core.PortSet<ServiceTutorial4State,W3C.Soap.Fault> responsePort) :
                base(body, responsePort)
        {
        }
    }

    public class Replace : Replace<ServiceTutorial4State, PortSet<DefaultReplaceResponseType, Fault>>
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

    #region CODECLIP 01-1
    public class Subscribe : Subscribe<SubscribeRequestType, PortSet<SubscribeResponseType, Fault>>
    {
    }
    #endregion
}
