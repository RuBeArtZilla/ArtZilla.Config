namespace ArtZilla.Config.Configurators {
	public interface IIoThread {
		bool TryLoad<TConfiguration>(out TConfiguration configuration) where TConfiguration : IConfiguration;
		bool TryLoad<TKey, TConfiguration>(TKey key, out TConfiguration configuration) where TConfiguration : IConfiguration;
		void ToSave<TConfiguration>(TConfiguration configuration) where TConfiguration : IConfiguration;
		void ToSave<TKey, TConfiguration>(TKey key, TConfiguration configuration) where TConfiguration : IConfiguration;
		void Reset<TConfiguration>() where TConfiguration : IConfiguration;
		void Reset<TKey, TConfiguration>(TKey key) where TConfiguration : IConfiguration;
		void Flush();
	}
}
