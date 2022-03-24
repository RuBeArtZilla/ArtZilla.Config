using System;
using System.Diagnostics.CodeAnalysis;

namespace ArtZilla.Net.Config.Builders; 

[SuppressMessage("ReSharper", "StaticMemberInGenericType")]
static class TmpCfgClass<T> where T : IConfiguration {
	public static readonly Type CopyType;
	public static readonly Type NotifyingType;
	public static readonly Type ReadonlyType;
	public static readonly Type RealtimeType;

	static TmpCfgClass() {
		if (!typeof(T).IsInterface)
			throw new InvalidOperationException(typeof(T).Name + " is not an interface");

		CopyType = new CopyConfigTypeBuilder<T>().Create();
		NotifyingType = new NotifyingConfigTypeBuilder<T>().Create();
		ReadonlyType = new ReadonlyConfigTypeBuilder<T>().Create();
		RealtimeType = new RealtimeConfigTypeBuilder<T>().Create();
	}
}