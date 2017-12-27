using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using ArtZilla.Net.Core;

namespace ArtZilla.Config.Configurators {
	public sealed class FileConfigurator: MemoryConfigurator {
		public const bool DefaultIsUseIoThread = false;

		public string Company { get; set; } = ConfigManager.CompanyName;

		public string AppName { get; set; } = ConfigManager.AppName;

		public IStreamSerializer Serializer { get; set; } = new SimpleXmlSerializer();

		public TimeSpan IoThreadCooldown {
			get => _ioThread.Cooldown;
			set => _ioThread.Cooldown = value;
		}

		public bool IsUseIOThread {
			get => _isUseIOThread;
			set {
				_isUseIOThread = value;
				_ioThread.Enabled(value);
			}
		}

		public FileConfigurator() : this(DefaultIsUseIoThread) { }

		public FileConfigurator(bool useIoThread)
			=> _ioThread = new BackgroundRepeater(RepeatedWrite, TimeSpan.FromSeconds(1), useIoThread);

		public override void Save<T>(T value) {
			base.Save(value);

			var path = GetPath<T>();
			var type = GetSimpleType<T>();
			var copy = GetCopy<T>();

			if (IsUseIOThread) {
				_cfgsToSave.AddOrUpdate(path, (type, copy), (key, old) => (type, copy));
				_toSave.Add(path);
			} else {
				SaveToFile(path, type, copy);
			}
		}

		public override void Reset<T>() {
			Remove<T>();
			base.Reset<T>();
		}

		public void Flush() {
			if (!IsUseIOThread)
				return;

			SpinWait.SpinUntil(() => _toSave.Count == 0 && _cfgsToSave.Count == 0 && !_isSaving);
		}

		private void Remove<T>() where T : IConfiguration {
			var fi = new FileInfo(GetPath<T>());
			if (fi.Exists)
				fi.Delete();
		}

		protected override T Load<T>()
			=> TryLoad<T>(out var value) ? value : base.Load<T>();

		private void RepeatedWrite(CancellationToken token) {
			while (!token.IsCancellationRequested
						 && _toSave.TryTake(out var path, Timeout.Infinite, token)
						 && _cfgsToSave.TryRemove(path, out var cfg)) {
				SaveToFile(path, cfg.Type, cfg.Value);
			}
		}

		private bool TryLoad<T>(out T value) where T : IConfiguration {
			try {
				using (var stream = new FileStream(GetPath<T>(), FileMode.Open)) {
					value = (T)Serializer.Deserialize(stream, GetSimpleType<T>());
					return true;
				}
			} catch {
				value = default(T);
				return false;
			}
		}

		private void SaveToFile(string path, Type type, IConfiguration value) {
			try {
				_isSaving = true;
				CreateDirIfNotExist(path);
				using (var stream = new FileStream(path, FileMode.Create))
					Serializer.Serialize(stream, type, value);
			} finally {
				_isSaving = false;
			}
		}

		private void CreateDirIfNotExist(string path) {
			var di = new DirectoryInfo(Path.GetDirectoryName(path));
			if (!di.Exists)
				di.Create();
		}

		private string GetPath<T>() where T : IConfiguration
			=> _paths.GetOrAdd(typeof(T), t => PathEx(GetDirectory<T>(), GetFileName<T>()));

		private string GetPath(Type type)
			=> _paths.GetOrAdd(type, t => PathEx(GetDirectory(type), GetFileName(type)));

		private string GetDirectory<T>() where T : IConfiguration
			=> GetDirectory(typeof(T));

		private string GetDirectory(Type type)
			=> PathEx(GetBaseDirectory(type), Company, AppName);

		private string GetBaseDirectory(Type type)
			=> Environment.GetFolderPath(IsUserOnlyConfiguration(type)
				? Environment.SpecialFolder.LocalApplicationData
				: Environment.SpecialFolder.CommonApplicationData);

		private string GetFileName<T>() => typeof(T).Name;

		private string GetFileName(Type type) => type.Name;

		private string PathEx(params string[] paths)
			=> Path.Combine(paths.Where(path => !string.IsNullOrWhiteSpace(path)).ToArray());

		// todo: allow all users configuration
		private bool IsUserOnlyConfiguration(Type type) => true;

		private bool _isSaving;
		private bool _isUseIOThread = DefaultIsUseIoThread;
		private readonly BackgroundRepeater _ioThread;
		private readonly ConcurrentDictionary<string, (Type Type, IConfiguration Value)> _cfgsToSave = new ConcurrentDictionary<string, (Type Type, IConfiguration Value)>();
		private readonly BlockingCollection<string> _toSave = new BlockingCollection<string>();
		private readonly ConcurrentDictionary<Type, string> _paths = new ConcurrentDictionary<Type, string>();
	}
}
