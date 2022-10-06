using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

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
	
	/// <inheritdoc />
	protected override void OnListChanged(NotifyCollectionChangedEventArgs args) {
		base.OnListChanged(args);
		_settings.OnPropertyChanged(_args);
	}
}

