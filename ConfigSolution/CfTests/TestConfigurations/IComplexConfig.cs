using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

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

	public interface IConfigWithGuid : IConfiguration {
		[DefaultValueByCtor(typeof(Guid), "{D1F71EC6-76A6-40F8-8910-68E67D753CD4}")]
		Guid SomeGuid { get; set; }

		Guid NextGuid { get; set; }
	}

	public interface IConfigWithEnum : IConfigWithGuid {
		[DefaultValue(Girls.Homura)]
		Girls MyWaifu { get; set; }

		[DefaultValue(4)]
		Girls Headless { get; set; }
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
				[16] = string.Empty,
				[23] = null,
				[42] = "All your base are belong to us",
			};
		private DateTime _date = DateTime.Now;

		public int[] ValueArray { get; set; }
			= DefaultArray;

		public List<int> ValueList { get; set; }
			= new List<int>(DefaultArray);

		public Dictionary<int, string> ValueDictionary { get; set; }
			= new Dictionary<int, string>(DefaultDictionary);

		public DateTime Date {
			get { return _date; }
			set {
				if (_date == value)
					return;
				_date = value;
				Console.WriteLine("some string");
			}
		}

		public void Copy(IConfiguration source) {
			// todo:
		}
	}

	public interface IDateConfig : IConfiguration {
		DateTime Date { get; set; }
	}

	public interface IDateConfigEx : IDateConfig {
		// [DefaultValueByMethod(typeof(DateTime), nameof(DateTime.Now))]
		DateTime CreatedAt { get; set; }
	}

	public interface IGuidConfig : IConfiguration {
		[DefaultValueByMethod(typeof(Guid), nameof(Guid.NewGuid))]
		Guid Guid { get; set; }
	}

	[Serializable]
	public struct ShoujoState : IEquatable<ShoujoState> {
		[XmlAttribute]
		public bool IsAlive;

		[XmlAttribute]
		public Girls Shoujo;

		public ShoujoState(Girls shoujo, bool isAlive = false) {
			IsAlive = isAlive;
			Shoujo = shoujo;
		}

		public override bool Equals(object obj) => obj is ShoujoState && Equals((ShoujoState)obj);
		public bool Equals(ShoujoState other) => IsAlive == other.IsAlive && Shoujo == other.Shoujo;
		public static bool operator ==(ShoujoState state1, ShoujoState state2) => state1.Equals(state2);
		public static bool operator !=(ShoujoState state1, ShoujoState state2) => !(state1 == state2);
	}

	public interface IStructConfig : IConfiguration {
		[DefaultValueByCtor(typeof(ShoujoState), Girls.Homura, true)]
		ShoujoState Waifu { get; set; }
	}

	public interface IBaseConfig : IConfiguration {
		[DefaultValue(true)]
		bool IsEnabled { get; set; }
	}

	public interface IDerivedConfig : IBaseConfig {
		int MagicNumber { get ; set; }
	}
}