using System;
using System.Collections.Generic;
using ArtZilla.Config;
using ArtZilla.Config.Configurators;
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

		protected virtual IEnumerable<T> CreateAllVariants() {
			yield return Create();
		}

		protected virtual void RunAll(Action<T> testMethod) {
			foreach (var ctr in CreateAllVariants()) {
				Console.WriteLine("Testing " + ctr);
				testMethod(ctr);
			}
		}

		[TestMethod]
		public void SimpleResetTest() => RunAll(ResetTest);

		[TestMethod]
		public void SimpleLoadTest() => RunAll(LoadTest);

		[TestMethod]
		public void SimpleSaveTest() => RunAll(SaveTest);
		
		[TestMethod]
		public void ComplexResetTest() => RunAll(ResetTest);

		[TestMethod]
		public void ComplexLoadTest() => RunAll(LoadTest);

		[TestMethod]
		public void ComplexSaveTest() => RunAll(SaveTest);

		protected virtual void ResetTest(T ctr) {
			// changing configuration
			var cfg = ctr.GetCopy<ITestConfiguration>();
			cfg.Int32 = MagicNumber;
			cfg.String = MagicLine;
			ctr.Save(cfg);

			// should not be in the default state
			AssertCfg.IsNotDefault(ctr.GetReadOnly<ITestConfiguration>());

			// reseting configuration
			ctr.Reset<ITestConfiguration>();

			// checking that it in default state now
			AssertCfg.IsDefault(ctr.GetReadOnly<ITestConfiguration>());
		}

		protected virtual void LoadTest(T ctr) {
			var cfg = ctr.GetCopy<ITestConfiguration>();
			cfg.Int32 = MagicNumber;
			cfg.String = MagicLine;
			ctr.Save(cfg);

			// should not be in the default state
			AssertCfg.IsNotDefault(ctr.GetReadOnly<ITestConfiguration>());

			// should be equal
			AssertCfg.AssertEqualProperties(cfg, ctr.GetReadOnly<ITestConfiguration>());
		}

		protected virtual void SaveTest(T ctr) {
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

		protected virtual void ComplexResetTest(T ctr) {
			// changing configuration
			var cfg = ctr.GetCopy<IComplexConfig>();

			cfg.ValueArray = ComplexConfig.MagicArray;
			cfg.ValueList = new List<int>(ComplexConfig.MagicArray);
			cfg.ValueDictionary = new Dictionary<int, string>(ComplexConfig.MagicDictionary);

			ctr.Save(cfg);

			// should not be in the default state
			AssertCfg.IsNotDefault(ctr.GetReadOnly<IComplexConfig>());

			// reseting configuration
			ctr.Reset<ITestConfiguration>();

			// checking that it in default state now
			AssertCfg.IsDefault(ctr.GetReadOnly<IComplexConfig>());
		}

		protected virtual void ComplexLoadTest(T ctr) {
			var cfg = ctr.GetCopy<IComplexConfig>();

			cfg.ValueArray = ComplexConfig.MagicArray;
			cfg.ValueList = new List<int>(ComplexConfig.MagicArray);
			cfg.ValueDictionary = new Dictionary<int, string>(ComplexConfig.MagicDictionary);

			ctr.Save(cfg);

			// should not be in the default state
			AssertCfg.IsNotDefault(ctr.GetReadOnly<IComplexConfig>());

			// should be equal
			AssertCfg.AssertEqualProperties(cfg, ctr.GetReadOnly<IComplexConfig>());
		}

		protected virtual void ComplexSaveTest(T ctr) {
			ctr.Reset<IComplexConfig>();
			Assert.IsFalse(AssertCfg.AreEquals(ctr.GetReadOnly<IComplexConfig>().ValueArray, ComplexConfig.MagicArray),
				"Change test magic number");

			var cfg = new ComplexConfig();

			cfg.ValueArray = ComplexConfig.MagicArray;
			cfg.ValueList = new List<int>(ComplexConfig.MagicArray);
			cfg.ValueDictionary = new Dictionary<int, string>(ComplexConfig.MagicDictionary);

			ctr.Save<IComplexConfig>(cfg);

			Assert.IsTrue(AssertCfg.AreEquals(ctr.GetReadOnly<IComplexConfig>().ValueArray, ComplexConfig.MagicArray));
		}
	}

	[TestClass]
	public class MemoryCtrTestClass: ConfiguratorTestClass<MemoryConfigurator> {
		protected override MemoryConfigurator Create() => new MemoryConfigurator();
	}

	[TestClass]
	public class FileCtrTestClass: ConfiguratorTestClass<FileConfigurator> {
		protected override FileConfigurator Create() => new FileConfigurator();

		protected override IEnumerable<FileConfigurator> CreateAllVariants() {
			yield return Create();
			yield return new FileConfigurator { Company = null };
			yield return new FileConfigurator { AppName = null };
			yield return new FileConfigurator { Company = null, AppName = null };
			yield return new FileConfigurator { Company = "" };
			yield return new FileConfigurator { AppName = "" };
			yield return new FileConfigurator { Company = "", AppName = "" };
		}

		[TestMethod]
		public virtual void MultipleInstancesLoadTest() {
			var saver = Create();
			var cfg = saver.GetCopy<ITestConfiguration>();
			cfg.Int32 = MagicNumber;
			cfg.String = MagicLine;
			saver.Save(cfg);
			saver.Flush();

			// should not be in the default state
			AssertCfg.IsNotDefault(saver.GetReadOnly<ITestConfiguration>());

			var loader = Create();

			// should be equal
			AssertCfg.AssertEqualProperties(saver.GetReadOnly<ITestConfiguration>(), loader.GetReadOnly<ITestConfiguration>());
		}
	}
}
