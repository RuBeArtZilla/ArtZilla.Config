using System.ComponentModel;

namespace ArtZilla.Net.Config.Tests;

[GenerateConfiguration]
interface IInheritedSettings : ISimpleSettings {
	[DefaultValue(.442f)]
	[System.ComponentModel.Description("Some int value"), Browsable(false)]
	Single? Value { get; set; }
}