namespace ArtZilla.Net.Config;

/// <inheritdoc />
public abstract class SyncSettingsProviderBase : ISettingsProvider {
	/// <inheritdoc />
	public abstract bool IsExist(Type type, string? key = null);

	/// <inheritdoc />
	public abstract bool Delete(Type type, string? key = null);

	/// <inheritdoc />
	public abstract void Reset(Type type, string? key = null);

	/// <inheritdoc />
	public abstract void Flush(Type? type = null, string? key = null);

	/// <inheritdoc />
	public abstract ISettings Get(Type type, SettingsKind kind, string? key = null);

	/// <inheritdoc />
	public abstract void Set(ISettings settings, string? key = null);

	/// <inheritdoc />
	Task<bool> IAsyncSettingsProvider.IsExistAsync(Type type, string? key)
		=> Task.FromResult(IsExist(type, key));

	/// <inheritdoc />
	Task<bool> IAsyncSettingsProvider.DeleteAsync(Type type, string? key)
		=> Task.FromResult(Delete(type, key));

	/// <inheritdoc />
	Task IAsyncSettingsProvider.ResetAsync(Type? type, string? key) {
		Reset(type, key);
		return Task.CompletedTask;
	}

	/// <inheritdoc />
	Task IAsyncSettingsProvider.FlushAsync(Type? type, string? key) {
		Flush(type, key);
		return Task.CompletedTask;
	}

	/// <inheritdoc />
	Task<ISettings> IAsyncSettingsProvider.GetAsync(Type type, SettingsKind kind, string? key)
		=> Task.FromResult(Get(type, kind, key));

	/// <inheritdoc />
	Task IAsyncSettingsProvider.SetAsync(ISettings settings, string? key) {
		Set(settings, key);
		return Task.CompletedTask;
	}

	/// <inheritdoc />
	public abstract ISettingsTypeConstructor Constructor { get; }

	/// <inheritdoc />
	public abstract void ThrowIfNotSupported(Type type);
								/*
	/// <inheritdoc />
	public abstract IKeySettingsProvider ByKey(Type keyType, Type settingsType);   */
}