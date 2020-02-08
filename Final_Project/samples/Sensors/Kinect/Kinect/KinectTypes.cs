//------------------------------------------------------------------------------
//  <copyright file="KinectTypes.cs" company="Microsoft Corporation">
//      Copyright (C) Microsoft Corporation.  All rights reserved.
//  </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Robotics.Services.Sensors.Kinect
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using Microsoft.Ccr.Core;
    using Microsoft.Dss.Core;
    using Microsoft.Dss.Core.Attributes;
    using Microsoft.Dss.Core.DsspHttp;
    using Microsoft.Dss.ServiceModel.Dssp;
    using Microsoft.Dss.ServiceModel.DsspServiceBase;
    using W3C.Soap;

    using kinect = Microsoft.Kinect;

    /// <summary>
    /// Contract class for Kinect service
    /// </summary>
    public sealed class Contract
    {
        /// <summary>
        /// Contract Identifier
        /// </summary>
        [DataMember]
        public const string Identifier = "http://schemas.microsoft.com/robotics/2011/08/kinect.user.html";
    }

    /// <summary>     
    /// Numeric limits correspond to Kinect sensor limits
    /// </summary>
    [DataContract]
    public enum KinectReservedSampleValues
    {
        /// <summary>
        /// Maximum value for Kinect Tilt Angle
        /// </summary>
        MaximumTiltAngle = 27,

        /// <summary>
        /// Minimum value for Kinect Tilt Angle
        /// </summary>
        MinimumTiltAngle = -27
    }     

    /// <summary>
    /// State class for Kinect service
    /// </summary>    
    [DataContract]
    public class KinectState
    {
        /// <summary>
        /// Gets or sets the alternate contract polling frame rate
        /// </summary>
        [DataMember]
        public double FrameRate { get; set; }

        /// <summary>
        /// Max rate supported by Kinect
        /// </summary>
        [DataMember]
        public const double MaxFrameRate = 30;

        /// <summary>
        /// Gets or sets tilt of the camera as reported by Kinect
        /// </summary>
        [DataMember]
        public double TiltDegrees { get; set; }        
        
        /// <summary>
        /// Gets or sets device ID that can be set to identify a device if needed
        /// </summary>
        [DataMember]
        public int DeviceID { get; set; }
        
        /// <summary>
        /// Gets or sets a value indicating whether to use Color image
        /// </summary>
        [DataMember]
        public bool UseColor { get; set; }
               
        /// <summary>
        /// Gets or sets a value indicating whether to use Depth
        /// </summary>
        [DataMember]
        public bool UseDepth { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to use skeletal tracking
        /// </summary>
        [DataMember]
        public bool UseSkeletalTracking { get; set; }
                
        /// <summary>
        /// Gets or sets the color image format of the Kinect camera        
        /// </summary>        
        [DataMember]
        public kinect.ColorImageFormat ColorImageFormat { get; set; }
                
        /// <summary>
        /// Gets or sets the depth image format of the Kinect camera         
        /// </summary>
        [DataMember]
        public kinect.DepthImageFormat DepthImageFormat { get; set; }

        /// <summary>
        /// Gets or sets the range for depth values
        /// </summary>
        [DataMember]
        public kinect.DepthRange DepthStreamRange { get; set; }
                
        /// <summary>
        /// Gets or sets a value indicating whether Skeletal Data smoothing is enabled.
        /// Smoothing paramerters are defined by SkeletalEngineTransformSmoothParameters
        /// </summary>
        [DataMember]
        public bool TransformSmooth { get; set; }        

        /// <summary>
        /// Gets or sets skeletal smoothing TransformSmoothParameters
        /// </summary>
        [DataMember]
        public kinect.TransformSmoothParameters SkeletalEngineTransformSmoothParameters { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether Depth sensor service is updated on regular basis. 
        /// If set to 'false' will not process the depth image and post updates to 
        /// DepthCam alternate and improve perf. Raw depth frame can still be queried on demand
        /// </summary>
        [DataMember]
        public bool IsDepthServiceUpdateEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether webcam sensor service is updated on regular basis. 
        /// If set to 'false' will not process the video image and post updates to  WebCam alternate and improve perf. 
        /// Raw image frame can still be queried on demand
        /// </summary>
        [DataMember]
        public bool IsWebCamServiceUpdateEnabled { get; set; }        
    }

    /// <summary>
    /// Kinect Camera Operations Port
    /// </summary>
    [ServicePort]
    public class KinectOperations : 
        PortSet<DsspDefaultLookup, DsspDefaultDrop, Get, Subscribe, HttpPost, HttpGet, HttpQuery,
          QueryRawFrame, SetFrameRate, UpdateTilt, DepthToColorImage, SkeletonToDepthImage,
          SkeletonToColorImage, DepthImageToSkeleton, UpdateSkeletalSmoothing, GetDepthProperties>
    {
    }

    /// <summary>
    /// Get operation
    /// </summary>
    public class Get : Get<GetRequestType, PortSet<KinectState, Fault>>
    { 
    }

    /// <summary>
    /// Subscribe operation
    /// </summary>
    public class Subscribe : Subscribe<SubscribeRequestType, PortSet<SubscribeResponseType, Fault>>
    {
    }

    /// <summary>
    /// Response coordinates as values in the color image space.
    /// </summary>        
    [DataContract]
    public class GetDepthPropertiesResponse
    {
        /// <summary>
        /// Gets or sets maximum depth value.
        /// </summary>
        [DataMember]
        public int MaxDepthValue { get; set; }

        /// <summary>
        /// Gets or sets minimum depth value.
        /// </summary>
        [DataMember]
        public int MinDepthValue { get; set; }

        /// <summary>
        /// Gets or sets the smallest depth value that is too far from the Kinect sensor to be used.
        /// </summary>
        [DataMember]
        public int TooFarDepthValue { get; set; }

        /// <summary>
        /// Gets or sets the largest depth value that is too near to the Kinect sensor to be used.
        /// </summary>
        [DataMember]
        public int TooNearDepthValue { get; set; }

        /// <summary>
        /// Gets or sets the value returned when the depth is unknown.
        /// </summary>
        [DataMember]
        public int UnknownDepthValue { get; set; }
    }

    /// <summary>
    /// Query depth properties operation
    /// </summary>
    public class GetDepthProperties : Query<GetRequestType, PortSet<GetDepthPropertiesResponse, Fault>>
    {
    }    

    /// <summary>
    /// Returns the desired frame rate of updating depth and webcam sensor alternates
    /// </summary>
    [DataContract, DataMemberConstructor]
    public class SetFrameRateRequest
    {
        /// <summary>
        /// Gets or sets frame rate in fps
        /// </summary>
        [DataMember]
        public double FrameRate { get; set; }
    }

    /// <summary>
    /// Sets the desired frame rate of updating depth and webcam sensor alternates
    /// </summary>
    public class SetFrameRate : 
        Update<SetFrameRateRequest, PortSet<DefaultUpdateResponseType, Fault>>
    {
    }        

    /// <summary>
    /// Update Kinect camera tilt request
    /// </summary>    
    [DataContract]
    public class UpdateTiltRequest
    {
        /// <summary>
        /// Gets or sets new tilt angle in degrees to be set on the camera
        /// </summary>
        [DataMember]
        public double Tilt { get; set; }
    }

    /// <summary>
    /// UpdateTilt operation 
    /// </summary>    
    public class UpdateTilt : Update<UpdateTiltRequest, PortSet<DefaultUpdateResponseType, Fault>>
    {
    }

    /// <summary>
    /// Sets skeletal smoothing attributes that affect joint position claculation algorithms
    /// </summary>    
    [DataContract]
    public class UpdateSkeletalSmoothingRequest
    {
        /// <summary>
        /// Gets or sets a value indicating whether or not skeletal smoothing is enabled
        /// </summary>
        [DataMember]
        public bool TransfrormSmooth { get; set; }

        /// <summary>
        /// Gets or sets Smoothing parameters that affect how skeletons are tracked. 
        /// </summary>
        [DataMember]
        public kinect.TransformSmoothParameters SkeletalEngineTransformSmoothParameters { get; set; }
    }

    /// <summary>
    /// Skeletal smoothing operation
    /// </summary>    
    public class UpdateSkeletalSmoothing : 
        Update<UpdateSkeletalSmoothingRequest, PortSet<DefaultUpdateResponseType, Fault>>
    {
    }
    
    /// <summary>
    /// Raw Kinect Frame operation
    /// </summary>
    public class QueryRawFrame : 
        Query<QueryRawFrameRequest, PortSet<GetRawFrameResponse, Fault>>
    {
    }

    /// <summary>
    /// Raw Kinect frame request
    /// </summary>
    [DataContract]
    public class QueryRawFrameRequest 
    {
        /// <summary>
        /// Gets or sets a value indicating whether or not depth is to be queried from Kinect
        /// </summary>
        [DataMember]
        public bool IncludeDepth { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether or not video is to be queried from Kinect
        /// </summary>
        [DataMember]
        public bool IncludeVideo { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether or not skeletons are to be queried from Kinect
        /// </summary>
        [DataMember]
        public bool IncludeSkeletons { get; set; }
    }

    /// <summary>
    /// KinectFrame contains all the additional data for an image or depth frame
    /// </summary>
    public class KinectFrameInfo
    {
        /// <summary>
        /// Public constructor
        /// </summary>        
        public KinectFrameInfo()
        {
        }

        /// <summary>
        /// Public constructor from ImageFrame
        /// </summary>        
        /// <param name="frame">Kinect Frame</param>
        public KinectFrameInfo(kinect.ImageFrame frame)
        {
            this.BytesPerPixel = frame.BytesPerPixel;
            this.Height = frame.Height;
            this.Width = frame.Width;
            this.FrameNumber = frame.FrameNumber;
            this.Timestamp = frame.Timestamp;
        }

        /// <summary>
        /// Gets or sets number of bytes per pixel
        /// </summary>
        public int BytesPerPixel { get; set; }

        /// <summary>
        /// Gets or sets frame height
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// Gets or sets frame width
        /// </summary>        
        public int Width { get; set; }

        /// <summary>
        /// Gets or sets frame number
        /// </summary>        
        public int FrameNumber { get; set; }

        /// <summary>
        /// Gets or sets frame timestamp
        /// </summary>        
        public long Timestamp { get; set; }
    }

    /// <summary>
    /// Container class for skeleton frame data
    /// </summary>
    public class SkeletonDataFrame
    {
        /// <summary>
        /// Public constructor from SkeletonDataFrame
        /// </summary>        
        public SkeletonDataFrame()
        {
        }
                
        /// <summary>
        /// Public constructor from SkeletonDataFrame
        /// </summary>        
        /// <param name="skeletonFrame">Skeleton Frame</param>
        public SkeletonDataFrame(kinect.SkeletonFrame skeletonFrame)
        {
            this.FrameNumber = skeletonFrame.FrameNumber;            
            this.FloorClipPlane = skeletonFrame.FloorClipPlane;
            this.Timestamp = skeletonFrame.Timestamp;
            this.SkeletonData = new kinect.Skeleton[skeletonFrame.SkeletonArrayLength];
            skeletonFrame.CopySkeletonDataTo(this.SkeletonData);
        }        
        
        /// <summary>
        /// Gets or sets clip plane of the floor
        /// </summary>
        public Tuple<float, float, float, float> FloorClipPlane { get; set; }
        
        /// <summary>
        /// Gets or sets frame number
        /// </summary>    
        public int FrameNumber { get; set; }
                
        /// <summary>
        /// Gets or sets frame timestamp
        /// </summary>                
        public long Timestamp { get; set; }

        /// <summary>
        /// Gets or sets skeleton data
        /// </summary>                        
        public kinect.Skeleton[] SkeletonData { get; set; }        
    }

    /// <summary>
    /// Container for raw Kinect frame data
    /// </summary>
    [DataContract(ExcludeFromProxy = true)]
    public class RawKinectFrames : IDssSerializable
    {
        /// <summary>
        /// Gets or sets Skeltal Frame
        /// </summary>
        [DataMember]
        public SkeletonDataFrame RawSkeletonFrameData { get; set; }

        /// <summary>
        /// Gets or sets color image frame info
        /// </summary>
        [DataMember]
        public KinectFrameInfo RawColorFrameInfo { get; set; }

        /// <summary>
        /// Gets or sets color image frame data
        /// </summary>
        [DataMember]
        public byte[] RawColorFrameData { get; set; }
        
        /// <summary>
        /// Gets or sets depth frame info
        /// </summary>
        [DataMember]
        public KinectFrameInfo RawDepthFrameInfo { get; set; }

        /// <summary>
        /// Gets or sets depth frame data
        /// </summary>
        [DataMember]
        public short[] RawDepthFrameData { get; set; }
                
        /// <summary>
        /// Shallow clone
        /// </summary>
        /// <returns>A new shallow copied instance of RawKinectFrames</returns>
        public object Clone()
        {
            var copy = new RawKinectFrames();
            this.CopyTo(copy);
            return copy;
        }

        /// <summary>
        /// Shallow copy
        /// </summary>
        /// <param name="target">An instance of RawKinectFrames to be shallow copied</param>
        public void CopyTo(IDssSerializable target)
        {
            var copy = target as RawKinectFrames;
                        
            // While it is risky to not clone, its necessery to avoid copies. 
            // users that need to manipulate bits will need to make copies
            copy.RawColorFrameInfo = this.RawColorFrameInfo;
            copy.RawColorFrameData = this.RawColorFrameData;
            copy.RawDepthFrameInfo = this.RawDepthFrameInfo;            
            copy.RawDepthFrameData = this.RawDepthFrameData;            
            copy.RawSkeletonFrameData = this.RawSkeletonFrameData;                    
        }

        /// <summary>
        /// Method for raw frame deserialization
        /// </summary>
        /// <param name="reader">Binary Reader</param>
        /// <returns>Deserialized object</returns>
        public object Deserialize(BinaryReader reader)
        {
            this.DeserializeColorFrame(reader);
            this.DeserializeDepthFrame(reader); 
            this.RawSkeletonFrameData = this.DeserializeSkeletalFrame(reader);

            return this;
        }

        /// <summary>
        /// Method for raw frame serialization
        /// </summary>
        /// <param name="writer">Binary writer</param>
        public void Serialize(BinaryWriter writer)
        {
            this.SerializeColorFrame(writer);
            this.SerializeDepthFrame(writer);            
            this.SerializeSkeletalFrame(this.RawSkeletonFrameData, writer);
        }

        /// <summary>
        /// Serialize skeletal frame
        /// </summary>
        /// <param name="frame">Skeletal Frame</param>
        /// <param name="writer">Binary Writer</param>
        private void SerializeSkeletalFrame(SkeletonDataFrame frame, BinaryWriter writer)
        {
            if (frame == null)
            {
                writer.Write(0);
                return;
            }
            
            writer.Write(1);
            writer.Write(frame.FloorClipPlane.Item1);
            writer.Write(frame.FloorClipPlane.Item2);
            writer.Write(frame.FloorClipPlane.Item3);
            writer.Write(frame.FloorClipPlane.Item4);

            writer.Write(frame.FrameNumber);
            writer.Write(frame.Timestamp);

            writer.Write(frame.SkeletonData.Length);

            for (int i = 0; i < frame.SkeletonData.Length; i++)
            {
                this.SerializeSkeleton(frame.SkeletonData[i], writer);
            }          
        }

        /// <summary>
        /// Serialize a skeleton
        /// </summary>
        /// <param name="skeleton">Skeleton to serialize</param>
        /// <param name="writer">Binary writer</param>
        private void SerializeSkeleton(kinect.Skeleton skeleton, BinaryWriter writer)
        {
            writer.Write((int)skeleton.ClippedEdges);

            writer.Write(skeleton.Position.X);
            writer.Write(skeleton.Position.Y); 
            writer.Write(skeleton.Position.Z);

            writer.Write(skeleton.TrackingId);
            writer.Write((int)skeleton.TrackingState);

            writer.Write(skeleton.Joints.Count);

            foreach (kinect.Joint joint in skeleton.Joints)
            {
                writer.Write((int)joint.JointType);
                writer.Write((int)joint.TrackingState);
                writer.Write(joint.Position.X);
                writer.Write(joint.Position.Y);
                writer.Write(joint.Position.Z);
            }
        }

        /// <summary>
        /// Deserialize skeletal frame
        /// </summary>
        /// <param name="reader">Binary reader</param>
        /// <returns>Skeleton frame</returns>
        private SkeletonDataFrame DeserializeSkeletalFrame(BinaryReader reader)
        {
            if (reader.ReadInt32() == 0)
            {
                return null;
            }

            SkeletonDataFrame frame = new SkeletonDataFrame();

            float value1 = reader.ReadSingle();
            float value2 = reader.ReadSingle();
            float value3 = reader.ReadSingle();
            float value4 = reader.ReadSingle();
            frame.FloorClipPlane = Tuple.Create(value1, value2, value3, value4);
            
            frame.FrameNumber = reader.ReadInt32(); 
            frame.Timestamp = reader.ReadInt64();
            
            int numSkeletons = reader.ReadInt32();
            frame.SkeletonData = new kinect.Skeleton[numSkeletons];

            for (int i = 0; i < numSkeletons; i++)
            {
                frame.SkeletonData[i] = this.DeserializeSkeleton(reader);
            }
            
            return frame;
        }

        /// <summary>
        /// Deserialize a skeleton
        /// </summary>
        /// <param name="reader">Binary reader</param>
        /// <returns>Deserialized skeleton</returns>
        private kinect.Skeleton DeserializeSkeleton(BinaryReader reader)
        {
            kinect.Skeleton skeleton = new kinect.Skeleton();

            skeleton.ClippedEdges = (kinect.FrameEdges)reader.ReadInt32();   
            
            kinect.SkeletonPoint point = new kinect.SkeletonPoint();
            point.X = reader.ReadSingle();
            point.Y = reader.ReadSingle();
            point.Z = reader.ReadSingle();
            skeleton.Position = point;

            skeleton.TrackingId = reader.ReadInt32();
            skeleton.TrackingState = (kinect.SkeletonTrackingState)reader.ReadInt32();
        
            int jointsCount = reader.ReadInt32();

            for (int index = 0; index < jointsCount; index++) 
            {
                kinect.JointType jointType = (kinect.JointType)reader.ReadInt32();
                kinect.Joint joint = skeleton.Joints[jointType];

                joint.TrackingState = (kinect.JointTrackingState)reader.ReadInt32();

                point.X = reader.ReadSingle();
                point.Y = reader.ReadSingle();
                point.Z = reader.ReadSingle();            
                joint.Position = point;
                           
                skeleton.Joints[joint.JointType] = joint;
            }

            return skeleton;
        }

        /// <summary>
        /// Binary deserializer for Kinect color frame
        /// </summary>
        /// <param name="reader">Binary reader</param>                
        private void DeserializeColorFrame(BinaryReader reader)
        {
            if (reader.ReadInt32() == 0)
            {
                this.RawColorFrameInfo = null;
                this.RawColorFrameData = null;
                return;
            }

            this.RawColorFrameInfo = this.DeserializeFrameInfo(reader);

            if (this.RawColorFrameInfo.BytesPerPixel != 0)
            {
                int imageBytes = reader.ReadInt32();
                this.RawColorFrameData = new byte[imageBytes];
                this.RawColorFrameData = reader.ReadBytes(imageBytes);
            }
        }

        /// <summary>
        /// Binary deserializer for Kinect depth frame
        /// </summary>
        /// <param name="reader">Binary reader</param>                
        private void DeserializeDepthFrame(BinaryReader reader)
        {
            if (reader.ReadInt32() == 0)
            {
                this.RawDepthFrameInfo = null;
                this.RawDepthFrameData = null;
                return;
            }
            
            this.RawDepthFrameInfo = this.DeserializeFrameInfo(reader);

            if (this.RawDepthFrameInfo.BytesPerPixel != 0)
            {
                int depthSize = reader.ReadInt32();
                this.RawDepthFrameData = new short[depthSize];

                for (int i = 0; i < this.RawDepthFrameData.Length; i++)
                {
                    this.RawDepthFrameData[i] = reader.ReadInt16();
                }
            }
        }

        /// <summary>
        /// Binary deserializer for Kinect frame info
        /// </summary>
        /// <param name="reader">Binary reader</param>        
        /// <returns>New KinectFrame instance</returns>
        private KinectFrameInfo DeserializeFrameInfo(BinaryReader reader)
        {
            KinectFrameInfo frameInfo = new KinectFrameInfo();

            frameInfo.FrameNumber = reader.ReadInt32();                
            frameInfo.BytesPerPixel = reader.ReadInt32();
            frameInfo.Height = reader.ReadInt32();
            frameInfo.Width = reader.ReadInt32();
            frameInfo.Timestamp = reader.ReadInt64();            

            return frameInfo;
        }

        /// <summary>
        /// Binary serializaiton of Kinect color frame for cross-DSS node communicaiton
        /// </summary>                
        /// <param name="writer">Binary Writer</param>
        private void SerializeColorFrame(BinaryWriter writer)
        {
            if (this.RawColorFrameData == null)
            {
                writer.Write(0);
                return;
            }

            this.SerializeFrameInfo(this.RawColorFrameInfo, writer);
            writer.Write((int)this.RawColorFrameData.Length);
            writer.Write((byte[])this.RawColorFrameData);        
        }

        /// <summary>
        /// Binary serializaiton of Kinect depth frame for cross-DSS node communicaiton
        /// </summary>                
        /// <param name="writer">Binary Writer</param>
        private void SerializeDepthFrame(BinaryWriter writer)
        {
            if (this.RawDepthFrameData == null)
            {
                writer.Write(0);
                return;
            } 
            
            this.SerializeFrameInfo(this.RawDepthFrameInfo, writer);
            writer.Write((int)this.RawDepthFrameData.Length);
            
            for (int i = 0; i < this.RawDepthFrameData.Length; i++)
            {
                writer.Write(this.RawDepthFrameData[i]);
            }
        }
            
        /// <summary>
        /// Binary serializaiton of Kinect frame info for cross-DSS node communicaiton
        /// </summary>        
        /// <param name="frameInfo">Kinect frame info</param>
        /// <param name="writer">Binary Writer</param>
        private void SerializeFrameInfo(KinectFrameInfo frameInfo, BinaryWriter writer)
        {         
            writer.Write(1);
            writer.Write((int)frameInfo.FrameNumber);
            writer.Write((int)frameInfo.BytesPerPixel);
            writer.Write((int)frameInfo.Height);
            writer.Write((int)frameInfo.Width);                       
            writer.Write((long)frameInfo.Timestamp);            
        }        
    }

    /// <summary>
    /// GetGetRawFrame Request
    /// </summary>    
    [DataContract]
    public class GetRawFrameResponse
    {
        /// <summary>
        /// Gets or sets combined depth/vide/skeletal frames (depending on what was 
        /// requested in QueryRawFrameRequest)
        /// </summary>
        [DataMember]
        public RawKinectFrames RawFrames { get; set; }
    }

    /// <summary>
    /// Returns x and y coordinates in the color image that to a given depth point
    /// </summary>    
    [DataContract]
    public class DepthToColorRequest
    {
        /// <summary>
        /// Gets or sets X coordinate of depth pixel 
        /// </summary>
        [DataMember]
        public int X { get; set; }

        /// <summary>
        /// Gets or sets Y coordinate of depth pixel 
        /// </summary>
        [DataMember]
        public int Y { get; set; }

        /// <summary>
        /// Gets or sets depth value
        /// </summary>
        [DataMember]
        public uint Depth { get; set; }
    }

    /// <summary>
    /// Response coordinates as values in the color image space.
    /// </summary>        
    [DataContract]
    public class DepthToColorResponse
    {
        /// <summary>
        /// Gets or sets Y coordinate on color image
        /// </summary>
        [DataMember]
        public int X { get; set; }

        /// <summary>
        /// Gets or sets Y coordinate on color image
        /// </summary>
        [DataMember]
        public int Y { get; set; }
    }

    /// <summary>
    /// Returns x and y coordinates in the color image that to a given depth point
    /// </summary>        
    public class DepthToColorImage :
        Query<DepthToColorRequest, PortSet<DepthToColorResponse, Fault>>
    {
    }

    /// <summary>
    /// SkeletonToDepthImage Request
    /// </summary>    
    [DataContract]
    public class SkeletonToDepthImageRequest
    {
        /// <summary>
        /// Gets or sets joint coordinates
        /// </summary>
        [DataMember]
        public kinect.SkeletonPoint SkeletonVector { get; set; }
    }

    /// <summary>
    /// SkeletonToDepthImage Response
    /// </summary>        
    [DataContract]
    public class SkeletonToDepthImageResponse
    {
        /// <summary>
        /// Gets or sets X coordinate of depth pixel 
        /// </summary>
        [DataMember]
        public kinect.DepthImagePoint DepthPoint { get; set; }
    }

    /// <summary>
    /// Skeletal engine operation: Returns x and y coordinates in the depth image that correspond to the skeleton 
    /// position. The transformation is required because the skeleton data and the depth data are 
    /// based on different coordinate systems. 
    /// </summary>
    public class SkeletonToDepthImage : 
        Query<SkeletonToDepthImageRequest, PortSet<SkeletonToDepthImageResponse, Fault>>
    {
    }

    /// <summary>
    /// DepthImageToSkeleton Request
    /// </summary>    
    [DataContract]
    public class DepthImageToSkeletonRequest
    {
        /// <summary>
        /// Gets or sets X coordinate of depth pixel 
        /// </summary>
        [DataMember]
        public int X { get; set; }

        /// <summary>
        /// Gets or sets Y coordinate of depth pixel 
        /// </summary>
        [DataMember]
        public int Y { get; set; }

        /// <summary>
        /// Gets or sets depth value
        /// </summary>
        [DataMember]
        public short Depth { get; set; }
    }

    /// <summary>
    /// DepthImageToSkeleton Response
    /// </summary>        
    [DataContract]
    public class DepthImageToSkeletonResponse
    {
        /// <summary>
        /// Gets or sets Skeleton Vector. See documentation for Kinect SDK For 
        /// Windows on DepthImageToSkeleton()
        /// </summary>
        [DataMember]
        public kinect.SkeletonPoint SkeletonVector { get; set; }
    }

    /// <summary>
    /// Skeletal engine operation: DepthImageToSkeleton. See documentation for Kinect SDK For 
    /// Windows on DepthImageToSkeleton()
    /// </summary>
    public class DepthImageToSkeleton : 
        Query<DepthImageToSkeletonRequest, PortSet<DepthImageToSkeletonResponse, Fault>>
    {
    }
    
    /// <summary>
    /// SkeletonToColorImage Request
    /// </summary>    
    [DataContract]
    public class SkeletonToColorImageRequest
    {
        /// <summary>
        /// Gets or sets joint coordinates
        /// </summary>
        [DataMember]
        public kinect.SkeletonPoint SkeletonVector { get; set; }
    }

    /// <summary>
    /// SkeletonToColorImage Response
    /// </summary>        
    [DataContract]
    public class SkeletonToColorImageResponse
    {
        /// <summary>
        /// Gets or sets Y coordinate on color image
        /// </summary>
        [DataMember]
        public int X { get; set; }

        /// <summary>
        /// Gets or sets Y coordinate on color image
        /// </summary>
        [DataMember]
        public int Y { get; set; }
    }

    /// <summary>
    /// Skeletal engine operation: Returns x and y coordinates in the color image that correspond to the skeleton 
    /// position. The transformation is required because the skeleton data and the depth data are 
    /// based on different coordinate systems. 
    /// </summary>
    public class SkeletonToColorImage :
        Query<SkeletonToColorImageRequest, PortSet<SkeletonToColorImageResponse, Fault>>
    {
    }
}