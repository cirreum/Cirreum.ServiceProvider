# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is **Cirreum.ServiceProvider**, a foundational .NET 10 library that provides base classes and patterns for implementing service providers with built-in health checks, OpenTelemetry tracing, and dependency injection integration.

## Architecture

### Core Components

- **ServiceProviderRegistrar**: Base abstract class for registering service providers with DI container
  - Handles multiple instances per provider type
  - Automatic health check registration when enabled
  - OpenTelemetry tracing integration
  - Connection string parsing and validation

- **ServiceProviderSettings**: Configuration hierarchy
  - `ServiceProviderSettings<TInstanceSettings, THealthOptions>`: Root settings with tracing control
  - `ServiceProviderInstanceSettings<THealthOptions>`: Per-instance configuration with connection strings and health options

- **Health Check Integration**: Built-in health monitoring
  - Configurable health check options per instance
  - Readiness and liveness check support
  - Automatic registration with .NET health check system

### Key Patterns

1. **Generic Type Constraints**: Heavy use of generic constraints to ensure type safety across the provider hierarchy
2. **Instance Management**: Dictionary-based instance tracking to prevent duplicate registrations
3. **Connection String Management**: Automatic resolution from configuration or connection strings section
4. **Health Check Registration**: Conditional registration based on settings with tag-based categorization

## Common Development Commands

Since this is a .NET 10 library project, use standard .NET CLI commands:

```bash
# Build the project
dotnet build

# Run tests (if test project exists)
dotnet test

# Pack NuGet package
dotnet pack

# Restore dependencies
dotnet restore
```

## Build Configuration

- **Target Framework**: .NET 10.0
- **Language Version**: Latest C#
- **Nullable Reference Types**: Enabled
- **Implicit Usings**: Enabled
- **Documentation Generation**: Enabled

Key build properties are centralized in:
- `src/Directory.Build.props`: CI/CD detection, versioning, and common imports
- `build/Common.props`: Target framework and language settings
- `build/*.props`: Author info, icons, source linking, and package metadata

## Dependencies

- **Microsoft.Extensions.Caching.Memory**: Memory caching support
- **Microsoft.Extensions.Logging.Configuration**: Logging configuration
- **Microsoft.Extensions.Diagnostics.HealthChecks**: Health check framework
- **OpenTelemetry.Extensions.Hosting**: Telemetry integration
- **Cirreum.Providers**: Parent provider framework

## Development Guidelines

The README emphasizes conservative development practices:
- Minimal new abstractions to maintain API stability
- Limited dependency expansion (foundational, version-stable only)
- Favor additive, non-breaking changes
- Include thorough unit tests
- Document architectural decisions
- Follow .NET conventions from Microsoft.Extensions.*