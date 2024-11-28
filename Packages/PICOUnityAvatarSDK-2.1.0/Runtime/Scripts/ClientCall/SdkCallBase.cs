using System.Collections.Generic;


namespace Pico
{
	namespace Avatar
	{
		namespace SdkCall
		{
			public class SdkCallBase
			{
				public int instanceId { get; set; }

				public virtual string typeName { get; }

				public virtual string methodName { get; }

				public virtual bool needReturn { get; }

				public virtual Dictionary<string, object> BuildInvokeBody()
				{
					return new Dictionary<string, object>();
				}

				public virtual void HandleInvoke(Dictionary<string, object> body)
				{
				}
			}
		}
	}
}