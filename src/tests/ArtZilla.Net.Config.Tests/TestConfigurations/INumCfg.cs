using System.ComponentModel;

namespace ArtZilla.Net.Config.Tests.TestConfigurations; 

public interface INumCfg : IConfiguration {
	[DefaultValue(42)]
	int Value { get; set; }
}