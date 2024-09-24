using UnityEngine;

namespace Pico
{
	namespace Avatar
	{
		/// <summary>
		/// manipulator for mesh socket. ONLY used internally, developer SHOULD NOT use the class.
		/// </summary>
		public class PicoAvatarMeshSocketManipulator : MonoBehaviour
		{
			public PicoAvatar avatar;
			public string meshName;
			public string assetId;
			public bool desireMainAvatar = true;

			public void AttachTarget()
			{
				if (avatar == null && PicoAvatarManager.instance != null)
				{
					PicoAvatarManager.instance.VisitAvatars((avatar_) =>
					{
						if (avatar_.isAnyEntityReady)
						{
							avatar = avatar_;

							if (desireMainAvatar && avatar_.capabilities.isMainAvatar)
							{
								return true;
							}
							else
							{
								return false;
							}
						}

						return true;
					});
				}

				if (avatar == null || avatar.entity == null)
				{
					return;
				}

				var avatarLod = avatar.entity.GetCurrentAvatarLod();
				if (avatarLod != null)
				{
					var primitives = avatarLod.primitives;
					foreach (var x in primitives)
					{
						if (x.Value.meshSocket != null)
						{
							if (assetId == x.Value.GetAssetId())
							{
								_meshSocket = x.Value.meshSocket;
								break;
							}
						}
					}
				}
			}

			public void OnGUI()
			{
			}

			private void Update()
			{
				if (_meshSocket != null)
				{
					_meshSocket.SetLocalPosition(transform.localPosition);
					_meshSocket.SetLocalScale(transform.localScale);
					_meshSocket.SetLocalOrientation(transform.localRotation);
				}
			}

			#region Private Fields

			private AvatarMeshSocket _meshSocket;

			#endregion
		}
	}
}