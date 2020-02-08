//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: IpCamera.cs $ $Revision: 11 $
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Net.Mime;
using System.Xml;

using Microsoft.Ccr.Core;
using Microsoft.Dss.Core;
using Microsoft.Dss.Core.Attributes;
using Microsoft.Dss.Core.DsspHttp;
using Microsoft.Dss.Core.DsspHttpUtilities;
using Microsoft.Dss.ServiceModel.Dssp;
using Microsoft.Dss.ServiceModel.DsspServiceBase;

using W3C.Soap;

using webcam = Microsoft.Robotics.Services.WebCam.Proxy;
using sm = Microsoft.Dss.Services.SubscriptionManager;

namespace Microsoft.Robotics.Services.Sample.IpCamera
{
    /// <summary>
    /// Ip camera service implementation
    /// </summary>
    [DisplayName("(User) Ip Camera")]
    [Description("Provides access to an IP camera.")]
    [AlternateContract(webcam.Contract.Identifier)]
    [Contract(Contract.Identifier)]
    [DssServiceDescription("http://msdn.microsoft.com/library/cc998472.aspx")]
    public class IpCameraService : DsspServiceBase
    {
        [InitialStatePartner(Optional = true)]
        private IpCameraState _state = new IpCameraState();

        string _prefix;
        string _alternatePrefix;

        [EmbeddedResource("Microsoft.Robotics.Services.Sample.IpCamera.WebCam.user.xslt")]
        string _transform = null;

        [EmbeddedResource("Microsoft.Robotics.Services.Sample.IpCamera.ImageNotSpecified.png")]
        string _imageNotSpecified = null;

        [ServicePort("/ipcamera", AllowMultipleInstances = true)]
        private IpCameraOperations _mainPort = new IpCameraOperations();

        [AlternateServicePort(AlternateContract = webcam.Contract.Identifier)]
        webcam.WebCamOperations _alternatePort = new webcam.WebCamOperations();

        [SubscriptionManagerPartner]
        sm.SubscriptionManagerPort _subMgr = new sm.SubscriptionManagerPort();

        DsspHttpUtilitiesPort _utilitiesPort;

        PortSet<DateTime, Bitmap> _imagePort = new PortSet<DateTime, Bitmap>();
        /// <summary>
        /// Initialization constructor
        /// </summary>
        /// <param name="creationPort"></param>
        public IpCameraService(DsspServiceCreationPort creationPort) :
                base(creationPort)
        {
        }
        /// <summary>
        /// Service start method
        /// </summary>
        protected override void Start()
        {
            if (_state == null)
            {
                _state = new IpCameraState();
            }

            _utilitiesPort = DsspHttpUtilitiesService.Create(Environment);

            Uri httpAlias = ServiceInfo.HttpServiceAlias;
            if (httpAlias != null)
            {
                _prefix = httpAlias.AbsolutePath;
                _alternatePrefix = AlternateContractServiceInfo[0].HttpServiceAlias.AbsolutePath;
            }
            else
            {
                LogError(LogGroups.Activation, "Service requires HTTP transport");
                DefaultDropHandler(new DsspDefaultDrop());
                return;
            }

            base.Start();

            MainPortInterleave.CombineWith(
                new Interleave(
                    new ExclusiveReceiverGroup(
                        Arbiter.Receive<Bitmap>(true, _imagePort, ImageHandler)
                    ),
                    new ConcurrentReceiverGroup(
                        Arbiter.Receive<DateTime>(true, _imagePort, GetImageHandler)
                    )
                )
            );

            _imagePort.Post(DateTime.UtcNow);
        }

        void GetImageHandler(DateTime signal)
        {
            Uri location;

            if (_state.CameraImageLocation == null)
            {
                Uri uri = new Uri(
                    ServiceInfo.HttpServiceAlias,
                    _imageNotSpecified
                );

                location = uri;
            }
            else
            {
                location = _state.CameraImageLocation;
            }

            SpawnIterator<Uri>(location, LoadImageFromLocation);
        }

        IEnumerator<ITask> LoadImageFromLocation(Uri location)
        {
            WebResponse response = null;

            try
            {
                WebRequest request = WebRequest.Create(location);
                request.UseDefaultCredentials = true;

                IAsyncResult ar = null;
                Port<IAsyncResult> completion = new Port<IAsyncResult>();

                ar = request.BeginGetResponse(completion.Post, null);

                yield return Arbiter.Receive(false, completion, EmptyHandler);

                try
                {
                    response = request.EndGetResponse(ar);
                }
                catch (Exception e)
                {
                    LogError("Unable to get response from camera", e);
                    yield break;
                }

                using (Stream stream = response.GetResponseStream())
                {
                    try
                    {
                        using (Bitmap bmp = new Bitmap(stream))
                        {
                            _imagePort.Post(new Bitmap(bmp));
                        }
                    }
                    catch (Exception e)
                    {
                        LogError("Unable to convert response to image", e);
                        yield break;
                    }
                }
            }
            finally
            {
                Activate(
                    Arbiter.Receive(false, TimeoutPort(60), _imagePort.Post)
                );
            }
        }

        void ImageHandler(Bitmap bitmap)
        {
            _state.Frame = bitmap;

            _state.ImageSize = new Microsoft.Robotics.PhysicalModel.Proxy.Vector2(
                bitmap.Width,
                bitmap.Height
            );

            webcam.UpdateFrameRequest request = new webcam.UpdateFrameRequest();
            request.TimeStamp = DateTime.Now;

            _mainPort.Post(new webcam.UpdateFrame(request));
        }
        /// <summary>
        /// Get handler for ip camera state
        /// </summary>
        /// <param name="get"></param>
        [ServiceHandler]
        public void GetHandler(Get get)
        {
            get.ResponsePort.Post(_state);
        }
        /// <summary>
        /// Get handler for generic webcam state
        /// </summary>
        /// <param name="get"></param>
        [ServiceHandler(PortFieldName = "_alternatePort")]
        public void WebCamGetHandler(webcam.Get get)
        {
            get.ResponsePort.Post((webcam.WebCamState)_state.Clone());
        }
        /// <summary>
        /// Replace handler for Ip camera state
        /// </summary>
        /// <param name="replace"></param>
        [ServiceHandler]
        public void ReplaceHandler(Replace replace)
        {
            _state = replace.Body;
            replace.ResponsePort.Post(DefaultReplaceResponseType.Instance);
        }
        /// <summary>
        /// Replace handler for webcam generic state
        /// </summary>
        /// <param name="replace"></param>
        [ServiceHandler(PortFieldName = "_alternatePort")]
        public void WebCamReplaceHandler(webcam.Replace replace)
        {
            replace.Body.CopyTo(_state);

            replace.ResponsePort.Post(DefaultReplaceResponseType.Instance);
        }
        /// <summary>
        /// Subscribe handler
        /// </summary>
        /// <param name="subscribe"></param>
        [ServiceHandler]
        [ServiceHandler(PortFieldName = "_alternatePort")]
        public void SubscribeHandler(webcam.Subscribe subscribe)
        {
            SubscribeHelper(_subMgr, subscribe.Body, subscribe.ResponsePort);
        }
        /// <summary>
        /// Update handler for generic webcam
        /// </summary>
        /// <param name="updateFrame"></param>
        [ServiceHandler]
        [ServiceHandler(PortFieldName = "_alternatePort")]
        public void UpdateFrameHandler(webcam.UpdateFrame updateFrame)
        {
            _state.LastFrameUpdate = updateFrame.Body.TimeStamp;

            updateFrame.ResponsePort.Post(DefaultUpdateResponseType.Instance);

            SendNotification(_subMgr, updateFrame);
        }
        /// <summary>
        /// QueryFrame handler for generic webcam
        /// </summary>
        /// <param name="queryFrame"></param>
        [ServiceHandler]
        [ServiceHandler(PortFieldName = "_alternatePort")]
        public void QueryFrameHandler(webcam.QueryFrame queryFrame)
        {
            if (_state.Frame == null)
            {
                queryFrame.ResponsePort.Post(new webcam.QueryFrameResponse());
            }
            else if (queryFrame.Body.Format == Guid.Empty)
            {
                // raw image requested;
                BitmapData raw = null;
                try
                {
                    raw = _state.Frame.LockBits(
                        new Rectangle(Point.Empty,
                            new Size((int)_state.ImageSize.X,(int)_state.ImageSize.Y)),
                        ImageLockMode.ReadOnly,
                        PixelFormat.Format24bppRgb);

                    int size = raw.Height * raw.Stride;

                    webcam.QueryFrameResponse response = new webcam.QueryFrameResponse();

                    response.TimeStamp = _state.LastFrameUpdate;
                    response.Frame = new byte[size];
                    response.Size = new Size(raw.Width, raw.Height);
                    response.Format = Guid.Empty;

                    System.Runtime.InteropServices.Marshal.Copy(raw.Scan0, response.Frame, 0, size);

                    queryFrame.ResponsePort.Post(response);
                }
                finally
                {
                    if (raw != null)
                    {
                        _state.Frame.UnlockBits(raw);
                    }
                }
            }
            else
            {
                ImageFormat format = new ImageFormat(queryFrame.Body.Format);

                using (MemoryStream stream = new MemoryStream())
                {
                    Size size = new Size((int)queryFrame.Body.Size.X, (int)queryFrame.Body.Size.Y);
                    if (size == _state.Frame.Size ||
                        size.Width == 0 ||
                        size.Height == 0 ||
                        size.Width >= _state.Frame.Width ||
                        size.Height >= _state.Frame.Height)
                    {
                        size = _state.Frame.Size;
                        _state.Frame.Save(stream, format);
                    }
                    else
                    {
                        using (Bitmap temp = new Bitmap(_state.Frame, size))
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

                    queryFrame.ResponsePort.Post(response);
                }
            }
        }
        /// <summary>
        /// HttpQuery handler for generic webcam 
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [ServiceHandler]
        [ServiceHandler(PortFieldName = "_alternatePort")]
        public IEnumerator<ITask> HttpQueryHandler(HttpQuery query)
        {
            return HttpHandler(query.Body.Context, query.ResponsePort);
        }
        /// <summary>
        /// HttpGet handler for generic webcam 
        /// </summary>
        /// <param name="get"></param>
        /// <returns></returns>
        [ServiceHandler]
        [ServiceHandler(PortFieldName = "_alternatePort")]
        public IEnumerator<ITask> HttpGetHandler(HttpGet get)
        {
            return HttpHandler(get.Body.Context, get.ResponsePort);
        }
        /// <summary>
        /// HttpPost handler for generic webcam 
        /// </summary>
        /// <param name="post"></param>
        /// <returns></returns>
        [ServiceHandler]
        public IEnumerator<ITask> HttpPostHandler(HttpPost post)
        {
            HttpPostRequestData request = post.GetHeader<HttpPostRequestData>();
            NameValueCollection collection = null;


            if (request == null)
            {
                LogError("No form data available with HttpPost");
            }
            else
            {
                collection = request.Parameters;

                string location = collection["Location"];

                if (location != null)
                {
                    _state.CameraImageLocation = new Uri(location);
                    yield return Arbiter.Choice(
                        SaveState(_state),
                        delegate(DefaultReplaceResponseType success) { },
                        delegate(Fault fault)
                        {
                            LogError(null, "Unable to save state", fault);
                        }
                    );
                }
            }

            yield return new IterativeTask<HttpListenerContext, PortSet<HttpResponseType, Fault>>(
                post.Body.Context,
                post.ResponsePort,
                HttpHandler
            );
        }

        IEnumerator<ITask> HttpHandler(HttpListenerContext context, PortSet<HttpResponseType, Fault> responsePort)
        {
            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;

            bool alternate = false;

            string path = request.Url.AbsolutePath.ToLowerInvariant();
            if (path.StartsWith(_alternatePrefix))
            {
                alternate = true;
                path = path.Substring(_alternatePrefix.Length);
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
                        responsePort.Post(new HttpResponseType(HttpStatusCode.OK, _state.Clone(), _transform));
                    }
                    else
                    {
                        responsePort.Post(new HttpResponseType(HttpStatusCode.OK, _state, _transform));
                    }
                    yield break;
            }

            if (_state.Frame == null)
            {
                if (alternate)
                {
                    responsePort.Post(new HttpResponseType(HttpStatusCode.NotFound, _state.Clone(), _transform));
                }
                else
                {
                    responsePort.Post(new HttpResponseType(HttpStatusCode.NotFound, _state, _transform));
                }
                yield break;
            }

            using (MemoryStream stream = new MemoryStream())
            {
                _state.Frame.Save(stream, format);
                stream.Position = 0;

                response.AddHeader("Cache-Control", "No-cache");

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

    }
}
