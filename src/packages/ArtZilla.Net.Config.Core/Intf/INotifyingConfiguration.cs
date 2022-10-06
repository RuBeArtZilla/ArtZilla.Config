using System.ComponentModel;

namespace ArtZilla.Net.Config; 

/// <see cref="IConfiguration"/> with <see cref="INotifyPropertyChanged"/>
public interface INotifyingConfiguration : IConfiguration, INotifyPropertyChanged { }