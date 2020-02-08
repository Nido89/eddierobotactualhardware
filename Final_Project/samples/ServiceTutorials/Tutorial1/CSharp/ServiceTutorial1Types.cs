//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: ServiceTutorial1Types.cs $ $Revision: 5 $
//-----------------------------------------------------------------------

#region CODECLIP Appendix A-1
using Microsoft.Ccr.Core;
using Microsoft.Dss.Core.Attributes;
#region CODECLIP 03-1
using Microsoft.Dss.Core.DsspHttp;
#endregion
using Microsoft.Dss.ServiceModel.Dssp;

using System;
using System.Collections.Generic;
using W3C.Soap;

using servicetutorial1 = RoboticsServiceTutorial1;
#endregion


namespace RoboticsServiceTutorial1
{

    #region CODECLIP Appendix A-2
    /// <summary>
    /// ServiceTutorial1 Contract class
    /// </summary>
    public sealed class Contract
    {
        /// <summary>
        /// The Dss Service contract
        /// </summary>
        public const String Identifier = "http://schemas.microsoft.com/2006/06/servicetutorial1.user.html";
    }
    #endregion

    #region CODECLIP Appendix A-3
    /// <summary>
    /// The ServiceTutorial1 State
    /// </summary>
    [DataContract]
    public class ServiceTutorial1State
    {
        #region CODECLIP 03-2
        private string _member = "This is my State!";

        [DataMember]
        public string Member
        {
            get { return _member; }
            set { _member = value; }
        }
        #endregion
    }
    #endregion

    #region CODECLIP Appendix A-4
    /// <summary>
    /// ServiceTutorial1 Main Operations Port
    /// </summary>
    [ServicePort]
    public class ServiceTutorial1Operations : PortSet<DsspDefaultLookup, DsspDefaultDrop, Get, HttpGet, Replace>
    {
    }
    #endregion

    #region CODECLIP Appendix A-5
    /// <summary>
    /// ServiceTutorial1 Get Operation
    /// </summary>
    /// <remarks>All services require their own specific Get class because
    /// the service state is different for every service.</remarks>
    public class Get : Get<GetRequestType, PortSet<ServiceTutorial1State, Fault>>
    {
        /// <summary>
        /// ServiceTutorial1 Get Operation
        /// </summary>
        public Get()
        {
        }

        /// <summary>
        /// ServiceTutorial1 Get Operation
        /// </summary>
        public Get(Microsoft.Dss.ServiceModel.Dssp.GetRequestType body) :
                base(body)
        {
        }

        /// <summary>
        /// ServiceTutorial1 Get Operation
        /// </summary>
        public Get(Microsoft.Dss.ServiceModel.Dssp.GetRequestType body, Microsoft.Ccr.Core.PortSet<ServiceTutorial1State,W3C.Soap.Fault> responsePort) :
                base(body, responsePort)
        {
        }
    }

    #region CODECLIP 06-1
    /// <summary>
    /// ServiceTutorial1 Replace Operation
    /// </summary>
    /// <remarks>The Replace class is specific to a service because it uses
    /// the service state.</remarks>
    public class Replace : Replace<ServiceTutorial1State, PortSet<DefaultReplaceResponseType, Fault>>
    {
    }
    #endregion
    #endregion

}
