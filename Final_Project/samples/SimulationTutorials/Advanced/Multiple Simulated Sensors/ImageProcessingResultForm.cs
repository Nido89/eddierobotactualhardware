//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: ImageProcessingResultForm.cs $ $Revision: 7 $
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Imaging;
using sicklrf = Microsoft.Robotics.Services.Sensors.SickLRF.Proxy;
using Microsoft.Robotics.Simulation.Engine;

namespace Robotics.MultipleSimulatedSensors
{
    public partial class ImageProcessingResultForm : Form
    {
        #region WinForm data members
        public Bitmap WebcamBitmap { get; set; }
        public PictureBox PictureBox { get { return _webcamPanel; } }

        Bitmap LRFBitmap { get; set; }
        Bitmap SonarBitmap { get; set; }
        #endregion

        public ImageProcessingResultForm()
        {
            InitializeComponent();

            #region Initializing WinForm data members
            WebcamBitmap = new Bitmap(_webcamPanel.Width, _webcamPanel.Height);
            _webcamPanel.Image = WebcamBitmap;

            LRFBitmap = new Bitmap(_lrfPanel.Width, _lrfPanel.Height, PixelFormat.Format32bppRgb);
            _lrfPanel.Image = LRFBitmap;
            #endregion
        }


        private void ImageProcessingResultForm_Load(object sender, EventArgs e)
        {
            UpdateSensorGridValues();
        }

        #region WinForm Update Sensor Data
        internal void UpdateWebcamImage(Bitmap bmp)
        {
            WebcamBitmap = bmp;
            _webcamPanel.Size = new Size(bmp.Width, bmp.Height);
            _webcamPanel.Image = WebcamBitmap;
            this.Invalidate(true);
        }

        double _brightnessSensorValue;
        Color _colorSensorValue = new Color();
        double _compassSensorValue;
        double _infraredDistanceReading;
        double _sonarDistanceReading;

        /// <summary>
        /// Updates the property grid on the form with the current sensor values
        /// </summary>
        private void UpdateSensorGridValues()
        {
            _sensorGrid.SelectedObject = new
            {
                Color = _colorSensorValue,
                NormalizedBrightness = _brightnessSensorValue,
                Orientation = _compassSensorValue,
                IRDistanceReading = _infraredDistanceReading,
                SonarDistanceReading = _sonarDistanceReading
            };
            _sensorGrid.Invalidate(true);
        }

        internal void SetBrightnessReadingValue(double brightness)
        {
            _brightnessSensorValue = brightness;
            UpdateSensorGridValues();
        }
        internal void SetColorReadingValue(double r, double g, double b)
        {
            _colorSensorValue = Color.FromArgb(ClampToByte(r*255), ClampToByte(g*255), ClampToByte(b*255));
            UpdateSensorGridValues();
        }

        private byte ClampToByte(double x)
        {
            return (byte)Math.Min(255.0, Math.Max(0.0, x));
        }
        internal void SetCompassReadingValue(double orientation)
        {
            _compassSensorValue = orientation;
            UpdateSensorGridValues();
        }

        internal void SetIRReadingValue(double distance)
        {
            _infraredDistanceReading = distance;
            UpdateSensorGridValues();
        }

        internal void SetSonarReadingValue(double distance)
        {
            _sonarDistanceReading = distance;
            UpdateSensorGridValues();
        }

        double _previousLrfMax = 0.1;
        /// <summary>
        /// Copies 'distanceMeasurements' paramater to Bitmap contained in the _lrfPanel
        /// </summary>
        /// <param name="distanceMeasurements"></param>
        /// <param name="units"></param>
        internal void SetLRFData(int[] distanceMeasurements, sicklrf.Units units)
        {
            if (_lrfPanel == null || distanceMeasurements==null)
                return;

            double lrfMax = 0;
            int maxIndex = distanceMeasurements.Length - 1;
            float scalingFactor = maxIndex / (float)(_lrfPanel.Width - 1);

            byte[] bmpData = new byte[4*_lrfPanel.Width * _lrfPanel.Height];
            for (int y = 0; y < _lrfPanel.Height; y++)
            {
                for (int x = 0; x < _lrfPanel.Width; x++)
                {
                    int i = 4*(y*_lrfPanel.Width + x);
                    int originalDistance = distanceMeasurements[maxIndex - (int)(x * scalingFactor)];
                    lrfMax = Math.Max(originalDistance, lrfMax);

                    double distance = (255.0/_previousLrfMax) * originalDistance;
                    bmpData[i] = ClampToByte(distance);
                }
            }
            _previousLrfMax = Math.Max(lrfMax, double.Epsilon);

            Bitmap bmp = LRFBitmap;
            MultipleSimulatedSensorsService.CopyBytesToBitmap(bmpData, _lrfPanel.Width, _lrfPanel.Height, ref bmp);
            if (bmp != LRFBitmap)
            {
                LRFBitmap = bmp;
                _lrfPanel.Image = LRFBitmap;
            }
        }

        #endregion
    }
}
