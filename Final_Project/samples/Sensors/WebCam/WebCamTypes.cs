//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: WebCamTypes.cs $ $Revision: 22 $
//-----------------------------------------------------------------------
using Microsoft.Ccr.Core;
using Microsoft.Dss.Core.Attributes;
using Microsoft.Dss.ServiceModel.Dssp;
using System;
using System.Collections.Generic;

using W3C.Soap;
using Microsoft.Dss.Core.DsspHttp;

using webcam = Microsoft.Robotics.Services.WebCam;
using System.ComponentModel;

namespace Microsoft.Robotics.Services.MultiDeviceWebCam
{

    /// <summary>
    /// Webcam service contract
    /// </summary>
    public static class Contract
    {
        /// <summary>
        /// Webcam service contract identifier.
        /// </summary>
        public const string Identifier = "http://schemas.microsoft.com/robotics/2006/05/multidevicewebcamservice.user.html";
    }

    /// <summary>
    /// Webcam service operations port
    /// </summary>
    [ServicePort]
    public class WebCamServiceOperations : PortSet
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public WebCamServiceOperations()
            : base(
                typeof(DsspDefaultLookup),
                typeof(DsspDefaultDrop),
                typeof(webcam.QueryFrame),
                typeof(UpdateFormat),
                typeof(webcam.UpdateFrame),
                typeof(webcam.Subscribe),
                typeof(Get),
                typeof(Replace),
                typeof(UpdateDevice),
                typeof(HttpGet),
                typeof(HttpPost),
                typeof(HttpQuery)
            )
        {
        }

        /// <summary>
        /// Post a message
        /// </summary>
        /// <param name="item">message to post to the port</param>
        public void Post(object item) { base.PostUnknownType(item); }

        /// <summary>
        /// Default accessor for DsspDefaultLookup message port
        /// </summary>
        /// <param name="portSet">webcam service operations port</param>
        /// <returns>DesspDefaultLookup message port</returns>
        public static implicit operator Port<DsspDefaultLookup>(WebCamServiceOperations portSet)
        {
            if (portSet == null)
            {
                return null;
            }
            return (Port<DsspDefaultLookup>)portSet[typeof(DsspDefaultLookup)];
        }

        /// <summary>
        /// Default accessor for DsspDefaultDrop message port
        /// </summary>
        /// <param name="portSet">WebCam Service Operations port</param>
        /// <returns>DsspDefaultDrop message port</returns>
        public static implicit operator Port<DsspDefaultDrop>(WebCamServiceOperations portSet)
        {
            if (portSet == null)
            {
                return null;
            }
            return (Port<DsspDefaultDrop>)portSet[typeof(DsspDefaultDrop)];
        }

        /// <summary>
        /// Default accessor for webcam.QueryFrame message port
        /// </summary>
        /// <param name="portSet">WebCam Service Operations port</param>
        /// <returns>webcam.QueryFrame message port</returns>
        public static implicit operator Port<webcam.QueryFrame>(WebCamServiceOperations portSet)
        {
            if (portSet == null)
            {
                return null;
            }
            return (Port<webcam.QueryFrame>)portSet[typeof(webcam.QueryFrame)];
        }

        /// <summary>
        /// Default accessor for UpdateFormat message port
        /// </summary>
        /// <param name="portSet">WebCam Service Operations port</param>
        /// <returns>UpdateFormat message port</returns>
        public static implicit operator Port<UpdateFormat>(WebCamServiceOperations portSet)
        {
            if (portSet == null)
            {
                return null;
            }
            return (Port<UpdateFormat>)portSet[typeof(UpdateFormat)];
        }

        /// <summary>
        /// Default accessor for webcam.UpdateFrame message port
        /// </summary>
        /// <param name="portSet">WebCam Service Operations port</param>
        /// <returns>webcam.UpdateFrame message port</returns>
        public static implicit operator Port<webcam.UpdateFrame>(WebCamServiceOperations portSet)
        {
            if (portSet == null)
            {
                return null;
            }
            return (Port<webcam.UpdateFrame>)portSet[typeof(webcam.UpdateFrame)];
        }

        /// <summary>
        /// Default accessor for webcam.Subscribe message port
        /// </summary>
        /// <param name="portSet">WebCam Service Operations port</param>
        /// <returns>webcam.Subscribe message port</returns>
        public static implicit operator Port<webcam.Subscribe>(WebCamServiceOperations portSet)
        {
            if (portSet == null)
            {
                return null;
            }
            return (Port<webcam.Subscribe>)portSet[typeof(webcam.Subscribe)];
        }

        /// <summary>
        /// Default accessor for Get message port
        /// </summary>
        /// <param name="portSet">WebCam Service Operations port</param>
        /// <returns>Get message port</returns>
        public static implicit operator Port<Get>(WebCamServiceOperations portSet)
        {
            if (portSet == null)
            {
                return null;
            }
            return (Port<Get>)portSet[typeof(Get)];
        }

        /// <summary>
        /// Default accessor for Replace message port
        /// </summary>
        /// <param name="portSet">WebCam Service Operations port</param>
        /// <returns>Replace message port</returns>
        public static implicit operator Port<Replace>(WebCamServiceOperations portSet)
        {
            if (portSet == null)
            {
                return null;
            }
            return (Port<Replace>)portSet[typeof(Replace)];
        }

        /// <summary>
        /// Default accessor for UpdateDevice message port
        /// </summary>
        /// <param name="portSet">WebCam Service Operations port</param>
        /// <returns>UpdateDevice message port</returns>
        public static implicit operator Port<UpdateDevice>(WebCamServiceOperations portSet)
        {
            if (portSet == null)
            {
                return null;
            }
            return (Port<UpdateDevice>)portSet[typeof(UpdateDevice)];
        }

        /// <summary>
        /// Default accessor for HttpGet message port
        /// </summary>
        /// <param name="portSet">WebCam Service Operations port</param>
        /// <returns>HttpGet message port</returns>
        public static implicit operator Port<HttpGet>(WebCamServiceOperations portSet)
        {
            if (portSet == null)
            {
                return null;
            }
            return (Port<HttpGet>)portSet[typeof(HttpGet)];
        }

        /// <summary>
        /// Default accessor for HttpPost message port
        /// </summary>
        /// <param name="portSet">WebCam Service Operations port</param>
        /// <returns>HttpPost message port</returns>
        public static implicit operator Port<HttpPost>(WebCamServiceOperations portSet)
        {
            if (portSet == null)
            {
                return null;
            }
            return (Port<HttpPost>)portSet[typeof(HttpPost)];
        }

        /// <summary>
        /// Default accessor for HttpQuery message port
        /// </summary>
        /// <param name="portSet">WebCam Service Operations port</param>
        /// <returns>HttpQuery message port</returns>
        public static implicit operator Port<HttpQuery>(WebCamServiceOperations portSet)
        {
            if (portSet == null)
            {
                return null;
            }
            return (Port<HttpQuery>)portSet[typeof(HttpQuery)];
        }
    }

    /// <summary>
    /// Format description
    /// </summary>
    [DataContract]
    public class Format
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public Format() { }

        internal Format(int width, int height, int maxFrameRate, int minFrameRate, uint compression)
        {
            _width = width;
            _height = height;
            _minFramesPerSecond = minFrameRate;
            _maxFramesPerSecond = maxFrameRate;

            _compression = string.Empty;
            uint convert = compression;
            while (convert > 0)
            {
                char c = (char)(convert & 0xFF);
                if (c == '\0')
                {
                    break;
                }
                _compression += c;
                convert >>= 8;
            }
        }

        private int _width;
        /// <summary>
        /// Image width
        /// </summary>
        [DataMember]
        [Description("Specifies the image width.")]
        public int Width
        {
            get { return _width; }
            set { _width = value; }
        }

        private int _height;
        /// <summary>
        /// Image height
        /// </summary>
        [DataMember]
        [Description("Specifies the image height.")]
        public int Height
        {
            get { return _height; }
            set { _height = value; }
        }

        private int _minFramesPerSecond;
        /// <summary>
        /// Minimum FPS
        /// </summary>
        [DataMember]
        [Description("Specifies the minumum capture rate in frames per second (fps).")]
        public int MinFramesPerSecond
        {
            get { return _minFramesPerSecond; }
            set { _minFramesPerSecond = value; }
        }

        private int _maxFramesPerSecond;
        /// <summary>
        /// Max FPS
        /// </summary>
        [DataMember]
        [Description("Specifies the maximum capture rate in frames per second (fps).")]
        public int MaxFramesPerSecond
        {
            get { return _maxFramesPerSecond; }
            set { _maxFramesPerSecond = value; }
        }

        private string _compression;
        /// <summary>
        /// Compression to use (FourCC 4 byte code)
        /// </summary>
        [DataMember]
        [Description("Specifies the compression to use.\n(FourCC 4 byte code.)")]
        public string Compression
        {
            get { return _compression; }
            set { _compression = value; }
        }

#if false
        /// <summary>
        /// Converts FourCC string to this format instance
        /// </summary>
        /// <param name="format"></param>
        /// <returns></returns>
        internal static vision.Format ToVisionFormat(Format format)
        {
            uint fourcc = 0;

            if (!string.IsNullOrEmpty(format.Compression))
            {
                string comp = format.Compression;
                int length = Math.Min(4, comp.Length);

                for (int i = length - 1; i >= 0; i--)
                {
                    fourcc = (uint)((fourcc << 8) + ((int)comp[i] & 0x7F));
                }
            }

            return new vision.Format(
                format.Width,
                format.Height,
                format.MinFramesPerSecond,
                format.MaxFramesPerSecond,
                fourcc
            );
        }
#endif

        internal uint FourCcCompression
        {
            get
            {
                uint fourcc = 0;

                if (!string.IsNullOrEmpty(Compression))
                {
                    string comp = Compression;
                    int length = Math.Min(4, comp.Length);

                    for (int i = length - 1; i >= 0; i--)
                    {
                        fourcc = (uint)((fourcc << 8) + ((int)comp[i] & 0x7F));
                    }
                }

                return fourcc;
            }
        }
    }

    /// <summary>
    /// type used for serializing the pipe server output log
    /// </summary>
    [DataContract]
    public class PipeServerOutput
    {
        /// <summary>
        /// the output
        /// </summary>
        [DataMember]
        public List<string> Output { get; set; }
    }


    /// <summary>
    /// Get message
    /// </summary>
    [Description("Retrieves the capture state and a list of connected cameras.")]
    public class Get : Get<GetRequestType, PortSet<WebCamState, Fault>>
    {
    }

    /// <summary>
    /// Replace message
    /// </summary>
    [Description("Provides the startup state of the camera service (or indicates when the entire state of the camera service changes).")]
    public class Replace : Replace<WebCamState, PortSet<DefaultReplaceResponseType, Fault>>
    {
    }

    /// <summary>
    /// UpdateDevice message
    /// </summary>
    [Description("Selects which connected camera will be used for capture (or indicates when the camera has been changed).")]
    public class UpdateDevice : Update<UpdateDeviceRequest, PortSet<DefaultUpdateResponseType, Fault>>
    {
    }

    /// <summary>
    /// Frame format update operation
    /// </summary>
    [Description("Sets or changes which size and compression will be used for capture (or indicates when these have been changed).")]
    public class UpdateFormat : Update<Format, PortSet<DefaultUpdateResponseType, Fault>>
    {
    }
}
