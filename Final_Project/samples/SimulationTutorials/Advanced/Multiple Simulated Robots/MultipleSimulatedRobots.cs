//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: MultipleSimulatedRobots.cs $ $Revision: 5 $
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
using webcam = Microsoft.Robotics.Services.WebCam.Proxy;
using drive = Microsoft.Robotics.Services.Drive.Proxy;
using Microsoft.Dss.Core;
using System.Drawing;
using Microsoft.Ccr.Adapters.WinForms;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using Microsoft.Robotics.Simulation.Engine;

using lbr3arm = Microsoft.Robotics.Services.Simulation.LBR3Arm.Proxy;
using Microsoft.Robotics.PhysicalModel;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Robotics.Simulation.Physics;
using System.Diagnostics;

namespace Robotics.MultipleSimulatedRobots
{
    [Contract(Contract.Identifier)]
    [DisplayName("(User) Multiple Simulated Robots")]
    [Description("Shows how to add multiple robots to a scene")]
    class MultipleSimulatedRobotsService : DsspServiceBase
    {
        /// <summary>
        /// Service state
        /// </summary>
        [ServiceState]
        MultipleSimulatedRobotsState _state = new MultipleSimulatedRobotsState();

        /// <summary>
        /// Main service port
        /// </summary>
        [ServicePort("/MultipleSimulatedRobots", AllowMultipleInstances = true)]
        MultipleSimulatedRobotsOperations _mainPort = new MultipleSimulatedRobotsOperations();

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
        /// Port for first webcam
        /// </summary>
        [Partner("SimulatedWebcam_1", Contract = webcam.Contract.Identifier, CreationPolicy = PartnerCreationPolicy.UsePartnerListEntry)]
        webcam.WebCamOperations _firstWebcamPort = new webcam.WebCamOperations();

        #region Second webcam partner
        /// <summary>
        /// Port for second webcam
        /// </summary>
        [Partner("SimulatedWebcam_2", Contract = webcam.Contract.Identifier, CreationPolicy = PartnerCreationPolicy.UsePartnerListEntry)]
        webcam.WebCamOperations _secondWebcamPort = new webcam.WebCamOperations();
        #endregion

        #endregion

        /// <summary>
        /// Service constructor
        /// </summary>
        public MultipleSimulatedRobotsService(DsspServiceCreationPort creationPort)
            : base(creationPort)
        {
        }


        #region Data Members
        Port<DateTime>[] _timers = new[]{ new Port<DateTime>(), new Port<DateTime>() };

        Stopwatch _stopWatch = new Stopwatch();

        // used to display gradient we compute
        ImageProcessingResultForm[] _webcamForms = new ImageProcessingResultForm[2];
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

            #region Run WinForms and Activate Timers
            _stopWatch.Start();

            var webcamPorts = new[] { _firstWebcamPort, _secondWebcamPort };
            for (int idx = 0; idx < _webcamForms.Length; ++idx)
            {
                int i = idx;
                WinFormsServicePort.Post(new RunForm(() =>
                {
                    _webcamForms[i] = new ImageProcessingResultForm();
                    _webcamForms[i].Show();

                    Activate(Arbiter.Receive(false, _timers[i], dateTime => UpdateWebcam(webcamPorts[i],
                    _webcamForms[i], _timers[i])));
                    TaskQueue.EnqueueTimer(TimeSpan.FromMilliseconds(60), _timers[i]);

                    return _webcamForms[i];
                }));
            }
            #endregion

        }

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
            TaskQueue.Dispose();

            DefaultDropHandler(drop);
            yield break;
        }

        #region Update Image
        #region Query Webcam
        IEnumerator<ITask> UpdateWebcamHelper(webcam.WebCamOperations webcamPort, ImageProcessingResultForm webcamForm,
            Port<DateTime> timerPort)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            yield return new IterativeTask(() => UpdateImage(webcamPort, webcamForm));

            Activate(Arbiter.Receive(false, timerPort, dateTime => UpdateWebcam(webcamPort,
                webcamForm, timerPort)));
            TaskQueue.EnqueueTimer(TimeSpan.FromMilliseconds(Math.Max(1, 60 - stopwatch.ElapsedMilliseconds)), timerPort);

            yield break;
        }
        void UpdateWebcam(webcam.WebCamOperations webcamPort, ImageProcessingResultForm webcamForm,
            Port<DateTime> timerPort)
        {
            SpawnIterator<webcam.WebCamOperations, ImageProcessingResultForm, Port<DateTime>>(
                webcamPort, webcamForm, timerPort, UpdateWebcamHelper);
        }

        IEnumerator<ITask> UpdateImage(webcam.WebCamOperations webcamPort, ImageProcessingResultForm webcamForm)
        {
            long timestamp = _stopWatch.ElapsedMilliseconds;

            byte[] rgbData = null;
            Size size = new Size(0, 0);

            yield return Arbiter.Choice(webcamPort.QueryFrame(),
                success =>
                {
                    rgbData = success.Frame;
                    size = success.Size;
                },
                failure =>
                {
                    LogError(failure.ToException());
                });

            TaskQueue.Enqueue(new Task(() =>
                {
                    if (rgbData != null)
                    {
                        ComputeGradient(ref rgbData, size);
                        UpdateBitmap(rgbData, size, webcamForm, timestamp);
                    }
                }));
        }
        #endregion

        private void UpdateBitmap(byte[] rgbData, Size size, ImageProcessingResultForm webcamForm,
            long timestamp)
        {
            if (webcamForm == null)
                return;

            WinFormsServicePort.Post(new FormInvoke(() =>
            {
                if (timestamp > webcamForm.TimeStamp)
                {
                    webcamForm.TimeStamp = timestamp;

                    Bitmap bmp = webcamForm.ImageResultBitmap;
                    CopyBytesToBitmap(rgbData, size.Width, size.Height, ref bmp);
                    if (bmp != webcamForm.ImageResultBitmap)
                    {
                        webcamForm.UpdateForm(bmp);
                    }
                    webcamForm.Invalidate(true);
                }
            }));
        }
        #endregion

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

    }
}


