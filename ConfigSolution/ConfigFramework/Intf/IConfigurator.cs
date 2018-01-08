using System.Collections.Generic;

namespace ArtZilla.Config {
	public interface IConfigurator {
		/// <summary>
		/// Save <paramref name="value"/> as <typeparamref name="TConfiguration"/>
		/// </summary>
		/// <typeparam name="TConfiguration"></typeparam>
		/// <param name="value"></param>
		void Save<TConfiguration>(TConfiguration value) where TConfiguration : IConfiguration;

		/// <summary>
		/// Reset <typeparamref name="TConfiguration"/> to default values
		/// </summary>
		/// <typeparam name="TConfiguration"></typeparam>
		void Reset<TConfiguration>() where TConfiguration : IConfiguration;

		bool IsExist<TConfiguration>() where TConfiguration : IConfiguration;

		/// <summary>
		/// return a copy of actual <typeparamref name="TConfiguration"/>
		/// </summary>
		/// <typeparam name="TConfiguration"></typeparam>
		/// <returns></returns>
		TConfiguration Copy<TConfiguration>() where TConfiguration : IConfiguration;

		/// <summary>
		/// Method return a copy of actual <typeparamref name="TConfiguration"/> with <see cref="INotifyingConfiguration"/> implementation
		/// </summary>
		/// <typeparam name="TConfiguration">type of <see cref="IConfiguration"/> to return</typeparam>
		/// <returns><see cref="INotifyingConfiguration"/> implementation of actual <typeparamref name="TConfiguration"/></returns>
		TConfiguration Notifying<TConfiguration>() where TConfiguration : IConfiguration;

		/// <summary>
		/// return a read only configuration
		/// </summary>
		/// <typeparam name="TConfiguration"></typeparam>
		/// <returns></returns>
		TConfiguration Readonly<TConfiguration>() where TConfiguration : IConfiguration;

		/// <summary>
		/// Method return actual <typeparamref name="TConfiguration"/> with <see cref="IRealtimeConfiguration"/> implementation
		/// </summary>
		/// <typeparam name="TConfiguration"></typeparam>
		/// <returns></returns>
		TConfiguration Realtime<TConfiguration>() where TConfiguration : IConfiguration;

		IConfigurator<TConfiguration> As<TConfiguration>() where TConfiguration : IConfiguration;

		IConfigurator<TKey, TConfiguration> As<TKey, TConfiguration>() where TConfiguration : IConfiguration;
	}

	public interface IConfigurator<TConfiguration> where TConfiguration : IConfiguration {
		bool IsExist();
		void Reset();
		void Save(TConfiguration value);
		TConfiguration Copy();
		TConfiguration Notifying();
		TConfiguration Readonly();
		TConfiguration Realtime();

		IConfigurator<TKey, TConfiguration> As<TKey>();
	}

	public interface IConfigurator<TKey, TConfiguration> : IEnumerable<TConfiguration> where TConfiguration : IConfiguration {
		bool IsExist(TKey key);
		void Reset(TKey key);
		void Save(TKey key, TConfiguration value);
		TConfiguration Copy(TKey key);
		TConfiguration Notifying(TKey key);
		TConfiguration Readonly(TKey key);
		TConfiguration Realtime(TKey key);

		TConfiguration this[TKey key] { get; set; }
	}
}