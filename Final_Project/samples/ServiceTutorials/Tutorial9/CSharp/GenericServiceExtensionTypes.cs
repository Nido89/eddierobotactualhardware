//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: GenericServiceExtensionTypes.cs $ $Revision: 7 $
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel;

using Microsoft.Ccr.Core;
using Microsoft.Dss.Core.Attributes;
using Microsoft.Dss.Core.DsspHttp;
using Microsoft.Dss.ServiceModel.Dssp;

using W3C.Soap;

using generic = ServiceTutorial8.Proxy;

namespace ServiceTutorial9.Extension
{
    #region Service Contract
    /// <summary>
    /// Generic Service Extension Contract Identifier
    /// /// </summary>
    public static class Contract
    {
        public const string Identifier = "http://schemas.microsoft.com/2007/08/servicetutorial9/genericservice/extension.user.html";
    }
    #endregion

    /// <summary>
    /// State for the generic service extension
    /// </summary>
    #region State Definition
    [DataContract]
    [DisplayName("(User) Service Tutorial 9: Extension Service State")]
    [Description("This service state extends the generic service state provided in Service Tutorial 8.")]
    public class ExtensionState : generic.GenericState
    {
        int _age;

        [DataMember]
        [DisplayName("(User) Age")]
        [Description("Specifies the age of a person.")]
        public int Age
        {
            get { return _age; }
            set { _age = value; }
        }
    #endregion

        /// <summary>
        /// Default Constructor
        /// </summary>
        public ExtensionState()
        {
        }

        #region Type Conversion
        internal ExtensionState(generic.GenericState state)
        {
            this.FirstName = state.FirstName;
            this.LastName = state.LastName;
            this.Age = -1;
        }

        internal generic.GenericState ToGenericState()
        {
            generic.GenericState gen = new generic.GenericState();
            gen.FirstName = this.FirstName;
            gen.LastName = this.LastName;
            return gen;
        }
        #endregion
    }

    /// <summary>
    /// Wrapper for carrying the age field in an update request. The wrapper is 
    /// required because a request must be a complex type in order to be serializable.
    /// </summary>
    [DataContract]
    public class UpdateAgeRequest
    {
        [DataMember]
        [Description("Updated person age")]
        public int Age;
    }

    #region Service Operations
    /// <summary>
    /// Generic Service Extension Main Operations Port
    /// </summary>
    [ServicePort]
    public class GenericServiceExtensionOperations :
        PortSet<DsspDefaultLookup, DsspDefaultDrop, Get, Replace, UpdateAge>
    {
    }
    #endregion

    /// <summary>
    /// Get Operation
    /// </summary>
    [Description("Gets the current state.")]
    public class Get : Get<GetRequestType, PortSet<ExtensionState, Fault>>
    {
    }

    /// <summary>
    /// Replace Operation
    /// </summary>
    [Description("Replaces the current state.")]
    public class Replace : Replace<ExtensionState, PortSet<DefaultReplaceResponseType, Fault>>
    {
    }

    /// <summary>
    /// Get Operation
    /// </summary>
    [Description("Updates the age.")]
    public class UpdateAge : Update<UpdateAgeRequest, PortSet<DefaultUpdateResponseType, Fault>>
    {
    }

}
