//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: ScriptGenerator.cs $ $Revision: 13 $
//-----------------------------------------------------------------------

using Microsoft.Ccr.Core;
using Microsoft.Dss.Core;
using Microsoft.Dss.Core.Attributes;
using Microsoft.Dss.ServiceModel.Dssp;
using Microsoft.Dss.ServiceModel.DsspServiceBase;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml;
using irobot = Microsoft.Robotics.Services.IRobot.Roomba;
using create = Microsoft.Robotics.Services.IRobot.Create;
using W3C.Soap;


namespace Microsoft.Robotics.Services.IRobot.Create.ScriptingEngine
{

    /// <summary>
    /// Implementation class for the IRobotScriptingEngine Service
    /// </summary>
    [DisplayName("(User) iRobot� Create Scripting Engine")]
    [Description("Generates scripts for the iRobot Create.")]
    [Contract(Contract.Identifier)]
    public class IRobotScriptingEngineService : DsspServiceBase
    {
        /// <summary>
        /// iRobot Scripting Engine state
        /// </summary>
        [InitialStatePartner(Optional = true, ServiceUri = ServicePaths.Store + "/iRobot.ScriptingEngine.config.xml")]
        private IRobotScriptingEngineState _state = new IRobotScriptingEngineState();

        /// <summary>
        /// _main Port
        /// </summary>
        [ServicePort("/irobot/create/scriptingengine", AllowMultipleInstances=true)]
        private IRobotScriptingEngineOperations _mainPort = new IRobotScriptingEngineOperations();

        /// <summary>
        /// Default Service Constructor
        /// </summary>
        public IRobotScriptingEngineService(DsspServiceCreationPort creationPort) :
                base(creationPort)
        {
        }

        /// <summary>
        /// Start the Service
        /// </summary>
        protected override void Start()
        {
            ValidateState();

            base.Start();
        }

        /// <summary>
        /// Validate the initial state.
        /// </summary>
        private void ValidateState()
        {
            if (_state == null)
            {
                _state = new IRobotScriptingEngineState();
            }

            if (_state.CurrentScript == null)
            {
                _state.CurrentScript = new ScriptDefinition();
                _state.CurrentScript.Commands = new List<string>();
                _state.CurrentScript.PacketData = new byte[1];
                _state.CurrentScript.PacketData[0] = (byte)(_state.CurrentScript.PacketData.Length - 1);
            }

            if (_state.SavedScripts == null)
                _state.SavedScripts = new List<ScriptDefinition>();
        }

        #region Standard Handlers

        /// <summary>
        /// Get Handler
        /// </summary>
        /// <param name="get"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public virtual IEnumerator<ITask> GetHandler(Get get)
        {
            get.ResponsePort.Post(_state);
            yield break;
        }

        #endregion

        #region Script Add Standard Command Handlers


        /// <summary>
        /// AddControl Handler
        /// </summary>
        /// <param name="header"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public virtual IEnumerator<ITask> AddControlHandler(AddControl header)
        {
            AddScriptCommand(header.Body, header.ResponsePort);
            yield break;
        }



        /// <summary>
        /// AddDrive Handler
        /// </summary>
        /// <param name="header"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public virtual IEnumerator<ITask> AddDriveHandler(AddDrive header)
        {
            AddScriptCommand(header.Body, header.ResponsePort);
            yield break;
        }



        /// <summary>
        /// AddFull Handler
        /// </summary>
        /// <param name="header"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public virtual IEnumerator<ITask> AddFullHandler(AddFull header)
        {
            AddScriptCommand(header.Body, header.ResponsePort);
            yield break;
        }


        /// <summary>
        /// AddLeds Handler
        /// </summary>
        /// <param name="header"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public virtual IEnumerator<ITask> AddLedsHandler(AddLeds header)
        {
            AddScriptCommand(header.Body, header.ResponsePort);
            yield break;
        }


        /// <summary>
        /// AddPlaySong Handler
        /// </summary>
        /// <param name="header"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public virtual IEnumerator<ITask> AddPlaySongHandler(AddPlaySong header)
        {
            AddScriptCommand(header.Body, header.ResponsePort);
            yield break;
        }



        /// <summary>
        /// AddSafe Handler
        /// </summary>
        /// <param name="header"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public virtual IEnumerator<ITask> AddSafeHandler(AddSafe header)
        {
            AddScriptCommand(header.Body, header.ResponsePort);
            yield break;
        }


        /// <summary>
        /// AddSensors Handler
        /// </summary>
        /// <param name="header"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public virtual IEnumerator<ITask> AddSensorsHandler(AddSensors header)
        {
            AddScriptCommand(header.Body, header.ResponsePort);
            yield break;
        }



        /// <summary>
        /// AddStart Handler
        /// </summary>
        /// <param name="header"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public virtual IEnumerator<ITask> AddStartHandler(AddStart header)
        {
            AddScriptCommand(header.Body, header.ResponsePort);
            yield break;
        }

        /// <summary>
        /// AddDemo Handler
        /// </summary>
        /// <param name="header"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public virtual IEnumerator<ITask> AddDemoHandler(AddDemo header)
        {
            AddScriptCommand(header.Body, header.ResponsePort);
            yield break;
        }



        /// <summary>
        /// AddDriveDirect Handler
        /// </summary>
        /// <param name="header"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public virtual IEnumerator<ITask> AddDriveDirectHandler(AddDriveDirect header)
        {
            AddScriptCommand(header.Body, header.ResponsePort);
            yield break;
        }


        /// <summary>
        /// AddPlayScript Handler
        /// </summary>
        /// <param name="header"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public virtual IEnumerator<ITask> AddPlayScriptHandler(AddPlayScript header)
        {
            AddScriptCommand(header.Body, header.ResponsePort);
            yield break;
        }



        /// <summary>
        /// AddQueryList Handler
        /// </summary>
        /// <param name="header"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public virtual IEnumerator<ITask> AddQueryListHandler(AddQueryList header)
        {
            AddScriptCommand(header.Body, header.ResponsePort);
            yield break;
        }


        /// <summary>
        /// AddWaitAngle Handler
        /// </summary>
        /// <param name="header"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public virtual IEnumerator<ITask> AddWaitAngleHandler(AddWaitAngle header)
        {
            AddScriptCommand(header.Body, header.ResponsePort);
            yield break;
        }


        /// <summary>
        /// AddWaitDistance Handler
        /// </summary>
        /// <param name="header"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public virtual IEnumerator<ITask> AddWaitDistanceHandler(AddWaitDistance header)
        {
            AddScriptCommand(header.Body, header.ResponsePort);
            yield break;
        }


        /// <summary>
        /// AddWaitEvent Handler
        /// </summary>
        /// <param name="header"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public virtual IEnumerator<ITask> AddWaitEventHandler(AddWaitEvent header)
        {
            AddScriptCommand(header.Body, header.ResponsePort);
            yield break;
        }


        /// <summary>
        /// AddWaitTime Handler
        /// </summary>
        /// <param name="header"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public virtual IEnumerator<ITask> AddWaitTimeHandler(AddWaitTime header)
        {
            AddScriptCommand(header.Body, header.ResponsePort);
            yield break;
        }

        #region Named Command Handling

        /// <summary>
        /// ClearCurrentScript Handler
        /// </summary>
        /// <param name="resetCommandList"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public virtual IEnumerator<ITask> ClearCurrentScriptHandler(ClearCurrentScript resetCommandList)
        {
            ClearCurrentScript();
            resetCommandList.ResponsePort.Post(DefaultDeleteResponseType.Instance);
            yield break;
        }

        /// <summary>
        /// Clear the current script.
        /// </summary>
        private void ClearCurrentScript()
        {
            _state.CurrentScript = new ScriptDefinition();
            _state.CurrentScript.Name = string.Empty;
            _state.CurrentScript.Commands = new List<string>();
            _state.CurrentScript.PacketData = new byte[1];
            _state.CurrentScript.PacketData[0] = (byte)(_state.CurrentScript.PacketData.Length - 1);
            _state.CurrentScript.ExpectedScriptResponseBytes = 0;
        }

        /// <summary>
        /// Save the current script
        /// </summary>
        /// <param name="saveCurrentScript"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public virtual IEnumerator<ITask> SaveCurrentScriptHandler(SaveCurrentScript saveCurrentScript)
        {
            if (!string.IsNullOrEmpty(saveCurrentScript.Body.Name))
                _state.CurrentScript.Name = saveCurrentScript.Body.Name;

            if (string.IsNullOrEmpty(_state.CurrentScript.Name))
            {
                saveCurrentScript.ResponsePort.Post(Fault.FromException(new InvalidOperationException("Save Failed: Script is not named.")));
                yield break;
            }

            int ix = _state.SavedScripts.FindIndex(delegate(ScriptDefinition script) { return saveCurrentScript.Body.Name == script.Name; });
            if (ix >= 0)
            {
                _state.SavedScripts.RemoveAt(ix);
            }

            // Every script should query for data at the end
            // so that we can detect the end of the script.
            if (_state.CurrentScript.ExpectedScriptResponseBytes == 0)
            {
                AddSensors addSensors = new AddSensors();
                addSensors.Body = new irobot.CmdSensors(CreateSensorPacket.AllCreate);
                AddScriptCommand(addSensors.Body, addSensors.ResponsePort);
                yield return Arbiter.Choice(addSensors.ResponsePort,
                    delegate(DefaultInsertResponseType ok) { },
                    delegate(Fault fault) { });
            }

            _state.SavedScripts.Add(_state.CurrentScript);

            // Clear the current script, ready for a new one.
            ClearCurrentScript();

            // Save the state
            SaveState(_state);

            // Success
            saveCurrentScript.ResponsePort.Post(_state.CurrentScript);
        }

        /// <summary>
        /// Delete the named script
        /// </summary>
        /// <param name="deleteNamedScript"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public virtual IEnumerator<ITask> DeleteNamedScriptHandler(DeleteNamedScript deleteNamedScript)
        {
            if (string.IsNullOrEmpty(deleteNamedScript.Body.Name))
            {
                deleteNamedScript.ResponsePort.Post(Fault.FromException(new InvalidOperationException("Delete Failed: Script was not specified.")));
                yield break;
            }
            if (deleteNamedScript.Body.Name.Equals("all", StringComparison.InvariantCultureIgnoreCase))
            {
                _state.SavedScripts = new List<ScriptDefinition>();
            }
            else
            {
                int ix = _state.SavedScripts.FindIndex(delegate(ScriptDefinition script) { return deleteNamedScript.Body.Name == script.Name; });
                if (ix >= 0)
                {
                    _state.SavedScripts.RemoveAt(ix);
                }
            }

            // Save the state
            SaveState(_state);

            // Success
            deleteNamedScript.ResponsePort.Post(DefaultDeleteResponseType.Instance);
        }


        /// <summary>
        /// Load the Named Script and make it the Current Script
        /// </summary>
        /// <param name="loadCurrentScript"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public virtual IEnumerator<ITask> LoadCurrentScriptHandler(LoadCurrentScript loadCurrentScript)
        {
            if (string.IsNullOrEmpty(loadCurrentScript.Body.Name))
            {
                loadCurrentScript.ResponsePort.Post(Fault.FromException(new InvalidOperationException("Load Failed: Script must have a name.")));
                yield break;
            }

            int ix = _state.SavedScripts.FindIndex(delegate(ScriptDefinition script) { return loadCurrentScript.Body.Name == script.Name; });
            if (ix < 0)
            {
                loadCurrentScript.ResponsePort.Post(Fault.FromException(new InvalidOperationException("Load Failed: Script " + loadCurrentScript.Body.Name + " not found.")));
                yield break;
            }

            _state.CurrentScript = _state.SavedScripts[ix];

            // Success
            loadCurrentScript.ResponsePort.Post(_state.CurrentScript);
            yield break;

        }

        #endregion

        /// <summary>
        /// AddCover Handler
        /// </summary>
        /// <param name="header"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public virtual IEnumerator<ITask> AddCoverHandler(AddCover header)
        {
            AddScriptCommand(header.Body, header.ResponsePort);
            yield break;
        }


        /// <summary>
        /// AddDefineSong Handler
        /// </summary>
        /// <param name="header"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public virtual IEnumerator<ITask> AddDefineSongHandler(AddDefineSong header)
        {
            AddScriptCommand(header.Body, header.ResponsePort);
            yield break;
        }


        /// <summary>
        /// AddDigitalOutputs Handler
        /// </summary>
        /// <param name="header"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public virtual IEnumerator<ITask> AddDigitalOutputsHandler(AddDigitalOutputs header)
        {
            AddScriptCommand(header.Body, header.ResponsePort);
            yield break;
        }


        /// <summary>
        /// AddForceSeekingDock Handler
        /// </summary>
        /// <param name="header"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public virtual IEnumerator<ITask> AddForceSeekingDockHandler(AddForceSeekingDock header)
        {
            AddScriptCommand(header.Body, header.ResponsePort);
            yield break;
        }


        /// <summary>
        /// AddPower Handler
        /// </summary>
        /// <param name="header"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public virtual IEnumerator<ITask> AddPowerHandler(AddPower header)
        {
            AddScriptCommand(header.Body, header.ResponsePort);
            yield break;
        }


        /// <summary>
        /// AddPWMLowSideDrivers Handler
        /// </summary>
        /// <param name="header"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public virtual IEnumerator<ITask> AddPWMLowSideDriversHandler(AddPWMLowSideDrivers header)
        {
            AddScriptCommand(header.Body, header.ResponsePort);
            yield break;
        }


        /// <summary>
        /// AddSendIR Handler
        /// </summary>
        /// <param name="header"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public virtual IEnumerator<ITask> AddSendIRHandler(AddSendIR header)
        {
            AddScriptCommand(header.Body, header.ResponsePort);
            yield break;
        }


        /// <summary>
        /// AddShowScript Handler
        /// </summary>
        /// <param name="header"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public virtual IEnumerator<ITask> AddShowScriptHandler(AddShowScript header)
        {
            AddScriptCommand(header.Body, header.ResponsePort);
            yield break;
        }


        /// <summary>
        /// AddSpot Handler
        /// </summary>
        /// <param name="header"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public virtual IEnumerator<ITask> AddSpotHandler(AddSpot header)
        {
            AddScriptCommand(header.Body, header.ResponsePort);
            yield break;
        }


        /// <summary>
        /// AddStream Handler
        /// </summary>
        /// <param name="header"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public virtual IEnumerator<ITask> AddStreamHandler(AddStream header)
        {
            AddScriptCommand(header.Body, header.ResponsePort);
            yield break;
        }


        /// <summary>
        /// AddStreamPauseResume Handler
        /// </summary>
        /// <param name="header"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public virtual IEnumerator<ITask> AddStreamPauseResumeHandler(AddStreamPauseResume header)
        {
            AddScriptCommand(header.Body, header.ResponsePort);
            yield break;
        }

        #endregion


        /// <summary>
        /// Add an iRobot command to our script.
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="responsePort"></param>
        private void AddScriptCommand(Roomba.RoombaCommand cmd, PortSet<DefaultInsertResponseType, Fault> responsePort)
        {
            _state.CurrentScript.Commands.Add(cmd.ToString());
            _state.CurrentScript.PacketData = Roomba.ByteArray.Combine(_state.CurrentScript.PacketData, cmd.GetPacket());
            _state.CurrentScript.PacketData[0] = (byte)(_state.CurrentScript.PacketData.Length - 1);

            _state.CurrentScript.ExpectedScriptResponseBytes += cmd.ExpectedResponseBytes();

            responsePort.Post(DefaultInsertResponseType.Instance);
        }
    }
}
