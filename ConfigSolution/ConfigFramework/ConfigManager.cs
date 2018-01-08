using System;
using System.Reflection;
using ArtZilla.Config.Configurators;

namespace ArtZilla.Config {
	public static class ConfigManager {
		// TODO: reconfigure default configurator?
		public static string CompanyName { get; set; }

		// TODO: reconfigure default configurator?
		public static string AppName { get; set; } = "Noname";

		static ConfigManager() {
			AppName = Assembly.GetExecutingAssembly().GetName().Name;
		}

		public static void SetDefaultConfigurator<TConfigurator>(TConfigurator configurator) where TConfigurator : class, IConfigurator, new() {
			_instance = configurator;
			SetDefaultConfigurator<TConfigurator>();
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
			=> _instance ?? (_instance = (IConfigurator)GetDefaultConfiguratorCtor().Invoke(null));

		static ConstructorInfo GetDefaultConfiguratorCtor()
			=> _ctor ?? (_ctor = _configurator.GetConstructor(Type.EmptyTypes));


		public static TConfiguration GetCopy<TConfiguration>() where TConfiguration : IConfiguration
			=> GetDefaultConfigurator().Copy<TConfiguration>();

		public static TConfiguration GetNotifying<TConfiguration>() where TConfiguration : IConfiguration
			=> GetDefaultConfigurator().Notifying<TConfiguration>();

		public static TConfiguration GetReadonly<TConfiguration>() where TConfiguration : IConfiguration
			=> GetDefaultConfigurator().Readonly<TConfiguration>();

		public static TConfiguration GetRealtime<TConfiguration>() where TConfiguration : IConfiguration
			=> GetDefaultConfigurator().Realtime<TConfiguration>();

		private static IConfigurator _instance;
		private static ConstructorInfo _ctor;
		private static Type _configurator = typeof(MemoryConfigurator);
	}
}