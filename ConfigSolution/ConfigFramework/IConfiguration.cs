using System;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using ArtZilla.Config.Builders;
using System.Collections.Generic;
using System.Globalization;

namespace ArtZilla.Config {
	public interface IConfiguration {
		void Copy(IConfiguration source);
	}

	public interface IRealtimeConfiguration: IConfiguration {
	}

	public class CfgIniAttribute: Attribute { }
	public class CfgIgnoreAttribute: Attribute { }

	public class CfgMachineAttribute: Attribute { }
	public class CfgUserAttribute: Attribute { }

	[Serializable]
	public class ReadOnlyException: Exception {
		public ReadOnlyException() { }
		public ReadOnlyException(String message) : base(message) { }
		public ReadOnlyException(String message, Exception inner) : base(message, inner) { }
		protected ReadOnlyException(
		System.Runtime.Serialization.SerializationInfo info,
		System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
	}

	[AttributeUsage(AttributeTargets.Property)]
	public class DefaultValueAttribute: Attribute {
		public Object Value { get; }

		public DefaultValueAttribute(Object value)
			=> Value = value;

		public DefaultValueAttribute(Type type, String strValue)
			=> Value = ConvertFromString(type, strValue);

		static Object ConvertFromString(Type type, String strValue)
			=> _converters.TryGetValue(type, out var f)
				? f(strValue)
				: throw new Exception("Can't convert " + strValue + " to instance of type " + type.Name);

		static Dictionary<Type, Func<String, Object>> _converters
			= new Dictionary<Type, Func<String, Object>> {
				[typeof(Byte)] = s => Byte.Parse(s),
				[typeof(SByte)] = s => SByte.Parse(s),
				[typeof(Int16)] = s => Int16.Parse(s),
				[typeof(Int32)] = s => Int32.Parse(s),
				[typeof(Int64)] = s => Int64.Parse(s),
				[typeof(UInt16)] = s => UInt16.Parse(s),
				[typeof(UInt32)] = s => UInt32.Parse(s),
				[typeof(UInt64)] = s => UInt64.Parse(s),

				[typeof(Single)] = s => Single.Parse(s),
				[typeof(Double)] = s => Double.Parse(s),
				[typeof(Decimal)] = s => Decimal.Parse(s, NumberStyles.Number),
				[typeof(Boolean)] = s => Boolean.Parse(s),

				[typeof(String)] = s => s,
			};
	}

	public interface IAutoConfiguration : IConfiguration { }

	public interface IAutoConfiguration<T> : IAutoConfiguration where T : IConfiguration { }

	/// <summary>
	/// Read only configuration
	/// </summary>
	public interface IReadOnlyConfiguration : IConfiguration { }

	/// <summary>
	/// Read only configuration
	/// </summary>
	/// <typeparam name="T">Interface that implement IConfiguration</typeparam>
	public interface IReadOnlyConfiguration<T> : IReadOnlyConfiguration where T : IConfiguration { }

	public interface IConfigurator {
		/// <summary>
		/// return auto-updated (automatically loading/saving) configuration 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		T GetAuto<T>() where T : IConfiguration;

		// return a auto-updated copy of actual configuration
		T GetAutoCopy<T>() where T : IConfiguration;

		// return a copy of actual configuration
		TConfiguration GetCopy<TConfiguration>() where TConfiguration : IConfiguration;

		// return read only configuration
		T GetReadOnly<T>() where T : IConfiguration;

		void Save<T>(T value) where T : IConfiguration;

		void Reset<T>() where T : IConfiguration;
	}

	public static class ConfigManager {
		public static String CompanyName { get; set; }
		public static String AppName { get; set; } = "Noname";

		static ConfigManager() {
			AppName = Assembly.GetExecutingAssembly().GetName().Name;
		}

		public static IConfigurator GetDefaultConfigurator() {
			return new MemoryConfigurator();
		}
	}

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

		/*static Type GenerateReadOnlyType() {
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
		}*/

		static ModuleBuilder GetModuleBuilder() {
			var an = new AssemblyName("gen_" + typeof(T).Name);
			var asm = AppDomain.CurrentDomain.DefineDynamicAssembly(an, AssemblyBuilderAccess.RunAndSave);

			var moduleName = Path.ChangeExtension(an.Name, "dll");
			return asm.DefineDynamicModule(moduleName, false);
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
