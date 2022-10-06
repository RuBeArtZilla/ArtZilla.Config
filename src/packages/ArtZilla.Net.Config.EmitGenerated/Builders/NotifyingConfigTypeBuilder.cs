using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;

namespace ArtZilla.Net.Config.Builders;

public static class PropertyChangedInvoker {
	public static void Invoke(INotifyPropertyChanged sender, PropertyChangedEventHandler source, string propertyName)
		=> source?.Invoke(sender, new(propertyName));
}

public class NotifyingConfigTypeBuilder<T> : CopyConfigTypeBuilder<T> where T : IConfiguration {
	protected override string ClassPrefix => "Notifying";

	protected override void AddInterfaces() {
		AddInpcImplementation();
		base.AddInterfaces();
	}

	/*
	protected override IDefaultValueProvider GetIListDefaultValue(PropertyInfo pi)
		=> new NotifyingIListDefaultValueProvider(pi.PropertyType.GetGenericArguments()[0]);
	*/

	protected virtual void AddInpcImplementation() {
		// todo: refactor this method?

		Tb.AddInterfaceImplementation(typeof(INotifyPropertyChanged));
		Tb.AddInterfaceImplementation(typeof(INotifyingConfiguration));

		var field = Tb.DefineField(
			"PropertyChanged",
			typeof(PropertyChangedEventHandler),
			FieldAttributes.Private);

		var eventInfo = Tb.DefineEvent(
			"PropertyChanged",
			EventAttributes.None,
			typeof(PropertyChangedEventHandler));

		{
			var ibaseMethod = typeof(INotifyPropertyChanged).GetMethod("add_PropertyChanged");
			Debug.Assert(ibaseMethod != null, nameof(ibaseMethod) + " != null");

			var addMethod = Tb.DefineMethod("add_PropertyChanged",
				ibaseMethod.Attributes ^ MethodAttributes.Abstract,
				ibaseMethod.CallingConvention,
				ibaseMethod.ReturnType,
				new[] { typeof(PropertyChangedEventHandler) });

			var combine = typeof(Delegate).GetMethod("Combine", new[] { typeof(Delegate), typeof(Delegate) });
			Debug.Assert(combine != null, nameof(combine) + " != null");

			var generator = addMethod.GetILGenerator();
			generator.Emit(OpCodes.Ldarg_0);
			generator.Emit(OpCodes.Ldarg_0);
			generator.Emit(OpCodes.Ldfld, field);
			generator.Emit(OpCodes.Ldarg_1);
			generator.Emit(OpCodes.Call, combine);
			generator.Emit(OpCodes.Castclass, typeof(PropertyChangedEventHandler));
			generator.Emit(OpCodes.Stfld, field);
			generator.Emit(OpCodes.Ret);
			eventInfo.SetAddOnMethod(addMethod);
		}

		{
			var ibaseMethod = typeof(INotifyPropertyChanged).GetMethod("remove_PropertyChanged");
			Debug.Assert(ibaseMethod != null, nameof(ibaseMethod) + " != null");

			var removeMethod = Tb.DefineMethod("remove_PropertyChanged",
				ibaseMethod.Attributes ^ MethodAttributes.Abstract,
				ibaseMethod.CallingConvention,
				ibaseMethod.ReturnType,
				new[] { typeof(PropertyChangedEventHandler) });
			var remove = typeof(Delegate).GetMethod("Remove", new[] { typeof(Delegate), typeof(Delegate) });
			Debug.Assert(remove != null, nameof(remove) + " != null");

			var generator = removeMethod.GetILGenerator();
			generator.Emit(OpCodes.Ldarg_0);
			generator.Emit(OpCodes.Ldarg_0);
			generator.Emit(OpCodes.Ldfld, field);
			generator.Emit(OpCodes.Ldarg_1);
			generator.Emit(OpCodes.Call, remove);
			generator.Emit(OpCodes.Castclass, typeof(PropertyChangedEventHandler));
			generator.Emit(OpCodes.Stfld, field);
			generator.Emit(OpCodes.Ret);
			eventInfo.SetRemoveOnMethod(removeMethod);
		}

		{
			var methodBuilder = Tb.DefineMethod("OnPropertyChanged",
				MethodAttributes.Family | MethodAttributes.Virtual | MethodAttributes.HideBySig |
				MethodAttributes.NewSlot, CallingConventions.HasThis, typeof(void),
				new[] { typeof(string) });
			var generator = methodBuilder.GetILGenerator();
			var returnLabel = generator.DefineLabel();
			generator.DeclareLocal(typeof(PropertyChangedEventHandler));
			generator.Emit(OpCodes.Ldarg_0);
			generator.Emit(OpCodes.Ldarg_0);
			generator.Emit(OpCodes.Ldfld, field);
			generator.Emit(OpCodes.Ldarg_1);
			generator.Emit(OpCodes.Call, typeof(PropertyChangedInvoker).GetMethod("Invoke"));
			generator.MarkLabel(returnLabel);
			generator.Emit(OpCodes.Ret);
			eventInfo.SetRaiseMethod(methodBuilder);
			_onPropertyChangedMethod = methodBuilder.GetBaseDefinition();
		}
	}

	protected override void ImplementSimplePropertySetMethod(in SimplePropertyBuilder spb, MethodInfo mi, MethodBuilder mb) {
		var il = mb.GetILGenerator();
		il.Print("Call " + ClassName + "." + mb.Name);

		var endOfMethod = il.DefineLabel();

		il.Emit(OpCodes.Ldarg_0);
		il.Emit(OpCodes.Ldfld, spb.Fb);
		il.Emit(OpCodes.Ldarg_1);

		if (IsEqualityType(spb.Type)) {
			Debug.WriteLine($"{spb.Type} is equality type");

			var method = spb.Type.GetMethod("op_Equality", new[] { spb.Type, spb.Type });
			Debug.Assert(method != null);

			il.Emit(OpCodes.Call, method);
		} else {
			Debug.WriteLine($"{spb.Type} is not equality type");
			// for user defined struct should exist operator ==
			if (spb.Type.IsValueType && !spb.Type.IsPrimitive && !spb.Type.IsEnum)
				throw new($"Type {spb.Type} of property {spb.Pi.Name} has no operator ==");

			il.Emit(OpCodes.Ceq);
		}

		il.Emit(OpCodes.Brtrue_S, endOfMethod);

#if DEBUG
		{
			var dbgMethod = typeof(Debug).GetMethod(nameof(Debug.WriteLine), new[] { typeof(string), typeof(object[]) });
			Debug.Assert(dbgMethod != null, nameof(Debug.WriteLine) + " not found");

			il.Emit(OpCodes.Ldstr, $"Change property {spb.Pi.Name} from {{0}} to {{1}}");
			il.Emit(OpCodes.Ldc_I4_2);
			il.Emit(OpCodes.Newarr, typeof(object));
			il.Emit(OpCodes.Dup);
			il.Emit(OpCodes.Ldc_I4_0);
			il.Emit(OpCodes.Ldarg_0); // this
			il.Emit(OpCodes.Ldfld, spb.Fb);
			il.Emit(OpCodes.Box, spb.Type);
			il.Emit(OpCodes.Stelem_Ref);
			il.Emit(OpCodes.Dup);
			il.Emit(OpCodes.Ldc_I4_1);
			il.Emit(OpCodes.Ldarg_1);
			il.Emit(OpCodes.Box, spb.Type);
			il.Emit(OpCodes.Stelem_Ref);
			il.Emit(OpCodes.Call, dbgMethod);
			il.Emit(OpCodes.Nop);
		}
#endif

		// some code if they are not equal
		il.Emit(OpCodes.Ldarg_0);
		il.Emit(OpCodes.Ldarg_1);
		il.Emit(OpCodes.Stfld, spb.Fb);


		il.Emit(OpCodes.Ldarg_0);
		il.Emit(OpCodes.Ldstr, spb.Pi.Name);
		il.Emit(OpCodes.Call, _onPropertyChangedMethod);


		// this marks the return point
		il.MarkLabel(endOfMethod);
		il.Emit(OpCodes.Ret);
	}

	bool IsEqualityType(Type type) => type.GetMethod("op_Equality", new[] { type, type }) != null;

	protected MethodInfo _onPropertyChangedMethod;

	/*class NotifyingIListDefaultValueProvider : IDefaultValueProvider {
		public NotifyingIListDefaultValueProvider(Type itemType) => _itemType = itemType;

		public void GenerateFieldCtorCode(ILGenerator il, FieldBuilder fb) {
			var type = typeof(ObservableCollection<>).MakeGenericType(_itemType);
			var ctor = type.GetConstructor(Type.EmptyTypes);
			Debug.Assert(ctor != null, nameof(ctor) + " != null");

			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Newobj, ctor);
			il.Emit(OpCodes.Stfld, fb);
		}

		public override string ToString() => "ObservableCollection<" + typeof(T).Name + ">";

		private readonly Type _itemType;
	}*/
}