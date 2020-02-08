//-----------------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples
//
//  <copyright file="DepthCamForm.cs" company="Microsoft Corporation">
//      Copyright (C) Microsoft Corporation.  All rights reserved.
//  </copyright>
//
//  Form for displaying DepthCam video
//
//  $File: DepthCamForm.cs $ $Revision: 1 $
//-----------------------------------------------------------------------------

namespace Microsoft.Robotics.Services.RobotDashboard
{
    using System;
    using System.Drawing;
    using System.Windows.Forms;

    /// <summary>
    /// DepthCamForm - Form for displaying DepthCam video
    /// </summary>
    public partial class DepthCamForm : Form
    {
        /// <summary>
        /// Port for communicating with the Dashboard service
        /// </summary>
        private RobotDashboardOperations mainPort;

        /// <summary>
        /// Initializes a new instance of the DepthCamForm class
        /// </summary>
        /// <param name="port">The main Robot Dashboard operations port</param>
        /// <param name="startX">Initial X position</param>
        /// <param name="startY">Initial Y position</param>
        /// <param name="width">Initial Width</param>
        /// <param name="height">Initial Height</param>
        public DepthCamForm(RobotDashboardOperations port, int startX, int startY, int width, int height)
        {
            this.InitializeComponent();      // Required for a Windows Form
            this.mainPort = port;

            this.StartPosition = FormStartPosition.Manual;
            this.Location = new Point(startX, startY);
            this.Size = new Size(width, height);
        }

        /// <summary>
        /// A bitmap to hold the camera image
        /// </summary>
        private Bitmap cameraImage;

        /// <summary>
        /// Gets or sets the Camera Image
        /// </summary>
        /// <remarks>Provides external access for updating the camera image</remarks>
        public Bitmap CameraImage
        {
            get
            {
                return this.cameraImage;
            }

            set
            {
                this.cameraImage = value;

                Image old = this.picCamera.Image;
                this.picCamera.Image = value;

                // Dispose of the old bitmap to save memory
                // (It will be garbage collected eventually, but this is faster)
                if (old != null)
                {
                    old.Dispose();
                }
            }
        }
    }
}