//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: SimpleVision.cs $ $Revision: 2 $
//-----------------------------------------------------------------------

using W3C.Soap;
using Microsoft.Ccr.Core;
using Microsoft.Dss.Core;
using Microsoft.Dss.Core.Attributes;
using Microsoft.Dss.ServiceModel.Dssp;
using Microsoft.Dss.ServiceModel.DsspServiceBase;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml;

using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using Microsoft.Ccr.Adapters.WinForms;

using submgr = Microsoft.Dss.Services.SubscriptionManager;
using webcam = Microsoft.Robotics.Services.WebCam.Proxy;
using vision = Microsoft.Robotics.Services.Sample.SimpleVision;

namespace Microsoft.Robotics.Services.Sample.SimpleVision
{

    /// <summary>
    /// Implementation class for Vision
    /// </summary>
    [DisplayName("(User) Simple Vision")]
    [Description("Provides access to a set of basic services for color and face detection and hand gesture recognition.")]
    [Contract(Contract.Identifier)]
    public class RobotVisionTrackingService : DsspServiceBase
    {
        /// <summary>
        /// _state
        /// </summary>
        [InitialStatePartner(Optional = true, ServiceUri = ServicePaths.Store + "/SimpleVision.config.xml")]
        private SimpleVisionState _state;

        /// <summary>
        /// _main Port
        /// </summary>
        [ServicePort("/simplevision", AllowMultipleInstances = false)]
        private SimpleVisionOperations _mainPort = new SimpleVisionOperations();

        /// <summary>
        /// Internal port to update private state.
        /// </summary>
        private SimpleVisionOperations _internalPort = new SimpleVisionOperations();

        [Partner("SubMgr", Contract = submgr.Contract.Identifier, CreationPolicy = PartnerCreationPolicy.CreateAlways)]
        submgr.SubscriptionManagerPort _subMgrPort = new submgr.SubscriptionManagerPort();

        [Partner("Webcam", Contract = webcam.Contract.Identifier, CreationPolicy = PartnerCreationPolicy.UseExistingOrCreate)]
        webcam.WebCamOperations _webCamPort = new webcam.WebCamOperations();

        /// <summary>
        /// Default Service Constructor
        /// </summary>
        public RobotVisionTrackingService(DsspServiceCreationPort creationPort) :
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
                _state = new SimpleVisionState();

                _state.WebCamPollingIntervalInMs = 100;

                _state.TrackingObjectColor = new ColorVector(0.51, 0.23, 0.26, 0.99);
                _state.ColorAreaThreshold = 200;
                _state.SkinAreaThreshold = 250;
                _state.HeadAreaThreshold = 250;

                base.SaveState(_state);
            }

            base.Start();

            MainPortInterleave.CombineWith(
                Arbiter.Interleave(
                    new TeardownReceiverGroup(),
                    new ExclusiveReceiverGroup(
                        Arbiter.Receive<NotifyObjectDetection>(true, _internalPort, ObjectDetectionHandler),
                        Arbiter.Receive<NotifyFaceDetection>(true, _internalPort, FaceDetectionHandler),
                        Arbiter.Receive<NotifyHandGestureDetection>(true, _internalPort, HandGestureDetectionHandler)
                    ),
                    new ConcurrentReceiverGroup()
            ));

            WinFormsServicePort.Post(new RunForm(CreateVisionForm));

            Activate(Arbiter.ReceiveWithIterator(false, TimeoutPort(3000), GetFrame));
        }

        VisionForm _form;
        Form CreateVisionForm()
        {
            _form = new VisionForm(_mainPort, _lastProcessingResult);
            return _form;
        }

        /// <summary>
        /// Get Handler
        /// </summary>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public virtual IEnumerator<ITask> GetHandler(Get get)
        {
            get.ResponsePort.Post(_state);
            yield break;
        }

        /// <summary>
        /// Subscribe Handler
        /// </summary>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public IEnumerator<ITask> SubscribeHandler(Subscribe subscribe)
        {
            LogInfo(LogGroups.Console, "Subscribe request from: " + subscribe.Body.Subscriber);
            SubscribeHelper(_subMgrPort, subscribe.Body, subscribe.ResponsePort);
            yield break;
        }

        /// <summary>
        /// UpdateObjectColor Handler
        /// </summary>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public IEnumerator<ITask> SetObjectTrackingColorHandler(SetObjectTrackingColor update)
        {
            _state.TrackingObjectColor = update.Body;
            update.ResponsePort.Post(DefaultUpdateResponseType.Instance);
            base.SaveState(_state);
            yield break;
        }

        /// <summary>
        /// ObjectDetection External Handler for throwing exception
        /// </summary>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public IEnumerator<ITask> ObjectDetectionHandlerExternal(NotifyObjectDetection update)
        {
            update.ResponsePort.Post(Fault.FromException(new InvalidOperationException("Object Detection is only valid for notifications")));
            yield break;
        }

        /// <summary>
        /// FaceDetection External Handler for throwing exception
        /// </summary>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public IEnumerator<ITask> FaceDetectionHandlerExternal(NotifyFaceDetection update)
        {
            update.ResponsePort.Post(Fault.FromException(new InvalidOperationException("Face Detection is only valid for notifications")));
            yield break;
        }

        /// <summary>
        /// HandGestureDetection External Handler for throwing exception
        /// </summary>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public IEnumerator<ITask> HandGestureDetectionHandlerExternal(NotifyHandGestureDetection update)
        {
            update.ResponsePort.Post(Fault.FromException(new InvalidOperationException("HandGesture Detection is only valid for notifications")));
            yield break;
        }

        /// <summary>
        /// ObjectDetection Handler
        /// </summary>
        public void ObjectDetectionHandler(NotifyObjectDetection update)
        {
            _state.CurObjectResult = update.Body;
            update.ResponsePort.Post(DefaultUpdateResponseType.Instance);
            SendNotification(_subMgrPort, update);
        }

        /// <summary>
        /// FaceDetection Handler
        /// </summary>
        public void FaceDetectionHandler(NotifyFaceDetection update)
        {
            _state.CurFaceResult = update.Body;
            update.ResponsePort.Post(DefaultUpdateResponseType.Instance);
            SendNotification(_subMgrPort, update);
        }

        /// <summary>
        /// HandGestureDetection Handler
        /// </summary>
        public void HandGestureDetectionHandler(NotifyHandGestureDetection update)
        {
            _state.CurHandGestureResult = update.Body;
            update.ResponsePort.Post(DefaultUpdateResponseType.Instance);
            SendNotification(_subMgrPort, update);
        }

        const int ImageWidth = SimpleVisionState.ImageWidth;
        const int ImageHeight = SimpleVisionState.ImageHeight;
        const int FaceWidth = SimpleVisionState.FaceWidth;
        const int FaceHeight = SimpleVisionState.FaceHeight;

        byte[] _processFrame = new byte[ImageWidth * ImageHeight * 3];
        byte[] _testFrame = new byte[ImageWidth * ImageHeight];
        byte[] _testFrame2 = new byte[ImageWidth * ImageHeight];

        ImageProcessingResult _lastProcessingResult = new ImageProcessingResult();

        /// <summary>
        /// GetFrame method is called N times a second to process a webcam
        /// </summary>
        /// <returns></returns>
        public IEnumerator<ITask> GetFrame(DateTime timeout)
        {
            webcam.QueryFrameResponse frame = null;
            Fault fault = null;

            // When you using this service in simualtion environment,
            // if UpdateInterval of webcam(robocam) entity is 0(msec) in your simulation environment manifest,
            // you have to tell our simulated webcam to update its frame.
            // In PioneerSim.xml, UpdateInterval of robocam entity is set to 100(msec), so we don't need this UpdateFrame.
            //
            //_webCamPort.Post(new webcam.UpdateFrame());

            yield return Arbiter.Choice(
                _webCamPort.QueryFrame(),
                delegate(webcam.QueryFrameResponse success)
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
                LogError(LogGroups.Console, "Could not query webcam frame", fault);
                yield break;
            }

            if (frame != null && frame.Frame != null)
            {
                try
                {
                    byte[] _curFrame = null;

                    int curWidth = frame.Size.Width;
                    int curHeight = frame.Size.Height;

                    _curFrame = frame.Frame;
                    if (_curFrame.Length != _processFrame.Length)
                    {
                        bool success = Scale(3, _curFrame, new RectangleType(0, 0, curWidth - 1, curHeight - 1), _processFrame, ImageWidth, ImageHeight);
                        if (success == false)
                        {
                            LogError(LogGroups.Console, "Unable to scale image");
                            yield break;
                        }
                        else
                        {
                            _curFrame = _processFrame;
                        }
                    }

                    //LogInfo(string.Format("ProcessFrameHandler [{0},{1}] => [{2},{3}]", curWidth, curHeight, ImageWidth, ImageHeight));

                    // ProcessFrameHandler where the actual image processing methods are processed
                    ProcessFrameHandler(
                        _state.TrackingObjectColor,
                        _state.ColorAreaThreshold, _state.SkinAreaThreshold, _state.HeadAreaThreshold,
                        _curFrame, _testFrame, _testFrame2,
                        _lastProcessingResult);

                    _internalPort.Post(new NotifyObjectDetection(new ObjectResult(_lastProcessingResult)));

                    if (_lastProcessingResult.HeadFound)
                        _internalPort.Post(new NotifyFaceDetection(new FaceResult(_lastProcessingResult)));

                    if ((_lastProcessingResult.HeadFound || _lastProcessingResult.ObjectFound) &&
                        (_lastProcessingResult.LeftHandGestureFound || _lastProcessingResult.RightHandGestureFound))
                        _internalPort.Post(new NotifyHandGestureDetection(new HandGestureResult(_lastProcessingResult)));

                    DrawImages(_curFrame);
                }
                catch (Exception e)
                {
                    LogError(LogGroups.Console, "Exception in Image Processing", e);
                }
            }
            //else
            //    LogError(LogGroups.Console, "Error frame or frame.Frame is null");

            Activate(Arbiter.ReceiveWithIterator(false, TimeoutPort(_state.WebCamPollingIntervalInMs), GetFrame));
        }

        private void DrawImages(byte[] _curFrame)
        {
            Bitmap bmp = new Bitmap(
                ImageWidth,
                ImageHeight,
                PixelFormat.Format24bppRgb
            );
            Bitmap testBmp = new Bitmap(
                ImageWidth,
                ImageHeight,
                PixelFormat.Format8bppIndexed
            );
            Bitmap testBmp2 = new Bitmap(
                ImageWidth,
                ImageHeight,
                PixelFormat.Format8bppIndexed
            );
            Bitmap facebmp = null;

            BitmapData data = bmp.LockBits(
                new Rectangle(0, 0, ImageWidth, ImageHeight),
                ImageLockMode.WriteOnly,
                PixelFormat.Format24bppRgb
            );
            Marshal.Copy(_curFrame, 0, data.Scan0, _curFrame.Length);
            bmp.UnlockBits(data);

            BitmapData data2 = testBmp.LockBits(
                new Rectangle(0, 0, ImageWidth, ImageHeight),
                ImageLockMode.WriteOnly,
                PixelFormat.Format8bppIndexed
            );
            Marshal.Copy(_testFrame, 0, data2.Scan0, _testFrame.Length);
            testBmp.UnlockBits(data2);

            BitmapData data3 = testBmp2.LockBits(
                new Rectangle(0, 0, ImageWidth, ImageHeight),
                ImageLockMode.WriteOnly,
                PixelFormat.Format8bppIndexed
            );
            Marshal.Copy(_testFrame2, 0, data3.Scan0, _testFrame2.Length);
            testBmp2.UnlockBits(data3);

            if (_lastProcessingResult.HeadFound == true && bmp != null)
            {
                Rectangle faceRect = Rectangle.FromLTRB(_lastProcessingResult.HeadBoxRegion.Sx, _lastProcessingResult.HeadBoxRegion.Sy,
                    _lastProcessingResult.HeadBoxRegion.Ex, _lastProcessingResult.HeadBoxRegion.Ey);
                Bitmap tmpbmp = bmp.Clone(faceRect, PixelFormat.Format24bppRgb);
                facebmp = new Bitmap(tmpbmp, new Size(FaceWidth, FaceHeight));
            }

            FormInvoke setImage = new FormInvoke(
                delegate()
                {
                    _form.CameraImage = bmp;
                    _form.TestImage = testBmp;
                    _form.TestImage2 = testBmp2;
                    if (_lastProcessingResult.HeadFound == true && facebmp != null)
                        _form.FaceImage = facebmp;
                }
            );
            WinFormsServicePort.Post(setImage);
        }

        #region Image processing for color object, head and hand gestures detection

        int leftHandArea;
        int leftHandXSum;
        int leftHandYSum;

        int rightHandArea;
        int rightHandXSum;
        int rightHandYSum;

        int motionFailCount;

        byte detectedObjPixelFillColor = 255;
        byte detectedObjRegionFillColor = 219;
        byte detectedSkinPixelFillColor = 205;
        byte detectedMotionPixelFillColor = 255;
        byte detectedMotionWithSkinPixelFillColor = 180;

        byte[] grayImg = new byte[ImageWidth * ImageHeight];
        byte[] colorImg = new byte[ImageWidth * ImageHeight];
        byte[] skinImg = new byte[ImageWidth * ImageHeight];
        byte[] motionImg = new byte[ImageWidth * ImageHeight];
        byte[] tmpImg = new byte[ImageWidth * ImageHeight];

        byte[] grayImgOld = new byte[ImageWidth * ImageHeight];
        byte[] backgroundImg = new byte[ImageWidth * ImageHeight];

        int[] boundto = new int[ImageWidth * ImageHeight];
        int[] regcard = new int[ImageWidth * ImageHeight];
        int[] regmap = new int[ImageWidth * ImageHeight];

        /// <summary>
        /// Find an object in the image, using the thresholds provided
        /// </summary>
        /// <param name="trackingObjectColor"></param>
        /// <param name="colorAreaThreshold"></param>
        /// <param name="skinAreaThreshold"></param>
        /// <param name="headAreaThreshold"></param>
        /// <param name="rgbValues"></param>
        /// <param name="testValues"></param>
        /// <param name="testValues2"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public void ProcessFrameHandler(
            ColorVector trackingObjectColor,
            int colorAreaThreshold,
            int skinAreaThreshold,
            int headAreaThreshold,
            byte[] rgbValues,
            byte[] testValues,
            byte[] testValues2,
            ImageProcessingResult result)
        {
            if (trackingObjectColor == null)
                return;
            if (rgbValues == null)
                return;
            if (testValues == null)
                return;
            if (testValues2 == null)
                return;
            if (result == null)
                return;

            for (int i = 0, j = 0; i < grayImg.Length; i++, j += 3)
            {
                grayImg[i] = (byte)(((int)rgbValues[j] + (int)rgbValues[j + 1] + (int)rgbValues[j + 2]) / 3);
            }

            //Array.Clear(colorImg, 0, colorImg.Length);
            //Array.Clear(skinImg, 0, skinImg.Length);
            //Array.Clear(motionImg, 0, motionImg.Length);
            for (int i = 0; i < colorImg.Length; i++)
            {
                colorImg[i] = 0;
                skinImg[i] = 0;
                motionImg[i] = 0;
                tmpImg[i] = 0;
            }

            int offset = 0;
            for (int y = 0; y < ImageHeight; y++)
            {
                for (int x = 0; x < ImageWidth; x++)
                {
                    int r, g, b;
                    int indedx = (int)(y * ImageWidth + x);

                    b = rgbValues[offset++];
                    g = rgbValues[offset++];
                    r = rgbValues[offset++];

                    if (IsSkin(r, g, b))
                    {
                        skinImg[indedx] = 255;
                    }

                    if (IsMyColor(r, g, b, trackingObjectColor))
                    {
                        colorImg[indedx] = 255;
                    }
                }
            }

            //skin image noise smoothing
            //Smooth(skinImg, tmpImg);
            //Smooth(tmpImg, skinImg);

            //color image noise smoothing
            Smooth(colorImg, tmpImg);
            Smooth(tmpImg, colorImg);

            result.ObjectFound = false;
            int colRegionNum = SegmentationRegions(colorImg, colorAreaThreshold, result);
            if (colRegionNum > 0)
                DetectColorObject(colorImg, colorAreaThreshold, detectedObjRegionFillColor, result);

            // cacluate frame difference
            for (int i = 0; i < ImageWidth * ImageHeight; i++)
            {
                motionImg[i] = (byte)Math.Abs(grayImg[i] - grayImgOld[i]);
            }

            bool motionFound = DetectMotion(motionImg, 40, 250, result);
            //LogInfo(string.Format("motionFailCount {0}", motionFailCount));

            if (motionFound == false)
            {
                motionFailCount++;
            }
            else
            {
                motionFailCount = 0;
            }

            if (motionFailCount > 1)
            {
                //LogInfo("Get backgroundImg");
                grayImg.CopyTo(backgroundImg, 0);
            }

            // cacluate foreground from difference btw background and current image
            for (int i = 0; i < ImageWidth * ImageHeight; i++)
            {
                tmpImg[i] = (byte)Math.Abs(backgroundImg[i] - grayImg[i]);
                if (tmpImg[i] > 30)
                    tmpImg[i] = 255;
                else
                    tmpImg[i] = 0;
            }

            DrawCurrentResult(testValues, testValues2);

            // find head from skin image
            FindHead(skinImg, skinAreaThreshold, headAreaThreshold, testValues2, result);

            // find hand gestures from foreground + skin image with object and face information
            FindHandGestures(skinImg, tmpImg, result);

            grayImg.CopyTo(grayImgOld, 0);
        }

        private void DrawCurrentResult(byte[] testValues, byte[] testValues2)
        {
            for (int i = 0; i < ImageWidth * ImageHeight; i++)
            {
                if (colorImg[i] == detectedObjRegionFillColor) testValues[i] = detectedObjRegionFillColor;
                else if (colorImg[i] != 0) testValues[i] = detectedObjPixelFillColor;
                else if (skinImg[i] != 0) testValues[i] = detectedSkinPixelFillColor;
                else testValues[i] = 0;
            }

            for (int i = 0; i < ImageWidth * ImageHeight; i++)
            {
                if (tmpImg[i] != 0 && skinImg[i] != 0) testValues2[i] = detectedMotionWithSkinPixelFillColor;
                //else if (tmpImg[i] != 0) testValues2[i] = detectedMotionPixelFillColor; //if you want to see a foreground image
                else if (motionImg[i] != 0) testValues2[i] = detectedMotionPixelFillColor;//if you want to see a motion image
                else testValues2[i] = 0;
            }
        }

        static bool IsSkin(int red, int green, int blue)
        {
            ColorVector Skin1 = new ColorVector((double)225 / (225 + 165), (double)165 / (225 + 165));
            ColorVector Skin2 = new ColorVector((double)125 / (125 + 50), (double)50 / (125 + 50));
            ColorVector curColor = new ColorVector((double)red / (red + green), (double)green / (red + green));

            double sum = red + green + blue;
            if (sum == 0) return false;


            if ((red < 60) || (green < 40) || (blue < 20))
                return false;
            else if ((green > 160))
                return false;

            if ((Math.Abs(ColorVector.CompareSimilarity(curColor, Skin1)) > 0.995) || (Math.Abs(ColorVector.CompareSimilarity(curColor, Skin2)) > 0.995))
                return true;

            return false;
        }

        static bool IsMyColor(int red, int green, int blue, ColorVector myColor)
        {
            double sum = red + green + blue;
            if (sum == 0) return false;

            ColorVector curColor = new ColorVector((double)red / sum, (double)green / sum, (double)blue / sum);
            if (Math.Abs(ColorVector.CompareSimilarity(curColor, myColor)) > myColor.SimilarityMeasure)
                return true;

            return false;
        }

        static int Neighbor(byte[] image, int x, int y)
        {
            int neighbor = 0;

            // self
            if (image[ImageWidth * y + x] != 0) neighbor++;

            // 4 neighbor
            if (image[ImageWidth * (y - 1) + x] != 0) neighbor++;
            if (image[ImageWidth * (y + 1) + x] != 0) neighbor++;
            if (image[ImageWidth * y + x - 1] != 0) neighbor++;
            if (image[ImageWidth * y + x + 1] != 0) neighbor++;

            // 8 neighbor
            if (image[ImageWidth * (y - 1) + x - 1] != 0) neighbor++;
            if (image[ImageWidth * (y - 1) + x + 1] != 0) neighbor++;
            if (image[ImageWidth * (y + 1) + x - 1] != 0) neighbor++;
            if (image[ImageWidth * (y + 1) + x + 1] != 0) neighbor++;

            return neighbor;
        }

        static void Smooth(byte[] pbSrc, byte[] pbTar)
        {
            int x, y;
            int neighbor;

            for (y = 1; y < ImageHeight - 1; y++)
            {
                for (x = 1; x < ImageWidth - 1; x++)
                {
                    neighbor = Neighbor(pbSrc, x, y);

                    int index = ImageWidth * y + x;
                    if (neighbor > 4)
                        pbTar[index] = 255;
                    else
                        pbTar[index] = 0;
                }
            }
        }

        static bool DetectMotion(byte[] image, byte diffThresh, int sizeThresh, ImageProcessingResult result)
        {
            int i, x, y;

            for (i = 0; i < ImageWidth * ImageHeight; i++)
            {
                if (image[i] > diffThresh)
                    image[i] = 255;
                else
                    image[i] = 0;
            }

            int sumX = 0;
            int sumY = 0;
            int pixelCount = 0;
            for (y = 0; y < ImageHeight; y++)
            {
                for (x = 0; x < ImageWidth; x++)
                {
                    if (image[y * ImageWidth + x] != 0)
                    {
                        sumX += x;
                        sumY += y;
                        pixelCount++;
                    }
                }
            }

            if (pixelCount > sizeThresh)
            {
                result.MotionFound = true;
                result.XMotion = (sumX / pixelCount);
                result.YMotion = (sumY / pixelCount);
                result.MotionSize = pixelCount;
            }
            else
                result.MotionFound = false;

            return result.MotionFound;
        }

        static double BoxFilledRatio(int width, int height, int area)
        {
            double boxsize = (width * height);
            if (boxsize == 0)
                return 0;

            return (double)area / boxsize;
        }

        static bool DetectColorObject(byte[] image, int areaThreshold, byte fillColor, ImageProcessingResult result)
        {
            bool objectFound = false;
            int area = 0;
            int[] yProjection = new int[ImageHeight];
            int[] xProjection = new int[ImageWidth];
            int xMean = 0;
            int yMean = 0;

            // select a largest color blob in segmented regions
            int sx = result.boundBox[0].Sx;
            int ex = result.boundBox[0].Ex;
            int sy = result.boundBox[0].Sy;
            int ey = result.boundBox[0].Ey;

            for (int y = sy; y < ey; y++)
            {
                for (int x = sx; x < ex; x++)
                {
                    int indedx = (int)(y * ImageWidth + x);
                    if (image[indedx] != 0)
                    {
                        area++;
                        xProjection[x]++;
                        yProjection[y]++;
                        xMean += x;
                        yMean += y;

                        image[indedx] = fillColor;
                    }
                }
            }

            if (area > areaThreshold)
            {

                xMean = xMean / area;
                yMean = yMean / area;

                int xOff = -xMean;
                int xSecond = 0;
                int xThird = 0;

                for (int i = 0; i < xProjection.Length; i++)
                {
                    if (xProjection[i] > 0)
                    {
                        int square = xOff * xOff * xProjection[i];

                        xSecond += square;
                        xThird += xOff * square;
                    }
                    xOff++;
                }

                double xStdDev = Math.Sqrt((double)xSecond / area);
                double xSkew = xThird / (area * xStdDev * xStdDev * xStdDev);

                int yOff = -yMean;
                int ySecond = 0;
                int yThird = 0;

                for (int i = 0; i < yProjection.Length; i++)
                {
                    if (yProjection[i] > 0)
                    {
                        int square = yOff * yOff * yProjection[i];

                        ySecond += square;
                        yThird += yOff * square;
                    }
                    yOff++;
                }

                double yStdDev = Math.Sqrt((double)ySecond / area);
                double ySkew = yThird / (area * yStdDev * yStdDev * yStdDev);


                if (BoxFilledRatio((int)(3 * xStdDev), (int)(3 * yStdDev), area) > 0.6 )
                {
                    objectFound = true;

                    result.XMean = xMean;
                    result.YMean = yMean;
                    result.Area = area;

                    result.XStdDev = xStdDev;
                    result.YStdDev = yStdDev;
                    result.XSkew = xSkew;
                    result.YSkew = ySkew;
                }
                else
                {
                    objectFound = false;
                }

            }
            result.ObjectFound = objectFound;
            return objectFound;
        }

        static int FindHeadRegionCandidates(int headAreaThresh, ImageProcessingResult result)
        {
            RectangleType[] boundBox = result.boundBox;
            RectangleType[] headBox = result.headBox;
            int[] size = result.size;
            int nRegions = result.numberOfLocations;

            double ratio;
            double h, w;
            double boxfilled;

            int heads = 0;
            for (int i = 0; i < nRegions; i++)
            {
                w = Math.Abs((double)boundBox[i].Ex - (double)boundBox[i].Sx);
                h = Math.Abs((double)boundBox[i].Ey - (double)boundBox[i].Sy);

                boxfilled = (double)size[i] / (h * w);

                int mcy = (result.boundBox[i].Ey + result.boundBox[i].Sy) / 2;

                // geometric constraint: face should be in upper image plane
                if (mcy > (ImageHeight / 2) || size[i] < headAreaThresh) continue;

                if (boxfilled >= 0.480)
                {
                    ratio = w / h;
                    //LogInfo(string.Format("ratio: {0}", ratio));

                    if (ratio >= 0.4 && ratio <= 1.25)
                    {
                        headBox[heads].Sy = boundBox[i].Sy;
                        headBox[heads].Sx = boundBox[i].Sx;
                        headBox[heads].Ey = boundBox[i].Ey;
                        headBox[heads].Ex = boundBox[i].Ex;
                        heads++;
                    }
                }
            }

            result.numberOfHeadCandidates = heads;
            return heads;
        }

        void FindHead(byte[] image, int skinAreaThreshold, int headAreaThreshold, byte[] testValues2, ImageProcessingResult result)
        {
            // finds head
            result.HeadFound = false;

            int regionNum = SegmentationRegions(image, skinAreaThreshold, result);
            if (regionNum > 0)
                FindHeadRegionCandidates(headAreaThreshold, result);
            else return;

            if (result.numberOfHeadCandidates > 0)
            {
                // step 1: face finding with a color object(considering a color object as a user's colored shirt)
                result.HeadFoundOnColorObject = false;
                if (result.ObjectFound)
                {
                    for (int i = 0; i < result.numberOfHeadCandidates; i++)
                    {
                        int headX = (result.headBox[i].Ex + result.headBox[i].Sx) / 2;
                        int headY = (result.headBox[i].Ey + result.headBox[i].Sy) / 2;

                        // is headbox[i] located in the upper side of color object?
                        if (Math.Abs(result.XMean - headX) < 30
                            && result.YMean > headY
                            && ValidateFaceByEllipse(image, i, testValues2, result))
                        {
                            result.foundHeadIndex = i;

                            result.HeadFound = true;
                            result.HeadFoundOnColorObject = true;
                            result.HeadBoxRegion = result.headBox[i];

                            //LogInfo(string.Format("headFound1 by Color Obj headX = {0}, result.XMean = {1}", headX, result.XMean));
                            break;
                        }
                    }
                }

                // step 2: face finding without color object
                if (result.HeadFound == false)
                {
                    for (int i = 0; i < result.numberOfHeadCandidates; i++)
                    {
                        int headX = (result.headBox[i].Ex + result.headBox[i].Sx) / 2;

                        if (ValidateFaceByEllipse(image, i, testValues2, result))
                        {
                            result.foundHeadIndex = i;

                            result.HeadFound = true;
                            result.HeadBoxRegion = result.headBox[i];
                            //LogInfo(string.Format("headFound2 by ellipse headX = {0}", headX));
                            break;

                        }
                    }
                }
            }
        }

        void FindHandGestures(byte[] image, byte[] diffImage, ImageProcessingResult result)
        {
            int headX = 0;
            int headY = 20;

            if (CheckForeground(diffImage, result) == false)
                return;

            if (result.HeadFound)
            {
                headX = (result.headBox[result.foundHeadIndex].Ex + result.headBox[result.foundHeadIndex].Sx) / 2;
                headY = (result.headBox[result.foundHeadIndex].Ey + result.headBox[result.foundHeadIndex].Sy) / 2;
            }

            // find right hand gesture
            int sx = 0;
            int ex = ImageWidth / 2 - 20;
            int sy = 20;
            int ey = ImageHeight;

            if (result.HeadFound)
            {
                sy = headY;
                ex = headX - 20;
            }
            else if (result.ObjectFound)
            {
                sy = result.YMean;
                ex = (int)(result.XMean - result.XStdDev);
            }

            int rightHandX = 0;
            int rightHandY = 0;
            int area = 0;
            for (int y = sy; y < ey; y++)
            {
                for (int x = sx; x < ex; x++)
                {
                    int i = y * ImageWidth + x;

                    // gather hand pixels from skin image and foreground image in a selected range
                    if (diffImage[i] != 0 && image[i] != 0)
                    {
                        // exclude head region
                        if (!(result.HeadFound && x > result.headBox[result.foundHeadIndex].Sx && x < result.headBox[result.foundHeadIndex].Ex &&
                            y > result.headBox[result.foundHeadIndex].Sy && y < result.headBox[result.foundHeadIndex].Ey))
                        {
                            area++;
                            rightHandX += x;
                            rightHandY += y;
                        }
                    }
                }
            }

            if (area > 20)
            {
                // accumulate hand blob area to enhance the motion informs.
                rightHandArea += area;

                rightHandXSum += rightHandX;
                rightHandYSum += rightHandY;


                result.RightHandX = rightHandXSum / rightHandArea;
                result.RightHandY = rightHandYSum / rightHandArea;
                //LogInfo(string.Format("rightHand = area {0} x {1} y {2}", result.rightHandArea, result.rightHandX, result.rightHandY));
            }
            else
            {
                result.RightHandGestureFound = false;
                rightHandArea = 0;
                rightHandXSum = 0;
                rightHandYSum = 0;
            }

            if (rightHandArea > 200 && result.RightHandGestureFound == false)
            {
                result.RightHandGestureFound = true;
                //LogInfo(string.Format("rightHandGesture Found {0} x {1} y {2}", result.rightHandArea, result.rightHandX, result.rightHandY));
            }

            // find left hand gesture
            sx = ImageWidth / 2 + 20;
            ex = ImageWidth;
            sy = 20;
            ey = ImageHeight;

            if (result.HeadFound)
            {
                sx = headX + 20;
                sy = headY;
            }
            else if (result.ObjectFound)
            {
                sx = (int)(result.XMean + result.XStdDev);
                sy = result.YMean;
            }

            int leftHandX = 0;
            int leftHandY = 0;
            area = 0;
            for (int y = sy; y < ey; y++)
            {
                for (int x = sx; x < ex; x++)
                {
                    int i = y * ImageWidth + x;

                    // gather hand pixels from skin image and foreground image in a selected range
                    if (diffImage[i] != 0 && image[i] != 0)
                    {
                        // exclude head region
                        if (!(result.HeadFound && x > result.headBox[result.foundHeadIndex].Sx && x < result.headBox[result.foundHeadIndex].Ex &&
                            y > result.headBox[result.foundHeadIndex].Sy && y < result.headBox[result.foundHeadIndex].Ey))
                        {
                            area++;
                            leftHandX += x;
                            leftHandY += y;
                        }
                    }
                }
            }

            if (area > 20)
            {
                // accumulate hand blob area to enhance the motion informs.
                leftHandArea += area;

                leftHandXSum += leftHandX;
                leftHandYSum += leftHandY;


                result.LeftHandX = leftHandXSum / leftHandArea;
                result.LeftHandY = leftHandYSum / leftHandArea;

                //LogInfo(string.Format("leftHand = area {0} x {1} y {2}", result.leftHandArea, result.leftHandX, result.leftHandY));
            }
            else
            {
                result.LeftHandGestureFound = false;
                leftHandArea = 0;
                leftHandXSum = 0;
                leftHandYSum = 0;
            }

            if (leftHandArea > 200 && result.LeftHandGestureFound == false)
            {
                result.LeftHandGestureFound = true;
                //LogInfo(string.Format("LeftHandGesture Found {0} x {1} y {2}", result.leftHandArea, result.leftHandX, result.leftHandY));
            }

        }

        bool CheckForeground(byte[] diffImage, ImageProcessingResult result)
        {
            // discard if foreground image is too big
            int diffArea = 0;
            for (int i = 0; i < diffImage.Length; i++)
            {
                if (diffImage[i] != 0) diffArea++;
            }

            if (BoxFilledRatio(ImageWidth, ImageHeight, diffArea) > 0.3)
            {
                //LogInfo("Discard diffImage");

                result.RightHandGestureFound = false;
                rightHandArea = 0;
                rightHandXSum = 0;
                rightHandYSum = 0;

                result.LeftHandGestureFound = false;
                leftHandArea = 0;
                leftHandXSum = 0;
                leftHandYSum = 0;

                return false;
            }
            return true;
        }

        static bool ValidateFaceByEllipse(byte[] image, int i, byte[] drawImage, ImageProcessingResult result)
        {
            double M_PI = 3.1415926535897932;

            int mcx = (result.headBox[i].Ex + result.headBox[i].Sx) / 2;
            int mcy = (result.headBox[i].Ey + result.headBox[i].Sy) / 2;

            int x, y;

            // second moment variables
            double moment_rr_temp = 0.0;
            double moment_rc_temp = 0.0;
            double moment_cc_temp = 0.0;


            int sx = result.headBox[i].Sx;
            int ex = result.headBox[i].Ex;
            int sy = result.headBox[i].Sy;
            int ey = result.headBox[i].Ey;

            int area = 0;
            for (y = sy; y < ey; y++)
                for (x = sx; x < ex; x++)
                {
                    if (image[y * ImageWidth + x] != 0)
                    {
                        area++;
                        moment_rr_temp += Math.Pow((double)(y - mcy), 2.0);
                        moment_rc_temp += (double)(x - mcx) * (double)(y - mcy);
                        moment_cc_temp += Math.Pow((double)(x - mcx), 2.0);
                    }
                }

            double moment_rr = moment_rr_temp / (double)area;
            double moment_rc = moment_rc_temp / (double)area;
            double moment_cc = moment_cc_temp / (double)area;

            double major_axis = (double)2.0 * Math.Sqrt(2.0) *
                    Math.Sqrt((moment_cc + moment_rr) +
                    Math.Sqrt(Math.Pow((moment_cc - moment_rr), 2.0) + (double)4.0 * Math.Pow(moment_rc, 2.0)));

            double minor_axis = (double)2.0 * Math.Sqrt(2.0) *
                    Math.Sqrt((moment_cc + moment_rr) -
                    Math.Sqrt(Math.Pow((moment_cc - moment_rr), 2.0) + (double)4.0 * Math.Pow(moment_rc, 2.0)));

            double temporary1 = (moment_rr - moment_cc) +
                    Math.Sqrt(Math.Pow((moment_rr - moment_cc), 2.0) + 4.0 * Math.Pow(moment_rc, 2.0));

            double temporary2 = (moment_cc - moment_rr) +
                    Math.Sqrt(Math.Pow((moment_cc - moment_rr), 2.0) + 4.0 * Math.Pow(moment_rc, 2.0));

            double orientation = 0.0;
            if (moment_rr > moment_cc)
            {
                orientation = Math.Atan(temporary1 / (2.0 * moment_rc));
            }
            else
            {
                orientation = Math.Atan((2.0 * moment_rc) / temporary2);
            }

            major_axis = major_axis / 2.0;
            minor_axis = minor_axis / 2.0;
            double ellipse_area = major_axis * minor_axis / 4.0 * M_PI;
            double ratio = area / ellipse_area;
            double eccen = major_axis / minor_axis;
            orientation = orientation * 180.0 / M_PI;

            //LogInfo(string.Format("Ratio {0} Eccen {1} Orientation {2} mcx {3} mcy {4}", ratio, eccen, orientation, mcx, mcy));

            double CONVEX_RATIO = 0.7;
            double ECCEN_RATIO = 2;
            // validate face by ellipse(limit of convex and concave limit of distortion)
            if (ratio > CONVEX_RATIO
                && eccen < ECCEN_RATIO
                && Math.Abs(orientation) > 60)
            {
                BresEllipse(mcx, mcy,
                    (int)major_axis, (int)minor_axis, (double)orientation, drawImage, 250);

                return true;
            }
            return false;
        }

        /// <summary>
        /// Find an object in the image, using the thresholds provided
        /// </summary>
        /// <param name="pixelBytes"></param>
        /// <param name="sourceImage"></param>
        /// <param name="sourceRect"></param>
        /// <param name="tarImage"></param>
        /// <param name="tarWidth"></param>
        /// <param name="tarHeight"></param>
        /// <returns>success</returns>
        public static bool Scale(int pixelBytes, byte[] sourceImage, RectangleType sourceRect, byte[] tarImage, int tarWidth, int tarHeight)
        {
            if (sourceImage == null)
                return false;
            if (tarImage == null)
                return false;

            float XRatio, YRatio, X, Y;
            int Xc, Yc;

            int SourceWidth = sourceRect.Ex - sourceRect.Sx + 1;
            int SourceHeight = sourceRect.Ey - sourceRect.Sy + 1;

            // error checking
            if (pixelBytes == 0 || SourceWidth == 0 || SourceHeight == 0 || tarWidth == 0 || tarHeight == 0)
                return false;
            if (sourceImage.Length < (SourceWidth * SourceHeight * pixelBytes) || tarImage.Length < (tarWidth * tarHeight * pixelBytes))
                return false;

            // compute the scaling ratios
            XRatio = (float)((float)SourceWidth /
                          (float)tarWidth);

            YRatio = (float)((float)SourceHeight /
                          (float)tarHeight);

            // alloc the look up table
            int[] XPoints = new int[tarWidth];
            int[] YPoints = new int[tarHeight];

            for (Xc = 0, X = sourceRect.Sx; Xc < tarWidth; X += XRatio, Xc++)
                XPoints[Xc] = (int)X;

            for (Yc = 0, Y = sourceRect.Sy; Yc < tarHeight; Y += YRatio, Yc++)
                YPoints[Yc] = (int)Y;

            // scale the bitmap using the look up table
            int srcOffset = 0;
            for (Yc = 0; Yc < tarHeight; Yc++)
            {
                int Offset = YPoints[Yc] * SourceWidth * pixelBytes;
                for (Xc = 0; Xc < tarWidth; Xc++)
                {
                    int targOffset = Offset + XPoints[Xc] * pixelBytes;

                    for (int i = 0; i < pixelBytes; i++)
                        tarImage[srcOffset++] = sourceImage[targOffset++];
                }
            }
            return true;
        }

        /// <summary>
        /// A segmentation algorithm for a binary byte image
        /// </summary>
        /// <param name="image">a byte image</param>
        /// <param name="sizeThresh">a size threshold</param>
        /// <param name="result">a ImageProcessing result</param>
        /// <returns>the number of regions found</returns>
        int SegmentationRegions(byte[] image, int sizeThresh, ImageProcessingResult result)
        {
            int width = SimpleVisionState.ImageWidth;
            int height = SimpleVisionState.ImageHeight;
            int MaxRegions = SimpleVisionState.MaxRegions;

            PointType[] location = result.location;
            RectangleType[] boundBox = result.boundBox;
            int[] size = result.size;

            int numLocations = 0;
            int imageSize = height * width;

            int i, j, k;

            byte backpix, uppix, curpix;
            int nRegions = 0;

            int upID, backID;

            int[] sizeid = new int[MaxRegions];

            // initialize the region map and boundto variables
            for (i = 0; i < imageSize; i++)
            {
                regcard[i] = 0;
                boundto[i] = i;
                regmap[i] = -1;
            }

            // initialize the parts of the image we won't use
            for (i = 0; i < width; i++)
            {
                image[i] = 0;
            }

            for (i = 0; i < height; i++)
            {
                image[i * width] = 0;
            }

            // segment the image using a 2-pass algorithm (4-connected)
            int itb = width + 1;
            int itbu = 1;
            int iregptr = width + 1;

            for (i = 0; i < height - 1; i++, itb++, itbu++, iregptr++)
            {
                backpix = image[itb - 1];
                for (j = 0; j < width - 1; j++, itb++, itbu++, iregptr++)
                {
                    if (itb >= image.Length || itbu >= image.Length || iregptr >= regmap.Length)
                        continue;
                    // get the value of the current pixel
                    curpix = image[itb];

                    // set the default value of regptr
                    regmap[iregptr] = -1;


                    // if the pixel is in the region
                    if (curpix != 0)
                    {
                        // get the up pixel
                        uppix = image[itbu];

                        // test if the rgb values are similar for the up and back pixels
                        if (uppix != 0)
                        {
                            // similar to up pixel
                            if (backpix != 0)
                            {
                                // similar to back pixel as well so get the region IDs
                                upID = regmap[iregptr - width];
                                backID = regmap[iregptr - 1];
                                if (backID == upID)
                                {
                                    // equal region ids
                                    regmap[iregptr] = upID;
                                }
                                else
                                {
                                    // not equal region ids
                                    if (boundto[upID] < boundto[backID])
                                    {
                                        regmap[iregptr] = boundto[upID];
                                        boundto[backID] = boundto[upID];
                                    }
                                    else
                                    {
                                        regmap[iregptr] = boundto[backID];
                                        boundto[upID] = boundto[backID];
                                    }
                                }
                            }
                            else
                            {
                                // similar only to the top pixel
                                regmap[iregptr] = regmap[iregptr - width];
                            }
                        }
                        else if (backpix != 0)
                        {
                            // similar only to back pixel
                            regmap[iregptr] = regmap[iregptr - 1];
                        }
                        else
                        {
                            // not similar to either pixel
                            regmap[iregptr] = nRegions++;
                        }
                    }

                    backpix = curpix;
                }
            }

            // get out if there's nothing else to do
            if (nRegions == 0)
            {
                return 0;
            }

            // second pass, fix the IDs and calculate the region sizes
            for (iregptr = 0; iregptr < regmap.Length; iregptr++)
            {

                if (regmap[iregptr] >= 0)
                {
                    regmap[iregptr] = boundto[regmap[iregptr]];

                    // need to follow the tree in some special cases
                    while (boundto[regmap[iregptr]] != regmap[iregptr])
                        regmap[iregptr] = boundto[regmap[iregptr]];

                    regcard[regmap[iregptr]]++;
                }
            }

            // grab the N largest ones
            for (i = 0; i < MaxRegions; i++)
            {
                size[i] = 0;
                sizeid[i] = -1;
                for (j = 0; j < nRegions; j++)
                {

                    // don't consider regions already in the list
                    for (k = 0; k < i; k++)
                    {
                        if (j == sizeid[k])
                            break;
                    }
                    if (k < i)
                        continue;

                    if ((regcard[j] > sizeThresh) && (regcard[j] > size[i]))
                    {
                        size[i] = regcard[j];
                        sizeid[i] = j;
                    }
                }
                if (size[i] == 0)
                {
                    break;
                }

                // do this so we can fix the id values below
                boundto[sizeid[i]] = -2;
            }

            numLocations = i;

            // calculate new ids for the regions in order of size
            for (i = 0, j = 0; i < nRegions; i++)
            {
                if (boundto[i] == -2)
                {
                    boundto[i] = j;
                    j++;
                }
                else
                    boundto[i] = -1;
            }

            // initialize the bounding boxes and centroids
            for (i = 0; i < numLocations; i++)
            {
                location[i].x = location[i].y = 0;
                boundBox[i].Sy = boundBox[i].Sx = 1000;
                boundBox[i].Ey = boundBox[i].Ex = 0;
                size[i] = 0;
            }

            // calculate region sizes, bounding boxes, and centroids
            iregptr = 0;
            for (j = 0; j < height; j++)
            {
                for (k = 0; k < width; k++, iregptr++)
                {

                    if (iregptr >= regmap.Length)
                        continue;
                    // assign the new region id to the region map
                    if (regmap[iregptr] >= 0)
                    {
                        regmap[iregptr] = boundto[regmap[iregptr]];
                        if (regmap[iregptr] < 0)
                            continue;

                        location[regmap[iregptr]].x += k;
                        location[regmap[iregptr]].y += j;

                        boundBox[regmap[iregptr]].Sy = j < boundBox[regmap[iregptr]].Sy ? j : boundBox[regmap[iregptr]].Sy;
                        boundBox[regmap[iregptr]].Sx = k < boundBox[regmap[iregptr]].Sx ? k : boundBox[regmap[iregptr]].Sx;
                        boundBox[regmap[iregptr]].Ey = j > boundBox[regmap[iregptr]].Ey ? j : boundBox[regmap[iregptr]].Ey;
                        boundBox[regmap[iregptr]].Ex = k > boundBox[regmap[iregptr]].Ex ? k : boundBox[regmap[iregptr]].Ex;

                        // recalculate size
                        size[regmap[iregptr]]++;
                    }
                }
            }

            for (i = 0; i < numLocations; i++)
            {
                location[i].x /= size[i];
                location[i].y /= size[i];
            }

            //LogInfo(string.Format("Terminating segmentation {0} regions", numLocations));
            result.numberOfLocations = numLocations;

            return numLocations;
        }

        static void display(int ox, int oy, int x, int y, double angle, byte[] image, byte color)
        {
            double M_PI = 3.1415926535897932;

            int px, py, xx, yy;
            int index = 0;
            px = (int)(x * Math.Cos(M_PI / 180.0 * (double)angle) - y * Math.Sin(M_PI / 180.0 * (double)angle));
            py = (int)(x * Math.Sin(M_PI / 180.0 * (double)angle) + y * Math.Cos(M_PI / 180.0 * (double)angle));
            xx = ox + px;
            yy = oy + py;
            index = yy * ImageWidth + xx;
            if (index >= 0 && index < image.Length) image[index] = color;

            px = (int)(x * Math.Cos(M_PI / 180.0 * (double)angle) - (-1.0) * y * Math.Sin(M_PI / 180.0 * (double)angle));
            py = (int)(x * Math.Sin(M_PI / 180.0 * (double)angle) + (-1.0) * y * Math.Cos(M_PI / 180.0 * (double)angle));
            xx = ox + px;
            yy = oy + py;
            index = yy * ImageWidth + xx;
            if (index >= 0 && index < image.Length) image[index] = color;


            px = (int)((-1.0) * x * Math.Cos(M_PI / 180.0 * (double)angle) - y * Math.Sin(M_PI / 180.0 * (double)angle));
            py = (int)((-1.0) * x * Math.Sin(M_PI / 180.0 * (double)angle) + y * Math.Cos(M_PI / 180.0 * (double)angle));
            xx = ox + px;
            yy = oy + py;
            index = yy * ImageWidth + xx;
            if (index >= 0 && index < image.Length) image[index] = color;


            px = (int)((-1.0) * x * Math.Cos(M_PI / 180.0 * (double)angle) - (-1.0) * y * Math.Sin(M_PI / 180.0 * (double)angle));
            py = (int)((-1.0) * x * Math.Sin(M_PI / 180.0 * (double)angle) + (-1.0) * y * Math.Cos(M_PI / 180.0 * (double)angle));
            xx = ox + px;
            yy = oy + py;
            index = yy * ImageWidth + xx;
            if (index >= 0 && index < image.Length) image[index] = color;
        }

        static void BresEllipse(int ox, int oy, int a, int b, double angle, byte[] image, byte color)
        {
            int x, y, p, sa4, sb4, tx, ty;

            x = 0;
            y = b;
            sa4 = 4 * a * a;
            sb4 = 4 * b * b;
            tx = (int)(a * a / Math.Sqrt((double)(a * a + b * b)) + 0.5);
            ty = (int)(b * b / Math.Sqrt((double)(a * a + b * b)) + 0.5);
            p = a * a + 8 * b * b - 2 * a * a * b;
            while (x <= tx)
            {
                display(ox, oy, x, y, angle, image, color);
                if (p < 0)
                {
                    p += sb4 * x;
                }
                else
                {
                    p += sb4 * x - sa4 * y + sa4;
                    y--;
                }
                x++;
            }
            x = a;
            y = 0;
            p = b * b + 8 * a * a - 2 * b * b * a;
            while (y <= ty)
            {
                display(ox, oy, x, y, angle, image, color);
                if (p < 0)
                {
                    p += sa4 * y;
                }
                else
                {
                    p += sa4 * y - sb4 * x + sb4;
                    x--;
                }
                y++;
            }
        }
        #endregion

    }

    #region Image processing result
    /// <summary>
    /// ImageProcessingResult
    /// </summary>
    public class ImageProcessingResult
    {
        const int MaxRegions = SimpleVisionState.MaxRegions;

        // color object detection result
        /// <summary>
        /// Object Found flag
        /// </summary>
        public bool ObjectFound;
        /// <summary>
        /// X Mean
        /// </summary>
        public int XMean;
        /// <summary>
        /// Y Mean
        /// </summary>
        public int YMean;
        /// <summary>
        /// Object Area
        /// </summary>
        public int Area;

        /// <summary>
        /// X StdDev
        /// </summary>
        public double XStdDev;
        /// <summary>
        /// Y StdDev
        /// </summary>
        public double YStdDev;
        /// <summary>
        /// X Skew
        /// </summary>
        public double XSkew;
        /// <summary>
        /// Y Skew
        /// </summary>
        public double YSkew;

        // motion detection result
        /// <summary>
        /// Motion Found flag
        /// </summary>
        public bool MotionFound;
        /// <summary>
        /// X Motion
        /// </summary>
        public int XMotion;
        /// <summary>
        /// Y Motion
        /// </summary>
        public int YMotion;
        /// <summary>
        /// Motion Size
        /// </summary>
        public int MotionSize;

        // head detection result
        /// <summary>
        /// Head Found flag
        /// </summary>
        public bool HeadFound;
        /// <summary>
        /// Head Found On ColorObject flag
        /// </summary>
        public bool HeadFoundOnColorObject;
        /// <summary>
        /// RectangleType of HeadBoxRegion
        /// </summary>
        public RectangleType HeadBoxRegion;

        // hand gestures detection result
        /// <summary>
        /// Left HandGesture Found flag
        /// </summary>
        public bool LeftHandGestureFound;
        /// <summary>
        /// LeftHand X
        /// </summary>
        public int LeftHandX;
        /// <summary>
        /// LeftHand Y
        /// </summary>
        public int LeftHandY;
        /// <summary>
        /// Right HandGesture Found flag
        /// </summary>
        public bool RightHandGestureFound;
        /// <summary>
        /// RightHand X
        /// </summary>
        public int RightHandX;
        /// <summary>
        /// RightHand Y
        /// </summary>
        public int RightHandY;

        // SegmentationRegions result
        /// <summary>
        /// number of segmented Locations
        /// </summary>
        public int numberOfLocations;
        /// <summary>
        /// array of location for segmantation
        /// </summary>
        public PointType[] location = new PointType[MaxRegions];
        /// <summary>
        /// array of boundBox for segmantation
        /// </summary>
        public RectangleType[] boundBox = new RectangleType[MaxRegions];
        /// <summary>
        /// array of size for segmantation
        /// </summary>
        public int[] size = new int[MaxRegions];

        // FindHead and FindHeadRegionCandidates result
        /// <summary>
        /// foundHeadIndex in headBox array
        /// </summary>
        public int foundHeadIndex;
        /// <summary>
        /// number of HeadCandidates after FindHeadRegionCandidates
        /// </summary>
        public int numberOfHeadCandidates;
        /// <summary>
        /// array of headBox
        /// </summary>
        public RectangleType[] headBox = new RectangleType[MaxRegions];
    }
    #endregion
}
