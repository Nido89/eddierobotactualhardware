//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: Coordination.cs $ $Revision: 14 $
//-----------------------------------------------------------------------

using Microsoft.Dss.Core;
using Microsoft.Dss.Core.Attributes;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Xml.Serialization;

namespace Microsoft.Robotics.Services.Coordination
{
    /// <summary>
    /// Coordination Contract
    /// </summary>
    [DisplayName("(User) Cross Service Coordination Helper")]
    [Description("Provides message coordination utilities to synchronize across multiple independent requests.")]
    public static class Contract
    {
        /// Contract Identifier
        public const string Identifier = "http://schemas.microsoft.com/robotics/2006/07/coordination.html";
    }

    /// <summary>
    /// Used for advanced services which
    /// need to coordinate motor commands
    /// </summary>
    [DataContract]
    [XmlRootAttribute("ActuatorCoordination", Namespace = Contract.Identifier)]
    public class ActuatorCoordination: ICloneable, IDssSerializable
    {
        /// <summary>
        /// ActuatorCoordination Constructor
        /// </summary>
        public ActuatorCoordination()
        {
            Count = 2;
            RequestId = Guid.NewGuid();
        }

        /// <summary>
        /// ActuatorCoordination Initialization Constructor
        /// </summary>
        /// <param name="requestCount"></param>
        public ActuatorCoordination(int requestCount)
        {
            Count = requestCount;
            RequestId = Guid.NewGuid();
        }

        /// <summary>
        /// The number of motors being coordinated.
        /// </summary>
        [DataMember]
        [Description("Identifies the number of motors being coordinated.")]
        public int Count;

        /// <summary>
        /// A guid which uniquely identifies
        /// a set of coordinated motor requests
        /// </summary>
        [DataMember]
        [Description("Specifies a unique identifier (GUID) for a set of motor requests.")]
        public Guid RequestId;


        #region IDssSerializable Members

        /// <summary>
        /// Clone ActuatorCoordination
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            ActuatorCoordination dmc = new ActuatorCoordination();
            dmc.RequestId = this.RequestId;
            dmc.Count = this.Count;
            return dmc;
        }

        /// <summary>
        /// CopyTo
        /// </summary>
        /// <param name="target"></param>
        public void CopyTo(IDssSerializable target)
        {
            ActuatorCoordination typedTarget = target as ActuatorCoordination;
            if (typedTarget == null)
                throw new ArgumentException(string.Format("CopyTo({0}) requires type {0}", this.GetType().FullName));
            typedTarget.Count = this.Count;
            typedTarget.RequestId = this.RequestId;
        }

        /// <summary>
        /// Deserialize
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public object Deserialize(System.IO.BinaryReader reader)
        {
            this.Count = reader.ReadInt32();
            this.RequestId = new Guid(reader.ReadString());
            return this;
        }

        /// <summary>
        /// Serialize
        /// </summary>
        /// <param name="writer"></param>
        public void Serialize(System.IO.BinaryWriter writer)
        {
            writer.Write(this.Count);
            writer.Write(this.RequestId.ToString());
        }

        #endregion
    }
}
