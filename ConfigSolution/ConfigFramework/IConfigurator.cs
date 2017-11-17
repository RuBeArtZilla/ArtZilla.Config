namespace ArtZilla.Config {
	public interface IConfigurator {
		/// <summary>return auto-updated (automatically loading/saving) configuration</summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		T GetAuto<T>() where T : IConfiguration;

		// return a auto-updated copy of actual configuration
		T GetAutoCopy<T>() where T : IConfiguration;

		// return a copy of actual configuration
		TConfiguration GetCopy<TConfiguration>() where TConfiguration : IConfiguration;

		// return read only configuration
		T GetReadOnly<T>() where T : IConfiguration;

		void Save<T>(T value) where T : IConfiguration;

		void Reset<T>() where T : IConfiguration;
	}
}