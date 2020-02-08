//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: RoombaPrivate.cs $ $Revision: 9 $
//-----------------------------------------------------------------------
using Microsoft.Ccr.Core;
using Microsoft.Dss.Core;
using Microsoft.Dss.Core.Attributes;
using Microsoft.Dss.ServiceModel.Dssp;
using Microsoft.Dss.ServiceModel.DsspServiceBase;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Security.Permissions;

using submgr = Microsoft.Dss.Services.SubscriptionManager;
using W3C.Soap;
using Microsoft.Robotics.Services.IRobot.SensorUpdates;
using Microsoft.Dss.Core.DsspHttpUtilities;
using Microsoft.Dss.Core.DsspHttp;
using System.Net;
using System.Collections.Specialized;
using Microsoft.Robotics.Services.IRobot.Create;
using stream = Microsoft.Robotics.Services.DssStream.Proxy;
using istream = Microsoft.Robotics.Services.IRobot.DssStream;

namespace Microsoft.Robotics.Services.IRobot.Roomba
{
    /// <summary>
    /// Internal Messages
    /// </summary>
    class InternalMessages : PortSet
    {
        /// <summary>
        /// Internal Messages
        /// </summary>
        public InternalMessages(): base(
            typeof(stream.ReadText),
            typeof(ATResponse),
            typeof(ATFail),
            typeof(WakeupNotification),
            typeof(Configure),
            typeof(Connect),
            typeof(UpdatePose),
            typeof(UpdatePower),
            typeof(UpdateBumpsCliffsAndWalls),
            typeof(UpdateAll),
            typeof(UpdateCliffDetail),
            typeof(UpdateTelemetry),
            typeof(UpdateMode),
            typeof(UpdateNotifications),
            typeof(ChangeToMode),
            typeof(ProcessAtomicCommand),
            typeof(WakeupRoomba),
            typeof(ExecuteIRobotCommand),
            typeof(DateTime))
        {
        }

        #region Implicit Operators

        /// <summary>
        /// Implicit Operator for Port of stream.ReadText
        /// </summary>
        /// <param name="portSet"></param>
        /// <returns></returns>
        public static implicit operator Port<stream.ReadText>(InternalMessages portSet)
        {
            if (portSet == null) return null;
            return (Port<stream.ReadText>)portSet[typeof(stream.ReadText)];
        }

        /// <summary>
        /// Implicit Operator for Port of ATResponse
        /// </summary>
        /// <param name="portSet"></param>
        /// <returns></returns>
        public static implicit operator Port<ATResponse>(InternalMessages portSet)
        {
            if (portSet == null) return null;
            return (Port<ATResponse>)portSet[typeof(ATResponse)];
        }

        /// <summary>
        /// Implicit Operator for Port of ATFail
        /// </summary>
        /// <param name="portSet"></param>
        /// <returns></returns>
        public static implicit operator Port<ATFail>(InternalMessages portSet)
        {
            if (portSet == null) return null;
            return (Port<ATFail>)portSet[typeof(ATFail)];
        }

        /// <summary>
        /// Implicit Operator for Port of WakeupNotification
        /// </summary>
        /// <param name="portSet"></param>
        /// <returns></returns>
        public static implicit operator Port<WakeupNotification>(InternalMessages portSet)
        {
            if (portSet == null) return null;
            return (Port<WakeupNotification>)portSet[typeof(WakeupNotification)];
        }

        /// <summary>
        /// Implicit Operator for Port of Configure
        /// </summary>
        /// <param name="portSet"></param>
        /// <returns></returns>
        public static implicit operator Port<Configure>(InternalMessages portSet)
        {
            if (portSet == null) return null;
            return (Port<Configure>)portSet[typeof(Configure)];
        }

        /// <summary>
        /// Implicit Operator for Port of Connect
        /// </summary>
        /// <param name="portSet"></param>
        /// <returns></returns>
        public static implicit operator Port<Connect>(InternalMessages portSet)
        {
            if (portSet == null) return null;
            return (Port<Connect>)portSet[typeof(Connect)];
        }

        /// <summary>
        /// Implicit Operator for Port of UpdatePose
        /// </summary>
        /// <param name="portSet"></param>
        /// <returns></returns>
        public static implicit operator Port<UpdatePose>(InternalMessages portSet)
        {
            if (portSet == null) return null;
            return (Port<UpdatePose>)portSet[typeof(UpdatePose)];
        }

        /// <summary>
        /// Implicit Operator for Port of UpdatePower
        /// </summary>
        /// <param name="portSet"></param>
        /// <returns></returns>
        public static implicit operator Port<UpdatePower>(InternalMessages portSet)
        {
            if (portSet == null) return null;
            return (Port<UpdatePower>)portSet[typeof(UpdatePower)];
        }

        /// <summary>
        /// Implicit Operator for Port of UpdateBumpsCliffsAndWalls
        /// </summary>
        /// <param name="portSet"></param>
        /// <returns></returns>
        public static implicit operator Port<UpdateBumpsCliffsAndWalls>(InternalMessages portSet)
        {
            if (portSet == null) return null;
            return (Port<UpdateBumpsCliffsAndWalls>)portSet[typeof(UpdateBumpsCliffsAndWalls)];
        }

        /// <summary>
        /// Implicit Operator for Port of UpdateAll
        /// </summary>
        /// <param name="portSet"></param>
        /// <returns></returns>
        public static implicit operator Port<UpdateAll>(InternalMessages portSet)
        {
            if (portSet == null) return null;
            return (Port<UpdateAll>)portSet[typeof(UpdateAll)];
        }

        /// <summary>
        /// Implicit Operator for Port of UpdateCliffDetail
        /// </summary>
        /// <param name="portSet"></param>
        /// <returns></returns>
        public static implicit operator Port<UpdateCliffDetail>(InternalMessages portSet)
        {
            if (portSet == null) return null;
            return (Port<UpdateCliffDetail>)portSet[typeof(UpdateCliffDetail)];
        }

        /// <summary>
        /// Implicit Operator for Port of UpdateTelemetry
        /// </summary>
        /// <param name="portSet"></param>
        /// <returns></returns>
        public static implicit operator Port<UpdateTelemetry>(InternalMessages portSet)
        {
            if (portSet == null) return null;
            return (Port<UpdateTelemetry>)portSet[typeof(UpdateTelemetry)];
        }

        /// <summary>
        /// Implicit Operator for Port of UpdateMode
        /// </summary>
        /// <param name="portSet"></param>
        /// <returns></returns>
        public static implicit operator Port<UpdateMode>(InternalMessages portSet)
        {
            if (portSet == null) return null;
            return (Port<UpdateMode>)portSet[typeof(UpdateMode)];
        }

        /// <summary>
        /// Implicit Operator for Port of UpdateNotifications
        /// </summary>
        /// <param name="portSet"></param>
        /// <returns></returns>
        public static implicit operator Port<UpdateNotifications>(InternalMessages portSet)
        {
            if (portSet == null) return null;
            return (Port<UpdateNotifications>)portSet[typeof(UpdateNotifications)];
        }

        /// <summary>
        /// Implicit Operator for Port of ChangeToMode
        /// </summary>
        /// <param name="portSet"></param>
        /// <returns></returns>
        public static implicit operator Port<ChangeToMode>(InternalMessages portSet)
        {
            if (portSet == null) return null;
            return (Port<ChangeToMode>)portSet[typeof(ChangeToMode)];
        }

        /// <summary>
        /// Implicit Operator for Port of ProcessAtomicCommand
        /// </summary>
        /// <param name="portSet"></param>
        /// <returns></returns>
        public static implicit operator Port<ProcessAtomicCommand>(InternalMessages portSet)
        {
            if (portSet == null) return null;
            return (Port<ProcessAtomicCommand>)portSet[typeof(ProcessAtomicCommand)];
        }

        /// <summary>
        /// Implicit Operator for Port of WakeupRoomba
        /// </summary>
        /// <param name="portSet"></param>
        /// <returns></returns>
        public static implicit operator Port<WakeupRoomba>(InternalMessages portSet)
        {
            if (portSet == null) return null;
            return (Port<WakeupRoomba>)portSet[typeof(WakeupRoomba)];
        }

        /// <summary>
        /// Implicit Operator for Port of ExecuteIRobotCommand
        /// </summary>
        /// <param name="portSet"></param>
        /// <returns></returns>
        public static implicit operator Port<ExecuteIRobotCommand>(InternalMessages portSet)
        {
            if (portSet == null) return null;
            return (Port<ExecuteIRobotCommand>)portSet[typeof(ExecuteIRobotCommand)];
        }

        /// <summary>
        /// Implicit Operator for Port of DateTime
        /// </summary>
        /// <param name="portSet"></param>
        /// <returns></returns>
        public static implicit operator Port<DateTime>(InternalMessages portSet)
        {
            if (portSet == null) return null;
            return (Port<DateTime>)portSet[typeof(DateTime)];
        }

        #endregion

        /// <summary>
        /// Process an atomic command
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns>The response port</returns>
        public DsspResponsePort<RoombaReturnPacket> ProcessAtomicCommand(RoombaCommand cmd)
        {
            ProcessAtomicCommand processCmd = new ProcessAtomicCommand(cmd);
            this.PostUnknownType(processCmd);
            return processCmd.ResponsePort;
        }

        /// <summary>
        /// Update the current Roomba Mode
        /// </summary>
        /// <param name="mode"></param>
        /// <returns></returns>
        public PortSet<DefaultUpdateResponseType, Fault> UpdateMode(RoombaMode mode)
        {
            UpdateMode update = new UpdateMode(mode);
            this.PostUnknownType(update);
            return update.ResponsePort;
        }

        /// <summary>
        /// Update the current Roomba Mode and Model
        /// </summary>
        /// <param name="model"></param>
        /// <param name="firmwareDate"></param>
        /// <returns></returns>
        public PortSet<DefaultUpdateResponseType, Fault> UpdateMode(IRobotModel model, DateTime firmwareDate)
        {
            UpdateMode update = new UpdateMode(RoombaMode.NotSpecified, model, firmwareDate);
            this.PostUnknownType(update);
            return update.ResponsePort;
        }

        /// <summary>
        /// Update the current Roomba Mode and optionally maintain the mode.
        /// </summary>
        /// <param name="mode"></param>
        /// <param name="maintainMode">The mode to be maintained after this command completes</param>
        /// <returns></returns>
        public PortSet<DefaultUpdateResponseType, Fault> UpdateMode(RoombaMode mode, RoombaMode maintainMode)
        {
            UpdateMode update = new UpdateMode(mode, maintainMode);
            this.PostUnknownType(update);
            return update.ResponsePort;
        }


        /// <summary>
        /// Send a wakeup command to the iRobot
        /// </summary>
        /// <returns></returns>
        public PortSet<DefaultSubmitResponseType, Fault> SendWakeup()
        {
            WakeupRoomba cmd = new WakeupRoomba();
            this.PostUnknownType(cmd);
            return cmd.ResponsePort;
        }

        /// <summary>
        /// Change to the specified mode.
        /// </summary>
        /// <param name="newMode"></param>
        /// <returns></returns>
        public PortSet<DefaultSubmitResponseType, Fault> ChangeToMode(RoombaMode newMode)
        {
            ChangeToMode changeToMode = new ChangeToMode(newMode);
            this.PostUnknownType(changeToMode);
            return changeToMode.ResponsePort;
        }

    }


    /// <summary>
    /// starts "ok" cr lf
    /// starts "bat:   min 0  sec 57  mV 16894  mA 1270  deg-C 11
    /// starts "key-wakeup " ... to cr lf
    /// starts "processor-sleep" ... to cr lf
    /// starts "do-charging" ...
    /// starts "slept for "...
    /// starts "battery-current-quiescent-raw "
    /// starts "2005-07-14-1331-L"
    /// </summary>
    class TextResponse
    {
        public TextResponse(){ }
        public TextResponse(string text)
        {
            this.Text = text;
        }

        public string Text;
    }


    /// <summary>
    /// starts "ok" cr lf
    /// </summary>
    class ATResponse: TextResponse
    {
        public ATResponse(string text): base(text){}
    }

    /// <summary>
    /// Failure on AT Command
    /// </summary>
    class ATFail : TextResponse
    {
        public ATFail(string text) : base(text) { }
    }

    /// <summary>
    /// starts "key-wakeup " ... to cr lf
    /// </summary>
    class WakeupNotification : TextResponse
    {
        public WakeupNotification(string text) : base(text) { }
    }


    /// <summary>
    /// Change to the specified Mode
    /// </summary>
    class ChangeToMode : Submit<ReturnMode, DsspResponsePort<DefaultSubmitResponseType>>
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public ChangeToMode()
        {
            this.Body = new ReturnMode(RoombaMode.NotSpecified);
        }

        /// <summary>
        /// Initialization constructor
        /// </summary>
        /// <param name="roombaMode"></param>
        public ChangeToMode(RoombaMode roombaMode)
        {
            this.Body = new ReturnMode(roombaMode);
        }
    }

    /// <summary>
    /// An internal Submit operation which sends a
    /// command to the iRobotStream, waits for a response,
    /// then updates state appropriately.
    /// </summary>
    public class ProcessAtomicCommand : Submit<RoombaCommand, DsspResponsePort<RoombaReturnPacket>>
    {
        /// <summary>
        /// Default Constructor
        /// </summary>
        public ProcessAtomicCommand() { }

        /// <summary>
        /// Initialization Constructor
        /// </summary>
        public ProcessAtomicCommand(RoombaCommand cmd) { this.Body = cmd; }

        /// <summary>
        /// String representation of the embedded command
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (this.Body == null)
                return "Null";
            return this.Body.ToString();
        }

    }

    #region WakeupRoomba

    /// <summary>
    /// Wake up the iRobot using the specified connection type.
    /// </summary>
    public class WakeupRoomba : Submit<RobotConnectionType, PortSet<DefaultSubmitResponseType, Fault>>
    {
        /// <summary>
        /// Default Constructor
        /// </summary>
        public WakeupRoomba()
        {
        }

        /// <summary>
        /// Wake up the iRobot using the specified connection type.
        /// </summary>
        /// <param name="iRobotConnectionType"></param>
        public WakeupRoomba(iRobotConnectionType iRobotConnectionType)
        {
            this.Body = new RobotConnectionType(iRobotConnectionType);
        }
    }

    /// <summary>
    /// Wake up the iRobot using the specified connection type.
    /// </summary>
    public class RobotConnectionType
    {
        private iRobotConnectionType _iRobotConnectionType = iRobotConnectionType.NotConfigured;

        /// <summary>
        /// Default Constructor
        /// </summary>
        public RobotConnectionType() { }

        /// <summary>
        /// Wake up the iRobot using the specified connection type.
        /// </summary>
        /// <param name="iRobotConnectionType"></param>
        public RobotConnectionType(iRobotConnectionType iRobotConnectionType)
        {
            _iRobotConnectionType = iRobotConnectionType;
        }

        /// <summary>
        /// The iRobot Connection Type
        /// </summary>
        [DataMember, DataMemberConstructor(Order = 1)]
        public iRobotConnectionType ConnectionType
        {
            get { return _iRobotConnectionType; }
            set { _iRobotConnectionType = value; }
        }
    }
    #endregion


    #region ExecuteIRobotCommand

    /// <summary>
    /// Execute an iRobotCommand and wait for the response
    /// </summary>
    public class ExecuteIRobotCommand : Submit<RoombaCommand, PortSet<RoombaReturnPacket, Fault>>
    {
        /// <summary>
        /// Default Constructor
        /// </summary>
        public ExecuteIRobotCommand()
        {
        }

        /// <summary>
        /// Initialization Constructor
        /// </summary>
        /// <param name="cmd"></param>
        public ExecuteIRobotCommand(RoombaCommand cmd)
        {
            this.Body = cmd;
        }
    }


    /// <summary>
    /// A packet of bytes
    /// </summary>
    class BytePacket
    {
        private static int _autoSequence = 1000000;

        /// <summary>
        /// A sequence number
        /// </summary>
        public int Sequence;

        /// <summary>
        /// The data
        /// </summary>
        public byte[] Data;

        /// <summary>
        /// The time of the packet origination
        /// </summary>
        public DateTime PacketTime = DateTime.Now;

        /// <summary>
        /// Default constructor
        /// </summary>
        public BytePacket()
        {
            this.PacketTime = DateTime.Now;
            this.Sequence = _autoSequence++;
        }

        /// <summary>
        /// Initialization constructor
        /// </summary>
        /// <param name="streamData"></param>
        public BytePacket(stream.StreamData streamData)
        {
            this.Data = streamData.Data;
            this.PacketTime = streamData.Timestamp;
            this.Sequence = _autoSequence++;
        }

        /// <summary>
        /// Initialization constructor
        /// </summary>
        /// <param name="data"></param>
        /// <param name="sequence"></param>
        public BytePacket(byte[] data, int sequence)
        {
            this.Data = data;
            this.Sequence = sequence;
            this.PacketTime = DateTime.Now;
        }

        /// <summary>
        /// Full initialization
        /// </summary>
        /// <param name="data"></param>
        /// <param name="sequence"></param>
        /// <param name="packetTime"></param>
        public BytePacket(byte[] data, int sequence, DateTime packetTime)
        {
            this.Data = data;
            this.Sequence = sequence;
            this.PacketTime = packetTime;
        }

        /// <summary>
        /// Create a new data packet with the remaining bytes from this packet.
        /// </summary>
        /// <param name="startIx"></param>
        /// <returns></returns>
        public BytePacket CloneTrailingBytes(int startIx)
        {
            byte[] newData = null;
            if (startIx == 0)
            {
                newData = (byte[])this.Data.Clone();
            }
            else if (startIx > 0 && this.Data != null && startIx < this.Data.Length)
            {
                newData = new byte[this.Data.Length - startIx];
                for (int ix = 0; ix < (this.Data.Length - startIx); ix++)
                {
                    newData[ix] = this.Data[ix + startIx];
                }
            }

            return new BytePacket(newData, this.Sequence, this.PacketTime);
        }

        /// <summary>
        /// Combine two packets into one.
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public static BytePacket Combine(BytePacket first, BytePacket second)
        {

            BytePacket processPacket = new BytePacket(new byte[first.Data.Length + second.Data.Length], second.Sequence, second.PacketTime);

            // Copy the prior packet to the start of our new buffer
            first.Data.CopyTo(processPacket.Data, 0);

            // copy it at the end of the prior packet.
            second.Data.CopyTo(processPacket.Data, first.Data.Length);

            return processPacket;
        }
    }

    #endregion
}
