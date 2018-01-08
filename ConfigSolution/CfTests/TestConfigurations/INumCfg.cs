using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtZilla.Config.Tests.TestConfigurations {
	interface INumCfg : IConfiguration {
		[DefaultValue(42)]
		int Value { get; set; }
	}
}
