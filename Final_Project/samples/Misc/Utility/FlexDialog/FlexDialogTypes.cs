//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: FlexDialogTypes.cs $ $Revision: 8 $
//-----------------------------------------------------------------------
#if !URT_MINCLR
using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Ccr.Core;
using Microsoft.Dss.Core.Attributes;
using Microsoft.Dss.ServiceModel.Dssp;
using System.ComponentModel;
using Microsoft.Dss.Core.DsspHttp;
using Microsoft.Dss.Core;

namespace Microsoft.Robotics.Services.Sample.FlexDialog
{
    /// <summary>
    /// Flexible Dialog contract
    /// </summary>
    public static class Contract
    {
        /// <summary>
        /// Contract Identifier
        /// </summary>
        public const string Identifier = "http://schemas.microsoft.com/robotics/2007/08/flexdialog.user.html";
    }

    /// <summary>
    /// Flexible Dialog State
    /// </summary>
    [DataContract]
    [Description("The state of the Flexible Dialog service.")]
    public class FlexDialogState
    {
        private string _title;
        /// <summary>
        /// Title - Appears in the dialog's title bar
        /// </summary>
        [DataMember]
        [Description("Specifies the title of the dialog box.")]
        public string Title
        {
            get { return _title; }
            set { _title = value; }
        }

        private bool _visible;
        /// <summary>
        /// Visible - Determines if dialog is visible or not
        /// </summary>
        [DataMember]
        [Description("Specifies if the dialog is visible.")]
        public bool Visible
        {
            get { return _visible; }
            set { _visible = value; }
        }

        private List<FlexControl> _controls = new List<FlexControl>();
        /// <summary>
        /// Controls - List of controls on the dialog
        /// </summary>
        [DataMember(IsRequired = true)]
        [Description("The set of controls included in the dialog (displayed vertically).")]
        public List<FlexControl> Controls
        {
            get { return _controls; }
            set { _controls = value; }
        }

        private List<FlexButton> _buttons = new List<FlexButton>();
        /// <summary>
        /// Buttons - List of buttons on the dialog
        /// </summary>
        [DataMember(IsRequired = true)]
        [Description("The collection of buttons included in the dialog (displayed horizontally).")]
        public List<FlexButton> Buttons
        {
            get { return _buttons; }
            set { _buttons = value; }
        }
    }

    /// <summary>
    /// FlexControl - Defines a control on the dialog
    /// </summary>
    [DataContract]
    [Description("Defines a control in the dialog (vertically).")]
    public class FlexControl
    {
        private string _id;
        /// <summary>
        /// Id - Name of the control (must be unique)
        /// </summary>
        [DataMember, DataMemberConstructor]
        [Description("Specifies the unique identifier for the control.\n(This parameter takes a string value.)")]
        public string Id
        {
            get { return _id; }
            set { _id = value; }
        }

        private FlexControlType _controlType;
        /// <summary>
        /// ControlType - Type of this UI control
        /// </summary>
        [DataMember, DataMemberConstructor]
        [Description("Specifies the type of UI control.")]
        public FlexControlType ControlType
        {
            get { return _controlType; }
            set { _controlType = value; }
        }

        private string _text;
        /// <summary>
        /// Text - Text that appears in the control
        /// </summary>
        [DataMember, DataMemberConstructor]
        [Description("Specifies the text that appears for the UI control.\nFor ComboBoxes, this property specifies the text that appears in the list\n(separated by |).\nThis property is ignored for TextBox, multiline TextBox, and Separator controls.")]
        public string Text
        {
            get { return _text; }
            set { _text = value; }
        }

        private string _value;
        /// <summary>
        /// Value - Data entered into the control (Textbox and MultiLineTextbox)
        /// </summary>
        [DataMember, DataMemberConstructor]
        [Description("Specifies the current value of the control.\nThis property must be true or false for a Checkbox or RadioButton.\nThis property is ignored for Label, Button, or Separator controls.")]
        public string Value
        {
            get { return _value; }
            set { _value = value; }
        }

        /// <summary>
        /// CompareId - See if two controls are the same
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool CompareId(FlexControl other)
        {
            return string.CompareOrdinal(_id, other._id) == 0;
        }
    }

    /// <summary>
    /// FlexButton - Special case of FlexControl
    /// </summary>
    [DataContract]
    [Description("Defines a button that will appear horizontally in the dialog.")]
    public class FlexButton : FlexControl
    {
    }

    /// <summary>
    /// FlexControlType - Types of UI controls
    /// </summary>
    [DataContract]
    public enum FlexControlType
    {
        /// <summary>
        /// Label - Readonly text
        /// </summary>
        Label,
        /// <summary>
        /// TextBox - Input text
        /// </summary>
        TextBox,
        /// <summary>
        /// MultiLineTextBox - Textbox that can contain line breaks
        /// </summary>
        MultiLineTextBox,
        /// <summary>
        /// Button - Pushbutton
        /// </summary>
        Button,
        /// <summary>
        /// CheckBox - True or False
        /// </summary>
        CheckBox,
        /// <summary>
        /// RadioButton - One of many
        /// </summary>
        RadioButton,
        /// <summary>
        /// ComboBox - Combination TextBox and Dropdown List
        /// </summary>
        ComboBox,
        /// <summary>
        /// Seperator - Dividing line
        /// </summary>
        Seperator,
    }

    /// <summary>
    /// ButtonPressRequest
    /// </summary>
    [DataContract]
    public class ButtonPressRequest
    {
        private string _id;
        /// <summary>
        /// Id - Button id
        /// </summary>
        [DataMember, DataMemberConstructor]
        [Description ("The unique identifier for the control.")]
        public string Id
        {
            get { return _id; }
            set { _id = value; }
        }

        private bool _pressed;
        /// <summary>
        /// Pressed - True if pressed
        /// </summary>
        [DataMember, DataMemberConstructor]
        [Description("Returns if the button pressed.\nReturns true if pressed, otherwise false.")]
        public bool Pressed
        {
            get { return _pressed; }
            set { _pressed = value; }
        }
    }

    /// <summary>
    /// ShowRequest - Sets the Visible property
    /// </summary>
    [DataContract]
    public class ShowRequest
    {
        /// <summary>
        /// Default Constructor
        /// </summary>
        public ShowRequest()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="show"></param>
        public ShowRequest(bool show)
        {
            _show = show;
        }

        private bool _show;
        /// <summary>
        /// Show - True means visible, False means invisible
        /// </summary>
        [DataMember, DataMemberConstructor]
        [Description("Indicates whether the dialog is currently visible.\nReturns true if visible, otherwise false.")]
        public bool Show
        {
            get { return _show; }
            set { _show = value; }
        }
    }

    /// <summary>
    /// SetTitleRequest - Info for setting Title
    /// </summary>
    [DataContract]
    public class SetTitleRequest
    {
        private string _title;
        /// <summary>
        /// Title - The title for the dialog's title bar
        /// </summary>
        [DataMember, DataMemberConstructor]
        [Description("Specifies the title of the dialog.")]
        public string Title
        {
            get { return _title; }
            set { _title = value; }
        }
    }

    class InternalUpdate : IDssSerializable
    {
        /// <summary>
        /// Default Constructor
        /// </summary>
        private InternalUpdate()
        {
        }

        /// <summary>
        /// Instance - 
        /// </summary>
        static public readonly InternalUpdate Instance = new InternalUpdate();

        #region ICloneable Members

        /// <summary>
        /// Clone - Make a copy of this object
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            return this;
        }

        #endregion

        #region IDssSerializable Members

        /// <summary>
        /// CopyTo - NOT IMPLEMENTED
        /// </summary>
        /// <param name="target"></param>
        public void CopyTo(IDssSerializable target)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        /// <summary>
        /// Deserialize - NOT IMPLEMENTED
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public object Deserialize(System.IO.BinaryReader reader)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        /// <summary>
        /// Serialize - NOT IMPLEMENTED
        /// </summary>
        /// <param name="writer"></param>
        public void Serialize(System.IO.BinaryWriter writer)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion
    }

    /// <summary>
    /// HttpNotification
    /// </summary>
    [DataContract]
    public class HttpNotification
    {
        private string _operation;
        /// <summary>
        /// Operation 
        /// </summary>
        [DataMember]
        public string Operation
        {
            get { return _operation; }
            set { _operation = value; }
        }

        private FlexControl _control;
        /// <summary>
        /// Control - The control the notification is from
        /// </summary>
        [DataMember]
        public FlexControl Control
        {
            get { return _control; }
            set { _control = value; }
        }

        private FlexButton _button;
        /// <summary>
        /// Button - The button the notification is from
        /// </summary>
        [DataMember]
        public FlexButton Button
        {
            get { return _button; }
            set { _button = value; }
        }

        private string _title;
        /// <summary>
        /// Title - Dialog title
        /// </summary>
        [DataMember]
        public string Title
        {
            get { return _title; }
            set { _title = value; }
        }

        private ShowRequest _show;
        /// <summary>
        /// Show - True if dialog is visible
        /// </summary>
        [DataMember]
        public ShowRequest Show
        {
            get { return _show; }
            set { _show = value; }
        }

        private ButtonPressRequest _buttonPress;
        /// <summary>
        /// ButtonPress
        /// </summary>
        [DataMember]
        public ButtonPressRequest ButtonPress
        {
            get { return _buttonPress; }
            set { _buttonPress = value; }
        }

        private SetTitleRequest _setTitle;
        /// <summary>
        /// SetTitle
        /// </summary>
        [DataMember]
        public SetTitleRequest SetTitle
        {
            get { return _setTitle; }
            set { _setTitle = value; }
        }

        private HandOffMessage _handOff;
        /// <summary>
        /// HandOff
        /// </summary>
        [DataMember]
        public HandOffMessage HandOff
        {
            get { return _handOff; }
            set { _handOff = value; }
        }

        /// <summary>
        /// FromOperation - Sends a notification
        /// </summary>
        /// <param name="operation"></param>
        /// <returns></returns>
        public static HttpNotification FromOperation(DsspOperation operation)
        {
            HttpNotification notification = new HttpNotification();

            notification._operation = operation.GetType().Name;

            if (operation.Body is FlexButton)
            {
                notification._button = operation.Body as FlexButton;
            }
            else if (operation.Body is FlexControl)
            {
                notification._control = operation.Body as FlexControl;
            }
            else if (operation.Body is ShowRequest)
            {
                notification._show = operation.Body as ShowRequest;
            }
            else if (operation.Body is ButtonPressRequest)
            {
                notification._buttonPress = operation.Body as ButtonPressRequest;
            }
            else if (operation.Body is SetTitleRequest)
            {
                notification._setTitle = operation.Body as SetTitleRequest;
            }
            else if (operation.Body is HandOffMessage)
            {
                notification._handOff = operation.Body as HandOffMessage;
            }

            return notification;
        }
    }

    /// <summary>
    /// HandOffMessage - Used to transfer control to another dialog
    /// </summary>
    [DataContract]
    public class HandOffMessage
    {
        private string _title;
        /// <summary>
        /// Title - Title of the dialog to transfer to
        /// </summary>
        [DataMember,DataMemberConstructor]
        public string Title
        {
            get { return _title; }
            set { _title = value; }
        }

        private string _service;
        /// <summary>
        /// Service - Service to transfer to
        /// </summary>
        [DataMember,DataMemberConstructor]
        public string Service
        {
            get { return _service; }
            set { _service = value; }
        }
    }

    /// <summary>
    /// Flexible Dialog Operations PortSet
    /// </summary>
    [ServicePort]
    public class FlexDialogOperations : PortSet<DsspDefaultLookup, DsspDefaultDrop, Get, HttpGet, HttpPost, HttpQuery, Subscribe, InsertButton, DeleteButton, UpdateButton, InsertControl, DeleteControl, UpdateControl, ButtonPress, Show, SetTitle, HandOff>
    {
    }

    /// <summary>
    /// Get - Return the state of the dialog
    /// </summary>
    [Description("Returns the current state of the dialog.")]
    public class Get : Get<GetRequestType, DsspResponsePort<FlexDialogState>>
    {
    }

    /// <summary>
    /// Subscribe - Request nofitications
    /// </summary>
    public class Subscribe : Subscribe<SubscribeRequestType, DsspResponsePort<SubscribeResponseType>>
    {
    }

    /// <summary>
    /// InsertControl - Add a control to the dialog
    /// </summary>
    [Description("Adds a control (or indicates a control has been added)\nto the dialog.")]
    public class InsertControl : Insert<FlexControl, DsspResponsePort<DefaultUpdateResponseType>>
    {
    }

    /// <summary>
    /// DeleteControl - Remove a control from the dialog
    /// </summary>
    [Description("Removes a control (or indicates a control has been removed)\nfrom the dialog.")]
    public class DeleteControl : Delete<FlexControl, DsspResponsePort<DefaultDeleteResponseType>>
    {
    }

    /// <summary>
    /// UpdateControl - Change a control or its contents
    /// </summary>
    [Description("Updates a control or its contents (or indicates a control has been updated)\nin the dialog.")] 
    public class UpdateControl : Update<FlexControl, DsspResponsePort<DefaultUpdateResponseType>>
    {
        /// <summary>
        /// Default Constructor
        /// </summary>
        public UpdateControl()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="body"></param>
        public UpdateControl(FlexControl body)
            : base(body)
        {
            AddHeader(InternalUpdate.Instance);
        }
    }

    /// <summary>
    /// InsertButton - Add a button to the dialog
    /// </summary>
    [Description("Adds a button (or indicates a button has been added)\nto the set of horizontal buttons in the dialog.")]
    public class InsertButton : Insert<FlexButton, DsspResponsePort<DefaultUpdateResponseType>>
    {
    }

    /// <summary>
    /// DeleteButton - Remove a button from the dialog
    /// </summary>
    [Description("Removes a button (or indicates a button has been removed)\nfrom the set of horizontal buttons in the dialog.")]
    public class DeleteButton : Delete<FlexButton, DsspResponsePort<DefaultDeleteResponseType>>
    {
    }

    /// <summary>
    /// UpdateButton - Change a button on the dialog
    /// </summary>
    [Description("Updates (or indicates an update to)\nthe state of a horizontal button in the dialog.")]
    public class UpdateButton : Update<FlexButton, DsspResponsePort<DefaultUpdateResponseType>>
    {
    }

    /// <summary>
    /// ButtonPress - Notification of button presses
    /// </summary>
    [Description("Indicates that a button has been pressed.")]
    public class ButtonPress : Update<ButtonPressRequest, DsspResponsePort<DefaultUpdateResponseType>>
    {
    }

    /// <summary>
    /// Show - Display or Hide the dialog
    /// </summary>
    [Description ("Displays or hides the dialog, or indicates\nthat the dialog is now visible or hidden.")]
    public class Show : Update<ShowRequest, DsspResponsePort<DefaultUpdateResponseType>>
    {
        /// <summary>
        /// Default Constructor
        /// </summary>
        public Show()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="body"></param>
        public Show(ShowRequest body)
            : base(body)
        {
        }
    }

    /// <summary>
    /// SetTitle - Set dialog Title
    /// </summary>
    [Description("Sets the title for the dialog, or indicates\nthat the title has been changed.")]
    public class SetTitle : Update<SetTitleRequest, DsspResponsePort<DefaultUpdateResponseType>>
    {
    }

    /// <summary>
    /// HandOff - Switch to another dialog (only applies to web browser)
    /// </summary>
    [Description("Switches focus to another dialog, or \nindicates that focus has been changed.")]
    public class HandOff : Update<HandOffMessage, DsspResponsePort<HandOffMessage>>
    {
    }
}
#endif