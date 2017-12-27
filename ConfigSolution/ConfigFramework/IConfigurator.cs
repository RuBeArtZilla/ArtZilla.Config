using System.ComponentModel;

namespace ArtZilla.Config {
	public interface IXxx : IConfiguration, INotifyPropertyChanged { }
	// public interface IYyy<T> where T: IConfiguration { }
	// public interface IZzz<T> : INotifyPropertyChanged, IYyy<IConfiguration> { }
	// public interface IAutoConfiguration<in T> : T, INotifyPropertyChanged where T: IConfiguration { }

	public interface IConfigurator {
		/// <summary>
		/// return (automatically loading/saving) configuration implemented <see cref="INotifyPropertyChanged"/>
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		T GetAuto<T>() where T : IConfiguration;

		/// <summary>
		///	return a copy of actual configuration implemented <see cref="INotifyPropertyChanged"/>
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		T GetAutoCopy<T>() where T : IConfiguration;

		/// <summary>
		/// return a copy of actual configuration 
		/// </summary>
		/// <typeparam name="TConfiguration"></typeparam>
		/// <returns></returns>
		TConfiguration GetCopy<TConfiguration>() where TConfiguration : IConfiguration;

		/// <summary>
		/// return a read only configuration
		/// </summary>
		/// <typeparam name="TConfiguration"></typeparam>
		/// <returns></returns>
		T GetReadOnly<T>() where T : IConfiguration;

		void Save<T>(T value) where T : IConfiguration;

		void Reset<T>() where T : IConfiguration;
	}
}