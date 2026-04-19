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
using BexioApiNet.Interfaces.Connectors.Banking;
using BexioApiNet.Services.Connectors.Banking;

namespace BexioApiNet.E2eTests.Tests.Banking.OutgoingPayments;

/// <summary>
/// Dedicated live E2E base for the <see cref="OutgoingPaymentService"/>. Builds the
/// service against a real <see cref="BexioConnectionHandler"/> so the tests don't
/// depend on <c>IBexioApiClient.BankingOutgoingPayments</c> being wired up (that
/// aggregate wiring is tracked in sub-issue #49).
/// <para>
/// Tests are skipped when <c>BexioApiNet__BaseUri</c> or <c>BexioApiNet__JwtToken</c>
/// env vars are missing. A <c>BexioApiNet__OutgoingPaymentBillId</c> env var may be
/// supplied for tests that need a real bill id; otherwise dependent tests are skipped.
/// </para>
/// </summary>
[Category("E2E")]
public abstract class OutgoingPaymentE2eTestBase
{
    /// <summary>
    /// Live outgoing-payment service for each test, or <see langword="null"/> when the
    /// test was skipped because credentials are missing.
    /// </summary>
    protected IOutgoingPaymentService? OutgoingPayments;

    private BexioConnectionHandler? _connectionHandler;

    /// <summary>
    /// Optional bill id populated from <c>BexioApiNet__OutgoingPaymentBillId</c>. Tests
    /// that require a specific bill should call <see cref="RequireBillId"/> to auto-skip
    /// when this is not provided.
    /// </summary>
    protected Guid? BillId;

    /// <summary>
    /// Reads credentials from environment variables, builds a dedicated connection
    /// handler and <see cref="OutgoingPaymentService"/>, and parses the optional bill
    /// id. Calls <see cref="Assert.Ignore(string)"/> when credentials are missing.
    /// </summary>
    [SetUp]
    public void Setup()
    {
        var baseUri = Environment.GetEnvironmentVariable("BexioApiNet__BaseUri");
        var jwtToken = Environment.GetEnvironmentVariable("BexioApiNet__JwtToken");

        if (string.IsNullOrWhiteSpace(baseUri) || string.IsNullOrWhiteSpace(jwtToken))
        {
            Assert.Ignore("credentials not configured");
            return;
        }

        _connectionHandler = new BexioConnectionHandler(
            new BexioConfiguration
            {
                BaseUri = baseUri,
                JwtToken = jwtToken,
                AcceptHeaderFormat = ApiAcceptHeaders.JsonFormatted
            });

        OutgoingPayments = new OutgoingPaymentService(_connectionHandler);

        if (Guid.TryParse(Environment.GetEnvironmentVariable("BexioApiNet__OutgoingPaymentBillId"), out var billId))
            BillId = billId;
    }

    /// <summary>
    /// Disposes the dedicated connection handler if it was created for this test.
    /// </summary>
    [TearDown]
    public void Teardown()
    {
        _connectionHandler?.Dispose();
    }

    /// <summary>
    /// Helper for tests that need a bill id — skips the test with
    /// <see cref="Assert.Ignore(string)"/> when <c>BexioApiNet__OutgoingPaymentBillId</c>
    /// is not configured.
    /// </summary>
    /// <returns>The configured bill id.</returns>
    protected Guid RequireBillId()
    {
        if (BillId is { } id)
            return id;

        Assert.Ignore("BexioApiNet__OutgoingPaymentBillId not configured");
        return default;
    }
}
