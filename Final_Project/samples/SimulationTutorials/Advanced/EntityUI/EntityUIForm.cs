//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: EntityUIForm.cs $ $Revision: 4 $
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Microsoft.Robotics.Simulation.Engine;
using Robotics.SimpleSimulatedRobot;
using Robotics.CustomSimulatedEntities;
using Microsoft.Robotics.Simulation.Physics;
using Microsoft.Robotics.PhysicalModel;
using Microsoft.Ccr.Core;
using W3C.Soap;
using Microsoft.Robotics.Simulation;

namespace Robotics.EntityUI
{
    public partial class EntityUIForm : Form
    {
        DispatcherQueue _dispatcherQueue;
        
        public EntityUIForm(DispatcherQueue dispatcherQueue)
        {
            InitializeComponent();

            _dispatcherQueue = dispatcherQueue;
        }
        enum Entities
        {
            HeightFieldEntity,
            SkyDomeEntity,
            LightSourceEntity,
            MotorBaseWithDrive,
            SimulatedBrightnessSensorEntity,
            SimulatedColorSensorEntity,
            SimulatedCompassEntity,
            SimulatedIREntity,
            SimulatedLRFEntity,
            SimulatedSonarEntity,
            SimulatedWebcamEntity,
            SimulatedGPSEntity
        }
        private void EntityUIForm_Load(object sender, EventArgs e)
        {
        }

        private VisualEntity CreateEntity(Vector3 entityPosition, VisualEntity entityParent, Entities entityType)
        {
            VisualEntity e = null;

            #region Create Entity
            switch (entityType)
            {
                case Entities.HeightFieldEntity:
                    {
                        e = new HeightFieldEntity("ground", "03RamieSc.dds", new
                            MaterialProperties("ground", 0.8f, 0.5f, 0.8f));

                    } break;
                case Entities.SkyDomeEntity:
                    {
                        e = new SkyDomeEntity("skydome.dds", "sky_diff.dds");
                        SimulationEngine.GlobalInstancePort.Insert(e);
                    } break;
                case Entities.LightSourceEntity:
                    {
                        var lightEntity = new LightSourceEntity();
                        e = lightEntity;
                        lightEntity.State.Name = "LightSourceEntity";
                        lightEntity.Type = LightSourceEntityType.Directional;
                        lightEntity.Color = new Vector4(.8f, .8f, .8f, 1);
                        lightEntity.State.Pose.Position = new Vector3(0, 1, 0);
                        lightEntity.Direction = new Vector3(0, -1, 0);
                        e.Flags = e.Flags | VisualEntityProperties.DisableRendering;

                    } break;
                case Entities.MotorBaseWithDrive:
                    {
                        e = new MotorBaseWithDrive(entityPosition);
                        e.State.Name = "MotorBaseWithDrive";
                        MakeNameUnique(e);
                    } break;
                case Entities.SimulatedBrightnessSensorEntity:
                    {
                        if (entityParent == null)
                        {
                            ShowEntityRequiresParentError(entityType.ToString() + " requires parent");
                        }
                        else
                        {
                            e = new SimulatedBrightnessSensorEntity(32, 32, 2.0f * (float)Math.PI / 180.0f);
                            e.State.Name = "SimulatedBrightnessSensorEntity";
                            MakeNameUnique(e);
                            entityParent.InsertEntity(e);
                        }

                    } break;
                case Entities.SimulatedColorSensorEntity:
                    {
                        if (entityParent == null)
                        {
                            ShowEntityRequiresParentError(entityType.ToString() + " requires parent");
                        }
                        else
                        {
                            e = new SimulatedColorSensorEntity(32, 32, 2.0f * (float)Math.PI / 180.0f);
                            e.State.Name = "SimulatedColorSensorEntity";
                            MakeNameUnique(e);
                            entityParent.InsertEntity(e);
                        }

                    } break;
                case Entities.SimulatedCompassEntity:
                    {
                        if (entityParent == null)
                        {
                            ShowEntityRequiresParentError(entityType.ToString() + " requires parent");
                        }

                        else
                        {
                            e = new SimulatedCompassEntity();
                            e.State.Name = "SimulatedCompassEntity";
                            MakeNameUnique(e);
                            entityParent.InsertEntity(e);
                        }

                    } break;

                case Entities.SimulatedGPSEntity:
                    {
                        if (entityParent == null)
                        {
                            ShowEntityRequiresParentError(entityType.ToString() + " requires parent");
                        }

                        else
                        {
                            e = new SimulatedGPSEntity();
                            e.State.Name = "SimulatedGPSEntity";
                            MakeNameUnique(e);
                            entityParent.InsertEntity(e);
                        }

                    } break;

                case Entities.SimulatedIREntity:
                    {
                        if (entityParent == null)
                        {
                            ShowEntityRequiresParentError(entityType.ToString() + " requires parent");
                        }

                        else
                        {
                            e = new SimulatedIREntity(new Pose(entityPosition));
                            e.State.Name = "SimulatedIREntity";
                            e.State.Flags |= EntitySimulationModifiers.DisableCollisions;
                            MakeNameUnique(e);
                            entityParent.InsertEntity(e);
                        }

                    } break;
                case Entities.SimulatedLRFEntity:
                    {
                        if (entityParent == null)
                        {
                            ShowEntityRequiresParentError(entityType.ToString() + " requires parent");
                        }
                        else
                        {
                            e = new SimulatedLRFEntity(new Pose(entityPosition));
                            e.State.Name = "SimulatedLRFEntity";
                            e.State.Flags |= EntitySimulationModifiers.DisableCollisions;
                            MakeNameUnique(e);
                            entityParent.InsertEntity(e);
                        }

                    } break;
                case Entities.SimulatedSonarEntity:
                    {
                        if (entityParent == null)
                        {
                            ShowEntityRequiresParentError(entityType.ToString() + " requires parent");
                        }
                        else
                        {
                            e = new SimulatedSonarEntity(new Pose(entityPosition));
                            e.State.Name = "SimulatedSonarEntity";
                            e.State.Flags |= EntitySimulationModifiers.DisableCollisions;
                            MakeNameUnique(e);
                            entityParent.InsertEntity(e);
                        }

                    } break;
                case Entities.SimulatedWebcamEntity:
                    {
                        if (entityParent == null)
                        {
                            ShowEntityRequiresParentError(entityType.ToString() + " requires parent");
                        }
                        else
                        {
                            e = new SimulatedWebcamEntity(entityPosition,
                                320, 240, 45.0f * (float)Math.PI / 180.0f);
                            e.State.Name = "SimulatedWebcamEntity";
                            MakeNameUnique(e);
                            entityParent.InsertEntity(e);
                        }

                    } break;
            }
            #endregion

            return e;
        }

        private void MakeNameUnique(VisualEntity entity)
        {
            int append = 1;
            string baseName = entity.State.Name;
            while (SimulationEngine.GlobalInstance.IsEntityNameInUse(entity.State.Name))
            {
                entity.State.Name = baseName + append;
                ++append;
            }
        }

        private static void ShowEntityRequiresParentError(string errorMessage)
        {
            MessageBox.Show(errorMessage, errorMessage);
        }

        private Vector3 ParseEntityPosition()
        {
            string[] xyz = _entityPositionTxt.Text.Split(new[] { ' ', ',', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            if (xyz.Length != 3)
            {
                return new Vector3(0, 0, 0);
            }
            else
            {
                try
                {
                    return new Vector3(Single.Parse(xyz[0]),
                        Single.Parse(xyz[1]),
                        Single.Parse(xyz[2]));
                }
                catch
                {
                    return new Vector3(0, 0, 0);
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                var heightfield = CreateEntity(new Vector3(), null, Entities.HeightFieldEntity);
                var light = CreateEntity(new Vector3(), null, Entities.LightSourceEntity);
                var sky = CreateEntity(new Vector3(), null, Entities.SkyDomeEntity);

                SimulationEngine.GlobalInstancePort.Insert(heightfield);
                SimulationEngine.GlobalInstancePort.Insert(light);
                SimulationEngine.GlobalInstancePort.Insert(sky);
            }
            catch
            {
            }
        }

        private void _addMotorBaseBtn_Click(object sender, EventArgs e)
        {
            try
            {
                var motorBase = CreateEntity(ParseEntityPosition(), null, Entities.MotorBaseWithDrive);

                if(_webcamCheckBox.Checked)
                    CreateEntity(new Vector3(0, 0.5f, 0), motorBase, Entities.SimulatedWebcamEntity);

                if(_lightCheckBox.Checked)
                    CreateEntity(new Vector3(0, 0.5f, 0), motorBase, Entities.SimulatedBrightnessSensorEntity);

                if(_colorCheckBox.Checked)
                    CreateEntity(new Vector3(0, 0.5f, 0), motorBase, Entities.SimulatedColorSensorEntity);

                if(_sonarCheckBox.Checked)
                    CreateEntity(new Vector3(0, 0.3f, 0), motorBase, Entities.SimulatedSonarEntity);

                if(_lrfCheckBox.Checked)
                    CreateEntity(new Vector3(0, 0.3f, 0), motorBase, Entities.SimulatedLRFEntity);

                if(_irCheckBox.Checked)
                    CreateEntity(new Vector3(0, 0.3f, 0), motorBase, Entities.SimulatedIREntity);

                if(_compassCheckBox.Checked)
                    CreateEntity(new Vector3(0, 0.3f, 0), motorBase, Entities.SimulatedCompassEntity);

                if(_gpsCheckBox.Checked)
                    CreateEntity(new Vector3(0, 0.3f, 0), motorBase, Entities.SimulatedGPSEntity);

                SimulationEngine.GlobalInstancePort.Insert(motorBase);
            }
            catch
            {
            }
        }
    }
}
