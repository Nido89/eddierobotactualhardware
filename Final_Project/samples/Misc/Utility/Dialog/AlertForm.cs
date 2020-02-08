//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: AlertForm.cs $ $Revision: 7 $
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Text;
using System.Windows.Forms;

using Microsoft.Ccr.Core;
using Microsoft.Robotics.Services.Sample.Properties;

using math = System.Math;

namespace Microsoft.Robotics.Services.Sample.Dialog
{
    /// <summary>
    /// AlertForm - Used for Alert dialogs
    /// </summary>
    public partial class AlertForm : Form
    {
        private SuccessFailurePort _result;

        /// <summary>
        /// SuccessFailurePort - Used when user clicks OK
        /// </summary>
        public SuccessFailurePort Result
        {
            get { return _result; }
        }

        /// <summary>
        /// Message - Message to display in dialog
        /// </summary>
        public string Message
        {
            get { return lblMessage.Text; }
            set { lblMessage.Text = value; }
        }

        /// <summary>
        /// Time remaining until this dialog times out
        /// </summary>
        public TimeSpan Countdown { get; set; }

        /// <summary>
        /// Gets a value specifying whether this dialog timed out.
        /// </summary>
        public bool Timeout { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public AlertForm()
            :this(new SuccessFailurePort())
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="result"></param>
        public AlertForm(SuccessFailurePort result)
        {
            _result = result;

            InitializeComponent();
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            Close();
        }

        /// <summary>
        /// Posts the appropriate response when handling the dialog being closed
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            _result.Post(SuccessResult.Instance);
        }

        private void timerCountdown_Tick(object sender, EventArgs e)
        {
            Countdown = Countdown - TimeSpan.FromMilliseconds(timerCountdown.Interval);

            if (Countdown.TotalSeconds <= 0)
            {
                Timeout = true;
                Close();
            }
            else if (Countdown.TotalSeconds < 16)
            {
                lblCountdown.Text = string.Format(
                    CultureInfo.InvariantCulture,
                    Resources.TimeoutWarning,
                    math.Floor(Countdown.TotalSeconds)
                );
            }
        }
    }
}