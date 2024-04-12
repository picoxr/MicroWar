using System;

namespace Pico
{
	namespace Avatar
	{
		/// <summary>
		/// Configuration when loading avatar
		/// </summary>
		[System.Serializable]
		public class AvatarLoadContext
		{
			#region Public Fields

			public string userId = String.Empty;
			public string avatarId;

			public string jsonConfig;

			// Capabilities to control detail aspects to load for the avatar.
			public AvatarCapabilities capabilities;

			// Primitive node types that should depress merge. bits from AvatarNodeTypes
			public uint depressMergeNodeTypes = 0;

			// Placeholder avatar to use.
			public PicoPlaceholderAvatar placeHolderAvatar;

			#endregion


			#region Public Methods

			public AvatarLoadContext()
			{
			}

			/// <summary>
			/// AvatarLoadContext construction by specifying parameters
			/// </summary>
			/// <param name="userId_">Avatar is unique ID , if the backpack image is used, the openid obtained by the platform </param>
			/// <param name="avatarId_">The backpack image id specified under the current userId , the default "" is the current image of the backpack</param>
			/// <param name="jsonConfig_">Specify the image json configuration, the priority is greater than userId and avatarId</param>
			/// <param name="capabilities_">Set up image IK and other capacity configuration</param>
			public AvatarLoadContext(string userId_, string avatarId_, string jsonConfig_,
				AvatarCapabilities capabilities_ = null)
			{
				userId = userId_;
				avatarId = avatarId_;
				jsonConfig = jsonConfig_;
				capabilities = capabilities_ ?? new AvatarCapabilities();
			}

			/// <summary>
			/// AvatarLoadContext creation by userID and avatarId, this method is the wrapper of the constructor method
			/// </summary>
			/// <param name="userId_">Avatar is unique ID , if the backpack image is used, the openid obtained by the platform</param>
			/// <param name="avatarId_">The backpack image id specified under the current userId , the default "" is the current image of the backpack </param>
			/// <param name="capabilities_">Set up image IK and other capacity configuration</param>
			/// <returns>AvatarLoadContext</returns>
			public static AvatarLoadContext CreateByAvatarId(string userId_, string avatarId_,
				AvatarCapabilities capabilities_ = null)
			{
				return new AvatarLoadContext(userId_, avatarId_, String.Empty, capabilities_);
			}

			/// <summary>
			/// AvatarLoadContext is created through json configuration, this method is a wrapper of the constructor method
			/// </summary>
			/// <param name="userId_">Avatar is unique ID , if the backpack image is used, the openid obtained by the platform</param>
			/// <param name="jsonConfig_">Specify the image json configuration, the priority is greater than userId and avatarId</param>
			/// <param name="capabilities_">Set up image IK and other capacity configuration</param>
			/// <returns>AvatarLoadContext</returns>
			/// <exception cref="ArgumentNullException">jsonConfig_ can not be empty</exception>
			public static AvatarLoadContext CreateByJsonConfig(string userId_, string jsonConfig_,
				AvatarCapabilities capabilities_ = null)
			{
				if (string.IsNullOrEmpty(jsonConfig_))
				{
					throw new System.ArgumentNullException("jsonConfig_ can not be empty.");
				}

				// Make up a fake avatar id.
				var avatarId = _fakeAvatarIdPrefix + (_nextFakeAvatarId++).ToString();
				//
				return new AvatarLoadContext(userId_, avatarId, jsonConfig_, capabilities_);
			}

			/// <summary>
			///   if you have not userid or avatarid, you can use this func to create avatar
			///   but no userid may not be got avatar correctly
			/// </summary>
			/// <param name="capabilities_">create avatar capabilities desc</param>
			/// <returns>AvatarLoadContext</returns>
			public static AvatarLoadContext CreateDefault(AvatarCapabilities capabilities_ = null)
			{
				return new AvatarLoadContext(String.Empty, String.Empty, String.Empty, capabilities_);
			}

			/// <summary>
			/// Send native load request
			/// </summary>
			/// <returns>request success or not</returns>
			internal bool DoRequest()
			{
				//
				string capabilitiesData = capabilities.ToJson();
				long requestId = -1;
				//
				if (!string.IsNullOrEmpty(jsonConfig))
				{
					requestId = Pico.Avatar.LoadAvatarWithJsonSpecRequest.DoRequest(userId, jsonConfig,
						capabilitiesData, null);
				}
				else if (avatarId != null)
				{
					requestId = Pico.Avatar.LoadAvatarRequest.DoRequest(userId, avatarId, capabilitiesData, null);
				}
				else
				{
					AvatarEnv.Log(DebugLogMask.GeneralError,
						"AvatarLoadContext DoRequest filed, jsonConfig and avatarId are both null !");
					requestId = -1;
				}

				return requestId != -1;
			}

			#endregion


			#region Private Fields

			// Fake avatar id prefix.
			private static string _fakeAvatarIdPrefix = "fakeAvatar_";
			private static int _nextFakeAvatarId = 1;

			#endregion
		}
	}
}