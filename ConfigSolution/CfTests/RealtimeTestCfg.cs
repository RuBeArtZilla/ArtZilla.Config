using System;
using ArtZilla.Config;
using ArtZilla.Config.Configurators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CfTests {
	[TestClass]
	public class RealtimeTestCfg {
		[TestMethod]
		public void RealtimeConfigurationCreationTest() {
			var cfg = ConfigManager.GetRealtime<ITestConfiguration>();

			Assert.IsNotNull(cfg);
			Assert.IsInstanceOfType(cfg, typeof(ITestConfiguration));
			Assert.IsInstanceOfType(cfg, typeof(IRealtimeConfiguration));
		}

		[TestMethod]
		public void RealtimeMemoryConfigurationEditTest() {
			var ctr = new MemoryConfigurator();
			ctr.Reset<ITestConfiguration>();

			var cfg = ctr.GetRealtime<ITestConfiguration>();
			AssertCfg.IsDefault(cfg);

			const string message = "Testing realtime";
			cfg.String = message;

			var other = ctr.GetReadonly<ITestConfiguration>();
			AssertCfg.IsNotDefault(cfg);
			AssertCfg.IsNotDefault(other);
			Assert.AreEqual(cfg.String, message, "realtime config not changed");
			Assert.AreEqual(other.String, message, "readonly config not actual");
		}

		[TestMethod]
		public void RealtimeFileConfigurationEditTest() {
			var appName = Guid.NewGuid().ToString();
			var company = nameof(ArtZilla.Config);
			var ctr1 = new FileConfigurator() { AppName = appName , Company = company };
			ctr1.Reset<ITestConfiguration>();

			var cfg = ctr1.GetRealtime<ITestConfiguration>();
			AssertCfg.IsDefault(cfg);

			const string message = "Testing realtime";
			cfg.String = message;
			ctr1.Flush();

			var ctr2 = new FileConfigurator() { AppName = appName, Company = company };
			var other = ctr2.GetReadonly<ITestConfiguration>();
			Assert.AreEqual(message, cfg.String, "realtime config not changed");
			Assert.AreEqual(message, other.String, "readonly config not actual");

			ctr1.Reset<ITestConfiguration>();
			ctr2.Reset<ITestConfiguration>();
		}
	}
}
