---
title: Feature Addition Guide — Adding a Bexio Endpoint
tags: [development, guide, how-to, contributing]
---

# Feature Addition Guide — Adding a Bexio Endpoint

This guide defines the **strict, reproducible, 5-step process** for adding a new Bexio API endpoint to BexioApiNet. Every new endpoint must follow this process so the library remains 1:1 with the Bexio REST API documentation.

**Source of truth for every endpoint, field and parameter:** <https://docs.bexio.com/>.

Before starting, make sure you have read:
- [`ai_instructions.md`](../../ai_instructions.md) — agent rules.
- [`testing-guide.md`](./testing-guide.md) — how to test what you add.
- Existing reference implementation: `ManualEntryService` (most complete example — GET, POST, DELETE, multipart upload, pagination).

## Worked Example Used Below

Throughout this guide we use an imaginary endpoint to make the steps concrete:

- **Domain:** `Contacts`
- **Entity:** `Contact`
- **Bexio docs tag:** `Contacts` (<https://docs.bexio.com/#tag/Contacts>)
- **Routes:** `GET /2.0/contact`, `POST /2.0/contact`, `GET /2.0/contact/{id}`, `DELETE /2.0/contact/{id}`

Adjust names for your actual endpoint — do not copy these literally.

---

## Step 1 — Domain Models

**Where:** `src/BexioApiNet.Abstractions/Models/<Domain>/<Subdomain>/`

**What to create:**
1. A `record` per resource (e.g., `Contact.cs`).
2. A `Views/` subfolder with `<Entity>Create.cs` and `<Entity>Edit.cs` if the endpoint supports create/update.
3. An enum file (if the Bexio payload contains enum-like fields).

**Rules:**
- `public sealed record <Entity>` — never `class`.
- Mark `required` for fields Bexio always returns; use `init` accessors.
- Map every JSON field with `[JsonPropertyName("bexio_field_name")]`.
- Match Bexio's types exactly: `int` for numeric IDs, `decimal` for money, `DateOnly`/`DateTime` for dates, `string?` for nullable strings.
- XML `<summary>` on the record and every property (required — doc generation fails otherwise).
- If the response contains nested objects, model them as separate records in the same folder.

**Worked example:**

```csharp
// src/BexioApiNet.Abstractions/Models/Contacts/Contacts/Contact.cs
using System.Text.Json.Serialization;

namespace BexioApiNet.Abstractions.Models.Contacts.Contacts;

/// <summary>
/// Contact as returned by the Bexio contacts endpoint.
/// <see href="https://docs.bexio.com/#tag/Contacts/operation/v2ListContact"/>
/// </summary>
public sealed record Contact
{
    /// <summary>Unique contact identifier.</summary>
    [JsonPropertyName("id")]
    public required int Id { get; init; }

    /// <summary>Internal name / full name of the contact.</summary>
    [JsonPropertyName("name_1")]
    public required string Name1 { get; init; }

    /// <summary>Optional second name / suffix.</summary>
    [JsonPropertyName("name_2")]
    public string? Name2 { get; init; }
}
```

### Step 1b — Query parameters

**Where:** `src/BexioApiNet/Models/QueryParameter<Entity>.cs`

Every `GET` endpoint that accepts optional query parameters needs a typed wrapper extending `QueryParameter`. This keeps pagination and filter parameters strongly typed at the call site while keeping the handler generic.

```csharp
public sealed class QueryParameterContact : QueryParameter
{
    public QueryParameterContact(int? limit = null, int? offset = null, string? orderBy = null)
        : base(BuildParameters(limit, offset, orderBy))
    {
    }

    private static Dictionary<string, object> BuildParameters(int? limit, int? offset, string? orderBy)
    {
        var parameters = new Dictionary<string, object>();
        if (limit is { } l) parameters["limit"] = l;
        if (offset is { } o) parameters["offset"] = o;
        if (!string.IsNullOrWhiteSpace(orderBy)) parameters["order_by"] = orderBy;
        return parameters;
    }
}
```

---

## Step 2 — Interfaces

**Where:** `src/BexioApiNet/Interfaces/Connectors/<Domain>/I<Entity>Service.cs`

Define the public contract of the service. Only return `Task<ApiResult<T>>` — never raw `T` or `Task<T>`.

```csharp
// src/BexioApiNet/Interfaces/Connectors/Contacts/IContactService.cs
using System.Runtime.InteropServices;
using BexioApiNet.Abstractions.Models.Api;
using BexioApiNet.Abstractions.Models.Contacts.Contacts;
using BexioApiNet.Abstractions.Models.Contacts.Contacts.Views;
using BexioApiNet.Models;

namespace BexioApiNet.Interfaces.Connectors.Contacts;

/// <summary>
/// Bexio contacts connector. <see href="https://docs.bexio.com/#tag/Contacts"/>
/// </summary>
public interface IContactService
{
    /// <summary>List contacts. Set <paramref name="autoPage"/> to true to walk all pages.</summary>
    Task<ApiResult<List<Contact>?>> Get([Optional] QueryParameterContact? query, [Optional] bool autoPage, [Optional] CancellationToken cancellationToken);

    /// <summary>Create a contact.</summary>
    Task<ApiResult<Contact>> Create(ContactCreate payload, [Optional] CancellationToken cancellationToken);

    /// <summary>Delete a contact by id.</summary>
    Task<ApiResult<object>> Delete(int id, [Optional] CancellationToken cancellationToken);
}
```

---

## Step 3 — Implementation

**Where:** `src/BexioApiNet/Services/Connectors/<Domain>/<Entity>Service.cs` (+ `<Entity>Configuration.cs`).

1. Create the endpoint constants in a static `<Entity>Configuration` class:

```csharp
internal static class ContactConfiguration
{
    public const string ApiVersion = "2.0";
    public const string EndpointRoot = "contact";
}
```

2. Implement the service — inherit from `ConnectorService`, implement the interface, use the protected `ConnectionHandler` for all HTTP calls:

```csharp
public sealed class ContactService : ConnectorService, IContactService
{
    private const string ApiVersion = ContactConfiguration.ApiVersion;
    private const string EndpointRoot = ContactConfiguration.EndpointRoot;

    public ContactService(IBexioConnectionHandler bexioConnectionHandler) : base(bexioConnectionHandler) { }

    public async Task<ApiResult<List<Contact>?>> Get([Optional] QueryParameterContact? query, [Optional] bool autoPage, [Optional] CancellationToken cancellationToken)
    {
        var result = await ConnectionHandler.GetAsync<List<Contact>?>($"{ApiVersion}/{EndpointRoot}", query?.QueryParameter, cancellationToken);

        if (!autoPage || !result.IsSuccess || result.Data is null ||
            result.ResponseHeaders?.GetValueOrDefault(ApiHeaderNames.TotalResults) is not { } totalResults)
            return result;

        result.Data.AddRange(await ConnectionHandler.FetchAll<Contact>(
            result.Data.Count, totalResults, $"{ApiVersion}/{EndpointRoot}", query?.QueryParameter, cancellationToken));
        return result;
    }

    public async Task<ApiResult<Contact>> Create(ContactCreate payload, [Optional] CancellationToken cancellationToken)
        => await ConnectionHandler.PostAsync<Contact, ContactCreate>(payload, $"{ApiVersion}/{EndpointRoot}", cancellationToken);

    public async Task<ApiResult<object>> Delete(int id, [Optional] CancellationToken cancellationToken)
        => await ConnectionHandler.Delete($"{ApiVersion}/{EndpointRoot}/{id}", cancellationToken);
}
```

**Rules:**
- No direct `HttpClient` use. Everything goes through `ConnectionHandler`.
- Every method is `async`/`await` — no `.Result`/`.Wait()`.
- All routes are composed from the `<Entity>Configuration` constants, never inlined literals.
- `Get` follows the canonical pagination pattern: first call respects `queryParameter`, then `FetchAll` walks until `total_results`.

---

## Step 4 — DI Registration

Three places to edit:

1. **`src/BexioApiNet/Interfaces/IBexioApiClient.cs`** — add the property:
   ```csharp
   /// <summary>Bexio contacts connector. <see href="https://docs.bexio.com/#tag/Contacts"/></summary>
   public IContactService Contacts { get; set; }
   ```

2. **`src/BexioApiNet/Services/BexioApiClient.cs`** — add the property implementation and accept the service in the constructor:
   ```csharp
   public IContactService Contacts { get; set; }

   public BexioApiClient(
       IBexioConnectionHandler bexioConnectionHandler,
       /* existing parameters */
       IContactService contacts)
   {
       /* existing assignments */
       Contacts = contacts;
   }
   ```

3. **`src/BexioApiNet.AspNetCore/BexioServiceCollection.cs`** — register the service in `AddBexioServices`:
   ```csharp
   services.AddScoped<IContactService, ContactService>();
   ```
   Leave the existing `services.AddHttpClient<IBexioConnectionHandler, BexioConnectionHandler>(...)` block untouched — connector services do not need their own typed client.

---

## Step 5 — Testing

Every connector change ships with **two** test layers. Short version:

| Layer | Required? | Runs offline? | Location |
|-------|-----------|---------------|----------|
| Offline Unit Test | **Mandatory** | Yes — no creds needed | `src/BexioApiNet.Tests/UnitTests/<Domain>/...` |
| Live E2E Test | Optional but recommended | No — requires `BexioApiNet__BaseUri` + `BexioApiNet__JwtToken` | `src/BexioApiNet.Tests/Tests/<Domain>/...` |

**Offline unit test — minimum coverage per method:**
- Verifies the correct HTTP verb is used.
- Verifies the correct route is hit (including API version and id substitution).
- Verifies query parameters are serialized correctly.
- Verifies the response is deserialized into the correct model.

Mock `IBexioConnectionHandler` with NSubstitute (preferred) or wire a fake `HttpMessageHandler` into the real `BexioConnectionHandler` using its DI-friendly constructor.

**Live E2E test — requirements:**
- Inherits from `TestBase` (which is already categorized `[Category("E2E")]` and skips via `Assert.Ignore` if credentials are missing).
- Creates, asserts, and **cleans up** test data. No orphan records left in the test tenant.
- Prefix created test data with `"E2E-"` so manual inspection in the Bexio UI shows what is safe to delete.

**See** [`testing-guide.md`](./testing-guide.md) **for the full rules, example code, and the NSubstitute / WireMock.Net patterns.**

---

## Pre-Commit Checklist

Before opening a PR / finishing the task:

- [ ] Bexio docs URL referenced in at least one XML doc comment on the new interface.
- [ ] `dotnet restore && dotnet build` → **0 errors, 0 warnings**.
- [ ] New offline unit tests pass (`dotnet test --filter TestCategory=Unit`).
- [ ] Live E2E tests either pass with credentials, or are skipped gracefully without.
- [ ] `IBexioApiClient` exposes the new connector.
- [ ] `BexioServiceCollection.AddBexioServices` registers the new service.
- [ ] No `HttpClient` instantiated outside `BexioConnectionHandler`.
- [ ] No Newtonsoft.Json / MediatR / etc. sneaked in.
- [ ] No secrets in tests, fixtures or commit messages.
