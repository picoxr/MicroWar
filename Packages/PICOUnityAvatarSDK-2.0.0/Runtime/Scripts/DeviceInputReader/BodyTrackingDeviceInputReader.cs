#if NO_XR

using UnityEngine;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace Pico
{
    namespace Avatar
    {
        public class BodyTrackingDeviceInputReader : PXRDeviceInputReader
        {
            #region BodyTracking Data Definition
            /// <summary>
            /// Body tracking bone type.
            /// </summary>
            private enum BodyTrackerRole
            {
                Pelvis = 0,
                LEFT_HIP = 1,
                RIGHT_HIP = 2,
                SPINE1 = 3,
                LEFT_KNEE = 4,
                RIGHT_KNEE = 5,
                SPINE2 = 6,
                LEFT_ANKLE = 7,
                RIGHT_ANKLE = 8,
                SPINE3 = 9,
                LEFT_FOOT = 10,
                RIGHT_FOOT = 11,
                NECK = 12,
                LEFT_COLLAR = 13,
                RIGHT_COLLAR = 14,
                HEAD = 15,
                LEFT_SHOULDER = 16,
                RIGHT_SHOULDER = 17,
                LEFT_ELBOW = 18,
                RIGHT_ELBOW = 19,
                LEFT_WRIST = 20,
                RIGHT_WRIST = 21,
                LEFT_HAND = 22,
                RIGHT_HAND = 23,
                NONE_ROLE = 24,     // unvalid
                MIN_ROLE = 0,       // min value
                MAX_ROLE = 23,      // max value
                ROLE_NUM = 24,
            }

            // <summary>
            /// The struct that defines the lengths (in centimeters) of different body parts of the avatar.
            /// </summary>
            private struct BodyTrackingBoneLength
            {
                /// <summary>
                /// The length of the head, which is from the top of the head to the upper area of the neck.
                /// </summary>
                public float headLen;
                /// <summary>
                /// The length of the neck, which is from the upper area of the neck to the lower area of the neck.
                /// </summary>
                public float neckLen;
                /// <summary>
                /// The length of the torso, which is from the lower area of the neck to the navel.
                /// </summary>
                public float torsoLen;
                /// <summary>
                /// The length of the hip, which is from the navel to the center of the upper area of the upper leg.
                /// </summary>
                public float hipLen;
                /// <summary>
                /// The length of the upper leg, which from the hip to the knee-joint.
                /// </summary>
                public float upperLegLen;
                /// <summary>
                /// The length of the lower leg, which is from the knee-joint to the ankle.
                /// </summary>
                public float lowerLegLen;
                /// <summary>
                /// The length of the foot, which is from the ankle to the tiptoe.
                /// </summary>
                public float footLen;
                /// <summary>
                /// The length of the shoulder, which is between the left and right shoulder joints.
                /// </summary>
                public float shoulderLen;
                /// <summary>
                /// The length of the upper arm, which is from the sholder joint to the elbow joint.
                /// </summary>
                public float upperArmLen;
                /// <summary>
                /// The length of the lower arm, which is from the elbow joint to the wrist.
                /// </summary>
                public float lowerArmLen;
                /// <summary>
                /// The length of the hand, which is from the wrist to the finger tip.
                /// </summary>
                public float handLen;
            }

            private struct BodyTrackerTransPose
            {
                public long TimeStamp;  // time stamp of imu
                public double PosX;     // position of x
                public double PosY;     // position of y
                public double PosZ;     // position of z
                public double RotQx;    // x components of Quaternion
                public double RotQy;    // y components of Quaternion
                public double RotQz;    // z components of Quaternion
                public double RotQw;    // w components of Quaternion
            }

            private struct BodyTrackerTransform
            {
                public BodyTrackerRole bone; // bone name. if bone == NONE_ROLE, this bone is not calculated

                // [MarshalAs(UnmanagedType.Struct)]
                public BodyTrackerTransPose localpose;

                // [MarshalAs(UnmanagedType.Struct)]
                public BodyTrackerTransPose globalpose;

                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
                public double[] velo; // velocity of x,y,z

                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
                public double[] acce; // acceleration of x,y,z

                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
                public double[] wvelo; // angular velocity of x,y,z

                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
                public double[] wacce; // angular acceleration of x,y,z

                public uint Action; // multiple actions can be supported at the same time by means of OR

                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
                public byte[] reserved;
            }

            private struct BodyTrackerResult
            {
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 24)]
                public BodyTrackerTransform[] trackingdata;
            }
            
            /// <summary>
            /// Body tracking device input data.
            /// </summary>
            [StructLayout(LayoutKind.Sequential, Pack = 4)]
            private struct BodyTrackingData
            {
                [MarshalAs(UnmanagedType.I1)]
                public byte version;
                [MarshalAs(UnmanagedType.I1)]
                public byte reserve1;
                [MarshalAs(UnmanagedType.I1)]
                public byte reserve2;
                [MarshalAs(UnmanagedType.I1)]
                public byte reserve3;

                /// Calibrate state of swift
                public int bodyTrackingCalibrateState;

                /// Vector3[]
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = BodyTrackerRoleCount)]
                public Vector3[] bodyTrackingWorldPositions;

                /// Quaternion[]
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = BodyTrackerRoleCount)]
                public Quaternion[] bodyTrackingWorldOrientations;
            };
            #endregion

            #region Public Fields
            public const int BodyTrackerRoleCount = (int)BodyTrackerRole.ROLE_NUM;
            
            /// <summary>
            /// Whether the PICO Motion Tracker has completed calibration.
            /// </summary>
            /// <returns>
            ///   true - calibration completed;
            ///   false - calibration uncompleted, need to launch the "PICO Motion Tracker" app for calibration.
            /// </returns>
            public bool IsCalibrated
            {
                get
                {
                    int calibrated = -1;
                    PT_GetSwiftCalibState(ref calibrated);

                    if (_bodyTrackingData.bodyTrackingCalibrateState != 1 && calibrated == 1)
                    {
                        // After calibration, use body tracking driving.
                        _owner?.bodyAnimController?.SetUseBodyTracking(true);

                        _isFirstFrame = true;
                    }
                    else if (_bodyTrackingData.bodyTrackingCalibrateState == 1 && calibrated != 1)
                    {
                        // After disconnection, use IK driving.
                        _owner?.bodyAnimController?.SetUseBodyTracking(false);
                    }

                    _bodyTrackingData.bodyTrackingCalibrateState = calibrated;
                    return calibrated == 1;
                }
            }
            #endregion

            #region Private Fields
            private BodyTrackerResult _bodyTrackerResult;
            private BodyTrackingData _bodyTrackingData;

            private float _toAdjustHeight = 0f;    // use for adjusting the feet of Avatar to fit to the ground
            private float _tPosePelvisHeight = 0f; // record the pelvis height in T-pose.
            private bool _isFirstFrame = true;
            #endregion

            #region Public Methods
            /// <summary>
            /// Set the tracking mode. If not set, the default tracking mode is leg tracking.
            /// </summary>
            /// <param name="swiftMode">
            ///   0 - leg tracking only, lower latency but lower tracking accuracy compared to full-body tracking;
            ///   1 - full-body tracking, higher latency but higher tracking accuracy compared to leg tracking.
            /// </param>
            public void SetSwiftMode(int swiftMode)
            {
                PT_SetBodyTrackingMode(swiftMode);
            }

            /// <summary>
            /// After the PICO Motion Tracker has completed calibration,
            /// adjust the feet of Avatar to fit to the ground(requires the user to stand upright on the ground, not squatting, not jumping).
            /// </summary>
            public override void FitGround()
            {
                base.FitGround();
                _toAdjustHeight = (float)_bodyTrackerResult.trackingdata[(int)BodyTrackerRole.Pelvis].localpose.PosY - _tPosePelvisHeight;
            }
            #endregion

            #region Internal Methods
            internal override void Initialize(System.IntPtr nativeHandler_, AvatarEntity owner)
            {
                if (owner == null || owner.bodyAnimController == null) return;

                base.Initialize(nativeHandler_, owner);

                _bodyTrackerResult = new BodyTrackerResult();
                _bodyTrackerResult.trackingdata = new BodyTrackerTransform[BodyTrackerRoleCount];

                _bodyTrackingData.version = 0;
                _bodyTrackingData.bodyTrackingCalibrateState = -1;
                _bodyTrackingData.bodyTrackingWorldPositions = new Vector3[BodyTrackerRoleCount];
                _bodyTrackingData.bodyTrackingWorldOrientations = new Quaternion[BodyTrackerRoleCount];

                _tPosePelvisHeight = owner.bodyAnimController.GetJointWorldXForm(JointType.Hips).position.y;

                SetSwiftMode(1);
                UpdateBonesLength(owner.bodyAnimController);
            }

            internal override void UpdateButtonStatus()
            {
                base.UpdateButtonStatus();
            }

            internal override void UpdateConnectionStatus()
            {
                base.UpdateConnectionStatus();
            }

            internal override void UpdateDevicePose()
            {
                if (_owner == null || _owner.bodyAnimController == null) return;

                base.UpdateDevicePose();

                if (!IsCalibrated) return;

                if (!_owner.bodyAnimController.isUsingBodyTracking) return;
                
                // Update body tracking data
                PT_GetBodyTrackingData(ref _bodyTrackerResult);

                ProcessTrackingData();

                if (isAvatarHubMode)
                {
                    DisableMoving();
                    DisableJumping();
                }

                for (int i = 0; i < _bodyTrackerResult.trackingdata.Length; ++i)
                {
                    _bodyTrackingData.bodyTrackingWorldPositions[i].Set(
                        (float)_bodyTrackerResult.trackingdata[i].localpose.PosX,
                        (float)_bodyTrackerResult.trackingdata[i].localpose.PosY,
                        (float)_bodyTrackerResult.trackingdata[i].localpose.PosZ);

                    _bodyTrackingData.bodyTrackingWorldOrientations[i].Set(
                        (float)_bodyTrackerResult.trackingdata[i].localpose.RotQx,
                        (float)_bodyTrackerResult.trackingdata[i].localpose.RotQy,
                        (float)_bodyTrackerResult.trackingdata[i].localpose.RotQz,
                        (float)_bodyTrackerResult.trackingdata[i].localpose.RotQw);
                }

                pav_AvatarBodyTrackingDeviceInputReader_SetDeviceInputData(nativeHandle, ref _bodyTrackingData);
            }
            #endregion

            #region Private Methods
            private void ProcessTrackingData()
            {
                // Record the '_toAdjustHeight' value.
                if (_isFirstFrame)
                {
                    FitGround();
                    _isFirstFrame = false;
                }

                for (int i = 0; i < _bodyTrackerResult.trackingdata.Length; ++i)
                {
                    _bodyTrackerResult.trackingdata[i].localpose.PosX *= -1;

                    _bodyTrackerResult.trackingdata[i].localpose.RotQz *= -1;
                    _bodyTrackerResult.trackingdata[i].localpose.RotQw *= -1;

                    // Adjust the height of both feet to fit the ground.
                    _bodyTrackerResult.trackingdata[i].localpose.PosY -= _toAdjustHeight;
                }
            }

            // Fix the XZ coordinate of Avatar.
            private void DisableMoving()
            {
                // Set the xz of root joint to 0.
                _bodyTrackerResult.trackingdata[(int)BodyTrackerRole.Pelvis].localpose.PosX = 0;
                _bodyTrackerResult.trackingdata[(int)BodyTrackerRole.Pelvis].localpose.PosZ = 0;
            }

            // Fix the Avatar's feet on the ground.
            private void DisableJumping()
            {
                var pelvisHeight = _bodyTrackerResult.trackingdata[(int)BodyTrackerRole.Pelvis].localpose.PosY;
                if (pelvisHeight > _tPosePelvisHeight)
                {
                    var deltaHeight = pelvisHeight - _tPosePelvisHeight;

                    for (int i = 0; i < _bodyTrackerResult.trackingdata.Length; ++i)
                    {
                        _bodyTrackerResult.trackingdata[i].localpose.PosY -= deltaHeight;
                    }
                }
            }

            // Note: This update needs to be called after switching the avatar
            private void UpdateBonesLength(AvatarBodyAnimController bodyAnimController)
            {
                // The unit of xxxPos is meter
                // spine 
                Vector3 hipPos = bodyAnimController.GetJointWorldXForm(JointType.Hips).position;

                Vector3 spineLowerPos = bodyAnimController.GetJointWorldXForm(JointType.SpineLower).position;
                if (spineLowerPos == Vector3.zero) spineLowerPos = hipPos;  // optional, maybe not have(equal zero)

                Vector3 spineUpperPos = bodyAnimController.GetJointWorldXForm(JointType.SpineUpper).position;
                if (spineUpperPos == Vector3.zero) spineUpperPos = spineLowerPos;  // optional, maybe not have(equal zero)

                Vector3 chestPos = bodyAnimController.GetJointWorldXForm(JointType.Chest).position;

                Vector3 neckPos = bodyAnimController.GetJointWorldXForm(JointType.Neck).position;

                Vector3 headPos = bodyAnimController.GetJointWorldXForm(JointType.Head).position;

                // lower limbs
                Vector3 leftLegUpperPos = bodyAnimController.GetJointWorldXForm(JointType.LeftLegUpper).position;

                Vector3 leftLegLowerPos = bodyAnimController.GetJointWorldXForm(JointType.LeftLegLower).position;

                Vector3 leftFootAnklePos = bodyAnimController.GetJointWorldXForm(JointType.LeftFootAnkle).position;

                Vector3 leftToePos = bodyAnimController.GetJointWorldXForm(JointType.LeftToe).position;
                if (leftToePos == Vector3.zero) leftToePos = leftFootAnklePos;  // optional, maybe not have(equal zero)

                Vector3 leftToeEndPos = bodyAnimController.GetJointWorldXForm(JointType.LeftToeEnd).position;
                if (leftToeEndPos == Vector3.zero) leftToeEndPos = leftToePos;  // optional, maybe not have(equal zero)

                // upper limbs
                Vector3 rightArmUpperPos = bodyAnimController.GetJointWorldXForm(JointType.RightArmUpper).position;

                Vector3 leftArmUpperPos = bodyAnimController.GetJointWorldXForm(JointType.LeftArmUpper).position;

                Vector3 leftArmLowerPos = bodyAnimController.GetJointWorldXForm(JointType.LeftArmLower).position;

                Vector3 leftHandWristPos = bodyAnimController.GetJointWorldXForm(JointType.LeftHandWrist).position;

                Vector3 leftHandMiddleRootPos = bodyAnimController.GetJointWorldXForm(JointType.LeftHandMiddleMetacarpal).position;
                if (leftHandMiddleRootPos == Vector3.zero) leftHandMiddleRootPos = leftHandWristPos;  // optional, maybe not have(equal zero)

                Vector3 leftHandMiddleMetaPos = bodyAnimController.GetJointWorldXForm(JointType.LeftHandMiddleProximal).position;
                if (leftHandMiddleMetaPos == Vector3.zero) leftHandMiddleMetaPos = leftHandMiddleRootPos;  // optional, maybe not have(equal zero)

                Vector3 leftHandMiddleProximalPos = bodyAnimController.GetJointWorldXForm(JointType.LeftHandMiddleIntermediate).position;
                if (leftHandMiddleProximalPos == Vector3.zero) leftHandMiddleProximalPos = leftHandMiddleMetaPos;  // optional, maybe not have(equal zero)

                Vector3 leftHandMiddleIntermediatePos = bodyAnimController.GetJointWorldXForm(JointType.LeftHandMiddleDistal).position;
                if (leftHandMiddleIntermediatePos == Vector3.zero) leftHandMiddleIntermediatePos = leftHandMiddleProximalPos;  // optional, maybe not have(equal zero)

                Vector3 leftHandMiddleDistalPos = bodyAnimController.GetJointWorldXForm(JointType.LeftHandMiddleTip).position;
                if (leftHandMiddleDistalPos == Vector3.zero) leftHandMiddleDistalPos = leftHandMiddleIntermediatePos;  // optional, maybe not have(equal zero)


                // The unit of BodyTrackingBoneLength is centimeter
                BodyTrackingBoneLength bonesLength;

                // if "GetTotalLength()" of the specified bone is far less than its default length,
                // e.g. Mathf.Abs(bonesLength.footLen - kDefaultFootLen) > kBoneLenEpsilon,
                // that means missing essential bones to calculate the correct length,
                // then we should use the defualt length.
                const float kBoneLenEpsilon = 0.05f * 100f;

                // 头长(头顶到脖子上端的长度)
                const float kDefaultHeadLen = 0.2f * 100f;
                bonesLength.headLen = kDefaultHeadLen;

                //脖子长(脖子上端到脖子下端的长度)
                bonesLength.neckLen = GetTotalLength(new List<Vector3> { headPos, neckPos }) * 100f;

                //躯干长度（脖子下端到腰部肚脐的长度）
                bonesLength.torsoLen = GetTotalLength(new List<Vector3> { neckPos, chestPos, spineUpperPos, spineLowerPos }) * 100f;

                //腰部长度 (腰部肚脐到大腿上端中心的长度)
                const float kDefaultHipLen = 0.075f * 100f;
                bonesLength.hipLen = GetTotalLength(new List<Vector3> { spineLowerPos, hipPos }) * 100f;
                if (Mathf.Abs(bonesLength.hipLen - kDefaultHipLen) > kBoneLenEpsilon) bonesLength.hipLen = kDefaultHipLen;  // not have SpineLower, use default value

                //大腿长（臀部到膝关节长度）
                bonesLength.upperLegLen = GetTotalLength(new List<Vector3> { leftLegUpperPos, leftLegLowerPos }) * 100f;

                //小腿长（膝关节到脚踝长度）
                bonesLength.lowerLegLen = GetTotalLength(new List<Vector3> { leftLegLowerPos, leftFootAnklePos }) * 100f;

                //脚长（踝关节到脚尖的长度）
                const float kDefaultFootLen = 0.135f * 100f;
                bonesLength.footLen = GetTotalLength(new List<Vector3> { leftFootAnklePos, leftToePos, leftToeEndPos }) * 100f;
                if (Mathf.Abs(bonesLength.footLen - kDefaultFootLen) > kBoneLenEpsilon) bonesLength.footLen = kDefaultFootLen;  // not have leftToe/LeftToeEnd, use default value

                //肩宽（左右两个肩关节之间的长度）
                bonesLength.shoulderLen = GetTotalLength(new List<Vector3> { leftArmUpperPos, rightArmUpperPos }) * 100f;

                //大臂长（肩关节到肘关节长度）
                bonesLength.upperArmLen = GetTotalLength(new List<Vector3> { leftArmUpperPos, leftArmLowerPos }) * 100f;

                //小臂长（肘关节到手腕长度）
                bonesLength.lowerArmLen = GetTotalLength(new List<Vector3> { leftArmLowerPos, leftHandWristPos }) * 100f;

                //手长（腕关节到指尖的长度）
                const float kDefaultHandLen = 0.17f * 100f;
                bonesLength.handLen = GetTotalLength(new List<Vector3> { leftHandWristPos, leftHandMiddleRootPos,
                    leftHandMiddleMetaPos, leftHandMiddleProximalPos, leftHandMiddleIntermediatePos, leftHandMiddleDistalPos }) * 100f;
                if (Mathf.Abs(bonesLength.handLen - kDefaultHandLen) > kBoneLenEpsilon) bonesLength.handLen = kDefaultHandLen;  // not have leftHandMiddleXxx, use default value

                PT_SetSwiftBoneLength(ref bonesLength);
            }

            private static float GetTotalLength(List<Vector3> points)
            {
                float totalLength = 0f;
                // Count = 1 or 0
                if (points.Count < 2)
                {
                    return totalLength;
                }

                for (int i = 0; i < points.Count - 1; ++i)
                {
                    totalLength += Vector3.Distance(points[i], points[i + 1]);
                }
                return totalLength;
            }

            #endregion

            #region Native Methods
            [DllImport(libTrackingName, CallingConvention = CallingConvention.Cdecl)]
            private static extern int PT_GetSwiftCalibState(ref int calibrated);

            [DllImport(libTrackingName, CallingConvention = CallingConvention.Cdecl)]
            private static extern int PT_GetBodyTrackingData(ref BodyTrackerResult bodyTrackerResult);

            [DllImport(libTrackingName, CallingConvention = CallingConvention.Cdecl)]
            private static extern int PT_SetBodyTrackingMode(int mode);

            [DllImport(libTrackingName, CallingConvention = CallingConvention.Cdecl)]
            private static extern int PT_SetSwiftBoneLength(ref BodyTrackingBoneLength boneLength);


            const string PavDLLName = DllLoaderHelper.PavDLLName;

            [DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
            private static extern NativeResult pav_AvatarBodyTrackingDeviceInputReader_SetDeviceInputData(System.IntPtr nativeHandle, ref BodyTrackingData data);

            [DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
            private static extern NativeResult pav_AvatarBodyTrackingDeviceInputReader_GetDeviceInputData(System.IntPtr nativeHandle, ref BodyTrackingData data);
            #endregion
        }
    }
}

#else

using UnityEngine;
using Unity.XR.PXR;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Pico
{
	namespace Avatar
	{
		/// <summary>
		/// BodyTrackingDeviceInputReader
		/// </summary>
		public class BodyTrackingDeviceInputReader : PXRDeviceInputReader
		{
			[StructLayout(LayoutKind.Sequential, Pack = 4)]
			struct BodyTrackingData
			{
				[MarshalAs(UnmanagedType.I1)] public byte version;
				[MarshalAs(UnmanagedType.I1)] public byte reserve1;
				[MarshalAs(UnmanagedType.I1)] public byte reserve2;
				[MarshalAs(UnmanagedType.I1)] public byte reserve3;

				/// Calibrate state of swift
				public int bodyTrackingCalibrateState;

				/// Vector3[]
				[MarshalAs(UnmanagedType.ByValArray, SizeConst = BodyTrackerRoleCount)]
				public Vector3[] bodyTrackingWorldPositions;

				/// Quaternion[]
				[MarshalAs(UnmanagedType.ByValArray, SizeConst = BodyTrackerRoleCount)]
				public Quaternion[] bodyTrackingWorldOrientations;
			};

			#region Public Fields

			public const int BodyTrackerRoleCount = (int)BodyTrackerRole.ROLE_NUM;

            /// <summary>
            /// Whether the PICO Motion Tracker has completed calibration.
            /// </summary>
            /// <returns>
            ///   true - calibration completed;
            ///   false - calibration uncompleted, need to launch the "PICO Motion Tracker" app for calibration.
            /// </returns>
            public bool IsCalibrated
			{
				get
				{
					if (_bodyTrackingData.bodyTrackingCalibrateState == 1) return true;

					PXR_Input.GetMotionTrackerCalibState(ref _bodyTrackingData.bodyTrackingCalibrateState);
					return _bodyTrackingData.bodyTrackingCalibrateState == 1;
				}
			}

			#endregion

			#region Private Fields

			private BodyTrackerResult _bodyTrackerResult;
			private BodyTrackingData _bodyTrackingData;
			private float _toAdjustHeight = 0f;
			private float _tPosePelvisHeight = 0f;

			#endregion

			#region Public Methods

            /// <summary>
            /// Set the tracking mode. If not set, the default tracking mode is leg tracking.
            /// </summary>
            /// <param name="swiftMode">
            ///   0 - leg tracking only, lower latency but lower tracking accuracy compared to full-body tracking;
            ///   1 - full-body tracking, higher latency but higher tracking accuracy compared to leg tracking.
            /// </param>
            public void SetSwiftMode(int swiftMode)
			{
                //Yagiz: PXR_Input.SetSwiftMode No Longer Exists 
                //PXR_Input.SetSwiftMode(swiftMode);
                PXR_Input.SetBodyTrackingMode((BodyTrackingMode)swiftMode);
            }

            /// <summary>
            /// Launch the "PICO Motion Tracker" app.
            /// </summary>
			public void CalibrateSwiftTracker()
			{
				try
				{
                    PXR_MotionTracking.StartMotionTrackerCalibApp();
                }
				catch (System.Exception ex)
				{
					Debug.LogError($"[BodyTrackingInput] Open swift calibration app fail, error message: {ex.Message}");
				}
			}

            /// <summary>
            /// After the PICO Motion Tracker has completed calibration,
            /// adjust the feet of Avatar to fit to the ground(requires the user to stand upright on the ground, not squatting, not jumping).
            /// </summary>
            public void FitGround()
			{
				_toAdjustHeight = _bodyTrackingData.bodyTrackingWorldPositions[(int)BodyTrackerRole.Pelvis].y -
				                  _tPosePelvisHeight;
			}

			#endregion

			#region Internal Methods

			internal override void Initialize(System.IntPtr nativeHandler_, AvatarEntity owner)
			{
				if (owner == null || owner.bodyAnimController == null) return;

				base.Initialize(nativeHandler_, owner);

				_bodyTrackerResult = new BodyTrackerResult();
				_bodyTrackerResult.trackingdata = new BodyTrackerTransform[BodyTrackerRoleCount];

				_bodyTrackingData.version = 0;
				_bodyTrackingData.bodyTrackingCalibrateState = -1;
				_bodyTrackingData.bodyTrackingWorldPositions = new Vector3[BodyTrackerRoleCount];
				_bodyTrackingData.bodyTrackingWorldOrientations = new Quaternion[BodyTrackerRoleCount];

				UpdateBonesLength(owner.bodyAnimController);
				SetTPose();
			}


			internal override void UpdateDevicePose()
			{
				if (_owner == null || _owner.bodyAnimController == null) return;

				base.UpdateDevicePose();

				AvatarAutoFitController autoFit = _owner.bodyAnimController.autoFitController;

				// Not using body tracking, need to enable AutoFit.
				if (!_owner.bodyAnimController.isUsingBodyTracking)
				{
					if (autoFit != null && !autoFit.localAvatarHeightFittingEnable)
					{
						autoFit.localAvatarHeightFittingEnable = true;
					}

					return;
				}

				// Now is using body tracking, need to disable AutoFit.
				if (autoFit != null && autoFit.localAvatarHeightFittingEnable)
				{
					autoFit.localAvatarHeightFittingEnable = false;
				}

				if (IsCalibrated)
				{
					// Update body tracking data
					PXR_Input.GetBodyTrackingPose(0d, ref _bodyTrackerResult);

					for (int i = 0; i < _bodyTrackerResult.trackingdata.Length; ++i)
					{
						_bodyTrackingData.bodyTrackingWorldPositions[i].Set(
							(float)_bodyTrackerResult.trackingdata[i].localpose.PosX,
							(float)_bodyTrackerResult.trackingdata[i].localpose.PosY,
							(float)_bodyTrackerResult.trackingdata[i].localpose.PosZ);

						_bodyTrackingData.bodyTrackingWorldOrientations[i].Set(
							(float)_bodyTrackerResult.trackingdata[i].localpose.RotQx,
							(float)_bodyTrackerResult.trackingdata[i].localpose.RotQy,
							(float)_bodyTrackerResult.trackingdata[i].localpose.RotQz,
							(float)_bodyTrackerResult.trackingdata[i].localpose.RotQw);
					}

					// Adjust the height of both feet to fit the ground.
					_bodyTrackingData.bodyTrackingWorldPositions[(int)BodyTrackerRole.Pelvis].y -= _toAdjustHeight;
				}

				pav_AvatarBodyTrackingDeviceInputReader_SetDeviceInputData(nativeHandle, ref _bodyTrackingData);
			}

			internal override void UpdateConnectionStatus()
			{
				base.UpdateConnectionStatus();
			}

			#endregion

			#region Private Methods

			// Note: This update needs to be called after switching the avatar
			private void UpdateBonesLength(AvatarBodyAnimController bodyAnimController)
			{
				// The unit of xxxPos is meter
				// spine 
				Vector3 hipPos = bodyAnimController.GetJointWorldXForm(JointType.Hips).position;

				Vector3 spineLowerPos = bodyAnimController.GetJointWorldXForm(JointType.SpineLower).position;
				if (spineLowerPos == Vector3.zero) spineLowerPos = hipPos; // optional, maybe not have(equal zero)

				Vector3 spineUpperPos = bodyAnimController.GetJointWorldXForm(JointType.SpineUpper).position;
				if (spineUpperPos == Vector3.zero)
					spineUpperPos = spineLowerPos; // optional, maybe not have(equal zero)

				Vector3 chestPos = bodyAnimController.GetJointWorldXForm(JointType.Chest).position;

				Vector3 neckPos = bodyAnimController.GetJointWorldXForm(JointType.Neck).position;

				Vector3 headPos = bodyAnimController.GetJointWorldXForm(JointType.Head).position;

				// lower limbs
				Vector3 leftLegUpperPos = bodyAnimController.GetJointWorldXForm(JointType.LeftLegUpper).position;

				Vector3 leftLegLowerPos = bodyAnimController.GetJointWorldXForm(JointType.LeftLegLower).position;

				Vector3 leftFootAnklePos = bodyAnimController.GetJointWorldXForm(JointType.LeftFootAnkle).position;

				Vector3 leftToePos = bodyAnimController.GetJointWorldXForm(JointType.LeftToe).position;
				if (leftToePos == Vector3.zero) leftToePos = leftFootAnklePos; // optional, maybe not have(equal zero)

				Vector3 leftToeEndPos = bodyAnimController.GetJointWorldXForm(JointType.LeftToeEnd).position;
				if (leftToeEndPos == Vector3.zero) leftToeEndPos = leftToePos; // optional, maybe not have(equal zero)

				// upper limbs
				Vector3 rightArmUpperPos = bodyAnimController.GetJointWorldXForm(JointType.RightArmUpper).position;

				Vector3 leftArmUpperPos = bodyAnimController.GetJointWorldXForm(JointType.LeftArmUpper).position;

				Vector3 leftArmLowerPos = bodyAnimController.GetJointWorldXForm(JointType.LeftArmLower).position;

				Vector3 leftHandWristPos = bodyAnimController.GetJointWorldXForm(JointType.LeftHandWrist).position;

				Vector3 leftHandMiddleRootPos =
					bodyAnimController.GetJointWorldXForm(JointType.LeftHandMiddleMetacarpal).position;
				if (leftHandMiddleRootPos == Vector3.zero)
					leftHandMiddleRootPos = leftHandWristPos; // optional, maybe not have(equal zero)

				Vector3 leftHandMiddleMetaPos =
					bodyAnimController.GetJointWorldXForm(JointType.LeftHandMiddleProximal).position;
				if (leftHandMiddleMetaPos == Vector3.zero)
					leftHandMiddleMetaPos = leftHandMiddleRootPos; // optional, maybe not have(equal zero)

				Vector3 leftHandMiddleProximalPos =
					bodyAnimController.GetJointWorldXForm(JointType.LeftHandMiddleIntermediate).position;
				if (leftHandMiddleProximalPos == Vector3.zero)
					leftHandMiddleProximalPos = leftHandMiddleMetaPos; // optional, maybe not have(equal zero)

				Vector3 leftHandMiddleIntermediatePos =
					bodyAnimController.GetJointWorldXForm(JointType.LeftHandMiddleDistal).position;
				if (leftHandMiddleIntermediatePos == Vector3.zero)
					leftHandMiddleIntermediatePos = leftHandMiddleProximalPos; // optional, maybe not have(equal zero)

				Vector3 leftHandMiddleDistalPos =
					bodyAnimController.GetJointWorldXForm(JointType.LeftHandMiddleTip).position;
				if (leftHandMiddleDistalPos == Vector3.zero)
					leftHandMiddleDistalPos = leftHandMiddleIntermediatePos; // optional, maybe not have(equal zero)


				// The unit of BodyTrackingBoneLength is centimeter
				BodyTrackingBoneLength bonesLength;

				// if "GetTotalLength()" of the specified bone is far less than its default length,
				// e.g. Mathf.Abs(bonesLength.footLen - kDefaultFootLen) > kBoneLenEpsilon,
				// that means missing essential bones to calculate the correct length,
				// then we should use the defualt length.
				const float kBoneLenEpsilon = 0.05f * 100f;

				// 头长(头顶到脖子上端的长度)
				const float kDefaultHeadLen = 0.2f * 100f;
				bonesLength.headLen = kDefaultHeadLen;

				//脖子长(脖子上端到脖子下端的长度)
				bonesLength.neckLen = GetTotalLength(new List<Vector3> { headPos, neckPos }) * 100f;

				//躯干长度（脖子下端到腰部肚脐的长度）
				bonesLength.torsoLen = GetTotalLength(new List<Vector3>
					{ neckPos, chestPos, spineUpperPos, spineLowerPos }) * 100f;

				//腰部长度 (腰部肚脐到大腿上端中心的长度)
				const float kDefaultHipLen = 0.075f * 100f;
				bonesLength.hipLen = GetTotalLength(new List<Vector3> { spineLowerPos, hipPos }) * 100f;
				if (Mathf.Abs(bonesLength.hipLen - kDefaultHipLen) > kBoneLenEpsilon)
					bonesLength.hipLen = kDefaultHipLen; // not have SpineLower, use default value

				//大腿长（臀部到膝关节长度）
				bonesLength.upperLegLen = GetTotalLength(new List<Vector3> { leftLegUpperPos, leftLegLowerPos }) * 100f;

				//小腿长（膝关节到脚踝长度）
				bonesLength.lowerLegLen =
					GetTotalLength(new List<Vector3> { leftLegLowerPos, leftFootAnklePos }) * 100f;

				//脚长（踝关节到脚尖的长度）
				const float kDefaultFootLen = 0.135f * 100f;
				bonesLength.footLen =
					GetTotalLength(new List<Vector3> { leftFootAnklePos, leftToePos, leftToeEndPos }) * 100f;
				if (Mathf.Abs(bonesLength.footLen - kDefaultFootLen) > kBoneLenEpsilon)
					bonesLength.footLen = kDefaultFootLen; // not have leftToe/LeftToeEnd, use default value

				//肩宽（左右两个肩关节之间的长度）
				bonesLength.shoulderLen =
					GetTotalLength(new List<Vector3> { leftArmUpperPos, rightArmUpperPos }) * 100f;

				//大臂长（肩关节到肘关节长度）
				bonesLength.upperArmLen = GetTotalLength(new List<Vector3> { leftArmUpperPos, leftArmLowerPos }) * 100f;

				//小臂长（肘关节到手腕长度）
				bonesLength.lowerArmLen =
					GetTotalLength(new List<Vector3> { leftArmLowerPos, leftHandWristPos }) * 100f;

				//手长（腕关节到指尖的长度）
				const float kDefaultHandLen = 0.17f * 100f;
				bonesLength.handLen = GetTotalLength(new List<Vector3>
				{
					leftHandWristPos, leftHandMiddleRootPos,
					leftHandMiddleMetaPos, leftHandMiddleProximalPos, leftHandMiddleIntermediatePos,
					leftHandMiddleDistalPos
				}) * 100f;
				if (Mathf.Abs(bonesLength.handLen - kDefaultHandLen) > kBoneLenEpsilon)
					bonesLength.handLen = kDefaultHandLen; // not have leftHandMiddleXxx, use default value

				PXR_Input.SetBodyTrackingBoneLength(bonesLength);
			}

			private static float GetTotalLength(List<Vector3> points)
			{
				float totalLength = 0f;
				// Count = 1 or 0
				if (points.Count < 2)
				{
					return totalLength;
				}

				for (int i = 0; i < points.Count - 1; ++i)
				{
					totalLength += Vector3.Distance(points[i], points[i + 1]);
				}

				return totalLength;
			}

			private void SetTPose()
			{
				// Set the initial t-pose
				for (int i = 0; i < BodyTrackerRoleCount; ++i)
				{
					_bodyTrackingData.bodyTrackingWorldOrientations[i] = Quaternion.identity;
				}

				_bodyTrackingData.bodyTrackingWorldPositions[(uint)BodyTrackerRole.Pelvis].y =
					_owner.bodyAnimController.GetJointWorldXForm(JointType.Hips).position.y;

				_tPosePelvisHeight = _bodyTrackingData.bodyTrackingWorldPositions[(uint)BodyTrackerRole.Pelvis].y;
			}

			#endregion

			#region Native Methods

			const string PavDLLName = DllLoaderHelper.PavDLLName;

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarBodyTrackingDeviceInputReader_SetDeviceInputData(
				System.IntPtr nativeHandle, ref BodyTrackingData data);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarBodyTrackingDeviceInputReader_GetDeviceInputData(
				System.IntPtr nativeHandle, ref BodyTrackingData data);

			#endregion
		}
	}
}
#endif