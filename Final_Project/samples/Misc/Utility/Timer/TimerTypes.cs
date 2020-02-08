//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: TimerTypes.cs $ $Revision: 21 $
//-----------------------------------------------------------------------
using Microsoft.Ccr.Core;
using Microsoft.Dss.Core.Attributes;
using Microsoft.Dss.ServiceModel.Dssp;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using W3C.Soap;

using timer = Microsoft.Robotics.Services.Sample.Timer;

namespace Microsoft.Robotics.Services.Sample.Timer
{
    /// <summary>
    /// Timer Service contract
    /// </summary>
    public static class Contract
    {
        /// <summary>
        /// The Unique Contract Identifier for the Timer service
        /// </summary>
        public const string Identifier = "http://schemas.microsoft.com/robotics/2006/08/timer.user.html";
    }

    /// <summary>
    /// Timer State
    /// </summary>
    [DataContract()]
    public class TimerState
    {
        private int _timeout;
        /// <summary>
        /// Timeout - Interval in milliseconds
        /// </summary>
        [DataMember]
        [Description("Indicates the timeout interval (in ms).")]
        public int Timeout
        {
            get { return _timeout; }
            set { _timeout = value; }
        }

        private DateTime _expires = DateTime.MaxValue;
        /// <summary>
        /// DateTime - Time when interval expires
        /// </summary>
        [DataMember]
        [Description("Indicates when the timeout interval expires.")]
        public DateTime Expires
        {
            get { return _expires; }
            set { _expires = value; }
        }

        private int _ticks;

        /// <summary>
        /// Ticks - Tick counter
        /// </summary>
        public int Ticks
        {
            get { return _ticks; }
            set { _ticks = value; }
        }
    }

    /// <summary>
    /// SetTimerRequest
    /// </summary>
    [DataContract]
    public class SetTimerRequest
    {
        int _interval;
        /// <summary>
        /// Interval - Time interval in milliseconds
        /// </summary>
        [DataMember]
        [Description("Specifies the timeout interval setting (in ms).")]
        public int Interval
        {
            get { return _interval; }
            set { _interval = value; }
        }
    }

    /// <summary>
    /// FireTimerRequest
    /// </summary>
    [DataContract]
    public class FireTimerRequest : SetTimerRequest
    {
        private DateTime _fired;
        /// <summary>
        /// Fired - Timestamp when the timer fired
        /// </summary>
        [DataMember]
        [Description("Indicates the time at which the timer fired.")]
        public DateTime Fired
        {
            get { return _fired; }
            set { _fired = value; }
        }
    }

    /// <summary>
    /// TickRequest
    /// </summary>
    [DataContract]
    public class TickRequest
    {
    }

    /// <summary>
    /// TimeRequest - Ask for Date/Time
    /// </summary>
    [DataContract]
    public class TimeRequest
    {
    }

    /// <summary>
    /// TimeResponse
    /// </summary>
    [DataContract]
    public class TimeResponse
    {
        /// <summary>
        /// Default Constructor
        /// </summary>
        public TimeResponse()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="time"></param>
        public TimeResponse(DateTime time)
        {
            _time = time;
        }

        private DateTime _time;
        /// <summary>
        /// Time - Current Date/Time
        /// </summary>
        [DataMember]
        [Description("Indicates the current time.")]
        public DateTime Time
        {
            get { return _time; }
            set { _time = value; }
        }
    }

    /// <summary>
    /// Timer Service Operations PortSet
    /// </summary>
    [ServicePort]
    public class TimerOperations : PortSet
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public TimerOperations()
            : base(
                typeof(DsspDefaultLookup),
                typeof(DsspDefaultDrop),
                typeof(Get),
                typeof(FireTimer),
                typeof(SetTimer),
                typeof(Tick),
                typeof(Wait),
                typeof(Subscribe),
                typeof(Time)
            )
        {
        }

        /// <summary>
        /// Implicit Operator to extract port
        /// </summary>
        /// <param name="portSet"></param>
        /// <returns></returns>
        public static implicit operator Port<FireTimer>(TimerOperations portSet)
        {
            if (portSet == null)
            {
                return null;
            }
            return (Port<FireTimer>)portSet[typeof(FireTimer)];
        }

        /// <summary>
        /// Explicit Post for FireTimer messages
        /// </summary>
        /// <param name="msg"></param>
        public void Post(FireTimer msg)
        {
            base.PostUnknownType(msg);
        }

        /// <summary>
        /// Implicit Operator to extract port
        /// </summary>
        /// <param name="portSet"></param>
        /// <returns></returns>
        public static implicit operator Port<Tick>(TimerOperations portSet)
        {
            if (portSet == null)
            {
                return null;
            }
            return (Port<Tick>)portSet[typeof(Tick)];
        }

        /// <summary>
        /// Explicit Post for Tick messages
        /// </summary>
        /// <param name="msg"></param>
        public void Post(Tick msg)
        {
            base.PostUnknownType(msg);
        }
    }

    /// <summary>
    /// Get - Gets the state
    /// </summary>
    [Description("Get the timer's current state.\nThis includes the currently set timeout value (if any) and the expected time at which it will fire.")]
    public class Get : Get<GetRequestType, PortSet<TimerState, Fault>>
    {
    }

    /// <summary>
    /// Subscribe - Adds subscriptions
    /// </summary>
    public class Subscribe : Subscribe<SubscribeRequestType, PortSet<SubscribeResponseType, Fault>>
    {
    }

    /// <summary>
    /// SetTimer - Operation
    /// </summary>
    [Description("Sets the timer in milliseconds or indicates the timer has been set.\nThe TimerComplete notification is sent after that interval has passed\nSetting a timer before the previous timer has fired superceds the previous timer.\nSetting a timer to 0 will stop any existing timer.")]
    public class SetTimer : Update<SetTimerRequest, PortSet<DefaultUpdateResponseType, Fault>>
    {
    }

    /// <summary>
    /// Tick - Notification (once a second)
    /// </summary>
    [Description("Sends a notification every second.")]
    public class Tick : Update<TickRequest, PortSet<DefaultUpdateResponseType, Fault>>
    {
        /// <summary>
        /// Default Constructor
        /// </summary>
        public Tick()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="body"></param>
        public Tick(TickRequest body)
            : base(body)
        {
        }
    }

    /// <summary>
    /// TimerComplete - Notification that timer has expired
    /// </summary>
    [DisplayName("(User) TimerComplete")]
    [Description("Indicates that the timer (set with SetTimer) has elapsed.")]
    public class FireTimer : Update<FireTimerRequest, PortSet<DefaultUpdateResponseType, Fault>>
    {
        /// <summary>
        /// Default Constructor
        /// </summary>
        public FireTimer()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="body"></param>
        public FireTimer(FireTimerRequest body)
            : base(body)
        {
        }
    }

    /// <summary>
    /// Wait - Wait for specified time interval
    /// </summary>
    [Description("Wait for the specified interval in milliseconds.\nNote: This is intended for intervals of less than 60 seconds only,\nfor longer intervals use SetTimer.")]
    public class Wait : Submit<SetTimerRequest, PortSet<DefaultSubmitResponseType, Fault>>
    {
    }

    /// <summary>
    /// Time - Gets Date/Time
    /// </summary>
    [DisplayName("(User) GetCurrentTime")]
    [Description("Returns the current time.")]
    public class Time : Submit<TimeRequest, PortSet<TimeResponse, Fault>>
    {
    }
}
