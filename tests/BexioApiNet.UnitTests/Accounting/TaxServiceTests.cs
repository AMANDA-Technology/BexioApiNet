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
using BexioApiNet.Abstractions.Models.Accounting.Taxes;
using BexioApiNet.Abstractions.Models.Accounting.Taxes.Enums;
using BexioApiNet.Abstractions.Models.Api;
using BexioApiNet.Models;
using BexioApiNet.Services.Connectors.Accounting;

namespace BexioApiNet.UnitTests.Accounting;

/// <summary>
/// Offline unit tests for <see cref="TaxService"/>. Verifies the connector
/// builds the right request path and forwards the connection handler's
/// <see cref="ApiResult{T}"/> back to the caller without mutation.
/// </summary>
[TestFixture]
public sealed class TaxServiceTests : ServiceTestBase
{
    private const string ExpectedPath = "3.0/taxes";

    /// <summary>
    /// With no parameters the service hits <c>3.0/taxes</c> exactly once with
    /// a <see langword="null"/> query parameter and returns the connection
    /// handler's <see cref="ApiResult{T}"/> as-is.
    /// </summary>
    [Test]
    public async Task Get_WithNoParams_CallsGetAsyncOnceWithExpectedPath()
    {
        var expected = new ApiResult<List<Tax>>
        {
            IsSuccess = true,
            Data = [NewTax(1)]
        };
        ConnectionHandler
            .GetAsync<List<Tax>>(Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

        var service = new TaxService(ConnectionHandler);

        var result = await service.Get();

        Assert.That(result, Is.SameAs(expected));
        await ConnectionHandler.Received(1).GetAsync<List<Tax>>(
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
            .GetAsync<List<Tax>>(Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new ApiResult<List<Tax>> { IsSuccess = true, Data = [] }));

        var service = new TaxService(ConnectionHandler);

        await service.Get(cancellationToken: cts.Token);

        await ConnectionHandler.Received(1).GetAsync<List<Tax>>(
            ExpectedPath,
            null,
            cts.Token);
    }

    /// <summary>
    /// A failing <see cref="ApiResult{T}"/> from the connection handler must
    /// surface to the caller untouched — the service may not swallow errors.
    /// </summary>
    [Test]
    public async Task Get_ReturnsApiResultFromConnectionHandlerUnchanged()
    {
        var apiError = new ApiError(ErrorCode: 404, Message: "not found", Errors: new object());
        var response = new ApiResult<List<Tax>>
        {
            IsSuccess = false,
            ApiError = apiError,
            StatusCode = System.Net.HttpStatusCode.NotFound,
            Data = null
        };
        ConnectionHandler
            .GetAsync<List<Tax>>(Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(response));

        var service = new TaxService(ConnectionHandler);

        var result = await service.Get();

        Assert.That(result, Is.SameAs(response));
    }

    /// <summary>
    /// GetById builds the request path with the tax id appended to the endpoint
    /// root and forwards a single <c>GetAsync</c> call to the connection handler
    /// with a <see langword="null"/> query parameter.
    /// </summary>
    [Test]
    public async Task GetById_CallsGetAsyncWithExpectedPath()
    {
        const int id = 42;
        var expected = new ApiResult<Tax>
        {
            IsSuccess = true,
            Data = NewTax(id)
        };
        ConnectionHandler
            .GetAsync<Tax>(Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

        var service = new TaxService(ConnectionHandler);

        var result = await service.GetById(id);

        Assert.That(result, Is.SameAs(expected));
        await ConnectionHandler.Received(1).GetAsync<Tax>(
            $"{ExpectedPath}/{id}",
            null,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// GetById forwards the caller-supplied <see cref="CancellationToken"/> so
    /// cooperative cancellation flows end-to-end.
    /// </summary>
    [Test]
    public async Task GetById_ForwardsCancellationTokenToConnectionHandler()
    {
        const int id = 7;
        using var cts = new CancellationTokenSource();
        ConnectionHandler
            .GetAsync<Tax>(Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new ApiResult<Tax> { IsSuccess = true, Data = NewTax(id) }));

        var service = new TaxService(ConnectionHandler);

        await service.GetById(id, cts.Token);

        await ConnectionHandler.Received(1).GetAsync<Tax>(
            $"{ExpectedPath}/{id}",
            null,
            cts.Token);
    }

    /// <summary>
    /// A failing <see cref="ApiResult{T}"/> from the connection handler must
    /// surface to the caller untouched — the service may not swallow errors.
    /// </summary>
    [Test]
    public async Task GetById_ReturnsApiResultFromConnectionHandlerUnchanged()
    {
        var apiError = new ApiError(ErrorCode: 404, Message: "not found", Errors: new object());
        var response = new ApiResult<Tax>
        {
            IsSuccess = false,
            ApiError = apiError,
            StatusCode = System.Net.HttpStatusCode.NotFound,
            Data = null
        };
        ConnectionHandler
            .GetAsync<Tax>(Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(response));

        var service = new TaxService(ConnectionHandler);

        var result = await service.GetById(99);

        Assert.That(result, Is.SameAs(response));
    }

    /// <summary>
    /// Delete forwards a single call to <see cref="IBexioConnectionHandler.Delete"/>
    /// with a path built from the endpoint root and the tax id, returning the
    /// connection handler's <see cref="ApiResult{T}"/> as-is.
    /// </summary>
    [Test]
    public async Task Delete_CallsConnectionHandlerDeleteWithExpectedPath()
    {
        const int id = 42;
        var expected = new ApiResult<object> { IsSuccess = true };
        ConnectionHandler
            .Delete(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

        var service = new TaxService(ConnectionHandler);

        var result = await service.Delete(id);

        Assert.That(result, Is.SameAs(expected));
        await ConnectionHandler.Received(1).Delete(
            $"{ExpectedPath}/{id}",
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Delete forwards the caller-supplied <see cref="CancellationToken"/> so
    /// cooperative cancellation flows end-to-end.
    /// </summary>
    [Test]
    public async Task Delete_ForwardsCancellationTokenToConnectionHandler()
    {
        const int id = 7;
        using var cts = new CancellationTokenSource();
        ConnectionHandler
            .Delete(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new ApiResult<object> { IsSuccess = true }));

        var service = new TaxService(ConnectionHandler);

        await service.Delete(id, cts.Token);

        await ConnectionHandler.Received(1).Delete(
            $"{ExpectedPath}/{id}",
            cts.Token);
    }

    /// <summary>
    /// A failing <see cref="ApiResult{T}"/> (e.g. a 409 Conflict when the tax is
    /// referenced elsewhere) must propagate to the caller without mutation.
    /// </summary>
    [Test]
    public async Task Delete_ReturnsApiResultFromConnectionHandlerUnchanged()
    {
        var apiError = new ApiError(ErrorCode: 409, Message: "Conflict", Errors: new object());
        var response = new ApiResult<object>
        {
            IsSuccess = false,
            ApiError = apiError,
            StatusCode = System.Net.HttpStatusCode.Conflict,
            Data = null
        };
        ConnectionHandler
            .Delete(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(response));

        var service = new TaxService(ConnectionHandler);

        var result = await service.Delete(1);

        Assert.That(result, Is.SameAs(response));
    }

    /// <summary>
    /// When a <see cref="QueryParameterTax"/> is supplied, its inner <see cref="QueryParameter"/>
    /// is forwarded to the connection handler verbatim so the optional <c>scope</c>, <c>date</c>,
    /// <c>types</c>, <c>limit</c> and <c>offset</c> query parameters reach the API.
    /// </summary>
    [Test]
    public async Task Get_WithQueryParameter_PassesQueryParameterToConnectionHandler()
    {
        var query = new QueryParameterTax(
            Scope: TaxScope.active,
            Date: new DateOnly(2026, 4, 30),
            Type: TaxType.sales_tax,
            Limit: 50,
            Offset: 100);
        ConnectionHandler
            .GetAsync<List<Tax>>(Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new ApiResult<List<Tax>> { IsSuccess = true, Data = [] }));

        var service = new TaxService(ConnectionHandler);

        await service.Get(query);

        await ConnectionHandler.Received(1).GetAsync<List<Tax>>(
            ExpectedPath,
            query.QueryParameter,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// When <c>autoPage</c> is on and the first response advertises a <c>X-Total-Count</c>
    /// header, the service must call <c>FetchAll</c> with the count of already-fetched items,
    /// the total, the same path, and the same query parameter so the rest of the result set
    /// is loaded.
    /// </summary>
    [Test]
    public async Task Get_WithAutoPage_WhenTotalResultsHeaderPresent_CallsFetchAll()
    {
        var first = NewTax(1);
        var firstPage = new ApiResult<List<Tax>>
        {
            IsSuccess = true,
            Data = [first],
            ResponseHeaders = new Dictionary<string, int?>
            {
                [ApiHeaderNames.TotalResults] = 3
            }
        };
        ConnectionHandler
            .GetAsync<List<Tax>>(Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(firstPage));
        var remaining = new List<Tax> { NewTax(2), NewTax(3) };
        ConnectionHandler
            .FetchAll<Tax>(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(remaining));

        var service = new TaxService(ConnectionHandler);

        var result = await service.Get(autoPage: true);

        await ConnectionHandler.Received(1).FetchAll<Tax>(
            1,
            3,
            ExpectedPath,
            null,
            Arg.Any<CancellationToken>());
        Assert.That(result.Data, Has.Count.EqualTo(3));
    }

    private static Tax NewTax(int id) =>
        new(
            Id: id,
            Uuid: $"uuid-{id}",
            Name: $"Tax {id}",
            Code: $"T{id}",
            Digit: id.ToString(),
            Type: "sales_tax",
            AccountId: null,
            TaxSettlementType: "effective",
            Value: 7.7m,
            NetTaxValue: null,
            StartYear: null,
            EndYear: null,
            IsActive: true,
            DisplayName: $"Tax {id}",
            StartMonth: null,
            EndMonth: null);
}

