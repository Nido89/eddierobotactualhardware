//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: iRobotLiteTypes.cs $ $Revision: 20 $
//-----------------------------------------------------------------------

using Microsoft.Ccr.Core;
using Microsoft.Dss.Core.Attributes;
using Microsoft.Dss.Core.DsspHttp;
using Microsoft.Dss.ServiceModel.Dssp;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using W3C.Soap;

using create = Microsoft.Robotics.Services.IRobot.Create;
using irobotlite = Microsoft.Robotics.Services.IRobot.Lite;
using irobot = Microsoft.Robotics.Services.IRobot.Roomba;
using istream = Microsoft.Robotics.Services.IRobot.DssStream;
using sensorupdates = Microsoft.Robotics.Services.IRobot.SensorUpdates;
using stream = Microsoft.Robotics.Services.DssStream.Proxy;
using System.ComponentModel;

namespace Microsoft.Robotics.Services.IRobot.Lite
{

    /// <summary>
    /// IRobotLite Main Operations Port
    /// </summary>
    [ServicePort]
    public class IRobotLiteOperations : PortSet
    {
        /// <summary>
        /// IRobotLite Main Operations Port
        /// </summary>
        public IRobotLiteOperations()
            : base(
                // Standard service operations
                typeof(DsspDefaultLookup),
                typeof(DsspDefaultDrop),
                typeof(irobot.Get),
                typeof(HttpGet),
                typeof(HttpPost),
                typeof(sensorupdates.Subscribe),
                // Available Commands
                typeof(irobot.Configure),
                typeof(irobot.Connect),
                typeof(irobot.RoombaSetMode),
                typeof(irobot.RoombaSetLeds),
                typeof(irobot.RoombaPlaySong),
                typeof(irobot.RoombaGetSensors),
                typeof(irobot.RoombaDrive),
                typeof(create.CreateDriveDirect),
#if DEBUG
                typeof(QueryPerf),
#endif
                // All Create Sensor Updates show up here.
                typeof(sensorupdates.UpdateAll),
                typeof(sensorupdates.UpdateBumpsCliffsAndWalls),
                typeof(sensorupdates.UpdatePose),
                typeof(sensorupdates.UpdatePower),
                typeof(sensorupdates.UpdateMode),
                typeof(sensorupdates.UpdateCliffDetail),
                typeof(sensorupdates.UpdateTelemetry),
                typeof(sensorupdates.UpdateNotifications))
        { }

        #region Implicit Operators

#if DEBUG
        /// <summary>
        /// Implicit Operator for Port of QueryPerf
        /// </summary>
        /// <param name="portSet"></param>
        /// <returns></returns>
        public static implicit operator Port<QueryPerf>(IRobotLiteOperations portSet)
        {
            if (portSet == null) return null;
            return (Port<QueryPerf>)portSet[typeof(QueryPerf)];
        }
#endif

        /// <summary>
        /// Implicit Operator for Port of DsspDefaultLookup
        /// </summary>
        /// <param name="portSet"></param>
        /// <returns></returns>
        public static implicit operator Port<DsspDefaultLookup>(IRobotLiteOperations portSet)
        {
            if (portSet == null) return null;
            return (Port<DsspDefaultLookup>)portSet[typeof(DsspDefaultLookup)];
        }
        /// <summary>
        /// Implicit Operator for Port of DsspDefaultDrop
        /// </summary>
        /// <param name="portSet"></param>
        /// <returns></returns>
        public static implicit operator Port<DsspDefaultDrop>(IRobotLiteOperations portSet)
        {
            if (portSet == null) return null;
            return (Port<DsspDefaultDrop>)portSet[typeof(DsspDefaultDrop)];
        }
        /// <summary>
        /// Implicit Operator for Port of irobot.Get
        /// </summary>
        /// <param name="portSet"></param>
        /// <returns></returns>
        public static implicit operator Port<irobot.Get>(IRobotLiteOperations portSet)
        {
            if (portSet == null) return null;
            return (Port<irobot.Get>)portSet[typeof(irobot.Get)];
        }
        /// <summary>
        /// Implicit Operator for Port of Microsoft.Dss.Core.DsspHttp.HttpGet
        /// </summary>
        /// <param name="portSet"></param>
        /// <returns></returns>
        public static implicit operator Port<Microsoft.Dss.Core.DsspHttp.HttpGet>(IRobotLiteOperations portSet)
        {
            if (portSet == null) return null;
            return (Port<Microsoft.Dss.Core.DsspHttp.HttpGet>)portSet[typeof(Microsoft.Dss.Core.DsspHttp.HttpGet)];
        }
        /// <summary>
        /// Implicit Operator for Port of Microsoft.Dss.Core.DsspHttp.HttpPost
        /// </summary>
        /// <param name="portSet"></param>
        /// <returns></returns>
        public static implicit operator Port<Microsoft.Dss.Core.DsspHttp.HttpPost>(IRobotLiteOperations portSet)
        {
            if (portSet == null) return null;
            return (Port<Microsoft.Dss.Core.DsspHttp.HttpPost>)portSet[typeof(Microsoft.Dss.Core.DsspHttp.HttpPost)];
        }
        /// <summary>
        /// Implicit Operator for Port of sensorupdates.Subscribe
        /// </summary>
        /// <param name="portSet"></param>
        /// <returns></returns>
        public static implicit operator Port<sensorupdates.Subscribe>(IRobotLiteOperations portSet)
        {
            if (portSet == null) return null;
            return (Port<sensorupdates.Subscribe>)portSet[typeof(sensorupdates.Subscribe)];
        }
        /// <summary>
        /// Implicit Operator for Port of irobot.Connect
        /// </summary>
        /// <param name="portSet"></param>
        /// <returns></returns>
        public static implicit operator Port<irobot.Connect>(IRobotLiteOperations portSet)
        {
            if (portSet == null) return null;
            return (Port<irobot.Connect>)portSet[typeof(irobot.Connect)];
        }

        /// <summary>
        /// Implicit Operator for Port of irobot.Configure
        /// </summary>
        /// <param name="portSet"></param>
        /// <returns></returns>
        public static implicit operator Port<irobot.Configure>(IRobotLiteOperations portSet)
        {
            if (portSet == null) return null;
            return (Port<irobot.Configure>)portSet[typeof(irobot.Configure)];
        }

        /// <summary>
        /// Implicit Operator for Port of irobot.RoombaSetMode
        /// </summary>
        /// <param name="portSet"></param>
        /// <returns></returns>
        public static implicit operator Port<irobot.RoombaSetMode>(IRobotLiteOperations portSet)
        {
            if (portSet == null) return null;
            return (Port<irobot.RoombaSetMode>)portSet[typeof(irobot.RoombaSetMode)];
        }
        /// <summary>
        /// Implicit Operator for Port of irobot.RoombaSetLeds
        /// </summary>
        /// <param name="portSet"></param>
        /// <returns></returns>
        public static implicit operator Port<irobot.RoombaSetLeds>(IRobotLiteOperations portSet)
        {
            if (portSet == null) return null;
            return (Port<irobot.RoombaSetLeds>)portSet[typeof(irobot.RoombaSetLeds)];
        }
        /// <summary>
        /// Implicit Operator for Port of irobot.RoombaPlaySong
        /// </summary>
        /// <param name="portSet"></param>
        /// <returns></returns>
        public static implicit operator Port<irobot.RoombaPlaySong>(IRobotLiteOperations portSet)
        {
            if (portSet == null) return null;
            return (Port<irobot.RoombaPlaySong>)portSet[typeof(irobot.RoombaPlaySong)];
        }
        /// <summary>
        /// Implicit Operator for Port of irobot.RoombaGetSensors
        /// </summary>
        /// <param name="portSet"></param>
        /// <returns></returns>
        public static implicit operator Port<irobot.RoombaGetSensors>(IRobotLiteOperations portSet)
        {
            if (portSet == null) return null;
            return (Port<irobot.RoombaGetSensors>)portSet[typeof(irobot.RoombaGetSensors)];
        }

        /// <summary>
        /// Implicit Operator for Port of irobot.RoombaDrive
        /// </summary>
        /// <param name="portSet"></param>
        /// <returns></returns>
        public static implicit operator Port<irobot.RoombaDrive>(IRobotLiteOperations portSet)
        {
            if (portSet == null) return null;
            return (Port<irobot.RoombaDrive>)portSet[typeof(irobot.RoombaDrive)];
        }

        /// <summary>
        /// Implicit Operator for Port of create.CreateDriveDirect
        /// </summary>
        /// <param name="portSet"></param>
        /// <returns></returns>
        public static implicit operator Port<create.CreateDriveDirect>(IRobotLiteOperations portSet)
        {
            if (portSet == null) return null;
            return (Port<create.CreateDriveDirect>)portSet[typeof(create.CreateDriveDirect)];
        }

        /// <summary>
        /// Implicit Operator for Port of sensorupdates.UpdateAll
        /// </summary>
        /// <param name="portSet"></param>
        /// <returns></returns>
        public static implicit operator Port<sensorupdates.UpdateAll>(IRobotLiteOperations portSet)
        {
            if (portSet == null) return null;
            return (Port<sensorupdates.UpdateAll>)portSet[typeof(sensorupdates.UpdateAll)];
        }
        /// <summary>
        /// Implicit Operator for Port of sensorupdates.UpdateBumpsCliffsAndWalls
        /// </summary>
        /// <param name="portSet"></param>
        /// <returns></returns>
        public static implicit operator Port<sensorupdates.UpdateBumpsCliffsAndWalls>(IRobotLiteOperations portSet)
        {
            if (portSet == null) return null;
            return (Port<sensorupdates.UpdateBumpsCliffsAndWalls>)portSet[typeof(sensorupdates.UpdateBumpsCliffsAndWalls)];
        }
        /// <summary>
        /// Implicit Operator for Port of sensorupdates.UpdatePose
        /// </summary>
        /// <param name="portSet"></param>
        /// <returns></returns>
        public static implicit operator Port<sensorupdates.UpdatePose>(IRobotLiteOperations portSet)
        {
            if (portSet == null) return null;
            return (Port<sensorupdates.UpdatePose>)portSet[typeof(sensorupdates.UpdatePose)];
        }
        /// <summary>
        /// Implicit Operator for Port of sensorupdates.UpdatePower
        /// </summary>
        /// <param name="portSet"></param>
        /// <returns></returns>
        public static implicit operator Port<sensorupdates.UpdatePower>(IRobotLiteOperations portSet)
        {
            if (portSet == null) return null;
            return (Port<sensorupdates.UpdatePower>)portSet[typeof(sensorupdates.UpdatePower)];
        }
        /// <summary>
        /// Implicit Operator for Port of sensorupdates.UpdateMode
        /// </summary>
        /// <param name="portSet"></param>
        /// <returns></returns>
        public static implicit operator Port<sensorupdates.UpdateMode>(IRobotLiteOperations portSet)
        {
            if (portSet == null) return null;
            return (Port<sensorupdates.UpdateMode>)portSet[typeof(sensorupdates.UpdateMode)];
        }
        /// <summary>
        /// Implicit Operator for Port of sensorupdates.UpdateCliffDetail
        /// </summary>
        /// <param name="portSet"></param>
        /// <returns></returns>
        public static implicit operator Port<sensorupdates.UpdateCliffDetail>(IRobotLiteOperations portSet)
        {
            if (portSet == null) return null;
            return (Port<sensorupdates.UpdateCliffDetail>)portSet[typeof(sensorupdates.UpdateCliffDetail)];
        }
        /// <summary>
        /// Implicit Operator for Port of sensorupdates.UpdateTelemetry
        /// </summary>
        /// <param name="portSet"></param>
        /// <returns></returns>
        public static implicit operator Port<sensorupdates.UpdateTelemetry>(IRobotLiteOperations portSet)
        {
            if (portSet == null) return null;
            return (Port<sensorupdates.UpdateTelemetry>)portSet[typeof(sensorupdates.UpdateTelemetry)];
        }
        /// <summary>
        /// Implicit Operator for Port of sensorupdates.UpdateNotifications
        /// </summary>
        /// <param name="portSet"></param>
        /// <returns></returns>
        public static implicit operator Port<sensorupdates.UpdateNotifications>(IRobotLiteOperations portSet)
        {
            if (portSet == null) return null;
            return (Port<sensorupdates.UpdateNotifications>)portSet[typeof(sensorupdates.UpdateNotifications)];
        }
        #endregion

        #region Helper Methods

#if DEBUG
        /// <summary>
        /// Post QueryPerf and return Performance Data
        /// </summary>
        /// <returns></returns>
        public virtual PortSet<iRobotPerf, Fault> QueryPerf()
        {
            QueryPerf op = new QueryPerf();
            this.Post(op);
            return op.ResponsePort;
        }
#endif

        /// <summary>
        /// Untyped Post
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public void Post(object item) { base.PostUnknownType(item); }

        /// <summary>
        /// Post DsspDefaultLookup and return the response port.
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        public virtual PortSet<LookupResponse, Fault> DsspDefaultLookup(LookupRequestType body)
        {
            DsspDefaultLookup op = new DsspDefaultLookup();
            op.Body = body ?? new LookupRequestType();
            this.Post(op);
            return op.ResponsePort;

        }
        /// <summary>
        /// Post DsspDefaultLookup and return the response port.
        /// </summary>
        /// <returns></returns>
        public virtual PortSet<LookupResponse, Fault> DsspDefaultLookup()
        {
            DsspDefaultLookup op = new DsspDefaultLookup();
            op.Body = new LookupRequestType();
            this.Post(op);
            return op.ResponsePort;

        }
        /// <summary>
        /// Post DsspDefaultDrop and return the response port.
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        public virtual PortSet<DefaultDropResponseType, Fault> DsspDefaultDrop(DropRequestType body)
        {
            DsspDefaultDrop op = new DsspDefaultDrop();
            op.Body = body ?? new DropRequestType();
            this.Post(op);
            return op.ResponsePort;

        }
        /// <summary>
        /// Post DsspDefaultDrop and return the response port.
        /// </summary>
        /// <returns></returns>
        public virtual PortSet<DefaultDropResponseType, Fault> DsspDefaultDrop()
        {
            DsspDefaultDrop op = new DsspDefaultDrop();
            op.Body = new DropRequestType();
            this.Post(op);
            return op.ResponsePort;

        }
        /// <summary>
        /// Post Get and return the response port.
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        public virtual PortSet<irobot.RoombaState, Fault> Get(GetRequestType body)
        {
            irobot.Get op = new irobot.Get();
            op.Body = body ?? new GetRequestType();
            this.Post(op);
            return op.ResponsePort;

        }
        /// <summary>
        /// Post Get and return the response port.
        /// </summary>
        /// <returns></returns>
        public virtual PortSet<irobot.RoombaState, Fault> Get()
        {
            irobot.Get op = new irobot.Get();
            op.Body = new GetRequestType();
            this.Post(op);
            return op.ResponsePort;

        }
        /// <summary>
        /// Post HttpGet and return the response port.
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        public virtual PortSet<Microsoft.Dss.Core.DsspHttp.HttpResponseType, Fault> HttpGet(Microsoft.Dss.Core.DsspHttp.HttpGetRequestType body)
        {
            Microsoft.Dss.Core.DsspHttp.HttpGet op = new Microsoft.Dss.Core.DsspHttp.HttpGet();
            op.Body = body ?? new Microsoft.Dss.Core.DsspHttp.HttpGetRequestType();
            this.Post(op);
            return op.ResponsePort;

        }
        /// <summary>
        /// Post HttpPost and return the response port.
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        public virtual PortSet<Microsoft.Dss.Core.DsspHttp.HttpResponseType, Fault> HttpPost(Microsoft.Dss.Core.DsspHttp.HttpPostRequestType body)
        {
            Microsoft.Dss.Core.DsspHttp.HttpPost op = new Microsoft.Dss.Core.DsspHttp.HttpPost();
            op.Body = body ?? new Microsoft.Dss.Core.DsspHttp.HttpPostRequestType();
            this.Post(op);
            return op.ResponsePort;

        }
        /// <summary>
        /// Post Subscribe and return the response port.
        /// </summary>
        /// <param name="body"></param>
        /// <param name="notificationPort"></param>
        /// <returns></returns>
        public virtual PortSet<SubscribeResponseType, Fault> Subscribe(SubscribeRequestType body, IPort notificationPort)
        {
            sensorupdates.Subscribe op = new sensorupdates.Subscribe();
            op.Body = body ?? new SubscribeRequestType();
            op.NotificationPort = notificationPort;
            this.Post(op);
            return op.ResponsePort;

        }
        /// <summary>
        /// Post Subscribe and return the response port.
        /// </summary>
        /// <param name="notificationPort"></param>
        /// <returns></returns>
        public virtual PortSet<SubscribeResponseType, Fault> Subscribe(IPort notificationPort)
        {
            sensorupdates.Subscribe op = new sensorupdates.Subscribe();
            op.Body = new SubscribeRequestType();
            op.NotificationPort = notificationPort;
            this.Post(op);
            return op.ResponsePort;

        }

        /// <summary>
        /// Post Connect and return the response port.
        /// </summary>
        public virtual PortSet<Microsoft.Dss.ServiceModel.Dssp.DefaultUpdateResponseType, Fault> Connect()
        {
            irobot.Connect op = new irobot.Connect();
            this.Post(op);
            return op.ResponsePort;
        }

        /// <summary>
        /// Post Connect and return the response port.
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        public virtual PortSet<DefaultUpdateResponseType, Fault> Connect(irobot.IRobotConnect body)
        {
            irobot.Connect op = new irobot.Connect(body);
            if (op.Body == null)
                op.Body = new irobot.IRobotConnect();
            this.Post(op);
            return op.ResponsePort;

        }
        /// <summary>
        /// Post RoombaSetMode and return the response port.
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        public virtual irobot.StandardResponse SetMode(irobot.CmdSetMode body)
        {
            irobot.RoombaSetMode op = new irobot.RoombaSetMode();
            op.Body = body ?? new irobot.CmdSetMode();
            this.Post(op);
            return op.ResponsePort;

        }
        /// <summary>
        /// Post RoombaSetLeds and return the response port.
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        public virtual irobot.StandardResponse SetLeds(irobot.CmdLeds body)
        {
            irobot.RoombaSetLeds op = new irobot.RoombaSetLeds();
            op.Body = body ?? new irobot.CmdLeds();
            this.Post(op);
            return op.ResponsePort;

        }
        /// <summary>
        /// Post RoombaPlaySong and return the response port.
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        public virtual irobot.StandardResponse PlaySong(irobot.CmdPlaySong body)
        {
            irobot.RoombaPlaySong op = new irobot.RoombaPlaySong();
            op.Body = body ?? new irobot.CmdPlaySong();
            this.Post(op);
            return op.ResponsePort;

        }
        /// <summary>
        /// Post RoombaGetSensors and return the response port.
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        public virtual irobot.GetSensorsResponse GetSensors(irobot.CmdSensors body)
        {
            irobot.RoombaGetSensors op = new irobot.RoombaGetSensors();
            op.Body = body ?? new irobot.CmdSensors();
            this.Post(op);
            return op.ResponsePort;

        }
        /// <summary>
        /// Post RoombaDrive and return the response port.
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        public virtual irobot.StandardResponse RoombaDrive(irobot.CmdDrive body)
        {
            irobot.RoombaDrive op = new irobot.RoombaDrive(body ?? new irobot.CmdDrive());
            this.Post(op);
            return op.ResponsePort;
        }

        /// <summary>
        /// Post CreateDriveDirect and return the response port.
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        public virtual irobot.StandardResponse DriveDirect(create.CmdDriveDirect body)
        {
            create.CreateDriveDirect op = new create.CreateDriveDirect();
            op.Body = body ?? new create.CmdDriveDirect();
            this.Post(op);
            return op.ResponsePort;

        }

        /// <summary>
        /// Post CreateDriveDirect and return the response port.
        /// </summary>
        /// <param name="rightVelocity"></param>
        /// <param name="leftVelocity"></param>
        /// <returns></returns>
        public virtual irobot.StandardResponse DriveDirect(int rightVelocity, int leftVelocity)
        {
            create.CreateDriveDirect op = new create.CreateDriveDirect(rightVelocity, leftVelocity);
            this.Post(op);
            return op.ResponsePort;
        }

        #endregion
    }

    /// <summary>
    /// Serial Port Operations Port
    /// </summary>
    class SerialPortOperations : PortSet
    {
        /// <summary>
        /// Stream Service Operations Port
        /// </summary>
        public SerialPortOperations(): base(
            typeof(stream.ReplaceStreamState),
            typeof(stream.WriteData),
            typeof(irobot.ProcessAtomicCommand),
            //typeof(stream.WriteText),
            //typeof(stream.ReadData),
            //typeof(stream.ReadText),
            //typeof(stream.ClearStreamBuffers),
            //typeof(stream.SetStreamProperty),
            //typeof(stream.QueryStreamProperty),
            typeof(DataWaiting))
        {
        }

        #region Implicit Operators

        /// <summary>
        /// Implicit conversion
        /// </summary>
        public static implicit operator Port<stream.ReplaceStreamState>(SerialPortOperations portSet)
        {
            if (portSet == null) return null;
            return (Port<stream.ReplaceStreamState>)portSet[typeof(stream.ReplaceStreamState)];
        }

        /// <summary>
        /// Implicit conversion
        /// </summary>
        public static implicit operator Port<DataWaiting>(SerialPortOperations portSet)
        {
            if (portSet == null) return null;
            return (Port<DataWaiting>)portSet[typeof(DataWaiting)];
        }

        /// <summary>
        /// Implicit conversion
        /// </summary>
        public static implicit operator Port<stream.WriteData>(SerialPortOperations portSet)
        {
            if (portSet == null) return null;
            return (Port<stream.WriteData>)portSet[typeof(stream.WriteData)];
        }

        /// <summary>
        /// Implicit conversion
        /// </summary>
        /// <param name="portSet"></param>
        /// <returns></returns>
        public static implicit operator Port<irobot.ProcessAtomicCommand>(SerialPortOperations portSet)
        {
            if (portSet == null) return null;
            return (Port<irobot.ProcessAtomicCommand>)portSet[typeof(irobot.ProcessAtomicCommand)];
        }

        ///// <summary>
        ///// Implicit conversion
        ///// </summary>
        //public static implicit operator Port<stream.WriteText>(SerialPortOperations portSet)
        //{
        //    if (portSet == null) return null;
        //    return (Port<stream.WriteText>)portSet[typeof(stream.WriteText)];
        //}
        ///// <summary>
        ///// Implicit conversion
        ///// </summary>
        //public static implicit operator Port<stream.ReadData>(SerialPortOperations portSet)
        //{
        //    if (portSet == null) return null;
        //    return (Port<stream.ReadData>)portSet[typeof(stream.ReadData)];
        //}
        ///// <summary>
        ///// Implicit conversion
        ///// </summary>
        //public static implicit operator Port<stream.ReadText>(SerialPortOperations portSet)
        //{
        //    if (portSet == null) return null;
        //    return (Port<stream.ReadText>)portSet[typeof(stream.ReadText)];
        //}
        ///// <summary>
        ///// Implicit conversion
        ///// </summary>
        //public static implicit operator Port<stream.ClearStreamBuffers>(SerialPortOperations portSet)
        //{
        //    if (portSet == null) return null;
        //    return (Port<stream.ClearStreamBuffers>)portSet[typeof(stream.ClearStreamBuffers)];
        //}
        ///// <summary>
        ///// Implicit conversion
        ///// </summary>
        //public static implicit operator Port<stream.SetStreamProperty>(SerialPortOperations portSet)
        //{
        //    if (portSet == null) return null;
        //    return (Port<stream.SetStreamProperty>)portSet[typeof(stream.SetStreamProperty)];
        //}
        ///// <summary>
        ///// Implicit conversion
        ///// </summary>
        //public static implicit operator Port<stream.QueryStreamProperty>(SerialPortOperations portSet)
        //{
        //    if (portSet == null) return null;
        //    return (Port<stream.QueryStreamProperty>)portSet[typeof(stream.QueryStreamProperty)];
        //}



        #endregion

        #region Helper Methods

        /// <summary>
        /// Data Waiting
        /// </summary>
        /// <returns></returns>
        public PortSet<DefaultUpdateResponseType, Fault> DataWaiting()
        {
            DataWaiting dw = new DataWaiting();
            this.PostUnknownType(dw);
            return dw.ResponsePort;
        }


        /// <summary>
        /// Write Data Packet
        /// </summary>
        /// <param name="data"></param>
        /// <param name="timestamp"></param>
        /// <returns></returns>
        public virtual PortSet<DefaultSubmitResponseType, Fault> WriteData(byte[] data, System.DateTime timestamp)
        {
            stream.StreamData body = new stream.StreamData(data, timestamp);
            stream.WriteData op = new stream.WriteData(body);
            this.PostUnknownType(op);
            return op.ResponsePort;
        }

        /// <summary>
        /// Process an Atomic Command
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        public virtual DsspResponsePort<irobot.RoombaReturnPacket> ProcessAtomicCommand(irobot.RoombaCommand cmd)
        {
            irobot.ProcessAtomicCommand op = new irobot.ProcessAtomicCommand(cmd);
            this.PostUnknownType(op);
            return op.ResponsePort;
        }


        /// <summary>
        /// Post Replace Stream State and return the response port.
        /// </summary>
        public virtual PortSet<stream.ReplaceStreamResponse, Fault> ConfigureAndConnect(stream.StreamState body)
        {
            stream.ReplaceStreamState op = new stream.ReplaceStreamState();
            op.Body = body ?? new stream.StreamState();
            this.PostUnknownType(op);
            return op.ResponsePort;
        }

        #endregion
    }

    /// <summary>
    /// Data Waiting Info
    /// </summary>
    public class DataWaitingInfo
    {
    }

    /// <summary>
    /// Write Command Info
    /// </summary>
    public class WriteCommandInfo
    {
        /// <summary>
        /// Atomic Command
        /// </summary>
        public irobot.ProcessAtomicCommand AtomicCommand;

        /// <summary>
        /// Write Command Info
        /// </summary>
        public WriteCommandInfo() { }
        /// <summary>
        /// Write Command Info
        /// </summary>
        /// <param name="atomicCommand"></param>
        public WriteCommandInfo(irobot.ProcessAtomicCommand atomicCommand)
        {
            this.AtomicCommand = atomicCommand;
        }
    }


    /// <summary>
    /// Process waiting data on the serial port
    /// </summary>
    public class DataWaiting: Update<DataWaitingInfo, PortSet<DefaultUpdateResponseType, Fault>>
    {
        /// <summary>
        /// Process waiting data on the serial port
        /// </summary>
        public DataWaiting()
        {
            if (this.Body == null)
                this.Body = new DataWaitingInfo();
        }

        /// <summary>
        /// Process waiting data on the serial port
        /// </summary>
        /// <param name="body"></param>
        public DataWaiting(DataWaitingInfo body)
        {
            this.Body = body;
        }
    }

    /// <summary>
    /// Private State
    /// </summary>
    [DataContract]
    public class PrivateState
    {
        /// <summary>
        /// Maximum buffer size
        /// </summary>
        public static readonly int Maxbuffer = 2048;

        /// <summary>
        /// Private State Initialization
        /// </summary>
        public PrivateState()
        {
            ReadBuffer = new byte[Maxbuffer];
            Start = 0;
            End = 0;
            PendingCommands = new List<irobot.ProcessAtomicCommand>();
        }

        /// <summary>
        /// The inbound data
        /// </summary>
        public byte[] ReadBuffer;

        /// <summary>
        /// Index to the start of the current data
        /// </summary>
        public int Start;

        /// <summary>
        /// Index to the byte following the end of the current data
        /// </summary>
        public int End;

        /// <summary>
        /// Stream State
        /// </summary>
        public stream.StreamState StreamState;

        /// <summary>
        /// Serial Port
        /// </summary>
        public SerialPort SerialPort;

        /// <summary>
        /// iRobot Connection
        /// </summary>
        public istream.iRobotConnection IRobotConnection;

        /// <summary>
        /// Pending Commands
        /// </summary>
        public List<irobot.ProcessAtomicCommand> PendingCommands;

        /// <summary>
        /// iRobot is initialized
        /// </summary>
        public bool Initialized;

        /// <summary>
        /// Have we already opened a browser to configure the service?
        /// </summary>
        public bool BrowsedToService = false;

#if DEBUG
        /// <summary>
        /// iRobot Peformance Statistics
        /// </summary>
        [DataMember]
        public iRobotPerf iRobotPerf = new iRobotPerf();
#endif
    }

#if DEBUG
    /// <summary>
    /// Query Performance Data
    /// </summary>
    [DisplayName("(User) QueryPerformanceData")]
    [Description("Provides performance information associated with communication between the service and the robot.")]
    public class QueryPerf : Query<iRobotPerf, PortSet<iRobotPerf, Fault>>{}


    /// <summary>
    /// IRobot Perf Statistics
    /// </summary>
    [DataContract]
    public class iRobotPerf
    {
        /// <summary>
        /// The number of times we have polled for data on the serial port
        /// </summary>
        [DataMember]
        [Description("Indicates the number of times the serial port has been polled for data.")]
        public int PollingCount = 0;

        /// <summary>
        /// The number of times we have found data waiting on the serial port
        /// </summary>
        [DataMember]
        [Description("Indicates the number of times data was found on the port.")]
        public int DataWaitingCount = 0;


        /// <summary>
        /// PollingCount / DataWaitingCount
        /// </summary>
        [DataMember]
        [Description("Identifies the number of polls per request.\n(PollingCount / DataWatingCount)")]
        public double PollsPerRequest = 0.0;

        /// <summary>
        /// The min response time for a request sent to the iRobot
        /// </summary>
        [DataMember]
        [Description("Identifies the minimum response time for a request sent to the robot (ms).")]
        public double MinResponseTimeMs = 999.0;

        /// <summary>
        /// The max response time for a request sent to the iRobot
        /// </summary>
        [DataMember]
        [Description("Identifies the maximum response time for a request sent to the robot (ms).")]
        public double MaxResponseTimeMs = 0.0;

        /// <summary>
        /// The total time spent waiting for a response
        /// </summary>
        [DataMember]
        [Description("Identifies the total response time waiting for a request sent to the robot (ms).")]
        public double TotalResponseTimeMs = 0.0;

        /// <summary>
        /// The total number of Responses received
        /// </summary>
        [DataMember]
        [Description("Identifies the total number of responses received from the robot.")]
        public double TotalResponses = 0.0;


        /// <summary>
        /// TotalResponseTimeMs / TotalResponses
        /// </summary>
        [DataMember]
        [Description("Identifies the average response time  for a request sent to the robot (ms).")]
        public double AverageResponseMs = 0.0;
    }
#endif

    #region iRobot Lite Contract
    /// <summary>
    /// IRobotLite Contract class
    /// </summary>
    public sealed class Contract
    {
        /// <summary>
        /// The Dss Service contract
        /// </summary>
        public const String Identifier = "http://schemas.microsoft.com/robotics/2007/02/irobotlite.user.html";
    }
    #endregion
}
