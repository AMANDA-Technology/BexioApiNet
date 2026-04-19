# BexioApiNet

BexioApiNet is a .NET client library for the Bexio REST API (v3.0.0). It provides strongly-typed abstractions, domain models, and connector services to interact with Bexio's accounting, banking, and tax endpoints. The library simplifies API consumption by handling authentication, raw HTTP request execution, automatic pagination, and error serialization. It also includes an ASP.NET Core integration package to easily register the client via dependency injection.

> **AI agents:** strict procedural rules live in [`ai_instructions.md`](./ai_instructions.md). That file is the contract for agent behavior and overrides this file where they disagree.
>
> **Contributors adding endpoints:** follow [`doc/development/feature-addition-guide.md`](./doc/development/feature-addition-guide.md).
>
> **Contributors writing tests:** follow [`doc/development/testing-guide.md`](./doc/development/testing-guide.md).

## Tech Stack
- **Language**: C# 13
- **Framework**: .NET 9.0
- **Testing**: NUnit 4, Coverlet
- **Core Dependencies**: `Microsoft.Extensions.DependencyInjection`, `Microsoft.Extensions.Http` (for the AspNetCore package), `System.Text.Json`

## Solution Structure
The solution `BexioApiNet.sln` is organized into projects under `src/` and `tests/`:
- `src/BexioApiNet.Abstractions`: Contains the domain models (DTOs), interfaces, exceptions, and enums. It holds no implementation logic.
- `src/BexioApiNet`: The core implementation library. Contains `BexioApiClient`, `BexioConnectionHandler`, and the various connector services (e.g., `ManualEntryService`).
- `src/BexioApiNet.AspNetCore`: Provides `IServiceCollection` extension methods to register BexioApiNet in ASP.NET Core applications (using typed `HttpClient` via `IHttpClientFactory`).
- `tests/BexioApiNet.UnitTests`: NUnit 4 unit tests (offline, mocked dependencies).
- `tests/BexioApiNet.IntegrationTests`: NUnit 4 integration tests (offline, WireMock.Net stubs).
- `tests/BexioApiNet.E2eTests`: NUnit 4 live tests (requires credentials).

## Build Commands
- **Restore**: `dotnet restore`
- **Build**: `dotnet build`
- **Unit tests (offline, no creds)**: `dotnet test --filter TestCategory=Unit`
- **Integration tests (offline, no creds)**: `dotnet test --filter TestCategory=Integration`
- **Live E2E tests**: `dotnet test --filter TestCategory=E2E` (requires `BexioApiNet__BaseUri` + `BexioApiNet__JwtToken`)
- **Skip live tests in CI**: `dotnet test --filter TestCategory!=E2E` (runs Unit + Integration)
- **Pack**: `dotnet pack` (NuGet packages are automatically generated on build via `GeneratePackageOnBuild`)

## Key Conventions
- **ApiResult Wrapper**: All API calls return an `ApiResult<T>` (or `ApiResult`) instead of throwing exceptions on non-2xx status codes. This wrapper contains the `IsSuccess` boolean, `StatusCode`, `ApiError` details, `Data`, and extracted `ResponseHeaders`.
- **Connector Pattern**: API endpoints are grouped logically into namespaces (e.g., `Accounting`, `Banking`) and implemented as individual connector services inheriting from `ConnectorService`.
- **Dependency Injection**: The core `BexioApiClient` aggregates all connector services and is designed to be injected via `IBexioApiClient`.
- **Query Parameters**: Optional query parameters are wrapped in domain-specific parameter objects extending `QueryParameter` (e.g., `QueryParameterManualEntry`).
- **Typed HttpClient**: `BexioConnectionHandler` is registered as a typed client via `IHttpClientFactory` in `BexioServiceCollection.AddBexioServices` — no manual `HttpClient` instantiation in DI scenarios.

## Important File Locations
- **AI rules**: `ai_instructions.md`
- **Feature guide**: `doc/development/feature-addition-guide.md`
- **Testing guide**: `doc/development/testing-guide.md`
- **Entry Point (DI)**: `src/BexioApiNet.AspNetCore/BexioServiceCollection.cs`
- **Main Client Interface**: `src/BexioApiNet/Interfaces/IBexioApiClient.cs`
- **Connection Handler**: `src/BexioApiNet/Services/BexioConnectionHandler.cs` (Handles HTTP execution and pagination; dual constructor — owns or borrows its `HttpClient`)
- **Domain Models**: `src/BexioApiNet.Abstractions/Models/` (Grouped by domain, e.g., `Accounting/ManualEntries/`)

## Constraints & Gotchas
- **Testing Requirements**: Live E2E tests (`Tests/`) call the real Bexio API. They require `BexioApiNet__BaseUri` and `BexioApiNet__JwtToken` env vars. When missing, `TestBase` calls `Assert.Ignore` — tests are skipped, **not** failed. Offline unit tests (`UnitTests/`) run anywhere without credentials and are mandatory for every new connector method.
- **Pagination**: The `BexioConnectionHandler.FetchAll<T>` method automatically handles Bexio's offset-based pagination to retrieve all available records when requested.
- **HTTP Client Lifecycle**: In ASP.NET Core, `BexioConnectionHandler` is resolved through `IHttpClientFactory` (typed client) and does **not** dispose the injected `HttpClient`. Non-DI consumers can still construct it with a configuration only — in that path it owns and disposes its own client.
