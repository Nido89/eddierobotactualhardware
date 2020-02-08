//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: MathTypes.cs $ $Revision: 15 $
//-----------------------------------------------------------------------
using Microsoft.Dss.Core.Attributes;
using Microsoft.Dss.ServiceModel.Dssp;
using Microsoft.Ccr.Core;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using W3C.Soap;

using math = System.Math;

namespace Microsoft.Robotics.Services.Sample.Math
{
    /// <summary>
    /// Math Service contract
    /// </summary>
    public static class Contract
    {
        /// The Unique Contract Identifier for the Timer service
        public const string Identifier = "http://schemas.microsoft.com/robotics/2006/08/math.user.html";
    }

    /// <summary>
    /// Response - Used for all results
    /// </summary>
    [DataContract]
    public class Response
    {
        /// <summary>
        /// Default Constructor
        /// </summary>
        public Response()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="value"></param>
        public Response(double value)
        {
            Result = value;
        }

        /// <summary>
        /// Result - The result of the math operation
        /// </summary>
        [DataMember]
        [Description("Indicates the result of the math operation.")]
        public double Result;
    }

    /// <summary>
    /// MathConstants - Common constants for use in calculations
    /// </summary>
    [DataContract]
    public class MathConstants
    {
        /// <summary>
        /// Value of Pi
        /// </summary>
        [DataMember]
        [Description("Specifies the value of pi.")]
        public double PI = math.PI;
        /// <summary>
        /// Value of e
        /// </summary>
        [DataMember]
        [Description("Specifies the value of E.")]
        public double E = math.E;
    }

    /// <summary>
    /// AcosRequest - Arcosine (inverse cosine)
    /// </summary>
    [DataContract]
    public class AcosRequest
    {
        /// <summary>
        /// D - Value between -1 and +1
        /// </summary>
        [DataMember]
        [Description("Specifies a number representing a cosine, where D is a value between -1 and 1.")]
        public double D;
    }

    /// <summary>
    /// AsinRequest - Arcsine (inverse sine)
    /// </summary>
    [DataContract]
    public class AsinRequest
    {
        /// <summary>
        /// D - Value between -1 and +1
        /// </summary>
        [DataMember]
        [Description("Specifies a number representing a sine, where D is a value between -1 and 1.")]
        public double D;
    }

    /// <summary>
    /// AtanRequest - Arctangent (inverse tangent)
    /// </summary>
    [DataContract]
    public class AtanRequest
    {
        /// <summary>
        /// D - Value to process
        /// </summary>
        [DataMember]
        [Description("Specifies a number representing a tangent.")]
        public double D;
    }

    /// <summary>
    /// Atan2Request - Arctangent with two parameters (for correct quadrant)
    /// </summary>
    [DataContract]
    public class Atan2Request
    {
        /// <summary>
        /// Y - Value in Y direction (vertical)
        /// </summary>
        [DataMember]
        [Description("Specifies the y coordinate of a point.")]
        public double Y;
        /// <summary>
        /// X - Value in X direction (horizontal)
        /// </summary>
        [DataMember]
        [Description("Specifies the x coordinate of a point.")]
        public double X;
    }

    /// <summary>
    /// CosRequest - Cosine of an angle
    /// </summary>
    [DataContract]
    public class CosRequest
    {
        /// <summary>
        /// Angle - Angle in radians
        /// </summary>
        [DataMember]
        [Description("Specifies an angle, measured in radians.")]
        public double Angle;
    }

    /// <summary>
    /// ExpRequest - Exponential (e raised to a power)
    /// </summary>
    [DataContract]
    public class ExpRequest
    {
        /// <summary>
        /// Exponent - Power to raise e to
        /// </summary>
        [DataMember]
        [Description("Specifies a number specifying a power.")]
        public double Exponent;
    }

    /// <summary>
    /// LogRequest - Logarithm of a number with a specified base
    /// </summary>
    [DataContract]
    public class LogRequest
    {
        /// <summary>
        /// A - Value to take the log of (must be positive)
        /// </summary>
        [DataMember]
        [Description("Specifies a number whose logarithm is to be found.")]
        public double A;
        /// <summary>
        /// Base - Logarithm base (usually 2, 10, e)
        /// </summary>
        [DataMember]
        [Description("Specifies the base of the logarithm.")]
        public double Base;
    }

    /// <summary>
    /// PowRequest - Raises a number to a power
    /// </summary>
    [DataContract]
    public class PowRequest
    {
        /// <summary>
        /// A - Value to be raised to power (exponent)
        /// </summary>
        [DataMember]
        [Description("Specifies a number to be raised to a power.")]
        public double A;
        /// <summary>
        /// Exponent - Power to raise A to
        /// </summary>
        [DataMember]
        [Description("Specifies a power.")]
        public double Exponent;
    }

    /// <summary>
    /// RandomRequest - Returns a random number
    /// </summary>
    [DataContract]
    public class RandomRequest
    {
    }

    /// <summary>
    /// RoundRequest - Rounds a number to specified number of places
    /// </summary>
    [DataContract]
    public class RoundRequest
    {
        /// <summary>
        /// Value - Number to be rounded
        /// </summary>
        [DataMember]
        [Description("Specifies a decimal number to be rounded.")]
        public double Value;
        /// <summary>
        /// Digits - Number of significant decimal places
        /// </summary>
        [DataMember]
        [Description("Specifies the number of significant decimal places (precision) in the return value.")]
        public int Digits;
    }

    /// <summary>
    /// SinRequest - Sine of an angle
    /// </summary>
    [DataContract]
    public class SinRequest
    {
        /// <summary>
        /// Angle - Angle in radians
        /// </summary>
        [DataMember]
        [Description("Specifies an angle, measured in radians.")]
        public double Angle;
    }

    /// <summary>
    /// SqrtRequest - Square Root of a number
    /// </summary>
    [DataContract]
    public class SqrtRequest
    {
        /// <summary>
        /// A - Value to find square root of (must be positive)
        /// </summary>
        [DataMember]
        [Description("Specifies a number to calculate its square root.")]
        public double A;
    }

    /// <summary>
    /// TanRequest - Tangent of an angle
    /// </summary>
    [DataContract]
    public class TanRequest
    {
        /// <summary>
        /// Angle - Angle in radians
        /// </summary>
        [DataMember]
        [Description("Specifies an angle, measured in radians.")]
        public double Angle;
    }

    /// <summary>
    /// TruncateRequest - Truncates a number
    /// </summary>
    [DataContract]
    public class TruncateRequest
    {
        /// <summary>
        /// A - Number to be truncated
        /// </summary>
        [DataMember]
        [Description("Specifies a number to truncate.")]
        public double A;
    }

    /// <summary>
    /// ToRadiansRequest - Converts radians to degrees
    /// </summary>
    [DataContract]
    public class ToRadiansRequest
    {
        /// <summary>
        /// Angle - Angle (in degrees) to convert to Radians
        /// </summary>
        [DataMember]
        [Description("Specifies an angle, measured in degrees.")]
        public double Angle;
    }

    /// <summary>
    /// ToDegreesRequest - Converts degrees to radians
    /// </summary>
    [DataContract]
    public class ToDegreesRequest
    {
        /// <summary>
        /// Angle - Angle (in radians) to conver to Degrees
        /// </summary>
        [DataMember]
        [Description("Specifies an angle, measured in radians.")]
        public double Angle;
    }

    /// <summary>
    /// Operations PortSet for Math Service
    /// </summary>
    [ServicePort]
    public class MathOperations : PortSet
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public MathOperations()
            : base(
                typeof(DsspDefaultLookup),
                typeof(Constants),
                typeof(Acos),
                typeof(Asin),
                typeof(Atan),
                typeof(Atan2),
                typeof(Cos),
                typeof(Exp),
                typeof(Log),
                typeof(Pow),
                typeof(Random),
                typeof(Round),
                typeof(Sin),
                typeof(Sqrt),
                typeof(Tan),
                typeof(Truncate),
                typeof(ToRadians),
                typeof(ToDegrees)
            )
        {
        }

    }

    /// <summary>
    /// Constants
    /// </summary>
    [DisplayName("(User) PiandE")]
    [Description("Returns the values of the mathematical constants PI and E.")]
    public class Constants : Get<GetRequestType, PortSet<MathConstants, Fault>>
    {
    }

    /// <summary>
    /// ArcCosine
    /// </summary>
    [DisplayName("(User) ArcCosine")]
    [Description("Returns the angle (in radians) whose cosine is the specified number.")]
    public class Acos : Submit<AcosRequest, PortSet<Response, Fault>>
    {
    }

    /// <summary>
    /// ArcSine
    /// </summary>
    [DisplayName("(User) ArcSine")]
    [Description("Returns the angle (in radians) whose sine is the specified number.")]
    public class Asin : Submit<AsinRequest, PortSet<Response, Fault>>
    {
    }

    /// <summary>
    /// ArcTangent
    /// </summary>
    [DisplayName("(User) ArcTangent")]
    [Description("Returns the angle (in radians) whose tangent is the specified number.")]
    public class Atan : Submit<AtanRequest, PortSet<Response, Fault>>
    {
    }

    /// <summary>
    /// ArcTangent (two parameter version)
    /// </summary>
    [DisplayName("(User) ArcTangent2")]
    [Description("Returns the angle (in radians) whose tangent is the quotient of two specified numbers.")]
    public class Atan2 : Submit<Atan2Request, PortSet<Response, Fault>>
    {
    }

    /// <summary>
    /// Cosine
    /// </summary>
    [DisplayName("(User) Cosine")]
    [Description("Returns the cosine of the specified angle (in radians).")]
    public class Cos : Submit<CosRequest, PortSet<Response, Fault>>
    {
    }

    /// <summary>
    /// Exponential
    /// </summary>
    [DisplayName("(User) Exponent")]
    [Description("Returns e raised to the specified power.")]
    public class Exp : Submit<ExpRequest, PortSet<Response, Fault>>
    {
    }

    /// <summary>
    /// Logarithm
    /// </summary>
    [DisplayName("(User) Logarithm")]
    [Description("Returns the logarithm of a specified number. If base is specified as 0 then a natural logarithm (base e) is calculated.")]
    public class Log : Submit<LogRequest, PortSet<Response, Fault>>
    {
    }

    /// <summary>
    /// Raise to a Power
    /// </summary>
    [DisplayName("(User) Power")]
    [Description("Raises 'A' to the power 'Exponent'.")]
    public class Pow : Submit<PowRequest, PortSet<Response, Fault>>
    {
    }

    /// <summary>
    /// Random number
    /// </summary>
    [DisplayName("(User) Random")]
    [Description("Returns a psuedo-random number between 0.0 and 1.0.")]
    public class Random : Query<RandomRequest, PortSet<Response, Fault>>
    {
    }

    /// <summary>
    /// Round off a number
    /// </summary>
    [DisplayName("(User) Round")]
    [Description("Rounds a double-precision floating-point value to the specified precision.")]
    public class Round : Submit<RoundRequest, PortSet<Response, Fault>>
    {
    }

    /// <summary>
    /// Sine
    /// </summary>
    [DisplayName("(User) Sine")]
    [Description("Returns the sine of the specified angle (in radians).")]
    public class Sin : Submit<SinRequest, PortSet<Response, Fault>>
    {
    }

    /// <summary>
    /// Square Root
    /// </summary>
    [DisplayName("(User) Square Root")]
    [Description("Returns the square root of the specified number.")]
    public class Sqrt : Submit<SqrtRequest, PortSet<Response, Fault>>
    {
    }

    /// <summary>
    /// Tangent
    /// </summary>
    [DisplayName("(User) Tangent")]
    [Description("Returns the tangent of the specified angle (in radians).")]
    public class Tan : Submit<TanRequest, PortSet<Response, Fault>>
    {
    }

    /// <summary>
    /// Truncate a number
    /// </summary>
    [DisplayName("(User) Truncate")]
    [Description("Calculates the integral part of a specified double-precision floating-point number.")]
    public class Truncate : Submit<TruncateRequest, PortSet<Response, Fault>>
    {
    }

    /// <summary>
    /// Convert Degrees to Radians
    /// </summary>
    [DisplayName("(User) ToRadians")]
    [Description("Converts a value in degrees to radians.")]
    public class ToRadians : Submit<ToRadiansRequest, PortSet<Response, Fault>>
    {
    }

    /// <summary>
    /// Convert Radians to Degrees
    /// </summary>
    [DisplayName("(User) ToDegrees")]
    [Description("Converts a value in radians to degrees.")]
    public class ToDegrees : Submit<ToDegreesRequest, PortSet<Response, Fault>>
    {
    }

}
