using System;
using System.IO;

namespace ArtZilla.Net.Config.Serializers; 

public interface IStreamSerializer {
	void Serialize(Stream stream, Type type, object obj);

	object Deserialize(Stream stream, Type type);
}