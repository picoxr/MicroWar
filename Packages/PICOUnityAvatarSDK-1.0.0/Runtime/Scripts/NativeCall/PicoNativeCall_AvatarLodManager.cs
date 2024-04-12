namespace Pico
{
	namespace Avatar
	{
		/// <summary>
		/// Proxy for native invoke for LodManager.
		/// </summary>
		class NativeCall_AvatarLodManager : NativeCallProxy
		{
			#region Caller Methods

            public void Initialize(AvatarLodLevel forceLodLevel, AvatarLodLevel maxLodLevel,
                float lod0ScreenPercentage, float lod1ScreenPercentage
                , float lod2ScreenPercentage, float lod3ScreenPercentage, float lod4ScreenPercentage)
            {
                var args = this._method_Initialize.invokeArgumentTable;
                args.SetIntParam(0, (int)forceLodLevel);
                args.SetIntParam(1, (int)maxLodLevel);
                args.SetFloatParam(2, lod0ScreenPercentage);
                args.SetFloatParam(3, lod1ScreenPercentage);
                args.SetFloatParam(4, lod2ScreenPercentage);
                args.SetFloatParam(5, lod3ScreenPercentage);
                args.SetFloatParam(6, lod4ScreenPercentage);
                //
                this._method_Initialize.DoApply();
            }
            public void SetForceAndMaxLodLevel(AvatarLodLevel forceLodLevel, AvatarLodLevel maxLodLevel)
            {
                var args = this._method_SetForceAndMaxLodLevel.invokeArgumentTable;
                args.SetIntParam(0, (int)forceLodLevel);
                args.SetIntParam(1, (int)maxLodLevel);
                //
                this._method_SetForceAndMaxLodLevel.DoApply();
            }
            public void SetLodScreenPercentages(float lod0ScreenPercentage, float lod1ScreenPercentage
                , float lod2ScreenPercentage, float lod3ScreenPercentage, float lod4ScreenPercentage)
            {
                var args = this._method_SetLodScreenPercentages.invokeArgumentTable;
                args.SetFloatParam(0, lod0ScreenPercentage);
                args.SetFloatParam(1, lod1ScreenPercentage);
                args.SetFloatParam(2, lod2ScreenPercentage);
                args.SetFloatParam(3, lod3ScreenPercentage);
                args.SetFloatParam(4, lod4ScreenPercentage);
                //
                this._method_SetLodScreenPercentages.DoApply();
            }

			public void SetLodScreenPercentages(float lod0ScreenPercentage, float lod1ScreenPercentage
				, float lod2ScreenPercentage, float lod3ScreenPercentage)
			{
				var args = this._method_SetLodScreenPercentages.invokeArgumentTable;
				args.SetFloatParam(0, lod0ScreenPercentage);
				args.SetFloatParam(1, lod1ScreenPercentage);
				args.SetFloatParam(2, lod2ScreenPercentage);
				args.SetFloatParam(3, lod3ScreenPercentage);
				//
				this._method_SetLodScreenPercentages.DoApply();
			}

			#endregion


			#region Callee Methods

			#endregion


			#region NativeCall Framework Methods/Fields

			public NativeCall_AvatarLodManager(PicoAvatarLodManager LodManager, uint instanceId)
				: base(0)
			{
				_LodManager = LodManager;

				/// Callee methods.
				{
					//AddCalleeMethod(_attribute_OnAvatarLoadFailed, OnAvatarLoadFailed);
				}

				/// Caller methods.
				{
					this._method_Initialize = AddCallerMethod(_attribute_Initialize);
					this._method_SetForceAndMaxLodLevel = AddCallerMethod(_attribute_SetForceAndMaxLodLevel);
					this._method_SetLodScreenPercentages = AddCallerMethod(_attribute_SetLodScreenPercentages);
				}
			}

			#region Private Fields

			private PicoAvatarLodManager _LodManager;
			private NativeCaller _method_Initialize;
			private NativeCaller _method_SetForceAndMaxLodLevel;
			private NativeCaller _method_SetLodScreenPercentages;

			#endregion

			#region Static Part

			private const string className = "AvatarLodManager";

			/// Caller Attributes.
			private static NativeCallerAttribute _attribute_Initialize =
				new NativeCallerAttribute(className, "Initialize", (uint)0);

			private static NativeCallerAttribute _attribute_SetForceAndMaxLodLevel =
				new NativeCallerAttribute(className, "SetForceAndMaxLodLevel", (uint)0);

			private static NativeCallerAttribute _attribute_SetLodScreenPercentages =
				new NativeCallerAttribute(className, "SetLodScreenPercentages", (uint)0);

			/// Callee Attributes.
			//private static NativeCalleeAttribute _attribute_OnAvatarLoadFailed = new NativeCalleeAttribute(typeof(NativeCallee), className, "OnAvatarLoadFailed");

			#endregion

			#endregion
		}
	}
}