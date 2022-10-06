using System.Reflection;

namespace ArtZilla.Net.Config;

[AttributeUsage(AttributeTargets.Property)]
public class DefaultValueByCtorAttribute : Attribute {
	public Type Type { get; }

	public object[] Args { get; }

	public ConstructorInfo Ctor { get; }

	public DefaultValueByCtorAttribute(Type type, params object[] args) {
		Type = type;
		Args = args;
		Ctor = type.GetConstructor(args.Select(v => v.GetType()).ToArray());
	}
}