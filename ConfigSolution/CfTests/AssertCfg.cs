using System;
using System.Linq;
using ArtZilla.Config;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CfTests {
	public static class AssertCfg {
		public static void IsDefault(ITestConfiguration cfg)
			=> AssertEqualProperties(cfg, new TestConfiguration());

		public static void IsNotDefault(ITestConfiguration cfg, String message = "Configuration is not in default state")
			=> Assert.IsFalse(AreEqualAllProperties(cfg, new TestConfiguration()), message);

		public static void AssertEqualProperties(ITestConfiguration x, ITestConfiguration y) {
			foreach (var p in typeof(ITestConfiguration).GetProperties())
				Assert.AreEqual(p.GetValue(y), p.GetValue(x), "Property " + p.Name);
		}

		public static Boolean AreEqualAllProperties(ITestConfiguration x, ITestConfiguration y) {
			var equals = true;
			foreach (var p in typeof(ITestConfiguration).GetProperties())
				equals = equals && p.GetValue(y).Equals(p.GetValue(x));
			return equals;
		}

		public static Boolean AreNotAllPropertiesEquals(ITestConfiguration x, ITestConfiguration y) 
			=> typeof(ITestConfiguration).GetProperties()
																	 .Any(p => p.GetValue(y).Equals(p.GetValue(x)));
	}
}
