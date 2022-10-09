#if NET60_OR_GREATER
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using System.Diagnostics.CodeAnalysis;
#endif

namespace ArtZilla.Net.Config; 



#if NET60_OR_GREATER

public sealed class JsonFileSerializer : FileSerializer {
	public bool EnumAsString {
		get => _enumAsString;
		set {
			if (_enumAsString == value)
				return;
			_enumAsString = value;
			if (value)
				Options.Converters.Add(EnumAsStringConverter);
			else
				Options.Converters.Remove(EnumAsStringConverter);
		}
	}

	bool _enumAsString;
	
	public JsonFileSerializer(ISettingsTypeConstructor constructor) : base(constructor) { }
	
	/// <inheritdoc />
	public override string GetFileExtension() => ".json";
	
	/// <inheritdoc />
	public override async Task Serialize(Type type, IRealSettings settings, string path) {
		await using var stream = File.OpenWrite(path);
		var copyType = Constructor.GetType(type, SettingsKind.Copy);
		var copy = Constructor.Create(type, SettingsKind.Copy, settings);
		await JsonSerializer.SerializeAsync(stream, copy, copyType, Options);
	}

	/// <inheritdoc />
	public override async Task Deserialize(Type type, IRealSettings settings, string path) {
		await using var stream = File.OpenRead(path);
		var copyType = Constructor.GetType(type, SettingsKind.Copy);
		if (await JsonSerializer.DeserializeAsync(stream, copyType, Options) is ISettings parseResult)
			settings.Copy(parseResult);
	}
	
	
	readonly JsonSerializerOptions Options = new() {
		WriteIndented = true,
		AllowTrailingCommas = true,
		DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
		Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic),
		PropertyNameCaseInsensitive = true,
	};

	static readonly JsonConverter EnumAsStringConverter = new JsonStringEnumConverter(JsonNamingPolicy.CamelCase);
}

public class JsonFileSettingsProvider : FileSettingsProvider {
	public JsonFileSettingsProvider()
		: this(new SameAssemblySettingsTypeConstructor()) { }
	public JsonFileSettingsProvider(string location)
		: this(location, new SameAssemblySettingsTypeConstructor()) { }

	public JsonFileSettingsProvider(ISettingsTypeConstructor constructor)
		: base(new JsonFileSerializer(constructor), constructor) { }
	
	public JsonFileSettingsProvider(string location, ISettingsTypeConstructor constructor)
		: base(new JsonFileSerializer(constructor), constructor, location) { }
	            
	public static JsonFileSettingsProvider Create(string app, string? company = null) 
		=> new(GetLocation(app, company));
}

#endif