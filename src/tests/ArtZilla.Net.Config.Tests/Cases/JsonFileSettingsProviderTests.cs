namespace ArtZilla.Net.Config.Tests;

[TestClass]
public class JsonFileSettingsProviderTests : FileSettingsProviderTests<JsonFileSettingsProvider> {
	/// <inheritdoc />
	protected override JsonFileSettingsProvider CreateUniqueProvider(string? name = null)
		=> new(
			Path.Combine(
				Path.GetTempPath(),
				"tests",
				nameof(JsonFileSettingsProvider),
				name!,
				DateTime.Now.Ticks.ToString("D")
			)
		);
}