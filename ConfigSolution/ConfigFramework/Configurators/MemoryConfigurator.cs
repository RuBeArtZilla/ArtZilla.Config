using System;
using System.Collections.Concurrent;

namespace ArtZilla.Config {
	public class MemoryConfigurator: IConfigurator {
		public virtual TConfiguration GetCopy<TConfiguration>() where TConfiguration : IConfiguration
			=> (TConfiguration) Activator.CreateInstance(TmpCfgClass<TConfiguration>.CopyType, Get<TConfiguration>());

		public virtual TConfiguration GetAuto<TConfiguration>() where TConfiguration : IConfiguration
			=> (TConfiguration)Activator.CreateInstance(TmpCfgClass<TConfiguration>.AutoType, Get<TConfiguration>());

		public virtual TConfiguration GetRealtime<TConfiguration>() where TConfiguration : IConfiguration
			=> (TConfiguration) Activator.CreateInstance(TmpCfgClass<TConfiguration>.AutoType, Get<TConfiguration>());

		public virtual TConfiguration GetAutoCopy<TConfiguration>() where TConfiguration : IConfiguration
			=> (TConfiguration) Activator.CreateInstance(TmpCfgClass<TConfiguration>.AutoCopyType, Get<TConfiguration>());

		public virtual TConfiguration GetReadOnly<TConfiguration>() where TConfiguration : IConfiguration
			=> (TConfiguration) Activator.CreateInstance(TmpCfgClass<TConfiguration>.ReadOnlyType, Get<TConfiguration>());

		public virtual void Reset<TConfiguration>() where TConfiguration : IConfiguration
			=> Save(CreateDefault<TConfiguration>());

		public virtual void Save<TConfiguration>(TConfiguration value) where TConfiguration : IConfiguration
			=> Get<TConfiguration>().Copy(value);

		protected virtual TConfiguration Load<TConfiguration>() where TConfiguration : IConfiguration
			=> CreateDefault<TConfiguration>();

		protected virtual TConfiguration Get<TConfiguration>() where TConfiguration : IConfiguration
			=> (TConfiguration) _cache.GetOrAdd(typeof(TConfiguration), Load<TConfiguration>());


		protected virtual TConfiguration CreateDefault<TConfiguration>() where TConfiguration : IConfiguration
			=> (TConfiguration) Activator.CreateInstance(TmpCfgClass<TConfiguration>.CopyType);

		protected Type GetSimpleType<TConfiguration>() where TConfiguration : IConfiguration
			=> typeof(TConfiguration).IsInterface ? TmpCfgClass<TConfiguration>.CopyType : typeof(TConfiguration);

		private readonly ConcurrentDictionary<Type, IConfiguration> _cache
			= new ConcurrentDictionary<Type, IConfiguration>();
	}
}
