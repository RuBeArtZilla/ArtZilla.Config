using System;
using System.Linq;
using ArtZilla.Config;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CfTests {
	[TestClass]
	public class BaseTests {
		const Int32 NewInteger = 4;
		const Double NewDouble = 8;
		const String NewString = "15";

		[TestMethod]
		public void FirstTestMethod() {
			var cfr = ConfigManager.GetDefaultConfigurator();
			var cfg = cfr.GetCopy<ITestConfiguration>();
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
			var cfg = cfr.GetCopy<ITestConfiguration>();
			CheckIsDefault(cfg);
			ChangeConfig(cfg);
			CheckIsChanged(cfg);
		}

		[TestMethod]
		public void ReadOnlyConfigurationTest() {
			var cfr = ConfigManager.GetDefaultConfigurator();
			var cfg = cfr.GetReadOnly<ITestConfiguration>();
			Assert.ThrowsException<ReadOnlyException>(() => cfg.Int32 = 1);
		}

		[TestMethod]
		public void TestDefaultValues() {
			var cfr = ConfigManager.GetDefaultConfigurator();
			cfr.Reset<ITestConfiguration>();

			CheckIsDefault(cfr.GetCopy<ITestConfiguration>());
			CheckIsDefault(cfr.GetAuto<ITestConfiguration>());
			CheckIsDefault(cfr.GetAutoCopy<ITestConfiguration>());
			CheckIsDefault(cfr.GetReadOnly<ITestConfiguration>());
		}

		[TestMethod]
		public void TestCopyMethod() {
			var cfr = ConfigManager.GetDefaultConfigurator();
			var src = cfr.GetCopy<ITestConfiguration>();
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

	[TestClass]
	public class ReadOnlyTestCfg {
		[TestMethod]
		public void TestIsImplementReadOnly() {
			var cfg = CreateTestConfiguration();

			Assert.IsInstanceOfType(cfg, typeof(ITestConfiguration));
			Assert.IsInstanceOfType(cfg, typeof(IReadOnlyConfiguration));
			Assert.IsInstanceOfType(cfg, typeof(IReadOnlyConfiguration<ITestConfiguration>));
		}

		[TestMethod]
		public void TestIsReadOnly() {
			var cfg = CreateTestConfiguration();

			Assert.ThrowsException<ReadOnlyException>(() => cfg.Int32 = 1);
			Assert.ThrowsException<ReadOnlyException>(() => cfg.Int64 = cfg.Int64);
		}

		[TestMethod]
		public void TestDefaultValues()
			=> AssertCfg.IsDefault(CreateTestConfiguration());

		private static ITestConfiguration CreateTestConfiguration()
			=> new MemoryConfigurator().GetReadOnly<ITestConfiguration>();
	}

	[TestClass]
	public abstract class ConfiguratorTestClass<T> where T : IConfigurator {
		protected const int MagicNumber = 777;
		protected const string MagicLine = "Boku wa tomodachi ga sukunai";

		protected abstract T Create();

		[TestMethod]
		public void ResetTest() {
			var ctr = Create();

			// changing configuration
			var cfg = ctr.GetCopy<ITestConfiguration>();
			cfg.Int32 = MagicNumber;
			cfg.String = MagicLine;
			ctr.Save(cfg);

			// should be not in default state
			AssertCfg.IsNotDefault(ctr.GetReadOnly<ITestConfiguration>());

			// reseting configuration
			ctr.Reset<ITestConfiguration>();

			// checking that it in default state now
			AssertCfg.IsDefault(ctr.GetReadOnly<ITestConfiguration>());
		}

		[TestMethod]
		public void SaveTest() {
			var ctr = Create();

			ctr.Reset<ITestConfiguration>();
			Assert.AreNotEqual(ctr.GetReadOnly<ITestConfiguration>().Int32, MagicNumber, "Change test magic number");
			Assert.AreNotEqual(ctr.GetReadOnly<ITestConfiguration>().String, MagicLine, "Change test magic line");

			var cfg = new TestConfiguration();
			cfg.Int32 = MagicNumber;
			cfg.String = MagicLine;
			ctr.Save<ITestConfiguration>(cfg);

			Assert.AreEqual(ctr.GetReadOnly<ITestConfiguration>().Int32, MagicNumber);
			Assert.AreEqual(ctr.GetReadOnly<ITestConfiguration>().String, MagicLine);
		}
	}

	[TestClass]
	public class MemoryConfiguratorTestClass : ConfiguratorTestClass<MemoryConfigurator> {
		protected override MemoryConfigurator Create() => new MemoryConfigurator();
	}
}
