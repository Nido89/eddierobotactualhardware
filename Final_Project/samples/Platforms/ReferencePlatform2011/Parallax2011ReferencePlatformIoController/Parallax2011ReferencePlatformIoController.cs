//------------------------------------------------------------------------------
//  <copyright file="Parallax2011ReferencePlatformIoController.cs" company="Microsoft Corporation">
//      Copyright (C) Microsoft Corporation.  All rights reserved.
//  </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Robotics.ReferencePlatform.Parallax2011ReferencePlatformIoController
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Text;
    
    using Microsoft.Ccr.Core;
    using Microsoft.Dss.Core.Attributes;
    using Microsoft.Dss.ServiceModel.Dssp;
    using Microsoft.Dss.ServiceModel.DsspServiceBase;

    using adc = Microsoft.Robotics.Services.ADCPinArray;
    using board = ParallaxControlBoard;
    using drive = Microsoft.Robotics.Services.Drive;
    using gpio = Microsoft.Robotics.Services.GpioPinArray;
    using motor = Microsoft.Robotics.Services.Motor;
    using serialcomservice = Microsoft.Robotics.Services.SerialComService.Proxy;
    using soap = W3C.Soap;
    using submgr = Microsoft.Dss.Services.SubscriptionManager;

    /// <summary>
    /// Main class for the service
    /// </summary>
    [Contract(Contract.Identifier)]
    [AlternateContract(adc.Contract.Identifier)]
    [AlternateContract(drive.Contract.Identifier)]
    [AlternateContract(gpio.Contract.Identifier)]
    [DisplayName("Parallax2011ReferencePlatformIoController")]
    [Description("Brick service for the 2011 Reference Platform")]
    public partial class Parallax2011ReferencePlatformIoControllerService : DsspServiceBase
    {
        /// <summary> XSLT used for HTTP GET </summary>
        private const string Transform = "Parallax2011ReferencePlatformIoController.xslt";
       
        /// <summary>
        /// Service state
        /// </summary>
        [ServiceState]
        [InitialStatePartner(Optional = true, ServiceUri = "Parallax2011ReferencePlatformIoController.config.xml")]
        private Parallax2011ReferencePlatformIoControllerState state;

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
        /// Partner SerialCOMService main service port
        /// </summary>
        [Partner("SerialCOMService", Contract = serialcomservice.Contract.Identifier, CreationPolicy = PartnerCreationPolicy.UsePartnerListEntry)]
        private serialcomservice.SerialCOMServiceOperations serialCOMServicePort = new serialcomservice.SerialCOMServiceOperations();

        /// <summary>
        /// Partner SerialCOMService notification port
        /// </summary>
        private serialcomservice.SerialCOMServiceOperations serialCOMServiceNotify = new serialcomservice.SerialCOMServiceOperations();

        /// <summary>
        /// Main service port
        /// </summary>
        [ServicePort("/Parallax2011ReferencePlatformIoController", AllowMultipleInstances = false)]
        private Parallax2011ReferencePlatformIoControllerOperations mainPort = new Parallax2011ReferencePlatformIoControllerOperations();

        /// <summary>
        /// Initializes a new instance of Parallax2011ReferencePlatformIoControllerService class
        /// </summary>
        /// <param name="creationPort">Instance of type DsspServiceCreationPort</param>
        public Parallax2011ReferencePlatformIoControllerService(DsspServiceCreationPort creationPort)
            : base(creationPort)
        {
        }

        /// <summary>
        /// Determines if a response packet contains an ERROR
        /// </summary>
        /// <param name="p">Packet received after a command</param>
        /// <returns>True if error identifier detected, otherwise false</returns>
        private bool HasFWError(serialcomservice.Packet p)
        {
            if (p == null || p.Message == null)
            {
                return true;
            }

            if (Encoding.ASCII.GetString(p.Message).IndexOf(board.Error) != -1)
            {
                return true;
            }

            return false;
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
                    this.state = new Parallax2011ReferencePlatformIoControllerState();
                }

                // Fill out the L/R drive state specifics
                if (this.state.DriveState.LeftWheel == null || this.state.DriveState.LeftWheel.Radius == 0)
                {
                    this.state.DriveState.LeftWheel = new motor.WheeledMotorState() { Radius = board.DefaultWheelRadius };
                }

                this.state.DriveState.LeftWheel.MotorState = new motor.MotorState();
                this.state.DriveState.LeftWheel.MotorState.Name = this.wheelNames[(int)Sides.Left];
                this.state.DriveState.LeftWheel.MotorState.HardwareIdentifier = (int)Sides.Left;
                this.state.DriveState.LeftWheel.MotorState.PowerScalingFactor = 1;

                if (this.state.DriveState.LeftWheel.EncoderState == null || this.state.DriveState.LeftWheel.EncoderState.TicksPerRevolution == 0)
                {
                    this.state.DriveState.LeftWheel.EncoderState = new Services.Encoder.EncoderState() { TicksPerRevolution = board.DefaultTicksPerRevolution };
                }

                if (this.state.DriveState.RightWheel == null || this.state.DriveState.RightWheel.Radius == 0)
                {
                    this.state.DriveState.RightWheel = new motor.WheeledMotorState() { Radius = board.DefaultWheelRadius };
                }

                this.state.DriveState.RightWheel.MotorState = new motor.MotorState();
                this.state.DriveState.RightWheel.MotorState.Name = this.wheelNames[(int)Sides.Right];
                this.state.DriveState.RightWheel.MotorState.HardwareIdentifier = (int)Sides.Right;
                this.state.DriveState.RightWheel.MotorState.PowerScalingFactor = 1;

                if (this.state.DriveState.RightWheel.EncoderState == null || this.state.DriveState.RightWheel.EncoderState.TicksPerRevolution == 0)
                {
                    this.state.DriveState.RightWheel.EncoderState = new Services.Encoder.EncoderState() { TicksPerRevolution = board.DefaultTicksPerRevolution };
                }

                // Reflect all the default GPIO pin states into the state

                if (this.state.GpioState == null)
                {
                    this.state.GpioState = new gpio.GpioPinArrayState();
                }

                this.state.GpioState.Pins = new List<gpio.GpioPinState>(board.GPIOPinCount);
                for (int pinIndex = 0; pinIndex < board.GPIOPinCount; pinIndex++)
                {
                    gpio.GpioPinState pinState = new gpio.GpioPinState();

                    // GPIO bank defaults to IN for PING sensors
                    pinState.PinDirection = gpio.GpioPinState.GpioPinDirection.In;

                    // Pins are labeled on the control board using 1-based indexing
                    pinState.Number = pinIndex + 1;

                    this.state.GpioState.Pins.Add(pinState);
                }

                // Populate the ADCPins
                // NOTE: Pins on the board are labelled using 1-based indexing

                if (this.state.AdcState == null)
                {
                    this.state.AdcState = new adc.ADCPinArrayState();
                }

                this.state.AdcState.Pins = new List<adc.ADCPinState>(board.ADCPinCount + board.DigitalPinCount);
                for (int pinIndex = 0; pinIndex < board.ADCPinCount + board.DigitalPinCount; pinIndex++)
                {
                    adc.ADCPinState a = new adc.ADCPinState();
                    a.Name = string.Format("Pin_{0}", pinIndex);
                    a.TimeStamp = DateTime.Now;
                    a.Number = pinIndex + 1;
                    this.state.AdcState.Pins.Add(a);
                }
            }
            catch (Exception e)
            {
                LogError(e);
                this.Shutdown();
                return false;
            }

            this.state.DriveState.TimeStamp = 
            this.state.LastStartTime = DateTime.Now;
            
            SaveState(this.state);
            return true;
        }

        /// <summary>
        /// Retrieve the COM port specified in the SerialCOMService state.
        /// Perform initialization if COM port is correct and available.
        /// </summary>
        /// <returns>Enumerator of type ITask</returns>
        private IEnumerator<ITask> InternalInitialize()
        {
            // Make sure we have a valid and open COM port
            int currentCOMPort = 0;

            yield return this.serialCOMServicePort.GetConfiguration().Choice(
                s => currentCOMPort = s.PortNumber,
                f => LogError("Failed to retrieve config from Serial Port partner service"));

            yield return this.serialCOMServicePort.OpenPort().Choice(
                s => LogInfo(string.Format("Opened COM{0}", currentCOMPort)),
                f => LogError(string.Format("Failed to open COM{0}", currentCOMPort)));

            if (currentCOMPort == 0)
            {
                LogError("Parallax2011ReferencePlatformIoController Service failed to initialize: Check 'PortNumber' in serialcomservice.config.xml");
                this.Shutdown();
                yield break;
            }

            this.initialized = this.Initialize();
            if (this.initialized)
            {
                // Make sure we have a live Parallax board on the COM port by retrieving the FW version string
                serialcomservice.SendAndGetRequest sg = new serialcomservice.SendAndGetRequest();
                sg.Timeout = this.state.DefaultResponsePause;
                sg.Terminator = board.PacketTerminatorString;
                sg.Data = new serialcomservice.Packet(board.CreatePacket<byte>(board.GetVersionString));

                PortSet<serialcomservice.Packet, soap.Fault> resultPort = this.serialCOMServicePort.SendAndGet(sg);
                yield return resultPort.Choice();

                soap.Fault f = (soap.Fault)resultPort;
                if (f == null)
                {
                    serialcomservice.Packet p = (serialcomservice.Packet)resultPort;
                    if (p != null)
                    {
                        if (p.Message != null)
                        {
                            string str = Encoding.ASCII.GetString(p.Message);
                            this.state.FWVersion = Convert.ToInt32(str, 16);
                        }
                    }
                }
                else
                {
                    LogError(string.Format("Failed to send command: {0}", Encoding.ASCII.GetString(sg.Data.Message)));
                    LogError("Failed to receive FW version!");
                }
                
                LogInfo(string.Format("FW Version: {0}", this.state.FWVersion));

                serialcomservice.SendAndGetRequest accSendAndGet = new serialcomservice.SendAndGetRequest();

                accSendAndGet.Timeout = this.state.DefaultResponsePause;
                accSendAndGet.Terminator = board.PacketTerminatorString;
                accSendAndGet.Data = new serialcomservice.Packet(board.CreatePacket<ushort>(board.SetRampingValueString, (ushort)this.state.AccelerationRate));

                resultPort = this.serialCOMServicePort.SendAndGet(accSendAndGet);
                yield return resultPort.Choice();

                f = (soap.Fault)resultPort;
                if (f != null)
                {
                    LogError(string.Format("Failed to send command: {0}", Encoding.ASCII.GetString(accSendAndGet.Data.Message)));
                    LogError("Failed to set acceleration!");
                }

                this.encoderTicksPerMeter = new double[]
                {
                    this.state.DriveState.LeftWheel.EncoderState.TicksPerRevolution / (2 * this.state.DriveState.LeftWheel.Radius * Math.PI),
                    this.state.DriveState.RightWheel.EncoderState.TicksPerRevolution / (2 * this.state.DriveState.RightWheel.Radius * Math.PI)
                };

                SaveState(this.state);
                base.Start();

                // Make sure the pin polling port is in the main interleave because it modifies service state
                MainPortInterleave.CombineWith(
                                               new Interleave(
                                               new ExclusiveReceiverGroup(
                                               Arbiter.ReceiveWithIterator(true, this.pinPollingPort, this.PollPins)),
                                               new ConcurrentReceiverGroup()));

                // Start the pin polling interval
                this.pinPollingPort.Post(DateTime.Now);
            }
        }

        /// <summary>
        /// Service start:
        /// 1) Ensure we have open comms to the Parallax board via SerialCOMService 
        /// 2) Read the current FW version into this service's state
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
        [ServiceHandler]
        public void GetHandler(Get get)
        {
            get.ResponsePort.Post(this.state);
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
