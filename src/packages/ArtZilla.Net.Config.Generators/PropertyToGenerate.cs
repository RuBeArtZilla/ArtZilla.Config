using Microsoft.CodeAnalysis;

namespace ArtZilla.Net.Config.Generators;

record PropertyToGenerate {
	public IPropertySymbol Ps;
	public string Name;
	public string FieldName;
	public string TypeName;
	public AttributeData? Attr;
	public AttributeKind AttrKind;

	public bool IsIList;
	public bool IsIConfigList;
	public ITypeSymbol? ItemType;

	public bool IsCollection
		=> IsIList || IsIConfigList;

	public PropertyToGenerate(IPropertySymbol ps, Cache cache) {
		Ps = ps;
		var propName = ps.Name;
		Name = propName;
		FieldName = "_" + char.ToLowerInvariant(propName.Length > 0 ? propName[0] : '_') + propName.Substring(1);
		TypeName = ps.Type.ToDisplayString();

		foreach (var attribute in ps.GetAttributes())
			if (cache.IsDefaultValue(attribute))
				SetAttr(attribute, AttributeKind.Const, propName);
			else if (cache.IsDefaultValueByCtor(attribute))
				SetAttr(attribute, AttributeKind.Ctor, propName);
			else if (cache.IsDefaultValueByMethod(attribute)) 
				SetAttr(attribute, AttributeKind.Method, propName);

		if (ps.Type is not INamedTypeSymbol pnts)
			return;

		IsIList = cache.IsIList(pnts.ConstructedFrom);
		IsIConfigList = cache.IsIConfigList(pnts.ConstructedFrom);
		if (IsIList || IsIConfigList)
			ItemType = pnts.TypeArguments[0];

		void SetAttr(AttributeData attr, AttributeKind kind, string propertyName) {
			ThrowIfMultipleAttributesExist(propertyName);
			Attr = attr;
			AttrKind = kind;
		}
	}

	void ThrowIfMultipleAttributesExist(string propertyName) {
		if (Attr is not null)
			throw new($"Property {propertyName} has multiple default value definitions");
	}

	public enum AttributeKind { Const, Ctor, Method }
}