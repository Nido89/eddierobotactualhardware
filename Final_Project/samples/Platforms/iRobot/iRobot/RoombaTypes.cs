//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: RoombaTypes.cs $ $Revision: 28 $
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel;
using W3C.Soap;

using Microsoft.Ccr.Core;
using Microsoft.Dss.Core.Attributes;
using Microsoft.Dss.Core.DsspHttp;
using Microsoft.Dss.ServiceModel.Dssp;
using Microsoft.Robotics.Services.IRobot.Roomba;
using Microsoft.Robotics.Services.IRobot.Create;
using notifications = Microsoft.Robotics.Services.IRobot.SensorUpdates;
using roomba = Microsoft.Robotics.Services.IRobot.Roomba;

namespace Microsoft.Robotics.Services.IRobot.SensorUpdates
{

    /// <summary>
    /// iRobot Sensor Updates Response Port
    /// </summary>
    [ServicePort]
    public class IRobotSensorUpdatesPort : PortSet
    {
        /// <summary>
        /// iRobot Sensor Updates Response Port
        /// </summary>
        public IRobotSensorUpdatesPort()
            : base(
                typeof(DsspDefaultLookup),
                typeof(Subscribe),
                // All Create/Roomba Sensor Updates show up here.
                typeof(UpdateAll),
                typeof(UpdateBumpsCliffsAndWalls),
                typeof(UpdatePose),
                typeof(UpdatePower),
                typeof(UpdateMode),
                typeof(UpdateCliffDetail),
                typeof(UpdateTelemetry),
                typeof(UpdateNotifications))
        { }

        #region Implicit Operators


        /// <summary>
        /// Implicit Operator for Port of DsspDefaultLookup
        /// </summary>
        /// <param name="portSet"></param>
        /// <returns></returns>
        public static implicit operator Port<DsspDefaultLookup>(IRobotSensorUpdatesPort portSet)
        {
            if (portSet == null) return null;
            return (Port<DsspDefaultLookup>)portSet[typeof(DsspDefaultLookup)];
        }
        /// <summary>
        /// Implicit Operator for Port of roomba.Subscribe
        /// </summary>
        /// <param name="portSet"></param>
        /// <returns></returns>
        public static implicit operator Port<Subscribe>(IRobotSensorUpdatesPort portSet)
        {
            if (portSet == null) return null;
            return (Port<Subscribe>)portSet[typeof(Subscribe)];
        }
        /// <summary>
        /// Implicit Operator for Port of UpdateAll
        /// </summary>
        /// <param name="portSet"></param>
        /// <returns></returns>
        public static implicit operator Port<UpdateAll>(IRobotSensorUpdatesPort portSet)
        {
            if (portSet == null) return null;
            return (Port<UpdateAll>)portSet[typeof(UpdateAll)];
        }
        /// <summary>
        /// Implicit Operator for Port of UpdateBumpsCliffsAndWalls
        /// </summary>
        /// <param name="portSet"></param>
        /// <returns></returns>
        public static implicit operator Port<UpdateBumpsCliffsAndWalls>(IRobotSensorUpdatesPort portSet)
        {
            if (portSet == null) return null;
            return (Port<UpdateBumpsCliffsAndWalls>)portSet[typeof(UpdateBumpsCliffsAndWalls)];
        }
        /// <summary>
        /// Implicit Operator for Port of UpdatePose
        /// </summary>
        /// <param name="portSet"></param>
        /// <returns></returns>
        public static implicit operator Port<UpdatePose>(IRobotSensorUpdatesPort portSet)
        {
            if (portSet == null) return null;
            return (Port<UpdatePose>)portSet[typeof(UpdatePose)];
        }
        /// <summary>
        /// Implicit Operator for Port of UpdatePower
        /// </summary>
        /// <param name="portSet"></param>
        /// <returns></returns>
        public static implicit operator Port<UpdatePower>(IRobotSensorUpdatesPort portSet)
        {
            if (portSet == null) return null;
            return (Port<UpdatePower>)portSet[typeof(UpdatePower)];
        }
        /// <summary>
        /// Implicit Operator for Port of UpdateMode
        /// </summary>
        /// <param name="portSet"></param>
        /// <returns></returns>
        public static implicit operator Port<UpdateMode>(IRobotSensorUpdatesPort portSet)
        {
            if (portSet == null) return null;
            return (Port<UpdateMode>)portSet[typeof(UpdateMode)];
        }
        /// <summary>
        /// Implicit Operator for Port of UpdateCliffDetail
        /// </summary>
        /// <param name="portSet"></param>
        /// <returns></returns>
        public static implicit operator Port<UpdateCliffDetail>(IRobotSensorUpdatesPort portSet)
        {
            if (portSet == null) return null;
            return (Port<UpdateCliffDetail>)portSet[typeof(UpdateCliffDetail)];
        }
        /// <summary>
        /// Implicit Operator for Port of UpdateTelemetry
        /// </summary>
        /// <param name="portSet"></param>
        /// <returns></returns>
        public static implicit operator Port<UpdateTelemetry>(IRobotSensorUpdatesPort portSet)
        {
            if (portSet == null) return null;
            return (Port<UpdateTelemetry>)portSet[typeof(UpdateTelemetry)];
        }
        /// <summary>
        /// Implicit Operator for Port of UpdateNotifications
        /// </summary>
        /// <param name="portSet"></param>
        /// <returns></returns>
        public static implicit operator Port<UpdateNotifications>(IRobotSensorUpdatesPort portSet)
        {
            if (portSet == null) return null;
            return (Port<UpdateNotifications>)portSet[typeof(UpdateNotifications)];
        }

        #endregion

        /// <summary>
        /// Post Subscribe and return the response port.
        /// </summary>
        /// <param name="body"></param>
        /// <param name="notificationPort"></param>
        /// <returns></returns>
        public virtual PortSet<SubscribeResponseType, Fault> Subscribe(SubscribeRequestType body, IPort notificationPort)
        {
            Subscribe op = new Subscribe();
            op.Body = body ?? new SubscribeRequestType();
            op.NotificationPort = notificationPort;
            this.PostUnknownType(op);
            return op.ResponsePort;

        }
        /// <summary>
        /// Post Subscribe and return the response port.
        /// </summary>
        /// <param name="notificationPort"></param>
        /// <returns></returns>
        public virtual PortSet<SubscribeResponseType, Fault> Subscribe(IPort notificationPort)
        {
            Subscribe op = new Subscribe();
            op.Body = new SubscribeRequestType();
            op.NotificationPort = notificationPort;
            this.PostUnknownType(op);
            return op.ResponsePort;

        }
    }

    #region Operations


    /// <summary>
    /// Subscribe Operation
    /// </summary>
    [DisplayName("(User) Subscribe")]
    [Description("Subscribes to iRobot service notifications.")]
    public class Subscribe : Subscribe<SubscribeRequestType, PortSet<SubscribeResponseType, Fault>, notifications.IRobotSensorUpdatesPort>
    {
        /// <summary>
        /// Subscribe to Create/Roomba Sensors
        /// </summary>
        public Subscribe() { this.Body = new SubscribeRequestType(); }

    }


    /// <summary>
    /// iRobot Create Notification Results
    /// </summary>
    [Description("Indicates an update from iRobot Create stream notifications.")]
    public class UpdateNotifications : Update<ReturnStream, DsspResponsePort<DefaultUpdateResponseType>>
    {
        /// <summary>
        /// Update Create Notification Detail
        /// </summary>
        /// <param name="data"></param>
        public UpdateNotifications(ReturnStream data) { this.Body = data; }

        /// <summary>
        /// Update Create Notification Detail
        /// </summary>
        public UpdateNotifications() { }
    }

    /// <summary>
    /// CliffDetail Results
    /// </summary>
    [DisplayName("(User) UpdateCliffDetail")]
    [Description("Indicates an update to the iRobot Create's cliff sensors.")]
    public class UpdateCliffDetail : Update<ReturnCliffDetail, DsspResponsePort<DefaultUpdateResponseType>>
    {
        /// <summary>
        /// Update Cliff Detail
        /// </summary>
        /// <param name="data"></param>
        public UpdateCliffDetail(ReturnCliffDetail data) { this.Body = data; }

        /// <summary>
        /// Update Cliff Detail
        /// </summary>
        public UpdateCliffDetail() { }
    }


    /// <summary>
    /// Telemetry Results
    /// </summary>
    [Description("Indicates an update to the iRobot Create's telemetry.")]
    public class UpdateTelemetry : Update<ReturnTelemetry, DsspResponsePort<DefaultUpdateResponseType>>
    {
        /// <summary>
        /// Update Cliff Detail
        /// </summary>
        /// <param name="data"></param>
        public UpdateTelemetry(ReturnTelemetry data) { this.Body = data; }

        /// <summary>
        /// Update Cliff Detail
        /// </summary>
        public UpdateTelemetry() { }
    }
    /// <summary>
    /// ReturnAll Notification
    /// </summary>
    [DisplayName("(User) UpdateAll")]
    [Description("Indicates an update to all sensors.")]
    public class UpdateAll : Update<ReturnAll, DsspResponsePort<DefaultUpdateResponseType>>
    {
        /// <summary>
        /// Default Constructor
        /// </summary>
        /// <param name="data"></param>
        public UpdateAll(ReturnAll data) { this.Body = data; }

        /// <summary>
        /// Default Constructor
        /// </summary>
        public UpdateAll() { }
    }

    /// <summary>
    /// ReturnSensors Notification
    /// </summary>
    [DisplayName("(User) UpdateBumpsCliffsAndWalls")]
    [Description("Indicates an update to bumper, cliff, or wall sensors.")]
    public class UpdateBumpsCliffsAndWalls : Update<ReturnSensors, DsspResponsePort<DefaultUpdateResponseType>>
    {
        /// <summary>
        /// Default Constructor
        /// </summary>
        /// <param name="data"></param>
        public UpdateBumpsCliffsAndWalls(ReturnSensors data) { this.Body = data; }

        /// <summary>
        /// Default Constructor
        /// </summary>
        public UpdateBumpsCliffsAndWalls() { }

    }

    /// <summary>
    /// ReturnPose Notification
    /// </summary>
    [DisplayName("(User) UpdatePose")]
    [Description("Indicates an update to the robot's pose.")]
    public class UpdatePose : Update<ReturnPose, DsspResponsePort<DefaultUpdateResponseType>>
    {
        /// <summary>
        /// Default Constructor
        /// </summary>
        /// <param name="data"></param>
        public UpdatePose(ReturnPose data) { this.Body = data; }

        /// <summary>
        /// Default Constructor
        /// </summary>
        public UpdatePose() { }
    }


    /// <summary>
    /// ReturnPower Notification
    /// </summary>
    [DisplayName("(User) UpdatePower")]
    [Description("Indicates an update to the power setting.")]
    public class UpdatePower : Update<ReturnPower, DsspResponsePort<DefaultUpdateResponseType>>
    {
        /// <summary>
        /// Default Constructor
        /// </summary>
        /// <param name="data"></param>
        public UpdatePower(ReturnPower data) { this.Body = data; }

        /// <summary>
        /// Default Constructor
        /// </summary>
        public UpdatePower() { }
    }

    /// <summary>
    /// Update the Roomba Mode
    /// </summary>
    [DisplayName("(User) UpdateMode")]
    [Description("Indicates an update to the current operating mode.")]
    public class UpdateMode : Update<ReturnMode, DsspResponsePort<DefaultUpdateResponseType>>
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public UpdateMode()
        {
            this.Body = new ReturnMode(RoombaMode.NotSpecified);
        }

        /// <summary>
        /// Initialization constructor
        /// </summary>
        /// <param name="roombaMode"></param>
        public UpdateMode(RoombaMode roombaMode)
        {
            this.Body = new ReturnMode(roombaMode);
        }

        /// <summary>
        /// Initialization constructor
        /// </summary>
        /// <param name="roombaMode"></param>
        /// <param name="maintainMode">The mode to be maintained after the current command completes</param>
        public UpdateMode(RoombaMode roombaMode, RoombaMode maintainMode)
        {
            this.Body = new ReturnMode(roombaMode, maintainMode);
        }

        /// <summary>
        /// Initialization constructor
        /// </summary>
        /// <param name="roombaMode"></param>
        /// <param name="robotModel"></param>
        /// <param name="firmwareDate"></param>
        public UpdateMode(RoombaMode roombaMode, IRobotModel robotModel, DateTime firmwareDate)
        {
            this.Body = new ReturnMode(roombaMode);
            this.Body.IRobotModel = robotModel;
            this.Body.FirmwareDate = firmwareDate;
        }

        /// <summary>
        /// Initialization constructor
        /// </summary>
        /// <param name="roombaState"></param>
        public UpdateMode(RoombaState roombaState)
        {
            if (roombaState == null)
                this.Body = new ReturnMode();
            else
                this.Body = new ReturnMode(roombaState.Mode, roombaState.MaintainMode, roombaState.IRobotModel, roombaState.FirmwareDate);
        }
    }
    #endregion

    #region Service Contract

    /// <summary>
    /// Roomba Service Contract
    /// </summary>
    [DisplayName("(User) iRobot� Sensors")]
    [Description("Provides access to iRobot Create or Roomba sensor notifications.\n(Partner with the 'iRobot� Create / Roomba' service.)")]
    public static class Contract
    {
        /// <summary>
        /// Roomba More Commands Service Contract
        /// </summary>
        public const string Identifier = "http://schemas.microsoft.com/robotics/2007/01/irobot/notifications.user.html";
    }

    #endregion

}

namespace Microsoft.Robotics.Services.IRobot.Create
{

    /// <summary>
    /// iRobot Create Commands
    /// </summary>
    [ServicePort]
    public class CreateOperations : PortSet
    {
        /// <summary>
        /// iRobot Create Commands
        /// </summary>
        public CreateOperations()
            : base(
                typeof(DsspDefaultLookup),
                typeof(CreateDemo),
                typeof(CreatePWMLowSideDrivers),
                typeof(CreateDriveDirect),
                typeof(CreateDigitalOutputs),
                typeof(CreateStream),
                typeof(CreateQueryList),
                typeof(CreateStreamPauseResume),
                typeof(CreateSendIR),
                typeof(CreateDefineScript),
                typeof(CreatePlayScript),
                typeof(CreateShowScript),
                typeof(CreateWaitTime),
                typeof(CreateWaitDistance),
                typeof(CreateWaitAngle),
                typeof(CreateWaitEvent))
        { }

        #region Implicit Operators


        /// <summary>
        /// Implicit Operator for Port of DsspDefaultLookup
        /// </summary>
        /// <param name="portSet"></param>
        /// <returns></returns>
        public static implicit operator Port<DsspDefaultLookup>(CreateOperations portSet)
        {
            if (portSet == null) return null;
            return (Port<DsspDefaultLookup>)portSet[typeof(DsspDefaultLookup)];
        }
        /// <summary>
        /// Implicit Operator for Port of CreateDemo
        /// </summary>
        /// <param name="portSet"></param>
        /// <returns></returns>
        public static implicit operator Port<CreateDemo>(CreateOperations portSet)
        {
            if (portSet == null) return null;
            return (Port<CreateDemo>)portSet[typeof(CreateDemo)];
        }
        /// <summary>
        /// Implicit Operator for Port of CreatePWMLowSideDrivers
        /// </summary>
        /// <param name="portSet"></param>
        /// <returns></returns>
        public static implicit operator Port<CreatePWMLowSideDrivers>(CreateOperations portSet)
        {
            if (portSet == null) return null;
            return (Port<CreatePWMLowSideDrivers>)portSet[typeof(CreatePWMLowSideDrivers)];
        }
        /// <summary>
        /// Implicit Operator for Port of CreateDriveDirect
        /// </summary>
        /// <param name="portSet"></param>
        /// <returns></returns>
        public static implicit operator Port<CreateDriveDirect>(CreateOperations portSet)
        {
            if (portSet == null) return null;
            return (Port<CreateDriveDirect>)portSet[typeof(CreateDriveDirect)];
        }
        /// <summary>
        /// Implicit Operator for Port of CreateDigitalOutputs
        /// </summary>
        /// <param name="portSet"></param>
        /// <returns></returns>
        public static implicit operator Port<CreateDigitalOutputs>(CreateOperations portSet)
        {
            if (portSet == null) return null;
            return (Port<CreateDigitalOutputs>)portSet[typeof(CreateDigitalOutputs)];
        }
        /// <summary>
        /// Implicit Operator for Port of CreateStream
        /// </summary>
        /// <param name="portSet"></param>
        /// <returns></returns>
        public static implicit operator Port<CreateStream>(CreateOperations portSet)
        {
            if (portSet == null) return null;
            return (Port<CreateStream>)portSet[typeof(CreateStream)];
        }
        /// <summary>
        /// Implicit Operator for Port of CreateQueryList
        /// </summary>
        /// <param name="portSet"></param>
        /// <returns></returns>
        public static implicit operator Port<CreateQueryList>(CreateOperations portSet)
        {
            if (portSet == null) return null;
            return (Port<CreateQueryList>)portSet[typeof(CreateQueryList)];
        }
        /// <summary>
        /// Implicit Operator for Port of CreateStreamPauseResume
        /// </summary>
        /// <param name="portSet"></param>
        /// <returns></returns>
        public static implicit operator Port<CreateStreamPauseResume>(CreateOperations portSet)
        {
            if (portSet == null) return null;
            return (Port<CreateStreamPauseResume>)portSet[typeof(CreateStreamPauseResume)];
        }
        /// <summary>
        /// Implicit Operator for Port of CreateSendIR
        /// </summary>
        /// <param name="portSet"></param>
        /// <returns></returns>
        public static implicit operator Port<CreateSendIR>(CreateOperations portSet)
        {
            if (portSet == null) return null;
            return (Port<CreateSendIR>)portSet[typeof(CreateSendIR)];
        }
        /// <summary>
        /// Implicit Operator for Port of CreateDefineScript
        /// </summary>
        /// <param name="portSet"></param>
        /// <returns></returns>
        public static implicit operator Port<CreateDefineScript>(CreateOperations portSet)
        {
            if (portSet == null) return null;
            return (Port<CreateDefineScript>)portSet[typeof(CreateDefineScript)];
        }
        /// <summary>
        /// Implicit Operator for Port of CreatePlayScript
        /// </summary>
        /// <param name="portSet"></param>
        /// <returns></returns>
        public static implicit operator Port<CreatePlayScript>(CreateOperations portSet)
        {
            if (portSet == null) return null;
            return (Port<CreatePlayScript>)portSet[typeof(CreatePlayScript)];
        }
        /// <summary>
        /// Implicit Operator for Port of CreateShowScript
        /// </summary>
        /// <param name="portSet"></param>
        /// <returns></returns>
        public static implicit operator Port<CreateShowScript>(CreateOperations portSet)
        {
            if (portSet == null) return null;
            return (Port<CreateShowScript>)portSet[typeof(CreateShowScript)];
        }
        /// <summary>
        /// Implicit Operator for Port of CreateWaitTime
        /// </summary>
        /// <param name="portSet"></param>
        /// <returns></returns>
        public static implicit operator Port<CreateWaitTime>(CreateOperations portSet)
        {
            if (portSet == null) return null;
            return (Port<CreateWaitTime>)portSet[typeof(CreateWaitTime)];
        }
        /// <summary>
        /// Implicit Operator for Port of CreateWaitDistance
        /// </summary>
        /// <param name="portSet"></param>
        /// <returns></returns>
        public static implicit operator Port<CreateWaitDistance>(CreateOperations portSet)
        {
            if (portSet == null) return null;
            return (Port<CreateWaitDistance>)portSet[typeof(CreateWaitDistance)];
        }
        /// <summary>
        /// Implicit Operator for Port of CreateWaitAngle
        /// </summary>
        /// <param name="portSet"></param>
        /// <returns></returns>
        public static implicit operator Port<CreateWaitAngle>(CreateOperations portSet)
        {
            if (portSet == null) return null;
            return (Port<CreateWaitAngle>)portSet[typeof(CreateWaitAngle)];
        }
        /// <summary>
        /// Implicit Operator for Port of CreateWaitEvent
        /// </summary>
        /// <param name="portSet"></param>
        /// <returns></returns>
        public static implicit operator Port<CreateWaitEvent>(CreateOperations portSet)
        {
            if (portSet == null) return null;
            return (Port<CreateWaitEvent>)portSet[typeof(CreateWaitEvent)];
        }

        #endregion

        /// <summary>
        /// Generic Post
        /// </summary>
        /// <param name="op"></param>
        public void Post(object op)
        {
            this.PostUnknownType(op);
        }

        #region Operation Helper Methods

        /// <summary>
        /// Post DsspDefaultLookup and return the response port.
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        public virtual PortSet<LookupResponse, Fault> DsspDefaultLookup(LookupRequestType body)
        {
            DsspDefaultLookup op = new DsspDefaultLookup();
            op.Body = body ?? new LookupRequestType();
            this.PostUnknownType(op);
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
            this.PostUnknownType(op);
            return op.ResponsePort;

        }
        /// <summary>
        /// Post CreateDemo and return the response port.
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        public virtual StandardResponse CreateDemo(CmdDemo body)
        {
            CreateDemo op = new CreateDemo();
            op.Body = body ?? new CmdDemo();
            this.PostUnknownType(op);
            return op.ResponsePort;

        }
        /// <summary>
        /// Post CreatePWMLowSideDrivers and return the response port.
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        public virtual StandardResponse CreatePWMLowSideDrivers(CmdPWMLowSideDrivers body)
        {
            CreatePWMLowSideDrivers op = new CreatePWMLowSideDrivers();
            op.Body = body ?? new CmdPWMLowSideDrivers();
            this.PostUnknownType(op);
            return op.ResponsePort;

        }
        /// <summary>
        /// Post CreateDriveDirect and return the response port.
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        public virtual StandardResponse CreateDriveDirect(CmdDriveDirect body)
        {
            CreateDriveDirect op = new CreateDriveDirect();
            op.Body = body ?? new CmdDriveDirect();
            this.PostUnknownType(op);
            return op.ResponsePort;

        }
        /// <summary>
        /// Post CreateDigitalOutputs and return the response port.
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        public virtual StandardResponse CreateDigitalOutputs(CmdDigitalOutputs body)
        {
            CreateDigitalOutputs op = new CreateDigitalOutputs();
            op.Body = body ?? new CmdDigitalOutputs();
            this.PostUnknownType(op);
            return op.ResponsePort;

        }
        /// <summary>
        /// Post CreateStream and return the response port.
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        public virtual StandardResponse CreateStream(CmdStream body)
        {
            CreateStream op = new CreateStream();
            op.Body = body ?? new CmdStream();
            this.PostUnknownType(op);
            return op.ResponsePort;

        }
        /// <summary>
        /// Post CreateQueryList and return the response port.
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        public virtual PortSet<ReturnQueryList, Fault> CreateQueryList(CmdQueryList body)
        {
            CreateQueryList op = new CreateQueryList();
            op.Body = body ?? new CmdQueryList();
            this.PostUnknownType(op);
            return op.ResponsePort;

        }
        /// <summary>
        /// Post CreateStreamPauseResume and return the response port.
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        public virtual StandardResponse CreateStreamPauseResume(CmdStreamPauseResume body)
        {
            CreateStreamPauseResume op = new CreateStreamPauseResume();
            op.Body = body ?? new CmdStreamPauseResume();
            this.PostUnknownType(op);
            return op.ResponsePort;

        }
        /// <summary>
        /// Post CreateSendIR and return the response port.
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        public virtual StandardResponse CreateSendIR(CmdSendIR body)
        {
            CreateSendIR op = new CreateSendIR();
            op.Body = body ?? new CmdSendIR();
            this.PostUnknownType(op);
            return op.ResponsePort;

        }
        /// <summary>
        /// Post CreateDefineScript and return the response port.
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        public virtual PortSet<ReturnDefineScript, Fault> CreateDefineScript(CmdDefineScript body)
        {
            CreateDefineScript op = new CreateDefineScript();
            op.Body = body ?? new CmdDefineScript();
            this.PostUnknownType(op);
            return op.ResponsePort;

        }
        /// <summary>
        /// Post CreatePlayScript and return the response port.
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        public virtual StandardResponse CreatePlayScript(CmdPlayScript body)
        {
            CreatePlayScript op = new CreatePlayScript();
            if (body != null)
                op.Body = body;
            this.PostUnknownType(op);
            return op.ResponsePort;
        }

        /// <summary>
        /// Post CreatePlayScript and return the response port.
        /// </summary>
        /// <returns></returns>
        public virtual StandardResponse CreatePlayScript()
        {
            return CreatePlayScript(null);
        }

        /// <summary>
        /// Post CreateShowScript and return the response port.
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        public virtual PortSet<ReturnScript, Fault> CreateShowScript(CmdShowScript body)
        {
            CreateShowScript op = new CreateShowScript();
            op.Body = body ?? new CmdShowScript();
            this.PostUnknownType(op);
            return op.ResponsePort;

        }
        /// <summary>
        /// Post CreateWaitTime and return the response port.
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        public virtual StandardResponse CreateWaitTime(CmdWaitTime body)
        {
            CreateWaitTime op = new CreateWaitTime();
            op.Body = body ?? new CmdWaitTime();
            this.PostUnknownType(op);
            return op.ResponsePort;

        }
        /// <summary>
        /// Post CreateWaitDistance and return the response port.
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        public virtual StandardResponse CreateWaitDistance(CmdWaitDistance body)
        {
            CreateWaitDistance op = new CreateWaitDistance();
            op.Body = body ?? new CmdWaitDistance();
            this.PostUnknownType(op);
            return op.ResponsePort;

        }
        /// <summary>
        /// Post CreateWaitAngle and return the response port.
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        public virtual StandardResponse CreateWaitAngle(CmdWaitAngle body)
        {
            CreateWaitAngle op = new CreateWaitAngle();
            op.Body = body ?? new CmdWaitAngle();
            this.PostUnknownType(op);
            return op.ResponsePort;

        }
        /// <summary>
        /// Post CreateWaitEvent and return the response port.
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        public virtual StandardResponse CreateWaitEvent(CmdWaitEvent body)
        {
            CreateWaitEvent op = new CreateWaitEvent();
            op.Body = body ?? new CmdWaitEvent();
            this.PostUnknownType(op);
            return op.ResponsePort;

        }

        #endregion

    }


    #region Service Contract
    /// <summary>
    /// Create Service Contract
    /// </summary>
    [DisplayName("(User) iRobot� Create Supplemental")]
    [Description("Provides access to additional operations for the iRobot Create.\n(Partner with the 'iRobot� Create / Roomba' service.)")]
    public static class Contract
    {
        /// <summary>
        /// Create Service Contract
        /// </summary>
        public const string Identifier = "http://schemas.microsoft.com/robotics/2007/01/irobot/create.user.html";
    }
    #endregion
}

namespace Microsoft.Robotics.Services.IRobot.Roomba
{

    /// <summary>
    /// Current state of the Roomba
    /// </summary>
    [DataContract]
    [Description("Specifies the state of the iRobot service.")]
    public class RoombaState
    {
        /// <summary>
        /// The robot Name
        /// </summary>
        [DataMember(IsRequired = false)]
        [Description("Specifies a user friendly name for this robot.")]
        public String Name;

        /// <summary>
        /// The Firmware Date
        /// </summary>
        [DataMember]
        [Description("Specifies the date of the robot's firmware.")]
        [Browsable(false)]
        public DateTime FirmwareDate;

        /// <summary>
        /// Serial Port connection
        /// </summary>
        [DataMember]
        [Description ("Specifies the serial port used for the connection to the robot.")]
        public int SerialPort;

        /// <summary>
        /// Connection Baud Rate
        /// <remarks>0 uses the default Baud rate</remarks>
        /// </summary>
        [DataMember(IsRequired = false)]
        [Description("Specifies the baud rate for connection.\n (0 - default)")]
        public int BaudRate;

        /// <summary>
        /// The type of iRobot Create or Roomba
        /// </summary>
        [DataMember]
        [Description("The type of robot model.\n(Create or Roomba)")]
        public IRobotModel IRobotModel;

        /// <summary>
        /// The connection type
        /// </summary>
        [DataMember]
        [Description("Specifies how the robot is connected with the service host.")]
        public iRobotConnectionType ConnectionType;

        /// <summary>
        /// Stop Motors when sensor data is garbled
        /// </summary>
        [DataMember(IsRequired=false)]
        [Description("Specifies to stop when sensor data is scrambled and attempt to resync communications.")]
        public bool StopOnResync;

        /// <summary>
        /// SCI Mode
        /// </summary>
        [DataMember(IsRequired = false)]
        [Description ("Specifies the robot's current operational mode.")]
        [Browsable(false)]
        public RoombaMode Mode;

        /// <summary>
        /// The SCI Mode to be maintained
        /// </summary>
        [DataMember(IsRequired = false)]
        [Description("Specifies whether to maintain the operational mode.\n(Passive, Safe, or Full; NotSpecified turns it off.)")]
        public RoombaMode MaintainMode;

        /// <summary>
        ///  Sensors
        /// </summary>
        [DataMember(IsRequired = false)]
        [Description("Identifies the robot's set of sensors.")]
        [Browsable(false)]
        public ReturnSensors Sensors;

        /// <summary>
        /// Pose
        /// </summary>
        [DataMember(IsRequired = false)]
        [Description ("Indicates the position and orientation of the robot.")]
        [Browsable(false)]
        public ReturnPose Pose;

        /// <summary>
        /// Power
        /// </summary>
        [DataMember(IsRequired = false)]
        [Description ("Indicates the robot's current power reading.")]
        [Browsable(false)]
        public ReturnPower Power;

        /// <summary>
        /// Date last updated
        /// </summary>
        [DataMember(IsRequired = false)]
        [Description("Indicates the time of the most current state update.")]
        [Browsable(false)]
        public DateTime LastUpdated;

        /// <summary>
        /// Polling interval in ms
        /// <remarks>
        ///    -1 = no polling (less than zero)
        /// 0-199 = default for Create/Roomba
        /// 200-N = ms between polling
        /// </remarks>
        /// </summary>
        [DataMember(IsRequired = false)]
        [Description("Specifies the polling interval (in ms).\n(0 = default, -1 = Off, > 0 = ms)")]
        public int PollingInterval;

        /// <summary>
        /// Song Definitions
        /// </summary>
        [DataMember(IsRequired = false)]
        [Description("Specifies a set of song definitions.")]
        public List<CmdDefineSong> SongDefinitions;

        /// <summary>
        /// Wait for Connect before establishing a connection with the iRobot.
        /// </summary>
        [DataMember(IsRequired = false)]
        [Description("Specifies to pause briefly when establishing a connection with the robot.")]
        public bool WaitForConnect;

        /// <summary>
        /// An Image which represents this iRobot.
        /// </summary>
        [DataMember(IsRequired=false)]
        [Description("Identifies an image that represents the robot.")]
        [Browsable(false)]
        public byte[] RobotImage;

        // *********************************************************
        // iRobot Create specific data types
        // *********************************************************

        /// <summary>
        /// iRobot Create Cliff Detail
        /// </summary>
        [DataMember(IsRequired = false)]
        [Browsable(false)]
        [Description("Identifies Create cliff sensor data.\n(Supported only for Create.)")]
        public ReturnCliffDetail CliffDetail;


        /// <summary>
        /// iRobot Create Telemetry
        /// </summary>
        [DataMember(IsRequired = false)]
        [Description("Identifies Create telemetry.\n(Supported only for Create.)")]
        [Browsable(false)]
        public ReturnTelemetry Telemetry;

        /// <summary>
        /// Notifications which will be requested
        /// when connecting to an iRobot Create
        /// </summary>
        [DataMember(IsRequired = false)]
        [Description("Specifies the set of notifications to be returned.\n(Supported only for Create.)")]
        public List<CreateSensorPacket> CreateNotifications;

        // *********************************************************
        // Determine if we have established a two-way connection with the iRobot
        private bool _isInitialized = false;
        internal bool IsInitialized
        {
            get
            {
                if (_isInitialized)
                    return true;

                if (this.Telemetry != null)
                {
                    if (this.Telemetry.OIMode == RoombaMode.Uninitialized)
                        return false;

                    _isInitialized = true;
                    return true;
                }

                return (this.Mode != RoombaMode.Uninitialized);
            }
            set { _isInitialized = value; }
        }

    }

    /// <summary>
    /// IRobot Connect Message
    /// </summary>
    [DataContract]
    [Description("Connect to the robot.")]
    public class IRobotConnect{ }


    /// <summary>
    /// Roomba Operations
    /// </summary>
    [ServicePort]
    public class RoombaOperations : PortSet
    {
        /// <summary>
        /// iRobot Roomba Operations
        /// </summary>
        public RoombaOperations()
            : base(
                typeof(DsspDefaultLookup),
                typeof(DsspDefaultDrop),
                typeof(Connect),
                typeof(Configure),
                typeof(Get),
                typeof(HttpGet),
                typeof(HttpPost),
                /****************/
                typeof(RoombaSetMode),
                typeof(RoombaDrive),
                typeof(RoombaSetCleaningMotors),
                typeof(RoombaSetLeds),
                typeof(RoombaDefineSong),
                typeof(RoombaPlaySong),
                typeof(RoombaGetSensors),
                typeof(RoombaSeekDock),
                typeof(RoombaStartCleaning),
                typeof(RoombaStartSpotCleaning),
                typeof(RoombaStartMaxCleaning),
                typeof(RoombaGetFirmwareDate))
        { }

        #region Implicit Operators

        /// <summary>
        /// Implicit Operator for Port of DsspDefaultLookup
        /// </summary>
        /// <param name="portSet"></param>
        /// <returns></returns>
        public static implicit operator Port<DsspDefaultLookup>(RoombaOperations portSet)
        {
            if (portSet == null) return null;
            return (Port<DsspDefaultLookup>)portSet[typeof(DsspDefaultLookup)];
        }
        /// <summary>
        /// Implicit Operator for Port of DsspDefaultDrop
        /// </summary>
        /// <param name="portSet"></param>
        /// <returns></returns>
        public static implicit operator Port<DsspDefaultDrop>(RoombaOperations portSet)
        {
            if (portSet == null) return null;
            return (Port<DsspDefaultDrop>)portSet[typeof(DsspDefaultDrop)];
        }
        /// <summary>
        /// Implicit Operator for Port of Configure
        /// </summary>
        /// <param name="portSet"></param>
        /// <returns></returns>
        public static implicit operator Port<Configure>(RoombaOperations portSet)
        {
            if (portSet == null) return null;
            return (Port<Configure>)portSet[typeof(Configure)];
        }
        /// <summary>
        /// Implicit Operator for Port of Connect
        /// </summary>
        /// <param name="portSet"></param>
        /// <returns></returns>
        public static implicit operator Port<Connect>(RoombaOperations portSet)
        {
            if (portSet == null) return null;
            return (Port<Connect>)portSet[typeof(Connect)];
        }
        /// <summary>
        /// Implicit Operator for Port of Get
        /// </summary>
        /// <param name="portSet"></param>
        /// <returns></returns>
        public static implicit operator Port<Get>(RoombaOperations portSet)
        {
            if (portSet == null) return null;
            return (Port<Get>)portSet[typeof(Get)];
        }
        /// <summary>
        /// Implicit Operator for Port of Microsoft.Dss.Core.DsspHttp.HttpGet
        /// </summary>
        /// <param name="portSet"></param>
        /// <returns></returns>
        public static implicit operator Port<Microsoft.Dss.Core.DsspHttp.HttpGet>(RoombaOperations portSet)
        {
            if (portSet == null) return null;
            return (Port<Microsoft.Dss.Core.DsspHttp.HttpGet>)portSet[typeof(Microsoft.Dss.Core.DsspHttp.HttpGet)];
        }
        /// <summary>
        /// Implicit Operator for Port of Microsoft.Dss.Core.DsspHttp.HttpPost
        /// </summary>
        /// <param name="portSet"></param>
        /// <returns></returns>
        public static implicit operator Port<Microsoft.Dss.Core.DsspHttp.HttpPost>(RoombaOperations portSet)
        {
            if (portSet == null) return null;
            return (Port<Microsoft.Dss.Core.DsspHttp.HttpPost>)portSet[typeof(Microsoft.Dss.Core.DsspHttp.HttpPost)];
        }
        /// <summary>
        /// Implicit Operator for Port of RoombaSetMode
        /// </summary>
        /// <param name="portSet"></param>
        /// <returns></returns>
        public static implicit operator Port<RoombaSetMode>(RoombaOperations portSet)
        {
            if (portSet == null) return null;
            return (Port<RoombaSetMode>)portSet[typeof(RoombaSetMode)];
        }
        /// <summary>
        /// Implicit Operator for Port of RoombaDrive
        /// </summary>
        /// <param name="portSet"></param>
        /// <returns></returns>
        public static implicit operator Port<RoombaDrive>(RoombaOperations portSet)
        {
            if (portSet == null) return null;
            return (Port<RoombaDrive>)portSet[typeof(RoombaDrive)];
        }
        /// <summary>
        /// Implicit Operator for Port of RoombaSetCleaningMotors
        /// </summary>
        /// <param name="portSet"></param>
        /// <returns></returns>
        public static implicit operator Port<RoombaSetCleaningMotors>(RoombaOperations portSet)
        {
            if (portSet == null) return null;
            return (Port<RoombaSetCleaningMotors>)portSet[typeof(RoombaSetCleaningMotors)];
        }
        /// <summary>
        /// Implicit Operator for Port of RoombaSetLeds
        /// </summary>
        /// <param name="portSet"></param>
        /// <returns></returns>
        public static implicit operator Port<RoombaSetLeds>(RoombaOperations portSet)
        {
            if (portSet == null) return null;
            return (Port<RoombaSetLeds>)portSet[typeof(RoombaSetLeds)];
        }
        /// <summary>
        /// Implicit Operator for Port of RoombaDefineSong
        /// </summary>
        /// <param name="portSet"></param>
        /// <returns></returns>
        public static implicit operator Port<RoombaDefineSong>(RoombaOperations portSet)
        {
            if (portSet == null) return null;
            return (Port<RoombaDefineSong>)portSet[typeof(RoombaDefineSong)];
        }
        /// <summary>
        /// Implicit Operator for Port of RoombaPlaySong
        /// </summary>
        /// <param name="portSet"></param>
        /// <returns></returns>
        public static implicit operator Port<RoombaPlaySong>(RoombaOperations portSet)
        {
            if (portSet == null) return null;
            return (Port<RoombaPlaySong>)portSet[typeof(RoombaPlaySong)];
        }
        /// <summary>
        /// Implicit Operator for Port of RoombaGetSensors
        /// </summary>
        /// <param name="portSet"></param>
        /// <returns></returns>
        public static implicit operator Port<RoombaGetSensors>(RoombaOperations portSet)
        {
            if (portSet == null) return null;
            return (Port<RoombaGetSensors>)portSet[typeof(RoombaGetSensors)];
        }
        /// <summary>
        /// Implicit Operator for Port of RoombaSeekDock
        /// </summary>
        /// <param name="portSet"></param>
        /// <returns></returns>
        public static implicit operator Port<RoombaSeekDock>(RoombaOperations portSet)
        {
            if (portSet == null) return null;
            return (Port<RoombaSeekDock>)portSet[typeof(RoombaSeekDock)];
        }
        /// <summary>
        /// Implicit Operator for Port of RoombaStartCleaning
        /// </summary>
        /// <param name="portSet"></param>
        /// <returns></returns>
        public static implicit operator Port<RoombaStartCleaning>(RoombaOperations portSet)
        {
            if (portSet == null) return null;
            return (Port<RoombaStartCleaning>)portSet[typeof(RoombaStartCleaning)];
        }
        /// <summary>
        /// Implicit Operator for Port of RoombaStartSpotCleaning
        /// </summary>
        /// <param name="portSet"></param>
        /// <returns></returns>
        public static implicit operator Port<RoombaStartSpotCleaning>(RoombaOperations portSet)
        {
            if (portSet == null) return null;
            return (Port<RoombaStartSpotCleaning>)portSet[typeof(RoombaStartSpotCleaning)];
        }
        /// <summary>
        /// Implicit Operator for Port of RoombaStartMaxCleaning
        /// </summary>
        /// <param name="portSet"></param>
        /// <returns></returns>
        public static implicit operator Port<RoombaStartMaxCleaning>(RoombaOperations portSet)
        {
            if (portSet == null) return null;
            return (Port<RoombaStartMaxCleaning>)portSet[typeof(RoombaStartMaxCleaning)];
        }
        /// <summary>
        /// Implicit Operator for Port of RoombaGetFirmwareDate
        /// </summary>
        /// <param name="portSet"></param>
        /// <returns></returns>
        public static implicit operator Port<RoombaGetFirmwareDate>(RoombaOperations portSet)
        {
            if (portSet == null) return null;
            return (Port<RoombaGetFirmwareDate>)portSet[typeof(RoombaGetFirmwareDate)];
        }

        #endregion

        /// <summary>
        /// Generic Post
        /// </summary>
        /// <param name="op"></param>
        public void Post(object op)
        {
            this.PostUnknownType(op);
        }

        #region Operation Helper Methods

        /// <summary>
        /// Post Dssp Default Lookup and return the response port.
        /// </summary>
        public virtual PortSet<Microsoft.Dss.ServiceModel.Dssp.LookupResponse, Fault> DsspDefaultLookup(Microsoft.Dss.ServiceModel.Dssp.LookupRequestType body)
        {
            Microsoft.Dss.ServiceModel.Dssp.DsspDefaultLookup op = new Microsoft.Dss.ServiceModel.Dssp.DsspDefaultLookup();
            op.Body = body ?? new Microsoft.Dss.ServiceModel.Dssp.LookupRequestType();
            this.Post(op);
            return op.ResponsePort;

        }
        /// <summary>
        /// Post Dssp Default Lookup and return the response port.
        /// </summary>
        public virtual PortSet<Microsoft.Dss.ServiceModel.Dssp.LookupResponse, Fault> DsspDefaultLookup()
        {
            Microsoft.Dss.ServiceModel.Dssp.DsspDefaultLookup op = new Microsoft.Dss.ServiceModel.Dssp.DsspDefaultLookup();
            op.Body = new Microsoft.Dss.ServiceModel.Dssp.LookupRequestType();
            this.Post(op);
            return op.ResponsePort;

        }
        /// <summary>
        /// Post Dssp Default Drop and return the response port.
        /// </summary>
        public virtual PortSet<Microsoft.Dss.ServiceModel.Dssp.DefaultDropResponseType, Fault> DsspDefaultDrop(Microsoft.Dss.ServiceModel.Dssp.DropRequestType body)
        {
            Microsoft.Dss.ServiceModel.Dssp.DsspDefaultDrop op = new Microsoft.Dss.ServiceModel.Dssp.DsspDefaultDrop();
            op.Body = body ?? new Microsoft.Dss.ServiceModel.Dssp.DropRequestType();
            this.Post(op);
            return op.ResponsePort;

        }
        /// <summary>
        /// Post Dssp Default Drop and return the response port.
        /// </summary>
        public virtual PortSet<Microsoft.Dss.ServiceModel.Dssp.DefaultDropResponseType, Fault> DsspDefaultDrop()
        {
            Microsoft.Dss.ServiceModel.Dssp.DsspDefaultDrop op = new Microsoft.Dss.ServiceModel.Dssp.DsspDefaultDrop();
            op.Body = new Microsoft.Dss.ServiceModel.Dssp.DropRequestType();
            this.Post(op);
            return op.ResponsePort;

        }

        /// <summary>
        /// Post Connect and return the response port.
        /// </summary>
        public virtual PortSet<Microsoft.Dss.ServiceModel.Dssp.DefaultUpdateResponseType, Fault> Connect()
        {
            Connect op = new Connect();
            this.Post(op);
            return op.ResponsePort;
        }

        /// <summary>
        /// Post Connect and return the response port.
        /// </summary>
        public virtual PortSet<Microsoft.Dss.ServiceModel.Dssp.DefaultUpdateResponseType, Fault> Connect(IRobotConnect body)
        {
            Connect op = new Connect(body);
            if (op.Body == null)
                op.Body = new IRobotConnect();
            this.Post(op);
            return op.ResponsePort;
        }

        /// <summary>
        /// Post Configure and return the response port.
        /// </summary>
        public virtual PortSet<Microsoft.Dss.ServiceModel.Dssp.DefaultReplaceResponseType, Fault> Configure(RoombaState body)
        {
            Configure op = new Configure();
            op.Body = body ?? new RoombaState();
            this.Post(op);
            return op.ResponsePort;

        }
        /// <summary>
        /// Post Get and return the response port.
        /// </summary>
        public virtual PortSet<RoombaState, Fault> Get(Microsoft.Dss.ServiceModel.Dssp.GetRequestType body)
        {
            Get op = new Get();
            op.Body = body ?? new Microsoft.Dss.ServiceModel.Dssp.GetRequestType();
            this.Post(op);
            return op.ResponsePort;

        }
        /// <summary>
        /// Post Get and return the response port.
        /// </summary>
        public virtual PortSet<RoombaState, Fault> Get()
        {
            Get op = new Get();
            op.Body = new Microsoft.Dss.ServiceModel.Dssp.GetRequestType();
            this.Post(op);
            return op.ResponsePort;

        }
        /// <summary>
        /// Post Http Get and return the response port.
        /// </summary>
        public virtual PortSet<Microsoft.Dss.Core.DsspHttp.HttpResponseType, Fault> HttpGet(Microsoft.Dss.Core.DsspHttp.HttpGetRequestType body)
        {
            Microsoft.Dss.Core.DsspHttp.HttpGet op = new Microsoft.Dss.Core.DsspHttp.HttpGet();
            op.Body = body ?? new Microsoft.Dss.Core.DsspHttp.HttpGetRequestType();
            this.Post(op);
            return op.ResponsePort;

        }
        /// <summary>
        /// Post Http Post and return the response port.
        /// </summary>
        public virtual PortSet<Microsoft.Dss.Core.DsspHttp.HttpResponseType, Fault> HttpPost(Microsoft.Dss.Core.DsspHttp.HttpPostRequestType body)
        {
            Microsoft.Dss.Core.DsspHttp.HttpPost op = new Microsoft.Dss.Core.DsspHttp.HttpPost();
            op.Body = body ?? new Microsoft.Dss.Core.DsspHttp.HttpPostRequestType();
            this.Post(op);
            return op.ResponsePort;

        }
        /// <summary>
        /// Set the iRobot Mode.
        /// </summary>
        public virtual StandardResponse RoombaSetMode(RoombaMode roombaMode, bool maintainMode)
        {
            CmdSetMode body = new CmdSetMode(roombaMode, maintainMode);
            RoombaSetMode op = new RoombaSetMode(body);
            this.Post(op);
            return op.ResponsePort;

        }
        /// <summary>
        /// Post Roomba Set Mode and return the response port.
        /// </summary>
        public virtual StandardResponse RoombaSetMode(CmdSetMode body)
        {
            RoombaSetMode op = new RoombaSetMode();
            op.Body = body ?? new CmdSetMode();
            this.Post(op);
            return op.ResponsePort;

        }
        /// <summary>
        /// Controls Roomba's drive wheels.
        /// <remarks>The command takes four data
        /// bytes, which are interpreted as two 16 bit signed values using
        /// twos-complement. The first two bytes specify the average velocity
        /// of the drive wheels in millimeters per second (mm/s), with the
        /// high byte sent first. The next two bytes specify the radius, in
        /// millimeters, at which Roomba should turn. The longer radii make
        /// Roomba drive straighter; shorter radii make it turn more. A Drive
        /// command with a positive velocity and a positive radius will make
        /// Roomba drive forward while turning toward the left. A negative
        /// radius will make it turn toward the right. Special cases for the
        /// radius make Roomba turn in place or drive straight, as specified
        /// below. </remarks>
        /// </summary>
        public virtual StandardResponse RoombaDrive(int velocity, int radius)
        {
            CmdDrive body = new CmdDrive(velocity, radius);
            RoombaDrive op = new RoombaDrive(body);
            this.Post(op);
            return op.ResponsePort;

        }
        /// <summary>
        /// Post Roomba Drive and return the response port.
        /// </summary>
        public virtual StandardResponse RoombaDrive(CmdDrive body)
        {
            RoombaDrive op = new RoombaDrive();
            op.Body = body ?? new CmdDrive();
            this.Post(op);
            return op.ResponsePort;

        }
        /// <summary>
        /// Post Roomba Set Cleaning Motors and return the response port.
        /// </summary>
        public virtual StandardResponse RoombaSetCleaningMotors(CmdMotors body)
        {
            RoombaSetCleaningMotors op = new RoombaSetCleaningMotors();
            op.Body = body ?? new CmdMotors();
            this.Post(op);
            return op.ResponsePort;

        }
        /// <summary>
        /// Post Roomba Set Leds and return the response port.
        /// </summary>
        public virtual StandardResponse RoombaSetLeds(CmdLeds body)
        {
            RoombaSetLeds op = new RoombaSetLeds();
            op.Body = body ?? new CmdLeds();
            this.Post(op);
            return op.ResponsePort;

        }
        /// <summary>
        /// Plays one of 16 songs, as specified by an earlier Song command.
        /// <remarks>If the requested song has not been specified yet,
        /// the Play command does nothing. </remarks>
        /// </summary>
        public virtual StandardResponse RoombaDefineSong(int songNumber)
        {
            CmdDefineSong body = new CmdDefineSong(songNumber);
            RoombaDefineSong op = new RoombaDefineSong(body);
            this.Post(op);
            return op.ResponsePort;

        }
        /// <summary>
        /// Post Roomba Define Song and return the response port.
        /// </summary>
        public virtual StandardResponse RoombaDefineSong(CmdDefineSong body)
        {
            RoombaDefineSong op = new RoombaDefineSong();
            op.Body = body ?? new CmdDefineSong();
            this.Post(op);
            return op.ResponsePort;

        }
        /// <summary>
        /// Plays one of 16 songs, as specified by an earlier Song command.
        /// <remarks>If the requested song has not been specified yet,
        /// the Play command does nothing. </remarks>
        /// </summary>
        public virtual StandardResponse RoombaPlaySong(int songNumber)
        {
            CmdPlaySong body = new CmdPlaySong(songNumber);
            RoombaPlaySong op = new RoombaPlaySong(body);
            this.Post(op);
            return op.ResponsePort;

        }
        /// <summary>
        /// Post Roomba Play Song and return the response port.
        /// </summary>
        public virtual StandardResponse RoombaPlaySong(CmdPlaySong body)
        {
            RoombaPlaySong op = new RoombaPlaySong();
            op.Body = body ?? new CmdPlaySong();
            this.Post(op);
            return op.ResponsePort;

        }

        /// <summary>
        /// Post Roomba Get Sensors and return the response port.
        /// </summary>
        public virtual GetSensorsResponse RoombaGetSensors(CmdSensors body)
        {
            RoombaGetSensors op = new RoombaGetSensors();
            op.Body = body ?? new CmdSensors();
            this.Post(op);
            return op.ResponsePort;

        }

        /// <summary>
        /// Post Roomba Seek Dock and return the response port.
        /// </summary>
        public virtual StandardResponse RoombaSeekDock(CmdForceSeekingDock body)
        {
            RoombaSeekDock op = new RoombaSeekDock();
            op.Body = body ?? new CmdForceSeekingDock();
            this.Post(op);
            return op.ResponsePort;

        }
        /// <summary>
        /// Post Roomba Start Cleaning and return the response port.
        /// </summary>
        public virtual StandardResponse RoombaStartCleaning(CmdClean body)
        {
            RoombaStartCleaning op = new RoombaStartCleaning();
            op.Body = body ?? new CmdClean();
            this.Post(op);
            return op.ResponsePort;

        }
        /// <summary>
        /// Post Roomba Start Spot Cleaning and return the response port.
        /// </summary>
        public virtual StandardResponse RoombaStartSpotCleaning(CmdSpot body)
        {
            RoombaStartSpotCleaning op = new RoombaStartSpotCleaning();
            op.Body = body ?? new CmdSpot();
            this.Post(op);
            return op.ResponsePort;

        }
        /// <summary>
        /// Post Roomba Start Max Cleaning and return the response port.
        /// </summary>
        public virtual StandardResponse RoombaStartMaxCleaning(CmdMax body)
        {
            RoombaStartMaxCleaning op = new RoombaStartMaxCleaning();
            op.Body = body ?? new CmdMax();
            this.Post(op);
            return op.ResponsePort;

        }
        /// <summary>
        /// Post Roomba Get Firmware Date and return the response port.
        /// </summary>
        public virtual PortSet<ReturnFirmwareDate, Fault> RoombaGetFirmwareDate(CmdFirmwareDate body)
        {
            RoombaGetFirmwareDate op = new RoombaGetFirmwareDate();
            op.Body = body ?? new CmdFirmwareDate();
            this.Post(op);
            return op.ResponsePort;

        }


        #endregion

    }

    /// <summary>
    /// The Standard iRobot command response.
    /// </summary>
    public class StandardResponse : PortSet<RoombaCommandReceived, Fault> { };

    /// <summary>
    /// The response type for RoombaGetSensors
    /// </summary>
    public class GetSensorsResponse : PortSet
    {
        /// <summary>
        /// The response type for RoombaGetSensors
        /// </summary>
        public GetSensorsResponse(): base(
            typeof(ReturnAll),
            typeof(ReturnSensors),
            typeof(ReturnPose),
            typeof(ReturnPower),
            typeof(ReturnCliffDetail),
            typeof(ReturnTelemetry),
            typeof(ReturnFirmwareDate),
            typeof(ReturnMode),
            typeof(ReturnQueryList),
            typeof(Fault) )
        {
        }

        #region Implicit Operators
        /// <summary>
        /// Implicit operator for ReturnAll
        /// </summary>
        /// <param name="portSet"></param>
        /// <returns></returns>
        public static implicit operator Port<ReturnAll>(GetSensorsResponse portSet)
        {
            if (portSet == null) return null;
            return (Port<ReturnAll>)portSet[typeof(ReturnAll)];
        }
        /// <summary>
        /// Implicit operator for ReturnSensors
        /// </summary>
        /// <param name="portSet"></param>
        /// <returns></returns>
        public static implicit operator Port<ReturnSensors>(GetSensorsResponse portSet)
        {
            if (portSet == null) return null;
            return (Port<ReturnSensors>)portSet[typeof(ReturnSensors)];
        }
        /// <summary>
        /// Implicit operator for ReturnPose
        /// </summary>
        /// <param name="portSet"></param>
        /// <returns></returns>
        public static implicit operator Port<ReturnPose>(GetSensorsResponse portSet)
        {
            if (portSet == null) return null;
            return (Port<ReturnPose>)portSet[typeof(ReturnPose)];
        }
        /// <summary>
        /// Implicit operator for ReturnPower
        /// </summary>
        /// <param name="portSet"></param>
        /// <returns></returns>
        public static implicit operator Port<ReturnPower>(GetSensorsResponse portSet)
        {
            if (portSet == null) return null;
            return (Port<ReturnPower>)portSet[typeof(ReturnPower)];
        }
        /// <summary>
        /// Implicit operator for ReturnCliffDetail
        /// </summary>
        /// <param name="portSet"></param>
        /// <returns></returns>
        public static implicit operator Port<ReturnCliffDetail>(GetSensorsResponse portSet)
        {
            if (portSet == null) return null;
            return (Port<ReturnCliffDetail>)portSet[typeof(ReturnCliffDetail)];
        }
        /// <summary>
        /// Implicit operator for ReturnTelemetry
        /// </summary>
        /// <param name="portSet"></param>
        /// <returns></returns>
        public static implicit operator Port<ReturnTelemetry>(GetSensorsResponse portSet)
        {
            if (portSet == null) return null;
            return (Port<ReturnTelemetry>)portSet[typeof(ReturnTelemetry)];
        }
        /// <summary>
        /// Implicit operator for Fault
        /// </summary>
        /// <param name="portSet"></param>
        /// <returns></returns>
        public static implicit operator Port<Fault>(GetSensorsResponse portSet)
        {
            if (portSet == null) return null;
            return (Port<Fault>)portSet[typeof(Fault)];
        }
        #endregion
    }

    /// <summary>
    /// Internal Roomba Command Response Port
    /// </summary>
    public class RoombaResponsePort : PortSet
    {
        /// <summary>
        /// Roomba Response Port
        /// </summary>
        public RoombaResponsePort()
            : base(
            typeof(RoombaCommandReceived),
            typeof(Fault),
            typeof(ReturnAll),
            typeof(ReturnSensors),
            typeof(ReturnPose),
            typeof(ReturnPower),
            typeof(ReturnCliffDetail),
            typeof(ReturnTelemetry),
            typeof(ReturnQueryList),
            typeof(ReturnFirmwareDate))
        { }

        #region Implicit Operators

        /// <summary>
        /// Implicit Operator for Port of RoombaCommandReceived
        /// </summary>
        /// <param name="portSet"></param>
        /// <returns></returns>
        public static implicit operator Port<RoombaCommandReceived>(RoombaResponsePort portSet)
        {
            if (portSet == null) return null;
            return (Port<RoombaCommandReceived>)portSet[typeof(RoombaCommandReceived)];
        }

        /// <summary>
        /// Implicit Operator for Port of Fault
        /// </summary>
        /// <param name="portSet"></param>
        /// <returns></returns>
        public static implicit operator Port<Fault>(RoombaResponsePort portSet)
        {
            if (portSet == null) return null;
            return (Port<Fault>)portSet[typeof(Fault)];
        }

        /// <summary>
        /// Implicit Operator for Port of ReturnAll
        /// </summary>
        /// <param name="portSet"></param>
        /// <returns></returns>
        public static implicit operator Port<ReturnAll>(RoombaResponsePort portSet)
        {
            if (portSet == null) return null;
            return (Port<ReturnAll>)portSet[typeof(ReturnAll)];
        }

        /// <summary>
        /// Implicit Operator for Port of ReturnSensors
        /// </summary>
        /// <param name="portSet"></param>
        /// <returns></returns>
        public static implicit operator Port<ReturnSensors>(RoombaResponsePort portSet)
        {
            if (portSet == null) return null;
            return (Port<ReturnSensors>)portSet[typeof(ReturnSensors)];
        }

        /// <summary>
        /// Implicit Operator for Port of ReturnPose
        /// </summary>
        /// <param name="portSet"></param>
        /// <returns></returns>
        public static implicit operator Port<ReturnPose>(RoombaResponsePort portSet)
        {
            if (portSet == null) return null;
            return (Port<ReturnPose>)portSet[typeof(ReturnPose)];
        }

        /// <summary>
        /// Implicit Operator for Port of ReturnPower
        /// </summary>
        /// <param name="portSet"></param>
        /// <returns></returns>
        public static implicit operator Port<ReturnPower>(RoombaResponsePort portSet)
        {
            if (portSet == null) return null;
            return (Port<ReturnPower>)portSet[typeof(ReturnPower)];
        }

        /// <summary>
        /// Implicit Operator for Port of ReturnCliffDetail
        /// </summary>
        /// <param name="portSet"></param>
        /// <returns></returns>
        public static implicit operator Port<ReturnCliffDetail>(RoombaResponsePort portSet)
        {
            if (portSet == null) return null;
            return (Port<ReturnCliffDetail>)portSet[typeof(ReturnCliffDetail)];
        }

        /// <summary>
        /// Implicit Operator for Port of ReturnTelemetry
        /// </summary>
        /// <param name="portSet"></param>
        /// <returns></returns>
        public static implicit operator Port<ReturnTelemetry>(RoombaResponsePort portSet)
        {
            if (portSet == null) return null;
            return (Port<ReturnTelemetry>)portSet[typeof(ReturnTelemetry)];
        }

        /// <summary>
        /// Implicit Operator for Port of ReturnQueryList
        /// </summary>
        /// <param name="portSet"></param>
        /// <returns></returns>
        public static implicit operator Port<ReturnQueryList>(RoombaResponsePort portSet)
        {
            if (portSet == null) return null;
            return (Port<ReturnQueryList>)portSet[typeof(ReturnQueryList)];
        }

        /// <summary>
        /// Implicit Operator for Port of ReturnFirmwareDate
        /// </summary>
        /// <param name="portSet"></param>
        /// <returns></returns>
        public static implicit operator Port<ReturnFirmwareDate>(RoombaResponsePort portSet)
        {
            if (portSet == null) return null;
            return (Port<ReturnFirmwareDate>)portSet[typeof(ReturnFirmwareDate)];
        }

        #endregion
    }

    /// <summary>
    /// Internal serial receive port
    /// </summary>
    public class iRobotReceivePort : PortSet<RoombaReturnPacket, string> { }

    /// <summary>
    /// Initiates a connection between the service host/PC and the iRobot with full configuration.
    /// </summary>
    [Description("Initiates a connection between the service host/PC and the robot and configures it.")]
    public class Configure : Replace<RoombaState, PortSet<DefaultReplaceResponseType, Fault>>
    {
        /// <summary>
        /// Initiates a connection between the service host/PC and the iRobot with full configuration.
        /// </summary>
        public Configure() { this.Body = new RoombaState(); }

        /// <summary>
        /// Initiates a connection between the service host/PC and the iRobot with full configuration.
        /// </summary>
        /// <param name="state"></param>
        public Configure(RoombaState state)
        {
            this.Body = state;
        }
    }

    /// <summary>
    /// iRobot Connect Operation
    /// </summary>
    [Description ("Initiates a connection between the service host/PC and the robot.")]
    public class Connect : Update<IRobotConnect, PortSet<DefaultUpdateResponseType, Fault>>
    {
        /// <summary>
        /// iRobot Connect Operation
        /// </summary>
        public Connect()
        {
            if (this.Body == null)
                this.Body = new IRobotConnect();
        }

        /// <summary>
        /// iRobot Connect Operation
        /// </summary>
        /// <param name="body"></param>
        public Connect(IRobotConnect body) : base(body)
        {
        }

    }

    /// <summary>
    /// Get Operation
    /// </summary>
    [Description("Gets the complete state of the robot.")]
    public class Get : Get<GetRequestType, PortSet<RoombaState, Fault>> { }



    #region Roomba Direct Commands

    /// <summary>
    /// Query for Roomba/Create Firmware Date
    /// </summary>
    [DisplayName("(User) GetFirmwareDate")]
    [Description("Gets the robot's firmware date.")]
    public class RoombaGetFirmwareDate : Query<CmdFirmwareDate, PortSet<ReturnFirmwareDate, Fault>> { }

    /// <summary>
    /// Place the iRobot in the specified mode.
    /// </summary>
    [DisplayName("(User) SetMode")]
    [Description("Sets the robot's mode.")]
    public class RoombaSetMode : Update<CmdSetMode, StandardResponse>
    {
        /// <summary>
        /// Place the iRobot in the specified mode.
        /// </summary>
        public RoombaSetMode() { }

        /// <summary>
        /// Place the iRobot in the specified mode.
        /// </summary>
        /// <param name="body"></param>
        public RoombaSetMode(CmdSetMode body): base(body) { }

        /// <summary>
        /// Place the iRobot in the specified mode.
        /// </summary>
        /// <param name="mode"></param>
        /// <param name="maintainMode"></param>
        public RoombaSetMode(RoombaMode mode, bool maintainMode)
        {
            this.Body = new CmdSetMode(mode, maintainMode);
        }
    }


    /// <summary>
    /// Starts a spot cleaning cycle, the same as a normal "spot" button press.
    /// <remarks>This command puts the SCI in passive mode.</remarks>
    /// </summary>
    [Description("Sets the spot cleaning behavior.")]
    public class RoombaStartSpotCleaning : Update<CmdSpot, StandardResponse>
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public RoombaStartSpotCleaning()
        {
            this.Body = new CmdSpot();
        }
    }

    /// <summary>
    /// Starts a normal cleaning cycle, the same as a normal "clean" button press.
    /// <remarks>This command puts the SCI in passive mode.</remarks>
    /// </summary>
    [Description("Sets normal cleaning behavior.")]
    public class RoombaStartCleaning : Update<CmdClean, StandardResponse>
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public RoombaStartCleaning()
        {
            this.Body = new CmdClean();
        }
    }

    /// <summary>
    /// Starts a maximum time cleaning cycle, the same as a normal "max" button press.
    /// <remarks>This command puts the SCI in passive mode.</remarks>
    /// </summary>
    [Description("Sets maximum time cleaning cycle.")]
    public class RoombaStartMaxCleaning : Update<CmdMax, StandardResponse>
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public RoombaStartMaxCleaning()
        {
            this.Body = new CmdMax();
        }
    }

    /// <summary>
    /// Starts a Create demo.
    /// <remarks></remarks>
    /// </summary>
    [DisplayName("(User) Start Demo")]
    [Description("Begins a demo.\n(Supported only on Create.")]
    public class CreateStartDemo : Update<CmdDemo, StandardResponse>
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public CreateStartDemo(DemoMode demoMode)
        {
            this.Body = new CmdDemo(demoMode);
        }
    }

    /// <summary>
    /// Controls Roomba's drive wheels.
    /// <remarks>The Roomba is controlled by providing the average velocity
    /// of the drive wheels in millimeters per second (mm/s), and the radius, in
    /// millimeters, at which Roomba should turn. The longer radii make
    /// Roomba drive straighter; shorter radii make it turn more. A Drive
    /// command with a positive velocity and a positive radius will make
    /// Roomba drive forward while turning toward the left. A negative
    /// radius will make it turn toward the right. Special cases for the
    /// radius make Roomba turn in place or drive straight, as specified
    /// below. </remarks>
    /// </summary>
    [DisplayName("(User) StartDriving")]
    [Description("Starts the robot's drive motors.")]
    public class RoombaDrive : Update<CmdDrive, StandardResponse>
    {
        /// <summary>
        /// Controls Roomba's drive wheels.
        /// <remarks>The Roomba is controlled by providing the average velocity
        /// of the drive wheels in millimeters per second (mm/s), and the radius, in
        /// millimeters, at which Roomba should turn. The longer radii make
        /// Roomba drive straighter; shorter radii make it turn more. A Drive
        /// command with a positive velocity and a positive radius will make
        /// Roomba drive forward while turning toward the left. A negative
        /// radius will make it turn toward the right. Special cases for the
        /// radius make Roomba turn in place or drive straight, as specified
        /// below. </remarks>
        /// </summary>
        public RoombaDrive()
        {
            this.Body = new CmdDrive();
        }

        /// <summary>
        /// Controls Roomba's drive wheels.
        /// <remarks>The Roomba is controlled by providing the average velocity
        /// of the drive wheels in millimeters per second (mm/s), and the radius, in
        /// millimeters, at which Roomba should turn. The longer radii make
        /// Roomba drive straighter; shorter radii make it turn more. A Drive
        /// command with a positive velocity and a positive radius will make
        /// Roomba drive forward while turning toward the left. A negative
        /// radius will make it turn toward the right. Special cases for the
        /// radius make Roomba turn in place or drive straight, as specified
        /// below. </remarks>
        /// </summary>
        public RoombaDrive(CmdDrive body): base(body) { }


        /// <summary>
        /// Initialization constructor
        /// </summary>
        /// <param name="velocity">mm/s (-500 to +500)</param>
        /// <param name="radius">Radius in mm (-2000 to +2000, 32768)
        /// 1 = counter clockwise when velocity greater than 0 and clockwise when velocity less than 0.
        /// -1 = counter clockwise when velocity less than 0 and clockwise when velocity greater than 0.
        /// 32768 = drive straight ahead</param>
        public RoombaDrive(int velocity, int radius)
        {
            this.Body = new CmdDrive(velocity, radius);
        }
    }

    /// <summary>
    /// Controls Roomba�s cleaning motors.
    /// <remarks>Multiple motors may be specified at once.
    /// This command does not change the mode.</remarks>
    /// </summary>
    [Description("Sets Roomba�s cleaning motors.")]
    public class RoombaSetCleaningMotors : Update<CmdMotors, StandardResponse>
    {
        /// <summary>
        /// Default Constructor
        /// </summary>
        public RoombaSetCleaningMotors()
        {
            this.Body = new CmdMotors();
        }

        /// <summary>
        /// Initialization Constructor
        /// </summary>
        /// <param name="motorFlags"></param>
        public RoombaSetCleaningMotors(RoombaMotorBits motorFlags)
        {
            this.Body = new CmdMotors(motorFlags);
        }
    }

    /// <summary>
    /// Controls Roomba�s LEDs.
    /// <remarks>Multiple LEDs may be specified at once.
    /// This command does not change the mode.</remarks>
    /// </summary>
    [DisplayName("(User) SetLEDs")]
    [Description("Sets the robot�s LEDs.")]
    public class RoombaSetLeds : Update<CmdLeds, StandardResponse>
    {
        /// <summary>
        /// Default Constructor
        /// </summary>
        public RoombaSetLeds()
        {
            this.Body = new CmdLeds();
        }


        /// <summary>
        /// Initialization Constructor
        /// </summary>
        /// <param name="ledBits"></param>
        /// <param name="powerColor">0-green, 255-red</param>
        /// <param name="powerIntensity">0-off, 255-Full</param>
        public RoombaSetLeds(RoombaLedBits ledBits, int powerColor, int powerIntensity)
        {
            this.Body = new CmdLeds(ledBits, powerColor, powerIntensity);
        }
    }

    /// <summary>
    /// Specifies a song to the SCI to be played later.
    /// </summary>
    [DisplayName("(User) DefineSong")]
    [Description("Defines a song to be played on the robot.")]
    public class RoombaDefineSong : Update<CmdDefineSong, StandardResponse>
    {
        /// <summary>
        /// Specifies a song to the SCI to be played later.
        /// </summary>
        public RoombaDefineSong()
        {
            this.Body = new CmdDefineSong();
        }

        /// <summary>
        /// Specifies a song to the SCI to be played later.
        /// </summary>
        public RoombaDefineSong(CmdDefineSong body) : base(body) { }

    }

    /// <summary>
    /// Plays one of 16 songs, as specified by an earlier RooombaDefineSong command.
    /// </summary>
    [DisplayName("(User) PlaySong")]
    [Description("Plays a pre-defined song.")]
    public class RoombaPlaySong : Update<CmdPlaySong, StandardResponse>
    {
        /// <summary>
        /// Plays one of 16 songs, as specified by an earlier RooombaDefineSong command.
        /// </summary>
        public RoombaPlaySong()
        {
            this.Body = new CmdPlaySong();
        }

        /// <summary>
        /// Plays one of 16 songs, as specified by an earlier RooombaDefineSong command.
        /// </summary>
        /// <param name="body"></param>
        public RoombaPlaySong(CmdPlaySong body): base(body) { }

    }

    /// <summary>
    /// Query for Roomba Pose, Power, and Sensors
    /// </summary>
    [DisplayName("(User) GetSensors")]
    [Description("Gets the current state of robot's sensors.")]
    public class RoombaGetSensors : Query<CmdSensors, GetSensorsResponse>
    {
        /// <summary>
        /// Default Constructor
        /// </summary>
        public RoombaGetSensors()
        {
            this.Body = new CmdSensors(RoombaQueryType.ReturnAll);
        }

        /// <summary>
        /// Get the specified sensors from Roomba
        /// </summary>
        /// <param name="queryType"></param>
        public RoombaGetSensors(RoombaQueryType queryType)
        {
            this.Body = new CmdSensors(queryType);
        }
    }

    /// <summary>
    /// Turns on force-seeking-dock mode.
    /// </summary>
    [DisplayName("(User) SeekDock")]
    [Description("Sets the robot's Force Seeking Dock behavior.")]
    public class RoombaSeekDock : Update<CmdForceSeekingDock, StandardResponse>
    {
        /// <summary>
        /// Default Constructor
        /// </summary>
        public RoombaSeekDock()
        {
            this.Body = new CmdForceSeekingDock();
        }

    }

    #endregion


    #region Service Contract
    /// <summary>
    /// Roomba Service Contract
    /// </summary>
    public static class Contract
    {
        /// <summary>
        /// Roomba Service Contract
        /// </summary>
        public const string Identifier = "http://schemas.microsoft.com/robotics/2007/01/irobot.user.html";

        /// <summary>
        /// The iRobot wheel base (mm)
        /// </summary>
        public const double iRobotWheelBase = 258;
    }

    #endregion
}
