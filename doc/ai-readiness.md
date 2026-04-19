---
title: AI Readiness Assessment
tags: [readiness, ai, assessment]
---

# AI Readiness Assessment

_Last updated: 2026-04-18 (Issue #8 — vendor Bexio OpenAPI spec)_

## Section 1: Documentation Quality
The documentation landscape for this project is formal and AI-aware.

- **What exists and is useful**:
  - `CLAUDE.md` at the repository root — high-level human overview: tech stack, solution structure, key conventions, important file locations.
  - [`ai_instructions.md`](../ai_instructions.md) at the repository root — strict procedural rules for AI coding agents (tech stack, architecture patterns, testing hard limits, forbidden patterns).
  - [`doc/development/feature-addition-guide.md`](./development/feature-addition-guide.md) — reproducible 5-step guide for implementing any Bexio endpoint (models → interfaces → implementation → DI → tests).
  - [`doc/development/testing-guide.md`](./development/testing-guide.md) — heavy-testing strategy: offline unit tests (NSubstitute / WireMock.Net / stub `HttpMessageHandler`) plus categorized live E2E tests.
  - Comprehensive C4 architecture models and ADRs under `doc/architecture/` (`README.md`, `context.md`, `containers.md`, `components/library.md`, `glossary.md`).
  - `README.md` clearly states the purpose and status of the project.
- **What is missing or insufficient**:
  - Nothing critical. A locally vendored OpenAPI spec is now committed at `doc/openapi/bexio-v3.json` (Issue #8).
- **Rate**: Ready

## Section 2: Test Coverage
Three-tier testing strategy now in place: offline unit tests, offline integration tests (WireMock.Net), and live E2E tests (optional, gracefully skipped when credentials are missing).

- **Test frameworks in use**:
  - NUnit 4 + NUnit Analyzers + Coverlet, configured across `tests/BexioApiNet.*/*.csproj`.
  - Permitted mocking libraries for offline tests: **NSubstitute** (preferred for `IBexioConnectionHandler` in unit tests) and **WireMock.Net** (for end-to-end stubs against the real `BexioConnectionHandler` in integration tests).
  - **Run commands**:
    - Offline Unit only: `dotnet test --filter TestCategory=Unit`
    - Offline Integration only: `dotnet test --filter TestCategory=Integration`
    - Live E2E only: `dotnet test --filter TestCategory=E2E` (requires `BexioApiNet__BaseUri` + `BexioApiNet__JwtToken`)
    - CI-safe default (Offline only): `dotnet test --filter TestCategory!=E2E`
- **What IS covered today**:
  - **Unit tests**: AccountService, CurrencyService, TaxService, BankAccountService, ManualEntryService, BexioConnectionHandler
  - **Integration tests**: Cancellation, Concurrency, ErrorResponse, Pagination, ParamValidation, per-service integration tests (Accounting / Banking / Contacts domains)
  - **E2E tests**:
    - `Accounting/Accounts/GetAll`
    - `Accounting/Currencies/GetAll`
    - `Accounting/ManualEntries` (Create, CreateAndAddFile, CreateAndAddFileFromStream, GetAll, GetAllAndDelete)
    - `Accounting/Taxes/GetAll`
    - `Banking/BankAccount/GetAll`
- **What is NOT covered**:
  - Significant portions of the Bexio API (Contacts, Invoices, Items, Projects, etc.) are not yet implemented.
- **Test quality assessment**:
  - Offline tests are comprehensive and fast. The E2E test suite skips gracefully, unblocking agents and CI that lack credentials. The E2E base class `BexioE2eTestBase` is categorized `[Category("E2E")]`, and offline test classes are categorized `[Category("Unit")]` or `[Category("Integration")]`.
- **Rate**: Partial Coverage (ready to expand; infrastructure is in place)

## Section 3: Technical Debt & Danger Zones

### 1. Live Integration Testing (Resolved)
- **Status**: **Resolved**
- **Location**: `tests/BexioApiNet.E2eTests/BexioE2eTestBase.cs`
- **What changed**: `BexioE2eTestBase.Setup()` now reads `BexioApiNet__BaseUri` and `BexioApiNet__JwtToken` from env vars and calls `Assert.Ignore(...)` when either is missing. The class is marked `[Category("E2E")]`. This lets `dotnet test` run safely in CI and agent sandboxes without Bexio credentials.
- **Remaining precaution**: Agents must still avoid running live E2E tests (`--filter TestCategory=E2E`) without explicit credentials.

### 2. HttpClient Lifecycle Management (Resolved)
- **Status**: **Resolved**
- **Location**: `src/BexioApiNet/Services/BexioConnectionHandler.cs`, `src/BexioApiNet.AspNetCore/BexioServiceCollection.cs`
- **What changed**: `BexioConnectionHandler` now exposes a dual constructor:
  1. `(IBexioConfiguration)` — legacy path for non-DI consumers. The handler owns and disposes its own `HttpClient`.
  2. `(HttpClient, IBexioConfiguration)` — DI path. `Dispose` is a no-op for the client because `IHttpClientFactory` owns it.
  
  `BexioServiceCollection.AddBexioServices` registers the handler as a typed client via `services.AddHttpClient<IBexioConnectionHandler, BexioConnectionHandler>(...)`, eliminating the socket-exhaustion risk under load.

### 3. Incomplete API Coverage (Known Debt)
- **Status**: Ongoing
- **Location**: `README.md` and `src/BexioApiNet/Interfaces/Connectors/`
- **Why it's noted**: Many Bexio domains (Items, Projects, etc.) are not implemented.
- **Precautions**: When implementing a new domain, follow [`doc/development/feature-addition-guide.md`](./development/feature-addition-guide.md) verbatim. Ship unit tests with every new method.

## Section 4: Backlog Ideas

| Title | Description | Complexity | Priority |
|-------|-------------|------------|----------|
| Expand API Connectors | Implement missing Bexio domains (Contacts, Projects, Invoices, Items, ...) per the feature-addition guide. | L | Medium |
| Wire Unit Tests into CI | Add a GitHub Actions job that runs `dotnet test --filter TestCategory!=E2E` on every PR (independently of E2E credentials availability). | S | Medium |

## Section 5: Completed (Issue #3, Issue #7, and Issue #8)

| Title | Resolved In |
|-------|-------------|
| Create `ai_instructions.md` | Issue #3 — present at repo root. |
| Create feature-addition guide | Issue #3 — `doc/development/feature-addition-guide.md`. |
| Create testing guide | Issue #3 — `doc/development/testing-guide.md`. |
| Refactor to `IHttpClientFactory` | Issue #3 — dual constructor on `BexioConnectionHandler` + `AddHttpClient<>` in DI registration. |
| Tests skip gracefully without credentials | Issue #3 — `TestBase.Setup` uses `Assert.Ignore`; base class `[Category("E2E")]`. |
| Harmonize `CLAUDE.md` with agent docs | Issue #3 — cross-links added, redundant rules moved to `ai_instructions.md`. |
| Scaffold three-tier test architecture | Issue #7 — UnitTests, IntegrationTests, and E2eTests projects. |
| Add comprehensive offline Unit and Integration coverage | Issue #7 — comprehensive Unit tests and WireMock.Net integration tests added. |
| Vendor Bexio OpenAPI Spec | Issue #8 — `doc/openapi/bexio-v3.json` committed (OpenAPI 3.0.2, API v3.0.0, 355 paths, retrieved 2026-04-18). Refresh procedure in `doc/openapi/README.md`. |
cedure in `doc/openapi/README.md`. |
