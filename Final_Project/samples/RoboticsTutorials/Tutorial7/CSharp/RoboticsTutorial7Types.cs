//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: RoboticsTutorial7Types.cs $ $Revision: 6 $
//-----------------------------------------------------------------------

using Microsoft.Ccr.Core;
using Microsoft.Dss.Core.Attributes;
using Microsoft.Dss.ServiceModel.Dssp;
using System;
using System.Collections.Generic;
using W3C.Soap;


namespace Microsoft.Robotics.Services.RoboticsTutorial7
{

    /// <summary>
    /// RoboticsTutorial7 Contract class
    /// </summary>
    public sealed class Contract
    {
        /// <summary>
        /// The Dss Service contract
        /// </summary>
        public const String Identifier = "http://schemas.microsoft.com/robotics/2007/05/roboticstutorial7.user.html";
    }
    /// <summary>
    /// The RoboticsTutorial7 State
    /// </summary>
    [DataContract]
    public class RoboticsTutorial7State
    {
    }
    /// <summary>
    /// RoboticsTutorial7 Main Operations Port
    /// </summary>
    [ServicePort]
    public class RoboticsTutorial7Operations : PortSet<DsspDefaultLookup, DsspDefaultDrop, Get>
    {
        /// <summary>
        /// Required Lookup request body type
        /// </summary>
        public virtual Microsoft.Ccr.Core.PortSet<Microsoft.Dss.ServiceModel.Dssp.LookupResponse,W3C.Soap.Fault> DsspDefaultLookup()
        {
            Microsoft.Dss.ServiceModel.Dssp.LookupRequestType body = new Microsoft.Dss.ServiceModel.Dssp.LookupRequestType();
            Microsoft.Dss.ServiceModel.Dssp.DsspDefaultLookup op = new Microsoft.Dss.ServiceModel.Dssp.DsspDefaultLookup(body);
            this.Post(op);
            return op.ResponsePort;

        }
        /// <summary>
        /// Post Dssp Default Lookup and return the response port.
        /// </summary>
        public virtual Microsoft.Ccr.Core.PortSet<Microsoft.Dss.ServiceModel.Dssp.LookupResponse,W3C.Soap.Fault> DsspDefaultLookup(Microsoft.Dss.ServiceModel.Dssp.LookupRequestType body)
        {
            Microsoft.Dss.ServiceModel.Dssp.DsspDefaultLookup op = new Microsoft.Dss.ServiceModel.Dssp.DsspDefaultLookup();
            op.Body = body ?? new Microsoft.Dss.ServiceModel.Dssp.LookupRequestType();
            this.Post(op);
            return op.ResponsePort;

        }
        /// <summary>
        /// A request to drop the service.
        /// </summary>
        public virtual Microsoft.Ccr.Core.PortSet<Microsoft.Dss.ServiceModel.Dssp.DefaultDropResponseType,W3C.Soap.Fault> DsspDefaultDrop()
        {
            Microsoft.Dss.ServiceModel.Dssp.DropRequestType body = new Microsoft.Dss.ServiceModel.Dssp.DropRequestType();
            Microsoft.Dss.ServiceModel.Dssp.DsspDefaultDrop op = new Microsoft.Dss.ServiceModel.Dssp.DsspDefaultDrop(body);
            this.Post(op);
            return op.ResponsePort;

        }
        /// <summary>
        /// Post Dssp Default Drop and return the response port.
        /// </summary>
        public virtual Microsoft.Ccr.Core.PortSet<Microsoft.Dss.ServiceModel.Dssp.DefaultDropResponseType,W3C.Soap.Fault> DsspDefaultDrop(Microsoft.Dss.ServiceModel.Dssp.DropRequestType body)
        {
            Microsoft.Dss.ServiceModel.Dssp.DsspDefaultDrop op = new Microsoft.Dss.ServiceModel.Dssp.DsspDefaultDrop();
            op.Body = body ?? new Microsoft.Dss.ServiceModel.Dssp.DropRequestType();
            this.Post(op);
            return op.ResponsePort;

        }
        /// <summary>
        /// Required Get body type
        /// </summary>
        public virtual Microsoft.Ccr.Core.PortSet<RoboticsTutorial7State,W3C.Soap.Fault> Get()
        {
            Microsoft.Dss.ServiceModel.Dssp.GetRequestType body = new Microsoft.Dss.ServiceModel.Dssp.GetRequestType();
            Get op = new Get(body);
            this.Post(op);
            return op.ResponsePort;

        }
        /// <summary>
        /// Post Get and return the response port.
        /// </summary>
        public virtual Microsoft.Ccr.Core.PortSet<RoboticsTutorial7State,W3C.Soap.Fault> Get(Microsoft.Dss.ServiceModel.Dssp.GetRequestType body)
        {
            Get op = new Get();
            op.Body = body ?? new Microsoft.Dss.ServiceModel.Dssp.GetRequestType();
            this.Post(op);
            return op.ResponsePort;

        }
    }
    /// <summary>
    /// RoboticsTutorial7 Get Operation
    /// </summary>
    public class Get : Get<GetRequestType, PortSet<RoboticsTutorial7State, Fault>>
    {
        /// <summary>
        /// RoboticsTutorial7 Get Operation
        /// </summary>
        public Get()
        {
        }
        /// <summary>
        /// RoboticsTutorial7 Get Operation
        /// </summary>
        public Get(Microsoft.Dss.ServiceModel.Dssp.GetRequestType body) :
                base(body)
        {
        }
        /// <summary>
        /// RoboticsTutorial7 Get Operation
        /// </summary>
        public Get(Microsoft.Dss.ServiceModel.Dssp.GetRequestType body, Microsoft.Ccr.Core.PortSet<RoboticsTutorial7State,W3C.Soap.Fault> responsePort) :
                base(body, responsePort)
        {
        }
    }
}
