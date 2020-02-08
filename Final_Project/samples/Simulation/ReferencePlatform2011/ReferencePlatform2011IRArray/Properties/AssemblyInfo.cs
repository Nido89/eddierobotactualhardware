//------------------------------------------------------------------------------
//  <copyright file="AssemblyInfo.cs" company="Microsoft Corporation">
//      Copyright (C) Microsoft Corporation.  All rights reserved.
//  </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using dss = Microsoft.Dss.Core.Attributes;
using interop = System.Runtime.InteropServices;

[assembly: dss.ServiceDeclaration(dss.DssServiceDeclaration.ServiceBehavior)]
[assembly: interop.ComVisible(false)]

