namespace Cirreum.ServiceProvider.Health;

using Cirreum.Health;
using Microsoft.Extensions.Diagnostics.HealthChecks;

/// <summary>
/// A marker interface for a Service Provider's service Health Checks
/// </summary>
public interface IServiceProviderHealthCheck<THealthOptions>
	: IHealthCheck
	where THealthOptions : ServiceProviderHealthCheckOptions {
}