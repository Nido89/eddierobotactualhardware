//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: AssemblyInfo.cs $ $Revision: 4 $
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using interopservices = System.Runtime.InteropServices;

using Microsoft.Dss.Core.Attributes;

#region Service Declaration
[assembly: ServiceDeclaration(DssServiceDeclaration.DataContract)]
#endregion

[assembly: interopservices.ComVisible(false)]