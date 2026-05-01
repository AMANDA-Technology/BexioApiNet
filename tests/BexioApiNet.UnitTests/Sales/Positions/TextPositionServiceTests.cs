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
/// Offline unit tests for <see cref="TextPositionService"/>. Each test verifies that the
/// service forwards its calls to <see cref="IBexioConnectionHandler"/> with the expected path —
/// including the <c>kb_position_text</c> URL segment — and arguments, and returns the handler's
/// result unchanged. Includes parametrized cases over the three OpenAPI-allowed document
/// types (<c>kb_offer</c>, <c>kb_order</c>, <c>kb_invoice</c>); deliveries are excluded
/// because text positions are not supported on delivery documents per the spec. No network,
/// no filesystem access.
/// </summary>
[TestFixture]
public sealed class TextPositionServiceTests : ServiceTestBase
{
    private const int DocumentId = 2;
    private const int PositionId = 20;

    private TextPositionService _sut = null!;

    /// <summary>
    /// Creates a fresh <see cref="TextPositionService"/> per test, bound to the
    /// <see cref="ServiceTestBase.ConnectionHandler"/> substitute.
    /// </summary>
    [SetUp]
    public void CreateSut()
    {
        _sut = new TextPositionService(ConnectionHandler);
    }

    /// <summary>
    /// GetAll calls <see cref="IBexioConnectionHandler.GetAsync{TResult}"/> once with the
    /// expected list endpoint path and a null query parameter for each supported document type.
    /// </summary>
    [TestCase(KbDocumentType.Offer)]
    [TestCase(KbDocumentType.Order)]
    [TestCase(KbDocumentType.Invoice)]
    public async Task GetAll_CallsGetAsync_WithExpectedPath(string documentType)
    {
        var response = new ApiResult<List<PositionText>> { IsSuccess = true, Data = [] };
        ConnectionHandler
            .GetAsync<List<PositionText>>(Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.GetAll(documentType, DocumentId);

        await ConnectionHandler.Received(1).GetAsync<List<PositionText>>(
            $"2.0/{documentType}/{DocumentId}/kb_position_text",
            null,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// GetAll returns the <see cref="ApiResult{T}"/> produced by the connection handler.
    /// </summary>
    [Test]
    public async Task GetAll_ReturnsApiResult()
    {
        var response = new ApiResult<List<PositionText>> { IsSuccess = true, Data = [] };
        ConnectionHandler
            .GetAsync<List<PositionText>>(Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(response);

        var result = await _sut.GetAll(KbDocumentType.Invoice, DocumentId);

        Assert.That(result, Is.SameAs(response));
    }

    /// <summary>
    /// GetById calls <see cref="IBexioConnectionHandler.GetAsync{TResult}"/> with the expected
    /// path including the position id.
    /// </summary>
    [Test]
    public async Task GetById_CallsGetAsync_WithIdInPath()
    {
        var response = new ApiResult<PositionText> { IsSuccess = true };
        ConnectionHandler
            .GetAsync<PositionText>(Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.GetById(KbDocumentType.Invoice, DocumentId, PositionId);

        await ConnectionHandler.Received(1).GetAsync<PositionText>(
            $"2.0/{KbDocumentType.Invoice}/{DocumentId}/kb_position_text/{PositionId}",
            null,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// GetById returns the <see cref="ApiResult{T}"/> produced by the connection handler.
    /// </summary>
    [Test]
    public async Task GetById_ReturnsApiResult()
    {
        var response = new ApiResult<PositionText> { IsSuccess = true };
        ConnectionHandler
            .GetAsync<PositionText>(Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(response);

        var result = await _sut.GetById(KbDocumentType.Invoice, DocumentId, PositionId);

        Assert.That(result, Is.SameAs(response));
    }

    /// <summary>
    /// Create calls <see cref="IBexioConnectionHandler.PostAsync{TResult,TCreate}"/> with the
    /// payload and the expected list endpoint path.
    /// </summary>
    [Test]
    public async Task Create_CallsPostAsync_WithExpectedPath()
    {
        var payload = new PositionTextCreate(Text: "Payment terms: 30 days net.", ShowPosNr: false);
        var response = new ApiResult<PositionText> { IsSuccess = true };
        ConnectionHandler
            .PostAsync<PositionText, PositionTextCreate>(
                Arg.Any<PositionTextCreate>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.Create(KbDocumentType.Invoice, DocumentId, payload);

        await ConnectionHandler.Received(1).PostAsync<PositionText, PositionTextCreate>(
            payload,
            $"2.0/{KbDocumentType.Invoice}/{DocumentId}/kb_position_text",
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Create returns the <see cref="ApiResult{T}"/> produced by the connection handler.
    /// </summary>
    [Test]
    public async Task Create_ReturnsApiResult()
    {
        var payload = new PositionTextCreate(Text: "Payment terms: 30 days net.", ShowPosNr: false);
        var response = new ApiResult<PositionText> { IsSuccess = true };
        ConnectionHandler
            .PostAsync<PositionText, PositionTextCreate>(
                Arg.Any<PositionTextCreate>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(response);

        var result = await _sut.Create(KbDocumentType.Invoice, DocumentId, payload);

        Assert.That(result, Is.SameAs(response));
    }

    /// <summary>
    /// Update calls <see cref="IBexioConnectionHandler.PostAsync{TResult,TUpdate}"/> (not PUT)
    /// with the position id in the path — Bexio uses POST for position updates.
    /// </summary>
    [Test]
    public async Task Update_CallsPostAsync_WithIdInPath()
    {
        var payload = new PositionTextCreate(Text: "Updated payment terms.", ShowPosNr: true);
        var response = new ApiResult<PositionText> { IsSuccess = true };
        ConnectionHandler
            .PostAsync<PositionText, PositionTextCreate>(
                Arg.Any<PositionTextCreate>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.Update(KbDocumentType.Invoice, DocumentId, PositionId, payload);

        await ConnectionHandler.Received(1).PostAsync<PositionText, PositionTextCreate>(
            payload,
            $"2.0/{KbDocumentType.Invoice}/{DocumentId}/kb_position_text/{PositionId}",
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Delete calls <see cref="IBexioConnectionHandler.Delete"/> with the expected path including
    /// the position id.
    /// </summary>
    [Test]
    public async Task Delete_CallsDelete_WithIdInPath()
    {
        var response = new ApiResult<object> { IsSuccess = true };
        ConnectionHandler
            .Delete(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.Delete(KbDocumentType.Invoice, DocumentId, PositionId);

        await ConnectionHandler.Received(1).Delete(
            $"2.0/{KbDocumentType.Invoice}/{DocumentId}/kb_position_text/{PositionId}",
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
            .Delete(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(response);

        var result = await _sut.Delete(KbDocumentType.Invoice, DocumentId, PositionId);

        Assert.That(result, Is.SameAs(response));
    }
}
