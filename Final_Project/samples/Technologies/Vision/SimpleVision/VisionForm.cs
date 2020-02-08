//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
// 
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: VisionForm.cs $ $Revision: 2 $
//----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;
using System.Windows.Forms;
using Microsoft.Dss.ServiceModel.Dssp;


namespace Microsoft.Robotics.Services.Sample.SimpleVision
{
    partial class VisionForm : Form
    {
        SimpleVisionOperations _mainPort;
        ImageProcessingResult _result;

        private Bitmap _cameraImage;

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

        private Bitmap _testImage;

        public Bitmap TestImage
        {
            get { return _testImage; }
            set
            {
                _testImage = value;

                Image old = picTest.Image;
                picTest.Image = value;

                if (old != null)
                {
                    old.Dispose();
                }
            }
        }

        private Bitmap _testImage2;

        public Bitmap TestImage2
        {
            get { return _testImage2; }
            set
            {
                _testImage2 = value;

                Image old = picTest2.Image;
                picTest2.Image = value;

                if (old != null)
                {
                    old.Dispose();
                }
            }
        }

        private Bitmap _faceImage;

        public Bitmap FaceImage
        {
            get { return _faceImage; }
            set
            {
                _faceImage = value;

                Image old = picFace.Image;
                picFace.Image = value;

                if (old != null)
                {
                    old.Dispose();
                }
            }
        }

        public VisionForm(SimpleVisionOperations mainPort, ImageProcessingResult result)
        {
            InitializeComponent();

            _mainPort = mainPort;
            _result = result;

        }

        private void VisionForm_KeyDown(object sender, KeyEventArgs e)
        {
        }

        private void VisionForm_KeyPress(object sender, KeyPressEventArgs e)
        {
        }

        private void VisionForm_KeyUp(object sender, KeyEventArgs e)
        {
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
            double width, height;

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

            if (_result.ObjectFound)
            {
                width = 3 * _result.XStdDev;
                height = 3 * _result.YStdDev;

                Rectangle rect = new Rectangle(
                    (int)(_result.XMean - width / 2),
                    (int)(_result.YMean - height / 2),
                    (int)width,
                    (int)height
                );

                e.Graphics.DrawRectangle(Pens.White, rect);
                rect.Inflate(1, 1);
                e.Graphics.DrawRectangle(Pens.Black, rect);
            }

            // Draw center of motion
            if (_result.MotionFound)
            {
                width = height = 15;
                Rectangle rect = new Rectangle(
                    (int)(_result.XMotion - width / 2),
                    (int)(_result.YMotion - height / 2),
                    (int)width,
                    (int)height
                );
                e.Graphics.DrawRectangle(Pens.Red, rect);
            }

            if (_result.LeftHandX > 0)
            {
                width = height = 15;
                Rectangle rect = new Rectangle(
                    (int)(_result.LeftHandX - width / 2),
                    (int)(_result.LeftHandY - height / 2),
                    (int)width,
                    (int)height
                );
                e.Graphics.DrawRectangle(Pens.Orange, rect);

            }
            if (_result.RightHandX > 0)
            {
                width = height = 15;
                Rectangle rect = new Rectangle(
                    (int)(_result.RightHandX - width / 2),
                    (int)(_result.RightHandY - height / 2),
                    (int)width,
                    (int)height
                );
                e.Graphics.DrawRectangle(Pens.Orange, rect);

            }

            if (_result.HeadFound)
            {
                Rectangle rect = new Rectangle(
                    _result.HeadBoxRegion.Sx,
                    _result.HeadBoxRegion.Sy,
                    _result.HeadBoxRegion.Ex - _result.HeadBoxRegion.Sx,
                    _result.HeadBoxRegion.Ey - _result.HeadBoxRegion.Sy
                );
                e.Graphics.DrawRectangle(Pens.Yellow, rect);
                rect.Inflate(1, 1);
                e.Graphics.DrawRectangle(Pens.Yellow, rect);
            }

            if (_result.HeadFound || _result.ObjectFound)
            {
                if (_result.LeftHandGestureFound && _result.RightHandGestureFound)
                    handGesture.Text = "BOTH";
                else if (_result.LeftHandGestureFound)
                    handGesture.Text = "LEFT";
                else if (_result.RightHandGestureFound)
                    handGesture.Text = "RIGHT";
                else
                    handGesture.Text = "NONE";
            }
            else
                handGesture.Text = "NONE";

            for (int i = 0; i < _result.numberOfLocations; i++)
            {
                Rectangle rect = new Rectangle(
                    _result.boundBox[i].Sx,
                    _result.boundBox[i].Sy,
                    _result.boundBox[i].Ex - _result.boundBox[i].Sx,
                    _result.boundBox[i].Ey - _result.boundBox[i].Sy
                );
                e.Graphics.DrawRectangle(Pens.YellowGreen, rect);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (_radius == 0 || !_ready)
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

                    Color color = bmp.GetPixel(x, y);

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

            int RedMin = (int)Math.Round(meanRed - redDev);
            int RedMax = (int)Math.Round(meanRed + redDev);
            int GreenMin = (int)Math.Round(meanGreen - greenDev);
            int GreenMax = (int)Math.Round(meanGreen + greenDev);
            int BlueMin = (int)Math.Round(meanBlue - blueDev);
            int BlueMax = (int)Math.Round(meanBlue + blueDev);

            rRange.Text = "R = " + RedMin.ToString() + "," + RedMax.ToString();
            gRange.Text = "G = " + GreenMin.ToString() + "," + GreenMax.ToString();
            bRange.Text = "B = " + BlueMin.ToString() + "," + BlueMax.ToString();

            double sum = meanRed + meanGreen + meanBlue;
            if (sum == 0) return;
            double nR = (double)meanRed / (double)sum;
            double nG = (double)meanGreen / (double)sum;
            double nB = (double)meanBlue / (double)sum;

            normR.Text = "nR = " + nR.ToString();
            normG.Text = "nG = " + nG.ToString();
            normB.Text = "nB = " + nB.ToString();


            double SimilarityMeasure = Convert.ToDouble(textSmilarity.Text);

            if (SimilarityMeasure == 0) SimilarityMeasure = 0.995;
            _mainPort.Post(new SetObjectTrackingColor(new ColorVector(nR, nG, nB, SimilarityMeasure)));

        }

        private static double CalculateDeviation(double mean, int[] projection)
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

        private void button2_Click(object sender, EventArgs e)
        {
        }
        private void button3_Click(object sender, EventArgs e)
        {
        }

        private void button4_Click(object sender, EventArgs e)
        {
            //red
            _mainPort.Post(new SetObjectTrackingColor(new ColorVector(0.6, 0.2, 0.15)));
        }

        private void button5_Click(object sender, EventArgs e)
        {
            //blue
            _mainPort.Post(new SetObjectTrackingColor(new ColorVector(0.17, 0.35, 0.47)));
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            _mainPort.Post(new DsspDefaultDrop());
        }

    }

}