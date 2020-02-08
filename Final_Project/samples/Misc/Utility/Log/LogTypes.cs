//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: LogTypes.cs $ $Revision: 14 $
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Text;

using log = Microsoft.Robotics.Services.Sample.Log;
using Microsoft.Dss.ServiceModel.Dssp;
using Microsoft.Ccr.Core;
using Microsoft.Dss.Core.Attributes;
using W3C.Soap;
using System.ComponentModel;

namespace Microsoft.Robotics.Services.Sample.Log
{
    /// <summary>
    /// Log Service contract
    /// </summary>
    public static class Contract
    {
        /// <summary>
        /// Contract Identifier
        /// </summary>
        public const string Identifier = "http://schemas.microsoft.com/robotics/2006/08/log.user.html";
    }

    /// <summary>
    /// Log Service Operations PortSet
    /// </summary>
    [ServicePort]
    public class LogOperations : PortSet<DsspDefaultLookup, DsspDefaultDrop, LogInfo, LogWarning, LogError>
    {
    }

    /// <summary>
    /// LogInfo - Operation
    /// </summary>
    [DisplayName("(User) LogInfo")]
    [Description("Logs an information message to the console output service.")]
    public class LogInfo : Submit<LogInfoRequest, PortSet<DefaultSubmitResponseType, Fault>>
    {
    }

    /// <summary>
    /// LogWarning - Operation
    /// </summary>
    [DisplayName("(User) LogWarning")]
    [Description("Logs a warning message to the console output service.")]
    public class LogWarning : Submit<LogWarningRequest, PortSet<DefaultSubmitResponseType, Fault>>
    {
    }

    /// <summary>
    /// LogError - Operation
    /// </summary>
    [DisplayName("(User) LogError")]
    [Description("Logs an error message to the console output service.")]
    public class LogError : Submit<LogErrorRequest, PortSet<DefaultSubmitResponseType, Fault>>
    {
    }

    /// <summary>
    /// LogInfoRequest
    /// </summary>
    [DataContract]
    public class LogInfoRequest
    {
        private string _message;
        /// <summary>
        /// Message - The message to be logged
        /// </summary>
        [DataMember]
        [DataMemberConstructor(Order = 1)]
        [Description("Identifies the information message that you want to log.")]
        public string Message
        {
            get { return _message; }
            set { _message = value; }
        }

        private string _category;
        /// <summary>
        /// Category - Message Category (Activation, Console, Mount, etc.)
        /// </summary>
        [DataMember]
        [Description("Identifies the category of information message that you want to log.")]
        [DataMemberConstructor(Order = 2)]
        public string Category
        {
            get { return _category; }
            set { _category = value; }
        }
    }

    /// <summary>
    /// LogWarningRequest
    /// </summary>
    [DataContract]
    public class LogWarningRequest
    {
        private string _message;
        /// <summary>
        /// Message - The message to be logged
        /// </summary>
        [DataMember]
        [Description("Identifies the warning message that you want to log.")]
        [DataMemberConstructor(Order = 1)]
        public string Message
        {
            get { return _message; }
            set { _message = value; }
        }

        private string _category;
        /// <summary>
        /// Category - Message Category (Activation, Console, Mount, etc.)
        /// </summary>
        [DataMember]
        [DataMemberConstructor(Order = 2)]
        [Description("Identifies the category of warning message that you want to log.")]
        public string Category
        {
            get { return _category; }
            set { _category = value; }
        }
    }

    /// <summary>
    /// LogErrorRequest
    /// </summary>
    [DataContract]
    public class LogErrorRequest
    {
        private string _message;
        /// <summary>
        /// Message - The message to be logged
        /// </summary>
        [DataMember]
        [DataMemberConstructor(Order = 1)]
        [Description("Identifies the error message that you want to log.")]
        public string Message
        {
            get { return _message; }
            set { _message = value; }
        }

        private string _category;
        /// <summary>
        /// Category - Message Category (Activation, Console, Mount, etc.)
        /// </summary>
        [DataMember]
        [DataMemberConstructor(Order = 2)]
        [Description("Identifies the category of error message that you want to log.")]
        public string Category
        {
            get { return _category; }
            set { _category = value; }
        }
    }

}
