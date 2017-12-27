using System;
using System.Collections.Concurrent;

namespace ArtZilla.Config {
	public class MemoryConfigurator: IConfigurator {
		public virtual T GetCopy<T>() where T : IConfiguration
			=> (T) Activator.CreateInstance(TmpCfgClass<T>.CopyType, Get<T>());

		public virtual T GetAuto<T>() where T : IConfiguration
			=> (T) Activator.CreateInstance(TmpCfgClass<T>.AutoType, Get<T>());

		public virtual T GetAutoCopy<T>() where T : IConfiguration
			=> (T) Activator.CreateInstance(TmpCfgClass<T>.AutoCopyType, Get<T>());

		public virtual T GetReadOnly<T>() where T : IConfiguration
			=> (T) Activator.CreateInstance(TmpCfgClass<T>.ReadOnlyType, Get<T>());

		public virtual void Reset<T>() where T : IConfiguration
			=> Save(CreateDefault<T>());

		public virtual void Save<T>(T value) where T : IConfiguration
			=> Get<T>().Copy(value);

		protected virtual T Load<T>() where T : IConfiguration
			=> CreateDefault<T>();

		protected virtual T Get<T>() where T : IConfiguration
			=> (T) _cache.GetOrAdd(typeof(T), Load<T>());


		protected virtual T CreateDefault<T>() where T : IConfiguration
			=> (T) Activator.CreateInstance(TmpCfgClass<T>.CopyType);

		protected Type GetSimpleType<T>() where T : IConfiguration
			=> typeof(T).IsInterface ? TmpCfgClass<T>.CopyType : typeof(T);

		private readonly ConcurrentDictionary<Type, IConfiguration> _cache
			= new ConcurrentDictionary<Type, IConfiguration>();
	}
}
