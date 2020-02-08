//-----------------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples
//
//  <copyright file="AssemblyInfo.cs" company="Microsoft Corporation">
//      Copyright (C) Microsoft Corporation.  All rights reserved.
//  </copyright>
//
//  $File: AssemblyInfo.cs $ $Revision: 1 $
//-----------------------------------------------------------------------------
using System.Reflection;
using dss = Microsoft.Dss.Core.Attributes;
using interop = System.Runtime.InteropServices;

[assembly: dss.ServiceDeclaration(dss.DssServiceDeclaration.ServiceBehavior)]
[assembly: interop.ComVisible(false)]
[assembly: AssemblyTitle("RobotDashboard")]
[assembly: AssemblyDescription("The Robot Dashboard can be used to control a Reference Platform robot. Use an Xbox Controller to drive the robot. The Depth and RGB data streams are displayed in separate windows.")]
