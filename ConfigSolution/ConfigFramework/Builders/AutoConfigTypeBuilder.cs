﻿using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace ArtZilla.Config.Builders {
	public class AutoConfigTypeBuilder<T>: ConfigTypeBuilder<T> where T : IConfiguration {
		protected override String ClassPrefix => "Auto";

		protected override void AddProperty(PropertyInfo pi) {
			// using this field to store property values
			var fb = GetOrCreatePrivateField(GetFieldName(pi), pi.PropertyType);

			var dv = pi.GetCustomAttributes(typeof(DefaultValueAttribute), true).OfType<DefaultValueAttribute>().FirstOrDefault();
			if (dv != null)
				AddDefaultFieldValue(fb, dv.Value);

			base.AddProperty(pi);
		}

		protected override void ImplementPropertyGetMethod(PropertyInfo pi, PropertyBuilder pb, MethodInfo mi, MethodBuilder mb) {
			var fb = GetPrivateField(GetFieldName(pi));
			var il = mb.GetILGenerator();
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Ldfld, fb);
			il.Emit(OpCodes.Ret);
		}

		protected override void ImplementPropertySetMethod(PropertyInfo pi, PropertyBuilder pb, MethodInfo mi, MethodBuilder mb) {
			var fb = GetPrivateField(GetFieldName(pi));
			var il = mb.GetILGenerator();
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Ldarg_1);
			il.Emit(OpCodes.Stfld, fb);
			il.Emit(OpCodes.Ret);
		}
	}

	public class AutoCopyConfigTypeBuilder<T>: ConfigTypeBuilder<T> where T : IConfiguration {
		protected override String ClassPrefix => "AutoCopy";

		protected override void AddProperty(PropertyInfo pi) {
			// using this field to store property values
			var fb = GetOrCreatePrivateField(GetFieldName(pi), pi.PropertyType);

			var dv = pi.GetCustomAttributes(typeof(DefaultValueAttribute), true).OfType<DefaultValueAttribute>().FirstOrDefault();
			if (dv != null)
				AddDefaultFieldValue(fb, dv.Value);

			base.AddProperty(pi);
		}

		protected override void ImplementPropertyGetMethod(PropertyInfo pi, PropertyBuilder pb, MethodInfo mi, MethodBuilder mb) {
			var fb = GetPrivateField(GetFieldName(pi));
			var il = mb.GetILGenerator();
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Ldfld, fb);
			il.Emit(OpCodes.Ret);
		}

		protected override void ImplementPropertySetMethod(PropertyInfo pi, PropertyBuilder pb, MethodInfo mi, MethodBuilder mb) {
			var fb = GetPrivateField(GetFieldName(pi));
			var il = mb.GetILGenerator();
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Ldarg_1);
			il.Emit(OpCodes.Stfld, fb);
			il.Emit(OpCodes.Ret);
		}
	}

	public class ReadOnlyConfigTypeBuilder<T>: ConfigTypeBuilder<T> where T : IConfiguration {
		protected override String ClassPrefix => "ReadOnly";

		protected override void AddInterfaces() {
			base.AddInterfaces();

			Tb.AddInterfaceImplementation(typeof(IReadOnlyConfiguration));
			Tb.AddInterfaceImplementation(typeof(IReadOnlyConfiguration<T>));
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
			il.Emit(OpCodes.Newobj, typeof(ReadOnlyException).GetConstructor(new Type[] { typeof(String) }));
			il.Emit(OpCodes.Throw);
		}
	}

	public class CopyConfigTypeBuilder<T>: ConfigTypeBuilder<T> where T : IConfiguration {
		protected override String ClassPrefix => "Copy";

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
			il.Emit(OpCodes.Ldarg_1);
			il.Emit(OpCodes.Stfld, fb);
			il.Emit(OpCodes.Ret);
		}
	}
}
