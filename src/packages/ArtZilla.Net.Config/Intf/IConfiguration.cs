namespace ArtZilla.Net.Config; 

/// <summary> Base configuration interface </summary>
public interface IConfiguration {
	void Copy(IConfiguration source);
}