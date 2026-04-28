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
using BexioApiNet.Abstractions.Models.MasterData.Permissions;
using BexioApiNet.Models;
using BexioApiNet.Services.Connectors.MasterData;

namespace BexioApiNet.UnitTests.MasterData;

/// <summary>
/// Offline unit tests for <see cref="PermissionService"/>. Verifies the singleton lookup forwards
/// to <see cref="IBexioConnectionHandler.GetAsync{TResult}"/> against the <c>3.0/permissions</c>
/// route.
/// </summary>
[TestFixture]
public sealed class PermissionServiceTests : ServiceTestBase
{
    private const string ExpectedEndpoint = "3.0/permissions";

    private PermissionService _sut = null!;

    /// <summary>
    /// Creates a fresh <see cref="PermissionService"/> per test bound to the base-fixture substitute.
    /// </summary>
    [SetUp]
    public void CreateSut()
    {
        _sut = new PermissionService(ConnectionHandler);
    }

    /// <summary>
    /// <c>Get</c> hits <c>3.0/permissions</c> (no id) via the connection handler with a
    /// <see langword="null"/> query parameter.
    /// </summary>
    [Test]
    public async Task Get_CallsGetAsync_WithExpectedSingletonPath()
    {
        var response = new ApiResult<Permission>
        {
            IsSuccess = true,
            Data = new Permission
            {
                Components = ["functionality1"],
                Permissions = new Dictionary<string, PermissionAccess>
                {
                    { "contact", new PermissionAccess { Activation = "enabled", Edit = "own", Show = "all" } }
                }
            }
        };
        ConnectionHandler
            .GetAsync<Permission>(Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(response));

        var result = await _sut.Get();

        result.ShouldBeSameAs(response);
        await ConnectionHandler.Received(1).GetAsync<Permission>(
            ExpectedEndpoint,
            null,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// The cancellation token supplied by the caller is forwarded to the connection handler.
    /// </summary>
    [Test]
    public async Task Get_ForwardsCancellationTokenToConnectionHandler()
    {
        using var cts = new CancellationTokenSource();
        ConnectionHandler
            .GetAsync<Permission>(Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new ApiResult<Permission> { IsSuccess = true, Data = new Permission() }));

        await _sut.Get(cts.Token);

        await ConnectionHandler.Received(1).GetAsync<Permission>(
            ExpectedEndpoint,
            null,
            cts.Token);
    }
}
