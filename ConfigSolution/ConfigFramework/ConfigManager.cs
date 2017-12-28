using System;
using System.Reflection;

namespace ArtZilla.Config {
	public static class ConfigManager {
		public static string CompanyName { get; set; }

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

		public static TConfiguration GetAuto<TConfiguration>() where TConfiguration : IConfiguration
			=> GetDefaultConfigurator().GetRealtime<TConfiguration>();

		public static TConfiguration GetAutoCopy<TConfiguration>() where TConfiguration : IConfiguration
			=> GetDefaultConfigurator().GetAutoCopy<TConfiguration>();

		public static TConfiguration GetCopy<TConfiguration>() where TConfiguration : IConfiguration
			=> GetDefaultConfigurator().GetCopy<TConfiguration>();

		public static TConfiguration GetReadOnly<TConfiguration>() where TConfiguration : IConfiguration
			=> GetDefaultConfigurator().GetReadOnly<TConfiguration>();

		private static IConfigurator _instance;
		private static ConstructorInfo _ctor;
		private static Type _configurator = typeof(MemoryConfigurator);
	}
}