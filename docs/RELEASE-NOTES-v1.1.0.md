# Cirreum.ServiceProvider 1.1.0 — Identifier and the Credential Block on the Instance Base

## Why this release exists

Service provider implementations that authenticate with a platform identity have had no
configuration surface for *which* identity, *which* tenant, or *which* strategy — the underlying
clients were constructed with a bare platform-default credential. `Cirreum.Providers` 1.3.0
introduced a vendor-neutral credential vocabulary (`CredentialMode` + `CredentialSettings`); this
release surfaces it on the service instance base, alongside the `Identifier` property the secrets
family has always had, so both provider families now tell one configuration story.

## What's new

`ServiceProviderInstanceSettings` gains two properties:

```csharp
public string? Identifier { get; set; }
public CredentialSettings? Credential { get; set; }
```

configured alongside the existing connection value:

```json
"Instances": {
  "default": {
    "ConnectionString": "<connection string or endpoint uri>",
    "Identifier": "<optional provider-interpreted id>",
    "Credential": { "Mode": "ManagedIdentity", "IdentityId": "<platform identity id>" }
  }
}
```

**`Identifier`** is "an Id associated with the provider, if required" — the same wording, and the
same convention, as the secrets family. Implementations document their interpretation; Azure
providers resolve it as the Entra tenant.

**`Credential`** selects `Default` (the platform's default credential chain), `ManagedIdentity`
(the platform-assigned workload identity, deterministic), or `Developer` (developer tooling
credentials only), with `IdentityId` selecting a specific platform identity when the host has
several. It is nullable by design: absence means default credential behavior, and presence lets an
implementation detect contradictory configuration — such as a credential block alongside key-based
authentication — and fail fast.

## Compatibility

Purely additive — no existing property, method, or configuration key changes. The surface takes
effect per implementation as each adopts it: the Azure Cosmos DB persistence provider is first,
followed by the Storage, Messaging, and Communications Azure providers.

## See also

- [CHANGELOG](CHANGELOG.md)
- [Cirreum.Providers — Credential Configuration](https://github.com/cirreum/Cirreum.Providers#credential-configuration)
