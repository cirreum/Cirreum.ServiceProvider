# Cirreum.ServiceProvider Changelog

All notable changes to **Cirreum.ServiceProvider** are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Fixed

- The registrar's duplicate-registration guards were process-global static state
  (instance keys, health-check names, and connection-string hashes): a second host
  composed in the same process (the integration-test norm) was rejected for
  re-using an instance key or connection string its own composition had never
  seen — and, worse, silently received **no health-check registrations** at all,
  because the health-check dedup believed the first host's registrations covered
  the process. All guard state now lives in the service collection, so hosts are
  fully isolated (ADR-0028 principle; same defect class fixed in
  `Cirreum.AuthenticationProvider` 1.3.0 and `Cirreum.IdentityProvider` 1.0.8).
  The internal `ValidateConnectionString` extension gains an `IServiceCollection`
  parameter to carry the composition scope.
- First test suite (`tests/Cirreum.ServiceProvider.Tests.slnx`): the registration
  guards, cross-composition isolation, and per-composition health-check
  registration.

## [1.0.22] - 2026-07-18

### Updated

- Updated NuGet packages.

## [1.0.21] - 2026-07-04

### Updated

- Updated NuGet packages.

## [1.0.20] - 2026-07-04

### Updated

- Updated NuGet packages.

## [1.0.19] - 2026-06-05

### Fixed

- Updated dependencies to their latest versions to pick up upstream fixes:
  - `Cirreum.Providers` `1.1.1` → `1.2.1`
  - `Microsoft.Extensions.Caching.Memory` `10.0.7` → `10.0.8`
  - `Microsoft.Extensions.Diagnostics.HealthChecks` `10.0.7` → `10.0.8`
  - `Microsoft.Extensions.Logging.Configuration` `10.0.7` → `10.0.8`

## [1.0.18]

Baseline entry. Prior history predates this changelog.
