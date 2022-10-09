using System.ComponentModel;
using ArtZilla.Net.Config.Tests.Generators;

namespace ArtZilla.Net.Config.Tests; 

[GenerateConfiguration]
public interface ITestConfiguration : ISettings {
	[DefaultValue(TestConfiguration.DefaultUInt64)] 
	ulong UInt64 { get; set; }

	[DefaultValue(TestConfiguration.DefaultUInt32)]
	uint UInt32 { get; set; } 

	[DefaultValue(TestConfiguration.DefaultUInt16)]
	ushort UInt16 { get; set; }

	[DefaultValue(TestConfiguration.DefaultUInt8)]
	byte Byte { get; set; }

	[DefaultValue(TestConfiguration.DefaultInt8)]
	sbyte SByte  { get; set; }

	[DefaultValue(TestConfiguration.DefaultInt16)]
	short Int16 { get; set; }
	
	[System.ComponentModel.Description("Integer value"), Obsolete("just for lulz")]
	[DefaultValue(TestConfiguration.DefaultInt32)]
	int Int32 { get; set; }

	[DefaultValue(TestConfiguration.DefaultInt64)]
	long Int64 { get; set; }
	
	[DefaultValue(TestConfiguration.DefaultSingle)]
	float Single { get; set; }

	[DefaultValue(TestConfiguration.DefaultDouble)]
	double Double { get; set; }

	//[DefaultValue(typeof(Decimal), "0.0042")]
	//[DefaultValue(TestConfiguration.DefaultDecimal)]
	//Decimal Decimal { get; set; }

	[DefaultValue(TestConfiguration.DefaultBoolean)]
	bool Boolean { get; set; }

	[DefaultValue(TestConfiguration.DefaultChar)]
	char Char{ get; set; }

	/// Text value
	[DefaultValue(TestConfiguration.DefaultString)]
	string String { get; set; }
}

