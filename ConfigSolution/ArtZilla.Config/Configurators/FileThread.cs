using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using ArtZilla.Net.Core;
using ArtZilla.Net.Core.Extensions;

namespace ArtZilla.Config.Configurators {
	public class FileThread: IoThread {
		public const string DefaultExtension = "cfg";
		public const string ExtensionSeparator = ".";
		public string AppName { get; set; }

		public string Company { get; set; }

		public string Extension { get; set; } = DefaultExtension;

		public TimeSpan IoThreadCooldown {
			get => _ioThread.Cooldown;
			set => _ioThread.Cooldown = value;
		}

		public bool AsyncWrite { get; set; } = true;

		public IStreamSerializer Serializer { get; set; } = new SimpleXmlSerializer();

		public FileThread() : this(ConfigManager.AppName, ConfigManager.CompanyName) { }
		public FileThread(string appName, string company = "") {
			AppName = appName;
			Company = company;

			_ioThread = new BackgroundRepeater(RepeatedWrite, TimeSpan.FromSeconds(1), AsyncWrite);
		}

		public override bool TryLoad<TConfiguration>(out TConfiguration configuration) {
			try {
				var path = GetPath<TConfiguration>();
				WaitPath(path);
				if (!File.Exists(path)) {
					configuration = default;
					return false;
				}

				using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read)) {
					var loaded = (TConfiguration)Serializer.Deserialize(stream, GetSimpleType<TConfiguration>());
					configuration = (TConfiguration)Activator.CreateInstance(TmpCfgClass<TConfiguration>.RealtimeType);
					configuration.Copy(loaded);
					return true;
				}
			} catch (Exception e) {
#if DEBUG
				Console.WriteLine("Exception on " + nameof(TryLoad) + ": " + e);
#endif
				configuration = default;
				return false;
			}
		}

		public override bool TryLoad<TKey, TConfiguration>(TKey key, out TConfiguration configuration) {
			try {
				var path = GetPath<TKey, TConfiguration>(key);
				WaitPath(path);
				if (!File.Exists(path)) {
					configuration = default;
					return false;
				}

				using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read)) {
					var loaded = (TConfiguration)Serializer.Deserialize(stream, GetSimpleType<TConfiguration>());
					configuration = (TConfiguration)Activator.CreateInstance(TmpCfgClass<TConfiguration>.RealtimeType);
					configuration.Copy(loaded);
					return true;
				}
			} catch (Exception e) {
#if DEBUG
				Console.WriteLine("Exception on " + nameof(TryLoad) + ": " + e);
#endif
				configuration = default;
				return false;
			}
		}

		public override void ToSave<TConfiguration>(TConfiguration configuration)
			=> _toSave.Add((GetPath<TConfiguration>(), GetSimpleType<TConfiguration>(), configuration));

		public override void ToSave<TKey, TConfiguration>(TKey key, TConfiguration configuration)
			=> _toSave.Add((GetPath<TKey, TConfiguration> (key), GetSimpleType<TConfiguration>(), configuration));

		public override void Reset<TConfiguration>() {
			// TODO: Just do it!
			for (var i = 0; i < 5; i++) {
				try {
					Remove<TConfiguration>();
					return;
				} catch {	}
			}
		}

		public override void Reset<TKey, TConfiguration>(TKey key) {
			// TODO: Just do it!
			for (var i = 0; i < 5; i++) {
				try {
					Remove<TKey, TConfiguration>(key);
					return;
				} catch { }
			}
		}

		public override void Flush() {
			if (!AsyncWrite)
				return;

			SpinWait.SpinUntil(() => _toSave.Count == 0 && !_isSaving);
		}

		private string GetPath<TConfiguration>() where TConfiguration : IConfiguration
			=> _paths.GetOrAdd(typeof(TConfiguration), t => PathEx(GetDirectory<TConfiguration>(), GetFileName<TConfiguration>()));

		private string GetPath<TKey, TConfiguration>(TKey key) where TConfiguration : IConfiguration
			=> _paths2.GetOrAdd((typeof(TConfiguration), typeof(TKey), key),
				t => PathEx(GetDirectory<TKey, TConfiguration>(), GetFileName<TKey, TConfiguration>(key)));

		private string GetPath(Type type)
			=> _paths.GetOrAdd(type, t => PathEx(GetDirectory(t), GetFileName(t)));

		private string GetDirectory<T>() where T : IConfiguration
			=> GetDirectory(typeof(T));

		private string GetDirectory<TKey, TConfiguration>() where TConfiguration : IConfiguration
			=> GetDirectory(typeof(TConfiguration), typeof(TKey));

		private string GetDirectory(Type type)
			=> PathEx(GetBaseDirectory(type), Company, AppName);

		private string GetDirectory(Type type, Type keyType)
			=> PathEx(GetBaseDirectory(type), Company, AppName, type.Name + "_by_" + keyType.Name);

		private string GetBaseDirectory(Type type)
			=> Environment.GetFolderPath(IsUserOnlyConfiguration(type)
				? Environment.SpecialFolder.LocalApplicationData
				: Environment.SpecialFolder.CommonApplicationData);

		private string GetFileName<TConfiguration>()
			=> typeof(TConfiguration).Name.Combine(ExtensionSeparator, Extension);

		private string GetFileName<TKey, TConfiguration>(TKey key)
			=> key.ToString().Combine(ExtensionSeparator, Extension);

		private string GetFileName(Type type)
			=> type.Name.Combine(ExtensionSeparator, Extension);

		private string PathEx(params string[] paths)
			=> Path.Combine(paths.Where(path => !string.IsNullOrWhiteSpace(path)).ToArray());

		// todo: allow all users configuration
		private bool IsUserOnlyConfiguration(Type type) => true;

		private Type GetSimpleType<TConfiguration>()
			where TConfiguration : IConfiguration
			=> typeof(TConfiguration).IsInterface
				? TmpCfgClass<TConfiguration>.RealtimeType
				: typeof(TConfiguration);

		private void RepeatedWrite(CancellationToken token) {
			try {
				var items = new List<(string Path, Type Type, IConfiguration Value)>();
				if (_toSave.TryTake(out var item, Timeout.Infinite, token)) {
					// collect first item
					_isSaving = true;
					items.Add(item);
				} else {
					return;
				}

				// collecting items while cooldown or not cancelled
				while (_toSave.TryTake(out item, IoThreadCooldown.Milliseconds, token))
					items.Add(item);

				// collecting last items
				while (_toSave.Count > 0)
					items.Add(_toSave.Take());

				// saving items
				foreach ((var path, var type, var value) in items.Distinct()) {
					SaveToFile(path, type, value);
				}
			} finally {
				_isSaving = false;
			}
		}

		private void WaitPath(string path) {
			if (!_isSaving)
				return;

			// todo: use path
			SpinWait.SpinUntil(() => !_isSaving);
		}

		private void SaveToFile(string path, Type type, IConfiguration value) {
			if (value is null) {
				Remove(path);
				return;
			}

			try {
				CreateDirIfNotExist(path);
				using (var stream = new FileStream(path, FileMode.Create, FileAccess.ReadWrite))
					Serializer.Serialize(stream, type, value);
			} catch (Exception e) {
#if DEBUG
				Console.WriteLine("Exception on " + nameof(SaveToFile) + ": " + e);
#endif
			}
		}

		private void CreateDirIfNotExist(string path) {
			var di = new DirectoryInfo(Path.GetDirectoryName(path));
			if (!di.Exists)
				di.Create();
		}

		private void Remove<TConfiguration>()
			where TConfiguration : IConfiguration
			=> Remove(GetPath<TConfiguration>());

		private void Remove<TKey, TConfiguration>(TKey key)
			where TConfiguration : IConfiguration
			=> Remove(GetPath<TKey, TConfiguration>(key));

		private void Remove(string path) {
			WaitPath(path);
			var fi = new FileInfo(path);
			if (fi.Exists)
				fi.Delete();
		}

		private bool _isSaving;
		private readonly BackgroundRepeater _ioThread;
		private readonly ConcurrentDictionary<Type, string> _paths
			= new ConcurrentDictionary<Type, string>();
		private readonly ConcurrentDictionary<(Type Config, Type KeyType, object KeyValue), string> _paths2
			= new ConcurrentDictionary<(Type Config, Type KeyType, object KeyValue), string>();
		private readonly BlockingCollection<(string Path, Type Type, IConfiguration Value)> _toSave
			= new BlockingCollection<(string Path, Type Type, IConfiguration Value)>();
	}
}
