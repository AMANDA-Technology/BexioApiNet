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
using BexioApiNet.Abstractions.Models.Projects.ProjectState;
using BexioApiNet.Models;
using BexioApiNet.Services.Connectors.Projects;

namespace BexioApiNet.UnitTests.Projects;

/// <summary>
///     Offline unit tests for <see cref="ProjectStateService" />. Verifies the read-only lookup
///     forwards to <see cref="IBexioConnectionHandler.GetAsync{TResult}" /> with the expected path.
/// </summary>
[TestFixture]
public sealed class ProjectStateServiceTests : ServiceTestBase
{
    /// <summary>
    ///     Creates a fresh <see cref="ProjectStateService" /> per test.
    /// </summary>
    [SetUp]
    public void CreateSut()
    {
        _sut = new ProjectStateService(ConnectionHandler);
    }

    private const string ExpectedEndpoint = "2.0/pr_project_state";

    private ProjectStateService _sut = null!;

    /// <summary>
    ///     Get calls <see cref="IBexioConnectionHandler.GetAsync{TResult}" /> once with the expected
    ///     endpoint path and a null query parameter (this endpoint takes no filters).
    /// </summary>
    [Test]
    public async Task Get_CallsGetAsync_WithExpectedPath()
    {
        var response = new ApiResult<List<ProjectState>> { IsSuccess = true, Data = [] };
        ConnectionHandler
            .GetAsync<List<ProjectState>>(
                Arg.Any<string>(),
                Arg.Any<QueryParameter?>(),
                Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.Get();

        await ConnectionHandler.Received(1).GetAsync<List<ProjectState>>(
            ExpectedEndpoint,
            null,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    ///     Get returns the <see cref="ApiResult{T}" /> produced by the connection handler without modification.
    /// </summary>
    [Test]
    public async Task Get_ReturnsApiResultFromConnectionHandler()
    {
        var response = new ApiResult<List<ProjectState>>
        {
            IsSuccess = true,
            Data = [new ProjectState(1, "Active")]
        };
        ConnectionHandler
            .GetAsync<List<ProjectState>>(
                Arg.Any<string>(),
                Arg.Any<QueryParameter?>(),
                Arg.Any<CancellationToken>())
            .Returns(response);

        var result = await _sut.Get();

        result.ShouldBeSameAs(response);
    }
}
