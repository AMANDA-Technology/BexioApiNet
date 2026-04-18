/*
MIT License

Copyright (c) 2022 Philip Näf <philip.naef@amanda-technology.ch>
Copyright (c) 2022 Manuel Gysin <manuel.gysin@amanda-technology.ch>

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using BexioApiNet.Abstractions.Enums.Api;
using BexioApiNet.Services.Connectors.Accounting;
using BexioApiNet.Services.Connectors.Banking;

namespace BexioApiNet.Tests;

/// <summary>
/// Base class for live end-to-end bexio API tests. Tests inheriting from this class
/// call the real Bexio API and are automatically skipped when credentials are absent.
/// Categorize with <c>[Category("E2E")]</c> by inheriting, so CI runs can filter with
/// <c>dotnet test --filter TestCategory!=E2E</c>.
/// </summary>
[Category("E2E")]
public class TestBase
{
    /// <summary>
    /// Default instance of bexio API client. Null when credentials are missing and the test is skipped.
    /// </summary>
    protected IBexioApiClient? BexioApiClient;

    /// <summary>
    /// Setup. Reads <c>BexioApiNet__BaseUri</c> and <c>BexioApiNet__JwtToken</c> from environment
    /// variables. Calls <see cref="Assert.Ignore(string)"/> if either is missing so the test suite
    /// does not fail CI or AI agent runs that lack live credentials.
    /// </summary>
    [SetUp]
    public void Setup()
    {
        var baseUri = Environment.GetEnvironmentVariable("BexioApiNet__BaseUri");
        var jwtToken = Environment.GetEnvironmentVariable("BexioApiNet__JwtToken");

        if (string.IsNullOrWhiteSpace(baseUri) || string.IsNullOrWhiteSpace(jwtToken))
        {
            Assert.Ignore("Missing Bexio API credentials (BexioApiNet__BaseUri or BexioApiNet__JwtToken). Skipping live E2E test.");
            return;
        }

        var connectionHandler = new BexioConnectionHandler(
            new BexioConfiguration
            {
                BaseUri = baseUri,
                JwtToken = jwtToken,
                AcceptHeaderFormat = ApiAcceptHeaders.JsonFormatted
            });

        BexioApiClient = new BexioApiClient(
            connectionHandler,
            new BankAccountService(connectionHandler),
            new AccountService(connectionHandler),
            new CurrencyService(connectionHandler),
            new ManualEntryService(connectionHandler),
            new TaxService(connectionHandler));
    }

    /// <summary>
    /// Teardown. Disposes of the client if it was created.
    /// </summary>
    [TearDown]
    public void Teardown()
    {
        BexioApiClient?.Dispose();
    }
}
