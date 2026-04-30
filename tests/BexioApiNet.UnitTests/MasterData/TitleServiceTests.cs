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
using BexioApiNet.Abstractions.Models.MasterData.Titles;
using BexioApiNet.Abstractions.Models.MasterData.Titles.Views;
using BexioApiNet.Models;
using BexioApiNet.Services.Connectors.MasterData;

namespace BexioApiNet.UnitTests.MasterData;

/// <summary>
///     Offline unit tests for <see cref="TitleService" />. Each test verifies that the service
///     forwards its calls to <see cref="IBexioConnectionHandler" /> with the expected verb and path,
///     propagates the response unchanged, and honours the canonical <c>autoPage</c> + <c>X-Total-Count</c>
///     pagination contract. The Bexio Titles API uses <c>POST /2.0/title/{id}</c> for full
///     replacement edits — see <see href="https://docs.bexio.com/#tag/Titles/operation/v2EditTitle" />.
///     No network access.
/// </summary>
[TestFixture]
public sealed class TitleServiceTests : ServiceTestBase
{
    /// <summary>
    ///     Creates a fresh <see cref="TitleService" /> bound to the
    ///     <see cref="ServiceTestBase.ConnectionHandler" /> substitute for every test.
    /// </summary>
    [SetUp]
    public void CreateSut()
    {
        _sut = new TitleService(ConnectionHandler);
    }

    private const string ExpectedEndpoint = "2.0/title";

    private TitleService _sut = null!;

    /// <summary>
    ///     Get (no parameters) performs a single <c>GetAsync</c> against <c>2.0/title</c>
    ///     with a null query parameter and never triggers <c>FetchAll</c>.
    /// </summary>
    [Test]
    public async Task Get_WithNoParams_CallsGetAsyncOnceWithExpectedPathAndNullQuery()
    {
        var expected = new ApiResult<List<Title>?> { IsSuccess = true, Data = [] };
        ConnectionHandler
            .GetAsync<List<Title>?>(Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

        var result = await _sut.Get();

        Assert.That(result, Is.SameAs(expected));
        await ConnectionHandler.Received(1).GetAsync<List<Title>?>(
            ExpectedEndpoint,
            null,
            Arg.Any<CancellationToken>());
        await ConnectionHandler.DidNotReceive().FetchAll<Title>(
            Arg.Any<int>(),
            Arg.Any<int>(),
            Arg.Any<string>(),
            Arg.Any<QueryParameter?>(),
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    ///     When a <see cref="QueryParameterTitle" /> is supplied, its inner
    ///     <see cref="QueryParameter" /> instance is forwarded to the connection
    ///     handler verbatim.
    /// </summary>
    [Test]
    public async Task Get_WithQueryParameter_PassesQueryParameterToConnectionHandler()
    {
        var queryParameter = new QueryParameterTitle(50, 100);
        ConnectionHandler
            .GetAsync<List<Title>?>(Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new ApiResult<List<Title>?> { IsSuccess = true, Data = [] }));

        await _sut.Get(queryParameter);

        await ConnectionHandler.Received(1).GetAsync<List<Title>?>(
            ExpectedEndpoint,
            queryParameter.QueryParameter,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    ///     <see cref="QueryParameterTitle" /> emits the spec-supported pagination and ordering
    ///     parameters (<c>limit</c>, <c>offset</c>, <c>order_by</c>) and only includes a key
    ///     when the caller supplied a value.
    /// </summary>
    [Test]
    public void QueryParameterTitle_Serializes_AllSpecSupportedParameters()
    {
        var queryParameter = new QueryParameterTitle(limit: 25, offset: 50, orderBy: "name_desc");

        Assert.That(queryParameter.QueryParameter, Is.Not.Null);
        Assert.That(queryParameter.QueryParameter.Parameters.Keys,
            Is.EquivalentTo(new[] { "limit", "offset", "order_by" }));
        Assert.That(queryParameter.QueryParameter.Parameters["limit"], Is.EqualTo(25));
        Assert.That(queryParameter.QueryParameter.Parameters["offset"], Is.EqualTo(50));
        Assert.That(queryParameter.QueryParameter.Parameters["order_by"], Is.EqualTo("name_desc"));
    }

    /// <summary>
    ///     <see cref="QueryParameterTitle" /> does not include keys for parameters the caller
    ///     omitted — only supplied values reach the URL.
    /// </summary>
    [Test]
    public void QueryParameterTitle_OmitsUnsetParameters()
    {
        var queryParameter = new QueryParameterTitle(limit: 25);

        Assert.That(queryParameter.QueryParameter, Is.Not.Null);
        Assert.That(queryParameter.QueryParameter.Parameters.Keys, Is.EquivalentTo(new[] { "limit" }));
        Assert.That(queryParameter.QueryParameter.Parameters["limit"], Is.EqualTo(25));
    }

    /// <summary>
    ///     When <c>autoPage</c> is on and the first response advertises a
    ///     <c>X-Total-Count</c> header, the service calls <c>FetchAll</c> with the
    ///     count of already-fetched items, the total, the same path, and the
    ///     same query parameter.
    /// </summary>
    [Test]
    public async Task Get_WithAutoPage_WhenTotalResultsHeaderPresent_CallsFetchAll()
    {
        var firstTitle = NewTitle(1);
        var firstPage = new ApiResult<List<Title>?>
        {
            IsSuccess = true,
            Data = [firstTitle],
            ResponseHeaders = new Dictionary<string, int?>
            {
                [ApiHeaderNames.TotalResults] = 3
            }
        };
        ConnectionHandler
            .GetAsync<List<Title>?>(Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(firstPage));
        var remaining = new List<Title> { NewTitle(2), NewTitle(3) };
        ConnectionHandler
            .FetchAll<Title>(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>(), Arg.Any<QueryParameter?>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(remaining));

        var result = await _sut.Get(autoPage: true);

        await ConnectionHandler.Received(1).FetchAll<Title>(
            1,
            3,
            ExpectedEndpoint,
            null,
            Arg.Any<CancellationToken>());
        Assert.That(result.Data, Has.Count.EqualTo(3));
        Assert.That(result.Data, Is.EquivalentTo(new[] { firstTitle, remaining[0], remaining[1] }));
    }

    /// <summary>
    ///     When <c>autoPage</c> is requested but the response carries no
    ///     <c>X-Total-Count</c> header, the service does not invoke <c>FetchAll</c>.
    /// </summary>
    [Test]
    public async Task Get_WithAutoPage_WhenTotalResultsHeaderMissing_DoesNotCallFetchAll()
    {
        var response = new ApiResult<List<Title>?>
        {
            IsSuccess = true,
            Data = [NewTitle(1)],
            ResponseHeaders = new Dictionary<string, int?>()
        };
        ConnectionHandler
            .GetAsync<List<Title>?>(Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(response));

        var result = await _sut.Get(autoPage: true);

        Assert.That(result, Is.SameAs(response));
        await ConnectionHandler.DidNotReceive().FetchAll<Title>(
            Arg.Any<int>(),
            Arg.Any<int>(),
            Arg.Any<string>(),
            Arg.Any<QueryParameter?>(),
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    ///     GetById calls <see cref="IBexioConnectionHandler.GetAsync{TResult}" /> once
    ///     with the path <c>2.0/title/{id}</c> and a null query parameter.
    /// </summary>
    [Test]
    public async Task GetById_CallsGetAsyncWithIdInPath()
    {
        const int id = 42;
        var expected = new ApiResult<Title?>
        {
            IsSuccess = true,
            Data = NewTitle(id)
        };
        ConnectionHandler
            .GetAsync<Title?>(Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

        var result = await _sut.GetById(id);

        Assert.That(result, Is.SameAs(expected));
        await ConnectionHandler.Received(1).GetAsync<Title?>(
            $"{ExpectedEndpoint}/{id}",
            null,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    ///     Create forwards the create view to <see cref="IBexioConnectionHandler.PostAsync{TResult,TCreate}" />
    ///     against the endpoint root (no id suffix).
    /// </summary>
    [Test]
    public async Task Create_CallsPostAsyncWithEndpointRoot()
    {
        var payload = new TitleCreate("Dr.");
        var expected = new ApiResult<Title> { IsSuccess = true, Data = NewTitle(1) };
        ConnectionHandler
            .PostAsync<Title, TitleCreate>(
                Arg.Any<TitleCreate>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

        var result = await _sut.Create(payload);

        Assert.That(result, Is.SameAs(expected));
        await ConnectionHandler.Received(1).PostAsync<Title, TitleCreate>(
            payload,
            ExpectedEndpoint,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    ///     Search forwards the criteria list and optional pagination parameters to
    ///     <see cref="IBexioConnectionHandler.PostSearchAsync{TResult}" /> against
    ///     the <c>/search</c> sub-path.
    /// </summary>
    [Test]
    public async Task Search_CallsPostSearchAsyncWithSearchPath()
    {
        var criteria = new List<SearchCriteria>
        {
            new() { Field = "name", Value = "Dr.", Criteria = "like" }
        };
        var queryParameter = new QueryParameterTitle(10, 0);
        var expected = new ApiResult<List<Title>>
        {
            IsSuccess = true,
            Data = [NewTitle(1)]
        };
        ConnectionHandler
            .PostSearchAsync<Title>(
                Arg.Any<List<SearchCriteria>>(),
                Arg.Any<string>(),
                Arg.Any<QueryParameter?>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

        var result = await _sut.Search(criteria, queryParameter);

        Assert.That(result, Is.SameAs(expected));
        await ConnectionHandler.Received(1).PostSearchAsync<Title>(
            criteria,
            $"{ExpectedEndpoint}/search",
            queryParameter.QueryParameter,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    ///     Search without a query parameter passes <see langword="null" /> pagination to
    ///     the connection handler and still targets the <c>/search</c> sub-path.
    /// </summary>
    [Test]
    public async Task Search_WithoutQueryParameter_PassesNullPagination()
    {
        var criteria = new List<SearchCriteria>
        {
            new() { Field = "name", Value = "Dr.", Criteria = "=" }
        };
        ConnectionHandler
            .PostSearchAsync<Title>(
                Arg.Any<List<SearchCriteria>>(),
                Arg.Any<string>(),
                Arg.Any<QueryParameter?>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new ApiResult<List<Title>> { IsSuccess = true, Data = [] }));

        await _sut.Search(criteria);

        await ConnectionHandler.Received(1).PostSearchAsync<Title>(
            criteria,
            $"{ExpectedEndpoint}/search",
            null,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    ///     Update forwards the update view to <see cref="IBexioConnectionHandler.PostAsync{TResult,TCreate}" />
    ///     against the per-id sub-path. The Bexio Titles API uses <c>POST</c> (not <c>PUT</c>)
    ///     for full-replacement edits — see
    ///     <see href="https://docs.bexio.com/#tag/Titles/operation/v2EditTitle" />.
    /// </summary>
    [Test]
    public async Task Update_CallsPostAsyncWithIdInPath()
    {
        const int id = 7;
        var payload = new TitleUpdate("Prof.");
        var expected = new ApiResult<Title> { IsSuccess = true, Data = NewTitle(id) };
        ConnectionHandler
            .PostAsync<Title, TitleUpdate>(
                Arg.Any<TitleUpdate>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

        var result = await _sut.Update(id, payload);

        Assert.That(result, Is.SameAs(expected));
        await ConnectionHandler.Received(1).PostAsync<Title, TitleUpdate>(
            payload,
            $"{ExpectedEndpoint}/{id}",
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    ///     Delete forwards to <see cref="IBexioConnectionHandler.Delete" /> with the
    ///     per-id path exactly once.
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
    ///     The cancellation token supplied by the caller must be forwarded to the
    ///     connection handler on <c>Get</c> so cooperative cancellation flows end-to-end.
    /// </summary>
    [Test]
    public async Task Get_ForwardsCancellationTokenToConnectionHandler()
    {
        using var cts = new CancellationTokenSource();
        ConnectionHandler
            .GetAsync<List<Title>?>(Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new ApiResult<List<Title>?> { IsSuccess = true, Data = [] }));

        await _sut.Get(cancellationToken: cts.Token);

        await ConnectionHandler.Received(1).GetAsync<List<Title>?>(
            ExpectedEndpoint,
            null,
            cts.Token);
    }

    private static Title NewTitle(int id)
    {
        return new Title(id, $"Title {id}");
    }
}
