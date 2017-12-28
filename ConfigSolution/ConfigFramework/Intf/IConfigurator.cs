namespace ArtZilla.Config {
	public interface IConfigurator {
		/// <summary>
		/// return a copy of actual <typeparamref name="TConfiguration"/>
		/// </summary>
		/// <typeparam name="TConfiguration"></typeparam>
		/// <returns></returns>
		TConfiguration GetCopy<TConfiguration>() where TConfiguration : IConfiguration;

		/// <summary>
		/// Method return a copy of actual <typeparamref name="TConfiguration"/> with <see cref="INotifyingConfiguration"/> implementation
		/// </summary>
		/// <typeparam name="TConfiguration">type of <see cref="IConfiguration"/> to return</typeparam>
		/// <returns><see cref="INotifyingConfiguration"/> implementation of actual <typeparamref name="TConfiguration"/></returns>
		TConfiguration GetNotifying<TConfiguration>() where TConfiguration : IConfiguration;

		/// <summary>
		/// return a read only configuration
		/// </summary>
		/// <typeparam name="TConfiguration"></typeparam>
		/// <returns></returns>
		TConfiguration GetReadonly<TConfiguration>() where TConfiguration : IConfiguration;

		/// <summary>
		/// Method return actual <typeparamref name="TConfiguration"/> with <see cref="IRealtimeConfiguration"/> implementation
		/// </summary>
		/// <typeparam name="TConfiguration"></typeparam>
		/// <returns></returns>
		TConfiguration GetRealtime<TConfiguration>() where TConfiguration : IConfiguration;

		/// <summary>
		/// Save <paramref name="value"/> as <typeparamref name="TConfiguration"/>
		/// </summary>
		/// <typeparam name="TConfiguration"></typeparam>
		/// <param name="value"></param>
		void Save<TConfiguration>(TConfiguration value) where TConfiguration : IConfiguration;

		/// <summary>
		/// Reset <typeparamref name="TConfiguration"/> to default values
		/// </summary>
		/// <typeparam name="TConfiguration"></typeparam>
		void Reset<TConfiguration>() where TConfiguration : IConfiguration;
	}
}