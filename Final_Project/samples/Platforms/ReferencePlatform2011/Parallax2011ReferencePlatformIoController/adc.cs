//------------------------------------------------------------------------------
//  <copyright file="adc.cs" company="Microsoft Corporation">
//      Copyright (C) Microsoft Corporation.  All rights reserved.
//  </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Robotics.ReferencePlatform.Parallax2011ReferencePlatformIoController
{
    using System.Net;

    using Microsoft.Dss.Core.Attributes;
    using Microsoft.Dss.Core.DsspHttp;
    using Microsoft.Dss.ServiceModel.DsspServiceBase;

    using adc = Microsoft.Robotics.Services.ADCPinArray;
    using submgr = Microsoft.Dss.Services.SubscriptionManager;

    /// <summary>
    /// Main service class
    /// </summary>
    public partial class Parallax2011ReferencePlatformIoControllerService : DsspServiceBase
    {
        /// <summary>
        /// ADC Port Identifier used in attributes
        /// </summary>
        private const string ADCPortName = "adcPort";

        /// <summary>
        /// Alternate contract service port
        /// </summary>
        [AlternateServicePort(AlternateContract = adc.Contract.Identifier)]
        private adc.ADCPinArrayOperations adcPort = new adc.ADCPinArrayOperations();

        /// <summary>
        /// ADC Pin Array service subscription port
        /// </summary>
        [SubscriptionManagerPartner("adc")]
        private submgr.SubscriptionManagerPort submgrAdcPort = new submgr.SubscriptionManagerPort();

        /// <summary>
        /// Handles Get requests on alternate port
        /// </summary>
        /// <param name="get">Request message</param>
        [ServiceHandler(PortFieldName = ADCPortName)]
        public void AdcGetHandler(adc.Get get)
        {
            get.ResponsePort.Post(this.state.AdcState);
        }

        /// <summary>
        /// Handles HttpGet requests on alternate port
        /// </summary>
        /// <param name="httpget">Request message</param>
        [ServiceHandler(PortFieldName = ADCPortName)]
        public void AdcHttpGetHandler(Microsoft.Dss.Core.DsspHttp.HttpGet httpget)
        {
            HttpResponseType resp = new HttpResponseType(HttpStatusCode.OK, this.state.AdcState);
            httpget.ResponsePort.Post(resp);
        }
    }
}