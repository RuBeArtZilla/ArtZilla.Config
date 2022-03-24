using System;
using System.Reflection.Emit;
using ArtZilla.Net.Config.Builders;

namespace ArtZilla.Net.Config; 

[AttributeUsage(AttributeTargets.Property)]
[Obsolete("Will be replaced by System.ComponentModel.DefaultValueAttribute", false)]
internal class DefaultValueAttribute : Attribute, IDefaultValueProvider {
	public object Value { get; }

	public DefaultValueAttribute(object value)
		=> Value = value;

	public void GenerateFieldCtorCode(ILGenerator il, FieldBuilder fb) {
		il.Emit(OpCodes.Ldarg_0);
		var value = fb.FieldType.IsEnum ? Value : Convert.ChangeType(Value, fb.FieldType);
		il.Print($"Ctor set field {fb.Name} to {value}");
		PushObject(il, value);
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