//------------------------------------------------------------------------------
//  <copyright file="MarkRobot.cs" company="Microsoft Corporation">
//      Copyright (C) Microsoft Corporation.  All rights reserved.
//  </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Robotics.ReferencePlatform.MarkRobot
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Text;

    using Microsoft.Ccr.Core;
    using Microsoft.Dss.Core.Attributes;
    using Microsoft.Dss.Core.DsspHttp;
    using Microsoft.Dss.ServiceModel.Dssp;
    using Microsoft.Dss.ServiceModel.DsspServiceBase;
    
    using battery = Microsoft.Robotics.Services.Battery;
    using drive = Microsoft.Robotics.Services.Drive;
    using ir = Microsoft.Robotics.Services.InfraredSensorArray;
    using motor = Microsoft.Robotics.Services.Motor;
    using physmod = Microsoft.Robotics.PhysicalModel;
    using soap = W3C.Soap;
    using sonar = Microsoft.Robotics.Services.SonarSensorArray;
    using submgr = Microsoft.Dss.Services.SubscriptionManager;

    /// <summary>
    /// Main class for the service
    /// </summary>
    [Contract(Contract.Identifier)]
    [AlternateContract(drive.Contract.Identifier)]
    [AlternateContract(ir.Contract.Identifier)]
    [AlternateContract(sonar.Contract.Identifier)]
    [AlternateContract(battery.Contract.Identifier)]
    [DisplayName("(User) MarkRobot")]
    [Description("Mobile autonomous robot using Kinect service for the 2011 Reference Platform")]
    public partial class MarkRobotService : DsspServiceBase
    {
        /// <summary>
        /// Default IR array hardware identifiers
        /// </summary>
        private static readonly int[] DefaultInfraredArrayIdentifiers = { 2, 1, 0 };

        /// <summary>
        /// Default Sonar array hardware identifiers
        /// </summary>
        private static readonly int[] DefaultSonarArrayIdentifiers = { 9, 8 };

        /// <summary>
        /// Default maximum battery power - assuming 12v battery
        /// </summary>
        private const int DefaultMaxBatteryPower = 12;

        /// <summary>
        /// Service state
        /// </summary>
        [ServiceState]
        [InitialStatePartner(Optional = true)]
        private MarkRobotState state;

        /// <summary>
        /// Initialization flag for service startup
        /// </summary>
        private bool initialized = false;

        /// <summary>
        /// Main service subscription port
        /// </summary>
        [SubscriptionManagerPartner]
        private submgr.SubscriptionManagerPort submgrPort = new submgr.SubscriptionManagerPort();

        /// <summary>
        /// Main service port
        /// </summary>
        [ServicePort("/MarkRobot", AllowMultipleInstances = false)]
        private MarkRobotOperations mainPort = new MarkRobotOperations();

        /// <summary>
        /// Initializes a new instance of MarkRobotService class
        /// </summary>
        /// <param name="creationPort">Instance of type DsspServiceCreationPort</param>
        public MarkRobotService(DsspServiceCreationPort creationPort)
            : base(creationPort)
        {
        }

        /// <summary>
        /// Allocation and assignments for first run
        /// </summary>
        /// <returns>True if initialization succeeded, otherwise False</returns>
        private bool Initialize()
        {
            if (this.initialized)
            {
                return this.initialized;
            }

            try
            {
                // No persisted state file, create a new one
                if (this.state == null)
                {
                    this.state = new MarkRobotState();
                }

                // Populate the IR sensor state
                if (this.state.InfraredSensorState == null)
                {
                    this.state.InfraredSensorState = new ir.InfraredSensorArrayState();
                }

                if (this.state.InfraredSensorState.Sensors == null)
                {
                    this.state.InfraredSensorState.Sensors = new List<Services.Infrared.InfraredState>();
                }

                if (this.state.InfraredSensorState.Sensors.Count == 0)
                {
                    foreach (int irIdentifier in DefaultInfraredArrayIdentifiers)
                    {
                        this.state.InfraredSensorState.Sensors.Add(new Services.Infrared.InfraredState() { HardwareIdentifier = irIdentifier });
                    }
                }

                // Populate the Sonar sensor state
                if (this.state.SonarSensorState == null)
                {
                    this.state.SonarSensorState = new sonar.SonarSensorArrayState();
                }

                if (this.state.SonarSensorState.Sensors == null)
                {
                    this.state.SonarSensorState.Sensors = new List<Services.Sonar.SonarState>();
                }

                if (this.state.SonarSensorState.Sensors.Count == 0)
                {
                    foreach (int sonarIdentifier in DefaultSonarArrayIdentifiers)
                    {
                        this.state.SonarSensorState.Sensors.Add(new Services.Sonar.SonarState() { HardwareIdentifier = sonarIdentifier });
                    }
                }

                this.state.DriveState = new drive.DriveDifferentialTwoWheelState();

                if (this.state.BatteryState == null)
                {
                    this.state.BatteryState = new Services.Battery.BatteryState();
                }

                if (this.state.BatteryState.MaxBatteryPower == 0)
                {
                    this.state.BatteryState.MaxBatteryPower = DefaultMaxBatteryPower;
                }
            }
            catch (Exception e)
            {
                LogError(e);
                this.Shutdown();
                return false;
            }

            this.state.LastStartTime = DateTime.Now;
            
            SaveState(this.state);

            base.Start();

            // Make sure the pin polling port is in the main interleave because it modifies service state
            MainPortInterleave.CombineWith(
                                           new Interleave(
                                           new ExclusiveReceiverGroup(
                                           Arbiter.ReceiveWithIterator(true, this.sensorPollingPort, this.PollSensors)),
                                           new ConcurrentReceiverGroup(
                                           Arbiter.Receive<drive.Update>(true, this.driveNotifyPort, this.DriveNotification))));

            this.controllerDrivePort.Post(new drive.Subscribe() { NotificationPort = this.driveNotifyPort });
            this.controllerDrivePort.Post(new drive.ReliableSubscribe() { NotificationPort = this.driveNotifyPort });

            // Start the pin polling interval
            this.sensorPollingPort.Post(DateTime.Now);
            
            return true;
        }

        /// <summary>
        /// Retrieve the COM port specified in the SerialCOMService state.
        /// Perform initialization if COM port is correct and available.
        /// </summary>
        /// <returns>Enumerator of type ITask</returns>
        private IEnumerator<ITask> InternalInitialize()
        {
            this.initialized = this.Initialize();

            yield break;
        }

        /// <summary>
        /// Initialize the servive
        /// </summary>
        protected override void Start()
        {
            if (!this.initialized)
            {
                SpawnIterator(this.InternalInitialize);
            }
        }

        /// <summary>
        /// Handler for GET operations
        /// </summary>
        /// <param name="get">A GET instance</param>
        /// <returns>A CCR task enumerator</returns>
        [ServiceHandler]
        public IEnumerator<ITask> GetHandler(Get get)
        {
            MarkRobotState getState = new MarkRobotState();
            drive.Get driveGet = new drive.Get();

            getState.DriveState = new drive.DriveDifferentialTwoWheelState();

            // Get the up-to-date drive state
            this.controllerDrivePort.Post(driveGet);
            yield return driveGet.ResponsePort.Choice(s => getState.DriveState = s, EmptyHandler);

            // MarkRobot service specific configuration
            getState.SonarTimeValueMultiplier = this.state.SonarTimeValueMultiplier;
            getState.SensorPollingInterval = this.state.SensorPollingInterval;
            getState.LastStartTime = this.state.LastStartTime;
            getState.InfraredRawValueDivisorScalar = this.state.InfraredRawValueDivisorScalar;
            getState.InfraredDistanceExponent = this.state.InfraredDistanceExponent;
            getState.BatteryVoltagePinIndex = this.state.BatteryVoltagePinIndex;
            getState.BatteryVoltageDivider = this.state.BatteryVoltageDivider;
            
            // Polled state
            getState.BatteryState = this.state.BatteryState;
            getState.InfraredSensorState = this.state.InfraredSensorState;
            getState.SonarSensorState = this.state.SonarSensorState;
            
            get.ResponsePort.Post(getState);
        }

        /// <summary>
        /// HTTP GET handler
        /// </summary>
        /// <param name="httpGet">HTTP Get message</param>
        /// <returns>A CCR task iterator</returns>
        [ServiceHandler]
        public IEnumerator<ITask> HttpGetHandler(HttpGet httpGet)
        {
            Get get = new Get();
            soap.Fault fault = null;

            this.mainPort.Post(get);

            yield return get.ResponsePort.Choice();

            fault = (soap.Fault)get.ResponsePort;

            if (fault != null)
            {
                httpGet.ResponsePort.Post(new HttpResponseType(System.Net.HttpStatusCode.InternalServerError, fault));
            }
            else
            {
                httpGet.ResponsePort.Post(new HttpResponseType(System.Net.HttpStatusCode.OK, (MarkRobotState)get.ResponsePort));
            }

            yield break;
        }

        /// <summary>
        /// Drop handler
        /// </summary>
        /// <param name="drop">DSS default drop type</param>
        [ServiceHandler]
        public void DropHandler(DsspDefaultDrop drop)
        {
            this.DefaultDropHandler(drop);
        }

        /// <summary>
        /// Handles Subscribe messages
        /// </summary>
        /// <param name="subscribe">The subscribe request</param>
        /// <returns>Enumerator of type ITask</returns>
        [ServiceHandler]
        public IEnumerator<ITask> SubscribeHandler(Subscribe subscribe)
        {
            SuccessFailurePort responsePort = SubscribeHelper(this.submgrPort, subscribe.Body, subscribe.ResponsePort);
            yield return responsePort.Choice();

            var success = (SuccessResult)responsePort;
            if (success != null)
            {
                SendNotificationToTarget<Replace>(subscribe.Body.Subscriber, this.submgrPort, this.state);
            }

            yield break;
        }
    }
}
