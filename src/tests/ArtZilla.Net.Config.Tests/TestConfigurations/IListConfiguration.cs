using System.Collections.Generic;

namespace ArtZilla.Net.Config.Tests.TestConfigurations; 

public interface IListConfiguration: IConfiguration {
	IList<Hero> Heroes { get; set; }
	IList<string> Names { get; set; }
}