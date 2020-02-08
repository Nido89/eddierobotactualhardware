//------------------------------------------------------------------------------
//  <copyright file="gpio.cs" company="Microsoft Corporation">
//      Copyright (C) Microsoft Corporation.  All rights reserved.
//  </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Robotics.ReferencePlatform.Parallax2011ReferencePlatformIoController
{
    using System.Collections.Generic;
    using System.Net;
    using System.Text;
    
    using Microsoft.Ccr.Core;
    using Microsoft.Dss.Core.Attributes;
    using Microsoft.Dss.Core.DsspHttp;
    using Microsoft.Dss.ServiceModel.Dssp;
    using Microsoft.Dss.ServiceModel.DsspServiceBase;

    using board = ParallaxControlBoard;
    using gpio = Microsoft.Robotics.Services.GpioPinArray;
    using serialcomservice = Microsoft.Robotics.Services.SerialComService.Proxy;
    using soap = W3C.Soap;
    using submgr = Microsoft.Dss.Services.SubscriptionManager;

    /// <summary>
    /// Main service class
    /// </summary>
    public partial class Parallax2011ReferencePlatformIoControllerService : DsspServiceBase
    {
        /// <summary>
        /// GPIO Port Identifier used in attributes
        /// </summary>
        private const string GpioPortName = "gpioPort";

        /// <summary>
        /// Alternate contract service port
        /// </summary>
        [AlternateServicePort(AlternateContract = gpio.Contract.Identifier)]
        private gpio.GpioPinArrayOperations gpioPort = new gpio.GpioPinArrayOperations();

        /// <summary>
        /// GPIO Pin Array service subscription port
        /// </summary>
        [SubscriptionManagerPartner("gpio")]
        private submgr.SubscriptionManagerPort submgrGpioPort = new submgr.SubscriptionManagerPort();

        /// <summary>
        /// Handles Get requests on alternate port
        /// </summary>
        /// <param name="get">Request message</param>
        [ServiceHandler(PortFieldName = GpioPortName)]
        public void GpioGetHandler(gpio.Get get)
        {
            get.ResponsePort.Post(this.state.GpioState);
        }

        /// <summary>
        /// Handles HttpGet requests on alternate port
        /// </summary>
        /// <param name="httpget">Request message</param>
        [ServiceHandler(PortFieldName = GpioPortName)]
        public void GpioHttpGetHandler(Microsoft.Dss.Core.DsspHttp.HttpGet httpget)
        {
            HttpResponseType resp = new HttpResponseType(HttpStatusCode.OK, this.state.GpioState);
            httpget.ResponsePort.Post(resp);
        }

        /// <summary>
        /// Handles SetPin requests on alternate port
        /// </summary>
        /// <param name="setPin">Request message</param>
        /// <returns>An IEnumerator object of type ITask</returns>
        [ServiceHandler(PortFieldName = GpioPortName)]
        public IEnumerator<ITask> GpioSetPinHandler(gpio.SetPin setPin)
        {
            soap.Fault f;

            // Pins are labeled on the control board using 1-based indexing
            int pinIndex = setPin.Body.PinState.Number - 1;
            if (pinIndex < 0 || pinIndex >= board.GPIOPinCount)
            {
                f = soap.Fault.FromCodeSubcodeReason(
                                                     soap.FaultCodes.Receiver,
                                                     DsspFaultCodes.OperationFailed,
                                                     "Invalid GPIO pin index specified!");
                setPin.ResponsePort.Post(f);
                yield break;
            }

            int pinMask = 1 << pinIndex;

            string cmdString = 
                (setPin.Body.PinState.PinDirection == gpio.GpioPinState.GpioPinDirection.Out) ? 
                board.SetGPIODirectionOutString : board.SetGPIODirectionInString;

            byte[] cmdpacket = board.CreatePacket(cmdString, pinMask);

            serialcomservice.SendAndGetRequest sg = new serialcomservice.SendAndGetRequest();
            sg.Timeout = this.state.DefaultResponsePause;
            sg.Data = new serialcomservice.Packet(cmdpacket);

            var resultPort = this.serialCOMServicePort.SendAndGet(sg);
            yield return resultPort.Choice();

            f = (soap.Fault)resultPort;
            if (f != null)
            {
                LogError(string.Format("Failed to send command: {0}", Encoding.ASCII.GetString(sg.Data.Message)));
                setPin.ResponsePort.Post(f);
                yield break;
            }

            if (this.HasFWError((serialcomservice.Packet)resultPort))
            {
                f = soap.Fault.FromCodeSubcodeReason(
                                                     soap.FaultCodes.Receiver,
                                                     DsspFaultCodes.OperationFailed,
                                                     "Error received from FW!");
                setPin.ResponsePort.Post(f);
                yield break;
            }

            // If we have an OUT pin, then it's appropriate to take action on the HIGH/LOW state
            // Ignore PinStates that have a HIGH/LOW specifier when direction is set to IN
            if (setPin.Body.PinState.PinDirection == gpio.GpioPinState.GpioPinDirection.Out)
            {
                cmdString = 
                    (setPin.Body.PinState.PinState == gpio.GpioPinState.GpioPinSignal.Low) ? 
                    board.SetGPIOStateLowString : board.SetGPIOStateHighString;

                cmdpacket = board.CreatePacket(cmdString, pinMask);

                sg.Timeout = this.state.DefaultResponsePause;
                sg.Data = new serialcomservice.Packet(cmdpacket);

                resultPort = this.serialCOMServicePort.SendAndGet(sg);
                yield return resultPort.Choice();

                f = (soap.Fault)resultPort;
                if (f != null)
                {
                    LogError(string.Format("Failed to send command: {0}", Encoding.ASCII.GetString(sg.Data.Message)));
                    setPin.ResponsePort.Post(f);
                    yield break;
                }

                if (this.HasFWError((serialcomservice.Packet)resultPort))
                {
                    f = soap.Fault.FromCodeSubcodeReason(
                                                         soap.FaultCodes.Receiver,
                                                         DsspFaultCodes.OperationFailed,
                                                         "Error received from FW!");
                    setPin.ResponsePort.Post(f);
                    yield break;
                }
            }

            this.state.GpioState.Pins[pinIndex] = setPin.Body.PinState;
            setPin.ResponsePort.Post(DefaultUpdateResponseType.Instance);
        }

        #region SubscriptionHandlers
        /// <summary>
        /// Handles ReliableSubscribe requests on alternate port
        /// </summary>
        /// <param name="reliablesubscribe">Request message</param>
        [ServiceHandler(PortFieldName = GpioPortName)]
        public void GpioReliableSubscribeHandler(gpio.ReliableSubscribe reliablesubscribe)
        {
            SubscribeHelper(this.submgrGpioPort, reliablesubscribe.Body, reliablesubscribe.ResponsePort);
        }

        /// <summary>
        /// Handles Subscribe requests on alternate port
        /// </summary>
        /// <param name="subscribe">Request message</param>
        [ServiceHandler(PortFieldName = GpioPortName)]
        public void GpioSubscribeHandler(gpio.Subscribe subscribe)
        {
            SubscribeHelper(this.submgrGpioPort, subscribe.Body, subscribe.ResponsePort);
        }
        #endregion
    }
}