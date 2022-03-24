using System.ComponentModel;

namespace ArtZilla.Net.Config; 

/// <summary><see cref="IConfiguration"/> with <see cref="INotifyPropertyChanged"/></summary>
public interface INotifyingConfiguration : IConfiguration, INotifyPropertyChanged { }