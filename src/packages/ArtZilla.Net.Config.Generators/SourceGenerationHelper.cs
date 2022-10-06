using System.Text;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ArtZilla.Net.Config.Generators;

static class SourceGenerationHelper {
	public static INamedTypeSymbol? GetDefaultValueAttribute(this Compilation compilation)
		=> compilation.GetTypeByMetadataName("System.ComponentModel.DefaultValueAttribute");

	public static INamedTypeSymbol? GetDefaultValueByMethodAttribute(this Compilation compilation)
		=> compilation.GetTypeByMetadataName("ArtZilla.Net.Config.DefaultValueByMethodAttribute");

	public static INamedTypeSymbol? GetDefaultValueByCtorAttribute(this Compilation compilation)
		=> compilation.GetTypeByMetadataName("ArtZilla.Net.Config.DefaultValueByCtorAttribute");

	public static INamedTypeSymbol? GetISettingsInterface(this Compilation compilation)
		=> compilation.GetTypeByMetadataName("ArtZilla.Net.Config.ISettings");
	
	public static INamedTypeSymbol? GetIConfigListInterface(this Compilation compilation)
		=> compilation.GetTypeByMetadataName("ArtZilla.Net.Config.IConfigList`1");
	
	public static INamedTypeSymbol? GetIListInterface(this Compilation compilation)
		=> compilation.GetTypeByMetadataName("System.Collections.Generic.IList`1");

	public static bool IsAttribute(this AttributeData attributeData, INamedTypeSymbol attributeSymbol)
		=> attributeSymbol.Equals(attributeData.AttributeClass, SymbolEqualityComparer.Default);

	public static string ToKeyword(this Accessibility value) => value switch {
		Accessibility.NotApplicable => "",
		Accessibility.Private => "",
		Accessibility.ProtectedAndInternal => "protected internal",
		Accessibility.Protected => "protected",
		Accessibility.Internal => "",
		Accessibility.ProtectedOrInternal => "",
		Accessibility.Public => "public",
		_ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
	};
	
	public static StringBuilder Indent(this StringBuilder sb, int count) 
		=> sb.Append('\t', count);

	[StringFormatMethod("format")]
	public static StringBuilder AppendLine(this StringBuilder sb, string format, object? arg0) 
		=> sb.AppendFormat(format, arg0).AppendLine();
	
	[StringFormatMethod("format")]
	public static StringBuilder AppendLine(this StringBuilder sb, string format, object? arg0, object? arg1) 
		=> sb.AppendFormat(format, arg0, arg1).AppendLine();
	
	[StringFormatMethod("format")]
	public static StringBuilder AppendLine(this StringBuilder sb, string format, object? arg0, object? arg1, object? arg2) 
		=> sb.AppendFormat(format, arg0, arg1, arg2).AppendLine();
	
	[StringFormatMethod("format")]
	public static StringBuilder AppendLine(this StringBuilder sb, string format, params object?[] args) 
		=> sb.AppendFormat(format, args).AppendLine();
	
	public static StringBuilder AppendLine(this StringBuilder sb, int indent, string value) 
		=> sb.Indent(indent).AppendLine(value);

	[StringFormatMethod("format")]
	public static StringBuilder AppendLine(this StringBuilder sb, int indent, string format, object? arg0) 
		=> sb.Indent(indent).AppendFormat(format, arg0).AppendLine();
	
	[StringFormatMethod("format")]
	public static StringBuilder AppendLine(this StringBuilder sb, int indent, string format, object? arg0, object? arg1) 
		=> sb.Indent(indent).AppendFormat(format, arg0, arg1).AppendLine();
	
	[StringFormatMethod("format")]
	public static StringBuilder AppendLine(this StringBuilder sb, int indent, string format, object? arg0, object? arg1, object? arg2) 
		=> sb.Indent(indent).AppendFormat(format, arg0, arg1, arg2).AppendLine();
	
	[StringFormatMethod("format")]
	public static StringBuilder AppendLine(this StringBuilder sb, int indent, string format, params object?[] args) 
		=> sb.Indent(indent).AppendFormat(format, args).AppendLine();

	public static StringBuilder AppendInheritdoc(this StringBuilder sb) 
		=> sb.AppendLine("/// <inheritdoc />");
	
	public static StringBuilder AppendInheritdoc(this StringBuilder sb, int indent) 
		=> sb.AppendLine(indent, "/// <inheritdoc />");

	public static StringBuilder AppendHeaderComment(this StringBuilder sb, InterfaceDeclarationSyntax ids) => sb
		.AppendLine("// generated {0}", DateTime.Now)
		.AppendLine("// for interface {0}", ids.Identifier.Text)
		.AppendLine();

	public static StringBuilder AppendClassComment(this StringBuilder sb, string comment) => sb
		.AppendLine("// ---------------- {0} ----------------", comment)
		.AppendLine();

	public static StringBuilder AppendClassPrefix(
		this StringBuilder sb,
		string className,
		string accessibility,
		bool isAbstract
	) {
		sb.Append(accessibility);
		if (accessibility.Length != 0)
			sb.Append(" ");

		if (isAbstract)
			sb.Append("abstract ");

		sb.Append("partial class ");
		sb.Append(className);
		return sb;
	}

	public static StringBuilder AppendClassBodyBegin(this StringBuilder sb) 
		=> sb.AppendLine(" {");

	public static StringBuilder AppendSettingsKind(this StringBuilder sb, int indent, SettingsKind kind)
		=> sb.AppendLine(indent, "public override cfg::SettingsKind GetSettingsKind() => cfg::SettingsKind.{0};", kind);

	public static StringBuilder AppendClassBodyEnd(this StringBuilder sb, string className) 
		=> sb.AppendLine("}} // class {0}", className);

	public static StringBuilder AppendInpcImplementation(this StringBuilder sb) => sb
		.AppendLine("#region INotifyPropertyChanged implementation")
		.AppendLine()
		.AppendLine(1, "public event cm::PropertyChangedEventHandler? PropertyChanged;")
		.AppendLine()
		.AppendLine(1, "protected virtual void OnPropertyChanged([cs::CallerMemberName] string? propertyName = null)")
		.AppendLine(2, "=> PropertyChanged?.Invoke(this, new cm::PropertyChangedEventArgs(propertyName));")
		.AppendLine()
		.AppendLine(1, "protected bool Set<T>(ref T field, T value, [cs::CallerMemberName] string? propertyName = null) {")
		.AppendLine(2, "if (EqualityComparer<T>.Default.Equals(field, value))")
		.AppendLine(3, "return false;")
		.AppendLine(2, "field = value;")
		.AppendLine(2, "OnPropertyChanged(propertyName);")
		.AppendLine(2, "return true;")
		.AppendLine(1, "}")
		.AppendLine()
		.AppendLine(1, "protected virtual void SetList<T>(IList<T> list, IList<T> values, [cs::CallerMemberName] string? propertyName = null) {")
		.AppendLine(2, "try {")
		.AppendLine(3, "int i, count = values.Count;")
		.AppendLine(3, "for (i = 0; i < list.Count || i < count;) {")
		.AppendLine(4, "var item = list[i];")
		.AppendLine(4, "var value = values[i];")
		.AppendLine(4, "if (EqualityComparer<T>.Default.Equals(item, value))")
		.AppendLine(5, "++i;")
		.AppendLine(4, "else")
		.AppendLine(5, "list.RemoveAt(i);")
		.AppendLine(3, "}")
		.AppendLine()
		.AppendLine(3, "while (list.Count > count)")
		.AppendLine(4, "list.RemoveAt(count);")
		.AppendLine()
		.AppendLine(3, "for (; i < count; ++i) ")
		.AppendLine(4, "list.Add(values[i]);")
		.AppendLine(2, "} finally {")
		.AppendLine(3, "OnPropertyChanged(propertyName);")
		.AppendLine(2, "}")
		.AppendLine(1, "}")
		.AppendLine()
		.AppendLine("#endregion // INotifyPropertyChanged");
}