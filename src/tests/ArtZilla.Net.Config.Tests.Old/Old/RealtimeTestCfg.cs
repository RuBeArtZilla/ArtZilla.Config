using System;
using System.Collections.Generic;
using System.ComponentModel;
using ArtZilla.Net.Config.Configurators;
using ArtZilla.Net.Config.Extensions;
using ArtZilla.Net.Config.Tests.TestConfigurations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ArtZilla.Net.Config.Tests; 

[TestClass]
public class RealtimeTestCfg {
	[TestMethod]
	public void RealtimeConfigurationCreationTest() {
		var cfg = ConfigManager.Realtime<ITestConfiguration>();

		Assert.IsNotNull(cfg);
		Assert.IsInstanceOfType(cfg, typeof(ITestConfiguration));
		Assert.IsInstanceOfType(cfg, typeof(IRealtimeConfiguration));
	}

	[TestMethod]
	public void RealtimeMemoryConfigurationEditTest() {
		var ctr = new MemoryConfigurator();
		ctr.Reset<ITestConfiguration>();

		var cfg = ctr.Realtime<ITestConfiguration>();
		AssertCfg.IsDefault(cfg);

		const string message = "Testing realtime";
		cfg.String = message;

		var other = ctr.Readonly<ITestConfiguration>();
		AssertCfg.IsNotDefault(cfg);
		AssertCfg.IsNotDefault(other);
		Assert.AreEqual(cfg.String, message, "realtime config not changed");
		Assert.AreEqual(other.String, message, "readonly config not actual");
	}

	[TestMethod]
	public void RealtimeFileConfigurationEditTest() {
		var appName = DateTimeOffset.Now.Ticks.ToString();
		var company = "ArtZilla.Config";

		FileConfigurator ctr1 = null;
		FileConfigurator ctr2 = null;
		try {
			ctr1 = new FileConfigurator(appName, company);
			ctr1.Reset<ITestConfiguration>();
			var cfg = ctr1.Realtime<ITestConfiguration>();
			AssertCfg.IsDefault(cfg);

			const string message = "Testing realtime";
			cfg.String = message;
			ctr1.Flush();

			ctr2 = new FileConfigurator(appName, company);
			var other = ctr2.Readonly<ITestConfiguration>();
			Assert.AreEqual(message, cfg.String, "realtime config not changed");
			Assert.AreEqual(message, other.String, "readonly config not actual");
		} finally {
			ctr1?.Reset<ITestConfiguration>();
			ctr2?.Reset<ITestConfiguration>();
		}
	}

	[TestMethod]
	public void ResetCallbackTest() {
		var changes = new List<string>();
		void PropertyChangedMethod(object sender, PropertyChangedEventArgs e) => changes.Add(e.PropertyName);

		var randomCtr = CreateRandom();
		var ctr = randomCtr.As<ITestConfiguration>();
		ctr.Reset();

		var cfg = ctr.Realtime();
		cfg.Int32 = 1234;

		cfg.AsRealtime().PropertyChanged += PropertyChangedMethod;
		Assert.IsTrue(changes.Count == 0);

		ctr.Reset();
		Assert.IsTrue(changes.Count == 1);
		Assert.IsTrue(changes.Contains(nameof(ITestConfiguration.Int32)));
	}

	private FileConfigurator CreateRandom() {
		var appName = Guid.NewGuid().ToString();
		var company = nameof(ArtZilla.Net.Config);
		return new FileConfigurator(appName, company);
	}
}