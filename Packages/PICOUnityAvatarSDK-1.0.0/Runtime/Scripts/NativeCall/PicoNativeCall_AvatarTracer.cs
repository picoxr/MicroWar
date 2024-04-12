#if PAV_INTERNAL_DEV
namespace Pico
{
	namespace Avatar
	{
		class NativeCall_AvatarTracer : NativeCallProxy
		{
			#region Caller Methods

			#endregion

			#region Callee Methods

			private void OnTracePoint(IDParameterTable invokeArguments, NativeCallee invokee)
			{
				var avatarId = invokeArguments.GetStringParam(0);
				var tracePointName = invokeArguments.GetStringParam(1);
				var paramJsonText = invokeArguments.GetStringParam(2);
				//
				this._AvatarTracer?.OnTracePoint(avatarId, tracePointName, paramJsonText);
			}

			#endregion


			#region NativeCall Framework Methods/Fields

			public NativeCall_AvatarTracer(AvatarTracer owner, uint instanceId)
				: base(instanceId)
			{
				_AvatarTracer = owner;

				/// Callee methods.
				{
					AddCalleeMethod(_attribute_OnTracePoint, OnTracePoint);
				}

				/// Caller methods.
				{
				}
			}

			#region Private Fields

			private AvatarTracer _AvatarTracer;
			private NativeCaller _method_OnTracePoint;

			#endregion

			#region Static Part

			private const string className = "AvatarTracer";

			/// Caller Attributes.
			/// Callee Attributes.
			private static NativeCalleeAttribute _attribute_OnTracePoint =
				new NativeCalleeAttribute(typeof(NativeCallee), className, "OnTracePoint");

			#endregion

			#endregion
		}
	}
}
#endif