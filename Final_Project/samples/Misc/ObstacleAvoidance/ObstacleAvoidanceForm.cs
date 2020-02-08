//------------------------------------------------------------------------------
//  <copyright file="ObstacleAvoidanceForm.cs" company="Microsoft Corporation">
//      Copyright (C) Microsoft Corporation.  All rights reserved.
//  </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Robotics.Services.ObstacleAvoidanceDrive
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using System.Drawing;
    using System.Linq;
    using System.Text;
    using System.Windows.Forms;
    using Microsoft.Ccr.Core;
    using Microsoft.Robotics.Services.ObstacleAvoidanceDrive;

    using joystick = Microsoft.Robotics.Services.GameController.Proxy;
    using webcam = Microsoft.Robotics.Services.WebCam.Proxy;

    /// <summary>
    /// The main ObstacleAvoidance Form
    /// </summary>
    public partial class ObstacleAvoidanceForm : Form
    {
        /// <summary>
        /// Hold the current active PID index
        /// </summary>
        private int currentPIDSelection;

        /// <summary>
        /// Holds all PID controllers in the form
        /// </summary>
        private List<Microsoft.Ccr.Core.Tuple<NumericUpDown, Control>> pidControllersControl;

        /// <summary>
        /// The port for sending events
        /// </summary>
        private ObstacleAvoidanceFormEvents eventsPort;

        /// <summary>
        /// Initializes a new instance of the DashboardForm class
        /// </summary>
        /// <param name="theEventsPort">The Events Port for passing events back to the service</param>
        /// <param name="state">The service state</param>
        public ObstacleAvoidanceForm(ObstacleAvoidanceFormEvents theEventsPort, ObstacleAvoidanceDriveState state)
        {
            this.eventsPort = theEventsPort;

            this.InitializeComponent();

            this.UpdatePIDControllersValue(
                state.Controller.Kp,
                state.Controller.Ki,
                state.Controller.Kd);

            this.currentPIDSelection = 0;
            this.pidControllersControl = new List<Ccr.Core.Tuple<NumericUpDown, Control>>()
            {
                new Ccr.Core.Tuple<NumericUpDown, Control>(this.KpNumeric, this.KpIndicator),
                new Ccr.Core.Tuple<NumericUpDown, Control>(this.KiNumeric, this.KiIndicator),
                new Ccr.Core.Tuple<NumericUpDown, Control>(this.KdNumeric, this.KdIndicator)
            };

            this.pidControllersControl[this.currentPIDSelection].Item1.Visible = true;
        }

        /// <summary>
        /// Handle Form Load
        /// </summary>
        /// <param name="sender">The parameter is not used.</param>
        /// <param name="e">The parameter is not used.</param>
        private void ObstacleAvoidanceForm_Load(object sender, EventArgs e)
        {
            this.eventsPort.Post(new OnLoad(this));
        }

        /// <summary>
        /// Handle Form Closed
        /// </summary>
        /// <param name="sender">The parameter is not used.</param>
        /// <param name="e">The parameter is not used.</param>
        private void ObstacleAvoidanceForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.eventsPort.Post(new OnClosed(this));
        }

        /// <summary>
        /// Handle PID increment value change
        /// </summary>
        /// <param name="sender">Sender of the event</param>
        /// <param name="e">Event arguments</param>
        private void IncrementNumeric_ValueChanged(object sender, EventArgs e)
        {
            this.KpNumeric.Increment = this.incrementNumeric.Value;
            this.KiNumeric.Increment = this.incrementNumeric.Value;
            this.KdNumeric.Increment = this.incrementNumeric.Value;
        }

        /// <summary>
        /// Update PID controller parameter values on the form.
        /// This is a one-off call during constructor where we need to 
        /// set the initial state of the PID controllers
        /// </summary>
        /// <param name="kp">Proportional constant</param>
        /// <param name="ki">Integral constant</param>
        /// <param name="kd">Derivative constant</param>
        public void UpdatePIDControllersValue(double kp, double ki, double kd)
        {
            // Set all the PID values on the form
            this.KpNumeric.Text = kp.ToString("F2");
            this.KiNumeric.Text = ki.ToString("F2");
            this.KdNumeric.Text = kd.ToString("F2");
        }

        /// <summary>
        /// Set new PID controller values
        /// </summary>
        public void PostPIDControllersValue()
        {
            this.eventsPort.Post(new OnPIDChanges(this, double.Parse(this.KpNumeric.Text), double.Parse(this.KiNumeric.Text), double.Parse(this.KdNumeric.Text)));
        }

        /// <summary>
        /// Handle the joystick buttons
        /// </summary>
        /// <param name="buttons">The current state of all the Joytick buttons</param>
        public void UpdateJoystickButtons(joystick.Buttons buttons)
        {
            if (buttons.Pressed != null && buttons.Pressed.Count >= 8)
            {
                if (buttons.Pressed[4])
                {
                    // RB shoulder button -> Increase PID parameter value
                    this.pidControllersControl[this.currentPIDSelection].Item0.DownButton();
                }
                else if (buttons.Pressed[5])
                {
                    // LB shoulder button -> Decrease PID parameter value
                    this.pidControllersControl[this.currentPIDSelection].Item0.UpButton();
                }
                else if (buttons.Pressed[6])
                {
                    // Back button -> Switch active PID parameter, to the left
                    this.pidControllersControl[this.currentPIDSelection].Item1.Visible = false;
                    this.currentPIDSelection = (this.currentPIDSelection == 0) ? (this.pidControllersControl.Count - 1) : (this.currentPIDSelection - 1);
                    this.pidControllersControl[this.currentPIDSelection].Item1.Visible = true;
                }
                else if (buttons.Pressed[7])
                {
                    // Start button -> Switch active PID parameter, to the right
                    this.pidControllersControl[this.currentPIDSelection].Item1.Visible = false;
                    this.currentPIDSelection = (this.currentPIDSelection + 1) % this.pidControllersControl.Count;
                    this.pidControllersControl[this.currentPIDSelection].Item1.Visible = true;
                }
            }
        }

        /// <summary>
        /// Handle value changes on PID controls
        /// </summary>
        /// <param name="sender">Sender of the event</param>
        /// <param name="e">Event arguments</param>
        private void PIDControllers_ValueChanged(object sender, EventArgs e)
        {
            this.PostPIDControllersValue();
        }

        /// <summary>
        /// A bitmap to hold the depth profile image
        /// </summary>
        private Bitmap depthProfileImage;

        /// <summary>
        /// Gets or sets the Depth Profile Image
        /// </summary>
        /// <remarks>Provides external access for updating the depth profile image</remarks>
        public Bitmap DepthProfileImage
        {
            get
            {
                return this.depthProfileImage;
            }

            set
            {
                this.depthProfileImage = value;

                Image old = this.depthProfileCtrl.Image;
                this.depthProfileCtrl.Image = value;

                // Dispose of the old bitmap to save memory
                // (It will be garbage collected eventually, but this is faster)
                if (old != null)
                {
                    old.Dispose();
                }
            }
        }
    }

    /// <summary>
    /// Operations Port for ObstacleAvoidance Events
    /// </summary>
    public class ObstacleAvoidanceFormEvents :
        PortSet<OnLoad,
            OnClosed,
            OnQueryFrame,
            OnPIDChanges>
    {
    }

    /// <summary>
    /// Class used for events sent by the ObstacleAvoidance Form back to the service
    /// </summary>
    public class ObstacleAvoidanceFormEvent
    {
        /// <summary>
        ///  Obstacle Avoidance Form
        /// </summary>
        private ObstacleAvoidanceForm obstacleAvoidanceForm;

        /// <summary>
        /// Gets or sets the associated Form
        /// </summary>
        public ObstacleAvoidanceForm ObstacleAvoidanceForm
        {
            get { return this.obstacleAvoidanceForm; }
            set { this.obstacleAvoidanceForm = value; }
        }

        /// <summary>
        /// Initializes an instance of the ObstacleAvoidanceFormEvent class
        /// </summary>
        /// <param name="obstacleAvoidanceForm">The associated Form</param>
        public ObstacleAvoidanceFormEvent(ObstacleAvoidanceForm obstacleAvoidanceForm)
        {
            this.obstacleAvoidanceForm = obstacleAvoidanceForm;
        }
    }

    /// <summary>
    /// Form Loaded message
    /// </summary>
    public class OnLoad : ObstacleAvoidanceFormEvent
    {
        /// <summary>
        /// Initializes an instance of the OnLoad class
        /// </summary>
        /// <param name="form">The associated Form</param>
        public OnLoad(ObstacleAvoidanceForm form)
            : base(form)
        {
        }
    }

    /// <summary>
    /// Form Closed message
    /// </summary>
    public class OnClosed : ObstacleAvoidanceFormEvent
    {
        /// <summary>
        /// Initializes an instance of the OnClosed class
        /// </summary>
        /// <param name="form">The associated Form</param>
        public OnClosed(ObstacleAvoidanceForm form)
            : base(form)
        {
        }
    }

    /// <summary>
    /// Query Frame message
    /// </summary>
    public class OnQueryFrame : ObstacleAvoidanceFormEvent
    {
        /// <summary>
        /// Initializes an instance of the OnQueryFrame class
        /// </summary>
        /// <param name="form">The associated form</param>
        public OnQueryFrame(ObstacleAvoidanceForm form)
            : base(form)
        {
        }
    }

    /// <summary>
    /// PID parameter values changes
    /// </summary>
    public class OnPIDChanges : ObstacleAvoidanceFormEvent
    {
        /// <summary>
        /// The Proportional constant
        /// </summary>
        private double kp;

        /// <summary>
        /// Gets or sets the Proportional constant
        /// </summary>
        public double Kp
        {
            get { return this.kp; }
            set { this.kp = value; }
        }

        /// <summary>
        /// The Integral constant
        /// </summary>
        private double ki;

        /// <summary>
        /// Gets or sets the Proportional constant
        /// </summary>
        public double Ki
        {
            get { return this.ki; }
            set { this.ki = value; }
        }

        /// <summary>
        /// The Derivative constant
        /// </summary>
        private double kd;

        /// <summary>
        /// Gets or sets the Derivative constant
        /// </summary>
        public double Kd
        {
            get { return this.kd; }
            set { this.kd = value; }
        }

        /// <summary>
        /// Initializes an instance of the OnPIDChanges class
        /// </summary>
        /// <param name="form">The associated form</param>
        /// <param name="kp">The proportional constant</param>
        /// <param name="ki">The integral constant</param>
        /// <param name="kd">The derivative constant</param>
        public OnPIDChanges(ObstacleAvoidanceForm form, double kp, double ki, double kd)
            : base(form)
        {
            this.kp = kp;
            this.ki = ki;
            this.kd = kd;
        }
    }
}
