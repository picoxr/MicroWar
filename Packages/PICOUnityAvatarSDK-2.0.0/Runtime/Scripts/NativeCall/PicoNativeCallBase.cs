using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;


namespace Pico
{
	namespace Avatar
	{
		// Invoke Callback.
		using NativeCallCallback = System.Action<IDParameterTable, NativeCallee>;

		// Invoke target type.
		public enum NativeCallTargetType : uint
		{
			/// C#
			Client = 0,

			/// JS
			Script = 1,
		}

		// Flags for native invoke.
		public enum NativeCallFlags : uint
		{
			/// whether need call return method.
			NeedReturn = 1 << 0,

			/// whether has no invoke argument parameter table.
			NoCallArgument = 1 << 1,

			/// whether has no return value parameter table.
			NoReturnArgument = 1 << 2,

			/// whether asyn invoke.
			Async = 1 << 10,

			/// whether need resuse. if need reuse, should manually invoke destroy method.
			NotReuse = 1 << 11,
		};


		/// <summary>
		/// Caller(Caller) attributes.
		/// </summary>
		[AttributeUsage(AttributeTargets.Class)]
		public class NativeCallerAttribute : Attribute
		{
			// flags.
			public uint flags { get; private set; }
			public string className { get; private set; }
			public string methodName { get; private set; }

			// need return.
			public bool NeedReturn
			{
				get => (flags & (uint)NativeCallFlags.NeedReturn) != 0;
			}

			public bool NoCallArgument
			{
				get => (flags & (uint)NativeCallFlags.NoCallArgument) != 0;
			}

			public bool NoReturnArgument
			{
				get => (flags & (uint)NativeCallFlags.NoReturnArgument) != 0;
			}

			public bool Async
			{
				get => (flags & (uint)NativeCallFlags.Async) != 0;
			}

			public bool Reuse
			{
				get => (flags & (uint)NativeCallFlags.NotReuse) == 0;
			}

			public bool NotReuse
			{
				get => (flags & (uint)NativeCallFlags.NotReuse) != 0;
			}


			// class name id.
			public uint signatureID
			{
				get
				{
					if (_signatureID == 0)
					{
						_signatureID = Pico.Avatar.Utility.AddNameToIDNameTable(className + methodName);
					}

					return _signatureID;
				}
			}
			
			public NativeCallerAttribute(string className, string methodName, uint flags = 0)
			{
				this.className = className;
				this.methodName = methodName;
				this.flags = flags;
			}

			// Fields.
			private uint _signatureID = 0;
		}


		// Base class for native invoke.
		public class NativeCaller : NativeObject
		{
			#region Public Properties

			// instance id. generate locally.
			public uint instanceId { get; private set; }

			// whether asynchronous invoke returned.
			public bool isAsyncReturned { get; private set; }

			// class attribute
			public NativeCallerAttribute attribute { get; private set; }

			/// <summary>
			/// Gets invoke argument table.
			/// </summary>
			public IDParameterTable invokeArgumentTable
			{
				get => _callArguments;
			}

			/// <summary>
			/// Gets return argument table.
			/// </summary>
			public IDParameterTable returnArgumentTable
			{
				get => GetReturnParamsTable();
			}

			#endregion

			#region Public Methods
			
			public NativeCaller(NativeCallerAttribute attribute, uint instanceId)
			{
				this.attribute = attribute;
				this.instanceId = instanceId;
				//
				CheckCreateNative();
			}

			/// <summary>
			/// Do remote invoke.
			/// </summary>
			/// <param name="asyncReturnCallback"></param>
			/// <param name="asyncApply"></param>
			public void DoApply(System.Action<IDParameterTable, NativeCaller> asyncReturnCallback = null,
				bool asyncApply = false)
			{
				// set callback to receive retuned parameters.
				_asyncReturnCallback = asyncReturnCallback;

				// if asynchronous invoke, should keep track the invoker.
				if (!_inGlobalTable && attribute.Async &&
				    (attribute.Reuse || attribute.NeedReturn))
				{
					//
					isAsyncReturned = false;

					// set in table flag.
					_inGlobalTable = true;

					// increase reference count.
					this.Retain();
					//
					_allCallers.Add(nativeHandle, this);
				}

				if (asyncApply)
					ApplyAsync();
				else
					ApplyImmidiately();
			}

			/// <summary>
			/// Invoked when invoked
			/// </summary>
			/// <param name="returnParams"></param>
			public virtual void OnAsyncReturn(IDParameterTable returnParams)
			{
				//
				this.isAsyncReturned = true;
				//
				if (_asyncReturnCallback != null)
				{
					var callback = _asyncReturnCallback;
					_asyncReturnCallback = null;
					callback(returnParams, this);
				}
			}


			// Derived class can override the method to release resources when the object will be destroyed.
			protected override void OnDestroy()
			{
				ReferencedObject.ReleaseField(ref _callArguments);
				ReferencedObject.ReleaseField(ref _returnParams);
				//
				base.OnDestroy();
			}

			#endregion


			#region Private Fields

			// invoke arguments table.
			private IDParameterTable _callArguments;

			// invoke arguments table.
			private IDParameterTable _returnParams;

			// return callback.
			private System.Action<IDParameterTable, NativeCaller> _asyncReturnCallback;

			private static Queue<NativeCaller> _asyncCallers = new Queue<NativeCaller>();

			// waiting invoker list.
			private static Dictionary<System.IntPtr, NativeCaller> _allCallers = new Dictionary<IntPtr, NativeCaller>();

			// whether in global table.
			private bool _inGlobalTable = false;

			#endregion

			#region Private Methods

			// Do remote invoke asynchronous.
			private void ApplyAsync()
			{
				_asyncCallers.Enqueue(this);
			}

			// Do remote invoke immidiately.
			private void ApplyImmidiately()
			{
#if DEBUG
				if (PicoAvatarApp.instance != null && PicoAvatarApp.instance.localDebugSettings.traceNativeCaller)
				{
					AvatarEnv.Log(DebugLogMask.GeneralInfo, string.Format("caller : class:{0} method:{1} id:{2}"
						, attribute.className, attribute.methodName, attribute.signatureID));
				}
#endif

				//
				if (NativeResult.Success != pav_NativeCaller_DoApply(nativeHandle))
				{
					if (AvatarEnv.NeedLog(DebugLogMask.GeneralError))
					{
						AvatarEnv.Log(DebugLogMask.GeneralError, string.Format(
							"failed to call DoApply: class:{0} method:{1} id:{2}"
							, attribute.className, attribute.methodName, attribute.signatureID));
					}
				}
				else
				{
					//if (AvatarEnv.NeedLog(DebugLogMask.NativeCall_Trivial))
					//{
					//    //AvatarEnv.Log(string.Format("c#: do invoke success: class:{0} method:{1} id:{2}", attribute.className, attribute.methdName, attribute.signatureID));
					//}
				}
			}

			// Check create native object.
			private void CheckCreateNative()
			{
				if (nativeHandle != System.IntPtr.Zero)
				{
					return;
				}

				//var attrs = this.GetType().GetCustomAttributes(typeof(NativeCallerAttribute), true);
				//if (attrs == null || attrs.Length == 0)
				//{
				//    throw new System.ArgumentException("");
				//}
				//_ClassAttribute = (NativeCallerAttribute)attrs[0];

				//
				CreateResult result = pav_NativeCaller_Create((uint)NativeCallTargetType.Script,
					this.attribute.signatureID, this.attribute.flags, this.instanceId);

				//
				if (result.nativeInvokePtr != System.IntPtr.Zero)
				{
					if (result.invokeArgumentTablePtr != System.IntPtr.Zero)
					{
						_callArguments = new IDParameterTable(result.invokeArgumentTablePtr);
						_callArguments.Retain();
					}

					//
					SetNativeHandle(result.nativeInvokePtr, false);
				}
				else
				{
					// TODO:
				}
			}

			// Gets return parameter with native handle.
			IDParameterTable GetReturnParamsTable(System.IntPtr returnParamsPtr)
			{
				if (_returnParams != null)
				{
					NativeObject.ReleaseNative(ref returnParamsPtr);
					return _returnParams;
				}

				_returnParams = new IDParameterTable(returnParamsPtr);
				_returnParams.Retain();
				return _returnParams;
			}

			// Gets return parameter from invoker object.
			IDParameterTable GetReturnParamsTable()
			{
				if (_returnParams != null)
				{
					return _returnParams;
				}

				//
				_returnParams = new IDParameterTable(pav_NativeCaller_GetReturnArgumentTable(nativeHandle));
				_returnParams.Retain();
				return _returnParams;
			}

			#endregion


			#region Called by NativeCallMarshal

			// Invoked by NativeCallMarshal to apply async calls immidietely.
			internal static void ApplyAsyncCalls()
			{
				while (_asyncCallers.Count > 0)
				{
					var caller = _asyncCallers.Dequeue();
					if (caller != null)
						caller.ApplyImmidiately();
				}
			}


			// Invoked by NativeCallMarshal to process returned message.
			internal static void DispatchReturn(System.IntPtr NativeCallerHandle, System.IntPtr returnParamsPtr)
			{
				bool releaseReturnArgumentPtr = true;
				//
				if (_allCallers != null && _allCallers.TryGetValue(NativeCallerHandle, out NativeCaller caller))
				{
					IDParameterTable returnParams = null;
					bool needReleaseCaller = false;
					//
					try
					{
						//
						if (AvatarEnv.NeedLog(DebugLogMask.NativeCallTrivial))
						{
							AvatarEnv.Log(DebugLogMask.NativeCallTrivial, string.Format(
								"ProcessReturn class:{0} method:{1} id:{2}"
								, caller.attribute.className, caller.attribute.methodName,
								caller.attribute.signatureID));
						}

						//
						if (caller.attribute.NotReuse)
						{
							caller._inGlobalTable = false;
							needReleaseCaller = true;
							//
							_allCallers.Remove(NativeCallerHandle);

							//
							returnParams = new IDParameterTable(returnParamsPtr);
						}
						else
						{
							returnParams = caller.GetReturnParamsTable(returnParamsPtr);
						}

						returnParams.Retain();

						// if takeover the return argument, should not release.
						releaseReturnArgumentPtr = false;
						//
						caller.OnAsyncReturn(returnParams);
					}
					catch (System.Exception e)
					{
						if (AvatarEnv.NeedLog(DebugLogMask.GeneralError))
						{
							AvatarEnv.Log(DebugLogMask.GeneralError, e.Message);
						}
					}

					//
					if (returnParams != null)
					{
						returnParams.Release();
					}

					// release reference count.
					if (needReleaseCaller)
					{
						caller.Release();
					}
				}
				else
				{
					if (AvatarEnv.NeedLog(DebugLogMask.GeneralError))
					{
						AvatarEnv.Log(DebugLogMask.GeneralError,
							"ProcessReturn error: caller with native handle lost.");
					}
				}

				// always release NativeCallerHandle.
				NativeObject.ReleaseNative(ref NativeCallerHandle);

				// maybe need release returnParamsPtr.
				if (releaseReturnArgumentPtr && returnParamsPtr != System.IntPtr.Zero)
				{
					NativeObject.ReleaseNative(ref returnParamsPtr);
				}
			}

			/// <summary>
			/// Invoked by NativeCallMarshal when system shutdown to clear waiting invokes.
			/// </summary>
			public static void RemoveWaitingCalls()
			{
				if (_allCallers == null || _allCallers.Count == 0)
				{
					return;
				}

				var tmp = _allCallers;
				_allCallers = new Dictionary<IntPtr, NativeCaller>();
				foreach (var x in tmp)
				{
					try
					{
						x.Value.OnAsyncReturn(null);
					}
					catch (System.Exception e)
					{
						if (AvatarEnv.NeedLog(DebugLogMask.GeneralError))
						{
							AvatarEnv.Log(DebugLogMask.GeneralError, e.Message);
						}
					}

					//
					x.Value.Release();
				}
			}

			#endregion


			#region Native Methods

			const string PavDLLName = DllLoaderHelper.PavDLLName;

			[StructLayout(LayoutKind.Sequential, Pack = 8)]
			struct CreateResult
			{
				public System.IntPtr nativeInvokePtr;
				public System.IntPtr invokeArgumentTablePtr;
			}

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern CreateResult pav_NativeCaller_Create(uint targetType, uint signatureId, uint flags,
				uint instanceId);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_NativeCaller_DoApply(System.IntPtr nativeHandle);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern System.IntPtr pav_NativeCaller_GetReturnArgumentTable(System.IntPtr nativeHandle);

			#endregion
		}


		// Callee(callee) attributes.
		public class NativeCalleeAttribute
		{
			#region Public Properties

			// invokee type.
			public Type calleeType { get; private set; }
			public string className { get; private set; }
			public string methodName { get; private set; }

			// class name id.
			public uint signatureId
			{
				get
				{
					if (_signatureID == 0)
					{
						_signatureID = Pico.Avatar.Utility.AddNameToIDNameTable(className + methodName);
					}

					return _signatureID;
				}
			}

			#endregion

			#region Public Methods

			//
			public NativeCalleeAttribute(Type calleeType, string className, string methodName)
			{
				this.calleeType = calleeType;
				this.className = className;
				this.methodName = methodName;
			}
			
			public NativeCallee CreateInstance(System.IntPtr NativeCalleePtr, uint flags)
			{
				var invokee = (NativeCallee)calleeType.GetConstructor(_CalleeConstructorTypes).Invoke(
					_CalleeConstructorDefaultParams);
				invokee.AttachNativeCallee(NativeCalleePtr, flags);
				return invokee;
			}

			/// <summary>
			/// Adds skeleton object with instance id to the attribute.
			/// </summary>
			/// <param name="instanceId"></param>
			/// <param name="skeletonObject"></param>
			public void AddInvokeProxy(uint instanceId, NativeCallProxy skeletonObject)
			{
				_calleeProxies.Add(instanceId, skeletonObject);
			}

			/// <summary>
			/// Adds skeleton object with instance id to the attribute.
			/// </summary>
			/// <param name="instanceId"></param>
			public void RemoveInvokeProxy(uint instanceId)
			{
				_calleeProxies.Remove(instanceId);
			}

			/// <summary>
			/// Gets skeleton object with instanceId.
			/// </summary>
			/// <param name="instanceId"></param>
			/// <returns>null if failed to find</returns>
			public NativeCallProxy GetInvokeProxy(uint instanceId)
			{
				if (_calleeProxies.TryGetValue(instanceId, out NativeCallProxy skeletonObject))
				{
					return skeletonObject;
				}

				return null;
			}

			#endregion

			#region Private Fields

			// Fields.
			private uint _signatureID = 0;

			// skeleton object table. key is instance id.
			private Dictionary<uint, NativeCallProxy> _calleeProxies = new Dictionary<uint, NativeCallProxy>();

			private static Type[] _CalleeConstructorTypes = new Type[] { };

			//private static object[] _CalleeConstructorReleaseOnReturnParams = new object[1]{true};
			private static object[] _CalleeConstructorDefaultParams = new object[0] { };

			#endregion
		}


		// Base class for native invokee(callee).
		public class NativeCallee : NativeObject
		{
			// Instance id.
			public uint instanceId
			{
				get => _instanceId;
			}

			// call arguments
			public IDParameterTable callArguments
			{
				get => _callArguments;
			}

			//

			#region Public Methods

			public NativeCallee()
			{
			}

			/// <summary>
			/// Should be override by derived class to process invoke.
			/// </summary>
			public virtual void onCalled()
			{
				if (_onCalledCallback != null)
				{
					_onCalledCallback(_callArguments, this);
				}
			}

			/// <summary>
			/// check invoke return.
			/// </summary>
			public void DoReturn()
			{
				if (((_flags & (uint)NativeCallFlags.NeedReturn) != 0) && !_returned)
				{
					//
					_returned = true;
					//
					pav_NativeCallee_DoReturn(nativeHandle);
					//
					if (_needReleaseOnReturn)
					{
						this.Release();
					}
				}
			}

			#endregion


			#region Protected Methods

			// Derived class can override the method to release resources when the object will be destroyed.
			protected override void OnDestroy()
			{
				// destroy the callee.
				{
					// check return.
					if (((_flags & (uint)NativeCallFlags.NeedReturn) != 0) && !_returned)
					{
						DoReturn();
					}

					pav_NativeCallee_Destroy(nativeHandle);

					//
					ReferencedObject.ReleaseField(ref _returnArguments);
					ReferencedObject.ReleaseField(ref _callArguments);
				}
				//
				base.OnDestroy();
			}

			// Gets invoke argument table. If need not return value, need not invoke the method.
			protected IDParameterTable GetReturnArgumentTable()
			{
				if (_returnArguments == null)
				{
					_returnArguments = new IDParameterTable(pav_NativeCallee_GetReturnArgumentTable(nativeHandle));
					_returnArguments.Retain();
				}

				return _returnArguments;
			}

			public IDParameterTable SetCallArgumentTable(System.IntPtr nativeCallArgument)
			{
				if (_callArguments == null)
				{
					_callArguments = new IDParameterTable(nativeCallArgument);
					_callArguments.Retain();
				}
				else
				{
					NativeObject.ReleaseNative(ref nativeCallArgument);
				}

				return _callArguments;
			}

			#endregion

			#region Private Properties

			// invoke arguments table.
			private IDParameterTable _returnArguments;

			// invoke arguments table.
			private IDParameterTable _callArguments;

			// whether returned.
			private bool _returned = false;

			// need returned.
			private uint _flags = 0;

			// instance id.
			private uint _instanceId = 0;

			// onInvoke callback.
			private NativeCallCallback _onCalledCallback;

			// whether need release on return.
			private bool _needReleaseOnReturn = false;

			#endregion

			#region Called By NativeCallMarshal

			/// <summary>
			/// Attach native handle.
			/// </summary>
			/// <param name="nativeHandle_"></param>
			/// <param name="flags"></param>
			public void AttachNativeCallee(System.IntPtr nativeHandle_, uint flags)
			{
				_flags = flags;
				//
				SetNativeHandle(nativeHandle_, false);
			}

			/// <summary>
			/// Sets need return. Invoked by NativeCallMarshal to specify need return.
			/// </summary>
			/// <param name="instanceId"></param>
			/// <param name="needReleaseOnReturn">if a request service, callee should be automacially Released when OnReturn invoked.</param>
			public void SetNeedReturn(uint instanceId, bool needReleaseOnReturn)
			{
				_returned = false;
				_needReleaseOnReturn = needReleaseOnReturn;
			}

			/// <summary>
			/// Sets invoke callback.
			/// </summary>
			/// <param name="skeletonCallback"></param>
			public void SetSkeletonMethod(NativeCallCallback skeletonCallback)
			{
				_onCalledCallback = skeletonCallback;
			}

			#endregion

			#region Native Methods

			const string PavDLLName = DllLoaderHelper.PavDLLName;

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_NativeCallee_DoReturn(System.IntPtr nativeHandle);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern System.IntPtr pav_NativeCallee_GetReturnArgumentTable(System.IntPtr nativeHandle);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern void pav_NativeCallee_Destroy(System.IntPtr nativeHandle);

			#endregion
		}

		/// <summary>
		/// RMI(Remote Method Invoke) skeleton object.
		/// </summary>
		public class NativeCallProxy : ReferencedObject
		{
			// Instance id.
			public uint instanceId
			{
				get => _instanceId;
			}
			
			public NativeCallProxy(uint instanceId_)
			{
				_instanceId = instanceId_;
			}


			// Derived class can override the method to release resources when the object will be destroyed.
			protected override void OnDestroy()
			{
				// destroy it.
				{
					foreach (var x in _calleeMethods)
					{
						x.Key.RemoveInvokeProxy(_instanceId);
					}

					_calleeMethods.Clear();

					foreach (var x in _callees)
					{
						x.Value.Release();
					}

					_callees.Clear();

					// clear invokers.
					foreach (var x in _callers)
					{
						x.Release();
					}

					_callers.Clear();
				}

				// noitfy parent.
				base.OnDestroy();
			}

			#region Framework Methods

			// Adds method
			protected void AddCalleeMethod(NativeCalleeAttribute invokeeAttribute, NativeCallCallback methodCallback)
			{
				invokeeAttribute.AddInvokeProxy(_instanceId, this);
				//
				_calleeMethods.Add(invokeeAttribute, methodCallback);
				//
				NativeCallMarshal.RegisterCallee(invokeeAttribute);
			}

			/// <summary>
			/// Get NativeCallee object from the skeleton object.
			/// </summary>
			/// <param name="invokeAttribute"></param>
			/// <param name="NativeCalleePtr"></param>
			/// <param name="invokeArgumentsPtr"></param>
			/// <param name="flags"></param>
			/// <param name="instanceId"></param>
			/// <returns></returns>
			public NativeCallee GetCallee(NativeCalleeAttribute invokeAttribute, System.IntPtr NativeCalleePtr,
				System.IntPtr invokeArgumentsPtr, uint flags, uint instanceId)
			{
				// if no method bound with the attribute, do nothing.
				if (_callees.TryGetValue(invokeAttribute, out NativeCallee invokee)
				    || !_calleeMethods.TryGetValue(invokeAttribute, out NativeCallCallback invokeCallback))
				{
					NativeObject.ReleaseNative(ref NativeCalleePtr);
					NativeObject.ReleaseNative(ref invokeArgumentsPtr);
					return invokee;
				}

				// create new and add to table.
				invokee = invokeAttribute.CreateInstance(NativeCalleePtr, flags);
				invokee.Retain();
				//
				invokee.SetSkeletonMethod(invokeCallback);
				//
				invokee.SetCallArgumentTable(invokeArgumentsPtr);

				_callees.Add(invokeAttribute, invokee);
				return invokee;
			}

			/// <summary>
			/// Adds invoker method.
			/// </summary>
			/// <param name="attribute"></param>
			/// <returns></returns>
			public NativeCaller AddCallerMethod(NativeCallerAttribute attribute)
			{
				var invoker = new NativeCaller(attribute, _instanceId);
				invoker.Retain();
				//
				_callers.Add(invoker);
				//
				return invoker;
			}

			#endregion

			#region Private Fields

			//
			private uint _instanceId;

			// method table.
			private Dictionary<NativeCalleeAttribute, NativeCallCallback> _calleeMethods =
				new Dictionary<NativeCalleeAttribute, NativeCallCallback>();

			// method table.
			private Dictionary<NativeCalleeAttribute, NativeCallee> _callees =
				new Dictionary<NativeCalleeAttribute, NativeCallee>();

			//
			private List<NativeCaller> _callers = new List<NativeCaller>();

			#endregion
		}
	}
}