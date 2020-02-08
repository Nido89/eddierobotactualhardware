//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: SimulatedIRobotLite.cs $ $Revision: 5 $
//-----------------------------------------------------------------------

using Microsoft.Ccr.Core;
using Microsoft.Dss.Core;
using Microsoft.Dss.Core.Attributes;
using Microsoft.Dss.ServiceModel.Dssp;
using Microsoft.Dss.ServiceModel.DsspServiceBase;
using W3C.Soap;
using System;
using System.Collections.Generic;
using System.Threading;
using dssphttp = Microsoft.Dss.Core.DsspHttp;
using create = Microsoft.Robotics.Services.IRobot.Create.Proxy;
using irobot = Microsoft.Robotics.Services.IRobot.Lite.Proxy;
using irobottypes = Microsoft.Robotics.Services.IRobot.Roomba.Proxy;
using irobotupdates = Microsoft.Robotics.Services.IRobot.SensorUpdates.Proxy;
using submgr = Microsoft.Dss.Services.SubscriptionManager;
using System.ComponentModel;
using simengine = Microsoft.Robotics.Simulation.Engine;
using simcommon = Microsoft.Robotics.Simulation.Physics;
using Microsoft.Robotics.PhysicalModel;
using Microsoft.Robotics.Services.IRobot.Roomba.Proxy;
using xna = Microsoft.Xna.Framework;
using xnagrfx = Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Robotics.Services.Sample.SimulatedIRobotLite
{

    /// <summary>
    /// Simulated iRobot Lite Service
    /// </summary>
    [DisplayName("(User) iRobot� Create Lite Simulation")]
    [Description("Provides direct access to the iRobot Create Lite Simulation service which contains a reduced subset of the iRobot Create commands and is available in the simulation environment.")]
    [Contract(Contract.Identifier)]
    [AlternateContract(irobot.Contract.Identifier)]
    public class SimulatedIRobotLiteService : DsspServiceBase
    {
        private int _tickInterval = 250;
        private DateTime _nextTimer = DateTime.MaxValue;

        #region Simulation Variables
        simengine.SimulationEnginePort _simEngine;
        simengine.IRobotCreate _entity;
        IREntity _irEntity;
        simengine.SimulationEnginePort _notificationTarget;
        Port<simcommon.EntityContactNotification> _contactPort;
        #endregion

        /// <summary>
        /// _main Port
        /// </summary>
        [ServicePort("/simulatedirobotlite", AllowMultipleInstances=true)]
        private irobot.IRobotLiteOperations _mainPort = new irobot.IRobotLiteOperations();

        // Subscription manager partner
        [Partner(Microsoft.Dss.ServiceModel.Dssp.Partners.SubscriptionManagerString,
            Contract = submgr.Contract.Identifier,
            CreationPolicy = PartnerCreationPolicy.CreateAlways)]
        submgr.SubscriptionManagerPort _subMgrPort = new submgr.SubscriptionManagerPort();

        /// <summary>
        /// Microsoft.Robotics.Services.Sample.SimulatedIRobotLite.SimulatedIRobotLiteService State
        /// </summary>
        [InitialStatePartner(Optional = true, ServiceUri = ServicePaths.Store + "/SimulatedIRobotLite.config.xml")]
        private irobottypes.RoombaState _state = new irobottypes.RoombaState();

        // This is a copy of the state which represents the current hardware state.
        // At each polling interval, we examine this state for differences with the service
        // state and send notifications to subscribers if there are differences.
        private irobottypes.RoombaState _hardwareState = null;

        /// <summary>
        /// Default Service Constructor
        /// </summary>
        public SimulatedIRobotLiteService(DsspServiceCreationPort creationPort) :
                base(creationPort)
        {
        }

        /// <summary>
        /// Service Start
        /// </summary>
        protected override void Start()
        {
            ValidateState();

            base.Start();

            // Find our simulation entity that represents the "hardware" or real-world service.
            // To hook up with simulation entities we do the following steps
            // 1) have a manifest or some other service create us, specifying a partner named SimulationEntity
            // 2) in the simulation service (us) issue a subscribe to the simulation engine looking for
            //    an instance of that simulation entity. We use the Entity.State.Name for the match so it must be
            //    exactly the same. See SimulationTutorial2 for the creation process
            // 3) Listen for a notification telling us the entity is available
            // 4) cache reference to entity and communicate with it issuing low level commands.

            _simEngine = simengine.SimulationEngine.GlobalInstancePort;
            _notificationTarget = new simengine.SimulationEnginePort();

            MainPortInterleave.CombineWith(new Interleave(
                new TeardownReceiverGroup(),
                new ExclusiveReceiverGroup
                (
                    Arbiter.Receive<simengine.InsertSimulationEntity>(true, _notificationTarget, InsertEntityNotificationHandlerFirstTime),
                    Arbiter.ReceiveWithIterator<irobottypes.Connect>(true, _mainPort, ConnectHandler)
                ),
                new ConcurrentReceiverGroup()
            ));

            SubscribeForEntity();
        }

        void SubscribeForEntity()
        {
            // our entity instance name is passed as a partner
            PartnerType entityPartner = Dss.ServiceModel.DsspServiceBase.DsspServiceBase.FindPartner(
                new System.Xml.XmlQualifiedName(Microsoft.Robotics.Simulation.Partners.Entity, Microsoft.Robotics.Simulation.Contract.Identifier),
                ServiceInfo.PartnerList);

            if (entityPartner == null)
            {
                LogError("Invalid entity name specified as a partner to the iRobotLite Service");
            }
            else
            {
                // PartnerType.Service is the entity instance name. 
                simengine.EntitySubscribeRequestType esrt = new simengine.EntitySubscribeRequestType();

                esrt.Name = new Uri(entityPartner.Service).LocalPath.TrimStart('/');
                esrt.Subscriber = ServiceInfo.Service;
                _simEngine.Subscribe(esrt, _notificationTarget);
            }
        }

        private void ValidateState()
        {
            if (_state == null)
                _state = new irobottypes.RoombaState();

            // Default connection is wired RS232
            _state.ConnectionType = irobottypes.iRobotConnectionType.RoombaSerialPort;

            _state.IRobotModel = irobottypes.IRobotModel.Create;

            if (_state.CliffDetail == null)
                _state.CliffDetail = new create.ReturnCliffDetail();

            if (_state.Telemetry == null)
            {
                _state.Telemetry = new create.ReturnTelemetry();
                _state.Telemetry.OIMode = _state.Mode;
            }

            if (_state.CreateNotifications == null)
            {
                _state.CreateNotifications = new List<create.CreateSensorPacket>();
                _state.CreateNotifications.Add(create.CreateSensorPacket.OIMode);
                _state.CreateNotifications.Add(create.CreateSensorPacket.AllBumpsCliffsAndWalls);
                _state.CreateNotifications.Add(create.CreateSensorPacket.Buttons);
            }

            if (_state.Pose == null)
                _state.Pose = new  irobottypes.ReturnPose();
            if (_state.Power == null)
                _state.Power = new irobottypes.ReturnPower();
            if (_state.Sensors == null)
                _state.Sensors = new irobottypes.ReturnSensors();

            // Give the robot a name.
            if (string.IsNullOrEmpty(_state.Name))
                _state.Name = "Hiro";

            if (_state.SongDefinitions == null)
                _state.SongDefinitions = new List<irobottypes.CmdDefineSong>();

            bool song1Defined = false;
            bool song2Defined = false;
            if (_state.SongDefinitions == null)
            {
                _state.SongDefinitions = new List<irobottypes.CmdDefineSong>();
            }
            else
            {
                foreach (irobottypes.CmdDefineSong cmdDefineSong in _state.SongDefinitions)
                {
                    if (cmdDefineSong.SongNumber == 1)
                        song1Defined = true;
                    if (cmdDefineSong.SongNumber == 2)
                        song2Defined = true;
                }
            }
            irobottypes.CmdDefineSong song;
            if (!song1Defined)
            {
                song = DefineAscendingSong(1);
                _state.SongDefinitions.Add(song);
            }

            if (!song2Defined)
            {
                song = DefineDecendingSong(2);
                _state.SongDefinitions.Add(song);
            }
        }

        /// <summary>
        /// Define two ascending notes
        /// </summary>
        /// <param name="songId"></param>
        /// <returns></returns>
        private static irobottypes.CmdDefineSong DefineAscendingSong(int songId)
        {
            // Define a song
            List<irobottypes.RoombaNote> notes = new List<irobottypes.RoombaNote>();
            notes.Add(new irobottypes.RoombaNote(irobottypes.RoombaFrequency.B_Hz_493p9, 20));
            notes.Add(new irobottypes.RoombaNote(irobottypes.RoombaFrequency.B_Hz_987p8, 10));
            return new irobottypes.CmdDefineSong(songId, notes);
        }

        /// <summary>
        /// Define two descending notes
        /// </summary>
        /// <param name="songId"></param>
        /// <returns></returns>
        private static irobottypes.CmdDefineSong DefineDecendingSong(int songId)
        {
            // Define a song
            List<irobottypes.RoombaNote> notes = new List<irobottypes.RoombaNote>();
            notes.Add(new irobottypes.RoombaNote(irobottypes.RoombaFrequency.B_Hz_987p8, 10));
            notes.Add(new irobottypes.RoombaNote(irobottypes.RoombaFrequency.B_Hz_493p9, 20));
            return new irobottypes.CmdDefineSong(songId, notes);
        }

        #region iRobot Lite Operation Handlers

        /// <summary>
        /// Get Handler
        /// </summary>
        /// <param name="get"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public virtual IEnumerator<ITask> GetHandler(irobottypes.Get get)
        {
            get.ResponsePort.Post(_state);
            yield break;
        }

        /// <summary>
        /// HttpGet Handler
        /// </summary>
        /// <param name="get"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public virtual IEnumerator<ITask> HttpGetHandler(Microsoft.Dss.Core.DsspHttp.HttpGet get)
        {
            get.ResponsePort.Post(new dssphttp.HttpResponseType(_state));
            yield break;
        }

        /// <summary>
        /// HttpPost Handler
        /// </summary>
        /// <param name="submit"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public virtual IEnumerator<ITask> HttpPostHandler(dssphttp.HttpPost submit)
        {
            LogError("This service doesn't accept HttpPosts");
            submit.ResponsePort.Post(new dssphttp.HttpResponseType());
            yield break;
        }

        /// <summary>
        /// Subscribe Handler
        /// </summary>
        /// <param name="subscribe"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public virtual IEnumerator<ITask> SubscribeHandler(Microsoft.Robotics.Services.IRobot.SensorUpdates.Proxy.Subscribe subscribe)
        {
            yield return Arbiter.Choice(
                base.SubscribeHelper(_subMgrPort, subscribe.Body, subscribe.ResponsePort),
                delegate(SuccessResult success)
                {
                    irobottypes.ReturnMode returnMode = new irobottypes.ReturnMode();
                    returnMode.IRobotModel = _state.IRobotModel;
                    returnMode.MaintainMode = _state.MaintainMode;
                    returnMode.RoombaMode = _state.Mode;
                    returnMode.FirmwareDate = _state.FirmwareDate;
                    SendNotificationToTarget<irobotupdates.UpdateMode>(subscribe.Body.Subscriber, _subMgrPort, returnMode);
                },
                delegate(Exception fault)
                {
                    LogError(fault);
                }
            );

            yield break;

        }

        /// <summary>
        /// Connect Handler
        /// </summary>
        /// <param name="update"></param>
        /// <returns></returns>
        public virtual IEnumerator<ITask> ConnectHandler(irobottypes.Connect update)
        {
            _state.Mode = irobottypes.RoombaMode.Full;
            _state.Telemetry.OIMode = _state.Mode;

            update.ResponsePort.Post(DefaultUpdateResponseType.Instance);
            yield break;
        }

        /// <summary>
        /// Configure Handler
        /// </summary>
        /// <param name="configure"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public virtual IEnumerator<ITask> ConfigureHandler(irobottypes.Configure configure)
        {
            _state = configure.Body;
            configure.ResponsePort.Post(DefaultReplaceResponseType.Instance);
            yield break;
        }

        /// <summary>
        /// RoombaSetMode Handler
        /// </summary>
        /// <param name="update"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public virtual IEnumerator<ITask> RoombaSetModeHandler(irobottypes.RoombaSetMode update)
        {
            if (update.Body.MaintainMode)
            {
                _state.MaintainMode = update.Body.RoombaMode;
            }
            _state.Mode = update.Body.RoombaMode;


            update.ResponsePort.Post(new irobottypes.RoombaCommandReceived(irobottypes.RoombaMode.NotSpecified));
            yield break;
        }

        /// <summary>
        /// RoombaSetLeds Handler
        /// </summary>
        /// <param name="update"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public virtual IEnumerator<ITask> RoombaSetLedsHandler(irobottypes.RoombaSetLeds update)
        {
            // Simulation doesn't currently support the LEDs
            update.ResponsePort.Post(new irobottypes.RoombaCommandReceived(irobottypes.RoombaMode.NotSpecified));
            yield break;
        }

        /// <summary>
        /// RoombaPlaySong Handler
        /// </summary>
        /// <param name="update"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public virtual IEnumerator<ITask> RoombaPlaySongHandler(irobottypes.RoombaPlaySong update)
        {
            if (update.Body.SongNumber > 0
                && _state.SongDefinitions != null
                && update.Body.SongNumber <= _state.SongDefinitions.Count)
            {
                _state.Telemetry.SongPlaying = true;
                foreach (irobottypes.RoombaNote note in _state.SongDefinitions[update.Body.SongNumber - 1].Notes)
                {
                    if (note.Tone == irobottypes.RoombaFrequency.Rest)
                        Thread.Sleep((int)(note.Duration * 1000.0f / 64.0f));
                    else
                        Console.Beep(GetFrequency(note.Tone), (int)(note.Duration * 1000.0f / 64.0f));
                }
            }
            else
            {
                throw new ArgumentOutOfRangeException("Song " + update.Body.SongNumber.ToString() + " is not defined");
            }

            update.ResponsePort.Post(new irobottypes.RoombaCommandReceived(irobottypes.RoombaMode.NotSpecified));
            _state.Telemetry.SongPlaying = false;
            yield break;
        }

        private int GetFrequency(irobottypes.RoombaFrequency tone)
        {
            string freq = tone.ToString().Split(new char[]{'_'})[2];
            int len = 0;
            while (char.IsDigit(freq[len]))
                len++;
            return int.Parse(freq.Substring(0, len));
        }

        /// <summary>
        /// RoombaGetSensors Handler
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public virtual IEnumerator<ITask> RoombaGetSensorsHandler(irobottypes.RoombaGetSensors query)
        {
            if (query.Body.CreateSensorPacket == create.CreateSensorPacket.AllCreate)
            {
                irobottypes.ReturnAll returnAll = new irobottypes.ReturnAll();
                returnAll.Pose = _state.Pose;
                returnAll.Power = _state.Power;
                returnAll.Sensors = _state.Sensors;
                returnAll.CliffDetail = _state.CliffDetail;
                returnAll.Telemetry = _state.Telemetry;
                returnAll.RoombaMode = _state.Mode;
                returnAll.Timestamp = _state.LastUpdated;

                query.ResponsePort.Post(returnAll);
                yield break;
            }

            // TODO: Implement Query operations here.
            throw new NotImplementedException("TODO: Implement Query operations here.");
        }
        /// <summary>
        /// CreateDriveDirect Handler
        /// </summary>
        /// <param name="update"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public virtual IEnumerator<ITask> CreateDriveDirectHandler(create.CreateDriveDirect update)
        {
            if (_entity == null)
            {
                if (update.Body.LeftVelocity == 0 && update.Body.RightVelocity == 0)
                    update.ResponsePort.Post(new irobottypes.RoombaCommandReceived(irobottypes.RoombaMode.NotSpecified));
                else
                    update.ResponsePort.Post(Fault.FromException(new InvalidOperationException("The Simulation iRobot has not yet been initialized")));

                yield break;
            }

            _entity.SetVelocity(update.Body.LeftVelocity / 1000.0f, update.Body.RightVelocity / 1000.0f);
            update.ResponsePort.Post(new irobottypes.RoombaCommandReceived(irobottypes.RoombaMode.NotSpecified));
            yield break;
        }
        /// <summary>
        /// UpdateAll Handler
        /// </summary>
        /// <param name="update"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public virtual IEnumerator<ITask> UpdateAllHandler(irobotupdates.UpdateAll update)
        {
            throw new InvalidOperationException("Sensor Updates are outbound operations and not valid for sending requests.");
        }

        /// <summary>
        /// UpdateBumpsCliffsAndWalls Handler
        /// </summary>
        /// <param name="update"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public virtual IEnumerator<ITask> UpdateBumpsCliffsAndWallsHandler(irobotupdates.UpdateBumpsCliffsAndWalls update)
        {
            throw new InvalidOperationException("Sensor Updates are outbound operations and not valid for sending requests.");
        }
        /// <summary>
        /// UpdatePose Handler
        /// </summary>
        /// <param name="update"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public virtual IEnumerator<ITask> UpdatePoseHandler(irobotupdates.UpdatePose update)
        {
            throw new InvalidOperationException("Sensor Updates are outbound operations and not valid for sending requests.");
        }
        /// <summary>
        /// UpdatePower Handler
        /// </summary>
        /// <param name="update"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public virtual IEnumerator<ITask> UpdatePowerHandler(irobotupdates.UpdatePower update)
        {
            throw new InvalidOperationException("Sensor Updates are outbound operations and not valid for sending requests.");
        }
        /// <summary>
        /// UpdateMode Handler
        /// </summary>
        /// <param name="update"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public virtual IEnumerator<ITask> UpdateModeHandler(irobotupdates.UpdateMode update)
        {
            throw new InvalidOperationException("Sensor Updates are outbound operations and not valid for sending requests.");
        }

        /// <summary>
        /// UpdateCliffDetail Handler
        /// </summary>
        /// <param name="update"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public virtual IEnumerator<ITask> UpdateCliffDetailHandler(irobotupdates.UpdateCliffDetail update)
        {
            throw new InvalidOperationException("Sensor Updates are outbound operations and not valid for sending requests.");
        }

        /// <summary>
        /// UpdateTelemetry Handler
        /// </summary>
        /// <param name="update"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public virtual IEnumerator<ITask> UpdateTelemetryHandler(irobotupdates.UpdateTelemetry update)
        {
            throw new InvalidOperationException("Sensor Updates are outbound operations and not valid for sending requests.");
        }

        /// <summary>
        /// UpdateNotifications Handler
        /// </summary>
        /// <param name="update"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public virtual IEnumerator<ITask> UpdateNotificationsHandler(irobotupdates.UpdateNotifications update)
        {
            throw new InvalidOperationException("Sensor Updates are outbound operations and not valid for sending requests.");
        }

        /// <summary>
        /// Custom Drop Handler
        /// </summary>
        /// <param name="drop"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Teardown)]
        public virtual void DropHandler(DsspDefaultDrop drop)
        {
            LogInfo(LogGroups.Console, string.Format("Shutting down simulation iRobot: {0}", _state.Name));
            base.DefaultDropHandler(drop);
        }

        #endregion

        /// <summary>
        /// This handler receives an announcement from the simulation engine that
        /// contains a pointer to the entity associated with this service.
        /// </summary>
        /// <param name="ins"></param>
        void InsertEntityNotificationHandlerFirstTime(simengine.InsertSimulationEntity ins)
        {
            _entity = (simengine.IRobotCreate)ins.Body;
            _entity.ServiceContract = Contract.Identifier;
            _contactPort = new Port<simcommon.EntityContactNotification>();
            // add the contact handler
            MainPortInterleave.CombineWith(new Interleave(
                new TeardownReceiverGroup(
                ),
                new ExclusiveReceiverGroup
                (
                ),
                new ConcurrentReceiverGroup
                (
                    Arbiter.Receive<simcommon.EntityContactNotification>(true, _contactPort, ContactHandler)
                )
            ));

            _entity.Subscribe(_contactPort);

            PartnerType irEntityPartner = Dss.ServiceModel.DsspServiceBase.DsspServiceBase.FindPartner(
                new System.Xml.XmlQualifiedName("IREntity", Contract.Identifier), ServiceInfo.PartnerList);


            // only add an IR Entity if one was requested
            if (irEntityPartner != null)
            {
                // add an IR sensor, if necessary
                _irEntity = null;
                foreach (simengine.VisualEntity child in _entity.Children)
                    if (child.GetType() == typeof(IREntity))
                        _irEntity = (IREntity)child;

                if (_irEntity == null)
                {
                    // didn't find on already there so add one
                    
                    // irentity is on the right front quadrant of the Create facing to the right
                    _irEntity = new IREntity(_entity.State.Name + "_IR", new Pose(
                        new Vector3(0.115f, 0.055f, -0.115f),
                        simengine.TypeConversion.FromXNA(xna.Quaternion.CreateFromAxisAngle(new xna.Vector3(0, 1, 0), (float)(Math.PI / 2)))));
                    _entity.InsertEntity(_irEntity);
                    // refresh the whole entity
                    System.Reflection.MethodInfo mInfo = typeof(simengine.SimulationEngine).GetMethod("RefreshEntity",
                        System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
                    if(mInfo != null)
                        mInfo.Invoke(simengine.SimulationEngine.GlobalInstance, new object[] { _entity });
                }
            }

            // start up the polling timer
            int timerInterval = (_state.PollingInterval <= 0) ? 200 : _state.PollingInterval;
            if ((timerInterval > 0 && timerInterval < 200))
                timerInterval = 200;

            StartTimer(timerInterval);
            // create default state based on the physics entity
            //_state.DistanceBetweenWheels = _entity.ChassisShape.BoxState.Dimensions.X;
            //_state.LeftWheel.MotorState.PowerScalingFactor = _entity.MotorTorqueScaling;
            //_state.RightWheel.MotorState.PowerScalingFactor = _entity.MotorTorqueScaling;
        }

        /// <summary>
        /// This handler receives a notification when the bumper contact state changes
        /// </summary>
        /// <param name="contact"></param>
        void ContactHandler(simcommon.EntityContactNotification contact)
        {
            if (contact.Stage == simcommon.ContactNotificationStage.Finished)
            {
                _entity.BumperLeft = _entity.BumperRight = false;
            }
            else
            {
                bool right = false;
                bool left = false;
                foreach(simcommon.ShapeContact sc in contact.ShapeContacts)
                {
                    switch(sc.LocalShape.State.Name[sc.LocalShape.State.Name.Length-2])
                    {
                        case 'L': left = true; break;
                        case 'R': right = true; break;
                        case 'B': left = true; right = true; break;
                    }
                }
                _entity.BumperLeft = left;
                _entity.BumperRight = right;
            }
        }

        #region Polling Timer

        /// <summary>
        /// Start the timer
        /// </summary>
        private void StartTimer(int tickInterval)
        {
            // if no baseline state has been established, do it now
            if (_hardwareState == null)
                _hardwareState = (irobottypes.RoombaState)_state.Clone();

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
            if (_state.Mode == irobottypes.RoombaMode.Shutdown)
                return;

            // ignore timer if the Create is not initialized.
            if (_state.Mode == irobottypes.RoombaMode.Uninitialized)
            {
                WaitForNextTimer();
                return;
            }

            // Is Polling turned off?
            if (_state.PollingInterval < 0)
            {
                WaitForNextTimer();
                return;
            }

            LogVerbose(LogGroups.Console, "Timer: Starting query");
            bool dataChanged = false;

            UpdateHardwareStateFromEntity(_hardwareState);

            // check each block of state for differences
            if (StateIsDifferent(_hardwareState.Pose, _state.Pose))
            {
                SendNotification<irobotupdates.UpdatePose>(_subMgrPort, _hardwareState.Pose);
                dataChanged = true;
            }
            if (StateIsDifferent(_hardwareState.Sensors, _state.Sensors))
            {
                SendNotification<irobotupdates.UpdateBumpsCliffsAndWalls>(_subMgrPort, _hardwareState.Sensors);
                dataChanged = true;
            }
            if (StateIsDifferent(_hardwareState.Power, _state.Power))
            {
                SendNotification<irobotupdates.UpdatePower>(_subMgrPort, _hardwareState.Power);
                dataChanged = true;
            }
            if (StateIsDifferent(_hardwareState.CliffDetail, _state.CliffDetail))
            {
                SendNotification<irobotupdates.UpdateCliffDetail>(_subMgrPort, _hardwareState.CliffDetail);
                dataChanged = true;
            }

            if (dataChanged)
            {
                _state = _hardwareState;
                _hardwareState = (irobottypes.RoombaState)_state.Clone();
            }
            else
            {
                _state.LastUpdated = _hardwareState.LastUpdated;
                _state.Pose.Timestamp = _hardwareState.Pose.Timestamp;
                _state.Power.Timestamp = _hardwareState.Power.Timestamp;
                _state.Sensors.Timestamp = _hardwareState.Sensors.Timestamp;
                _state.CliffDetail.Timestamp = _hardwareState.CliffDetail.Timestamp;
                _state.Telemetry.Timestamp = _hardwareState.Telemetry.Timestamp;
            }

            WaitForNextTimer();
        }

        private bool StateIsDifferent(irobottypes.ReturnPose a, irobottypes.ReturnPose b)
        {
            return ((a.Angle != b.Angle) ||
                    (a.ButtonsCreate != b.ButtonsCreate) ||
                    (a.Distance != b.Distance) ||
                    (a.RoombaMode != b.RoombaMode));
        }

        private bool StateIsDifferent(create.ReturnCliffDetail a, create.ReturnCliffDetail b)
        {
            return ((a.CliffFrontLeftSignal != b.CliffFrontLeftSignal) ||
                    (a.CliffFrontRightSignal != b.CliffFrontRightSignal) ||
                    (a.CliffLeftSignal != b.CliffLeftSignal) ||
                    (a.CliffRightSignal != b.CliffRightSignal) ||
                    (a.WallSignal != b.WallSignal));
        }

        private bool StateIsDifferent(irobottypes.ReturnSensors a, irobottypes.ReturnSensors b)
        {
            return ((a.BumpsWheeldrops != b.BumpsWheeldrops) ||
                    (a.CliffFrontLeft != b.CliffFrontLeft) ||
                    (a.CliffFrontRight != b.CliffFrontRight) ||
                    (a.CliffLeft != b.CliffLeft) ||
                    (a.CliffRight != b.CliffRight) ||
                    (a.DirtDetectorLeft != b.DirtDetectorLeft) ||
                    (a.DirtDetectorRight != b.DirtDetectorRight) ||
                    (a.MotorOvercurrents != b.MotorOvercurrents) ||
                    (a.RoombaMode != b.RoombaMode) ||
                    (a.VirtualWall != b.VirtualWall) ||
                    (a.Wall != b.Wall));
        }
        private bool StateIsDifferent(irobottypes.ReturnPower a, irobottypes.ReturnPower b)
        {
            return ((a.Capacity != b.Capacity) ||
                    (a.Charge != b.Charge) ||
                    (a.ChargingState != b.ChargingState) ||
                    (a.Current != b.Current) ||
                    (a.RoombaMode != b.RoombaMode) ||
                    (a.Temperature != b.Temperature) ||
                    (a.Voltage != b.Voltage));
        }
        private void UpdateHardwareStateFromEntity(irobottypes.RoombaState _hardwareState)
        {
            DateTime now = DateTime.Now;

            _hardwareState.Pose.ButtonsCreate = 0;
            if (_entity.PlayButtonPressed)
                _hardwareState.Pose.ButtonsCreate |= create.ButtonsCreate.Play;
            if (_entity.AdvanceButtonPressed)
                _hardwareState.Pose.ButtonsCreate |= create.ButtonsCreate.Advance;

            // Update the sensor timestamps
            _hardwareState.LastUpdated = now;
            _hardwareState.CliffDetail.Timestamp = now;
            _hardwareState.Pose.Timestamp = now;
            _hardwareState.Power.Timestamp = now;
            _hardwareState.Sensors.Timestamp = now;
            _hardwareState.Telemetry.Timestamp = now;

            _hardwareState.CliffDetail.CliffLeftSignal = _entity.CliffLeft;
            _hardwareState.CliffDetail.CliffFrontLeftSignal = _entity.CliffFrontLeft;
            _hardwareState.CliffDetail.CliffRightSignal = _entity.CliffRight;
            _hardwareState.CliffDetail.CliffFrontRightSignal = _entity.CliffFrontRight;

            if (_irEntity != null)
            {
                // convert distance value to something close to what the Create reports
                float distance = _irEntity.Distance;
                if (distance > 0.08f)    // range is close to 8 cm
                    distance = 0.08f;
                // distance now ranges from 0 to 0.08, make it range from 1 to 0;
                distance = 1f - (distance / 0.08f);
                // add a little noise to make the signal more realistic and to cause it to update more often
                _hardwareState.CliffDetail.WallSignal = (int)(distance * 200) + (int)(now.Ticks) % 2;
            }

            // bumpers
            _hardwareState.Sensors.BumpsWheeldrops = 0;
            if (_entity.BumperLeft)
                _hardwareState.Sensors.BumpsWheeldrops |= BumpsWheeldrops.BumpLeft;
            if (_entity.BumperRight)
                _hardwareState.Sensors.BumpsWheeldrops |= BumpsWheeldrops.BumpRight;
        }

        /// <summary>
        /// Wait for _tickInterval before calling the TimerHandler
        /// </summary>
        private void WaitForNextTimer()
        {
            // Stop the timer if we are shutting down.
            if (_state.Mode == irobottypes.RoombaMode.Shutdown)
                return;

            // Find the next scheduled time
            _nextTimer = _nextTimer.AddMilliseconds(_tickInterval);

            // grab a moment in time
            DateTime now = DateTime.Now;
            int waitMs = (int)((TimeSpan)_nextTimer.Subtract(now)).TotalMilliseconds;

            // If it's already past time to run, execute the handler ASAP.
            if (waitMs < 10)
                waitMs = 10;

            Activate(Arbiter.Receive(false, TimeoutPort(waitMs), TimerHandler));
        }
        #endregion
    }
    
    #region IR Entity
    /// <summary>
    /// An IR distance sensor.
    /// </summary>
    [DataContract]
    [simengine.RequiresParentAttribute]
    public class IREntity : simengine.VisualEntity
    {
        /// <summary>
        /// angle is in degrees
        /// </summary>
        [DataMember]
        public float DispersionConeAngle = 8f; 
        /// <summary>
        /// the number of rays in each direction
        /// </summary>
        [DataMember]
        public float Samples = 3f;  
        /// <summary>
        /// Default value is 30 inches converted to meters
        /// </summary>
        [DataMember]
        public float MaximumRange = (30f * 2.54f / 100.0f); 

        float _elapsedSinceLastScan;
        Port<simcommon.RaycastResult> _raycastResultsPort;
        simcommon.RaycastResult _lastResults;
        Port<simcommon.RaycastResult> _raycastResults = new Port<simcommon.RaycastResult>();
        simcommon.RaycastProperties _raycastProperties;
        simengine.CachedEffectParameter _timeAttenuationHandle;
        float _appTime;
        simcommon.Shape _particlePlane;

        /// <summary>
        /// Raycast configuration
        /// </summary>
        public simcommon.RaycastProperties RaycastProperties
        {
            get { return _raycastProperties; }
            set { _raycastProperties = value; }
        }

        float _distance;
        /// <summary>
        /// The shortest distance to an impact point
        /// </summary>
        [DataMember]
        public float Distance
        {
            get { return _distance; }
            set { _distance = value; }
        }

        /// <summary>
        /// Default constructor used when this entity is deserialized
        /// </summary>
        public IREntity()
        {
        }

        /// <summary>
        /// Initialization constructor used when this entity is built programmatically
        /// </summary>
        /// <param name="name"></param>
        /// <param name="initialPose"></param>
        public IREntity(string name, Pose initialPose)
        {
            base.State.Name = name;
            base.State.Pose = initialPose;

            // used for rendering impact points
            base.State.Assets.Effect = "LaserRangeFinder.fx";
        }

        /// <summary>
        /// Initialize the IR Distance sensor
        /// </summary>
        /// <param name="device"></param>
        /// <param name="physicsEngine"></param>
        public override void Initialize(xnagrfx.GraphicsDevice device, simcommon.PhysicsEngine physicsEngine)
        {
            try
            {
                if (Parent == null)
                    throw new Exception("This entity must be a child of another entity.");

                // make sure that we take at least 2 samples in each direction
                if (Samples < 2f)
                    Samples = 2f;

                _raycastProperties = new simcommon.RaycastProperties();
                _raycastProperties.StartAngle = -DispersionConeAngle / 2.0f;
                _raycastProperties.EndAngle = DispersionConeAngle / 2.0f;
                _raycastProperties.AngleIncrement = DispersionConeAngle / (Samples - 1f);
                _raycastProperties.Range = MaximumRange;
                _raycastProperties.OriginPose = new Pose();

                // set flag so rendering engine renders us last
                Flags |= simengine.VisualEntityProperties.UsesAlphaBlending;

                base.Initialize(device, physicsEngine);

                // set up for rendering impact points
                simcommon.HeightFieldShapeProperties hf = new simcommon.HeightFieldShapeProperties("height field", 2, 0.02f, 2, 0.02f, 0, 0, 1, 1);
                hf.HeightSamples = new simcommon.HeightFieldSample[hf.RowCount * hf.ColumnCount];
                for (int i = 0; i < hf.HeightSamples.Length; i++)
                    hf.HeightSamples[i] = new simcommon.HeightFieldSample();

                _particlePlane = new simcommon.Shape(hf);
                _particlePlane.State.Name = "laser impact plane";

                // The mesh is used to render the ray impact points rather than the sensor geometry.
                int index = Meshes.Count;
                Meshes.Add(simengine.SimulationEngine.ResourceCache.CreateMesh(device, _particlePlane.State));
                Meshes[0].Textures[0] = simengine.SimulationEngine.ResourceCache.CreateTextureFromFile(device, "particle.bmp");

                // we have a custom effect, with an additional global parameter. Get handle to it here
                if (Effect != null)
                    _timeAttenuationHandle = Effect.GetParameter("timeAttenuation");

            }
            catch (Exception ex)
            {
                // clean up
                if (PhysicsEntity != null)
                    PhysicsEngine.DeleteEntity(PhysicsEntity);

                HasBeenInitialized = false;
                InitError = ex.ToString();
            }
        }

        const float SCAN_INTERVAL = 0.250f;

        /// <summary>
        /// Update the IR distance sensor
        /// </summary>
        /// <param name="update"></param>
        public override void Update(simengine.FrameUpdate update)
        {
            base.Update(update);
            _elapsedSinceLastScan += (float)update.ElapsedTime;
            _appTime = (float)update.ApplicationTime;

            // only retrieve raycast results every SCAN_INTERVAL.
            // For entities that are compute intenisve, you should consider giving them
            // their own task queue so they dont flood a shared queue
            if ((_elapsedSinceLastScan > SCAN_INTERVAL) && (_raycastProperties != null))
            {
                _elapsedSinceLastScan = 0;

                // The default pose has the IR sensor looking toward the back of the robot.  Rotate
                // it by 180 degrees.
                _raycastProperties.OriginPose.Orientation = simengine.TypeConversion.FromXNA(
                    simengine.TypeConversion.ToXNA(Parent.State.Pose.Orientation) *
                    simengine.TypeConversion.ToXNA(State.Pose.Orientation));

                _raycastProperties.OriginPose.Position = simengine.TypeConversion.FromXNA(
                    xna.Vector3.Transform(simengine.TypeConversion.ToXNA(State.Pose.Position), Parent.World));

                xna.Matrix orientation = xna.Matrix.CreateFromQuaternion(simengine.TypeConversion.ToXNA(State.Pose.Orientation));
                World =
                    xna.Matrix.Multiply(orientation,
                    xna.Matrix.CreateTranslation(simengine.TypeConversion.ToXNA(State.Pose.Position)));

                // This entity is relative to its parent
                World = xna.Matrix.Multiply(World, Parent.World);


                // cast rays on a horizontal plane and again on a vertical plane
                _raycastResultsPort = PhysicsEngine.Raycast2D(_raycastProperties);
                _raycastResultsPort.Test(out _lastResults);
                if (_lastResults != null)
                {
                    simcommon.RaycastResult verticalResults;

                    // rotate the plane by 90 degrees
                    _raycastProperties.OriginPose.Orientation =
                        simengine.TypeConversion.FromXNA(simengine.TypeConversion.ToXNA(_raycastProperties.OriginPose.Orientation) *
                        xna.Quaternion.CreateFromAxisAngle(new xna.Vector3(0, 0, 1), (float)Math.PI / 2f));

                    _raycastResultsPort = PhysicsEngine.Raycast2D(_raycastProperties);
                    _raycastResultsPort.Test(out verticalResults);

                    // combine the results of the second raycast with the first
                    if (verticalResults != null)
                    {
                        foreach (simcommon.RaycastImpactPoint impact in verticalResults.ImpactPoints)
                            _lastResults.ImpactPoints.Add(impact);
                    }

                    // find the shortest distance to an impact point
                    float minRange = MaximumRange * MaximumRange;
                    xna.Vector4 origin = new xna.Vector4(simengine.TypeConversion.ToXNA(_raycastProperties.OriginPose.Position), 1);
                    foreach (simcommon.RaycastImpactPoint impact in _lastResults.ImpactPoints)
                    {
                        xna.Vector3 impactVector = new xna.Vector3(
                            impact.Position.X - origin.X,
                            impact.Position.Y - origin.Y,
                            impact.Position.Z - origin.Z);

                        float impactDistanceSquared = impactVector.LengthSquared();
                        if (impactDistanceSquared < minRange)
                            minRange = impactDistanceSquared;
                    }
                    _distance = (float)Math.Sqrt(minRange);
                }
            }
        }
        /// <summary>
        /// Frame render
        /// </summary>
        public override void Render(RenderMode renderMode, simengine.MatrixTransforms transforms, simengine.CameraEntity currentCamera)
        {
            if ((int)(Flags & simengine.VisualEntityProperties.DisableRendering) > 0)
                return;

            if (_lastResults != null)
                RenderResults(renderMode, transforms, currentCamera);
        }

        void RenderResults(RenderMode renderMode, simengine.MatrixTransforms transforms, simengine.CameraEntity currentCamera)
        {
            _timeAttenuationHandle.SetValue(new xna.Vector4(100 * (float)Math.Cos(_appTime * (1.0f / SCAN_INTERVAL)), 0, 0, 1));

            // render impact points as a quad
            xna.Matrix inverseViewRotation = currentCamera.ViewMatrix;
            inverseViewRotation.M41 = inverseViewRotation.M42 = inverseViewRotation.M43 = 0;
            xna.Matrix.Invert(ref inverseViewRotation, out inverseViewRotation);
            xna.Matrix localTransform = xna.Matrix.CreateFromAxisAngle(new xna.Vector3(1, 0, 0), (float)-Math.PI / 2) * inverseViewRotation;

            simengine.SimulationEngine.GlobalInstance.Device.DepthStencilState = xnagrfx.DepthStencilState.DepthRead;
            for (int i = 0; i < _lastResults.ImpactPoints.Count; i++)
            {
                xna.Vector3 pos = new xna.Vector3(_lastResults.ImpactPoints[i].Position.X,
                    _lastResults.ImpactPoints[i].Position.Y,
                    _lastResults.ImpactPoints[i].Position.Z);

                xna.Vector3 resultDir = pos - Position;
                resultDir.Normalize();

                localTransform.Translation = pos - .02f * resultDir;
                transforms.World = localTransform;

                base.Render(renderMode, transforms, Meshes[0]);
            }
            simengine.SimulationEngine.GlobalInstance.Device.DepthStencilState = xnagrfx.DepthStencilState.Default;
        }
    }
    #endregion
}
