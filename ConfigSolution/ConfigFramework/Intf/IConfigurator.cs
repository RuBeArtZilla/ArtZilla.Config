using System.ComponentModel;

namespace ArtZilla.Config {
	public interface IConfigurator {
		/// <summary>
		/// Method return <see cref="IAutoConfiguration"/> implementation of actual <typeparamref name="TConfiguration"/>
		/// </summary>
		/// <typeparam name="TConfiguration">type of <see cref="IConfiguration"/> to return</typeparam>
		/// <returns><see cref="IAutoConfiguration"/> implementation of actual <typeparamref name="TConfiguration"/></returns>
		TConfiguration GetAuto<TConfiguration>() where TConfiguration : IConfiguration;

		/// <summary>
		/// Method return <see cref="IRealtimeConfiguration"/> implementation of actual <typeparamref name="TConfiguration"/>
		/// </summary>
		/// <typeparam name="TConfiguration"></typeparam>
		/// <returns></returns>
		TConfiguration GetRealtime<TConfiguration>() where TConfiguration : IConfiguration;

		/// <summary>
		///	return a copy of actual configuration implemented <see cref="INotifyPropertyChanged"/>
		/// </summary>
		/// <typeparam name="TConfiguration"></typeparam>
		/// <returns></returns>
		TConfiguration GetAutoCopy<TConfiguration>() where TConfiguration : IConfiguration;

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
		TConfiguration GetReadOnly<TConfiguration>() where TConfiguration : IConfiguration;

		void Save<TConfiguration>(TConfiguration value) where TConfiguration : IConfiguration;

		void Reset<TConfiguration>() where TConfiguration : IConfiguration;
	}
}