//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: ApartmentScene.cs $ $Revision: 5 $
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Ccr.Core;
using Microsoft.Dss.Core.Attributes;
using Microsoft.Dss.ServiceModel.Dssp;
using Microsoft.Dss.ServiceModel.DsspServiceBase;
using W3C.Soap;
using submgr = Microsoft.Dss.Services.SubscriptionManager;
using engine = Microsoft.Robotics.Simulation.Engine.Proxy;
using simulatedwebcam = Microsoft.Robotics.Services.Simulation.Sensors.SimulatedWebcam.Proxy;
using webcam = Microsoft.Robotics.Services.WebCam.Proxy;
using simulateddrive = Microsoft.Robotics.Services.Simulation.Drive.Proxy;
using drive = Microsoft.Robotics.Services.Drive.Proxy;

#region Using Statements
using Microsoft.Dss.Core;
using System.Drawing;
using Microsoft.Ccr.Adapters.WinForms;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using Microsoft.Robotics.Services.Samples;
using Microsoft.Robotics.Simulation.Engine;
#endregion

namespace Robotics.ApartmentScene
{
    [Contract(Contract.Identifier)]
    [DisplayName("(User) Apartment Scene")]
    [Description("Shows how to perform basic image processing on an image from the simulated webcam service in an apartment environment")]
    class ApartmentSceneService : DsspServiceBase
    {
        /// <summary>
        /// Service state
        /// </summary>
        [ServiceState]
        ApartmentSceneState _state = new ApartmentSceneState();

        /// <summary>
        /// Main service port
        /// </summary>
        [ServicePort("/ApartmentScene", AllowMultipleInstances = true)]
        ApartmentSceneOperations _mainPort = new ApartmentSceneOperations();

        [SubscriptionManagerPartner]
        submgr.SubscriptionManagerPort _submgrPort = new submgr.SubscriptionManagerPort();

        #region Partners
        /// <summary>
        /// SimulationEngine partner
        /// </summary>
        [Partner("SimulationEngine", Contract = engine.Contract.Identifier, CreationPolicy = PartnerCreationPolicy.UseExistingOrCreate)]
        engine.SimulationEnginePort _simulationEnginePort = new engine.SimulationEnginePort();
        engine.SimulationEnginePort _simulationEngineNotify = new engine.SimulationEnginePort();

        /// <summary>
        /// SimulatedWebcamService partner
        /// </summary>
        [Partner("SimulatedWebcamService", Contract = webcam.Contract.Identifier, CreationPolicy = PartnerCreationPolicy.UsePartnerListEntry)]
        webcam.WebCamOperations _simulatedWebcamServicePort = new webcam.WebCamOperations();

        /// <summary>
        /// SimulatedDifferentialDriveService partner
        /// </summary>
        [Partner("SimulatedDifferentialDriveService", Contract = drive.Contract.Identifier, CreationPolicy = PartnerCreationPolicy.UsePartnerListEntry)]
        drive.DriveOperations _simulatedDifferentialDriveServicePort = new drive.DriveOperations();

        #endregion

        /// <summary>
        /// Service constructor
        /// </summary>
        public ApartmentSceneService(DsspServiceCreationPort creationPort)
            : base(creationPort)
        {
        }

        #region Data Members
        Port<DateTime> _dateTimePort = new Port<DateTime>();

        // used to display gradient we compute
        ImageProcessingResultForm _imageProcessingForm;
        #endregion

        /// <summary>
        /// Service start
        /// </summary>
        protected override void Start()
        {

            // 
            // Add service specific initialization here
            // 

            base.Start();

            #region Activate Timer
            Activate(Arbiter.ReceiveWithIterator(false, _dateTimePort, UpdateImage));
            TaskQueue.EnqueueTimer(TimeSpan.FromMilliseconds(60), _dateTimePort);
            #endregion

            #region Run Win Form
            WinFormsServicePort.Post(new RunForm(() =>
            {
                _imageProcessingForm = new ImageProcessingResultForm();
                _imageProcessingForm.Show();
                return _imageProcessingForm;
            }));
            #endregion
        }

        #region Update Image
        IEnumerator<ITask> UpdateImage(DateTime dateTime)
        {
            byte[] rgbData = null;
            Size size = new Size(0, 0);

            yield return Arbiter.Choice(_simulatedWebcamServicePort.QueryFrame(),
                success =>
                {
                    rgbData = success.Frame;
                    size = success.Size;
                },
                failure =>
                {
                    LogError(failure.ToException());
                });

            if (rgbData != null)
            {
                ComputeGradient(ref rgbData, size);
                UpdateBitmap(rgbData, size);
            }

            Activate(Arbiter.ReceiveWithIterator(false, _dateTimePort, UpdateImage));
            TaskQueue.EnqueueTimer(TimeSpan.FromMilliseconds(60), _dateTimePort);
        }
        #endregion

        #region Additional Methods
        private void UpdateBitmap(byte[] rgbData, Size size)
        {
            if (_imageProcessingForm == null)
                return;

            #region Running code on the WinForms thread
            WinFormsServicePort.Post(new FormInvoke(() =>
                {
                    Bitmap bmp = _imageProcessingForm.ImageResultBitmap;
                    CopyBytesToBitmap(rgbData, size.Width, size.Height, ref bmp);
                    if (bmp != _imageProcessingForm.ImageResultBitmap)
                    {
                        _imageProcessingForm.UpdateForm(bmp);
                    }
                    _imageProcessingForm.Invalidate(true);
                }));
            #endregion
        }


        #region Basic Image Processing Methods
        private void ComputeGradient(ref byte[] rgbData, Size size)
        {
            byte[] gradient = new byte[rgbData.Length];
            int[,] mask = new[,]
                {
                    {+2, +1, 0},
                    {+1, 0, -1},
                    {0, -1, -2}
                };
            const int filterSize = 3;
            const int halfFilterSize = filterSize / 2;

            //convolve use simple n^2 method, but this can easily be made 2n
            for (int y = halfFilterSize; y < size.Height - halfFilterSize; y++)
            {
                for (int x = halfFilterSize; x < size.Width - halfFilterSize; x++)
                {
                    float result = 0;
                    for (int yy = -halfFilterSize; yy <= halfFilterSize; ++yy)
                    {
                        int y0 = yy + y;
                        for (int xx = -halfFilterSize; xx <= halfFilterSize; ++xx)
                        {
                            int x0 = xx + x;
                            int k = mask[yy + halfFilterSize, xx + halfFilterSize];
                            int i = 3 * (y0 * size.Width + x0);
                            int r = rgbData[i];
                            int g = rgbData[i + 1];
                            int b = rgbData[i + 2];
                            result += k * (r + g + b) / (3.0f);
                        }
                    }
                    result /= 4.0f; // normalize by max value 
                    //the "result*5" makes edges more visible in the image, but is not really necessary
                    //  (only nice for display purposes)
                    byte byteResult = Clamp(Math.Abs(result * 5.0f), 0.0f, 255.0f);
                    int idx = 3 * (y * size.Width + x);
                    gradient[idx] = byteResult;
                    gradient[idx + 1] = byteResult;
                    gradient[idx + 2] = byteResult;
                }
            }

            rgbData = gradient;
        }

        private byte Clamp(float x, float min, float max)
        {
            return (byte)Math.Min(Math.Max(min, x), max);
        }


        /// <summary>
        /// Updates a bitmap from a byte array
        /// </summary>
        /// <param name="srcData">Should be 32 or 24 bits per pixel (ARGB or RGB format)</param>
        /// <param name="srcDataWidth">Width of the image srcData represents</param>
        /// <param name="srcDataHeight">Height of the image srcData represents</param>
        /// <param name="destBitmap">Bitmap to copy to. Will be recreated if necessary to copy to the array.</param>
        private void CopyBytesToBitmap(byte[] srcData, int srcDataWidth, int srcDataHeight, ref Bitmap destBitmap)
        {
            int bytesPerPixel = srcData.Length / (srcDataWidth * srcDataHeight);
            if (destBitmap == null
                || destBitmap.Width != srcDataWidth
                || destBitmap.Height != srcDataHeight
                || (destBitmap.PixelFormat == PixelFormat.Format32bppArgb && bytesPerPixel == 3)
                || (destBitmap.PixelFormat == PixelFormat.Format32bppRgb && bytesPerPixel == 3)
                || (destBitmap.PixelFormat == PixelFormat.Format24bppRgb && bytesPerPixel == 4))
            {
                if (bytesPerPixel == 3)
                    destBitmap = new Bitmap(srcDataWidth, srcDataHeight, PixelFormat.Format24bppRgb);
                else
                    destBitmap = new Bitmap(srcDataWidth, srcDataHeight, PixelFormat.Format32bppRgb);
            }
            BitmapData bmpData = null;
            try
            {
                if (bytesPerPixel == 3)
                    bmpData = destBitmap.LockBits(new Rectangle(0, 0, srcDataWidth, srcDataHeight), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
                else
                    bmpData = destBitmap.LockBits(new Rectangle(0, 0, srcDataWidth, srcDataHeight), ImageLockMode.WriteOnly, PixelFormat.Format32bppRgb);

                Marshal.Copy(srcData, 0, bmpData.Scan0, srcData.Length);
                destBitmap.UnlockBits(bmpData);
            }
            catch (Exception)
            {
            }
        }
        #endregion
        #endregion

        /// <summary>
        /// Handles Subscribe messages
        /// </summary>
        /// <param name="subscribe">the subscribe request</param>
        [ServiceHandler]
        public void SubscribeHandler(Subscribe subscribe)
        {
            SubscribeHelper(_submgrPort, subscribe.Body, subscribe.ResponsePort);
        }

        /// <summary>
        /// Handles Drop Messages
        /// </summary>
        /// <param name="drop"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Teardown)]
        public IEnumerator<ITask> DropHandler(DsspDefaultDrop drop)
        {
            // cancel any active tasks that are still active
            TaskQueue.Dispose();

            DefaultDropHandler(drop);
            yield break;
        }
    }
}


