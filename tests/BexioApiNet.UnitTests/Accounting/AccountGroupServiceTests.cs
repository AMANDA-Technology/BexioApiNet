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
using BexioApiNet.Abstractions.Models.Accounting.AccountGroups;
using BexioApiNet.Abstractions.Models.Api;
using BexioApiNet.Models;
using BexioApiNet.Services.Connectors.Accounting;

namespace BexioApiNet.UnitTests.Accounting;

/// <summary>
/// Offline unit tests for <see cref="AccountGroupService"/>. Verify the connector
/// builds the right request path, forwards <see cref="QueryParameter"/>s, and
/// triggers <c>FetchAll</c> only when <c>autoPage</c> is requested and the
/// response actually carries a <c>X-Total-Count</c> header.
/// </summary>
[TestFixture]
public sealed class AccountGroupServiceTests : ServiceTestBase
{
    private const string ExpectedPath = "2.0/account_groups";

    /// <summary>
    /// With no parameters, the service forwards a single <c>GetAsync</c> call to
    /// <c>2.0/account_groups</c> with a <see langword="null"/> query parameter and never
    /// touches <c>FetchAll</c>.
    /// </summary>
    [Test]
    public async Task Get_WithNoParams_CallsGetAsyncOnceWithExpectedPathAndNullQuery()
    {
        var expected = new ApiResult<List<AccountGroup>>
        {
            IsSuccess = true,
            Data = []
        };
        ConnectionHandler
            .GetAsync<List<AccountGroup>>(Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

        var service = new AccountGroupService(ConnectionHandler);

        var result = await service.Get();

        Assert.That(result, Is.SameAs(expected));
        await ConnectionHandler.Received(1).GetAsync<List<AccountGroup>>(
            ExpectedPath,
            null,
            Arg.Any<CancellationToken>());
        await ConnectionHandler.DidNotReceive().FetchAll<AccountGroup>(
            Arg.Any<int>(),
            Arg.Any<int>(),
            Arg.Any<string>(),
            Arg.Any<QueryParameter?>(),
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// When a <see cref="QueryParameterAccountGroup"/> is supplied, its inner
    /// <see cref="QueryParameter"/> instance is forwarded to the connection
    /// handler verbatim — the service must not rewrap or substitute it.
    /// </summary>
    [Test]
    public async Task Get_WithQueryParameter_PassesQueryParameterToConnectionHandler()
    {
        var queryParameter = new QueryParameterAccountGroup(Limit: 50, Offset: 100);
        ConnectionHandler
            .GetAsync<List<AccountGroup>>(Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new ApiResult<List<AccountGroup>> { IsSuccess = true, Data = [] }));

        var service = new AccountGroupService(ConnectionHandler);

        await service.Get(queryParameter);

        await ConnectionHandler.Received(1).GetAsync<List<AccountGroup>>(
            ExpectedPath,
            queryParameter.QueryParameter,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// When <c>autoPage</c> is on and the first response advertises a
    /// <c>X-Total-Count</c> header, the service must call <c>FetchAll</c> with
    /// the count of already-fetched items, the total, the same path, and the
    /// same query parameter.
    /// </summary>
    [Test]
    public async Task Get_WithAutoPage_WhenTotalResultsHeaderPresent_CallsFetchAll()
    {
        var firstGroup = NewAccountGroup(1);
        var firstPage = new ApiResult<List<AccountGroup>>
        {
            IsSuccess = true,
            Data = [firstGroup],
            ResponseHeaders = new Dictionary<string, int?>
            {
                [ApiHeaderNames.TotalResults] = 3
            }
        };
        ConnectionHandler
            .GetAsync<List<AccountGroup>>(Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(firstPage));
        var remaining = new List<AccountGroup> { NewAccountGroup(2), NewAccountGroup(3) };
        ConnectionHandler
            .FetchAll<AccountGroup>(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(remaining));

        var service = new AccountGroupService(ConnectionHandler);

        var result = await service.Get(autoPage: true);

        await ConnectionHandler.Received(1).FetchAll<AccountGroup>(
            1,
            3,
            ExpectedPath,
            null,
            Arg.Any<CancellationToken>());
        Assert.That(result.Data, Has.Count.EqualTo(3));
        Assert.That(result.Data, Is.EquivalentTo(new[] { firstGroup, remaining[0], remaining[1] }));
    }

    /// <summary>
    /// When <c>autoPage</c> is requested but the response carries no
    /// <c>X-Total-Count</c> header, the service must not invoke <c>FetchAll</c>
    /// — there is nothing more to fetch.
    /// </summary>
    [Test]
    public async Task Get_WithAutoPage_WhenTotalResultsHeaderMissing_DoesNotCallFetchAll()
    {
        var response = new ApiResult<List<AccountGroup>>
        {
            IsSuccess = true,
            Data = [NewAccountGroup(1)],
            ResponseHeaders = new Dictionary<string, int?>()
        };
        ConnectionHandler
            .GetAsync<List<AccountGroup>>(Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(response));

        var service = new AccountGroupService(ConnectionHandler);

        var result = await service.Get(autoPage: true);

        Assert.That(result, Is.SameAs(response));
        await ConnectionHandler.DidNotReceive().FetchAll<AccountGroup>(
            Arg.Any<int>(),
            Arg.Any<int>(),
            Arg.Any<string>(),
            Arg.Any<QueryParameter?>(),
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// The success/failure shape, status code, error and data of the
    /// <see cref="ApiResult{T}"/> returned by the connection handler must reach
    /// the caller untouched when no auto-paging happens.
    /// </summary>
    [Test]
    public async Task Get_WhenConnectionHandlerReturnsFailure_PropagatesApiResultUnchanged()
    {
        var apiError = new ApiError(ErrorCode: 500, Message: "boom", Errors: new object());
        var response = new ApiResult<List<AccountGroup>>
        {
            IsSuccess = false,
            ApiError = apiError,
            StatusCode = System.Net.HttpStatusCode.InternalServerError,
            Data = null
        };
        ConnectionHandler
            .GetAsync<List<AccountGroup>>(Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(response));

        var service = new AccountGroupService(ConnectionHandler);

        var result = await service.Get(autoPage: true);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.SameAs(response));
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ApiError, Is.SameAs(apiError));
            Assert.That(result.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.InternalServerError));
            Assert.That(result.Data, Is.Null);
        });
        await ConnectionHandler.DidNotReceive().FetchAll<AccountGroup>(
            Arg.Any<int>(),
            Arg.Any<int>(),
            Arg.Any<string>(),
            Arg.Any<QueryParameter?>(),
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
            .GetAsync<List<AccountGroup>>(Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new ApiResult<List<AccountGroup>> { IsSuccess = true, Data = [] }));

        var service = new AccountGroupService(ConnectionHandler);

        await service.Get(cancellationToken: cts.Token);

        await ConnectionHandler.Received(1).GetAsync<List<AccountGroup>>(
            ExpectedPath,
            null,
            cts.Token);
    }

    private static AccountGroup NewAccountGroup(int id) =>
        new(
            Id: id,
            Uuid: $"uuid-{id}",
            AccountNo: id.ToString(),
            Name: $"Account Group {id}",
            ParentFibuAccountGroupId: null,
            IsActive: true,
            IsLocked: false);
}
