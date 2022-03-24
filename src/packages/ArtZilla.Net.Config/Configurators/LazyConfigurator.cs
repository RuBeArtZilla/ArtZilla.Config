using System;
using System.ComponentModel;

namespace ArtZilla.Net.Config.Configurators; 

public class LazyConfigurator: MemoryConfigurator {
	public LazyConfigurator(IIoThread thread) => Io = thread ?? throw new ArgumentNullException(nameof(thread));

	public void Flush() => Io.Flush();

	protected override IConfigurator<TConfiguration> CreateTypedConfigurator<TConfiguration>()
		=> new LazyConfigurator<TConfiguration>(Io);

	protected readonly IIoThread Io;
}


public class LazyConfigurator<TConfiguration>: MemoryConfigurator<TConfiguration> where TConfiguration : class, IConfiguration {
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

public class LazyConfigurator<TKey, TConfiguration>
	: MemoryConfigurator<TKey, TConfiguration> where TConfiguration : class, IConfiguration {
	public LazyConfigurator(IIoThread thread)
		=> Io = thread ?? throw new ArgumentNullException(nameof(thread));

	public void Flush() => Io.Flush();

	public override void Save(TKey key, TConfiguration value) {
		base.Save(key, value);

		if (value is IRealtimeConfiguration)
			Io.ToSave(key, value);
		else
			Io.ToSave(key, Get(key));
	}

	public override void Reset(TKey key) {
		base.Reset(key);
		Io.Reset<TKey, TConfiguration>(key);
	}

	public override bool IsExist(TKey key) {
		if (base.IsExist(key))
			return true;

		return Io.TryLoad<TKey, TConfiguration>(key, out _);
	}

	protected override TConfiguration Load(TKey key)
		=> Io.TryLoad<TKey, TConfiguration>(key, out var loaded)
			? loaded
			: base.Load(key);

	protected override void ConfigurationChanged(TKey key, TConfiguration configuration, string propertyName)
		=> Io.ToSave(key, configuration);

	protected readonly IIoThread Io;
}