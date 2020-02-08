//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: ColorSegmentTypes.cs $ $Revision: 2 $
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;

using Microsoft.Ccr.Core;
using Microsoft.Dss.Core.Attributes;
using Microsoft.Dss.ServiceModel.Dssp;

using W3C.Soap;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using Microsoft.Dss.Core.DsspHttp;

namespace Microsoft.Robotics.Services.Sample.ColorSegment
{
    public sealed class Contract
    {
        [DataMember]
        public const String Identifier = "http://schemas.microsoft.com/robotics/2007/07/colorsegment.user.html";
    }

    [DataContract]
    public class ColorSegmentState
    {
        private bool _processing;
        [DataMember, Browsable(false)]
        public bool Processing
        {
            get { return _processing; }
            set { _processing = value; }
        }

        private int _frameCount;
        [DataMember, Browsable(false)]
        public int FrameCount
        {
            get { return _frameCount; }
            set { _frameCount = value; }
        }

        private int _droppedFrames;
        [DataMember, Browsable(false)]
        public int DroppedFrames
        {
            get { return _droppedFrames; }
            set { _droppedFrames = value; }
        }

        private Uri _imageSource;
        [DataMember, Browsable(false)]
        public Uri ImageSource
        {
            get { return _imageSource; }
            set { _imageSource = value; }
        }

        private Settings _settings = new Settings();
        [DataMember(IsRequired = true)]
        public Settings Settings
        {
            get { return _settings; }
            set { _settings = value; }
        }

        private List<ColorSet> _colors = new List<ColorSet>();
        [DataMember(IsRequired = true)]
        public List<ColorSet> Colors
        {
            get { return _colors; }
            set { _colors = value; }
        }

        private SegmentedImage _segmentedImage;
        [DataMember, Browsable(false)]
        public SegmentedImage SegmentedImage
        {
            get { return _segmentedImage; }
            set { _segmentedImage = value; }
        }

        private FoundColorAreas _foundColorAreas;
        [DataMember, Browsable(false)]
        public FoundColorAreas FoundColorAreas
        {
            get { return _foundColorAreas; }
            set { _foundColorAreas = value; }
        }

        public ColorSegmentState SmallCopy
        {
            get
            {
                ColorSegmentState copy = new ColorSegmentState();
                copy._frameCount = _frameCount;
                copy._droppedFrames = _droppedFrames;
                copy._imageSource = _imageSource;
                copy._processing = _processing;
                copy._settings = _settings;
                copy._colors = _colors;
                copy._segmentedImage = null;
                copy._foundColorAreas = _foundColorAreas;
                return copy;
            }
        }

        int[] _colorSetMap = new int[4096];

        public int[] ColorSetMap
        {
            get
            {
                return _colorSetMap;
            }
        }

        public void UpdateColorSetMap()
        {
            double threshold = _settings.Threshold;

            for (int i = 0; i < 4096; i++)
            {
                int red = (i & 0xF00) >> 8;
                int grn = (i & 0x0F0) >> 4;
                int blu = (i & 0x00F);

                red |= (red << 4);
                grn |= (grn << 4);
                blu |= (blu << 4);


                ColorDefinition curr = new ColorDefinition(red, grn, blu, "palette");

                ColorDefinition best = null;
                double bestDistance = 0;
                int bestIndex = 0;

                int index = 0;
                foreach (ColorSet set in _colors)
                {
                    index++;

                    double distance;
                    double setInverse = 1;

                    foreach (ColorDefinition color in set.Colors)
                    {
                        distance = color.Distance(curr, threshold);

                        setInverse *= (1 - distance);

                        if (setInverse <= 0)
                        {
                            break;
                        }
                    }

                    if (setInverse <= 0)
                    {
                        best = set.Colors[0];
                        bestDistance = 1;
                        bestIndex = index;
                        break;
                    }

                    distance = 1 - setInverse;

                    if (distance > bestDistance)
                    {
                        best = set.Colors[0];
                        bestDistance = distance;
                        //bestIndex = index;
                    }
                }
                _colorSetMap[i] = bestIndex;
            }
        }

    }

    [DataContract]
    public class Settings : ICloneable
    {
        public Settings()
        {
        }

        public Settings(double threshold, bool showPartialMatches, bool despeckle, int minBlobSize)
        {
            _threshold = threshold;
            _showPartialMatches = showPartialMatches;
            _despeckle = despeckle;
            _minBlobSize = minBlobSize;
        }

        private double _threshold = 1.0;
        [DataMember, DataMemberConstructor]
        public double Threshold
        {
            get { return _threshold; }
            set { _threshold = value; }
        }

        private bool _showPartialMatches = true;
        [DataMember, DataMemberConstructor]
        public bool ShowPartialMatches
        {
            get { return _showPartialMatches; }
            set { _showPartialMatches = value; }
        }

        private bool _despeckle;
        [DataMember, DataMemberConstructor]
        public bool Despeckle
        {
            get { return _despeckle; }
            set { _despeckle = value; }
        }

        private int _minBlobSize = 100;
        [DataMember, DataMemberConstructor]
        public int MinBlobSize
        {
            get { return _minBlobSize; }
            set { _minBlobSize = value; }
        }

        #region ICloneable Members

        public object Clone()
        {
            return new Settings(_threshold, _showPartialMatches, _despeckle, _minBlobSize);
        }

        #endregion
    }

    [DataContract]
    public class ColorSet
    {
        private string _name;
        [DataMember]
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        private List<ColorDefinition> _colors = new List<ColorDefinition>();
        [DataMember(IsRequired = true)]
        public List<ColorDefinition> Colors
        {
            get { return _colors; }
            set { _colors = value; }
        }
    }

    [DataContract]
    public class ColorDefinition
    {
        public ColorDefinition()
        {
        }

        public ColorDefinition(string name, int y, int cb, int cr)
        {
            _name = name;

            _y = y;
            _cb = cb;
            _cr = cr;

            Validate();
        }

        public ColorDefinition(int r, int g, int b, string name)
        {
            _name = name;

            _r = r;
            _g = g;
            _b = b;

            Validate();
        }

        private string _name;
        [DataMember,DataMemberConstructor]
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        private int _y;
        [DataMember,DataMemberConstructor]
        public int Y
        {
            get { return _y; }
            set { _y = value; }
        }

        private int _cb;
        [DataMember,DataMemberConstructor]
        public int Cb
        {
            get { return _cb; }
            set { _cb = value; }
        }

        private int _cr;
        [DataMember,DataMemberConstructor]
        public int Cr
        {
            get { return _cr; }
            set { _cr = value; }
        }

        private int _sigmaY;
        [DataMember]
        public int SigmaY
        {
            get { return _sigmaY; }
            set { _sigmaY = value; }
        }

        private int _sigmaCb;
        [DataMember]
        public int SigmaCb
        {
            get { return _sigmaCb; }
            set { _sigmaCb = value; }
        }

        private int _sigmaCr;
        [DataMember]
        public int SigmaCr
        {
            get { return _sigmaCr; }
            set { _sigmaCr = value; }
        }

        private int _r;
        [DataMember]
        public int R
        {
            get { return _r; }
            set { _r = value; }
        }

        private int _g;
        [DataMember]
        public int G
        {
            get { return _g; }
            set { _g = value; }
        }

        private int _b;
        [DataMember]
        public int B
        {
            get { return _b; }
            set { _b = value; }
        }

        public bool Validate()
        {
            if (_y <= 0 && _cb <= 0 && _cr <= 0)
            {
                double r = _r / 256.0;
                double g = _g / 256.0;
                double b = _b / 256.0;

                _y = (int)(16.5 + r * 65.481 + g * 128.553 + b * 24.966);
                _cb = (int)(128.5 + r * -37.797 + g * -74.203 + b * 112.000);
                _cr = (int)(128.5 + r * 112.000 + g * -93.786 + b * -18.214);
            }
            else
            {
                int y = _y - 16;
                int cb = _cb - 128;
                int cr = _cr - 128;

                _r = (int)((298.082 * y /* + 0.0 * cb */ + 408.583 * cr) / 256 + 0.5);
                _g = (int)((298.082 * y - 100.291 * cb - 208.120 * cr) / 256 + 0.5);
                _b = (int)((298.082 * y + 516.411 * cb /* + 0.0 * cr */) / 256 + 0.5);

            }
            if (_sigmaY <= 0 || _sigmaCb <= 0 || _sigmaCr <= 0)
            {
                return false;
            }
            return !string.IsNullOrEmpty(_name);
        }

        public static ColorDefinition FromKnownColor(KnownColor knownColor)
        {
            Color color = Color.FromKnownColor(knownColor);
            return FromColor(color, color.Name);
        }

        public static ColorDefinition FromColor(Color color, string name)
        {
            ColorDefinition definition = new ColorDefinition();

            definition._name = name;

            definition._r = color.R;
            definition._g = color.G;
            definition._b = color.B;

            definition.Validate();

            return definition;
        }

        public double Distance(ColorDefinition pixel, double threshold)
        {
            double dy = (pixel._y - _y) / (double)_sigmaY;
            double dcb = (pixel._cb - _cb) / (double)_sigmaCb;
            double dcr = (pixel._cr - _cr) / (double)_sigmaCr;

            double square = (dy * dy + dcb * dcb + dcr * dcr) - threshold;
            if (square <= 0)
            {
                return 1;
            }
            else
            {
                return Math.Exp(-square / 2);
            }
        }

        // e^-(1/2)
        // 0.60653065971263342360379953499118;
        public static readonly double OneSigma = Math.Exp(-0.5);
        // e^-(4/2)
        // 0.13533528323661269189399949497248;
        public static readonly double TwoSigma = Math.Exp(-2);
        // e^-(9/2)
        // 0.011108996538242306496143134286931;
        public static readonly double ThreeSigma = Math.Exp(-4.5);

        public bool Compare(ColorDefinition other)
        {
            return other._name == _name;
        }

        public bool Compare(ColorSet other)
        {
            return other.Name == _name;
        }

        public bool CompareColor(ColorDefinition other)
        {
            return other._name == _name &&
                other._y == _y &&
                other._cb == _cb &&
                other._cr == _cr;
        }
    }

    [DataContract]
    public class ProcessFrameRequest
    {
        public ProcessFrameRequest()
        {
        }

        public ProcessFrameRequest(bool process)
        {
            _process = process;
        }

        private bool _process;
        [DataMember,DataMemberConstructor]
        public bool Process
        {
            get { return _process; }
            set { _process = value; }
        }
    }

    [DataContract]
    public class SegmentedImage
    {
        public SegmentedImage()
        {
            _timeStamp = DateTime.UtcNow;
        }

        public SegmentedImage(DateTime timeStamp, int width, int height, byte[] frame, byte[,] segmented)
        {
            _timeStamp = timeStamp.ToUniversalTime();
            _width = width;
            _height = height;
            _frame = frame;

            _segmented = new byte[width * height];

            int offset = 0;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    _segmented[offset++] = segmented[x,y];
                }
            }
        }

        private DateTime _timeStamp;
        [DataMember]
        public DateTime TimeStamp
        {
            get { return _timeStamp; }
            set { _timeStamp = value; }
        }

        private int _width;
        [DataMember, DataMemberConstructor]
        public int Width
        {
            get { return _width; }
            set { _width = value; }
        }

        private int _height;
        [DataMember, DataMemberConstructor]
        public int height
        {
            get { return _height; }
            set { _height = value; }
        }

        private byte[] _frame;

        public byte[] Frame
        {
            get { return _frame; }
            set { _frame = value; }
        }

        private byte[] _segmented;
        [DataMember]
        public byte[] Segmented
        {
            get { return _segmented; }
            set { _segmented = value; }
        }

        public Bitmap Bitmap
        {
            get
            {
                Bitmap bmp = new Bitmap(_width, _height,PixelFormat.Format24bppRgb);
                BitmapData bits = bmp.LockBits(
                    new Rectangle(0, 0, _width, _height),
                    ImageLockMode.WriteOnly,
                    PixelFormat.Format24bppRgb
                );
                Marshal.Copy(_frame, 0, bits.Scan0, _frame.Length);
                bmp.UnlockBits(bits);
                return bmp;
            }
        }
    }

    [DataContract]
    public class FilteredSubscribeRequest : SubscribeRequestType
    {
        private Filter _filter;
        [DataMember]
        public Filter Filter
        {
            get { return _filter; }
            set { _filter = value; }
        }
    }

    [DataContract, Flags]
    public enum Filter
    {
        None = 0,
        ColorDefinitions = 0x1,
        SegmentedImage = 0x2,
        Settings = 0x4,
        ColorAreas = 0x8,
        Internal = 0x10,
        All = 0x1F
    }

    [DataContract]
    public class FoundColorAreas
    {
        private DateTime _timeStamp;
        [DataMember]
        public DateTime TimeStamp
        {
            get { return _timeStamp; }
            set { _timeStamp = value; }
        }

        private List<ColorArea> _area = new List<ColorArea>();
        [DataMember(IsRequired = true)]
        public List<ColorArea> Areas
        {
            get { return _area; }
            set { _area = value; }
        }
    }

    [DataContract]
    public class ColorArea
    {
        public ColorArea()
        {
        }

        public ColorArea(string name)
        {
            _name = name;
        }

        private string _name;
        [DataMember,DataMemberConstructor]
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        private int _centerX;
        [DataMember]
        public int CenterX
        {
            get { return _centerX; }
            set { _centerX = value; }
        }

        private int _centerY;
        [DataMember]
        public int CenterY
        {
            get { return _centerY; }
            set { _centerY = value; }
        }

        private int _minX = int.MaxValue;
        [DataMember]
        public int MinX
        {
            get { return _minX; }
            set { _minX = value; }
        }

        private int _maxX = int.MinValue;
        [DataMember]
        public int MaxX
        {
            get { return _maxX; }
            set { _maxX = value; }
        }

        private int _minY = int.MaxValue;
        [DataMember]
        public int MinY
        {
            get { return _minY; }
            set { _minY = value; }
        }

        private int _maxY = int.MinValue;
        [DataMember]
        public int MaxY
        {
            get { return _maxY; }
            set { _maxY = value; }
        }

        private int _area;
        [DataMember]
        public int Area
        {
            get { return _area; }
            set { _area = value; }
        }


        internal void Complete()
        {
            if (_area <= 0)
            {
                _area = 0;
                _centerX = 0;
                _centerY = 0;
            }
            else
            {
                _centerX = _xAccum / _area;
                _centerY = _yAccum / _area;
            }
        }

        internal void AddPixel(int x, int y)
        {
            _xAccum += x;
            _yAccum += y;
            _area++;

            if (x < _minX)
            {
                _minX = x;
            }
            if (x > _maxX)
            {
                _maxX = x;
            }
            if (y < _minY)
            {
                _minY = y;
            }
            if (y > _maxY)
            {
                _maxY = y;
            }
        }

        int _xAccum;
        int _yAccum;

        internal void Flood(byte[,] indexed, int startX, int startY)
        {
            int color = indexed[startX, startY];
            Stack<Point> stack = new Stack<Point>();

            stack.Push(new Point(startX, startY));

            while (stack.Count > 0)
            {
                Point curr = stack.Pop();
                int x = curr.X;
                int y = curr.Y;

                indexed[x, y] = 0;
                AddPixel(x, y);

                if (x > 0 && indexed[x - 1, y] == color)
                {
                    stack.Push(new Point(x - 1, y));
                }
                if (x < indexed.GetLength(0) - 1 && indexed[x + 1,y] == color)
                {
                    stack.Push(new Point(x + 1, y));
                }
                if (y > 0 && indexed[x, y - 1] == color)
                {
                    stack.Push(new Point(x, y - 1));
                }
                if (y < indexed.GetLength(1) - 1 && indexed[x, y + 1] == color)
                {
                    stack.Push(new Point(x, y + 1));
                }
            }
            Complete();
        }
    }

    [ServicePort]
    public class ColorSegmentOperations : PortSet<
        DsspDefaultLookup, DsspDefaultDrop, Get,
        ProcessFrame, UpdateSegmentedImage, UpdateColorAreas,
        FilteredSubscribe, Subscribe,
        AddColorDefinition, RemoveColorDefinition, UpdateColorDefinition, FindColorDefinition,
        HttpGet, HttpQuery, HttpPost,
        UpdateSettings>
    {
    }

    public class Get : Get<GetRequestType, PortSet<ColorSegmentState, Fault>>
    {
    }

    public class ProcessFrame : Update<ProcessFrameRequest, DsspResponsePort<DefaultUpdateResponseType>>
    {
        public ProcessFrame()
        {
        }

        public ProcessFrame(bool processing)
            : base(new ProcessFrameRequest(processing), null)
        {
        }
    }

    public class UpdateSegmentedImage : Update<SegmentedImage, DsspResponsePort<DefaultUpdateResponseType>>
    {
        public UpdateSegmentedImage()
        {
        }

        public UpdateSegmentedImage(SegmentedImage body)
            : base(body)
        {
        }
    }

    public class UpdateColorAreas : Update<FoundColorAreas, DsspResponsePort<DefaultUpdateResponseType>>
    {
        public UpdateColorAreas()
        {
        }

        public UpdateColorAreas(FoundColorAreas body)
            : base(body)
        {
        }
    }

    public class AddColorDefinition : Insert<ColorDefinition, DsspResponsePort<DefaultInsertResponseType>>
    {
        public AddColorDefinition()
        {
        }

        public AddColorDefinition(ColorDefinition body)
            : base(body)
        {
        }
    }

    public class RemoveColorDefinition : Delete<ColorDefinition, DsspResponsePort<DefaultDeleteResponseType>>
    {
        public RemoveColorDefinition()
        {
        }

        public RemoveColorDefinition(ColorDefinition body)
            : base(body)
        {
        }
    }

    public class UpdateColorDefinition : Update<ColorDefinition, DsspResponsePort<DefaultUpdateResponseType>>
    {
        public UpdateColorDefinition()
        {
        }

        public UpdateColorDefinition(ColorDefinition body)
            : base(body)
        {
        }
    }

    public class FindColorDefinition : Query<ColorDefinition, DsspResponsePort<ColorSet>>
    {
        public FindColorDefinition()
        {
        }

        public FindColorDefinition(ColorDefinition body)
            : base(body)
        {
        }
    }

    public class UpdateSettings : Update<Settings, DsspResponsePort<DefaultUpdateResponseType>>
    {
        public UpdateSettings()
        {
        }

        public UpdateSettings(Settings body)
            : base(body)
        {
        }
    }

    public class Subscribe : Subscribe<SubscribeRequestType, DsspResponsePort<SubscribeResponseType>>
    {
    }

    public class FilteredSubscribe : Subscribe<FilteredSubscribeRequest, DsspResponsePort<SubscribeResponseType>>
    {
    }
}
