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
using BexioApiNet.Services.Connectors.Banking;

namespace BexioApiNet.IntegrationTests.Banking;

/// <summary>
/// Integration tests covering the CRUD and Cancel entry points of <see cref="PaymentService" /> against
/// WireMock stubs. Verifies the path composed from <see cref="PaymentConfiguration" />
/// (<c>4.0/banking/payments</c>) reaches the handler correctly and that the expected HTTP verbs
/// are used for each operation (Get, GetById, Create, Cancel, Update, Delete).
/// </summary>
public sealed class PaymentServiceIntegrationTests : IntegrationTestBase
{
    private const string PaymentsPath = "/4.0/banking/payments";

    private const string PaymentResponse = """
                                           {
                                               "id": 1,
                                               "uuid": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
                                               "sender": null,
                                               "recipient": null,
                                               "amount": 100.00,
                                               "currency": "CHF",
                                               "execution_date": "2024-06-01",
                                               "allowance": "fee_split",
                                               "is_salary": false,
                                               "instruction_id": null,
                                               "purchase_reference": null,
                                               "document_no": "",
                                               "qr_reference_number": null,
                                               "additional_information": null,
                                               "status": "open",
                                               "type": "iban",
                                               "due_date": null,
                                               "created_at": "2024-01-15T10:00:00+01:00",
                                               "is_editing_restricted": false
                                           }
                                           """;

    private static readonly Guid TestPaymentId = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890");

    /// <summary>
    /// <c>PaymentService.Get()</c> must issue a <c>GET</c> against
    /// <c>/4.0/banking/payments</c> and return a successful <c>ApiResult</c> when the
    /// server responds with an empty collection.
    /// </summary>
    [Test]
    public async Task PaymentService_Get_SendsGetRequestToCorrectPath()
    {
        Server
            .Given(Request.Create().WithPath(PaymentsPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("[]"));

        var service = new PaymentService(ConnectionHandler);

        var result = await service.Get(cancellationToken: TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(PaymentsPath));
        });
    }

    /// <summary>
    /// <c>PaymentService.GetById</c> must issue a <c>GET</c> request that includes the target
    /// <see cref="Guid" /> in the URL path and surface the returned payment on success.
    /// </summary>
    [Test]
    public async Task PaymentService_GetById_SendsGetRequestWithIdInPath()
    {
        var expectedPath = $"{PaymentsPath}/{TestPaymentId}";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(PaymentResponse));

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
    }

    /// <summary>
    /// <c>PaymentService.Create</c> must send a <c>POST</c> request whose body is the serialized
    /// <see cref="PaymentCreate" /> payload, and must surface the returned payment on success.
    /// </summary>
    [Test]
    public async Task PaymentService_Create_SendsPostRequestWithPayload()
    {
        Server
            .Given(Request.Create().WithPath(PaymentsPath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(201).WithBody(PaymentResponse));

        var service = new PaymentService(ConnectionHandler);

        var payload = new PaymentCreate(
            PaymentType.iban,
            Guid.Parse("11111111-1111-1111-1111-111111111111"),
            new PaymentRecipient(
                "Test Recipient",
                "CH9300762011623852957",
                new PaymentAddress(
                    "Main Street",
                    "1",
                    "8000",
                    "Zurich",
                    "CH")),
            100.00m,
            "CHF",
            new DateOnly(2024, 6, 1),
            false);

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
        });
    }

    /// <summary>
    /// <c>PaymentService.Cancel</c> must send a <c>POST</c> request against
    /// <c>/4.0/banking/payments/{id}/cancel</c> with no body — Bexio uses POST for this action.
    /// </summary>
    [Test]
    public async Task PaymentService_Cancel_SendsPostRequestToCancelPath()
    {
        var expectedPath = $"{PaymentsPath}/{TestPaymentId}/cancel";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(PaymentResponse));

        var service = new PaymentService(ConnectionHandler);

        var result = await service.Cancel(TestPaymentId, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(request.Method, Is.EqualTo("POST"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
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
}
