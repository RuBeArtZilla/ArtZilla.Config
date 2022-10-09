using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using CommunityToolkit.Diagnostics;

namespace ArtZilla.Net.Config;

/// <inheritdoc />
public interface IConfigList<T> : IList<T> {
	/// occurs when list changed
	event NotifyCollectionChangedEventHandler? ListChanged;
}

/// implementation of <see cref="IConfigList{T}"/>
/// <typeparam name="T"></typeparam>
public class ConfigList<T> : ObservableCollection<T>, IConfigList<T> {
	/// <inheritdoc />
	public ConfigList() { }

	/// <inheritdoc />
	public ConfigList(IEnumerable<T> collection) : base(collection) { }

	/// <inheritdoc />
	public ConfigList(List<T> list) : base(list) { }

	/// <inheritdoc />
	protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs args) {
		base.OnCollectionChanged(args);
		OnListChanged(args);
	}

	/// <inheritdoc />
	public event NotifyCollectionChangedEventHandler? ListChanged;

	/// when list changed
	protected virtual void OnListChanged(NotifyCollectionChangedEventArgs e) 
		=> ListChanged?.Invoke(this, e);
}


/// implementation of <see cref="IConfigList{T}"/>
/// <typeparam name="T"></typeparam>
public sealed class InpcConfigList<T> : ConfigList<T>, IConfigList<T> {
	readonly PropertyChangedEventArgs _args;
	readonly SettingsInpcBase _settings;
	
	/// <inheritdoc />
	public InpcConfigList(SettingsInpcBase settings, string name)
		=> (_settings, _args) = (settings, new(name));

	/// <inheritdoc />
	public InpcConfigList(SettingsInpcBase settings, string name, IEnumerable<T> collection) : base(collection)
		=> (_settings, _args) = (settings, new(name));

	/// <inheritdoc />
	public InpcConfigList(SettingsInpcBase settings, string name, List<T> list)
		: base(list) => (_settings, _args) = (settings, new(name));

	public void Set(IList<T> list) 
		=> _settings.SetList(this, list, _args.PropertyName); // todo: ...

	/// <inheritdoc />
	protected override void OnListChanged(NotifyCollectionChangedEventArgs args) {
		base.OnListChanged(args);
		_settings.OnPropertyChanged(_args);
	}
}

/// 
/// <typeparam name="TKey"></typeparam>
/// <typeparam name="TSettings"></typeparam>
public interface ISettingsDict<TKey, TSettings>
	: IReadOnlyDictionary<TKey, TSettings> 
		where TSettings : class, ISettings 
		where TKey : notnull {
	
	/// 
	/// <param name="key"></param>
	/// <returns></returns>
	TSettings AddNew(TKey key);

	/// 
	/// <param name="key"></param>
	/// <returns></returns>
	bool Delete(TKey key);
}

public class SettingsDict<TKey, TSettings> : ISettingsDict<TKey, TSettings> where TSettings : class, ISettings where TKey : notnull {
	/// <inheritdoc />
	public int Count => Map.Count;

	/// <inheritdoc />
	public IEnumerable<TKey> Keys => Map.Keys;

	/// <inheritdoc />
	public IEnumerable<TSettings> Values => Map.Values;

	/// <inheritdoc />
	public TSettings this[TKey key] => Map[key];
	
	protected ISettingsProvider Provider => Settings.Source!;

	///
	protected readonly SettingsKind Kind;
	
	///
	protected readonly SettingsBase Settings;
	
	///
	protected readonly Dictionary<TKey, TSettings> Map = new();

	public SettingsDict(SettingsBase settings) {
		Guard.IsNull(settings.SourceKey);
		Settings = settings;
		Kind = Settings.GetSettingsKind();
	}
	
	public SettingsDict(SettingsBase settings, ISettingsDict<TKey, TSettings> source) {
		Guard.IsNull(settings.SourceKey);
		Settings = settings;
		Kind = Settings.GetSettingsKind();
		
		// todo: copy source list;
	}

	/// <inheritdoc />
	public bool ContainsKey(TKey key) 
		=> Map.ContainsKey(key);

	/// <inheritdoc />
	public bool TryGetValue(TKey key, out TSettings value) 
		=> Map.TryGetValue(key, out value!);

	/// <inheritdoc />
	public IEnumerator<KeyValuePair<TKey, TSettings>> GetEnumerator()
		=> Map.GetEnumerator();

	/// <inheritdoc />
	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	
	/// <inheritdoc />
	public virtual TSettings AddNew(TKey key) {
		Guard.IsNotNull(key);
		var str = key.ToString();
		Guard.IsNotNull(str);
		var settings = Provider.Get<TSettings>(Kind, str);
		Map.Add(key, settings);
		return settings;
	}
	
	/// <inheritdoc />
	public virtual bool Delete(TKey key) {
		Guard.IsNotNull(key);
		var str = key.ToString();
		Guard.IsNotNull(str);
		var deleted = Provider.Delete<TSettings>(str);
		var removed = Map.Remove(key);
		return deleted && removed;
	}
}

public class InpcSettingsDict<TKey, TSettings> : SettingsDict<TKey, TSettings> where TSettings : class, ISettings where TKey : notnull {
	readonly SettingsInpcBase _settings;
	readonly PropertyChangedEventArgs _args;
	
	public InpcSettingsDict(SettingsInpcBase settings, string name) 
		: base(settings) 
		=> (_settings, _args) = (settings, new (name));

	public InpcSettingsDict(SettingsInpcBase settings, string name, ISettingsDict<TKey, TSettings> source) 
		: base(settings, source) 
		=> (_settings, _args) = (settings, new (name));
	
	/// <inheritdoc />
	public override TSettings AddNew(TKey key) {
		var settings = base.AddNew(key);
		RaisePropertyChanged();
		return settings;
	}

	/// <inheritdoc />
	public override bool Delete(TKey key) {
		var result = base.Delete(key);
		RaisePropertyChanged();
		return result;
	}
	
	public void Set(IReadOnlyDictionary<TKey, TSettings> dict) {
		// todo: ...
	}
	
	void RaisePropertyChanged()
		=> _settings.OnPropertyChanged(_args);
}  