using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ArtZilla.Net.Config;

///
public interface ISettingsProviderItem {
	/// Source settings provider
	[System.Runtime.Serialization.IgnoreDataMember]
	[System.Xml.Serialization.XmlIgnore]
#if !NETSTANDARD2_0
	[System.Text.Json.Serialization.JsonIgnore]
#endif
	ISettingsProvider? Source { get; set; }

	/// Source settings provider
	[System.Runtime.Serialization.IgnoreDataMember]
	[System.Xml.Serialization.XmlIgnore]
#if !NETSTANDARD2_0
	[System.Text.Json.Serialization.JsonIgnore]
#endif
	string? SourceKey { get; set; }
}

/// Base settings interface
public interface ISettings : ISettingsProviderItem {
	///
	void Copy(ISettings source);
	
	/// Returns actual type of settings interface
	Type GetInterfaceType();

	/// Returns actual kind of underlying settings
	SettingsKind GetSettingsKind();
}


/// Mutable version of settings 
public interface ICopySettings : ISettings { }

/// Immutable version of settings
public interface IReadSettings : ISettings { }

/// Mutable version of settings that implemented INotifyPropertyChanged
public interface IInpcSettings : ISettings, INotifyPropertyChanged { }

/// 
public interface IRealSettings : IInpcSettings { }

/// 
public abstract class SettingsBase : ISettings {
	/// <inheritdoc />
	[System.Xml.Serialization.XmlIgnore]
	[System.Runtime.Serialization.IgnoreDataMember]
#if !NETSTANDARD2_0
	[System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.Always)]
#endif
	public ISettingsProvider? Source {
		get => _source;
		set => _source = value;
	}

	/// <inheritdoc />
	[System.Xml.Serialization.XmlIgnore]
	[System.Runtime.Serialization.IgnoreDataMember]
#if !NETSTANDARD2_0
	[System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.Always)]
#endif
	public string? SourceKey {
		get => _sourceKey;
		set => _sourceKey = value;
	}

	ISettingsProvider? _source;
	string? _sourceKey;

	/// <inheritdoc />
	public abstract Type GetInterfaceType();

	/// <inheritdoc />
	public abstract SettingsKind GetSettingsKind();

	/// <inheritdoc />
	public abstract void Copy(ISettings source);

#if !NETSTANDARD2_0
	/// <inheritdoc />
	public override string ToString()
		=> System.Text.Json.JsonSerializer.Serialize(this, GetInterfaceType(), _toString);

	/// returns settings as json string
	public virtual string ToJsonString()
		=> System.Text.Json.JsonSerializer.Serialize(this, GetInterfaceType(), _toJson);

	static readonly System.Text.Json.JsonSerializerOptions _toJson = new() {
		WriteIndented = true,
		DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault,
		Encoder = System.Text.Encodings.Web.JavaScriptEncoder.Create(System.Text.Unicode.UnicodeRanges.All),
		PropertyNameCaseInsensitive = true,
	};

	static readonly System.Text.Json.JsonSerializerOptions _toString = new() {
		WriteIndented = false,
		DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault,
		PropertyNameCaseInsensitive = true,
		Encoder = System.Text.Encodings.Web.JavaScriptEncoder.Create(System.Text.Unicode.UnicodeRanges.All),
	};
#endif
}

/// INPC abstract implementation of SettingsBase
public abstract class SettingsInpcBase : SettingsBase, IInpcSettings {
	/// <inheritdoc cref="INotifyPropertyChanged.PropertyChanged"/>
	public event PropertyChangedEventHandler? PropertyChanged;

	/// 
	/// <param name="propertyName"></param>
	internal virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
		=> PropertyChanged?.Invoke(this, new(propertyName));

	/// 
	internal virtual void OnPropertyChanged(PropertyChangedEventArgs args)
		=> PropertyChanged?.Invoke(this, args);

	/// 
	/// <param name="field"></param>
	/// <param name="value"></param>
	/// <param name="propertyName"></param>
	/// <typeparam name="T"></typeparam>
	/// <returns></returns>
	protected virtual bool Set<T>(ref T field, T value, [CallerMemberName] string? propertyName = null) {
		if (EqualityComparer<T>.Default.Equals(field, value)) return false;
		field = value;
		OnPropertyChanged(propertyName);
		return true;
	}

	/// 
	/// <param name="list"></param>
	/// <param name="values"></param>
	/// <param name="propertyName"></param>
	/// <typeparam name="T"></typeparam>
	public virtual void SetList<T>(IList<T> list, IList<T> values, [CallerMemberName] string? propertyName = null) {
		var isRaisePropertyChanged = false;
		var comparer = EqualityComparer<T>.Default;
		int i, count = values.Count;
		for (i = 0; i < list.Count && i < count;) {
			var item = list[i];
			var value = values[i];
			if (comparer.Equals(item, value))
				++i;
			else {
				list.RemoveAt(i);
				isRaisePropertyChanged = true;
			}
		}

		isRaisePropertyChanged |= list.Count > count;
		while (list.Count > count) 
			list.RemoveAt(count);

		isRaisePropertyChanged |= i < count;
		for (; i < count; ++i)
			list.Add(values[i]);

		if (isRaisePropertyChanged)
			OnPropertyChanged(propertyName);
	}
}