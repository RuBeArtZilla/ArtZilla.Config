using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ArtZilla.Net.Config.Tests;

public abstract class Core {
	protected const int NewInteger = 4;
	protected const double NewDouble = 8;
	protected const string NewString = "15";

	protected void ChangeConfig(ITestConfiguration cfg) {
#pragma warning disable CS0618
		cfg.Int32 = NewInteger;
		cfg.Double = NewDouble;
		cfg.String = NewString;
#pragma warning restore CS0618
	}

	protected void CheckIsChanged(ITestConfiguration cfg) {
#pragma warning disable CS0618
		Assert.AreEqual(NewInteger, cfg.Int32);
		Assert.AreEqual(NewDouble, cfg.Double);
		Assert.AreEqual(NewString, cfg.String);
#pragma warning restore CS0618
	} 
}