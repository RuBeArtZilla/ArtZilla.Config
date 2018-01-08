using ArtZilla.Config;
using ArtZilla.Config.Tests.TestConfigurations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CfTests {
	[TestClass]
	public class BaseTests {
		const int NewInteger = 4;
		const double NewDouble = 8;
		const string NewString = "15";

		[TestMethod]
		public void FirstTestMethod() {
			var cfr = ConfigManager.GetDefaultConfigurator();
			var cfg = cfr.Copy<ITestConfiguration>();
			CheckIsDefault(cfg);
			ChangeConfig(cfg);
			cfr.Save(cfg);
			CheckIsChanged(cfg);
			cfr.Reset<ITestConfiguration>();

			cfg.Int32 = 24;
			cfg.Double = 2.4D;
			cfg.String = "NewTestString";
		}

		[TestMethod]
		public void CreateConfigurationTest() {
			var cfr = ConfigManager.GetDefaultConfigurator();
			cfr.Reset<ITestConfiguration>();
			var cfg = cfr.Copy<ITestConfiguration>();
			CheckIsDefault(cfg);
			ChangeConfig(cfg);
			CheckIsChanged(cfg);
		}

		[TestMethod]
		public void ReadOnlyConfigurationTest() {
			var cfr = ConfigManager.GetDefaultConfigurator();
			var cfg = cfr.Readonly<ITestConfiguration>();
			Assert.ThrowsException<ReadonlyException>(() => cfg.Int32 = 1);
		}

		[TestMethod]
		public void TestDefaultValues() {
			var cfr = ConfigManager.GetDefaultConfigurator();
			cfr.Reset<ITestConfiguration>();

			CheckIsDefault(cfr.Copy<ITestConfiguration>());
			CheckIsDefault(cfr.Notifying<ITestConfiguration>());
			CheckIsDefault(cfr.Realtime<ITestConfiguration>());
			CheckIsDefault(cfr.Readonly<ITestConfiguration>());
		}

		[TestMethod]
		public void TestKeyConfig() {
			var cfr = ConfigManager.GetDefaultConfigurator();
			


		}

		[TestMethod]
		public void TestCopyMethod() {
			var cfr = ConfigManager.GetDefaultConfigurator();
			var src = cfr.Copy<ITestConfiguration>();
			var dst = new TestConfiguration();
			ChangeConfig(src);
			if (AssertCfg.AreEqualAllProperties(dst, src))
				Assert.Fail();

			dst.Copy(src);
			AssertCfg.AssertEqualProperties(src, dst);
		}

		public static void CheckIsDefault(ITestConfiguration cfg)
			=> CheckEquals(cfg, new TestConfiguration());

		public static void CheckEquals(ITestConfiguration x, ITestConfiguration y) {
			foreach (var p in typeof(ITestConfiguration).GetProperties())
				Assert.AreEqual(p.GetValue(y), p.GetValue(x), "Property " + p.Name);
		}

		public static void CheckNotEquals(ITestConfiguration x, ITestConfiguration y) {
			foreach (var p in typeof(ITestConfiguration).GetProperties())
				Assert.AreEqual(p.GetValue(y), p.GetValue(x), "Property " + p.Name);
		}

		void ChangeConfig(ITestConfiguration cfg) {
			cfg.Int32 = NewInteger;
			cfg.Double = NewDouble;
			cfg.String = NewString;
		}

		void CheckIsChanged(ITestConfiguration cfg) {
			Assert.AreEqual(NewInteger, cfg.Int32);
			Assert.AreEqual(NewDouble, cfg.Double);
			Assert.AreEqual(NewString, cfg.String);
		}
	}
}
