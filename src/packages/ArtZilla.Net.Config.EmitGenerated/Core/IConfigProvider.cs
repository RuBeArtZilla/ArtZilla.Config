namespace ArtZilla.Net.Config;

/// 
[Obsolete("Use ISettingsProvider")]
public interface IConfigProvider {
	/// 
	bool IsExist();
	
	/// 
	void Reset();
	
	///
	void Save(IConfiguration value);
	
	///
	IConfiguration GetCopy();
	
	///
	INotifyingConfiguration GetNotifying();
	
	///
	IReadonlyConfiguration GetReadonly();
	
	///
	IRealtimeConfiguration GetRealtime();
}