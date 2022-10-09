using System.ComponentModel;

namespace ArtZilla.Net.Config;

/// Extension methods for settings
public static class SettingsExtensions {
	/// Gets a value indicating whether a settings exists.
	/// <returns><see langword="true" /> if the settings exists; <see langword="false" /> if the settings does not exist.</returns>
	public static bool IsExist(this ISettings settings)
		=> settings.Source?.IsExist(settings.GetInterfaceType()) ?? false;
	
	/// Gets a value indicating whether a settings exists.
	/// <returns><see langword="true" /> if the settings exists; <see langword="false" /> if the settings does not exist.</returns>
	public static Task<bool> IsExistAsync(this ISettings settings)
		=> settings.Source?.IsExistAsync(settings.GetInterfaceType()) ?? Task.FromResult(false);
	
	/// Remove this settings from settings provider
	public static bool Delete(this ISettings settings)
		=> settings.Source?.Delete(settings.GetInterfaceType())
		   ?? throw new("Can't delete settings without settings provider");
	
	/// Remove this settings from settings provider
	public static Task<bool> DeleteAsync(this ISettings settings)
		=> settings.Source?.DeleteAsync(settings.GetInterfaceType())
		   ?? throw new("Can't delete settings without settings provider");
	
	/// Reset all values to default
	public static void Reset(this ISettings settings)
		=> settings.Source?.Reset(settings.GetInterfaceType());
	
	///  Reset all values to default
	public static Task ResetAsync(this ISettings settings)
		=> settings.Source?.ResetAsync(settings.GetInterfaceType()) 
		   ?? throw new("Can't reset settings without settings provider");
	
	/// Flush this settings with settings provider (i.e. finish write to file, db) 
	public static void Flush(this ISettings settings)
		=> settings.Source?.Flush(settings.GetInterfaceType());
	
	/// Flush this settings with settings provider (i.e. finish write to file, db) 
	public static Task FlushAsync(this ISettings settings)
		=> settings.Source?.FlushAsync(settings.GetInterfaceType()) 
		   ?? throw new("Can't flush settings without settings provider");

	/// Subscribe to property changed event
	public static TSettings Subscribe<TSettings>(this TSettings settings, PropertyChangedEventHandler handler) 
		where TSettings : IInpcSettings {
		settings.PropertyChanged += handler;
		return settings;
	}
	
	/// Unsubscribe from property changed event
	public static TSettings Unsubscribe<TSettings>(this TSettings settings, PropertyChangedEventHandler handler) 
		where TSettings : IInpcSettings {
		settings.PropertyChanged -= handler;
		return settings;
	}
}