//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: Stream.cs $ $Revision: 19 $
//-----------------------------------------------------------------------

using Microsoft.Ccr.Core;
using Microsoft.Dss.Core.Attributes;
using Microsoft.Dss.ServiceModel.Dssp;

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Text;

using W3C.Soap;

namespace Microsoft.Robotics.Services.DssStream
{
    /// <summary>
    /// Stream Data Packet
    /// </summary>
    [DataContract]
    public class StreamData
    {
        private byte[] _data;
        private DateTime _timestamp;

        /// <summary>
        /// Stream Data
        /// </summary>
        [DataMember, DataMemberConstructor(Order = 1)]
        public byte[] Data
        {
            get { return _data; }
            set { _data = value; }
        }

        /// <summary>
        /// The origination time of the data packet
        /// </summary>
        [DataMember(XmlOmitDefaultValue = true)]
        [DataMemberConstructor(Order = 2)]
        public DateTime Timestamp
        {
            get { return _timestamp; }
            set { _timestamp = value; }
        }
    }

    /// <summary>
    /// Stream Text Packet
    /// </summary>
    [DataContract]
    public class StreamText
    {
        private string _text;
        private DateTime _timestamp;

        /// <summary>
        /// Stream Text
        /// </summary>
        [DataMember, DataMemberConstructor(Order = 1)]
        public string Text
        {
            get { return _text; }
            set { _text = value; }
        }

        /// <summary>
        /// The origination time of the text packet
        /// </summary>
        [DataMember(XmlOmitDefaultValue = true)]
        [DataMemberConstructor(Order = 2)]
        public DateTime Timestamp
        {
            get { return _timestamp; }
            set { _timestamp = value; }
        }
    }

    /// <summary>
    /// Stream State
    /// </summary>
    [DataContract]
    public class StreamState
    {
        private bool _initialized;
        private int _identifier;
        private string _description;
        private List<NameValuePair> _configurations;

        /// <summary>
        /// Stream configuration identifier
        /// </summary>
        [DataMember]
        public int Identifier
        {
            get { return this._identifier; }
            set { this._identifier = value; }
        }

        /// <summary>
        /// Is this stream initialized?
        /// </summary>
        [DataMember]
        public bool Initialized
        {
            get { return this._initialized; }
            set { this._initialized = value; }
        }

        /// <summary>
        /// Name or Description of the stream
        /// </summary>
        [DataMember]
        public string Description
        {
            get { return this._description; }
            set { this._description = value; }
        }

        /// <summary>
        /// Name/Value configuration pairs
        /// </summary>
        [DataMember]
        public List<NameValuePair> Configurations
        {
            get { return this._configurations; }
            set { this._configurations = value; }
        }
    }

    /// <summary>
    /// Stream Service Operations Port
    /// </summary>
    [ServicePort]
    public class StreamOperations : PortSet
    {
        /// <summary>
        /// Stream Service Operations Port
        /// </summary>
        public StreamOperations(): base(
            typeof(DsspDefaultLookup),
            typeof(DsspDefaultDrop),
            typeof(ReliableSubscribe),
            typeof(Subscribe),
            typeof(GetStreamState),
            typeof(ReplaceStreamState),
            typeof(WriteData),
            typeof(WriteText),
            typeof(ReadData),
            typeof(ReadText),
            typeof(ClearStreamBuffers),
            typeof(SetStreamProperty),
            typeof(QueryStreamProperty))
        {
        }

        #region Implicit Operators

        /// <summary>
        /// Implicit conversion
        /// </summary>
        public static implicit operator Port<DsspDefaultLookup>(StreamOperations portSet)
        {
            if (portSet == null) return null;
            return (Port<DsspDefaultLookup>)portSet[typeof(DsspDefaultLookup)];
        }
        /// <summary>
        /// Implicit conversion
        /// </summary>
        public static implicit operator Port<DsspDefaultDrop>(StreamOperations portSet)
        {
            if (portSet == null) return null;
            return (Port<DsspDefaultDrop>)portSet[typeof(DsspDefaultDrop)];
        }
        /// <summary>
        /// Implicit conversion
        /// </summary>
        public static implicit operator Port<Subscribe>(StreamOperations portSet)
        {
            if (portSet == null) return null;
            return (Port<Subscribe>)portSet[typeof(Subscribe)];
        }
        /// <summary>
        /// Implicit conversion
        /// </summary>
        public static implicit operator Port<GetStreamState>(StreamOperations portSet)
        {
            if (portSet == null) return null;
            return (Port<GetStreamState>)portSet[typeof(GetStreamState)];
        }
        /// <summary>
        /// Implicit conversion
        /// </summary>
        public static implicit operator Port<ReplaceStreamState>(StreamOperations portSet)
        {
            if (portSet == null) return null;
            return (Port<ReplaceStreamState>)portSet[typeof(ReplaceStreamState)];
        }
        /// <summary>
        /// Implicit conversion
        /// </summary>
        public static implicit operator Port<WriteData>(StreamOperations portSet)
        {
            if (portSet == null) return null;
            return (Port<WriteData>)portSet[typeof(WriteData)];
        }
        /// <summary>
        /// Implicit conversion
        /// </summary>
        public static implicit operator Port<WriteText>(StreamOperations portSet)
        {
            if (portSet == null) return null;
            return (Port<WriteText>)portSet[typeof(WriteText)];
        }
        /// <summary>
        /// Implicit conversion
        /// </summary>
        public static implicit operator Port<ReadData>(StreamOperations portSet)
        {
            if (portSet == null) return null;
            return (Port<ReadData>)portSet[typeof(ReadData)];
        }
        /// <summary>
        /// Implicit conversion
        /// </summary>
        public static implicit operator Port<ReadText>(StreamOperations portSet)
        {
            if (portSet == null) return null;
            return (Port<ReadText>)portSet[typeof(ReadText)];
        }
        /// <summary>
        /// Implicit conversion
        /// </summary>
        public static implicit operator Port<ClearStreamBuffers>(StreamOperations portSet)
        {
            if (portSet == null) return null;
            return (Port<ClearStreamBuffers>)portSet[typeof(ClearStreamBuffers)];
        }
        /// <summary>
        /// Implicit conversion
        /// </summary>
        public static implicit operator Port<ReliableSubscribe>(StreamOperations portSet)
        {
            if (portSet == null) return null;
            return (Port<ReliableSubscribe>)portSet[typeof(ReliableSubscribe)];
        }

        /// <summary>
        /// Implicit conversion
        /// </summary>
        public static implicit operator Port<SetStreamProperty>(StreamOperations portSet)
        {
            if (portSet == null) return null;
            return (Port<SetStreamProperty>)portSet[typeof(SetStreamProperty)];
        }
        /// <summary>
        /// Implicit conversion
        /// </summary>
        public static implicit operator Port<QueryStreamProperty>(StreamOperations portSet)
        {
            if (portSet == null) return null;
            return (Port<QueryStreamProperty>)portSet[typeof(QueryStreamProperty)];
        }

        #endregion

    }

    /// <summary>
    /// A Name and Value pair
    /// </summary>
    [DataContract]
    public class NameValuePair
    {
        private string _name;
        private string _value;

        /// <summary>
        /// Default Constructor
        /// </summary>
        public NameValuePair() { }

        /// <summary>
        /// Initialization Constructor
        /// </summary>
        public NameValuePair(string name, string value)
        {
            this._name = name;
            this._value = value;
        }

        /// <summary>
        /// The Name
        /// </summary>
        [DataMember, DataMemberConstructor(Order=1)]
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        /// <summary>
        /// The Value
        /// </summary>
        [DataMember, DataMemberConstructor(Order = 2)]
        public string Value
        {
            get { return _value; }
            set { _value = value; }
        }
    }


    /// <summary>
    /// Stream Service Get Operation
    /// </summary>
    [Description("Gets the stream's current state.")]
    public class GetStreamState : Get<GetRequestType, PortSet<StreamState, Fault>> { }

    /// <summary>
    /// Send a series of bytes to the Stream
    /// </summary>
    [Description("Sends a series of bytes to the stream.")]
    public class WriteData : Submit<StreamData, PortSet<DefaultSubmitResponseType, Fault>> { }

    /// <summary>
    /// Send a text string to the Stream
    /// </summary>
    [Description("Sends a text string to the stream.")]
    public class WriteText : Submit<StreamText, PortSet<DefaultSubmitResponseType, Fault>> { }


    /// <summary>
    /// Receive Data Bytes from the Stream
    /// </summary>
    [Description("Receives a series of bytes from the stream.")]
    public class ReadData : Insert<StreamData, PortSet<DefaultInsertResponseType, Fault>> { }

    /// <summary>
    /// Send/Receive a string to the Stream
    /// </summary>
    [Description("Receives a text string from the stream.")]
    public class ReadText : Insert<StreamText, PortSet<DefaultInsertResponseType, Fault>> { }

    /// <summary>
    /// Query a stream property
    /// </summary>
    [Description("Queries an implementation specific property of the stream.")]
    public class QueryStreamProperty : Query<NameValuePair, PortSet<NameValuePair, Fault>> { }

    /// <summary>
    /// Set a stream property
    /// </summary>
    [Description("Sets (or updates) an implementation specific property of the stream.")]
    public class SetStreamProperty : Update<NameValuePair, PortSet<DefaultUpdateResponseType, Fault>> { }

    /// <summary>
    /// Stream Service Replace Operation
    /// </summary>
    [Description("Configures (or indicates a change to) the stream's entire state.")]
    public class ReplaceStreamState : Replace<StreamState, PortSet<ReplaceStreamResponse, Fault>> { }

    /// <summary>
    /// Custom Stream Replace Response
    /// </summary>
    [DataContract]
    public class ReplaceStreamResponse
    {
        /// <summary>
        /// Identifies whether or not the stream is connected after a configuration.
        /// </summary>
        [DataMember, DataMemberConstructor]
        public bool Connected;
    }

    /// <summary>
    /// Clear Stream Buffers
    /// </summary>
    [DataContract]
    public class ClearBuffers
    {
        /// <summary>
        /// Default Constructor
        /// </summary>
        public ClearBuffers() { }
        /// <summary>
        /// Initialization Constructor
        /// </summary>
        /// <param name="clearReadBuffer"></param>
        /// <param name="clearWriteBuffer"></param>
        public ClearBuffers(bool clearReadBuffer, bool clearWriteBuffer)
        {
            this.ClearReadBuffer = clearReadBuffer;
            this.ClearWriteBuffer = clearWriteBuffer;
        }

        /// <summary>
        /// Clear the pending read buffer
        /// </summary>
        [DataMember, DataMemberConstructor(Order=1)]
        public bool ClearReadBuffer;

        /// <summary>
        /// Clear the pending write buffer
        /// </summary>
        [DataMember, DataMemberConstructor(Order=2)]
        public bool ClearWriteBuffer;
    }

    /// <summary>
    /// Stream Service Subscribe Operation
    /// </summary>
    public class Subscribe : Subscribe<SubscribeRequestType, PortSet<SubscribeResponseType, Fault>> { }

    /// <summary>
    /// Stream Service ReliableSubscribe Operation
    /// </summary>
    public class ReliableSubscribe : Subscribe<ReliableSubscribeRequestType, DsspResponsePort<SubscribeResponseType>, StreamOperations> { }

    /// <summary>
    /// Clear Stream Buffers
    /// </summary>
    public class ClearStreamBuffers : Submit<ClearBuffers, PortSet<DefaultSubmitResponseType, Fault>>
    {
        /// <summary>
        /// Default Constructor
        /// </summary>
        public ClearStreamBuffers() { this.Body = new ClearBuffers(); }
        /// <summary>
        /// Initialization Constructor
        /// </summary>
        /// <param name="clearReadBuffer"></param>
        /// <param name="clearWriteBuffer"></param>
        public ClearStreamBuffers(bool clearReadBuffer, bool clearWriteBuffer)
        {
            this.Body = new ClearBuffers(clearReadBuffer, clearWriteBuffer);
        }
    }

    #region DssStream Contract
    /// <summary>
    /// A Generic Contract which allows a text or binary stream
    /// to be communicated between services.
    /// <remarks>The underlying stream is converted into a series of
    /// text or binary packets.</remarks>
    /// </summary>
    [DisplayName("(User) Generic Stream")]
    [Description("Provides bi-directional packet-based stream access.")]
    [DssServiceDescription("http://msdn.microsoft.com/library/dd146311.aspx")]
    public static class Contract
    {
        /// The Unique Contract Identifier for the Stream service
        public const string Identifier = "http://schemas.microsoft.com/robotics/generic/2006/12/dssstream.html";
    }
    #endregion
}
