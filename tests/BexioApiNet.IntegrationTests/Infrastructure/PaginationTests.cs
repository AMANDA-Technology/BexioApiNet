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

using System.Text.Json.Serialization;
using BexioApiNet.Models;

namespace BexioApiNet.IntegrationTests.Infrastructure;

/// <summary>
/// Integration tests covering the pagination behavior of
/// <see cref="BexioConnectionHandler.FetchAll{TResult}"/> and the query-string
/// composition of <see cref="BexioConnectionHandler.GetAsync{TResult}"/>. Tests
/// exercise the real connection handler against WireMock so URL construction and
/// loop termination are verified end-to-end.
/// </summary>
public sealed class PaginationTests : IntegrationTestBase
{
    private const string AccountsPath = "/2.0/accounts";
    private const string AccountsRequestPath = "2.0/accounts";

    /// <summary>
    /// Small payload record deserialized from WireMock stub responses. Mirrors the
    /// minimal shape of a paginated item so tests do not need the full domain model.
    /// </summary>
    private sealed record PagingItem([property: JsonPropertyName("id")] int Id);

    /// <summary>
    /// When the caller already has all objects (<c>fetchedObjects == maxObjects</c>),
    /// the loop body never executes, no HTTP calls are made and an empty list is returned.
    /// </summary>
    [Test]
    public async Task FetchAll_WhenAllDataFetched_InSinglePage_ReturnsCorrectCount()
    {
        var result = await ConnectionHandler.FetchAll<PagingItem>(
            fetchedObjects: 3,
            maxObjects: 3,
            requestPath: AccountsRequestPath,
            queryParameter: null,
            cancellationToken: TestContext.CurrentContext.CancellationToken);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Empty);
            Assert.That(Server.LogEntries, Is.Empty, "no HTTP request should be issued");
        });
    }

    /// <summary>
    /// When <c>fetchedObjects &lt; maxObjects</c>, <c>FetchAll</c> issues sequential GET
    /// requests until the cumulative count meets the target. Each iteration updates the
    /// <c>offset</c> on the supplied query parameter so the next request fetches the next page.
    /// </summary>
    [Test]
    public async Task FetchAll_WhenRemainingPages_FetchesUntilDone()
    {
        Server
            .Given(Request.Create().WithPath(AccountsPath).UsingGet().WithParam("offset", "0"))
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("""[{"id":1},{"id":2}]"""));

        Server
            .Given(Request.Create().WithPath(AccountsPath).UsingGet().WithParam("offset", "2"))
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("""[{"id":3},{"id":4}]"""));

        var queryParam = new QueryParameter(new Dictionary<string, object> { ["offset"] = 0 });

        var result = await ConnectionHandler.FetchAll<PagingItem>(
            fetchedObjects: 0,
            maxObjects: 4,
            requestPath: AccountsRequestPath,
            queryParameter: queryParam,
            cancellationToken: TestContext.CurrentContext.CancellationToken);

        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Count.EqualTo(4));
            Assert.That(result.Select(x => x.Id), Is.EqualTo(new[] { 1, 2, 3, 4 }));
            Assert.That(Server.LogEntries.Count(), Is.EqualTo(2));
        });
    }

    /// <summary>
    /// When the server returns a non-success status code mid-pagination, <c>FetchAll</c>
    /// surfaces the failure as an <see cref="InvalidOperationException"/> so callers do not
    /// silently receive partial results.
    /// </summary>
    [Test]
    public void FetchAll_WhenPageFails_ThrowsInvalidOperationException()
    {
        Server
            .Given(Request.Create().WithPath(AccountsPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(400).WithBody("""{"error":"bad request"}"""));

        var queryParam = new QueryParameter(new Dictionary<string, object> { ["offset"] = 0 });

        var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await ConnectionHandler.FetchAll<PagingItem>(
                fetchedObjects: 0,
                maxObjects: 5,
                requestPath: AccountsRequestPath,
                queryParameter: queryParam,
                cancellationToken: TestContext.CurrentContext.CancellationToken));

        Assert.That(ex!.Message, Does.Contain("Paging failed"));
    }

    /// <summary>
    /// <see cref="BexioConnectionHandler.GetAsync{TResult}"/> must append every entry from
    /// <see cref="QueryParameter.Parameters"/> to the outgoing URI so paginated calls reach
    /// the correct page on the server.
    /// </summary>
    [Test]
    public async Task GetAsync_WithPaginationParams_AppendsOffsetAndLimitToUri()
    {
        Server
            .Given(Request.Create().WithPath(AccountsPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("[]"));

        var queryParam = new QueryParameter(new Dictionary<string, object>
        {
            ["offset"] = 10,
            ["limit"] = 50
        });

        var result = await ConnectionHandler.GetAsync<List<PagingItem>>(
            AccountsRequestPath,
            queryParam,
            TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(AccountsPath));
            Assert.That(request.RawQuery, Does.Contain("offset=10"));
            Assert.That(request.RawQuery, Does.Contain("limit=50"));
        });
    }

    /// <summary>
    /// When the caller supplies an initial <c>offset</c>, <c>FetchAll</c> subtracts it from
    /// <paramref name="maxObjects"/> and preserves it across iterations so subsequent requests
    /// stay aligned with the caller's starting point.
    /// </summary>
    [Test]
    public async Task FetchAll_WithInitialOffset_AccountsForOffset()
    {
        Server
            .Given(Request.Create().WithPath(AccountsPath).UsingGet().WithParam("offset", "5"))
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("""[{"id":6},{"id":7},{"id":8}]"""));

        Server
            .Given(Request.Create().WithPath(AccountsPath).UsingGet().WithParam("offset", "8"))
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("""[{"id":9},{"id":10}]"""));

        var queryParam = new QueryParameter(new Dictionary<string, object> { ["offset"] = 5 });

        var result = await ConnectionHandler.FetchAll<PagingItem>(
            fetchedObjects: 0,
            maxObjects: 10,
            requestPath: AccountsRequestPath,
            queryParameter: queryParam,
            cancellationToken: TestContext.CurrentContext.CancellationToken);

        var requests = Server.LogEntries.Select(e => e.RequestMessage!.RawQuery).ToList();

        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Count.EqualTo(5));
            Assert.That(requests, Has.Count.EqualTo(2));
            Assert.That(requests[0], Does.Contain("offset=5"));
            Assert.That(requests[1], Does.Contain("offset=8"));
            Assert.That(queryParam.Parameters["offset"], Is.EqualTo(10));
        });
    }
}
