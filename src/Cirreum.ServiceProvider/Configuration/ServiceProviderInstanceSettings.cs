namespace Cirreum.ServiceProvider.Configuration;

using Cirreum.Health;
using Cirreum.Providers.Configuration;

/// <summary>
/// The base settings for a service provider instance, for use within a providers configuration section.
/// </summary>
public abstract class ServiceProviderInstanceSettings
	: IProviderInstanceSettings {

	/// <summary>
	/// Gets or sets the name of an instance of the provider.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This value is used to retrieve the connection string from either the connection 
	/// strings sections of either the local settings file or a secrets provider.
	/// </para>
	/// </remarks>
	public string Name { get; set; } = "";

	/// <summary>
	/// Gets or sets a boolean value that indicates if health checking is enabled.
	/// </summary>
	/// <value>
	/// The default value is <see langword="false"/>.
	/// </value>
	public bool HealthChecks { get; set; } = false;

	/// <summary>
	/// Gets or sets the resolved connection string (connection string, service url, endpoint, namespace etc.)
	/// </summary>
	public string? ConnectionString { get; set; }

	/// <summary>
	/// Allows a derived service provider to implement custom connection string parsing and potentially populate
	/// other property values.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Default just [re]sets the <see cref="ConnectionString"/> with the specified raw value.
	/// </para>
	/// </remarks>
	public virtual void ParseConnectionString(string rawValue) {

		if (string.IsNullOrWhiteSpace(rawValue)) {
			throw new ArgumentException("Connection string cannot be null or empty.", nameof(rawValue));
		}

		// Overwrite with the Real connection string
		this.ConnectionString = rawValue;

	}

	/// <summary>
	/// Optional string value to include when verifying unique connection string
	/// values.
	/// </summary>
	protected internal virtual string? ConnectionStringDiscriminator() => string.Empty;

}

/// <summary>
/// The base settings for a service provider instance, for use within a providers configuration section.
/// </summary>
public abstract class ServiceProviderInstanceSettings<THealthOptions>
	: ServiceProviderInstanceSettings
	where THealthOptions : ServiceProviderHealthCheckOptions {

	/// <summary>
	/// The base/default options for the health check.
	/// </summary>
	public abstract THealthOptions? HealthOptions { get; set; }

}