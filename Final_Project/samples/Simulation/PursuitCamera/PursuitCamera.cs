//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: PursuitCamera.cs $ $Revision: 5 $
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml;

using Microsoft.Ccr.Core;
using Microsoft.Dss.Core;
using Microsoft.Dss.Core.Attributes;
using Microsoft.Dss.ServiceModel.Dssp;
using Microsoft.Dss.ServiceModel.DsspServiceBase;

using W3C.Soap;

using sime = Microsoft.Robotics.Simulation.Engine;
using simep = Microsoft.Robotics.Simulation.Engine.Proxy;
using physics = Microsoft.Robotics.Simulation.Physics;
using xna = Microsoft.Xna.Framework;

namespace Microsoft.Robotics.Services.PursuitCamera
{
    /// <summary>
    /// Implements a Pursuit Camera for Simulation
    /// </summary>
    [DisplayName("(User) Pursuit Camera")]
    [Contract(Contract.Identifier)]
    [DssServiceDescription("http://msdn.microsoft.com/library/cc998478.aspx")]
    public class PursuitCameraService : DsspServiceBase
    {
        [InitialStatePartner(Optional = true)]
        private PursuitCameraState _state = new PursuitCameraState();

        [ServicePort("/pursuitcamera", AllowMultipleInstances=false)]
        private PursuitCameraOperations _mainPort = new PursuitCameraOperations();

        [Partner("SimulationEngine", Contract = simep.Contract.Identifier, CreationPolicy = PartnerCreationPolicy.UseExistingOrCreate)]
        simep.SimulationEnginePort _simEngineProxy = new simep.SimulationEnginePort();

        sime.SimulationEnginePort _simPort;
        sime.SimulationEnginePort _simNotify = new sime.SimulationEnginePort();
        sime.VisualEntity _entity;
        Entities.PursuitCamera.PursuitCameraEntity _camera;

        physics.PhysicsEngine _physics;
        
        /// <summary>
        /// PursuitCamera constructor that takes a PortSet used to communicate when the
        /// camera is created
        /// </summary>
        /// <param name="creationPort"></param>
        public PursuitCameraService(DsspServiceCreationPort creationPort) : 
                base(creationPort)
        {
        }
        
        /// <summary>
        /// PursuitCamera Start is called when service initializes
        /// </summary>
        protected override void Start()
        {
            if (_state == null)
            {
                _state = new PursuitCameraState
                {
                    MinDistance = 4,
                    MaxDistance = 6,
                    FieldOfView = 45,
                    Altitude = 2,
                    OcclusionThreshold = 0.5f,
                    PreventOcclusion = true,
                    CameraName = "PursuitCamera"
                };
            }

			base.Start();

            _physics = physics.PhysicsEngine.GlobalInstance;
            _simPort = sime.SimulationEngine.GlobalInstancePort;
            
            _simPort.Subscribe(ServiceInfo.PartnerList, _simNotify);

            base.MainPortInterleave.CombineWith(
                Arbiter.Interleave(
                    new TeardownReceiverGroup(),
                    new ExclusiveReceiverGroup(
                        Arbiter.ReceiveWithIterator<sime.InsertSimulationEntity>(true, _simNotify, OnInsertEntity),
                        Arbiter.Receive<sime.DeleteSimulationEntity>(true, _simNotify, OnDeleteEntity)
                    ),
                    new ConcurrentReceiverGroup()
                )
            );
        }

        /// <summary>
        /// Get PursuitCamera state
        /// </summary>
        /// <param name="get"></param>
        [ServiceHandler]
        public void OnGet(Get get)
        {
            get.ResponsePort.Post(_state);
        }

        IEnumerator<ITask> OnInsertEntity(sime.InsertSimulationEntity insert)
        {
            _entity = insert.Body;

            var query = new sime.VisualEntity();
            query.State.Name = _state.CameraName;

            yield return Arbiter.Choice(
                _simPort.Query(query),
                success => _camera = success.Entity as Entities.PursuitCamera.PursuitCameraEntity,
                failure => LogError("Unable to find camera", failure)
            );

            if (_camera == null)
            {
                _entity = null;
                yield break;
            }

            SetCameraProperties();
            _camera.Target = _entity;
        }

        private void SetCameraProperties()
        {
            _camera.TargetName = _entity.State.Name;

            _camera.MinDistance = _state.MinDistance;
            _camera.MaxDistance = _state.MaxDistance;
            _camera.FieldOfView = _state.FieldOfView;
            _camera.OcclusionThreshold = _state.OcclusionThreshold;
            _camera.PreventOcclusion = _state.PreventOcclusion;

            _camera.Altitude = _state.Altitude;
        }

        void OnDeleteEntity(sime.DeleteSimulationEntity delete)
        {
            _entity = null;
            _camera = null;
        }

        /// <summary>
        /// Handler that processes messages that change settings in the PursuitCamera entity
        /// </summary>
        /// <param name="update"></param>
        [ServiceHandler]
        public void OnChangeSettings(ChangeSettings update)
        {
            _state.settings = update.Body;
            update.ResponsePort.Post(DefaultUpdateResponseType.Instance);

            if (_camera != null)
            {
                SetCameraProperties();
            }
        }
    }
}
