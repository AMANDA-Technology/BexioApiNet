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

using BexioApiNet.Abstractions.Models.Expenses.Expenses;
using BexioApiNet.Abstractions.Models.Expenses.Expenses.Enums;
using BexioApiNet.Abstractions.Models.Expenses.Expenses.Views;
using BexioApiNet.Services.Connectors.Expenses;

namespace BexioApiNet.IntegrationTests.Expenses;

/// <summary>
/// Integration tests for <see cref="ExpenseService"/> against WireMock stubs. Verifies the
/// path composed from <see cref="ExpenseConfiguration"/> (<c>4.0/expenses/expenses</c> and
/// <c>4.0/expenses/documentnumbers/expenses</c>) reaches the handler correctly and that the
/// expected HTTP verbs are used for each operation — including <c>PUT</c> for Update and
/// UpdateBookings, which differs from the v2.0 Invoice convention.
/// </summary>
[Category("Integration")]
public sealed class ExpenseServiceIntegrationTests : IntegrationTestBase
{
    private const string ExpensesPath = "/4.0/expenses/expenses";
    private const string DocNumbersPath = "/4.0/expenses/documentnumbers/expenses";

    private static readonly Guid TestExpenseId = Guid.Parse("64bf865d-988a-496d-a24f-bab2d52e4b4a");

    private const string ExpenseResponse = """
                                           {
                                               "id": "64bf865d-988a-496d-a24f-bab2d52e4b4a",
                                               "document_no": "LR-12345",
                                               "title": "Expense 42",
                                               "status": "DRAFT",
                                               "lastname_company": "Organisation",
                                               "created_at": "2026-02-12T09:53:49",
                                               "supplier_id": 1323,
                                               "contact_partner_id": 647,
                                               "expense_date": "2026-02-12",
                                               "due_date": "2026-03-14",
                                               "manual_amount": true,
                                               "currency_code": "CHF",
                                               "base_currency_code": "CHF",
                                               "item_net": false,
                                               "split_into_line_items": true,
                                               "overdue": false,
                                               "address": {
                                                   "lastname_company": "Acme",
                                                   "type": "COMPANY"
                                               },
                                               "line_items": [
                                                   {
                                                       "id": "2d267f64-6b94-4109-818e-c54515837004",
                                                       "position": 0,
                                                       "amount": 56.8
                                                   }
                                               ],
                                               "discounts": [],
                                               "attachment_ids": []
                                           }
                                           """;

    private const string ExpenseListBody = """
                                           {
                                               "data": [],
                                               "paging": {
                                                   "page": 1,
                                                   "page_size": 100,
                                                   "page_count": 0,
                                                   "item_count": 0
                                               }
                                           }
                                           """;

    private const string DocumentNumberBody = """
                                              {
                                                  "valid": false,
                                                  "next_available_no": "AB-1235"
                                              }
                                              """;

    /// <summary>
    /// <c>ExpenseService.Get</c> issues a <c>GET</c> against <c>/4.0/expenses/expenses</c>
    /// and deserializes the paged envelope response on success.
    /// </summary>
    [Test]
    public async Task ExpenseService_Get_SendsGetRequest()
    {
        Server
            .Given(Request.Create().WithPath(ExpensesPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(ExpenseListBody));

        var service = new ExpenseService(ConnectionHandler);

        var result = await service.Get(cancellationToken: TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data!.Data, Is.Empty);
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(ExpensesPath));
        });
    }

    /// <summary>
    /// <c>ExpenseService.GetById</c> issues a <c>GET</c> request with the expense id
    /// in the URL path and surfaces the returned expense on success.
    /// </summary>
    [Test]
    public async Task ExpenseService_GetById_SendsGetRequestWithIdInPath()
    {
        var expectedPath = $"{ExpensesPath}/{TestExpenseId}";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(ExpenseResponse));

        var service = new ExpenseService(ConnectionHandler);

        var result = await service.GetById(TestExpenseId, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data!.Id, Is.EqualTo(TestExpenseId));
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
        });
    }

    /// <summary>
    /// <c>ExpenseService.GetDocNumbers</c> routes to
    /// <c>/4.0/expenses/documentnumbers/expenses</c> and passes the proposed number via
    /// the <c>document_no</c> query parameter.
    /// </summary>
    [Test]
    public async Task ExpenseService_GetDocNumbers_SendsGetRequestToDocNumberEndpoint()
    {
        Server
            .Given(Request.Create().WithPath(DocNumbersPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(DocumentNumberBody));

        var service = new ExpenseService(ConnectionHandler);

        var result = await service.GetDocNumbers("AB-1234", TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data!.Valid, Is.False);
            Assert.That(result.Data.NextAvailableNo, Is.EqualTo("AB-1235"));
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(DocNumbersPath));
            Assert.That(request.Url, Does.Contain("document_no=AB-1234"));
        });
    }

    /// <summary>
    /// <c>ExpenseService.Create</c> sends a <c>POST</c> request whose body contains
    /// the serialized <see cref="ExpenseCreate"/> payload.
    /// </summary>
    [Test]
    public async Task ExpenseService_Create_SendsPostRequestWithPayload()
    {
        Server
            .Given(Request.Create().WithPath(ExpensesPath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(201).WithBody(ExpenseResponse));

        var service = new ExpenseService(ConnectionHandler);

        var payload = new ExpenseCreate(
            SupplierId: 1323,
            ContactPartnerId: 647,
            CurrencyCode: "CHF",
            Address: new ExpenseAddress("Acme", ExpenseAddressType.COMPANY),
            ExpenseDate: new DateOnly(2026, 2, 12),
            DueDate: new DateOnly(2026, 3, 14),
            ManualAmount: false,
            ItemNet: true,
            LineItems: [new ExpenseLineItem(Position: 0, Amount: 56.8m)],
            Discounts: [],
            AttachmentIds: []);

        var result = await service.Create(payload, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(request.Method, Is.EqualTo("POST"));
            Assert.That(request.AbsolutePath, Is.EqualTo(ExpensesPath));
            Assert.That(request.Body, Does.Contain("\"currency_code\":\"CHF\""));
            Assert.That(request.Body, Does.Contain("\"supplier_id\":1323"));
        });
    }

    /// <summary>
    /// <c>ExpenseService.Update</c> sends a <c>PUT</c> request against
    /// <c>/4.0/expenses/expenses/{id}</c> — v4.0 uses <c>PUT</c> for updates, not <c>POST</c>.
    /// </summary>
    [Test]
    public async Task ExpenseService_Update_SendsPutRequestWithIdInPath()
    {
        var expectedPath = $"{ExpensesPath}/{TestExpenseId}";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingPut())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(ExpenseResponse));

        var service = new ExpenseService(ConnectionHandler);

        var payload = new ExpenseUpdate(
            SupplierId: 1323,
            ContactPartnerId: 647,
            CurrencyCode: "CHF",
            Address: new ExpenseAddress("Acme", ExpenseAddressType.COMPANY),
            ExpenseDate: new DateOnly(2026, 2, 12),
            DueDate: new DateOnly(2026, 3, 14),
            ManualAmount: false,
            ItemNet: true,
            SplitIntoLineItems: true,
            LineItems: [new ExpenseLineItem(Position: 0, Amount: 56.8m)],
            Discounts: [],
            AttachmentIds: []);

        var result = await service.Update(TestExpenseId, payload, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(request.Method, Is.EqualTo("PUT"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
        });
    }

    /// <summary>
    /// <c>ExpenseService.Delete</c> issues a <c>DELETE</c> request that includes the
    /// expense id in the URL path.
    /// </summary>
    [Test]
    public async Task ExpenseService_Delete_SendsDeleteRequestWithIdInPath()
    {
        var expectedPath = $"{ExpensesPath}/{TestExpenseId}";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingDelete())
            .RespondWith(Response.Create().WithStatusCode(204));

        var service = new ExpenseService(ConnectionHandler);

        var result = await service.Delete(TestExpenseId, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("DELETE"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
        });
    }
}
