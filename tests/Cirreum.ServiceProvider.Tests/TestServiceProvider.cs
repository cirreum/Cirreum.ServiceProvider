namespace Cirreum.ServiceProvider.Tests;

using Cirreum.Health;
using Cirreum.Providers;
using Cirreum.ServiceProvider;
using Cirreum.ServiceProvider.Configuration;
using Cirreum.ServiceProvider.Health;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Concrete test doubles for exercising
/// <see cref="ServiceProviderRegistrar{TSettings, TInstanceSettings, THealthOptions}"/>
/// registration behavior.
/// </summary>
internal sealed class TestInstanceSettings : ServiceProviderInstanceSettings<ServiceProviderHealthCheckOptions> {

	public override ServiceProviderHealthCheckOptions? HealthOptions { get; set; }

}

internal sealed class TestProviderSettings : ServiceProviderSettings<TestInstanceSettings, ServiceProviderHealthCheckOptions> {
}

internal sealed class TestServiceProviderRegistrar : ServiceProviderRegistrar<TestProviderSettings, TestInstanceSettings, ServiceProviderHealthCheckOptions> {

	public override ProviderType ProviderType => ProviderType.None;

	public override string ProviderName => "TestService";

	public override string[] ActivitySourceNames => [];

	protected override void AddServiceProviderInstance(
		IServiceCollection services,
		string serviceKey,
		TestInstanceSettings settings) {
	}

	protected override IServiceProviderHealthCheck<ServiceProviderHealthCheckOptions> CreateHealthCheck(
		IServiceProvider serviceProvider,
		string serviceKey,
		TestInstanceSettings settings) =>
		throw new NotSupportedException("Composition-only test double.");

}
