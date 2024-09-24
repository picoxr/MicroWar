using UnityEngine;
using System;

namespace Pico
{
	namespace Avatar
	{
		public class EditorDeviceInputReader : IDeviceInputReader
		{
			#region Public Fields

			public bool useEditorTarget = true;

			#endregion


			#region Internal Methods

			internal override void Initialize(System.IntPtr nativeHandler_, AvatarEntity owner)
			{
				base.Initialize(nativeHandler_, owner);

				for (int i = 0; i < (int)DeviceType.Count; ++i)
				{
					deviceOffsets[i] = new Vector3(0, 0, 0);
					deviceRotationOffsets[i] = new Quaternion(0, 0, 0, 1);
				}
			}

			internal override void UpdateDevicePose()
			{
				Matrix4x4 rootMat = xrRoot == null ? Matrix4x4.identity : xrRoot.worldToLocalMatrix;
				Quaternion rootRotationInv = xrRoot == null ? Quaternion.identity : Quaternion.Inverse(xrRoot.rotation);
				Vector3 rootPosition = xrRoot == null ? Vector3.zero : xrRoot.position;

				Matrix4x4 avatarMat =
					_owner.owner == null ? Matrix4x4.identity : _owner.owner.transform.worldToLocalMatrix;
				Vector3 avatarRootScale = _owner.owner == null ? Vector3.zero : _owner.owner.transform.lossyScale;
				Vector3 avatarRootPosition = _owner.owner == null ? Vector3.zero : _owner.owner.transform.position;
				Quaternion avatarRotationInv = _owner.owner == null
					? Quaternion.identity
					: Quaternion.Inverse(_owner.owner.transform.rotation);
				Vector3 avatarScale = _owner.GetAvatarScale();

				for (int i = 0; i < (int)DeviceType.Count; ++i)
				{
					if (targetTransforms[i] != null)
					{
						if (useRelative)
						{
							deviceData.positions[i] = rootMat.MultiplyPoint(targetTransforms[i].position);
							///deviceData.positions[i] = rootRotationInv * (targetTransforms[i].position - rootPosition);
							deviceData.orientations[i] = rootRotationInv * targetTransforms[i].rotation;
						}
						else if (Math.Abs(avatarRootScale.x * avatarRootScale.y * avatarRootScale.z) > 0.01)
						{
							deviceData.positions[i] = avatarMat.MultiplyPoint(targetTransforms[i].position);
							///deviceData.positions[i] = rootRotationInv * (targetTransforms[i].position - rootPosition);
							deviceData.orientations[i] = avatarRotationInv * targetTransforms[i].rotation;
						}
						else
						{
							deviceData.positions[i] =
								rootRotationInv * (targetTransforms[i].position - avatarRootPosition);
							deviceData.orientations[i] = avatarRotationInv * targetTransforms[i].rotation;
						}
					}
				}

				deviceArmSpan = Vector3.Distance(deviceData.positions[(int)DeviceType.LeftHand],
					deviceData.positions[(int)DeviceType.RightHand]);

				//device offsets
				for (int i = 0; i < (int)DeviceType.Count; ++i)
				{
					deviceData.orientations[i] = deviceData.orientations[i] * deviceRotationOffsets[i];
					Vector3 positionOffset = deviceData.orientations[i] * deviceOffsets[i];
					positionOffset = Vector3.Scale(positionOffset, avatarScale);
					deviceData.positions[i] -= positionOffset;
				}
			}

			internal override void UpdateConnectionStatus()
			{
				for (int i = 0; i < (int)DeviceType.Count; ++i)
				{
					if (targetTransforms[i] != null)
					{
						connectionStatus[i] = (targetTransforms[i] != null &&
						                       targetTransforms[i].gameObject.activeInHierarchy);
					}
				}

				for (int i = 0; i < (int)DeviceType.Count; ++i)
				{
					deviceData.connectionStatus[i] = Convert.ToUInt32(connectionStatus[i]);
				}
			}

			internal override void UpdateButtonStatus()
			{
				if (actionBased)
				{
					if (buttonActions != null)
					{
						for (int i = 0; i < (int)ControllerButtons.Count && i < buttonActions.Length; ++i)
						{
							deviceData.controllerButtonStatus[i] =
								Convert.ToUInt32(buttonActions[i].action.ReadValue<float>());
						}
					}
				}
			}

			#endregion
		}
	}
}