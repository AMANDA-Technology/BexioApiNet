# BexioApiNet

Unofficial .NET 10 API client library for the [Bexio v3 REST API](https://docs.bexio.com/) (version 3.0.0). Provides a typed C# client with domain models, `ApiResult<T>` error handling, automatic pagination, and ASP.NET Core DI integration.

[Bexio](https://www.bexio.com/) is a Swiss cloud business platform for accounting, invoicing, and banking.

> Work in progress!

[![BuildNuGetAndPublish](https://github.com/AMANDA-Technology/BexioApiNet/actions/workflows/main.yml/badge.svg)](https://github.com/AMANDA-Technology/BexioApiNet/actions/workflows/main.yml)
[![PR CI](https://github.com/AMANDA-Technology/BexioApiNet/actions/workflows/pr.yml/badge.svg)](https://github.com/AMANDA-Technology/BexioApiNet/actions/workflows/pr.yml)
[![CodeQL](https://github.com/AMANDA-Technology/BexioApiNet/actions/workflows/codeql-analysis.yml/badge.svg)](https://github.com/AMANDA-Technology/BexioApiNet/actions/workflows/codeql-analysis.yml)
[![SonarCloud](https://github.com/AMANDA-Technology/BexioApiNet/actions/workflows/sonar-analysis.yml/badge.svg)](https://github.com/AMANDA-Technology/BexioApiNet/actions/workflows/sonar-analysis.yml)

## Packages

| Package | Description |
|---------|-------------|
| [BexioApiNet](https://www.nuget.org/packages/BexioApiNet/) | Client, connection handler, and connector services |
| [BexioApiNet.Abstractions](https://www.nuget.org/packages/BexioApiNet.Abstractions/) | Models, enums, interfaces |
| [BexioApiNet.AspNetCore](https://www.nuget.org/packages/BexioApiNet.AspNetCore/) | ASP.NET Core dependency injection registration |

## Getting Started

### Installation

```bash
# ASP.NET Core (pulls in the core packages)
dotnet add package BexioApiNet.AspNetCore
```

### Authentication

Bexio uses JWT bearer tokens — see [JWT authentication](https://docs.bexio.com/#section/Authentication/JWT-(JSON-Web-Tokens)) for how to obtain one, and [API routes](https://docs.bexio.com/#section/API-basics/API-routes) for the correct base URI.

### ASP.NET Core (DI)

Register the client in `Program.cs`:

```csharp
builder.Services.AddBexioServices(
    baseUri: builder.Configuration["BexioApiNet:BaseUri"]!,
    jwtToken: builder.Configuration["BexioApiNet:JwtToken"]!);
```

Then inject `IBexioApiClient` wherever you need it:

```csharp
public class ContactsController(IBexioApiClient bexio) : ControllerBase
{
    [HttpGet]
    public async Task<IReadOnlyList<Contact>> GetAll(CancellationToken ct)
    {
        var result = await bexio.Contacts.Get(autoPage: true, cancellationToken: ct);
        return result.IsSuccess ? result.Data ?? [] : [];
    }
}
```

## Usage Examples

### List with auto-pagination

```csharp
// Fetches every page via Bexio's X-Total-Count header
var result = await bexio.Contacts.Get(autoPage: true);
foreach (var c in result.Data ?? [])
    Console.WriteLine($"{c.Nr} - {c.Name1}");
```

### Create

```csharp
var create = new ContactCreate(
    ContactTypeId: 2, Name1: "Doe", UserId: 1, OwnerId: 1, Name2: "Jane");

var result = await bexio.Contacts.Create(create);
if (result.IsSuccess)
    Console.WriteLine($"Created contact #{result.Data!.Id}");
```

## Result Handling

All API calls return `ApiResult<T>` — no exceptions are thrown for non-2xx responses. Inspect `IsSuccess`, `StatusCode`, `ApiError`, and `Data`:

```csharp
var result = await bexio.Contacts.GetById(id: 42);

if (!result.IsSuccess)
{
    Console.WriteLine($"{(int)result.StatusCode}: {result.ApiError?.Message}");
    return;
}

var contact = result.Data!;
```

## License

[MIT](LICENSE)
