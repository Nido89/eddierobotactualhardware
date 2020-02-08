//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: Sound.cs $ $Revision: 12 $
//-----------------------------------------------------------------------
#if !URT_MINCLR
using System;
using System.Collections.Generic;
using System.Text;
using System.Media;
using Microsoft.Dss.ServiceModel.DsspServiceBase;
using Microsoft.Dss.Core;
using Microsoft.Dss.Core.Attributes;
using System.ComponentModel;
using System.Security;
using System.Security.Permissions;
using Microsoft.Dss.ServiceModel.Dssp;
using Microsoft.Ccr.Core;
using W3C.Soap;

namespace Microsoft.Robotics.Services.Sample.Sound
{
    /// <summary>
    /// SoundService - Plays WAV (sound) files and system sounds
    /// </summary>
    [DisplayName("(User) Sound Player")]
    [Description("Plays .wav files and system sounds.")]
    [DssCategory(DssCategoryPrefixes.Root + "audio.html")]
    [Contract(Contract.Identifier)]
    [DssServiceDescription("http://msdn.microsoft.com/library/dd126873.aspx")]
    public class SoundService : DsspServiceBase
    {
        [ServicePort("sound", AllowMultipleInstances = false)]
        SoundOperations _mainPort = new SoundOperations();

        /// <summary>
        /// Constructor for service
        /// </summary>
        /// <param name="creationPort"></param>
        public SoundService(DsspServiceCreationPort creationPort) :
                base(creationPort)
        {
        }

        /// <summary>
        /// Service Start
        /// </summary>
        protected override void Start()
        {
            base.Start();
        }

        /// <summary>
        /// PlayHandler - Play a sound file
        /// </summary>
        /// <param name="play"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public virtual IEnumerator<ITask> PlayHandler(Play play)
        {
            SoundPlayer player = new SoundPlayer(play.Body.Filename);

            if (play.Body.Synchronous)
            {
                player.Load();
                player.PlaySync();
            }
            else
            {
                player.Play();
            }
            play.ResponsePort.Post(DefaultSubmitResponseType.Instance);
            yield break;
        }

        /// <summary>
        /// AsteriskHandler - Play the Windows "Asterisk" sound
        /// </summary>
        /// <param name="asterisk"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public virtual IEnumerator<ITask> AsteriskHandler(Asterisk asterisk)
        {
            return PlaySound(asterisk.Body.Synchronous, asterisk.ResponsePort, SystemSounds.Asterisk);
        }

        /// <summary>
        /// BeepHandler - Beep (what else?)
        /// </summary>
        /// <param name="beep"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public virtual IEnumerator<ITask> BeepHandler(Beep beep)
        {
            return PlaySound(beep.Body.Synchronous, beep.ResponsePort, SystemSounds.Beep);
        }

        /// <summary>
        /// ExclamationHandler - Play the Windows "Exclamation" sound
        /// </summary>
        /// <param name="exclamation"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public virtual IEnumerator<ITask> ExclamationHandler(Exclamation exclamation)
        {
            return PlaySound(exclamation.Body.Synchronous, exclamation.ResponsePort, SystemSounds.Exclamation);
        }

        /// <summary>
        /// HandHandler - Play the Windows "Hand" sound
        /// </summary>
        /// <param name="hand"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public virtual IEnumerator<ITask> HandHandler(Hand hand)
        {
            return PlaySound(hand.Body.Synchronous, hand.ResponsePort, SystemSounds.Hand);
        }

        /// <summary>
        /// QuestionHandler - Play the Windows "Question" sound
        /// </summary>
        /// <param name="question"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public virtual IEnumerator<ITask> QuestionHandler(Question question)
        {
            return PlaySound(question.Body.Synchronous, question.ResponsePort, SystemSounds.Question);
        }

        private IEnumerator<ITask> PlaySound(bool synchronous, PortSet<DefaultSubmitResponseType, Fault> responsePort, SystemSound systemSound)
        {
            if (synchronous)
            {
                systemSound.Play();
            }
            else
            {
                Spawn(delegate()
                    {
                        systemSound.Play();
                    }
                );
            }
            responsePort.Post(DefaultSubmitResponseType.Instance);
            yield break;
        }
    }
}
#endif