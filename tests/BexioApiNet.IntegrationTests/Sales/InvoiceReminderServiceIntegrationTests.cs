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
using BexioApiNet.Abstractions.Models.Sales.InvoiceReminders.Views;
using BexioApiNet.Services.Connectors.Sales;

namespace BexioApiNet.IntegrationTests.Sales;

/// <summary>
/// Integration tests covering the full surface of <see cref="InvoiceReminderService"/> against
/// WireMock stubs. Verifies that the nested path composed from
/// <see cref="InvoiceReminderConfiguration"/> (<c>2.0/kb_invoice/{invoice_id}/kb_reminder</c>)
/// reaches the handler correctly, that the expected HTTP verbs are used (including the
/// Bexio-specific body-less <c>POST</c> for action endpoints such as <c>/mark_as_sent</c>),
/// and that payloads are serialized with the expected snake_case field names.
/// </summary>
public sealed class InvoiceReminderServiceIntegrationTests : IntegrationTestBase
{
    private const int InvoiceId = 1;
    private const int ReminderId = 7;
    private static readonly string RemindersPath = $"/2.0/kb_invoice/{InvoiceId}/kb_reminder";

    private const string ReminderResponse = """
        {
            "id": 7,
            "kb_invoice_id": 1,
            "title": "First reminder",
            "is_valid_from": "2026-04-01",
            "is_valid_to": "2026-05-01",
            "reminder_period_in_days": 14,
            "reminder_level": 1,
            "show_positions": true,
            "remaining_price": "100.00",
            "received_total": "0.00",
            "is_sent": false,
            "header": null,
            "footer": null
        }
        """;

    /// <summary>
    /// <c>InvoiceReminderService.Get</c> must issue a <c>GET</c> request against
    /// <c>/2.0/kb_invoice/{invoice_id}/kb_reminder</c> and return a successful
    /// <c>ApiResult</c> when the server returns an empty array.
    /// </summary>
    [Test]
    public async Task InvoiceReminderService_Get_SendsGetRequest()
    {
        Server
            .Given(Request.Create().WithPath(RemindersPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("[]"));

        var service = new InvoiceReminderService(ConnectionHandler);

        var result = await service.Get(InvoiceId, cancellationToken: TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(RemindersPath));
        });
    }

    /// <summary>
    /// <c>InvoiceReminderService.GetById</c> must issue a <c>GET</c> request that includes the
    /// target reminder id in the URL path and surface the returned reminder on success.
    /// </summary>
    [Test]
    public async Task InvoiceReminderService_GetById_SendsGetRequest()
    {
        var expectedPath = $"{RemindersPath}/{ReminderId}";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(ReminderResponse));

        var service = new InvoiceReminderService(ConnectionHandler);

        var result = await service.GetById(InvoiceId, ReminderId, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data!.Id, Is.EqualTo(ReminderId));
            Assert.That(result.Data!.KbInvoiceId, Is.EqualTo(InvoiceId));
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
        });
    }

    /// <summary>
    /// <c>InvoiceReminderService.GetPdf</c> must issue a <c>GET</c> request against
    /// <c>/2.0/kb_invoice/{invoice_id}/kb_reminder/{reminder_id}/pdf</c> and return
    /// the raw PDF bytes.
    /// </summary>
    [Test]
    public async Task InvoiceReminderService_GetPdf_SendsGetRequest_AndReturnsBytes()
    {
        var expectedPath = $"{RemindersPath}/{ReminderId}/pdf";
        var pdfBytes = new byte[] { 0x25, 0x50, 0x44, 0x46 }; // "%PDF"

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/pdf")
                .WithBody(pdfBytes));

        var service = new InvoiceReminderService(ConnectionHandler);

        var result = await service.GetPdf(InvoiceId, ReminderId, TestContext.CurrentContext.CancellationToken);

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
    /// <c>InvoiceReminderService.Create</c> must send a <c>POST</c> request whose body is the
    /// serialized <see cref="InvoiceReminderCreate"/> payload, and must surface the returned
    /// reminder on success.
    /// </summary>
    [Test]
    public async Task InvoiceReminderService_Create_SendsPostRequest()
    {
        Server
            .Given(Request.Create().WithPath(RemindersPath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(201).WithBody(ReminderResponse));

        var service = new InvoiceReminderService(ConnectionHandler);

        var payload = new InvoiceReminderCreate(
            Title: "First reminder",
            ReminderPeriodInDays: 14,
            ShowPositions: true);

        var result = await service.Create(InvoiceId, payload, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data!.Id, Is.EqualTo(ReminderId));
            Assert.That(request.Method, Is.EqualTo("POST"));
            Assert.That(request.AbsolutePath, Is.EqualTo(RemindersPath));
            Assert.That(request.Body, Does.Contain("\"title\":\"First reminder\""));
            Assert.That(request.Body, Does.Contain("\"reminder_period_in_days\":14"));
            Assert.That(request.Body, Does.Contain("\"show_positions\":true"));
        });
    }

    /// <summary>
    /// <c>InvoiceReminderService.Search</c> must send a <c>POST</c> request against
    /// <c>/2.0/kb_invoice/{invoice_id}/kb_reminder/search</c> with the <see cref="SearchCriteria"/>
    /// list as the JSON body.
    /// </summary>
    [Test]
    public async Task InvoiceReminderService_Search_SendsPostRequest_ToSearchPath()
    {
        var expectedPath = $"{RemindersPath}/search";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody($"[{ReminderResponse}]"));

        var service = new InvoiceReminderService(ConnectionHandler);

        var criteria = new List<SearchCriteria>
        {
            new() { Field = "title", Value = "First", Criteria = "like" }
        };

        var result = await service.Search(InvoiceId, criteria, cancellationToken: TestContext.CurrentContext.CancellationToken);

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
    /// <c>InvoiceReminderService.Send</c> must send a <c>POST</c> against
    /// <c>/2.0/kb_invoice/{invoice_id}/kb_reminder/{reminder_id}/send</c> with the serialized
    /// <see cref="InvoiceReminderSendRequest"/> payload.
    /// </summary>
    [Test]
    public async Task InvoiceReminderService_Send_SendsPostRequest_WithBody()
    {
        var expectedPath = $"{RemindersPath}/{ReminderId}/send";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("{\"success\":true}"));

        var service = new InvoiceReminderService(ConnectionHandler);

        var payload = new InvoiceReminderSendRequest(
            RecipientEmail: "reminder@example.com",
            Subject: "Overdue invoice",
            Message: "Please find the document at [Network Link].");

        var result = await service.Send(InvoiceId, ReminderId, payload, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("POST"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
            Assert.That(request.Body, Does.Contain("\"recipient_email\":\"reminder@example.com\""));
            Assert.That(request.Body, Does.Contain("\"subject\":\"Overdue invoice\""));
        });
    }

    /// <summary>
    /// <c>InvoiceReminderService.MarkAsSent</c> must send a body-less <c>POST</c> against
    /// <c>/2.0/kb_invoice/{invoice_id}/kb_reminder/{reminder_id}/mark_as_sent</c>.
    /// </summary>
    [Test]
    public async Task InvoiceReminderService_MarkAsSent_SendsPostRequest_ToMarkAsSentPath()
    {
        var expectedPath = $"{RemindersPath}/{ReminderId}/mark_as_sent";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("{\"success\":true}"));

        var service = new InvoiceReminderService(ConnectionHandler);

        var result = await service.MarkAsSent(InvoiceId, ReminderId, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("POST"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
        });
    }

    /// <summary>
    /// <c>InvoiceReminderService.MarkAsUnsent</c> must send a body-less <c>POST</c> against
    /// <c>/2.0/kb_invoice/{invoice_id}/kb_reminder/{reminder_id}/mark_as_unsent</c>.
    /// </summary>
    [Test]
    public async Task InvoiceReminderService_MarkAsUnsent_SendsPostRequest_ToMarkAsUnsentPath()
    {
        var expectedPath = $"{RemindersPath}/{ReminderId}/mark_as_unsent";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("{\"success\":true}"));

        var service = new InvoiceReminderService(ConnectionHandler);

        var result = await service.MarkAsUnsent(InvoiceId, ReminderId, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("POST"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
        });
    }

    /// <summary>
    /// <c>InvoiceReminderService.Delete</c> must issue a <c>DELETE</c> request that includes
    /// the target reminder id in the URL path.
    /// </summary>
    [Test]
    public async Task InvoiceReminderService_Delete_SendsDeleteRequest()
    {
        var expectedPath = $"{RemindersPath}/{ReminderId}";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingDelete())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("{\"success\":true}"));

        var service = new InvoiceReminderService(ConnectionHandler);

        var result = await service.Delete(InvoiceId, ReminderId, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("DELETE"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
        });
    }
}
