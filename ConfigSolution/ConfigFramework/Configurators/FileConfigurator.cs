using System;
using System.Collections.Concurrent;
using System.IO;

namespace ArtZilla.Config.Configurators {
	public class FileConfigurator: MemoryConfigurator {
		public bool IsUseIOThread { get; set; } = true;

		public override void Save<T>(T value) {
			base.Save(value);

			GetSaveTask(value);

			SaveToFile(value);
		}

		protected virtual void GetSaveTask<T>(T value) where T : IConfiguration {
			var path = GetPath<T>();


		}
		
		protected virtual void SaveToFile<T>(T value) where T : IConfiguration {
			if (IsUseIOThread) {

			} else {

			}
		}

		protected virtual string GetPath<T>() where T : IConfiguration 
			=> _paths.GetOrAdd(typeof(T), t => Path.Combine(GetDirectory<T>(), GetFileName<T>()));

		protected virtual string GetPath(Type type)
			=> _paths.GetOrAdd(type, t => Path.Combine(GetDirectory(type), GetFileName(type)));

		protected virtual string GetDirectory<T>() where T : IConfiguration {
			throw new ArgumentNullException();
		}

		protected virtual string GetDirectory(Type type) {
			throw new ArgumentNullException();
		}

		protected virtual string GetFileName<T>() => typeof(T).Name;

		protected virtual string GetFileName(Type type) => type.Name;

		private readonly ConcurrentDictionary<Type, string> _paths = new ConcurrentDictionary<Type, string>();
	}
}
