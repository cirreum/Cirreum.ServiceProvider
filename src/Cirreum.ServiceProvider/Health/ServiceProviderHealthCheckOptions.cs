namespace Cirreum.Health;

using Microsoft.Extensions.Diagnostics.HealthChecks;

/// <summary>
/// The base health check options for a service.
/// </summary>
public class ServiceProviderHealthCheckOptions {

	/// <summary>
	/// The tag name for a health check that participates in a readiness probe.
	/// </summary>
	public const string ReadinessTag = "ready";

	/// <summary>
	/// Set to <see langword="true"/>, to include this health check in Readiness checks.
	/// </summary>
	public bool IncludeInReadinessCheck { get; set; }

	/// <summary>
	/// Gets or sets the cache duration for health check results.
	/// Default is 60 seconds. Set to null to disable caching.
	/// </summary>
	public virtual TimeSpan? CachedResultTimeout { get; set; } = TimeSpan.FromSeconds(60);

	/// <summary>
	/// Gets or sets the status that should be reported when the health check fails.
	/// If not set (default), the health check will report Unhealthy.
	/// </summary>
	public HealthStatus FailureStatus { get; set; } = default;

	/// <summary>
	/// Gets or sets the timeout for the health check execution.
	/// If not set (default), the system-wide timeout will be used.
	/// </summary>
	public TimeSpan? Timeout { get; set; } = default;

}