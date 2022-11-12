using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Diagnostics;

namespace ArtZilla.Net.Config;

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
	public int Count
		=> Map.Count;

	/// <inheritdoc />
	public IEnumerable<TKey> Keys
		=> Map.Keys;

	/// <inheritdoc />
	public IEnumerable<TSettings> Values
		=> Map.Values;

	/// <inheritdoc />
	public TSettings this[TKey key]
		=> Map[key];

	///
	protected readonly SettingsKind Kind;
	
	///
	protected readonly SettingsBase Settings;
	
	///
	protected readonly Dictionary<TKey, TSettings?> Map = new();
	
	///
	protected ISettingsProvider Provider 
		=> Settings.Source!;

	public SettingsDict(SettingsBase settings) {
		// Guard.IsNotNull(settings.Source);
		Guard.IsNull(settings.SourceKey);
		Settings = settings;
		Kind = Settings.GetSettingsKind();
	}
	
	public SettingsDict(SettingsBase settings, ISettingsDict<TKey, TSettings> source) {
		Guard.IsNotNull(settings.Source);
		Guard.IsNull(settings.SourceKey);
		Settings = settings;
		Kind = Settings.GetSettingsKind();

		foreach (var (key, value) in source) {
			var str = KeyToString(key);
			var item = Provider.Get<TSettings>(Kind, str);
			// item.Copy(value);
			Map[key] = item;
		}
	}

	///
	/// <param name="values"></param>
	public void SetKeys(TKey[] values) {
		var keys = Map.Keys.ToArray();
		var comparer = EqualityComparer<TKey>.Default;
		foreach (var key in keys) {
			if (values.Contains(key))
				continue;

			Delete(key);
		}

		foreach (var key in values) {
			if (keys.Contains(key))
				continue;

			Map.Add(key, null);
		}
	}

	/// <inheritdoc />
	public bool ContainsKey(TKey key) 
		=> Map.ContainsKey(key);

	/// <inheritdoc />
#if NET60
	public bool TryGetValue(TKey key, [NotNullWhen(true)] out TSettings? value) {
#else
	public bool TryGetValue(TKey key, out TSettings? value) {
#endif
		if (!Map.TryGetValue(key, out value))
			return false;

		value ??= AddNew(key);
		return true;
	}

	/// <inheritdoc />
	public IEnumerator<KeyValuePair<TKey, TSettings>> GetEnumerator()
		=> Map.GetEnumerator();

	/// <inheritdoc />
	IEnumerator IEnumerable.GetEnumerator()
		=> GetEnumerator();
	
	/// <inheritdoc />
	public virtual TSettings AddNew(TKey key) {
		var settings = Load(key);
		Guard.IsNotNull(settings);
		Map.Add(key, settings);
		return settings;
	}
	
	/// <inheritdoc />
	public virtual bool Delete(TKey key) {
		var str = KeyToString(key);
		var deleted = Provider.Delete<TSettings>(str);
		var removed = Map.Remove(key);
		return deleted && removed;
	}

	///
	protected string KeyToString(TKey key) {
		Guard.IsNotNull(key);
		var str = key.ToString();
		Guard.IsNotNull(str);
		return str;
	}

	///
	protected virtual TSettings Load(TKey key) {
		var str = KeyToString(key);
		var settings = Provider.Get<TSettings>(Kind, str);
		return settings;
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
	
	public void Set(IReadOnlyDictionary<TKey, TSettings> source) {
		var toDelete = Map.Keys.ToList().Where(key => !source.ContainsKey(key));
		foreach (var key in toDelete)
			Delete(key);

		foreach (var key in source.Keys.ToList()) {
			if (!source.TryGetValue(key, out var srcSettings)) {
				Debug.Fail("wtf?");
				continue;
			}

			if (!TryGetValue(key, out var dstSettings))
				dstSettings = AddNew(key);

			Guard.IsNotNull(dstSettings);
			dstSettings.Copy(srcSettings);
		}
	}
	
	void RaisePropertyChanged()
		=> _settings.OnPropertyChanged(_args);
}  