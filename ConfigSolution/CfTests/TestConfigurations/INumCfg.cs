using System.ComponentModel;

namespace ArtZilla.Config.Tests.TestConfigurations {
	public interface INumCfg : IConfiguration {
		[DefaultValue(42)]
		int Value { get; set; }
	}
}
