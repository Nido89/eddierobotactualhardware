//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: Replay.cs $ $Revision: 22 $
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Net.Mime;

using System.Text;

using Microsoft.Ccr.Core;
using Microsoft.Dss.Core;
using Microsoft.Dss.Core.Attributes;
using Microsoft.Dss.Core.DsspHttp;
using Microsoft.Dss.Core.DsspHttpUtilities;
using Microsoft.Dss.ServiceModel.Dssp;
using Microsoft.Dss.ServiceModel.DsspServiceBase;
using sm = Microsoft.Dss.Services.SubscriptionManager;

using replay = Microsoft.Robotics.Services.WebCamReplay;
using webcam = Microsoft.Robotics.Services.WebCam;
using mdwebcam = Microsoft.Robotics.Services.MultiDeviceWebCam;
using System.Drawing;
using System.ComponentModel;
using W3C.Soap;
using Microsoft.Robotics.PhysicalModel;

namespace Microsoft.Robotics.Services.WebCamReplay
{
    /// <summary>
    /// Contract class for WebCam Replay service
    /// </summary>
    public static class Contract
    {
        /// <summary>
        /// Contract Identifier
        /// </summary>
        public const string Identifier = "http://schemas.microsoft.com/robotics/2006/07/webcamreplay.user.html";
    }

    /// <summary>
    /// Webcam Replay service.
    /// Replays images previously captured by the WebCam service.
    /// </summary>
    [Contract(Contract.Identifier)]
    [AlternateContract(webcam.Contract.Identifier)]
    [AlternateContract(mdwebcam.Contract.Identifier)]
    [ActivationSettings(ExecutionUnitsPerDispatcher = 1, ShareDispatcher = false)]
    [DisplayName("(User) WebCam Replay")]
    [Description("Replays images previously captured by the WebCam service.")]
    public class Replay : DsspServiceBase
    {
        [InitialStatePartner(Optional = false)]
        mdwebcam.WebCamState _state;

        [EmbeddedResource("Microsoft.Robotics.Services.MultiDeviceWebCam.WebCam.user.xslt")]
        string _transform = null;

        [AlternateServicePort(AlternateContract = WebCam.Contract.Identifier)]
        webcam.WebCamOperations _altPort = new webcam.WebCamOperations();

        [ServicePort("/webcamreplay", AllowMultipleInstances = true)]
        mdwebcam.WebCamServiceOperations _mainPort = new mdwebcam.WebCamServiceOperations();
        webcam.WebCamOperations _fwdPort;

        [Partner("SubMgr", Contract = sm.Contract.Identifier, CreationPolicy = PartnerCreationPolicy.CreateAlways)]
        sm.SubscriptionManagerPort _subMgrPort = new sm.SubscriptionManagerPort();

        DsspHttpUtilitiesPort _utilitiesPort;
        mdwebcam.SaveStreamPort _streamPort;

        string _prefix;
        List<string> _alternatePrefixes;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="creationPort"></param>
        public Replay(DsspServiceCreationPort creationPort) :
                base(creationPort)
        {
        }

        /// <summary>
        /// Service startup
        /// </summary>
        protected override void Start()
        {
            _fwdPort = ServiceForwarder<webcam.WebCamOperations>(ServiceInfo.Service);
            _utilitiesPort = DsspHttpUtilitiesService.Create(Environment);

            Uri httpAlias = ServiceInfo.HttpServiceAlias;
            if (httpAlias != null)
            {
                _prefix = httpAlias.AbsolutePath;
                _alternatePrefixes = AlternateContractServiceInfo.ConvertAll<string>(
                delegate(ServiceInfoType alternate)
                {
                    return alternate.HttpServiceAlias.AbsolutePath;
                }
            );

            }

            try
            {
                _streamPort = mdwebcam.ReadStream.Create(_state.CaptureFile, Environment.TaskQueue);
            }
            catch (Exception e)
            {
                LogError("Unable to open stream file: " + e.Message);
                base.StartFailed();
                return;
            }

            base.Start();

            base.MainPortInterleave.CombineWith(
                new Interleave(
                    new ExclusiveReceiverGroup(
                        Arbiter.Receive<mdwebcam.Frame>(true, _streamPort, ReadFrameHandler)
                    ),
                    new ConcurrentReceiverGroup()
                )
            );
        }

        /// <summary>
        /// Handle get requests
        /// </summary>
        /// <param name="get"></param>
        [ServiceHandler]
        public void GetHandler(mdwebcam.Get get)
        {
            get.ResponsePort.Post(_state);
        }

        /// <summary>
        /// Handle get requests for the generic contract
        /// </summary>
        /// <param name="get"></param>
        [ServiceHandler(PortFieldName = "_altPort")]
        public void GenericGetHandler(webcam.Get get)
        {
            get.ResponsePort.Post(_state.ToGenericState());
        }

        /// <summary>
        /// Handle HTTP GET requests
        /// </summary>
        /// <param name="get"></param>
        /// <returns></returns>
        [ServiceHandler, ServiceHandler(PortFieldName = "_altPort")]
        public IEnumerator<ITask> HttpGetHandler(HttpGet get)
        {
            return HttpHandler(get.Body.Context, get.ResponsePort);
        }

        /// <summary>
        /// Handle HTTP QUERY requests
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [ServiceHandler, ServiceHandler(PortFieldName = "_altPort")]
        public IEnumerator<ITask> HttpQueryHandler(HttpQuery query)
        {
            return HttpHandler(query.Body.Context, query.ResponsePort);
        }

        IEnumerator<ITask> HttpHandler(HttpListenerContext context, PortSet<HttpResponseType, Fault> responsePort)
        {
            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;

            string path = request.Url.AbsolutePath;
            bool alternate = false;

            string alternatePrefix = _alternatePrefixes.Find(path.StartsWith);

            if (!string.IsNullOrEmpty(alternatePrefix))
            {
                alternate = true;
                path = path.Substring(alternatePrefix.Length);
            }
            else if (path.StartsWith(_prefix))
            {
                path = path.Substring(_prefix.Length);
            }

            string type;
            ImageFormat format;

            switch (path)
            {
                case "/jpeg":
                case "/jpg":
                    type = MediaTypeNames.Image.Jpeg;
                    format = ImageFormat.Jpeg;
                    break;
                case "/bmp":
                    type = "image/bmp";
                    format = ImageFormat.Bmp;
                    break;
                case "/png":
                    type = "image/png";
                    format = ImageFormat.Png;
                    break;
                case "/tif":
                case "/tiff":
                    type = "image/tiff";
                    format = ImageFormat.Tiff;
                    break;
                case "/gif":
                    type = MediaTypeNames.Image.Gif;
                    format = ImageFormat.Gif;
                    break;
                default:
                    if (alternate)
                    {
                        responsePort.Post(new HttpResponseType(HttpStatusCode.OK, _state.ToGenericState(), _transform));
                    }
                    else
                    {
                        responsePort.Post(new HttpResponseType(HttpStatusCode.OK, _state, _transform));
                    }
                    yield break;
            }

            if (_state.Image == null)
            {
                if (alternate)
                {
                    responsePort.Post(new HttpResponseType(HttpStatusCode.NotFound, _state.ToGenericState(), _transform));
                }
                else
                {
                    responsePort.Post(new HttpResponseType(HttpStatusCode.NotFound, _state, _transform));
                }
                yield break;
            }

            using (MemoryStream stream = new MemoryStream())
            {
                _state.Image.Save(stream, format);
                stream.Position = 0;

                context.Response.AddHeader("Cache-Control", "No-cache");

                WriteResponseFromStream write = new WriteResponseFromStream(
                    context, stream, type
                );

                _utilitiesPort.Post(write);

                yield return Arbiter.Choice(
                    write.ResultPort,
                    delegate(Stream res)
                    {
                        stream.Close();
                    },
                    delegate(Exception e)
                    {
                        stream.Close();
                        LogError(e);
                    }
                );
            }
        }

        /// <summary>
        /// Handle Replace requests
        /// </summary>
        /// <param name="replace"></param>
        [ServiceHandler]
        public void ReplaceHandler(mdwebcam.Replace replace)
        {
            _state = replace.Body;
            if (_streamPort != null)
            {
                _streamPort.Post(new Shutdown());
            }
            try
            {
                _streamPort = mdwebcam.ReadStream.Create(_state.CaptureFile, Environment.TaskQueue);
            }
            catch (Exception e)
            {
                LogError("Unable to open stream file: " + e.Message);
            }

            replace.ResponsePort.Post(DefaultReplaceResponseType.Instance);

            SendNotification(_subMgrPort, replace);
        }

        /// <summary>
        /// Handle HTTP POST requests
        /// </summary>
        /// <param name="httpPost"></param>
        [ServiceHandler]
        public void HttpPostHandler(HttpPost httpPost)
        {
            httpPost.ResponsePort.Post(
                Fault.FromCodeSubcodeReason(
                    FaultCodes.Receiver,
                    DsspFaultCodes.ActionNotSupported,
                    "HTTP Post is not supported by the webcam replay service"
                )
            );
        }

        /// <summary>
        /// Handle UpdateFormat messages
        /// </summary>
        /// <param name="update"></param>
        [ServiceHandler]
        public void UpdateFormatHandler(mdwebcam.UpdateFormat update)
        {
            update.ResponsePort.Post(
                Fault.FromCodeSubcodeReason(
                    FaultCodes.Receiver,
                    DsspFaultCodes.ActionNotSupported,
                    "UpdateFormat is not supported by the webcam replay service"
                )
            );
        }

        /// <summary>
        /// Handle UpdateFrame messages
        /// </summary>
        /// <param name="update"></param>
        [ServiceHandler, ServiceHandler(PortFieldName = "_altPort")]
        public void UpdateFrameHandler(webcam.UpdateFrame update)
        {
            update.ResponsePort.Post(DefaultUpdateResponseType.Instance);
            SendNotification(_subMgrPort, update);
        }

        void ReadFrameHandler(mdwebcam.Frame frame)
        {
            Bitmap old = _state.Image;

            if (frame.Image != null)
            {
                _state.Image = new Bitmap(frame.Image);
                _state.ImageSize = new Vector2(_state.Image.Size.Width,_state.Image.Size.Height);
                frame.Image.Dispose();
            }
            else
            {
                _state.Image = null;
            }
            _state.LastFrameUpdate = DateTime.UtcNow;


            if (old != null)
            {
                old.Dispose();
            }

            webcam.UpdateFrameRequest request = new webcam.UpdateFrameRequest();
            request.TimeStamp = _state.LastFrameUpdate;

            webcam.UpdateFrame update = new webcam.UpdateFrame();
            update.Body = request;
            update.ResponsePort = null;
            _fwdPort.Post(update);
        }


        /// <summary>
        /// Handle QueryFrame requests
        /// </summary>
        /// <param name="query"></param>
        [ServiceHandler, ServiceHandler(PortFieldName = "_altPort")]
        public void QueryFrameHandler(webcam.QueryFrame query)
        {
            if (_state.Image == null)
            {
                query.ResponsePort.Post(new webcam.QueryFrameResponse());
                return;
            }

            if (query.Body.Format == Guid.Empty)
            {
                // raw image requested;
                BitmapData raw = null;
                try
                {
                    raw = _state.Image.LockBits(new Rectangle(Point.Empty,
                        new Size((int)_state.ImageSize.X, (int)_state.ImageSize.Y)),
                        ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

                    int size = raw.Height * raw.Stride;

                    webcam.QueryFrameResponse response = new webcam.QueryFrameResponse();

                    response.TimeStamp = _state.LastFrameUpdate;
                    response.Frame = new byte[size];
                    response.Size = new Size(raw.Width, raw.Height);
                    response.Format = Guid.Empty;

                    System.Runtime.InteropServices.Marshal.Copy(raw.Scan0, response.Frame, 0, size);

                    query.ResponsePort.Post(response);
                }
                finally
                {
                    if (raw != null)
                    {
                        _state.Image.UnlockBits(raw);
                    }
                }
            }
            else
            {
                ImageFormat format = new ImageFormat(query.Body.Format);

                using (MemoryStream stream = new MemoryStream())
                {
                    Size size = new Size((int)query.Body.Size.X,(int)query.Body.Size.Y);
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


                    webcam.QueryFrameResponse response = new webcam.QueryFrameResponse();
                    response.TimeStamp = _state.LastFrameUpdate;
                    response.Frame = new byte[(int)stream.Length];
                    response.Size = size;
                    response.Format = format.Guid;

                    stream.Position = 0;
                    stream.Read(response.Frame, 0, response.Frame.Length);

                    query.ResponsePort.Post(response);
                }
            }
        }

        /// <summary>
        /// Handle Subscribe requests
        /// </summary>
        /// <param name="subscribe"></param>
        /// <returns></returns>
        [ServiceHandler, ServiceHandler(PortFieldName = "_altPort")]
        public IEnumerator<ITask> SubscribeHandler(webcam.Subscribe subscribe)
        {
            SubscribeRequestType request = subscribe.Body;

            yield return Arbiter.Choice(
                SubscribeHelper(_subMgrPort, request, subscribe.ResponsePort),
                delegate(SuccessResult success)
                {
                    SendNotificationToTarget<webcam.Replace>(request.Subscriber, _subMgrPort, _state);
                },
                delegate(Exception e)
                {
                    LogError(null, "Failure while processing Subscribe Request", Fault.FromException(e));
                }
            );
            yield break;
        }
    }
}
