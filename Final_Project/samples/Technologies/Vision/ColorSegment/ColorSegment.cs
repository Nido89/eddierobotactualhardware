//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: ColorSegment.cs $ $Revision: 5 $
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
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
using submgr = Microsoft.Dss.Services.SubscriptionManager;

namespace Microsoft.Robotics.Services.Sample.ColorSegment
{
    [DisplayName("(User) ColorSegment")]
    [Description("The ColorSegment Service")]
    [Contract(Contract.Identifier)]
    [DssServiceDescription("http://msdn.microsoft.com/library/cc998458.aspx")]
    public class ColorSegmentService : DsspServiceBase
    {
        #region State, Ports, Partners etc

        [InitialStatePartner(Optional = true, ServiceUri = ServicePaths.Store + "/ColorSegment.user.config.xml")]
        ColorSegmentState _state = new ColorSegmentState();

        [ServicePort("/colorsegment", AllowMultipleInstances=false)]
        ColorSegmentOperations _mainPort = new ColorSegmentOperations();
        ColorSegmentOperations _fwdPort;

        [Partner("WebCam", Contract = webcam.Contract.Identifier, CreationPolicy = PartnerCreationPolicy.UsePartnerListEntry)]
        webcam.WebCamOperations _webcamPort = new webcam.WebCamOperations();
        webcam.WebCamOperations _webcamNotify = new webcam.WebCamOperations();

        [Partner("SubMgr", Contract = submgr.Contract.Identifier, CreationPolicy = PartnerCreationPolicy.CreateAlways)]
        submgr.SubscriptionManagerPort _submgrPort = new submgr.SubscriptionManagerPort();
        const string _externalFilter = "External";
        const string _internalFilter = "Internal";

        DsspHttpUtilitiesPort _utilitiesPort = new DsspHttpUtilitiesPort();

        [EmbeddedResource("Microsoft.Robotics.Services.Sample.ColorSegment.ColorSegment.user.xslt")]
        string _transform = null;

        #endregion

        #region Construction and Start

        public ColorSegmentService(DsspServiceCreationPort creationPort) :
                base(creationPort)
        {
        }

        protected override void Start()
        {
            if (_state == null)
            {
                _state = new ColorSegmentState();
            }
            else
            {
                for (int set = 0; set < _state.Colors.Count; set++)
                {
                    ColorSet colorSet = _state.Colors[set];

                    for (int index = 0; index < colorSet.Colors.Count; index++)
                    {
                        ColorDefinition color = colorSet.Colors[index];
                        if (!color.Validate())
                        {
                            colorSet.Colors.RemoveAt(index);
                            index--;
                        }
                    }

                    if (colorSet.Colors.Count == 0)
                    {
                        _state.Colors.RemoveAt(set);
                        set--;
                    }
                }
                _state.UpdateColorSetMap();
                _state.ImageSource = null;
                _state.Processing = false;
                _state.FrameCount = 0;
                _state.DroppedFrames = 0;
                _state.FoundColorAreas = null;
            }

            _utilitiesPort = DsspHttpUtilitiesService.Create(Environment);

            _fwdPort = ServiceForwarder<ColorSegmentOperations>(ServiceInfo.Service);

            base.Start();

            Activate<ITask>(
                Arbiter.Receive<webcam.UpdateFrame>(true, _webcamNotify, OnWebcamUpdateFrame)
            );
            _webcamPort.Subscribe(_webcamNotify, typeof(webcam.UpdateFrame));

        }

        #endregion

        #region Webcam Notification Handlers

        void OnWebcamUpdateFrame(webcam.UpdateFrame updateFrame)
        {
            _fwdPort.Post(new ProcessFrame(true));
        }

        #endregion

        #region DSSP fundementals

        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public void OnGet(Get get)
        {
            get.ResponsePort.Post(_state.SmallCopy);
        }

        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public IEnumerator<ITask> OnSubscribe(Subscribe subscribe)
        {
            yield return Arbiter.Choice(
                SubscribeHelper(_submgrPort, subscribe.Body, subscribe.ResponsePort),
                EmptyHandler,
                EmptyHandler
            );
        }

        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public IEnumerator<ITask> OnFilteredSubscribe(FilteredSubscribe subscribe)
        {
            FilteredSubscribeRequest request = subscribe.Body;

            submgr.InsertSubscriptionMessage message = new submgr.InsertSubscriptionMessage(request);
            if (request.Filter != Filter.None && request.Filter != Filter.All)
            {
                List<submgr.QueryType> queries = new List<submgr.QueryType>();
                for (int i = 1; i < (int)Filter.All; i <<= 1)
                {
                    Filter filter = (Filter)i;
                    if ((request.Filter & filter) == filter)
                    {
                        queries.Add(new submgr.QueryType(filter.ToString()));
                    }
                }
                message.QueryList = queries.ToArray();
            }
            submgr.InsertSubscription insert = new submgr.InsertSubscription(message);

            yield return Arbiter.Choice(
                insert.ResponsePort,
                subscribe.ResponsePort.Post,
                subscribe.ResponsePort.Post
            );
        }

        #endregion

        #region Color Segment Operation handlers

        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public IEnumerator<ITask> OnProcessFrame(ProcessFrame processFrame)
        {
            if (_state.ImageSource == null)
            {
                yield return Arbiter.Choice(
                    _webcamPort.DsspDefaultLookup(),
                    delegate(LookupResponse response)
                    {
                        _state.ImageSource = response.HttpServiceAlias;
                    },
                    EmptyHandler
                );

            }

            if (processFrame.Body.Process)
            {
                if (_state.Processing)
                {
                    _state.DroppedFrames++;
                }
                else if (_state.Colors.Count > 0)
                {
                    _state.Processing = true;
                    _state.FrameCount++;

                    SpawnIterator(VisionProcessingParameters.FromState(_state), DoVisionProcessing);
                }
                else
                {
                    _state.SegmentedImage = null;
                }
            }
            else
            {
                _state.Processing = false;
            }

            processFrame.ResponsePort.Post(DefaultUpdateResponseType.Instance);

            SendNotification(_submgrPort, processFrame, Filter.Internal.ToString());

            yield break;
        }

        class VisionProcessingParameters
        {
            List<ColorSet> _colors = new List<ColorSet>();
            Settings _settings;
            int[] _colorSetMap;

            public static VisionProcessingParameters FromState(ColorSegmentState state)
            {
                VisionProcessingParameters parameters = new VisionProcessingParameters();

                foreach (ColorSet set in state.Colors)
                {
                    ColorSet clone = new ColorSet();
                    clone.Name = set.Name;
                    clone.Colors = new List<ColorDefinition>(set.Colors);
                    parameters._colors.Add(clone);
                }
                parameters._settings = (Settings)state.Settings.Clone();
                parameters._colorSetMap = (int[])state.ColorSetMap.Clone();

                return parameters;
            }

            public List<ColorSet> Colors { get { return _colors; } }
            public Settings Settings { get { return _settings; } }
            public int[] ColorSetMap { get { return _colorSetMap; } }
        }

        IEnumerator<ITask> DoVisionProcessing(VisionProcessingParameters parameters)
        {
            try
            {
                List<ColorSet> colors = parameters.Colors;
                Settings settings = parameters.Settings;
                int[] colorSetMap = parameters.ColorSetMap;

                webcam.QueryFrameResponse response = null;

                yield return Arbiter.Choice(
                    _webcamPort.QueryFrame(),
                    delegate(webcam.QueryFrameResponse success)
                    {
                        response = success;
                    },
                    LogError
                );

                if (response == null)
                {
                    yield break;
                }

                double threshold = settings.Threshold;

                int height = response.Size.Height;
                int width = response.Size.Width;
                int offset;
                int stride = response.Frame.Length / height;
                byte[] bytes = response.Frame;
                byte[,] indexed = new byte[width, height];
                int segOffset;
                int segStride = width;
                byte[] segmented = new byte[width * height];

                for (int y = 0; y < height; y++)
                {
                    offset = stride * y;
                    segOffset = segStride * y;

                    for (int x = 0; x < width; x++)
                    {
                        int blu = bytes[offset++];
                        int grn = bytes[offset++];
                        int red = bytes[offset++];

                        int col = (((red + 7) & 0xF0) << 4) |
                            ((grn + 7) & 0xF0) |
                            (((blu + 7) & 0xF0) >> 4);

                        int bestIndex = colorSetMap[col];

                        indexed[x, y] = (byte)bestIndex;
                        segmented[segOffset++] = (byte)bestIndex;
                    }
                }

                if (settings.Despeckle)
                {
                    int[] votes;
                    byte[,] despeckled = new byte[width, height];

                    offset = 0;
                    for (int y = 0; y < height; y++)
                    {
                        offset = y * stride;

                        for (int x = 0; x < width; x++, offset++)
                        {
                            votes = new int[colors.Count + 1];
                            int color;
                            int leader = 0;

                            if (x == 0 || y == 0 ||
                                x == width - 1 || y == height - 1)
                            {
                                leader = indexed[x, y];
                                votes[leader]++;
                            }
                            else
                            {
                                for (int iy = -1; iy < 2; iy++)
                                {
                                    for (int ix = -1; ix < 2; ix++)
                                    {
                                        color = indexed[x + ix, y + iy];
                                        votes[color]++;
                                        if (votes[color] > votes[leader])
                                        {
                                            leader = color;
                                        }
                                    }
                                }

                                //
                                // if there is a tie, then use the original color.
                                //
                                int bestVote = votes[leader];
                                for (int i = 0; i < votes.Length; i++)
                                {
                                    if (i != leader &&
                                        votes[i] == bestVote)
                                    {
                                        leader = indexed[x, y];
                                        break;
                                    }
                                }
                            }

                            despeckled[x, y] = (byte)leader;
                        }
                    }

                    indexed = despeckled;
                }

                List<ColorArea> areas = new List<ColorArea>();
                byte[,] copy = (byte[,])indexed.Clone();

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int curr = copy[x, y];

                        if (curr != 0)
                        {
                            ColorArea area = new ColorArea(colors[curr - 1].Name);
                            area.Flood(copy, x, y);

                            areas.Add(area);
                        }
                    }
                }

                areas = areas.FindAll(
                    delegate(ColorArea test)
                    {
                        return test.Area >= settings.MinBlobSize;
                    }
                );

                areas.Sort(
                    delegate(ColorArea left, ColorArea right)
                    {
                        return -left.Area.CompareTo(right.Area);
                    }
                );

                Color[] outputColors = new Color[colors.Count + 1];

                for (int i = 0; i < outputColors.Length; i++)
                {
                    if (i == 0)
                    {
                        outputColors[i] = Color.Black;
                    }
                    else
                    {
                        ColorSet set = colors[i - 1];
                        ColorDefinition def = set.Colors[0];
                        outputColors[i] = Color.FromArgb(def.R, def.G, def.B);
                    }
                }

                for (int y = 0; y < height; y++)
                {
                    offset = y * stride;
                    segOffset = y * segStride;

                    for (int x = 0; x < width; x++)
                    {
                        int palIndex = segmented[segOffset++];
                        Color pixel = outputColors[palIndex];

                        bytes[offset++] = (byte)pixel.B;
                        bytes[offset++] = (byte)pixel.G;
                        bytes[offset++] = (byte)pixel.R;
                    }
                }
                segmented = bytes;

                SegmentedImage image = new SegmentedImage(response.TimeStamp, width, height, segmented, indexed);

                _mainPort.Post(new UpdateSegmentedImage(image));

                FoundColorAreas foundColors = new FoundColorAreas();
                foundColors.TimeStamp = response.TimeStamp;

                foreach (ColorArea area in areas)
                {
                    if (area.Area > 0)
                    {
                        area.Complete();
                        foundColors.Areas.Add(area);
                    }
                }

                _fwdPort.Post(new UpdateColorAreas(foundColors));
            }
            finally
            {
                _fwdPort.Post(new ProcessFrame(false));
            }
        }

        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public void OnSegmentedImage(UpdateSegmentedImage update)
        {
            _state.SegmentedImage = update.Body;
            update.ResponsePort.Post(DefaultUpdateResponseType.Instance);

            SendNotification(_submgrPort, update, Filter.SegmentedImage.ToString());
        }

        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public void OnUpdateColorAreas(UpdateColorAreas update)
        {
            _state.FoundColorAreas = update.Body;
            update.ResponsePort.Post(DefaultUpdateResponseType.Instance);

            SendNotification(_submgrPort, update, Filter.ColorAreas.ToString());
        }

        #endregion

        #region HTTP Handlers

        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public IEnumerator<ITask> OnHttpGet(HttpGet httpGet)
        {
            HttpListenerContext context = httpGet.Body.Context;
            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;

            string path = request.RawUrl;
            string file = Path.GetFileName(path);

            if (file != "SegmentedImage")
            {
                httpGet.ResponsePort.Post(new HttpResponseType(HttpStatusCode.OK, _state.SmallCopy, _transform));
                yield break;
            }

            Bitmap bitmap = null;
            bool dispose = false;

            switch (file)
            {
                case "SegmentedImage":
                    if (_state.SegmentedImage != null)
                    {
                        bitmap = _state.SegmentedImage.Bitmap;
                        dispose = true;
                    }
                    else
                    {
                        bitmap = Resources.SegNotAvailable;
                    }
                    break;
                default:
                    break;
            }

            yield return Arbiter.ExecuteToCompletion(TaskQueue,
                new IterativeTask<HttpListenerContext, Bitmap>(context, bitmap, StreamImage));

            if (dispose)
            {
                bitmap.Dispose();
            }
        }

        private IEnumerator<ITask> StreamImage(HttpListenerContext context, Bitmap bitmap)
        {
            HttpListenerResponse response = context.Response;

            using (MemoryStream stream = new MemoryStream())
            {
                bitmap.Save(stream, ImageFormat.Png);
                stream.Position = 0;

                response.AddHeader("Cache-Control", "No-cache");

                WriteResponseFromStream write = new WriteResponseFromStream(
                    context, stream, "image/png"
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

        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public IEnumerator<ITask> OnHttpPost(HttpPost httpPost)
        {
            HttpListenerContext context = httpPost.Body.Context;
            NameValueCollection parameters = null;
            Fault fault = null;

            try
            {

                ReadFormData readForm = new ReadFormData(httpPost);
                _utilitiesPort.Post(readForm);

                yield return Arbiter.Choice(
                    readForm.ResultPort,
                    delegate(NameValueCollection success)
                    {
                        parameters = success;
                    },
                    delegate(Exception e)
                    {
                        LogError("Error reading form data", e);
                        fault = Fault.FromException(e);
                    }
                );

                if (fault != null)
                {
                    yield break;
                }

                string name = string.Empty;
                string deleteName = null;
                string expandName = null;
                int left = 0;
                int top = 0;
                int width = 0;
                int height = 0;
                int deleteY = 0;
                int deleteCb = 0;
                int deleteCr = 0;
                double threshold = 1.0;
                int minBlobSize = 0;
                bool showPartial = false;
                bool despeckle = false;
                bool updateSettings = false;
                bool save = false;

                foreach (string key in parameters.Keys)
                {
                    if (key.StartsWith("Delete."))
                    {
                        string[] segments = key.Split('.');

                        deleteName = segments[1];
                        deleteY = int.Parse(segments[2]);
                        deleteCb = int.Parse(segments[3]);
                        deleteCr = int.Parse(segments[4]);
                    }
                    else if (key.StartsWith("ExpandY."))
                    {
                        string[] segments = key.Split('.');

                        expandName = segments[1];
                        deleteY = int.Parse(segments[2]);
                        deleteCb = int.Parse(segments[3]);
                        deleteCr = int.Parse(segments[4]);
                    }
                    else
                    {
                        switch (key)
                        {
                            case "Save":
                                save = true;
                                break;
                            case "Threshold":
                                threshold = double.Parse(parameters[key]);
                                break;
                            case "ShowPartial":
                                showPartial = parameters[key] == "on";
                                break;
                            case "Despeckle":
                                despeckle = parameters[key] == "on";
                                break;
                            case "UpdateSettings":
                                updateSettings = true;
                                break;
                            case "MinBlobSize":
                                minBlobSize = int.Parse(parameters[key]);
                                break;
                            case "New.Left":
                                left = int.Parse(parameters[key]);
                                break;
                            case "New.Top":
                                top = int.Parse(parameters[key]);
                                break;
                            case "New.Width":
                                width = int.Parse(parameters[key]);
                                break;
                            case "New.Height":
                                height = int.Parse(parameters[key]);
                                break;
                            case "New.Name":
                                name = parameters[key].Trim();
                                break;
                            default:
                                break;
                        }
                    }
                }

                if (save)
                {
                    yield return Arbiter.Choice(
                        SaveState(_state.SmallCopy),
                        EmptyHandler,
                        EmptyHandler
                    );
                }
                else if (!string.IsNullOrEmpty(deleteName))
                {
                    ColorDefinition definition = new ColorDefinition(deleteName, deleteY, deleteCb, deleteCr);

                    RemoveColorDefinition remove = new RemoveColorDefinition(definition);

                    OnRemoveColorDefinition(remove);
                }
                else if (!string.IsNullOrEmpty(expandName))
                {
                    ColorDefinition definition = new ColorDefinition(expandName, deleteY, deleteCb, deleteCr);
                    ColorSet set = _state.Colors.Find(definition.Compare);
                    if (set != null)
                    {
                        ColorDefinition existing = set.Colors.Find(definition.CompareColor);

                        definition.SigmaCb = existing.SigmaCb;
                        definition.SigmaCr = existing.SigmaCr;
                        if (existing.SigmaY < 1)
                        {
                            definition.SigmaY = 2;
                        }
                        else
                        {
                            definition.SigmaY = 3 * existing.SigmaY / 2;
                        }
                    }

                    UpdateColorDefinition update = new  UpdateColorDefinition(definition);

                    OnUpdateColorDefinition(update);
                }
                else if (updateSettings)
                {
                    UpdateSettings update = new UpdateSettings(
                        new Settings(
                            threshold,
                            showPartial,
                            despeckle,
                            minBlobSize
                        )
                    );

                    OnUpdateSettings(update);
                }
                else
                {
                    webcam.QueryFrameResponse response = null;

                    yield return Arbiter.Choice(
                        _webcamPort.QueryFrame(),
                        delegate(webcam.QueryFrameResponse success)
                        {
                            response = success;
                        },
                        delegate(Fault failure)
                        {
                            LogError("Unable to query frame for update", failure);
                        }
                    );

                    if (response == null)
                    {
                        yield break;
                    }

                    int right = left + width;
                    int bottom = top + height;
                    byte[] data = response.Frame;
                    int stride = data.Length / response.Size.Height;
                    int rowOffset = left * 3;
                    int offset;

                    int r, g, b;
                    int[] yProjection = new int[256];
                    int[] cbProjection = new int[256];
                    int[] crProjection = new int[256];
                    int count = 0;
                    double yMean = 0;
                    double cbMean = 0;
                    double crMean = 0;

                    for (int y = top; y < bottom; y++)
                    {
                        offset = rowOffset + y * stride;

                        for (int x = left; x < right; x++)
                        {
                            b = data[offset++];
                            g = data[offset++];
                            r = data[offset++];

                            ColorDefinition pixel = new ColorDefinition(r, g, b, "pixel");

                            yProjection[pixel.Y]++;
                            cbProjection[pixel.Cb]++;
                            crProjection[pixel.Cr]++;

                            count++;

                            yMean += pixel.Y;
                            cbMean += pixel.Cb;
                            crMean += pixel.Cr;
                        }
                    }

                    if (count <= 16)
                    {
                        LogError("The area was too small to generalize a color");
                        yield break;
                    }

                    yMean /= count;
                    cbMean /= count;
                    crMean /= count;

                    double ySigma = CalculateStdDev(yMean, yProjection, count);
                    double cbSigma = CalculateStdDev(cbMean, cbProjection, count);
                    double crSigma = CalculateStdDev(crMean, crProjection, count);

                    ColorDefinition definition = new ColorDefinition(
                        name,
                        (int)Math.Round(yMean),
                        (int)Math.Round(cbMean),
                        (int)Math.Round(crMean)
                    );
                    definition.SigmaY = (int)Math.Max(1, Math.Round(2 * ySigma));
                    definition.SigmaCb = (int)Math.Max(1, Math.Round(2 * cbSigma));
                    definition.SigmaCr = (int)Math.Max(1, Math.Round(2 * crSigma));

                    if (!string.IsNullOrEmpty(expandName))
                    {
                        definition.Name = expandName;
                        UpdateColorDefinition update = new UpdateColorDefinition(definition);
                        OnUpdateColorDefinition(update);
                    }
                    else
                    {
                        AddColorDefinition add = new AddColorDefinition(definition);

                        OnAddColorDefinition(add);
                    }
                }
            }
            finally
            {
                httpPost.ResponsePort.Post(new HttpResponseType(HttpStatusCode.OK, _state.SmallCopy, _transform));
            }
        }

        private double CalculateStdDev(double yMean, int[] yProjection, int count)
        {
            double offset = -yMean;
            double accum = 0;

            for (int i = 0; i < yProjection.Length; i++, offset++)
            {
                accum += offset * offset * yProjection[i];
            }
            return Math.Sqrt(accum / count);
        }

        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public IEnumerator<ITask> OnHttpQuery(HttpQuery httpQuery)
        {
            HttpListenerContext context = httpQuery.Body.Context;
            HttpListenerRequest request = context.Request;
            NameValueCollection query = httpQuery.Body.Query;

            string[] segments = request.Url.Segments;
            string file = segments[segments.Length - 1];

            if (file != "SegmentedImage")
            {
                httpQuery.ResponsePort.Post(new HttpResponseType(HttpStatusCode.OK, _state.SmallCopy, _transform));
                yield break;
            }

            Bitmap bitmap = null;

            switch (file)
            {
                case "SegmentedImage":
                    if (_state.SegmentedImage != null)
                    {
                        bitmap = _state.SegmentedImage.Bitmap;
                    }
                    else
                    {
                        bitmap = Resources.SegNotAvailable;
                    }
                    break;
                default:
                    break;
            }

            yield return Arbiter.ExecuteToCompletion(TaskQueue,
                new IterativeTask<HttpListenerContext, Bitmap>(context, bitmap, StreamImage));
        }

        #endregion

        #region Color Definition List Management

        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public void OnAddColorDefinition(AddColorDefinition add)
        {
            ColorDefinition insert = add.Body;

            if (insert.Validate() == false)
            {
                add.ResponsePort.Post(
                    Fault.FromCodeSubcodeReason(
                        FaultCodes.Receiver,
                        DsspFaultCodes.OperationFailed,
                        "ColorDefinition is invalid"
                    )
                );
                return;
            }

            ColorSet set = _state.Colors.Find(insert.Compare);

            if (set != null)
            {
                if (set.Colors.Exists(insert.CompareColor))
                {
                    add.ResponsePort.Post(
                        Fault.FromCodeSubcodeReason(
                            FaultCodes.Receiver,
                            DsspFaultCodes.DuplicateEntry,
                            "ColorSet Definition List already contains an entry for: " + insert.Name
                        )
                    );
                    return;
                }

                set.Colors.Add(insert);
                _state.UpdateColorSetMap();
                add.ResponsePort.Post(DefaultInsertResponseType.Instance);
            }
            else
            {
                set = new ColorSet();
                set.Name = insert.Name;
                set.Colors.Add(insert);
                _state.Colors.Add(set);
                _state.UpdateColorSetMap();
                add.ResponsePort.Post(DefaultInsertResponseType.Instance);
            }

            SendNotification(_submgrPort, add, Filter.ColorDefinitions.ToString());
        }

        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public void OnRemoveColorDefinition(RemoveColorDefinition remove)
        {
            ColorDefinition delete = remove.Body;
            ColorSet set = _state.Colors.Find(delete.Compare);
            if (set != null)
            {
                ColorDefinition existing = set.Colors.Find(delete.CompareColor);

                if (existing != null)
                {
                    set.Colors.Remove(existing);
                    if (set.Colors.Count == 0)
                    {
                        _state.Colors.Remove(set);
                    }
                    _state.UpdateColorSetMap();
                    remove.ResponsePort.Post(DefaultDeleteResponseType.Instance);
                    SendNotification(_submgrPort, remove, Filter.ColorDefinitions.ToString());
                }
                else
                {
                    remove.ResponsePort.Post(
                        Fault.FromCodeSubcodeReason(
                            FaultCodes.Receiver,
                            DsspFaultCodes.UnknownEntry,
                            "Color Definition List does not contain an entry for: " + delete.Name
                        )
                    );
                }
            }
            else
            {
                remove.ResponsePort.Post(
                    Fault.FromCodeSubcodeReason(
                        FaultCodes.Receiver,
                        DsspFaultCodes.UnknownEntry,
                        "Color Set List does not contain an entry for: " + delete.Name
                    )
                );
            }
        }

        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public void OnUpdateColorDefinition(UpdateColorDefinition update)
        {
            ColorDefinition def = update.Body;
            if (!def.Validate())
            {
                update.ResponsePort.Post(
                    Fault.FromCodeSubcodeReason(
                        FaultCodes.Receiver,
                        DsspFaultCodes.OperationFailed,
                        "ColorDefinition is not valid."
                    )
                );
                return;
            }

            ColorSet set = _state.Colors.Find(def.Compare);
            if (set == null)
            {
                update.ResponsePort.Post(
                    Fault.FromCodeSubcodeReason(
                        FaultCodes.Receiver,
                        DsspFaultCodes.UnknownEntry,
                        "ColorSet List does not contain an entry for: " + def.Name
                    )
                );
                return;
            }

            int index = set.Colors.FindIndex(def.CompareColor);

            if (index >= 0)
            {
                set.Colors[index] = def;
                update.ResponsePort.Post(DefaultUpdateResponseType.Instance);
                _state.UpdateColorSetMap();
                SendNotification(_submgrPort, update, Filter.ColorDefinitions.ToString());
            }
            else
            {
                update.ResponsePort.Post(
                    Fault.FromCodeSubcodeReason(
                        FaultCodes.Receiver,
                        DsspFaultCodes.UnknownEntry,
                        "ColorSet Definition List does not contain an entry for: " + def.Name
                    )
                );
            }
        }

        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public IEnumerator<ITask> OnFindColorDefinition(FindColorDefinition find)
        {
            ColorDefinition query = find.Body;
            ColorSet existing = _state.Colors.Find(query.Compare);

            if (existing != null)
            {
                find.ResponsePort.Post(existing);
            }
            else
            {
                find.ResponsePort.Post(
                    Fault.FromCodeSubcodeReason(
                        FaultCodes.Receiver,
                        DsspFaultCodes.UnknownEntry,
                        "Color Definition List does not contain an entry for: " + query.Name
                    )
                );
            }
            yield break;
        }
        #endregion

        #region Settings

        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public void OnUpdateSettings(UpdateSettings update)
        {
            _state.Settings = update.Body;
            _state.UpdateColorSetMap();
            update.ResponsePort.Post(DefaultUpdateResponseType.Instance);

            SendNotification(_submgrPort, update, Filter.Settings.ToString());
        }

        #endregion
    }
}
