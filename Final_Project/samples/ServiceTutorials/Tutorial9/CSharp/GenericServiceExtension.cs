//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: GenericServiceExtension.cs $ $Revision: 7 $
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

using generic = ServiceTutorial8.Proxy;

namespace ServiceTutorial9.Extension
{
    /// <summary>
    /// This service implements a generic service but extends it with additional
    /// state and operations. The service exposes two operation ports: the extended
    /// service as well as the generic service which allows is to be used by clients
    /// that can talk one or both of these contracts.
    /// </summary>
    [Contract(Contract.Identifier)]
    [DisplayName("(User) Service Tutorial 9: Generic Service Extension")]
    [Description("This service extends the generic service provided in Service Tutorial 8.")]
    [DssServiceDescription("http://msdn.microsoft.com/library/bb727257.aspx")]
    public class ExtensionService : DsspServiceBase
    {
        #region Service Initialization
        // The state of this service is an extension of the generic service
        private ExtensionState _state = new ExtensionState();

        // The operations port is an extension of the generic service in that it supports UPDATE as well
        [ServicePort("/GenericServiceExtension", AllowMultipleInstances = false)]
        private GenericServiceExtensionOperations _mainPort = new GenericServiceExtensionOperations();

        // The alternate port where we only have the generic service operations (without UPDATE)
        [AlternateServicePort(AlternateContract = generic.Contract.Identifier)]
        private generic.GenericServiceOperations _altPort = new generic.GenericServiceOperations();
        #endregion

        /// <summary>
        /// Default Service Constructor
        /// </summary>
        public ExtensionService(DsspServiceCreationPort creationPort)
            :
                base(creationPort)
        {
        }

        /// <summary>
        /// Service Start
        /// </summary>
        protected override void Start()
        {
            // Add service specific initialization here.
            _state.FirstName = "John";
            _state.LastName = "Doe";
            _state.Age = 35;

            base.Start();
        }

        #region Extension Handlers
        /// <summary>
        /// Get Handler
        /// </summary>
        /// <param name="get"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public virtual IEnumerator<ITask> GetHandler(Get get)
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
        public virtual IEnumerator<ITask> ReplaceHandler(Replace replace)
        {
            _state = replace.Body;
            replace.ResponsePort.Post(DefaultReplaceResponseType.Instance);
            yield break;
        }

        /// <summary>
        /// Update Handler
        /// </summary>
        /// <param name="update"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public virtual IEnumerator<ITask> UpdateAgeHandler(UpdateAge update)
        {
            _state.Age = update.Body.Age;
            update.ResponsePort.Post(DefaultUpdateResponseType.Instance);
            yield break;
        }
        #endregion

        #region Generic Handlers
        /// <summary>
        /// Get Handler
        /// </summary>
        /// <param name="get"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent, PortFieldName = "_altPort")]
        public virtual IEnumerator<ITask> GenericGetHandler(generic.Get get)
        {
            get.ResponsePort.Post(_state.ToGenericState());
            yield break;
        }

        /// <summary>
        /// Replace Handler
        /// </summary>
        /// <param name="replace"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive, PortFieldName = "_altPort")]
        public virtual IEnumerator<ITask> GenericReplaceHandler(generic.Replace replace)
        {
            _state = new ExtensionState(replace.Body);
            replace.ResponsePort.Post(DefaultReplaceResponseType.Instance);
            yield break;
        }
        #endregion
    }
}
