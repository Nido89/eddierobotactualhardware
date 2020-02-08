//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
// 
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: WpfTextToSpeechUI.cs $ $Revision: 5 $
//-----------------------------------------------------------------------


#region Tutorial outline
/* Tutorial outline
 * 
 * 1 Create new service with VS service wizard
 * 1.1 Disable "Use subscription manager" option
 * 1.2 Add TextToSpeech (TTS) service as partner. (name it "tts", add notification port)
 * 2 Subscribe to VisemeNotify
 * 2.1 Add empty handler for VisemeNotify
 * 3 Add reference to System.Linq
 * 4 Add reference to Microsoft.Ccr.Adapters.Wpf
 * 5 Project --> Add New Item ... 
 * 5.1 In "Categories" choose "WPF"
 * 5.2 Select "WPF User Control" and change name to "TextToSpeechUI"
 * 5.3 In TextToSpeechUI.xaml and TextToSpeechUI.xaml.cs change base class from UserControl to Window
 * 
 * 6. Implement code as in this project
 */
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Ccr.Core;
using Microsoft.Dss.Core.Attributes;
using Microsoft.Dss.ServiceModel.Dssp;
using Microsoft.Dss.ServiceModel.DsspServiceBase;
using W3C.Soap;

using texttospeech = Microsoft.Robotics.Technologies.Speech.TextToSpeech.Proxy;
using ccrwpf = Microsoft.Ccr.Adapters.Wpf;
using System.Windows;

namespace Microsoft.Dss.Services.Samples.WpfTextToSpeechUI
{
    /// <summary>
    /// WpfTextToSpeechUIService - Example of using WPF and TTS
    /// </summary>
    [Contract(Contract.Identifier)]
    [DisplayName("(User) WPF UI for TextToSpeech")]
    [Description("Sample WPF user interface for the TextToSpeech (TTS) service")]
    [DssServiceDescription("http://msdn.microsoft.com/library/cc998523.aspx")]
    class WpfTextToSpeechUIService : DsspServiceBase
    {
        [ServiceState]
        WpfTextToSpeechUIState _state = new WpfTextToSpeechUIState();

        [ServicePort("/WpfTextToSpeechUI", AllowMultipleInstances = true)]
        WpfTextToSpeechUIOperations _mainPort = new WpfTextToSpeechUIOperations();

        /// <summary>
        /// TextToSpeech (TTS) partner service.
        /// <remarks>
        /// If not configured this will connect to a local instance of the TTS service or create one. You can use
        /// a manifest to connect the user interface to a remotely running TTS service.
        /// </remarks>
        /// </summary>
        [Partner("tts", Contract = texttospeech.Contract.Identifier, CreationPolicy = PartnerCreationPolicy.UseExistingOrCreate)]
        texttospeech.SpeechTextOperations _ttsPort = new texttospeech.SpeechTextOperations();
        texttospeech.SpeechTextOperations _ttsNotifications = new texttospeech.SpeechTextOperations();

        ccrwpf.WpfServicePort _wpfServicePort;

        TextToSpeechUI _userInterface;

        /// <summary>
        /// Service constructor
        /// </summary>
        public WpfTextToSpeechUIService(DsspServiceCreationPort creationPort)
            : base(creationPort)
        {
        }

        /// <summary>
        /// Service start
        /// </summary>
        protected override void Start()
        {
            SpawnIterator(Initialize);
        }

        IEnumerator<ITask> Initialize()
        {
            #region Subscribe to TTS
            // subscribe to TTS service to receive viseme notifications
            var subscribe = _ttsPort.Subscribe(_ttsNotifications, typeof(texttospeech.VisemeNotify));
            
            yield return (Choice)subscribe;
            
            var fault = (Fault)subscribe;
            if (fault != null)
            {
                LogError(fault);
                StartFailed();
                yield break;
            }
            #endregion

            #region Create UI
            // create WPF adapter
            _wpfServicePort = ccrwpf.WpfAdapter.Create(TaskQueue);

            var runWindow = _wpfServicePort.RunWindow(() => new TextToSpeechUI(this));
            yield return (Choice)runWindow;

            var exception = (Exception)runWindow;
            if (exception != null)
            {
                LogError(exception);
                StartFailed();
                yield break;
            }

            // need double cast because WPF adapter doesn't know about derived window types
            _userInterface = (Window)runWindow as TextToSpeechUI;
            #endregion

            StartComplete();
        }

        // Complete the initialization
        void StartComplete()
        {
            base.Start();

            #region Activate TTS Notification Handler
            // activate a handler for viseme notifications
            MainPortInterleave.CombineWith(
                Arbiter.Interleave(
                    new TeardownReceiverGroup(),
                    new ExclusiveReceiverGroup(
                        Arbiter.Receive<texttospeech.VisemeNotify>(true, _ttsNotifications, VisemeNotifyHandler)
                    ),
                    new ConcurrentReceiverGroup()
                )
            );
            #endregion
        }

        #region Viseme Handler
        /// <summary>
        /// Handle viseme notifications from the TTS service
        /// </summary>
        /// <param name="viseme">notification</param>
        void VisemeNotifyHandler(texttospeech.VisemeNotify viseme)
        {
            // The notification handler executes outside context of the WPF dispatcher.
            // Because we want to change a property that causes changes to the UI we need
            // to set it in the WPF dispatcher.
            _wpfServicePort.Invoke(() =>
            {
                _userInterface.Viseme = viseme.Body;   
            });
        }
        #endregion

        /// <summary>
        /// Sends a say text request to the TTS service
        /// </summary>
        /// <param name="text">text to be spoken</param>
        /// <remarks>
        /// This method executes in the context of the WPF dispatcher.
        /// Any long running computations or blocking calls would make the
        /// user interface unresponsive. We simply hand over to DSS/CCR.
        /// </remarks>
        internal void SayTextFromUi(string text)
        {
            Activate(
                Arbiter.Choice(
                    _ttsPort.SayText(new texttospeech.SayTextRequest { SpeechText = text }),
                    success =>
                    {
                        // nothing to do
                    },
                    fault =>
                    {
                        // the fault handler is outside the WPF dispatcher
                        // to perfom any UI related operation we need to go through the WPF adapter

                        // show an error message
                        _wpfServicePort.Invoke(() => _userInterface.ShowFault(fault));
                    }
                )
            );
        }
    }
}


