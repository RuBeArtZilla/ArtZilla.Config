using System;
using System.Collections.Generic;
using System.Linq;

namespace ArtZilla.Config.Tests.TestConfigurations {
	public interface IComplexConfig : IConfiguration {
		int[] ValueArray { get; set; }
		List<int> ValueList { get; set; }
		Dictionary<int, string> ValueDictionary { get; set; }
	}

	public interface IMediaLibraryConfiguration : IConfiguration {
		List<string> Paths { get; }
	}

	public enum Girls : long {
		Madoka = 0,
		Homura = 1,
		Sayaka = 2,
		Kyoko = 3,
		Mami = 4,
	}

	public interface IConfigWithEnum : IConfiguration {
		[DefaultValue(Girls.Homura)]
		Girls MyWaifu { get; set; }

		[DefaultValue(4)]
		Girls Headless { get; set; }

		[DefaultValueByCtor(typeof(Guid), "{D1F71EC6-76A6-40F8-8910-68E67D753CD4}")]
		Guid SomeGuid { get; set; }
	}

	public class ComplexConfig : IComplexConfig {
		public static readonly int[] DefaultArray = {0, 1, 2, 3, 4, 5, 6, 7};

		public static readonly Dictionary<int, string> DefaultDictionary
			= DefaultArray.ToDictionary(x => x, x => (x << 2).ToString());

		public static readonly int[] MagicArray = {4, 8, 15, 16, 23, 42};

		public static readonly Dictionary<int, string> MagicDictionary
			= new Dictionary<int, string> {
				[4] = "Quick brown fox jumps over the lazy dog",
				[8] = "Эй, жлоб! Где туз? Прячь юных съёмщиц в шкаф.",
				[15] = "いろはにほへと ちりぬるを わかよたれそ つねならむ うゐのおくやま けふこえて あさきゆめみし ゑひもせす",
				[16] = String.Empty,
				[23] = null,
				[42] = "All your base are belong to us",
			};

		public int[] ValueArray { get; set; }
			= DefaultArray;

		public List<int> ValueList { get; set; }
			= new List<int>(DefaultArray);

		public Dictionary<int, string> ValueDictionary { get; set; }
			= new Dictionary<int, string>(DefaultDictionary);

		public void Copy(IConfiguration source) {
			// todo:
		}
	}
}