using System;
using System.Collections.Concurrent;
using System.ComponentModel;

namespace ArtZilla.Config.Configurators {
	public class MemoryConfigurator: IConfigurator {
		/// <summary>
		/// return a copy of actual <typeparamref name="TConfiguration" />
		/// </summary>
		/// <typeparam name="TConfiguration"></typeparam>
		/// <returns></returns>
		public virtual TConfiguration GetCopy<TConfiguration>()
			where TConfiguration : IConfiguration
			=> (TConfiguration) Activator.CreateInstance(
				TmpCfgClass<TConfiguration>.CopyType,
				Get<TConfiguration>());

		/// <summary>
		/// return a read only configuration
		/// </summary>
		/// <typeparam name="TConfiguration"></typeparam>
		/// <returns></returns>
		public virtual TConfiguration GetReadonly<TConfiguration>()
			where TConfiguration : IConfiguration
			=> (TConfiguration) Activator.CreateInstance(
				TmpCfgClass<TConfiguration>.ReadonlyType,
				Get<TConfiguration>());

		/// <summary>
		/// Method return a copy of actual <typeparamref name="TConfiguration" /> with <see cref="INotifyingConfiguration" /> implementation
		/// </summary>
		/// <typeparam name="TConfiguration">type of <see cref="IConfiguration" /> to return</typeparam>
		/// <returns><see cref="INotifyingConfiguration" /> implementation of actual <typeparamref name="TConfiguration" /></returns>
		public virtual TConfiguration GetNotifying<TConfiguration>()
			where TConfiguration : IConfiguration
			=> (TConfiguration)Activator.CreateInstance(
				TmpCfgClass<TConfiguration>.NotifyingType, 
				Get<TConfiguration>());

		/// <summary>
		/// Method return actual <typeparamref name="TConfiguration" /> with <see cref="IRealtimeConfiguration" /> implementation
		/// </summary>
		/// <typeparam name="TConfiguration"></typeparam>
		/// <returns></returns>
		public virtual TConfiguration GetRealtime<TConfiguration>()
			where TConfiguration : IConfiguration
			=> Get<TConfiguration>();
			// => (TConfiguration) Activator.CreateInstance(TmpCfgClass<TConfiguration>.RealtimeType, Get<TConfiguration>());

		/// <summary>
		/// Reset <typeparamref name="TConfiguration" /> to default values
		/// </summary>
		/// <typeparam name="TConfiguration"></typeparam>
		public virtual void Reset<TConfiguration>()
			where TConfiguration : IConfiguration
			=> Save(CreateDefault<TConfiguration>());

		/// <summary>
		/// Save <paramref name="value" /> as <typeparamref name="TConfiguration" />
		/// </summary>
		/// <typeparam name="TConfiguration"></typeparam>
		/// <param name="value"></param>
		public virtual void Save<TConfiguration>(TConfiguration value)
			where TConfiguration : IConfiguration
			=> Get<TConfiguration>().Copy(value);

		protected virtual TConfiguration Load<TConfiguration>()
			where TConfiguration : IConfiguration
			=> CreateDefault<TConfiguration>();

		protected virtual TConfiguration Get<TConfiguration>()
			where TConfiguration : IConfiguration
			=> (TConfiguration) _cache.GetOrAdd(typeof(TConfiguration), Load<TConfiguration>());

		protected virtual TConfiguration CreateDefault<TConfiguration>()
			where TConfiguration : IConfiguration {
			var cfg = (TConfiguration)Activator.CreateInstance(TmpCfgClass<TConfiguration>.RealtimeType);
			Subscribe(cfg);
			return cfg;
		}

		protected virtual void Subscribe<TConfiguration>(TConfiguration cfg)
			where TConfiguration : IConfiguration {
			var real = (IRealtimeConfiguration)cfg;
			real.PropertyChanged += ConfigurationChanged<TConfiguration>;
		}

		protected virtual void ConfigurationChanged<TConfiguration>(object sender, PropertyChangedEventArgs e)
			where TConfiguration : IConfiguration {
			//
		}

		protected Type GetSimpleType<TConfiguration>()
			where TConfiguration : IConfiguration
			=> typeof(TConfiguration).IsInterface
				? TmpCfgClass<TConfiguration>.CopyType
				: typeof(TConfiguration);

		private readonly ConcurrentDictionary<Type, IConfiguration> _cache
			= new ConcurrentDictionary<Type, IConfiguration>();
	}
}
