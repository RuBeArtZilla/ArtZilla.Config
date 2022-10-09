namespace ArtZilla.Net.Config.Tests;

[GenerateConfiguration]
interface IListSettings : ISettings {
	[DefaultValueByMethod(typeof(Init), nameof(Init.InitLines))]
	IConfigList<string> Lines { get; set; }
}