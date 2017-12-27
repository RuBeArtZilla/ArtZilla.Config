using System;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using ArtZilla.Config.Builders;

namespace ArtZilla.Config {
	static class TmpCfgClass<T> where T : IConfiguration {
		const TypeAttributes ClassAttributes = TypeAttributes.Class
			| TypeAttributes.Serializable
			| TypeAttributes.AnsiClass
			| TypeAttributes.Sealed
			| TypeAttributes.Public;

		internal static readonly Type CopyType = new CopyConfigTypeBuilder<T>().Create();
		internal static readonly Type AutoType = new AutoConfigTypeBuilder<T>().Create();
		internal static readonly Type AutoCopyType = new AutoCopyConfigTypeBuilder<T>().Create();
		internal static readonly Type ReadOnlyType = new ReadOnlyConfigTypeBuilder<T>().Create();

		static TmpCfgClass() {
			if (!typeof(T).IsInterface)
				throw new InvalidOperationException(typeof(T).Name + " is not an interface");
		}

		/*
		static Type GenerateReadOnlyType() {
			var tb = GetModuleBuilder().DefineType(GenerateClassName("ReadOnly"), ClassAttributes);

			// Пусть реализует требуемый интерфейс
			tb.AddInterfaceImplementation(typeof(T));
			tb.AddInterfaceImplementation(typeof(IReadOnlyConfiguration));

			// реализуем все свойства требуемого интерфейса
			foreach (var pi in typeof(T).GetProperties())
				AddRoProperty(tb, pi);

			// добавляем конструктор по умолчанию
			var ctor = tb.DefineConstructor(MethodAttributes.Public, CallingConventions.HasThis, Type.EmptyTypes);
			var il = ctor.GetILGenerator();
			il.Emit(OpCodes.Ret);

			return tb.CreateType();
		}
		*/

		static ModuleBuilder GetModuleBuilder() {
			var an = new AssemblyName("gen_" + typeof(T).Name);

#if NET40
			var asm = AppDomain.CurrentDomain.DefineDynamicAssembly(an, AssemblyBuilderAccess.RunAndSave);
			var moduleName = Path.ChangeExtension(an.Name, "dll");
			return asm.DefineDynamicModule(moduleName, false);
#else
			var asm = AssemblyBuilder.DefineDynamicAssembly(an, AssemblyBuilderAccess.Run);
			var moduleName = Path.ChangeExtension(an.Name, "dll");
			return asm.DefineDynamicModule(moduleName);
#endif
		}

		static String GenerateClassName(String prefix) {
			var ns = typeof(T).Namespace;
			if (!String.IsNullOrEmpty(ns))
				ns += ".";

			return ns + prefix + "_" + typeof(T).Name;
		}

		static void AddRwProperty(TypeBuilder tb, PropertyInfo pi, ILGenerator ctor) {
			// создаем приватное поле для хранения значения свойства
			var pf = tb.DefineField("m_" + pi.Name, pi.PropertyType, FieldAttributes.Private);

			// создаем свойство
			var pb = tb.DefineProperty(pi.Name, pi.Attributes, pi.PropertyType, Type.EmptyTypes);

			// реализуем операцию чтения, если необходимо
			if (pi.CanRead) {
				var mi = pi.GetGetMethod();
				var mb = tb.DefineMethod(
					mi.Name,
					MethodAttributes.Public | MethodAttributes.Virtual,
					pi.PropertyType,
					Type.EmptyTypes);

				var il = mb.GetILGenerator();
				il.Emit(OpCodes.Ldarg_0);
				il.Emit(OpCodes.Ldfld, pf);
				il.Emit(OpCodes.Ret);

				pb.SetGetMethod(mb);
				tb.DefineMethodOverride(mb, mi);
			}

			// реализуем операцию записи, если необходимо
			if (pi.CanWrite) {
				var mi = pi.GetSetMethod();
				var mb = tb.DefineMethod(
					mi.Name,
					MethodAttributes.Public | MethodAttributes.Virtual,
					typeof(void),
					new Type[] { pi.PropertyType });

				var il = mb.GetILGenerator();
				il.Emit(OpCodes.Ldarg_0);
				il.Emit(OpCodes.Ldarg_1);
				il.Emit(OpCodes.Stfld, pf);
				il.Emit(OpCodes.Ret);

				pb.SetSetMethod(mb);
				tb.DefineMethodOverride(mb, mi);
			}
		}

		static void AddRoProperty(TypeBuilder tb, PropertyInfo pi) {
			// создаем приватное поле для хранения значения свойства
			var pf = tb.DefineField("m_" + pi.Name, pi.PropertyType, FieldAttributes.Private);

			// создаем свойство
			var pb = tb.DefineProperty(pi.Name, pi.Attributes, pi.PropertyType, Type.EmptyTypes);

			// реализуем операцию чтения, если необходимо
			if (pi.CanRead) {
				var mi = pi.GetGetMethod();
				var mb = tb.DefineMethod(
					mi.Name,
					MethodAttributes.Public | MethodAttributes.Virtual,
					pi.PropertyType,
					Type.EmptyTypes);

				var il = mb.GetILGenerator();
				il.Emit(OpCodes.Ldarg_0);
				il.Emit(OpCodes.Ldfld, pf);
				il.Emit(OpCodes.Ret);

				pb.SetGetMethod(mb);
				tb.DefineMethodOverride(mb, mi);
			}

			// реализуем операцию записи, если необходимо
			if (pi.CanWrite) {
				var mi = pi.GetSetMethod();
				var mb = tb.DefineMethod(
					mi.Name,
					MethodAttributes.Public | MethodAttributes.Virtual,
					typeof(void),
					new Type[] { pi.PropertyType });

				var il = mb.GetILGenerator();
				il.Emit(OpCodes.Ldarg_0);
				il.Emit(OpCodes.Ldstr, "Can't modify a property " + pi.Name + " in a read-only implementation of " + typeof(T).Name + ".");
				il.Emit(OpCodes.Newobj, typeof(ReadOnlyException).GetConstructor(new Type[] { typeof(String) }));
				il.Emit(OpCodes.Throw);

				il.Emit(OpCodes.Ret);

				pb.SetSetMethod(mb);
				tb.DefineMethodOverride(mb, mi);
			}
		}
	}
}