using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Reflection.Emit;
using CommunityToolkit.Diagnostics;

namespace ArtZilla.Net.Config.Builders;

/// Extension methods for type builders
public static class BuilderUtils {
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

	public static void PushObject(this ILGenerator il, object value) {
		switch (value) {
			case sbyte x: {
				il.Emit(OpCodes.Ldc_I4, (int) x);
				il.Emit(OpCodes.Conv_I1);
				return;
			}

			case short x: {
				il.Emit(OpCodes.Ldc_I4, (int) x);
				il.Emit(OpCodes.Conv_I2);
				return;
			}

			case int x: {
				il.Emit(OpCodes.Ldc_I4, x);
				return;
			}

			case long x: {
				il.Emit(OpCodes.Ldc_I8, x);
				return;
			}

			case byte x: {
				il.Emit(OpCodes.Ldc_I4, (int) x);
				il.Emit(OpCodes.Conv_I1);
				return;
			}

			case ushort x: {
				il.Emit(OpCodes.Ldc_I4, x);
				il.Emit(OpCodes.Conv_I2);
				return;
			}

			case uint x: {
				il.Emit(OpCodes.Ldc_I4, x);
				return;
			}

			case ulong x: {
				il.Emit(OpCodes.Ldc_I8, (long) x);
				return;
			}

			case char x: {
				il.Emit(OpCodes.Ldc_I4, x);
				return;
			}

			case bool x: {
				il.Emit(x ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);
				return;
			}

			case float x: {
				il.Emit(OpCodes.Ldc_R4, x);
				return;
			}

			case double x: {
				il.Emit(OpCodes.Ldc_R8, x);
				return;
			}

			case string x: {
				il.Emit(OpCodes.Ldstr, x);
				return;
			}

			case Enum x: {
				var type = x.GetType();
				var underlyingType = Enum.GetUnderlyingType(type);
				var v = Convert.ChangeType(x, underlyingType);
				PushObject(il, v);
				return;
			}
		}

		throw new BuildException("Type " + value.GetType() + " not supported yet.");
	}

#if NET60_OR_GREATER
	[DoesNotReturn]
#endif
	public static void ThrowNotFoundError(string whatNotFound)
		=> throw new("Can't find " + whatNotFound);

	public static void EmitCallDebugWriteLine(this ILGenerator il)
		=> il.Emit(OpCodes.Call, _debugWriteLine);

	static BuilderUtils() {
		_debugWriteLine = typeof(Debug).GetMethod(nameof(Debug.WriteLine), new[] { typeof(object) })!;
		Guard.IsNotNull(_debugWriteLine, nameof(_debugWriteLine));
	}

	static MethodInfo _debugWriteLine;
}