using System.ComponentModel;
using ArtZilla.Config;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CfTests {
	[TestClass]
	public class NotifyingTestCfg {
		[TestMethod]
		public void NotifyingConfigurationCreationTest() {
			var cfg = ConfigManager.Notifying<ITestConfiguration>();

			Assert.IsNotNull(cfg);
			Assert.IsInstanceOfType(cfg, typeof(ITestConfiguration));
			Assert.IsInstanceOfType(cfg, typeof(INotifyingConfiguration));
		}

		[TestMethod]
		public void NotifyingConfigurationEventTest() {
			var cfg = ConfigManager.Notifying<ITestConfiguration>();
			var auto = (INotifyingConfiguration)cfg;
			var changed = false;
			void Inpc_PropertyChanged(object sender, PropertyChangedEventArgs e) => changed = true;
			auto.PropertyChanged += Inpc_PropertyChanged;

			var old = cfg.Int32;
			cfg.Int32 = old;
			Assert.IsFalse(changed, nameof(INotifyPropertyChanged.PropertyChanged) + " invoked");

			cfg.Int32 = 172;
			Assert.IsTrue(changed, nameof(INotifyPropertyChanged.PropertyChanged) + " not invoked");
		}
	}
}
