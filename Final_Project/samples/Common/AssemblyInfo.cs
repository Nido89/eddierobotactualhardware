//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: AssemblyInfo.cs $ $Revision: 5 $
//-----------------------------------------------------------------------

using System;
using Microsoft.Dss.Core;
using Microsoft.Dss.Core.Attributes;

[assembly: ServiceDeclaration(DssServiceDeclaration.ServiceBehavior | DssServiceDeclaration.DataContract)]
[assembly: EmbeddedResource("*.png,Drive.user.xslt,WebCam.user.xslt")]