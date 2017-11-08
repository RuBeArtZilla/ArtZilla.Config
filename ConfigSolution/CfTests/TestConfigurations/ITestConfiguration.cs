using System;
using System.Linq;
using ArtZilla.Config;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CfTests {
	public interface ITestConfiguration : IConfiguration {
		[DefaultValue(TestConfiguration.DefaultInt8)]
		SByte SByte  { get; set; }

		[DefaultValue(TestConfiguration.DefaultInt16)]
		Int16 Int16 { get; set; }

		[DefaultValue(TestConfiguration.DefaultInt32)]
		Int32 Int32 { get; set; }

		[DefaultValue(TestConfiguration.DefaultInt64)]
		Int64 Int64 { get; set; }

		[DefaultValue(TestConfiguration.DefaultUInt8)]
		Byte Byte { get; set; }

		[DefaultValue(TestConfiguration.DefaultUInt16)]
		UInt16 UInt16 { get; set; }

		[DefaultValue(TestConfiguration.DefaultUInt32)]
		UInt32 UInt32 { get; set; }

		[DefaultValue(TestConfiguration.DefaultUInt64)]
		UInt64 UInt64 { get; set; }

		[DefaultValue(TestConfiguration.DefaultSingle)]
		Single Single { get; set; }

		[DefaultValue(TestConfiguration.DefaultDouble)]
		Double Double { get; set; }

		//[DefaultValue(typeof(Decimal), "0.0042")]
		//[DefaultValue(TestConfiguration.DefaultDecimal)]
		//Decimal Decimal { get; set; }

		[DefaultValue(TestConfiguration.DefaultBoolean)]
		Boolean Boolean { get; set; }

		[DefaultValue(TestConfiguration.DefaultChar)]
		Char Char{ get; set; }

		[DefaultValue(TestConfiguration.DefaultString)]
		String String { get; set; }
	}
}
