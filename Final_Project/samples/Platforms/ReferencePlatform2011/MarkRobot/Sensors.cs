//------------------------------------------------------------------------------
//  <copyright file="Sensors.cs" company="Microsoft Corporation">
//      Copyright (C) Microsoft Corporation.  All rights reserved.
//  </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Robotics.ReferencePlatform.MarkRobot
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Net;
    using System.Text;
    
    using Microsoft.Ccr.Core;
    using Microsoft.Dss.Core.Attributes;
    using Microsoft.Dss.Core.DsspHttp;
    using Microsoft.Dss.ServiceModel.Dssp;
    using Microsoft.Dss.ServiceModel.DsspServiceBase;

    using adc = Microsoft.Robotics.Services.ADCPinArray;
    using battery = Microsoft.Robotics.Services.Battery;
    using ir = Microsoft.Robotics.Services.InfraredSensorArray;
    using soap = W3C.Soap;
    using sonar = Microsoft.Robotics.Services.SonarSensorArray;
    using submgr = Microsoft.Dss.Services.SubscriptionManager;

    /// <summary>
    /// Main class for the service
    /// </summary>
    public partial class MarkRobotService : DsspServiceBase
    {
        /// <summary>
        /// Sensor normalized values are in meters
        /// </summary>
        private const double CentimetersPerMeter = 100;

        /// <summary>
        /// IR Sensors Port Identifier used in attributes
        /// </summary>
        private const string IRSensorsPortName = "irSensorsPort";

        /// <summary>
        /// Sonar Sensors Port Identifier used in attributes
        /// </summary>
        private const string SonarSensorsPortName = "sonarSensorsPort";

        /// <summary>
        /// Reference platform controller service ADC pin array port
        /// </summary>
        [Partner("ReferencePlatformControllerADCPinArray", Contract = adc.Contract.Identifier, CreationPolicy = PartnerCreationPolicy.UsePartnerListEntry)]
        private adc.ADCPinArrayOperations controllerAdcPinArrayPort = new adc.ADCPinArrayOperations();

        /// <summary>
        /// IR Sensor service subscription port
        /// </summary>
        [SubscriptionManagerPartner("IRSensors")]
        private submgr.SubscriptionManagerPort submgrIRSensorsPort = new submgr.SubscriptionManagerPort();

        /// <summary>
        /// Sonar Sensor service subscription port
        /// </summary>
        [SubscriptionManagerPartner("SonarSensors")]
        private submgr.SubscriptionManagerPort submgrSonarSensorsPort = new submgr.SubscriptionManagerPort();

        /// <summary>
        /// Alternate contract service port for IR Sensors
        /// </summary>
        [AlternateServicePort("/irsensors", AlternateContract = ir.Contract.Identifier)]
        private ir.InfraredSensorOperations irSensorsPort = new ir.InfraredSensorOperations();

        /// <summary>
        /// Alternate contract service port for Sonar Sensors
        /// </summary>
        [AlternateServicePort("/sonarsensors", AlternateContract = sonar.Contract.Identifier)]
        private sonar.SonarSensorOperations sonarSensorsPort = new sonar.SonarSensorOperations();

        /// <summary>
        /// Keep the FW watchdog alive by polling the sensor values
        /// </summary>
        private Port<DateTime> sensorPollingPort = new Port<DateTime>();

        #region IR Sensor Port Public Handlers 
        /// <summary>
        /// Handles Get requests on alternate port for IR sensors
        /// </summary>
        /// <param name="get">Request message</param>
        [ServiceHandler(PortFieldName = IRSensorsPortName)]
        public void IRSensorsGetHandler(ir.Get get)
        {
            get.ResponsePort.Post(this.state.InfraredSensorState);
        }

        /// <summary>
        /// Handles HttpGet requests on alternate port for IR sensors
        /// </summary>
        /// <param name="httpget">Request message</param>
        [ServiceHandler(PortFieldName = IRSensorsPortName)]
        public void IRSensorsHttpGetHandler(HttpGet httpget)
        {
            HttpResponseType resp = new HttpResponseType(HttpStatusCode.OK, this.state.InfraredSensorState);
            httpget.ResponsePort.Post(resp);
        }
        #endregion

        #region Sonar Sensor Port Public Handlers
        /// <summary>
        /// Handles Get requests on alternate port for sonar sensors
        /// </summary>
        /// <param name="get">Request message</param>
        [ServiceHandler(PortFieldName = SonarSensorsPortName)]
        public void SonarSensorsGetHandler(sonar.Get get)
        {
            get.ResponsePort.Post(this.state.SonarSensorState);
        }

        /// <summary>
        /// Handles HttpGet requests on alternate port sonar sensors
        /// </summary>
        /// <param name="httpget">Request message</param>
        [ServiceHandler(PortFieldName = SonarSensorsPortName)]
        public void SonarSensorsHttpGetHandler(HttpGet httpget)
        {
            HttpResponseType resp = new HttpResponseType(HttpStatusCode.OK, this.state.SonarSensorState);
            httpget.ResponsePort.Post(resp);
        }
        #endregion

        /// <summary>
        /// Keep the FW watchdog alive by continuously retrieving sensor+encoder values
        /// </summary>
        /// <param name="dt">A instance of type DateTime</param>
        /// <returns>A instance of IEnumerator of type ITask</returns>
        private IEnumerator<ITask> PollSensors(DateTime dt)
        {
            try
            {
                soap.Fault fault = null;
                adc.ADCPinArrayState pinArrayState = null;

                adc.Get adcGet = new adc.Get();

                this.controllerAdcPinArrayPort.Post(adcGet);

                yield return adcGet.ResponsePort.Choice();

                fault = (soap.Fault)adcGet.ResponsePort;

                if (fault != null)
                {
                    LogError(string.Format("Fault on getting ADCPinArray state: {0}", fault.Detail));
                    throw fault.ToException();
                }

                pinArrayState = (adc.ADCPinArrayState)adcGet.ResponsePort;

                for (int i = 0; i < this.state.InfraredSensorState.Sensors.Count; i++)
                {
                    adc.ADCPinState pinState = pinArrayState.Pins[this.state.InfraredSensorState.Sensors[i].HardwareIdentifier];

                    this.state.InfraredSensorState.Sensors[i].DistanceMeasurement = this.state.InfraredRawValueDivisorScalar * Math.Pow(pinState.PinValue, this.state.InfraredDistanceExponent) / CentimetersPerMeter;
                }

                for (int i = 0; i < this.state.SonarSensorState.Sensors.Count; i++)
                {
                    adc.ADCPinState pinState = pinArrayState.Pins[this.state.SonarSensorState.Sensors[i].HardwareIdentifier];

                    this.state.SonarSensorState.Sensors[i].DistanceMeasurement = pinState.PinValue * this.state.SonarTimeValueMultiplier / CentimetersPerMeter;
                }

                this.state.BatteryState.PercentBatteryPower = (pinArrayState.Pins[this.state.BatteryVoltagePinIndex].PinValue * this.state.BatteryVoltageDivider) / this.state.BatteryState.MaxBatteryPower * 100.0;

                if (this.state.BatteryState.PercentBatteryPower <= this.state.BatteryState.PercentCriticalBattery)
                {
                    battery.BatteryNotification batteryNotification = new battery.BatteryNotification(
                        (int)this.state.BatteryState.MaxBatteryPower,
                        this.state.BatteryState.PercentBatteryPower);

                    this.SendNotification<battery.Replace>(this.submgrBatteryPort, batteryNotification);
                }
            }
            finally
            {
                // Ensure we haven't been droppped
                if (ServicePhase == ServiceRuntimePhase.Started)
                {
                    // Issue another polling request
                    Activate(TimeoutPort(this.state.SensorPollingInterval).Receive(this.sensorPollingPort.Post));
                } 
            }

            yield break;
        }
                                                                                                            
        #region IR Sensor Subscribe Handlers
        /// <summary>
        /// Handles ReliableSubscribe requests on alternate port for IR Sensors
        /// </summary>
        /// <param name="reliablesubscribe">Request message</param>
        [ServiceHandler(PortFieldName = IRSensorsPortName)]
        public void IRSensorsReliableSubscribeHandler(ir.ReliableSubscribe reliablesubscribe)
        {
            SubscribeHelper(this.submgrIRSensorsPort, reliablesubscribe.Body, reliablesubscribe.ResponsePort);
        }

        /// <summary>
        /// Handles Subscribe requests on alternate port for IR Sensors
        /// </summary>
        /// <param name="subscribe">Request message</param>
        [ServiceHandler(PortFieldName = IRSensorsPortName)]
        public void IRSensorsSubscribeHandler(ir.Subscribe subscribe)
        {
            SubscribeHelper(this.submgrIRSensorsPort, subscribe.Body, subscribe.ResponsePort);
        }

        #endregion

        #region Sonar Sensor Subscribe Handlers
        /// <summary>
        /// Handles ReliableSubscribe requests on alternate port for Sonar Sensors
        /// </summary>
        /// <param name="reliablesubscribe">Request message</param>
        [ServiceHandler(PortFieldName = SonarSensorsPortName)]
        public void SonarSensorsReliableSubscribeHandler(sonar.ReliableSubscribe reliablesubscribe)
        {
            SubscribeHelper(this.submgrSonarSensorsPort, reliablesubscribe.Body, reliablesubscribe.ResponsePort);
        }

        /// <summary>
        /// Handles Subscribe requests on alternate port for Sonar Sensors
        /// </summary>
        /// <param name="subscribe">Request message</param>
        [ServiceHandler(PortFieldName = SonarSensorsPortName)]
        public void SonarSensorsSubscribeHandler(sonar.Subscribe subscribe)
        {
            SubscribeHelper(this.submgrSonarSensorsPort, subscribe.Body, subscribe.ResponsePort);
        }
        #endregion
    }
}