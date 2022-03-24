using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Emit;

namespace ArtZilla.Net.Config.Builders;

public static class BuilderExt {
	public static void AddAttribute<TAttribute>(this TypeBuilder tb, params object[] args) where TAttribute : Attribute {
		var ctorArgs = args?.Select(a => a.GetType()).ToArray() ?? Type.EmptyTypes;
		var ctor = typeof(TAttribute).GetConstructor(ctorArgs);
		Debug.Assert(ctor != null, nameof(ctor) + " != null");
		tb.SetCustomAttribute(new (ctor, args ?? new object[0]));
	}

	public static void AddAttribute<TAttribute>(this PropertyBuilder pb, params object[] args) where TAttribute : Attribute {
		var ctorArgs = args?.Select(a => a.GetType()).ToArray() ?? Type.EmptyTypes;
		var ctor = typeof(TAttribute).GetConstructor(ctorArgs);
		Debug.Assert(ctor != null, nameof(ctor) + " != null");
		pb.SetCustomAttribute(new (ctor, args ?? new object[0]));
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