//------------------------------------------------------------------------------
//  <copyright file="SerialCOMServiceTypes.cs" company="Microsoft Corporation">
//      Copyright (C) Microsoft Corporation.  All rights reserved.
//  </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Robotics.Services.SerialComService
{
    using System;    
    using System.Text;
    
    using Microsoft.Ccr.Core;
    using Microsoft.Dss.Core.Attributes;
    using Microsoft.Dss.ServiceModel.Dssp;
    using W3C.Soap;

    using SerialComPort = System.IO.Ports;

    /// <summary>
    /// Constants for .Net Serial Port communications
    /// Some of these can be specified in SerialCOMService.user.config.xml
    /// </summary>
    public static class SerialCOMConstants
    {
        /// <summary>
        /// Default COM port "COM1"
        /// </summary>
        public static int DefaultPortNumber = 1;

        /// <summary>
        /// Default baud rate
        /// </summary>
        public static int DefaultBaudRate = 115200;

        /// <summary>
        /// Default Parity
        /// </summary>
        public static SerialComPort.Parity DefaultParity = SerialComPort.Parity.None;

        /// <summary>
        /// Default Data Bits
        /// </summary>
        public static int DefaultDataBits = 8;

        /// <summary>
        /// Default Stop Bits
        /// </summary>
        public static SerialComPort.StopBits DefaultStopBits = SerialComPort.StopBits.One;

        /// <summary>
        /// Default timeout (ms) for read operations
        /// </summary>
        public static int DefaultReadTimeout = 200;

        /// <summary>
        /// Default timeout (ms) for write operations
        /// </summary>
        public static int DefaultWriteTimeout = 200;

        /// <summary>
        /// Default Handshake
        /// </summary>
        public static SerialComPort.Handshake DefaultHandshake = SerialComPort.Handshake.None;

        /// <summary>
        /// Default RTS flag
        /// </summary>
        public static bool DefaultDefaultRtsEnable = false;

        /// <summary>
        /// Default DTR flag
        /// </summary>
        public static bool DefaultDefaultDtrEnable = false;

        /// <summary>
        /// Default discarding of trailing nulls
        /// </summary>
        public static bool DefaultDiscardNull = false;

        /// <summary>
        /// Default asynchronous mode flag
        /// </summary>
        public static bool DefaultAsynchronous = false;

        /// <summary>
        /// Default autoconnect flag
        /// </summary>
        public static bool DefaultAutoConnect = true;
    }

    /// <summary>
    /// SerialCOMService contract class
    /// </summary>
    public sealed class Contract
    {
        /// <summary>
        /// DSS contract identifer for SerialCOMService
        /// </summary>
        [DataMember]
        public const string Identifier = "http://www.microsoft.com/2011/07/serialcomservice.user.html";
    }

    /// <summary>
    /// SerialCOMService state
    /// </summary>
    [DataContract]
    public class SerialCOMServiceState
    {
        /// <summary>
        /// Gets or sets COM port number
        /// </summary>
        [DataMember]
        public int PortNumber { get; set; }

        /// <summary>
        /// Gets or sets Baud rate
        /// </summary>
        [DataMember]
        public int BaudRate { get; set; }

        /// <summary>
        /// Gets or sets COM port Parity
        /// </summary>
        [DataMember]
        public SerialComPort.Parity Parity { get; set; }

        /// <summary>
        /// Gets or sets Data Bits
        /// </summary>
        [DataMember]
        public int DataBits { get; set; }

        /// <summary>
        /// Gets or sets Stop Bits
        /// </summary>
        [DataMember]
        public SerialComPort.StopBits StopBits { get; set; }

        /// <summary>
        /// Gets or sets Timeout (ms) for read operations
        /// </summary>
        [DataMember]
        public int ReadTimeout { get; set; }

        /// <summary>
        /// Gets or sets Timeout (ms) for write operations
        /// </summary>
        [DataMember]
        public int WriteTimeout { get; set; }

        /// <summary>
        /// Gets or sets COM port Handshake
        /// </summary>
        [DataMember]
        public SerialComPort.Handshake Handshake { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the RTS is enabled
        /// </summary>
        [DataMember]
        public bool RtsEnable { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether DTR is on
        /// </summary>
        [DataMember]
        public bool DtrEnable { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether Discarding of trailing nulls is on
        /// </summary>
        [DataMember]
        public bool DiscardNull { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether Asynchronous mode is on
        /// </summary>
        [DataMember]
        public bool Asynchronous { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether Autoconnect is on
        /// </summary>
        [DataMember]
        public bool AutoConnect { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether Current connection is on
        /// </summary>
        [DataMember]
        public bool IsConnected { get; set; }

        /// <summary>
        /// Initializes a new instance of the SerialCOMServiceState class
        /// </summary>
        public SerialCOMServiceState()
        {
            this.PortNumber = SerialCOMConstants.DefaultPortNumber;

            this.BaudRate = SerialCOMConstants.DefaultBaudRate;

            this.Parity = SerialCOMConstants.DefaultParity;

            this.DataBits = SerialCOMConstants.DefaultDataBits;

            this.StopBits = SerialCOMConstants.DefaultStopBits;

            this.ReadTimeout = SerialCOMConstants.DefaultReadTimeout;

            this.WriteTimeout = SerialCOMConstants.DefaultWriteTimeout;

            this.Handshake = SerialCOMConstants.DefaultHandshake;

            this.RtsEnable = SerialCOMConstants.DefaultDefaultRtsEnable;

            this.DtrEnable = SerialCOMConstants.DefaultDefaultDtrEnable;

            this.DiscardNull = SerialCOMConstants.DefaultDiscardNull;

            this.Asynchronous = SerialCOMConstants.DefaultAsynchronous;

            this.AutoConnect = SerialCOMConstants.DefaultAutoConnect;

            this.IsConnected = false;
        }
    }

    /// <summary>
    /// Thin wrapper over serial port buffer data
    /// </summary>
    [DataContract]
    [DataMemberConstructor]
    public class Packet
    {
        /// <summary>
        /// Gets or sets the internal byte array
        /// </summary>
        [DataMember]
        public byte[] Message
        {
            get;
            set;
        }

        /// <summary>
        /// Default ctor
        /// </summary>
        public Packet()
        { 
        }

        /// <summary>
        /// Parameterized ctor
        /// </summary>
        /// <param name="message">A byte array containing packet data</param>
        public Packet(byte[] message)
        {
            this.Message = message;
        }
    }

    /// <summary>
    /// SerialCOMService main operations port
    /// </summary>
    [ServicePort]
    public class SerialCOMServiceOperations :
        PortSet<DsspDefaultLookup, DsspDefaultDrop, GetConfiguration, SetConfiguration, OpenPort, 
                ClosePort, ClearBuffer, SendPacket, GetPacket, ReceivedPacket, SendAndGet, Replace, Subscribe>
    {
    }

    /// <summary>
    /// Get service state 
    /// </summary>
    public class GetConfiguration : Get<GetRequestType, DsspResponsePort<SerialCOMServiceState>> 
    { 
    }
    
    /// <summary>
    /// Set service state
    /// </summary>
    public class SetConfiguration : Replace<SerialCOMServiceState, PortSet<DefaultReplaceResponseType, Fault>> 
    { 
    }

    /// <summary>
    /// Open the underlying serial port
    /// </summary>
    public class OpenPort : Update<OpenPortRequestType, PortSet<DefaultUpdateResponseType, Fault>> 
    { 
    }

    /// <summary>
    /// Object type used for OpenPortRequests
    /// </summary>
    [DataContract]
    public class OpenPortRequestType 
    { 
    }

    /// <summary>
    /// Close the underlying serial port
    /// </summary>
    public class ClosePort : Update<ClosePortRequestType, PortSet<DefaultUpdateResponseType, Fault>> 
    { 
    }

    /// <summary>
    /// Object tyle used for ClosePortRequests
    /// </summary>
    [DataContract]
    public class ClosePortRequestType 
    { 
    }

    /// <summary>
    /// Clear the underlying serial port
    /// </summary>
    public class ClearBuffer : Submit<ClearBufferRequest, PortSet<DefaultSubmitResponseType, Fault>>
    {
        /// <summary>
        /// Default ctor
        /// </summary>
        public ClearBuffer()
        { 
        }
        
        /// <summary>
        /// Parameterized ctor
        /// </summary>
        /// <param name="body">Instance of type ClearBufferReuqest</param>
        public ClearBuffer(ClearBufferRequest body) : base(body)
        { 
        }
    }

    /// <summary>
    /// Object type for ClearBuffer operation
    /// </summary>
    [DataContract]
    public class ClearBufferRequest
    {
        /// <summary>
        /// Gets or sets enum specifying which buffer to clear
        /// </summary>
        [DataMember]
        public SerialPortClearOptions BufferToClear
        {
            get;
            set;
        }
    }

    /// <summary>
    /// Clear Options enum - Specifies which buffer(s) to clear
    /// </summary>
    [DataContract]
    public enum SerialPortClearOptions
    {
        /// <summary>
        /// Clear both Input and Output Buffers
        /// </summary>
        Both,
        
        /// <summary>
        /// Clear only Input Buffer
        /// </summary>
        Input,

        /// <summary>
        /// Clear only Output Buffer
        /// </summary>
        Output
    }

    /// <summary>
    /// Send a packet to the underlying COM port
    /// </summary>
    public class SendPacket : Update<SendPacketRequest, PortSet<DefaultUpdateResponseType, Fault>>
    {
        /// <summary>
        /// Default ctor
        /// </summary>
        public SendPacket() 
        {
        }

        /// <summary>
        /// Parameterized ctor
        /// </summary>
        /// <param name="body">Instance of type Packet</param>
        public SendPacket(SendPacketRequest body) : base(body) 
        { 
        }
    }

    /// <summary>
    /// Object type for SendPacket operations
    /// </summary>
    [DataContract]
    [DataMemberConstructor]
    public class SendPacketRequest
    {
        /// <summary>
        /// Gets or sets data to send
        /// </summary>
        [DataMember]
        public Packet Data
        {
            get;
            set;
        }
    }

    /// <summary>
    /// Retrieve a packet from the underlying serial port
    /// </summary>
    public class GetPacket : Update<GetPacketRequest, PortSet<Packet, Fault>>
    {
        /// <summary>
        /// Default ctor
        /// </summary>
        public GetPacket() 
        { 
        }
        
        /// <summary>
        /// Parameterized ctor
        /// </summary>
        /// <param name="body">Instance of type GetPacketRequest</param>
        public GetPacket(GetPacketRequest body) : base(body) 
        { 
        }
    }

    /// <summary>
    /// Object type for GetPacket operations
    /// </summary>
    [DataContract]
    [DataMemberConstructor]
    public class GetPacketRequest
    {
        /// <summary>
        /// Gets or sets string containing optional packet terminator
        /// </summary>
        [DataMember]
        public string Terminator
        {
            get;
            set;
        }
    }

    /// <summary>
    /// Send a request and expect a packet of data returned as a response
    /// </summary>
    public class SendAndGet : Update<SendAndGetRequest, PortSet<Packet, Fault>> 
    { 
    }

    /// <summary>
    /// Object type for SendAndGet operations
    /// </summary>
    [DataContract]
    public class SendAndGetRequest
    {
        /// <summary>
        /// Gets or sets timeout value in milliseconds
        /// </summary>
        [DataMember]
        public int Timeout
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets string containing optional packet terminator
        /// </summary>
        [DataMember]
        public string Terminator
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets data to send
        /// </summary>
        [DataMember]
        public Packet Data
        {
            get;
            set;
        }
    }
    
    /// <summary>
    /// Data for Notification via Subscribe
    /// </summary>
    public class ReceivedPacket : Update<Packet, PortSet<DefaultUpdateResponseType, Fault>>
    {
        /// <summary>
        /// Default ctor
        /// </summary>
        public ReceivedPacket() 
        { 
        }
        
        /// <summary>
        /// Parameterized ctor
        /// </summary>
        /// <param name="body">Instance of type Packet</param>
        public ReceivedPacket(Packet body) : base(body) 
        { 
        }
    }

    /// <summary>
    /// SerialCOMService subscribe operation
    /// </summary>
    public class Subscribe : Subscribe<SubscribeRequestType, PortSet<SubscribeResponseType, Fault>>
    {
        /// <summary>
        /// Default ctor
        /// </summary>
        public Subscribe() 
        { 
        }
        
        /// <summary>
        /// Parameterized ctor
        /// </summary>
        /// <param name="body">Instance of type SubscribeRequestType</param>
        public Subscribe(SubscribeRequestType body) : base(body) 
        {
        }

        /// <summary>
        /// Parameterized ctor
        /// </summary>
        /// <param name="body">Instance of type SubscribeRequestType</param>
        /// <param name="responsePort">Instance of type PortSet</param>
        public Subscribe(SubscribeRequestType body, PortSet<SubscribeResponseType, Fault> responsePort) : base(body, responsePort) 
        {
        }
    }

    /// <summary>
    /// Replace Operation
    /// </summary>
    public class Replace : Replace<SerialCOMServiceState, PortSet<DefaultReplaceResponseType, Fault>>
    {
        /// <summary>
        /// Default no-param ctor
        /// </summary>
        public Replace()
        {
        }

        /// <summary>
        /// Service State-based ctor
        /// </summary>
        /// <param name="state">Service State</param>
        public Replace(SerialCOMServiceState state)
            : base(state)
        {
        }

        /// <summary>
        /// State and Port ctor
        /// </summary>
        /// <param name="state">Service State</param>
        /// <param name="responsePort">Response Port</param>
        public Replace(SerialCOMServiceState state, PortSet<DefaultReplaceResponseType, Fault> responsePort)
            : base(state, responsePort)
        {
        }
    }
}
