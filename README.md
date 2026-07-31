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

Bexio authenticates with bearer tokens — see [Authentication](https://docs.bexio.com/#section/Authentication) for how to obtain one, and [API routes](https://docs.bexio.com/#section/API-basics/API-routes) for the correct base URI. The token is resolved per request from an `IBexioTokenProvider`, so a short-lived token is renewed without recreating the HTTP client.

| Mode | Registration | Use when |
|------|--------------|----------|
| OIDC authorization code + `offline_access` | `AddBexioServicesWithRefreshToken(...)` | **The supported path for unattended integrations.** Needs one-time user consent and a refresh token store. |
| Personal Access Token | `AddBexioServices(baseUri, jwtToken)` | Scripts and local development. Expires after 60 days and bexio documents it as strictly personal — not a fit for a server. |
| OIDC `client_credentials` | `AddBexioServicesWithClientCredentials(...)` | **Unproven against bexio.** Never mentioned in their docs, and their permission model grants API access with the rights of the user who set up the connection — a token with no user behind it may mint and still be rejected. See [`doc/analysis/api-doc-discrepancies.md`](doc/analysis/api-doc-discrepancies.md). |

#### Personal Access Token

```csharp
builder.Services.AddBexioServices(
    baseUri: builder.Configuration["BexioApiNet:BaseUri"]!,
    jwtToken: builder.Configuration["BexioApiNet:JwtToken"]!);
```

#### OIDC with refresh token rotation

Register your own refresh token store — the library ships the interface only, because storage is an application decision:

```csharp
builder.Services.AddScoped<IBexioRefreshTokenStore, MyRefreshTokenStore>();

builder.Services.AddBexioServicesWithRefreshToken(
    new BexioConfiguration
    {
        BaseUri = builder.Configuration["BexioApiNet:BaseUri"]!,
        AcceptHeaderFormat = ApiAcceptHeaders.JsonFormatted
    },
    new BexioOAuthOptions
    {
        ClientId = builder.Configuration["Bexio:ClientId"]!,
        ClientSecret = builder.Configuration["Bexio:ClientSecret"],
        RedirectUri = "https://myapp.example/bexio/callback",
        Scopes = ["accounting", BexioAuthDefaults.OfflineAccessScope]
    });
```

Bootstrap it once through the consent flow. `BexioAuthorizeUrlBuilder` builds the redirect, and `IBexioTokenClient` exchanges the code your callback receives:

```csharp
// 1. send the user here
var consentUrl = BexioAuthorizeUrlBuilder.Build(oauthOptions, state: antiForgeryToken);

// 2. in the callback, exchange the code and persist the refresh token
var tokens = await tokenClient.ExchangeAuthorizationCodeAsync(code, cancellationToken: ct);
await store.StoreRefreshTokenAsync(tokens.RefreshToken!, ct);
```

From then on the provider renews access tokens on its own. When a renewal returns a **rotated** refresh token, it is handed to `StoreRefreshTokenAsync` and the renewal only counts as successful once that write completes; when it returns none, the stored token is kept. Make the write durable: bexio invalidates the previous token as soon as it issues a replacement, so a lost write leaves no usable credential and the customer has to consent again.

Two things the flow does not do, by design: scopes are fixed at consent time and cannot be widened on refresh, and offline sessions idle out after a year of no use.

If the token endpoint answers `401` for an otherwise correct request, set `ClientAuthenticationMethod = BexioClientAuthenticationMethod.ClientSecretBasic` — bexio does not document which method portal-registered apps are configured for.

#### OIDC with client credentials

```csharp
builder.Services.AddBexioServicesWithClientCredentials(
    bexioConfiguration,
    new BexioOAuthOptions
    {
        ClientId = builder.Configuration["Bexio:ClientId"]!,
        ClientSecret = builder.Configuration["Bexio:ClientSecret"]!,
        Scopes = ["accounting"]
    });
```

Token endpoint failures throw `BexioAuthenticationException` rather than returning an `ApiResult` — there is no API request to attach a result to. Check `IsInvalidGrant` to tell "the customer must consent again" apart from a transient failure worth retrying.

`IBexioTokenClient.RevokeTokenAsync` revokes an access or refresh token for clean teardown when an integration is disconnected.

### ASP.NET Core (DI)

After registering, inject `IBexioApiClient` wherever you need it:

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
