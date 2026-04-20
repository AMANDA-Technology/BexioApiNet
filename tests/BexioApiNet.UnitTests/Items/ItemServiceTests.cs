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
using BexioApiNet.Abstractions.Models.Items.Items;
using BexioApiNet.Abstractions.Models.Items.Items.Views;
using BexioApiNet.Models;
using BexioApiNet.Services.Connectors.Items;

namespace BexioApiNet.UnitTests.Items;

/// <summary>
/// Offline unit tests for <see cref="ItemService"/>. Each test verifies that the service
/// forwards its calls to <see cref="IBexioConnectionHandler"/> with the expected arguments and
/// returns the handler's result unchanged. No network, no filesystem access.
/// </summary>
[TestFixture]
public sealed class ItemServiceTests : ServiceTestBase
{
    /// <summary>
    /// Creates a fresh <see cref="ItemService"/> per test, bound to the
    /// <see cref="ServiceTestBase.ConnectionHandler"/> substitute provided by the base fixture.
    /// </summary>
    [SetUp]
    public void CreateSut()
    {
        _sut = new ItemService(ConnectionHandler);
    }

    private const string ExpectedEndpoint = "2.0/article";

    private ItemService _sut = null!;

    /// <summary>
    /// Get (no parameters) calls <see cref="IBexioConnectionHandler.GetAsync{TResult}"/> once with
    /// the expected endpoint path and a null query parameter.
    /// </summary>
    [Test]
    public async Task Get_WithNoParams_CallsGetAsync()
    {
        var response = new ApiResult<List<Item>?> { IsSuccess = true, Data = [] };
        ConnectionHandler
            .GetAsync<List<Item>?>(
                Arg.Any<string>(),
                Arg.Any<QueryParameter?>(),
                Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.Get();

        await ConnectionHandler.Received(1).GetAsync<List<Item>?>(
            ExpectedEndpoint,
            null,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Get forwards the <see cref="QueryParameterItem"/>'s underlying <see cref="QueryParameter"/>
    /// to the connection handler so the caller's filters (limit/offset/order_by) reach the API.
    /// </summary>
    [Test]
    public async Task Get_WithQueryParameter_PassesQueryParameterToConnectionHandler()
    {
        var queryParameter = new QueryParameterItem(Limit: 100, Offset: 50);
        var response = new ApiResult<List<Item>?> { IsSuccess = true, Data = [] };
        ConnectionHandler
            .GetAsync<List<Item>?>(
                Arg.Any<string>(),
                Arg.Any<QueryParameter?>(),
                Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.Get(queryParameter);

        await ConnectionHandler.Received(1).GetAsync<List<Item>?>(
            ExpectedEndpoint,
            queryParameter.QueryParameter,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Get (autoPage = true) triggers <see cref="IBexioConnectionHandler.FetchAll{TResult}"/> when
    /// the <c>X-Total-Count</c> header is present and the initial response only returned a page of
    /// the full result set.
    /// </summary>
    [Test]
    public async Task Get_WithAutoPage_CallsFetchAll()
    {
        const int totalResults = 10;
        var initialData = new List<Item> { BuildItem(1), BuildItem(2) };
        var initial = new ApiResult<List<Item>?>
        {
            IsSuccess = true,
            Data = initialData,
            ResponseHeaders = new Dictionary<string, int?>
            {
                { ApiHeaderNames.TotalResults, totalResults }
            }
        };
        ConnectionHandler
            .GetAsync<List<Item>?>(
                Arg.Any<string>(),
                Arg.Any<QueryParameter?>(),
                Arg.Any<CancellationToken>())
            .Returns(initial);
        ConnectionHandler
            .FetchAll<Item>(
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Any<string>(),
                Arg.Any<QueryParameter?>(),
                Arg.Any<CancellationToken>())
            .Returns([]);

        await _sut.Get(autoPage: true);

        await ConnectionHandler.Received(1).FetchAll<Item>(
            initialData.Count,
            totalResults,
            ExpectedEndpoint,
            null,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Get returns the <see cref="ApiResult{T}"/> produced by the connection handler without modification.
    /// </summary>
    [Test]
    public async Task Get_ReturnsApiResult()
    {
        var response = new ApiResult<List<Item>?> { IsSuccess = true, Data = [] };
        ConnectionHandler
            .GetAsync<List<Item>?>(
                Arg.Any<string>(),
                Arg.Any<QueryParameter?>(),
                Arg.Any<CancellationToken>())
            .Returns(response);

        var result = await _sut.Get();

        Assert.That(result, Is.SameAs(response));
    }

    /// <summary>
    /// GetById calls <see cref="IBexioConnectionHandler.GetAsync{TResult}"/> with the expected
    /// endpoint path including the item id.
    /// </summary>
    [Test]
    public async Task GetById_CallsGetAsync_WithIdInPath()
    {
        const int id = 42;
        var response = new ApiResult<Item> { IsSuccess = true };
        string? capturedPath = null;
        ConnectionHandler
            .GetAsync<Item>(
                Arg.Do<string>(path => capturedPath = path),
                Arg.Any<QueryParameter?>(),
                Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.GetById(id);

        Assert.That(capturedPath, Is.EqualTo($"{ExpectedEndpoint}/{id}"));
    }

    /// <summary>
    /// GetById returns the <see cref="ApiResult{T}"/> produced by the connection handler without modification.
    /// </summary>
    [Test]
    public async Task GetById_ReturnsApiResultFromConnectionHandler()
    {
        var response = new ApiResult<Item> { IsSuccess = true };
        ConnectionHandler
            .GetAsync<Item>(
                Arg.Any<string>(),
                Arg.Any<QueryParameter?>(),
                Arg.Any<CancellationToken>())
            .Returns(response);

        var result = await _sut.GetById(1);

        Assert.That(result, Is.SameAs(response));
    }

    /// <summary>
    /// Create forwards the payload and the expected endpoint path to
    /// <see cref="IBexioConnectionHandler.PostAsync{TResult,TCreate}"/>.
    /// </summary>
    [Test]
    public async Task Create_CallsPostAsync()
    {
        var payload = BuildCreatePayload();
        var response = new ApiResult<Item> { IsSuccess = true };
        ConnectionHandler
            .PostAsync<Item, ItemCreate>(
                Arg.Any<ItemCreate>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.Create(payload);

        await ConnectionHandler.Received(1).PostAsync<Item, ItemCreate>(
            payload,
            ExpectedEndpoint,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Create returns the <see cref="ApiResult{T}"/> produced by the connection handler without modification.
    /// </summary>
    [Test]
    public async Task Create_ReturnsApiResultFromConnectionHandler()
    {
        var payload = BuildCreatePayload();
        var response = new ApiResult<Item> { IsSuccess = true };
        ConnectionHandler
            .PostAsync<Item, ItemCreate>(
                Arg.Any<ItemCreate>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>())
            .Returns(response);

        var result = await _sut.Create(payload);

        Assert.That(result, Is.SameAs(response));
    }

    /// <summary>
    /// Search forwards the criteria, the <c>/search</c> path and the optional query parameter to
    /// <see cref="IBexioConnectionHandler.PostSearchAsync{TResult}"/>.
    /// </summary>
    [Test]
    public async Task Search_CallsPostSearchAsync()
    {
        var criteria = new List<SearchCriteria>
        {
            new() { Field = "intern_name", Value = "Webhosting", Criteria = "like" }
        };
        var queryParameter = new QueryParameterItem(Limit: 50);
        var response = new ApiResult<List<Item>> { IsSuccess = true, Data = [] };
        ConnectionHandler
            .PostSearchAsync<Item>(
                Arg.Any<List<SearchCriteria>>(),
                Arg.Any<string>(),
                Arg.Any<QueryParameter?>(),
                Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.Search(criteria, queryParameter);

        await ConnectionHandler.Received(1).PostSearchAsync<Item>(
            criteria,
            $"{ExpectedEndpoint}/search",
            queryParameter.QueryParameter,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Update calls <see cref="IBexioConnectionHandler.PostAsync{TResult,TUpdate}"/> at
    /// <c>/2.0/article/{id}</c> — Bexio edits items via POST on this resource.
    /// </summary>
    [Test]
    public async Task Update_CallsPostAsync_WithIdInPath()
    {
        const int id = 42;
        var payload = BuildUpdatePayload();
        var response = new ApiResult<Item> { IsSuccess = true };
        string? capturedPath = null;
        ConnectionHandler
            .PostAsync<Item, ItemUpdate>(
                Arg.Any<ItemUpdate>(),
                Arg.Do<string>(path => capturedPath = path),
                Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.Update(id, payload);

        Assert.That(capturedPath, Is.EqualTo($"{ExpectedEndpoint}/{id}"));
        await ConnectionHandler.Received(1).PostAsync<Item, ItemUpdate>(
            payload,
            $"{ExpectedEndpoint}/{id}",
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Update returns the <see cref="ApiResult{T}"/> produced by the connection handler without modification.
    /// </summary>
    [Test]
    public async Task Update_ReturnsApiResultFromConnectionHandler()
    {
        var payload = BuildUpdatePayload();
        var response = new ApiResult<Item> { IsSuccess = true };
        ConnectionHandler
            .PostAsync<Item, ItemUpdate>(
                Arg.Any<ItemUpdate>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>())
            .Returns(response);

        var result = await _sut.Update(1, payload);

        Assert.That(result, Is.SameAs(response));
    }

    /// <summary>
    /// Delete forwards the call to <see cref="IBexioConnectionHandler.Delete"/> with the item id
    /// appended to the endpoint root.
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
    /// Delete returns the <see cref="ApiResult{T}"/> produced by the connection handler without modification.
    /// </summary>
    [Test]
    public async Task Delete_ReturnsApiResultFromConnectionHandler()
    {
        const int id = 42;
        var response = new ApiResult<object> { IsSuccess = true };
        ConnectionHandler
            .Delete(
                Arg.Any<string>(),
                Arg.Any<CancellationToken>())
            .Returns(response);

        var result = await _sut.Delete(id);

        Assert.That(result, Is.SameAs(response));
    }

    private static ItemCreate BuildCreatePayload()
    {
        return new ItemCreate(
            UserId: 1,
            ArticleTypeId: 1,
            InternCode: "wh-2019",
            InternName: "Webhosting");
    }

    private static ItemUpdate BuildUpdatePayload()
    {
        return new ItemUpdate(
            UserId: 1,
            InternCode: "wh-2019",
            InternName: "Webhosting Updated");
    }

    private static Item BuildItem(int id)
    {
        return new Item(
            Id: id,
            UserId: 1,
            ArticleTypeId: 2,
            ContactId: null,
            DelivererCode: null,
            DelivererName: null,
            DelivererDescription: null,
            InternCode: $"art-{id}",
            InternName: $"Article {id}",
            InternDescription: null,
            PurchasePrice: null,
            SalePrice: null,
            PurchaseTotal: null,
            SaleTotal: null,
            CurrencyId: null,
            TaxIncomeId: null,
            TaxId: null,
            TaxExpenseId: null,
            UnitId: null,
            IsStock: false,
            StockId: null,
            StockPlaceId: null,
            StockNr: 0,
            StockMinNr: 0,
            StockReservedNr: 0,
            StockAvailableNr: 0,
            StockPickedNr: 0,
            StockDisposedNr: 0,
            StockOrderedNr: 0,
            Width: null,
            Height: null,
            Weight: null,
            Volume: null,
            HtmlText: null,
            Remarks: null,
            DeliveryPrice: null,
            ArticleGroupId: null,
            AccountId: null,
            ExpenseAccountId: null);
    }
}
