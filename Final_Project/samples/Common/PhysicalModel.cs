//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: PhysicalModel.cs $ $Revision: 43 $
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using Microsoft.Dss.Core.Attributes;

namespace Microsoft.Robotics.PhysicalModel
{
    /// <summary>
    /// Physical Model Contract
    /// </summary>
    public static class Contract
    {
        /// Contract Identifier
        public const string Identifier = "http://schemas.microsoft.com/robotics/2006/07/physicalmodel.html";
    }

    #region Basic Types


    /// <summary>
    /// Stores the red, green, blue, and alpha channel values that together define a specific color.
    /// </summary>
    [DataContract]
    [DataMemberConstructor]
#if !URT_MINCLR
    [TypeConverter(typeof(ColorValueConverter))]
#endif
    public struct ColorValue
    {
        private float _red;
        private float _green;
        private float _blue;
        private float _alpha;

        /// <summary>Initializes a new instance of the ColorValue structure.</summary>
        public ColorValue(float r, float g, float b)
        {
            _red = r;
            _green = g;
            _blue = b;
            _alpha = 1;
        }

        /// <summary>Initializes a new instance of the ColorValue structure.</summary>
        public ColorValue(int r, int g, int b)
        {
            _red = r / 255f;
            _green = g / 255f;
            _blue = b / 255f;
            _alpha = 1;
        }

        /// <summary>Initializes a new instance of the ColorValue structure.</summary>
        public ColorValue(float r, float g, float b, float a)
        {
            _red = r;
            _green = g;
            _blue = b;
            _alpha = a;
        }

        /// <summary>Initializes a new instance of the ColorValue structure.</summary>
        public ColorValue(int r, int g, int b, int a)
        {
            _red = r / 255f;
            _green = g / 255f;
            _blue = b / 255f;
            _alpha = a / 255f;
        }

        /// <summary>Retrieves or sets the red value of the current color.</summary>
        [DataMember]
        public float Red { get { return _red; } set { _red = value; } }
        /// <summary>Retrieves or sets the green value of the current color.</summary>
        [DataMember]
        public float Green { get { return _green; } set { _green = value; } }
        /// <summary>Retrieves or sets the blue value of the current color.</summary>
        [DataMember]
        public float Blue { get { return _blue; } set { _blue = value; } }
        /// <summary>Retrieves or sets the alpha channel value of the current color.</summary>
        [DataMember]
        public float Alpha { get { return _alpha; } set { _alpha = value; } }

        /// <summary>Explicity convert a color to a four component vector as Red, Green, Blue, Alpha.</summary>
        public static explicit operator Vector4(ColorValue color)
        {
            return new Vector4(color.Red, color.Green, color.Blue, color.Alpha);
        }

        /// <summary>Explicity convert a four component vector to a ColorValue as X, Y, Z, W.</summary>
        public static explicit operator ColorValue(Vector4 vector)
        {
            return new ColorValue(vector.X, vector.Y, vector.Z, vector.W);
        }

        /// <summary>Explicity convert a ColorValue to a four System.Drawing.Color. Be careful of saturation.</summary>
        public static explicit operator System.Drawing.Color(ColorValue colorValue)
        {
            return System.Drawing.Color.FromArgb(
                ((int)Math.Min(Math.Max(colorValue.Alpha * 255, 0), 255) << 24) +
                ((int)Math.Min(Math.Max(colorValue.Red * 255, 0), 255) << 16) +
                ((int)Math.Min(Math.Max(colorValue.Green * 255, 0), 255) << 8) +
                (int)Math.Min(Math.Max(colorValue.Blue * 255, 0), 255));
        }

        /// <summary>Explicity convert a System.Drawing.Color to a ColorValue.</summary>
        public static explicit operator ColorValue(System.Drawing.Color color)
        {
            return new ColorValue(color.R, color.G, color.B, color.A);
        }

        /// <summary>Get color properties in string form.</summary>
        public override string ToString()
        {
            return string.Format("{0:.00} {1.00} {2.00} {3.00}", Red, Green, Blue, Alpha);
        }
    }

    /// <summary>
    /// Vector3
    /// </summary>
    [DataContract]
    [DataMemberConstructor]
#if !URT_MINCLR
    [TypeConverter(typeof(Vector3Converter))]
#endif
    public struct Vector3
    {
        /// <summary>
        /// Vector3 Constructor
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public Vector3(float x, float y, float z)
        {
            _x = x;
            _y = y;
            _z = z;
        }

        private float _x;
        private float _y;
        private float _z;

        /// <summary>
        /// X
        /// </summary>
        [DataMember]
        [Description("The X component of the vector.")]
        public float X
        {
            get { return this._x; }
            set { this._x = value; }
        }

        /// <summary>
        /// Y
        /// </summary>
        [DataMember]
        [Description("The Y component of the vector.")]
        public float Y
        {
            get { return this._y; }
            set { this._y = value; }
        }

        /// <summary>
        /// Z
        /// </summary>
        [DataMember]
        [Description("The Z component of the vector.")]
        public float Z
        {
            get { return this._z; }
            set { this._z = value; }
        }

        /// <summary>
        /// Returns a vector pointing in the opposite direction.
        /// </summary>
        /// <param name="value">Source vector.</param>
        /// <returns>Vector pointing in the opposite direction.</returns>
        public static Vector3 operator -(Vector3 value)
        {
            return new Vector3(-value.X, -value.Y, -value.Z);
        }

        /// <summary>
        /// Vector3 operator +
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static Vector3 operator +(Vector3 left, Vector3 right)
        {
            return new Vector3(left.X + right.X,
                left.Y + right.Y,
                left.Z + right.Z);
        }

        /// <summary>
        /// Vector3 operator -
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static Vector3 operator -(Vector3 left, Vector3 right)
        {
            return new Vector3(left.X - right.X,
                left.Y - right.Y,
                left.Z - right.Z);
        }

        /// <summary>
        /// Get the magnitude of the vector
        /// </summary>
        public static float Length(Vector3 v)
        {
            return (float)Math.Sqrt(v.X * v.X + v.Y * v.Y + v.Z * v.Z);
        }

        /// <summary>
        /// Normalize a vector to a unit vector
        /// </summary>
        public static Vector3 Normalize(Vector3 v)
        {
            float mag = Vector3.Length(v);
            Vector3 unitv = new Vector3();

            //avoid division by 0
            if (mag != 0)
            {
                unitv.X = v.X / mag;
                unitv.Y = v.Y / mag;
                unitv.Z = v.Z / mag;
            }
            else
            {
                unitv.X = v.X;
                unitv.Y = v.Y;
                unitv.Z = v.Z;
            }
            return unitv;
        }

        /// <summary>
        /// Scale
        /// </summary>
        /// <param name="v"></param>
        /// <param name="factor"></param>
        /// <returns></returns>
        public static Vector3 Scale(Vector3 v, float factor)
        {
            return new Vector3(v.X * factor, v.Y * factor, v.Z * factor);
        }

        /// <summary>
        /// Dot
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static float Dot(Vector3 left, Vector3 right)
        {
            return left.X * right.X + left.Y * right.Y + left.Z * right.Z;
        }

        /// <summary>
        /// Vector3 Cross
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static Vector3 Cross(Vector3 left, Vector3 right)
        {
            Vector3 v = new Vector3();
            v.X = left.Y * right.Z - left.Z * right.Y;
            v.Y = left.Z * right.X - left.X * right.Z;
            v.Z = left.X * right.Y - left.Y * right.X;
            return v;
        }

        /// <summary>
        /// Retrieves a string representation of the Vector3.
        /// </summary>
        /// <returns>String that represents the Vector3.</returns>
        public override string ToString()
        {
            return string.Format("X:{0} Y:{1} Z:{2}", X, Y, Z);
        }

        /// <summary>
        /// Returns a vector that represents the +X axis.
        /// </summary>
        /// <returns></returns>
        public static Vector3 XAxis
        {
            get { return new Vector3(1, 0, 0); }
        }
        
        /// <summary>
        /// Returns a vector that represents the -X axis.
        /// </summary>
        /// <returns></returns>
        public static Vector3 NegativeXAxis
        {
            get { return new Vector3(-1, 0, 0); }
        }

        /// <summary>
        /// Returns a vector that represents the +Y axis.
        /// </summary>
        /// <returns></returns>
        public static Vector3 YAxis
        {
            get { return new Vector3(0, 1, 0); }
        }

        /// <summary>
        /// Returns a vector that represents the -Y axis.
        /// </summary>
        /// <returns></returns>
        public static Vector3 NegativeYAxis
        {
            get { return new Vector3(0, -1, 0); }
        }

        /// <summary>
        /// Returns a vector that represents the +Z axis.
        /// </summary>
        /// <returns></returns>
        public static Vector3 ZAxis
        {
            get { return new Vector3(0, 0, 1); }
        }

        /// <summary>
        /// Returns a vector that represents the -Z axis.
        /// </summary>
        /// <returns></returns>
        public static Vector3 NegativeZAxis
        {
            get { return new Vector3(0, 0, -1); }
        }
    }

    /// <summary>
    /// Vector4
    /// </summary>
    [DataContract]
    [DataMemberConstructor]
#if !URT_MINCLR
    [TypeConverter(typeof(Vector4Converter))]
#endif
    public struct Vector4
    {
        /// <summary>
        /// Vector4 Constructor
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="w"></param>
        public Vector4(float x, float y, float z, float w)
        {
            _x = x;
            _y = y;
            _z = z;
            _w = w;
        }

        private float _x;
        private float _y;
        private float _z;
        private float _w;

        /// <summary>
        /// X
        /// </summary>
        [DataMember]
        [Description("The X component of the vector.")]
        public float X
        {
            get { return this._x; }
            set { this._x = value; }
        }

        /// <summary>
        /// Y
        /// </summary>
        [DataMember]
        [Description("The Y component of the vector.")]
        public float Y
        {
            get { return this._y; }
            set { this._y = value; }
        }

        /// <summary>
        /// Z
        /// </summary>
        [DataMember]
        [Description("The Z component of the vector.")]
        public float Z
        {
            get { return this._z; }
            set { this._z = value; }
        }

        /// <summary>
        /// W
        /// </summary>
        [DataMember]
        [Description("The W component of the vector.")]
        public float W
        {
            get { return this._w; }
            set { this._w = value; }
        }

        /// <summary>
        /// Vector4 + Operator
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static Vector4 operator +(Vector4 left, Vector4 right)
        {
            return new Vector4(left.X + right.X,
                left.Y + right.Y,
                left.Z + right.Z,
                left.W + right.W);
        }

        /// <summary>
        /// Retrieves a string representation of the Vector4.
        /// </summary>
        /// <returns>String that represents the Vector4.</returns>
        public override string ToString()
        {
            return string.Format("X:{0} Y:{1} Z:{2} W:{3}", X, Y, Z, W);
        }
    }

    /// <summary>
    /// Vector2
    /// </summary>
    [DataContract]
    [DataMemberConstructor]
    public struct Vector2
    {
        /// <summary>
        /// Vector2 Constructor
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public Vector2(float x, float y)
        {
            _x = x;
            _y = y;
        }

        private float _x;
        private float _y;

        /// <summary>
        /// X
        /// </summary>
        [DataMember]
        [Description("The X dimension.")]
        public float X
        {
            get { return this._x; }
            set { this._x = value; }
        }

        /// <summary>
        /// Y
        /// </summary>
        [DataMember]
        [Description("The Y dimension.")]
        public float Y
        {
            get { return this._y; }
            set { this._y = value; }
        }

        /// <summary>
        /// Retrieves a string representation of the Vector2.
        /// </summary>
        /// <returns>String that represents the Vector2.</returns>
        public override string ToString()
        {
            return string.Format("X:{0} Y:{1}", X, Y);
        }
    }

    /// <summary>
    /// Ray
    /// </summary>
    [DataContract]
    public struct Ray
    {
        /// <summary>
        /// Starting point of the ray.
        /// </summary>
        [DataMember]
        public Vector3 Position;

        /// <summary>
        /// Unit vector specifying the direction the Ray is pointing.
        /// </summary>
        [DataMember]
        public Vector3 Direction;

        /// <summary>
        /// Creates a new instance of a Ray
        /// </summary>
        /// <param name="position">Specifies the starting point of the Ray.</param>
        /// <param name="direction">Unit vector describing the direction of the Ray.</param>
        public Ray(Vector3 position, Vector3 direction)
        {
            Position = position;
            Direction = direction;
        }
    }

    /// <summary>
    /// Quaternion
    /// </summary>
    [DataContract]
    [DataMemberConstructor]
    public struct Quaternion
    {

        /// <summary>
        /// Quaternion Constructor
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="w"></param>
        public Quaternion(float x, float y, float z, float w)
        {
            _x = x;
            _y = y;
            _z = z;
            _w = w;
        }

        private float _x;
        private float _y;
        private float _z;
        private float _w;

        /// <summary>
        /// X
        /// </summary>
        [DataMember]
        [Description("The X component of the quaternion.")]
        public float X
        {
            get { return this._x; }
            set { this._x = value; }
        }

        /// <summary>
        /// Y
        /// </summary>
        [DataMember]
        [Description("The Y component of the quaternion.")]
        public float Y
        {
            get { return this._y; }
            set { this._y = value; }
        }

        /// <summary>
        /// Z
        /// </summary>
        [DataMember]
        [Description("The Z component of the quaternion.")]
        public float Z
        {
            get { return this._z; }
            set { this._z = value; }
        }

        /// <summary>
        /// W
        /// </summary>
        [DataMember]
        [Description("The W component of the quaternion.")]
        public float W
        {
            get { return this._w; }
            set { this._w = value; }
        }

        /// <summary>
        /// Multiplies two quaternions.
        /// </summary>
        /// <param name="lhs">Left hand side.</param>
        /// <param name="rhs">Right hand side.</param>
        /// <returns>The result of the multiplication.</returns>
        public static Quaternion operator *(Quaternion lhs, Quaternion rhs)
        {
            return Quaternion.Multiply(lhs, rhs);
        }

        /// <summary>
        /// Subtracts a quaternion from another quaternion.
        /// </summary>
        /// <param name="lhs">Left hand side.</param>
        /// <param name="rhs">Right hand side.</param>
        /// <returns>Result of the subtraction.</returns>
        public static Quaternion operator -(Quaternion lhs, Quaternion rhs)
        {
            return new Quaternion(lhs.X - rhs.X, lhs.Y - rhs.Y, lhs.Z - rhs.Z, lhs.W - rhs.W);
        }

        /// <summary>
        /// Get the magnitude of the quaternion
        /// </summary>
        public static float Length(Quaternion q)
        {
            return (float)Math.Sqrt(q.X * q.X + q.Y * q.Y + q.Z * q.Z + q.W * q.W);
        }

        /// <summary>
        /// Normalize a quaternion to a unit quaternion
        /// </summary>
        public static Quaternion Normalize(Quaternion q)
        {
            float mag = Quaternion.Length(q);
            Quaternion unitq = new Quaternion();

            if (mag != 0)
            {
                unitq.X = q.X / mag;
                unitq.Y = q.Y / mag;
                unitq.Z = q.Z / mag;
                unitq.W = q.W / mag;
            }
            else
            {
                unitq.X = q.X;
                unitq.Y = q.Y;
                unitq.Z = q.Z;
                unitq.W = q.W;
            }
            return unitq;
        }

        /// <summary>
        /// Create a quaternion given axis and angle
        /// </summary>
        /// <param name="x">X component of axis</param>
        /// <param name="y">Y component of axis</param>
        /// <param name="z">Z component of axis</param>
        /// <param name="angle">Angle in radians</param>
        /// <returns>Quaternion representing given axis angle rotation</returns>
        public static Quaternion FromAxisAngle(float x, float y, float z, float angle)
        {
            float qx, qy, qz, qw;

            qw = (float)Math.Cos(angle / 2);
            qx = x * (float)Math.Sin(angle / 2);
            qy = y * (float)Math.Sin(angle / 2);
            qz = z * (float)Math.Sin(angle / 2);

            Quaternion q = new Quaternion(qx, qy, qz, qw);

            return Quaternion.Normalize(q);
        }

        /// <summary>
        /// Convert an AxisAngle to a quaternion
        /// </summary>
        public static Quaternion FromAxisAngle(AxisAngle a)
        {
            if (Vector3.Length(a.Axis) > 1)
                a.Axis = Vector3.Normalize(a.Axis);

            Quaternion q = new Quaternion();
            q.X = (float)Math.Sin(a.Angle / 2) * a.Axis.X;
            q.Y = (float)Math.Sin(a.Angle / 2) * a.Axis.Y;
            q.Z = (float)Math.Sin(a.Angle / 2) * a.Axis.Z;
            q.W = (float)Math.Cos(a.Angle / 2);
            return q;
        }

        /// <summary>
        /// Convert a quaternion to an AxisAngle
        /// </summary>
        public static AxisAngle ToAxisAngle(Quaternion q)
        {
            //needs to normalize for Acos
            if (q.W > 1)
                q = Quaternion.Normalize(q);

            AxisAngle a = new AxisAngle();
            a.Angle = (float)Math.Acos(q.W) * 2;
            float s = (float)Math.Sqrt(1 - q.W * q.W);

            //avoid division by 0
            if (s == 0)
            {
                a.Axis.X = q.X;
                a.Axis.Y = q.Y;
                a.Axis.Z = q.Z;
            }
            else
            {
                a.Axis.X = q.X / s;
                a.Axis.Y = q.Y / s;
                a.Axis.Z = q.Z / s;
            }
            return a;
        }

        /// <summary>
        /// Multiply q1 by q2. The order of the multiplication is important.
        /// Use Quaternion.Normalize to get unit quaternions
        /// </summary>
        public static Quaternion Multiply(Quaternion q1, Quaternion q2)
        {
            Quaternion q = new Quaternion();
            q.W = q1.W * q2.W - q1.X * q2.X - q1.Y * q2.Y - q1.Z * q2.Z;
            q.X = q1.W * q2.X + q1.X * q2.W + q1.Y * q2.Z - q1.Z * q2.Y;
            q.Y = q1.W * q2.Y - q1.X * q2.Z + q1.Y * q2.W + q1.Z * q2.X;
            q.Z = q1.W * q2.Z + q1.X * q2.Y - q1.Y * q2.X + q1.Z * q2.W;
            return q;
        }

        /// <summary>
        /// Returns the inverse of a Quaternion
        /// </summary>
        /// <param name="q">Source Quaternion</param>
        /// <returns>The inverse of the Quaternion</returns>
        public static Quaternion Inverse(Quaternion q)
        {
            float invMag = 1f / (q.X * q.X + q.Y * q.Y + q.Z * q.Z + q.W * q.W);
            return new Quaternion(-invMag * q.X, -invMag * q.Y, -invMag * q.Z, invMag * q.W);
        }

        /// <summary>
        /// Rotates a vector by a quaternion
        /// </summary>
        /// <param name="q">Quaternion representing a rotation</param>
        /// <param name="v">Vector to rotate</param>
        /// <returns>q^-1*v*q</returns>
        public static Vector3 Rotate(Quaternion q, Vector3 v)
        {
            Quaternion qv = Quaternion.Multiply(Quaternion.Multiply(q, new Quaternion(v.X, v.Y, v.Z, 0)), Quaternion.Inverse(q));
            return new Vector3(qv.X, qv.Y, qv.Z);
        }

        /// <summary>
        /// Retrieves a string representation of the Quaternion.
        /// </summary>
        /// <returns>String that represents the Quaternion.</returns>
        public override string ToString()
        {
            return string.Format("X:{0} Y:{1} Z:{2} W:{3}", X, Y, Z, W);
        }
    }

    /// <summary>
    /// Pose
    /// </summary>
    [DataContract]
    public struct Pose
    {
        /// <summary>
        /// Position
        /// </summary>
        [DataMember]
        [Description("Identifies the physical position of the device.")]
        public Vector3 Position;

        /// <summary>
        /// Orientation
        /// </summary>
        [DataMember]
        [Description("Identifies the physical orientation of the device.")]
        public Quaternion Orientation;

        /// <summary>
        /// Pose Initialization Constructor
        /// </summary>
        public Pose(Vector3 position)
        {
            Position = position;
            Orientation = new Quaternion(0, 0, 0, 1);
        }

        /// <summary>
        /// Pose Initialization Constructor
        /// </summary>
        public Pose(Vector3 position, Quaternion orientation)
            : this(position)
        {
            Orientation = orientation;
        }

        /// <summary>
        /// Retrieves a string representation of the Pose.
        /// </summary>
        /// <returns>String that represents the Pose.</returns>
        public override string ToString()
        {
            return string.Format("Position=({0}), Orientation=({1})", this.Position, this.Orientation);
        }

        /// <summary>
        /// Checks for equality of two Poses.
        /// </summary>
        /// <param name="obj">Pose to be compared</param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (!(obj is Pose))
            {
                return false;
            }

            Pose poseObj = (Pose)obj;

            return
                this.Position.X == poseObj.Position.X &&
                this.Position.Y == poseObj.Position.Y &&
                this.Position.Z == poseObj.Position.Z &&
                this.Orientation.W == poseObj.Orientation.W &&
                this.Orientation.X == poseObj.Orientation.X &&
                this.Orientation.Y == poseObj.Orientation.Y &&
                this.Orientation.Z == poseObj.Orientation.Z;
        }

        /// <summary>
        /// Returns HashCode of object.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    /// <summary>
    /// Defines a basis for a transformation. It can be converted to a matrix or Quaternion
    /// </summary>
    [DataMemberConstructor]
    [DataContract]
    public struct AxisAngle
    {
        /// <summary>
        /// Vertical (upright) axis
        /// </summary>
        [DataMember]
        [Description("Identifies the vertical (upright) axis.")]
        public Vector3 Axis;

        /// <summary>
        /// Rotation in radians
        /// </summary>
        [DataMember]
        [Description("Identifies the rotation (in radians).")]
        public float Angle;

        /// <summary>
        /// Axis Angle
        /// </summary>
        /// <param name="axis"></param>
        /// <param name="angle"></param>
        public AxisAngle(Vector3 axis, float angle)
        {
            Axis = axis;
            Angle = angle;
        }
    }

    /// <summary>
    /// Defines a 4x4 matrix
    /// </summary>
    [DataContract]
    [DataMemberConstructor]
    public struct Matrix
    {
        #region Matrix Data Members

        /// <summary>
        /// M11
        /// </summary>
        [DataMember]
        [Description("Identifies the M11 data member of the matrix.")]
        public float M11;

        /// <summary>
        /// M12
        /// </summary>
        [DataMember]
        [Description("Identifies the M12 data member of the matrix.")]
        public float M12;

        /// <summary>
        /// M13
        /// </summary>
        [DataMember]
        [Description("Identifies the M13 data member of the matrix.")]
        public float M13;

        /// <summary>
        /// M14
        /// </summary>
        [DataMember]
        [Description("Identifies the M14 data member of the matrix.")]
        public float M14;

        /// <summary>
        /// M21
        /// </summary>
        [DataMember]
        [Description("Identifies the M21 data member of the matrix.")]
        public float M21;

        /// <summary>
        /// M22
        /// </summary>
        [DataMember]
        [Description("Identifies the M22 data member of the matrix.")]
        public float M22;

        /// <summary>
        /// M23
        /// </summary>
        [DataMember]
        [Description("Identifies the M23 data member of the matrix.")]
        public float M23;

        /// <summary>
        /// M24
        /// </summary>
        [DataMember]
        [Description("Identifies the M24 data member of the matrix.")]
        public float M24;

        /// <summary>
        /// M31
        /// </summary>
        [DataMember]
        [Description("Identifies the M31 data member of the matrix.")]
        public float M31;

        /// <summary>
        /// M32
        /// </summary>
        [DataMember]
        [Description("Identifies the M32 data member of the matrix.")]
        public float M32;

        /// <summary>
        /// M33
        /// </summary>
        [DataMember]
        [Description("Identifies the M33 data member of the matrix.")]
        public float M33;

        /// <summary>
        /// M34
        /// </summary>
        [DataMember]
        [Description("Identifies the M34 data member of the matrix.")]
        public float M34;

        /// <summary>
        /// M41
        /// </summary>
        [DataMember]
        [Description("Identifies the M41 data member of the matrix.")]
        public float M41;

        /// <summary>
        /// M42
        /// </summary>
        [DataMember]
        [Description("Identifies the M42 data member of the matrix.")]
        public float M42;

        /// <summary>
        /// M43
        /// </summary>
        [DataMember]
        [Description("Identifies the M43 data member of the matrix.")]
        public float M43;

        /// <summary>
        /// M44
        /// </summary>
        [DataMember]
        [Description("Identifies the M44 data member of the matrix.")]
        public float M44;
        #endregion

        /// <summary>
        /// Builds a matrix that rotates around the x-axis
        /// </summary>
        public static Matrix RotationX(float angle)
        {
            Matrix rotationX = new Matrix();
            rotationX.M11 = 1f;
            rotationX.M22 = (float)Math.Cos(angle);
            rotationX.M23 = (float)Math.Sin(angle);
            rotationX.M32 = (float)-Math.Sin(angle);
            rotationX.M33 = (float)Math.Cos(angle);
            rotationX.M44 = 1f;
            return rotationX;
        }

        /// <summary>
        /// Builds a matrix that rotates around the z-axis
        /// </summary>
        public static Matrix RotationZ(float angle)
        {
            Matrix rotationZ = new Matrix();
            rotationZ.M11 = (float)Math.Cos(angle);
            rotationZ.M12 = (float)Math.Sin(angle);
            rotationZ.M21 = (float)-Math.Sin(angle);
            rotationZ.M22 = (float)Math.Cos(angle);
            rotationZ.M33 = 1f;
            rotationZ.M44 = 1f;
            return rotationZ;
        }

        /// <summary>
        /// Builds a translation matrix using specified offsets
        /// </summary>
        public static Matrix Translation(Vector3 v)
        {
            Matrix translate = new Matrix();
            translate.M11 = 1f;
            translate.M22 = 1f;
            translate.M33 = 1f;
            translate.M44 = 1f;

            translate.M41 = v.X;
            translate.M42 = v.Y;
            translate.M43 = v.Z;
            return translate;
        }

        /// <summary>
        /// Matrix Transpose
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        public static Matrix Transpose(Matrix m)
        {
            Matrix transpose = new Matrix();
            transpose.M11 = m.M11;
            transpose.M12 = m.M21;
            transpose.M13 = m.M31;
            transpose.M14 = m.M41;

            transpose.M21 = m.M12;
            transpose.M22 = m.M22;
            transpose.M23 = m.M32;
            transpose.M24 = m.M42;

            transpose.M31 = m.M13;
            transpose.M32 = m.M23;
            transpose.M33 = m.M33;
            transpose.M34 = m.M43;

            transpose.M41 = m.M14;
            transpose.M42 = m.M24;
            transpose.M43 = m.M34;
            transpose.M44 = m.M44;
            return transpose;
        }

        /// <summary>
        /// Multiply Matrix
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static Matrix Multiply(Matrix left, Matrix right)
        {
            Matrix product = new Matrix();
            product.M11 = left.M11 * right.M11 + left.M12 * right.M21 + left.M13 * right.M31 + left.M14 * right.M41;
            product.M12 = left.M11 * right.M12 + left.M12 * right.M22 + left.M13 * right.M32 + left.M14 * right.M42;
            product.M13 = left.M11 * right.M13 + left.M12 * right.M23 + left.M13 * right.M33 + left.M14 * right.M43;
            product.M14 = left.M11 * right.M14 + left.M12 * right.M24 + left.M13 * right.M34 + left.M14 * right.M44;

            product.M21 = left.M21 * right.M11 + left.M22 * right.M21 + left.M23 * right.M31 + left.M24 * right.M41;
            product.M22 = left.M21 * right.M12 + left.M22 * right.M22 + left.M23 * right.M32 + left.M24 * right.M42;
            product.M23 = left.M21 * right.M13 + left.M22 * right.M23 + left.M23 * right.M33 + left.M24 * right.M43;
            product.M24 = left.M21 * right.M14 + left.M22 * right.M24 + left.M23 * right.M34 + left.M24 * right.M44;

            product.M31 = left.M31 * right.M11 + left.M32 * right.M21 + left.M33 * right.M31 + left.M34 * right.M41;
            product.M32 = left.M31 * right.M12 + left.M32 * right.M22 + left.M33 * right.M32 + left.M34 * right.M42;
            product.M33 = left.M31 * right.M13 + left.M32 * right.M23 + left.M33 * right.M33 + left.M34 * right.M43;
            product.M34 = left.M31 * right.M14 + left.M32 * right.M24 + left.M33 * right.M34 + left.M34 * right.M44;

            product.M41 = left.M41 * right.M11 + left.M42 * right.M21 + left.M43 * right.M31 + left.M44 * right.M41;
            product.M42 = left.M41 * right.M12 + left.M42 * right.M22 + left.M43 * right.M32 + left.M44 * right.M42;
            product.M43 = left.M41 * right.M13 + left.M42 * right.M23 + left.M43 * right.M33 + left.M44 * right.M43;
            product.M44 = left.M41 * right.M14 + left.M42 * right.M24 + left.M43 * right.M34 + left.M44 * right.M44;
            return product;
        }
    }

    #endregion

    #region Materials, Spring


    /// <summary>
    /// Spring coefficients
    /// </summary>
    [DataContract]
    public class SpringProperties
    {
        /// <summary>
        /// Spting stiffness
        /// </summary>
        [DataMember]
        [Description("The spring stiffness setting.")]
        public float SpringCoefficient;
        /// <summary>
        /// Damping coefficient
        /// </summary>
        [DataMember]
        [Description("The spring damping setting.")]
        public float DamperCoefficient;
        /// <summary>
        /// Position, on the vertical axis, of the spring rest point. If the spring, at rest
        /// and there is no deformation, this should be set to zero
        /// </summary>
        [DataMember]
        [Description("Position, on the vertical axis, of the spring rest position.\nIf the spring is at rest and there is no deformation, this should be set to zero.")]
        public float EquilibriumPosition;

        /// <summary>
        /// Default constructor
        /// </summary>
        public SpringProperties() { }

        /// <summary>
        /// Initialization constructor
        /// </summary>
        /// <param name="springCoefficient"></param>
        /// <param name="damperCoefficient"></param>
        /// <param name="equilibriumPosition"></param>
        public SpringProperties(float springCoefficient, float damperCoefficient, float equilibriumPosition)
        {
            SpringCoefficient = springCoefficient;
            DamperCoefficient = damperCoefficient;
            EquilibriumPosition = equilibriumPosition;
        }

        /// <summary>
        /// Returns short name for property editor
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return GetType().Name;
        }
    }

    /// <summary>
    /// Tire Force Function Description
    /// </summary>
    [DataContract]
    public class TireForceFunctionDescription
    {
        /// <summary>
        /// ExtremumSlip
        /// </summary>
        [DataMember]
        [Description("The tire extremum slip setting.")]
        public float ExtremumSlip;

        /// <summary>
        /// ExtremumValue
        /// </summary>
        [DataMember]
        [Description("The tire extremum value setting.")]
        public float ExtremumValue;

        /// <summary>
        /// AsymptoteSlip
        /// </summary>
        [DataMember]
        [Description("The tire asymptote slip setting.")]
        public float AsymptoteSlip;

        /// <summary>
        /// AsymptoteValue
        /// </summary>
        [DataMember]
        [Description("The tire asymptote value setting.")]
        public float AsymptoteValue;

        /// <summary>
        /// StiffnessFactor
        /// </summary>
        [DataMember]
        [Description("The tire stiffness setting.")]
        public float StiffnessFactor;

        /// <summary>
        /// Returns short name for property editor
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return GetType().Name;
        }
    }
    #endregion

    #region Joints

    /// <summary>
    /// Joint instance class
    /// </summary>
    [DataContract]
#if !URT_MINCLR
    [TypeConverter(typeof(JointTypeConverter))]
#endif
    public class Joint
    {
        IntPtr _internalHandle;

        /// <summary>
        /// for internal use only
        /// </summary>
        public IntPtr InternalHandle
        {
            get { return _internalHandle; }
            set { _internalHandle = value; }
        }

        JointProperties _state;

        /// <summary>
        /// Joint state
        /// </summary>
        [DataMember]
        [Description("The joint's current state.")]
        public JointProperties State
        {
            get { return _state; }
            set { _state = value; }
        }

        /// <summary>
        /// Returns short name for property editor
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return GetType().Name;
        }
    }

    /// <summary>
    /// Joint Flags
    /// </summary>
    [Flags]
    [DataContract]
    [Description("The settings for the joint.")]
    public enum JointFlags : int
    {
        /// <summary>
        /// Collision Enabled
        /// </summary>
        CollisionEnabled = 1,
        /// <summary>
        /// Enable Visualization
        /// </summary>
        EnableVisualization = 2
    }

    /// <summary>
    /// Entity Joint Connector
    /// </summary>
    [DataContract]
    public class EntityJointConnector
    {
        /// <summary>
        /// Runtime entity instance
        /// </summary>
        public object ConnectedEntity;

        /// <summary>
        /// Runtime entity instance
        /// </summary>
        public object Entity
        {
            get { return ConnectedEntity; }
            set { ConnectedEntity = value; }
        }

        string _entityName;

        /// <summary>
        /// The name of the entity referenced by this connector.
        /// This field is used only for serialization.
        /// </summary>
        [DataMember]
        [Description("The name of the connected entity.")]
        public String EntityName
        {
            get { return _entityName; }
            set { _entityName = value; }
        }

        /// <summary>
        /// Joint normal in entity (local) coordinate space
        /// </summary>
        [DataMember]
        [Description("The joint normal local coordinates setting.")]
        public Vector3 JointNormal;

        /// <summary>
        /// Joint axis in entity (local) coordinate space
        /// </summary>
        [DataMember]
        [Description("The joint axis local coordinates setting.")]
        public Vector3 JointAxis;

        /// <summary>
        /// Joint connection point in entity (local) coordinate space
        /// </summary>
        [DataMember]
        [Description("The joint connection point local coordinates setting.")]
        public Vector3 JointConnectPoint;

        /// <summary>
        /// Default constructor
        /// </summary>
        public EntityJointConnector() { }

        /// <summary>
        /// Initialization constructor
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="normal"></param>
        /// <param name="axis"></param>
        /// <param name="connectPoint"></param>
        public EntityJointConnector(object entity, Vector3 normal, Vector3 axis, Vector3 connectPoint)
        {
            Entity = entity;
            JointNormal = normal;
            JointAxis = axis;
            JointConnectPoint = connectPoint;
        }

        /// <summary>
        /// Returns short name for property editor
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return GetType().Name;
        }
    }

    /// <summary>
    /// Joint properties
    /// </summary>
    [DataContract]
    public class JointProperties
    {
        /// <summary>
        /// Joint name. Must be unique for all joints between an entity pair
        /// </summary>
        [DataMember]
        [Description("Specifies the descriptive identifier for the joint.")]
        public string Name;

        /// <summary>
        /// Pair of entities joined through this joint
        /// </summary>
        [DataMember]
        [Description("Specifies the pair of entities connected through this joint.")]
        public EntityJointConnector[] Connectors = new EntityJointConnector[2];

        /// <summary>
        /// Maximum force supported by the joint
        /// </summary>
        [DataMember]
        [Description("Specifies the maximum force supported by the joint.")]
        public float MaximumForce;

        /// <summary>
        /// Maximum torque supported by the joint
        /// </summary>
        [DataMember]
        [Description("Specifies the maximum torque supported by the joint.")]
        public float MaximumTorque;

        /// <summary>
        /// Enables collision modelling between entities coupled by the joint
        /// </summary>
        [DataMember]
        [Description("Enables collision between entities couples by the joint.")]
        public bool EnableCollisions;

        /// <summary>
        /// Underlying physics mechanism to compensate joint simulation errors
        /// </summary>
        [DataMember]
        [Description("Identifies the underlying physics mechanism to compensate joint simulation errors.")]
        public JointProjectionProperties Projection;

        /// <summary>
        /// If set, defines a joint with translation/linear position drives
        /// </summary>
        [DataMember]
        [Description("Identifies if the joint supports translation/linear position drives.")]
        public JointLinearProperties Linear;

        /// <summary>
        /// If set, defines a joint with angular drives.
        /// </summary>
        [DataMember]
        [Description("Specifies if the joint supports angular drives.")]
        public JointAngularProperties Angular;

        /// <summary>
        /// Default constructor
        /// </summary>
        public JointProperties() { }

        /// <summary>
        /// Initializes joint in angular drive mode
        /// </summary>
        public JointProperties(JointAngularProperties angular, params EntityJointConnector[] connectors)
        {
            Connectors = connectors;
            Angular = angular;
        }

        /// <summary>
        /// Initializes joint in linear/translation drive mode
        /// </summary>
        public JointProperties(JointLinearProperties linear, params EntityJointConnector[] connectors)
        {
            Connectors = connectors;
            Linear = linear;
        }

        /// <summary>
        /// Returns short name for property editor
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return GetType().Name;
        }
    }

    /// <summary>
    /// Specifies degree of freedom modes
    /// </summary>
    [DataContract]
    [Description("Specifies the degree-of-freedom (DOF) modes for the joint.")]
    public enum JointDOFMode
    {
        /// <summary>
        /// DOF does not allow relative motion
        /// </summary>
        Locked = 0,
        /// <summary>
        /// DOF only allows motion with a limited range
        /// </summary>
        Limited,
        /// <summary>
        /// DOF has full range of motions
        /// </summary>
        Free
    }

    /// <summary>
    /// Joint drive mode
    /// </summary>
    [DataContract]
    [Description("Identifies the joint's drive mode setting.")]
    public enum JointDriveMode
    {
        /// <summary>
        /// Drive uses target position
        /// </summary>
        Position = 0,
        /// <summary>
        /// Drive uses target velocity
        /// </summary>
        Velocity
    }


    /// <summary>
    /// Drive properties for a motor/servo powered joint
    /// </summary>
    [DataContract]
    public class JointDriveProperties
    {
        /// <summary>
        /// Type of drive control the drive understands
        /// </summary>
        [DataMember]
        [Description("Identifies the type of drive control for the joint.")]
        public JointDriveMode Mode;
        /// <summary>
        /// Spring properties
        /// </summary>
        [DataMember]
        [Description("Specifies the spring's properties.")]
        public SpringProperties Spring;
        /// <summary>
        /// Force or torque limit on the drive
        /// </summary>
        [DataMember]
        [Description("Identifies the force or torque limit of the drive.")]
        public float ForceLimit;

        /// <summary>
        /// Default constructor
        /// </summary>
        public JointDriveProperties() { }

        /// <summary>
        /// Initialization constructor
        /// </summary>
        public JointDriveProperties(JointDriveMode mode, SpringProperties spring, float forceLimit)
        {
            Mode = mode;
            Spring = spring;
            ForceLimit = forceLimit;
        }

        /// <summary>
        /// Returns short name for property editor
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return GetType().Name;
        }
    }

    /// <summary>
    /// Joint projection is used by the underlying physics engine to compensate for joint errors
    /// </summary>
    [DataContract]
    [Description("Identifies the joint projection mode setting; used by the physics engine to compensate for joint errors.")]
    public enum JointProjectionMode
    {
        /// <summary>
        /// None
        /// </summary>
        None = 0,
        /// <summary>
        /// Point Minimum Distance
        /// </summary>
        PointMinimumDistance,
        /// <summary>
        /// Linear Minimum Distance
        /// </summary>
        LinearMinimumDistance
    }

    /// <summary>
    /// Describes the joint behavior when it reaches a limit
    /// </summary>
    [DataContract]
    public class JointLimitProperties
    {
        /// <summary>
        /// The position/angle threshold beyond which the limit applies
        /// </summary>
        [DataMember]
        [Description("Identifies joint limit's position/angle threshold.")]
        public float LimitThreshold;

        /// <summary>
        /// Controls amount of bounce when joint hits the limit
        /// </summary>
        [DataMember]
        [Description("Identifies the amount of bounce when the joint hits its limit.")]
        public float Restitution;

        /// <summary>
        /// If set describes the behavior of a spring attached to joint
        /// </summary>
        [DataMember]
        [Description("Identifies the behavior of a spring attached to the joint.")]
        public SpringProperties Spring;

        /// <summary>
        /// Default constructor
        /// </summary>
        public JointLimitProperties() { }

        /// <summary>
        /// Initialization constructor
        /// </summary>
        /// <param name="threshold"></param>
        /// <param name="restitution"></param>
        /// <param name="spring"></param>
        public JointLimitProperties(float threshold, float restitution, SpringProperties spring)
        {
            LimitThreshold = threshold;
            Restitution = restitution;
            Spring = spring;
        }

        /// <summary>
        /// Returns short name for property editor
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return GetType().Name;
        }
    }

    /// <summary>
    /// Joint projection is used to compensate for joint simulation errors
    /// </summary>
    [DataContract]
    public class JointProjectionProperties
    {
        /// <summary>
        /// Projection mode
        /// </summary>
        [DataMember]
        [Description("Identifies joint's projection properties; used to compensate for joint simulation errors.")]
        public JointProjectionMode ProjectionMode;
        /// <summary>
        /// Distance above which to project joint
        /// </summary>
        [DataMember]
        [Description("Identifies the distance which to project the joint.")]
        public float ProjectionDistanceThreshold;
        /// <summary>
        /// Angle above which to project joint
        /// </summary>
        [Description("Identifies the angle which to project the joint.")]
        [DataMember]
        public float ProjectionAngleThreshold;

        /// <summary>
        /// Returns short name for property editor
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return GetType().Name;
        }
    }

    /// <summary>
    /// Translation, linear position joint properties
    /// </summary>
    [DataContract]
    public class JointLinearProperties
    {
        /// <summary>
        /// X axis translation mode
        /// </summary>
        [DataMember]
        [Description("Identifies the X axis translation.")]
        public JointDOFMode XMotionMode;
        /// <summary>
        /// Y axis translation mode
        /// </summary>
        [DataMember]
        [Description("Identifies the Y axis translation.")]
        public JointDOFMode YMotionMode;
        /// <summary>
        /// Z axis translation mode
        /// </summary>
        [DataMember]
        [Description("Identifies the Z axis translation.")]
        public JointDOFMode ZMotionMode;
        /// <summary>
        /// X axis drive properties
        /// </summary>
        [DataMember]
        [Description("Identifies the X axis drive settings.")]
        public JointDriveProperties XDrive;
        /// <summary>
        /// Y axis drive properties
        /// </summary>
        [DataMember]
        [Description("Identifies the Y axis drive settings.")]
        public JointDriveProperties YDrive;
        /// <summary>
        /// Z axis drive properties
        /// </summary>
        [DataMember]
        [Description("Identifies the Z axis drive settings.")]
        public JointDriveProperties ZDrive;
        /// <summary>
        /// Defines drive motion limits,
        /// if one of the drives has limits enabled
        /// </summary>
        [DataMember]
        [Description("Identifies drive motion limit settings.")]
        public JointLimitProperties MotionLimit;
        /// <summary>
        /// Drive target position
        /// </summary>
        [DataMember]
        [Description("Identifies drive target position coordinates.")]
        public Vector3 DriveTargetPosition;
        /// <summary>
        /// Drive target velocity
        /// </summary>
        [DataMember]
        [Description("Identifies drive target velocity.")]
        public Vector3 DriveTargetVelocity;

        /// <summary>
        /// Returns short name for property editor
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return GetType().Name;
        }
    }

    /// <summary>
    /// Joint angular properties
    /// </summary>
    [DataContract]
    public class JointAngularProperties
    {
        /// <summary>
        /// Swing mode for first degree of freedom.
        /// Swing is rotation of x axis in respect to y and z
        /// </summary>
        [DataMember]
        [Description("Identifies the swing mode for the first degree of freedom of the joint.\nSwing is rotation of x axis in respect to y and z.")]
        public JointDOFMode Swing1Mode;
        /// <summary>
        /// Swing model for second degree of freedom
        /// </summary>
        [DataMember]
        [Description("Identifies the swing mode for the second degree of freedom of the joint.")]
        public JointDOFMode Swing2Mode;
        /// <summary>
        /// Twist mode. Twist is rotation about the x axis
        /// </summary>
        [DataMember]
        [Description("Identifies the twist mode for the joint.\nTwist is the rotation around the x-axis.")]
        public JointDOFMode TwistMode;
        /// <summary>
        /// Swing limit
        /// </summary>
        [DataMember]
        [Description("Identifies the swing limit for the first degree of freedom for the joint.")]
        public JointLimitProperties Swing1Limit;
        /// <summary>
        /// Swing limit
        /// </summary>
        [DataMember]
        [Description("Identifies the swing limit for the second degree of freedom for the joint.")]
        public JointLimitProperties Swing2Limit;
        /// <summary>
        /// Upper Twist limit
        /// </summary>
        [DataMember]
        [Description("Identifies the upper twist limit for the joint.\nTwist is the rotation around the x-axis.")]
        public JointLimitProperties UpperTwistLimit;
        /// <summary>
        /// Lower Twist limit
        /// </summary>
        [DataMember]
        [Description("Identifies the lower twist limit for the joint.\nTwist is the rotation around the x-axis.")]
        public JointLimitProperties LowerTwistLimit;
        /// <summary>
        /// Swing drive properties
        /// </summary>
        [DataMember]
        [Description("Identifies the swing drive settings the joint.")]
        public JointDriveProperties SwingDrive;
        /// <summary>
        /// Twist drive properties
        /// </summary>
        [DataMember]
        [Description("Identifies the twist drive settings the joint.")]
        public JointDriveProperties TwistDrive;
        /// <summary>
        /// If set, it drives the joint across the shortest spherical arc
        /// </summary>
        [DataMember]
        [Description("Defines if the joint is driven across the shorteest spherical arc.")]
        public JointDriveProperties SlerpDrive;
        /// <summary>
        /// Target orientation
        /// </summary>
        [DataMember]
        [Description("Identifies the target orientation coordinates.")]
        public Quaternion DriveTargetOrientation;
        /// <summary>
        /// Target velocity
        /// </summary>
        [DataMember]
        [Description("Identifies the target velocity.")]
        public Vector3 DriveTargetVelocity;
        /// <summary>
        /// Ratio which to drive angular velocity of second entity
        /// in relation to first entities angular velocity.
        /// </summary>
        /// <remarks>Set to 0 for disabling geared drive</remarks>
        [DataMember]
        [Description("Identifies the ratio which to drive angular velocity of second entity in relation to first entities angular velocity\nSet to 0 for disabling geared drive.")]
        public float GearRatio;

        /// <summary>
        /// Returns short name for property editor
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return GetType().Name;
        }
    }
    #endregion
#if !URT_MINCLR

    #region TypeConverters
    internal class Vector3Converter : ExpandableObjectConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type t)
        {
            if (t == typeof(string))
            {
                return true;
            }
            return base.CanConvertFrom(context, t);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo info, object value)
        {
            if (value is string)
            {
                string[] components = (value as string).Split(new char[] { ',' });
                Vector3 val = new Vector3();
                val.X = Single.Parse(components[0]);
                val.Y = Single.Parse(components[1]);
                val.Z = Single.Parse(components[2]);
                return val;
            }
            return base.ConvertFrom(context, info, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo info, object value, Type destType)
        {
            if (destType == typeof(string) && (value is Vector3))
            {
                Vector3 val = (Vector3)value;
                return (string.Format("{0}, {1}, {2}", val.X, val.Y, val.Z));
            }
            return base.ConvertTo(context, info, value, destType);
        }

        /// <summary>
        /// Returns the Vector3 properties in the order X, Y, Z
        /// </summary>
        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
        {
            PropertyDescriptorCollection unorderedProps = base.GetProperties(context, value, attributes);
            PropertyDescriptorCollection props = new PropertyDescriptorCollection(null);
            props.Add(unorderedProps["X"]);
            props.Add(unorderedProps["Y"]);
            props.Add(unorderedProps["Z"]);
            return props;
        }
    }

    internal class Vector4Converter : ExpandableObjectConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string); // Can only convert from strings
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            string[] components = (value as string).Split(',');
            Vector4 ret = new Vector4();
            ret.X = float.Parse(components[0]);
            ret.Y = float.Parse(components[1]);
            ret.Z = float.Parse(components[2]);
            ret.W = float.Parse(components[3]);
            return ret;
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string) && value is Vector4)
            {
                Vector4 color = (Vector4)value;
                return string.Format("{0:f2}, {1:f2}, {2:f2}, {3:f2}", color.X, color.Y, color.Z, color.W);
            }
            else
            {
                return base.ConvertTo(context, culture, value, destinationType);
            }
        }

        /// <summary>
        /// Returns the Vector4 properties in the order X, Y, Z, W
        /// </summary>
        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
        {
            PropertyDescriptorCollection unorderedProps = base.GetProperties(context, value, attributes);
            PropertyDescriptorCollection props = new PropertyDescriptorCollection(null);
            props.Add(unorderedProps["X"]);
            props.Add(unorderedProps["Y"]);
            props.Add(unorderedProps["Z"]);
            props.Add(unorderedProps["W"]);
            return props;
        }
    }

    internal class ColorValueConverter : ExpandableObjectConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string); // Can only convert from strings
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            string[] components = (value as string).Split(',');
            ColorValue ret = new ColorValue();
            ret.Red = float.Parse(components[0]);
            ret.Green = float.Parse(components[1]);
            ret.Blue = float.Parse(components[2]);
            ret.Alpha = float.Parse(components[3]);
            return ret;
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string) && value is ColorValue)
            {
                ColorValue color = (ColorValue)value;
                return string.Format("{0:f2}, {1:f2}, {2:f2}, {3:f2}", color.Red, color.Green, color.Blue, color.Alpha);
            }
            else
            {
                return base.ConvertTo(context, culture, value, destinationType);
            }
        }

        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
        {
            PropertyDescriptorCollection oldProps = base.GetProperties(context, value, attributes);

            Dictionary<string, PropertyDescriptor> namePropMap = new Dictionary<string, PropertyDescriptor>();
            foreach (PropertyDescriptor prop in oldProps)
            {
                namePropMap[prop.Name] = prop;
            }

            PropertyDescriptorCollection newProps = new PropertyDescriptorCollection(null);
            newProps.Add(namePropMap["Red"]);
            newProps.Add(namePropMap["Green"]);
            newProps.Add(namePropMap["Blue"]);
            newProps.Add(namePropMap["Alpha"]);
            return newProps;
        }
    }

    #region Joint Editing
    /// <summary>
    /// Uses reflected field information to allow a field to be edited in a property grid.
    /// </summary>
    internal class JointFieldAsPropertyDescriptor : PropertyDescriptor
    {
        private FieldInfo _fieldInfo;
        private int _index;
        private object _component;
        private Joint _joint;

        internal Joint Joint
        {
            get { return _joint; }
        }

        public JointFieldAsPropertyDescriptor(object component, Joint joint, FieldInfo fieldInfo, int index, Attribute[] attrs)
            : base(index >= 0 ? string.Format("{0}[{1}]", fieldInfo.Name, index) : fieldInfo.Name, attrs)
        {
            _fieldInfo = fieldInfo;
            _index = index;
            _component = component;
            _joint = joint;
        }

        public override bool CanResetValue(object component)
        {
            return false;
        }

        public override Type ComponentType
        {
            get { return _component.GetType(); }
        }

        public override object GetValue(object component)
        {
            if (_index >= 0)
                return (_fieldInfo.GetValue(_component) as Array).GetValue(_index);
            else
                return _fieldInfo.GetValue(_component);
        }

        public override bool IsReadOnly
        {
            get { return false; }
        }

        public override Type PropertyType
        {
            get
            {
                if (_index < 0)
                    return _fieldInfo.FieldType;
                else
                    return GetValue(null).GetType();
            }
        }

        public override void ResetValue(object component)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public override void SetValue(object component, object value)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(_joint, new PropertyChangedEventArgs(Name));
            }
            if (_index < 0)
                _fieldInfo.SetValue(_component, value);
        }

        public override bool ShouldSerializeValue(object component)
        {
            return false;
        }

        public override void AddValueChanged(object component, EventHandler handler)
        {
            base.AddValueChanged(component, handler);
            PropertyChanged += new PropertyChangedEventHandler(handler);
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    /// <summary>
    /// Special type converter for joints so that the joint state is represented in the property grid.
    /// </summary>
    public class JointTypeConverter : ExpandableObjectConverter
    {
        /// <summary>
        /// Gets joint fields and returns them as properties.
        /// Returns them in a manner suitable for editing in a property grid.
        /// </summary>
        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
        {
            PropertyDescriptorCollection props = new PropertyDescriptorCollection(null, false);

            object target = (value is Joint) ? (value as Joint).State : value;

            Joint joint = value as Joint ??
                context.Instance as Joint ??
                (context.PropertyDescriptor as JointFieldAsPropertyDescriptor).Joint;

            foreach (FieldInfo curFieldInfo in target.GetType().GetFields())
            {
                if (curFieldInfo.Name == "Connectors")
                {
                    props.Add(new JointFieldAsPropertyDescriptor(target, joint, curFieldInfo, 0, new Attribute[]
                        {
                            new TypeConverterAttribute(this.GetType())
                        }));
                    props.Add(new JointFieldAsPropertyDescriptor(target, joint, curFieldInfo, 1, new Attribute[]
                        {
                            new TypeConverterAttribute(this.GetType())
                        }));

                    continue;
                }

                if (curFieldInfo.Name == "ConnectedEntity")
                {
                    props.Add(new JointFieldAsPropertyDescriptor(target, joint, curFieldInfo, -1,
                        new Attribute[]
                        {
                            new TypeConverterAttribute(this.GetType())
                        }));
                    continue;
                }

                bool convertableFromString = TypeDescriptor.GetConverter(curFieldInfo.FieldType).CanConvertFrom(typeof(string));

                if (curFieldInfo.GetValue(target) == null)
                    props.Add(new JointFieldAsPropertyDescriptor(target, joint, curFieldInfo, -1,
                        new Attribute[]
                        {
                            new EditorAttribute(typeof(PropInitializationEditor), typeof(System.Drawing.Design.UITypeEditor)),
                            new TypeConverterAttribute(this.GetType())
                        }));
                else if (!convertableFromString && !curFieldInfo.FieldType.IsArray)
                    props.Add(new JointFieldAsPropertyDescriptor(target, joint, curFieldInfo, -1,
                        new Attribute[]
                        {
                            new EditorAttribute(typeof(PropInitializationEditor), typeof(System.Drawing.Design.UITypeEditor)),
                            new TypeConverterAttribute(this.GetType())
                        }));
                else
                    props.Add(new JointFieldAsPropertyDescriptor(target, joint, curFieldInfo, -1, null));
            }

            return props;
        }
    }

    /// <summary>
    /// Adds a ... button that either initializes a property with a default constructor, or sets it to null.
    /// </summary>
    class PropInitializationEditor : System.Drawing.Design.UITypeEditor
    {
        public override System.Drawing.Design.UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return System.Drawing.Design.UITypeEditorEditStyle.Modal;
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            if (value == null)
                return TypeDescriptor.CreateInstance(null, context.PropertyDescriptor.PropertyType, null, null);
            else
                return null;
        }
    }
    #endregion

    #endregion
#endif
}