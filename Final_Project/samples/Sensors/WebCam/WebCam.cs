//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: WebCam.cs $ $Revision: 40 $
//-----------------------------------------------------------------------
using Microsoft.Ccr.Core;
using Microsoft.Dss.Core.Attributes;
using Microsoft.Dss.ServiceModel.Dssp;
using Microsoft.Dss.ServiceModel.DsspServiceBase;
using System;
using System.Collections.Generic;
using System.Security.Permissions;
using Microsoft.Dss.Core.DsspHttp;

using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Drawing.Imaging;
using W3C.Soap;
using System.Net;
using System.Net.Mime;
using Microsoft.Dss.Core.DsspHttpUtilities;
using submgr = Microsoft.Dss.Services.SubscriptionManager;
using webcam = Microsoft.Robotics.Services.WebCam;
using physics = Microsoft.Robotics.PhysicalModel;
using System.Collections.Specialized;
using Microsoft.Dss.Core;
using System.ComponentModel;
using System.Collections;
using ci = System.Globalization.CultureInfo;


namespace Microsoft.Robotics.Services.MultiDeviceWebCam
{
    /// <summary>
    /// Webcam service implementation class
    /// </summary>
    [AlternateContract(WebCam.Contract.Identifier)]
    [Contract(Contract.Identifier)]
    [ActivationSettings(ExecutionUnitsPerDispatcher=1,ShareDispatcher=false)]
    [DisplayName("(User) WebCam")]
    [Description("Captures images from an attached camera.")]
    [DssServiceDescription("http://msdn.microsoft.com/library/bb483096.aspx")]
    public partial class WebCamService : DsspServiceBase
    {
        [InitialStatePartner(Optional = true, ServiceUri = ServicePaths.Store + "/webcam.user.config.xml")]
        WebCamState _state;
        DateTime _captureEpoch = DateTime.MinValue;

        [EmbeddedResource("Microsoft.Robotics.Services.MultiDeviceWebCam.WebCam.user.xslt")]
        string _transform = null;

        string _prefix;
        string _alternatePrefix;

        [AlternateServicePort(AlternateContract = WebCam.Contract.Identifier)]
        webcam.WebCamOperations _altPort = new webcam.WebCamOperations();

        [ServicePort("/webcam", AllowMultipleInstances = true)]
        WebCamServiceOperations _mainPort = new WebCamServiceOperations();
        webcam.WebCamOperations _altFwdPort;
        WebCamServiceOperations _fwdPort;

        FramePort _framePort = new FramePort();
        FrameRequestPort _frameRequestPort = new FrameRequestPort();

        DsspHttpUtilitiesPort _utilitiesPort = new DsspHttpUtilitiesPort();
#if !URT_MINCLR
        DispatcherQueue _queue = null;
        SaveStreamPort _streamPort = null;

        [SubscriptionManagerPartner]
        submgr.SubscriptionManagerPort _submgrPort = new submgr.SubscriptionManagerPort();
#else
        private Dictionary<string, Port<DsspOperation>> _subscribers = new Dictionary<string, Port<DsspOperation>>();
#endif

        /// <summary>
        /// Normal constructor used in service creation
        /// </summary>
        /// <param name="creationPort">service creation port</param>
        public WebCamService(DsspServiceCreationPort creationPort) :
                base(creationPort)
        {
        }

        /// <summary>
        /// Complete second phase startup for this service
        /// </summary>
        protected override void Start()
        {
            _fwdPort = ServiceForwarder<WebCamServiceOperations>(ServiceInfo.Service);
            _altFwdPort = ServiceForwarder<webcam.WebCamOperations>(AlternateContractServiceInfo[0].Service);

            _utilitiesPort = DsspHttpUtilitiesService.Create(Environment);

            _prefix = new Uri(ServiceInfo.Service).AbsolutePath;
            _alternatePrefix = new Uri(AlternateContractServiceInfo[0].Service).AbsolutePath;

            if (_state == null)
            {
                _state = new WebCamState();
            }

            base.ActivateDsspOperationHandlers();

            if (_state.FramesOnDemand)
            {
                _framesOnDemand = true;
                Activate(Arbiter.ReceiveWithIterator(true, _framePort, OnDemandFrameHandler));
            }
            else
            {
                base.MainPortInterleave.CombineWith(
                    Arbiter.Interleave(
                        new TeardownReceiverGroup(),
                        new ExclusiveReceiverGroup
                        (
                            Arbiter.Receive(true, _framePort, FrameHandler)
                        ),
                        new ConcurrentReceiverGroup()
                    )
                );
            }

            StartPipeServer();

            SpawnIterator(_state, GetInitialState);
        }

#if !URT_MINCLR
        Choice InitializeInternalService()
        {
            AllocateExecutionResource allocExecRes = new AllocateExecutionResource(
                true, 
                1, 
                "WebCamResourceGroup",
                ServiceInfo.Service
            );

            ResourceManagerPort.Post(allocExecRes);

            return Arbiter.Choice(
                allocExecRes.Result,
                delegate(ExecutionAllocationResult result)
                {
                    _queue = result.TaskQueue;
                },
                delegate(Exception e)
                {
                    LogError(e);
                }
            );
        }

        void ShutdownInternalService()
        {
            if (_queue != null)
            {
                if (_streamPort != null)
                {
                    _streamPort.Post(new Shutdown());
                    _streamPort = null;
                }

                ResourceManagerPort.Post(new FreeExecutionResource(_queue));
                _queue = null;
            }
        }
#endif

        IEnumerator<ITask> GetInitialState(WebCamState initialState)
        {
            bool deviceSelected = false;

            try
            {
                WebCamState state = new WebCamState();
                state.CaptureFile = initialState.CaptureFile;
                state.Quality = initialState.Quality;
                state.FramesOnDemand = initialState.FramesOnDemand;

#if !URT_MINCLR
                yield return InitializeInternalService();
#endif


                Port<EmptyValue> completion = new Port<EmptyValue>();

                SpawnIterator(state, completion, RefreshCameraList);

                yield return Arbiter.Receive(false, completion, EmptyHandler);

                state.Image = null;

                Replace replace = new Replace();
                replace.Body = state;
                _fwdPort.Post(replace);

                yield return Arbiter.Choice(
                    replace.ResponsePort,
                    delegate(DefaultReplaceResponseType success) { },
                    delegate(Fault fault)
                    {
                        LogError(null, "Unable to set camera list", fault);
                    }
                );

                int deviceIndex;

                if (initialState.Selected != null &&
                    (!string.IsNullOrEmpty(initialState.Selected.DevicePath) ||
                     !string.IsNullOrEmpty(initialState.Selected.FriendlyName)))
                {
                    deviceIndex = -1;
                }
                else
                {
                    deviceIndex = 0;
                }

                bool gotDevice = false;
                while (deviceIndex < state.Cameras.Count && !gotDevice)
                {
                    UpdateDeviceRequest request = new UpdateDeviceRequest();

                    if (deviceIndex < 0)
                    {
                        request.Selected = initialState.Selected;
                        request.Selected.InUse = false;
                    }
                    else if (deviceIndex < state.Cameras.Count)
                    {
                        request.Selected = state.Cameras[deviceIndex];
                    }

                    if (!request.Selected.InUse)
                    {
                        UpdateDevice update = new UpdateDevice();
                        update.Body = request;
                        _fwdPort.Post(update);

                        yield return Arbiter.Choice(
                            update.ResponsePort,
                            success => gotDevice = true,
                            fault => LogInfo("Unable to select camera: " + deviceIndex + ": " + fault)
                        );
                    }
                    else
                    {
                        LogInfo("Not trying camera (InUse = true): " + deviceIndex);
                    }

                    deviceIndex++;
                }

                if (!gotDevice)
                {
                    LogError("Unable to select device");
                    yield break;
                }

                deviceSelected = true;

                if (initialState.Selected == null ||
                    initialState.Selected.Format == null)
                {
                    yield break;
                }

                UpdateFormat updateFormat = new UpdateFormat();
                updateFormat.Body = initialState.Selected.Format;

                _fwdPort.Post(updateFormat);

                yield return Arbiter.Choice(
                    updateFormat.ResponsePort,
                    delegate(DefaultUpdateResponseType success) { },
                    delegate(Fault fault)
                    {
                        LogError(null, "Unable to select format", fault);
                    }
                );
            }
            finally
            {
                if (deviceSelected)
                {
                    DirectoryInsert();
                }
                else
                {
                    LogWarning(LogGroups.Console, "Dropping webcam service, no cameras found");
                    _fwdPort.Post(new DsspDefaultDrop());
                }
            }
        }

        private IEnumerator<ITask> RefreshCameraList(WebCamState state, Port<EmptyValue> completionPort)
        {
            var devicePort = EnumDevices();

            yield return Arbiter.Receive(false, devicePort, list => state.Cameras = list);

            completionPort.Post(EmptyValue.SharedInstance);
        }

#if false
        private List<Format> ConvertFormats(vision.Format[] formats)
        {
            List<Format> converted = new List<Format>(formats.Length);

            foreach(vision.Format format in formats)
            {
                converted.Add(new Format(format));
            }
            return converted;
        }
#endif

        /// <summary>
        /// Drop handler.
        /// </summary>
        /// <remarks>
        /// Disconnects the service from the selected camera if necessary,
        /// before continuing with the default drop process.
        /// </remarks>
        /// <param name="drop"></param>
        [ServiceHandler(ServiceHandlerBehavior.Teardown)]
        [ServiceHandler(ServiceHandlerBehavior.Teardown, PortFieldName = "_altPort")]
        public void DropHandler(DsspDefaultDrop drop)
        {
#if !URT_MINCLR
            ShutdownInternalService();
#endif
            QuitClient();

            base.DefaultDropHandler(drop);
        }

        /// <summary>
        /// Get the service state
        /// </summary>
        /// <remarks>This gets the service state for the main port</remarks>
        /// <param name="get">DSSP Get request</param>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public IEnumerator<ITask> GetHandler(Get get)
        {
            Port<EmptyValue> completion = new Port<EmptyValue>();
            
            SpawnIterator(_state, completion, RefreshCameraList);

            yield return Arbiter.Receive(false, completion, EmptyHandler);

            get.ResponsePort.Post(_state);
        }

        /// <summary>
        /// Get the generic service state
        /// </summary>
        /// <remarks>This gets the generic webcam view of the service state</remarks>
        /// <param name="get">DSSP Get request</param>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent,PortFieldName = "_altPort")]
        public void GenericGetHandler(webcam.Get get)
        {
            get.ResponsePort.Post(_state.ToGenericState());
        }

        /// <summary>
        /// Replace the service state.
        /// </summary>
        /// <remarks>
        /// This message is sent internally within the webcam service
        /// as part of the startup process
        /// </remarks>
        /// <param name="replace">DSSP Replace request</param>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public void ReplaceHandler(Replace replace)
        {
            _state = replace.Body;

            replace.ResponsePort.Post(DefaultReplaceResponseType.Instance);

#if !URT_MINCLR
            SendNotification(_submgrPort, replace);

            if (_streamPort != null)
            {
                _streamPort.Post(new Shutdown());
                _streamPort = null;
            }

            if (!string.IsNullOrEmpty(_state.CaptureFile))
            {
                _streamPort = SaveStream.Create(_state.CaptureFile, _state.Quality, _queue);
            }
#else
            SendNotification(replace);
#endif
        }

        static readonly Guid _formatBmp = new Guid("b96b3cab-0728-11d3-9d7b-0000f81ef32e");
        static readonly Guid _formatGif = new Guid("b96b3cb0-0728-11d3-9d7b-0000f81ef32e");
        static readonly Guid _formatJpeg = new Guid("b96b3cae-0728-11d3-9d7b-0000f81ef32e");
        static readonly Guid _formatPng = new Guid("b96b3caf-0728-11d3-9d7b-0000f81ef32e");

        /// <summary>
        /// Ask the webcam for the current frame.
        /// </summary>
        /// <param name="query">DSSP Query request</param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        [ServiceHandler(ServiceHandlerBehavior.Concurrent, PortFieldName = "_altPort")]
        public IEnumerator<ITask> QueryFrameHandler(webcam.QueryFrame query)
        {
            if (_state.FramesOnDemand)
            {
                var request = new InternalFrameRequest
                {
                    DsspQuery = query
                };
                _frameRequestPort.Post(request);

                yield return Arbiter.Receive(false, request.ResponsePort, EmptyHandler);
            }
            else if (_state.Image == null)
            {
                query.ResponsePort.Post(new webcam.QueryFrameResponse());
            }
            else
            {
                ReplyToQueryFrame(query, _state.Image);
            }
        }

        void ReplyToQueryFrame(webcam.QueryFrame query, Bitmap image)
        {
            Size size = new Size((int)query.Body.Size.X, (int)query.Body.Size.Y);
            bool noresize = (size == _state.Image.Size ||
                                size.Width == 0 ||
                                size.Height == 0 ||
                                size.Width >= _state.Image.Width ||
                                size.Height >= _state.Image.Height);

            if (query.Body.Format == Guid.Empty)
            {
                // raw image requested;
                BitmapData raw = null;
                Bitmap bmp = null;
                try
                {
#if URT_MINCLR
                    noresize = true;
                    bmp = image;
#else
                    if (noresize)
                    {
                        bmp = image;
                    }
                    else
                    {
                        bmp = new Bitmap(image, size);
                    }
#endif
                    raw = bmp.LockBits(
                        new Rectangle(0, 0, bmp.Width, bmp.Height),
                        ImageLockMode.ReadOnly,
                        PixelFormat.Format24bppRgb
                    );

                    int bytesize = raw.Height * Math.Abs(raw.Stride);

                    webcam.QueryFrameResponse response = new webcam.QueryFrameResponse();

                    response.TimeStamp = _state.LastFrameUpdate;
                    byte[] rawImage = new byte[bytesize];
                    response.Size = new Size(raw.Width, raw.Height);
                    response.Format = Guid.Empty;

                    System.Runtime.InteropServices.Marshal.Copy(raw.Scan0, rawImage, 0, bytesize);
                    response.Frame = rawImage;

                    query.ResponsePort.Post(response);
                }
                finally
                {
                    if (raw != null)
                    {
                        bmp.UnlockBits(raw);
                    }
                    if (bmp != null && !noresize)
                    {
                        bmp.Dispose();
                    }
                }
            }
            else
            {
                ImageFormat format;

                if (query.Body.Format == _formatBmp)
                {
                    format = ImageFormat.Bmp;
                }
                else if (query.Body.Format == _formatJpeg)
                {
                    format = ImageFormat.Jpeg;
                }
                else if (query.Body.Format == _formatGif)
                {
                    format = ImageFormat.Gif;
                }
                else if (query.Body.Format == _formatPng)
                {
                    format = ImageFormat.Png;
                }
                else
                {
                    query.ResponsePort.Post(
                        Fault.FromCodeSubcodeReason(
                            FaultCodes.Receiver,
                            DsspFaultCodes.ActionNotSupported,
                            "Unsupported image format"
                        )
                    );
                    return;
                }

                using (MemoryStream stream = new MemoryStream())
                {
                    if (noresize)
                    {
                        size = image.Size;
                        image.Save(stream, format);
                    }
                    else
                    {
#if URT_MINCLR
                        size = _state.Image.Size;
                        _state.Image.Save(stream, format);
#else
                        using (Bitmap temp = new Bitmap(image, size))
                        {
                            temp.Save(stream, format);
                        }
#endif
                    }


                    webcam.QueryFrameResponse response = new webcam.QueryFrameResponse();
                    response.TimeStamp = _state.LastFrameUpdate;
                    response.Frame = new byte[(int)stream.Length];
                    response.Size = size;
                    response.Format = query.Body.Format;

                    stream.Position = 0;
                    stream.Read(response.Frame, 0, response.Frame.Length);

                    query.ResponsePort.Post(response);
                }
            }
        }

        /// <summary>
        /// Handle UpdateFrame processing.
        /// </summary>
        /// <remarks>
        /// <para>The UpdateFrame message is used internally by the webcam service
        /// to indicate that a new frame is available to subscribers.</para>
        /// <para>This message is only used when the state member FramesOnDemand is false</para>
        /// </remarks>
        /// <param name="update">DSSP Update message</param>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        [ServiceHandler(ServiceHandlerBehavior.Exclusive, PortFieldName = "_altPort")]
        public void UpdateFrameHandler(webcam.UpdateFrame update)
        {
            update.ResponsePort.Post(DefaultUpdateResponseType.Instance);

#if !URT_MINCLR
            SendNotification(_submgrPort, update);
#else
            SendNotification(update);
#endif
        }

        /// <summary>
        /// Ask the webcam to change which device it is using.
        /// </summary>
        /// <param name="update">DSSP Update message</param>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public IEnumerator<ITask> UpdateDeviceHandler(UpdateDevice update)
        {
            CameraInstance selected = update.Body.Selected;
            bool updated = false;
            Exception exception = null;

            if (_state.Selected != null && 
                _state.Selected.Started)
            {
                var stopPort = StopCapture();

                yield return (Choice)stopPort;

                _state.Selected.Started = false;

                _state.Selected = null;
                _state.CameraDeviceName = null;
            }


            var devicePort = EnumDevices();
            List<CameraInstance> cameras = null;

            yield return Arbiter.Receive(false, devicePort, list => cameras = list);

            foreach (var camera in cameras)
            {
                if (selected.Equals(camera))
                {
                    var openPort = OpenCamera(camera);

                    yield return (Choice)openPort;

                    exception = openPort;
                    if (exception != null)
                    {
                        LogError("Unable to open device", exception);
                        continue;
                    }

                    var formatsPort = EnumFormats(camera);

                    yield return Arbiter.Receive(false, formatsPort, list => selected.SupportedFormats = list);

                    Format requestFormat = selected.Format ?? new Format();
                    bool setFormat = selected.IsValidFormat(requestFormat);

                    if (setFormat)
                    {
                        var setFormatPort = SetFormat(requestFormat);

                        yield return (Choice)setFormatPort;

                        exception = setFormatPort;
                        if (exception != null)
                        {
                            LogError("Unable to set the requested format", exception);
                            exception = null;
                        }
                    }

                    var startPort = StartCapture();

                    yield return (Choice)startPort;

                    exception = startPort;
                    if (exception != null)
                    {
                        LogError("Unable to start capture", exception);
                        continue;
                    }

                    _state.CameraDeviceName = selected.FriendlyName;
                    updated = true;
                    break;
                }
            }

            if (updated)
            {
                _state.Cameras = cameras;
                _state.Selected = selected;
                _state.Selected.Started = true;
                if (selected.Format != null)
                {
                    _state.ImageSize = new physics.Vector2(selected.Format.Width, selected.Format.Height);
                }

                update.ResponsePort.Post(DefaultUpdateResponseType.Instance);

#if !URT_MINCLR
                SendNotification(_submgrPort, update);
#else
                SendNotification(update);
#endif
            }
            else
            {
                update.ResponsePort.Post(Fault.FromCodeSubcodeReason(
                    W3C.Soap.FaultCodes.Receiver,
                    DsspFaultCodes.UnknownEntry,
                    "Camera not found"
                    )
                );
            }
        }

        /// <summary>
        /// Ask the webcam to change the format of the captured image
        /// </summary>
        /// <remarks>
        /// This is used to change the size or capture rate of the current capture device.
        /// </remarks>
        /// <param name="update">DSSP Update message</param>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public IEnumerator<ITask> UpdateFormatHandler(UpdateFormat update)
        {
            Format format = update.Body;

            if (_state.Selected != null &&
                _state.Selected.Started)
            {
                yield return (Choice)StopCapture();

                yield return (Choice)SetFormat(format);

                yield return (Choice)StartCapture();

                _state.Selected.Format = format;
                if (format.MaxFramesPerSecond > 0)
                {
                    _state.Selected.Format.MaxFramesPerSecond = format.MaxFramesPerSecond;
                }
                if (format.MinFramesPerSecond > 0)
                {
                    _state.Selected.Format.MinFramesPerSecond = format.MinFramesPerSecond;
                }
                _state.ImageSize = new physics.Vector2(format.Width, format.Height);
                update.ResponsePort.Post(DefaultUpdateResponseType.Instance);
            }
            else
            {
                update.ResponsePort.Post(
                    Fault.FromCodeSubcodeReason(
                        W3C.Soap.FaultCodes.Receiver,
                        DsspFaultCodes.OperationFailed,
                        "No camera is currently selected"
                    )
                );
            }
        }

        void OnCaptureFrame(TimeSpan timeStamp, Bitmap bmp)
        {
            Frame frame = new Frame();
            frame.Image = bmp;
            frame.Offset = timeStamp;

            _framePort.Post(frame);
        }

        void OnDemandCaptureFrame(TimeSpan timeStamp, Bitmap bmp)
        {
            if (_frameRequestPort.ItemCount > 0)
            {
                Frame frame = new Frame();
                frame.Image = bmp;
                frame.Offset = timeStamp;

                _framePort.Post(frame);
            }
            else
            {
                bmp.Dispose();
            }
        }

        void FrameHandler(Frame frame)
        {
            Frame previous = new Frame();
            previous.Image = _state.Image;
            previous.TimeStamp = _state.LastFrameUpdate;

            _state.Image = frame.Image;
            _state.ImageSize = new Microsoft.Robotics.PhysicalModel.Vector2(
                _state.Image.Width,
                _state.Image.Height
            );

            if (_state.Selected != null)
            {
                if (_state.Selected.Format == null ||
                    _state.Selected.Format.Width != _state.Image.Width ||
                    _state.Selected.Format.Height != _state.Image.Height)
                {
                    _state.Selected.Format = new Format(
                        _state.Image.Width,
                        _state.Image.Height,
                        0,
                        0,
                        0
                    );
                }
            }

            if (_captureEpoch == DateTime.MinValue)
            {
                _captureEpoch = DateTime.UtcNow - frame.Offset;
            }

            _state.LastFrameUpdate = _captureEpoch + frame.Offset;


            if (previous.Image != null)
            {
#if !URT_MINCLR
                if (_streamPort != null)
                {
                    _streamPort.Post(previous);
                }
                else
#endif
                {
                    previous.Image.Dispose();
                }
            }

            webcam.UpdateFrame update = new webcam.UpdateFrame(new webcam.UpdateFrameRequest(frame.TimeStamp), null);
            _altFwdPort.Post(update);
        }

        IEnumerator<ITask> OnDemandFrameHandler(Frame frame)
        {
            InternalFrameRequest request;

            using (Bitmap bmp = frame.Image)
            {
                using (var stream = new MemoryStream())
                {
                    while (_frameRequestPort.Test(out request))
                    {
                        if (request.IsHttpRequest)
                        {
                            if (stream.Length == 0)
                            {
                                frame.Image.Save(stream, request.Format);
                            }
                            stream.Position = 0;

                            yield return Arbiter.Choice(
                                WriteImageToHttpResponse(request.Context, request.Type, stream),
                                success => stream.Close(),
                                exception => { stream.Close(); LogWarning("Writing image to HTTP: " + exception.Message); }
                            );
                        }
                        else
                        {
                            ReplyToQueryFrame(request.DsspQuery, bmp);
                        }
                        request.ResponsePort.Post(EmptyValue.SharedInstance);
                    }
                }
            }
        }

        /// <summary>
        /// Handles HTTP POST events
        /// </summary>
        /// <param name="post"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public IEnumerator<ITask> HttpPostHandler(HttpPost post)
        {
            Fault fault = null;
            NameValueCollection collection = null;

            ReadFormData readForm = new ReadFormData(post);
            _utilitiesPort.Post(readForm);

            yield return Arbiter.Choice(
                readForm.ResultPort,
                delegate(NameValueCollection col)
                {
                    collection = col;
                },
                delegate(Exception e)
                {
                    fault = Fault.FromException(e);
                    LogError(null, "Error processing form data", fault);
                }
            );

            if (fault != null)
            {
                post.ResponsePort.Post(fault);
                yield break;
            }

            if (!string.IsNullOrEmpty(collection["ChangeCamera"]))
            {
                string device = string.Empty;
                try
                {
                    device = collection["Camera"];
                }
                catch (Exception e)
                {
                    fault = Fault.FromException(e);
                    LogError(null, "Error reading form data", fault);
                }

                if (fault != null)
                {
                    post.ResponsePort.Post(fault);
                    yield break;
                }

                UpdateDeviceRequest request = new UpdateDeviceRequest();
                request.Selected.DevicePath = device;

                UpdateDevice update = new UpdateDevice();
                update.Body = request;

                SpawnIterator(update, UpdateDeviceHandler);

                yield return Arbiter.Choice(
                    update.ResponsePort,
                    delegate(DefaultUpdateResponseType success)
                    {
                        SaveState(_state);
                    },
                    delegate(Fault f)
                    {
                        fault = f;
                        LogError(null, "Unable to change camera", fault);
                    }
                );
            }
            else if (!string.IsNullOrEmpty(collection["ChangeFormat"]))
            {
                int formatIndex = 0;
                Format format = null;
                try
                {
                    formatIndex = int.Parse(collection["CaptureFormat"]);
                    format = _state.Selected.SupportedFormats[formatIndex - 1];
                }
                catch (Exception e)
                {
                    fault = Fault.FromException(e);
                    LogError(null, "Error parsing form data", fault);
                }

                if (fault != null)
                {
                    post.ResponsePort.Post(fault);
                    yield break;
                }

                UpdateFormat update = new UpdateFormat();
                update.Body = format;

                SpawnIterator(update, UpdateFormatHandler);

                yield return Arbiter.Choice(
                    update.ResponsePort,
                    delegate(DefaultUpdateResponseType success)
                    {
                        SaveState(_state);
                    },
                    delegate(Fault f)
                    {
                        fault = f;
                        LogError(null, "Unable to change format", fault);
                    }
                );
            }

            if (fault != null)
            {
                post.ResponsePort.Post(fault);
                yield break;
            }

            post.ResponsePort.Post(new HttpResponseType(HttpStatusCode.OK, _state, _transform));
            yield break;
        }

        /// <summary>
        /// Handles HTTP QUERY events
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        [ServiceHandler(ServiceHandlerBehavior.Concurrent, PortFieldName = "_altPort")]
        public IEnumerator<ITask> HttpQueryHandler(HttpQuery query)
        {
            return HttpHandler(query.Body.Context, query.ResponsePort);
        }

        /// <summary>
        /// Handles HTTP GET requests
        /// </summary>
        /// <param name="get"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        [ServiceHandler(ServiceHandlerBehavior.Concurrent, PortFieldName = "_altPort")]
        public IEnumerator<ITask> HttpGetHandler(HttpGet get)
        {
            return HttpHandler(get.Body.Context, get.ResponsePort);
        }

        IEnumerator<ITask> HttpHandler(HttpListenerContext context, PortSet<HttpResponseType,Fault> responsePort)
        {
            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;

            bool alternate = false;

            string path = request.Url.AbsolutePath.ToLower(ci.InvariantCulture);
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
#if !URT_MINCLR
                case "/tif":
                case "/tiff":
                    type = "image/tiff";
                    format = ImageFormat.Tiff;
                    break;
#endif
                case "/gif":
                    type = MediaTypeNames.Image.Gif;
                    format = ImageFormat.Gif;
                    break;
                default:
                    if (alternate)
                    {
                        responsePort.Post(new HttpResponseType(HttpStatusCode.OK, _state.ToGenericState(), _transform));
                    }
                    else if (path == "/log")
                    {
                        var log = new PipeServerOutput();
                        lock (_pipeServerOutput)
                        {
                            log.Output = new List<string>(_pipeServerOutput);
                        }
                        responsePort.Post(new HttpResponseType(log));
                    }
                    else
                    {
                        responsePort.Post(new HttpResponseType(HttpStatusCode.OK, _state, _transform));
                    }
                    yield break;
            }

            if (_state.Image == null)
            {
                if (_state.FramesOnDemand)
                {
                    if (_state.Selected != null &&
                        _state.Selected.Started)
                    {
                        var internalRequest = new InternalFrameRequest
                        {
                            Context = context,
                            Type = type,
                            Format = format
                        };

                        _frameRequestPort.Post(internalRequest);

//                        _state.Selected.FrameGrabber.ResumeCapture();

                        yield return Arbiter.Receive(false, internalRequest.ResponsePort, EmptyHandler);

//                        _state.Selected.FrameGrabber.PauseCapture();
                    }
                }
                else if (alternate)
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

                yield return Arbiter.Choice(
                    WriteImageToHttpResponse(context, type, stream),
                        success => stream.Close(),
                        exception => { stream.Close(); LogWarning("Writing image to stream:" + exception.Message); }
                );
            }
        }

        private PortSet<Stream, Exception> WriteImageToHttpResponse(HttpListenerContext context, string type, Stream stream)
        {
            var response = context.Response;

            response.AddHeader("Cache-Control", "No-cache");

            WriteResponseFromStream write = new WriteResponseFromStream(
                context, stream, type
            );

            _utilitiesPort.Post(write);

            return write.ResultPort;
        }

        /// <summary>
        /// Handles incoming subscribe requests
        /// </summary>
        /// <param name="subscribe">DSSP Subscribe request</param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        [ServiceHandler(ServiceHandlerBehavior.Concurrent, PortFieldName = "_altPort")]
        public IEnumerator<ITask> SubscribeHandler(webcam.Subscribe subscribe)
        {
#if !URT_MINCLR
            yield return Arbiter.Choice(
                SubscribeHelper(_submgrPort, subscribe.Body, subscribe.ResponsePort),
                delegate(SuccessResult success)
                {
                    LogInfo("Subscription from: " + subscribe.Body.Subscriber);
                },
                delegate(Exception e)
                {
                    LogError(null, "Subscription failed", e);
                }
            );
#else
            string key = subscribe.Body.Subscriber.ToLower();

            if (!_subscribers.ContainsKey(key))
            {
                Port<DsspOperation> notificationPort = ServiceForwarderUnknownType(new Uri(subscribe.Body.Subscriber));
                _subscribers.Add(key, notificationPort);
            }

            subscribe.ResponsePort.Post(new SubscribeResponseType());
            yield break;
#endif
        }

#if URT_MINCLR
        #region Subscription Manager


        /// <summary>
        /// Sends a message to the subscription manager to send a notification.
        /// </summary>
        /// <typeparam name="T">The message type to send as a notification</typeparam>
        /// <param name="notificationBody">Message body to send in the notification</param>
        protected void SendNotification<T>(
            object notificationBody)
            where T : DsspOperation, new()
        {
            SendNotification<T>(string.Empty, notificationBody);
        }

        /// <summary>
        /// Sends a message to the subscription manager to send a notification.
        /// </summary>
        /// <typeparam name="T">The message type to send as a notification</typeparam>
        /// <param name="subscriber">Address of a subscriber to notify. If this is null or Empty then all subscribers are notified</param>
        /// <param name="notificationBody">Message body to send in the notification</param>
        protected void SendNotification<T>(
            string subscriber,
            object notificationBody)
            where T : DsspOperation, new()
        {
            T msg = new T();
            Type msgType = msg.GetType();
            Type[] genericTypes = msgType.GetGenericArguments();

            if (genericTypes.Length >= 2 &&
                genericTypes[0] != notificationBody.GetType())
            {
                throw new ArgumentException(
                    "Incorrect notification type, expecting: " + genericTypes[0].FullName,
                    "notificationBody");
            }

            msg.Body = notificationBody;

            SendNotification<T>(subscriber, msg);
        }

        /// <summary>
        /// Send Notification (without SubMgr)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="notification"></param>
        private void SendNotification<T>(T notification) where T : DsspOperation, new()
        {
            notification.ConvertToNotification();
            foreach (Port<DsspOperation> notificationPort in _subscribers.Values)
                notificationPort.PostUnknownType(notification);
        }

        /// <summary>
        /// Sends a message to the subscription manager to send a notification.
        /// </summary>
        /// <typeparam name="T">The message type to send as a notification</typeparam>
        /// <param name="subscriber">Address of a subscriber to notify. If this is null or Empty then all subscribers are notified</param>
        /// <param name="notification">Message to send as a notification</param>
        protected void SendNotification<T>(
            string subscriber,
            T notification)
            where T : DsspOperation
        {
            if (notification.Action != DsspActions.DeleteRequest &&
                notification.Action != DsspActions.InsertRequest &&
                notification.Action != DsspActions.ReplaceRequest &&
                notification.Action != DsspActions.UpdateRequest &&
                notification.Action != DsspActions.UpsertRequest)
            {
                throw new ArgumentException(
                    notification.Action + " is not a state modifying verb",
                    notification.GetType().FullName);
            }

            notification.ConvertToNotification();
            if (string.IsNullOrEmpty(subscriber))
            {
                foreach (Port<DsspOperation> notificationPort in _subscribers.Values)
                    notificationPort.PostUnknownType(notification);
            }
            else
            {
                string key = subscriber.ToLower();
                if (!_subscribers.ContainsKey(key))
                {
                    Port<DsspOperation> notificationPort = _subscribers[key];
                    if (notificationPort != null)
                        notificationPort.PostUnknownType(notification);
                }
            }
        }

        #endregion
#endif

    }

    class Frame
    {
        public Bitmap Image { get; set; }
        public TimeSpan Offset { get; set; }
        public DateTime TimeStamp { get; set; }
    }

    class FramePort : Port<Frame> { }
    
    class FrameRequestPort : Port<InternalFrameRequest> { }
    
    class InternalFrameRequest
    {
        public HttpListenerContext Context;
        public string Type;
        public ImageFormat Format;
        public Port<EmptyValue> ResponsePort = new Port<EmptyValue>();
        public webcam.QueryFrame DsspQuery;

        public bool IsHttpRequest
        {
            get { return Context != null; }
        }
    }
}
