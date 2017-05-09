using System;
using System.Linq;
using ArtZilla.Config;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CfTests {
	public class TestConfiguration : ITestConfiguration {
		public const SByte DefaultInt8 = -8;
		public const Int16 DefaultInt16 = -16;
		public const Int32 DefaultInt32 = -32;
		public const Int64 DefaultInt64 = -64;

		public const Byte DefaultUInt8 = 8;
		public const UInt16 DefaultUInt16 = 16;
		public const UInt32 DefaultUInt32 = 32;
		public const UInt64 DefaultUInt64 = 64;

		public const Boolean DefaultBoolean = true;
		public const Single DefaultSingle = 0.42F;
		public const Double DefaultDouble = 0.042D;
		public const Decimal DefaultDecimal = 0.0042M;

		public const Char DefaultChar = '★';
		public const String DefaultString = "String";

		public SByte SByte { get; set; } = DefaultInt8;
		public Int16 Int16 { get; set; } = DefaultInt16;
		public Int32 Int32 { get; set; } = DefaultInt32;
		public Int64 Int64 { get; set; } = DefaultInt64;

		public Byte Byte { get; set; } = DefaultUInt8;
		public UInt16 UInt16 { get; set; } = DefaultUInt16;
		public UInt32 UInt32 { get; set; } = DefaultUInt32;
		public UInt64 UInt64 { get; set; } = DefaultUInt64;

		public Boolean Boolean { get; set; } = DefaultBoolean;
		public Single Single { get; set; } = DefaultSingle;
		public Double Double { get; set; } = DefaultDouble;
		public Decimal Decimal { get; set; } = DefaultDecimal;

		public Char Char{ get; set; } = DefaultChar;
		public String String { get; set; } = DefaultString;

		public void Copy(IConfiguration cfg)
			=> Copy((ITestConfiguration)cfg);

		public void Copy(ITestConfiguration cfg) {
			Int32 = cfg.Int32;
			String = cfg.String;

			foreach (var p in typeof(ITestConfiguration).GetProperties())
				p.SetValue(this, p.GetValue(cfg));
		}
	}
}
