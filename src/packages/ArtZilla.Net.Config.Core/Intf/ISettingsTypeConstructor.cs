using System.Reflection;

namespace ArtZilla.Net.Config;

/// Settings type constructor for interface
public interface ISettingsTypeConstructor {
	/// 
	/// <param name="intfType"></param>
	/// <param name="kind"></param>
	/// <param name="source"></param>
	/// <returns></returns>
	ISettings Create(Type intfType, SettingsKind kind, ISettings? source = null);
	
	/// 
	/// <param name="intfType"></param>
	/// <param name="kind"></param>
	/// <returns></returns>
	Type GetType(Type intfType, SettingsKind kind);
}

/// 
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