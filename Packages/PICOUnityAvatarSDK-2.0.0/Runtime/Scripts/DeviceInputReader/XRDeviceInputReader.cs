#if NO_XR
namespace Pico
{
    namespace Avatar
    {
        public abstract class XRDeviceInputReader : IDeviceInputReader
        {

            internal override void InitInputFeatureUsage()
            {
            }
            internal override void UpdateButtonStatus()
            {
            }
            internal override void UpdateConnectionStatus()
            {
            }
            internal override void UpdateDevicePose()
            {
            }
        }
    }
}
#else
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.XR;

namespace Pico
{
	namespace Avatar
	{
		public abstract class XRDeviceInputReader : IDeviceInputReader
		{
			//InputFeatureUsage
			protected InputFeatureUsage<bool>[] buttonFeature =
				new InputFeatureUsage<bool>[(int)ControllerButtons.Count];

			protected InputFeatureUsage<Vector3> devicePositionFeature;
			protected InputFeatureUsage<Quaternion> deviceRotationFeature;

			protected InputFeatureUsage<bool> leftControllerStatusFeature;
			protected InputFeatureUsage<bool> rightControllerStatusFeature;
			protected InputFeatureUsage<bool> headControllerStatusFeature;

			internal override void InitInputFeatureUsage()
			{
				buttonFeature[(int)ControllerButtons.LPrimary2DButton] = UnityEngine.XR.CommonUsages.primary2DAxisClick;
				buttonFeature[(int)ControllerButtons.LMenuButton] = UnityEngine.XR.CommonUsages.menuButton;
				buttonFeature[(int)ControllerButtons.LGripButton] = UnityEngine.XR.CommonUsages.gripButton;
				buttonFeature[(int)ControllerButtons.LTriggerButton] = UnityEngine.XR.CommonUsages.triggerButton;
				buttonFeature[(int)ControllerButtons.XButton] = UnityEngine.XR.CommonUsages.primaryButton;
				buttonFeature[(int)ControllerButtons.YButton] = UnityEngine.XR.CommonUsages.secondaryButton;

				buttonFeature[(int)ControllerButtons.RPrimary2DButton] = UnityEngine.XR.CommonUsages.primary2DAxisClick;
				buttonFeature[(int)ControllerButtons.RMenuButton] = UnityEngine.XR.CommonUsages.menuButton;
				buttonFeature[(int)ControllerButtons.RGripButton] = UnityEngine.XR.CommonUsages.gripButton;
				buttonFeature[(int)ControllerButtons.RTriggerButton] = UnityEngine.XR.CommonUsages.triggerButton;
				buttonFeature[(int)ControllerButtons.AButton] = UnityEngine.XR.CommonUsages.primaryButton;
				buttonFeature[(int)ControllerButtons.BButton] = UnityEngine.XR.CommonUsages.secondaryButton;

				devicePositionFeature = UnityEngine.XR.CommonUsages.devicePosition;
				deviceRotationFeature = UnityEngine.XR.CommonUsages.deviceRotation;

				headControllerStatusFeature = UnityEngine.XR.CommonUsages.userPresence;
				leftControllerStatusFeature = UnityEngine.XR.CommonUsages.isTracked;
				rightControllerStatusFeature = UnityEngine.XR.CommonUsages.isTracked;


				deviceOffsets[(int)DeviceType.Head] = new Vector3(0, -0.15f, -0.1f);
				deviceOffsets[(int)DeviceType.LeftHand] = new Vector3(0.02f, 0.0f, 0.08f);
				deviceOffsets[(int)DeviceType.RightHand] = new Vector3(-0.02f, 0.0f, 0.08f);
				for (int i = 0; i < (int)DeviceType.Count; ++i)
				{
					deviceRotationOffsets[i] = new Quaternion(0, 0, 0, 1);
				}

				for (int i = 0; i < (int)DeviceType.Count; ++i)
				{
					connectionStatus[i] = true;
				}
			}

			internal override void UpdateButtonStatus()
			{
				if (!actionBased)
				{
					ControllerButtons[] leftControllerButtons =
					{
						ControllerButtons.LPrimary2DButton,
						ControllerButtons.LMenuButton,
						ControllerButtons.LGripButton,
						ControllerButtons.LTriggerButton,
						ControllerButtons.XButton,
						ControllerButtons.YButton
					};

					ControllerButtons[] rightControllerButtons =
					{
						ControllerButtons.RPrimary2DButton,
						ControllerButtons.RMenuButton,
						ControllerButtons.RGripButton,
						ControllerButtons.RTriggerButton,
						ControllerButtons.AButton,
						ControllerButtons.BButton
					};

					GetButtonState(XRNode.LeftHand, leftControllerButtons);
					GetButtonState(XRNode.RightHand, rightControllerButtons);

					for (int i = 0; i < (int)ControllerButtons.Count; ++i)
					{
						deviceData.controllerButtonStatus[i] = Convert.ToUInt32(controllerButtonStatus[i]);
					}
				}
				else if (buttonActions != null)
				{
					for (int i = 0; i < (int)ControllerButtons.Count && i < buttonActions.Length; ++i)
					{
						deviceData.controllerButtonStatus[i] =
							Convert.ToUInt32(buttonActions[i].action.ReadValue<float>());
					}
				}
			}

			protected void GetButtonState(XRNode node, params ControllerButtons[] buttons)
			{
				if (buttons == null) return;
				var inputDevice = GetXRInputDevice(node);
				for (int i = 0; i < buttons.Length; ++i)
				{
					inputDevice.TryGetFeatureValue(buttonFeature[(int)buttons[i]],
						out controllerButtonStatus[(int)buttons[i]]);
				}
			}

			internal override void UpdateDevicePose()
			{
				if (_owner == null || _owner.bodyAnimController == null)
				{
					return;
				}

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

				if (targetTransforms[(uint)DeviceType.Head] == null)
				{
					GetHeadXRDevice().TryGetFeatureValue(devicePositionFeature,
						out deviceData.positions[(int)DeviceType.Head]);
					GetHeadXRDevice().TryGetFeatureValue(deviceRotationFeature,
						out deviceData.orientations[(int)DeviceType.Head]);
				}

				if (targetTransforms[(uint)DeviceType.LeftHand] == null)
				{
					GetLeftHandXRDevice().TryGetFeatureValue(devicePositionFeature,
						out deviceData.positions[(int)DeviceType.LeftHand]);
					GetLeftHandXRDevice().TryGetFeatureValue(deviceRotationFeature,
						out deviceData.orientations[(int)DeviceType.LeftHand]);
				}

				if (targetTransforms[(uint)DeviceType.RightHand] == null)
				{
					GetRightHandXRDevice().TryGetFeatureValue(devicePositionFeature,
						out deviceData.positions[(int)DeviceType.RightHand]);
					GetRightHandXRDevice().TryGetFeatureValue(deviceRotationFeature,
						out deviceData.orientations[(int)DeviceType.RightHand]);
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
#if UNITY_EDITOR
				for (int i = 0; i < (int)DeviceType.Count; ++i)
				{
					deviceData.connectionStatus[i] = 1;
				}

				return;
#endif
				GetLeftHandXRDevice().TryGetFeatureValue(leftControllerStatusFeature,
					out connectionStatus[(int)DeviceType.LeftHand]);
				GetRightHandXRDevice().TryGetFeatureValue(rightControllerStatusFeature,
					out connectionStatus[(int)DeviceType.RightHand]);
				GetHeadXRDevice().TryGetFeatureValue(headControllerStatusFeature,
					out connectionStatus[(int)DeviceType.Head]);

				for (int i = 0; i < (int)DeviceType.Count; ++i)
				{
					deviceData.connectionStatus[i] = (uint)(connectionStatus[i] ? 1 : 0);
				}
			}

			#region XR Helpers

			Dictionary<XRNode, UnityEngine.XR.InputDevice> _xrNodes =
				new Dictionary<XRNode, UnityEngine.XR.InputDevice>();

			UnityEngine.XR.InputDevice _leftHandXRDevice;
			UnityEngine.XR.InputDevice _rightHandXRDevice;
			UnityEngine.XR.InputDevice _headHandXRDevice;

			private UnityEngine.XR.InputDevice GetXRInputDevice(XRNode xnode)
			{
				UnityEngine.XR.InputDevice device;
				if (_xrNodes.TryGetValue(xnode, out device))
				{
					if (device.isValid)
					{
						return device;
					}

					_xrNodes.Remove(xnode);
				}

				//UnityEngine.Debug.LogError("pav: GetXRInputDevice " + xnode.ToString());
				//
				device = InputDevices.GetDeviceAtXRNode(xnode);
				if (device.isValid)
				{
					_xrNodes.Add(xnode, device);
				}

				return device;
			}

			private UnityEngine.XR.InputDevice GetLeftHandXRDevice()
			{
				if (_leftHandXRDevice.isValid)
				{
					return _leftHandXRDevice;
				}

				_leftHandXRDevice = GetXRInputDevice(XRNode.LeftHand);
				return _leftHandXRDevice;
			}

			private UnityEngine.XR.InputDevice GetRightHandXRDevice()
			{
				if (_rightHandXRDevice.isValid)
				{
					return _rightHandXRDevice;
				}

				_rightHandXRDevice = GetXRInputDevice(XRNode.RightHand);
				return _rightHandXRDevice;
			}

			private UnityEngine.XR.InputDevice GetHeadXRDevice()
			{
				if (_headHandXRDevice.isValid)
				{
					return _headHandXRDevice;
				}

				_headHandXRDevice = GetXRInputDevice(XRNode.Head);
				return _headHandXRDevice;
			}

			#endregion
		}
	}
}
#endif