namespace Cirreum.ServiceProvider;

/// <summary>
/// Service-collection-scoped marker recording that a registration key (a provider
/// instance key, a health-check name, or a connection-string hash) has been processed.
/// The registrar and settings validation check for an existing marker to reject
/// duplicates within one composition; markers live and die with their service
/// collection, so multiple hosts in one process never cross-contaminate.
/// </summary>
/// <param name="Value">The fully-qualified registration key.</param>
internal sealed record ProcessedRegistrationKey(string Value);
