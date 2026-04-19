/*
MIT License

Copyright (c) 2022 Philip Naef <philip.naef@amanda-technology.ch>
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

using BexioApiNet.Abstractions.Models.Banking.OutgoingPayments.Enums;
using BexioApiNet.Abstractions.Models.Banking.OutgoingPayments.Views;
using BexioApiNet.Models;
using BexioApiNet.Services.Connectors.Banking;

namespace BexioApiNet.IntegrationTests.Smoke.Banking;

/// <summary>
///     Smoke tests covering the CRUD entry points of <see cref="OutgoingPaymentService" /> against
///     WireMock stubs. Verifies the path composed from <see cref="OutgoingPaymentConfiguration" />
///     (<c>4.0/purchase/outgoing-payments</c>) reaches the handler correctly and that the expected
///     HTTP verbs are used for each operation (Get, GetById, Create, Update, Delete).
///     Note: Update uses <c>PUT</c> on the collection path (no <c>/{id}</c>) per the Bexio API spec.
/// </summary>
public sealed class OutgoingPaymentSmokeTests : IntegrationTestBase
{
    private const string OutgoingPaymentsPath = "/4.0/purchase/outgoing-payments";

    private const string OutgoingPaymentResponse = """
                                                   {
                                                       "id": "b2c3d4e5-f6a7-8901-bcde-f12345678901",
                                                       "status": "PENDING",
                                                       "created_at": "2024-01-15T10:00:00+01:00",
                                                       "bill_id": "c3d4e5f6-a7b8-9012-cdef-123456789012",
                                                       "payment_type": "MANUAL",
                                                       "execution_date": "2024-06-01",
                                                       "amount": 250.00,
                                                       "currency_code": "CHF",
                                                       "exchange_rate": 1.0,
                                                       "note": "Test payment",
                                                       "sender_bank_account_id": 1,
                                                       "sender_iban": null,
                                                       "sender_name": null,
                                                       "sender_street": null,
                                                       "sender_house_no": null,
                                                       "sender_city": null,
                                                       "sender_postcode": null,
                                                       "sender_country_code": null,
                                                       "sender_bc_no": null,
                                                       "sender_bank_no": null,
                                                       "sender_bank_name": null,
                                                       "receiver_account_no": null,
                                                       "receiver_iban": null,
                                                       "receiver_name": null,
                                                       "receiver_street": null,
                                                       "receiver_house_no": null,
                                                       "receiver_city": null,
                                                       "receiver_postcode": null,
                                                       "receiver_country_code": null,
                                                       "receiver_bc_no": null,
                                                       "receiver_bank_no": null,
                                                       "receiver_bank_name": null,
                                                       "fee_type": null,
                                                       "is_salary_payment": false,
                                                       "reference_no": null,
                                                       "message": null,
                                                       "booking_text": null,
                                                       "banking_payment_id": null,
                                                       "banking_payment_entry_id": null,
                                                       "transaction_id": null
                                                   }
                                                   """;

    private const string OutgoingPaymentListResponse = """
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

    private static readonly Guid TestPaymentId = Guid.Parse("b2c3d4e5-f6a7-8901-bcde-f12345678901");
    private static readonly Guid TestBillId = Guid.Parse("c3d4e5f6-a7b8-9012-cdef-123456789012");

    /// <summary>
    ///     <c>OutgoingPaymentService.Get</c> must issue a <c>GET</c> against
    ///     <c>/4.0/purchase/outgoing-payments</c> with the required <c>bill_id</c> query parameter
    ///     and return a successful <c>ApiResult</c> when the server responds with an empty page.
    /// </summary>
    [Test]
    public async Task OutgoingPaymentService_Get_SendsGetRequestWithBillIdQueryParameter()
    {
        Server
            .Given(Request.Create().WithPath(OutgoingPaymentsPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(OutgoingPaymentListResponse));

        var service = new OutgoingPaymentService(ConnectionHandler);

        var queryParameter = new QueryParameterOutgoingPayment(TestBillId);

        var result = await service.Get(queryParameter, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data!.Data, Is.Empty);
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(OutgoingPaymentsPath));
        });
    }

    /// <summary>
    ///     <c>OutgoingPaymentService.GetById</c> must issue a <c>GET</c> request that includes the target
    ///     <see cref="Guid" /> in the URL path and surface the returned outgoing payment on success.
    /// </summary>
    [Test]
    public async Task OutgoingPaymentService_GetById_SendsGetRequestWithIdInPath()
    {
        var expectedPath = $"{OutgoingPaymentsPath}/{TestPaymentId}";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(OutgoingPaymentResponse));

        var service = new OutgoingPaymentService(ConnectionHandler);

        var result = await service.GetById(TestPaymentId, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data!.Id, Is.EqualTo(TestPaymentId));
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
        });
    }

    /// <summary>
    ///     <c>OutgoingPaymentService.Create</c> must send a <c>POST</c> request whose body is the
    ///     serialized <see cref="OutgoingPaymentCreate" /> payload, and must surface the returned
    ///     outgoing payment on success.
    /// </summary>
    [Test]
    public async Task OutgoingPaymentService_Create_SendsPostRequestWithPayload()
    {
        Server
            .Given(Request.Create().WithPath(OutgoingPaymentsPath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(201).WithBody(OutgoingPaymentResponse));

        var service = new OutgoingPaymentService(ConnectionHandler);

        var payload = new OutgoingPaymentCreate(
            TestBillId,
            OutgoingPaymentType.MANUAL,
            new DateOnly(2024, 6, 1),
            250.00m,
            "CHF",
            1.0m,
            1,
            false,
            "Test payment");

        var result = await service.Create(payload, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(request.Method, Is.EqualTo("POST"));
            Assert.That(request.AbsolutePath, Is.EqualTo(OutgoingPaymentsPath));
            Assert.That(request.Body, Does.Contain("\"payment_type\":\"MANUAL\""));
            Assert.That(request.Body, Does.Contain("\"currency_code\":\"CHF\""));
        });
    }

    /// <summary>
    ///     <c>OutgoingPaymentService.Update</c> must send a <c>PUT</c> request against
    ///     <c>/4.0/purchase/outgoing-payments</c> (collection path, no <c>/{id}</c>) whose body is the
    ///     serialized <see cref="OutgoingPaymentUpdate" /> payload containing the <c>payment_id</c>.
    ///     This is correct per the Bexio API spec.
    /// </summary>
    [Test]
    public async Task OutgoingPaymentService_Update_SendsPutRequestToCollectionPath()
    {
        Server
            .Given(Request.Create().WithPath(OutgoingPaymentsPath).UsingPut())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(OutgoingPaymentResponse));

        var service = new OutgoingPaymentService(ConnectionHandler);

        var payload = new OutgoingPaymentUpdate(
            TestPaymentId,
            new DateOnly(2024, 6, 15),
            300.00m,
            false);

        var result = await service.Update(payload, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(request.Method, Is.EqualTo("PUT"));
            Assert.That(request.AbsolutePath, Is.EqualTo(OutgoingPaymentsPath));
            Assert.That(request.Body, Does.Contain($"\"payment_id\":\"{TestPaymentId}\""));
        });
    }

    /// <summary>
    ///     <c>OutgoingPaymentService.Delete</c> must issue a <c>DELETE</c> request that includes the
    ///     target <see cref="Guid" /> in the URL path.
    /// </summary>
    [Test]
    public async Task OutgoingPaymentService_Delete_SendsDeleteRequestWithIdInPath()
    {
        var expectedPath = $"{OutgoingPaymentsPath}/{TestPaymentId}";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingDelete())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("{\"success\":true}"));

        var service = new OutgoingPaymentService(ConnectionHandler);

        var result = await service.Delete(TestPaymentId, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("DELETE"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
        });
    }
}