using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;

namespace ArtZilla.Config.Builders {
	public sealed class ReadonlyConfigTypeBuilder<T>: ConfigTypeBuilder<T> where T : IConfiguration {
		protected override string ClassPrefix => "Readonly";

		protected override void AddInterfaces() {
			base.AddInterfaces();
			Tb.AddInterfaceImplementation(typeof(IReadonlyConfiguration));
		}

		protected override void ImplementSimplePropertySetMethod(in SimplePropertyBuilder spb, MethodInfo mi, MethodBuilder mb) {
			var il = mb.GetILGenerator();
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Ldstr, "Can't modify a property " + spb.Pi.Name + " in a read-only implementation of " + typeof(T).Name + ".");
			var ctor = typeof(ReadonlyException).GetConstructor(new [] { typeof(string) });
			Debug.Assert(ctor != null, "Can't find ReadonlyException's ctor");
			il.Emit(OpCodes.Newobj, ctor);
			il.Emit(OpCodes.Throw);
		}
	}
}
