//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: Direction.cs $ $Revision: 4 $
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Security.Permissions;
using System.Windows.Forms;
using winforms = System.Windows.Forms;

namespace Microsoft.Robotics.Services.Sample.DirectionDialog
{
    /// <summary>
    /// Direction Form - Contains five buttons
    /// </summary>
    public partial class Direction : Form
    {
        #region Direction Form Initialization
        DirectionDialogOperations _mainPort;

        /// <summary>
        /// Constructor for Direction Form
        /// </summary>
        /// <param name="mainPort">Port to post messages back to</param>
        public Direction(DirectionDialogOperations mainPort)
        {
            _mainPort = mainPort;

            InitializeComponent();
        }
        #endregion

        #region Button Press Event
        private void button_MouseDown(object sender, MouseEventArgs e)
        {
            if (sender is winforms.Button)
            {
                winforms.Button button = (winforms.Button)sender;
                if (button.Name.StartsWith("btn"))
                {
                    string name = button.Name.Substring(3);

                    _mainPort.Post(new ButtonPress(new ButtonPressRequest(name)));
                }
            }
        }
        #endregion

        #region Button Release Event
        private void button_MouseUp(object sender, MouseEventArgs e)
        {
            if (sender is winforms.Button)
            {
                winforms.Button button = (winforms.Button)sender;
                if (button.Name.StartsWith("btn"))
                {
                    string name = button.Name.Substring(3);

                    _mainPort.Post(new ButtonRelease(new ButtonReleaseRequest(name)));
                }
            }
        }
        #endregion

        #region Keyboard Events
        private void Direction_KeyDown(object sender, KeyEventArgs e)
        {
            if (sender is winforms.Form)
            {
                string keyName = GetDirectionName(e.KeyData);
                if (keyName.Length > 0)
                {
                    _mainPort.Post(new ButtonPress(new ButtonPressRequest(keyName)));
                }
            }
        }

        private void Direction_KeyUp(object sender, KeyEventArgs e)
        {
            string keyName = GetDirectionName(e.KeyData);
            if (keyName.Length > 0)
            {
                _mainPort.Post(new ButtonRelease(new ButtonReleaseRequest(keyName)));
            }
        }
        #endregion

        #region Key Processing
        private string GetDirectionName(Keys key)
        {
            string name = string.Empty;

            switch (key)
            {
                case Keys.Up:
                case Keys.W:
                case Keys.NumPad8:
                    name = "Forwards";
                    break;
                case Keys.Right:
                case Keys.D:
                case Keys.NumPad6:
                    name = "Right";
                    break;
                case Keys.Down:
                case Keys.S:
                case Keys.NumPad2:
                    name = "Backwards";
                    break;
                case Keys.Left:
                case Keys.A:
                case Keys.NumPad4:
                    name = "Left";
                    break;
                case Keys.NumPad5:
                case Keys.Space:
                    name = "Stop";
                    break;
            }

            return name;
        }

        /// <summary>
        /// ProcessDialogKey - Override the normal key processing
        /// </summary>
        /// <param name="keyData">Keystroke to check</param>
        /// <returns></returns>
        [UIPermission(SecurityAction.LinkDemand, Window = UIPermissionWindow.AllWindows)]
        protected override bool ProcessDialogKey(Keys keyData)
        {
            // Allow arrow keys to be captured by the KeyDown event
            if (keyData == Keys.Up || keyData == Keys.Down ||
                keyData == Keys.Left || keyData == Keys.Right) return false;

            return base.ProcessDialogKey(keyData);
        }
        #endregion

    }
}