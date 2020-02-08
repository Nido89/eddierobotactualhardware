// -----------------------------------------------------------------------
// <copyright file="DepthCamAlternate.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Robotics.Services.Simulation.Sensors.SimulatedKinect
{
    using Microsoft.Dss.Core.Attributes;
    using Microsoft.Dss.Core.DsspHttp;
    using Microsoft.Dss.ServiceModel.Dssp;
    using Microsoft.Robotics.Services.DepthCamSensor;
    using depthcam = Microsoft.Robotics.Services.DepthCamSensor.Proxy;

    /// <summary>
    /// The SimulatedKinect service
    /// </summary>
    [AlternateContract(depthcam.Contract.Identifier)]
    public partial class SimulatedKinect
    {
        /// <summary>
        /// The DepthCamAltPort
        /// </summary>
        private const string DepthCamAltPort = "depthCamOps";

        /// <summary>
        /// The operations port for the depthcam
        /// </summary>
        [AlternateServicePort("/depthcam", AlternateContract = depthcam.Contract.Identifier)]
        private depthcam.DepthCamSensorOperationsPort depthCamOps = new depthcam.DepthCamSensorOperationsPort();

        /// <summary>
        /// The depthCam partner
        /// </summary>
        [Partner(
            "DepthCam", 
            Contract = depthcam.Contract.Identifier, 
            CreationPolicy = PartnerCreationPolicy.UsePartnerListEntry)]
        private depthcam.DepthCamSensorOperationsPort depthCamPartner = new depthcam.DepthCamSensorOperationsPort();

        /// <summary>
        /// The DepthCam get handler.
        /// </summary>
        /// <param name="get">The get request</param>
        [ServiceHandler(PortFieldName = DepthCamAltPort)]
        public void GetHandler(depthcam.Get get)
        {
            this.depthCamPartner.Post(get);
        }

        /// <summary>
        /// The DepthCam replace handler.
        /// </summary>
        /// <param name="replace">The replace request</param>
        [ServiceHandler(PortFieldName = DepthCamAltPort)]
        public void ReplaceHandler(depthcam.Replace replace)
        {
            this.depthCamPartner.Post(replace);
        }

        /// <summary>
        /// The DepthCam subscribe handler.
        /// </summary>
        /// <param name="subscribe">The subscribe request</param>
        [ServiceHandler(PortFieldName = DepthCamAltPort)]
        public void SubscribeHandler(depthcam.Subscribe subscribe)
        {
            this.ForwardSubscription(this.depthCamPartner, subscribe.Body, subscribe.ResponsePort);
        }

        /// <summary>
        /// The DepthCam http GET handler.
        /// </summary>
        /// <param name="get">The GET request</param>
        [ServiceHandler(PortFieldName = DepthCamAltPort)]
        public void DepthCamHttpGetHandler(HttpGet get)
        {
            this.RedirectHttpRequest(
                depthcam.Contract.Identifier, 
                this.depthCamPartner, 
                get.Body.Context, 
                get.ResponsePort);
        }

        /// <summary>
        /// The DepthCam http QUERY handler.
        /// </summary>
        /// <param name="query">The QUERY request</param>
        [ServiceHandler(PortFieldName = DepthCamAltPort)]
        public void DepthCamHttpQueryHandler(HttpQuery query)
        {
            this.RedirectHttpRequest(
                depthcam.Contract.Identifier,
                this.depthCamPartner,
                query.Body.Context,
                query.ResponsePort);
        }

        /// <summary>
        /// Gets the state of the depth cam.
        /// </summary>
        /// <returns>The response port</returns>
        private DsspResponsePort<DepthCamSensorState> GetDepthCamState()
        {
            return this.depthCamPartner.Get();
        }
    }
}
