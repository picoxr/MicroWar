using System.Collections.Generic;
using UnityEngine;

namespace Pico
{
	namespace Avatar
	{
		/// <summary>
		/// Avatar hand side
		/// </summary>
		public enum HandSide : int
		{
			/// <summary>
			/// Invalid side
			/// </summary>
			Invalid = -1,

			/// <summary>
			/// Left hand
			/// </summary>
			Left = 0,

			/// <summary>
			/// Right hand
			/// </summary>
			Right = 1,

			Count
		};

		/// <summary>
		/// Finger pose calculation mode
		/// </summary>
		public enum FingerPoseSyncMode : int
		{
			/// <summary>
			/// Finger joint orientation consistent with incoming gesture
			/// </summary>
			SyncRotation = 0,

			/// <summary>
			/// Finger joint position consistent with incoming gesture
			/// </summary>
			SyncPosition = 1,
		}

		/// <summary>
		/// AvatarCustomHandPose
		/// </summary>
		public class AvatarCustomHandPose : NativeObject
		{
			#region Types

			private struct JointTransform
			{
				public JointType type;
				public Transform transform;
				public int parentIndex;
			}

			#endregion


			#region Public Fields / Properties

			/// <summary>
			/// handPose
			/// </summary>
			public GameObject handPose = null;

			/// <summary>
			/// handSkeleton
			/// </summary>
			public GameObject handSkeleton = null;

			/// <summary>
			/// fingerPoseSyncMode
			/// </summary>
			public FingerPoseSyncMode fingerPoseSyncMode = FingerPoseSyncMode.SyncRotation;

			/// <summary>
			/// syncWristTransform
			/// </summary>
			public bool syncWristTransform
			{
				get { return _syncWristTransform; }
				set
				{
					_syncWristTransform = value;
					_ikTargetDirty = true;
				}
			}

			public Vector3 wristOffset = new Vector3(0.0f, 0.0f, 0.0f);

			#endregion


			#region Private Fields

			HandSide side = HandSide.Invalid;

			IKEffectorType ikEffectorType = IKEffectorType.LeftHand;

			Dictionary<string, HandJointType> SkeletonData = new Dictionary<string, HandJointType>();

			Dictionary<JointType, Vector3> myJointOffsets = new Dictionary<JointType, Vector3>();

			Dictionary<JointType, Vector3> recevieJointOffsets = new Dictionary<JointType, Vector3>();

			private List<JointTransform> _poseJointTransforms = new List<JointTransform>();

			private int _wristIndex;

			private Transform _avatarTransform;

			private XForm _wristXform = new XForm();

			private XForm _wristWorldXform = new XForm();

			private bool _syncWristTransform = true;

			private bool _ikTargetDirty = true;

			Quaternion rootRetargetQuat = new Quaternion();

			Vector3 ReceiveUp;
			Vector3 ReceiveForward;

			Vector3 MyRightUp = new Vector3(0.0f, 0.0f, 1.0f);
			Vector3 MyRightForward = new Vector3(1.0f, 0.0f, 0.0f);

			Vector3 MyLeftUp = new Vector3(0.0f, 0.0f, -1.0f);
			Vector3 MyLeftForward = new Vector3(-1.0f, 0.0f, 0.0f);

			bool hasAlignSkeleton = false;

			int middleJointCount = 5;

			bool scaleHandFlag = false;

			HandJointType[] MiddleJoints = new HandJointType[5]
			{
				HandJointType.MiddleMetacarpal, HandJointType.MiddleProximal,
				HandJointType.MiddleIntermediate, HandJointType.MiddleDistal, HandJointType.MiddleTip
			};

			float handScale = 1.0f;

			#endregion


			#region Public Methods

			/// <summary>
			/// Initialize CustomHandPose
			/// </summary>
			/// <param name="_side">HandSide</param>
			/// <param name="_handSkeleton">HandSkeleton</param>
			/// <param name="_handPose">HandPose</param>
			/// <param name="_up">Up</param>
			/// <param name="_forward">Forward</param>
			/// <param name="_offset">Offset</param>
			/// <param name="_avatarTrans">AvatarTrans</param>
			internal void Initialize(HandSide _side, GameObject _handSkeleton, GameObject _handPose, Vector3 _up,
				Vector3 _forward, Vector3 _offset, Transform _avatarTrans)
			{
				// Debug.Log("CustomHandPose:  handUp[" + _up.ToString("f5") + "]   handForward[" + _forward.ToString("f5") + "]");
				side = _side;
				handSkeleton = _handSkeleton;
				handPose = _handPose;
				ReceiveUp = _up;
				ReceiveForward = _forward;
				wristOffset = _offset;
				_avatarTransform = _avatarTrans;
				ikEffectorType = (_side == HandSide.Left) ? IKEffectorType.LeftHand : IKEffectorType.RightHand;
				SkeletonData = getCustomSkeletonData(handSkeleton);
				_poseJointTransforms = GetJointTransforms(handPose);
			}

			/// <summary>
			/// SetWristXForm
			/// </summary>
			/// <param name="bodyAnimController">body animation controller.</param>
			public void SetWristXForm(AvatarBodyAnimController bodyAnimController)
			{
				Matrix4x4 rootMat = _avatarTransform.worldToLocalMatrix;
				Quaternion rootQuat = _avatarTransform.rotation;
				if (bodyAnimController != null && bodyAnimController.bipedIKController != null && _ikTargetDirty &&
				    bodyAnimController.owner.isAnyLodReady)
				{
					//bodyAnimController.SetStretchEnable((uint)ikEffectorType, _syncWristTransform);
					bodyAnimController.bipedIKController.SetUpdateIKTargetFromDevice(ikEffectorType,
						!_syncWristTransform);
					_ikTargetDirty = false;
				}

				foreach (var jointTrans in _poseJointTransforms)
				{
					JointType nowType = jointTrans.type;
					if (nowType == JointType.RightHandWrist || nowType == JointType.LeftHandWrist)
					{
						if (_syncWristTransform)
						{
							//Vector3 wristOffsetPos = jointTrans.transform.position;
							Matrix4x4 wristMat = jointTrans.transform.localToWorldMatrix;

							Vector3 wristOffsetPos = wristMat.MultiplyPoint(wristOffset);
							//wristOffsetPos += Quaternion.Inverse(jointTrans.transform.localRotation) * wristOffset;

							_wristWorldXform.orientation = jointTrans.transform.rotation * rootRetargetQuat;
							_wristWorldXform.position = wristOffsetPos;
							_wristWorldXform.scale = new Vector3(handScale, handScale, handScale);
							//wristWorldXform.scale = new Vector3(1.0f, 1.0f, 1.0f);

							_wristXform.position = rootMat.MultiplyPoint(_wristWorldXform.position);
							_wristXform.orientation = Quaternion.Inverse(rootQuat) * _wristWorldXform.orientation;
							_wristXform.scale = new Vector3(1.0f, 1.0f, 1.0f);

							//Debug.Log("CustomHand Test " + nowType + "  wrist world position111:" + wristWorldXform.position.ToString("f6"));
							//Debug.Log("CustomHand Test " + nowType + "  wrist local position111:" + wristXform.position.ToString("f6"));
							bodyAnimController.bipedIKController.SetIKEffectorXForm(ikEffectorType, _wristXform);
						}

						//else
						//{
						//    if (nowType == JointType.LeftHandWrist)
						//    {
						//        if (deviceReader != null)
						//        {
						//            wristXform.position = deviceReader.deviceData.positions[(uint)IDeviceInputReader.DeviceType.LeftHand];
						//            wristXform.orientation = deviceReader.deviceData.orientations[(uint)IDeviceInputReader.DeviceType.LeftHand];
						//        }
						//    }
						//    else
						//    {
						//        if (deviceReader != null)
						//        {
						//            wristXform.position = deviceReader.deviceData.positions[(uint)IDeviceInputReader.DeviceType.RightHand];
						//            wristXform.orientation = deviceReader.deviceData.orientations[(uint)IDeviceInputReader.DeviceType.RightHand];
						//        }
						//    }
						//    wristXform.scale = new Vector3(1.0f, 1.0f, 1.0f);
						//    bodyAnimController.SetIKEffectorXForm((uint)ikEffectorType, wristXform);
						//}
						break;
					}
				}
			}

			/// <summary>
			/// SetHandPose
			/// </summary>
			/// <param name="bodyAnimController">body animation controller</param>
			public void SetHandPose(AvatarBodyAnimController bodyAnimController)
			{
				if (!hasAlignSkeleton)
				{
					AlignHandSkeleton(bodyAnimController);
					hasAlignSkeleton = true;
				}

				uint jointCount = 0;
				uint[] poseTypes = new uint[_poseJointTransforms.Count];
				Vector3[] worldOffsets = new Vector3[_poseJointTransforms.Count];

				foreach (var jointTrans in _poseJointTransforms)
				{
					JointType nowType = jointTrans.type;
					if (nowType == JointType.Invalid) continue;
					if (nowType == JointType.RightHandWrist || nowType == JointType.LeftHandWrist)
					{
						//Vector3 wristOffsetPos = jointTrans.transform.position;
						//wristOffsetPos += Quaternion.Inverse(jointTrans.transform.localRotation) * wristOffset;
						Matrix4x4 wristMat = jointTrans.transform.localToWorldMatrix;
						Vector3 wristOffsetPos = wristMat.MultiplyPoint(wristOffset);
						_wristWorldXform.orientation = jointTrans.transform.rotation * rootRetargetQuat;
						_wristWorldXform.position = wristOffsetPos;
						_wristWorldXform.scale = new Vector3(handScale, handScale, handScale);
						//wristWorldXform.scale = new Vector3(1.0f, 1.0f, 1.0f);

						// Debug.Log("CustomHand Test " + nowType + " world rotation:" + wristWorldXform.orientation.ToString("f6") + "  wrist rotation:" + wristXform.orientation.ToString("f6") + 
						//     "  wrist position:" + wristXform.position.ToString("f6"));
						//Debug.Log("CustomHand Test " + nowType + "  wrist world position:" + wristWorldXform.position.ToString("f6"));
					}
					else
					{
						int parentIndex = jointTrans.parentIndex;
						JointType parentType = _poseJointTransforms[parentIndex].type;
						if (parentType == JointType.RightHandWrist || parentType == JointType.LeftHandWrist) continue;
						// align local position
						if (fingerPoseSyncMode == FingerPoseSyncMode.SyncPosition)
						{
							worldOffsets[jointCount] = jointTrans.transform.position;
						}
						else
						{
							worldOffsets[jointCount] = jointTrans.transform.position -
							                           _poseJointTransforms[parentIndex].transform.position;
						}

						poseTypes[jointCount] = (uint)nowType;
						// Debug.Log("CustomHandPose check position  now " + nowType + jointTrans.transform.position.ToString("f5") + "  parent " + parentType + "  " + 
						//     poseJointTransforms[parentIndex].transform.position.ToString("f5") + "  worldlocal:" + worldOffsets[jointCount].ToString("f6"));
						jointCount++;
					}
				}

				bodyAnimController.UpdateCustomHandPose(jointCount, poseTypes, worldOffsets, _wristWorldXform,
					(uint)side, _syncWristTransform, (uint)fingerPoseSyncMode, scaleHandFlag);
			}

			/// <summary>
			/// StopCustomHand
			/// </summary>
			/// <param name="bodyAnimController">body animation controller</param>
			public void StopCustomHand(AvatarBodyAnimController bodyAnimController)
			{
				if (bodyAnimController != null && bodyAnimController.bipedIKController != null &&
				    bodyAnimController.owner.isAnyLodReady)
				{
					bodyAnimController.bipedIKController.SetUpdateIKTargetFromDevice(ikEffectorType, true);
				}
			}

			#endregion


			#region Private Methods

			private void AlignHandSkeleton(AvatarBodyAnimController bodyAnimController)
			{
				uint[] jointTypes = new uint[(int)HandJointType.Count];
				Vector3[] initPositions = new Vector3[(int)HandJointType.Count];
				for (int i = 0; i < (int)HandJointType.Count; ++i)
				{
					jointTypes[i] = (uint)JointTypeFromHandType((HandJointType)i, side);
				}

				bodyAnimController.GetSkeletonJointInitOffsets((uint)HandJointType.Count, jointTypes,
					ref initPositions);
				for (int i = 0; i < (int)HandJointType.Count; ++i)
				{
					myJointOffsets.Add((JointType)jointTypes[i], initPositions[i]);
				}

				if (side == HandSide.Left)
				{
					alignHandRoot(ReceiveForward, ReceiveUp, MyLeftForward, MyLeftUp);
				}
				else
				{
					alignHandRoot(ReceiveForward, ReceiveUp, MyRightForward, MyRightUp);
				}

				if (scaleHandFlag)
				{
					float myMiddleLength = 0.0f;
					float recevieMiddleLength = 0.0f;
					for (int i = 0; i < middleJointCount; ++i)
					{
						JointType nowJoint = JointTypeFromHandType(MiddleJoints[i], side);
						if (myJointOffsets.ContainsKey(nowJoint))
						{
							myMiddleLength += myJointOffsets[nowJoint].magnitude;
						}

						if (recevieJointOffsets.ContainsKey(nowJoint))
						{
							recevieMiddleLength += recevieJointOffsets[nowJoint].magnitude;
						}
					}

					handScale = recevieMiddleLength / myMiddleLength;
					if (handScale < 1.0f) handScale = 1.0f;
				}
			}

			private List<JointTransform> GetJointTransforms(GameObject gob)
			{
				_wristIndex = -1;
				var joints = new List<JointTransform>();
				var transformToIndex = new Dictionary<Transform, int>();
				bool rootFlag = true;
				foreach (var trans in gob.GetComponentsInChildren<Transform>())
				{
					int parentIndex = -1;
					if (trans.parent && !transformToIndex.TryGetValue(trans.parent, out parentIndex))
					{
						parentIndex = -1;
					}

					var type = HandJointType.Invalid;
					if (SkeletonData.ContainsKey(trans.name))
					{
						type = SkeletonData[trans.name];
						if (type == HandJointType.Wrist)
						{
							rootFlag = false;
						}
					}
					else if (rootFlag)
					{
						type = HandJointType.Wrist;
						rootFlag = false;
					}

					if (type == HandJointType.Wrist)
					{
						_wristIndex = joints.Count;
					}

					if (parentIndex >= 0 && joints[parentIndex].type == JointType.Invalid)
					{
						parentIndex = joints[parentIndex].parentIndex;
					}

					JointType nowType = JointTypeFromHandType(type, side);
					joints.Add(new JointTransform
					{
						type = nowType,
						transform = trans,
						parentIndex = parentIndex
					});
					if (!recevieJointOffsets.ContainsKey(nowType))
					{
						if (side == HandSide.Right) recevieJointOffsets.Add(nowType, trans.localPosition);
						else
						{
							// left is a mirror of right, mirror axis is X
							Vector3 leftOffset = trans.localPosition;
							leftOffset[0] = -leftOffset[0];
							recevieJointOffsets.Add(nowType, leftOffset);
						}
					}

					transformToIndex.Add(trans, joints.Count - 1);
				}

				return joints;
			}

			private Dictionary<string, HandJointType> getCustomSkeletonData(GameObject gob)
			{
				Dictionary<string, HandJointType> skeletonData = new Dictionary<string, HandJointType>();
				foreach (var trans in gob.GetComponentsInChildren<Transform>())
				{
					var typeComponent = trans.GetComponent<PicoHandJointType>();
					var type = HandJointType.Invalid;
					if (typeComponent != null)
					{
						type = typeComponent.jointType;
					}

					skeletonData.Add(trans.name, type);
				}

				return skeletonData;
			}


			private void alignHandRoot(Vector3 recevieForward, Vector3 recevieUp, Vector3 myForward, Vector3 myUp)
			{
				Quaternion RecevieQuat = Quaternion.LookRotation(recevieForward, recevieUp);
				Quaternion MyQuat = Quaternion.LookRotation(myForward, myUp);
				rootRetargetQuat = RecevieQuat * Quaternion.Inverse(MyQuat);
			}

			private JointType JointTypeFromHandType(HandJointType handType, HandSide _side)
			{
				if (handType == HandJointType.Invalid)
				{
					return JointType.Invalid;
				}

				if (handType == HandJointType.Wrist)
				{
					return _side == HandSide.Left ? JointType.LeftHandWrist : JointType.RightHandWrist;
				}

				var handBase = _side == HandSide.Left
					? JointType.LeftHandThumbTrapezium
					: JointType.RightHandThumbTrapezium;

				return handBase + (int)handType - 1;
			}

			#endregion
		}
	}
}