namespace ArtZilla.Net.Config.Tests;

[GenerateConfiguration]
interface IInitByMethodSettings : ISettings {
	[DefaultValueByMethod(typeof(Init), nameof(Init.InitLines))]
	IList<string> Lines { get; }

	[DefaultValueByMethod(typeof(Init), nameof(Init.InitNumber))]
	int Number { get; set; }

	[DefaultValueByMethod(typeof(Init), nameof(Init.InitText), ": being meguka is suffering! Я★")]
	string Text { get; set; }

	[DefaultValueByMethod(typeof(Init), nameof(Init.IsOldInit), true)]
	bool IsOldInit { get; set; }
}