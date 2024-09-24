namespace Pico
{
	namespace Avatar
	{
		//The class controls transition of avatar lod(Level Of Detail).
        //@remark The object SHOULD NOT be used by SDK user. The class is used by 
        //PicoAvatarManager and PicoAvatarManager.
        internal class PicoAvatarLodManager
		{
			#region Public Properties

			// singleton instance.
			public static PicoAvatarLodManager instance
			{
				get => _instance;
			}

			#endregion


			#region Public Methods

			public PicoAvatarLodManager()
			{
				if (_instance == null)
				{
					_instance = this;
				}
			}
			
            //Initialize lod configurations. 
            //@remark Should ONLY be invoked by PicoAvatarApp to initialize
            internal static void Initialize()
            {
                if(_instance == null && PicoAvatarApp.instance != null)
                {
                    _instance = new PicoAvatarLodManager();
                    //
                    _instance.Initialize(PicoAvatarApp.instance.lodSettings.forceLodLevel
                        , PicoAvatarApp.instance.lodSettings.maxLodLevel
                        , PicoAvatarApp.instance.lodSettings.lod0ScreenPercentage
                        , PicoAvatarApp.instance.lodSettings.lod1ScreenPercentage
                        , PicoAvatarApp.instance.lodSettings.lod2ScreenPercentage
                        , PicoAvatarApp.instance.lodSettings.lod3ScreenPercentage
                        , PicoAvatarApp.instance.lodSettings.lod4ScreenPercentage
                        );
                }
            }

			internal static void Unitialize()
			{
				if (_instance != null)
				{
					_instance.Uninitialize();
					_instance = null;
				}
			}
			
            //Sets lod range ratio, used to control lod switch range.
            //@remark Different scene need different ratio, generally is 1.0.
            internal void SetLodScreenPercentages(float lod0ScreenPercentage, float lod1ScreenPercentage
                , float lod2ScreenPercentage, float lod3ScreenPercentage, float lod4ScreenPercentage)
            {
                if (_rmiObject != null)
                {
                    _rmiObject.SetLodScreenPercentages(lod0ScreenPercentage, lod1ScreenPercentage,
                        lod2ScreenPercentage, lod3ScreenPercentage, lod4ScreenPercentage);
                }
            }

            /**
             * Sets force lod level.
             * @param forceLodLevel if is AvatarLodLevel.Invalid do not force lod level.
             * @param maxLodLevel max lod level.
             */
            internal void SetForceAndMaxLodLevel(AvatarLodLevel forceLodLevel, AvatarLodLevel maxLodLevel)
			{
				if (_rmiObject != null)
				{
					_rmiObject.SetForceAndMaxLodLevel(forceLodLevel, maxLodLevel);
				}
			}

			#endregion


			#region Private Fields

			// singleton instance.
			private static PicoAvatarLodManager _instance;

			// remote object.
			private NativeCall_AvatarLodManager _rmiObject;

			#endregion


			#region Framework Methods
			
            //Initialize native lod manager. Invoked by AvatarManager when initialized.
            //@param forceLodLevel if is AvatarLodLevel.Invalid do not force lod level.
            private void Initialize(AvatarLodLevel forceLodLevel, AvatarLodLevel maxLodLevel,
                float lod0ScreenPercentage, float lod1ScreenPercentage
                , float lod2ScreenPercentage, float lod3ScreenPercentage, float lod4ScreenPercentage)
            {
                if (this._rmiObject == null)
                {
                    _rmiObject = new NativeCall_AvatarLodManager(this, 0);
                    _rmiObject.Retain();
                    _rmiObject.Initialize(forceLodLevel, maxLodLevel,
                          lod0ScreenPercentage,  lod1ScreenPercentage,
                          lod2ScreenPercentage,  lod3ScreenPercentage, lod4ScreenPercentage);
                }
            }

			
            //Initialized by AvatarManager.
            public void Uninitialize()
			{
				if (this._rmiObject != null)
				{
					_rmiObject.Release();
					_rmiObject = null;
				}
			}

			#endregion


			#region Private Methods

			#endregion
		}
	}
}