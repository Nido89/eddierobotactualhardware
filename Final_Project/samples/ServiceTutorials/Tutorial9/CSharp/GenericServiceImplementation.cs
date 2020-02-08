//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: GenericServiceImplementation.cs $ $Revision: 7 $
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel;
using xml = System.Xml;

using Microsoft.Ccr.Core;
using Microsoft.Dss.Core;
using Microsoft.Dss.Core.Attributes;
using Microsoft.Dss.ServiceModel.Dssp;
using Microsoft.Dss.ServiceModel.DsspServiceBase;

using W3C.Soap;

#region Generic Contract Alias
using generic = ServiceTutorial8.Proxy;
#endregion

namespace ServiceTutorial9.Implementation
{
    #region Service Attributes
    /// <summary>
    /// This service provides an implementation of the generic service
    /// </summary>
    [Contract(Contract.Identifier)]
    [AlternateContract(generic.Contract.Identifier)]
    [DisplayName("(User) Service Tutorial 9: Generic Service Implementation")]
    [Description("This service implements the generic service provided in Service Tutorial 8.")]
    [DssServiceDescription("http://msdn.microsoft.com/library/bb727257.aspx")]
    public class ImplementationService : DsspServiceBase
    #endregion
    {
        #region Service Initialization
        // The state of this service is exactly that of the generic service
        [ServiceState]
        private generic.GenericState _state = new generic.GenericState();

        // The operations port is exactly that of the generic service
        [ServicePort("/GenericServiceImplementation", AllowMultipleInstances = false)]
        private generic.GenericServiceOperations _mainPort = new generic.GenericServiceOperations();

        /// <summary>
        /// Default Service Constructor
        /// </summary>
        public ImplementationService(DsspServiceCreationPort creationPort)
            :
                base(creationPort)
        {
        }
        #endregion

        #region Service Start
        /// <summary>
        /// Service Start
        /// </summary>
        protected override void Start()
        {
            // Add service specific initialization here.
            _state.FirstName = "John";
            _state.LastName = "Doe";

            base.Start();
        }
        #endregion

        #region Service Handlers
        /// <summary>
        /// Get Handler
        /// </summary>
        /// <param name="get"></param>
        /// <returns></returns>
        // Note that this service does not need to implement Get unless there are
        // some specific functions that it must perform
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public virtual IEnumerator<ITask> GetHandler(generic.Get get)
        {
            get.ResponsePort.Post(_state);
            yield break;
        }

        /// <summary>
        /// Replace Handler
        /// </summary>
        /// <param name="replace"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public virtual IEnumerator<ITask> ReplaceHandler(generic.Replace replace)
        {
            _state = replace.Body;
            replace.ResponsePort.Post(DefaultReplaceResponseType.Instance);
            yield break;
        }
        #endregion
    }
}
