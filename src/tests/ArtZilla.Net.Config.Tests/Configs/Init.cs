namespace ArtZilla.Net.Config.Tests;

public static partial class Init {
	public static void InitLines(IList<string> lines) {
		lines.Add("Homura");
		lines.Add("Madoka");
	}

	public static void InitMap(ISettingsDict<Guid, ISimpleSettings> map) {
		var s1 = map.AddNew(Guid.NewGuid());
		s1.Text = "Akemi Homura";
		var s2 = map.AddNew(Guid.NewGuid());
		s2.Text = "Kaname Madoka";
	}

	public static void InitText(out string value) => value = "Homura";

	public static void InitText(out string value, string suffix) => value = "Homura" + suffix;

	public static void InitNumber(ref int number) => number = 42;

	public static bool IsOldInit(bool value) => value;
}