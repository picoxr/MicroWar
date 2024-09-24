using UnityEngine;
using Pico.Avatar.XNode;

namespace Pico
{
	namespace Avatar
	{
		public class AvatarAnimationEntry : Node
		{
			[Node.Output(backingValue = ShowBackingValue.Never)]
			public Transition entry;

			public override void InitTransition(AvatarAnimationLayer animationLayer)
			{
				NodePort nodePort = GetOutputPort(nameof(entry)).Connection;
				if (nodePort != null)
				{
					AvatarAnimationStateNode nextNode = nodePort.node as AvatarAnimationStateNode;
					Debug.Log("entry : " + nextNode.stateName);
					animationLayer.entryState.AddTransition(nextNode.animationState);
				}
			}
		}
	}
}