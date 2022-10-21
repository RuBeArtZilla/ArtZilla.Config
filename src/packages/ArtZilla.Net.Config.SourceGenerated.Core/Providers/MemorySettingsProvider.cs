using System.Collections.Concurrent;

namespace ArtZilla.Net.Config;

/// 
public class MemorySettingsProvider : SyncSettingsProviderBase {
	/// 
	public MemorySettingsProvider() { }

	/// 
	public MemorySettingsProvider(ISettingsTypeConstructor constructor)
		: base(constructor) { }

	/// <inheritdoc />
	public override bool IsExist(Type type, string? key = null)
		=> _map.ContainsKey((type, key));

	/// <inheritdoc />
	public override bool Delete(Type type, string? key = null)
		=> _map.TryRemove((type, key), out _);

	/// <inheritdoc />
	public override void Reset(Type type, string? key = null) 
		=> Get(type, key).Copy(GetDefault(type, SettingsKind.Read));

	/// <inheritdoc />
	public override void Flush(Type? type = null, string? key = null) { }

	/// <inheritdoc />
	public override ISettings Get(Type type, SettingsKind kind, string? key = null) {
		var settings = Get(type, key);
		return kind == SettingsKind.Real
			? settings
			: Constructor.Clone(settings, kind);

	}

	/// <inheritdoc />
	public override void Set(ISettings settings, string? key = null)
		=> Get(settings.GetInterfaceType(), key).Copy(settings);
	
	IRealSettings Get(Type type, string? key)
		=> _map.GetOrAdd((type, key), Create);

	IRealSettings Create((Type Type, string? Key) pair)
		=> Constructor.DefaultReal(pair.Type, this, pair.Key);

	readonly ConcurrentDictionary<(Type Type, string? Key), IRealSettings> _map = new();
}

