using System;
using System.Reflection;

namespace ArtZilla.Config {
	public static class ConfigManager {
		public static string CompanyName { get; set; }
		public static string AppName { get; set; } = "Noname";

		static ConfigManager() {
			AppName = Assembly.GetExecutingAssembly().GetName().Name;
		}

		public static void SetDefaultConfigurator<TConfigurator>() where TConfigurator : class, IConfigurator, new() {
			_configurator = typeof(TConfigurator);
			_ctor = null;
		}

		public static void SetDefaultConfigurator(Type tconfigurator) {
			// ToDo: Assert.IsImplement(IConfigurator);
			_configurator = tconfigurator;
			_ctor = null;
		}

		public static IConfigurator GetDefaultConfigurator() 
			=> (IConfigurator)(_ctor ?? (_ctor = _configurator.GetConstructor(Type.EmptyTypes)).Invoke(null));

		private static ConstructorInfo _ctor;
		private static Type _configurator = typeof(MemoryConfigurator);
	}
}