using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using ArtZilla.Net.Core.Extensions;
using Guard = CommunityToolkit.Diagnostics.Guard;
#if NET60_OR_GREATER
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using System.Diagnostics.CodeAnalysis;
#endif

namespace ArtZilla.Net.Config;

public abstract class FileSettingsProvider : ISettingsProvider, IDisposable {
	/// <inheritdoc cref="ISettingsTypeConstructor"/> 
	public ISettingsTypeConstructor Constructor { get; }

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
	readonly ConcurrentDictionary<Type, Pack> _map = new();

	public FileSettingsProvider()
		: this(GetDefaultLocation()) { }

	public FileSettingsProvider(string location)
		: this(location, new SameAssemblySettingsTypeConstructor()) { }

	public FileSettingsProvider(string location, ISettingsTypeConstructor constructor, bool isTrackChanges = true) {
		Guard.IsNotNullOrWhiteSpace(location);
		Guard.IsNotNull(constructor);
		Debug.Print("Created {0} with path {1}", GetType().Name, location);
		(Location, Constructor, _isTrackChanges) = (location, constructor, isTrackChanges);

		Task.Factory.StartNew(UpdateTask, _stopAllTasks.Token, _stopAllTasks.Token);
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
	public bool IsExist(Type type)
		=> TryGetPack(type, out var pack) && File.Exists(pack!.Path);

	/// <inheritdoc />
	public bool Delete(Type type) {
		const int maxTryCount = 10;
		const int pauseMs = 1000;

		Pack? pack = default;
		for (var i = 0; i < maxTryCount; ++i) {
			if (_map.TryRemove(type, out pack))
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
	public void Reset(Type type) {
		var pack = GetPack(type);
		var real = pack.Settings;
		var read = Constructor.CreateRead(real.GetInterfaceType());
		real.Copy(read);
	}

	/// <inheritdoc />
	public void Flush(Type? type = null) {
		if (type != null) {
			var pack = GetPack(type);
			var real = pack.Settings;
			SpinWait.SpinUntil(() => !_isSaving && !_toSaveQueue.Contains(real));
		} else
			SpinWait.SpinUntil(() => !_isSaving && _toSaveQueue.Count == 0);
	}

	/// <inheritdoc />
	public ISettings Get(Type type, SettingsKind kind) {
		var pack = GetPack(type);
		var real = pack.Settings;
		if (kind == SettingsKind.Real)
			return real;

		var settings = Constructor.Create(type, kind, real);
		settings.Source = this;
		return settings;
	}

	/// <inheritdoc />
	public void Set(ISettings settings) {
		var type = settings.GetInterfaceType();
		var pack = GetPack(type);
		var real = pack.Settings;
		real.Copy(settings);
	}

	/// <inheritdoc />
	public async Task<bool> IsExistAsync(Type type)
		=> await TryGetPackAsync(type, out var pack) && File.Exists(pack!.Path);

	/// <inheritdoc />
	public Task<bool> DeleteAsync(Type type)
		=> Task.FromResult(Delete(type));

	/// <inheritdoc />
	public async Task ResetAsync(Type type) {
		var pack = await GetPackAsync(type);
		var real = pack.Settings;
		var read = Constructor.CreateRead(real.GetInterfaceType());
		real.Copy(read);
	}

	/// <inheritdoc />
	public async Task FlushAsync(Type? type = null) {
		if (type != null) {
			var pack = await GetPackAsync(type);
			var real = pack.Settings;
			SpinWait.SpinUntil(() => !_isSaving && !_toSaveQueue.Contains(real));
		} else
			SpinWait.SpinUntil(() => !_isSaving && _toSaveQueue.Count == 0);
	}

	/// <inheritdoc />
	public async Task<ISettings> GetAsync(Type type, SettingsKind kind) {
		var pack = await GetPackAsync(type);
		var real = pack.Settings;
		if (kind == SettingsKind.Real)
			return real;

		var settings = Constructor.Create(type, kind, real);
		settings.Source = this;
		return settings;
	}

	/// <inheritdoc />
	public async Task SetAsync(ISettings settings) {
		var type = settings.GetInterfaceType();
		var pack = await GetPackAsync(type);
		var real = pack.Settings;
		real.Copy(settings);
	}

	/// <inheritdoc />
	public void ThrowIfNotSupported(Type type) 
		=> AssertType(type).Wait();

	public string GetPathToSettings(Type type)
		=> Path.Combine(Location, type.Name + GetFileExtension());

#if NETSTANDARD20
	protected virtual bool TryGetPack(Type type, out Pack? pack)
#else
	protected virtual bool TryGetPack(Type type, [NotNullWhen(true)] out Pack? pack)
#endif
		=> _map.TryGetValue(type, out pack);

#if NETSTANDARD20
	protected virtual Task<bool> TryGetPackAsync(Type type, out Pack? pack)
#else
	protected virtual Task<bool> TryGetPackAsync(Type type, [NotNullWhen(true)] out Pack? pack)
#endif
		=> Task.FromResult(TryGetPack(type, out pack));

	protected virtual Pack GetPack(Type type)
		=> _map.GetOrAdd(type, i => CreatePack(i).Result);

	protected virtual Task<Pack> GetPackAsync(Type type)
		=> Task.FromResult(GetPack(type));

	protected virtual string GetFileExtension() => ".cfg";

	protected abstract Task Serialize(Type type, IRealSettings settings, string path);

	protected abstract Task Deserialize(Type type, IRealSettings settings, string path);

	async Task AssertType(Type type) {
		var settings = Constructor.CreateReal(type);
		var tempPath = Path.GetTempFileName();
		await Serialize(type, settings, tempPath);
		await Deserialize(type, settings, tempPath);
	}

	async Task<Pack> CreatePack(Type type) {
		var real = Constructor.CreateReal(type);
		real.Source = this;
		var path = GetPathToSettings(real.GetInterfaceType());
		if (File.Exists(path))
			await Deserialize(real.GetInterfaceType(), real, path);
		Pack pack = new() {
			Path = path,
			Settings = real
		};
		real.Subscribe(OnPropertyChanged);
		Debug.Print("Loaded {0} as {1}", type, real);
		return pack;
	}

	void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
		=> _toSaveQueue.Add((IRealSettings)sender!);

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
			if (!await TryGetPackAsync(intfType, out var pack)) {
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
			await Serialize(intfType, real, path);
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
			var filter = "*" + GetFileExtension();

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
		var ext = GetFileExtension();
		foreach (var item in list) {
			try {
				var name = item.Name.TrimSuffix(ext, StringComparison.OrdinalIgnoreCase);
				var pack = _map.Where(pair => pair.Key.Name == name).Select(pair => pair.Value).FirstOrDefault();
				if (pack is null) {
					Debug.Print("pack {0} not found", name);
					continue;
				}

				var settings = pack.Settings;
				var type = settings.GetInterfaceType();
				var path = pack.Path;
				if ((item.ChangeType & WatcherChangeTypes.Deleted) != 0)
					await DeleteAsync(type);
				else {
					var fi = new FileInfo(path);
					if (!fi.Exists || fi.LastWriteTime <= pack.Changed || fi.Length == pack.Length)
						continue;
					
					await Deserialize(type, settings, path);
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