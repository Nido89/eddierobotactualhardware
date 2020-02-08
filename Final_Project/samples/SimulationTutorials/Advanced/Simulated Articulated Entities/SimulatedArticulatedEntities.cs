//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: SimulatedArticulatedEntities.cs $ $Revision: 5 $
//-----------------------------------------------------------------------

using Microsoft.Ccr.Core;
using Microsoft.Dss.Core;
using Microsoft.Dss.Core.Attributes;
using Microsoft.Dss.ServiceModel.Dssp;
using Microsoft.Dss.ServiceModel.DsspServiceBase;
using System;
using System.Collections.Generic;
using System.Security.Permissions;

#region Simulation namespaces
using simcommonproxy = Microsoft.Robotics.Simulation.Proxy;
using simcommon = Microsoft.Robotics.Simulation;
using Microsoft.Robotics.Simulation.Engine;
using engineproxy = Microsoft.Robotics.Simulation.Engine.Proxy;
using Microsoft.Robotics.Simulation.Physics;
using arm = Microsoft.Robotics.Services.ArticulatedArm.Proxy;
using W3C.Soap;
using Microsoft.Robotics.PhysicalModel;
using physicalmodelproxy = Microsoft.Robotics.PhysicalModel.Proxy;
using System.ComponentModel;
#endregion

namespace Robotics.SimulatedArticulatedEntities
{
    [DisplayName("(User) Simulated Articulated Entities")]
    [Description("Simulated Articulated Entities Service")]
    [Contract(Contract.Identifier)]
    public class SimulatedArticulatedEntities : DsspServiceBase
    {
        // partner attribute will cause simulation engine service to start
        [Partner("Engine",
            Contract = engineproxy.Contract.Identifier,
            CreationPolicy = PartnerCreationPolicy.UseExistingOrCreate)]
        private engineproxy.SimulationEnginePort _engineServicePort = new engineproxy.SimulationEnginePort();

        // port for interacting with the simulated arm service
        arm.ArticulatedArmOperations _armServicePort;

        State _state = new State();

        // Main service port
        [ServicePort("/SimulatedArticulatedEntities", AllowMultipleInstances=false)]
        private SimulationTutorial4Operations _mainPort = new SimulationTutorial4Operations();

        public SimulatedArticulatedEntities(DsspServiceCreationPort creationPort) :
                base(creationPort)
        {
        }

        #region CODECLIP 01-1
        protected override void Start()
        {
            base.Start();
            // Orient sim camera view point
            SetupCamera();
            // Add objects (entities) in our simulated world
            PopulateWorld();
        }

        private void SetupCamera()
        {
            // Set up initial view
            CameraView view = new CameraView();
            view.EyePosition = new Vector3(1.8f, 0.598689f, -1.28f);
            view.LookAtPoint = new Vector3(0, 0, 0.2830455f);
            SimulationEngine.GlobalInstancePort.Update(view);
        }

        private void PopulateWorld()
        {
            AddSky();
            AddGround();
            AddDomino(new Vector3(0.3f, 0, 0.3f), 10);
            SpawnIterator(AddArticulatedArm);
        }
        #endregion

        #region Environment Entities

        void AddSky()
        {
            // Add a sky using a static texture. We will use the sky texture
            // to do per pixel lighting on each simulation visual entity
            SkyDomeEntity sky = new SkyDomeEntity("skydome.dds", "sky_diff.dds");
            SimulationEngine.GlobalInstancePort.Insert(sky);

            // Add a directional light to simulate the sun.
            LightSourceEntity sun = new LightSourceEntity();
            sun.State.Name = "Sun";
            sun.Type = LightSourceEntityType.Directional;
            sun.Color = new Vector4(0.8f, 0.8f, 0.8f, 1);
            sun.Direction = new Vector3(-1.0f, -1.0f, 0.5f);
            SimulationEngine.GlobalInstancePort.Insert(sun);
        }

        void AddGround()
        {
            // create a large horizontal plane, at zero elevation.
            HeightFieldEntity ground = new HeightFieldEntity(
                "simple ground", // name
                "03RamieSc.dds", // texture image
                new MaterialProperties("ground",
                    0.2f, // restitution
                    0.5f, // dynamic friction
                    0.5f) // static friction
                );
            SimulationEngine.GlobalInstancePort.Insert(ground);
        }

        void AddDomino(Vector3 startPosition, int pieces)
        {
            Vector3 dim = new Vector3(0.5f, 1.0f, 0.15f);
            float spacing = 0.45f;
            for (int i = 0; i < pieces; i++)
            {
                Vector3 pos = new Vector3(0,
                    0.02f, i * spacing) + startPosition;

                BoxShapeProperties shape = null;

                shape = new BoxShapeProperties(10f, new Pose(), dim);
                shape.Material = new MaterialProperties("domino", 0.2f, 0.5f, 0.5f);

                SingleShapeEntity entity = new SingleShapeEntity(new BoxShape(shape), pos);
                entity.State.Name = "Domino Box:" + i.ToString();
                SimulationEngine.GlobalInstancePort.Insert(entity);
            }
        }
        #endregion

        #region Robot Entities

        arm.ArticulatedArmState _cachedArmState;

        #region CODECLIP 01-2
        IEnumerator<ITask> AddArticulatedArm()
        {
            Vector3 position = new Vector3(0, 0, 0);

            // Create an instance of our custom arm entity.
            // Source code for this entity can be found under
            // Samples\Simulation\Entities\Entities.cs
            KukaLBR3Entity entity = new KukaLBR3Entity(position);

            // Name the entity
            entity.State.Name = "LBR3Arm";

            // Insert entity in simulation.
            SimulationEngine.GlobalInstancePort.Insert(entity);

            // create simulated arm service
            DsspResponsePort<CreateResponse> resultPort = CreateService(
                Microsoft.Robotics.Services.Simulation.LBR3Arm.Proxy.Contract.Identifier,
                Microsoft.Robotics.Simulation.Partners.CreateEntityPartner("http://localhost/" + entity.State.Name));

            // asynchronously handle service creation result.
            yield return Arbiter.Choice(resultPort,
                delegate(CreateResponse rsp)
                {
                    _armServicePort = ServiceForwarder<arm.ArticulatedArmOperations>(rsp.Service);
                },
                delegate(Fault fault)
                {
                    LogError(fault);
                });

            if (_armServicePort == null)
                yield break;

            // we re-issue the get until we get a response with a fully initialized state
            do
            {
                yield return Arbiter.Receive(false, TimeoutPort(1000), delegate(DateTime signal) { });

                yield return Arbiter.Choice(
                    _armServicePort.Get(new GetRequestType()),
                    delegate(arm.ArticulatedArmState state)
                    {
                        _cachedArmState = state;
                    },
                    delegate(Fault f)
                    {
                        LogError(f);
                    });

                // exit on error
                if (_cachedArmState == null)
                    yield break;
            } while (_cachedArmState.Joints == null);

            // Start a timer that will move the arm in a loop
            // Comment out the line below if you want to control
            // the arm through SimpleDashboard
            // Spawn<DateTime>(DateTime.Now, MoveArm);
        }
        #endregion

        float _angleInRadians = 0;

        #region CODECLIP 01-3
        void MoveArm(DateTime signal)
        {
            _angleInRadians += 0.005f;
            float angle = (float)Math.Sin(_angleInRadians);

            // Create set pose operation. Notice we have specified
            // null for the response port to eliminate a roundtrip
            arm.SetJointTargetPose setPose = new arm.SetJointTargetPose(
                new arm.SetJointTargetPoseRequest(),
                null);

            // specify by name, which joint to orient
            setPose.Body.JointName = _cachedArmState.Joints[0].State.Name;
            setPose.Body.TargetOrientation = new physicalmodelproxy.AxisAngle(
                new physicalmodelproxy.Vector3(1, 0, 0), angle);

            // issue request to arm service for joint0 move.
            _armServicePort.Post(setPose);

            // now move other joints.
            setPose.Body.JointName = _cachedArmState.Joints[1].State.Name;
            _armServicePort.Post(setPose);
            setPose.Body.JointName = _cachedArmState.Joints[2].State.Name;
            _armServicePort.Post(setPose);
            setPose.Body.JointName = _cachedArmState.Joints[3].State.Name;
            _armServicePort.Post(setPose);
            setPose.Body.JointName = _cachedArmState.Joints[4].State.Name;
            _armServicePort.Post(setPose);

            // re- issue timer request so we wake up again
            Activate(Arbiter.Receive(false, TimeoutPort(15), MoveArm));
        }
        #endregion

        #endregion
    }

    public static class Contract
    {
        public const string Identifier = "http://schemas.microsoft.com/2009/03/simulationarticulatedentities.user.html";
    }

    [DataContract]
    public class State
    {
        string _activeJoint;

        [DataMember]
        public string ActiveJoint
        {
            get { return _activeJoint; }
            set { _activeJoint = value; }
        }
    }

    [ServicePort]
    public class SimulationTutorial4Operations : PortSet<DsspDefaultLookup, DsspDefaultDrop, Get, Replace>
    {
    }

    public class Get : Get<GetRequestType, PortSet<State, Fault>>
    {
    }

    public class Replace : Replace<State, PortSet<DefaultReplaceResponseType, Fault>>
    {
    }
}
