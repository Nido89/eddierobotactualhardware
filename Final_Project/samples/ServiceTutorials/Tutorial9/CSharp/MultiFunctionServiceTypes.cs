//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: MultiFunctionServiceTypes.cs $ $Revision: 6 $
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel;

using Microsoft.Ccr.Core;
using Microsoft.Dss.Core.Attributes;
using Microsoft.Dss.ServiceModel.Dssp;

using W3C.Soap;

namespace ServiceTutorial9.Multi
{
    
    /// <summary>
    /// Generic Service Implementation Contract Identifier
    /// </summary>
    public sealed class Contract
    {
        /// The Unique Contract Identifier for the Service2 service
        [DataMember()]
        public const String Identifier = "http://schemas.microsoft.com/2007/08/servicetutorial9/genericservice/multi.user.html";
    }

    /// <summary>
    /// State for the "address" service
    /// </summary>
    [DataContract]
    [DisplayName("(User) Address Service State")]
    [Description("Specifies the state for the address service.")]
    public class AddressState
    {
        string _address;
        string _zipCode;

        [DataMember]
        [DisplayName("(User) Address")]
        [Description("Specifies the address or a person")]
        public string Address
        {
            get { return _address; }
            set { _address = value; }
        }

        [DataMember]
        [DisplayName("(User) Zip Code")]
        [Description("Specifies the zip code of a person.")]
        public string ZipCode
        {
            get { return _zipCode; }
            set { _zipCode = value; }
        }
    }

    /// <summary>
    /// Generic Service Main Operations Port
    /// </summary>
    [ServicePort]
    public class AddressServiceOperations :
        PortSet<DsspDefaultLookup, DsspDefaultDrop, Get, Replace>
    {
    }

    /// <summary>
    /// Get Operation
    /// </summary>
    [Description("Gets the current state.")]
    public class Get : Get<GetRequestType, PortSet<AddressState, Fault>>
    {
    }

    /// <summary>
    /// Replace Operation
    /// </summary>
    [Description("Replaces the current state.")]
    public class Replace : Replace<AddressState, PortSet<DefaultReplaceResponseType, Fault>>
    {
    }
}
