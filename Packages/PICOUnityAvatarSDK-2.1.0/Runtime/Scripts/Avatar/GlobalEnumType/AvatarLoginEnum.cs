namespace Pico.Avatar
{
	/// <summary>
	/// server type. boe or not
	/// </summary>
	public enum ServerType : uint
	{
		/// <summary>
		/// Offline Environment
		/// </summary>
		OfflineEnv = 0,

		/// <summary>
		/// Production Environment
		/// </summary>
		ProductionEnv = 1,
	};

	/// <summary>
	/// access type. Own Application or Third Application.
	/// </summary>
	public enum AccessType : uint
	{
		/// <summary>
		/// Own Application
		/// </summary>
		OwnApp = 0,

		/// <summary>
		/// Third Application
		/// </summary>
		ThirdApp = 1,
		
		/// <summary>
		/// Own AssetsPlatform
		/// </summary>
		OwnAssetsPlatform = 2,
	};
	
	/// <summary>
	/// App ModeType.
	/// </summary>
	public enum AppModeType
	{
		/// <summary>
		/// Unknown
		/// </summary>
		Unknown = 0,
		/// <summary>
		/// Public Mode
		/// </summary>
		Public = 1,

		/// <summary>
		/// Single Mode
		/// </summary>
		Single = 2,
		
		/// <summary>
		/// Private Mode
		/// </summary>
		Private = 3,
	};
	
	/// <summary>
	/// nation type. china or oversea
	/// </summary>
	public enum NationType : uint
	{
		/// <summary>
		/// Unknown
		/// </summary>
		Unknown = 0,
		/// <summary>
		/// China
		/// </summary>
		China = 1,

		/// <summary>
		/// Oversea
		/// </summary>
		Oversea = 2,
	};
}