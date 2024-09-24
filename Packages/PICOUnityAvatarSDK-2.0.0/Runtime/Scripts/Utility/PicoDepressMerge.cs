using UnityEngine;

namespace Pico
{
	namespace Avatar
	{
		/// <summary>
		/// Helper object to depress mesh render merge.
		/// </summary>
		class PicoDepressMerge : MonoBehaviour
		{
			// depress mesh renderer.
			public MeshRenderer depressMergeMeshRenderer;

			private void Start()
			{
				if (depressMergeMeshRenderer == null)
				{
					depressMergeMeshRenderer.SetPropertyBlock(new MaterialPropertyBlock());
				}
			}
		}
	}
}