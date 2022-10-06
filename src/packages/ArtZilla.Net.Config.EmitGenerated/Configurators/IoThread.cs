namespace ArtZilla.Net.Config.Configurators; 

public abstract class IoThread: IIoThread {
	public abstract bool TryLoad<TConfiguration>(out TConfiguration configuration) where TConfiguration : IConfiguration;
	public abstract bool TryLoad<TKey, TConfiguration>(TKey key, out TConfiguration configuration) where TConfiguration : IConfiguration;
	public abstract void ToSave<TConfiguration>(TConfiguration configuration) where TConfiguration : IConfiguration;
	public abstract void ToSave<TKey, TConfiguration>(TKey key, TConfiguration configuration) where TConfiguration : IConfiguration;
	public abstract void Reset<TConfiguration>() where TConfiguration : IConfiguration;
	public abstract void Reset<TKey, TConfiguration>(TKey key) where TConfiguration : IConfiguration;
	public abstract void Flush();
}