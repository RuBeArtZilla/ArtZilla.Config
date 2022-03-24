using ArtZilla.Net.Config.Configurators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ArtZilla.Net.Config.Tests;

[TestClass]
public class JsonFileConfiguratorTests : FileCtrTestClass {
	protected override FileConfigurator Create() 
		=> new FileConfigurator(new JsonFileThread(nameof(JsonFileConfiguratorTests), "AZ"));
}