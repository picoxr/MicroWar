using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace Pico
{
	namespace Avatar
	{
		// Manage definition client app interface, and then send msg event to client app.
		// @remark client corresponding ”com.pvr.avatarsdk.communication.base.MsgEventHub" class
		public class SdkClient
		{
			#region Native Invoke

			private static Dictionary<string, System.Reflection.ConstructorInfo> _sdkCallTraits =
				new Dictionary<string, System.Reflection.ConstructorInfo>();

			private static Dictionary<int, SdkCall.SdkCallBase> _waitingCalls =
				new Dictionary<int, SdkCall.SdkCallBase>();

			private static int _lastInstanceId = -1; // C# instance id--，  java instance id++

			/// <summary>
			/// Handle msg event from client app call
			/// </summary>
			/// <param name="callData"></param>
			public static void HandleSdkCall(string callData)
			{
#if DEBUG
				AvatarEnv.Log(DebugLogMask.NativeCallTrivial, "c# from java: HandleSdkCall. data:" + callData);
#endif
				var jsonObject = JsonConvert.DeserializeObject<JObject>(callData);
				if (jsonObject != null && jsonObject.TryGetValue("body", out JToken bodyValue))
				{
					var jsonBody = JsonConvert.DeserializeObject<Dictionary<string, object>>(bodyValue.ToString());
					var instanceId = jsonObject.Value<int>("instanceId");

					// Check if return msg event
					if (instanceId < 0)
					{
#if DEBUG
						AvatarEnv.Log(DebugLogMask.NativeCallTrivial,
							"from java: waiting invoke returned. instanceId:" + instanceId);
#endif
						SdkCall.SdkCallBase sdkCallBase = null;
						if (_waitingCalls.TryGetValue(instanceId, out sdkCallBase))
						{
							_waitingCalls.Remove(instanceId);

							sdkCallBase.HandleInvoke(jsonBody);
						}

						return;
					}
					else
					{
						// Invoked by client app and find local XXCall
						var typeName = jsonObject.Value<string>("type");
						var methodName = jsonObject.Value<string>("method");

						var key = typeName + "+" + methodName;

						//
						System.Reflection.ConstructorInfo invokeTrait = null;
						if (_sdkCallTraits.TryGetValue(key, out invokeTrait))
						{
							SdkCall.SdkCallBase sdkCallBase = (SdkCall.SdkCallBase)invokeTrait.Invoke(null);

							sdkCallBase.instanceId = jsonObject.Value<int>("instanceId");

#if DEBUG
							AvatarEnv.Log(DebugLogMask.NativeCallTrivial,
								"call from java : " + " typeName:" + typeName + " methodName:" + methodName);
#endif
							//
							sdkCallBase.HandleInvoke(jsonBody);
						}
						else
						{
							AvatarEnv.Log(DebugLogMask.GeneralError, string.Format(
								"c# Not supported NativeInvoe. type:{0} method:{1}"
								, typeName, methodName));
						}
					}
				}
			}

			/// <summary>
			/// Invoke sdk client.
			/// </summary>
			/// <param name="sdkCallBase"></param>
			public static void InvokePeer(SdkCall.SdkCallBase sdkCallBase)
			{
				//  If instanceId equal 0，explain take the initiative to invoke
				if (sdkCallBase.instanceId == 0)
				{
					sdkCallBase.instanceId = --_lastInstanceId;
					//
					if (sdkCallBase.needReturn)
					{
						_waitingCalls.Add(sdkCallBase.instanceId, sdkCallBase);
					}
				}

				//
				Dictionary<string, object> invoke = new Dictionary<string, object>();
				invoke["type"] = sdkCallBase.typeName;
				invoke["method"] = sdkCallBase.methodName;
				invoke["instanceId"] = sdkCallBase.instanceId;
				invoke["body"] = sdkCallBase.BuildInvokeBody();

#if DEBUG
				AvatarEnv.Log(DebugLogMask.NativeCallTrivial,
					"c# call sdk client. cassBase: " + JsonConvert.SerializeObject(invoke));
#endif
				//
				MsgEventHub.SentEventToApp(MsgEventHub.EventType.NativeCall, JsonConvert.SerializeObject(invoke), null);
			}

			/// <summary>
			/// Register class implement in SDK. eg: XXXCall. if C# take the initiative to invoke, please use xxxEvent name
			/// </summary>
			/// <param name="sdkCallBase"></param>
			public static void RegisterSDKInvoke(SdkCall.SdkCallBase sdkCallBase)
			{
				var key = sdkCallBase.typeName + "+" + sdkCallBase.methodName;
				//
				_sdkCallTraits.Add(key, sdkCallBase.GetType().GetConstructor(new System.Type[0] { }));
			}

			#endregion
		}
	}
}