//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: BlobTrackerCalibrate.cs $ $Revision: 5 $
//-----------------------------------------------------------------------
using Microsoft.Ccr.Core;
using Microsoft.Dss.Core;
using Microsoft.Dss.Core.Attributes;
using Microsoft.Dss.ServiceModel.Dssp;
using Microsoft.Dss.ServiceModel.DsspServiceBase;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml;
using bt = Microsoft.Robotics.Services.Sample.BlobTracker.Proxy;
using cam = Microsoft.Robotics.Services.WebCam.Proxy;
using W3C.Soap;
using Microsoft.Ccr.Adapters.WinForms;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;


namespace Microsoft.Robotics.Services.Sample.BlobTrackerCalibrate
{

    /// <summary>
    /// Implementation class for BlobTrackerCalibrate
    /// </summary>
    [DisplayName("(User) Blob Tracker Calibrate")]
    [Description("Provides a simple interface for training the Blob Tracker service.")]
    [Contract(Contract.Identifier)]
    [DssServiceDescription("http://msdn.microsoft.com/library/cc998457.aspx")]
    public class BlobTrackerCalibrateService : DsspServiceBase
    {
        /// <summary>
        /// _state
        /// </summary>
        private BlobTrackerCalibrateState _state = new BlobTrackerCalibrateState();
        /// <summary>
        /// _main Port
        /// </summary>
        [ServicePort("/blobtrackercalibrate", AllowMultipleInstances=false)]
        private BlobTrackerCalibrateOperations _mainPort = new BlobTrackerCalibrateOperations();

        [Partner("blobtracker", Contract = bt.Contract.Identifier, CreationPolicy = PartnerCreationPolicy.UseExisting)]
        private bt.BlobTrackerOperations _blobTrackerPort = new bt.BlobTrackerOperations();
        private bt.BlobTrackerOperations _blobTrackerNotify = new bt.BlobTrackerOperations();

        private cam.WebCamOperations _webCamPort;
        private cam.WebCamOperations _webCamNotify = new cam.WebCamOperations();

        Port<Shutdown> _shutdownPort = new Port<Shutdown>();
        VisualizationForm _form;

        /// <summary>
        /// Default Service Constructor
        /// </summary>
        public BlobTrackerCalibrateService(DsspServiceCreationPort creationPort) :
                base(creationPort)
        {
        }

        /// <summary>
        /// Service Start
        /// </summary>
        protected override void Start()
        {
            base.Start();

            SpawnIterator(InitializeCamera);
        }

        IEnumerator<ITask> InitializeCamera()
        {
            ServiceInfoType info = null;
            Fault fault = null;

            yield return Arbiter.Choice(
                _blobTrackerPort.DsspDefaultLookup(),
                delegate(LookupResponse success)
                {
                    info = success;
                },
                delegate(Fault f)
                {
                    fault = f;
                }
            );

            if (fault != null)
            {
                LogError(null, "Lookup failed on BlobTracker partner", fault);
                yield break;
            }

            PartnerType camera = FindPartner(
                new XmlQualifiedName("WebCam", bt.Contract.Identifier),
                info.PartnerList
            );

            if (camera == null ||
                string.IsNullOrEmpty(camera.Service))
            {
                LogError("No camera partner found for BlobTracker");
                yield break;
            }

            _webCamPort = ServiceForwarder<cam.WebCamOperations>(camera.Service);

            yield return Arbiter.Choice(
                _webCamPort.Subscribe(_webCamNotify, typeof(cam.UpdateFrame)),
                delegate(SubscribeResponseType success) { },
                delegate(Fault f)
                {
                    fault = f;
                }
            );

            if (fault != null)
            {
                LogError(null, "Failed to subscribe to webcam", fault);
                yield break;
            }

            yield return Arbiter.Choice(
                _blobTrackerPort.Subscribe(_blobTrackerNotify),
                delegate(SubscribeResponseType success) { },
                delegate(Fault f)
                {
                    fault = f;
                }
            );

            if (fault != null)
            {
                LogError(null, "Failed to subscribe to blob tracker", fault);
                yield break;
            }

            RunForm runForm = new RunForm(CreateVisualization);

            WinFormsServicePort.Post(runForm);

            yield return Arbiter.Choice(
                runForm.pResult,
                delegate(SuccessResult success) { },
                delegate(Exception e)
                {
                    fault = Fault.FromException(e);
                }
            );

            if (fault != null)
            {
                LogError(null, "Failed to Create Visualization window", fault);
                yield break;
            }

            base.MainPortInterleave.CombineWith(
                Arbiter.Interleave(
                    new TeardownReceiverGroup(),
                    new ExclusiveReceiverGroup(
                        Arbiter.Receive<cam.UpdateFrame>(true, _webCamNotify, CameraUpdateFrameHandler),
                        Arbiter.Receive<bt.ImageProcessed>(true, _blobTrackerNotify, BlobTrackerImageProcessedHandler),
                        Arbiter.Receive<Shutdown>(true, _shutdownPort, ShutdownHandler)
                    ),
                    new ConcurrentReceiverGroup(
                        Arbiter.Receive<bt.DeleteBin>(true, _blobTrackerNotify, EmptyHandler),
                        Arbiter.Receive<bt.InsertBin>(true, _blobTrackerNotify, EmptyHandler),
                        Arbiter.Receive<bt.UpdateBin>(true, _blobTrackerNotify, EmptyHandler)
                    )
                )
            );
        }


        Form CreateVisualization()
        {
            _form = new VisualizationForm(_shutdownPort, _blobTrackerPort);
            return _form;
        }

        /// <summary>
        /// Get Handler
        /// </summary>
        /// <param name="get"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public void GetHandler(Get get)
        {
            get.ResponsePort.Post(_state);
        }

        /// <summary>
        /// OnUpdateProcessing Handler
        /// </summary>
        /// <param name="update"></param>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public void OnUpdateProcessing(UpdateProcessing update)
        {
            if (_state.Shutdown)
            {
                _state.Processing = true;
                _mainPort.Post(new DsspDefaultDrop());
            }
            else
            {
                _state.Processing = update.Body.Processing;
            }
            update.ResponsePort.Post(DefaultUpdateResponseType.Instance);

        }

        void ShutdownHandler(Shutdown shutdown)
        {
            _state.Shutdown = true;

            if (_state.Processing)
            {
                return;
            }
            else
            {
                _mainPort.Post(new DsspDefaultDrop());
            }
        }

        void CameraUpdateFrameHandler(cam.UpdateFrame update)
        {
            if (_state.Processing || _state.Shutdown)
            {
                return;
            }

            _state.Processing = true;

            SpawnIterator(update, DoCameraUpdateFrameHandler);
        }


        IEnumerator<ITask> DoCameraUpdateFrameHandler(cam.UpdateFrame update)
        {
            try
            {
                cam.QueryFrameResponse frame = null;
                Fault fault = null;

                yield return Arbiter.Choice(
                    _webCamPort.QueryFrame(new cam.QueryFrameRequest()),
                    delegate(cam.QueryFrameResponse success)
                    {
                        frame = success;
                    },
                    delegate(Fault f)
                    {
                        fault = f;
                    }
                );

                if (fault != null)
                {
                    LogError(null, "Failed to get frame from camera", fault);
                    yield break;
                }

                Bitmap bmp = new Bitmap(
                    frame.Size.Width,
                    frame.Size.Height,
                    PixelFormat.Format24bppRgb
                );

                BitmapData data = bmp.LockBits(
                    new Rectangle(0, 0, bmp.Width, bmp.Height),
                    ImageLockMode.WriteOnly,
                    PixelFormat.Format24bppRgb
                );

                Marshal.Copy(frame.Frame, 0, data.Scan0, frame.Frame.Length);

                bmp.UnlockBits(data);

                FormInvoke setImage = new FormInvoke(
                    delegate()
                    {
                        if (!_form.IsDisposed)
                        {
                            _form.CameraImage = bmp;
                        }
                    }
                );

                WinFormsServicePort.Post(setImage);

                Arbiter.Choice(
                    setImage.ResultPort,
                    delegate(EmptyValue success) { },
                    delegate(Exception e)
                    {
                        fault = Fault.FromException(e);
                    }
                );

                if (fault != null)
                {
                    LogError(null, "Unable to set camera image on form", fault);
                    yield break;
                }
            }
            finally
            {
                _mainPort.Post(new UpdateProcessing(false));
            }
        }

        void BlobTrackerImageProcessedHandler(bt.ImageProcessed processed)
        {
            if (_state.Processing || _state.Shutdown)
            {
                return;
            }

            _state.Processing = true;

            SpawnIterator(processed, DoBlobTrackerImageProcessedHandler);
        }

        IEnumerator<ITask> DoBlobTrackerImageProcessedHandler(bt.ImageProcessed processed)
        {
            try
            {
                Fault fault = null;

                FormInvoke setTracking = new FormInvoke(
                    delegate
                    {
                        if (!_form.IsDisposed)
                        {
                            _form.Tracking = processed.Body.Results;
                        }
                    }
                );

                WinFormsServicePort.Post(setTracking);

                yield return Arbiter.Choice(
                    setTracking.ResultPort,
                    delegate(EmptyValue success) { },
                    delegate(Exception e)
                    {
                        fault = Fault.FromException(e);
                    }
                );

                if (fault != null)
                {
                    LogError(null, "Unable to set tracking information", fault);
                    yield break;
                }
            }
            finally
            {
                _mainPort.Post(new UpdateProcessing(false));
            }
        }
    }
}
