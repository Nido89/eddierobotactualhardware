//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
// 
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: ServiceTutorial10Types.cs $ $Revision: 5 $
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel;

using Microsoft.Ccr.Core;
using Microsoft.Dss.Core.Attributes;
using Microsoft.Dss.ServiceModel.Dssp;

using W3C.Soap;

namespace ServiceTutorial10
{
    /// <summary>
    /// ServiceTutorial10 Contract class
    /// </summary>
    public sealed class Contract
    {
        /// <summary>
        /// The Dss Service contract
        /// </summary>
        [DataMember()]
        public const String Identifier = "http://schemas.microsoft.com/2007/12/servicetutorial10.user.html";
    }

    #region State
    /// <summary>
    /// The ServiceTutorial10 State
    /// </summary>
    [DataContract()]
    [DisplayName("(User) Service Tutorial 10 State Document")]
    [Description("Specifies the state document for the service Service Tutorial 10.")]
    public class ServiceTutorial10State
    {
    #endregion
        string _firstName;
        string _initials;
        string _lastName;
        DateTime _lastModified;

        #region BrowsableProperty
        /// <summary>
        /// This property specifies the first name of a person. 
        /// </summary>
        [DataMember]
        [DisplayName("(User) First Name")]
        [Description("Specifies the first name of a person.")]
        public string FirstName
        {
          get { return _firstName; }
          set { _firstName = value; }
        }
        #endregion

        /// <summary>
        /// This property specifies any middle initials of a person. 
        /// </summary>
        [DataMember]
        [DisplayName("(User) Middle Initials")]
        [Description("Specifies any middle initials.")]
        public string Initials
        {
          get { return _initials; }
          set { _initials = value; }
        }

        /// <summary>
        /// This property specifies the last name of a person. 
        /// </summary>
        [DataMember]
        [DisplayName("(User) Last Name")]
        [Description("Specifies the last name of a person.")]
        public string LastName
        {
            get { return _lastName; }
            set { _lastName = value; }
        }

        #region NonBrowsableProperty
        /// <summary>
        /// This property specifies the last time this state was modified. 
        /// </summary>
        [DataMember]
        [DisplayName("(User) Last Modified Date")]
        [Description("Specifies the last date and time the state was modified.")]
        [Browsable(false)]
        public DateTime LastModified
        {
            get { return _lastModified; }
            set { _lastModified = value; }
        }
        #endregion
    }
    
    /// <summary>
    /// The ServiceTutorial10 Main Operations Port defines public operarations supported by this service
    /// </summary>
    [ServicePort()]
    public class ServiceTutorial10Operations : PortSet<DsspDefaultLookup, DsspDefaultDrop, Get>
    {
    }

    #region MessageOperation
    /// <summary>
    /// The ServiceTutorial10 Get Operation returns the current state of the service
    /// </summary>
    [DisplayName("(User) Get Service State")]
    [Description("Gets the current state of the service.")]
    public class Get : Get<GetRequestType, PortSet<ServiceTutorial10State, Fault>>
    {
    }
    #endregion

    #region Categories
    /// <summary>
    /// Categories present a mechanism for grouping together related services.
    /// </summary>
    [DataContract]
    [Description("Identifies a set of categories used to identify tutorials.")]
    public sealed class TutorialCategories
    {
        /// <summary>
        /// Indicates that the service is a DSS service tutorial.
        /// </summary>
        [DataMember]
        [Description("Indicates that the service is a DSS service tutorial.")]
        public const string ServiceTutorial = "http://schemas.microsoft.com/categories/dss/tutorial.html";

        /// <summary>
        /// Indicates that the service is a robotics tutorial.
        /// </summary>
        [Description("Indicates that the service is a robotics tutorial.")]
        [DataMember]
        public const string RoboticsTutorial = "http://schemas.microsoft.com/categories/robotics/tutorial.html";
    }
    #endregion
}
