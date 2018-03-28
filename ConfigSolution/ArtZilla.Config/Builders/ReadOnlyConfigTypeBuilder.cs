using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace ArtZilla.Config.Builders {
	public class ReadonlyConfigTypeBuilder<T>: ConfigTypeBuilder<T> where T : IConfiguration {
		protected override String ClassPrefix => "Readonly";

		protected override void AddInterfaces() {
			base.AddInterfaces();

			Tb.AddInterfaceImplementation(typeof(IReadonlyConfiguration));
		}

		protected override void AddProperty(PropertyInfo pi) {
			// using this field to store property values
			var fb = GetOrCreatePrivateField(GetFieldName(pi), pi.PropertyType);

			var dv = pi.GetCustomAttributes(typeof(DefaultValueAttribute), true).OfType<DefaultValueAttribute>().FirstOrDefault();
			if (dv != null)
				AddDefaultFieldValue(fb, dv.Value);

			base.AddProperty(pi);
		}

		protected override void ImplementPropertyGetMethod(PropertyInfo pi, 
			                                                 PropertyBuilder pb, 
																											 MethodInfo mi, 
																											 MethodBuilder mb) {
			var fb = GetPrivateField(GetFieldName(pi));
			var il = mb.GetILGenerator();
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Ldfld, fb);
			il.Emit(OpCodes.Ret);
		}

		protected override void ImplementPropertySetMethod(PropertyInfo pi, 
			                                                 PropertyBuilder pb, 
																											 MethodInfo mi, 
																											 MethodBuilder mb) {
			var fb = GetPrivateField(GetFieldName(pi));
			var il = mb.GetILGenerator();
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Ldstr, "Can't modify a property " + pi.Name + " in a read-only implementation of " + typeof(T).Name + ".");
			il.Emit(OpCodes.Newobj, typeof(ReadonlyException).GetConstructor(new Type[] { typeof(String) }));
			il.Emit(OpCodes.Throw);
		}
	}
}
