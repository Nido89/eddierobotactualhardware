//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
// 
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: DriveInSquare.cs $ $Revision: 6 $
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Ccr.Core;
using Microsoft.Dss.Core;
using Microsoft.Dss.Core.Attributes;
using Microsoft.Dss.ServiceModel.Dssp;
using Microsoft.Dss.ServiceModel.DsspServiceBase;
using W3C.Soap;
using drive = Microsoft.Robotics.Services.Drive.Proxy;

namespace Microsoft.Robotics
{
    /// <summary>
    /// DriveInSquare - Drives a robot in a geometric pattern
    /// </summary>
    [Contract(Contract.Identifier)]
    [DisplayName("(User) DriveInSquare")]
    [Description("The DriveInSquare service drives a robot in a geometric pattern")]
    class DriveInSquareService : DsspServiceBase
    {
        #region Parameters

        // Constants for motor powers and timing
        const float drivePower = 0.3f;      // Power driving forward
        const float rotatePower = 0.5f;     // Power during rotation
        const int repeatCount = 2;          // Number of times to repeat behavior
        const int settlingTime = 1000;      // Time to wait after each move to
                                            // allow the robot to "settle"

        // Values for "exact" movements using DriveDistance and RotateDegrees
        const float driveDistance = 0.50f;  // Drive 50cm
        const float rotateAngle = 90.0f;    // Turn 90 degrees to the left

        // Timer values for the motions using timers
        const int driveTime = 2000;         // Time to drive forward (millisec)
        const int rotateTime = 1500;        // Time to rotate (millisec)

        private bool useTimers = false;     // Use timer for moves

        #endregion

        /// <summary>
        /// Service state
        /// </summary>
        [ServiceState]
        DriveInSquareState _state = new DriveInSquareState();

        /// <summary>
        /// Main service port
        /// </summary>
        [ServicePort("/DriveInSquare", AllowMultipleInstances = true)]
        DriveInSquareOperations _mainPort = new DriveInSquareOperations();

        /// <summary>
        /// DriveDifferentialTwoWheel partner
        /// </summary>
        [Partner("DriveDifferentialTwoWheel", Contract = drive.Contract.Identifier, CreationPolicy = PartnerCreationPolicy.UseExisting)]
        drive.DriveOperations _drivePort = new drive.DriveOperations();
        drive.DriveOperations _driveNotify = new drive.DriveOperations();

        // This port is sent a message every time that there is a
        // Canceled or Complete message from the Drive, so it can
        // be used to wait for completion.
        Port<drive.DriveStage> completionPort = new Port<drive.DriveStage>();

        /// <summary>
        /// Service constructor
        /// </summary>
        public DriveInSquareService(DsspServiceCreationPort creationPort)
            : base(creationPort)
        {
        }

        /// <summary>
        /// Service start
        /// </summary>
        protected override void Start()
        {
            // Set this to true to see how it works with just SetDrivePower and
            // timers to control the motions
            useTimers = false;

            // Subscribe to the drive for notification messages
            _drivePort.Subscribe(_driveNotify);

            // Add the necessary receivers
            // NOTE: These are INDEPENDENT receivers.
            // This is OK because they do not affect the state of the service.
            Activate(Arbiter.ReceiveWithIterator<drive.DriveDistance>(true, _driveNotify, DriveDistanceUpdateHandler));
            Activate(Arbiter.ReceiveWithIterator<drive.RotateDegrees>(true, _driveNotify, RotateDegreesUpdateHandler));

            base.Start();

            // Execute the geometric pattern
            SpawnIterator(Behavior);
        }

        #region Behavior

        // Iterator to execute the Behavior
        // It is important to use an Iterator so that it can relinquish control
        // when there is nothing to do, i.e. yield return
        IEnumerator<ITask> Behavior()
        {
            // Wait for the robot to initialize, otherwise it will
            // miss the initial command
            for (int i = 10; i > 0; i--)
            {
                LogInfo(LogGroups.Console, i.ToString());
                yield return Timeout(1000);
            }

            if (useTimers)
                LogInfo(LogGroups.Console, "Starting now using Timers ...");
            else
                LogInfo(LogGroups.Console, "Starting now using Controlled Moves ...");

            // Make sure that the drive is enabled first!
            _drivePort.EnableDrive(true);

            for (int times = 0; times < repeatCount; times++)
            {
                // Drive along the four sides of a square
                for (int side = 0; side < 4; side++)
                {
                    // Display progress info for the user
                    LogInfo(LogGroups.Console, "Side " + side);

                    if (useTimers)
                    {
                        // This appoach just uses SetDrivePower and sets timers
                        // to control the drive and rotate motions. This is not
                        // very accurate and the exact timer parameters will be
                        // different for different types of robots.

                        // Start driving and then wait for a while
                        _drivePort.SetDrivePower(drivePower, drivePower);

                        yield return Timeout(driveTime);

                        // Stop the motors and wait for robot to settle
                        _drivePort.SetDrivePower(0, 0);

                        yield return Timeout(settlingTime);

                        // Now turn left and wait for a different amount of time
                        _drivePort.SetDrivePower(-rotatePower, rotatePower);

                        yield return Timeout(rotateTime);

                        // Stop the motors and wait for robot to settle
                        _drivePort.SetDrivePower(0, 0);

                        yield return Timeout(settlingTime);
                    }
                    else
                    {
                        // This code uses the DriveDistance and RotateDegrees
                        // operations to control the robot. These are not precise,
                        // but they should be better than using timers and they
                        // should also work regardless of the type of robot.

                        bool success = true;
                        Fault fault = null;

                        // Drive straight ahead
                        yield return Arbiter.Choice(
                            _drivePort.DriveDistance(driveDistance, drivePower),
                            delegate(DefaultUpdateResponseType response) { success = true; },
                            delegate(Fault f) { success = false; fault = f; }
                        );

                        // If the DriveDistance was accepted, then wait for it to complete.
                        // It is important not to wait if the request failed.
                        // NOTE: This approach only works if you always wait for a
                        // completion message. If you send any other drive request
                        // while the current one is active, then the current motion
                        // will be canceled, i.e. cut short.
                        if (success)
                            yield return WaitForCompletion();
                        else
                            LogError("Error occurred on DriveDistance: " + fault);

                        // Wait for settling time
                        yield return Timeout(settlingTime);

                        // Now turn left
                        yield return Arbiter.Choice(
                            _drivePort.RotateDegrees(rotateAngle, rotatePower),
                            delegate(DefaultUpdateResponseType response) { success = true; },
                            delegate(Fault f) { success = false; fault = f; }
                        );

                        // If the RotateDegrees was accepted, then wait for it to complete.
                        // It is important not to wait if the request failed.
                        if (success)
                            yield return WaitForCompletion();
                        else
                            LogError("Error occurred on RotateDegrees: " + fault);

                        // Wait for settling time
                        yield return Timeout(settlingTime);
                    }
                }
            }

            // And finally make sure that the robot is stopped!
            _drivePort.SetDrivePower(0, 0);

            LogInfo(LogGroups.Console, "Finished");

            yield break;
        }


        /// <summary>
        /// WaitForCompletion - Helper function to wait on Completion Port
        /// </summary>
        /// <returns>Receiver suitable for waiting on</returns>
        public Receiver<drive.DriveStage> WaitForCompletion()
        {
            // Note that this method does nothing with the drive status
            return Arbiter.Receive(false, completionPort, EmptyHandler<drive.DriveStage>);
        }

        #endregion

        #region Handlers

        /// <summary>
        /// DriveDistanceUpdateHandler - Posts a message on Canceled or Complete
        /// </summary>
        /// <param name="distance"></param>
        /// <returns></returns>
        IEnumerator<ITask> DriveDistanceUpdateHandler(drive.DriveDistance distance)
        {
            // This code could be collapsed, but this structure makes it
            // clear and also easy to change
            if (distance.Body.DriveDistanceStage == drive.DriveStage.Canceled)
            {
                LogInfo(LogGroups.Console, "Drive Canceled");
                completionPort.Post(distance.Body.DriveDistanceStage);
            }
            if (distance.Body.DriveDistanceStage == drive.DriveStage.Completed)
            {
                LogInfo(LogGroups.Console, "Drive Complete");
                completionPort.Post(distance.Body.DriveDistanceStage);
            }
            yield break;
        }

        /// <summary>
        /// RotateDegreesUpdateHandler - Posts a message on Canceled or Complete
        /// </summary>
        /// <param name="rotate"></param>
        /// <returns></returns>
        IEnumerator<ITask> RotateDegreesUpdateHandler(drive.RotateDegrees rotate)
        {
            if (rotate.Body.RotateDegreesStage == drive.DriveStage.Canceled)
            {
                LogInfo(LogGroups.Console, "Rotate Canceled");
                completionPort.Post(rotate.Body.RotateDegreesStage);
            }
            if (rotate.Body.RotateDegreesStage == drive.DriveStage.Completed)
            {
                LogInfo(LogGroups.Console, "Rotate Complete");
                completionPort.Post(rotate.Body.RotateDegreesStage);
            }
            yield break;
        }

        #endregion

    }
}


