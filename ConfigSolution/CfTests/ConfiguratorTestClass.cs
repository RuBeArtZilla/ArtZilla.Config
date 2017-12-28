using System;
using System.Collections.Generic;
using ArtZilla.Config;
using ArtZilla.Config.Tests.TestConfigurations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CfTests {
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
			AssertCfg.IsNotDefault(ctr.GetReadonly<ITestConfiguration>());

			// reseting configuration
			ctr.Reset<ITestConfiguration>();

			// checking that it in default state now
			AssertCfg.IsDefault(ctr.GetReadonly<ITestConfiguration>());
		}

		protected virtual void LoadTest(T ctr) {
			var cfg = ctr.GetCopy<ITestConfiguration>();
			cfg.Int32 = MagicNumber;
			cfg.String = MagicLine;
			ctr.Save(cfg);

			// should not be in the default state
			AssertCfg.IsNotDefault(ctr.GetReadonly<ITestConfiguration>());

			// should be equal
			AssertCfg.AssertEqualProperties(cfg, ctr.GetReadonly<ITestConfiguration>());
		}

		protected virtual void SaveTest(T ctr) {
			ctr.Reset<ITestConfiguration>();
			Assert.AreNotEqual(ctr.GetReadonly<ITestConfiguration>().Int32, MagicNumber, "Change test magic number");
			Assert.AreNotEqual(ctr.GetReadonly<ITestConfiguration>().String, MagicLine, "Change test magic line");

			var cfg = new TestConfiguration();
			cfg.Int32 = MagicNumber;
			cfg.String = MagicLine;
			ctr.Save<ITestConfiguration>(cfg);

			Assert.AreEqual(ctr.GetReadonly<ITestConfiguration>().Int32, MagicNumber);
			Assert.AreEqual(ctr.GetReadonly<ITestConfiguration>().String, MagicLine);
		}

		protected virtual void ComplexResetTest(T ctr) {
			// changing configuration
			var cfg = ctr.GetCopy<IComplexConfig>();

			cfg.ValueArray = ComplexConfig.MagicArray;
			cfg.ValueList = new List<int>(ComplexConfig.MagicArray);
			cfg.ValueDictionary = new Dictionary<int, string>(ComplexConfig.MagicDictionary);

			ctr.Save(cfg);

			// should not be in the default state
			AssertCfg.IsNotDefault(ctr.GetReadonly<IComplexConfig>());

			// reseting configuration
			ctr.Reset<ITestConfiguration>();

			// checking that it in default state now
			AssertCfg.IsDefault(ctr.GetReadonly<IComplexConfig>());
		}

		protected virtual void ComplexLoadTest(T ctr) {
			var cfg = ctr.GetCopy<IComplexConfig>();

			cfg.ValueArray = ComplexConfig.MagicArray;
			cfg.ValueList = new List<int>(ComplexConfig.MagicArray);
			cfg.ValueDictionary = new Dictionary<int, string>(ComplexConfig.MagicDictionary);

			ctr.Save(cfg);

			// should not be in the default state
			AssertCfg.IsNotDefault(ctr.GetReadonly<IComplexConfig>());

			// should be equal
			AssertCfg.AssertEqualProperties(cfg, ctr.GetReadonly<IComplexConfig>());
		}

		protected virtual void ComplexSaveTest(T ctr) {
			ctr.Reset<IComplexConfig>();
			Assert.IsFalse(AssertCfg.AreEquals(ctr.GetReadonly<IComplexConfig>().ValueArray, ComplexConfig.MagicArray),
				"Change test magic number");

			var cfg = new ComplexConfig();

			cfg.ValueArray = ComplexConfig.MagicArray;
			cfg.ValueList = new List<int>(ComplexConfig.MagicArray);
			cfg.ValueDictionary = new Dictionary<int, string>(ComplexConfig.MagicDictionary);

			ctr.Save<IComplexConfig>(cfg);

			Assert.IsTrue(AssertCfg.AreEquals(ctr.GetReadonly<IComplexConfig>().ValueArray, ComplexConfig.MagicArray));
		}
	}
}
