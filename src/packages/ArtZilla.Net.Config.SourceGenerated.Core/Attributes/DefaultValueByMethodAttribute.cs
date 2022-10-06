using System.Diagnostics.CodeAnalysis;

namespace ArtZilla.Net.Config;

[AttributeUsage(AttributeTargets.Property)]
public class DefaultValueByMethodAttribute : Attribute {
	public Type Type { get; }

	public object[] Args { get; }

	public string MethodName { get; }

	public DefaultValueByMethodAttribute(Type type, string methodName, params object[] args) {
		Type = type;
		Args = args;
		MethodName = methodName;
	}
}