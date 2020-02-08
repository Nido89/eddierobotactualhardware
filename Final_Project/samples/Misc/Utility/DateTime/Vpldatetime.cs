 //-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: Vpldatetime.cs $ $Revision: 6 $
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml;

using Microsoft.Ccr.Core;
using Microsoft.Dss.Core;
using Microsoft.Dss.Core.Attributes;
using Microsoft.Dss.ServiceModel.Dssp;
using Microsoft.Dss.ServiceModel.DsspServiceBase;

using W3C.Soap;

namespace Microsoft.Robotics.Services.Sample.VplDateTimeHelper
{
    /// <summary>
    /// Implementation class for Vpldatetime
    /// </summary>
    [DisplayName("(User) Date Time")]
    [Description("Provides date and time functions (using .Net DateTime and TimeSpan).")]
    [Contract(Contract.Identifier)]
    [DssServiceDescription("http://msdn.microsoft.com/library/dd126863.aspx")]
    public class DateTimeHelperService : DsspServiceBase
    {
        /// <summary>
        /// _main Port
        /// </summary>
        [ServicePort("/vpldatetime", AllowMultipleInstances = false)]
        private VplDateTimeHelperOperations _mainPort = new VplDateTimeHelperOperations();

        /// <summary>
        /// Default Service Constructor
        /// </summary>
        public DateTimeHelperService(DsspServiceCreationPort creationPort) :
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
        /// Get Handler
        /// </summary>
        /// <param name="get"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public void GetHandler(Get get)
        {
            DateTimeState current_state = new DateTimeState();
            current_state.Now = DateTime.Now;
            current_state.UtcNow = DateTime.UtcNow;
            current_state.Today = DateTime.Today;
            get.ResponsePort.Post(current_state);
        }

        /// <summary>
        /// Subtract TimeSpan From Date Time Handler
        /// </summary>
        /// <param name="request"></param>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public void SubtractTimeSpanFromDateTimeHandler(SubtractTimeSpanFromDateTime request)
        {
            request.ResponsePort.Post(request.Body.DateTime.Subtract(request.Body.TimeSpan));
        }

        /// <summary>
        /// Subtract Date Time From Date Time Handler
        /// </summary>
        /// <param name="request"></param>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public void SubtractDateTimeFromDateTimeHandler(SubtractDateTimeFromDateTime request)
        {
            request.ResponsePort.Post(new DssTimeSpan(request.Body.DateTimeToSubtractFrom.Subtract(request.Body.DateTimeToSubtract)));
        }

        /// <summary>
        /// Subtract TimeSpan From TimeSpan Handler
        /// </summary>
        /// <param name="request"></param>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public void SubtractTimeSpanFromTimeSpanHandler(SubtractTimeSpanFromTimeSpan request)
        {
            request.ResponsePort.Post(new DssTimeSpan(request.Body.TimeSpanToSubtractFrom.ToTimeSpan().Subtract(request.Body.TimeSpanToSubtract.ToTimeSpan())));
        }

        /// <summary>
        /// Add TimeSpan To Date Time Handler
        /// </summary>
        /// <param name="request"></param>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public void AddTimeSpanToDateTimeHandler(AddTimeSpanToDateTime request)
        {
            request.ResponsePort.Post(request.Body.DateTime.Add(request.Body.TimeSpan));
        }


        /// <summary>
        /// Add TimeSpan To TimeSpan Handler
        /// </summary>
        /// <param name="request"></param>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public void AddTimeSpanToTimeSpanHandler(AddTimeSpanToTimeSpan request)
        {
            request.ResponsePort.Post(new DssTimeSpan(request.Body.TimeSpan1.ToTimeSpan().Add(request.Body.TimeSpan2.ToTimeSpan())));
        }

        /// <summary>
        /// Create TimeSpan Handler
        /// </summary>
        /// <param name="request"></param>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public void CreateTimeSpanHandler(CreateTimeSpan request)
        {
            request.ResponsePort.Post(
                new DssTimeSpan(
                    new TimeSpan(
                        request.Body.Days
                        , request.Body.Hours
                        , request.Body.Minutes
                        , request.Body.Seconds
                        , request.Body.Milliseconds
                    )
                )
            );
        }

        /// <summary>
        /// Create Date/Time Handler
        /// </summary>
        /// <param name="request"></param>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public void CreateDateTimeHandler(CreateDateTime request)
        {
            request.ResponsePort.Post(
                new DateTime(
                    request.Body.Year
                    , request.Body.Month
                    , request.Body.Day
                    , request.Body.Hour
                    , request.Body.Minute
                    , request.Body.Second
                    , request.Body.Millisecond
                )
            );
        }
    }
}
