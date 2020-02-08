
//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: PanTiltTypes.cs $ $Revision: 1 $
//-----------------------------------------------------------------------

namespace Microsoft.Robotics.Services.PanTilt
{
using Microsoft.Ccr.Core;
using Microsoft.Dss.Core.Attributes;
using Microsoft.Dss.Core.DsspHttp;
using Microsoft.Dss.ServiceModel.Dssp;

using System;
using System.Net;
using System.Collections.Generic;
using System.ComponentModel;

using W3C.Soap;

using saj = Microsoft.Robotics.Services.SingleAxisJoint;

    /// <summary>
    /// Pan/Tilt Contract
    /// </summary>
    [DisplayName("(User) Pan/Tilt Mechanism")]
    [Description("Provides access to a pan/tilt mechanism")]
    public static class Contract
    {
        /// The Unique Contract Identifier for the Sonar service
        public const string Identifier = "http://schemas.microsoft.com/robotics/2011/10/pantilt.html";
    }

    /// <summary>
    /// Pan/Tilt operations port.
    /// </summary>
    [ServicePort]
    public class PanTiltOperationsPort : PortSet<DsspDefaultLookup,
        DsspDefaultDrop,
        Get,
        Replace,
        Subscribe,
        Rotate,
        UpdateMotionBlocked,
        FindJointIndexPosition,
        SetHoldingTorque,
        StartTrajectory,
        SetPDGains,
        HttpPost>
    {
    }

    /// <summary>
    /// Pan/Tilt state.
    /// </summary>
    [DataContract]
    public class PanTiltState
    {
        /// <summary>
        /// State of the pan joint.
        /// </summary>
        [DataMember]
        public saj.SingleAxisJointState PanState { get; set; }

        /// <summary>
        /// State of the tilt joint.
        /// </summary>
        [DataMember]
        public saj.SingleAxisJointState TiltState { get; set; }
    }

    /// <summary>
    /// Get operation
    /// </summary>
    [DataContract]
    public class Get : Get<GetRequestType, PortSet<PanTiltState, Fault>>
    {
    }

    /// <summary>
    /// Replace operation
    /// </summary>
    [DataContract]
    public class Replace : Replace<PanTiltState, PortSet<DefaultReplaceResponseType, Fault>>
    {
    }

    /// <summary>
    /// Subscribe operation
    /// </summary>
    public class Subscribe : Subscribe<SubscribeRequestType, DsspResponsePort<SubscribeResponseType>>
    {
    }

    /// <summary>
    ///  Rotate message
    /// </summary>
    [DataContract]
    [DataMemberConstructor]
    public class RotateMessage
    {
        /// <summary>
        /// Gets or sets RotateSingleAxisRequest for pan joint
        /// </summary>
        [DataMember]
        public saj.RotateSingleAxisRequest RotatePanRequest { get; set; }

        /// <summary>
        /// Gets or sets RotateSingleAxisRequest for tilt joint
        /// </summary>
        [DataMember] 
        public saj.RotateSingleAxisRequest RotateTiltRequest { get; set; }
    }

    /// <summary>
    /// Rotate operation
    /// </summary>
    public class Rotate : Update<RotateMessage, PortSet<DefaultUpdateResponseType, Fault>>
    {
    }

    /// <summary>
    /// UpdateMotionBlocked message
    /// </summary>
    [DataContract]
    [DataMemberConstructor]
    public class UpdateMotionBlockedMessage
    {
        /// <summary>
        /// Gets or sets UpdateMotionBlockRequest for pan joint
        /// </summary>
        [DataMember]
        public saj.UpdateJointMotionBlockRequest PanMotionBlockedRequest {get; set;}

        /// <summary>
        /// Gets or sets UpdateMotionBlockRequest for pan joint
        /// </summary>
        [DataMember]
        public saj.UpdateJointMotionBlockRequest TiltMotionBlockedRequest {get;set;}
    }

    /// <summary>
    /// UpdateMotionBlocked operation
    /// </summary>
    public class UpdateMotionBlocked : Update<UpdateMotionBlockedMessage, PortSet<DefaultUpdateResponseType, Fault>>
    {
    }

    /// <summary>
    /// FindJointIndexPosition message
    /// </summary>
    [DataContract]
    [DataMemberConstructor]
    public class FindJointIndexPositionMessage
    {
        /// <summary>
        /// Gets or sets a value indicating whether to find the index position of the pan joint.
        /// </summary>
        [DataMember]
        public bool FindPanIndexPosition { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to find the index position of the tilt joint.
        /// </summary>
        [DataMember]
        public bool FindTiltIndexPosition { get; set; }
    }

    /// <summary>
    /// FindJointIndexPosition operation
    /// </summary>
    public class FindJointIndexPosition : Update<FindJointIndexPositionMessage, PortSet<DefaultUpdateResponseType, Fault>>
    {
    }

    /// <summary>
    /// SetHoldingTorque message
    /// </summary>
    [DataContract]
    [DataMemberConstructor]
    public class SetHoldingTorqueMessage
    {
        /// <summary>
        /// Gets or sets a value indicating whether to set the pan joint holding torque.
        /// </summary>
        [DataMember]
        public bool SetPanHolidngTorque { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to set the tilt joint holding torque.
        /// </summary>
        [DataMember]
        public bool SetTiltHoldingTorque { get; set; }
    }

    /// <summary>
    /// SetHoldingTorque operation.
    /// </summary>
    public class SetHoldingTorque : Update<SetHoldingTorqueMessage, PortSet<DefaultUpdateResponseType, Fault>>
    {
    }

    /// <summary>
    /// StartTrajectory message
    /// </summary>
    [DataContract]
    [DataMemberConstructor]
    public class StartTrajectoryMessage
    {
        /// <summary>
        /// Gets or sets pan trajectory.
        /// </summary>
        public saj.StartSingleJointTrajectoryRequest PanTrajectory { get; set; }

        /// <summary>
        /// Gets or sets tilt trajectory.
        /// </summary>
        public saj.StartSingleJointTrajectoryRequest TiltTrajectory { get; set; }
    }

    /// <summary>
    /// StartTrajectory operation
    /// </summary>
    public class StartTrajectory : Update<StartTrajectoryMessage, PortSet<DefaultUpdateResponseType, Fault>>
    {
    }

    /// <summary>
    /// SetPDGains message.
    /// </summary>
    [DataContract]
    [DataMemberConstructor]
    public class SetPDGainsMessage
    {
        /// <summary>
        /// Gets or sets the PD gains for the pan joint.
        /// </summary>
        [DataMember]
        public saj.SetPDGainsRequest PanPDGains { get; set; }

        /// <summary>
        /// Gets or sets the PD gains for the pan joint.
        /// </summary>
        [DataMember]
        public saj.SetPDGainsRequest TiltPDGains { get; set; }
    }

    /// <summary>
    /// SetPDGains operation.
    /// </summary>
    public class SetPDGains : Update<SetPDGainsMessage, PortSet<DefaultUpdateResponseType, Fault>>
    {
    }


}