using System.Text;

// ReSharper disable MemberCanBePrivate.Global

namespace ArtZilla.Net.Config;

/// 
#if !GENERATOR
public
#endif
static class ConfigUtils {
	/// 
	/// <param name="interfaceName"></param>
	/// <returns></returns>
	public static (string Copy, string Read, string Inpc, string Real) GenerateTypeNames(string interfaceName) {
		var classPrefix = InterfaceNameToClassPrefix(interfaceName);
		return (
			GenerateCopyTypeNameFromClassPrefix(classPrefix),
			GenerateReadTypeNameFromClassPrefix(classPrefix),
			GenerateInpcTypeNameFromClassPrefix(classPrefix),
			GenerateRealTypeNameFromClassPrefix(classPrefix)
		);
	}

	/// 
	/// <param name="interfaceName"></param>
	/// <returns></returns>
	public static string GenerateCopyTypeName(string interfaceName)
		=> GenerateCopyTypeNameFromClassPrefix(InterfaceNameToClassPrefix(interfaceName));

	/// 
	/// <param name="interfaceName"></param>
	/// <returns></returns>
	public static string GenerateReadTypeName(string interfaceName)
		=> GenerateReadTypeNameFromClassPrefix(InterfaceNameToClassPrefix(interfaceName));

	/// 
	/// <param name="interfaceName"></param>
	/// <returns></returns>
	public static string GenerateInpcTypeName(string interfaceName)
		=> GenerateInpcTypeNameFromClassPrefix(InterfaceNameToClassPrefix(interfaceName));

	/// 
	/// <param name="interfaceName"></param>
	/// <returns></returns>
	public static string GenerateRealTypeName(string interfaceName)
		=> GenerateRealTypeNameFromClassPrefix(InterfaceNameToClassPrefix(interfaceName));

	/// 
	/// <param name="interfaceName"></param>
	/// <returns></returns>
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

	/// 
	/// <param name="className"></param>
	/// <returns></returns>
	public static string GenerateBaseTypeNameFromClassPrefix(string className)
		=> className + "_Base";

	/// 
	/// <param name="className"></param>
	/// <returns></returns>
	public static string GenerateInpcBaseTypeNameFromClassPrefix(string className)
		=> className + "_InpcBase";

	/// 
	/// <param name="className"></param>
	/// <returns></returns>
	public static string GenerateCopyTypeNameFromClassPrefix(string className)
		=> className + "_Copy";

	/// 
	/// <param name="className"></param>
	/// <returns></returns>
	public static string GenerateReadTypeNameFromClassPrefix(string className)
		=> className + "_Read";

	/// 
	/// <param name="className"></param>
	/// <returns></returns>
	public static string GenerateInpcTypeNameFromClassPrefix(string className)
		=> className + "_Inpc";

	/// 
	/// <param name="className"></param>
	/// <returns></returns>
	public static string GenerateRealTypeNameFromClassPrefix(string className)
		=> className + "_Real";

	#if NETSTANDARD2_0
	public static void Deconstruct<TKey, TValue>(this KeyValuePair<TKey, TValue> pair, out TKey key, out TValue value)
		=> (key, value) = (pair.Key, pair.Value);
	#endif
}