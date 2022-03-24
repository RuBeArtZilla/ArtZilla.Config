using System.Reflection.Emit;

namespace ArtZilla.Net.Config;

public interface IDefaultValueProvider {
	void GenerateFieldCtorCode(ILGenerator il, FieldBuilder fb);
}