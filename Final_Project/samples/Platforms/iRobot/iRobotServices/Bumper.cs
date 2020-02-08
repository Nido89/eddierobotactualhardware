//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: Bumper.cs $ $Revision: 17 $
//-----------------------------------------------------------------------
using Microsoft.Ccr.Core;
using Microsoft.Dss.Core;
using Microsoft.Dss.Core.Attributes;
using Microsoft.Dss.ServiceModel.Dssp;
using Microsoft.Dss.ServiceModel.DsspServiceBase;
using System;
using System.Collections.Generic;
using System.Security.Permissions;

using bumper = Microsoft.Robotics.Services.ContactSensor.Proxy;
using roomba = Microsoft.Robotics.Services.IRobot.Roomba.Proxy;
using submgr = Microsoft.Dss.Services.SubscriptionManager;
using System.ComponentModel;
using sensorupdates = Microsoft.Robotics.Services.IRobot.SensorUpdates.Proxy;
using lite = Microsoft.Robotics.Services.IRobot.Lite.Proxy;

namespace Microsoft.Robotics.Services.IRobot.Roomba.Bumper
{

    /// <summary>
    /// Bumper Service
    /// </summary>
    [Contract(Contract.Identifier)]
    [AlternateContract(bumper.Contract.Identifier)]
    [DisplayName("(User) iRobot� Generic Contact Sensors")]
    [Description("Provides access to an iRobot Roomba or Create's bumpers, wall, and ledge detectors\n(Uses the Generic Contact Sensors contract.)\n(Partner with the 'iRobot� Create / Roomba' service).")]
    public class BumperService : DsspServiceBase
    {
        /// <summary>
        /// Contact Sensor Array State
        /// </summary>
        private bumper.ContactSensorArrayState _state = new Microsoft.Robotics.Services.ContactSensor.Proxy.ContactSensorArrayState();

        /// <summary>
        /// Main Port
        /// </summary>
        [ServicePort("/irobot/bumper", AllowMultipleInstances=true)]
        private bumper.ContactSensorArrayOperations _mainPort = new Microsoft.Robotics.Services.ContactSensor.Proxy.ContactSensorArrayOperations();

        [Partner("iRobotSensorUpdates", Contract = lite.Contract.Identifier,
            CreationPolicy = PartnerCreationPolicy.UseExisting, Optional = false)]
        lite.IRobotLiteOperations _iRobotLitePort = new lite.IRobotLiteOperations();

        lite.IRobotLiteOperations _iRobotSensorUpdates = new lite.IRobotLiteOperations();

        /// <summary>
        /// The subscription manager which keeps track of subscriptions to our Roomba Bumper data
        /// </summary>
        [Partner("SubMgr", Contract = submgr.Contract.Identifier,
            CreationPolicy = PartnerCreationPolicy.CreateAlways)]
        submgr.SubscriptionManagerPort _subMgrPort = new submgr.SubscriptionManagerPort();

        /// <summary>
        /// Default Service Constructor
        /// </summary>
        public BumperService(DsspServiceCreationPort creationPort) :
                base(creationPort)
        {
        }

        /// <summary>
        /// Service Start
        /// </summary>
        protected override void Start()
        {
            ValidateState();

            // Listen on the main port for requests and call the appropriate handler.
            ActivateDsspOperationHandlers();

            // Publish the service to the local service Directory
            DirectoryInsert();

            MainPortInterleave.CombineWith(new Interleave(
                new ExclusiveReceiverGroup(),
                new ConcurrentReceiverGroup(
                    Arbiter.Receive<sensorupdates.UpdateBumpsCliffsAndWalls>(true, _iRobotSensorUpdates, ReturnSensorsHandler)
                )));

            // Subscribe to the iRobot sensors
            _iRobotLitePort.Subscribe(_iRobotSensorUpdates, typeof(sensorupdates.UpdateBumpsCliffsAndWalls));

            // display HTTP service Uri
            LogInfo(LogGroups.Console, "Service uri: ");
        }

        private void ValidateState()
        {
            //configure initial state
            if (_state == null)
                _state = new bumper.ContactSensorArrayState();

            if (_state.Sensors == null)
                _state.Sensors = new List<bumper.ContactSensor>();

            if (_state.Sensors.Count == 0)
            {
                _state.Sensors.Add(new bumper.ContactSensor((int)RoombaBumpers.LeftBumper, "Front Left Bumper"));
                _state.Sensors.Add(new bumper.ContactSensor((int)RoombaBumpers.RightBumper, "Front Right Bumper"));
                _state.Sensors.Add(new bumper.ContactSensor((int)RoombaBumpers.Wall, "Wall"));
                _state.Sensors.Add(new bumper.ContactSensor((int)RoombaBumpers.VirtualWall, "Virtual Wall"));
                _state.Sensors.Add(new bumper.ContactSensor((int)RoombaBumpers.CliffLeft, "Cliff Left"));
                _state.Sensors.Add(new bumper.ContactSensor((int)RoombaBumpers.CliffFrontLeft, "Cliff Front Left"));
                _state.Sensors.Add(new bumper.ContactSensor((int)RoombaBumpers.CliffFrontRight, "Cliff Front Right"));
                _state.Sensors.Add(new bumper.ContactSensor((int)RoombaBumpers.CliffRight, "Cliff Right"));
                _state.Sensors.Add(new bumper.ContactSensor((int)RoombaBumpers.WheelDropLeft, "Left Wheel Drop"));
                _state.Sensors.Add(new bumper.ContactSensor((int)RoombaBumpers.WheelDropRight, "Right Wheel Drop"));
                _state.Sensors.Add(new bumper.ContactSensor((int)RoombaBumpers.WheelDropRear, "Rear Wheel Drop"));
                _state.Sensors.Add(new bumper.ContactSensor(99, "Wheels Stalled"));

                SaveState(_state);
            }
        }

        /// <summary>
        /// Get Handler
        /// </summary>
        /// <param name="get"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public virtual IEnumerator<ITask> GetHandler(bumper.Get get)
        {
            get.ResponsePort.Post(_state);
            yield break;
        }

        /// <summary>
        /// Replace Handler
        /// </summary>
        /// <param name="replace"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public virtual IEnumerator<ITask> ReplaceHandler(bumper.Replace replace)
        {
            _state = replace.Body;
            replace.ResponsePort.Post(DefaultReplaceResponseType.Instance);
            yield break;
        }

        /// <summary>
        /// Update Handler
        /// </summary>
        /// <param name="update"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public virtual IEnumerator<ITask> UpdateHandler(bumper.Update update)
        {
            bool replace = false;
            DateTime timestamp = DateTime.Now;

            update.ResponsePort.Post(DefaultUpdateResponseType.Instance);

            foreach (bumper.ContactSensor contactSensor in _state.Sensors)
            {
                if (contactSensor.HardwareIdentifier == update.Body.HardwareIdentifier)
                {
                    contactSensor.Name = update.Body.Name;
                    contactSensor.Pressed = update.Body.Pressed;
                    contactSensor.TimeStamp = timestamp;

                    SendNotification<bumper.Update>(_subMgrPort, new bumper.Update(contactSensor));

                    replace = true;
                }
            }

            if (replace)
            {
                SendNotification<bumper.Replace>(_subMgrPort, new bumper.Replace(_state));
            }
            yield break;
        }

        /// <summary>
        /// Subscribe Handler
        /// </summary>
        /// <param name="subscribe"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public virtual IEnumerator<ITask> SubscribeHandler(bumper.Subscribe subscribe)
        {
            SubscribeHelper(_subMgrPort, subscribe.Body, subscribe.ResponsePort);
            yield break;
        }

        /// <summary>
        /// ReliableSubscribe Handler
        /// </summary>
        /// <param name="subscribe"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public virtual IEnumerator<ITask> ReliableSubscribeHandler(bumper.ReliableSubscribe subscribe)
        {
            SubscribeHelper(_subMgrPort, subscribe.Body, subscribe.ResponsePort);
            yield break;
        }

        #region Roomba Sensors Notifications Handlers

        private void ReturnAllHandler(sensorupdates.UpdateAll notify)
        {
            if (_state.Sensors == null)
                return;

            ProcessSensors(notify.Body.Sensors);
        }

        private void ProcessSensors(roomba.ReturnSensors sensors)
        {
            foreach (bumper.ContactSensor bumper in _state.Sensors)
            {
                bool bump = false;
                // Find the Roomba sensor which corresponds to this bumper
                switch ((RoombaBumpers)bumper.HardwareIdentifier)
                {
                    case RoombaBumpers.LeftBumper:
                        bump = ((sensors.BumpsWheeldrops & roomba.BumpsWheeldrops.BumpLeft) == roomba.BumpsWheeldrops.BumpLeft);
                        break;
                    case RoombaBumpers.RightBumper:
                        bump = ((sensors.BumpsWheeldrops & roomba.BumpsWheeldrops.BumpRight) == roomba.BumpsWheeldrops.BumpRight);
                        break;
                    case RoombaBumpers.Wall:
                        bump = sensors.Wall;
                        break;
                    case RoombaBumpers.VirtualWall:
                        bump = sensors.VirtualWall;
                        break;
                    case RoombaBumpers.CliffLeft:
                        bump = sensors.CliffLeft;
                        break;
                    case RoombaBumpers.CliffFrontLeft:
                        bump = sensors.CliffFrontLeft;
                        break;
                    case RoombaBumpers.CliffFrontRight:
                        bump = sensors.CliffFrontRight;
                        break;
                    case RoombaBumpers.CliffRight:
                        bump = sensors.CliffRight;
                        break;
                    case RoombaBumpers.WheelDropLeft:
                        bump = ((sensors.BumpsWheeldrops & roomba.BumpsWheeldrops.WheelDropLeft) == roomba.BumpsWheeldrops.WheelDropLeft);
                        break;
                    case RoombaBumpers.WheelDropRight:
                        bump = ((sensors.BumpsWheeldrops & roomba.BumpsWheeldrops.WheelDropRight) == roomba.BumpsWheeldrops.WheelDropRight);
                        break;
                    case RoombaBumpers.WheelDropRear:
                        bump = ((sensors.BumpsWheeldrops & roomba.BumpsWheeldrops.WheelDropCaster) == roomba.BumpsWheeldrops.WheelDropCaster);
                        break;
                    case (RoombaBumpers)99: // wheels stalled
                        bump = ((int)(sensors.MotorOvercurrents & (roomba.MotorOvercurrents.DriveLeft | roomba.MotorOvercurrents.DriveRight)) != 0);
                        break;
                }

                if (bumper.Pressed != bump)
                {
                    bumper.Pressed = bump;
                    this.SendNotification<bumper.Update>(_subMgrPort, new bumper.Update(bumper));
                }

            }
        }

        private void ReturnSensorsHandler(sensorupdates.UpdateBumpsCliffsAndWalls notify)
        {
            ProcessSensors(notify.Body);
        }

        #endregion

    }
}
