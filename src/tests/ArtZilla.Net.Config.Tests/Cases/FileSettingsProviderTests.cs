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