using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace ArtZilla.Config.Builders {
	public class CopyConfigTypeBuilder<T>: ConfigTypeBuilder<T> where T : IConfiguration {
		protected override String ClassPrefix => "Copy";

		protected override void AddProperty(PropertyInfo pi) {
			// using this field to store property values
			var fb = GetOrCreatePrivateField(GetFieldName(pi), pi.PropertyType);

			var dv = pi.GetCustomAttributes(true).OfType<IDefaultValueAttribute>().FirstOrDefault();
			if (dv != null)
				AddDefaultFieldValue(fb, dv);

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
			il.Emit(OpCodes.Ldarg_1);
			il.Emit(OpCodes.Stfld, fb);
			il.Emit(OpCodes.Ret);
		}
	}
}
