//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
// 
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: TextToSpeechUI.xaml.cs $ $Revision: 5 $
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
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
using Microsoft.Robotics.Technologies.Speech.TextToSpeech.Proxy;

namespace Microsoft.Dss.Services.Samples.WpfTextToSpeechUI
{
    /// <summary>
    /// Interaction logic for TextToSpeechUI.xaml
    /// </summary>
    public partial class TextToSpeechUI : Window
    {
        #region Viseme
        /// <summary>
        /// Gets or sets the most recent viseme that was received from the TTS service
        /// </summary>
        public VisemeNotification Viseme
        {
            get { return (VisemeNotification)GetValue(VisemeProperty); }
            set { SetValue(VisemeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Viseme.  This enables animation, styling, binding, etc...
        /// <summary>
        /// VisemeProperty
        /// </summary>
        public static readonly DependencyProperty VisemeProperty =
            DependencyProperty.Register("Viseme", typeof(VisemeNotification), typeof(TextToSpeechUI));
        #endregion

        WpfTextToSpeechUIService _service;

        /// <summary>
        /// Creates a new instance of the user interface
        /// </summary>
        public TextToSpeechUI()
        {
            InitializeComponent();

            // set the initial value so that something is displayed
            Viseme = new VisemeNotification();

            Closed += TextToSpeechUI_Closed;
        }

        void TextToSpeechUI_Closed(object sender, EventArgs e)
        {
            MessageBox.Show(
                this,
                Properties.Resources.NodeIsNotClosed,
                Title,
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        /// <summary>
        /// Creates a new instance of the user interface
        /// </summary>
        /// <param name="service">the service that handles communication with the TTS service</param>
        internal TextToSpeechUI(WpfTextToSpeechUIService service)
            : this()
        {
            _service = service;
        }

        #region Say It
        /// <summary>
        /// This method is called when the "Say it!" button is pressed.
        /// </summary>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // let the service do it
            if (_service != null)
            {
                _service.SayTextFromUi(_text.Text);
            }
        }
        #endregion

        /// <summary>
        /// Displays a fault message
        /// </summary>
        /// <param name="fault">fault</param>
        internal void ShowFault(W3C.Soap.Fault fault)
        {
            var error = Properties.Resources.ErrorMessage;

            if (fault.Reason != null && fault.Reason.Length > 0 && !string.IsNullOrEmpty(fault.Reason[0].Value))
            {
                error = fault.Reason[0].Value;
            }

            MessageBox.Show(
                this,
                error,
                Title,
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }
}
