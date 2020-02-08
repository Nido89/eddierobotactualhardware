//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: BlobTrackerTypes.cs $ $Revision: 3 $
//-----------------------------------------------------------------------
using Microsoft.Ccr.Core;
using Microsoft.Dss.Core.Attributes;
using Microsoft.Dss.ServiceModel.Dssp;
using System;
using System.Collections.Generic;
using W3C.Soap;
using blobtracker = Microsoft.Robotics.Services.Sample.BlobTracker;
using System.Drawing;
using System.ComponentModel;


namespace Microsoft.Robotics.Services.Sample.BlobTracker
{

    /// <summary>
    /// BlobTracker Contract class
    /// </summary>
    public sealed class Contract
    {
        /// <summary>
        /// The Dss Service contract
        /// </summary>
        public const String Identifier = "http://schemas.microsoft.com/robotics/2007/03/blobtracker.user.html";
    }
    /// <summary>
    /// The BlobTracker State
    /// </summary>
    [DataContract]
    [Description ("The blob tracker's state")]
    public class BlobTrackerState
    {
        /// <summary>
        /// Indicates whether an update frame is available.
        /// </summary>
        [Browsable(false)]
        public bool UpdateFrame
        {
            get { return _updateFrame; }
            set { _updateFrame = value; }
        }
        private bool _updateFrame;

        /// <summary>
        /// The set of color bins
        /// </summary>
        [DataMember(IsRequired = true)]
        public List<ColorBin> ColorBins
        {
            get { return _colorBins; }
            set { _colorBins = value; }
        }
        private List<ColorBin> _colorBins = new List<ColorBin>();

        /// <summary>
        /// TimeStamp of the last update
        /// </summary>
        [DataMember]
        [Browsable(false)]
        public DateTime TimeStamp
        {
            get { return _timeStamp; }
            set { _timeStamp = value; }
        }
        private DateTime _timeStamp;

        /// <summary>
        /// The list of matching blobs found
        /// </summary>
        [DataMember(IsRequired = true)]
        [Browsable(false)]
        public List<FoundBlob> Results
        {
            get { return _results; }
            set { _results = value; }
        }
        private List<FoundBlob> _results = new List<FoundBlob>();
    }

    /// <summary>
    /// Specifies a color bin (set)
    /// </summary>
    [DataContract]
    public class ColorBin
    {
        /// <summary>
        /// Indicates the name of the color bin (set).
        /// </summary>
        [DataMember]
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }
        private string _name;

        /// <summary>
        /// Indicates minimum red value of the color bin.
        /// </summary>
        [DataMember]
        public int RedMin
        {
            get { return _redMin; }
            set { _redMin = value; }
        }
        private int _redMin;

        /// <summary>
        /// Indicates maximum red value of the color bin.
        /// </summary>
        [DataMember]
        public int RedMax
        {
            get { return _redMax; }
            set { _redMax = value; }
        }
        private int _redMax;

        /// <summary>
        /// Indicates minimum green value of the color bin.
        /// </summary>
        [DataMember]
        public int GreenMin
        {
            get { return _greenMin; }
            set { _greenMin = value; }
        }
        private int _greenMin;

        /// <summary>
        /// Indicates maximum green value of the color bin.
        /// </summary>
        [DataMember]
        public int GreenMax
        {
            get { return _greenMax; }
            set { _greenMax = value; }
        }
        private int _greenMax;

        /// <summary>
        /// Indicates minimum blue value of the color bin.
        /// </summary>
        [DataMember]
        public int BlueMin
        {
            get { return _blueMin; }
            set { _blueMin = value; }
        }
        private int _blueMin;

        /// <summary>
        /// Indicates maximum blue value of the color bin.
        /// </summary>
        [DataMember]
        public int BlueMax
        {
            get { return _blueMax; }
            set { _blueMax = value; }
        }
        private int _blueMax;

        /// <summary>
        /// Checks if the specified color matches the current color bin.
        /// </summary>
        /// <param name="color">Color object</param>
        /// <returns>True if the color matches, false otherwise</returns>
        public bool Test(Color color)
        {
            return Test(color.R, color.G, color.B);
        }

        internal bool Test(int red, int green, int blue)
        {
            return (red >= _redMin && red < _redMax &&
                green >= _greenMin && green < _greenMax &&
                blue >= _blueMin && blue < _blueMax);
        }
    }

    /// <summary>
    /// Specifies information about the detected blob.
    /// </summary>
    [DataContract]
    public class FoundBlob
    {
        /// <summary>
        /// Indicates the X projection.
        /// </summary>
        [DataMember]
        public int[] XProjection
        {
            get { return _xProjection; }
            set { _xProjection = value; }
        }
        private int[] _xProjection;

        /// <summary>
        /// Indicates the Y projection.
        /// </summary>
        [DataMember]
        public int[] YProjection
        {
            get { return _yProjection; }
            set { _yProjection = value; }
        }
        private int[] _yProjection;

        /// <summary>
        /// Indicates the name of the blob.
        /// </summary>
        [DataMember]
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }
        private string _name;

        /// <summary>
        /// Indicates the X mean value.
        /// </summary>
        [DataMember]
        public double MeanX
        {
            get { return _meanX; }
            set { _meanX = value; }
        }
        private double _meanX;

        /// <summary>
        /// Indicates the Y mean value.
        /// </summary>
        [DataMember]
        public double MeanY
        {
            get { return _meanY; }
            set { _meanY = value; }
        }
        private double _meanY;

        /// <summary>
        /// Indicates the X standard deviation value.
        /// </summary>
        [DataMember]
        public double StdDevX
        {
            get { return _stdDevX; }
            set { _stdDevX = value; }
        }
        private double _stdDevX;

        /// <summary>
        /// Indicates the Y standard deviation value.
        /// </summary>
        [DataMember]
        public double StdDevY
        {
            get { return _stdDevY; }
            set { _stdDevY = value; }
        }
        private double _stdDevY;

        /// <summary>
        /// Indicates the X skew value.
        /// </summary>
        [DataMember]
        public double SkewX
        {
            get { return _skewX; }
            set { _skewX = value; }
        }
        private double _skewX;

        /// <summary>
        /// Indicates the Y skew value.
        /// </summary>
        [DataMember]
        public double SkewY
        {
            get { return _skewY; }
            set { _skewY = value; }
        }
        private double _skewY;

        /// <summary>
        /// Indicates area. This is the number of pixels that contribute to the blob.
        /// </summary>
        [DataMember]
        public double Area
        {
            get { return _area; }
            set { _area = value; }
        }
        private double _area;

        internal void CalculateMoments()
        {
            double square;

            double yOff = -_meanY;
            _stdDevY = 0.0;
            _skewY = 0.0;

            for (int y = 0; y < _yProjection.Length; y++, yOff++)
            {
                if (_yProjection[y] > 0)
                {
                    square = yOff * yOff * _yProjection[y];

                    _stdDevY += square;
                    _skewY += yOff * square;
                }
            }

            _stdDevY = Math.Sqrt(_stdDevY / _area);
            _skewY = _skewY / (_area * _stdDevY * _stdDevY * _stdDevY);

            double xOff = -_meanX;
            _stdDevX = 0.0;
            _skewX = 0.0;

            for (int x = 0; x < _xProjection.Length; x++, xOff++)
            {
                if (_xProjection[x] > 0)
                {
                    square = xOff * xOff * _xProjection[x];

                    _stdDevX += square;
                    _skewX += xOff * square;
                }
            }

            _stdDevX = Math.Sqrt(_stdDevX / _area);
            _skewX = _skewX / (_area * _stdDevX * _stdDevX * _stdDevX);
        }

        internal void AddPixel(int x, int y)
        {
            _meanX += x;
            _meanY += y;
            _xProjection[x]++;
            _yProjection[y]++;
            _area++;
        }
    }

    /// <summary>
    /// Indicates the ImageProcessed request.
    /// </summary>
    [DataContract]
    public class ImageProcessedRequest
    {
        /// <summary>
        /// Indicates the time the image was processed.
        /// </summary>
        [DataMember]
        public DateTime TimeStamp
        {
            get { return _timeStamp; }
            set { _timeStamp = value; }
        }
        private DateTime _timeStamp;

        /// <summary>
        /// Indicates the list of blobs found.
        /// </summary>
        [DataMember(IsRequired = true)]
        public List<FoundBlob> Results
        {
            get { return _results; }
            set { _results = value; }
        }
        private List<FoundBlob> _results = new List<FoundBlob>();
    }

    /// <summary>
    /// BlobTracker Main Operations Port
    /// </summary>
    [ServicePort]
    public class BlobTrackerOperations : PortSet
    {
        /// <summary>
        /// BlobTracker Operations PortSet
        /// </summary>
        public BlobTrackerOperations()
            : base(
                typeof(DsspDefaultLookup),
                typeof(DsspDefaultDrop),
                typeof(Get),
                typeof(ImageProcessed),
                typeof(Subscribe),
                typeof(InsertBin),
                typeof(DeleteBin),
                typeof(UpdateBin)
            )
        {
        }

        /// <summary>
        /// Implicit cast from BlobTrackerOperations to Port&lt;ImageProcessed&gt;
        /// </summary>
        public static implicit operator Port<ImageProcessed>(BlobTrackerOperations portSet)
        {
            if (portSet == null)
            {
                return null;
            }
            return (Port<ImageProcessed>)portSet[typeof(ImageProcessed)];
        }

        /// <summary>
        /// Post helper method
        /// </summary>
        public void Post(ImageProcessed msg)
        {
            base.PostUnknownType(msg);
        }

        /// <summary>
        /// Implicit cast from BlobTrackerOperations to Port&lt;DsspDefaultDrop&gt;
        /// </summary>
        public static implicit operator Port<DsspDefaultDrop>(BlobTrackerOperations portSet)
        {
            if (portSet == null)
            {
                return null;
            }
            return (Port<DsspDefaultDrop>)portSet[typeof(DsspDefaultDrop)];
        }

        /// <summary>
        /// Post helper method
        /// </summary>
        public void Post(DsspDefaultDrop msg)
        {
            base.PostUnknownType(msg);
        }
    }
    /// <summary>
    /// BlobTracker Get Operation
    /// </summary>
    [Description("Gets the current state of the service.")]
    public class Get : Get<GetRequestType, PortSet<BlobTrackerState, Fault>>
    {
    }

    /// <summary>
    /// BlobTracker Subscribe Operation
    /// </summary>
    [Description("Subscribes to the service notifications.")]
    public class Subscribe : Subscribe<SubscribeRequestType, PortSet<SubscribeResponseType, Fault>>
    {
    }

    /// <summary>
    /// BlobTracker Update Operation
    /// </summary>
    [Description("Indicates when an image has been processed.")]
    public class ImageProcessed : Update<ImageProcessedRequest, PortSet<DefaultUpdateResponseType, Fault>>
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public ImageProcessed()
        {
        }

        /// <summary>
        /// Creates a new ImageProcessed message with the specified request body
        /// </summary>
        /// <param name="body"></param>
        public ImageProcessed(ImageProcessedRequest body)
            : base(body)
        {
        }
    }

    /// <summary>
    /// BlobTracker InsertBin Operation
    /// </summary>
    [Description("Inserts a color bin for processing/analysis.")]
    public class InsertBin : Insert<ColorBin, PortSet<DefaultInsertResponseType, Fault>>
    {
    }

    /// <summary>
    /// BlobTracker DeleteBin Operation
    /// </summary>
    [Description("Deletes a color bin for processing/analysis.")]
    public class DeleteBin : Delete<ColorBin, PortSet<DefaultDeleteResponseType, Fault>>
    {
    }

    /// <summary>
    /// BlobTracker UpdateBin Operation
    /// </summary>
    [Description("Updates a color bin for processing/analysis.")]
    public class UpdateBin : Update<ColorBin, PortSet<DefaultUpdateResponseType, Fault>>
    {
    }
}
