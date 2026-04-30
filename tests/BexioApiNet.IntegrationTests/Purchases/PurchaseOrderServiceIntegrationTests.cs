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

using BexioApiNet.Abstractions.Models.Purchases.PurchaseOrders.Views;
using BexioApiNet.Models;
using BexioApiNet.Services.Connectors.Purchases;

namespace BexioApiNet.IntegrationTests.Purchases;

/// <summary>
/// Integration tests for <see cref="PurchaseOrderService"/> against WireMock stubs.
/// Verifies the path composed from <see cref="PurchaseOrderConfiguration"/>
/// (<c>3.0/purchase_orders</c>) reaches the handler correctly and that the expected
/// HTTP verbs are used per the v3.0.0 OpenAPI spec — including <c>PUT</c> for updates
/// (<see href="https://docs.bexio.com/#tag/Purchase-Orders/operation/v3PurchaseOrderUpdate" />).
/// Responses round-trip through the canonical <see cref="BexioApiNet.Abstractions.Models.Purchases.PurchaseOrders.PurchaseOrder" />
/// schema using the spec field name <c>document_nr</c>.
/// </summary>
public sealed class PurchaseOrderServiceIntegrationTests : IntegrationTestBase
{
    private const string PurchaseOrdersPath = "/3.0/purchase_orders";

    private const int TestPurchaseOrderId = 42;

    /// <summary>
    /// Fully-populated PurchaseOrder response matching the v3.0.0 OpenAPI <c>Purchase Order</c>
    /// schema (including the spec's <c>document_nr</c> field, both validity dates,
    /// MwSt-related flags, address overrides and read-only timestamps).
    /// </summary>
    private const string PurchaseOrderResponse = """
                                                 {
                                                     "id": 42,
                                                     "document_nr": "PO-1001",
                                                     "kb_payment_template_id": null,
                                                     "payment_type_id": 1,
                                                     "title": "Office supplies",
                                                     "contact_id": 1323,
                                                     "contact_sub_id": null,
                                                     "template_slug": "581a8010821e01426b8b456b",
                                                     "user_id": 1,
                                                     "project_id": null,
                                                     "logopaper_id": 1,
                                                     "language_id": 1,
                                                     "bank_account_id": 1,
                                                     "currency_id": 1,
                                                     "header": "We would like to order the following products:",
                                                     "footer": "Many thanks for the fast processing of our order.",
                                                     "total_rounding_difference": -0.02,
                                                     "mwst_type": "included",
                                                     "mwst_is_net": true,
                                                     "is_compact_view": false,
                                                     "show_position_taxes": false,
                                                     "salesman_user_id": 1,
                                                     "is_valid_from": "2026-04-01",
                                                     "is_valid_to": "2026-05-01",
                                                     "is_valid_until": "2026-05-01",
                                                     "delivery_address_type": "manual",
                                                     "contact_address_manual": "bexio AG\nReinluftweg 1\nCH - 9630 Wattwil",
                                                     "delivery_address_manual": "bexio AG\nReinluftweg 1\nCH - 9630 Wattwil",
                                                     "nb_decimals_amount": 2,
                                                     "nb_decimals_price": 2,
                                                     "kb_item_status_id": 22,
                                                     "terms_of_payment_text": "Payable within 30 days",
                                                     "reference": "Based on Quote Q-3860",
                                                     "api_reference": null,
                                                     "mail": "support@bexio.com",
                                                     "viewed_by_client_at": null,
                                                     "date_format": "d.m.Y",
                                                     "created_at": "2026-04-01",
                                                     "updated_at": "2026-04-02"
                                                 }
                                                 """;

    private const string PurchaseOrderListBody = """
                                                 [
                                                     {
                                                         "id": 42,
                                                         "document_nr": "PO-1001",
                                                         "title": "Office supplies",
                                                         "contact_id": 1323,
                                                         "currency_id": 1,
                                                         "user_id": 1,
                                                         "is_valid_from": "2026-04-01",
                                                         "is_valid_until": "2026-05-01"
                                                     },
                                                     {
                                                         "id": 43,
                                                         "document_nr": "PO-1002",
                                                         "title": "IT consumables",
                                                         "contact_id": 1324,
                                                         "currency_id": 1,
                                                         "user_id": 1,
                                                         "is_valid_from": "2026-04-15",
                                                         "is_valid_until": "2026-05-15"
                                                     }
                                                 ]
                                                 """;

    /// <summary>
    /// <c>PurchaseOrderService.Get</c> issues a <c>GET</c> against
    /// <c>/3.0/purchase_orders</c> and deserializes the array of orders, surfacing
    /// the spec's <c>document_nr</c> field on every entry.
    /// </summary>
    [Test]
    public async Task PurchaseOrderService_Get_SendsGetRequest_DeserializesList()
    {
        Server
            .Given(Request.Create().WithPath(PurchaseOrdersPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(PurchaseOrderListBody));

        var service = new PurchaseOrderService(ConnectionHandler);

        var result = await service.Get(cancellationToken: TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data!, Has.Count.EqualTo(2));
            Assert.That(result.Data![0].Id, Is.EqualTo(42));
            Assert.That(result.Data[0].DocumentNr, Is.EqualTo("PO-1001"));
            Assert.That(result.Data[0].Title, Is.EqualTo("Office supplies"));
            Assert.That(result.Data[0].ContactId, Is.EqualTo(1323));
            Assert.That(result.Data[0].CurrencyId, Is.EqualTo(1));
            Assert.That(result.Data[0].UserId, Is.EqualTo(1));
            Assert.That(result.Data[1].Id, Is.EqualTo(43));
            Assert.That(result.Data[1].DocumentNr, Is.EqualTo("PO-1002"));
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(PurchaseOrdersPath));
        });
    }

    /// <summary>
    /// <c>PurchaseOrderService.Get</c> with a populated <see cref="QueryParameter"/>
    /// appends the supplied keys to the URL.
    /// </summary>
    [Test]
    public async Task PurchaseOrderService_Get_WithQueryParameter_AppendsKeys()
    {
        Server
            .Given(Request.Create().WithPath(PurchaseOrdersPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("[]"));

        var service = new PurchaseOrderService(ConnectionHandler);

        var queryParameter = new QueryParameter(new Dictionary<string, object>
        {
            ["limit"] = 50,
            ["offset"] = 100,
            ["order_by"] = "document_nr"
        });

        var result = await service.Get(queryParameter, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Url, Does.Contain("limit=50"));
            Assert.That(request.Url, Does.Contain("offset=100"));
            Assert.That(request.Url, Does.Contain("order_by=document_nr"));
        });
    }

    /// <summary>
    /// <c>PurchaseOrderService.GetById</c> issues a <c>GET</c> request with the
    /// purchase order id in the URL path and deserializes every property of the full
    /// <c>PurchaseOrderWithDetails</c> schema — including the spec's <c>document_nr</c>,
    /// the address overrides and read-only timestamps.
    /// </summary>
    [Test]
    public async Task PurchaseOrderService_GetById_DeserializesFullPurchaseOrder()
    {
        var expectedPath = $"{PurchaseOrdersPath}/{TestPurchaseOrderId}";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(PurchaseOrderResponse));

        var service = new PurchaseOrderService(ConnectionHandler);

        var result = await service.GetById(TestPurchaseOrderId, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);

            var po = result.Data!;
            Assert.That(po.Id, Is.EqualTo(TestPurchaseOrderId));
            Assert.That(po.DocumentNr, Is.EqualTo("PO-1001"));
            Assert.That(po.PaymentTypeId, Is.EqualTo(1));
            Assert.That(po.Title, Is.EqualTo("Office supplies"));
            Assert.That(po.ContactId, Is.EqualTo(1323));
            Assert.That(po.UserId, Is.EqualTo(1));
            Assert.That(po.CurrencyId, Is.EqualTo(1));
            Assert.That(po.LanguageId, Is.EqualTo(1));
            Assert.That(po.BankAccountId, Is.EqualTo(1));
            Assert.That(po.LogopaperId, Is.EqualTo(1));
            Assert.That(po.TemplateSlug, Is.EqualTo("581a8010821e01426b8b456b"));
            Assert.That(po.Header, Is.EqualTo("We would like to order the following products:"));
            Assert.That(po.Footer, Is.EqualTo("Many thanks for the fast processing of our order."));
            Assert.That(po.TotalRoundingDifference, Is.EqualTo(-0.02m));
            Assert.That(po.MwstType, Is.EqualTo("included"));
            Assert.That(po.MwstIsNet, Is.True);
            Assert.That(po.IsCompactView, Is.False);
            Assert.That(po.ShowPositionTaxes, Is.False);
            Assert.That(po.SalesmanUserId, Is.EqualTo(1));
            Assert.That(po.IsValidFrom, Is.EqualTo("2026-04-01"));
            Assert.That(po.IsValidTo, Is.EqualTo("2026-05-01"));
            Assert.That(po.IsValidUntil, Is.EqualTo("2026-05-01"));
            Assert.That(po.DeliveryAddressType, Is.EqualTo("manual"));
            Assert.That(po.ContactAddressManual, Does.Contain("bexio AG"));
            Assert.That(po.DeliveryAddressManual, Does.Contain("Reinluftweg"));
            Assert.That(po.NbDecimalsAmount, Is.EqualTo(2));
            Assert.That(po.NbDecimalsPrice, Is.EqualTo(2));
            Assert.That(po.KbItemStatusId, Is.EqualTo(22));
            Assert.That(po.TermsOfPaymentText, Is.EqualTo("Payable within 30 days"));
            Assert.That(po.Reference, Is.EqualTo("Based on Quote Q-3860"));
            Assert.That(po.Mail, Is.EqualTo("support@bexio.com"));
            Assert.That(po.DateFormat, Is.EqualTo("d.m.Y"));
            Assert.That(po.CreatedAt, Is.EqualTo("2026-04-01"));
            Assert.That(po.UpdatedAt, Is.EqualTo("2026-04-02"));

            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
        });
    }

    /// <summary>
    /// <c>PurchaseOrderService.Create</c> sends a <c>POST</c> request whose body contains
    /// the serialized <see cref="PurchaseOrderCreate"/> payload. The body must use the
    /// spec field names — most notably <c>document_nr</c> (not <c>document_no</c>).
    /// </summary>
    [Test]
    public async Task PurchaseOrderService_Create_SendsPostRequestWithPayload()
    {
        Server
            .Given(Request.Create().WithPath(PurchaseOrdersPath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(201).WithBody(PurchaseOrderResponse));

        var service = new PurchaseOrderService(ConnectionHandler);

        var payload = new PurchaseOrderCreate(
            ContactId: 1323,
            CurrencyId: 1,
            UserId: 1,
            DocumentNr: "PO-1001",
            Title: "Office supplies");

        var result = await service.Create(payload, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data!.DocumentNr, Is.EqualTo("PO-1001"));
            Assert.That(request.Method, Is.EqualTo("POST"));
            Assert.That(request.AbsolutePath, Is.EqualTo(PurchaseOrdersPath));
            Assert.That(request.Body, Does.Contain("\"contact_id\":1323"));
            Assert.That(request.Body, Does.Contain("\"currency_id\":1"));
            Assert.That(request.Body, Does.Contain("\"user_id\":1"));
            Assert.That(request.Body, Does.Contain("\"document_nr\":\"PO-1001\""));
            Assert.That(request.Body, Does.Contain("\"title\":\"Office supplies\""));
        });
    }

    /// <summary>
    /// <c>PurchaseOrderService.Update</c> sends a <c>PUT</c> request against
    /// <c>/3.0/purchase_orders/{id}</c> per the v3.0.0 OpenAPI spec.
    /// </summary>
    [Test]
    public async Task PurchaseOrderService_Update_SendsPutRequestWithIdInPath()
    {
        var expectedPath = $"{PurchaseOrdersPath}/{TestPurchaseOrderId}";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingPut())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(PurchaseOrderResponse));

        var service = new PurchaseOrderService(ConnectionHandler);

        var payload = new PurchaseOrderUpdate(
            ContactId: 1323,
            CurrencyId: 1,
            UserId: 1,
            DocumentNr: "PO-1001",
            Title: "Office supplies (updated)");

        var result = await service.Update(TestPurchaseOrderId, payload, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data!.DocumentNr, Is.EqualTo("PO-1001"));
            Assert.That(request.Method, Is.EqualTo("PUT"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
            Assert.That(request.Body, Does.Contain("\"document_nr\":\"PO-1001\""));
        });
    }

    /// <summary>
    /// <c>PurchaseOrderService.Delete</c> issues a <c>DELETE</c> request that includes the
    /// purchase order id in the URL path and accepts the spec's empty <c>204</c> response.
    /// </summary>
    [Test]
    public async Task PurchaseOrderService_Delete_SendsDeleteRequestWithIdInPath()
    {
        var expectedPath = $"{PurchaseOrdersPath}/{TestPurchaseOrderId}";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingDelete())
            .RespondWith(Response.Create().WithStatusCode(204));

        var service = new PurchaseOrderService(ConnectionHandler);

        var result = await service.Delete(TestPurchaseOrderId, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("DELETE"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
        });
    }
}
