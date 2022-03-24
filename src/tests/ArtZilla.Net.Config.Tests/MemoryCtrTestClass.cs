using ArtZilla.Net.Config.Configurators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ArtZilla.Net.Config.Tests; 

[TestClass]
public class MemoryCtrTestClass: ConfiguratorTestClass<MemoryConfigurator> {
	protected override MemoryConfigurator Create() => new MemoryConfigurator();
}