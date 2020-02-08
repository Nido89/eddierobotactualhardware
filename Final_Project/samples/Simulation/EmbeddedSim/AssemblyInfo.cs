//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
// 
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: assemblyinfo.cs $ $Revision: 2 $
//-----------------------------------------------------------------------

using Microsoft.Dss.Core.Attributes;
using System;
using System.Collections.Generic;
using interopservices = System.Runtime.InteropServices;

[assembly: ServiceDeclaration(DssServiceDeclaration.ServiceBehavior)]
[assembly: interopservices.ComVisible(false)]


