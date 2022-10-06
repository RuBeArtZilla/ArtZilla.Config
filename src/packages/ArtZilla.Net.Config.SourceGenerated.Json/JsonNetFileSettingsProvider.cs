using ArtZilla.Net.Core.Extensions;
using Newtonsoft.Json;
using Guard = CommunityToolkit.Diagnostics.Guard;

namespace ArtZilla.Net.Config; 

public class JsonNetFileSettingsProvider : FileSettingsProvider {
	public JsonNetFileSettingsProvider() { }

	public JsonNetFileSettingsProvider(string location)
		: base(location) { }

	public JsonNetFileSettingsProvider(string location, ISettingsTypeConstructor constructor)
		: base(location, constructor) { }

	public static JsonNetFileSettingsProvider Create(string app, string? company = null) {
		Guard.IsNotNull(app);
		var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
		var location = company.IsNotNullOrWhiteSpace()
			? Path.Combine(localAppData, company!, app, "settings")
			: Path.Combine(localAppData, app, "settings");
		return new(location);
	}
	
	/// <inheritdoc />
	protected override Task Serialize(Type type, IRealSettings settings, string path) {
		var copyType = Constructor.GetType(type, SettingsKind.Copy);
		var copy = Constructor.Create(type, SettingsKind.Copy, settings);
		
		using var writer = File.CreateText(path);
		using var jsonWriter = new JsonTextWriter(writer);
		_serializer.Serialize(jsonWriter, copy, copyType);
		return Task.CompletedTask;
	}

	/// <inheritdoc />
	protected override Task Deserialize(Type type, IRealSettings settings, string path) {
		var copyType = Constructor.GetType(type, SettingsKind.Copy);
		
		using var reader = File.OpenText(path);
		using var jsonReader = new JsonTextReader(reader);
		var copy = _serializer.Deserialize(jsonReader, copyType);
		settings.Copy((ICopySettings) copy);
		return Task.CompletedTask;
	}

	readonly JsonSerializer _serializer = JsonSerializer.Create(new() { Formatting = Formatting.Indented });
}