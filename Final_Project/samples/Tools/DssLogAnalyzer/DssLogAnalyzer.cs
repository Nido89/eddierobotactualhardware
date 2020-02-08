//------------------------------------------------------------------------------
//  <copyright file="DsslogAnalyzer.cs" company="Microsoft Corporation">
//      Copyright (C) Microsoft Corporation.  All rights reserved.
//  </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Robotics.Tools.DssLogAnalyzer
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Threading;
    using Microsoft.Ccr.Core;
    using Microsoft.Dss.Core.Attributes;
    using Microsoft.Dss.ServiceModel.Dssp;
    using Microsoft.Dss.ServiceModel.DsspServiceBase;
    using Microsoft.Robotics.Tools.DssLogAnalyzer.Properties;
    using W3C.Soap;
    using ccrwpf = Microsoft.Ccr.Adapters.Wpf;
    using submgr = Microsoft.Dss.Services.SubscriptionManager;

    /// <summary>
    /// DssLogAnalyzer service class
    /// </summary>
    [Contract(Contract.Identifier)]
    [DisplayName("(User) DssLogAnalyzer")]
    [Description("DssLogAnalyzer service (no description provided)")]
    public partial class DssLogAnalyzerService : DsspServiceBase
    {
        /// <summary>
        /// The default process timeout
        /// </summary>
        private const int DefaultProcessTimeout = 2000;

        /// <summary>
        /// Service state
        /// </summary>
        [InitialStatePartner(Optional = true)]
        private DssLogAnalyzerState serviceState = new DssLogAnalyzerState();

        /// <summary>
        /// Main service port
        /// </summary>
        [ServicePort("/DssLogAnalyzer", AllowMultipleInstances = true)]
        private DssLogAnalyzerOperations mainPort = new DssLogAnalyzerOperations();

        /// <summary>
        /// Subscription manager port
        /// </summary>
        [SubscriptionManagerPartner]
        private submgr.SubscriptionManagerPort submgrPort = new submgr.SubscriptionManagerPort();

        /// <summary>
        /// WPF service port
        /// </summary>
        private ccrwpf.WpfServicePort wpfServicePort;

        /// <summary>
        /// Main user interface object
        /// </summary>
        private UserInterface userInterface;

        /// <summary>
        /// Initializes a new instance of the <see cref="DssLogAnalyzerService"/> class.
        /// </summary>
        /// <param name="creationPort">Creation port</param>
        public DssLogAnalyzerService(DsspServiceCreationPort creationPort)
            : base(creationPort)
        {
        }

        /// <summary>
        /// Handles Subscribe messages
        /// </summary>
        /// <param name="subscribe">The subscribe request</param>
        [ServiceHandler]
        public void SubscribeHandler(Subscribe subscribe)
        {
            SubscribeHelper(this.submgrPort, subscribe.Body, subscribe.ResponsePort);
        }

        /// <summary>
        /// Handles files loaded messages
        /// </summary>
        /// <param name="logFilesLoaded">Which files loaded</param>
        [ServiceHandler]
        public void LogFilesLoadedHandler(LogFilesLoaded logFilesLoaded)
        {
            SendNotification(this.submgrPort, logFilesLoaded);
            logFilesLoaded.ResponsePort.Post(DefaultUpdateResponseType.Instance);
        }

        /// <summary>
        /// Handles enevelope selected messages
        /// </summary>
        /// <param name="envelopesSelected">Which envelope selected</param>
        [ServiceHandler]
        public void EnvelopesSelectedHandler(EnvelopesSelected envelopesSelected)
        {
            SendNotification(this.submgrPort, envelopesSelected);
            envelopesSelected.ResponsePort.Post(DefaultUpdateResponseType.Instance);
        }

        /// <summary>
        /// Handler for drop messages
        /// </summary>
        /// <param name="drop">Drop message</param>
        /// <returns>ITask enumerator</returns>
        [ServiceHandler(ServiceHandlerBehavior.Teardown)]
        public IEnumerator<ITask> DropHandler(DsspDefaultDrop drop)
        {
            DefaultDropHandler(drop);

            yield break;
        }

        /// <summary>
        /// Load files
        /// </summary>
        /// <param name="logFilesFolder">Root file directory</param>
        internal void Load(string logFilesFolder)
        {
            LogFileResult loadResult;
            if (this.serviceState != null && string.IsNullOrEmpty(logFilesFolder) == false)
            {
                loadResult = this.LoadLogs(logFilesFolder);

                if (loadResult == LogFileResult.Success)
                {
                    this.DisplayInfo(Resources.LogFilesLoaded);
                    this.RefreshUserInterface();
                }
                else if (loadResult == LogFileResult.XmlLogsDetected)
                {
                    // need to prompt user where to save the new binary files
                    var newLogFolderPort = new PortSet<string, Fault>();
                    this.WpfInvoke(() => this.userInterface.PromptForNewLogFolder(newLogFolderPort));

                    Activate(newLogFolderPort.Choice(
                        newLogFilesFolder =>
                        {
                            if (string.IsNullOrEmpty(newLogFilesFolder) == false)
                            {
                                this.WpfInvoke(() => this.userInterface.ShowProgressBar());
                                
                                var resultPort = ConvertXmlLogsToBinary(
                                                    logFilesFolder, 
                                                    newLogFilesFolder,
                                                    progressUpdate => this.WpfInvoke(() => this.userInterface.UpdateProgress(progressUpdate)));
                                                    Activate(resultPort.Receive(
                                                    logFileResult =>
                                                    {
                                                    this.WpfInvoke(() => this.userInterface.HideProgressBar());

                                                    if (logFileResult == LogFileResult.Success)
                                                    {
                                                        Load(newLogFilesFolder);
                                                    }
                                }));
                            }
                        },
                        fault => { /*user canceled load*/ }));
                }
            }
        }

        /// <summary>
        /// Refresh the user interface
        /// </summary>
        internal void RefreshUserInterface()
        {
            this.WpfInvoke(() => this.userInterface.RefreshUI(this.serviceState.LogFileEnvelopes, this.serviceState.LogFilesFolder));
        }

        /// <summary>
        /// Shutdown the log analyzer
        /// </summary>
        internal void ShutdownLogAnalyzer()
        {
            SpawnIterator(this.ShutdownSuccessfullyOrKillProcess);
        }

        /// <summary>
        /// Update selected envelopes
        /// </summary>
        /// <param name="currentSelectedEnvelopes">Current selected envelope</param>
        internal void UpdateSelectedEnvelopes(EnvelopeList currentSelectedEnvelopes)
        {
            Interlocked.Exchange(ref this.serviceState.CurrentSelectedEnvelopes, currentSelectedEnvelopes);

            this.mainPort.Post(new EnvelopesSelected());
        }

        /// <summary>
        /// Service start
        /// </summary>
        protected override void Start()
        {
            base.Start();

            SpawnIterator(this.Initialize);
        }

        /// <summary>
        /// Initialize service
        /// </summary>
        /// <returns>ITask enumerator</returns>
        private IEnumerator<ITask> Initialize()
        {
            if (this.serviceState == null)
            {
                this.serviceState = new DssLogAnalyzerState();
                this.serviceState.LogFilesFolder = string.Empty;
                this.serviceState.Envelopes = new List<EnvelopeList>();
            }

            if (this.serviceState.Headless == false)
            {
                this.wpfServicePort = ccrwpf.WpfAdapter.Create(TaskQueue);
                var runWindow = this.wpfServicePort.RunWindow(() => new UserInterface(this));
                yield return (Choice)runWindow;

                var exception = (Exception)runWindow;
                if (exception != null)
                {
                    LogError(exception);
                    StartFailed();
                    yield break;
                }

                this.userInterface = (UserInterface)runWindow;
            }

            this.Load();
        }

        /// <summary>
        /// Load files
        /// </summary>
        private void Load()
        {
            this.Load(this.serviceState.LogFilesFolder);
        }

        /// <summary>
        /// Invoke a wpf thread action method
        /// </summary>
        /// <param name="a">Which action to invoke</param>
        private void WpfInvoke(Action a)
        {
            if (this.wpfServicePort != null)
            {
                this.wpfServicePort.Invoke(() => a.Invoke());
            }
        }

        /// <summary>
        /// Display error
        /// </summary>
        /// <param name="errorMessage">Error message to display</param>
        private void DisplayError(string errorMessage)
        {
            this.WpfInvoke(() => this.userInterface.DisplayError(errorMessage));
        }

        /// <summary>
        /// Display an informational message
        /// </summary>
        /// <param name="infoMessage">Message to display</param>
        private void DisplayInfo(string infoMessage)
        {
            this.WpfInvoke(() => this.userInterface.DisplayInfo(infoMessage));
        }

        /// <summary>
        /// Shutdown or kill process
        /// </summary>
        /// <returns>ITask enumerator</returns>
        private IEnumerator<ITask> ShutdownSuccessfullyOrKillProcess()
        {
            var shutdownOrTimedOut = new SuccessFailurePort();
            Activate(TimeoutPort(DefaultProcessTimeout).Receive(dateTime => shutdownOrTimedOut.Post(new Exception())));
            Activate(shutdownOrTimedOut.Choice(
                success => { }, /*cleanly shutdown*/
                failure => /*timed out*/ System.Diagnostics.Process.GetCurrentProcess().Kill()));

            ControlPanelPort.Post(new DsspDefaultDrop());
            Activate(Arbiter.ReceiveWithIterator<DsspDefaultDrop>(false, this.mainPort, this.DropHandler));

            shutdownOrTimedOut.Post(SuccessResult.Instance);

            yield break;
        }
    }
}
