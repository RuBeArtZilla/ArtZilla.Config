namespace ArtZilla.Net.Config; 

/// Base configuration interface
public interface IConfiguration {
	void Copy(IConfiguration source);
}

/// 
[Obsolete("Use ISettingsProvider")]
public interface IConfigurator {
	///	Remove all <see cref="IConfiguration"/> from this instance
	void Clear();

	/// Save <paramref name="value"/> as <typeparamref name="TConfiguration"/>
	/// <typeparam name="TConfiguration"></typeparam>
	/// <param name="value"></param>
	void Save<TConfiguration>(TConfiguration value) where TConfiguration : class, IConfiguration;
		
	/// Reset <typeparamref name="TConfiguration"/> to default values
	/// <typeparam name="TConfiguration"></typeparam>
	void Reset<TConfiguration>() where TConfiguration : class, IConfiguration;

	bool IsExist<TConfiguration>() where TConfiguration : class, IConfiguration;

	/// return a copy of actual <typeparamref name="TConfiguration"/>
	/// <typeparam name="TConfiguration"></typeparam>
	/// <returns></returns>
	TConfiguration Copy<TConfiguration>() where TConfiguration : class, IConfiguration;

	/// Method return a copy of actual <typeparamref name="TConfiguration"/> with <see cref="INotifyingConfiguration"/> implementation
	/// <typeparam name="TConfiguration">type of <see cref="IConfiguration"/> to return</typeparam>
	/// <returns><see cref="INotifyingConfiguration"/> implementation of actual <typeparamref name="TConfiguration"/></returns>
	TConfiguration Notifying<TConfiguration>() where TConfiguration : class, IConfiguration;

	/// return a read only configuration
	/// <typeparam name="TConfiguration"></typeparam>
	/// <returns></returns>
	TConfiguration Readonly<TConfiguration>() where TConfiguration : class, IConfiguration;

	/// Method return actual <typeparamref name="TConfiguration"/> with <see cref="IRealtimeConfiguration"/> implementation
	/// <typeparam name="TConfiguration"></typeparam>
	/// <returns></returns>
	TConfiguration Realtime<TConfiguration>() where TConfiguration : class, IConfiguration;
	
	/// 
	IConfigurator<TConfiguration> As<TConfiguration>() where TConfiguration : class, IConfiguration;
	
	/// 
	IConfigurator<TKey, TConfiguration> As<TKey, TConfiguration>() where TConfiguration : class, IConfiguration;
	
	/// 
	IConfiguration[] Get();
	
	/// 
	void Set(params IConfiguration[] configurations);
	
	/// 
	void CloneTo(IConfigurator destination);
}

/// 
[Obsolete("Use ISettingsProvider")]
public interface IConfigurator<TConfiguration> : IConfigProvider where TConfiguration : class, IConfiguration {
	/// 
	void Save(TConfiguration value);
	
	/// 
	TConfiguration Copy();
	
	/// 
	TConfiguration Notifying();
	
	/// 
	TConfiguration Readonly();
	
	/// 
	TConfiguration Realtime();

	/// 
	IConfigurator<TKey, TConfiguration> As<TKey>();
}

/// 
[Obsolete("Use ISettingsProvider")]
public interface IConfigurator<TKey, TConfiguration> : IEnumerable<TConfiguration> where TConfiguration : class, IConfiguration {
	/// 
	bool IsExist(TKey key);
	
	/// 
	void Reset(TKey key);
	
	/// 
	void Save(TKey key, TConfiguration value);
	
	/// 
	TConfiguration Copy(TKey key);
	
	/// 
	TConfiguration Notifying(TKey key);
	
	/// 
	TConfiguration Readonly(TKey key);
	
	/// 
	TConfiguration Realtime(TKey key);
	
	/// 
	TConfiguration this[TKey key] { get; set; }
}