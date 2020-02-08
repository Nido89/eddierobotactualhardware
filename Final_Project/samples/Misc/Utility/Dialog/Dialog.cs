//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: Dialog.cs $ $Revision: 17 $
//-----------------------------------------------------------------------
#if !URT_MINCLR
using Microsoft.Ccr.Core;
using Microsoft.Dss.Core;
using Microsoft.Dss.Core.Attributes;
using Microsoft.Dss.ServiceModel.Dssp;
using Microsoft.Dss.ServiceModel.DsspServiceBase;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Security.Permissions;
using xml = System.Xml;
using sm = Microsoft.Dss.Services.SubscriptionManager;
using System.Text;
using Microsoft.Ccr.Adapters.WinForms;
using W3C.Soap;

namespace Microsoft.Robotics.Services.Sample.Dialog
{
    /// <summary>
    /// DialogService - Provides Alert, Confirm and Prompt dialogs
    /// </summary>
    /// <remarks>Provides a way to display Alert, Confirm or Prompt dialogs using Windows Forms</remarks>
    [DisplayName("(User) Simple Dialog")]
    [Description("Provides display of simple message dialogs.\n(These dialogs automatically timeout after 60 seconds.)")]
    [Contract(Contract.Identifier)]
    [DssServiceDescription("http://msdn.microsoft.com/library/dd126871.aspx")]
    public class DialogService : DsspServiceBase
    {
        [ServicePort("/dialog", AllowMultipleInstances = true)]
        private DialogOperations _mainPort = new DialogOperations();

        /// <summary>
        /// Default Service Constructor
        /// </summary>
        public DialogService(DsspServiceCreationPort creationPort) :
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

        static TimeSpan _defaultTimeout = new TimeSpan(0, 0, 50);

        /// <summary>
        /// Alert Handler - Processes requests for an Alert dialog
        /// </summary>
        /// <param name="alert">Alert Request</param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public virtual IEnumerator<ITask> AlertHandler(Alert alert)
        {
            SuccessFailurePort result = new SuccessFailurePort();

            AlertForm form = null;

            RunForm runForm = new RunForm(
                delegate()
                {
                    form = new AlertForm(result);
                    form.Message = alert.Body.Message;
                    form.Countdown = _defaultTimeout;

                    return form;
                }
            );
            WinFormsServicePort.Post(runForm);

            yield return Arbiter.Choice(
                runForm.pResult,
                delegate(SuccessResult success){},
                delegate(Exception e)
                {
                    result.Post(e);
                }
            );

            yield return Arbiter.Choice(
                result,
                delegate(SuccessResult success)
                {
                    alert.ResponsePort.Post(DefaultSubmitResponseType.Instance);

                    if (form.Timeout)
                    {
                        LogWarning("Alert dialog timed out.");
                    }
                },
                delegate(Exception e)
                {
                    Fault fault = Fault.FromException(e);
                    LogError(null, "Error in Alert Handler", fault);
                    alert.ResponsePort.Post(fault);
                }
            );
        }

        /// <summary>
        /// Confirm Handler - Processes requests for a Confirm dialog
        /// </summary>
        /// <param name="confirm">Confirm request</param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public virtual IEnumerator<ITask> ConfirmHandler(Confirm confirm)
        {
            PortSet<bool, Exception> result = new PortSet<bool, Exception>();
            ConfirmForm form = null;

            RunForm runForm = new RunForm(
                delegate()
                {
                    form = new ConfirmForm(result);
                    form.Message = confirm.Body.Message;
                    form.Countdown = _defaultTimeout;

                    return form;
                }
            );
            WinFormsServicePort.Post(runForm);

            yield return Arbiter.Choice(
                runForm.pResult,
                delegate(SuccessResult success) { },
                delegate(Exception e)
                {
                    result.Post(e);
                }
            );

            yield return Arbiter.Choice(
                result,
                delegate(bool confirmed)
                {
                    ConfirmResponse response = new ConfirmResponse();
                    response.Confirmed = confirmed;

                    confirm.ResponsePort.Post(response);

                    if (form.Timeout)
                    {
                        LogWarning("Confirm dialog cancelled due to timeout.");
                    }
                },
                delegate(Exception e)
                {
                    Fault fault = Fault.FromException(e);
                    LogError(null, "Error in Confirm Handler", fault);
                    confirm.ResponsePort.Post(fault);
                }
            );
        }

        /// <summary>
        /// Prompt Handler - Processes requests for a Prompt dialog
        /// </summary>
        /// <param name="prompt">Prompt request</param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public virtual IEnumerator<ITask> PromptHandler(Prompt prompt)
        {
            PortSet<string, Exception> result = new PortSet<string, Exception>();
            PromptForm form = null;

            RunForm runForm = new RunForm(
                delegate()
                {
                    form = new PromptForm(result);
                    form.Message = prompt.Body.Message;
                    form.DefaultValue = prompt.Body.DefaultValue;
                    form.Countdown = _defaultTimeout;

                    return form;
                }
            );
            WinFormsServicePort.Post(runForm);

            yield return Arbiter.Choice(
                runForm.pResult,
                delegate(SuccessResult success) { },
                delegate(Exception e)
                {
                    result.Post(e);
                }
            );

            yield return Arbiter.Choice(
                result,
                delegate(string value)
                {
                    PromptResponse response = new PromptResponse();
                    response.TextData = value;
                    response.Confirmed = true;

                    prompt.ResponsePort.Post(response);
                },
                delegate(Exception e)
                {
                    if (e.Message == "Cancelled")
                    {
                        // If the Cancel button was pressed, return the default text
                        PromptResponse response = new PromptResponse();
                        response.TextData = prompt.Body.DefaultValue;
                        response.Confirmed = false;

                        prompt.ResponsePort.Post(response);

                        if (form.Timeout)
                        {
                            LogWarning("Prompt dialog cancelled due to timeout.");
                        }
                    }
                    else
                    {
                        Fault fault = Fault.FromException(e);
                        LogError(null, "Error in Prompt Handler", fault);
                        prompt.ResponsePort.Post(fault);
                    }
                }
            );
        }
    }
}
#endif
