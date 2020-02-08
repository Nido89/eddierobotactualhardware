// #define USE_CONCURRENT_TEXTURE_READ_BACKS

//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: Entities.cs $ $Revision: 258 $
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text;
using System.Threading;
using xna = Microsoft.Xna.Framework;
using xnagrfx = Microsoft.Xna.Framework.Graphics;
using xnaprof = Microsoft.Robotics.Simulation.MeshLoader;
using Microsoft.Dss.Core.Attributes;
using Microsoft.Dss.Core;
using Microsoft.Ccr.Core;
using Microsoft.Robotics.Simulation.Physics;
using Microsoft.Robotics.PhysicalModel;
using System.Runtime.InteropServices;

using System.IO;
using System.Drawing.Imaging;
using System.Drawing;
using System.Windows.Forms;

namespace Microsoft.Robotics.Simulation.Engine
{
    #region Sensors

    /// <summary>
    /// Models an array of contact sensors
    /// </summary>
    [DataContract]
    [CLSCompliant(true)]
    [EditorAttribute(typeof(GenericObjectEditor), typeof(System.Drawing.Design.UITypeEditor))]
    public class BumperArrayEntity : VisualEntity
    {
        Port<EntityContactNotification> _notifications = new Port<EntityContactNotification>();

        BoxShape[] _shapes;

        /// <summary>
        /// Shapes for each bumper
        /// </summary>
        [DataMember]
        [Description("Contains the shapes which represent the bumpers.")]
        public BoxShape[] Shapes
        {
            get { return _shapes; }
            set { _shapes = value; }
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public BumperArrayEntity() { }

        /// <summary>
        /// Initialization constructor
        /// </summary>
        /// <param name="shapes">Shapes to use to represent each bumper</param>
        public BumperArrayEntity(params BoxShape[] shapes)
        {
            _shapes = shapes;
        }

        /// <summary>
        /// Initialization
        /// </summary>
        /// <param name="device"></param>
        /// <param name="physicsEngine"></param>
        public override void Initialize(xnagrfx.GraphicsDevice device, PhysicsEngine physicsEngine)
        {
            try
            {
                InitError = string.Empty;

                foreach (BoxShape b in _shapes)
                {
                    State.PhysicsPrimitives.Add(b);
                    b.BoxState.EnableContactNotifications = true;
                }

                base.Initialize(device, physicsEngine);

                if (Parent == null)
                    throw new Exception("This entity must be a child of another entity.");


                CreateAndInsertPhysicsEntity(physicsEngine);

                Flags |= VisualEntityProperties.DisableRendering;

                foreach (BoxShape shape in _shapes)
                    AddShapeToPhysicsEntity(shape, null);
            }
            catch (Exception ex)
            {
                HasBeenInitialized = false;
                InitError = ex.ToString();
            }
        }

        /// <summary>
        /// Adds a notification port to the list of subscriptions that get notified when the bumper shapes
        /// collide in the physics world
        /// </summary>
        /// <param name="notificationTarget"></param>
        public void Subscribe(Port<EntityContactNotification> notificationTarget)
        {
            // the parent has the physics entity create (we are just part of that entity)
            // so subscribe there.
            PhysicsEntity.SubscribeForContacts(notificationTarget);
        }
    }

    /// <summary>
    /// Models a camera sensor
    /// </summary>
    [DataContract]
    [CLSCompliant(true)]
    [EditorAttribute(typeof(GenericObjectEditor), typeof(System.Drawing.Design.UITypeEditor))]
    public class CameraEntity : VisualEntity
    {
        // used to indicate the entity is being disposed so that additional
        // CaptureScene requests are not enqueued.
        bool _isDisposing = false;

        // this field is not public, to ensure CLS compliancy
        // its calls can be done to the containing VisualEntity
        // that redirects them to this private field
        Camera _frameworkCamera;

        private bool _isRealTimeCamera;

        /// <summary>
        /// If true the camera is a realtime camera and renders at each frame
        /// </summary>
        [DataMember]
        [Description("Indicates whether the camera is rendered each frame.")]
        public bool IsRealTimeCamera
        {
            get { return _isRealTimeCamera; }
            set { _isRealTimeCamera = value; }
        }

        private List<PictureBox> _surfaces = new List<PictureBox>();

        internal bool ActsLikeRealTimeCamera
        {
            get { return (_isRealTimeCamera || ((_surfaces != null) && (_surfaces.Count > 0))) && (WriteSurface != null) && (ReadSurface != null); }
        }

        internal void EnableRealTimeRendering()
        {
            DeviceReset(Device);
        }

        /// <summary>
        /// Add a PictureBox container to display the results of each render from this camera.
        /// </summary>
        /// <param name="surface"></param>
        public void AddSurface(PictureBox surface)
        {
            if (!_surfaces.Contains(surface))
            {
                _surfaces.Add(surface);
            }
        }

        /// <summary>
        /// Disassociate a PictureBox container from this camera.
        /// </summary>
        /// <param name="surface"></param>
        public void RemoveSurface(PictureBox surface)
        {
            _surfaces.Remove(surface);
        }

        internal bool PlaybackMode = false;

        private int _updateInterval = 67;   // default rate is about 15 fps
        /// <summary>
        /// This time indicates the amount of time between frame updates in milliseconds.
        /// 0 means that the camera never updates on its own but must be queried.
        /// </summary>
        [DataMember]
        [Description("The amount of time between frames in milliseconds.  After the time period expires, a notification is sent indicating a new frame.")]
        public int UpdateInterval
        {
            get { return _updateInterval; }
            set { _updateInterval = value; }
        }

        /// <summary>
        /// Flags to enable or disable shadow rendering by the camera
        /// </summary>
        [DataContract]
        public enum ShadowDisplayMode : int
        {
            /// <summary>Special value so that we don't get an invalid value from proxy objects</summary>
            [Browsable(false)]
            Invalid = 0,

            /// <summary>Camera should show shadows</summary>
            ShowShadows,

            /// <summary>Camera should not show shadows</summary>
            HideShadows,
        }
        private ShadowDisplayMode _shadowDisplay = ShadowDisplayMode.ShowShadows;
        /// <summary>
        /// Gets or sets whether the camera should be rendering shadows
        /// </summary>
        [DataMember]
        [Category("Rendering"), Description("Gets or sets whether the camera should be rendering shadows")]
        [DisplayName("(User) Shadow display")]
        public ShadowDisplayMode ShadowDisplay
        {
            get { return _shadowDisplay; }
            set
            {
                if (value == ShadowDisplayMode.Invalid)
                    _shadowDisplay = ShadowDisplayMode.ShowShadows;
                else
                    _shadowDisplay = value;
            }
        }

        /// <summary>
        /// Flags to specify the camera model
        /// </summary>
        [DataContract]
        public enum CameraModelType : int
        {
            /// <summary>
            /// Main camera.  Uses a location and look-at point to specify the position and orientation.
            /// Moves only in Pitch and Yaw.  Up is always oriented in the +Y direction.
            /// </summary>
            FirstPerson,
            /// <summary>
            /// A camera attached to another object.  Uses the pose of the camera entity to define its position and orientation.
            /// Moves in Pitch, Yaw, and Roll.  Up depends on the orientation of the camera entity.
            /// </summary>
            AttachedChild
        }

        private CameraModelType _cameraModel;
        /// <summary>
        /// Specifies the model used by the camera entity.
        /// </summary>
        [DataMember]
        public CameraModelType CameraModel
        {
            get { return _cameraModel; }
            set
            {
                _cameraModel = value;
                if (HasBeenInitialized)
                {
                    _initFromPose = true;
                    InitializeCameraModel();
                    SetProjectionParameters(_viewAngle, _aspect, _near, _far, _viewSizeX, _viewSizeY);
                }
            }
        }

        private float _near;
        private float _far;

        /// <summary>
        /// Distance to the near clipping plane
        /// </summary>
        [Category("Projection Parameters")]
        [DataMember]
        public float Near
        {
            get { return _near; }
            set
            {
                _near = value;
                if (HasBeenInitialized)
                    SetProjectionParameters(_viewAngle, _aspect, value, _far, _viewSizeX, _viewSizeY);
            }
        }

        /// <summary>
        /// Distance to the far clipping plane
        /// </summary>
        [Category("Projection Parameters")]
        [DataMember]
        public float Far
        {
            get { return _far; }
            set
            {
                _far = value;
                if (HasBeenInitialized)
                    SetProjectionParameters(_viewAngle, _aspect, _near, value, _viewSizeX, _viewSizeY);
            }
        }

        private int _viewSizeX;
        private int _viewSizeY;

        /// <summary>
        /// Camera horizontal resolution, used for offscreen realtime camera rendering
        /// can only be initialized at constuction time
        /// </summary>
        [DataMember]
        [BrowsableAttribute(false)]
        [Description("Camera horizontal resolution used for realtime camera rendering.  Can only be set at construction time.")]
        public int ViewSizeX
        {
            get { return _viewSizeX; }
            set
            {
                if (_frameworkCamera == null)
                    _viewSizeX = value;
            }
        }

        /// <summary>
        /// Camera horizontal resolution (read-only)
        /// </summary>
        [Category("Resolution")]
        public int Width { get { return _viewSizeX; } }

        /// <summary>
        /// Camera vertical resolution, used for offscreen realtime camera rendering
        /// can only be initialized at constuction time
        /// </summary>
        [DataMember]
        [BrowsableAttribute(false)]
        [Description("Camera vertical resolution used for realtime rendering.  Can only be set at construction time.")]
        public int ViewSizeY
        {
            get { return _viewSizeY; }
            set
            {
                // if setting is done BEFORE initialization
                if (_frameworkCamera == null)
                {
                    _viewSizeY = value;
                }
            }
        }

        /// <summary>
        /// Camera vertical resolution (read-only)
        /// </summary>
        [Category("Resolution")]
        public int Height { get { return _viewSizeY; } }


        private float _viewAngle;

        /// <summary>
        /// Rendering camera FOV angle, used for offscreen realtime camera rendering
        /// careful, this angle is in fact HALF of the total FOV
        /// can only be initialized at constuction time
        /// </summary>
        [DataMember]
        [BrowsableAttribute(false)]
        [Description("The camera Horizontal Field of View half-angle.  Can only be set at construction time.")]
        public float ViewAngle
        {
            get { return _viewAngle; }
            set
            {
                // if setting is done BEFORE initialization
                if (_frameworkCamera == null)
                    _viewAngle = value;
            }
        }

        /// <summary>
        /// Camera vertical resolution in degrees
        /// </summary>
        [Category("Projection Parameters")]
        public float FieldOfView
        {
            get
            {
                return xna.MathHelper.ToDegrees(_viewAngle);
            }
            set
            {
                SetProjectionParameters(xna.MathHelper.ToRadians(value), _aspect, _near, _far, _viewSizeX, _viewSizeY);
            }
        }

        private float _aspect;

        /// <summary>
        /// Aspect ratio (width/height) of the camera view (read-only)
        /// </summary>
        [Category("Projection Parameters")]
        public float Aspect
        {
            get { return _aspect; }
        }

        const int _numSurfacesToBuffer 
#if USE_CONCURRENT_TEXTURE_READ_BACKS
            = 32;
#else
            = 2;
#endif

        /// <summary>
        /// Targets we want to read / write from
        /// </summary>
        int _currentWriteIdx = 0;
        int _currentReadIdx = 0;

        /// <summary>
        /// A list of buffered surfaces used for offscreen realtime camera rendering.
        /// Two kind of surfaces are needed: one for color and the other for depth
        /// </summary>
        private xnagrfx.RenderTarget2D[] _drawSurfaces = new xnagrfx.RenderTarget2D[_numSurfacesToBuffer];       

        /// <summary>
        /// Surface we write to
        /// </summary>
        [BrowsableAttribute(false)]
        public xnagrfx.RenderTarget2D WriteSurface
        {
            get { return _drawSurfaces[_currentWriteIdx]; }
        }

        /// <summary>
        /// Surface we read from
        /// </summary>
        [BrowsableAttribute(false)]
        public xnagrfx.RenderTarget2D ReadSurface
        {
            get { return _drawSurfaces[_currentReadIdx]; }
        }

        // create a rigid sphere that defines the camera's physics interactions
        SphereShape _camBody = new SphereShape(new SphereShapeProperties(1, new Pose(), 0.01f));
        PhysicsEngine _thisPhysicsEngine = null;

        /// <summary>
        /// If true the camera can interact with the physics simulation
        /// </summary>
        [DataMember]
        [BrowsableAttribute(false)]
        [Description("True if the camera has a physics entity associated with it.")]
        public bool IsPhysicsVisible
        {
            get
            {
                return (PhysicsEntity != null);
            }
            set
            {
                if (value && this.PhysicsEntity == null)
                {
                    if (_thisPhysicsEngine != null)
                    {
                        CreateAndInsertPhysicsEntity(_thisPhysicsEngine);
                        this.PhysicsEntity.IsKinematic = true;
                    }
                }
                else if (!value && this.PhysicsEntity != null)
                {
                    PhysicsEngine.DeleteEntity(PhysicsEntity);
                    this.PhysicsEntity = null;
                }
            }
        }

        internal xna.Vector3 Right
        {
            get { return _frameworkCamera.InverseViewMatrix.Right; }
        }

        internal xna.Vector3 Up
        {
            get { return _frameworkCamera.InverseViewMatrix.Up; }
        }

        /// <summary>
        /// Eye point of camera
        /// </summary>
        public xna.Vector3 Eye
        {
            get { return _frameworkCamera.InverseViewMatrix.Translation; }
        }
        
        /// <summary>
        /// Look vector of camera
        /// </summary>
        public xna.Vector3 Forward
        {
            get { return _frameworkCamera.InverseViewMatrix.Forward; }
        }

        /// <summary>
        /// Custom lens effect used for rendering
        /// </summary>
        public CachedEffect LensEffect { get; set; }

        /// <summary>
        /// base constructor: no realtime, 90 (45+45) degree FOV, 800x600 camera, first person camera model
        /// </summary>
        public CameraEntity()
        {
            ConstructCameraEntity(800, 600, (float)Math.PI / 4f, CameraModelType.FirstPerson);
        }

        private void ConstructCameraEntity(int ViewSizeX, int ViewSizeY, float ViewAngle, CameraModelType model)
        {
            _isRealTimeCamera = false;

            if (ViewSizeX > 0)
                _viewSizeX = ViewSizeX;
            else
                _viewSizeX = 800;

            if (ViewSizeY > 0)
                _viewSizeY = ViewSizeY;
            else
                _viewSizeY = 600;

            if ((ViewAngle > 0) && (ViewAngle < (float)Math.PI))
                _viewAngle = ViewAngle;
            else
                _viewAngle = (float)Math.PI / 4f;

            _near = 0.1f;
            _far = 5000.0f;

            _cameraModel = model;
        }

        /// <summary>
        /// constructor: no realtime, 90 (45+45) degree FOV, size specified by user, 1st person model
        /// </summary>
        public CameraEntity(int ViewSizeX, int ViewSizeY)
        {
            ConstructCameraEntity(ViewSizeX, ViewSizeY, (float)Math.PI / 4f, CameraModelType.FirstPerson);
        }

        /// <summary>
        /// constructor: no realtime, 90 (45+45) degree FOV, size specified by user, model specified by user
        /// </summary>
        public CameraEntity(int ViewSizeX, int ViewSizeY, CameraModelType model)
        {
            ConstructCameraEntity(ViewSizeX, ViewSizeY, (float)Math.PI / 4f, model);
        }

        /// <summary>
        /// constructor: no realtime, size and FOV angle specified by user, 1st person model
        /// </summary>
        public CameraEntity(int ViewSizeX, int ViewSizeY, float ViewAngle)
        {
            ConstructCameraEntity(ViewSizeX, ViewSizeY, ViewAngle, CameraModelType.FirstPerson);
        }

        /// <summary>
        /// constructor: no realtime, size and FOV angle specified by user, model specified by user
        /// </summary>
        public CameraEntity(int ViewSizeX, int ViewSizeY, float ViewAngle, CameraModelType model)
        {
            ConstructCameraEntity(ViewSizeX, ViewSizeY, ViewAngle, model);
        }

        #region FrameworkCameraAdapters

        /// <summary>
        /// Projection matrix
        /// </summary>
        [BrowsableAttribute(false)]
        public xna.Matrix ProjectionMatrix
        {
            get { return _frameworkCamera.ProjectionMatrix; }
        }

        private bool _initFromPose = true;
        private xna.Vector3 _initLookAt;
        private xna.Vector3 _initLocation;

        /// <summary>
        /// View matrix
        /// </summary>
        [BrowsableAttribute(false)]
        public xna.Matrix ViewMatrix
        {
            get { return _frameworkCamera.ViewMatrix; }
        }

        /// <summary>
        /// The center of the camera view
        /// </summary>
        [Category("Eyepoint")]
        [DataMember]
        [Description("The 3D point at which the camera is looking.")]
        public xna.Vector3 LookAt
        {
            get
            {
                if (_frameworkCamera != null)
                {
                    return new xna.Vector3(
                      (float)UIMath.Hundredths(_frameworkCamera.LookAtPoint.X),
                      (float)UIMath.Hundredths(_frameworkCamera.LookAtPoint.Y),
                      (float)UIMath.Hundredths(_frameworkCamera.LookAtPoint.Z));
                }
                else
                    return new xna.Vector3(0);
            }
            set
            {
                if (HasBeenInitialized)
                    _frameworkCamera.SetViewParameters(_frameworkCamera.EyeLocation, value);
                else
                {
                    _initLookAt = value;
                    _initFromPose = false;
                }
            }
        }

        /// <summary>
        /// The location of the camera
        /// </summary>
        [Category("Eyepoint")]
        [DataMember]
        [Description("The location of the camera.")]
        public xna.Vector3 Location
        {
            get
            {
                if (_frameworkCamera != null)
                {
                    return new xna.Vector3(
                      (float)UIMath.Hundredths(_frameworkCamera.EyeLocation.X),
                      (float)UIMath.Hundredths(_frameworkCamera.EyeLocation.Y),
                      (float)UIMath.Hundredths(_frameworkCamera.EyeLocation.Z));
                }
                else
                    return new xna.Vector3(0);
            }
            set
            {
                if (HasBeenInitialized)
                    _frameworkCamera.SetViewParameters(value, _frameworkCamera.LookAtPoint);
                else
                {
                    _initLocation = value;
                    _initFromPose = false;
                }
            }
        }

        /// <summary>
        /// Set camera transforms
        /// </summary>
        /// <param name="fov"></param>
        /// <param name="aspect"></param>
        /// <param name="near"></param>
        /// <param name="far"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public void SetProjectionParameters(float fov, float aspect, float near, float far, int width, int height)
        {
            _viewAngle = fov;
            _aspect = aspect;
            _near = near;
            _far = far;
            _viewSizeX = width;
            _viewSizeY = height;
            if (_frameworkCamera != null)
            {
                _frameworkCamera.SetProjectionParameters(fov, aspect, near, far);
            }
        }

        #endregion

        private void InitializeCameraModel()
        {
            if (_cameraModel == CameraModelType.FirstPerson)
            {
                _frameworkCamera = new FirstPersonCamera();
                _frameworkCamera.IsPositionMovementEnabled = true;
                if (_initFromPose ||
                    ((_initLocation.X == _initLookAt.X) &&
                     (_initLocation.Y == _initLookAt.Y) &&
                     (_initLocation.Z == _initLookAt.Z)))
                {
                    xna.Vector3 lookAt = new xna.Vector3(0, 0, -1);
                    xna.Vector3 lookAt4 = xna.Vector3.Transform(lookAt,
                        xna.Matrix.CreateFromQuaternion(TypeConversion.ToXNA(State.Pose.Orientation)));

                    lookAt.X = lookAt4.X + TypeConversion.ToXNA(State.Pose.Position).X;
                    lookAt.Y = lookAt4.Y + TypeConversion.ToXNA(State.Pose.Position).Y;
                    lookAt.Z = lookAt4.Z + TypeConversion.ToXNA(State.Pose.Position).Z;

                    _frameworkCamera.SetViewParameters(TypeConversion.ToXNA(State.Pose.Position), lookAt);
                }
                else
                {
                    _frameworkCamera.SetViewParameters(_initLocation, _initLookAt);
                }
            }
            else if (_cameraModel == CameraModelType.AttachedChild)
            {
                _frameworkCamera = new AttachedChildCamera(this, xna.MathHelper.ToRadians(FieldOfView), Aspect, Near, Far);
            }
            else
                throw new Exception("Invalid camera model specified.");
        }

        /// <summary>
        /// Initialization
        /// </summary>
        /// <param name="device"></param>
        /// <param name="physicsEngine"></param>
        public override void Initialize(xnagrfx.GraphicsDevice device, PhysicsEngine physicsEngine)
        {
            try
            {
                _thisPhysicsEngine = physicsEngine;  // cache for later use
                InitError = string.Empty;

                InitializeCameraModel();

                // Add mass to our entity so we become a dynamic, kinematic entity
                State.MassDensity.Mass = 1;

                base.Initialize(device, physicsEngine);

                DeviceReset(device);

                // keep track of the physics shape
                State.PhysicsPrimitives.Add(_camBody);
                this.PhysicsEntity = null;      // default to no physics shape

                // set the projection parameters in the camera model
                _aspect = (float)_viewSizeX / (float)_viewSizeY;
                SetProjectionParameters(_viewAngle, _aspect, _near, _far, _viewSizeX, _viewSizeY);

                // allocate N frame buffers
                const int FrameBufferLookAsideCount = 3;
                for (int i = 0; i < FrameBufferLookAsideCount; i++)
                {
                    // allocate ARGB, 1 int per pixel
                    var buffer = new int[_viewSizeX * _viewSizeY];
                    this.frameBuffers.Add(buffer);
                }

                this.nextBufferIndexToUpdate = 0;
            }
            catch (Exception ex)
            {
                HasBeenInitialized = false;
                InitError = ex.ToString();
            }
        }

        /// <summary>
        /// Dispose entity resources
        /// </summary>
        public override void Dispose()
        {
            _isDisposing = true;

            // empty the deferred task queue
            // CaptureScene tasks will immediately return an exception because
            // _isDisposing is true;
            ITask task;
            while (DeferredTaskQueue.Test(out task))
            {
                task.Execute();
            }

            for (int i = 0; i < _drawSurfaces.Length; ++i)
            {
                if (_drawSurfaces[i]!=null)
                {
                    SafeDispose(ref _drawSurfaces[i]);
                }
            }

            base.Dispose();
        }

        private void SafeDispose<T>(ref T t) where T : IDisposable
        {
            if (t != null)
            {
                t.Dispose();
                t = default(T);
            }
        }

        /// <summary>
        /// Disposes device resources
        /// </summary>
        /// <param name="device"></param>
        public override void DeviceResetting(xnagrfx.GraphicsDevice device)
        {
            for(int i=0; i < _drawSurfaces.Length; ++i)
            {
                if (_drawSurfaces[i] != null)
                {
                    _drawSurfaces[i].Dispose();
                    _drawSurfaces[i] = null;
                }
            }

            base.DeviceResetting(device);
        }

        /// <summary>
        /// Recreates device resources
        /// </summary>
        /// <param name="device"></param>
        public override void DeviceReset(xnagrfx.GraphicsDevice device)
        {
            if (device == null)
                return;

            // creating surfaces
            for (int i = 0; i < _drawSurfaces.Length; ++i)
            {
                if (_drawSurfaces[i] == null)
                {
                    _drawSurfaces[i] = new xnagrfx.RenderTarget2D(
                        device,
                        ViewSizeX,
                        ViewSizeY,
                        false,
                        xnagrfx.SurfaceFormat.Color,
                        device.PresentationParameters.DepthStencilFormat,
                        device.PresentationParameters.MultiSampleCount,
                        Microsoft.Xna.Framework.Graphics.RenderTargetUsage.PlatformContents);
                }
            }

            base.DeviceReset(device);
        }

        /// <summary>
        /// List of available buffer to fill with frame data
        /// </summary>
        private List<int[]> frameBuffers = new List<int[]>();

        /// <summary>
        /// Exception caught during frame buffer update
        /// </summary>
        private Exception lastExceptionFromFrameBufferUpdate;

        /// <summary>
        /// Index of buffer in frame buffer list that should filled. Consumers
        /// are safe to use nextBufferIndexToUpdate - 1
        /// </summary>
        private int nextBufferIndexToUpdate;

        /// <summary>
        /// Frame update
        /// </summary>
        /// <param name="update"></param>
        public override void Update(FrameUpdate update)
        {
            if ((update.ActiveCamera == this) && (update.CurrentRenderMode != Microsoft.Robotics.Simulation.RenderMode.Floorplan))
            {
                _frameworkCamera.Update(update.ElapsedRealTime, update.WindowHasFocus);
            }

            if (PlaybackMode)
            {
                _frameworkCamera.SetViewParameters(State.Pose);
                _frameworkCamera.EyeLocation = TypeConversion.ToXNA(State.Pose.Position);
                _frameworkCamera.LookAtPoint = TypeConversion.ToXNA(State.Pose.Position) + _frameworkCamera.ViewMatrix.Forward;
            }
            else if (Parent != null)
            {
                base.Update(update);

                _frameworkCamera.SetViewParameters(World.Translation, World.Translation + World.Forward);

                // Process any queued requests in the context of update.
                // Requests are queued in the CaptureScene method call, for processing when its safe to access the
                // render target surface.
                // Normally this is done automaticallywhen you call base.Update() but that method also updates
                // the world and view transforms which we dont want in our case
                ITask task;
                while (DeferredTaskQueue.Test(out task))
                {
                    task.Execute();
                }
            }
            else
            {
                base.Update(update);
                if (update.ActiveCamera == this)
                {
                    // this function is only useful in the main camera, since it updates the
                    // SIMULATOR position and orientation from the matrices modified by
                    // mouse and keys in the windows framework
                    UpdateState();
                }
            }


            if (this.RecentImage)
            {
                UpdateFrameBuffersFromRenderTargetSurface(ReadSurface);
            }

            RecentImage = false;
        }

        /// <summary>
        /// Parse input devices that control camera motion
        /// </summary>
        /// <param name="elapsedTime"></param>
        /// <param name="keyboardState"></param>
        /// <param name="mouseState"></param>
        /// <param name="gamepadState"></param>
        /// <param name="hasFocus"></param>
        /// <returns></returns>
        public virtual bool UpdateInput(double elapsedTime,
                    xna.Input.KeyboardState keyboardState,
                    xna.Input.MouseState mouseState,
                    xna.Input.GamePadState gamepadState,
                    bool hasFocus)
        {
            return _frameworkCamera.UpdateInput(elapsedTime,
                keyboardState,
                mouseState,
                gamepadState,
                hasFocus);
        }

        void UpdateState()
        {
            State.Pose.Orientation =
                TypeConversion.FromXNA(xna.Quaternion.CreateFromRotationMatrix(_frameworkCamera.InverseViewMatrix));
            State.Pose.Position = TypeConversion.FromXNA(_frameworkCamera.EyeLocation);
            xna.Vector3 toTarget = -_frameworkCamera.EyeLocation + _frameworkCamera.LookAtPoint;
            toTarget.Normalize();
            toTarget = xna.Vector3.Multiply(toTarget, 0.2f);
            // place the physics entity a little bit infront of the camera, but remember position
            Vector3 realPosition = State.Pose.Position;
            State.Pose.Position += TypeConversion.FromXNA(toTarget);
            // move physics entity if it is enabled
            if (PhysicsEntity != null)
                PhysicsEntity.MovePose(State.Pose);

            State.Pose.Position = realPosition;
            xna.Matrix m = World;
            m.Translation = TypeConversion.ToXNA(State.Pose.Position);

            // An intuitive way to think of a camera is a left-handed frame (x->right, y->up, z->forward)
            // We switch around the z-vector such that specifing a (0,0,1) offset results in 1 unit in the 
            //  forward direction. Not doing this would require the user to specify (0,0,-1) to denote
            //  one unit forward, which seems counter-intuitive
            // Also note, since this method is only used by the main camera, we can safely assume
            //  the World matrix has no concatenated transforms
            m.Backward = -m.Backward;
            World = m;
        }

        /// <summary>
        /// Queue request for updating view parameters
        /// </summary>
        /// <param name="eyeLocation"></param>
        /// <param name="lookAtPoint"></param>
        public void SetViewParameters(xna.Vector3 eyeLocation, xna.Vector3 lookAtPoint)
        {
            Task<xna.Vector3, xna.Vector3> task = new Task<xna.Vector3, xna.Vector3>(eyeLocation, lookAtPoint, SetViewParametersInternal);
            DeferredTaskQueue.Post(task);
        }

        internal void SetViewParametersInternal(xna.Vector3 eyeLocation, xna.Vector3 lookAtPoint)
        {
            _frameworkCamera.SetViewParameters(eyeLocation, lookAtPoint);
            UpdateState();
        }

        /// <summary>
        /// Queue request to capture scene to integer array.
        /// Can be used to avoid races with only reading the data in other threads,
        ///     whereas Bitmap's GetPixel() cannot be called from another thread when the
        ///     sim thread is using it.
        /// User is responsible for ensuring only the returned array is not being written
        ///     to when the simulation engine is using it.
        /// Suggested use is to either only read the returned array or create a copy
        ///     of the array in the callee's thread.
        /// </summary>
        /// <param name="resultPort">Result port</param>
        public void CaptureScene(PortSet<int[], Exception> resultPort)
        {
            if (_isDisposing)
            {
                resultPort.Post(new ObjectDisposedException(State.Name, "This CameraEntity is disposed."));
                return;
            }

            // the first time we get a capture scene request, the camera is converted to real time
            // so the render engine updates our write surface and textures
            this.IsRealTimeCamera = true;

            // make sure the frame buffer manipulation happens in the entity execution context
            var task = new Task(() =>
                {
                    if (this.lastExceptionFromFrameBufferUpdate != null)
                    {
                        resultPort.Post(this.lastExceptionFromFrameBufferUpdate);
                        return;
                    }

                    var validFrameIndex = (this.nextBufferIndexToUpdate - 1);
                    if (validFrameIndex < 0)
                    {
                        validFrameIndex = this.frameBuffers.Count - 1;
                    }

                    // its entirely possible no frames have been updated. Return the stale or zero buffers
                    // anyway
                    resultPort.Post(this.frameBuffers[validFrameIndex]);
                });

            this.DeferredTaskQueue.Post(task);
        }

        /// <summary>
        /// Creates a bitmap given an integer pixel array
        /// </summary>
        /// <param name="imageData32bitBgra">Pixel data in 32bit packed format</param>
        /// <returns>Bitmap instance</returns>
        public Bitmap CreateBitmapFromArray(int[] imageData32bitBgra)
        {
            if (imageData32bitBgra.Length != _viewSizeX * _viewSizeY)
            {
                throw new ArgumentOutOfRangeException("imageData32bitBgra");
            }

            var offscreenTextureBitmap = new Bitmap(_viewSizeX, _viewSizeY, PixelFormat.Format32bppRgb);
            // move (no longer reallying copying) the data into the bitmap
            BitmapData bmd = offscreenTextureBitmap.LockBits(
                new Rectangle(0, 0, _viewSizeX, _viewSizeY),
                ImageLockMode.WriteOnly,
                PixelFormat.Format32bppArgb);

            Marshal.Copy(imageData32bitBgra, 0, bmd.Scan0, imageData32bitBgra.Length);
            offscreenTextureBitmap.UnlockBits(bmd);
            return offscreenTextureBitmap;
        }

        /// <summary>
        /// Copy the image bits from a render target surface to a bitmap
        /// </summary>
        /// <param name="rtSurface"></param>
        /// <returns></returns>
        unsafe internal void UpdateFrameBuffersFromRenderTargetSurface(xnagrfx.RenderTarget2D rtSurface)
        {
            Int64 frameId = SimulationEngine.GlobalInstance.CurrentFrameUpdate.FrameId;

            using (Profiler.AutoPop autoPop = Profiler.PushAutoPopSection("GetBitmapFromRenderTargetSurface : " + this.State.Name, Profiler.SectionType.Update))
            {
                try
                {

                    int width = 640;
                    int height = 480;
                    xnagrfx.GraphicsDevice device = null;

                    if (rtSurface == null)
                    {
                        device = this.Device;
                        width = this.Width;
                        height = this.Height;
                    }
                    else
                    {
                        width = rtSurface.Width;
                        height = rtSurface.Height;
                        device = rtSurface.GraphicsDevice;
                    }

                    if (device == null)
                    {
                        return;
                    }

                    xnagrfx.Texture2D rtBits = (xnagrfx.Texture2D)rtSurface;

                    if (rtBits == null)
                    {
                        return;
                    }

                    if (this.frameBuffers[0].Length != width * height)
                    {
                        for (int i = 0; i < this.frameBuffers.Count; i++)
                        {
                            this.frameBuffers[i] = new int[width * height];
                        }
                    }

                    var frameBuffer = this.frameBuffers[this.nextBufferIndexToUpdate];
                    // now get the bits from the rendertarget texture into a managed array
                    Profiler.PushSection("rtBits.GetData", Profiler.SectionType.Update);
                    rtBits.GetData<int>(frameBuffer);

                    // swizzle the color channels from the ARGB format used by XNA to 
                    // the ABGR format expected by GDI, WPF, and the rest of the world.
                    for (int i = 0; i < frameBuffer.Length; i++)
                    {
                        int src = frameBuffer[i];
                        frameBuffer[i] =
                            (src & 0xff) << 16 |
                            (src & 0xff00) |
                            (src & 0xff0000) >> 16;
                    }
                    Profiler.PopSection();

                    if (_surfaces == null || _surfaces.Count == 0)
                    {
                        return;
                    }

                    using (var offscreenTextureBitmap = new Bitmap(width, height, PixelFormat.Format32bppRgb))
                    {
                        // move (no longer reallying copying) the data into the bitmap
                        BitmapData bmd = offscreenTextureBitmap.LockBits(
                            new Rectangle(0, 0, width, height),
                            ImageLockMode.WriteOnly,
                            PixelFormat.Format32bppArgb);

                        Marshal.Copy(frameBuffer, 0, bmd.Scan0, frameBuffer.Length);
                        offscreenTextureBitmap.UnlockBits(bmd);

                        PictureBox disposedPictureBox = null;
                        foreach (PictureBox container in _surfaces)
                        {
                            if (!container.IsDisposed)
                            {
                                if (container.Image != null)
                                {
                                    container.Image.Dispose();
                                }

                                container.Image = new Bitmap(offscreenTextureBitmap);
                            }
                            else
                            {
                                disposedPictureBox = container;
                            }
                        }

                        if (disposedPictureBox != null)
                        {
                            _surfaces.Remove(disposedPictureBox);
                        }
                    }

                    this.lastExceptionFromFrameBufferUpdate = null;
                }
                catch (Exception e)
                {
                    this.lastExceptionFromFrameBufferUpdate = e;
                }
                finally
                {
                    this.nextBufferIndexToUpdate = (this.nextBufferIndexToUpdate + 1) % this.frameBuffers.Count;
                }
            }
        }

        /// <summary>
        /// Indicates that an image was rendered with this camera last frame
        /// </summary>
        public bool RecentImage = false;

        /// <summary>
        /// Call once a frame before SetRenderTarget
        /// </summary>
        internal void UpdateWriteSurface()
        {
#if USE_CONCURRENT_TEXTURE_READ_BACKS
            // we should use a reader-writer lock pattern here or something else thread safe
#error UpdateReadWriteSurfaces() needs to be thread safe and is not currently implemented

#else
            ++_currentWriteIdx;
            _currentWriteIdx %= _drawSurfaces.Length;
#endif
        }

        /// <summary>
        /// Skips trying to read any old frames, moving onto newer frames that were rendered
        /// </summary>
        internal void UpdateReadIndex()
        {
#if USE_CONCURRENT_TEXTURE_READ_BACKS
            // we should use a reader-writer lock pattern here or something else thread safe
#error UpdateReadIndex() needs to be thread safe and is not currently implemented

#else
            _currentReadIdx = _currentWriteIdx;
#endif
        }
    }

    /// <summary>
    /// Specifies a camera position and look-at point
    /// </summary>
    [DataContract]
    [CLSCompliant(true)]
    public class CameraView
    {
        private Vector3 _eyePosition;
        /// <summary>
        /// The location of the camera
        /// </summary>
        [DataMember]
        [Description("The position of the camera.  Not set if EyePosition == LookAtPoint.")]
        public Vector3 EyePosition
        {
            get { return _eyePosition; }
            set { _eyePosition = value; }
        }

        private Vector3 _lookAtPoint;
        /// <summary>
        /// The point the camera is looking at
        /// </summary>
        [DataMember]
        [Description("The point at which the camera is looking.  Not set if EyePosition == LookAtPoint.")]
        public Vector3 LookAtPoint
        {
            get { return _lookAtPoint; }
            set { _lookAtPoint = value; }
        }

        private int _xResolution;
        /// <summary>
        /// The pixel width of the camera image.  0 means don't change.
        /// </summary>
        [DataMember]
        [Description("The pixel width of the camera image.  0 means don't change.")]
        public int XResolution
        {
            get { return _xResolution; }
            set { _xResolution = value; }
        }

        private int _yResolution;
        /// <summary>
        /// The pixel height of the camera image.  0 means don't change.
        /// </summary>
        [DataMember]
        [Description("The pixel height of the camera image.  0 means don't change.")]
        public int YResolution
        {
            get { return _yResolution; }
            set { _yResolution = value; }
        }

        private string _cameraName;
        /// <summary>
        /// The name of the camera entity to set as the default camera.
        /// Empty string means no change.
        /// </summary>
        [DataMember]
        [Description("The name of the camera entity to set as the default camera.  Empty means don't change.")]
        public string CameraName
        {
            get { return _cameraName; }
            set { _cameraName = value; }
        }

        private RenderMode _renderMode;
        /// <summary>
        /// The render mode for the default camera.
        /// </summary>
        [DataMember]
        [Description("The render mode for the default camera.")]
        public RenderMode RenderMode
        {
            get { return _renderMode; }
            set { _renderMode = value; }
        }
    }

    /// <summary>
    /// Models a laser range finder using physics raycasting to determine impact points
    /// </summary>
    [DataContract]
    [CLSCompliant(true)]
    [EditorAttribute(typeof(GenericObjectEditor), typeof(System.Drawing.Design.UITypeEditor))]
    [RequiresParent]
    public class LaserRangeFinderEntity : VisualEntity
    {
        CachedEffectParameter _timeAttenuationHandle;

        Shape _particlePlane;

        float _elapsedSinceLastScan;
        /// <summary>
        /// Time since last update
        /// </summary>
        protected float ElapsedSinceLastScan
        {
            get { return _elapsedSinceLastScan; }
            set { _elapsedSinceLastScan = value; }
        }

        float _appTime;
        /// <summary>
        /// Application time
        /// </summary>
        protected float ApplicationTime
        {
            get { return _appTime; }
            set { _appTime = value; }
        }

        Port<RaycastResult> _raycastResultsPort;
        /// <summary>
        /// Port where raycast results are posted
        /// </summary>
        protected Port<RaycastResult> RaycastResultsPort
        {
            get { return _raycastResultsPort; }
            set { _raycastResultsPort = value; }
        }

        RaycastResult _lastResults;
        /// <summary>
        /// Latest raycast results
        /// </summary>
        protected RaycastResult LastResults
        {
            get { return _lastResults; }
            set { _lastResults = value; }
        }

        const float SCAN_INTERVAL = 0.250f;
        /// <summary>
        /// We scan 5 times a second
        /// </summary>
        protected float ScanInterval { get { return SCAN_INTERVAL; } }
        const float IMPACT_SPHERE_RADIUS = 0.02f;

        RaycastProperties _raycastProperties;
        /// <summary>
        /// Raycast configuration
        /// </summary>
        public RaycastProperties RaycastProperties
        {
            get { return _raycastProperties; }
            set { _raycastProperties = value; }
        }

        BoxShape _laserBox;

        /// <summary>
        /// Geometric representation of laser physical sensor
        /// </summary>
        [DataMember]
        [Description("The geometry representation of the laser rangefinder.")]
        public BoxShape LaserBox
        {
            get { return _laserBox; }
            set { _laserBox = value; }
        }

        VisualEntityMesh _impactPointBillboard;
        CachedEffect _impactPointEffect;

        string _impactPointEffectFileName = "LaserRangeFinder.fx";

        /// <summary>
        /// Filename of impact point effect
        /// </summary>
        [DataMember]
        [Description("Effect file used to draw laser impact billboards")]
        [Editor(typeof(FXOpenFileNameEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [Category("Rendering")]
        public string ImpactPointEffect
        {
            get { return _impactPointEffectFileName; }
            set
            {
                if (value != null)
                {
                    _impactPointEffectFileName = value;
                    if (HasBeenInitialized || ((InitError != null) && (InitError != String.Empty)))
                        SimulationEngine.GlobalInstance.RefreshEntity(this);
                }
            }
        }

        Port<RaycastResult> _serviceNotification;

        /// <summary>
        /// Default constructor
        /// </summary>
        public LaserRangeFinderEntity() { }

        /// <summary>
        /// Initialization constructor
        /// </summary>
        /// <param name="localPose"></param>
        public LaserRangeFinderEntity(Pose localPose)
        {
            // create a new instance of the laser pose so we dont re-use the raycast reference
            // That reference will be updated regularly
            BoxShapeProperties box = new BoxShapeProperties("SickLRF", 0.5f,
                localPose,
                new Vector3(0.15f, 0.20f, 0.18f));
            _laserBox = new BoxShape(box);
        }

        /// <summary>
        /// Initialization
        /// </summary>
        /// <param name="device"></param>
        /// <param name="physicsEngine"></param>
        public override void Initialize(xnagrfx.GraphicsDevice device, PhysicsEngine physicsEngine)
        {
            try
            {
                InitError = string.Empty;

                if (Parent == null)
                    throw new Exception("This entity must be a child of another entity.");

                // set flag so rendering engine renders us last
                Flags |= VisualEntityProperties.UsesAlphaBlending;

                // creates effect, loads meshes, etc
                State.PhysicsPrimitives.Add(_laserBox);
                base.Initialize(device, physicsEngine);
                CreateAndInsertPhysicsEntity(physicsEngine);

                HeightFieldShapeProperties hf = new HeightFieldShapeProperties("height field", 2, 0.02f, 2, 0.02f, 0, 0, 1, 1);
                hf.HeightSamples = new HeightFieldSample[hf.RowCount * hf.ColumnCount];
                for (int i = 0; i < hf.HeightSamples.Length; i++)
                    hf.HeightSamples[i] = new HeightFieldSample();

                _particlePlane = new Shape(hf);
                _particlePlane.State.Name = "laser impact plane";

                // we render on our own only the laser impact points. The laser Box is rendered as part of the parent.
                _impactPointBillboard = SimulationEngine.ResourceCache.CreateMesh(device, _particlePlane.State);
                _impactPointBillboard.Textures[0] = SimulationEngine.ResourceCache.CreateTextureFromFile(device, "particle.bmp");

                // Initialize the custom effect and the handle to the time parameter
                _impactPointEffect = ResourceCache.GlobalInstance.CreateEffectFromFile(device, _impactPointEffectFileName);
                if (_impactPointEffect != null)
                    _timeAttenuationHandle = _impactPointEffect.GetParameter("timeAttenuation");

                DisableEntityViewFrustumCulling(); 
            }
            catch (Exception ex)
            {
                HasBeenInitialized = false;
                InitError = ex.ToString();
            }
        }

        /// <summary>
        /// Frame update
        /// </summary>
        /// <param name="update"></param>
        public override void Update(FrameUpdate update)
        {
            using (Profiler.AutoPop autoPop = Profiler.PushAutoPopSection("LaserRangeFinderEntity.Update(FrameUpdate update)", Profiler.SectionType.Update))
            {
                if (_raycastProperties == null)
                {
                    base.Update(update);
                    return;
                }

                _appTime = (float)update.ApplicationTime;

                _elapsedSinceLastScan += (float)update.ElapsedTime;
                // only retrieve raycast results every SCAN_INTERVAL.
                // For entities that are compute intenisve, you should consider giving them
                // their own task queue so they dont flood a shared queue
                if (_elapsedSinceLastScan > SCAN_INTERVAL)
                {
                    _elapsedSinceLastScan = 0;
                    // the LRF looks towards the negative Z axis (towards the user), not the positive Z axis
                    // which is the default orientation. So we have to rotate its orientation by 180 degrees

                    _raycastProperties.OriginPose.Orientation = TypeConversion.FromXNA(
                        TypeConversion.ToXNA(State.Pose.Orientation) * xna.Quaternion.CreateFromAxisAngle(new xna.Vector3(0, 1, 0), (float)Math.PI));

                    // to calculate the position of the origin of the raycast, we must first rotate the LocalPose position
                    // of the raycast (an offset from the origin of the parent entity) by the orientation of the parent entity.
                    // The origin of the raycast is then this rotated offset added to the parent position.
                    xna.Matrix parentOrientation = xna.Matrix.CreateFromQuaternion(TypeConversion.ToXNA(State.Pose.Orientation));
                    xna.Vector3 localOffset = xna.Vector3.Transform(TypeConversion.ToXNA(_laserBox.State.LocalPose.Position), parentOrientation);

                    _raycastProperties.OriginPose.Position = State.Pose.Position + TypeConversion.FromXNA(localOffset);

                    Profiler.PushSection("LaserRangeFinder.Update : PhysicsEngine.Raycast2D", Profiler.SectionType.Update);
                    _raycastResultsPort = PhysicsEngine.Raycast2D(_raycastProperties);
                    Profiler.PopSection();

                    _raycastResultsPort.Test(out _lastResults);
                    if (_serviceNotification != null && _lastResults != null)
                    {
                        _serviceNotification.Post(_lastResults);
                    }
                }

                base.Update(update);
            }
        }

        /// <summary>
        /// Frame render
        /// </summary>
        public override void Render(RenderMode renderMode, MatrixTransforms transforms, CameraEntity currentCamera)
        {
            if ((int)(Flags & VisualEntityProperties.DisableRendering) > 0)
                return;

            // Render the box for the lrf only if the parent doesn't already have a mesh.
            transforms.World = xna.Matrix.Identity;
            if (string.IsNullOrEmpty(Parent.State.Assets.Mesh))
                base.Render(renderMode, transforms, currentCamera);

            if (_lastResults != null)
                RenderResults(renderMode, transforms, currentCamera);
        }

        void RenderResults(RenderMode renderMode, MatrixTransforms transforms, CameraEntity currentCamera)
        {
            if (currentCamera.LensEffect != null)
                return;

            //Save state
            var bs = Device.BlendState;
            var rs = Device.RasterizerState;
            var ds = Device.DepthStencilState;

            try
            {
                using (Profiler.AutoPop autoPop = Profiler.PushAutoPopSection("RenderResults", Profiler.SectionType.Draw))
                {
                    // Swap out current effect
                    CachedEffect oldEffect = Effect;
                    Effect = _impactPointEffect;

                    _timeAttenuationHandle.SetValue(new xna.Vector4(100 * (float)Math.Cos(_appTime * (1.0f / SCAN_INTERVAL)), 0, 0, 1));

                    // render impact points as a quad
                    xna.Matrix inverseViewRotation = currentCamera.ViewMatrix;
                    inverseViewRotation.M41 = inverseViewRotation.M42 = inverseViewRotation.M43 = 0;
                    xna.Matrix.Invert(ref inverseViewRotation, out inverseViewRotation);
                    xna.Matrix localTransform = xna.Matrix.CreateFromAxisAngle(new xna.Vector3(1, 0, 0), (float)-Math.PI / 2) * inverseViewRotation;

                    Batching.Graphics.DepthStencilState = xnagrfx.DepthStencilState.None;

                    xna.Matrix[] arrayOfTransforms = null;
                    int currentMatrix = 0;

                    if (Batching.IsBatchingEnabled())
                    {
                        _impactPointBillboard.AllowInstancing = true;

                        Effect.SetSceneTexture(_impactPointBillboard.Textures[0]);
                        Effect.SetMaterial(_impactPointBillboard.RenderingMaterials[0]);
                        Effect.SetMatrixTransforms(transforms);

                        base.BeginBatchRender("RenderSceneTexturedInstanced", ref arrayOfTransforms);
                    }

                    for (int i = 0; i < _lastResults.ImpactPoints.Count; i++)
                    {
                        xna.Vector3 pos = new xna.Vector3(_lastResults.ImpactPoints[i].Position.X,
                            _lastResults.ImpactPoints[i].Position.Y,
                            _lastResults.ImpactPoints[i].Position.Z);

                        xna.Vector3 resultDir = pos - Position;
                        resultDir.Normalize();

                        localTransform.Translation = pos - .02f * resultDir;

                        if (Batching.IsBatchingEnabled())
                        {
                            arrayOfTransforms[currentMatrix] = MeshTransform * _impactPointBillboard.LocalTransform * localTransform;

                            ++currentMatrix;
                            if (currentMatrix == Batching.Graphics.BatchSize)
                            {
                                base.RenderBatched(arrayOfTransforms, Batching.Graphics.BatchSize, _impactPointBillboard);
                                currentMatrix = 0;
                            }
                        }
                        else
                        {
                            _impactPointBillboard.AllowInstancing = false;

                            transforms.World = localTransform;

                            Profiler.PushSection("RenderResults (inside loop)", Profiler.SectionType.Draw);
                            base.Render(renderMode, transforms, _impactPointBillboard);
                            Profiler.PopSection();
                        }
                    }

                    if (Batching.IsBatchingEnabled())
                    {
                        if (currentMatrix > 0)
                        {
                            base.RenderBatched(arrayOfTransforms, currentMatrix, _impactPointBillboard);
                        }

                        base.EndBatchRender();
                    }

                    Batching.Graphics.DepthStencilState = xnagrfx.DepthStencilState.Default;
                    Effect = oldEffect;
                }
            }

            finally
            {
                //Restore state
                Device.BlendState = bs;
                Device.RasterizerState = rs;
                Device.DepthStencilState = ds;
            }
        }

        /// <summary>
        /// Registers a port for queueing raycast results from the physics engine
        /// </summary>
        /// <param name="notificationTarget"></param>
        public void Register(Port<RaycastResult> notificationTarget)
        {
            if (notificationTarget == null)
                throw new ArgumentNullException("notificationTarget");
            if (_serviceNotification != null)
                throw new InvalidOperationException("A notification target is already registered");
            _serviceNotification = notificationTarget;
        }
    }

    /// <summary>
    /// Models a sensor that uses a simulated depth cam to make raycasts
    /// </summary>
    [DataContract]
    [CLSCompliant(true)]
    [EditorAttribute(typeof(GenericObjectEditor), typeof(System.Drawing.Design.UITypeEditor))]
    [RequiresParent]
    public class DepthCamBasedRaycastSensorEntity : DepthCameraEntity
    {
        /// <summary>
        /// Latest reading
        /// </summary>
        public double LatestReading { get; private set; }

        const int ScanIntervalInMS = 250;

        private Port<DepthCameraEntity.DepthCameraResult> _depthCamResultPort;

        /// <summary>
        ///  The maximum range of this sensor
        /// </summary>
        [DataMember]
        public float MaximumRange { get; set; }

        /// <summary>
        /// The minimum range of this sensor
        /// </summary>
        [DataMember]
        public float MinimumRange { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public DepthCamBasedRaycastSensorEntity()
        { 
        }

        /// <summary>
        /// Initialization constructor
        /// </summary>
        /// <param name="localPose"></param>
        /// <param name="maxRange"></param>
        /// <param name="minRange"></param>
        /// <param name="radianAngleOfProjection"></param>
        /// <param name="shader">Shader file name to use for this depth camera</param>
        public DepthCamBasedRaycastSensorEntity(Pose localPose, float minRange, float maxRange, float radianAngleOfProjection, string shader)
            : base(32, 24, radianAngleOfProjection, shader)
        {
            this.MinimumRange = minRange;
            this.MaximumRange = maxRange;
            this.LatestReading = this.MaximumRange;

            IsRealTimeCamera = true;
            UpdateInterval = ScanIntervalInMS;
            ShadowDisplay = CameraEntity.ShadowDisplayMode.HideShadows;
            CameraModel = CameraEntity.CameraModelType.AttachedChild;

            Near = this.MinimumRange;
            Far = this.MaximumRange;

            ViewSizeX = 32;
            ViewSizeY = 24;
            FieldOfView = (float)(radianAngleOfProjection * (180 / Math.PI));
            EntityState.Pose = localPose;
        }

        /// <summary>
        /// Initialization
        /// </summary>
        /// <param name="device"></param>
        /// <param name="physicsEngine"></param>
        public override void Initialize(xnagrfx.GraphicsDevice device, PhysicsEngine physicsEngine)
        {
            try
            {
                InitError = string.Empty;

                if (Parent == null)
                    throw new Exception("This entity must be a child of another entity.");

                // Register to receive frames from ourselves
                this._depthCamResultPort = new Port<DepthCameraResult>();
                this.Register(_depthCamResultPort);

                SimulationEngine.GlobalInstance.Activate(
                Arbiter.Receive<DepthCameraEntity.DepthCameraResult>(
                    true,
                    this._depthCamResultPort,
                    result => DepthCamResultsHandler(result)));

                base.Initialize(device, physicsEngine);
            }
            catch (Exception ex)
            {
                this.HasBeenInitialized = false;
                this.InitError = ex.ToString();
            }
        }

        /// <summary>
        /// Handles updates from the depth camera
        /// </summary>
        /// <param name="result"></param>
        private void DepthCamResultsHandler(DepthCameraEntity.DepthCameraResult result)
        {
            try
            {
                if (result.Data == null)
                {
                    return;
                }

                var depthImage = new short[result.Data.Length];

                short maxDepthMM = (short)(this.MaximumRange * 1000);
                short minDepthMM = (short)(this.MinimumRange * 1000);
                short closestMM = maxDepthMM;

                for (int i = 0; i < result.Data.Length; i++)
                {
                    var s = (short)(result.Data[i] & 0xFF);
                    var depth = (short)((s * (short)maxDepthMM) / byte.MaxValue);
                    if (depth > maxDepthMM || depth < minDepthMM)
                    {
                        // this if branch is redundant if the shader sets the depth limit but its defense in depth.
                        depthImage[i] = maxDepthMM;
                    }
                    else
                    {
                        depthImage[i] = depth;
                    }

                    closestMM = Math.Min(closestMM, depthImage[i]);
                }

                this.LatestReading = (double)closestMM / 1000;
            }
            finally
            {
                _depthCamResultPort.Clear();
            }
        }
    }

    /// <summary>
    /// Models an infrared sensor using a depth camera
    /// </summary>
    [DataContract]
    [CLSCompliant(true)]
    [EditorAttribute(typeof(GenericObjectEditor), typeof(System.Drawing.Design.UITypeEditor))]
    [RequiresParent]
    public class IREntity : DepthCamBasedRaycastSensorEntity
    {
        /// <summary>
        /// Default Max range is .8m
        /// </summary>
        const float DefaultMaximumIRRange = 0.8f;

        /// <summary>
        /// Default Min range is .1m
        /// </summary>
        const float DefaultMinimumIRRange = 0.1f;

        /// <summary>
        /// Default IR has a very narrow cone. 2 degree total
        /// </summary>
        const float DefaultRadianAngleOfProjection = 0.0349f;

        /// <summary>
        /// Default constructor
        /// </summary>
        public IREntity() 
        {
            this.ShaderFile = StandardShaders.IRSensor;
        }

        /// <summary>
        /// Initialization constructor
        /// </summary>
        /// <param name="localPose"></param>
        public IREntity(Pose localPose)
            : base(localPose, DefaultMinimumIRRange, DefaultMaximumIRRange, DefaultRadianAngleOfProjection, StandardShaders.IRSensor)
        {
        }

        /// <summary>
        /// Initialization constructor
        /// </summary>
        /// <param name="localPose">Pose of IREntity</param>
        /// <param name="maxRange">Maximum range in meters that this sensor can detect</param>
        /// <param name="minRange">Minimum range in meters that this sensor can detect</param>
        /// <param name="radianAngleOfProjection">Full angular size of detection cone in radians</param>
        public IREntity(Pose localPose, float minRange, float maxRange, float radianAngleOfProjection)
            : base(localPose, minRange, maxRange, radianAngleOfProjection, StandardShaders.IRSensor)
        {
        }
    }

    /// <summary>
    /// Models an sonar sensor using a depth camera
    /// </summary>
    [DataContract]
    [CLSCompliant(true)]
    [EditorAttribute(typeof(GenericObjectEditor), typeof(System.Drawing.Design.UITypeEditor))]
    [RequiresParent]
    public class SonarEntity : DepthCamBasedRaycastSensorEntity
    {
        /// <summary>
        /// Default Max range is 2m
        /// </summary>
        const float DefaultMaximumIRRange = 2.0f;

        /// <summary>
        /// Default Min range is .1m
        /// </summary>
        const float DefaultMinimumIRRange = 0.1f;

        /// <summary>
        /// Default Sonar has a large cone - 45 degrees
        /// </summary>
        const float DefaultRadianAngleOfProjection = 0.78539f;

        /// <summary>
        /// Default constructor
        /// </summary>
        public SonarEntity() 
        {
            this.ShaderFile = StandardShaders.SonarSensor;
        }

        /// <summary>
        /// Initialization constructor
        /// </summary>
        /// <param name="localPose"></param>
        public SonarEntity(Pose localPose)
            : base(localPose, DefaultMinimumIRRange, DefaultMaximumIRRange, DefaultRadianAngleOfProjection, StandardShaders.SonarSensor)
        {
        }

        /// <summary>
        /// Initialization constructor
        /// </summary>
        /// <param name="localPose">Pose of SonarEntity</param>
        /// <param name="maxRange">Maximum range in meters that this sensor can detect</param>
        /// <param name="minRange">Minimum range in meters that this sensor can detect</param>
        /// <param name="radianAngleOfProjection">Full angular size of detection cone in radians</param>
        public SonarEntity(Pose localPose, float minRange, float maxRange, float radianAngleOfProjection)
            : base(localPose, minRange, maxRange, radianAngleOfProjection, StandardShaders.SonarSensor)
        {
        }
    }
    
    /// <summary>
    /// An entity simulating a Microsoft GPS device.
    /// NOTE: Prototype, not functioning yet
    /// </summary>
    [DataContract]
    [CLSCompliant(true)]
    [EditorAttribute(typeof(GenericObjectEditor), typeof(System.Drawing.Design.UITypeEditor))]
    [BrowsableAttribute(false)] // prevent from being displayed in NewEntity dialog
    public class MicrosoftGpsEntity : VisualEntity
    {
        float _elapsedSinceLastScan;
        float _appTime;
        Port<RaycastResult> _raycastResultsPort;
        RaycastResult _lastResults;
        List<xna.Vector3> _satelliteLocations;

        /// <summary>
        /// Simulated satellite positions
        /// </summary>
        [DataMember]
        [Description("Simulated satellite positions.")]
        public List<xna.Vector3> SatelliteLocations
        {
            get { return _satelliteLocations; }
            set { _satelliteLocations = value; }
        }

        /// <summary>
        /// Ray cast interval
        /// </summary>
        const float SCAN_INTERVAL = 1.0f;
        BoxShape _gpsUnit;

        /// <summary>
        /// Geometric representation of physical sensor unit
        /// </summary>
        [DataMember]
        [Description("Geometric representation of the physical sensor unit.")]
        public BoxShape GpsUnit
        {
            get { return _gpsUnit; }
            set { _gpsUnit = value; }
        }

        Port<List<bool>> _serviceNotification;

        /// <summary>
        /// Default constructor
        /// </summary>
        public MicrosoftGpsEntity() { }

        /// <summary>
        /// Initialization constructor
        /// </summary>
        /// <param name="localPose"></param>
        public MicrosoftGpsEntity(Pose localPose)
        {
            // create a new instance of the laser pose so we dont re-use the raycast reference
            // That reference will be updated regularly
            BoxShapeProperties box = new BoxShapeProperties("GpsUnit", 0.050f,
                localPose,
                new Vector3(0.05f, 0.01f, 0.05f));
            _gpsUnit = new BoxShape(box);
        }

        /// <summary>
        /// Initialization
        /// </summary>
        /// <param name="device"></param>
        /// <param name="physicsEngine"></param>
        public override void Initialize(xnagrfx.GraphicsDevice device, PhysicsEngine physicsEngine)
        {
            try
            {
                InitError = string.Empty;
                // creates effect, loads meshes, etc
                base.Initialize(device, physicsEngine);

                if (Parent == null)
                    throw new Exception("This entity must be a child of another entity.");

                if (Parent != null)
                {
                    if (Meshes.Count > 0)
                        Parent.AddShapeToPhysicsEntity(_gpsUnit, Meshes[0]);
                    else
                        Parent.AddShapeToPhysicsEntity(_gpsUnit, null);
                }
            }
            catch (Exception ex)
            {
                HasBeenInitialized = false;
                InitError = ex.ToString();
            }
        }

        /// <summary>
        /// Frame update
        /// </summary>
        /// <param name="update"></param>
        public override void Update(FrameUpdate update)
        {
            if (_satelliteLocations == null)
            {
                base.Update(update);
                return;
            }

            _appTime = (float)update.ApplicationTime;

            // assume pose of parent
            if (Parent != null)
                State.Pose = Parent.State.Pose;

            _elapsedSinceLastScan += (float)update.ElapsedTime;
            // only retrieve raycast results every SCAN_INTERVAL.
            // For entities that are compute intenisve, you should consider giving them
            // their own task queue so they dont flood a shared queue
            if (_elapsedSinceLastScan > SCAN_INTERVAL)
            {
                _elapsedSinceLastScan = 0;
                // the LRF looks towards the negative Z axis (towards the user), not the positive Z axis
                // which is the default orientation. So we have to rotate its orientation by 180 degrees

                RaycastProperties raycast = new RaycastProperties();
                raycast.OriginPose.Orientation = State.Pose.Orientation;
                raycast.OriginPose.Position = State.Pose.Position + _gpsUnit.State.LocalPose.Position;
                _raycastResultsPort = PhysicsEngine.Raycast2D(raycast);
                _raycastResultsPort.Test(out _lastResults);
                if (_serviceNotification != null && _lastResults != null)
                {

                }
            }

            base.Update(update);
        }

        /// <summary>
        /// Register port for sending notifications
        /// </summary>
        /// <param name="notificationTarget"></param>
        public void Register(Port<List<bool>> notificationTarget)
        {
            if (notificationTarget == null)
                throw new ArgumentNullException("notificationTarget");
            if (_serviceNotification != null)
                throw new InvalidOperationException("A notification target is already registered");
            _serviceNotification = notificationTarget;
        }

        /// <summary>
        /// Add satellite position
        /// </summary>
        /// <param name="position"></param>
        public void AddSatellite(xna.Vector3 position)
        {
            _satelliteLocations.Add(position);
        }
    }


    #endregion

    #region Actuators
    /// <summary>
    /// Rendering wrapper around PhysicsWheel shape. If you are not interested in rendering
    /// just use PhysicsWheel directly
    /// </summary>
    [DataContract]
    [CLSCompliant(true)]
    [EditorAttribute(typeof(GenericObjectEditor), typeof(System.Drawing.Design.UITypeEditor))]
    public class WheelEntity : VisualEntity
    {
        PhysicsWheel _wheel;

        /// <summary>
        /// Physics entity simulating wheel.
        /// <remarks>Used for modifying and inspecting state while simulation is active</remarks>
        /// </summary>
        [Browsable(false)]
        public PhysicsWheel Wheel
        {
            get { return _wheel; }
            set { _wheel = value; }
        }
        /// <summary>
        /// Wheel shape instance
        /// </summary>
        [DataMember]
        [Description("The shape that represents the wheel.")]
        public WheelShape WheelShape
        {
            get { return _wheel; }
            set { _wheel = PhysicsWheel.Create(value.WheelState); }
        }

        const float rotationScale = (float)(-1.0 / (2.0 * Math.PI));
        float _rotations = 0;
        
        /// <summary>
        /// A count of wheel rotations.
        /// </summary>
        [DataMember]
        [Description("A count of wheel rotations.")]
        public float Rotations
        {
            get { return _rotations; }
            set { _rotations = value; }
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public WheelEntity() { }

        /// <summary>
        /// Initialization constructor
        /// </summary>
        /// <param name="mass"></param>
        /// <param name="radius"></param>
        /// <param name="position"></param>
        /// <param name="YRotationDegrees"></param>
        public WheelEntity(float mass, float radius, Vector3 position, float YRotationDegrees)
        {
            WheelShapeProperties shape = new WheelShapeProperties("WheelShapeProperties" + Guid.NewGuid(), mass, radius);
            shape.LocalPose.Position = position;
            shape.LocalPose.Orientation = Quaternion.FromAxisAngle(0, 1, 0, (float)(YRotationDegrees * Math.PI / 180.0));
            _wheel = PhysicsWheel.Create(shape);
        }

        /// <summary>
        /// Initialization constructor
        /// </summary>
        /// <param name="shape"></param>
        public WheelEntity(WheelShapeProperties shape)
        {
            if(shape==null)
            {
                shape = new WheelShapeProperties("WheelShapeProperties" + Guid.NewGuid(), 1.0f, 1.0f);
            }    
            _wheel = PhysicsWheel.Create(shape);
        }

        VisualEntityMesh _visualMesh = null;

        /// <summary>
        /// Initialization
        /// </summary>
        /// <param name="device"></param>
        /// <param name="physicsEngine"></param>
        public override void Initialize(xnagrfx.GraphicsDevice device, PhysicsEngine physicsEngine)
        {
            try
            {
                InitError = string.Empty;

                // This entity adds a shape to its parent rather than defining its own shapes
                AddsShapeToParent = true;

                // we don't support rendering. The parent can decide to render us as part of its geometry
                base.Initialize(device, physicsEngine);

                if (Parent == null)
                    throw new Exception("This entity must be a child of another entity.");

                // add to parent
                _visualMesh = Parent.AddShapeToPhysicsEntity(Wheel, null);
            }
            catch (Exception ex)
            {
                HasBeenInitialized = false;
                InitError = ex.ToString();
            }
        }

        /// <summary>
        /// Remove the wheel shape from the parent on dispose
        /// </summary>
        public override void Dispose()
        {
            if (Wheel.Parent != null)
            {
                ((VisualEntity)Wheel.Parent).RemoveShapeFromPhysicsEntity(Wheel, _visualMesh);
                _visualMesh = null;
            }

            base.Dispose();
        }

        /// <summary>
        /// Frame update
        /// </summary>
        /// <param name="update"></param>
        public override void Update(FrameUpdate update)
        {
            ProcessDeferredTaskQueue();

            // set our global pose to be the parents pose. Then our wheel shape will be
            // positioned relative to that. We could also just set our global position to be
            // parent pose combines with local pose, and then just use base.Render()
            if (Parent != null)
                State.Pose = Parent.State.Pose;

            // set the wheel orientation to match rotations and the steer angle
            if (!SuspendUpdates)
            {
                Wheel.State.LocalPose.Orientation =
                    Quaternion.FromAxisAngle(
                        0, 1, 0,
                        Wheel.SteerAngle
                    ) * Quaternion.FromAxisAngle(
                        -1, 0, 0,
                        (float)(Rotations * 2 * Math.PI)
                    );
            }

            // update the rotations for the next frame
            Rotations += (float)(Wheel.AxleSpeed * update.ElapsedTime * rotationScale);
        }

        /// <summary>
        /// Frame render
        /// </summary>
        public override void Render(RenderMode renderMode, MatrixTransforms transforms, CameraEntity currentCamera)
        {
            // if we have our own mesh, render it, otherwise return and render as part
            // of parents mesh
            if (string.IsNullOrEmpty(State.Assets.Mesh))
                return;

            RenderShape(renderMode, transforms, _wheel.WheelState, Meshes[0]);
        }

        /// <summary>
        /// Wheel entity uses localpose
        /// </summary>
        /// <returns></returns>
        public override Pose GetPlaybackPose()
        {
            return Wheel.State.LocalPose;
        }

        /// <summary>
        /// Wheel entity uses localpose
        /// </summary>
        /// <param name="pose"></param>
        public override void SetPlaybackPose(Pose pose)
        {
            Wheel.State.LocalPose = pose;
        }

    }

    #region KUKA LBR3 Arm
    #region CODECLIP 01-1
    /// <summary>
    /// Models KUKA LBR3 robotic arm
    /// </summary>
    [DataContract]
    [CLSCompliant(true)]
    [EditorAttribute(typeof(GenericObjectEditor), typeof(System.Drawing.Design.UITypeEditor))]
    public class KukaLBR3Entity : SingleShapeEntity
    {
        const float ARM_MASS = 1f;
        const float ARM_THICKNESS = 0.03f;
        const float ARM_LENGTH = 0.075f;
        const float DELTA = 0.000f;

        // approximation of the Lbr3 arms for arms 1->5
        // base is considered arm/link 0, and the end effectors are 6 and 7
        float ARM_LENGTH2 = ARM_LENGTH + ARM_THICKNESS * 2;

        List<Joint> _joints = new List<Joint>();
        /// <summary>
        /// Joints
        /// </summary>
        [Description("All of the joints in the entity.")]
        public List<Joint> Joints
        {
            get
            {
                if (_joints.Count == 0)
                {
                    VisualEntity entity = this;
                    while (true)
                    {
                        if (entity.Children.Count == 0)
                            return _joints;

                        if (entity.Children[0].ParentJoint != null)
                            _joints.Add(entity.Children[0].ParentJoint);

                        entity = entity.Children[0];
                    }
                }
                return _joints;
            }
            set { _joints = value; }
        }
        #endregion
        #region CustomJointSingleShapeEntity
        /// <summary>
        /// Defines a new entity type that overrides the ParentJoint with 
        /// custom joint properties.  It also handles serialization and
        /// deserialization properly.
        /// </summary>
        [DataContract]
        public class CustomJointSingleShapeEntity : SingleShapeEntity
        {
            private Joint _customJoint;

            /// <summary>
            /// A custom joint definition used for the ParentJoint
            /// </summary>
            [DataMember]
            public Joint CustomJoint
            {
                get { return _customJoint; }
                set { _customJoint = value; }
            }

            /// <summary>
            /// Default constructor
            /// </summary>
            public CustomJointSingleShapeEntity() { }

            /// <summary>
            /// Initialization constructor
            /// </summary>
            /// <param name="shape"></param>
            /// <param name="initialPos"></param>
            public CustomJointSingleShapeEntity(Shape shape, Vector3 initialPos)
                : base(shape, initialPos)
            {
            }

            /// <summary>
            /// Initialization override which adds support for custom joint initialization
            /// </summary>
            /// <param name="device"></param>
            /// <param name="physicsEngine"></param>
            public override void Initialize(Microsoft.Xna.Framework.Graphics.GraphicsDevice device, PhysicsEngine physicsEngine)
            {
                base.Initialize(device, physicsEngine);

                // update the parent joint to match our custom joint parameters
                if (_customJoint != null)
                {
                    if ((ParentJoint != null) && (ParentJoint.InternalHandle != (IntPtr)0))
                        PhysicsEngine.DeleteJoint((PhysicsJoint)ParentJoint);

                    PhysicsEntity.SolverIterationCount = 128;
                    Parent.PhysicsEntity.SolverIterationCount = 128;

                    _customJoint.State.Connectors[0].Entity = Parent;
                    _customJoint.State.Connectors[1].Entity = this;

                    ParentJoint = _customJoint;
                    PhysicsEngine.InsertJoint((PhysicsJoint)ParentJoint);
                }
            }
        }
        #endregion
        #region CODECLIP 01-2
        private class SegmentDescriptor
        {
            public string Name;
            public string Mesh;
            public float Radius;
            public float Length;
            public float Mass;
            public Vector3 MeshRotation;
            public Vector3 MeshOffset;
            public Vector3 NormalAxis;
            public Vector3 LocalAxis;
            public Vector3 Connect0;
            public Vector3 Connect1;
            public Shape Shape;
            public SegmentDescriptor()
            {
                Name = Mesh = string.Empty;
                Radius = Length = 0f;
                Mass = 1;
                NormalAxis = LocalAxis = Connect0 = Connect1 = new Vector3();
                MeshRotation = MeshOffset = new Vector3();
                Shape = null;
            }

            public CustomJointSingleShapeEntity CreateSegment()
            {
                CustomJointSingleShapeEntity result = null;
                if (Shape != null)
                {
                    result = new CustomJointSingleShapeEntity(Shape, new Vector3());
                }
                else
                {
                    CapsuleShape capsule = new CapsuleShape(new CapsuleShapeProperties(
                        Mass,
                        new Pose(new Vector3(0, Length / 2 + Radius, 0)),
                        Radius,
                        Length));

                    result = new CustomJointSingleShapeEntity(capsule, new Vector3());
                }
                result.State.Name = "Segment"+Name;
                result.State.Assets.Mesh = Mesh;
                result.State.Pose.Orientation = new Quaternion(0, 0, 0, 1);
                result.MeshTranslation = MeshOffset;
                result.MeshRotation = MeshRotation;
                result.State.MassDensity.AngularDamping = 5000;
                result.State.MassDensity.LinearDamping = 5000;


                JointAngularProperties angular = new JointAngularProperties();
                angular.TwistMode = JointDOFMode.Free;
                angular.TwistDrive = new JointDriveProperties(
                    JointDriveMode.Position,
                    new SpringProperties(500000, 100000, 0),
                    1000000);

                EntityJointConnector[] connectors = new EntityJointConnector[2]
                {
                    new EntityJointConnector(null, NormalAxis, LocalAxis, Connect0),
                    new EntityJointConnector(null, NormalAxis, LocalAxis, Connect1)
                };

                result.CustomJoint = new Joint();
                result.CustomJoint.State = new JointProperties(angular, connectors);
                result.CustomJoint.State.Name = "Joint" + Name;

                return result;
            }
        }
        #endregion
        #region CODECLIP 01-3
        /// <summary>
        /// Default constructor
        /// </summary>
        public KukaLBR3Entity()
        {
        }

        /// <summary>
        /// Initialization constructor
        /// </summary>
        /// <param name="position"></param>
        public KukaLBR3Entity(Vector3 position)
        {
            State.Pose.Position = position;

            const float BaseMass = 1;
            const float BaseRadius = 0.03f;
            const float BaseHeight = 0.015f;
            Vector3 XAxis = new Vector3(1, 0, 0);
            Vector3 YAxis = new Vector3(0, 1, 0);
            Vector3 ZAxis = new Vector3(0, 0, 1);
            Vector3 NegativeZAxis = new Vector3(0, 0, -1);

            // Base (this entity)
            this.CapsuleShape = new CapsuleShape(new CapsuleShapeProperties(
                BaseMass,
                new Pose(new Vector3(0, BaseRadius + BaseHeight / 2, 0)),
                BaseRadius,
                BaseHeight));

            // define arm segments and connection points
            SegmentDescriptor[] _segments = new SegmentDescriptor[7];

            // Segment 0
            _segments[0] = new SegmentDescriptor();
            _segments[0].Name = "0 " + Guid.NewGuid().ToString();
            _segments[0].Mesh = "lbr3_j1.obj";
            _segments[0].Radius = ARM_THICKNESS;
            _segments[0].Length = ARM_LENGTH;
            _segments[0].Mass = 1;
            _segments[0].NormalAxis = ZAxis;
            _segments[0].LocalAxis = YAxis;
            _segments[0].Connect0 = new Vector3(0, BaseRadius * 2 + BaseHeight, 0);
            _segments[0].Connect1 = new Vector3(0, 0, 0);

            // Segment 1
            _segments[1] = new SegmentDescriptor();
            _segments[1].Name = "1 " + Guid.NewGuid().ToString();
            _segments[1].Mesh = "lbr3_j1.obj";
            _segments[1].Radius = ARM_THICKNESS;
            _segments[1].Length = ARM_LENGTH;
            _segments[1].Mass = 1;
            _segments[1].MeshOffset = new Vector3(0, ARM_LENGTH + 2 * ARM_THICKNESS, 0);
            _segments[1].MeshRotation = new Vector3(180, 0, 0);
            _segments[1].NormalAxis = YAxis;
            _segments[1].LocalAxis = NegativeZAxis;
            _segments[1].Connect0 = new Vector3(0, ARM_LENGTH2, 0);
            _segments[1].Connect1 = new Vector3(0, 0, 0);

            // Segment 2
            _segments[2] = new SegmentDescriptor();
            _segments[2].Name = "2 " + Guid.NewGuid().ToString();
            _segments[2].Mesh = "lbr3_j1.obj";
            _segments[2].Radius = ARM_THICKNESS;
            _segments[2].Length = ARM_LENGTH;
            _segments[2].Mass = 1;
            _segments[2].MeshOffset = new Vector3(0, 0, 0);
            _segments[2].MeshRotation = new Vector3(0, 180, 0);
            _segments[2].NormalAxis = ZAxis;
            _segments[2].LocalAxis = YAxis;
            _segments[2].Connect0 = new Vector3(0, ARM_LENGTH2, 0);
            _segments[2].Connect1 = new Vector3(0, 0, 0);

            // Segment 3
            _segments[3] = new SegmentDescriptor();
            _segments[3].Name = "3 " + Guid.NewGuid().ToString();
            _segments[3].Mesh = "lbr3_j1.obj";
            _segments[3].Radius = ARM_THICKNESS;
            _segments[3].Length = ARM_LENGTH;
            _segments[3].Mass = 1;
            _segments[3].MeshOffset = new Vector3(0, ARM_LENGTH + 2 * ARM_THICKNESS, 0);
            _segments[3].MeshRotation = new Vector3(0, 0, 180);
            _segments[3].NormalAxis = YAxis;
            _segments[3].LocalAxis = NegativeZAxis;
            _segments[3].Connect0 = new Vector3(0, ARM_LENGTH2, 0);
            _segments[3].Connect1 = new Vector3(0, 0, 0);

            // Segment 4
            _segments[4] = new SegmentDescriptor();
            _segments[4].Name = "4 " + Guid.NewGuid().ToString();
            _segments[4].Mesh = "lbr3_j5.obj";
            _segments[4].Radius = ARM_THICKNESS;
            _segments[4].Length = ARM_LENGTH;
            _segments[4].Mass = 1;
            _segments[4].MeshOffset = new Vector3(0, 0, 0);
            _segments[4].MeshRotation = new Vector3(0, 0, -90);
            _segments[4].NormalAxis = ZAxis;
            _segments[4].LocalAxis = YAxis;
            _segments[4].Connect0 = new Vector3(0, ARM_LENGTH2, 0);
            _segments[4].Connect1 = new Vector3(0, 0, 0);

            // Segment 5
            _segments[5] = new SegmentDescriptor();
            _segments[5].Name = "5 " + Guid.NewGuid().ToString();
            _segments[5].Mesh = "lbr3_j6.obj";
            _segments[5].Radius = ARM_THICKNESS;
            _segments[5].Length = ARM_LENGTH;
            _segments[5].Mass = 1;
            _segments[5].MeshOffset = new Vector3(0, 0, 0.01f);
            _segments[5].MeshRotation = new Vector3(0, 0, -90);
            _segments[5].NormalAxis = YAxis;
            _segments[5].LocalAxis = NegativeZAxis;
            _segments[5].Connect0 = new Vector3(0, ARM_LENGTH2 - 0.01f, 0);
            _segments[5].Connect1 = new Vector3(0, 0, 0.02f - 0.006f);
            _segments[5].Shape = new CapsuleShape(new CapsuleShapeProperties(1, new Pose(), 0.02f, 0.01f));

            // Segment 6
            _segments[6] = new SegmentDescriptor();
            _segments[6].Name = "6 " + Guid.NewGuid().ToString();
            _segments[6].Mesh = string.Empty;
            _segments[6].Radius = ARM_THICKNESS;
            _segments[6].Length = ARM_LENGTH;
            _segments[6].Mass = 0.1f;
            _segments[6].MeshOffset = new Vector3(0, 0.015f, 0);
            _segments[6].MeshRotation = new Vector3(0, 0, 0);
            _segments[6].NormalAxis = XAxis;
            _segments[6].LocalAxis = YAxis;
            _segments[6].Connect0 = new Vector3(0, 0.025f, 0.01f);
            _segments[6].Connect1 = new Vector3(0, 0, 0);
            _segments[6].Shape =  new BoxShape(new BoxShapeProperties(0.1f,
                new Pose(new Vector3(0, 0, 0),
                TypeConversion.FromXNA(xna.Quaternion.CreateFromAxisAngle(new xna.Vector3(0, 1, 0), -(float)Math.PI / 2))
                ),
                new Vector3(0.05f, 0.002f, 0.05f)));

            Vector3 initialPosition = new Vector3(0, ARM_LENGTH, 0);
            // start out with the base entity as parent
            VisualEntity parent = this; 
            // insert each entity as a child of the previous
            foreach (SegmentDescriptor desc in _segments)
            {
                if (desc != null)
                {
                    CustomJointSingleShapeEntity child = desc.CreateSegment();
                    child.State.Pose.Position = initialPosition;
                    child.CustomJoint.State.Connectors[0].Entity = parent;
                    child.CustomJoint.State.Connectors[1].Entity = child;
                    parent.InsertEntity(child);
                    parent = child;
                    initialPosition += new Vector3(0, ARM_LENGTH, 0);
                }
            }

            // Make the base kinematic.  Clear this flag after calling the constructor
            // to attach to another entity.
            State.Flags |= EntitySimulationModifiers.Kinematic;
            State.Assets.Mesh = "lbr3_j0.obj";
        }
        #endregion
        #region CODECLIP 01-4
        /// <summary>
        /// Sets orientation only for angular drives
        /// </summary>
        /// <param name="j"></param>
        /// <param name="axisAngle"></param>
        public void SetJointTargetOrientation(Joint j, AxisAngle axisAngle)
        {
            Task<Joint, AxisAngle> deferredTask = new Task<Joint, AxisAngle>(j, axisAngle, SetJointTargetOrientationInternal);
            DeferredTaskQueue.Post(deferredTask);
        }

        /// <summary>
        /// Sets position and orientation depending on the DOF configuration of the joint
        /// </summary>
        /// <param name="j"></param>
        /// <param name="pose"></param>
        public void SetJointTargetPose(Joint j, Pose pose)
        {
            Task<Joint, Pose> deferredTask = new Task<Joint, Pose>(j, pose, SetJointTargetPoseInternal);
            DeferredTaskQueue.Post(deferredTask);
        }

        /// <summary>
        /// Sets angular or linear velocity
        /// </summary>
        /// <param name="j"></param>
        /// <param name="velocity"></param>
        public void SetJointTargetVelocity(Joint j, Vector3 velocity)
        {
            Task<Joint, Vector3> deferredTask = new Task<Joint, Vector3>(j, velocity, SetJointTargetVelocityInternal);
            DeferredTaskQueue.Post(deferredTask);
        }

        void SetJointTargetPoseInternal(Joint j, Pose pose)
        {
            if (j.State.Linear != null)
                ((PhysicsJoint)j).SetLinearDrivePosition(pose.Position);

            if (j.State.Angular != null)
                ((PhysicsJoint)j).SetAngularDriveOrientation(pose.Orientation);
        }

        void SetJointTargetOrientationInternal(Joint j, AxisAngle axisAngle)
        {
            if (j.State.Angular != null)
            {
                // the physics engine doesn't deal well with axis angles in the range -2*Pi to -Pi
                // and Pi to 2*Pi.  Work around this problem by adjusting the angle to the range -Pi to Pi
                // and then map the angle to the range 2*Pi to 3*Pi if positive and -2*Pi to -3*Pi if negative.
                // This seems to get the physics engine to do the right thing.
                float fullRotation = (float)(2 * Math.PI);

                while (axisAngle.Angle < -Math.PI)
                    axisAngle.Angle += fullRotation;

                while (axisAngle.Angle > Math.PI)
                    axisAngle.Angle -= fullRotation;

                if (axisAngle.Angle < 0.0f)
                    axisAngle.Angle -= fullRotation;
                else
                    axisAngle.Angle += fullRotation;

                Quaternion target =
                    TypeConversion.FromXNA(
                    xna.Quaternion.CreateFromAxisAngle(TypeConversion.ToXNA(axisAngle.Axis), axisAngle.Angle)
                    );
                ((PhysicsJoint)j).SetAngularDriveOrientation(target);
            }
        }

        void SetJointTargetVelocityInternal(Joint j, Vector3 velocity)
        {
            if (j.State.Linear != null)
                ((PhysicsJoint)j).SetLinearDriveVelocity(velocity);
            else
                ((PhysicsJoint)j).SetAngularDriveVelocity(velocity);
        }
        #endregion
    }
    #endregion
    
    #endregion


    #region Robot platforms
    /// <summary>
    /// Models a differential drive motor base with two active wheels and one caster
    /// </summary>
    [DataContract]
    [CLSCompliant(true)]
    [EditorAttribute(typeof(GenericObjectEditor), typeof(System.Drawing.Design.UITypeEditor))]
    [BrowsableAttribute(false)] // prevent from being displayed in NewEntity dialog
    public class DifferentialDriveEntity : VisualEntity
    {
        #region State
        /// <summary>
        /// Chassis mass in kilograms
        /// </summary>
        public float MASS;

        // the default settings are approximating a Pioneer 3-DX activMedia robot
        /// <summary>
        /// Chassis dimensions
        /// </summary>
        protected Vector3 CHASSIS_DIMENSIONS;
        /// <summary>
        /// Left front wheel position
        /// </summary>
        protected Vector3 LEFT_FRONT_WHEEL_POSITION;
        /// <summary>
        /// Right front wheel position
        /// </summary>
        protected Vector3 RIGHT_FRONT_WHEEL_POSITION;
        /// <summary>
        /// Caster wheel position
        /// </summary>
        protected Vector3 CASTER_WHEEL_POSITION;

        /// <summary>
        /// Distance from ground of chassis
        /// </summary>
        protected float CHASSIS_CLEARANCE;
        /// <summary>
        /// Mass of front wheels
        /// </summary>
        protected float FRONT_WHEEL_MASS;
        /// <summary>
        /// Radius of front wheels
        /// </summary>
        protected float FRONT_WHEEL_RADIUS;
        /// <summary>
        /// Caster wheel radius
        /// </summary>
        protected float CASTER_WHEEL_RADIUS;
        /// <summary>
        /// Front wheels width
        /// </summary>
        protected float FRONT_WHEEL_WIDTH;
        /// <summary>
        /// Caster wheel width
        /// </summary>
        protected float CASTER_WHEEL_WIDTH;
        /// <summary>
        /// distance of the axle from the center of robot
        /// </summary>
        protected float FRONT_AXLE_DEPTH_OFFSET;

        string _wheelMesh;

        /// <summary>
        /// Mesh file for front wheels
        /// </summary>
        public string WheelMesh
        {
            get { return _wheelMesh; }
            set { _wheelMesh = value; }
        }

        bool _isEnabled;

        /// <summary>
        /// True if drive mechanism is enabled
        /// </summary>
        [DataMember]
        [Description("True if the drive mechanism is enabled.")]
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set { _isEnabled = value; }
        }

        float _motorTorqueScaling;

        /// <summary>
        /// Scaling factor to apply to motor torque requests
        /// </summary>
        [DataMember]
        [Description("Scaling factor to apply to motor torgue requests.")]
        public float MotorTorqueScaling
        {
            get { return _motorTorqueScaling; }
            set { _motorTorqueScaling = value; }
        }

        WheelEntity _rightWheel;

        /// <summary>
        /// Right wheel child entity
        /// </summary>
        [DataMember]
        [Description("Right wheel child entity.")]
        public WheelEntity RightWheel
        {
            get { return _rightWheel; }
            set { _rightWheel = value; }
        }
        WheelEntity _leftWheel;

        /// <summary>
        /// Left wheel child entity
        /// </summary>
        [DataMember]
        [Description("Left wheel child entity.")]
        public WheelEntity LeftWheel
        {
            get { return _leftWheel; }
            set { _leftWheel = value; }
        }

        BoxShape _chassisShape;

        /// <summary>
        /// Chassis physics shapes
        /// </summary>
        [DataMember]
        [Description("Chassis physics shapes.")]
        public BoxShape ChassisShape
        {
            get { return _chassisShape; }
            set { _chassisShape = value; }
        }

        SphereShape _casterWheelShape;
        /// <summary>
        /// Caster wheel physics shape
        /// </summary>
        [DataMember]
        [Description("Caster wheel physics shape.")]
        public SphereShape CasterWheelShape
        {
            get { return _casterWheelShape; }
            set { _casterWheelShape = value; }
        }

        double _rotateDegreesAngleThreshold = 0.2f;

        /// <summary>
        /// Threshold, in radians, for stopping rotation
        /// </summary>
        [DataMember]
        [Description("Threshold for stopping scheduled rotation")]
        public double RotateDegreesAngleThreshold
        {
            get { return _rotateDegreesAngleThreshold; }
            set { _rotateDegreesAngleThreshold = value; }
        }

        #endregion

        /// <summary>
        /// Default constructor
        /// </summary>
        public DifferentialDriveEntity() { }

        /// <summary>
        /// Initialization
        /// </summary>
        /// <param name="device"></param>
        /// <param name="physicsEngine"></param>
        public override void Initialize(xnagrfx.GraphicsDevice device, PhysicsEngine physicsEngine)
        {
            try
            {
                InitError = string.Empty;
                ProgrammaticallyBuildModel(device, physicsEngine);
                
                // Set the parent entity for the wheel entities, clear any local rotation
                // on the wheel shape so that the wheel contact is always in the -Y direction.
                _leftWheel.Parent = this;
                _leftWheel.Wheel.State.LocalPose.Orientation = new Quaternion(0,0,0,1);
                _rightWheel.Parent = this;
                _rightWheel.Wheel.State.LocalPose.Orientation = new Quaternion(0, 0, 0, 1);
                _leftWheel.Initialize(device, physicsEngine);
                _rightWheel.Initialize(device, PhysicsEngine);

                base.Initialize(device, physicsEngine);

                _isEnabled = true;
            }
            catch (Exception ex)
            {
                // clean up
                if (PhysicsEntity != null)
                    PhysicsEngine.DeleteEntity(PhysicsEntity);

                HasBeenInitialized = false;
                InitError = ex.ToString();
            }
        }

        /// <summary>
        /// Builds the simulated robotic entity using local fields for positionm size, orientation
        /// </summary>
        /// <param name="device"></param>
        /// <param name="physicsEngine"></param>
        [DataObjectMethod(DataObjectMethodType.Update, true)]
        public virtual void ProgrammaticallyBuildModel(xnagrfx.GraphicsDevice device, PhysicsEngine physicsEngine)
        {
            if (_casterWheelShape == null)
            {
                if (CASTER_WHEEL_RADIUS <= float.Epsilon)
                    CASTER_WHEEL_RADIUS = 0.05f;

                ConstructCasterWheelShape();
            }

            base.State.PhysicsPrimitives.Add(_casterWheelShape);

            if (_chassisShape != null)
                base.State.PhysicsPrimitives.Add(_chassisShape);

            base.CreateAndInsertPhysicsEntity(physicsEngine);
            // increase physics fidelity
            base.PhysicsEntity.SolverIterationCount = 64;

            // if we were created from xml the wheel entities would already be instantiated
            if (_leftWheel != null && _rightWheel != null)
                return;

            ConstructWheels();
        }

        /// <summary>
        /// Call this in the non-default constructor on an entity derived from
        /// DifferentialDriveEntity after setting CASTER_WHEEL_POSITION and 
        /// CASTER_WHEEL_RADIUS
        /// </summary>
        protected void ConstructStateMembers()
        {
            ConstructCasterWheelShape();
            ConstructWheels();
        }

        /// <summary>
        /// Constructs the wheel components
        /// </summary>
        protected void ConstructWheels()
        {
            // front left wheel
            WheelShapeProperties w = new WheelShapeProperties("front left wheel", FRONT_WHEEL_MASS, FRONT_WHEEL_RADIUS);
            // Set this flag on both wheels if you want to use axle speed instead of torque
            w.Flags |= WheelShapeBehavior.OverrideAxleSpeed;
            w.InnerRadius = 0.7f * w.Radius;
            w.LocalPose = new Pose(LEFT_FRONT_WHEEL_POSITION);
            _leftWheel = new WheelEntity(w);
            _leftWheel.State.Name = State.Name + ":" + "Left wheel";
            _leftWheel.State.Assets.Mesh = _wheelMesh;
            _leftWheel.Parent = this;
            //_leftWheel.WheelShape.WheelState.Material = new MaterialProperties("wheel", 0.5f, 0f, 1f);
            // wheels must have zero friction material.The wheel model will do friction differently

            // front right wheel
            w = new WheelShapeProperties("front right wheel", FRONT_WHEEL_MASS, FRONT_WHEEL_RADIUS);
            w.Flags |= WheelShapeBehavior.OverrideAxleSpeed;
            w.InnerRadius = 0.7f * w.Radius;
            w.LocalPose = new Pose(RIGHT_FRONT_WHEEL_POSITION);
            _rightWheel = new WheelEntity(w);
            _rightWheel.State.Name = State.Name + ":" + "Right wheel";
            _rightWheel.State.Assets.Mesh = _wheelMesh;
            _rightWheel.MeshRotation = new Vector3(0, 180, 0);   // flip the wheel mesh
            _rightWheel.Parent = this;
            //_rightWheel.WheelShape.WheelState.Material = _leftWheel.WheelShape.WheelState.Material;
        }

        private void ConstructCasterWheelShape()
        {
            // add caster wheel as a basic sphere shape
            CasterWheelShape = new SphereShape(
                new SphereShapeProperties("rear wheel", 0.001f,
                new Pose(CASTER_WHEEL_POSITION), CASTER_WHEEL_RADIUS));
            CasterWheelShape.State.Name = "Caster wheel";

            // a fixed caster wheel has high friction when moving laterely, but low friction when it moves along the
            // body axis its aligned with. We use anisotropic friction to model this
            CasterWheelShape.State.Material = new MaterialProperties("small friction with anisotropy", 0.5f, 0.5f, 1);
            CasterWheelShape.State.Material.Advanced = new MaterialAdvancedProperties();
            CasterWheelShape.State.Material.Advanced.AnisotropicDynamicFriction = 0.3f;
            CasterWheelShape.State.Material.Advanced.AnisotropicStaticFriction = 0.4f;
            CasterWheelShape.State.Material.Advanced.AnisotropyDirection = new Vector3(0, 0, 1);
        }

        /// <summary>
        /// Special dispose to handle embedded entities
        /// </summary>
        public override void Dispose()
        {
            if (_leftWheel != null)
                _leftWheel.Dispose();

            if (_rightWheel != null)
                _rightWheel.Dispose();

            base.Dispose();
        }

        /// <summary>
        /// Updates pose for our entity. We override default implementation
        /// since we control our own rendering when no file mesh is supplied, which means
        /// we dont need world transform updates
        /// </summary>
        /// <param name="update"></param>
        public override void Update(FrameUpdate update)
        {
            // update state for us and all the shapes that make up the rigid body
            PhysicsEntity.UpdateState(true);

            if (_distanceToTravel > 0)
            {
                // DriveDistance update
                double currentDistance = Vector3.Length(State.Pose.Position - _startPoseForDriveDistance.Position);
                if (currentDistance >= _distanceToTravel)
                {
                    _rightWheel.Wheel.AxleSpeed = 0;
                    _leftWheel.Wheel.AxleSpeed = 0;
                    _leftTargetVelocity = 0;
                    _rightTargetVelocity = 0;
                    _distanceToTravel = 0;
                    // now that we're finished, post a response
                    if (_driveDistancePort != null)
                    {
                        Port<OperationResult> tmp = _driveDistancePort;
                        _driveDistancePort = null;
                        tmp.Post(OperationResult.Completed);
                    }
                }
                else if ((_timeoutSeconds != 0) && (DateTime.Now - _startTime).TotalSeconds > _timeoutSeconds)
                {
                    if (_driveDistancePort != null)
                    {
                        Port<OperationResult> tmp = _driveDistancePort;
                        _driveDistancePort = null;
                        tmp.Post(OperationResult.Error);
                    }
                }
                else
                {
                    // need to drive further, check to see if we should slow down
                    if (progressPoints.Count >= averageKernel)
                    {
                        double distanceRemaining = _distanceToTravel - currentDistance;
                        double framesToCompletion = Math.Abs(distanceRemaining * averageKernel / (currentDistance - progressPoints.Dequeue()));
                        if (framesToCompletion < decelerateThreshold)
                        {
                            if (Math.Abs(_leftTargetVelocity) > 0.1)
                            {
                                _leftTargetVelocity *= 0.5f;
                                _rightTargetVelocity *= 0.5f;
                            }
                            progressPoints.Clear();
                        }
                    }
                    progressPoints.Enqueue(currentDistance);
                }
            }
            else if (_targetRotation != double.MaxValue)
            {
                // RotateDegrees update
                float currentHeading = CurrentHeading;
                double angleDelta = currentHeading - _previousHeading;
                while (angleDelta > Math.PI)
                    angleDelta -= twoPI;
                while (angleDelta <= -Math.PI)
                    angleDelta += twoPI;
                _currentRotation += angleDelta;
                _previousHeading = currentHeading;  // for next frame

                float angleError;
                if (_targetRotation < 0)
                    angleError = (float)(_currentRotation - _targetRotation);
                else
                    angleError = (float)(_targetRotation - _currentRotation);

                if (angleError < acceptableRotationError)
                {
                    // current heading is within acceptableError or has overshot
                    // end the rotation
                    _targetRotation = double.MaxValue;
                    _rightWheel.Wheel.AxleSpeed = 0;
                    _leftWheel.Wheel.AxleSpeed = 0;
                    _leftTargetVelocity = 0;
                    _rightTargetVelocity = 0;
                    // now that we're finished, post a response
                    if (_rotateDegreesPort != null)
                    {
                        Port<OperationResult> tmp = _rotateDegreesPort;
                        _rotateDegreesPort = null;
                        tmp.Post(OperationResult.Completed);
                    }
                }
                else if ((_timeoutSeconds != 0) && (DateTime.Now - _startTime).TotalSeconds > _timeoutSeconds)
                {
                    if (_rotateDegreesPort != null)
                    {
                        Port<OperationResult> tmp = _rotateDegreesPort;
                        _rotateDegreesPort = null;
                        tmp.Post(OperationResult.Error);
                    }
                }
                else
                {
                    if (angleDelta != 0)
                    {
                        // need to turn more, check to see if we should slow down
                        if (progressPoints.Count >= averageKernel)
                        {
                            double framesToCompletion = Math.Abs(angleError * averageKernel / (_currentRotation - progressPoints.Dequeue()));
                            if (framesToCompletion < decelerateThreshold)
                            {
                                if (Math.Abs(_leftTargetVelocity) > 0.1)
                                    _leftTargetVelocity *= 0.5f;

                                if (Math.Abs(_rightTargetVelocity) > 0.1)
                                    _rightTargetVelocity *= 0.5f;
                                progressPoints.Clear();
                            }
                        }
                        progressPoints.Enqueue(_currentRotation);
                    }
                }
            }

            float leftError = _leftWheel.Wheel.AxleSpeed + _leftTargetVelocity;
            float rightError = _rightWheel.Wheel.AxleSpeed + _rightTargetVelocity;

            if (Math.Abs(leftError) > SPEED_DELTA)
            {
                if (leftError > 0)
                    _leftWheel.Wheel.AxleSpeed -= SPEED_DELTA;
                else
                    _leftWheel.Wheel.AxleSpeed += SPEED_DELTA;
            }
            else
            {
                _leftWheel.Wheel.AxleSpeed = -_leftTargetVelocity;
            }

            if (Math.Abs(rightError) > SPEED_DELTA)
            {
                if (rightError > 0)
                    _rightWheel.Wheel.AxleSpeed -= SPEED_DELTA;
                else
                    _rightWheel.Wheel.AxleSpeed += SPEED_DELTA;
            }
            else
            {
                _rightWheel.Wheel.AxleSpeed = -_rightTargetVelocity;
            }

            // if the drive is not enabled, it should not be moving.
            if (!_isEnabled)
            {
                _leftWheel.Wheel.AxleSpeed = 0;
                _rightWheel.Wheel.AxleSpeed = 0;

                // cancel any pending operations
                if ((_driveDistancePort != null) || (_rotateDegreesPort != null))
                    ResetRotationAndDistance();
            }

            // update entities in fields
            _leftWheel.Update(update);
            _rightWheel.Update(update);

            // sim engine will update children
            base.Update(update);
        }

        /// <summary>
        /// Render entities stored as fields
        /// </summary>
        public override void Render(RenderMode renderMode, MatrixTransforms transforms, CameraEntity currentCamera)
        {
            var entityEffect = _leftWheel.Effect;
            if (currentCamera.LensEffect != null)
            {
                _leftWheel.Effect = currentCamera.LensEffect;
                _rightWheel.Effect = currentCamera.LensEffect;
            }

            _leftWheel.Render(renderMode, transforms, currentCamera);
            _rightWheel.Render(renderMode, transforms, currentCamera);

            _leftWheel.Effect = entityEffect;
            _rightWheel.Effect = entityEffect;

            base.Render(renderMode, transforms, currentCamera);
        }

        #region Motor Base Control

        const float SPEED_DELTA = 0.5f;

        // DriveDistance variables
        Pose _startPoseForDriveDistance;
        double _distanceToTravel;
        Port<OperationResult> _driveDistancePort = null;

        /// <summary>
        /// Applies constant power to both wheels, driving the motor base for a fixed distance, in the current direction
        /// </summary>
        /// <param name="distance">Distance to travel, in meters</param>
        /// <param name="power">Normalized power (torque) value for both wheels</param>
        /// <param name="responsePort">A port to report the result of the request, success or exception</param>
        [CLSCompliant(false)]
        public void DriveDistance(float distance, float power, Port<OperationResult> responsePort)
        {
            if (!_isEnabled)
            {
                responsePort.Post(OperationResult.Error);
                return;
            }

            // reset any drivedistance or rotate degrees commands that haven't completed
            ResetRotationAndDistance();

            // keep track of the response port for when we complete the request
            _driveDistancePort = responsePort;

            // handle negative distances
            if (distance < 0)
            {
                distance = -distance;
                power = -power;
            }
            _startPoseForDriveDistance = State.Pose;
            _distanceToTravel = distance;
            SetAxleVelocity(power * _motorTorqueScaling, power * _motorTorqueScaling);
            _startTime = DateTime.Now;
        }

        // DriveDistance and RotateDegrees shared variables
        Queue<double> progressPoints = new Queue<double>();
        const int averageKernel = 6;
        const int decelerateThreshold = 6;
        const float twoPI = (float)(2 * Math.PI);
        DateTime _startTime;

        // RotateDegrees variables
        double _targetRotation = double.MaxValue;
        double _currentRotation = 0;
        double _previousHeading = 0;
        const float acceptableRotationError = 0.005f;
        Port<OperationResult> _rotateDegreesPort = null;

        float _timeoutSeconds = 30.0f;

        /// <summary>
        /// The timeout for DriveDistance and RotateDegrees commands in seconds.
        /// </summary>
        [DataMember]
        public float TimeoutSeconds
        {
            get { return _timeoutSeconds; }
            set { _timeoutSeconds = value; }
        }

        /// <summary>
        /// Applies constant power to each wheel (but of inverse polarity), rotating the motor base 
        /// through the given rotation.
        /// </summary>
        /// <param name="degrees">Rotation around Y axis, in degrees.</param>
        /// <param name="power">Normalized power (torque) value for both wheels</param>
        /// <param name="responsePort">A port to report the result of the request, success or exception</param>
        [CLSCompliant(false)]
        public void RotateDegrees(float degrees, float power, Port<OperationResult> responsePort)
        {
            if (!_isEnabled)
            {
                responsePort.Post(OperationResult.Error);
                return;
            }

            // reset any drivedistance or rotate degrees commands that haven't completed
            ResetRotationAndDistance();

            // keep track of the response port for when we complete the request
            _rotateDegreesPort = responsePort;

            _targetRotation = xna.MathHelper.ToRadians(degrees);
            _currentRotation = 0;
            _previousHeading = CurrentHeading;

            if (degrees < 0)
                SetAxleVelocity(power * _motorTorqueScaling, -power * _motorTorqueScaling);
            else
                SetAxleVelocity(-power * _motorTorqueScaling, power * _motorTorqueScaling);

            _startTime = DateTime.Now;
        }

        /// <summary>
        /// Current heading, in radians, of robot base
        /// </summary>
        public float CurrentHeading
        {
            get
            {
                // return the axis angle of the quaternion
                xna.Vector3 euler = UIMath.QuaternionToEuler(State.Pose.Orientation);
                return xna.MathHelper.ToRadians(euler.Y); // heading is the rotation about the Y axis.
            }
        }

        /// <summary>
        /// When a direct update to motor torque or wheel velocity occurs
        /// we abandon any current DriveDistance or RotateDegrees commands
        /// </summary>
        /// <summary>
        /// When a direct update to motor torque or wheel velocity occurs
        /// we abandon any current DriveDistance or RotateDegrees commands
        /// </summary>
        void ResetRotationAndDistance()
        {
            progressPoints.Clear();
            _distanceToTravel = 0;
            _targetRotation = double.MaxValue;
            if (_driveDistancePort != null)
            {
                _driveDistancePort.Post(OperationResult.Canceled);
                _driveDistancePort = null;
            }
            if (_rotateDegreesPort != null)
            {
                _rotateDegreesPort.Post(OperationResult.Canceled);
                _rotateDegreesPort = null;
            }
        }

        /// <summary>
        /// Sets motor torque on the active wheels
        /// </summary>
        /// <param name="leftWheel"></param>
        /// <param name="rightWheel"></param>
        public void SetMotorTorque(float leftWheel, float rightWheel)
        {
            if (_leftWheel == null || _rightWheel == null)
                return;

            // convert to velocity and call SetVelocity
            SetVelocity(leftWheel * _motorTorqueScaling * _leftWheel.Wheel.State.Radius,
                rightWheel * _motorTorqueScaling * _rightWheel.Wheel.State.Radius);
        }

        float _leftTargetVelocity;
        float _rightTargetVelocity;

        /// <summary>
        /// Sets angular velocity (radians/sec) on both wheels
        /// </summary>
        /// <param name="value"></param>
        public void SetVelocity(float value)
        {
            ResetRotationAndDistance();
            SetVelocity(value, value);
        }

        /// <summary>
        /// Sets angular velocity on the wheels
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        public void SetVelocity(float left, float right)
        {
            ResetRotationAndDistance();
            if (_leftWheel == null || _rightWheel == null)
                return;

            left = ValidateWheelVelocity(left);
            right = ValidateWheelVelocity(right);

            // v is in m/sec - convert to an axle speed
            //  2Pi(V/2PiR) = V/R
            SetAxleVelocity(left / _leftWheel.Wheel.State.Radius,
                right / _rightWheel.Wheel.State.Radius);
        }

        private void SetAxleVelocity(float left, float right)
        {
            // if not enabled, don't move.
            if (!_isEnabled)
                left = right = 0;

            _leftTargetVelocity = left;
            _rightTargetVelocity = right;
        }

        const float MAX_VELOCITY = 20.0f;
        const float MIN_VELOCITY = -MAX_VELOCITY;

        float ValidateWheelVelocity(float value)
        {
            if (value > MAX_VELOCITY)
                return MAX_VELOCITY;
            if (value < MIN_VELOCITY)
                return MIN_VELOCITY;

            return value;
        }
        #endregion
    }

    /// <summary>
    /// MotorBase is an implementation of the differential drive entity. 
    /// </summary>
    [DataContract]
    [CLSCompliant(true)]
    public class MotorBase : DifferentialDriveEntity
    {
         /// <summary>
        /// Default constructor
        /// </summary>
        public MotorBase() { }

        /// <summary>
        /// Initialization constructor
        /// </summary>
        /// <param name="initialPos"></param>
        public MotorBase(Vector3 initialPos)
        {
            MASS = 9;
            CHASSIS_DIMENSIONS = new Vector3(0.393f, 0.18f, 0.40f);
            CHASSIS_CLEARANCE = 0.05f;
            FRONT_WHEEL_RADIUS = 0.08f;
            CASTER_WHEEL_RADIUS = 0.025f; // = CHASSIS_CLEARANCE / 2; // to keep things simple we make caster a bit bigger
            FRONT_WHEEL_WIDTH = 4.74f;  //not used
            CASTER_WHEEL_WIDTH = 0.02f; //not used
            FRONT_AXLE_DEPTH_OFFSET = -0.05f; // distance of the axle from the center of robot

            base.State.Name = "MotorBaseWithThreeWheels";
            base.State.MassDensity.Mass = MASS;
            base.State.Pose.Position = initialPos;

            // reference point for all shapes is the projection of
            // the center of mass onto the ground plane
            // (basically the spot under the center of mass, at Y = 0, or ground level)

            // chassis position
            BoxShapeProperties motorBaseDesc = new BoxShapeProperties("chassis", MASS,
                new Pose(new Vector3(
                0, // Chassis center is also the robot center, so use zero for the X axis offset
                CHASSIS_CLEARANCE + CHASSIS_DIMENSIONS.Y / 2, // chassis is off the ground and its center is DIM.Y/2 above the clearance
                0)), // no offset in the z/depth axis, since again, its center is the robot center
                CHASSIS_DIMENSIONS);

            motorBaseDesc.Material = new MaterialProperties("high friction", 0.0f, 1.0f, 20.0f);
            motorBaseDesc.Name = "Chassis";
            ChassisShape = new BoxShape(motorBaseDesc);

            // rear wheel is also called the caster
            CASTER_WHEEL_POSITION = new Vector3(0, // center of chassis
                CASTER_WHEEL_RADIUS, // distance from ground
                CHASSIS_DIMENSIONS.Z / 2 - CASTER_WHEEL_RADIUS); // all the way at the back of the robot

            // NOTE: right/left is from the perspective of the robot, looking forward

            FRONT_WHEEL_MASS = 0.10f;

            RIGHT_FRONT_WHEEL_POSITION = new Vector3(
                CHASSIS_DIMENSIONS.X / 2 + 0.01f - 0.05f,// left of center
                FRONT_WHEEL_RADIUS,// distance from ground of axle
                FRONT_AXLE_DEPTH_OFFSET); // distance from center, on the z-axis

            LEFT_FRONT_WHEEL_POSITION = new Vector3(
                -CHASSIS_DIMENSIONS.X / 2 - 0.01f + 0.05f,// right of center
                FRONT_WHEEL_RADIUS,// distance from ground of axle
                FRONT_AXLE_DEPTH_OFFSET); // distance from center, on the z-axis

            MotorTorqueScaling = 20;

            ConstructStateMembers();
        }
    }

    /// <summary>
    /// MobileRobots Pioneer3DX implementation of the differential entity. It just specifies different physical properties in
    /// its custom constructor, otherwise uses the base class as is.
    /// </summary>
    [DataContract]
    [CLSCompliant(true)]
    public class Pioneer3DX : DifferentialDriveEntity
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public Pioneer3DX() { }

        /// <summary>
        /// Initialization constructor
        /// </summary>
        /// <param name="initialPos"></param>
        public Pioneer3DX(Vector3 initialPos)
        {
            MASS = 9;
            // the default settings are approximating a Pioneer 3-DX activMedia robot
            CHASSIS_DIMENSIONS = new Vector3(0.393f, 0.18f, 0.40f);
            CHASSIS_CLEARANCE = 0.05f;
            FRONT_WHEEL_RADIUS = 0.08f;
            CASTER_WHEEL_RADIUS = 0.025f; // = CHASSIS_CLEARANCE / 2; // to keep things simple we make caster a bit bigger
            FRONT_WHEEL_WIDTH = 4.74f;  //not used
            CASTER_WHEEL_WIDTH = 0.02f; //not used
            FRONT_AXLE_DEPTH_OFFSET = -0.05f; // distance of the axle from the center of robot

            base.State.Name = "MotorBaseWithThreeWheels";
            base.State.MassDensity.Mass = MASS;
            base.State.Pose.Position = initialPos;
            base.State.Assets.Mesh = "Pioneer3dx.bos";
            base.WheelMesh = "PioneerWheel.bos";

            // reference point for all shapes is the projection of
            // the center of mass onto the ground plane
            // (basically the spot under the center of mass, at Y = 0, or ground level)

            // chassis position
            BoxShapeProperties motorBaseDesc = new BoxShapeProperties("chassis", MASS,
                new Pose(new Vector3(
                0, // Chassis center is also the robot center, so use zero for the X axis offset
                CHASSIS_CLEARANCE + CHASSIS_DIMENSIONS.Y / 2, // chassis is off the ground and its center is DIM.Y/2 above the clearance
                0)), // no offset in the z/depth axis, since again, its center is the robot center
                CHASSIS_DIMENSIONS);

            motorBaseDesc.Material = new MaterialProperties("high friction", 0.0f, 1.0f, 20.0f);
            motorBaseDesc.Name = "Chassis";
            ChassisShape = new BoxShape(motorBaseDesc);

            // rear wheel is also called the caster
            CASTER_WHEEL_POSITION = new Vector3(0, // center of chassis
                CASTER_WHEEL_RADIUS, // distance from ground
                CHASSIS_DIMENSIONS.Z / 2 - CASTER_WHEEL_RADIUS); // all the way at the back of the robot

            // NOTE: right/left is from the perspective of the robot, looking forward

            FRONT_WHEEL_MASS = 0.10f;

            RIGHT_FRONT_WHEEL_POSITION = new Vector3(
                CHASSIS_DIMENSIONS.X / 2 + 0.01f - 0.05f,// left of center
                FRONT_WHEEL_RADIUS,// distance from ground of axle
                FRONT_AXLE_DEPTH_OFFSET); // distance from center, on the z-axis

            LEFT_FRONT_WHEEL_POSITION = new Vector3(
                -CHASSIS_DIMENSIONS.X / 2 - 0.01f + 0.05f,// right of center
                FRONT_WHEEL_RADIUS,// distance from ground of axle
                FRONT_AXLE_DEPTH_OFFSET); // distance from center, on the z-axis

            MotorTorqueScaling = 20;

            ConstructStateMembers();
        }
    }

    /// <summary>
    /// Lego NXT variant of the motor base entity. It just specifies different physical properties in
    /// its custom constructor, otherwise uses the base class as is.
    /// </summary>
    [DataContract]
    [CLSCompliant(true)]
    public class LegoNXTTribot : DifferentialDriveEntity
    {
        /// <summary>
        /// Default constructor, used for creating the entity from an XML description
        /// </summary>
        public LegoNXTTribot() { }

        /// <summary>
        /// Custom constructor for building model from hardcoded values. Used to create entity programmatically
        /// </summary>
        /// <param name="initialPos"></param>
        public LegoNXTTribot(Vector3 initialPos)
        {
            MASS = 0.5f; //kg
            // the default settings approximate the Lego NXT baseline chassis
            CHASSIS_DIMENSIONS = new Vector3(0.105f, //meters wide
                                             0.12f,  //meters high
                                             0.14f); //meters long
            FRONT_WHEEL_MASS = 0.01f;
            CHASSIS_CLEARANCE = 0.015f;
            FRONT_WHEEL_RADIUS = 0.025f;
            CASTER_WHEEL_RADIUS = 0.0125f;
            FRONT_WHEEL_WIDTH = 0.028f;
            CASTER_WHEEL_WIDTH = 0.008f; //not currently used, but dim is accurate
            FRONT_AXLE_DEPTH_OFFSET = -0.005f; // distance of the axle from the center of robot

            base.State.Name = "LegoNXTTribot";
            base.State.MassDensity.Mass = MASS;
            base.State.Pose.Position = initialPos;
            base.State.Assets.Mesh = "LegoNXTTribot.bos";
            base.WheelMesh = "LegoNXTTribotWheel.bos";

            // reference point for all shapes is the projection of
            // the center of mass onto the ground plane
            // (basically the spot under the center of mass, at Y = 0, or ground level)

            // NOTE: right/left is from the perspective of the robot, looking forward
            // NOTE: X = width of robot (right to left), Y = height, Z = length

            // chassis position
            BoxShapeProperties motorBaseDesc = new BoxShapeProperties("NXT brick", MASS,
                new Pose(new Vector3(
                0, // Chassis center is also the robot center, so use zero for the X axis offset
                CHASSIS_CLEARANCE + CHASSIS_DIMENSIONS.Y / 2, // chassis is off the ground and its center is DIM.Y/2 above the clearance
                0.025f)), // minor offset in the z/depth axis
                CHASSIS_DIMENSIONS);

            motorBaseDesc.Material = new MaterialProperties("high friction", 0.0f, 1.0f, 20.0f);
            motorBaseDesc.Name = "Chassis";
            ChassisShape = new BoxShape(motorBaseDesc);

            // rear wheel is also called the caster
            CASTER_WHEEL_POSITION = new Vector3(0, // center of chassis
                CASTER_WHEEL_RADIUS, // distance from ground
                CHASSIS_DIMENSIONS.Z / 2 + 0.055f); // all the way at the back of the robot

            // Deleted the extra offset of  +- FRONT_WHEEL_WIDTH / 2
            RIGHT_FRONT_WHEEL_POSITION = new Vector3(
                +CHASSIS_DIMENSIONS.X / 2,// left of center
                FRONT_WHEEL_RADIUS,// distance from ground of axle
                FRONT_AXLE_DEPTH_OFFSET); // distance from center, on the z-axis

            LEFT_FRONT_WHEEL_POSITION = new Vector3(
                -CHASSIS_DIMENSIONS.X / 2,// right of center
                FRONT_WHEEL_RADIUS,// distance from ground of axle
                FRONT_AXLE_DEPTH_OFFSET); // distance from center, on the z-axis

            MotorTorqueScaling = 30;

            ConstructStateMembers();
        }
    }

    /// <summary>
    /// IRobotCreate variant of the motor base entity. It just specifies different physical properties in
    /// its custom constructor, otherwise uses the base class as is.
    /// </summary>
    [DataContract]
    [CLSCompliant(true)]
    public class IRobotCreate : DifferentialDriveEntity
    {
        Port<EntityContactNotification> _notifications = new Port<EntityContactNotification>();

        /// <summary>
        /// Default constructor, used for creating the entity from an XML description
        /// </summary>
        public IRobotCreate() { }

        /// <summary>
        /// Custom constructor for building model from hardcoded values. Used to create entity programmatically
        /// </summary>
        /// <param name="initialPos"></param>
        public IRobotCreate(Vector3 initialPos)
        {
            MASS = 2.27f; //kg  (around 5 pounds)
            // the default settings approximate the Lego NXT baseline chassis
            CHASSIS_DIMENSIONS = new Vector3(0.31f, //meters wide
                                             0.06f,  //meters high
                                             0.26f); //meters long
            FRONT_WHEEL_MASS = 0.01f;
            CHASSIS_CLEARANCE = 0.025f;
            FRONT_WHEEL_RADIUS = 0.025f;
            CASTER_WHEEL_RADIUS = 0.0125f;
            FRONT_WHEEL_WIDTH = 0.07f;
            CASTER_WHEEL_WIDTH = 0.03f; //not currently used, but dim is accurate
            FRONT_AXLE_DEPTH_OFFSET = 0.01f; // distance of the axle from the center of robot

            base.State.Name = "IRobotCreate";
            base.State.MassDensity.Mass = MASS;
            //base.State.MassDensity.CenterOfMass = new Pose(
            //    new Vector3(0, CHASSIS_CLEARANCE, 0));
            base.State.Pose.Position = initialPos;

            // reference point for all shapes is the projection of
            // the center of mass onto the ground plane
            // (basically the spot under the center of mass, at Y = 0, or ground level)

            // NOTE: right/left is from the perspective of the robot, looking forward
            // NOTE: X = width of robot (right to left), Y = height, Z = length

            // chassis position
            BoxShapeProperties motorBaseDesc = new BoxShapeProperties("Create Body", MASS,
                new Pose(new Vector3(
                0, // Chassis center is also the robot center, so use zero for the X axis offset
                CHASSIS_CLEARANCE + CHASSIS_DIMENSIONS.Y / 2, // chassis is off the ground and its center is DIM.Y/2 above the clearance
                0.03f)), // minor offset in the z/depth axis
                CHASSIS_DIMENSIONS);

            ChassisShape = null;

            // rear wheel is also called the caster
            CASTER_WHEEL_POSITION = new Vector3(0, // center of chassis
                CASTER_WHEEL_RADIUS, // distance from ground
                CHASSIS_DIMENSIONS.Z / 2); // at the rear of the robot

            RIGHT_FRONT_WHEEL_POSITION = new Vector3(
                +CHASSIS_DIMENSIONS.X / 2 - FRONT_WHEEL_WIDTH / 2 + 0.01f,// left of center
                FRONT_WHEEL_RADIUS,// distance from ground of axle
                FRONT_AXLE_DEPTH_OFFSET); // distance from center, on the z-axis

            LEFT_FRONT_WHEEL_POSITION = new Vector3(
                -CHASSIS_DIMENSIONS.X / 2 + FRONT_WHEEL_WIDTH / 2 - 0.01f,// right of center
                FRONT_WHEEL_RADIUS,// distance from ground of axle
                FRONT_AXLE_DEPTH_OFFSET); // distance from center, on the z-axis

            MotorTorqueScaling = 20;

            // specify a default mesh because we can't generate a mesh from the convex mesh shape.
            State.Assets.Mesh = "iRobot-Create.bos";

            ConstructStateMembers();
        }

        /// <summary>
        /// Initialization
        /// </summary>
        /// <param name="device"></param>
        /// <param name="physicsEngine"></param>
        public override void Initialize(Microsoft.Xna.Framework.Graphics.GraphicsDevice device, PhysicsEngine physicsEngine)
        {
            // add a front wheel
            Vector3 FRONT_WHEEL_POSITION = new Vector3(0, // center of chassis
                CASTER_WHEEL_RADIUS, // distance from ground
                -CHASSIS_DIMENSIONS.Z / 2 + 0.000f); // at the front of the robot

            if (_frontWheelShape == null)
            {
                _frontWheelShape = new SphereShape(
                    new SphereShapeProperties("front wheel", 0.001f,
                    new Pose(FRONT_WHEEL_POSITION), CASTER_WHEEL_RADIUS));
                _frontWheelShape.State.Name = "front wheel";

                // a fixed caster wheel has high friction when moving laterely, but low friction when it moves along the
                // body axis its aligned with. We use anisotropic friction to model this
                _frontWheelShape.State.Material = new MaterialProperties("small friction with anisotropy", 0.5f, 0.5f, 1);
                _frontWheelShape.State.Material.Advanced = new MaterialAdvancedProperties();
                _frontWheelShape.State.Material.Advanced.AnisotropicDynamicFriction = 0.3f;
                _frontWheelShape.State.Material.Advanced.AnisotropicStaticFriction = 0.4f;
                _frontWheelShape.State.Material.Advanced.AnisotropyDirection = new Vector3(0, 0, 1);
            }

            State.PhysicsPrimitives.Add(frontWheelShape);

            if (Bumpers == null)
            {
                string[] bumperName = new string[] { "L1", "L2", "L3", "B1", "B2", "R1", "R2", "R3" };

                // add some bumper shapes
                float start = (float)(-0.78 * Math.PI / 2.0f);
                float end = (float)(0.78 * Math.PI / 2.0f);
                int segments = 8; // 8 bumpers
                float increment = (end - start) / (segments - 1);
                int segment = 0;

                _bumpers = new BoxShape[segments];

                for (float angle = start; angle <= end; angle += increment, segment++)
                {
                    BoxShape bumper = new BoxShape(
                        new BoxShapeProperties(
                            bumperName[segment], 0.001f, //mass
                            new Pose(new Vector3((float)(0.16 * Math.Sin(angle)), 0.055f, (float)(-0.16 * Math.Cos(angle))), // position
                            Quaternion.FromAxisAngle(0, 1, 0, -angle)), // rotation
                        new Vector3(0.053f, 0.06f, 0.01f))); // dimensions

                    bumper.BoxState.Material = new MaterialProperties("iRobotCreate chassis", 0.0f, 0.25f, 0.5f);

                    bumper.Parent = this;
                    bumper.BoxState.EnableContactNotifications = true;
                    if (bumperName[segment][0] == 'L')
                        bumper.State.DiffuseColor.X = 1.0f;
                    else if (bumperName[segment][0] == 'R')
                        bumper.State.DiffuseColor.Y = 1.0f;
                    else if (bumperName[segment][0] == 'B')
                    {
                        bumper.State.DiffuseColor.X = 1.0f;
                        bumper.State.DiffuseColor.Y = 1.0f;
                    }
                    _bumpers[segment] = bumper;
                }
            }

            foreach (BoxShape bumper in _bumpers)
                State.PhysicsPrimitives.Add(bumper);


            // load a low-LOD mesh for the device and create a convex shape around it
            string mesh = State.Assets.Mesh;
            State.Assets.Mesh = "iRobot-Create-LOD.obj";
            LoadResources(device);

            if (Meshes.Count >= 1)
            {
                // Our only shape is a convex mesh using our render mesh index and vertex data for
                // physics shape calculations
                ConvexMeshShape convexMeshShape = null;
                convexMeshShape = VisualEntityMesh.CreateConvexMeshShape(Meshes[0], State.Assets.Mesh, State.MassDensity.Mass);
                convexMeshShape.State.Material = new MaterialProperties("iRobotCreate chassis", 0.0f, 0.25f, 0.5f);
                convexMeshShape.State.Name = "chassis";

                base.State.PhysicsPrimitives.Add(convexMeshShape);
            }
            // get rid of the simplified mesh used to create the physics shape
            State.Assets.Mesh = mesh;
            Meshes.Clear();
            base.Initialize(device, physicsEngine);
        }

        /// <summary>
        /// Adds a notification port to the list of subscriptions that get notified when the bumper shapes
        /// collide in the physics world
        /// </summary>
        /// <param name="notificationTarget"></param>
        public void Subscribe(Port<EntityContactNotification> notificationTarget)
        {
            PhysicsEntity.SubscribeForContacts(notificationTarget);
        }

        SphereShape _frontWheelShape;
        /// <summary>
        /// front wheel physics shape
        /// </summary>
        [DataMember]
        [Description("Front wheel physics shape.")]
        public SphereShape frontWheelShape
        {
            get { return _frontWheelShape; }
            set { _frontWheelShape = value; }
        }

        BoxShape[] _bumpers;
        /// <summary>
        /// Shapes for each bumper
        /// </summary>
        [DataMember]
        [Description("Shapes for each bumper.")]
        public BoxShape[] Bumpers
        {
            get { return _bumpers; }
            set { _bumpers = value; }
        }

        bool _playButtonPressed = false;
        /// <summary>
        /// Indicates that the Play button is pressed.
        /// </summary>
        [DataMember]
        [Description("Indicates that the Play button is pressed.")]
        [Category("Buttons")]
        public bool PlayButtonPressed
        {
            get { return _playButtonPressed; }
            set { _playButtonPressed = value; }
        }

        bool _advanceButtonPressed = false;
        /// <summary>
        /// Indicates that the Advance button is pressed.
        /// </summary>
        [DataMember]
        [Description("Indicates that the Advance button is pressed.")]
        [Category("Buttons")]
        public bool AdvanceButtonPressed
        {
            get { return _advanceButtonPressed; }
            set { _advanceButtonPressed = value; }
        }

        int _cliffLeft = 0;
        /// <summary>
        /// The analog value of the left cliff sensor.
        /// </summary>
        [DataMember]
        [Description("The analog value of the left cliff sensor.")]
        [Category("Cliff Sensors")]
        public int CliffLeft
        {
            get { return _cliffLeft; }
            set { _cliffLeft = value; }
        }

        int _cliffFrontLeft = 0;
        /// <summary>
        /// The analog value of the front left cliff sensor.
        /// </summary>
        [DataMember]
        [Description("The analog value of the front left cliff sensor.")]
        [Category("Cliff Sensors")]
        public int CliffFrontLeft
        {
            get { return _cliffFrontLeft; }
            set { _cliffFrontLeft = value; }
        }

        int _cliffRight = 0;
        /// <summary>
        /// The analog value of the right cliff sensor.
        /// </summary>
        [DataMember]
        [Description("The analog value of the right cliff sensor.")]
        [Category("Cliff Sensors")]
        public int CliffRight
        {
            get { return _cliffRight; }
            set { _cliffRight = value; }
        }

        int _cliffFrontRight = 0;
        /// <summary>
        /// The analog value of the front left cliff sensor.
        /// </summary>
        [DataMember]
        [Description("The analog value of the front left cliff sensor.")]
        [Category("Cliff Sensors")]
        public int CliffFrontRight
        {
            get { return _cliffFrontRight; }
            set { _cliffFrontRight = value; }
        }

        bool _bumperLeft = false;
        /// <summary>
        /// Indicates that the left bumper is in contact with another object.
        /// </summary>
        [DataMember]
        [Description("Indicates that the left bumper is in contact with another object.")]
        [Category("Bumpers")]
        public bool BumperLeft
        {
            get { return _bumperLeft; }
            set { _bumperLeft = value; }
        }

        bool _bumperRight = false;
        /// <summary>
        /// Indicates that the right bumper is in contact with another object.
        /// </summary>
        [DataMember]
        [Description("Indicates that the right bumper is in contact with another object.")]
        [Category("Bumpers")]
        public bool BumperRight
        {
            get { return _bumperRight; }
            set { _bumperRight = value; }
        }
    }

    #endregion

    #region Environment

    /// <summary>
    /// Rendering only (not part of physics simulation) entity used to render a sky
    /// </summary>
    [CLSCompliant(true)]
    [DataContract]
    [EditorAttribute(typeof(GenericObjectEditor), typeof(System.Drawing.Design.UITypeEditor))]
    public class SkyEntity : VisualEntity
    {
        private string _visualTextureFileName;
        /// <summary>
        /// Filename of cube map used to draw the sky.
        /// </summary>
        [DataMember]
        [Description("Filename of cube map used to draw the sky.")]
        [Editor(typeof(DDSOpenFileNameEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [Category("Textures")]
        public string VisualTexture
        {
            get { return _visualTextureFileName; }
            set
            {
                _visualTextureFileName = value;
                if (HasBeenInitialized || ((InitError != null) && (InitError != String.Empty)))
                    SimulationEngine.GlobalInstance.RefreshEntity(this);
            }
        }

        /// <summary>
        /// Cube map used to draw the sky
        /// </summary>
        [Browsable(false)]
        public xnagrfx.TextureCube VisualCubeTexture
        {
            get
            {
                if (Meshes.Count > 0 && Meshes[0].Textures.Length > 0)
                    return Meshes[0].Textures[0] as xnagrfx.TextureCube;
                else
                    return null;
            }
            set
            {
                if (Meshes.Count > 0 && Meshes[0].Textures.Length > 0)
                    Meshes[0].Textures[0] = value as xnagrfx.TextureCube;
            }
        }

        private string _lightingTextureFileName;
        /// <summary>
        /// Filename of cube map used to diffuse light objects in the scene
        /// </summary>
        [DataMember]
        [Description("Filename of cube map used to diffuse light objects in the scene.")]
        [Editor(typeof(DDSOpenFileNameEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [Category("Textures")]
        public String LightingTexture
        {
            get { return _lightingTextureFileName; }
            set
            {
                _lightingTextureFileName = value;
                if (HasBeenInitialized || ((InitError != null) && (InitError != String.Empty)))
                    SimulationEngine.GlobalInstance.RefreshEntity(this);
            }
        }

        private xnagrfx.TextureCube _lightingCubeTexture;
        /// <summary>
        /// Cube map used to diffuse light objects in the scene
        /// </summary>
        [BrowsableAttribute(false)]
        public xnagrfx.TextureCube LightingCubeTexture
        {
            get { return _lightingCubeTexture; }
            set { _lightingCubeTexture = value; }
        }


        #region Fog Parameters
        private ColorValue _fogColor = new ColorValue(.85f, .85f, 1, 1);
        /// <summary>
        /// Get or set fog color of the sky
        /// </summary>
        [DataMember]
        [Browsable(true), Category("Fog"), Description("Color that objects fade to in the distance")]
        [Editor(typeof(HDRColorUIEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public ColorValue FogColor
        {
            get { return _fogColor; }
            set { _fogColor = value; }
        }

        private float _fogStart = 100f;
        /// <summary>
        /// Get or set the point at which fog starts
        /// </summary>
        [DataMember]
        [Browsable(true), Category("Fog"), Description("Starting distance of fog")]
        public float FogStart
        {
            get { return _fogStart; }
            set { _fogStart = value; SimulationEngine.GlobalInstance.RequestShaderReload(); }
        }

        private float _fogEnd = 1000f;
        /// <summary>
        /// Get or set the point at which fog ends
        /// </summary>
        [DataMember]
        [Browsable(true), Category("Fog"), Description("Ending distance of fog")]
        public float FogEnd
        {
            get { return _fogEnd; }
            set { _fogEnd = value; SimulationEngine.GlobalInstance.RequestShaderReload(); }
        }
        #endregion

        /// <summary>
        /// Default constructor
        /// </summary>
        public SkyEntity()
        {

        }

        /// <summary>
        /// Initialization constructor
        /// </summary>
        /// <param name="visualCubeTextureFile">filename of texture used to draw the sky</param>
        /// <param name="lightingCubeTextureFile">filename of texture used to light the scene</param>
        public SkyEntity([DefaultParameterValue("sky.dds")] string visualCubeTextureFile, 
            [DefaultParameterValue("sky_diff.dds")] string lightingCubeTextureFile)
        {
            State.Name = "Sky";
            _visualTextureFileName = visualCubeTextureFile;
            _lightingTextureFileName = lightingCubeTextureFile;
        }

        /// <summary>
        /// Positions the sky so that it is always centered around the camera before rendering
        /// </summary>
        public override void Render(VisualEntity.RenderMode renderMode, MatrixTransforms transforms, CameraEntity currentCamera)
        {
            // Scale the sky so that it won't be clipped by the near plane but no so much that it clips the far plane (hopefully)
            World = xna.Matrix.CreateScale(currentCamera.Near * 2);

            xna.Matrix oldView = transforms.View;
            transforms.View.Translation = xna.Vector3.Zero;
            base.Render(renderMode, transforms, currentCamera);
            transforms.View = oldView;
        }

        /// <summary>
        /// Initialization
        /// </summary>
        /// <param name="device"></param>
        /// <param name="physicsEngine"></param>
        public override void Initialize(xnagrfx.GraphicsDevice device, PhysicsEngine physicsEngine)
        {
            try
            {
                InitError = string.Empty;
                Device = device;    // cache a copy of the current device

                if (string.IsNullOrEmpty(State.Assets.Effect))
                {
                    State.Assets.Effect = "SkyBox.fx";
                }

                if (Effect == null)
                    Effect = ResourceCache.GlobalInstance.CreateEffectFromFile(device, State.Assets.Effect);

                // creating the sphere using the helper function
                // this requires a fake SphereShapeProperties structure
                // the alternative is to create a sphere obj file, but this way it's cleaner

                World = xna.Matrix.Identity;

                xnagrfx.Texture visualTexture = ResourceCache.GlobalInstance.CreateTextureFromFile(device, VisualTexture);
                if ((visualTexture != null) && !(visualTexture is xnagrfx.TextureCube))
                    throw new InvalidOperationException("SkyEntity's visual texture must be a cube map");

                Robotics.Simulation.Physics.SphereShapeProperties sphere = new Robotics.Simulation.Physics.SphereShapeProperties();
                sphere.Radius = 1.0f;
                Meshes.Add(new VisualEntityMesh(device, sphere));
                if ((Meshes.Count > 0) && (Meshes[0].Textures != null) && (Meshes[0].Textures.Length > 0))
                    Meshes[0].Textures[0] = visualTexture;

                LightingCubeTexture = ResourceCache.GlobalInstance.CreateCubeTextureFromFile(device, LightingTexture);

                CompactVertexBuffer(device);
                HasBeenInitialized = true;
            }
            catch (Exception ex)
            {
                HasBeenInitialized = false;
                InitError = ex.ToString();
            }
        }

        /// <summary>
        /// Old scene files will deserialize FogStart and FogEnd to 0.
        /// These are bad values for fog, so give them good defaults.
        /// </summary>
        public override void PostDeserialize()
        {
            base.PostDeserialize();

            if (FogStart == 0 && FogEnd == 0)
            {
                FogStart = 100;
                FogEnd = 1000;
                FogColor = new ColorValue(.85f, .85f, 1, 1);
            }
        }
    }

    /// <summary>
    /// Sky entity that uses a 2D dome texture rather than a cube map.
    /// </summary>
    [CLSCompliant(true)]
    [DataContract]
    [EditorAttribute(typeof(GenericObjectEditor), typeof(System.Drawing.Design.UITypeEditor))]
    public class SkyDomeEntity : SkyEntity
    {
        #region Initialization
        /// <summary>
        /// Default empty constructor
        /// </summary>
        public SkyDomeEntity()
        {
        }

        /// <summary>
        /// Initialization constructor
        /// </summary>
        public SkyDomeEntity([DefaultParameterValue("skydome.dds")] string visualTextureFile, 
            [DefaultParameterValue("sky_diff.dds")] string lightingCubeTextureFile)
            : base(visualTextureFile, lightingCubeTextureFile)
        {
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="device"></param>
        /// <param name="physicsEngine"></param>
        public override void Initialize(Microsoft.Xna.Framework.Graphics.GraphicsDevice device, PhysicsEngine physicsEngine)
        {
            try
            {
                InitError = string.Empty;
                Device = device;    // cache a copy of the current device

                if (string.IsNullOrEmpty(State.Assets.Effect))
                {
                    State.Assets.Effect = "SkyDome.fx";
                }

                if (Effect == null)
                    Effect = ResourceCache.GlobalInstance.CreateEffectFromFile(device, State.Assets.Effect);

                World = xna.Matrix.Identity;


                xnagrfx.Texture visualTexture = ResourceCache.GlobalInstance.CreateTextureFromFile(device, VisualTexture);
                if (!(visualTexture is xnagrfx.Texture2D))
                {
                    throw new InvalidOperationException("SkyDomeEntity's visual texture must be 2D texture");
                }

                Meshes.Add(ResourceCache.GlobalInstance.CreateMeshFromFile(device, "SkyDome.bos"));
                Meshes[0].Textures[0] = visualTexture;

                LightingCubeTexture = ResourceCache.GlobalInstance.CreateCubeTextureFromFile(device, LightingTexture);

                CompactVertexBuffer(device);

                HasBeenInitialized = true;
            }
            catch (Exception ex)
            {
                HasBeenInitialized = false;
                InitError = ex.ToString();
            }
        }
        #endregion
    }

    /// <summary>
    /// Models a single geometric shape with physical properties.
    /// </summary>
    /// <remarks>Use this to quickly add an arbitrary graphics mesh in the simulation, using an simple physical geometry to approximate collisions for physics</remarks>
    [CLSCompliant(true)]
    [DataContract]
    [EditorAttribute(typeof(GenericObjectEditor), typeof(System.Drawing.Design.UITypeEditor))]
    public class SingleShapeEntity : VisualEntity
    {
        // we can only have one shape but to serialize and deserialize properly
        // we use strongly types shape fields. One of them, and only one will be set,
        // depending on the shape type that was used when the entity was created.
        Shape _shape;

        /// <summary>
        /// Box shape. Valid if no other shape instance is set
        /// </summary>
        [DataMember]
        [Description("Box shape. Valid if no other shape instance is set.")]
        [Category("Shape")]
        public BoxShape BoxShape
        {
            get { return _shape as BoxShape; }
            set { if (value != null) _shape = value; }
        }

        /// <summary>
        /// Sphere shape. Valid if no other shape instance is set
        /// </summary>
        [DataMember]
        [Description("Sphere shape. Valid if no other shape instance is set.")]
        [Category("Shape")]
        public SphereShape SphereShape
        {
            get { return _shape as SphereShape; }
            set { if (value != null) _shape = value; }
        }
        /// <summary>
        /// Capsule shape. Valid if no other shape instance is set
        /// </summary>
        [DataMember]
        [Description("Capsule shape. Valid if no other shape instance is set.")]
        [Category("Shape")]
        public CapsuleShape CapsuleShape
        {
            get { return _shape as CapsuleShape; }
            set { if (value != null) _shape = value; }
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public SingleShapeEntity() { }

        /// <summary>
        /// Initialization constructor
        /// </summary>
        /// <param name="shape"></param>
        /// <param name="initialPos"></param>
        public SingleShapeEntity(Shape shape, Vector3 initialPos)
        {
            base.State.Pose.Position = initialPos;
            _shape = shape;
        }

        /// <summary>
        /// Initialization
        /// </summary>
        /// <param name="device"></param>
        /// <param name="physicsEngine"></param>
        public override void Initialize(xnagrfx.GraphicsDevice device, PhysicsEngine physicsEngine)
        {
            try
            {
                InitError = string.Empty;
                if (_shape == null)
                {
                    _shape = new SphereShape(new SphereShapeProperties(State.Name + "/Shape", 1.0f,
                        new Pose(new Vector3(0, 0, 0), new Quaternion(0, 0, 0, 1)), 1.0f));
                }
                base.State.PhysicsPrimitives.Add(_shape);

                if (base.State.PhysicsPrimitives[0].State.Name == null)
                    base.State.PhysicsPrimitives[0].State.Name = State.Name;

                // To be visible to the physics simulation we have to explicitly insert ourselves
                CreateAndInsertPhysicsEntity(physicsEngine);

                // call base initialize so it can load effect, cache handles, compute
                // bounding geometry
                base.Initialize(device, physicsEngine);
            }
            catch (Exception ex)
            {
                HasBeenInitialized = false;
                InitError = ex.ToString();
            }
        }
    }

    /// <summary>
    /// Models an entity with multiple shapes, with fixed pose with respect to each other.
    /// </summary>
    /// <remarks>This entity should be used to model composite rigid objects</remarks>
    [CLSCompliant(true)]
    [DataContract]
    [EditorAttribute(typeof(GenericObjectEditor), typeof(System.Drawing.Design.UITypeEditor))]
    public class MultiShapeEntity : VisualEntity
    {

        List<BoxShape> _boxShapes = new List<BoxShape>();

        /// <summary>
        /// List of box shapes that make up the entity
        /// </summary>
        [DataMember]
        [Description("List of box shapes that make up the entity.")]
        [Category("Shapes")]
        public List<BoxShape> BoxShapes
        {
            get { return _boxShapes; }
            set { _boxShapes = value; }
        }

        List<SphereShape> _sphereShapes = new List<SphereShape>();

        /// <summary>
        /// List of sphere shapes that make up the entity
        /// </summary>
        [DataMember]
        [Description("List of sphere shapes that make up the entity.")]
        [Category("Shapes")]
        public List<SphereShape> SphereShapes
        {
            get { return _sphereShapes; }
            set { _sphereShapes = value; }
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public MultiShapeEntity() { }

        /// <summary>
        /// Constructor which allows shapes to be specified
        /// </summary>
        public MultiShapeEntity(BoxShape[] boxShapes, SphereShape[] sphereShapes)
        {
            if (boxShapes != null)
            {
                foreach (BoxShape box in boxShapes)
                    _boxShapes.Add(box);
            }

            if (sphereShapes != null)
            {
                foreach (SphereShape sphere in sphereShapes)
                    _sphereShapes.Add(sphere);
            }
        }

        /// <summary>
        /// Initialization
        /// </summary>
        /// <param name="device"></param>
        /// <param name="physicsEngine"></param>
        public override void Initialize(xnagrfx.GraphicsDevice device, PhysicsEngine physicsEngine)
        {
            try
            {
                InitError = string.Empty;
                int i = 0;
                if (_boxShapes != null)
                {

                    foreach (BoxShape b in _boxShapes)
                    {
                        base.State.PhysicsPrimitives.Add(b);
                        if (b.State.Name == null)
                            b.State.Name = State.Name + ":" + i.ToString();
                        i++;
                    }
                }

                if (_sphereShapes != null)
                {
                    foreach (SphereShape s in _sphereShapes)
                    {
                        base.State.PhysicsPrimitives.Add(s);
                        if (s.State.Name == null)
                            s.State.Name = State.Name + ":" + i.ToString();
                        i++;
                    }
                }

                // To be visible to the physics simulation we have to explicitly insert ourselves
                CreateAndInsertPhysicsEntity(physicsEngine);

                // call base initialize so it can load effect, cache handles, compute
                // bounding geometry
                base.Initialize(device, physicsEngine);

                // update pose per shape
                Flags |= VisualEntityProperties.DoCompletePhysicsShapeUpdate;
            }
            catch (Exception ex)
            {
                HasBeenInitialized = false;
                InitError = ex.ToString();
            }
        }
    }


    /// <summary>
    /// Types of mesh entities
    /// </summary>
    [DataContract]
    [CLSCompliant(true)]
    public enum MeshEntityType
    {
        /// <summary>
        /// The physics object is the bounding box of the mesh.
        /// </summary>
        Box,
        /// <summary>
        /// The physics object is the bounding sphere of the mesh.
        /// </summary>
        Sphere,
        /// <summary>
        /// The physics object is the convex mesh of the mesh.
        /// </summary>
        Convex,
        /// <summary>
        /// The physics object is the same as the input mesh.
        /// </summary>
        Triangle
    }

    /// <summary>
    /// An entity that has one or more physics shapes based on a mesh.
    /// </summary>
    [CLSCompliant(true)]
    [DataContract]
    [EditorAttribute(typeof(GenericObjectEditor), typeof(System.Drawing.Design.UITypeEditor))]
    public class MeshEntity : VisualEntity
    {

        MeshEntityType _type;

        /// <summary>
        /// The type of the mesh entity
        /// </summary>
        [DataMember]
        [Description("Mesh entity type")]
        public MeshEntityType MeshEntityType
        {
            get { return _type; }
            set { _type = value; }
        }

        MaterialProperties _material = null;

        /// <summary>
        /// The physics material for the shape to use.  Must be specified
        /// before initialization.
        /// </summary>
        [BrowsableAttribute(false)]
        public MaterialProperties Material
        {
            get { return _material; }
            set { _material = value; }
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public MeshEntity() { }

        /// <summary>
        /// Initialization constructor
        /// </summary>
        /// <param name="type"></param>
        /// <param name="initialPos"></param>
        /// <param name="meshResource"></param>
        /// <param name="mass"></param>
        public MeshEntity(MeshEntityType type, Vector3 initialPos, string meshResource, float mass)
        {
            _type = type;
            base.State.Pose.Position = initialPos;
            base.State.Assets.Mesh = meshResource;
            base.State.MassDensity.Mass = mass;
        }

        /// <summary>
        /// Initialization
        /// </summary>
        /// <param name="device"></param>
        /// <param name="physicsEngine"></param>
        public override void Initialize(xnagrfx.GraphicsDevice device, PhysicsEngine physicsEngine)
        {
            try
            {
                InitError = string.Empty;
                // call base initialize so it can load effect, cache handles, compute
                // bounding geometry
                base.Initialize(device, physicsEngine);

                    switch (_type)
                    {
                        case MeshEntityType.Convex:
                            {
                                ConvexMeshShape convex = VisualEntityMesh.CreateConvexMeshShape(Meshes[0], State.Assets.Mesh, State.MassDensity.Mass);
                                base.State.PhysicsPrimitives.Add(convex);
                                convex.State.MassDensity.Mass = State.MassDensity.Mass;
                                if (_material != null)
                                    convex.State.Material = _material;
                                break;
                            }
                        case MeshEntityType.Triangle:
                            {
                                TriangleMeshShape triangle = VisualEntityMesh.CreateTriangleMeshShape(Meshes[0], State.Assets.Mesh, State.MassDensity.Mass);
                                base.State.PhysicsPrimitives.Add(triangle);
                                triangle.State.MassDensity.Mass = State.MassDensity.Mass;
                                if (_material != null)
                                    triangle.State.Material = _material;
                                break;
                            }
                        case MeshEntityType.Box:
                            {
                                Meshes[0].ComputeBoundingVolume();
                                xna.BoundingBox bbox = Meshes[0].BoundingBox;
                                Vector3 dimensions = new Vector3(
                                    bbox.Max.X - bbox.Min.X,
                                    bbox.Max.Y - bbox.Min.Y,
                                    bbox.Max.Z - bbox.Min.Z);


                                BoxShape box = new BoxShape(
                                    new BoxShapeProperties(
                                        this.State.MassDensity.Mass,
                                        new Pose(),
                                        dimensions));

                                base.State.PhysicsPrimitives.Add(box);
                                if (_material != null)
                                    box.State.Material = _material;
                                break;
                            }
                        case MeshEntityType.Sphere:
                            {
                                xna.BoundingSphere bsphere = Meshes[0].ComputeBoundingVolume();

                                SphereShape sphere = new SphereShape(
                                    new SphereShapeProperties(
                                        this.State.MassDensity.Mass,
                                        new Pose(),
                                        bsphere.Radius));
                                base.State.PhysicsPrimitives.Add(sphere);
                                sphere.State.MassDensity.Mass = State.MassDensity.Mass;
                                if (_material != null)
                                    sphere.State.Material = _material;
                                break;
                            }
                    }

                if(base.State.PhysicsPrimitives.Count != 0)
                    CreateAndInsertPhysicsEntity(PhysicsEngine);
            }
            catch (Exception ex)
            {
                HasBeenInitialized = false;
                InitError = ex.ToString();
            }
        }
    }



    /// <summary>
    /// Generates a simplified convex hull for physics collisitions, using convex mesh data
    /// </summary>
    [CLSCompliant(true)]
    [DataContract]
    [EditorAttribute(typeof(GenericObjectEditor), typeof(System.Drawing.Design.UITypeEditor))]
    public class SimplifiedConvexMeshEnvironmentEntity : MeshEntity
    {
        /// <summary>
        /// Generated convex mesh shape
        /// </summary>
        [DataMember]
        [Description("Generated convex mesh shape.")]
        public ConvexMeshShape ConvexMeshShape
        {
            get { return (State.PhysicsPrimitives.Count > 0) ? (ConvexMeshShape)State.PhysicsPrimitives[0] : null; }
            set 
            { 
                if(State.PhysicsPrimitives.Count > 0)
                    State.PhysicsPrimitives[0] = value;
                else
                    State.PhysicsPrimitives.Add(value);
            }
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public SimplifiedConvexMeshEnvironmentEntity()
            : base() { }

        /// <summary>
        /// Initialization constructor
        /// </summary>
        /// <param name="initialPos"></param>
        /// <param name="meshResource"></param>
        /// <param name="physicsShape"></param>
        public SimplifiedConvexMeshEnvironmentEntity(Vector3 initialPos, string meshResource, Shape physicsShape)
            : base(MeshEntityType.Convex, initialPos, meshResource, 0)
        {
        }
    }

    /// <summary>
    /// The types of items supported by the floorplan entity
    /// </summary>
    [DataContract]
    [CLSCompliant(true)]
    public enum FloorplanItemType
    {
        /// <summary>
        /// A simple wall item
        /// </summary>
        Wall,
        /// <summary>
        /// A 2.13 meter high door
        /// </summary>
        Door,
        /// <summary>
        /// A 0.61 meter tall window 1.524 meters above the ground
        /// </summary>
        Window
    }

    /// <summary>
    /// Generates a physics collision mesh using the same exact geometry as a triangle based graphics mesh
    /// </summary>
    [CLSCompliant(true)]
    [DataContract]
    [EditorAttribute(typeof(GenericObjectEditor), typeof(System.Drawing.Design.UITypeEditor))]
    public class TriangleMeshEnvironmentEntity : MeshEntity
    {
        /// <summary>
        /// Triangle Mesh shape
        /// </summary>
        [DataMember]
        [Description("Triangle Mesh shape.")]
        public TriangleMeshShape TriangleMeshShape
        {
            get { return (State.PhysicsPrimitives.Count > 0) ? (TriangleMeshShape)State.PhysicsPrimitives[0] : null; }
            set 
            { 
                if(State.PhysicsPrimitives.Count > 0)
                    State.PhysicsPrimitives[0] = value;
                else
                    State.PhysicsPrimitives.Add(value);
            }
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public TriangleMeshEnvironmentEntity() { }

        /// <summary>
        /// Initialization constructor
        /// </summary>
        /// <param name="initialPos"></param>
        /// <param name="meshResource"></param>
        /// <param name="physicsShape"></param>
        public TriangleMeshEnvironmentEntity(Vector3 initialPos, string meshResource, Shape physicsShape)
            : base(MeshEntityType.Triangle, initialPos, meshResource, 0)
        {
        }
    }

    /// <summary>
    /// A user-instance of a floorplan item
    /// </summary>
    [DataContract]
    [CLSCompliant(true)]
    public class FloorplanItemInstance
    {
        FloorplanItemType _type;

        /// <summary>
        /// The type of this instance
        /// </summary>
        [DataMember]
        public FloorplanItemType Type
        {
            get { return _type; }
            set { _type = value; }
        }

        Vector3 _pt1;

        /// <summary>
        /// The first point that defines the position of the item
        /// </summary>
        [DataMember]
        public Vector3 Pt1
        {
            get { return _pt1; }
            set { _pt1 = value; }
        }

        Vector3 _pt2;

        /// <summary>
        /// The second point that defines the position of the item
        /// </summary>
        [DataMember]
        public Vector3 Pt2
        {
            get { return _pt2; }
            set { _pt2 = value; }
        }

        /// <summary>
        /// Attributes of the item
        /// </summary>
        [DataMember]
        public Dictionary<string, string> Attributes;

        /// <summary>
        /// A class that holds the parameter types
        /// </summary>
        public object AttributeClass;

        FloorplanEntity _owner = null;

        /// <summary>
        /// The floorplan entity that owns this instance
        /// </summary>
        public FloorplanEntity Owner
        {
            get { return _owner; }
            set { _owner = value; }
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public FloorplanItemInstance()
        {
        }

        /// <summary>
        /// Data Constructor
        /// </summary>
        /// <param name="pt1X"></param>
        /// <param name="pt1Z"></param>
        /// <param name="pt2X"></param>
        /// <param name="pt2Z"></param>
        /// <param name="type"></param>
        /// <param name="owner"></param>
        public FloorplanItemInstance(float pt1X, float pt1Z, float pt2X, float pt2Z, FloorplanItemType type, FloorplanEntity owner)
        {
            _pt1 = new Vector3(pt1X, 0, pt1Z);
            _pt2 = new Vector3(pt2X, 0, pt2Z);
            _type = type;
            _owner = owner;
        }

        /// <summary>
        /// This method updates reads the attributes from the attributes dictionary and updates
        /// the attributes class.
        /// </summary>
        public void BeginEdit()
        {
            if (Attributes == null)
                Attributes = new Dictionary<string, string>();

            switch (Type)
            {
                case FloorplanItemType.Door:
                    {
                        DoorAttributes dp = new DoorAttributes();
                        dp.GetValues(Attributes, _owner);
                        AttributeClass = dp;
                        break;
                    }
                case FloorplanItemType.Wall:
                    {
                        WallAttributes wp = new WallAttributes();
                        wp.GetValues(Attributes, _owner);
                        AttributeClass = wp;
                        break;
                    }
                case FloorplanItemType.Window:
                    {
                        WindowAttributes wp = new WindowAttributes();
                        wp.GetValues(Attributes, _owner);
                        AttributeClass = wp;
                        break;
                    }
            }
        }

        /// <summary>
        /// This method updates the attributes dictionary with the new attributes so that
        /// the item can be properly serialized.
        /// </summary>
        public void EndEdit()
        {
            Attributes.Clear();
            switch (Type)
            {
                case FloorplanItemType.Door:
                    ((DoorAttributes)AttributeClass).SetValues(Attributes, _owner);
                    break;
                case FloorplanItemType.Wall:
                    ((WallAttributes)AttributeClass).SetValues(Attributes, _owner);
                    break;
                case FloorplanItemType.Window:
                    ((WindowAttributes)AttributeClass).SetValues(Attributes, _owner);
                    break;
            }
        }
    }

    /// <summary>
    /// Wall Attributes
    /// </summary>
    public class WallAttributes
    {
        float _thickness;
        float _height;

        /// <summary>
        /// Wall thickness
        /// </summary>
        public float Thickness
        {
            get { return _thickness; }
            set { _thickness = value; }
        }
        /// <summary>
        /// Wall height
        /// </summary>
        public float Height
        {
            get { return _height; }
            set { _height = value; }
        }
        /// <summary>
        /// Default constructor
        /// </summary>
        public WallAttributes()
        {
            Thickness = Height = 0;
        }
        /// <summary>
        /// Get the values from the attributes dictionary
        /// </summary>
        /// <param name="attributes"></param>
        /// <param name="defaults"></param>
        public virtual void GetValues(Dictionary<string, string> attributes, FloorplanEntity defaults)
        {
            string value = string.Empty;
            float floatValue = 0;

            Thickness = defaults.DefaultWallThickness;
            Height = defaults.DefaultWallHeight;
            if (attributes.TryGetValue("Thickness", out value))
                if (Single.TryParse(value, out floatValue))
                    Thickness = floatValue;
            if (attributes.TryGetValue("Height", out value))
                if (Single.TryParse(value, out floatValue))
                    Height = floatValue;
            
            Validate();
        }

        /// <summary>
        /// Check that the attributes fall within valid ranges.
        /// </summary>
        public virtual void Validate()
        {
            if (Thickness < 0)
                Thickness = 0;

            if (Height < 0)
                Height = 0;
            if (Height > 1000)
                Height = 1000;
        }

        /// <summary>
        /// Set the values in the attributes dictionary
        /// </summary>
        /// <param name="attributes"></param>
        /// <param name="defaults"></param>
        public virtual void SetValues(Dictionary<string, string> attributes, FloorplanEntity defaults)
        {
            Validate();
            if(Thickness != defaults.DefaultWallThickness)
                attributes.Add("Thickness", Thickness.ToString());

            if(Height != defaults.DefaultWallHeight)
                attributes.Add("Height", Height.ToString());
        }
    }

    /// <summary>
    /// Attributes of the door item
    /// </summary>
    public class DoorAttributes : WallAttributes
    {
        float _topOfOpening;

        /// <summary>
        /// The height of the top of the opening in meters
        /// </summary>
        public float TopOfOpening
        {
            get { return _topOfOpening; }
            set { _topOfOpening = value; }
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public DoorAttributes() :
            base()
        {
            TopOfOpening = 0;
        }

        /// <summary>
        /// Get the values from the attributes dictionary
        /// </summary>
        /// <param name="attributes"></param>
        /// <param name="defaults"></param>
        public override void GetValues(Dictionary<string, string> attributes, FloorplanEntity defaults)
        {
            string value = string.Empty;
            float floatValue = 0;

            TopOfOpening = defaults.DefaultTopOfOpening;

            if (attributes.TryGetValue("TopOfOpening", out value))
                if (Single.TryParse(value, out floatValue))
                    TopOfOpening = floatValue;
            base.GetValues(attributes, defaults);
            
            Validate();
        }

        /// <summary>
        /// Check that the attributes fall within valid ranges.
        /// </summary>
        public override void Validate()
        {
            base.Validate();
            if (TopOfOpening > Height)
                TopOfOpening = Height;
            if (TopOfOpening < 0)
                TopOfOpening = 0;
        }

        /// <summary>
        /// Set the values in the attributes dictionary
        /// </summary>
        /// <param name="attributes"></param>
        /// <param name="defaults"></param>
        public override void SetValues(Dictionary<string, string> attributes, FloorplanEntity defaults)
        {
            Validate();

            base.SetValues(attributes, defaults);

            if (TopOfOpening != defaults.DefaultTopOfOpening)
                attributes.Add("TopOfOpening", TopOfOpening.ToString());
        }
    }

    /// <summary>
    /// Attributes of the window item
    /// </summary>
    public class WindowAttributes : DoorAttributes
    {
        float _bottomOfOpening;

        /// <summary>
        /// The height of the bottom of the opening in meters
        /// </summary>
        public float BottomOfOpening
        {
            get { return _bottomOfOpening; }
            set { _bottomOfOpening = value; }
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public WindowAttributes() :
            base()
        {
            BottomOfOpening = 0;
        }

        /// <summary>
        /// Get the values from the attributes dictionary
        /// </summary>
        /// <param name="attributes"></param>
        /// <param name="defaults"></param>
        public override void GetValues(Dictionary<string, string> attributes, FloorplanEntity defaults)
        {
            string value = string.Empty;
            float floatValue = 0;

            BottomOfOpening = defaults.DefaultBottomOfOpening;

            if (attributes.TryGetValue("BottomOfOpening", out value))
                if (Single.TryParse(value, out floatValue))
                    BottomOfOpening = floatValue;
            base.GetValues(attributes, defaults);

            Validate();
        }

        /// <summary>
        /// Check that the attributes fall within valid ranges.
        /// </summary>
        public override void Validate()
        {
            base.Validate();
            if (BottomOfOpening > TopOfOpening)
                BottomOfOpening = TopOfOpening;
            if (BottomOfOpening < 0)
                BottomOfOpening = 0;
        }

        /// <summary>
        /// Set the values in the attributes dictionary
        /// </summary>
        /// <param name="attributes"></param>
        /// <param name="defaults"></param>
        public override void SetValues(Dictionary<string, string> attributes, FloorplanEntity defaults)
        {
            Validate();
            
            base.SetValues(attributes, defaults);

            if (BottomOfOpening != defaults.DefaultBottomOfOpening)
                attributes.Add("BottomOfOpening", BottomOfOpening.ToString());
        }
    }

    /// <summary>
    /// Base class for items that make up a floor plan
    /// </summary>
    [DataContract]
    [CLSCompliant(true)]
    public class FloorplanItem
    {
        /// <summary>
        /// The name of the item
        /// </summary>
        [DataMember]
        public string Name;

        /// <summary>
        /// The type of the item
        /// </summary>
        [DataMember]
        public FloorplanItemType Type;

        /// <summary>
        /// Default Constructor
        /// </summary>
        public FloorplanItem()
        {
        }

        /// <summary>
        /// Data constructor
        /// </summary>
        /// <param name="name"></param>
        /// <param name="type"></param>
        public FloorplanItem(string name, FloorplanItemType type)
        {
            Name = name;
            Type = type;
        }
    }

    /// <summary>
    /// Models a building with walls, doors, and windows as well as other structures such as 
    /// mazes and even lines painted on the floor.
    /// </summary>
    [CLSCompliant(true)]
    [DataContract]
    [EditorAttribute(typeof(GenericObjectEditor), typeof(System.Drawing.Design.UITypeEditor))]
    public class FloorplanEntity : VisualEntity
    {
        private float _defaultWallThickness = 0.15f;
        /// <summary>
        /// The thickness of each wall.
        /// </summary>
        [DataMember]
        [Category("Wall Dimensions")]
        public float DefaultWallThickness
        {
            get { return _defaultWallThickness; }
            set { _defaultWallThickness = Math.Max(value, 0.0f); }
        }

        private float _defaultWallHeight = 3.0f;
        /// <summary>
        /// The height of each wall.
        /// </summary>
        [DataMember]
        [Category("Wall Dimensions")]
        public float DefaultWallHeight
        {
            get { return _defaultWallHeight; }
            set { _defaultWallHeight = Math.Max(value, 0.0f); }
        }

        private float _defaultBottomOfOpening = 1.524f;
        /// <summary>
        /// The default height of the bottom of a window.
        /// </summary>
        [DataMember]
        [Category("Wall Dimensions")]
        public float DefaultBottomOfOpening
        {
            get { return _defaultBottomOfOpening; }
            set { _defaultBottomOfOpening = value; }
        }

        private float _defaultTopOfOpening = 2.13f;
        /// <summary>
        /// The default top height of a window or door
        /// </summary>
        [DataMember]
        [Category("Wall Dimensions")]
        public float DefaultTopOfOpening
        {
            get { return _defaultTopOfOpening; }
            set { _defaultTopOfOpening = value; }
        }

        private string _underlayImageFilename;

        /// <summary>
        /// Specifies the filename of the underlay image
        /// </summary>
        [DataMember]
        [Category("Underlay Image")]
        public string UnderlayImage
        {
            get { return _underlayImageFilename; }
            set { _underlayImageFilename = value; }
        }

        private Vector2 _underlayOffset;
        /// <summary>
        /// Specifies the X,Y position of the underlay center
        /// </summary>
        [DataMember]
        [Category("Underlay Image")]
        public Vector2 UnderlayOffset
        {
            get { return _underlayOffset; }
            set { _underlayOffset = value; }
        }

        private float _underlayScale;
        /// <summary>
        /// The distance the width of the underlay covers in meters
        /// </summary>
        [DataMember]
        [Category("Underlay Image")]
        public float UnderlayScale
        {
            get { return _underlayScale; }
            set { _underlayScale = value; }
        }

        private float _gridSpacing = 0.0375f;

        /// <summary>
        /// The spacing of the snap-to grid.  0 means no snap.
        /// </summary>
        [DataMember]
        public float SnapToGridSpacing
        {
            get { return _gridSpacing; }
            set { _gridSpacing = value; }
        }

        /// <summary>
        /// All of the items contained by the floorplan entity
        /// </summary>
        [DataMember]
        public Dictionary<FloorplanItemType, FloorplanItem> Items;

        /// <summary>
        /// The instances of floorplan items
        /// </summary>
        [DataMember]
        public List<FloorplanItemInstance> Instances;

        private SolidBrush _selectedBrush;
        private SolidBrush _greenHandleBrush;
        private SolidBrush _redHandleBrush;

        /// <summary>
        /// Default constructor
        /// </summary>
        public FloorplanEntity() { }

        /// <summary>
        /// Constructor
        /// </summary>
        public FloorplanEntity(Vector3 position, List<FloorplanItemInstance> instances)
        {
            State.Pose.Position = position;

            Items = new Dictionary<FloorplanItemType, FloorplanItem>();
            Items.Add(FloorplanItemType.Wall, new FloorplanItem("Wall", FloorplanItemType.Wall));
            Items.Add(FloorplanItemType.Door, new FloorplanItem("Door", FloorplanItemType.Door));
            Items.Add(FloorplanItemType.Window, new FloorplanItem("Window", FloorplanItemType.Window));

            Instances = new List<FloorplanItemInstance>();
            if (instances != null)
                foreach (FloorplanItemInstance instance in instances)
                    Instances.Add(instance);
        }

        /// <summary>
        /// Initialization
        /// </summary>
        /// <param name="device"></param>
        /// <param name="physicsEngine"></param>
        public override void Initialize(xnagrfx.GraphicsDevice device, PhysicsEngine physicsEngine)
        {
            try
            {
                InitError = string.Empty;

                _selectedBrush = new SolidBrush(Color.FromArgb(150, 255, 255, 255));
                _redHandleBrush = new SolidBrush(Color.FromArgb(253, 0, 0));
                _greenHandleBrush = new SolidBrush(Color.FromArgb(0, 255, 0));

                // set this object up as the owner of its item instances and build shapes
                foreach (FloorplanItemInstance instance in Instances)
                {
                    instance.Owner = this;
                    if (instance.Attributes == null)
                        instance.Attributes = new Dictionary<string, string>();

                    AddPhysicsShapesAndMeshes(_oldInfo, instance);
                }

                // initialize meshes and other assets
                base.Initialize(device, physicsEngine);

                // Add ourself to the physics engine
                if (State.PhysicsPrimitives.Count != 0)
                    CreateAndInsertPhysicsEntity(physicsEngine);

            }
            catch (Exception ex)
            {
                HasBeenInitialized = false;
                InitError = ex.ToString();
            }
        }

        FloorplanRenderInfo _oldInfo = null;

        /// <summary>
        /// Create and add the physics shapes for an item instance
        /// </summary>
        /// <param name="info"></param>
        /// <param name="instance"></param>
        public void AddPhysicsShapesAndMeshes(FloorplanRenderInfo info, FloorplanItemInstance instance)
        {
            try
            {
                FloorplanItem item = Items[instance.Type];

                // build the top-level matrix
                xna.Matrix World =
                    xna.Matrix.Multiply(xna.Matrix.CreateFromQuaternion(TypeConversion.ToXNA(State.Pose.Orientation)),
                        xna.Matrix.CreateTranslation(TypeConversion.ToXNA(State.Pose.Position)));

                xna.Vector3 wPt1 = xna.Vector3.Transform(TypeConversion.ToXNA(instance.Pt1), World);
                xna.Vector3 wPt2 = xna.Vector3.Transform(TypeConversion.ToXNA(instance.Pt2), World);

                switch (item.Type)
                {
                    case FloorplanItemType.Wall:
                    case FloorplanItemType.Door:
                    case FloorplanItemType.Window:
                        {
                            // build a box that matches the wall
                            // first find the angle of rotation
                            xna.Vector3 wallVector = (wPt2 - wPt1);
                            float length = wallVector.Length();
                            double angle = Math.Atan2(wallVector.Z, wallVector.X)  + 2 * Math.PI;
                            xna.Vector3 center = xna.Vector3.Multiply((wPt2 + wPt1), 0.5f);
                            Vector3 position;

                            if (item.Type == FloorplanItemType.Wall)
                            {
                                WallAttributes wa = new WallAttributes();
                                wa.GetValues(instance.Attributes, instance.Owner);
                                position = new Vector3(
                                        center.X - State.Pose.Position.X,
                                        center.Y - State.Pose.Position.Y + wa.Height / 2,
                                        center.Z - State.Pose.Position.Z);
                                instance.AttributeClass = wa;
                                BoxShape wallBox = new BoxShape(new BoxShapeProperties(
                                    0,
                                    new Pose(
                                        position,
                                        Quaternion.FromAxisAngle(0, -1, 0, (float)angle)),
                                    new Vector3(length, wa.Height, wa.Thickness)));
                                State.PhysicsPrimitives.Add(wallBox);
                            }
                            else if (item.Type == FloorplanItemType.Door)
                            {
                                DoorAttributes da = new DoorAttributes();
                                da.GetValues(instance.Attributes, instance.Owner);
                                position = new Vector3(
                                        center.X - State.Pose.Position.X,
                                        center.Y - State.Pose.Position.Y + da.Height / 2,
                                        center.Z - State.Pose.Position.Z);
                                instance.AttributeClass = da;
                                float AboveDoor = da.Height - da.TopOfOpening;
                                if (AboveDoor > 0)
                                {
                                    position.Y = da.Height - AboveDoor / 2;
                                    BoxShape aboveDoorBox = new BoxShape(new BoxShapeProperties(
                                        0,
                                        new Pose(
                                            position,
                                            Quaternion.FromAxisAngle(0, -1, 0, (float)angle)),
                                        new Vector3(length, AboveDoor, da.Thickness)));
                                    State.PhysicsPrimitives.Add(aboveDoorBox);
                                }
                            }
                            else if (item.Type == FloorplanItemType.Window)
                            {
                                WindowAttributes wa = new WindowAttributes();
                                wa.GetValues(instance.Attributes, instance.Owner);
                                instance.AttributeClass = wa;
                                position = new Vector3(
                                        center.X - State.Pose.Position.X,
                                        center.Y - State.Pose.Position.Y + wa.Height / 2,
                                        center.Z - State.Pose.Position.Z);
                                float AboveWindow = wa.Height - wa.TopOfOpening;
                                if (AboveWindow > 0)
                                {
                                    position.Y = wa.TopOfOpening + AboveWindow / 2;
                                    BoxShape aboveWindowBox = new BoxShape(new BoxShapeProperties(
                                        0,
                                        new Pose(
                                            position,
                                            Quaternion.FromAxisAngle(0, -1, 0, (float)angle)),
                                        new Vector3(length, AboveWindow, wa.Thickness)));
                                    State.PhysicsPrimitives.Add(aboveWindowBox);
                                }
                                position.Y = wa.BottomOfOpening / 2;
                                BoxShape belowWindowBox = new BoxShape(new BoxShapeProperties(
                                    0,
                                    new Pose(
                                        position,
                                        Quaternion.FromAxisAngle(0, -1, 0, (float)angle)),
                                    new Vector3(length, wa.BottomOfOpening, wa.Thickness)));
                                State.PhysicsPrimitives.Add(belowWindowBox);
                            }

                            break;
                        }
                }
            }
            catch
            {
            }
        }

        /// <summary>
        /// Returns true if the specified screen point is contained by the item
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="info"></param>
        /// <param name="instance"></param>
        /// <returns></returns>
        public bool IsContained(int x, int y, FloorplanRenderInfo info, FloorplanItemInstance instance)
        {
            try
            {
                FloorplanItem item = Items[instance.Type];

                // build the top-level matrix
                xna.Matrix World =
                    xna.Matrix.Multiply(xna.Matrix.CreateFromQuaternion(TypeConversion.ToXNA(State.Pose.Orientation)),
                        xna.Matrix.CreateTranslation(TypeConversion.ToXNA(State.Pose.Position)));

                xna.Vector3 wPt1 = xna.Vector3.Transform(TypeConversion.ToXNA(instance.Pt1), World);
                xna.Vector3 wPt2 = xna.Vector3.Transform(TypeConversion.ToXNA(instance.Pt2), World);

                switch (item.Type)
                {
                    case FloorplanItemType.Wall:
                    case FloorplanItemType.Door:
                    case FloorplanItemType.Window:
                        {
                            // calculate perpendicular vector
                            xna.Vector3 vert = new xna.Vector3(0, 1, 0);
                            xna.Vector3 perp = xna.Vector3.Cross(vert, wPt1 - wPt2);
                            perp.Normalize();
                            perp = xna.Vector3.Multiply(perp, _defaultWallThickness / 2);

                            // render
                            PointF sPt1 = PtToScreen(wPt2 + perp, info);
                            PointF sPt2 = PtToScreen(wPt2 - perp, info);
                            PointF sPt3 = PtToScreen(wPt1 - perp, info);
                            PointF sPt4 = PtToScreen(wPt1 + perp, info);

                            // make sure the point is contained by all four sides
                            if (((sPt2.X - sPt1.X) * (x - sPt1.X) + (sPt2.Y - sPt1.Y) * (y - sPt1.Y)) < 0)
                                return false;
                            if (((sPt3.X - sPt2.X) * (x - sPt2.X) + (sPt3.Y - sPt2.Y) * (y - sPt2.Y)) < 0)
                                return false;
                            if (((sPt4.X - sPt3.X) * (x - sPt3.X) + (sPt4.Y - sPt3.Y) * (y - sPt3.Y)) < 0)
                                return false;
                            if (((sPt1.X - sPt4.X) * (x - sPt4.X) + (sPt1.Y - sPt4.Y) * (y - sPt4.Y)) < 0)
                                return false;

                            return true;
                        }
                }
            }
            catch
            {
            }
            return false;
        }

        private int ScreenPositionX(float src, FloorplanRenderInfo info)
        {
            return ((int)((src + info.Offset.X) * info.Scale.X + info.Center.X + 0.5f));
        }
        private int ScreenPositionY(float src, FloorplanRenderInfo info)
        {
            return ((int)((src + info.Offset.Y) * info.Scale.Y + info.Center.Y + 0.5f));
        }
        private int ScreenDistanceX(float src, FloorplanRenderInfo info)
        {
            return ((int)(src * info.Scale.X + 0.5f));
        }
        private int ScreenDistanceY(float src, FloorplanRenderInfo info)
        {
            return ((int)(src * info.Scale.Y + 0.5f));
        }

        /// <summary>
        /// Draws the floorplan underlay image
        /// </summary>
        /// <param name="info"></param>
        public void DrawUnderlay(FloorplanRenderInfo info)
        {
            if (!string.IsNullOrEmpty(_underlayImageFilename))
            {
                Bitmap underlayImage = ResourceCache.GlobalInstance.CreateBitmapFromFile(_underlayImageFilename);
                if (underlayImage != null)
                {
                    float invAspect = underlayImage.Height / (float)underlayImage.Width;
                    Rectangle dstRect = new Rectangle(
                        ScreenPositionX(_underlayOffset.X - _underlayScale / 2, info),
                        ScreenPositionY(_underlayOffset.Y - invAspect * _underlayScale / 2, info),
                        ScreenDistanceX(_underlayScale, info),
                        ScreenDistanceY(_underlayScale * invAspect, info));

                    info.G.DrawImage(underlayImage, dstRect, 0, 0, underlayImage.Width, underlayImage.Height, GraphicsUnit.Pixel);
                }
            }
        }

        bool _controlDown;

        /// <summary>
        /// 2D floorplan mode rendering
        /// </summary>
        /// <param name="info"></param>
        public override void Render2D(FloorplanRenderInfo info)
        {
            _oldInfo = info;    // keep a copy

            if (info.EditTarget == this)
            {
                // render the underlay
                DrawUnderlay(info);
            }

            _controlDown = Utilities.GetKeyboardState().IsKeyDown(xna.Input.Keys.LeftControl);
            foreach (FloorplanItemInstance instance in Instances)
                RenderInstance2D(instance, info);

            // draw crosshairs at the center of the entity
            int centerX = ScreenPositionX(State.Pose.Position.X, info);
            int centerY = ScreenPositionY(State.Pose.Position.Z, info);
            const int CrosshairSize = 10;
            info.G.DrawLine(Pens.Red, centerX, centerY - CrosshairSize, centerX, centerY + CrosshairSize);
            info.G.DrawLine(Pens.Red, centerX - CrosshairSize, centerY, centerX + CrosshairSize, centerY);

            // draw the position and angle of the selected item
            if (_selectedInstance != null)
            {
                Vector3 dir = _selectedInstance.Pt2 - _selectedInstance.Pt1;
                double angle = Math.Atan2(dir.Z, dir.X) * 180.0 / Math.PI;

                info.G.FillRectangle(_selectedBrush, 0, 0, info.Target.Width, 20);
                info.G.DrawRectangle(Pens.Black, 0, 0, info.Target.Width, 20);
                info.G.DrawString(String.Format("Green: {0:F2}, {1:F2}, {2:F2}    Red: {3:F2}, {4:F2}, {5:F2}   Angle: {6:F2} deg",
                    _selectedInstance.Pt1.X,
                    _selectedInstance.Pt1.Y,
                    _selectedInstance.Pt1.Z,
                    _selectedInstance.Pt2.X,
                    _selectedInstance.Pt2.Y,
                    _selectedInstance.Pt2.Z,
                    angle), info.DefaultFont, Brushes.Black, 0, 0);
            }
        }

        /// <summary>
        /// Render a floorplan item to a 2D image
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="info"></param>
        public void RenderInstance2D(FloorplanItemInstance instance, FloorplanRenderInfo info)
        {
            try
            {
                FloorplanItem item = Items[instance.Type];

                // build the top-level matrix
                xna.Matrix World =
                    xna.Matrix.Multiply(xna.Matrix.CreateFromQuaternion(TypeConversion.ToXNA(State.Pose.Orientation)),
                        xna.Matrix.CreateTranslation(TypeConversion.ToXNA(State.Pose.Position)));

                xna.Vector3 wPt1 = xna.Vector3.Transform(TypeConversion.ToXNA(instance.Pt1), World);
                xna.Vector3 wPt2 = xna.Vector3.Transform(TypeConversion.ToXNA(instance.Pt2), World);

                switch (item.Type)
                {
                    case FloorplanItemType.Wall:
                    case FloorplanItemType.Door:
                    case FloorplanItemType.Window:
                        {
                            // calculate perpendicular vector
                            xna.Vector3 axis = wPt1 - wPt2;
                            xna.Vector3 vert = new xna.Vector3(0, 1, 0);
                            xna.Vector3 perp = xna.Vector3.Cross(vert, axis);
                            perp.Normalize();
                            perp = xna.Vector3.Multiply(perp, _defaultWallThickness / 2);

                            // render
                            PointF sPt1 = PtToScreen(wPt2 + perp, info);
                            PointF sPt2 = PtToScreen(wPt2 - perp, info);
                            PointF sPt3 = PtToScreen(wPt1 - perp, info);
                            PointF sPt4 = PtToScreen(wPt1 + perp, info);

                            if (instance == _selectedInstance)
                            {
                                info.G.FillPolygon(_selectedBrush, new PointF[] { sPt1, sPt2, sPt3, sPt4 });
                                info.G.DrawPolygon(Pens.DarkBlue, new PointF[] { sPt1, sPt2, sPt3, sPt4 });

                                if (item.Type != FloorplanItemType.Wall)
                                {
                                    info.G.DrawLine(Pens.DarkBlue, sPt1, sPt3);
                                    info.G.DrawLine(Pens.DarkBlue, sPt2, sPt4);
                                }

                                // draw the handles
                                perp.Normalize();
                                perp = xna.Vector3.Multiply(perp, _defaultWallThickness / 4);
                                axis.Normalize();

                                if (_controlDown)
                                {
                                    // draw circles
                                    xna.Vector3 centerAxis = xna.Vector3.Multiply(axis, 5 * _defaultWallThickness / 12);
                                    float radius = ScreenDistanceX(_defaultWallThickness / 4, info);

                                    sPt1 = PtToScreen(wPt2 + centerAxis, info);
                                    info.G.FillEllipse(_redHandleBrush, sPt1.X - radius, sPt1.Y - radius, radius * 2, radius * 2);

                                    sPt1 = PtToScreen(wPt1 - centerAxis, info);
                                    info.G.FillEllipse(_greenHandleBrush, sPt1.X - radius, sPt1.Y - radius, radius * 2, radius * 2);
                                }
                                else
                                {
                                    // draw squares
                                    xna.Vector3 shortAxis = xna.Vector3.Multiply(axis, _defaultWallThickness / 6);
                                    xna.Vector3 longAxis = xna.Vector3.Multiply(axis, 2 * _defaultWallThickness / 3);
                                    sPt1 = PtToScreen(wPt2 + shortAxis + perp, info);
                                    sPt2 = PtToScreen(wPt2 + shortAxis - perp, info);
                                    sPt3 = PtToScreen(wPt2 + longAxis - perp, info);
                                    sPt4 = PtToScreen(wPt2 + longAxis + perp, info);
                                    info.G.FillPolygon(_redHandleBrush, new PointF[] { sPt1, sPt2, sPt3, sPt4 });

                                    sPt1 = PtToScreen(wPt1 - shortAxis + perp, info);
                                    sPt2 = PtToScreen(wPt1 - shortAxis - perp, info);
                                    sPt3 = PtToScreen(wPt1 - longAxis - perp, info);
                                    sPt4 = PtToScreen(wPt1 - longAxis + perp, info);
                                    info.G.FillPolygon(_greenHandleBrush, new PointF[] { sPt1, sPt2, sPt3, sPt4 });
                                }
                            }
                            else
                            {
                                info.G.DrawPolygon(Pens.DarkBlue, new PointF[] { sPt1, sPt2, sPt3, sPt4 });
                                if (item.Type != FloorplanItemType.Wall)
                                {
                                    info.G.DrawLine(Pens.DarkBlue, sPt1, sPt3);
                                    info.G.DrawLine(Pens.DarkBlue, sPt2, sPt4);
                                }
                            }
                            break;
                        }
                }
            }
            catch
            {
            }
        }

        private PointF PtToScreen(xna.Vector3 pt, FloorplanRenderInfo info)
        {
            PointF ret = new Point();

            ret.X = (pt.X + info.Offset.X) * info.Scale.X + info.Center.X;
            ret.Y = (pt.Z + info.Offset.Y) * info.Scale.Y + info.Center.Y;
            return ret;
        }

        private xna.Vector3 ScreenToPt(PointF pt, FloorplanRenderInfo info)
        {
            xna.Vector3 ret = new xna.Vector3();

            ret.X = (pt.X - info.Center.X) / info.Scale.X - info.Offset.X;
            ret.Y = 0;
            ret.Z = (pt.Y - info.Center.Y) / info.Scale.Y - info.Offset.Y;

            return ret;
        }

        FloorplanItemInstance _selectedInstance = null;
        Vector3 _originalPt1;
        Vector3 _originalPt2;
        bool _constrainedMovement;
        bool _movePt1;
        bool _movePt2;
        xna.Input.MouseState _mouseDownState;
        bool _ctrlCDown = false;

        /// <summary>
        /// Handle keypresses
        /// </summary>
        /// <param name="kbState"></param>
        public void ProcessKeys(xna.Input.KeyboardState kbState)
        {
            if (kbState.IsKeyDown(xna.Input.Keys.Delete))
            {
                if (_selectedInstance != null)
                {
                    Instances.Remove(_selectedInstance);
                    _selectedInstance = null;
                    SetFloorplanItemAttributes(null);
                }
            }
            if (kbState.IsKeyDown(xna.Input.Keys.LeftControl) &&
                kbState.IsKeyDown(xna.Input.Keys.C))
            {
                if ((_selectedInstance != null) && !_ctrlCDown)
                {
                    float offset = _gridSpacing;
                    if (offset == 0)
                        offset = 0.1f;

                    FloorplanItemInstance newInstance = new FloorplanItemInstance(
                        _selectedInstance.Pt1.X + offset,
                        _selectedInstance.Pt1.Z + offset,
                        _selectedInstance.Pt2.X + offset,
                        _selectedInstance.Pt2.Z + offset,
                        _selectedInstance.Type,
                        this);
                    Instances.Add(newInstance);
                    _selectedInstance = newInstance;
                    SetFloorplanItemAttributes(_selectedInstance);
                }
                _ctrlCDown = true;
            }
            else
                _ctrlCDown = false;
        }

        /// <summary>
        /// MouseDown
        /// </summary>
        /// <param name="state"></param>
        /// <param name="keyboard"></param>
        public void MouseDown(xna.Input.MouseState state, xna.Input.KeyboardState keyboard)
        {
            _selectedInstance = null;
            SetFloorplanItemAttributes(null);
            // check to see if an item was clicked
            foreach (FloorplanItemInstance instance in Instances)
            {
                if (IsContained(state.X, state.Y, _oldInfo, instance))
                {
                    _mouseDownState = state;
                    _originalPt1 = instance.Pt1;
                    _originalPt2 = instance.Pt2;
                    _selectedInstance = instance;
                    SetFloorplanItemAttributes(_selectedInstance);
                    Color hitColor = _oldInfo.Target.GetPixel(state.X, state.Y);
                    if ((hitColor.R == 0) && (hitColor.G == 255))
                    {
                        _movePt1 = true;
                        _movePt2 = false;
                        _constrainedMovement = !keyboard.IsKeyDown(xna.Input.Keys.LeftControl);
                    }
                    else if ((hitColor.R == 253) && (hitColor.G == 0))
                    {
                        _movePt1 = false;
                        _movePt2 = true;
                        _constrainedMovement = !keyboard.IsKeyDown(xna.Input.Keys.LeftControl);
                    }
                    else
                    {
                        _movePt1 = true;
                        _movePt2 = true;
                        _constrainedMovement = false;
                    }
                }
            }
        }

        /// <summary>
        /// MouseDrag - returns true if the floorplan entity consumes the drag
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="scale"></param>
        /// <returns></returns>
        public bool MouseDrag(xna.Vector2 pos, Vector2 scale)
        {
            if (_selectedInstance != null)
            {
                Vector3 mouse = new Vector3(pos.X / scale.X, 0, pos.Y / scale.Y);
                if (_constrainedMovement)
                {
                    Vector3 axis = _originalPt2 - _originalPt1;
                    axis = Vector3.Normalize(axis);
                    float dot = (float)Vector3.Dot(axis, mouse);
                    mouse = Vector3.Scale(axis, dot);
                }
                if (_movePt1)
                    _selectedInstance.Pt1 = SnapToGrid(_originalPt1 + mouse);

                if (_movePt2)
                    _selectedInstance.Pt2 = SnapToGrid(_originalPt2 + mouse);

                return true;
            }
            return false;
        }

        private Vector3 SnapToGrid(Vector3 inVector)
        {
            if (_gridSpacing <= 0)
                return inVector;

            Vector3 ret = new Vector3();
            ret.X = (float)(Math.Round(inVector.X / _gridSpacing, MidpointRounding.AwayFromZero) * _gridSpacing);
            ret.Y = (float)(Math.Round(inVector.Y / _gridSpacing, MidpointRounding.AwayFromZero) * _gridSpacing);
            ret.Z = (float)(Math.Round(inVector.Z / _gridSpacing, MidpointRounding.AwayFromZero) * _gridSpacing);

            return ret;
        }



        /// <summary>
        /// MouseUp
        /// </summary>
        public void MouseUp()
        {
        }

        /// <summary>
        /// Edit Mode
        /// </summary>
        /// <returns></returns>
        public override bool Edit()
        {
            Panel parentPanel = SimulationEngine.GlobalInstance.SetFloorplanMode(true);
            SimulationEngine.GlobalInstance.SetPropertyEditEntity(this);

            HandleFloorplanEdit(parentPanel);

            return false;
        }

        bool HandleFloorplanEdit(Panel parent)
        {
            Button OKButton = new System.Windows.Forms.Button();
            FlowLayoutPanel flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            // 
            // OKButton
            // 
            OKButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            OKButton.Location = new System.Drawing.Point(8, 226);
            OKButton.Name = "OKButton";
            OKButton.Size = new System.Drawing.Size(78, 26);
            OKButton.TabIndex = 8;
            OKButton.Text = "OK";
            OKButton.UseVisualStyleBackColor = true;
            OKButton.Click += new EventHandler(OKButton_Click);
            OKButton.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Top;
            flowLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Size = new System.Drawing.Size(284, 220);
            flowLayoutPanel1.TabIndex = 10;
            flowLayoutPanel1.AutoScroll = true;
            //
            // Edit Panel
            //
            Panel editPanel = new Panel();
            editPanel.Name = "EditPanel";
            editPanel.AutoScroll = true;
            parent.Controls.Add(editPanel);
            editPanel.Dock = DockStyle.Fill;
            editPanel.Controls.Add(flowLayoutPanel1);

            // add buttons
            foreach (FloorplanItem item in Items.Values)
            {
                    Button newButton = new Button();
                    newButton.Width = 40;
                    newButton.Height = 20;
                    newButton.AutoSize = true;
                    newButton.Text = item.Name;

                    newButton.Tag = item.Type;
                    newButton.Click += new EventHandler(newButton_Click);
                    flowLayoutPanel1.Controls.Add(newButton);
            }

            _floorplanItemAttributeGrid = new PropertyGrid();
            _floorplanItemAttributeGrid.Name = "FloorplanItemProperties";
            _floorplanItemAttributeGrid.PropertySort = PropertySort.Alphabetical;
            _floorplanItemAttributeGrid.HelpVisible = false;
            _floorplanItemAttributeGrid.ToolbarVisible = false;
            _floorplanItemAttributeGrid.Visible = false;
            flowLayoutPanel1.Controls.Add(_floorplanItemAttributeGrid);
            flowLayoutPanel1.Controls.Add(OKButton);
            return false;
        }
        PropertyGrid _floorplanItemAttributeGrid = null;

        FloorplanItemInstance _previousItem = null;
        /// <summary>
        /// Display floorplanitem properties
        /// </summary>
        /// <param name="item"></param>
        public void SetFloorplanItemAttributes(FloorplanItemInstance item)
        {
            if (item == null)
            {
                _floorplanItemAttributeGrid.Visible = false;
                if(_previousItem != null)
                    _previousItem.EndEdit();
            }
            else
            {
                item.BeginEdit();
                _floorplanItemAttributeGrid.Visible = true;
                _floorplanItemAttributeGrid.Width = _floorplanItemAttributeGrid.Parent.Width - 8;
                _floorplanItemAttributeGrid.SelectedObject = item.AttributeClass;
            }
            _previousItem = item;
        }

        void newButton_Click(object sender, EventArgs e)
        {
            Button buttonSender = sender as Button;
            xna.Vector3 origin = ScreenToPt(new PointF(_oldInfo.Center.X, _oldInfo.Center.Y), _oldInfo);
            FloorplanItemInstance newInstance = new FloorplanItemInstance(
                origin.X - 0.5f, 
                origin.Z,
                origin.X + 0.5f, 
                origin.Z, 
                (FloorplanItemType)buttonSender.Tag,
                this);
            _selectedInstance = newInstance;
            SetFloorplanItemAttributes(_selectedInstance); 
            Instances.Add(newInstance);
        }

        void OKButton_Click(object sender, EventArgs e)
        {
            if(_previousItem != null)
                _previousItem.EndEdit();

            SimulationEngine.GlobalInstance.RefreshEntity(this);
            SimulationEngine.GlobalInstance.SetFloorplanMode(false);
        }
    }


    /// <summary>
    /// Models a terrain composed out of height field samples
    /// </summary>
    [CLSCompliant(true)]
    [DataContract]
    [EditorAttribute(typeof(GenericObjectEditor), typeof(System.Drawing.Design.UITypeEditor))]
    public class HeightFieldEntity : VisualEntity
    {
        HeightFieldShape _heightField;

        private const string _defaultHeightfieldTextureFilename = "03RamieSc.dds";
        private const float _defaultRestitutionConstant = 0.8f;
        private const float _defaultDynamicFrictionConstant = 0.5f;
        private const float _defaultStaticFrictionConstant = 0.8f;

        /// <summary>
        /// Height field shape
        /// </summary>
        [DataMember]
        [Description("Height field shape.")]
        public HeightFieldShape HeightFieldShape
        {
            get { return _heightField; }
            set { _heightField = value; }
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public HeightFieldEntity()
        {
        }

        /// <summary>
        /// Initialization constructor picked up by the simulation editor
        /// </summary>
        /// <param name="textureResource">Texture file to overlay over the height field</param>
        /// <param name="restitution">Restitution constant for ground</param>
        /// <param name="dynamicFriction">Dynamic friction constant for ground</param>
        /// <param name="staticFriction">Static friction constant for ground</param>
        public HeightFieldEntity([DefaultParameterValue(_defaultHeightfieldTextureFilename)] string textureResource, 
            [DefaultParameterValue(_defaultRestitutionConstant)] float restitution, 
            [DefaultParameterValue(_defaultDynamicFrictionConstant)] float dynamicFriction, 
            [DefaultParameterValue(_defaultStaticFrictionConstant)] float staticFriction)
            : this("ground", textureResource, new MaterialProperties("ground", restitution, dynamicFriction, staticFriction))
        {
        }

        /// <summary>
        /// Initialization constructor
        /// </summary>
        /// <param name="name">Entity name</param>
        /// <param name="textureResource">Texture file to overlay over the height field</param>
        /// <param name="groundMaterial">Material properties for ground</param>
        public HeightFieldEntity(string name, string textureResource, MaterialProperties groundMaterial)
        {
            State.Assets.DefaultTexture = textureResource;
            State.Name = name;
            HeightFieldShapeProperties hf = new HeightFieldShapeProperties("height field",
                8, // number of rows
                1000, // distance in meters, between rows
                8, // number of columns
                1000, // distance in meters, between columns
                1, // scale factor to multiple height values
                -1000); // vertical extent of the height field. Should be set to large negative values

            hf.Name = name;
            // create array with height samples
            hf.HeightSamples = new HeightFieldSample[hf.RowCount * hf.ColumnCount];
            hf.HeightSamples.Initialize();

            // create a material for the entire field. We could also specify material per sample.
            hf.Material = groundMaterial;
            _heightField = new HeightFieldShape(hf);
        }

        /// <summary>
        /// Initialization constructor
        /// </summary>
        /// <param name="shapeDesc"></param>
        /// <param name="textureResource"></param>
        public HeightFieldEntity(HeightFieldShapeProperties shapeDesc, string textureResource)
        {
            if (shapeDesc == null)
                throw new ArgumentNullException("shapeDesc");
            if (shapeDesc.ColumnCount < 2 || shapeDesc.RowCount < 2)
                throw new ArgumentOutOfRangeException("shapeDesc", "RowCount and ColumnCount must be greater than two");
            if (shapeDesc.RowScale <= 0 || shapeDesc.ColumnScale <= 0)
                throw new ArgumentOutOfRangeException("shapeDesc", "RowScale and ColumnScale must be greater than zero");
            _heightField = new HeightFieldShape(shapeDesc);

            base.State.Assets.DefaultTexture = textureResource;
            base.State.Name = "ground";
        }

        /// <summary>
        /// Initialization
        /// </summary>
        /// <param name="device"></param>
        /// <param name="physicsEngine"></param>
        public override void Initialize(xnagrfx.GraphicsDevice device, PhysicsEngine physicsEngine)
        {
            try
            {
                InitError = string.Empty;
                Flags |= VisualEntityProperties.Ground;

                // add heightfield to physics primitives
                base.State.PhysicsPrimitives.Add(_heightField);

                // set name, all shapes must have a name
                if (string.IsNullOrEmpty(_heightField.State.Name))
                    _heightField.State.Name = State.Name;

                // if we were deserialized from XML, the height samples will be null.
                // TODO: Read from file. For now init to zero
                if (_heightField.HeightFieldState.HeightSamples == null)
                    _heightField.HeightFieldState.HeightSamples =
                        new HeightFieldSample[_heightField.HeightFieldState.RowCount * _heightField.HeightFieldState.ColumnCount];

                //translate height field position to be center of field
                base.State.PhysicsPrimitives[0].State.LocalPose.Position = new Vector3(
                    -(float)_heightField.HeightFieldState.RowCount * _heightField.HeightFieldState.RowScale / 2.0f,
                    0,
                    -(float)_heightField.HeightFieldState.ColumnCount * _heightField.HeightFieldState.ColumnScale / 2.0f);

                World = xna.Matrix.CreateTranslation(TypeConversion.ToXNA(State.PhysicsPrimitives[0].State.LocalPose.Position));

                // build a custom plane mesh
                Meshes.Add(SimulationEngine.ResourceCache.CreateMesh(device, _heightField.State));

                // tone down the specular highlight a bit
                Meshes[0].RenderingMaterials[0].SpecularColor = new PhysicalModel.ColorValue(0.2f, 0.2f, 0.2f);
                Meshes[0].RenderingMaterials[0].Power = 64;

                // ask engine to initialize everything else
                base.Initialize(device, physicsEngine);

                // create a physics entity to represent us in the physics sim
                CreateAndInsertPhysicsEntity(PhysicsEngine);

                // force height field samples to GC we will not need them after this point
                _heightField.HeightFieldState.HeightSamples = null;
            }
            catch (Exception ex)
            {
                HasBeenInitialized = false;
                InitError = ex.ToString();
            }
        }

        /// <summary>
        /// Frame update. Disable frame updates since nothing needs to be done
        /// </summary>
        /// <param name="update"></param>
        public override void Update(FrameUpdate update)
        {
        }
    }

    /// <summary>
    /// Basic terrain entity, read from file height data and builds a matrix of
    /// ageia heightfields, a rendering mesh is created for each chunk
    /// rendering uses frustum culling to reduce chunks to be rendered
    /// </summary>
    [CLSCompliant(true)]
    [DataContract]
    [EditorAttribute(typeof(GenericObjectEditor), typeof(System.Drawing.Design.UITypeEditor))]
    public class TerrainEntity : VisualEntity
    {
        /// <summary>
        /// side of each terrain chunk
        /// actually the side is 257 points,
        /// but one is shared between adiacent patches
        /// </summary>
        public const int ChunkSize = 256;


        /// <summary>
        /// earth circumference in meters
        /// mean earth radius from Geodetic Reference System 1980 (GRS80)
        /// most of GIS express grid resolution as fraction of this; commonly found are
        ///   1 arc second = 30.8 m
        /// 1/3 arc second = 10.2 m
        /// 1/9 arc second =  3.4 m
        /// </summary>
        public const float GeosphereLength = (float)(6371008.7714 * 2.0 * Math.PI);

        private String _terrainFileName;

        /// <summary>
        /// terrain data filename
        /// can be a bitmap XXXX.bmp (use the extension)
        /// or a couple hdr+flt YYYYY (do not use extension)
        /// </summary>
        [DataMember]
        [Description("Terrain data filename.")]
        public String TerrainFileName
        {
            get { return _terrainFileName; }
            set { _terrainFileName = value; }
        }

        // height data, multiplied by 10, to conserve 1/10 of unit resolution
        private short[,] _heightData;

        // height offset from real height, (multiplied by 10)
        // to have things closer to origin
        // calculated as maxh+minh / 2
        private short _offset;

        // distance between samples, AKA grid resolution
        // is the same in both directions
        private float _sampleDist;

        // for frustum culling, keep centers and extent of chunks
        // but all the chunks have the same size. Since it's terrain,
        private xna.Vector3[,] _centers;
        private float _radius;

        // heightfield chunks
        private HeightFieldShape[,] _heightChunks;

        /// <summary>
        /// Default filenames for terrain ctor
        /// </summary>
        private const string _defaultTerrainHeightFileName = "terrain.bmp";
        private const string _defaultTerrainTextureFileName = "terrain_tex.jpg";

        /// <summary>
        /// open simple bmp file with heights
        /// </summary>
        private void FillDataFromFileBMP(string fileName)
        {
            int dimX, dimY;     // grid size

            Bitmap img = ResourceCache.GlobalInstance.CreateBitmapFromFile(fileName);
            dimX = img.Width;
            dimY = img.Height;

            _sampleDist = 1.0f; // 1 meter between samples

            //now i know the chunk radius
            //side x sqrt(2) + something... 1.42
            _radius = (ChunkSize * _sampleDist) * 1.42f;

            // create storage space for heights
            _heightData = new short[dimX, dimY];

            // no vertical offset for bitmaps
            _offset = 0;

            // copying the data
            float hg;
            for (int yy = 0; yy < dimY; yy++)
                for (int xx = 0; xx < dimX; xx++)
                {
                    hg = (img.GetPixel(xx, yy).R - 128f) / 10.0f;

                    // multiply by ten to have the first decimal,
                    // ageia heightfields uses short, but multiplying by 10
                    // and then selecting 0.1 as heightscale will preserve
                    // 1/10 resolution
                    hg = hg * 10.0f;

                    _heightData[xx, yy] = (short)hg;
                }

        }

        /// <summary>
        /// open file couple .hdr(header) .flt(raw data),
        /// initialize the heightmap structure
        /// read and copy data from file to heightfield.
        /// </summary>
        private void FillDataFromFileFLT(string fileName)
        {
            String inpfname;    // full path
            int dimX, dimY;     // grid size
            StreamReader inpfs; // stream for string read
            FileStream inpfd;   // stream for data reading

            // opening input file (.hdr), this is header.
            // header contains row and column number, plus information on samplig rate
            inpfname = PathManager.GetFullPathForMediaFile(fileName + ".hdr");
            inpfs = new StreamReader(inpfname);

            String result;
            String trim = "NCOLSncolsNROWSnrowsCELLSIZEcellsize ";

            // number of cols
            result = inpfs.ReadLine();
            result = result.Trim(trim.ToCharArray());

            dimX = Convert.ToInt32(result);

            // number of rows
            result = inpfs.ReadLine();
            result = result.Trim(trim.ToCharArray());

            dimY = Convert.ToInt32(result);

            // skip x ll corner
            result = inpfs.ReadLine();
            // skip y ll corner
            result = inpfs.ReadLine();

            // resolution (in terms of angle)
            result = inpfs.ReadLine();
            result = result.Trim(trim.ToCharArray());

            _sampleDist = (float)((GeosphereLength / 360.0) * Convert.ToDouble(result));

            //now i know the chunk radius
            //side x sqrt(2) + something... 1.42
            _radius = (ChunkSize * _sampleDist) * 1.42f;

            inpfs.Close();

            // to have perfect tiling, data should be multiple of
            // chunksize (+1). we will add some rows, cols to fill the gap
            int restX = 0, restY = 0;
            if ((dimX % ChunkSize) != 1)
            {
                restX = 1 + ChunkSize - (dimX % ChunkSize);
            }

            if ((dimY % ChunkSize) != 1)
            {
                restY = 1 + ChunkSize - (dimY % ChunkSize);
            }

            // opening (.flt), real data is here, just a matrix of ieee32float, row major order
            inpfname = PathManager.GetFullPathForMediaFile(fileName + ".flt");
            inpfd = new FileStream(inpfname, FileMode.Open);
            BinaryReader filereader = new BinaryReader(inpfd);

            float data;
            short minh = 10000;
            short maxh = -10000;
            _heightData = new short[dimX + restX, dimY + restY];

            // careful, the X coordinates are inverted in this file format
            for (int iy = 0; iy < dimY; iy++)
                for (int ix = dimX - 1; ix >= 0; ix--)
                {
                    data = filereader.ReadSingle();

                    _heightData[ix, iy] = (short)(data * 10);
                    // multiply by ten to have the first decimal,
                    // ageia heightfields uses short, but multiplying by 10
                    // and then selecting 0.1 as heightscale will preserve
                    // 1/10 resolution (10 cm if meters)
                    if (minh > _heightData[ix, iy])
                        minh = _heightData[ix, iy];
                    if (maxh < _heightData[ix, iy])
                        maxh = _heightData[ix, iy];
                }

            _offset = (short)((minh + maxh) / 2);

            // subtract the offset to all data, to have
            // terrain vertically centered in origin
            for (int iy = 0; iy < dimY; iy++)
                for (int ix = 0; ix < dimX; ix++)
                {
                    _heightData[ix, iy] -= _offset;
                }

            // filling the extension with zeros
            for (int iy = dimY; iy < dimY + restY; iy++)
                for (int ix = 0; ix < dimX; ix++)
                {
                    _heightData[ix, iy] = _heightData[ix, dimY - 1];
                }
            for (int iy = 0; iy < dimY; iy++)
                for (int ix = dimX; ix < dimX + restX; ix++)
                {
                    _heightData[ix, iy] = _heightData[dimX - 1, iy];
                }

            inpfd.Close();
        }

        private void CreateChunks()
        {
            int numX, numY;

            numX = (_heightData.GetLength(0) - 1) / ChunkSize;
            numY = (_heightData.GetLength(1) - 1) / ChunkSize;

            //initialize centers vector
            _centers = new xna.Vector3[numX, numY];

            // allocating chunks
            _heightChunks = new HeightFieldShape[numX, numY];

            for (int cy = 0; cy < numY; cy++)
                for (int cx = 0; cx < numX; cx++)
                {
                    FillChunk(cx, cy);
                }

        }

        private void FillChunk(int indexX, int indexY)
        {

            HeightFieldShapeProperties hf = new HeightFieldShapeProperties("TerrainProps",
                            ChunkSize + 1, // number of data rows, equals to grid size+1
                            _sampleDist,   // distance in meters, between rows
                            ChunkSize + 1, // number of columns, equals to grid size+1
                            _sampleDist,   // distance in meters, between columns
                            0.1f,          // scale factor to multiple height values
                            -500);         // vertical extent of the height field.
            // Should be set to large negative values

            hf.Name = State.Name + "_" + indexX.ToString() + "_" + indexY.ToString();

            // create array with height samples
            hf.HeightSamples = new HeightFieldSample[hf.RowCount * hf.ColumnCount];

            // copy height from heightdata
            int index = 0;
            int baseX, baseY; // base index

            baseX = ChunkSize * indexY;
            baseY = ChunkSize * indexX;

            for (int sampy = baseY; sampy < baseY + ChunkSize + 1; sampy++)
                for (int sampx = baseX; sampx < baseX + ChunkSize + 1; sampx++)
                {
                    hf.HeightSamples[index++].Height = _heightData[sampy, sampx];
                }

            // create a material for the entire field. We could also specify material per sample.
            // hf.Material = groundMaterial;

            _heightChunks[indexX, indexY] = new HeightFieldShape(hf);
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public TerrainEntity()
        {
        }

        /// <summary>
        /// Initialization constructor
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="textureResource"></param>
        public TerrainEntity([DefaultParameterValue(_defaultTerrainHeightFileName)] string fileName,
            [DefaultParameterValue(_defaultTerrainTextureFileName)] string textureResource)
            : this(fileName, textureResource, null)
        {
        }

        /// <summary>
        /// Initialization constructor
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="textureResource"></param>
        /// <param name="groundMaterial"></param>
        public TerrainEntity([DefaultParameterValue(_defaultTerrainHeightFileName)] string fileName,
            [DefaultParameterValue(_defaultTerrainTextureFileName)] string textureResource, MaterialProperties groundMaterial)
        {
            _terrainFileName = fileName;

            State.Assets.DefaultTexture = textureResource;
            State.Name = "Terrain";
            State.Assets.Effect = "Terrain.fx";
        }

        /// <summary>
        /// Initialization
        /// </summary>
        /// <param name="device"></param>
        /// <param name="physicsEngine"></param>
        public override void Initialize(xnagrfx.GraphicsDevice device, PhysicsEngine physicsEngine)
        {
            try
            {
                InitError = string.Empty;
                Flags |= VisualEntityProperties.Ground;

                // init height data reading height values from a file
                if (_terrainFileName.EndsWith(".bmp", true, null))
                {
                    FillDataFromFileBMP(_terrainFileName);     // bitmap
                }
                else
                {
                    FillDataFromFileFLT(_terrainFileName);     // flt + hdr
                }

                // create physical terrain chunks
                // from heightfield data
                CreateChunks();

                // world position for the grid center
                // each one of the chunks will have its own position
                float wposX = -(float)(_heightData.GetLength(0) - 1) * _sampleDist / 2.0f;
                float wposY = 0;
                float wposZ = -(float)(_heightData.GetLength(1) - 1) * _sampleDist / 2.0f;

                World = xna.Matrix.CreateTranslation(wposX, wposY, wposZ);

                int yChunkCount = _heightChunks.GetLength(1);
                int xChunkCount = _heightChunks.GetLength(0);
                int primindex = 0;
                for (int chy = 0; chy < yChunkCount; chy++)
                {
                    for (int chx = 0; chx < xChunkCount; chx++)
                    {
                        // add chunk to physics primitives
                        base.State.PhysicsPrimitives.Add(_heightChunks[chx, chy]);

                        // if we were deserialized from XML, the height samples will be null.
                        // TODO: Read from file. For now init to zero
                        if (_heightChunks[chx, chy].HeightFieldState.HeightSamples == null)
                        {
                            _heightChunks[chx, chy].HeightFieldState.HeightSamples =
                                new HeightFieldSample[(ChunkSize + 1) * (ChunkSize + 1)];
                        }

                        //translate height field position to be center of field
                        float posX, posZ;

                        posX = wposX;
                        posZ = wposZ;

                        posX += (ChunkSize * _sampleDist) * chx;
                        posZ += (ChunkSize * _sampleDist) * chy;

                        base.State.PhysicsPrimitives[primindex++].State.LocalPose.Position = new Vector3(posX, 0, posZ);

                        // set center position for the chunk
                        _centers[chx, chy] = new xna.Vector3(posX, 0, posZ);

                        // build a custom plane mesh with max detail
                        Meshes.Add(
                            SimulationEngine.ResourceCache.CreateHeightFieldMesh(device,
                            _heightChunks[chx, chy].HeightFieldState,
                            0,
                            chx,
                            chy,
                            xChunkCount,
                            yChunkCount));
                    }
                }

                // smooth the normals between chunks
                for (int chy = 0; chy < yChunkCount; chy++)
                {
                    int rowOffset = chy * xChunkCount;
                    for (int chx = 0; chx < (xChunkCount - 1); chx++)
                    {
                        VisualEntityMesh.SmoothChunkNormalsTB(Meshes[rowOffset + chx], Meshes[rowOffset + chx + 1], _heightChunks[chx, chy]);
                    }
                }

                for (int chx = 0; chx < xChunkCount; chx++)
                    for (int chy = 0; chy < (yChunkCount - 1); chy++)
                        VisualEntityMesh.SmoothChunkNormalsLR(Meshes[chy * xChunkCount + chx], Meshes[(chy + 1) * xChunkCount + chx], _heightChunks[chx, chy]);


                // ask engine to initialize everything else
                base.Initialize(device, physicsEngine);

                // create a physics entity to represent us in the physics sim
                CreateAndInsertPhysicsEntity(PhysicsEngine);


                for (int chy = 0; chy < yChunkCount; chy++)
                    for (int chx = 0; chx < xChunkCount; chx++)
                    {
                        // force height field samples to GC we will not need them after this point
                        _heightChunks[chx, chy].HeightFieldState.HeightSamples = null;
                    }

                DisableEntityViewFrustumCulling();
            }
            catch (Exception ex)
            {
                HasBeenInitialized = false;
                InitError = ex.ToString();
            }
        }

        /// <summary>
        /// Frame update. Disables update, nothing to do
        /// </summary>
        /// <param name="update"></param>
        public override void Update(FrameUpdate update)
        {
        }

        /// <summary>
        /// Frame render
        /// </summary>
        public override void Render(RenderMode renderMode, MatrixTransforms transforms, CameraEntity currentCamera)
        {
            if ((int)(Flags & VisualEntityProperties.DisableRendering) > 0)
                return;

            // calculate frustum planes
            xna.Vector3[] pnorm = new xna.Vector3[6];
            float[] pdist = new float[6];

            xna.Matrix mft = transforms.View * transforms.Projection;

            // near
            pnorm[0].X = mft.M14 + mft.M13;
            pnorm[0].Y = mft.M24 + mft.M23;
            pnorm[0].Z = mft.M34 + mft.M33;
            pdist[0] = mft.M44 + mft.M43;
            pdist[0] /= pnorm[0].Length();
            pnorm[0].Normalize();

            //far
            pnorm[1].X = mft.M14 - mft.M13;
            pnorm[1].Y = mft.M24 - mft.M23;
            pnorm[1].Z = mft.M34 - mft.M33;
            pdist[1] = mft.M44 - mft.M43;
            pdist[1] /= pnorm[1].Length();
            pnorm[1].Normalize();

            //left
            pnorm[2].X = mft.M14 + mft.M11;
            pnorm[2].Y = mft.M24 + mft.M21;
            pnorm[2].Z = mft.M34 + mft.M31;
            pdist[2] = mft.M44 + mft.M41;
            pdist[2] /= pnorm[2].Length();
            pnorm[2].Normalize();

            //right
            pnorm[3].X = mft.M14 - mft.M11;
            pnorm[3].Y = mft.M24 - mft.M21;
            pnorm[3].Z = mft.M34 - mft.M31;
            pdist[3] = mft.M44 - mft.M41;
            pdist[3] /= pnorm[3].Length();
            pnorm[3].Normalize();

            // excluding top & bottom, this kind of culling is less important since
            // terrain is always on ground level. Moreover, we are culling against a
            // non complete data, we just have center + horizontal redius, the real heignt
            // of the terrain chunks is not taken into account. it's like culling with
            // verical cylinders.

            int i = 0;
            bool skip;
            for (int chy = 0; chy < _heightChunks.GetLength(1); chy++)
            {
                for (int chx = 0; chx < _heightChunks.GetLength(0); chx++)
                {
                    skip = false;
                    for (int pnum = 0; ((pnum < 4) && (!skip)); pnum++)
                    {
                        if (xna.Vector3.Dot(_centers[chx, chy], pnorm[pnum]) + pdist[pnum] + _radius < 0)
                            skip = true;
                    }

                    if (!skip)
                    {
                        RenderShape(renderMode, transforms, State.PhysicsPrimitives[i].State, Meshes[i]);
                    }
                    i++;
                }
            }

        }
    }

    /// <summary>
    /// LOD rendering terrain entity, read from file height data and builds a matrix of
    /// ageia heightfields, a rendering mesh is created for each chunk
    /// rendering uses frustum culling to reduce chunks to be rendered
    /// chunks are rendered with different level of detail, detail is selected and updated
    /// depending on the camera position
    /// </summary>
    [CLSCompliant(true)]
    [DataContract]
    [EditorAttribute(typeof(GenericObjectEditor), typeof(System.Drawing.Design.UITypeEditor))]
    public class TerrainEntityLOD : VisualEntity
    {
        /// <summary>
        /// Side of each terrain chunk
        /// actually the side is 33 points,
        /// but one is shared between adiacent patches
        /// </summary>
        public const int ChunkSize = 32;

        /// <summary>
        /// earth circumference in meters
        /// mean earth radius from Geodetic Reference System 1980 (GRS80)
        /// most of GIS express grid resolution as fraction of this; commonly found are
        ///   1 arc second = 30.8 m
        /// 1/3 arc second = 10.2 m
        /// 1/9 arc second =  3.4 m
        /// </summary>
        public const float GeosphereLength = (float)(6371008.7714 * 2.0 * Math.PI);

        // terrain data filename
        // can be a bitmap XXXX.bmp (use the extension)
        // or a couple hdr+flt YYYYY (do not use extension)
        private String _terrainFileName;

        // height data, multiplied by 10, to conserve 1/10 of unit resolution
        private short[,] _heightData;

        // height offset from real height, (multiplied by 10)
        // to have things closer to origin
        // calculated as maxh+minh / 2
        private short _offset;

        // distance between samples, AKA grid resolution
        // is the same in both directions
        private float _sampleDist;

        // for frustum culling, keep centers and extent of chunks
        // but all the chunks have the same size. Since it's terrain,
        private xna.Vector3[,] _centers;
        private float _radius;

        // LOD data.
        private int _midChunkX;     // center of interest chunk X index
        private int _midChunkY;     // center of interest chunk Y index
        private int[,] _chunkLOD;   // level of detail for each chunk
        // level=0 full res, halving each time

        // element of the update queue, contains coords of the element
        // to be updated and the new LOD level
        struct updateElement
        {
            public int indexX;
            public int indexY;
            public int newLevel;
        }

        private Queue<updateElement> _queueLODUpdate; // queue for LOD update.

        // heightfield chunks
        private HeightFieldShape[,] _heightChunks;

        //-------------------------------------------------------------------------

        // open simple bmp file with heights
        //
        private void fillDataFromFileBMP(string fileName)
        {
            int dimX, dimY;     // grid size

            Bitmap img = ResourceCache.GlobalInstance.CreateBitmapFromFile(fileName);
            dimX = img.Width;
            dimY = img.Height;

            _sampleDist = 1.0f; // 1 meter between samples

            //now i know the chunk radius
            //side x sqrt(2) + something... 1.42
            _radius = (ChunkSize * _sampleDist) * 1.42f;

            // create storage space for heights
            _heightData = new short[dimX, dimY];

            // no vertical offset for bitmaps
            _offset = 0;

            // copying the data
            float hg;
            for (int yy = 0; yy < dimY; yy++)
                for (int xx = 0; xx < dimX; xx++)
                {
                    hg = (img.GetPixel(xx, yy).R - 128f) / 10.0f;

                    // multiply by ten to have the first decimal,
                    // ageia heightfields uses short, but multiplying by 10
                    // and then selecting 0.1 as heightscale will preserve
                    // 1/10 resolution
                    hg = hg * 10.0f;

                    _heightData[xx, yy] = (short)hg;
                }

        }

        // open file couple .hdr(header) .flt(raw data),
        // initialize the heightmap structure
        // read and copy data from file to heightfield.
        private void FillDataFromFileFLT(string fileName)
        {
            String inpfname;    // full path
            int dimX, dimY;     // grid size
            StreamReader inpfs; // stream for string read
            FileStream inpfd;   // stream for data reading

            // opening input file (.hdr), this is header.
            // header contains row and column number, plus information on samplig rate
            inpfname = PathManager.GetFullPathForMediaFile(fileName + ".hdr");
            inpfs = new StreamReader(inpfname);

            String result;
            String trim = "NCOLSncolsNROWSnrowsCELLSIZEcellsize ";

            // number of cols
            result = inpfs.ReadLine();
            result = result.Trim(trim.ToCharArray());

            dimX = Convert.ToInt32(result);

            // number of rows
            result = inpfs.ReadLine();
            result = result.Trim(trim.ToCharArray());

            dimY = Convert.ToInt32(result);

            // skip x ll corner
            result = inpfs.ReadLine();
            // skip y ll corner
            result = inpfs.ReadLine();

            // resolution (in terms of angle)
            result = inpfs.ReadLine();
            result = result.Trim(trim.ToCharArray());

            _sampleDist = (float)((GeosphereLength / 360.0) * Convert.ToDouble(result));

            //now i know the chunk radius
            //side x sqrt(2) + something... 1.42
            _radius = (ChunkSize * _sampleDist) * 1.42f;

            inpfs.Close();

            // to have perfect tiling, data should be multiple of
            // chunksize (+1). we will add some rows, cols to fill the gap
            int restX = 0, restY = 0;
            if ((dimX % ChunkSize) != 1)
            {
                restX = 1 + ChunkSize - (dimX % ChunkSize);
            }

            if ((dimY % ChunkSize) != 1)
            {
                restY = 1 + ChunkSize - (dimY % ChunkSize);
            }

            // opening (.flt), real data is here, just a matrix of ieee32float, row major order
            inpfname = PathManager.GetFullPathForMediaFile(fileName + ".flt");
            inpfd = new FileStream(inpfname, FileMode.Open);
            BinaryReader filereader = new BinaryReader(inpfd);

            float data;
            short minh = 10000;
            short maxh = -10000;
            _heightData = new short[dimX + restX, dimY + restY];

            // careful, the X coordinates are inverted in this file format
            for (int iy = 0; iy < dimY; iy++)
                for (int ix = dimX - 1; ix >= 0; ix--)
                {
                    data = filereader.ReadSingle();

                    _heightData[ix, iy] = (short)(data * 10);
                    // multiply by ten to have the first decimal,
                    // ageia heightfields uses short, but multiplying by 10
                    // and then selecting 0.1 as heightscale will preserve
                    // 1/10 resolution (10 cm if meters)
                    if (minh > _heightData[ix, iy])
                        minh = _heightData[ix, iy];
                    if (maxh < _heightData[ix, iy])
                        maxh = _heightData[ix, iy];
                }

            _offset = (short)((minh + maxh) / 2);

            // subtract the offset to all data, to have
            // terrain vertically centered in origin
            for (int iy = 0; iy < dimY; iy++)
                for (int ix = 0; ix < dimX; ix++)
                {
                    _heightData[ix, iy] -= _offset;
                }

            // filling the extension with zeros
            for (int iy = dimY; iy < dimY + restY; iy++)
                for (int ix = 0; ix < dimX; ix++)
                {
                    _heightData[ix, iy] = _heightData[ix, dimY - 1];
                }
            for (int iy = 0; iy < dimY; iy++)
                for (int ix = dimX; ix < dimX + restX; ix++)
                {
                    _heightData[ix, iy] = _heightData[dimX - 1, iy];
                }

            inpfd.Close();
        }

        private void CreateChunks()
        {
            int numX, numY;

            numX = (_heightData.GetLength(0) - 1) / ChunkSize;
            numY = (_heightData.GetLength(1) - 1) / ChunkSize;

            //initialize centers vector
            _centers = new xna.Vector3[numX, numY];

            // allocating chunks
            _heightChunks = new HeightFieldShape[numX, numY];

            for (int cy = 0; cy < numY; cy++)
                for (int cx = 0; cx < numX; cx++)
                {
                    FillChunk(cx, cy);
                }

        }

        private void FillChunk(int indexX, int indexY)
        {

            HeightFieldShapeProperties hf = new HeightFieldShapeProperties("TerrainProps",
                            ChunkSize + 1, // number of data rows, equals to grid size+1
                            _sampleDist,   // distance in meters, between rows
                            ChunkSize + 1, // number of columns, equals to grid size+1
                            _sampleDist,   // distance in meters, between columns
                            0.1f,          // scale factor to multiple height values
                            -500);         // vertical extent of the height field.
            // Should be set to large negative values

            hf.Name = State.Name + "_" + indexX.ToString() + "_" + indexY.ToString();

            // create array with height samples
            hf.HeightSamples = new HeightFieldSample[hf.RowCount * hf.ColumnCount];

            // copy height from heightdata
            int index = 0;
            int baseX, baseY; // base index

            baseX = ChunkSize * indexY;
            baseY = ChunkSize * indexX;

            for (int sampy = baseY; sampy < baseY + ChunkSize + 1; sampy++)
                for (int sampx = baseX; sampx < baseX + ChunkSize + 1; sampx++)
                {
                    hf.HeightSamples[index++].Height = _heightData[sampy, sampx];
                }

            // create a material for the entire field. We could also specify material per sample.
            //hf.Material = groundMaterial;

            _heightChunks[indexX, indexY] = new HeightFieldShape(hf);
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public TerrainEntityLOD()
        {
        }

        /// <summary>
        /// Initialization constructor
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="textureResource"></param>
        /// <param name="groundMaterial"></param>
        public TerrainEntityLOD(string fileName, string textureResource, MaterialProperties groundMaterial)
        {
            _terrainFileName = fileName;

            State.Assets.DefaultTexture = textureResource;
            State.Name = "Terrain";
            State.Assets.Effect = "Terrain.fx";
        }

        /// <summary>
        /// Initialization
        /// </summary>
        /// <param name="device"></param>
        /// <param name="physicsEngine"></param>
        public override void Initialize(xnagrfx.GraphicsDevice device, PhysicsEngine physicsEngine)
        {
            try
            {
                InitError = string.Empty;
                Flags |= VisualEntityProperties.Ground;

                // init height data reading height values from a file
                if (_terrainFileName.EndsWith(".bmp", true, null))
                {
                    fillDataFromFileBMP(_terrainFileName);     // bitmap
                }
                else
                {
                    FillDataFromFileFLT(_terrainFileName);     // flt + hdr
                }

                // create physical terrain chunks
                // from heightfield data
                CreateChunks();

                // world position for the grid center
                // each one of the chunks will have its own position
                float wposX = -(float)(_heightData.GetLength(0) - 1) * _sampleDist / 2.0f;
                float wposY = 0;
                float wposZ = -(float)(_heightData.GetLength(1) - 1) * _sampleDist / 2.0f;

                World = xna.Matrix.CreateTranslation(wposX, wposY, wposZ);

                // determining initial level of detail for the chunks
                // creating LOD table
                _chunkLOD = new int[_heightChunks.GetLength(0), _heightChunks.GetLength(1)];
                int LODdistance;

                // middle chunk is the starting center of interest
                _midChunkX = (int)Math.Floor((double)(_heightChunks.GetLength(0) / 2));
                _midChunkY = (int)Math.Floor((double)(_heightChunks.GetLength(1) / 2));

                // queue for LOD update initialization
                _queueLODUpdate = new Queue<updateElement>(_heightChunks.GetLength(0) * _heightChunks.GetLength(1) / 2);
                _queueLODUpdate.Clear();
                // filling
                for (int chx = 0; chx < _heightChunks.GetLength(0); chx++)
                    for (int chy = 0; chy < _heightChunks.GetLength(1); chy++)
                    {
                        // manhattan distance = square LOD frontier
                        LODdistance = Math.Max(Math.Abs(_midChunkX - chx), Math.Abs(_midChunkY - chy));

                        if (LODdistance <= 2)
                        {
                            _chunkLOD[chx, chy] = 0;
                        }
                        else if (LODdistance <= 5)
                        {
                            _chunkLOD[chx, chy] = 1;
                        }
                        else if (LODdistance <= 8)
                        {
                            _chunkLOD[chx, chy] = 2;
                        }
                        else
                        {
                            _chunkLOD[chx, chy] = 3;
                        }
                    }

                int primindex = 0;

                for (int chy = 0; chy < _heightChunks.GetLength(1); chy++)
                    for (int chx = 0; chx < _heightChunks.GetLength(0); chx++)
                    {
                        // add chunk to physics primitives
                        base.State.PhysicsPrimitives.Add(_heightChunks[chx, chy]);

                        // if we were deserialized from XML, the height samples will be null.
                        // TODO: Read from file. For now init to zero
                        if (_heightChunks[chx, chy].HeightFieldState.HeightSamples == null)
                            _heightChunks[chx, chy].HeightFieldState.HeightSamples =
                                new HeightFieldSample[(ChunkSize + 1) * (ChunkSize + 1)];

                        //translate height field position to be center of field
                        float posX, posZ;

                        posX = wposX;
                        posZ = wposZ;

                        posX += (ChunkSize * _sampleDist) * chx;
                        posZ += (ChunkSize * _sampleDist) * chy;

                        base.State.PhysicsPrimitives[primindex++].State.LocalPose.Position = new Vector3(posX, 0, posZ);

                        // set center position for the chunk
                        _centers[chx, chy] = new xna.Vector3(posX, 0, posZ);

                        // build a custom plane mesh
                        // first, i create the geometric entity with max detail, so normals are calculated with max LOD
                        // then, i update the structure reflecting the current LOD, in this way i'll keep the higher res normals.
                        // the last four params are used to compute texture coordinates that span across the entire field
                        Meshes.Add(SimulationEngine.ResourceCache.CreateHeightFieldMesh(device,
                            _heightChunks[chx, chy].HeightFieldState,
                            0,
                            chx,
                            chy,
                            _heightChunks.GetLength(0),
                            _heightChunks.GetLength(1)));
                        Meshes[Meshes.Count - 1].UpdateNormals();

                        SimulationEngine.ResourceCache.UpdateHeightFieldMeshLOD(Device, Meshes[Meshes.Count - 1], ChunkSize + 1, _chunkLOD[chx, chy]);

                        // note that it is possible to assign a separate texture to each of these meshes, using the following code
                        // Meshes[Meshes.Count - 1].Textures.Add(SimulationEngine.ResourceCache.CreateTextureFromFile(device, ...));


                    }

                // ask engine to initialize everything else
                base.Initialize(device, physicsEngine);

                // create a physics entity to represent us in the physics sim
                CreateAndInsertPhysicsEntity(PhysicsEngine);


                for (int chy = 0; chy < _heightChunks.GetLength(1); chy++)
                    for (int chx = 0; chx < _heightChunks.GetLength(0); chx++)
                    {
                        // force height field samples to GC we will not need them after this point
                        _heightChunks[chx, chy].HeightFieldState.HeightSamples = null;
                    }

                DisableEntityViewFrustumCulling();
            }
            catch (Exception ex)
            {
                HasBeenInitialized = false;
                InitError = ex.ToString();
            }
        }

        private void DoUpdate()
        {
            int updatenum;
            updateElement thisupdate;
            int updateind;

            updatenum = 0;

            while ((_queueLODUpdate.Count > 0) && (updatenum < 5))
            {
                updatenum++;

                thisupdate = _queueLODUpdate.Dequeue();

                _chunkLOD[thisupdate.indexX, thisupdate.indexY] = thisupdate.newLevel;

                updateind = thisupdate.indexX + (thisupdate.indexY * _heightChunks.GetLength(0));

                SimulationEngine.ResourceCache.UpdateHeightFieldMeshLOD(Device, Meshes[updateind], ChunkSize + 1, _chunkLOD[thisupdate.indexX, thisupdate.indexY]);
            }

        }

        /// <summary>
        /// Frame update
        /// </summary>
        /// <param name="update"></param>
        public override void Update(FrameUpdate update)
        {
            int centerX, centerY;
            int newlod;
            int LODdistance;
            double side = ChunkSize * _sampleDist; // side

            // update chunks in queue
            DoUpdate();

            // now check for new changes
            xna.Vector3 camerap = TypeConversion.ToXNA(update.CameraPose.Position);

            // updating center of interest
            centerX = (int)Math.Floor((double)(camerap.X / side));
            centerY = (int)Math.Floor((double)(camerap.Z / side));

            centerX += (int)Math.Floor((double)(_heightChunks.GetLength(0) / 2));
            centerY += (int)Math.Floor((double)(_heightChunks.GetLength(1) / 2));


            // if changed Center of Interest, calculating new LOD
            if ((_midChunkX != centerX) || (_midChunkY != centerY))
            {
                _midChunkX = centerX;
                _midChunkY = centerY;

                for (int chy = 0; chy < _heightChunks.GetLength(1); chy++)
                    for (int chx = 0; chx < _heightChunks.GetLength(0); chx++)
                    {
                        // manhattan distance = square LOD frontier
                        LODdistance = Math.Max(Math.Abs(_midChunkX - chx), Math.Abs(_midChunkY - chy));

                        if (LODdistance <= 2)
                        {
                            newlod = 0;
                        }
                        else if (LODdistance <= 5)
                        {
                            newlod = 1;
                        }
                        else if (LODdistance <= 8)
                        {
                            newlod = 2;
                        }
                        else
                        {
                            newlod = 3;
                        }

                        if (newlod != _chunkLOD[chx, chy])
                        {
                            updateElement newelem;

                            newelem.indexX = chx;
                            newelem.indexY = chy;
                            newelem.newLevel = newlod;
                            _queueLODUpdate.Enqueue(newelem);
                        }
                    }


            }

        }

        /// <summary>
        /// Frame render
        /// </summary>
        public override void Render(RenderMode renderMode, MatrixTransforms transforms, CameraEntity currentCamera)
        {
            if ((int)(Flags & VisualEntityProperties.DisableRendering) > 0)
                return;

            // calculate frustum planes
            xna.Vector3[] pnorm = new xna.Vector3[6];
            float[] pdist = new float[6];

            xna.Matrix mft = transforms.View * transforms.Projection;

            // near
            pnorm[0].X = mft.M14 + mft.M13;
            pnorm[0].Y = mft.M24 + mft.M23;
            pnorm[0].Z = mft.M34 + mft.M33;
            pdist[0] = mft.M44 + mft.M43;
            pdist[0] /= pnorm[0].Length();
            pnorm[0].Normalize();

            //far
            pnorm[1].X = mft.M14 - mft.M13;
            pnorm[1].Y = mft.M24 - mft.M23;
            pnorm[1].Z = mft.M34 - mft.M33;
            pdist[1] = mft.M44 - mft.M43;
            pdist[1] /= pnorm[1].Length();
            pnorm[1].Normalize();

            //left
            pnorm[2].X = mft.M14 + mft.M11;
            pnorm[2].Y = mft.M24 + mft.M21;
            pnorm[2].Z = mft.M34 + mft.M31;
            pdist[2] = mft.M44 + mft.M41;
            pdist[2] /= pnorm[2].Length();
            pnorm[2].Normalize();

            //right
            pnorm[3].X = mft.M14 - mft.M11;
            pnorm[3].Y = mft.M24 - mft.M21;
            pnorm[3].Z = mft.M34 - mft.M31;
            pdist[3] = mft.M44 - mft.M41;
            pdist[3] /= pnorm[3].Length();
            pnorm[3].Normalize();

            // excluding top & bottom, this vind of culling is less important since
            // terrain is always on ground level. moreover, we are culling against a
            // non complete data, we just have center + horizontal redius, the real heignt
            // of the terrain chunks is not taken into account. it's like culling with
            // verical cylinders.

            int i = 0;
            bool skip;
            for (int chy = 0; chy < _heightChunks.GetLength(1); chy++)
                for (int chx = 0; chx < _heightChunks.GetLength(0); chx++)
                {
                    skip = false;
                    for (int pnum = 0; ((pnum < 4) && (!skip)); pnum++)
                    {
                        if (xna.Vector3.Dot(_centers[chx, chy], pnorm[pnum]) + pdist[pnum] + _radius < 0)
                            skip = true;
                    }

                    if (!skip)
                    {
                        RenderShape(renderMode, transforms, State.PhysicsPrimitives[i].State, Meshes[i]);
                    }
                    i++;
                }
        }
    }

    #endregion

    #region SimEditor rendering
    [BrowsableAttribute(false)] // prevent from being displayed in NewEntity dialog
    class EditorEntity : VisualEntity
    {
        private bool _visible;
        private VisualEntity _target = null;
        private xna.Input.MouseState _cachedMouseState;
        private bool _buttonDown = false;

        private bool _fpLeftDown = false;
        private bool _fpRightDown = false;
        private xna.Input.MouseState _fpCachedMouseState;
        private float _originalScale;
        private Vector2 _originalOffset;


        public VisualEntity Target
        {
            get { return _target; }
            set { _target = value; }
        }

        public Vector2 FloorplanScale = new Vector2(100, 100);
        public Vector2 FloorplanOffset = new Vector2(0, 0);

        public EditorEntity()
        {
            State.Name = "Simulation Editor Entity";
            State.Assets.Effect = "simEditor.fx";
        }

        public override void Initialize(xnagrfx.GraphicsDevice device, PhysicsEngine physicsEngine)
        {
            try
            {
                InitError = string.Empty;
                // create a sphere with radius = 1.0
                Robotics.Simulation.Physics.SphereShapeProperties sphere =
                    new Robotics.Simulation.Physics.SphereShapeProperties();
                sphere.Radius = 1.0f;
                sphere.VerticalSlices = 20;
                sphere.HorizontalSlices = 20;

                Meshes.Add(new VisualEntityMesh(device, sphere));

                Material mat = new Material();
                mat.DiffuseColor = new ColorValue(1f, 1f, 1f, 0.2f);
                Meshes[0].RenderingMaterials[0] = mat;

                base.Initialize(device, physicsEngine);
            }
            catch (Exception ex)
            {
                HasBeenInitialized = false;
                InitError = ex.ToString();
            }
        }

        public void EditUpdateFloorplan(xna.Input.KeyboardState keyboardState, xna.Input.MouseState mouseState, FrameUpdate update)
        {
            FloorplanEntity fpEntity = null;

            if (_target != null)
                fpEntity = _target as FloorplanEntity;

            if (fpEntity != null)
                fpEntity.ProcessKeys(keyboardState);

            // check to see if the left mouse button was pressed
            if (update.WindowHasFocus && mouseState.LeftButton == xna.Input.ButtonState.Pressed)
            {
                if (!_fpLeftDown)
                {
                    // button was just pressed
                    _fpLeftDown = true;
                    _fpCachedMouseState = mouseState;
                    _originalOffset = FloorplanOffset;
                    if ((fpEntity != null) && (mouseState.X >= 0) && (mouseState.Y >= 0))
                        fpEntity.MouseDown(mouseState, keyboardState);

                    return;
                }

                // button was down last frame and is still down, handle drag
                xna.Vector2 delta;
                delta.X = mouseState.X - _fpCachedMouseState.X;
                delta.Y = mouseState.Y - _fpCachedMouseState.Y;

                if ((fpEntity == null) || (!fpEntity.MouseDrag(delta, FloorplanScale)))
                {
                    // move the floorplan scene
                    FloorplanOffset.X = _originalOffset.X + delta.X / FloorplanScale.X;
                    FloorplanOffset.Y = _originalOffset.Y + delta.Y / FloorplanScale.Y;
                }
            }
            else
            {
                if (fpEntity != null)
                    fpEntity.MouseUp();

                _fpLeftDown = false;
            }

            // check to see if the Right mouse button was pressed
            if (update.WindowHasFocus && mouseState.RightButton == xna.Input.ButtonState.Pressed)
            {
                if (!_fpRightDown)
                {
                    // button was just pressed
                    _fpRightDown = true;
                    _fpCachedMouseState = mouseState;
                    _originalScale = FloorplanScale.X;
                    return;
                }

                // button was down last frame and is still down, handle drag
                xna.Vector2 delta;
                delta.X = mouseState.X - _fpCachedMouseState.X;

                float scale = 1.0f;
                
                // scale the floorplan scene
                if (delta.X > 0)
                    scale = 1 + delta.X / 100;
                else
                    scale = 1/(1 - delta.X / 100);

                // scale the floorplan scene
                FloorplanScale.X = _originalScale * scale;
                FloorplanScale.Y = _originalScale * scale;
            }
            else
                _fpRightDown = false;
        }

        public override void Update(FrameUpdate update)
        {
            xna.Input.KeyboardState keyboardState = Utilities.GetKeyboardState();
            xna.Input.MouseState mouseState = Utilities.GetMouseState();

            bool inEditMode = SimulationEngine.GlobalInstance.SimulatorMode == SimulationEngine.SimModeType.Edit;
            bool hasMouseFocus = update.WindowHasFocus && SimulationEngine.GlobalInstance.RenderTargetContains(mouseState.X, mouseState.Y);

            if (update.CurrentRenderMode == Microsoft.Robotics.Simulation.RenderMode.Floorplan)
            {
                EditUpdateFloorplan(keyboardState, mouseState, update);
                return;
            }

            // Perform picking if in edit mode and the mouse has been pressed while the window has focus
            if (inEditMode && hasMouseFocus &&
                mouseState.RightButton == xna.Input.ButtonState.Pressed &&
                _cachedMouseState.RightButton == xna.Input.ButtonState.Released &&
                Device != null)
            {
                CameraEntity activeCamera = update.ActiveCamera;

                xna.Vector3 cameraPos = activeCamera.World.Translation;
                xna.Vector3 mousePos = Device.Viewport.Unproject(new xna.Vector3(Utilities.GetMouseState().X, Utilities.GetMouseState().Y, 1),
                        activeCamera.ProjectionMatrix, activeCamera.ViewMatrix, xna.Matrix.Identity);

                xna.Ray r = new xna.Ray(
                    cameraPos,
                    xna.Vector3.Normalize(mousePos - cameraPos)); // Xna is peculiar and rays need normalized directions for bounding spheres

                // Picking has trouble with the following types
                Type[] typesNotToPick = new Type[] { typeof(SkyEntity), typeof(TerrainEntity), typeof(HeightFieldEntity), typeof(SkyDomeEntity) };

                List<TriangleIntersectionRecord> intersectionRecords = SimulationEngine.GlobalInstance.IntersectRayWithoutTypes(r, SimulationEngine.IntersectInvisFlags.SkipDisabledRendering, typesNotToPick);
                if (intersectionRecords.Count > 0)
                    SimulationEngine.SimUI.SelectSingleEntity(intersectionRecords[0].OwnerEntity);
                else
                    SimulationEngine.SimUI.SelectSingleEntity(null);
            }

            // Drag around the currently selected entity if in edit mode and left control is down
            if (inEditMode && (_target != null) &&
                (update.CurrentRenderMode != Microsoft.Robotics.Simulation.RenderMode.Floorplan) &&
                keyboardState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftControl))
            {
                State.Pose.Position = TypeConversion.FromXNA(_target.EntityWorldBoundingSphere.Center);
                State.Pose.Orientation = TypeConversion.FromXNA(xna.Quaternion.CreateFromRotationMatrix(_target.World));
                _visible = true;

                xna.Matrix transform = xna.Matrix.CreateFromQuaternion(TypeConversion.ToXNA(State.Pose.Orientation)) *
                    xna.Matrix.CreateTranslation(TypeConversion.ToXNA(State.Pose.Position));

                World = xna.Matrix.Multiply(
                    xna.Matrix.CreateScale(_target.EntityWorldBoundingSphere.Radius * 1.05f),
                    transform);

                CameraEntity activeCamera = update.ActiveCamera;
                float distance = (float)(2.5 * _target.EntityBoundingSphere.Radius * 1.05f / Math.Tan(activeCamera.ViewAngle));
                distance = Math.Max(0.1f, distance);
                if (keyboardState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftShift))
                    distance = -distance;

                xna.Vector3 lookAt = TypeConversion.ToXNA(_target.State.Pose.Position);
                xna.Vector3 offset = new xna.Vector3(0, 0, 0);
                if (keyboardState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Left))
                    activeCamera.SetViewParameters(lookAt + new xna.Vector3(distance, 0, 0), lookAt);
                else if (keyboardState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Up))
                    activeCamera.SetViewParameters(lookAt + new xna.Vector3(0, distance, 0), lookAt);
                else if (keyboardState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Right))
                    activeCamera.SetViewParameters(lookAt + new xna.Vector3(0, 0, distance), lookAt);

                // check to see if the mouse button was pressed
                if (update.WindowHasFocus && mouseState.LeftButton == xna.Input.ButtonState.Pressed)
                {
                    if (!_buttonDown)
                    {
                        // button was just pressed
                        _buttonDown = true;
                        _cachedMouseState = mouseState;
                        return;
                    }

                    // button was down last frame and is still down, handle drag
                    xna.Vector2 delta;
                    delta.X = _cachedMouseState.X - mouseState.X;
                    delta.Y = mouseState.Y - _cachedMouseState.Y;
                    _cachedMouseState = mouseState;

                    xna.Vector3 mask;
                    switch (SimulationEngine.SimUI.EditorConstraint)
                    {
                        case Constraint.MoveXYZ: mask = new xna.Vector3(1, 1, 1); break;
                        case Constraint.MoveY: mask = new xna.Vector3(0, 1, 0); break;
                        case Constraint.MoveZ: mask = new xna.Vector3(0, 0, 1); break;
                        case Constraint.MoveX: mask = new xna.Vector3(1, 0, 0); break;
                        case Constraint.MoveXZ: mask = new xna.Vector3(1, 0, 1); break;
                        default: mask = new xna.Vector3(0, 0, 0); break;
                    }

                    if ((mask.X != 0) || (mask.Y != 0) || (mask.Z != 0))
                    {

                        xna.Vector3 translation;

                        xna.Vector3 xScale = new xna.Vector3(-delta.X / 150f);
                        xna.Vector3 yScale = new xna.Vector3(-delta.Y / 150f);
                        translation = mask * (activeCamera.Right * xScale + activeCamera.Up * yScale);
                        if (_target.Parent != null && _target.PhysicsEntity == null)
                            translation = xna.Vector3.TransformNormal(translation, _target.Parent.WorldToLocal);

                        Pose oldPose = _target.State.Pose;
                        _target.Position += translation;
                        if (_target.PhysicsEntity != null)
                            _target.PhysicsEntity.SetPose(_target.State.Pose);

                        SimulationEngine.GlobalInstance.AddUndoPose(_target.State.Name, oldPose, _target.State.Pose);
                    }
                    else
                    {
                        // rotation
                        Pose oldPose = _target.State.Pose;
                        switch (SimulationEngine.SimUI.EditorConstraint)
                        {
                            case Constraint.RotateXYZ: mask = new xna.Vector3(1, 1, 1); break;
                            case Constraint.RotateY: mask = new xna.Vector3(0, 1, 0); break;
                            case Constraint.RotateZ: mask = new xna.Vector3(0, 0, 1); break;
                            case Constraint.RotateX: mask = new xna.Vector3(1, 0, 0); break;
                            default: mask = new xna.Vector3(0, 0, 0); break;
                        }
                        xna.Vector3 Original = _target.Rotation;

                        xna.Vector3 right = activeCamera.Right;
                        xna.Vector3 up = activeCamera.Up;
                        xna.Vector3 xRot = new xna.Vector3(
                            (delta.X / 150f) * right.X + (delta.Y / 150f) * up.X,
                            (delta.X / 150f) * right.Y + (delta.Y / 150f) * up.Y,
                            (delta.X / 150f) * right.Z + (delta.Y / 150f) * up.Z);

                        xna.Quaternion newRot = TypeConversion.ToXNA(_target.State.Pose.Orientation);

                        // Transform rotation if in a reference frame local to parent.
                        xna.Quaternion parentLocalToWorld = xna.Quaternion.Identity,
                            parentWorldToLocal = xna.Quaternion.Identity;
                        if (_target.Parent != null && _target.PhysicsEntity == null)
                        {
                            parentLocalToWorld = xna.Quaternion.CreateFromRotationMatrix(_target.Parent.LocalToWorld);
                            parentWorldToLocal = xna.Quaternion.Inverse(parentLocalToWorld);
                        }

                        newRot = parentWorldToLocal * xna.Quaternion.CreateFromAxisAngle(right, delta.Y / 150f) * parentLocalToWorld * newRot;
                        newRot = parentWorldToLocal * xna.Quaternion.CreateFromAxisAngle(up, -delta.X / 150f) * parentLocalToWorld * newRot;

                        xna.Vector3 newEulerRotation = UIMath.QuaternionToEuler(TypeConversion.FromXNA(newRot));
                        if (mask.X == 0) newEulerRotation.X = Original.X;
                        if (mask.Y == 0) newEulerRotation.Y = Original.Y;
                        if (mask.Z == 0) newEulerRotation.Z = Original.Z;

                        _target.Rotation = newEulerRotation;

                        SimulationEngine.GlobalInstance.AddUndoPose(_target.State.Name, oldPose, _target.State.Pose);
                    }
                }
                else
                {
                    _buttonDown = false;
                }
            }
            else
            {
                _visible = false;
                _buttonDown = false;
            }

            _cachedMouseState = Utilities.GetMouseState();
        }

        internal enum FieldType
        {
            None,
            PositionAll,
            PositionX,
            PositionY,
            PositionZ,
            PositionXZ,
            RotationAll,
            RotationX,
            RotationY,
            RotationZ
        };

        public override void Render(RenderMode renderMode, MatrixTransforms transforms, CameraEntity currentCamera)
        {
            if (_visible)
            {
                // Render hightlight sphere on top of everything
                Batching.Graphics.BlendState = xnagrfx.BlendState.NonPremultiplied;
                Batching.Graphics.DepthStencilState = xnagrfx.DepthStencilState.None;

                base.Render(renderMode, transforms, currentCamera);

                Batching.Graphics.BlendState = xnagrfx.BlendState.Opaque;
                Batching.Graphics.DepthStencilState = xnagrfx.DepthStencilState.Default;

                _target.Render(renderMode, transforms, currentCamera);
            }
        }
    }

    #endregion

    #region Physics Geometry rendering

    [BrowsableAttribute(false)] // prevent from being displayed in NewEntity dialog
    class PhysicsSceneEntity : VisualEntity
    {
        SceneGeometry _geometryBuffers = new SceneGeometry();
        static xnagrfx.VertexDeclaration _vertexDeclaration = PositionColored.VertexDeclaration;

        bool _IsActive;
        public bool IsActive
        {
            get { return _IsActive; }
            set { _IsActive = value; }
        }

        public PhysicsSceneEntity()
        {
            State.Name = "Physics Scene Geometry Renderer";
            State.Assets.Effect = "PhysicsScene.fx";
        }

        public override void Initialize(xnagrfx.GraphicsDevice device, PhysicsEngine physicsEngine)
        {
            base.Initialize(device, physicsEngine);
        }

        public override void Update(FrameUpdate update)
        {
            if (IsActive)
            {
                PhysicsEngine.GetSceneGeometry(_geometryBuffers);
            }
        }

        public override void Render(RenderMode renderMode, MatrixTransforms transforms, CameraEntity currentCamera)
        {
            if (!_IsActive ||
                (_geometryBuffers.PointCount == 0 &&
                 _geometryBuffers.LineCount == 0 &&
                 _geometryBuffers.TriangleCount == 0))
            {
                return;
            }

            // Send transform matrices
            transforms.World = xna.Matrix.Identity;
            Effect.SetMatrixTransforms(transforms);

            Effect.RenderAllPasses("RenderScene", delegate
            {
                Device.Indices = null;
                // find out how many potential line strips there are
                int elim = 0;
                for (int i = 0; i < _geometryBuffers.Lines.Length - 1; i++)
                    if (_geometryBuffers.Lines[i] == _geometryBuffers.Lines[i + 1])
                        elim++;

                if (_geometryBuffers.LineCount > 0)
                {
                    // draw in batches of 1000
                    for (int batch = 1; (batch - 1) * 1000 < _geometryBuffers.LineCount; batch++)
                    {
                        Device.DrawUserPrimitives(
                            xnagrfx.PrimitiveType.LineList,
                            _geometryBuffers.Lines,
                            (batch - 1) * 1000 * 2,   // offset into buffer
                            (batch * 1000) > _geometryBuffers.LineCount ? _geometryBuffers.LineCount - (batch - 1) * 1000 : 1000,
                            PositionColored.VertexDeclaration);
                    }
                }

                if (_geometryBuffers.TriangleCount > 0)
                {
                    // draw in batches of 1000
                    for (int batch = 1; (batch - 1) * 1000 < _geometryBuffers.PointCount; batch++)
                    {
                        Device.DrawUserPrimitives(
                            xnagrfx.PrimitiveType.TriangleList,
                            _geometryBuffers.Triangles,
                            (batch - 1) * 1000 * 3,   // offset into buffer
                            (batch * 1000) > _geometryBuffers.TriangleCount ? _geometryBuffers.TriangleCount - (batch - 1) * 1000 : 1000,
                            PositionColored.VertexDeclaration);
                    }
                }
            });
        }

    }

    #endregion

    #region LightEntity
    /// <summary>
    /// Types of light sources
    /// </summary>
    [DataContract]
    [CLSCompliant(true)]
    public enum LightSourceEntityType : int
    {
        /// <summary>
        /// Unimplimented
        /// </summary>
        None = 0,

        /// <summary>
        /// Directional
        /// </summary>
        Directional = 1,

        /// <summary>
        /// Omni directional
        /// </summary>
        Omni = 2,

        /// <summary>
        /// Spot light
        /// </summary>
        Spot = 3
    }

    /// <summary>
    /// Illuminates the scene
    /// </summary>
    [DataContract]
    [CLSCompliant(true)]
    [EditorAttribute(typeof(GenericObjectEditor), typeof(System.Drawing.Design.UITypeEditor))]
    public class LightSourceEntity : VisualEntity
    {
        #region Constants
        /// <summary>
        /// With no orientation, a light will point down by default.
        /// </summary>
        readonly Vector3 DEF_LIGHT_DIR = new Vector3(0, -1, 0);

        const string OMNILIGHT_MESH_NAME = "OmniLight.obj";
        const string DIRECTIONAL_MESH_NAME = "Arrow.obj";
        const string SPOTLIGHT_MESH_NAME = "Spotlight.obj";

        /// <summary>
        /// This is just the name for the property grid category that light specific properties go under
        /// </summary>
        const string LIGHT_PROPERTY_CATEGORY = "Light Properties";
        #endregion

        #region Fields
        private xna.Vector4 _color;
        private LightSourceEntityType _lightType;
        private float _spotUmbra = 90f;
        private float _falloffStart = 0f;
        private float _falloffEnd = 30f;
        private bool _castsShadows = false;

        private VisualEntityMesh _omniMesh;
        private VisualEntityMesh _directionalMesh;
        private VisualEntityMesh _spotMesh;
        #endregion

        #region Editor Properties
        /// <summary>
        /// Get or set the light type
        /// </summary>
        [DataMember]
        [Category(LIGHT_PROPERTY_CATEGORY)]
        [Description("Defines the behavior of the light")]
        [EditorAttribute(typeof(GenericObjectEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public LightSourceEntityType Type
        {
            get { return _lightType; }
            set
            {
                LightSourceEntityType oldType = _lightType;
                _lightType = value;

                if (oldType != _lightType && SimulationEngine.GlobalInstance != null)
                {
                    SyncMeshToLightType();
                    SimulationEngine.GlobalInstance.RequestShaderReload();
                }
            }
        }

        /// <summary>
        /// Get or set the direction that the light is pointing
        /// </summary>
        [Category(LIGHT_PROPERTY_CATEGORY)]
        [Description("The direction that the light is pointing. In the case of an omni light, this property is not used")]
        public Vector3 Direction
        {
            get
            {
                // Rotate the up vector into the direction of the light using the orientation of the light
                xna.Quaternion temp = new xna.Quaternion(DEF_LIGHT_DIR.X, DEF_LIGHT_DIR.Y, DEF_LIGHT_DIR.Z, 0);
                temp = TypeConversion.ToXNA(State.Pose.Orientation) * temp * xna.Quaternion.Inverse(TypeConversion.ToXNA(State.Pose.Orientation));

                // Filter to hundredths since there's too many numbers to look at
                return new Vector3((float)UIMath.Hundredths(temp.X), (float)UIMath.Hundredths(temp.Y), (float)UIMath.Hundredths(temp.Z));
            }
            set
            {
                // Special case: direction is straight up or down, cross product with up vector will be zero
                if (value.X == 0 && value.Z == 0)
                {
                    if (value.Y < 0)
                    {
                        State.Pose.Orientation = new Quaternion(0, 0, 0, 1);
                    }
                    else
                    {
                        State.Pose.Orientation = Quaternion.FromAxisAngle(1, 0, 0, (float)Math.PI);
                    }
                }
                else // Direction is not straight up or down
                {
                    Vector3 axis = Vector3.Normalize(Vector3.Cross(DEF_LIGHT_DIR, value));
                    float angle = (float)Math.Acos(Vector3.Dot(value, DEF_LIGHT_DIR) / Vector3.Length(value));

                    State.Pose.Orientation = Quaternion.FromAxisAngle(new AxisAngle(axis, angle));
                }
            }
        }

        /// <summary>
        /// Gets the normalized color of the light
        /// </summary>
        [DataMember]
        [Category(LIGHT_PROPERTY_CATEGORY)]
        [Editor(typeof(HDRColorUIEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [Description("Specifies the color of the light")]
        public Vector4 Color
        {
            set { _color = TypeConversion.ToXNA(value); }
            get { return TypeConversion.FromXNA(_color); }
        }

        /// <summary>
        /// Angle of cone created by a spotlight
        /// </summary>
        [DataMember]
        [Category(LIGHT_PROPERTY_CATEGORY)]
        [Description("Specifies the size of the cone of the spotlight")]
        public float SpotUmbra
        {
            get { return _spotUmbra; }
            set { _spotUmbra = value; }
        }

        /// <summary>
        /// Starting falloff for a point or spot light
        /// </summary>
        [DataMember]
        [Category(LIGHT_PROPERTY_CATEGORY)]
        [Description("Light begins to attenuate at this distance")]
        public float FalloffStart
        {
            get { return _falloffStart; }
            set { _falloffStart = value; }
        }

        /// <summary>
        /// Ending falloff for a point or spot light
        /// </summary>
        [DataMember]
        [Category(LIGHT_PROPERTY_CATEGORY)]
        [Description("Light is fully attenuated at this distance")]
        public float FalloffEnd
        {
            get { return _falloffEnd; }
            set { _falloffEnd = value; }
        }

        /// <summary>
        /// Gets or sets whether the light creates shadows
        /// </summary>
        [DataMember]
        [Category(LIGHT_PROPERTY_CATEGORY), Description("Specifies whether the light creates shadows")]
        public bool CastsShadows
        {
            get { return _castsShadows; }
            set { _castsShadows = value; SimulationEngine.GlobalInstance.RequestShaderReload(); }
        }
        #endregion

        #region Other Properties
        ///<summary>
        /// Get the position of the light in world space to send to an effect
        ///</summary>
        internal xna.Vector4 LightPosition
        {
            get { return new xna.Vector4(World.M41, World.M42, World.M43, 1); }
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Default constructor, uses point light type
        /// </summary>
        public LightSourceEntity()
        {
            _lightType = LightSourceEntityType.Omni;
            Color = new Vector4(.8f, .8f, .8f, 1);
            Position = new xna.Vector3(0, 1, 0);
            Direction = new Vector3(0, -1, 0);
            Flags = Flags | VisualEntityProperties.DisableRendering;
        }

        /// <summary>
        /// Initialization constructor
        /// </summary>
        public LightSourceEntity([DefaultParameterValue(LightSourceEntityType.Directional)] LightSourceEntityType lightType)
        {
            _lightType = lightType;
            Color = new Vector4(.8f, .8f, .8f, 1);
            Position = new xna.Vector3(0, 1, 0);
            Direction = new Vector3(0, -1, 0);
            Flags = Flags | VisualEntityProperties.DisableRendering;
        }

        /// <summary>
        /// Initializes light
        /// </summary>
        /// <param name="device"></param>
        /// <param name="physicsEngine"></param>
        public override void Initialize(Xna.Framework.Graphics.GraphicsDevice device, PhysicsEngine physicsEngine)
        {
            base.Initialize(device, physicsEngine);

            _omniMesh = ResourceCache.GlobalInstance.CreateMeshFromFile(device, OMNILIGHT_MESH_NAME);
            _directionalMesh = ResourceCache.GlobalInstance.CreateMeshFromFile(device, DIRECTIONAL_MESH_NAME);
            _spotMesh = ResourceCache.GlobalInstance.CreateMeshFromFile(device, SPOTLIGHT_MESH_NAME);

            SyncMeshToLightType();
        }

        /// <summary>
        /// A LightSourceEntity can not have a custom mesh since it is overriden anyways
        /// </summary>
        /// <param name="device"></param>
        public override void LoadResources(xnagrfx.GraphicsDevice device)
        {
            State.Assets.Mesh = null;
            base.LoadResources(device);
        }
        #endregion

        #region Private Methods
        private void SyncMeshToLightType()
        {
            if (!HasBeenInitialized)
                return;

            Meshes.Clear();

            if (_lightType == LightSourceEntityType.Omni)
                Meshes.Add(_omniMesh);
            else if (_lightType == LightSourceEntityType.Directional)
                Meshes.Add(_directionalMesh);
            else if (_lightType == LightSourceEntityType.Spot)
                Meshes.Add(_spotMesh);
            else if (_lightType == LightSourceEntityType.None)
                Meshes.Clear();

            if (_lightType != LightSourceEntityType.None)
                EntityBoundingSphere = Meshes[0].BoundingSphere;
            else
                EntityBoundingSphere = new xna.BoundingSphere();
        }
        #endregion

        #region Rendering
        /// <summary>
        /// Overload so that lights are always drawn in edit mode
        /// </summary>
        public override void Render(RenderMode renderMode, MatrixTransforms transforms, CameraEntity currentCamera)
        {
            VisualEntityProperties oldFlags = Flags;
            if (SimulationEngine.GlobalInstance.SimulatorMode == SimulationEngine.SimModeType.Edit)
            {
                Flags &= ~VisualEntityProperties.DisableRendering;
            }

            base.Render(renderMode, transforms, currentCamera);

            Flags = oldFlags;
        }
        #endregion

        #region Destruction
        /// <summary>
        ///
        /// </summary>
        public override void Dispose()
        {
            Meshes.Clear();
            base.Dispose();
        }
        #endregion
    }
    #endregion

    /// <summary>
    /// Entity that holds a list of joints used in a Collada scene.
    /// </summary>
    [DataContract]
    [CLSCompliant(true)]
    public class GlobalJointEntity : VisualEntity
    {
        private List<PhysicsJoint> _physicsJoints = new List<PhysicsJoint>();

        List<Joint> _joints = new List<Joint>();
        /// <summary>
        /// A global list of joints in the Collada Scene.
        /// </summary>
        [DataMember]
        public List<Joint> Joints
        {
            get { return _joints; }
            set { _joints = value; }
        }

        string _summary;
        /// <summary>
        /// A textual summary of all the joints
        /// </summary>
        [EditorAttribute(typeof(System.ComponentModel.Design.MultilineStringEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public string Summary
        {
            get { return _summary; }
            set { _summary = value; }
        }

        List<Vector3> _rotationVelocities = new List<Vector3>();

        /// <summary>
        /// A list of automatic rotation velocities for each joint
        /// </summary>
        [DataMember]
        public List<Vector3> RotationVelocities
        {
            get { return _rotationVelocities; }
            set { _rotationVelocities = value; }
        }

        double _simulatedTime;
        /// <summary>
        /// Amount of time joint has been physically simulated for
        /// </summary>
        [DataMember]
        public double SimulatedTime
        {
            get { return _simulatedTime; }
            set { _simulatedTime = value; }
        }

        /// <summary>
        /// Default Constructor
        /// </summary>
        public GlobalJointEntity()
        {
        }

        /// <summary>
        /// Adds a joint to the global joint entity
        /// </summary>
        /// <param name="joint"></param>
        public void AddJoint(Joint joint)
        {
            bool moveJoint = false;
            if (joint.State.Angular != null)
            {
                if ((joint.State.Angular.TwistDrive != null) && (joint.State.Angular.TwistDrive.Mode == JointDriveMode.Velocity))
                {
                    moveJoint = true;
                }
                if ((joint.State.Angular.SwingDrive != null) && (joint.State.Angular.SwingDrive.Mode == JointDriveMode.Velocity))
                {
                    moveJoint = true;
                }
                if ((joint.State.Angular.SlerpDrive != null) && (joint.State.Angular.SlerpDrive.Mode == JointDriveMode.Velocity))
                {
                    moveJoint = true;
                }
            }

            if (moveJoint)  // moveJoint will be false if joint.State.Angular is null
                _rotationVelocities.Add(new Vector3(
                    joint.State.Angular.DriveTargetVelocity.X,
                    joint.State.Angular.DriveTargetVelocity.Y,
                    joint.State.Angular.DriveTargetVelocity.Z));
            else
                _rotationVelocities.Add(new Vector3(0, 0, 0));

            Joints.Add(joint);
        }

        /// <summary>
        /// Initializes the joints and inserts them into the physics engine.
        /// </summary>
        /// <param name="device"></param>
        /// <param name="physicsEngine"></param>
        public override void Initialize(Microsoft.Xna.Framework.Graphics.GraphicsDevice device, PhysicsEngine physicsEngine)
        {
            
            StringBuilder sb = new StringBuilder();

            BoxShapeProperties box = new BoxShapeProperties(
                State.Name,
                0,
                new Pose(),
                new Vector3(100000, 100000, 100000));
            Meshes = new List<VisualEntityMesh> { new VisualEntityMesh(device, box) };

            // Make this shape invisible, kinematic, and not able to collide with other shapes
            State.Flags |= EntitySimulationModifiers.Kinematic;
            State.Flags |= EntitySimulationModifiers.DisableCollisions;

            base.Initialize(device, physicsEngine);

            DeferredTaskQueue.Post(new Task(() =>
                {
                    List<Joint> newJoints = new List<Joint>();
                    foreach (Joint joint in _joints)
                    {
                        if (AssignEntitiesToJoint(joint) == SuccessFailResult.Fail)
                        {
                            HandleJointInitializationFailure(joint, newJoints);
                            continue;
                        }

                        PhysicsJoint pj = PhysicsJoint.Create(joint.State);
                        if (pj != null)
                        {
                            try
                            {
                                PhysicsEngine.InsertJoint(pj);
                            }
                            catch (Exception)
                            {
                                HandleJointInitializationFailure(joint, newJoints);
                                continue;
                            }
                            _physicsJoints.Add(pj);
                            newJoints.Add(pj);

                            ComputeJointSummaryInfo(sb, pj);
                        }
                    }
                    _joints = newJoints;
                    _summary = sb.ToString();
                }));
        }

        private void HandleJointInitializationFailure(Joint joint, List<Joint> newJoints)
        {
            _physicsJoints.Add(null);
            newJoints.Add(joint);
        }

        private static SuccessFailResult AssignEntitiesToJoint(Joint joint)
        {
            if (joint.State.Connectors[0].Entity == null)
            {
                joint.State.Connectors[0].Entity = SimulationEngine.GlobalInstance.ReturnEntity(joint.State.Connectors[0].EntityName);
                // if entity name is non-null, the returned entity from 'ReturnEntity' must be non-null
                if (joint.State.Connectors[0].Entity == null && string.IsNullOrEmpty(joint.State.Connectors[0].EntityName) == false)
                    return SuccessFailResult.Fail; 
            }
            else
                joint.State.Connectors[0].EntityName = ((VisualEntity)joint.State.Connectors[0].Entity).State.Name;

            if (joint.State.Connectors[1].Entity == null)
            {
                joint.State.Connectors[1].Entity = SimulationEngine.GlobalInstance.ReturnEntity(joint.State.Connectors[1].EntityName);
                // if entity name is non-null, the returned entity from 'ReturnEntity' must be non-null
                if (joint.State.Connectors[1].Entity == null && string.IsNullOrEmpty(joint.State.Connectors[1].EntityName) == false)
                    return SuccessFailResult.Fail; 
            }
            else
                joint.State.Connectors[1].EntityName = ((VisualEntity)joint.State.Connectors[1].Entity).State.Name;

            return SuccessFailResult.Success;
        }

        private static void ComputeJointSummaryInfo(StringBuilder sb, PhysicsJoint pj)
        {
            // joint summary debug info
            if ((pj.State.Angular != null) && ((pj.State.Angular.TwistMode == JointDOFMode.Free) || (pj.State.Angular.TwistMode == JointDOFMode.Limited)))
                sb.Append('T');
            else
                sb.Append('_');

            if ((pj.State.Angular != null) && ((pj.State.Angular.Swing1Mode == JointDOFMode.Free) || (pj.State.Angular.Swing1Mode == JointDOFMode.Limited)))
                sb.Append('1');
            else
                sb.Append('_');

            if ((pj.State.Angular != null) && ((pj.State.Angular.Swing2Mode == JointDOFMode.Free) || (pj.State.Angular.Swing2Mode == JointDOFMode.Limited)))
                sb.Append('2');
            else
                sb.Append('_');

            if ((pj.State.Linear != null) && ((pj.State.Linear.XMotionMode == JointDOFMode.Free) || (pj.State.Linear.XMotionMode == JointDOFMode.Limited)))
                sb.Append('X');
            else
                sb.Append('_');

            if ((pj.State.Linear != null) && ((pj.State.Linear.YMotionMode == JointDOFMode.Free) || (pj.State.Linear.YMotionMode == JointDOFMode.Limited)))
                sb.Append('Y');
            else
                sb.Append('_');

            if ((pj.State.Linear != null) && ((pj.State.Linear.ZMotionMode == JointDOFMode.Free) || (pj.State.Linear.ZMotionMode == JointDOFMode.Limited)))
                sb.Append('Z');
            else
                sb.Append('_');

            sb.Append("  ");

            VisualEntity e0 = (VisualEntity)pj.State.Connectors[0].Entity;
            VisualEntity e1 = (VisualEntity)pj.State.Connectors[1].Entity;
            sb.AppendLine(pj.State.Name + " joins " + ((e0 != null) ? e0.State.Name : "null") + " to " + ((e1 != null) ? e1.State.Name : "null"));
        }

        enum SuccessFailResult { Success, Fail }
        private SuccessFailResult ReInitializeJoint(List<Joint> joints, int ithJoint)
        {
            int i = ithJoint;

            if (AssignEntitiesToJoint(joints[i]) == SuccessFailResult.Fail)
                return SuccessFailResult.Fail;

            PhysicsJoint pj = PhysicsJoint.Create(joints[i].State);
            if (pj != null)
            {
                try
                {
                    PhysicsEngine.InsertJoint(pj);
                }
                catch (Exception)
                {
                    return SuccessFailResult.Fail;
                }
                _physicsJoints[i] = pj;
                _joints[i] = pj;

                var sb = new StringBuilder();
                ComputeJointSummaryInfo(sb, pj);
                _summary += sb.ToString();
            }
            return SuccessFailResult.Success;
        }

        /// <summary>
        /// Delete joints upon disposal
        /// </summary>
        public override void Dispose()
        {
            try
            {
                foreach (PhysicsJoint pj in _physicsJoints)
                {
                    if(pj!=null)
                        PhysicsEngine.DeleteJoint(pj);
                }
            }
            catch
            {
            }

            base.Dispose();
        }

        /// <summary>
        /// Update joint positions, if necessary
        /// </summary>
        /// <param name="update"></param>
        public override void Update(FrameUpdate update)
        {
            base.Update(update);
            for(int i=0; i<_rotationVelocities.Count; i++)
            {
                if (_physicsJoints[i] == null)
                {
                    if (ReInitializeJoint(_joints, i) == SuccessFailResult.Fail)
                        continue;
                }

                Vector3 rot = _rotationVelocities[i];
                if((rot.X != 0) || (rot.Y != 0) || (rot.Z != 0))
                {
                    PhysicsJoint pj = (PhysicsJoint)Joints[i];
                    pj.SetAngularDriveVelocity(_rotationVelocities[i]);
                    _rotationVelocities[i] = new Vector3(0, 0, 0);
                }
            }

            // if we step anymore at any given time, Ageia's joint simulator becomes unstable
            const float _maxTimeStep = 1.0f / 15.0f;
            if (SimulationEngine.GlobalInstance.State.Pause == false)
                _simulatedTime += Math.Min(_maxTimeStep, update.ElapsedTime);
        }

        /// <summary>
        /// This object is not rendered. However, we don't want to set the 'DisableRendering' flag
        ///  because we still want the editor to be able to select this entity
        /// </summary>
        /// <param name="renderMode"></param>
        /// <param name="transforms"></param>
        /// <param name="currentCamera"></param>
        public override void Render(VisualEntity.RenderMode renderMode, MatrixTransforms transforms, CameraEntity currentCamera)
        {
        }

        /// <summary>
        /// Workaround design of editor to allow clicking on "container" entities
        /// </summary>
        internal override void UpdateEntityWorldBoundingSphere()
        {
            EntityBoundingSphere = new xna.BoundingSphere();
            base.UpdateEntityWorldBoundingSphere();
            EntityBoundingSphere = new xna.BoundingSphere(EntityWorldBoundingSphere.Center - this.Position, EntityWorldBoundingSphere.Radius);
        }
    }

    /// <summary>
    /// Entity with material that it uses for its mesh, rather than
    /// using the material specified by the mesh.
    /// </summary>
    [DataContract]
    [CLSCompliant(true)]
    public class EntityWithMaterials : VisualEntity
    {
        #region Fields
        private ExtendedMaterial[] _materials;
        private List<xnagrfx.Texture> _textures = new List<xnagrfx.Texture>();
        private List<ConvexMeshShape> _convexShapes = new List<ConvexMeshShape>();
        private List<TriangleMeshShape> _triangleShapes = new List<TriangleMeshShape>();
        private List<SphereShape> _sphereShapes = new List<SphereShape>();
        private List<BoxShape> _boxShapes = new List<BoxShape>();
        #endregion


        #region Properties
        /// <summary>
        /// Get or set the list of materials the entity uses
        /// </summary>
        [DataMember]
        public ExtendedMaterial[] Materials
        {
            get { return _materials; }
            set { _materials = value; }
        }

        /// <summary>
        /// Get or set the list of convex mesh shapes in the entity
        /// </summary>
        [DataMember]
        public List<ConvexMeshShape> ConvexShapes
        {
            get { return _convexShapes; }
            set { _convexShapes = value; }
        }

        /// <summary>
        /// Get or set the list of triangle mesh shapes in the entity
        /// </summary>
        [DataMember]
        public List<TriangleMeshShape> TriangleShapes
        {
            get { return _triangleShapes; }
            set { _triangleShapes = value; }
        }

        /// <summary>
        /// Get or set the list of sphere shapes in the entity
        /// </summary>
        [DataMember]
        public List<SphereShape> SphereShapes 
        {
            get { return _sphereShapes; }
            set { _sphereShapes = value; }
        }

        /// <summary>
        /// Get or set the list of box shapes in the entity
        /// </summary>
        [DataMember]
        public List<BoxShape> BoxShapes 
        {
            get { return _boxShapes; }
            set { _boxShapes = value; }
        }

        /// <summary>
        /// Number of solver iterations performed when processing joint/contacts connected to this body.
        /// </summary>
        [DataMember]
        public int SolverIterationCount { get; set; }

        /// <summary>
        /// Maximum allowed angular velocity.
        /// </summary>
        [DataMember]
        public float MaxAngularVelocity { get; set; }
        #endregion


        #region Initialization
        /// <summary>
        /// Empty constructor
        /// </summary>
        public EntityWithMaterials()
        {
        }

        /// <summary>
        /// Initializes any textures needed from the entity's material list
        /// </summary>
        public override void Initialize(Microsoft.Xna.Framework.Graphics.GraphicsDevice device, PhysicsEngine physicsEngine)
        {
            try
            {

                // add a default texture so that the vertex buffer optimizer will not remove the 
                // texture coordinate information.  This texture will be fixed later.
                State.Assets.DefaultTexture = "Concrete.jpg";

                foreach (ConvexMeshShape cvx in _convexShapes)
                    State.PhysicsPrimitives.Add(cvx);

                foreach (TriangleMeshShape tri in _triangleShapes)
                    State.PhysicsPrimitives.Add(tri);

                foreach (var sphere in SphereShapes)
                    State.PhysicsPrimitives.Add(sphere);

                foreach (var box in BoxShapes)
                    State.PhysicsPrimitives.Add(box);

                base.Initialize(device, physicsEngine);

                if (State.PhysicsPrimitives.Count > 0)
                {
                    CreateAndInsertPhysicsEntity(physicsEngine);
                    PhysicsEntity.MaximumAngularVelocity = MaxAngularVelocity;
                    PhysicsEntity.SolverIterationCount = SolverIterationCount;
                }

                if ((_materials == null) || (_materials.Length == 0))
                {
                    _materials = new ExtendedMaterial[1];
                    _materials[0] = new ExtendedMaterial 
                    { 
                        Material = new Material 
                        { 
                            AmbientColor = new ColorValue(.2f, .2f, .2f),
                            DiffuseColor = new ColorValue(1.0f, 1.0f, 1.0f),
                            EmissiveColor = new ColorValue(0,0,0),
                            Power = 32.0f,
                            SpecularColor = new ColorValue(.4f, .4f, .4f)
                        },
                        TextureFileName = "env2.bmp"
                    };
                }

                // copy the extended materials into the meshes
                int MaterialIndex = 0;
                foreach(VisualEntityMesh mesh in Meshes)
                {
                    for(int batch=0; batch < mesh.RenderingMaterials.Length; batch++)
                    {
                        mesh.RenderingMaterials[batch] = _materials[MaterialIndex].Material;
                        xnagrfx.Texture txt = null;
                        if(!string.IsNullOrEmpty(_materials[MaterialIndex].TextureFileName))
                            txt = SimulationEngine.ResourceCache.CreateTextureFromFile(device, _materials[MaterialIndex].TextureFileName);
                        mesh.Textures[batch] = txt;
                        mesh.TextureFilenames[batch] = _materials[MaterialIndex].TextureFileName;
                        MaterialIndex++;
                        if(MaterialIndex >= _materials.Length)
                            MaterialIndex = _materials.Length-1;
                    }
                }
            }
            catch (Exception ex)
            {
                HasBeenInitialized = false;
                InitError = ex.ToString();
            }
        }
        #endregion
    }

    /// <summary>
    /// Type of Sprite Pivots
    /// </summary>
    [DataContract]
    [CLSCompliant(true)]
    public enum SpritePivotType
    {
        /// <summary>
        /// The sprite does not pivot at all
        /// </summary>
        Fixed,
        /// <summary>
        /// The sprite turns on its Y axis to face the active camera
        /// </summary>
        YAxis,
        /// <summary>
        /// The sprite pivots around its center to face the active camera
        /// </summary>
        Center
    }

    /// <summary>
    /// Type of Sprite Positioning
    /// </summary>
    [DataContract]
    [CLSCompliant(true)]
    public enum SpritePositioningType
    {
        /// <summary>
        /// The sprite is defined in local space (default)
        /// </summary>
        Local,
        /// <summary>
        /// The sprite is defined in world space (ie, will remain "Fixed" at a location)
        /// </summary>
        World
    }

    /// <summary>
    /// Base entity for all sprites
    /// </summary>
    [CLSCompliant(true)]
    [DataContract]
    [EditorAttribute(typeof(GenericObjectEditor), typeof(System.Drawing.Design.UITypeEditor))]
    public class SpriteEntity : VisualEntity
    {
        private float _width;

        /// <summary>
        /// Width of the sprite in meters
        /// </summary>
        [DataMember]
        public float Width
        {
            get { return _width; }
            set { _width = value; }
        }

        private float _height;

        /// <summary>
        /// Height of the sprite in meters
        /// </summary>
        [DataMember]
        public float Height
        {
            get { return _height; }
            set { _height = value; }
        }

        private int _textureWidth = 256;

        /// <summary>
        /// Width of the texture in texels
        /// </summary>
        [DataMember]
        public int TextureWidth
        {
            get { return _textureWidth; }
            set { _textureWidth = value; }
        }

        private int _textureHeight = 256;

        /// <summary>
        /// Height of the texture in texels
        /// </summary>
        [DataMember]
        public int TextureHeight
        {
            get { return _textureHeight; }
            set { _textureHeight = value; }
        }

        private SpritePivotType _pivotType;

        /// <summary>
        /// The sprite pivot behavior
        /// </summary>
        [DataMember]
        public SpritePivotType PivotType
        {
            get { return _pivotType; }
            set { _pivotType = value; }
        }

        private SpritePositioningType _positioningType;

        /// <summary>
        /// The sprite positioning behavior
        /// </summary>
        [DataMember]
        public SpritePositioningType PositioningType 
        {
            get { return _positioningType; }
            set { _positioningType = value; }
        }
        
        /// <summary>
        /// Default constructor
        /// </summary>
        public SpriteEntity() { }

        /// <summary>
        /// Initialization constructor with initial bitmap
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="bitmapFile"></param>
        /// <param name="pivot"></param>
        /// <param name="initialPos"></param>
        public SpriteEntity(float width, float height, string bitmapFile, SpritePivotType pivot, Vector3 initialPos)
        {
            base.State.Pose.Position = initialPos;
            _width = width;
            _height = height;
            _pivotType = pivot;
            _positioningType = SpritePositioningType.Local;
            State.Assets.DefaultTexture = bitmapFile;
        }

        /// <summary>
        /// Initialization constructor with initial bitmap
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="bitmapFile"></param>
        /// <param name="pivot"></param>
        /// <param name="initialPos"></param>
        /// <param name="positioning"></param>
        public SpriteEntity(float width, float height, string bitmapFile, SpritePivotType pivot, Vector3 initialPos, 
            SpritePositioningType positioning)
            : this(width, height, bitmapFile, pivot, initialPos)
        {
            PositioningType = positioning;
        }

        /// <summary>
        /// Initialization constructor with empty bitmap
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="textureWidth"></param>
        /// <param name="textureHeight"></param>
        /// <param name="pivot"></param>
        /// <param name="initialPos"></param>
        public SpriteEntity(float width, float height, int textureWidth, int textureHeight, SpritePivotType pivot, Vector3 initialPos)
        {
            base.State.Pose.Position = initialPos;
            _width = width;
            _height = height;
            _textureWidth = textureWidth;
            _textureHeight = textureHeight;
            _pivotType = pivot;
            _positioningType = SpritePositioningType.Local;
        }

        /// <summary>
        /// Initialization constructor with empty bitmap
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="textureWidth"></param>
        /// <param name="textureHeight"></param>
        /// <param name="pivot"></param>
        /// <param name="initialPos"></param>
        /// <param name="positioning"></param>
        public SpriteEntity(float width, float height, int textureWidth, int textureHeight, SpritePivotType pivot, Vector3 initialPos,
            SpritePositioningType positioning)
            : this(width, height, textureWidth, textureHeight, pivot, initialPos)
        {
            PositioningType = positioning;
        }

        /// <summary>
        /// Initialization
        /// </summary>
        /// <param name="device"></param>
        /// <param name="physicsEngine"></param>
        public override void Initialize(xnagrfx.GraphicsDevice device, PhysicsEngine physicsEngine)
        {
            try
            {
                InitError = string.Empty;

                if (!string.IsNullOrEmpty(State.Assets.DefaultTexture))
                {
                    _deferredBitmap = ResourceCache.GlobalInstance.CreateBitmapFromFile(State.Assets.DefaultTexture);
                    if (_deferredBitmap != null)
                        _textureWidth = _textureHeight = 0;
                }

                if ((_textureWidth == 0) && (_deferredBitmap != null))
                    _textureWidth = _deferredBitmap.Width;

                if ((_textureHeight == 0) && (_deferredBitmap != null))
                    _textureHeight = _deferredBitmap.Height;

                if (_textureWidth <= 0 || _textureHeight <= 0)
                {
                    HasBeenInitialized = false;
                    InitError = "TextureWidth and TextureHeight must be > 0";
                    return;
                }

                // don't use the resource cache to create the mesh 
                Meshes.Add(new VisualEntityMesh(device, _width, _height));
                ColorValue black = new ColorValue(0, 0, 0, 255);
                Meshes[0].RenderingMaterials[0].DiffuseColor = black;
                Meshes[0].RenderingMaterials[0].EmissiveColor = black;
                Meshes[0].RenderingMaterials[0].SpecularColor = black;
                Meshes[0].RenderingMaterials[0].AmbientColor = new ColorValue(255, 255, 255, 255);

                // create a texture to apply to the sprite
                Meshes[0].Textures[0] = new xnagrfx.Texture2D(
                    device, _textureWidth, _textureHeight, false,
                    Microsoft.Xna.Framework.Graphics.SurfaceFormat.Color);

                // call base initialize so it can load effect, cache handles, compute
                // bounding geometry
                base.Initialize(device, physicsEngine);

                HasBeenInitialized = true;

                if (_deferredBitmap != null)
                {
                    SetBitmap(_deferredBitmap);
                    _deferredBitmap = null;
                }

                // sprites default to no back face culling since they're a quad in 3D space
                //  (users generally expect to see it from the front and back)
                Flags |= VisualEntityProperties.DisableBackfaceCulling;
            }
            catch (Exception ex)
            {
                HasBeenInitialized = false;
                InitError = ex.ToString();
            }
        }

        /// <summary>
        /// Returns a bitmap with the proper dimensions and format to draw to the sprite texture
        /// </summary>
        /// <returns></returns>
        public Bitmap GetBitmap()
        {
            return new Bitmap(_textureWidth, _textureHeight, PixelFormat.Format32bppArgb);
        }

        Bitmap _deferredBitmap = null;

        /// <summary>
        /// Copies the contents of a bitmap to the sprite texture
        /// </summary>
        /// <param name="bmp"></param>
        public void SetBitmap(Bitmap bmp)
        {
            if (!HasBeenInitialized)
            {
                _deferredBitmap = bmp;
                return;
            }

            try
            {
                Profiler.PushSection("SpriteEntity.SetBitmap", Profiler.SectionType.Update);
                
                Int32[] dst = new Int32[bmp.Width * bmp.Height];
                BitmapData bmd = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                Marshal.Copy(bmd.Scan0, dst, 0, dst.Length);
                bmp.UnlockBits(bmd);
                xnagrfx.Texture2D txt = (xnagrfx.Texture2D)Meshes[0].Textures[0];
                txt.SetData<Int32>(dst);

                Profiler.PopSection();
            }
            catch
            {
            }
        }

        /// <summary>
        /// Update Method that positions the sprite toward the Camera
        /// </summary>
        /// <param name="update"></param>
        public override void Update(FrameUpdate update)
        {
            xna.Vector3 position = TypeConversion.ToXNA(State.Pose.Position);
            VisualEntity savedParent = Parent;
            
            switch (PositioningType)
            {
                case SpritePositioningType.Local:
                    if (Parent != null)
                        position = xna.Vector3.Transform(position, Parent.World);
                    break;

                case SpritePositioningType.World:
                default:
                    Parent = null;
                    break;
            }
                
            switch (_pivotType)
            {
                case SpritePivotType.Center:
                    World = xna.Matrix.CreateBillboard(
                        position,
                        TypeConversion.ToXNA(update.ActiveCamera.State.Pose.Position),
                        update.ActiveCamera.Up, null);
                    break;

                case SpritePivotType.YAxis:
                    {
                        xna.Vector3 modifiedCamera = TypeConversion.ToXNA(update.ActiveCamera.State.Pose.Position);
                        modifiedCamera.Y = State.Pose.Position.Y;
                        World = xna.Matrix.CreateBillboard(
                            position,
                            modifiedCamera,
                            new xna.Vector3(0,1,0), null);
                        break;
                    }

                case SpritePivotType.Fixed:
                    base.Update(update);
                    break;
            }

            Parent = savedParent;
        }
    }

    /// <summary>
    /// Camera sprite.
    /// </summary>
    [CLSCompliant(true)]
    [DataContract]
    [EditorAttribute(typeof(GenericObjectEditor), typeof(System.Drawing.Design.UITypeEditor))]
    [RequiresParentAttribute(ParentTypeAllowed = typeof(CameraEntity), IncludeDerivedTypes = true)]
    public class CameraSprite : SpriteEntity
    {

        private float _captureInterval = 100;
        private DateTime _lastCaptureTime;

#if USE_CONCURRENT_TEXTURE_READ_BACKS
        private Port<int> _textureReadbackPort = new Port<int>();

        static private Dispatcher _readBackTextureDispatcher = null; 
        static private DispatcherQueue _readBackTextureDispatcherQueue = null;
#endif

        /// <summary>
        /// The number of milliseconds to wait between sprite image updates
        /// </summary>
        [DataMember]
        public float CaptureInterval
        {
            get { return _captureInterval; }
            set { _captureInterval = value; }
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public CameraSprite() { }

#if USE_CONCURRENT_TEXTURE_READ_BACKS
        /// <summary>
        /// Static constructor
        /// </summary>
        static CameraSprite()
        {
            if (_readBackTextureDispatcher == null)
            {
                _readBackTextureDispatcher = new Dispatcher(1, ThreadPriority.Lowest, 
                    DispatcherOptions.UseBackgroundThreads|DispatcherOptions.UseProcessorAffinity, "CameraSprite.readBackTextureDispatcher");
                _readBackTextureDispatcherQueue = new DispatcherQueue("CameraSprite.readBackTextureDispatcherQueue", _readBackTextureDispatcher);
            }
        }
#endif

        /// <summary>
        /// Initialization constructor
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="pivot"></param>
        /// <param name="interval"></param>
        /// <param name="initialPos"></param>
        /// <param name="positioning"></param>
        public CameraSprite(float width, float height, SpritePivotType pivot, float interval, Vector3 initialPos, SpritePositioningType positioning)
            : this(width, height, pivot, interval, initialPos)
        {
            PositioningType = positioning;
        }

        /// <summary>
        /// Initialization constructor
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="pivot"></param>
        /// <param name="interval"></param>
        /// <param name="initialPos"></param>
        public CameraSprite(float width, float height, SpritePivotType pivot, float interval, Vector3 initialPos)
        {
            base.State.Pose.Position = initialPos;
            Width = width;
            Height = height;
            PivotType = pivot;
            _captureInterval = interval;
            _lastCaptureTime = DateTime.Now;
            PositioningType = SpritePositioningType.Local;
        }

        /// <summary>
        /// Initialize
        /// </summary>
        /// <param name="device"></param>
        /// <param name="physicsEngine"></param>
        public override void Initialize(Microsoft.Xna.Framework.Graphics.GraphicsDevice device, PhysicsEngine physicsEngine)
        {
            try
            {
                InitError = string.Empty;
                if (Parent == null)
                    throw new Exception("This entity must be a child of another entity.");

                CameraEntity cam = Parent as CameraEntity;
                if (cam == null)
                    throw new Exception("This entity must be a child of a Camera entity.");
                
                if (!cam.IsRealTimeCamera)
                    throw new Exception("This entity must be a child of a realtime Camera entity.");

                TextureWidth = cam.Width;
                TextureHeight = cam.Height;

#if USE_CONCURRENT_TEXTURE_READ_BACKS
                _textureReadbackPort.Clear();
                _textureReadbackPort.Post(1);
#endif

                base.Initialize(device, physicsEngine);
            }
            catch (Exception ex)
            {
                HasBeenInitialized = false;
                InitError = ex.ToString();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="renderMode"></param>
        /// <param name="transforms"></param>
        /// <param name="currentCamera"></param>
        public override void Render(VisualEntity.RenderMode renderMode, MatrixTransforms transforms, CameraEntity currentCamera)
        {
            CameraEntity parentCamera = Parent as CameraEntity;
            if (parentCamera != null && parentCamera.ReadSurface != null)
            {
                Meshes[0].Textures[0] = (xnagrfx.Texture2D)parentCamera.ReadSurface;
                BatchedMesh.Texture = Meshes[0].Textures[0] as xnagrfx.Texture2D;

                base.Render(renderMode, transforms, currentCamera);
            }
        }


        /// <summary>
        /// Update
        /// </summary>
        /// <param name="update"></param>
        public override void Update(FrameUpdate update)
        {
            if (!HasBeenInitialized)
                return;

            base.Update(update);

            CameraEntity cam = (CameraEntity)Parent;

            TimeSpan since = DateTime.Now - _lastCaptureTime;
            if (since.TotalMilliseconds > _captureInterval)
            {
                _lastCaptureTime = DateTime.Now;
                try
                {
                    // get the camera image
#if USE_CONCURRENT_TEXTURE_READ_BACKS
                    Arbiter.Activate( _readBackTextureDispatcherQueue,
                        Arbiter.Receive(false, _textureReadbackPort,
                        (k) =>
                        {
#endif
                    cam.UpdateFrameBuffersFromRenderTargetSurface(cam.ReadSurface);
#if USE_CONCURRENT_TEXTURE_READ_BACKS
                            // let the next read request go through
                            _textureReadbackPort.Post(1);
                        }
                    ));
#endif
                }
                catch
                {
                }
            }
        }

    }

    /// <summary>
    /// Camera sprite.
    /// </summary>
    [CLSCompliant(true)]
    [DataContract]
    [EditorAttribute(typeof(GenericObjectEditor), typeof(System.Drawing.Design.UITypeEditor))]
    [RequiresParentAttribute]
    public class EntityNameSprite : SpriteEntity
    {
        private string _fontFamily;
        private float _fontSize;

        /// <summary>
        /// The font family name of the font to use
        /// </summary>
        [DataMember]
        public string FontFamily
        {
            get { return _fontFamily; }
            set { _fontFamily = value; }
        }

        /// <summary>
        /// The font size of the font to use
        /// </summary>
        [DataMember]
        public float FontSize
        {
            get { return _fontSize; }
            set { _fontSize = value; }
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public EntityNameSprite() { }

        /// <summary>
        /// Initialization constructor
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="textureWidth"></param>
        /// <param name="textureHeight"></param>
        /// <param name="pivot"></param>
        /// <param name="initialPos"></param>
        /// <param name="family"></param>
        /// <param name="fontSize"></param>
        public EntityNameSprite([DefaultParameterValue(256.0f)] float width, [DefaultParameterValue(256.0f)] float height,
            [DefaultParameterValue(256)] int textureWidth, [DefaultParameterValue(256)] int textureHeight, SpritePivotType pivot, Vector3 initialPos,
            [DefaultParameterValue("Arial")] string family, [DefaultParameterValue(14.0f)] float fontSize)
            : base(width, height, textureWidth, textureHeight, pivot, initialPos)
        {
            _fontFamily = family;
            _fontSize = fontSize;
        }

        /// <summary>
        /// Initialize
        /// </summary>
        /// <param name="device"></param>
        /// <param name="physicsEngine"></param>
        public override void Initialize(Microsoft.Xna.Framework.Graphics.GraphicsDevice device, PhysicsEngine physicsEngine)
        {
            try
            {
                InitError = string.Empty;
                if (Parent == null)
                    throw new Exception("This entity must be a child of another entity.");

                Font nameFont = new Font(_fontFamily, _fontSize);
                base.Initialize(device, physicsEngine);
                if (string.IsNullOrEmpty(InitError) == false)
                {
                    HasBeenInitialized = false;
                    return;
                }

                Bitmap target = GetBitmap();
                Graphics g = Graphics.FromImage(target);
                g.FillRectangle(Brushes.DarkBlue, 0, 0, target.Width, target.Height);
                StringFormat nameFormat = new StringFormat();
                nameFormat.Alignment = StringAlignment.Center;
                nameFormat.LineAlignment = StringAlignment.Center;
                g.DrawString(Parent.State.Name, nameFont, Brushes.White, new Rectangle(0, 0, target.Width, target.Height), nameFormat);
                SetBitmap(target);
            }
            catch (Exception ex)
            {
                HasBeenInitialized = false;
                InitError = ex.ToString();
            }
        }
    }


    /// <summary>
    /// Display sprite.
    /// </summary>
    [CLSCompliant(true)]
    [DataContract]
    [EditorAttribute(typeof(GenericObjectEditor), typeof(System.Drawing.Design.UITypeEditor))]
    [RequiresParent]
    public class DisplayEntity : SpriteEntity
    {
        /// <summary>
        /// Refresh rate of the display in Hz
        /// </summary>
        [DataMember]
        public float RefreshRate {get; set;}

        /// <summary>
        /// Display entity default constructor
        /// </summary>
        public DisplayEntity() : base()
        {
        }
        
        /// <summary>
        /// Display entity constructor
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="horizontalResolution"></param>
        /// <param name="verticalResolution"></param>
        /// <param name="refreshRate"></param>
        /// <param name="initialPos"></param>
        public DisplayEntity([DefaultParameterValue(1.333f)] float width, [DefaultParameterValue(1.0f)] float height, 
            [DefaultParameterValue(320)] int horizontalResolution,
            [DefaultParameterValue(240)]  int verticalResolution,
            [DefaultParameterValue(60.0f)] float refreshRate, Vector3 initialPos)
            : base(width, height, horizontalResolution, verticalResolution, SpritePivotType.Fixed, initialPos, SpritePositioningType.Local)
        {
            RefreshRate = refreshRate;
        }


        /// <summary>
        /// Copies the contents of a bitmap to the sprite texture
        /// </summary>
        /// <param name="rgbData"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public void SetImage(byte[] rgbData, int width, int height)
        {
            DeferredTaskQueue.Post(new Task(() =>
                {
                    try
                    {
                        if (rgbData == null || rgbData.Length != 3 * width * height)
                            return;

                        Int32[] dst = new Int32[width * height];
                        CopyRGBToRGBA(dst, rgbData);
                        xnagrfx.Texture2D txt = (xnagrfx.Texture2D)Meshes[0].Textures[0];
                        txt.SetData<Int32>(dst);
                    }
                    catch
                    {
                    }
                }));
        }

        private unsafe void CopyRGBToRGBA(int[] dst, byte[] rgbData)
        {
            fixed (int* pdst = &dst[0])
            {
                fixed (byte* psrc = &rgbData[0])
                {
                    for (int i = 0; i < dst.Length; i++)
                    {
                        pdst[i] = psrc[3 * i] | (psrc[3 * i + 1] << 8) | (psrc[3 * i + 2] << 16) | (0xFF << 24);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Models a camera sensor
    /// </summary>
    [DataContract]
    [EditorAttribute(typeof(GenericObjectEditor), typeof(System.Drawing.Design.UITypeEditor))]
    public class DepthCameraEntity : CameraEntity
    {
        /// <summary>
        /// The outstanding updates
        /// </summary>
        private int outstandingUpdates;

        /// <summary>
        /// The result ports
        /// </summary>
        private List<Port<DepthCameraResult>> resultsPorts;

        /// <summary>
        /// The filename of the HLSL file to use as a depth camera pixel shader
        /// </summary>
        [DataMember]
        [Description("The filename of the HLSL file to use as a depth camera pixel shader.")]
        public string ShaderFile {get; set;}
        
        /// <summary>
        /// Initializes a new instance of the <see cref="DepthCameraEntity"/> class.
        /// </summary>
        public DepthCameraEntity()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DepthCameraEntity"/> class.
        /// </summary>
        /// <param name="viewSizeX">The view size X.</param>
        /// <param name="viewSizeY">The view size Y.</param>
        /// <param name="viewAngle">The view angle.</param>
        /// <param name="shader">Shader file name to use for this depth camera</param>
        public DepthCameraEntity(int viewSizeX, int viewSizeY, float viewAngle, string shader)
            : base(viewSizeX, viewSizeY, viewAngle)
        {
            this.ShaderFile = shader;
        }

        /// <summary>
        /// Registers the specified results port.
        /// </summary>
        /// <param name="resultsPort">The results port.</param>
        public void Register(Port<DepthCameraResult> resultsPort)
        {
            if (this.resultsPorts == null)
            {
                this.resultsPorts = new List<Port<DepthCameraResult>>();
            }

            this.resultsPorts.Add(resultsPort);
        }

        /// <summary>
        /// Initialize as real -time camera with special lens effect
        /// </summary>
        /// <param name="device">The graphics device</param>
        /// <param name="physicsEngine">The physics engine</param>
        public override void Initialize(xnagrfx.GraphicsDevice device, PhysicsEngine physicsEngine)
        {
            if (string.IsNullOrEmpty(this.ShaderFile))
            {
                this.ShaderFile = StandardShaders.Kinect;
            }

            LensEffect = SimulationEngine.ResourceCache.CreateEffectFromFile(device, this.ShaderFile);
            base.Initialize(device, physicsEngine);
        }

        /// <summary>
        /// Updates shader vars
        /// </summary>
        /// <param name="update">The frame update</param>
        public override void Update(FrameUpdate update)
        {
            base.Update(update);

            try
            {
                if (this.resultsPorts == null || this.resultsPorts.Count < 1)
                {
                    return;
                }

                ProcessDeferredTaskQueue();

                if (Interlocked.CompareExchange(ref this.outstandingUpdates, 1, 0) == 1)
                {
                    return;
                }

                var resultPort = new PortSet<int[], Exception>();
                CaptureScene(resultPort);

                SimulationEngine.GlobalInstance.Activate(
                    Arbiter.Receive<int[]>(
                        false,
                        resultPort,
                        data =>
                            {
                                Interlocked.Exchange(ref this.outstandingUpdates, 0);

                                var depthCamResult = new DepthCameraResult
                                    { Width = ViewSizeX, Height = ViewSizeY, Data = data };
                                foreach (var port in this.resultsPorts)
                                {
                                    port.Post(depthCamResult);
                                }
                            }));
            }
            catch
            {
            }
        }

        /// <summary>
        /// Current depth image
        /// </summary>
        public class DepthCameraResult
        {
            /// <summary>
            /// Gets or sets the width.
            /// </summary>
            /// <value>
            /// The width.
            /// </value>
            public int Width { get; set; }

            /// <summary>
            /// Gets or sets the height.
            /// </summary>
            /// <value>
            /// The height.
            /// </value>
            public int Height { get; set; }

            /// <summary>
            /// Gets or sets the data.
            /// </summary>
            /// <value>
            /// The data.
            /// </value>
            public int[] Data { get; set; }
        }

        /// <summary>
        /// Contains definitions for a standard set of shader files
        /// used for depth camera simulations
        /// </summary>
        public class StandardShaders
        {
            /// <summary>
            /// Simulates the standard Kinect behavoir
            /// </summary>
            public const string Kinect = "DepthEffect80cmto400cm.fx";

            /// <summary>
            /// Simulates an alternate Kinect with a greater range of detection
            /// </summary>
            public const string ExtendedKinect = "DepthEffect50cmto1000cm.fx";

            /// <summary>
            /// Simulates an IR sensor with up to 2 meters of detection
            /// </summary>
            public const string IRSensor = "IREffect.fx";

            /// <summary>
            /// Simulates a sonar sensor
            /// </summary>
            public const string SonarSensor = "SonarEffect.fx";
        }

        /// <summary>
        /// Properties for the depth camera
        /// </summary>
        public class DepthCameraProperties
        {
            /// <summary>
            /// Gets or sets the max range.
            /// </summary>
            /// <value>
            /// The max range.
            /// </value>
            public float MaxRange { get; set; }

            /// <summary>
            /// Gets or sets the min range.
            /// </summary>
            /// <value>
            /// The min range.
            /// </value>
            public float MinRange { get; set; }

            /// <summary>
            /// Gets or sets the view size X.
            /// </summary>
            /// <value>
            /// The view size X.
            /// </value>
            public int ViewSizeX { get; set; }

            /// <summary>
            /// Gets or sets the view size Y.
            /// </summary>
            /// <value>
            /// The view size Y.
            /// </value>
            public int ViewSizeY { get; set; }

            /// <summary>
            /// Gets or sets the field of view.
            /// </summary>
            /// <value>
            /// The field of view.
            /// </value>
            public float FieldOfView { get; set; }
        }
    }

    /// <summary>
    /// Models a kinect sensor base
    /// </summary>
    [DataContract]
    [EditorAttribute(typeof(GenericObjectEditor), typeof(System.Drawing.Design.UITypeEditor))]
    public class KinectEntity : SingleShapeEntity
    {
        /// <summary>
        /// Dimension of the kinect base
        /// </summary>
        private static Vector3 kinectDimension = new Vector3(0.07f, 0.02f, 0.07f);

        /// <summary>
        /// Position of the kinect base
        /// </summary>
        private static Vector3 kinectPosition = new Vector3(0.0f, 0.01f, 0.0f);

        /// <summary>
        /// Joint Local Axis
        /// </summary>
        private static Vector3 kinectLocalAxis = new Vector3(0.0f, 0.0f, -1.0f);

        /// <summary>
        /// Joint Normal Axis
        /// </summary>
        private static Vector3 kinectNormalAxis = new Vector3(0.0f, 1.0f, 0.0f);

        /// <summary>
        /// Joint Connection Base
        /// </summary>
        private static Vector3 kinectBaseConnection = new Vector3(0.0f, 0.071f, 0.0f);

        /// <summary>
        /// Joint Connection Camera
        /// </summary>
        private static Vector3 kinectCameraConnection = new Vector3(0.0f, 0.0f, 0.0f);

        /// <summary>
        /// The depth cam
        /// </summary>
        private KinectCameraEntity depthCam;

        /// <summary>
        /// The web cam
        /// </summary>
        private CameraEntity webcam;

        /// <summary>
        /// Prepended to the all child entities
        /// </summary>
        private string prefix;

        /// <summary>
        /// Initializes a new instance of the <see cref="KinectEntity"/> class.
        /// </summary>
        public KinectEntity()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KinectEntity"/> class.
        /// </summary>
        /// <param name="initialPos">The initial pos.</param>
        /// <param name="prefix">Prepended to entity name</param>
        /// <remarks>
        /// Custom constructor for building model from hard coded values. Used to create entity programmatically
        /// </remarks>
        public KinectEntity(Vector3 initialPos, string prefix)
        {
            State.MassDensity.Mass = 0.1f;
            this.prefix = prefix;
            State.Name = prefix + "KinectEntity";
            State.Pose.Position = initialPos;
        }

        /// <summary>
        /// Gets or sets the Kinect Base Dimension
        /// </summary>
        /// <value>
        /// The dimension.
        /// </value>
        [DataMember]
        [Description("Kinect Base Dimension")]
        public Vector3 Dimension
        {
            get { return kinectDimension; }
            set { kinectDimension = value; }
        }

        /// <summary>
        /// Gets or sets the DepthCam
        /// </summary>
        [DataMember]
        [Description("Kinect Camera Entity")]
        public KinectCameraEntity DepthCam
        {
            get { return this.depthCam; }
            set { this.depthCam = value; }
        }

        /// <summary>
        /// Gets or sets the webcam
        /// </summary>
        [DataMember]
        [Description("Web Camera Entity")]
        public CameraEntity WebCam
        {
            get { return this.webcam; }
            set { this.webcam = value; }
        }

        /// <summary>
        /// Initialize the entity
        /// </summary>
        /// <param name="device">The graphics device</param>
        /// <param name="physicsEngine">The physics engine</param>
        public override void Initialize(
            Microsoft.Xna.Framework.Graphics.GraphicsDevice device, 
            PhysicsEngine physicsEngine)
        {
            if (this.depthCam == null)
            {
                this.depthCam = new KinectCameraEntity(this.prefix)
                {
                    IsRealTimeCamera = true,
                    UpdateInterval = 30,
                    ShadowDisplay = CameraEntity.ShadowDisplayMode.HideShadows,
                    CameraModel = CameraEntity.CameraModelType.AttachedChild,

                    // The depth camera can only see out to 4m
                    // We set the near plane to be 10cm in front of the robot in order to prevent
                    // the robot's mesh from occluding any actual obstacles
                    Near = 0.1f,
                    Far = 4f,

                    // Standard depth cam view size
                    ViewSizeX = 320,
                    ViewSizeY = 240,
                    FieldOfView = 43, //default is 43-degress fov like real Kinect
                };
                this.InsertEntity(this.DepthCam);
            }

            if (this.webcam == null)
            {
                this.webcam = new CameraEntity()
                {
                    IsRealTimeCamera = true,
                    UpdateInterval = 30,
                    ShadowDisplay = CameraEntity.ShadowDisplayMode.ShowShadows,
                    CameraModel = CameraEntity.CameraModelType.AttachedChild,

                    ViewSizeX = this.depthCam.ViewSizeX,
                    ViewSizeY = this.depthCam.ViewSizeY,
                    FieldOfView = this.depthCam.FieldOfView,

                    Position = this.depthCam.Position,
                };
                this.webcam.State.Name = prefix + "SimulatedWebcam";
                this.InsertEntity(this.WebCam);
            }

            var kinectDesc = new BoxShapeProperties(
                "KinectBaseBox",
                0.001f,
                new Pose(kinectPosition),
                kinectDimension);

            this.BoxShape = new BoxShape(kinectDesc);

            this.State.Assets.Mesh = "kinectbase.obj";
            this.MeshScale = new Vector3(0.01f, 0.01f, 0.01f);
            this.MeshRotation = new Vector3(0.0f, 180.0f, 0.0f);

            base.Initialize(device, physicsEngine);
        }
    }

    /// <summary>
    /// Models a kinect sensor
    /// </summary>
    [DataContract]
    [EditorAttribute(typeof(GenericObjectEditor), typeof(System.Drawing.Design.UITypeEditor))]
    public class KinectCameraEntity : DepthCameraEntity
    {
        /// <summary>
        /// Dimension for the Kinect sensor
        /// </summary>
        private Vector3 kinectDimension = new Vector3(0.25f, 0.045f, 0.07f);

        /// <summary>
        /// Position of the kinect sensor
        /// </summary>
        private Vector3 kinectPosition = new Vector3(0.0f, 0.054f, 0.0f);

        /// <summary>
        /// Initializes a new instance of the <see cref="KinectCameraEntity"/> class.
        /// </summary>
        public KinectCameraEntity()
        {
            this.State.MassDensity.Mass = 0.1f;
            this.State.Name = "KinectCamera";
            this.State.Pose.Position = this.kinectPosition;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KinectCameraEntity"/> class.
        /// </summary>
        /// <param name="prefix">Prepended to entity name</param>
        public KinectCameraEntity(string prefix)
        {
            this.State.MassDensity.Mass = 0.1f;
            this.State.Name = prefix + "KinectCamera";
            this.State.Pose.Position = this.kinectPosition;
        }

        /// <summary>
        /// Gets or sets the Kinect Dimension
        /// </summary>
        [DataMember]
        [Description("Kinect Dimension")]
        public Vector3 Dimension
        {
            get { return this.kinectDimension; }
            set { this.kinectDimension = value; }
        }

        /// <summary>
        /// Initialize the entity
        /// </summary>
        /// <param name="device">The graphics device</param>
        /// <param name="physicsEngine">The physics engine</param>
        public override void Initialize(Microsoft.Xna.Framework.Graphics.GraphicsDevice device, PhysicsEngine physicsEngine)
        {
            var kinectDesc = new BoxShapeProperties(
                "KinectCameraEntityBox",
                0.001f,
                new Pose(this.kinectPosition),
                this.kinectDimension);

            this.State.Assets.Mesh = "kinectcamera.obj";
            this.MeshScale = new Vector3(0.01f, 0.01f, 0.01f);
            this.MeshRotation = new Vector3(0.0f, 180.0f, 0.0f);

            base.Initialize(device, physicsEngine);
        }
    }
}