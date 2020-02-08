//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: Roomba.cs $ $Revision: 43 $
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
using cons = Microsoft.Dss.Services.Constructor;
using stream = Microsoft.Robotics.Services.DssStream.Proxy;
using istream = Microsoft.Robotics.Services.IRobot.DssStream;
using lite = Microsoft.Robotics.Services.IRobot.Lite;
using System.Text;
using Microsoft.Robotics.Services.IRobot.Roomba.Properties;

namespace Microsoft.Robotics.Services.IRobot.Roomba
{
    /// <summary>
    /// iRobot Roomba and Create Service Implementation
    /// </summary>
    /// <remarks>The iRobot Roomba service calls to the serial port and may block a thread
    /// The ActivationSettings attribute with Sharing == false makes the runtime
    /// dedicate a dispatcher thread pool just for this service.</remarks>
    [ActivationSettings(ShareDispatcher = false, ExecutionUnitsPerDispatcher = 2)]
    [DisplayName("(User) iRobot� Create / Roomba")]
    [Description("Provides access to an iRobot Create or Roomba.")]
    [Contract(Contract.Identifier)]
    [AlternateContract(Create.Contract.Identifier)]
    [AlternateContract(lite.Contract.Identifier)]
    public class IRobotService : DsspServiceBase
    {
        [EmbeddedResource("Microsoft.Robotics.Services.IRobot.Roomba.Resources.iRobot.user.xslt")]
        private string _transform = null;

        [InitialStatePartner(Optional = true, ServiceUri = ServicePaths.Store + "/iRobot.user.config.xml")]
        private RoombaState _state = new RoombaState();

        [ServicePort("/irobot", AllowMultipleInstances=true)]
        private RoombaOperations _mainPort = new RoombaOperations();

        DsspHttpUtilitiesPort _httpUtilities = new DsspHttpUtilitiesPort();

        [AlternateServicePort("/create",
            AllowMultipleInstances = true,
            AlternateContract = Create.Contract.Identifier)]
        private CreateOperations _createOperationsPort = new CreateOperations();

        [AlternateServicePort("/lite",
            AllowMultipleInstances = true,
            AlternateContract = lite.Contract.Identifier)]
        private lite.IRobotLiteOperations _liteOperationsPort = new lite.IRobotLiteOperations();

        /// <summary>
        /// Internal port for handling state updates
        /// </summary>
        private InternalMessages _internalPort = new InternalMessages();
        private Port<ExecuteIRobotCommand> _exclusiveCommandPort = new Port<ExecuteIRobotCommand>();

        private bool _streamStarted = false;
        private bool _streamPaused = false;
        private bool _directoryInserted = false;
        private bool _browsedToService = false;

        private Port<ExecuteIRobotCommand> _internalCommandWaitingPort = new Port<ExecuteIRobotCommand>();
        private Port<BytePacket> _internalSerialBytesPending = new Port<BytePacket>();
        private stream.StreamOperations _notificationsPort = new stream.StreamOperations();

        /// <summary>
        /// Default sensor polling rate without streaming sensors (ms)
        /// </summary>
        private const int _defaultPollingInterval = 250;

        /// <summary>
        /// Default sensor polling rate with streaming sensors (ms)
        /// </summary>
        private const int _defaultPollingWithStreaming = 1000;

        /// <summary>
        /// Minimum polling rate over wireless connection (ms)
        /// </summary>
        private const int _minWirelessInterval = 70;

        private int _tickInterval;
        private DateTime _nextTimer = DateTime.MaxValue;
        private DateTime _startedInvalidMode = DateTime.MaxValue;

        // Subscription manager partner
        [Partner(Partners.SubscriptionManagerString,
            Contract = submgr.Contract.Identifier,
            CreationPolicy = PartnerCreationPolicy.CreateAlways)]
        submgr.SubscriptionManagerPort _subMgrPort = new submgr.SubscriptionManagerPort();

        // iRobot communications
        [Partner(Contract.Identifier + ":irobotstream", Contract = stream.Contract.Identifier,
            CreationPolicy = PartnerCreationPolicy.UsePartnerListEntry,
            Optional = true)]
        stream.StreamOperations _streamPort = new stream.StreamOperations();

        private bool _subscribedToStream = false;
        private byte[] _priorNotification = null;


        /// <summary>
        /// Default Service Constructor
        /// </summary>
        public IRobotService(DsspServiceCreationPort creationPort) :
                base(creationPort)
        {
        }

        /// <summary>
        /// Service Start
        /// </summary>
        protected override void Start()
        {
            //needed for HttpPost
            _httpUtilities = DsspHttpUtilitiesService.Create(Environment);
            ValidateState(true);

#if NET_CF
            LogWarning(LogGroups.Console, Resources.MsgUseIRobotLiteServiceForCE);
#endif

            // Configure the Roomba
            SpawnIterator<bool>(!_state.WaitForConnect, ConfigureRoomba);

            // Listen on the main port for requests and call the appropriate handler.
            Interleave mainInterleave = ActivateDsspOperationHandlers();


            // Listen for external iRobot Create commands
            // mainInterleave.CombineWith(ActivateDsspOperationHandlers(this, _iRobotNotificationPort));
            // mainInterleave.CombineWith(ActivateDsspOperationHandlers(this, _createOperationsPort));
            mainInterleave.CombineWith(
                new Interleave(
                new ExclusiveReceiverGroup(
                    Arbiter.ReceiveWithIterator <Subscribe>(true, _liteOperationsPort, SubscribeHandler)
                ),
                new ConcurrentReceiverGroup(
                    // ---------------------------------------------------------------------------------------------
                    Arbiter.Receive<CreateDemo>(true, _createOperationsPort, CreateDemoHandler),
                    Arbiter.Receive<CreatePWMLowSideDrivers>(true, _createOperationsPort, CreatePWMLowSideDriversHandler),
                    Arbiter.Receive<CreateDriveDirect>(true, _createOperationsPort, CreateDriveDirectHandler),
                    Arbiter.Receive<CreateDigitalOutputs>(true, _createOperationsPort, CreateDigitalOutputsHandler),
                    Arbiter.Receive<CreateStream>(true, _createOperationsPort, CreateStreamHandler),
                    Arbiter.Receive<CreateQueryList>(true, _createOperationsPort, CreateQueryListHandler),
                    Arbiter.Receive<CreateStreamPauseResume>(true, _createOperationsPort, CreateStreamPauseResumeHandler),
                    Arbiter.Receive<CreateSendIR>(true, _createOperationsPort, CreateSendIRHandler),
                    Arbiter.Receive<CreateDefineScript>(true, _createOperationsPort, CreateDefineScriptHandler),
                    Arbiter.Receive<CreatePlayScript>(true, _createOperationsPort, CreatePlayScriptHandler),
                    Arbiter.Receive<CreateShowScript>(true, _createOperationsPort, CreateShowScriptHandler),
                    Arbiter.Receive<CreateWaitTime>(true, _createOperationsPort, CreateWaitTimeHandler),
                    Arbiter.Receive<CreateWaitDistance>(true, _createOperationsPort, CreateWaitDistanceHandler),
                    Arbiter.Receive<CreateWaitAngle>(true, _createOperationsPort, CreateWaitAngleHandler),
                    Arbiter.Receive<CreateWaitEvent>(true, _createOperationsPort, CreateWaitEventHandler),
                    // ---------------------------------------------------------------------------------------------
                    Arbiter.Receive<Get>(true, _liteOperationsPort, GetHandler),
                    Arbiter.Receive<HttpGet>(true, _liteOperationsPort, HttpGetHandler),
                    Arbiter.ReceiveWithIterator<HttpPost>(true, _liteOperationsPort, HttpPostHandler),
                    Arbiter.Receive<Configure>(true, _liteOperationsPort, ConfigureHandler),
                    Arbiter.Receive<Connect>(true, _liteOperationsPort, ConnectHandler),
                    Arbiter.Receive<RoombaSetMode>(true, _liteOperationsPort, RoombaSetModeHandler),
                    Arbiter.Receive<RoombaSetLeds>(true, _liteOperationsPort, RoombaSetLedsHandler),
                    Arbiter.Receive<RoombaPlaySong>(true, _liteOperationsPort, RoombaPlaySongHandler),
                    Arbiter.Receive<RoombaGetSensors>(true, _liteOperationsPort, RoombaGetSensorsHandler),
                    Arbiter.Receive<RoombaDrive>(true, _liteOperationsPort, RoombaDriveHandler),
                    Arbiter.Receive<CreateDriveDirect>(true, _liteOperationsPort, CreateDriveDirectHandler),
                    Arbiter.Receive<UpdateAll>(true, _liteOperationsPort, BlockedCommandHandler),
                    Arbiter.Receive<UpdateBumpsCliffsAndWalls>(true, _liteOperationsPort, BlockedCommandHandler),
                    Arbiter.Receive<UpdatePose>(true, _liteOperationsPort, BlockedCommandHandler),
                    Arbiter.Receive<UpdatePower>(true, _liteOperationsPort, BlockedCommandHandler),
                    Arbiter.Receive<UpdateMode>(true, _liteOperationsPort, BlockedCommandHandler),
                    Arbiter.Receive<UpdateCliffDetail>(true, _liteOperationsPort, BlockedCommandHandler),
                    Arbiter.Receive<UpdateTelemetry>(true, _liteOperationsPort, BlockedCommandHandler),
                    Arbiter.Receive<UpdateNotifications>(true, _liteOperationsPort, BlockedCommandHandler)
                )));


            // Concurrent internal commands
            Activate(
                new Interleave(
                    new TeardownReceiverGroup(
                        Arbiter.Receive<Shutdown>(false, CleanupPort, delegate(Shutdown done) { CleanupPort.Post(done); })),
                    new ExclusiveReceiverGroup(),
                    new ConcurrentReceiverGroup(
                        Arbiter.ReceiveWithIterator<ChangeToMode>(true, _internalPort, ChangeToModeHandler),
                        Arbiter.ReceiveWithIterator<WakeupRoomba>(true, _internalPort, WakeupRoombaHandler),
                        Arbiter.Receive<stream.ReadText>(true, _internalPort, ReceiveText),
                        Arbiter.ReceiveWithIterator<ExecuteIRobotCommand>(true, _internalPort, ExecuteIRobotNoAckHandler),
                        Arbiter.ReceiveWithIterator<ProcessAtomicCommand>(true, _internalPort, ProcessAtomicCommandHandler)
                        )));


            // Set up a seperate interleave to manage state updates.
            // These commands are independent of all others, but exclusive to each other.
            // All updates to the service state must be done within these handlers.
            Activate(
                new Interleave(
                    new TeardownReceiverGroup(
                        Arbiter.Receive<Shutdown>(false, CleanupPort, delegate(Shutdown done) { CleanupPort.Post(done); })),
                    new ExclusiveReceiverGroup(
                        Arbiter.Receive<UpdatePose>(true, _internalPort, UpdateStatePoseHandler),
                        Arbiter.Receive<UpdatePower>(true, _internalPort, UpdateStatePowerHandler),
                        Arbiter.Receive<UpdateBumpsCliffsAndWalls>(true, _internalPort, UpdateStateSensorsHandler),
                        Arbiter.Receive<UpdateAll>(true, _internalPort, UpdateStateAllHandler),
                        Arbiter.Receive<UpdateTelemetry>(true, _internalPort, UpdateTelemetryHandler),
                        Arbiter.Receive<UpdateCliffDetail>(true, _internalPort, UpdateCliffDetailHandler),
                        Arbiter.Receive<UpdateMode>(true, _internalPort, UpdateModeHandler),
                        Arbiter.Receive<DateTime>(true, _internalPort, SetLastUpdatedHandler),
                        Arbiter.Receive<UpdateNotifications>(true, _internalPort, UpdateStreamNotificationHandler),
                        Arbiter.Receive<Configure>(true, _internalPort, UpdateConfigureHandler),
                        Arbiter.Receive<Connect>(true, _internalPort, UpdateConnectHandler)
                    ),
                    new ConcurrentReceiverGroup()));


            // Set up a seperate interleave to manage atomic roomba commands.
            // ExecuteIRobotCommands are independent of all others, but exclusive to each other.
            Activate(new Interleave(
                new TeardownReceiverGroup(
                    Arbiter.Receive<Shutdown>(false, CleanupPort, delegate(Shutdown done) { CleanupPort.Post(done); })),
                new ExclusiveReceiverGroup(
                    Arbiter.ReceiveWithIterator<ExecuteIRobotCommand>(true, _exclusiveCommandPort, ExecuteIRobotWithResponseHandler)),
                new ConcurrentReceiverGroup()));


            // Set up a seperate interleave to manage inbound stream notifications from the iRobot
            // This ensures that they are processed in order.
            Activate(new Interleave(
                new TeardownReceiverGroup(
                    Arbiter.Receive<Shutdown>(false, CleanupPort, delegate(Shutdown done) { CleanupPort.Post(done); })),
                new ExclusiveReceiverGroup(
                    Arbiter.Receive<stream.ReadData>(true, _notificationsPort, ReceiveData)),
                new ConcurrentReceiverGroup()));

        }

        /// <summary>
        /// Set the iRobot Mode
        /// </summary>
        /// <param name="roombaSetMode"></param>
        /// <param name="isCreate"></param>
        private static RoombaCommand GetModeCommand(RoombaSetMode roombaSetMode, bool isCreate)
        {
            RoombaCommand cmd;
            switch (roombaSetMode.Body.RoombaMode)
            {
                case RoombaMode.Off:
                    if (isCreate)
                        cmd = new InternalCmdReset();
                    else
                        cmd = new InternalCmdPower();
                    break;
                case RoombaMode.Passive:
                    cmd = new InternalCmdStart();
                    break;
                case RoombaMode.Safe:
                    cmd = new InternalCmdControl();
                    break;
                case RoombaMode.Full:
                    cmd = new InternalCmdFull();
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
                    cmd.MaintainMode = RoombaMode.NotSpecified;
            }

            return cmd;
        }

        /// <summary>
        /// Make sure the state is valid
        /// </summary>
        /// <param name="initialize">Initialize the state</param>
        private void ValidateState(bool initialize)
        {
            IRobotUtility.ValidateState(ref _state, initialize);
            if (initialize)
                SaveState(_state);
        }


        /// <summary>
        /// Configure the IRobot Stream
        /// <remarks>This should be called any time the
        /// iRobot configuration changes</remarks>
        /// </summary>
        /// <param name="state"></param>
        /// <returns>The Configuration Response Port</returns>
        private PortSet<stream.ReplaceStreamResponse, Fault> ConfigureIRobotStream(RoombaState state)
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
            return _streamPort.ReplaceStreamState(config);
        }

        /// <summary>
        /// Subscribe to data from the iRobot serial port.
        /// </summary>
        private void SubscribeToStream()
        {
            if (_subscribedToStream)
                return;

            Activate(Arbiter.Choice(_streamPort.Subscribe(_notificationsPort),
                delegate(SubscribeResponseType response)
                {
                    _subscribedToStream = true;
                },
                delegate(Fault fault) { LogError(fault); }
            ));
        }

        /// <summary>
        /// Receive Text from iRobot communications stream
        /// </summary>
        /// <param name="insert"></param>
        private void ReceiveText(stream.ReadText insert)
        {
            LogVerbose(LogGroups.Console, "iRobot Text:" + insert.Body.Text);
            if (insert.Body.Text == "OK")
            {
                // starts "ok" cr lf
                _internalPort.PostUnknownType(new ATResponse(insert.Body.Text));
                return;
            }

            // starts "bat:   min 0  sec 57  mV 16894  mA 1270  deg-C 11
            // starts "key-wakeup " ... to cr lf
            // starts "processor-sleep" ... to cr lf
            // starts "do-charging" ...
            // starts "slept for "...
            // starts "battery-current-quiescent-raw "
            // starts "2005-07-14-1331-L"

        }

        /// <summary>
        /// Have we requested notifications?
        /// </summary>
        /// <returns></returns>
        private bool RequestedNotifications()
        {
            return (_state.IRobotModel == IRobotModel.Create && _streamStarted);
        }

        /// <summary>
        /// Receive Data from iRobot communications stream
        /// <remarks>This is guarded by an exclusive activation and
        /// received data packets will be processed one at a time.</remarks>
        /// </summary>
        /// <param name="insert"></param>
        private void ReceiveData(stream.ReadData insert)
        {
            BytePacket processPacket = null;
            BytePacket priorPacket = null;

            BytePacket packet = new BytePacket(insert.Body);
            bool hasNotifications = RequestedNotifications();

            // Fast Path - Try to process the incoming packet for
            // unrequested text or notifications
            int nextIx = ProcessUnrequestedPackets(packet, 0, hasNotifications);
            if (nextIx == packet.Data.Length)
                return;

            // Do we have unhandled data?
            // Retrieve the prior packet.
            if (_internalSerialBytesPending.Test(out priorPacket))
            {
                processPacket = BytePacket.Combine(priorPacket, packet);
                if (nextIx == 0)
                {
                    // As long as we haven't started processing the new packet
                    // combine it with the prior packet
                    processPacket = BytePacket.Combine(priorPacket, packet);
                    priorPacket = null;
                }
                else
                {
                    processPacket = BytePacket.Combine(priorPacket, packet.CloneTrailingBytes(nextIx));
                    priorPacket = null;
                    nextIx = 0;
                }
            }
            else
            {
                processPacket = packet;
            }

            while (nextIx < processPacket.Data.Length)
            {
                int startIx = nextIx;
                int startNotifications = -1;

                if (hasNotifications)
                {
                    // Look for Notifications anywhere in the remaining buffer
                    // startIx, startNotifications, nextIx
                    for (int ix = startIx; ix < processPacket.Data.Length - 4 ; ix++)
                    {
                        if (processPacket.Data[ix] != 19)
                            continue;

                        // Check to see if this is a notification.
                        int packetLength = IRobotUtility.ValidateNotification(processPacket.Data, ix);
                        if (packetLength > 0)
                        {
                            // find the start of the 1st notification.
                            if (startNotifications < 0)
                                startNotifications = ix;

                            nextIx = ProcessUnrequestedPackets(processPacket, ix, hasNotifications);
                            break;
                        }
                    }

                    // Did the notifications take up the entire buffer?
                    if (startNotifications == startIx && nextIx == processPacket.Data.Length)
                        return;

                }

                int packetIx;
                ExecuteIRobotCommand executeIRobotCommand = null;
                if (_internalCommandWaitingPort.Test(out executeIRobotCommand))
                {
                    bool responseWaiting = true;

                    try
                    {

                        // *************************************************
                        // If no notifications were found,
                        // check for the response packet and finish up.
                        // *************************************************
                        if (startNotifications < 0)
                        {
                            packetIx = AnalyzeBuffer(processPacket.Data, startIx, processPacket.Data.Length, processPacket.PacketTime, executeIRobotCommand);
                            if (packetIx > startIx)
                            {
                                responseWaiting = false;
                                nextIx = packetIx;
                            }

                            if (nextIx == 0)
                                _internalSerialBytesPending.Post(processPacket);
                            else if (nextIx < processPacket.Data.Length)
                                _internalSerialBytesPending.Post(processPacket.CloneTrailingBytes(nextIx));

                            return;
                        }

                        // *************************************************
                        // We had a notification and some data left over.
                        // *************************************************

                        // Do we have data after the end of notifications, and expecting a response?
                        if (responseWaiting && (nextIx < processPacket.Data.Length))
                        {
                            // Look in the second section for the response
                            packetIx = AnalyzeBuffer(processPacket.Data, nextIx, processPacket.Data.Length, processPacket.PacketTime, executeIRobotCommand);
                            if (packetIx > nextIx)
                            {
                                responseWaiting = false;
                                nextIx = packetIx;

                                // Are we done now?
                                if (startNotifications == startIx && nextIx == processPacket.Data.Length)
                                    return;

                                // go back to the top and look for more notifications
                                continue;
                            }
                        }

                        // Do we have data before the notifications and still expecting a response?
                        if (responseWaiting && (startNotifications > 0))
                        {
                            packetIx = AnalyzeBuffer(processPacket.Data, startIx, startNotifications, processPacket.PacketTime, executeIRobotCommand);
                            if (packetIx > startIx)
                            {
                                responseWaiting = false;
                                startIx = packetIx;

                                // Are we done now?
                                if (startNotifications == startIx && nextIx == processPacket.Data.Length)
                                    return;

                                // go back to the top and look for more notifications
                                continue;
                            }
                        }

                    }
                    finally
                    {
                        // If we didn't find the response, place the command back on the queue.
                        if (responseWaiting)
                        {
                            _internalCommandWaitingPort.Post(executeIRobotCommand);
                        }
                    }
                }

                // *************************************************
                // We have left over data to deal with.
                // *************************************************

                // Do we have a packet before the notifications?
                if (startIx < startNotifications)
                    priorPacket = new BytePacket(ByteArray.SubArray(processPacket.Data, startIx, startNotifications - startIx), processPacket.Sequence, processPacket.PacketTime);
                else
                    priorPacket = null;

                // Do we have a packet after the notifications?
                if (nextIx < processPacket.Data.Length)
                {
                    processPacket = processPacket.CloneTrailingBytes(nextIx);
                    nextIx = 0;
                }
                else
                {
                    processPacket = priorPacket;
                    priorPacket = null;
                    // we don't want to process any more right now.
                    nextIx = processPacket.Data.Length;
                }

                if (processPacket != null)
                {
                    if (priorPacket != null)
                    {
                        LogInfo("Dropping old response bytes: ");
                        ShowBuffer(priorPacket.Data);
                    }

                    // If we've been all the way through looking for notifications,
                    // post the bytes back to be used later.
                    if (startNotifications < 0)
                    {
                        _internalSerialBytesPending.Post(processPacket);
                        return;
                    }

                }
            }

        }

        /// <summary>
        /// Display the current buffer
        /// </summary>
        /// <param name="data"></param>
        private void ShowBuffer(byte[] data)
        {
            if (data == null)
                return;

            int counter = 0;
            StringBuilder line = new StringBuilder();
            for (int ix = 0; ix < data.Length; ix++)
            {
                line.Append(data[ix].ToString("X2"));
                line.Append(' ');
                counter++;
                if (counter == 16)
                {
                    LogInfo(line.ToString());
                    line.Length = 0;
                    counter = 0;
                }
            }
            if (line.Length > 0)
            {
                LogInfo(line.ToString());
            }
        }

        /// <summary>
        /// Process zero or more consecutive
        /// unrequested packets.
        /// </summary>
        /// <param name="processPacket"></param>
        /// <param name="startIx"></param>
        /// <param name="checkForNotifications"></param>
        /// <returns>The index of the next byte to process.
        /// 0 - packet could not be processed.
        /// processPacket.Data.Length - The entire packet was processed.
        /// </returns>
        private int ProcessUnrequestedPackets(BytePacket processPacket, int startIx, bool checkForNotifications)
        {
            bool isCreate = (_state.IRobotModel == IRobotModel.Create);
            int processAtIx = startIx;
            int nextIx;

            while (true)
            {
                if (isCreate && checkForNotifications)
                {
                    nextIx = ProcessNotification(processPacket, processAtIx);
                    if (nextIx > processAtIx)
                    {
                        processAtIx = nextIx;

                        // Is there more to process?
                        if (nextIx < processPacket.Data.Length)
                            continue;
                    }
                }

                nextIx = ProcessUnrequestedText(processPacket, processAtIx);
                if (nextIx > processAtIx)
                {
                    processAtIx = nextIx;

                    // Is there more to process?
                    if (nextIx < processPacket.Data.Length)
                        continue;
                }

                break;  // can't process any more.
            }

            return processAtIx;
        }

        /// <summary>
        /// Determine if this is a Notification.
        /// Post all pending bytes to the appropriate port.
        /// </summary>
        /// <param name="bytePacket">The data</param>
        /// <param name="startIx">The first byte to process</param>
        /// <returns></returns>
        private int ProcessNotification(BytePacket bytePacket, int startIx)
        {
            byte[] data = bytePacket.Data;
            int endIx = data.Length;

            // Check to see if this is a notification.
            int packetLength = IRobotUtility.ValidateNotification(data, startIx);
            if (packetLength > 0)
            {
                byte[] notificationData;

                if (packetLength == endIx)
                {
                    // Send the whole packet
                    notificationData = data;
                }
                else
                {
                    notificationData = ByteArray.SubArray(data, startIx, packetLength);
                }

                if (!ByteArray.IsEqual(_priorNotification, notificationData))
                {
                    _priorNotification = notificationData;
                    ReturnStream notification = new ReturnStream(notificationData, bytePacket.PacketTime);
                    if (notification != null)
                    {
                        _internalPort.PostUnknownType(new UpdateNotifications(notification));
                    }
                    else
                    {
                        LogError(LogGroups.Console, "Invalid Notification received");
                    }
                }
                else
                {
                    // Update _state.LastUpdated
                    _internalPort.PostUnknownType(bytePacket.PacketTime);
                }

                LogVerbose(LogGroups.Console, "Notification " + packetLength.ToString() + " bytes.");
                return startIx + packetLength;
            }

            // Couldn't process the data
            return startIx;
        }

        /// <summary>
        /// Determine if this is a text string.
        /// Post textto the appropriate port.
        /// </summary>
        /// <param name="bytePacket">The data</param>
        /// <param name="startIx">The first byte to process</param>
        /// <returns></returns>
        private int ProcessUnrequestedText(BytePacket bytePacket, int startIx)
        {
            byte[] data = bytePacket.Data;
            int endIx = data.Length;

            bool isString = true;
            bool lineEnd = false;
            StringBuilder text = new StringBuilder(255);
            int ix = startIx;
            while (ix < endIx)
            {
                char c = (char)data[ix++];
                if (isString && (c == '\r' || c == '\n'))
                {
                    lineEnd = true;
                }
                else if (lineEnd && (text.Length > 0))
                {
                    // we have been processing a string
                    // found one or more CR or LF
                    // but now one byte past the end of the string
                    ix--;
                    break;
                }
                else if ((c >= ' ') && c < 128)
                {
                    text.Append(c);
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

            // If the line isn't terminated, or no text was found,
            // we can't say for sure it is really text.
            if (isString && lineEnd && (text.Length > 0))
            {
                string result = text.ToString();
                _internalPort.PostUnknownType(new stream.ReadText(new stream.StreamText(result, bytePacket.PacketTime)));
                return ix;
            }

            // Couldn't process the data
            return startIx;
        }

        #region Internal Port Handlers

        /// <summary>
        /// Update iRobot Configuration
        /// </summary>
        /// <param name="configure"></param>
        private void UpdateConfigureHandler(Configure configure)
        {
            _state = configure.Body;

            // Configure without connecting
            SpawnIterator<bool>(false, ConfigureRoomba);

            configure.ResponsePort.Post(DefaultReplaceResponseType.Instance);
        }


        /// <summary>
        /// Update Connect Handler
        /// </summary>
        /// <param name="update"></param>
        private void UpdateConnectHandler(Connect update)
        {
            // Configure and connect
            SpawnIterator<bool>(true, ConfigureRoomba);

            update.ResponsePort.Post(DefaultUpdateResponseType.Instance);
        }


        /// <summary>
        /// Update state with current pose.
        /// </summary>
        /// <param name="updatePose"></param>
        private void UpdateStatePoseHandler(UpdatePose updatePose)
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
        private void UpdateStatePowerHandler(UpdatePower updatePower)
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
        private void UpdateStateSensorsHandler(UpdateBumpsCliffsAndWalls updateBumpsCliffsAndWalls)
        {
            if (updateBumpsCliffsAndWalls.Body == null || !updateBumpsCliffsAndWalls.Body.ValidPacket)
            {
                updateBumpsCliffsAndWalls.ResponsePort.Post(Fault.FromException(new ArgumentOutOfRangeException("Invalid Sensors packet")));
                return;
            }

            _state.Sensors = updateBumpsCliffsAndWalls.Body;


            // If any Roomba wheels have dropped, change the Mode to passive.
            if ((_state.Mode == RoombaMode.Safe || _state.Mode == RoombaMode.Full)
                && (_state.IRobotModel == IRobotModel.Roomba)
                && ((_state.Sensors.BumpsWheeldrops & (BumpsWheeldrops.WheelDropCaster | BumpsWheeldrops.WheelDropLeft | BumpsWheeldrops.WheelDropRight)) != 0))
            {
                _state.Mode = RoombaMode.Passive;
            }

            _state.LastUpdated = DateTime.Now;
            updateBumpsCliffsAndWalls.ResponsePort.Post(DefaultUpdateResponseType.Instance);
        }

        /// <summary>
        /// Update state with current telemetry sensors.
        /// </summary>
        /// <param name="updateTelemetry"></param>
        private void UpdateTelemetryHandler(UpdateTelemetry updateTelemetry)
        {
            if (updateTelemetry.Body == null || !updateTelemetry.Body.ValidPacket)
            {
                updateTelemetry.ResponsePort.Post(Fault.FromException(new ArgumentOutOfRangeException("Invalid Telemetry packet")));
                return;
            }

            _state.Telemetry = updateTelemetry.Body;
            _state.LastUpdated = DateTime.Now;

            if (_state.Telemetry.OIMode != RoombaMode.NotSpecified
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
        private void UpdateCliffDetailHandler(UpdateCliffDetail updateCliffDetail)
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
        private void UpdateStateAllHandler(UpdateAll updateAll)
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

                if (_state.IRobotModel == IRobotModel.Create)
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
                        if (_state.Telemetry.OIMode != RoombaMode.NotSpecified
                            && _state.Mode != _state.Telemetry.OIMode)
                        {
                            LogInfo(LogGroups.Console, "Create changed to an unexpected mode (from " + _state.Mode.ToString() + " to " + _state.Telemetry.OIMode.ToString() + ")");
                            _state.Mode = _state.Telemetry.OIMode;
                        }
                    }
                }
                // If any Roomba wheels have dropped, change the Mode to passive.
                else if ((_state.Mode == RoombaMode.Safe || _state.Mode == RoombaMode.Full)
                    && (_state.IRobotModel == IRobotModel.Roomba)
                    && ((_state.Sensors.BumpsWheeldrops & (BumpsWheeldrops.WheelDropCaster | BumpsWheeldrops.WheelDropLeft | BumpsWheeldrops.WheelDropRight)) != 0))
                {
                    _state.Mode = RoombaMode.Passive;
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
        private void UpdateModeHandler(UpdateMode updateMode)
        {
            bool sendNotification = false;

            if (updateMode.Body.IRobotModel != IRobotModel.NotSpecified
                && _state.IRobotModel != updateMode.Body.IRobotModel)
            {
                _state.LastUpdated = DateTime.Now;
                _state.IRobotModel = updateMode.Body.IRobotModel;
                if (_state.IRobotModel == IRobotModel.Create)
                {
                    ValidateState(false);
                }
            }

            if (updateMode.Body.FirmwareDate != DateTime.MinValue)
            {
                _state.LastUpdated = DateTime.Now;
                _state.FirmwareDate = updateMode.Body.FirmwareDate;
            }

            if (updateMode.Body.MaintainMode != RoombaMode.Off)
            {
                _state.MaintainMode = updateMode.Body.MaintainMode;
            }

            if (updateMode.Body.RoombaMode != RoombaMode.NotSpecified)
            {
                // If the mode changed, send a notification
                if (_state.Mode != updateMode.Body.RoombaMode)
                {
                    _state.LastUpdated = DateTime.Now;
                    sendNotification = true;
                }
                _state.Mode = updateMode.Body.RoombaMode;
            }

            if (sendNotification)
            {
                // We modify updateMode with the current state,
                // so make sure this is at the end of the handler.
                updateMode.Body.IRobotModel = _state.IRobotModel;
                updateMode.Body.MaintainMode = _state.MaintainMode;
                updateMode.Body.RoombaMode = _state.Mode;

                SendNotification<UpdateMode>(_subMgrPort, updateMode);
            }

            updateMode.ResponsePort.Post(DefaultUpdateResponseType.Instance);
        }


        /// <summary>
        /// Convert to Roomba Mode
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public static RoombaMode GetRoombaMode(byte code)
        {
            if (code < 128)
                return (RoombaMode)code;

            int mode = (int)code - 256;
            return (RoombaMode)mode;
        }

        /// <summary>
        /// Convert to Roomba Mode
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public static RoombaMode GetRoombaMode(int code)
        {
            if (code < 128)
                return (RoombaMode)code;

            return (RoombaMode)(256 - code);
        }

        /// <summary>
        /// Update state with the iRobot Create Stream Notification
        /// </summary>
        /// <param name="updateNotifications"></param>
        /// <remarks>Exclusive</remarks>
        private void UpdateStreamNotificationHandler(UpdateNotifications updateNotifications)
        {
            bool changed = false;
            bool updatePose = false;
            bool updatePower = false;
            bool updateBumps = false;
            bool updateTelemetry = false;
            bool updateCliffDetail = false;

            try
            {
                // Retrieve a list of notifications which have changed.
                Dictionary<CreateSensorPacket, int> sensors = updateNotifications.Body.RetrieveChangedValues(_state);
                foreach (CreateSensorPacket code in sensors.Keys)
                {
                    // Copy all individual sensor codes to _state
                    switch (code)
                    {
                        case CreateSensorPacket.OIMode:
                            RoombaMode mode = GetRoombaMode(sensors[code]);
                            if (mode != RoombaMode.NotSpecified
                                && (mode != _state.Telemetry.OIMode
                                || mode != _state.Mode))
                            {
                                _state.Telemetry.OIMode = mode;

                                if (mode != _state.Mode)
                                    LogInfo(LogGroups.Console, "Create changed modes from " + _state.Mode.ToString() + " to " + _state.Telemetry.OIMode.ToString());

                                _state.Mode = _state.Telemetry.OIMode;
                                updateTelemetry = true;
                                SendNotification<UpdateMode>(_subMgrPort, new UpdateMode(_state));
                            }
                            break;
                        case CreateSensorPacket.Angle:
                            // Streaming data does not update Pose.Angle
                            // updatePose = true;
                            break;
                        case CreateSensorPacket.BatteryCapacity:
                            _state.Power.Capacity = sensors[code];
                            updatePower = true;
                            break;
                        case CreateSensorPacket.BatteryCharge:
                            _state.Power.Charge = sensors[code];
                            updatePower = true;
                            break;
                        case CreateSensorPacket.BatteryTemperature:
                            _state.Power.Temperature = sensors[code];
                            updatePower = true;
                            break;
                        case CreateSensorPacket.BumpsWheelDrops:
                            _state.Sensors.BumpsWheeldrops = (BumpsWheeldrops)sensors[code];
                            updateBumps = true;
                            break;
                        case CreateSensorPacket.Buttons:
                            _state.Pose.ButtonsRoomba = (ButtonsRoomba)sensors[code];
                            updatePose = true;
                            break;
                        case CreateSensorPacket.CargoBayAnalogSignal:
                            // The Analog signal is equivalent to the Roomba Dirt Detectors.
                            _state.CliffDetail.UserAnalogInput = sensors[code];
                            updateCliffDetail = true;
                            break;
                        case CreateSensorPacket.CargoBayDigitalInputs:
                            _state.CliffDetail.UserDigitalInputs = (CargoBayDigitalInputs)sensors[code];
                            updateCliffDetail = true;
                            break;
                        case CreateSensorPacket.ChargingSourcesAvailable:
                            _state.CliffDetail.ChargingSourcesAvailable = (ChargingSourcesAvailable)sensors[code];
                            updateCliffDetail = true;
                            break;
                        case CreateSensorPacket.ChargingState:
                            _state.Power.ChargingState = (ChargingState)sensors[code];
                            updatePower = true;
                            break;
                        case CreateSensorPacket.CliffFrontLeft:
                            _state.Sensors.CliffFrontLeft = (sensors[code] != 0);
                            updateBumps = true;
                            break;
                        case CreateSensorPacket.CliffFrontLeftSignal:
                            _state.CliffDetail.CliffFrontLeftSignal = sensors[code];
                            updateCliffDetail = true;
                            break;
                        case CreateSensorPacket.CliffFrontRight:
                            _state.Sensors.CliffFrontRight = (sensors[code] != 0);
                            updateBumps = true;
                            break;
                        case CreateSensorPacket.CliffFrontRightSignal:
                            _state.CliffDetail.CliffFrontRightSignal = sensors[code];
                            updateCliffDetail = true;
                            break;
                        case CreateSensorPacket.CliffLeft:
                            _state.Sensors.CliffLeft = (sensors[code] != 0);
                            updateBumps = true;
                            break;
                        case CreateSensorPacket.CliffLeftSignal:
                            _state.CliffDetail.CliffLeftSignal = sensors[code];
                            updateCliffDetail = true;
                            break;
                        case CreateSensorPacket.CliffRight:
                            _state.Sensors.CliffRight = (sensors[code] != 0);
                            updateBumps = true;
                            break;
                        case CreateSensorPacket.CliffRightSignal:
                            _state.CliffDetail.CliffRightSignal = sensors[code];
                            updateCliffDetail = true;
                            break;
                        case CreateSensorPacket.Current:
                            _state.Power.Current = sensors[code];
                            updatePower = true;
                            break;
                        case CreateSensorPacket.Distance:
                            // Streaming data does not update Pose.Angle
                            // updatePose = true;
                            break;
                        case CreateSensorPacket.Infrared:
                            _state.Pose.RemoteControlCommand = (RemoteIR)sensors[code];
                            updatePose = true;
                            break;
                        case CreateSensorPacket.MotorOvercurrents:
                            _state.Sensors.MotorOvercurrents = (MotorOvercurrents)sensors[code];
                            updateBumps = true;
                            break;
                        case CreateSensorPacket.NumberOfStreamPackets:
                            _state.Telemetry.NumberOfStreamPackets = sensors[code];
                            updateTelemetry = true;
                            break;
                        case CreateSensorPacket.RequestedLeftVelocity:
                            _state.Telemetry.RequestedLeftVelocity = sensors[code];
                            updateTelemetry = true;
                            break;
                        case CreateSensorPacket.RequestedRadius:
                            _state.Telemetry.RequestedRadius = sensors[code];
                            updateTelemetry = true;
                            break;
                        case CreateSensorPacket.RequestedRightVelocity:
                            _state.Telemetry.RequestedRightVelocity = sensors[code];
                            updateTelemetry = true;
                            break;
                        case CreateSensorPacket.RequestedVelocity:
                            _state.Telemetry.RequestedVelocity = sensors[code];
                            updateTelemetry = true;
                            break;
                        case CreateSensorPacket.SongNumber:
                            _state.Telemetry.SongNumber = sensors[code];
                            updateTelemetry = true;
                            break;
                        case CreateSensorPacket.SongPlaying:
                            _state.Telemetry.SongPlaying = (sensors[code] != 0);
                            updateTelemetry = true;
                            break;
                        case CreateSensorPacket.Unused15:
                            _state.Sensors.DirtDetectorLeft = sensors[code];
                            updateBumps = true;
                            break;
                        case CreateSensorPacket.Unused16:
                            _state.Sensors.DirtDetectorRight = sensors[code];
                            updateBumps = true;
                            break;
                        case CreateSensorPacket.VirtualWall:
                            _state.Sensors.VirtualWall = (sensors[code] != 0);
                            updateBumps = true;
                            break;
                        case CreateSensorPacket.Voltage:
                            _state.Power.Voltage = sensors[code];
                            updatePower = true;
                            break;
                        case CreateSensorPacket.Wall:
                            _state.Sensors.Wall = (sensors[code] != 0);
                            updateBumps = true;
                            break;
                        case CreateSensorPacket.WallSignal:
                            _state.CliffDetail.WallSignal = sensors[code];
                            updateCliffDetail = true;
                            break;
                        default:
                            break;
                    }
                }

                changed = ( updatePose || updatePower || updateBumps || updateTelemetry || updateCliffDetail);
                if (changed)
                {
                    // Set last updated based on when the notification was received.
                    _state.LastUpdated = updateNotifications.Body.Timestamp;

                    // Send a notification
                    SendNotification<UpdateNotifications>(_subMgrPort, updateNotifications);

                    if (updateBumps)
                        SendNotification<UpdateBumpsCliffsAndWalls>(_subMgrPort, _state.Sensors);

                    if (updatePose)
                        SendNotification<UpdatePose>(_subMgrPort, _state.Pose);

                    if (updatePower)
                        SendNotification<UpdatePower>(_subMgrPort, _state.Power);

                    if (updateTelemetry)
                        SendNotification<UpdateTelemetry>(_subMgrPort, _state.Telemetry);

                    if (updateCliffDetail)
                        SendNotification<UpdateCliffDetail>(_subMgrPort, _state.CliffDetail);

                    if (!_streamStarted)
                        _streamStarted = true;
                }
            }
            catch (NullReferenceException)
            {
                if (_state.Telemetry == null)
                {
                    _state.Telemetry = new ReturnTelemetry();
                    _state.Telemetry.OIMode = _state.Mode;
                }
                if (_state.CliffDetail == null)
                    _state.CliffDetail = new ReturnCliffDetail();
            }
            finally
            {
                // updateNotifications.ResponsePort.Post(DefaultUpdateResponseType.Instance);
            }
        }


        /// <summary>
        /// Change to the specified Mode
        /// </summary>
        /// <param name="changeMode"></param>
        /// <returns></returns>
        private IEnumerator<ITask> ChangeToModeHandler(ChangeToMode changeMode)
        {
            bool success = false;
            bool wakeup = (_state.Mode == RoombaMode.Off)
                && (changeMode.Body.RoombaMode != RoombaMode.Off)
                && (_state.IRobotModel != IRobotModel.Create);
            if (wakeup)
            {
                LogInfo(LogGroups.Console, "Roomba was asleep.  Waking up...");
                // Wake up the iRobot
                yield return Arbiter.Choice(_internalPort.SendWakeup(),
                    delegate(DefaultSubmitResponseType wakeupResponse)
                    {
                        LogInfo(LogGroups.Console, "Roomba is awake.");
                    },
                    delegate(Fault wakeupFault) { LogError(wakeupFault); });

                // Try to move to passive mode, even if Wakeup state is inconclusive.
                yield return Arbiter.Choice(_internalPort.UpdateMode(RoombaMode.Passive),
                    delegate(DefaultUpdateResponseType modeResponse)
                    {
                        success = true;
                    },
                    delegate(Fault fault)
                    {
                        LogError("Failure updating mode after waking up the Roomba.");
                    });
            }

            RoombaCommand setModeCommand = IRobotUtility.SetModeCommand(changeMode.Body.RoombaMode, _state);
            while (setModeCommand != null)
            {
                LogInfo(LogGroups.Console, "Changing Roomba mode to: " + changeMode.Body.RoombaMode.ToString());
                success = false;
                yield return Arbiter.Choice<RoombaReturnPacket, Fault>(
                    _internalPort.ProcessAtomicCommand(setModeCommand),
                    delegate(RoombaReturnPacket responsePacket)
                    {
                        success = true;
                        LogInfo(LogGroups.Console, "Roomba mode is: " + changeMode.Body.RoombaMode.ToString());
                    },
                    delegate(Fault fault)
                    {
                        LogError("Error Changing Roomba mode to " + changeMode.Body.RoombaMode.ToString());
                        changeMode.ResponsePort.PostUnknownType(fault);

                    });

                if (!success)
                {
                    yield break;
                }

                setModeCommand = IRobotUtility.SetModeCommand(changeMode.Body.RoombaMode, _state);
            }

            changeMode.ResponsePort.Post(DefaultSubmitResponseType.Instance);
            yield break;
        }

        #endregion

        #region Roomba Operations Handlers

        /// <summary>
        /// Connect Handler
        /// </summary>
        /// <param name="update"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public virtual void ConnectHandler(Connect update)
        {
            _internalPort.PostUnknownType(update);
        }

        /// <summary>
        /// Connect with full Configuration Handler
        /// </summary>
        /// <param name="configure"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public virtual void ConfigureHandler(Configure configure)
        {
            _internalPort.PostUnknownType(configure);
        }

        /// <summary>
        /// Get Handler
        /// </summary>
        /// <param name="get"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public virtual void GetHandler(Get get)
        {
            get.ResponsePort.Post(_state);
        }

        /// <summary>
        /// General Subscription
        /// </summary>
        //[ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public IEnumerator<ITask> SubscribeHandler(Subscribe subscribe)
        {
            yield return Arbiter.Choice(
                base.SubscribeHelper(_subMgrPort, subscribe.Body, subscribe.ResponsePort),
                delegate(SuccessResult success)
                {
                    SendNotificationToTarget<UpdateMode>(subscribe.Body.Subscriber, _subMgrPort, new UpdateMode(_state));
                },
                delegate(Exception fault)
                {
                    LogError(fault);
                }
            );

            yield break;
        }


        /// <summary>
        /// Custom Drop Handler
        /// </summary>
        /// <param name="drop"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Teardown)]
        public virtual IEnumerator<ITask> DropHandler(DsspDefaultDrop drop)
        {
            string model = IRobotUtility.iRobotModelName(_state);
            LogInfo(LogGroups.Console, "Shutdown requested, Stopping " + model + " ...");
            PortSet<RoombaReturnPacket, Fault> resultPort = null;

            if (_streamPort != null && (_state.Mode == RoombaMode.Safe || _state.Mode == RoombaMode.Full))
            {
                if (_state.IRobotModel == IRobotModel.Roomba)
                {
                    LogVerbose(LogGroups.Console, "Shutdown: Turning off the Roomba ...");
                    resultPort = ExecuteIRobotCommand(new Roomba.InternalCmdPower());
                    yield return Arbiter.Choice(
                        Arbiter.Receive<RoombaReturnPacket>(false, resultPort,
                            delegate(RoombaReturnPacket startResponse)
                            {
                                LogInfo(LogGroups.Console, "Shutdown: Roomba is powered off.");
                            }),
                        Arbiter.Receive<Fault>(false, resultPort,
                            delegate(Fault fault)
                            {
                                LogError("Shutdown: Failure turning off the Roomba during shutdown.");
                            }),
                        Arbiter.Receive<DateTime>(false, TimeoutPort(5000),
                            delegate(DateTime timeout)
                            {
                                LogError("Shutdown: Timeout turning off the Roomba during shutdown.");
                            }));
                }
                else if (_state.IRobotModel == IRobotModel.Create)
                {

                    if (_streamStarted)
                    {
                        LogVerbose(LogGroups.Console, "Shutdown: Stopping Stream Data");

                        Activate(Arbiter.Choice<RoombaReturnPacket, Fault>(
                            ExecuteIRobotCommand(new CmdStreamPauseResume(false)),
                            delegate(RoombaReturnPacket responsePacket)
                            {
                                _streamPaused = true;
                                LogVerbose(LogGroups.Console, "Shutdown: Stream Stopped");
                            },
                            delegate(Fault fault)
                            {
                                LogError(fault);
                            }));
                    }

                    LogVerbose(LogGroups.Console, "Shutdown: Returning the Create to passive mode ...");
                    resultPort = ExecuteIRobotCommand(new Roomba.InternalCmdStart());
                    yield return Arbiter.Choice(
                        Arbiter.Receive<RoombaReturnPacket>(false, resultPort,
                            delegate(RoombaReturnPacket startResponse)
                            {
                                LogInfo(LogGroups.Console, "Shutdown: Create is in passive mode.");
                            }),
                        Arbiter.Receive<Fault>(false, resultPort,
                            delegate(Fault fault)
                            {
                                LogError("Shutdown: Failure returning the Create to passive mode.");
                            }),
                        Arbiter.Receive<DateTime>(false, TimeoutPort(5000),
                            delegate(DateTime timeout)
                            {
                                LogError("Shutdown: Timeout returning the Create to passive mode.");
                            }));

                    LogVerbose(LogGroups.Console, "Shutdown: Turning off the Create ...");
                    resultPort = ExecuteIRobotCommand(new Roomba.InternalCmdReset());
                    yield return Arbiter.Choice(
                        Arbiter.Receive<RoombaReturnPacket>(false, resultPort,
                            delegate(RoombaReturnPacket startResponse)
                            {
                                LogInfo(LogGroups.Console, "Shutdown: Create is shut down.");
                            }),
                        Arbiter.Receive<Fault>(false, resultPort,
                            delegate(Fault fault)
                            {
                                LogError("Shutdown: Failure shutting down the Create.");
                            }),
                        Arbiter.Receive<DateTime>(false, TimeoutPort(5000),
                            delegate(DateTime timeout)
                            {
                                LogError("Shutdown: Timeout shutting down the Create.");
                            }));


                }
            }

            yield return Arbiter.Choice(_internalPort.UpdateMode(RoombaMode.Shutdown, RoombaMode.Off),
                delegate(DefaultUpdateResponseType modeResponse) { },
                delegate(Fault fault)
                {
                    LogError("Shutdown: Failure updating mode during " + model + " shutdown.");
                });

            if (_streamPort != null)
            {
                yield return Arbiter.Choice(_streamPort.DsspDefaultDrop(),
                    delegate(DefaultDropResponseType response) { },
                    delegate(Fault fault)
                    {
                        LogError("Shutdown: Failure shutting down the " + model + " communications service.");
                    });
            }

            base.DefaultDropHandler(drop);
            yield break;
        }

        #region Roomba Direct Command Handlers

        /// <summary>
        /// Roomba Get Firmware Date
        /// </summary>
        /// <param name="getFirmwareDate"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public IEnumerator<ITask> RoombaGetFirmwareDateHandler(RoombaGetFirmwareDate getFirmwareDate)
        {
            SendCommand(getFirmwareDate.Body, getFirmwareDate.ResponsePort);
            yield break;
        }

        /// <summary>
        /// Roomba SetMode Handler
        /// </summary>
        /// <param name="roombaSetMode"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public void RoombaSetModeHandler(RoombaSetMode roombaSetMode)
        {
            bool isCreate = (_state.IRobotModel == IRobotModel.Create);
            RoombaCommand cmd = GetModeCommand(roombaSetMode, isCreate);
            SendCommand(cmd, roombaSetMode.ResponsePort);
        }


        /// <summary>
        /// Send Roomba Drive Handler
        /// </summary>
        /// <param name="update"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public void RoombaDriveHandler(RoombaDrive update)
        {
            SendCommand(update.Body, update.ResponsePort);
        }

        /// <summary>
        /// Send Roomba SetCleaningMotors Handler
        /// </summary>
        /// <param name="update"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public IEnumerator<ITask> RoombaSetCleaningMotorsHandler(RoombaSetCleaningMotors update)
        {
            SendCommand(update.Body, update.ResponsePort);
            yield break;
        }

        /// <summary>
        /// Send Roomba SetLeds Handler
        /// </summary>
        /// <param name="update"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public void RoombaSetLedsHandler(RoombaSetLeds update)
        {
            SendCommand(update.Body, update.ResponsePort);
        }

        /// <summary>
        /// Send Roomba DefineSong Handler
        /// </summary>
        /// <param name="update"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public IEnumerator<ITask> RoombaDefineSongHandler(RoombaDefineSong update)
        {
            SendCommand(update.Body, update.ResponsePort);
            yield break;
        }


        /// <summary>
        /// Send Roomba PlaySong Handler
        /// </summary>
        /// <param name="update"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public void RoombaPlaySongHandler(RoombaPlaySong update)
        {
            SendCommand(update.Body, update.ResponsePort);
        }

        /// <summary>
        /// GetSensors Handler
        /// </summary>
        /// <param name="getSensors"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public virtual void RoombaGetSensorsHandler(RoombaGetSensors getSensors)
        {
            SendCommand(getSensors.Body, getSensors.ResponsePort);
        }


        /// <summary>
        /// Send a ForceSeekingDock command to the Roomba
        /// </summary>
        /// <param name="cmdHeader"></param>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public IEnumerator<ITask> RoombaSeekDockHandler(RoombaSeekDock cmdHeader)
        {
            bool abort = false;
            StandardResponse responsePort = new StandardResponse();

            if (_state.IRobotModel == IRobotModel.Create)
            {
                // Create has a different command for Seek Dock.
                yield return (
                    Arbiter.Choice<RoombaCommandReceived, Fault>(
                        (StandardResponse)SendCommand(new CmdDemo(DemoMode.CoverAndSeekDock), responsePort),
                        delegate(RoombaCommandReceived ok) { },
                        delegate(Fault fault)
                        {
                            LogError(fault);
                            abort = true;
                            cmdHeader.ResponsePort.Post(fault);
                        }));
            }
            else
            {

                // ForceSeekingDockMode requires the Roomba to be in cleaning mode.
                // Start cleaning for one second and then send the ForceSeekingDock command.
                SendCommand(new InternalCmdStart(), null);
                yield return (
                    Arbiter.Choice<RoombaCommandReceived, Fault>(
                        (StandardResponse)SendCommand(new CmdClean(), responsePort),
                        delegate(RoombaCommandReceived ok) { },
                        delegate(Fault fault)
                        {
                            LogError(fault);
                            abort = true;
                            cmdHeader.ResponsePort.Post(fault);
                        }));

                if (abort)
                    yield break;

                SendCommand(new CmdForceSeekingDock(), null);
                yield return Arbiter.Receive(false, TimeoutPort(250), delegate(DateTime t) { });
                SendCommand(cmdHeader.Body, cmdHeader.ResponsePort);
            }
            yield break;
        }

        /// <summary>
        /// Start a cleaning cycle
        /// </summary>
        /// <param name="cmdHeader"></param>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public void RoombaStartCleaningHandler(RoombaStartCleaning cmdHeader)
        {
            SendCommand(cmdHeader.Body, cmdHeader.ResponsePort);
        }

        /// <summary>
        /// Start a spot cleaning cycle
        /// </summary>
        /// <param name="cmdHeader"></param>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public void RoombaStartSpotCleaningHandler(RoombaStartSpotCleaning cmdHeader)
        {
            SendCommand(cmdHeader.Body, cmdHeader.ResponsePort);
        }


        /// <summary>
        /// Start a max cleaning cycle
        /// </summary>
        /// <param name="cmdHeader"></param>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public void RoombaStartMaxCleaningHandler(RoombaStartMaxCleaning cmdHeader)
        {
            SendCommand(cmdHeader.Body, cmdHeader.ResponsePort);
        }

        /// <summary>
        /// Invalid Inbound Command Handler
        /// </summary>
        /// <param name="header"></param>
        /// <returns></returns>
        public virtual void BlockedCommandHandler(object header)
        {
            throw new InvalidOperationException("Outbound sensor notifications are not valid for sending requests.");
        }
        #endregion

        #region Get / HttpGet / HttpPost

        /// <summary>
        /// Http Get Handler.  Needed for XSLT transform
        /// </summary>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public virtual void HttpGetHandler(HttpGet httpGet)
        {
            HttpListenerRequest request = httpGet.Body.Context.Request;
            HttpListenerResponse response = httpGet.Body.Context.Response;

            string path = request.Url.AbsolutePath;

            HttpResponseType rsp = new HttpResponseType(HttpStatusCode.OK, _state, _transform);
            httpGet.ResponsePort.Post(rsp);
        }

        /// <summary>
        /// Http Post Handler.  Handles http form inputs
        /// </summary>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public IEnumerator<ITask> HttpPostHandler(HttpPost httpPost)
        {
            // Use helper to read form data
            ReadFormData readForm = new ReadFormData(httpPost);
            _httpUtilities.Post(readForm);

            RoombaState config = new RoombaState();

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
                            config.ConnectionType = (iRobotConnectionType)Enum.Parse(typeof(iRobotConnectionType), connectionTypeParm, true);
                        }
                        catch
                        {
                            config.ConnectionType = _state.ConnectionType;
                        }

                        try
                        {
                            config.IRobotModel = (IRobotModel)Enum.Parse(typeof(IRobotModel), parameters["IRobotModel"].ToString(), true);
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
                            config.MaintainMode = (RoombaMode)Enum.Parse(typeof(RoombaMode), parameters["MaintainMode"].ToString().Replace(" ", string.Empty), true);
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

            Configure configure = new Configure(config);
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
        /// Send Http Post Success Response
        /// </summary>
        /// <param name="httpPost"></param>
        /// <param name="state"></param>
        /// <param name="transform"></param>
        private static void HttpPostSuccess(HttpPost httpPost, RoombaState state, string transform)
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
        #endregion

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
            if (_state.Mode == RoombaMode.Shutdown)
                return;

            // ignore timer if Roomba is not initialized.
            if (!_state.IsInitialized)
            {
                WaitForNextTimer();
                return;
            }

            // If we are maintaining mode and have been in the wrong mode for > 1 second
            // then attempt to set the mode back to our maintained mode.
            if ((_state.MaintainMode == RoombaMode.Safe || _state.MaintainMode == RoombaMode.Full)
                && _state.MaintainMode != _state.Mode)
            {
                int waitForModeChangeMs = Math.Max(1000, _tickInterval);
                if (_startedInvalidMode == DateTime.MaxValue)
                {
                    _startedInvalidMode = DateTime.Now;
                }
                else if (((TimeSpan)DateTime.Now.Subtract(_startedInvalidMode)).TotalMilliseconds >= waitForModeChangeMs)
                {
                    LogInfo(LogGroups.Console, "Maintain Mode (automatic mode switch from " + _state.Mode.ToString() + " to " + _state.MaintainMode.ToString() + ").");
                    ChangeToMode changeMode = new ChangeToMode(_state.MaintainMode);
                    _internalPort.PostUnknownType(changeMode);

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

            RoombaResponsePort response;
            // Has the system been unresponsive?
            if (_streamStarted && ((TimeSpan)DateTime.Now.Subtract(_state.LastUpdated)).TotalMilliseconds > 2000)
            {
                LogVerbose(LogGroups.Console, "System may be unresponsive.");

                if (_state.IRobotModel == IRobotModel.Create)
                {
                    LogVerbose(LogGroups.Console, "TimerHandler: Resume Stream");
                    if (!_streamPaused)
                        _streamPaused = true;

                    Activate( Arbiter.Choice<RoombaReturnPacket, Fault>(
                        ExecuteIRobotCommand(new CmdStreamPauseResume(true)),
                        delegate(RoombaReturnPacket responsePacket)
                        {
                            _streamPaused = false;
                        },
                        delegate(Fault fault)
                        {
                            LogError(fault);
                        }));
                    LogVerbose(LogGroups.Console, "TimerHandler: Resume Stream completed");

                    if (((TimeSpan)DateTime.Now.Subtract(_state.LastUpdated)).TotalMilliseconds > 5000)
                    {
                        _streamStarted = false;
                    }

                }
            }

            // Is Polling turned off?
            if (_state.PollingInterval < 0)
            {
                WaitForNextTimer();
                return;
            }

            LogVerbose(LogGroups.Console, "Timer: Starting query");
            // Get Sensors
            if (_state.IRobotModel == IRobotModel.Create)
            {
                response = SendRoombaCommand(new CmdSensors(RoombaQueryType.ReturnAllCreate));
            }
            else
            {
                response = SendRoombaCommand(new CmdSensors(RoombaQueryType.ReturnAll));
            }

            Activate( Arbiter.Choice(
                Arbiter.Receive<ReturnAll>(false, response,
                    delegate(ReturnAll rsp)
                    {
                        WaitForNextTimer();
                    }),
                Arbiter.Receive<Fault>(false, response,
                    delegate(Fault f)
                    {
                        string reason = (f.Reason != null && f.Reason.Length >= 1) ? f.Reason[0].Value : "No sensor data received from the Roomba";
                        LogError(reason);
                        WaitForNextTimer();
                    }),
                Arbiter.Receive<DateTime>(false, TimeoutPort(5000),
                delegate(DateTime timeout)
                {
                    LogError("Timer expired on periodic sensor query.  Restarting...");
                    WaitForNextTimer();
                })));
        }

        /// <summary>
        /// Wait for _tickInterval before calling the TimerHandler
        /// </summary>
        private void WaitForNextTimer()
        {
            // Stop the timer if we are shutting down.
            if (_state.Mode == RoombaMode.Shutdown)
                return;

            // Find the next scheduled time
            _nextTimer = _nextTimer.AddMilliseconds(_tickInterval);

            // grab a moment in time
            DateTime now = DateTime.Now;
            int waitMs = (int)((TimeSpan)_nextTimer.Subtract(now)).TotalMilliseconds;

            // If it's already past time to run, execute the handler ASAP.
            if (waitMs < 10)
                waitMs = 1;

            Activate(Arbiter.Receive(false, TimeoutPort(waitMs), TimerHandler));

        }

        #endregion

        #region Create Operations Handlers

        /// <summary>
        /// Create Demo Command Handler
        /// </summary>
        /// <param name="cmdHeader"></param>
        //[ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public void CreateDemoHandler(CreateDemo cmdHeader)
        {
            SendCommand(cmdHeader.Body, cmdHeader.ResponsePort);
        }

        /// <summary>
        /// Create PWMLowSideDrivers Command Handler
        /// </summary>
        /// <param name="cmdHeader"></param>
        //[ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public void CreatePWMLowSideDriversHandler(CreatePWMLowSideDrivers cmdHeader)
        {
            SendCommand(cmdHeader.Body, cmdHeader.ResponsePort);
        }


        /// <summary>
        /// Drive with Left and Right Velocity
        /// </summary>
        /// <remarks>Works with both Roomba and Create</remarks>
        /// <param name="cmdHeader"></param>
        //[ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public void CreateDriveDirectHandler(CreateDriveDirect cmdHeader)
        {
            RoombaCommand cmd = cmdHeader.Body;

            if (_state.IRobotModel == IRobotModel.Roomba)
            {
                double normalizedLeft = (double)(cmdHeader.Body.LeftVelocity) / 500.0;
                double normalizedRight = (double)(cmdHeader.Body.RightVelocity) / 500.0;
                cmd = IRobotUtility.ConvertToDrive(normalizedLeft, normalizedRight);
            }

            SendCommand(cmd, cmdHeader.ResponsePort);
        }


        /// <summary>
        /// Create CreateDigitalOutputs Command Handler
        /// </summary>
        /// <param name="cmdHeader"></param>
        //[ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public void CreateDigitalOutputsHandler(CreateDigitalOutputs cmdHeader)
        {
            SendCommand(cmdHeader.Body, cmdHeader.ResponsePort);
        }

        /// <summary>
        /// Create CreateStream Command Handler
        /// </summary>
        /// <param name="cmdHeader"></param>
        //[ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public void CreateStreamHandler(CreateStream cmdHeader)
        {
            SendCommand(cmdHeader.Body, cmdHeader.ResponsePort);
        }

        /// <summary>
        /// Create CreateQueryList Command Handler
        /// </summary>
        /// <param name="cmdHeader"></param>
        //[ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public void CreateQueryListHandler(CreateQueryList cmdHeader)
        {
            SendCommand(cmdHeader.Body, cmdHeader.ResponsePort);
        }

        /// <summary>
        /// Create CreateStreamPauseResume Command Handler
        /// </summary>
        /// <param name="cmdHeader"></param>
        //[ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public void CreateStreamPauseResumeHandler(CreateStreamPauseResume cmdHeader)
        {
            SendCommand(cmdHeader.Body, cmdHeader.ResponsePort);
        }

        /// <summary>
        /// Create CreateSendIR Command Handler
        /// </summary>
        /// <param name="cmdHeader"></param>
        //[ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public void CreateSendIRHandler(CreateSendIR cmdHeader)
        {
            SendCommand(cmdHeader.Body, cmdHeader.ResponsePort);
        }

        /// <summary>
        /// Create CreateDefineScript Command Handler
        /// </summary>
        /// <param name="cmdHeader"></param>
        //[ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public void CreateDefineScriptHandler(CreateDefineScript cmdHeader)
        {
            SendCommand(cmdHeader.Body, cmdHeader.ResponsePort);
        }

        /// <summary>
        /// Create CreatePlayScript Command Handler
        /// </summary>
        /// <param name="cmdHeader"></param>
        //[ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public void CreatePlayScriptHandler(CreatePlayScript cmdHeader)
        {
            SendCommand(cmdHeader.Body, cmdHeader.ResponsePort);
        }

        /// <summary>
        /// Create CreateShowScript Command Handler
        /// </summary>
        /// <param name="cmdHeader"></param>
        //[ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public void CreateShowScriptHandler(CreateShowScript cmdHeader)
        {
            SendCommand(cmdHeader.Body, cmdHeader.ResponsePort);
        }

        /// <summary>
        /// Create CreateWaitTime Command Handler
        /// </summary>
        /// <param name="cmdHeader"></param>
        //[ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public void CreateWaitTimeHandler(CreateWaitTime cmdHeader)
        {
            SendCommand(cmdHeader.Body, cmdHeader.ResponsePort);
        }

        /// <summary>
        /// Create CreateWaitDistance Command Handler
        /// </summary>
        /// <param name="cmdHeader"></param>
        //[ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public void CreateWaitDistanceHandler(CreateWaitDistance cmdHeader)
        {
            SendCommand(cmdHeader.Body, cmdHeader.ResponsePort);
        }

        /// <summary>
        /// Create CreateWaitAngle Command Handler
        /// </summary>
        /// <param name="cmdHeader"></param>
        //[ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public void CreateWaitAngleHandler(CreateWaitAngle cmdHeader)
        {
            SendCommand(cmdHeader.Body, cmdHeader.ResponsePort);
        }

        /// <summary>
        /// Create CreateWaitEvent Command Handler
        /// </summary>
        /// <param name="cmdHeader"></param>
        //[ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public void CreateWaitEventHandler(CreateWaitEvent cmdHeader)
        {
            SendCommand(cmdHeader.Body, cmdHeader.ResponsePort);
        }

        #endregion


        /// <summary>
        /// Configure the Roomba
        /// </summary>
        /// <param name="connect"></param>
        /// <returns></returns>
        IEnumerator<ITask> ConfigureRoomba(bool connect)
        {
            ValidateState(connect);
            _state.FirmwareDate = DateTime.MinValue;

            string model = IRobotUtility.iRobotModelName(_state);
            int errorCount = 0;
            DateTime firmwareDate = DateTime.MinValue;

            // If no stream port was specified,
            // start the default iRobot Stream service.
            if (_streamPort == null)
            {
                // Start the default implementation of the iRobot stream service
                ServiceInfoType info = new ServiceInfoType(istream.Contract.Identifier);
                cons.Create create = new cons.Create(info);
                create.TimeSpan = DsspOperation.DefaultShortTimeSpan;

                ConstructorPort.Post(create);
                yield return Arbiter.Choice(
                    create.ResponsePort,
                    delegate(CreateResponse createResponse)
                    {
                        _streamPort = ServiceForwarder<stream.StreamOperations>(createResponse.Service);
                    },
                    delegate(Fault f)
                    {
                        LogError(f);
                        this.Shutdown();
                    }
                );
            }

            if (!_directoryInserted)
            {
                // Publish the service to the local Node Directory
                DirectoryInsert();

                // display HTTP service Uri
                LogVerbose(LogGroups.Console, "Service uri: ");

                _directoryInserted = true;
            }

            yield return Arbiter.Choice(_internalPort.UpdateMode(RoombaMode.Uninitialized, RoombaMode.Off),
                delegate(DefaultUpdateResponseType response) { },
                delegate(Fault fault) { LogError(fault); });

            _state.BaudRate = IRobotUtility.GetDefaultBaudRate(_state.ConnectionType, _state.BaudRate);

            if (_streamStarted)
            {
                LogInfo(LogGroups.Console, "Reconnecting to the iRobot Create...");
                _streamStarted = false;
            }

            bool connected = false;
            if (connect)
            {
                yield return Arbiter.Choice(ConfigureIRobotStream(_state),
                    delegate(stream.ReplaceStreamResponse response) { connected = response.Connected; },
                    delegate(Fault fault) { LogError(fault); });
            }

            // Subscribe to iRobot Communications,
            // even if we are not connected.
            SubscribeToStream();

            if (!connected || !IRobotUtility.ValidState(_state))
            {
                _state.Sensors = null;
                _state.Pose = null;
                _state.Power = null;
                _state.Telemetry = null;
                _state.CliffDetail = null;

                BrowseToThisService();
                yield break;
            }

            // *****************************************************
            // Only for Roomba.
            // Toggle pin 5 on the serial port to cause the Roomba to wake up.
            if (_state.IRobotModel == IRobotModel.Roomba)
            {
                // Wake up the Roomba
                RoombaMode mode = _state.Mode;
                yield return Arbiter.Choice(_internalPort.SendWakeup(),
                    delegate(DefaultSubmitResponseType wakeupResponse)
                    {
                        LogInfo("Roomba is awake.");
                        mode = RoombaMode.Passive;
                    },
                    delegate(Fault wakeupFault) { LogError(wakeupFault); });

                if (mode != _state.Mode)
                {
                    yield return Arbiter.Choice(_internalPort.UpdateMode(mode),
                        delegate(DefaultUpdateResponseType response) { },
                        delegate(Fault fault) { LogError(fault); });
                }

            }
            // *****************************************************

            LogInfo("Connecting to " + model + " ...");
            RoombaResponsePort responsePort = new RoombaResponsePort();

            bool success = false;
            // Send Start (Place the Roomba in Passive mode)
            while (!success && errorCount < 10)
            {
                responsePort = SendRoombaCommand(new InternalCmdStart());
                yield return Arbiter.Choice(
                    Arbiter.Receive<RoombaCommandReceived>(false, responsePort,
                        delegate(RoombaCommandReceived receivedStart)
                        {
                            success = true;
                            errorCount = 0;
                        }),
                    Arbiter.Receive<Fault>(false, responsePort,
                        delegate(Fault fault)
                        {
                            LogError(fault);
                            errorCount++;
                        }));

                // If not successful yet, pause before retry.
                if (!success)
                    yield return Arbiter.Receive(false, TimeoutPort(500), delegate(DateTime timeout) { });
            }

            // Get the Firmware Date.
            responsePort = SendRoombaCommand(new CmdFirmwareDate());
            yield return Arbiter.Choice(
                Arbiter.Receive<ReturnFirmwareDate>(false, responsePort,
                    delegate(ReturnFirmwareDate response)
                    {
                        firmwareDate = response.FirmwareDate;
                    }),
                Arbiter.Receive<Fault>(false, responsePort,
                    delegate(Fault fault)
                    {
                        LogError(fault);
                        errorCount++;
                    }));

            // Put Roomba in Control (Safe) mode
            responsePort = SendRoombaCommand(new InternalCmdControl());
            yield return Arbiter.Choice(
                Arbiter.Receive<RoombaCommandReceived>(false, responsePort,
                    delegate(RoombaCommandReceived receivedStart) { }),
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
                _state.SongDefinitions = new List<CmdDefineSong>();
            }
            else
            {
                foreach (CmdDefineSong cmdDefineSong in _state.SongDefinitions)
                {
                    if (cmdDefineSong.SongNumber == 1)
                        song1Defined = true;
                    if (cmdDefineSong.SongNumber == 2)
                        song2Defined = true;
                    SendRoombaCommand(cmdDefineSong);
                }
            }
            CmdDefineSong song;
            if (!song1Defined)
            {
                song = IRobotUtility.DefineSimpleSong(1);
                SendRoombaCommand(song);
                _state.SongDefinitions.Add(song);
            }

            if (!song2Defined)
            {
                song = IRobotUtility.DefinePlayfulSong(2);
                SendRoombaCommand(song);
                _state.SongDefinitions.Add(song);
            }

            // Play the song
            LogInfo(LogGroups.Console, "Playing a song");
            SendRoombaCommand(new CmdPlaySong(1));

            IRobotModel newRobotModel = _state.IRobotModel;
            bool identifyModel = (_state.IRobotModel == IRobotModel.NotSpecified);

            // Read the sensors one time.
            LogInfo(LogGroups.Console, "Reading the sensors");
            responsePort = SendRoombaCommand(new CmdSensors(RoombaQueryType.ReturnAll));
            yield return Arbiter.Choice(
                Arbiter.Receive<ReturnAll>(false, responsePort, delegate(ReturnAll rsp)
                {
                    LogInfo(LogGroups.Console, "Success reading standard sensor data");
                    if (identifyModel)
                        newRobotModel = IRobotModel.Roomba;
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
                BrowseToThisService();
                yield break;
            }

            // A graceful check for Create which won't break the Roomba.
            if (identifyModel || _state.IRobotModel == IRobotModel.Create)
            {
                responsePort = SendRoombaCommand(new CmdSensors(RoombaQueryType.ReturnAllCreate));
                yield return Arbiter.Choice(
                    Arbiter.Receive<ReturnAll>(false, responsePort, delegate(ReturnAll rsp)
                    {
                        LogInfo(LogGroups.Console, "Identified iRobot model as Create");
                        if (identifyModel)
                            newRobotModel = IRobotModel.Create;
                    }),
                    Arbiter.Receive<Fault>(false, responsePort, delegate(Fault f)
                    {
                        if (_state.IRobotModel == IRobotModel.Create)
                            LogError(LogGroups.Console, "Unable to retrieve Create specific sensor data.\r\n"
                                + "Please reset your connection and try again.\r\n"
                                + "If you are connecting to a Roomba Discovery,\r\n"
                                + "please update your configuration file per the Roomba Readme.");
                        else
                            LogInfo(LogGroups.Console, "Identified iRobot model as Roomba");
                    }));

            }

            if ((firmwareDate != DateTime.MinValue && firmwareDate != _state.FirmwareDate)
                || (identifyModel && (newRobotModel != IRobotModel.NotSpecified)))
            {
                // We just changed the mode or firmware date, update _state!
                yield return Arbiter.Choice(_internalPort.UpdateMode(newRobotModel, firmwareDate),
                    delegate(DefaultUpdateResponseType modeResponse) { },
                    delegate(Fault fault) { LogError(fault); });
            }

            int timerInterval;
            bool hasStreamingNotifications = false;
            if (_state.IRobotModel == IRobotModel.Create)
            {
                // Request continuous sensor data
                CmdStream cmdStream = null;

                if (_state.CreateNotifications != null && _state.CreateNotifications.Count > 0)
                {
                    hasStreamingNotifications = true;

                    // Request continuous sensor data
                    cmdStream = new CmdStream(_state.CreateNotifications);

                    // Rootooth has size limitiations on Notifications.
                    if (_state.ConnectionType == iRobotConnectionType.RooTooth)
                    {
                        int notificationSize = IRobotUtility.ReturnNotificationSize(_state.CreateNotifications);
                        if (notificationSize > 12)
                        {
                            LogError(LogGroups.Console,
                                "Rootooth has limitations on the size of the returned Notification packet.\r\n"
                                + "Please reduce the number of notifications being requested,\r\n"
                                + "otherwise commands sent to the iRobot Create may not be responsive.");
                        }
                    }

                }
                else
                {
                    // Request no sensor stream data
                    // This is necessary if the Create was already
                    // streaming data when our service connected.
                    cmdStream = new CmdStream();
                }

                responsePort = SendRoombaCommand(cmdStream);
                yield return Arbiter.Choice(
                    Arbiter.Receive<RoombaCommandReceived>(false, responsePort,
                        delegate(RoombaCommandReceived ackStream)
                        {
                            _streamStarted = ((_state.CreateNotifications != null) && (_state.CreateNotifications.Count > 0));
                        }),
                    Arbiter.Receive<Fault>(false, responsePort,
                        delegate(Fault fault)
                        {
                            _streamStarted = false;
                            LogError(fault);
                        }));
            }

            // calculate the polling frequency
            if (_state.PollingInterval <= 0) // use default
                timerInterval = (hasStreamingNotifications) ? _defaultPollingWithStreaming : _defaultPollingInterval;
            else
                timerInterval = _state.PollingInterval;


            // for wireless connections, we should limit the minimum polling frequency.
            if ((timerInterval > 0 && timerInterval < _minWirelessInterval)
                && (_state.ConnectionType == iRobotConnectionType.BluetoothAdapterModule
                    || _state.ConnectionType == iRobotConnectionType.RooTooth))
            {
                timerInterval = _minWirelessInterval;
            }

            StartTimer(timerInterval);
            yield break;
        }

        /// <summary>
        /// Start up the web browser so user can configure
        /// </summary>
        private void BrowseToThisService()
        {
            lock (this)
            {
                if (_browsedToService)
                    return;

                _browsedToService = true;
            }

#if NET_CF
            LogInfo(LogGroups.Console, "Please set up a valid configuration file to initialize your iRobot");
#else
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            process.StartInfo.FileName = FindServiceAliasFromScheme(Uri.UriSchemeHttp);
            process.Start();
#endif
        }

        /// <summary>
        /// Send a Roomba command and wait for the reply.
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        private RoombaResponsePort SendRoombaCommand(RoombaCommand cmd)
        {
            return (RoombaResponsePort)SendCommand(cmd, null);
        }


        /// <summary>
        /// Send a command to the iRobot and post the response to responsePort.
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="responsePort"></param>
        /// <returns></returns>
        public IPortSet SendCommand(RoombaCommand cmd, IPortSet responsePort)
        {
            if (responsePort == null)
                responsePort = new RoombaResponsePort();

            SpawnIterator<RoombaCommand, IPortSet>(cmd, responsePort, ProcessCommand);
            return responsePort;
        }

        #region Wake up Roomba

        /// <summary>
        /// Wake up the iRobot
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        private IEnumerator<ITask> WakeupRoombaHandler(WakeupRoomba cmd)
        {
            if (_state.ConnectionType == iRobotConnectionType.NotConfigured)
            {
                cmd.ResponsePort.Post(Fault.FromException(new InvalidOperationException("Invalid iRobotConnectionType.")));
                yield break;
            }

            if (_state.IRobotModel == IRobotModel.Create)
            {
                // Create does not support wakeup.
                // Try to set the Create to Passive mode.
                ChangeToMode changeMode = new ChangeToMode(RoombaMode.Passive);
                bool success = true;
                if (_state.Mode != RoombaMode.Passive)
                {
                    LogInfo(LogGroups.Console, "Set Mode from " + _state.Mode.ToString() + " to Passive.");
                    _internalPort.PostUnknownType(changeMode);
                    yield return Arbiter.Choice(changeMode.ResponsePort,
                        delegate(DefaultSubmitResponseType changeResponse) { },
                        delegate(Fault fault)
                        {
                            success = false;
                            cmd.ResponsePort.Post(Fault.FromException(new InvalidOperationException("Invalid iRobotConnectionType.")));
                        });

                }
                if (success)
                    cmd.ResponsePort.Post(DefaultSubmitResponseType.Instance);

                yield break;
            }

            LogInfo(LogGroups.Console, "Wake up iRobot");
            Port<bool> successPort = new Port<bool>();
            switch (_state.ConnectionType)
            {
                case iRobotConnectionType.RooTooth:
                    SpawnIterator<Port<bool>>(successPort, WakeUpRooTooth);
                    break;
                case iRobotConnectionType.RoombaSerialPort:
                    SpawnIterator<Port<bool>>(successPort, WakeUpRS232);
                    break;
                case iRobotConnectionType.CreateSerialPort:
                    successPort.Post(true);
                    break;
                case iRobotConnectionType.BluetoothAdapterModule:
                    LogError("The Roomba does not work with the Create Bluetooth Adapter Module!");
                    successPort.Post(false);
                    break;
                default:
                    LogError("The Roomba connection has not been configured");
                    successPort.Post(false);
                    break;
            }

            bool awake = false;
            yield return Arbiter.Receive(false, successPort,
                delegate(bool success)
                {
                    awake = success;
                });

            if (awake)
            {
                cmd.ResponsePort.Post(DefaultSubmitResponseType.Instance);
                yield break;
            }

            cmd.ResponsePort.Post(Fault.FromException(new ApplicationException("Failed to wake up the iRobot.")));
            yield break;
        }


        /// <summary>
        /// Wake up an iRobot when connected to the RooTooth
        /// Bluetooth adapter.
        /// </summary>
        /// <param name="responsePort"></param>
        /// <returns></returns>
        private IEnumerator<ITask> WakeUpRooTooth(Port<bool> responsePort)
        {
            bool ok = false;
            int tryCount = 0;

            yield return Arbiter.Receive(false, SendATCommand("+++\r", false),
                delegate(bool success)
                {
                    ok |= success;
                });

            yield return Arbiter.Receive(false, TimeoutPort(10), delegate(DateTime timeout) { });
            if (ok)
            {
                ok = false;
                tryCount = 0;
                while (!ok && tryCount++ < 5)
                {
                    yield return Arbiter.Receive(false, SendATCommand("ATSW22,6,1,1\r", true),
                        delegate(bool success)
                        {
                            ok = success;
                        });

                    yield return Arbiter.Receive(false, TimeoutPort(200), delegate(DateTime timeout) { });
                }
            }

            if (ok)
            {
                ok = false;
                tryCount = 0;
                while (!ok && tryCount++ < 5)
                {
                    yield return Arbiter.Receive(false, SendATCommand("ATSW23,6,0,1\r", true),
                        delegate(bool success)
                        {
                            ok = success;
                        });

                    yield return Arbiter.Receive(false, TimeoutPort(200), delegate(DateTime timeout) { });
                }
            }

            if (ok)
            {
                ok = false;
                tryCount = 0;
                while (!ok && tryCount++ < 5)
                {

                    yield return Arbiter.Receive(false, SendATCommand("ATSW23,6,1,1\r", true),
                        delegate(bool success)
                        {
                            ok = success;
                        });

                    yield return Arbiter.Receive(false, TimeoutPort(100), delegate(DateTime timeout) { });
                }
            }

            if (ok)
            {
                ok = false;
                tryCount = 0;
                while (!ok && tryCount++ < 5)
                {
                    yield return Arbiter.Receive(false, SendATCommand("ATMD\r", true),
                        delegate(bool success)
                        {
                            ok = success;
                        });

                    yield return Arbiter.Receive(false, TimeoutPort(100), delegate(DateTime timeout) { });

                }
            }

            if (ok)
            {
                LogInfo(LogGroups.Console, "iRobot connection is established");
            }
            else
            {
                LogInfo(LogGroups.Console, "Rootooth is silent.\r\n"
                + "    Please make sure your Roomba is charged,\r\n"
                + "    the Rootooth is plugged in,\r\n"
                + "    and the Roomba has been paired to your PC.\r\n");
            }

            responsePort.Post(ok);
            yield break;
        }


        /// <summary>
        /// Wake up the iRobot when connected directly to a serial port
        /// </summary>
        /// <returns></returns>
        private IEnumerator<ITask> WakeUpRS232(Port<bool> responsePort)
        {
            bool abort = false;

            // Set RtsEnable = false/true/false
            yield return Arbiter.Choice(SetWakeUpRooma(),
                delegate(DefaultUpdateResponseType ok) { },
                delegate(Fault fault)
                {
                    LogError(fault);
                    abort = true;
                });

            if (abort)
            {
                responsePort.Post(false);
                yield break;
            }

            responsePort.Post(true);
            yield break;
        }

        #endregion

        /// <summary>
        /// Request that the stream connection wake up Roomba directly.
        /// </summary>
        /// <returns></returns>
        private PortSet<DefaultUpdateResponseType, Fault> SetWakeUpRooma()
        {
            return _streamPort.SetStreamProperty(new stream.NameValuePair("WakeupRoomba", string.Empty));
        }

        /// <summary>
        /// Send an AT command and wait for an AT Response
        /// </summary>
        /// <param name="command"></param>
        /// <param name="requireResponse"></param>
        /// <returns></returns>
        private Port<bool> SendATCommand(string command, bool requireResponse)
        {
            Port<bool> ATResponsePort = new Port<bool>();

            // First send the text to the communication stream
            PortSet<DefaultSubmitResponseType, Fault> streamResponsePort = _streamPort.WriteText(new stream.StreamText(command, DateTime.Now));

            // Wait for a response from the write.  This is different
            // than a response from the command itself.
            Activate(Arbiter.Choice(
                Arbiter.Receive<DefaultSubmitResponseType>(false, streamResponsePort,
                    delegate(DefaultSubmitResponseType response)
                    {
                        if (requireResponse)
                        {
                            // The write was successful.  Now wait
                            // on the internal port for an AT Response,
                            // or timeout.
                            Activate(Arbiter.Choice(
                            Arbiter.Receive<ATResponse>(false, _internalPort,
                                delegate(ATResponse success)
                                {
                                    ATResponsePort.Post(true);
                                }),
                            Arbiter.Receive<DateTime>(false, TimeoutPort(2000),
                                delegate(DateTime timeout)
                                {
                                    LogError(LogGroups.Console, new TimeoutException("Timeout receiving AT Command response from iRobot"));
                                    ATResponsePort.Post(false);
                                })));
                        }
                        else  // no response required.  We are done.
                        {
                            ATResponsePort.Post(true);
                        }
                    }),
                Arbiter.Receive<Fault>(false, streamResponsePort,
                    delegate(Fault fault)
                    {
                        LogError(fault);
                        ATResponsePort.Post(false);
                    }),
                Arbiter.Receive<DateTime>(false, TimeoutPort(5000),
                    delegate(DateTime timeout)
                    {
                        LogError(new TimeoutException("Timeout sending AT Command to iRobot"));
                        ATResponsePort.Post(false);
                    })));

            return ATResponsePort;
        }


        /// <summary>
        /// Post ExecuteIRobotCommand and return the response port.
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        private PortSet<RoombaReturnPacket, Fault> ExecuteIRobotCommand(RoombaCommand cmd)
        {
            ExecuteIRobotCommand op = new ExecuteIRobotCommand(cmd);
            if (cmd.ExpectedResponseBytes() > 0)
                _exclusiveCommandPort.Post(op);
            else
                _internalPort.PostUnknownType(op);

            return op.ResponsePort;
        }


        /// <summary>
        /// Execute an iRobot Command which does not expect a response.
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        private IEnumerator<ITask> ExecuteIRobotNoAckHandler(ExecuteIRobotCommand cmd)
        {
            LogVerbose(LogGroups.Console, "    ExecuteIRobotNoAckHandler started for " + cmd.Body.RoombaCommandCode.ToString());
            // If the command expects a response, we don't belong here.
            if (cmd.Body.ExpectedResponseBytes() > 0)
            {
                // Return an error
                string errorMessage = "Attempted to execute a query without waiting for response bytes.";
                LogError(errorMessage);
                cmd.ResponsePort.Post(Fault.FromException(new TimeoutException(errorMessage)));
                LogVerbose(LogGroups.Console, "    ExecuteIRobotNoAckHandler ABORTED for " + cmd.Body.RoombaCommandCode.ToString());
                yield break;
            }

            Fault writeFault = null;
            PortSet<DefaultSubmitResponseType, Fault> writeResponse = _streamPort.WriteData(new stream.StreamData(cmd.Body.GetPacket(), DateTime.Now));
            yield return Arbiter.Choice(
                Arbiter.Receive<DefaultSubmitResponseType>(false, writeResponse,
                    delegate(DefaultSubmitResponseType submitResponse) { }),
                Arbiter.Receive<Fault>(false, writeResponse,
                    delegate(Fault fault)
                    {
                        writeFault = fault;
                    }),
                Arbiter.Receive(false, TimeoutPort(1000),
                    delegate(DateTime timeout)
                    {
                        string errorMessage = "Timeout sending the " + cmd.Body.RoombaCommandCode.ToString() + " request to the iRobot.";
                        LogError(errorMessage);
                        writeFault = Fault.FromException(new TimeoutException(errorMessage));
                    }));

            if (writeFault != null)
            {
                // return failure
                cmd.ResponsePort.Post(writeFault);
                LogVerbose(LogGroups.Console, "    ExecuteIRobotNoAckHandler ABORTED for " + cmd.Body.RoombaCommandCode.ToString());
            }
            else
            {
                // command was sent successfully
                if (cmd.Body.CmdTimeoutMs > 0)
                {
                    // Wait for the specified timeout before sending a response.
                    yield return Arbiter.Receive(false, TimeoutPort(cmd.Body.CmdTimeoutMs), delegate(DateTime timeout) { });
                }

                // Generate a success response
                RoombaReturnPacket response;
                switch(cmd.Body.RoombaCommandCode)
                {
                    case RoombaCommandCode.DefineScript:
                        response = new ReturnDefineScript(((CmdDefineScript)cmd.Body).ScriptResponseBytes);
                        break;
                    default:
                        response = new RoombaCommandReceived(cmd.Body.RoombaCommandCode);
                        break;
                }

                cmd.ResponsePort.Post(response);
                LogVerbose(LogGroups.Console, "    ExecuteIRobotNoAckHandler SUCCESS for " + cmd.Body.RoombaCommandCode.ToString());
            }

            yield break;
        }

        /// <summary>
        /// Execute an iRobot Command
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        public virtual IEnumerator<ITask> ExecuteIRobotWithResponseHandler(ExecuteIRobotCommand cmd)
        {
            // We expect data to be returned by this command.
            // If the command does not return data, we are in the wrong handler.
            if (cmd.Body.ExpectedResponseBytes() == 0)
            {
                // Return an error
                string errorMessage = "Attempted to execute a command/response handler for a command which does not require a response.";
                LogError(errorMessage);
                cmd.ResponsePort.Post(Fault.FromException(new TimeoutException(errorMessage)));
                yield break;
            }

            // Save the real response so we can respond to it ourself
            PortSet<RoombaReturnPacket, Fault> delayedResponsePort = cmd.ResponsePort;

            // Set up a temporary response port for the internal command waiting port
            cmd.ResponsePort = new PortSet<RoombaReturnPacket, Fault>();

            _internalSerialBytesPending.Clear();

            // Signal that we have a new command which expects a response
            _internalCommandWaitingPort.Post(cmd);

            int commandTimeout = cmd.Body.CmdTimeoutMs;
            if (commandTimeout <= 0) commandTimeout = 1000;

            Fault writeFault = null;
            PortSet<DefaultSubmitResponseType, Fault> writeResponse = _streamPort.WriteData(new stream.StreamData(cmd.Body.GetPacket(), DateTime.Now));
            yield return Arbiter.Choice(
                Arbiter.Receive<DefaultSubmitResponseType>(false, writeResponse,
                    delegate(DefaultSubmitResponseType submitResponse){}),
                Arbiter.Receive<Fault>(false, writeResponse,
                    delegate(Fault fault)
                    {
                        writeFault = fault;
                    }),
                Arbiter.Receive(false, TimeoutPort(commandTimeout),
                    delegate(DateTime timeout)
                    {
                        string errorMessage = "Timeout sending the " + cmd.Body.RoombaCommandCode.ToString() + " request to the iRobot.";
                        LogError(errorMessage);
                        writeFault = Fault.FromException(new TimeoutException(errorMessage));
                    }));
                ;

            if (writeFault != null)
            {
                // Failed to write the data to the stream.
                // we were expecting data back, cancel the data waiting message
                // and send failure back to the original caller
                delayedResponsePort.Post(writeFault);
                _internalCommandWaitingPort.Clear();
                yield break;
            }

            // Waiting for return data to be posted to our temporary response port.
            yield return Arbiter.Choice(
                Arbiter.Receive<RoombaReturnPacket>(false, cmd.ResponsePort,
                    delegate(RoombaReturnPacket streamResponse)
                    {
                        // We have the data, return to caller
                        delayedResponsePort.Post(streamResponse);
                    }),
                Arbiter.Receive<Fault>(false, cmd.ResponsePort,
                    delegate(Fault fault)
                    {
                        string errorMessage = (fault.Reason != null && fault.Reason.Length > 0) ? fault.Reason[0].Value : "Fault";
                        LogError("Error in ExecuteIRobotCommandHandler: " + errorMessage);
                        delayedResponsePort.Post(fault);
                    }),
                Arbiter.Receive(false, TimeoutPort(3000),
                    delegate(DateTime timeout)
                    {
                        LogError("Timeout waiting for an iRobot " + cmd.Body.RoombaCommandCode.ToString() + " request to respond.");
                        delayedResponsePort.Post(Fault.FromException(new TimeoutException("Timeout waiting for an iRobot command response from " + cmd.Body.RoombaCommandCode.ToString())));
                        _internalCommandWaitingPort.Clear();
                    }));

            yield break;
        }

        /// <summary>
        /// Process a single Roomba command and update all state
        /// </summary>
        /// <param name="atomicCommand"></param>
        /// <returns></returns>
        private IEnumerator<ITask> ProcessAtomicCommandHandler(ProcessAtomicCommand atomicCommand)
        {
            LogVerbose(LogGroups.Console, "ProcessAtomicCommand: " + atomicCommand.Body.RoombaCommandCode.ToString());
            bool success = false;

            // Pause the stream for commands that return data,
            // but only on Create when stream data has been started.
            bool pauseStream = _streamStarted
                && (atomicCommand.Body.ExpectedResponseBytes() > 0)
                && (_state.IRobotModel == IRobotModel.Create)
                // If we are playing a script, don't pause the stream.
                && (atomicCommand.Body.RoombaCommandCode != RoombaCommandCode.PlayScript);


            if (pauseStream)
            {
                #region Pause the Stream before Create sends a command that returns data.
                LogVerbose(LogGroups.Console, "ProcessAtomicCommand: " + atomicCommand.Body.RoombaCommandCode.ToString() + " : Stream Pause");
                yield return Arbiter.Choice<RoombaReturnPacket, Fault>(
                    ExecuteIRobotCommand(new CmdStreamPauseResume(false)),
                    delegate(RoombaReturnPacket responsePacket)
                    {
                        _streamPaused = true;
                        success = true;
                    },
                    delegate(Fault fault)
                    {
                        LogError(fault);
                    });
                #endregion
            }


            RoombaMode newMode = _state.Mode;
            bool modeWillChange = IRobotUtility.GetChangedMode(atomicCommand.Body.RoombaCommandCode, _state.Mode, _state.IRobotModel, out newMode);
            object response = null;

            // if this command sets a mode to be maintained then update the state
            if (atomicCommand.Body.MaintainMode != _state.MaintainMode
                && atomicCommand.Body.MaintainMode != RoombaMode.Off)
            {
                #region if this command sets a mode to be maintained then update the state
                LogVerbose(LogGroups.Console, "ProcessAtomicCommand: " + atomicCommand.Body.RoombaCommandCode.ToString() + " : Calling MaintainMode(" + atomicCommand.Body.MaintainMode.ToString() + ")");

                yield return Arbiter.Choice(_internalPort.UpdateMode(RoombaMode.NotSpecified, atomicCommand.Body.MaintainMode),
                    delegate(DefaultUpdateResponseType modeResponse) {},
                    delegate(Fault fault) { LogError(fault); });

                LogVerbose(LogGroups.Console, "ProcessAtomicCommand: " + atomicCommand.Body.RoombaCommandCode.ToString() + " : Finished MaintainMode(" + atomicCommand.Body.MaintainMode.ToString() + ")");
                #endregion
            }

            if (pauseStream && success)
            {
                #region Pause the Stream before Create sends a command that returns data.
                for (int ix = 0; ix < 100; ix++)
                {
                    if (((TimeSpan)DateTime.Now.Subtract(_state.LastUpdated)).TotalMilliseconds > 60)
                    {
                        break;
                    }
                    LogVerbose(LogGroups.Console, "ProcessAtomicCommand: waiting for stream to stop.");

                    // if we paused the stream, pause to ensure the stream has stopped.
                    yield return Arbiter.Receive<DateTime>(false, TimeoutPort(10), delegate(DateTime waitOver) { });
                }
                LogVerbose(LogGroups.Console, "ProcessAtomicCommand: " + atomicCommand.Body.RoombaCommandCode.ToString() + " : Stream Pause Done");
                #endregion
            }

            LogVerbose(LogGroups.Console, "ProcessAtomicCommand: " + atomicCommand.Body.RoombaCommandCode.ToString() + " : ExecuteIRobotCommand");
            success = false; // keep track of the actual command status
            // Execute the command
            yield return Arbiter.Choice<RoombaReturnPacket, Fault>(
                ExecuteIRobotCommand(atomicCommand.Body),
                delegate(RoombaReturnPacket responsePacket)
                {
                    success = true;
                    if (responsePacket == null)
                        response = Fault.FromException(new InvalidOperationException("Response not received"));
                    else
                        response = responsePacket;
                },
                delegate(Fault fault)
                {
                    response = fault;
                });
            LogVerbose(LogGroups.Console, "ProcessAtomicCommand: " + atomicCommand.Body.RoombaCommandCode.ToString() + " : ExecuteIRobotCommand completed");

            #region Resume Stream if it was paused
            if (pauseStream)
            {
                LogVerbose(LogGroups.Console, "ProcessAtomicCommand: " + atomicCommand.Body.RoombaCommandCode.ToString() + " : Resume Stream");
                yield return Arbiter.Choice<RoombaReturnPacket, Fault>(
                    ExecuteIRobotCommand(new CmdStreamPauseResume(true)),
                    delegate(RoombaReturnPacket responsePacket)
                    {
                        _streamPaused = false;
                    },
                    delegate(Fault fault)
                    {
                        LogError(fault);
                    });
                LogVerbose(LogGroups.Console, "ProcessAtomicCommand: " + atomicCommand.Body.RoombaCommandCode.ToString() + " : Resume Stream completed");
            }
            #endregion

            if (success && modeWillChange)
            {
                // If this is a Create with an active stream, wait for the OIMode to change.
                // otherwise, update the mode status and assume the command completed correctly.
                #region Monitor the mode change
                if (RequestedNotifications())
                {
                    LogVerbose(LogGroups.Console, "ProcessAtomicCommand: " + atomicCommand.Body.RoombaCommandCode.ToString() + " : Waiting for response");
                    success = false; // keep track of this command's status
                    for (int ix = 0; ix < 200; ix++)
                    {
                        // We just changed the mode, wait for the notification to update our state.
                        yield return Arbiter.Receive(false, TimeoutPort(10), delegate(DateTime timeout) { });
                        if (_state.Mode == newMode)
                        {
                            LogInfo(LogGroups.Console, "Success changing to Mode " + _state.Mode.ToString());
                            success = true;
                            break;
                        }
                    }
                    if (!success)
                    {
                        string errorMessage = "Timeout waiting for Create to change from " + _state.Mode.ToString() + " to " + newMode.ToString();
                        LogError(LogGroups.Console, errorMessage);
                        response = Fault.FromException(new InvalidOperationException(errorMessage));
                    }
                }
                else
                {
                    LogVerbose(LogGroups.Console, "ProcessAtomicCommand: " + atomicCommand.Body.RoombaCommandCode.ToString() + " : Send Mode Update");
                    // We just changed the mode, update _state.Mode!
                    yield return Arbiter.Choice(_internalPort.UpdateMode(newMode),
                        delegate(DefaultUpdateResponseType modeResponse) { },
                        delegate(Fault fault) { LogError(fault); });
                    LogVerbose(LogGroups.Console, "ProcessAtomicCommand: " + atomicCommand.Body.RoombaCommandCode.ToString() + " : Send Mode Update completed");
                }
                #endregion
            }

            if (atomicCommand.Body.RoombaCommandCode == RoombaCommandCode.PlayScript)
            {
                // When a script fails to return, or does not return any data
                // it may still be running.  We need to send an internal 'Start' command
                // to cancel the pending script.  This will place the Create
                // into Passive mode.
                #region Special handling for scripts.
                if (!success || atomicCommand.Body.ExpectedResponseBytes() == 0)
                {
                    #region Cancel the Script
                    LogVerbose(LogGroups.Console, "ProcessAtomicCommand: Ending the Script");
                    yield return Arbiter.Choice<RoombaReturnPacket, Fault>(
                        ExecuteIRobotCommand(new InternalCmdStart(false)),
                        delegate(RoombaReturnPacket responsePacket) { },
                        delegate(Fault fault) { LogError(fault); });
                    #endregion
                }
                #endregion
            }

            // Send back the response.
            atomicCommand.ResponsePort.PostUnknownType(response);
            LogVerbose(LogGroups.Console, "ProcessAtomicCommand: " + atomicCommand.Body.RoombaCommandCode.ToString() + " : DONE");
            yield break;
        }

        /// <summary>
        /// <remarks>SpawnIterator[RoombaCommand, IPortSet](ProcessCommand)</remarks>
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="responsePort"></param>
        /// <returns></returns>
        private IEnumerator<ITask> ProcessCommand(RoombaCommand cmd, IPortSet responsePort)
        {
            RoombaMode newMode;
            bool success = false;

            #region Is this command suitable for the iRobot model?
            if (_state.IRobotModel == IRobotModel.Roomba
                && cmd.iRobotCreate)
            {
                responsePort.PostUnknownType(Fault.FromException(
                    new InvalidOperationException(
                        string.Format("iRobot Create command {0} is invalid for the iRobot Roomba.",
                            ((RoombaCommandCode)cmd.Command).ToString()))));

                yield break;
            }
            #endregion

            #region Place iRobot in proper mode for this command
            if (!IRobotUtility.ValidStateForCommand(cmd, _state, out newMode))
            {
                ChangeToMode changeMode = new ChangeToMode(newMode);
                LogInfo(LogGroups.Console, "Set Mode from " + _state.Mode.ToString() + " to " + newMode.ToString() + ".");
                _internalPort.PostUnknownType(changeMode);
                yield return Arbiter.Choice(changeMode.ResponsePort,
                    delegate(DefaultSubmitResponseType changeResponse)
                    {
                        success = true;
                    },
                    delegate(Fault fault)
                    {
                        success = false;
                        responsePort.PostUnknownType(fault);
                    });

                if (!success)
                    yield break;
            }
            #endregion

            LogVerbose(LogGroups.Console, "Process Command: " + cmd.RoombaCommandCode.ToString() + ".");
            success = false;
            yield return Arbiter.Choice<RoombaReturnPacket, Fault>(
                _internalPort.ProcessAtomicCommand(cmd),
                delegate(RoombaReturnPacket responsePacket)
                {
                    LogVerbose(LogGroups.Console, "  Response to " + cmd.RoombaCommandCode.ToString() + ": Success");
                    success = true;
                    responsePort.PostUnknownType(responsePacket);
                },
                delegate(Fault fault)
                {
                    LogInfo("  Response to " + cmd.RoombaCommandCode.ToString() + ": Failure");
                    responsePort.PostUnknownType(fault);
                });

            #region If necessary, maintain a certain mode
            // Do we need to maintain a particular mode?
            if (_state.Mode != _state.MaintainMode &&
                (_state.MaintainMode == RoombaMode.Passive || _state.MaintainMode == RoombaMode.Safe || _state.MaintainMode == RoombaMode.Full))
            {
                ChangeToMode changeMode = new ChangeToMode(_state.MaintainMode);
                LogInfo(LogGroups.Console, "Maintain Mode (automatic mode switch from " + _state.Mode.ToString() + " to " + _state.MaintainMode.ToString() + ").");
                _internalPort.PostUnknownType(changeMode);
                yield return Arbiter.Choice(changeMode.ResponsePort,
                    delegate(DefaultSubmitResponseType changeResponse)
                    {
                    },
                    delegate(Fault fault)
                    {
                        LogError(fault);
                    });

            }
            #endregion

            yield break;

        }

        /// <summary>
        /// Retrieve a response packet from the data stream
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="data"></param>
        /// <param name="startIx"></param>
        /// <param name="timestamp"></param>
        /// <returns></returns>
        private RoombaReturnPacket GetResponsePacket(RoombaCommand cmd, byte[] data, int startIx, DateTime timestamp)
        {
            if (cmd == null)
                return null;

            bool dataChanged = false;

            // If this command doesn't expect a response
            // generate a standard response.
            int packetLength = cmd.ExpectedResponseBytes();

            // See if this command causes the mode to change.
            RoombaMode newMode = _state.Mode;
            IRobotUtility.GetChangedMode(cmd.RoombaCommandCode, _state.Mode, _state.IRobotModel, out newMode);

            if (packetLength == 0)
                return RoombaCommandReceived.Instance(newMode);

            // If there isn't enough data, exit
            if (data == null || packetLength > (data.Length - startIx))
                return null;

            // Get the exact packet
            byte[] source = (data.Length == packetLength) ? data : ByteArray.SubArray(data, startIx, packetLength);


            switch (cmd.RoombaCommandCode)
            {
                case RoombaCommandCode.FirmwareDate:
                    ReturnFirmwareDate returnFirmwareDate = new ReturnFirmwareDate(source, timestamp);
                    return returnFirmwareDate;

                case RoombaCommandCode.Sensors:
                    CmdSensors cmdSensors = cmd as CmdSensors;
                    #region retrieve the variable length Sensors data
                    if (cmdSensors != null)
                    {
                        switch (cmdSensors.CreateSensorPacket)
                        {
                            case CreateSensorPacket.AllPose:
                                ReturnPose returnPose = new ReturnPose(_state.Mode, source);
                                returnPose.RoombaMode = newMode;
                                IRobotUtility.AdjustDistanceAndAngle(returnPose, _state.Pose);

                                dataChanged = (_state.Pose == null || !ByteArray.IsEqual(returnPose.Data, _state.Pose.Data));
                                if (dataChanged && returnPose.ValidPacket)
                                {
                                    UpdatePose updatePose = new UpdatePose(returnPose);
                                    _internalPort.PostUnknownType(updatePose);
                                    SendNotification<UpdatePose>(_subMgrPort, updatePose);
                                }

                                return returnPose;

                            case CreateSensorPacket.AllPower:
                                ReturnPower returnPower = new ReturnPower(_state.Mode, source);
                                returnPower.RoombaMode = newMode;

                                dataChanged = (_state.Power == null || !ByteArray.IsEqual(returnPower.Data, _state.Power.Data));
                                if (dataChanged && returnPower.ValidPacket)
                                {
                                    UpdatePower updatePower = new UpdatePower(returnPower);
                                    _internalPort.PostUnknownType(updatePower);
                                    SendNotification<UpdatePower>(_subMgrPort, updatePower);
                                }
                                return returnPower;

                            case CreateSensorPacket.AllBumpsCliffsAndWalls:
                                ReturnSensors returnSensors = new ReturnSensors(_state.Mode, source, timestamp);
                                returnSensors.RoombaMode = newMode;

                                dataChanged = (_state.Sensors == null || !ByteArray.IsEqual(returnSensors.Data, _state.Sensors.Data));
                                if (dataChanged && returnSensors.ValidPacket)
                                {
                                    UpdateBumpsCliffsAndWalls updateBumpsCliffsAndWalls = new UpdateBumpsCliffsAndWalls(returnSensors);
                                    _internalPort.PostUnknownType(updateBumpsCliffsAndWalls);
                                    SendNotification<UpdateBumpsCliffsAndWalls>(_subMgrPort, updateBumpsCliffsAndWalls);
                                }

                                return returnSensors;

                            case CreateSensorPacket.AllTelemetry:
                                ReturnTelemetry returnTelemetry = new ReturnTelemetry(source);
                                returnTelemetry.RoombaMode = returnTelemetry.OIMode;

                                dataChanged = (_state.Telemetry == null || !ByteArray.IsEqual(returnTelemetry.Data, _state.Telemetry.Data));
                                if (dataChanged && returnTelemetry.ValidPacket)
                                {
                                    UpdateTelemetry updateTelemetry = new UpdateTelemetry(returnTelemetry);
                                    _internalPort.PostUnknownType(updateTelemetry);
                                    SendNotification<UpdateTelemetry>(_subMgrPort, updateTelemetry);
                                }

                                return returnTelemetry;

                            case CreateSensorPacket.AllCliffDetail:
                                ReturnCliffDetail returnCliffDetail = new ReturnCliffDetail(newMode, source);

                                dataChanged = (_state.CliffDetail == null || !ByteArray.IsEqual(returnCliffDetail.Data, _state.CliffDetail.Data));
                                if (dataChanged && returnCliffDetail.ValidPacket)
                                {
                                    UpdateCliffDetail updateCliffDetail = new UpdateCliffDetail(returnCliffDetail);
                                    _internalPort.PostUnknownType(updateCliffDetail);
                                    SendNotification<UpdateCliffDetail>(_subMgrPort, updateCliffDetail);
                                }

                                return returnCliffDetail;

                            case CreateSensorPacket.AllRoomba:
                                ReturnAll returnAll = new ReturnAll();
                                ByteArray.CopyTo(returnAll.Sensors.Data, source, 0, 0, 10);
                                ByteArray.CopyTo(returnAll.Pose.Data, source, 10, 0, 6);
                                ByteArray.CopyTo(returnAll.Power.Data, source, 16, 0, 10);
                                returnAll.RoombaMode = newMode;
                                IRobotUtility.AdjustDistanceAndAngle(returnAll.Pose, _state.Pose);

                                if (returnAll.ValidPacket)
                                {
                                    // When data changes, send Notifications for Pose, Power, & Sensors
                                    if (_state.Sensors == null || !ByteArray.IsEqual(returnAll.Sensors.Data, _state.Sensors.Data))
                                    {
                                        SendNotification<UpdateBumpsCliffsAndWalls>(_subMgrPort, returnAll.Sensors);
                                        dataChanged = true;
                                    }
                                    if (_state.Pose == null || !ByteArray.IsEqual(returnAll.Pose.Data, _state.Pose.Data))
                                    {
                                        SendNotification<UpdatePose>(_subMgrPort, returnAll.Pose);
                                        dataChanged = true;
                                    }
                                    if (_state.Power == null || !ByteArray.IsEqual(returnAll.Power.Data, _state.Power.Data))
                                    {
                                        SendNotification<UpdatePower>(_subMgrPort, returnAll.Power);
                                        dataChanged = true;
                                    }

                                    if (dataChanged)
                                    {
                                        UpdateAll updateAll = new UpdateAll(returnAll);
                                        _internalPort.PostUnknownType(updateAll);
                                    }
                                }
                                else
                                {
                                    // bad data!
                                    LogError(LogGroups.Console,"Bad Data:");
                                }

                                return returnAll;

                            case CreateSensorPacket.AllCreate:
                                ReturnAll returnAllCreate = new ReturnAll();
                                if (returnAllCreate.CliffDetail == null)
                                    returnAllCreate.CliffDetail = new ReturnCliffDetail();
                                if (returnAllCreate.Telemetry == null)
                                    returnAllCreate.Telemetry = new ReturnTelemetry();

                                ByteArray.CopyTo(returnAllCreate.Sensors.Data, source, 0, 0, 10);
                                ByteArray.CopyTo(returnAllCreate.Pose.Data, source, 10, 0, 6);
                                ByteArray.CopyTo(returnAllCreate.Power.Data, source, 16, 0, 10);
                                ByteArray.CopyTo(returnAllCreate.CliffDetail.Data, source, 26, 0, 14);
                                ByteArray.CopyTo(returnAllCreate.Telemetry.Data, source, 40, 0, 12);
                                returnAllCreate.RoombaMode = newMode;
                                IRobotUtility.AdjustDistanceAndAngle(returnAllCreate.Pose, _state.Pose);

                                if (returnAllCreate.ValidPacket)
                                {

                                    // Send Notifications for Pose, Power, Sensors, CliffDetail, & Telemetry
                                    if (_state.Sensors == null || !ByteArray.IsEqual(returnAllCreate.Sensors.Data, _state.Sensors.Data))
                                    {
                                        SendNotification<UpdateBumpsCliffsAndWalls>(_subMgrPort, returnAllCreate.Sensors);
                                        dataChanged = true;
                                    }
                                    if (_state.Pose == null || !ByteArray.IsEqual(returnAllCreate.Pose.Data, _state.Pose.Data))
                                    {
                                        SendNotification<UpdatePose>(_subMgrPort, returnAllCreate.Pose);
                                        dataChanged = true;
                                    }
                                    if (_state.Power == null || !ByteArray.IsEqual(returnAllCreate.Power.Data, _state.Power.Data))
                                    {
                                        SendNotification<UpdatePower>(_subMgrPort, returnAllCreate.Power);
                                        dataChanged = true;
                                    }
                                    if (returnAllCreate.Telemetry != null &&
                                        ( _state.Telemetry == null
                                            || !ByteArray.IsEqual(returnAllCreate.Telemetry.Data, _state.Telemetry.Data)))
                                    {
                                        SendNotification<UpdateTelemetry>(_subMgrPort, returnAllCreate.Telemetry);
                                        dataChanged = true;
                                    }
                                    if (returnAllCreate.CliffDetail != null &&
                                        ( _state.CliffDetail == null
                                            || !ByteArray.IsEqual(returnAllCreate.CliffDetail.Data, _state.CliffDetail.Data)))
                                    {
                                        SendNotification<UpdateCliffDetail>(_subMgrPort, returnAllCreate.CliffDetail);
                                        dataChanged = true;
                                    }

                                    if (dataChanged)
                                    {
                                        UpdateAll updateAllCreate = new UpdateAll(returnAllCreate);
                                        _internalPort.PostUnknownType(updateAllCreate);
                                    }
                                }
                                return returnAllCreate;

                            default:
                                // Requested a single value.
                                List<CreateSensorPacket> sensor = new List<CreateSensorPacket>();
                                sensor.Add(cmdSensors.CreateSensorPacket);
                                CmdQueryList cmdSensorList = new CmdQueryList(sensor);
                                ReturnQueryList returnQueryList = new ReturnQueryList(source, cmdSensorList);
                                return returnQueryList;
                        }
                    }
                    #endregion
                    break;
                case RoombaCommandCode.QueryList:
                    CmdQueryList cmdQueryList = cmd as CmdQueryList;
                    if (cmdQueryList != null)
                    {
                        ReturnQueryList returnQueryList = new ReturnQueryList(source, cmdQueryList);
                        return returnQueryList;
                    }
                    break;
                case RoombaCommandCode.ShowScript:
                    ReturnScript returnScript = new ReturnScript(source, timestamp);
                    return returnScript;

                case RoombaCommandCode.PlayScript:
                    // Throw away the data returned by the script.
                    // This is generally used to signal the end of the script.
                    return RoombaCommandReceived.Instance(_state.Mode);

                default:
                    throw new Exception("The method or operation is not implemented: " + cmd.RoombaCommandCode.ToString() + ".");
            }
            return null;

        }


        #region Private Methods

        /// <summary>
        /// Determine if this is a binary data packet or a text string.
        /// Post all pending bytes to the appropriate port.
        /// </summary>
        /// <param name="data">The data buffer</param>
        /// <param name="startIx">The first byte to process</param>
        /// <param name="endIx">One past the last valid byte</param>
        /// <param name="packetTime">The time the data was received</param>
        /// <param name="executeIRobotCommand">The pending command</param>
        /// <returns></returns>
        private int AnalyzeBuffer(byte[] data, int startIx, int endIx, DateTime packetTime, ExecuteIRobotCommand executeIRobotCommand)
        {
            if (executeIRobotCommand == null || executeIRobotCommand.Body == null)
                return startIx;

            int ix = startIx;
            int packetLength;

            packetLength = executeIRobotCommand.Body.ExpectedResponseBytes();
            if (packetLength > (endIx - startIx))
            {
                // Don't have enough bytes yet.
                if (_internalCommandWaitingPort.ItemCount == 0)
                    return startIx;

                // This is an error condition
                LogError(LogGroups.Console, "Multiple pending commands are not allowed");
                return startIx;
            }

            RoombaReturnPacket returnPacket = GetResponsePacket(executeIRobotCommand.Body, data, startIx, packetTime);
            if (returnPacket == null)
            {
                // This wasn't our data.
                // put the pending command back on the queue
                LogInfo("  Invalid Response Packet.");
                if (data != null && ((data.Length - startIx) > 0))
                    ShowBuffer(ByteArray.SubArray(data, startIx, data.Length - startIx));

                return startIx;
            }
            else if (!returnPacket.ValidPacket)
            {
                bool found = false;
                // If we have more data than expected, then check to see
                // if the end of the buffer contains our returnPacket.
                if (packetLength < (endIx - startIx))
                {
                    returnPacket = GetResponsePacket(executeIRobotCommand.Body, data, endIx - packetLength, packetTime);
                    if (returnPacket != null && returnPacket.ValidPacket)
                    {
                        startIx = endIx - packetLength;
                        found = true;
                    }
                }

                if (!found)
                {
                    LogInfo("  Invalid Response Packet " + packetLength.ToString() + " bytes.");
                    ShowBuffer(returnPacket.Data);

                    // Should we return a fault?
                    executeIRobotCommand.ResponsePort.Post(returnPacket);
                    return startIx;
                }
            }

            // Send the command response.
            executeIRobotCommand.ResponsePort.Post(returnPacket);
            LogVerbose(LogGroups.Console, "  Command Response Packet " + packetLength.ToString() + " bytes.");
            return startIx + packetLength;
        }

        #endregion

    }
}
