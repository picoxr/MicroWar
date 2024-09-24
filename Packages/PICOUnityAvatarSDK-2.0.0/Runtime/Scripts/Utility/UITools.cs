using UnityEngine;

namespace Pico
{
	namespace Avatar
	{
		public class UITools
		{
			public static T GetChildComponent<T>(Transform root, string path) where T : Component
			{
				if (root == null)
					return null;
				var item = root.Find(path);
				if (item == null)
					return null;
				return item.GetComponent<T>();
			}

			public static void DebugLog(string content)
			{
				Debug.Log(string.Format("sdk_home_pav:{0},Time.time={1}", content, Time.frameCount));
			}

			public static void DebugErrrorLog(string content)
			{
				Debug.LogError(string.Format("sdk_home_pav:{0}", content));
			}
		}
	}
}