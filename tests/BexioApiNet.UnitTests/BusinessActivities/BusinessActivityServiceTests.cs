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
using BexioApiNet.Abstractions.Models.BusinessActivities.BusinessActivity;
using BexioApiNet.Abstractions.Models.BusinessActivities.BusinessActivity.Views;
using BexioApiNet.Models;
using BexioApiNet.Services.Connectors.BusinessActivities;

namespace BexioApiNet.UnitTests.BusinessActivities;

/// <summary>
/// Offline unit tests for <see cref="BusinessActivityService"/>. Each test verifies that the
/// service forwards its calls to <see cref="IBexioConnectionHandler"/> with the expected
/// arguments and returns the handler's result unchanged. No network access.
/// </summary>
[TestFixture]
public sealed class BusinessActivityServiceTests : ServiceTestBase
{
    private const string ExpectedEndpoint = "2.0/client_service";
    private const string ExpectedSearchEndpoint = "2.0/client_service/search";

    private BusinessActivityService _sut = null!;

    /// <summary>
    /// Creates a fresh <see cref="BusinessActivityService"/> per test bound to the
    /// <see cref="ServiceTestBase.ConnectionHandler"/> substitute from the base fixture.
    /// </summary>
    [SetUp]
    public void CreateSut()
    {
        _sut = new BusinessActivityService(ConnectionHandler);
    }

    /// <summary>
    /// With no parameters <c>Get</c> hits <c>2.0/client_service</c> exactly once with a
    /// <see langword="null"/> query parameter.
    /// </summary>
    [Test]
    public async Task Get_WithNoParams_CallsGetAsyncOnceWithExpectedPath()
    {
        var response = new ApiResult<List<BusinessActivity>?>
        {
            IsSuccess = true,
            Data = [new BusinessActivity(Id: 1, Name: "Consulting", DefaultIsBillable: true, DefaultPricePerHour: 150m, AccountId: null)]
        };
        ConnectionHandler
            .GetAsync<List<BusinessActivity>?>(Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(response));

        var result = await _sut.Get();

        result.ShouldBeSameAs(response);
        await ConnectionHandler.Received(1).GetAsync<List<BusinessActivity>?>(
            ExpectedEndpoint,
            null,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// When a <see cref="QueryParameterBusinessActivity"/> is supplied, its inner
    /// <see cref="QueryParameter"/> instance is forwarded verbatim — the service must
    /// not rewrap or substitute it.
    /// </summary>
    [Test]
    public async Task Get_WithQueryParameter_PassesQueryParameterToConnectionHandler()
    {
        var queryParameter = new QueryParameterBusinessActivity(Limit: 50, Offset: 25);
        ConnectionHandler
            .GetAsync<List<BusinessActivity>?>(Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new ApiResult<List<BusinessActivity>?> { IsSuccess = true, Data = [] }));

        await _sut.Get(queryParameter);

        await ConnectionHandler.Received(1).GetAsync<List<BusinessActivity>?>(
            ExpectedEndpoint,
            queryParameter.QueryParameter,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// <c>Get(autoPage: true)</c> triggers <see cref="IBexioConnectionHandler.FetchAll{TResult}"/>
    /// when the <c>X-Total-Count</c> header is present and the first response only returned a page.
    /// </summary>
    [Test]
    public async Task Get_WithAutoPage_CallsFetchAll()
    {
        const int totalResults = 8;
        var initialData = new List<BusinessActivity>
        {
            new(Id: 1, Name: "Photography", DefaultIsBillable: true, DefaultPricePerHour: null, AccountId: null),
            new(Id: 2, Name: "Design", DefaultIsBillable: true, DefaultPricePerHour: null, AccountId: null)
        };
        var initial = new ApiResult<List<BusinessActivity>?>
        {
            IsSuccess = true,
            Data = initialData,
            ResponseHeaders = new Dictionary<string, int?>
            {
                { ApiHeaderNames.TotalResults, totalResults }
            }
        };
        ConnectionHandler
            .GetAsync<List<BusinessActivity>?>(Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(initial));
        ConnectionHandler
            .FetchAll<BusinessActivity>(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns([]);

        await _sut.Get(autoPage: true);

        await ConnectionHandler.Received(1).FetchAll<BusinessActivity>(
            initialData.Count,
            totalResults,
            ExpectedEndpoint,
            null,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// The cancellation token supplied by the caller of <c>Get</c> must be forwarded to the
    /// connection handler so cooperative cancellation flows end-to-end.
    /// </summary>
    [Test]
    public async Task Get_ForwardsCancellationTokenToConnectionHandler()
    {
        using var cts = new CancellationTokenSource();
        ConnectionHandler
            .GetAsync<List<BusinessActivity>?>(Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new ApiResult<List<BusinessActivity>?> { IsSuccess = true, Data = [] }));

        await _sut.Get(cancellationToken: cts.Token);

        await ConnectionHandler.Received(1).GetAsync<List<BusinessActivity>?>(
            ExpectedEndpoint,
            null,
            cts.Token);
    }

    /// <summary>
    /// A failing <see cref="ApiResult{T}"/> from the connection handler must surface to the
    /// caller untouched — the service may not swallow errors.
    /// </summary>
    [Test]
    public async Task Get_ReturnsApiResultFromConnectionHandlerUnchanged()
    {
        var apiError = new ApiError(ErrorCode: 401, Message: "unauthorized", Errors: new object());
        var response = new ApiResult<List<BusinessActivity>?>
        {
            IsSuccess = false,
            ApiError = apiError,
            StatusCode = System.Net.HttpStatusCode.Unauthorized,
            Data = null
        };
        ConnectionHandler
            .GetAsync<List<BusinessActivity>?>(Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(response));

        var result = await _sut.Get();

        result.ShouldBeSameAs(response);
    }

    /// <summary>
    /// <c>Create</c> posts the supplied <see cref="BusinessActivityCreate"/> to
    /// <c>2.0/client_service</c> via <see cref="IBexioConnectionHandler.PostAsync{TOut, TIn}"/>.
    /// </summary>
    [Test]
    public async Task Create_CallsPostAsyncWithExpectedPathAndBody()
    {
        var createModel = new BusinessActivityCreate(Name: "New Activity", DefaultIsBillable: true, DefaultPricePerHour: 100m, AccountId: null);
        var response = new ApiResult<BusinessActivity>
        {
            IsSuccess = true,
            Data = new BusinessActivity(Id: 2, Name: "New Activity", DefaultIsBillable: true, DefaultPricePerHour: 100m, AccountId: null)
        };
        ConnectionHandler
            .PostAsync<BusinessActivity, BusinessActivityCreate>(Arg.Any<BusinessActivityCreate>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(response));

        var result = await _sut.Create(createModel);

        result.ShouldBeSameAs(response);
        await ConnectionHandler.Received(1).PostAsync<BusinessActivity, BusinessActivityCreate>(
            createModel,
            ExpectedEndpoint,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// The cancellation token supplied by the caller of <c>Create</c> must be forwarded to
    /// the connection handler.
    /// </summary>
    [Test]
    public async Task Create_ForwardsCancellationTokenToConnectionHandler()
    {
        using var cts = new CancellationTokenSource();
        var createModel = new BusinessActivityCreate(Name: "Ad-hoc", DefaultIsBillable: false, DefaultPricePerHour: null, AccountId: null);
        ConnectionHandler
            .PostAsync<BusinessActivity, BusinessActivityCreate>(Arg.Any<BusinessActivityCreate>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new ApiResult<BusinessActivity> { IsSuccess = true }));

        await _sut.Create(createModel, cts.Token);

        await ConnectionHandler.Received(1).PostAsync<BusinessActivity, BusinessActivityCreate>(
            createModel,
            ExpectedEndpoint,
            cts.Token);
    }

    /// <summary>
    /// A failing <see cref="ApiResult{T}"/> returned by <see cref="IBexioConnectionHandler.PostAsync{TOut, TIn}"/>
    /// must surface to the caller of <c>Create</c> untouched.
    /// </summary>
    [Test]
    public async Task Create_ReturnsApiResultFromConnectionHandlerUnchanged()
    {
        var apiError = new ApiError(ErrorCode: 422, Message: "name is required", Errors: new object());
        var response = new ApiResult<BusinessActivity>
        {
            IsSuccess = false,
            ApiError = apiError,
            StatusCode = System.Net.HttpStatusCode.UnprocessableEntity,
            Data = null
        };
        ConnectionHandler
            .PostAsync<BusinessActivity, BusinessActivityCreate>(Arg.Any<BusinessActivityCreate>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(response));

        var result = await _sut.Create(new BusinessActivityCreate(Name: string.Empty, DefaultIsBillable: null, DefaultPricePerHour: null, AccountId: null));

        result.ShouldBeSameAs(response);
    }

    /// <summary>
    /// <c>Search</c> posts the supplied <see cref="SearchCriteria"/> list to
    /// <c>2.0/client_service/search</c> via <see cref="IBexioConnectionHandler.PostSearchAsync{TResult}"/>.
    /// </summary>
    [Test]
    public async Task Search_CallsPostSearchAsyncWithExpectedPathAndBody()
    {
        var criteria = new List<SearchCriteria>
        {
            new() { Field = "name", Value = "Consulting", Criteria = "like" }
        };
        var response = new ApiResult<List<BusinessActivity>>
        {
            IsSuccess = true,
            Data = [new BusinessActivity(Id: 1, Name: "Consulting", DefaultIsBillable: true, DefaultPricePerHour: null, AccountId: null)]
        };
        ConnectionHandler
            .PostSearchAsync<BusinessActivity>(Arg.Any<List<SearchCriteria>>(), Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(response));

        var result = await _sut.Search(criteria);

        result.ShouldBeSameAs(response);
        await ConnectionHandler.Received(1).PostSearchAsync<BusinessActivity>(
            criteria,
            ExpectedSearchEndpoint,
            null,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// When a <see cref="QueryParameterBusinessActivity"/> is supplied to <c>Search</c>, the
    /// inner <see cref="QueryParameter"/> is forwarded to the handler so pagination /
    /// ordering parameters reach the URI.
    /// </summary>
    [Test]
    public async Task Search_WithQueryParameter_PassesQueryParameterToConnectionHandler()
    {
        var criteria = new List<SearchCriteria>
        {
            new() { Field = "name", Value = "Design", Criteria = "=" }
        };
        var queryParameter = new QueryParameterBusinessActivity(Limit: 10, Offset: 0);
        ConnectionHandler
            .PostSearchAsync<BusinessActivity>(Arg.Any<List<SearchCriteria>>(), Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new ApiResult<List<BusinessActivity>> { IsSuccess = true, Data = [] }));

        await _sut.Search(criteria, queryParameter);

        await ConnectionHandler.Received(1).PostSearchAsync<BusinessActivity>(
            criteria,
            ExpectedSearchEndpoint,
            queryParameter.QueryParameter,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// The cancellation token supplied by the caller of <c>Search</c> must be forwarded to
    /// the connection handler.
    /// </summary>
    [Test]
    public async Task Search_ForwardsCancellationTokenToConnectionHandler()
    {
        using var cts = new CancellationTokenSource();
        var criteria = new List<SearchCriteria>
        {
            new() { Field = "name", Value = "Foo", Criteria = "=" }
        };
        ConnectionHandler
            .PostSearchAsync<BusinessActivity>(Arg.Any<List<SearchCriteria>>(), Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new ApiResult<List<BusinessActivity>> { IsSuccess = true, Data = [] }));

        await _sut.Search(criteria, cancellationToken: cts.Token);

        await ConnectionHandler.Received(1).PostSearchAsync<BusinessActivity>(
            criteria,
            ExpectedSearchEndpoint,
            null,
            cts.Token);
    }

    /// <summary>
    /// A failing <see cref="ApiResult{T}"/> returned by <see cref="IBexioConnectionHandler.PostSearchAsync{TResult}"/>
    /// must surface to the caller of <c>Search</c> untouched.
    /// </summary>
    [Test]
    public async Task Search_ReturnsApiResultFromConnectionHandlerUnchanged()
    {
        var apiError = new ApiError(ErrorCode: 422, Message: "invalid search", Errors: new object());
        var response = new ApiResult<List<BusinessActivity>>
        {
            IsSuccess = false,
            ApiError = apiError,
            StatusCode = System.Net.HttpStatusCode.UnprocessableEntity,
            Data = null
        };
        ConnectionHandler
            .PostSearchAsync<BusinessActivity>(Arg.Any<List<SearchCriteria>>(), Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(response));

        var result = await _sut.Search([]);

        result.ShouldBeSameAs(response);
    }
}
