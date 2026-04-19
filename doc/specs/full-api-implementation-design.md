---
title: Full API Implementation Design
created: 2026-04-19
status: approved
reference: CashCtrlApiNet (AMANDA-Technology/CashCtrlApiNet)
---

# Full API Implementation Design — BexioApiNet 100% Coverage

## 1. Objective

Achieve **100% Bexio API coverage** with **100% accuracy** and **100% test coverage** following the proven CashCtrlApiNet methodology: systematic audit → wave-based parallel implementation → progressive E2E verification.

**Target:** 309 endpoint methods across 56 API tags, implemented as ~60 connector services with mandatory unit + integration tests per method and grouped E2E verification.

---

## 2. Strategy Overview

```
Phase 0: Infrastructure & Golden Reference
    ↓
Wave 1-6: Parallel Domain Implementation (3-4 services per wave, parallelizable)
    ↓
Phase F: Final Audit & E2E Verification Groups
```

### Key Principles (from CashCtrlApiNet)

1. **Spec-driven accuracy** — Every model field verified against `doc/openapi/bexio-v3.json`
2. **Wave parallelization** — Independent domain groups implemented concurrently
3. **Progressive verification** — E2E tests run in numbered groups, discrepancies tracked
4. **Single golden pattern** — ManualEntryService is the reference; all services follow it
5. **Coverage matrix as checkpoint** — `doc/analysis/api-completeness-audit.md` updated on every merge

---

## 3. Phase 0: Infrastructure Prerequisites

Before any wave begins, the following must be in place.

### 3.1 ConnectionHandler Extensions

Current `BexioConnectionHandler` supports: GET, POST (JSON + multipart), DELETE.

**Missing patterns to add:**

```csharp
// PUT — full entity update
Task<ApiResult<TResult>> PutAsync<TResult, TPayload>(TPayload payload, string requestPath, CancellationToken ct);

// PATCH — partial entity update
Task<ApiResult<TResult>> PatchAsync<TResult, TPayload>(TPayload payload, string requestPath, CancellationToken ct);

// POST action (no request body, triggers workflow)
Task<ApiResult<TResult>> PostActionAsync<TResult>(string requestPath, CancellationToken ct);

// POST action (no response body expected)
Task<ApiResult> PostActionAsync(string requestPath, CancellationToken ct);

// GET binary (PDF download, file preview)
Task<ApiResult<byte[]>> GetBinaryAsync(string requestPath, CancellationToken ct);

// POST search (body contains filter criteria, returns list)
Task<ApiResult<List<TResult>>> PostSearchAsync<TResult, TFilter>(TFilter filter, string requestPath, CancellationToken ct);

// POST bulk create
Task<ApiResult<List<TResult>>> PostBulkAsync<TResult, TPayload>(List<TPayload> payloads, string requestPath, CancellationToken ct);
```

**Acceptance:** All new methods have unit tests + integration tests (WireMock) before Wave 1 begins.

### 3.2 Search Pattern Standardization

Bexio uses `POST /{entity}/search` with a JSON body for filtering. This needs a consistent abstraction:

```csharp
public sealed record SearchCriteria
{
    [JsonPropertyName("field")]
    public required string Field { get; init; }

    [JsonPropertyName("value")]
    public required string Value { get; init; }

    [JsonPropertyName("criteria")]
    public required string Criteria { get; init; } // "like", "=", "!=", etc.
}
```

### 3.3 Document Position Polymorphic Base

All 7 position types share the same URL pattern. Create a generic base:

```csharp
public abstract class PositionService<TPosition, TPositionCreate> : ConnectorService
{
    protected abstract string PositionType { get; } // "article", "custom", "discount", etc.

    public async Task<ApiResult<List<TPosition>>> Get(string documentType, int documentId, ...)
        => await ConnectionHandler.GetAsync<List<TPosition>>(
            $"2.0/{documentType}/{documentId}/kb_position_{PositionType}", ...);
}
```

---

## 4. Wave Execution Model

### 4.1 Wave Structure

| Wave | Domain Groups | Endpoints | Parallel Services | Depends On |
|------|---------------|-----------|-------------------|------------|
| 0 | Infrastructure | N/A | N/A | — |
| 1 | Accounting + Banking + Contacts | 67 | 12 services | Phase 0 |
| 2 | Invoices + Quotes + Orders | 58 | 5 services | Wave 1 (shared models) |
| 3 | Items + Document Positions | 51 | 11 services | Wave 2 (sales docs exist) |
| 4 | Projects + Timesheets + Tasks | 38 | 8 services | Wave 1 (contacts exist) |
| 5 | Purchase + Expenses + Payroll | 31 | 6 services | Phase 0 |
| 6 | Files + Master Data | 53 | 13 services | Phase 0 |

### 4.2 Per-Service Implementation Checklist

For each service (1 PR per service):

- [ ] Models created in `src/BexioApiNet.Abstractions/Models/<Domain>/<Subdomain>/`
- [ ] Views created for create/update payloads
- [ ] Enums created where API uses integer codes with defined meanings
- [ ] QueryParameter class created for list endpoints
- [ ] Interface defined in `src/BexioApiNet/Interfaces/Connectors/<Domain>/`
- [ ] Configuration static class created
- [ ] Service implementation inheriting ConnectorService
- [ ] Service wired into IBexioApiClient + BexioApiClient + BexioServiceCollection
- [ ] Unit tests for every public method (NSubstitute)
- [ ] Integration smoke test (WireMock.Net)
- [ ] E2E test stub (skippable without credentials)
- [ ] `dotnet build /warnaserror` clean
- [ ] Coverage audit updated (TODO → DONE)
- [ ] XML docs on all public types and members

### 4.3 Service Size Guidelines

| Complexity | Endpoints | Example | PR Size |
|---|---|---|---|
| Simple (read-only) | 1-2 | AccountGroups, Permissions | XS (~3 files + tests) |
| Standard CRUD | 4-6 | Countries, Units, Items | S (~6 files + tests) |
| Full CRUD + Search | 6-8 | Contacts, Projects | M (~8 files + tests) |
| Complex (actions, sub-resources) | 10-26 | Invoices, Manual Entries | L (~12+ files + tests) |

---

## 5. Model Accuracy Protocol

### 5.1 Primary Source: OpenAPI Spec

Every model field must be verified against the vendored spec:

```bash
# Extract schema for a specific endpoint
python3 -c "
import json
with open('doc/openapi/bexio-v3.json') as f:
    spec = json.load(f)
schema = spec['paths']['/2.0/contact']['get']['responses']['200']['content']['application/json']['schema']
print(json.dumps(schema, indent=2))
"
```

### 5.2 Model Verification Checklist

For each model:
- [ ] Every field in the OpenAPI schema has a matching property
- [ ] `[JsonPropertyName]` matches the exact JSON key from the spec
- [ ] Nullability matches the schema (`required` array vs optional)
- [ ] Types match: `integer` → `int`, `number` → `decimal`, `string($date)` → `DateOnly`, `string($date-time)` → `DateTime`
- [ ] Enum values match the spec's `enum` array
- [ ] Nested objects are modeled as separate records

### 5.3 Discrepancy Tracking

When E2E tests reveal spec-vs-reality differences:
1. Document in `doc/analysis/api-doc-discrepancies.md`
2. Note the actual behavior observed
3. Adjust the model to match reality (not the spec)
4. Add a code comment referencing the discrepancy doc

---

## 6. Testing Strategy

### 6.1 Three-Tier Coverage Matrix

| Tier | Tool | Required | Runs Offline | Validates |
|------|------|----------|--------------|-----------|
| Unit | NSubstitute | MANDATORY | Yes | Correct HTTP verb, route, params, deserialization |
| Integration | WireMock.Net | MANDATORY | Yes | Full HTTP pipeline, pagination, errors, concurrency |
| E2E | Live API | RECOMMENDED | No | Model accuracy, real API behavior |

### 6.2 Unit Test Pattern (per method)

```csharp
[TestFixture]
[Category("Unit")]
public sealed class ContactServiceTests : ServiceTestBase
{
    [Test]
    public async Task Get_WithNoParams_CallsCorrectEndpoint()
    {
        // Arrange
        var expected = new ApiResult<List<Contact>?> { IsSuccess = true, Data = [] };
        ConnectionHandler
            .GetAsync<List<Contact>?>(Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

        var service = new ContactService(ConnectionHandler);

        // Act
        var result = await service.Get();

        // Assert
        Assert.That(result, Is.SameAs(expected));
        await ConnectionHandler.Received(1).GetAsync<List<Contact>?>(
            "2.0/contact", null, Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Create_SendsCorrectPayload()
    {
        var payload = new ContactCreate { Name1 = "Test" };
        var expected = new ApiResult<Contact> { IsSuccess = true, Data = new Contact { Id = 1, Name1 = "Test" } };
        ConnectionHandler
            .PostAsync<Contact, ContactCreate>(Arg.Any<ContactCreate>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

        var service = new ContactService(ConnectionHandler);
        var result = await service.Create(payload);

        Assert.That(result, Is.SameAs(expected));
        await ConnectionHandler.Received(1).PostAsync<Contact, ContactCreate>(
            payload, "2.0/contact", Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Delete_SendsCorrectId()
    {
        var expected = new ApiResult<object> { IsSuccess = true };
        ConnectionHandler
            .Delete(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

        var service = new ContactService(ConnectionHandler);
        var result = await service.Delete(42);

        Assert.That(result, Is.SameAs(expected));
        await ConnectionHandler.Received(1).Delete("2.0/contact/42", Arg.Any<CancellationToken>());
    }
}
```

### 6.3 Integration Test Pattern (per domain)

```csharp
[TestFixture]
[Category("Integration")]
public sealed class ContactSmokeTests : IntegrationTestBase
{
    [Test]
    public async Task GetContacts_ReturnsDeserializedList()
    {
        Server
            .Given(Request.Create().WithPath("/2.0/contact").UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody("""[{"id":1,"name_1":"Acme Corp","name_2":null}]"""));

        var result = await ConnectionHandler.GetAsync<List<Contact>>("2.0/contact", null, CancellationToken.None);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data, Has.Count.EqualTo(1));
        Assert.That(result.Data![0].Name1, Is.EqualTo("Acme Corp"));
    }
}
```

### 6.4 E2E Verification Groups

Following CashCtrlApiNet's pattern, E2E tests are organized in numbered groups:

| Group | Domains | Services Tested | Depends On |
|---|---|---|---|
| 1 | Accounting | Accounts, Currencies, Taxes, ManualEntries, VatPeriods | — |
| 2 | Banking | BankAccounts, Payments, PaymentTypes | — |
| 3 | Contacts | Contacts, ContactGroups, ContactRelations, Addresses | — |
| 4 | Items | Items, Units, Stock | — |
| 5 | Sales | Invoices, Quotes, Orders, Deliveries | Group 3 + 4 |
| 6 | Positions | All 7 position types | Group 5 |
| 7 | Projects | Projects, Milestones, Packages, Timesheets, Tasks | Group 3 |
| 8 | Purchase | Bills, PurchaseOrders, Expenses | Group 3 |
| 9 | Files & Admin | Files, Users, Company, Notes | — |
| 10 | Payroll | Employees, Absences, Paystubs | — |

---

## 7. Epic & Issue Structure

### 7.1 GitHub Issue Hierarchy

```
Epic: 100% API Coverage (parent tracking issue)
├── Phase 0: Infrastructure Extensions
│   ├── Sub: Add PUT/PATCH/PostAction to ConnectionHandler
│   ├── Sub: Add binary download support
│   ├── Sub: Add search pattern abstraction
│   └── Sub: Add position service base class
├── Wave 1: Accounting + Banking + Contacts
│   ├── Sub: Complete CurrencyService (6 endpoints)
│   ├── Sub: Complete ManualEntryService (8 endpoints)
│   ├── Sub: Complete TaxService (2 endpoints)
│   ├── Sub: Complete BankAccountService (1 endpoint)
│   ├── Sub: AccountGroupService (1 endpoint)
│   ├── Sub: BusinessYearService (2 endpoints)
│   ├── Sub: CalendarYearService (4 endpoints)
│   ├── Sub: VatPeriodService (2 endpoints)
│   ├── Sub: ReportService (1 endpoint)
│   ├── Sub: PaymentTypeService (2 endpoints)
│   ├── Sub: PaymentService (6 endpoints)
│   ├── Sub: OutgoingPaymentService (5 endpoints)
│   ├── Sub: ContactService (8 endpoints)
│   ├── Sub: ContactGroupService (6 endpoints)
│   ├── Sub: ContactRelationService (6 endpoints)
│   ├── Sub: ContactSectorService (2 endpoints)
│   └── Sub: AdditionalAddressService (6 endpoints)
├── Wave 2: Sales Documents
│   ├── Sub: InvoiceService (18 endpoints)
│   ├── Sub: InvoiceReminderService (8 endpoints)
│   ├── Sub: QuoteService (17 endpoints)
│   ├── Sub: OrderService (12 endpoints)
│   └── Sub: DeliveryService (3 endpoints)
├── Wave 3: Items & Positions
│   ├── Sub: ItemService (6 endpoints)
│   ├── Sub: UnitService (6 endpoints)
│   ├── Sub: StockAreaService (2 endpoints)
│   ├── Sub: StockLocationService (2 endpoints)
│   ├── Sub: ItemPositionService (5 endpoints)
│   ├── Sub: DefaultPositionService (5 endpoints)
│   ├── Sub: DiscountPositionService (5 endpoints)
│   ├── Sub: TextPositionService (5 endpoints)
│   ├── Sub: SubtotalPositionService (5 endpoints)
│   ├── Sub: SubPositionService (5 endpoints)
│   └── Sub: PagebreakPositionService (5 endpoints)
├── Wave 4: Projects & Time
│   ├── Sub: ProjectService (8 endpoints)
│   ├── Sub: ProjectStateService (1 endpoint)
│   ├── Sub: ProjectTypeService (1 endpoint)
│   ├── Sub: MilestoneService (5 endpoints)
│   ├── Sub: PackageService (5 endpoints)
│   ├── Sub: TimesheetService (6 endpoints)
│   ├── Sub: TimesheetStatusService (1 endpoint)
│   ├── Sub: TaskService (6 endpoints)
│   ├── Sub: TaskPriorityService (1 endpoint)
│   ├── Sub: TaskStatusService (1 endpoint)
│   └── Sub: BusinessActivityService (3 endpoints)
├── Wave 5: Purchase, Expenses & Payroll
│   ├── Sub: BillService (8 endpoints)
│   ├── Sub: PurchaseOrderService (5 endpoints)
│   ├── Sub: ExpenseService (8 endpoints)
│   ├── Sub: EmployeeService (4 endpoints)
│   ├── Sub: AbsenceService (5 endpoints)
│   └── Sub: PaystubService (1 endpoint)
├── Wave 6: Files & Master Data
│   ├── Sub: FileService (9 endpoints)
│   ├── Sub: DocumentSettingService (1 endpoint)
│   ├── Sub: DocumentTemplateService (1 endpoint)
│   ├── Sub: CountryService (6 endpoints)
│   ├── Sub: LanguageService (2 endpoints)
│   ├── Sub: SalutationService (6 endpoints)
│   ├── Sub: TitleService (6 endpoints)
│   ├── Sub: CommunicationTypeService (2 endpoints)
│   ├── Sub: CompanyProfileService (2 endpoints)
│   ├── Sub: UserService (3 endpoints)
│   ├── Sub: FictionalUserService (5 endpoints)
│   ├── Sub: PermissionService (1 endpoint)
│   ├── Sub: NoteService (6 endpoints)
│   └── Sub: CommentService (3 endpoints)
└── Phase F: E2E Verification & Final Audit
    ├── Sub: E2E Group 1-4 (independent domains)
    ├── Sub: E2E Group 5-8 (dependent domains)
    └── Sub: Final coverage audit sign-off
```

### 7.2 Issue Template (per service)

```markdown
## Implement {ServiceName}

**Domain Group:** {N} — {GroupName}
**Wave:** {WaveN}
**Tag:** [{BexioTagName}](https://docs.bexio.com/#tag/{BexioTagName})
**Endpoints:** {count}

### Endpoint List

| Method | Path | Notes |
|--------|------|-------|
| ... | ... | ... |

### Acceptance Criteria

- [ ] Models in `src/BexioApiNet.Abstractions/Models/{Domain}/{Subdomain}/`
- [ ] Interface `I{Entity}Service` in `src/BexioApiNet/Interfaces/Connectors/{Domain}/`
- [ ] Implementation `{Entity}Service` in `src/BexioApiNet/Services/Connectors/{Domain}/`
- [ ] Configuration class with API version + endpoint root
- [ ] Wired into IBexioApiClient + BexioApiClient + BexioServiceCollection
- [ ] QueryParameter class (if list endpoint exists)
- [ ] Unit tests for all {count} methods
- [ ] Integration smoke test (WireMock)
- [ ] E2E test stub (graceful skip)
- [ ] `dotnet build /warnaserror` clean
- [ ] Coverage audit updated

### Reference

- OpenAPI spec: `doc/openapi/bexio-v3.json` (path: `/{path}`)
- Feature guide: `doc/development/feature-addition-guide.md`
- Testing guide: `doc/development/testing-guide.md`
- Golden reference: `ManualEntryService`
```

---

## 8. Accuracy Verification Approach

### 8.1 Schema Extraction Script

Provide a helper script for agents to extract exact schemas:

```bash
# scripts/extract-schema.py
# Usage: python3 scripts/extract-schema.py "/2.0/contact" "get" "200"
```

### 8.2 Model Generation Workflow

1. Extract response schema from OpenAPI spec
2. Map `properties` → C# record properties
3. Map `required` array → `required` keyword
4. Map `$ref` → separate record files
5. Map `enum` → C# enum type
6. Verify JSON round-trip with sample response from spec `examples`

### 8.3 E2E Discrepancy Protocol

When live API diverges from spec:
1. Log to `doc/analysis/api-doc-discrepancies.md`
2. Trust observed behavior over spec
3. Adjust model + add XML doc noting the divergence
4. Keep unit/integration tests aligned with actual behavior

---

## 9. Parallelization Strategy

### 9.1 Within a Wave

Services within the same wave are **independent** and can be implemented by parallel agents:

- Each service has its own models, interface, implementation, tests
- No cross-service dependencies within a wave
- Only shared dependency: `IBexioApiClient` aggregate (merge conflicts resolved at wave end)

### 9.2 Between Waves

Strict ordering where noted:
- Wave 2 (Sales) depends on Wave 1 contacts for shared model references
- Wave 3 (Positions) depends on Wave 2 for document types
- Wave 4 (Projects) can run parallel with Wave 2-3
- Wave 5-6 depend only on Phase 0

### 9.3 Recommended Batch Sizes

Per CashCtrlApiNet experience: **3-4 parallel agents per wave**, each handling 2-4 services.

---

## 10. Definition of Done

The project achieves 100% when:

- [ ] All 309 endpoint methods have implementations
- [ ] All 309 methods have unit tests
- [ ] All domains have integration smoke tests
- [ ] E2E verification groups 1-10 pass with live credentials
- [ ] `doc/analysis/api-completeness-audit.md` shows 0 TODO items
- [ ] `doc/analysis/api-doc-discrepancies.md` documents all observed divergences
- [ ] `dotnet build /warnaserror` clean
- [ ] `dotnet test --filter TestCategory!=E2E` passes (offline suite)
- [ ] NuGet packages build successfully (`dotnet pack`)
- [ ] README updated with full feature matrix

---

## 11. Estimated Effort

Based on CashCtrlApiNet metrics (375 endpoints, 58 services, ~6 weeks with parallel agents):

| Phase | Services | Effort (sequential) | Effort (3-4 parallel) |
|---|---|---|---|
| Phase 0 | Infrastructure | 1-2 days | 1-2 days |
| Wave 1 | 17 services | 5-7 days | 2-3 days |
| Wave 2 | 5 services | 4-5 days | 2 days |
| Wave 3 | 11 services | 4-5 days | 2 days |
| Wave 4 | 11 services | 4-5 days | 2 days |
| Wave 5 | 6 services | 3-4 days | 1-2 days |
| Wave 6 | 14 services | 5-6 days | 2-3 days |
| Phase F | Verification | 2-3 days | 2-3 days |
| **TOTAL** | **~60 services** | **~30 days** | **~15 days** |
