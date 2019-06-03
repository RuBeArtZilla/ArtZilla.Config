using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using ArtZilla.Config.Builders;

namespace ArtZilla.Config.Configurators {
	public class MemoryConfigurator : IConfigurator {
		public bool IsExist<TConfiguration>()
			where TConfiguration : class, IConfiguration
			=> As<TConfiguration>().IsExist();

		/// <summary>
		/// Reset <typeparamref name="TConfiguration" /> to default values
		/// </summary>
		/// <typeparam name="TConfiguration"></typeparam>
		public virtual void Reset<TConfiguration>()
			where TConfiguration : class, IConfiguration
			=> As<TConfiguration>().Reset();

		public void Clear() => _dict.Clear();

		/// <summary>
		/// Save <paramref name="value" /> as <typeparamref name="TConfiguration" />
		/// </summary>
		/// <typeparam name="TConfiguration"></typeparam>
		/// <param name="value"></param>
		public virtual void Save<TConfiguration>(TConfiguration value)
			where TConfiguration : class, IConfiguration
			=> As<TConfiguration>().Save(value);

		/// <summary>
		/// return a copy of actual <typeparamref name="TConfiguration" />
		/// </summary>
		/// <typeparam name="TConfiguration"></typeparam>
		/// <returns></returns>
		public virtual TConfiguration Copy<TConfiguration>()
			where TConfiguration : class, IConfiguration
			=> As<TConfiguration>().Copy();

		/// <summary>
		/// return a read only configuration
		/// </summary>
		/// <typeparam name="TConfiguration"></typeparam>
		/// <returns></returns>
		public virtual TConfiguration Readonly<TConfiguration>()
			where TConfiguration : class, IConfiguration
			=> As<TConfiguration>().Readonly();

		/// <summary>
		/// Method return a copy of actual <typeparamref name="TConfiguration" /> with <see cref="INotifyingConfiguration" /> implementation
		/// </summary>
		/// <typeparam name="TConfiguration">type of <see cref="IConfiguration" /> to return</typeparam>
		/// <returns><see cref="INotifyingConfiguration" /> implementation of actual <typeparamref name="TConfiguration" /></returns>
		public virtual TConfiguration Notifying<TConfiguration>()
			where TConfiguration : class, IConfiguration
			=> As<TConfiguration>().Notifying();

		/// <summary>
		/// Method return actual <typeparamref name="TConfiguration" /> with <see cref="IRealtimeConfiguration" /> implementation
		/// </summary>
		/// <typeparam name="TConfiguration"></typeparam>
		/// <returns></returns>
		public virtual TConfiguration Realtime<TConfiguration>()
			where TConfiguration : class, IConfiguration
			=> As<TConfiguration>().Realtime();

		public IConfigurator<TConfiguration> As<TConfiguration>()
			where TConfiguration : class, IConfiguration
			=> (IConfigurator<TConfiguration>) _dict.GetOrAdd(typeof(TConfiguration), CreateTypedConfigurator<TConfiguration>());

		public IConfigurator<TKey, TConfiguration> As<TKey, TConfiguration>()
			where TConfiguration : class, IConfiguration
			=> As<TConfiguration>().As<TKey>();

		public IConfiguration[] Get() => _dict.Values.Select(c => c.GetCopy()).ToArray();

		public void Set(params IConfiguration[] configurations) {
			throw new NotImplementedException();
		}

		protected virtual TConfiguration Load<TConfiguration>()
			where TConfiguration : class, IConfiguration
			=> CreateDefault<TConfiguration>();

		protected virtual TConfiguration CreateDefault<TConfiguration>()
			where TConfiguration : class, IConfiguration {
			var cfg = (TConfiguration) Activator.CreateInstance(TmpCfgClass<TConfiguration>.RealtimeType);
			Subscribe(cfg);
			return cfg;
		}

		protected virtual void Subscribe<TConfiguration>(TConfiguration cfg)
			where TConfiguration : class, IConfiguration {
			var real = (IRealtimeConfiguration) cfg;
			real.PropertyChanged += ConfigurationChanged<TConfiguration>;
		}

		protected virtual void ConfigurationChanged<TConfiguration>(object sender, PropertyChangedEventArgs e)
			where TConfiguration : class, IConfiguration {
			//
		}

		protected Type GetSimpleType<TConfiguration>()
			where TConfiguration : class, IConfiguration
			=> typeof(TConfiguration).IsInterface
				? TmpCfgClass<TConfiguration>.CopyType
				: typeof(TConfiguration);

		protected virtual IConfigurator<TConfiguration> CreateTypedConfigurator<TConfiguration>()
			where TConfiguration : class, IConfiguration
			=> new MemoryConfigurator<TConfiguration>();

		private readonly ConcurrentDictionary<Type, IConfigProvider> _dict
			= new ConcurrentDictionary<Type, IConfigProvider>();
	}

	public class MemoryConfigurator<TConfiguration>
		: IConfigurator<TConfiguration> where TConfiguration : class, IConfiguration {
		public virtual bool IsExist() {
			lock (_guard) {
				return !EqualityComparer<TConfiguration>.Default.Equals(Value, default);
			}
		}

		public virtual void Reset() {
			lock (_guard) {
				Save(CreateDefault());
			}
		}

		public void Save(IConfiguration value) => Get().Copy(value);

		public IConfiguration GetCopy()
			=> Copy();

		public INotifyingConfiguration GetNotifying()
			=> (INotifyingConfiguration) Notifying();

		public IReadonlyConfiguration GetReadonly()
			=> (IReadonlyConfiguration) Readonly();

		public IRealtimeConfiguration GetRealtime()
			=> (IRealtimeConfiguration) Realtime();

		public virtual void Save(TConfiguration value)
			=> Get().Copy(value);

		public virtual TConfiguration Copy()
			=> (TConfiguration) Activator.CreateInstance(TmpCfgClass<TConfiguration>.CopyType, Get());

		public virtual TConfiguration Notifying()
			=> (TConfiguration) Activator.CreateInstance(TmpCfgClass<TConfiguration>.NotifyingType, Get());

		public virtual TConfiguration Readonly()
			=> (TConfiguration) Activator.CreateInstance(TmpCfgClass<TConfiguration>.ReadonlyType, Get());

		public virtual TConfiguration Realtime()
			=> Get();

		public virtual IConfigurator<TKey, TConfiguration> As<TKey>()
			=> (IConfigurator<TKey, TConfiguration>) _dict.GetOrAdd(typeof(TKey), CreateKeysConfigurator<TKey>());

		protected virtual IConfigurator<TKey, TConfiguration> CreateKeysConfigurator<TKey>()
			=> new MemoryConfigurator<TKey, TConfiguration>();

		protected virtual TConfiguration Get() {
			lock (_guard) {
				return IsExist()
					? Value
					: Value = Load();
			}
		}

		protected virtual TConfiguration Load()
			=> CreateDefault();

		protected virtual TConfiguration CreateDefault() {
			var cfg = (TConfiguration) Activator.CreateInstance(TmpCfgClass<TConfiguration>.RealtimeType);
			Subscribe(cfg);
			return cfg;
		}

		protected virtual void Subscribe(TConfiguration cfg)
			=> ((IRealtimeConfiguration) cfg).PropertyChanged += ConfigurationChanged;

		protected virtual void ConfigurationChanged(object sender, PropertyChangedEventArgs e) {
			//
		}

		private readonly object _guard = new object();
		protected TConfiguration Value = default;

		private readonly ConcurrentDictionary<Type, object> _dict
			= new ConcurrentDictionary<Type, object>();
	}

	public class MemoryConfigurator<TKey, TConfiguration>
		: IConfigurator<TKey, TConfiguration> where TConfiguration : class,  IConfiguration {
		public TConfiguration this[TKey key] {
			get => Get(key);
			set => Save(key, value);
		}

		public virtual bool IsExist(TKey key)
			=> _cache.ContainsKey(key);

		public virtual void Reset(TKey key) {
			if (_cache.TryRemove(key, out var removed))
				Unsubscribe(key, removed.Configuration, removed.Handler);
		}

		public virtual void Save(TKey key, TConfiguration value)
			=> Get(key).Copy(value);

		public virtual TConfiguration Copy(TKey key)
			=> (TConfiguration) Activator.CreateInstance(TmpCfgClass<TConfiguration>.CopyType, Get(key));

		public virtual TConfiguration Notifying(TKey key)
			=> (TConfiguration) Activator.CreateInstance(TmpCfgClass<TConfiguration>.NotifyingType, Get(key));

		public virtual TConfiguration Readonly(TKey key)
			=> (TConfiguration) Activator.CreateInstance(TmpCfgClass<TConfiguration>.ReadonlyType, Get(key));

		public virtual TConfiguration Realtime(TKey key)
			=> Get(key);

		public IEnumerator<TConfiguration> GetEnumerator()
			=> _cache.Values.Select(i => i.Configuration).GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator()
			=> GetEnumerator();

		protected virtual TConfiguration Get(TKey key)
			=> _cache.GetOrAdd(key, SetCacheValue(key)).Configuration;

		protected virtual (TConfiguration, PropertyChangedEventHandler) SetCacheValue(TKey key) {
			var cfg = Load(key);
			return (cfg, Subscribe(key, cfg));
		}

		protected virtual TConfiguration Load(TKey key)
			=> CreateDefault(key);

		protected virtual TConfiguration CreateDefault(TKey key)
			=> (TConfiguration) Activator.CreateInstance(TmpCfgClass<TConfiguration>.RealtimeType);

		protected virtual PropertyChangedEventHandler Subscribe(TKey key, TConfiguration cfg) {
			PropertyChangedEventHandler handler = (s, e) => ConfigurationChanged(key, cfg, e.PropertyName);
			Subscribe(key, cfg, handler);
			return handler;
		}

		protected virtual void Subscribe(TKey key, TConfiguration cfg, PropertyChangedEventHandler handler) {
			((IRealtimeConfiguration) cfg).PropertyChanged += handler;
		}

		protected virtual void Unsubscribe(TKey key, TConfiguration cfg, PropertyChangedEventHandler handler) {
			((IRealtimeConfiguration) cfg).PropertyChanged -= handler;
		}

		protected virtual void ConfigurationChanged(TKey key, TConfiguration configuration, string propertyName) {
			//
		}

		private readonly ConcurrentDictionary<TKey, (TConfiguration Configuration, PropertyChangedEventHandler Handler)>
			_cache
				= new ConcurrentDictionary<TKey, (TConfiguration Configuration, PropertyChangedEventHandler Handler)>();
	}
}