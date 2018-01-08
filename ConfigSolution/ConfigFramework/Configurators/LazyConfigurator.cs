﻿using System;
using System.ComponentModel;

namespace ArtZilla.Config.Configurators {
	public class LazyConfigurator: MemoryConfigurator {
		public LazyConfigurator(IIoThread thread) => Io = thread ?? throw new ArgumentNullException(nameof(thread));

		public void Flush() => Io.Flush();

		protected override IConfigurator<TConfiguration> CreateTypedConfigurator<TConfiguration>()
			=> new LazyConfigurator<TConfiguration>(Io);

		protected readonly IIoThread Io;
	}


	public class LazyConfigurator<TConfiguration>: MemoryConfigurator<TConfiguration> where TConfiguration : IConfiguration {
		public LazyConfigurator(IIoThread thread) => Io = thread ?? throw new ArgumentNullException(nameof(thread));

		public void Flush() => Io.Flush();

		public override void Save(TConfiguration value) {
			base.Save(value);

			if (value is IRealtimeConfiguration)
				Io.ToSave(value);
			else
				Io.ToSave(Get());
		}

		public override void Reset() {
			base.Reset();
			Io.Reset<TConfiguration>();
		}

		public override bool IsExist() {
			if (base.IsExist())
				return true;
			if (Io.TryLoad<TConfiguration>(out var configuration)) {
				Value = configuration;
				Subscribe(Value);
				return true;
			}

			return false;
		}

		protected override TConfiguration Load() {
			if (Io.TryLoad<TConfiguration>(out var loaded)) {
				Subscribe(loaded);
				return loaded;
			}

			return base.Load();
		}

		protected override IConfigurator<TKey, TConfiguration> CreateKeysConfigurator<TKey>()
			=> new LazyConfigurator<TKey, TConfiguration>(Io);

		protected override void ConfigurationChanged(object sender, PropertyChangedEventArgs e) {
			base.ConfigurationChanged(sender, e);
			Io.ToSave(Get());
		}

		protected readonly IIoThread Io;
	}

	public class LazyConfigurator<TKey, TConfiguration>: MemoryConfigurator<TKey, TConfiguration> where TConfiguration : IConfiguration {
		public LazyConfigurator(IIoThread thread) => Io = thread ?? throw new ArgumentNullException(nameof(thread));

		public void Flush() => Io.Flush();

		protected readonly IIoThread Io;
	}


	/*
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
			=> _ioThread = new BackgroundRepeater(RepeatedWrite, TimeSpan.FromSeconds(1), (_isUseIOThread = useIoThread));

		/// <summary>
		/// Save <paramref name="value" /> as <typeparamref name="TConfiguration" />
		/// </summary>
		/// <typeparam name="TConfiguration"></typeparam>
		/// <param name="value"></param>
		public override void Save<TConfiguration>(TConfiguration value) {
			base.Save(value);

			var path = GetPath<TConfiguration>();
			var type = GetSimpleType<TConfiguration>();
			var copy = Copy<TConfiguration>();

			if (IsUseIOThread) {
				_cfgsToSave.AddOrUpdate(path, (type, copy), (key, old) => (type, copy));
				_toSave.Add(path);
			} else {
				SaveToFile(path, type, copy);
			}
		}

		/// <summary>
		/// Reset <typeparamref name="TConfiguration" /> to default values
		/// </summary>
		/// <typeparam name="TConfiguration"></typeparam>
		public override void Reset<TConfiguration>() {
			Remove<TConfiguration>();
			base.Reset<TConfiguration>();
		}

		public void Flush() {
			if (!IsUseIOThread)
				return;

			SpinWait.SpinUntil(() => _toSave.Count == 0 && _cfgsToSave.Count == 0 && !_isSaving);
		}

		protected override T Load<T>() {
			if (TryLoad<T>(out var loaded))
				return loaded;
			return base.Load<T>();
		}

		protected override void ConfigurationChanged<TConfiguration>(object sender, PropertyChangedEventArgs e) {
			var path = GetPath<TConfiguration>();
			var type = GetSimpleType<TConfiguration>();
			var cfg = Copy<TConfiguration>();

			if (IsUseIOThread) {
				_cfgsToSave.AddOrUpdate(path, (type, cfg), (key, old) => (type, cfg));
				_toSave.Add(path);
			} else {
				SaveToFile(path, type, cfg);
			}
		}

		protected override IConfigurator<TConfiguration> CreateTypedConfigurator<TConfiguration>()
			=> new FileConfigurator<TConfiguration>();

		private void RepeatedWrite(CancellationToken token) {
			while (!token.IsCancellationRequested
						 && _toSave.TryTake(out var path, Timeout.Infinite, token)
						 && _cfgsToSave.TryRemove(path, out var cfg)) {
				SaveToFile(path, cfg.Type, cfg.Value);
			}
		}

		private void Remove<T>() where T : IConfiguration {
			var fi = new FileInfo(GetPath<T>());
			if (fi.Exists)
				fi.Delete();
		}

		private bool TryLoad<T>(out T value) where T : IConfiguration {
			try {
				using (var stream = new FileStream(GetPath<T>(), FileMode.Open)) {
					var loaded = (T)Serializer.Deserialize(stream, GetSimpleType<T>());
					value = (T)Activator.CreateInstance(TmpCfgClass<T>.RealtimeType);
					value.Copy(loaded);
					Subscribe(value);
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
		private bool _isUseIOThread;
		private readonly BackgroundRepeater _ioThread;
		private readonly ConcurrentDictionary<string, (Type Type, IConfiguration Value)> _cfgsToSave = new ConcurrentDictionary<string, (Type Type, IConfiguration Value)>();
		private readonly BlockingCollection<string> _toSave = new BlockingCollection<string>();
		private readonly ConcurrentDictionary<Type, string> _paths = new ConcurrentDictionary<Type, string>();
	}
	*/
}
