using System.ComponentModel;

namespace ArtZilla.Net.Config.Tests;

public interface ITextConfiguration : IConfiguration {
	[DefaultValue("Lorem ipsum dolor sit amet, consectetur adipiscing elit. "
	              + "Pellentesque id mollis neque. Aenean leo augue, ultrices sit amet tincidunt vel, elementum et urna. "
	              + "Mauris vel odio neque. In hac habitasse platea dictumst. "
	              + "Morbi elit risus, fermentum eget libero in, maximus ultricies massa. "
	              + "Aliquam elementum egestas justo. Vestibulum eget vulputate est, vitae blandit urna. "
	              + "Pellentesque orci neque, posuere id accumsan quis, pulvinar ut diam.")]
	string Text { get; set; }
}
             
// [GenerateConfiguration]
public interface IInheritedConfiguration : ITextConfiguration {
	[DefaultValue(42)]
	int? Value { get; set; }
}        

public interface IConfigListConfiguration : IConfiguration {
	IConfigList<string> Items { get; set; }
}