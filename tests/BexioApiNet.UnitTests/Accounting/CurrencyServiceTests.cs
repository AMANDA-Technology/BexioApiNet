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

using System.Net;
using BexioApiNet.Abstractions.Enums.Api;
using BexioApiNet.Abstractions.Models.Accounting.Currencies;
using BexioApiNet.Abstractions.Models.Accounting.Currencies.Views;
using BexioApiNet.Abstractions.Models.Api;
using BexioApiNet.Models;
using BexioApiNet.Services.Connectors.Accounting;

namespace BexioApiNet.UnitTests.Accounting;

/// <summary>
/// Offline unit tests for <see cref="CurrencyService" />. Each test verifies that the service
/// forwards its calls to <see cref="IBexioConnectionHandler" /> with the expected arguments and
/// returns the handler's <see cref="ApiResult{T}" /> unchanged. No network, no filesystem access.
/// </summary>
[TestFixture]
public sealed class CurrencyServiceTests : ServiceTestBase
{
    /// <summary>
    /// Creates a fresh <see cref="CurrencyService" /> per test, bound to the
    /// <see cref="ServiceTestBase.ConnectionHandler" /> substitute provided by the base fixture.
    /// </summary>
    [SetUp]
    public void CreateSut()
    {
        _sut = new CurrencyService(ConnectionHandler);
    }

    private const string ExpectedPath = "3.0/currencies";

    private CurrencyService _sut = null!;

    /// <summary>
    /// With no parameters the service hits <c>3.0/currencies</c> exactly once
    /// with a <see langword="null" /> query parameter and returns the connection
    /// handler's <see cref="ApiResult{T}" /> as-is.
    /// </summary>
    [Test]
    public async Task Get_WithNoParams_CallsGetAsyncOnceWithExpectedPath()
    {
        var expected = new ApiResult<List<Currency>>
        {
            IsSuccess = true,
            Data = [new Currency(1, "CHF", 0.05)]
        };
        ConnectionHandler
            .GetAsync<List<Currency>>(Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

        var result = await _sut.Get();

        Assert.That(result, Is.SameAs(expected));
        await ConnectionHandler.Received(1).GetAsync<List<Currency>>(
            ExpectedPath,
            null,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// The cancellation token supplied by the caller must be forwarded to the
    /// connection handler so cooperative cancellation flows end-to-end.
    /// </summary>
    [Test]
    public async Task Get_ForwardsCancellationTokenToConnectionHandler()
    {
        using var cts = new CancellationTokenSource();
        ConnectionHandler
            .GetAsync<List<Currency>>(Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new ApiResult<List<Currency>> { IsSuccess = true, Data = [] }));

        await _sut.Get(cancellationToken: cts.Token);

        await ConnectionHandler.Received(1).GetAsync<List<Currency>>(
            ExpectedPath,
            null,
            cts.Token);
    }

    /// <summary>
    /// A failing <see cref="ApiResult{T}" /> from the connection handler must
    /// surface to the caller untouched — the service may not swallow errors.
    /// </summary>
    [Test]
    public async Task Get_ReturnsApiResultFromConnectionHandlerUnchanged()
    {
        var apiError = new ApiError(401, "unauthorized", new object());
        var response = new ApiResult<List<Currency>>
        {
            IsSuccess = false,
            ApiError = apiError,
            StatusCode = HttpStatusCode.Unauthorized,
            Data = null
        };
        ConnectionHandler
            .GetAsync<List<Currency>>(Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(response));

        var result = await _sut.Get();

        Assert.That(result, Is.SameAs(response));
    }

    /// <summary>
    /// <c>GetCodes</c> must hit <c>3.0/currencies/codes</c> exactly once
    /// with a <see langword="null" /> query parameter.
    /// </summary>
    [Test]
    public async Task GetCodes_CallsGetAsyncWithCodesPath()
    {
        var response = new ApiResult<List<string>>
        {
            IsSuccess = true,
            Data = ["EUR", "GBP", "PLN"]
        };
        ConnectionHandler
            .GetAsync<List<string>>(Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(response));

        var result = await _sut.GetCodes();

        Assert.That(result, Is.SameAs(response));
        await ConnectionHandler.Received(1).GetAsync<List<string>>(
            $"{ExpectedPath}/codes",
            null,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// <c>GetCodes</c> forwards the supplied cancellation token to the connection handler.
    /// </summary>
    [Test]
    public async Task GetCodes_ForwardsCancellationTokenToConnectionHandler()
    {
        using var cts = new CancellationTokenSource();
        ConnectionHandler
            .GetAsync<List<string>>(Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new ApiResult<List<string>> { IsSuccess = true, Data = [] }));

        await _sut.GetCodes(cts.Token);

        await ConnectionHandler.Received(1).GetAsync<List<string>>(
            $"{ExpectedPath}/codes",
            null,
            cts.Token);
    }

    /// <summary>
    /// <c>GetById</c> must build the request path with the currency id appended to the endpoint
    /// root and hit the handler exactly once.
    /// </summary>
    [Test]
    public async Task GetById_CallsGetAsyncWithIdInPath()
    {
        const int id = 42;
        var response = new ApiResult<Currency>
        {
            IsSuccess = true,
            Data = new Currency(id, "CHF", 0.05)
        };
        ConnectionHandler
            .GetAsync<Currency>(Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(response));

        var result = await _sut.GetById(id);

        Assert.That(result, Is.SameAs(response));
        await ConnectionHandler.Received(1).GetAsync<Currency>(
            $"{ExpectedPath}/{id}",
            null,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// <c>GetExchangeRates</c> calls the handler with the nested
    /// <c>3.0/currencies/{id}/exchange_rates</c> path and a <see langword="null" /> query parameter
    /// when no date filter is supplied.
    /// </summary>
    [Test]
    public async Task GetExchangeRates_WithNoQueryParameter_CallsGetAsyncWithExpectedPath()
    {
        const int id = 7;
        var response = new ApiResult<List<ExchangeRate>>
        {
            IsSuccess = true,
            Data = []
        };
        ConnectionHandler
            .GetAsync<List<ExchangeRate>>(Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(response));

        var result = await _sut.GetExchangeRates(id);

        Assert.That(result, Is.SameAs(response));
        await ConnectionHandler.Received(1).GetAsync<List<ExchangeRate>>(
            $"{ExpectedPath}/{id}/exchange_rates",
            null,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// When a <see cref="QueryParameterExchangeRate" /> is supplied with a date, the wrapped
    /// <see cref="QueryParameter" /> must be passed through to the connection handler so the
    /// <c>date</c> filter reaches the API.
    /// </summary>
    [Test]
    public async Task GetExchangeRates_WithDateQueryParameter_PassesQueryParameterToHandler()
    {
        const int id = 3;
        var query = new QueryParameterExchangeRate(new DateOnly(2024, 5, 1));
        var response = new ApiResult<List<ExchangeRate>>
        {
            IsSuccess = true,
            Data = []
        };
        ConnectionHandler
            .GetAsync<List<ExchangeRate>>(Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(response));

        await _sut.GetExchangeRates(id, query);

        await ConnectionHandler.Received(1).GetAsync<List<ExchangeRate>>(
            $"{ExpectedPath}/{id}/exchange_rates",
            query.QueryParameter,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// <c>Create</c> forwards the payload and the <c>3.0/currencies</c> endpoint path to
    /// <see cref="IBexioConnectionHandler.PostAsync{TResult,TCreate}" />.
    /// </summary>
    [Test]
    public async Task Create_CallsPostAsyncWithExpectedPath()
    {
        var payload = new CurrencyCreate("CHF", 0.05);
        var response = new ApiResult<Currency>
        {
            IsSuccess = true,
            Data = new Currency(1, "CHF", 0.05)
        };
        ConnectionHandler
            .PostAsync<Currency, CurrencyCreate>(
                Arg.Any<CurrencyCreate>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>())
            .Returns(response);

        var result = await _sut.Create(payload);

        Assert.That(result, Is.SameAs(response));
        await ConnectionHandler.Received(1).PostAsync<Currency, CurrencyCreate>(
            payload,
            ExpectedPath,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// <c>Patch</c> forwards the patch payload and the <c>3.0/currencies/{id}</c> endpoint path
    /// to <see cref="IBexioConnectionHandler.PatchAsync{TResult,TPatch}" />.
    /// </summary>
    [Test]
    public async Task Patch_CallsPatchAsyncWithIdInPath()
    {
        const int id = 99;
        var payload = new CurrencyPatch(0.10);
        var response = new ApiResult<Currency>
        {
            IsSuccess = true,
            Data = new Currency(id, "CHF", 0.10)
        };
        ConnectionHandler
            .PatchAsync<Currency, CurrencyPatch>(
                Arg.Any<CurrencyPatch>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>())
            .Returns(response);

        var result = await _sut.Patch(id, payload);

        Assert.That(result, Is.SameAs(response));
        await ConnectionHandler.Received(1).PatchAsync<Currency, CurrencyPatch>(
            payload,
            $"{ExpectedPath}/{id}",
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// <c>Delete</c> forwards the call to <see cref="IBexioConnectionHandler.Delete" /> exactly
    /// once, building the path with the currency id.
    /// </summary>
    [Test]
    public async Task Delete_CallsConnectionHandlerDeleteWithIdInPath()
    {
        const int id = 42;
        var response = new ApiResult<object> { IsSuccess = true };
        string? capturedPath = null;
        ConnectionHandler
            .Delete(
                Arg.Do<string>(path => capturedPath = path),
                Arg.Any<CancellationToken>())
            .Returns(response);

        var result = await _sut.Delete(id);

        Assert.That(result, Is.SameAs(response));
        Assert.That(capturedPath, Is.EqualTo($"{ExpectedPath}/{id}"));
        await ConnectionHandler.Received(1).Delete(
            Arg.Any<string>(),
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// When <c>autoPage</c> is on and the first response advertises a
    /// <c>X-Total-Count</c> header, the service must call <c>FetchAll</c> with
    /// the count of already-fetched items, the total, the same path, and the
    /// same query parameter so the rest of the result set is loaded.
    /// </summary>
    [Test]
    public async Task Get_WithAutoPage_WhenTotalResultsHeaderPresent_CallsFetchAll()
    {
        var first = new Currency(1, "CHF", 0.05);
        var firstPage = new ApiResult<List<Currency>>
        {
            IsSuccess = true,
            Data = [first],
            ResponseHeaders = new Dictionary<string, int?>
            {
                [ApiHeaderNames.TotalResults] = 3
            }
        };
        ConnectionHandler
            .GetAsync<List<Currency>>(Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(firstPage));
        var remaining = new List<Currency>
        {
            new(2, "EUR", 0.01),
            new(3, "USD", 0.01)
        };
        ConnectionHandler
            .FetchAll<Currency>(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(remaining));

        var result = await _sut.Get(autoPage: true);

        await ConnectionHandler.Received(1).FetchAll<Currency>(
            1,
            3,
            ExpectedPath,
            null,
            Arg.Any<CancellationToken>());
        Assert.That(result.Data, Has.Count.EqualTo(3));
    }

    /// <summary>
    /// When a <see cref="QueryParameterCurrency"/> is supplied, its inner <see cref="QueryParameter"/>
    /// is forwarded to the connection handler verbatim — the service must not rewrap or substitute it.
    /// </summary>
    [Test]
    public async Task Get_WithQueryParameter_PassesQueryParameterToConnectionHandler()
    {
        var query = new QueryParameterCurrency(Limit: 100, Offset: 0, Embed: "exchange_rate", Date: new DateOnly(2026, 4, 30));
        ConnectionHandler
            .GetAsync<List<Currency>>(Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new ApiResult<List<Currency>> { IsSuccess = true, Data = [] }));

        await _sut.Get(query);

        await ConnectionHandler.Received(1).GetAsync<List<Currency>>(
            ExpectedPath,
            query.QueryParameter,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// <c>Create</c> must call the connection handler's POST exactly once and forward the supplied
    /// payload — the service should not mutate or wrap the payload.
    /// </summary>
    [Test]
    public async Task Create_ForwardsPayloadVerbatim()
    {
        var payload = new CurrencyCreate("USD", 0.01);
        ConnectionHandler
            .PostAsync<Currency, CurrencyCreate>(
                Arg.Any<CurrencyCreate>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>())
            .Returns(new ApiResult<Currency> { IsSuccess = true });

        await _sut.Create(payload);

        await ConnectionHandler.Received(1).PostAsync<Currency, CurrencyCreate>(
            payload,
            ExpectedPath,
            Arg.Any<CancellationToken>());
    }
}
