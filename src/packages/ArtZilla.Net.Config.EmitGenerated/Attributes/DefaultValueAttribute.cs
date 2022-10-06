using System.Diagnostics;
using System.Reflection.Emit;
using ArtZilla.Net.Config.Builders;
using CommunityToolkit.Diagnostics;

namespace ArtZilla.Net.Config;

public class DefaultConstValueProvider : IDefaultValueProvider {
	public object Value { get; }

	public DefaultConstValueProvider(object value)
		=> Value = value;

	public void GenerateFieldCtorCode(ILGenerator il, FieldBuilder fb) {
		il.Emit(OpCodes.Ldarg_0);
		var value = fb.FieldType.IsEnum ? Value : Convert.ChangeType(Value, fb.FieldType);
		il.Print($"Ctor set field {fb.Name} to {value}");
		il.PushObject(value);
		il.Emit(OpCodes.Stfld, fb);
	}
}

class ConfigListValueProvider : IDefaultValueProvider {
	readonly IDefaultValueProvider? _iternalProvider;
	readonly Type _itemType;
	readonly Type _listType;

	public ConfigListValueProvider(IDefaultValueProvider? iternalProvider, Type itemType) {
		_iternalProvider = iternalProvider;
		_itemType = itemType;
		_listType = typeof(ConfigList<>).MakeGenericType(_itemType);
	}

	/// <inheritdoc />
	public void GenerateFieldCtorCode(ILGenerator il, FieldBuilder fb) {
		Debug.Print(nameof(GenerateFieldCtorCode));

		var ctor = _listType.GetConstructor(Type.EmptyTypes);
		Guard.IsNotNull(ctor, nameof(ctor));

		il.Print("Creating field " + fb.DeclaringType + " " + fb.Name + "(actual: " + _listType.Name + ")");
		il.Emit(OpCodes.Ldarg_0); 
		il.Emit(OpCodes.Newobj, ctor); 
		il.Emit(OpCodes.Stfld, fb);
		_iternalProvider?.GenerateFieldCtorCode(il, fb); 
	}

	public override string ToString()
		=> "ConfigList<" + _itemType.Name + ">";
}