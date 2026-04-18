---
title: Container Architecture
tags: [architecture, c4, containers]
---

# Container Architecture: BexioApiNet

The BexioApiNet solution is partitioned into separate NuGet packages to minimize dependencies and strictly separate domain definitions from ASP.NET Core integrations.

## C4 Container Diagram

```mermaid
C4Container
  title Container diagram for BexioApiNet

  Person(developer, "Developer / .NET App", "Consumes the library")

  System_Boundary(bexioApiNet, "BexioApiNet SDK") {
    Container(abstractions, "BexioApiNet.Abstractions", "NuGet Package (.NET 9)", "Provides interfaces, domain models (DTOs), and enums. No external dependencies.")
    Container(core, "BexioApiNet", "NuGet Package (.NET 9)", "Provides core API client implementation, connectors, and HTTP handling.")
    Container(aspnetcore, "BexioApiNet.AspNetCore", "NuGet Package (.NET 9)", "Provides IServiceCollection extensions for Microsoft.Extensions.DependencyInjection.")
  }

  System_Ext(bexioApi, "Bexio REST API", "Upstream API")

  Rel(developer, aspnetcore, "Registers services via", "C#")
  Rel(developer, abstractions, "Uses models from", "C#")
  Rel(developer, core, "Injects IBexioApiClient from", "C#")

  Rel(aspnetcore, core, "References")
  Rel(aspnetcore, abstractions, "References")
  Rel(core, abstractions, "References")

  Rel(core, bexioApi, "Makes REST requests", "HTTPS/JSON")
```

## Deployable Units

| Container | Path | Responsibility |
|-----------|------|----------------|
| **BexioApiNet.Abstractions** | `src/BexioApiNet.Abstractions/` | Defines the contract. Contains all domain models mapped to the Bexio API schema, enums (like `ApiResponseCodes`), exceptions, and connector interfaces. |
| **BexioApiNet** | `src/BexioApiNet/` | The actual implementation. Contains the `BexioConnectionHandler` (HTTP client logic) and service implementations (e.g., `ManualEntryService`, `AccountService`). |
| **BexioApiNet.AspNetCore** | `src/BexioApiNet.AspNetCore/` | A lightweight wrapper adding ASP.NET Core dependency injection compatibility (`AddBexioServices()`). |
| **BexioApiNet.Tests** | `src/BexioApiNet.Tests/` | *Not deployed.* NUnit integration tests validating real API calls against Bexio. |
