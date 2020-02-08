//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
// 
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: GenericServiceImplementationTypes.cs $ $Revision: 7 $
//----------------------------------------------------------------------

using Microsoft.Dss.Core.Attributes;
using System;
using System.Collections.Generic;

namespace ServiceTutorial9.Implementation
{
    #region Service Contract
    /// <summary>
    /// Generic Service Implementation Contract Identifier
    /// </summary>
    public sealed class Contract
    {
        /// The Unique Contract Identifier for this service
        [DataMember()]
        public const String Identifier = "http://schemas.microsoft.com/2007/08/servicetutorial9/genericservice/implementation.user.html";
    }
    #endregion
}
