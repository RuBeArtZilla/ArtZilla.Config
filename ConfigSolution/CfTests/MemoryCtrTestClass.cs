using ArtZilla.Config;
using ArtZilla.Config.Configurators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CfTests {
	[TestClass]
	public class MemoryCtrTestClass: ConfiguratorTestClass<MemoryConfigurator> {
		protected override MemoryConfigurator Create() => new MemoryConfigurator();
	}
}
