using UnityEngine;
namespace Pico
{
	namespace Avatar
	{
		/// <summary>
		/// recieve client app msg event center
		/// client corresponding ”com.pvr.avatarsdk.communication.base.MsgEventHub“ class
		/// </summary>
		public class MsgEventHub
		{
			#region Event Names

			public enum EventType
			{
				NativeCall = 0, // local native invoke name type.
			}

			#endregion


			#region Event Sender

			/// <summary>
			/// send sync event to app client.
			/// </summary>
			/// <param name="eventType"></param>
			/// <param name="param1"></param>
			/// <param name="param2"></param>
			public static void SentEventToApp(EventType eventType, string param1, string param2)
			{
#if UNITY_EDITOR
				// Do Nothing.
#elif UNITY_ANDROID
        Android_SentEventToApp(eventType, param1, param2);
#endif
			}

			#endregion


			#region Event Consumer.

			/// <summary>
			/// Invoked by framework to read events from app level.
			/// </summary>
			/// <returns></returns>
			public static bool ReadAndDispatchMsgEvents()
			{
#if UNITY_EDITOR
				return false;
#elif UNITY_ANDROID
                return Android_ReadAndDispatchMsgEvents();
#else
                return false;
#endif
				return false;
			}

			// Dispatch Event in main thread.
			private static void DispatchMsgEvent(EventType eventType, string param1, string param2)
			{
#if DEBUG
				Debug.Log(string.Format("c DispatchMsgEvent. eventType:{0}, param1:{1}, param3:{2}",
					eventType.ToString(), param1, param2));
#endif

				// if general message. // postEvent("eventType", null, "eventType", param2);
				if (eventType == EventType.NativeCall)
				{
					SdkClient.HandleSdkCall(param1);
					return;
				}
			}

			#endregion


			#region Android Implemention

#if UNITY_ANDROID
			private static AndroidJavaClass _msgEventHubClass = null;
			private static AndroidJavaClass _unityBridgeEventItemClass = null;

			private static System.IntPtr _eventItem_Field_eventType;
			private static System.IntPtr _eventItem_Field_param1;
			private static System.IntPtr _eventItem_Field_param2;

			/// <summary>
			/// send sync event to app client.
			/// </summary>
			/// <param name="eventType"></param>
			/// <param name="param1"></param>
			/// <param name="param2"></param>
			public static void Android_SentEventToApp(EventType eventType, string param1, string param2)
			{
				if (CheckGetMsgEventHubClass())
				{
					_msgEventHubClass.CallStatic("onEvent", (int)eventType, param1, param2);
				}
			}

			/// <summary>
			/// Process NativeMsg event. Invoked by MsgManager in work thread.
			/// </summary>
			/// <returns></returns>
			public static bool Android_ReadAndDispatchMsgEvents()
			{
				bool eventArrived = false;
				//
				if (CheckGetMsgEventHubClass())
				{
					var joEventItemArray = _msgEventHubClass.CallStatic<AndroidJavaObject>("ReadEvents");
					if (joEventItemArray != null)
					{
						var ptrEventItems = joEventItemArray.GetRawObject();
						var size = AndroidJNI.GetArrayLength(ptrEventItems);

						if (size > 0)
						{
							eventArrived = true;
							//
							for (int i = 0; i < size; ++i)
							{
								var joEventItem = AndroidJNI.GetObjectArrayElement(ptrEventItems, i);
								//
								var eventType =
									(EventType)AndroidJNI.GetIntField(joEventItem, _eventItem_Field_eventType);
								var param1 = AndroidJNI.GetStringField(joEventItem, _eventItem_Field_param1);
								var param2 = AndroidJNI.GetStringField(joEventItem, _eventItem_Field_param2);
								//
								DispatchMsgEvent(eventType, param1, param2);
							}
						}

						joEventItemArray.Dispose();
					}
				}

				//
				return eventArrived;
			}

			private static bool CheckGetMsgEventHubClass()
			{
				if (_unityBridgeEventItemClass == null)
				{
					_unityBridgeEventItemClass = new AndroidJavaClass("com.pvr.avatarsdk.communication.base.MsgEvent");
#if DEBUG
					if (_unityBridgeEventItemClass == null)
					{
						UnityEngine.Debug.LogError("com.pvr.avatarsdk.communication.base.MsgEvent not found!");
						return false;
					}
#endif
					_eventItem_Field_eventType =
						AndroidJNIHelper.GetFieldID(_unityBridgeEventItemClass.GetRawClass(), "eventType");
					_eventItem_Field_param1 =
						AndroidJNIHelper.GetFieldID(_unityBridgeEventItemClass.GetRawClass(), "param1");
					_eventItem_Field_param2 =
						AndroidJNIHelper.GetFieldID(_unityBridgeEventItemClass.GetRawClass(), "param2");
				}

				// read events.
				if (_msgEventHubClass == null)
				{
					_msgEventHubClass = new AndroidJavaClass("com.pvr.avatarsdk.communication.base.MsgEventHub");
#if DEBUG
					if (_msgEventHubClass == null)
					{
						UnityEngine.Debug.LogError("com.pvr.avatarsdk.communication.base.MsgEventHub not found!");
						return false;
					}
#endif
				}

				return true;
			}
#endif

			#endregion
		}
	}
}