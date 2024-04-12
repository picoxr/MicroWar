using Pico.Avatar.XNode;

namespace Pico
{
	namespace Avatar
	{
		public class AvatarAnimationExit : Node
		{
			[Node.Input(backingValue = ShowBackingValue.Never)]
			public Enter exit;
		}
	}
}