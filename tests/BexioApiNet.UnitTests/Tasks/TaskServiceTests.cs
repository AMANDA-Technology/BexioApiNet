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
using BexioApiNet.Abstractions.Models.Tasks.Task;
using BexioApiNet.Abstractions.Models.Tasks.Task.Views;
using BexioApiNet.Models;
using BexioApiNet.Services.Connectors.Tasks;

namespace BexioApiNet.UnitTests.Tasks;

/// <summary>
///     Offline unit tests for <see cref="TaskService" />. Each test verifies that the service
///     forwards its calls to <see cref="IBexioConnectionHandler" /> with the expected arguments and
///     returns the handler's result unchanged. No network, no filesystem access.
/// </summary>
[TestFixture]
public sealed class TaskServiceTests : ServiceTestBase
{
    /// <summary>
    ///     Creates a fresh <see cref="TaskService" /> per test, bound to the
    ///     <see cref="ServiceTestBase.ConnectionHandler" /> substitute provided by the base fixture.
    /// </summary>
    [SetUp]
    public void CreateSut()
    {
        _sut = new TaskService(ConnectionHandler);
    }

    private const string ExpectedEndpoint = "2.0/task";

    private TaskService _sut = null!;

    /// <summary>
    ///     Get (no parameters) calls <see cref="IBexioConnectionHandler.GetAsync{TResult}" /> once with
    ///     the expected endpoint path and a null query parameter.
    /// </summary>
    [Test]
    public async Task Get_WithNoParams_CallsGetAsync()
    {
        var response = new ApiResult<List<BexioTask>?> { IsSuccess = true, Data = [] };
        ConnectionHandler
            .GetAsync<List<BexioTask>?>(
                Arg.Any<string>(),
                Arg.Any<QueryParameter?>(),
                Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.Get();

        await ConnectionHandler.Received(1).GetAsync<List<BexioTask>?>(
            ExpectedEndpoint,
            null,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    ///     Get forwards the <see cref="QueryParameterTask" />'s underlying <see cref="QueryParameter" />
    ///     to the connection handler so the caller's filters (limit/offset/order_by) reach the API.
    /// </summary>
    [Test]
    public async Task Get_WithQueryParameter_PassesQueryParameterToConnectionHandler()
    {
        var queryParameter = new QueryParameterTask(100, 50);
        var response = new ApiResult<List<BexioTask>?> { IsSuccess = true, Data = [] };
        ConnectionHandler
            .GetAsync<List<BexioTask>?>(
                Arg.Any<string>(),
                Arg.Any<QueryParameter?>(),
                Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.Get(queryParameter);

        await ConnectionHandler.Received(1).GetAsync<List<BexioTask>?>(
            ExpectedEndpoint,
            queryParameter.QueryParameter,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    ///     Get forwards a <see cref="QueryParameterTask" /> populated with <c>order_by</c> so the
    ///     handler receives the v2 list ordering directive (<c>id</c>, <c>finish_date</c>, optionally
    ///     <c>_asc</c>/<c>_desc</c>).
    /// </summary>
    [Test]
    public async Task Get_WithOrderByQueryParameter_PassesQueryParameterToConnectionHandler()
    {
        var queryParameter = new QueryParameterTask(OrderBy: "finish_date_desc");
        var response = new ApiResult<List<BexioTask>?> { IsSuccess = true, Data = [] };
        ConnectionHandler
            .GetAsync<List<BexioTask>?>(
                Arg.Any<string>(),
                Arg.Any<QueryParameter?>(),
                Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.Get(queryParameter);

        await ConnectionHandler.Received(1).GetAsync<List<BexioTask>?>(
            ExpectedEndpoint,
            queryParameter.QueryParameter,
            Arg.Any<CancellationToken>());
        queryParameter.QueryParameter!.Parameters["order_by"].ShouldBe("finish_date_desc");
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
        var initialData = new List<BexioTask> { BuildTask(1), BuildTask(2) };
        var initial = new ApiResult<List<BexioTask>?>
        {
            IsSuccess = true,
            Data = initialData,
            ResponseHeaders = new Dictionary<string, int?>
            {
                { ApiHeaderNames.TotalResults, totalResults }
            }
        };
        ConnectionHandler
            .GetAsync<List<BexioTask>?>(
                Arg.Any<string>(),
                Arg.Any<QueryParameter?>(),
                Arg.Any<CancellationToken>())
            .Returns(initial);
        ConnectionHandler
            .FetchAll<BexioTask>(
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Any<string>(),
                Arg.Any<QueryParameter?>(),
                Arg.Any<CancellationToken>())
            .Returns([]);

        await _sut.Get(autoPage: true);

        await ConnectionHandler.Received(1).FetchAll<BexioTask>(
            initialData.Count,
            totalResults,
            ExpectedEndpoint,
            null,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    ///     Get (autoPage = false) must not invoke <see cref="IBexioConnectionHandler.FetchAll{TResult}" />,
    ///     even when the connection handler reports a positive <c>X-Total-Count</c>.
    /// </summary>
    [Test]
    public async Task Get_WithoutAutoPage_DoesNotCallFetchAll()
    {
        var response = new ApiResult<List<BexioTask>?>
        {
            IsSuccess = true,
            Data = [BuildTask(1)],
            ResponseHeaders = new Dictionary<string, int?>
            {
                { ApiHeaderNames.TotalResults, 100 }
            }
        };
        ConnectionHandler
            .GetAsync<List<BexioTask>?>(
                Arg.Any<string>(),
                Arg.Any<QueryParameter?>(),
                Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.Get();

        await ConnectionHandler.DidNotReceive().FetchAll<BexioTask>(
            Arg.Any<int>(),
            Arg.Any<int>(),
            Arg.Any<string>(),
            Arg.Any<QueryParameter?>(),
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    ///     Get returns the <see cref="ApiResult{T}" /> produced by the connection handler when
    ///     auto-paging is not requested (no additional FetchAll round-trip, result passes through).
    /// </summary>
    [Test]
    public async Task Get_ReturnsApiResult()
    {
        var response = new ApiResult<List<BexioTask>?> { IsSuccess = true, Data = [] };
        ConnectionHandler
            .GetAsync<List<BexioTask>?>(
                Arg.Any<string>(),
                Arg.Any<QueryParameter?>(),
                Arg.Any<CancellationToken>())
            .Returns(response);

        var result = await _sut.Get();

        result.ShouldBeSameAs(response);
    }

    /// <summary>
    ///     Get forwards the caller's cancellation token to the connection handler so cooperative
    ///     cancellation propagates end-to-end.
    /// </summary>
    [Test]
    public async Task Get_ForwardsCancellationTokenToConnectionHandler()
    {
        using var cts = new CancellationTokenSource();
        ConnectionHandler
            .GetAsync<List<BexioTask>?>(Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(new ApiResult<List<BexioTask>?> { IsSuccess = true, Data = [] });

        await _sut.Get(cancellationToken: cts.Token);

        await ConnectionHandler.Received(1).GetAsync<List<BexioTask>?>(
            ExpectedEndpoint,
            null,
            cts.Token);
    }

    /// <summary>
    ///     GetById calls <see cref="IBexioConnectionHandler.GetAsync{TResult}" /> with the expected
    ///     endpoint path including the task id.
    /// </summary>
    [Test]
    public async Task GetById_CallsGetAsync_WithIdInPath()
    {
        const int id = 42;
        var response = new ApiResult<BexioTask> { IsSuccess = true };
        string? capturedPath = null;
        ConnectionHandler
            .GetAsync<BexioTask>(
                Arg.Do<string>(path => capturedPath = path),
                Arg.Any<QueryParameter?>(),
                Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.GetById(id);

        capturedPath.ShouldBe($"{ExpectedEndpoint}/{id}");
    }

    /// <summary>
    ///     GetById returns the <see cref="ApiResult{T}" /> produced by the connection handler without modification.
    /// </summary>
    [Test]
    public async Task GetById_ReturnsApiResultFromConnectionHandler()
    {
        var response = new ApiResult<BexioTask> { IsSuccess = true };
        ConnectionHandler
            .GetAsync<BexioTask>(
                Arg.Any<string>(),
                Arg.Any<QueryParameter?>(),
                Arg.Any<CancellationToken>())
            .Returns(response);

        var result = await _sut.GetById(1);

        result.ShouldBeSameAs(response);
    }

    /// <summary>
    ///     GetById forwards the caller's cancellation token to the connection handler so cooperative
    ///     cancellation propagates end-to-end.
    /// </summary>
    [Test]
    public async Task GetById_ForwardsCancellationTokenToConnectionHandler()
    {
        using var cts = new CancellationTokenSource();
        ConnectionHandler
            .GetAsync<BexioTask>(Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(new ApiResult<BexioTask> { IsSuccess = true });

        await _sut.GetById(1, cts.Token);

        await ConnectionHandler.Received(1).GetAsync<BexioTask>(
            $"{ExpectedEndpoint}/1",
            null,
            cts.Token);
    }

    /// <summary>
    ///     Create forwards the payload and the expected endpoint path to
    ///     <see cref="IBexioConnectionHandler.PostAsync{TResult,TCreate}" />.
    /// </summary>
    [Test]
    public async Task Create_CallsPostAsync()
    {
        var payload = BuildCreatePayload();
        var response = new ApiResult<BexioTask> { IsSuccess = true };
        ConnectionHandler
            .PostAsync<BexioTask, TaskCreate>(
                Arg.Any<TaskCreate>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.Create(payload);

        await ConnectionHandler.Received(1).PostAsync<BexioTask, TaskCreate>(
            payload,
            ExpectedEndpoint,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    ///     Create returns the <see cref="ApiResult{T}" /> produced by the connection handler without modification.
    /// </summary>
    [Test]
    public async Task Create_ReturnsApiResultFromConnectionHandler()
    {
        var payload = BuildCreatePayload();
        var response = new ApiResult<BexioTask> { IsSuccess = true };
        ConnectionHandler
            .PostAsync<BexioTask, TaskCreate>(
                Arg.Any<TaskCreate>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>())
            .Returns(response);

        var result = await _sut.Create(payload);

        result.ShouldBeSameAs(response);
    }

    /// <summary>
    ///     Create forwards the caller's cancellation token to the connection handler so cooperative
    ///     cancellation propagates end-to-end.
    /// </summary>
    [Test]
    public async Task Create_ForwardsCancellationTokenToConnectionHandler()
    {
        using var cts = new CancellationTokenSource();
        var payload = BuildCreatePayload();
        ConnectionHandler
            .PostAsync<BexioTask, TaskCreate>(Arg.Any<TaskCreate>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new ApiResult<BexioTask> { IsSuccess = true });

        await _sut.Create(payload, cts.Token);

        await ConnectionHandler.Received(1).PostAsync<BexioTask, TaskCreate>(
            payload,
            ExpectedEndpoint,
            cts.Token);
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
            new() { Field = "subject", Value = "Send docs", Criteria = "like" }
        };
        var queryParameter = new QueryParameterTask(50);
        var response = new ApiResult<List<BexioTask>> { IsSuccess = true, Data = [] };
        ConnectionHandler
            .PostSearchAsync<BexioTask>(
                Arg.Any<List<SearchCriteria>>(),
                Arg.Any<string>(),
                Arg.Any<QueryParameter?>(),
                Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.Search(criteria, queryParameter);

        await ConnectionHandler.Received(1).PostSearchAsync<BexioTask>(
            criteria,
            $"{ExpectedEndpoint}/search",
            queryParameter.QueryParameter,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    ///     Search without an explicit query parameter still issues the request; the connection handler
    ///     receives <see langword="null" /> so no query string is appended.
    /// </summary>
    [Test]
    public async Task Search_WithoutQueryParameter_PassesNullToConnectionHandler()
    {
        var criteria = new List<SearchCriteria>
        {
            new() { Field = "subject", Value = "Send docs", Criteria = "=" }
        };
        ConnectionHandler
            .PostSearchAsync<BexioTask>(
                Arg.Any<List<SearchCriteria>>(),
                Arg.Any<string>(),
                Arg.Any<QueryParameter?>(),
                Arg.Any<CancellationToken>())
            .Returns(new ApiResult<List<BexioTask>> { IsSuccess = true, Data = [] });

        await _sut.Search(criteria);

        await ConnectionHandler.Received(1).PostSearchAsync<BexioTask>(
            criteria,
            $"{ExpectedEndpoint}/search",
            null,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    ///     Search forwards the caller's cancellation token to the connection handler so cooperative
    ///     cancellation propagates end-to-end.
    /// </summary>
    [Test]
    public async Task Search_ForwardsCancellationTokenToConnectionHandler()
    {
        using var cts = new CancellationTokenSource();
        var criteria = new List<SearchCriteria>
        {
            new() { Field = "subject", Value = "Send docs", Criteria = "=" }
        };
        ConnectionHandler
            .PostSearchAsync<BexioTask>(Arg.Any<List<SearchCriteria>>(), Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(new ApiResult<List<BexioTask>> { IsSuccess = true, Data = [] });

        await _sut.Search(criteria, cancellationToken: cts.Token);

        await ConnectionHandler.Received(1).PostSearchAsync<BexioTask>(
            criteria,
            $"{ExpectedEndpoint}/search",
            null,
            cts.Token);
    }

    /// <summary>
    ///     Update calls <see cref="IBexioConnectionHandler.PostAsync{TResult,TUpdate}" /> (not PUT) at
    ///     <c>/2.0/task/{id}</c> — Bexio edits tasks via POST on this resource.
    /// </summary>
    [Test]
    public async Task Update_CallsPostAsync_WithIdInPath()
    {
        const int id = 42;
        var payload = BuildUpdatePayload();
        var response = new ApiResult<BexioTask> { IsSuccess = true };
        string? capturedPath = null;
        ConnectionHandler
            .PostAsync<BexioTask, TaskUpdate>(
                Arg.Any<TaskUpdate>(),
                Arg.Do<string>(path => capturedPath = path),
                Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.Update(id, payload);

        capturedPath.ShouldBe($"{ExpectedEndpoint}/{id}");
        await ConnectionHandler.Received(1).PostAsync<BexioTask, TaskUpdate>(
            payload,
            $"{ExpectedEndpoint}/{id}",
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    ///     Update returns the <see cref="ApiResult{T}" /> produced by the connection handler without modification.
    /// </summary>
    [Test]
    public async Task Update_ReturnsApiResultFromConnectionHandler()
    {
        var payload = BuildUpdatePayload();
        var response = new ApiResult<BexioTask> { IsSuccess = true };
        ConnectionHandler
            .PostAsync<BexioTask, TaskUpdate>(
                Arg.Any<TaskUpdate>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>())
            .Returns(response);

        var result = await _sut.Update(1, payload);

        result.ShouldBeSameAs(response);
    }

    /// <summary>
    ///     Update forwards the caller's cancellation token to the connection handler so cooperative
    ///     cancellation propagates end-to-end.
    /// </summary>
    [Test]
    public async Task Update_ForwardsCancellationTokenToConnectionHandler()
    {
        using var cts = new CancellationTokenSource();
        var payload = BuildUpdatePayload();
        ConnectionHandler
            .PostAsync<BexioTask, TaskUpdate>(Arg.Any<TaskUpdate>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new ApiResult<BexioTask> { IsSuccess = true });

        await _sut.Update(7, payload, cts.Token);

        await ConnectionHandler.Received(1).PostAsync<BexioTask, TaskUpdate>(
            payload,
            $"{ExpectedEndpoint}/7",
            cts.Token);
    }

    /// <summary>
    ///     Delete forwards the call to <see cref="IBexioConnectionHandler.Delete" /> with the task id
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

        capturedPath.ShouldBe($"{ExpectedEndpoint}/{id}");
    }

    /// <summary>
    ///     Delete returns the <see cref="ApiResult{T}" /> produced by the connection handler without modification.
    /// </summary>
    [Test]
    public async Task Delete_ReturnsApiResultFromConnectionHandler()
    {
        var response = new ApiResult<object> { IsSuccess = true };
        ConnectionHandler
            .Delete(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(response);

        var result = await _sut.Delete(1);

        result.ShouldBeSameAs(response);
    }

    /// <summary>
    ///     Delete forwards the caller's cancellation token to the connection handler so cooperative
    ///     cancellation propagates end-to-end.
    /// </summary>
    [Test]
    public async Task Delete_ForwardsCancellationTokenToConnectionHandler()
    {
        using var cts = new CancellationTokenSource();
        ConnectionHandler
            .Delete(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new ApiResult<object> { IsSuccess = true });

        await _sut.Delete(99, cts.Token);

        await ConnectionHandler.Received(1).Delete(
            $"{ExpectedEndpoint}/99",
            cts.Token);
    }

    private static TaskCreate BuildCreatePayload()
    {
        return new TaskCreate(
            1,
            "Send documents");
    }

    private static TaskUpdate BuildUpdatePayload()
    {
        return new TaskUpdate(
            1,
            "Send documents");
    }

    private static BexioTask BuildTask(int id)
    {
        return new BexioTask(
            id,
            1,
            null,
            $"Task {id}",
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            1,
            null,
            null,
            null,
            null,
            null);
    }
}
