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
	};
}