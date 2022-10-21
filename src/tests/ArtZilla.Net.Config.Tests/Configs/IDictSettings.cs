using System.Runtime.Serialization;

namespace ArtZilla.Net.Config.Tests;

[GenerateConfiguration]
interface IDictSettings : ISettings {
	[DefaultValueByMethod(typeof(Init), nameof(Init.InitMap))]
	ISettingsDict<Guid, ISimpleSettings> Map { get; }
}