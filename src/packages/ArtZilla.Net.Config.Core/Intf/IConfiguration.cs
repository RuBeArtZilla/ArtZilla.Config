using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Xml.Serialization;
#if !NETSTANDARD2_0
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Encodings.Web;
using System.Text.Unicode;
#endif

namespace ArtZilla.Net.Config;

/// Base configuration interface
public interface IConfiguration {
	void Copy(IConfiguration source);
}

/// Extended configuration interface
public interface ISettings : IConfiguration {
	/// Source settings provider
	[IgnoreDataMember, XmlIgnore]
#if !NETSTANDARD2_0
	[JsonIgnore]
#endif
	ISettingsProvider? Source { get; set; }

	/// Returns actual type of settings interface
	Type GetInterfaceType();

	/// Returns actual kind of underlying settings
	SettingsKind GetSettingsKind();

#if !NETSTANDARD2_0
	/// Returns formatted json string representation of this settings
	string ToJsonString();
#endif
}

/// Mutable version of settings 
public interface ICopySettings : ISettings { }

/// Immutable version of settings
public interface IReadSettings : ISettings, IReadonlyConfiguration { }

/// Mutable version of settings that implemented INotifyPropertyChanged
public interface IInpcSettings : ISettings, INotifyingConfiguration, INotifyPropertyChanged { }

/// 
public interface IRealSettings : IInpcSettings, IRealtimeConfiguration { }

public interface ISyncSettingsProvider {
	/// Gets a value indicating whether a settings exists.
	/// <param name="type"></param>
	/// <returns><see langword="true" /> if the settings exists; <see langword="false" /> if the settings does not exist.</returns>
	bool IsExist(Type type);

	bool Delete(Type type);

	void Reset(Type type);

	void Flush(Type? type = null);

	ISettings Get(Type type, SettingsKind kind);

	void Set(ISettings settings);
}

public interface IAsyncSettingsProvider {
	/// Gets a value indicating whether a settings exists.
	/// <param name="type"></param>
	/// <returns><see langword="true" /> if the settings exists; <see langword="false" /> if the settings does not exist.</returns>
	Task<bool> IsExistAsync(Type type);

	Task<bool> DeleteAsync(Type type);

	Task ResetAsync(Type type);

	Task FlushAsync(Type? type = null);

	Task<ISettings> GetAsync(Type type, SettingsKind kind);

	Task SetAsync(ISettings settings);
}

/// Settings provider
public interface ISettingsProvider : ISyncSettingsProvider, IAsyncSettingsProvider {
	void ThrowIfNotSupported(Type type);
}

public static class SettingsService {
	public static ISettingsProvider Provider
		=> _provider;

	static ISettingsProvider _provider;
}

public abstract class SettingsBase : ISettings {
	/// <inheritdoc />
	[IgnoreDataMember]
#if !NETSTANDARD2_0
	[JsonIgnore(Condition = JsonIgnoreCondition.Always)]
#endif
	public ISettingsProvider? Source {
		get => _source;
		set => _source = value;
	}

	ISettingsProvider? _source;

	/// <inheritdoc />
	public abstract Type GetInterfaceType();

	/// <inheritdoc />
	public abstract SettingsKind GetSettingsKind();

	/// <inheritdoc />
	public abstract void Copy(IConfiguration source);

#if !NETSTANDARD2_0
	/// <inheritdoc />
	public override string ToString()
		=> JsonSerializer.Serialize(this, GetInterfaceType(), _toString);

	/// <inheritdoc />
	public virtual string ToJsonString()
		=> JsonSerializer.Serialize(this, GetInterfaceType(), _toJson);

	static readonly JsonSerializerOptions _toJson = new() {
		WriteIndented = true,
		DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
		Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
		PropertyNameCaseInsensitive = true,
	};

	static readonly JsonSerializerOptions _toString = new() {
		WriteIndented = false,
		DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
		PropertyNameCaseInsensitive = true,
		Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
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
	protected virtual void SetList<T>(IList<T> list, IList<T> values, [CallerMemberName] string? propertyName = null) {
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

public interface ISettingsTypeConstructor {
	ISettings Create(Type intfType, SettingsKind kind, ISettings? source = null);

	Type GetType(Type intfType, SettingsKind kind);

	IConfiguration CreateOld(Type intfType, SettingsKind kind, IConfiguration? source = null);
}

public sealed class SameAssemblySettingsTypeConstructor : ISettingsTypeConstructor {
	/// <inheritdoc />
	public ISettings Create(Type intfType, SettingsKind kind, ISettings? source)
		=> GetRecord(intfType).Create(kind, source);

	/// <inheritdoc />
	public Type GetType(Type intfType, SettingsKind kind)
		=> GetRecord(intfType).Get(kind);

	Types GetRecord(Type intfType) {
		if (_map.TryGetValue(intfType, out var record))
			return record;

		AssertType(intfType);
		lock (_sync) {
			var intfName = intfType.FullName ?? intfType.Name;
			var (copyName, readName, inpcName, realName) = ConfigUtils.GenerateTypeNames(intfName);
			// var assembly = Assembly.GetAssembly(intfType); // should exist in the same assembly
			if (Assembly.GetAssembly(intfType) is not { } assembly)
				throw new($"Can't find assembly for type {intfType}");

			var copy = assembly.GetType(copyName, true)!;
			var read = assembly.GetType(readName, true)!;
			var inpc = assembly.GetType(inpcName, true)!;
			var real = assembly.GetType(realName, true)!;

			var map = new Dictionary<Type, Types>(_map);
			record = new(copy, read, inpc, real);
			map[intfType] = record;

			// do not change original collection, only swap reference to new
			_map = map;
			return record;
		}
	}

	static void AssertType(Type intfType) {
		if (!intfType.IsInterface)
			throw new($"Type {intfType} is not interface");
		if (!typeof(ISettings).IsAssignableFrom(intfType))
			throw new($"Type {intfType} is not implement ISettings interface");
	}

	/// <inheritdoc />
	public IConfiguration CreateOld(Type intfType, SettingsKind kind, IConfiguration? source)
		=> Create(intfType, kind, source as ISettings);

	record Types(Type Copy, Type Read, Type Inpc, Type Real) {
		public readonly Type Copy = Copy;
		public readonly Type Read = Read;
		public readonly Type Inpc = Inpc;
		public readonly Type Real = Real;

		public Type Get(SettingsKind kind)
			=> kind switch {
				SettingsKind.Copy => Copy,
				SettingsKind.Read => Read,
				SettingsKind.Inpc => Inpc,
				SettingsKind.Real => Real,
				_ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
			};

		public ISettings Create(SettingsKind kind, ISettings? source)
			=> source is null
				? (ISettings)Activator.CreateInstance(Get(kind))!
				: (ISettings)Activator.CreateInstance(Get(kind), source)!;
	}

	readonly object _sync = new();
	Dictionary<Type, Types> _map = new();
}