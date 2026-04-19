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
using BexioApiNet.Models;
using BexioApiNet.Services.Connectors.Sales;

namespace BexioApiNet.UnitTests.Sales;

/// <summary>
/// Offline unit tests for <see cref="DeliveryService" />. Each test verifies that the service
/// forwards its calls to <see cref="IBexioConnectionHandler" /> with the expected arguments and
/// returns the handler's result unchanged. No network, no filesystem access.
/// </summary>
[TestFixture]
public sealed class DeliveryServiceTests : ServiceTestBase
{
    /// <summary>
    /// Creates a fresh <see cref="DeliveryService" /> per test, bound to the
    /// <see cref="ServiceTestBase.ConnectionHandler" /> substitute provided by the base fixture.
    /// </summary>
    [SetUp]
    public void CreateSut()
    {
        _sut = new DeliveryService(ConnectionHandler);
    }

    private const string ExpectedEndpoint = "2.0/kb_delivery";

    private DeliveryService _sut = null!;

    /// <summary>
    /// Get (no parameters) calls <see cref="IBexioConnectionHandler.GetAsync{TResult}" /> once with
    /// the expected endpoint path and a null query parameter.
    /// </summary>
    [Test]
    public async Task Get_WithNoParams_CallsGetAsync()
    {
        var response = new ApiResult<List<Delivery>?> { IsSuccess = true, Data = [] };
        ConnectionHandler
            .GetAsync<List<Delivery>?>(
                Arg.Any<string>(),
                Arg.Any<QueryParameter?>(),
                Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.Get();

        await ConnectionHandler.Received(1).GetAsync<List<Delivery>?>(
            ExpectedEndpoint,
            null,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Get forwards the <see cref="QueryParameterDelivery" />'s underlying <see cref="QueryParameter" />
    /// to the connection handler so the caller's filters (limit/offset/order_by) reach the API.
    /// </summary>
    [Test]
    public async Task Get_WithQueryParameter_PassesQueryParameterToConnectionHandler()
    {
        var queryParameter = new QueryParameterDelivery(100, 50);
        var response = new ApiResult<List<Delivery>?> { IsSuccess = true, Data = [] };
        ConnectionHandler
            .GetAsync<List<Delivery>?>(
                Arg.Any<string>(),
                Arg.Any<QueryParameter?>(),
                Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.Get(queryParameter);

        await ConnectionHandler.Received(1).GetAsync<List<Delivery>?>(
            ExpectedEndpoint,
            queryParameter.QueryParameter,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Get (autoPage = true) triggers <see cref="IBexioConnectionHandler.FetchAll{TResult}" /> when
    /// the <c>X-Total-Count</c> header is present and the initial response only returned a page of
    /// the full result set.
    /// </summary>
    [Test]
    public async Task Get_WithAutoPage_CallsFetchAll()
    {
        const int totalResults = 10;
        var initialData = new List<Delivery> { BuildDelivery(1), BuildDelivery(2) };
        var initial = new ApiResult<List<Delivery>?>
        {
            IsSuccess = true,
            Data = initialData,
            ResponseHeaders = new Dictionary<string, int?>
            {
                { ApiHeaderNames.TotalResults, totalResults }
            }
        };
        ConnectionHandler
            .GetAsync<List<Delivery>?>(
                Arg.Any<string>(),
                Arg.Any<QueryParameter?>(),
                Arg.Any<CancellationToken>())
            .Returns(initial);
        ConnectionHandler
            .FetchAll<Delivery>(
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Any<string>(),
                Arg.Any<QueryParameter?>(),
                Arg.Any<CancellationToken>())
            .Returns([]);

        await _sut.Get(autoPage: true);

        await ConnectionHandler.Received(1).FetchAll<Delivery>(
            initialData.Count,
            totalResults,
            ExpectedEndpoint,
            null,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Get returns the <see cref="ApiResult{T}" /> produced by the connection handler when
    /// auto-paging is not requested.
    /// </summary>
    [Test]
    public async Task Get_ReturnsApiResult()
    {
        var response = new ApiResult<List<Delivery>?> { IsSuccess = true, Data = [] };
        ConnectionHandler
            .GetAsync<List<Delivery>?>(
                Arg.Any<string>(),
                Arg.Any<QueryParameter?>(),
                Arg.Any<CancellationToken>())
            .Returns(response);

        var result = await _sut.Get();

        Assert.That(result, Is.SameAs(response));
    }

    /// <summary>
    /// GetById calls <see cref="IBexioConnectionHandler.GetAsync{TResult}" /> with the expected
    /// endpoint path including the delivery id.
    /// </summary>
    [Test]
    public async Task GetById_CallsGetAsync_WithIdInPath()
    {
        const int id = 42;
        var response = new ApiResult<Delivery> { IsSuccess = true };
        string? capturedPath = null;
        ConnectionHandler
            .GetAsync<Delivery>(
                Arg.Do<string>(path => capturedPath = path),
                Arg.Any<QueryParameter?>(),
                Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.GetById(id);

        Assert.That(capturedPath, Is.EqualTo($"{ExpectedEndpoint}/{id}"));
    }

    /// <summary>
    /// Issue posts to the <c>/{id}/issue</c> action endpoint with no request body.
    /// </summary>
    [Test]
    public async Task Issue_CallsPostActionAsync_WithIssuePath()
    {
        const int id = 42;
        var response = new ApiResult<object> { IsSuccess = true };
        string? capturedPath = null;
        ConnectionHandler
            .PostActionAsync(
                Arg.Do<string>(path => capturedPath = path),
                Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.Issue(id);

        Assert.That(capturedPath, Is.EqualTo($"{ExpectedEndpoint}/{id}/issue"));
    }

    private static Delivery BuildDelivery(int id)
    {
        return new Delivery(
            Id: id,
            DocumentNr: $"LS-{id:D5}",
            Title: $"Delivery {id}",
            ContactId: null,
            ContactSubId: null,
            UserId: 1,
            LogopaperId: null,
            LanguageId: null,
            BankAccountId: null,
            CurrencyId: null,
            Header: null,
            Footer: null,
            TotalGross: null,
            TotalNet: null,
            TotalTaxes: null,
            Total: null,
            TotalRoundingDifference: null,
            MwstType: null,
            MwstIsNet: null,
            IsValidFrom: null,
            ContactAddress: null,
            DeliveryAddressType: null,
            DeliveryAddress: null,
            KbItemStatusId: null,
            ApiReference: null,
            ViewedByClientAt: null,
            UpdatedAt: null,
            Taxs: null,
            Positions: null);
    }
}
