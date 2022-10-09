using System.ComponentModel;

namespace ArtZilla.Net.Config; 

/// <see cref="IConfiguration"/> with <see cref="INotifyPropertyChanged"/>
[Obsolete("Use ISettingsProvider")]
public interface INotifyingConfiguration : IConfiguration, INotifyPropertyChanged { }