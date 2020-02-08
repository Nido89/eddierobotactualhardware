//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
// 
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: EmbeddedSimTypes.cs $ $Revision: 3 $
//-----------------------------------------------------------------------

using Microsoft.Ccr.Core;
using Microsoft.Dss.Core.Attributes;
using Microsoft.Dss.ServiceModel.Dssp;
using System;
using System.Collections.Generic;
using W3C.Soap;

namespace Microsoft.Simulation.Embedded
{
    
    /// <summary>
    /// EmbeddedSim Contract class
    /// </summary>
    public sealed class Contract
    {
        /// <summary>
        /// The Dss Service contract
        /// </summary>
        public const String Identifier = "http://schemas.microsoft.com/robotics/simulation/services/2008/05/embeddedsim.user.html";
    }

    /// <summary>
    /// The EmbeddedSim State
    /// </summary>
    [DataContract()]
    public class EmbeddedSimState
    {
    }

    /// <summary>
    /// EmbeddedSim Main Operations Port
    /// </summary>
    [ServicePort()]
    public class EmbeddedSimOperations : PortSet<DsspDefaultLookup, DsspDefaultDrop, Get>
    {
    }
    /// <summary>
    /// EmbeddedSim Get Operation
    /// </summary>
    public class Get : Get<GetRequestType, PortSet<EmbeddedSimState, Fault>>
    {
        /// <summary>
        /// EmbeddedSim Get Operation
        /// </summary>
        public Get()
        {
        }
        /// <summary>
        /// EmbeddedSim Get Operation
        /// </summary>
        public Get(Microsoft.Dss.ServiceModel.Dssp.GetRequestType body) :
            base(body)
        {
        }
        /// <summary>
        /// EmbeddedSim Get Operation
        /// </summary>
        public Get(Microsoft.Dss.ServiceModel.Dssp.GetRequestType body, Microsoft.Ccr.Core.PortSet<EmbeddedSimState, W3C.Soap.Fault> responsePort) :
            base(body, responsePort)
        {
        }
    }

    #region WinForms communication

    public class FromWinformEvents : Port<FromWinformMsg>
    {
    }

    public class FromWinformMsg
    {
        public enum MsgEnum
        {
            Loaded,
            Drag,
            Zoom
        }

        private string[] _parameters;
        public string[] Parameters
        {
            get { return _parameters; }
            set { _parameters = value; }
        }

        private MsgEnum _command;
        public MsgEnum Command
        {
            get { return _command; }
            set { _command = value; }
        }

        private object _object;
        public object Object
        {
            get { return _object; }
            set { _object = value; }
        }

        public FromWinformMsg(MsgEnum command, string[] parameters)
        {
            _command = command;
            _parameters = parameters;
        }
        public FromWinformMsg(MsgEnum command, string[] parameters, object objectParam)
        {
            _command = command;
            _parameters = parameters;
            _object = objectParam;
        }
    }
    #endregion
}
