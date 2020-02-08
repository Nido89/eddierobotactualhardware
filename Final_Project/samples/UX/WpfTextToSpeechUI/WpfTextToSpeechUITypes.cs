//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
// 
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: WpfTextToSpeechUITypes.cs $ $Revision: 3 $
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Ccr.Core;
using Microsoft.Dss.Core.Attributes;
using Microsoft.Dss.ServiceModel.Dssp;
using Microsoft.Dss.ServiceModel.DsspServiceBase;
using W3C.Soap;

namespace Microsoft.Dss.Services.Samples.WpfTextToSpeechUI
{
    /// <summary>
    /// WpfTextToSpeechUI contract class
    /// </summary>
    public sealed class Contract
    {
        /// <summary>
        /// DSS contract identifer for WpfTextToSpeechUI
        /// </summary>
        [DataMember]
        public const string Identifier = "http://schemas.microsoft.com/2008/08/wpftexttospeechui.user.html";
    }

    /// <summary>
    /// WpfTextToSpeechUI state
    /// </summary>
    [DataContract]
    public class WpfTextToSpeechUIState
    {
    }

    /// <summary>
    /// WpfTextToSpeechUI main operations port
    /// </summary>
    [ServicePort]
    public class WpfTextToSpeechUIOperations : PortSet<DsspDefaultLookup, DsspDefaultDrop, Get>
    {
    }

    /// <summary>
    /// WpfTextToSpeechUI get operation
    /// </summary>
    public class Get : Get<GetRequestType, PortSet<WpfTextToSpeechUIState, Fault>>
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
        public Get(GetRequestType body, PortSet<WpfTextToSpeechUIState, Fault> responsePort)
            : base(body, responsePort)
        {
        }
    }
}


