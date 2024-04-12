namespace Pico
{
	namespace Avatar
	{
		/// <summary>
		/// Proxy for native invoke for AvatarPreloadAssetPackageSet.
		/// </summary>
		class NativeCall_AvatarPreloadAssetPackageSet : NativeCallProxy
		{
			#region Caller Methods

			public void AddPackageMetaAsString(string packageName, string packageMetaJsonText,
				ResourceRootType resRootType, string basePathName)
			{
				var args = this._method_AddPackageMetaAsString.invokeArgumentTable;
				//
				args.SetStringParam(0, packageName);
				args.SetStringParam(1, packageMetaJsonText);
				args.SetUIntParam(2, (uint)resRootType);
				args.SetStringParam(3, basePathName);

				this._method_AddPackageMetaAsString.DoApply();
			}

			public void RemovePackage(string packageName)
			{
				var args = this._method_RemovePackage.invokeArgumentTable;
				args.SetStringParam(0, packageName);
				this._method_RemovePackage.DoApply();
			}

			public void RemovePackages()
			{
				var args = this._method_RemovePackages.invokeArgumentTable;
				this._method_RemovePackages.DoApply();
			}

			public void LoadPackageAssets(string jsonText, System.Action<NativeResult, string> callback)
			{
				var args = this._method_LoadPackageAssets.invokeArgumentTable;
				args.SetStringParam(0, jsonText);
				this._method_LoadPackageAssets.DoApply((resultParams, nativeCaller) =>
				{
					uint resultCode = 0;
					resultParams.GetUIntParam(0, ref resultCode);
					var errorText = resultParams.GetStringParam(1);
					if (callback != null)
					{
						callback((NativeResult)resultCode, errorText);
					}
				}, true);
			}

			#endregion


			#region Callee Methods

			#endregion


			#region NativeCall Framework Methods/Fields

			public NativeCall_AvatarPreloadAssetPackageSet(AvatarPreloadAssetPackageSet AvatarPreloadAssetPackageSet,
				uint instanceId)
				: base(0)
			{
				_avatarPreloadAssetPackageSet = AvatarPreloadAssetPackageSet;

				/// Callee methods.
				{
				}

				/// Caller methods.
				{
					this._method_AddPackageMetaAsString = AddCallerMethod(_attribute_AddPackageMetaAsString);
					this._method_RemovePackage = AddCallerMethod(_attribute_RemovePackage);
					this._method_RemovePackages = AddCallerMethod(_attribute_RemovePackages);
					this._method_LoadPackageAssets = AddCallerMethod(_attribute_LoadPackageAssets);
				}
			}

			#region Private Fields

			private AvatarPreloadAssetPackageSet _avatarPreloadAssetPackageSet;
			private NativeCaller _method_AddPackageMetaAsString;
			private NativeCaller _method_RemovePackage;
			private NativeCaller _method_RemovePackages;
			private NativeCaller _method_LoadPackageAssets;

			#endregion

			#region Static Part

			private const string className = "AvatarPreloadAssetPackageSet";

			/// Caller Attributes.
			private static NativeCallerAttribute _attribute_AddPackageMetaAsString =
				new NativeCallerAttribute(className, "AddPackageMetaAsString", (uint)0);

			private static NativeCallerAttribute _attribute_RemovePackage =
				new NativeCallerAttribute(className, "RemovePackage", (uint)0);

			private static NativeCallerAttribute _attribute_RemovePackages =
				new NativeCallerAttribute(className, "RemovePackages");

			private static NativeCallerAttribute _attribute_LoadPackageAssets = new NativeCallerAttribute(className,
				"LoadPackageAssets", (uint)NativeCallFlags.Async | (uint)NativeCallFlags.NeedReturn);

			/// Callee Attributes.

			#endregion
			
			#endregion
		}
	}
}