namespace ArtZilla.Net.Config.Tests;

[GenerateConfiguration]
interface INotFoundMethodName : ISettings {
	[DefaultValueByMethod(typeof(Init), "ThisMethodShouldNotExist")]
	int X { get; set; }
}