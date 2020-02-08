//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: Log.cs $ $Revision: 14 $
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Dss.ServiceModel.DsspServiceBase;
using Microsoft.Dss.ServiceModel.Dssp;
using System.ComponentModel;
using System.Security;
using System.Security.Permissions;
using Microsoft.Dss.Core.Attributes;
using Microsoft.Ccr.Core;
using System.Xml;
using Microsoft.Dss.Core;

namespace Microsoft.Robotics.Services.Sample.Log
{
    /// <summary>
    /// LogService - Logs messages
    /// </summary>
    [DisplayName("(User) Log")]
    [Description("Provides a simple message logging support.")]
    [Contract(Contract.Identifier)]
    [DssServiceDescription("http://msdn.microsoft.com/library/dd126867.aspx")]
    public class LogService : DsspServiceBase
    {
        [ServicePort("log", AllowMultipleInstances = false)]
        LogOperations _mainPort = new LogOperations();

        /// <summary>
        /// Constructor for service
        /// </summary>
        /// <param name="creationPort"></param>
        public LogService(DsspServiceCreationPort creationPort) :
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
        /// LogInfoHandler - Processes LogInfo requests
        /// </summary>
        /// <param name="logInfo"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public virtual IEnumerator<ITask> LogInfoHandler(LogInfo logInfo)
        {
            LogInfoRequest request = logInfo.Body;

            if (string.IsNullOrEmpty(request.Category))
            {
                LogInfo(LogGroups.Console, request.Message);
            }
            else
            {
                LogInfo(
                    new XmlQualifiedName(request.Category, Contract.Identifier),
                    request.Message
                );
            }
            logInfo.ResponsePort.Post(DefaultSubmitResponseType.Instance);
            yield break;
        }

        /// <summary>
        /// LogWarningHandler - Processes LogWarning requests
        /// </summary>
        /// <param name="logWarning"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public virtual IEnumerator<ITask> LogWarningHandler(LogWarning logWarning)
        {
            LogWarningRequest request = logWarning.Body;

            if (string.IsNullOrEmpty(request.Category))
            {
                LogWarning(LogGroups.Console, request.Message);
            }
            else
            {
                LogWarning(
                    new XmlQualifiedName(request.Category, Contract.Identifier),
                    request.Message
                );
            }
            logWarning.ResponsePort.Post(DefaultSubmitResponseType.Instance);
            yield break;
        }

        /// <summary>
        /// LogErrorHandler - Processes LogError requests
        /// </summary>
        /// <param name="logError"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public virtual IEnumerator<ITask> LogErrorHandler(LogError logError)
        {
            LogErrorRequest request = logError.Body;

            if (string.IsNullOrEmpty(request.Category))
            {
                LogError(LogGroups.Console, request.Message);
            }
            else
            {
                LogError(
                    new XmlQualifiedName(request.Category, Contract.Identifier),
                    request.Message
                );
            }
            logError.ResponsePort.Post(DefaultSubmitResponseType.Instance);
            yield break;
        }
    }
}
