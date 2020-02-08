//------------------------------------------------------------------------------
//  <copyright file="RawViewerPluginUI.xaml.cs" company="Microsoft Corporation">
//      Copyright (C) Microsoft Corporation.  All rights reserved.
//  </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Robotics.Tools.DssLogAnalyzerPlugins.RawViewer
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Documents;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Windows.Navigation;
    using System.Windows.Shapes;
    using loganalyzer = Microsoft.Robotics.Tools.DssLogAnalyzer;

    /// <summary>
    /// Interaction logic for RawViewerPluginUi.xaml
    /// </summary>
    public partial class RawViewerPluginUi : Window, loganalyzer.IDssLogAnalyzerPluginUi
    {
        /// <summary>
        /// Raw Viewer Plug in Service
        /// </summary>
        private RawViewerPluginService plugInService;
        
        /// <summary>
        /// Envelope for Log Analyzer
        /// </summary>
        private List<loganalyzer.Envelope> envelopes;

        /// <summary>
        /// Initializes a new instance of the <see cref="RawViewerPluginUi"/> class.
        /// </summary>
        /// <param name="plugInService">The service.</param>
        public RawViewerPluginUi(RawViewerPluginService plugInService)
        {
            this.plugInService = plugInService;

            this.InitializeComponent();
        }

        #region IDssLogAnalyzerPluginUi Members

        /// <summary>
        /// Interface to Update the UI from data
        /// </summary>
        /// <param name="envelopes">List of data envelopes</param>
        public void UpdateUiFromEnvelopes(List<loganalyzer.Envelope> envelopes)
        {
            this._selectFrameSlider.Maximum = envelopes.Count;
            this.envelopes = envelopes;

            if (this._selectFrameSlider.Value == 0)
            {
                this._selectFrameSlider_ValueChanged(null, null);
            }
            else
            {
                this._selectFrameSlider.Value = 0;
            }
        }

        #endregion

        /// <summary>
        /// Handles the ValueChanged event of the _selectFrameSlider control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event arguments.</param>
        private void _selectFrameSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var selectedEnvelopeIndex = (int)this._selectFrameSlider.Value;
            this.plugInService.DeserializeEnvelopeBody(this.envelopes[selectedEnvelopeIndex], resultingXml => this.DisplayEnvelope(resultingXml));
        }

        /// <summary>
        /// Displays the envelope.
        /// </summary>
        /// <param name="xml">The XML.</param>
        private void DisplayEnvelope(string xml)
        {
            if (xml == null)
            {
                return;
            }

            const int XmlLengthMax = 600;
            if (xml.Length > XmlLengthMax
                && this._displayLongContent.IsChecked.HasValue
                && this._displayLongContent.IsChecked.Value == false)
            {
                this._rawXmlText.Text = xml.Substring(0, XmlLengthMax); 
            }
            else
            {
                this._rawXmlText.Text = xml;
            }
        }

        /// <summary>
        /// Handles the Checked event of the _displayLongContent control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void _displayLongContent_Checked(object sender, RoutedEventArgs e)
        {
            this._selectFrameSlider_ValueChanged(null, null);
        }
    }
}
