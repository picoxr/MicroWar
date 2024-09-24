using UnityEngine;

namespace Pico.Avatar
{
	// Avatar autofit height trigger,There are a few triggers
	// 1.your app is return focus
	public class PicoAvatarAutoFitTrigger : MonoBehaviour
	{
		#region Public Methods

		/// <summary>
		/// set autoFit target avatarEntity
		/// This function must be called when entity load is complete
		/// </summary>
		/// <param name="triggerCall"></param>
		public void SetTriggerCallback(System.Action triggerCall = null)
		{
			this.triggerCall = triggerCall;
		}

		#endregion


		#region Private/Friend Methods

		private System.Action triggerCall = null;
		private bool isOnFocus = true;

		private void OnApplicationFocus(bool focus)
		{
			Debug.Log("pav:OnApplicationFocus:" + focus);
			if (!isOnFocus && focus)
			{
				TriggerAutoFit();
			}

			isOnFocus = focus;
		}

		private void OnApplicationPause(bool pause)
		{
			Debug.Log("pav:OnApplicationPause:" + pause);
		}

		private void TriggerAutoFit()
		{
			if (triggerCall != null)
			{
				triggerCall.Invoke();
			}
		}

		#endregion
	}
}