using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace ArtZilla.Config.Builders {
	public class RealtimeConfigTypeBuilder<T>: NotifyingConfigTypeBuilder<T> where T : IConfiguration {
		protected override String ClassPrefix => "Realtime";

		protected override void AddInterfaces() {
			AddRealtimeImplementation();
			base.AddInterfaces();
		}

		protected virtual void AddRealtimeImplementation() {
			Tb.AddInterfaceImplementation(typeof(IRealtimeConfiguration));
		}

		/*	protected override void ImplementPropertySetMethod(PropertyInfo pi, PropertyBuilder pb, MethodInfo mi, MethodBuilder mb) {
			var fb = GetPrivateField(GetFieldName(pi));
			var il = mb.GetILGenerator();
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Ldarg_1);
			il.Emit(OpCodes.Stfld, fb);
			il.Emit(OpCodes.Ret);
		}*/
	}
}
