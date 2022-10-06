using System.Reflection;

namespace ArtZilla.Net.Config;

public static class SettingsManager {
	public static string Company { get; set; }

	public static string AppName { get; set; }

	public static ISettingsProvider Provider { get; }

	static SettingsManager() {
		AppName = Assembly.GetExecutingAssembly().GetName().Name ?? AppDomain.CurrentDomain.FriendlyName;
		Company = "";
		#if !NETSTANDARD20
			Provider = JsonFileSettingsProvider.Create(AppName, Company);
		#else
			Provider = JsonNetFileSettingsProvider.Create(AppName, Company);
		#endif
	}
}