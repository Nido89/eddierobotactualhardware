//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
// 
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: EmbeddedSimUI.cs $ $Revision: 5 $
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using xnaTypes = Microsoft.Xna.Framework;
using Microsoft.Robotics.Simulation.Engine;
using Microsoft.Robotics.PhysicalModel;

namespace Microsoft.Simulation.Embedded
{
    public partial class EmbeddedSimUI : Form
    {
        FromWinformEvents _fromWinformPort;

        public EmbeddedSimUI(FromWinformEvents EventsPort)
        {
            _fromWinformPort = EventsPort;

            InitializeComponent();
            _cameraImage.MouseMove += new MouseEventHandler(_cameraImage_MouseMove);
            _cameraImage.MouseDown += new MouseEventHandler(_cameraImage_MouseDown);
            _fromWinformPort.Post(new FromWinformMsg(FromWinformMsg.MsgEnum.Loaded, null, this));
        }

        Vector2 _mouseDown;
        void _cameraImage_MouseDown(object sender, MouseEventArgs e)
        {
            _mouseDown = new Vector2(e.X, e.Y);
        }

        void _cameraImage_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _fromWinformPort.Post(new FromWinformMsg(FromWinformMsg.MsgEnum.Drag, null, new Vector2(e.X - _mouseDown.X, e.Y - _mouseDown.Y)));
                _mouseDown = new Vector2(e.X, e.Y);
            }
            else if (e.Button == MouseButtons.Right)
            {
                _fromWinformPort.Post(new FromWinformMsg(FromWinformMsg.MsgEnum.Zoom, null, new Vector2(e.X - _mouseDown.X, e.Y - _mouseDown.Y)));
                _mouseDown = new Vector2(e.X, e.Y);
            }
        }
        public void SetHeadless(bool headless)
        {
            checkBox1.Checked = headless;
            this.Refresh();
        }

        public void SetCameraImage(Bitmap bmp)
        {
            if (_cameraImage.Image != null)
            {
                _cameraImage.Image.Dispose();
            }
            _cameraImage.Image = bmp;
            _cameraImage.Invalidate();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            SimulatorConfiguration config = new SimulatorConfiguration(true);
            config.Headless = checkBox1.Checked;
            SimulationEngine.GlobalInstancePort.Update(config);
        }

    }
}