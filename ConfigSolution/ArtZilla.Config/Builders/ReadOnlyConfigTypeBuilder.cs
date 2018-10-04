using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace ArtZilla.Config.Builders {
	public sealed class ReadonlyConfigTypeBuilder<T>: ConfigTypeBuilder<T> where T : IConfiguration {
		protected override string ClassPrefix => "Readonly";

		protected override void AddInterfaces() {
			base.AddInterfaces();

			Tb.AddInterfaceImplementation(typeof(IReadonlyConfiguration));
		}

		protected override void AddProperty(PropertyInfo pi) {
			var type = pi.PropertyType;

			// using this field to store property values
			var fb = GetOrCreatePrivateField(GetFieldName(pi), type);
			var dv = pi.GetCustomAttributes(true).OfType<IDefaultValueProvider>().FirstOrDefault();
			if (dv != null)
				AddDefaultFieldValue(fb, dv);
			else if (type.IsGenericType && type == typeof(IList<>).MakeGenericType(type.GetGenericArguments()[0]))
				AddDefaultFieldValue(fb, GetIListDefaultValue(pi));

			base.AddProperty(pi);
		}

		private IDefaultValueProvider GetIListDefaultValue(PropertyInfo pi)
			=> new ReadonlyIListDefaultValueProvider(pi.PropertyType.GetGenericArguments()[0]);

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

		private class ReadonlyIListDefaultValueProvider : IDefaultValueProvider {
			public ReadonlyIListDefaultValueProvider(Type itemType) => _itemType = itemType;

			public void GenerateFieldCtorCode(ILGenerator il, FieldBuilder fb) {
				var type = typeof(List<>).MakeGenericType(_itemType);
				var ctor = type.GetConstructor(Type.EmptyTypes);
				il.Emit(OpCodes.Newobj, ctor);
				il.Emit(OpCodes.Stfld, fb);
			}

			private readonly Type _itemType;
		}
	}
}
