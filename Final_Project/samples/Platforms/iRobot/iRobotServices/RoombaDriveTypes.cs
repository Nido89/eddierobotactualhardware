//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: RoombaDriveTypes.cs $ $Revision: 9 $
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using irobot = Microsoft.Robotics.Services.IRobot.Roomba;
using drive = Microsoft.Robotics.Services.Drive.Proxy;

using Microsoft.Ccr.Core;
using Microsoft.Dss.Core;
using Microsoft.Dss.Core.Attributes;
using Microsoft.Dss.Core.DsspHttp;
using Microsoft.Dss.ServiceModel.Dssp;

using W3C.Soap;


namespace Microsoft.Robotics.Services.IRobot.Roomba.Drive
{

    /// <summary>
    /// RoombaDrive Contract
    /// </summary>
    public static class Contract
    {
        /// The Unique Contract Identifier for the RoombaDrive service
        public const String Identifier = "http://schemas.microsoft.com/robotics/2006/12/irobot/drive.user.html";
    }

    struct RoombaDriveState
    {
        public bool driveCommandInProgress;
        public drive.DriveRequestOperation pendingDriveOperation;
        public irobot.IRobotModel robotModel;

        public drive.DriveRequestOperation _internalPendingDriveOperation;
    }
}
