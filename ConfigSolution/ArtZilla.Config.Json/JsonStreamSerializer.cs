using System;
using System.IO;
using ArtZilla.Config.Configurators;
using Newtonsoft.Json;

namespace ArtZilla.Config {
	public class JsonStreamSerializer : IStreamSerializer {
		public JsonStreamSerializer()
			=> _serializer = JsonSerializer.Create(
			new JsonSerializerSettings {
				Formatting = Formatting.Indented,
			});

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

		private readonly JsonSerializer _serializer;
	}

	public class JsonFileThread : FileThread {
		public JsonFileThread() 
			: base() 
			=> Serializer = new JsonStreamSerializer();

		public JsonFileThread(string appName, string company) 
			: base(appName, company) 
			=> Serializer = new JsonStreamSerializer();
	}
}
