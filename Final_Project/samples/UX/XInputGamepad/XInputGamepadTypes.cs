//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: XInputGamepadTypes.cs $ $Revision: 3 $
//-----------------------------------------------------------------------
using Microsoft.Ccr.Core;
using Microsoft.Dss.Core.Attributes;
using Microsoft.Dss.ServiceModel.Dssp;
using Microsoft.Dss.Core.DsspHttp;

using System;
using System.Net;
using System.Collections.Generic;
using System.ComponentModel;
using W3C.Soap;

using xinput = Microsoft.Xna.Framework.Input;
using xna = Microsoft.Xna.Framework;


namespace Microsoft.Robotics.Services.Sample.XInputGamepad
{
    /// <summary>
    /// Xinput Contract
    /// </summary>
    public static class Contract
    {
        /// The Unique Contract Identifier for the Xinput service
        public const String Identifier = "http://schemas.microsoft.com/robotics/2006/09/xinputgamepad.user.html";
    }


    /// <summary>
    /// Specifies the state of the XInput controller.
    /// </summary>
    [DataContract]
    public class XInputGamepadState
    {
        private DateTime _timeStamp = DateTime.Now;
        /// <summary>
        /// Indicates time of the controller's state update.
        /// </summary>
        [DataMember]
        [Browsable(false)]
        public DateTime TimeStamp
        {
            get { return _timeStamp; }
            set { _timeStamp = value; }
        }

        private Controller _controller = new Controller();
        /// <summary>
        /// Indicates controller state.
        /// </summary>
        [DataMember(IsRequired = true)]
        public Controller Controller
        {
            get { return _controller; }
            set { _controller = value; }
        }

        private DPad _dPad = new DPad();
        /// <summary>
        /// Indicates the state of the directional pad.
        /// </summary>
        [DataMember(IsRequired = true)]
        [Browsable(false)]
        public DPad DPad
        {
            get { return _dPad; }
            set { _dPad = value; }
        }

        private Buttons _buttons = new Buttons();
        /// <summary>
        /// Identifies the state of the buttons.
        /// </summary>
        [Browsable(false)]
        [DataMember(IsRequired = true)]
        public Buttons Buttons
        {
            get { return _buttons; }
            set { _buttons = value; }
        }

        private Triggers _triggers = new Triggers();
        /// <summary>
        /// Identifies the state of the triggers.
        /// </summary>
        [DataMember(IsRequired = true)]
        [Browsable(false)]
        public Triggers Triggers
        {
            get { return _triggers; }
            set { _triggers = value; }
        }

        private Thumbsticks _thumbsticks = new Thumbsticks();
        /// <summary>
        /// Identifies the state of the thumbsticks.
        /// </summary>
        [Browsable(false)]
        [DataMember(IsRequired = true)]
        public Thumbsticks Thumbsticks
        {
            get { return _thumbsticks; }
            set { _thumbsticks = value; }
        }

        private Vibration _vibration = new Vibration();
        /// <summary>
        /// Identifies the state of the vibration motors.
        /// </summary>
        [DataMember(IsRequired = true)]
        [Browsable(false)]
        public Vibration Vibration
        {
            get { return _vibration; }
            set { _vibration = value; }
        }

        private xinput.GamePadState _previous = new xinput.GamePadState();

        internal xinput.GamePadState GetState()
        {
            return xinput.GamePad.GetState(
                (xna.PlayerIndex)_controller.PlayerIndex
            );
        }

        internal bool Update(DateTime timeStamp, xinput.GamePadState state)
        {
            if (_previous == state)
            {
                return false;
            }

            _timeStamp = timeStamp;

            _previous = state;
            return true;
        }

        internal void SetVibration(Vibration vibration)
        {
            vibration.Left = Math.Max(0, Math.Min(vibration.Left, 1.0f));
            vibration.Right = Math.Max(0, Math.Min(vibration.Right, 1.0f));
            vibration.TimeStamp = DateTime.Now;

            xinput.GamePad.SetVibration(
                (xna.PlayerIndex)_controller.PlayerIndex,
                vibration.Left,
                vibration.Right
            );

            _vibration = vibration;
        }
    }

    /// <summary>
    /// Identifies the player index.
    /// </summary>
    [DataContract]
    [Browsable(false)]
    public enum PlayerIndex
    {
        /// <summary>
        /// Player index (1)
        /// </summary>
        One = 0,
        /// <summary>
        /// Player index (2)
        /// </summary>
        Two = 1,
        /// <summary>
        /// Player index (3)
        /// </summary>
        Three = 2,
        /// <summary>
        /// Player index (4)
        /// </summary>
        Four = 3,
        /// <summary>
        /// Player index (not specified)
        /// </summary>
        Any = 255
    }

    /// <summary>
    /// Identifies the controller type.
    /// </summary>
    [DataContract]
    [Browsable(false)]
    public enum ControllerType
    {
        /// <summary>
        /// Controller type is not specified
        /// </summary>
        NotSelected = 0,
        /// <summary>
        /// Controller is a gamepad
        /// </summary>
        GamePad = 1,
        /// <summary>
        /// Controller is a steering wheel
        /// </summary>
        Wheel = 2,
        /// <summary>
        /// Controller is a joystick
        /// </summary>
        ArcadeStick = 3,
        /// <summary>
        /// Controller is a flight control stick
        /// </summary>
        FlightStick = 4,
        /// <summary>
        /// Controller is a dance pad
        /// </summary>
        DancePad = 5,
    }

    /// <summary>
    /// This class represents a controller.
    /// </summary>
    [DataContract]
    public class Controller
    {
        private DateTime _timeStamp = DateTime.Now;
        /// <summary>
        /// Indicates the time of the controller change.
        /// </summary>
        [DataMember]
        [Browsable(false)]
        public DateTime TimeStamp
        {
            get { return _timeStamp; }
            set { _timeStamp = value; }
        }

        private PlayerIndex _playerIndex = PlayerIndex.One;
        /// <summary>
        /// Identifies the player index.
        /// </summary>
        [DataMember]
        public PlayerIndex PlayerIndex
        {
            get { return _playerIndex; }
            set { _playerIndex = value; }
        }

        private bool _isConnected;
        /// <summary>
        /// Indicates whether a controller is connected.
        /// </summary>
        [DataMember]
        [Browsable(false)]
        public bool IsConnected
        {
            get { return _isConnected; }
            set { _isConnected = value; }
        }

        internal bool Update(xinput.GamePadState curr)
        {
            if (_isConnected == curr.IsConnected)
            {
                return false;
            }

            _isConnected = curr.IsConnected;

            return true;
        }

        /// <summary>
        /// True if the controller is valid.
        /// </summary>
        public bool IsValid
        {
            get
            {
                return _playerIndex >= PlayerIndex.One &&
               _playerIndex <= PlayerIndex.Four;
            }
        }
    }

    /// <summary>
    /// Represents the capabilities of a controller.
    /// </summary>
    [DataContract]
    public class ControllerCaps : Controller
    {
        private ControllerType _ControllerType;
        /// <summary>
        /// Type type of the controller.
        /// </summary>
        [DataMember]
        public ControllerType ControllerType
        {
            get { return _ControllerType; }
            set { _ControllerType = value; }
        }
    }

    /// <summary>
    /// Identifies the state of the directional pad.
    /// </summary>
    [DataContract]
    public class DPad
    {
        private DateTime _timeStamp = DateTime.Now;
        /// <summary>
        /// Identifies the time of the directional pad input.
        /// </summary>
        [DataMember]
        [Browsable(false)]
        public DateTime TimeStamp
        {
            get { return _timeStamp; }
            set { _timeStamp = value; }
        }

        private bool _left;
        /// <summary>
        /// Indicates if the directional pad is pressed left.
        /// </summary>
        [DataMember]
        [Browsable(false)]
        public bool Left
        {
            get { return _left; }
            set { _left = value; }
        }

        private bool _down;
        /// <summary>
        /// Indicates if the directional pad is pressed down.
        /// </summary>
        [DataMember]
        [Browsable(false)]
        public bool Down
        {
            get { return _down; }
            set { _down = value; }
        }

        private bool _right;
        /// <summary>
        /// Indicates if the directional pad is pressed right.
        /// </summary>
        [DataMember]
        [Browsable(false)]
        public bool Right
        {
            get { return _right; }
            set { _right = value; }
        }

        private bool _up;
        /// <summary>
        /// Indicates if the directional pad is pressed up.
        /// </summary>
        [DataMember]
        [Browsable(false)]
        public bool Up
        {
            get { return _up; }
            set { _up = value; }
        }

        xinput.GamePadDPad _previous = new xinput.GamePadDPad();

        internal bool Update(DateTime timeStamp, xinput.GamePadDPad curr)
        {
            if (_previous == curr)
            {
                return false;
            }

            _timeStamp = timeStamp;

            _left = (curr.Left == xinput.ButtonState.Pressed);
            _down = (curr.Down == xinput.ButtonState.Pressed);
            _right = (curr.Right == xinput.ButtonState.Pressed);
            _up = (curr.Up == xinput.ButtonState.Pressed);

            _previous = curr;

            return true;
        }
    }

    /// <summary>
    /// Identifies the state of the controller buttons.
    /// </summary>
    [DataContract]
    [Browsable(false)]
    public class Buttons
    {
        private DateTime _timeStamp = DateTime.Now;
        /// <summary>
        /// Identifies the time of the button input.
        /// </summary>
        [DataMember]
        [Browsable(false)]
        public DateTime TimeStamp
        {
            get { return _timeStamp; }
            set { _timeStamp = value; }
        }

        bool _a;
        /// <summary>
        /// Identifies whether the A button was pressed.
        /// </summary>
        [DataMember]
        [Browsable(false)]
        public bool A
        {
            get { return _a; }
            set { _a = value; }
        }

        bool _b;
        /// <summary>
        /// Identifies whether the B button was pressed.
        /// </summary>
        [DataMember]
        [Browsable(false)]
        public bool B
        {
            get { return _b; }
            set { _b = value; }
        }

        bool _x;
        /// <summary>
        /// Identifies whether the X button was pressed.
        /// </summary>
        [DataMember]
        [Browsable(false)]
        public bool X
        {
            get { return _x; }
            set { _x = value; }
        }

        bool _y;
        /// <summary>
        /// Identifies whether the Y button was pressed.
        /// </summary>
        [DataMember]
        [Browsable(false)]
        public bool Y
        {
            get { return _y; }
            set { _y = value; }
        }

        bool _back;
        /// <summary>
        /// Identifies whether the Back button was pressed.
        /// </summary>
        [DataMember]
        [Browsable(false)]
        public bool Back
        {
            get { return _back; }
            set { _back = value; }
        }

        bool _start;
        /// <summary>
        /// Identifies whether the Start button was pressed.
        /// </summary>
        [DataMember]
        [Browsable(false)]
        public bool Start
        {
            get { return _start; }
            set { _start = value; }
        }

        bool _leftStick;
        /// <summary>
        /// Identifies whether the left thumbstick was pressed.
        /// </summary>
        [DataMember]
        [Browsable(false)]
        public bool LeftStick
        {
            get { return _leftStick; }
            set { _leftStick = value; }
        }

        bool _rightStick;
        /// <summary>
        /// Identifies whether the right thumbstick was pressed.
        /// </summary>
        [DataMember]
        [Browsable(false)]
        public bool RightStick
        {
            get { return _rightStick; }
            set { _rightStick = value; }
        }

        bool _leftShoulder;
        /// <summary>
        /// Identifies whether the left shoulder (bumper) was pressed.
        /// </summary>
        [DataMember]
        [Browsable(false)]
        public bool LeftShoulder
        {
            get { return _leftShoulder; }
            set { _leftShoulder = value; }
        }

        bool _rightShoulder;
        /// <summary>
        /// Identifies whether the right shoulder (bumper) was pressed.
        /// </summary>
        [DataMember]
        [Browsable(false)]
        public bool RightShoulder
        {
            get { return _rightShoulder; }
            set { _rightShoulder = value; }
        }

        xinput.GamePadButtons _previous = new xinput.GamePadButtons();

        internal bool Update(DateTime timeStamp, xinput.GamePadButtons curr)
        {
            if (curr == _previous)
            {
                return false;
            }

            _timeStamp = timeStamp;

            _a = (curr.A == xinput.ButtonState.Pressed);
            _b = (curr.B == xinput.ButtonState.Pressed);
            _x = (curr.X == xinput.ButtonState.Pressed);
            _y = (curr.Y == xinput.ButtonState.Pressed);

            _leftStick = (curr.LeftStick == xinput.ButtonState.Pressed);
            _rightStick = (curr.RightStick == xinput.ButtonState.Pressed);
            _leftShoulder = (curr.LeftShoulder == xinput.ButtonState.Pressed);
            _rightShoulder = (curr.RightShoulder == xinput.ButtonState.Pressed);

            _back = (curr.Back == xinput.ButtonState.Pressed);
            _start = (curr.Start == xinput.ButtonState.Pressed);

            _previous = curr;

            return true;
        }
    }

    /// <summary>
    /// Identifies the state of the controller's triggers.
    /// </summary>
    [DataContract]
    [Browsable(false)]
    public class Triggers
    {
        private DateTime _timeStamp = DateTime.Now;
        /// <summary>
        /// Identifies the time of the trigger input.
        /// </summary>
        [DataMember]
        [Browsable(false)]
        public DateTime TimeStamp
        {
            get { return _timeStamp; }
            set { _timeStamp = value; }
        }

        float _left;
        /// <summary>
        /// Identifies the left trigger value.
        /// </summary>
        [DataMember]
        [Browsable(false)]
        public float Left
        {
            get { return _left; }
            set { _left = value; }
        }

        float _right;
        /// <summary>
        /// Identifies the right trigger value.
        /// </summary>
        [DataMember]
        [Browsable(false)]
        public float Right
        {
            get { return _right; }
            set { _right = value; }
        }

        xinput.GamePadTriggers _previous = new xinput.GamePadTriggers();

        internal bool Update(DateTime timeStamp, xinput.GamePadTriggers curr)
        {
            if (_left == curr.Left &&
                _right == curr.Right)
            {
                return false;
            }

            _timeStamp = timeStamp;

            _left = curr.Left;
            _right = curr.Right;

            _previous = curr;
            return true;
        }
    }

    /// <summary>
    /// Identifies the thumbsticks state.
    /// </summary>
    [DataContract]
    [Browsable(false)]
    public class Thumbsticks
    {
        private DateTime _timeStamp = DateTime.Now;
        /// <summary>
        /// Identifies the time of the thumbstick input.
        /// </summary>
        [DataMember]
        [Browsable(false)]
        public DateTime TimeStamp
        {
            get { return _timeStamp; }
            set { _timeStamp = value; }
        }

        float _leftX;
        /// <summary>
        /// Identifies the X value of left thumbstick.
        /// </summary>
        [DataMember]
        [Browsable(false)]
        public float LeftX
        {
            get { return _leftX; }
            set { _leftX = value; }
        }

        float _leftY;
        /// <summary>
        /// Identifies the Y value of left thumbstick.
        /// </summary>
        [DataMember]
        [Browsable(false)]
        public float LeftY
        {
            get { return _leftY; }
            set { _leftY = value; }
        }

        float _rightX;
        /// <summary>
        /// Identifies the X value of right thumbstick.
        /// </summary>
        [DataMember]
        [Browsable(false)]
       public float RightX
        {
            get { return _rightX; }
            set { _rightX = value; }
        }

        float _rightY;
        /// <summary>
        /// Identifies the Y value of right thumbstick.
        /// </summary>
        [DataMember]
        [Browsable(false)]
        public float RightY
        {
            get { return _rightY; }
            set { _rightY = value; }
        }

        xinput.GamePadThumbSticks _previous = new xinput.GamePadThumbSticks();

        internal bool Update(DateTime timeStamp, xinput.GamePadThumbSticks curr)
        {
            if (_leftX == curr.Left.X &&
                _leftY == curr.Left.Y &&
                _rightX == curr.Right.X &&
                _rightY == curr.Right.Y)
            {
                return false;
            }

            _timeStamp = timeStamp;

            _leftX = curr.Left.X;
            _leftY = curr.Left.Y;
            _rightX = curr.Right.X;
            _rightY = curr.Right.Y;

            _previous = curr;

            return true;
        }
    }

    /// <summary>
    /// A request to cause the service to update the state of the controllers.
    /// </summary>
    [DataContract]
    public class PollRequest
    {
    }

    /// <summary>
    /// Identifies the vibration motors' state.
    /// </summary>
    [DataContract]
    [Browsable(false)]
    public class Vibration
    {
        private DateTime _timeStamp = DateTime.Now;
        /// <summary>
        /// Identifies the time of the vibration motors change.
        /// </summary>
        [DataMember]
        [Browsable(false)]
        public DateTime TimeStamp
        {
            get { return _timeStamp; }
            set { _timeStamp = value; }
        }

        private float _left;
        /// <summary>
        /// Identifies the value of the left vibration motor.
        /// </summary>
        [DataMember]
        [Browsable(false)]
        public float Left
        {
            get { return _left; }
            set { _left = value; }
        }

        private float _right;
        /// <summary>
        /// Identifies the value of the right vibration motor.
        /// </summary>
        [DataMember]
        [Browsable(false)]
        public float Right
        {
            get { return _right; }
            set { _right = value; }
        }
    }

    /// <summary>
    /// A request to return information about the current controllers.
    /// </summary>
    [DataContract]
    public class QueryControllersRequest
    {
    }

    /// <summary>
    /// The response that contains information about the current controllers.
    /// </summary>
    [DataContract]
    public class QueryControllersResponse
    {
        private List<ControllerCaps> _controllers = new List<ControllerCaps>();
        /// <summary>
        /// A list of capabilities for each controller.
        /// </summary>
        [DataMember(IsRequired = true)]
        public List<ControllerCaps> Controllers
        {
            get { return _controllers; }
            set { _controllers = value; }
        }
    }

    /// <summary>
    /// The operations supported by this service.
    /// </summary>
    [ServicePort]
    public class XInputGamepadOperations : PortSet<DsspDefaultLookup, DsspDefaultDrop, Subscribe, Get, HttpGet, Replace, Poll, DPadChanged, ButtonsChanged, TriggersChanged, ThumbsticksChanged, ControllerChanged, SetVibration, QueryControllers>
    {
    }

    /// <summary>
    /// A subscribe operation requests the service to send notifications to another service.
    /// </summary>
    public class Subscribe : Subscribe<SubscribeRequestType, PortSet<SubscribeResponseType, Fault>>
    {
    }

    /// <summary>
    /// Gets the current state of the controller.
    /// </summary>
    public class Get : Get<GetRequestType, PortSet<XInputGamepadState, Fault>>
    {
    }

    /// <summary>
    /// Sets the current state of the game controller.
    /// This raises an event when a subscription is initially established.
    /// </summary>
    public class Replace : Replace<XInputGamepadState, PortSet<DefaultReplaceResponseType, Fault>>
    {
    }

    /// <summary>
    /// Polls the controller to update the current state.\n If the state has changed, then appropriate notifications are sent.
    /// </summary>
    public class Poll : Submit<PollRequest, PortSet<DefaultSubmitResponseType, Fault>>
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public Poll()
        {
        }

        /// <summary>
        /// Data constructor.
        /// </summary>
        /// <param name="body"></param>
        public Poll(PollRequest body)
            : base(body)
        {

        }
    }
    /// <summary>
    /// Send this message to change which controller this service instance is using.
    /// This event is raised when the controller is changed or when the controller is connected or disconnected
    /// </summary>
    public class ControllerChanged : Update<Controller, PortSet<DefaultUpdateResponseType, Fault>>
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public ControllerChanged()
        {
        }

        /// <summary>
        /// Data constructor.
        /// </summary>
        /// <param name="body"></param>
        public ControllerChanged(Controller body)
            : base(body)
        {
        }
    }

    /// <summary>
    /// Indicates when the directional pad position changes.
    /// </summary>
    public class DPadChanged : Update<DPad, PortSet<DefaultUpdateResponseType, Fault>>
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public DPadChanged()
        {
        }

        /// <summary>
        /// Data constructor.
        /// </summary>
        /// <param name="body"></param>
        public DPadChanged(DPad body)
            : base(body)
        {
        }
    }

    /// <summary>
    /// Indicates when one or more buttons are pressed or released on the controller.
    /// </summary>
    public class ButtonsChanged : Update<Buttons, PortSet<DefaultUpdateResponseType, Fault>>
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public ButtonsChanged()
        {
        }

        /// <summary>
        /// Data constructor.
        /// </summary>
        /// <param name="body"></param>
        public ButtonsChanged(Buttons body)
            : base(body)
        {
        }
    }

    /// <summary>
    /// Indicates when a trigger's value when it is pressed.
    /// </summary>
    public class TriggersChanged : Update<Triggers, PortSet<DefaultUpdateResponseType, Fault>>
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public TriggersChanged()
        {
        }

        /// <summary>
        /// Data constructor.
        /// </summary>
        /// <param name="body"></param>
        public TriggersChanged(Triggers body)
            : base(body)
        {
        }
    }

    /// <summary>
    /// Indicates when a thumbstick position changes.
    /// </summary>
    [DisplayName("(User) ThumbsticksChange")]
    public class ThumbsticksChanged : Update<Thumbsticks, PortSet<DefaultUpdateResponseType, Fault>>
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public ThumbsticksChanged()
        {
        }

        /// <summary>
        /// Data constructor.
        /// </summary>
        /// <param name="body"></param>
        public ThumbsticksChanged(Thumbsticks body)
            : base(body)
        {
        }
    }
    /// <summary>
    /// Sets the amount of vibration in the left and right rumble motors.
    /// The left motor is the low-frequency rumble motor.
    /// The right motor is the high-frequency rumble motor.
    /// This notification is raised when the vibration settings are changed.
    /// </summary>
    [DisplayName("(User) SetVibrationMotor")]
    public class SetVibration : Update<Vibration, PortSet<DefaultUpdateResponseType, Fault>>
    {
    }

    /// <summary>
    /// Gets the list of attached controllers.
    /// </summary>
    [DisplayName("(User) QueryControllers")]
    public class QueryControllers : Query<QueryControllersRequest, PortSet<QueryControllersResponse, Fault>>
    {
    }
}
