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
			saver.Reset<ITestConfiguration>();
			var cfg = saver.Copy<ITestConfiguration>();
			cfg.Int32 = MagicNumber;
			cfg.String = MagicLine;
			saver.Save(cfg);
			saver.Flush();

			// should not be in the default state
			AssertCfg.IsNotDefault(saver.Readonly<ITestConfiguration>());

			var loader = Create();

			// should be equal
			AssertCfg.AssertEqualProperties(saver.Readonly<ITestConfiguration>(), loader.Readonly<ITestConfiguration>());
		}
	}
}
