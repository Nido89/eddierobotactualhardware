//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: BlobTracker.cs $ $Revision: 5 $
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Xml;

using Microsoft.Ccr.Core;
using Microsoft.Dss.Core;
using Microsoft.Dss.Core.Attributes;
using Microsoft.Dss.ServiceModel.Dssp;
using Microsoft.Dss.ServiceModel.DsspServiceBase;
using W3C.Soap;

using sm = Microsoft.Dss.Services.SubscriptionManager;
using cam = Microsoft.Robotics.Services.WebCam.Proxy;
using System.Runtime.InteropServices;

namespace Microsoft.Robotics.Services.Sample.BlobTracker
{

    /// <summary>
    /// Implementation class for BlobTracker
    /// </summary>
    [DisplayName("(User) Blob Tracker")]
    [Description("Finds specific blobs (regions) within an image for simple color tracking.")]
    [Contract(Contract.Identifier)]
    [DssServiceDescription("http://msdn.microsoft.com/library/cc998456.aspx")]
    public class BlobTrackerService : DsspServiceBase
    {
        /// <summary>
        /// _state
        /// </summary>
        [InitialStatePartner(Optional = true, ServiceUri = ServicePaths.Store + "/BlobTracker.config.xml")]
        private BlobTrackerState _state = new BlobTrackerState();

        /// <summary>
        /// _main Port
        /// </summary>
        [ServicePort("/blobtracker", AllowMultipleInstances=false)]
        private BlobTrackerOperations _mainPort = new BlobTrackerOperations();

        [Partner("SubMgr", Contract = sm.Contract.Identifier, CreationPolicy = PartnerCreationPolicy.CreateAlways)]
        sm.SubscriptionManagerPort _subMgrPort = new sm.SubscriptionManagerPort();

        [Partner("WebCam", Contract = cam.Contract.Identifier, CreationPolicy = PartnerCreationPolicy.UseExisting)]
        cam.WebCamOperations _camPort = new cam.WebCamOperations();
        cam.WebCamOperations _camNotify = new cam.WebCamOperations();

        /// <summary>
        /// Default Service Constructor
        /// </summary>
        public BlobTrackerService(DsspServiceCreationPort creationPort) :
                base(creationPort)
        {
        }

        /// <summary>
        /// Service Start
        /// </summary>
        protected override void Start()
        {
            if (_state == null)
            {
                _state = new BlobTrackerState();
            }

            base.Start();

            base.MainPortInterleave.CombineWith(
                Arbiter.Interleave(
                    new TeardownReceiverGroup(),
                    new ExclusiveReceiverGroup(
                        Arbiter.Receive<cam.UpdateFrame>(true, _camNotify, OnCameraUpdateFrame)
                    ),
                    new ConcurrentReceiverGroup()
                )
            );

            Activate(
                Arbiter.Receive(false, TimeoutPort(5000), StartTimer)
            );
        }

        void StartTimer(DateTime signal)
        {
            //_camPort = ServiceForwarder<cam.WebCamOperations>(FindPartner("WebCam").Service);
            LogInfo(LogGroups.Console, "Subscribe to webcam");
            Activate(
                Arbiter.Choice(
                    _camPort.Subscribe(_camNotify),
                    OnSubscribed,
                    OnSubscribeFailed
                )
            );
        }

        void OnSubscribed(SubscribeResponseType success)
        {
            LogInfo(LogGroups.Console, "Subscribed to camera");
        }

        void OnSubscribeFailed(Fault fault)
        {
            LogError(LogGroups.Console, "Failed to subscribe to camera");
            _mainPort.Post(new DsspDefaultDrop());
        }

        /// <summary>
        /// Get Handler
        /// </summary>
        /// <param name="get"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public virtual IEnumerator<ITask> GetHandler(Get get)
        {
            get.ResponsePort.Post(_state);
            yield break;
        }

        void OnCameraUpdateFrame(cam.UpdateFrame updateFrame)
        {
            if (!_state.UpdateFrame)
            {
                _state.UpdateFrame = true;
                SpawnIterator(new List<ColorBin>(_state.ColorBins), ProcessImage);
            }
        }

        /// <summary>
        /// ImageProcessed Handler
        /// </summary>
        /// <param name="imageProcessed"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public IEnumerator<ITask> ImageProcessedHandler(ImageProcessed imageProcessed)
        {
            _state.UpdateFrame = false;

            if (imageProcessed.Body.Results.Count > 0 ||
                _state.Results.Count > 0)
            {
                _state.TimeStamp = imageProcessed.Body.TimeStamp;
                _state.Results = imageProcessed.Body.Results;
                SendNotification(_subMgrPort, imageProcessed);
            }
            imageProcessed.ResponsePort.Post(DefaultUpdateResponseType.Instance);

            yield break;
        }

        /// <summary>
        /// Subscribe Handler
        /// </summary>
        /// <param name="subscribe"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public IEnumerator<ITask> SubscribeHandler(Subscribe subscribe)
        {
            SubscribeHelper(_subMgrPort, subscribe.Body, subscribe.ResponsePort);
            yield break;
        }

        /// <summary>
        /// Insert Bin Handler
        /// </summary>
        /// <param name="insert"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public IEnumerator<ITask> InsertBinHandler(InsertBin insert)
        {
            ColorBin existing = _state.ColorBins.Find(
                delegate(ColorBin test)
                {
                    return test.Name == insert.Body.Name;
                }
            );

            if (existing == null)
            {
                _state.ColorBins.Add(insert.Body);
                insert.ResponsePort.Post(DefaultInsertResponseType.Instance);
                SendNotification(_subMgrPort, insert);
                SaveState(_state);
            }
            else
            {
                insert.ResponsePort.Post(
                    Fault.FromCodeSubcodeReason(
                        FaultCodes.Receiver,
                        DsspFaultCodes.DuplicateEntry,
                        "A Color Bin named " + insert.Body.Name + " already exists."
                    )
                );
            }
            yield break;
        }

        /// <summary>
        /// DeleteBin Handler
        /// </summary>
        /// <param name="delete"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public IEnumerator<ITask> DeleteBinHandler(DeleteBin delete)
        {
            ColorBin existing = _state.ColorBins.Find(
                delegate(ColorBin test)
                {
                    return test.Name == delete.Body.Name;
                }
            );

            if (existing != null)
            {
                _state.ColorBins.Remove(existing);
                delete.ResponsePort.Post(DefaultDeleteResponseType.Instance);
                SendNotification<DeleteBin>(_subMgrPort, existing);
                SaveState(_state);
            }
            else
            {
                delete.ResponsePort.Post(
                    Fault.FromCodeSubcodeReason(
                        FaultCodes.Receiver,
                        DsspFaultCodes.UnknownEntry,
                        "A Color Bin named " + delete.Body.Name + " could not be found."
                    )
                );
            }
            yield break;
        }

        /// <summary>
        /// UpdateBin Handler
        /// </summary>
        /// <param name="update"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public IEnumerator<ITask> UpdateBinHandler(UpdateBin update)
        {
            ColorBin existing = _state.ColorBins.Find(
                delegate(ColorBin test)
                {
                    return test.Name == update.Body.Name;
                }
            );

            if (existing != null)
            {
                int index = _state.ColorBins.IndexOf(existing);
                _state.ColorBins[index] = update.Body;

                update.ResponsePort.Post(DefaultUpdateResponseType.Instance);
                SendNotification(_subMgrPort, update);
                SaveState(_state);
            }
            else
            {
                update.ResponsePort.Post(
                    Fault.FromCodeSubcodeReason(
                        FaultCodes.Receiver,
                        DsspFaultCodes.UnknownEntry,
                        "A Color Bin named " + update.Body.Name + " could not be found."
                    )
                );
            }
            yield break;
        }

        IEnumerator<ITask> ProcessImage(List<ColorBin> bins)
        {
            Fault fault = null;

            cam.QueryFrameRequest request = new cam.QueryFrameRequest();
            request.Format = Guid.Empty;// new Guid("b96b3cae-0728-11d3-9d7b-0000f81ef32e");

            byte[] frame = null;
            DateTime timestamp = DateTime.MinValue;
            int height = 0;
            int width = 0;

            yield return Arbiter.Choice(
                _camPort.QueryFrame(request),
                delegate(cam.QueryFrameResponse response)
                {
                    timestamp = response.TimeStamp;
                    frame = response.Frame;
                    width = response.Size.Width;
                    height = response.Size.Height;

                },
                delegate(Fault f)
                {
                    fault = f;
                }
            );

            ImageProcessedRequest processed = new ImageProcessedRequest();

            if (fault != null)
            {
                _mainPort.Post(new ImageProcessed(processed));
                yield break;
            }

            int size = width * height * 3;

            processed.TimeStamp = timestamp;
            List<FoundBlob> results = processed.Results;

            foreach (ColorBin bin in bins)
            {
                FoundBlob blob = new FoundBlob();

                blob.Name = bin.Name;
                blob.XProjection = new int[width];
                blob.YProjection = new int[height];

                results.Add(blob);
            }

            int offset;

            for (int y = 0; y < height; y++)
            {
                offset = y * width * 3;

                for (int x = 0; x < width; x++, offset += 3)
                {
                    int r, g, b;

                    b = frame[offset];
                    g = frame[offset + 1];
                    r = frame[offset + 2];

                    for (int i = 0; i < bins.Count; i++)
                    {
                        ColorBin bin = bins[i];

                        if (bin.Test(r, g, b))
                        {
                            results[i].AddPixel(x, y);
                        }
                    }
                }
            }

            foreach (FoundBlob blob in results)
            {
                if (blob.Area > 0)
                {
                    blob.MeanX = blob.MeanX / blob.Area;
                    blob.MeanY = blob.MeanY / blob.Area;

                    blob.CalculateMoments();
                }
            }

            _mainPort.Post(new ImageProcessed(processed));

        }
    }
}
