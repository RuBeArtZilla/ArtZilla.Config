﻿namespace ArtZilla.Net.Config.Tests.Generators; 

public static class AssertCfg {
	public static void IsDefault(ITestConfiguration cfg)
		=> AssertEqualProperties(cfg, new TestConfiguration());

	public static void IsNotDefault(ITestConfiguration cfg, string message = "Configuration is not in default state")
		=> Assert.IsFalse(AreEqualAllProperties(cfg, new TestConfiguration()), message);

	public static void AssertEqualProperties(ITestConfiguration x, ITestConfiguration y) {
		foreach (var p in typeof(ITestConfiguration).GetProperties())
			Assert.AreEqual(p.GetValue(y), p.GetValue(x), "Property " + p.Name);
	}

	public static bool AreEqualAllProperties(ITestConfiguration x, ITestConfiguration y) {
		var equals = true;
		foreach (var p in typeof(ITestConfiguration).GetProperties())
			equals = equals && p.GetValue(y).Equals(p.GetValue(x));
		return equals;
	}

	public static bool AreNotAllPropertiesEquals(ITestConfiguration x, ITestConfiguration y)
		=> typeof(ITestConfiguration).GetProperties()
			.Any(p => p.GetValue(y).Equals(p.GetValue(x)));

	public static bool AreEquals<T>(IEnumerable<T> x, IEnumerable<T> y) where T:IEquatable<T> {
		var xi = x.GetEnumerator();
		var yi = y.GetEnumerator();
		var xx = false;
		var yy = false;

		do {
			if (!xi.Current.Equals(yi.Current))
				return false;

			xx = xi.MoveNext();
			yy = yi.MoveNext();
		} while (xx && yy);
		return xx == yy;
	}
}