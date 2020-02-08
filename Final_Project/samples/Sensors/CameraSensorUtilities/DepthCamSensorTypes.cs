//------------------------------------------------------------------------------
//  <copyright file="DepthCamSensorTypes.cs" company="Microsoft Corporation">
//      Copyright (C) Microsoft Corporation.  All rights reserved.
//  </copyright>
//------------------------------------------------------------------------------
namespace Microsoft.Robotics.Services.DepthCamSensor
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing;
    using System.IO;
    using System.Net;
    using System.Windows.Media.Imaging;
    using System.Xml.Serialization;

    using Microsoft.Ccr.Core;
    using Microsoft.Dss.Core;
    using Microsoft.Dss.Core.Attributes;
    using Microsoft.Dss.Core.DsspHttp;
    using Microsoft.Dss.Core.DsspHttpUtilities;
    using Microsoft.Dss.ServiceModel.Dssp;
    using Microsoft.Robotics.Common;
    using Microsoft.Robotics.PhysicalModel;

    using pm = Microsoft.Robotics.PhysicalModel;
    using swm = System.Windows.Media;

    /// <summary>
    /// Image Feature Extractor Contract
    /// </summary>
    [DisplayName("(User) Generic Depth Camera Contract")]
    public static class Contract
    {
        /// <summary>
        /// Contract identifier
        /// </summary>
        public const string Identifier = "http://schemas.microsoft.com/robotics/2011/01/depthcamsensor.user.html";
    }

    /// <summary>
    /// Depth Camera Http Utilities 
    /// </summary>
    public static class DepthCamSensorHttpUtilities
    {
        /// <summary>
        /// Query type constant
        /// </summary>
        private const string QueryType = "type";

        /// <summary>
        /// Query format constant
        /// </summary>
        private const string QueryFormat = "format";

        /// <summary>
        /// Query string used when constructing uri for Rgb stream.
        /// </summary>
        private const string Rgb = "rgb";

        /// <summary>
        /// Query string used when constructing uri for depth  stream.
        /// </summary>
        private const string Depth = "depth";

        /// <summary>
        /// Query string used when constructing uri for depth 8 bits and RGB combined stream.
        /// </summary>
        private const string DepthPlusRgb = "depthplusrgb";

        /// <summary>
        /// Query string used when requesting png image format.
        /// </summary>
        private const string Png = "png";

        /// <summary>
        /// Query string used when requesting raw stream format.
        /// </summary>
        private const string Raw = "raw";

        /// <summary>
        /// Png Media Type
        /// </summary>
        private const string MediaPng = "image/png";

        /// <summary>
        /// Octet Stream used for raw image format.
        /// </summary>
        private const string OctetStream = "application/octet-stream";

        /// <summary>
        /// Query string used when constructing uri for ir stream
        /// </summary>
        private const string Ir = "ir";

        /// <summary>
        /// Shift that maps short depth values to byte
        /// </summary>
        private const int ShiftToByte = 4;

        /// <summary>
        /// Dots per inch
        /// </summary>
        private const int Dpi = 96;

        /// <summary>
        /// Provides a standard implementation of HttpGet
        /// </summary>
        /// <param name="get">The HttpGet message to process</param>
        /// <param name="state">The depthcam state</param>
        /// <param name="transform">The embedded resource to use as the XSLT transform</param>
        public static void HttpGetHelper(HttpGet get, DepthCamSensorState state, string transform)
        {
            var copy = state.Clone() as DepthCamSensorState;
            copy.DepthImage = null;
            copy.VisibleImage = null;

            get.ResponsePort.Post(
                new HttpResponseType(
                    HttpStatusCode.OK,
                    copy,
                    transform));
        }

        /// <summary>
        /// Handles http query request.
        /// </summary>
        /// <param name="query">The http query</param>
        /// <param name="state">Depth camera state</param>
        /// <param name="transform"> XSLT transform to be applied</param>
        /// <param name="utilitiesPort">Utitilise port to post the response</param>
        /// <param name="visibleWidth">Width of a visible image - needed to blend depth and rgb pictures for a visual represenation</param>
        /// <param name="visibleHeight">Height of a visible image - needed to blend depth and rgb pictures for a visual represenation</param>
        /// <returns>CCR Task Chunk</returns>        
        public static IEnumerator<ITask> HttpQueryHelper(
            HttpQuery query,
            DepthCamSensorState state,
            string transform,
            DsspHttpUtilitiesPort utilitiesPort,
            int visibleWidth,
            int visibleHeight)
        {
            var type = query.Body.Query[QueryType];
            var format = query.Body.Query[QueryFormat];

            // Default is Png
            if (format == null)
            {
                format = Png;
            }

            switch (type.ToLowerInvariant())
            {
                case DepthPlusRgb:
                // Intentional fall through.
                case Rgb:
                    if (state.ImageMode == DepthCamSensor.DepthCamSensorImageMode.Infrared)
                    {
                        state.ImageMode = DepthCamSensor.DepthCamSensorImageMode.Rgb;
                    }

                    break;
                case Depth:
                    // Do nothing.
                    break;
                case Ir:
                    if (state.ImageMode == DepthCamSensor.DepthCamSensorImageMode.Rgb)
                    {
                        state.ImageMode = DepthCamSensor.DepthCamSensorImageMode.Infrared;
                    }

                    break;
            }

            if (format.ToLowerInvariant().Equals(Raw))
            {
                return HttpQueryHelperRawFormat(query, state, transform, utilitiesPort);
            }
            else
            {
                return HttpQueryHelperBitmapSource(query, state, transform, utilitiesPort, visibleWidth, visibleHeight);
            }
        }

        /// <summary>
        /// Handlers http query request for bitmap source format.
        /// </summary>
        /// <param name="query">The http query</param>
        /// <param name="state">Depth camera state</param>
        /// <param name="transform"> XSLT transform to be applied</param>
        /// <param name="utilitiesPort">Utitilise port to post the response</param>        
        /// <param name="visibleWidth">Width of a visible image - needed to blend depth and rgb pictures for a visual represenation</param>
        /// <param name="visibleHeight">Height of a visible image - needed to blend depth and rgb pictures for a visual represenation</param>
        /// <returns>CCR Task Chunk</returns>                
        private static IEnumerator<ITask> HttpQueryHelperBitmapSource(
            HttpQuery query,
            DepthCamSensorState state,
            string transform,
            DsspHttpUtilitiesPort utilitiesPort,
            int visibleWidth,
            int visibleHeight)
        {
            var type = query.Body.Query[QueryType];
            BitmapSource bitmapSource = null;

            try
            {
                switch (type.ToLowerInvariant())
                {
                    case DepthPlusRgb:

                        // we must downscale visible image to match the resolution of depth image - in order to merge the 
                        // two pictures. Its an expensive call - but we do it only here, where it was explicitly requested
                        byte[] resizedVisibleImage = ResizeVisibleImageToMatchDepthImageDimentions(
                            state.VisibleImage, 
                            visibleWidth, 
                            visibleHeight, 
                            state.DepthImageSize.Width, 
                            state.DepthImageSize.Height);

                        bitmapSource = CreateBitmapSourceFromByteArray(
                            ConvertVisibleAndDepthTo32bppByteArray(resizedVisibleImage, state.DepthImage),
                            state.DepthImageSize.Width,
                            state.DepthImageSize.Height,
                            swm.PixelFormats.Bgra32);
                        break;
                    case Rgb:
                        bitmapSource = CreateBitmapSourceFromByteArray(
                            state.VisibleImage,
                            visibleWidth,
                            visibleHeight,
                            swm.PixelFormats.Bgr24);
                        break;
                    case Depth:
                        bitmapSource = CreateBitmapSourceFromShortPixelArray(                            
                            state.DepthImage,
                            state.DepthImageSize.Width,
                            state.DepthImageSize.Height,
                            (int)(state.MaximumRange * 1000),
                            state.DepthImage.Clone() as short[]);
                        break;
                    case Ir:
                        bitmapSource = CreateBitmapSourceFromInfraredArray(
                            state.VisibleImage,
                            state.DepthImageSize.Width,
                            state.DepthImageSize.Height,
                            swm.PixelFormats.Gray16);
                        break;
                }
            }
            catch
            {
                // We do not attempt to synchronize the HTTP view with 
                // state switching, so we might switch to RGB but still
                // have data from IR mode

                var width = state.DepthImageSize.Width;
                var height = state.DepthImageSize.Height;
                var zeroData = new byte[3 * width * height];
                bitmapSource = BitmapSource.Create(
                    width,
                    height,
                    96,
                    96,
                    swm.PixelFormats.Bgr24,
                    null,
                    zeroData,
                    state.DepthImageSize.Width * 3);
            }

            if (bitmapSource == null)
            {
                query.ResponsePort.Post(
                    new HttpResponseType(
                        HttpStatusCode.OK,
                        state,
                        transform));
            }
            else
            {
                using (var mem = new MemoryStream())
                {
                    BitmapEncoder encoder = null;

                    PngBitmapEncoder pngEncoder = new PngBitmapEncoder();
                    pngEncoder.Interlace = PngInterlaceOption.Off;
                    encoder = pngEncoder;

                    encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
                    encoder.Save(mem);

                    mem.Position = 0;

                    var response = new WriteResponseFromStream(
                        query.Body.Context,
                        mem,
                        MediaPng);
                    utilitiesPort.Post(response);

                    yield return response.ResultPort.Choice();
                }
            }
        }

        /// <summary>
        /// We want to scale down the visible image such that the visual and depth matrixes can be overlayed
        /// </summary>
        /// <param name="webCamData">Original web cam data</param>
        /// <param name="visibleImageWidth">Width of the RGB image</param>
        /// <param name="visibleImageHeight">Height of the RGB image</param>
        /// <param name="depthImageWidth">Width of the depth data</param>
        /// <param name="depthImageHeight">Height of the depth data</param>
        /// <returns>Scaled down byte array of the webCamData</returns>
        private static byte[] ResizeVisibleImageToMatchDepthImageDimentions(
            byte[] webCamData,
            int visibleImageWidth, 
            int visibleImageHeight, 
            int depthImageWidth, 
            int depthImageHeight)
        {
            if (visibleImageWidth == depthImageWidth && visibleImageHeight == depthImageHeight)
            {
                return webCamData;
            }

            if (visibleImageWidth < depthImageWidth || visibleImageHeight < depthImageHeight)
            {
                return webCamData;
            }

            int widthRatio = visibleImageWidth / depthImageWidth;
            int heightRatio = visibleImageHeight / depthImageHeight;

            byte[] newVisibleImage = new byte[depthImageWidth * depthImageHeight * 3];

            for (int y = 0; y < depthImageHeight; y++)
            {
                for (int x = 0; x < depthImageWidth; x++)
                {
                    int i = ((y * depthImageWidth) + x) * 3;
                    int j = ((y * depthImageWidth * widthRatio * heightRatio) + (x * heightRatio)) * 3;

                    newVisibleImage[i + 0] = webCamData[j + 0];
                    newVisibleImage[i + 1] = webCamData[j + 1];
                    newVisibleImage[i + 2] = webCamData[j + 2];
                }
            }

            return newVisibleImage;
        }

        /// <summary>
        /// Creates bitmap from short array
        /// </summary>
        /// <param name="visibleImage">Visible image buffer</param>
        /// <param name="width">Width in pixels</param>
        /// <param name="height">Height in pixels</param>
        /// <param name="pixelFormat">Pixel format</param>
        /// <returns>Bitmap source instance</returns>
        private static BitmapSource CreateBitmapSourceFromInfraredArray(
            byte[] visibleImage,
            int width,
            int height,
            swm.PixelFormat pixelFormat)
        {
            var ir = new short[visibleImage.Length / 2];

            // first convert the byte array to short and track max value
            short maxValue = 0;
            for (int i = 0; i < ir.Length; i++)
            {
                ir[i] = (short)(visibleImage[i * 2] | (visibleImage[(i * 2) + 1] << 8));
                maxValue = Math.Max(maxValue, ir[i]);
            }

            // then rescale [0,maxValue] -> [0, ushort.Max]
            for (int i = 0; i < ir.Length; i++)
            {
                ir[i] = (short)((ushort.MaxValue * ir[i]) / maxValue);
            }

            var stride = (pixelFormat.BitsPerPixel / 8) * width;
            return BitmapSource.Create(
                width,
                height,
                96,
                96,
                pixelFormat,
                null,
                ir,
                stride);
        }

        /// <summary>
        /// Creates a BitmapSource from an image
        /// </summary>
        /// <param name="inBuffer">Input buffer</param>
        /// <param name="width">Width of the image</param>
        /// <param name="height">Height of the image</param>
        /// <param name="maxRangeInMM">Depth camera's max range in MM</param>
        /// <param name="outBuffer">Output buffer</param>
        /// <returns>A new instance of BitmapSource</returns>
        private static BitmapSource CreateBitmapSourceFromShortPixelArray(
            short[] inBuffer,
            int width,
            int height,
            int maxRangeInMM,
            short[] outBuffer)
        {            
            int stride = width;
            var pf = System.Windows.Media.PixelFormats.Gray16;
            var data = inBuffer;
            if (3 == (inBuffer.Length / (width * height)))
            {
                stride *= 3;
                pf = System.Windows.Media.PixelFormats.Rgb48;
                for (int i = 0; i < data.Length; i += 3)
                {
                    // 'r' is the first channel in Bgr layout, so need to swap 
                    //  channels since there is no Brg48 format
                    int r = data[i], g = data[i + 1], b = data[i + 2];
                    outBuffer[i] = (short)((ushort)b << 8);
                    outBuffer[i + 1] = (short)((ushort)g << 8);
                    outBuffer[i + 2] = (short)((ushort)r << 8);
                }
            }
            else
            {
                // we use maxRange / 255 in order to achieve the highest dynamic range for the grayscale depth image we create
                int depthImageScalingFactor = maxRangeInMM / 255;

                for (int i = 0; i < outBuffer.Length; i++)
                {
                    outBuffer[i] = (short)(data[i] * depthImageScalingFactor);
                }
            }

            return BitmapSource.Create(
                width,
                height,
                96,
                96,
                pf,
                null,
                outBuffer,
                stride * sizeof(short));
        }

        /// <summary>
        /// Handlers http query request for raw binary format.
        /// </summary>
        /// <param name="query">The http query</param>
        /// <param name="state">Depth camera state</param>
        /// <param name="transform"> XSLT transform to be applied</param>
        /// <param name="utilitiesPort">Utitilise port to post the response</param>
        /// <returns>CCR Task Chunk</returns>   
        private static IEnumerator<ITask> HttpQueryHelperRawFormat(
            HttpQuery query,
            DepthCamSensorState state,
            string transform,
            DsspHttpUtilitiesPort utilitiesPort)
        {
            var type = query.Body.Query[QueryType];
            byte[] imageByteArray = null;
            switch (type.ToLowerInvariant())
            {
                case DepthPlusRgb:
                    imageByteArray =
                        ConvertVisibleAndDepthToByteArray(
                        state.VisibleImage,
                        state.DepthImage);
                    break;
                case Rgb:
                    imageByteArray = state.VisibleImage;
                    break;
                case Depth:
                    imageByteArray = new byte[state.DepthImage.Length * sizeof(short)];
                    Buffer.BlockCopy(state.DepthImage, 0, imageByteArray, 0, imageByteArray.Length);
                    break;
                case Ir:
                    imageByteArray = state.VisibleImage;
                    break;
            }

            if (imageByteArray == null)
            {
                query.ResponsePort.Post(
                    new HttpResponseType(
                        HttpStatusCode.OK,
                        state,
                        transform));
            }
            else
            {
                using (var mem = new MemoryStream(imageByteArray))
                {
                    var response = new WriteResponseFromStream(
                        query.Body.Context,
                        mem,
                        OctetStream);
                    utilitiesPort.Post(response);

                    yield return response.ResultPort.Choice();
                }
            }
        }

        /// <summary>
        /// Creates a bitmap source from the image data.
        /// </summary>
        /// <param name="byteArray">Byte Array containing image data.</param>
        /// <param name="width">Width of the image.</param>
        /// <param name="height">Height of the image.</param>
        /// <param name="pf">Pixel Format of the image</param>
        /// <returns>BitmapSource containing the image</returns>
        /// <returns></returns>
        private static BitmapSource CreateBitmapSourceFromByteArray(
            byte[] byteArray,
            int width,
            int height,
            System.Windows.Media.PixelFormat pf)
        {
            BitmapSource bitmapSource;
            int stride = (width * pf.BitsPerPixel) / 8;

            bitmapSource = BitmapSource.Create(
                width,
                height,
                Dpi,
                Dpi,
                pf,
                null,
                byteArray,
                stride);

            return bitmapSource;
        }

        /// <summary>
        /// Packs Visible and Depth to 40 bit array with
        /// first 2 bytes containing the depth as millimeters and last 3 bytes containing RGB
        /// </summary>
        /// <param name="visibleImage">Visible Image</param>
        /// <param name="depthMap">Depth Map Values</param>
        /// <returns>Depth plus rgb image data as byte array </returns>
        private static byte[] ConvertVisibleAndDepthToByteArray(byte[] visibleImage, short[] depthMap)
        {
            var byteDepths = new byte[5 * depthMap.Length];
            byte[] depthByteArray = new byte[depthMap.Length * sizeof(short)];
            Buffer.BlockCopy(depthMap, 0, depthByteArray, 0, depthByteArray.Length);
            for (int i = 0; i < depthMap.Length; i++)
            {
                byteDepths[(5 * i)] = visibleImage[(3 * i)];
                byteDepths[(5 * i) + 1] = visibleImage[(3 * i) + 1];
                byteDepths[(5 * i) + 2] = visibleImage[(3 * i) + 2];
                byteDepths[(5 * i) + 3] = depthByteArray[(2 * i)];
                byteDepths[(5 * i) + 4] = depthByteArray[(2 * i) + 1];
            }

            return byteDepths;
        }

        /// <summary>
        /// Packs Visible and Depth to 32 bpp ARGB image with Depth in A channel.
        /// </summary>
        /// <param name="visibleImage">Visible Image</param>
        /// <param name="depthMap">Depth Map Values</param>
        /// <returns>32 bpp image data as byte array </returns>
        private static byte[] ConvertVisibleAndDepthTo32bppByteArray(byte[] visibleImage, short[] depthMap)
        {
            // 4*8 = 32bpp
            var byteDepths = new byte[4 * depthMap.Length];

            for (int i = 0; i < depthMap.Length; i++)
            {
                byte byteDepthValue = (byte)(depthMap[i] >> ShiftToByte);
                byteDepths[(4 * i)] = visibleImage[(3 * i)];
                byteDepths[(4 * i) + 1] = visibleImage[(3 * i) + 1];
                byteDepths[(4 * i) + 2] = visibleImage[(3 * i) + 2];
                byteDepths[(4 * i) + 3] = byteDepthValue;
            }

            return byteDepths;
        }        
    }

    /// <summary>
    /// Depth Cam Sensor Image Mode
    /// </summary>
    public enum DepthCamSensorImageMode
    {
        /// <summary>
        /// Rgb image type
        /// </summary>
        Rgb,

        /// <summary>
        /// Infrared image type
        /// </summary>
        Infrared
    }

    /// <summary>
    /// Depth camera state
    /// </summary>
    [XmlRoot(Namespace = Contract.Identifier)]
    [DataContract(ExcludeFromProxy = true, Name = "DepthCamSensorState", Namespace = Contract.Identifier)]
    public class DepthCamSensorState : IDssSerializable
    {
        /// <summary>
        /// Gets or sets the Array of 2 dimensional depth map represented the same way as a grayscale image
        /// Depth Map size field indicates width and height of depth map.
        /// A value of DepthCamSensorReservedSampleValues.NoReading is reserved to indicate 
        ///     no reading or shadow in the sample
        /// </summary>
        [DataMember]
        public short[] DepthImage;

        /// <summary>
        /// Gets or sets the optional 24bpp visible spectrum image
        /// </summary>
        [DataMember]
        public byte[] VisibleImage;

        /// <summary>
        /// Gets or sets the Dimensions of depth image
        /// </summary>
        [DataMember]
        public Size DepthImageSize
        {
            get 
            { 
                return this.depthImageSize; 
            }

            set
            {
                if (this.depthImageSize != value)
                {
                    this.depthImageSize = value;
                    this.UpdateProjectionMatrix();
                }
            }
        }

        /// <summary>
        /// Depth image size
        /// </summary>
        private Size depthImageSize;

        /// <summary>
        /// Gets or sets Depth camera orientation
        /// </summary>
        [DataMember]
        public Pose Pose;

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
        /// Gets or sets Timestamp of this measurement
        /// </summary>
        [DataMember]
        public DateTime TimeStamp;

        /// <summary>
        /// Gets or sets the horizontal field of view, in radians
        /// </summary>
        [DataMember]
        public double FieldOfView
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
        /// Horizontal FOV
        /// </summary>
        private double horizontalFieldOfView;

        /// <summary>
        /// Max range in meters
        /// </summary>
        [DataMember]
        public double MaximumRange;

        /// <summary>
        /// Min range in meters
        /// </summary>
        [DataMember]
        public double MinimumRange;

        /// <summary>
        /// The depth sample value returned by the depth cam when 
        /// the range is further than the max value.
        /// </summary>
        [DataMember]
        public short FurtherThanMaxDepthValue;

        /// <summary>
        /// The depth sample value returned by the depth cam when
        /// the range is nearer than the min value.
        /// </summary>
        [DataMember]
        public short NearerThanMinDepthValue;

        /// <summary>
        /// Gets or sets depth cam visible image 
        /// (intentionally not serialized, avoids breaking serialized state, 
        /// not intended to be passed across a process boundary)
        /// </summary>
        public DepthCamSensorImageMode ImageMode { get; set; }

        /// <summary>
        /// Computes projection matrix from width, height, fov and caches inverse
        /// </summary>
        private void UpdateProjectionMatrix()
        {
            if (this.DepthImageSize.Width != 0 && this.DepthImageSize.Height != 0 && this.FieldOfView != 0)
            {
                this.ProjectionMatrix = MathUtilities.ComputeProjectionMatrix(
                    (float)this.FieldOfView,
                    this.DepthImageSize.Width,
                    this.DepthImageSize.Height,
                    this.MaximumRange == 0 ? 10 : this.MaximumRange);
                this.InverseProjectionMatrix = MathUtilities.Invert(this.ProjectionMatrix);
            }
        }

        #region IDssSerializable Members

        /// <summary>
        /// Shallow clone
        /// </summary>
        /// <returns>New instance</returns>
        public object Clone()
        {
            var copy = new DepthCamSensorState();
            this.CopyTo(copy);
            return copy;
        }

        /// <summary>
        /// Copy to another instance
        /// </summary>
        /// <param name="target">Instance to copy to</param>
        public void CopyTo(IDssSerializable target)
        {
            var copy = target as DepthCamSensorState;

            // While it is very risky to not clone, its necessery to avoid copies. 
            // Users of depthcam data requiring write access must clone then 
            //  manipulate the arrays 
            copy.DepthImage = this.DepthImage;
            copy.DepthImageSize = this.DepthImageSize;
            copy.FieldOfView = this.FieldOfView;
            copy.MaximumRange = this.MaximumRange;
            copy.MinimumRange = this.MinimumRange;
            copy.FurtherThanMaxDepthValue = this.FurtherThanMaxDepthValue;
            copy.NearerThanMinDepthValue = this.NearerThanMinDepthValue;
            copy.Pose = this.Pose;
            copy.TimeStamp = this.TimeStamp;
            copy.VisibleImage = this.VisibleImage;
            copy.ImageMode = this.ImageMode;
        }

        /// <summary>
        /// Binary deserialize
        /// </summary>
        /// <param name="reader">Binary reader instance</param>
        /// <returns>Deserialized instance</returns>
        public object Deserialize(BinaryReader reader)
        {
            int length = reader.ReadInt32();
            if (length > 0)
            {
                this.DepthImage = new short[length];
                for (int i = 0; i < this.DepthImage.Length; i++)
                {
                    this.DepthImage[i] = reader.ReadInt16();
                }
            }

            var depthWidth = reader.ReadInt32();
            var depthHeight = reader.ReadInt32();
            this.DepthImageSize = new Size(depthWidth, depthHeight);
            this.FieldOfView = reader.ReadDouble();
            this.MaximumRange = reader.ReadDouble();
            this.MinimumRange = reader.ReadDouble();
            this.FurtherThanMaxDepthValue = reader.ReadInt16();
            this.NearerThanMinDepthValue = reader.ReadInt16();
            this.Pose.Orientation.W = reader.ReadSingle();
            this.Pose.Orientation.X = reader.ReadSingle();
            this.Pose.Orientation.Y = reader.ReadSingle();
            this.Pose.Orientation.Z = reader.ReadSingle();
            this.Pose.Position.X = reader.ReadSingle();
            this.Pose.Position.Y = reader.ReadSingle();
            this.Pose.Position.Z = reader.ReadSingle();
            this.TimeStamp = DateTime.FromBinary(reader.ReadInt64());

            length = reader.ReadInt32();
            if (length > 0)
            {
                this.VisibleImage = reader.ReadBytes(length);
            }

            return this;
        }

        /// <summary>
        /// Binary serialize
        /// </summary>
        /// <param name="writer">Binarywriter instance</param>
        public void Serialize(BinaryWriter writer)
        {
            if (this.DepthImage == null)
            {
                writer.Write((int)0);
            }
            else
            {
                writer.Write((int)this.DepthImage.Length);
                for (int i = 0; i < this.DepthImage.Length; i++)
                {
                    writer.Write(this.DepthImage[i]);
                }                
            }

            writer.Write((int)this.DepthImageSize.Width);
            writer.Write((int)this.DepthImageSize.Height);
            writer.Write(this.FieldOfView);
            writer.Write(this.MaximumRange);
            writer.Write(this.MinimumRange);
            writer.Write(this.FurtherThanMaxDepthValue);
            writer.Write(this.NearerThanMinDepthValue);
            writer.Write(this.Pose.Orientation.W);
            writer.Write(this.Pose.Orientation.X);
            writer.Write(this.Pose.Orientation.Y);
            writer.Write(this.Pose.Orientation.Z);
            writer.Write(this.Pose.Position.X);
            writer.Write(this.Pose.Position.Y);
            writer.Write(this.Pose.Position.Z);
            writer.Write(this.TimeStamp.ToBinary());

            if (this.VisibleImage == null)
            {
                writer.Write((int)0);
            }
            else
            {
                writer.Write((int)this.VisibleImage.Length);
                writer.Write(this.VisibleImage);
            }
        }

        #endregion
    }

    /// <summary>
    /// Get operation
    /// </summary>
    public class Get : Get<GetRequestType, DsspResponsePort<DepthCamSensorState>>
    {
    }

    /// <summary>
    /// Replace operation
    /// </summary>
    public class Replace : Replace<DepthCamSensorState, DsspResponsePort<DefaultReplaceResponseType>>
    {
    }

    /// <summary>
    /// Subscribe operation
    /// </summary>
    public class Subscribe : Subscribe<SubscribeRequestType, DsspResponsePort<SubscribeResponseType>>
    {
    }

    /// <summary>
    /// Depth camera operations port
    /// </summary>
    [ServicePort]
    public class DepthCamSensorOperationsPort : PortSet<DsspDefaultLookup, DsspDefaultDrop, Get, Replace, Subscribe, HttpGet, HttpQuery>
    {
    }
}