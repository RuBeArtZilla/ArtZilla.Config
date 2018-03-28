using System;
using System.Reflection;
using ArtZilla.Config.Configurators;

namespace ArtZilla.Config {
	public static class ConfigManager {
		// TODO: reconfigure default configurator?
		public static string CompanyName { get; set; }

		// TODO: reconfigure default configurator?
		public static string AppName { get; set; } = "Noname";

		public static IConfigurator DefaultConfigurator {
			get => GetDefaultConfigurator();
			// set => SetDefaultConfigurator(value); // todo: implement set method.
		}

		static ConfigManager() {
			AppName = Assembly.GetExecutingAssembly().GetName().Name;
		}

		public static void SetDefaultConfigurator<TConfigurator>(TConfigurator configurator) where TConfigurator : class, IConfigurator, new() {
			_instance = configurator;
			SetDefaultConfigurator<TConfigurator>();
		}

		public static void SetDefaultConfigurator<TConfigurator>() where TConfigurator : class, IConfigurator, new() 
			=> SetDefaultConfigurator(typeof(TConfigurator));

		public static void SetDefaultConfigurator(Type tconfigurator) {
			if (tconfigurator.IsAbstract)
				throw new Exception("Default configurator type can't be abstract");

			// ToDo: Assert.IsImplement(IConfigurator);
			_configurator = tconfigurator;
			_ctor = null;
		}

		public static IConfigurator GetDefaultConfigurator()
			=> _instance ?? (_instance = (IConfigurator) GetDefaultConfiguratorCtor().Invoke(null));

		static ConstructorInfo GetDefaultConfiguratorCtor()
			=> _ctor ?? (_ctor = _configurator.GetConstructor(Type.EmptyTypes));

		public static IConfigurator<TConfiguration> As<TConfiguration>()
			where TConfiguration : IConfiguration
			=> GetDefaultConfigurator().As<TConfiguration>();

		public static IConfigurator<TKey, TConfiguration> As<TKey, TConfiguration>()
			where TConfiguration : IConfiguration
			=> GetDefaultConfigurator().As<TKey, TConfiguration>();

		public static TConfiguration Copy<TConfiguration>()
			where TConfiguration : IConfiguration
			=> GetDefaultConfigurator().Copy<TConfiguration>();

		public static TConfiguration Notifying<TConfiguration>()
			where TConfiguration : IConfiguration
			=> GetDefaultConfigurator().Notifying<TConfiguration>();

		public static TConfiguration Readonly<TConfiguration>()
			where TConfiguration : IConfiguration
			=> GetDefaultConfigurator().Readonly<TConfiguration>();

		public static TConfiguration Realtime<TConfiguration>()
			where TConfiguration : IConfiguration
			=> GetDefaultConfigurator().Realtime<TConfiguration>();

		private static IConfigurator _instance;
		private static ConstructorInfo _ctor;
		private static Type _configurator = typeof(MemoryConfigurator);
	}
}