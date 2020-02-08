//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: DirectionDialogTypes.cs $ $Revision: 4 $
//-----------------------------------------------------------------------
using Microsoft.Ccr.Core;
using Microsoft.Dss.Core.Attributes;
using Microsoft.Dss.ServiceModel.Dssp;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using W3C.Soap;

using directiondialog = Microsoft.Robotics.Services.Sample.DirectionDialog;


namespace Microsoft.Robotics.Services.Sample.DirectionDialog
{

    /// <summary>
    /// DirectionDialog Contract
    /// </summary>
    public static class Contract
    {
        /// The Unique Contract Identifier for the DirectionDialog service
        public const String Identifier = "http://schemas.microsoft.com/robotics/2006/08/directiondialog.user.html";
    }

    #region State
    /// <summary>
    /// DirectionDialogState
    /// </summary>
    [DataContract]
    public class DirectionDialogState
    {
        private List<Button> _buttons = new List<Button>();

        /// <summary>
        /// Buttons - List of buttons on the Form
        /// </summary>
        [DataMember(IsRequired = true)]
        public List<Button> Buttons
        {
            get { return _buttons ; }
            set { _buttons  = value; }
        }
    }
    #endregion

    #region Button Direction enum
    /// <summary>
    /// Button Directions
    /// </summary>
    [DataContract]
    public enum ButtonDirection
    {
        /// <summary>
        /// Stop
        /// </summary>
        Stop = 0,
        /// <summary>
        /// Forwards
        /// </summary>
        Forwards,
        /// <summary>
        /// Backwards
        /// </summary>
        Backwards,
        /// <summary>
        /// Left
        /// </summary>
        Left,
        /// <summary>
        /// Right
        /// </summary>
        Right,
    }
    #endregion

    #region Button Class
    /// <summary>
    /// Button info for messages
    /// </summary>
    [DataContract]
    public class Button
    {
        private bool _pressed;
        private ButtonDirection _direction;

        /// <summary>
        /// Constructor
        /// </summary>
        public Button()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The button name</param>
        public Button(string name)
        {
            this.Name = name;
        }

        /// <summary>
        /// The button direction
        /// </summary>
        /// <param name="direction">Direction that the button represents</param>
        public Button(ButtonDirection direction)
        {
            _direction = direction;
        }

        /// <summary>
        /// The Button Name
        /// </summary>
        [DataMember]
        public string Name
        {
            get { return _direction.ToString(); }
            set
            {
                try
                {
                    _direction = (ButtonDirection)Enum.Parse(typeof(ButtonDirection), value);
                }
                catch
                {
                    _direction = ButtonDirection.Stop;
                }
            }
        }

        /// <summary>
        /// The Button Direction
        /// </summary>
        [DataMember, DataMemberConstructor]
        public ButtonDirection Direction
        {
            get { return _direction; }
            set { _direction = value; }
        }

        /// <summary>
        /// Pressed - Set to true if the button is pressed
        /// </summary>
        [DataMember]
        public bool Pressed
        {
            get { return _pressed; }
            set { _pressed = value; }
        }
    }
    #endregion

    #region Button Press Notification
    /// <summary>
    /// Notification message type for Button Press
    /// </summary>
    [DataContract]
    public class ButtonPressRequest
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public ButtonPressRequest()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The button name</param>
        public ButtonPressRequest(string name)
        {
            this.Name = name;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="direction">The button direction</param>
        public ButtonPressRequest(ButtonDirection direction)
        {
            this._direction = direction;
        }

        private ButtonDirection _direction;

        /// <summary>
        /// The Button Name
        /// </summary>
        [DataMember]
        public string Name
        {
            get { return _direction.ToString(); }
            set
            {
                try
                {
                    _direction = (ButtonDirection)Enum.Parse(typeof(ButtonDirection), value);
                }
                catch
                {
                    _direction = ButtonDirection.Stop;
                }
            }
        }

        /// <summary>
        /// The Button Direction
        /// </summary>
        [DataMember, DataMemberConstructor]
        public ButtonDirection Direction
        {
            get { return _direction; }
            set { _direction = value; }
        }

    }
    #endregion

    #region Button Release Notification
    /// <summary>
    /// Notification message type for Button Release
    /// </summary>
    [DataContract]
    public class ButtonReleaseRequest
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public ButtonReleaseRequest()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The button name</param>
        public ButtonReleaseRequest(string name)
        {
            this.Name = name;
        }

        private ButtonDirection _direction;

        /// <summary>
        /// The Button Name
        /// </summary>
        [DataMember]
        public string Name
        {
            get { return _direction.ToString(); }
            set
            {
                try
                {
                    _direction = (ButtonDirection)Enum.Parse(typeof(ButtonDirection), value);
                }
                catch
                {
                    _direction = ButtonDirection.Stop;
                }
            }
        }

        /// <summary>
        /// The Button Direction
        /// </summary>
        [DataMember, DataMemberConstructor]
        public ButtonDirection Direction
        {
            get { return _direction; }
            set { _direction = value; }
        }
    }
    #endregion

    #region Operations Port
    /// <summary>
    /// Main Operations Port for the service
    /// </summary>
    [ServicePort]
    public class DirectionDialogOperations : PortSet<DsspDefaultLookup, DsspDefaultDrop, Get, Replace, ButtonPress, ButtonRelease, Subscribe>
    {
    }
    #endregion

    /// <summary>
    /// Get - Gets the state
    /// </summary>
    [DisplayName("(User) Get")]
    [Description("Gets the current state of the dialog as a list of the buttons on the dialog and their current pressed states.")]
    public class Get : Get<GetRequestType, PortSet<DirectionDialogState, Fault>>
    {
    }

    /// <summary>
    /// Replace - Replaces the state
    /// </summary>
    [DisplayName("(User) DialogStateChange")]
    [Description("Indicates when the dialog state changes.")]
    public class Replace : Replace<DirectionDialogState, PortSet<DefaultReplaceResponseType, Fault>>
    {
    }

    /// <summary>
    /// ButtonPress - Message sent when a button is pressed
    /// </summary>
    [DisplayName("(User) ButtonPress")]
    [Description("Indicates when a button in the dialog is pressed.")]
    public class ButtonPress : Update<ButtonPressRequest, PortSet<DefaultUpdateResponseType, Fault>>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public ButtonPress()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="body">Button press request message</param>
        public ButtonPress(ButtonPressRequest body)
            : base(body)
        {
        }
    }

    /// <summary>
    /// ButtonRleases - Message sent when a button is released
    /// </summary>
    [DisplayName("(User) ButtonRelease")]
    [Description("Indicates when a button in the dialog is released.")]
    public class ButtonRelease : Update<ButtonReleaseRequest, PortSet<DefaultUpdateResponseType, Fault>>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public ButtonRelease()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="body">Button release request message</param>
        public ButtonRelease(ButtonReleaseRequest body)
            : base(body)
        {
        }
    }

    /// <summary>
    /// Subscribe - Request message for subscriptions
    /// </summary>
    public class Subscribe : Subscribe<SubscribeRequestType, PortSet<SubscribeResponseType, Fault>>
    {
    }
}
