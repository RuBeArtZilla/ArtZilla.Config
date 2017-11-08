using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Xml.Serialization;
using ArtZilla.Sharp.Lib;

namespace ArtZilla.Config.Configurators {
	public interface IStreamSerializer {
		void Serialize(Stream stream, Type type, object obj);

		object Deserialize(Stream stream, Type type);
	}
}
