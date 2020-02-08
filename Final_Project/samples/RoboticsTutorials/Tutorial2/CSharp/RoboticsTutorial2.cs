//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: RoboticsTutorial2.cs $ $Revision: 18 $
//-----------------------------------------------------------------------

using Microsoft.Ccr.Core;
using Microsoft.Dss.Core;
using Microsoft.Dss.Core.Attributes;
using Microsoft.Dss.ServiceModel.Dssp;
using Microsoft.Dss.ServiceModel.DsspServiceBase;
using System;
using System.Collections.Generic;
using System.Security.Permissions;

using bumper = Microsoft.Robotics.Services.ContactSensor.Proxy;
#region CODECLIP 01-1
using motor = Microsoft.Robotics.Services.Motor.Proxy;
#endregion
using System.ComponentModel;

namespace Microsoft.Robotics.Services.RoboticsTutorial2
{
    [DisplayName("(User) Robotics Tutorial 2 (C#): Coordinating Services")]
    [Description("This tutorial is a continuation of Robotics Tutorial 1 and shows how to use a contact sensor to start and stop a motor.")]
    [DssServiceDescription("http://msdn.microsoft.com/library/bb483051.aspx")]
    [Contract(Contract.Identifier)]
    public class RoboticsTutorial2Service : DsspServiceBase
    {
        [ServiceState]
        private RoboticsTutorial2State _state = new RoboticsTutorial2State();

        [ServicePort("/RoboticsTutorial2", AllowMultipleInstances = false)]
        private RoboticsTutorial2Operations _mainPort = new RoboticsTutorial2Operations();

        [Partner("bumper", Contract = bumper.Contract.Identifier,
            CreationPolicy = PartnerCreationPolicy.UseExisting)]
        private bumper.ContactSensorArrayOperations _bumperPort = new bumper.ContactSensorArrayOperations();

        #region CODECLIP 01-2
        [Partner("motor", Contract = motor.Contract.Identifier,
            CreationPolicy = PartnerCreationPolicy.UseExisting)]
        private motor.MotorOperations _motorPort = new motor.MotorOperations();
        #endregion

        public RoboticsTutorial2Service(DsspServiceCreationPort creationPort) :
                base(creationPort)
        {
        }

        protected override void Start()
        {
            base.Start();

            // Start listening for bumpers
            SubscribeToBumpers();
        }

        /// <summary>
        /// Replace Handler
        /// </summary>
        /// <param name="replace">Replace message</param>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public virtual IEnumerator<ITask> ReplaceHandler(Replace replace)
        {
            _state = replace.Body;
            replace.ResponsePort.Post(DefaultReplaceResponseType.Instance);
            yield break;
        }

        #region CODECLIP 03-2
        /// <summary>
        /// Handle Bumper Notifications
        /// </summary>
        /// <param name="notification">Update notification</param>
        private void BumperHandler(bumper.Update notification)
        {
            string message;

            if (!notification.Body.Pressed)
                return;

            // Toggle the motor state
            _state.MotorOn = !_state.MotorOn;

            // Create a motor request
            motor.SetMotorPowerRequest motorRequest = new motor.SetMotorPowerRequest();

            if (_state.MotorOn)
            {
                // Set the motor power on
                motorRequest.TargetPower = 1.0;
                message = "Motor On";
            }
            else
            {
                // Set the motor power off
                motorRequest.TargetPower = 0.0;
                message = "Motor Off";
            }

            // Send the motor request
            _motorPort.SetMotorPower(motorRequest);

            // Show the motor status on the console screen
            LogInfo(LogGroups.Console, message);
        }
        #endregion

        #region CODECLIP 02-1
        /// <summary>
        /// Subscribe to notifications when bumpers are pressed
        /// </summary>
        void SubscribeToBumpers()
        {
            // Create bumper notification port
            bumper.ContactSensorArrayOperations bumperNotificationPort = new bumper.ContactSensorArrayOperations();

            // Subscribe to the bumper service, send notifications to bumperNotificationPort
            _bumperPort.Subscribe(bumperNotificationPort);

            // Start listening for Bumper Replace notifications
            Activate(
                Arbiter.Receive<bumper.Update>
                    (true, bumperNotificationPort, BumperHandler));
        }
        #endregion

    }
}
