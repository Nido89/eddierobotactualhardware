//------------------------------------------------------------------------------
//  <copyright file="ReferencePlatform2011IRArray.cs" company="Microsoft Corporation">
//      Copyright (C) Microsoft Corporation.  All rights reserved.
//  </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Robotics.Services.Simulation.ReferencePlatform2011IRArray
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using Microsoft.Ccr.Core;
    using Microsoft.Dss.Core.Attributes;
    using Microsoft.Dss.ServiceModel.Dssp;
    using Microsoft.Dss.ServiceModel.DsspServiceBase;
    using W3C.Soap;
    using analogarray = Microsoft.Robotics.Services.AnalogSensorArray.Proxy;
    using analogsensor = Microsoft.Robotics.Services.AnalogSensor.Proxy;
    using dssphttp = Microsoft.Dss.Core.DsspHttp;
    using infraredarray = Microsoft.Robotics.Services.InfraredSensorArray.Proxy;
    using infraredsensor = Microsoft.Robotics.Services.Infrared.Proxy;
    using submgr = Microsoft.Dss.Services.SubscriptionManager;

    /// <summary>
    /// IrSensor enumeration
    /// </summary>
    [Description("Infra-red proximity sensors.")]
    public enum IrSensors
    {
        /// <summary>
        /// Proximity sensor 1
        /// </summary>
        LeftFrontProximityInMeters,
        
        /// <summary>
        /// Proximity sensor 2
        /// </summary>
        CenterFrontProximityInMeters,
        
        /// <summary>
        /// Proximity sensor 3
        /// </summary>
        RightFrontProximityInMeters,

        /// <summary>
        /// Total number of sensors in the enum
        /// </summary>
        /// <remarks>Must be last element in
        /// enum.</remarks>
        SensorCount,
    }
    
    /// <summary>
    /// This service encapsulates a set of three IR sensors
    /// </summary>
    [Contract(Contract.Identifier)]
    [DisplayName("(User) ReferencePlatform2011IRArray")]
    [Description("ReferencePlatform2011IRArray service (no description provided)")]
    [AlternateContract(analogarray.Contract.Identifier)]
    [AlternateContract(infraredarray.Contract.Identifier)]
    public class ReferencePlatform2011IRArrayService : DsspServiceBase
    {
        /// <summary>
        /// Pose of the left ir sensor
        /// </summary>
        private readonly PhysicalModel.Proxy.Pose LeftIRPose = new PhysicalModel.Proxy.Pose { Position = new PhysicalModel.Proxy.Vector3(-0.16f, 0.15f, 0.12f), Orientation = new PhysicalModel.Proxy.Quaternion(0, -0.07991469f, 0, 0.996801734f) };

        /// <summary>
        /// Pose of the middle ir sensor
        /// </summary>
        private readonly PhysicalModel.Proxy.Pose MiddleIRPose = new PhysicalModel.Proxy.Pose { Position = new PhysicalModel.Proxy.Vector3(0, 0.15f, 0.22f), Orientation = new PhysicalModel.Proxy.Quaternion(0, 0, 0, 1) };

        /// <summary>
        /// Pose of the right ir sensor
        /// </summary>
        private readonly PhysicalModel.Proxy.Pose RightIRPose = new PhysicalModel.Proxy.Pose { Position = new PhysicalModel.Proxy.Vector3(.16f, 0.15f, 0.12f), Orientation = new PhysicalModel.Proxy.Quaternion(0, 0.07991469f, 0, 0.996801734f) };

        /// <summary>
        /// Manufacturer for simulation IR sensor
        /// </summary>
        private const string SimSensorManufacturer = "Simulation Infrared Sensor";

        /// <summary>
        /// Name of the alternate service port
        /// </summary>
        private const string AlternatePort = "altPort";

        /// <summary>
        /// Sensor ports
        /// </summary>
        private analogsensor.AnalogSensorOperations[] sensorPorts = new analogsensor.AnalogSensorOperations[(int)IrSensors.SensorCount];

        /// <summary>
        /// Main port for operations
        /// </summary>
        [ServicePort("/ReferencePlatform2011IRArray", AllowMultipleInstances = true)]
        private analogarray.AnalogSensorOperations mainPort = new analogarray.AnalogSensorOperations();

        /// <summary>
        /// Alternate port for operations
        /// </summary>
        [AlternateServicePort("/ReferencePlatform2011IRSensorArray", AlternateContract = infraredarray.Contract.Identifier, AllowMultipleInstances = true)]
        private infraredarray.InfraredSensorOperations altPort   = new infraredarray.InfraredSensorOperations();

        /// <summary>
        /// Subscription manager port
        /// </summary>
        [SubscriptionManagerPartner]
        private submgr.SubscriptionManagerPort subMgrPort = new submgr.SubscriptionManagerPort();

        /// <summary>
        /// Front middle left proximity sensor port
        /// </summary>
        [Partner("FrontLeftIR", Contract = analogsensor.Contract.Identifier, CreationPolicy = PartnerCreationPolicy.UsePartnerListEntry)]
        private analogsensor.AnalogSensorOperations frontLeftSensorPort = new analogsensor.AnalogSensorOperations();

        /// <summary>
        /// Front middle proximity sensor port
        /// </summary>
        [Partner("FrontMiddleIR", Contract = analogsensor.Contract.Identifier, CreationPolicy = PartnerCreationPolicy.UsePartnerListEntry)]
        private analogsensor.AnalogSensorOperations frontMiddleSensorPort = new analogsensor.AnalogSensorOperations();

        /// <summary>
        /// Front middle right proximity sensor port
        /// </summary>
        [Partner("FrontRightIR", Contract = analogsensor.Contract.Identifier, CreationPolicy = PartnerCreationPolicy.UsePartnerListEntry)]
        private analogsensor.AnalogSensorOperations frontRightSensorPort = new analogsensor.AnalogSensorOperations();
        
        /// <summary>
        /// Constructs a ReferencePlatform2011IRArrayService
        /// </summary>
        /// <param name="creationPort">The port of creation</param>
        public ReferencePlatform2011IRArrayService(DsspServiceCreationPort creationPort)
            : base(creationPort)
        {
        }

        /// <summary>
        /// HttpGet AnalogSensor Handler
        /// </summary>
        /// <param name="httpGet">HTTP get request</param>
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
        /// HttpGet InfraredSensor Handler
        /// </summary>
        /// <param name="httpGet">HTTP get request</param>
        /// <returns>A CCR iterator</returns>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent, PortFieldName = AlternatePort)]
        public virtual IEnumerator<ITask> HttpGetInfraredSensorHandler(dssphttp.HttpGet httpGet)
        {
            infraredarray.Get get = new infraredarray.Get();
            this.altPort.Post(get);
            yield return get.ResponsePort.Choice();
            infraredarray.InfraredSensorArrayState infraredState = get.ResponsePort;
            httpGet.ResponsePort.Post(new dssphttp.HttpResponseType(infraredState));
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
            var getSensorState = new analogsensor.Get();

            foreach (var port in this.sensorPorts)
            {
                state.Sensors.Add(null);
                port.Post(getSensorState);
            }

            ICollection<analogsensor.AnalogSensorState> sensorStates = null;
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
                state.Sensors[sensorState.HardwareIdentifier] = sensorState;
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
        public IEnumerator<ITask> GetHandler(infraredarray.Get get)
        {
            var state = new infraredarray.InfraredSensorArrayState();
            state.Sensors = new List<infraredsensor.InfraredState>(this.sensorPorts.Length);

            // compose state on the fly by issuing N parallel GET requests to partners
            var getSensorState = new analogsensor.Get();

            foreach (var port in this.sensorPorts)
            {
                state.Sensors.Add(null);
                port.Post(getSensorState);
            }

            ICollection<analogsensor.AnalogSensorState> sensorStates = null;
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
                PhysicalModel.Proxy.Pose sensorPose;
                switch (sensorState.HardwareIdentifier)
                {
                    case 0:
                        sensorPose = this.LeftIRPose;
                        break;
                    case 1:
                        sensorPose = this.MiddleIRPose;
                        break;
                    case 2:
                        sensorPose = this.RightIRPose;
                        break;
                    default:
                        sensorPose = new PhysicalModel.Proxy.Pose();
                        break;
                }

                state.Sensors[sensorState.HardwareIdentifier] = new infraredsensor.InfraredState()
                {
                    DistanceMeasurement = sensorState.RawMeasurement,
                    HardwareIdentifier = sensorState.HardwareIdentifier,
                    ManufacturerIdentifier = SimSensorManufacturer,
                    MaxDistance = int.MaxValue,
                    MinDistance = 0,
                    Pose = sensorPose,
                    TimeStamp = sensorState.TimeStamp
                };
            }

            var stateClone = state.Clone() as infraredarray.InfraredSensorArrayState;

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

            // Get each sensor's current state
            // And assign hardware identifiers
            var replaceSensorState = new analogsensor.Replace();
            foreach (var sensor in this.sensorPorts)
            {
                var getResultPort = this.sensorPorts[i].Get();
                yield return getResultPort.Choice();

                analogsensor.AnalogSensorState sensorState = getResultPort;
                if (sensorState != null)
                {
                    replaceSensorState.Body = sensorState;
                }
                else
                {
                    replaceSensorState.Body = new analogsensor.AnalogSensorState();
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
            this.sensorPorts[(int)IrSensors.LeftFrontProximityInMeters] = this.frontLeftSensorPort;
            this.sensorPorts[(int)IrSensors.CenterFrontProximityInMeters] = this.frontMiddleSensorPort;
            this.sensorPorts[(int)IrSensors.RightFrontProximityInMeters] = this.frontRightSensorPort;
        }
    }
}
