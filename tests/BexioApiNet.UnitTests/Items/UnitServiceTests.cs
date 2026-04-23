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
using BexioApiNet.Abstractions.Models.Items.Units;
using BexioApiNet.Abstractions.Models.Items.Units.Views;
using BexioApiNet.Models;
using BexioApiNet.Services.Connectors.Items;

namespace BexioApiNet.UnitTests.Items;

/// <summary>
/// Offline unit tests for <see cref="UnitService"/>. Each test verifies that the service
/// forwards its calls to <see cref="IBexioConnectionHandler"/> with the expected verb and path,
/// propagates the response unchanged, and honours the canonical <c>autoPage</c> + <c>X-Total-Count</c>
/// pagination contract. No network access.
/// </summary>
[TestFixture]
public sealed class UnitServiceTests : ServiceTestBase
{
    /// <summary>
    /// Creates a fresh <see cref="UnitService"/> bound to the
    /// <see cref="ServiceTestBase.ConnectionHandler"/> substitute for every test.
    /// </summary>
    [SetUp]
    public void CreateSut()
    {
        _sut = new UnitService(ConnectionHandler);
    }

    private const string ExpectedEndpoint = "2.0/unit";

    private UnitService _sut = null!;

    /// <summary>
    /// Get (no parameters) performs a single <c>GetAsync</c> against <c>2.0/unit</c>
    /// with a null query parameter and never triggers <c>FetchAll</c>.
    /// </summary>
    [Test]
    public async Task Get_WithNoParams_CallsGetAsyncOnceWithExpectedPathAndNullQuery()
    {
        var expected = new ApiResult<List<Unit>?> { IsSuccess = true, Data = [] };
        ConnectionHandler
            .GetAsync<List<Unit>?>(Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

        var result = await _sut.Get();

        Assert.That(result, Is.SameAs(expected));
        await ConnectionHandler.Received(1).GetAsync<List<Unit>?>(
            ExpectedEndpoint,
            null,
            Arg.Any<CancellationToken>());
        await ConnectionHandler.DidNotReceive().FetchAll<Unit>(
            Arg.Any<int>(),
            Arg.Any<int>(),
            Arg.Any<string>(),
            Arg.Any<QueryParameter?>(),
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// When a <see cref="QueryParameterUnit"/> is supplied, its inner
    /// <see cref="QueryParameter"/> instance is forwarded to the connection
    /// handler verbatim.
    /// </summary>
    [Test]
    public async Task Get_WithQueryParameter_PassesQueryParameterToConnectionHandler()
    {
        var queryParameter = new QueryParameterUnit(Limit: 50, Offset: 100);
        ConnectionHandler
            .GetAsync<List<Unit>?>(Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new ApiResult<List<Unit>?> { IsSuccess = true, Data = [] }));

        await _sut.Get(queryParameter);

        await ConnectionHandler.Received(1).GetAsync<List<Unit>?>(
            ExpectedEndpoint,
            queryParameter.QueryParameter,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// When <c>autoPage</c> is on and the first response advertises a
    /// <c>X-Total-Count</c> header, the service calls <c>FetchAll</c> with the
    /// count of already-fetched items, the total, the same path, and the
    /// same query parameter.
    /// </summary>
    [Test]
    public async Task Get_WithAutoPage_WhenTotalResultsHeaderPresent_CallsFetchAll()
    {
        var firstUnit = NewUnit(1);
        var firstPage = new ApiResult<List<Unit>?>
        {
            IsSuccess = true,
            Data = [firstUnit],
            ResponseHeaders = new Dictionary<string, int?>
            {
                [ApiHeaderNames.TotalResults] = 3
            }
        };
        ConnectionHandler
            .GetAsync<List<Unit>?>(Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(firstPage));
        var remaining = new List<Unit> { NewUnit(2), NewUnit(3) };
        ConnectionHandler
            .FetchAll<Unit>(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(remaining));

        var result = await _sut.Get(autoPage: true);

        await ConnectionHandler.Received(1).FetchAll<Unit>(
            1,
            3,
            ExpectedEndpoint,
            null,
            Arg.Any<CancellationToken>());
        Assert.That(result.Data, Has.Count.EqualTo(3));
        Assert.That(result.Data, Is.EquivalentTo(new[] { firstUnit, remaining[0], remaining[1] }));
    }

    /// <summary>
    /// When <c>autoPage</c> is requested but the response carries no
    /// <c>X-Total-Count</c> header, the service does not invoke <c>FetchAll</c>.
    /// </summary>
    [Test]
    public async Task Get_WithAutoPage_WhenTotalResultsHeaderMissing_DoesNotCallFetchAll()
    {
        var response = new ApiResult<List<Unit>?>
        {
            IsSuccess = true,
            Data = [NewUnit(1)],
            ResponseHeaders = new Dictionary<string, int?>()
        };
        ConnectionHandler
            .GetAsync<List<Unit>?>(Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(response));

        var result = await _sut.Get(autoPage: true);

        Assert.That(result, Is.SameAs(response));
        await ConnectionHandler.DidNotReceive().FetchAll<Unit>(
            Arg.Any<int>(),
            Arg.Any<int>(),
            Arg.Any<string>(),
            Arg.Any<QueryParameter?>(),
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// GetById calls <see cref="IBexioConnectionHandler.GetAsync{TResult}"/> once
    /// with the path <c>2.0/unit/{id}</c> and a null query parameter.
    /// </summary>
    [Test]
    public async Task GetById_CallsGetAsyncWithIdInPath()
    {
        const int id = 42;
        var expected = new ApiResult<Unit?>
        {
            IsSuccess = true,
            Data = NewUnit(id)
        };
        ConnectionHandler
            .GetAsync<Unit?>(Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

        var result = await _sut.GetById(id);

        Assert.That(result, Is.SameAs(expected));
        await ConnectionHandler.Received(1).GetAsync<Unit?>(
            $"{ExpectedEndpoint}/{id}",
            null,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Create forwards the create view to <see cref="IBexioConnectionHandler.PostAsync{TResult,TCreate}"/>
    /// against the endpoint root (no id suffix).
    /// </summary>
    [Test]
    public async Task Create_CallsPostAsyncWithEndpointRoot()
    {
        var payload = new UnitCreate("kg");
        var expected = new ApiResult<Unit> { IsSuccess = true, Data = NewUnit(1) };
        ConnectionHandler
            .PostAsync<Unit, UnitCreate>(
                Arg.Any<UnitCreate>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

        var result = await _sut.Create(payload);

        Assert.That(result, Is.SameAs(expected));
        await ConnectionHandler.Received(1).PostAsync<Unit, UnitCreate>(
            payload,
            ExpectedEndpoint,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Search forwards the criteria list and optional pagination parameters to
    /// <see cref="IBexioConnectionHandler.PostSearchAsync{TResult}"/> against
    /// the <c>/search</c> sub-path.
    /// </summary>
    [Test]
    public async Task Search_CallsPostSearchAsyncWithSearchPath()
    {
        var criteria = new List<SearchCriteria>
        {
            new() { Field = "name", Value = "kg", Criteria = "like" }
        };
        var queryParameter = new QueryParameterUnit(Limit: 10, Offset: 0);
        var expected = new ApiResult<List<Unit>>
        {
            IsSuccess = true,
            Data = [NewUnit(1)]
        };
        ConnectionHandler
            .PostSearchAsync<Unit>(
                Arg.Any<List<SearchCriteria>>(),
                Arg.Any<string>(),
                Arg.Any<QueryParameter?>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

        var result = await _sut.Search(criteria, queryParameter);

        Assert.That(result, Is.SameAs(expected));
        await ConnectionHandler.Received(1).PostSearchAsync<Unit>(
            criteria,
            $"{ExpectedEndpoint}/search",
            queryParameter.QueryParameter,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Search without a query parameter passes <see langword="null"/> pagination to
    /// the connection handler and still targets the <c>/search</c> sub-path.
    /// </summary>
    [Test]
    public async Task Search_WithoutQueryParameter_PassesNullPagination()
    {
        var criteria = new List<SearchCriteria>
        {
            new() { Field = "name", Value = "kg", Criteria = "=" }
        };
        ConnectionHandler
            .PostSearchAsync<Unit>(
                Arg.Any<List<SearchCriteria>>(),
                Arg.Any<string>(),
                Arg.Any<QueryParameter?>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new ApiResult<List<Unit>> { IsSuccess = true, Data = [] }));

        await _sut.Search(criteria);

        await ConnectionHandler.Received(1).PostSearchAsync<Unit>(
            criteria,
            $"{ExpectedEndpoint}/search",
            null,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Update forwards the update view to <see cref="IBexioConnectionHandler.PutAsync{TResult,TUpdate}"/>
    /// against the per-id sub-path (the Bexio Units API uses PUT for full-replacement edits).
    /// </summary>
    [Test]
    public async Task Update_CallsPutAsyncWithIdInPath()
    {
        const int id = 7;
        var payload = new UnitUpdate("kilogram");
        var expected = new ApiResult<Unit> { IsSuccess = true, Data = NewUnit(id) };
        ConnectionHandler
            .PutAsync<Unit, UnitUpdate>(
                Arg.Any<UnitUpdate>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

        var result = await _sut.Update(id, payload);

        Assert.That(result, Is.SameAs(expected));
        await ConnectionHandler.Received(1).PutAsync<Unit, UnitUpdate>(
            payload,
            $"{ExpectedEndpoint}/{id}",
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Delete forwards to <see cref="IBexioConnectionHandler.Delete"/> with the
    /// per-id path exactly once.
    /// </summary>
    [Test]
    public async Task Delete_CallsConnectionHandlerDeleteWithIdInPath()
    {
        const int id = 42;
        var expected = new ApiResult<object> { IsSuccess = true };
        ConnectionHandler
            .Delete(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

        var result = await _sut.Delete(id);

        Assert.That(result, Is.SameAs(expected));
        await ConnectionHandler.Received(1).Delete(
            $"{ExpectedEndpoint}/{id}",
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// The cancellation token supplied by the caller must be forwarded to the
    /// connection handler on <c>Get</c> so cooperative cancellation flows end-to-end.
    /// </summary>
    [Test]
    public async Task Get_ForwardsCancellationTokenToConnectionHandler()
    {
        using var cts = new CancellationTokenSource();
        ConnectionHandler
            .GetAsync<List<Unit>?>(Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new ApiResult<List<Unit>?> { IsSuccess = true, Data = [] }));

        await _sut.Get(cancellationToken: cts.Token);

        await ConnectionHandler.Received(1).GetAsync<List<Unit>?>(
            ExpectedEndpoint,
            null,
            cts.Token);
    }

    private static Unit NewUnit(int id) => new(Id: id, Name: $"Unit {id}");
}
