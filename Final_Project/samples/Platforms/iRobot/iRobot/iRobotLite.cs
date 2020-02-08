//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: iRobotLite.cs $ $Revision: 43 $
//-----------------------------------------------------------------------

using Microsoft.Ccr.Core;
using Microsoft.Dss.Core;
using Microsoft.Dss.Core.Attributes;
using Microsoft.Dss.Core.DsspHttp;
using Microsoft.Dss.Core.DsspHttpUtilities;
using Microsoft.Dss.ServiceModel.Dssp;
using Microsoft.Dss.ServiceModel.DsspServiceBase;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Net;
using System.Xml;
using W3C.Soap;

using cons = Microsoft.Dss.Services.Constructor;
using create = Microsoft.Robotics.Services.IRobot.Create;
using irobot = Microsoft.Robotics.Services.IRobot.Roomba;
using irobotlite = Microsoft.Robotics.Services.IRobot.Lite;
using istream = Microsoft.Robotics.Services.IRobot.DssStream;
using sensorupdates = Microsoft.Robotics.Services.IRobot.SensorUpdates;
using stream = Microsoft.Robotics.Services.DssStream.Proxy;
using submgr = Microsoft.Dss.Services.SubscriptionManager;
using System.IO.Ports;
using System.Text;
using System.IO;

namespace Microsoft.Robotics.Services.IRobot.Lite
{

    /// <summary>
    /// iRobot Create Lite
    /// Service Implementation for lightweight processors
    /// </summary>
    /// <remarks>The iRobot Lite service calls to the serial port and may block a thread
    /// The ActivationSettings attribute with Sharing == false makes the runtime
    /// dedicate a dispatcher thread pool just for this service.</remarks>
    [ActivationSettings(ShareDispatcher = false, ExecutionUnitsPerDispatcher = 3)]
    [DisplayName("(User) iRobot� Create Lite")]
    [Description("Provides access to an iRobot Create service which contains a subset of commands.")]
    [Contract(Contract.Identifier)]
    public class IRobotLiteService : DsspServiceBase
    {
        /// <summary>
        /// iRobot State
        /// </summary>
        [InitialStatePartner(Optional = true, ServiceUri = ServicePaths.Store + "/iRobotLite.config.xml")]
        private irobot.RoombaState _state = new irobot.RoombaState();

        /// <summary>
        /// _main Port
        /// </summary>
        [ServicePort(_root, AllowMultipleInstances=true)]
        private IRobotLiteOperations _mainPort = new IRobotLiteOperations();

        #region Internal Ports

        /// <summary>
        /// Internal port for handling state updates
        /// </summary>
        private irobot.InternalMessages _internalPort = new irobot.InternalMessages();

        /// <summary>
        /// Serial Port Communications Port
        /// </summary>
        private SerialPortOperations _streamPort = new SerialPortOperations();

        #endregion

        #region Private State
#if NET_CF
        // **************************************************************
        // CE default timing
        // **************************************************************

        /// <summary>
        /// Default sensor polling rate (ms)
        /// </summary>
        private const int _defaultPollingInterval = 200;

        /// <summary>
        /// Default serial port time to wait after request (ms)
        /// </summary>
        private const int _initialWaitForCommand = 55;

        /// <summary>
        /// Default serial port polling when a response is expected (ms)
        /// </summary>
        private const int _dataWaitingInterval = 25;

        /// <summary>
        /// Minimum polling rate over wireless connection (ms)
        /// </summary>
        private const int _minWirelessInterval = 150;

        // **************************************************************
#else
        // **************************************************************
        // X86 timing
        // **************************************************************

        /// <summary>
        /// Default sensor polling rate (ms)
        /// </summary>
        private const int _defaultPollingInterval = 200;

        /// <summary>
        /// Default serial port time to wait after request (ms)
        /// </summary>
        private const int _initialWaitForCommand = 40;

        /// <summary>
        /// Default serial port polling when a response is expected (ms)
        /// </summary>
        private const int _dataWaitingInterval = 15;

        /// <summary>
        /// Minimum polling rate over wireless connection (ms)
        /// </summary>
        private const int _minWirelessInterval = 50;

        // **************************************************************
#endif
        private PrivateState _internalState = new PrivateState();

        private int _tickInterval;
        private DateTime _nextTimer = DateTime.MaxValue;
        private DateTime _startedInvalidMode = DateTime.MaxValue;
        private DateTime _noResponseSince = DateTime.MaxValue;
        private readonly TimeSpan _waitingForResponse = TimeSpan.FromMilliseconds(_dataWaitingInterval);

        #region Subscription Helpers
        private Dictionary<string, Port<DsspOperation>> _subscribers = new Dictionary<string, Port<DsspOperation>>();
        #endregion

        #endregion

        #region Http Processing Helpers
        /// <summary>
        /// Http Post Helper Port
        /// </summary>
        DsspHttpUtilitiesPort _httpUtilities = new DsspHttpUtilitiesPort();

        [EmbeddedResource("Microsoft.Robotics.Services.IRobot.Roomba.Resources.iRobot.user.xslt")]
        private string _transform = null;

        private const string _root = "/irobotlite";
        private const string _showPerf = "/perf";

        #endregion


        /// <summary>
        /// Default Service Constructor
        /// </summary>
        public IRobotLiteService(DsspServiceCreationPort creationPort) :
                base(creationPort)
        {
        }

        /// <summary>
        /// Service Start
        /// </summary>
        protected override void Start()
        {
            // Needed for HttpPost
            _httpUtilities = DsspHttpUtilitiesService.Create(Environment);

            // Publish the service to the local Node Directory
            DirectoryInsert();

            bool connect = (_state != null && !_state.WaitForConnect);
            SpawnIterator<bool>(connect, ConfigureCreate);

            base.MainPortInterleave = new Interleave(
                new TeardownReceiverGroup(
                    Arbiter.Receive<DsspDefaultDrop>(false, _mainPort, DropHandler)
                    ),
                new ExclusiveReceiverGroup(
                    Arbiter.Receive<sensorupdates.Subscribe>(true, _mainPort, SubscribeHandler)
                    ),
                new ConcurrentReceiverGroup(

                // *************************************************************************************************
                // Dss Handlers

#if DEBUG
                    Arbiter.Receive<QueryPerf>(true, _mainPort, QueryPerfHandler),
#endif
                    Arbiter.Receive<DsspDefaultLookup>(true, _mainPort, base.DefaultLookupHandler),
                    Arbiter.Receive<irobot.Get>(true, _mainPort, GetHandler),
                    Arbiter.ReceiveWithIterator<HttpGet>(true, _mainPort, HttpGetHandler),
                    Arbiter.ReceiveWithIterator<HttpPost>(true, _mainPort, HttpPostHandler),
                    Arbiter.ReceiveWithIterator<irobot.Connect>(true, _mainPort, ConnectHandler),
                    Arbiter.Receive<irobot.Configure>(true, _mainPort, ConfigureHandler),

                // *************************************************************************************************
                // iRobot Commands

                    Arbiter.Receive<irobot.RoombaSetMode>(true, _mainPort, RoombaSetModeHandler),
                    Arbiter.Receive<irobot.RoombaSetLeds>(true, _mainPort, RoombaSetLedsHandler),
                    Arbiter.Receive<irobot.RoombaPlaySong>(true, _mainPort, RoombaPlaySongHandler),
                    Arbiter.Receive<irobot.RoombaGetSensors>(true, _mainPort, RoombaGetSensorsHandler),

                // *************************************************************************************************
                // One Time handlers (so we can throttle requests)

                    Arbiter.Receive<create.CreateDriveDirect>(false, _mainPort, CreateDriveDirectHandler),
                    Arbiter.Receive<irobot.RoombaDrive>(false, _mainPort, RoombaDriveHandler),

                // *************************************************************************************************
                // Ignore external updates to our notification types

                    Arbiter.Receive<sensorupdates.UpdateAll>(true, _mainPort, BlockedCommandHandler),
                    Arbiter.Receive<sensorupdates.UpdateBumpsCliffsAndWalls>(true, _mainPort, BlockedCommandHandler),
                    Arbiter.Receive<sensorupdates.UpdatePose>(true, _mainPort, BlockedCommandHandler),
                    Arbiter.Receive<sensorupdates.UpdatePower>(true, _mainPort, BlockedCommandHandler),
                    Arbiter.Receive<sensorupdates.UpdateMode>(true, _mainPort, BlockedCommandHandler),
                    Arbiter.Receive<sensorupdates.UpdateCliffDetail>(true, _mainPort, BlockedCommandHandler),
                    Arbiter.Receive<sensorupdates.UpdateTelemetry>(true, _mainPort, BlockedCommandHandler),
                    Arbiter.Receive<sensorupdates.UpdateNotifications>(true, _mainPort, BlockedCommandHandler)

                // *************************************************************************************************
                    ));
            Activate(base.MainPortInterleave);

            // Concurrent internal commands
            Activate(
                new Interleave(
                    new TeardownReceiverGroup(
                        Arbiter.Receive<Shutdown>(false, CleanupPort, delegate(Shutdown done) { CleanupPort.Post(done); })),
                    new ExclusiveReceiverGroup(),
                    new ConcurrentReceiverGroup(
                        Arbiter.ReceiveWithIterator<irobot.ChangeToMode>(true, _internalPort, ChangeToModeHandler)
                        )));


            // Interleave to manage updates to _internalState
            Activate(
                new Interleave(
                    new TeardownReceiverGroup(
                        Arbiter.Receive<Shutdown>(false, CleanupPort, delegate(Shutdown done) { CleanupPort.Post(done); })),
                    new ExclusiveReceiverGroup(
                        Arbiter.Receive<DataWaiting>(true, _streamPort, DataWaitingHandler),
                        Arbiter.Receive<stream.ReplaceStreamState>(true, _streamPort, ConfigureStreamHandler),
                        Arbiter.ReceiveWithIterator<irobot.ProcessAtomicCommand>(true, _streamPort, ProcessAtomicCommandHandler)
                    ),
                    new ConcurrentReceiverGroup()));


            // Set up a seperate interleave to manage state updates.
            // These commands are independent of all others, but exclusive to each other.
            // All updates to the service state must be done within these handlers.
            Activate(
                new Interleave(
                    new TeardownReceiverGroup(
                        Arbiter.Receive<Shutdown>(false, CleanupPort, delegate(Shutdown done) { CleanupPort.Post(done); })),
                    new ExclusiveReceiverGroup(
                        Arbiter.Receive<sensorupdates.UpdatePose>(true, _internalPort, UpdateStatePoseHandler),
                        Arbiter.Receive<sensorupdates.UpdatePower>(true, _internalPort, UpdateStatePowerHandler),
                        Arbiter.Receive<sensorupdates.UpdateBumpsCliffsAndWalls>(true, _internalPort, UpdateStateSensorsHandler),
                        Arbiter.Receive<sensorupdates.UpdateAll>(true, _internalPort, UpdateStateAllHandler),
                        Arbiter.Receive<sensorupdates.UpdateTelemetry>(true, _internalPort, UpdateTelemetryHandler),
                        Arbiter.Receive<sensorupdates.UpdateCliffDetail>(true, _internalPort, UpdateCliffDetailHandler),
                        Arbiter.Receive<DateTime>(true, _internalPort, SetLastUpdatedHandler),
                        //Arbiter.Receive<sensorupdates.UpdateNotifications>(true, _internalPort, UpdateStreamNotificationHandler),
                        Arbiter.Receive<irobot.Configure>(true, _internalPort, UpdateConfigureHandler),
                        Arbiter.Receive<irobot.Connect>(true, _internalPort, UpdateConnectHandler),
                        Arbiter.Receive<sensorupdates.UpdateMode>(true, _internalPort, UpdateModeHandler)
                    ),
                    new ConcurrentReceiverGroup()));


        }


        #region Internal State Update Handlers

        /// <summary>
        /// Update iRobot Configuration
        /// </summary>
        /// <param name="configure"></param>
        private void UpdateConfigureHandler(irobot.Configure configure)
        {
            _state = configure.Body;

            // Configure without connecting
            SpawnIterator<bool>(false, ConfigureCreate);

            configure.ResponsePort.Post(DefaultReplaceResponseType.Instance);
        }


        /// <summary>
        /// Update Connect Handler
        /// </summary>
        /// <param name="update"></param>
        private void UpdateConnectHandler(irobot.Connect update)
        {
            // Configure and connect
            SpawnIterator<bool>(true, ConfigureCreate);

            update.ResponsePort.Post(DefaultUpdateResponseType.Instance);
        }


        /// <summary>
        /// Update state with current pose.
        /// </summary>
        /// <param name="updatePose"></param>
        private void UpdateStatePoseHandler(sensorupdates.UpdatePose updatePose)
        {
            if (updatePose.Body == null || !updatePose.Body.ValidPacket)
            {
                updatePose.ResponsePort.Post(Fault.FromException(new ArgumentOutOfRangeException("Invalid Pose packet")));
                return;
            }

            _state.Pose = updatePose.Body;
            _state.LastUpdated = DateTime.Now;
            updatePose.ResponsePort.Post(DefaultUpdateResponseType.Instance);
        }

        /// <summary>
        /// Update state with current power.
        /// </summary>
        /// <param name="updatePower"></param>
        private void UpdateStatePowerHandler(sensorupdates.UpdatePower updatePower)
        {
            if (updatePower.Body == null || !updatePower.Body.ValidPacket)
            {
                updatePower.ResponsePort.Post(Fault.FromException(new ArgumentOutOfRangeException("Invalid Power packet")));
                return;
            }

            _state.Power = updatePower.Body;
            _state.LastUpdated = DateTime.Now;
            updatePower.ResponsePort.Post(DefaultUpdateResponseType.Instance);
        }

        /// <summary>
        /// Update state with current sensors.
        /// </summary>
        /// <param name="updateBumpsCliffsAndWalls"></param>
        private void UpdateStateSensorsHandler(sensorupdates.UpdateBumpsCliffsAndWalls updateBumpsCliffsAndWalls)
        {
            if (updateBumpsCliffsAndWalls.Body == null || !updateBumpsCliffsAndWalls.Body.ValidPacket)
            {
                updateBumpsCliffsAndWalls.ResponsePort.Post(Fault.FromException(new ArgumentOutOfRangeException("Invalid Sensors packet")));
                return;
            }

            _state.Sensors = updateBumpsCliffsAndWalls.Body;


            // If any Roomba wheels have dropped, change the Mode to passive.
            if ((_state.Mode == irobot.RoombaMode.Safe || _state.Mode == irobot.RoombaMode.Full)
                && (_state.IRobotModel == irobot.IRobotModel.Roomba)
                && ((_state.Sensors.BumpsWheeldrops & (irobot.BumpsWheeldrops.WheelDropCaster | irobot.BumpsWheeldrops.WheelDropLeft | irobot.BumpsWheeldrops.WheelDropRight)) != 0))
            {
                _state.Mode = irobot.RoombaMode.Passive;
            }

            _state.LastUpdated = DateTime.Now;
            updateBumpsCliffsAndWalls.ResponsePort.Post(DefaultUpdateResponseType.Instance);
        }

        /// <summary>
        /// Update state with current telemetry sensors.
        /// </summary>
        /// <param name="updateTelemetry"></param>
        private void UpdateTelemetryHandler(sensorupdates.UpdateTelemetry updateTelemetry)
        {
            if (updateTelemetry.Body == null || !updateTelemetry.Body.ValidPacket)
            {
                updateTelemetry.ResponsePort.Post(Fault.FromException(new ArgumentOutOfRangeException("Invalid Telemetry packet")));
                return;
            }

            _state.Telemetry = updateTelemetry.Body;
            _state.LastUpdated = DateTime.Now;

            if (_state.Telemetry.OIMode != irobot.RoombaMode.NotSpecified
                && _state.Mode != _state.Telemetry.OIMode)
            {
                LogInfo(LogGroups.Console, "Create changed to an unexpected mode (from " + _state.Mode.ToString() + " to " + _state.Telemetry.OIMode.ToString() + ")");
                _state.Mode = _state.Telemetry.OIMode;
            }

            updateTelemetry.ResponsePort.Post(DefaultUpdateResponseType.Instance);

        }

        /// <summary>
        /// Update state with current cliff sensors.
        /// </summary>
        /// <param name="updateCliffDetail"></param>
        private void UpdateCliffDetailHandler(sensorupdates.UpdateCliffDetail updateCliffDetail)
        {
            if (updateCliffDetail.Body == null || !updateCliffDetail.Body.ValidPacket)
            {
                updateCliffDetail.ResponsePort.Post(Fault.FromException(new ArgumentOutOfRangeException("Invalid CliffDetail packet")));
                return;
            }
            _state.CliffDetail = updateCliffDetail.Body;
            _state.LastUpdated = DateTime.Now;
            updateCliffDetail.ResponsePort.Post(DefaultUpdateResponseType.Instance);
        }

        /// <summary>
        /// Update state with all current sensors.
        /// </summary>
        /// <param name="updateAll"></param>
        private void UpdateStateAllHandler(sensorupdates.UpdateAll updateAll)
        {
            if (updateAll.Body == null || !updateAll.Body.ValidPacket)
            {
                updateAll.ResponsePort.Post(Fault.FromException(new ArgumentOutOfRangeException("Invalid ReturnAll Sensors packet")));
                return;
            }

            bool done = false;
            bool updated = false;

            try
            {
                if (updateAll.Body.Pose != null && updateAll.Body.Pose.ValidPacket)
                {
                    _state.Pose = updateAll.Body.Pose;
                    updated = true;
                }

                if (updateAll.Body.Power != null && updateAll.Body.Power.ValidPacket)
                {
                    _state.Power = updateAll.Body.Power;
                    updated = true;
                }

                if (updateAll.Body.Sensors != null && updateAll.Body.Sensors.ValidPacket)
                {
                    _state.Sensors = updateAll.Body.Sensors;
                    updated = true;
                }

                if (_state.IRobotModel == irobot.IRobotModel.Create)
                {
                    if (updateAll.Body.CliffDetail != null && updateAll.Body.CliffDetail.ValidPacket)
                    {
                        updated = true;
                        _state.CliffDetail = updateAll.Body.CliffDetail;
                    }
                    if (updateAll.Body.Telemetry != null && updateAll.Body.Telemetry.ValidPacket)
                    {
                        updated = true;
                        _state.Telemetry = updateAll.Body.Telemetry;
                        if (_state.Telemetry.OIMode != irobot.RoombaMode.NotSpecified
                            && _state.Mode != _state.Telemetry.OIMode)
                        {
                            LogInfo(LogGroups.Console, "Create changed to an unexpected mode (from " + _state.Mode.ToString() + " to " + _state.Telemetry.OIMode.ToString() + ")");
                            _state.Mode = _state.Telemetry.OIMode;
                        }
                    }
                }
                // If any Roomba wheels have dropped, change the Mode to passive.
                else if (_state.Mode == irobot.RoombaMode.Safe
                    && (_state.IRobotModel == irobot.IRobotModel.Roomba)
                    && ((_state.Sensors.BumpsWheeldrops & (irobot.BumpsWheeldrops.WheelDropCaster | irobot.BumpsWheeldrops.WheelDropLeft | irobot.BumpsWheeldrops.WheelDropRight)) != 0))
                {
                    _state.Mode = irobot.RoombaMode.Passive;
                }

                if (updated)
                {
                    _state.LastUpdated = DateTime.Now;
                    done = true;
                }
            }
            finally
            {
                if (done)
                    updateAll.ResponsePort.Post(DefaultUpdateResponseType.Instance);
                else
                    updateAll.ResponsePort.Post(Fault.FromException(new InvalidOperationException("Failure to update all sensors")));
            }

        }

        /// <summary>
        /// Update the _state.LastUpdated
        /// </summary>
        /// <param name="lastUpdated"></param>
        private void SetLastUpdatedHandler(DateTime lastUpdated)
        {
            _state.LastUpdated = lastUpdated;
        }

        /// <summary>
        /// Update state with the current Roomba Mode
        /// </summary>
        /// <param name="updateMode"></param>
        private void UpdateModeHandler(sensorupdates.UpdateMode updateMode)
        {
            bool sendNotification = false;

            if (updateMode.Body.IRobotModel != irobot.IRobotModel.NotSpecified
                && _state.IRobotModel != updateMode.Body.IRobotModel)
            {
                _state.LastUpdated = DateTime.Now;
                _state.IRobotModel = updateMode.Body.IRobotModel;
                if (_state.IRobotModel == irobot.IRobotModel.Create)
                {
                    create.IRobotUtility.ValidateState(ref _state, false);
                }
            }

            if (updateMode.Body.FirmwareDate != DateTime.MinValue)
            {
                _state.LastUpdated = DateTime.Now;
                _state.FirmwareDate = updateMode.Body.FirmwareDate;
            }

            if (updateMode.Body.MaintainMode != irobot.RoombaMode.Off)
            {
                _state.MaintainMode = updateMode.Body.MaintainMode;
            }

            if (updateMode.Body.RoombaMode != irobot.RoombaMode.NotSpecified)
            {
                // If the mode changed, send a notification
                if (_state.Mode != updateMode.Body.RoombaMode)
                {
                    _state.LastUpdated = DateTime.Now;
                    sendNotification = true;
                }
                _state.Mode = updateMode.Body.RoombaMode;
                if (_state.Mode == irobot.RoombaMode.Uninitialized)
                    _state.IsInitialized = false;
            }

            if (sendNotification)
            {
                // We modify updateMode with the current state,
                // so make sure this is at the end of the handler.
                updateMode.Body.IRobotModel = _state.IRobotModel;
                updateMode.Body.MaintainMode = _state.MaintainMode;
                updateMode.Body.RoombaMode = _state.Mode;

                SendNotification<sensorupdates.UpdateMode>(updateMode);
            }

            updateMode.ResponsePort.Post(DefaultUpdateResponseType.Instance);
        }

        #endregion

        #region Operation Handlers

        /// <summary>
        /// Get Handler
        /// </summary>
        /// <param name="get"></param>
        /// <returns></returns>
        private void GetHandler(irobot.Get get)
        {
            get.ResponsePort.Post(_state);
        }

#if DEBUG
        /// <summary>
        /// Query Performance
        /// </summary>
        /// <param name="query"></param>
        private void QueryPerfHandler(QueryPerf query)
        {
            query.ResponsePort.Post(_internalState.iRobotPerf);
        }
#endif

        /// <summary>
        /// HttpGet Handler
        /// </summary>
        /// <param name="httpGet"></param>
        /// <returns></returns>
        private IEnumerator<ITask> HttpGetHandler(HttpGet httpGet)
        {
            HttpListenerRequest request = httpGet.Body.Context.Request;
            HttpListenerResponse response = httpGet.Body.Context.Response;

            string path = request.Url.AbsolutePath;
            HttpResponseType rsp;

#if DEBUG
            if (path.StartsWith(_root, StringComparison.InvariantCultureIgnoreCase))
            {
                if (path.EndsWith(_showPerf, StringComparison.InvariantCultureIgnoreCase))
                {

                    using (MemoryStream stream = new MemoryStream())
                    {
                        System.Xml.Serialization.XmlSerializer xmlSerializer = new System.Xml.Serialization.XmlSerializer(typeof(iRobotPerf));
                        xmlSerializer.Serialize(stream, _internalState.iRobotPerf);
                        stream.Position = 0;
                        response.AddHeader("Cache-Control", "No-cache");

                        WriteResponseFromStream write = new WriteResponseFromStream(
                            httpGet.Body.Context, stream, "text/xml"
                        );

                        _httpUtilities.Post(write);

                        yield return Arbiter.Choice(
                            write.ResultPort,
                            delegate(Stream res)
                            {
                                stream.Close();
                            },
                            delegate(Exception e)
                            {
                                stream.Close();
                                LogError(e);
                            }
                        );
                    }
                    yield break;
                }
            }

#endif
            rsp = new HttpResponseType(HttpStatusCode.OK, _state, _transform);
            httpGet.ResponsePort.Post(rsp);
            yield break;
        }

        /// <summary>
        /// HttpPost Handler
        /// </summary>
        /// <param name="httpPost"></param>
        /// <returns></returns>
        private IEnumerator<ITask> HttpPostHandler(HttpPost httpPost)
        {
            // Use helper to read form data
            ReadFormData readForm = new ReadFormData(httpPost);
            _httpUtilities.Post(readForm);

            irobot.RoombaState config = new irobot.RoombaState();

            // Wait for result
            yield return Arbiter.Choice(readForm.ResultPort,
                delegate(NameValueCollection parameters)
                {
                    if (!string.IsNullOrEmpty(parameters["Action"])
                        && (parameters["Action"] == "iRobotConfig")
                        && (parameters["buttonOk"] == "Connect"))
                    {
                        config.SerialPort = _state.SerialPort;
                        try
                        {
                            int port = int.Parse(parameters["SerialPort"]);
                            if (port >= 0)
                                config.SerialPort = port;
                        }
                        catch (System.FormatException) { }
                        catch (System.ArgumentNullException) { }

                        config.BaudRate = _state.BaudRate;
                        try
                        {
                            int baudRate = int.Parse(parameters["BaudRate"]);
                            if (baudRate >= 0)
                                config.BaudRate = baudRate;
                        }
                        catch (System.FormatException) { }
                        catch (System.ArgumentNullException) { }

                        try
                        {
                            string connectionTypeParm = parameters["ConnectionType"] ?? "NotConfigured";
                            config.ConnectionType = (irobot.iRobotConnectionType)Enum.Parse(typeof(irobot.iRobotConnectionType), connectionTypeParm, true);
                        }
                        catch
                        {
                            config.ConnectionType = _state.ConnectionType;
                        }

                        try
                        {
                            config.IRobotModel = (irobot.IRobotModel)Enum.Parse(typeof(irobot.IRobotModel), parameters["IRobotModel"].ToString(), true);
                        }
                        catch
                        {
                            config.IRobotModel = _state.IRobotModel;
                        }

                        try
                        {
                            config.WaitForConnect = (parameters["WaitForConnect"] == "on");
                        }
                        catch
                        {
                            config.WaitForConnect = _state.WaitForConnect;
                        }

                        try
                        {
                            config.Name = parameters["Name"];
                        }
                        catch
                        {
                            config.Name = _state.Name;
                        }

                        try
                        {
                            config.MaintainMode = (irobot.RoombaMode)Enum.Parse(typeof(irobot.RoombaMode), parameters["MaintainMode"].ToString().Replace(" ", string.Empty), true);
                        }
                        catch
                        {
                            config.MaintainMode = _state.MaintainMode;
                        }

                        try
                        {
                            config.PollingInterval = int.Parse(parameters["PollingInterval"]);
                        }
                        catch
                        {
                            config.PollingInterval = _state.PollingInterval;
                        }

                        // Gather remaining settings from existing state.
                        config.CreateNotifications = _state.CreateNotifications;
                        config.RobotImage = _state.RobotImage;
                        config.SongDefinitions = _state.SongDefinitions;
                    }
                    else
                    {
                        config = null;
                    }
                },
                delegate(Exception Failure)
                {
                    config = null;
                    LogError(Failure.Message);
                });

            if (config == null)
            {
                HttpPostFailure(httpPost, Fault.FromCodeSubcode(FaultCodes.Sender, DsspFaultCodes.ActionNotSupported));
                yield break;
            }

            bool configured = false;

            irobot.Configure configure = new irobot.Configure(config);
            _mainPort.Post(configure);
            yield return Arbiter.Choice(
                Arbiter.Receive<DefaultReplaceResponseType>(false, configure.ResponsePort,
                    delegate(DefaultReplaceResponseType response)
                    {
                        configured = true;
                        HttpPostSuccess(httpPost, _state, _transform);
                        SaveState(_state);
                    }),
                Arbiter.Receive<Fault>(false, configure.ResponsePort,
                    delegate(Fault f)
                    {
                        HttpPostFailure(httpPost, f);
                    })
            );

            if (configured)
            {
                yield return Arbiter.Choice(
                    _mainPort.Connect(),
                        delegate(DefaultUpdateResponseType response)
                        {
                            HttpPostSuccess(httpPost, _state, _transform);
                            SaveState(_state);
                        },
                        delegate(Fault f)
                        {
                            HttpPostFailure(httpPost, f);
                        });
            }

            yield break;
        }


        /// <summary>
        /// Subscribe Handler
        /// </summary>
        /// <param name="subscribe"></param>
        /// <returns></returns>
        private void SubscribeHandler(sensorupdates.Subscribe subscribe)
        {
            string key = subscribe.Body.Subscriber.ToLower();
            Port<DsspOperation> notificationPort;
            if (_subscribers.ContainsKey(key))
            {
                notificationPort = _subscribers[key];
            }
            else
            {
                // notificationPort = subscribe.NotificationPort;
                notificationPort = ServiceForwarderUnknownType(new Uri(subscribe.Body.Subscriber));
                _subscribers.Add(key, notificationPort);
            }

            SendNotification<sensorupdates.UpdateMode>(subscribe.Body.Subscriber, new sensorupdates.UpdateMode(_state));
            subscribe.ResponsePort.Post(new SubscribeResponseType());
        }


        /// <summary>
        /// Connect Handler
        /// </summary>
        /// <param name="connect"></param>
        /// <returns></returns>
        private IEnumerator<ITask> ConnectHandler(irobot.Connect connect)
        {
            if (_state.ConnectionType == irobot.iRobotConnectionType.NotConfigured)
            {
                connect.ResponsePort.Post(Fault.FromException(new InvalidOperationException("iRobot Connection Type is not configured")));
                yield break;
            }

            if (_state.IRobotModel != irobot.IRobotModel.Create)
            {
                connect.ResponsePort.Post(Fault.FromException(new InvalidOperationException("iRobot Model must be configure for the iRobot Create")));
                yield break;
            }

            if (_state.SerialPort <= 0)
            {
                connect.ResponsePort.Post(Fault.FromException(new InvalidOperationException("iRobot Serial Port must be configured")));
                yield break;
            }

            // Configure and connect, wait until we're done
            yield return Arbiter.ExecuteToCompletion(this.Environment.TaskQueue,
                new IterativeTask<bool>(true, ConfigureCreate));

            if (_state.Mode != irobot.RoombaMode.Uninitialized)
                connect.ResponsePort.Post(DefaultUpdateResponseType.Instance);
            else
                connect.ResponsePort.Post(Fault.FromException(new InvalidOperationException("Unable to connect to the iRobot")));
            yield break;
        }

        /// <summary>
        /// Configure Handler
        /// </summary>
        /// <param name="configure"></param>
        /// <returns></returns>
        private void ConfigureHandler(irobot.Configure configure)
        {
            _state = configure.Body;

            // Configure without connecting
            SpawnIterator<bool>(false, ConfigureCreate);

            configure.ResponsePort.Post(DefaultReplaceResponseType.Instance);
        }

        /// <summary>
        /// RoombaSetMode Handler
        /// </summary>
        /// <param name="createSetMode"></param>
        /// <returns></returns>
        private void RoombaSetModeHandler(irobot.RoombaSetMode createSetMode)
        {
            irobot.RoombaCommand cmd = GetModeCommand(createSetMode);
            SendCommand(cmd, createSetMode.ResponsePort);
        }

        /// <summary>
        /// RoombaSetLeds Handler
        /// </summary>
        /// <param name="setLeds"></param>
        /// <returns></returns>
        private void RoombaSetLedsHandler(irobot.RoombaSetLeds setLeds)
        {
            SendCommand(setLeds.Body, setLeds.ResponsePort);
        }

        /// <summary>
        /// RoombaPlaySong Handler
        /// </summary>
        /// <param name="playSong"></param>
        /// <returns></returns>
        private void RoombaPlaySongHandler(irobot.RoombaPlaySong playSong)
        {
            SendCommand(playSong.Body, playSong.ResponsePort);
        }

        /// <summary>
        /// RoombaGetSensors Handler
        /// </summary>
        /// <param name="getSensors"></param>
        /// <returns></returns>
        private void RoombaGetSensorsHandler(irobot.RoombaGetSensors getSensors)
        {
            SendCommand(getSensors.Body, getSensors.ResponsePort);
        }


        /// <summary>
        /// RoombaDrive Handler
        /// </summary>
        /// <remarks></remarks>
        /// <param name="drive"></param>
        /// <returns></returns>
        private void RoombaDriveHandler(irobot.RoombaDrive drive)
        {
            try
            {
                irobot.RoombaDrive pendingCommand = _mainPort.Test<irobot.RoombaDrive>();
                while (pendingCommand != null)
                {
                    drive.ResponsePort.Post(irobot.RoombaCommandReceived.Instance(irobot.RoombaMode.NotSpecified));
                    drive = pendingCommand;
                    pendingCommand = _mainPort.Test<irobot.RoombaDrive>();
                }

                SendCommand(drive.Body, drive.ResponsePort);
            }
            finally
            {
                Activate(Arbiter.Receive<irobot.RoombaDrive>(false, _mainPort, RoombaDriveHandler));
            }
        }

        /// <summary>
        /// CreateDriveDirect Handler
        /// </summary>
        /// <param name="driveDirect"></param>
        /// <returns></returns>
        private void CreateDriveDirectHandler(create.CreateDriveDirect driveDirect)
        {
            try
            {
                create.CreateDriveDirect pendingCommand = _mainPort.Test<create.CreateDriveDirect>();
                while (pendingCommand != null)
                {
                    driveDirect.ResponsePort.Post(irobot.RoombaCommandReceived.Instance(irobot.RoombaMode.NotSpecified));
                    driveDirect = pendingCommand;
                    pendingCommand = _mainPort.Test<create.CreateDriveDirect>();
                }

                SendCommand(driveDirect.Body, driveDirect.ResponsePort);
            }
            finally
            {
                Activate(Arbiter.Receive<create.CreateDriveDirect>(false, _mainPort, CreateDriveDirectHandler));
            }
        }

        #region outbound notifications

        /// <summary>
        /// Invalid Inbound Command Handler
        /// </summary>
        /// <param name="header"></param>
        /// <returns></returns>
        private void BlockedCommandHandler(object header)
        {
            throw new InvalidOperationException("Outbound sensor notifications are not valid for sending requests.");
        }

        #endregion

        /// <summary>
        /// Custom Drop Handler
        /// </summary>
        /// <param name="drop"></param>
        /// <returns></returns>
        private void DropHandler(DsspDefaultDrop drop)
        {
            CloseConnection(true);
            base.DefaultDropHandler(drop);
        }

        #endregion

        #region Communications

        /// <summary>
        /// Change to the specified Mode
        /// </summary>
        /// <param name="changeMode"></param>
        /// <returns></returns>
        private IEnumerator<ITask> ChangeToModeHandler(irobot.ChangeToMode changeMode)
        {
            bool success = false;

            irobot.RoombaCommand setModeCommand = create.IRobotUtility.SetModeCommand(changeMode.Body.RoombaMode, _state);
            while (setModeCommand != null)
            {
                LogInfo(LogGroups.Console, "Changing Roomba mode to: " + changeMode.Body.RoombaMode.ToString());

                success = false;
                yield return Arbiter.Choice<irobot.RoombaReturnPacket, Fault>(
                    _streamPort.ProcessAtomicCommand(setModeCommand),
                    delegate(irobot.RoombaReturnPacket responsePacket)
                    {
                        success = true;
                        LogVerbose(LogGroups.Console, "Roomba mode is: " + changeMode.Body.RoombaMode.ToString());
                    },
                    delegate(Fault fault)
                    {
                        success = false;
                        LogError("Error Changing Roomba mode to " + changeMode.Body.RoombaMode.ToString());
                        changeMode.ResponsePort.PostUnknownType(fault);

                    });

                if (!success)
                {
                    yield break;
                }

                setModeCommand = create.IRobotUtility.SetModeCommand(changeMode.Body.RoombaMode, _state);
            }

            changeMode.ResponsePort.Post(DefaultSubmitResponseType.Instance);
            yield break;
        }


        /// <summary>
        /// Configure the iRobot communications stream
        /// </summary>
        /// <param name="replace"></param>
        /// <returns></returns>
        private void ConfigureStreamHandler(stream.ReplaceStreamState replace)
        {
            _internalState.StreamState = replace.Body;
            bool connected = Connect();
            stream.ReplaceStreamResponse response = new stream.ReplaceStreamResponse(connected);
            replace.ResponsePort.Post(response);
        }


        /// <summary>
        /// Connect to the underlying communications port.
        /// </summary>
        /// <returns></returns>
        private bool Connect()
        {
            create.IRobotUtility.ParseConfiguration(_internalState.StreamState, ref _internalState.IRobotConnection);

            bool success = false;

            // Close connection without disposing
            CloseConnection(false);

            if (!string.IsNullOrEmpty(_internalState.IRobotConnection.PortName))
            {
                if (_internalState.SerialPort == null)
                {
                    _internalState.SerialPort = new SerialPort(_internalState.IRobotConnection.PortName, _internalState.IRobotConnection.BaudRate);
                    _internalState.SerialPort.ReceivedBytesThreshold = 1;
                }
                else
                {
                    _internalState.SerialPort.PortName = _internalState.IRobotConnection.PortName;
                    _internalState.SerialPort.BaudRate = _internalState.IRobotConnection.BaudRate;
                }

                _internalState.SerialPort.WriteTimeout = 1000;
                _internalState.SerialPort.ReadTimeout = 200;
                _internalState.SerialPort.Encoding = _internalState.IRobotConnection.Encoding;
                _internalState.SerialPort.Parity = _internalState.IRobotConnection.Parity;
                _internalState.SerialPort.DataBits = _internalState.IRobotConnection.DataBits;
                _internalState.SerialPort.StopBits = _internalState.IRobotConnection.StopBits;

                try
                {
                    _internalState.SerialPort.Open();
                    success = _internalState.SerialPort.IsOpen;
                }
                catch (Exception ex)
                {
                    LogError(LogGroups.Console, ex);
                }
            }

            _internalState.Initialized = success;
            return _internalState.Initialized;
        }

        /// <summary>
        /// Close the serial port connections
        /// </summary>
        /// <param name="dispose"></param>
        private void CloseConnection(bool dispose)
        {
            // Clear any pending data
            if (_internalState.PendingCommands != null)
                _internalState.PendingCommands.Clear();
            _internalState.Start = _internalState.End = 0;

            // Close any open serial port
            if (_internalState.SerialPort != null)
            {
                if (_internalState.SerialPort.IsOpen)
                {
                    try
                    {
                        _internalState.SerialPort.Close();
                    }
                    catch (Exception ex)
                    {
                        LogError("Error closing iRobot connection: " + ex.Message);
                    }
                }

                try
                {
                    if (dispose)
                        _internalState.SerialPort.Dispose();
                }
                catch (Exception ex)
                {
                    LogError("Error shutting down iRobot connection: " + ex.Message);
                }

                if (dispose)
                    _internalState.SerialPort = null;
            }

        }


        /// <summary>
        /// Send iRobot Command to the serial port.
        /// </summary>
        /// <remarks>Exclusive to handlers which reference _internalState.</remarks>
        /// <param name="atomicCommand"></param>
        /// <returns></returns>
        private IEnumerator<ITask> ProcessAtomicCommandHandler(irobot.ProcessAtomicCommand atomicCommand)
        {
            bool waitForResponse = (atomicCommand.Body.ExpectedResponseBytes() > 0);

            if (_internalState.SerialPort == null || !_internalState.SerialPort.IsOpen)
            {
                atomicCommand.ResponsePort.Post(Fault.FromException(new InvalidOperationException("Serial Port is not open")));
                yield break;
            }

            // See if this command causes the mode to change.
            irobot.RoombaMode newMode;
            create.IRobotUtility.GetChangedMode(atomicCommand.Body.RoombaCommandCode, _state.Mode, _state.IRobotModel, out newMode);

            if (waitForResponse)
            {
                atomicCommand.Body.CmdTimeoutMs = 0;
                atomicCommand.Body.CmdStarted = DateTime.Now;
                atomicCommand.Body.CmdExpiration = CalcCmdExpiration(atomicCommand.Body.CmdStarted, atomicCommand.Body.CmdTimeoutMs);

                // We will handle the response later.
                _internalState.PendingCommands.Add(atomicCommand);
            }


            byte[] data = atomicCommand.Body.GetPacket();
            _internalState.SerialPort.Write(data, 0, data.Length);

            if (!waitForResponse)
            {
                // handle the response now
                irobot.RoombaReturnPacket response = new irobot.RoombaCommandReceived(atomicCommand.Body.RoombaCommandCode);
                atomicCommand.ResponsePort.Post(response);
            }

            if (newMode != _state.Mode)
            {
                yield return Arbiter.Choice(_internalPort.UpdateMode(newMode), DefaultUpdateResponseTypeHandler, DefaultFaultHandler);
            }

            // Initial wait time for the command to be processed on the iRobot.
            Activate(Arbiter.Receive(false, TimeoutPort(_initialWaitForCommand), PollForData));

            yield break;
        }

        /// <summary>
        /// Calculate the Command Expiration Time
        /// </summary>
        /// <param name="start"></param>
        /// <param name="ms"></param>
        /// <returns></returns>
        private DateTime CalcCmdExpiration(DateTime start, int ms)
        {
            if (ms <= 0)
                ms = 1000;
            return start.AddMilliseconds((double)ms);
        }

        /// <summary>
        /// Acknowledge a success
        /// </summary>
        /// <param name="ok"></param>
        private void DefaultUpdateResponseTypeHandler(DefaultUpdateResponseType ok)
        {
        }

        /// <summary>
        /// Default Error Handler
        /// </summary>
        /// <param name="fault"></param>
        private void DefaultFaultHandler(Fault fault)
        {
            LogError(fault);
        }

        private void PollForData(DateTime now)
        {
            _streamPort.DataWaiting();
        }

        /// <summary>
        /// Process waiting data on the serial port.
        /// </summary>
        /// <param name="dataWaiting"></param>
        /// <returns></returns>
        private void DataWaitingHandler(DataWaiting dataWaiting)
        {
            DateTime now = DateTime.Now;
#if DEBUG
            _internalState.iRobotPerf.PollingCount++;
            if (_internalState.iRobotPerf.DataWaitingCount > 0)
                _internalState.iRobotPerf.PollsPerRequest = (double)_internalState.iRobotPerf.PollingCount / (double)_internalState.iRobotPerf.DataWaitingCount;
#endif
            int totalBytesToProcess = _internalState.SerialPort.BytesToRead;

            if (totalBytesToProcess == 0)
            {
                #region Check for expired requests
                while (_internalState.PendingCommands.Count > 0)
                {
                    // Check for expired requests
                    if (_internalState.PendingCommands[0].Body.CmdExpiration <= now)
                    {
                        if (_noResponseSince == DateTime.MaxValue)
                            _noResponseSince = _internalState.PendingCommands[0].Body.CmdStarted;

                        _internalState.PendingCommands[0].ResponsePort.Post(
                            Fault.FromException(
                                new TimeoutException(string.Format("Timeout processing {0}", _internalState.PendingCommands[0].Body.RoombaCommandCode))));
                        _internalState.PendingCommands.RemoveAt(0);
                        continue;
                    }

                    break;
                }
                #endregion

                // If we still have outstanding commands, continue polling for data
                if (_internalState.PendingCommands.Count > 0)
                    Activate(Arbiter.Receive(false, TimeoutPort(_waitingForResponse), PollForData));

                return;
            }

            if (_noResponseSince != DateTime.MaxValue)
            {
                _noResponseSince = DateTime.MaxValue;
            }
#if DEBUG
            _internalState.iRobotPerf.DataWaitingCount++;
            _internalState.iRobotPerf.PollsPerRequest = (double)_internalState.iRobotPerf.PollingCount / (double)_internalState.iRobotPerf.DataWaitingCount;
#endif

            // Any time we have no prior data,
            // make sure to start at the front of the buffer.
            if (_internalState.End == _internalState.Start && _internalState.Start > 0)
            {
                _internalState.Start = _internalState.End = 0;
            }

            try
            {
                while (totalBytesToProcess > 0)
                {
                    if (totalBytesToProcess >= PrivateState.Maxbuffer)
                    {
                        // Dump the prior data
                        _internalState.Start = _internalState.End = 0;
                    }

                    // Calculate the number of bytes to read
                    int bytesToRead = Math.Min(_internalState.SerialPort.BytesToRead, Math.Min(PrivateState.Maxbuffer - _internalState.End, totalBytesToProcess));

                    if (bytesToRead < totalBytesToProcess && bytesToRead < 100)
                    {
                        // Truncate and reassemble the buffer.
                        if ((bytesToRead + _internalState.Start) < totalBytesToProcess)
                            _internalState.Start = totalBytesToProcess - bytesToRead;

                        System.Buffer.BlockCopy(_internalState.ReadBuffer, _internalState.Start, _internalState.ReadBuffer, 0, _internalState.End - _internalState.Start);

                        _internalState.End -= _internalState.Start;
                        _internalState.Start = 0;
                    }

                    _internalState.SerialPort.Read(_internalState.ReadBuffer, _internalState.End, bytesToRead);
                    _internalState.End += bytesToRead;

                    totalBytesToProcess -= bytesToRead;

                    ProcessUnrequestedText();
                }

                if (_internalState.PendingCommands.Count > 0)
                {
                    while (AnalyzeBuffer(now)) ;
                }

            }
            finally
            {
                if (_internalState.PendingCommands.Count > 0)
                {
                    Activate(Arbiter.Receive(false, TimeoutPort(_waitingForResponse), PollForData));
                }
            }
        }

        /// <summary>
        /// Process unrequested text and ignore it.
        /// </summary>
        private void ProcessUnrequestedText()
        {
            byte[] data = _internalState.ReadBuffer;
            int endIx = _internalState.End;


            bool isString = true;

            while (isString)
            {
                int ix = _internalState.Start;
                bool lineEnd = false;
                bool cr = false;
                int textcount = 0;
                while (ix < endIx)
                {
                    char c = (char)data[ix++];
                    if (isString && (c == '\r' || c == '\n'))
                    {
                        if (c == '\r')
                            cr = true;
                        else if (cr)
                            lineEnd = true;
                        else
                        {
                            isString = false;
                            break;
                        }
                    }
                    else if (lineEnd && (textcount > 0))
                    {
                        // we have been processing a string
                        // found one or more CR or LF
                        // but now one byte past the end of the string
                        ix--;
                        _internalState.Start = ix;
                        break;
                    }
                    else if ((c >= ' ') && c < 128)
                    {
                        textcount++;
                        // if we started with a line end, ignore it until we get the text.
                        if (lineEnd)
                            lineEnd = false;
                    }
                    else
                    {
                        isString = false;
                        break;
                    }
                }
            }

            return;
        }

        /// <summary>
        /// Analyze the buffer.
        /// When a command response is found,
        ///   the response is posted back to the caller,
        ///   the pending command is removed from the queue,
        ///   and the ReadBuffer is updated.
        /// </summary>
        /// <param name="dataReceivedTime"></param>
        /// <returns></returns>
        private bool AnalyzeBuffer(DateTime dataReceivedTime)
        {
            bool found = false;
            int ixCmdStart;
            int cmdCount = _internalState.PendingCommands.Count;
            for (int cmdIx = 0; cmdIx < cmdCount; cmdIx++)
            {
                irobot.ProcessAtomicCommand atomicCommand = _internalState.PendingCommands[cmdIx];
                irobot.RoombaCommand cmd = atomicCommand.Body;
                int packetLength = cmd.ExpectedResponseBytes();
                int bufLength = (_internalState.End - _internalState.Start);
                if (bufLength >= packetLength)
                {
                    irobot.RoombaReturnPacket response = GetResponsePacket(cmd, dataReceivedTime, out ixCmdStart);
                    if (response != null)
                    {
                        if (response.ValidPacket)
                        {
                            found = true;

#if DEBUG
                            #region Calculate Perf Statistics

                            double ms = ((TimeSpan)(DateTime.Now.Subtract(atomicCommand.Body.CmdStarted))).TotalMilliseconds;
                            _internalState.iRobotPerf.TotalResponseTimeMs += ms;
                            _internalState.iRobotPerf.TotalResponses++;
                            _internalState.iRobotPerf.AverageResponseMs = _internalState.iRobotPerf.TotalResponseTimeMs / _internalState.iRobotPerf.TotalResponses;
                            if (_internalState.iRobotPerf.MinResponseTimeMs > ms)
                            {
                                _internalState.iRobotPerf.MinResponseTimeMs = ms;
                                LogVerbose(LogGroups.Console, "Minimum response: " + _internalState.iRobotPerf.MinResponseTimeMs.ToString());
                            }
                            if (_internalState.iRobotPerf.MaxResponseTimeMs < ms)
                            {
                                _internalState.iRobotPerf.MaxResponseTimeMs = ms;
                                LogVerbose(LogGroups.Console, "Maximum response: " + _internalState.iRobotPerf.MaxResponseTimeMs.ToString());
                            }
                            #endregion
#endif

                            // Post the response
                            atomicCommand.ResponsePort.Post(response);

                            // If we have any prior commands we must deal with them now.
                            for (int ix = 0; ix < cmdIx; ix++)
                            {
                                _internalState.PendingCommands[ix].ResponsePort.Post(Fault.FromException(new InvalidOperationException("Response was lost on the buffer")));
                            }

                            // Remove the pending command
                            _internalState.PendingCommands.RemoveRange(0, cmdIx + 1);

                            _internalState.Start = ixCmdStart + packetLength;
                            break;
                        }
                        else
                        {
                            LogInfo(LogGroups.Console, "Bad response from iRobot.  Purge and Resync...");
                            _internalState.Start = _internalState.End = 0;
                            _internalState.PendingCommands.Clear();

                            if (_state.StopOnResync)
                                _streamPort.ProcessAtomicCommand(new irobot.CmdDrive(0, 0));

                            if (cmd.RoombaCommandCode != irobot.RoombaCommandCode.FirmwareDate)
                                _streamPort.ProcessAtomicCommand(new irobot.CmdFirmwareDate());
                        }
                    }
                }
            }
            return found;
        }

        /// <summary>
        /// Retrieve a response packet from the data stream
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="timestamp"></param>
        /// <param name="ixCmdStart"></param>
        /// <returns></returns>
        private irobot.RoombaReturnPacket GetResponsePacket(irobot.RoombaCommand cmd, DateTime timestamp, out int ixCmdStart)
        {
            ixCmdStart = _internalState.Start;
            irobot.RoombaMode newMode = _state.Mode;

            if (cmd == null)
                return null;

            bool dataChanged = false;

            // If this command doesn't expect a response
            // generate a standard response.
            int packetLength = cmd.ExpectedResponseBytes();

            // See if this command causes the mode to change.
            create.IRobotUtility.GetChangedMode(cmd.RoombaCommandCode, _state.Mode, _state.IRobotModel, out newMode);

            if (packetLength == 0)
                return irobot.RoombaCommandReceived.Instance(newMode);

            // If there isn't enough data, exit
            if (_internalState.ReadBuffer == null || packetLength > (_internalState.End - _internalState.Start))
                return null;

            // Get the exact packet
            byte[] source = irobot.ByteArray.SubArray(_internalState.ReadBuffer, _internalState.Start, packetLength);

            switch (cmd.RoombaCommandCode)
            {
                case irobot.RoombaCommandCode.FirmwareDate:
                    int ix = create.IRobotUtility.FindFirmwarePacket(_internalState.ReadBuffer, _internalState.Start, _internalState.End);
                    if (ix >= 0)
                    {
                        if (ix > _internalState.Start)
                        {
                            source = irobot.ByteArray.SubArray(_internalState.ReadBuffer, ix, packetLength);
                            ixCmdStart = _internalState.Start;
                        }

                        irobot.ReturnFirmwareDate returnFirmwareDate = new irobot.ReturnFirmwareDate(source, timestamp);
                        returnFirmwareDate.RoombaMode = newMode;
                        return returnFirmwareDate;
                    }
                    break;

                case irobot.RoombaCommandCode.Sensors:
                    bool changedPose, changedPower, changedSensors, changedTelemetry, changedCliffDetail;
                    changedPose = changedPower = changedSensors = changedTelemetry = changedCliffDetail = false;

                    irobot.CmdSensors cmdSensors = cmd as irobot.CmdSensors;
                    #region retrieve the variable length Sensors data
                    if (cmdSensors != null)
                    {
                        switch (cmdSensors.CreateSensorPacket)
                        {
                            case create.CreateSensorPacket.AllPose:
                                irobot.ReturnPose returnPose = new irobot.ReturnPose(_state.Mode, source);
                                returnPose.RoombaMode = newMode;
                                returnPose.Timestamp = timestamp;
                                create.IRobotUtility.AdjustDistanceAndAngle(returnPose, _state.Pose);

                                dataChanged = (_state.Pose == null || !irobot.ByteArray.IsEqual(returnPose.Data, _state.Pose.Data));
                                if (dataChanged && returnPose.ValidPacket)
                                {
                                    _internalPort.PostUnknownType(new sensorupdates.UpdatePose(returnPose));
                                    SendNotification<sensorupdates.UpdatePose>(returnPose);
                                }

                                return returnPose;

                            case create.CreateSensorPacket.AllPower:
                                irobot.ReturnPower returnPower = new irobot.ReturnPower(_state.Mode, source);
                                returnPower.RoombaMode = newMode;
                                returnPower.Timestamp = timestamp;

                                dataChanged = (_state.Power == null || !irobot.ByteArray.IsEqual(returnPower.Data, _state.Power.Data));
                                if (dataChanged && returnPower.ValidPacket)
                                {
                                    _internalPort.PostUnknownType(new sensorupdates.UpdatePower(returnPower));
                                    SendNotification<sensorupdates.UpdatePower>(returnPower);
                                }
                                return returnPower;

                            case create.CreateSensorPacket.AllBumpsCliffsAndWalls:
                                irobot.ReturnSensors returnSensors = new irobot.ReturnSensors(_state.Mode, source, timestamp);
                                returnSensors.RoombaMode = newMode;

                                dataChanged = (_state.Sensors == null || !irobot.ByteArray.IsEqual(returnSensors.Data, _state.Sensors.Data));
                                if (dataChanged && returnSensors.ValidPacket)
                                {
                                    _internalPort.PostUnknownType(new sensorupdates.UpdateBumpsCliffsAndWalls(returnSensors));
                                    SendNotification<sensorupdates.UpdateBumpsCliffsAndWalls>(returnSensors);
                                }

                                return returnSensors;

                            case create.CreateSensorPacket.AllTelemetry:
                                create.ReturnTelemetry returnTelemetry = new create.ReturnTelemetry(source);
                                returnTelemetry.RoombaMode = returnTelemetry.OIMode;
                                returnTelemetry.Timestamp = timestamp;

                                dataChanged = (_state.Telemetry == null || !irobot.ByteArray.IsEqual(returnTelemetry.Data, _state.Telemetry.Data));
                                if (dataChanged && returnTelemetry.ValidPacket)
                                {
                                    _internalPort.PostUnknownType(new sensorupdates.UpdateTelemetry(returnTelemetry));
                                    SendNotification<sensorupdates.UpdateTelemetry>(returnTelemetry);
                                }

                                return returnTelemetry;

                            case create.CreateSensorPacket.AllCliffDetail:
                                create.ReturnCliffDetail returnCliffDetail = new create.ReturnCliffDetail(newMode, source);
                                returnCliffDetail.Timestamp = timestamp;

                                dataChanged = (_state.CliffDetail == null || !irobot.ByteArray.IsEqual(returnCliffDetail.Data, _state.CliffDetail.Data));
                                if (dataChanged && returnCliffDetail.ValidPacket)
                                {
                                    _internalPort.PostUnknownType(new sensorupdates.UpdateCliffDetail(returnCliffDetail));
                                    SendNotification<sensorupdates.UpdateCliffDetail>(returnCliffDetail);
                                }
                                return returnCliffDetail;

                            case create.CreateSensorPacket.AllRoomba:
                                irobot.ReturnAll returnAll = new irobot.ReturnAll();
                                returnAll.Telemetry = null;
                                returnAll.CliffDetail = null;
                                irobot.ByteArray.CopyTo(returnAll.Sensors.Data, source, 0, 0, 10);
                                irobot.ByteArray.CopyTo(returnAll.Pose.Data, source, 10, 0, 6);
                                irobot.ByteArray.CopyTo(returnAll.Power.Data, source, 16, 0, 10);
                                returnAll.RoombaMode = newMode;
                                returnAll.Sensors.Timestamp = timestamp;
                                returnAll.Pose.Timestamp = timestamp;
                                returnAll.Power.Timestamp = timestamp;
                                returnAll.Timestamp = timestamp;
                                create.IRobotUtility.AdjustDistanceAndAngle(returnAll.Pose, _state.Pose);

                                if (returnAll.ValidPacket)
                                {
                                    // When data changes, send Notifications for Pose, Power, & Sensors
                                    if (_state.Sensors == null || !irobot.ByteArray.IsEqual(returnAll.Sensors.Data, _state.Sensors.Data))
                                        dataChanged = changedSensors = true;
                                    else
                                        returnAll.Sensors = null;

                                    if (_state.Pose == null || !irobot.ByteArray.IsEqual(returnAll.Pose.Data, _state.Pose.Data))
                                        dataChanged = changedPose = true;
                                    else
                                        returnAll.Pose = null;

                                    if (_state.Power == null || !irobot.ByteArray.IsEqual(returnAll.Power.Data, _state.Power.Data))
                                        dataChanged = changedPower = true;
                                    else
                                        returnAll.Power = null;

                                    if (dataChanged)
                                    {
                                        _internalPort.PostUnknownType(new sensorupdates.UpdateAll(returnAll));
#if NET_CF
                                        SendNotification<sensorupdates.UpdateAll>(returnAll);
#else
                                        if (changedSensors)
                                            SendNotification<sensorupdates.UpdateBumpsCliffsAndWalls>(returnAll.Sensors);
                                        if (changedPose)
                                            SendNotification<sensorupdates.UpdatePose>(returnAll.Pose);
                                        if (changedPower)
                                            SendNotification<sensorupdates.UpdatePower>(returnAll.Power);
#endif
                                    }
                                }
                                else
                                {
                                    // bad data!
                                    LogError(LogGroups.Console, "Bad Data:");
                                }

                                return returnAll;

                            case create.CreateSensorPacket.AllCreate:
                                irobot.ReturnAll returnAllCreate = new irobot.ReturnAll();

                                if (returnAllCreate.CliffDetail == null)
                                    returnAllCreate.CliffDetail = new create.ReturnCliffDetail();
                                if (returnAllCreate.Telemetry == null)
                                    returnAllCreate.Telemetry = new create.ReturnTelemetry();

                                irobot.ByteArray.CopyTo(returnAllCreate.Sensors.Data, source, 0, 0, 10);
                                irobot.ByteArray.CopyTo(returnAllCreate.Pose.Data, source, 10, 0, 6);
                                irobot.ByteArray.CopyTo(returnAllCreate.Power.Data, source, 16, 0, 10);
                                irobot.ByteArray.CopyTo(returnAllCreate.CliffDetail.Data, source, 26, 0, 14);
                                irobot.ByteArray.CopyTo(returnAllCreate.Telemetry.Data, source, 40, 0, 12);
                                returnAllCreate.RoombaMode = newMode;
                                returnAllCreate.Sensors.Timestamp = timestamp;
                                returnAllCreate.Pose.Timestamp = timestamp;
                                returnAllCreate.Power.Timestamp = timestamp;
                                returnAllCreate.Telemetry.Timestamp = timestamp;
                                returnAllCreate.CliffDetail.Timestamp = timestamp;
                                returnAllCreate.Timestamp = timestamp;

                                create.IRobotUtility.AdjustDistanceAndAngle(returnAllCreate.Pose, _state.Pose);

                                if (returnAllCreate.ValidPacket)
                                {
                                    // Send Notifications for Pose, Power, Sensors, CliffDetail, & Telemetry
                                    if (_state.Sensors == null || !irobot.ByteArray.IsEqual(returnAllCreate.Sensors.Data, _state.Sensors.Data))
                                        dataChanged = changedSensors = true;

                                    if (_state.Pose == null || !irobot.ByteArray.IsEqual(returnAllCreate.Pose.Data, _state.Pose.Data))
                                        dataChanged = changedPose = true;

                                    if (_state.Power == null || !irobot.ByteArray.IsEqual(returnAllCreate.Power.Data, _state.Power.Data))
                                        dataChanged = changedPower = true;

                                    if (returnAllCreate.Telemetry != null &&
                                        (_state.Telemetry == null
                                            || !irobot.ByteArray.IsEqual(returnAllCreate.Telemetry.Data, _state.Telemetry.Data)))
                                        dataChanged = changedTelemetry = true;

                                    if (returnAllCreate.CliffDetail != null &&
                                        (_state.CliffDetail == null
                                            || !irobot.ByteArray.IsEqual(returnAllCreate.CliffDetail.Data, _state.CliffDetail.Data)))
                                        dataChanged = changedCliffDetail = true;

                                    if (dataChanged)
                                    {
                                        if (!changedSensors)
                                            returnAllCreate.Sensors = null;
                                        if (!changedPose)
                                            returnAllCreate.Pose = null;
                                        if (!changedCliffDetail)
                                            returnAllCreate.CliffDetail = null;
                                        if (!changedPower)
                                            returnAllCreate.Power = null;
                                        if (!changedTelemetry)
                                            returnAllCreate.Telemetry = null;

                                        _internalPort.PostUnknownType(new sensorupdates.UpdateAll(returnAllCreate));
#if NET_CF
                                        SendNotification<sensorupdates.UpdateAll>(new sensorupdates.UpdateAll(returnAllCreate));
#else
                                        if (changedSensors)
                                            SendNotification<sensorupdates.UpdateBumpsCliffsAndWalls>(returnAllCreate.Sensors);
                                        if (changedPose)
                                            SendNotification<sensorupdates.UpdatePose>(returnAllCreate.Pose);
                                        if (changedCliffDetail)
                                            SendNotification<sensorupdates.UpdateCliffDetail>(returnAllCreate.CliffDetail);
                                        if (changedPower)
                                            SendNotification<sensorupdates.UpdatePower>(returnAllCreate.Power);
                                        if (changedTelemetry)
                                            SendNotification<sensorupdates.UpdateTelemetry>(returnAllCreate.Telemetry);
#endif
                                    }
                                }
                                return returnAllCreate;

                            default:
                                // Requested a single value.
                                List<create.CreateSensorPacket> sensor = new List<create.CreateSensorPacket>();
                                sensor.Add(cmdSensors.CreateSensorPacket);
                                create.CmdQueryList cmdSensorList = new create.CmdQueryList(sensor);
                                create.ReturnQueryList returnQueryList = new create.ReturnQueryList(source, cmdSensorList);
                                returnQueryList.Timestamp = timestamp;

                                // Did we just get the Mode?
                                if (cmdSensors.CreateSensorPacket == create.CreateSensorPacket.OIMode)
                                    newMode = (irobot.RoombaMode)returnQueryList.NamedValues(_state)[create.CreateSensorPacket.OIMode];

                                returnQueryList.RoombaMode = newMode;
                                return returnQueryList;
                        }
                    }
                    #endregion
                    break;

                case irobot.RoombaCommandCode.QueryList:
                    create.CmdQueryList cmdQueryList = cmd as create.CmdQueryList;
                    if (cmdQueryList != null)
                    {
                        create.ReturnQueryList returnQueryList = new create.ReturnQueryList(source, cmdQueryList);
                        returnQueryList.Timestamp = timestamp;
                        returnQueryList.RoombaMode = newMode;
                        return returnQueryList;
                    }
                    break;

                case irobot.RoombaCommandCode.ShowScript:
                    create.ReturnScript returnScript = new create.ReturnScript(source, timestamp);
                    returnScript.RoombaMode = newMode;
                    return returnScript;

                case irobot.RoombaCommandCode.PlayScript:
                    // Throw away the data returned by the script.
                    // This is generally used to signal the end of the script.
                    return irobot.RoombaCommandReceived.Instance(_state.Mode);

                default:
                    throw new Exception("The method or operation is not implemented: " + cmd.RoombaCommandCode.ToString() + ".");
            }
            return null;

        }


        /// <summary>
        /// Configure the Create
        /// </summary>
        /// <param name="connect"></param>
        /// <returns></returns>
        private IEnumerator<ITask> ConfigureCreate(bool connect)
        {
            DateTime firmwareDate = DateTime.MinValue;

            create.IRobotUtility.ValidateState(ref _state, connect);

            string model = create.IRobotUtility.iRobotModelName(_state);
            int errorCount = 0;

            // Initialize the mode
            yield return Arbiter.Choice(_internalPort.UpdateMode(irobot.RoombaMode.Uninitialized, irobot.RoombaMode.Off), DefaultUpdateResponseTypeHandler, DefaultFaultHandler);

            _state.BaudRate = create.IRobotUtility.GetDefaultBaudRate(_state.ConnectionType, _state.BaudRate);

            bool connected = false;
            if (connect)
            {
                yield return Arbiter.Choice(ConnectIRobotStream(_state),
                    delegate(stream.ReplaceStreamResponse response) { connected = response.Connected; },
                    DefaultFaultHandler);
            }

            if (!connected || !create.IRobotUtility.ValidState(_state))
            {
                _state.Sensors = null;
                _state.Pose = null;
                _state.Power = null;
                _state.Telemetry = null;
                _state.CliffDetail = null;

                BrowseToThisService(connect);

                yield break;
            }

            LogInfo("Connecting to " + model + " ...");
            irobot.RoombaResponsePort responsePort = new irobot.RoombaResponsePort();

            bool success = false;
            // Send Start (Place the Roomba in Passive mode)
            while (!success && errorCount < 10)
            {
                responsePort = SendRoombaCommand(new irobot.InternalCmdStart());
                yield return Arbiter.Choice(
                    Arbiter.Receive<irobot.RoombaCommandReceived>(false, responsePort,
                        delegate(irobot.RoombaCommandReceived receivedStart)
                        {
                            success = true;
                        }),
                    Arbiter.Receive<Fault>(false, responsePort,
                        delegate(Fault fault)
                        {
                            LogError(fault);
                            errorCount++;
                        }));

                if (success)
                {
                    success = false;

                    // Get the Firmware Date.
                    responsePort = SendRoombaCommand(new irobot.CmdFirmwareDate());
                    yield return Arbiter.Choice(
                        Arbiter.Receive<irobot.ReturnFirmwareDate>(false, responsePort,
                            delegate(irobot.ReturnFirmwareDate response)
                            {
                                success = true;
                                firmwareDate = response.FirmwareDate;
                            }),
                        Arbiter.Receive<Fault>(false, responsePort,
                            delegate(Fault fault)
                            {
                                LogError(fault);
                                errorCount++;
                            }));

                }

                // If not successful yet, pause before retry.
                if (!success)
                    yield return Arbiter.Receive(false, TimeoutPort(500), delegate(DateTime timeout) { });
            }

            if (!success)
            {
                // iRobot is not initialized
                if (_state.Mode != irobot.RoombaMode.Uninitialized)
                    yield return Arbiter.Choice(_internalPort.UpdateMode(irobot.RoombaMode.Uninitialized),
                        DefaultUpdateResponseTypeHandler,
                        DefaultFaultHandler);

                BrowseToThisService(connect);
                yield break;
            }

            // Put Roomba in Safe mode
            responsePort = SendRoombaCommand(new irobot.InternalCmdSafe());
            yield return Arbiter.Choice(
                Arbiter.Receive<irobot.RoombaCommandReceived>(false, responsePort,
                    delegate(irobot.RoombaCommandReceived receivedStart) { }),
                Arbiter.Receive<Fault>(false, responsePort,
                    delegate(Fault fault)
                    {
                        LogError(fault);
                        errorCount++;
                    }));

            bool song1Defined = false;
            bool song2Defined = false;
            if (_state.SongDefinitions == null)
            {
                _state.SongDefinitions = new List<irobot.CmdDefineSong>();
            }
            else
            {
                foreach (irobot.CmdDefineSong cmdDefineSong in _state.SongDefinitions)
                {
                    if (cmdDefineSong.SongNumber == 1)
                        song1Defined = true;
                    if (cmdDefineSong.SongNumber == 2)
                        song2Defined = true;
                    SendRoombaCommand(cmdDefineSong);
                }
            }
            irobot.CmdDefineSong song;
            if (!song1Defined)
            {
                song = create.IRobotUtility.DefineSimpleSong(1);
                SendRoombaCommand(song);
                _state.SongDefinitions.Add(song);
            }

            if (!song2Defined)
            {
                song = create.IRobotUtility.DefinePlayfulSong(2);
                SendRoombaCommand(song);
                _state.SongDefinitions.Add(song);
            }

            // Play the song
            LogInfo(LogGroups.Console, "Playing a song");
            SendRoombaCommand(new irobot.CmdPlaySong(1));

            irobot.IRobotModel newRobotModel = _state.IRobotModel;
            bool identifyModel = (_state.IRobotModel == irobot.IRobotModel.NotSpecified);

            // Read the sensors one time.
            LogInfo(LogGroups.Console, "Reading the sensors");
            responsePort = SendRoombaCommand(new irobot.CmdSensors(irobot.RoombaQueryType.ReturnAll));
            yield return Arbiter.Choice(
                Arbiter.Receive<irobot.ReturnAll>(false, responsePort, delegate(irobot.ReturnAll rsp)
                {
                    LogInfo(LogGroups.Console, "Success reading standard sensor data");
                    if (identifyModel)
                        newRobotModel = irobot.IRobotModel.Roomba;
                }),
                Arbiter.Receive<Fault>(false, responsePort, delegate(Fault f)
                {
                    string reason = (f.Reason != null && f.Reason.Length >= 1) ? f.Reason[0].Value : "No sensor data received";
                    LogError(LogGroups.Console, reason);
                    errorCount++;
                }));

            // Are we connected?
            if (errorCount > 0)
            {
                BrowseToThisService(connect);
                yield break;
            }

            // A graceful check for Create which won't break the Roomba.
            if (identifyModel || _state.IRobotModel == irobot.IRobotModel.Create)
            {
                responsePort = SendRoombaCommand(new irobot.CmdSensors(irobot.RoombaQueryType.ReturnAllCreate));
                yield return Arbiter.Choice(
                    Arbiter.Receive<irobot.ReturnAll>(false, responsePort, delegate(irobot.ReturnAll rsp)
                    {
                        LogInfo(LogGroups.Console, "Identified iRobot model as Create");
                        if (identifyModel)
                            newRobotModel = irobot.IRobotModel.Create;
                    }),
                    Arbiter.Receive<Fault>(false, responsePort, delegate(Fault f)
                    {
                        if (_state.IRobotModel == irobot.IRobotModel.Create)
                            LogError(LogGroups.Console, "Unable to retrieve Create specific sensor data.\r\n"
                                + "Please reset your connection and try again.\r\n"
                                + "If you are connecting to a Roomba Discovery,\r\n"
                                + "please update your configuration file per the Roomba Readme.");
                        else
                            LogInfo(LogGroups.Console, "Identified iRobot model as Roomba");
                    }));

            }
            if ((firmwareDate != DateTime.MinValue && firmwareDate != _state.FirmwareDate)
                || (identifyModel && (newRobotModel != irobot.IRobotModel.NotSpecified)))
            {
                // We just changed the mode or firmware date, update _state!
                yield return Arbiter.Choice(_internalPort.UpdateMode(newRobotModel, firmwareDate), DefaultUpdateResponseTypeHandler, DefaultFaultHandler);
            }

            int timerInterval;
            if (_state.IRobotModel == irobot.IRobotModel.Create)
            {
                // Request no sensor stream data
                // This is necessary if the Create was already
                // streaming data when our service connected.
                create.CmdStream cmdStream = new create.CmdStream();
                responsePort = SendRoombaCommand(cmdStream);
                yield return Arbiter.Choice(
                    Arbiter.Receive<irobot.RoombaCommandReceived>(false, responsePort,
                        delegate(irobot.RoombaCommandReceived ackStream)
                        {
                        }),
                    Arbiter.Receive<Fault>(false, responsePort,
                        delegate(Fault fault)
                        {
                            LogError(fault);
                        }));

                // We will catch up on all sensors periodically.
                timerInterval = (_state.PollingInterval <= 0) ? _defaultPollingInterval : _state.PollingInterval;
            }
            else
            {
                LogError(LogGroups.Console, "iRobotLite only supports the iRobot Create.");
                _mainPort.DsspDefaultDrop();
                yield break;
            }

            // for wireless connections, we should not poll more frequently than every 50ms.
            if ((timerInterval > 0 && timerInterval < _minWirelessInterval)
                && (_state.ConnectionType == irobot.iRobotConnectionType.BluetoothAdapterModule
                    || _state.ConnectionType == irobot.iRobotConnectionType.RooTooth))
            {
                timerInterval = _minWirelessInterval;
            }

            StartTimer(timerInterval);
            yield break;
        }


        /// <summary>
        /// Configure the IRobot Stream
        /// <remarks>This should be called any time the
        /// iRobot configuration changes</remarks>
        /// </summary>
        /// <param name="state"></param>
        /// <returns>The Configuration Response Port</returns>
        private PortSet<stream.ReplaceStreamResponse, Fault> ConnectIRobotStream(irobot.RoombaState state)
        {
            List<stream.NameValuePair> parms = new List<stream.NameValuePair>();
            parms.Add(new stream.NameValuePair("SerialPort", state.SerialPort.ToString()));
            parms.Add(new stream.NameValuePair("BaudRate", state.BaudRate.ToString()));
            parms.Add(new stream.NameValuePair("ConnectionType", state.ConnectionType.ToString()));
            stream.StreamState config = new stream.StreamState();
            config.Configurations = parms;
            config.Description = "iRobot Connection";
            config.Identifier = _state.SerialPort;
            config.Initialized = false;
            return _streamPort.ConfigureAndConnect(config);
        }

        /// <summary>
        /// Send a Roomba command and wait for the reply.
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        private irobot.RoombaResponsePort SendRoombaCommand(irobot.RoombaCommand cmd)
        {
            return (irobot.RoombaResponsePort)SendCommand(cmd, null);
        }

        /// <summary>
        /// Send a command to the iRobot and post the response to responsePort.
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="responsePort"></param>
        /// <returns></returns>
        private IPortSet SendCommand(irobot.RoombaCommand cmd, IPortSet responsePort)
        {
            if (responsePort == null)
                responsePort = new irobot.RoombaResponsePort();

            SpawnIterator<irobot.RoombaCommand, IPortSet>(cmd, responsePort, ProcessCommand);
            return responsePort;
        }

        /// <summary>
        /// <remarks>SpawnIterator[RoombaCommand, IPortSet](ProcessCommand)</remarks>
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="responsePort"></param>
        /// <returns></returns>
        private IEnumerator<ITask> ProcessCommand(irobot.RoombaCommand cmd, IPortSet responsePort)
        {
            irobot.RoombaMode newMode;
            bool success = false;

            // Place iRobot in proper mode for this command
            #region Place iRobot in proper mode for this command
            if (!create.IRobotUtility.ValidStateForCommand(cmd, _state, out newMode))
            {
                success = true;
                LogInfo(LogGroups.Console, "Set Mode from " + _state.Mode.ToString() + " to " + newMode.ToString() + ".");
                yield return Arbiter.Choice(_internalPort.ChangeToMode(newMode),
                    DefaultSubmitResponseTypeHandler,
                    delegate(Fault fault)
                    {
                        success = false;
                        responsePort.PostUnknownType(fault);
                    });

                if (!success)
                    yield break;
            }
            #endregion

            success = true;
            double ms = 0.0;
            LogVerbose(LogGroups.Console, "Process Command: " + cmd.RoombaCommandCode.ToString() + ".");
            yield return Arbiter.Choice<irobot.RoombaReturnPacket, Fault>(
                _streamPort.ProcessAtomicCommand(cmd),
                    responsePort.PostUnknownType,
                    delegate(Fault fault)
                    {
                        string reason = (fault.Reason != null && fault.Reason.Length > 0) ? fault.Reason[0].Value : "Fault";
                        ms = DateTime.Now.Subtract(cmd.CmdStarted).TotalMilliseconds;
                        success = false;
                        LogInfo(LogGroups.Console, string.Format("  Response to {0}: Failure after {1} ms: {2}", cmd.RoombaCommandCode, ms, reason));
                        responsePort.PostUnknownType(fault);
                    });


            #region If necessary, maintain a certain mode
            // Do we need to maintain a particular mode?
            if (_state.Mode != _state.MaintainMode &&
                _state.IsInitialized &&
                (_state.MaintainMode == irobot.RoombaMode.Passive
                    || _state.MaintainMode == irobot.RoombaMode.Safe
                    || _state.MaintainMode == irobot.RoombaMode.Full))
            {
                LogInfo(LogGroups.Console, "Maintain Mode (automatic mode switch from " + _state.Mode.ToString() + " to " + _state.MaintainMode.ToString() + ").");
                yield return Arbiter.Choice(_internalPort.ChangeToMode(_state.MaintainMode),
                    DefaultSubmitResponseTypeHandler,
                    DefaultFaultHandler);

            }
            #endregion

            yield break;

        }


        /// <summary>
        /// Default Submit Response Type Handler
        /// </summary>
        /// <param name="ok"></param>
        private void DefaultSubmitResponseTypeHandler(DefaultSubmitResponseType ok)
        {
        }

        /// <summary>
        /// Set the iRobot Mode
        /// </summary>
        /// <param name="roombaSetMode"></param>
        private static irobot.RoombaCommand GetModeCommand(irobot.RoombaSetMode roombaSetMode)
        {
            irobot.RoombaCommand cmd;
            switch (roombaSetMode.Body.RoombaMode)
            {
                case irobot.RoombaMode.Off:
                    cmd = new irobot.InternalCmdReset();
                    break;
                case irobot.RoombaMode.Passive:
                    cmd = new irobot.InternalCmdStart();
                    break;
                case irobot.RoombaMode.Safe:
                    // For Create Only
                    cmd = new irobot.InternalCmdSafe();
                    break;
                case irobot.RoombaMode.Full:
                    cmd = new irobot.InternalCmdFull();
                    break;
                default:
                    cmd = null;
                    break;
            }

            if (cmd != null)
            {
                if (roombaSetMode.Body.MaintainMode)
                    cmd.MaintainMode = roombaSetMode.Body.RoombaMode;
                else
                    cmd.MaintainMode = irobot.RoombaMode.NotSpecified;
            }

            return cmd;
        }


        #endregion

        #region HTTP Processing

        /// <summary>
        /// Send Http Post Success Response
        /// </summary>
        /// <param name="httpPost"></param>
        /// <param name="state"></param>
        /// <param name="transform"></param>
        private static void HttpPostSuccess(HttpPost httpPost, irobot.RoombaState state, string transform)
        {
            HttpResponseType rsp =
                new HttpResponseType(HttpStatusCode.OK, state, transform);
            httpPost.ResponsePort.Post(rsp);
        }

        /// <summary>
        /// Send Http Post Failure Response
        /// </summary>
        private static void HttpPostFailure(HttpPost httpPost, Fault fault)
        {
            HttpResponseType rsp =
                new HttpResponseType(HttpStatusCode.BadRequest, fault);
            httpPost.ResponsePort.Post(rsp);
        }

        /// <summary>
        /// Start up the web browser so user can configure
        /// </summary>
        /// <param name="triedToConnect"></param>
        private void BrowseToThisService(bool triedToConnect)
        {
            if (!_internalState.BrowsedToService)
            {
                _internalState.BrowsedToService = true;
#if NET_CF
            if (triedToConnect)
                LogInfo(LogGroups.Console, "Please set up a valid configuration file to initialize your iRobot");
#else
                System.Diagnostics.Process process = new System.Diagnostics.Process();
                process.StartInfo.FileName = FindServiceAliasFromScheme(Uri.UriSchemeHttp);
                process.Start();
#endif
            }
        }

        #endregion

        #region Keep Alive Polling

        /// <summary>
        /// Start the timer
        /// </summary>
        private void StartTimer(int tickInterval)
        {
            if (_tickInterval != tickInterval)
                _tickInterval = tickInterval;

            if (_nextTimer == DateTime.MaxValue)
            {
                _nextTimer = DateTime.Now.AddMilliseconds(_tickInterval);
                Activate(Arbiter.Receive(false, TimeoutPort(_tickInterval), TimerHandler));
            }
            else
            {
                // skip any backlog of timer events
                _nextTimer = DateTime.Now.AddMilliseconds(_tickInterval);

                // Don't need to activate because there is still a pending receive.
            }
        }

        /// <summary>
        /// The Timer Handler fires every _tickInterval milliseconds
        /// </summary>
        /// <param name="time"></param>
        private void TimerHandler(DateTime time)
        {
            // Stop the timer if we are shutting down
            if (_state.Mode == irobot.RoombaMode.Shutdown)
                return;

            // ignore timer if Roomba is not initialized.
            if (_state.Mode == irobot.RoombaMode.Uninitialized)
            {
                WaitForNextTimer();
                return;
            }

            DateTime now = DateTime.Now;


            // If we are maintaining mode and have been in the wrong mode for > 1 second
            // then attempt to set the mode back to our maintained mode.
            if ((_state.MaintainMode == irobot.RoombaMode.Safe || _state.MaintainMode == irobot.RoombaMode.Full)
                && _state.MaintainMode != _state.Mode
                && _state.IsInitialized)
            {
                int waitForModeChangeMs = Math.Max(1000, _tickInterval);
                if (_startedInvalidMode == DateTime.MaxValue)
                {
                    _startedInvalidMode = now;
                }
                else if (((TimeSpan)now.Subtract(_startedInvalidMode)).TotalMilliseconds >= waitForModeChangeMs)
                {
                    LogInfo(LogGroups.Console, "Maintain Mode (automatic mode switch from " + _state.Mode.ToString() + " to " + _state.MaintainMode.ToString() + ").");
                    _internalPort.ChangeToMode(_state.MaintainMode);

                    // reset timer, even if we failed to change the mode
                    // in this case, we'll try again in another second.
                    _startedInvalidMode = DateTime.MaxValue;
                }
            }
            else if (_startedInvalidMode != DateTime.MaxValue)
            {
                // The mode changed back as a result of a command
                // or we are no longer maintaining mode, so
                // reset the maintain mode timer.
                _startedInvalidMode = DateTime.MaxValue;
            }

            irobot.RoombaResponsePort response;

            // Is Polling turned off?
            if (_state.PollingInterval < 0)
            {
                WaitForNextTimer();
                return;
            }

            LogVerbose(LogGroups.Console, "Timer: Starting query");
            // Get Sensors
            if (_state.IRobotModel == irobot.IRobotModel.Create)
            {
                response = SendRoombaCommand(new irobot.CmdSensors(irobot.RoombaQueryType.ReturnAllCreate));
            }
            else
            {
                response = SendRoombaCommand(new irobot.CmdSensors(irobot.RoombaQueryType.ReturnAll));
            }

            Activate(Arbiter.Choice(
                Arbiter.Receive<irobot.ReturnAll>(false, response, WaitForNextTimer),
                Arbiter.Receive<Fault>(false, response,
                    delegate(Fault f)
                    {
                        string reason = (f.Reason != null && f.Reason.Length >= 1) ? f.Reason[0].Value : "No sensor data received from the Roomba";
                        LogError(reason);
                        WaitForNextTimer();
                    }),
                Arbiter.Receive<DateTime>(false, TimeoutPort(5000), WaitForNextTimer)));
        }

        /// <summary>
        /// Wait for _tickInterval before calling the TimerHandler
        /// </summary>
        /// <param name="ok"></param>
        private void WaitForNextTimer(DateTime ok)
        {
            WaitForNextTimer();
        }

        /// <summary>
        /// Wait for _tickInterval before calling the TimerHandler
        /// </summary>
        /// <param name="ok"></param>
        private void WaitForNextTimer(irobot.ReturnAll ok)
        {
            WaitForNextTimer();
        }

        /// <summary>
        /// Wait for _tickInterval before calling the TimerHandler
        /// </summary>
        private void WaitForNextTimer()
        {
            // Stop the timer if we are shutting down.
            if (_state.Mode == irobot.RoombaMode.Shutdown)
                return;

            // Find the next scheduled time
            _nextTimer = _nextTimer.AddMilliseconds(_tickInterval);

            // grab a moment in time
            DateTime now = DateTime.Now;
            int waitMs = (int)_nextTimer.Subtract(now).TotalMilliseconds;

            // If it's already past time to run, execute the handler ASAP.
            if (waitMs < 10)
                waitMs = 1;

            Activate(Arbiter.Receive(false, TimeoutPort(waitMs), TimerHandler));
        }

        #endregion

        #region Subscription Helper Methods


        /// <summary>
        /// Sends a message to the subscription manager to send a notification.
        /// </summary>
        /// <typeparam name="T">The message type to send as a notification</typeparam>
        /// <param name="notificationBody">Message body to send in the notification</param>
        protected void SendNotification<T>(
            object notificationBody)
            where T : DsspOperation, new()
        {
            SendNotification<T>(string.Empty, notificationBody);
        }

        /// <summary>
        /// Sends a message to the subscription manager to send a notification.
        /// </summary>
        /// <typeparam name="T">The message type to send as a notification</typeparam>
        /// <param name="subscriber">Address of a subscriber to notify. If this is null or Empty then all subscribers are notified</param>
        /// <param name="notificationBody">Message body to send in the notification</param>
        protected void SendNotification<T>(
            string subscriber,
            object notificationBody)
            where T : DsspOperation, new()
        {
            T msg = new T();
            Type msgType = msg.GetType();
            Type[] genericTypes = msgType.GetGenericArguments();

            if (genericTypes.Length >= 2 &&
                genericTypes[0] != notificationBody.GetType())
            {
                throw new ArgumentException(
                    "Incorrect notification type, expecting: " + genericTypes[0].FullName,
                    "notificationBody");
            }

            msg.Body = notificationBody;

            SendNotification<T>(subscriber, msg);
        }

        /// <summary>
        /// Send Notification (without SubMgr)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="notification"></param>
        protected void SendNotification<T>(T notification) where T : DsspOperation, new()
        {
            notification.ConvertToNotification();
            foreach (Port<DsspOperation> notificationPort in _subscribers.Values)
                notificationPort.PostUnknownType(notification);
        }

        /// <summary>
        /// Sends a message to the subscription manager to send a notification.
        /// </summary>
        /// <typeparam name="T">The message type to send as a notification</typeparam>
        /// <param name="subscriber">Address of a subscriber to notify. If this is null or Empty then all subscribers are notified</param>
        /// <param name="notification">Message to send as a notification</param>
        protected void SendNotification<T>(
            string subscriber,
            T notification)
            where T : DsspOperation
        {
            if (notification.Action != DsspActions.DeleteRequest &&
                notification.Action != DsspActions.InsertRequest &&
                notification.Action != DsspActions.ReplaceRequest &&
                notification.Action != DsspActions.UpdateRequest &&
                notification.Action != DsspActions.UpsertRequest)
            {
                throw new ArgumentException(
                    notification.Action + " is not a state modifying verb",
                    notification.GetType().FullName);
            }

            notification.ConvertToNotification();
            if (string.IsNullOrEmpty(subscriber))
            {
                foreach (Port<DsspOperation> notificationPort in _subscribers.Values)
                    notificationPort.PostUnknownType(notification);
            }
            else
            {
                string key = subscriber.ToLower();
                if (_subscribers.ContainsKey(key))
                {
                    Port<DsspOperation> notificationPort = _subscribers[key];
                    if (notificationPort != null)
                        notificationPort.PostUnknownType(notification);
                }
            }
        }

        #endregion
    }
}
