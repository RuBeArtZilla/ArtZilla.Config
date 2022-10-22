#if NET60_OR_GREATER
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using System.Diagnostics.CodeAnalysis;
#endif

namespace ArtZilla.Net.Config;

#if NET60_OR_GREATER

/// <inheritdoc />
public sealed class JsonFileSerializer : FileSerializer {
	// ReSharper disable once UnusedMember.Global
	///
	public bool EnumAsString {
		get => _enumAsString;
		set {
			if (_enumAsString == value)
				return;
			_enumAsString = value;
			if (value)
				_options.Converters.Add(EnumAsStringConverter);
			else
				_options.Converters.Remove(EnumAsStringConverter);
		}
	}

	bool _enumAsString;
	readonly JsonSerializerOptions _options = new() {
		WriteIndented = true,
		AllowTrailingCommas = true,
		DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
		Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic),
		PropertyNameCaseInsensitive = true,
	};

	/// <inheritdoc />
	public JsonFileSerializer(ISettingsTypeConstructor constructor) : base(constructor) { }

	/// <inheritdoc />
	public override string GetFileExtension() => ".json";

	/// <inheritdoc />
	public override void Serialize(Type type, string path, ISettings settings) {
		var copyType = Constructor.GetType(type, SettingsKind.Copy);
		var copy = settings.GetType() == copyType
			? settings
			: Constructor.CloneCopy(settings);

		using var stream = File.Create(path);
		JsonSerializer.Serialize(stream, copy, copyType, _options);
	}

	/// <inheritdoc />
	public override ISettings Deserialize(Type type, string path) {
		using var stream = File.OpenRead(path);
		var copyType = Constructor.GetType(type, SettingsKind.Copy);
		if (JsonSerializer.Deserialize(stream, copyType, _options) is ISettings parseResult)
			return parseResult;
		throw new($"Can't deserialize {type} from {path}");
	}

	static readonly JsonConverter EnumAsStringConverter = new JsonStringEnumConverter(JsonNamingPolicy.CamelCase);
}

///
public class JsonFileSettingsProvider : FileSettingsProvider {
	/// <inheritdoc />
	public JsonFileSettingsProvider()
		: this(new SameAssemblySettingsTypeConstructor()) { }

	/// <inheritdoc />
	public JsonFileSettingsProvider(string location)
		: this(location, new SameAssemblySettingsTypeConstructor()) { }

	/// <inheritdoc />
	public JsonFileSettingsProvider(ISettingsTypeConstructor constructor)
		: base(constructor, new JsonFileSerializer(constructor)) { }

	/// <inheritdoc />
	public JsonFileSettingsProvider(string location, ISettingsTypeConstructor constructor)
		: base(constructor, new JsonFileSerializer(constructor), location) { }

	///
	/// <param name="app"></param>
	/// <param name="company"></param>
	/// <returns></returns>
	public static JsonFileSettingsProvider Create(string app, string? company = null)
		=> new(GetLocation(app, company));
}

#endif