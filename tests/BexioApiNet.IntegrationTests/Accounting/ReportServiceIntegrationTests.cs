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

using BexioApiNet.Models;
using BexioApiNet.Services.Connectors.Accounting;

namespace BexioApiNet.IntegrationTests.Accounting;

/// <summary>
/// Integration tests covering the <see cref="ReportService" /> entry points against WireMock
/// stubs. Verifies the path composed from <see cref="ReportConfiguration" />
/// (<c>3.0/accounting/journal</c>) reaches the handler correctly, that the expected
/// HTTP verb and query parameters are used, and that fully populated JSON responses
/// (matching the Bexio v3 OpenAPI <c>v3JournalResponse</c> schema) deserialize into every
/// field of the <see cref="BexioApiNet.Abstractions.Models.Accounting.Reports.Journal"/>
/// model.
/// </summary>
public sealed class ReportServiceIntegrationTests : IntegrationTestBase
{
    private const string JournalPath = "/3.0/accounting/journal";

    private const string JournalListResponse = """
        [
            {
                "id": 1,
                "ref_id": 13,
                "ref_uuid": "11111111-1111-4111-9111-111111111111",
                "ref_class": "KbInvoice",
                "date": "2019-02-17T00:00:00+02:00",
                "debit_account_id": 77,
                "credit_account_id": 139,
                "description": "Test journal entry",
                "amount": 328.25,
                "currency_id": 1,
                "currency_factor": 1.0,
                "base_currency_id": 1,
                "base_currency_amount": 328.25
            }
        ]
        """;

    /// <summary>
    /// <c>ReportService.GetJournal()</c> must issue a <c>GET</c> request against
    /// <c>/3.0/accounting/journal</c> and deserialize a fully populated
    /// <c>v3JournalResponse</c> array — every field of the
    /// <see cref="BexioApiNet.Abstractions.Models.Accounting.Reports.Journal"/> record
    /// (id, ref_id, ref_uuid, ref_class, date, debit_account_id, credit_account_id,
    /// description, amount, currency_id, currency_factor, base_currency_id,
    /// base_currency_amount) must round-trip from the JSON body.
    /// </summary>
    [Test]
    public async Task ReportService_GetJournal_DeserializesAllFields()
    {
        Server
            .Given(Request.Create().WithPath(JournalPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(JournalListResponse));

        var service = new ReportService(ConnectionHandler);

        var result = await service.GetJournal(cancellationToken: TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(JournalPath));
            Assert.That(result.Data, Has.Count.EqualTo(1));

            var entry = result.Data![0];
            Assert.That(entry.Id, Is.EqualTo(1));
            Assert.That(entry.RefId, Is.EqualTo(13));
            Assert.That(entry.RefUuid, Is.EqualTo("11111111-1111-4111-9111-111111111111"));
            Assert.That(entry.RefClass, Is.EqualTo("KbInvoice"));
            Assert.That(entry.DebitAccountId, Is.EqualTo(77));
            Assert.That(entry.CreditAccountId, Is.EqualTo(139));
            Assert.That(entry.Description, Is.EqualTo("Test journal entry"));
            Assert.That(entry.Amount, Is.EqualTo(328.25m));
            Assert.That(entry.CurrencyId, Is.EqualTo(1));
            Assert.That(entry.CurrencyFactor, Is.EqualTo(1.0m));
            Assert.That(entry.BaseCurrencyId, Is.EqualTo(1));
            Assert.That(entry.BaseCurrencyAmount, Is.EqualTo(328.25m));
        });
    }

    /// <summary>
    /// <c>ReportService.GetJournal</c> with a <see cref="QueryParameterJournal" /> must
    /// forward the <c>from</c>, <c>to</c>, <c>account_uuid</c>, <c>limit</c> and <c>offset</c>
    /// query parameters onto the URL — matching the v3 OpenAPI spec query parameter list.
    /// </summary>
    [Test]
    public async Task ReportService_GetJournal_WithFullQuery_AppendsAllQueryParameters()
    {
        Server
            .Given(Request.Create().WithPath(JournalPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("[]"));

        var service = new ReportService(ConnectionHandler);

        var result = await service.GetJournal(
            new QueryParameterJournal(
                From: new DateOnly(2024, 1, 1),
                To: new DateOnly(2024, 12, 31),
                AccountUuid: "11111111-1111-4111-9111-111111111111",
                Limit: 100,
                Offset: 50),
            cancellationToken: TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(JournalPath));
            Assert.That(request.RawQuery, Does.Contain("from=2024-01-01"));
            Assert.That(request.RawQuery, Does.Contain("to=2024-12-31"));
            Assert.That(request.RawQuery, Does.Contain("account_uuid=11111111-1111-4111-9111-111111111111"));
            Assert.That(request.RawQuery, Does.Contain("limit=100"));
            Assert.That(request.RawQuery, Does.Contain("offset=50"));
        });
    }
}
