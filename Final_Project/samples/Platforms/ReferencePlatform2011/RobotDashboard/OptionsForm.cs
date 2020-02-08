//-----------------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples
//
//  <copyright file="OptionsForm.cs" company="Microsoft Corporation">
//      Copyright (C) Microsoft Corporation.  All rights reserved.
//  </copyright>
//
//  $File: OptionsForm.cs $ $Revision: 1 $
//-----------------------------------------------------------------------------

namespace Microsoft.Robotics.Services.RobotDashboard
{
    using System;
    using System.Windows.Forms;

    /// <summary>
    /// The Options Form
    /// </summary>
    public partial class OptionsForm : Form
    {
        /// <summary>
        /// Local copy of a reference to the Option settings
        /// </summary>
        /// <remarks>
        /// See RobotDashboardTypes.cs for the definition of
        /// the GUIOptions class.
        /// </remarks>
        private GUIOptions opt;

        /// <summary>
        /// Initializes an instance of the OptionsForm class
        /// </summary>
        /// <param name="options">Option settings</param>
        /// <remarks>Constructor takes a reference to the options so that
        /// we can return the results of changes made by the user</remarks>
        public OptionsForm(ref GUIOptions options)
        {
            this.InitializeComponent();
            this.opt = options;
        }

        /// <summary>
        /// Handle Form Load
        /// </summary>
        /// <param name="sender">The parameter is not used.</param>
        /// <param name="e">The parameter is not used.</param>
        /// <remarks>Display all of the current option settings when the
        /// form is loaded</remarks>
        private void OptionsFormLoad(object sender, EventArgs e)
        {
            this.txtDeadZoneX.Text = this.opt.DeadZoneX.ToString();
            this.txtDeadZoneY.Text = this.opt.DeadZoneY.ToString();
            this.txtTranslateScaleFactor.Text = this.opt.TranslateScaleFactor.ToString();
            this.txtRotateScaleFactor.Text = this.opt.RotateScaleFactor.ToString();
            this.txtCameraInterval.Text = this.opt.CameraInterval.ToString();
        }

        /// <summary>
        /// Handle OK button
        /// </summary>
        /// <param name="sender">The parameter is not used.</param>
        /// <param name="e">The parameter is not used.</param>
        private void OKButtonClick(object sender, EventArgs e)
        {
            double dX, dY, trans, rot;
            int interval;
            string err = string.Empty;

            // Set some defaults
            // This is just to keep the compiler happy - the values
            // will be overwritten, but the compiler does not understand
            // the try/catch blocks and says the variables are not
            // assigned a value ...
            dX = dY = 80;
            trans = 1.0;
            rot = 0.5;
            interval = 250;

            try
            {
                dX = Math.Abs(double.Parse(this.txtDeadZoneX.Text));
            }
            catch
            {
                err += "Enter a number for Dead Zone X\n";
            }

            try
            {
                dY = Math.Abs(double.Parse(this.txtDeadZoneY.Text));
            }
            catch
            {
                err += "Enter a number for Dead Zone X\n";
            }

            try
            {
                trans = double.Parse(this.txtTranslateScaleFactor.Text);
            }
            catch
            {
                err += "Enter a number for TranslateScaleFactor\n";
            }

            try
            {
                rot = double.Parse(this.txtRotateScaleFactor.Text);
            }
            catch
            {
                err += "Enter a number for RotateScaleFactor\n";
            }

            try
            {
                interval = int.Parse(this.txtCameraInterval.Text);
                if (interval <= 0)
                {
                    err += "Interval must be greater than zero\n";
                }
            }
            catch
            {
                err += "Enter a number for Camera Interval (in milliseconds)\n";
            }

            // TT - Version 3
            // Allow negative values
            // This might seem strange, but it allows the axes to be
            // flipped on the trackball which some people might find
            // more convenient.
            if (trans == 0)
            {
                err += "Translate Scale Factor must not be zero\n";
            }

            if (rot == 0)
            {
                err += "Rotate Scale Factor must not be zero\n";
            }

            // If any of the tests above generated an error message,
            // then display it now and don't make the changes
            if (err != string.Empty)
            {
                MessageBox.Show(err, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            // Set the new values
            this.opt.DeadZoneX = dX;
            this.opt.DeadZoneY = dY;
            this.opt.TranslateScaleFactor = trans;
            this.opt.RotateScaleFactor = rot;
            this.opt.CameraInterval = interval;

            // Set our return result
            this.DialogResult = DialogResult.OK;

            // Close down and return to caller
            this.Close();
        }

        /// <summary>
        /// Handle Cancel button
        /// </summary>
        /// <param name="sender">The parameter is not used.</param>
        /// <param name="e">The parameter is not used.</param>
        private void CancelButtonClick(object sender, EventArgs e)
        {
            // Just set the result and die quietly
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}