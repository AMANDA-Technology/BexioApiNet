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
using BexioApiNet.Abstractions.Models.Api;
using BexioApiNet.Abstractions.Models.Banking.PaymentTypes;
using BexioApiNet.Models;
using BexioApiNet.Services.Connectors.Banking;

namespace BexioApiNet.UnitTests.Banking;

/// <summary>
///     Offline unit tests for <see cref="PaymentTypeService" />. The service is a
///     thin pass-through over <see cref="IBexioConnectionHandler.GetAsync{T}" /> and
///     <see cref="IBexioConnectionHandler.PostSearchAsync{T}" />, so verification
///     focuses on path, verb, body, query parameter, and pass-through semantics.
/// </summary>
[TestFixture]
public sealed class PaymentTypeServiceTests : ServiceTestBase
{
    private const string ExpectedListPath = "2.0/payment_type";
    private const string ExpectedSearchPath = "2.0/payment_type/search";

    /// <summary>
    ///     With no parameters the service hits <c>2.0/payment_type</c> exactly once
    ///     with a <see langword="null" /> query parameter and returns the connection
    ///     handler's <see cref="ApiResult{T}" /> as-is.
    /// </summary>
    [Test]
    public async Task Get_WithNoParams_CallsGetAsyncOnceWithExpectedPath()
    {
        var expected = new ApiResult<List<PaymentType>>
        {
            IsSuccess = true,
            Data = [new PaymentType(1, "Cash")]
        };
        ConnectionHandler
            .GetAsync<List<PaymentType>>(Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

        var service = new PaymentTypeService(ConnectionHandler);

        var result = await service.Get();

        Assert.That(result, Is.SameAs(expected));
        await ConnectionHandler.Received(1).GetAsync<List<PaymentType>>(
            ExpectedListPath,
            null,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    ///     When a <see cref="QueryParameterPaymentType" /> is supplied, its inner
    ///     <see cref="QueryParameter" /> instance is forwarded to the connection
    ///     handler verbatim — the service must not rewrap or substitute it.
    /// </summary>
    [Test]
    public async Task Get_WithQueryParameter_PassesQueryParameterToConnectionHandler()
    {
        var queryParameter = new QueryParameterPaymentType(20, 40);
        ConnectionHandler
            .GetAsync<List<PaymentType>>(Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new ApiResult<List<PaymentType>> { IsSuccess = true, Data = [] }));

        var service = new PaymentTypeService(ConnectionHandler);

        await service.Get(queryParameter);

        await ConnectionHandler.Received(1).GetAsync<List<PaymentType>>(
            ExpectedListPath,
            queryParameter.QueryParameter,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    ///     The service must never invoke <c>FetchAll</c>: <see cref="PaymentTypeService" />
    ///     does not implement auto-paging even when <c>autoPage</c> is set.
    /// </summary>
    [Test]
    public async Task Get_WithAutoPage_DoesNotCallFetchAll()
    {
        var response = new ApiResult<List<PaymentType>>
        {
            IsSuccess = true,
            Data = [new PaymentType(1, "Cash")]
        };
        ConnectionHandler
            .GetAsync<List<PaymentType>>(Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(response));

        var service = new PaymentTypeService(ConnectionHandler);

        await service.Get(autoPage: true);

        await ConnectionHandler.DidNotReceive().FetchAll<PaymentType>(
            Arg.Any<int>(),
            Arg.Any<int>(),
            Arg.Any<string>(),
            Arg.Any<QueryParameter?>(),
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    ///     The cancellation token supplied by the caller must be forwarded to the
    ///     connection handler so cooperative cancellation flows end-to-end.
    /// </summary>
    [Test]
    public async Task Get_ForwardsCancellationTokenToConnectionHandler()
    {
        using var cts = new CancellationTokenSource();
        ConnectionHandler
            .GetAsync<List<PaymentType>>(Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new ApiResult<List<PaymentType>> { IsSuccess = true, Data = [] }));

        var service = new PaymentTypeService(ConnectionHandler);

        await service.Get(cancellationToken: cts.Token);

        await ConnectionHandler.Received(1).GetAsync<List<PaymentType>>(
            ExpectedListPath,
            null,
            cts.Token);
    }

    /// <summary>
    ///     A failing <see cref="ApiResult{T}" /> from the connection handler must
    ///     surface to the caller untouched — the service may not swallow errors.
    /// </summary>
    [Test]
    public async Task Get_ReturnsApiResultFromConnectionHandlerUnchanged()
    {
        var apiError = new ApiError(403, "forbidden", new object());
        var response = new ApiResult<List<PaymentType>>
        {
            IsSuccess = false,
            ApiError = apiError,
            StatusCode = HttpStatusCode.Forbidden,
            Data = null
        };
        ConnectionHandler
            .GetAsync<List<PaymentType>>(Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(response));

        var service = new PaymentTypeService(ConnectionHandler);

        var result = await service.Get();

        Assert.That(result, Is.SameAs(response));
    }

    /// <summary>
    ///     With a search body and no query parameter, the service POSTs the criteria
    ///     list to <c>2.0/payment_type/search</c> with a <see langword="null" /> query
    ///     parameter and returns the connection handler's <see cref="ApiResult{T}" />
    ///     unchanged.
    /// </summary>
    [Test]
    public async Task Search_WithCriteria_CallsPostSearchAsyncWithExpectedPath()
    {
        var criteria = new List<SearchCriteria>
        {
            new() { Field = "name", Value = "Cash", Criteria = "=" }
        };
        var expected = new ApiResult<List<PaymentType>>
        {
            IsSuccess = true,
            Data = [new PaymentType(1, "Cash")]
        };
        ConnectionHandler
            .PostSearchAsync<PaymentType>(Arg.Any<List<SearchCriteria>>(), Arg.Any<string>(),
                Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

        var service = new PaymentTypeService(ConnectionHandler);

        var result = await service.Search(criteria);

        Assert.That(result, Is.SameAs(expected));
        await ConnectionHandler.Received(1).PostSearchAsync<PaymentType>(
            criteria,
            ExpectedSearchPath,
            null,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    ///     When a <see cref="QueryParameterPaymentType" /> is supplied to
    ///     <see cref="PaymentTypeService.Search" />, its inner <see cref="QueryParameter" />
    ///     must flow to the connection handler verbatim so pagination and sort flags
    ///     are preserved on the wire.
    /// </summary>
    [Test]
    public async Task Search_WithQueryParameter_PassesQueryParameterToConnectionHandler()
    {
        var criteria = new List<SearchCriteria>
        {
            new() { Field = "name", Value = "Bank", Criteria = "like" }
        };
        var queryParameter = new QueryParameterPaymentType(10, 5);
        ConnectionHandler
            .PostSearchAsync<PaymentType>(Arg.Any<List<SearchCriteria>>(), Arg.Any<string>(),
                Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new ApiResult<List<PaymentType>> { IsSuccess = true, Data = [] }));

        var service = new PaymentTypeService(ConnectionHandler);

        await service.Search(criteria, queryParameter);

        await ConnectionHandler.Received(1).PostSearchAsync<PaymentType>(
            criteria,
            ExpectedSearchPath,
            queryParameter.QueryParameter,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    ///     The cancellation token supplied by the caller to <see cref="PaymentTypeService.Search" />
    ///     must be forwarded to the connection handler unchanged.
    /// </summary>
    [Test]
    public async Task Search_ForwardsCancellationTokenToConnectionHandler()
    {
        using var cts = new CancellationTokenSource();
        var criteria = new List<SearchCriteria>
        {
            new() { Field = "name", Value = "Cash", Criteria = "=" }
        };
        ConnectionHandler
            .PostSearchAsync<PaymentType>(Arg.Any<List<SearchCriteria>>(), Arg.Any<string>(),
                Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new ApiResult<List<PaymentType>> { IsSuccess = true, Data = [] }));

        var service = new PaymentTypeService(ConnectionHandler);

        await service.Search(criteria, cancellationToken: cts.Token);

        await ConnectionHandler.Received(1).PostSearchAsync<PaymentType>(
            criteria,
            ExpectedSearchPath,
            null,
            cts.Token);
    }

    /// <summary>
    ///     A failing <see cref="ApiResult{T}" /> from the connection handler must
    ///     surface to the caller untouched — <see cref="PaymentTypeService.Search" />
    ///     may not swallow or remap errors.
    /// </summary>
    [Test]
    public async Task Search_ReturnsApiResultFromConnectionHandlerUnchanged()
    {
        var criteria = new List<SearchCriteria>
        {
            new() { Field = "name", Value = "Nope", Criteria = "=" }
        };
        var apiError = new ApiError(400, "bad search", new object());
        var response = new ApiResult<List<PaymentType>>
        {
            IsSuccess = false,
            ApiError = apiError,
            StatusCode = HttpStatusCode.BadRequest,
            Data = null
        };
        ConnectionHandler
            .PostSearchAsync<PaymentType>(Arg.Any<List<SearchCriteria>>(), Arg.Any<string>(),
                Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(response));

        var service = new PaymentTypeService(ConnectionHandler);

        var result = await service.Search(criteria);

        Assert.That(result, Is.SameAs(response));
    }
}
