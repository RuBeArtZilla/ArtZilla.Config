using System.Diagnostics;
using ArtZilla.Net.Core.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ArtZilla.Net.Config.Tests;

[TestClass]
public class ConfigListCases : Core {
	readonly IConfigurator _configurator;

	public ConfigListCases()
		=> _configurator = new JsonFileConfigurator(nameof(ConfigListCases), "AZ");


	[TestMethod]
	public void FirstTest() {
		_configurator.Reset<IStringsConfiguration>();
		var realtime = _configurator.Realtime<IStringsConfiguration>();
		var count = 0;
		realtime.Items.ListChanged += Items_ListChanged;

		Assert.AreEqual(2, realtime.Items.Count);
		Assert.AreEqual("Madoka", realtime.Items[1]);
		realtime.Items.Add("Sayaka");
		realtime.Items.Remove("Madoka");
		Assert.AreEqual(2, realtime.Items.Count);
		Assert.AreEqual(2, count);

		void Items_ListChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
			Debug.WriteLine($">>> List changed: {e.Action}| old:{e.OldItems}| new:{e.NewItems}");
			++count;
		}
	}
}