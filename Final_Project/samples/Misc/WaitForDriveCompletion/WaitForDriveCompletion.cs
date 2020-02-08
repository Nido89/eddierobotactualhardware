//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
// 
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: WaitForDriveCompletion.cs $ $Revision: 9 $
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Ccr.Core;
using Microsoft.Dss.Core.Attributes;
using Microsoft.Dss.ServiceModel.Dssp;
using Microsoft.Dss.ServiceModel.DsspServiceBase;
using W3C.Soap;
using submgr = Microsoft.Dss.Services.SubscriptionManager;
using drive = Microsoft.Robotics.Services.Drive.Proxy;

namespace Microsoft.Dss.Services.Samples.WaitForDriveCompletion
{
    /// <summary>
    /// WaitForDriveCompletion Service - Waits for a Canceled or Completed notification from a Differential Drive
    /// </summary>
    [Contract(Contract.Identifier)]
    [DisplayName("(User) Wait For Drive Completion")]
    [Description("The WaitForDriveCompletion service waits for Canceled or Completed notifications from a Differential Drive")]
    [DssServiceDescription("http://msdn.microsoft.com/library/cc998519.aspx")]
    class WaitForDriveCompletionService : DsspServiceBase
    {
        /// <summary>
        /// Service state
        /// </summary>
        [ServiceState]
        WaitForDriveCompletionState _state = new WaitForDriveCompletionState();

        // This port is sent a message every time that there is a
        // Canceled or Completed message from the Drive, so it can
        // be used to wait for completion.
        Port<drive.DriveStage> completionPort = new Port<drive.DriveStage>();

        /// <summary>
        /// Main service port
        /// </summary>
        [ServicePort("/WaitForDriveCompletion", AllowMultipleInstances = true)]
        WaitForDriveCompletionOperations _mainPort = new WaitForDriveCompletionOperations();

        [SubscriptionManagerPartner]
        submgr.SubscriptionManagerPort _submgrPort = new submgr.SubscriptionManagerPort();

        /// <summary>
        /// DriveDifferentialTwoWheel partner
        /// </summary>
        [Partner("DriveDifferentialTwoWheel", Contract = drive.Contract.Identifier, CreationPolicy = PartnerCreationPolicy.UseExisting)]
        drive.DriveOperations _drivePort = new drive.DriveOperations();
        drive.DriveOperations _driveNotify = new drive.DriveOperations();

        /// <summary>
        /// Service constructor
        /// </summary>
        public WaitForDriveCompletionService(DsspServiceCreationPort creationPort)
            : base(creationPort)
        {
        }

        /// <summary>
        /// Service start
        /// </summary>
        protected override void Start()
        {
            // Initialize the state
            if (_state == null)
                _state = new WaitForDriveCompletionState();
            _state.LastStatus = drive.DriveStage.InitialRequest;

            // Subscribe to the drive for notification messages
            _drivePort.Subscribe(_driveNotify);

            // Add the necessary receivers
            // NOTE: These are INDEPENDENT receivers.
            // This is OK because they do not affect the state of the service.
            Activate(Arbiter.ReceiveWithIterator<drive.DriveDistance>(true, _driveNotify, DriveDistanceUpdateHandler));
            Activate(Arbiter.ReceiveWithIterator<drive.RotateDegrees>(true, _driveNotify, RotateDegreesUpdateHandler));

            base.Start();
        }

        #region Handlers

        /// <summary>
        /// DriveDistanceUpdateHandler - Posts a message on Canceled or Completed
        /// </summary>
        /// <param name="distance"></param>
        /// <returns></returns>
        IEnumerator<ITask> DriveDistanceUpdateHandler(drive.DriveDistance distance)
        {
            if (distance.Body.DriveDistanceStage == drive.DriveStage.Canceled)
            {
                completionPort.Post(distance.Body.DriveDistanceStage);
            }
            if (distance.Body.DriveDistanceStage == drive.DriveStage.Completed)
            {
                completionPort.Post(distance.Body.DriveDistanceStage);
            }
            yield break;
        }

        /// <summary>
        /// RotateDegreesUpdateHandler - Posts a message on Canceled or Completed
        /// </summary>
        /// <param name="rotate"></param>
        /// <returns></returns>
        IEnumerator<ITask> RotateDegreesUpdateHandler(drive.RotateDegrees rotate)
        {
            if (rotate.Body.RotateDegreesStage == drive.DriveStage.Canceled)
            {
                completionPort.Post(rotate.Body.RotateDegreesStage);
            }
            if (rotate.Body.RotateDegreesStage == drive.DriveStage.Completed)
            {
                completionPort.Post(rotate.Body.RotateDegreesStage);
            }
            yield break;
        }

        /// <summary>
        /// DriveUpdateHandler - Prevents Update messages from filling the queue
        /// </summary>
        /// <param name="update"></param>
        /// <returns></returns>
        IEnumerator<ITask> DriveUpdateHandler(drive.Update update)
        {
            // Ignore update messages because we are not interested
            yield break;
        }

        /// <summary>
        /// Handles Replace messages
        /// </summary>
        /// <param name="replace">The Replace request</param>
        [ServiceHandler]
        public void ReplaceHandler(Replace replace)
        {
            _state = replace.Body;
            SendNotification<Replace>(_submgrPort, replace.Body);
            replace.ResponsePort.Post(DefaultReplaceResponseType.Instance);
        }

        /// <summary>
        /// Handles Subscribe messages
        /// </summary>
        /// <param name="subscribe">The Subscribe request</param>
        [ServiceHandler]
        public void SubscribeHandler(Subscribe subscribe)
        {
            SubscribeHelper(_submgrPort, subscribe.Body, subscribe.ResponsePort);
            SendNotificationToTarget<Replace>(subscribe.Body.Subscriber, _submgrPort, _state);
        }

        /// <summary>
        /// Wait - Waits for a drive notification of Canceled or Completed
        /// </summary>
        /// <param name="wait">Wait request</param>
        /// <returns>Iterator</returns>
        [ServiceHandler]
        public IEnumerator<ITask> WaitHandler(Wait wait)
        {
            // Create a variable to hold the completion status
            drive.DriveStage result = drive.DriveStage.InitialRequest;

            // Wait until the next Canceled or Completed notification
            // NOTE: There is no guarantee that the notification is the result
            // of a particular drive motion request, but it drive requests are
            // always paired with Waits then this should be the case.
            yield return (Receiver<drive.DriveStage>)Arbiter.Receive(false, completionPort,
                delegate(drive.DriveStage status) { result = status; });

            // Make a new state and send it to ourselves as a Replace message
            // This will force a notification out to subscribers
            WaitForDriveCompletionState body = new WaitForDriveCompletionState();
            body.LastStatus = result;
            _mainPort.Post(new Replace(body));

            // Finally, send back the response message so that the caller
            // can now continue
            wait.ResponsePort.Post(new WaitResponseType(result));

            yield break;
        }

        #endregion

    }
}
