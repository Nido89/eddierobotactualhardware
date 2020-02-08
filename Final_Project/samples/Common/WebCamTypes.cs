//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: WebCamTypes.cs $ $Revision: 20 $
//-----------------------------------------------------------------------

using Microsoft.Ccr.Core;
using Microsoft.Dss.Core.Attributes;
using Microsoft.Dss.Core.DsspHttp;
using Microsoft.Dss.ServiceModel.Dssp;
using Microsoft.Robotics.PhysicalModel;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;

using W3C.Soap;

namespace Microsoft.Robotics.Services.WebCam
{

    /// <summary>
    /// SimulatedWebcam Contract
    /// </summary>
    [DisplayName("(User) Generic WebCam")]
    [Description("Provides access to a webcam.")]
    [DssServiceDescription("http://msdn.microsoft.com/library/dd145259.aspx")]
    public static class Contract
    {
        /// The Unique Contract Identifier for the SimulatedWebcam service
        public const String Identifier = "http://schemas.microsoft.com/robotics/2006/05/webcamservice.html";
    }
    /// <summary>
    /// Generic WebCam State
    /// </summary>
    [DataContract]
    [Description("The webcam's state.")]
    public class WebCamState
    {
        private Vector2 _imageSize;
        private float _viewAngle;
        private Microsoft.Robotics.PhysicalModel.Pose _pose;
        private String _cameraName;
        private DateTime _lastFrameUpdate;

        /// <summary>
        /// Camera device name represented by this service
        /// </summary>
        [DataMember]
        [Description("Identifies the camera device name represented by this service.")]
        public String CameraDeviceName
        {
            get { return _cameraName; }
            set { _cameraName = value; }
        }
        /// <summary>
        /// Position and orientation of the camera
        /// </summary>
        [DataMember]
        [Description ("Specifies the position and orientation of the camera.")]
        public Microsoft.Robotics.PhysicalModel.Pose Pose
        {
            get { return _pose; }
            set { _pose = value; }
        }
        /// <summary>
        /// Image size in the X dimension
        /// </summary>
        [DataMember]
        [Description("Specifies the camera's image size.")]
        public Vector2 ImageSize
        {
            get { return _imageSize; }
            set { _imageSize = value; }
        }

        /// <summary>
        /// Field of View (FOV) angle for the camera
        /// </summary>
        [DataMember]
        [Description("Specifies the camera's Field Of View (FOV) angle.")]
        public float ViewAngle
        {
            get { return _viewAngle; }
            set { _viewAngle = value; }
        }

        double _quality;
        /// <summary>
        /// Compression quality level
        /// </summary>
        [DataMember]
        [Description("Specifies the camera's compression quality level setting.")]
        public double Quality
        {
            get { return _quality; }
            set { _quality = value; }
        }

        /// <summary>
        /// Timestamp of last frame capture
        /// </summary>
        [DataMember(XmlOmitDefaultValue = true)]
        [Browsable(false)]
        [Description("The time the last frame was captured.")]
        public DateTime LastFrameUpdate
        {
            get { return _lastFrameUpdate; }
            set { _lastFrameUpdate = value; }
        }

        private Bitmap _image;
        /// <summary>
        /// Raw image data of last frame captured
        /// </summary>
        [Description("The image data of the last frame captured.")]
        public Bitmap Image
        {
            get { return _image; }
            set { _image = value; }
        }


    }
    /// <summary>
    /// Request for latest frame
    /// </summary>
    [DataContract]
    public class QueryFrameRequest
    {
        /// <summary>
        /// format identifier
        /// </summary>
        [DataMember]
        [Description("Specifies the format of the image.")]
        public Guid Format;
        /// <summary>
        /// Image size
        /// </summary>
        [DataMember]
        [Description("Specifies the size of the image.")]
        public Vector2 Size;
    }
    /// <summary>
    /// Raw data of last frame captured
    /// </summary>
    [DataContract]
    public class QueryFrameResponse
    {
        /// <summary>
        /// Format identifier
        /// </summary>
        [DataMember]
        [Description("Specifies the format of the image.")]
        public Guid Format;
        /// <summary>
        /// Image size
        /// </summary>
        [DataMember]
        [Description("Specifies the size of the image.")]
        public Size Size;
        /// <summary>
        /// Raw bytes of captured frame
        /// </summary>
        [DataMember]
        [Description("Specifies the frame's raw data.")]
        public byte[] Frame;
        /// <summary>
        /// Timestamp of frame data
        /// </summary>
        [DataMember(XmlOmitDefaultValue = true)]
        [Description("Identifies the time of the frame data.")]
        public DateTime TimeStamp;
    }

    /// <summary>
    /// Frame update
    /// </summary>
    [DataContract]
    public class UpdateFrameRequest
    {
        /// <summary>
        /// Time when frame was captured
        /// </summary>
        [DataMember(XmlOmitDefaultValue = true)]
        [DataMemberConstructor(Order=1)]
        [Description("Specifies the time the frame was captured.")]
        public DateTime TimeStamp;

        /// <summary>
        /// Frame update
        /// </summary>
        public UpdateFrameRequest()
        {
        }
        /// <summary>
        /// Frame update
        /// </summary>
        public UpdateFrameRequest(System.DateTime timeStamp)
        {
            this.TimeStamp = timeStamp;
        }

    }

    /// <summary>
    /// Get operation
    /// </summary>
    [Description("Gets the current state of the webcam.")]
    public class Get : Get<GetRequestType, PortSet<WebCamState, Fault>>
    {
    }
    /// <summary>
    /// Replace operation
    /// </summary>
    [Description("Changes (or indicates a change to) the entire state of the webcam.")]
    [DisplayName("(User) WebCamReplace")]
    public class Replace : Replace<WebCamState, PortSet<DefaultReplaceResponseType, Fault>>
    {
    }
    /// <summary>
    /// Frame update operation
    /// </summary>
    [Description("Indicates when a new frame has been captured.")]
    public class UpdateFrame : Update<UpdateFrameRequest, PortSet<DefaultUpdateResponseType, Fault>>
    {
        /// <summary>
        /// Frame update operation
        /// </summary>
        public UpdateFrame()
        {
        }
        /// <summary>
        /// Frame update operation
        /// </summary>
        public UpdateFrame(UpdateFrameRequest body)
            :
                base(body)
        {
        }
        /// <summary>
        /// Frame update operation
        /// </summary>
        public UpdateFrame(UpdateFrameRequest body, PortSet<DefaultUpdateResponseType, Fault> responsePort)
            :
                base(body, responsePort)
        {
        }
    }

    /// <summary>
    /// Subscribe operation
    /// </summary>
    public class Subscribe : Subscribe<SubscribeRequestType, PortSet<SubscribeResponseType, Fault>>
    {
    }
    /// <summary>
    /// Request for latest frame
    /// </summary>
    [Description("Gets the most recently captured frame.")]
    public class QueryFrame : Query<QueryFrameRequest, PortSet<QueryFrameResponse, Fault>>
    {
    }

    /// <summary>
    /// WebCam operations port
    /// </summary>
    [ServicePort]
    public class WebCamOperations : PortSet
    {
        /// <summary>
        /// Default Constructor
        /// </summary>
        public WebCamOperations() : base(
            typeof(DsspDefaultLookup),
            typeof(DsspDefaultDrop),
            typeof(UpdateFrame),
            typeof(QueryFrame),
            typeof(Get),
            typeof(HttpGet),
            typeof(HttpQuery),
            typeof(Replace),
            typeof(Subscribe))
        {
        }

        /// <summary>
        /// Untyped post
        /// </summary>
        /// <param name="item"></param>
        public void Post(object item)
        {
            base.PostUnknownType(item);
        }

        /// <summary>
        /// Implicit conversion
        /// </summary>
        public static implicit operator Port<Get>(WebCamOperations portSet)
        {
            if (portSet == null) return null;
            return (Port<Get>)portSet[typeof(Get)];
        }
        /// <summary>
        /// Implicit conversion
        /// </summary>
        public static implicit operator Port<Replace>(WebCamOperations portSet)
        {
            if (portSet == null) return null;
            return (Port<Replace>)portSet[typeof(Replace)];
        }
        /// <summary>
        /// Implicit conversion
        /// </summary>
        public static implicit operator Port<UpdateFrame>(WebCamOperations portSet)
        {
            if (portSet == null) return null;
            return (Port<UpdateFrame>)portSet[typeof(UpdateFrame)];
        }
        /// <summary>
        /// Implicit conversion
        /// </summary>
        public static implicit operator Port<Subscribe>(WebCamOperations portSet)
        {
            if (portSet == null) return null;
            return (Port<Subscribe>)portSet[typeof(Subscribe)];
        }
        /// <summary>
        /// Implicit conversion
        /// </summary>
        public static implicit operator Port<QueryFrame>(WebCamOperations portSet)
        {
            if (portSet == null) return null;
            return (Port<QueryFrame>)portSet[typeof(QueryFrame)];
        }
        /// <summary>
        /// Implicit conversion
        /// </summary>
        public static implicit operator Port<DsspDefaultLookup>(WebCamOperations portSet)
        {
            if (portSet == null) return null;
            return (Port<DsspDefaultLookup>)portSet[typeof(DsspDefaultLookup)];
        }
        /// <summary>
        /// Implicit conversion
        /// </summary>
        public static implicit operator Port<DsspDefaultDrop>(WebCamOperations portSet)
        {
            if (portSet == null) return null;
            return (Port<DsspDefaultDrop>)portSet[typeof(DsspDefaultDrop)];
        }
        /// <summary>
        /// Implicit conversion
        /// </summary>
        public static implicit operator Port<HttpGet>(WebCamOperations portSet)
        {
            if (portSet == null) return null;
            return (Port<HttpGet>)portSet[typeof(HttpGet)];
        }
        /// <summary>
        /// Implicit conversion
        /// </summary>
        public static implicit operator Port<HttpQuery>(WebCamOperations portSet)
        {
            if (portSet == null) return null;
            return (Port<HttpQuery>)portSet[typeof(HttpQuery)];
        }
    }
}