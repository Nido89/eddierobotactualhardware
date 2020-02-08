//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: SimulatedFourByFourDrive.cs $ $Revision: 7 $
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

using pm = Microsoft.Robotics.PhysicalModel;
using sim = Microsoft.Robotics.Simulation;
using sime = Microsoft.Robotics.Simulation.Engine;
using simep = Microsoft.Robotics.Simulation.Engine.Proxy;
using phys = Microsoft.Robotics.Simulation.Physics;

namespace Microsoft.Robotics.Services.Samples.SimulatedFourByFourDrive
{
    /// <summary>
    /// A simulated four wheel drive service for simulating off-road vehicles
    /// </summary>
    [DisplayName("(User) Simulated Four By Four Drive Service")]
    [Description("A Simulated Four wheel drive service")]
    [DssCategory(sim.PublishedCategories.SimulationService)]
    [Contract(Contract.Identifier)]
    [DssServiceDescription("http://msdn.microsoft.com/library/cc998495.aspx")]
    public class SimulatedFourByFourDriveService : DsspServiceBase
    {
        private SimulatedFourByFourState _state = new SimulatedFourByFourState();

        [ServicePort("/simfourbyfourdrive", AllowMultipleInstances=true)]
        private SimToy5Operations _mainPort = new SimToy5Operations();

        sime.SimulationEnginePort _simEngine = null;
        FourWheelDriveEntity _entity = null;
        sime.SimulationEnginePort _simNotify = new sime.SimulationEnginePort();

        [EmbeddedResource("Microsoft.Robotics.Services.Samples.SimulatedFourByFourDrive.Models.4x4Body.obj")]
        string _modelFile = string.Empty;

        /// <summary>
        /// SimulatedFourByFourDriveService constructor
        /// </summary>
        /// <param name="creationPort"></param>
        public SimulatedFourByFourDriveService(DsspServiceCreationPort creationPort)
            : base(creationPort)
        {
        }

        /// <summary>
        /// SimulatedFourByFourDriveService start gets called when service initializes
        /// </summary>
        protected override void Start()
        {
            _simEngine = sime.SimulationEngine.GlobalInstancePort;
			base.Start();

            _simEngine.Subscribe(ServiceInfo.PartnerList, _simNotify);


            //
            // don't start service handlers until we have found the entity. 
            // however, do insert into the directory.
            //
            Activate(
                new Interleave(
                    new TeardownReceiverGroup(
                        Arbiter.Receive<sime.InsertSimulationEntity>(false, _simNotify, OnFoundEntity),
                        Arbiter.Receive<DsspDefaultDrop>(false, _mainPort, DefaultDropHandler)
                    ),
                    new ExclusiveReceiverGroup(),
                    new ConcurrentReceiverGroup()
                )
            );

            DirectoryInsert();
        }

        void OnFoundEntity(sime.InsertSimulationEntity insert)
        {
            OnInsertEntity(insert);

            MainPortInterleave.CombineWith(
                new Interleave(
                    new TeardownReceiverGroup(),
                    new ExclusiveReceiverGroup(
                        Arbiter.Receive<sime.InsertSimulationEntity>(true, _simNotify, OnInsertEntity),
                        Arbiter.Receive<sime.DeleteSimulationEntity>(true, _simNotify, OnDeleteEntity)
                    ),
                    new ConcurrentReceiverGroup()
                )
            );
        }

        void OnInsertEntity(sime.InsertSimulationEntity insert)
        {
            _entity = insert.Body as FourWheelDriveEntity;
            if (_entity != null)
            {
                _entity.ServiceContract = Contract.Identifier;

                if (_entity.ChassisShape != null)
                {
                    _state.DistanceBetweenWheels = _entity.DistanceBetweenWheels;
                    _state.WheelBase = _entity.WheelBase;
                }
            }
        }

        void OnDeleteEntity(sime.DeleteSimulationEntity delete)
        {
            _entity = null;
        }

        /// <summary>
        /// Gets SimulatedFourByFourDriveService service state
        /// </summary>
        /// <param name="get"></param>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public void OnGet(Get get)
        {
            get.ResponsePort.Post(_state);
        }

        /// <summary>
        /// SimulatedFourByFourDriveService handler that processes a drive message
        /// </summary>
        /// <param name="drive"></param>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public void OnDrive(Drive drive)
        {
            _state.DriveRequest = drive.Body;


            if (Math.Abs(drive.Body.SteeringAngle) >= 1)
            {
                double angle = -drive.Body.SteeringAngle * Math.PI / 180.0;
                bool left = true;
                if (angle < 0)
                {
                    left = false;
                    angle *= -1;
                }
                double centerRearRadius = _state.WheelBase / Math.Tan(angle);

                double innerRearRadius = centerRearRadius - _state.DistanceBetweenWheels / 2;
                double outerRearRadius = centerRearRadius + _state.DistanceBetweenWheels / 2;

                double innerFrontRadius = Math.Sqrt(_state.WheelBase * _state.WheelBase + innerRearRadius * innerRearRadius);
                double outerFrontRadius = Math.Sqrt(_state.WheelBase * _state.WheelBase + outerRearRadius * outerRearRadius);

                double innerFrontAngle = Math.Atan2(_state.WheelBase, innerRearRadius);
                double outerFrontAngle = Math.Atan2(_state.WheelBase, outerRearRadius);

                double innerFrontSpeed = drive.Body.Power * innerFrontRadius / centerRearRadius;
                double innerRearSpeed = drive.Body.Power * innerRearRadius / centerRearRadius;
                double outerFrontSpeed = drive.Body.Power * outerFrontRadius / centerRearRadius;
                double outerRearSpeed = drive.Body.Power * outerRearRadius / centerRearRadius;

                if (left)
                {
                    _entity.SetWheelAngles((float)innerFrontAngle, (float)outerFrontAngle, 0, 0);
                    _entity.SetMotorTorque((float)innerFrontSpeed, (float)outerFrontSpeed, (float)innerRearSpeed, (float)outerRearSpeed);
                }
                else
                {
                    _entity.SetWheelAngles((float)-outerFrontAngle, (float)-innerFrontAngle, 0, 0);
                    _entity.SetMotorTorque((float)outerFrontSpeed, (float)innerFrontSpeed, (float)outerRearSpeed, (float)innerRearSpeed);
                }

                //double tan = Math.Tan(angle);
                //double radius = _state.WheelBase / tan;

                //double leftRadius = radius - _state.DistanceBetweenWheels / 2;
                //double rightRadius = radius + _state.DistanceBetweenWheels / 2;

                //double leftAngle = Math.Atan2(_state.WheelBase, leftRadius);
                //double rightAngle = Math.Atan2(_state.WheelBase, rightRadius);

                //double leftPower = drive.Body.Power * leftRadius / radius;
                //double rightPower = drive.Body.Power * rightRadius / radius;

                //if (left)
                //{
                //    _entity.SetWheelAngles((float)leftAngle, (float)rightAngle, 0, 0);
                //    _entity.SetMotorTorque((float)leftPower, (float)rightPower);
                //}
                //else
                //{
                //    _entity.SetWheelAngles((float)-rightAngle, (float)-leftAngle, 0, 0);
                //    _entity.SetMotorTorque((float)rightPower, (float)leftPower);
                //}
            }
            else
            {
                _entity.SetWheelAngles(0, 0, 0, 0);
                _entity.SetMotorTorque(drive.Body.Power, drive.Body.Power);
            }

            drive.ResponsePort.Post(DefaultUpdateResponseType.Instance);
        }
    }
}
