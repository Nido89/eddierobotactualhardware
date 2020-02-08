//------------------------------------------------------------------------------
//  <copyright file="RawViewer.cs" company="Microsoft Corporation">
//      Copyright (C) Microsoft Corporation.  All rights reserved.
//  </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Robotics.Tools.DssLogAnalyzerPlugins.RawViewer
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Text;
    using System.Xml;
    using Microsoft.Ccr.Core;
    using Microsoft.Dss.Core.Attributes;
    using Microsoft.Dss.ServiceModel.Dssp;
    using Microsoft.Dss.ServiceModel.DsspServiceBase;
    using Microsoft.Dss.Services.Serializer;
    using ccrwpf = Microsoft.Ccr.Adapters.Wpf;
    using loganalyzer = Microsoft.Robotics.Tools.DssLogAnalyzer;
    using submgr = Microsoft.Dss.Services.SubscriptionManager;

    /// <summary>
    /// Raw Viewer Plug in Service
    /// </summary>
    [Contract(Contract.Identifier)]
    [DisplayName("(User) RawViewer")]
    [Description("RawViewer service (shows xml deserialized view of the object)")]
    public class RawViewerPluginService : DsspServiceBase, loganalyzer.IDssLogAnalyzerPluginService
    {
        /// <summary>
        /// Service state
        /// </summary>
        [ServiceState]
        private RawViewerState state = new RawViewerState();

        /// <summary>
        /// Main service port
        /// </summary>
        [ServicePort("/RawViewer", AllowMultipleInstances = true)]
        private RawViewerOperations mainPort = new RawViewerOperations();

        /// <summary>
        /// Sub Mgr Port
        /// </summary>
        [SubscriptionManagerPartner]
        private submgr.SubscriptionManagerPort submgrPort = new submgr.SubscriptionManagerPort();

        /// <summary>
        /// DssLogAnalyzerService partner
        /// </summary>
        [Partner("DssLogAnalyzerService", Contract = loganalyzer.Contract.Identifier, CreationPolicy = PartnerCreationPolicy.UsePartnerListEntry)]
        private loganalyzer.DssLogAnalyzerOperations logAnalyzer = new loganalyzer.DssLogAnalyzerOperations();

        /// <summary>
        /// WPF Service Port
        /// </summary>
        private ccrwpf.WpfServicePort wpfServicePort;

        /// <summary>
        /// User Interface
        /// </summary>
        private RawViewerPluginUi userInterface;

        /// <summary>
        /// Initializes a new instance of the <see cref="RawViewerPluginService"/> class.
        /// </summary>
        /// <param name="creationPort">The creation port.</param>
        public RawViewerPluginService(DsspServiceCreationPort creationPort)
            : base(creationPort)
        {
        }

        /// <summary>
        /// Updates the UI from lists of envelopes.
        /// </summary>
        /// <param name="listOfEnvelopes">The list of envelopes.</param>
        /// <returns>Yield Break on WPF Service Port Invoke</returns>
        public IEnumerator<ITask> UpdateUiFromListsOfEnvelopes(List<loganalyzer.EnvelopeList> listOfEnvelopes)
        {
            var envelopes = new List<loganalyzer.Envelope>();

            foreach (var list in listOfEnvelopes)
            {
                envelopes.AddRange(list.Envelopes);
            }

            envelopes.Sort(new SortByTimestamp());

            this.wpfServicePort.Invoke(() => this.userInterface.UpdateUiFromEnvelopes(envelopes));

            yield break;
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

        #region IDssLogAnalyzerPluginService Members

        /// <summary>
        /// ITask enumerator interface for processing selected envelopes
        /// </summary>
        /// <param name="envelopesSelected">Envelopes selected</param>
        /// <returns>
        /// ITask enumerator
        /// </returns>
        public IEnumerator<ITask> EnvelopesSelected(loganalyzer.EnvelopesSelected envelopesSelected)
        {
            var get = new loganalyzer.Get();
            this.logAnalyzer.Post(get);

            loganalyzer.DssLogAnalyzerState logAnalyzerState = null;

            yield return get.ResponsePort.Choice(
                state => logAnalyzerState = state,
                fault => LogError(fault));

            if (logAnalyzerState == null)
            {
                yield break;
            }

            var envelopes = logAnalyzerState.CurrentSelectedEnvelopes;

            yield return new IterativeTask(() => this.UpdateUiFromListsOfEnvelopes(new List<loganalyzer.EnvelopeList> { envelopes }));
        }

        /// <summary>
        /// ITask enumerator interface for processing loaded log files
        /// </summary>
        /// <param name="logFilesLoaded">Which log files have been loaded</param>
        /// <returns>
        /// ITask enumerator
        /// </returns>
        public IEnumerator<ITask> LogFilesLoaded(loganalyzer.LogFilesLoaded logFilesLoaded)
        {
            if (this.wpfServicePort == null)
            {
                this.wpfServicePort = ccrwpf.WpfAdapter.Create(TaskQueue);
                var runWindow = this.wpfServicePort.RunWindow(() => new RawViewerPluginUi(this));
                yield return (Choice)runWindow;

                var exception = (Exception)runWindow;
                if (exception != null)
                {
                    LogError(exception);
                    StartFailed();
                    yield break;
                }

                this.userInterface = (RawViewerPluginUi)runWindow;
            }

            var get = new loganalyzer.Get();
            this.logAnalyzer.Post(get);

            loganalyzer.DssLogAnalyzerState logAnalyzerState = null;

            yield return get.ResponsePort.Choice(
                state => logAnalyzerState = state,
                fault => LogError(fault));

            if (logAnalyzerState == null)
            {
                yield break;
            }

            var listOfEnvelopes = logAnalyzerState.Envelopes;

            yield return new IterativeTask(() => this.UpdateUiFromListsOfEnvelopes(listOfEnvelopes));

            yield break;
        }

        #endregion

        /// <summary>
        /// Deserializes the envelope body.
        /// </summary>
        /// <param name="envelope">The envelope.</param>
        /// <param name="callback">The callback.</param>
        internal void DeserializeEnvelopeBody(loganalyzer.Envelope envelope, Action<string> callback)
        {
            Activate(
                Arbiter.Choice(
                    DeserializeEnvelopeBody(envelope),
                    text => this.wpfServicePort.Invoke(() => callback(text)),
                    failure => LogError(failure)));
        }

        /// <summary>
        /// Deserializes the envelope body.
        /// </summary>
        /// <param name="envelope">The envelope.</param>
        /// <returns>Returns the Result Port</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Reliability",
            "CA2000:Dispose objects before losing scope",
            Justification = "The writer lives beyond the scope because its referenced in an async task. " +
                            "Also, in this case, it doesn't hold anything other than GC resources.")]
        internal PortSet<string, Exception> DeserializeEnvelopeBody(loganalyzer.Envelope envelope)
        {
            var xmlResultPort = new PortSet<XmlWriter, Exception>();

            var o = loganalyzer.DssLogAnalyzerHelper.DeserializeEnvelope(envelope).Body;

            var stringbuilderOutput = new StringBuilder();
            var xmlWriter = XmlWriter.Create(stringbuilderOutput, new XmlWriterSettings { Indent = true });

            var ser = new SerializeToXmlWriter(o, xmlWriter, xmlResultPort);
            SerializerPort.Post(ser);

            var resultPort = new PortSet<string, Exception>();

            Activate(
                Arbiter.Choice(
                    xmlResultPort,
                    success => resultPort.Post(stringbuilderOutput.ToString()),
                    failure => resultPort.Post(failure)));

            return resultPort;
        }

        /// <summary>
        /// Service start
        /// </summary>
        protected override void Start()
        {
            // Add service specific initialization here
            base.Start();

            SpawnIterator(this.Initialize);
        }

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        /// <returns>Yield Returns on the Choice</returns>
        private IEnumerator<ITask> Initialize()
        {
            // subscribe to log analyzer
            var notificationPort = new loganalyzer.DssLogAnalyzerOperations();
            Activate(Arbiter.ReceiveWithIterator<loganalyzer.LogFilesLoaded>(true, notificationPort, this.LogFilesLoaded));
            Activate(Arbiter.ReceiveWithIterator<loganalyzer.EnvelopesSelected>(true, notificationPort, this.EnvelopesSelected));

            var sub = new loganalyzer.Subscribe();
            sub.NotificationPort = notificationPort;
            sub.Body.TypeFilter = new string[] 
            { 
                GetTypeFilterDescription<loganalyzer.LogFilesLoaded>(),
                GetTypeFilterDescription<loganalyzer.EnvelopesSelected>()
            };
            this.logAnalyzer.Post(sub);
            yield return sub.ResponsePort.Choice(
                success => { },
                fault => LogError(fault));
        }

        /// <summary>
        /// Sort by Time Stamp
        /// </summary>
        private class SortByTimestamp : IComparer<loganalyzer.Envelope>
        {
            #region IComparer<loganalyzer.Envelope> Members

            /// <summary>
            /// Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
            /// </summary>
            /// <param name="x">The first object to compare.</param>
            /// <param name="y">The second object to compare.</param>
            /// <returns>
            /// Value
            /// Condition
            /// Less than zero
            /// <paramref name="x"/> is less than <paramref name="y"/>.
            /// Zero
            /// <paramref name="x"/> equals <paramref name="y"/>.
            /// Greater than zero
            /// <paramref name="x"/> is greater than <paramref name="y"/>.
            /// </returns>
            public int Compare(loganalyzer.Envelope x, loganalyzer.Envelope y)
            {
                return x.Header.TimeStamp.CompareTo(y.Header.TimeStamp);
            }

            #endregion
        }
    }
}