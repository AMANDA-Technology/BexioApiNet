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

using BexioApiNet.Abstractions.Models.Api;
using BexioApiNet.Abstractions.Models.MasterData.Users;
using BexioApiNet.Models;
using BexioApiNet.Services.Connectors.MasterData;

namespace BexioApiNet.UnitTests.MasterData;

/// <summary>
///     Offline unit tests for <see cref="UserService" />. Each test asserts that the service forwards
///     its calls to <see cref="IBexioConnectionHandler" /> with the expected verb, path and query
///     parameter, and returns the handler's <see cref="ApiResult{T}" /> unchanged. Verifies that the
///     singleton <c>GetMe</c> call hits <c>3.0/users/me</c> (no id segment). No network, no filesystem.
/// </summary>
[TestFixture]
[Category("Unit")]
public sealed class UserServiceTests : ServiceTestBase
{
    /// <summary>
    ///     Creates a fresh <see cref="UserService" /> bound to the <see cref="ServiceTestBase.ConnectionHandler" />
    ///     substitute before each test.
    /// </summary>
    [SetUp]
    public void CreateSut()
    {
        _sut = new UserService(ConnectionHandler);
    }

    private const string ExpectedEndpoint = "3.0/users";

    private UserService _sut = null!;

    private static User NewUser(int id = 1)
    {
        return new User(id, "male", "Rudolph", "Smith", "rudolph.smith@example.com", false, false);
    }

    /// <summary>
    ///     <c>GetAll</c> with no query parameter must hit <c>3.0/users</c> exactly once with a
    ///     <see langword="null" /> query parameter and return the connection handler's
    ///     <see cref="ApiResult{T}" /> as-is.
    /// </summary>
    [Test]
    public async Task GetAll_WithNoParams_CallsGetAsyncOnceWithExpectedPath()
    {
        var expected = new ApiResult<List<User>>
        {
            IsSuccess = true,
            Data = [NewUser()]
        };
        ConnectionHandler
            .GetAsync<List<User>>(Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

        var result = await _sut.GetAll();

        Assert.That(result, Is.SameAs(expected));
        await ConnectionHandler.Received(1).GetAsync<List<User>>(
            ExpectedEndpoint,
            null,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    ///     When a <see cref="QueryParameterUser" /> is supplied, the wrapped dictionary is forwarded to
    ///     the connection handler so pagination reaches the API.
    /// </summary>
    [Test]
    public async Task GetAll_WithQueryParameter_PassesQueryParameterToHandler()
    {
        var query = new QueryParameterUser(100, 50);
        ConnectionHandler
            .GetAsync<List<User>>(Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new ApiResult<List<User>> { IsSuccess = true, Data = [] }));

        await _sut.GetAll(query);

        await ConnectionHandler.Received(1).GetAsync<List<User>>(
            ExpectedEndpoint,
            query.QueryParameter,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    ///     The cancellation token supplied by the caller must be forwarded to the connection handler
    ///     so cooperative cancellation flows end-to-end.
    /// </summary>
    [Test]
    public async Task GetAll_ForwardsCancellationTokenToConnectionHandler()
    {
        using var cts = new CancellationTokenSource();
        ConnectionHandler
            .GetAsync<List<User>>(Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new ApiResult<List<User>> { IsSuccess = true, Data = [] }));

        await _sut.GetAll(cancellationToken: cts.Token);

        await ConnectionHandler.Received(1).GetAsync<List<User>>(
            ExpectedEndpoint,
            null,
            cts.Token);
    }

    /// <summary>
    ///     <c>GetMe</c> must hit the singleton <c>3.0/users/me</c> route (no id segment) exactly once
    ///     with a <see langword="null" /> query parameter.
    /// </summary>
    [Test]
    public async Task GetMe_CallsGetAsyncWithMePath()
    {
        var expected = new ApiResult<User>
        {
            IsSuccess = true,
            Data = NewUser()
        };
        ConnectionHandler
            .GetAsync<User>(Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

        var result = await _sut.GetMe();

        Assert.That(result, Is.SameAs(expected));
        await ConnectionHandler.Received(1).GetAsync<User>(
            $"{ExpectedEndpoint}/me",
            null,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    ///     <c>GetMe</c> forwards the supplied cancellation token to the connection handler.
    /// </summary>
    [Test]
    public async Task GetMe_ForwardsCancellationTokenToConnectionHandler()
    {
        using var cts = new CancellationTokenSource();
        ConnectionHandler
            .GetAsync<User>(Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new ApiResult<User> { IsSuccess = true }));

        await _sut.GetMe(cts.Token);

        await ConnectionHandler.Received(1).GetAsync<User>(
            $"{ExpectedEndpoint}/me",
            null,
            cts.Token);
    }

    /// <summary>
    ///     <c>GetById</c> must build the request path with the user id appended to the endpoint root.
    /// </summary>
    [Test]
    public async Task GetById_CallsGetAsyncWithIdInPath()
    {
        const int id = 42;
        var expected = new ApiResult<User>
        {
            IsSuccess = true,
            Data = NewUser(id)
        };
        ConnectionHandler
            .GetAsync<User>(Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

        var result = await _sut.GetById(id);

        Assert.That(result, Is.SameAs(expected));
        await ConnectionHandler.Received(1).GetAsync<User>(
            $"{ExpectedEndpoint}/{id}",
            null,
            Arg.Any<CancellationToken>());
    }
}
