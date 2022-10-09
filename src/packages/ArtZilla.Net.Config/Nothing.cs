namespace ArtZilla.Net.Config;

public static class CurrentManager {
	public static object? Get() => SettingsManager.Provider;
}