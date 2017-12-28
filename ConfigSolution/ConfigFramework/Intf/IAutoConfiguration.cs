using System.ComponentModel;

namespace ArtZilla.Config {
	/// <summary><see cref="IConfiguration"/> with <see cref="INotifyPropertyChanged"/></summary>
	public interface IAutoConfiguration : IConfiguration, INotifyPropertyChanged { }

	// /// <summary>auto saving <see cref="IConfiguration"/> with <see cref="INotifyPropertyChanged"/></summary>
	// public interface IAutoConfiguration: INpcConfiguration {

	//}
}