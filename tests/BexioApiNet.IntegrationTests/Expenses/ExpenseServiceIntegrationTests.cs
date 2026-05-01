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
/// path composed from <see cref="ExpenseConfiguration"/> (<c>4.0/expenses</c> and
/// <c>4.0/expenses/documentnumbers</c>) reaches the handler correctly and that the
/// expected HTTP verbs are used for each operation. Stub bodies replicate the exact
/// schemas defined by the v3 OpenAPI spec so that deserialization is exercised end-to-end.
/// </summary>
[Category("Integration")]
public sealed class ExpenseServiceIntegrationTests : IntegrationTestBase
{
    private const string ExpensesPath = "/4.0/expenses";
    private const string DocNumbersPath = "/4.0/expenses/documentnumbers";

    private static readonly Guid TestExpenseId = Guid.Parse("64bf865d-988a-496d-a24f-bab2d52e4b4a");

    private const string ExpenseResponse = """
                                           {
                                               "id": "64bf865d-988a-496d-a24f-bab2d52e4b4a",
                                               "document_no": "LR-12345",
                                               "title": "Expense 42",
                                               "status": "DRAFT",
                                               "firstname_suffix": "Rexpol",
                                               "lastname_company": "Acme Ltd.",
                                               "created_at": "2026-02-12T09:53:49+00:00",
                                               "supplier_id": 1323,
                                               "paid_on": "2026-02-12",
                                               "bank_account_id": 5,
                                               "booking_account_id": 16,
                                               "currency_code": "CHF",
                                               "base_currency_code": "CHF",
                                               "exchange_rate": 1.5243546497,
                                               "amount": 80.54,
                                               "tax_man": 6.7,
                                               "tax_calc": 6.7,
                                               "tax_id": 15,
                                               "base_currency_amount": 122.74,
                                               "transaction_id": "b388a4da-7085-475a-87a0-a2acb4d8d68f",
                                               "invoice_id": "9d47155f-eac4-491e-96d0-8e187c5a7ab6",
                                               "project_id": "c14aa91c-b4f5-43ca-ae2a-882f94cd40f4",
                                               "address": {
                                                   "title": "Prof",
                                                   "salutation": "Ms",
                                                   "firstname_suffix": "John",
                                                   "lastname_company": "Acme Ltd.",
                                                   "address_line": "Mega Street 1",
                                                   "postcode": "8000",
                                                   "city": "Zurich",
                                                   "country_code": "CH",
                                                   "main_contact_id": 45,
                                                   "contact_address_id": 827,
                                                   "type": "COMPANY"
                                               },
                                               "attachment_ids": [
                                                   "06573f59-01a2-493d-9876-462deda4cee3",
                                                   "a230f087-f742-4259-925e-cf3abea5e6bf"
                                               ]
                                           }
                                           """;

    private const string ExpenseListBody = """
                                           {
                                               "data": [
                                                   {
                                                       "id": "e27be5f4-c8db-4193-92f3-1c6f1dc98f1b",
                                                       "created_at": "2019-03-23T09:53:49+00:00",
                                                       "document_no": "NO-1",
                                                       "status": "DRAFT",
                                                       "firstname_suffix": "John",
                                                       "lastname_company": "Doe",
                                                       "vendor": "John Doe",
                                                       "title": "Title 1",
                                                       "currency_code": "CHF",
                                                       "paid_on": "2019-03-07",
                                                       "booking_account_id": 387,
                                                       "net": 26.65,
                                                       "gross": 29.43,
                                                       "project_id": "c14aa91c-b4f5-43ca-ae2a-882f94cd40f4",
                                                       "chargeable_contact_id": 4,
                                                       "transaction_id": "b388a4da-7085-475a-87a0-a2acb4d8d68f",
                                                       "invoice_id": "9d47155f-eac4-491e-96d0-8e187c5a7ab6",
                                                       "attachment_ids": [
                                                           "60dd4dfa-24a3-4114-a934-108380789edc",
                                                           "a3161942-1b1d-42c1-816d-dc44cd53c7e6"
                                                       ]
                                                   }
                                               ],
                                               "paging": {
                                                   "page": 1,
                                                   "page_size": 10,
                                                   "page_count": 1,
                                                   "item_count": 1
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
    /// <c>ExpenseService.Get</c> issues a <c>GET</c> against <c>/4.0/expenses</c>
    /// and deserializes the paged envelope response on success. Asserts every field on
    /// both <see cref="ExpenseListItem"/> and <see cref="ExpensePaging"/> is populated.
    /// </summary>
    [Test]
    public async Task ExpenseService_Get_SendsGetRequestAndDeserializesEnvelope()
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
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(ExpensesPath));

            var paging = result.Data!.Paging;
            Assert.That(paging.Page, Is.EqualTo(1));
            Assert.That(paging.PageSize, Is.EqualTo(10));
            Assert.That(paging.PageCount, Is.EqualTo(1));
            Assert.That(paging.ItemCount, Is.EqualTo(1));

            Assert.That(result.Data.Data, Has.Count.EqualTo(1));
            var item = result.Data.Data[0];
            Assert.That(item.Id, Is.EqualTo(Guid.Parse("e27be5f4-c8db-4193-92f3-1c6f1dc98f1b")));
            Assert.That(item.DocumentNo, Is.EqualTo("NO-1"));
            Assert.That(item.Status, Is.EqualTo(ExpenseStatus.DRAFT));
            Assert.That(item.CreatedAt, Is.EqualTo("2019-03-23T09:53:49+00:00"));
            Assert.That(item.CurrencyCode, Is.EqualTo("CHF"));
            Assert.That(item.PaidOn, Is.EqualTo(new DateOnly(2019, 3, 7)));
            Assert.That(item.Gross, Is.EqualTo(29.43m));
            Assert.That(item.Net, Is.EqualTo(26.65m));
            Assert.That(item.Title, Is.EqualTo("Title 1"));
            Assert.That(item.FirstnameSuffix, Is.EqualTo("John"));
            Assert.That(item.LastnameCompany, Is.EqualTo("Doe"));
            Assert.That(item.Vendor, Is.EqualTo("John Doe"));
            Assert.That(item.BookingAccountId, Is.EqualTo(387));
            Assert.That(item.ProjectId, Is.EqualTo(Guid.Parse("c14aa91c-b4f5-43ca-ae2a-882f94cd40f4")));
            Assert.That(item.ChargeableContactId, Is.EqualTo(4));
            Assert.That(item.TransactionId, Is.EqualTo(Guid.Parse("b388a4da-7085-475a-87a0-a2acb4d8d68f")));
            Assert.That(item.InvoiceId, Is.EqualTo(Guid.Parse("9d47155f-eac4-491e-96d0-8e187c5a7ab6")));
            Assert.That(item.AttachmentIds, Has.Count.EqualTo(2));
            Assert.That(item.AttachmentIds[0], Is.EqualTo(Guid.Parse("60dd4dfa-24a3-4114-a934-108380789edc")));
        });
    }

    /// <summary>
    /// <c>ExpenseService.GetById</c> issues a <c>GET</c> request with the expense id
    /// in the URL path and deserializes every field of the full <see cref="Expense"/> response.
    /// </summary>
    [Test]
    public async Task ExpenseService_GetById_SendsGetRequestAndDeserializesAllFields()
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
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));

            Assert.That(result.Data, Is.Not.Null);
            var data = result.Data!;
            Assert.That(data.Id, Is.EqualTo(TestExpenseId));
            Assert.That(data.DocumentNo, Is.EqualTo("LR-12345"));
            Assert.That(data.Title, Is.EqualTo("Expense 42"));
            Assert.That(data.Status, Is.EqualTo(ExpenseStatus.DRAFT));
            Assert.That(data.FirstnameSuffix, Is.EqualTo("Rexpol"));
            Assert.That(data.LastnameCompany, Is.EqualTo("Acme Ltd."));
            Assert.That(data.CreatedAt, Is.EqualTo("2026-02-12T09:53:49+00:00"));
            Assert.That(data.SupplierId, Is.EqualTo(1323));
            Assert.That(data.PaidOn, Is.EqualTo(new DateOnly(2026, 2, 12)));
            Assert.That(data.BankAccountId, Is.EqualTo(5));
            Assert.That(data.BookingAccountId, Is.EqualTo(16));
            Assert.That(data.CurrencyCode, Is.EqualTo("CHF"));
            Assert.That(data.BaseCurrencyCode, Is.EqualTo("CHF"));
            Assert.That(data.ExchangeRate, Is.EqualTo(1.5243546497m));
            Assert.That(data.Amount, Is.EqualTo(80.54m));
            Assert.That(data.TaxMan, Is.EqualTo(6.7m));
            Assert.That(data.TaxCalc, Is.EqualTo(6.7m));
            Assert.That(data.TaxId, Is.EqualTo(15));
            Assert.That(data.BaseCurrencyAmount, Is.EqualTo(122.74m));
            Assert.That(data.TransactionId, Is.EqualTo(Guid.Parse("b388a4da-7085-475a-87a0-a2acb4d8d68f")));
            Assert.That(data.InvoiceId, Is.EqualTo(Guid.Parse("9d47155f-eac4-491e-96d0-8e187c5a7ab6")));
            Assert.That(data.ProjectId, Is.EqualTo(Guid.Parse("c14aa91c-b4f5-43ca-ae2a-882f94cd40f4")));
            Assert.That(data.AttachmentIds, Has.Count.EqualTo(2));

            Assert.That(data.Address, Is.Not.Null);
            var address = data.Address!;
            Assert.That(address.Title, Is.EqualTo("Prof"));
            Assert.That(address.Salutation, Is.EqualTo("Ms"));
            Assert.That(address.FirstnameSuffix, Is.EqualTo("John"));
            Assert.That(address.LastnameCompany, Is.EqualTo("Acme Ltd."));
            Assert.That(address.AddressLine, Is.EqualTo("Mega Street 1"));
            Assert.That(address.Postcode, Is.EqualTo("8000"));
            Assert.That(address.City, Is.EqualTo("Zurich"));
            Assert.That(address.CountryCode, Is.EqualTo("CH"));
            Assert.That(address.MainContactId, Is.EqualTo(45));
            Assert.That(address.ContactAddressId, Is.EqualTo(827));
            Assert.That(address.Type, Is.EqualTo(ExpenseAddressType.COMPANY));
        });
    }

    /// <summary>
    /// <c>ExpenseService.GetDocNumbers</c> routes to <c>/4.0/expenses/documentnumbers</c>
    /// and passes the proposed number via the <c>document_no</c> query parameter.
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
    /// the serialized <see cref="ExpenseCreate"/> payload with the schema-required fields.
    /// </summary>
    [Test]
    public async Task ExpenseService_Create_SendsPostRequestWithPayload()
    {
        Server
            .Given(Request.Create().WithPath(ExpensesPath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(201).WithBody(ExpenseResponse));

        var service = new ExpenseService(ConnectionHandler);

        var payload = new ExpenseCreate(
            PaidOn: new DateOnly(2026, 2, 12),
            Amount: 80.54m,
            CurrencyCode: "CHF",
            AttachmentIds: [Guid.Parse("06573f59-01a2-493d-9876-462deda4cee3")],
            SupplierId: 1323,
            Title: "Expense 42",
            BankAccountId: 5,
            BookingAccountId: 16,
            TaxId: 15,
            Address: new ExpenseAddress("Acme Ltd.", ExpenseAddressType.COMPANY));

        var result = await service.Create(payload, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(request.Method, Is.EqualTo("POST"));
            Assert.That(request.AbsolutePath, Is.EqualTo(ExpensesPath));
            Assert.That(request.Body, Does.Contain("\"paid_on\":\"2026-02-12\""));
            Assert.That(request.Body, Does.Contain("\"amount\":80.54"));
            Assert.That(request.Body, Does.Contain("\"currency_code\":\"CHF\""));
            Assert.That(request.Body, Does.Contain("\"supplier_id\":1323"));
            Assert.That(request.Body, Does.Contain("\"attachment_ids\""));
        });
    }

    /// <summary>
    /// <c>ExpenseService.Update</c> sends a <c>PUT</c> request against
    /// <c>/4.0/expenses/{id}</c> with the schema-required <see cref="ExpenseUpdate"/> body.
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
            PaidOn: new DateOnly(2026, 2, 12),
            CurrencyCode: "CHF",
            Amount: 80.54m,
            AttachmentIds: [Guid.Parse("06573f59-01a2-493d-9876-462deda4cee3")],
            SupplierId: 1323,
            DocumentNo: "LR-12345",
            Title: "Expense 42",
            BankAccountId: 5,
            BookingAccountId: 16,
            TaxId: 15);

        var result = await service.Update(TestExpenseId, payload, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(request.Method, Is.EqualTo("PUT"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
            Assert.That(request.Body, Does.Contain("\"document_no\":\"LR-12345\""));
        });
    }

    /// <summary>
    /// <c>ExpenseService.Actions</c> sends a <c>POST</c> request to the
    /// <c>/{id}/actions</c> sub-path with the action name in the body.
    /// </summary>
    [Test]
    public async Task ExpenseService_Actions_SendsPostRequestToActionsPath()
    {
        var expectedPath = $"{ExpensesPath}/{TestExpenseId}/actions";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(ExpenseResponse));

        var service = new ExpenseService(ConnectionHandler);

        var result = await service.Actions(
            TestExpenseId,
            new ExpenseActionRequest(ExpenseAction.DUPLICATE),
            TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("POST"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
            Assert.That(request.Body, Does.Contain("\"action\":\"DUPLICATE\""));
        });
    }

    /// <summary>
    /// <c>ExpenseService.UpdateBookings</c> sends a <c>PUT</c> request against
    /// <c>/{id}/bookings/{status}</c> with no request body — the transition is encoded in the URL.
    /// </summary>
    [Test]
    public async Task ExpenseService_UpdateBookings_SendsPutRequestToBookingsPath()
    {
        var expectedPath = $"{ExpensesPath}/{TestExpenseId}/bookings/DONE";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingPut())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(ExpenseResponse));

        var service = new ExpenseService(ConnectionHandler);

        var result = await service.UpdateBookings(TestExpenseId, ExpenseBookingStatus.DONE, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
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
