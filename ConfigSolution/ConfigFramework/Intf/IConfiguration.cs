namespace ArtZilla.Config {
	/// <summary>
	/// Represent configuration
	/// </summary>
	public interface IConfiguration {
		void Copy(IConfiguration source);
	}
}