namespace Cirreum.ServiceProvider.Configuration;

using Cirreum.ServiceProvider;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for validating service provider instance settings.
/// </summary>
internal static class SettingValidationExtensions {

	/// <summary>
	/// Validates that the connection string is properly configured and unique across the
	/// service instances registered into <paramref name="services"/>. Uniqueness is scoped
	/// to the service collection — the state lives in the composition, not the process,
	/// so multiple hosts in one process are isolated.
	/// </summary>
	/// <param name="settings">The service provider instance settings to validate.</param>
	/// <param name="services">The service collection the instance is being registered into.</param>
	/// <param name="providerServiceType">The type of provider service.</param>
	/// <param name="providerServiceName">The name of the provider service.</param>
	/// <exception cref="InvalidOperationException">
	/// Thrown when the connection string is missing or when the same connection string
	/// is used by multiple service instances in this composition.
	/// </exception>
	public static void ValidateConnectionString(
		this ServiceProviderInstanceSettings settings,
		IServiceCollection services,
		ProviderType providerServiceType,
		string providerServiceName) {

		var connectionString = settings.ConnectionString ??
			throw new InvalidOperationException(
				$"The 'ConnectionString' is missing for service instance '{settings.Name}'");

		var fullConnectionString = settings.ConnectionStringDiscriminator() is { Length: > 0 } discriminator
			? $"{connectionString}__{discriminator}"
			: connectionString;

		var connectionHash = GetConnectionHash(fullConnectionString) ??
			throw new InvalidOperationException(
				"A service could not be configured. Unable to resolve a 'ConnectionString'");

		var connectionKey = $"Cirreum.{providerServiceType}.{providerServiceName}.Connections:{connectionHash}";

		if (services.Any(d => d.ImplementationInstance is ProcessedRegistrationKey processed
			&& processed.Value == connectionKey)) {
			throw new InvalidOperationException(
				$"A connection string for service instance '{settings.Name}' has already been configured. Cannot register the same connection with multiple instances.");
		}
		services.AddSingleton(new ProcessedRegistrationKey(connectionKey));

	}

	/// <summary>
	/// Generates a SHA256 hash of the connection string for unique identification.
	/// </summary>
	/// <param name="connectionString">The connection string to hash.</param>
	/// <returns>A Base64-encoded SHA256 hash of the connection string.</returns>
	private static string GetConnectionHash(string connectionString) {
		// Use a hash function to create a consistent, unique identifier
		var bytes = System.Text.Encoding.UTF8.GetBytes(connectionString);
		var hash = System.Security.Cryptography.SHA256.HashData(bytes);
		return Convert.ToBase64String(hash);
	}

}
