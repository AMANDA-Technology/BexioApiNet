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

using BexioApiNet.Abstractions.Enums.Sales;
using BexioApiNet.Abstractions.Models.Api;
using BexioApiNet.Abstractions.Models.Sales.Positions;
using BexioApiNet.Abstractions.Models.Sales.Positions.Views;
using BexioApiNet.Models;
using BexioApiNet.Services.Connectors.Sales.Positions;

namespace BexioApiNet.UnitTests.Sales.Positions;

/// <summary>
/// Offline unit tests for <see cref="PagebreakPositionService"/>. Each test verifies that the
/// service forwards its calls to <see cref="IBexioConnectionHandler"/> with the expected path and
/// arguments, and returns the handler's result unchanged. No network, no filesystem access.
/// </summary>
[TestFixture]
public sealed class PagebreakPositionServiceTests : ServiceTestBase
{
    private const string DocumentType = KbDocumentType.Order;
    private const int DocumentId = 5;
    private const int PositionId = 9;

    private PagebreakPositionService _sut = null!;

    /// <summary>Creates a fresh <see cref="PagebreakPositionService"/> bound to the substitute per test.</summary>
    [SetUp]
    public void CreateSut()
    {
        _sut = new PagebreakPositionService(ConnectionHandler);
    }

    private static string ExpectedBasePath => $"2.0/{DocumentType}/{DocumentId}/kb_position_pagebreak";

    /// <summary>
    /// Get calls <see cref="IBexioConnectionHandler.GetAsync{TResult}"/> once with the expected
    /// position-list path and a null query parameter.
    /// </summary>
    [Test]
    public async Task Get_CallsGetAsync_WithExpectedPath()
    {
        var response = new ApiResult<List<PositionPagebreak>> { IsSuccess = true, Data = [] };
        ConnectionHandler
            .GetAsync<List<PositionPagebreak>>(
                Arg.Any<string>(),
                Arg.Any<QueryParameter?>(),
                Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.Get(DocumentType, DocumentId);

        await ConnectionHandler.Received(1).GetAsync<List<PositionPagebreak>>(
            ExpectedBasePath,
            null,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Get returns the <see cref="ApiResult{T}"/> produced by the connection handler unchanged.
    /// </summary>
    [Test]
    public async Task Get_ReturnsApiResult()
    {
        var response = new ApiResult<List<PositionPagebreak>> { IsSuccess = true, Data = [] };
        ConnectionHandler
            .GetAsync<List<PositionPagebreak>>(
                Arg.Any<string>(),
                Arg.Any<QueryParameter?>(),
                Arg.Any<CancellationToken>())
            .Returns(response);

        var result = await _sut.Get(DocumentType, DocumentId);

        Assert.That(result, Is.SameAs(response));
    }

    /// <summary>
    /// GetById calls <see cref="IBexioConnectionHandler.GetAsync{TResult}"/> with the position id
    /// appended to the path.
    /// </summary>
    [Test]
    public async Task GetById_CallsGetAsync_WithPositionIdInPath()
    {
        var response = new ApiResult<PositionPagebreak> { IsSuccess = true };
        string? capturedPath = null;
        ConnectionHandler
            .GetAsync<PositionPagebreak>(
                Arg.Do<string>(p => capturedPath = p),
                Arg.Any<QueryParameter?>(),
                Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.GetById(DocumentType, DocumentId, PositionId);

        Assert.That(capturedPath, Is.EqualTo($"{ExpectedBasePath}/{PositionId}"));
    }

    /// <summary>
    /// Create calls <see cref="IBexioConnectionHandler.PostAsync{TResult,TCreate}"/> with the
    /// expected list path and the payload unchanged.
    /// </summary>
    [Test]
    public async Task Create_CallsPostAsync_WithExpectedPath()
    {
        var payload = new PositionPagebreakCreate(Pagebreak: true);
        var response = new ApiResult<PositionPagebreak> { IsSuccess = true };
        PositionPagebreakCreate? capturedPayload = null;
        string? capturedPath = null;
        ConnectionHandler
            .PostAsync<PositionPagebreak, PositionPagebreakCreate>(
                Arg.Do<PositionPagebreakCreate>(p => capturedPayload = p),
                Arg.Do<string>(p => capturedPath = p),
                Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.Create(DocumentType, DocumentId, payload);

        Assert.Multiple(() =>
        {
            Assert.That(capturedPath, Is.EqualTo(ExpectedBasePath));
            Assert.That(capturedPayload, Is.SameAs(payload));
        });
    }

    /// <summary>
    /// Update calls <see cref="IBexioConnectionHandler.PostAsync{TResult,TUpdate}"/> (not PUT) with
    /// the position id in the path and the payload unchanged.
    /// </summary>
    [Test]
    public async Task Update_CallsPostAsync_WithPositionIdInPath()
    {
        var payload = new PositionPagebreakUpdate(Pagebreak: true);
        var response = new ApiResult<PositionPagebreak> { IsSuccess = true };
        string? capturedPath = null;
        ConnectionHandler
            .PostAsync<PositionPagebreak, PositionPagebreakUpdate>(
                Arg.Any<PositionPagebreakUpdate>(),
                Arg.Do<string>(p => capturedPath = p),
                Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.Update(DocumentType, DocumentId, PositionId, payload);

        Assert.That(capturedPath, Is.EqualTo($"{ExpectedBasePath}/{PositionId}"));
    }

    /// <summary>
    /// Delete calls <see cref="IBexioConnectionHandler.Delete"/> with the position id appended to
    /// the path.
    /// </summary>
    [Test]
    public async Task Delete_CallsDelete_WithPositionIdInPath()
    {
        var response = new ApiResult<object> { IsSuccess = true };
        string? capturedPath = null;
        ConnectionHandler
            .Delete(
                Arg.Do<string>(p => capturedPath = p),
                Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.Delete(DocumentType, DocumentId, PositionId);

        Assert.That(capturedPath, Is.EqualTo($"{ExpectedBasePath}/{PositionId}"));
    }
}
