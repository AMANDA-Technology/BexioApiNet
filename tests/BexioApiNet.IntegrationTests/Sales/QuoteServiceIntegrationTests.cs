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
using BexioApiNet.Abstractions.Models.Sales.Quotes.Views;
using BexioApiNet.Services.Connectors.Sales;

namespace BexioApiNet.IntegrationTests.Sales;

/// <summary>
/// Integration tests covering the full surface of <see cref="QuoteService"/> against WireMock
/// stubs. Verifies the path composed from <see cref="QuoteConfiguration"/>
/// (<c>2.0/kb_offer</c>) reaches the handler correctly, that the expected HTTP verbs are used
/// (including the Bexio-specific <c>POST</c> for edits and <c>POST</c>-without-body for action
/// endpoints such as <c>/issue</c>, <c>/accept</c>, <c>/reject</c>, <c>/reissue</c>,
/// <c>/mark_as_sent</c>, <c>/revertIssue</c>), and that payloads are serialized with the expected
/// snake_case field names.
/// </summary>
public sealed class QuoteServiceIntegrationTests : IntegrationTestBase
{
    private const string QuotesPath = "/2.0/kb_offer";

    private const string QuoteResponse = """
        {
            "id": 1,
            "document_nr": "AN-1000",
            "title": "Test quote",
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
            "total": "100.00",
            "total_rounding_difference": 0.0,
            "mwst_type": 0,
            "mwst_is_net": true,
            "show_position_taxes": false,
            "is_valid_from": "2026-04-01",
            "is_valid_until": "2026-05-01",
            "contact_address": null,
            "contact_address_manual": null,
            "delivery_address_type": 0,
            "delivery_address": null,
            "delivery_address_manual": null,
            "kb_item_status_id": 1,
            "api_reference": null,
            "viewed_by_client_at": null,
            "kb_terms_of_payment_template_id": null,
            "show_total": true,
            "updated_at": "2026-04-01 12:00:00",
            "template_slug": null,
            "taxs": [],
            "network_link": null,
            "positions": []
        }
        """;

    private const string InvoiceResponse = """
        {
            "id": 77,
            "document_nr": "RE-1000",
            "title": "Invoice from quote",
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

    /// <summary>
    /// <c>QuoteService.Get()</c> must issue a <c>GET</c> request against <c>/2.0/kb_offer</c>
    /// and return a successful <c>ApiResult</c> when the server returns an empty array.
    /// </summary>
    [Test]
    public async Task QuoteService_Get_SendsGetRequest()
    {
        Server
            .Given(Request.Create().WithPath(QuotesPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("[]"));

        var service = new QuoteService(ConnectionHandler);

        var result = await service.Get(cancellationToken: TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(QuotesPath));
        });
    }

    /// <summary>
    /// <c>QuoteService.GetById</c> must issue a <c>GET</c> request that includes the target id
    /// in the URL path and surface the returned quote on success.
    /// </summary>
    [Test]
    public async Task QuoteService_GetById_SendsGetRequest()
    {
        const int id = 1;
        var expectedPath = $"{QuotesPath}/{id}";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(QuoteResponse));

        var service = new QuoteService(ConnectionHandler);

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
    /// <c>QuoteService.GetPdf</c> must issue a <c>GET</c> request against
    /// <c>/2.0/kb_offer/{id}/pdf</c> and return the raw PDF bytes.
    /// </summary>
    [Test]
    public async Task QuoteService_GetPdf_SendsGetRequest_AndReturnsBytes()
    {
        const int id = 1;
        var expectedPath = $"{QuotesPath}/{id}/pdf";
        var pdfBytes = new byte[] { 0x25, 0x50, 0x44, 0x46 }; // "%PDF"

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/pdf")
                .WithBody(pdfBytes));

        var service = new QuoteService(ConnectionHandler);

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
    /// <c>QuoteService.Create</c> must send a <c>POST</c> request whose body is the serialized
    /// <see cref="QuoteCreate"/> payload, and must surface the returned quote on success.
    /// </summary>
    [Test]
    public async Task QuoteService_Create_SendsPostRequest()
    {
        Server
            .Given(Request.Create().WithPath(QuotesPath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(201).WithBody(QuoteResponse));

        var service = new QuoteService(ConnectionHandler);

        var payload = new QuoteCreate(
            UserId: 1,
            Title: "Test quote",
            ContactId: 42);

        var result = await service.Create(payload, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data!.Id, Is.EqualTo(1));
            Assert.That(request.Method, Is.EqualTo("POST"));
            Assert.That(request.AbsolutePath, Is.EqualTo(QuotesPath));
            Assert.That(request.Body, Does.Contain("\"user_id\":1"));
            Assert.That(request.Body, Does.Contain("\"title\":\"Test quote\""));
            Assert.That(request.Body, Does.Contain("\"contact_id\":42"));
        });
    }

    /// <summary>
    /// <c>QuoteService.Update</c> must send a <c>POST</c> (not <c>PUT</c>) request against
    /// <c>/2.0/kb_offer/{id}</c> — Bexio uses POST for full-replacement edits on this resource.
    /// </summary>
    [Test]
    public async Task QuoteService_Update_SendsPostRequest_WithIdInPath()
    {
        const int id = 1;
        var expectedPath = $"{QuotesPath}/{id}";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(QuoteResponse));

        var service = new QuoteService(ConnectionHandler);

        var payload = new QuoteUpdate(
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
    /// <c>QuoteService.Delete</c> must issue a <c>DELETE</c> request that includes the target id
    /// in the URL path.
    /// </summary>
    [Test]
    public async Task QuoteService_Delete_SendsDeleteRequest()
    {
        const int idToDelete = 1;
        var expectedPath = $"{QuotesPath}/{idToDelete}";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingDelete())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("{\"success\":true}"));

        var service = new QuoteService(ConnectionHandler);

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
    /// <c>QuoteService.Search</c> must send a <c>POST</c> request against
    /// <c>/2.0/kb_offer/search</c> with the <see cref="SearchCriteria"/> list as the JSON body.
    /// </summary>
    [Test]
    public async Task QuoteService_Search_SendsPostRequest_ToSearchPath()
    {
        var expectedPath = $"{QuotesPath}/search";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody($"[{QuoteResponse}]"));

        var service = new QuoteService(ConnectionHandler);

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
    /// <c>QuoteService.Issue</c> must send a body-less <c>POST</c> against
    /// <c>/2.0/kb_offer/{id}/issue</c>.
    /// </summary>
    [Test]
    public async Task QuoteService_Issue_SendsPostRequest_ToIssuePath()
    {
        const int id = 1;
        var expectedPath = $"{QuotesPath}/{id}/issue";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("{\"success\":true}"));

        var service = new QuoteService(ConnectionHandler);

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
    /// <c>QuoteService.RevertIssue</c> must send a body-less <c>POST</c> against
    /// <c>/2.0/kb_offer/{id}/revertIssue</c>. Note the camelCase <c>revertIssue</c> path
    /// — it differs from invoices' snake_case <c>revert_issue</c>.
    /// </summary>
    [Test]
    public async Task QuoteService_RevertIssue_SendsPostRequest_ToRevertIssuePath()
    {
        const int id = 1;
        var expectedPath = $"{QuotesPath}/{id}/revertIssue";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("{\"success\":true}"));

        var service = new QuoteService(ConnectionHandler);

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
    /// <c>QuoteService.Accept</c> must send a body-less <c>POST</c> against
    /// <c>/2.0/kb_offer/{id}/accept</c>.
    /// </summary>
    [Test]
    public async Task QuoteService_Accept_SendsPostRequest_ToAcceptPath()
    {
        const int id = 1;
        var expectedPath = $"{QuotesPath}/{id}/accept";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("{\"success\":true}"));

        var service = new QuoteService(ConnectionHandler);

        var result = await service.Accept(id, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("POST"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
        });
    }

    /// <summary>
    /// <c>QuoteService.Reject</c> must send a body-less <c>POST</c> against
    /// <c>/2.0/kb_offer/{id}/reject</c> (Bexio names the action "decline" but routes it under
    /// <c>/reject</c>).
    /// </summary>
    [Test]
    public async Task QuoteService_Reject_SendsPostRequest_ToRejectPath()
    {
        const int id = 1;
        var expectedPath = $"{QuotesPath}/{id}/reject";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("{\"success\":true}"));

        var service = new QuoteService(ConnectionHandler);

        var result = await service.Reject(id, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("POST"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
        });
    }

    /// <summary>
    /// <c>QuoteService.Reissue</c> must send a body-less <c>POST</c> against
    /// <c>/2.0/kb_offer/{id}/reissue</c>.
    /// </summary>
    [Test]
    public async Task QuoteService_Reissue_SendsPostRequest_ToReissuePath()
    {
        const int id = 1;
        var expectedPath = $"{QuotesPath}/{id}/reissue";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("{\"success\":true}"));

        var service = new QuoteService(ConnectionHandler);

        var result = await service.Reissue(id, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("POST"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
        });
    }

    /// <summary>
    /// <c>QuoteService.MarkAsSent</c> must send a body-less <c>POST</c> against
    /// <c>/2.0/kb_offer/{id}/mark_as_sent</c>.
    /// </summary>
    [Test]
    public async Task QuoteService_MarkAsSent_SendsPostRequest_ToMarkAsSentPath()
    {
        const int id = 1;
        var expectedPath = $"{QuotesPath}/{id}/mark_as_sent";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("{\"success\":true}"));

        var service = new QuoteService(ConnectionHandler);

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
    /// <c>QuoteService.Send</c> must send a <c>POST</c> against <c>/2.0/kb_offer/{id}/send</c>
    /// with the <see cref="QuoteSendRequest"/> payload.
    /// </summary>
    [Test]
    public async Task QuoteService_Send_SendsPostRequest_WithBody()
    {
        const int id = 1;
        var expectedPath = $"{QuotesPath}/{id}/send";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("{\"success\":true}"));

        var service = new QuoteService(ConnectionHandler);

        var payload = new QuoteSendRequest(
            RecipientEmail: "test@example.com",
            Subject: "Your quote",
            Message: "Please find your quote at [Network Link].");

        var result = await service.Send(id, payload, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("POST"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
            Assert.That(request.Body, Does.Contain("\"recipient_email\":\"test@example.com\""));
            Assert.That(request.Body, Does.Contain("\"subject\":\"Your quote\""));
        });
    }

    /// <summary>
    /// <c>QuoteService.Copy</c> must send a <c>POST</c> against <c>/2.0/kb_offer/{id}/copy</c>
    /// with the <see cref="QuoteCopyRequest"/> payload, and return the new quote.
    /// </summary>
    [Test]
    public async Task QuoteService_Copy_SendsPostRequest_WithBody()
    {
        const int id = 1;
        var expectedPath = $"{QuotesPath}/{id}/copy";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(201).WithBody(QuoteResponse));

        var service = new QuoteService(ConnectionHandler);

        var payload = new QuoteCopyRequest(ContactId: 42, Title: "Copy of quote");

        var result = await service.Copy(id, payload, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(request.Method, Is.EqualTo("POST"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
            Assert.That(request.Body, Does.Contain("\"contact_id\":42"));
            Assert.That(request.Body, Does.Contain("\"title\":\"Copy of quote\""));
        });
    }

    /// <summary>
    /// <c>QuoteService.CreateOrderFromQuote</c> must send a <c>POST</c> against
    /// <c>/2.0/kb_offer/{id}/order</c>. The response is returned as a generic <see cref="object"/>
    /// until a dedicated <c>Order</c> model is added in a later sub-issue.
    /// </summary>
    [Test]
    public async Task QuoteService_CreateOrderFromQuote_SendsPostRequest_ToOrderPath()
    {
        const int id = 1;
        var expectedPath = $"{QuotesPath}/{id}/order";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(201).WithBody("{\"id\":55}"));

        var service = new QuoteService(ConnectionHandler);

        var result = await service.CreateOrderFromQuote(id, new QuoteConvertRequest(), TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("POST"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
        });
    }

    /// <summary>
    /// <c>QuoteService.CreateInvoiceFromQuote</c> must send a <c>POST</c> against
    /// <c>/2.0/kb_offer/{id}/invoice</c> and surface the resulting invoice.
    /// </summary>
    [Test]
    public async Task QuoteService_CreateInvoiceFromQuote_SendsPostRequest_ToInvoicePath()
    {
        const int id = 1;
        var expectedPath = $"{QuotesPath}/{id}/invoice";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(201).WithBody(InvoiceResponse));

        var service = new QuoteService(ConnectionHandler);

        var result = await service.CreateInvoiceFromQuote(id, new QuoteConvertRequest(), TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data!.Id, Is.EqualTo(77));
            Assert.That(request.Method, Is.EqualTo("POST"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
        });
    }
}
