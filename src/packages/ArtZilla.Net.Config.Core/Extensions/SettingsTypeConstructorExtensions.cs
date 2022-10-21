namespace ArtZilla.Net.Config;

/// 
public static class SettingsTypeConstructorExtensions {
	///
	public static ISettings Clone(this ISettingsTypeConstructor ctor, ISettings source, SettingsKind kind = SettingsKind.Copy)
		=> ctor.Create(source.GetInterfaceType(), kind, source.Source, source.SourceKey, source);

	///
	public static T Clone<T>(this ISettingsTypeConstructor ctor, ISettings source, SettingsKind kind = SettingsKind.Copy)
		where T : ISettings => (T) ctor.Clone(source, kind);

	///
	public static ICopySettings CloneCopy(this ISettingsTypeConstructor ctor, ISettings source)
		=> (ICopySettings) ctor.Clone(source, SettingsKind.Copy);

	///
	public static IRealSettings CloneReal(this ISettingsTypeConstructor ctor, ISettings source)
		=> (IRealSettings) ctor.Clone(source, SettingsKind.Real);

	///
	public static ISettings Default(this ISettingsTypeConstructor ctor, Type type, SettingsKind kind = SettingsKind.Copy, ISettingsProvider? provider = null, string? key = null)
		=> ctor.Create(type, kind, provider, key, true);

	///
	public static T Default<T>(this ISettingsTypeConstructor ctor, SettingsKind kind = SettingsKind.Copy, ISettingsProvider? provider = null, string? key = null)
		where T : ISettings => (T) ctor.Default(typeof(T), kind, provider, key);

	///
	public static IRealSettings DefaultReal(this ISettingsTypeConstructor ctor, Type type, ISettingsProvider? provider, string? key)
		=> (IRealSettings) ctor.Default(type, SettingsKind.Real, provider, key);

	///
	public static IReadSettings DefaultRead(this ISettingsTypeConstructor ctor, Type type, ISettingsProvider? provider, string? key)
		=> (IReadSettings) ctor.Default(type, SettingsKind.Read, provider, key);

	/*
	/// 
	public static T Create<T>(this ISettingsTypeConstructor ctor, SettingsKind kind) where T : ISettings
		=> (T) ctor.Create(typeof(T), kind);
	
	/// 
	public static T Create<T>(this ISettingsTypeConstructor ctor, SettingsKind kind, T source) where T : class, ISettings
		=> (T) ctor.Create(typeof(T), kind, source);
	
	/// 
	public static ICopySettings CreateCopy(this ISettingsTypeConstructor ctor, Type type)
		=> (ICopySettings) ctor.Create(type, SettingsKind.Copy);
	
	/// 
	public static T CreateCopy<T>(this ISettingsTypeConstructor ctor) where T : ISettings
		=> (T) ctor.Create(typeof(T), SettingsKind.Copy);
	
	/// 
	public static T CreateCopy<T>(this ISettingsTypeConstructor ctor, T source) where T : class, ISettings
		=> (T) ctor.Create(typeof(T), SettingsKind.Copy, source);
	
	/// 
	public static IReadSettings CreateRead(this ISettingsTypeConstructor ctor, Type type)
		=> (IReadSettings) ctor.Create(type, SettingsKind.Read);

	///
	public static IReadSettings CreateRead(this ISettingsTypeConstructor ctor, ISettings source)
		=> (IReadSettings) ctor.Create(source.GetInterfaceType(), SettingsKind.Read, source);

	///
	public static IReadSettings CreateRead(this ISettingsTypeConstructor ctor, Type type, ISettings source)
		=> (IReadSettings) ctor.Create(type, SettingsKind.Read, source);

	/// 
	public static T CreateRead<T>(this ISettingsTypeConstructor ctor) where T : ISettings
		=> (T) ctor.Create(typeof(T), SettingsKind.Read);
	
	/// 
	public static T CreateRead<T>(this ISettingsTypeConstructor ctor, T source) where T : class, ISettings
		=> (T) ctor.Create(typeof(T), SettingsKind.Read, source);
	
	/// 
	public static IInpcSettings CreateInpc(this ISettingsTypeConstructor ctor, Type type)
		=> (IInpcSettings) ctor.Create(type, SettingsKind.Inpc);
	
	/// 
	public static T CreateInpc<T>(this ISettingsTypeConstructor ctor) where T : ISettings
		=> (T) ctor.Create(typeof(T), SettingsKind.Inpc);
	
	/// 
	public static T CreateInpc<T>(this ISettingsTypeConstructor ctor, T source) where T : class, ISettings
		=> (T) ctor.Create(typeof(T), SettingsKind.Inpc, source);
	
	/// 
	public static IRealSettings CreateReal(this ISettingsTypeConstructor ctor, Type type)
		=> (IRealSettings) ctor.Create(type, SettingsKind.Real);

	/// 
	public static IRealSettings CreateReal(this ISettingsTypeConstructor ctor, Type type, ISettingsProvider? provider, string? key)
		=> (IRealSettings) ctor.Create(type, SettingsKind.Real, provider, key);
	
	/// 
	public static T CreateReal<T>(this ISettingsTypeConstructor ctor) where T : ISettings
		=> (T) ctor.Create(typeof(T), SettingsKind.Real);
	
	/// 
	public static T CreateReal<T>(this ISettingsTypeConstructor ctor, T source) where T : class, ISettings
		=> (T) ctor.Create(typeof(T), SettingsKind.Real, source);
	*/
}