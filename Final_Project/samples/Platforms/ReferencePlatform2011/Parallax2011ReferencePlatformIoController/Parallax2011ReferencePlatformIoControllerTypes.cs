//------------------------------------------------------------------------------
//  <copyright file="Parallax2011ReferencePlatformIoControllerTypes.cs" company="Microsoft Corporation">
//      Copyright (C) Microsoft Corporation.  All rights reserved.
//  </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Robotics.ReferencePlatform.Parallax2011ReferencePlatformIoController
{
    using System;
    using Microsoft.Ccr.Core;
    using Microsoft.Dss.Core.Attributes;
    using Microsoft.Dss.Core.DsspHttp;
    using Microsoft.Dss.ServiceModel.Dssp;
    using W3C.Soap;

    using adc = Microsoft.Robotics.Services.ADCPinArray;
    using drive = Microsoft.Robotics.Services.Drive;
    using gpio = Microsoft.Robotics.Services.GpioPinArray;

    /// <summary>
    /// Parallax2011ReferencePlatformIoController contract class
    /// </summary>
    public sealed class Contract
    {
        /// <summary>
        /// DSS contract identifer for Parallax2011ReferencePlatformIoController
        /// </summary>
        [DataMember]
        public const string Identifier = "http://www.microsoft.com/2011/07/parallax2011referenceplatformiocontroller.html";
    }

    /// <summary>
    /// Parallax2011ReferencePlatformIoController state
    /// </summary>
    [DataContract]
    public class Parallax2011ReferencePlatformIoControllerState
    {
        /// <summary>
        /// Timestamp information for the service state
        /// </summary>
        [DataMember]
        public DateTime LastStartTime = DateTime.Now;

        /// <summary>
        /// Firmware version of the Parallax microcontroller
        /// </summary>
        [DataMember]
        public int FWVersion = 0;

        /// <summary>
        /// Acceleration ramping rate
        /// </summary>
        [DataMember]
        public int AccelerationRate = 5;

        /// <summary>
        /// Time in MS to wait for a response from FW over serial line
        /// </summary>
        [DataMember]
        public int DefaultResponsePause = 50; // Any faster than this and the Parallax FW can't keep up

        /// <summary>
        /// Time in MS between retrieving sensor values
        /// </summary>
        [DataMember]
        public int PinPollingInterval = 100;

        /// <summary>
        /// Alternate contract state data for drive
        /// </summary>
        [DataMember]
        public drive.DriveDifferentialTwoWheelState DriveState = new drive.DriveDifferentialTwoWheelState();

        /// <summary>
        /// Alternate contract state data for GPIOPinArray
        /// </summary>
        [DataMember]
        public gpio.GpioPinArrayState GpioState = new gpio.GpioPinArrayState();

        /// <summary>
        /// Alternate contract state data for ADCPinArray
        /// </summary>
        [DataMember]
        public adc.ADCPinArrayState AdcState = new adc.ADCPinArrayState();
    }

    /// <summary>
    /// Parallax2011ReferencePlatformIoController main operations port
    /// </summary>
    [ServicePort]
    public class Parallax2011ReferencePlatformIoControllerOperations : PortSet<DsspDefaultLookup, DsspDefaultDrop, Get, HttpGet, Subscribe>
    {
    }

    /// <summary>
    /// Parallax2011ReferencePlatformIoController get operation
    /// Boilerplate interface definition, no code required
    /// </summary>
    public class Get : Get<GetRequestType, PortSet<Parallax2011ReferencePlatformIoControllerState, Fault>>
    {
        /// <summary>
        /// Creates a new instance of Get
        /// </summary>
        public Get()
        {
        }

        /// <summary>
        /// Creates a new instance of Get
        /// </summary>
        /// <param name="body">The request message body</param>
        public Get(GetRequestType body) : base(body) 
        {
        }

        /// <summary>
        /// Creates a new instance of Get
        /// </summary>
        /// <param name="body">The request message body</param>
        /// <param name="responsePort">The response port for the request</param>
        public Get(GetRequestType body, PortSet<Parallax2011ReferencePlatformIoControllerState, Fault> responsePort) : base(body, responsePort) 
        {
        }
    }

    /// <summary>
    /// Parallax2011ReferencePlatformIoController Replace operation
    /// </summary>
    public class Replace : Replace<Parallax2011ReferencePlatformIoControllerState, PortSet<DefaultReplaceResponseType, Fault>>
    {
        /// <summary>
        /// Default no-param ctor
        /// </summary>
        public Replace() 
        {
        }

        /// <summary>
        /// Service State-based ctor
        /// </summary>
        /// <param name="state">Service State</param>
        public Replace(Parallax2011ReferencePlatformIoControllerState state) : base(state) 
        {
        }

        /// <summary>
        /// State and Port ctor
        /// </summary>
        /// <param name="state">Service State</param>
        /// <param name="responsePort">Response Port</param>
        public Replace(Parallax2011ReferencePlatformIoControllerState state, PortSet<DefaultReplaceResponseType, Fault> responsePort)
            : base(state, responsePort)
        { 
        }
    }
    
    /// <summary>
    /// Parallax2011ReferencePlatformIoController subscribe operation
    /// </summary>
    public class Subscribe : Subscribe<SubscribeRequestType, PortSet<SubscribeResponseType, Fault>>
    {
        /// <summary>
        /// Creates a new instance of Subscribe
        /// </summary>
        public Subscribe()
        {
        }

        /// <summary>
        /// Creates a new instance of Subscribe
        /// </summary>
        /// <param name="body">The request message body</param>
        public Subscribe(SubscribeRequestType body) : base(body)
        {
        }

        /// <summary>
        /// Creates a new instance of Subscribe
        /// </summary>
        /// <param name="body">The request message body</param>
        /// <param name="responsePort">The response port for the request</param>
        public Subscribe(SubscribeRequestType body, PortSet<SubscribeResponseType, Fault> responsePort) : base(body, responsePort)
        {
        }
    }
}
