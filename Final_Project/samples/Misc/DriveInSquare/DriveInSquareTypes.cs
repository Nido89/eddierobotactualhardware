//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
// 
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: DriveInSquareTypes.cs $ $Revision: 3 $
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Ccr.Core;
using Microsoft.Dss.Core.Attributes;
using Microsoft.Dss.ServiceModel.Dssp;
using Microsoft.Dss.ServiceModel.DsspServiceBase;
using W3C.Soap;

namespace Microsoft.Robotics
{
    /// <summary>
    /// DriveInSquare contract class
    /// </summary>
    public sealed class Contract
    {
        /// <summary>
        /// DSS contract identifer for DriveInSquare
        /// </summary>
        [DataMember]
        public const string Identifier = "http://schemas.microsoft.com/robotics/2008/09/driveinsquare.user.html";
    }

    /// <summary>
    /// DriveInSquare state
    /// </summary>
    [DataContract]
    public class DriveInSquareState
    {
    }

    /// <summary>
    /// DriveInSquare main operations port
    /// </summary>
    [ServicePort]
    public class DriveInSquareOperations : PortSet<DsspDefaultLookup, DsspDefaultDrop, Get>
    {
    }

    /// <summary>
    /// DriveInSquare get operation
    /// </summary>
    public class Get : Get<GetRequestType, PortSet<DriveInSquareState, Fault>>
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
        public Get(GetRequestType body, PortSet<DriveInSquareState, Fault> responsePort)
            : base(body, responsePort)
        {
        }
    }
}


