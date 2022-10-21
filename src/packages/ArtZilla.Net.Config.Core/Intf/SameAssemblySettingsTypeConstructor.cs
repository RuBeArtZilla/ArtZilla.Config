using System.Reflection;

namespace ArtZilla.Net.Config;

/// 
public sealed class SameAssemblySettingsTypeConstructor : ISettingsTypeConstructor {
	/// <inheritdoc />
	public ISettings Create(Type intfType, SettingsKind kind, ISettingsProvider? provider, string? key, bool isInit = false)
		=> GetRecord(intfType).Create(kind, isInit, provider, key);

	/// <inheritdoc />
	public ISettings Create(Type intfType, SettingsKind kind, ISettingsProvider? provider, string? key, ISettings? source = null)
		=> GetRecord(intfType).Create(kind, source, provider, key);

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
			record = new(intfType, copy, read, inpc, real);
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

	record Types {
		public readonly Type Copy;
		public readonly Type Read;
		public readonly Type Inpc;
		public readonly Type Real;

		readonly ConstructorInfo _copyCtor1;
		readonly ConstructorInfo _copyCtor2;
		readonly ConstructorInfo _readCtor1;
		readonly ConstructorInfo _readCtor2;
		readonly ConstructorInfo _inpcCtor1;
		readonly ConstructorInfo _inpcCtor2;
		readonly ConstructorInfo _realCtor1;
		readonly ConstructorInfo _realCtor2;

		static readonly Type[] Args1 = { typeof(bool), typeof(ISettingsProvider), typeof(string) };

		public Types(Type intf, Type copy, Type read, Type inpc, Type real) {
			Copy = copy;
			Read = read;
			Inpc = inpc;
			Real = real;

			var args2 = new[] { intf, typeof(ISettingsProvider), typeof(string) };
			_copyCtor1 = copy.GetConstructor(Args1) ?? throw new InvalidOperationException();
			_copyCtor2 = copy.GetConstructor(args2) ?? throw new InvalidOperationException();
			_readCtor1 = read.GetConstructor(Args1) ?? throw new InvalidOperationException();
			_readCtor2 = read.GetConstructor(args2) ?? throw new InvalidOperationException();
			_inpcCtor1 = inpc.GetConstructor(Args1) ?? throw new InvalidOperationException();
			_inpcCtor2 = inpc.GetConstructor(args2) ?? throw new InvalidOperationException();
			_realCtor1 = real.GetConstructor(Args1) ?? throw new InvalidOperationException();
			_realCtor2 = real.GetConstructor(args2) ?? throw new InvalidOperationException();
		}

		public Type Get(SettingsKind kind)
			=> kind switch {
				SettingsKind.Copy => Copy,
				SettingsKind.Read => Read,
				SettingsKind.Inpc => Inpc,
				SettingsKind.Real => Real,
				_ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
			};

		public ISettings Create(SettingsKind kind, bool isInit)
			=> CreateEx(kind, isInit: isInit);


		public ISettings Create(SettingsKind kind, bool isInit, ISettingsProvider? provider, string? key)
			=> CreateEx(kind, null, provider, key, isInit);

		public ISettings Create(SettingsKind kind, ISettings? source, ISettingsProvider? provider, string? key)
			=> CreateEx(kind, source, provider, key);

		ISettings CreateEx(SettingsKind kind, ISettings? source = null, ISettingsProvider? provider = null, string? key = null, bool isInit = false) {
			if (source is null) {
				var args = new object?[] { isInit, provider, key };
				return kind switch {
					SettingsKind.Copy => (ISettings) _copyCtor1.Invoke(args),
					SettingsKind.Read => (ISettings) _readCtor1.Invoke(args),
					SettingsKind.Inpc => (ISettings) _inpcCtor1.Invoke(args),
					SettingsKind.Real => (ISettings) _realCtor1.Invoke(args),
					_ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
				};
			} else {
				var args = new object?[] { source, provider, key };
				return kind switch {
					SettingsKind.Copy => (ISettings) _copyCtor2.Invoke(args),
					SettingsKind.Read => (ISettings) _readCtor2.Invoke(args),
					SettingsKind.Inpc => (ISettings) _inpcCtor2.Invoke(args),
					SettingsKind.Real => (ISettings) _realCtor2.Invoke(args),
					_ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
				};
			}
		}

		public void Deconstruct(out Type copy, out Type read, out Type inpc, out Type real) {
			copy = Copy;
			read = Read;
			inpc = Inpc;
			real = Real;
		}
	}

	readonly object _sync = new();
	Dictionary<Type, Types> _map = new();

}