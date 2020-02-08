//------------------------------------------------------------------------------
//  <copyright file="WebCamSensorTypes.cs" company="Microsoft Corporation">
//      Copyright (C) Microsoft Corporation.  All rights reserved.
//  </copyright>
//------------------------------------------------------------------------------
namespace Microsoft.Robotics.Services.WebCamSensor
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing;
    using System.IO;
    using System.Net;
    using System.Net.Mime;
    using System.Runtime.InteropServices;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Xml.Serialization;
    using Microsoft.Ccr.Core;
    using Microsoft.Dss.Core;
    using Microsoft.Dss.Core.Attributes;
    using Microsoft.Dss.Core.DsspHttp;
    using Microsoft.Dss.Core.DsspHttpUtilities;
    using Microsoft.Dss.ServiceModel.Dssp;
    using Microsoft.Robotics.Common;
    using Microsoft.Robotics.Properties;
    using pm = Microsoft.Robotics.PhysicalModel;
    using sdi = System.Drawing.Imaging;

    /// <summary>
    /// Standard optimized webcam contract 
    /// </summary>
    [DisplayName("(User) Generic WebCamSensor")]
    [Description("WebCamSensor contract")]
    public static class Contract
    {
        /// <summary>
        /// Unique contract identifier and xml namespace
        /// </summary>
        [DataMember]
        public const string Identifier = "http://schemas.microsoft.com/robotics/2011/01/webcamsensor.user.html";
    }

    /// <summary>
    /// Service state for the Generic WebCamSensor contract
    /// </summary>
    [XmlRoot(Namespace = Contract.Identifier)]
    [DataContract(ExcludeFromProxy = true, Name = "WebCamSensorState", Namespace = Contract.Identifier)]
    public class WebCamSensorState : IDssSerializable
    {
        /// <summary>
        /// Gets or sets the device that generated the image
        /// </summary>
        [DataMember]
        public string DeviceName { get; set; }

        /// <summary>
        /// Gets or sets the Timestamp of the image. This should always be in UTC time
        /// </summary>
        [DataMember]
        public DateTime TimeStamp { get; set; }

        /// <summary>
        /// Gets or sets the width in pixels of the image
        /// </summary>
        [DataMember]
        public int Width
        {
            get 
            {
                return this.width; 
            }

            set
            {
                if (this.width != value)
                {
                    this.width = value;
                    this.UpdateProjectionMatrix();
                }
            }
        }

        /// <summary>
        /// Image width
        /// </summary>
        private int width;

        /// <summary>
        /// Gets or sets the height in pixels of the image
        /// </summary>
        [DataMember]
        public int Height
        {
            get 
            { 
                return this.height; 
            }

            set
            {
                if (this.height != value)
                {
                    this.height = value;
                    this.UpdateProjectionMatrix();
                }
            }
        }

        /// <summary>
        /// Image height
        /// </summary>
        private int height;

        /// <summary>
        /// Gets or sets the stride in bytes of the image
        /// </summary>
        [DataMember]
        public int Stride { get; set; }

        /// <summary>
        /// Gets or sets the raw image data in PixelFormat.Bgr24
        /// </summary>
        [DataMember]
        public byte[] Data { get; set; }

        /// <summary>
        /// Gets or sets Horizontal field of view (in radians)
        /// </summary>
        [DataMember]
        public double HorizontalFieldOfView 
        {
            get 
            {
                return this.horizontalFieldOfView; 
            }
            
            set
            {
                if (this.horizontalFieldOfView != value)
                {
                    this.horizontalFieldOfView = value;
                    this.UpdateProjectionMatrix();
                }
            }
        }

        /// <summary>
        /// 55 degrees is a relatively common horizontal fov, so we pick it as a default
        /// It is also the horizontal fov of the cinema hd
        /// </summary>
        private double horizontalFieldOfView = 55 * Math.PI / 180.0;

        /// <summary>
        /// Gets or sets projection matrix that converts real world view space points to camera space points
        /// Not data member, a cached value based on other data member values
        /// </summary>
        public pm.Matrix ProjectionMatrix { get; set; }

        /// <summary>
        /// Gets or sets inverse of above projection matrix 
        /// Not data member, a cached value based on other data member values
        /// </summary>
        public pm.Matrix InverseProjectionMatrix { get; set; }

        /// <summary>
        /// Computes projection matrix from width, height, fov and caches inverse
        /// </summary>
        private void UpdateProjectionMatrix()
        {
            if (this.Width != 0 && this.Height != 0 && this.HorizontalFieldOfView != 0)
            {
                this.ProjectionMatrix = MathUtilities.ComputeProjectionMatrix(
                    (float)this.HorizontalFieldOfView,
                    this.Width,
                    this.Height,
                    10);
                this.InverseProjectionMatrix = MathUtilities.Invert(this.ProjectionMatrix);
            }
        }

        #region Image accessors

        /// <summary>
        /// Gets a System.Drawing.Bitmap of the image
        /// </summary>
        [XmlIgnore]
        public Bitmap Bitmap
        {
            get
            {
                if (this.Data == null)
                {
                    return null;
                }

                var pixelFormat = default(sdi.PixelFormat);

                if (this.Stride == this.Width * 3)
                {
                    pixelFormat = sdi.PixelFormat.Format24bppRgb;
                }
                else if (this.Stride == this.Width * 4)
                {
                    pixelFormat = sdi.PixelFormat.Format32bppRgb;
                }
                else
                {
                    throw new Exception(Resources.StrideDoesNotMatch);
                }

                var bmp = new Bitmap(this.Width, this.Height, pixelFormat);
                try
                {
                    var bits = bmp.LockBits(
                        new Rectangle(0, 0, this.Width, this.Height),
                        sdi.ImageLockMode.ReadWrite,
                        pixelFormat);

                    if (this.Stride != bits.Stride)
                    {
                        throw new Exception(Resources.StrideDoesNotMatch);
                    }

                    var size = bits.Stride * bits.Height;
                    if (size > this.Data.Length)
                    {
                        throw new Exception(Resources.DataTooSmall);
                    }

                    Marshal.Copy(this.Data, 0, bits.Scan0, size);

                    bmp.UnlockBits(bits);
                }
                catch (Exception)
                {
                    bmp.Dispose();
                    throw;
                }

                return bmp;
            }
        }

        /// <summary>
        /// Gets a BitmapSource of the image
        /// </summary>
        [XmlIgnore]
        public BitmapSource BitmapSource
        {
            get
            {
                if (this.Data == null)
                {
                    return null;
                }

                if (this.Stride == this.Width * 3)
                {
                    return BitmapSource.Create(
                        this.Width,
                        this.Height,
                        96,
                        96,
                        PixelFormats.Bgr24,
                        null,
                        this.Data,
                        this.Stride);
                }
                else if (this.Stride == this.Width * 4) 
                {
                    return BitmapSource.Create(
                        this.Width,
                        this.Height,
                        96,
                        96,
                        PixelFormats.Bgr32,
                        null,
                        this.Data,
                        this.Stride);
                }
                else
                {
                    throw new Exception(Resources.StrideDoesNotMatch);
                }
            }
        }

        /// <summary>
        /// Updates or creates a WriteableBitmap of the image
        /// </summary>
        /// <remarks>
        /// If the supplied <paramref name="input"/> is null or a different size from the image, then
        /// a new WriteableBitmap is returned.
        /// </remarks>
        /// <param name="input">A previously existing WriteableBitmap to update, or null to create a new writeable bitmap</param>
        /// <returns>A writeable bitmap</returns>
        public WriteableBitmap UpdateWritableBitmap(WriteableBitmap input)
        {
            if (input == null ||
                input.PixelWidth != this.Width ||
                input.PixelHeight != this.Height)
            {
                input = new WriteableBitmap(
                    this.Width,
                    this.Height,
                    96,
                    96,
                    PixelFormats.Bgr24,
                    null);
            }

            input.WritePixels(
                new Int32Rect(0, 0, this.Width, this.Height),
                this.Data,
                this.Stride,
                0);

            return input;
        }

        #endregion

        #region IDssSerializable Members

        /// <summary>
        /// Clones this object
        /// </summary>
        /// <remarks>This is a shallow clone, avoiding copying the image data</remarks>
        /// <returns>A shallow clone of this object</returns>
        public object Clone()
        {
            var clone = new WebCamSensorState();
            this.CopyTo(clone);
            return clone;
        }

        /// <summary>
        /// Copies this object to the target
        /// </summary>
        /// <remarks>This assumes that target is the correct type</remarks>
        /// <param name="target">A WebCamSensorState instance to copy to</param>
        public void CopyTo(IDssSerializable target)
        {
            var other = (WebCamSensorState)target;

            other.DeviceName = this.DeviceName;
            other.TimeStamp = this.TimeStamp;
            other.Width = this.Width;
            other.Height = this.Height;
            other.Stride = this.Stride;
            other.Data = this.Data;
        }

        /// <summary>
        /// Deserialize from a binary stream
        /// </summary>
        /// <param name="reader">The binary reader to deserialize from</param>
        /// <returns>The deserialized instance (always the current object)</returns>
        public object Deserialize(System.IO.BinaryReader reader)
        {
            this.TimeStamp = DateTime.FromBinary(reader.ReadInt64());
            this.Width = reader.ReadInt32();
            this.Height = reader.ReadInt32();
            this.Stride = reader.ReadInt32();

            var present = reader.ReadByte();
            if (present == DefinedElement)
            {
                var length = reader.ReadInt32();
                this.Data = reader.ReadBytes(length);
            }
            else
            {
                this.Data = null;
            }

            return this;
        }

        /// <summary>
        /// Null element marker
        /// </summary>
        private const byte NullElement = 0;

        /// <summary>
        /// Defined element marker
        /// </summary>
        private const byte DefinedElement = 1;

        /// <summary>
        /// Serialize to a binary stream
        /// </summary>
        /// <param name="writer">The stream to serialize too</param>
        public void Serialize(System.IO.BinaryWriter writer)
        {
            writer.Write(this.TimeStamp.ToBinary());
            writer.Write(this.Width);
            writer.Write(this.Height);
            writer.Write(this.Stride);
            if (this.Data == null)
            {
                writer.Write(NullElement);
            }
            else
            {
                writer.Write(DefinedElement);
                writer.Write(this.Data.Length);
                writer.Write(this.Data);
            }
        }

        #endregion
    }

    /// <summary>
    /// The ServicePort for the webcam generic contract
    /// </summary>
    [ServicePort("/webcam")]
    public class WebCamSensorOperations : PortSet<DsspDefaultLookup, DsspDefaultDrop, Get, Replace, Subscribe, HttpGet, HttpQuery>
    {
        #region Message Helper Functions
        /// <summary>
        /// Helper function for the Get message. 
        /// This posts a get message to the port.
        /// </summary>
        /// <returns>The response port for the get message</returns>
        public DsspResponsePort<WebCamSensorState> Get()
        {
            var get = new Get();

            Post(get);

            return get.ResponsePort;
        }

        /// <summary>
        /// Helper function for the Replace message.
        /// This posts a replace message to the port.
        /// </summary>
        /// <param name="state">The state to replace.</param>
        /// <returns>The response port for the replace message.</returns>
        public DsspResponsePort<DefaultReplaceResponseType> Replace(WebCamSensorState state)
        {
            var replace = new Replace(state);

            Post(replace);

            return replace.ResponsePort;
        }

        /// <summary>
        /// Helper function for the Subscribe message.
        /// This posts a subscribe message to the port.
        /// There is no overload for filtering by type, because the only notification type posted is Replace
        /// </summary>
        /// <param name="notifyPort">The port that notifications will be posted to.</param>
        /// <returns>The response port for the subscribe message.</returns>
        public DsspResponsePort<SubscribeResponseType> Subscribe(WebCamSensorOperations notifyPort)
        {
            var subscribe = new Subscribe();
            subscribe.NotificationPort = notifyPort;

            Post(subscribe);

            return subscribe.ResponsePort;
        }
        #endregion

        #region Implementation Helper Functions

        /// <summary>
        /// Provides a standard implementation of HttpGet
        /// </summary>
        /// <param name="get">The HttpGet message to process</param>
        /// <param name="state">The webcam state</param>
        /// <param name="transform">The embedded resource to use as the XSLT transform</param>
        public void HttpGetHelper(HttpGet get, WebCamSensorState state, string transform)
        {
            var copy = state.Clone() as WebCamSensorState;
            copy.Data = null;

            get.ResponsePort.Post(
                new HttpResponseType(
                    HttpStatusCode.OK,
                    copy,
                    transform));
        }

        /// <summary>
        /// Jpeq tag
        /// </summary>
        private const string Jpeg = "jpeg";

        /// <summary>
        /// GIF tag
        /// </summary>
        private const string Gif = "gif";

        /// <summary>
        /// Bitmap tag
        /// </summary>
        private const string Bmp = "bmp";

        /// <summary>
        /// Png tag
        /// </summary>
        private const string Png = "png";

        /// <summary>
        /// Http bmp media tag
        /// </summary>
        private const string MediaBmp = "image/bmp";

        /// <summary>
        /// Http PNG tag
        /// </summary>
        private const string MediaPng = "image/png";

        /// <summary>
        /// Query type
        /// </summary>
        private const string QueryType = "Type";

        /// <summary>
        /// Provides a standard implementation of HttpQuery
        /// </summary>
        /// <param name="query">The HttpQuery message to process</param>
        /// <param name="state">The webcam state</param>
        /// <param name="transform">The embedded resource to use as the XSLT transform</param>
        /// <param name="utilitiesPort">The utilities port used to stream images</param>
        /// <returns>Stanard CCR iterator</returns>
        public IEnumerator<ITask> HttpQueryHelper(HttpQuery query, WebCamSensorState state, string transform, DsspHttpUtilitiesPort utilitiesPort)
        {
            var type = query.Body.Query[QueryType];
            var mediaType = string.Empty;
            var format = default(sdi.ImageFormat);

            switch (type.ToLowerInvariant())
            {
                case Jpeg:
                    format = sdi.ImageFormat.Jpeg;
                    mediaType = MediaTypeNames.Image.Jpeg;
                    break;

                case Gif:
                    format = sdi.ImageFormat.Gif;
                    mediaType = MediaTypeNames.Image.Gif;
                    break;

                case Png:
                    format = sdi.ImageFormat.Png;
                    mediaType = MediaPng;
                    break;

                case Bmp:
                    format = sdi.ImageFormat.Bmp;
                    mediaType = MediaBmp;
                    break;
                    
                default:
                    var copy = state.Clone() as WebCamSensorState;
                    copy.Data = null;

                    query.ResponsePort.Post(
                        new HttpResponseType(
                            HttpStatusCode.OK,
                            copy,
                            transform));
                    yield break;
            }

            if (state.Data == null || state.Data.Length == 0)
            {
                query.ResponsePort.Post(
                        new HttpResponseType(
                            HttpStatusCode.OK,
                            state,
                            transform));
                yield break;
            }

            using (var image = state.Bitmap)
            {
                using (var mem = new MemoryStream())
                {
                    image.Save(mem, format);
                    mem.Position = 0;

                    var response = new WriteResponseFromStream(
                        query.Body.Context,
                        mem,
                        mediaType);

                    utilitiesPort.Post(response);

                    yield return response.ResultPort.Choice();
                }
            }
        }
        
        #endregion
    }

    /// <summary>
    /// Get message for the webcam generic contract 
    /// </summary>
    public class Get : Get<GetRequestType, DsspResponsePort<WebCamSensorState>>
    {
    }

    /// <summary>
    /// Replace message for the webcam generic contract
    /// </summary>
    public class Replace : Replace<WebCamSensorState, DsspResponsePort<DefaultReplaceResponseType>>
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public Replace()
        {
        }

        /// <summary>
        /// Initializing constructor
        /// </summary>
        /// <param name="body">The initial body value</param>
        public Replace(WebCamSensorState body)
            : base(body)
        {
        }
    }

    /// <summary>
    /// Subscribe message for the webcam generic contract
    /// </summary>
    public class Subscribe : Subscribe<SubscribeRequestType, DsspResponsePort<SubscribeResponseType>>
    {
    }
}
