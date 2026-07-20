namespace Cirreum.ServiceProvider.Tests;

using Cirreum.Health;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

/// <summary>
/// Unit tests for the service provider registrar's collection-scoped registration
/// guards: duplicate instance keys and duplicate connection strings are rejected
/// within one composition but never across compositions (multiple hosts in one
/// process are isolated), and health checks register per composition rather than
/// once per process.
/// </summary>
public class ServiceProviderRegistrarTests {

	private static readonly IConfiguration EmptyConfiguration =
		new ConfigurationBuilder().Build();

	[Fact]
	public void RegisterInstance_DuplicateKeyInSameCollection_Throws() {
		var services = new ServiceCollection();
		new TestServiceProviderRegistrar()
			.RegisterInstance("alpha", CreateInstance("one", "cs-1"), services, EmptyConfiguration);

		var act = () => new TestServiceProviderRegistrar()
			.RegisterInstance("alpha", CreateInstance("two", "cs-2"), services, EmptyConfiguration);

		act.Should().Throw<InvalidOperationException>()
			.WithMessage("*alpha*");
	}

	[Fact]
	public void RegisterInstance_SameKeyInFreshCollection_DoesNotThrow() {
		// Two hosts composed in one process must be isolated: the guard is
		// collection-scoped, not process-global.
		new TestServiceProviderRegistrar()
			.RegisterInstance("alpha", CreateInstance("one", "cs-1"), new ServiceCollection(), EmptyConfiguration);

		var act = () => new TestServiceProviderRegistrar()
			.RegisterInstance("alpha", CreateInstance("one", "cs-1"), new ServiceCollection(), EmptyConfiguration);

		act.Should().NotThrow();
	}

	[Fact]
	public void RegisterInstance_DuplicateConnectionStringInSameCollection_Throws() {
		var services = new ServiceCollection();
		new TestServiceProviderRegistrar()
			.RegisterInstance("alpha", CreateInstance("one", "shared-cs"), services, EmptyConfiguration);

		var act = () => new TestServiceProviderRegistrar()
			.RegisterInstance("beta", CreateInstance("two", "shared-cs"), services, EmptyConfiguration);

		act.Should().Throw<InvalidOperationException>()
			.WithMessage("*connection string*");
	}

	[Fact]
	public void RegisterInstance_SameConnectionStringInFreshCollection_DoesNotThrow() {
		new TestServiceProviderRegistrar()
			.RegisterInstance("alpha", CreateInstance("one", "shared-cs"), new ServiceCollection(), EmptyConfiguration);

		var act = () => new TestServiceProviderRegistrar()
			.RegisterInstance("beta", CreateInstance("two", "shared-cs"), new ServiceCollection(), EmptyConfiguration);

		act.Should().NotThrow();
	}

	[Fact]
	public void RegisterInstance_HealthChecksEnabled_RegistersInEveryCollection() {
		// A second host composed in the same process must get its own health-check
		// registration — the dedup is per composition, never silently skipped.
		var first = new ServiceCollection();
		new TestServiceProviderRegistrar()
			.RegisterInstance("alpha", CreateInstance("one", "cs-1", healthChecks: true), first, EmptyConfiguration);

		var second = new ServiceCollection();
		new TestServiceProviderRegistrar()
			.RegisterInstance("alpha", CreateInstance("one", "cs-1", healthChecks: true), second, EmptyConfiguration);

		HasHealthCheckRegistration(first).Should().BeTrue();
		HasHealthCheckRegistration(second).Should().BeTrue();
	}

	[Fact]
	public void TryAddHealthCheck_DuplicateNameInSameCollection_RegistersOnce() {
		var services = new ServiceCollection();
		var registrar = new TestServiceProviderRegistrar();
		registrar.TryAddHealthCheck(services, CreateHealthCheckRegistration("dup-check"));
		var afterFirst = HealthCheckConfigureDescriptors(services);

		registrar.TryAddHealthCheck(services, CreateHealthCheckRegistration("dup-check"));

		HealthCheckConfigureDescriptors(services).Should().Be(afterFirst);
	}

	private static HealthCheckRegistration CreateHealthCheckRegistration(string name) => new(
		name,
		_ => throw new NotSupportedException("Composition-only test double."),
		failureStatus: null,
		tags: null);

	private static TestInstanceSettings CreateInstance(string name, string connectionString, bool healthChecks = false) => new() {
		Name = name,
		ConnectionString = connectionString,
		HealthChecks = healthChecks,
		HealthOptions = healthChecks ? new ServiceProviderHealthCheckOptions() : null,
	};

	private static bool HasHealthCheckRegistration(IServiceCollection services) =>
		HealthCheckConfigureDescriptors(services) > 0;

	private static int HealthCheckConfigureDescriptors(IServiceCollection services) =>
		services.Count(d => d.ServiceType == typeof(IConfigureOptions<HealthCheckServiceOptions>));

}
