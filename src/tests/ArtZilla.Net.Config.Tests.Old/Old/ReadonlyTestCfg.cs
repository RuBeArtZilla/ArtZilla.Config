﻿using ArtZilla.Net.Config.Configurators;
using ArtZilla.Net.Config.Tests.TestConfigurations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ArtZilla.Net.Config.Tests; 

[TestClass]
public class ReadonlyTestCfg {
	[TestMethod]
	public void TestIsImplementReadonly() {
		var cfg = CreateTestConfiguration();

		Assert.IsInstanceOfType(cfg, typeof(ITestConfiguration));
		Assert.IsInstanceOfType(cfg, typeof(IReadonlyConfiguration));
	}

	[TestMethod]
	public void TestIsReadonly() {
		var cfg = CreateTestConfiguration();

		Assert.ThrowsException<ReadonlyException>(() => cfg.Int32 = 1);
		Assert.ThrowsException<ReadonlyException>(() => cfg.Int64 = cfg.Int64);
	}

	[TestMethod]
	public void TestDefaultValues()
		=> AssertCfg.IsDefault(CreateTestConfiguration());

	private static ITestConfiguration CreateTestConfiguration()
		=> new MemoryConfigurator().Readonly<ITestConfiguration>();
}