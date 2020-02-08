//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
// 
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: WaitForDriveCompletionTypes.cs $ $Revision: 4 $
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Ccr.Core;
using Microsoft.Dss.Core.Attributes;
using Microsoft.Dss.ServiceModel.Dssp;
using Microsoft.Dss.ServiceModel.DsspServiceBase;
using W3C.Soap;
using drive = Microsoft.Robotics.Services.Drive.Proxy;

namespace Microsoft.Dss.Services.Samples.WaitForDriveCompletion
{
    /// <summary>
    /// WaitForDriveCompletion contract class
    /// </summary>
    public sealed class Contract
    {
        /// <summary>
        /// DSS contract identifer for WaitForDriveCompletion
        /// </summary>
        [DataMember]
        public const string Identifier = "http://schemas.microsoft.com/robotics/2008/09/waitfordrivecompletion.user.html";
    }

    /// <summary>
    /// WaitForDriveCompletion state
    /// </summary>
    [DataContract]
    public class WaitForDriveCompletionState
    {
        private drive.DriveStage _lastStatus;

        /// <summary>
        /// LastStatus - Status of the last operation (NOT IMPLEMENTED)
        /// </summary>
        [DataMember]
        public drive.DriveStage LastStatus
        {
            get { return _lastStatus; }
            set { _lastStatus = value; }
        }
    }

    /// <summary>
    /// WaitForDriveCompletion main operations port
    /// </summary>
    [ServicePort]
    public class WaitForDriveCompletionOperations : PortSet<DsspDefaultLookup, DsspDefaultDrop, Get, Replace, Subscribe, Wait>
    {
    }

    /// <summary>
    /// WaitForDriveCompletion Get operation
    /// </summary>
    public class Get : Get<GetRequestType, PortSet<WaitForDriveCompletionState, Fault>>
    {
        /// <summary>
        /// Creates a new instance of Get
        /// </summary>
        public Get()
        {
        }

        /// <summary>
        /// Creates a new instance of Get
        /// </summary>
        /// <param name="body">the request message body</param>
        public Get(GetRequestType body)
            : base(body)
        {
        }

        /// <summary>
        /// Creates a new instance of Get
        /// </summary>
        /// <param name="body">the request message body</param>
        /// <param name="responsePort">the response port for the request</param>
        public Get(GetRequestType body, PortSet<WaitForDriveCompletionState, Fault> responsePort)
            : base(body, responsePort)
        {
        }
    }

    /// <summary>
    /// WaitForDriveCompletion Replace operation
    /// </summary>
    public class Replace : Replace<WaitForDriveCompletionState, PortSet<DefaultReplaceResponseType, Fault>>
    {
        /// <summary>
        /// Creates a new instance of Replace
        /// </summary>
        public Replace()
        {
        }

        /// <summary>
        /// Creates a new instance of Replace
        /// </summary>
        /// <param name="body">the request message body</param>
        public Replace(WaitForDriveCompletionState body)
            : base(body)
        {
        }

        /// <summary>
        /// Creates a new instance of Replace
        /// </summary>
        /// <param name="body">the request message body</param>
        /// <param name="responsePort">the response port for the request</param>
        public Replace(WaitForDriveCompletionState body, PortSet<DefaultReplaceResponseType, Fault> responsePort)
            : base(body, responsePort)
        {
        }
    }

    /// <summary>
    /// WaitForDriveCompletion subscribe operation
    /// </summary>
    public class Subscribe : Subscribe<SubscribeRequestType, PortSet<SubscribeResponseType, Fault>>
    {
        /// <summary>
        /// Creates a new instance of Subscribe
        /// </summary>
        public Subscribe()
        {
        }

        /// <summary>
        /// Creates a new instance of Subscribe
        /// </summary>
        /// <param name="body">the request message body</param>
        public Subscribe(SubscribeRequestType body)
            : base(body)
        {
        }

        /// <summary>
        /// Creates a new instance of Subscribe
        /// </summary>
        /// <param name="body">the request message body</param>
        /// <param name="responsePort">the response port for the request</param>
        public Subscribe(SubscribeRequestType body, PortSet<SubscribeResponseType, Fault> responsePort)
            : base(body, responsePort)
        {
        }
    }

    /// <summary>
    /// WaitForDriveCompletion get operation
    /// </summary>
    public class Wait : Submit<WaitRequestType, PortSet<WaitResponseType, Fault>>
    {
        /// <summary>
        /// Creates a new instance of Wait
        /// </summary>
        public Wait()
        {
        }

        /// <summary>
        /// Creates a new instance of Wait
        /// </summary>
        /// <param name="body">the request message body</param>
        public Wait(WaitRequestType body)
            : base(body)
        {
        }

        /// <summary>
        /// Creates a new instance of Wait
        /// </summary>
        /// <param name="body">the request message body</param>
        /// <param name="responsePort">the response port for the request</param>
        public Wait(WaitRequestType body, PortSet<WaitResponseType, Fault> responsePort)
            : base(body, responsePort)
        {
        }
    }

    /// <summary>
    /// WaitRequestType
    /// </summary>
    [DataContract]
    public class WaitRequestType
    {
        /// <summary>
        /// Default Constructor
        /// </summary>
        public WaitRequestType()
        {
        }

    }

    /// <summary>
    /// WaitResponseType
    /// </summary>
    [DataContract]
    [DataMemberConstructor]
    public class WaitResponseType
    {
        /// <summary>
        /// Default Constructor
        /// </summary>
        public WaitResponseType()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="result"></param>
        public WaitResponseType(drive.DriveStage result)
        {
            _driveStatus = result;
        }

        private drive.DriveStage _driveStatus;

        /// <summary>
        /// DriveStatus - Returns the DriveStage from the Differential Drive
        /// </summary>
        [DataMember]
        [DataMemberConstructor]
        public drive.DriveStage DriveStatus
        {
            get { return _driveStatus; }
            set { _driveStatus = value; }
        }
    }

}


