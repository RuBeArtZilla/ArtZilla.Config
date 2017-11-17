namespace ArtZilla.Config {
	/// <summary>Read only configuration</summary>
	/// <typeparam name="T">Interface that implement IConfiguration</typeparam>
	public interface IReadOnlyConfiguration<T> : IReadOnlyConfiguration where T : IConfiguration { }
}