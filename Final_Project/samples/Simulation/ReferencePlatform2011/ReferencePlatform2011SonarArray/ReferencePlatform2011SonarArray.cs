//------------------------------------------------------------------------------
//  <copyright file="ReferencePlatform2011SonarArray.cs" company="Microsoft Corporation">
//      Copyright (C) Microsoft Corporation.  All rights reserved.
//  </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Robotics.Services.Simulation.ReferencePlatform2011.SonarArray
{
    using System.Collections.Generic;
    using System.ComponentModel;

    using Microsoft.Ccr.Core;
    using Microsoft.Dss.Core.Attributes;
    using Microsoft.Dss.ServiceModel.Dssp;

    using W3C.Soap;

    using analogarray = Microsoft.Robotics.Services.AnalogSensorArray.Proxy;
    using analogsensor = Microsoft.Robotics.Services.AnalogSensor.Proxy;
    using dssphttp = Microsoft.Dss.Core.DsspHttp;
    using sonar = Microsoft.Robotics.Services.Sonar;
    using sonararray = Microsoft.Robotics.Services.SonarSensorArray.Proxy;
    using sonarsensor = Microsoft.Robotics.Services.Sonar.Proxy;
    using submgr = Microsoft.Dss.Services.SubscriptionManager;
    using svcbase = Microsoft.Dss.ServiceModel.DsspServiceBase;

    /// <summary>
    /// IrSensor enumeration
    /// </summary>
    [Description("Sonar proximity sensors.")]
    public enum SonarSensors
    {
        /// <summary>
        /// Proximity sensor 1
        /// </summary>
        LeftSonarProximityInMeters,

        /// <summary>
        /// Proximity sensor 2
        /// </summary>
        RightSonarProximityInMeters,
        
        /// <summary>
        /// Total number of sensors in the enum
        /// </summary>
        /// <remarks>Must be last element in
        /// enum.</remarks>
        SensorCount,
    }

    /// <summary>
    /// Encapsulates a pair of sonars
    /// </summary>
    [Contract(Contract.Identifier)]
    [DisplayName("(User) ReferencePlatform2011SonarArray")]
    [Description("ReferencePlatform2011SonarArray service ")]
    [AlternateContract(analogarray.Contract.Identifier)]
    [AlternateContract(sonararray.Contract.Identifier)]
    public class ReferencePlatform2011SonarArrayService : svcbase.DsspServiceBase
    {
        /// <summary>
        /// Name of the alternate service port
        /// </summary>
        private const string AlternatePort = "altPort";

        /// <summary>
        /// Sensor ports
        /// </summary>
        private sonar.SonarOperations[] sensorPorts = new sonar.SonarOperations[(int)SonarSensors.SensorCount];
        
        /// <summary>
        /// The main port
        /// </summary>
        [ServicePort("/ReferencePlatform2011SonarArray", AllowMultipleInstances = true)]
        private analogarray.AnalogSensorOperations mainPort = new analogarray.AnalogSensorOperations();

        /// <summary>
        /// The alternate port
        /// </summary>
        [AlternateServicePort("/ReferencePlatform2011SonarStateArray", AlternateContract = sonararray.Contract.Identifier, AllowMultipleInstances = true)]
        private sonararray.SonarSensorOperations altPort = new sonararray.SonarSensorOperations();

        /// <summary>
        /// Subscription manager port
        /// </summary>
        [SubscriptionManagerPartner]
        private submgr.SubscriptionManagerPort subMgrPort = new submgr.SubscriptionManagerPort();
        
        /// <summary>
        /// Left sonar
        /// </summary>
        [Partner("SonarLeft", Contract = sonar.Contract.Identifier, CreationPolicy = PartnerCreationPolicy.UsePartnerListEntry)]
        private sonar.SonarOperations leftSonar = new sonar.SonarOperations();

        /// <summary>
        /// Right sonar
        /// </summary>
        [Partner("SonarRight", Contract = sonar.Contract.Identifier, CreationPolicy = PartnerCreationPolicy.UsePartnerListEntry)]
        private sonar.SonarOperations rightSonar = new sonar.SonarOperations();

        /// <summary>
        /// Constructs a ReferencePlatform2011SonarArrayService
        /// </summary>
        /// <param name="creationPort">The port of creation</param>
        public ReferencePlatform2011SonarArrayService(DsspServiceCreationPort creationPort)
            : base(creationPort)
        {
        }

        /// <summary>
        /// HttpGet AnalogSensor Handler
        /// </summary>
        /// <param name="httpGet">HTTP Get request</param>
        /// <returns>A CCR iterator</returns>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public virtual IEnumerator<ITask> HttpGetAnalogSensorHandler(dssphttp.HttpGet httpGet)
        {
            analogarray.Get get = new analogarray.Get();
            this.mainPort.Post(get);
            yield return get.ResponsePort.Choice();
            analogarray.AnalogSensorArrayState analogState = get.ResponsePort;
            httpGet.ResponsePort.Post(new dssphttp.HttpResponseType(analogState));
            yield break;
        }

        /// <summary>
        /// HttpGet SonarSensorArray Handler
        /// </summary>
        /// <param name="httpGet">HTTP Get request</param>
        /// <returns>A CCR iterator</returns>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent, PortFieldName = AlternatePort)]
        public virtual IEnumerator<ITask> HttpGetSonarSensorHandler(dssphttp.HttpGet httpGet)
        {
            sonararray.Get get = new sonararray.Get();
            this.altPort.Post(get);
            yield return get.ResponsePort.Choice();
            sonararray.SonarSensorArrayState sonararrayState = get.ResponsePort;
            httpGet.ResponsePort.Post(new dssphttp.HttpResponseType(sonararrayState));
            yield break;
        }

        /// <summary>
        /// Get the state
        /// </summary>
        /// <param name="get">The get message</param>
        /// <returns>A CCR iterator</returns>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public IEnumerator<ITask> GetHandler(analogarray.Get get)
        {
            var state = new analogarray.AnalogSensorArrayState();
            state.Sensors = new List<analogsensor.AnalogSensorState>(this.sensorPorts.Length);

            // compose state on the fly by issuing N parallel GET requests to partners
            var getSensorState = new sonar.Get();

            foreach (var port in this.sensorPorts)
            {
                state.Sensors.Add(null);
                port.Post(getSensorState);
            }

            ICollection<sonar.SonarState> sensorStates = null;
            ICollection<Fault> faults = null;
            yield return getSensorState.ResponsePort.MultipleItemReceive(
                this.sensorPorts.Length,
                (s, f) =>
                {
                    sensorStates = s;
                    faults = f;
                });

            if (faults != null && faults.Count > 0)
            {
                get.ResponsePort.Post(Fault.FromCodeSubcode(FaultCodes.Receiver, DsspFaultCodes.OperationFailed));
                yield break;
            }

            foreach (var sensorState in sensorStates)
            {
                state.Sensors[sensorState.HardwareIdentifier] = new analogsensor.AnalogSensorState()
                {
                    HardwareIdentifier = sensorState.HardwareIdentifier,
                    NormalizedMeasurement = sensorState.DistanceMeasurement,
                    RawMeasurement = sensorState.DistanceMeasurement,
                    RawMeasurementRange = sensorState.MaxDistance,
                    Pose = new PhysicalModel.Proxy.Pose
                    {
                        Orientation = new PhysicalModel.Proxy.Quaternion(
                            sensorState.Pose.Orientation.X,
                            sensorState.Pose.Orientation.Y,
                            sensorState.Pose.Orientation.Z,
                            sensorState.Pose.Orientation.W),
                        Position = new PhysicalModel.Proxy.Vector3(
                            sensorState.Pose.Position.X,
                            sensorState.Pose.Position.Y,
                            sensorState.Pose.Position.Z),
                    },
                    TimeStamp = sensorState.TimeStamp,
                };
            }

            var stateClone = state.Clone() as analogarray.AnalogSensorArrayState;
            get.ResponsePort.Post(stateClone);
            yield break;
        }

        /// <summary>
        /// Get the state
        /// </summary>
        /// <param name="get">The get message</param>
        /// <returns>A CCR iterator</returns>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent, PortFieldName = AlternatePort)]
        public IEnumerator<ITask> GetSonarArrayHandler(sonararray.Get get)
        {
            var state = new sonararray.SonarSensorArrayState();

            state.Sensors = new List<sonarsensor.SonarState>(this.sensorPorts.Length);

            // compose state on the fly by issuing N parallel GET requests to partners
            var getSensorState = new sonar.Get();

            foreach (var port in this.sensorPorts)
            {
                state.Sensors.Add(null);
                port.Post(getSensorState);
            }

            ICollection<sonar.SonarState> sensorStates = null;
            ICollection<Fault> faults = null;
            yield return getSensorState.ResponsePort.MultipleItemReceive(
                this.sensorPorts.Length,
                (s, f) =>
                {
                    sensorStates = s;
                    faults = f;
                });

            if (faults != null && faults.Count > 0)
            {
                get.ResponsePort.Post(Fault.FromCodeSubcode(FaultCodes.Receiver, DsspFaultCodes.OperationFailed));
                yield break;
            }

            foreach (var sensorState in sensorStates)
            {
                state.Sensors[sensorState.HardwareIdentifier] = new sonarsensor.SonarState()
                {
                    HardwareIdentifier = sensorState.HardwareIdentifier,
                    AngularRange = sensorState.AngularRange,
                    AngularResolution = sensorState.AngularResolution,
                    DistanceMeasurement = sensorState.DistanceMeasurement,
                    DistanceMeasurements = sensorState.DistanceMeasurements,
                    MaxDistance = sensorState.MaxDistance,
                    Pose = new PhysicalModel.Proxy.Pose
                    {
                        Orientation = new PhysicalModel.Proxy.Quaternion(
                            sensorState.Pose.Orientation.X,
                            sensorState.Pose.Orientation.Y,
                            sensorState.Pose.Orientation.Z,
                            sensorState.Pose.Orientation.W),
                        Position = new PhysicalModel.Proxy.Vector3(
                            sensorState.Pose.Position.X,
                            sensorState.Pose.Position.Y,
                            sensorState.Pose.Position.Z),
                    },
                    TimeStamp = sensorState.TimeStamp,
                };
            }

            var stateClone = state.Clone() as sonararray.SonarSensorArrayState;
            get.ResponsePort.Post(stateClone);
            yield break;
        }

        /// <summary>
        /// Service start routine
        /// </summary>
        protected override void Start()
        {
            this.CreateDefaultState();
            TaskQueue.Enqueue(new IterativeTask(() => this.ConfigureSensors()));
        }

        /// <summary>
        /// Configure sensors
        /// </summary>
        /// <returns>A CCR iterator</returns>
        private IEnumerator<ITask> ConfigureSensors()
        {
            int i = 0;

            // get each sensor's current state
            // and assign hardware identifiers
            var replaceSensorState = new sonar.Replace();
            var get = new sonar.Get();
            foreach (var sensor in this.sensorPorts)
            {
                this.sensorPorts[i].Post(get);
                yield return get.ResponsePort.Choice();

                sonar.SonarState sensorState = get.ResponsePort;
                if (sensorState != null)
                {
                    replaceSensorState.Body = sensorState;
                }
                else
                {
                    replaceSensorState.Body = new sonar.SonarState();
                }

                replaceSensorState.Body.HardwareIdentifier = i;
                sensor.Post(replaceSensorState);
                i++;
            }

            ICollection<Fault> faults = null;
            yield return replaceSensorState.ResponsePort.MultipleItemReceive(
                this.sensorPorts.Length,
                (successes, f) =>
                {
                    faults = f;
                });

            if (faults != null && faults.Count > 0)
            {
                LogError("Failure configuring IR sensors");
                this.mainPort.Post(new DsspDefaultDrop());
            }

            this.StartAfterConfigure();
        }

        /// <summary>
        /// Start after configure
        /// </summary>
        private void StartAfterConfigure()
        {
            base.Start();
        }

        /// <summary>
        /// Create default state
        /// </summary>
        private void CreateDefaultState()
        {
            this.sensorPorts[(int)SonarSensors.LeftSonarProximityInMeters] = this.leftSonar;
            this.sensorPorts[(int)SonarSensors.RightSonarProximityInMeters] = this.rightSonar;
        }
    }
}