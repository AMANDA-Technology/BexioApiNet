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

using BexioApiNet.Abstractions.Models.Api;
using BexioApiNet.Abstractions.Models.Sales.Invoices.Views;
using BexioApiNet.Services.Connectors.Sales;

namespace BexioApiNet.IntegrationTests.Sales;

/// <summary>
/// Integration tests covering the full surface of <see cref="InvoiceService"/> against WireMock
/// stubs. Verifies the path composed from <see cref="InvoiceConfiguration"/>
/// (<c>2.0/kb_invoice</c>) reaches the handler correctly, that the expected HTTP verbs are used
/// (including the Bexio-specific <c>POST</c> for edits and <c>POST</c>-without-body for action
/// endpoints such as <c>/issue</c>), that payment sub-resource routes are composed correctly,
/// and that payloads are serialized with the expected snake_case field names.
/// </summary>
public sealed class InvoiceServiceIntegrationTests : IntegrationTestBase
{
    private const string InvoicesPath = "/2.0/kb_invoice";

    private const string InvoiceResponse = """
        {
            "id": 1,
            "document_nr": "RE-1000",
            "title": "Test invoice",
            "contact_id": 42,
            "contact_sub_id": null,
            "user_id": 1,
            "project_id": null,
            "pr_project_id": null,
            "logopaper_id": null,
            "language_id": null,
            "bank_account_id": null,
            "currency_id": 1,
            "payment_type_id": null,
            "header": null,
            "footer": null,
            "total_gross": "100.00",
            "total_net": "92.00",
            "total_taxes": "8.00",
            "total_received_payments": "0.00",
            "total_credit_vouchers": "0.00",
            "total_remaining_payments": "100.00",
            "total": "100.00",
            "total_rounding_difference": 0.0,
            "mwst_type": 0,
            "mwst_is_net": true,
            "show_position_taxes": false,
            "is_valid_from": "2026-04-01",
            "is_valid_to": "2026-05-01",
            "contact_address": null,
            "contact_address_manual": null,
            "kb_item_status_id": 7,
            "reference": null,
            "api_reference": null,
            "viewed_by_client_at": null,
            "updated_at": "2026-04-01 12:00:00",
            "esr_id": null,
            "qr_invoice_id": null,
            "template_slug": null,
            "taxs": [],
            "network_link": null,
            "positions": []
        }
        """;

    private const string PaymentResponse = """
        {
            "id": 10,
            "date": "2026-04-10",
            "value": "100.00",
            "bank_account_id": 1,
            "title": "Received Payment",
            "payment_service_id": null,
            "is_client_account_redemption": false,
            "is_cash_discount": false,
            "kb_invoice_id": 1,
            "kb_credit_voucher_id": null,
            "kb_bill_id": null,
            "kb_credit_voucher_text": null
        }
        """;

    /// <summary>
    /// <c>InvoiceService.Get()</c> must issue a <c>GET</c> request against <c>/2.0/kb_invoice</c>
    /// and return a successful <c>ApiResult</c> when the server returns an empty array.
    /// </summary>
    [Test]
    public async Task InvoiceService_Get_SendsGetRequest()
    {
        Server
            .Given(Request.Create().WithPath(InvoicesPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("[]"));

        var service = new InvoiceService(ConnectionHandler);

        var result = await service.Get(cancellationToken: TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(InvoicesPath));
        });
    }

    /// <summary>
    /// <c>InvoiceService.GetById</c> must issue a <c>GET</c> request that includes the target id
    /// in the URL path and surface the returned invoice on success.
    /// </summary>
    [Test]
    public async Task InvoiceService_GetById_SendsGetRequest()
    {
        const int id = 1;
        var expectedPath = $"{InvoicesPath}/{id}";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(InvoiceResponse));

        var service = new InvoiceService(ConnectionHandler);

        var result = await service.GetById(id, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data!.Id, Is.EqualTo(id));
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
        });
    }

    /// <summary>
    /// <c>InvoiceService.GetPdf</c> must issue a <c>GET</c> request against
    /// <c>/2.0/kb_invoice/{id}/pdf</c> and return the raw PDF bytes.
    /// </summary>
    [Test]
    public async Task InvoiceService_GetPdf_SendsGetRequest_AndReturnsBytes()
    {
        const int id = 1;
        var expectedPath = $"{InvoicesPath}/{id}/pdf";
        var pdfBytes = new byte[] { 0x25, 0x50, 0x44, 0x46 }; // "%PDF"

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/pdf")
                .WithBody(pdfBytes));

        var service = new InvoiceService(ConnectionHandler);

        var result = await service.GetPdf(id, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.EqualTo(pdfBytes));
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
        });
    }

    /// <summary>
    /// <c>InvoiceService.Create</c> must send a <c>POST</c> request whose body is the serialized
    /// <see cref="InvoiceCreate"/> payload, and must surface the returned invoice on success.
    /// </summary>
    [Test]
    public async Task InvoiceService_Create_SendsPostRequest()
    {
        Server
            .Given(Request.Create().WithPath(InvoicesPath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(201).WithBody(InvoiceResponse));

        var service = new InvoiceService(ConnectionHandler);

        var payload = new InvoiceCreate(
            UserId: 1,
            Title: "Test invoice",
            ContactId: 42);

        var result = await service.Create(payload, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data!.Id, Is.EqualTo(1));
            Assert.That(request.Method, Is.EqualTo("POST"));
            Assert.That(request.AbsolutePath, Is.EqualTo(InvoicesPath));
            Assert.That(request.Body, Does.Contain("\"user_id\":1"));
            Assert.That(request.Body, Does.Contain("\"title\":\"Test invoice\""));
            Assert.That(request.Body, Does.Contain("\"contact_id\":42"));
        });
    }

    /// <summary>
    /// <c>InvoiceService.Update</c> must send a <c>POST</c> (not <c>PUT</c>) request against
    /// <c>/2.0/kb_invoice/{id}</c> — Bexio uses POST for full-replacement edits on this resource.
    /// </summary>
    [Test]
    public async Task InvoiceService_Update_SendsPostRequest_WithIdInPath()
    {
        const int id = 1;
        var expectedPath = $"{InvoicesPath}/{id}";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(InvoiceResponse));

        var service = new InvoiceService(ConnectionHandler);

        var payload = new InvoiceUpdate(
            UserId: 1,
            Title: "Updated title",
            ContactId: 42);

        var result = await service.Update(id, payload, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data!.Id, Is.EqualTo(id));
            Assert.That(request.Method, Is.EqualTo("POST"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
            Assert.That(request.Body, Does.Contain("\"title\":\"Updated title\""));
        });
    }

    /// <summary>
    /// <c>InvoiceService.Delete</c> must issue a <c>DELETE</c> request that includes the target id
    /// in the URL path.
    /// </summary>
    [Test]
    public async Task InvoiceService_Delete_SendsDeleteRequest()
    {
        const int idToDelete = 1;
        var expectedPath = $"{InvoicesPath}/{idToDelete}";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingDelete())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("{\"success\":true}"));

        var service = new InvoiceService(ConnectionHandler);

        var result = await service.Delete(idToDelete, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("DELETE"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
        });
    }

    /// <summary>
    /// <c>InvoiceService.Search</c> must send a <c>POST</c> request against
    /// <c>/2.0/kb_invoice/search</c> with the <see cref="SearchCriteria"/> list as the JSON body.
    /// </summary>
    [Test]
    public async Task InvoiceService_Search_SendsPostRequest_ToSearchPath()
    {
        var expectedPath = $"{InvoicesPath}/search";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody($"[{InvoiceResponse}]"));

        var service = new InvoiceService(ConnectionHandler);

        var criteria = new List<SearchCriteria>
        {
            new() { Field = "title", Value = "Test", Criteria = "like" }
        };

        var result = await service.Search(criteria, cancellationToken: TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("POST"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
            Assert.That(request.Body, Does.Contain("\"field\":\"title\""));
            Assert.That(request.Body, Does.Contain("\"criteria\":\"like\""));
        });
    }

    /// <summary>
    /// <c>InvoiceService.Issue</c> must send a body-less <c>POST</c> against
    /// <c>/2.0/kb_invoice/{id}/issue</c>.
    /// </summary>
    [Test]
    public async Task InvoiceService_Issue_SendsPostRequest_ToIssuePath()
    {
        const int id = 1;
        var expectedPath = $"{InvoicesPath}/{id}/issue";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("{\"success\":true}"));

        var service = new InvoiceService(ConnectionHandler);

        var result = await service.Issue(id, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("POST"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
        });
    }

    /// <summary>
    /// <c>InvoiceService.RevertIssue</c> must send a body-less <c>POST</c> against
    /// <c>/2.0/kb_invoice/{id}/revert_issue</c>.
    /// </summary>
    [Test]
    public async Task InvoiceService_RevertIssue_SendsPostRequest_ToRevertIssuePath()
    {
        const int id = 1;
        var expectedPath = $"{InvoicesPath}/{id}/revert_issue";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("{\"success\":true}"));

        var service = new InvoiceService(ConnectionHandler);

        var result = await service.RevertIssue(id, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("POST"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
        });
    }

    /// <summary>
    /// <c>InvoiceService.Cancel</c> must send a body-less <c>POST</c> against
    /// <c>/2.0/kb_invoice/{id}/cancel</c>.
    /// </summary>
    [Test]
    public async Task InvoiceService_Cancel_SendsPostRequest_ToCancelPath()
    {
        const int id = 1;
        var expectedPath = $"{InvoicesPath}/{id}/cancel";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("{\"success\":true}"));

        var service = new InvoiceService(ConnectionHandler);

        var result = await service.Cancel(id, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("POST"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
        });
    }

    /// <summary>
    /// <c>InvoiceService.MarkAsSent</c> must send a body-less <c>POST</c> against
    /// <c>/2.0/kb_invoice/{id}/mark_as_sent</c>.
    /// </summary>
    [Test]
    public async Task InvoiceService_MarkAsSent_SendsPostRequest_ToMarkAsSentPath()
    {
        const int id = 1;
        var expectedPath = $"{InvoicesPath}/{id}/mark_as_sent";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("{\"success\":true}"));

        var service = new InvoiceService(ConnectionHandler);

        var result = await service.MarkAsSent(id, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("POST"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
        });
    }

    /// <summary>
    /// <c>InvoiceService.Copy</c> must send a <c>POST</c> against <c>/2.0/kb_invoice/{id}/copy</c>
    /// with the <see cref="InvoiceCopyRequest"/> payload, and return the new invoice.
    /// </summary>
    [Test]
    public async Task InvoiceService_Copy_SendsPostRequest_WithBody()
    {
        const int id = 1;
        var expectedPath = $"{InvoicesPath}/{id}/copy";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(201).WithBody(InvoiceResponse));

        var service = new InvoiceService(ConnectionHandler);

        var payload = new InvoiceCopyRequest(ContactId: 42, Title: "Copy of invoice");

        var result = await service.Copy(id, payload, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(request.Method, Is.EqualTo("POST"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
            Assert.That(request.Body, Does.Contain("\"contact_id\":42"));
            Assert.That(request.Body, Does.Contain("\"title\":\"Copy of invoice\""));
        });
    }

    /// <summary>
    /// <c>InvoiceService.Send</c> must send a <c>POST</c> against <c>/2.0/kb_invoice/{id}/send</c>
    /// with the <see cref="InvoiceSendRequest"/> payload.
    /// </summary>
    [Test]
    public async Task InvoiceService_Send_SendsPostRequest_WithBody()
    {
        const int id = 1;
        var expectedPath = $"{InvoicesPath}/{id}/send";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("{\"success\":true}"));

        var service = new InvoiceService(ConnectionHandler);

        var payload = new InvoiceSendRequest(
            RecipientEmail: "test@example.com",
            Subject: "Your invoice",
            Message: "Please find your invoice at [Network Link].");

        var result = await service.Send(id, payload, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("POST"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
            Assert.That(request.Body, Does.Contain("\"recipient_email\":\"test@example.com\""));
            Assert.That(request.Body, Does.Contain("\"subject\":\"Your invoice\""));
        });
    }

    /// <summary>
    /// <c>InvoiceService.GetPayments</c> must issue a <c>GET</c> against
    /// <c>/2.0/kb_invoice/{invoice_id}/payment</c>.
    /// </summary>
    [Test]
    public async Task InvoiceService_GetPayments_SendsGetRequest_ToPaymentPath()
    {
        const int invoiceId = 1;
        var expectedPath = $"{InvoicesPath}/{invoiceId}/payment";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("[]"));

        var service = new InvoiceService(ConnectionHandler);

        var result = await service.GetPayments(invoiceId, cancellationToken: TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
        });
    }

    /// <summary>
    /// <c>InvoiceService.GetPaymentById</c> must issue a <c>GET</c> against
    /// <c>/2.0/kb_invoice/{invoice_id}/payment/{payment_id}</c> and surface the payment.
    /// </summary>
    [Test]
    public async Task InvoiceService_GetPaymentById_SendsGetRequest()
    {
        const int invoiceId = 1;
        const int paymentId = 10;
        var expectedPath = $"{InvoicesPath}/{invoiceId}/payment/{paymentId}";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(PaymentResponse));

        var service = new InvoiceService(ConnectionHandler);

        var result = await service.GetPaymentById(invoiceId, paymentId, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data!.Id, Is.EqualTo(paymentId));
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
        });
    }

    /// <summary>
    /// <c>InvoiceService.CreatePayment</c> must send a <c>POST</c> against
    /// <c>/2.0/kb_invoice/{invoice_id}/payment</c> with the serialized
    /// <see cref="InvoicePaymentCreate"/> body.
    /// </summary>
    [Test]
    public async Task InvoiceService_CreatePayment_SendsPostRequest_WithBody()
    {
        const int invoiceId = 1;
        var expectedPath = $"{InvoicesPath}/{invoiceId}/payment";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(201).WithBody(PaymentResponse));

        var service = new InvoiceService(ConnectionHandler);

        var payload = new InvoicePaymentCreate(
            Value: "100.00",
            Date: new DateOnly(2026, 4, 10),
            BankAccountId: 1);

        var result = await service.CreatePayment(invoiceId, payload, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(request.Method, Is.EqualTo("POST"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
            Assert.That(request.Body, Does.Contain("\"value\":\"100.00\""));
            Assert.That(request.Body, Does.Contain("\"bank_account_id\":1"));
        });
    }

    /// <summary>
    /// <c>InvoiceService.DeletePayment</c> must issue a <c>DELETE</c> against
    /// <c>/2.0/kb_invoice/{invoice_id}/payment/{payment_id}</c>.
    /// </summary>
    [Test]
    public async Task InvoiceService_DeletePayment_SendsDeleteRequest()
    {
        const int invoiceId = 1;
        const int paymentId = 10;
        var expectedPath = $"{InvoicesPath}/{invoiceId}/payment/{paymentId}";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingDelete())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("{\"success\":true}"));

        var service = new InvoiceService(ConnectionHandler);

        var result = await service.DeletePayment(invoiceId, paymentId, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("DELETE"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
        });
    }
}
