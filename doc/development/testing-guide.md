---
title: Testing Guide — Heavy, Resilient Testing for BexioApiNet
tags: [testing, nunit, nsubstitute, wiremock, guide]
---

# Testing Guide — BexioApiNet

This guide defines how we test BexioApiNet. Our goal is **heavy, fast, resilient testing** that works with and without Bexio credentials, so both CI and AI agents can verify changes confidently.

Two independent layers:

1. **Unit tests (offline, mandatory)** — fast, run anywhere, no network. Mocked dependencies.
2. **End-to-end / integration tests (live, optional)** — hit the real Bexio API. Require credentials. Skipped automatically when credentials are absent.

Both layers use **NUnit 4** as the runner.

---

## 1. Test Frameworks

| Package | Purpose | Mandatory? |
|---------|---------|------------|
| `NUnit` (v4) | Runner + assertions | Yes |
| `NUnit3TestAdapter` | VS / `dotnet test` adapter | Yes |
| `NUnit.Analyzers` | Static analysis on tests | Yes |
| `Microsoft.NET.Test.Sdk` | Test platform | Yes |
| `coverlet.collector` | Coverage | Yes |
| `NSubstitute` | Mocking `IBexioConnectionHandler` in unit tests | **Add when writing the first unit test** |
| `WireMock.Net` | HTTP-level stubs for `BexioConnectionHandler` end-to-end unit tests | Add when hand-rolled test handlers are not enough |

`NSubstitute` and `WireMock.Net` are the only mocking libraries permitted — do not introduce Moq, FakeItEasy, etc.

---

## 2. Test Layout

```
src/BexioApiNet.Tests/
├── UnitTests/                   # Offline unit tests. No credentials required.
│   ├── Accounting/
│   │   └── ManualEntries/
│   │       └── ManualEntryServiceTests.cs
│   └── Connection/
│       └── BexioConnectionHandlerTests.cs
├── Tests/                        # Live E2E tests. Credentials required, skipped otherwise.
│   ├── Accounting/
│   └── Banking/
├── TestBase.cs                   # Live-test base class ([Category("E2E")], Assert.Ignore fallback).
└── Usings.cs
```

**Do not** mix unit and E2E tests in the same file. Keep them in their respective folders.

---

## 3. Unit Tests (Offline)

Unit tests are the primary safety net. Every new connector method needs one.

### 3.1 Categorization

Every unit-test class must carry:

```csharp
[Category("Unit")]
```

Run offline tests only:

```bash
dotnet test --filter TestCategory=Unit
```

### 3.2 Pattern A — Mock `IBexioConnectionHandler`

Use when the goal is to verify the connector service **builds the right request path, verb and payload** and forwards the connection handler's response unchanged.

```csharp
using BexioApiNet.Abstractions.Models.Api;
using BexioApiNet.Interfaces;
using BexioApiNet.Services.Connectors.Accounting;
using NSubstitute;
using NUnit.Framework;

namespace BexioApiNet.Tests.UnitTests.Accounting.ManualEntries;

[TestFixture]
[Category("Unit")]
public class ManualEntryServiceTests
{
    [Test]
    public async Task Delete_calls_connection_handler_with_correct_path()
    {
        // Arrange
        var handler = Substitute.For<IBexioConnectionHandler>();
        handler.Delete("3.0/accounting/manual_entries/42", Arg.Any<CancellationToken>())
               .Returns(new ApiResult<object> { IsSuccess = true });
        var service = new ManualEntryService(handler);

        // Act
        var result = await service.Delete(42);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        await handler.Received(1).Delete("3.0/accounting/manual_entries/42", Arg.Any<CancellationToken>());
    }
}
```

### 3.3 Pattern B — Fake `HttpMessageHandler` in real `BexioConnectionHandler`

Use when you need to verify **serialization / deserialization** and **request URL construction** through the real handler. This exercises the actual code path customers hit.

This leverages the DI-friendly constructor `BexioConnectionHandler(HttpClient, IBexioConfiguration)`.

```csharp
internal sealed class StubHandler(Func<HttpRequestMessage, HttpResponseMessage> responder) : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        => Task.FromResult(responder(request));
}

[Test]
public async Task GetAsync_deserializes_bexio_payload()
{
    var stub = new StubHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
    {
        Content = new StringContent("""{ "id": 1, "name_1": "Acme" }""")
    });
    var httpClient = new HttpClient(stub) { BaseAddress = new Uri("https://api.example/") };
    var config = new BexioConfiguration { BaseUri = "https://api.example/", JwtToken = "fake", AcceptHeaderFormat = "application/json" };

    using var handler = new BexioConnectionHandler(httpClient, config);
    var result = await handler.GetAsync<Contact>("2.0/contact/1");

    Assert.Multiple(() =>
    {
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data!.Id, Is.EqualTo(1));
        Assert.That(result.Data.Name1, Is.EqualTo("Acme"));
    });
}
```

### 3.4 Pattern C — `WireMock.Net` for full HTTP stubs

Use when you need multi-call orchestration (pagination, retries) or need to verify headers/body at the HTTP level.

```csharp
using WireMock.Server;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;

using var server = WireMockServer.Start();
server.Given(Request.Create().WithPath("/3.0/accounting/manual_entries").UsingGet())
      .RespondWith(Response.Create().WithStatusCode(200)
                   .WithHeader("X-Total-Results", "0")
                   .WithBody("[]"));

var config = new BexioConfiguration { BaseUri = server.Url!, JwtToken = "fake", AcceptHeaderFormat = "application/json" };
using var handler = new BexioConnectionHandler(config);
var service = new ManualEntryService(handler);

var result = await service.Get();

Assert.That(result.IsSuccess, Is.True);
Assert.That(result.Data, Is.Empty);
```

### 3.5 Unit Test Rules

1. **No network calls** from unit tests. Ever. If a test accidentally hits the internet, it belongs in `Tests/`, not `UnitTests/`.
2. **No shared mutable state** between tests. Use `[TestFixture]` per concern; create a fresh mock per test.
3. **Assert behavior, not implementation.** Verifying the request URL, verb, payload and response mapping is fine. Verifying "a private method was called" is not.
4. **One behavior per test.** Paging, error-mapping and happy-path deserialization are three separate tests.
5. **Deterministic.** No `DateTime.Now`, random data, or `Thread.Sleep`. Use fixed values / `Bogus` with a seeded `Randomizer` if you need data variety.
6. **Dispose handlers and clients.** Use `using` on `HttpClient` / `BexioConnectionHandler` when you own them.

---

## 4. End-to-End / Integration Tests (Live)

Live tests exercise the real Bexio API. They are intentionally slower and riskier — they mutate real data in a test tenant.

### 4.1 Credentials

Provide via environment variables:

- `BexioApiNet__BaseUri` — e.g., `https://api.bexio.com/` (no secrets).
- `BexioApiNet__JwtToken` — opaque JWT from the Bexio developer portal. **Never** commit this value. Provide via CI secrets or `dotenv`-style local injection.

If either variable is missing, `TestBase` calls `Assert.Ignore(...)` — the suite does **not** fail. That is intentional: agents and contributors without a Bexio account can still verify the build without false-negative test failures.

### 4.2 Categorization

`TestBase` is decorated with `[Category("E2E")]`, so every class inheriting from it is already categorized. Filter:

```bash
# Run only live E2E tests (credentials expected).
dotnet test --filter TestCategory=E2E

# Skip live tests — safe default for CI without credentials.
dotnet test --filter TestCategory!=E2E
```

### 4.3 Test Data Lifecycle

E2E tests mutate a shared test tenant. Leave it clean:

1. **Prefix** every created record with `"E2E-"` followed by the test name (e.g., `"E2E-Contact-Create-20260418-123456"`). Makes orphans trivial to spot in the Bexio UI.
2. **Create and delete within the same test.** Use `try/finally` or `[TearDown]` to delete records even when assertions fail.
3. **Never hardcode IDs** from the tenant into tests — always create what you need, assert, delete.
4. **No destructive cross-test coupling.** A test must be re-runnable on a fresh tenant without manual setup.

### 4.4 E2E Test Rules

1. **No new E2E-only behaviors** in production code. Anything a test needs must also be useful to consumers.
2. **Smoke over exhaustiveness.** E2E tests should prove "the wire format is what we think it is". Detailed edge cases belong in unit tests.
3. **Single logical assertion group.** Use `Assert.Multiple` when you genuinely have several outputs from one API response; otherwise split into separate tests.
4. **Never log JWTs or raw tokens** even in failure output.

---

## 5. Bug Fixes & Regression Tests

Per the agent rules in [`ai_instructions.md`](../../ai_instructions.md) § 8:

- **Write the failing test first.** A bug report without a reproducing test is not a bug, it is hearsay.
- Make the test fail for the right reason (feature missing / behavior wrong) — not a typo or compile error.
- Fix the bug with the smallest possible change.
- Keep the test in the suite forever as a regression guard.

---

## 6. CI Recipe

The recommended CI layout (for future GitHub Actions work):

```yaml
- name: Restore & Build
  run: dotnet restore && dotnet build --no-restore -warnaserror

- name: Unit tests (always)
  run: dotnet test --no-build --filter TestCategory=Unit

- name: E2E tests (only when credentials available)
  if: env.BEXIO_JWT != ''
  env:
    BexioApiNet__BaseUri: ${{ vars.BEXIO_BASE_URI }}
    BexioApiNet__JwtToken: ${{ secrets.BEXIO_JWT }}
  run: dotnet test --no-build --filter TestCategory=E2E
```

Pull requests should block on unit tests. E2E tests should be informational only until the tenant strategy is formalized.

---

## 7. Quick Checklist (Copy Into PR Description)

- [ ] Offline unit tests added for every new / changed public method.
- [ ] Unit tests run green without any environment variables set.
- [ ] Live E2E tests pass with credentials **or** are skipped cleanly without.
- [ ] No new test-only code in production types.
- [ ] No orphan data left in the Bexio test tenant after running E2E tests.
- [ ] No secrets in commits, test fixtures, or diagnostic output.
