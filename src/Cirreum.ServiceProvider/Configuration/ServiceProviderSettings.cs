namespace Cirreum.ServiceProvider.Configuration;

using Cirreum.Health;
using Cirreum.Providers.Configuration;

/// <summary>
/// Base class for service provider configuration settings.
/// </summary>
public abstract class ServiceProviderSettings {
	/// <summary>
	/// Gets the string value for a Default service registration.
	/// </summary>
	public const string DefaultKey = "default";
}

/// <summary>
/// Generic base class for service provider configuration settings with typed instance settings and health options.
/// </summary>
/// <typeparam name="TInstanceSettings">The type of instance-specific settings.</typeparam>
/// <typeparam name="THealthOptions">The type of health check options.</typeparam>
public abstract class ServiceProviderSettings<TInstanceSettings, THealthOptions>
	: ServiceProviderSettings
	, IProviderSettings<TInstanceSettings>
	where THealthOptions : ServiceProviderHealthCheckOptions
	where TInstanceSettings : ServiceProviderInstanceSettings<THealthOptions> {

	/// <summary>
	/// Gets or sets a boolean value that indicates whether the OpenTelemetry tracing is enabled or not.
	/// </summary>
	/// <value>
	/// The default value is <see langword="true"/>.
	/// </value>
	public bool Tracing { get; set; } = true;

	/// <summary>
	/// Collection of Provider service instance settings
	/// </summary>
	public Dictionary<string, TInstanceSettings> Instances { get; set; } = [];

}