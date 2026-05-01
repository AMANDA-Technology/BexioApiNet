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
using BexioApiNet.Abstractions.Models.Projects.Project;
using BexioApiNet.Abstractions.Models.Projects.Project.Views;
using BexioApiNet.Models;
using BexioApiNet.Services.Connectors.Projects;

namespace BexioApiNet.UnitTests.Projects;

/// <summary>
///     Offline unit tests for <see cref="ProjectService" />. Each test verifies that the service
///     forwards its calls to <see cref="IBexioConnectionHandler" /> with the expected arguments and
///     returns the handler's result unchanged. No network, no filesystem access.
/// </summary>
[TestFixture]
public sealed class ProjectServiceTests : ServiceTestBase
{
    /// <summary>
    ///     Creates a fresh <see cref="ProjectService" /> per test, bound to the
    ///     <see cref="ServiceTestBase.ConnectionHandler" /> substitute provided by the base fixture.
    /// </summary>
    [SetUp]
    public void CreateSut()
    {
        _sut = new ProjectService(ConnectionHandler);
    }

    private const string ExpectedEndpoint = "2.0/pr_project";

    private ProjectService _sut = null!;

    /// <summary>
    ///     Get (no parameters) calls <see cref="IBexioConnectionHandler.GetAsync{TResult}" /> once with
    ///     the expected endpoint path and a null query parameter.
    /// </summary>
    [Test]
    public async Task Get_WithNoParams_CallsGetAsync()
    {
        var response = new ApiResult<List<Project>?> { IsSuccess = true, Data = [] };
        ConnectionHandler
            .GetAsync<List<Project>?>(
                Arg.Any<string>(),
                Arg.Any<QueryParameter?>(),
                Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.Get();

        await ConnectionHandler.Received(1).GetAsync<List<Project>?>(
            ExpectedEndpoint,
            null,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    ///     Get forwards the <see cref="QueryParameterProject" />'s underlying <see cref="QueryParameter" />
    ///     to the connection handler so the caller's filters (limit/offset/order_by) reach the API.
    /// </summary>
    [Test]
    public async Task Get_WithQueryParameter_PassesQueryParameterToConnectionHandler()
    {
        var queryParameter = new QueryParameterProject(100, 50);
        var response = new ApiResult<List<Project>?> { IsSuccess = true, Data = [] };
        ConnectionHandler
            .GetAsync<List<Project>?>(
                Arg.Any<string>(),
                Arg.Any<QueryParameter?>(),
                Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.Get(queryParameter);

        await ConnectionHandler.Received(1).GetAsync<List<Project>?>(
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
        var initialData = new List<Project> { BuildProject(1), BuildProject(2) };
        var initial = new ApiResult<List<Project>?>
        {
            IsSuccess = true,
            Data = initialData,
            ResponseHeaders = new Dictionary<string, int?>
            {
                { ApiHeaderNames.TotalResults, totalResults }
            }
        };
        ConnectionHandler
            .GetAsync<List<Project>?>(
                Arg.Any<string>(),
                Arg.Any<QueryParameter?>(),
                Arg.Any<CancellationToken>())
            .Returns(initial);
        ConnectionHandler
            .FetchAll<Project>(
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Any<string>(),
                Arg.Any<QueryParameter?>(),
                Arg.Any<CancellationToken>())
            .Returns([]);

        await _sut.Get(autoPage: true);

        await ConnectionHandler.Received(1).FetchAll<Project>(
            initialData.Count,
            totalResults,
            ExpectedEndpoint,
            null,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    ///     Get returns the <see cref="ApiResult{T}" /> produced by the connection handler when
    ///     auto-paging is not requested (no additional FetchAll round-trip, result passes through).
    /// </summary>
    [Test]
    public async Task Get_ReturnsApiResult()
    {
        var response = new ApiResult<List<Project>?> { IsSuccess = true, Data = [] };
        ConnectionHandler
            .GetAsync<List<Project>?>(
                Arg.Any<string>(),
                Arg.Any<QueryParameter?>(),
                Arg.Any<CancellationToken>())
            .Returns(response);

        var result = await _sut.Get();

        result.ShouldBeSameAs(response);
    }

    /// <summary>
    ///     GetById calls <see cref="IBexioConnectionHandler.GetAsync{TResult}" /> with the expected
    ///     endpoint path including the project id.
    /// </summary>
    [Test]
    public async Task GetById_CallsGetAsync_WithIdInPath()
    {
        const int id = 42;
        var response = new ApiResult<Project> { IsSuccess = true };
        string? capturedPath = null;
        ConnectionHandler
            .GetAsync<Project>(
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
        var response = new ApiResult<Project> { IsSuccess = true };
        ConnectionHandler
            .GetAsync<Project>(
                Arg.Any<string>(),
                Arg.Any<QueryParameter?>(),
                Arg.Any<CancellationToken>())
            .Returns(response);

        var result = await _sut.GetById(1);

        result.ShouldBeSameAs(response);
    }

    /// <summary>
    ///     Create forwards the payload and the expected endpoint path to
    ///     <see cref="IBexioConnectionHandler.PostAsync{TResult,TCreate}" />.
    /// </summary>
    [Test]
    public async Task Create_CallsPostAsync()
    {
        var payload = BuildCreatePayload();
        var response = new ApiResult<Project> { IsSuccess = true };
        ConnectionHandler
            .PostAsync<Project, ProjectCreate>(
                Arg.Any<ProjectCreate>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.Create(payload);

        await ConnectionHandler.Received(1).PostAsync<Project, ProjectCreate>(
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
        var response = new ApiResult<Project> { IsSuccess = true };
        ConnectionHandler
            .PostAsync<Project, ProjectCreate>(
                Arg.Any<ProjectCreate>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>())
            .Returns(response);

        var result = await _sut.Create(payload);

        result.ShouldBeSameAs(response);
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
            new() { Field = "name", Value = "Amanda", Criteria = "like" }
        };
        var queryParameter = new QueryParameterProject(50);
        var response = new ApiResult<List<Project>> { IsSuccess = true, Data = [] };
        ConnectionHandler
            .PostSearchAsync<Project>(
                Arg.Any<List<SearchCriteria>>(),
                Arg.Any<string>(),
                Arg.Any<QueryParameter?>(),
                Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.Search(criteria, queryParameter);

        await ConnectionHandler.Received(1).PostSearchAsync<Project>(
            criteria,
            $"{ExpectedEndpoint}/search",
            queryParameter.QueryParameter,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    ///     Search returns the <see cref="ApiResult{T}" /> produced by the connection handler without modification.
    /// </summary>
    [Test]
    public async Task Search_ReturnsApiResultFromConnectionHandler()
    {
        var criteria = new List<SearchCriteria>
        {
            new() { Field = "name", Value = "Amanda", Criteria = "like" }
        };
        var response = new ApiResult<List<Project>> { IsSuccess = true, Data = [] };
        ConnectionHandler
            .PostSearchAsync<Project>(
                Arg.Any<List<SearchCriteria>>(),
                Arg.Any<string>(),
                Arg.Any<QueryParameter?>(),
                Arg.Any<CancellationToken>())
            .Returns(response);

        var result = await _sut.Search(criteria);

        result.ShouldBeSameAs(response);
    }

    /// <summary>
    ///     Update calls <see cref="IBexioConnectionHandler.PostAsync{TResult,TUpdate}" /> (not PUT) at
    ///     <c>/2.0/pr_project/{id}</c> — Bexio edits projects via POST on this resource.
    /// </summary>
    [Test]
    public async Task Update_CallsPostAsync_WithIdInPath()
    {
        const int id = 42;
        var payload = BuildUpdatePayload();
        var response = new ApiResult<Project> { IsSuccess = true };
        string? capturedPath = null;
        ConnectionHandler
            .PostAsync<Project, ProjectUpdate>(
                Arg.Any<ProjectUpdate>(),
                Arg.Do<string>(path => capturedPath = path),
                Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.Update(id, payload);

        capturedPath.ShouldBe($"{ExpectedEndpoint}/{id}");
        await ConnectionHandler.Received(1).PostAsync<Project, ProjectUpdate>(
            payload,
            $"{ExpectedEndpoint}/{id}",
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    ///     Archive posts to the <c>/{id}/archive</c> action endpoint with no request body.
    /// </summary>
    [Test]
    public async Task Archive_CallsPostActionAsync_WithArchivePath()
    {
        const int id = 42;
        var response = new ApiResult<object> { IsSuccess = true };
        string? capturedPath = null;
        ConnectionHandler
            .PostActionAsync(
                Arg.Do<string>(path => capturedPath = path),
                Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.Archive(id);

        capturedPath.ShouldBe($"{ExpectedEndpoint}/{id}/archive");
    }

    /// <summary>
    ///     Reactivate posts to the <c>/{id}/reactivate</c> action endpoint with no request body.
    /// </summary>
    [Test]
    public async Task Reactivate_CallsPostActionAsync_WithReactivatePath()
    {
        const int id = 42;
        var response = new ApiResult<object> { IsSuccess = true };
        string? capturedPath = null;
        ConnectionHandler
            .PostActionAsync(
                Arg.Do<string>(path => capturedPath = path),
                Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.Reactivate(id);

        capturedPath.ShouldBe($"{ExpectedEndpoint}/{id}/reactivate");
    }

    /// <summary>
    ///     Delete forwards the call to <see cref="IBexioConnectionHandler.Delete" /> with the project
    ///     id appended to the endpoint root.
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

    private static ProjectCreate BuildCreatePayload()
    {
        return new ProjectCreate(
            "New Project",
            1,
            1,
            1,
            1);
    }

    private static ProjectUpdate BuildUpdatePayload()
    {
        return new ProjectUpdate(
            "Updated Project",
            1,
            1,
            1,
            1);
    }

    private static Project BuildProject(int id)
    {
        return new Project(
            id,
            null,
            null,
            $"Project {id}",
            null,
            null,
            null,
            1,
            1,
            1,
            null,
            null,
            null,
            null,
            null,
            1);
    }
}
