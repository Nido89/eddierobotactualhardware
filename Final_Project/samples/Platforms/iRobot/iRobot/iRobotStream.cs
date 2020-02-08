//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: iRobotStream.cs $ $Revision: 15 $
//-----------------------------------------------------------------------
using Microsoft.Ccr.Core;
using Microsoft.Dss.Core;
using Microsoft.Dss.Core.Attributes;
using Microsoft.Dss.ServiceModel.Dssp;
using Microsoft.Dss.ServiceModel.DsspServiceBase;
using System;
using System.Collections.Generic;
using stream = Microsoft.Robotics.Services.DssStream.Proxy;
using submgr = Microsoft.Dss.Services.SubscriptionManager;
using System.IO.Ports;
using System.Text;
using W3C.Soap;
using roomba = Microsoft.Robotics.Services.IRobot.Roomba;
using System.Collections.Specialized;
using Microsoft.Robotics.Services.IRobot.Create;
using System.ComponentModel;

namespace Microsoft.Robotics.Services.IRobot.DssStream
{

    /// <summary>
    /// iRobotstream Service
    /// </summary>
    /// <remarks>The iRobot stream service calls to the serial port and may block a thread
    /// The ActivationSettings attribute with Sharing == false makes the runtime
    /// dedicate a dispatcher thread pool just for this service.</remarks>
    [ActivationSettings(ShareDispatcher = false, ExecutionUnitsPerDispatcher = 1)]
    [Contract(Contract.Identifier)]
    [AlternateContract(stream.Contract.Identifier)]
    [DisplayName("(User) iRobot� Stream Communications")]
    [Description("Provides stream communications support for the 'iRobot� Create / Roomba' service.")]
    public class iRobotStreamService : DsspServiceBase
    {
        private const int _maxBuffer = 1024;
        private iRobotConnection _iRobotConnection = new iRobotConnection();

        /// <summary>
        /// _main Port
        /// </summary>
        [ServicePort("/irobotstream", AllowMultipleInstances = true)]
        private stream.StreamOperations _mainPort = new stream.StreamOperations();

        /// <summary>
        /// Stream state
        /// </summary>
        private stream.StreamState _state = new stream.StreamState();

        /// <summary>
        /// The connection to the iRobot
        /// </summary>
        private SerialPort _serialPort;

        // Subscription manager partner
        [Partner(Partners.SubscriptionManagerString, Contract = submgr.Contract.Identifier, CreationPolicy = PartnerCreationPolicy.CreateAlways)]
        submgr.SubscriptionManagerPort _subMgrPort = new submgr.SubscriptionManagerPort();

        /// <summary>
        /// Default Service Constructor
        /// </summary>
        public iRobotStreamService(DsspServiceCreationPort creationPort) :
                base(creationPort)
        {
        }

        /// <summary>
        /// Service Start
        /// </summary>
        protected override void Start()
        {
            ActivateDsspOperationHandlers();

            // To keep this service private,
            // comment out the following line:
            base.DirectoryInsert();
        }

        #region Operations Handlers

        /// <summary>
        /// Write Data to the iRobot devices
        /// </summary>
        /// <param name="submit"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public virtual IEnumerator<ITask> WriteDataHandler(stream.WriteData submit)
        {
            if (submit == null || submit.Body == null || submit.Body.Data == null)
            {
                submit.ResponsePort.Post(Fault.FromException(new InvalidOperationException("Invalid data sent to the iRobot.")));
                yield break;
            }

            if (_serialPort == null || !_serialPort.IsOpen)
            {
                submit.ResponsePort.Post(Fault.FromException(new InvalidOperationException("Attempting to send a command before the iRobot Connection has been established.")));
                yield break;
            }

            if (_iRobotConnection.ConnectionType == roomba.iRobotConnectionType.RooTooth
                && !_serialPort.CtsHolding
                && !_serialPort.DsrHolding)
            {
                string errorMessage = "The Bluetooth serial port is paired to a device,\r\n"
                + "    but the Rootooth may not be connected.\r\n";
                LogWarning(LogGroups.Console, errorMessage);
                if (!_state.Initialized)
                {
                    submit.ResponsePort.Post(Fault.FromException(new System.IO.IOException(errorMessage)));
                    yield break;
                }
            }

            try
            {
                _serialPort.Write(submit.Body.Data, 0, submit.Body.Data.Length);
            }
            catch (Exception ex)
            {
                _state.Initialized = false;
                LogError(LogGroups.Console, ex);
                submit.ResponsePort.Post(Fault.FromException(ex));
                yield break;
            }

            submit.ResponsePort.Post(DefaultSubmitResponseType.Instance);
            yield break;
        }

        /// <summary>
        /// Write Text to the iRobot device
        /// </summary>
        /// <param name="submit"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public virtual IEnumerator<ITask> WriteTextHandler(stream.WriteText submit)
        {
            if (submit == null || submit.Body == null || submit.Body.Text == null)
            {
                submit.ResponsePort.Post(Fault.FromException(new InvalidOperationException("Invalid data sent to the iRobot.")));
                yield break;
            }

            if (_serialPort == null || !_serialPort.IsOpen)
            {
                submit.ResponsePort.Post(Fault.FromException(new InvalidOperationException("Attempting to send a command before the iRobot Connection has been established.")));
                yield break;
            }

            try
            {
                if (submit.Body.Text.EndsWith("\r") || submit.Body.Text.EndsWith("\n"))
                    _serialPort.Write(submit.Body.Text);
                else
                    _serialPort.WriteLine(submit.Body.Text);
            }
            catch (Exception ex)
            {
                LogError(LogGroups.Console, ex);
                submit.ResponsePort.Post(Fault.FromException(ex));
                yield break;
            }

            submit.ResponsePort.Post(DefaultSubmitResponseType.Instance);
            yield break;
        }

        /// <summary>
        /// Configure the iRobot communications stream
        /// </summary>
        /// <param name="replace"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public virtual IEnumerator<ITask> ReplaceHandler(stream.ReplaceStreamState replace)
        {
            _state = replace.Body;
            bool connected = Connect();
            stream.ReplaceStreamResponse response = new stream.ReplaceStreamResponse(connected);
            replace.ResponsePort.Post(response);

            // Update the Replace body with any state that has changed
            // before sending the notification.
            replace.Body = (stream.StreamState)_state.Clone();
            SendNotification<stream.ReplaceStreamState>(_subMgrPort, replace);
            yield break;
        }

        /// <summary>
        /// Clear Stream Buffers
        /// </summary>
        /// <param name="clearBuffers"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public virtual IEnumerator<ITask> ClearStreamBuffersHandler(stream.ClearStreamBuffers clearBuffers)
        {
            if (_serialPort != null && _serialPort.IsOpen)
            {
                try
                {
                    if (clearBuffers.Body.ClearReadBuffer)
                    {
                        if (_serialPort.BytesToRead > 0)
                            _serialPort.DiscardInBuffer();
                    }

                    if (clearBuffers.Body.ClearWriteBuffer)
                    {
                        if (_serialPort.BytesToWrite > 0)
                            _serialPort.DiscardOutBuffer();
                    }

                    clearBuffers.ResponsePort.Post(DefaultSubmitResponseType.Instance);
                    yield break;
                }
                catch (Exception ex)
                {
                    clearBuffers.ResponsePort.Post(Fault.FromException(ex));
                    yield break;
                }
            }

            clearBuffers.ResponsePort.Post(Fault.FromException(new InvalidOperationException("iRobot Connection is not initialized")));
            yield break;
        }

        /// <summary>
        /// Set Stream Property Handler
        /// </summary>
        /// <param name="setStreamProperty"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public virtual IEnumerator<ITask> SetStreamPropertyHandler(stream.SetStreamProperty setStreamProperty)
        {
            bool success = false;
            bool badValue = false;
            switch (setStreamProperty.Body.Name)
            {
                case "WakeupRoomba":

                    try
                    {
                        _serialPort.RtsEnable = false;
                    }
                    catch (Exception ex)
                    {
                        setStreamProperty.ResponsePort.Post(Fault.FromException(ex));
                        yield break;
                    }
                    yield return Arbiter.Receive(false, TimeoutPort(100), delegate(DateTime timeout) { });

                    try
                    {
                        _serialPort.RtsEnable = true;
                        _serialPort.RtsEnable = true;
                        _serialPort.RtsEnable = true;
                        _serialPort.RtsEnable = true;
                        _serialPort.RtsEnable = true;
                    }
                    catch (Exception ex)
                    {
                        setStreamProperty.ResponsePort.Post(Fault.FromException(ex));
                        yield break;
                    }
                    yield return Arbiter.Receive(false, TimeoutPort(100), delegate(DateTime timeout) { });

                    try
                    {
                        _serialPort.RtsEnable = false;
                    }
                    catch (Exception ex)
                    {
                        setStreamProperty.ResponsePort.Post(Fault.FromException(ex));
                        yield break;
                    }
                    yield return Arbiter.Receive(false, TimeoutPort(100), delegate(DateTime timeout) { });

                    setStreamProperty.ResponsePort.Post(DefaultUpdateResponseType.Instance);
                    yield break;

                case "RtsEnable":
                    if (_serialPort != null)
                    {
                        bool rtsEnable;
                        try
                        {
                            rtsEnable = bool.Parse(setStreamProperty.Body.Value);
                            _serialPort.RtsEnable = rtsEnable;
                            _serialPort.RtsEnable = rtsEnable;
                            success = true;
                        }
                        catch (System.FormatException) { }
                        catch (System.ArgumentNullException) { }
                    }
                    break;
            }

            if (success)
                setStreamProperty.ResponsePort.Post(DefaultUpdateResponseType.Instance);
            else
            {
                string errorMessage = string.Empty;
                if (string.IsNullOrEmpty(errorMessage))
                {
                    if (badValue)
                        errorMessage = "Invalid value for " + setStreamProperty.Body.Name;
                    else
                        errorMessage = "Unknown property: " + setStreamProperty.Body.Name;
                }
                setStreamProperty.ResponsePort.Post(Fault.FromException(new ArgumentOutOfRangeException(errorMessage)));
            }
            yield break;
        }

        /// <summary>
        /// Query Stream Property Handler
        /// </summary>
        /// <param name="queryStreamProperty"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public virtual IEnumerator<ITask> QueryStreamPropertyHandler(stream.QueryStreamProperty queryStreamProperty)
        {
            queryStreamProperty.Body.Value = null;
            switch(queryStreamProperty.Body.Name)
            {
                case "RtsEnable":
                    if (_serialPort != null)
                        queryStreamProperty.Body.Value = _serialPort.RtsEnable.ToString();
                    break;
                case "BaudRate":
                    if (_serialPort != null)
                        queryStreamProperty.Body.Value = _serialPort.BaudRate.ToString();
                    break;
                case "DataBits":
                    if (_serialPort != null)
                        queryStreamProperty.Body.Value = _serialPort.DataBits.ToString();
                    break;
                case "Encoding":
                    if (_serialPort != null)
                        queryStreamProperty.Body.Value = _serialPort.Encoding.ToString();
                    break;
                case "Parity":
                    if (_serialPort != null)
                        queryStreamProperty.Body.Value = _serialPort.Parity.ToString();
                    break;
                case "StopBits":
                    if (_serialPort != null)
                        queryStreamProperty.Body.Value = _serialPort.StopBits.ToString();
                    break;
                case "PortName":
                    if (_serialPort != null)
                        queryStreamProperty.Body.Value = _serialPort.PortName.ToString();
                    break;
                case "ConnectionType":
                    if (_iRobotConnection != null)
                        queryStreamProperty.Body.Value = _iRobotConnection.ConnectionType.ToString();
                    break;
            }

            queryStreamProperty.ResponsePort.Post(queryStreamProperty.Body);
            yield break;
        }


        #region Invalid inbound Operations

        /// <summary>
        /// ReadData is a notification and invalid when sent to the stream service.
        /// </summary>
        /// <param name="insert"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public virtual IEnumerator<ITask> ReadDataHandler(stream.ReadData insert)
        {
            // ReadData is only valid when received on a notification!
            throw new InvalidOperationException("Subscribe to ReadData to receive data from the stream.");
        }

        /// <summary>
        /// ReadText is a notification and invalid when sent to the stream service.
        /// </summary>
        /// <param name="insert"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public virtual IEnumerator<ITask> ReadTextHandler(stream.ReadText insert)
        {
            // ReadText is only valid when received on a notification!
            throw new InvalidOperationException("Subscribe to ReadText to receive text from the stream.");
        }

        #endregion

        #region Standard Handlers

        /// <summary>
        /// GetStreamState Handler
        /// </summary>
        /// <param name="get"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public virtual IEnumerator<ITask> GetStreamStateHandler(stream.GetStreamState get)
        {
            get.ResponsePort.Post(_state);
            yield break;
        }

        /// <summary>
        /// Subscribe Handler
        /// </summary>
        /// <param name="subscribe"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public virtual IEnumerator<ITask> SubscribeHandler(stream.Subscribe subscribe)
        {
            yield return Arbiter.Choice(
                base.SubscribeHelper(_subMgrPort, subscribe.Body, subscribe.ResponsePort),
                delegate(SuccessResult success)
                {
                    SendNotificationToTarget<stream.ReplaceStreamState>(subscribe.Body.Subscriber, _subMgrPort, new stream.ReplaceStreamState(_state));
                },
                delegate(Exception fault)
                {
                    LogError(fault);
                }
            );
            yield break;
        }

        /// <summary>
        /// ReliableSubscribe Handler
        /// </summary>
        /// <param name="subscribe"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public virtual IEnumerator<ITask> ReliableSubscribeHandler(stream.ReliableSubscribe subscribe)
        {
            yield return Arbiter.Choice(
                base.SubscribeHelper(_subMgrPort, subscribe.Body, subscribe.ResponsePort),
                delegate(SuccessResult success)
                {
                    SendNotificationToTarget<stream.ReplaceStreamState>(subscribe.Body.Subscriber, _subMgrPort, new stream.ReplaceStreamState(_state));
                },
                delegate(Exception fault)
                {
                    LogError(fault);
                }
            );
            yield break;
        }
        #endregion

        /// <summary>
        /// Release serial port resources
        /// </summary>
        /// <param name="drop"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Teardown)]
        public virtual IEnumerator<ITask> DropHandler(DsspDefaultDrop drop)
        {
            // Close connection and Dispose() serial port.
            CloseConnection(true);
            drop.ResponsePort.Post(DefaultDropResponseType.Instance);
            yield break;
        }

        #endregion


        #region Private Methods

        /// <summary>
        /// Connect to the underlying communications port.
        /// </summary>
        /// <returns></returns>
        private bool Connect()
        {
            IRobotUtility.ParseConfiguration(_state, ref _iRobotConnection);

            bool success = false;

            // Close connection without disposing
            CloseConnection(false);

            if (!string.IsNullOrEmpty(_iRobotConnection.PortName))
            {
                if (_serialPort == null)
                {
                    _serialPort = new SerialPort(_iRobotConnection.PortName, _iRobotConnection.BaudRate);
                    _serialPort.ReceivedBytesThreshold = 1;
                }
                else
                {
                    _serialPort.PortName = _iRobotConnection.PortName;
                    _serialPort.BaudRate = _iRobotConnection.BaudRate;
                }

                _serialPort.WriteTimeout = 1000;
                _serialPort.Encoding = _iRobotConnection.Encoding;
                _serialPort.Parity = _iRobotConnection.Parity;
                _serialPort.DataBits = _iRobotConnection.DataBits;
                _serialPort.StopBits = _iRobotConnection.StopBits;

                try
                {
                    _serialPort.Open();
                    _serialPort.DataReceived += new SerialDataReceivedEventHandler(_serialPort_DataReceived);

                    success = _serialPort.IsOpen;
                }
                catch (Exception ex)
                {
                    LogError(LogGroups.Console, ex);
                }
            }

            _state.Initialized = success;
            return _state.Initialized;
        }

        /// <summary>
        /// Receive data from the serial port
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (e.EventType == SerialData.Chars)
            {
                // Send the bytes as quickly as possible to subscribers.
                int totalBytesToProcess = _serialPort.BytesToRead;
                DateTime now = DateTime.Now;
                while (totalBytesToProcess > 0)
                {
                    // Calculate the number of bytes to read
                    int count = Math.Min(_maxBuffer, totalBytesToProcess);
                    byte[] data = new byte[count];
                    _serialPort.Read(data, 0, count);
                    SendNotification<stream.ReadData>(_subMgrPort, new stream.ReadData(new stream.StreamData(data, now)));
                    totalBytesToProcess -= count;
                    now = now.AddTicks(1);
                }
            }
        }

        /// <summary>
        /// Close the Connection
        /// </summary>
        /// <param name="dispose"></param>
        private void CloseConnection(bool dispose)
        {
            if (_serialPort != null)
            {
                if (_serialPort.IsOpen)
                {
                    try
                    {
                        _serialPort.Close();
                    }
                    catch (Exception ex)
                    {
                        LogError("Error closing iRobot connection: " + ex.Message);
                    }
                }

                try
                {
                    _serialPort.DataReceived -= new SerialDataReceivedEventHandler(_serialPort_DataReceived);
                    if (dispose)
                        _serialPort.Dispose();
                }
                catch (Exception ex)
                {
                    LogError("Error shutting down iRobot connection: " + ex.Message);
                }

                if (dispose)
                    _serialPort = null;
            }
        }


        #endregion


    }



    #region iRobot DssStream Contract

    /// <summary>
    /// iRobotstream Contract
    /// </summary>
    public sealed class Contract
    {
        /// The Unique Contract Identifier for the iRobotstream service
        public const String Identifier = "http://schemas.microsoft.com/robotics/2006/12/irobot/stream.user.html";
    }

    #endregion

    #region Private State
    /// <summary>
    /// iRobot Connection class
    /// </summary>
    public class iRobotConnection
    {
        /// <summary>
        /// Port Name
        /// </summary>
        public string PortName;

        /// <summary>
        /// Baud Rate
        /// </summary>
        public int BaudRate;

        /// <summary>
        /// Encoding
        /// </summary>
        public Encoding Encoding;

        /// <summary>
        /// Parity
        /// </summary>
        public Parity Parity;

        /// <summary>
        /// Data Bits
        /// </summary>
        public int DataBits;

        /// <summary>
        /// Stop Bits
        /// </summary>
        public StopBits StopBits;

        /// <summary>
        /// Roomba Mode
        /// </summary>
        public Roomba.RoombaMode Mode;

        /// <summary>
        /// Connection Type
        /// </summary>
        public Roomba.iRobotConnectionType ConnectionType;

        /// <summary>
        /// Default Constructor
        /// </summary>
        public iRobotConnection()
        {
            this.PortName = string.Empty;
            this.BaudRate = 119200;
            this.Encoding = Encoding.ASCII;
            this.Parity = Parity.None;
            this.DataBits = 8;
            this.StopBits = StopBits.One;
            this.Mode = roomba.RoombaMode.Uninitialized;
            this.ConnectionType = roomba.iRobotConnectionType.NotConfigured;
        }

        /// <summary>
        /// Initialization Constructor
        /// </summary>
        public iRobotConnection(int serialPort)
        {
            this.PortName = "COM" + serialPort.ToString();
            this.BaudRate = 57600;
            this.Encoding = Encoding.Default;
            this.Parity = Parity.None;
            this.DataBits = 8;
            this.StopBits = StopBits.One;
            this.Mode = roomba.RoombaMode.Uninitialized;
            this.ConnectionType = roomba.iRobotConnectionType.NotConfigured;
        }
    }

    #endregion
}
