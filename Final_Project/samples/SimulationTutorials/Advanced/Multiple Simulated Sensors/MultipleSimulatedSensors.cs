//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: MultipleSimulatedSensors.cs $ $Revision: 7 $
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Ccr.Core;
using Microsoft.Dss.Core.Attributes;
using Microsoft.Dss.ServiceModel.Dssp;
using Microsoft.Dss.ServiceModel.DsspServiceBase;
using W3C.Soap;


#region Service Using Statements
using Microsoft.Dss.Core;
using System.Drawing;
using Microsoft.Ccr.Adapters.WinForms;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using Microsoft.Robotics.Simulation.Engine;

using submgr = Microsoft.Dss.Services.SubscriptionManager;
using engine = Microsoft.Robotics.Simulation.Engine.Proxy;
using simulatedwebcam = Microsoft.Robotics.Services.Simulation.Sensors.SimulatedWebcam.Proxy;
using webcam = Microsoft.Robotics.Services.WebCam.Proxy;
using simulateddrive = Microsoft.Robotics.Services.Simulation.Drive.Proxy;
using drive = Microsoft.Robotics.Services.Drive.Proxy;

using laserrangefinder = Microsoft.Robotics.Services.Simulation.Sensors.LaserRangeFinder.Proxy;
using simsonar = Microsoft.Robotics.Services.Simulation.Sensors.Sonar.Proxy;
using sicklrf = Microsoft.Robotics.Services.Sensors.SickLRF.Proxy;
using infrared = Microsoft.Robotics.Services.Simulation.Sensors.Infrared.Proxy;
using colorsensor = Microsoft.Robotics.Services.Simulation.Sensors.ColorSensor.Proxy;
using brightnesssensor = Microsoft.Robotics.Services.Simulation.Sensors.BrightnessSensor.Proxy;
using compass = Microsoft.Robotics.Services.Simulation.Sensors.Compass.Proxy;
using analogsensor = Microsoft.Robotics.Services.AnalogSensor.Proxy;
using sonar = Microsoft.Robotics.Services.Sonar.Proxy;
#endregion

namespace Robotics.MultipleSimulatedSensors
{
    [Contract(Contract.Identifier)]
    [DisplayName("(User) Multiple Simulated Sensors")]
    [Description("Shows how to read and process data from multiple sensors")]
    class MultipleSimulatedSensorsService : DsspServiceBase
    {
        /// <summary>
        /// Service state
        /// </summary>
        [ServiceState]
        MultipleSimulatedSensorsState _state = new MultipleSimulatedSensorsState();

        /// <summary>
        /// Main service port
        /// </summary>
        [ServicePort("/MultipleSimulatedSensors", AllowMultipleInstances = true)]
        MultipleSimulatedSensorsOperations _mainPort = new MultipleSimulatedSensorsOperations();

        [SubscriptionManagerPartner]
        submgr.SubscriptionManagerPort _submgrPort = new submgr.SubscriptionManagerPort();

        #region Partners
        /// <summary>
        /// SimulationEngine partner
        /// </summary>
        [Partner("SimulationEngine", Contract = engine.Contract.Identifier, CreationPolicy = PartnerCreationPolicy.UseExisting)]
        engine.SimulationEnginePort _simulationEnginePort = new engine.SimulationEnginePort();
        engine.SimulationEnginePort _simulationEngineNotify = new engine.SimulationEnginePort();

        /// <summary>
        /// SimulatedWebcamService partner
        /// </summary>
        [Partner("SimulatedWebcamService", Contract = webcam.Contract.Identifier, CreationPolicy = PartnerCreationPolicy.UsePartnerListEntry)]
        webcam.WebCamOperations _simulatedWebcamServicePort = new webcam.WebCamOperations();

        /// <summary>
        /// SimulatedCompass partner
        /// </summary>
        [Partner("SimulatedCompass", Contract = analogsensor.Contract.Identifier, CreationPolicy = PartnerCreationPolicy.UsePartnerListEntry)]
        analogsensor.AnalogSensorOperations _simulatedCompassPort = new analogsensor.AnalogSensorOperations();

        /// <summary>
        /// SimulatedBrightnessCell partner
        /// </summary>
        [Partner("SimulatedBrightnessCell", Contract = analogsensor.Contract.Identifier, CreationPolicy = PartnerCreationPolicy.UsePartnerListEntry)]
        analogsensor.AnalogSensorOperations _simulatedBrightnessCellPort = new analogsensor.AnalogSensorOperations();

        /// <summary>
        /// SimulatedColorSensor partner
        /// </summary>
        [Partner("SimulatedColorSensor", Contract = colorsensor.Contract.Identifier, CreationPolicy = PartnerCreationPolicy.UsePartnerListEntry)]
        colorsensor.ColorSensorOperations _simulatedColorSensorPort = new colorsensor.ColorSensorOperations();

        /// <summary>
        /// SimulatedLRFService partner
        /// </summary>
        [Partner("SimulatedLRFService", Contract = sicklrf.Contract.Identifier, CreationPolicy = PartnerCreationPolicy.UsePartnerListEntry)]
        sicklrf.SickLRFOperations _simulatedLRFServicePort = new sicklrf.SickLRFOperations();

        /// <summary>
        /// SimulatedSonarService partner
        /// </summary>
        [Partner("SimulatedSonarService", Contract = sonar.Contract.Identifier, CreationPolicy = PartnerCreationPolicy.UsePartnerListEntry)]
        sonar.SonarOperations _simulatedSonarServicePort = new sonar.SonarOperations();

        /// <summary>
        /// SimulatedIRService partner
        /// </summary>
        [Partner("SimulatedIRService", Contract = analogsensor.Contract.Identifier, CreationPolicy = PartnerCreationPolicy.UsePartnerListEntry)]
        analogsensor.AnalogSensorOperations _simulatedIRServicePort = new analogsensor.AnalogSensorOperations();

        /// <summary>
        /// SimulatedDifferentialDriveService partner
        /// </summary>
        [Partner("SimulatedDifferentialDriveService", Contract = drive.Contract.Identifier, CreationPolicy = PartnerCreationPolicy.UsePartnerListEntry)]
        drive.DriveOperations _simulatedDifferentialDriveServicePort = new drive.DriveOperations();
        
        #endregion

        /// <summary>
        /// Service constructor
        /// </summary>
        public MultipleSimulatedSensorsService(DsspServiceCreationPort creationPort)
            : base(creationPort)
        {
        }

        #region Data Members
        Port<DateTime> _dateTimePort = new Port<DateTime>();

        // used to display gradient we compute
        ImageProcessingResultForm _imageProcessingForm;
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

            #region Run WinForm and Activate Timer
            WinFormsServicePort.Post(new RunForm(() =>
            {
                _imageProcessingForm = new ImageProcessingResultForm();
                _imageProcessingForm.Show();

                Activate(Arbiter.ReceiveWithIterator(false, _dateTimePort, UpdateSensorData));
                TaskQueue.EnqueueTimer(TimeSpan.FromMilliseconds(60), _dateTimePort);
            
                return _imageProcessingForm;
            }));
            #endregion
        }

        #region Helper Methods
        class CompletionPort : Port<bool> { }
        IEnumerator<ITask> PostOnTaskCompletionHelper(CompletionPort completionPort, IteratorHandler handler)
        {
            yield return new IterativeTask(handler);
            completionPort.Post(true);
        }
        void PostOnTaskCompletion(CompletionPort completionPort, IteratorHandler handler)
        {
            SpawnIterator<CompletionPort, IteratorHandler>(completionPort, handler, PostOnTaskCompletionHelper);
        }

        bool HasError<T>(PortSet<T, Fault> sensorOrFault)
        {
            Fault fault = (Fault)sensorOrFault;
            if (fault != null)
            {
                LogError(fault.ToException());
                return true;
            }
            else
                return false;
        }
        #endregion

        #region UpdateSensorData
        IEnumerator<ITask> UpdateSensorData(DateTime dateTime)
        {
            var resultPort = new CompletionPort();
            PostOnTaskCompletion(resultPort, UpdateColorSensor);
            PostOnTaskCompletion(resultPort, UpdateBrightnessSensor);
            PostOnTaskCompletion(resultPort, UpdateCompass);
            PostOnTaskCompletion(resultPort, UpdateLRF);
            PostOnTaskCompletion(resultPort, UpdateSonar);
            PostOnTaskCompletion(resultPort, UpdateInfrared);
            PostOnTaskCompletion(resultPort, UpdateWebCamImage);

            Activate(Arbiter.MultipleItemReceive(false, resultPort, 7, allComplete =>
                {
                    Activate(Arbiter.ReceiveWithIterator(false, _dateTimePort, UpdateSensorData));
                    TaskQueue.EnqueueTimer(TimeSpan.FromMilliseconds(60), _dateTimePort);
                }));
            
            yield break;
        }
        #endregion

        #region LRF, Sonar, and Infrared Sensors
        IEnumerator<ITask> UpdateLRF() 
        {
            var sensorOrFault = _simulatedLRFServicePort.Get();
            yield return sensorOrFault.Choice();

            if(!HasError(sensorOrFault))
            {
                sicklrf.State sensorState = (sicklrf.State)sensorOrFault;
                WinFormsServicePort.Post(new FormInvoke(() =>
                {
                    _imageProcessingForm.SetLRFData(sensorState.DistanceMeasurements, sensorState.Units);
                }));
            }
            yield break;
        }
        IEnumerator<ITask> UpdateSonar()
        {
            var sensorOrFault = _simulatedSonarServicePort.Get();
            yield return sensorOrFault.Choice();

            if (!HasError(sensorOrFault))
            {
                sonar.SonarState sensorState = (sonar.SonarState)sensorOrFault;
                WinFormsServicePort.Post(new FormInvoke(() =>
                {
                    _imageProcessingForm.SetSonarReadingValue(sensorState.DistanceMeasurement);
                }));
            }
            yield break;
        }
        #endregion

        #region Color, Brightness, and Color sensor readings
        IEnumerator<ITask> UpdateColorSensor()
        {
            var sensorOrFault = _simulatedColorSensorPort.Get();
            yield return sensorOrFault.Choice();

            if (!HasError(sensorOrFault))
            {
                colorsensor.ColorSensorState sensorState = (colorsensor.ColorSensorState)sensorOrFault;
                WinFormsServicePort.Post(new FormInvoke(() =>
                    {
                        _imageProcessingForm.SetColorReadingValue(sensorState.NormalizedAverageBlue,
                            sensorState.NormalizedAverageGreen, sensorState.NormalizedAverageBlue);
                    }));
            }
        }
        IEnumerator<ITask> UpdateBrightnessSensor()
        {
            var sensorOrFault = _simulatedBrightnessCellPort.Get();
            yield return sensorOrFault.Choice();

            if (!HasError(sensorOrFault))
            {
                analogsensor.AnalogSensorState sensorState = (analogsensor.AnalogSensorState)sensorOrFault;
                WinFormsServicePort.Post(new FormInvoke(() =>
                {
                    _imageProcessingForm.SetBrightnessReadingValue(sensorState.NormalizedMeasurement);
                }));
            }
        }
        IEnumerator<ITask> UpdateCompass()
        {
            var sensorOrFault = _simulatedCompassPort.Get();
            yield return sensorOrFault.Choice();

            if (!HasError(sensorOrFault))
            {
                analogsensor.AnalogSensorState sensorState = (analogsensor.AnalogSensorState)sensorOrFault;
                WinFormsServicePort.Post(new FormInvoke(() =>
                {
                    _imageProcessingForm.SetCompassReadingValue(sensorState.RawMeasurement);
                }));
            }
        }
        IEnumerator<ITask> UpdateInfrared()
        {
            var sensorOrFault = _simulatedIRServicePort.Get();
            yield return sensorOrFault.Choice();

            if (!HasError(sensorOrFault))
            {
                analogsensor.AnalogSensorState sensorState = (analogsensor.AnalogSensorState)sensorOrFault;
                WinFormsServicePort.Post(new FormInvoke(() =>
                {
                    _imageProcessingForm.SetIRReadingValue(sensorState.RawMeasurement);
                }));
            }
            yield break;
        }
        #endregion

        #region Webcam Related
        IEnumerator<ITask> UpdateWebCamImage()
        {
            byte[] rgbData = null;
            Size size = new Size(0, 0);

            yield return Arbiter.Choice(_simulatedWebcamServicePort.QueryFrame(),
                success =>
                {
                    rgbData = success.Frame;
                    size = success.Size;
                },
                failure =>
                {
                    LogError(failure.ToException());
                });

            if (rgbData != null)
            {
                ComputeGradient(ref rgbData, size);
                UpdateBitmap(rgbData, size);
            }
        }

        private void UpdateBitmap(byte[] rgbData, Size size)
        {
            if (_imageProcessingForm == null)
                return;

            WinFormsServicePort.Post(new FormInvoke(() =>
                {
                    Bitmap bmp = _imageProcessingForm.WebcamBitmap;
                    CopyBytesToBitmap(rgbData, size.Width, size.Height, ref bmp);
                    if (bmp != _imageProcessingForm.WebcamBitmap)
                    {
                        _imageProcessingForm.UpdateWebcamImage(bmp);
                    }
                    _imageProcessingForm.Invalidate(true);
                }));
        }

        #region Basic Image Processing Methods
        private void ComputeGradient(ref byte[] rgbData, Size size)
        {
            byte[] gradient = new byte[rgbData.Length];
            int[,] mask = new [,]
                {
                    {+2, +1, 0},
                    {+1, 0, -1},
                    {0, -1, -2}
                };
            const int filterSize = 3;
            const int halfFilterSize = filterSize/2;

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
                    byte byteResult = Clamp(Math.Abs(result*5.0f), 0.0f, 255.0f);
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
        static internal void CopyBytesToBitmap(byte[] srcData, int srcDataWidth, int srcDataHeight, ref Bitmap destBitmap)
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
        #endregion

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
            // cancel any active tasks that are still active
            TaskQueue.Dispose();

            DefaultDropHandler(drop);
            yield break;
        }
    }
}


