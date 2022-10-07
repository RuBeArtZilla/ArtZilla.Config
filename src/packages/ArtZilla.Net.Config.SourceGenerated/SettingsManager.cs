using System.Reflection;

namespace ArtZilla.Net.Config;

/// Global settings service
public static class SettingsManager {
	/// Company name
	public static string Company { get; set; } = null!;

	/// Application name 
	public static string AppName { get; set; } = null!;

	/// Default settings provider
	public static ISettingsProvider Provider { get; set; } = null!;

	static SettingsManager() 
		=> Init(); // init with default values

	/// Initialize SettingsManager
	/// <param name="app">application name</param>
	/// <param name="company">company name</param>
	/// <param name="provider">settings provider</param>
	public static void Init(string? app = null, string? company = null, ISettingsProvider? provider = null) {
		AppName = app ?? GetDefaultAppName();
		Company = company ?? GetDefaultCompany();
		Provider = provider ?? GetDefaultProvider();
	}

	static string GetDefaultAppName()
		=> Assembly.GetExecutingAssembly().GetName().Name ?? AppDomain.CurrentDomain.FriendlyName;
	
	static string GetDefaultCompany()
		=> string.Empty;
	
	static ISettingsProvider GetDefaultProvider()
	#if !NETSTANDARD20
		=> JsonFileSettingsProvider.Create(AppName, Company);
	#else
		=> JsonNetFileSettingsProvider.Create(AppName, Company);
	#endif
}