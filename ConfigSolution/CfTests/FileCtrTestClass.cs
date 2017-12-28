using System.Collections.Generic;
using ArtZilla.Config.Configurators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CfTests {
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
			AssertCfg.IsNotDefault(saver.GetReadonly<ITestConfiguration>());

			var loader = Create();

			// should be equal
			AssertCfg.AssertEqualProperties(saver.GetReadonly<ITestConfiguration>(), loader.GetReadonly<ITestConfiguration>());
		}
	}
}
