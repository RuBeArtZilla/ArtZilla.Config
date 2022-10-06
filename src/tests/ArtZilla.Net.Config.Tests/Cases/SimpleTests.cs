using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;

namespace ArtZilla.Net.Config.Tests.Generators;

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
		var settings = new InitByMethodSettings_Read();
		Debug.WriteLine(settings);
		Assert.AreEqual(2, settings.Lines.Count);
		Assert.AreEqual(42, settings.Number);
		Assert.AreEqual("Homura: being meguka is suffering! Я★", settings.Text);
		Assert.AreEqual(true, settings.IsOldInit);
	}

	[TestMethod, Timeout(DefaultTimeout)]
	public void SameAssemblySettingsTypeConstructorTest() {
		var ctor = new SameAssemblySettingsTypeConstructor();
		var copy = ctor.CreateCopy<ISimpleSettings>();
		copy.Text = "Marie Rose";
		var read = ctor.CreateRead(copy);
		Assert.AreEqual(copy.Text, read.Text);
	}
}

[GenerateConfiguration]
public interface ISimpleSettings : ISettings {
	[DefaultValue("Hello World!")]
	string Text { get; set; }
}

[GenerateConfiguration]
interface IInheritedSettings : ISimpleSettings {
	[DefaultValue(.442f)]
	[System.ComponentModel.Description("Some int value"), Browsable(false)]
	Single? Value { get; set; }
}

[GenerateConfiguration]
interface INotSupportedSettings : INotifyPropertyChanged {
	int SomeRandomValue { get; set; }
}

[GenerateConfiguration]
interface INotFoundMethodName : ISettings {
	[DefaultValueByMethod(typeof(Init), "ThisMethodShouldNotExist")]
	int X { get; set; }
}

[GenerateConfiguration]
interface IInitByMethodSettings : ISettings {
	[DefaultValueByMethod(typeof(Init), nameof(Init.InitLines))]
	IList<string> Lines { get; }

	[DefaultValueByMethod(typeof(Init), nameof(Init.InitNumber))]
	int Number { get; set; }

	[DefaultValueByMethod(typeof(Init), nameof(Init.InitText), ": being meguka is suffering! Я★")]
	string Text { get; set; }

	[DefaultValueByMethod(typeof(Init), nameof(Init.IsOldInit), true)]
	bool IsOldInit { get; set; }
}

[GenerateConfiguration]
interface IListSettings : ISettings {
	[DefaultValueByMethod(typeof(Init), nameof(Init.InitLines))]
	IConfigList<string> Lines { get; set; }
}

public static partial class Init {
	public static void InitLines(IList<string> lines) {
		lines.Add("Homura");
		lines.Add("Madoka");
	}

	public static void InitText(out string value) => value = "Homura";

	public static void InitText(out string value, string suffix) => value = "Homura" + suffix;

	public static void InitNumber(ref int number) => number = 42;

	public static bool IsOldInit(bool value) => value;
}