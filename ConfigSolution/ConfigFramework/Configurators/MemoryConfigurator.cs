﻿using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;

namespace ArtZilla.Config.Configurators {
	public class MemoryConfigurator: IConfigurator {
		public bool IsExist<TConfiguration>()
			where TConfiguration : IConfiguration
			=> As<TConfiguration>().IsExist();

		/// <summary>
		/// Reset <typeparamref name="TConfiguration" /> to default values
		/// </summary>
		/// <typeparam name="TConfiguration"></typeparam>
		public virtual void Reset<TConfiguration>()
			where TConfiguration : IConfiguration
			=> As<TConfiguration>().Reset();
		// => Save(CreateDefault<TConfiguration>());

		/// <summary>
		/// Save <paramref name="value" /> as <typeparamref name="TConfiguration" />
		/// </summary>
		/// <typeparam name="TConfiguration"></typeparam>
		/// <param name="value"></param>
		public virtual void Save<TConfiguration>(TConfiguration value)
			where TConfiguration : IConfiguration
			=> As<TConfiguration>().Save(value);
		// => Get<TConfiguration>().Copy(value);

		/// <summary>
		/// return a copy of actual <typeparamref name="TConfiguration" />
		/// </summary>
		/// <typeparam name="TConfiguration"></typeparam>
		/// <returns></returns>
		public virtual TConfiguration Copy<TConfiguration>()
			where TConfiguration : IConfiguration
			=> As<TConfiguration>().Copy();
		// => (TConfiguration)Activator.CreateInstance(TmpCfgClass<TConfiguration>.CopyType,	Get<TConfiguration>());

		/// <summary>
		/// return a read only configuration
		/// </summary>
		/// <typeparam name="TConfiguration"></typeparam>
		/// <returns></returns>
		public virtual TConfiguration Readonly<TConfiguration>()
			where TConfiguration : IConfiguration
			=> As<TConfiguration>().Readonly();
		// => (TConfiguration)Activator.CreateInstance(TmpCfgClass<TConfiguration>.ReadonlyType, Get<TConfiguration>());

		/// <summary>
		/// Method return a copy of actual <typeparamref name="TConfiguration" /> with <see cref="INotifyingConfiguration" /> implementation
		/// </summary>
		/// <typeparam name="TConfiguration">type of <see cref="IConfiguration" /> to return</typeparam>
		/// <returns><see cref="INotifyingConfiguration" /> implementation of actual <typeparamref name="TConfiguration" /></returns>
		public virtual TConfiguration Notifying<TConfiguration>()
			where TConfiguration : IConfiguration
			=> As<TConfiguration>().Notifying();
		// => (TConfiguration)Activator.CreateInstance(TmpCfgClass<TConfiguration>.NotifyingType, Get<TConfiguration>());

		/// <summary>
		/// Method return actual <typeparamref name="TConfiguration" /> with <see cref="IRealtimeConfiguration" /> implementation
		/// </summary>
		/// <typeparam name="TConfiguration"></typeparam>
		/// <returns></returns>
		public virtual TConfiguration Realtime<TConfiguration>()
			where TConfiguration : IConfiguration
			=> As<TConfiguration>().Realtime();
		// => Get<TConfiguration>();
		// => (TConfiguration) Activator.CreateInstance(TmpCfgClass<TConfiguration>.RealtimeType, Get<TConfiguration>());

		public IConfigurator<TConfiguration> As<TConfiguration>()
			where TConfiguration : IConfiguration
			=> (IConfigurator<TConfiguration>) _dict.GetOrAdd(typeof(TConfiguration), CreateTypedConfigurator<TConfiguration>());

		public IConfigurator<TKey, TConfiguration> As<TKey, TConfiguration>()
			where TConfiguration : IConfiguration
			=> As<TConfiguration>().As<TKey>();

		protected virtual TConfiguration Load<TConfiguration>()
			where TConfiguration : IConfiguration
			=> CreateDefault<TConfiguration>();
		
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

		protected virtual IConfigurator<TConfiguration> CreateTypedConfigurator<TConfiguration>()
			where TConfiguration : IConfiguration
			=> new MemoryConfigurator<TConfiguration>();

		private readonly ConcurrentDictionary<Type, object> _dict
			= new ConcurrentDictionary<Type, object>();
	}

	public class MemoryConfigurator<TConfiguration>
		: IConfigurator<TConfiguration> where TConfiguration : IConfiguration {
		public virtual bool IsExist() {
			lock (_guard) {
				return !EqualityComparer<TConfiguration>.Default.Equals(Value, default);
			}
		}

		public virtual void Reset() {
			lock (_guard) {
				Value = default;
			}
		}

		public virtual void Save(TConfiguration value)
			=> Get().Copy(value);

		public virtual TConfiguration Copy()
			=> (TConfiguration)Activator.CreateInstance(TmpCfgClass<TConfiguration>.CopyType, Get());

		public virtual TConfiguration Notifying()
			=> (TConfiguration)Activator.CreateInstance(TmpCfgClass<TConfiguration>.NotifyingType, Get());

		public virtual TConfiguration Readonly()
			=> (TConfiguration)Activator.CreateInstance(TmpCfgClass<TConfiguration>.ReadonlyType, Get());

		public virtual TConfiguration Realtime()
			=> Get();

		public virtual IConfigurator<TKey, TConfiguration> As<TKey>()
			=> (IConfigurator<TKey, TConfiguration>)_dict.GetOrAdd(typeof(TKey), CreateKeysConfigurator<TKey>());

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
			var cfg = (TConfiguration)Activator.CreateInstance(TmpCfgClass<TConfiguration>.RealtimeType);
			Subscribe(cfg);
			return cfg;
		}

		protected virtual void Subscribe(TConfiguration cfg)
			=> ((IRealtimeConfiguration)cfg).PropertyChanged += ConfigurationChanged;

		protected virtual void ConfigurationChanged(object sender, PropertyChangedEventArgs e) {
			//
		}

		private readonly object _guard = new object();
		protected TConfiguration Value = default;

		private readonly ConcurrentDictionary<Type, object> _dict
			= new ConcurrentDictionary<Type, object>();
	}

	public class MemoryConfigurator<TKey, TConfiguration>
		: IConfigurator<TKey, TConfiguration> where TConfiguration : IConfiguration {

		public TConfiguration this[TKey key] {
			get => Get(key);
			set => Save(key, value);
		}

		public bool IsExist(TKey key)
			=> _cache.ContainsKey(key);

		public void Reset(TKey key) {
			if (_cache.TryRemove(key, out var removed))
				Unsubscribe(removed);
		}

		public void Save(TKey key, TConfiguration value)
			=> Get(key).Copy(value);

		public TConfiguration Copy(TKey key)
			=> (TConfiguration)Activator.CreateInstance(TmpCfgClass<TConfiguration>.CopyType, Get(key));

		public TConfiguration Notifying(TKey key)
			=> (TConfiguration)Activator.CreateInstance(TmpCfgClass<TConfiguration>.NotifyingType, Get(key));

		public TConfiguration Readonly(TKey key)
			=> (TConfiguration)Activator.CreateInstance(TmpCfgClass<TConfiguration>.ReadonlyType, Get(key));

		public TConfiguration Realtime(TKey key)
			=> Get(key);

		public IEnumerator<TConfiguration> GetEnumerator()
			=> _cache.Values.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator()
			=> GetEnumerator();

		protected virtual TConfiguration Load(TKey key)
			=> CreateDefault(key);

		protected virtual TConfiguration Get(TKey key)
			=> _cache.GetOrAdd(key, Load(key));

		protected virtual TConfiguration CreateDefault(TKey key) {
			var cfg = (TConfiguration)Activator.CreateInstance(TmpCfgClass<TConfiguration>.RealtimeType);
			Subscribe(cfg);
			return cfg;
		}

		protected virtual void Subscribe(TConfiguration cfg)
			=> ((IRealtimeConfiguration)cfg).PropertyChanged += ConfigurationChanged;

		protected virtual void Unsubscribe(TConfiguration cfg)
			=> ((IRealtimeConfiguration)cfg).PropertyChanged -= ConfigurationChanged;

		protected virtual void ConfigurationChanged(object sender, PropertyChangedEventArgs e) {
			//
		}

		private readonly ConcurrentDictionary<TKey, TConfiguration> _cache
			= new ConcurrentDictionary<TKey, TConfiguration>();
	}
}
