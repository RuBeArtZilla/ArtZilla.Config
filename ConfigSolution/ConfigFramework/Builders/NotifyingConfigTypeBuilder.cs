using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace ArtZilla.Config.Builders {
	public static class PropertyChangedInvoker {
		public static void Invoke(INotifyPropertyChanged sender, PropertyChangedEventHandler source, string propertyName)
			=> source?.Invoke(sender, new PropertyChangedEventArgs(propertyName));
	}

	public class NotifyingConfigTypeBuilder<T>: CopyConfigTypeBuilder<T> where T : IConfiguration {
		protected override String ClassPrefix => "Notifying";

		protected override void AddInterfaces() {
			AddInpcImplementation();
			base.AddInterfaces();
		}

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
				var addMethod = Tb.DefineMethod("add_PropertyChanged",
						ibaseMethod.Attributes ^ MethodAttributes.Abstract,
						ibaseMethod.CallingConvention,
						ibaseMethod.ReturnType,
						new[] { typeof(PropertyChangedEventHandler) });
				var generator = addMethod.GetILGenerator();
				var combine = typeof(Delegate).GetMethod("Combine", new[] { typeof(Delegate), typeof(Delegate) });
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
				var removeMethod = Tb.DefineMethod("remove_PropertyChanged",
						ibaseMethod.Attributes ^ MethodAttributes.Abstract,
						ibaseMethod.CallingConvention,
						ibaseMethod.ReturnType,
						new[] { typeof(PropertyChangedEventHandler) });
				var remove = typeof(Delegate).GetMethod("Remove", new[] { typeof(Delegate), typeof(Delegate) });
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
			var fb = GetPrivateField(GetFieldName(pi));
			var il = mb.GetILGenerator();

			Label endOfMethod = il.DefineLabel();

			// comparing property old and new value.
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Ldfld, fb);
			il.Emit(OpCodes.Ldarg_1);
			il.Emit(OpCodes.Ceq);

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

		protected MethodInfo _onPropertyChangedMethod;
	}
}
