using System.Collections.Concurrent;

namespace ArtZilla.Net.Config;

/// 
public class MemorySettingsProvider : SyncSettingsProviderBase {
	/// <inheritdoc cref="ISettingsTypeConstructor"/> 
	public override ISettingsTypeConstructor Constructor { get; }

	/// 
	public MemorySettingsProvider()
		: this(new SameAssemblySettingsTypeConstructor()) { }

	/// 
	public MemorySettingsProvider(ISettingsTypeConstructor constructor)
		=> Constructor = constructor;

	/// <inheritdoc />
	public override bool IsExist(Type type, string? key = null)
		=> _map.ContainsKey((type, key));

	/// <inheritdoc />
	public override bool Delete(Type type, string? key = null)
		=> _map.TryRemove((type, key), out _);

	/// <inheritdoc />
	public override void Reset(Type type, string? key = null) 
		=> Get(type, key).Copy(Constructor.Create(type, SettingsKind.Read));

	/// <inheritdoc />
	public override void Flush(Type? type = null, string? key = null) { }

	/// <inheritdoc />
	public override ISettings Get(Type type, SettingsKind kind, string? key = null) {
		var real = Get(type, key);
		if (kind == SettingsKind.Real)
			return real;

		var settings = Constructor.Create(type, kind, Get(type, key));
		settings.Source = this;
		return settings;
	}

	/// <inheritdoc />
	public override void Set(ISettings settings, string? key = null)
		=> Get(settings.GetInterfaceType(), key).Copy(settings);

	/// <inheritdoc />
	public override void ThrowIfNotSupported(Type type) 
		=> Create((type, null));

	IRealSettings Get(Type type, string? key)
		=> _map.GetOrAdd((type, key), Create);

	IRealSettings Create((Type Type, string? Key) pair) {
		var settings = Constructor.CreateReal(pair.Type);
		settings.Source = this;
		settings.SourceKey = pair.Key;
		return settings;
	}

	readonly ConcurrentDictionary<(Type Type, string? Key), IRealSettings> _map = new();
}

