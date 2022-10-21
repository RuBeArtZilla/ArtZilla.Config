using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace ArtZilla.Net.Config.Tests;

public abstract class SettingsProviderTest<T> : Core where T : class, ISettingsProvider, new() {
	protected virtual T CreateUniqueProvider([CallerMemberName] string? name = null) => new();

	[TestMethod, Timeout(DefaultTimeout)]
	public void _IsTypeSupportedTest() {
		var provider = CreateUniqueProvider();

		provider.ThrowIfNotSupported<ISimpleSettings>();
		Assert.AreEqual(new SimpleSettings_Read(true).ToString(), provider.ReadSimpleSettings().ToString());
		provider.ThrowIfNotSupported<IInheritedSettings>();
		Assert.AreEqual(new InheritedSettings_Read(true).ToString(), provider.ReadInheritedSettings().ToString());
		provider.ThrowIfNotSupported<IInitByMethodSettings>();
		Assert.AreEqual(new InitByMethodSettings_Read(true).ToString(), provider.ReadInitByMethodSettings().ToString());
		provider.ThrowIfNotSupported<IListSettings>();
		Assert.AreEqual(new ListSettings_Read(true).ToString(), provider.ReadListSettings().ToString());

		provider.TryDispose();
	}

	[TestMethod, Timeout(DefaultTimeout)]
	public void IsExistTest() {
		var provider = CreateUniqueProvider();
		Assert.IsFalse(provider.IsExist<ISimpleSettings>());

		var settings = provider.RealSimpleSettings();
		settings.Text = "nothing";
		settings.Flush();
		Assert.IsTrue(settings.IsExist());

		settings.Delete();
		Assert.IsFalse(settings.IsExist());
		
		provider.TryDispose();
	}

	[TestMethod, Timeout(DefaultTimeout)]
	public async Task IsExistAsyncTest() {
		var provider = CreateUniqueProvider();
		Assert.IsFalse(await provider.IsExistAsync<ISimpleSettings>());

		var settings = await provider.RealSimpleSettingsAsync();
		settings.Text = "nothing";
		await settings.FlushAsync();
		Assert.IsTrue(await settings.IsExistAsync());

		await settings.DeleteAsync();
		Assert.IsFalse(await settings.IsExistAsync());
		
		provider.TryDispose();
	}

	[TestMethod, Timeout(DefaultTimeout)]
	public void ReadTest() {
		var provider = CreateUniqueProvider();
		var settings = provider.ReadSimpleSettings();
		Assert.ThrowsException<Exception>(() => settings.Text = "readonly property");
		
		provider.TryDispose();
	}

	[TestMethod, Timeout(DefaultTimeout)]
	public async Task InpcTest() {
		const int expectedChangeCount = 3;
		var provider = CreateUniqueProvider();
		var settings = await provider.InpcSimpleSettingsAsync();
		using var changed = new ChangesList(settings);
		settings.Text = "1";
		settings.Text = "12";
		settings.Text = "42";
		await settings.ResetAsync();

		SpinWait.SpinUntil(() => changed.Count == expectedChangeCount, TimeSpan.FromSeconds(5D));
		Assert.AreEqual(expectedChangeCount, changed.Count);
		Assert.IsTrue(changed.All(i => i == nameof(ISimpleSettings.Text)));
		
		provider.TryDispose();
	}

	[TestMethod, Timeout(DefaultTimeout)]
	public async Task InpcListTest() {
		const int expectedChangeCount = 3;
		var provider = CreateUniqueProvider();
		var settings = await provider.InpcListSettingsAsync();
		using var changed = new ChangesList(settings);
		settings.Lines.Add("1");
		settings.Lines.Add("12");
		settings.Lines.Add("42");
		await settings.ResetAsync();

		SpinWait.SpinUntil(() => changed.Count == expectedChangeCount, TimeSpan.FromSeconds(5D));
		Assert.AreEqual(expectedChangeCount, changed.Count);
		Assert.IsTrue(changed.All(i => i == nameof(IListSettings.Lines)));
		
		provider.TryDispose();
	}

	[TestMethod, Timeout(DefaultTimeout)]
	public async Task InpcRealTest() {
		const int expectedChangeCount = 4;
		var provider = CreateUniqueProvider();
		var settings = await provider.RealSimpleSettingsAsync();
		using var changed = new ChangesList(settings);

		settings.Text = "1";
		settings.Text = "12";
		settings.Text = "42";
		await settings.ResetAsync();

		SpinWait.SpinUntil(() => changed.Count == expectedChangeCount, TimeSpan.FromSeconds(5D));
		Assert.AreEqual(expectedChangeCount, changed.Count);
		Assert.IsTrue(changed.All(i => i == nameof(ISimpleSettings.Text)));
		
		provider.TryDispose();
	}

	[TestMethod, Timeout(DefaultTimeout)]
	public async Task InpcRealListTest() {
		const int expectedChangeCount = 3;
		var provider = CreateUniqueProvider();
		var settings = await provider.RealListSettingsAsync();
		using var changed = new ChangesList(settings);
		settings.Lines.Add("1");
		settings.Lines.Add("12");
		settings.Lines.Add("42");

		SpinWait.SpinUntil(() => changed.Count == expectedChangeCount, TimeSpan.FromSeconds(5D));
		Assert.AreEqual(expectedChangeCount, changed.Count);
		Assert.IsTrue(changed.All(i => i == nameof(IListSettings.Lines)));

		await settings.ResetAsync();
		Assert.IsTrue(changed.Count > expectedChangeCount);
		
		provider.TryDispose();
	}

	[TestMethod, Timeout(DefaultTimeout)]
	public async Task SetTest() {
		var provider = CreateUniqueProvider();
		var settings = await provider.CopySimpleSettingsAsync();
		settings.Text = LongText;
		await provider.SetAsync(settings);

		var actual = await provider.ReadSimpleSettingsAsync();
		Assert.AreEqual(LongText, actual.Text);
		
		provider.TryDispose();
	}

	[TestMethod, Timeout(DefaultTimeout)]
	public async Task ByKeyTest() {
		var provider = CreateUniqueProvider();
		var s1 = (ISimpleSettings) await provider.GetAsync(typeof(ISimpleSettings), SettingsKind.Real, "Madoka");
		var s2 = (ISimpleSettings) await provider.GetAsync(typeof(ISimpleSettings), SettingsKind.Real, "Homura");
		var s3 = await provider.RealSimpleSettingsAsync();
		
		Assert.AreEqual(s1.Text, s2.Text);

		s1.Text = "Kaname";
		s2.Text = "Akemi";
		s3.Text = "Sayaka Miki";
		
		await provider.FlushAsync();
		
		Assert.AreNotEqual(s1.Text, s2.Text);
		Assert.AreNotEqual(s1.Text, s3.Text);
		Assert.AreNotEqual(s2.Text, s3.Text);

		if (provider is FileSettingsProvider fsp) {
			var path1 = fsp.GetPathToSettings(s1.GetInterfaceType(), s1.SourceKey);
			var path2 = fsp.GetPathToSettings(s2.GetInterfaceType(), s2.SourceKey);
			var path3 = fsp.GetPathToSettings(s3.GetInterfaceType(), s3.SourceKey);
			Debug.Print(path1);
			Debug.Print(path2);
			Debug.Print(path3);
			Assert.AreNotEqual(path1, path2);
			Assert.AreNotEqual(path1, path3);
			Assert.AreNotEqual(path2, path3);
			
			var file1 = await File.ReadAllTextAsync(path1);
			var file2 = await File.ReadAllTextAsync(path2);
			var file3 = await File.ReadAllTextAsync(path3);
			Debug.Print(file1);
			Debug.Print(file2);
			Debug.Print(file3);
			Assert.AreNotEqual(file1, file2);
			Assert.AreNotEqual(file1, file3);
			Assert.AreNotEqual(file2, file3);
		}
		
		provider.TryDispose();
	}

	[TestMethod, Timeout(DefaultTimeout)]
	public async Task DictTest() {
		var provider = CreateUniqueProvider();
		var settings = await provider.RealDictSettingsAsync();
		var guid = Guid.NewGuid();
		var item = settings.Map.AddNew(guid);
		item.Text = LongText;
		await provider.FlushAsync();

		var read = await provider.ReadDictSettingsAsync();
		Assert.AreEqual(3, read.Map.Count);

		provider.TryDispose();
	}
}