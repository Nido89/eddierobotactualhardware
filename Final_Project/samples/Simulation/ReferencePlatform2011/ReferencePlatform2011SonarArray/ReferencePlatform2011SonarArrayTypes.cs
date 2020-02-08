//------------------------------------------------------------------------------
//  <copyright file="ReferencePlatform2011SonarArrayTypes.cs" company="Microsoft Corporation">
//      Copyright (C) Microsoft Corporation.  All rights reserved.
//  </copyright>
//------------------------------------------------------------------------------
namespace Microsoft.Robotics.Services.Simulation.ReferencePlatform2011.SonarArray
{
    using Microsoft.Dss.Core.Attributes;

    /// <summary>
    /// The contract
    /// </summary>
    public sealed class Contract
    {
        /// <summary>
        /// The contract
        /// </summary>
        [DataMember]
        public const string Identifier = "http://schemas.microsoft.com/robotics/2011/08/referenceplatform2011sonararray.user.html";
    }
}