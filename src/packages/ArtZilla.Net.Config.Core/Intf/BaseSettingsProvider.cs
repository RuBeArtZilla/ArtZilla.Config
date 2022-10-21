using CommunityToolkit.Diagnostics;

namespace ArtZilla.Net.Config;

/// Base class for implementation of <see cref="ISettingsProvider"/>
public abstract class BaseSettingsProvider : ISettingsProvider {
	/// <inheritdoc />
	public ISettingsTypeConstructor Constructor { get; }

	///
	protected BaseSettingsProvider()
		: this(new SameAssemblySettingsTypeConstructor()) { }

	///
	/// <param name="constructor"></param>
	protected BaseSettingsProvider(ISettingsTypeConstructor constructor)
		=> Constructor = constructor;

	/// <inheritdoc />
	public virtual ISettings GetDefault(Type type, SettingsKind kind) {
		if (kind == SettingsKind.Real)
			throw new("Can't create real default settings");

		return Constructor.Create(type, kind, this, null, true);
	}

	/// <inheritdoc />
	public virtual void ThrowIfNotSupported(Type type) {
		var settings = Constructor.DefaultRead(type, this, null);
		Guard.IsNotNull(settings);
	}

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
	public abstract Task<bool> IsExistAsync(Type type, string? key = null);

	/// <inheritdoc />
	public abstract Task<bool> DeleteAsync(Type type, string? key = null);

	/// <inheritdoc />
	public abstract Task ResetAsync(Type type, string? key = null);

	/// <inheritdoc />
	public abstract Task FlushAsync(Type? type = null, string? key = null);

	/// <inheritdoc />
	public abstract Task<ISettings> GetAsync(Type type, SettingsKind kind, string? key = null);

	/// <inheritdoc />
	public abstract Task SetAsync(ISettings settings, string? key = null);
}