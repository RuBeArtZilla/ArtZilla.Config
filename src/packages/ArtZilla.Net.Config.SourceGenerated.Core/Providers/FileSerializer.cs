namespace ArtZilla.Net.Config;

///
public abstract class FileSerializer {
	///
	public ISettingsTypeConstructor Constructor { get; }

	///
	public FileSerializer(ISettingsTypeConstructor constructor)
		=> Constructor = constructor;

	/// returns file extension for settings
	public virtual string GetFileExtension() => ".cfg";

	/// 
	/// <param name="type"></param>
	/// <param name="settings"></param>
	/// <param name="path"></param>
	/// <returns></returns>
	public abstract Task Serialize(Type type, string path, ISettings settings);

	/// 
	/// <param name="type"></param>
	/// <param name="path"></param>
	/// <returns></returns>
	public abstract Task<ISettings> Deserialize(Type type, string path);
}