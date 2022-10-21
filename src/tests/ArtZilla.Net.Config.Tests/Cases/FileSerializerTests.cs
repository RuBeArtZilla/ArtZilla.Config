using System.Diagnostics;

namespace ArtZilla.Net.Config.Tests;

public abstract class FileSerializerTests<T> : Core where T: FileSerializer {
	protected abstract T CreateSerializer();

	[TestMethod, Timeout(DefaultTimeout)]
	public async Task BasicTest() {
		var serializer = CreateSerializer();
		var path = Path.GetTempFileName();
		var type = typeof(ISimpleSettings);
		var settings = new SimpleSettings_Copy(true) {
			Text = LongText
		};

		await serializer.Serialize(type, path, settings);
		{
			var expected = settings.ToJsonString();
			var actual = await File.ReadAllTextAsync(path);
			Debug.Print("expected text: {0}", expected);
			Debug.Print("actual text: {0}", actual);
			// Assert.AreEqual(expected, actual);
		}

		{
			var expected = (ISimpleSettings) settings;
			var actual = (ISimpleSettings) await serializer.Deserialize(type, path);
			Debug.Print("expected: {0}", expected);
			Debug.Print("actual: {0}", actual);
			Assert.AreEqual(expected.Text, actual.Text);
			// Assert.AreEqual(expected, actual); // <- todo
		}
	}
}

[TestClass]
public class JsonFileSerializerTests : FileSerializerTests<JsonFileSerializer> {
	protected override JsonFileSerializer CreateSerializer()
		=> new(new SameAssemblySettingsTypeConstructor());
}

[TestClass]
public class JsonNetFileSerializerTests : FileSerializerTests<JsonNetFileSerializer> {
	protected override JsonNetFileSerializer CreateSerializer()
		=> new(new SameAssemblySettingsTypeConstructor());
}