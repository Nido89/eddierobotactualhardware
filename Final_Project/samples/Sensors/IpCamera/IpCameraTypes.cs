//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: IpCameraTypes.cs $ $Revision: 6 $
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel;

using Microsoft.Ccr.Core;
using Microsoft.Dss.Core.Attributes;
using Microsoft.Dss.Core.DsspHttp;
using Microsoft.Dss.ServiceModel.Dssp;

using W3C.Soap;

using webcam = Microsoft.Robotics.Services.WebCam.Proxy;
using System.Drawing;


namespace Microsoft.Robotics.Services.Sample.IpCamera
{
    /// <summary>
    /// Contract class
    /// </summary>
    public sealed class Contract
    {
        /// <summary>
        /// The Dss Service contract
        /// </summary>
        public const String Identifier = "http://schemas.microsoft.com/robotics/2007/02/ipcamera.user.html";
    }

    /// <summary>
    /// State class for Ip Camera service
    /// </summary>
    [DataContract]
    [Description("Specifies the IP camera state.")]
    public class IpCameraState : webcam.WebCamState
    {
        private Uri _cameraImageLocation;
        /// <summary>
        /// Specifies the location of the camera image.
        /// </summary>
        [DataMember]
        public Uri CameraImageLocation
        {
            get { return _cameraImageLocation; }
            set { _cameraImageLocation = value; }
        }

        private Bitmap _frame;
        /// <summary>
        /// Last frame captured from the camera
        /// </summary>
        public Bitmap Frame
        {
            get { return _frame; }
            set { _frame = value; }
        }
    }

    /// <summary>
    /// Ip Camera Operations Port
    /// </summary>
    [ServicePort]
    public class IpCameraOperations : PortSet<DsspDefaultLookup, DsspDefaultDrop, Get, Replace,
        webcam.UpdateFrame, webcam.QueryFrame, webcam.Subscribe, HttpGet, HttpQuery, HttpPost>
    {
    }

    /// <summary>
    /// Get operation
    /// </summary>
    [Description("Gets the state of the IP camera service.")]
    public class Get : Get<GetRequestType, PortSet<IpCameraState, Fault>>
    {
    }
    /// <summary>
    /// Replace operation
    /// </summary>
    [Description("Changes (or indicates a change to) the entire IP camera state.")]
    public class Replace : Replace<IpCameraState, DsspResponsePort<DefaultReplaceResponseType>>
    {
    }
}
