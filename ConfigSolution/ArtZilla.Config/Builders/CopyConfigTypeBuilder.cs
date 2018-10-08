namespace ArtZilla.Config.Builders {
	public class CopyConfigTypeBuilder<T>: ConfigTypeBuilder<T> where T : IConfiguration {
		protected override string ClassPrefix => "Copy";
	}
}
