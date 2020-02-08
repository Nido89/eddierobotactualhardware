//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: SimulatedFourByFourTypes.cs $ $Revision: 4 $
//-----------------------------------------------------------------------
using Microsoft.Ccr.Core;
using Microsoft.Dss.Core.Attributes;
using Microsoft.Dss.ServiceModel.Dssp;
using System;
using System.Collections.Generic;
using W3C.Soap;
using System.ComponentModel;


namespace Microsoft.Robotics.Services.Samples.SimulatedFourByFourDrive
{
    /// <summary>
    /// SimulatedFourByFourDrive service contract
    /// </summary>
    public sealed class Contract
    {
        /// <summary>
        /// The Dss Service contract
        /// </summary>
        public const String Identifier = "http://schemas.microsoft.com/robotics/2007/10/simulatedfourbyfourdrive.user.html";
    }

    /// <summary>
    /// SimulatedFourByFourDrive state
    /// </summary>
    [DataContract]
    public class SimulatedFourByFourState
    {
        private float _distanceBetweenWheels;
        /// <summary>
        /// The lateral distance between the wheels
        /// </summary>
        [DataMember]
        public float DistanceBetweenWheels
        {
            get { return _distanceBetweenWheels; }
            set { _distanceBetweenWheels = value; }
        }

        private float _wheelBase;
        /// <summary>
        /// The longitudinal distance between the front and rear axles
        /// </summary>
        [DataMember]
        public float WheelBase
        {
            get { return _wheelBase; }
            set { _wheelBase = value; }
        }

        private DriveRequest _driveRequest;
        /// <summary>
        /// The parameters of the last processed drive request
        /// </summary>
        [DataMember]
        public DriveRequest DriveRequest
        {
            get { return _driveRequest; }
            set { _driveRequest = value; }
        }
	
    }

    /// <summary>
    /// Used for issuing a drive request to the 4x4 service
    /// </summary>
    [DataContract]
    public class DriveRequest
    {
        private float _power;
        /// <summary>
        /// The power with which to drive, from -1 (full reverse) to +1 (full forwards)
        /// </summary>
        [DataMember, DataMemberConstructor]
        public float Power
        {
            get { return _power; }
            set { _power = value; }
        }

        private float _steeringAngle;
        /// <summary>
        /// The steering angle, in degrees, to turn. This is the angle of a tangent through the front axle center line.
        /// </summary>
        [DataMember, DataMemberConstructor]
        public float SteeringAngle
        {
            get { return _steeringAngle; }
            set { _steeringAngle = value; }
        }
    }

    /// <summary>
    /// Service operations for the SimulatedFourByFourDrive service
    /// </summary>
    [ServicePort]
    public class SimToy5Operations : PortSet<DsspDefaultLookup, DsspDefaultDrop, Get, Drive>
    {
    }

    /// <summary>
    /// Message for getting SimulatedFourByFourDrive service state
    /// </summary>
    public class Get : Get<GetRequestType, PortSet<SimulatedFourByFourState, Fault>>
    {
    }

    /// <summary>
    /// Message for driving SimulatedFourByFourDrive service 
    /// </summary>
    public class Drive : Update<DriveRequest, PortSet<DefaultUpdateResponseType, Fault>>
    {
    }
}
