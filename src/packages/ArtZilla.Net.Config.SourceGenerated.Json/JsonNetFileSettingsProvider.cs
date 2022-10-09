using Newtonsoft.Json;

namespace ArtZilla.Net.Config;

public class JsonNetFileSerializer : FileSerializer {
	public JsonNetFileSerializer(ISettingsTypeConstructor constructor) : base(constructor) { }
	
	/// <inheritdoc />
	public override string GetFileExtension() => ".json";
	
	/// <inheritdoc />
	public override Task Serialize(Type type, IRealSettings settings, string path) {
		var copyType = Constructor.GetType(type, SettingsKind.Copy);
		var copy = Constructor.Create(type, SettingsKind.Copy, settings);
		
		using var writer = File.CreateText(path);
		using var jsonWriter = new JsonTextWriter(writer);
		_serializer.Serialize(jsonWriter, copy, copyType);
		return Task.CompletedTask;
	}

	/// <inheritdoc />
	public override Task Deserialize(Type type, IRealSettings settings, string path) {
		var copyType = Constructor.GetType(type, SettingsKind.Copy);
		
		using var reader = File.OpenText(path);
		using var jsonReader = new JsonTextReader(reader);
		var copy = _serializer.Deserialize(jsonReader, copyType);
		settings.Copy((ICopySettings) copy);
		return Task.CompletedTask;
	}
	
	readonly JsonSerializer _serializer = JsonSerializer.Create(new() { Formatting = Formatting.Indented });
}

public class JsonNetFileSettingsProvider : FileSettingsProvider {
	public JsonNetFileSettingsProvider()
		: this(new SameAssemblySettingsTypeConstructor()) { }
	public JsonNetFileSettingsProvider(string location)
		: this(location, new SameAssemblySettingsTypeConstructor()) { }

	public JsonNetFileSettingsProvider(ISettingsTypeConstructor constructor)
		: base(new JsonNetFileSerializer(constructor), constructor) { }
	
	public JsonNetFileSettingsProvider(string location, ISettingsTypeConstructor constructor)
		: base(new JsonNetFileSerializer(constructor), constructor, location) { }

	public static JsonNetFileSettingsProvider Create(string app, string? company = null) 
		=> new(GetLocation(app, company));
	
	readonly JsonSerializer _serializer = JsonSerializer.Create(new() { Formatting = Formatting.Indented });
}