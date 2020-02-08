//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: BlobTrackerCalibrateTypes.cs $ $Revision: 4 $
//-----------------------------------------------------------------------
using Microsoft.Ccr.Core;
using Microsoft.Dss.Core.Attributes;
using Microsoft.Dss.ServiceModel.Dssp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using W3C.Soap;
using blobtrackercalibrate = Microsoft.Robotics.Services.Sample.BlobTrackerCalibrate;


namespace Microsoft.Robotics.Services.Sample.BlobTrackerCalibrate
{

    /// <summary>
    /// BlobTrackerCalibrate Contract class
    /// </summary>
    public sealed class Contract
    {
        /// <summary>
        /// The Dss Service contract
        /// </summary>
        public const String Identifier = "http://schemas.microsoft.com/robotics/2007/04/blobtrackercalibrate.user.html";
    }
    /// <summary>
    /// The BlobTrackerCalibrate State
    /// </summary>
    [DataContract()]
    public class BlobTrackerCalibrateState
    {
        /// <summary>
        /// Indicates if the service is in the processing state.
        /// </summary>
        [DataMember]
        public bool Processing
        {
            get { return _processing; }
            set { _processing = value; }
        }
        private bool _processing;

        /// <summary>
        /// Indicates if the service is shut down.
        /// </summary>
        [DataMember]
        public bool Shutdown
        {
            get { return _shutdown; }
            set { _shutdown = value; }
        }
        private bool _shutdown;
    }

    /// <summary>
    /// Request type for updating the Processing state
    /// </summary>
    [DataContract]
    public class UpdateProcessingRequest
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public UpdateProcessingRequest()
        {
        }

        /// <summary>
        /// Creates a new UpdateProcessingRequest object.
        /// </summary>
        /// <param name="processing"></param>
        public UpdateProcessingRequest(bool processing)
        {
            _processing = processing;
        }

        /// <summary>
        /// Is processing?
        /// </summary>
        [DataMember, DataMemberConstructor]
        public bool Processing
        {
            get { return _processing; }
            set { _processing = value; }
        }
        private bool _processing;
    }

    /// <summary>
    /// BlobTrackerCalibrate Main Operations Port
    /// </summary>
    [ServicePort]
    public class BlobTrackerCalibrateOperations : PortSet<DsspDefaultLookup, DsspDefaultDrop, Get, UpdateProcessing>
    {
    }

    /// <summary>
    /// BlobTrackerCalibrate Get Operation
    /// </summary>
    [Description("Gets the current state of the training service.")]
    public class Get : Get<GetRequestType, PortSet<BlobTrackerCalibrateState, Fault>>
    {
    }

    /// <summary>
    /// BlobTrackerCalibrate UpdateProcessing Operation
    /// </summary>
    public class UpdateProcessing : Update<UpdateProcessingRequest, DsspResponsePort<DefaultUpdateResponseType>>
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public UpdateProcessing()
        {
        }

        /// <summary>
        /// Creates a new UpdateProcessing message with the specified body.
        /// </summary>
        /// <param name="processing"></param>
        public UpdateProcessing(bool processing)
            : base(new UpdateProcessingRequest(processing))
        {
        }
    }
}
