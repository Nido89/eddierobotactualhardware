//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: VpldatetimeTypes.cs $ $Revision: 5 $
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;
using System.Diagnostics;

using Microsoft.Ccr.Core;
using Microsoft.Dss.Core;
using Microsoft.Dss.Core.Attributes;
using Microsoft.Dss.ServiceModel.Dssp;

using W3C.Soap;

namespace Microsoft.Robotics.Services.Sample.VplDateTimeHelper
{
    /// <summary>
    /// Vpl DateTime helper Contract class
    /// </summary>
    public sealed class Contract
    {
        /// <summary>
        /// The Dss Service contract
        /// </summary>
        [DataMember()]
        public const String Identifier = "http://schemas.microsoft.com/2008/08/vpldatetimehelper.user.html";
    }

    /// <summary>
    /// The DateTime State
    /// </summary>
    [DataContract()]
    public class DateTimeState
    {
        private DateTime _now;
        /// <summary>
        /// Now - Current Date/Time
        /// </summary>
        [DataMember]
        [Description("Indicates the current date and time on this computer, expressed as the local time.")]
        public DateTime Now
        {
            get { return _now; }
            set { _now = value; }
        }

        private DateTime _nowUTC;
        /// <summary>
        /// UtcNow - Current Date/Time in UTC
        /// </summary>
        [DataMember]
        [Description("Indicates the current date and time on this computer, expressed as the Universal Time Coordinated (UTC).")]
        public DateTime UtcNow
        {
            get { return _nowUTC; }
            set { _nowUTC = value; }
        }

        private DateTime _today;
        /// <summary>
        /// Today - Today's date
        /// </summary>
        [DataMember]
        [Description("Indicates today's date.")]
        public DateTime Today
        {
            get { return _today; }
            set { _today = value; }
        }
    }

    /// <summary>
    /// Vpldatetime Main Operations Port
    /// </summary>
    [ServicePort]
    public class VplDateTimeHelperOperations : PortSet
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public VplDateTimeHelperOperations()
            : base(
                typeof(DsspDefaultLookup),
                typeof(DsspDefaultDrop),
                typeof(Get),
                typeof(CreateDateTime),
                typeof(CreateTimeSpan),
                typeof(AddTimeSpanToDateTime),
                typeof(AddTimeSpanToTimeSpan),
                typeof(SubtractTimeSpanFromTimeSpan),
                typeof(SubtractTimeSpanFromDateTime),
                typeof(SubtractDateTimeFromDateTime)
            )
        {
        }

    }


    /// <summary>
    /// Get - Operation
    /// </summary>
    [Description("Returns the state of the Date Time service.")]
    public class Get : Get<GetRequestType, PortSet<DateTimeState, Fault>>
    {
    }

    /// <summary>
    /// CreateDateTime - Operation
    /// </summary>
    [DisplayName("(User) CreateDateTime")]
    [Description("Returns a DateTime created from date and time parameters.")]
    public class CreateDateTime : Submit<CreateDateTimeRequest, PortSet<DateTime, Fault>>
    {
    }

    /// <summary>
    /// CreateDateTimeRequest
    /// </summary>
    [DataContract]
    public class CreateDateTimeRequest
    {
        /// <summary>
        /// Year
        /// </summary>
        [DataMember()]
        [Description("Indicates the year of the date.")]
        public int Year
        { get; set; }

        /// <summary>
        /// Month
        /// </summary>
        [DataMember()]
        [Description("Indicates the month of the date.")]
        public int Month
        { get; set; }

        /// <summary>
        /// Day
        /// </summary>
        [DataMember()]
        [Description("Indicates the day of the month.")]
        public int Day
        { get; set; }

        /// <summary>
        /// Hour
        /// </summary>
        [DataMember()]
        [Description("Indicates the hour of the date.")]
        public int Hour
        { get; set; }

        /// <summary>
        /// Minute
        /// </summary>
        [DataMember()]
        [Description("Indicates the minute of the date.")]
        public int Minute
        { get; set; }

        /// <summary>
        /// Second
        /// </summary>
        [DataMember()]
        [Description("Indicates the second of the date.")]
        public int Second
        { get; set; }

        /// <summary>
        /// Millisecond
        /// </summary>
        [DataMember()]
        [Description("Indicates the millisecond of the date.")]
        public int Millisecond
        { get; set; }
    }

    /// <summary>
    /// CreateTimeSpan - Operation
    /// </summary>
    [DisplayName("(User) CreateTimeSpan")]
    [Description("Returns a TimeSpan created from parameters.")]
    public class CreateTimeSpan : Submit<CreateTimeSpanRequest, PortSet<DssTimeSpan, Fault>>
    {
    }

    /// <summary>
    /// CreateTimeSpanRequest
    /// </summary>
    [DataContract]
    public class CreateTimeSpanRequest
    {
        /// <summary>
        /// Days
        /// </summary>
        [DataMember()]
        [Description("Indicates the number of days of the TimeSpan.")]
        public int Days
        { get; set; }

        /// <summary>
        /// Hours
        /// </summary>
        [DataMember()]
        [Description("Indicates the number of hours of the Timespan.")]
        public int Hours
        { get; set; }

        /// <summary>
        /// Minutes
        /// </summary>
        [DataMember()]
        [Description("Indicates the number of minutes of the TimeSpan.")]
        public int Minutes
        { get; set; }

        /// <summary>
        /// Seconds
        /// </summary>
        [DataMember()]
        [Description("Indicates the number of seconds of the TimeSpan.")]
        public int Seconds
        { get; set; }

        /// <summary>
        /// Milliseconds
        /// </summary>
        [DataMember()]
        [Description("Indicates the number of milliseconds of the TimeSpan.")]
        public int Milliseconds
        { get; set; }
    }

    /// <summary>
    /// Add TimeSpan To DateTime - Operation
    /// </summary>
    [DisplayName("(User) AddTimeSpanToDateTime")]
    [Description("Returns the result of adding a TimeSpan to a DateTime.")]
    public class AddTimeSpanToDateTime : Submit<AddTimeSpanToDateTimeRequest, PortSet<DateTime, Fault>>
    {
    }

    /// <summary>
    /// AddTimeSpanToDateTimeRequest
    /// </summary>
    [DataContract]
    public class AddTimeSpanToDateTimeRequest
    {
        /// <summary>
        /// DateTime - The Date/Time to add to
        /// </summary>
        [DataMember]
        [Description("Identifies the DateTime to which to add the TimeSpan.")]
        public DateTime DateTime
        { get;set; }

        /// <summary>
        /// TimeSpan - The TimeSpan to add
        /// </summary>
        [DataMember]
        [Description("Identifies the TimeSpan that will be added to DateTime.")]
        public DssTimeSpan TimeSpan
        { get;set; }
    }

    /// <summary>
    /// Add TimeSpan To TimeSpan - Operation
    /// </summary>
    [DisplayName("(User) AddTimeSpanToTimeSpan")]
    [Description("Returns the result of adding a TimeSpan to a TimeSpan.")]
    public class AddTimeSpanToTimeSpan : Submit<AddTimeSpanToTimeSpanRequest, PortSet<DssTimeSpan, Fault>>
    {
    }

    /// <summary>
    /// AddTimeSpanToTimeSpanRequest
    /// </summary>
    [DataContract]
    public class AddTimeSpanToTimeSpanRequest
    {
        /// <summary>
        /// TimeSpan1 - The TimeSpan to add to
        /// </summary>
        [DataMember]
        [Description("Identifies the TimeSpan to which to add the TimeSpan.")]
        public DssTimeSpan TimeSpan1
        { get; set; }

        /// <summary>
        /// TimeSpan2 - The TimeSpan to add
        /// </summary>
        [DataMember]
        [Description("Identifies the TimeSpan that will be added to TimeSpan.")]
        public DssTimeSpan TimeSpan2
        { get; set; }
    }

    /// <summary>
    /// Subtract TimeSpan From DateTime - Operation
    /// </summary>
    [DisplayName("(User) SubtractTimeSpanFromDateTime")]
    [Description("Returns the result of subtracting a TimeSpan from a DateTime.")]
    public class SubtractTimeSpanFromDateTime : Submit<SubtractTimeSpanFromDateTimeRequest, PortSet<DateTime, Fault>>
    {
    }

    /// <summary>
    /// SubtractTimeSpanFromDateTimeRequest
    /// </summary>
    [DataContract]
    public class SubtractTimeSpanFromDateTimeRequest
    {
        /// <summary>
        /// DateTime - Date/Time to subtract from
        /// </summary>
        [DataMember]
        [Description("Identifies the DateTime from which to subtract the TimeSpan.")]
        public DateTime DateTime
        { get; set; }

        /// <summary>
        /// TimeSpan - The TimeSpan to subtract
        /// </summary>
        [DataMember]
        [Description("Identifies the TimeSpan that will be subtracted from DateTime.")]
        public DssTimeSpan TimeSpan
        { get; set; }
    }
    
    /// <summary>
    /// Subtract DateTime From DateTime - Operation
    /// </summary>
    [DisplayName("(User) SubtractDateTimeFromDateTime")]
    [Description("Returns the result of subtracting a DateTime from another DateTime,\nwhich is the TimeSpan interval between the DateTimes.")]
    public class SubtractDateTimeFromDateTime : Submit<SubtractDateTimeFromDateTimeRequest, PortSet<DssTimeSpan, Fault>>
    {
    }

    /// <summary>
    /// SubtractDateTimeFromDateTimeRequest
    /// </summary>
    [DataContract]
    public class SubtractDateTimeFromDateTimeRequest
    {
        /// <summary>
        /// DateTimeToSubtractFrom
        /// </summary>
        [DataMember]
        [Description("Identifies the DateTime (the minuend)\nfrom which to subtract the other DateTime.")]
        public DateTime DateTimeToSubtractFrom
        { get; set; }

        /// <summary>
        /// DateTimeToSubtract
        /// </summary>
        [DataMember]
        [Description("Identifies the DateTime (the subtrahend)\nwhich will be subtracted from the other DateTime.")]
        public DateTime DateTimeToSubtract
        { get; set; }
    }

    /// <summary>
    /// Subtract TimeSpan From TimeSpan - Operation
    /// </summary>
    [DisplayName("(User) SubtractTimeSpanFromTimeSpan")]
    [Description("Returns the result of subtracting a TimeSpan from TimeSpan.")]
    public class SubtractTimeSpanFromTimeSpan : Submit<SubtractTimeSpanFromTimeSpanRequest, PortSet<DssTimeSpan, Fault>>
    {
    }

    /// <summary>
    /// SubtractTimeSpanFromTimeSpanRequest
    /// </summary>
    [DataContract]
    public class SubtractTimeSpanFromTimeSpanRequest
    {
        /// <summary>
        /// TimeSpanToSubtractFrom
        /// </summary>
        [DataMember]
        [Description("Identifies the DateTime (the minuend) from which\nto subtract the other DateTime.")]
        public DssTimeSpan TimeSpanToSubtractFrom
        { get; set; }

        /// <summary>
        /// TimeSpanToSubtract
        /// </summary>
        [DataMember]
        [Description("identifies the DateTime (the subtrahend) which\nwill be subtracted from the other DateTime.")]
        public DssTimeSpan TimeSpanToSubtract
        { get; set; }
    }

    /// <summary>
    /// DssTimeSpan - Data type for serialization
    /// </summary>
    [DataContract(ExcludeFromProxy = true)]
    [XmlRootAttribute("DssTimeSpan", Namespace = Contract.Identifier)]
    public class DssTimeSpan : ICloneable, IDssSerializable, IComparable
    {
        private TimeSpan _time = TimeSpan.Zero;

        /// <summary>
        /// Ticks
        /// </summary>
        [Description("The number of ticks that represent the value of the TimeSpan.")]
        public long Ticks
        {
            get { return _time.Ticks; }
            set
            {
                _time = new TimeSpan(value);
            }
        }
        private TimeSpan TimeSpan
        {
            get { return _time; }
        }
        /// Days
        [Description("Specifies the number of days of the time interval represented by the TimeSpan.")]
        public int Days
        {
            get { return _time.Days; }
        }
        /// Hours
        [Description("Specifies the number of hours of the time interval represented by the TimeSpan.")]
        public int Hours
        {
            get { return _time.Hours; }
        }
        /// Minutes
        [Description("Specifies the number of minutes of the time interval represented by the TimeSpan.")]
        public int Minutes
        {
            get { return _time.Minutes; }
        }
        /// Seconds
        [Description("Specifies the number of seconds of the time interval represented by the TimeSpan.")]
        public int Seconds
        {
            get { return _time.Seconds; }
        }
        /// Milliseconds
        [Description("Specifies the number of milliseconds of the time interval represented by the TimeSpan.")]
        public int Milliseconds
        {
            get { return _time.Milliseconds; }
        }
        /// TotalDays
        [Description("Specifies the value of the TimeSpan expressed in whole and fractional days.")]
        public double TotalDays
        {
            get { return _time.TotalDays; }
        }
        /// TotalHours
        [Description("Specifies the value of the TimeSpan expressed in whole and fractional hours.")]
        public double TotalHours
        {
            get { return _time.TotalHours; }
        }
        /// TotalMinutes
        [Description("Specifies the value of the TimeSpan expressed in whole and fractional minutes.")]
        public double TotalMinutes
        {
            get { return _time.TotalMinutes; }
        }
        /// TotalSeconds
        [Description("Specifies the value of the TimeSpan expressed in whole and fractional seconds.")]
        public double TotalSeconds
        {
            get { return _time.TotalSeconds; }
        }
        /// TotalMilliseconds
        [Description("Specifies the value of the TimeSpan expressed in whole and fractional milliseconds.")]
        public double TotalMilliseconds
        {
            get { return _time.TotalMilliseconds; }
        }

        #region constructors
        /// <summary>
        /// A Serializable TimeSpan
        /// </summary>
        public DssTimeSpan()
        {
        }

        /// <summary>
        /// A Serializable TimeSpan
        /// </summary>
        /// <param name="ticks"></param>
        public DssTimeSpan(long ticks)
        {
            this.Ticks = ticks;
        }
        /// <summary>
        /// A Serializable TimeSpan
        /// </summary>
        /// <param name="time"></param>
        public DssTimeSpan(TimeSpan time)
        {
            _time = time;
        }
        #endregion

        #region casts
        /// <summary>
        /// Convert to TimeSpan
        /// </summary>
        /// <returns></returns>
        public TimeSpan ToTimeSpan()
        {
            return _time;
        }

        /// <summary>
        /// Convert DssTimeSpan to TimeSpan
        /// </summary>
        /// <param name="dsstime"></param>
        /// <returns></returns>
        public static implicit operator TimeSpan(DssTimeSpan dsstime)
        {
            if (dsstime == null) return TimeSpan.Zero;
            return dsstime.ToTimeSpan();
        }

        /// <summary>
        /// Convert Uri to DssUri
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static implicit operator DssTimeSpan(TimeSpan time)
        {
            if (time == null) return null;
            return new DssTimeSpan(time.Ticks);
        }

        /// <summary>
        /// Returns an appropriate string representation of the current TimeSpan
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return _time.ToString();
        }
        #endregion

        #region IDssSerializable Members
        /// <summary>
        /// Copy To Dss TimeSpan
        /// </summary>
        public virtual void CopyTo(IDssSerializable target)
        {
            if (target == null)
                target = new DssTimeSpan();

            DssTimeSpan typedTarget = target as DssTimeSpan;

            if (typedTarget == null)
                throw new ArgumentException("CopyTo({0}) requires type {0}", this.GetType().FullName);

            typedTarget.Ticks = this.Ticks;
        }
        /// <summary>
        /// Clone Dss TimeSpan
        /// </summary>
        public virtual object Clone()
        {
            DssTimeSpan target = new DssTimeSpan();
            target.Ticks = this.Ticks;
            return target;
        }

        /// <summary>
        /// Serialize Dss TimeSpan
        /// </summary>
        public virtual void Serialize(System.IO.BinaryWriter writer)
        {
            writer.Write(Ticks);
        }
        /// <summary>
        /// Deserialize Dss TimeSpan
        /// </summary>
        public virtual object Deserialize(System.IO.BinaryReader reader)
        {
            this.Ticks = reader.ReadInt64();
            return this;
        }
        #endregion

        #region IComparable Members

        /// <summary>
        /// Allows comparison of this instance with another DssTimeSpan or TimeSpan.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        int IComparable.CompareTo(object obj)
        {
            if (obj == null)
                return (this._time == null) ? 0 : 1;

            DssTimeSpan target = obj as DssTimeSpan;
            if (target == null)
            {
                if (obj.GetType() == typeof(TimeSpan))
                    return this.ToTimeSpan().CompareTo(obj);

                throw new ArgumentException(string.Format("{0} cannot be compared to DssTimeSpan", obj.GetType()));
            }
            return this.Ticks.CompareTo(target.Ticks);
        }
        #endregion
    }
}
