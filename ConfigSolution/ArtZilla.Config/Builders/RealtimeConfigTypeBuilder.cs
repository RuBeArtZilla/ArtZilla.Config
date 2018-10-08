namespace ArtZilla.Config.Builders {
	public class RealtimeConfigTypeBuilder<T>: NotifyingConfigTypeBuilder<T> where T : IConfiguration {
		protected override string ClassPrefix => "Realtime";

		protected override void AddInterfaces() {
			AddRealtimeImplementation();
			base.AddInterfaces();
		}

		protected virtual void AddRealtimeImplementation() => Tb.AddInterfaceImplementation(typeof(IRealtimeConfiguration));
	}
}
