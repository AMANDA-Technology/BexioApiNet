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
using BexioApiNet.Abstractions.Models.Projects.Packages;
using BexioApiNet.Abstractions.Models.Projects.Packages.Views;
using BexioApiNet.Models;
using BexioApiNet.Services.Connectors.Projects;

namespace BexioApiNet.UnitTests.Projects;

/// <summary>
///     Offline unit tests for <see cref="PackageService" />. Each test verifies that the service
///     forwards its calls to <see cref="IBexioConnectionHandler" /> with the expected arguments and
///     returns the handler's result unchanged. Work packages are nested under a parent project, so
///     every call includes the parent project id in the request path. No network access.
/// </summary>
[TestFixture]
public sealed class PackageServiceTests : ServiceTestBase
{
    /// <summary>
    ///     Creates a fresh <see cref="PackageService" /> per test, bound to the
    ///     <see cref="ServiceTestBase.ConnectionHandler" /> substitute provided by the base fixture.
    /// </summary>
    [SetUp]
    public void CreateSut()
    {
        _sut = new PackageService(ConnectionHandler);
    }

    private const int ProjectId = 42;
    private const string ExpectedBaseEndpoint = "3.0/projects/42/packages";

    private PackageService _sut = null!;

    /// <summary>
    ///     <c>GetAsync</c> issues a single <c>GetAsync</c> against
    ///     <c>3.0/projects/{projectId}/packages</c> with a null query parameter.
    /// </summary>
    [Test]
    public async Task GetAsync_CallsGetAsync_WithCorrectPath()
    {
        var response = new ApiResult<List<Package>?> { IsSuccess = true, Data = [] };
        ConnectionHandler
            .GetAsync<List<Package>?>(Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(response));

        await _sut.GetAsync(ProjectId);

        await ConnectionHandler.Received(1).GetAsync<List<Package>?>(
            ExpectedBaseEndpoint,
            null,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    ///     <c>GetAsync</c> returns the <see cref="ApiResult{T}" /> produced by the connection handler
    ///     unchanged.
    /// </summary>
    [Test]
    public async Task GetAsync_ReturnsApiResultFromConnectionHandlerUnchanged()
    {
        var response = new ApiResult<List<Package>?> { IsSuccess = true, Data = [] };
        ConnectionHandler
            .GetAsync<List<Package>?>(Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(response));

        var result = await _sut.GetAsync(ProjectId);

        result.ShouldBeSameAs(response);
    }

    /// <summary>
    ///     The cancellation token supplied by the caller of <c>GetAsync</c> must be forwarded to the
    ///     connection handler so cooperative cancellation flows end-to-end.
    /// </summary>
    [Test]
    public async Task GetAsync_ForwardsCancellationTokenToConnectionHandler()
    {
        using var cts = new CancellationTokenSource();
        ConnectionHandler
            .GetAsync<List<Package>?>(Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new ApiResult<List<Package>?> { IsSuccess = true, Data = [] }));

        await _sut.GetAsync(ProjectId, cts.Token);

        await ConnectionHandler.Received(1).GetAsync<List<Package>?>(
            ExpectedBaseEndpoint,
            null,
            cts.Token);
    }

    /// <summary>
    ///     <c>GetByIdAsync</c> issues a <c>GetAsync</c> against
    ///     <c>3.0/projects/{projectId}/packages/{id}</c>.
    /// </summary>
    [Test]
    public async Task GetByIdAsync_CallsGetAsync_WithIdInPath()
    {
        const int id = 7;
        var response = new ApiResult<Package> { IsSuccess = true };
        ConnectionHandler
            .GetAsync<Package>(Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(response));

        await _sut.GetByIdAsync(ProjectId, id);

        await ConnectionHandler.Received(1).GetAsync<Package>(
            $"{ExpectedBaseEndpoint}/{id}",
            null,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    ///     <c>GetByIdAsync</c> returns the <see cref="ApiResult{T}" /> produced by the connection
    ///     handler unchanged.
    /// </summary>
    [Test]
    public async Task GetByIdAsync_ReturnsApiResultFromConnectionHandlerUnchanged()
    {
        var response = new ApiResult<Package> { IsSuccess = true };
        ConnectionHandler
            .GetAsync<Package>(Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(response));

        var result = await _sut.GetByIdAsync(ProjectId, 1);

        result.ShouldBeSameAs(response);
    }

    /// <summary>
    ///     <c>CreateAsync</c> forwards the supplied <see cref="PackageCreate" /> payload and the
    ///     nested endpoint path to <see cref="IBexioConnectionHandler.PostAsync{TResult,TCreate}" />.
    /// </summary>
    [Test]
    public async Task CreateAsync_CallsPostAsync_WithPayloadAndPath()
    {
        var payload = new PackageCreate("Package 1");
        var response = new ApiResult<Package> { IsSuccess = true };
        ConnectionHandler
            .PostAsync<Package, PackageCreate>(
                Arg.Any<PackageCreate>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(response));

        await _sut.CreateAsync(ProjectId, payload);

        await ConnectionHandler.Received(1).PostAsync<Package, PackageCreate>(
            payload,
            ExpectedBaseEndpoint,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    ///     <c>CreateAsync</c> returns the <see cref="ApiResult{T}" /> produced by the connection
    ///     handler unchanged.
    /// </summary>
    [Test]
    public async Task CreateAsync_ReturnsApiResultFromConnectionHandlerUnchanged()
    {
        var payload = new PackageCreate("Package 1");
        var response = new ApiResult<Package> { IsSuccess = true };
        ConnectionHandler
            .PostAsync<Package, PackageCreate>(
                Arg.Any<PackageCreate>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(response));

        var result = await _sut.CreateAsync(ProjectId, payload);

        result.ShouldBeSameAs(response);
    }

    /// <summary>
    ///     <c>PatchAsync</c> forwards the supplied <see cref="PackagePatch" /> payload via
    ///     <see cref="IBexioConnectionHandler.PatchAsync{TResult,TPatch}" /> against the per-id path.
    /// </summary>
    [Test]
    public async Task PatchAsync_CallsPatchAsync_WithIdInPath()
    {
        const int id = 7;
        var payload = new PackagePatch("Package Updated");
        var response = new ApiResult<Package> { IsSuccess = true };
        ConnectionHandler
            .PatchAsync<Package, PackagePatch>(
                Arg.Any<PackagePatch>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(response));

        await _sut.PatchAsync(ProjectId, id, payload);

        await ConnectionHandler.Received(1).PatchAsync<Package, PackagePatch>(
            payload,
            $"{ExpectedBaseEndpoint}/{id}",
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    ///     <c>DeleteAsync</c> forwards the call to <see cref="IBexioConnectionHandler.Delete" /> with
    ///     the work package id appended to the nested endpoint path.
    /// </summary>
    [Test]
    public async Task DeleteAsync_CallsConnectionHandlerDelete_WithIdInPath()
    {
        const int id = 7;
        var response = new ApiResult<object> { IsSuccess = true };
        ConnectionHandler
            .Delete(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(response));

        await _sut.DeleteAsync(ProjectId, id);

        await ConnectionHandler.Received(1).Delete(
            $"{ExpectedBaseEndpoint}/{id}",
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    ///     <c>DeleteAsync</c> returns the <see cref="ApiResult{T}" /> produced by the connection
    ///     handler unchanged.
    /// </summary>
    [Test]
    public async Task DeleteAsync_ReturnsApiResultFromConnectionHandlerUnchanged()
    {
        var response = new ApiResult<object> { IsSuccess = true };
        ConnectionHandler
            .Delete(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(response));

        var result = await _sut.DeleteAsync(ProjectId, 1);

        result.ShouldBeSameAs(response);
    }
}