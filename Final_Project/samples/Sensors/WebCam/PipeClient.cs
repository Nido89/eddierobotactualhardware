//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: PipeClient.cs $ $Revision: 4 $
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Text;

using Microsoft.Ccr.Core;
using Microsoft.Dss.Core;
using Microsoft.Dss.ServiceModel.DsspServiceBase;
using System.Threading;

namespace Microsoft.Robotics.Services.MultiDeviceWebCam
{
    public partial class WebCamService : DsspServiceBase
    {
        static string _pipeName;
        static Process _pipeServer;
        static object _serverLock = new object();

        NamedPipeClientStream _client;
        bool _framesOnDemand = false;
        int _shutdown = 0;
        FormatResponse _format;
        PortSet<HresultResponse, EnumResponse, FormatResponse> _pipeDataPort = new PortSet<HresultResponse, EnumResponse, FormatResponse>();

        private void StartPipeServer()
        {
            lock (_serverLock)
            {
                //
                // check if an existing server process has exited.
                //
                if (_pipeServer != null &&
                    _pipeServer.HasExited)
                {
                    _pipeServer.Close();
                    _pipeServer = null;
                    _pipeName = null;
                }

                if (_pipeServer == null || string.IsNullOrEmpty(_pipeName))
                {
                    _pipeName = Guid.NewGuid().ToString("D");
                    var filename = Path.Combine(LayoutPaths.LocalBinPath, "WebcamPipeServer.exe");

                    var process = new Process();
                    process.StartInfo = new ProcessStartInfo();
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.FileName = filename;
                    process.StartInfo.Arguments = _pipeName;
                    process.StartInfo.RedirectStandardError = true;
                    process.StartInfo.RedirectStandardOutput = true;

                    process.ErrorDataReceived += OnPipeServerOutput;
                    process.OutputDataReceived += OnPipeServerOutput;

                    process.Start();
                    process.BeginErrorReadLine();
                    process.BeginOutputReadLine();

                    _pipeServer = process;
                }
            }
            _client = new NamedPipeClientStream(".", _pipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
            _client.Connect();

            var exceptions = new Port<Exception>();

            Dispatcher.AddCausality(new Causality("webcam pipe", exceptions));
            Activate(
                Arbiter.Receive(true, exceptions, e => LogError(null, "Unhandled exception processing pipe data", e))
            );

            SpawnIterator(_client, ProcessPipe);
        }

        List<string> _pipeServerOutput = new List<string>();
        void OnPipeServerOutput(object sender, DataReceivedEventArgs args)
        {
            if (!string.IsNullOrEmpty(args.Data))
            {
                lock (_pipeServerOutput)
                {
                    _pipeServerOutput.Add(args.Data);
                }
            }
        }

        IEnumerator<ITask> ProcessPipe(NamedPipeClientStream client)
        {
            Port<IAsyncResult> continuation = new Port<IAsyncResult>();

            for (; ; )
            {
                IAsyncResult ar = null;
                var response = new WebcamResponse();

                client.BeginRead(response.header,
                    0,
                    WebcamResponse.HeaderSize,
                    continuation.Post,
                    null
                );

                yield return Arbiter.Receive(false, continuation, result => ar = result);

                var length = client.EndRead(ar);
                if (length != 8)
                {
                    if (0 < Interlocked.CompareExchange(ref _shutdown, 1, 1))
                    {
                        LogInfo("Pipe has been closed");
                        client = null;
                    }
                    else
                    {
                        LogError("Bad header length read from pipe");
                    }
                    yield break;
                }

                response.packet = new byte[response.packetSize];

                client.BeginRead(response.packet,
                    0,
                    response.packetSize,
                    continuation.Post,
                    null
                );

                yield return Arbiter.Receive(false, continuation, result => ar = result);

                length = client.EndRead(ar);
                if (length != response.packetSize)
                {
                    if (0 < Interlocked.CompareExchange(ref _shutdown, 1, 1))
                    {
                        LogInfo("Pipe has been closed");
                        client = null;
                    }
                    else
                    {
                        LogError("Bad packet length read from pipe");
                    }
                    yield break;
                }

                if (response.IsKnownType)
                {
                    var item = response.KnownType;

                    if (item is EnumResponse)
                    {
                        _pipeDataPort.Post((EnumResponse)item);
                    }
                    else if (item is FormatResponse)
                    {
                        var format = (FormatResponse)item;

                        if (format.Index == -1)
                        {
                            _format = format;
                            if (_state.Selected != null)
                            {
                                if (_state.Selected.Format == null)
                                {
                                    _state.Selected.Format = new Format();
                                }
                                _state.Selected.Format.Width = _format.Width;
                                _state.Selected.Format.Height = _format.Height;
                            }
                        }
                        else
                        {
                            _pipeDataPort.Post(format);
                        }
                    }
                    else if (item is HresultResponse)
                    {
                        _pipeDataPort.Post((HresultResponse)item);
                    }
                }
                else if (response.IsFrame)
                {
                    ProcessFrame(response);
                }
            }
        }

        void ProcessFrame(WebcamResponse frame)
        {
            if (_format == null)
            {
                return;
            }

            var timeStamp = new TimeSpan(
                BitConverter.ToInt64(frame.packet, 0)
            );

            var bmp = new Bitmap(_format.Width, _format.Height, PixelFormat.Format24bppRgb);
            var data = bmp.LockBits(
                new Rectangle(0, 0, bmp.Width, bmp.Height),
                ImageLockMode.ReadWrite,
                PixelFormat.Format24bppRgb
            );

            var dataSize = data.Stride * data.Height;

            if (frame.packetSize - 8 == dataSize)
            {
                Marshal.Copy(frame.packet, 8, data.Scan0, dataSize);
            }

            bmp.UnlockBits(data);

            if (_framesOnDemand)
            {
                OnDemandCaptureFrame(timeStamp, bmp);
            }
            else
            {
                OnCaptureFrame(timeStamp, bmp);
            }
        }

        Port<List<CameraInstance>> EnumDevices()
        {
            new EnumRequest().Send(_client);

            Port<List<CameraInstance>> resultPort = new Port<List<CameraInstance>>();

            SpawnIterator(resultPort, WaitForEnumResponses);

            return resultPort;
        }

        IEnumerator<ITask> WaitForEnumResponses(Port<List<CameraInstance>> resultPort)
        {
            List<CameraInstance> cameras = new List<CameraInstance>();

            for (; ; )
            {
                EnumResponse response = null;
                HresultResponse done = null;

                yield return Arbiter.Choice(
                    Arbiter.Receive<EnumResponse>(false, _pipeDataPort, success => response = success),
                    Arbiter.Receive<HresultResponse>(false, _pipeDataPort, hresult => done = hresult)
                );

                if (done != null)
                {
                    if (done.type == WebcamResponse.EnumerateDevices)
                    {
                        break;
                    }
                }
                else
                {
                    var camera = new CameraInstance();

                    camera.DevicePath = response.device;
                    camera.FriendlyName = response.name;

                    cameras.Add(camera);
                }
            }

            foreach (var camera in cameras)
            {
                var formatPort = EnumFormats(camera);

                yield return Arbiter.Receive(false, formatPort, success => camera.SupportedFormats = success);
            }

            resultPort.Post(cameras);
        }

        Port<List<Format>> EnumFormats(CameraInstance camera)
        {
            Port<List<Format>> resultPort = new Port<List<Format>>();

            SpawnIterator(camera, resultPort, DoEnumFormats);

            return resultPort;
        }

        IEnumerator<ITask> DoEnumFormats(CameraInstance camera, Port<List<Format>> resultPort)
        {
            List<Format> formats = new List<Format>();

            new OpenRequest(camera.DevicePath).Send(_client);

            HresultResponse hr = null;

            do
            {
                yield return Arbiter.Receive<HresultResponse>(false, _pipeDataPort, success => hr = success);
            } while (hr.type != WebcamResponse.OpenDevices);

            new FormatRequest().Send(_client);

            for (; ; )
            {
                FormatResponse response = null;
                HresultResponse done = null;

                yield return Arbiter.Choice(
                    Arbiter.Receive<FormatResponse>(false, _pipeDataPort, success => response = success),
                    Arbiter.Receive<HresultResponse>(false, _pipeDataPort, hresult => done = hresult)
                );

                if (done != null)
                {
                    if (done.type == WebcamResponse.EnumeratFormats)
                    {
                        break;
                    }
                }
                else
                {
                    var format = new Format(
                        response.Width,
                        response.Height,
                        0,
                        0,
                        response.Compression
                    );
                        
                    formats.Add(format);
                }
            }


            resultPort.Post(formats);
        }

        SuccessFailurePort OpenCamera(CameraInstance camera)
        {
            SuccessFailurePort resultPort = new SuccessFailurePort();

            new OpenRequest(camera.DevicePath).Send(_client);

            Activate(
                Arbiter.Receive<HresultResponse>(false,
                    _pipeDataPort,
                    delegate(HresultResponse result)
                    {
                        if (result.hr == 0)
                        {
                            resultPort.Post(SuccessResult.Instance);
                        }
                        else
                        {
                            resultPort.Post(Marshal.GetExceptionForHR(result.hr));
                        }
                    },
                    test => test.type == WebcamResponse.OpenDevices
                )
            );

            return resultPort;
        }

        SuccessFailurePort StartCapture()
        {
            SuccessFailurePort resultPort = new SuccessFailurePort();

            new StartRequest().Send(_client);

            Activate(
                Arbiter.Receive<HresultResponse>(false,
                    _pipeDataPort,
                    delegate(HresultResponse result)
                    {
                        if (result.hr == 0)
                        {
                            resultPort.Post(SuccessResult.Instance);
                        }
                        else
                        {
                            resultPort.Post(Marshal.GetExceptionForHR(result.hr));
                        }
                    },
                    test => test.type == WebcamResponse.StartCapture
                )
            );

            return resultPort;
        }

        SuccessFailurePort StopCapture()
        {
            SuccessFailurePort resultPort = new SuccessFailurePort();

            new StopRequest().Send(_client);

            Activate(
                Arbiter.Receive<HresultResponse>(false,
                    _pipeDataPort,
                    delegate(HresultResponse result)
                    {
                        if (result.hr == 0)
                        {
                            resultPort.Post(SuccessResult.Instance);
                        }
                        else
                        {
                            resultPort.Post(Marshal.GetExceptionForHR(result.hr));
                        }
                    },
                    test => test.type == WebcamResponse.StopCapture
                )
            );

            return resultPort;
        }

        SuccessFailurePort SetFormat(Format format)
        {
            SuccessFailurePort resultPort = new SuccessFailurePort();

            var setFormat = new SetFormatRequest
            {
                Width = format.Width,
                Height = format.Height,
                MinFrameRate = format.MinFramesPerSecond,
                MaxFrameRate = format.MaxFramesPerSecond,
                Compression = format.FourCcCompression
            };


            setFormat.Send(_client);

            Activate(
                Arbiter.Receive<HresultResponse>(false,
                    _pipeDataPort,
                    delegate(HresultResponse result)
                    {
                        if (result.hr == 0)
                        {
                            resultPort.Post(SuccessResult.Instance);
                        }
                        else
                        {
                            resultPort.Post(Marshal.GetExceptionForHR(result.hr));
                        }
                    },
                    test => test.type == WebcamResponse.SetFormat
                )
            );

            return resultPort;
        }

        void QuitClient()
        {
            if (1 == Interlocked.Increment(ref _shutdown))
            {
                new QuitRequest().Send(_client);
                _client.Close();
                _client = null;
            }
        }
    }

    class WebcamResponse
    {
        public const int HeaderSize = 8;
        public byte[] header = new byte[HeaderSize];

        public int packetSize
        {
            get
            {
                if (packet == null)
                {
                    UInt32 totalSize = (UInt32)header[4] +
                            (UInt32)(header[5] << 8) +
                            (UInt32)(header[6] << 16) +
                            (UInt32)(header[7] << 24);

                    if (totalSize < 8 || totalSize >= 0x80000000)
                    {
                        throw new InvalidDataException("Packet size is invalid");
                    }
                    return (int)(totalSize - 8);
                }
                else
                {
                    return packet.Length;
                }
            }
        }

        public string type
        {
            get
            {
                return Encoding.ASCII.GetString(header, 0, 4);
            }
        }

        public byte[] packet;

        public bool IsKnownType
        {
            get
            {
                if (packetSize == 4)
                {
                    return true;
                }
                return (type == WebcamResponse.EnumerateDevices ||
                        type == WebcamResponse.EnumeratFormats);
            }
        }

        public bool IsFrame
        {
            get
            {
                return type == WebcamResponse.Sample;
            }
        }

        public object KnownType
        {
            get
            {
                if (packetSize == 4)
                {
                    return HresultResponse.FromResponse(this);
                }
                switch (type)
                {
                    case WebcamResponse.EnumerateDevices:
                        return EnumResponse.FromResponse(this);

                    case WebcamResponse.EnumeratFormats:
                        return FormatResponse.FromResponse(this);

                    default:
                        break;
                }
                return null;
            }
        }

        public override string ToString()
        {
            return string.Format(
                "{0} : {1}", type, packetSize
            );
        }

        public const string Quit = "QUIT";
        public const string EnumerateDevices = "ENUM";
        public const string OpenDevices = "OPEN";
        public const string StartCapture = "STRT";
        public const string StopCapture = "STOP";
        public const string SetFormat = "SETF";
        public const string Sample = "SMPL";
        public const string EnumeratFormats = "FRMT";


    }

    abstract class StandardRequest
    {
        public const int HeaderSize = 8;

        public void Send(Stream stream)
        {
            byte[] buffer = Serialize();

            stream.Write(buffer, 0, buffer.Length);
        }

        protected int SetDword(byte[] buffer, int offset, int dword)
        {
            var array = BitConverter.GetBytes(dword);

            array.CopyTo(buffer, offset);

            return array.Length;
        }

        protected int CreateHeader(byte[] buffer, string type, int bufferSize)
        {
            if (string.IsNullOrEmpty(type) || type.Length != 4)
            {
                throw new ArgumentException("type must be 4 characters long");
            }

            var typeData = Encoding.ASCII.GetBytes(type);
            typeData.CopyTo(buffer, 0);

            SetDword(buffer, 4, bufferSize);

            return HeaderSize;
        }

        abstract public string Tag { get; }
        abstract protected byte[] Serialize();
    }

    class HresultResponse
    {
        public string type;
        public int hr;

        public static HresultResponse FromResponse(WebcamResponse response)
        {
            var start = new HresultResponse();

            start.type = response.type;
            start.hr = BitConverter.ToInt32(response.packet, 0);

            return start;
        }

        public override string ToString()
        {
            var err = Marshal.GetExceptionForHR(hr);
            if (err != null)
            {
                return type + " : " + err.Message;
            }
            else
            {
                return type + " : S_OK";
            }
        }
    }

    class EnumRequest : StandardRequest
    {
        public override string Tag
        {
            get { return WebcamResponse.EnumerateDevices; }
        }

        protected override byte[] Serialize()
        {
            byte[] serialized = new byte[HeaderSize];

            CreateHeader(serialized, Tag, serialized.Length);

            return serialized;
        }
    }

    class EnumResponse
    {
        public int index;
        public string name;
        public string device;

        public static EnumResponse FromResponse(WebcamResponse response)
        {
            var curr = new EnumResponse();

            var packet = response.packet;
            var offset = 0;

            curr.index = BitConverter.ToInt32(packet, offset);
            offset += 4;

            var nLen = BitConverter.ToInt32(packet, offset);
            offset += 4;

            curr.name = Encoding.Unicode.GetString(packet, offset, nLen);
            offset += nLen;

            var nDevice = BitConverter.ToInt32(packet, offset);
            offset += 4;

            curr.device = Encoding.Unicode.GetString(packet, offset, nDevice);
            offset += nDevice;

            return curr;
        }

        public override string ToString()
        {
            return string.Format("ENUM : {0} : {1} : {2}", index, name, device);
        }
    }

    class OpenRequest : StandardRequest
    {
        public string Device;

        public OpenRequest(string device)
        {
            Device = device;
        }

        public override string Tag
        {
            get { return WebcamResponse.OpenDevices; }
        }

        protected override byte[] Serialize()
        {
            byte[] device = Encoding.Unicode.GetBytes(Device);

            byte[] serialized = new byte[HeaderSize + 4 + device.Length];

            var offset = CreateHeader(serialized, Tag, serialized.Length);

            offset += SetDword(serialized, offset, device.Length);

            device.CopyTo(serialized, offset);

            return serialized;
        }
    }

    class QuitRequest : StandardRequest
    {
        const string _tag = WebcamResponse.Quit;

        public override string Tag
        {
            get { return _tag; }
        }

        protected override byte[] Serialize()
        {
            byte[] serialized = new byte[HeaderSize];

            CreateHeader(serialized, Tag, serialized.Length);

            return serialized;
        }
    }

    class StartRequest : StandardRequest
    {
        public override string Tag
        {
            get { return WebcamResponse.StartCapture; }
        }

        protected override byte[] Serialize()
        {
            byte[] serialized = new byte[HeaderSize];

            CreateHeader(serialized, Tag, serialized.Length);

            return serialized;
        }
    }

    class StopRequest : StandardRequest
    {
        public override string Tag
        {
            get { return WebcamResponse.StopCapture; }
        }

        protected override byte[] Serialize()
        {
            byte[] serialized = new byte[HeaderSize];

            CreateHeader(serialized, Tag, serialized.Length);

            return serialized;
        }
    }

    class FormatRequest : StandardRequest
    {
        public override string Tag
        {
            get { return WebcamResponse.EnumeratFormats; }
        }

        protected override byte[] Serialize()
        {
            byte[] serialized = new byte[HeaderSize];

            CreateHeader(serialized, Tag, serialized.Length);

            return serialized;
        }
    }

    class FormatResponse
    {
        public int Index;
        public int Width;
        public int Height;
        public UInt32 Compression;

        public static FormatResponse FromResponse(WebcamResponse response)
        {
            var format = new FormatResponse();

            format.Index = BitConverter.ToInt32(response.packet, 0);
            format.Width = BitConverter.ToInt32(response.packet, 8);
            format.Height = BitConverter.ToInt32(response.packet, 12);
            format.Compression = BitConverter.ToUInt32(response.packet, 20);

            return format;
        }

        public override string ToString()
        {
            return string.Format("FRMT : {0} : {1}x{2}",
                Index,
                Width,
                Height
            );
        }
    }

    class SetFormatRequest : StandardRequest
    {
        public int Width;
        public int Height;
        public int MinFrameRate;
        public int MaxFrameRate;
        public uint Compression;

        public SetFormatRequest() 
        { 
        }

        public SetFormatRequest(FormatResponse format)
        {
            Width = format.Width;
            Height = format.Height;
            MinFrameRate = 0;
            MaxFrameRate = 0;
            Compression = format.Compression;
        }

        public override string Tag
        {
            get { return WebcamResponse.SetFormat; }
        }

        protected override byte[] Serialize()
        {
            byte[] serialized = new byte[HeaderSize + 20];

            var offset = CreateHeader(serialized, Tag, serialized.Length);

            offset += SetDword(serialized, offset, Width);
            offset += SetDword(serialized, offset, Height);
            offset += SetDword(serialized, offset, MinFrameRate);
            offset += SetDword(serialized, offset, MaxFrameRate);
            offset += SetDword(serialized, offset, (int)Compression);

            return serialized;
        }
    }

}
