//------------------------------------------------------------------------------
//  <copyright file="AssemblyInfo.cs" company="Microsoft Corporation">
//      Copyright (C) Microsoft Corporation.  All rights reserved.
//  </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.Dss.Core.Attributes;
using interopservices = System.Runtime.InteropServices;

[assembly: ServiceDeclaration(DssServiceDeclaration.ServiceBehavior)]
[assembly: interopservices.ComVisible(false)]
