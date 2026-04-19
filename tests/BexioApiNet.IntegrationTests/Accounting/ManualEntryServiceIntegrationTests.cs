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

using BexioApiNet.Abstractions.Models.Accounting.ManualEntries.Enums;
using BexioApiNet.Abstractions.Models.Accounting.ManualEntries.Views;
using BexioApiNet.Services.Connectors.Accounting;

namespace BexioApiNet.IntegrationTests.Accounting;

/// <summary>
/// Integration tests covering the CRUD entry points of <see cref="ManualEntryService"/> against
/// WireMock stubs. Verifies the path composed from <see cref="ManualEntryConfiguration"/>
/// (<c>3.0/accounting/manual_entries</c>) reaches the handler correctly and that the expected
/// HTTP verbs are used.
/// </summary>
public sealed class ManualEntryServiceIntegrationTests : IntegrationTestBase
{
    private const string ManualEntriesPath = "/3.0/accounting/manual_entries";

    private const string ManualEntryResponse = """
        {
            "id": 1,
            "type": "manual_single_entry",
            "booking_type": "fiscal",
            "date": "2024-01-01",
            "reference_nr": "TEST",
            "entries": [],
            "is_locked": false,
            "locked_info": ""
        }
        """;

    /// <summary>
    /// <c>ManualEntryService.Get()</c> must issue a <c>GET</c> request against
    /// <c>/3.0/accounting/manual_entries</c> and return a successful <c>ApiResult</c>
    /// when the server returns an empty array.
    /// </summary>
    [Test]
    public async Task ManualEntryService_Get_SendsGetRequest()
    {
        Server
            .Given(Request.Create().WithPath(ManualEntriesPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("[]"));

        var service = new ManualEntryService(ConnectionHandler);

        var result = await service.Get(cancellationToken: TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(ManualEntriesPath));
        });
    }

    /// <summary>
    /// <c>ManualEntryService.Create</c> must send a <c>POST</c> request whose body is the
    /// serialized <see cref="ManualEntryEntryCreate"/> payload, and must surface the returned
    /// <see cref="BexioApiNet.Abstractions.Models.Accounting.ManualEntries.ManualEntry"/> on
    /// success.
    /// </summary>
    [Test]
    public async Task ManualEntryService_Create_SendsPostRequest()
    {
        Server
            .Given(Request.Create().WithPath(ManualEntriesPath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(201).WithBody(ManualEntryResponse));

        var service = new ManualEntryService(ConnectionHandler);

        var payload = new ManualEntryEntryCreate(
            Type: ManualEntryType.manual_single_entry,
            Date: new DateOnly(2024, 1, 1),
            ReferenceNr: "TEST",
            Entries: new[]
            {
                new ManualEntryCreate(
                    DebitAccountId: 1,
                    CreditAccountId: 2,
                    TaxId: null,
                    TaxAccountId: null,
                    Description: "Integration smoke",
                    Amount: 10m,
                    CurrencyId: 1,
                    CurrencyFactor: 1m)
            });

        var result = await service.Create(payload, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data!.Id, Is.EqualTo(1));
            Assert.That(request.Method, Is.EqualTo("POST"));
            Assert.That(request.AbsolutePath, Is.EqualTo(ManualEntriesPath));
            Assert.That(request.Body, Does.Contain("\"reference_nr\":\"TEST\""));
        });
    }

    /// <summary>
    /// <c>ManualEntryService.Delete</c> must issue a <c>DELETE</c> request that includes the
    /// target id in the URL path and must surface the <c>2xx</c> response as a successful
    /// <c>ApiResult</c>.
    /// </summary>
    [Test]
    public async Task ManualEntryService_Delete_SendsDeleteRequest()
    {
        const int idToDelete = 1;
        var expectedPath = $"{ManualEntriesPath}/{idToDelete}";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingDelete())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("true"));

        var service = new ManualEntryService(ConnectionHandler);

        var result = await service.Delete(idToDelete, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("DELETE"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
        });
    }
}
