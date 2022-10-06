using System.Reflection;
using System.Reflection.Emit;
using ArtZilla.Net.Config.Builders;

namespace ArtZilla.Net.Config;

[AttributeUsage(AttributeTargets.Property)]
public class DefaultValueByCtorAttribute : Attribute, IDefaultValueProvider {
	public Type Type { get; }

	public object[] Args { get; }

	public ConstructorInfo Ctor { get; }

	public DefaultValueByCtorAttribute(Type type, params object[] args) {
		Type = type;
		Args = args;
		Ctor = type.GetConstructor(args.Select(v => v.GetType()).ToArray());
	}

	public void GenerateFieldCtorCode(ILGenerator il, FieldBuilder fb) {
		il.Emit(OpCodes.Ldarg_0);

		foreach (var arg in Args)
			il.PushObject(arg);

		il.Emit(OpCodes.Newobj, Ctor);
		il.Emit(OpCodes.Stfld, fb);

		/* il example
			  IL_0089: ldarg.0      // this
				IL_008a: ldstr        "{D1F71EC6-76A6-40F8-8910-68E67D753CD4}"
				IL_008f: newobj       instance void [mscorlib]System.Guid::.ctor(string)
				IL_0094: stfld        valuetype [mscorlib]System.Guid CfTests.TestConfiguration::'<Guid>k__BackingField'
				IL_0099: ldarg.0      // this
				IL_009a: call         instance void [mscorlib]System.Object::.ctor()
				IL_009f: nop          
				IL_00a0: ret
			 */
	}
}