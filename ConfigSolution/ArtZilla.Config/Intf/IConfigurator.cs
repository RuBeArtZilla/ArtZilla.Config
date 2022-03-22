using System;
using System.Collections.Generic;

namespace ArtZilla.Config {
	public interface IConfigurator {
		/// <summary>
		///	Remove all <see cref="IConfiguration"/> from this instance
		/// </summary>
		void Clear();

		/// Save <paramref name="value"/> as <typeparamref name="TConfiguration"/>
		/// <typeparam name="TConfiguration"></typeparam>
		/// <param name="value"></param>
		void Save<TConfiguration>(TConfiguration value) where TConfiguration : class, IConfiguration;
		
		/// <summary>
		/// Reset <typeparamref name="TConfiguration"/> to default values
		/// </summary>
		/// <typeparam name="TConfiguration"></typeparam>
		void Reset<TConfiguration>() where TConfiguration : class, IConfiguration;

		bool IsExist<TConfiguration>() where TConfiguration : class, IConfiguration;

		/// <summary>
		/// return a copy of actual <typeparamref name="TConfiguration"/>
		/// </summary>
		/// <typeparam name="TConfiguration"></typeparam>
		/// <returns></returns>
		TConfiguration Copy<TConfiguration>() where TConfiguration : class, IConfiguration;

		/// <summary>
		/// Method return a copy of actual <typeparamref name="TConfiguration"/> with <see cref="INotifyingConfiguration"/> implementation
		/// </summary>
		/// <typeparam name="TConfiguration">type of <see cref="IConfiguration"/> to return</typeparam>
		/// <returns><see cref="INotifyingConfiguration"/> implementation of actual <typeparamref name="TConfiguration"/></returns>
		TConfiguration Notifying<TConfiguration>() where TConfiguration : class, IConfiguration;

		/// <summary>
		/// return a read only configuration
		/// </summary>
		/// <typeparam name="TConfiguration"></typeparam>
		/// <returns></returns>
		TConfiguration Readonly<TConfiguration>() where TConfiguration : class, IConfiguration;

		/// <summary>
		/// Method return actual <typeparamref name="TConfiguration"/> with <see cref="IRealtimeConfiguration"/> implementation
		/// </summary>
		/// <typeparam name="TConfiguration"></typeparam>
		/// <returns></returns>
		TConfiguration Realtime<TConfiguration>() where TConfiguration : class, IConfiguration;

		IConfigurator<TConfiguration> As<TConfiguration>() where TConfiguration : class, IConfiguration;

		IConfigurator<TKey, TConfiguration> As<TKey, TConfiguration>() where TConfiguration : class, IConfiguration;

		IConfiguration[] Get();

		void Set(params IConfiguration[] configurations);

		void CloneTo(IConfigurator destination);
	}

	public interface IConfigProvider {
		bool IsExist();
		void Reset();
		void Save(IConfiguration value);
		IConfiguration GetCopy();
		INotifyingConfiguration GetNotifying();
		IReadonlyConfiguration GetReadonly();
		IRealtimeConfiguration GetRealtime();
	}

	public interface IConfigurator<TConfiguration> : IConfigProvider where TConfiguration : class, IConfiguration {
		void Save(TConfiguration value);
		TConfiguration Copy();
		TConfiguration Notifying();
		TConfiguration Readonly();
		TConfiguration Realtime();

		IConfigurator<TKey, TConfiguration> As<TKey>();
	}

	public interface IConfigurator<TKey, TConfiguration> : IEnumerable<TConfiguration> where TConfiguration : class, IConfiguration {
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