using System.ComponentModel;

namespace ArtZilla.Net.Config;

public static class SettingsTypeConstructorExtensions {
	public static T Create<T>(this ISettingsTypeConstructor ctor, SettingsKind kind) where T : ISettings
		=> (T) ctor.Create(typeof(T), kind);
	
	public static T Create<T>(this ISettingsTypeConstructor ctor, SettingsKind kind, T source) where T : class, ISettings
		=> (T) ctor.Create(typeof(T), kind, source);
	
	public static ICopySettings CreateCopy(this ISettingsTypeConstructor ctor, Type type)
		=> (ICopySettings) ctor.Create(type, SettingsKind.Copy);
	
	public static T CreateCopy<T>(this ISettingsTypeConstructor ctor) where T : ISettings
		=> (T) ctor.Create(typeof(T), SettingsKind.Copy);
	
	public static T CreateCopy<T>(this ISettingsTypeConstructor ctor, T source) where T : class, ISettings
		=> (T) ctor.Create(typeof(T), SettingsKind.Copy, source);
	
	public static IReadSettings CreateRead(this ISettingsTypeConstructor ctor, Type type)
		=> (IReadSettings) ctor.Create(type, SettingsKind.Read);
	
	public static T CreateRead<T>(this ISettingsTypeConstructor ctor) where T : ISettings
		=> (T) ctor.Create(typeof(T), SettingsKind.Read);
	
	public static T CreateRead<T>(this ISettingsTypeConstructor ctor, T source) where T : class, ISettings
		=> (T) ctor.Create(typeof(T), SettingsKind.Read, source);
	
	public static IInpcSettings CreateInpc(this ISettingsTypeConstructor ctor, Type type)
		=> (IInpcSettings) ctor.Create(type, SettingsKind.Inpc);
	
	public static T CreateInpc<T>(this ISettingsTypeConstructor ctor) where T : ISettings
		=> (T) ctor.Create(typeof(T), SettingsKind.Inpc);
	
	public static T CreateInpc<T>(this ISettingsTypeConstructor ctor, T source) where T : class, ISettings
		=> (T) ctor.Create(typeof(T), SettingsKind.Inpc, source);
	
	public static IRealSettings CreateReal(this ISettingsTypeConstructor ctor, Type type)
		=> (IRealSettings) ctor.Create(type, SettingsKind.Real);

	public static T CreateReal<T>(this ISettingsTypeConstructor ctor) where T : ISettings
		=> (T) ctor.Create(typeof(T), SettingsKind.Real);
	
	public static T CreateReal<T>(this ISettingsTypeConstructor ctor, T source) where T : class, ISettings
		=> (T) ctor.Create(typeof(T), SettingsKind.Real, source);


	public static T CreateOld<T>(this ISettingsTypeConstructor ctor, SettingsKind kind) where T : IConfiguration
		=> (T) ctor.CreateOld(typeof(T), kind);
	
	public static T CreateOld<T>(this ISettingsTypeConstructor ctor, SettingsKind kind, T source) where T : class, IConfiguration
		=> (T) ctor.CreateOld(typeof(T), kind, source);
	
	public static T CreateCopyOld<T>(this ISettingsTypeConstructor ctor) where T : IConfiguration
		=> (T) ctor.CreateOld(typeof(T), SettingsKind.Copy);
	
	public static T CreateCopyOld<T>(this ISettingsTypeConstructor ctor, T source) where T : class, IConfiguration
		=> (T) ctor.CreateOld(typeof(T), SettingsKind.Copy, source);
	
	public static T CreateReadOld<T>(this ISettingsTypeConstructor ctor) where T : IConfiguration
		=> (T) ctor.CreateOld(typeof(T), SettingsKind.Read);
	
	public static T CreateReadOld<T>(this ISettingsTypeConstructor ctor, T source) where T : class, IConfiguration
		=> (T) ctor.CreateOld(typeof(T), SettingsKind.Read, source);
	
	public static T CreateInpcOld<T>(this ISettingsTypeConstructor ctor) where T : IConfiguration
		=> (T) ctor.CreateOld(typeof(T), SettingsKind.Inpc);
	
	public static T CreateInpcOld<T>(this ISettingsTypeConstructor ctor, T source) where T : class, IConfiguration
		=> (T) ctor.CreateOld(typeof(T), SettingsKind.Inpc, source);
	
	public static T CreateRealOld<T>(this ISettingsTypeConstructor ctor) where T : IConfiguration
		=> (T) ctor.CreateOld(typeof(T), SettingsKind.Real);
	
	public static T CreateRealOld<T>(this ISettingsTypeConstructor ctor, T source) where T : class, IConfiguration
		=> (T) ctor.CreateOld(typeof(T), SettingsKind.Real, source);
}

public static class SettingsProviderExtensions {
	/// <inheritdoc cref="ISyncSettingsProvider.IsExist"/>
	public static bool IsExist<TSettings>(this ISyncSettingsProvider provider) where TSettings : class, ISettings
		=> provider.IsExist(typeof(TSettings));
	
	/// <inheritdoc cref="ISyncSettingsProvider.Flush"/>
	public static void Flush<TSettings>(this ISyncSettingsProvider provider) where TSettings : class, ISettings
		=> provider.Flush(typeof(TSettings));
	
	/// <inheritdoc cref="ISyncSettingsProvider.Delete"/>
	public static bool Delete<TSettings>(this ISyncSettingsProvider provider) where TSettings : class, ISettings
		=> provider.Delete(typeof(TSettings));
	
	/// Get settings (copy)
	public static ICopySettings Copy(this ISyncSettingsProvider provider, Type type) 
		=> (ICopySettings) provider.Get(type, SettingsKind.Copy);
	
	/// Get settings (read)
	public static IReadSettings Read(this ISyncSettingsProvider provider, Type type) 
		=> (IReadSettings) provider.Get(type, SettingsKind.Read);
	
	/// Get settings (inpc)
	public static IInpcSettings Inpc(this ISyncSettingsProvider provider, Type type) 
		=> (IInpcSettings) provider.Get(type, SettingsKind.Inpc);
	
	/// Get settings (real)
	public static IRealSettings Real(this ISyncSettingsProvider provider, Type type) 
		=> (IRealSettings) provider.Get(type, SettingsKind.Real);
	
	/// Get settings (copy)
	public static TSettings Copy<TSettings>(this ISyncSettingsProvider provider) where TSettings : class, ISettings 
		=> (TSettings) provider.Get(typeof(TSettings), SettingsKind.Copy);
	
	/// Get settings (read)
	public static TSettings Read<TSettings>(this ISyncSettingsProvider provider) where TSettings : class, ISettings 
		=> (TSettings) provider.Get(typeof(TSettings), SettingsKind.Read);
	
	/// Get settings (inpc)
	public static TSettings Inpc<TSettings>(this ISyncSettingsProvider provider) where TSettings : class, ISettings 
		=> (TSettings) provider.Get(typeof(TSettings), SettingsKind.Inpc);
	
	/// Get settings (real)
	public static TSettings Real<TSettings>(this ISyncSettingsProvider provider) where TSettings : class, ISettings 
		=> (TSettings) provider.Get(typeof(TSettings), SettingsKind.Real);
	
	/// <inheritdoc cref="IAsyncSettingsProvider.IsExistAsync"/>
	public static Task<bool> IsExistAsync<TSettings>(this IAsyncSettingsProvider provider) where TSettings : class, ISettings
		=> provider.IsExistAsync(typeof(TSettings));
	
	/// <inheritdoc cref="IAsyncSettingsProvider.FlushAsync"/>
	public static Task FlushAsync<TSettings>(this IAsyncSettingsProvider provider) where TSettings : class, ISettings
		=> provider.FlushAsync(typeof(TSettings));
	
	/// <inheritdoc cref="IAsyncSettingsProvider.DeleteAsync"/>
	public static Task<bool> DeleteAsync<TSettings>(this IAsyncSettingsProvider provider) where TSettings : class, ISettings
		=> provider.DeleteAsync(typeof(TSettings));
	
	/// Get settings (copy)
	public static async Task<ICopySettings> CopyAsync(this IAsyncSettingsProvider provider, Type type) 
		=> (ICopySettings) await provider.GetAsync(type, SettingsKind.Copy);

	/// Get settings (read)
	public static async Task<IReadSettings> ReadAsync(this IAsyncSettingsProvider provider, Type type) 
		=> (IReadSettings) await provider.GetAsync(type, SettingsKind.Read);
	
	/// Get settings (inpc)
	public static async Task<IInpcSettings> InpcAsync(this IAsyncSettingsProvider provider, Type type) 
		=> (IInpcSettings) await provider.GetAsync(type, SettingsKind.Inpc);
	
	/// Get settings (real)
	public static async Task<IRealSettings> RealAsync(this IAsyncSettingsProvider provider, Type type) 
		=> (IRealSettings)  await provider.GetAsync(type, SettingsKind.Real);
	
	/// Get settings (copy)
	public static async Task<TSettings> CopyAsync<TSettings>(this IAsyncSettingsProvider provider) where TSettings : class, ISettings 
		=> (TSettings)  await provider.GetAsync(typeof(TSettings), SettingsKind.Copy);
	
	/// Get settings (read)
	public static async Task<TSettings> ReadAsync<TSettings>(this IAsyncSettingsProvider provider) where TSettings : class, ISettings 
		=> (TSettings) await  provider.GetAsync(typeof(TSettings), SettingsKind.Read);
	
	/// Get settings (inpc)
	public static async Task<TSettings> InpcAsync<TSettings>(this IAsyncSettingsProvider provider) where TSettings : class, ISettings 
		=> (TSettings)  await provider.GetAsync(typeof(TSettings), SettingsKind.Inpc);
	
	/// Get settings (real)
	public static async Task<TSettings> RealAsync<TSettings>(this IAsyncSettingsProvider provider) where TSettings : class, ISettings 
		=> (TSettings)  await provider.GetAsync(typeof(TSettings), SettingsKind.Real);

	public static void ThrowIfNotSupported<TSettings>(this ISettingsProvider provider) where TSettings : ISettings
		=> provider.ThrowIfNotSupported(typeof(TSettings));
}

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