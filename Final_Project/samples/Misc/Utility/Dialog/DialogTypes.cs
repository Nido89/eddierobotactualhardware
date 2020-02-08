//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: DialogTypes.cs $ $Revision: 17 $
//-----------------------------------------------------------------------
#if !URT_MINCLR
using Microsoft.Ccr.Core;
using Microsoft.Dss.Core.Attributes;
using Microsoft.Dss.ServiceModel.Dssp;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using W3C.Soap;


using dialog = Microsoft.Robotics.Services.Sample.Dialog;

namespace Microsoft.Robotics.Services.Sample.Dialog
{

    /// Timer Contract
    public static class Contract
    {
        /// The Unique Contract Identifier for the Timer service
        public const string Identifier = "http://schemas.microsoft.com/robotics/2006/08/dialog.user.html";
    }

    /// <summary>
    /// Simple Dialog Operations PortSet
    /// </summary>
    [ServicePort]
    public class DialogOperations : PortSet<DsspDefaultLookup, DsspDefaultDrop, Alert, Prompt, Confirm>
    {
    }

    /// <summary>
    /// Alert - Display an Alert Dialog
    /// </summary>
    [DisplayName("(User) AlertDialog")]
    [Description("Displays a simple text message dialog to the user.")]
    public class Alert : Submit<AlertRequest, PortSet<DefaultSubmitResponseType, Fault>>
    {
    }

    /// <summary>
    /// Prompt - Display a Prompt Dialog
    /// </summary>
    [DisplayName("(User) PromptDialog")]
    [Description("Displays a simple dialog that prompts the user to enter a text string.")]
    public class Prompt : Submit<PromptRequest, PortSet<PromptResponse, Fault>>
    {
    }

    /// <summary>
    /// Confirm - Display a Confirm Dialog
    /// </summary>
    [DisplayName("(User) ConfirmDialog")]
    [Description("Displays a simple dialog that asks the user a question and provides OK and Cancel buttons.")]
    public class Confirm : Submit<ConfirmRequest, PortSet<ConfirmResponse, Fault>>
    {
    }

    /// <summary>
    /// AlertRequest
    /// </summary>
    [DataContract]
    public class AlertRequest
    {
        private string _message;
        /// <summary>
        /// Message - Message to display
        /// </summary>
        [DataMember]
        [DisplayName("(User) AlertText")]
        [Description("Specifies the message text in the dialog.")]
        public string Message
        {
            get { return _message; }
            set { _message = value; }
        }
    }

    /// <summary>
    /// PromptRequest
    /// </summary>
    [DataContract]
    public class PromptRequest
    {
        private string _message;
        /// <summary>
        /// Message - Message to display as a prompt
        /// </summary>
        [DataMember]
        [DisplayName("(User) PromptText")]
        [Description("Specifies the prompt text in the dialog.")]
        public string Message
        {
            get { return _message; }
            set { _message = value; }
        }

        private string _defaultValue;
        /// <summary>
        /// DefaultValue - Initial value in the text field
        /// </summary>
        [DataMember]
        [DisplayName("(User) DefaultValue")]
        [Description("Specifies the text in the dialog's TextBox.")]
        public string DefaultValue
        {
            get { return _defaultValue; }
            set { _defaultValue = value; }
        }
    }

    /// <summary>
    /// PromptResponse
    /// </summary>
    [DataContract]
    public class PromptResponse
    {
        private bool _confirmed;
        /// <summary>
        /// Confirmed - True if OK was pressed, false otherwise
        /// </summary>
        [DataMember]
        [DisplayName("(User) Confirmed")]
        [Description("Returns whether the dialog's OK (true) or Cancel (false) button was clicked.")]
        public bool Confirmed
        {
            get { return _confirmed; }
            set { _confirmed = value; }
        }

        private string _textData;
        /// <summary>
        /// TextData - The text that was entered by the user
        /// </summary>
        [DataMember]
        [DisplayName("(User) TextData")]
        [Description("Returns the text in the TextBox when the OK button is clicked, or the default text if Cancel is clicked.")]
        public string TextData
        {
            get { return _textData; }
            set { _textData = value; }
        }
    }

    /// <summary>
    /// ConfirmRequest
    /// </summary>
    [DataContract]
    public class ConfirmRequest
    {
        private string _message;
        /// <summary>
        /// Message - Message to display
        /// </summary>
        [DataMember]
        [DisplayName("(User) ConfirmText")]
        [Description("Specifies the text to show in the dialog.")]
        public string Message
        {
            get { return _message; }
            set { _message = value; }
        }
    }

    /// <summary>
    /// ConfirmResponse
    /// </summary>
    [DataContract]
    public class ConfirmResponse
    {
        private bool _confirmed;
        /// <summary>
        /// Confirmed - True if OK was pressed, false otherwise
        /// </summary>
        [DataMember]
        [DisplayName("(User) Confirmed")]
        [Description("Returns true if the OK button was clicked, false if Cancel (or Close) was clicked.")]
        public bool Confirmed
        {
            get { return _confirmed; }
            set { _confirmed = value; }
        }
    }
}
#endif
