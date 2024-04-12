using System.Collections.Generic;

namespace Pico
{
	namespace Avatar
	{
		/// <summary>
		/// Default animation layers for avatar hand anim
		/// </summary>
		public class AvatarDefaultAnimation
		{
			#region Public Properties

			#endregion


			#region Public Methods
			public AvatarDefaultAnimation(AvatarBodyAnimController bodyAnimController)
			{
				_bodyAnimController = bodyAnimController;
			}


			/// <summary>
			/// Animation layers for avatar hand pose, temporary simple version.
			/// Each layer contains a single hand fist animation clip, more animations will be provided and controller will be replaced by animation graph in future
			/// </summary>
			public void InitAnimationLayers()
			{
				if (_bodyAnimController == null)
				{
					return;
				}

				//gesture layers
				AvatarAnimationLayer lThumbLayer = _bodyAnimController.CreateAnimationLayerByName("LThumbLayer");
				lThumbLayer.SetLayerBlendMode(AnimLayerBlendMode.Override);
				lThumbLayer.SetSRTEnable(false, true, false);

				AvatarMask mask = new AvatarMask(_bodyAnimController);
				mask.SetAllJointsRotationEnable(false);
				for (uint i = (uint)JointType.LeftHandThumbTrapezium; i < (uint)JointType.LeftHandIndexMetacarpal; ++i)
				{
					mask.SetJointRotationEnable((JointType)i, true);
				}

				lThumbLayer.SetAvatarMask(mask);
				IDeviceInputReader.ControllerButtons[] lThumbButtons =
					{ IDeviceInputReader.ControllerButtons.XButton, IDeviceInputReader.ControllerButtons.YButton };
				_animationLayers.Add((lThumbLayer, lThumbButtons));
				_animationNames.Add("lHandFist");
				_animationStatus.Add(false);

				AvatarAnimationLayer rThumbLayer = _bodyAnimController.CreateAnimationLayerByName("RThumbLayer");
				rThumbLayer.SetLayerBlendMode(AnimLayerBlendMode.Override);
				rThumbLayer.SetSRTEnable(false, true, false);

				mask = new AvatarMask(_bodyAnimController);
				mask.SetAllJointsRotationEnable(false);
				for (uint i = (uint)JointType.RightHandThumbTrapezium;
				     i < (uint)JointType.RightHandIndexMetacarpal;
				     ++i)
				{
					mask.SetJointRotationEnable((JointType)i, true);
				}

				rThumbLayer.SetAvatarMask(mask);
				IDeviceInputReader.ControllerButtons[] rThumbButtons =
					{ IDeviceInputReader.ControllerButtons.AButton, IDeviceInputReader.ControllerButtons.BButton };
				_animationLayers.Add((rThumbLayer, rThumbButtons));
				_animationNames.Add("rHandFist");
				_animationStatus.Add(false);


				AvatarAnimationLayer lIndexLayer = _bodyAnimController.CreateAnimationLayerByName("LIndexLayer");
				lIndexLayer.SetLayerBlendMode(AnimLayerBlendMode.Override);
				lIndexLayer.SetSRTEnable(false, true, false);

				mask = new AvatarMask(_bodyAnimController);
				mask.SetAllJointsRotationEnable(false);
				for (uint i = (uint)JointType.LeftHandIndexMetacarpal;
				     i < (uint)JointType.LeftHandMiddleMetacarpal;
				     ++i)
				{
					mask.SetJointRotationEnable((JointType)i, true);
				}

				lIndexLayer.SetAvatarMask(mask);
				IDeviceInputReader.ControllerButtons[] lIndexButtons =
					{ IDeviceInputReader.ControllerButtons.LTriggerButton };
				_animationLayers.Add((lIndexLayer, lIndexButtons));
				_animationNames.Add("lHandFist");
				_animationStatus.Add(false);


				AvatarAnimationLayer rIndexLayer = _bodyAnimController.CreateAnimationLayerByName("RIndexLayer");
				rIndexLayer.SetLayerBlendMode(AnimLayerBlendMode.Override);
				rIndexLayer.SetSRTEnable(false, true, false);

				mask = new AvatarMask(_bodyAnimController);
				mask.SetAllJointsRotationEnable(false);
				for (uint i = (uint)JointType.RightHandIndexMetacarpal;
				     i < (uint)JointType.RightHandMiddleMetacarpal;
				     ++i)
				{
					mask.SetJointRotationEnable((JointType)i, true);
				}

				rIndexLayer.SetAvatarMask(mask);
				IDeviceInputReader.ControllerButtons[] rIndexButtons =
					{ IDeviceInputReader.ControllerButtons.RTriggerButton };
				_animationLayers.Add((rIndexLayer, rIndexButtons));
				_animationNames.Add("rHandFist");
				_animationStatus.Add(false);


				AvatarAnimationLayer lMiddleLayer = _bodyAnimController.CreateAnimationLayerByName("LMiddleLayer");
				lMiddleLayer.SetLayerBlendMode(AnimLayerBlendMode.Override);
				lMiddleLayer.SetSRTEnable(false, true, false);

				mask = new AvatarMask(_bodyAnimController);
				mask.SetAllJointsRotationEnable(false);
				for (uint i = (uint)JointType.LeftHandMiddleMetacarpal;
				     i < (uint)JointType.RightHandThumbMetacarpal;
				     ++i)
				{
					mask.SetJointRotationEnable((JointType)i, true);
				}


				lMiddleLayer.SetAvatarMask(mask);
				IDeviceInputReader.ControllerButtons[] lMiddleButtons =
					{ IDeviceInputReader.ControllerButtons.LGripButton };
				_animationLayers.Add((lMiddleLayer, lMiddleButtons));
				_animationNames.Add("lHandFist");
				_animationStatus.Add(false);


				AvatarAnimationLayer rMiddleLayer = _bodyAnimController.CreateAnimationLayerByName("RMiddleLayer");
				rMiddleLayer.SetLayerBlendMode(AnimLayerBlendMode.Override);
				rMiddleLayer.SetSRTEnable(false, true, false);

				mask = new AvatarMask(_bodyAnimController);
				mask.SetAllJointsRotationEnable(false);
				for (uint i = (uint)JointType.RightHandMiddleMetacarpal; i < (uint)JointType.RightHandPinkyTip; ++i)
				{
					mask.SetJointRotationEnable((JointType)i, true);
				}

				rMiddleLayer.SetAvatarMask(mask);
				IDeviceInputReader.ControllerButtons[] rMiddleButtons =
					{ IDeviceInputReader.ControllerButtons.RGripButton };
				_animationLayers.Add((rMiddleLayer, rMiddleButtons));
				_animationNames.Add("rHandFist");
				_animationStatus.Add(false);
			}


			internal void UpdateFrame()
			{
				if (_bodyAnimController == null || _bodyAnimController.owner == null)
				{
					return;
				}


				//update hand pose
				IDeviceInputReader deviceReader = _bodyAnimController.owner.deviceInputReader;
				if (deviceReader == null)
				{
					return;
				}

				var buttonStatus = deviceReader.deviceData.controllerButtonStatus;

				//trigger finger animations according to button status
				int layerIndex = 0;
				foreach (var layerAndButton in _animationLayers)
				{
					var layer = layerAndButton.Item1;
					var buttons = layerAndButton.Item2;
					bool needAnimation = false;
					foreach (var button in buttons)
					{
						if (buttonStatus[(uint)button] > 0)
						{
							needAnimation = true;
							if (_animationStatus[layerIndex] == false)
							{
								layer.PlayAnimationClip(_animationNames[layerIndex], 0, 1, 0.2f);
								_animationStatus[layerIndex] = true;
							}

							break;
						}

						if (needAnimation == false && _animationStatus[layerIndex] == true)
						{
							layer.StopAnimation(0.2f);
							_animationStatus[layerIndex] = false;
						}
					}

					++layerIndex;
				}
			}

			#endregion


			#region Private Fields

			AvatarBodyAnimController _bodyAnimController = null;

			List<(AvatarAnimationLayer, IDeviceInputReader.ControllerButtons[])> _animationLayers =
				new List<(AvatarAnimationLayer, IDeviceInputReader.ControllerButtons[])>();

			List<string> _animationNames = new List<string>();
			List<bool> _animationStatus = new List<bool>();

			#endregion
		}
	}
}