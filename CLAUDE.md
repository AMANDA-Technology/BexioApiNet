# BexioApiNet

BexioApiNet is a .NET client library for the Bexio REST API (v3.0.0). It provides strongly-typed abstractions, domain models, and connector services to interact with Bexio's accounting, banking, and tax endpoints. The library simplifies API consumption by handling authentication, raw HTTP request execution, automatic pagination, and error serialization. It also includes an ASP.NET Core integration package to easily register the client via dependency injection.

## Tech Stack
- **Language**: C# 13
- **Framework**: .NET 9.0
- **Testing**: NUnit 4, Coverlet
- **Core Dependencies**: `Microsoft.Extensions.DependencyInjection` (for the AspNetCore package), `System.Text.Json`

## Solution Structure
The solution `BexioApiNet.sln` is organized into four projects:
- `src/BexioApiNet.Abstractions`: Contains the domain models (DTOs), interfaces, exceptions, and enums. It holds no implementation logic.
- `src/BexioApiNet`: The core implementation library. Contains `BexioApiClient`, `BexioConnectionHandler`, and the various connector services (e.g., `ManualEntryService`).
- `src/BexioApiNet.AspNetCore`: Provides `IServiceCollection` extension methods to register BexioApiNet in ASP.NET Core applications.
- `src/BexioApiNet.Tests`: Integration tests utilizing NUnit.

## Build Commands
- **Restore**: `dotnet restore`
- **Build**: `dotnet build`
- **Test**: `dotnet test` (Note: Tests require actual API credentials. See 'Constraints & Gotchas')
- **Pack**: `dotnet pack` (NuGet packages are automatically generated on build via `GeneratePackageOnBuild`)

## Key Conventions
- **ApiResult Wrapper**: All API calls return an `ApiResult<T>` (or `ApiResult`) instead of throwing exceptions on non-2xx status codes. This wrapper contains the `IsSuccess` boolean, `StatusCode`, `ApiError` details, `Data`, and extracted `ResponseHeaders`.
- **Connector Pattern**: API endpoints are grouped logically into namespaces (e.g., `Accounting`, `Banking`) and implemented as individual connector services inheriting from `ConnectorService`.
- **Dependency Injection**: The core `BexioApiClient` aggregates all connector services and is designed to be injected via `IBexioApiClient`.
- **Query Parameters**: Optional query parameters are wrapped in domain-specific parameter objects extending `QueryParameter` (e.g., `QueryParameterManualEntry`).

## Important File Locations
- **Entry Point (DI)**: `src/BexioApiNet.AspNetCore/BexioServiceCollection.cs`
- **Main Client Interface**: `src/BexioApiNet/Interfaces/IBexioApiClient.cs`
- **Connection Handler**: `src/BexioApiNet/Services/BexioConnectionHandler.cs` (Handles HTTP execution and pagination)
- **Domain Models**: `src/BexioApiNet.Abstractions/Models/` (Grouped by domain, e.g., `Accounting/ManualEntries/`)

## Constraints & Gotchas
- **Testing Requirements**: The `BexioApiNet.Tests` project does not mock the API. It makes live calls to Bexio. To run tests, you MUST set the environment variables `BexioApiNet__BaseUri` and `BexioApiNet__JwtToken`.
- **Pagination**: The `BexioConnectionHandler.FetchAll<T>` method automatically handles Bexio's offset-based pagination to retrieve all available records when requested.
- **HTTP Client Lifecycle**: `BexioConnectionHandler` manages its own `HttpClient` instance. Ensure proper disposal of `IBexioApiClient` or rely on the DI container's scoped lifecycle management.
