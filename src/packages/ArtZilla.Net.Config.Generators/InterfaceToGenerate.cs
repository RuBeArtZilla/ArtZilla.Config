using System.Text;
using Microsoft.CodeAnalysis;

namespace ArtZilla.Net.Config.Generators;

record struct InterfaceToGenerate {
	public string InterfaceName;
	public string ClassName;
	public string Namespace;
	public Accessibility Accessibility;
	public string AccessibilityKeyword;
	public PropertyToGenerate[] Properties;

	public string BaseClassName;
	public string InpcBaseClassName;
	public string CopyClassName;
	public string ReadClassName;
	public string InpcClassName;
	public string RealClassName;
	
	public void Calc() {
		AccessibilityKeyword = this.Accessibility.ToKeyword();

		ClassName = ConfigUtils.InterfaceNameToClassPrefix(InterfaceName);
		BaseClassName = ConfigUtils.GenerateBaseTypeNameFromClassPrefix(ClassName);
		InpcBaseClassName = ConfigUtils.GenerateInpcBaseTypeNameFromClassPrefix(ClassName);
		CopyClassName = ConfigUtils.GenerateCopyTypeNameFromClassPrefix(ClassName);
		InpcClassName = ConfigUtils.GenerateInpcTypeNameFromClassPrefix(ClassName);
		ReadClassName = ConfigUtils.GenerateReadTypeNameFromClassPrefix(ClassName);
		RealClassName = ConfigUtils.GenerateRealTypeNameFromClassPrefix(ClassName);
	}

	public void AppendCopyClassDefinition(StringBuilder sb) {
		if (AccessibilityKeyword.Length > 0)
			sb.Append(AccessibilityKeyword).Append(" ");
		sb.AppendLine("partial class {0}: {1}, {2}, cfg::ICopySettings {{", 
			CopyClassName, BaseClassName, InterfaceName);
	}
	
	public void AppendReadClassDefinition(StringBuilder sb) {
		if (AccessibilityKeyword.Length > 0)
			sb.Append(AccessibilityKeyword).Append(" ");
		sb.AppendLine("partial class {0}: {1}, {2}, cfg::IReadSettings {{", 
			ReadClassName, BaseClassName, InterfaceName);
	}
	
	public void AppendInpcClassDefinition(StringBuilder sb) {
		if (AccessibilityKeyword.Length > 0)
			sb.Append(AccessibilityKeyword).Append(" ");
		sb.AppendLine("partial class {0}: {1}, {2}, cfg::IInpcSettings {{", 
			InpcClassName, InpcBaseClassName, InterfaceName);
	}
	
	public void AppendRealClassDefinition(StringBuilder sb) {
		if (AccessibilityKeyword.Length > 0)
			sb.Append(AccessibilityKeyword).Append(" ");
		sb.AppendLine("partial class {0}: {1}, {2}, cfg::IRealSettings {{", 
			RealClassName, InpcClassName, InterfaceName);
	}
}