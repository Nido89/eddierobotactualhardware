//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: ScriptGeneratorTypes.cs $ $Revision: 10 $
//-----------------------------------------------------------------------

using Microsoft.Ccr.Core;
using Microsoft.Dss.Core.Attributes;
using Microsoft.Dss.ServiceModel.Dssp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using W3C.Soap;

using irobot = Microsoft.Robotics.Services.IRobot.Roomba;
using create = Microsoft.Robotics.Services.IRobot.Create;


namespace Microsoft.Robotics.Services.IRobot.Create.ScriptingEngine
{

    /// <summary>
    /// IRobotCreateScript Contract class
    /// </summary>
    public static class Contract
    {
        /// <summary>
        /// The Dss Service contract
        /// </summary>
        public const String Identifier = "http://schemas.microsoft.com/robotics/2007/01/irobot/create/scriptingengine.user.html";
    }

    #region Scripting Data Contracts

    /// <summary>
    /// The IRobotCreateScript State
    /// </summary>
    [DataContract]
    public class IRobotScriptingEngineState
    {
        /// <summary>
        /// The Current Script
        /// </summary>
        [DataMember]
        public ScriptDefinition CurrentScript;

        /// <summary>
        /// Saved Scripts
        /// </summary>
        [DataMember]
        public List<ScriptDefinition> SavedScripts;

    }

    /// <summary>
    /// Clear the current script.
    /// </summary>
    [DataContract]
    public class ClearScript { }

    /// <summary>
    /// Clear the entire command list
    /// </summary>
    [DataContract]
    public class Script
    {
        /// <summary>
        /// The script name
        /// </summary>
        [DataMember]
        public string Name;
    }

    /// <summary>
    /// Clear the entire command list
    /// </summary>
    [DataContract]
    public class ScriptDefinition
    {
        /// <summary>
        /// The script name
        /// </summary>
        [DataMember]
        public string Name;

        /// <summary>
        /// Packet Data ready
        /// </summary>
        [DataMember]
        public byte[] PacketData;

        /// <summary>
        /// The commands which compose this script
        /// </summary>
        [DataMember]
        public List<string> Commands;

        /// <summary>
        /// Expected Script Response Bytes
        /// </summary>
        [DataMember]
        public int ExpectedScriptResponseBytes;

    }

    #endregion

    /// <summary>
    /// IRobotCreateScript Main Operations Port
    /// </summary>
    [ServicePort]
    public class IRobotScriptingEngineOperations:
        PortSet
    {
        /// <summary>
        /// Portset Definition
        /// </summary>
        public IRobotScriptingEngineOperations():base(
            typeof(DsspDefaultLookup),
            typeof(DsspDefaultDrop),
            typeof(Get),
            typeof(ClearCurrentScript),
            typeof(SaveCurrentScript),
            typeof(LoadCurrentScript),
            typeof(DeleteNamedScript),
            typeof(AddStart),
            typeof(AddControl),
            typeof(AddSafe),
            typeof(AddFull),
            typeof(AddDrive),
            typeof(AddDriveDirect),
            typeof(AddPlayScript),
            typeof(AddWaitAngle),
            typeof(AddWaitDistance),
            typeof(AddWaitEvent),
            typeof(AddWaitTime),
            typeof(AddSensors),
            typeof(AddDemo),
            typeof(AddLeds),
            typeof(AddPlaySong),
            typeof(AddQueryList),
            typeof(AddCover),
            typeof(AddDefineSong),
            typeof(AddDigitalOutputs),
            typeof(AddForceSeekingDock),
            typeof(AddPower),
            typeof(AddPWMLowSideDrivers),
            typeof(AddSendIR),
            typeof(AddShowScript),
            typeof(AddSpot),
            typeof(AddStream),
            typeof(AddStreamPauseResume))
        { }

        /// <summary>
        /// Untyped Post
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public void Post(object item) { base.PostUnknownType(item); }

        #region Implicit Operators
        /// <summary>
        /// Implicit Operator for Port of DsspDefaultLookup
        /// </summary>
        /// <param name="portSet"></param>
        /// <returns></returns>
        public static implicit operator Port<DsspDefaultLookup>(IRobotScriptingEngineOperations portSet)
        {
            if (portSet == null) return null;
            return (Port<DsspDefaultLookup>)portSet[typeof(DsspDefaultLookup)];
        }
        /// <summary>
        /// Implicit Operator for Port of DsspDefaultDrop
        /// </summary>
        /// <param name="portSet"></param>
        /// <returns></returns>
        public static implicit operator Port<DsspDefaultDrop>(IRobotScriptingEngineOperations portSet)
        {
            if (portSet == null) return null;
            return (Port<DsspDefaultDrop>)portSet[typeof(DsspDefaultDrop)];
        }
        /// <summary>
        /// Implicit Operator for Port of Get
        /// </summary>
        /// <param name="portSet"></param>
        /// <returns></returns>
        public static implicit operator Port<Get>(IRobotScriptingEngineOperations portSet)
        {
            if (portSet == null) return null;
            return (Port<Get>)portSet[typeof(Get)];
        }
        /// <summary>
        /// Implicit Operator for Port of ClearCurrentScript
        /// </summary>
        /// <param name="portSet"></param>
        /// <returns></returns>
        public static implicit operator Port<ClearCurrentScript>(IRobotScriptingEngineOperations portSet)
        {
            if (portSet == null) return null;
            return (Port<ClearCurrentScript>)portSet[typeof(ClearCurrentScript)];
        }
        /// <summary>
        /// Implicit Operator for Port of SaveCurrentScript
        /// </summary>
        /// <param name="portSet"></param>
        /// <returns></returns>
        public static implicit operator Port<SaveCurrentScript>(IRobotScriptingEngineOperations portSet)
        {
            if (portSet == null) return null;
            return (Port<SaveCurrentScript>)portSet[typeof(SaveCurrentScript)];
        }
        /// <summary>
        /// Implicit Operator for Port of LoadCurrentScript
        /// </summary>
        /// <param name="portSet"></param>
        /// <returns></returns>
        public static implicit operator Port<LoadCurrentScript>(IRobotScriptingEngineOperations portSet)
        {
            if (portSet == null) return null;
            return (Port<LoadCurrentScript>)portSet[typeof(LoadCurrentScript)];
        }
        /// <summary>
        /// Implicit Operator for Port of DeleteNamedScript
        /// </summary>
        /// <param name="portSet"></param>
        /// <returns></returns>
        public static implicit operator Port<DeleteNamedScript>(IRobotScriptingEngineOperations portSet)
        {
            if (portSet == null) return null;
            return (Port<DeleteNamedScript>)portSet[typeof(DeleteNamedScript)];
        }
        /// <summary>
        /// Implicit Operator for Port of AddStart
        /// </summary>
        /// <param name="portSet"></param>
        /// <returns></returns>
        public static implicit operator Port<AddStart>(IRobotScriptingEngineOperations portSet)
        {
            if (portSet == null) return null;
            return (Port<AddStart>)portSet[typeof(AddStart)];
        }
        /// <summary>
        /// Implicit Operator for Port of AddControl
        /// </summary>
        /// <param name="portSet"></param>
        /// <returns></returns>
        public static implicit operator Port<AddControl>(IRobotScriptingEngineOperations portSet)
        {
            if (portSet == null) return null;
            return (Port<AddControl>)portSet[typeof(AddControl)];
        }
        /// <summary>
        /// Implicit Operator for Port of AddSafe
        /// </summary>
        /// <param name="portSet"></param>
        /// <returns></returns>
        public static implicit operator Port<AddSafe>(IRobotScriptingEngineOperations portSet)
        {
            if (portSet == null) return null;
            return (Port<AddSafe>)portSet[typeof(AddSafe)];
        }
        /// <summary>
        /// Implicit Operator for Port of AddFull
        /// </summary>
        /// <param name="portSet"></param>
        /// <returns></returns>
        public static implicit operator Port<AddFull>(IRobotScriptingEngineOperations portSet)
        {
            if (portSet == null) return null;
            return (Port<AddFull>)portSet[typeof(AddFull)];
        }
        /// <summary>
        /// Implicit Operator for Port of AddDrive
        /// </summary>
        /// <param name="portSet"></param>
        /// <returns></returns>
        public static implicit operator Port<AddDrive>(IRobotScriptingEngineOperations portSet)
        {
            if (portSet == null) return null;
            return (Port<AddDrive>)portSet[typeof(AddDrive)];
        }
        /// <summary>
        /// Implicit Operator for Port of AddDriveDirect
        /// </summary>
        /// <param name="portSet"></param>
        /// <returns></returns>
        public static implicit operator Port<AddDriveDirect>(IRobotScriptingEngineOperations portSet)
        {
            if (portSet == null) return null;
            return (Port<AddDriveDirect>)portSet[typeof(AddDriveDirect)];
        }
        /// <summary>
        /// Implicit Operator for Port of AddPlayScript
        /// </summary>
        /// <param name="portSet"></param>
        /// <returns></returns>
        public static implicit operator Port<AddPlayScript>(IRobotScriptingEngineOperations portSet)
        {
            if (portSet == null) return null;
            return (Port<AddPlayScript>)portSet[typeof(AddPlayScript)];
        }
        /// <summary>
        /// Implicit Operator for Port of AddWaitAngle
        /// </summary>
        /// <param name="portSet"></param>
        /// <returns></returns>
        public static implicit operator Port<AddWaitAngle>(IRobotScriptingEngineOperations portSet)
        {
            if (portSet == null) return null;
            return (Port<AddWaitAngle>)portSet[typeof(AddWaitAngle)];
        }
        /// <summary>
        /// Implicit Operator for Port of AddWaitDistance
        /// </summary>
        /// <param name="portSet"></param>
        /// <returns></returns>
        public static implicit operator Port<AddWaitDistance>(IRobotScriptingEngineOperations portSet)
        {
            if (portSet == null) return null;
            return (Port<AddWaitDistance>)portSet[typeof(AddWaitDistance)];
        }
        /// <summary>
        /// Implicit Operator for Port of AddWaitEvent
        /// </summary>
        /// <param name="portSet"></param>
        /// <returns></returns>
        public static implicit operator Port<AddWaitEvent>(IRobotScriptingEngineOperations portSet)
        {
            if (portSet == null) return null;
            return (Port<AddWaitEvent>)portSet[typeof(AddWaitEvent)];
        }
        /// <summary>
        /// Implicit Operator for Port of AddWaitTime
        /// </summary>
        /// <param name="portSet"></param>
        /// <returns></returns>
        public static implicit operator Port<AddWaitTime>(IRobotScriptingEngineOperations portSet)
        {
            if (portSet == null) return null;
            return (Port<AddWaitTime>)portSet[typeof(AddWaitTime)];
        }
        /// <summary>
        /// Implicit Operator for Port of AddSensors
        /// </summary>
        /// <param name="portSet"></param>
        /// <returns></returns>
        public static implicit operator Port<AddSensors>(IRobotScriptingEngineOperations portSet)
        {
            if (portSet == null) return null;
            return (Port<AddSensors>)portSet[typeof(AddSensors)];
        }
        /// <summary>
        /// Implicit Operator for Port of AddDemo
        /// </summary>
        /// <param name="portSet"></param>
        /// <returns></returns>
        public static implicit operator Port<AddDemo>(IRobotScriptingEngineOperations portSet)
        {
            if (portSet == null) return null;
            return (Port<AddDemo>)portSet[typeof(AddDemo)];
        }
        /// <summary>
        /// Implicit Operator for Port of AddLeds
        /// </summary>
        /// <param name="portSet"></param>
        /// <returns></returns>
        public static implicit operator Port<AddLeds>(IRobotScriptingEngineOperations portSet)
        {
            if (portSet == null) return null;
            return (Port<AddLeds>)portSet[typeof(AddLeds)];
        }
        /// <summary>
        /// Implicit Operator for Port of AddPlaySong
        /// </summary>
        /// <param name="portSet"></param>
        /// <returns></returns>
        public static implicit operator Port<AddPlaySong>(IRobotScriptingEngineOperations portSet)
        {
            if (portSet == null) return null;
            return (Port<AddPlaySong>)portSet[typeof(AddPlaySong)];
        }
        /// <summary>
        /// Implicit Operator for Port of AddQueryList
        /// </summary>
        /// <param name="portSet"></param>
        /// <returns></returns>
        public static implicit operator Port<AddQueryList>(IRobotScriptingEngineOperations portSet)
        {
            if (portSet == null) return null;
            return (Port<AddQueryList>)portSet[typeof(AddQueryList)];
        }
        /// <summary>
        /// Implicit Operator for Port of AddCover
        /// </summary>
        /// <param name="portSet"></param>
        /// <returns></returns>
        public static implicit operator Port<AddCover>(IRobotScriptingEngineOperations portSet)
        {
            if (portSet == null) return null;
            return (Port<AddCover>)portSet[typeof(AddCover)];
        }
        /// <summary>
        /// Implicit Operator for Port of AddDefineSong
        /// </summary>
        /// <param name="portSet"></param>
        /// <returns></returns>
        public static implicit operator Port<AddDefineSong>(IRobotScriptingEngineOperations portSet)
        {
            if (portSet == null) return null;
            return (Port<AddDefineSong>)portSet[typeof(AddDefineSong)];
        }
        /// <summary>
        /// Implicit Operator for Port of AddDigitalOutputs
        /// </summary>
        /// <param name="portSet"></param>
        /// <returns></returns>
        public static implicit operator Port<AddDigitalOutputs>(IRobotScriptingEngineOperations portSet)
        {
            if (portSet == null) return null;
            return (Port<AddDigitalOutputs>)portSet[typeof(AddDigitalOutputs)];
        }
        /// <summary>
        /// Implicit Operator for Port of AddForceSeekingDock
        /// </summary>
        /// <param name="portSet"></param>
        /// <returns></returns>
        public static implicit operator Port<AddForceSeekingDock>(IRobotScriptingEngineOperations portSet)
        {
            if (portSet == null) return null;
            return (Port<AddForceSeekingDock>)portSet[typeof(AddForceSeekingDock)];
        }
        /// <summary>
        /// Implicit Operator for Port of AddPower
        /// </summary>
        /// <param name="portSet"></param>
        /// <returns></returns>
        public static implicit operator Port<AddPower>(IRobotScriptingEngineOperations portSet)
        {
            if (portSet == null) return null;
            return (Port<AddPower>)portSet[typeof(AddPower)];
        }
        /// <summary>
        /// Implicit Operator for Port of AddPWMLowSideDrivers
        /// </summary>
        /// <param name="portSet"></param>
        /// <returns></returns>
        public static implicit operator Port<AddPWMLowSideDrivers>(IRobotScriptingEngineOperations portSet)
        {
            if (portSet == null) return null;
            return (Port<AddPWMLowSideDrivers>)portSet[typeof(AddPWMLowSideDrivers)];
        }
        /// <summary>
        /// Implicit Operator for Port of AddSendIR
        /// </summary>
        /// <param name="portSet"></param>
        /// <returns></returns>
        public static implicit operator Port<AddSendIR>(IRobotScriptingEngineOperations portSet)
        {
            if (portSet == null) return null;
            return (Port<AddSendIR>)portSet[typeof(AddSendIR)];
        }
        /// <summary>
        /// Implicit Operator for Port of AddShowScript
        /// </summary>
        /// <param name="portSet"></param>
        /// <returns></returns>
        public static implicit operator Port<AddShowScript>(IRobotScriptingEngineOperations portSet)
        {
            if (portSet == null) return null;
            return (Port<AddShowScript>)portSet[typeof(AddShowScript)];
        }
        /// <summary>
        /// Implicit Operator for Port of AddSpot
        /// </summary>
        /// <param name="portSet"></param>
        /// <returns></returns>
        public static implicit operator Port<AddSpot>(IRobotScriptingEngineOperations portSet)
        {
            if (portSet == null) return null;
            return (Port<AddSpot>)portSet[typeof(AddSpot)];
        }
        /// <summary>
        /// Implicit Operator for Port of AddStream
        /// </summary>
        /// <param name="portSet"></param>
        /// <returns></returns>
        public static implicit operator Port<AddStream>(IRobotScriptingEngineOperations portSet)
        {
            if (portSet == null) return null;
            return (Port<AddStream>)portSet[typeof(AddStream)];
        }
        /// <summary>
        /// Implicit Operator for Port of AddStreamPauseResume
        /// </summary>
        /// <param name="portSet"></param>
        /// <returns></returns>
        public static implicit operator Port<AddStreamPauseResume>(IRobotScriptingEngineOperations portSet)
        {
            if (portSet == null) return null;
            return (Port<AddStreamPauseResume>)portSet[typeof(AddStreamPauseResume)];
        }

        #endregion
    }


    /// <summary>
    /// IRobotCreateScript Get Operation
    /// </summary>
    public class Get : Get<GetRequestType, PortSet<IRobotScriptingEngineState, Fault>>
    {
    }


    #region Operations: Add iRobot Create Commands

    /// <summary>
    /// Add the Control Command to the scripting engine.
    /// </summary>
    public class AddControl : Insert<irobot.InternalCmdControl, PortSet<DefaultInsertResponseType, Fault>> { }

    /// <summary>
    /// Add the Demo Command to the scripting engine.
    /// </summary>
    public class AddDemo : Insert<create.CmdDemo, PortSet<DefaultInsertResponseType, Fault>> { }

    /// <summary>
    /// Add the Drive Command to the scripting engine.
    /// </summary>
    public class AddDrive : Insert<irobot.CmdDrive, PortSet<DefaultInsertResponseType, Fault>> { }

    /// <summary>
    /// Add the DriveDirect Command to the scripting engine.
    /// </summary>
    public class AddDriveDirect : Insert<create.CmdDriveDirect, PortSet<DefaultInsertResponseType, Fault>> { }

    /// <summary>
    /// Add the Full Command to the scripting engine.
    /// </summary>
    public class AddFull : Insert<irobot.InternalCmdFull, PortSet<DefaultInsertResponseType, Fault>> { }

    /// <summary>
    /// Add the Leds Command to the scripting engine.
    /// </summary>
    public class AddLeds : Insert<irobot.CmdLeds, PortSet<DefaultInsertResponseType, Fault>> { }

    /// <summary>
    /// Add the PlaySong Command to the scripting engine.
    /// </summary>
    public class AddPlaySong : Insert<irobot.CmdPlaySong, PortSet<DefaultInsertResponseType, Fault>> { }

    /// <summary>
    /// Add the PlayScript Command to the scripting engine.
    /// </summary>
    public class AddPlayScript : Insert<create.CmdPlayScript, PortSet<DefaultInsertResponseType, Fault>> { }

    /// <summary>
    /// Add the QueryList Command to the scripting engine.
    /// </summary>
    public class AddQueryList : Insert<create.CmdQueryList, PortSet<DefaultInsertResponseType, Fault>> { }

    /// <summary>
    /// Add the Safe Command to the scripting engine.
    /// </summary>
    public class AddSafe : Insert<irobot.InternalCmdSafe, PortSet<DefaultInsertResponseType, Fault>> { }

    /// <summary>
    /// Add the Sensors Command to the scripting engine.
    /// </summary>
    public class AddSensors : Insert<irobot.CmdSensors, PortSet<DefaultInsertResponseType, Fault>> { }

    /// <summary>
    /// Add the Start Command to the scripting engine.
    /// </summary>
    public class AddStart : Insert<irobot.InternalCmdStart, PortSet<DefaultInsertResponseType, Fault>> { }

    /// <summary>
    /// Add the WaitAngle Command to the scripting engine.
    /// </summary>
    public class AddWaitAngle : Insert<create.CmdWaitAngle, PortSet<DefaultInsertResponseType, Fault>> { }

    /// <summary>
    /// Add the WaitDistance Command to the scripting engine.
    /// </summary>
    public class AddWaitDistance : Insert<create.CmdWaitDistance, PortSet<DefaultInsertResponseType, Fault>> { }

    /// <summary>
    /// Add the WaitEvent Command to the scripting engine.
    /// </summary>
    public class AddWaitEvent : Insert<create.CmdWaitEvent, PortSet<DefaultInsertResponseType, Fault>> { }

    /// <summary>
    /// Add the WaitTime Command to the scripting engine.
    /// </summary>
    public class AddWaitTime : Insert<create.CmdWaitTime, PortSet<DefaultInsertResponseType, Fault>> { }


    /// <summary>
    /// Add the "Cover entire Room" Command to the scripting engine.
    /// </summary>
    public class AddCover : Insert<irobot.CmdClean, PortSet<DefaultInsertResponseType, Fault>> { }

    /// <summary>
    /// Add the DefineSong Command to the scripting engine.
    /// </summary>
    public class AddDefineSong : Insert<irobot.CmdDefineSong, PortSet<DefaultInsertResponseType, Fault>> { }

    /// <summary>
    /// Add the DigitalOutputs Command to the scripting engine.
    /// </summary>
    public class AddDigitalOutputs : Insert<create.CmdDigitalOutputs, PortSet<DefaultInsertResponseType, Fault>> { }

    /// <summary>
    /// Add the ForceSeekingDock Command to the scripting engine.
    /// </summary>
    public class AddForceSeekingDock : Insert<irobot.CmdForceSeekingDock, PortSet<DefaultInsertResponseType, Fault>> { }

    /// <summary>
    /// Add the Power Command to the scripting engine.
    /// </summary>
    public class AddPower : Insert<irobot.InternalCmdPower, PortSet<DefaultInsertResponseType, Fault>> { }

    /// <summary>
    /// Add the PWMLowSideDrivers Command to the scripting engine.
    /// </summary>
    public class AddPWMLowSideDrivers : Insert<create.CmdPWMLowSideDrivers, PortSet<DefaultInsertResponseType, Fault>> { }

    /// <summary>
    /// Add the SendIR Command to the scripting engine.
    /// </summary>
    public class AddSendIR : Insert<create.CmdSendIR, PortSet<DefaultInsertResponseType, Fault>> { }

    /// <summary>
    /// Add the ShowScript Command to the scripting engine.
    /// </summary>
    public class AddShowScript : Insert<create.CmdShowScript, PortSet<DefaultInsertResponseType, Fault>> { }

    /// <summary>
    /// Add the Spot Command to the scripting engine.
    /// </summary>
    public class AddSpot : Insert<irobot.CmdSpot, PortSet<DefaultInsertResponseType, Fault>> { }

    /// <summary>
    /// Add the Stream Command to the scripting engine.
    /// </summary>
    public class AddStream : Insert<create.CmdStream, PortSet<DefaultInsertResponseType, Fault>> { }

    /// <summary>
    /// Add the StreamPauseResume Command to the scripting engine.
    /// </summary>
    public class AddStreamPauseResume : Insert<create.CmdStreamPauseResume, PortSet<DefaultInsertResponseType, Fault>> { }

    /// <summary>
    /// Clear the Current Script
    /// </summary>
    [Description("Clears the current script.")]
    public class ClearCurrentScript : Delete<ClearScript, PortSet<DefaultDeleteResponseType, Fault>>
    {
    }

    /// <summary>
    /// Delete the Named Script
    /// </summary>
    [Description("Delete the named script or 'all' to delete all scripts.")]
    public class DeleteNamedScript : Delete<Script, PortSet<DefaultDeleteResponseType, Fault>>
    {
    }

    /// <summary>
    /// Save the Current Script
    /// <remarks>If no name is specified, the current script name will be maintained.</remarks>
    /// </summary>
    [Description("Saves the current script.")]
    public class SaveCurrentScript : Upsert<Script, PortSet<ScriptDefinition, Fault>>
    {
    }

    /// <summary>
    /// Load the Named Script and make it the Current Script
    /// </summary>
    [Description("Loads a specified script.")]
    public class LoadCurrentScript : Update<Script, PortSet<ScriptDefinition, Fault>>
    {
    }

    #endregion
}


