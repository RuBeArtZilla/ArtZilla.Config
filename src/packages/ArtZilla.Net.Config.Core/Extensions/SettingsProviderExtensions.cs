namespace ArtZilla.Net.Config;

/// Extension methods for settings provider
public static class SettingsProviderExtensions {
	/// <inheritdoc cref="ISyncSettingsProvider.IsExist"/>
	public static bool IsExist<TSettings>(this ISyncSettingsProvider provider) where TSettings : class, ISettings
		=> provider.IsExist(typeof(TSettings));
	
	/// <inheritdoc cref="ISyncSettingsProvider.IsExist"/>
	public static bool IsExist<TSettings>(this ISyncSettingsProvider provider, string key) where TSettings : class, ISettings
		=> provider.IsExist(typeof(TSettings), key);
	
	/// <inheritdoc cref="ISyncSettingsProvider.Flush"/>
	public static void Flush<TSettings>(this ISyncSettingsProvider provider) where TSettings : class, ISettings
		=> provider.Flush(typeof(TSettings));
	
	/// <inheritdoc cref="ISyncSettingsProvider.Flush"/>
	public static void Flush<TSettings>(this ISyncSettingsProvider provider, string key) where TSettings : class, ISettings
		=> provider.Flush(typeof(TSettings), key);
	
	/// <inheritdoc cref="ISyncSettingsProvider.Delete"/>
	public static bool Delete<TSettings>(this ISyncSettingsProvider provider) where TSettings : class, ISettings
		=> provider.Delete(typeof(TSettings));
	
	/// <inheritdoc cref="ISyncSettingsProvider.Delete"/>
	public static bool Delete<TSettings>(this ISyncSettingsProvider provider, string key) where TSettings : class, ISettings
		=> provider.Delete(typeof(TSettings), key);
	
	/// Get settings (copy)
	public static ICopySettings Copy(this ISyncSettingsProvider provider, Type type) 
		=> (ICopySettings) provider.Get(type, SettingsKind.Copy);
	
	/// Get settings (read)
	public static IReadSettings Read(this ISyncSettingsProvider provider, Type type, string? key = null)
		=> (IReadSettings) provider.Get(type, SettingsKind.Read, key);
	
	/// Get settings (inpc)
	public static IInpcSettings Inpc(this ISyncSettingsProvider provider, Type type) 
		=> (IInpcSettings) provider.Get(type, SettingsKind.Inpc);
	
	/// Get settings (real)
	public static IRealSettings Real(this ISyncSettingsProvider provider, Type type) 
		=> (IRealSettings) provider.Get(type, SettingsKind.Real);
		
	/// <inheritdoc cref="ISyncSettingsProvider.Get"/>
	public static TSettings Get<TSettings>(this ISyncSettingsProvider provider, SettingsKind kind) where TSettings : class, ISettings 
		=> (TSettings) provider.Get(typeof(TSettings), kind);
	
	/// <inheritdoc cref="ISyncSettingsProvider.Get"/>
	public static TSettings Get<TSettings>(this ISyncSettingsProvider provider, SettingsKind kind, string key) where TSettings : class, ISettings 
		=> (TSettings) provider.Get(typeof(TSettings), kind, key);
	
	/// Get settings (copy)
	public static TSettings Copy<TSettings>(this ISyncSettingsProvider provider) where TSettings : class, ISettings 
		=> (TSettings) provider.Get(typeof(TSettings), SettingsKind.Copy);
	
	/// Get settings (copy)
	public static TSettings Copy<TSettings>(this ISyncSettingsProvider provider, string key) where TSettings : class, ISettings 
		=> (TSettings) provider.Get(typeof(TSettings), SettingsKind.Copy, key);
	
	/// Get settings (read)
	public static TSettings Read<TSettings>(this ISyncSettingsProvider provider) where TSettings : class, ISettings 
		=> (TSettings) provider.Get(typeof(TSettings), SettingsKind.Read);
	
	/// Get settings (read)
	public static TSettings Read<TSettings>(this ISyncSettingsProvider provider, string key) where TSettings : class, ISettings 
		=> (TSettings) provider.Get(typeof(TSettings), SettingsKind.Read, key);
	
	/// Get settings (inpc)
	public static TSettings Inpc<TSettings>(this ISyncSettingsProvider provider) where TSettings : class, ISettings 
		=> (TSettings) provider.Get(typeof(TSettings), SettingsKind.Inpc);
	
	/// Get settings (inpc)
	public static TSettings Inpc<TSettings>(this ISyncSettingsProvider provider, string key) where TSettings : class, ISettings 
		=> (TSettings) provider.Get(typeof(TSettings), SettingsKind.Inpc, key);
	
	/// Get settings (real)
	public static TSettings Real<TSettings>(this ISyncSettingsProvider provider) where TSettings : class, ISettings 
		=> (TSettings) provider.Get(typeof(TSettings), SettingsKind.Real);
	
	/// Get settings (real)
	public static TSettings Real<TSettings>(this ISyncSettingsProvider provider, string key) where TSettings : class, ISettings 
		=> (TSettings) provider.Get(typeof(TSettings), SettingsKind.Real, key);
	
	/// <inheritdoc cref="IAsyncSettingsProvider.IsExistAsync"/>
	public static Task<bool> IsExistAsync<TSettings>(this IAsyncSettingsProvider provider) where TSettings : class, ISettings
		=> provider.IsExistAsync(typeof(TSettings));
	
	/// <inheritdoc cref="IAsyncSettingsProvider.IsExistAsync"/>
	public static Task<bool> IsExistAsync<TSettings>(this IAsyncSettingsProvider provider, string key) where TSettings : class, ISettings
		=> provider.IsExistAsync(typeof(TSettings), key);
	
	/// <inheritdoc cref="IAsyncSettingsProvider.FlushAsync"/>
	public static Task FlushAsync<TSettings>(this IAsyncSettingsProvider provider) where TSettings : class, ISettings
		=> provider.FlushAsync(typeof(TSettings));
	
	/// <inheritdoc cref="IAsyncSettingsProvider.FlushAsync"/>
	public static Task FlushAsync<TSettings>(this IAsyncSettingsProvider provider, string key) where TSettings : class, ISettings
		=> provider.FlushAsync(typeof(TSettings), key);
	
	/// <inheritdoc cref="IAsyncSettingsProvider.DeleteAsync"/>
	public static Task<bool> DeleteAsync<TSettings>(this IAsyncSettingsProvider provider) where TSettings : class, ISettings
		=> provider.DeleteAsync(typeof(TSettings));
	
	/// <inheritdoc cref="IAsyncSettingsProvider.DeleteAsync"/>
	public static Task<bool> DeleteAsync<TSettings>(this IAsyncSettingsProvider provider, string key) where TSettings : class, ISettings
		=> provider.DeleteAsync(typeof(TSettings), key);
	
	/// Get settings (copy)
	public static async Task<ICopySettings> CopyAsync(this IAsyncSettingsProvider provider, Type type) 
		=> (ICopySettings) await provider.GetAsync(type, SettingsKind.Copy);

	/// Get settings (read)
	public static async Task<IReadSettings> ReadAsync(this IAsyncSettingsProvider provider, Type type, string? key = null)
		=> (IReadSettings) await provider.GetAsync(type, SettingsKind.Read, key);
	
	/// Get settings (inpc)
	public static async Task<IInpcSettings> InpcAsync(this IAsyncSettingsProvider provider, Type type) 
		=> (IInpcSettings) await provider.GetAsync(type, SettingsKind.Inpc);
	
	/// Get settings (real)
	public static async Task<IRealSettings> RealAsync(this IAsyncSettingsProvider provider, Type type) 
		=> (IRealSettings)  await provider.GetAsync(type, SettingsKind.Real);
	
	/// Get settings (copy)
	public static async Task<TSettings> CopyAsync<TSettings>(this IAsyncSettingsProvider provider) where TSettings : class, ISettings 
		=> (TSettings)  await provider.GetAsync(typeof(TSettings), SettingsKind.Copy);
	
	/// Get settings (copy)
	public static async Task<TSettings> CopyAsync<TSettings>(this IAsyncSettingsProvider provider, string key) where TSettings : class, ISettings 
		=> (TSettings)  await provider.GetAsync(typeof(TSettings), SettingsKind.Copy, key);
	
	/// Get settings (read)
	public static async Task<TSettings> ReadAsync<TSettings>(this IAsyncSettingsProvider provider) where TSettings : class, ISettings 
		=> (TSettings) await  provider.GetAsync(typeof(TSettings), SettingsKind.Read);
	
	/// Get settings (read)
	public static async Task<TSettings> ReadAsync<TSettings>(this IAsyncSettingsProvider provider, string key) where TSettings : class, ISettings 
		=> (TSettings) await  provider.GetAsync(typeof(TSettings), SettingsKind.Read, key);
	
	/// Get settings (inpc)
	public static async Task<TSettings> InpcAsync<TSettings>(this IAsyncSettingsProvider provider) where TSettings : class, ISettings 
		=> (TSettings)  await provider.GetAsync(typeof(TSettings), SettingsKind.Inpc);
	
	/// Get settings (inpc)
	public static async Task<TSettings> InpcAsync<TSettings>(this IAsyncSettingsProvider provider, string key) where TSettings : class, ISettings 
		=> (TSettings)  await provider.GetAsync(typeof(TSettings), SettingsKind.Inpc, key);
	
	/// Get settings (real)
	public static async Task<TSettings> RealAsync<TSettings>(this IAsyncSettingsProvider provider) where TSettings : class, ISettings 
		=> (TSettings)  await provider.GetAsync(typeof(TSettings), SettingsKind.Real);

	/// Get settings (real)
	public static async Task<TSettings> RealAsync<TSettings>(this IAsyncSettingsProvider provider, string key) where TSettings : class, ISettings 
		=> (TSettings)  await provider.GetAsync(typeof(TSettings), SettingsKind.Real, key);

	public static void ThrowIfNotSupported<TSettings>(this ISettingsProvider provider) where TSettings : ISettings
		=> provider.ThrowIfNotSupported(typeof(TSettings));
}