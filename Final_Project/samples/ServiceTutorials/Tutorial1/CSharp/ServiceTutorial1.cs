//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: ServiceTutorial1.cs $ $Revision: 6 $
//-----------------------------------------------------------------------

#region CODECLIP Appendix A-6
using Microsoft.Ccr.Core;
using Microsoft.Dss.Core;
using Microsoft.Dss.Core.Attributes;
#region CODECLIP 04-1
using Microsoft.Dss.Core.DsspHttp;
#endregion
using Microsoft.Dss.ServiceModel.Dssp;
using Microsoft.Dss.ServiceModel.DsspServiceBase;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml;
#endregion

using servicetutorial1 = RoboticsServiceTutorial1;


namespace RoboticsServiceTutorial1
{

    #region CODECLIP Appendix A-7
    /// <summary>
    /// Implementation class for ServiceTutorial1
    /// </summary>
    [DisplayName("(User) Service Tutorial 1: Creating a Service")]
    [Description("Demonstrates how to write a basic service.")]
    [Contract(Contract.Identifier)]
    [DssServiceDescription("http://msdn.microsoft.com/library/bb483064.aspx")]
    public class ServiceTutorial1Service : DsspServiceBase
    {
    #endregion
        #region CODECLIP Appendix A-8
        /// <summary>
        /// Service State
        /// </summary>
        [ServiceState]
        private ServiceTutorial1State _state = new ServiceTutorial1State();
        #endregion

        #region CODECLIP Appendix A-9
        /// <summary>
        /// Main Port
        /// </summary>
        [ServicePort("/servicetutorial1", AllowMultipleInstances=false)]
        private ServiceTutorial1Operations _mainPort = new ServiceTutorial1Operations();
        #endregion

        #region CODECLIP Appendix A-10
        /// <summary>
        /// Default Service Constructor
        /// </summary>
        public ServiceTutorial1Service(DsspServiceCreationPort creationPort) :
                base(creationPort)
        {
        }
        #endregion

        #region CODECLIP Appendix A-11
        /// <summary>
        /// Service Start
        /// </summary>
        protected override void Start()
        {
            base.Start();
            // Add service specific initialization here.
        }
        #endregion

        #region CODECLIP Appendix A-12
        /// <summary>
        /// Get Handler
        /// </summary>
        /// <param name="get"></param>
        /// <returns></returns>
        /// <remarks>This is a standard Get handler. It is not required because
        ///  DsspServiceBase will handle it if the state is marked with [ServiceState].
        ///  It is included here for illustration only.</remarks>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public virtual IEnumerator<ITask> GetHandler(Get get)
        {
            get.ResponsePort.Post(_state);
            yield break;
        }
        #endregion

        #region CODECLIP Appendix A-13
        /// <summary>
        /// Http Get Handler
        /// </summary>
        /// <remarks>This is a standard HttpGet handler. It is not required because
        ///  DsspServiceBase will handle it if the state is marked with [ServiceState].
        ///  It is included here for illustration only.</remarks>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public IEnumerator<ITask> HttpGetHandler(HttpGet httpGet)
        {
            httpGet.ResponsePort.Post(new HttpResponseType(_state));
            yield break;
        }
        #endregion

        #region CODECLIP Appendix A-14
        /// <summary>
        /// Replace Handler
        /// </summary>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public IEnumerator<ITask> ReplaceHandler(Replace replace)
        {
            _state = replace.Body;
            replace.ResponsePort.Post(DefaultReplaceResponseType.Instance);
            yield break;
        }
        #endregion
    }

}
