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

using BexioApiNet.Abstractions.Models.Banking.Payments;
using BexioApiNet.Abstractions.Models.Banking.Payments.Enums;
using BexioApiNet.Abstractions.Models.Banking.Payments.Views;
using BexioApiNet.Models;
using BexioApiNet.Services.Connectors.Banking;

namespace BexioApiNet.IntegrationTests.Banking;

/// <summary>
/// Integration tests covering the CRUD and Cancel entry points of <see cref="PaymentService" /> against
/// WireMock stubs. Verifies the path composed from <see cref="PaymentConfiguration" />
/// (<c>4.0/banking/payments</c>) reaches the handler correctly, the expected HTTP verbs
/// are used for each operation, and the C# <see cref="Payment"/> model deserialises every
/// field of the OpenAPI v4.0 schema correctly.
/// </summary>
public sealed class PaymentServiceIntegrationTests : IntegrationTestBase
{
    private const string PaymentsPath = "/4.0/banking/payments";

    /// <summary>
    /// Schema-accurate JSON payload populated with values for every property defined by the
    /// <c>PaymentView</c> schema in <c>bexio-v3.json</c>, including the nested
    /// <c>sender</c>, <c>recipient</c>, <c>recipient.address</c>, and
    /// <c>purchase_reference</c> objects.
    /// </summary>
    private const string PaymentJson = """
                                       {
                                           "id": 1,
                                           "uuid": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
                                           "sender": {
                                               "id": 5,
                                               "uuid": "55555555-5555-5555-5555-555555555555",
                                               "iban": "CH3000784295116252003"
                                           },
                                           "recipient": {
                                               "name": "John Doe",
                                               "iban": "CH9300762011623852957",
                                               "address": {
                                                   "street_name": "Bahnhofstrasse",
                                                   "house_number": "1",
                                                   "zip": "8001",
                                                   "city": "Zurich",
                                                   "country_code": "CH"
                                               }
                                           },
                                           "amount": 100.50,
                                           "currency": "CHF",
                                           "execution_date": "2026-06-01",
                                           "allowance": "fee_split",
                                           "is_salary": false,
                                           "instruction_id": "INS-12345",
                                           "purchase_reference": {
                                               "bill_id": "11111111-2222-3333-4444-555555555555",
                                               "bill_payment_id": "66666666-7777-8888-9999-aaaaaaaaaaaa"
                                           },
                                           "document_no": "B-2026-001",
                                           "qr_reference_number": "210000000003139471430009017",
                                           "additional_information": "//S1/10/5541/11/191210/20/1235",
                                           "status": "open",
                                           "type": "iban",
                                           "due_date": "2026-06-15",
                                           "created_at": "2026-04-15T10:00:00+02:00",
                                           "is_editing_restricted": false
                                       }
                                       """;

    private static readonly Guid TestPaymentId = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890");

    /// <summary>
    /// <c>PaymentService.Get()</c> issues a <c>GET</c> against
    /// <c>/4.0/banking/payments</c> and deserialises the list response.
    /// </summary>
    [Test]
    public async Task PaymentService_Get_DeserialisesPaymentList()
    {
        Server
            .Given(Request.Create().WithPath(PaymentsPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody($"[{PaymentJson}]"));

        var service = new PaymentService(ConnectionHandler);

        var result = await service.Get(cancellationToken: TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data, Has.Count.EqualTo(1));
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(PaymentsPath));
        });

        AssertFullyPopulated(result.Data![0]);
    }

    /// <summary>
    /// <c>PaymentService.Get</c> with a <see cref="QueryParameterPayment" /> must forward
    /// <c>page</c>, <c>per-page</c>, and <c>filter-by</c> as URL query parameters to WireMock.
    /// </summary>
    [Test]
    public async Task PaymentService_Get_WithQueryParameter_PassesAllParametersOnUrl()
    {
        Server
            .Given(Request.Create().WithPath(PaymentsPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("[]"));

        var service = new PaymentService(ConnectionHandler);

        await service.Get(
            new QueryParameterPayment(Page: 2, PerPage: 25, FilterBy: "status:open"),
            cancellationToken: TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(PaymentsPath));
            Assert.That(request.Url, Does.Contain("page=2"));
            Assert.That(request.Url, Does.Contain("per-page=25"));
            Assert.That(request.Url, Does.Contain("filter-by=status%3aopen").Or.Contain("filter-by=status:open"));
        });
    }

    /// <summary>
    /// <c>PaymentService.GetById</c> must issue a <c>GET</c> request that includes the target
    /// <see cref="Guid" /> in the URL path, then deserialise every field of the fully-populated
    /// <c>PaymentView</c> response into the <see cref="Payment"/> model.
    /// </summary>
    [Test]
    public async Task PaymentService_GetById_DeserialisesAllSchemaFields()
    {
        var expectedPath = $"{PaymentsPath}/{TestPaymentId}";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(PaymentJson));

        var service = new PaymentService(ConnectionHandler);

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
    /// <c>PaymentService.Create</c> must send a <c>POST</c> request whose body is the serialised
    /// <see cref="PaymentCreate" /> payload (including the nested recipient address) and surface
    /// the returned payment on success.
    /// </summary>
    [Test]
    public async Task PaymentService_Create_SerialisesPayloadAndDeserialisesResponse()
    {
        Server
            .Given(Request.Create().WithPath(PaymentsPath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(201).WithBody(PaymentJson));

        var service = new PaymentService(ConnectionHandler);

        var payload = new PaymentCreate(
            Type: PaymentType.iban,
            AccountId: Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Recipient: new PaymentRecipient(
                "John Doe",
                "CH9300762011623852957",
                new PaymentAddress(
                    "Bahnhofstrasse",
                    "1",
                    "8001",
                    "Zurich",
                    "CH")),
            Amount: 100.50m,
            Currency: "CHF",
            ExecutionDate: new DateOnly(2026, 6, 1),
            IsSalary: false,
            Allowance: PaymentAllowance.fee_split);

        var result = await service.Create(payload, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(request.Method, Is.EqualTo("POST"));
            Assert.That(request.AbsolutePath, Is.EqualTo(PaymentsPath));
            Assert.That(request.Body, Does.Contain("\"type\":\"iban\""));
            Assert.That(request.Body, Does.Contain("\"currency\":\"CHF\""));
            Assert.That(request.Body, Does.Contain("\"allowance\":\"fee_split\""));
            Assert.That(request.Body, Does.Contain("\"execution_date\":\"2026-06-01\""));
            Assert.That(request.Body, Does.Contain("\"street_name\":\"Bahnhofstrasse\""));
            Assert.That(request.Body, Does.Contain("\"country_code\":\"CH\""));
        });

        AssertFullyPopulated(result.Data!);
    }

    /// <summary>
    /// <c>PaymentService.Cancel</c> must send a <c>POST</c> request against
    /// <c>/4.0/banking/payments/{id}/cancel</c> with no request body and deserialise the
    /// updated payment from the response.
    /// </summary>
    [Test]
    public async Task PaymentService_Cancel_PostsToCancelPathAndDeserialisesResponse()
    {
        var expectedPath = $"{PaymentsPath}/{TestPaymentId}/cancel";
        const string cancelledJson = """
                                     {
                                         "id": 1,
                                         "uuid": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
                                         "sender": null,
                                         "recipient": null,
                                         "amount": 100.00,
                                         "currency": "CHF",
                                         "execution_date": "2026-06-01",
                                         "allowance": "fee_split",
                                         "is_salary": false,
                                         "instruction_id": null,
                                         "purchase_reference": null,
                                         "document_no": "",
                                         "qr_reference_number": null,
                                         "additional_information": null,
                                         "status": "cancelled",
                                         "type": "iban",
                                         "due_date": null,
                                         "created_at": "2026-04-15T10:00:00+02:00",
                                         "is_editing_restricted": false
                                     }
                                     """;

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(cancelledJson));

        var service = new PaymentService(ConnectionHandler);

        var result = await service.Cancel(TestPaymentId, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data!.Status, Is.EqualTo(PaymentStatus.cancelled));
            Assert.That(request.Method, Is.EqualTo("POST"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
        });
    }

    /// <summary>
    /// <c>PaymentService.Update</c> must issue a <c>PUT</c> request against
    /// <c>/4.0/banking/payments/{id}</c> whose body is the serialised
    /// <see cref="PaymentUpdate" /> payload (only the fields that were set), and surface
    /// the updated payment on success.
    /// </summary>
    [Test]
    public async Task PaymentService_Update_SerialisesPayloadAndDeserialisesResponse()
    {
        var expectedPath = $"{PaymentsPath}/{TestPaymentId}";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingPut())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(PaymentJson));

        var service = new PaymentService(ConnectionHandler);

        var payload = new PaymentUpdate(Amount: 200.0m, AdditionalInformation: "Updated by test");

        var result = await service.Update(TestPaymentId, payload, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(request.Method, Is.EqualTo("PUT"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
            Assert.That(request.Body, Does.Contain("\"amount\":200"));
            Assert.That(request.Body, Does.Contain("\"additional_information\":\"Updated by test\""));
        });
    }

    /// <summary>
    /// <c>PaymentService.Delete</c> must issue a <c>DELETE</c> request that includes the target
    /// <see cref="Guid" /> in the URL path.
    /// </summary>
    [Test]
    public async Task PaymentService_Delete_SendsDeleteRequestWithIdInPath()
    {
        var expectedPath = $"{PaymentsPath}/{TestPaymentId}";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingDelete())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("{\"success\":true}"));

        var service = new PaymentService(ConnectionHandler);

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
    /// Asserts that every property of the <see cref="Payment"/> model has the expected value
    /// from the canonical <see cref="PaymentJson"/> sample, including nested objects.
    /// </summary>
    private static void AssertFullyPopulated(Payment payment)
    {
        Assert.Multiple(() =>
        {
            Assert.That(payment.Id, Is.EqualTo(1));
            Assert.That(payment.Uuid, Is.EqualTo("a1b2c3d4-e5f6-7890-abcd-ef1234567890"));
            Assert.That(payment.Sender, Is.Not.Null);
            Assert.That(payment.Sender!.Id, Is.EqualTo(5));
            Assert.That(payment.Sender.Uuid, Is.EqualTo("55555555-5555-5555-5555-555555555555"));
            Assert.That(payment.Sender.Iban, Is.EqualTo("CH3000784295116252003"));
            Assert.That(payment.Recipient, Is.Not.Null);
            Assert.That(payment.Recipient!.Name, Is.EqualTo("John Doe"));
            Assert.That(payment.Recipient.Iban, Is.EqualTo("CH9300762011623852957"));
            Assert.That(payment.Recipient.Address.StreetName, Is.EqualTo("Bahnhofstrasse"));
            Assert.That(payment.Recipient.Address.HouseNumber, Is.EqualTo("1"));
            Assert.That(payment.Recipient.Address.Zip, Is.EqualTo("8001"));
            Assert.That(payment.Recipient.Address.City, Is.EqualTo("Zurich"));
            Assert.That(payment.Recipient.Address.CountryCode, Is.EqualTo("CH"));
            Assert.That(payment.Amount, Is.EqualTo(100.50m));
            Assert.That(payment.Currency, Is.EqualTo("CHF"));
            Assert.That(payment.ExecutionDate, Is.EqualTo(new DateOnly(2026, 6, 1)));
            Assert.That(payment.Allowance, Is.EqualTo(PaymentAllowance.fee_split));
            Assert.That(payment.IsSalary, Is.False);
            Assert.That(payment.InstructionId, Is.EqualTo("INS-12345"));
            Assert.That(payment.PurchaseReference, Is.Not.Null);
            Assert.That(payment.PurchaseReference!.BillId, Is.EqualTo(Guid.Parse("11111111-2222-3333-4444-555555555555")));
            Assert.That(payment.PurchaseReference.BillPaymentId, Is.EqualTo(Guid.Parse("66666666-7777-8888-9999-aaaaaaaaaaaa")));
            Assert.That(payment.DocumentNo, Is.EqualTo("B-2026-001"));
            Assert.That(payment.QrReferenceNumber, Is.EqualTo("210000000003139471430009017"));
            Assert.That(payment.AdditionalInformation, Is.EqualTo("//S1/10/5541/11/191210/20/1235"));
            Assert.That(payment.Status, Is.EqualTo(PaymentStatus.open));
            Assert.That(payment.Type, Is.EqualTo(PaymentType.iban));
            Assert.That(payment.DueDate, Is.EqualTo(new DateOnly(2026, 6, 15)));
            Assert.That(payment.CreatedAt, Is.EqualTo(DateTimeOffset.Parse("2026-04-15T10:00:00+02:00")));
            Assert.That(payment.IsEditingRestricted, Is.False);
        });
    }
}
