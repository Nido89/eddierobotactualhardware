//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
// 
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: ByteArray.cs $ $Revision: 14 $
//----------------------------------------------------------------------

using Microsoft.Ccr.Core;
using Microsoft.Dss.Core;
using Microsoft.Dss.Core.Attributes;
using Microsoft.Dss.ServiceModel.Dssp;
using Microsoft.Dss.ServiceModel.DsspServiceBase;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml;
using bytearray = Microsoft.Robotics.Services.Sample.ByteArray;


namespace Microsoft.Robotics.Services.Sample.ByteArray
{

    /// <summary>
    /// ByteArrayService - Creates and converts byte arrays
    /// </summary>
    [DisplayName("(User) Byte Array")]
    [Description("Provides conversion between byte arrays and byte lists.")]
    [Contract(Contract.Identifier)]
    [DssServiceDescription("http://msdn.microsoft.com/library/dd126860.aspx")]
    public class ByteArrayService : DsspServiceBase
    {
        /// <summary>
        /// _main Port
        /// </summary>
        [ServicePort("/bytearray", AllowMultipleInstances=true)]
        private ByteArrayOperations _mainPort = new ByteArrayOperations();

        [InitialStatePartner(Optional = true)]
        private ByteList _state = new ByteList();

        /// <summary>
        /// Default Service Constructor
        /// </summary>
        public ByteArrayService(DsspServiceCreationPort creationPort) :
                base(creationPort)
        {
        }

        /// <summary>
        /// Service Start
        /// </summary>
        protected override void Start()
        {
            if (_state == null)
            {
                _state = new ByteList(null);
            }
            base.Start();
        }

        /// <summary>
        /// Get a pre-initialized List of Bytes.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Description("Get a pre-initialized List of Bytes.")]
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public virtual IEnumerator<ITask> GetListHandler(GetList request)
        {
            request.ResponsePort.Post(_state);
            yield break;
        }


        /// <summary>
        /// Get a pre-initialized Byte Array.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Description("Get a pre-initialized byte array.")]
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public virtual IEnumerator<ITask> GetArrayHandler(GetArray request)
        {
            request.ResponsePort.Post(new ByteArray(_state.Data));
            yield break;
        }

        /// <summary>
        /// Convert a Byte Array to a List of Bytes.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Description("Convert a byte array to a list of bytes.")]
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public virtual IEnumerator<ITask> ByteArrayToListHandler(ByteArrayToList request)
        {
            request.ResponsePort.Post(new ByteList(request.Body.Data));
            yield break;
        }

        /// <summary>
        /// Convert a List of Bytes to a Byte Array.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Description("Convert a list of bytes to a byte array.")]
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public virtual IEnumerator<ITask> ByteListToArrayHandler(ByteListToArray request)
        {
            request.ResponsePort.Post(new ByteArray(request.Body.Data));
            yield break;
        }
    }
}
