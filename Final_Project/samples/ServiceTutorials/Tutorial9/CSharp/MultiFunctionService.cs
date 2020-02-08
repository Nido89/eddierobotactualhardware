//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: MultiFunctionService.cs $ $Revision: 7 $
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

namespace ServiceTutorial9.Multi
{
    /// <summary>
    /// Service with two independent operations ports that from the outside
    /// operates as two independent services. This can be used to implement
    /// functionality that have tight internal dependencies but logically
    /// are best divided into multiple services.
    /// </summary>
    [Contract(Contract.Identifier)]
    [DisplayName("(User) Service Tutorial 9: Multi-headed Service Implementation")]
    [Description("This service is a multi-headed service with one being an implementation of the generic service provided in Service Tutorial 8.")]
    [DssServiceDescription("http://msdn.microsoft.com/library/bb727257.aspx")]
    public class MultiService : DsspServiceBase
    {
        #region Service Initialization
        // The first state of this multi headed service
        private AddressState _address = new AddressState();

        // The operations port used for the first service
        [ServicePort("/AddressService", AllowMultipleInstances = false)]
        private AddressServiceOperations _mainPort = new AddressServiceOperations();

        // The second state of this multi headed service
        private generic.GenericState _name = new generic.GenericState();

        // The alternate port used for the second service
        [AlternateServicePort(AlternateContract = generic.Contract.Identifier)]
        private generic.GenericServiceOperations _altPort = new generic.GenericServiceOperations();
        #endregion

        /// <summary>
        /// Default Service Constructor
        /// </summary>
        public MultiService(DsspServiceCreationPort creationPort)
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
            _address.Address = "One Microsoft Way, Redmond";
            _address.ZipCode = "98052";

            _name.FirstName = "John";
            _name.LastName = "Doe";

            base.Start();
        }

        #region Service Handlers
        
        // Service handlers for service one

        /// <summary>
        /// Get Handler
        /// </summary>
        /// <param name="get"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public virtual IEnumerator<ITask> GetAddressHandler(Get get)
        {
            get.ResponsePort.Post(_address);
            yield break;
        }

        /// <summary>
        /// Replace Handler
        /// </summary>
        /// <param name="replace"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public virtual IEnumerator<ITask> ReplaceAddressHandler(Replace replace)
        {
            _address = replace.Body;
            replace.ResponsePort.Post(DefaultReplaceResponseType.Instance);
            yield break;
        }

        // Service handlers for service two

        /// <summary>
        /// Get Handler
        /// </summary>
        /// <param name="get"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent, PortFieldName = "_altPort")]
        public virtual IEnumerator<ITask> GetNameHandler(generic.Get get)
        {
            get.ResponsePort.Post(_name);
            yield break;
        }

        /// <summary>
        /// Replace Handler
        /// </summary>
        /// <param name="replace"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive, PortFieldName = "_altPort")]
        public virtual IEnumerator<ITask> ReplaceNameHandler(generic.Replace replace)
        {
            _name = replace.Body;
            replace.ResponsePort.Post(DefaultReplaceResponseType.Instance);
            yield break;
        }
        #endregion
    }
}
