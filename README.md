# Cirreum.ServiceProvider

[![NuGet Version](https://img.shields.io/nuget/v/Cirreum.ServiceProvider.svg?style=flat-square&labelColor=1F1F1F&color=003D8F)](https://www.nuget.org/packages/Cirreum.ServiceProvider/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Cirreum.ServiceProvider.svg?style=flat-square&labelColor=1F1F1F&color=003D8F)](https://www.nuget.org/packages/Cirreum.ServiceProvider/)
[![GitHub Release](https://img.shields.io/github/v/release/cirreum/Cirreum.Core?style=flat-square&labelColor=1F1F1F&color=FF3B2E)](https://github.com/cirreum/Cirreum.Core/releases)
[![License](https://img.shields.io/github/license/cirreum/Cirreum.Core?style=flat-square&labelColor=1F1F1F&color=F2F2F2)](https://github.com/cirreum/Cirreum.Core/blob/main/LICENSE)
[![.NET](https://img.shields.io/badge/.NET-10.0-003D8F?style=flat-square&labelColor=1F1F1F)](https://dotnet.microsoft.com/)

**Foundation library for building service providers with health checks and telemetry**

## Overview

**Cirreum.ServiceProvider** provides base classes and patterns for implementing service providers within the Cirreum ecosystem. This library standardizes service registration, health monitoring, and telemetry integration across all service provider implementations.

## Features

- **Standardized Service Registration**: Base classes for consistent DI container integration
- **Built-in Health Checks**: Automatic health monitoring with configurable options
- **OpenTelemetry Integration**: First-class support for distributed tracing
- **Multi-Instance Support**: Configure and manage multiple instances per provider type
- **Connection String Management**: Automatic validation and uniqueness enforcement
- **Type-Safe Configuration**: Generic constraints ensure compile-time configuration safety

## Core Components

### ServiceProviderRegistrar<TSettings, TInstanceSettings, THealthOptions>

Abstract base class that handles:
- Service registration with dependency injection
- Health check configuration and registration  
- OpenTelemetry tracing setup
- Connection string validation
- Instance management and duplicate prevention

### ServiceProviderSettings & ServiceProviderInstanceSettings

Configuration hierarchy providing:
- Provider-level settings with tracing controls
- Instance-specific connection and health check configuration
- Automatic connection string parsing and validation

### Health Check Integration

- Configurable health monitoring per instance
- Readiness and liveness check categorization
- Custom failure status and timeout configuration
- Automatic registration with .NET health check system

## Quick Start

```csharp
// Implement your service provider registrar
public class MyServiceProviderRegistrar 
    : ServiceProviderRegistrar<MySettings, MyInstanceSettings, MyHealthOptions>
{
    public override ProviderType ProviderType => ProviderType.Custom;
    public override string ProviderName => "MyService";
    public override string[] ActivitySourceNames => ["MyService.Activities"];
    
    protected override void AddServiceProviderInstance(
        IServiceCollection services, 
        string serviceKey, 
        MyInstanceSettings settings)
    {
        // Register your service implementation
        services.AddKeyedSingleton<IMyService>(serviceKey, 
            sp => new MyService(settings.ConnectionString));
    }
    
    protected override IServiceProviderHealthCheck<MyHealthOptions> CreateHealthCheck(
        IServiceProvider serviceProvider, 
        string serviceKey, 
        MyInstanceSettings settings)
    {
        return new MyServiceHealthCheck(serviceProvider, serviceKey, settings);
    }
}

// Configure and register
services.Configure<MySettings>(configuration.GetSection("MyService"));
services.AddSingleton<IProviderRegistrar<MySettings, MyInstanceSettings>, MyServiceProviderRegistrar>();
```

## Contribution Guidelines

1. **Be conservative with new abstractions**  
   The API surface must remain stable and meaningful.

2. **Limit dependency expansion**  
   Only add foundational, version-stable dependencies.

3. **Favor additive, non-breaking changes**  
   Breaking changes ripple through the entire ecosystem.

4. **Include thorough unit tests**  
   All primitives and patterns should be independently testable.

5. **Document architectural decisions**  
   Context and reasoning should be clear for future maintainers.

6. **Follow .NET conventions**  
   Use established patterns from Microsoft.Extensions.* libraries.

## Versioning

Cirreum.ServiceProvider follows [Semantic Versioning](https://semver.org/):

- **Major** - Breaking API changes
- **Minor** - New features, backward compatible
- **Patch** - Bug fixes, backward compatible

Given its foundational role, major version bumps are rare and carefully considered.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

**Cirreum Foundation Framework**  
*Layered simplicity for modern .NET*