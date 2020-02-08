//-----------------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples
//
//  <copyright file="RobotDashboard.cs" company="Microsoft Corporation">
//      Copyright (C) Microsoft Corporation.  All rights reserved.
//  </copyright>
//
//  $File: RobotDashboard.cs $ $Revision: 1 $
//-----------------------------------------------------------------------------

namespace Microsoft.Robotics.Services.RobotDashboard
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;

    using Microsoft.Ccr.Adapters.WinForms;
    using Microsoft.Ccr.Core;
    using Microsoft.Dss.Core;
    using Microsoft.Dss.Core.Attributes;
    using Microsoft.Dss.ServiceModel.Dssp;
    using Microsoft.Dss.ServiceModel.DsspServiceBase;

    using W3C.Soap;

    using battery = Microsoft.Robotics.Services.Battery.Proxy;
    using depthcamsensor = Microsoft.Robotics.Services.DepthCamSensor.Proxy;
    using drive = Microsoft.Robotics.Services.Drive.Proxy;
    using game = Microsoft.Robotics.Services.GameController.Proxy;
    using infraredsensorarray = Microsoft.Robotics.Services.InfraredSensorArray.Proxy;
    using pantilt = Microsoft.Robotics.Services.PanTilt.Proxy;
    using sonarsensorarray = Microsoft.Robotics.Services.SonarSensorArray.Proxy;
    using submgr = Microsoft.Dss.Services.SubscriptionManager;
    using webcamsensor = Microsoft.Robotics.Services.WebCamSensor.Proxy;

    /// <summary>
    /// Main Robot Dashboard Service Class
    /// </summary>
    [Contract(Contract.Identifier)]
    [DisplayName("(User) Robot Dashboard")]
    [Description("Robot Dashboard service for Reference Platform")]
    public class RobotDashboard : DsspServiceBase, IDisposable
    {
        /// <summary>
        /// Scaling for the joystick values to convert to motor power
        /// </summary>
        /// <remarks>
        /// This scale factor is applied to power settings which must be in the
        /// range -1 to +1. However, the game controller and trackball have a
        /// range of -1000 to +1000.
        /// </remarks>
        private const double MotorPowerSaleFactor = 0.001;

        /// <summary>
        /// Used for converting distance, received in meters to centimeters
        /// </summary>
        private const double ConvertMetersToCm = 100;

        /// <summary>
        /// Service state
        /// </summary>
        [ServiceState]
        [InitialStatePartner(Optional = true, ServiceUri = ServicePaths.Store + "/RobotDashboard.config.xml")]
        private RobotDashboardState state = new RobotDashboardState();

        /// <summary>
        /// Main service port
        /// </summary>
        [ServicePort("/RobotDashboard", AllowMultipleInstances = true)]
        private RobotDashboardOperations mainPort = new RobotDashboardOperations();

        /// <summary>
        /// Subscription Manager port
        /// </summary>
        [SubscriptionManagerPartner]
        private submgr.SubscriptionManagerPort submgrPort = new submgr.SubscriptionManagerPort();

        /// <summary>
        /// Differential Drive partner
        /// </summary>
        [Partner("Drive", Contract = drive.Contract.Identifier, CreationPolicy = PartnerCreationPolicy.UseExistingOrCreate)]
        private drive.DriveOperations drivePort = new drive.DriveOperations();

        /// <summary>
        /// True if this object has been disposed.
        /// </summary>
        private bool disposed;

        /// <summary>
        /// The Pan Tilt operations
        /// </summary>
        [Partner("Pan/Tilt", Contract = pantilt.Contract.Identifier, CreationPolicy = PartnerCreationPolicy.UseExistingOrCreate, Optional = true)]
        private pantilt.PanTiltOperationsPort panTiltOps = new pantilt.PanTiltOperationsPort();

        /// <summary>
        /// The Pan Tilt notifications
        /// </summary>
        private pantilt.PanTiltOperationsPort panTiltNotify = new pantilt.PanTiltOperationsPort();

        /// <summary>
        /// DepthCamSensor partner
        /// </summary>
        [Partner("DepthCam", Contract = depthcamsensor.Contract.Identifier, CreationPolicy = PartnerCreationPolicy.UseExistingOrCreate, Optional = true)]
        private depthcamsensor.DepthCamSensorOperationsPort depthCamSensorPort = new depthcamsensor.DepthCamSensorOperationsPort();

        /// <summary>
        /// DepthCam Notifications Port
        /// </summary>
        private depthcamsensor.DepthCamSensorOperationsPort depthCamSensorNotify = new depthcamsensor.DepthCamSensorOperationsPort();

        /// <summary>
        /// WebCamSensor partner
        /// </summary>
        [Partner("WebCam", Contract = webcamsensor.Contract.Identifier, CreationPolicy = PartnerCreationPolicy.UseExistingOrCreate, Optional = true)]
        private webcamsensor.WebCamSensorOperations webCamPort = new webcamsensor.WebCamSensorOperations();

        /// <summary>
        /// WebCamSensor Notifications Port
        /// </summary>
        private webcamsensor.WebCamSensorOperations webCamNotify = new webcamsensor.WebCamSensorOperations();

        /// <summary>
        /// IR Sensors partner
        /// </summary>
        [Partner("IRSensorArray", Contract = infraredsensorarray.Contract.Identifier, CreationPolicy = PartnerCreationPolicy.UseExistingOrCreate, Optional = true)]
        private infraredsensorarray.InfraredSensorOperations irsensorArrayPort = new infraredsensorarray.InfraredSensorOperations();

        /// <summary>
        /// Sonar Sensors partner
        /// </summary>
        [Partner("SonarSensorArray", Contract = sonarsensorarray.Contract.Identifier, CreationPolicy = PartnerCreationPolicy.UseExistingOrCreate, Optional = true)]
        private sonarsensorarray.SonarSensorOperations sonarSensorArrayPort = new sonarsensorarray.SonarSensorOperations();

        /// <summary>
        /// Game Controller partner
        /// </summary>
        /// <remarks>Always create one of these, even if there is no Game Controller attached</remarks>
        [Partner("GameController", Contract = game.Contract.Identifier, CreationPolicy = PartnerCreationPolicy.UseExistingOrCreate)]
        private game.GameControllerOperations gameControllerPort = new game.GameControllerOperations();

        /// <summary>
        /// GameController Notifications Port
        /// </summary>
        private game.GameControllerOperations gameControllerNotify = new game.GameControllerOperations();

        /// <summary>
        /// The battery partner
        /// </summary>
        [Partner("Battery", Contract = battery.Contract.Identifier, CreationPolicy = PartnerCreationPolicy.UseExisting, Optional = true)]
        private battery.BatteryOperations batteryPort = new battery.BatteryOperations();

        /// <summary>
        /// A handle to the main WinForm UI
        /// </summary>
        private DashboardForm dashboardForm;

        /// <summary>
        /// Port for the UI to send messages back to here (main service)
        /// </summary>
        private DashboardFormEvents eventsPort = new DashboardFormEvents();

        /// <summary>
        /// The polling interval for reading the sensors (milliseconds)
        /// </summary>
        private int sensorPollingInterval = 100;

        /// <summary>
        /// The Webcam Form instance
        /// </summary>
        private WebCamForm cameraForm;

        /// <summary>
        /// The Depthcam Form instance
        /// </summary>
        private DepthCamForm depthCameraForm;

        /// <summary>
        /// Initializes a new instance of the <see cref="RobotDashboard"/> class.
        /// </summary>
        /// <param name="creationPort">The creation port.</param>
        public RobotDashboard(DsspServiceCreationPort creationPort)
            : base(creationPort)
        {
        }

        /// <summary>
        /// Dispose both managed and native resources
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Handles Subscribe messages
        /// </summary>
        /// <param name="subscribe">The subscribe request</param>
        [ServiceHandler]
        public void SubscribeHandler(Subscribe subscribe)
        {
            SubscribeHelper(this.submgrPort, subscribe.Body, subscribe.ResponsePort);
        }

        /// <summary>
        /// Service start
        /// </summary>
        protected override void Start()
        {
            base.Start();

            this.InitializeState();

            // Handlers that need write or exclusive access to state go under
            // the exclusive group. Handlers that need read or shared access, and can be
            // concurrent to other readers, go to the concurrent group.
            // Other internal ports can be included in interleave so you can coordinate
            // intermediate computation with top level handlers.
            MainPortInterleave.CombineWith(
            Arbiter.Interleave(
                new TeardownReceiverGroup(),
                new ExclusiveReceiverGroup(
                    Arbiter.ReceiveWithIterator<OnLoad>(true, this.eventsPort, this.OnLoadHandler),
                    Arbiter.Receive<OnClosed>(true, this.eventsPort, this.OnClosedHandler),
                    Arbiter.ReceiveWithIterator<OnChangeJoystick>(true, this.eventsPort, this.OnChangeJoystickHandler),
                    Arbiter.ReceiveWithIterator<OnChangeTilt>(true, this.eventsPort, this.OnChangeTiltHandler),
                    Arbiter.Receive<pantilt.Rotate>(true, this.panTiltNotify, this.OnRotateSingleAxis),
                    Arbiter.ReceiveWithIterator<OnOptionSettings>(true, this.eventsPort, this.OnOptionSettingsHandler),
                    Arbiter.Receive<webcamsensor.Replace>(true, this.webCamNotify, this.CameraUpdateFrameHandler),
                    Arbiter.Receive<depthcamsensor.Replace>(true, this.depthCamSensorNotify, this.DepthCameraUpdateFrameHandler)),
                new ConcurrentReceiverGroup(
                    Arbiter.Receive<OnResetEncoders>(true, this.eventsPort, this.OnResetEncodersHandler),
                    Arbiter.ReceiveWithIterator<game.Replace>(true, this.gameControllerNotify, this.JoystickReplaceHandler),
                    Arbiter.ReceiveWithIterator<game.UpdateAxes>(true, this.gameControllerNotify, this.JoystickUpdateAxesHandler),
                    Arbiter.ReceiveWithIterator<game.UpdateButtons>(true, this.gameControllerNotify, this.JoystickUpdateButtonsHandler),
                    Arbiter.ReceiveWithIterator<OnMove>(true, this.eventsPort, this.OnMoveHandler),
                    Arbiter.ReceiveWithIterator<OnMotionCommand>(true, this.eventsPort, this.OnMotionCommandHandler))));

            SpawnIterator(this.Setup);
        }

        /// <summary>
        /// Dispose this object
        /// </summary>
        /// <param name="disposing">Indicates whether both native and managed resources should be cleaned up</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    this.depthCameraForm.Dispose();
                    this.cameraForm.Dispose();
                }

                this.disposed = true;
            }
        }

        /// <summary>
        /// Initialize the State with appropriate values
        /// </summary>
        private void InitializeState()
        {
            bool newState = false;

            if (this.state == null)
            {
                this.state = new RobotDashboardState { TiltAngle = 0 };
                newState = true;
            }

            if (this.state.Options == null)
            {
                this.state.Options = new GUIOptions
                    {
                        DeadZoneX = 150, DeadZoneY = 150, WindowStartX = 10, WindowStartY = 10 
                    };

                // Window geometry parameters
                this.state.Options.DepthcamWindowStartX = this.state.Options.WindowStartX + 450;
                this.state.Options.DepthcamWindowStartY = this.state.Options.WindowStartY;
                this.state.Options.DepthcamWindowWidth = 360;
                this.state.Options.DepthcamWindowHeight = 300;
                this.state.Options.WebcamWindowStartX = this.state.Options.WindowStartX + 450;
                this.state.Options.WebcamWindowStartY = this.state.Options.WindowStartY + 310;
                this.state.Options.WebcamWindowWidth = 360;
                this.state.Options.WebcamWindowHeight = 300;

                this.state.Options.TranslateScaleFactor = 0.7;
                this.state.Options.RotateScaleFactor = 0.4;

                // Camera update interval in milliseconds
                // Note that this is only required for the
                // simulated webcam because it does not provide
                // updates when you subscribe
                this.state.Options.CameraInterval = 250;
            }

            if (this.state.Options.CameraInterval < 100)
            {
                this.state.Options.CameraInterval = 100;
            }

            if (newState)
            {
                SaveState(this.state);
            }
        }

        /// <summary>
        /// Perform the initial setup
        /// </summary>
        /// <returns>An iterator</returns>
        private IEnumerator<ITask> Setup()
        {
            SpawnIterator(this.ConnectDriveHandler);
            SpawnIterator(this.ConnectPanTiltHandler);
            SpawnIterator(this.ConnectWebCamHandler);
            SpawnIterator(this.ConnectDepthCamHandler);

            WinFormsServicePort.Post(new RunForm(this.CreateForm));

            SpawnIterator(this.ProcessSensors);

            yield break;
        }

        /// <summary>
        /// Called when reset encoders is clicked.
        /// </summary>
        /// <param name="resetEncodersRequest">The reset encoders request.</param>
        private void OnResetEncodersHandler(OnResetEncoders resetEncodersRequest)
        {
            if (this.drivePort != null)
            {
                this.drivePort.ResetEncoders();
            }
        }

        /// <summary>
        /// Process the Sensor data
        /// </summary>
        /// <returns>An iterator</returns>
        private IEnumerator<ITask> ProcessSensors()
        {
            if (this.batteryPort == null)
            {
                WinFormsServicePort.FormInvoke(() => this.dashboardForm.UpdateBatteryLevel(null));
            }

            DateTime? lastBatteryPoll = null;
            while (true)
            {
                DateTime now = DateTime.Now;
                if (this.irsensorArrayPort != null)
                {
                    // Create a request that includes a Diagnostic Header so that it can be picked
                    // up by selective logging. If no logging is required, the code would just be:
                    // yield return this.irsensorArrayPort.Get().Choice(
                    infraredsensorarray.Get getRequest = new infraredsensorarray.Get();
                    getRequest.AddHeader(new DiagnosticsHeader());
                    this.irsensorArrayPort.Post(getRequest);
                    yield return getRequest.ResponsePort.Choice(
                        s =>
                        {
                            var irValues = new double[Constants.IRSensorCount];
                            for (var i = 0; i < Constants.IRSensorCount; i++)
                            {
                                irValues[Constants.IRSensorCount - i - 1] = s.Sensors[i].DistanceMeasurement * ConvertMetersToCm;
                            }

                            WinFormsServicePort.FormInvoke(() => this.dashboardForm.UpdateIRSensors(irValues));
                        },
                        LogError);
                }

                if (this.sonarSensorArrayPort != null)
                {
                    // Create a request that includes a Diagnostic Header so that it can be picked
                    // up by selective logging. If no logging is required, the code would just be:
                    // yield return this.sonarSensorArrayPort.Get().Choice(
                    sonarsensorarray.Get getRequest = new sonarsensorarray.Get();
                    getRequest.AddHeader(new DiagnosticsHeader());
                    this.sonarSensorArrayPort.Post(getRequest);
                    yield return getRequest.ResponsePort.Choice(
                        s =>
                        {
                            var sonarValues = new double[Constants.SonarSensorCount];
                            for (int i = 0; i < Constants.SonarSensorCount; i++)
                            {
                                sonarValues[Constants.SonarSensorCount - i - 1] = s.Sensors[i].DistanceMeasurement * ConvertMetersToCm;
                            }

                            WinFormsServicePort.FormInvoke(() => this.dashboardForm.UpdateSonarSensors(sonarValues));
                        },
                        LogError);
                }

                if (this.drivePort != null)
                {
                    yield return this.drivePort.Get().Choice(
                        s => WinFormsServicePort.FormInvoke(() => this.dashboardForm.UpdateWheelState(s)),
                        this.LogError);
                }

                // Only poll the battery every minute
                if (this.batteryPort != null &&
                    (!lastBatteryPoll.HasValue || 
                     DateTime.Now.Subtract(lastBatteryPoll.Value) > TimeSpan.FromMinutes(1)))
                {
                    yield return
                        this.batteryPort.Get().Choice(
                            s => WinFormsServicePort.FormInvoke(() => this.dashboardForm.UpdateBatteryLevel(s)),
                            this.LogError);

                    lastBatteryPoll = DateTime.Now;
                }

                // Just sleep for the remainder of our sensor polling timout
                int delay = Math.Max(0, (int)(this.sensorPollingInterval - (DateTime.Now - now).TotalMilliseconds));
                yield return TimeoutPort(delay).Receive();
            }
        }

        /// <summary>
        /// Initialize the pantilt ops
        /// </summary>
        /// <returns>An iterator</returns>
        private IEnumerator<ITask> ConnectPanTiltHandler()
        {
            Fault fault = null;

            if (this.panTiltOps != null)
            {
                // Try to subscribe
                yield return this.panTiltOps
                    .Subscribe(this.panTiltNotify, typeof(pantilt.Rotate))
                    .Choice(EmptyHandler, f => fault = f);

                if (fault != null)
                {
                    // There is no Kinect partner
                    LogError(null, "Failed to subscribe to Pan/Tilt", fault);
                    yield break;
                }

                // Set the initial tilt angle now
                var request = new OnChangeTilt(this.dashboardForm, this.state.TiltAngle);
                SpawnIterator(request, this.OnChangeTiltHandler);
            }

            yield break;
        }

        /// <summary>
        /// Called on a rotate single axis.
        /// </summary>
        /// <param name="rotate">The rotation.</param>
        private void OnRotateSingleAxis(pantilt.Rotate rotate)
        {
            var degrees = rotate.Body.RotateTiltRequest.TargetRotationAngleInRadians * 180 / Math.PI;
            this.state.TiltAngle = Math.Round(degrees);

            var angle = (int)this.state.TiltAngle;
            var update = new FormInvoke(() =>
                {
                    this.dashboardForm.TiltTextbox.Text = angle.ToString();
                });
            WinFormsServicePort.Post(update);
        }

        /// <summary>
        /// Handle Tilt Angle requests
        /// </summary>
        /// <param name="tilt">The Tilt request</param>
        /// <returns>An iterator</returns>
        private IEnumerator<ITask> OnChangeTiltHandler(OnChangeTilt tilt)
        {
            Fault fault = null;
            var req = new pantilt.RotateMessage
                {
                    RotatePanRequest = null,
                    RotateTiltRequest = new SingleAxisJoint.Proxy.RotateSingleAxisRequest
                    {
                        TargetRotationAngleInRadians = (float)(tilt.Tilt * Math.PI / 180.0),
                        IsRelative = false
                    }
                };

            yield return this.panTiltOps.Rotate(req).Choice(EmptyHandler, f => fault = f);

            if (fault != null)
            {
                LogError("Update Tilt failed: ", fault.ToException());
            }
        }
        
        #region WinForms interaction

        /// <summary>
        /// Create the main Windows Form
        /// </summary>
        /// <returns>A Dashboard Form</returns>
        private System.Windows.Forms.Form CreateForm()
        {
            return new DashboardForm(this.eventsPort, this.state);
        }

        /// <summary>
        /// Handle the Form Load event for the Dashboard Form
        /// </summary>
        /// <param name="onLoad">The load message</param>
        /// <returns>An iterator</returns>
        private IEnumerator<ITask> OnLoadHandler(OnLoad onLoad)
        {
            this.dashboardForm = onLoad.DashboardForm;

            LogInfo("Loaded Form");

            yield return this.EnumerateJoysticks();

            yield return this.SubscribeToJoystick();
        }

        /// <summary>
        /// Handle the Form Closed event for the Dashboard Form
        /// </summary>
        /// <param name="onClosed">The closed message</param>
        private void OnClosedHandler(OnClosed onClosed)
        {
            if (onClosed.DashboardForm == this.dashboardForm)
            {
                LogInfo("Form Closed");

                this.mainPort.Post(new DsspDefaultDrop(DropRequestType.Instance));
                ControlPanelPort.Post(new DsspDefaultDrop(DropRequestType.Instance));

                if (this.cameraForm != null)
                {
                    var closeWebcam = new FormInvoke(
                        delegate
                            {
                                this.cameraForm.Close();
                                this.cameraForm = null;
                            });

                    WinFormsServicePort.Post(closeWebcam);
                }

                if (this.depthCameraForm != null)
                {
                    var closeDepthcam = new FormInvoke(
                        delegate
                            {
                                this.depthCameraForm.Close();
                                this.depthCameraForm = null;
                            });

                    WinFormsServicePort.Post(closeDepthcam);
                }
            }
        }

        /// <summary>
        /// Handle saving the Option Settings
        /// </summary>
        /// <param name="opt">An Option Settings object populated by the Options Form</param>
        /// <returns>An Iterator</returns>
        private IEnumerator<ITask> OnOptionSettingsHandler(OnOptionSettings opt)
        {
            this.state.Options = opt.Options;

            if (this.cameraForm != null)
            {
                this.state.Options.WebcamWindowStartX = this.cameraForm.Location.X;
                this.state.Options.WebcamWindowStartY = this.cameraForm.Location.Y;
                this.state.Options.WebcamWindowWidth = this.cameraForm.Width;
                this.state.Options.WebcamWindowHeight = this.cameraForm.Height;
            }

            if (this.depthCameraForm != null)
            {
                this.state.Options.DepthcamWindowStartX = this.depthCameraForm.Location.X;
                this.state.Options.DepthcamWindowStartY = this.depthCameraForm.Location.Y;
                this.state.Options.DepthcamWindowWidth = this.depthCameraForm.Width;
                this.state.Options.DepthcamWindowHeight = this.depthCameraForm.Height;
            }

            SaveState(this.state);

            yield break;
        }

        #endregion

        #region Joystick Handlers

        /// <summary>
        /// Connect the Handlers for the Joystick
        /// </summary>
        /// <returns>An Iterator</returns>
        private IEnumerator<ITask> ConnectJoystickHandlers()
        {
            yield return this.EnumerateJoysticks();

            yield return this.SubscribeToJoystick();
        }

        /// <summary>
        /// Enumerate the available Joysticks
        /// </summary>
        /// <returns>A Choice</returns>
        private Choice EnumerateJoysticks()
        {
            return Arbiter.Choice(
                this.gameControllerPort.GetControllers(new game.GetControllersRequest()),
                response =>
                this.WinFormsServicePort.FormInvoke(() => this.dashboardForm.ReplaceJoystickList(response.Controllers)),
                LogError);
        }

        /// <summary>
        /// Subscribe to the Joystick
        /// </summary>
        /// <returns>A Choice</returns>
        private Choice SubscribeToJoystick()
        {
            return Arbiter.Choice(this.gameControllerPort.Subscribe(this.gameControllerNotify), EmptyHandler, LogError);
        }

        /// <summary>
        /// Handle Joystick Replace messages
        /// </summary>
        /// <param name="replace">The replace message</param>
        /// <returns>An Iterator</returns>
        private IEnumerator<ITask> JoystickReplaceHandler(game.Replace replace)
        {
            var p = (Port<game.Replace>)this.gameControllerNotify[typeof(game.Replace)];
            if (p.ItemCount > 10)
            {
                Console.WriteLine("Joystick backlog: " + p.ItemCount);
            }

            if (this.dashboardForm != null)
            {
                WinFormsServicePort.FormInvoke(
                    delegate
                    {
                        this.dashboardForm.UpdateJoystickButtons(replace.Body.Buttons);
                        this.dashboardForm.UpdateJoystickAxes(replace.Body.Axes);
                    });
            }

            yield break;
        }

        /// <summary>
        /// Handle changes to the Joystick position
        /// </summary>
        /// <param name="update">The updated Axis information</param>
        /// <returns>An Iterator</returns>
        private IEnumerator<ITask> JoystickUpdateAxesHandler(game.UpdateAxes update)
        {
            var p = (Port<game.UpdateAxes>)this.gameControllerNotify[typeof(game.UpdateAxes)];
            if (p.ItemCount > 10)
            {
                Console.WriteLine("Joystick Axes backlog: " + p.ItemCount);
            }

            if (this.dashboardForm != null)
            {
                WinFormsServicePort.FormInvoke(() => this.dashboardForm.UpdateJoystickAxes(update.Body));
            }

            yield break;
        }

        /// <summary>
        /// Handle updates to the buttons on the Gamepad
        /// </summary>
        /// <param name="update">The parameter is not used.</param>
        /// <returns>An Iterator</returns>
        private IEnumerator<ITask> JoystickUpdateButtonsHandler(game.UpdateButtons update)
        {
            if (this.dashboardForm != null)
            {
                WinFormsServicePort.FormInvoke(() => this.dashboardForm.UpdateJoystickButtons(update.Body));
            }

            yield break;
        }

        /// <summary>
        /// Handle changes to the Joystick
        /// </summary>
        /// <param name="onChangeJoystick">The change message</param>
        /// <returns>An Iterator</returns>
        private IEnumerator<ITask> OnChangeJoystickHandler(OnChangeJoystick onChangeJoystick)
        {
            if (onChangeJoystick.DashboardForm == this.dashboardForm)
            {
                Activate(Arbiter.Choice(
                    this.gameControllerPort.ChangeController(onChangeJoystick.Joystick),
                    response => this.LogInfo("Changed Joystick"),
                    f => this.LogError(null, "Unable to change Joystick", f)));
            }

            yield break;
        }

        #endregion

        #region Drive Operations

        /// <summary>
        /// Connect to the Diff Drive
        /// </summary>
        /// <returns>An Iterator</returns>
        private IEnumerator<ITask> ConnectDriveHandler()
        {
            var request = new drive.EnableDriveRequest { Enable = true };

            if (this.drivePort != null)
            {
                yield return Arbiter.Choice(this.drivePort.EnableDrive(request), EmptyHandler, LogError);
            }
        }

        /// <summary>
        /// Handle Motion Commands
        /// </summary>
        /// <param name="onMove">The motion command</param>
        /// <returns>An Iterator</returns>
        private IEnumerator<ITask> OnMoveHandler(OnMove onMove)
        {
            var p = (Port<OnMove>)this.eventsPort[typeof(OnMove)];
            if (p.ItemCount > 10)
            {
                Console.WriteLine("OnMove backlog: " + p.ItemCount);
            }

            if (onMove.DashboardForm == this.dashboardForm && this.drivePort != null)
            {
                var request = new drive.SetDrivePowerRequest
                    {
                        LeftWheelPower = onMove.Left * MotorPowerSaleFactor,
                        RightWheelPower = onMove.Right * MotorPowerSaleFactor
                    };

                yield return Arbiter.Choice(this.drivePort.SetDrivePower(request), EmptyHandler, LogError);
            }
        }

        /// <summary>
        /// Handle Motion Commands
        /// </summary>
        /// <param name="onCommand">The motion command</param>
        /// <returns>An Iterator</returns>
        private IEnumerator<ITask> OnMotionCommandHandler(OnMotionCommand onCommand)
        {
            if (onCommand.DashboardForm == this.dashboardForm && this.drivePort != null)
            {
                switch (onCommand.Command)
                {
                    case MOTIONCOMMANDS.Rotate:

                        var rotRequest = new drive.RotateDegreesRequest
                            { Degrees = onCommand.Parameter, Power = onCommand.Power * MotorPowerSaleFactor };

                        yield return Arbiter.Choice(this.drivePort.RotateDegrees(rotRequest), EmptyHandler, LogError);
                        break;

                    case MOTIONCOMMANDS.Translate:
                        var transRequest = new drive.DriveDistanceRequest
                            { Distance = onCommand.Parameter, Power = onCommand.Power * MotorPowerSaleFactor };

                        yield return Arbiter.Choice(this.drivePort.DriveDistance(transRequest), EmptyHandler, LogError);
                        break;

                    case MOTIONCOMMANDS.Enable:
                        var request = new drive.EnableDriveRequest { Enable = true };

                        yield return Arbiter.Choice(this.drivePort.EnableDrive(request), EmptyHandler, LogError);
                        break;

                    default:
                        LogInfo("Requesting EStop");
                        var stopRequest = new drive.AllStopRequest();

                        yield return Arbiter.Choice(this.drivePort.AllStop(stopRequest), EmptyHandler, LogError);
                        break;
                }
            }

            yield break;
        }

        #endregion

        #region Camera

        /// <summary>
        /// Initialize the Web camera
        /// </summary>
        /// <returns>An iterator</returns>
        private IEnumerator<ITask> ConnectWebCamHandler()
        {
            Fault fault = null;

            yield return
                Arbiter.Choice(
                    this.webCamPort.Subscribe(this.webCamNotify),
                    EmptyHandler,
                    f => fault = f);

            if (fault != null)
            {
                LogError(null, "Failed to subscribe to webcam", fault);
                yield break;
            }

            var runForm = new RunForm(this.CreateWebCamForm);

            WinFormsServicePort.Post(runForm);

            yield return Arbiter.Choice(runForm.pResult, EmptyHandler, e => fault = Fault.FromException(e));

            if (fault != null)
            {
                LogError(null, "Failed to Create WebCam window", fault);
                yield break;
            }

            yield break;
        }

        /// <summary>
        /// Create a form for the webcam 
        /// </summary>
        /// <returns>A Webcam Form</returns>
        private Form CreateWebCamForm()
        {
            this.cameraForm = new WebCamForm(
                            this.mainPort,
                            this.state.Options.WebcamWindowStartX,
                            this.state.Options.WebcamWindowStartY,
                            this.state.Options.WebcamWindowWidth,
                            this.state.Options.WebcamWindowHeight);
            return this.cameraForm;
        }

        /// <summary>
        /// Handler for new frames from the camera 
        /// </summary>
        /// <param name="replace">A webcamsensor Replace message with the image</param>
        private void CameraUpdateFrameHandler(webcamsensor.Replace replace)
        {
            // Make sure that there is a form to display the image on
            if (this.cameraForm == null)
            {
                return;
            }

            var bmp   = new Bitmap(replace.Body.Width, replace.Body.Height, PixelFormat.Format24bppRgb);
            var bmpData = bmp.LockBits(
                new Rectangle(0, 0, bmp.Width, bmp.Height),
                ImageLockMode.WriteOnly,
                bmp.PixelFormat);
            Marshal.Copy(replace.Body.Data, 0, bmpData.Scan0, replace.Body.Data.Length);
            bmp.UnlockBits(bmpData);

            // MakeBitmap(frame.Size.Width, frame.Size.Height, frame.Frame);
            this.DisplayImage(bmp);
        }

        /// <summary>
        /// Display an image in the WebCam Form
        /// </summary>
        /// <param name="bmp">
        /// The bitmap to display
        /// </param>
        private void DisplayImage(Bitmap bmp)
        {
            Fault fault = null;

            var setImage = new FormInvoke(() => this.cameraForm.CameraImage = bmp);

            WinFormsServicePort.Post(setImage);

            Arbiter.Choice(setImage.ResultPort, EmptyHandler, e => fault = Fault.FromException(e));

            if (fault == null)
            {
                // LogInfo("New camera frame");
                return;
            }
            
            this.LogError(null, "Unable to set camera image on form", fault);
            return;
        }

        #endregion

        #region Depth Camera

        /// <summary>
        /// Initialize the Depth Camera
        /// </summary>
        /// <returns>An iterator</returns>
        private IEnumerator<ITask> ConnectDepthCamHandler()
        {
            Fault fault = null;

            yield return Arbiter.Choice(
                this.depthCamSensorPort.Subscribe(this.depthCamSensorNotify),
                EmptyHandler,
                f => fault = f);

            if (fault != null)
            {
                LogError(null, "Failed to subscribe to DepthCam", fault);
                yield break;
            }

            var runForm = new RunForm(this.CreateDepthCamForm);

            WinFormsServicePort.Post(runForm);

            yield return Arbiter.Choice(runForm.pResult, EmptyHandler, e => fault = Fault.FromException(e));

            if (fault != null)
            {
                LogError(null, "Failed to Create DepthCam window", fault);
                yield break;
            }

            yield break;
        }

        /// <summary>
        /// Create a form for the Depthcam
        /// </summary>
        /// <returns>An iterator</returns>
        private Form CreateDepthCamForm()
        {
            this.depthCameraForm = new DepthCamForm(
                                this.mainPort,
                                this.state.Options.DepthcamWindowStartX,
                                this.state.Options.DepthcamWindowStartY,
                                this.state.Options.DepthcamWindowWidth,
                                this.state.Options.DepthcamWindowHeight);
            return this.depthCameraForm;
        }

        /// <summary>
        /// Handler for new frames from the depth camera
        /// </summary>
        /// <param name="replace">A depthcamsensor Replace message containing the depth data</param>
        private void DepthCameraUpdateFrameHandler(depthcamsensor.Replace replace)
        {
            // Make sure that there is a form to display the image on
            if (this.depthCameraForm == null)
            {
                return;
            }

            int width = replace.Body.DepthImageSize.Width;
            int height = replace.Body.DepthImageSize.Height;
            Bitmap bmp = this.MakeDepthBitmap(width, height, replace.Body.DepthImage);
            this.DisplayDepthImage(bmp);

            return;
        }

        /// <summary>
        /// Creates a Bitmap from Depth Data
        /// </summary>
        /// <param name="width">Width of the image</param>
        /// <param name="height">Height of the image</param>
        /// <param name="depthData">Raw depth data (in millimeters)</param>
        /// <returns>A grayscale bitmap</returns>
        private Bitmap MakeDepthBitmap(int width, int height, short[] depthData)
        {
            // NOTE: This code implicitly assumes that the width is a multiple
            // of four bytes because Bitmaps have to be longword aligned.
            // We really should look at bmp.Stride to see if there is any padding.
            // However, the width and height come from the webcam and most cameras
            // have resolutions that are multiples of four.

            const short MaxDepthDataValue = 4003;

            var buff = new byte[width * height * 3];
            var j = 0;
            for (var i = 0; i < width * height; i++)
            {
                // Convert the data to a suitable range
                byte val;
                if (depthData[i] >= MaxDepthDataValue)
                {
                    val = byte.MaxValue;
                }
                else
                {
                    val = (byte)((depthData[i] / (double)MaxDepthDataValue) * byte.MaxValue);
                }

                // Set all R, G and B values the same, i.e. gray scale
                buff[j++] = val;
                buff[j++] = val;
                buff[j++] = val;
            }

            // NOTE: Windows Forms do not support Format16bppGrayScale which would be the
            // ideal way to display the data. Instead it is converted to RGB with all the
            // color values the same, i.e. 8-bit gray scale.
            Bitmap bmp = null;

            try
            {
                bmp = new Bitmap(width, height, PixelFormat.Format24bppRgb); 

                var data = bmp.LockBits(
                    new Rectangle(0, 0, bmp.Width, bmp.Height),
                    ImageLockMode.WriteOnly,
                    PixelFormat.Format24bppRgb);

                Marshal.Copy(buff, 0, data.Scan0, buff.Length);

                bmp.UnlockBits(data);
            }
            catch (Exception ex)
            {
                if (bmp != null)
                {
                    bmp.Dispose();
                    bmp = null;
                }

                LogError(ex);
            }

            return bmp;
        }

        /// <summary>
        /// Display an image in the DepthCam Form
        /// </summary>
        /// <param name="bmp">
        /// The bitmap to display
        /// </param>
        private void DisplayDepthImage(Bitmap bmp)
        {
            Fault fault = null;

            var setImage = new FormInvoke(() => this.depthCameraForm.CameraImage = bmp);

            WinFormsServicePort.Post(setImage);

            Arbiter.Choice(setImage.ResultPort, EmptyHandler, e => fault = Fault.FromException(e));

            if (fault != null)
            {
                LogError(null, "Unable to set depth camera image on form", fault);
                return;
            }
            
            // LogInfo("New camera frame");
            return;
        }

        #endregion
    }
}
