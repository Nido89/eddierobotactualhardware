//------------------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//  <copyright file="MainPage.xaml.cs" company="Microsoft Corporation">
//      Copyright (C) Microsoft Corporation.  All rights reserved.
//  </copyright>
//  $File: MainPage.xaml.cs $ $Revision: 1 $
//------------------------------------------------------------------------------

namespace StopWatch
{
    using System;
    using System.Collections.Generic;
    using System.Windows;
    using System.Windows.Controls;

    using Microsoft.Ccr.Core;

    /// <summary>
    /// The Application's MainPage
    /// </summary>
    public partial class MainPage : UserControl, IDisposable
    {
        /// <summary>
        /// CCR DispatcherQueue
        /// </summary>
        private DispatcherQueue taskQueue;

        /// <summary>
        /// PausableStopWatch for MainPage
        /// </summary>
        private PausableStopWatch pausableStopWatch;

        /// <summary>
        /// Track whether Dispose has been called.
        /// </summary>
        private bool disposed = false;
        
        /// <summary>
        /// Initializes a new isntance of the MainPage class.
        /// </summary>
        public MainPage()
        {
            this.InitializeComponent();

            this.taskQueue = new DispatcherQueue();
            this.pausableStopWatch = new PausableStopWatch();
        }

        /// <summary>
        /// Handle the MainPage OnLoaded
        /// </summary>
        /// <param name="sender">Object sending the RoutedEventArgs</param>
        /// <param name="e">RoutedEventArgs sent by the Object</param>
        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            this.taskQueue.SpawnIterator<DateTime>(DateTime.Now, this.CCRMainLoop);
        }

        /// <summary>
        /// Implement IDisposable.
        /// Do not make this method virtual.
        /// A derived class should not be able to override this method.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);

            // This object will be cleaned up by the Dispose method.
            // Therefore, you should call GC.SupressFinalize to
            // take this object off the finalization queue
            // and prevent finalization code for this object
            // from executing a second time.
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose(bool disposing) executes in two distinct scenarios.
        /// If disposing equals true, the method has been called directly
        /// or indirectly by a user's code. Managed and unmanaged resources
        /// can be disposed.
        /// If disposing equals false, the method has been called by the
        /// runtime from inside the finalizer and you should not reference
        /// other objects. Only unmanaged resources can be disposed.
        /// </summary>
        /// <param name="disposing">Determines which Dispose scenario to run.</param>
        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!this.disposed)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing)
                {
                    // Dispose managed resources.
                    this.taskQueue.Dispose();
                }

                // Note disposing has been done.
                this.disposed = true;
            }
        }

        /// <summary>
        /// Task that is triggered via a timer to explicitly handle CCR loop.
        /// </summary>
        /// <param name="ts">DateTime of triggered IterativeTask</param>
        /// <returns>CCR task iterator</returns>
        private IEnumerator<ITask> CCRMainLoop(DateTime ts)
        {
            yield return new IterativeTask(() => this.RenderDateTime());

            yield return new IterativeTask(() => this.RenderPausableStopWatch());

            this.taskQueue.Activate(
                Arbiter.ReceiveWithIterator(
                    false,
                    this.taskQueue.TimeoutPort(TimeSpan.FromSeconds(1 / 30.0)),
                    this.CCRMainLoop));
        }

        /// <summary>
        /// Task that is Executed by the CCRMainLoop
        /// Responsible for Rendering the Current DateTime
        /// </summary>
        /// <returns>CCR task iterator</returns>
        private IEnumerator<ITask> RenderDateTime()
        {
            DateTime renderTime = DateTime.Now;

            Dispatcher.BeginInvoke(() =>
            {
                this.lblDate.Content = renderTime.ToString("dddd, dd MMMM yyyy");
                this.lblTime.Content = renderTime.ToString("hh:mm:ss tt");
            });

            yield break;
        }

        /// <summary>
        /// Task that is Executed by the CCRMainLoop
        /// Responsible for Rendering the PausableStopWatch Elapsed DateTime
        /// </summary>
        /// <returns>CCR task iterator</returns>
        private IEnumerator<ITask> RenderPausableStopWatch()
        {
            DateTime renderTime = new DateTime(this.pausableStopWatch.Elapsed.Ticks);

            Dispatcher.BeginInvoke(() =>
            {
                this.lblTimer.Content = renderTime.ToString("m:ss.ff");
            });

            yield break;
        }

        /// <summary>
        /// Starts/Pauses the PausableStopWatch
        /// </summary>
        /// <param name="sender">Object sending the RoutedEventArgs</param>
        /// <param name="e">RoutedEventArgs sent by the Object</param>
        private void StartClick(object sender, RoutedEventArgs e)
        {
            if (this.pausableStopWatch.IsRunning)
            {
                this.pausableStopWatch.Stop();
                this.btnStart.Content = "Start";
            }
            else
            {
                this.pausableStopWatch.Start();
                this.btnStart.Content = "Pause";
            }
        }

        /// <summary>
        /// Resets the PausableStopWatch
        /// </summary>
        /// <param name="sender">Object sending the RoutedEventArgs</param>
        /// <param name="e">RoutedEventArgs sent by the Object</param>
        private void ResetClick(object sender, RoutedEventArgs e)
        {
            this.btnStart.Content = "Start";
            this.pausableStopWatch.Reset();
        }
    }

    /// <summary>
    /// PausableStopWatch Class
    /// Derives from Microsoft.Ccr.Core.Stopwatch and adds pause/resume functionality
    /// </summary>
    public class PausableStopWatch : Stopwatch
    {
        /// <summary>
        /// Gets a value indicating whether PausableStopWatch is running
        /// </summary>
        public bool IsRunning { get; private set; }

        /// <summary>
        /// Gets or sets the running TotalTime for the PausableStopWatch
        /// </summary>
        private TimeSpan TotalTime { get; set; }

        /// <summary>
        /// Constructor for PausableStopWatch
        /// </summary>
        public PausableStopWatch()
            : base()
        {
        }

        /// <summary>
        /// Reset the PausableStopWatch
        /// </summary>
        public new void Reset()
        {
            this.IsRunning = false;
            this.TotalTime = TimeSpan.Zero;
            base.Reset();
        }

        /// <summary>
        /// Start the PausableStopWatch
        /// </summary>
        public new void Start()
        {
            this.IsRunning = true;
            this.TotalTime += base.Elapsed;
            base.Start();
        }

        /// <summary>
        /// Stop the PausableStopWatch
        /// </summary>
        public new void Stop()
        {
            this.IsRunning = false;
            base.Stop();
        }

        /// <summary>
        /// Gets the total elapsed TimeSpan of the PausableStopWatchs 
        /// </summary>
        public new TimeSpan Elapsed
        {
            get
            {
                return base.Elapsed + this.TotalTime;
            }
        }
    }
}
