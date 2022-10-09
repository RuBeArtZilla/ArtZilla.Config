using System.Collections;
using System.Collections.Concurrent;
using System.ComponentModel;
using ArtZilla.Net.Config.Builders;

namespace ArtZilla.Net.Config.Configurators;

public class MemoryConfigurator : IConfigurator {
	public bool IsExist<TConfiguration>()
		where TConfiguration : class, IConfiguration
		=> As<TConfiguration>().IsExist();

	/// <inheritdoc />
	public virtual void Reset<TConfiguration>()
		where TConfiguration : class, IConfiguration
		=> As<TConfiguration>().Reset();

	public void Clear() => _dict.Clear();

	/// <inheritdoc />
	public virtual void Save<TConfiguration>(TConfiguration value)
		where TConfiguration : class, IConfiguration
		=> As<TConfiguration>().Save(value);

	/// <inheritdoc />
	public virtual TConfiguration Copy<TConfiguration>()
		where TConfiguration : class, IConfiguration
		=> As<TConfiguration>().Copy();

	/// <inheritdoc />
	public virtual TConfiguration Readonly<TConfiguration>()
		where TConfiguration : class, IConfiguration
		=> As<TConfiguration>().Readonly();

	/// <inheritdoc />
	public virtual TConfiguration Notifying<TConfiguration>()
		where TConfiguration : class, IConfiguration
		=> As<TConfiguration>().Notifying();

	/// <inheritdoc />
	public virtual TConfiguration Realtime<TConfiguration>()
		where TConfiguration : class, IConfiguration
		=> As<TConfiguration>().Realtime();

	/// <inheritdoc />
	public IConfigurator<TConfiguration> As<TConfiguration>()
		where TConfiguration : class, IConfiguration
		=> (IConfigurator<TConfiguration>)_dict.GetOrAdd(typeof(TConfiguration), CreateTypedConfigurator<TConfiguration>());

	/// <inheritdoc />
	public IConfigurator<TKey, TConfiguration> As<TKey, TConfiguration>()
		where TConfiguration : class, IConfiguration
		=> As<TConfiguration>().As<TKey>();

	/// <inheritdoc />
	public IConfiguration[] Get()
		=> _dict.Values.Select(c => c.GetCopy()).ToArray();

	/// <inheritdoc />
	public void Set(params IConfiguration[] configurations) {
		throw new NotImplementedException();
	}

	/// <inheritdoc />
	public void CloneTo(IConfigurator destination) {
		destination.Clear();
		foreach (var pair in _dict) {
			var configuration = destination.GetType()
				.GetMethod(nameof(IConfigurator.Readonly))
				.MakeGenericMethod(pair.Key)
				.Invoke(destination, null);

			typeof(MemoryConfigurator)
				.GetMethod(nameof(IConfigurator.Save))
				.MakeGenericMethod(pair.Key)
				.Invoke(this, new[] { configuration });
		}
	}

	protected virtual TConfiguration Load<TConfiguration>()
		where TConfiguration : class, IConfiguration
		=> CreateDefault<TConfiguration>();

	protected virtual TConfiguration CreateDefault<TConfiguration>()
		where TConfiguration : class, IConfiguration {
		var cfg = (TConfiguration)Activator.CreateInstance(TmpCfgClass<TConfiguration>.RealtimeType);
		Subscribe(cfg);
		return cfg;
	}

	protected virtual void Subscribe<TConfiguration>(TConfiguration cfg)
		where TConfiguration : class, IConfiguration {
		var real = (IRealtimeConfiguration)cfg;
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

	readonly ConcurrentDictionary<Type, IConfigProvider> _dict
		= new ConcurrentDictionary<Type, IConfigProvider>();
}

public class MemoryConfigurator<TConfiguration>
	: IConfigurator<TConfiguration> where TConfiguration : class, IConfiguration {
	/// <inheritdoc />
	public virtual bool IsExist() {
		lock (_guard)
			return !EqualityComparer<TConfiguration>.Default.Equals(Value, default);
	}

	/// <inheritdoc />
	public virtual void Reset() {
		lock (_guard) 
			Save(CreateDefault());
	}

	/// <inheritdoc />
	public void Save(IConfiguration value)
		=> Get().Copy(value);

	/// <inheritdoc />
	public IConfiguration GetCopy()
		=> Copy();

	/// <inheritdoc />
	public INotifyingConfiguration GetNotifying()
		=> (INotifyingConfiguration)Notifying();

	/// <inheritdoc />
	public IReadonlyConfiguration GetReadonly()
		=> (IReadonlyConfiguration)Readonly();

	/// <inheritdoc />
	public IRealtimeConfiguration GetRealtime()
		=> (IRealtimeConfiguration)Realtime();

	/// <inheritdoc />
	public virtual void Save(TConfiguration value)
		=> Get().Copy(value);

	/// <inheritdoc />
	public virtual TConfiguration Copy()
		=> (TConfiguration)Activator.CreateInstance(TmpCfgClass<TConfiguration>.CopyType, Get());
		// => _constructor.CreateCopyOld(Get());

	/// <inheritdoc />
	public virtual TConfiguration Notifying()
		=> (TConfiguration)Activator.CreateInstance(TmpCfgClass<TConfiguration>.NotifyingType, Get());
		// => _constructor.CreateInpcOld(Get());

	/// <inheritdoc />
	public virtual TConfiguration Readonly()
		=> (TConfiguration)Activator.CreateInstance(TmpCfgClass<TConfiguration>.ReadonlyType, Get());
		// => _constructor.CreateReadOld(Get());

	/// <inheritdoc />
	public virtual TConfiguration Realtime()
		=> Get();

	/// <inheritdoc />
	public virtual IConfigurator<TKey, TConfiguration> As<TKey>()
		=> (IConfigurator<TKey, TConfiguration>)_dict.GetOrAdd(typeof(TKey), CreateKeysConfigurator<TKey>());

	protected virtual IConfigurator<TKey, TConfiguration> CreateKeysConfigurator<TKey>()
		=> new MemoryConfigurator<TKey, TConfiguration>();

	protected virtual TConfiguration Get() {
		lock (_guard)
			return IsExist()
				? Value
				: Value = Load();
	}

	protected virtual TConfiguration Load()
		=> CreateDefault();

	protected virtual TConfiguration CreateDefault() {
		var cfg = (TConfiguration)Activator.CreateInstance(TmpCfgClass<TConfiguration>.RealtimeType);
		// var cfg = _constructor.CreateRealOld<TConfiguration>();
		Subscribe(cfg);
		return cfg;
	}

	protected virtual void Subscribe(TConfiguration cfg)
		=> ((IRealtimeConfiguration)cfg).PropertyChanged += ConfigurationChanged;

	protected virtual void ConfigurationChanged(object sender, PropertyChangedEventArgs e) {
		//
	}

	ISettingsTypeConstructor _constructor = new SameAssemblySettingsTypeConstructor();
	protected TConfiguration? Value;
	readonly object _guard = new ();
	readonly ConcurrentDictionary<Type, object> _dict = new();
}

public class MemoryConfigurator<TKey, TConfiguration>
	: IConfigurator<TKey, TConfiguration> where TConfiguration : class, IConfiguration {
	/// <inheritdoc />
	public TConfiguration this[TKey key]
	{
		get => Get(key);
		set => Save(key, value);
	}

	/// <inheritdoc />
	public virtual bool IsExist(TKey key)
		=> _cache.ContainsKey(key);

	/// <inheritdoc />
	public virtual void Reset(TKey key) {
		if (_cache.TryRemove(key, out var removed))
			Unsubscribe(key, removed.Configuration, removed.Handler);
	}

	/// <inheritdoc />
	public virtual void Save(TKey key, TConfiguration value)
		=> Get(key).Copy(value);

	/// <inheritdoc />
	public virtual TConfiguration Copy(TKey key)
		=> (TConfiguration)Activator.CreateInstance(TmpCfgClass<TConfiguration>.CopyType, Get(key));

	/// <inheritdoc />
	public virtual TConfiguration Notifying(TKey key)
		=> (TConfiguration)Activator.CreateInstance(TmpCfgClass<TConfiguration>.NotifyingType, Get(key));

	/// <inheritdoc />
	public virtual TConfiguration Readonly(TKey key)
		=> (TConfiguration)Activator.CreateInstance(TmpCfgClass<TConfiguration>.ReadonlyType, Get(key));

	/// <inheritdoc />
	public virtual TConfiguration Realtime(TKey key)
		=> Get(key);

	/// <inheritdoc />
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
		=> (TConfiguration)Activator.CreateInstance(TmpCfgClass<TConfiguration>.RealtimeType);

	protected virtual PropertyChangedEventHandler Subscribe(TKey key, TConfiguration cfg) {
		PropertyChangedEventHandler handler = (s, e) => ConfigurationChanged(key, cfg, e.PropertyName);
		Subscribe(key, cfg, handler);
		return handler;
	}

	protected virtual void Subscribe(TKey key, TConfiguration cfg, PropertyChangedEventHandler handler) {
		((IRealtimeConfiguration)cfg).PropertyChanged += handler;
	}

	protected virtual void Unsubscribe(TKey key, TConfiguration cfg, PropertyChangedEventHandler handler) {
		((IRealtimeConfiguration)cfg).PropertyChanged -= handler;
	}

	protected virtual void ConfigurationChanged(TKey key, TConfiguration configuration, string propertyName) {
		//
	}

	ISettingsTypeConstructor _constructor = new SameAssemblySettingsTypeConstructor();
	readonly ConcurrentDictionary<TKey, (TConfiguration Configuration, PropertyChangedEventHandler Handler)>
		_cache = new();
}