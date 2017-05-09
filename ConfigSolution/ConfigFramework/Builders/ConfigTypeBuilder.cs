using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Serialization;

namespace ArtZilla.Config.Builders {
	[Serializable]
	public class BuildException : Exception {
		public BuildException() { }
		public BuildException(string message) : base(message) { }
		public BuildException(string message, Exception inner) : base(message, inner) { }
		protected BuildException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}

	public abstract class ConfigTypeBuilder<T> where T : IConfiguration {
		protected virtual TypeAttributes ClassAttributes
			=> TypeAttributes.Class
			 | TypeAttributes.Serializable
			 | TypeAttributes.AnsiClass
			 | TypeAttributes.Sealed
			 | TypeAttributes.Public;

		protected abstract String ClassPrefix { get; }

		protected ModuleBuilder Mb { get; set; }

		protected TypeBuilder Tb { get; set; }

		public virtual Type Create() {
			Mb = CreateModuleBuilder();
			Tb = CreateTypeBuilder();

			AddInterfaces();
			AddConstructors();
			return Tb.CreateType();
		}

		protected virtual ModuleBuilder CreateModuleBuilder() {
			var an = new AssemblyName("gen_" + typeof(T).Name);
			var asm = AppDomain.CurrentDomain.DefineDynamicAssembly(an, AssemblyBuilderAccess.RunAndSave);

			var moduleName = Path.ChangeExtension(an.Name, "dll");
			return asm.DefineDynamicModule(moduleName, false);
		}

		protected virtual TypeBuilder CreateTypeBuilder()
			=> Mb.DefineType(GenerateClassName(), ClassAttributes);

		protected virtual String GenerateClassName() {
			var ns = typeof(T).Namespace;
			if (!String.IsNullOrEmpty(ns))
				ns += ".";

			return ns + ClassPrefix + "_" + typeof(T).Name;
		}

		protected virtual void AddInterfaces() {
			// Добавляем интерфейс конфигурации по умолчанию
			AddInterfaceImplementation(typeof(T));
			AddIConfigurationImplementation();
		}

		protected virtual void AddInterfaceImplementation(Type intfType) {
			Tb.AddInterfaceImplementation(intfType);

			foreach (var pi in intfType.GetProperties())
				AddProperty(pi);

			/*
			foreach (var ei in intfType.GetEvents())
				AddEvent(ei);
			foreach (var mi in intfType.GetMethods().Where(m => _propMethods.All(x => m.Name != x.Name)))
				AddMethod(mi);
				*/
		}

		protected virtual void AddIConfigurationImplementation() {
			var mi = typeof(IConfiguration).GetMethod(nameof(IConfiguration.Copy), new[] { typeof(IConfiguration) });
			var mb = Tb.DefineMethod(
				nameof(IConfiguration.Copy),
				MethodAttributes.Public | MethodAttributes.Virtual,
				typeof(void),
				new Type[] { typeof(IConfiguration) });

			var il = mb.GetILGenerator();
			foreach (var pi in typeof(T).GetProperties()) {
				il.Emit(OpCodes.Ldarg_0);
				il.Emit(OpCodes.Ldarg_1);
				
				il.Emit(OpCodes.Callvirt, pi.GetGetMethod());
				il.Emit(OpCodes.Call, pi.GetSetMethod());
			}

			il.Emit(OpCodes.Ret);

			Tb.DefineMethodOverride(mb, mi);	
		}

		protected virtual void AddConstructors() {
			AddDefaultConstructor();
			AddCopyConstructor();
		}

		protected virtual void AddDefaultConstructor() {
			var ctor = Tb.DefineConstructor(MethodAttributes.Public, CallingConventions.HasThis, Type.EmptyTypes);
			var il = ctor.GetILGenerator();

			foreach (var dv in _fieldValues) {
				il.Emit(OpCodes.Ldarg_0);
				PushObject(il, dv.Item2);
				il.Emit(OpCodes.Stfld, dv.Item1);
			}

			il.Emit(OpCodes.Ret);
		}

		protected virtual void AddCopyConstructor() {
			var ctor = Tb.DefineConstructor(MethodAttributes.Public, CallingConventions.HasThis, new[] { typeof(T) });
			var il = ctor.GetILGenerator();

			foreach (var pi in typeof(T).GetProperties()) {
				il.Emit(OpCodes.Ldarg_0);
				il.Emit(OpCodes.Ldarg_1);
				il.Emit(OpCodes.Castclass, typeof(T));
				il.Emit(OpCodes.Callvirt, pi.GetGetMethod());
				var fb = GetPrivateField(GetFieldName(pi));
				il.Emit(OpCodes.Stfld, fb);
			}

			il.Emit(OpCodes.Ret);
		}

		protected static void PushObject(ILGenerator il, Object value) {
			switch (value) {
				case SByte x: {
						il.Emit(OpCodes.Ldc_I4, (Int32) x);
						il.Emit(OpCodes.Conv_I1);
						break;
					}

				case Int16 x: {
						il.Emit(OpCodes.Ldc_I4, (Int32) x);
						il.Emit(OpCodes.Conv_I2);
						break;
					}

				case Int32 x: {
						il.Emit(OpCodes.Ldc_I4, x);
						break;
					}

				case Int64 x: {
						il.Emit(OpCodes.Ldc_I8, x);
						break;
					}

				case Byte x: {
						il.Emit(OpCodes.Ldc_I4, (Int32) x);
						il.Emit(OpCodes.Conv_I1);
						break;
					}

				case UInt16 x: {
						il.Emit(OpCodes.Ldc_I4, x);
						il.Emit(OpCodes.Conv_I2);
						break;
					}

				case UInt32 x: {
						il.Emit(OpCodes.Ldc_I4, x);
						break;
					}

				case UInt64 x: {
						il.Emit(OpCodes.Ldc_I8, (Int64) x);
						break;
					}

				case Char x: {
						il.Emit(OpCodes.Ldc_I4, x);
						break;
					}

				case Boolean x: {
						il.Emit(x ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);
						break;
					}

				case Single x: {
						il.Emit(OpCodes.Ldc_R4, x);
						break;
					}

				case Double x: {
						il.Emit(OpCodes.Ldc_R8, x);
						break;
					}

				case String x: {
						il.Emit(OpCodes.Ldstr, x);
						break;
					}

				default:
					throw new BuildException("Type " + value.GetType() + " not supported yet.");
			}
		}

		protected virtual void AddProperty(PropertyInfo pi) {
			// создаем свойство
			var pb = Tb.DefineProperty(pi.Name, pi.Attributes, pi.PropertyType, Type.EmptyTypes);
			if (pi.CanRead)
				AddPropertyGetter(pi, pb);

			if (pi.CanWrite)
				AddPropertySetter(pi, pb);
		}

		protected virtual void AddPropertyGetter(PropertyInfo pi, PropertyBuilder pb) {
			var mi = pi.GetGetMethod();
			var mb = Tb.DefineMethod(
					mi.Name,
					MethodAttributes.Public | MethodAttributes.Virtual,
					pi.PropertyType,
					Type.EmptyTypes);

			ImplementPropertyGetMethod(pi, pb, mi, mb);

			pb.SetGetMethod(mb);
			DefineMethodOverride(mb, mi);
		}

		protected virtual void ImplementPropertyGetMethod(PropertyInfo pi, PropertyBuilder pb, MethodInfo mi, MethodBuilder mb)
			=> throw new BuildException("Can't override get method for property " + pi.Name);

		protected virtual void AddPropertySetter(PropertyInfo pi, PropertyBuilder pb) {
			var mi = pi.GetSetMethod();
			var mb = Tb.DefineMethod(
				mi.Name,
				MethodAttributes.Public | MethodAttributes.Virtual,
				typeof(void),
				new Type[] { pi.PropertyType });

			ImplementPropertySetMethod(pi, pb, mi, mb);

			pb.SetSetMethod(mb);
			DefineMethodOverride(mb, mi);
		}

		protected virtual void DefineMethodOverride(MethodBuilder mb, MethodInfo mi) {
			_propMethods.Add(mb);
			Tb.DefineMethodOverride(mb, mi);
		}

		protected virtual void ImplementPropertySetMethod(PropertyInfo pi, PropertyBuilder pb, MethodInfo mi, MethodBuilder mb)
			=> throw new BuildException("Can't override set method for property " + pi.Name);

		protected virtual void AddEvent(EventInfo ei)
			=> throw new BuildException("Can't implement event " + ei.Name);

		protected virtual void AddMethod(MethodInfo mi)
			=> throw new BuildException("Can't implement method " + mi.Name);

		protected virtual FieldBuilder CreateField(String name, Type type, FieldAttributes attr) {
			var fb = Tb.DefineField(name, type, attr);
			_fields.Add(fb);
			return fb;
		}

		protected virtual FieldBuilder GetOrCreatePrivateField(String name, Type type)
			=> _fields.Find(f => f.Name == name)
			?? CreateField(name, type, FieldAttributes.Private);

		protected virtual FieldBuilder GetPrivateField(String name)
			=> _fields.Find(f => f.Name == name)
			?? throw new BuildException("Private field " + name + " not exist");

		protected virtual String GetFieldName(PropertyInfo pi)
			=> "_" + pi.Name;

		protected virtual void AddDefaultFieldValue(FieldBuilder fb, Object value)
			=> _fieldValues.Add(new Tuple<FieldBuilder, Object>(fb, value));

		readonly List<FieldBuilder> _fields = new List<FieldBuilder>();
		readonly List<MethodBuilder> _propMethods = new List<MethodBuilder>();
		protected readonly List<Tuple<FieldBuilder, Object>> _fieldValues = new List<Tuple<FieldBuilder, Object>>();
	}
}
