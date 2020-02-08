// -----------------------------------------------------------------------
// <copyright file="WebCamSensorAlternate.cs" company="Microsoft">
//  Copyright (C) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Robotics.Services.Simulation.Sensors.SimulatedWebcam
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Ccr.Core;
    using Microsoft.Dss.Core.Attributes;
    using Microsoft.Dss.Core.DsspHttp;
    using Microsoft.Dss.ServiceModel.Dssp;
    using Microsoft.Robotics.Services.WebCamSensor;

    using sensor = Microsoft.Robotics.Services.WebCamSensor.Proxy;
    using submgr = Microsoft.Dss.Services.SubscriptionManager;


    /// <summary>
    /// The WebCamSensor alternate contract implementation
    /// </summary>
    [AlternateContract(sensor.Contract.Identifier)]
    public partial class SimulatedWebcamService
    {
        /// <summary>
        /// The Stride factor
        /// </summary>
        private const int StrideFactor = 3;

        /// <summary>
        /// The Sensor Port Name
        /// </summary>
        private const string SensorPortName = "sensorOps";

        /// <summary>
        /// The WebCamSensor operations
        /// </summary>
        [AlternateServicePort("/sensor", AlternateContract = sensor.Contract.Identifier, AllowMultipleInstances = true)]
        private sensor.WebCamSensorOperations sensorOps = new sensor.WebCamSensorOperations();

        /// <summary>
        /// The sensorState
        /// </summary>
        private WebCamSensorState sensorState = new WebCamSensorState();

        /// <summary>
        /// The HTTP Get handler.
        /// </summary>
        /// <param name="get">The get request</param>
        /// <returns>Standard Ccr iterator</returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive, PortFieldName = SensorPortName)]
        public IEnumerator<ITask> AltHttpGetHandler(HttpGet get)
        {
            return HttpHandler(get.Body.Context, get.ResponsePort);
        }

        /// <summary>
        /// The HTTP query handler.
        /// </summary>
        /// <param name="query">The query request.</param>
        /// <returns>Standard ccr iterator</returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive, PortFieldName = SensorPortName)]
        public IEnumerator<ITask> AltHttpQueryHandler(HttpQuery query)
        {
            return HttpHandler(query.Body.Context, query.ResponsePort);
        }

        /// <summary>
        /// Gets the handler.
        /// </summary>
        /// <param name="get">The get request.</param>
        [ServiceHandler(PortFieldName = SensorPortName)]
        public void GetHandler(sensor.Get get)
        {
            get.ResponsePort.Post(this.sensorState);
        }

        /// <summary>
        /// Replaces the handler.
        /// </summary>
        /// <param name="replace">The replace request.</param>
        [ServiceHandler(PortFieldName = SensorPortName)]
        public void ReplaceHandler(sensor.Replace replace)
        {
            this.sensorState = replace.Body;
            this.SendNotification(this._subMgrPort, replace);

            replace.ResponsePort.Post(DefaultReplaceResponseType.Instance);
        }

        /// <summary>
        /// Subscribes the handler.
        /// </summary>
        /// <param name="subscribe">The subscribe request.</param>
        [ServiceHandler(PortFieldName = SensorPortName)]
        public void SubscribeHandler(sensor.Subscribe subscribe)
        {
            // We share a subscription manager with the WebCam main contract, so this ensures we that
            // we automatically filter notifcations based on the contract that the subscription came
            // through
            if (subscribe.Body.TypeFilter == null || subscribe.Body.TypeFilter.Length == 0)
            {
                subscribe.Body.TypeFilter = new[] { GetTypeFilterDescription<sensor.Replace>() };
            }
     
            SubscribeHelper(this._subMgrPort, subscribe, subscribe.ResponsePort);
        }

        /// <summary>
        /// Does the web cam sensor replace.
        /// </summary>
        /// <param name="data">The data.</param>
        private void DoWebCamSensorReplace(int[] data)
        {
            var replace = new WebCamSensorState
                {
                    Data = this.ConvertRawDataToImageData(data),
                    DeviceName = _entity.State.Name,
                    Height = _entity.ViewSizeY,
                    Width = _entity.ViewSizeX,
                    HorizontalFieldOfView = _entity.ViewAngle,
                    Stride = StrideFactor * _entity.ViewSizeX,
                    TimeStamp = DateTime.Now
                };

            this.sensorOps.Replace(replace);
        }

        /// <summary>
        /// Converts the raw data to image data.
        /// </summary>
        /// <param name="rawData">The raw data.</param>
        /// <returns>The image data</returns>
        private byte[] ConvertRawDataToImageData(int[] rawData)
        {
            var imageData = new byte[StrideFactor * _entity.ViewSizeX * _entity.ViewSizeY];

            for (int i = 0, j = 0; i < rawData.Length && (j + StrideFactor - 1) < imageData.Length; ++i, j += StrideFactor)
            {
                var raw = rawData[i];
                imageData[j] = (byte)(raw & 0xFF);
                imageData[j + 1] = (byte)((raw & 0xFF00) >> 8);
                imageData[j + 2] = (byte)((raw & 0xFF0000) >> 16);
            }

            return imageData;
        }
    }
}
