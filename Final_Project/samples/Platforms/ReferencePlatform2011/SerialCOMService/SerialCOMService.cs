//------------------------------------------------------------------------------
//  <copyright file="SerialCOMService.cs" company="Microsoft Corporation">
//      Copyright (C) Microsoft Corporation.  All rights reserved.
//  </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Robotics.Services.SerialComService
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Text;

    using Microsoft.Ccr.Core;
    using Microsoft.Dss.Core.Attributes;
    using Microsoft.Dss.ServiceModel.Dssp;
    using Microsoft.Dss.ServiceModel.DsspServiceBase;
    using W3C.Soap;

    using common = Microsoft.Robotics.Common;
    using SerialComPort = System.IO.Ports;
    using submgr = Microsoft.Dss.Services.SubscriptionManager;
    
    /// <summary>
    /// Main service class for serial port communications
    /// </summary>
    [Contract(Contract.Identifier)]
    [DisplayName("(User) SerialCOMService")]
    [Description("Generic Serial Port IO")]
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "Internal SerialPort exists for the lifetime of this DSS Service.")]
    public class SerialCOMService : DsspServiceBase
    {
        /// <summary>
        /// Service state
        /// </summary>
        [ServiceState]
        [InitialStatePartner(Optional = false)]
        private SerialCOMServiceState state = new SerialCOMServiceState();

        /// <summary>
        /// Main service port
        /// </summary>
        [ServicePort("/serialcomservice", AllowMultipleInstances = true)]
        private SerialCOMServiceOperations mainPort = new SerialCOMServiceOperations();

        /// <summary>
        /// Subscription Manager handles subscriptions/notifications
        /// </summary>
        [SubscriptionManagerPartner]
        private submgr.SubscriptionManagerPort submgrPort = new submgr.SubscriptionManagerPort();

        /// <summary>
        /// The Serial Port object used to send/receive data
        /// </summary>
        private SerialComPort.SerialPort serialPort;

        /// <summary>
        /// Port used for service create
        /// </summary>
        /// <param name="creationPort">Port for creation</param>
        public SerialCOMService(DsspServiceCreationPort creationPort) : base(creationPort)
        { 
        }

        /// <summary>
        /// Service start
        /// </summary>
        protected override void Start()
        {
            if (this.state == null)
            {
                this.state = new SerialCOMServiceState();
                this.SaveState(this.state);
            }

            if (this.state.AutoConnect == true)
            {
                this.CreateAndOpenSerialPort();
            }

            base.Start();
        }

        #region Operations Handlers
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

        /// <summary>
        /// Drop handler
        /// </summary>
        /// <param name="drop">DSS default drop type</param>
        [ServiceHandler]
        public void DropHandler(DsspDefaultDrop drop)
        {
            this.serialPort.Dispose(); // Dispose will close the serial port
            this.DefaultDropHandler(drop);
        }

        /// <summary>
        /// Handler for replacing the service state
        /// </summary>
        /// <param name="setConfiguration">A setConfiguration object containing service state</param>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public void SetConfigurationHandler(SetConfiguration setConfiguration)
        {
            this.CloseSerialPort();

            this.state = (SerialCOMServiceState)setConfiguration.Body;
            this.state.IsConnected = false;

            if (this.state.AutoConnect)
            {
                this.CreateAndOpenSerialPort();
            }

            this.SaveState(this.state);
            this.SendNotification(this.submgrPort, setConfiguration);
            setConfiguration.ResponsePort.Post(DefaultReplaceResponseType.Instance);
        }

        /// <summary>
        /// Opens the underlying serial port
        /// </summary>
        /// <param name="openPort">OpenPort object</param>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public void OpenPortHandler(OpenPort openPort)
        {
            if (this.serialPort == null || !this.serialPort.IsOpen)
            {
                this.CreateAndOpenSerialPort();
            }

            openPort.ResponsePort.Post(DefaultUpdateResponseType.Instance);
        }

        /// <summary>
        /// Closes the underlying serial port
        /// </summary>
        /// <param name="closePort">ClosePort object</param>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public void ClosePortHandler(ClosePort closePort)
        {
            this.CloseSerialPort();
            this.SendNotification(this.submgrPort, closePort);
            closePort.ResponsePort.Post(DefaultUpdateResponseType.Instance);
        }

        /// <summary>
        /// Flushes the underlying serial port buffer
        /// </summary>
        /// <param name="clearBuffer">ClearBuffer object</param>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public void ClearBufferHandler(ClearBuffer clearBuffer)
        {
            this.DiscardBuffer(clearBuffer.Body.BufferToClear);
            clearBuffer.ResponsePort.Post(DefaultSubmitResponseType.Instance);
        }

        /// <summary>
        /// Sends a packet over the serial port
        /// </summary>
        /// <param name="sendPacket">SendPacket object</param>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public void SendPacketHandler(SendPacket sendPacket)
        {
            if (this.serialPort == null || this.serialPort.IsOpen == false)
            {
                Exception problem = new InvalidOperationException("Serial Port not initialized or connected!");
                throw problem;
            }

            this.serialPort.Write(sendPacket.Body.Data.Message, 0, sendPacket.Body.Data.Message.Length);

            sendPacket.ResponsePort.Post(DefaultUpdateResponseType.Instance);
        }

        /// <summary>
        /// Receives data from the serial port
        /// </summary>
        /// <param name="recvPacket">GetPacket object</param>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public void RecvPacketHandler(GetPacket recvPacket)
        {
            if (this.serialPort == null || this.serialPort.IsOpen == false)
            {
                Exception problem = new InvalidOperationException("Serial Port not initialized or connected!");
                throw problem;
            }

            Packet retData = new Packet();

            // If no data is available just return an empty packet
            if (this.serialPort.BytesToRead <= 0)
            {
                recvPacket.ResponsePort.Post(retData);
                return;
            }

            // A packet terminator was specified, so read until we hit it
            if (!string.IsNullOrEmpty(recvPacket.Body.Terminator))
            {
                string s = this.serialPort.ReadTo(recvPacket.Body.Terminator);
                retData.Message = Encoding.ASCII.GetBytes(s.ToCharArray());
            }
            else
            {
                byte[] data = new byte[this.serialPort.BytesToRead];
                int result = this.serialPort.Read(data, 0, data.Length);

                if (result <= 0)
                {
                    throw new InvalidOperationException(string.Format("Serial Port Read Error: {0}", result.ToString()));
                }

                retData.Message = data;
            }

            if (this.state.Asynchronous)
            {
                this.SendNotification(this.submgrPort, new ReceivedPacket(retData));
            }

            recvPacket.ResponsePort.Post(retData);
        }

        /// <summary>
        /// Handler for performing a Write followed by a Read as a single operation
        /// Read operation will be repeated until data is available, or timeout has been reached
        /// </summary>
        /// <param name="sendget">Instance of type SendAndGet</param>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public void SendAndGetHandler(SendAndGet sendget)
        {
            if (this.serialPort == null || this.serialPort.IsOpen == false)
            {
                Exception problem = new InvalidOperationException("Serial Port not initialized or connected!");
                throw problem;
            }

            ////////////////////////////////////////////////////////////
            // Send the incoming packet
            ////////////////////////////////////////////////////////////
            this.serialPort.Write(sendget.Body.Data.Message, 0, sendget.Body.Data.Message.Length);

            ////////////////////////////////////////////////////////////
            // Attempt to read the response from the previous send
            ////////////////////////////////////////////////////////////
            Packet retData = new Packet();

            // No minimum "try again" period and no terminator specified, so if no data is available just return an empty packet
            if ((sendget.Body.Timeout == 0) && 
                (this.serialPort.BytesToRead <= 0) && 
                string.IsNullOrEmpty(sendget.Body.Terminator))
            {
                sendget.ResponsePort.Post(retData);
                return;
            }

            // If a timeout value has been included, check the buffer until we either have data or run out of time
            if ((sendget.Body.Timeout > 0) && (this.serialPort.BytesToRead <= 0))
            {
                // Ensure that something insane hasn't been specified
                if (sendget.Body.Timeout > SerialCOMConstants.DefaultReadTimeout)
                {
                    sendget.Body.Timeout = SerialCOMConstants.DefaultReadTimeout;
                }

                bool timedOut = false;
                double startMS = common.Utilities.ElapsedMilliseconds;
                do
                {
                    if (this.serialPort.BytesToRead <= 0)
                    {
                        if ((common.Utilities.ElapsedMilliseconds - startMS) > sendget.Body.Timeout)
                        {
                            timedOut = true;
                            break;
                        }
                    }
                }
                while (this.serialPort.BytesToRead <= 0);

                // Don't throw here, the rate of timeout could become too high and cause logging overload
                if (timedOut)
                {
                    string s = string.Format("SendAndGet timeout ({0}ms) reached with no available data in serial port!", sendget.Body.Timeout);
                    Fault f = Fault.FromException(new TimeoutException(s));
                    sendget.ResponsePort.Post(f);
                    return;
                }
            }
            
            // A packet terminator was specified
            if (!string.IsNullOrEmpty(sendget.Body.Terminator))
            {
                string s = this.serialPort.ReadTo(sendget.Body.Terminator);
                retData.Message = Encoding.ASCII.GetBytes(s.ToCharArray());
            }
            else
            {
                byte[] data = new byte[this.serialPort.BytesToRead];
                int result = this.serialPort.Read(data, 0, data.Length);
                if (result <= 0)
                {
                    // Don't throw here, the rate of read errors could become burdensome on the logging
                    string s = "Serial Port Read Error!";
                    Fault f = Fault.FromException(new InvalidOperationException(s));
                    sendget.ResponsePort.Post(f);
                    return;
                }

                retData.Message = data;
            }

            if (this.state.Asynchronous)
            {
                this.SendNotification(this.submgrPort, new ReceivedPacket(retData));
            }

            sendget.ResponsePort.Post(retData);
        }
        #endregion

        #region Internal Serial Port Operations
        /// <summary>
        /// Creates and opens the internal serialport with state values
        /// </summary>
        private void CreateAndOpenSerialPort()
        {
            if (this.serialPort != null)
            {
                this.CloseSerialPort();
            }

            // Create a new SerialPort object
            string comPort = "COM" + this.state.PortNumber.ToString();
            this.serialPort = new SerialComPort.SerialPort(comPort);

            this.serialPort.BaudRate = (int)this.state.BaudRate;
            this.serialPort.Parity = this.state.Parity;
            this.serialPort.DataBits = (int)this.state.DataBits;
            this.serialPort.StopBits = this.state.StopBits;
            this.serialPort.ReadTimeout = (int)this.state.ReadTimeout;
            this.serialPort.WriteTimeout = (int)this.state.WriteTimeout;
            this.serialPort.Handshake = this.state.Handshake;
            this.serialPort.RtsEnable = this.state.RtsEnable;
            this.serialPort.DtrEnable = this.state.DtrEnable;
            this.serialPort.DiscardNull = this.state.DiscardNull;

            if (this.state.Asynchronous)
            {
                this.serialPort.DataReceived += this.DataReceivedTrigger;
                this.serialPort.ReceivedBytesThreshold = 1;
            }

            this.serialPort.ErrorReceived += this.ErrorReceivedTrigger;

            this.serialPort.Open();

            this.state.IsConnected = true;

            LogVerbose(string.Format("Serial Port Connected with Current Config: {0}", this.state.ToString()));
        }

        /// <summary>
        /// Safely Closes the internal Serial Port
        /// </summary>
        private void CloseSerialPort()
        {
            if (this.serialPort != null && this.serialPort.IsOpen)
            {
                this.serialPort.Close();
            }

            this.serialPort = null;
            this.state.IsConnected = false;

            LogVerbose(string.Format("Serial Port Disconnected with Last Config: {0}", this.state.ToString()));
        }

        /// <summary>
        /// Fires an event when new data arrives on COM port
        /// </summary>
        /// <param name="sender">Sending object</param>
        /// <param name="e">Serial Port event args instance</param>
        private void DataReceivedTrigger(object sender, SerialComPort.SerialDataReceivedEventArgs e)
        {
            this.mainPort.Post(new GetPacket());
        }

        /// <summary>
        /// Fires an event when a COM port operation results in an error
        /// </summary>
        /// <param name="sender">Sending object</param>
        /// <param name="e">Serial Port event args instance</param>
        private void ErrorReceivedTrigger(object sender, SerialComPort.SerialErrorReceivedEventArgs e)
        {
            LogError(e.EventType.ToString());
        }

        /// <summary>
        /// Flushes all the data in the underlying COM port buffer
        /// </summary>
        /// <param name="clearOptions">Specifies In, Out, or Both buffers</param>
        private void DiscardBuffer(SerialPortClearOptions clearOptions)
        {
            switch (clearOptions)
            {
                case SerialPortClearOptions.Both:
                    {
                        this.serialPort.DiscardInBuffer();
                        this.serialPort.DiscardOutBuffer();
                        break;
                    }

                case SerialPortClearOptions.Input:
                    {
                        this.serialPort.DiscardInBuffer();
                        break;
                    }

                case SerialPortClearOptions.Output:
                    {
                        this.serialPort.DiscardOutBuffer();
                        break;
                    }
            }
        }
        #endregion
    }
}
