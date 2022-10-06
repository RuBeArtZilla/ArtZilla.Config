using ArtZilla.Net.Config.Configurators;
using ArtZilla.Net.Config.Serializers;
using Newtonsoft.Json;

namespace ArtZilla.Net.Config; 

public class JsonStreamSerializer : IStreamSerializer {
	public JsonStreamSerializer()
		=> _serializer = JsonSerializer.Create(new () { Formatting = Formatting.Indented, });

	/// <inheritdoc />
	public void Serialize(Stream stream, Type type, object obj) {
		using var sr = new StreamWriter(stream);
		using var jsonTextWriter = new JsonTextWriter(sr);
		_serializer.Serialize(jsonTextWriter, obj, type);
	}

	/// <inheritdoc />
	public object Deserialize(Stream stream, Type type) {
		using var sr = new StreamReader(stream);
		using var jsonTextReader = new JsonTextReader(sr);
		return _serializer.Deserialize(jsonTextReader, type);
	}

	readonly JsonSerializer _serializer;
}

public sealed class JsonFileThread : FileThread {
	public JsonFileThread()
		: base(new JsonStreamSerializer()) { }
	
	public JsonFileThread(string appName, string company = default) 
		: base(appName, company, new JsonStreamSerializer()) { }
}

public sealed class JsonFileConfigurator: FileConfigurator {
	public JsonFileConfigurator() : this(new FileThread()) { }
	public JsonFileConfigurator(string appName) : this(new JsonFileThread(appName)) { }
	public JsonFileConfigurator(string appName, string companyName) : this(new JsonFileThread(appName, companyName)) { }
	public JsonFileConfigurator(IIoThread thread) : base(thread) { }

	protected override IConfigurator<TConfiguration> CreateTypedConfigurator<TConfiguration>()
		=> new JsonFileConfigurator<TConfiguration>(Io);
}

public sealed class JsonFileConfigurator<TConfiguration> : FileConfigurator<TConfiguration>  where TConfiguration : class,  IConfiguration {
	public JsonFileConfigurator() : this(new FileThread()) { }
	public JsonFileConfigurator(string appName) : this(new JsonFileThread(appName)) { }
	public JsonFileConfigurator(string appName, string companyName) : this(new JsonFileThread(appName, companyName)) { }
	public JsonFileConfigurator(IIoThread thread) : base(thread) { }

	protected override IConfigurator<TKey, TConfiguration> CreateKeysConfigurator<TKey>()
		=> new JsonFileConfigurator<TKey, TConfiguration>(Io);
}

public sealed class JsonFileConfigurator<TKey, TConfiguration> : FileConfigurator<TKey, TConfiguration> where TConfiguration : class, IConfiguration {
	public JsonFileConfigurator() : this(new FileThread()) { }
	public JsonFileConfigurator(string appName) : this(new JsonFileThread(appName)) { }
	public JsonFileConfigurator(string appName, string companyName) : this(new JsonFileThread(appName, companyName)) { }
	public JsonFileConfigurator(IIoThread thread) : base(thread) { }
}