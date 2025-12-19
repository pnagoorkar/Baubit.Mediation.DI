# Baubit.Mediation.DI

[![CircleCI](https://dl.circleci.com/status-badge/img/circleci/TpM4QUH8Djox7cjDaNpup5/2zTgJzKbD2m3nXCf5LKvqS/tree/master.svg?style=svg)](https://dl.circleci.com/status-badge/redirect/circleci/TpM4QUH8Djox7cjDaNpup5/2zTgJzKbD2m3nXCf5LKvqS/tree/master)
[![codecov](https://codecov.io/gh/pnagoorkar/Baubit.Mediation.DI/branch/master/graph/badge.svg)](https://codecov.io/gh/pnagoorkar/Baubit.Mediation.DI)<br/>
[![NuGet](https://img.shields.io/nuget/v/Baubit.Mediation.DI.svg)](https://www.nuget.org/packages/Baubit.Mediation.DI/)
[![NuGet](https://img.shields.io/nuget/dt/Baubit.Mediation.DI.svg)](https://www.nuget.org/packages/Baubit.Mediation.DI) <br/>
![.NET Standard 2.0](https://img.shields.io/badge/.NET%20Standard-2.0-512BD4?logo=dotnet&logoColor=white)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)<br/>
[![Known Vulnerabilities](https://snyk.io/test/github/pnagoorkar/Baubit.Mediation.DI/badge.svg)](https://snyk.io/test/github/pnagoorkar/Baubit.Mediation.DI)

## Overview

DI extension for [Baubit.Mediation](https://github.com/pnagoorkar/Baubit.Mediation). Enables registration of IMediator instances with configurable service lifetimes and keyed services support.

## Installation

```bash
dotnet add package Baubit.Mediation.DI
```

Or via NuGet Package Manager:

```
Install-Package Baubit.Mediation.DI
```

## Quick Start

Baubit.Mediation.DI supports three patterns for module loading, consistent with [Baubit.DI](https://github.com/pnagoorkar/Baubit.DI).

> **Critical:** If you're creating your own modules or using configuration-based loading, you **MUST** call `Register()` on your module registry before any module loading operations. See [Module Registration](#module-registration) below.

### Pattern 1: Modules from appsettings.json

Load mediator configuration from JSON using secure module keys.

```csharp
// IMPORTANT: Register your module registry first (if you have custom modules)
// MyModuleRegistry.Register();

await Host.CreateApplicationBuilder()
          .UseConfiguredServiceProviderFactory()
          .Build()
          .RunAsync();
```

**appsettings.json:**
```json
{
  "modules": [
    {
      "key": "baubit-mediation",
      "configuration": {
        "serviceLifetime": "Singleton"
      }
    }
  ]
}
```

**Security:** Configuration uses simple string keys (defined in `[BaubitModule]` attribute), not assembly-qualified type names, eliminating remote code execution vulnerabilities.

### Pattern 2: Modules from Code (IComponent)

Load mediator programmatically using `IComponent`. No configuration file needed.

```csharp
// IMPORTANT: Register your module registry first (if you have custom modules)
// MyModuleRegistry.Register();

public class AppComponent : Component
{
    protected override Result<ComponentBuilder> Build(ComponentBuilder builder)
    {
        return builder.WithModule<Module, Configuration>(config =>
        {
            config.ServiceLifetime = ServiceLifetime.Singleton;
        });
    }
}

await Host.CreateEmptyApplicationBuilder(new HostApplicationBuilderSettings())
          .UseConfiguredServiceProviderFactory(componentsFactory: () => [new AppComponent()])
          .Build()
          .RunAsync();
```

### Pattern 3: Hybrid Loading (appsettings.json + IComponent)

Combine configuration-based and code-based module loading.

```csharp
// IMPORTANT: Register your module registry first (if you have custom modules)
// MyModuleRegistry.Register();

await Host.CreateApplicationBuilder()
          .UseConfiguredServiceProviderFactory(componentsFactory: () => [new AppComponent()])
          .Build()
          .RunAsync();
```

**Loading order:**
1. Components from `componentsFactory` are loaded first
2. Modules from appsettings.json `modules` section are loaded second

### Using AddModule Directly

For direct service collection usage without Host:

```csharp
var services = new ServiceCollection();

// Add mediator using AddModule extension
services.AddModule<Module, Configuration>(config =>
{
    config.ServiceLifetime = ServiceLifetime.Singleton;
});

var serviceProvider = services.BuildServiceProvider();
var mediator = serviceProvider.GetRequiredService<IMediator>();
```

## Module Registration

> **Critical:** If you're using configuration-based module loading (Pattern 1 or Pattern 3) with custom modules in your own projects, you **MUST** call `Register()` on your module registry before calling `UseConfiguredServiceProviderFactory()`. This is a common source of frustration - don't skip this step!

Baubit.Mediation.DI's module is already registered in the Baubit.DI ecosystem via the `[BaubitModule("baubit-mediation")]` attribute. However, if you're creating your own modules in your application, you need to register them.

### For Consumer Projects with Custom Modules

If you're building an application or library with your own modules:

**Step 1: Create a Registry Class**

```csharp
using Baubit.DI;

namespace MyProject
{
    [GeneratedModuleRegistry]
    internal static partial class MyModuleRegistry
    {
        // Register() method will be generated automatically
    }
}
```

**Step 2: Define Your Custom Modules**

```csharp
[BaubitModule("my-custom-module")]
public class MyCustomModule : Module<MyConfig>
{
    public MyCustomModule(IConfiguration config) : base(config) { }
    
    public override void Load(IServiceCollection services)
    {
        services.AddSingleton<IMyService, MyService>();
    }
}
```

**Step 3: ALWAYS Call Register() Before Module Loading**

```csharp
// THIS IS CRITICAL - Call before UseConfiguredServiceProviderFactory()
MyModuleRegistry.Register();

await Host.CreateApplicationBuilder()
          .UseConfiguredServiceProviderFactory()
          .Build()
          .RunAsync();
```

**Why this is required:**
- The main `Baubit.DI.ModuleRegistry` only knows about modules in Baubit assemblies
- Consumer assemblies must explicitly register their modules with `ModuleRegistry.RegisterExternal()`
- The generated `Register()` method does this automatically
- Registration must happen before `UseConfiguredServiceProviderFactory()` initializes the module system
- **Forgetting this step will cause "module not found" errors**

### For Simple Applications Using Only Baubit Modules

If you're only using Baubit.Mediation.DI and other Baubit modules without creating your own custom modules, you don't need to create a registry - the modules are already registered in the Baubit ecosystem.

## Features

- **Flexible Service Lifetimes**: Register IMediator as Singleton, Transient, or Scoped
- **Keyed Services**: Support for keyed service registration and resolution
- **Cache Integration**: Configurable cache resolution with optional keyed services
- **Type-Safe Configuration**: Strongly-typed configuration with sensible defaults
- **.NET Standard 2.0**: Compatible with .NET Framework, .NET Core, and .NET 5+

## API Reference

### Configuration

Configuration class for the Mediation DI module:

```csharp
public class Configuration : Baubit.DI.Configuration
{
    // Optional key for resolving IOrderedCache<object>. Null resolves unkeyed service.
    public string CacheRegistrationKey { get; set; } = null;

    // Optional key for registering IMediator. Null registers without a key.
    public string RegistrationKey { get; set; } = null;

    // Service lifetime for IMediator. Defaults to Singleton.
    public ServiceLifetime ServiceLifetime { get; set; } = ServiceLifetime.Singleton;
}
```

### Module

DI module for registering IMediator:

```csharp
public class Module : Baubit.DI.Module<Configuration>
{
    public Module(IConfiguration configuration);
    public Module(Configuration configuration, List<IModule> nestedModules = null);
    public override void Load(IServiceCollection services);
}
```

### Usage Examples

**Singleton Registration (Default):**

```csharp
services.AddModule<Module, Configuration>(config =>
{
    config.ServiceLifetime = ServiceLifetime.Singleton;
});

var mediator = serviceProvider.GetRequiredService<IMediator>();
```

**Transient Registration:**

```csharp
services.AddModule<Module, Configuration>(config =>
{
    config.ServiceLifetime = ServiceLifetime.Transient;
});

// Each resolution creates a new instance
var mediator1 = serviceProvider.GetRequiredService<IMediator>();
var mediator2 = serviceProvider.GetRequiredService<IMediator>();
```

**Scoped Registration:**

```csharp
services.AddModule<Module, Configuration>(config =>
{
    config.ServiceLifetime = ServiceLifetime.Scoped;
});

using (var scope = serviceProvider.CreateScope())
{
    var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
}
```

**Keyed Service Registration:**

```csharp
services.AddModule<Module, Configuration>(config =>
{
    config.RegistrationKey = "my-mediator";
    config.ServiceLifetime = ServiceLifetime.Singleton;
});

var mediator = serviceProvider.GetRequiredKeyedService<IMediator>("my-mediator");
```

**Keyed Cache Resolution:**

```csharp
// Register cache with a key
services.AddKeyedSingleton<IOrderedCache<object>>("my-cache", /* implementation */);

// Configure module to use keyed cache
services.AddModule<Module, Configuration>(config =>
{
    config.CacheRegistrationKey = "my-cache";
});
```

## Contributing

Contributions are welcome. Open an issue or submit a pull request.

## License

MIT License - see [LICENSE](LICENSE) file for details.
