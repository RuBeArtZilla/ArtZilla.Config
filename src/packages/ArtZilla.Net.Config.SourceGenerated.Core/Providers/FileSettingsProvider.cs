using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using ArtZilla.Net.Core.Extensions;
using Guard = CommunityToolkit.Diagnostics.Guard;

namespace ArtZilla.Net.Config;

///
public abstract class FileSettingsProvider : BaseSettingsProvider, IDisposable {
	///
	public FileSerializer Serializer { get; }

	/// 
	public string Location { get; }

	/// 
	protected record Pack {
		public string Path;
		public IRealSettings Settings;

		public DateTime Changed;
		public long Length;

		public Pack() { }

		public Pack(IRealSettings settings, string path)
			=> (Settings, Path) = (settings, path);

		public void Set(FileInfo fi)
			=> (Length, Changed) = (fi.Length, fi.LastWriteTime);
	}

	bool _isSaving;
	FileSystemWatcher? _watcher;
	HashSet<IRealSettings> _toSaveQueue = new();
	HashSet<FileSystemEventArgs> _fsEvents = new();
	readonly bool _isTrackChanges;
	readonly CancellationTokenSource _stopAllTasks = new();
	readonly ConcurrentDictionary<(Type Type, string? Key), Pack> _map = new();
	readonly MemorySettingsProvider _memory;

	/// 
	/// <param name="constructor"></param>
	/// <param name="serializer"></param>
	/// <param name="location"></param>
	/// <param name="isTrackChanges"></param>
	protected FileSettingsProvider(
		ISettingsTypeConstructor constructor,
		FileSerializer serializer,
		string? location = null,
		bool isTrackChanges = true
	) : base(constructor) {
		Guard.IsNotNull(serializer);
		Guard.IsNotNull(constructor);
		Debug.Print("Created {0} with path {1}", GetType().Name, location);
		(Serializer, Location, _isTrackChanges) =
			(serializer, location ?? GetDefaultLocation(), isTrackChanges);

		_memory = new(constructor);
		Task.Factory.StartNew(UpdateTask, _stopAllTasks.Token, _stopAllTasks.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
	}

	/// 
	/// <param name="app"></param>
	/// <param name="company"></param>
	/// <returns></returns>
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
	public override bool IsExist(Type type, string? key = null)
		=> TryGetPack(type, key, out var pack) && File.Exists(pack!.Path);

	/// <inheritdoc />
	public override bool Delete(Type type, string? key = null) {
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
	public override void Reset(Type type, string? key = null) {
		Debug.Print("> {0}.{1}({3}, {4}); [{2}]", GetType().Name, nameof(Reset), GetId(), type, key);

		var pack = GetPack(type, key);
		var real = pack.Settings;
		var read = _memory.Read(type, key);

		real.Copy(read);

		Debug.Print("< {0}.{1}({3}, {4}); [{2}]", GetType().Name, nameof(Reset), GetId(), type, key);
	}

	/// <inheritdoc />
	public override void Flush(Type? type = null, string? key = null) {
		if (type != null) {
			var pack = GetPack(type, key);
			var real = pack.Settings;
			SpinWait.SpinUntil(() => !_isSaving && !_toSaveQueue.Contains(real));
		} else
			SpinWait.SpinUntil(() => !_isSaving && _toSaveQueue.Count == 0);
	}

	/// <inheritdoc />
	public override ISettings Get(Type type, SettingsKind kind, string? key = null) {
		var pack = GetPack(type, key);
		var real = pack.Settings;
		if (kind == SettingsKind.Real)
			return real;

		var settings = Constructor.Create(type, kind, this, key, real);
		return settings;
	}

	/// <inheritdoc />
	public override void Set(ISettings settings, string? key = null) {
		var type = settings.GetInterfaceType();
		var pack = GetPack(type, key);
		var real = pack.Settings;
		real.Copy(settings);
	}

	/// <inheritdoc />
	public override async Task<bool> IsExistAsync(Type type, string? key = null)
		=> await TryGetPackAsync(type, key, out var pack).ConfigureAwait(false) && File.Exists(pack!.Path);

	/// <inheritdoc />
	public override Task<bool> DeleteAsync(Type type, string? key = null)
		=> Task.FromResult(Delete(type));

	/// <inheritdoc />
	public override async Task ResetAsync(Type type, string? key = null) {
		Debug.Print("> {0}.{1}({3}, {4}); [{2}]", GetType().Name, nameof(ResetAsync), GetId(), type, key);

		var pack = await GetPackAsync(type, key).ConfigureAwait(false);
		var real = pack.Settings;
		var read = await _memory.ReadAsync(type, key);

		real.Copy(read);

		Debug.Print("< {0}.{1}({3}, {4}); [{2}]", GetType().Name, nameof(ResetAsync), GetId(), type, key);
	}

	long GetId() => GetHashCode();

	/// <inheritdoc />
	public override async Task FlushAsync(Type? type = null, string? key = null) {
		Debug.Print("> {0}.{1}({3}, {4}); [{2}]", GetType().Name, nameof(FlushAsync), GetId(), type, key);

		if (type != null) {
			var pack = await GetPackAsync(type, key).ConfigureAwait(false);
			var real = pack.Settings;
			SpinWait.SpinUntil(() => !_isSaving && !_toSaveQueue.Contains(real) && !_fsEvents.Any());
		} else
			SpinWait.SpinUntil(() => !_isSaving && _toSaveQueue.Count == 0 && !_fsEvents.Any());

		Debug.Print("< {0}.{1}({3}, {4}); [{2}]", GetType().Name, nameof(FlushAsync), GetId(), type, key);
	}

	/// <inheritdoc />
	public override async Task<ISettings> GetAsync(Type type, SettingsKind kind, string? key = null) {
		var pack = await GetPackAsync(type, key).ConfigureAwait(false);
		var real = pack.Settings;
		if (kind == SettingsKind.Real)
			return real;

		var settings = Constructor.Create(type, kind, this, key, real);
		return settings;
	}

	/// <inheritdoc />
	public override async Task SetAsync(ISettings settings, string? key = null) {
		var type = settings.GetInterfaceType();
		var pack = await GetPackAsync(type, key).ConfigureAwait(false);
		var real = pack.Settings;
		real.Copy(settings);
	}

	/// <inheritdoc />
	public override void ThrowIfNotSupported(Type type)
		=> AssertType(type).Wait();

	///
	public string GetPathToSettings(Type type, string? key = null)
		=> key is null
			? Path.Combine(Location, type.Name + Serializer.GetFileExtension())
			: Path.Combine(Location, type.Name, EnframeFilename(key) + Serializer.GetFileExtension());

	static string EnframeFilename(string filename) {
		return filename; // todo: ...
	}

	///
	public bool IsAnyChangesInQueue() {
		Debug.Print("In [{0}] queue write {1} read {2}",
		            GetId(), _toSaveQueue.Count, _fsEvents.Count);
		return _fsEvents.Count != 0 || _toSaveQueue.Count != 0;
	}

	///
#if NETSTANDARD20
	protected bool TryGetPack(Type type, string? key, out Pack? pack)
#else
	protected bool TryGetPack(Type type, string? key, [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out Pack? pack)
#endif
		=> _map.TryGetValue((type, key), out pack);

	///
#if NETSTANDARD20
	protected Task<bool> TryGetPackAsync(Type type, string? key, out Pack? pack)
#else
	protected Task<bool> TryGetPackAsync(Type type, string? key, [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out Pack? pack)
#endif
		=> Task.FromResult(TryGetPack(type, key, out pack));

	///
	protected Pack GetPack(Type type, string? key)
		=> _map.GetOrAdd((type, key), i => CreatePack(i.Type, i.Key));

	///
	protected Task<Pack> GetPackAsync(Type type, string? key)
		=> Task.FromResult(GetPack(type, key));

	async Task AssertType(Type type) {
		var tempPath = Path.GetTempFileName();
		var expected = Constructor.Create(type, SettingsKind.Copy, this, null, true);
		Serializer.Serialize(type, tempPath, expected);
		var actual = Serializer.Deserialize(type, tempPath);
		// Guard.IsEqualTo(expected, actual); // <- todo
	}

	Pack CreatePack(Type type, string? key) {
		var path = GetPathToSettings(type, key);
		var (source, size, time) = ReadAsync(type, path);
		var settings = source is not null
			? Constructor.CloneReal(source)
			: Constructor.DefaultReal(type, this, key);

		Pack pack = new(settings, path) { Length = size, Changed = time };

		settings.Subscribe(OnPropertyChanged);
		Debug.Print("Loaded {0} as {1}", type, settings);
		return pack;
	}

		(ISettings? Settings, long Size, DateTime Time) ReadAsync(Type type, string path) {
		try {
			var fi = new FileInfo(path);
			if (!fi.Exists)
				return default;

			var size = fi.Length;
			var time = fi.LastWriteTime;
			var settings = Serializer.Deserialize(type, path);
			return (settings, size, time);
		} catch (Exception e) {
			Debug.WriteLine(e);
			return default;
		}
	}

	void OnPropertyChanged(object? sender, PropertyChangedEventArgs args) {
		Debug.Print("Changed property {0} of {1}", args.PropertyName, sender);
		_toSaveQueue.Add((IRealSettings) sender!);
	}

	async Task UpdateTask(object? tokenRaw) {
		Debug.Print("Started {0}", nameof(UpdateTask));

		var token = (CancellationToken) tokenRaw!;
		if (_isTrackChanges)
			SubscribeToDirectory();

		while (!token.IsCancellationRequested) {
			SpinWait.SpinUntil(() => _toSaveQueue.Count != 0 || token.IsCancellationRequested);

			_isSaving = true;
			try {
				(var updated, _toSaveQueue) = (_toSaveQueue, new HashSet<IRealSettings>());
				await Task.Delay(100, token); // idk wa or not
				ProcessUpdates(updated);
			} finally {
				_isSaving = false;
			}
		}

		Debug.Print("Finished {0}", nameof(UpdateTask));
	}

	void ProcessUpdates(HashSet<IRealSettings> updated) {
		CheckLocation();

		foreach (var item in updated) {
			var intfType = item.GetInterfaceType();
			if (!TryGetPack(intfType, item.SourceKey, out var pack)) {
				Debug.Fail("Unknown item in updated queue");
				continue;
			}

			ProcessUpdate(item, intfType, pack!);
		}
	}

	void ProcessUpdate(IRealSettings settings, Type intfType, Pack pack) {
		try {
			var path = pack.Path;
			var real = pack.Settings;
			var fi = new FileInfo(pack.Path);
			if (settings.SourceKey is not null && fi.Directory is { Exists: false })
				fi.Directory.Create();

			Serializer.Serialize(intfType, path, real);
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

			Task.Factory.StartNew(ProcessFSEventsTask, _stopAllTasks.Token, _stopAllTasks.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
		} catch (Exception e) {
			Debug.Print("Error on {0}: {1}", nameof(SubscribeToDirectory), e);
		}
	}

	void OnFileDeleted(object sender, FileSystemEventArgs args) {
		Debug.Print("{0}[{3}]: {1} | {2}", nameof(OnFileDeleted), args.Name, args.FullPath, GetId());
		_fsEvents.Add(args);
	}

	void OnFileCreated(object sender, FileSystemEventArgs args) {
		Debug.Print("{0}[{3}]: {1} | {2}", nameof(OnFileCreated), args.Name, args.FullPath, GetId());
		_fsEvents.Add(args);
	}

	void OnFileChanged(object sender, FileSystemEventArgs args) {
		Debug.Print("{0}[{3}]: {1} | {2}", nameof(OnFileChanged), args.Name, args.FullPath, GetId());
		_fsEvents.Add(args);
	}

	async Task ProcessFSEventsTask(object? tokenRaw) {
		Debug.Print("Started {0}", nameof(ProcessFSEventsTask));
		var token = (CancellationToken) tokenRaw!;

		while (!token.IsCancellationRequested) {
			SpinWait.SpinUntil(() => _fsEvents.Count != 0 || token.IsCancellationRequested);
			Debug.Print("!!!Catched {0}", nameof(ProcessFSEventsTask));

			// _isUpdating = true;
			try {
				(var list, _fsEvents) = (_fsEvents, new HashSet<FileSystemEventArgs>());
				await Task.Delay(100, token); // idk wa or not
				ProcessEvents(list);
			} finally {
				// _isUpdating = false;
			}
		}

		Debug.Print("Finished {0}", nameof(ProcessFSEventsTask));
	}

	void ProcessEvents(HashSet<FileSystemEventArgs> list) {
		foreach (var item in list) {
			try {
				var path = item.FullPath;
				Pack? pack = null;
				var type = default(Type);
				var key = default(string?);
				foreach (var (mapKey, value) in _map) {
					if (value.Path != path)
						continue;

					pack = value;
					(type, key) = mapKey;
					break;
				}

				if (pack is null || type is null) {
					Debug.Print("* pack {0} not found!", path);
					continue;
				}

				if ((item.ChangeType & WatcherChangeTypes.Deleted) != 0) {
					Debug.Print("* ({0}, {1}) externally removed from {2}", type, key, GetId());
					Delete(type, key);
				} else {
					var fi = new FileInfo(path);
					if (!fi.Exists) {
						Debug.Print("* {0} skipped changes in {1}", path, GetId());
						continue;
					}

					var sameTime = fi.LastWriteTime == pack.Changed;
					var sameSize = fi.Length == pack.Length;
					if (sameTime && sameSize) {
						if (sameTime)
							Debug.Print("* {0} skipped changes in {1} (changed {2}, last {3})", path, GetId(), fi.LastWriteTime, pack.Changed);
						if (sameSize)
							Debug.Print("* {0} skipped changes in {1} (length {2}, last {3})", path, GetId(), fi.Length, pack.Length);
						continue;
					}

					Debug.Print("* ({0}, {1}) externally changed in {2}", type, key, GetId());
					var read = Serializer.Deserialize(type, path);
					pack.Set(fi);
					pack.Settings.Copy(read);
				}
			} catch (Exception e) {
				Debug.Print("* Error in {0}: {1}", nameof(ProcessEvents), e);
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