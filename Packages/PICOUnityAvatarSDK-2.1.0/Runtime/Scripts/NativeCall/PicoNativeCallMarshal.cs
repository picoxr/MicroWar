using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AOT;

namespace Pico
{
	namespace Avatar
	{
		/// <summary>
		/// native invoke marshal.
		/// </summary>
		public class NativeCallMarshal
		{
			// Manually register invokees.
			private static void RegisterCallees()
			{
				// register global callees.
				NativeCallee_Misc_LoadEmbededFileData.Register();
			}

			/// <summary>
			/// Initialize the marshal. Currently manually register types here.
			/// </summary>
			public static void Initialize()
			{
				if (_calleeTypes != null)
				{
					return;
				}

				_calleeTypes = new Dictionary<ulong, NativeCalleeAttribute>();

				///
				RegisterCallees();

				///
				pav_NativeCallMarshal_ClientConnect(OnDispatchCall, OnDispatchReturn);
			}

			/// <summary>
			/// Uninitialize.
			/// </summary>
			public static void Unitialize()
			{
				if (_calleeTypes == null)
				{
					return;
				}

				foreach (var x in _reuseCallees)
				{
					x.Value.Release();
				}

				_reuseCallees.Clear();

				//
				NativeCaller.RemoveWaitingCalls();

				// clear native invoked calback.
				pav_NativeCallMarshal_ClientDisconnect();


				//
				_calleeTypes.Clear();
				_calleeTypes = null;
			}

			/// <summary>
			/// Is marshal initialized.
			/// </summary>
			/// <returns></returns>
			public static bool IsInitialized()
			{
				return _calleeTypes != null;
			}

			public static void Update()
			{
				NativeCaller.ApplyAsyncCalls();
			}

			/// <summary>
			/// Register a invokees.
			/// </summary>
			/// <param name="invokeeAttribute"></param>
			/// <exception cref="Exception"></exception>
			public static void RegisterCallee(NativeCalleeAttribute invokeeAttribute)
			{
				//
				if (_calleeTypes.TryGetValue(invokeeAttribute.signatureId, out NativeCalleeAttribute existingOne))
				{
					if (existingOne != invokeeAttribute)
					{
						throw new System.Exception("RegisterCallee duplicated attribute with same signature id.");
					}

					return;
				}

				_calleeTypes.Add(invokeeAttribute.signatureId, invokeeAttribute);
			}


			#region Private Fields

			//
			private static Dictionary<System.IntPtr, NativeCallee> _reuseCallees =
				new Dictionary<System.IntPtr, NativeCallee>();

			// invokee list.
			private static Dictionary<ulong, NativeCalleeAttribute> _calleeTypes;

			#endregion


			#region Private Methods

			#endregion

			#region Native Methods

			const string PavDLLName = DllLoaderHelper.PavDLLName;

			//  callback type.
			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			internal delegate void OnDispatchCallCallback(System.IntPtr invokeePtr, System.IntPtr invokeParamsPtr,
				uint signatureID, uint flags, uint instanceId);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			internal delegate void OnDispatchReturnCallback(System.IntPtr invokerPtr, System.IntPtr returnParamsPtr);

			// PI callback.
			[MonoPInvokeCallback(typeof(OnDispatchCallCallback))]
			private static void OnDispatchCall(System.IntPtr invokeePtr, System.IntPtr invokeParamsPtr,
				uint signatureId, uint flags, uint instanceId)
			{
				bool releaseCalleePtr = true;
				bool releaseCallArgumentPtr = true;
				NativeCallee invokee = null;
				bool needReleaseCallee = true;
				bool needReturn = (flags & (uint)NativeCallFlags.NeedReturn) != 0;
				bool needReleaseCalleeOnReturn = false;

				// construct invokee object.
				if (_calleeTypes.TryGetValue(signatureId, out NativeCalleeAttribute invokeeAttribute))
				{
					//
					if (AvatarEnv.NeedLog(DebugLogMask.NativeCallTrivial))
					{
						AvatarEnv.Log(DebugLogMask.NativeCallTrivial,
							string.Format("OnDispatchInvoke class: {0} method: {1} ", invokeeAttribute.className,
								invokeeAttribute.methodName));
					}

					try
					{
						//
						if ((flags & (uint)NativeCallFlags.NotReuse) == 0)
						{
							var invokeeProxy = invokeeAttribute.GetInvokeProxy(instanceId);
							if (invokeeProxy != null)
							{
								invokee = invokeeProxy.GetCallee(invokeeAttribute, invokeePtr, invokeParamsPtr, flags,
									instanceId);
								//
								releaseCalleePtr = false;
								releaseCallArgumentPtr = false;
							}
							else if (_reuseCallees.TryGetValue(invokeePtr, out invokee))
							{
								if (invokee.instanceId != instanceId)
								{
									// set not reuse flag.
									flags |= (uint)NativeCallFlags.NotReuse;
									//
									invokee = null;
								}
							}
						}

						if (invokee == null)
						{
							needReleaseCalleeOnReturn = needReturn;
							needReleaseCallee = !needReturn;
							invokee = invokeeAttribute.CreateInstance(invokeePtr, flags);
							invokee.SetCallArgumentTable(invokeParamsPtr);
							//
							releaseCalleePtr = false;
							releaseCallArgumentPtr = false;
						}

						// retain invokee.
						invokee.Retain();

						//
						if ((flags & (uint)NativeCallFlags.NeedReturn) != 0)
						{
							invokee.SetNeedReturn(instanceId, needReleaseCalleeOnReturn);
						}

						//
						invokee.onCalled();
						//
					}
					catch (System.Exception e)
					{
						if (AvatarEnv.NeedLog(DebugLogMask.GeneralError))
						{
							AvatarEnv.Log(DebugLogMask.GeneralError,
								String.Format("NativeCallMarshal.onCalled New: {0} stack: {1}", e.Message,
									e.StackTrace));
						}
					}
				}
				else
				{
					var sb = new System.Text.StringBuilder();
					sb.Append("NativeCallMarshal.onCalled with invalid signatureID: ");
					sb.AppendLine(signatureId.ToString());
					sb.Append("Types:");
					foreach (var x in _calleeTypes)
					{
						sb.Append("  signatureID: ");
						sb.Append(x.Key);
						sb.Append("  name: ");
						sb.Append(x.Value.className);
						sb.Append(".");
						sb.Append(x.Value.methodName);
					}

					AvatarEnv.Log(DebugLogMask.GeneralError, sb.ToString());
				}

				//
				if (invokee != null && needReleaseCallee)
				{
					invokee.Release();
				}

				// release native ptr.
				if (releaseCalleePtr)
				{
					NativeObject.ReleaseNative(ref invokeePtr);
				}

				if (releaseCallArgumentPtr && invokeParamsPtr != System.IntPtr.Zero)
				{
					NativeObject.ReleaseNative(ref invokeParamsPtr);
				}
			}

			// PI callback.
			[MonoPInvokeCallback(typeof(OnDispatchReturnCallback))]
			private static void OnDispatchReturn(System.IntPtr invokerPtr, System.IntPtr returnParamsPtr)
			{
				// forward to NativeCaller.
				NativeCaller.DispatchReturn(invokerPtr, returnParamsPtr);
			}

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_NativeCallMarshal_ClientConnect(
				OnDispatchCallCallback onDispatchInvokeCallback, OnDispatchReturnCallback onDispatchReturnCallback);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_NativeCallMarshal_ClientDisconnect();

			#endregion
		}
	}
}