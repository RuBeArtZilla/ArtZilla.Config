using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ArtZilla.Config.Tests.TestConfigurations {
	public interface IListConfiguration: IConfiguration {
		IList<Hero> Heroes { get; set; }
	}
}