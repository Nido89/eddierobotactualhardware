//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: PursuitCameraTypes.cs $ $Revision: 3 $
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel;

using Microsoft.Ccr.Core;
using Microsoft.Dss.Core.Attributes;
using Microsoft.Dss.ServiceModel.Dssp;

using W3C.Soap;

namespace Microsoft.Robotics.Services.PursuitCamera
{
    /// <summary>
    /// PursuitCamera Contract
    /// </summary>
    public sealed class Contract
    {
        /// <summary>
        /// PursuitCamera unique contract identifier
        /// </summary>
        [DataMember]
        public const String Identifier = "http://schemas.microsoft.com/robotics/2008/04/pursuitcamera.user.html";
    }
    
    /// <summary>
    /// Stores the state for the PursuitCamera
    /// </summary>
    [DataContract]
    public class PursuitCameraState : Settings
    {
        /// <summary>
        /// Name of the camera to move
        /// </summary>
        [DataMember, Browsable(true)]
        public string CameraName { get; set; }

        /// <summary>
        /// Accessor for PursuitCamera settings
        /// </summary>
        public Settings settings
        {
            get 
            {
                return new Settings
                {
                    MinDistance = this.MinDistance,
                    MaxDistance = this.MaxDistance,
                    Altitude = this.Altitude,
                    FieldOfView = this.FieldOfView,
                    OcclusionThreshold = this.OcclusionThreshold,
                    PreventOcclusion = this.PreventOcclusion
                };
            }
            set
            {
                MinDistance = value.MinDistance;
                MaxDistance = value.MaxDistance;
                FieldOfView = value.FieldOfView;
                Altitude = value.Altitude;
                OcclusionThreshold = value.OcclusionThreshold;
                PreventOcclusion = value.PreventOcclusion;
            }
        }
    }

    /// <summary>
    /// PursuitCamera service operations 
    /// </summary>
    [ServicePort]
    public class PursuitCameraOperations : PortSet<DsspDefaultLookup, DsspDefaultDrop, Get, ChangeSettings>
    {
    }
    
    /// <summary>
    /// Get message for PursuitCamera service
    /// </summary>
    public class Get : Get<GetRequestType, PortSet<PursuitCameraState, Fault>>
    {
    }

    /// <summary>
    /// Various settings used by the PursuitCamera
    /// </summary>
    [DataContract]
    public class Settings
    {
        /// <summary>
        /// Minimum distance to keep the camera from the entity
        /// </summary>
        [DataMember, Browsable(true)]
        public float MinDistance { get; set; }
        /// <summary>
        /// Maximum distance to keep the camera from the entity
        /// </summary>
        [DataMember, Browsable(true)]
        public float MaxDistance { get; set; }
        /// <summary>
        /// Height above the ground plane to keep the camera
        /// </summary>
        [DataMember, Browsable(true)]
        public float Altitude { get; set; }
        /// <summary>
        /// Distance from target point for occlusion to be considered
        /// </summary>
        [DataMember, Browsable(true)]
        public float OcclusionThreshold { get; set; }
        /// <summary>
        /// If true, try to prevent the view from the camera to the target from being occluded by obstacles
        /// </summary>
        [DataMember, Browsable(true)]
        public bool PreventOcclusion { get; set; }
        /// <summary>
        /// Camera vertical resolution in degrees
        /// </summary>
        [DataMember, Browsable(true)]
        public float FieldOfView { get; set; }
    }

    /// <summary>
    /// ChangeSettings message is used to update the settings in the PursuitCamera service
    /// </summary>
    public class ChangeSettings : Update<Settings, PortSet<DefaultUpdateResponseType, Fault>>
    {
    }
}
