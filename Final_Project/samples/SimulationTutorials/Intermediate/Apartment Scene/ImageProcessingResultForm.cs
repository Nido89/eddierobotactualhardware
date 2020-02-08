//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: ImageProcessingResultForm.cs $ $Revision: 5 $
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Imaging;

namespace Microsoft.Robotics.Services.Samples
{
    public partial class ImageProcessingResultForm : Form
    {
        #region WinForm members
        public Bitmap ImageResultBitmap { get; set; }
        public PictureBox PictureBox { get { return _formPicture; } }
        public long TimeStamp { get; set; }
        #endregion

        public ImageProcessingResultForm()
        {
            InitializeComponent();

            #region Initialize WinForm members
            ImageResultBitmap = new Bitmap(_formPicture.Width, _formPicture.Height);
            _formPicture.Image = ImageResultBitmap;
            #endregion
        }

        #region UpdateForm
        internal void UpdateForm(Bitmap bmp)
        {
            ImageResultBitmap = bmp;
            this.Size = new Size(bmp.Width, bmp.Height);
            _formPicture.Size = new Size(bmp.Width, bmp.Height);
            _formPicture.Image = ImageResultBitmap;
            this.Invalidate(true);
        }
        #endregion
    }
}
