//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: EncoderTypes.cs $ $Revision: 23 $
//-----------------------------------------------------------------------

using Microsoft.Ccr.Core;
using Microsoft.Dss.Core.Attributes;
using Microsoft.Dss.Core.DsspHttp;
using Microsoft.Dss.ServiceModel.Dssp;

using System;
using System.Net;
using System.Collections.Generic;
using System.ComponentModel;

using W3C.Soap;

namespace Microsoft.Robotics.Services.Encoder
{
    /// <summary>
    /// Encoder Contract
    /// </summary>
    [DisplayName("(User) Generic Encoder")]
    [Description("Provides access to an encoder.")]
    [DssServiceDescription("http://msdn.microsoft.com/library/dd145252.aspx")]
    public static class Contract
    {
        /// <summary>
        /// Encoder contract
        /// </summary>
        public const string Identifier = "http://schemas.microsoft.com/robotics/2006/05/encoder.html";
    }

    /// <summary>
    /// Encoder Operations Port
    /// </summary>
    [ServicePort]
    public class EncoderOperations : PortSet
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public EncoderOperations()
            : base(
        typeof(DsspDefaultLookup),
        typeof(DsspDefaultDrop),
        typeof(Get),
        typeof(HttpGet),
        typeof(Reset),
        typeof(UpdateTickCount),
        typeof(Replace),
        typeof(ReliableSubscribe),
        typeof(Subscribe))
        {
        }

        /// <summary>
        /// Untyped post
        /// </summary>
        /// <param name="item"></param>
        public void Post(object item)
        {
            base.PostUnknownType(item);
        }

        /// <summary>
        /// Implicit conversion
        /// </summary>
        public static implicit operator Port<DsspDefaultLookup>(EncoderOperations portSet)
        {
            if (portSet == null) return null;
            return (Port<DsspDefaultLookup>)portSet[typeof(DsspDefaultLookup)];
        }
        /// <summary>
        /// Implicit conversion
        /// </summary>
        public static implicit operator Port<DsspDefaultDrop>(EncoderOperations portSet)
        {
            if (portSet == null) return null;
            return (Port<DsspDefaultDrop>)portSet[typeof(DsspDefaultDrop)];
        }
        /// <summary>
        /// Implicit conversion
        /// </summary>
        public static implicit operator Port<ReliableSubscribe>(EncoderOperations portSet)
        {
            if (portSet == null) return null;
            return (Port<ReliableSubscribe>)portSet[typeof(ReliableSubscribe)];
        }
        /// <summary>
        /// Implicit conversion
        /// </summary>
        public static implicit operator Port<Subscribe>(EncoderOperations portSet)
        {
            if (portSet == null) return null;
            return (Port<Subscribe>)portSet[typeof(Subscribe)];
        }
        /// <summary>
        /// Implicit conversion
        /// </summary>
        public static implicit operator Port<Get>(EncoderOperations portSet)
        {
            if (portSet == null) return null;
            return (Port<Get>)portSet[typeof(Get)];
        }
        /// <summary>
        /// Implicit conversion
        /// </summary>
        public static implicit operator Port<Reset>(EncoderOperations portSet)
        {
            if (portSet == null) return null;
            return (Port<Reset>)portSet[typeof(Reset)];
        }
        /// <summary>
        /// Implicit conversion
        /// </summary>
        public static implicit operator Port<UpdateTickCount>(EncoderOperations portSet)
        {
            if (portSet == null) return null;
            return (Port<UpdateTickCount>)portSet[typeof(UpdateTickCount)];
        }
        /// <summary>
        /// Implicit conversion
        /// </summary>
        public static implicit operator Port<Replace>(EncoderOperations portSet)
        {
            if (portSet == null) return null;
            return (Port<Replace>)portSet[typeof(Replace)];
        }
        /// <summary>
        /// Implicit conversion
        /// </summary>
        public static implicit operator Port<HttpGet>(EncoderOperations portSet)
        {
            if (portSet == null) return null;
            return (Port<HttpGet>)portSet[typeof(HttpGet)];
        }
    }

    /// <summary>
    /// Operation Retrieve Encoder State
    /// </summary>
    [Description("Gets the encoder's current state.")]
    public class Get : Get<GetRequestType, PortSet<EncoderState, Fault>>
    {
    }

    /// <summary>
    /// Operation Reset TicksSinceReset Counter
    /// </summary>
    [DisplayName("(User) ResetTickCounter")]
    [Description("Resets (or indicates a reset to) the tick counter.")]
    public class Reset : Update<ResetCounter, PortSet<DefaultUpdateResponseType, Fault>>
    {
        /// <summary>
        /// public constructor
        /// </summary>
        public Reset()
        {
            Body = new ResetCounter();
        }
    }

    /// <summary>
    /// Operation Replace: Configures the encoder
    /// </summary>
    [Description("Changes (or indicates a change to) the encoder's entire state.")]
    [DisplayName("(User) EncoderReplace")]
    public class Replace : Replace<EncoderState, PortSet<DefaultReplaceResponseType, Fault>>
    {
    }

    /// <summary>
    /// Operation UpdateTickcount: Configures the encoder
    /// </summary>
    [Description("Updates (or indicates a update to) the encoder's tick count.")]
    public class UpdateTickCount : Update<UpdateTickCountRequest, PortSet<DefaultUpdateResponseType, Fault>>
    {
    }

    /// <summary>
    /// Operation Subscribe to bumper
    /// </summary>
    public class Subscribe : Subscribe<SubscribeRequestType, PortSet<SubscribeResponseType, Fault>>
    {
    }

    /// <summary>
    /// Operation Subscribe to bumper
    /// </summary>
    public class ReliableSubscribe : Subscribe<ReliableSubscribeRequestType, PortSet<SubscribeResponseType, Fault>>
    {
    }
}
