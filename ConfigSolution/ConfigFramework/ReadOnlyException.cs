using System;

namespace ArtZilla.Config {
	[Serializable]
	public class ReadOnlyException: Exception {
		public ReadOnlyException() { }
		public ReadOnlyException(String message) : base(message) { }
		public ReadOnlyException(String message, Exception inner) : base(message, inner) { }
		protected ReadOnlyException(
		System.Runtime.Serialization.SerializationInfo info,
		System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
	}
}