//------------------------------------------------------------------------------
//  <copyright file="Drive.cs" company="Microsoft Corporation">
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
    using Microsoft.Dss.Core.DsspHttpUtilities;
    using Microsoft.Dss.ServiceModel.Dssp;
    using Microsoft.Dss.ServiceModel.DsspServiceBase;

    using drive = Microsoft.Robotics.Services.Drive;
    using motor = Microsoft.Robotics.Services.Motor;
    using sensor = Microsoft.Robotics.Services.AnalogSensor;
    using sensors = Microsoft.Robotics.Services.AnalogSensorArray;
    using soap = W3C.Soap;
    using submgr = Microsoft.Dss.Services.SubscriptionManager;

    /// <summary>
    /// Main service class
    /// </summary>
    public partial class MarkRobotService : DsspServiceBase
    {
        /// <summary>
        /// Drive Port Identifier used in attributes
        /// </summary>
        private const string DrivePortName = "drivePort";

        /// <summary>
        /// Alternate contract service port
        /// </summary>
        [AlternateServicePort(AlternateContract = drive.Contract.Identifier)]
        private drive.DriveOperations drivePort = new drive.DriveOperations();

        /// <summary>
        /// Controller board service drive port
        /// </summary>
        [Partner("ReferencePlatformIOControllerDrive", Contract = drive.Contract.Identifier, CreationPolicy = PartnerCreationPolicy.UsePartnerListEntry)]
        private drive.DriveOperations controllerDrivePort = new drive.DriveOperations();

        /// <summary>
        /// Notification port from the underlying service
        /// </summary>
        private drive.DriveOperations driveNotifyPort = new drive.DriveOperations();

        /// <summary>
        /// Drive service subscription port
        /// </summary>
        [SubscriptionManagerPartner("Drive")]
        private submgr.SubscriptionManagerPort submgrDrivePort = new submgr.SubscriptionManagerPort();

        /// <summary>
        /// Handles Get requests on alternate port: Drive
        /// </summary>
        /// <param name="get">Request message</param>
        [ServiceHandler(PortFieldName = DrivePortName)]
        public void DriveGetHandler(drive.Get get)
        {
            this.controllerDrivePort.Post(
                new drive.Get() { Body = get.Body, ResponsePort = get.ResponsePort });
        }

        /// <summary>
        /// Handles EnableDrive requests on alternate port: Drive
        /// </summary>
        /// <param name="enabledrive">Request message</param>
        [ServiceHandler(PortFieldName = DrivePortName)]
        public void DriveEnableDriveHandler(drive.EnableDrive enabledrive)
        {
            this.controllerDrivePort.Post(
                new drive.EnableDrive() { Body = enabledrive.Body, ResponsePort = enabledrive.ResponsePort });
        }

        /// <summary>
        /// Handles ResetEncoders requests on alternate port: Drive
        /// </summary>
        /// <param name="resetEncoders">Request message</param>
        [ServiceHandler(PortFieldName = DrivePortName)]
        public void DriveResetEncodersHandler(drive.ResetEncoders resetEncoders)
        {
            this.controllerDrivePort.Post(
                new drive.ResetEncoders() { Body = resetEncoders.Body, ResponsePort = resetEncoders.ResponsePort });
        }

        /// <summary>
        /// Handles SetDrivePower requests on alternate port: Drive
        /// </summary>
        /// <param name="setdrivepower">Request message</param>
        [ServiceHandler(PortFieldName = DrivePortName)]
        public void DriveSetDrivePowerHandler(drive.SetDrivePower setdrivepower)
        {
            this.controllerDrivePort.Post(
                new drive.SetDrivePower() { Body = setdrivepower.Body, ResponsePort = setdrivepower.ResponsePort });
        }

        /// <summary>
        /// Handles AllStop requests on alternate port: Drive
        /// </summary>
        /// <param name="allstop">Request message</param>
        [ServiceHandler(PortFieldName = DrivePortName)]
        public void DriveAllStopHandler(drive.AllStop allstop)
        {
            this.controllerDrivePort.Post(
                new drive.AllStop() { Body = allstop.Body, ResponsePort = allstop.ResponsePort });
        }

        /// <summary>
        /// Handles RotateDegrees requests on alternate port: Drive
        /// Rotates in place, N degrees, and P power
        /// </summary>
        /// <param name="rotatedegrees">Request message</param>
        [ServiceHandler(PortFieldName = DrivePortName)]
        public void DriveRotateDegreesHandler(drive.RotateDegrees rotatedegrees)
        {
            this.controllerDrivePort.Post(
                new drive.RotateDegrees() { Body = rotatedegrees.Body, ResponsePort = rotatedegrees.ResponsePort });
        }

        /// <summary>
        /// Handles DriveDistance requests on alternate port: Drive
        /// </summary>
        /// <param name="drivedistance">Request message</param>
        [ServiceHandler(PortFieldName = DrivePortName)]
        public void DriveDriveDistanceHandler(drive.DriveDistance drivedistance)
        {
            this.controllerDrivePort.Post(
                new drive.DriveDistance() { Body = drivedistance.Body, ResponsePort = drivedistance.ResponsePort });
        }

        /// <summary>
        /// Handles SetDriveSpeed requests on alternate port: Drive
        /// </summary>
        /// <param name="setdrivespeed">Request message</param>
        [ServiceHandler(PortFieldName = DrivePortName)]
        public void DriveSetDriveSpeedHandler(drive.SetDriveSpeed setdrivespeed)
        {
            this.controllerDrivePort.Post(
                new drive.SetDriveSpeed() { Body = setdrivespeed.Body, ResponsePort = setdrivespeed.ResponsePort });
        }

        /// <summary>
        /// Handles Update requests on alternate port: Drive
        /// </summary>
        /// <param name="update">Request message</param>
        [ServiceHandler(PortFieldName = DrivePortName)]
        public void DriveUpdateHandler(drive.Update update)
        {
            this.controllerDrivePort.Post(
                new drive.Update() { Body = update.Body, ResponsePort = update.ResponsePort });
        }

        /// <summary>
        /// Handles HttpGet requests on alternate port: Drive
        /// </summary>
        /// <param name="httpget">Request message</param>
        /// <returns>A CCR task iterator</returns>
        [ServiceHandler(PortFieldName = DrivePortName)]
        public IEnumerator<ITask> DriveHttpGetHandler(Microsoft.Dss.Core.DsspHttp.HttpGet httpget)
        {
            drive.Get get = new drive.Get();

            this.controllerDrivePort.Post(get);

            yield return get.ResponsePort.Choice();

            drive.DriveDifferentialTwoWheelState newState = (drive.DriveDifferentialTwoWheelState)get.ResponsePort;

            if (newState != null)
            {
                HttpResponseType resp = new HttpResponseType(HttpStatusCode.OK, newState);
                httpget.ResponsePort.Post(resp);
            }
            else
            {
                HttpResponseType resp = new HttpResponseType(HttpStatusCode.InternalServerError, (soap.Fault)get.ResponsePort);
                httpget.ResponsePort.Post(resp);
            }

            yield break;
        }

        /// <summary>
        /// Handles HttpPost requests on alternate port: Drive
        /// </summary>
        /// <param name="httppost">Request message</param>
        [ServiceHandler(PortFieldName = DrivePortName)]
        public void DriveHttpPostHandler(Microsoft.Dss.Core.DsspHttp.HttpPost httppost)
        {
            var request = httppost.GetHeader<HttpPostRequestData>();

            if (request == null || request.TranslatedOperation == null)
            {
                httppost.ResponsePort.Post(new HttpResponseType(HttpStatusCode.BadRequest, soap.Fault.FromException(new InvalidOperationException())));
            }

            var op = request.TranslatedOperation;
            var enableddrive = op as drive.EnableDrive;
            var setdrivepower = op as drive.SetDrivePower;
            var allstop = op as drive.AllStop;
            var rotatedegrees = op as drive.RotateDegrees;
            var drivedistance = op as drive.DriveDistance;
            var setdrivespeed = op as drive.SetDriveSpeed;
            var update = op as drive.Update;
            var resetencoders = op as drive.ResetEncoders;

            if (enableddrive != null)
            {
                this.drivePort.Post(enableddrive);
            }
            else if (resetencoders != null)
            {
                this.drivePort.Post(resetencoders);
            }
            else if (setdrivepower != null)
            {
                this.drivePort.Post(setdrivepower);
            }
            else if (allstop != null)
            {
                this.drivePort.Post(allstop);
            }
            else if (rotatedegrees != null)
            {
                this.drivePort.Post(rotatedegrees);
            }
            else if (drivedistance != null)
            {
                this.drivePort.Post(drivedistance);
            }
            else if (setdrivespeed != null)
            {
                this.drivePort.Post(setdrivespeed);
            }
            else if (update != null)
            {
                this.drivePort.Post(update);
            }
            else
            {
                httppost.ResponsePort.Post(new HttpResponseType(HttpStatusCode.BadRequest, soap.Fault.FromException(new InvalidOperationException())));
                return;
            }

            httppost.ResponsePort.Post(new HttpResponseType(HttpStatusCode.OK, this.state));
        }

        #region SubscriptionHandlers

        /// <summary>
        /// Handles ReliableSubscribe requests on alternate port Drive
        /// </summary>
        /// <param name="reliablesubscribe">Request message</param>
        [ServiceHandler(PortFieldName = DrivePortName)]
        public void DriveReliableSubscribeHandler(drive.ReliableSubscribe reliablesubscribe)
        {
            SubscribeHelper(this.submgrDrivePort, reliablesubscribe.Body, reliablesubscribe.ResponsePort);
        }

        /// <summary>
        /// Handles Subscribe requests on alternate port Drive
        /// </summary>
        /// <param name="subscribe">Request message</param>
        [ServiceHandler(PortFieldName = DrivePortName)]
        public void DriveSubscribeHandler(drive.Subscribe subscribe)
        {
            SubscribeHelper(this.submgrDrivePort, subscribe.Body, subscribe.ResponsePort);
        }

        /// <summary>
        /// Notification handler from underlying drive service
        /// </summary>
        /// <param name="notifyUpdate">Notification update from drive service</param>
        private void DriveNotification(drive.Update notifyUpdate)
        {
            SendNotification<drive.Update>(this.submgrDrivePort, new drive.Update(notifyUpdate.Body));
        }

        #endregion
    }
}