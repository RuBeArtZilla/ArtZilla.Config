namespace ArtZilla.Config.Extensions {
	public static class ConfigurationExtensions {
		public static IRealtimeConfiguration AsRealtime<TConfiguration>(this TConfiguration configuration) 
			where TConfiguration : IConfiguration
			=> (IRealtimeConfiguration) configuration;

		public static IReadonlyConfiguration AsReadonly<TConfiguration>(this TConfiguration configuration) 
			where TConfiguration : IConfiguration
			=> (IReadonlyConfiguration) configuration;

		public static INotifyingConfiguration AsNotifying<TConfiguration>(this TConfiguration configuration) 
			where TConfiguration : IConfiguration
			=> (INotifyingConfiguration) configuration;
	}
}