using System.Collections.Generic;
using UnityEngine;

namespace Pico
{
	namespace Avatar
	{
		public static class NetEnvHelper
		{
			private enum AssetServerType
			{
				China = 1,
				OverSea,
				Boe_China,
				Boe_OverSea,
			}

			private static readonly Dictionary<AssetServerType, string> ServerConfigDomain =
				new Dictionary<AssetServerType, string>()
				{
					{ AssetServerType.China, "https://avatar.picovr.com" },
					{ AssetServerType.OverSea, "https://avatar-global.picovr.com" },
					{ AssetServerType.Boe_China, "https://avatar.picovr.com" },
					{ AssetServerType.Boe_OverSea, "https://avatar-global.picovr.com" },
				};


			public const string SampleTokenApi = "/api/v2/avatar/editor/token";

			/// <summary>
			/// 获取base url
			/// </summary>
			/// <param name="providerName"></param>
			/// <returns></returns>
			public static string GetHostServer(string providerName)
			{
				if (PicoAvatarApp.instance == null)
					return string.Empty;
				var assetServerType = providerName.Equals("oidc-pico-global")
					? AssetServerType.OverSea
					: AssetServerType.China;

				assetServerType = PicoAvatarApp.instance.appSettings.serverType == ServerType.ProductionEnv
					? assetServerType
					: assetServerType == AssetServerType.China
						? AssetServerType.Boe_China
						: AssetServerType.Boe_OverSea;

				ServerConfigDomain.TryGetValue(assetServerType, out var url);
				return string.IsNullOrEmpty(url) ? string.Empty : url;
			}

			public static string GetFullRequestUrl(string api, string providerName)
			{
				return GetHostServer(providerName) + api;
			}
		}
	}
}