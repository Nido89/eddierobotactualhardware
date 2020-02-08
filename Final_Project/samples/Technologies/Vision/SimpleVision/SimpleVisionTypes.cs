//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: SimpleVisionTypes.cs $ $Revision: 2 $
//-----------------------------------------------------------------------

using Microsoft.Ccr.Core;
using Microsoft.Dss.Core.Attributes;
using Microsoft.Dss.ServiceModel.Dssp;
using System;
using System.Collections.Generic;
using W3C.Soap;

using vision = Microsoft.Robotics.Services.Sample.SimpleVision;
using System.ComponentModel;

namespace Microsoft.Robotics.Services.Sample.SimpleVision
{

    /// <summary>
    /// Vision Contract class
    /// </summary>
    public sealed class Contract
    {
        /// <summary>
        /// The Dss Service contract
        /// </summary>
        public const String Identifier = "http://schemas.microsoft.com/robotics/2007/05/simplevision.user.html";
    }

    /// <summary>
    /// The Vision State
    /// </summary>
    [DataContract()]
    [Description ("Identifies the vision service state.")]
    public class SimpleVisionState
    {
        /// <summary>
        /// Image Width
        /// </summary>
        [DataMember]
        [Description("Specifies the image width.")]
        public const int ImageWidth = 160;
        /// <summary>
        /// Image Height
        /// </summary>
        [DataMember]
        [Description("Specifies the image height.")]
        public const int ImageHeight = 120;
        /// <summary>
        /// Face Width
        /// </summary>
        [DataMember]
        [Description("Specifies the face width.")]
        public const int FaceWidth = 60;
        /// <summary>
        /// Face Height
        /// </summary>
        [DataMember]
        [Description("Specifies the face height.")]
        public const int FaceHeight = 60;
        /// <summary>
        /// Max Regions for segmentation
        /// </summary>
        [DataMember]
        [Description("Specifies maximum number of regions for segmentation.")]
        public const int MaxRegions = 5;

        /// <summary>
        /// WebCam PollingInterval In Ms
        /// </summary>
        [DataMember]
        [Description("Indicates the polling interval for the webcam service (in ms).")]
        public int WebCamPollingIntervalInMs;

        /// <summary>
        /// ColorVector of Tracking Object Color
        /// </summary>
        [DataMember]
        [Description("Specifies the color to be tracked (RGB and similarity threshold).")]
        public ColorVector TrackingObjectColor;

        /// <summary>
        /// AreaThreshold of Color Object detection
        /// </summary>
        [DataMember]
        [Description("Specifies the color object detection area threshold (minimum number of pixels).\n(Default value = 200, typical range = 50-5000)")]
        public int ColorAreaThreshold;

        /// <summary>
        /// AreaThreshold of Skin region detection
        /// </summary>

        [DataMember]
        [Description("Indicates the skin region detection area threshold (minimum number of pixels).\n(Default value = 250, typical range = 50-5000)")]
        public int SkinAreaThreshold;

        /// <summary>
        /// AreaThreshold of Head region detection
        /// </summary>
        [DataMember]
        [Description("Indicates the head region detection area threshold (minimum number of pixels).\n(Default value = 250, typical range = 50-5000)")]
        public int HeadAreaThreshold;

        /// <summary>
        /// Current Object Detection Result
        /// </summary>
        public ObjectResult CurObjectResult;
        /// <summary>
        /// Current Face Detection Result
        /// </summary>
        public FaceResult CurFaceResult;
        /// <summary>
        /// Current HandGesture Detection Result
        /// </summary>
        public HandGestureResult CurHandGestureResult;
    }

    /// <summary>
    /// Vision Main Operations Port
    /// </summary>
    ///
    [ServicePort]
    public class SimpleVisionOperations : PortSet<
        DsspDefaultLookup,
        DsspDefaultDrop,
        Get,
        Subscribe,
        SetObjectTrackingColor,
        NotifyObjectDetection,
        NotifyFaceDetection,
        NotifyHandGestureDetection
        >
    {
    }

    /// <summary>
    /// Get Operation
    /// </summary>
    [Description("Gets the current state of the vision tracking service.")]
    public class Get : Get<GetRequestType, PortSet<SimpleVisionState, Fault>>
    {
    }

    /// <summary>
    /// Subscribe Operation
    /// </summary>
    public class Subscribe : Subscribe<SubscribeRequestType, PortSet<SubscribeResponseType, Fault>>
    {
    }

    /// <summary>
    /// UpdateObjectColor Operation
    /// </summary>
    [Description("Sets the object tracking color.")]
    public class SetObjectTrackingColor : Update<ColorVector, PortSet<DefaultUpdateResponseType, Fault>>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public SetObjectTrackingColor()
        {
        }
        /// <summary>
        /// Constructor with ColorVector
        /// </summary>
        public SetObjectTrackingColor(ColorVector body)
            : base(body)
        {
        }
    }

    /// <summary>
    /// ObjectDetection Operation
    /// </summary>
    [DisplayName ("DetectColoredObject")]
    [Description("Indicates that a colored object has been detected.")]
    public class NotifyObjectDetection : Update<ObjectResult, PortSet<DefaultUpdateResponseType, Fault>>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public NotifyObjectDetection()
        {
        }
        /// <summary>
        /// Constructor with ObjectResult
        /// </summary>
        public NotifyObjectDetection(ObjectResult body)
            : base(body)
        {
        }
    }

    /// <summary>
    /// FaceDetection Operation
    /// </summary>
    [DisplayName("(User) DetectFace")]
    [Description("Indicates that a face was detected.")]
    public class NotifyFaceDetection : Update<FaceResult, PortSet<DefaultUpdateResponseType, Fault>>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public NotifyFaceDetection()
        {
        }
        /// <summary>
        /// Constructor with FaceResult
        /// </summary>
        public NotifyFaceDetection(FaceResult body)
            : base(body)
        {
        }
    }

    /// <summary>
    /// FaceDetection Operation
    /// </summary>
    [DisplayName("(User) DetectHandGesture")]
    [Description("Indicates that a hand gesture was detected.")]
    public class NotifyHandGestureDetection : Update<HandGestureResult, PortSet<DefaultUpdateResponseType, Fault>>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public NotifyHandGestureDetection()
        {
        }
        /// <summary>
        /// Constructor with HandGestureResult
        /// </summary>
        public NotifyHandGestureDetection(HandGestureResult body)
            : base(body)
        {
        }
    }

    /// <summary>
    /// ColorVector for color feature description
    /// </summary>
    [DataContract]
    public class ColorVector
    {
        /// <summary>
        /// Normalized Red Component
        /// </summary>
        [Description("Indicates the normalized Red value = Red/(Red+Green+Blue).\n(Range = 0.0-1.0)")]
        [DataMember]
        public double Red;
        /// <summary>
        /// Normalized Green Component
        /// </summary>
        [Description("Indicates the normalized Green value = Green/(Red+Green+Blue).\n(Range = 0.0-1.0)")]
        [DataMember]
        public double Green;
        /// <summary>
        /// Normalized Blue Component
        /// </summary>
        [Description("Indicates the normalized Blue value = Blue/(Red+Green+Blue).\n(Range = 0.0-1.0)")]
        [DataMember]
        public double Blue;

        /// <summary>
        /// Similarity Threshold Value
        /// </summary>
        [Description("Indicates the similarity threshold value; comparing two color vectors.\n(Typical range = 0.9~1.0)")]
        [DataMember]
        public double SimilarityMeasure;

        /// <summary>
        /// Constructor
        /// </summary>
        public ColorVector()
        {
            SimilarityMeasure = 0.995;
        }
        /// <summary>
        /// Constructor with red, green
        /// </summary>
        public ColorVector(double red, double green)
        {
            Red = red;
            Green = green;
            SimilarityMeasure = 0.995;
        }
        /// <summary>
        /// Constructor with red, green, blue
        /// </summary>
        public ColorVector(double red, double green, double blue)
        {
            Red = red;
            Green = green;
            Blue = blue;
            SimilarityMeasure = 0.995;
        }
        /// <summary>
        /// Constructor with red, green, blue, similarity
        /// </summary>
        public ColorVector(double red, double green, double blue, double similarity)
        {
            Red = red;
            Green = green;
            Blue = blue;
            SimilarityMeasure = similarity;
        }
        /// <summary>
        /// Return the Magnitude of the vector
        /// </summary>
        public double Magnitude()
        {
            return Math.Sqrt(Red * Red + Green * Green + Blue * Blue);
        }
        /// <summary>
        /// Return the Dot Product of two Vectors
        /// </summary>
        public static double DotProduct(ColorVector cv1, ColorVector cv2)
        {
            if (cv1 == null)
                return 0;
            if (cv2 == null)
                return 0;

            return (cv1.Red * cv2.Red) + (cv1.Green * cv2.Green) + (cv1.Blue * cv2.Blue);
        }
        /// <summary>
        /// Calculate the similarity between two color vectors
        /// </summary>
        public static double CompareSimilarity(ColorVector cv1, ColorVector cv2)
        {
            if (cv1 == null)
                return 0;
            if (cv2 == null)
                return 0;

            double mag = (cv1.Magnitude() * cv2.Magnitude());
            if (mag != 0)
                return (DotProduct(cv1, cv2) / mag);
            return 0;
        }
    }

    /// <summary>
    /// Color Object Detection Result
    /// </summary>
    [DataContract]
    public class ObjectResult
    {
        /// <summary>
        /// Object Found flag
        /// </summary>
        [DataMember]
        [Description ("Indicates a color object was found.")]
        public bool ObjectFound;
        /// <summary>
        /// X Mean
        /// </summary>
        [DataMember]
        [Description("Indicates the color object's X mean value.")]
        public int XMean;
        /// <summary>
        /// Y Mean
        /// </summary>
        [DataMember]
        [Description("Indicates the color object's Y mean value.")]
        public int YMean;
        /// <summary>
        /// Object Area
        /// </summary>
        [DataMember]
        [Description("Indicates the color object's area.")]
        public int Area;
        /// <summary>
        /// X StdDev
        /// </summary>
        [DataMember]
        [Description("Indicates the color object's X standard deviation value.")]
        public double XStdDev;
        /// <summary>
        /// Y StdDev
        /// </summary>
        [DataMember]
        [Description("Indicates the color object's Y standard deviation value.")]
        public double YStdDev;

        /// <summary>
        /// Motion Found flag
        /// </summary>
        [DataMember]
        [Description("Indicates object moved.")]
        public bool MotionFound;
        /// <summary>
        /// X Motion
        /// </summary>
        [DataMember]
        [Description("Indicates object movement in the X direction.")]
        public int XMotion;
        /// <summary>
        /// Y Motion
        /// </summary>
        [DataMember]
        [Description("Indicates object movement in the Y direction.")]
        public int YMotion;
        /// <summary>
        /// Motion Size
        /// </summary>
        [DataMember]
        [Description("Indicates size of the object movement.")]
        public int MotionSize;

        /// <summary>
        /// Constructor
        /// </summary>
        public ObjectResult()
        {
        }
        /// <summary>
        /// Constructor with ImageProcessingResult
        /// </summary>
        public ObjectResult(ImageProcessingResult result)
        {
            if (result == null)
                return;
            ObjectFound = result.ObjectFound;
            XMean = result.XMean;
            YMean = result.YMean;
            Area = result.Area;
            XStdDev = result.XStdDev;
            YStdDev = result.YStdDev;

            MotionFound = result.MotionFound;
            XMotion = result.XMotion;
            YMotion = result.YMotion;
            MotionSize = result.MotionSize;
        }
    }

    /// <summary>
    /// Face Detection Result
    /// </summary>
    [DataContract]
    public class FaceResult
    {
        /// <summary>
        /// Head Found flag
        /// </summary>
        [DataMember]
        [Description("Indicates a face was found.")]
        public bool HeadFound;
        /// <summary>
        /// Head Found On ColorObject flag
        /// </summary>
        [DataMember]
        [Description("Indicates a face was found on a color object.")]
        public bool HeadFoundOnColorObject;
        /// <summary>
        /// RectangleType of HeadBoxRegion
        /// </summary>
        [DataMember]
        [Description("Indicates the region rectangle of the detected face.")]
        public RectangleType HeadBoxRegion;
        /// <summary>
        /// boolean of IsFaceResultValid
        /// </summary>
        [Description("Indiates that the face result is valid.")]
        [DataMember]
        public bool IsFaceResultValid
        {
            get { return HeadFoundOnColorObject; }
            set { HeadFoundOnColorObject = value; }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public FaceResult()
        {
        }
        /// <summary>
        /// Constructor with ImageProcessingResult
        /// </summary>
        public FaceResult(ImageProcessingResult result)
        {
            if (result == null)
                return;
            HeadFound = result.HeadFound;
            HeadFoundOnColorObject = result.HeadFoundOnColorObject;
            HeadBoxRegion = result.HeadBoxRegion;
        }
    }

    /// <summary>
    /// Hand Gestures Detection Result
    /// </summary>
    [DataContract]
    public class HandGestureResult
    {
        private bool _leftHandGestureFound;
        private int _leftHandX;
        private int _leftHandY;
        private bool _rightHandGestureFound;
        private int _rightHandX;
        private int _rightHandY;
        private bool _headFound;
        private bool _objectFound;

        /// <summary>
        /// Left HandGesture Found flag
        /// </summary>
        [DataMember]
        [Description ("Indicates left hand gesture found.")]
        public bool LeftHandGestureFound
        {
            get { return _leftHandGestureFound; }
            set { _leftHandGestureFound = value; }
        }
        /// <summary>
        /// LeftHand X
        /// </summary>
        [Description("Indicates left hand X coordinate.")]
        [DataMember]
        public int LeftHandX
        {
            get { return _leftHandX; }
            set { _leftHandX = value; }
        }
        /// <summary>
        /// LeftHand Y
        /// </summary>
        [DataMember]
        [Description("Indicates left hand Y coordinate.")]
        public int LeftHandY
        {
            get { return _leftHandY; }
            set { _leftHandY = value; }
        }
        /// <summary>
        /// Right HandGesture Found flag
        /// </summary>
        [DataMember]
        [Description("Indicates right hand gesture found.")]
        public bool RightHandGestureFound
        {
            get { return _rightHandGestureFound; }
            set { _rightHandGestureFound = value; }
        }
        /// <summary>
        /// RightHand X
        /// </summary>
        [Description("Indicates right hand X coordinate.")]
        [DataMember]
        public int RightHandX
        {
            get { return _rightHandX; }
            set { _rightHandX = value; }
        }
        /// <summary>
        /// RightHand Y
        /// </summary>
        [DataMember]
        [Description("Indicates right hand Y coordinate.")]
        public int RightHandY
        {
            get { return _rightHandY; }
            set { _rightHandY = value; }
        }
        /// <summary>
        /// boolean of IsHandGestureResultValid
        /// </summary>
        [Description("Indicates if hand gesture detection result is valid.")]
        [DataMember]
        public bool IsHandGestureResultValid
        {
            get { return (_headFound || _objectFound); }
            set { _headFound = value; }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public HandGestureResult()
        {
        }
        /// <summary>
        /// Constructor with ImageProcessingResult
        /// </summary>
        public HandGestureResult(ImageProcessingResult result)
        {
            if (result == null)
                return;
            _leftHandGestureFound = result.LeftHandGestureFound;
            _leftHandX = result.LeftHandX;
            _leftHandY = result.LeftHandY;

            _rightHandGestureFound = result.RightHandGestureFound;
            _rightHandX = result.RightHandX;
            _rightHandY = result.RightHandY;

            _headFound = result.HeadFound;
            _objectFound = result.ObjectFound;
        }
    }

    /// <summary>
    /// PointType struct
    /// </summary>
    [DataContract]
    public struct PointType
    {
        /// <summary>
        /// x point
        /// </summary>
        public int x;
        /// <summary>
        /// y point
        /// </summary>
        public int y;

        /// <summary>
        /// GetHashCode
        /// </summary>
        public override int GetHashCode()
        {
            return x ^ y;
        }
        /// <summary>
        /// Equals
        /// </summary>
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (this.GetType() != obj.GetType()) return false;
            // safe because of the GetType check
            PointType cust = (PointType)obj;
            if (cust.x != x || cust.y != y) return false;
            return true;
        }
        /// <summary>
        /// operator ==
        /// </summary>
        public static bool operator ==(PointType point1, PointType point2) { return point1.Equals(point2); }
        /// <summary>
        /// operator !=
        /// </summary>
        public static bool operator !=(PointType point1, PointType point2) { return !point1.Equals(point2); }
    }

    /// <summary>
    /// RectangleType struct
    /// </summary>
    [DataContract]
    public struct RectangleType
    {
        /// <summary>
        /// left
        /// </summary>
        public int Sx;
        /// <summary>
        /// top
        /// </summary>
        public int Sy;
        /// <summary>
        /// right
        /// </summary>
        public int Ex;
        /// <summary>
        /// bottom
        /// </summary>
        public int Ey;

        /// <summary>
        /// Constructor
        /// </summary>
        public RectangleType(int sx, int sy, int ex, int ey)
        {
            Sx = sx;
            Sy = sy;
            Ex = ex;
            Ey = ey;
        }
        /// <summary>
        /// GetHashCode
        /// </summary>
        public override int GetHashCode()
        {
            return Sx ^ Sy ^ Ex ^ Ey;
        }
        /// <summary>
        /// Equals
        /// </summary>
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (this.GetType() != obj.GetType()) return false;
            // safe because of the GetType check
            RectangleType cust = (RectangleType)obj;
            if (cust.Sx != Sx || cust.Sy != Sy || cust.Ex != Ex || cust.Ey != Ey) return false;
            return true;
        }
        /// <summary>
        /// operator ==
        /// </summary>
        public static bool operator ==(RectangleType point1, RectangleType point2) { return point1.Equals(point2); }
        /// <summary>
        /// operator !=
        /// </summary>
        public static bool operator !=(RectangleType point1, RectangleType point2) { return !point1.Equals(point2); }
    }
}
