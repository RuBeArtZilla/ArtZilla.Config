using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using ArtZilla.Net.Core.Extensions;
using Guard = CommunityToolkit.Diagnostics.Guard;
#if NET60_OR_GREATER
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using System.Diagnostics.CodeAnalysis;
#endif

namespace ArtZilla.Net.Config; 

#if NET60_OR_GREATER
public class JsonFileSettingsProvider : FileSettingsProvider {
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
	
	public JsonFileSettingsProvider() { }

	public JsonFileSettingsProvider(string location)
		: base(location) { }

	public JsonFileSettingsProvider(string location, ISettingsTypeConstructor constructor)
		: base(location, constructor) { }

	public static JsonFileSettingsProvider Create(string app, string? company = null) {
		Guard.IsNotNull(app);
		var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
		var location = company.IsNotNullOrWhiteSpace()
			? Path.Combine(localAppData, company!, app, "settings")
			: Path.Combine(localAppData, app, "settings");
		return new(location);
	}

	protected override string GetFileExtension() => ".json";

	/// <inheritdoc />
	protected override async Task Serialize(Type type, IRealSettings settings, string path) {
		await using var stream = File.OpenWrite(path);
		var copyType = Constructor.GetType(type, SettingsKind.Copy);
		var copy = Constructor.Create(type, SettingsKind.Copy, settings);
		await JsonSerializer.SerializeAsync(stream, copy, copyType, Options);
	}

	/// <inheritdoc />
	protected override async Task Deserialize(Type type, IRealSettings settings, string path) {
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
#endif