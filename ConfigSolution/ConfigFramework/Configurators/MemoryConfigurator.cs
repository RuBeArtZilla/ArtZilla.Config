using System;
using System.Collections.Concurrent;

namespace ArtZilla.Config {
	public class MemoryConfigurator: IConfigurator {
		public virtual T GetCopy<T>() where T : IConfiguration
			=> (T)Activator.CreateInstance(TmpCfgClass<T>.CopyType, Get<T>());

		public virtual T GetAuto<T>() where T : IConfiguration
			=> Get<T>();

		public virtual T GetAutoCopy<T>() where T : IConfiguration
			=> (T)Activator.CreateInstance(TmpCfgClass<T>.AutoCopyType, Get<T>());

		public virtual T GetReadOnly<T>() where T : IConfiguration
			=> (T) Activator.CreateInstance(TmpCfgClass<T>.ReadOnlyType, Get<T>());

		public virtual void Reset<T>() where T : IConfiguration 
			=> _cache.AddOrUpdate(typeof(T), CreateDefault<T>, (t, cfg) => CreateDefault<T>(t));

		public virtual void Save<T>(T value) where T : IConfiguration 
			=> Get<T>().Copy(value);

		protected virtual T Get<T>() where T : IConfiguration 
			=> (T) _cache.GetOrAdd(typeof(T), CreateDefault<T>);

		protected virtual IConfiguration CreateDefault<T>(Type unusedArgument) where T : IConfiguration
			=> (T) Activator.CreateInstance(TmpCfgClass<T>.CopyType);

		readonly ConcurrentDictionary<Type, IConfiguration> _cache = new ConcurrentDictionary<Type, IConfiguration>();
	}
}
