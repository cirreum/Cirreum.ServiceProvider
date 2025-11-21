namespace Cirreum.ServiceProvider;

using Cirreum.Health;
using Cirreum.ServiceProvider.Configuration;
using Cirreum.ServiceProvider.Health;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

/// <summary>
/// Base registrar for service providers that handles registration of services, health checks, and telemetry.
/// </summary>
/// <typeparam name="TSettings">
/// The service provider's settings type that contains configuration for one or more instances 
/// of the service. Must inherit from ServiceProviderSettings.
/// </typeparam>
/// <typeparam name="TInstanceSettings">
/// Configuration settings for a single instance of the service provider. 
/// Must inherit from ServiceProviderInstanceSettings and contain health check options.
/// </typeparam>
/// <typeparam name="THealthOptions">
/// Health check configuration options specific to this service provider type.
/// Must inherit from ServiceProviderHealthCheckOptions and have a parameterless constructor.
/// </typeparam>
public abstract class ServiceProviderRegistrar<TSettings, TInstanceSettings, THealthOptions>
	: IProviderRegistrar<TSettings, TInstanceSettings>
	where TSettings : ServiceProviderSettings<TInstanceSettings, THealthOptions>
	where TInstanceSettings : ServiceProviderInstanceSettings<THealthOptions>
	where THealthOptions : ServiceProviderHealthCheckOptions, new() {

	private static readonly Dictionary<string, string> processedInstances = [];
	private static readonly Dictionary<string, bool> healthChecks = [];

	/// <inheritdoc/>
	public abstract ProviderType ProviderType { get; }

	/// <inheritdoc/>
	public abstract string ProviderName { get; }

	/// <summary>
	/// Gets the namespace(s) for open telemetry tracing.
	/// </summary>
	public abstract string[] ActivitySourceNames { get; }

	/// <inheritdoc/>
	public virtual void ValidateSettings(TInstanceSettings settings) {
	}

	/// <summary>
	/// Registers all of the provider's configured service implementations with the dependency injection container.
	/// </summary>
	/// <param name="providerSettings">An instance of the provider-specific settings populated from application settings.</param>
	/// <param name="services">The DI container's service collection where services will be registered.</param>
	/// <param name="configuration">The root configuration object providing access to the full application configuration.</param>
	/// <remarks>
	/// This method performs the complete service registration process:
	/// <list type="bullet">
	///   <item>
	///     <description>Reads and validates provider-specific configuration settings</description>
	///   </item>
	///   <item>
	///     <description>Registers service implementations with their appropriate DI lifetimes (Singleton, Scoped, or Transient)</description>
	///   </item>
	///   <item>
	///     <description>Configures and initializes any required service dependencies</description>
	///   </item>
	///   <item>
	///     <description>Sets up any necessary middleware or background services</description>
	///   </item>
	/// </list>
	/// </remarks>
	public virtual void Register(
		TSettings providerSettings,
		IServiceCollection services,
		IConfiguration configuration) {

		if (providerSettings is null || providerSettings.Instances.Count == 0) {
			return;
		}

		foreach (var (key, settings) in providerSettings.Instances) {
			this.RegisterInstance(key, settings, services, configuration);
		}

		if (providerSettings.Tracing && this.ActivitySourceNames is not null && this.ActivitySourceNames.Length > 0) {
			services.AddOpenTelemetry()
				.WithTracing(traceBuilder => traceBuilder.AddSource(this.ActivitySourceNames));
		}

	}

	/// <summary>
	/// Registers a single provider instance with the dependency injection container.
	/// </summary>
	/// <param name="key">The unique identifier for this provider instance, typically derived from configuration.</param>
	/// <param name="settings">The configuration settings specific to this provider instance.</param>
	/// <param name="services">The DI container's service collection where services will be registered.</param>
	/// <param name="configuration">The root configuration object providing access to the full application configuration.</param>
	/// <remarks>
	/// This method handles the registration of an individual service provider instance:
	/// <list type="bullet">
	///   <item>
	///     <description>Validates the instance-specific settings</description>
	///   </item>
	///   <item>
	///     <description>Registers the instance's services with appropriate DI lifetimes</description>
	///   </item>
	///   <item>
	///     <description>Configures instance-specific dependencies and health checks</description>
	///   </item>
	/// </list>
	/// This method is called by <see cref="Register"/> for each configured instance, but can also be used
	/// independently to register single instances when needed.
	/// </remarks>
	public virtual void RegisterInstance(
		string key,
		TInstanceSettings settings,
		IServiceCollection services,
		IConfiguration configuration) {

		// Ensure no duplicate registration keys
		var providerRegistrationKey = $"Cirreum.{this.ProviderType}.{this.ProviderName}:{key}";
		if (!processedInstances.TryAdd(providerRegistrationKey, settings.Name)) {
			throw new InvalidOperationException($"A service with the key of '{key}' has already been registered.");
		}

		// Must have settings...
		if (settings is null) {
			throw new InvalidOperationException($"Missing required settings for the service '{key}' instance");
		}

		// Must have a Name
		if (string.IsNullOrWhiteSpace(settings.Name)) {
			throw new InvalidOperationException($"Missing required name for the service '{key}' instance");
		}

		// Get and/or parse connection string
		if (configuration.GetConnectionString(settings.Name) is string configConnectionString) {
			// Assumes the value is in Azure KeyVault or in the ConnectionStrings section
			settings.ParseConnectionString(configConnectionString);
		} else if (!string.IsNullOrWhiteSpace(settings.ConnectionString)) {
			settings.ParseConnectionString(settings.ConnectionString);
		} else {
			// Must have a connection string
			throw new InvalidOperationException($"Missing required Instance ConnectionString for service '{key}'");
		}

		// Validate ConnectionString regardless of provider specific validation
		settings.ValidateConnectionString(this.ProviderType, this.ProviderName);

		// Provider specific validation...
		this.ValidateSettings(settings);

		// Add the ServiceProvider...
		this.AddServiceProviderInstance(services, key, settings);

		// Register health check if enabled
		if (settings.HealthChecks && settings.HealthOptions is not null) {

			var healthTags = new List<string> {
				$"{this.ProviderType}".ToLowerInvariant(),
				$"{this.ProviderName}".ToLowerInvariant()
			};
			if (settings.HealthOptions.IncludeInReadinessCheck) {
				healthTags.Insert(0, ServiceProviderHealthCheckOptions.ReadinessTag);
			}

			TimeSpan? hcTimeout = default;
			if (settings.HealthOptions.Timeout is not null && settings.HealthOptions.Timeout.Value.TotalSeconds > 0) {
				hcTimeout = settings.HealthOptions.Timeout;
			}
			this.TryAddHealthCheck(services, new HealthCheckRegistration(
				$"{providerRegistrationKey.ToKebabCase()}",
				sp => this.CreateHealthCheck(sp, key, settings),
				failureStatus: settings.HealthOptions.FailureStatus,
				tags: healthTags,
				timeout: hcTimeout));

		}

	}

	/// <summary>
	/// If configured, is called to register the service providers health check instance.
	/// </summary>
	/// <param name="services">The DI <see cref="IServiceCollection"/>.</param>
	/// <param name="healthCheckRegistration">The <see cref="HealthCheckRegistration"/> to register.</param>
	public virtual void TryAddHealthCheck(
		IServiceCollection services,
		HealthCheckRegistration healthCheckRegistration) {
		TryAddHealthCheck(services, healthCheckRegistration.Name, hcBuilder => hcBuilder.Add(healthCheckRegistration));
	}


	/// <summary>
	/// Adds a single instance of the service provider to the service collection.
	/// </summary>
	/// <param name="services">The DI container's service collection where services will be registered.</param>
	/// <param name="serviceKey">
	/// The unique key for this service instance. Used for keyed service registration 
	/// and must be unique across all service providers.
	/// </param>
	/// <param name="settings">
	/// Configuration settings for this specific instance of the service provider.
	/// Contains connection details, health check options, and provider-specific settings.
	/// </param>
	protected abstract void AddServiceProviderInstance(
		IServiceCollection services,
		string serviceKey,
		TInstanceSettings settings);

	/// <summary>
	/// Creates a health check instance for monitoring this service provider's health status.
	/// </summary>
	/// <param name="serviceProvider">
	/// The dependency injection container used to resolve required services.
	/// </param>
	/// <param name="serviceKey">
	/// The unique key identifying which service instance this health check monitors.
	/// </param>
	/// <param name="settings">
	/// Configuration settings for the service instance being monitored,
	/// including health check options.
	/// </param>
	/// <returns>
	/// An <see cref="IServiceProviderHealthCheck{THealthOptions}"/> implementation that can monitor the health 
	/// of this specific service provider instance.
	/// </returns>
	protected abstract IServiceProviderHealthCheck<THealthOptions> CreateHealthCheck(
		IServiceProvider serviceProvider,
		string serviceKey,
		TInstanceSettings settings);


	private static void TryAddHealthCheck(
		IServiceCollection services,
		string name,
		Action<IHealthChecksBuilder> addHealthCheck) {
		var healthCheckKey = $"Cirreum.ServiceProvider.HealthChecks::{name}";
		if (!healthChecks.ContainsKey(healthCheckKey)) {
			healthChecks[healthCheckKey] = true;
			addHealthCheck(services.AddHealthChecks());
		}
	}

}