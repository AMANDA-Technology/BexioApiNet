---
title: AI Readiness Assessment
tags: [readiness, ai, assessment]
---

# AI Readiness Assessment

## Section 1: Documentation Quality
The documentation landscape for this project is well-structured but lacks formal AI-specific instructions.

- **What exists and is useful**:
  - A `CLAUDE.md` file exists in the repository root, providing high-level tech stack details, key conventions, and important file locations.
  - Comprehensive C4 architecture models and ADRs are located in `doc/architecture/` (`README.md`, `context.md`, `containers.md`, `components/library.md`, `glossary.md`).
  - The `README.md` clearly states the purpose of the project and its discontinuation status.
- **What is missing or insufficient**:
  - There is no standard `ai_instructions.md` file in the root.
  - The API docs link in `README.md` points to Bexio's v3 API, but it would be beneficial for AI agents to have local OpenAPI specifications to generate models from.
- **What needs improvement**:
  - The project needs an `ai_instructions.md` to formally instruct AI agents (replacing or complementing the existing `CLAUDE.md`).
- **Rate**: Ready

## Section 2: Test Coverage
The testing approach is integration-heavy and relies entirely on live API connectivity.

- **Test frameworks in use**: 
  - NUnit 4 and Coverlet, configured in `src/BexioApiNet.Tests/BexioApiNet.Tests.csproj`.
  - **Run command**: `dotnet test src/BexioApiNet.Tests/BexioApiNet.Tests.csproj`
- **What IS covered**: 
  - `Accounting/Accounts/GetAll`
  - `Accounting/Currencies/GetAll`
  - `Accounting/ManualEntries` (Create, CreateAndAddFile, CreateAndAddFileFromStream, GetAll, GetAllAndDelete)
  - `Accounting/Taxes/GetAll`
  - `Banking/BankAccount/GetAll`
  - *(See `src/BexioApiNet.Tests/Tests/` directory for full list)*
- **What is NOT covered**: 
  - There are no unit tests using a mocked `HttpMessageHandler`.
  - Significant portions of the Bexio API (e.g., Contacts, Invoices, Items) are missing both library implementation and tests.
- **Test quality assessment**: 
  - Tests are high-fidelity integration tests but are **brittle** and impossible to run without live credentials. As seen in `src/BexioApiNet.Tests/TestBase.cs` lines 42-43, tests will throw `InvalidOperationException` if `BexioApiNet__BaseUri` and `BexioApiNet__JwtToken` environment variables are missing.
- **Rate**: Partial Coverage

## Section 3: Technical Debt & Danger Zones

### 1. Live Integration Testing (Danger Zone)
- **Location**: `src/BexioApiNet.Tests/TestBase.cs` lines 42-43
- **Why it's dangerous**: Running `dotnet test` requires real Bexio API credentials. If an AI agent attempts to run tests during code changes without these credentials, the test suite will instantly fail. AI agents cannot verify their changes properly.
- **Precautions**: Agents must avoid running `dotnet test` unless specifically provided with environment credentials. Instead, agents should rely on building (`dotnet build`) to check for compilation errors, and mock components where possible.

### 2. HttpClient Lifecycle Management (Technical Debt)
- **Location**: `src/BexioApiNet/Services/BexioConnectionHandler.cs` line 36
- **Why it's dangerous**: `BexioConnectionHandler` instantiates and manages its own `HttpClient` (`_client = new(new HttpClientHandler...)`). In ASP.NET Core applications, this can lead to socket exhaustion under heavy load. It does not use `IHttpClientFactory`.
- **Precautions**: Any AI agent modifying the HTTP pipeline must be careful not to introduce more manual `HttpClient` instantiations. A refactoring to `IHttpClientFactory` should be planned.

### 3. Incomplete API Coverage (Known Debt)
- **Location**: `README.md` and `src/BexioApiNet/Interfaces/Connectors/`
- **Why it's dangerous**: The `README.md` notes the project is temporarily discontinued due to a lack of free test accounts. Many Bexio domains (Contacts, Projects, Invoices) are not implemented. 
- **Precautions**: When an AI is asked to implement a new Bexio endpoint, it will have to create new Connector Services, Models, and Interfaces from scratch following the existing pattern.

## Section 4: Backlog Ideas

| Title | Description | Complexity | Priority |
|-------|-------------|------------|----------|
| Refactor to `IHttpClientFactory` | Modify `BexioConnectionHandler` and `BexioApiNet.AspNetCore` to utilize `IHttpClientFactory` instead of manual `HttpClient` instantiation to prevent socket exhaustion. | M | High |
| Add Offline Unit Tests | Create a suite of unit tests utilizing a mocked `HttpMessageHandler` to allow testing API parsing and logic without live Bexio credentials. | M | High |
| Create `ai_instructions.md` | Standardize AI instructions by migrating rules from `CLAUDE.md` to `ai_instructions.md` and adding explicit notes about testing constraints. | S | Low |
| Expand API Connectors | Implement missing Bexio API endpoints such as Contacts, Invoices, and Projects based on the existing `ConnectorService` pattern. | L | Medium |