//------------------------------------------------------------------------------
//  <copyright file="SingleAxisMultipleJointsAlternate.cs" company="Microsoft Corporation">
//      Copyright (C) Microsoft Corporation.  All rights reserved.
//  </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Robotics.Services.Sensors.Kinect
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Ccr.Core;
    using Microsoft.Dss.Core.Attributes;
    using Microsoft.Dss.ServiceModel.Dssp;
    using Microsoft.Dss.ServiceModel.DsspServiceBase;

    using W3C.Soap;

    using pantilt = Microsoft.Robotics.Services.PanTilt.Proxy;
    using saj = Microsoft.Robotics.Services.SingleAxisJoint.Proxy;
    using submgr = Microsoft.Dss.Services.SubscriptionManager;

    /// <summary>
    /// Kinect camera service implementation
    /// </summary>   
    public partial class KinectService : DsspServiceBase
    {
        /// <summary>
        /// Pan tilt port name
        /// </summary>
        private const string PanTiltPortFieldName = "panTiltPort";

        /// <summary>
        /// Number of degrees per radian.
        /// </summary>
        private const double DegreesPerRadian = 180.0 / Math.PI;

        /// <summary>
        /// Gets or sets the state of the kinect pan/tilt mechanism.
        /// Standard Kinect only supports tilt.
        /// </summary>
        private pantilt.PanTiltState panTiltState;

        /// <summary>
        /// Web cam port
        /// </summary>
        [AlternateServicePort("/pantilt", AlternateContract = pantilt.Contract.Identifier)]
        private pantilt.PanTiltOperationsPort panTiltPort = new pantilt.PanTiltOperationsPort();

        /// <summary>
        /// Get handler for state
        /// </summary>
        /// <param name="get">Get operation</param>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent, PortFieldName = PanTiltPortFieldName)]
        public void OnGet(pantilt.Get get)
        {
            get.ResponsePort.Post(this.panTiltState);
        }

        /// <summary>
        /// RotateSingleAxis handler
        /// </summary>
        /// <param name="rotate">Rotate request</param>
        /// <returns>A task</returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive, PortFieldName = PanTiltPortFieldName)]
        public IEnumerator<ITask> OnRotateSingle(pantilt.Rotate rotate)
        {
            // Just convert this to a kinect tilt operation
            if (rotate.Body.RotatePanRequest != null &&
                rotate.Body.RotatePanRequest.TargetRotationAngleInRadians != 0)
            {
                rotate.ResponsePort.Post(Fault.FromCodeSubcodeReason(
                    FaultCodes.Receiver,
                    DsspFaultCodes.ActionNotSupported,
                    Resources.PanNotSupported));
                yield break;
            }

            if (rotate.Body.RotateTiltRequest == null)
            {
                // no-op
                rotate.ResponsePort.Post(DefaultUpdateResponseType.Instance);
                yield break;
            }

            if (rotate.Body.RotateTiltRequest.IsMotionCompletionRequiredForResponse != false)
            {
                rotate.ResponsePort.Post(Fault.FromCodeSubcodeReason(
                    FaultCodes.Receiver,
                    DsspFaultCodes.ActionNotSupported,
                    Resources.ResponseOnMotionCompletedNotSupported));
                yield break;
            }

            if (rotate.Body.RotateTiltRequest.TargetAccelerationInRadiansPerSecondSecond != 0)
            {
                LogWarning(Resources.AccelerationIgnored);
            }

            if (rotate.Body.RotateTiltRequest.MaxSpeedInRadiansPerSecond != 0)
            {
                LogWarning(Resources.SpeedIgnored);
            }

            double targetTiltInDegrees;
            if (rotate.Body.RotateTiltRequest.IsRelative)
            {
                targetTiltInDegrees =
                    this.state.TiltDegrees + (rotate.Body.RotateTiltRequest.TargetRotationAngleInRadians * DegreesPerRadian);
            }
            else
            {
                targetTiltInDegrees = rotate.Body.RotateTiltRequest.TargetRotationAngleInRadians * DegreesPerRadian;
            }
                    
            this.panTiltState.TiltState.JointCommand.TargetAngleInRadians = targetTiltInDegrees / DegreesPerRadian;

            var updateTilt = new UpdateTilt
            {
                Body = { Tilt = targetTiltInDegrees },
                ResponsePort = rotate.ResponsePort
            };

            this.mainPort.Post(updateTilt);

            yield break;
        }

        /// <summary>
        /// Updates the state of the pan tilt.
        /// </summary>
        /// <param name="subscribe">Subscribe operation</param>
        [ServiceHandler(PortFieldName = PanTiltPortFieldName)]
        public void OnSubscribe(pantilt.Subscribe subscribe)
        {
            SubscribeHelper(this.submgrPort, subscribe, subscribe.ResponsePort);
        }

        /// <summary>
        /// Replace handler for state
        /// </summary>
        /// <param name="replace">Replace operation</param>
        [ServiceHandler(ServiceHandlerBehavior.Independent, PortFieldName = PanTiltPortFieldName)]
        public void OnReplace(pantilt.Replace replace)
        {
            // Can't actually send a replace to this service, but it's here
            // for symmetry with replace events that are raised as the device
            // signals changes to it.
            throw new NotSupportedException();
        }

        /// <summary>
        /// UpdateMotionBlocked handler.
        /// </summary>
        /// <param name="update">UpdateMotionBlocked operation</param>
        [ServiceHandler(ServiceHandlerBehavior.Independent, PortFieldName = PanTiltPortFieldName)]
        public void OnUpdateMotionBlocked(pantilt.UpdateMotionBlocked update)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// FindJointIndexPosition handler.
        /// </summary>
        /// <param name="update">FindJointIndexPosition operation</param>
        [ServiceHandler(ServiceHandlerBehavior.Independent, PortFieldName = PanTiltPortFieldName)]
        public void OnFindJointIndexPosition(pantilt.FindJointIndexPosition update)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// SetHoldingTorque handler.
        /// </summary>
        /// <param name="update">SetHoldingTorque operation</param>
        [ServiceHandler(ServiceHandlerBehavior.Independent, PortFieldName = PanTiltPortFieldName)]
        public void OnSetHoldingTorque(pantilt.SetHoldingTorque update)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// StartTrajectory handler.
        /// </summary>
        /// <param name="update">StartTrajectory operation</param>
        [ServiceHandler(ServiceHandlerBehavior.Independent, PortFieldName = PanTiltPortFieldName)]
        public void OnStartTrajectory(pantilt.StartTrajectory update)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// SetPDGains handler.
        /// </summary>
        /// <param name="update">SetPDGains operation</param>
        [ServiceHandler(ServiceHandlerBehavior.Independent, PortFieldName = PanTiltPortFieldName)]
        public void OnSetPDGains(pantilt.SetPDGains update)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Initializes the state of the pan tilt.
        /// </summary>
        /// <returns>The state</returns>
        private static pantilt.PanTiltState InitialPanTiltState()
        {
            var panTiltState = new pantilt.PanTiltState
            {
                PanState = null,
                TiltState = new saj.SingleAxisJointState
                {
                    JointFeedback = new saj.JointFeedback(),
                    JointCommand = new saj.JointCommand(),
                    JointInformation = new saj.JointInformation
                    {
                        BaseAttachPose = new PhysicalModel.Proxy.Pose { Orientation = new PhysicalModel.Proxy.Quaternion(0, 0, 0, 1) },
                        MinimumRotationAngleInRadians = (double)KinectReservedSampleValues.MinimumTiltAngle / DegreesPerRadian,
                        MaximumRotationAngleInRadians = (double)KinectReservedSampleValues.MaximumTiltAngle / DegreesPerRadian
                    }
                }
            };

            return panTiltState;
        }

        /// <summary>
        /// Updates the tilt angle.
        /// </summary>
        /// <param name="rotationAngleDegrees">Angle to rotate to</param>
        private void UpdateTiltAngle(double rotationAngleDegrees)
        {
            this.panTiltState.TiltState.JointFeedback.RotationAngleInRadians
                                    = rotationAngleDegrees / DegreesPerRadian;

            var rotate = new pantilt.Rotate();

            rotate.Body.RotateTiltRequest = new saj.RotateSingleAxisRequest();

            rotate.Body.RotateTiltRequest.TargetRotationAngleInRadians =
                this.panTiltState.TiltState.JointFeedback.RotationAngleInRadians;
            rotate.Body.RotateTiltRequest.IsRelative = false;

            SendNotification(this.submgrPort, rotate);
        }
    }
}
