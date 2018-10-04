using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.SymbolStore;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Serialization;

namespace ArtZilla.Config.Builders {
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

		protected ISymbolDocumentWriter Sdw { get; set; }

		public virtual Type Create() {
			Mb = CreateModuleBuilder();
			//Sdw = CreateSymbolDocumentWriter();
			Tb = CreateTypeBuilder();

			AddInterfaces();
			AddConstructors();

#if NET40
			return Tb.CreateType();
#else
			var ti = Tb.CreateTypeInfo();
			return ti.AsType();
#endif
		}

		protected virtual ModuleBuilder CreateModuleBuilder() {
			var an = new AssemblyName("gen_" + typeof(T).Name);
#if DEBUG
			const bool isDebug = true;
#else
			const bool isDebug = false;
#endif
#if NET40
			var asm = AppDomain.CurrentDomain.DefineDynamicAssembly(an, AssemblyBuilderAccess.RunAndSave);
			//MarkDebuggable(asm);
			var moduleName = Path.ChangeExtension(an.Name, "dll");
			return asm.DefineDynamicModule(moduleName, isDebug);
#elif NET45
			var asm = AssemblyBuilder.DefineDynamicAssembly(an, AssemblyBuilderAccess.Run);
			//MarkDebuggable(asm);
			var moduleName = Path.ChangeExtension(an.Name, "dll");
			return asm.DefineDynamicModule(moduleName, isDebug);
#elif NETSTANDARD
			var asm = AssemblyBuilder.DefineDynamicAssembly(an, AssemblyBuilderAccess.Run);
			var moduleName = Path.ChangeExtension(an.Name, "dll");
			return asm.DefineDynamicModule(moduleName);
#endif
		}

		[Conditional("DEBUG")]
		protected virtual void MarkDebuggable(AssemblyBuilder asm) {
			Debug.WriteLine("MarkDebuggable()");

			var daType = typeof(DebuggableAttribute);
			var daCtor = daType.GetConstructor(new[] { typeof(DebuggableAttribute.DebuggingModes) });
			Debug.Assert(daCtor != null, nameof(daCtor) + " != null");

			var daBuilder = new CustomAttributeBuilder(daCtor, new object[] {
				DebuggableAttribute.DebuggingModes.DisableOptimizations
				| DebuggableAttribute.DebuggingModes.Default });

			asm.SetCustomAttribute(daBuilder);
		}

		protected virtual ISymbolDocumentWriter CreateSymbolDocumentWriter() {
#if DEBUG && !NETSTANDARD
			return Mb.DefineDocument("Source.txt", Guid.Empty, Guid.Empty, Guid.Empty);
#else
			return default;
#endif
		}

		[Conditional("DEBUG")]
		protected virtual void MarkLine(ILGenerator il, int startLine, int startColumn, int? endLine = null, int endColumn = 120) {
#if !NETSTANDARD
			il.MarkSequencePoint(Sdw, startLine, startColumn, endLine ?? startLine, endColumn);
#endif
		}

		protected virtual TypeBuilder CreateTypeBuilder()
			=> Mb.DefineType(GenerateClassName(), ClassAttributes);

		protected virtual string GenerateClassName() {
			var ns = typeof(T).Namespace;
			if (!string.IsNullOrEmpty(ns))
				ns += ".";

			return ns + ClassPrefix + "_" + typeof(T).Name;
		}

		protected virtual void AddInterfaces() {
			// Добавляем интерфейс конфигурации по умолчанию
			RecursiveAddInterfaces(typeof(T));
			AddIConfigurationImplementation();

			var ctor = typeof(DataContractAttribute).GetConstructor(Type.EmptyTypes);
			Debug.Assert(ctor != null, nameof(ctor) + " != null");
			Tb.SetCustomAttribute(new CustomAttributeBuilder(ctor, new object[0]));

			void RecursiveAddInterfaces(Type type) {
				foreach (var intf in type.GetInterfaces())
					AddInterfaceImplementation(intf);
				AddInterfaceImplementation(type);
			}
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
				new[] { typeof(IConfiguration) });

			var il = mb.GetILGenerator();
			foreach (var pi in Tb.GetInterfaces().SelectMany(t => t.GetProperties())) {
				Debug.Print(GetType().Name + "." + nameof(IConfiguration.Copy) + " impl " + pi.Name);
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
			DebugMessage(il, "Call of IConfiguration.Constructor");

			foreach (var dv in _fieldValues)
				dv.Attr.GenerateFieldCtorCode(il, dv.Fb);

			il.Emit(OpCodes.Ret);
		}

		protected virtual void AddCopyConstructor() {
			var ctor = Tb.DefineConstructor(MethodAttributes.Public, CallingConventions.HasThis, new[] { typeof(T) });
			var il = ctor.GetILGenerator();

			DebugMessage(il, "Call of IConfiguration.Copy");

			foreach (var pi in Tb.GetInterfaces().SelectMany(t => t.GetProperties())) {
				DebugMessage(il, "Copy " + pi.Name + " property");
				il.Emit(OpCodes.Ldarg_0);
				il.Emit(OpCodes.Ldarg_1);
				il.Emit(OpCodes.Castclass, typeof(T));
				il.Emit(OpCodes.Callvirt, pi.GetGetMethod());
				var fb = GetPrivateField(GetFieldName(pi));
				il.Emit(OpCodes.Stfld, fb);
			}

			il.Emit(OpCodes.Ret);
		}

		protected virtual void AddProperty(PropertyInfo pi) {
			// создаем свойство
			var pb = Tb.DefineProperty(pi.Name, pi.Attributes, pi.PropertyType, Type.EmptyTypes);

			var ctor = typeof(DataMemberAttribute).GetConstructor(Type.EmptyTypes);
			Debug.Assert(ctor != null, nameof(ctor) + " != null");
			pb.SetCustomAttribute(new CustomAttributeBuilder(ctor, new object[0]));

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
				new[] { pi.PropertyType });

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

		protected virtual FieldBuilder CreateField(string name, Type type, FieldAttributes attr) {
			var fb = Tb.DefineField(name, type, attr);
			_fields.Add(fb);
			return fb;
		}

		protected virtual FieldBuilder GetOrCreatePrivateField(string name, Type type)
			=> _fields.Find(f => f.Name == name)
			?? CreateField(name, type, FieldAttributes.Private);

		protected virtual FieldBuilder GetPrivateField(string name)
			=> _fields.Find(f => f.Name == name)
			?? throw new BuildException("Private field " + name + " not exist");

		protected virtual string GetFieldName(PropertyInfo pi)
			=> "_" + pi.Name;

		protected virtual void AddDefaultFieldValue(FieldBuilder fb, IDefaultValueProvider attr) {
			_fieldValues.Add((fb, attr));
			Debug.Print(GetType().Name +" for field " + fb.Name + " has default value " + attr);
		}

		[Conditional("DEBUG")]
		protected void DebugMessage(ILGenerator il, string line) {
			il.Emit(OpCodes.Ldstr, line);
			var method = typeof(Debug).GetMethod(nameof(Debug.WriteLine), new[] { typeof(string) });
			Debug.Assert(method != null, "Debug.WriteLine not found");

			il.Emit(OpCodes.Call, method);
			il.Emit(OpCodes.Nop);
		}

		private readonly List<FieldBuilder> _fields = new List<FieldBuilder>();
		private readonly List<MethodBuilder> _propMethods = new List<MethodBuilder>();
		protected readonly List<(FieldBuilder Fb, IDefaultValueProvider Attr)> _fieldValues = new List<(FieldBuilder Fb, IDefaultValueProvider Attr)>();
	}
}