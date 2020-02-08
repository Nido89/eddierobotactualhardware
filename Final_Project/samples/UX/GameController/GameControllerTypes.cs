//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: GameControllerTypes.cs $ $Revision: 4 $
//-----------------------------------------------------------------------
using Microsoft.Ccr.Core;
using Microsoft.Dss.Core.Attributes;
using Microsoft.Dss.ServiceModel.Dssp;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using W3C.Soap;

using input = Microsoft.Robotics.Input;
using System.Diagnostics;

namespace Microsoft.Robotics.Services.GameController
{

    /// <summary>
    /// Joystick Contract
    /// </summary>
    public static class Contract
    {
        /// The Unique Contract Identifier for the Joystick service
        public const String Identifier = "http://schemas.microsoft.com/robotics/2006/09/gamecontroller.user.html";
    }

    /// <summary>
    /// The state of the game controller.
    /// </summary>
    [DataContract]
    public class GameControllerState
    {
        private DateTime _timeStamp = DateTime.Now;
        /// <summary>
        /// Indicates the time (in ms) of the last input reading of the Game Controller service.
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
        /// Specifies the current controller used by this instance of the Game Controller service.
        /// </summary>
        [DataMember(IsRequired = true)]
        public Controller Controller
        {
            get { return _controller; }
            set { _controller = value; }
        }

        private Axes _axes = new Axes();
        /// <summary>
        /// Identifies the axes of the controller.
        /// </summary>
        [DataMember(IsRequired = true)]
        [Browsable(false)]
        public Axes Axes
        {
            get { return _axes; }
            set { _axes = value; }
        }

        private Buttons _buttons = new Buttons();
        /// <summary>
        /// Identifies the buttons of the controller.
        /// </summary>
        [DataMember(IsRequired = true)]
        [Browsable(false)]
        public Buttons Buttons
        {
            get { return _buttons; }
            set { _buttons = value; }
        }

        private Sliders _sliders = new Sliders();
        /// <summary>
        /// Identifies the sliders of the controller.
        /// </summary>
        [DataMember(IsRequired = true)]
        [Browsable(false)]
        public Sliders Sliders
        {
            get { return _sliders; }
            set { _sliders = value; }
        }

        private PovHats _povHats = new PovHats();
        /// <summary>
        /// Identifies the directional or Point-Of-View (POV) hats controllers.
        /// </summary>
        [DataMember(IsRequired = true)]
        [Browsable(false)]
        public PovHats PovHats
        {
            get { return _povHats; }
            set { _povHats = value; }
        }

        /// <summary>
        /// Updates the state of the specified controller.
        /// </summary>
        /// <param name="timestamp"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        internal Substate Update(DateTime timestamp, input.JoystickState state)
        {
            _timeStamp = timestamp;

            Substate updated = Substate.None;

            updated |= _axes.Update(timestamp, state);
            updated |= _buttons.Update(timestamp, state.Buttons);
            updated |= _sliders.Update(timestamp, state.Sliders);
            updated |= _povHats.Update(timestamp, state.PovHats);

            return updated;
        }

        /// <summary>
        /// Updates the state of the current controller.
        /// </summary>
        /// <param name="timeStamp"></param>
        /// <returns></returns>
        public Substate Update(DateTime timeStamp)
        {
            input.JoystickState state = _controller.GetState();

            if (state != null)
            {
                return Update(timeStamp, state);
            }
            return Substate.None;
        }
    }

    /// <summary>
    /// Identifies groups of controller state.
    /// </summary>
    [Flags]
    [DataContract]
    public enum Substate
    {
        /// <summary>
        /// No controller state group specified
        /// </summary>
        None = 0x00,
        /// <summary>
        /// Controller state
        /// </summary>
        Controller = 0x01,
        /// <summary>
        /// Thumbstick state
        /// </summary>
        Axes = 0x02,
        /// <summary>
        /// Button state
        /// </summary>
        Buttons = 0x04,
        /// <summary>
        /// Slider state
        /// </summary>
        Sliders = 0x08,
        /// <summary>
        /// The directional or Point-Of-View (POV) hats state
        /// </summary>
        PovHats = 0x10
    }

    /// <summary>
    /// This class represents a game controller.
    /// </summary>
    [DataContract]
    public class Controller : IDisposable
    {
        private DateTime _timeStamp = DateTime.Now;
        /// <summary>
        /// Identifies the time (in ms) of the input reading for this instance.
        /// </summary>
        [DataMember, Browsable(false)]
        public DateTime TimeStamp
        {
            get { return _timeStamp; }
            set { _timeStamp = value; }
        }

        private Guid _instance;
        /// <summary>
        /// Specifies the unique identifier (GUID) for this instance.
        /// </summary>
        [DataMember, Browsable(false)]
        public Guid Instance
        {
            get { return _instance; }
            set { _instance = value; }
        }

        private Guid _product;
        /// <summary>
        /// Specifies the unique product identifier for this instance.
        /// </summary>
        [DataMember, Browsable(false)]
        public Guid Product
        {
            get { return _product; }
            set { _product = value; }
        }

        private string _instanceName;
        /// <summary>
        /// Specifies a user friendly name for this instance.
        /// </summary>
        [DataMember]
        public string InstanceName
        {
            get { return _instanceName; }
            set { _instanceName = value; }
        }

        private string _productName;
        /// <summary>
        /// Specifies a user friendly product name for this instance.
        /// </summary>
        [DataMember]
        public string ProductName
        {
            get { return _productName; }
            set { _productName = value; }
        }

        private bool _current;
        /// <summary>
        /// Specifies if this is the current controller.
        /// </summary>
        [DataMember, Browsable(false)]
        public bool Current
        {
            get { return _current; }
            set { _current = value; }
        }

        private input.Device _device;

        /// <summary>
        /// Returns the state of the controller.
        /// </summary>
        /// <param name="timestamp"></param>
        /// <param name="device"></param>
        /// <returns></returns>
        internal Substate Update(DateTime timestamp, input.Device device)
        {
            _instance = device.Instance;
            _product = device.Product;
            _instanceName = device.InstanceName;
            _productName = device.ProductName;
            _current = true;

            if (_device != null)
            {
                _device.Dispose();
                _device = null;
            }
            _device = device;

            _timeStamp = timestamp;

            return Substate.Controller;
        }

        /// <summary>
        /// Returns the joystick state.
        /// </summary>
        /// <returns></returns>
        internal input.JoystickState GetState()
        {
            if (_device != null)
            {
                return _device.GetState();
            }
            return null;
        }

        /// <summary>
        /// Finds an instance of this controller on the device.
        /// </summary>
        /// <returns></returns>
        public bool FindInstance()
        {
            using(input.DirectInput di = new input.DirectInput())
            {
                using (input.DeviceCollection devices = di.Devices)
                {
                    bool found = false;

                    for (int pass = 0; pass < 5; pass++)
                    {
                        foreach (input.Device device in devices)
                        {
                            switch (pass)
                            {
                                case 0:
                                    if (device.Instance == _instance)
                                    {
                                        found = true;
                                    }
                                    break;
                                case 1:
                                    if (device.Product == _product)
                                    {
                                        found = true;
                                    }
                                    break;
                                case 2:
                                    if (device.InstanceName == _instanceName)
                                    {
                                        found = true;
                                    }
                                    break;
                                case 3:
                                    if (device.ProductName == _productName)
                                    {
                                        found = true;
                                    }
                                    break;
                                default:
                                    found = true;
                                    break;
                            }
                            if (found)
                            {
                                Update(DateTime.Now, device);
                                return true;
                            }
                            device.Dispose();
                        }
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Enumerates all attached controllers.
        /// </summary>
        public static IEnumerable<Controller> Attached
        {
            get
            {
                using(input.DirectInput di = new input.DirectInput())
                {
                    foreach (input.Device device in di.Devices)
                    {
                        yield return FromDevice(device);
                        device.Dispose();
                    }
                }
            }
        }

        private static Controller FromDevice(input.Device device)
        {
            Controller controller = new Controller();

            controller.Instance = device.Instance;
            controller.Product = device.Product;
            controller.InstanceName = device.InstanceName;
            controller.ProductName = device.ProductName;

            return controller;
        }

        /// <summary>
        /// Disposes resources associated with the controller.
        /// </summary>
        public void Dispose()
        {
            if (_device != null)
            {
                _device.Dispose();
                _device = null;
            }
        }
    }

    /// <summary>
    /// Identifies the controller axes.
    /// </summary>
    [DataContract]
    public class Axes
    {
        private DateTime _timeStamp = DateTime.Now;
        /// <summary>
        /// Identifies the time (in ms) of the reading.
        /// </summary>
        [DataMember]
        public DateTime TimeStamp
        {
            get { return _timeStamp; }
            set { _timeStamp = value; }
        }

        private int _x;
        /// <summary>
        /// Identifies the horizontal (X) axis value.
        /// </summary>
        [DataMember]
        public int X
        {
            get { return _x; }
            set { _x = value; }
        }

        private int _y;
        /// <summary>
        /// Identifies the vertical (Y) axis value.
        /// </summary>
        [DataMember]
        public int Y
        {
            get { return _y; }
            set { _y = value; }
        }

        private int _z;
        /// <summary>
        /// Identifies the Z-axis value.
        /// </summary>
        [DataMember]
        public int Z
        {
            get { return _z; }
            set { _z = value; }
        }

        private int _rx;
        /// <summary>
        /// Identifies horizontal (X) axis rotation value.
        /// </summary>
        [DataMember]
        public int Rx
        {
            get { return _rx; }
            set { _rx = value; }
        }

        private int _ry;
        /// <summary>
        /// Identifies vertical (Y) axis rotation value.
        /// </summary>
        [DataMember]
        public int Ry
        {
            get { return _ry; }
            set { _ry = value; }
        }

        private int _rz;
        /// <summary>
        /// Identifies z-axis rotation value.
        /// </summary>
        [DataMember]
        public int Rz
        {
            get { return _rz; }
            set { _rz = value; }
        }

        /// <summary>
        /// Returns the state of the axes.
        /// </summary>
        /// <param name="timestamp"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        internal Substate Update(DateTime timestamp, input.JoystickState state)
        {
            Substate updated = Substate.None;

            if (_x != state.X ||
                _y != state.Y ||
                _z != state.Z ||
                _rx != state.Rx ||
                _ry != state.Ry ||
                _rz != state.Rz)
            {
                _x = state.X;
                _y = state.Y;
                _z = state.Z;
                _rx = state.Rx;
                _ry = state.Ry;
                _rz = state.Rz;

                _timeStamp = timestamp;

                updated = Substate.Axes;
            }
            return updated;
        }
    }

    /// <summary>
    /// Identifies the controller buttons.
    /// </summary>
    [DataContract]
    public class Buttons
    {
        private DateTime _timeStamp = DateTime.Now;
        /// <summary>
        /// Identifies the time (in ms) of the reading.
        /// </summary>
        [DataMember]
        public DateTime TimeStamp
        {
            get { return _timeStamp; }
            set { _timeStamp = value; }
        }

        private List<bool> _pressed = new List<bool>();
        /// <summary>
        /// Identifies the pressed state of the set of buttons.
        /// </summary>
        [DataMember(IsRequired = true)]
        public List<bool> Pressed
        {
            get { return _pressed; }
            set { _pressed = value; }
        }

        /// <summary>
        /// Returns the state of the buttons.
        /// </summary>
        /// <param name="timestamp"></param>
        /// <param name="buttons"></param>
        /// <returns></returns>
        public Substate Update(DateTime timestamp, bool[] buttons)
        {
            Substate updated = Substate.None;

            if (_pressed.Count != buttons.Length)
            {
                _pressed = new List<bool>(buttons);
                updated = Substate.Buttons;
            }
            else
            {
                for (int i = 0; i < buttons.Length; i++)
                {
                    if (_pressed[i] != buttons[i])
                    {
                        _pressed[i] = buttons[i];
                        updated = Substate.Buttons;
                    }
                }
            }

            if (updated != Substate.None)
            {
                _timeStamp = timestamp;
            }
            return updated;
        }
    }

    /// <summary>
    /// Identifies the controller sliders.
    /// </summary>
    [DataContract]
    public class Sliders
    {
        private DateTime _timeStamp = DateTime.Now;
        /// <summary>
        /// Identifies the time (in ms) of the current reading.
        /// </summary>
        [DataMember]
        public DateTime TimeStamp
        {
            get { return _timeStamp; }
            set { _timeStamp = value; }
        }

        private List<int> _position = new List<int>();
        /// <summary>
        /// Identifies the set of position values of the sliders.
        /// </summary>
        [DataMember(IsRequired = true)]
        public List<int> Position
        {
            get { return _position; }
            set { _position = value; }
        }

        /// <summary>
        /// Returns the state of the sliders.
        /// </summary>
        /// <param name="timestamp"></param>
        /// <param name="sliders"></param>
        /// <returns></returns>
        public Substate Update(DateTime timestamp, int[] sliders)
        {
            Substate updated = Substate.None;

            if (_position.Count != sliders.Length)
            {
                _position = new List<int>(sliders);
                updated = Substate.Sliders;
            }
            else
            {
                for (int i = 0; i < sliders.Length; i++)
                {
                    if (_position[i] != sliders[i])
                    {
                        _position[i] = sliders[i];
                        updated = Substate.Sliders;
                    }
                }
            }

            if (updated != Substate.None)
            {
                _timeStamp = timestamp;
            }
            return updated;
        }
    }

    /// <summary>
    /// Identifies the current value of the directional or Point-Of-View (POV) hat controls.
    /// </summary>
    [DataContract]
    public class PovHats
    {
        private DateTime _timeStamp = DateTime.Now;
        /// <summary>
        /// Identifies the time (in ms) of the reading.
        /// </summary>
        [DataMember]
        public DateTime TimeStamp
        {
            get { return _timeStamp; }
            set { _timeStamp = value; }
        }

        private List<int> _direction = new List<int>();
        /// <summary>
        /// Identifies the set of directional values of the control.
        /// </summary>
        [DataMember(IsRequired = true)]
        public List<int> Direction
        {
            get { return _direction; }
            set { _direction = value; }
        }

        /// <summary>
        /// Returns the state of the PovHats
        /// </summary>
        /// <param name="timestamp"></param>
        /// <param name="povHats"></param>
        /// <returns></returns>
        public Substate Update(DateTime timestamp, int[] povHats)
        {
            Substate updated = Substate.None;

            if (_direction.Count != povHats.Length)
            {
                _direction = new List<int>(povHats);
                updated = Substate.PovHats;
            }
            else
            {
                for (int i = 0; i < povHats.Length; i++)
                {
                    if (_direction[i] != povHats[i])
                    {
                        _direction[i] = povHats[i];
                        updated = Substate.PovHats;
                    }
                }
            }

            if (updated != Substate.None)
            {
                _timeStamp = timestamp;
            }
            return updated;
        }
    }

    /// <summary>
    /// Requests the service to update the controller state.
    /// </summary>
    [DataContract]
    public class PollRequest
    {
    }

    /// <summary>
    /// Requests the service to return information about all controllers
    /// </summary>
    [DataContract]
    public class GetControllersRequest
    {
    }

    /// <summary>
    /// Contains information about the controllers.
    /// </summary>
    [DataContract]
    public class GetControllersResponse
    {
        private List<Controller> _controllers = new List<Controller>();
        /// <summary>
        /// Identifies the set of controllers.
        /// </summary>
        [DataMember(IsRequired = true)]
        public List<Controller> Controllers
        {
            get { return _controllers; }
            set { _controllers = value; }
        }
    }

    /// <summary>
    /// The operations supported by this service.
    /// </summary>
    [ServicePort]
    public class GameControllerOperations : PortSet<DsspDefaultLookup, DsspDefaultDrop, Get, Replace, Poll, ChangeController, UpdateAxes, UpdateButtons, UpdatePovHats, UpdateSliders, Subscribe, GetControllers>
    {
    }

    /// <summary>
    /// Gets the current state of the controller.
    /// </summary>
    public class Get : Get<GetRequestType, PortSet<GameControllerState, Fault>>
    {
    }

    /// <summary>
    /// Changes (or indicates a change to) the entire state of the controller.\nThis is sent initially to every connection.
    /// </summary>
    public class Replace : Replace<GameControllerState, PortSet<DefaultReplaceResponseType, Fault>>
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public Replace()
        {
        }

        /// <summary>
        /// Data constructor.
        /// </summary>
        /// <param name="body"></param>
        public Replace(GameControllerState body)
            : base(body)
        {
        }
    }

    /// <summary>
    /// Polls the controller and updates the state since the controller was last polled.\nThe controller is automatically polled 20 times a second.
    /// </summary>
    [DisplayName("(User) Poll")]
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
    /// Changes which game controller is in use.
    /// </summary>
    public class ChangeController : Update<Controller, PortSet<DefaultUpdateResponseType, Fault>>
    {
    }

    /// <summary>
    /// Indicates when one of the axes is moved.
    /// </summary>
    public class UpdateAxes : Update<Axes, PortSet<DefaultUpdateResponseType, Fault>>
    {
    }

    /// <summary>
    /// Indicates when one or more buttons is pressed or released.
    /// </summary>
    public class UpdateButtons : Update<Buttons, PortSet<DefaultUpdateResponseType, Fault>>
    {
    }

    /// <summary>
    /// Indicates when the position of a slider is changed.
    /// </summary>
    public class UpdateSliders : Update<Sliders, PortSet<DefaultUpdateResponseType, Fault>>
    {
    }

    /// <summary>
    /// Indicates when the position of a Point-Of-View (POV) hat switch is changed.
    /// </summary>
    public class UpdatePovHats : Update<PovHats, PortSet<DefaultUpdateResponseType, Fault>>
    {
    }

    /// <summary>
    /// Subscribe message that adds another service to the subscriber list so that notifications
    /// can be sent to that service.
    /// </summary>
    public class Subscribe : Subscribe<SubscribeRequestType, PortSet<SubscribeResponseType, Fault>>
    {
    }

    /// <summary>
    /// Returns a list of attached controllers.
    /// </summary>
    public class GetControllers : Query<GetControllersRequest, PortSet<GetControllersResponse, Fault>>
    {
    }
}
