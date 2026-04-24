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
using BexioApiNet.Abstractions.Models.MasterData.Countries;
using BexioApiNet.Abstractions.Models.MasterData.Countries.Views;
using BexioApiNet.Models;
using BexioApiNet.Services.Connectors.MasterData;

namespace BexioApiNet.UnitTests.MasterData;

/// <summary>
/// Offline unit tests for <see cref="CountryService"/>. Each test verifies that the service
/// forwards its calls to <see cref="IBexioConnectionHandler"/> with the expected verb and path,
/// propagates the response unchanged, and honours the canonical <c>autoPage</c> + <c>X-Total-Count</c>
/// pagination contract. No network access.
/// </summary>
[TestFixture]
public sealed class CountryServiceTests : ServiceTestBase
{
    /// <summary>
    /// Creates a fresh <see cref="CountryService"/> bound to the
    /// <see cref="ServiceTestBase.ConnectionHandler"/> substitute for every test.
    /// </summary>
    [SetUp]
    public void CreateSut()
    {
        _sut = new CountryService(ConnectionHandler);
    }

    private const string ExpectedEndpoint = "2.0/country";

    private CountryService _sut = null!;

    /// <summary>
    /// Get (no parameters) performs a single <c>GetAsync</c> against <c>2.0/country</c>
    /// with a null query parameter and never triggers <c>FetchAll</c>.
    /// </summary>
    [Test]
    public async Task Get_WithNoParams_CallsGetAsyncOnceWithExpectedPathAndNullQuery()
    {
        var expected = new ApiResult<List<Country>?> { IsSuccess = true, Data = [] };
        ConnectionHandler
            .GetAsync<List<Country>?>(Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

        var result = await _sut.Get();

        Assert.That(result, Is.SameAs(expected));
        await ConnectionHandler.Received(1).GetAsync<List<Country>?>(
            ExpectedEndpoint,
            null,
            Arg.Any<CancellationToken>());
        await ConnectionHandler.DidNotReceive().FetchAll<Country>(
            Arg.Any<int>(),
            Arg.Any<int>(),
            Arg.Any<string>(),
            Arg.Any<QueryParameter?>(),
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// When a <see cref="QueryParameterCountry"/> is supplied, its inner
    /// <see cref="QueryParameter"/> instance is forwarded to the connection
    /// handler verbatim.
    /// </summary>
    [Test]
    public async Task Get_WithQueryParameter_PassesQueryParameterToConnectionHandler()
    {
        var queryParameter = new QueryParameterCountry(Limit: 50, Offset: 100);
        ConnectionHandler
            .GetAsync<List<Country>?>(Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new ApiResult<List<Country>?> { IsSuccess = true, Data = [] }));

        await _sut.Get(queryParameter);

        await ConnectionHandler.Received(1).GetAsync<List<Country>?>(
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
        var firstCountry = NewCountry(1);
        var firstPage = new ApiResult<List<Country>?>
        {
            IsSuccess = true,
            Data = [firstCountry],
            ResponseHeaders = new Dictionary<string, int?>
            {
                [ApiHeaderNames.TotalResults] = 3
            }
        };
        ConnectionHandler
            .GetAsync<List<Country>?>(Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(firstPage));
        var remaining = new List<Country> { NewCountry(2), NewCountry(3) };
        ConnectionHandler
            .FetchAll<Country>(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(remaining));

        var result = await _sut.Get(autoPage: true);

        await ConnectionHandler.Received(1).FetchAll<Country>(
            1,
            3,
            ExpectedEndpoint,
            null,
            Arg.Any<CancellationToken>());
        Assert.That(result.Data, Has.Count.EqualTo(3));
        Assert.That(result.Data, Is.EquivalentTo(new[] { firstCountry, remaining[0], remaining[1] }));
    }

    /// <summary>
    /// When <c>autoPage</c> is requested but the response carries no
    /// <c>X-Total-Count</c> header, the service does not invoke <c>FetchAll</c>.
    /// </summary>
    [Test]
    public async Task Get_WithAutoPage_WhenTotalResultsHeaderMissing_DoesNotCallFetchAll()
    {
        var response = new ApiResult<List<Country>?>
        {
            IsSuccess = true,
            Data = [NewCountry(1)],
            ResponseHeaders = new Dictionary<string, int?>()
        };
        ConnectionHandler
            .GetAsync<List<Country>?>(Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(response));

        var result = await _sut.Get(autoPage: true);

        Assert.That(result, Is.SameAs(response));
        await ConnectionHandler.DidNotReceive().FetchAll<Country>(
            Arg.Any<int>(),
            Arg.Any<int>(),
            Arg.Any<string>(),
            Arg.Any<QueryParameter?>(),
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// GetById calls <see cref="IBexioConnectionHandler.GetAsync{TResult}"/> once
    /// with the path <c>2.0/country/{id}</c> and a null query parameter.
    /// </summary>
    [Test]
    public async Task GetById_CallsGetAsyncWithIdInPath()
    {
        const int id = 42;
        var expected = new ApiResult<Country?>
        {
            IsSuccess = true,
            Data = NewCountry(id)
        };
        ConnectionHandler
            .GetAsync<Country?>(Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

        var result = await _sut.GetById(id);

        Assert.That(result, Is.SameAs(expected));
        await ConnectionHandler.Received(1).GetAsync<Country?>(
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
        var payload = new CountryCreate("Kiribati", "KI", "KI");
        var expected = new ApiResult<Country> { IsSuccess = true, Data = NewCountry(1) };
        ConnectionHandler
            .PostAsync<Country, CountryCreate>(
                Arg.Any<CountryCreate>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

        var result = await _sut.Create(payload);

        Assert.That(result, Is.SameAs(expected));
        await ConnectionHandler.Received(1).PostAsync<Country, CountryCreate>(
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
            new() { Field = "name", Value = "Switzerland", Criteria = "like" }
        };
        var queryParameter = new QueryParameterCountry(Limit: 10, Offset: 0);
        var expected = new ApiResult<List<Country>>
        {
            IsSuccess = true,
            Data = [NewCountry(1)]
        };
        ConnectionHandler
            .PostSearchAsync<Country>(
                Arg.Any<List<SearchCriteria>>(),
                Arg.Any<string>(),
                Arg.Any<QueryParameter?>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

        var result = await _sut.Search(criteria, queryParameter);

        Assert.That(result, Is.SameAs(expected));
        await ConnectionHandler.Received(1).PostSearchAsync<Country>(
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
            new() { Field = "name_short", Value = "CH", Criteria = "=" }
        };
        ConnectionHandler
            .PostSearchAsync<Country>(
                Arg.Any<List<SearchCriteria>>(),
                Arg.Any<string>(),
                Arg.Any<QueryParameter?>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new ApiResult<List<Country>> { IsSuccess = true, Data = [] }));

        await _sut.Search(criteria);

        await ConnectionHandler.Received(1).PostSearchAsync<Country>(
            criteria,
            $"{ExpectedEndpoint}/search",
            null,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Update forwards the update view to <see cref="IBexioConnectionHandler.PutAsync{TResult,TUpdate}"/>
    /// against the per-id sub-path (Update uses PUT per the epic blueprint).
    /// </summary>
    [Test]
    public async Task Update_CallsPutAsyncWithIdInPath()
    {
        const int id = 7;
        var payload = new CountryUpdate("Kiribati", "KIR", "KI");
        var expected = new ApiResult<Country> { IsSuccess = true, Data = NewCountry(id) };
        ConnectionHandler
            .PutAsync<Country, CountryUpdate>(
                Arg.Any<CountryUpdate>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

        var result = await _sut.Update(id, payload);

        Assert.That(result, Is.SameAs(expected));
        await ConnectionHandler.Received(1).PutAsync<Country, CountryUpdate>(
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
            .GetAsync<List<Country>?>(Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new ApiResult<List<Country>?> { IsSuccess = true, Data = [] }));

        await _sut.Get(cancellationToken: cts.Token);

        await ConnectionHandler.Received(1).GetAsync<List<Country>?>(
            ExpectedEndpoint,
            null,
            cts.Token);
    }

    private static Country NewCountry(int id) =>
        new(Id: id, Name: $"Country {id}", NameShort: $"C{id}", Iso3166Alpha2: "CH");
}
