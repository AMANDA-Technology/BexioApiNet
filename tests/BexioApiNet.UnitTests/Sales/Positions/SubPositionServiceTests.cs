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
/// Offline unit tests for <see cref="SubPositionService"/>. Each test verifies that the service
/// forwards its calls to <see cref="IBexioConnectionHandler"/> with the expected path —
/// including the <c>kb_position_subposition</c> URL segment — and arguments, and returns the
/// handler's result unchanged. Includes parametrized cases over the three OpenAPI-allowed
/// document types (<c>kb_offer</c>, <c>kb_order</c>, <c>kb_invoice</c>); deliveries are
/// excluded because sub-positions are not supported on delivery documents per the spec.
/// No network, no filesystem access.
/// </summary>
[TestFixture]
public sealed class SubPositionServiceTests : ServiceTestBase
{
    private const int DocumentId = 4;
    private const int PositionId = 7;

    private SubPositionService _sut = null!;

    /// <summary>Creates a fresh <see cref="SubPositionService"/> bound to the substitute per test.</summary>
    [SetUp]
    public void CreateSut()
    {
        _sut = new SubPositionService(ConnectionHandler);
    }

    /// <summary>
    /// Get calls <see cref="IBexioConnectionHandler.GetAsync{TResult}"/> once with the expected
    /// position-list path and a null query parameter, for each supported document type.
    /// </summary>
    [TestCase(KbDocumentType.Offer)]
    [TestCase(KbDocumentType.Order)]
    [TestCase(KbDocumentType.Invoice)]
    public async Task Get_CallsGetAsync_WithExpectedPath(string documentType)
    {
        var response = new ApiResult<List<PositionSubposition>> { IsSuccess = true, Data = [] };
        ConnectionHandler
            .GetAsync<List<PositionSubposition>>(
                Arg.Any<string>(),
                Arg.Any<QueryParameter?>(),
                Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.Get(documentType, DocumentId);

        await ConnectionHandler.Received(1).GetAsync<List<PositionSubposition>>(
            $"2.0/{documentType}/{DocumentId}/kb_position_subposition",
            null,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Get returns the <see cref="ApiResult{T}"/> produced by the connection handler unchanged.
    /// </summary>
    [Test]
    public async Task Get_ReturnsApiResult()
    {
        var response = new ApiResult<List<PositionSubposition>> { IsSuccess = true, Data = [] };
        ConnectionHandler
            .GetAsync<List<PositionSubposition>>(
                Arg.Any<string>(),
                Arg.Any<QueryParameter?>(),
                Arg.Any<CancellationToken>())
            .Returns(response);

        var result = await _sut.Get(KbDocumentType.Invoice, DocumentId);

        Assert.That(result, Is.SameAs(response));
    }

    /// <summary>
    /// GetById calls <see cref="IBexioConnectionHandler.GetAsync{TResult}"/> with the position id
    /// appended to the path.
    /// </summary>
    [Test]
    public async Task GetById_CallsGetAsync_WithPositionIdInPath()
    {
        var response = new ApiResult<PositionSubposition> { IsSuccess = true };
        ConnectionHandler
            .GetAsync<PositionSubposition>(
                Arg.Any<string>(),
                Arg.Any<QueryParameter?>(),
                Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.GetById(KbDocumentType.Invoice, DocumentId, PositionId);

        await ConnectionHandler.Received(1).GetAsync<PositionSubposition>(
            $"2.0/{KbDocumentType.Invoice}/{DocumentId}/kb_position_subposition/{PositionId}",
            null,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// GetById returns the <see cref="ApiResult{T}"/> produced by the connection handler.
    /// </summary>
    [Test]
    public async Task GetById_ReturnsApiResult()
    {
        var response = new ApiResult<PositionSubposition> { IsSuccess = true };
        ConnectionHandler
            .GetAsync<PositionSubposition>(
                Arg.Any<string>(),
                Arg.Any<QueryParameter?>(),
                Arg.Any<CancellationToken>())
            .Returns(response);

        var result = await _sut.GetById(KbDocumentType.Invoice, DocumentId, PositionId);

        Assert.That(result, Is.SameAs(response));
    }

    /// <summary>
    /// Create calls <see cref="IBexioConnectionHandler.PostAsync{TResult,TCreate}"/> with the
    /// expected list path and the payload unchanged.
    /// </summary>
    [Test]
    public async Task Create_CallsPostAsync_WithExpectedPath()
    {
        var payload = new PositionSubpositionCreate(Text: "Group heading", ShowPosNr: true);
        var response = new ApiResult<PositionSubposition> { IsSuccess = true };
        ConnectionHandler
            .PostAsync<PositionSubposition, PositionSubpositionCreate>(
                Arg.Any<PositionSubpositionCreate>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.Create(KbDocumentType.Invoice, DocumentId, payload);

        await ConnectionHandler.Received(1).PostAsync<PositionSubposition, PositionSubpositionCreate>(
            payload,
            $"2.0/{KbDocumentType.Invoice}/{DocumentId}/kb_position_subposition",
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Create returns the <see cref="ApiResult{T}"/> produced by the connection handler.
    /// </summary>
    [Test]
    public async Task Create_ReturnsApiResult()
    {
        var payload = new PositionSubpositionCreate(Text: "Group heading", ShowPosNr: true);
        var response = new ApiResult<PositionSubposition> { IsSuccess = true };
        ConnectionHandler
            .PostAsync<PositionSubposition, PositionSubpositionCreate>(
                Arg.Any<PositionSubpositionCreate>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>())
            .Returns(response);

        var result = await _sut.Create(KbDocumentType.Invoice, DocumentId, payload);

        Assert.That(result, Is.SameAs(response));
    }

    /// <summary>
    /// Update calls <see cref="IBexioConnectionHandler.PostAsync{TResult,TUpdate}"/> (not PUT) with
    /// the position id in the path and the payload unchanged.
    /// </summary>
    [Test]
    public async Task Update_CallsPostAsync_WithPositionIdInPath()
    {
        var payload = new PositionSubpositionUpdate(Text: "Updated heading");
        var response = new ApiResult<PositionSubposition> { IsSuccess = true };
        ConnectionHandler
            .PostAsync<PositionSubposition, PositionSubpositionUpdate>(
                Arg.Any<PositionSubpositionUpdate>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.Update(KbDocumentType.Invoice, DocumentId, PositionId, payload);

        await ConnectionHandler.Received(1).PostAsync<PositionSubposition, PositionSubpositionUpdate>(
            payload,
            $"2.0/{KbDocumentType.Invoice}/{DocumentId}/kb_position_subposition/{PositionId}",
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Delete calls <see cref="IBexioConnectionHandler.Delete"/> with the position id appended to
    /// the path.
    /// </summary>
    [Test]
    public async Task Delete_CallsDelete_WithPositionIdInPath()
    {
        var response = new ApiResult<object> { IsSuccess = true };
        ConnectionHandler
            .Delete(
                Arg.Any<string>(),
                Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.Delete(KbDocumentType.Invoice, DocumentId, PositionId);

        await ConnectionHandler.Received(1).Delete(
            $"2.0/{KbDocumentType.Invoice}/{DocumentId}/kb_position_subposition/{PositionId}",
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Delete returns the <see cref="ApiResult{T}"/> produced by the connection handler.
    /// </summary>
    [Test]
    public async Task Delete_ReturnsApiResult()
    {
        var response = new ApiResult<object> { IsSuccess = true };
        ConnectionHandler
            .Delete(
                Arg.Any<string>(),
                Arg.Any<CancellationToken>())
            .Returns(response);

        var result = await _sut.Delete(KbDocumentType.Invoice, DocumentId, PositionId);

        Assert.That(result, Is.SameAs(response));
    }
}
