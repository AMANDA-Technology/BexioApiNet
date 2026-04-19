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

using System.Text.Json;
using BexioApiNet.Abstractions.Models.Api;
using BexioApiNet.Abstractions.Models.Sales.Orders.Views;
using BexioApiNet.Services.Connectors.Sales;

namespace BexioApiNet.IntegrationTests.Sales;

/// <summary>
/// Integration tests covering the full surface of <see cref="OrderService"/> against WireMock
/// stubs. Verifies the path composed from <see cref="OrderConfiguration"/>
/// (<c>2.0/kb_order</c>) reaches the handler correctly, that the expected HTTP verbs are used
/// (including the Bexio-specific <c>POST</c> for edits and the nested
/// <c>/repetition</c>, <c>/delivery</c> and <c>/invoice</c> routes), and that payloads are
/// serialized with the expected snake_case field names.
/// </summary>
public sealed class OrderServiceIntegrationTests : IntegrationTestBase
{
    private const string OrdersPath = "/2.0/kb_order";

    private const string OrderResponse = """
        {
            "id": 1,
            "document_nr": "AU-1000",
            "title": "Test order",
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
            "contact_address": null,
            "contact_address_manual": null,
            "delivery_address_type": 0,
            "delivery_address": null,
            "delivery_address_manual": null,
            "kb_item_status_id": 5,
            "is_recurring": false,
            "api_reference": null,
            "viewed_by_client_at": null,
            "updated_at": "2026-04-01 12:00:00",
            "template_slug": null,
            "taxs": [],
            "network_link": null,
            "positions": []
        }
        """;

    private const string OrderRepetitionResponse = """
        {
            "start": "2026-04-01",
            "end": null,
            "repetition": { "type": "daily", "interval": 1 }
        }
        """;

    private const string InvoiceResponse = """
        {
            "id": 77,
            "document_nr": "RE-1000",
            "title": "Invoice from order",
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
    /// <c>OrderService.Get()</c> must issue a <c>GET</c> request against <c>/2.0/kb_order</c>
    /// and return a successful <c>ApiResult</c> when the server returns an empty array.
    /// </summary>
    [Test]
    public async Task OrderService_Get_SendsGetRequest()
    {
        Server
            .Given(Request.Create().WithPath(OrdersPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("[]"));

        var service = new OrderService(ConnectionHandler);

        var result = await service.Get(cancellationToken: TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(OrdersPath));
        });
    }

    /// <summary>
    /// <c>OrderService.GetById</c> must issue a <c>GET</c> request that includes the target id
    /// in the URL path and surface the returned order on success.
    /// </summary>
    [Test]
    public async Task OrderService_GetById_SendsGetRequest()
    {
        const int id = 1;
        var expectedPath = $"{OrdersPath}/{id}";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(OrderResponse));

        var service = new OrderService(ConnectionHandler);

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
    /// <c>OrderService.GetPdf</c> must issue a <c>GET</c> request against
    /// <c>/2.0/kb_order/{id}/pdf</c> and return the raw PDF bytes.
    /// </summary>
    [Test]
    public async Task OrderService_GetPdf_SendsGetRequest_AndReturnsBytes()
    {
        const int id = 1;
        var expectedPath = $"{OrdersPath}/{id}/pdf";
        var pdfBytes = new byte[] { 0x25, 0x50, 0x44, 0x46 }; // "%PDF"

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/pdf")
                .WithBody(pdfBytes));

        var service = new OrderService(ConnectionHandler);

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
    /// <c>OrderService.GetRepetition</c> must issue a <c>GET</c> request against
    /// <c>/2.0/kb_order/{id}/repetition</c> and return the repetition descriptor.
    /// </summary>
    [Test]
    public async Task OrderService_GetRepetition_SendsGetRequest()
    {
        const int id = 1;
        var expectedPath = $"{OrdersPath}/{id}/repetition";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(OrderRepetitionResponse));

        var service = new OrderService(ConnectionHandler);

        var result = await service.GetRepetition(id, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data!.Start, Is.EqualTo("2026-04-01"));
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
        });
    }

    /// <summary>
    /// <c>OrderService.Create</c> must send a <c>POST</c> request whose body is the serialized
    /// <see cref="OrderCreate"/> payload, and must surface the returned order on success.
    /// </summary>
    [Test]
    public async Task OrderService_Create_SendsPostRequest()
    {
        Server
            .Given(Request.Create().WithPath(OrdersPath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(201).WithBody(OrderResponse));

        var service = new OrderService(ConnectionHandler);

        var payload = new OrderCreate(
            UserId: 1,
            Title: "Test order",
            ContactId: 42);

        var result = await service.Create(payload, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data!.Id, Is.EqualTo(1));
            Assert.That(request.Method, Is.EqualTo("POST"));
            Assert.That(request.AbsolutePath, Is.EqualTo(OrdersPath));
            Assert.That(request.Body, Does.Contain("\"user_id\":1"));
            Assert.That(request.Body, Does.Contain("\"title\":\"Test order\""));
            Assert.That(request.Body, Does.Contain("\"contact_id\":42"));
        });
    }

    /// <summary>
    /// <c>OrderService.Update</c> must send a <c>POST</c> (not <c>PUT</c>) request against
    /// <c>/2.0/kb_order/{id}</c> — Bexio uses POST for full-replacement edits on this resource.
    /// </summary>
    [Test]
    public async Task OrderService_Update_SendsPostRequest_WithIdInPath()
    {
        const int id = 1;
        var expectedPath = $"{OrdersPath}/{id}";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(OrderResponse));

        var service = new OrderService(ConnectionHandler);

        var payload = new OrderUpdate(
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
    /// <c>OrderService.Delete</c> must issue a <c>DELETE</c> request that includes the target id
    /// in the URL path.
    /// </summary>
    [Test]
    public async Task OrderService_Delete_SendsDeleteRequest()
    {
        const int idToDelete = 1;
        var expectedPath = $"{OrdersPath}/{idToDelete}";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingDelete())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("{\"success\":true}"));

        var service = new OrderService(ConnectionHandler);

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
    /// <c>OrderService.Search</c> must send a <c>POST</c> request against
    /// <c>/2.0/kb_order/search</c> with the <see cref="SearchCriteria"/> list as the JSON body.
    /// </summary>
    [Test]
    public async Task OrderService_Search_SendsPostRequest_ToSearchPath()
    {
        var expectedPath = $"{OrdersPath}/search";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody($"[{OrderResponse}]"));

        var service = new OrderService(ConnectionHandler);

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
    /// <c>OrderService.CreateDeliveryFromOrder</c> must send a <c>POST</c> against
    /// <c>/2.0/kb_order/{id}/delivery</c>. The response is returned as a generic
    /// <see cref="object"/> until a dedicated <c>Delivery</c> model is added in a later sub-issue.
    /// </summary>
    [Test]
    public async Task OrderService_CreateDeliveryFromOrder_SendsPostRequest_ToDeliveryPath()
    {
        const int id = 1;
        var expectedPath = $"{OrdersPath}/{id}/delivery";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(201).WithBody("{\"id\":99}"));

        var service = new OrderService(ConnectionHandler);

        var result = await service.CreateDeliveryFromOrder(id, new OrderConvertRequest(), TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("POST"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
        });
    }

    /// <summary>
    /// <c>OrderService.CreateInvoiceFromOrder</c> must send a <c>POST</c> against
    /// <c>/2.0/kb_order/{id}/invoice</c> and surface the resulting invoice.
    /// </summary>
    [Test]
    public async Task OrderService_CreateInvoiceFromOrder_SendsPostRequest_ToInvoicePath()
    {
        const int id = 1;
        var expectedPath = $"{OrdersPath}/{id}/invoice";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(201).WithBody(InvoiceResponse));

        var service = new OrderService(ConnectionHandler);

        var result = await service.CreateInvoiceFromOrder(id, new OrderConvertRequest(), TestContext.CurrentContext.CancellationToken);

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

    /// <summary>
    /// <c>OrderService.CreateRepetition</c> must send a <c>POST</c> against
    /// <c>/2.0/kb_order/{id}/repetition</c> with the serialized <see cref="OrderRepetitionCreate"/>
    /// payload and surface the returned repetition descriptor.
    /// </summary>
    [Test]
    public async Task OrderService_CreateRepetition_SendsPostRequest_ToRepetitionPath()
    {
        const int id = 1;
        var expectedPath = $"{OrdersPath}/{id}/repetition";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(201).WithBody(OrderRepetitionResponse));

        var service = new OrderService(ConnectionHandler);

        using var repetition = JsonDocument.Parse("""{"type":"daily","interval":1}""");
        var payload = new OrderRepetitionCreate(
            Start: "2026-04-01",
            Repetition: repetition.RootElement.Clone());

        var result = await service.CreateRepetition(id, payload, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data!.Start, Is.EqualTo("2026-04-01"));
            Assert.That(request.Method, Is.EqualTo("POST"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
            Assert.That(request.Body, Does.Contain("\"start\":\"2026-04-01\""));
            Assert.That(request.Body, Does.Contain("\"type\":\"daily\""));
        });
    }

    /// <summary>
    /// <c>OrderService.DeleteRepetition</c> must issue a <c>DELETE</c> request against
    /// <c>/2.0/kb_order/{id}/repetition</c>.
    /// </summary>
    [Test]
    public async Task OrderService_DeleteRepetition_SendsDeleteRequest_ToRepetitionPath()
    {
        const int id = 1;
        var expectedPath = $"{OrdersPath}/{id}/repetition";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingDelete())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("{\"success\":true}"));

        var service = new OrderService(ConnectionHandler);

        var result = await service.DeleteRepetition(id, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("DELETE"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
        });
    }
}
