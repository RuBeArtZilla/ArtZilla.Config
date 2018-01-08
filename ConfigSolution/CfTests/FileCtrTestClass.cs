using System;
using System.Collections.Generic;
using ArtZilla.Config.Configurators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CfTests {
	[TestClass]
	public class FileCtrTestClass: ConfiguratorTestClass<FileConfigurator> {
		protected override FileConfigurator Create() => new FileConfigurator(nameof(FileCtrTestClass));

		protected override IEnumerable<FileConfigurator> CreateAllVariants() {
			yield return Create();
			yield return new FileConfigurator(appName: null);
			yield return new FileConfigurator(appName: null, companyName: null);
			yield return new FileConfigurator(appName: "");
			yield return new FileConfigurator(appName: "", companyName: "");
		}

		[TestMethod]
		public virtual void MultipleInstancesLoadTest() {
			var saver = Create();
			var x = saver.As<ITestConfiguration>();

			Console.WriteLine("Clear settings.");
			x.Reset();

			// should be in the default state
			AssertCfg.IsDefault(x.Readonly());

			Console.WriteLine("Changing settings");
			var cfg = x.Realtime();
			cfg.Int32 = MagicNumber;
			cfg.String = MagicLine;

			Console.WriteLine("Waiting file write");
			saver.Flush();

			// should not be in the default state
			AssertCfg.IsNotDefault(x.Readonly());

			Console.WriteLine("Read and compare");

			// should be equal
			AssertCfg.AssertEqualProperties(x.Readonly(), Create().Readonly<ITestConfiguration>());
		}

		[TestMethod]
		public virtual void UsingKeyTest() {
			const int key = 42;

			var s = Create();
			var x = s.As<ITestConfiguration>().As<int>();
			x.Reset(key);
			AssertCfg.IsDefault(x.Readonly(key));

			var c = x.Realtime(key);
			c.String = MagicLine;
			s.Flush();

			var s2 = Create();
			var y = s2.As<int, ITestConfiguration>();
			Assert.IsTrue(y.IsExist(key));
			AssertCfg.IsNotDefault(y.Readonly(key));
			Assert.AreEqual(c.String, y.Readonly(key).String);

			y.Reset(key);
			var s3 = Create();
			var z = s3.As<int, ITestConfiguration>();
			Assert.IsFalse(z.IsExist(key));
		}
	}
}
