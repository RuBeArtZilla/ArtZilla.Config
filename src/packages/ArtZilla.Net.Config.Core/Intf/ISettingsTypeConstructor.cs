namespace ArtZilla.Net.Config;

/// Settings type constructor for interface
public interface ISettingsTypeConstructor {
	///
	/// <param name="intfType"></param>
	/// <param name="kind"></param>
	/// <param name="key"></param>
	/// <param name="isInit"></param>
	/// <param name="provider"></param>
	/// <returns></returns>
	ISettings Create(Type intfType, SettingsKind kind, ISettingsProvider? provider, string? key, bool isInit = false);

	///
	/// <param name="intfType"></param>
	/// <param name="kind"></param>
	/// <param name="provider"></param>
	/// <param name="key"></param>
	/// <param name="source"></param>
	/// <returns></returns>
	ISettings Create(Type intfType, SettingsKind kind, ISettingsProvider? provider, string? key, ISettings source);

	/// 
	/// <param name="intfType"></param>
	/// <param name="kind"></param>
	/// <returns></returns>
	Type GetType(Type intfType, SettingsKind kind);
}