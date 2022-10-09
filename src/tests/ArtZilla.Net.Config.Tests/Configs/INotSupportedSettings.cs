using System.ComponentModel;

namespace ArtZilla.Net.Config.Tests;

[GenerateConfiguration]
interface INotSupportedSettings : INotifyPropertyChanged {
	int SomeRandomValue { get; set; }
}