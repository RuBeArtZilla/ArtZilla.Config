using System.Text;

namespace ArtZilla.Net.Config;

public enum SettingsKind { Copy, Read, Inpc, Real }

public static class ConfigUtils {
	public static (string Copy, string Read, string Inpc, string Real) GenerateTypeNames(string interfaceName) {
		var classPrefix = InterfaceNameToClassPrefix(interfaceName);
		return (
			GenerateCopyTypeNameFromClassPrefix(classPrefix),
			GenerateReadTypeNameFromClassPrefix(classPrefix),
			GenerateInpcTypeNameFromClassPrefix(classPrefix),
			GenerateRealTypeNameFromClassPrefix(classPrefix)
		);
	}

	public static string GenerateCopyTypeName(string interfaceName) 
		=> GenerateCopyTypeNameFromClassPrefix(InterfaceNameToClassPrefix(interfaceName));

	public static string GenerateReadTypeName(string interfaceName) 
		=> GenerateReadTypeNameFromClassPrefix(InterfaceNameToClassPrefix(interfaceName));
	
	public static string GenerateInpcTypeName(string interfaceName) 
		=> GenerateInpcTypeNameFromClassPrefix(InterfaceNameToClassPrefix(interfaceName));
	
	public static string GenerateRealTypeName(string interfaceName)
		=> GenerateRealTypeNameFromClassPrefix(InterfaceNameToClassPrefix(interfaceName));

	public static string InterfaceNameToClassPrefix(string interfaceName) {
		var startPos = interfaceName.LastIndexOf('.') + 1;
		if (startPos < 0 || startPos >= interfaceName.Length)
			startPos = 0;
			
		var intfLength = interfaceName.Length - startPos;
		if (intfLength < 2)
			return interfaceName;

		var firstChar = interfaceName[startPos];
		if (firstChar is not 'I')
			return interfaceName;

		if (!char.IsUpper(interfaceName, startPos + 1))
			return interfaceName;
		
		if (startPos == 0)
			return interfaceName.Substring(1);
		
		var sb = new StringBuilder(interfaceName);
		sb.Remove(startPos, 1);
		return sb.ToString();
	}
	
	public static string GenerateBaseTypeNameFromClassPrefix(string className) 
		=> className + "_Base";
	
	public static string GenerateInpcBaseTypeNameFromClassPrefix(string className) 
		=> className + "_InpcBase";

	public static string GenerateCopyTypeNameFromClassPrefix(string className) 
		=> className + "_Copy";

	public static string GenerateReadTypeNameFromClassPrefix(string className) 
		=> className + "_Read";
	
	public static string GenerateInpcTypeNameFromClassPrefix(string className) 
		=> className + "_Inpc";
	
	public static string GenerateRealTypeNameFromClassPrefix(string className) 
		=> className + "_Real";
}
