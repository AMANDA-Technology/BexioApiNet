---
title: AI Agent Instructions for BexioApiNet
tags: [ai, instructions, contributing, conventions]
---

# AI Agent Instructions — BexioApiNet

This file contains **strict, procedural instructions** for AI coding agents working in this repository. Follow these rules to the letter — they keep the library production-grade and aligned with the Bexio REST API.

- This file: **strict rules for AI agents** (what to do, what never to do).
- [`CLAUDE.md`](./CLAUDE.md): **high-level human overview** (tech stack, structure, locations).
- [`doc/development/feature-addition-guide.md`](./doc/development/feature-addition-guide.md): **step-by-step guide** for adding a new Bexio endpoint.
- [`doc/development/testing-guide.md`](./doc/development/testing-guide.md): **testing standards** (offline unit + live E2E).

If anything here conflicts with `CLAUDE.md`, **this file wins for agent behavior**.

## 1. Mission & Source of Truth

1. This library is a 1:1 typed .NET client for the **Bexio REST API v3.0.0**. Source of truth: <https://docs.bexio.com/>.
   - **Vendored spec (primary reference for AI agents):** [`doc/openapi/bexio-v3.json`](./doc/openapi/bexio-v3.json) — OpenAPI 3.0.2, API v3.0.0, 355 paths, retrieved 2026-04-18. Use this for offline, deterministic model generation. See [`doc/openapi/README.md`](./doc/openapi/README.md) for the refresh procedure.
   - **Human-readable mirror:** <https://docs.bexio.com/> — use for browsing documentation and for verifying context not captured in the JSON spec.
2. **Every** endpoint, DTO field, status code and query parameter must match the Bexio docs exactly. If the docs and the code disagree, **the docs are right** — open a change.
3. Never invent endpoints, fields or behavior that the Bexio docs do not describe. If the docs are ambiguous, stop and ask rather than guessing.

## 2. Tech Stack (non-negotiable)

| Concern | Value |
|--------|-------|
| Language | C# 13 |
| Framework | .NET 9.0 (`net9.0`) |
| JSON | `System.Text.Json` — **never** add Newtonsoft.Json |
| DI | `Microsoft.Extensions.DependencyInjection` (+ `Microsoft.Extensions.Http` for typed clients) |
| Tests | NUnit 4 + NUnit Analyzers + Coverlet |
| Nullability | `<Nullable>enable</Nullable>` everywhere |
| XML docs | `<GenerateDocumentationFile>true</GenerateDocumentationFile>` on shipping projects |

If a change would require bumping any of these, stop and escalate.

## 3. Architecture Patterns (do not deviate)

### 3.1 `ApiResult<T>` wrapper
- Every public connector method returns `Task<ApiResult<T>>` (or `Task<ApiResult<object>>` for `Delete`).
- **Never throw** on non-2xx Bexio responses. Populate `ApiResult.IsSuccess`, `ApiResult.StatusCode`, `ApiResult.ApiError`, `ApiResult.Data` and `ApiResult.ResponseHeaders`.
- Only throw for genuinely exceptional conditions (e.g., the paging invariant in `BexioConnectionHandler.FetchAll`).

### 3.2 `ConnectorService` base class
- Every endpoint group (`AccountService`, `ManualEntryService`, ...) inherits from `ConnectorService` in `src/BexioApiNet/Services/Connectors/Base/ConnectorService.cs`.
- Services never instantiate their own `HttpClient`. They always go through `ConnectionHandler` (the injected `IBexioConnectionHandler`).
- Endpoint constants live in a sibling `<Entity>Configuration` static class (e.g., `ManualEntryConfiguration.ApiVersion` and `ManualEntryConfiguration.EndpointRoot`). Do not inline string paths.

### 3.3 `IBexioApiClient` aggregate
- `BexioApiClient` is the aggregate root injected into consumer code. Every connector service is exposed as a property and resolved via DI.
- Adding a new connector means:
  1. Add the property on `IBexioApiClient`.
  2. Assign it in `BexioApiClient`'s constructor.
  3. Register it in `BexioServiceCollection.AddBexioServices`.

### 3.4 DI registration
- `AddBexioServices` registers the connection handler as a **typed `HttpClient`** via `IHttpClientFactory`. **Do not** reintroduce `services.AddScoped<IBexioConnectionHandler, BexioConnectionHandler>()` — that re-creates the socket-exhaustion bug.
- The dual constructor on `BexioConnectionHandler` is deliberate:
  - `(IBexioConfiguration)` — non-DI consumers; owns and disposes the `HttpClient`.
  - `(HttpClient, IBexioConfiguration)` — DI path; does **not** dispose the injected client.

### 3.5 Query parameters
- Optional query parameters go in a domain-specific `QueryParameter<Entity>` record (see `src/BexioApiNet/Models/QueryParameter*.cs`). The wrapped `QueryParameter.Parameters` dictionary is what the handler serializes onto the URL.

## 4. Model & DTO Rules

1. **Records**, not classes, for DTOs: `public sealed record Account { ... }`.
2. Use `required` for properties Bexio always returns or always needs. Use `init` for immutable properties; only use `set` where the Bexio flow genuinely requires mutation.
3. **Enable nullable reference types**. If Bexio can return `null`, the property type must be nullable.
4. Map property names with `[JsonPropertyName("...")]` that match the Bexio JSON names exactly, including casing and snake_case where applicable.
5. Place domain models in `src/BexioApiNet.Abstractions/Models/<Domain>/<Subdomain>/`. Keep "Create" / "Edit" view models in a sibling `Views/` folder.
6. For static / lookup collections prefer `IReadOnlyList<T>` or `ImmutableArray<T>` in public APIs.
7. Every `public` / `protected` type and member needs an XML `<summary>`. Missing docs produce build warnings, which break the build.

## 5. Testing Rules (hard limits)

1. **Never** run `dotnet test` against the live Bexio API unless the task prompt explicitly confirms credentials are provided. `BexioApiNet__BaseUri` and `BexioApiNet__JwtToken` must both be set — otherwise `BexioE2eTestBase` now calls `Assert.Ignore` and the test is skipped (no false failure).
2. Live E2E tests live in `tests/BexioApiNet.E2eTests/Tests/<Domain>/...`, inherit from `BexioE2eTestBase`, and are automatically categorized `[Category("E2E")]`.
3. **Every new connector method must also have an offline unit test** that does not require credentials. Mock `IBexioConnectionHandler` with NSubstitute for unit tests, or use WireMock.Net for integration tests.
4. For unit tests: place them in `tests/BexioApiNet.UnitTests/<Domain>/...` and mark them `[Category("Unit")]`.
5. For integration tests: place them in `tests/BexioApiNet.IntegrationTests/<Domain>/...` and mark them `[Category("Integration")]`.
6. Filter for CI:
   - Offline Unit: `dotnet test --filter TestCategory=Unit`
   - Offline Integration: `dotnet test --filter TestCategory=Integration`
   - Live E2E: `dotnet test --filter TestCategory=E2E` (only with credentials in env).
   - CI Default (Offline only): `dotnet test --filter TestCategory!=E2E`
7. Do **not** add test-only code paths to production types. Use fakes/mocks at the test boundary.
8. See [`doc/development/testing-guide.md`](./doc/development/testing-guide.md) for full details.

## 6. Build & Verification Rules

1. Before completing any task, run `dotnet build` and confirm **0 errors and 0 warnings**. Treat warnings as errors. The public API projects generate XML docs, so missing doc comments cause warnings.
2. If new NuGet packages are added, run `dotnet list package --outdated`. Only bump inside the current major version unless the task calls for a major upgrade.
3. Never introduce `.Result` or `.Wait()` on tasks; use `async`/`await` end-to-end.
4. Keep `ImplicitUsings` and `Nullable` enabled in all csproj files. Do not disable them per-file.

## 7. Secrets & Security

1. Never write real Bexio tokens, credentials, workspace tokens or customer data into source, tests, fixtures or docs.
2. `BexioApiNet__BaseUri` and `BexioApiNet__JwtToken` are supplied via the environment only. Reference them by name — never by value.
3. Never log or serialize `Authorization` headers or `JwtToken` values.
4. Do not commit `.env`, `appsettings.Development.json` or any file containing secrets.

## 8. Workflow Expectations for AI Agents

1. **Scope discipline.** Apply the minimum change that fulfills the task. Do not refactor unrelated code "on the way through".
2. **No speculative features.** Implement only what the issue/blueprint requests. Hypothetical future needs are not a reason to add abstractions.
3. **Incremental verification.** Build after each significant change, not only at the end.
4. **Commit before finishing.** Every agent run ends with either a clean tree or a new commit — never staged/unstaged diffs.
5. **Update docs alongside code.** If a convention changes, update this file, `CLAUDE.md`, and the relevant guide in `doc/development/` in the same change.
6. If the Bexio docs describe something this library does not yet support, do not silently stub it. Implement it end-to-end per [`doc/development/feature-addition-guide.md`](./doc/development/feature-addition-guide.md), or file a backlog entry in `doc/ai-readiness.md` § 4.

## 9. Forbidden Patterns

Do not:
- Instantiate `HttpClient` directly in new code. Use the injected client from `ConnectionHandler`.
- Throw exceptions from connector methods for normal non-2xx responses — populate `ApiResult`.
- Use classes when a record fits. Use mutable state when `init` / `required` fits.
- Add Newtonsoft.Json, AutoMapper, MediatR, FluentAssertions or other frameworks not already on the dependency list.
- Skip XML doc comments on public members — they are compiled into the NuGet packages.
- Hardcode `/2.0/`, `/3.0/` version strings in services — put them in the corresponding `<Entity>Configuration` constants.

## 10. When in Doubt

1. Re-read the relevant section of <https://docs.bexio.com/>.
2. Compare with a fully-implemented example in the repo: `ManualEntryService` + `ManualEntry` + `QueryParameterManualEntry` is the canonical reference.
3. If still unclear, stop and surface the question in the task result rather than guessing.
