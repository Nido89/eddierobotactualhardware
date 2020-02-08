//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: Servicetutorial8Types.cs $ $Revision: 5 $
//-----------------------------------------------------------------------

using System;
using System.ComponentModel;

using Microsoft.Ccr.Core;
using Microsoft.Dss.Core.Attributes;
using Microsoft.Dss.Core.DsspHttp;
using Microsoft.Dss.ServiceModel.Dssp;

using W3C.Soap;

namespace ServiceTutorial8
{
    #region Service Contract
    /// <summary>
    /// Generic Service without service implementation
    /// </summary>
    [DisplayName("(User) Service Tutorial 8: Generic Service Contract")]
    [Description("This is a generic contract without an actual service implementation. See Service Tutorial 9 for various ways to use this contract.")]
    [DssServiceDescription("http://msdn.microsoft.com/library/bb727256.aspx")]
    public sealed class Contract
    {
        /// <summary>
        /// The Dss Service contract
        /// </summary>
        [DataMember()]
        public const String Identifier = "http://schemas.microsoft.com/2007/08/servicetutorial8.user.html";
    }
    #endregion

    #region Service State
    /// <summary>
    /// State for the generic service
    /// </summary>
    [DataContract]
    [DisplayName("(User) Generic Service State")]
    [Description("Specifies the state of the generic service.")]
    public class GenericState
    {
        string _firstName;
        string _lastName;

        [DataMember]
        [Description("Specifies the first name of a person.")]
        [DisplayName("(User) First Name")]
        public string FirstName
        {
            get { return _firstName; }
            set { _firstName = value; }
        }

        [DataMember]
        [DisplayName("(User) Last Name")]
        [Description("Specifies the last name of a person.")]
        public string LastName
        {
            get { return _lastName; }
            set { _lastName = value; }
        }
    }
    #endregion

    #region Service Operations
    /// <summary>
    /// Generic Service Main Operations Port
    /// </summary>
    [ServicePort]
    public class GenericServiceOperations :
        PortSet<DsspDefaultLookup, DsspDefaultDrop, Get, Replace>
    {
    }

    /// <summary>
    /// Get Operation
    /// </summary>
    [Description("Gets the current state.")]
    public class Get : Get<GetRequestType, PortSet<GenericState, Fault>>
    {
    }

    /// <summary>
    /// Replace Operation
    /// </summary>
    [Description("Replaces the current state.")]
    public class Replace : Replace<GenericState, PortSet<DefaultReplaceResponseType, Fault>>
    {
    }
    #endregion
}
