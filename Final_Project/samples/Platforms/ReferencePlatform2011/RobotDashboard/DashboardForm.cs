//-----------------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples
//
//  <copyright file="DashboardForm.cs" company="Microsoft Corporation">
//      Copyright (C) Microsoft Corporation.  All rights reserved.
//  </copyright>
//
//  Main Form for the Dashboard
//
//  $File: DashboardForm.cs $ $Revision: 1 $
//-----------------------------------------------------------------------------

namespace Microsoft.Robotics.Services.RobotDashboard
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.IO;
    using System.Windows.Forms;

    using Microsoft.Ccr.Core;
    using Microsoft.Dss.Core;
    using Microsoft.Dss.ServiceModel.Dssp;
    using Microsoft.Robotics.PhysicalModel.Proxy;

    using battery = Microsoft.Robotics.Services.Battery.Proxy;
    using cs = Microsoft.Dss.Services.Constructor;
    using drive = Microsoft.Robotics.Services.Drive.Proxy;
    using joystick = Microsoft.Robotics.Services.GameController.Proxy;
    using webcam = Microsoft.Robotics.Services.WebCam.Proxy;

    /// <summary>
    /// The main Dashboard Form
    /// </summary>
    public partial class DashboardForm : Form
    {
        /// <summary>
        /// Maximum tilt of the Kinect (up)
        /// </summary>
        private const double MaxTilt = 27;

        /// <summary>
        /// Minimum tilt of the Kinect (down)
        /// </summary>
        private const double MinTilt = -27;

        /// <summary>
        /// The increment for tilting using the buttons
        /// </summary>
        private const double TiltIncrement = 5;

        /// <summary>
        /// The port for sending events
        /// </summary>
        private DashboardFormEvents eventsPort;

        /// <summary>
        /// Holds the option settings
        /// </summary>
        private GUIOptions options;

        /// <summary>
        /// The last axes reading from the joystick
        /// </summary>
        private joystick.Axes lastAxes = new joystick.Axes();

        /// <summary>
        /// Initializes a new instance of the DashboardForm class
        /// </summary>
        /// <param name="theEventsPort">The Events Port for passing events back to the service</param>
        /// <param name="state">The service state</param>
        public DashboardForm(DashboardFormEvents theEventsPort, RobotDashboardState state)
        {
            this.eventsPort = theEventsPort;

            this.InitializeComponent();

            this.options = new GUIOptions();
            this.options = state.Options;

            this.StartPosition = FormStartPosition.Manual;
            this.Location = new Point(this.options.WindowStartX, this.options.WindowStartY);

            // The dead zone can't be negative, but it can be zero
            this.options.DeadZoneX = Math.Abs(state.Options.DeadZoneX);
            this.options.DeadZoneY = Math.Abs(state.Options.DeadZoneY);

            // Just in case the scale factors have not been initialized
            if (state.Options.TranslateScaleFactor == 0)
            {
                this.options.TranslateScaleFactor = 1.0;
            }
            else
            {
                this.options.TranslateScaleFactor = state.Options.TranslateScaleFactor;
            }

            if (state.Options.RotateScaleFactor == 0)
            {
                this.options.RotateScaleFactor = 0.5;
            }
            else
            {
                this.options.RotateScaleFactor = state.Options.RotateScaleFactor;
            }
        }

        /// <summary>
        /// Update the Sensor values on the screen
        /// </summary>
        /// <param name="values">The array of sensor values</param>
        /// <returns>A value indicating if the updates was successful (true) or not (false)</returns>
        public bool UpdateIRSensors(double[] values)
        {
            if (values == null || values.Length != Constants.IRSensorCount)
            {
                return false;
            }

            // Set all the values on the screen
            this.IRLeftLabel.Text = values[(int)IrSensorNames.IrLeft].ToString("F0");
            this.IRCenterLabel.Text = values[(int)IrSensorNames.IrCenter].ToString("F0");
            this.IRRightLabel.Text = values[(int)IrSensorNames.IrRight].ToString("F0");

            return true;
        }

        /// <summary>
        /// Update the Sensor values on the screen
        /// </summary>
        /// <param name="values">The array of sonar sensor values</param>
        /// <returns>A value indicating if the updates was successful (true) or not (false)</returns>
        public bool UpdateSonarSensors(double[] values)
        {
            if (values == null || values.Length != Constants.SonarSensorCount)
            {
                return false;
            }

            // Set all the values on the screen
            this.SonarLeftLabel.Text = values[(int)SonarSensorNames.SonarLeft].ToString("F0");
            this.SonarRightLabel.Text = values[(int)SonarSensorNames.SonarRight].ToString("F0");

            return true;
        }

        /// <summary>
        /// Updates the battery level.
        /// </summary>
        /// <param name="batteryState">State of the battery.</param>
        public void UpdateBatteryLevel(battery.BatteryState batteryState)
        {
            if (batteryState == null)
            {
                this.BatteryLevelTxt.Text = "No battery";
            }
            else
            {
                this.BatteryLevelTxt.Text = ((int)batteryState.PercentBatteryPower) + "%";
                this.BatteryLevel.Value = Math.Min(this.BatteryLevel.Maximum, (int)batteryState.PercentBatteryPower);
                this.BatteryLevel.ForeColor = batteryState.PercentBatteryPower > batteryState.PercentCriticalBattery
                                                  ? SystemColors.Highlight
                                                  : Color.Red;
            }
        }

        /// <summary>
        /// Updates the state of the wheels.
        /// </summary>
        /// <param name="state">The state.</param>
        public void UpdateWheelState(drive.DriveDifferentialTwoWheelState state)
        {
            this.LeftWheelSpeed.Text = state.LeftWheel.WheelSpeed.ToString("0.00");
            this.RightWheelSpeed.Text = state.RightWheel.WheelSpeed.ToString("0.00");
            this.LeftEncoderTicks.Text = state.LeftWheel.EncoderState.CurrentReading.ToString();
            this.RightEncoderTicks.Text = state.RightWheel.EncoderState.CurrentReading.ToString();
        }

        /// <summary>
        /// Handle Form Load
        /// </summary>
        /// <param name="sender">The parameter is not used.</param>
        /// <param name="e">The parameter is not used.</param>
        private void DashboardFormLoad(object sender, EventArgs e)
        {
            this.eventsPort.Post(new OnLoad(this));
        }

        /// <summary>
        /// Handle Form Closed
        /// </summary>
        /// <param name="sender">The parameter is not used.</param>
        /// <param name="e">The parameter is not used.</param>
        private void DashboardFormFormClosed(object sender, FormClosedEventArgs e)
        {
            this.eventsPort.Post(new OnClosed(this));
        }

        #region Joystick

        /// <summary>
        /// Handle a change to the list of Joysticks
        /// </summary>
        /// <param name="sender">The parameter is not used.</param>
        /// <param name="e">The parameter is not used.</param>
        private void JoystickSelectedIndexChanged(object sender, EventArgs e)
        {
            IEnumerable<joystick.Controller> list = this.JoystickComboBox.Tag as IEnumerable<joystick.Controller>;

            if (list != null)
            {
                if (this.JoystickComboBox.SelectedIndex >= 0)
                {
                    int index = 0;
                    foreach (joystick.Controller controller in list)
                    {
                        if (index == this.JoystickComboBox.SelectedIndex)
                        {
                            OnChangeJoystick change = new OnChangeJoystick(this);
                            change.Joystick = controller;

                            this.eventsPort.Post(change);

                            return;
                        }

                        index++;
                    }
                }
            }
        }

        /// <summary>
        /// Update the list of available joysticks
        /// </summary>
        /// <param name="controllers">The list of joysticks</param>
        public void ReplaceJoystickList(IEnumerable<joystick.Controller> controllers)
        {
            this.JoystickComboBox.BeginUpdate();
            try
            {
                this.JoystickComboBox.Items.Clear();
                foreach (joystick.Controller controller in controllers)
                {
                    int index = this.JoystickComboBox.Items.Add(controller.InstanceName);
                    if (controller.Current == true)
                    {
                        this.JoystickComboBox.SelectedIndex = index;
                    }
                }

                this.JoystickComboBox.Tag = controllers;
            }
            finally
            {
                this.JoystickComboBox.EndUpdate();
            }
        }

        /// <summary>
        /// Handle updates to the joystick position
        /// </summary>
        /// <param name="axes">The new Axis values</param>
        public void UpdateJoystickAxes(joystick.Axes axes)
        {
            int x = axes.X;
            int y = -axes.Y;

            this.lastAxes = axes;
            this.lblX.Text = x.ToString();
            this.lblY.Text = y.ToString();
            this.lblZ.Text = axes.Z.ToString();

            this.DrawJoystick(x, y);

            // Only send a drive reqeust if not stopped and enabled
            if (this.StopCheckBox.Checked == false)
            {
                double left;
                double right;

                if (this.DriveCheckBox.Checked == true)
                {
                    // This is the raw length of the vector
                    double magnitude = Math.Sqrt((x * x) + (y * y));

                    // Check for the "dead zone"
                    // Adjust the speed values so that they do not
                    // suddenly jump after leaving the Dead Zone
                    if (Math.Abs(x) < this.options.DeadZoneX)
                    {
                        x = 0;
                    }
                    else
                    {
                        // Subtract off the dead zone value so that the
                        // coord starts from zero
                        if (x > 0)
                        {
                            // Remove the Dead Zone and rescale
                            int temp = x - (int)this.options.DeadZoneX;
                            x = temp * 1000 / (1000 - (int)this.options.DeadZoneX);
                        }
                        else
                        {
                            // Remove the Dead Zone and rescale
                            int temp = x + (int)this.options.DeadZoneX;
                            x = temp * 1000 / (1000 - (int)this.options.DeadZoneX);
                        }
                    }

                    if (Math.Abs(y) < this.options.DeadZoneY)
                    {
                        y = 0;
                    }
                    else
                    {
                        if (y > 0)
                        {
                            // Remove the Dead Zone and rescale
                            int temp = y - (int)this.options.DeadZoneY;
                            y = temp * 1000 / (1000 - (int)this.options.DeadZoneY);
                        }
                        else
                        {
                            // Remove the Dead Zone and rescale
                            int temp = y + (int)this.options.DeadZoneY;
                            y = temp * 1000 / (1000 - (int)this.options.DeadZoneY);
                        }
                    }

                    if (x == 0 && y == 0)
                    {
                        // Totally dead in the middle!
                        left = right = 0;
                    }
                    else
                    {
                        // Angle of the vector
                        double theta = Math.Atan2(y, x);

                        // This is the maximum magnitude for a given angle
                        // double maxMag;
                        double scaledMag = 1.0;

                        // A scaled down magnitude according to above
                        // double scaledMag = magnitude * 1000 / maxMag;
                        scaledMag = magnitude;

                        // Decompose the vector into motor components
                        left = ((scaledMag * this.options.TranslateScaleFactor) * Math.Sin(theta)) + 
                                    ((scaledMag * this.options.RotateScaleFactor) * Math.Cos(theta));
                        right = ((scaledMag * this.options.TranslateScaleFactor) * Math.Sin(theta)) - 
                                    ((scaledMag * this.options.RotateScaleFactor) * Math.Cos(theta));
                    }
                }
                else
                {
                    left = right = 0;
                }

                // Cap at 1000
                left = Math.Min(left, 1000);
                right = Math.Min(right, 1000);
                left = Math.Max(left, -1000);
                right = Math.Max(right, -1000);

                // Quick and dirty way to display results for debugging -
                // Uncomment the two lines below
                ////Console.WriteLine("Joy: " + data.X + ", " + data.Y
                ////        + " => " + left + ", " + right);

                this.eventsPort.Post(new OnMove(this, (int)Math.Round(left), (int)Math.Round(right)));
            }
        }

        /// <summary>
        /// Handle the Joystick buttons
        /// </summary>
        /// <param name="buttons">The current state of all the buttons</param>
        public void UpdateJoystickButtons(joystick.Buttons buttons)
        {
            if (buttons.Pressed != null && buttons.Pressed.Count > 0)
            {
                string[] buttonString = buttons.Pressed.ConvertAll<string>(
                    delegate(bool button)
                    {
                        return button ? "X" : "O";
                    }).ToArray();

                this.lblButtons.Text = string.Join(" ", buttonString);

                if (this.StopCheckBox.Checked && buttons.Pressed.Count > 2)
                {
                    if (buttons.Pressed[2] == true)
                    {
                        this.StopCheckBox.Checked = false;
                    }
                }
                else if (buttons.Pressed.Count > 1 && buttons.Pressed[1] == true)
                {
                    this.StopCheckBox.Checked = true;
                }

                if (buttons.Pressed[0])
                {
                    // Toggle the Drive button
                    this.DriveCheckBox.Checked = !this.DriveCheckBox.Checked;
                }
            }
        }

        /// <summary>
        /// Draw the "joystick" on the screen (a.k.a. Trackball)
        /// </summary>
        /// <param name="x">The x position of the joystick</param>
        /// <param name="y">The y position of the joystick</param>
        private void DrawJoystick(int x, int y)
        {
            Bitmap bmp = null;

            try
            {
                bmp = new Bitmap(this.JoystickPicture.Width, this.JoystickPicture.Height); 
                
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    using (GraphicsPath path = new GraphicsPath())
                    {
                        int width = bmp.Width - 1;
                        int height = bmp.Height - 1;

                        g.Clear(Color.Transparent);
                        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                        path.AddEllipse(0, 0, width, height);

                        using (PathGradientBrush pathBrush = new PathGradientBrush(path))
                        {
                            pathBrush.CenterPoint = new PointF(width / 3f, height / 3f);
                            pathBrush.CenterColor = Color.White;
                            pathBrush.SurroundColors = new Color[] { Color.LightGray };

                            g.FillPath(pathBrush, path);
                            g.DrawPath(Pens.Black, path);

                            int partial = y * height / 2200;
                            if (partial > 0)
                            {
                                g.DrawArc(
                                    Pens.Black,
                                    0,
                                    (height / 2) - partial,
                                    width,
                                    2 * partial,
                                    180,
                                    180);
                            }
                            else if (partial == 0)
                            {
                                g.DrawLine(Pens.Black, 0, height / 2, width, height / 2);
                            }
                            else
                            {
                                g.DrawArc(
                                    Pens.Black,
                                    0,
                                    (height / 2) + partial,
                                    width,
                                    -2 * partial,
                                    0,
                                    180);
                            }

                            partial = x * width / 2200;
                            if (partial > 0)
                            {
                                g.DrawArc(
                                    Pens.Black,
                                    (width / 2) - partial,
                                    0,
                                    2 * partial,
                                    height,
                                    270,
                                    180);
                            }
                            else if (partial == 0)
                            {
                                g.DrawLine(Pens.Black, width / 2, 0, width / 2, height);
                            }
                            else
                            {
                                g.DrawArc(
                                    Pens.Black,
                                    (width / 2) + partial,
                                    0,
                                    -2 * partial,
                                    height,
                                    90,
                                    180);
                            }

                            this.JoystickPicture.Image = bmp;
                            bmp = null;
                        }
                    }
                }
            }
            finally
            {
                if (bmp != null)
                {
                    bmp.Dispose();
                    bmp = null;
                }
            }
        }

        #endregion

        /// <summary>
        /// Handle the Drive Enable/Disable CheckBox
        /// </summary>
        /// <param name="sender">The parameter is not used.</param>
        /// <param name="e">The parameter is not used.</param>
        private void DriveCheckBoxCheckedChanged(object sender, EventArgs e)
        {
            if (this.DriveCheckBox.Checked)
            {
                this.DriveCheckBox.Text = "Disable Drive";
            }
            else
            {
                this.DriveCheckBox.Text = "Enable Drive";
            }

            this.UpdateJoystickAxes(this.lastAxes);
        }

        /// <summary>
        /// Handle the Stop CheckBox
        /// </summary>
        /// <param name="sender">The parameter is not used.</param>
        /// <param name="e">The parameter is not used.</param>
        private void StopCheckBoxCheckedChanged(object sender, EventArgs e)
        {
            if (this.StopCheckBox.Checked)
            {
                // Perform an Emergency Stop which disables the drive
                this.eventsPort.Post(new OnMotionCommand(this, MOTIONCOMMANDS.Stop, 0, 0));
            }
            else
            {
                // Re-enable the drive
                this.eventsPort.Post(new OnMotionCommand(this, MOTIONCOMMANDS.Enable, 0, 0));
            }
        }
        
        /// <summary>
        /// Handle Mouse Leave events for the Track Ball
        /// </summary>
        /// <param name="sender">The Track Ball</param>
        /// <param name="e">The Mouse event arguments</param>
        /// <remarks>When the mouse leaves the Track Ball, it should pop back to the zero position.
        /// However, it currently grabs the mouse so you can actually move outside the window.
        /// </remarks>
        private void JoystickPictureMouseLeave(object sender, EventArgs e)
        {
            this.UpdateJoystickButtons(new joystick.Buttons());
            this.UpdateJoystickAxes(new joystick.Axes());
        }

        /// <summary>
        /// Handle movements over the Track Ball
        /// </summary>
        /// <param name="sender">The Track Ball</param>
        /// <param name="e">The Mouse event arguments</param>
        private void JoystickPictureMouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                int x, y;
                x = Math.Min(this.JoystickPicture.Width, Math.Max(e.X, 0));
                y = Math.Min(this.JoystickPicture.Height, Math.Max(e.Y, 0));

                // Convert the values so that they are in the range -1000 to +1000
                x = ((x * 2000) / this.JoystickPicture.Width) - 1000;
                y = ((y * 2000) / this.JoystickPicture.Height) - 1000;

                // Simulate a message from the Xbox Controller
                joystick.Axes axes = new joystick.Axes();
                axes.X = x;
                axes.Y = y;
                this.UpdateJoystickAxes(axes);
            }
        }

        /// <summary>
        /// Handle Mouse Up events for the Track Ball
        /// </summary>
        /// <param name="sender">The Track Ball</param>
        /// <param name="e">The Mouse event arguments</param>
        private void JoystickPictureMouseUp(object sender, MouseEventArgs e)
        {
            // Just use the Mouse Leave handler
            this.JoystickPictureMouseLeave(sender, e);
        }
        
        /// <summary>
        /// File Save Dialog
        /// </summary>
        /// <param name="sender">The parameter is not used.</param>
        /// <param name="e">The Cancel Event Args used to cancel saving</param>
        private void SaveFileDialogFileOk(object sender, CancelEventArgs e)
        {
            string path = Path.GetFullPath(this.saveFileDialog.FileName);
            if (!path.StartsWith(this.saveFileDialog.InitialDirectory))
            {
                MessageBox.Show("Log file must be in a subdirectory of the store", Text, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                e.Cancel = true;
            }
        }

        #region Menu Items

        /// <summary>
        /// Handle Save Settings Menu Item
        /// </summary>
        /// <param name="sender">The parameter is not used.</param>
        /// <param name="e">The parameter is not used.</param>
        private void SaveSettingsMenuItemClick(object sender, EventArgs e)
        {
            OnOptionSettings opt = new OnOptionSettings(this, this.options);
            opt.Options.WindowStartX = this.Location.X;
            opt.Options.WindowStartY = this.Location.Y;
            this.eventsPort.Post(opt);
        }

        /// <summary>
        /// Handle Options Menu Item
        /// </summary>
        /// <param name="sender">The parameter is not used.</param>
        /// <param name="e">The parameter is not used.</param>
        private void OptionsMenuItemClick(object sender, EventArgs e)
        {
            GUIOptions opt = this.options;
            using (OptionsForm optDialog = new OptionsForm(ref opt))
            {
                DialogResult result = optDialog.ShowDialog();
                if (result == DialogResult.OK)
                {
                    this.options = opt;
                    ////ReformatForm();
                }
            }
        }

        /// <summary>
        /// Handle Exit Menu Item
        /// </summary>
        /// <param name="sender">The parameter is not used.</param>
        /// <param name="e">The parameter is not used.</param>
        private void ExitMenuItemClick(object sender, EventArgs e)
        {
            this.eventsPort.Post(new OnClosed(this));
            this.Close();
        }

        /// <summary>
        /// Handle About Menu Item
        /// </summary>
        /// <param name="sender">The parameter is not used.</param>
        /// <param name="e">The parameter is not used.</param>
        private void AboutMenuItemClick(object sender, EventArgs e)
        {
            using (AboutBox about = new AboutBox())
            {
                about.ShowDialog();
            }
        }

        #endregion

        #region Tilt Handlers

        /// <summary>
        /// Handle the Set Tilt button
        /// </summary>
        /// <param name="sender">The parameter is not used.</param>
        /// <param name="e">The parameter is not used.</param>
        private void SetTiltButtonClick(object sender, EventArgs e)
        {
            double tilt;
            if (double.TryParse(this.TiltTextbox.Text, out tilt))
            {
                if (tilt >= MinTilt && tilt <= MaxTilt)
                {
                    this.eventsPort.Post(new OnChangeTilt(this, tilt));
                }
                else
                {
                    MessageBox.Show("Tilt angle must be between" + MinTilt + " and " + MaxTilt);
                }
            }
            else
            {
                MessageBox.Show("Enter a Tilt angle first");
            }
        }

        /// <summary>
        /// Handle the Tilt Up button
        /// </summary>
        /// <param name="sender">The parameter is not used.</param>
        /// <param name="e">The parameter is not used.</param>
        private void TiltUpButtonClick(object sender, EventArgs e)
        {
            double tilt;
            if (double.TryParse(this.TiltTextbox.Text, out tilt))
            {
                if (tilt >= MinTilt && tilt <= (MaxTilt - TiltIncrement))
                {
                    tilt += 5;
                    this.eventsPort.Post(new OnChangeTilt(this, tilt));
                    this.TiltTextbox.Text = tilt.ToString();
                }
                else
                {
                    MessageBox.Show("Tilt angle must be between " + MinTilt + " and " + (MaxTilt - TiltIncrement) + " to tilt up");
                }
            }
            else
            {
                MessageBox.Show("Make sure the Tilt angle is valid");
            }
        }

        /// <summary>
        /// Handle the Tilt Down button
        /// </summary>
        /// <param name="sender">The parameter is not used.</param>
        /// <param name="e">The parameter is not used.</param>
        private void TiltDownButtonClick(object sender, EventArgs e)
        {
            double tilt;
            if (double.TryParse(this.TiltTextbox.Text, out tilt))
            {
                if (tilt >= (MinTilt + TiltIncrement) && tilt <= MaxTilt)
                {
                    tilt -= 5;
                    this.eventsPort.Post(new OnChangeTilt(this, tilt));
                    this.TiltTextbox.Text = tilt.ToString();
                }
                else
                {
                    MessageBox.Show("Tilt angle must be between " + (MinTilt + TiltIncrement) + " and " + MaxTilt + " to tilt down");
                }
            }
            else
            {
                MessageBox.Show("Make sure the Tilt angle is valid");
            }
        }

        /// <summary>
        /// Handle the Reset Tilt button
        /// </summary>
        /// <param name="sender">The parameter is not used.</param>
        /// <param name="e">The parameter is not used.</param>
        private void TiltResetButtonClick(object sender, EventArgs e)
        {
            this.eventsPort.Post(new OnChangeTilt(this, 0));
            this.TiltTextbox.Text = "0";
        }

        #endregion

        /// <summary>
        /// Handles the Click event of the btnResetEncoders control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void BtnResetEncodersClick(object sender, EventArgs e)
        {
            this.eventsPort.Post(new OnResetEncoders(this));
        }
    }

    /// <summary>
    /// Operations Port for Dashboard Events
    /// </summary>
    public class DashboardFormEvents : 
        PortSet<OnLoad,
            OnClosed,
            OnChangeJoystick,
            //// OnStartService,
            OnMove,
            OnMotionCommand,
            OnChangeTilt,
            OnQueryFrame,
            OnOptionSettings,
            OnResetEncoders>
    {
    }
    
    /// <summary>
    /// Class used for events sent by the Dashboard Form back to the service
    /// </summary>
    public class DashboardFormEvent
    {
        /// <summary>
        ///  Dashboard Form
        /// </summary>
        private DashboardForm dashboardForm;

        /// <summary>
        /// Gets or sets the associated Form
        /// </summary>
        public DashboardForm DashboardForm
        {
            get { return this.dashboardForm; }
            set { this.dashboardForm = value; }
        }

        /// <summary>
        /// Initializes an instance of the DashboardFormEvent class
        /// </summary>
        /// <param name="dashboardForm">The associated Form</param>
        public DashboardFormEvent(DashboardForm dashboardForm)
        {
            this.dashboardForm = dashboardForm;
        }
    }

    /// <summary>
    /// Form Loaded message
    /// </summary>
    public class OnLoad : DashboardFormEvent
    {
        /// <summary>
        /// Initializes an instance of the OnLoad class
        /// </summary>
        /// <param name="form">The associated Form</param>
        public OnLoad(DashboardForm form)
            : base(form)
        {
        }
    }

    /// <summary>
    /// Connect button pressed message
    /// </summary>
    public class OnConnect : DashboardFormEvent
    {
        /// <summary>
        /// The service to connect to
        /// </summary>
        private string service;

        /// <summary>
        /// Gets or sets the service to connect to
        /// </summary>
        public string Service
        {
            get { return this.service; }
            set { this.service = value; }
        }

        /// <summary>
        /// Initializes an instance of the OnConnect class
        /// </summary>
        /// <param name="form">The associated form</param>
        /// <param name="service">The service</param>
        public OnConnect(DashboardForm form, string service)
            : base(form)
        {
            this.service = service;
        }
    }

    /// <summary>
    /// Reset Encoders message
    /// </summary>
    public class OnResetEncoders : DashboardFormEvent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OnResetEncoders"/> class.
        /// </summary>
        /// <param name="dashboardForm">The associated Form</param>
        public OnResetEncoders(DashboardForm dashboardForm)
            : base(dashboardForm)
        {
        }
    }

    /// <summary>
    /// Form Closed message
    /// </summary>
    public class OnClosed : DashboardFormEvent
    {
        /// <summary>
        /// Initializes an instance of the OnClosed class
        /// </summary>
        /// <param name="form">The associated Form</param>
        public OnClosed(DashboardForm form)
            : base(form)
        {
        }
    }

    /// <summary>
    /// Joystick controller changed message
    /// </summary>
    public class OnChangeJoystick : DashboardFormEvent
    {
        /// <summary>
        /// The joystick
        /// </summary>
        private joystick.Controller joystick;

        /// <summary>
        /// Gets or sets the Joystick
        /// </summary>
        public joystick.Controller Joystick
        {
            get { return this.joystick; }
            set { this.joystick = value; }
        }

        /// <summary>
        /// Initializes an instance of the OnChangeJoystick class
        /// </summary>
        /// <param name="form">The associated form</param>
        public OnChangeJoystick(DashboardForm form)
            : base(form)
        {
        }
    }

    /// <summary>
    /// Joystick position changed message
    /// </summary>
    public class OnMove : DashboardFormEvent
    {
        /// <summary>
        /// The left power
        /// </summary>
        private int left;

        /// <summary>
        /// Gets or sets the Left Wheel Power
        /// </summary>
        public int Left
        {
            get { return this.left; }
            set { this.left = value; }
        }

        /// <summary>
        /// The right power
        /// </summary>
        private int right;

        /// <summary>
        /// Gets or sets the Right Wheel Power
        /// </summary>
        public int Right
        {
            get { return this.right; }
            set { this.right = value; }
        }

        /// <summary>
        /// Initializes an instance of the OnMove class
        /// </summary>
        /// <param name="form">The associated form</param>
        /// <param name="left">The left power</param>
        /// <param name="right">The right power</param>
        public OnMove(DashboardForm form, int left, int right)
            : base(form)
        {
            this.left = left;
            this.right = right;
        }
    }

    /// <summary>
    /// Motion Commands
    /// </summary>
    public enum MOTIONCOMMANDS
    {
        /// <summary>
        /// Stop moving
        /// </summary>
        Stop,

        /// <summary>
        /// Rotate on the spot
        /// </summary>
        Rotate,

        /// <summary>
        ///  Move in a straight line (forward or backward)
        /// </summary>
        Translate,

        /// <summary>
        /// Enable the drive (after a Stop)
        /// </summary>
        Enable
    }

    /// <summary>
    /// Motion message
    /// </summary>
    public class OnMotionCommand : DashboardFormEvent
    {
        /// <summary>
        /// The motion command
        /// </summary>
        private MOTIONCOMMANDS cmd;

        /// <summary>
        /// Gets or sets the motion command
        /// </summary>
        public MOTIONCOMMANDS Command
        {
            get { return this.cmd; }
            set { this.cmd = value; }
        }

        /// <summary>
        /// The parameter for the command
        /// </summary>
        private double param;

        /// <summary>
        /// Gets or sets the parameter for this command
        /// </summary>
        public double Parameter
        {
            get { return this.param; }
            set { this.param = value; }
        }

        /// <summary>
        /// The power
        /// </summary>
        private double power;

        /// <summary>
        /// Gets or sets the power
        /// </summary>
        public double Power
        {
            get { return this.power; }
            set { this.power = value; }
        }

        /// <summary>
        /// Initializes an instance of the OnMotionCommand class
        /// </summary>
        /// <param name="form">The associated Form</param>
        /// <param name="command">The motion command</param>
        /// <param name="parameter">A parameter to the command</param>
        /// <param name="power">The motor power</param>
        public OnMotionCommand(DashboardForm form, MOTIONCOMMANDS command, double parameter, double power)
            : base(form)
        {
            this.cmd = command;
            this.param = parameter;
            this.power = power;
        }
    }

    /// <summary>
    /// Tilt Change message
    /// </summary>
    public class OnChangeTilt : DashboardFormEvent
    {
        /// <summary>
        /// The tilt angle (degrees)
        /// </summary>
        private double tilt;

        /// <summary>
        /// Gets or sets the Tilt Angle (in degrees)
        /// </summary>
        public double Tilt
        {
            get { return this.tilt; }
            set { this.tilt = value; }
        }

        /// <summary>
        /// Initializes an instance of the OnChangeTilt class
        /// </summary>
        /// <param name="form">The associated form</param>
        /// <param name="tilt">The tilt angle</param>
        public OnChangeTilt(DashboardForm form, double tilt)
            : base(form)
        {
            this.tilt = tilt;
        }
    }
    
    /// <summary>
    /// Set Option Settings message
    /// </summary>
    public class OnOptionSettings : DashboardFormEvent
    {
        /// <summary>
        /// The GUI Options
        /// </summary>
        public GUIOptions Options;

        /// <summary>
        /// Initializes an instance of the OnOptionSettings class
        /// </summary>
        /// <param name="form">The associated form</param>
        public OnOptionSettings(DashboardForm form)
            : base(form)
        {
            this.Options = new GUIOptions();
        }

        /// <summary>
        /// Initializes an instance of the OnQueryFrame class
        /// </summary>
        /// <param name="form">The associated form</param>
        /// <param name="opt">The option settings</param>
        public OnOptionSettings(DashboardForm form, GUIOptions opt)
            : base(form)
        {
            this.Options = new GUIOptions();
            this.Options = opt;
        }
    }

    /// <summary>
    /// Connect Web Cam message
    /// </summary>
    public class OnConnectWebCam : OnConnect
    {
        /// <summary>
        /// Initializes an instance of the OnConnectWebCam class
        /// </summary>
        /// <param name="form">The associated form</param>
        /// <param name="service">The service to connect to</param>
        public OnConnectWebCam(DashboardForm form, string service)
            : base(form, service)
        {
        }
    }

    /// <summary>
    /// Disconnect Web Cam message
    /// </summary>
    public class OnDisconnectWebCam : DashboardFormEvent
    {
        /// <summary>
        /// Initializes an instance of the OnDisconnectWebCam class
        /// </summary>
        /// <param name="form">The associated form</param>
        public OnDisconnectWebCam(DashboardForm form)
            : base(form)
        {
        }
    }

    /// <summary>
    /// Query Frame message
    /// </summary>
    public class OnQueryFrame : DashboardFormEvent
    {
        /// <summary>
        /// Initializes an instance of the OnQueryFrame class
        /// </summary>
        /// <param name="form">The associated form</param>
        public OnQueryFrame(DashboardForm form)
            : base(form)
        {
        }
    }
}