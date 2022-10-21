namespace ArtZilla.Net.Config;

/// <inheritdoc cref="ISettingsProvider"/>
public abstract class SyncSettingsProviderBase : BaseSettingsProvider, ISettingsProvider {
	/// <inheritdoc />
	protected SyncSettingsProviderBase() { }

	/// <inheritdoc />
	protected SyncSettingsProviderBase(ISettingsTypeConstructor constructor)
		: base(constructor) { }

	/// <inheritdoc />
	public override Task<bool> IsExistAsync(Type type, string? key = null)
		=> Task.FromResult(IsExist(type, key));

	/// <inheritdoc />
	public override Task<bool> DeleteAsync(Type type, string? key = null)
		=> Task.FromResult(Delete(type, key));

	/// <inheritdoc />
	public override Task ResetAsync(Type type, string? key = null) {
		Reset(type, key);
		return Task.CompletedTask;
	}

	/// <inheritdoc />
	public override Task FlushAsync(Type? type = null, string? key = null) {
		Flush(type, key);
		return Task.CompletedTask;
	}

	/// <inheritdoc />
	public override Task<ISettings> GetAsync(Type type, SettingsKind kind, string? key = null)
		=> Task.FromResult(Get(type, kind, key));

	/// <inheritdoc />
	public override Task SetAsync(ISettings settings, string? key = null) {
		Set(settings, key);
		return Task.CompletedTask;
	}
}