namespace ArtZilla.Net.Config;

/// 
public interface ISyncSettingsProvider {
	/// Gets a value indicating whether a settings exists.
	/// <param name="type"></param>
	/// <param name="key"></param>
	/// <returns><see langword="true" /> if the settings exists; <see langword="false" /> if the settings does not exist.</returns>
	bool IsExist(Type type, string? key = null);

	/// 
	bool Delete(Type type, string? key = null);

	/// 
	void Reset(Type type, string? key = null);

	/// 
	void Flush(Type? type = null, string? key = null);

	/// 
	ISettings Get(Type type, SettingsKind kind, string? key = null);

	/// 
	void Set(ISettings settings, string? key = null);
}

/// 
public interface IAsyncSettingsProvider {
	/// Gets a value indicating whether a settings exists.
	/// <param name="type"></param>
	/// <param name="key"></param>
	/// <returns><see langword="true" /> if the settings exists; <see langword="false" /> if the settings does not exist.</returns>
	Task<bool> IsExistAsync(Type type, string? key = null);

	/// 
	Task<bool> DeleteAsync(Type type, string? key = null);

	/// 
	Task ResetAsync(Type type, string? key = null);

	/// 
	Task FlushAsync(Type? type = null, string? key = null);

	/// 
	Task<ISettings> GetAsync(Type type, SettingsKind kind, string? key = null);

	/// 
	Task SetAsync(ISettings settings, string? key = null);
}

/// Settings provider
public interface ISettingsProvider : ISyncSettingsProvider, IAsyncSettingsProvider {
	/// <see cref="ISettingsTypeConstructor"/>
	ISettingsTypeConstructor Constructor { get; }

	/// 
	void ThrowIfNotSupported(Type type);
}