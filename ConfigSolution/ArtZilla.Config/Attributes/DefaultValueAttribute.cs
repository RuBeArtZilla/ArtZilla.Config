using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using ArtZilla.Config.Builders;

namespace ArtZilla.Config {
	public interface IDefaultValueProvider {
		void GenerateFieldCtorCode(ILGenerator il, FieldBuilder fb);
	}

	[AttributeUsage(AttributeTargets.Property)]
	[Obsolete("Will be replaced by System.ComponentModel.DefaultValueAttribute", false)]
	internal class DefaultValueAttribute : Attribute, IDefaultValueProvider {
		public object Value { get; }

		public DefaultValueAttribute(object value)
			=> Value = value;

		[Obsolete]
		public DefaultValueAttribute(Type type, string strValue)
			=> Value = ConvertFromString(type, strValue);

		static object ConvertFromString(Type type, string strValue)
			=> _converters.TryGetValue(type, out var f)
				? f(strValue)
				: throw new ("Can't convert " + strValue + " to instance of type " + type.Name);

		static readonly Dictionary<Type, Func<string, object>> _converters
			= new () {
				[typeof(byte)] = s => byte.Parse(s),
				[typeof(sbyte)] = s => sbyte.Parse(s),
				[typeof(short)] = s => short.Parse(s),
				[typeof(int)] = s => int.Parse(s),
				[typeof(long)] = s => long.Parse(s),
				[typeof(ushort)] = s => ushort.Parse(s),
				[typeof(uint)] = s => uint.Parse(s),
				[typeof(ulong)] = s => ulong.Parse(s),

				[typeof(float)] = s => float.Parse(s),
				[typeof(double)] = s => double.Parse(s),
				[typeof(decimal)] = s => decimal.Parse(s, NumberStyles.Number),
				[typeof(bool)] = s => bool.Parse(s),

				[typeof(string)] = s => s,
			};

		public void GenerateFieldCtorCode(ILGenerator il, FieldBuilder fb) {
			il.Emit(OpCodes.Ldarg_0);
			PushObject(il, Value);
			il.Emit(OpCodes.Stfld, fb);
		}

		internal static void PushObject(ILGenerator il, object value) {
			switch (value) {
				case SByte x: {
					il.Emit(OpCodes.Ldc_I4, (Int32) x);
					il.Emit(OpCodes.Conv_I1);
					return;
				}

				case Int16 x: {
					il.Emit(OpCodes.Ldc_I4, (Int32) x);
					il.Emit(OpCodes.Conv_I2);
					return;
				}

				case Int32 x: {
					il.Emit(OpCodes.Ldc_I4, x);
					return;
				}

				case Int64 x: {
					il.Emit(OpCodes.Ldc_I8, x);
					return;
				}

				case Byte x: {
					il.Emit(OpCodes.Ldc_I4, (Int32) x);
					il.Emit(OpCodes.Conv_I1);
					return;
				}

				case UInt16 x: {
					il.Emit(OpCodes.Ldc_I4, x);
					il.Emit(OpCodes.Conv_I2);
					return;
				}

				case UInt32 x: {
					il.Emit(OpCodes.Ldc_I4, x);
					return;
				}

				case UInt64 x: {
					il.Emit(OpCodes.Ldc_I8, (Int64) x);
					return;
				}

				case Char x: {
					il.Emit(OpCodes.Ldc_I4, x);
					return;
				}

				case Boolean x: {
					il.Emit(x ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);
					return;
				}

				case Single x: {
					il.Emit(OpCodes.Ldc_R4, x);
					return;
				}

				case Double x: {
					il.Emit(OpCodes.Ldc_R8, x);
					return;
				}

				case String x: {
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
	}

	[AttributeUsage(AttributeTargets.Property)]
	public class DefaultValueByCtorAttribute : Attribute, IDefaultValueProvider {
		public Type Type { get; }

		public object[] Args { get; }

		public ConstructorInfo Ctor { get; }

		public DefaultValueByCtorAttribute(Type type, params object[] args) {
			Type = type;
			Args = args;
			Ctor = type.GetConstructor(args.Select(v => v.GetType()).ToArray());
		}

		public void GenerateFieldCtorCode(ILGenerator il, FieldBuilder fb) {
			il.Emit(OpCodes.Ldarg_0);

			foreach (var arg in Args)
				DefaultValueAttribute.PushObject(il, arg);

			il.Emit(OpCodes.Newobj, Ctor);
			il.Emit(OpCodes.Stfld, fb);

			/* il example
			  IL_0089: ldarg.0      // this
				IL_008a: ldstr        "{D1F71EC6-76A6-40F8-8910-68E67D753CD4}"
				IL_008f: newobj       instance void [mscorlib]System.Guid::.ctor(string)
				IL_0094: stfld        valuetype [mscorlib]System.Guid CfTests.TestConfiguration::'<Guid>k__BackingField'
				IL_0099: ldarg.0      // this
				IL_009a: call         instance void [mscorlib]System.Object::.ctor()
				IL_009f: nop          
				IL_00a0: ret
			 */
		}
	}

	[AttributeUsage(AttributeTargets.Property)]
	public class DefaultValueByMethodAttribute : Attribute, IDefaultValueProvider {
		public Type Type { get; }

		public object[] Args { get; }

		public string MethodName { get; }

		public DefaultValueByMethodAttribute(Type type, string methodName, params object[] args) {
			Type = type;
			Args = args;
			MethodName = methodName;
		}

		public void GenerateFieldCtorCode(ILGenerator il, FieldBuilder fb) {
			il.Emit(OpCodes.Ldarg_0);
			foreach (var arg in Args)
				DefaultValueAttribute.PushObject(il, arg);

			var method = Type.GetMethod(MethodName, BindingFlags.Public | BindingFlags.Static); // todo: find with args
			Debug.Assert(method != null);
			il.Emit(OpCodes.Call, method);
			il.Emit(OpCodes.Stfld, fb);

			/* il example
			 	IL_0000: ldarg.0
				IL_0001: call valuetype [mscorlib]System.Guid [mscorlib]System.Guid::NewGuid()
				IL_0006: stfld valuetype [mscorlib]System.Guid ArtZilla.Config.Tests.Class1::_guid
				IL_000b: ldarg.0
				IL_000c: call instance void [mscorlib]System.Object::.ctor()
				IL_0011: nop
				IL_0012: ret
			 
			 */
		}
	}
}