namespace Pico
{
	namespace Avatar
	{
		// Base class of asynchronous requests.
		public class AsyncRequestBase : NativeCaller
		{
			public AsyncRequestBase(NativeCallerAttribute attribute) : base(attribute, GetNextInstanceId())
			{
			}

			/// <summary>
			/// request id is the same as invoker instance id.
			/// </summary>
			public long requestId
			{
				get => (long)instanceId;
			}

			/// <summary>
			/// Gets next local instance id.
			/// </summary>
			/// <returns></returns>
			private static uint GetNextInstanceId()
			{
				return ++_instanceId;
			}

			// Last request instance id.
			private static uint _instanceId = 1;
		}
	}
}