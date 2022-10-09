namespace ArtZilla.Net.Config;

/// 
public static class SettingsTypeConstructorExtensions {
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
	public static T CreateReal<T>(this ISettingsTypeConstructor ctor) where T : ISettings
		=> (T) ctor.Create(typeof(T), SettingsKind.Real);
	
	/// 
	public static T CreateReal<T>(this ISettingsTypeConstructor ctor, T source) where T : class, ISettings
		=> (T) ctor.Create(typeof(T), SettingsKind.Real, source);
}