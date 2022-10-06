using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using ArtZilla.Net.Config.Tests.Generators;

namespace ArtZilla.Net.Config.Tests;

public abstract class SettingsProviderTest<T> : Core where T : ISettingsProvider, new() {
	sealed class ChangesList : List<string>, IDisposable {
		public ChangesList(IInpcSettings settings) {
			_settings = settings;
			Subscribe();
		}

		~ChangesList()
			=> ReleaseUnmanagedResources();

		public void Subscribe()
			=> _settings.Subscribe(OnPropertyChanged);

		public void Unsubscribe()
			=> _settings.Unsubscribe(OnPropertyChanged);

		void OnPropertyChanged(object? sender, PropertyChangedEventArgs args)
			=> this.Add(args.PropertyName);

		readonly IInpcSettings _settings;

		void ReleaseUnmanagedResources() {
			// release unmanaged resources here
		}

		/// <inheritdoc />
		public void Dispose() {
			ReleaseUnmanagedResources();
			GC.SuppressFinalize(this);
			Unsubscribe();
		}
	}

	protected virtual T CreateUniqueProvider([CallerMemberName] string? name = null) => new();

	[TestMethod, Timeout(DefaultTimeout)]
	public void _IsTypeSupportedTest() {
		var provider = CreateUniqueProvider();
		
		provider.ThrowIfNotSupported<ISimpleSettings>();
		Assert.AreEqual(new SimpleSettings_Read().ToString(), provider.ReadSimpleSettings().ToString());
		provider.ThrowIfNotSupported<IInheritedSettings>();
		Assert.AreEqual(new InheritedSettings_Read().ToString(), provider.ReadInheritedSettings().ToString());
		provider.ThrowIfNotSupported<IInitByMethodSettings>();
		Assert.AreEqual(new InitByMethodSettings_Read().ToString(), provider.ReadInitByMethodSettings().ToString());
		provider.ThrowIfNotSupported<IListSettings>();
		Assert.AreEqual(new ListSettings_Read().ToString(), provider.ReadListSettings().ToString());
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
	}

	[TestMethod, Timeout(DefaultTimeout)]
	public void ReadTest() {
		var provider = CreateUniqueProvider();
		var settings = provider.ReadSimpleSettings();
		Assert.ThrowsException<Exception>(() => settings.Text = "readonly property");
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
	}

	[TestMethod, Timeout(DefaultTimeout)]
	public async Task SetTest() {
		var provider = CreateUniqueProvider();
		var settings = await provider.CopySimpleSettingsAsync();
		settings.Text = LongText;
		await provider.SetAsync(settings);

		var actual = await provider.ReadSimpleSettingsAsync();
		Assert.AreEqual(LongText, actual.Text);
	}
}

public abstract class FileSettingsProviderTests<T> : SettingsProviderTest<T> where T : FileSettingsProvider, new() {
	[TestMethod, Timeout(DefaultTimeout), DoNotParallelize]
	public async Task SettingsFormatTest() {
		using var provider = CreateUniqueProvider();
		var s1 = await provider.RealSimpleSettingsAsync();
		s1.Text = LongText;
		var s2 = await provider.RealInheritedSettingsAsync();
		s2.Value = 3.14f;
		var s3 = await provider.RealListSettingsAsync();
		s3.Lines.Insert(1, LongText);
		var s4 = await provider.RealInitByMethodSettingsAsync();
		s4.Lines.Insert(1, LongText);
		await provider.FlushAsync();
		
		Debug.Print("Simple: {0}", await File.ReadAllTextAsync(provider.GetPathToSettings(s1.GetInterfaceType())));
		Debug.Print("Inherited: {0}", await File.ReadAllTextAsync(provider.GetPathToSettings(s2.GetInterfaceType())));
		Debug.Print("List: {0}", await File.ReadAllTextAsync(provider.GetPathToSettings(s3.GetInterfaceType())));
		Debug.Print("Init: {0}", await File.ReadAllTextAsync(provider.GetPathToSettings(s4.GetInterfaceType())));
	}

	[TestMethod, Timeout(DefaultTimeout), DoNotParallelize]
	public async Task SameLocationReadTest() {
		using var provider = CreateUniqueProvider();
		var expected = await provider.RealSimpleSettingsAsync();
		expected.Text = LongText;
		await expected.FlushAsync();

		using var provider2 = (T)Activator.CreateInstance(typeof(T), provider.Location)!;
		var actual = await provider2.ReadSimpleSettingsAsync();
		Assert.AreEqual(LongText, actual.Text);
	}

	[TestMethod, Timeout(DefaultTimeout), DoNotParallelize]
	public async Task SameLocationSyncTest() {
		using var provider = CreateUniqueProvider();
		using var provider2 = (T)Activator.CreateInstance(typeof(T), provider.Location)!;
		var expected = await provider.RealSimpleSettingsAsync();
		var actual = await provider2.RealSimpleSettingsAsync();
		expected.Text = LongText;
		await expected.FlushAsync();

		SpinWait.SpinUntil(() => actual.Text == LongText, DefaultTimeout);
		Assert.AreEqual(LongText, actual.Text);
	}

	[TestMethod, Timeout(DefaultTimeout), DoNotParallelize]
	public async Task SameLocationListSyncTest() {
		using var provider = CreateUniqueProvider();
		using var provider2 = (T)Activator.CreateInstance(typeof(T), provider.Location)!;
		var expected = await provider.RealListSettingsAsync();
		var actual = await provider2.RealListSettingsAsync();
		expected.Lines.Add("42");
		expected.Lines.Add("128");
		expected.Lines.Add(LongText);
		await expected.FlushAsync();

		SpinWait.SpinUntil(() => actual.Lines.Count == expected.Lines.Count, DefaultTimeout);
		Debug.Print("Expected: {0}", expected.ToJsonString());
		Debug.Print("Actual: {0}", actual.ToJsonString());
		Assert.IsTrue(actual.Lines.SequenceEqual(expected.Lines));
	}
}

[TestClass]
public class MemorySettingsProviderTests : SettingsProviderTest<MemorySettingsProvider> { }

[TestClass]
public class JsonFileSettingsProviderTests : FileSettingsProviderTests<JsonFileSettingsProvider> {
	/// <inheritdoc />
	protected override JsonFileSettingsProvider CreateUniqueProvider(string? name = null)
		=> new(
			Path.Combine(
				Path.GetTempPath(),
				"tests",
				nameof(JsonFileSettingsProvider),
				name!,
				DateTime.Now.Ticks.ToString("D")
			)
		);
}

[TestClass]
public class JsonNetFileSettingsProviderTests : FileSettingsProviderTests<JsonNetFileSettingsProvider> {
	/// <inheritdoc />
	protected override JsonNetFileSettingsProvider CreateUniqueProvider(string? name = null) 
		=> new(
			Path.Combine(
				Path.GetTempPath(),
				"tests",
				nameof(JsonNetFileSettingsProviderTests),
				name!,
				DateTime.Now.Ticks.ToString("D")
			)
		);
}