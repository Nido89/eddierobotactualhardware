//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
// 
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: ByteArrayTypes.cs $ $Revision: 7 $
//----------------------------------------------------------------------

using Microsoft.Ccr.Core;
using Microsoft.Dss.Core.Attributes;
using Microsoft.Dss.ServiceModel.Dssp;
using System;
using System.Collections.Generic;
using W3C.Soap;
using bytearray = Microsoft.Robotics.Services.Sample.ByteArray;
using System.ComponentModel;


namespace Microsoft.Robotics.Services.Sample.ByteArray
{

    /// <summary>
    /// ByteArray Contract class
    /// </summary>
    public sealed class Contract
    {
        /// <summary>
        /// The Dss Service contract
        /// </summary>
        [DataMember()]
        public const String Identifier = "http://schemas.microsoft.com/robotics/2007/07/bytearray.user.html";
    }

    /// <summary>
    /// A Byte Array
    /// </summary>
    [DataContract]
    [Description ("The byte array")]
    public class ByteArray
    {
        /// <summary>
        /// The Byte Array Data
        /// </summary>
        [Description("Specifies the set of data in the byte array.")]
        [DataMember, DataMemberConstructor]
        public byte[] Data;

        /// <summary>
        /// Constructor
        /// </summary>
        public ByteArray() { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="data"></param>
        public ByteArray(List<byte> data)
        {
            if (data == null)
                this.Data = null;
            else
                this.Data = data.ToArray();
        }
    }

    /// <summary>
    /// A Byte List
    /// </summary>
    [DataContract]
    [Description("The byte list")]
    public class ByteList
    {
        /// <summary>
        /// The Byte List Data
        /// </summary>
        [Description("Specifies the set of data in the byte list.")]
        [DataMember, DataMemberConstructor]
        public List<byte> Data;

        /// <summary>
        /// A Byte List
        /// </summary>
        public ByteList() { }

        /// <summary>
        /// A Byte List
        /// </summary>
        /// <param name="data"></param>
        public ByteList(byte[] data)
        {
            if (data == null)
                this.Data = new List<byte>();
            else
                this.Data = new List<byte>(data);
        }
    }


    /// <summary>
    /// ByteArray Main Operations PortSet
    /// </summary>
    [ServicePort()]
    public class ByteArrayOperations : PortSet<DsspDefaultLookup, GetArray, GetList, ByteArrayToList, ByteListToArray>
    {
    }

    /// <summary>
    /// Get a Byte Array as a List
    /// </summary>
    [Description("Get a pre-initialized list of bytes.")]
    public class GetList : Get<GetRequestType, PortSet<ByteList, Fault>>
    {
    }


    /// <summary>
    /// Get a Byte Array
    /// </summary>
    [Description("Get a pre-initialized byte array.")]
    public class GetArray : Query<GetRequestType, PortSet<ByteArray, Fault>>
    {
    }

    /// <summary>
    /// Convert a Byte Array to a List of Bytes
    /// </summary>
    [Description("Convert a byte array to a list of bytes.")]
    public class ByteArrayToList : Query<ByteArray, PortSet<ByteList, Fault>>
    {
    }

    /// <summary>
    /// Convert a List of Bytes to a Byte Array
    /// </summary>
    [Description("Convert a list of bytes to a byte array.")]
    public class ByteListToArray : Query<ByteList, PortSet<ByteArray, Fault>>
    {
    }
}
