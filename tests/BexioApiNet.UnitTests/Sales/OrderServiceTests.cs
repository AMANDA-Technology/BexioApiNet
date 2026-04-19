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

using BexioApiNet.Abstractions.Enums.Api;
using BexioApiNet.Abstractions.Models.Api;
using BexioApiNet.Abstractions.Models.Sales.Deliveries;
using BexioApiNet.Abstractions.Models.Sales.Invoices;
using BexioApiNet.Abstractions.Models.Sales.Orders;
using BexioApiNet.Abstractions.Models.Sales.Orders.Views;
using BexioApiNet.Models;
using BexioApiNet.Services.Connectors.Sales;

namespace BexioApiNet.UnitTests.Sales;

/// <summary>
///     Offline unit tests for <see cref="OrderService" />. Each test verifies that the service
///     forwards its calls to <see cref="IBexioConnectionHandler" /> with the expected arguments and
///     returns the handler's result unchanged. No network, no filesystem access.
/// </summary>
[TestFixture]
public sealed class OrderServiceTests : ServiceTestBase
{
    /// <summary>
    ///     Creates a fresh <see cref="OrderService" /> per test, bound to the
    ///     <see cref="ServiceTestBase.ConnectionHandler" /> substitute provided by the base fixture.
    /// </summary>
    [SetUp]
    public void CreateSut()
    {
        _sut = new OrderService(ConnectionHandler);
    }

    private const string ExpectedEndpoint = "2.0/kb_order";

    private OrderService _sut = null!;

    /// <summary>
    ///     Get (no parameters) calls <see cref="IBexioConnectionHandler.GetAsync{TResult}" /> once with
    ///     the expected endpoint path and a null query parameter.
    /// </summary>
    [Test]
    public async Task Get_WithNoParams_CallsGetAsync()
    {
        var response = new ApiResult<List<Order>?> { IsSuccess = true, Data = [] };
        ConnectionHandler
            .GetAsync<List<Order>?>(
                Arg.Any<string>(),
                Arg.Any<QueryParameter?>(),
                Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.Get();

        await ConnectionHandler.Received(1).GetAsync<List<Order>?>(
            ExpectedEndpoint,
            null,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    ///     Get forwards the <see cref="QueryParameterOrder" />'s underlying <see cref="QueryParameter" />
    ///     to the connection handler so the caller's filters (limit/offset/order_by) reach the API.
    /// </summary>
    [Test]
    public async Task Get_WithQueryParameter_PassesQueryParameterToConnectionHandler()
    {
        var queryParameter = new QueryParameterOrder(100, 50);
        var response = new ApiResult<List<Order>?> { IsSuccess = true, Data = [] };
        ConnectionHandler
            .GetAsync<List<Order>?>(
                Arg.Any<string>(),
                Arg.Any<QueryParameter?>(),
                Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.Get(queryParameter);

        await ConnectionHandler.Received(1).GetAsync<List<Order>?>(
            ExpectedEndpoint,
            queryParameter.QueryParameter,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    ///     Get (autoPage = true) triggers <see cref="IBexioConnectionHandler.FetchAll{TResult}" /> when
    ///     the <c>X-Total-Count</c> header is present and the initial response only returned a page of
    ///     the full result set.
    /// </summary>
    [Test]
    public async Task Get_WithAutoPage_CallsFetchAll()
    {
        const int totalResults = 10;
        var initialData = new List<Order> { BuildOrder(1), BuildOrder(2) };
        var initial = new ApiResult<List<Order>?>
        {
            IsSuccess = true,
            Data = initialData,
            ResponseHeaders = new Dictionary<string, int?>
            {
                { ApiHeaderNames.TotalResults, totalResults }
            }
        };
        ConnectionHandler
            .GetAsync<List<Order>?>(
                Arg.Any<string>(),
                Arg.Any<QueryParameter?>(),
                Arg.Any<CancellationToken>())
            .Returns(initial);
        ConnectionHandler
            .FetchAll<Order>(
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Any<string>(),
                Arg.Any<QueryParameter?>(),
                Arg.Any<CancellationToken>())
            .Returns([]);

        await _sut.Get(autoPage: true);

        await ConnectionHandler.Received(1).FetchAll<Order>(
            initialData.Count,
            totalResults,
            ExpectedEndpoint,
            null,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    ///     Get returns the <see cref="ApiResult{T}" /> produced by the connection handler when
    ///     auto-paging is not requested.
    /// </summary>
    [Test]
    public async Task Get_ReturnsApiResult()
    {
        var response = new ApiResult<List<Order>?> { IsSuccess = true, Data = [] };
        ConnectionHandler
            .GetAsync<List<Order>?>(
                Arg.Any<string>(),
                Arg.Any<QueryParameter?>(),
                Arg.Any<CancellationToken>())
            .Returns(response);

        var result = await _sut.Get();

        Assert.That(result, Is.SameAs(response));
    }

    /// <summary>
    ///     GetById calls <see cref="IBexioConnectionHandler.GetAsync{TResult}" /> with the expected
    ///     endpoint path including the order id.
    /// </summary>
    [Test]
    public async Task GetById_CallsGetAsync_WithIdInPath()
    {
        const int id = 42;
        var response = new ApiResult<Order> { IsSuccess = true };
        string? capturedPath = null;
        ConnectionHandler
            .GetAsync<Order>(
                Arg.Do<string>(path => capturedPath = path),
                Arg.Any<QueryParameter?>(),
                Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.GetById(id);

        Assert.That(capturedPath, Is.EqualTo($"{ExpectedEndpoint}/{id}"));
    }

    /// <summary>
    ///     GetPdf calls <see cref="IBexioConnectionHandler.GetBinaryAsync" /> against the
    ///     <c>/{id}/pdf</c> sub-resource.
    /// </summary>
    [Test]
    public async Task GetPdf_CallsGetBinaryAsync_WithIdInPath()
    {
        const int id = 42;
        var response = new ApiResult<byte[]> { IsSuccess = true, Data = [1, 2, 3] };
        string? capturedPath = null;
        ConnectionHandler
            .GetBinaryAsync(
                Arg.Do<string>(path => capturedPath = path),
                Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.GetPdf(id);

        Assert.That(capturedPath, Is.EqualTo($"{ExpectedEndpoint}/{id}/pdf"));
    }

    /// <summary>
    ///     GetRepetition calls <see cref="IBexioConnectionHandler.GetAsync{TResult}" /> with the
    ///     expected endpoint path including the order id and the <c>/repetition</c> sub-resource.
    /// </summary>
    [Test]
    public async Task GetRepetition_CallsGetAsync_WithRepetitionPath()
    {
        const int id = 42;
        var response = new ApiResult<OrderRepetition> { IsSuccess = true };
        string? capturedPath = null;
        ConnectionHandler
            .GetAsync<OrderRepetition>(
                Arg.Do<string>(path => capturedPath = path),
                Arg.Any<QueryParameter?>(),
                Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.GetRepetition(id);

        Assert.That(capturedPath, Is.EqualTo($"{ExpectedEndpoint}/{id}/repetition"));
    }

    /// <summary>
    ///     Create forwards the payload and the expected endpoint path to
    ///     <see cref="IBexioConnectionHandler.PostAsync{TResult,TCreate}" />.
    /// </summary>
    [Test]
    public async Task Create_CallsPostAsync()
    {
        var payload = BuildCreatePayload();
        var response = new ApiResult<Order> { IsSuccess = true };
        ConnectionHandler
            .PostAsync<Order, OrderCreate>(
                Arg.Any<OrderCreate>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.Create(payload);

        await ConnectionHandler.Received(1).PostAsync<Order, OrderCreate>(
            payload,
            ExpectedEndpoint,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    ///     Update calls <see cref="IBexioConnectionHandler.PostAsync{TResult,TUpdate}" /> (not PUT) at
    ///     <c>/2.0/kb_order/{id}</c> — Bexio edits orders via POST on this resource.
    /// </summary>
    [Test]
    public async Task Update_CallsPostAsync_WithIdInPath()
    {
        const int id = 42;
        var payload = BuildUpdatePayload();
        var response = new ApiResult<Order> { IsSuccess = true };
        string? capturedPath = null;
        ConnectionHandler
            .PostAsync<Order, OrderUpdate>(
                Arg.Any<OrderUpdate>(),
                Arg.Do<string>(path => capturedPath = path),
                Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.Update(id, payload);

        Assert.That(capturedPath, Is.EqualTo($"{ExpectedEndpoint}/{id}"));
    }

    /// <summary>
    ///     Delete forwards the call to <see cref="IBexioConnectionHandler.Delete" /> with the order id
    ///     appended to the endpoint root.
    /// </summary>
    [Test]
    public async Task Delete_CallsConnectionHandlerDelete_WithIdInPath()
    {
        const int id = 42;
        var response = new ApiResult<object> { IsSuccess = true };
        string? capturedPath = null;
        ConnectionHandler
            .Delete(
                Arg.Do<string>(path => capturedPath = path),
                Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.Delete(id);

        Assert.That(capturedPath, Is.EqualTo($"{ExpectedEndpoint}/{id}"));
    }

    /// <summary>
    ///     Search forwards the criteria, the <c>/search</c> path and the optional query parameter to
    ///     <see cref="IBexioConnectionHandler.PostSearchAsync{TResult}" />.
    /// </summary>
    [Test]
    public async Task Search_CallsPostSearchAsync()
    {
        var criteria = new List<SearchCriteria>
        {
            new() { Field = "title", Value = "Acme", Criteria = "like" }
        };
        var queryParameter = new QueryParameterOrder(50);
        var response = new ApiResult<List<Order>> { IsSuccess = true, Data = [] };
        ConnectionHandler
            .PostSearchAsync<Order>(
                Arg.Any<List<SearchCriteria>>(),
                Arg.Any<string>(),
                Arg.Any<QueryParameter?>(),
                Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.Search(criteria, queryParameter);

        await ConnectionHandler.Received(1).PostSearchAsync<Order>(
            criteria,
            $"{ExpectedEndpoint}/search",
            queryParameter.QueryParameter,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    ///     CreateDeliveryFromOrder posts the optional <see cref="OrderConvertRequest" /> body to the
    ///     <c>/{id}/delivery</c> endpoint and returns the newly created <see cref="Delivery" />.
    /// </summary>
    [Test]
    public async Task CreateDeliveryFromOrder_CallsPostAsync_WithDeliveryPath()
    {
        const int id = 42;
        var payload = new OrderConvertRequest();
        var response = new ApiResult<Delivery> { IsSuccess = true };
        string? capturedPath = null;
        ConnectionHandler
            .PostAsync<Delivery, OrderConvertRequest>(
                Arg.Any<OrderConvertRequest>(),
                Arg.Do<string>(path => capturedPath = path),
                Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.CreateDeliveryFromOrder(id, payload);

        Assert.That(capturedPath, Is.EqualTo($"{ExpectedEndpoint}/{id}/delivery"));
    }

    /// <summary>
    ///     CreateInvoiceFromOrder posts the optional <see cref="OrderConvertRequest" /> body to the
    ///     <c>/{id}/invoice</c> endpoint and returns the newly created <see cref="Invoice" />.
    /// </summary>
    [Test]
    public async Task CreateInvoiceFromOrder_CallsPostAsync_WithInvoicePath()
    {
        const int id = 42;
        var payload = new OrderConvertRequest();
        var response = new ApiResult<Invoice> { IsSuccess = true };
        string? capturedPath = null;
        ConnectionHandler
            .PostAsync<Invoice, OrderConvertRequest>(
                Arg.Any<OrderConvertRequest>(),
                Arg.Do<string>(path => capturedPath = path),
                Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.CreateInvoiceFromOrder(id, payload);

        Assert.That(capturedPath, Is.EqualTo($"{ExpectedEndpoint}/{id}/invoice"));
    }

    /// <summary>
    ///     CreateRepetition posts the <see cref="OrderRepetitionCreate" /> body to the
    ///     <c>/{id}/repetition</c> endpoint and returns the resulting <see cref="OrderRepetition" />.
    /// </summary>
    [Test]
    public async Task CreateRepetition_CallsPostAsync_WithRepetitionPath()
    {
        const int id = 42;
        var payload = BuildRepetitionCreatePayload();
        var response = new ApiResult<OrderRepetition> { IsSuccess = true };
        string? capturedPath = null;
        ConnectionHandler
            .PostAsync<OrderRepetition, OrderRepetitionCreate>(
                Arg.Any<OrderRepetitionCreate>(),
                Arg.Do<string>(path => capturedPath = path),
                Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.CreateRepetition(id, payload);

        Assert.That(capturedPath, Is.EqualTo($"{ExpectedEndpoint}/{id}/repetition"));
        await ConnectionHandler.Received(1).PostAsync<OrderRepetition, OrderRepetitionCreate>(
            payload,
            $"{ExpectedEndpoint}/{id}/repetition",
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    ///     DeleteRepetition calls <see cref="IBexioConnectionHandler.Delete" /> against the
    ///     <c>/{id}/repetition</c> sub-resource.
    /// </summary>
    [Test]
    public async Task DeleteRepetition_CallsConnectionHandlerDelete_WithRepetitionPath()
    {
        const int id = 42;
        var response = new ApiResult<object> { IsSuccess = true };
        string? capturedPath = null;
        ConnectionHandler
            .Delete(
                Arg.Do<string>(path => capturedPath = path),
                Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.DeleteRepetition(id);

        Assert.That(capturedPath, Is.EqualTo($"{ExpectedEndpoint}/{id}/repetition"));
    }

    private static OrderCreate BuildCreatePayload()
    {
        return new OrderCreate(1, Title: "Order");
    }

    private static OrderUpdate BuildUpdatePayload()
    {
        return new OrderUpdate(1, Title: "Order");
    }

    private static OrderRepetitionCreate BuildRepetitionCreatePayload()
    {
        return new OrderRepetitionCreate(
            "2026-01-01",
            new OrderRepetitionDaily { Interval = 1 },
            "2026-12-31");
    }

    private static Order BuildOrder(int id)
    {
        return new Order(
            id,
            $"AU-{id:D5}",
            $"Order {id}",
            null,
            null,
            1,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null);
    }
}