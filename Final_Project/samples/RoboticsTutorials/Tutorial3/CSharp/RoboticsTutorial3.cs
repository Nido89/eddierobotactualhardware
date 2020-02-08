//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: RoboticsTutorial3.cs $ $Revision: 20 $
//-----------------------------------------------------------------------

using Microsoft.Ccr.Core;
using Microsoft.Dss.Core;
using Microsoft.Dss.Core.Attributes;
using Microsoft.Dss.ServiceModel.Dssp;
using Microsoft.Dss.ServiceModel.DsspServiceBase;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Security.Permissions;
using xml = System.Xml;
#region CODECLIP 01-1
using contactsensor = Microsoft.Robotics.Services.ContactSensor.Proxy;
using drive = Microsoft.Robotics.Services.Drive.Proxy;
using motor = Microsoft.Robotics.Services.Motor.Proxy;
#endregion

namespace Microsoft.Robotics.Services.RoboticsTutorial3
{
    [DisplayName("(User) Robotics Tutorial 3 (C#): Creating Reusable Orchestration Services")]
    [Description("This tutorial demonstrates how to create a service that partners with abstract, base definitions of hardware services.")]
    [DssServiceDescription("http://msdn.microsoft.com/library/bb483052.aspx")]
    [Contract(Contract.Identifier)]
    public class RoboticsTutorial3Service : DsspServiceBase
    {
        #region CODECLIP 01-2
        [Partner("Bumper",
            Contract = contactsensor.Contract.Identifier,
            CreationPolicy = PartnerCreationPolicy.UseExisting)]
        contactsensor.ContactSensorArrayOperations _contactSensorPort = new contactsensor.ContactSensorArrayOperations();

        [Partner("Drive",
            Contract = drive.Contract.Identifier,
            CreationPolicy = PartnerCreationPolicy.UseExisting)]
        drive.DriveOperations _drivePort = new drive.DriveOperations();
        #endregion

        #region CODECLIP 02-1
        // target port for bumper notifications
        contactsensor.ContactSensorArrayOperations _contactNotificationPort = new contactsensor.ContactSensorArrayOperations();
        #endregion

        [ServicePort("/RoboticsTutorial3", AllowMultipleInstances=false)]
        private RoboticsTutorial3Operations _mainPort = new RoboticsTutorial3Operations();

        public RoboticsTutorial3Service(DsspServiceCreationPort creationPort) :
                base(creationPort)
        {
        }

        #region CODECLIP 02-2
        /// <summary>
        /// Service Start
        /// </summary>
        protected override void Start()
        {
            // Listen on the main port for requests and call the appropriate handler.
            // Publish the service to the local Node Directory
            base.Start();

            SubscribeToBumperSensors();
        }

        /// <summary>
        /// Subscribe to sensors and send notifications
        /// to BumperNotificationHandler
        /// </summary>
        void SubscribeToBumperSensors()
        {
            _contactSensorPort.Subscribe(_contactNotificationPort);

            // Attach handler to update notification on bumpers
            Activate(Arbiter.Receive<contactsensor.Update>(true, _contactNotificationPort, BumperNotificationHandler));
        }
        #endregion

        #region CODECLIP 02-3
        /// <summary>
        /// Handler for Contact Sensor Notifications
        /// </summary>
        /// <param name="updateNotification"></param>
        void BumperNotificationHandler(contactsensor.Update updateNotification)
        {
            // Since we are writing a generic wander service we dont really know
            // which bumper is front, side, rear etc. We expect a navigation service to be tuned
            // to a robot platform or read configuration through its initial state.
            // here, we just assume the bumpers are named intuitively so we search by name

            contactsensor.ContactSensor s = updateNotification.Body;
            if (!s.Pressed)
                return;

            if (!string.IsNullOrEmpty(s.Name) &&
                s.Name.ToLowerInvariant().Contains("front"))
            {
                SpawnIterator<double>(-1,BackUpTurnAndMove);
                return;
            }

            if (!string.IsNullOrEmpty(s.Name) &&
                s.Name.ToLowerInvariant().Contains("rear"))
            {
                SpawnIterator<double>(1,BackUpTurnAndMove);
                return;
            }
        }
        #endregion

        #region CODECLIP 03-1
        Random _randomGen = new Random();

        /// <summary>
        /// Implements a simple sequential state machine that makes the robot wander
        /// </summary>
        /// <param name="polarity">Use 1 for going forward, -1 for going backwards</param>
        /// <returns></returns>
        IEnumerator<ITask> BackUpTurnAndMove(double polarity)
        {
            // First backup a little.
            yield return Arbiter.Receive(false,
                StartMove(0.4*polarity),
                delegate(bool result) { });

            // wait
            yield return Arbiter.Receive(false, TimeoutPort(1500), delegate(DateTime t) { });

            // now Turn
            yield return Arbiter.Receive(false,
                StartTurn(),
                delegate(bool result) { });

            // wait
            yield return Arbiter.Receive(false, TimeoutPort(1500), delegate(DateTime t) { });

            // now reverse direction and keep moving straight
            yield return Arbiter.Receive(false,
                StartMove(_randomGen.NextDouble()*polarity),
                delegate(bool result) { });

            // done
            yield break;
        }

        Port<bool> StartTurn()
        {
            Port<bool> result = new Port<bool>();
            // start a turn
            SpawnIterator<Port<bool>>(result, RandomTurn);
            return result;
        }

        Port<bool> StartMove(double powerLevel)
        {
            Port<bool> result = new Port<bool>();
            // start movement
            SpawnIterator<Port<bool>, double>(result, powerLevel, MoveStraight);
            return result;
        }

        IEnumerator<ITask> RandomTurn(Port<bool> done)
        {
            // we turn by issuing motor commands, using reverse polarity for left and right
            // We could just issue a Rotate command but since its a higher level function
            // we cant assume (yet) all our implementations of differential drives support it
            double randomPower = _randomGen.NextDouble();
            drive.SetDrivePowerRequest setPower = new drive.SetDrivePowerRequest(randomPower, -randomPower);

            bool success = false;
            yield return
                Arbiter.Choice(
                    _drivePort.SetDrivePower(setPower),
                    delegate(DefaultUpdateResponseType rsp) { success = true; },
                    delegate(W3C.Soap.Fault failure)
                    {
                        // report error but report done anyway. we will attempt
                        // to do the next step in wander behavior even if turn failed
                        LogError("Failed setting drive power");
                    });

            done.Post(success);
            yield break;
        }

        IEnumerator<ITask> MoveStraight(Port<bool> done, double powerLevel)
        {
            drive.SetDrivePowerRequest setPower = new drive.SetDrivePowerRequest(powerLevel, powerLevel);

            yield return
                Arbiter.Choice(
                _drivePort.SetDrivePower(setPower),
                delegate(DefaultUpdateResponseType success) { done.Post(true); },
                delegate(W3C.Soap.Fault failure)
                {
                    // report error but report done anyway. we will attempt
                    // to do the next step in wander behavior even if turn failed
                    LogError("Failed setting drive power");
                    done.Post(false);
                });
        }
        #endregion

        double ValidatePowerLevel(double powerLevel)
        {
            // we want to have a minimum power setting
            if (Math.Abs(powerLevel) < 0.25)
            {
                // more readable to use if statement
                if (powerLevel < 0)
                    powerLevel = -0.25;
                else
                    powerLevel = 0.25;
            }
            return powerLevel;
        }
    }
}
