using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Diagnostics.SymbolStore;
using System.Diagnostics;

namespace ArtZilla.Config.Builders {
	public static class PropertyChangedInvoker {
		public static void Invoke(INotifyPropertyChanged sender, PropertyChangedEventHandler source, string propertyName)
			=> source?.Invoke(sender, new PropertyChangedEventArgs(propertyName));
	}

	public class NotifyingConfigTypeBuilder<T>: CopyConfigTypeBuilder<T> where T : IConfiguration {
		class NotifyingIListDefaultValueProvider: IDefaultValueProvider {
			public NotifyingIListDefaultValueProvider(Type itemType) => _itemType = itemType;

			public void GenerateFieldCtorCode(ILGenerator il, FieldBuilder fb) {
				var type = typeof(ObservableCollection<>).MakeGenericType(_itemType);
				var ctor = type.GetConstructor(Type.EmptyTypes);
				Debug.Assert(ctor != null, nameof(ctor) + " != null");

				il.Emit(OpCodes.Ldarg_0);
				il.Emit(OpCodes.Newobj, ctor);
				il.Emit(OpCodes.Stfld, fb);
			}

			private readonly Type _itemType;
		}

		protected override string ClassPrefix => "Notifying";

		protected override void AddInterfaces() {
			AddInpcImplementation();
			base.AddInterfaces();
		}

		protected override IDefaultValueProvider GetIListDefaultValue(PropertyInfo pi)
			=> new NotifyingIListDefaultValueProvider(pi.PropertyType.GetGenericArguments()[0]);

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
					MethodAttributes.NewSlot, CallingConventions.Standard | CallingConventions.HasThis, typeof(void),
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

		protected override void ImplementPropertySetMethod(PropertyInfo pi, PropertyBuilder pb, MethodInfo mi, MethodBuilder mb) {
			Debug.WriteLine($"NotifyingConfigTypeBuilder.ImplementPropertySetMethod {pi.Name} ({pi.PropertyType})");

			var fb = GetPrivateField(GetFieldName(pi));
			var il = mb.GetILGenerator();

			var endOfMethod = il.DefineLabel();

			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Ldfld, fb);
			il.Emit(OpCodes.Ldarg_1);

			var type = pi.PropertyType;
			if (IsEqualityType(type)) {
				var method = type.GetMethod("op_Equality", new[] { type, type });
				Debug.Assert(method != null);

				il.Emit(OpCodes.Call, method);
			} else {
				il.Emit(OpCodes.Ceq);
			}

			il.Emit(OpCodes.Brtrue_S, endOfMethod);

			// some code if they are not equal
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Ldarg_1);
			il.Emit(OpCodes.Stfld, fb);

			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Ldstr, pi.Name);
			il.Emit(OpCodes.Call, _onPropertyChangedMethod);

			// this marks the return point
			il.MarkLabel(endOfMethod);
			il.Emit(OpCodes.Ret);
		}

		private bool IsEqualityType(Type type) {
			return type.GetMethod("op_Equality", new[] { type, type }) != null;
		}

		protected MethodInfo _onPropertyChangedMethod;
	}
}
