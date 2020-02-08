//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: VisualizationForm.cs $ $Revision: 4 $
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using bt = Microsoft.Robotics.Services.Sample.BlobTracker.Proxy;
using Microsoft.Dss.ServiceModel.Dssp;
using Microsoft.Ccr.Core;

namespace Microsoft.Robotics.Services.Sample.BlobTrackerCalibrate
{
    /// <summary>
    /// BlobTracker Visualization Form
    /// </summary>
    public partial class VisualizationForm : Form
    {
        Port<Shutdown> _shutdownPort;
        bt.BlobTrackerOperations _blobTrackerPort;

        /// <summary>
        /// Creates a new VisualizationForm with the specified Shutdown port.
        /// </summary>
        /// <param name="shutdownPort">Shutdown port</param>
        /// <param name="blobTrackerPort">BlobTrackerOperations portset</param>
        public VisualizationForm(Port<Shutdown> shutdownPort, bt.BlobTrackerOperations blobTrackerPort)
        {
            InitializeComponent();

            _shutdownPort = shutdownPort;
            _blobTrackerPort = blobTrackerPort;
        }

        private Bitmap _cameraImage;

        /// <summary>
        /// Specifies a image frame captured by the camera.
        /// </summary>
        public Bitmap CameraImage
        {
            get { return _cameraImage; }
            set
            {
                _cameraImage = value;

                Image old = picCamera.Image;
                picCamera.Image = value;

                if (old != null)
                {
                    old.Dispose();
                }
            }
        }

        private List<bt.FoundBlob> _tracking;

        internal List<bt.FoundBlob> Tracking
        {
            get { return _tracking; }
            set { _tracking = value; }
        }

        bool _capturing;
        Point _center;
        int _radius;
        bool _ready = false;

        private void picCamera_MouseDown(object sender, MouseEventArgs e)
        {
            _capturing = true;
            _center = new Point(e.X, e.Y);
        }

        private void picCamera_MouseMove(object sender, MouseEventArgs e)
        {
            if (_capturing)
            {
                int dx = e.X - _center.X;
                int dy = e.Y - _center.Y;

                _radius = (int)Math.Round(
                    Math.Sqrt(dx * dx + dy * dy)
                );

                picCamera.Invalidate();
            }
        }

        private void picCamera_MouseUp(object sender, MouseEventArgs e)
        {
            if (_capturing)
            {
                int dx = e.X - _center.X;
                int dy = e.Y - _center.Y;

                _radius = (int)Math.Round(
                    Math.Sqrt(dx * dx + dy * dy)
                );

                _capturing = false;
                _ready = true;

                picCamera.Invalidate();
            }
        }

        private void picCamera_Paint(object sender, PaintEventArgs e)
        {
            if (_capturing || _ready)
            {
                e.Graphics.DrawEllipse(
                    Pens.Black,
                    _center.X - _radius,
                    _center.Y - _radius,
                    _radius * 2,
                    _radius * 2
                );
                e.Graphics.DrawEllipse(
                    Pens.White,
                    _center.X - _radius - 1,
                    _center.Y - _radius - 1,
                    _radius * 2 + 2,
                    _radius * 2 + 2
                );
            }

            if (_tracking != null && _tracking.Count > 0)
            {
                foreach (bt.FoundBlob blob in _tracking)
                {
                    if (blob.Area < 16)
                    {
                        continue;
                    }

                    double width, height;

                    width = 3 * blob.StdDevX;
                    height = 3 * blob.StdDevY;

                    Rectangle rect = new Rectangle(
                        (int)(blob.MeanX - width / 2),
                        (int)(blob.MeanY - height / 2),
                        (int)width,
                        (int)height
                    );

                    e.Graphics.DrawRectangle(Pens.White, rect);
                    rect.Inflate(1, 1);
                    e.Graphics.DrawRectangle(Pens.Black, rect);

                    e.Graphics.DrawString(blob.Name, Font, Brushes.Black, rect.Left, rect.Top);
                }
            }
        }

        private void btnTrain_Click(object sender, EventArgs e)
        {
            string name = txtName.Text;
            if (string.IsNullOrEmpty(name) || _radius == 0 || !_ready)
            {
                return;
            }

            Bitmap bmp = picCamera.Image as Bitmap;
            if (bmp == null)
            {
                return;
            }

            int accumRed = 0;
            int accumGreen = 0;
            int accumBlue = 0;
            int count = 0;

            int[] redProjection = new int[256];
            int[] greenProjection = new int[256];
            int[] blueProjection = new int[256];

            for (int y = _center.Y - _radius; y <= _center.Y + _radius; y++)
            {
                if (y < 0 || y >= bmp.Height)
                {
                    continue;
                }

                for (int x = _center.X - _radius; x <= _center.X + _radius; x++)
                {
                    if (x < 0 || x >= bmp.Width)
                    {
                        continue;
                    }

                    int dx = _center.X - x;
                    int dy = _center.Y - y;

                    if (dx * dx + dy * dy > _radius)
                    {
                        continue;
                    }

                    Color color = bmp.GetPixel(x,y);

                    count++;
                    accumRed += color.R;
                    accumGreen += color.G;
                    accumBlue += color.B;

                    redProjection[color.R]++;
                    greenProjection[color.G]++;
                    blueProjection[color.B]++;
                }
            }

            double meanRed = (double)accumRed / count;
            double meanGreen = (double)accumGreen / count;
            double meanBlue = (double)accumBlue / count;

            double redDev = 1 + CalculateDeviation(meanRed, redProjection);
            double greenDev = 1 + CalculateDeviation(meanGreen, greenProjection);
            double blueDev = 1 + CalculateDeviation(meanBlue, blueProjection);

            bt.ColorBin bin = new bt.ColorBin();
            bin.Name = name;

            bin.RedMin = (int)Math.Round(meanRed - redDev);
            bin.RedMax = (int)Math.Round(meanRed + redDev);
            bin.GreenMin = (int)Math.Round(meanGreen - greenDev);
            bin.GreenMax = (int)Math.Round(meanGreen + greenDev);
            bin.BlueMin = (int)Math.Round(meanBlue - blueDev);
            bin.BlueMax = (int)Math.Round(meanBlue + blueDev);

            if (_tracking != null &&
                _tracking.Exists(
                    delegate(bt.FoundBlob test)
                    {
                        return test.Name == name;
                    })
                )
            {
                _blobTrackerPort.UpdateBin(bin);
            }
            else
            {
                _blobTrackerPort.InsertBin(bin);
            }
            _ready = false;
        }

        private double CalculateDeviation(double mean, int[] projection)
        {
            int count = 0;
            double variance = 0;

            for (int i = 0; i < projection.Length; i++)
            {
                if (projection[i] > 0)
                {
                    double offset = i - mean;

                    variance += offset * offset * projection[i];
                    count += projection[i];
                }
            }

            if (count == 0)
            {
                return 0;
            }

            return Math.Sqrt(variance / count);
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            string name = txtName.Text;
            if (string.IsNullOrEmpty(name))
            {
                return;
            }

            bt.ColorBin bin = new bt.ColorBin();
            bin.Name = name;

            _blobTrackerPort.DeleteBin(bin);
        }

        /// <summary>
        /// OnClosed event
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            _shutdownPort.Post(new Shutdown());
        }

    }
}