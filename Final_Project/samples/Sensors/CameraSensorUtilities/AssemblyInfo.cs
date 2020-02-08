//------------------------------------------------------------------------------
//  <copyright file="AssemblyInfo.cs" company="Microsoft Corporation">
//      Copyright (C) Microsoft Corporation.  All rights reserved.
//  </copyright>
//------------------------------------------------------------------------------
using Microsoft.Dss.Core.Attributes;
using interopservices = System.Runtime.InteropServices;

[assembly: ServiceDeclaration(DssServiceDeclaration.DataContract)]
[assembly: interopservices.ComVisible(false)]