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
using BexioApiNet.Abstractions.Models.Purchases.PurchaseOrders;
using BexioApiNet.Abstractions.Models.Purchases.PurchaseOrders.Views;
using BexioApiNet.Models;
using BexioApiNet.Services.Connectors.Purchases;

namespace BexioApiNet.UnitTests.Purchases;

/// <summary>
/// Offline unit tests for <see cref="PurchaseOrderService"/>. Each test asserts that the service
/// forwards its calls to <see cref="IBexioConnectionHandler"/> with the expected verb, path and
/// payload, and returns the handler's <see cref="ApiResult{T}"/> unchanged. Note in particular
/// that <c>Update</c> uses <c>POST</c> (not <c>PUT</c>) — Bexio v3.0 follows the same
/// POST-to-id update convention as v2.0.
/// </summary>
[TestFixture]
public sealed class PurchaseOrderServiceTests : ServiceTestBase
{
    private const string ExpectedEndpoint = "3.0/purchase/orders";

    private PurchaseOrderService _sut = null!;

    /// <summary>
    /// Creates a fresh <see cref="PurchaseOrderService"/> bound to the
    /// <see cref="ServiceTestBase.ConnectionHandler"/> substitute before each test.
    /// </summary>
    [SetUp]
    public void CreateSut()
    {
        _sut = new PurchaseOrderService(ConnectionHandler);
    }

    /// <summary>
    /// Get with no query parameter forwards a <see langword="null"/> <see cref="QueryParameter"/>
    /// to <see cref="IBexioConnectionHandler.GetAsync{TResult}"/> at the purchase orders collection path.
    /// </summary>
    [Test]
    public async Task Get_WithNoParams_CallsGetAsync()
    {
        var response = new ApiResult<List<PurchaseOrder>?> { IsSuccess = true, Data = [] };
        ConnectionHandler
            .GetAsync<List<PurchaseOrder>?>(Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.Get();

        await ConnectionHandler.Received(1).GetAsync<List<PurchaseOrder>?>(
            ExpectedEndpoint,
            null,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Get forwards the supplied <see cref="QueryParameter"/> to the connection handler so the
    /// caller's filters (limit/offset/order_by) reach the API.
    /// </summary>
    [Test]
    public async Task Get_WithQueryParameter_ForwardsQueryParameter()
    {
        var queryParameter = new QueryParameter(new Dictionary<string, object> { ["limit"] = 50 });
        ConnectionHandler
            .GetAsync<List<PurchaseOrder>?>(Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(new ApiResult<List<PurchaseOrder>?> { IsSuccess = true, Data = [] });

        await _sut.Get(queryParameter);

        await ConnectionHandler.Received(1).GetAsync<List<PurchaseOrder>?>(
            ExpectedEndpoint,
            queryParameter,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Get returns the <see cref="ApiResult{T}"/> from the connection handler unchanged.
    /// </summary>
    [Test]
    public async Task Get_ReturnsApiResultFromConnectionHandler()
    {
        var response = new ApiResult<List<PurchaseOrder>?> { IsSuccess = true, Data = [] };
        ConnectionHandler
            .GetAsync<List<PurchaseOrder>?>(Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(response);

        var result = await _sut.Get();

        Assert.That(result, Is.SameAs(response));
    }

    /// <summary>
    /// GetById calls <see cref="IBexioConnectionHandler.GetAsync{TResult}"/> with the
    /// purchase order id appended to the endpoint root and a <see langword="null"/> query parameter.
    /// </summary>
    [Test]
    public async Task GetById_CallsGetAsync_WithIdInPath()
    {
        const int id = 42;
        string? capturedPath = null;
        ConnectionHandler
            .GetAsync<PurchaseOrder>(
                Arg.Do<string>(path => capturedPath = path),
                Arg.Any<QueryParameter?>(),
                Arg.Any<CancellationToken>())
            .Returns(new ApiResult<PurchaseOrder> { IsSuccess = true });

        await _sut.GetById(id);

        Assert.That(capturedPath, Is.EqualTo($"{ExpectedEndpoint}/{id}"));
        await ConnectionHandler.Received(1).GetAsync<PurchaseOrder>(
            $"{ExpectedEndpoint}/{id}",
            null,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Create forwards the <see cref="PurchaseOrderCreate"/> payload and the endpoint path
    /// to <see cref="IBexioConnectionHandler.PostAsync{TResult,TCreate}"/>.
    /// </summary>
    [Test]
    public async Task Create_CallsPostAsync()
    {
        var payload = BuildCreatePayload();
        ConnectionHandler
            .PostAsync<PurchaseOrder, PurchaseOrderCreate>(Arg.Any<PurchaseOrderCreate>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new ApiResult<PurchaseOrder> { IsSuccess = true });

        await _sut.Create(payload);

        await ConnectionHandler.Received(1).PostAsync<PurchaseOrder, PurchaseOrderCreate>(
            payload,
            ExpectedEndpoint,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Update calls <see cref="IBexioConnectionHandler.PostAsync{TResult,TUpdate}"/> (POST, not PUT)
    /// at <c>/3.0/purchase/orders/{id}</c> — Bexio v3.0 routes update via POST on the resource,
    /// matching the v2.0 sales-document convention.
    /// </summary>
    [Test]
    public async Task Update_CallsPostAsync_WithIdInPath()
    {
        const int id = 42;
        var payload = BuildUpdatePayload();
        string? capturedPath = null;
        ConnectionHandler
            .PostAsync<PurchaseOrder, PurchaseOrderUpdate>(
                Arg.Any<PurchaseOrderUpdate>(),
                Arg.Do<string>(path => capturedPath = path),
                Arg.Any<CancellationToken>())
            .Returns(new ApiResult<PurchaseOrder> { IsSuccess = true });

        await _sut.Update(id, payload);

        Assert.That(capturedPath, Is.EqualTo($"{ExpectedEndpoint}/{id}"));
        await ConnectionHandler.Received(1).PostAsync<PurchaseOrder, PurchaseOrderUpdate>(
            payload,
            $"{ExpectedEndpoint}/{id}",
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Delete calls <see cref="IBexioConnectionHandler.Delete"/> with the purchase order id
    /// appended to the endpoint root.
    /// </summary>
    [Test]
    public async Task Delete_CallsConnectionHandlerDelete_WithIdInPath()
    {
        const int id = 42;
        string? capturedPath = null;
        ConnectionHandler
            .Delete(Arg.Do<string>(path => capturedPath = path), Arg.Any<CancellationToken>())
            .Returns(new ApiResult<object> { IsSuccess = true });

        await _sut.Delete(id);

        Assert.That(capturedPath, Is.EqualTo($"{ExpectedEndpoint}/{id}"));
        await ConnectionHandler.Received(1).Delete(
            $"{ExpectedEndpoint}/{id}",
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Create returns the <see cref="ApiResult{T}"/> from the connection handler unchanged.
    /// </summary>
    [Test]
    public async Task Create_ReturnsApiResultFromConnectionHandler()
    {
        var payload = BuildCreatePayload();
        var response = new ApiResult<PurchaseOrder> { IsSuccess = true };
        ConnectionHandler
            .PostAsync<PurchaseOrder, PurchaseOrderCreate>(Arg.Any<PurchaseOrderCreate>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(response);

        var result = await _sut.Create(payload);

        Assert.That(result, Is.SameAs(response));
    }

    /// <summary>
    /// Delete returns the <see cref="ApiResult{T}"/> from the connection handler unchanged.
    /// </summary>
    [Test]
    public async Task Delete_ReturnsApiResultFromConnectionHandler()
    {
        var response = new ApiResult<object> { IsSuccess = true };
        ConnectionHandler
            .Delete(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(response);

        var result = await _sut.Delete(42);

        Assert.That(result, Is.SameAs(response));
    }

    private static PurchaseOrderCreate BuildCreatePayload() =>
        new(
            ContactId: 1323,
            CurrencyId: 1,
            UserId: 1,
            Title: "Order");

    private static PurchaseOrderUpdate BuildUpdatePayload() =>
        new(
            ContactId: 1323,
            CurrencyId: 1,
            UserId: 1,
            Title: "Order");
}
