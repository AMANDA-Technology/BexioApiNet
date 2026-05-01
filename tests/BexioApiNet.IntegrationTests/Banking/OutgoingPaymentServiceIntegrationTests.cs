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

using BexioApiNet.Abstractions.Models.Banking.OutgoingPayments;
using BexioApiNet.Abstractions.Models.Banking.OutgoingPayments.Enums;
using BexioApiNet.Abstractions.Models.Banking.OutgoingPayments.Views;
using BexioApiNet.Models;
using BexioApiNet.Services.Connectors.Banking;

namespace BexioApiNet.IntegrationTests.Banking;

/// <summary>
/// Integration tests covering the CRUD entry points of <see cref="OutgoingPaymentService" /> against
/// WireMock stubs. Verifies the path composed from <see cref="OutgoingPaymentConfiguration" />
/// (<c>4.0/purchase/outgoing-payments</c>) reaches the handler correctly, the expected
/// HTTP verbs are used for each operation, and every field of the OpenAPI v4.0 schema
/// (Bexio <c>OutgoingPayment</c> + list envelope) deserialises into the C# model.
/// Note: Update uses <c>PUT</c> on the collection path (no <c>/{id}</c>) per the Bexio API spec.
/// </summary>
public sealed class OutgoingPaymentServiceIntegrationTests : IntegrationTestBase
{
    private const string OutgoingPaymentsPath = "/4.0/purchase/outgoing-payments";

    /// <summary>
    /// Schema-accurate JSON for a fully populated <see cref="OutgoingPayment"/> as returned
    /// by <c>GET /4.0/purchase/outgoing-payments/{id}</c>, <c>POST /4.0/purchase/outgoing-payments</c>
    /// and <c>PUT /4.0/purchase/outgoing-payments</c>. Every property defined by the OpenAPI
    /// schema is populated.
    /// </summary>
    private const string OutgoingPaymentJson = """
                                               {
                                                   "id": "b2c3d4e5-f6a7-8901-bcde-f12345678901",
                                                   "status": "PENDING",
                                                   "created_at": "2026-04-15T10:00:00+02:00",
                                                   "bill_id": "c3d4e5f6-a7b8-9012-cdef-123456789012",
                                                   "payment_type": "IBAN",
                                                   "execution_date": "2026-06-01",
                                                   "amount": 250.00,
                                                   "currency_code": "CHF",
                                                   "exchange_rate": 1.0,
                                                   "note": "Test payment",
                                                   "sender_bank_account_id": 1,
                                                   "sender_iban": "CH9300762011623852957",
                                                   "sender_name": "Acme AG",
                                                   "sender_street": "Bahnhofstrasse",
                                                   "sender_house_no": "1",
                                                   "sender_city": "Zurich",
                                                   "sender_postcode": "8001",
                                                   "sender_country_code": "CH",
                                                   "sender_bc_no": "9000",
                                                   "sender_bank_no": "9000",
                                                   "sender_bank_name": "PostFinance",
                                                   "receiver_account_no": "12-345678-9",
                                                   "receiver_iban": "CH4431999123000889012",
                                                   "receiver_name": "Bexio Receiver",
                                                   "receiver_street": "Alpenstrasse",
                                                   "receiver_house_no": "5",
                                                   "receiver_city": "Bern",
                                                   "receiver_postcode": "3001",
                                                   "receiver_country_code": "CH",
                                                   "receiver_bc_no": "9000",
                                                   "receiver_bank_no": "9001",
                                                   "receiver_bank_name": "Berner Kantonalbank",
                                                   "fee_type": "BY_SENDER",
                                                   "is_salary_payment": false,
                                                   "reference_no": "210000000003139471430009017",
                                                   "message": "Invoice 2026-001",
                                                   "booking_text": "Office supplies",
                                                   "banking_payment_id": "1b583a8d-01d2-4f04-8f11-dcf657a2b6f9",
                                                   "banking_payment_entry_id": "2c694b9e-12e3-4a15-8a22-edd768b3c7e0",
                                                   "transaction_id": "a33c2049-6ccd-4748-89d7-93cf4f5ea36a"
                                               }
                                               """;

    /// <summary>
    /// Schema-accurate JSON envelope for <c>GET /4.0/purchase/outgoing-payments</c>, including
    /// a non-empty <c>data</c> array of <see cref="OutgoingPaymentListItem"/>s and the
    /// <c>paging</c> metadata object.
    /// </summary>
    private const string OutgoingPaymentListResponseJson = """
                                                           {
                                                               "data": [
                                                                   {
                                                                       "id": "b2c3d4e5-f6a7-8901-bcde-f12345678901",
                                                                       "bill_id": "c3d4e5f6-a7b8-9012-cdef-123456789012",
                                                                       "payment_type": "MANUAL",
                                                                       "status": "TRANSFERRED",
                                                                       "execution_date": "2026-06-01",
                                                                       "amount": 250.00,
                                                                       "sender_bank_account_id": 8,
                                                                       "receiver_account_no": "12-345678-9",
                                                                       "receiver_iban": "CH4431999123000889012",
                                                                       "banking_payment_id": "1b583a8d-01d2-4f04-8f11-dcf657a2b6f9",
                                                                       "transaction_id": "a33c2049-6ccd-4748-89d7-93cf4f5ea36a"
                                                                   }
                                                               ],
                                                               "paging": {
                                                                   "page": 1,
                                                                   "page_size": 100,
                                                                   "page_count": 1,
                                                                   "item_count": 1
                                                               }
                                                           }
                                                           """;

    private static readonly Guid TestPaymentId = Guid.Parse("b2c3d4e5-f6a7-8901-bcde-f12345678901");
    private static readonly Guid TestBillId = Guid.Parse("c3d4e5f6-a7b8-9012-cdef-123456789012");
    private static readonly Guid TestBankingPaymentId = Guid.Parse("1b583a8d-01d2-4f04-8f11-dcf657a2b6f9");
    private static readonly Guid TestBankingPaymentEntryId = Guid.Parse("2c694b9e-12e3-4a15-8a22-edd768b3c7e0");
    private static readonly Guid TestTransactionId = Guid.Parse("a33c2049-6ccd-4748-89d7-93cf4f5ea36a");

    /// <summary>
    /// <c>OutgoingPaymentService.Get</c> issues a <c>GET</c> against
    /// <c>/4.0/purchase/outgoing-payments</c> with the required <c>bill_id</c> on the URL,
    /// then deserialises the <c>{ data, paging }</c> envelope. Every field of the list-item
    /// schema and the paging schema is asserted.
    /// </summary>
    [Test]
    public async Task OutgoingPaymentService_Get_DeserialisesEnvelopeAndListItems()
    {
        Server
            .Given(Request.Create().WithPath(OutgoingPaymentsPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(OutgoingPaymentListResponseJson));

        var service = new OutgoingPaymentService(ConnectionHandler);

        var result = await service.Get(
            new QueryParameterOutgoingPayment(TestBillId),
            TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data!.Data, Has.Count.EqualTo(1));
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(OutgoingPaymentsPath));
            Assert.That(request.Url, Does.Contain($"bill_id={TestBillId}"));
        });

        var item = result.Data!.Data[0];
        Assert.Multiple(() =>
        {
            Assert.That(item.Id, Is.EqualTo(TestPaymentId));
            Assert.That(item.BillId, Is.EqualTo(TestBillId));
            Assert.That(item.PaymentType, Is.EqualTo(OutgoingPaymentType.MANUAL));
            Assert.That(item.Status, Is.EqualTo(OutgoingPaymentStatus.TRANSFERRED));
            Assert.That(item.ExecutionDate, Is.EqualTo(new DateOnly(2026, 6, 1)));
            Assert.That(item.Amount, Is.EqualTo(250.00m));
            Assert.That(item.SenderBankAccountId, Is.EqualTo(8));
            Assert.That(item.ReceiverAccountNo, Is.EqualTo("12-345678-9"));
            Assert.That(item.ReceiverIban, Is.EqualTo("CH4431999123000889012"));
            Assert.That(item.BankingPaymentId, Is.EqualTo(TestBankingPaymentId));
            Assert.That(item.TransactionId, Is.EqualTo(TestTransactionId));
        });

        var paging = result.Data.Paging;
        Assert.Multiple(() =>
        {
            Assert.That(paging.Page, Is.EqualTo(1));
            Assert.That(paging.PageSize, Is.EqualTo(100));
            Assert.That(paging.PageCount, Is.EqualTo(1));
            Assert.That(paging.ItemCount, Is.EqualTo(1));
        });
    }

    /// <summary>
    /// <c>OutgoingPaymentService.Get</c> with all four optional query parameters supplies
    /// each one to WireMock as a URL query parameter under the Bexio-defined keys
    /// (<c>bill_id</c>, <c>limit</c>, <c>page</c>, <c>order</c>, <c>sort</c>).
    /// </summary>
    [Test]
    public async Task OutgoingPaymentService_Get_WithAllQueryParameters_PassesAllOnUrl()
    {
        Server
            .Given(Request.Create().WithPath(OutgoingPaymentsPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(OutgoingPaymentListResponseJson));

        var service = new OutgoingPaymentService(ConnectionHandler);

        await service.Get(
            new QueryParameterOutgoingPayment(
                TestBillId,
                limit: 25,
                page: 3,
                order: "desc",
                sort: "execution_date"),
            TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(request.Url, Does.Contain($"bill_id={TestBillId}"));
            Assert.That(request.Url, Does.Contain("limit=25"));
            Assert.That(request.Url, Does.Contain("page=3"));
            Assert.That(request.Url, Does.Contain("order=desc"));
            Assert.That(request.Url, Does.Contain("sort=execution_date"));
        });
    }

    /// <summary>
    /// <c>OutgoingPaymentService.GetById</c> issues a <c>GET</c> request that includes
    /// the target <see cref="Guid"/> in the URL path and deserialises every field of
    /// the fully-populated single-object response.
    /// </summary>
    [Test]
    public async Task OutgoingPaymentService_GetById_DeserialisesAllSchemaFields()
    {
        var expectedPath = $"{OutgoingPaymentsPath}/{TestPaymentId}";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(OutgoingPaymentJson));

        var service = new OutgoingPaymentService(ConnectionHandler);

        var result = await service.GetById(TestPaymentId, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
        });

        AssertFullyPopulated(result.Data!);
    }

    /// <summary>
    /// <c>OutgoingPaymentService.Create</c> sends a <c>POST</c> request whose body is the
    /// serialised <see cref="OutgoingPaymentCreate"/> payload and surfaces the returned
    /// outgoing payment on success. Asserts both the wire payload and the deserialised
    /// response.
    /// </summary>
    [Test]
    public async Task OutgoingPaymentService_Create_SerialisesPayloadAndDeserialisesResponse()
    {
        Server
            .Given(Request.Create().WithPath(OutgoingPaymentsPath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(201).WithBody(OutgoingPaymentJson));

        var service = new OutgoingPaymentService(ConnectionHandler);

        var payload = new OutgoingPaymentCreate(
            BillId: TestBillId,
            PaymentType: OutgoingPaymentType.IBAN,
            ExecutionDate: new DateOnly(2026, 6, 1),
            Amount: 250.00m,
            CurrencyCode: "CHF",
            ExchangeRate: 1.0m,
            SenderBankAccountId: 1,
            IsSalaryPayment: false,
            SenderIban: "CH9300762011623852957",
            SenderName: "Acme AG",
            SenderStreet: "Bahnhofstrasse",
            SenderHouseNo: "1",
            SenderCity: "Zurich",
            SenderPostcode: "8001",
            SenderCountryCode: "CH",
            ReceiverIban: "CH4431999123000889012",
            ReceiverName: "Bexio Receiver",
            ReceiverStreet: "Alpenstrasse",
            ReceiverHouseNo: "5",
            ReceiverCity: "Bern",
            ReceiverPostcode: "3001",
            ReceiverCountryCode: "CH",
            FeeType: OutgoingPaymentFeeType.BY_SENDER);

        var result = await service.Create(payload, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(request.Method, Is.EqualTo("POST"));
            Assert.That(request.AbsolutePath, Is.EqualTo(OutgoingPaymentsPath));
            Assert.That(request.Body, Does.Contain($"\"bill_id\":\"{TestBillId}\""));
            Assert.That(request.Body, Does.Contain("\"payment_type\":\"IBAN\""));
            Assert.That(request.Body, Does.Contain("\"currency_code\":\"CHF\""));
            Assert.That(request.Body, Does.Contain("\"fee_type\":\"BY_SENDER\""));
            Assert.That(request.Body, Does.Contain("\"is_salary_payment\":false"));
            Assert.That(request.Body, Does.Contain("\"execution_date\":\"2026-06-01\""));
        });

        AssertFullyPopulated(result.Data!);
    }

    /// <summary>
    /// <c>OutgoingPaymentService.Update</c> sends a <c>PUT</c> request against
    /// <c>/4.0/purchase/outgoing-payments</c> (the collection path, no <c>/{id}</c>) whose
    /// body contains the <c>payment_id</c> and updated fields per the Bexio API spec, then
    /// deserialises the updated outgoing payment from the response.
    /// </summary>
    [Test]
    public async Task OutgoingPaymentService_Update_PutsToCollectionPathWithPaymentIdInBody()
    {
        Server
            .Given(Request.Create().WithPath(OutgoingPaymentsPath).UsingPut())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(OutgoingPaymentJson));

        var service = new OutgoingPaymentService(ConnectionHandler);

        var payload = new OutgoingPaymentUpdate(
            PaymentId: TestPaymentId,
            ExecutionDate: new DateOnly(2026, 6, 15),
            Amount: 300.00m,
            IsSalaryPayment: false,
            FeeType: OutgoingPaymentFeeType.BY_SENDER);

        var result = await service.Update(payload, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(request.Method, Is.EqualTo("PUT"));
            Assert.That(request.AbsolutePath, Is.EqualTo(OutgoingPaymentsPath));
            Assert.That(request.Body, Does.Contain($"\"payment_id\":\"{TestPaymentId}\""));
            Assert.That(request.Body, Does.Contain("\"execution_date\":\"2026-06-15\""));
            Assert.That(request.Body, Does.Contain("\"amount\":300"));
            Assert.That(request.Body, Does.Contain("\"is_salary_payment\":false"));
            Assert.That(request.Body, Does.Contain("\"fee_type\":\"BY_SENDER\""));
        });

        AssertFullyPopulated(result.Data!);
    }

    /// <summary>
    /// <c>OutgoingPaymentService.Delete</c> issues a <c>DELETE</c> request that includes the
    /// target <see cref="Guid"/> in the URL path.
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

    /// <summary>
    /// Asserts that every property of the <see cref="OutgoingPayment"/> model has the expected
    /// value from the canonical <see cref="OutgoingPaymentJson"/> sample.
    /// </summary>
    private static void AssertFullyPopulated(OutgoingPayment payment)
    {
        Assert.Multiple(() =>
        {
            Assert.That(payment.Id, Is.EqualTo(TestPaymentId));
            Assert.That(payment.Status, Is.EqualTo(OutgoingPaymentStatus.PENDING));
            Assert.That(payment.CreatedAt, Is.EqualTo(DateTimeOffset.Parse("2026-04-15T10:00:00+02:00")));
            Assert.That(payment.BillId, Is.EqualTo(TestBillId));
            Assert.That(payment.PaymentType, Is.EqualTo(OutgoingPaymentType.IBAN));
            Assert.That(payment.ExecutionDate, Is.EqualTo(new DateOnly(2026, 6, 1)));
            Assert.That(payment.Amount, Is.EqualTo(250.00m));
            Assert.That(payment.CurrencyCode, Is.EqualTo("CHF"));
            Assert.That(payment.ExchangeRate, Is.EqualTo(1.0m));
            Assert.That(payment.Note, Is.EqualTo("Test payment"));
            Assert.That(payment.SenderBankAccountId, Is.EqualTo(1));
            Assert.That(payment.SenderIban, Is.EqualTo("CH9300762011623852957"));
            Assert.That(payment.SenderName, Is.EqualTo("Acme AG"));
            Assert.That(payment.SenderStreet, Is.EqualTo("Bahnhofstrasse"));
            Assert.That(payment.SenderHouseNo, Is.EqualTo("1"));
            Assert.That(payment.SenderCity, Is.EqualTo("Zurich"));
            Assert.That(payment.SenderPostcode, Is.EqualTo("8001"));
            Assert.That(payment.SenderCountryCode, Is.EqualTo("CH"));
            Assert.That(payment.SenderBcNo, Is.EqualTo("9000"));
            Assert.That(payment.SenderBankNo, Is.EqualTo("9000"));
            Assert.That(payment.SenderBankName, Is.EqualTo("PostFinance"));
            Assert.That(payment.ReceiverAccountNo, Is.EqualTo("12-345678-9"));
            Assert.That(payment.ReceiverIban, Is.EqualTo("CH4431999123000889012"));
            Assert.That(payment.ReceiverName, Is.EqualTo("Bexio Receiver"));
            Assert.That(payment.ReceiverStreet, Is.EqualTo("Alpenstrasse"));
            Assert.That(payment.ReceiverHouseNo, Is.EqualTo("5"));
            Assert.That(payment.ReceiverCity, Is.EqualTo("Bern"));
            Assert.That(payment.ReceiverPostcode, Is.EqualTo("3001"));
            Assert.That(payment.ReceiverCountryCode, Is.EqualTo("CH"));
            Assert.That(payment.ReceiverBcNo, Is.EqualTo("9000"));
            Assert.That(payment.ReceiverBankNo, Is.EqualTo("9001"));
            Assert.That(payment.ReceiverBankName, Is.EqualTo("Berner Kantonalbank"));
            Assert.That(payment.FeeType, Is.EqualTo(OutgoingPaymentFeeType.BY_SENDER));
            Assert.That(payment.IsSalaryPayment, Is.False);
            Assert.That(payment.ReferenceNo, Is.EqualTo("210000000003139471430009017"));
            Assert.That(payment.Message, Is.EqualTo("Invoice 2026-001"));
            Assert.That(payment.BookingText, Is.EqualTo("Office supplies"));
            Assert.That(payment.BankingPaymentId, Is.EqualTo(TestBankingPaymentId));
            Assert.That(payment.BankingPaymentEntryId, Is.EqualTo(TestBankingPaymentEntryId));
            Assert.That(payment.TransactionId, Is.EqualTo(TestTransactionId));
        });
    }
}
