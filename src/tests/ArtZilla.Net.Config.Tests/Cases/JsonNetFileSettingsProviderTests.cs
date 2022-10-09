namespace ArtZilla.Net.Config.Tests;

[TestClass]
public class JsonNetFileSettingsProviderTests : FileSettingsProviderTests<JsonNetFileSettingsProvider> {
	/// <inheritdoc />
	protected override JsonNetFileSettingsProvider CreateUniqueProvider(string? name = null) 
		=> new(
			Path.Combine(
				Path.GetTempPath(),
				"tests",
				nameof(JsonNetFileSettingsProviderTests),
				name!,
				DateTime.Now.Ticks.ToString("D")
			)
		);
}