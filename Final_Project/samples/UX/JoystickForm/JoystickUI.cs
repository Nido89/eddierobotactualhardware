//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: JoystickUI.cs $ $Revision: 3 $
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;
using System.Windows.Forms;

using gc = Microsoft.Robotics.Services.GameController.Proxy;

namespace Microsoft.Robotics.Services.Sample.JoystickForm
{
    /// <summary>
    /// JoystickUI - Form for displaying the joystick
    /// </summary>
    public partial class JoystickUI : Form
    {
        gc.GameControllerOperations _fwdPort;
        bool[] _buttonState = new bool[10];
        gc.Axes _axes = new gc.Axes();
        bool _decay = true;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="fwdPort"></param>
        public JoystickUI(gc.GameControllerOperations fwdPort)
        {
            _fwdPort = fwdPort;

            InitializeComponent();

            DrawJoystick(0, 0);
        }

        /// <summary>
        /// OnClosed - Signal to the main service that the form is closing
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            _fwdPort.DsspDefaultDrop();
        }

        #region Buttons

        private void button_MouseDown(object sender, MouseEventArgs e)
        {
            HandleButton(sender, true);
        }

        private void HandleButton(object sender, bool down)
        {
            Button button = sender as Button;

            if (button == null)
            {
                return;
            }

            int number;

            if (int.TryParse(button.Text, out number))
            {
                if (number >= 0 && number <= 9)
                {
                    if (chkSticky.Checked)
                    {
                        if (down)
                        {
                            return;
                        }

                        bool pressed = false;
                        if (button.Tag is bool)
                        {
                            pressed = (bool)button.Tag;
                        }

                        button.Tag = !pressed;
                        down = !pressed;
                        _buttonState[number] = !pressed;
                    }

                    using (Graphics g = Graphics.FromHwnd(button.Handle))
                    {
                        if (down)
                        {
                            button.BackColor = SystemColors.ControlDarkDark;
                            button.ForeColor = SystemColors.ControlLightLight;
                        }
                        else
                        {
                            button.BackColor = SystemColors.Control;
                            button.ForeColor = SystemColors.ControlText;
                        }
                    }

                    _buttonState[number] = down;

                    gc.Buttons request = new gc.Buttons();
                    request.TimeStamp = DateTime.UtcNow;
                    request.Pressed = new List<bool>(_buttonState);
                    _fwdPort.UpdateButtons(request);
                }
            }
        }

        private void button_MouseUp(object sender, MouseEventArgs e)
        {
            HandleButton(sender, false);
        }

        private void chkSticky_CheckedChanged(object sender, EventArgs e)
        {
            bool sticky = chkSticky.Checked;

            if (!sticky)
            {
                bool update = false;

                foreach (Control control in Controls)
                {
                    Button button = control as Button;
                    if (button != null && button.Tag is bool)
                    {
                        int number;

                        if (int.TryParse(button.Text, out number))
                        {
                            bool pressed = (bool)button.Tag;

                            if (pressed)
                            {
                                button.BackColor = SystemColors.Control;
                                button.ForeColor = SystemColors.ControlText;
                                update = true;
                            }
                            _buttonState[number] = false;
                        }

                        button.Tag = false;
                    }
                }

                if (update)
                {
                    gc.Buttons request = new gc.Buttons();
                    request.TimeStamp = DateTime.UtcNow;
                    request.Pressed = new List<bool>(_buttonState);
                    _fwdPort.UpdateButtons(request);
                }
            }
        }

        #endregion

        #region Axes

        private void picJoystick_MouseLeave(object sender, EventArgs e)
        {
            _decay = true;
        }

        private void picJoystick_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                int x, y;
                x = Math.Min(picJoystick.Width, Math.Max(e.X, 0));
                y = Math.Min(picJoystick.Height, Math.Max(e.Y, 0));

                x = x * 2000 / picJoystick.Width - 1000;
                y = y * 2000 / picJoystick.Height - 1000;

                _axes.X = x;
                _axes.Y = y;

                _decay = false;
                UpdateJoystickAxes();
            }
        }

        void UpdateJoystickAxes()
        {
            DrawJoystick(_axes.X, -_axes.Y);
            lblX.Text = "X: " + _axes.X;
            lblY.Text = "Y: " + _axes.Y;

            _axes.TimeStamp = DateTime.UtcNow;
            _fwdPort.UpdateAxes(_axes);
        }

        private void picJoystick_MouseUp(object sender, MouseEventArgs e)
        {
            picJoystick_MouseLeave(sender, e);
        }

        private void DrawJoystick(int x, int y)
        {
            Bitmap bmp = new Bitmap(picJoystick.Width, picJoystick.Height);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                int width = bmp.Width - 1;
                int height = bmp.Height - 1;

                g.Clear(Color.Transparent);
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                GraphicsPath path = new GraphicsPath();
                path.AddEllipse(0, 0, width, height);

                PathGradientBrush pathBrush = new PathGradientBrush(path);
                pathBrush.CenterPoint = new PointF(width / 3f, height / 3f);
                pathBrush.CenterColor = SystemColors.ButtonHighlight;
                pathBrush.SurroundColors = new Color[] { SystemColors.ButtonShadow };

                g.FillPath(pathBrush, path);
                g.DrawPath(SystemPens.ControlText, path);

                int partial = y * height / 2200;
                if (partial > 0)
                {
                    g.DrawArc(SystemPens.ControlText,
                        0,
                        height / 2 - partial,
                        width,
                        2 * partial,
                        180,
                        180);
                }
                else if (partial == 0)
                {
                    g.DrawLine(SystemPens.ControlText, 0, height / 2, width, height / 2);
                }
                else
                {
                    g.DrawArc(SystemPens.ControlText,
                        0,
                        height / 2 + partial,
                        width,
                        -2 * partial,
                        0,
                        180);
                }

                partial = x * width / 2200;
                if (partial > 0)
                {
                    g.DrawArc(SystemPens.ControlText,
                        width / 2 - partial,
                        0,
                        2 * partial,
                        height,
                        270,
                        180);
                }
                else if (partial == 0)
                {
                    g.DrawLine(SystemPens.ControlText, width / 2, 0, width / 2, height);
                }
                else
                {
                    g.DrawArc(SystemPens.ControlText,
                        width / 2 + partial,
                        0,
                        -2 * partial,
                        height,
                        90,
                        180);
                }
            }


            Image old = picJoystick.Image;
            picJoystick.Image = bmp;

            if (old != null)
            {
                old.Dispose();
            }
        }

        private void decayTimer_Tick(object sender, EventArgs e)
        {
            if (_decay)
            {
                if (_axes.X == 0 && _axes.Y == 0 &&
                    _deltaX == 0 && _deltaY == 0)
                {
                    return;
                }
                else
                {
                    int spring = (int)(100 + Math.Sqrt(_axes.X * _axes.X + _axes.Y * _axes.Y) / 10);

                    if (_deltaX != 0)
                    {
                        _axes.X = Math.Min(Math.Max(-1000, _axes.X + _deltaX), 1000);
                    }
                    else if (_axes.X > 0)
                    {
                        _axes.X = Math.Max(0, _axes.X - spring);
                    }
                    else
                    {
                        _axes.X = Math.Min(0, _axes.X + spring);
                    }

                    if (_deltaY != 0)
                    {
                        _axes.Y = Math.Min(Math.Max(-1000, _axes.Y + _deltaY), 1000);
                    }
                    else if (_axes.Y > 0)
                    {
                        _axes.Y = Math.Max(0, _axes.Y - spring);
                    }
                    else
                    {
                        _axes.Y = Math.Min(0, _axes.Y + spring);
                    }
                    UpdateJoystickAxes();
                }
            }
        }
 
        #endregion

        int _deltaX = 0;
        int _deltaY = 0;

        private void JoystickUI_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.S:
                    _deltaY = 100;
                    return;

                case Keys.A:
                    _deltaX = -100;
                    return;

                case Keys.D:
                    _deltaX = 100;
                    return;

                case Keys.W:
                    _deltaY = -100;
                    return;
            }

            Button button = FindButtonFromKey(e.KeyCode);

            if (button != null)
            {
                button_MouseDown(button, new MouseEventArgs(MouseButtons.None, 1, button.Width / 2, button.Height / 2, 0));
            }
        }

        private void JoystickUI_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.S:
                case Keys.W:
                    _deltaY = 0;
                    return;

                case Keys.A:
                case Keys.D:
                    _deltaX = 0;
                    return;
            }

            Button button = FindButtonFromKey(e.KeyCode);

            if (button != null)
            {
                button_MouseUp(button, new MouseEventArgs(MouseButtons.None, 1, button.Width / 2, button.Height / 2, 0));
            }
        }


        Button FindButtonFromKey(Keys keyCode)
        {
            int number = 0;

            switch (keyCode)
            {
                case Keys.D0:
                case Keys.NumPad0:
                    number = 0;
                    break;
                case Keys.D1:
                case Keys.NumPad1:
                    number = 1;
                    break;
                case Keys.D2:
                case Keys.NumPad2:
                    number = 2;
                    break;
                case Keys.D3:
                case Keys.NumPad3:
                    number = 3;
                    break;
                case Keys.D4:
                case Keys.NumPad4:
                    number = 4;
                    break;
                case Keys.D5:
                case Keys.NumPad5:
                    number = 5;
                    break;
                case Keys.D6:
                case Keys.NumPad6:
                    number = 6;
                    break;
                case Keys.D7:
                case Keys.NumPad7:
                    number = 7;
                    break;
                case Keys.D8:
                case Keys.NumPad8:
                    number = 8;
                    break;
                case Keys.D9:
                case Keys.NumPad9:
                    number = 9;
                    break;
                default:
                    return null;
            }

            Control[] found = Controls.Find("button" + number, true);

            if (found.Length == 1)
            {
                return found[0] as Button;
            }
            return null;
        }
    }
}