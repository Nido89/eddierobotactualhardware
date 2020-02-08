//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
// 
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: ServiceTutorial10.cs $ $Revision: 5 $
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml;

using Microsoft.Ccr.Core;
using Microsoft.Dss.Core;
using Microsoft.Dss.Core.Attributes;
using Microsoft.Dss.ServiceModel.Dssp;
using Microsoft.Dss.ServiceModel.DsspServiceBase;

using W3C.Soap;

namespace ServiceTutorial10
{
    #region ServiceClass
    /// <summary>
    /// Implementation class for ServiceTutorial10
    /// </summary>
    /// <remarks>
    /// This tutorial shows how to document a service using several features.
    /// </remarks>
    [DisplayName("(User) Service Tutorial 10: Service Documentation")]
    [Description("This tutorial provides an example of how to document a service.")]
    [DssCategory(TutorialCategories.ServiceTutorial)]
    [Contract(Contract.Identifier)]
    [DssServiceDescription("http://msdn.microsoft.com/library/cc998489.aspx")]
    public class ServiceTutorial10Service : DsspServiceBase
    #endregion
    {
        /// <summary>
        /// Service State
        /// </summary>
        [ServiceState()]
        [InitialStatePartner(Optional = true, ServiceUri = ServicePaths.Store + "/ServiceTutorial10.xml")]
        private ServiceTutorial10State _state;
        
        [ServicePort("/servicetutorial10", AllowMultipleInstances=false)]
        private ServiceTutorial10Operations _mainPort = new ServiceTutorial10Operations();
        
        /// <summary>
        /// The default Service Constructor is called by the Constructor service 
        /// when creating a new instance of the service.
        /// </summary>
        public ServiceTutorial10Service(DsspServiceCreationPort creationPort) : 
                base(creationPort)
        {
        }
        
        /// <summary>
        /// The Service Start method is used to perform service initialization
        /// </summary>
        protected override void Start()
        {
			base.Start();
            if (_state == null)
            {
                // if state field is null, the configuration file was not present.
                // Set the state to a default initial value
                _state = new ServiceTutorial10State();
            }
        }
    }
}
