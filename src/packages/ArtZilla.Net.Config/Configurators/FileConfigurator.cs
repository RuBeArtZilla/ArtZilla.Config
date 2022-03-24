namespace ArtZilla.Net.Config.Configurators; 

public sealed class FileConfigurator: LazyConfigurator {
	public FileConfigurator() : this(new FileThread()) { }
	public FileConfigurator(string appName) : this(new FileThread(appName)) { }
	public FileConfigurator(string appName, string companyName) : this(new FileThread(appName, companyName)) { }
	public FileConfigurator(IIoThread thread) : base(thread) { }

	protected override IConfigurator<TConfiguration> CreateTypedConfigurator<TConfiguration>()
		=> new FileConfigurator<TConfiguration>(Io);
}

public sealed class FileConfigurator<TConfiguration>
	: LazyConfigurator<TConfiguration>
	where TConfiguration : class,  IConfiguration {
	public FileConfigurator() : this(new FileThread()) { }
	public FileConfigurator(string appName) : this(new FileThread(appName)) { }
	public FileConfigurator(string appName, string companyName) : this(new FileThread(appName, companyName)) { }
	public FileConfigurator(IIoThread thread) : base(thread) { }

	protected override IConfigurator<TKey, TConfiguration> CreateKeysConfigurator<TKey>()
		=> new FileConfigurator<TKey, TConfiguration>(Io);
}

public sealed class FileConfigurator<TKey, TConfiguration>
	: LazyConfigurator<TKey, TConfiguration>
	where TConfiguration : class, IConfiguration {
	public FileConfigurator() : this(new FileThread()) { }
	public FileConfigurator(string appName) : this(new FileThread(appName)) { }
	public FileConfigurator(string appName, string companyName) : this(new FileThread(appName, companyName)) { }
	public FileConfigurator(IIoThread thread) : base(thread) { }
}