using System;
using System.IO;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace ArtZilla.Config.Configurators {
	public class SimpleXmlSerializer: IStreamSerializer {
		public object Deserialize(Stream stream, Type type)
			=> GetSerializer(type).Deserialize(stream);

		public void Serialize(Stream stream, Type type, object obj)
			=> GetSerializer(type).Serialize(stream, obj);

		private XmlSerializer GetSerializer(Type type)
			=> new XmlSerializer(type);
	}

	public class OtherXmlSerializer : IStreamSerializer {
		public object Deserialize(Stream stream, Type type)
			=> GetSerializer(type).ReadObject(stream);

		public void Serialize(Stream stream, Type type, object obj)
			=> GetSerializer(type).WriteObject(stream, obj);

		private DataContractSerializer GetSerializer(Type type)
			=> new DataContractSerializer(type);
	}

	public class DarkSerializer {
		public DarkSerializer(Type type) {
			
		}

		public object Deserialize(Stream stream) {
			throw new NotImplementedException();
		}

		public void Serialize(Stream stream, object obj) {
			throw new NotImplementedException();
		}		
	}
}
