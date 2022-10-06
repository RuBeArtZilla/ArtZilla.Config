using System;

namespace ArtZilla.Net.Config; 

[Serializable]
public class ReadonlyException: Exception {
	public ReadonlyException() { }
	public ReadonlyException(String message) : base(message) { }
	public ReadonlyException(String message, Exception inner) : base(message, inner) { }
	protected ReadonlyException(
		System.Runtime.Serialization.SerializationInfo info,
		System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}