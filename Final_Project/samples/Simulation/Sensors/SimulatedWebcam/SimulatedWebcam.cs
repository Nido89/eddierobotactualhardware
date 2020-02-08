//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: SimulatedWebcam.cs $ $Revision: 44 $
//-----------------------------------------------------------------------
using Microsoft.Ccr.Core;
using Microsoft.Dss.Core;
using Microsoft.Dss.Core.Attributes;
using Microsoft.Dss.ServiceModel.Dssp;
using Microsoft.Dss.ServiceModel.DsspServiceBase;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Security.Permissions;
using Microsoft.Dss.Core.DsspHttp;
using simulatedwebcam = Microsoft.Robotics.Services.Simulation.Sensors.SimulatedWebcam;
using submgr = Microsoft.Dss.Services.SubscriptionManager;
using xml = System.Xml;
using simtypes = Microsoft.Robotics.Simulation;
using simengine = Microsoft.Robotics.Simulation.Engine;
using dssp = Microsoft.Dss.ServiceModel.Dssp;
using System.Drawing;
using System.Net;
using System.Drawing.Imaging;
using Microsoft.Dss.Core.DsspHttpUtilities;
using System.IO;
using System.Net.Mime;
using W3C.Soap;
using webcam = Microsoft.Robotics.Services.WebCam;
using Microsoft.Robotics.PhysicalModel;



namespace Microsoft.Robotics.Services.Simulation.Sensors.SimulatedWebcam
{
    /// <summary>
    /// Provides access to a simulated Webcam. : implements frame retrieval from simulation CameraEntity
    /// </summary>
    [DisplayName("(User) Simulated Webcam")]
    [Description("Provides access to a simulated WebCam.")]
    [Contract(Contract.Identifier)]
    [AlternateContract(webcam.Contract.Identifier)]
    public partial class SimulatedWebcamService : DsspServiceBase
    {
        #region Simulation Variables
        simengine.CameraEntity _entity;
        simengine.SimulationEnginePort _notificationTarget;
        #endregion

        [EmbeddedResource("Microsoft.Robotics.Services.Simulation.Sensors.SimulatedWebcam.WebCam.user.xslt")]
        string _transform = null;

        DsspHttpUtilitiesPort _utilitiesPort = new DsspHttpUtilitiesPort();

        private webcam.WebCamState _state = new webcam.WebCamState();
        [ServicePort("/simulatedwebcam", AllowMultipleInstances=true)]
        private webcam.WebCamOperations _mainPort = new webcam.WebCamOperations();

        [Partner("SubMgr", Contract = submgr.Contract.Identifier, CreationPolicy = PartnerCreationPolicy.CreateAlways)]
        submgr.SubscriptionManagerPort _subMgrPort = new submgr.SubscriptionManagerPort();

        /// <summary>
        /// Default Service Constructor
        /// </summary>
        public SimulatedWebcamService(DsspServiceCreationPort creationPort) :
                base(creationPort)
        {
        }

        /// <summary>
        /// Service Start
        /// </summary>
        protected override void Start()
        {
            _notificationTarget = new simengine.SimulationEnginePort();
            _utilitiesPort = DsspHttpUtilitiesService.Create(Environment);
            simengine.SimulationEngine.GlobalInstancePort.Subscribe(ServiceInfo.PartnerList, _notificationTarget);

            // dont start listening to DSSP operations, other than drop, until notification of entity
            Activate(new Interleave(
                new TeardownReceiverGroup
                (
                    Arbiter.Receive<simengine.InsertSimulationEntity>(false, _notificationTarget, InsertEntityNotificationHandlerFirstTime),
                    Arbiter.Receive<dssp.DsspDefaultDrop>(false, _mainPort, DefaultDropHandler)
                ),
                new ExclusiveReceiverGroup(),
                new ConcurrentReceiverGroup()
            ));
        }

        //----------------------------------------------

        void InsertEntityNotificationHandlerFirstTime(simengine.InsertSimulationEntity ins)
        {
            InsertEntityNotificationHandler(ins);
            base.Start();
            MainPortInterleave.CombineWith(
                new Interleave(
                    new TeardownReceiverGroup
                    (
                    ),
                    new ExclusiveReceiverGroup(
                        Arbiter.Receive<simengine.InsertSimulationEntity>(true, _notificationTarget, InsertEntityNotificationHandler),
                        Arbiter.Receive<simengine.DeleteSimulationEntity>(true, _notificationTarget, DeleteEntityNotificationHandler)
                    ),
                    new ConcurrentReceiverGroup()
                )
            );
        }

        void InsertEntityNotificationHandler(simengine.InsertSimulationEntity ins)
        {
            _entity = (simengine.CameraEntity)ins.Body;
            _entity.ServiceContract = Contract.Identifier;
            _state.CameraDeviceName = _entity.State.Name;
            _state.Pose = _entity.State.Pose;
            _state.ViewAngle = _entity.ViewAngle;
            _state.ImageSize = new Vector2(_entity.ViewSizeX, _entity.ViewSizeY);

            // send an initial update to self so we get image data filled in
            PostUpdateFrameIfNeeded();  

            if (_entity.IsRealTimeCamera)
            {
                if (_entity.UpdateInterval != 0)
                    Activate(Arbiter.Receive(false, TimeoutPort(_entity.UpdateInterval), CheckForUpdate));
                else
                    Activate(Arbiter.Receive(false, TimeoutPort(1000), CheckForUpdate));
            }
        }

        // Send an update message to ourself at an interval specified by the camera entity.
        // If the interval is 0, check once per second to see if the interval has changed but
        // don't do an update.
        void CheckForUpdate(DateTime time)
        {
            if (_entity == null)
                return; // the entity went away, do no more updates

            if (_entity.UpdateInterval != 0)
            {
                PostUpdateFrameIfNeeded();
                Activate(Arbiter.Receive(false, TimeoutPort(_entity.UpdateInterval), CheckForUpdate));
            }
            else
                Activate(Arbiter.Receive(false, TimeoutPort(1000), CheckForUpdate));
        }

        // no need to post another update if one is already queued
        private void PostUpdateFrameIfNeeded()
        {
            if (MainPortInterleave==null || MainPortInterleave.PendingExclusiveCount == 0)
            {
                _mainPort.Post(new webcam.UpdateFrame());
            }
        }

        void DeleteEntityNotificationHandler(simengine.DeleteSimulationEntity del)
        {
            _entity = null;
        }


        //----------------------------------------------
        /// <summary>
        /// Drop handler for web cam
        /// </summary>
        /// <param name="drop"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Teardown)]
        public IEnumerator<ITask> DropHandler(DsspDefaultDrop drop)
        {
            DefaultDropHandler(drop);
            yield break;
        }

        /**/
        /// <summary>
        /// HttpGet Handler
        /// </summary>
        /// <param name="get"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public IEnumerator<ITask> HttpGetHandler(HttpGet get)
        {
            return HttpHandler(get.Body.Context, get.ResponsePort);
        }

        /// <summary>
        /// Query handler for web cam
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public IEnumerator<ITask> HttpQueryHandler(HttpQuery query)
        {
            return HttpHandler(query.Body.Context, query.ResponsePort);
        }

        IEnumerator<ITask> HttpHandler(HttpListenerContext context, PortSet<HttpResponseType, Fault> responsePort)
        {
            PostUpdateFrameIfNeeded();

            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;

            string path = request.Url.AbsolutePath;
            string type;
            System.Drawing.Imaging.ImageFormat format;

            if ((path.EndsWith("/jpg")) || (path.EndsWith("/jpeg")))
            {
                type = MediaTypeNames.Image.Jpeg;
                format = System.Drawing.Imaging.ImageFormat.Jpeg;
            }
            else if (path.EndsWith("/bmp"))
            {
                type = "image/bmp";
                format = System.Drawing.Imaging.ImageFormat.Bmp;
            }
            else if (path.EndsWith("gif"))
            {
                type = MediaTypeNames.Image.Gif;
                format = System.Drawing.Imaging.ImageFormat.Gif;
            }
            else if (path.EndsWith("/png"))
            {
                type = "image/png";
                format = System.Drawing.Imaging.ImageFormat.Png;
            }
            else
            {
                responsePort.Post(new HttpResponseType(HttpStatusCode.OK, _state, _transform));
                yield break;
            }

            if (_entity == null)
            {
                responsePort.Post(Fault.FromException(new Exception("No camera entity found.")));
            }
            else
            {
                var result = new PortSet<int[], Exception>();
                _entity.CaptureScene(result);

                yield return Arbiter.Choice(result,
                    delegate(int [] data)
                    {
                        var memStream = new MemoryStream();
                        using (var b = _entity.CreateBitmapFromArray(data))
                        {
                            b.Save(memStream, format);
                        }

                        memStream.Seek(0, SeekOrigin.Begin);
                        memStream.WriteTo(response.OutputStream);
                        response.AddHeader("Cache-Control", "No-cache");
                        response.ContentType = type;
                        _state.LastFrameUpdate = DateTime.Now;
                        Utilities.HttpClose(context);
                    },
                    delegate(Exception ex)
                    {
                        responsePort.Post(Fault.FromException(ex));
                    }
                );
            }
        }

        /// <summary>
        /// Get Handler
        /// </summary>
        /// <param name="get"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public IEnumerator<ITask> GetHandler(webcam.Get get)
        {
            if(_entity!=null)
                _state.Pose = _entity.State.Pose;

            get.ResponsePort.Post(_state);
            yield break;
        }

        /// <summary>
        /// Handler that updates the copy of the web cam's bitmap image 
        /// </summary>
        /// <param name="update"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public IEnumerator<ITask> UpdateFrameHandler(webcam.UpdateFrame update)
        {
            if (_entity != null)
            {
                PortSet<int[], Exception> result = new PortSet<int[], Exception>();
                _entity.CaptureScene(result);

                yield return Arbiter.Choice(result,
                    delegate(int [] data)
                    {
                        var b = _entity.CreateBitmapFromArray(data);
                        _state.LastFrameUpdate = DateTime.Now;
                        _state.Image = b;
                        update.ResponsePort.Post(DefaultUpdateResponseType.Instance);
                        base.SendNotification<webcam.UpdateFrame>(_subMgrPort, update);

                        this.DoWebCamSensorReplace(data);
                    },
                    delegate(Exception ex)
                    {
                        update.ResponsePort.Post(Fault.FromException(ex));
                    });
            }
        }

        /// <summary>
        /// Queries a frame from the real time camera entity.
        /// Needs to be exclusive because it locks the _state.Image object.
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public IEnumerator<ITask> QueryFrameHandler(webcam.QueryFrame query)
        {
            if (_state.Image == null || _entity==null)
            {
                query.ResponsePort.Post(new webcam.QueryFrameResponse());
                yield break;
            }

            Size size = new Size((int)query.Body.Size.X, (int)query.Body.Size.Y);

            if (query.Body.Format == Guid.Empty)
            {
                // raw image requested;
                BitmapData raw = null;

                try
                {
                    lock (_state.Image)
                    {
                        bool isResizeRequired = false;
                        // size not specified
                        if (size.Width == 0)
                        {
                            size = _state.Image.Size;                            
                        }
                        else if (size.Width != _state.Image.Width ||
                            size.Height != _state.Image.Height)
                        {
                            isResizeRequired = true;
                        }

                        var image = _state.Image;
                        if (isResizeRequired)
                        {
                            image = new Bitmap(image, size);
                        }

                        raw = image.LockBits(new Rectangle(Point.Empty, size),
                            ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

                        int byteSize = raw.Height * raw.Stride;

                        webcam.QueryFrameResponse response = new webcam.QueryFrameResponse();

                        response.TimeStamp = _state.LastFrameUpdate;
                        response.Frame = new byte[byteSize];
                        response.Size = new Size(raw.Width, raw.Height);
                        response.Format = Guid.Empty;

                        System.Runtime.InteropServices.Marshal.Copy(raw.Scan0, response.Frame, 0, byteSize);

                        query.ResponsePort.Post(response);
                        image.UnlockBits(raw);
                    }
                }
                catch (Exception ex)
                {
                    if (raw != null)
                        _state.Image.UnlockBits(raw);

                    query.ResponsePort.Post(Fault.FromException(ex));
                }
            }
            else
            {
                ImageFormat format = new ImageFormat(query.Body.Format);

                using (MemoryStream stream = new MemoryStream())
                {

                    try
                    {
                        lock (_state.Image)
                        {
                            if (size == _state.Image.Size ||
                                size.Width == 0 ||
                                size.Height == 0 ||
                                size.Width >= _state.Image.Width ||
                                size.Height >= _state.Image.Height)
                            {
                                size = _state.Image.Size;
                                _state.Image.Save(stream, format);
                            }
                            else
                            {
                                using (Bitmap temp = new Bitmap(
                                    _state.Image, size))
                                {
                                    temp.Save(stream, format);
                                }
                            }
                        }


                        webcam.QueryFrameResponse response = new webcam.QueryFrameResponse();
                        response.TimeStamp = _state.LastFrameUpdate;
                        response.Frame = new byte[(int)stream.Length];
                        response.Size = size;
                        response.Format = format.Guid;

                        stream.Position = 0;
                        stream.Read(response.Frame, 0, response.Frame.Length);

                        query.ResponsePort.Post(response);
                    }
                    catch (Exception ex)
                    {
                        query.ResponsePort.Post(Fault.FromException(ex));
                    }
                }
            }
            yield break;
        }


        /// <summary>
        /// Replace Handler
        /// </summary>
        /// <param name="replace"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public IEnumerator<ITask> ReplaceHandler(webcam.Replace replace)
        {
            _state = replace.Body;
            // Note that replace is not fully implemented. It currently updates state
            // but does not reconfigure the simulation entity
            replace.ResponsePort.Post(DefaultReplaceResponseType.Instance);
            // send notification about the replace
            base.SendNotification<webcam.Replace>(_subMgrPort, replace);
            yield break;
        }

        /// <summary>
        /// Handler that processes subscribe messages for the web cam service
        /// </summary>
        /// <param name="subscribe"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public IEnumerator<ITask> SubscribeHandler(webcam.Subscribe subscribe)
        {
            // We share a subscription manager with the WebCamSensor alternate, so this ensures we that
            // we automatically filter notifcations based on the contract that the subscription came
            // through
            if (subscribe.Body.TypeFilter == null || subscribe.Body.TypeFilter.Length == 0)
            {
                subscribe.Body.TypeFilter = new[]
                    { 
                        GetTypeFilterDescription<webcam.Replace>(), 
                        GetTypeFilterDescription<webcam.UpdateFrame>() 
                    };
            }

            yield return Arbiter.Choice(
                SubscribeHelper(_subMgrPort, subscribe.Body, subscribe.ResponsePort),
                delegate(SuccessResult success)
                {
                    LogInfo("Subscription from: " + subscribe.Body.Subscriber);
                },
                delegate(Exception e)
                {
                    LogError(e);
                }
            );
        }
    }
}
