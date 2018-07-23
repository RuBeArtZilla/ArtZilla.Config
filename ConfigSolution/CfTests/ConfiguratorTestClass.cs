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

		[TestMethod]
		public void TypedTest() => RunAll(TypedExistTest);

		[TestMethod]
		public void KeysTest() => RunAll(KeysExistTest);

		[TestMethod]
		public void EnumPropertyTest() => RunAll(EnumPropertyTest);

		[TestMethod]
		public void ListsPropertyTest() => RunAll(ListsPropertyTest);

		[TestMethod]
		public void DatesPropertyTest() => RunAll(DatesPropertyTest);

		[TestMethod]
		public void DatesExPropertyTest() => RunAll(DatesExPropertyTest);

		protected virtual void KeysExistTest(T ctr) {
			var cfr = ctr.As<int, ITestConfiguration>();
			cfr.Reset(0);
			cfr.Reset(42);

			// Assert.IsTrue(cfr.IsExist(0), "Configuration should exist after reset");
			// Assert.IsTrue(cfr.IsExist(1), "Configuration should exist after reset");

			AssertCfg.IsDefault(cfr.Readonly(0));
			AssertCfg.IsDefault(cfr.Readonly(42));
		}

		protected virtual void TypedExistTest(T ctr) {
			var cfr = ctr.As<ITestConfiguration>();
			cfr.Reset();

			// Assert.IsTrue(cfr.IsExist(), "Configuration should exist after reset");

			AssertCfg.IsDefault(cfr.Readonly());
		}

		protected virtual void ResetTest(T ctr) {
			var x = ctr.As<ITestConfiguration>();
			// changing configuration
			var cfg = x.Realtime();
			cfg.Int32 = MagicNumber;
			cfg.String = MagicLine;

			// should not be in the default state
			AssertCfg.IsNotDefault(x.Readonly());

			// reseting configuration
			x.Reset();

			// checking that it in default state now
			AssertCfg.IsDefault(x.Readonly());
		}

		protected virtual void LoadTest(T ctr) {
			var cfg = ctr.Copy<ITestConfiguration>();
			cfg.Int32 = MagicNumber;
			cfg.String = MagicLine;
			ctr.Save(cfg);

			// should not be in the default state
			AssertCfg.IsNotDefault(ctr.Readonly<ITestConfiguration>());

			// should be equal
			AssertCfg.AssertEqualProperties(cfg, ctr.Readonly<ITestConfiguration>());
		}

		protected virtual void SaveTest(T ctr) {
			ctr.Reset<ITestConfiguration>();
			Assert.AreNotEqual(ctr.Readonly<ITestConfiguration>().Int32, MagicNumber, "Change test magic number");
			Assert.AreNotEqual(ctr.Readonly<ITestConfiguration>().String, MagicLine, "Change test magic line");

			var cfg = new TestConfiguration();
			cfg.Int32 = MagicNumber;
			cfg.String = MagicLine;
			ctr.Save<ITestConfiguration>(cfg);

			Assert.AreEqual(ctr.Readonly<ITestConfiguration>().Int32, MagicNumber);
			Assert.AreEqual(ctr.Readonly<ITestConfiguration>().String, MagicLine);
		}

		protected virtual void ComplexResetTest(T ctr) {
			// changing configuration
			var cfg = ctr.Copy<IComplexConfig>();

			cfg.ValueArray = ComplexConfig.MagicArray;
			cfg.ValueList = new List<int>(ComplexConfig.MagicArray);
			cfg.ValueDictionary = new Dictionary<int, string>(ComplexConfig.MagicDictionary);

			ctr.Save(cfg);

			// should not be in the default state
			AssertCfg.IsNotDefault(ctr.Readonly<IComplexConfig>());

			// reseting configuration
			ctr.Reset<ITestConfiguration>();

			// checking that it in default state now
			AssertCfg.IsDefault(ctr.Readonly<IComplexConfig>());
		}

		protected virtual void ComplexLoadTest(T ctr) {
			var cfg = ctr.Copy<IComplexConfig>();

			cfg.ValueArray = ComplexConfig.MagicArray;
			cfg.ValueList = new List<int>(ComplexConfig.MagicArray);
			cfg.ValueDictionary = new Dictionary<int, string>(ComplexConfig.MagicDictionary);

			ctr.Save(cfg);

			// should not be in the default state
			AssertCfg.IsNotDefault(ctr.Readonly<IComplexConfig>());

			// should be equal
			AssertCfg.AssertEqualProperties(cfg, ctr.Readonly<IComplexConfig>());
		}

		protected virtual void ComplexSaveTest(T ctr) {
			ctr.Reset<IComplexConfig>();
			Assert.IsFalse(AssertCfg.AreEquals(ctr.Readonly<IComplexConfig>().ValueArray, ComplexConfig.MagicArray),
				"Change test magic number");

			var cfg = new ComplexConfig {
				ValueArray = ComplexConfig.MagicArray,
				ValueList = new List<int>(ComplexConfig.MagicArray),
				ValueDictionary = new Dictionary<int, string>(ComplexConfig.MagicDictionary)
			};

			ctr.Save<IComplexConfig>(cfg);

			Assert.IsTrue(AssertCfg.AreEquals(ctr.Readonly<IComplexConfig>().ValueArray, ComplexConfig.MagicArray));
		}

		protected virtual void EnumPropertyTest(T ctr) {
			ctr.Reset<IConfigWithEnum>();

			var cfg = ctr.Realtime<IConfigWithEnum>();
			Assert.AreEqual(Girls.Homura, cfg.MyWaifu);
			Assert.AreEqual(Girls.Mami, cfg.Headless);
			Assert.AreEqual(new Guid("{D1F71EC6-76A6-40F8-8910-68E67D753CD4}"), cfg.SomeGuid);

			cfg.MyWaifu = Girls.Madoka;
			Assert.AreEqual(Girls.Madoka, ctr.Readonly<IConfigWithEnum>().MyWaifu);
		}

		protected virtual void ListsPropertyTest(T ctr) {
			var x = ctr.As<IListConfiguration>();
			x.Reset();

			Assert.IsNotNull(x.Copy().Heroes);
			Assert.IsNotNull(x.Realtime().Heroes);
			Assert.IsNotNull(x.Readonly().Heroes);
			Assert.IsNotNull(x.Notifying().Heroes);

			x.Realtime().Heroes.Add(new Hero("Midoria"));
			x.Realtime().Heroes.Add(new Hero("Saitama", 8999));
			x.Realtime().Heroes.Add(new Hero("Homura", 9001));

			Assert.AreEqual(3, x.Readonly().Heroes.Count);
			Assert.AreEqual(9001, x.Notifying().Heroes[2].Power);

			x.Reset();
			Assert.AreEqual(0, x.Readonly().Heroes.Count);
		}

		protected virtual void DatesPropertyTest(T ctr) {
			var today = DateTime.Today;
			var x = ctr.As<IDateConfig>();

			x.Realtime().Date = DateTime.Now;

			Assert.IsTrue(today < x.Readonly().Date);
		}

		protected virtual void DatesExPropertyTest(T ctr) {
			var x = ctr.As<IDateConfigEx>();
			x.Reset();

			Assert.IsTrue(x.Copy().CreatedAt > DateTime.Today); // there hidden bug =)
			x.Realtime().Date = DateTime.Now;

			Assert.IsTrue(x.Readonly().CreatedAt < x.Readonly().Date);
		}
	}
}
