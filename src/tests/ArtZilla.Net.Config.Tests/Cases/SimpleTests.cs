using System.Diagnostics;
using System.Reflection;

namespace ArtZilla.Net.Config.Tests;

[TestClass]
public sealed class SimpleTests : Core {
	[TestMethod, Timeout(DefaultTimeout)]
	public void GeneratedClassTest() {
		var intfName = typeof(ISimpleSettings).FullName;
		var (copyName, readName, inpcName, realName) = ConfigUtils.GenerateTypeNames(intfName!);
		Console.WriteLine($"intf: {intfName}");
		Console.WriteLine($"copy: {copyName}");
		Console.WriteLine($"read: {readName}");
		Console.WriteLine($"inpc: {inpcName}");
		Console.WriteLine($"real: {realName}");

		var copy = Assembly.GetExecutingAssembly().GetType(copyName);
		var read = Assembly.GetExecutingAssembly().GetType(readName);
		var inpc = Assembly.GetExecutingAssembly().GetType(inpcName);
		var real = Assembly.GetExecutingAssembly().GetType(realName);

		Assert.IsNotNull(copy, "copy type not found");
		Assert.IsNotNull(read, "read type not found");
		Assert.IsNotNull(inpc, "inpc type not found");
		Assert.IsNotNull(real, "real type not found");

		var copySettings = Activator.CreateInstance(copy);
		var copyToString = copySettings?.ToString();
		Console.WriteLine(copyToString);
		var readSettings = Activator.CreateInstance(read);
		var readToString = readSettings?.ToString();
		Console.WriteLine(readToString);
		var inpcSettings = Activator.CreateInstance(inpc);
		var inpcToString = inpcSettings?.ToString();
		Console.WriteLine(inpcToString);
		var realSettings = Activator.CreateInstance(real);
		var realToString = realSettings?.ToString();
		Console.WriteLine(realToString);
		Assert.AreEqual(copyToString, readToString);
		Assert.AreEqual(copyToString, inpcToString);
		Assert.AreEqual(copyToString, realToString);
	}

	[TestMethod, Timeout(DefaultTimeout)]
	public void InitByMethodTest() {
		var settings = new InitByMethodSettings_Read(true);
		Debug.WriteLine(settings);
		Assert.AreEqual(2, settings.Lines.Count);
		Assert.AreEqual(42, settings.Number);
		Assert.AreEqual("Homura: being meguka is suffering! Я★", settings.Text);
		Assert.AreEqual(true, settings.IsOldInit);
	}

	[TestMethod, Timeout(DefaultTimeout)]
	public void SameAssemblySettingsTypeConstructorTest() {
		var ctor = new SameAssemblySettingsTypeConstructor();
		var copy = ctor.Default<ISimpleSettings>();
		copy.Text = "Marie Rose";
		var read = ctor.Clone<ISimpleSettings>(copy);
		Assert.AreEqual(copy.Text, read.Text);
	}

	[TestMethod, Timeout(DefaultTimeout)]
	public void JsonFileSerializerTest() {
		var serializer = new JsonFileSerializer(new SameAssemblySettingsTypeConstructor());
		var path = Path.GetTempFileName();
		var type = typeof(ISimpleSettings);
		var settings = new SimpleSettings_Copy(true) {
			Text = LongText
		};

		serializer.Serialize(type, path, settings);
		{
			var expected = settings.ToJsonString();
			var actual = File.ReadAllText(path);
			Debug.Print("expected text: {0}", expected);
			Debug.Print("actual text: {0}", actual);
			Assert.AreEqual(expected, actual);
		}

		{
			var expected = (ISimpleSettings) settings;
			var actual = (ISimpleSettings) serializer.Deserialize(type, path);
			Debug.Print("expected: {0}", expected);
			Debug.Print("actual: {0}", actual);
			Assert.AreEqual(expected.Text, actual.Text);
			// Assert.AreEqual(expected, actual); // <- todo
		}
	}
}