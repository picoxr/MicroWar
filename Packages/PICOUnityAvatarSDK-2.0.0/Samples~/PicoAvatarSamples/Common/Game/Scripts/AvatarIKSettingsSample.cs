using System;
using UnityEngine;
using Pico.Avatar;

namespace Pico.Avatar.Sample
{
    /// <summary>
    /// This class declares some commonly used IK configuration options to simplify the IK configuration of the AvatarSDK.
    /// </summary>
    public class AvatarIKSettingsSample : MonoBehaviour
    {
        /// <summary>
        /// IK target settings
        /// </summary>
        [Serializable]
        public struct TargetSettings
        {
            /// <summary>
            /// IK target transform
            /// </summary>
            public Transform transform;

            /// <summary>
            /// IK target position offset
            /// </summary>
            public Vector3 positionOffset;

            /// <summary>
            /// IK target rotation offset
            /// </summary>
            public Vector3 rotationOffset;

            /// <summary>
            /// Set IK target to default value 
            /// </summary>
            /// <remarks>
            /// Please note that the value of transform will be set to null.
            /// </remarks>
            public void SetDefault()
            {
                transform = null;
                positionOffset = Vector3.zero;
                rotationOffset = Vector3.zero;
            }
        };

        /// <summary>
        /// Head and spine rotation limit settings
        /// </summary>
        [Serializable]
        public struct HeadRotationLimitSettings
        {
            /// <summary>
            /// Whether to enable head rotation limit
            /// </summary>
            public bool head;

            /// <summary>
            /// Whether to enable chest rotation limit
            /// </summary>
            public bool chest;

            /// <summary>
            /// Whether to enable hips rotation limit
            /// </summary>
            public bool hips;

            /// <summary>
            /// Set head and spine rotation limit to default value 
            /// </summary>
            public void SetDefault()
            {
                head = true;
                chest = true;
                hips = true;
            }
        }

        /// <summary>
        /// Head and spine IK settings
        /// </summary>
        [Serializable]
        public struct HeadSettings
        {
            /// <summary>
            /// Head and spine IK target settings
            /// </summary>
            public TargetSettings target;

            /// <summary>
            /// Whether to enable spine bending
            /// </summary>
            public bool enableSpineBend;

            /// <summary>
            /// Weight of spine rotation that influnced by avatar head rotation
            /// </summary>
            [Range(0, 1)]
            public float spineRotationWeight;

            /// <summary>
            /// Head and spine rotation limit settings
            /// </summary>
            public HeadRotationLimitSettings rotationLimits;

            /// <summary>
            /// Set head and spine IK settings to default value
            /// </summary>
            /// <remarks>
            /// Please note that the value of target.transform will be set to null.
            /// </remarks>
            public void SetDefault()
            {
                target.SetDefault();
                target.positionOffset = new Vector3(0.0f, 0.1f, 0.1f);
                
                enableSpineBend = true;
                spineRotationWeight = 0.5f;
                rotationLimits.SetDefault();
            }
        };

        /// <summary>
        /// Hand rotation limit settings
        /// </summary>
        [Serializable]
        public struct HandRotationLimitSettings
        {
            /// <summary>
            /// Whether to enable shoulder rotation limit
            /// </summary>
            public bool shoulder;

            /// <summary>
            /// Whether to enable upper arm rotation limit
            /// </summary>
            public bool upperArm;

            /// <summary>
            /// Whether to enable lower arm rotation limit
            /// </summary>
            public bool lowerArm;

            /// <summary>
            /// Whether to enable hand wrist rotation limit
            /// </summary>
            public bool handWrist;

            /// <summary>
            /// Set hand rotation limit to default value 
            /// </summary>
            public void SetDefault()
            {
                shoulder = true;
                upperArm = true;
                lowerArm = true;
                handWrist = true;
            }
        }

        /// <summary>
        /// Hand IK settings
        /// </summary>
        [Serializable]
        public struct HandSettings
        {
            /// <summary>
            /// left hand IK target settings
            /// </summary>
            public TargetSettings leftTarget;

            /// <summary>
            /// right hand IK target settings
            /// </summary>
            public TargetSettings rightTarget;

            /// <summary>
            /// Whether to enable hands stretching
            /// </summary>
            public bool stretch;

            /// <summary>
            /// Hands rotation limit settings
            /// </summary>
            public HandRotationLimitSettings rotationLimits;

            /// <summary>
            /// Set hands IK settings to default value 
            /// </summary>
            /// <remarks>
            /// Please note that the value of leftTarget/rightTarget.transform will be set to null.
            /// </remarks>
            public void SetDefault()
            {
                leftTarget.SetDefault();
                leftTarget.positionOffset = new Vector3(0.036f, 0.041f, 0.081f);
                leftTarget.rotationOffset = new Vector3(-20, 0, 0);

                rightTarget.SetDefault();
                rightTarget.positionOffset = new Vector3(-0.036f, 0.041f, 0.081f);
                rightTarget.rotationOffset = new Vector3(-20, 0, 0);

                stretch = true;
                rotationLimits.SetDefault();
            }
        };

        /// <summary>
        /// Foot rotation limit settings
        /// </summary>
        [Serializable]
        public struct FootRotationLimitSettings
        {
            /// <summary>
            /// Whether to enable upper leg rotation limit
            /// </summary>
            public bool upperLeg;

            /// <summary>
            /// Whether to enable lower leg rotation limit
            /// </summary>
            public bool lowerLeg;

            /// <summary>
            /// Whether to enable ankle rotation limit
            /// </summary>
            public bool ankle;

            /// <summary>
            /// Set foot rotation limit to default value 
            /// </summary>
            public void SetDefault()
            {
                upperLeg = true;
                lowerLeg = true;
                ankle = true;
            }
        }

        /// <summary>
        /// Procedural footstep settings
        /// </summary>
        [Serializable]
        public struct FootstepSettings
        {
            /// <summary>
            /// Maximum height of each footstep in meters
            /// </summary>
            [Range(0, 1)]
            public float stepHeight;

            /// <summary>
            /// Speed of each footstep
            /// </summary>
            [Range(0, 10)]
            public float stepSpeed;

            /// <summary>
            /// Foot collision radius in meters
            /// </summary>
            [Range(0, 1)]
            public float footCollisionRadius;

            /// <summary>
            /// Moving distance that triggers a footstep in meters
            /// </summary>
            [Range(0, 1)]
            public float positionThreshold;

            /// <summary>
            /// Rotating angle that triggers a footstep in degrees 
            /// </summary>
            [Range(0, 180)]
            public float rotationThreshold;

            /// <summary>
            /// Set procedural footstep settings to default value
            /// </summary>
            public void SetDefault()
            {
                stepHeight = 0.05f;
                stepSpeed = 2.0f;
                footCollisionRadius = 0.1f;
                positionThreshold = 0.1f;
                rotationThreshold = 40.0f;
            }
        }
        
        /// <summary>
        /// Foot IK settings
        /// </summary>
        [Serializable]
        public struct FootSettings
        {
            /// <summary>
            /// Whether to enable procedural footstep
            /// </summary>
            public bool autoFootstep;

            /// <summary>
            /// Procedural footstep settings
            /// </summary>
            public FootstepSettings footstep;

            /// <summary>
            /// left foot IK target settings
            /// </summary>
            public TargetSettings leftTarget;

            /// <summary>
            /// right foot IK target settings
            /// </summary>
            public TargetSettings rightTarget;

            /// <summary>
            /// Feet rotation limit settings
            /// </summary>
            public FootRotationLimitSettings rotationLimits;

            /// <summary>
            /// Set feet IK settings to default value 
            /// </summary>
            /// <remarks>
            /// Please note that the value of leftTarget/rightTarget.transform will be set to null.
            /// </remarks>
            public void SetDefault()
            {
                autoFootstep = true;
                footstep.SetDefault();
                leftTarget.SetDefault();
                rightTarget.SetDefault();
                rotationLimits.SetDefault();
            }
        };

        /// <summary>
        /// Height auto fit parameters
        /// </summary>
        [Serializable]
        public struct HeightAutoFitParam
        {
            /// <summary>
            /// The time threshold for Avatar's feet off the ground in seconds
            /// </summary>
            [Range(-1, 10)]
            public float maxFloatingTime;

            /// <summary>
            /// The crouching time threshold for Avatar in seconds
            /// </summary>
            [Range(-1, 10)]
            public float maxCrouchingTime;

            /// <summary>
            /// The distance threshold for the crouching state of Avatar in meters
            /// </summary>
            [Range(-1, 10)]
            public float crouchingDistance;

            /// <summary>
            /// The crouching distance threshold for Avatar in meters
            /// </summary>
            [Range(-1, 10)]
            public float maxCrouchingDistance;

            /// <summary>
            /// Set height auto fit parameters to default value in standing mode
            /// </summary>
            public void SetStandingModeDefault()
            {
                maxFloatingTime = 1.0f;
                maxCrouchingTime = 2.0f;
                crouchingDistance = 0.2f;
                maxCrouchingDistance = 0.7f;
            }

            /// <summary>
            /// Set height auto fit parameters to default value in sitting mode
            /// </summary>
            public void SetSittingModeDefault()
            {
                maxFloatingTime = 1.0f;
                maxCrouchingTime = 3.0f;
                crouchingDistance = 0.15f;
                maxCrouchingDistance = 0.3f;
            }

            /// <summary>
            /// Convert data format from this 'HeightAutoFitParam' to 'AvatarAutoFitController.AvatarAutoFitParam'
            /// </summary>
            public AvatarAutoFitController.AvatarAutoFitParam toAvatarAutoFitParam()
            {
                AvatarAutoFitController.AvatarAutoFitParam outParam =
                    new AvatarAutoFitController.AvatarAutoFitParam(maxFloatingTime, maxCrouchingTime, crouchingDistance, maxCrouchingDistance);
                return outParam;
            }
        };

        /// <summary>
        /// Standing mode parameters
        /// </summary>
        [Serializable]
        public struct StandingMode
        {
            /// <summary>
            /// Height auto fit parameters in standing mode
            /// </summary>
            public HeightAutoFitParam thresholds;

            /// <summary>
            /// Set standing mode to default value
            /// </summary>
            public void SetDefault()
            {
                thresholds.SetStandingModeDefault();
            }
        };

        /// <summary>
        /// Sitting mode parameters
        /// </summary>
        [Serializable]
        public struct SittingMode
        {
            /// <summary>
            /// Height auto fit parameters in sitting mode
            /// </summary>
            public HeightAutoFitParam thresholds;

            /// <summary>
            /// Whether enable auto stand up in sitting mode
            /// </summary>
            public bool autoStandUp;

            /// <summary>
            /// The distance threshold for automatically standing up in sitting mode
            /// </summary>
            [Range(0, 0.2f)]
            public float autoStandUpDistance;

            /// <summary>
            /// The angle threshold for automatically standing up in sitting mode
            /// </summary>
            [Range(0, 180)]
            public float autoStandUpAngle;

            /// <summary>
            /// The time threshold for automatically standing up in sitting mode
            /// </summary>
            [Range(0, 1)]
            public float autoStandUpTime;

            /// <summary>
            /// Sitting target transform, such as a chair transform
            /// </summary>
            public Transform sittingTarget;

            /// <summary>
            /// Sitting target height in meters, such as a chair height
            /// </summary>
            public float sittingHeight;

            /// <summary>
            /// Set sitting mode to default value
            /// </summary>
            /// <remarks>
            /// Please note that the value of sittingTarget will be set to null.
            /// </remarks>
            public void SetDefault()
            {
                thresholds.SetSittingModeDefault();
                autoStandUp = true;
                autoStandUpDistance = 0.05f;
                autoStandUpAngle = 135f;
                autoStandUpTime = 0f;
                sittingTarget = null;
                sittingHeight = 0f;
            }
        };

        /// <summary>
        /// Height auto fit settings
        /// </summary>
        [Serializable]
        public struct HeightAutoFitSettings
        {
            /// <summary>
            /// Whether to enable height auto fit
            /// </summary>
            public bool enableAutoFitHeight;

            /// <summary>
            /// Camera offset target transform
            /// </summary>
            public Transform cameraOffsetTarget;

            /// <summary>
            /// Height auto fit parameters in standing mode
            /// </summary>
            public StandingMode standingMode;

            /// <summary>
            /// Height auto fit parameters in sitting mode
            /// </summary>
            public SittingMode sittingMode;

            /// <summary>
            /// Set height auto fit settings to default value
            /// </summary>
            /// <remarks>
            /// Please note that the value of cameraOffsetTarget will be set to null.
            /// </remarks>
            public void SetDefault()
            {
                enableAutoFitHeight = false;
                cameraOffsetTarget = null;
                standingMode.SetDefault();
                sittingMode.SetDefault();
            }
        };

        #region Public Fields

        /// <summary>
        /// Avatar IK mode: None, UpperBody, FullBody
        /// </summary>
        public AvatarIKMode ikMode = AvatarIKMode.FullBody;

        /// <summary>
        /// Whether to stop IK when the device is disconnected, tracking is lost, or the distance is too far
        /// </summary>
        public bool autoStopIK = true;

        /// <summary>
        /// Whether use world space or relative space driving
        /// </summary>
        public bool worldSpaceDrive = true;

        /// <summary>
        /// XR origin transform
        /// </summary>
        public Transform XRRoot = null;
        
        /// <summary>
        /// Head and spine IK settings
        /// </summary>
        public HeadSettings head;

        /// <summary>
        /// Hands IK target settings
        /// </summary>
        public HandSettings hands;

        /// <summary>
        /// Feet IK target settings
        /// </summary>
        public FootSettings feet;

        /// <summary>
        /// Height auto fit settings
        /// </summary>
        public HeightAutoFitSettings heightAutoFit;

        #endregion
        
        #region Private Fields

        /// <summary>
        /// Callback for 'OnValidate' message method
        /// </summary>
        private System.Action _onValidateCallback = null;

        #endregion

        /// <summary>
        /// Set this Avatar IK settings to default value 
        /// </summary>
        /// <remarks>
        /// Please note that XRRoot, head.target.transform, hands.leftTarget/rightTarget.transform,
        /// feet.leftTarget/rightTarget.transform, heightAutoFit.cameraOffsetTarget, _onValidateCallback will be set to null.
        /// </remarks>
        public void SetDefault()
        {
            ikMode = AvatarIKMode.FullBody;
            autoStopIK = true;
            worldSpaceDrive = true;
            XRRoot = null;
            head.SetDefault();
            hands.SetDefault();
            feet.SetDefault();
            heightAutoFit.SetDefault();
            _onValidateCallback = null;

            isDirty = true;
        }

        /// <summary>
        /// Copy the AvatarIKSettingsSample object
        /// </summary>
        /// <param name="from">The input AvatarIKSettingsSample object</param>
        /// <param name="to">The ouput AvatarIKSettingsSample object</param>
        public static void Copy(AvatarIKSettingsSample from, AvatarIKSettingsSample to)
        {
            if (from == null || to == null) return;

            to.ikMode = from.ikMode;
            to.autoStopIK = from.autoStopIK;
            to.worldSpaceDrive = from.worldSpaceDrive;
            to.XRRoot = from.XRRoot;
            to.head = from.head;
            to.hands = from.hands;
            to.feet = from.feet;
            to.heightAutoFit = from.heightAutoFit;
            to._onValidateCallback = from._onValidateCallback;
            to.isDirty = true;
        }

        /// <summary>
        /// Update the internal IK target settings from this 
        /// </summary>
        /// <param name="ikTarget">Internal IK target settings</param>
        public void updateIKTargetsConfig(AvatarIKTargetsConfig ikTarget)
        {
            if (ikTarget == null) return;

            ikTarget.worldSpaceDrive = worldSpaceDrive;

            if (ikTarget.xrRoot != XRRoot)
            {
                ikTarget.xrRoot = XRRoot;
            }
            if (ikTarget.headTarget != head.target.transform)
            {
                ikTarget.headTarget = head.target.transform;
            }
            if (ikTarget.leftHandTarget != hands.leftTarget.transform)
            {
                ikTarget.leftHandTarget = hands.leftTarget.transform;
            }
            if (ikTarget.rightHandTarget != hands.rightTarget.transform)
            {
                ikTarget.rightHandTarget = hands.rightTarget.transform;
            }
            if (ikTarget.leftFootTarget != feet.leftTarget.transform)
            {
                ikTarget.leftFootTarget = feet.leftTarget.transform;
            }
            if (ikTarget.rightFootTarget != feet.rightTarget.transform)
            {
                ikTarget.rightFootTarget = feet.rightTarget.transform;
            }

            if (ikTarget.eyePositionOffset != head.target.positionOffset)
            {
                ikTarget.eyePositionOffset = head.target.positionOffset;
            }
            if (ikTarget.eyeRotationOffset != Quaternion.Euler(head.target.rotationOffset))
            {
                ikTarget.eyeRotationOffset = Quaternion.Euler(head.target.rotationOffset);
            }

            if (ikTarget.leftHandPositionOffset != hands.leftTarget.positionOffset)
            {
                ikTarget.leftHandPositionOffset = hands.leftTarget.positionOffset;
            }
            if (ikTarget.leftHandRotationOffset != Quaternion.Euler(hands.leftTarget.rotationOffset))
            {
                ikTarget.leftHandRotationOffset = Quaternion.Euler(hands.leftTarget.rotationOffset);
            }

            if (ikTarget.rightHandPositionOffset != hands.rightTarget.positionOffset)
            {
                ikTarget.rightHandPositionOffset = hands.rightTarget.positionOffset;
            }
            if (ikTarget.rightHandRotationOffset != Quaternion.Euler(hands.rightTarget.rotationOffset))
            {
                ikTarget.rightHandRotationOffset = Quaternion.Euler(hands.rightTarget.rotationOffset);
            }

            if (ikTarget.leftFootPositionOffset != feet.leftTarget.positionOffset)
            {
                ikTarget.leftFootPositionOffset = feet.leftTarget.positionOffset;
            }
            if (ikTarget.leftFootRotationOffset != Quaternion.Euler(feet.leftTarget.rotationOffset))
            {
                ikTarget.leftFootRotationOffset = Quaternion.Euler(feet.leftTarget.rotationOffset);
            }

            if (ikTarget.rightFootPositionOffset != feet.rightTarget.positionOffset)
            {
                ikTarget.rightFootPositionOffset = feet.rightTarget.positionOffset;
            }
            if (ikTarget.rightFootRotationOffset != Quaternion.Euler(feet.rightTarget.rotationOffset))
            {
                ikTarget.rightFootRotationOffset = Quaternion.Euler(feet.rightTarget.rotationOffset);
            }
        }

        /// <summary>
        /// Update the biped IK and height auto fit controllers from this 
        /// </summary>
        /// <param name="avatarEntity">The owner Avatar entity</param>
        public void UpdateAvatarIKSettings(AvatarEntity avatarEntity)
        {
            updateIKTargetsConfig(avatarEntity?.avatarIKTargetsConfig);

            // update biped IK controller
            var bipedIKController = avatarEntity?.bodyAnimController?.bipedIKController;
            if (bipedIKController != null)
            {
                // update head settings
                bipedIKController.SetSpineBendEnable(head.enableSpineBend);
                bipedIKController.SetSpineRotationWeight(head.spineRotationWeight);
                bipedIKController.SetRotationLimitEnable(JointType.Head, head.rotationLimits.head);
                bipedIKController.SetRotationLimitEnable(JointType.Chest, head.rotationLimits.chest);
                bipedIKController.SetRotationLimitEnable(JointType.Hips, head.rotationLimits.hips);

                // update hand settings
                bipedIKController.SetStretchEnable(IKEffectorType.LeftHand, hands.stretch);
                bipedIKController.SetStretchEnable(IKEffectorType.RightHand, hands.stretch);
                bipedIKController.SetRotationLimitEnable(JointType.LeftShoulder, hands.rotationLimits.shoulder);
                bipedIKController.SetRotationLimitEnable(JointType.LeftArmUpper, hands.rotationLimits.upperArm);
                bipedIKController.SetRotationLimitEnable(JointType.LeftArmLower, hands.rotationLimits.lowerArm);
                bipedIKController.SetRotationLimitEnable(JointType.LeftHandWrist, hands.rotationLimits.handWrist);
                bipedIKController.SetRotationLimitEnable(JointType.RightShoulder, hands.rotationLimits.shoulder);
                bipedIKController.SetRotationLimitEnable(JointType.RightArmUpper, hands.rotationLimits.upperArm);
                bipedIKController.SetRotationLimitEnable(JointType.RightArmLower, hands.rotationLimits.lowerArm);
                bipedIKController.SetRotationLimitEnable(JointType.RightHandWrist, hands.rotationLimits.handWrist);

                // update feet settings
                bipedIKController.SetProceduralFootstepEnable(feet.autoFootstep);
                bipedIKController.SetFootstepHeight(feet.footstep.stepHeight);
                bipedIKController.SetFootstepSpeed(feet.footstep.stepSpeed);
                bipedIKController.SetFootCollisionRadius(feet.footstep.footCollisionRadius);
                bipedIKController.SetFootstepPositionThreshold(feet.footstep.positionThreshold);
                bipedIKController.SetFootstepRotationThreshold(feet.footstep.rotationThreshold);
                bipedIKController.SetRotationLimitEnable(JointType.LeftLegUpper, feet.rotationLimits.upperLeg);
                bipedIKController.SetRotationLimitEnable(JointType.LeftLegLower, feet.rotationLimits.lowerLeg);
                bipedIKController.SetRotationLimitEnable(JointType.LeftFootAnkle, feet.rotationLimits.ankle);
                bipedIKController.SetRotationLimitEnable(JointType.RightLegUpper, feet.rotationLimits.upperLeg);
                bipedIKController.SetRotationLimitEnable(JointType.RightLegLower, feet.rotationLimits.lowerLeg);
                bipedIKController.SetRotationLimitEnable(JointType.RightFootAnkle, feet.rotationLimits.ankle);
            }

            // update height auto fit
            var autoFitController = avatarEntity?.bodyAnimController?.autoFitController;
            if (autoFitController != null)
            {
                autoFitController.localAvatarHeightFittingEnable = heightAutoFit.enableAutoFitHeight;
                autoFitController.presetStanding = heightAutoFit.standingMode.thresholds.toAvatarAutoFitParam();
                autoFitController.presetSitting = heightAutoFit.sittingMode.thresholds.toAvatarAutoFitParam();
                autoFitController.ApplyPreset(autoFitController.presetStanding);
                
                avatarEntity.bodyAnimController.autoStandUp = heightAutoFit.sittingMode.autoStandUp;
                if (bipedIKController != null)
                {
                    bipedIKController.SetAutoStandUpDistanceThreshold(heightAutoFit.sittingMode.autoStandUpDistance);
                    bipedIKController.SetAutoStandUpAngularThreshold(heightAutoFit.sittingMode.autoStandUpAngle);
                    bipedIKController.SetAutoStandUpTimeThreshold(heightAutoFit.sittingMode.autoStandUpTime);
                }
            }

            if (avatarEntity != null && bipedIKController != null && autoFitController != null)
            {
                // Ensure that this IK settings have been set into avatarEntity, bipedIKController, and autoFitController.
                isDirty = false;
            }
        }

        /// <summary>
        /// Set the callback for 'OnValidate' message method
        /// </summary>
        /// <param name="callback">The callback action</param>
        public void SetOnValidateCallback(System.Action callback)
        {
            _onValidateCallback = callback;
        }

        /// <summary>
        /// Whether this instance of 'AvatarIKSettingsSample' has been modified
        /// </summary>
        internal bool isDirty { set; get; } = true;

        private void OnValidate()
        {
            isDirty = true;

            _onValidateCallback?.Invoke();
        }
    }
}

