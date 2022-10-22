using Newtonsoft.Json;

namespace ArtZilla.Net.Config;

/// <inheritdoc />
public class JsonNetFileSerializer : FileSerializer {
	/// <inheritdoc />
	public JsonNetFileSerializer(ISettingsTypeConstructor constructor) : base(constructor) { }
	
	/// <inheritdoc />
	public override string GetFileExtension() => ".json";
	
	/// <inheritdoc />
	public override void Serialize(Type type, string path, ISettings settings) {
		var copyType = Constructor.GetType(type, SettingsKind.Copy);
		var copy = settings.GetType() == copyType
			? settings
			: Constructor.CloneCopy(settings);

		using var writer = File.CreateText(path);
		using var jsonWriter = new JsonTextWriter(writer);
		_serializer.Serialize(jsonWriter, copy, copyType);
	}

	/// <inheritdoc />
	public override ISettings Deserialize(Type type, string path) {
		using var reader = File.OpenText(path);
		using var jsonReader = new JsonTextReader(reader);
		var copyType = Constructor.GetType(type, SettingsKind.Copy);
		var copy = (ISettings) _serializer.Deserialize(jsonReader, copyType)!;
		return copy;
	}
	
	readonly JsonSerializer _serializer = JsonSerializer.Create(new() { Formatting = Formatting.Indented });
}

/// <inheritdoc />
public class JsonNetFileSettingsProvider : FileSettingsProvider {
	/// <inheritdoc />
	public JsonNetFileSettingsProvider()
		: this(new SameAssemblySettingsTypeConstructor()) { }
	/// <inheritdoc />
	public JsonNetFileSettingsProvider(string location)
		: this(location, new SameAssemblySettingsTypeConstructor()) { }

	/// <inheritdoc />
	public JsonNetFileSettingsProvider(ISettingsTypeConstructor constructor)
		: base(constructor, new JsonNetFileSerializer(constructor)) { }

	/// <inheritdoc />
	public JsonNetFileSettingsProvider(string location, ISettingsTypeConstructor constructor)
		: base(constructor, new JsonNetFileSerializer(constructor), location) { }

	///
	/// <param name="app"></param>
	/// <param name="company"></param>
	/// <returns></returns>
	public static JsonNetFileSettingsProvider Create(string app, string? company = null) 
		=> new(GetLocation(app, company));
}