using System;
using UnityEngine;

namespace Pico
{
	namespace Avatar
	{
		public static class TTNetUtility
		{
			public static bool isTTNetInited { get; private set; } = false;

			public static void InitTTNet()
			{
				if (isTTNetInited)
					return;

#if UNITY_EDITOR
				//do nothing
#elif UNITY_ANDROID
                try
                {
                    AndroidJavaClass UnityActivityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");

                    AndroidJavaObject UnityActivityObject =
 UnityActivityClass.GetStatic<AndroidJavaObject>("currentActivity");
                    AndroidJavaObject applicationContext =
 UnityActivityObject.Call<AndroidJavaObject>("getApplicationContext");

                    if(applicationContext == null)
                        Debug.LogError("getApplicationContext return null");

                    AndroidJavaClass TTNetUtilityClass = new AndroidJavaClass("com.pvr.avatarsdk.ttnet.TTNetUtility");

                    TTNetUtilityClass.CallStatic("InitTTNet", applicationContext);

                    isTTNetInited = true;
                }
                catch(Exception e)
                {
                    Debug.LogException(e);
                }

#endif
			}
		}
	}
}