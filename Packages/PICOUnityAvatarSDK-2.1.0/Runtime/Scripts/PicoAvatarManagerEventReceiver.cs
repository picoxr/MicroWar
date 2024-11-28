using System;
using UnityEngine;

namespace Pico.Avatar
{
	// Used to dispatch notification that avatar changed from avatar editor.
	internal class PicoAvatarManagerEventReceiver : MonoBehaviour
	{
		internal Action<string> OnRecvSystemAvatarChangeHandler;

		void OnRecvSystemAvatarChange(string data)
		{
			OnRecvSystemAvatarChangeHandler?.Invoke(data);
		}
	}
}