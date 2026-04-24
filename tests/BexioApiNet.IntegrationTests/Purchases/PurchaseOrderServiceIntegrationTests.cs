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
using BexioApiNet.Services.Connectors.Purchases;

namespace BexioApiNet.IntegrationTests.Purchases;

/// <summary>
/// Integration tests for <see cref="PurchaseOrderService"/> against WireMock stubs. Verifies the
/// path composed from <see cref="PurchaseOrderConfiguration"/> (<c>3.0/purchase/orders</c>)
/// reaches the handler correctly and that the expected HTTP verbs are used for each operation —
/// including <c>POST</c> (not <c>PUT</c>) for Update, which matches the v2.0 sales-document
/// convention rather than the v4.0 Bills convention.
/// </summary>
public sealed class PurchaseOrderServiceIntegrationTests : IntegrationTestBase
{
    private const string PurchaseOrdersPath = "/3.0/purchase/orders";

    private const int TestPurchaseOrderId = 42;

    private const string PurchaseOrderResponse = """
                                                 {
                                                     "id": 42,
                                                     "document_no": "PO-1001",
                                                     "title": "Office supplies",
                                                     "contact_id": 1323,
                                                     "currency_id": 1,
                                                     "user_id": 1,
                                                     "total_net": 540.00,
                                                     "total_gross": 581.40,
                                                     "is_valid_from": "2026-04-01",
                                                     "is_valid_until": "2026-05-01"
                                                 }
                                                 """;

    private const string PurchaseOrderListBody = "[]";

    /// <summary>
    /// <c>PurchaseOrderService.Get</c> issues a <c>GET</c> against <c>/3.0/purchase/orders</c>
    /// and deserializes the list response on success.
    /// </summary>
    [Test]
    public async Task PurchaseOrderService_Get_SendsGetRequest()
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
            Assert.That(result.Data!, Is.Empty);
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(PurchaseOrdersPath));
        });
    }

    /// <summary>
    /// <c>PurchaseOrderService.GetById</c> issues a <c>GET</c> request with the purchase order id
    /// in the URL path and surfaces the returned purchase order on success.
    /// </summary>
    [Test]
    public async Task PurchaseOrderService_GetById_SendsGetRequestWithIdInPath()
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
            Assert.That(result.Data!.Id, Is.EqualTo(TestPurchaseOrderId));
            Assert.That(result.Data.DocumentNo, Is.EqualTo("PO-1001"));
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
        });
    }

    /// <summary>
    /// <c>PurchaseOrderService.Create</c> sends a <c>POST</c> request whose body contains
    /// the serialized <see cref="PurchaseOrderCreate"/> payload.
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
            Title: "Office supplies");

        var result = await service.Create(payload, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(request.Method, Is.EqualTo("POST"));
            Assert.That(request.AbsolutePath, Is.EqualTo(PurchaseOrdersPath));
            Assert.That(request.Body, Does.Contain("\"contact_id\":1323"));
            Assert.That(request.Body, Does.Contain("\"currency_id\":1"));
            Assert.That(request.Body, Does.Contain("\"title\":\"Office supplies\""));
        });
    }

    /// <summary>
    /// <c>PurchaseOrderService.Update</c> sends a <c>POST</c> request against
    /// <c>/3.0/purchase/orders/{id}</c> — Bexio v3.0 uses <c>POST</c> for updates,
    /// not <c>PUT</c>.
    /// </summary>
    [Test]
    public async Task PurchaseOrderService_Update_SendsPostRequestWithIdInPath()
    {
        var expectedPath = $"{PurchaseOrdersPath}/{TestPurchaseOrderId}";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(PurchaseOrderResponse));

        var service = new PurchaseOrderService(ConnectionHandler);

        var payload = new PurchaseOrderUpdate(
            ContactId: 1323,
            CurrencyId: 1,
            UserId: 1,
            Title: "Office supplies (updated)");

        var result = await service.Update(TestPurchaseOrderId, payload, TestContext.CurrentContext.CancellationToken);

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
    /// <c>PurchaseOrderService.Delete</c> issues a <c>DELETE</c> request that includes the
    /// purchase order id in the URL path.
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
