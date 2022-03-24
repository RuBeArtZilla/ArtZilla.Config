using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace ArtZilla.Net.Config.Tests.TestConfigurations; 

public interface IExampleSettings : IConfiguration {
	[DefaultValue(true)]
	bool Switch { get; set; }

	[DefaultValue(69)]
	int Number { get; set; }

	[DefaultValue("Akemi★Homura")]
	string Text { get; set; }

	[DefaultValue(Girls.Homura)]
	Girls Girl { get; set; }

	IList<string> Lines { get; set; }
}

public static class ExampleUtils {
	public static void Fill1(this IExampleSettings settings) {
		settings.Switch = true;
		settings.Number = 69;
		settings.Text = "Akemi★Homura";
		settings.Girl = Girls.Homura;
		settings.Lines.Clear();
	}

	public static void Fill2(this IExampleSettings settings) {
		settings.Switch = false;
		settings.Number = 42;
		settings.Text = ComplexConfig.MagicDictionary[42];
		settings.Girl = Girls.Kyoko;
		settings.Lines = new List<string> { "1", "2", "3" };
	}

	public static bool IsEqual(this IExampleSettings l, IExampleSettings r)
		=> l.Switch == r.Switch &&
		   l.Number == r.Number &&
		   l.Text == r.Text &&
		   l.Girl == r.Girl &&
		   l.Lines.SequenceEqual(r.Lines);
}