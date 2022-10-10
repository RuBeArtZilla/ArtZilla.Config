namespace ArtZilla.Net.Config;

/// Kind of settings implementation
#if !GENERATOR
public
#endif

enum SettingsKind {
	/// <inheritdoc cref="ICopySettings"/> 
	Copy,

	/// <inheritdoc cref="IReadSettings"/> 
	Read,

	/// <inheritdoc cref="IInpcSettings"/> 
	Inpc,

	/// <inheritdoc cref="IRealSettings"/> 
	Real
}