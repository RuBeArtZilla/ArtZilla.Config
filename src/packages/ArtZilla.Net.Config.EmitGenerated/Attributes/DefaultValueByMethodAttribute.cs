using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Reflection.Emit;
using ArtZilla.Net.Config.Builders;

namespace ArtZilla.Net.Config;

[AttributeUsage(AttributeTargets.Property)]
public class DefaultValueByMethodAttribute : Attribute, IDefaultValueProvider {
	public Type Type { get; }

	public object[] Args { get; }

	public string MethodName { get; }

	public DefaultValueByMethodAttribute(Type type, string methodName, params object[] args) {
		Type = type;
		Args = args;
		MethodName = methodName;
	}

	public void GenerateFieldCtorCode(ILGenerator il, FieldBuilder fb) {
		var method = Type.GetMethod(MethodName, BindingFlags.Public | BindingFlags.Static);
		if (method is null)
			method = Type.GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public)
			             .FirstOrDefault(i => string.Equals(i.Name, MethodName, StringComparison.Ordinal));
		if (method is null)
			ThrowMethodNotFoundException(MethodName);
		
		if (method.ReturnType == typeof(void)) {
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Ldfld, fb);
			foreach (var arg in Args)
				il.PushObject(arg);

			Debug.Assert(method != null);
			il.Emit(OpCodes.Call, method);
		} else { 
			il.Emit(OpCodes.Ldarg_0);
			foreach (var arg in Args)
				il.PushObject(arg);

			Debug.Assert(method != null);
			il.Emit(OpCodes.Call, method);
			il.Emit(OpCodes.Stfld, fb);
		}
		 
		/* il example
			 IL_0000: ldarg.0
			IL_0001: call valuetype [mscorlib]System.Guid [mscorlib]System.Guid::NewGuid()
			IL_0006: stfld valuetype [mscorlib]System.Guid ArtZilla.Config.Tests.Class1::_guid
			IL_000b: ldarg.0
			IL_000c: call instance void [mscorlib]System.Object::.ctor()
			IL_0011: nop
			IL_0012: ret
		 
		 */
	}

#if NET60
	[DoesNotReturn]
#endif
	static void ThrowMethodNotFoundException(string methodName)
		=> throw new($"method {methodName} not found");
}