//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: RoboticsTutorial7.cs $ $Revision: 12 $
//---------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Speech.Recognition;
using System.Xml;

using Microsoft.Ccr.Core;
using Microsoft.Dss.Core;
using Microsoft.Dss.Core.Attributes;
using Microsoft.Dss.ServiceModel.Dssp;
using Microsoft.Dss.ServiceModel.DsspServiceBase;

using sr = Microsoft.Robotics.Technologies.Speech.SpeechRecognizer.Proxy;
using drive = Microsoft.Robotics.Services.Drive.Proxy;
using xinput = Microsoft.Robotics.Services.Sample.XInputGamepad.Proxy;
using blob = Microsoft.Robotics.Services.Sample.BlobTracker.Proxy;

namespace Microsoft.Robotics.Services.RoboticsTutorial7
{
    [DisplayName("(User) Robotics Tutorial 7 (C#): Speech and Vision in Robots")]
    [Description("Demonstrates how to use speech to drive a robot and how to use the Blob Tracker in order to make a follow-me service, using a mounted camera.")]
    [DssServiceDescription("http://msdn.microsoft.com/library/bb608247.aspx")]
    [Contract(Contract.Identifier)]
    public class RoboticsTutorial7Service : DsspServiceBase
    {
        private bool _followMe = false;

        /// <summary>
        /// Service State
        /// </summary>
        [ServiceState]
        private RoboticsTutorial7State _state = new RoboticsTutorial7State();

        /// <summary>
        /// Main Operations Port
        /// </summary>
        [ServicePort("/roboticstutorial7", AllowMultipleInstances=false)]
        private RoboticsTutorial7Operations _mainPort = new RoboticsTutorial7Operations();

        #region CODECLIP 01-1
        [Partner("SpeechRecognizer", Contract = sr.Contract.Identifier, CreationPolicy = PartnerCreationPolicy.UseExisting)]
        sr.SpeechRecognizerOperations _srPort = new sr.SpeechRecognizerOperations();
        sr.SpeechRecognizerOperations _srNotify = new sr.SpeechRecognizerOperations();

        [Partner("Drive", Contract = drive.Contract.Identifier, CreationPolicy = PartnerCreationPolicy.UseExisting)]
        drive.DriveOperations _drivePort = new drive.DriveOperations();

        [Partner("XInputGamePad", Contract = xinput.Contract.Identifier, CreationPolicy = PartnerCreationPolicy.CreateAlways)]
        xinput.XInputGamepadOperations _xinputPort = new xinput.XInputGamepadOperations();
        xinput.XInputGamepadOperations _xinputNotify = new xinput.XInputGamepadOperations();

        [Partner("BlobTracker", Contract = blob.Contract.Identifier, CreationPolicy = PartnerCreationPolicy.UseExisting)]
        blob.BlobTrackerOperations _blobPort = new blob.BlobTrackerOperations();
        blob.BlobTrackerOperations _blobNotify = new blob.BlobTrackerOperations();
        #endregion

        /// <summary>
        /// Default Service Constructor
        /// </summary>
        public RoboticsTutorial7Service(DsspServiceCreationPort creationPort) :
                base(creationPort)
        {
        }

        /// <summary>
        /// Service Start
        /// </summary>
        protected override void Start()
        {
            base.Start();

            #region CODECLIP 02-1
            _srPort.Subscribe(_srNotify);
            _xinputPort.Subscribe(_xinputNotify);
            _blobPort.Subscribe(_blobNotify);

            Activate<ITask>(
                Arbiter.Receive<sr.SpeechRecognized>(true, _srNotify, OnSRRecognition),
                Arbiter.Receive<xinput.ButtonsChanged>(true, _xinputNotify, OnButtonsChanged),
                Arbiter.Receive<blob.ImageProcessed>(true, _blobNotify, OnImageProcessed));
            #endregion
        }


        /// <summary>
        /// Handles the recognition event notification
        /// </summary>
        /// <param name="recognition"></param>
        #region CODECLIP 02-2
        void OnSRRecognition(sr.SpeechRecognized recognition)
        {
            if (recognition.Body.Semantics != null)
            {
                switch (recognition.Body.Semantics.ValueString)
                {
                    case "Forward":
                        _drivePort.SetDrivePower(0.5, 0.5);
                        break;
                    case "Backward":
                        _drivePort.SetDrivePower(-0.5, -0.5);
                        break;
                    case "Left":
                        _drivePort.SetDrivePower(-0.3, 0.3);
                        break;
                    case "Right":
                        _drivePort.SetDrivePower(0.3, -0.3);
                        break;
                    case "Stop":
                        _followMe = false;
                        _drivePort.SetDrivePower(0.0, 0.0);
                        break;
                    case "FollowMe":
                        _followMe = !_followMe;
                        if (!_followMe)
                        {
                            _drivePort.SetDrivePower(0.0, 0.0);
                        }
                        break;
                }
            }
        }
        #endregion

        /// <summary>
        /// Handles notification of buttons changed of the xinput controller.
        /// </summary>
        /// <param name="buttons"></param>
        void OnButtonsChanged(xinput.ButtonsChanged buttons)
        {
            if (buttons.Body.Y)
            {
                _drivePort.SetDrivePower(0.5, 0.5);
            }
            else if (buttons.Body.A)
            {
                _drivePort.SetDrivePower(-0.5, -0.5);
            }
            else if (buttons.Body.X)
            {
                _drivePort.SetDrivePower(-0.3, 0.3);
            }
            else if (buttons.Body.B)
            {
                _drivePort.SetDrivePower(0.3, -0.3);
            }
            else if (buttons.Body.RightShoulder)
            {
                _drivePort.SetDrivePower(0.0, 0.0);
            }
            else if (buttons.Body.LeftShoulder)
            {
                _followMe = !_followMe;
                if (!_followMe)
                {
                    _drivePort.SetDrivePower(0.0, 0.0);
                }
            }
        }

        /// <summary>
        /// Handles event when image is processed by the blob tracker.
        /// </summary>
        /// <param name="imageProcessed"></param>
        #region  CODECLIP 02-3
        void OnImageProcessed(blob.ImageProcessed imageProcessed)
        {
            if (_followMe)
            {
                if (imageProcessed.Body.Results.Count == 1)
                {
                    if (imageProcessed.Body.Results[0].Area > 100) //object detected
                    {
                        _drivePort.SetDrivePower(0.5, 0.5);
                    }
                    else //search object
                    {
                        _drivePort.SetDrivePower(-0.3, 0.3);
                    }
                }
            }
        }
        #endregion
    }
}
