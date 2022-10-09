using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using ArtZilla.Net.Core.Extensions;
using Guard = CommunityToolkit.Diagnostics.Guard;

namespace ArtZilla.Net.Config;

public abstract class FileSettingsProvider : ISettingsProvider, IDisposable {
	/// <inheritdoc cref="ISettingsTypeConstructor"/> 
	public ISettingsTypeConstructor Constructor { get; }

	public FileSerializer Serializer { get; }

	public string Location { get; }

	protected record Pack {
		public string Path;
		public IRealSettings Settings;

		public DateTime Changed;
		public long Length;
	}

	bool _isSaving;
	FileSystemWatcher? _watcher;
	HashSet<IRealSettings> _toSaveQueue = new();
	HashSet<FileSystemEventArgs> _fsEvents = new();
	readonly bool _isTrackChanges;
	readonly CancellationTokenSource _stopAllTasks = new();
	readonly ConcurrentDictionary<(Type Type, string? Key), Pack> _map = new();

	/// 
	/// <param name="serializer"></param>
	/// <param name="constructor"></param>
	/// <param name="location"></param>
	/// <param name="isTrackChanges"></param>
	protected FileSettingsProvider(
		FileSerializer serializer,
		ISettingsTypeConstructor constructor,
		string? location = null,
		bool isTrackChanges = true
	) {
		Guard.IsNotNull(serializer);
		Guard.IsNotNull(constructor);
		Debug.Print("Created {0} with path {1}", GetType().Name, location);
		(Serializer, Location, Constructor, _isTrackChanges) =
			(serializer, location ?? GetDefaultLocation(), constructor, isTrackChanges);

		Task.Factory.StartNew(UpdateTask, _stopAllTasks.Token, _stopAllTasks.Token);
	}

	public static string GetLocation(string app, string? company = null) {
		Guard.IsNotNull(app);
		var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
		var location = company.IsNotNullOrWhiteSpace()
			? Path.Combine(localAppData, company!, app, "settings")
			: Path.Combine(localAppData, app, "settings");
		return location;
	}

	/// <inheritdoc />
	public void Dispose() {
		_watcher?.Dispose();
		_stopAllTasks.Dispose();
	}

	static string GetDefaultLocation() {
		var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetCallingAssembly();
		var location = Path.GetDirectoryName(assembly.Location);
		if (Directory.Exists(location))
			return Path.Combine(location, "settings");

		var appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
		return Path.Combine(
			appdata,
			assembly.GetName().Name ?? assembly.FullName ?? AppDomain.CurrentDomain.FriendlyName,
			"settings"
		);
	}

	/// <inheritdoc />
	public bool IsExist(Type type, string? key = null)
		=> TryGetPack(type, key, out var pack) && File.Exists(pack!.Path);

	/// <inheritdoc />
	public bool Delete(Type type, string? key = null) {
		const int maxTryCount = 10;
		const int pauseMs = 1000;

		Pack? pack = default;
		for (var i = 0; i < maxTryCount; ++i) {
			if (_map.TryRemove((type, key), out pack))
				break;

			Thread.Sleep(pauseMs);
		}

		if (pack is null)
			return false;

		var path = pack.Path;
		var settings = pack.Settings;
		settings.Unsubscribe(OnPropertyChanged);
		settings.Source = default!;

		for (var i = 0; i < maxTryCount; ++i) {
			try {
				if (!File.Exists(path))
					File.Delete(path);
				Debug.Print("Deleted {0} from {1}", type, path);
				return true;
			} catch (Exception e) {
				Debug.Print("Error in {0}: {1}", nameof(Delete), e);
			}

			Thread.Sleep(pauseMs);
		}

		return false;
	}

	/// <inheritdoc />
	public void Reset(Type type, string? key = null) {
		var pack = GetPack(type, key);
		var real = pack.Settings;
		var read = Constructor.CreateRead(real.GetInterfaceType());
		real.Copy(read);
	}

	/// <inheritdoc />
	public void Flush(Type? type = null, string? key = null) {
		if (type != null) {
			var pack = GetPack(type, key);
			var real = pack.Settings;
			SpinWait.SpinUntil(() => !_isSaving && !_toSaveQueue.Contains(real));
		} else
			SpinWait.SpinUntil(() => !_isSaving && _toSaveQueue.Count == 0);
	}

	/// <inheritdoc />
	public ISettings Get(Type type, SettingsKind kind, string? key = null) {
		var pack = GetPack(type, key);
		var real = pack.Settings;
		if (kind == SettingsKind.Real)
			return real;

		var settings = Constructor.Create(type, kind, real);
		settings.Source = this;
		return settings;
	}

	/// <inheritdoc />
	public void Set(ISettings settings, string? key = null) {
		var type = settings.GetInterfaceType();
		var pack = GetPack(type, key);
		var real = pack.Settings;
		real.Copy(settings);
	}

	/// <inheritdoc />
	public async Task<bool> IsExistAsync(Type type, string? key = null)
		=> await TryGetPackAsync(type, key, out var pack) && File.Exists(pack!.Path);

	/// <inheritdoc />
	public Task<bool> DeleteAsync(Type type, string? key = null)
		=> Task.FromResult(Delete(type));

	/// <inheritdoc />
	public async Task ResetAsync(Type type, string? key = null) {
		var pack = await GetPackAsync(type, key);
		var real = pack.Settings;
		var read = Constructor.CreateRead(real.GetInterfaceType());
		real.Copy(read);
	}

	/// <inheritdoc />
	public async Task FlushAsync(Type? type = null, string? key = null) {
		if (type != null) {
			var pack = await GetPackAsync(type, key);
			var real = pack.Settings;
			SpinWait.SpinUntil(() => !_isSaving && !_toSaveQueue.Contains(real));
		} else
			SpinWait.SpinUntil(() => !_isSaving && _toSaveQueue.Count == 0);
	}

	/// <inheritdoc />
	public async Task<ISettings> GetAsync(Type type, SettingsKind kind, string? key = null) {
		var pack = await GetPackAsync(type, key);
		var real = pack.Settings;
		if (kind == SettingsKind.Real)
			return real;

		var settings = Constructor.Create(type, kind, real);
		settings.Source = this;
		return settings;
	}

	/// <inheritdoc />
	public async Task SetAsync(ISettings settings, string? key = null) {
		var type = settings.GetInterfaceType();
		var pack = await GetPackAsync(type, key);
		var real = pack.Settings;
		real.Copy(settings);
	}

	/// <inheritdoc />
	public void ThrowIfNotSupported(Type type)
		=> AssertType(type).Wait();

	///
	public string GetPathToSettings(Type type, string? key = null) 
		=> key is null
			? Path.Combine(Location, type.Name + Serializer.GetFileExtension()) 
			: Path.Combine(Location, type.Name, EnframeFilename(key) + Serializer.GetFileExtension());

	string EnframeFilename(string filename) {
		return filename; // todo: ...
	}

#if NETSTANDARD20
	protected virtual bool TryGetPack(Type type, string? key, out Pack? pack)
#else
	protected virtual bool TryGetPack(Type type, string? key, [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out Pack? pack)
#endif
		=> _map.TryGetValue((type, key), out pack);

#if NETSTANDARD20
	protected virtual Task<bool> TryGetPackAsync(Type type, string? key, out Pack? pack)
#else
	protected virtual Task<bool> TryGetPackAsync(Type type, string? key, [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out Pack? pack)
#endif
		=> Task.FromResult(TryGetPack(type, key, out pack));

	protected virtual Pack GetPack(Type type, string? key)
		=> _map.GetOrAdd((type, key), i => CreatePack(i.Type, i.Key).Result);

	protected virtual Task<Pack> GetPackAsync(Type type, string? key)
		=> Task.FromResult(GetPack(type, key));

	/// 
	/// <param name="keyType"></param>
	/// <param name="settingsType"></param>
	/// <returns></returns>
	/// <exception cref="NotImplementedException">when type is not supported</exception>
	protected virtual string MakeLocation(Type keyType, Type settingsType) {
		if (keyType == typeof(object))
			throw new NotImplementedException($"Type {keyType} as key is not supported");
		if (keyType.IsGenericType)
			throw new NotImplementedException($"Generic type {keyType} as key is not supported");
		if (settingsType == typeof(ISettings))
			throw new NotImplementedException($"Type {settingsType} as settings is not supported (should inherit ISettings)");
		if (!settingsType.IsInterface)
			throw new NotImplementedException($"Type {settingsType} as settings is not supported (should be interface)");
		if (settingsType.IsGenericType)
			throw new NotImplementedException($"Generic type {settingsType} as settings is not supported");
		
		var location = Path.Combine(Location, keyType.Name, settingsType.Name);
		return location;
	}

	async Task AssertType(Type type) {
		var settings = Constructor.CreateReal(type);
		var tempPath = Path.GetTempFileName();
		await Serializer.Serialize(type, settings, tempPath);
		await Serializer.Deserialize(type, settings, tempPath);
	}

	async Task<Pack> CreatePack(Type type, string? key) {
		var real = Constructor.CreateReal(type);
		real.Source = this;
		real.SourceKey = key;
		
		var path = GetPathToSettings(real.GetInterfaceType(), key);
		if (File.Exists(path))
			await Serializer.Deserialize(real.GetInterfaceType(), real, path);
		
		Pack pack = new() {
			Path = path,
			Settings = real
		};
		
		real.Subscribe(OnPropertyChanged);
		Debug.Print("Loaded {0} as {1}", type, real);
		return pack;
	}

	void OnPropertyChanged(object? sender, PropertyChangedEventArgs args) {
		Debug.Print("Changed property {0} of {1}", args.PropertyName, sender);
		_toSaveQueue.Add((IRealSettings)sender!);
	}

	async Task UpdateTask(object? tokenRaw) {
		Debug.Print("Started {0}", nameof(UpdateTask));

		var token = (CancellationToken)tokenRaw!;
		if (_isTrackChanges)
			SubscribeToDirectory();

		while (!token.IsCancellationRequested) {
			SpinWait.SpinUntil(() => _toSaveQueue.Count != 0 || token.IsCancellationRequested);

			_isSaving = true;
			try {
				(var updated, _toSaveQueue) = (_toSaveQueue, new HashSet<IRealSettings>());
				await Task.Delay(100, token); // idk wa or not
				await ProcessUpdates(updated);
			} finally {
				_isSaving = false;
			}
		}

		Debug.Print("Finished {0}", nameof(UpdateTask));
	}

	async Task ProcessUpdates(HashSet<IRealSettings> updated) {
		CheckLocation();

		foreach (var item in updated) {
			var intfType = item.GetInterfaceType();
			if (!await TryGetPackAsync(intfType, item.SourceKey, out var pack)) {
				Debug.Fail("Unknown item in updated queue");
				continue;
			}

			await ProcessUpdate(item, intfType, pack!);
		}
	}

	protected virtual async Task ProcessUpdate(IRealSettings settings, Type intfType, Pack pack) {
		try {
			var path = pack.Path;
			var real = pack.Settings;
			var fi = new FileInfo(pack.Path);
			if (settings.SourceKey is not null && fi.Directory is { Exists: false }) 
				fi.Directory.Create();

			await Serializer.Serialize(intfType, real, path);
			pack.Changed = fi.LastWriteTime;
			pack.Length = fi.Length;
			Debug.Print("Saved {0} as {1}", intfType, real);
		} catch (Exception e) {
			Debug.Print("error when saving settings: {0}", e);
			_toSaveQueue.Add(settings);
		}
	}

	void SubscribeToDirectory() {
		CheckLocation();

		try {
			var filter = "*" + Serializer.GetFileExtension();

			_watcher = new(Location, filter);
			_watcher.Changed += OnFileChanged;
			_watcher.Created += OnFileCreated;
			_watcher.Deleted += OnFileDeleted;
			_watcher.EnableRaisingEvents = true;

			Task.Factory.StartNew(ProcessFSEventsTask, _stopAllTasks.Token, _stopAllTasks.Token);
		} catch (Exception e) {
			Debug.Print("Error on {0}: {1}", nameof(SubscribeToDirectory), e);
		}
	}

	void OnFileDeleted(object sender, FileSystemEventArgs args) {
		Debug.Print("{0}: {1} | {2}", nameof(OnFileDeleted), args.Name, args.FullPath);
		_fsEvents.Add(args);
	}

	void OnFileCreated(object sender, FileSystemEventArgs args) {
		Debug.Print("{0}: {1} | {2}", nameof(OnFileCreated), args.Name, args.FullPath);
		_fsEvents.Add(args);
	}

	void OnFileChanged(object sender, FileSystemEventArgs args) {
		Debug.Print("{0}: {1} | {2}", nameof(OnFileChanged), args.Name, args.FullPath);
		_fsEvents.Add(args);
	}

	async Task ProcessFSEventsTask(object? tokenRaw) {
		Debug.Print("Started {0}", nameof(ProcessFSEventsTask));
		var token = (CancellationToken)tokenRaw!;

		while (!token.IsCancellationRequested) {
			SpinWait.SpinUntil(() => _fsEvents.Count != 0 || token.IsCancellationRequested);

			// _isUpdating = true;
			try {
				(var list, _fsEvents) = (_fsEvents, new HashSet<FileSystemEventArgs>());
				await Task.Delay(100, token); // idk wa or not
				await ProcessEvents(list);
			} finally {
				// _isUpdating = false;
			}
		}

		Debug.Print("Finished {0}", nameof(ProcessFSEventsTask));
	}

	async Task ProcessEvents(HashSet<FileSystemEventArgs> list) {
		// var ext = Serializer.GetFileExtension();
		foreach (var item in list) {
			try {
				// var name = item.Name.TrimSuffix(ext, StringComparison.OrdinalIgnoreCase);
				var path = item.FullPath;
				var pack = _map.Where(pair => pair.Value.Path == path).Select(pair => pair.Value).FirstOrDefault();
				if (pack is null) {
					Debug.Print("pack {0} not found!", path);
					continue;
				}

				var settings = pack.Settings;
				var type = settings.GetInterfaceType();
				// var path = pack.Path;
				if ((item.ChangeType & WatcherChangeTypes.Deleted) != 0)
					await DeleteAsync(type);
				else {
					var fi = new FileInfo(path);
					if (!fi.Exists || fi.LastWriteTime <= pack.Changed || fi.Length == pack.Length)
						continue;

					await Serializer.Deserialize(type, settings, path);
				}
			} catch (Exception e) {
				Debug.Print("Error in {0}: {1}", nameof(ProcessEvents), e);
			}
		}
	}

	void CheckLocation() {
		if (Directory.Exists(Location))
			return;

		try {
			Directory.CreateDirectory(Location);
		} catch (Exception e) {
			Debug.Print(e.ToString());
		}
	}
}

///
public abstract class FileSerializer {
	///
	public ISettingsTypeConstructor Constructor { get; }

	///
	public FileSerializer(ISettingsTypeConstructor constructor)
		=> Constructor = constructor;

	/// returns file extension for settings
	public virtual string GetFileExtension() => ".cfg";

	/// 
	/// <param name="type"></param>
	/// <param name="settings"></param>
	/// <param name="path"></param>
	/// <returns></returns>
	public abstract Task Serialize(Type type, IRealSettings settings, string path);

	/// 
	/// <param name="type"></param>
	/// <param name="settings"></param>
	/// <param name="path"></param>
	/// <returns></returns>
	public abstract Task Deserialize(Type type, IRealSettings settings, string path);
}