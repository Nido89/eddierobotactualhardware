//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: UrlLauncher.cs $ $Revision: 13 $
//-----------------------------------------------------------------------
#if !URT_MINCLR
using Microsoft.Ccr.Core;
using Microsoft.Dss.Core;
using Microsoft.Dss.Core.Attributes;
using Microsoft.Dss.ServiceModel.Dssp;
using Microsoft.Dss.ServiceModel.DsspServiceBase;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Security.Permissions;
using xml = System.Xml;
using sm = Microsoft.Dss.Services.SubscriptionManager;
using System.Text;
using System.Diagnostics;
using W3C.Soap;


namespace Microsoft.Robotics.Services.Sample.UrlLauncher
{
    /// <summary>
    /// UrlLauncherService - Launches a web browser with specified URL
    /// </summary>
    [DisplayName("(User) URL Launcher")]
    [Description("Opens a specified URL in a browser window.")]
    [Contract(Contract.Identifier)]
    [DssServiceDescription("http://msdn.microsoft.com/library/dd126876.aspx")]
    public class UrlLauncherService : DsspServiceBase
    {
        [ServicePort("/urllauncher", AllowMultipleInstances = false)]
        private UrlLauncherOperations _mainPort = new UrlLauncherOperations();

        /// <summary>
        /// Default Service Constructor
        /// </summary>
        public UrlLauncherService(DsspServiceCreationPort creationPort) :
                base(creationPort)
        {
        }

        /// <summary>
        /// Service Start
        /// </summary>
        protected override void Start()
        {
            base.Start();
        }

        /// <summary>
        /// LaunchUrlHandler - Processes requests to display a URL in browser
        /// </summary>
        /// <param name="launchUrl"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public IEnumerator<ITask> LaunchUrlHandler(LaunchUrl launchUrl)
        {
            Uri baseUri = ServiceInfo.HttpServiceAlias;
            Uri startupUri = new Uri(baseUri, launchUrl.Body.Url);

            if (startupUri.Scheme == Uri.UriSchemeHttp ||
                startupUri.Scheme == Uri.UriSchemeHttps ||
                startupUri.Scheme == Uri.UriSchemeMailto)
            {
                Process process = Process.Start(startupUri.ToString());

                launchUrl.ResponsePort.Post(DefaultSubmitResponseType.Instance);
            }
            else
            {
                Fault fault = Fault.FromCodeSubcodeReason(
                    W3C.Soap.FaultCodes.Receiver,
                    DsspFaultCodes.ActionNotSupported,
                    "Only http, https and mailto URLs are permitted"
                );
                LogError(fault);
                launchUrl.ResponsePort.Post(fault);
            }

            yield break;
        }
    }
}
#endif