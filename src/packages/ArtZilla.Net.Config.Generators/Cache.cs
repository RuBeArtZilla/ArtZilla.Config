using Microsoft.CodeAnalysis;

namespace ArtZilla.Net.Config.Generators;

record Cache(Compilation Compilation, SourceProductionContext Context) {
	public readonly Compilation Compilation 
		= Compilation;

	public readonly SourceProductionContext Context
		= Context;

	public readonly List<InterfaceToGenerate> Processed = new();

	public INamedTypeSymbol IListInterface => _iListInterface;
	
	readonly INamedTypeSymbol _defaultValueAttribute
		= Compilation.GetDefaultValueAttribute() ?? throw new("DefaultValueAttribure not found");
	
	readonly INamedTypeSymbol _defaultValueByCtorAttribute
		= Compilation.GetDefaultValueByCtorAttribute() ?? throw new("DefaultValueByCtorAttribute not found");
	
	readonly INamedTypeSymbol _defaultValueByMethodAttribute
		= Compilation.GetDefaultValueByMethodAttribute() ?? throw new("DefaultValueByMethodAttribute not found");

	readonly INamedTypeSymbol _iSettingsInterface 
		= Compilation.GetISettingsInterface() ?? throw new("interface ISettings not found");
	                                                  
	readonly INamedTypeSymbol _iConfigListInterface 
		= Compilation.GetIConfigListInterface() ?? throw new("interface IConfigList<T> not found");
	
	readonly INamedTypeSymbol _iListInterface 
		= Compilation.GetIListInterface() ?? throw new("interface IList<T> not found");
	                               
	public bool IsDefaultValue(AttributeData attribute) 
		=> attribute.IsAttribute(_defaultValueAttribute);
	
	public bool IsDefaultValueByCtor(AttributeData attribute)
		=> attribute.IsAttribute(_defaultValueByCtorAttribute);
	
	public bool IsDefaultValueByMethod(AttributeData attribute)
		=> attribute.IsAttribute(_defaultValueByMethodAttribute);

	public bool IsInheritISettings(INamedTypeSymbol intf) 
		=> intf.AllInterfaces.Contains(_iSettingsInterface);

	public bool IsIList(INamedTypeSymbol nts)
		=> SymbolEqualityComparer.IncludeNullability.Equals(nts, _iListInterface);

	public bool IsIConfigList(INamedTypeSymbol nts)
		=> SymbolEqualityComparer.IncludeNullability.Equals(nts, _iConfigListInterface);
	
	public bool IsInheritIConfigList(INamedTypeSymbol nts) {
		if (SymbolEqualityComparer.IncludeNullability.Equals(nts.ConstructedFrom, _iConfigListInterface))
			return true;

		foreach (var i in nts.AllInterfaces)
			if (SymbolEqualityComparer.IncludeNullability.Equals(i.ConstructedFrom, _iConfigListInterface))
				return true;

		return false;
	}
}