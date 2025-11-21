namespace Cirreum.ServiceProvider.Configuration;

using System.Collections.Generic;

/// <summary>
/// Extension methods for validating service provider instance settings.
/// </summary>
internal static class SettingValidationExtensions {

	private static readonly Dictionary<string, string> processedConnectionStrings = [];

	/// <summary>
	/// Validates that the connection string is properly configured and unique across service instances.
	/// </summary>
	/// <param name="settings">The service provider instance settings to validate.</param>
	/// <param name="providerServiceType">The type of provider service.</param>
	/// <param name="providerServiceName">The name of the provider service.</param>
	/// <exception cref="InvalidOperationException">
	/// Thrown when the connection string is missing or when the same connection string 
	/// is used by multiple service instances.
	/// </exception>
	public static void ValidateConnectionString(
		this ServiceProviderInstanceSettings settings,
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

		if (!processedConnectionStrings.TryAdd(connectionKey, settings.Name)) {
			throw new InvalidOperationException(
				$"A connection string for service instance '{settings.Name}' has already been configured. Cannot register the same connection with multiple instances.");
		}

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