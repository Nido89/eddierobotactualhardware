//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: WebCamState.cs $ $Revision: 19 $
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Drawing;

using Microsoft.Dss.Core.Attributes;
using webcam = Microsoft.Robotics.Services.WebCam;
using System.ComponentModel;


namespace Microsoft.Robotics.Services.MultiDeviceWebCam
{

    /// <summary>
    /// WebCam service state
    /// </summary>
    [DataContract]
    [Description("The state of the webcam service.")]
    public class WebCamState : webcam.WebCamState
    {
        private List<CameraInstance> _cameras = new List<CameraInstance>();
        /// <summary>
        /// List of available cameras on the system.
        /// </summary>
        [DataMember(IsRequired = true), Browsable(false)]
        [Description("Specifies the set of available cameras.")]
        public List<CameraInstance> Cameras
        {
            get { return _cameras; }
            set { _cameras = value; }
        }

        private CameraInstance _selected;
        /// <summary>
        /// The currently selected camera
        /// </summary>
        [DataMember]
        [Description("Specifies the currently selected camera.")]
        public CameraInstance Selected
        {
            get { return _selected; }
            set { _selected = value; }
        }

        string _captureFile;
        /// <summary>
        /// The file, if any, to capture a stream of image to.
        /// If this is non-null then every image that the camera captures
        /// will be stored to this file.
        /// </summary>
        /// <remarks>Not supported on .Net CF</remarks>
        [DataMember]
        [Description("Specifies the file, if any, to capture a stream of image to.\n(If this is non-null then every image that the camera captures will be stored to this file.)\nNot supported on .Net CF.")]
        public string CaptureFile
        {
            get { return _captureFile; }
            set { _captureFile = value; }
        }

        /// <summary>
        /// When this is true, webcam only captures frames when requested.
        /// </summary>
        [DataMember, Browsable(true)]
        [Description("When this is true, webcam only captures frames when requested.")]
        public bool FramesOnDemand { get; set; }

        internal webcam.WebCamState ToGenericState()
        {
            webcam.WebCamState generic = new webcam.WebCamState();
            generic.CameraDeviceName = this.CameraDeviceName;
            generic.Image = this.Image;
            generic.ImageSize = this.ImageSize;
            generic.LastFrameUpdate = this.LastFrameUpdate;
            generic.Pose = this.Pose;
            generic.Quality = this.Quality;
            generic.ViewAngle = this.ViewAngle;

            return generic;
        }
    }

    /// <summary>
    /// Represents a camera on the system.
    /// </summary>
    [DataContract]
    public class CameraInstance
    {
        private string _friendlyName;
        /// <summary>
        /// Human readable name for the camera
        /// </summary>
        [DataMember]
        [Description("Specifies the user friendly name for the camera.")]
        public string FriendlyName
        {
            get { return _friendlyName; }
            set { _friendlyName = value; }
        }

        private string _devicePath;
        /// <summary>
        /// System internal device name for the camera
        /// </summary>
        [DataMember]
        [Description("Specifies system internal device name for the camera.")]
        public string DevicePath
        {
            get { return _devicePath; }
            set { _devicePath = value; }
        }

        internal bool Started { get; set; }

        private List<Format> _supportedFormats;
        /// <summary>
        /// List of image formats supported by this camera
        /// </summary>
        [DataMember, Browsable(false)]
        [Description("Specifies the set of images supported by the camera.")]
        public List<Format> SupportedFormats
        {
            get { return _supportedFormats; }
            set { _supportedFormats = value; }
        }

        private Format _format;
        /// <summary>
        /// The currently selected format used for capture.
        /// </summary>
        [DataMember]
        [Description("Specifies the currently selected format used for capture.")]
        public Format Format
        {
            get { return _format; }
            set { _format = value; }
        }
        
        /// <summary>
        /// Specifies if this camera instance is currently being used for capture
        /// </summary>
        [DataMember,Description("Specifies if this camera instance is currently being used for capture")]
        public bool InUse { get; set; }

        /// <summary>
        /// Tests whether this CameraInstance is equivalent to the supplied Camera object
        /// </summary>
        /// <param name="camera">Camera object to test against</param>
        /// <returns>true if the FriendlyName or DevicePath match.</returns>
        internal bool Equals(CameraInstance camera)
        {
            bool hasFriendlyName = !string.IsNullOrEmpty(_friendlyName);
            bool hasDevicePath = !string.IsNullOrEmpty(_devicePath);

            if (hasFriendlyName && hasDevicePath)
            {
                return _friendlyName == camera.FriendlyName &&
                    _devicePath == camera.DevicePath;
            }
            else if (hasDevicePath)
            {
                return _devicePath == camera.DevicePath;
            }
            else if (hasFriendlyName)
            {
                return _friendlyName == camera.FriendlyName;
            }
            return false;
        }

        internal bool IsValidFormat(Format format)
        {
            foreach (Format valid in _supportedFormats)
            {
                if (valid.Width == format.Width &&
                    valid.Height == format.Height &&
                    valid.Compression == format.Compression)
                {
                    return true;
                }
            }
            return false;
        }
    }

    /// <summary>
    /// Message body used by the UpdateDevice message
    /// </summary>
    [DataContract]
    public class UpdateDeviceRequest
    {
        private CameraInstance _selected = new CameraInstance();
        /// <summary>
        /// The camera to set as the currently selected capture device.
        /// </summary>
        [DataMember(IsRequired = true)]
        [Description("Specifies the currently selected camera.")]
        public CameraInstance Selected
        {
            get { return _selected; }
            set { _selected = value; }
        }
    }

}
