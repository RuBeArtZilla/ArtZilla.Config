using System.Diagnostics;

namespace ArtZilla.Net.Config.Tests;

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

		using var provider2 = (T) Activator.CreateInstance(typeof(T), provider.Location)!;
		var actual = await provider2.ReadSimpleSettingsAsync();
		Assert.AreEqual(LongText, actual.Text);
	}

	[TestMethod, Timeout(DefaultTimeout), DoNotParallelize]
	public async Task SameLocationSyncTest() {
		var provider = CreateUniqueProvider();
		var provider2 = (T) Activator.CreateInstance(typeof(T), provider.Location)!;
		var expected = await provider.RealSimpleSettingsAsync();
		SpinWait.SpinUntil(() => !provider.IsAnyChangesInQueue(), DefaultTimeout);

		var actual = await provider2.RealSimpleSettingsAsync();
		expected.Text = LongText;
		await provider.FlushAsync();

		SpinWait.SpinUntil(() => !provider.IsAnyChangesInQueue(), DefaultTimeout);
		SpinWait.SpinUntil(() => !provider2.IsAnyChangesInQueue(), DefaultTimeout);
		SpinWait.SpinUntil(() => actual.Text == LongText, 5000);

		Assert.AreEqual(expected.Text, actual.Text);
		Assert.AreEqual(LongText, actual.Text);

		provider.TryDispose();
		provider2.TryDispose();
	}

	[TestMethod, Timeout(DefaultTimeout), DoNotParallelize]
	public async Task SameLocationListSyncTest() {
		using var provider = CreateUniqueProvider();
		using var provider2 = (T) Activator.CreateInstance(typeof(T), provider.Location)!;
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

	[TestMethod, Timeout(DefaultTimeout), DoNotParallelize]
	public async Task SameLocationDictSyncTest() {
		var provider = CreateUniqueProvider();
		var provider2 = (T) Activator.CreateInstance(typeof(T), provider.Location)!;
		var expected = await provider.RealDictSettingsAsync();

		await Task.Delay(2000);
		SpinWait.SpinUntil(() => !provider.IsAnyChangesInQueue(), DefaultTimeout);

		var actual = await provider2.RealDictSettingsAsync();
		var guid = Guid.NewGuid();
		var expectedSettings = expected.Map.AddNew(guid);
		expectedSettings.Text = LongText;
		await provider.FlushAsync();

		SpinWait.SpinUntil(() => !provider.IsAnyChangesInQueue(), DefaultTimeout);
		SpinWait.SpinUntil(() => !provider2.IsAnyChangesInQueue(), DefaultTimeout);
		await Task.Delay(3000);

		Debug.Print("Expected: {0}", expected.ToJsonString());
		Debug.Print("Actual: {0}", actual.ToJsonString());
		Assert.IsTrue(actual.Map.SequenceEqual(expected.Map));

		provider.TryDispose();
		provider2.TryDispose();
	}
}