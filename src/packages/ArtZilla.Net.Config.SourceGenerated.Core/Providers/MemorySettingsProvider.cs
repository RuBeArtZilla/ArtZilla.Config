using System.Collections.Concurrent;

namespace ArtZilla.Net.Config;

public abstract class SyncSettingsProviderBase : ISettingsProvider {
	/// <inheritdoc />
	public abstract bool IsExist(Type type);

	/// <inheritdoc />
	public abstract bool Delete(Type type);

	/// <inheritdoc />
	public abstract void Reset(Type type);

	/// <inheritdoc />
	public abstract void Flush(Type? type = null);

	/// <inheritdoc />
	public abstract ISettings Get(Type type, SettingsKind kind);

	/// <inheritdoc />
	public abstract void Set(ISettings settings);

	/// <inheritdoc />
	Task<bool> IAsyncSettingsProvider.IsExistAsync(Type type)
		=> Task.FromResult(IsExist(type));

	/// <inheritdoc />
	Task<bool> IAsyncSettingsProvider.DeleteAsync(Type type)
		=> Task.FromResult(Delete(type));

	/// <inheritdoc />
	Task IAsyncSettingsProvider.ResetAsync(Type? type) {
		Reset(type);
		return Task.CompletedTask;
	}

	/// <inheritdoc />
	Task IAsyncSettingsProvider.FlushAsync(Type? type) {
		Flush(type);
		return Task.CompletedTask;
	}

	/// <inheritdoc />
	Task<ISettings> IAsyncSettingsProvider.GetAsync(Type type, SettingsKind kind)
		=> Task.FromResult(Get(type, kind));

	/// <inheritdoc />
	Task IAsyncSettingsProvider.SetAsync(ISettings settings) {
		Set(settings);
		return Task.CompletedTask;
	}

	/// <inheritdoc />
	public abstract void ThrowIfNotSupported(Type type);
}


public class MemorySettingsProvider : SyncSettingsProviderBase {
	/// <inheritdoc cref="ISettingsTypeConstructor"/> 
	public ISettingsTypeConstructor Constructor { get; }

	public MemorySettingsProvider()
		: this(new SameAssemblySettingsTypeConstructor()) { }

	public MemorySettingsProvider(ISettingsTypeConstructor constructor)
		=> Constructor = constructor;

	/// <inheritdoc />
	public override bool IsExist(Type type)
		=> _map.ContainsKey(type);

	/// <inheritdoc />
	public override bool Delete(Type type)
		=> _map.TryRemove(type, out _);

	/// <inheritdoc />
	public override void Reset(Type type) 
		=> Get(type).Copy(Constructor.Create(type, SettingsKind.Read));

	/// <inheritdoc />
	public override void Flush(Type? type = null) { }

	/// <inheritdoc />
	public override ISettings Get(Type type, SettingsKind kind) {
		var real = Get(type);
		if (kind == SettingsKind.Real)
			return real;

		var settings = Constructor.Create(type, kind, Get(type));
		settings.Source = this;
		return settings;
	}

	/// <inheritdoc />
	public override void Set(ISettings settings)
		=> Get(settings.GetInterfaceType()).Copy(settings);

	/// <inheritdoc />
	public override void ThrowIfNotSupported(Type type) 
		=> Create(type);

	IRealSettings Get(Type type)
		=> _map.GetOrAdd(type, Create);

	IRealSettings Create(Type i) {
		var settings = Constructor.CreateReal(i);
		settings.Source = this;
		return settings;
	}

	readonly ConcurrentDictionary<Type, IRealSettings> _map = new();
}

