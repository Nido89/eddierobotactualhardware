//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: UrlLauncherTypes.cs $ $Revision: 11 $
//-----------------------------------------------------------------------
#if !URT_MINCLR
using Microsoft.Ccr.Core;
using Microsoft.Dss.Core.Attributes;
using Microsoft.Dss.ServiceModel.Dssp;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using W3C.Soap;


namespace Microsoft.Robotics.Services.Sample.UrlLauncher
{
    /// <summary>
    /// Timer Contract
    /// </summary>
    public static class Contract
    {
        /// The Unique Contract Identifier for the Timer service
        public const string Identifier = "http://schemas.microsoft.com/robotics/2006/10/urllauncher.user.html";
    }

    /// <summary>
    /// Url Launcher Sevice Operations PortSet
    /// </summary>
    [ServicePort]
    public class UrlLauncherOperations : PortSet<DsspDefaultLookup, DsspDefaultDrop, LaunchUrl>
    {
    }

    /// <summary>
    /// LaunchUrl - Operation
    /// </summary>
    [DisplayName("(User) LaunchURL")]
    [Description("Display a browser window for the specified URL.")]
    public class LaunchUrl : Submit<LaunchUrlRequest, PortSet<DefaultSubmitResponseType, Fault>>
    {
    }

    /// <summary>
    /// LaunchUrlRequest
    /// </summary>
    [DataContract]
    public class LaunchUrlRequest
    {
        private string _url;
        /// <summary>
        /// Url - The URL to open in the browser
        /// </summary>
        [DataMember]
        [Description("Specifies the URL to open.")]
        public string Url
        {
            get { return _url; }
            set { _url = value; }
        }
    }
}
#endif