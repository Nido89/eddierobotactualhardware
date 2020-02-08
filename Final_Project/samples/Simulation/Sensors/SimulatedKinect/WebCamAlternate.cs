// -----------------------------------------------------------------------
// <copyright file="WebCamAlternate.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Robotics.Services.Simulation.Sensors.SimulatedKinect
{
    using Microsoft.Ccr.Core;
    using Microsoft.Ccr.Core.Arbiters;
    using Microsoft.Dss.Core.Attributes;
    using Microsoft.Dss.Core.DsspHttp;
    using Microsoft.Dss.ServiceModel.Dssp;

    using webcam = Microsoft.Robotics.Services.WebCamSensor.Proxy;

    /// <summary>
    /// The SimulatedKinect service
    /// </summary>
    [AlternateContract(webcam.Contract.Identifier)]
    public partial class SimulatedKinect
    {
        /// <summary>
        /// The WebCamAltPort
        /// </summary>
        private const string WebCamAltPort = "webCamOps";

        /// <summary>
        /// The operations port for the webcam
        /// </summary>
        [AlternateServicePort("/webcam", AlternateContract = webcam.Contract.Identifier)]
        private webcam.WebCamSensorOperations webCamOps = new webcam.WebCamSensorOperations();

        /// <summary>
        /// The webcam partner
        /// </summary>
        [Partner(
            "WebCam",
            Contract = webcam.Contract.Identifier,
            CreationPolicy = PartnerCreationPolicy.UsePartnerListEntry)]
        private webcam.WebCamSensorOperations webCamPartner = new webcam.WebCamSensorOperations();

        /// <summary>
        /// The WebCam get handler.
        /// </summary>
        /// <param name="get">The get request</param>
        [ServiceHandler(PortFieldName = WebCamAltPort)]
        public void GetHandler(webcam.Get get)
        {
            this.webCamPartner.Post(get);
        }

        /// <summary>
        /// The WebCam replace handler.
        /// </summary>
        /// <param name="replace">The replace request</param>
        [ServiceHandler(PortFieldName = WebCamAltPort)]
        public void ReplaceHandler(webcam.Replace replace)
        {
            this.webCamPartner.Post(replace);
        }

        /// <summary>
        /// The WebCam subscribe handler.
        /// </summary>
        /// <param name="subscribe">The subscribe request</param>
        [ServiceHandler(PortFieldName = WebCamAltPort)]
        public void SubscribeHandler(webcam.Subscribe subscribe)
        {
            // Because the subscription is forwarded directly to the simulated webcam's
            // subscription manager, we specify a type filter directly if none is provided.
            if (subscribe.Body.TypeFilter == null || subscribe.Body.TypeFilter.Length == 0)
            {
                subscribe.Body.TypeFilter = new[] { GetTypeFilterDescription<webcam.Replace>() };
            }

            this.ForwardSubscription(this.webCamPartner, subscribe.Body, subscribe.ResponsePort);
        }

        /// <summary>
        /// The WebCam http GET handler.
        /// </summary>
        /// <param name="get">The GET request</param>
        [ServiceHandler(PortFieldName = WebCamAltPort)]
        public void WebCamHttpGetHandler(HttpGet get)
        {
            this.RedirectHttpRequest(
                webcam.Contract.Identifier, 
                this.webCamPartner,
                get.Body.Context,
                get.ResponsePort);
        }

        /// <summary>
        /// The WebCam http QUERY handler.
        /// </summary>
        /// <param name="query">The QUERY request</param>
        [ServiceHandler(PortFieldName = WebCamAltPort)]
        public void WebCamHttpQueryHandler(HttpQuery query)
        {
            this.RedirectHttpRequest(
                webcam.Contract.Identifier,
                this.webCamPartner,
                query.Body.Context,
                query.ResponsePort);
        }
    }
}
