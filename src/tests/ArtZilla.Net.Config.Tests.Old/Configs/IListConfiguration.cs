using System.Collections.Generic;

namespace ArtZilla.Net.Config.Tests.TestConfigurations; 

public interface IListConfiguration: IConfiguration {
	IList<Hero> Heroes { get; set; }
	
	[DefaultValueByMethod(typeof(ListInit), nameof(ListInit.GetNames), "Sayaka", "Mami")]
	IList<string> Names { get; set; }
}

public static class ListInit {
	public static IList<string> GetNames(params string[] names) {
		var list = new List<string> { "Homura", "Madoka" };
		list.AddRange(names);
		return list;
	}
}