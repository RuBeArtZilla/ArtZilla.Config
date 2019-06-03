using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using ArtZilla.Net.Core.Extensions;

namespace ArtZilla.Config.Builders {
	public static class BuilderExt {
		public static void AddAttribute<TAttribute>(this TypeBuilder tb, params object[] args) where TAttribute : Attribute {
			var ctorArgs = args?.Select(a => a.GetType()).ToArray() ?? Type.EmptyTypes;
			var ctor = typeof(TAttribute).GetConstructor(ctorArgs);
			Debug.Assert(ctor != null, nameof(ctor) + " != null");
			tb.SetCustomAttribute(new CustomAttributeBuilder(ctor, args ?? new object[0]));
		}

		public static void AddAttribute<TAttribute>(this PropertyBuilder pb, params object[] args) where TAttribute : Attribute {
			var ctorArgs = args?.Select(a => a.GetType()).ToArray() ?? Type.EmptyTypes;
			var ctor = typeof(TAttribute).GetConstructor(ctorArgs);
			Debug.Assert(ctor != null, nameof(ctor) + " != null");
			pb.SetCustomAttribute(new CustomAttributeBuilder(ctor, args ?? new object[0]));
		}

		[Conditional("DEBUG")]
		public static void Print(this ILGenerator il, string line) {
			const string methodName = nameof(Debug.WriteLine);

			il.Emit(OpCodes.Ldstr, line);
			var method = typeof(Debug).GetMethod(methodName, new[] { typeof(string) });
			Debug.Assert(method != null, methodName + " not found");

			il.Emit(OpCodes.Call, method);
		}

		[Conditional("DEBUG")]
		public static void PrintPart(this ILGenerator il, string partOfLine) {
			const string methodName = nameof(Debug.Write);

			il.Emit(OpCodes.Ldstr, partOfLine);
			var method = typeof(Debug).GetMethod(methodName, new[] { typeof(string) });
			Debug.Assert(method != null, methodName + " not found");

			il.Emit(OpCodes.Call, method);
		}
	}
	public abstract class ConfigTypeBuilder<T> where T : IConfiguration {
		protected static ModuleBuilder Mb => _mb ?? (_mb = CreateModuleBuilder());

		protected virtual TypeAttributes ClassAttributes
			=> TypeAttributes.Class
			 | TypeAttributes.Serializable
			 | TypeAttributes.AnsiClass
			 | TypeAttributes.Sealed
			 | TypeAttributes.Public;

		protected abstract string ClassPrefix { get; }

		protected TypeBuilder Tb { get; set; }

		public string ClassName => _className ?? (_className = ClassPrefix + "_" + typeof(T).Name);

		public virtual Type Create() {
			// Mb = CreateModuleBuilder();
			Tb = CreateTypeBuilder();

			AddInterfaces();
			AddConstructors();

			Debug.WriteLine("Creating type for " + ClassName);

#if NET40 
			return Tb.CreateType();
#else
			var ti = Tb.CreateTypeInfo();
			return ti.AsType();
#endif
		}

		protected static ModuleBuilder CreateModuleBuilder() {
			var an = new AssemblyName("gen_" + typeof(T).Name) {
				Version = Assembly.GetExecutingAssembly().GetName().Version // just for fun
			};

			var moduleName = Path.ChangeExtension(an.Name, "dll");

#if NET40
			var asm = AppDomain.CurrentDomain.DefineDynamicAssembly(an, AssemblyBuilderAccess.RunAndSave);
			return asm.DefineDynamicModule(moduleName, false);
#else
			var asm = AssemblyBuilder.DefineDynamicAssembly(an, AssemblyBuilderAccess.Run);
			return asm.DefineDynamicModule(moduleName);
#endif
		}

		protected virtual TypeBuilder CreateTypeBuilder() {
			var className = GenerateClassName();
			Debug.Print("Defining type " + className);

			var tb = Mb.DefineType(className, ClassAttributes);
			tb.AddAttribute<XmlRootAttribute>(typeof(T).Name);
			return tb;
		}

		protected virtual string GenerateClassName() {
			var ns = typeof(T).Namespace;
			if (!string.IsNullOrEmpty(ns))
				ns += ".";

			return ns + ClassName;
		}

		protected virtual void AddInterfaces() {
			// Добавляем интерфейс конфигурации по умолчанию
			RecursiveAddInterfaces(typeof(T));
			AddIConfigurationImplementation();

			Tb.AddAttribute<DataContractAttribute>();

			void RecursiveAddInterfaces(Type type) {
				foreach (var intf in type.GetInterfaces())
					AddInterfaceImplementation(intf);
				AddInterfaceImplementation(type);
			}
		}

		protected virtual void AddInterfaceImplementation(Type intfType) {
			Tb.AddInterfaceImplementation(intfType);

			foreach (var pi in intfType.GetProperties()) {
				var type = pi.PropertyType;
				var isIList = type.IsGenericType && type == typeof(IList<>).MakeGenericType(type.GetGenericArguments()[0]);
				if (isIList)
					AddIListProperty(pi, type.GetGenericArguments()[0]);
				else
					AddSimpleProperty(pi);
			}

			/*
			foreach (var ei in intfType.GetEvents())
				AddEvent(ei);
			foreach (var mi in intfType.GetMethods().Where(m => _propMethods.All(x => m.Name != x.Name)))
				AddMethod(mi);
				*/
		}

		protected virtual void AddIConfigurationImplementation() {
			var mi = typeof(IConfiguration).GetMethod(nameof(IConfiguration.Copy), new[] { typeof(IConfiguration) });
			Debug.Assert(mi != null, "IConfiguration.Copy(IConfiguration) method not found");

			var mb = DefineMethod(nameof(IConfiguration.Copy), typeof(void), typeof(IConfiguration));
			var il = mb.GetILGenerator();
			il.Print("Call " + ClassName + "." + mi.Name + "()");

			var comparsion = il.DefineLabel();
			var end = il.DefineLabel();

			// if (ReferenceEquals(source, null))
			il.Emit(OpCodes.Ldarg_1);
			il.Emit(OpCodes.Ldnull);
			il.Emit(OpCodes.Ceq);
			il.Emit(OpCodes.Brfalse_S, comparsion);

			// throw new ArgumentNullException(nameof(source));
			il.Emit(OpCodes.Ldstr, mi.GetParameters().FirstOrDefault()?.Name ?? "cfg");
			var ctor = typeof(ArgumentNullException).GetConstructor(new [] { typeof(string) });
			Debug.Assert(ctor != null, "Can't find ArgumentNullException's ctor");
			il.Emit(OpCodes.Newobj, ctor);
			il.Emit(OpCodes.Throw);
			
			il.MarkLabel(comparsion);

			// if (ReferenceEquals(this, source)) return;
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Ldarg_1);
			il.Emit(OpCodes.Ceq);
			il.Emit(OpCodes.Brtrue, end);
			
			// copy props
			foreach (var pi in Tb.GetInterfaces().SelectMany(t => t.GetProperties())) {
				il.Print("copy property " + pi.Name);

				var loc = il.DeclareLocal(pi.PropertyType);
				il.Emit(OpCodes.Ldarg_0);
				il.Emit(OpCodes.Ldarg_1);
				il.Emit(OpCodes.Castclass, typeof(T));
				il.Emit(OpCodes.Callvirt, pi.GetGetMethod());
				
				il.Emit(OpCodes.Call, pi.GetSetMethod());
			}

			il.MarkLabel(end);
			il.Print("End of " + ClassName + "." + mi.Name + "()");
			il.Emit(OpCodes.Ret);
			Tb.DefineMethodOverride(mb, mi);
		}

		protected virtual void AddConstructors() {
			AddDefaultConstructor();
			AddCopyConstructor();
		}

		protected virtual void AddDefaultConstructor() {
			var ctor = DefineConstructor();
			var il = ctor.GetILGenerator();
			il.Print("Call new " + ClassName + "()");

			foreach (var dv in _fieldValues)
				dv.Attr.GenerateFieldCtorCode(il, dv.Fb);

			il.Emit(OpCodes.Ret);
		}

		protected virtual void AddCopyConstructor() {
			var ctor = DefineConstructor(typeof(T));
			var il = ctor.GetILGenerator();
			il.Print("Call new " + ClassName + "(" + typeof(T).Name + ")");

			foreach (var pi in Tb.GetInterfaces().SelectMany(t => t.GetProperties())) {
				var type = pi.PropertyType;
				var isIList = type.IsGenericType && type == typeof(IList<>).MakeGenericType(type.GetGenericArguments()[0]);

				il.Print("Copy " + pi.Name + " property");

				if (isIList) {
					var fb = GetPrivateField(GetFieldName(pi));

					var itemType = type.GetGenericArguments()[0];
					var argTypes = new [] { typeof(IEnumerable<>).MakeGenericType(itemType)} ;
					var listType = typeof(ObservableCollection<>).MakeGenericType(itemType);
					var listCtor = listType.GetConstructor(argTypes);
					Debug.Assert(listCtor != null, nameof(listCtor) + " != null");

					il.Emit(OpCodes.Ldarg_1);
					il.Emit(OpCodes.Callvirt, pi.GetGetMethod());
					var get = il.DeclareLocal(pi.PropertyType);
					il.Emit(OpCodes.Stloc, get);

#if DEBUG
					var collectionType = typeof(ICollection<>).MakeGenericType(itemType);
					var getCountMethod = collectionType.GetMethod("get_Count");
					Debug.Assert(getCountMethod != null, nameof(getCountMethod) + " != null");
					il.PrintPart("copy items: ");
					il.Emit(OpCodes.Ldloc, get);
					il.Emit(OpCodes.Callvirt, getCountMethod);
					il.Emit(OpCodes.Box, typeof(int));
					il.Emit(OpCodes.Call, typeof(Debug).GetMethod(nameof(Debug.WriteLine), new[] { typeof(object) }));
#endif

					il.Emit(OpCodes.Ldarg_0);
					il.Emit(OpCodes.Ldloc, get);
					il.Emit(OpCodes.Newobj, listCtor);
					il.Emit(OpCodes.Stfld, fb);
				} else {
					il.Emit(OpCodes.Ldarg_0);
					il.Emit(OpCodes.Ldarg_1);
					il.Emit(OpCodes.Castclass, typeof(T));
					il.Emit(OpCodes.Callvirt, pi.GetGetMethod());
					var fb = GetPrivateField(GetFieldName(pi));
					il.Emit(OpCodes.Stfld, fb);
				}
			}

			il.Emit(OpCodes.Ret);
		}

		protected virtual PropertyBuilder DefineProperty(string name, Type type) {
			Debug.Print(ClassName + " defining property " + name);
			return Tb.DefineProperty(name, PropertyAttributes.None, type, Type.EmptyTypes);
		}

		protected virtual ConstructorBuilder DefineConstructor(params Type[] args) {
			Debug.Print($"Defining {ClassName}({args.Select(type => type.Name).Combine()})");
			return Tb.DefineConstructor(MethodAttributes.Public, CallingConventions.HasThis, args);
		}

		protected virtual MethodBuilder DefineMethod(string name, Type returnType, params Type[] args) {
#if DEBUG
			Debug.Print(
				$"Defining method public virtual {returnType.Name} {ClassName}.{name}({args.Select(type => type.Name).Combine()})");
#endif
			return Tb.DefineMethod(name, MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.HideBySig,
				CallingConventions.HasThis | CallingConventions.Standard, returnType, args);
		}

		protected virtual MethodBuilder DefineMethod(string name, MethodAttributes attrs, Type returnType, params Type[] args) {
#if DEBUG
			Debug.Print(
				$"Defining method {attrs} {returnType.Name} {ClassName}.{name}({args.Select(type => type.Name).Combine()})");
#endif
			return Tb.DefineMethod(name, attrs, CallingConventions.HasThis | CallingConventions.Standard, returnType, args);
		}

		protected virtual void AddSimpleProperty(PropertyInfo pi) {
			var pb = DefineProperty(pi.Name, pi.PropertyType);
			pb.AddAttribute<DataMemberAttribute>();

			var fb = GetOrCreatePrivateField(GetFieldName(pi), pi.PropertyType);
			var dvp = pi.GetCustomAttributes(true).OfType<IDefaultValueProvider>().FirstOrDefault();
			if (dvp != null)
				AddDefaultFieldValue(fb, dvp);

			var spb = new SimplePropertyBuilder(pi, pb, fb, dvp);

			if (pi.CanRead)
				AddSimplePropertyGetter(spb);

			if (pi.CanWrite)
				AddSimplePropertySetter(spb);
		}

		protected virtual void AddSimplePropertyGetter(in SimplePropertyBuilder spb) {
			var mi = spb.Pi.GetGetMethod();
			var mb = DefineMethod(mi.Name, spb.Type);
			ImplementSimplePropertyGetMethod(spb, mi, mb);
			spb.Pb.SetGetMethod(mb);
			DefineMethodOverride(mb, mi);
		}

		protected virtual void AddSimplePropertySetter(in SimplePropertyBuilder spb) {
			var mi = spb.Pi.GetSetMethod();
			var mb = DefineMethod(mi.Name, typeof(void), spb.Type);

			ImplementSimplePropertySetMethod(spb, mi, mb);
			spb.Pb.SetSetMethod(mb);
			DefineMethodOverride(mb, mi);
		}

		protected virtual void ImplementSimplePropertyGetMethod(in SimplePropertyBuilder spb, MethodInfo mi,
			MethodBuilder mb) {
			var il = mb.GetILGenerator();
			il.Print("Call " + ClassName + "." + mb.Name);

			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Ldfld, spb.Fb);
			il.Emit(OpCodes.Ret);
		}

		protected virtual void ImplementSimplePropertySetMethod(in SimplePropertyBuilder spb, MethodInfo mi, MethodBuilder mb) {
			var il = mb.GetILGenerator();
			il.Print("Call " + ClassName + "." + mb.Name);

			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Ldarg_1);
			il.Emit(OpCodes.Stfld, spb.Fb);
			il.Emit(OpCodes.Ret);
		}

		protected virtual void DefineMethodOverride(MethodBuilder mb, MethodInfo mi) {
			_propMethods.Add(mb);
			Tb.DefineMethodOverride(mb, mi);
		}

		protected virtual void AddIListProperty(PropertyInfo pi, Type itemType) {
			var ipb = DefineProperty(pi.Name, pi.PropertyType); // interface property
			ipb.AddAttribute<XmlIgnoreAttribute>();

			var ifb = GetOrCreatePrivateField(GetFieldName(pi), pi.PropertyType);
			var dvp = pi.GetCustomAttributes(true).OfType<IDefaultValueProvider>().FirstOrDefault() ?? GetIListDefaultValue(pi);
			if (dvp != null)
				AddDefaultFieldValue(ifb, dvp);

			var propType = itemType.MakeArrayType();
			var propName = ListPropertyBuilder.ArrPropertyPrefix + pi.Name;
			var apb = DefineProperty(propName, propType);       // array xml property
			apb.AddAttribute<DataMemberAttribute>();
			apb.AddAttribute<XmlArrayAttribute>(pi.Name);
			apb.AddAttribute<XmlArrayItemAttribute>(itemType.Name);

			var lpb = new ListPropertyBuilder(pi, ipb, ifb, apb, dvp);
			if (pi.CanRead)
				AddIListPropertyGetter(lpb);

			if (pi.CanWrite)
				AddIListPropertySetter(lpb);
		}

		protected virtual IDefaultValueProvider GetIListDefaultValue(PropertyInfo pi)
			=> new ListDefaultValueProvider(pi.PropertyType.GetGenericArguments()[0]);

		protected virtual void AddIListPropertyGetter(in ListPropertyBuilder lpb) {
			var imi = lpb.Ipi.GetGetMethod();
			var imb = DefineMethod(imi.Name, 
				// imi.Attributes & ~MethodAttributes.Abstract, 
				MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Virtual | MethodAttributes.SpecialName, 
				lpb.IType);

			ImplementIListPropertyGetter(lpb, imb);
			lpb.Ipb.SetGetMethod(imb);
			DefineMethodOverride(imb, imi);

			var amb = DefineMethod(
				lpb.Ipi.GetGetMethod().Name.Replace(lpb.Ipi.Name, lpb.AName),
				MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Final, lpb.AType);

			ImplementIListArrayPropertyGetter(lpb, amb);
			lpb.Apb.SetGetMethod(amb);
			// Tb.DefineMethodOverride(amb, amb);
		}

		protected virtual void ImplementIListPropertyGetter(in ListPropertyBuilder lpb, MethodBuilder mb) {
			var il = mb.GetILGenerator();
			il.Print("Call " + ClassName + "." + mb.Name + "()");


			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Ldfld, lpb.Ifb);

			il.Print("End of " + ClassName + "." + mb.Name + "()");

#if DEBUG
			var collectionType = typeof(ICollection<>).MakeGenericType(lpb.ItemType);
			var getCountMethod = collectionType.GetMethod("get_Count");
			Debug.Assert(getCountMethod != null, nameof(getCountMethod) + " != null");
			il.PrintPart("return items: ");
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Ldfld, lpb.Ifb);
			il.Emit(OpCodes.Callvirt, getCountMethod);
			il.Emit(OpCodes.Box, typeof(Int32));
			il.Emit(OpCodes.Call, typeof(Debug).GetMethod(nameof(Debug.WriteLine), new[] { typeof(object) }));
#endif

			il.Emit(OpCodes.Ret);
		}

		protected virtual void ImplementIListArrayPropertyGetter(in ListPropertyBuilder lpb, MethodBuilder mb) {
			var miToArray = typeof(Enumerable).GetMethod(nameof(Enumerable.ToArray))?.MakeGenericMethod(lpb.ItemType);
			Debug.Assert(miToArray != null, nameof(miToArray) + " != null");

			var il = mb.GetILGenerator();
			il.Print("Call " + ClassName + "." + mb.Name + "()");

			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Callvirt, lpb.Ipb.GetGetMethod());
			il.Emit(OpCodes.Call, miToArray);

			il.Print("End of " + ClassName + "." + mb.Name + "()");
			il.Emit(OpCodes.Ret);
		}

		protected virtual void AddIListPropertySetter(in ListPropertyBuilder lpb) {
			var imi = lpb.Ipi.GetSetMethod();
			// var imb = DefineMethod(imi.Name, imi.Attributes & ~MethodAttributes.Abstract, typeof(void), lpb.IType);
			var imb = DefineMethod(imi.Name,
				MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Virtual | MethodAttributes.SpecialName,
				typeof(void), lpb.IType);

			ImplementIListPropertySetter(lpb, imb);
			lpb.Ipb.SetSetMethod(imb);
			DefineMethodOverride(imb, imi);

			var amb = DefineMethod(
				lpb.Ipi.GetSetMethod().Name.Replace(lpb.Ipi.Name, lpb.AName),
				MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Final, typeof(void), lpb.AType);

			ImplementIListArrayPropertySetter(lpb, amb);
			lpb.Apb.SetSetMethod(amb);
			// Tb.DefineMethodOverride(amb, amb);
		}

		protected virtual void ImplementIListPropertySetter(in ListPropertyBuilder lpb, MethodBuilder mb) {
			var obsType = typeof(ObservableCollection<>).MakeGenericType(lpb.ItemType);

			var propType = lpb.Ipi.PropertyType;
			var listType = typeof(IList<>).MakeGenericType(lpb.ItemType);
			var collectionType = typeof(ICollection<>).MakeGenericType(lpb.ItemType);

			// var clearMethod = collectionType.GetMethod(nameof(ICollection<object>.Clear));
			var clearMethod = propType.GetInterfaces().Select(x => x.GetMethod("Clear")).FirstOrDefault();
			Debug.Assert(clearMethod != null, nameof(clearMethod) + " != null");

			// var getCountMethod = collectionType.GetMethod("get_Count");
			// var getCountMethod = lpb.Ipi.PropertyType.GetInterfaces().Select(i => i.GetMethod("get_Count", Type.EmptyTypes)).FirstOrDefault();
			// var getCountMethod = lpb.IType.GetInterfaces().Select(i => i.GetMethod("get_Count", Type.EmptyTypes)).FirstOrDefault();
			// var getCountMethod = collectionType.GetMethod("get_Count");
			var getCountMethod = collectionType.GetProperty("Count").GetGetMethod();
			Debug.Assert(getCountMethod != null, nameof(getCountMethod) + " != null");

			var getItemMethod = listType.GetMethod("get_Item");
			Debug.Assert(getItemMethod != null, nameof(getItemMethod) + " != null");

			var addMethod = collectionType.GetMethod(nameof(ICollection<object>.Add));
			Debug.Assert(addMethod != null, nameof(addMethod) + " != null");

			var il = mb.GetILGenerator();
			il.Print("Call " + ClassName + "." + mb.Name + "(...)");

			var loopFor = il.DefineLabel();
			var loopBegin = il.DefineLabel();
			var methodEnd = il.DefineLabel();

			var items = il.DeclareLocal(propType);
			var count = il.DeclareLocal(typeof(int));
			var i = il.DeclareLocal(typeof(int));

			// Debug.WriteLine(_field.Count);
			il.PrintPart("old field count: ");
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Ldfld, lpb.Ifb);
			il.Emit(OpCodes.Callvirt, getCountMethod);
			il.Emit(OpCodes.Box, typeof(Int32));
			il.Emit(OpCodes.Call, typeof(Debug).GetMethod(nameof(Debug.WriteLine), new[] { typeof(object) }));
			
			// _field.Clear();
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Ldfld, lpb.Ifb);
			il.Emit(OpCodes.Callvirt, clearMethod);

			// var items = value;
			il.Emit(OpCodes.Ldarg_1);
			il.Emit(OpCodes.Stloc, items);
			
			// var count = items.Count;
			il.Emit(OpCodes.Ldloc, items);
			il.Emit(OpCodes.Callvirt, getCountMethod);
			il.Emit(OpCodes.Stloc, count);

			// Debug.WriteLine(count);
			il.PrintPart("expected count: ");
			il.Emit(OpCodes.Ldloc, count);
			il.Emit(OpCodes.Box, typeof(Int32));
			il.Emit(OpCodes.Call, typeof(Debug).GetMethod(nameof(Debug.WriteLine), new[] { typeof(object) }));

			// var i = 0;
			il.Emit(OpCodes.Ldc_I4_0);
			il.Emit(OpCodes.Stloc, i);
			
			// goto loopFor
			il.Emit(OpCodes.Br_S, loopFor);
			
			il.MarkLabel(loopBegin);

			// _field.Add(items[i]);
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Ldfld, lpb.Ifb);
			il.Emit(OpCodes.Ldloc, items);
			il.Emit(OpCodes.Ldloc, i);
			il.Emit(OpCodes.Callvirt, getItemMethod);
			il.Emit(OpCodes.Callvirt, addMethod);
			
			// i++;
			il.Emit(OpCodes.Ldloc, i);
			il.Emit(OpCodes.Ldc_I4_1);
			il.Emit(OpCodes.Add);
			il.Emit(OpCodes.Stloc, i);

			il.MarkLabel(loopFor);
			
			// if (i < count) goto loopBegin;
			il.Emit(OpCodes.Ldloc, i);
			il.Emit(OpCodes.Ldloc, count);
			il.Emit(OpCodes.Blt_S, loopBegin);
			
			il.MarkLabel(methodEnd);
			il.Print("End of " + ClassName + "." + mb.Name + "(...)");
			il.Emit(OpCodes.Ret);
		}

		protected virtual void ImplementIListArrayPropertySetter(in ListPropertyBuilder lpb, MethodBuilder mb) {
			var il = mb.GetILGenerator();
			il.Print("Call " + ClassName + "." + mb.Name + "(...)");

			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Ldarg_1);
			il.Emit(OpCodes.Callvirt, lpb.Ipb.GetSetMethod());

			il.Print("End of " + ClassName + "." + mb.Name + "(...)");
			il.Emit(OpCodes.Ret);
		}

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
			=> "_" + ToLowerFirstLetter(pi.Name);

		private string ToLowerFirstLetter(string value)
			=> value.Length > 1
				? value[0].ToString().ToLowerInvariant() + value.Substring(1)
				: value.ToLowerInvariant();

		protected virtual void AddDefaultFieldValue(FieldBuilder fb, IDefaultValueProvider attr) {
			_fieldValues.Add((fb, attr));
			Debug.Print(ClassName + " for field " + fb.Name + " has default value " + attr);
		}

		private class ListDefaultValueProvider : IDefaultValueProvider {
			public ListDefaultValueProvider(Type itemType) => _itemType = itemType;

			public void GenerateFieldCtorCode(ILGenerator il, FieldBuilder fb) {
				var type = typeof(List<>).MakeGenericType(_itemType);
				var ctor = type.GetConstructor(Type.EmptyTypes);
				Debug.Assert(ctor != null, "Can't find List<" + _itemType.Name + "> ctor");

				il.Print("Creating field " + fb.DeclaringType +" "+ fb.Name + "(actual: " + type.Name+ ")");
				il.Emit(OpCodes.Newobj, ctor);
				il.Emit(OpCodes.Stfld, fb);
			}

			public override string ToString() => "List<" + typeof(T).Name + ">";

			private readonly Type _itemType;
		}

		protected readonly struct SimplePropertyBuilder {
			public readonly PropertyInfo Pi;
			public readonly PropertyBuilder Pb;
			public readonly FieldBuilder Fb;
			public readonly IDefaultValueProvider Dvp;
			public readonly Type Type;

			public SimplePropertyBuilder(PropertyInfo pi, PropertyBuilder pb, FieldBuilder fb, IDefaultValueProvider dvp = null) {
				Pi = pi ?? throw new ArgumentNullException(nameof(pi));
				Pb = pb ?? throw new ArgumentNullException(nameof(pb));
				Fb = fb ?? throw new ArgumentNullException(nameof(fb));
				Dvp = dvp;
				Type = pi.PropertyType;
			}
		}

		protected readonly struct ListPropertyBuilder {
			public const string ArrPropertyPrefix = "array__";

			public readonly PropertyInfo Ipi;
			public readonly PropertyBuilder Ipb;
			public readonly FieldBuilder Ifb;
			public readonly IDefaultValueProvider Dvp;

			// public readonly PropertyInfo Api;
			public readonly PropertyBuilder Apb;

			public readonly Type IType;
			public readonly Type AType;
			public readonly Type ItemType;

			public readonly string AName;

			public ListPropertyBuilder(PropertyInfo ipi, PropertyBuilder ipb, FieldBuilder ifb, PropertyBuilder apb,
				IDefaultValueProvider dvp = null) {
				Ipi = ipi ?? throw new ArgumentNullException(nameof(ipi));
				Ipb = ipb ?? throw new ArgumentNullException(nameof(ipb));
				Ifb = ifb ?? throw new ArgumentNullException(nameof(ifb));
				Dvp = dvp;

				Apb = apb ?? throw new ArgumentNullException(nameof(apb));

				IType = ipi.PropertyType;
				AType = apb.PropertyType;
				AName = ArrPropertyPrefix + ipi.Name;
				ItemType = IType.GetGenericArguments()[0];
			}
		}

		private string _className;
		private readonly List<FieldBuilder> _fields = new List<FieldBuilder>();
		private readonly List<MethodBuilder> _propMethods = new List<MethodBuilder>();
		protected readonly List<(FieldBuilder Fb, IDefaultValueProvider Attr)> _fieldValues = new List<(FieldBuilder Fb, IDefaultValueProvider Attr)>();

		private static ModuleBuilder _mb;
	}
}