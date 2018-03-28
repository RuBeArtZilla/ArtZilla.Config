using System.ComponentModel;

namespace ArtZilla.Config {
	/// <summary><see cref="IConfiguration"/> with <see cref="INotifyPropertyChanged"/></summary>
	public interface INotifyingConfiguration : IConfiguration, INotifyPropertyChanged { }
}