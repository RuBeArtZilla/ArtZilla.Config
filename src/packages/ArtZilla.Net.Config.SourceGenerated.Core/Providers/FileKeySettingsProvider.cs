using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics;
using CommunityToolkit.Diagnostics;

namespace ArtZilla.Net.Config;
			 /*
class FileKeySettingsProvider : IKeySettingsProvider {
	/// <inheritdoc />
	public Type KeyType => _keyType;

	/// <inheritdoc />
	public Type SettingsType => _settingsType;

	/// <inheritdoc />
	public ISettingsProvider Parent => _parent;

	/// <inheritdoc />
	public ISettingsTypeConstructor Constructor => _constructor;

	public FileSerializer Serializer => _serializer;

	readonly Type _keyType;
	readonly Type _settingsType;
	readonly ISettingsProvider _parent;
	readonly FileSerializer _serializer;
	readonly ISettingsTypeConstructor _constructor;
	readonly string _location;
	readonly ConcurrentDictionary<string, IRealSettings> _map = new();

	public FileKeySettingsProvider(
		FileSettingsProvider parent,
		FileSerializer serializer,
		ISettingsTypeConstructor constructor,
		string location,
		Type keyType,
		Type settingsType
	) {
		Guard.IsNotNull(parent);
		Guard.IsNotNull(serializer);
		Guard.IsNotNull(constructor);
		Guard.IsNotNullOrWhiteSpace(location);
		
		_parent = parent;
		_keyType = keyType;
		_location = location;
		_settingsType = settingsType;
		_serializer = serializer;
		_constructor = constructor;
		
		Debug.Print("Created {0} with path {1}", GetType().Name, location);
	}

	/// <inheritdoc />
	public Task<ISettings> GetAsync(object key, SettingsKind kind) {
		var actualType = key.GetType();
		if (!actualType.IsAssignableFrom(_keyType))
			throw new($"Key {key} with type {actualType} can't be converted to type {_keyType}");

		var filename = EnframeFilename(actualType.ToString()) + Serializer.GetFileExtension();
		var real = _map.GetOrAdd(filename, CreateSettings);
		if (kind == SettingsKind.Real)
			return Task.FromResult<ISettings>(real);

		var settings = Constructor.Create(_settingsType, kind, real);
		return Task.FromResult(settings);
	}

	IRealSettings CreateSettings(string filename) {
		var settings = Constructor.CreateReal(_settingsType);
		var path = Path.Combine(_location, filename);
		if (File.Exists(path))
			Serializer.Deserialize(_settingsType, settings, path).Wait();

		settings.Subscribe(OnPropertyChanged);
		return settings;
	}

	void OnPropertyChanged(object sender, PropertyChangedEventArgs args) 
		=> Debug.Print("Changed property {0} of {1}", args.PropertyName, sender);

	string EnframeFilename(string filename) {
		return filename; // todo: ...
	}

}                                     */