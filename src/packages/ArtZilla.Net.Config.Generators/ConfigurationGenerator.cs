﻿using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace ArtZilla.Net.Config.Generators;

[Generator]
public class ConfigurationsGenerator : IIncrementalGenerator {
	/// <inheritdoc />
	public void Initialize(IncrementalGeneratorInitializationContext context) {
		var filter = context.SyntaxProvider
			.CreateSyntaxProvider(
				predicate: IsSyntaxTargetForGeneration,
				transform: GetSemanticTargetForGeneration
			)
			.Where(static i => i is not null);

		var pairs = context.CompilationProvider.Combine(filter.Collect());
		context.RegisterSourceOutput(pairs, static (spc, source) => Execute(source.Left, spc, source.Right!));
	}

	static bool IsSyntaxTargetForGeneration(SyntaxNode node, CancellationToken token)
		=> node is InterfaceDeclarationSyntax { AttributeLists.Count: > 0 };

	static InterfaceDeclarationSyntax? GetSemanticTargetForGeneration(
		GeneratorSyntaxContext context,
		CancellationToken token
	) {
		var ids = (InterfaceDeclarationSyntax)context.Node;
		var sm = context.SemanticModel;
		foreach (var attributes in ids.AttributeLists)
			foreach (var attributeSyntax in attributes.Attributes) {
				if (sm.GetSymbolInfo(attributeSyntax).Symbol is not IMethodSymbol attributeSymbol)
					continue;

				var attributeContainingTypeSymbol = attributeSymbol.ContainingType;
				var fullName = attributeContainingTypeSymbol.ToDisplayString();
				if (fullName != "ArtZilla.Net.Config.GenerateConfigurationAttribute")
					continue;

				return ids;
			}

		return null;
	}

	static void Execute(
		Compilation compilation,
		SourceProductionContext context,
		ImmutableArray<InterfaceDeclarationSyntax> interfaces
	) {
		if (interfaces.IsDefaultOrEmpty)
			return;

		Cache cache = new(compilation, context);
		foreach (var ids in interfaces)
			Execute(cache, ids);
	}

	static void Execute(Cache cache, InterfaceDeclarationSyntax ids) {
		StringBuilder sb = new();
		try {
			Execute(cache, ids, sb);
		} catch (Exception e) {
			sb.Insert(0, "/*" + Environment.NewLine);
			sb.AppendLine("*/");
			sb.AppendLine();
			sb.AppendLine("/* unexpected error ");
			sb.AppendLine(e.ToString());
			sb.AppendLine("*/");
		}

		var filename = $"{ids.Identifier.Text}.g.cs";
		var text = sb.ToString();
		cache.Context.AddSource(filename, SourceText.From(text, Encoding.UTF8));
	}

	static void CollectProperties(Cache cache, List<PropertyToGenerate> list, INamedTypeSymbol nts)
		=> list.AddRange(nts.GetMembers().OfType<IPropertySymbol>().Select(ps => new PropertyToGenerate(ps, cache)));

	static void AppendNotInheritISettingsInterfaceError(StringBuilder sb, string intfName) => sb
		.AppendLine()
		.AppendLine("// ---------------------------------- ERROR ----------------------------------")
		.AppendLine("// interface {0} not inherit from ArtZilla.Net.Config.ISettings", intfName);

	[SuppressMessage("ReSharper", "ForCanBeConvertedToForeach")]
	[SuppressMessage("ReSharper", "LoopCanBeConvertedToQuery")]
	static void AppendUsings(StringBuilder sb, InterfaceDeclarationSyntax ids) {
		List<UsingDirectiveSyntax> set = new();
		var parentNode = ids.Parent;
		do {
			switch (parentNode) {
				case NamespaceDeclarationSyntax namespaceDecl: {
					var u = namespaceDecl.Usings;
					for (var i = 0; i < u.Count; i++)
						set.Add(u[i]);
					break;
				}
				case CompilationUnitSyntax compilationUnit: {
					var u = compilationUnit.Usings;
					for (var i = 0; i < u.Count; i++)
						set.Add(u[i]);
					break;
				}
			}

			parentNode = parentNode?.Parent;
		} while (parentNode != null);

		foreach (var uds in set)
			sb.AppendLine(uds.ToString());

		sb.AppendLine("using cfg = global::ArtZilla.Net.Config;");
		sb.AppendLine("using cm = global::System.ComponentModel;");
		sb.AppendLine("using cs = global::System.Runtime.CompilerServices;");
	}

	static bool TryToCollectProperties(Cache cache, INamedTypeSymbol nts, out List<PropertyToGenerate> props) {
		props = new();
		var isSettings = cache.IsInheritISettings(nts);
		CollectProperties(cache, props, nts);
		foreach (var baseIntf in nts.AllInterfaces)
			if (cache.IsInheritISettings(baseIntf)) {
				CollectProperties(cache, props, baseIntf);
				isSettings = true;
			}

		return isSettings;
	}

	static void Execute(Cache cache, InterfaceDeclarationSyntax ids, StringBuilder sb) {
		sb.AppendHeaderComment(ids);

		var sm = cache.Compilation.GetSemanticModel(ids.SyntaxTree);
		if (sm.GetDeclaredSymbol(ids) is not { } nts)
			return;

		AppendUsings(sb, ids);
		sb.AppendLine();
		sb.AppendLine("namespace {0};", nts.ContainingNamespace);
		if (!TryToCollectProperties(cache, nts, out var props)) {
			AppendNotInheritISettingsInterfaceError(sb, ids.Identifier.Text);
			return;
		}

		var itg = new InterfaceToGenerate {
			InterfaceName = ids.Identifier.Text,
			Namespace = nts.ContainingNamespace.ToString(),
			Accessibility = nts.DeclaredAccessibility,
			Properties = props.ToArray(),
		};

		itg.Calc();

		MakeBaseClass(cache, sb.AppendLine(), itg);
		MakeInpcBaseClass(cache, sb.AppendLine(), itg);
		MakeCopyClass(cache, sb.AppendLine(), itg);
		MakeReadClass(cache, sb.AppendLine(), itg);
		MakeInpcClass(cache, sb.AppendLine(), itg);
		MakeRealClass(cache, sb.AppendLine(), itg);
		MakeExtensionsClass(cache, sb.AppendLine(), itg);
		cache.Processed.Add(itg);
	}

	static void PrintPropertiesDefinition(StringBuilder sb, InterfaceToGenerate itg) {
		foreach (var prop in itg.Properties) {
			sb.AppendInheritdoc(1);
			foreach (var attr in prop.Ps.GetAttributes())
				sb.AppendLine(1, "[{0}]", attr.ApplicationSyntaxReference?.GetSyntax().ToFullString());

			if (prop.IsIConfigList) {
				sb.AppendLine(1, "[System.Runtime.Serialization.DataMemberAttribute]")
					.AppendLine(1, "[System.Xml.Serialization.XmlArray(\"{0}\")]", prop.Name)
					.AppendLine(1, "[System.Xml.Serialization.XmlArrayItem(\"{0}\")]", prop.ItemType)
					.AppendLine(1, "#if !NETSTANDARD2_0")
					.AppendLine(1, "[System.Text.Json.Serialization.JsonPropertyName(\"{0}\")]", prop.Name)
					.AppendLine(1, "#endif")
					.AppendLine(1, "public {0}[] __{1} {{", prop.ItemType, prop.Name)
					.AppendLine(2, "get => {0}.ToArray();", prop.Name)
					.AppendLine(2, "set => {0} = new(value);", prop.FieldName)
					.AppendLine(1, "}")
					.AppendLine()
					.AppendLine(1, "#if !NETSTANDARD2_0")
					.AppendLine(1, "[System.Text.Json.Serialization.JsonIgnore]")
					.AppendLine(1, "#endif")
					.AppendLine(1, "[System.Runtime.Serialization.IgnoreDataMember]")
					.AppendLine(1, "[System.Xml.Serialization.XmlIgnore]");
			} else if (prop.IsIList) {
				sb.AppendLine(1, "[System.Runtime.Serialization.DataMemberAttribute]")
					.AppendLine(1, "[System.Xml.Serialization.XmlArray(\"{0}\")]", prop.Name)
					.AppendLine(1, "[System.Xml.Serialization.XmlArrayItem(\"{0}\")]", prop.ItemType)
					.AppendLine(1, "#if !NETSTANDARD2_0")
					.AppendLine(1, "[System.Text.Json.Serialization.JsonPropertyName(\"{0}\")]", prop.Name)
					.AppendLine(1, "#endif")
					.AppendLine(1, "public {0}[] __{1} {{", prop.ItemType, prop.Name)
					.AppendLine(2, "get => {0}.ToArray();", prop.Name)
					.AppendLine(2, "set => {0} = new List<{1}>(value);", prop.FieldName, prop.ItemType)
					.AppendLine(1, "}")
					.AppendLine()
					.AppendLine(1, "#if !NETSTANDARD2_0")
					.AppendLine(1, "[System.Text.Json.Serialization.JsonIgnore]")
					.AppendLine(1, "#endif")
					.AppendLine(1, "[System.Runtime.Serialization.IgnoreDataMember]")
					.AppendLine(1, "[System.Xml.Serialization.XmlIgnore]");
			}


			sb.AppendLine(1, "public {0} {1} {{ ", prop.TypeName, prop.Name);
			sb.AppendLine(2, "get => {0};", prop.FieldName);
			if (prop.IsIConfigList)
				sb.AppendLine(2, "set => {0} = new cfg::ConfigList<{1}>(value);", prop.FieldName, prop.ItemType);
			else if (prop.IsIList)
				sb.AppendLine(2, "set => {0} = new List<{1}>(value);", prop.FieldName, prop.ItemType);
			else
				sb.AppendLine(2, "set => {0} = value;", prop.FieldName);
			sb.AppendLine(1, "}");
			sb.AppendLine();
		}
	}

	[SuppressMessage("ReSharper", "ConvertIfStatementToConditionalTernaryExpression")]
	static void PrintInpcPropertiesDefinition(StringBuilder sb, InterfaceToGenerate itg) {
		foreach (var prop in itg.Properties) {
			sb.AppendInheritdoc(1);
			foreach (var attr in prop.Ps.GetAttributes())
				sb.AppendLine(1, "[{0}]", attr.ApplicationSyntaxReference?.GetSyntax().ToFullString());

			sb.AppendLine(1, "public {0} {1} {{ ", prop.TypeName, prop.Name);
			sb.AppendLine(2, "get => {0};", prop.FieldName);
			if (prop.IsCollection)
				sb.AppendLine(2, "set => SetList({0}, value);", prop.FieldName);
			else
				sb.AppendLine(2, "set => Set(ref {0}, value);", prop.FieldName);
			sb.AppendLine(1, "}");
			sb.AppendLine();
		}
	}

	static void PrintReadonlyPropertiesDefinition(StringBuilder sb, InterfaceToGenerate itg) {
		foreach (var prop in itg.Properties) {
			sb.AppendInheritdoc(1);
			foreach (var attr in prop.Ps.GetAttributes())
				sb.AppendLine(1, "[{0}]", attr.ApplicationSyntaxReference?.GetSyntax().ToFullString());

			sb.AppendLine(1, "public {0} {1} {{ ", prop.TypeName, prop.Name);
			sb.AppendLine(2, "get => {0};", prop.FieldName);
			sb.AppendLine(2, "set => ThrowReadonlyException();");
			sb.AppendLine(1, "}");
			sb.AppendLine();
		}
	}

	static void PrintFieldsDefinition(StringBuilder sb, InterfaceToGenerate itg, bool isReadonly, bool isReadonlyLists) {
		foreach (var prop in itg.Properties) {
			sb.Indent(1);

			if (isReadonly || (isReadonlyLists && prop.IsCollection))
				sb.Append("readonly ");

			if (prop.IsIConfigList || (prop.IsIList && !isReadonly && isReadonlyLists))
				sb.Append("cfg::ConfigList<").Append(prop.ItemType).Append(">");
			else if (prop.IsIList)
				sb.Append("IList<").Append(prop.ItemType).Append(">");
			else
				sb.Append(prop.TypeName);

			sb.Append(" ").Append(prop.FieldName).AppendLine(";");
		}
	}

	static void AppendEmptyDefaultCtor(StringBuilder sb, string className)
		=> sb.AppendLine(1, "public {0}() {{ }}", className);

	static void AppendDefaultCtor(StringBuilder sb, InterfaceToGenerate itg, string className, bool isInpc) {
		sb.AppendInheritdoc(1);
		sb.AppendLine(1, "public {0}() {{", className);

		var props = itg.Properties;
		foreach (var prop in props) {
			if (prop.Attr is not { } attr)
				continue;

			switch (prop.AttrKind) {
				case PropertyToGenerate.AttributeKind.Const: {
					sb.Indent(2).Append(prop.FieldName).Append(" = ");
					var value = attr.ConstructorArguments.First().ToCSharpString();
					sb.Append(value);
					if (prop.TypeName == "float" || prop.TypeName == "float?" && char.IsDigit(value.Last()))
						sb.Append("f");
					sb.AppendLine(";");
					break;
				}

				case PropertyToGenerate.AttributeKind.Ctor: {
					sb.Indent(2).Append(prop.FieldName).Append(" = ");
					var args = attr.ConstructorArguments;
					var type = args[0];
					sb.Append("new ");
					sb.Append(type.Value);
					sb.Append("(");
					if (args.Length == 2) {
						var a = args[1];
						for (var i = 0; i < a.Values.Length; i++) {
							if (i != 0)
								sb.Append(", ");
							var value = a.Values[i];
							sb.Append(value.ToCSharpString());
						}
					}

					sb.AppendLine(");");
					break;
				}

				case PropertyToGenerate.AttributeKind.Method: {
					var args = attr.ConstructorArguments;
					var type = args[0].Value;
					var methodName = args[1].Value?.ToString() ?? "";
					var extraArgs = args[2].Values;

					if (type is not INamedTypeSymbol nts)
						throw new($"Error with attribute of property {prop.Name}");

					var members = nts.GetMembers(methodName);
					if (members.Length == 0)
						throw new($"Method {methodName} not found in type {nts.Name}");

					var isMethodFound = false;
					foreach (var member in members) {
						if (member is not IMethodSymbol ms)
							continue;

						var methodArgs = ms.Parameters;
						if (ms.ReturnsVoid) {
							if (methodArgs.Length - 1 != extraArgs.Length)
								continue;

							if (prop.IsCollection && isInpc)
								sb.AppendLine(2, "{0} = new cfg::InpcConfigList<{1}>(this, nameof({2}));", prop.FieldName, prop.ItemType, prop.Name);
							else {
								if (prop.IsIConfigList)
									sb.AppendLine(2, "{0} = new cfg::ConfigList<{1}>();", prop.FieldName, prop.ItemType);
								else if (prop.IsIList)
									sb.AppendLine(2, "{0} = new List<{1}>();", prop.FieldName, prop.ItemType);
							}

							var arg0 = methodArgs[0];
							var typeName = type.ToString();
							if (typeName.StartsWith(itg.Namespace, StringComparison.OrdinalIgnoreCase))
								typeName = typeName.Substring(itg.Namespace.Length + 1);

							sb.Indent(2);
							sb.Append(typeName);
							sb.Append(".");
							sb.Append(methodName);
							sb.Append("(");
							sb.Append(
								arg0.RefKind switch {
									RefKind.Out => "out ",
									RefKind.Ref => "ref ",
									_ => "",
								}
							);
							sb.Append(prop.FieldName);
							foreach (var value in extraArgs)
								sb.Append(", ").Append(value.ToCSharpString());
							sb.AppendLine(");");
							isMethodFound = true;
							break;
						} else {
							var typeName = type.ToString();
							if (typeName.StartsWith(itg.Namespace, StringComparison.OrdinalIgnoreCase))
								typeName = typeName.Substring(itg.Namespace.Length + 1);

							sb.Indent(2);
							sb.Append(prop.FieldName);
							sb.Append(" = ");
							sb.Append(typeName);
							sb.Append(".");
							sb.Append(methodName);
							sb.Append("(");
							for (var i = 0; i < extraArgs.Length; i++) {
								var value = extraArgs[i];
								if (i > 0)
									sb.Append(", ");
								sb.Append(value.ToCSharpString());
							}

							sb.AppendLine(");");
							isMethodFound = true;
							break;
						}
					}

					if (!isMethodFound)
						throw new($"Method {methodName} not found in type {nts.Name}");
					break;
				}

				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		sb.AppendLine(1, "} // default ctor");
	}

	static void AppendInheritedCopyCtor(StringBuilder sb, InterfaceToGenerate itg, string className)
		=> sb.AppendLine(1, "public {0}({1} source) : base(source) {{ }}", className, itg.InterfaceName);

	static void AppendCopyCtor(StringBuilder sb, InterfaceToGenerate itg, string className, bool isInpc) {
		sb.AppendInheritdoc(1);
		sb.AppendLine(1, "public {0}({1} source) {{", className, itg.InterfaceName);
		foreach (var prop in itg.Properties)
			if (prop.IsCollection)
				if (isInpc)
					sb.AppendLine(2, "{0} = new cfg::InpcConfigList<{1}>(this, nameof({2}), source.{2});", prop.FieldName, prop.ItemType, prop.Name);
				else
					sb.AppendLine(2, "{0} = new cfg::ConfigList<{1}>(source.{2});", prop.FieldName, prop.ItemType, prop.Name);
			else
				sb.AppendLine(2, "{0} = source.{1};", prop.FieldName, prop.Name);
		sb.AppendLine(1, "} // copy ctor");
	}

	static void AppendUntypedCopyMethod(StringBuilder sb, InterfaceToGenerate itg) => sb
		.AppendInheritdoc(1)
		.AppendLine(1, "public override void Copy(cfg::IConfiguration source) ")
		.AppendLine(2, "=> Copy(({0}) source);", itg.InterfaceName);

	static void AppendTypedCopyMethod(StringBuilder sb, InterfaceToGenerate itg) {
		sb.AppendInheritdoc(1);
		sb.AppendLine(1, "public void Copy({0} source) {{", itg.InterfaceName);
		foreach (var prop in itg.Properties)
			sb.AppendLine(2, "{0} = source.{0};", prop.Name);
		sb.AppendLine(1, "} // copy method");
	}

	static void AppendReadonlyTypedCopyMethod(StringBuilder sb, InterfaceToGenerate itg) => sb
		.AppendInheritdoc(1)
		.AppendLine(1, "public void Copy({0} source)", itg.InterfaceName)
		.AppendLine(2, "=> ThrowReadonlyException();");

	static void AppendThrowReadonlyExceptionMethod(StringBuilder sb) => sb
		.AppendLine("#if NETSTANDARD2_1_OR_GREATER || NET6_0_OR_GREATER")
		.AppendLine(1, "[System.Diagnostics.CodeAnalysis.DoesNotReturn]")
		.AppendLine("#endif")
		.AppendLine(1, "static void ThrowReadonlyException()")
		.AppendLine(2, "=> throw new System.Exception(\"Can't modify readonly configuration\");");

	static void MakeBaseClass(Cache _, StringBuilder sb, InterfaceToGenerate itg) => sb
		.AppendClassComment("BASE CLASS")
		.AppendClassPrefix(itg.BaseClassName, itg.AccessibilityKeyword, true)
		.Append(": cfg::SettingsBase, cfg::ISettings")
		.AppendClassBodyBegin()
		.AppendInheritdoc(1)
		.AppendLine(1, "public override Type GetInterfaceType() => typeof({0});", itg.InterfaceName)
		.AppendClassBodyEnd(itg.BaseClassName);

	static void MakeInpcBaseClass(Cache _, StringBuilder sb, InterfaceToGenerate itg) => sb
		.AppendClassComment("INPC BASE CLASS")
		.AppendClassPrefix(itg.InpcBaseClassName, itg.AccessibilityKeyword, true)
		.Append(": cfg::SettingsInpcBase, cfg::ISettings")
		.AppendClassBodyBegin()
		.AppendInheritdoc(1)
		.AppendLine(1, "public override Type GetInterfaceType() => typeof({0});", itg.InterfaceName)
		.AppendClassBodyEnd(itg.InpcBaseClassName);

	static void MakeCopyClass(Cache cache, StringBuilder sb, InterfaceToGenerate itg) {
		sb.AppendClassComment("RECORD IMPLEMENTATION");
		itg.AppendCopyClassDefinition(sb);
		PrintPropertiesDefinition(sb, itg);
		PrintFieldsDefinition(sb, itg, false, false);
		AppendDefaultCtor(sb.AppendLine(), itg, itg.CopyClassName, false);
		AppendCopyCtor(sb.AppendLine(), itg, itg.CopyClassName, false);
		AppendUntypedCopyMethod(sb.AppendLine(), itg);
		AppendTypedCopyMethod(sb.AppendLine(), itg);
		sb.AppendLine().AppendSettingsKind(1, SettingsKind.Copy);
		sb.AppendClassBodyEnd(itg.CopyClassName);
	}

	static void MakeReadClass(Cache cache, StringBuilder sb, InterfaceToGenerate itg) {
		sb.AppendClassComment("READONLY IMPLEMENTATION");
		itg.AppendReadClassDefinition(sb);
		PrintReadonlyPropertiesDefinition(sb, itg);
		PrintFieldsDefinition(sb, itg, true, true);
		AppendDefaultCtor(sb.AppendLine(), itg, itg.ReadClassName, false);
		AppendCopyCtor(sb.AppendLine(), itg, itg.ReadClassName, false);
		AppendUntypedCopyMethod(sb.AppendLine(), itg);
		AppendReadonlyTypedCopyMethod(sb.AppendLine(), itg);
		AppendThrowReadonlyExceptionMethod(sb.AppendLine());
		sb.AppendLine().AppendSettingsKind(1, SettingsKind.Read);
		sb.AppendClassBodyEnd(itg.ReadClassName);
	}

	static void MakeInpcClass(Cache cache, StringBuilder sb, InterfaceToGenerate itg) {
		sb.AppendClassComment("INPC IMPLEMENTATION");
		itg.AppendInpcClassDefinition(sb);
		PrintInpcPropertiesDefinition(sb, itg);
		PrintFieldsDefinition(sb, itg, false, true);
		AppendDefaultCtor(sb.AppendLine(), itg, itg.InpcClassName, true);
		AppendCopyCtor(sb.AppendLine(), itg, itg.InpcClassName, true);
		AppendUntypedCopyMethod(sb.AppendLine(), itg);
		AppendTypedCopyMethod(sb.AppendLine(), itg);
		sb.AppendLine().AppendSettingsKind(1, SettingsKind.Inpc);
		sb.AppendClassBodyEnd(itg.InpcClassName);
	}

	static void MakeRealClass(Cache cache, StringBuilder sb, InterfaceToGenerate itg) {
		sb.AppendClassComment("REAL IMPLEMENTATION");
		itg.AppendRealClassDefinition(sb);
		AppendEmptyDefaultCtor(sb, itg.RealClassName);
		AppendInheritedCopyCtor(sb, itg, itg.RealClassName);
		sb.AppendLine().AppendSettingsKind(1, SettingsKind.Real);
		sb.AppendClassBodyEnd(itg.RealClassName);
	}

	static void MakeExtensionsClass(Cache cache, StringBuilder sb, InterfaceToGenerate itg) {
		sb.AppendClassComment("EXTENSIONS")
			.AppendLine("{1} static partial class {0}Extensions {{", itg.ClassName, itg.AccessibilityKeyword)
			.AppendLine(1, "public static {0} Get{1}(this cfg::ISyncSettingsProvider provider, cfg::SettingsKind kind)", itg.InterfaceName, itg.ClassName)
			.AppendLine(2, "=> ({0}) provider.Get(typeof({0}), kind);", itg.InterfaceName)
			.AppendLine()
			.AppendLine(1, "public static {0} Copy{1}(this cfg::ISyncSettingsProvider provider)", itg.CopyClassName, itg.ClassName)
			.AppendLine(2, "=> ({0}) provider.Get(typeof({1}), SettingsKind.Copy);", itg.CopyClassName, itg.InterfaceName)
			.AppendLine()
			.AppendLine(1, "public static {0} Read{1}(this cfg::ISyncSettingsProvider provider)", itg.ReadClassName, itg.ClassName)
			.AppendLine(2, "=> ({0}) provider.Get(typeof({1}), SettingsKind.Read);", itg.ReadClassName, itg.InterfaceName)
			.AppendLine()
			.AppendLine(1, "public static {0} Inpc{1}(this cfg::ISyncSettingsProvider provider)", itg.InpcClassName, itg.ClassName)
			.AppendLine(2, "=> ({0}) provider.Get(typeof({1}), SettingsKind.Inpc);", itg.InpcClassName, itg.InterfaceName)
			.AppendLine()
			.AppendLine(1, "public static {0} Real{1}(this cfg::ISyncSettingsProvider provider)", itg.RealClassName, itg.ClassName)
			.AppendLine(2, "=> ({0}) provider.Get(typeof({1}), SettingsKind.Real);", itg.RealClassName, itg.InterfaceName)
			.AppendLine() // async methods
			.AppendLine(1, "public static async Task<{0}> Get{1}Async(this cfg::IAsyncSettingsProvider provider, cfg::SettingsKind kind)", itg.InterfaceName, itg.ClassName)
			.AppendLine(2, "=> ({0}) await provider.GetAsync(typeof({0}), kind);", itg.InterfaceName)
			.AppendLine()
			.AppendLine(1, "public static async Task<{0}> Copy{1}Async(this cfg::IAsyncSettingsProvider provider)", itg.CopyClassName, itg.ClassName)
			.AppendLine(2, "=> ({0}) await provider.GetAsync(typeof({1}), SettingsKind.Copy);", itg.CopyClassName, itg.InterfaceName)
			.AppendLine()
			.AppendLine(1, "public static async Task<{0}> Read{1}Async(this cfg::IAsyncSettingsProvider provider)", itg.ReadClassName, itg.ClassName)
			.AppendLine(2, "=> ({0}) await provider.GetAsync(typeof({1}), SettingsKind.Read);", itg.ReadClassName, itg.InterfaceName)
			.AppendLine()
			.AppendLine(1, "public static async Task<{0}> Inpc{1}Async(this cfg::IAsyncSettingsProvider provider)", itg.InpcClassName, itg.ClassName)
			.AppendLine(2, "=> ({0}) await provider.GetAsync(typeof({1}), SettingsKind.Inpc);", itg.InpcClassName, itg.InterfaceName)
			.AppendLine()
			.AppendLine(1, "public static async Task<{0}> Real{1}Async(this cfg::IAsyncSettingsProvider provider)", itg.RealClassName, itg.ClassName)
			.AppendLine(2, "=> ({0}) await provider.GetAsync(typeof({1}), SettingsKind.Real);", itg.RealClassName, itg.InterfaceName)
			.AppendLine("}");

	}
}