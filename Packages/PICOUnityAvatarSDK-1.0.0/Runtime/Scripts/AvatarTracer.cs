#if PAV_INTERNAL_DEV
namespace Pico
{
	namespace Avatar
	{
		/// <summary>
		/// The class instance created from PicoAvatarApp to receive avatar trace point events.
		/// </summary>
		public class AvatarTracer
		{
			// delegate of trace point event.
			public delegate void TracePointEvent(string avatarId, string tracePoint, string paramsJsonText);

			// event object list for trace point events.
			public event TracePointEvent OnTracePointEvent;

			public AvatarTracer()
			{
				this._rmiObject = new NativeCall_AvatarTracer(this, 0);
				_rmiObject.Retain();
			}

			/// <summary>
			/// Notification from native part that trace point arrived.
			/// </summary>
			/// <param name="avatarId"></param>
			/// <param name="tracePoint"></param>
			/// <param name="paramsJsonText"></param>
			public void OnTracePoint(string avatarId, string tracePoint, string paramsJsonText)
			{
				if (OnTracePointEvent != null)
				{
					OnTracePointEvent(avatarId, tracePoint, paramsJsonText);
				}
			}

			/// <summary>
			/// Destroy the object. invoked from PicoAvatarApp.
			/// </summary>
			public void Destroy()
			{
				if (_rmiObject != null)
				{
					_rmiObject.Release();
					_rmiObject = null;
				}
			}

			#region Private Fields

			// rmi object.
			private NativeCall_AvatarTracer _rmiObject = null;

			#endregion
		}
	}
}
#endif