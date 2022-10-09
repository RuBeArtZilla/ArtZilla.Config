using System.ComponentModel;

namespace ArtZilla.Net.Config.Tests;

[GenerateConfiguration]
public interface ISimpleSettings : ISettings {
	[DefaultValue("Hello World!")]
	string Text { get; set; }
}