//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: RoboticsTutorial4.cs $ $Revision: 22 $
//-----------------------------------------------------------------------
using Microsoft.Ccr.Core;
using Microsoft.Ccr.Adapters.WinForms;
using Microsoft.Dss.Core;
using Microsoft.Dss.Core.Attributes;
using Microsoft.Dss.ServiceModel.Dssp;
using Microsoft.Dss.ServiceModel.DsspServiceBase;
using System;
using System.Collections.Generic;
using System.Security.Permissions;
using xml = System.Xml;
using drive = Microsoft.Robotics.Services.Drive.Proxy;
using W3C.Soap;
using Microsoft.Robotics.Services.RoboticsTutorial4.Properties;
using Microsoft.Robotics.Services.Drive.Proxy;
using System.ComponentModel;


namespace Microsoft.Robotics.Services.RoboticsTutorial4
{
    [DisplayName("(User) Robotics Tutorial 4 (C#): Drive-By-Wire")]
    [Description("This tutorial demonstrates how to create a service that partners with abstract, base definitions of hardware services.")]
    [DssServiceDescription("http://msdn.microsoft.com/library/bb483053.aspx")]
    [Contract(Contract.Identifier)]
    public class RoboticsTutorial4 : DsspServiceBase
    {
        [ServiceState]
        private RoboticsTutorial4State _state = new RoboticsTutorial4State();

        [ServicePort("/RoboticsTutorial4", AllowMultipleInstances=false)]
        private RoboticsTutorial4Operations _mainPort = new RoboticsTutorial4Operations();

        [Partner("Drive", Contract = drive.Contract.Identifier, CreationPolicy = PartnerCreationPolicy.UseExisting)]
        private drive.DriveOperations _drivePort = new drive.DriveOperations();
        private drive.DriveOperations _driveNotify = new drive.DriveOperations();

        public RoboticsTutorial4(DsspServiceCreationPort creationPort) :
                base(creationPort)
        {
        }

        #region CODECLIP 02-1
        protected override void Start()
        {
            base.Start();

            WinFormsServicePort.Post(new RunForm(StartForm));

            #region CODECLIP 01-5
            _drivePort.Subscribe(_driveNotify);
            Activate(Arbiter.Receive<drive.Update>(true, _driveNotify, NotifyDriveUpdate));
            #endregion
        }
        #endregion

        #region CODECLIP 02-2
        private System.Windows.Forms.Form StartForm()
        {
            RoboticsTutorial4Form form = new RoboticsTutorial4Form(_mainPort);

            Invoke(delegate()
                {
                    PartnerType partner = FindPartner("Drive");
                    Uri uri = new Uri(partner.Service);
                    form.Text = string.Format(
                        Resources.Culture,
                        Resources.Title,
                        uri.AbsolutePath
                    );
                }
            );

            return form;
        }
        #endregion

        #region CODECLIP 02-3
        private void Invoke(System.Windows.Forms.MethodInvoker mi)
        {
            WinFormsServicePort.Post(new FormInvoke(mi));
        }
        #endregion


        /// <summary>
        /// Replace Handler
        /// </summary>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public virtual IEnumerator<ITask> ReplaceHandler(Replace replace)
        {
            _state = replace.Body;
            replace.ResponsePort.Post(DefaultReplaceResponseType.Instance);
            yield break;
        }

        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public virtual IEnumerator<ITask> StopHandler(Stop stop)
        {
            drive.SetDrivePowerRequest request = new drive.SetDrivePowerRequest();
            request.LeftWheelPower = 0;
            request.RightWheelPower = 0;

            yield return Arbiter.Choice(
                _drivePort.SetDrivePower(request),
                delegate(DefaultUpdateResponseType response) { },
                delegate(Fault fault)
                {
                    LogError(null, "Unable to stop", fault);
                }
            );
        }

        #region CODECLIP 01-3
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public virtual IEnumerator<ITask> ForwardHandler(Forward forward)
        {
            if (!_state.MotorEnabled)
            {
                yield return EnableMotor();
            }

            // This sample sets the power to 75%.
            // Depending on your robotic hardware,
            // you may wish to change these values.
            drive.SetDrivePowerRequest request = new drive.SetDrivePowerRequest();
            request.LeftWheelPower = 0.75;
            request.RightWheelPower = 0.75;

            yield return Arbiter.Choice(
                _drivePort.SetDrivePower(request),
                delegate(DefaultUpdateResponseType response) { },
                delegate(Fault fault)
                {
                    LogError(null, "Unable to drive forwards", fault);
                }
            );
        }
        #endregion

        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public virtual IEnumerator<ITask> BackwardHandler(Backward backward)
        {
            if (!_state.MotorEnabled)
            {
                yield return EnableMotor();
            }

            drive.SetDrivePowerRequest request = new drive.SetDrivePowerRequest();
            request.LeftWheelPower = -0.6;
            request.RightWheelPower = -0.6;

            yield return Arbiter.Choice(
                _drivePort.SetDrivePower(request),
                delegate(DefaultUpdateResponseType response) { },
                delegate(Fault fault)
                {
                    LogError(null, "Unable to drive backwards", fault);
                }
            );
        }

        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public virtual IEnumerator<ITask> TurnLeftHandler(TurnLeft turnLeft)
        {
            if (!_state.MotorEnabled)
            {
                yield return EnableMotor();
            }

            drive.SetDrivePowerRequest request = new drive.SetDrivePowerRequest();
            request.LeftWheelPower = -0.5;
            request.RightWheelPower = 0.5;

            yield return Arbiter.Choice(
                _drivePort.SetDrivePower(request),
                delegate(DefaultUpdateResponseType response) { },
                delegate(Fault fault)
                {
                    LogError(null, "Unable to turn left", fault);
                }
            );
        }

        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public virtual IEnumerator<ITask> TurnRightHandler(TurnRight forward)
        {
            if (!_state.MotorEnabled)
            {
                yield return EnableMotor();
            }

            drive.SetDrivePowerRequest request = new drive.SetDrivePowerRequest();
            request.LeftWheelPower = 0.5;
            request.RightWheelPower = -0.5;

            yield return Arbiter.Choice(
                _drivePort.SetDrivePower(request),
                delegate(DefaultUpdateResponseType response) { },
                delegate(Fault fault)
                {
                    LogError(null, "Unable to turn right", fault);
                }
            );
        }

        #region CODECLIP 01-4
        private Choice EnableMotor()
        {
            drive.EnableDriveRequest request = new drive.EnableDriveRequest();
            request.Enable = true;

            return Arbiter.Choice(
                _drivePort.EnableDrive(request),
                delegate(DefaultUpdateResponseType response) { },
                delegate(Fault fault)
                {
                    LogError(null, "Unable to enable motor", fault);
                }
            );
        }
        #endregion

        #region CODECLIP 01-6
        private void NotifyDriveUpdate(drive.Update update)
        {
            RoboticsTutorial4State state = new RoboticsTutorial4State();
            state.MotorEnabled = update.Body.IsEnabled;

            _mainPort.Post(new Replace(state));
        }
        #endregion
    }
}
