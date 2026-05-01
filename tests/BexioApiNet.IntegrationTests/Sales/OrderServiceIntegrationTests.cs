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
using BexioApiNet.Abstractions.Models.Sales.Orders;
using BexioApiNet.Abstractions.Models.Sales.Orders.Views;
using BexioApiNet.Services.Connectors.Sales;
using System.Text.Json;

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
    /// <c>/2.0/kb_order/{id}/delivery</c> and surface the resulting
    /// <see cref="Abstractions.Models.Sales.Deliveries.Delivery"/>.
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
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data!.Id, Is.EqualTo(99));
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

        var payload = new OrderRepetitionCreate(
            Start: "2026-04-01",
            Repetition: new OrderRepetitionDaily { Interval = 1 });

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

    /// <summary>
    /// Asserts every field declared by the Bexio <c>Order</c> / <c>OrderWithDetails</c> schema in
    /// <c>doc/openapi/bexio-v3.json</c> deserializes onto the <see cref="Order"/> record with the
    /// expected value. Guards against silent drift between the spec and the model.
    /// </summary>
    [Test]
    public async Task OrderService_GetById_DeserializesAllFieldsFromSpec()
    {
        const int id = 1;
        var expectedPath = $"{OrdersPath}/{id}";

        const string fullyPopulatedOrder = """
            {
                "id": 1,
                "document_nr": "AU-00001",
                "title": "Sample order",
                "contact_id": 14,
                "contact_sub_id": 21,
                "user_id": 1,
                "project_id": 99,
                "pr_project_id": null,
                "logopaper_id": 1,
                "language_id": 1,
                "bank_account_id": 1,
                "currency_id": 1,
                "payment_type_id": 1,
                "header": "Order header",
                "footer": "Order footer",
                "total_gross": "17.800000",
                "total_net": "17.800000",
                "total_taxes": "1.3706",
                "total": "19.150000",
                "total_rounding_difference": -0.02,
                "mwst_type": 0,
                "mwst_is_net": true,
                "show_position_taxes": false,
                "is_valid_from": "2019-06-24",
                "contact_address": "Muster AG\nMusterstrasse 15\n8640 Rapperswil",
                "contact_address_manual": null,
                "delivery_address_type": 0,
                "delivery_address": "Muster AG\nMusterstrasse 15\n8640 Rapperswil",
                "delivery_address_manual": null,
                "kb_item_status_id": 5,
                "is_recurring": false,
                "api_reference": "ext-002",
                "viewed_by_client_at": "2019-06-25 09:00:00",
                "updated_at": "2019-04-08 13:17:32",
                "template_slug": "581a8010821e01426b8b456b",
                "taxs": [
                    { "percentage": "7.70", "value": "1.3706" }
                ],
                "network_link": "https://office.bexio.com/share/order/abc",
                "positions": []
            }
            """;

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(fullyPopulatedOrder));

        var service = new OrderService(ConnectionHandler);

        var result = await service.GetById(id, TestContext.CurrentContext.CancellationToken);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data, Is.Not.Null);
        var order = result.Data!;

        Assert.Multiple(() =>
        {
            Assert.That(order.Id, Is.EqualTo(1));
            Assert.That(order.DocumentNr, Is.EqualTo("AU-00001"));
            Assert.That(order.Title, Is.EqualTo("Sample order"));
            Assert.That(order.ContactId, Is.EqualTo(14));
            Assert.That(order.ContactSubId, Is.EqualTo(21));
            Assert.That(order.UserId, Is.EqualTo(1));
            Assert.That(order.ProjectId, Is.EqualTo(99));
            Assert.That(order.PrProjectId, Is.Null);
            Assert.That(order.LogopaperId, Is.EqualTo(1));
            Assert.That(order.LanguageId, Is.EqualTo(1));
            Assert.That(order.BankAccountId, Is.EqualTo(1));
            Assert.That(order.CurrencyId, Is.EqualTo(1));
            Assert.That(order.PaymentTypeId, Is.EqualTo(1));
            Assert.That(order.Header, Is.EqualTo("Order header"));
            Assert.That(order.Footer, Is.EqualTo("Order footer"));
            Assert.That(order.TotalGross, Is.EqualTo("17.800000"));
            Assert.That(order.TotalNet, Is.EqualTo("17.800000"));
            Assert.That(order.TotalTaxes, Is.EqualTo("1.3706"));
            Assert.That(order.Total, Is.EqualTo("19.150000"));
            Assert.That(order.TotalRoundingDifference, Is.EqualTo(-0.02m));
            Assert.That(order.MwstType, Is.EqualTo(0));
            Assert.That(order.MwstIsNet, Is.True);
            Assert.That(order.ShowPositionTaxes, Is.False);
            Assert.That(order.IsValidFrom, Is.EqualTo("2019-06-24"));
            Assert.That(order.ContactAddress, Is.EqualTo("Muster AG\nMusterstrasse 15\n8640 Rapperswil"));
            Assert.That(order.ContactAddressManual, Is.Null);
            Assert.That(order.DeliveryAddressType, Is.EqualTo(0));
            Assert.That(order.DeliveryAddress, Is.EqualTo("Muster AG\nMusterstrasse 15\n8640 Rapperswil"));
            Assert.That(order.DeliveryAddressManual, Is.Null);
            Assert.That(order.KbItemStatusId, Is.EqualTo(5));
            Assert.That(order.IsRecurring, Is.False);
            Assert.That(order.ApiReference, Is.EqualTo("ext-002"));
            Assert.That(order.ViewedByClientAt, Is.EqualTo("2019-06-25 09:00:00"));
            Assert.That(order.UpdatedAt, Is.EqualTo("2019-04-08 13:17:32"));
            Assert.That(order.TemplateSlug, Is.EqualTo("581a8010821e01426b8b456b"));
            Assert.That(order.Taxs, Is.Not.Null);
            Assert.That(order.Taxs!, Has.Count.EqualTo(1));
            Assert.That(order.Taxs![0].Percentage, Is.EqualTo("7.70"));
            Assert.That(order.Taxs![0].Value, Is.EqualTo("1.3706"));
            Assert.That(order.NetworkLink, Is.EqualTo("https://office.bexio.com/share/order/abc"));
            Assert.That(order.Positions, Is.Not.Null);
            Assert.That(order.Positions!, Is.Empty);
        });
    }

    /// <summary>
    /// Cross-checks that every property name declared on the <c>Order</c> schema in the OpenAPI
    /// document maps to an <see cref="Order"/> property via its <c>JsonPropertyName</c> attribute.
    /// </summary>
    [Test]
    public void Order_AllSpecPropertyNames_AreCoveredByModel()
    {
        var specProperties = new[]
        {
            "id", "document_nr", "title", "contact_id", "contact_sub_id", "user_id", "project_id",
            "pr_project_id", "logopaper_id", "language_id", "bank_account_id", "currency_id",
            "payment_type_id", "header", "footer", "total_gross", "total_net", "total_taxes",
            "total", "total_rounding_difference", "mwst_type", "mwst_is_net", "show_position_taxes",
            "is_valid_from", "contact_address", "contact_address_manual", "delivery_address_type",
            "delivery_address", "delivery_address_manual", "kb_item_status_id", "is_recurring",
            "api_reference", "viewed_by_client_at", "updated_at", "template_slug", "taxs",
            "network_link", "positions"
        };

        var modelJsonNames = typeof(Order)
            .GetProperties()
            .Select(p => p.GetCustomAttributes(typeof(System.Text.Json.Serialization.JsonPropertyNameAttribute), false)
                .Cast<System.Text.Json.Serialization.JsonPropertyNameAttribute>()
                .Select(a => a.Name)
                .FirstOrDefault())
            .Where(name => name is not null)
            .ToHashSet();

        Assert.Multiple(() =>
        {
            foreach (var name in specProperties)
                Assert.That(modelJsonNames, Does.Contain(name), $"Order model is missing JSON property: {name}");
        });
    }

    /// <summary>
    /// Verifies that the <c>OrderRepetition</c> schema deserializes both into the
    /// <see cref="OrderRepetition"/> wrapper and into the polymorphic <see cref="OrderRepetitionDaily"/>
    /// subtype identified by its <c>type</c> discriminator.
    /// </summary>
    [Test]
    public async Task OrderService_GetRepetition_DeserializesPolymorphicSchedule()
    {
        const int id = 1;
        var expectedPath = $"{OrdersPath}/{id}/repetition";

        const string fullyPopulatedRepetition = """
            {
                "start": "2019-01-01",
                "end": "2019-12-31",
                "repetition": { "type": "daily", "interval": 5 }
            }
            """;

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(fullyPopulatedRepetition));

        var service = new OrderService(ConnectionHandler);

        var result = await service.GetRepetition(id, TestContext.CurrentContext.CancellationToken);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data, Is.Not.Null);
        var repetition = result.Data!;

        Assert.Multiple(() =>
        {
            Assert.That(repetition.Start, Is.EqualTo("2019-01-01"));
            Assert.That(repetition.End, Is.EqualTo("2019-12-31"));
            Assert.That(repetition.Repetition, Is.InstanceOf<OrderRepetitionDaily>());
            Assert.That(((OrderRepetitionDaily)repetition.Repetition!).Interval, Is.EqualTo(5));
        });
    }

    /// <summary>
    /// Verifies that <c>OrderService.Get</c> forwards <c>limit</c>, <c>offset</c> and
    /// <c>order_by</c> from <see cref="Models.QueryParameterOrder"/> as URL query parameters,
    /// matching the three optional query parameters declared on <c>GET /2.0/kb_order</c>.
    /// </summary>
    [Test]
    public async Task OrderService_Get_ForwardsQueryParameters()
    {
        Server
            .Given(Request.Create().WithPath(OrdersPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("[]"));

        var service = new OrderService(ConnectionHandler);

        var queryParameter = new Models.QueryParameterOrder(Limit: 50, Offset: 10, OrderBy: "id_desc");

        await service.Get(queryParameter, cancellationToken: TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;
        var queryString = request.RawQuery ?? string.Empty;

        Assert.Multiple(() =>
        {
            Assert.That(queryString, Does.Contain("limit=50"));
            Assert.That(queryString, Does.Contain("offset=10"));
            Assert.That(queryString, Does.Contain("order_by=id_desc"));
        });
    }

    /// <summary>
    /// Sanity check that the canned <c>OrderResponse</c> stays valid JSON. Catches accidental
    /// breakage of the fixture when fields are added or removed.
    /// </summary>
    [Test]
    public void OrderResponse_FixtureIsValidJson()
    {
        using var doc = JsonDocument.Parse(OrderResponse);
        var root = doc.RootElement;

        Assert.Multiple(() =>
        {
            Assert.That(root.TryGetProperty("id", out _), Is.True);
            Assert.That(root.TryGetProperty("kb_item_status_id", out _), Is.True);
            Assert.That(root.TryGetProperty("is_recurring", out _), Is.True);
        });
    }
}
