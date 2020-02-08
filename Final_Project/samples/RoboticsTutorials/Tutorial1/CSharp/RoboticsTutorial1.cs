//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: RoboticsTutorial1.cs $ $Revision: 17 $
//-----------------------------------------------------------------------

using Microsoft.Ccr.Core;
using Microsoft.Dss.Core;
using Microsoft.Dss.Core.Attributes;
using Microsoft.Dss.ServiceModel.Dssp;
using Microsoft.Dss.ServiceModel.DsspServiceBase;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml;
using roboticstutorial1 = Microsoft.Robotics.Services.RoboticsTutorial1;
#region CODECLIP 01
using bumper = Microsoft.Robotics.Services.ContactSensor.Proxy;
#endregion

namespace Microsoft.Robotics.Services.RoboticsTutorial1
{
    [DisplayName("(User) Robotics Tutorial 1 (C#): Accessing a Service")]
    [Description("This tutorial demonstrates a service that reads the output of a contact sensor and displays a message in the console window.")]
    [DssServiceDescription("http://msdn.microsoft.com/library/bb483050.aspx")]
    [Contract(Contract.Identifier)]
    public class RoboticsTutorial1Service : DsspServiceBase
    {
        [ServiceState]
        private RoboticsTutorial1State _state = new RoboticsTutorial1State();

        [ServicePort("/RoboticsTutorial1", AllowMultipleInstances = false)]
        private Tutorial1Operations _mainPort = new Tutorial1Operations();

        #region CODECLIP 02
        [Partner("bumper", Contract = bumper.Contract.Identifier,
            CreationPolicy = PartnerCreationPolicy.UseExisting)]
        private bumper.ContactSensorArrayOperations _bumperPort = new bumper.ContactSensorArrayOperations();
        #endregion

        /// <summary>
        /// Default Service Constructor
        /// </summary>
        public RoboticsTutorial1Service(DsspServiceCreationPort creationPort) :
                base(creationPort)
        {
        }

        /// <summary>
        /// Service Initialization
        /// </summary>
        protected override void Start()
        {
            base.Start();

            #region CODECLIP 03
            // Start listening for bumpers.
            SubscribeToBumpers();
            #endregion
        }

        #region CODECLIP 04
        /// <summary>
        /// Subscribe to the Bumpers service
        /// </summary>
        void SubscribeToBumpers()
        {
            // Create the bumper notification port.
            #region CODECLIP 04-1
            bumper.ContactSensorArrayOperations bumperNotificationPort = new bumper.ContactSensorArrayOperations();
            #endregion

            // Subscribe to the bumper service, receive notifications on the bumperNotificationPort.
            #region CODECLIP 04-2
            _bumperPort.Subscribe(bumperNotificationPort);
            #endregion

            // Start listening for updates from the bumper service.
            #region CODECLIP 04-3
            Activate(
                Arbiter.Receive<bumper.Update>
                    (true, bumperNotificationPort, BumperHandler));
            #endregion
        }
        #endregion

        /// <summary>
        /// Return the current state of this service
        /// </summary>
        /// <param name="get">Get message</param>
        // NOTE: This handler is not required if the state is tagged with the
        // ServiceState attribute because it has the default behavior.
        // However, it is included here to show how it works.
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public virtual IEnumerator<ITask> GetHandler(Get get)
        {
            get.ResponsePort.Post(_state);
            yield break;
        }

        #region CODECLIP 05
        /// <summary>
        /// Handle Bumper Notifications
        /// </summary>
        /// <param name="notification">Update notification</param>
        private void BumperHandler(bumper.Update notification)
        {
            if (notification.Body.Pressed)
                LogInfo(LogGroups.Console, "Ouch - the bumper was pressed.");
        }
        #endregion
    }
}
