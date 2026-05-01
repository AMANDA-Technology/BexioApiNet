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
/// Offline unit tests for <see cref="ItemPositionService" />. Each test verifies that the
/// service delegates to <see cref="IBexioConnectionHandler" /> with the correct endpoint path
/// — in particular that <c>kb_position_article</c> appears in every path — and that the handler
/// result is returned unchanged. Includes parametrized cases over the three supported document
/// types (<c>kb_offer</c>, <c>kb_order</c>, <c>kb_invoice</c>) per the OpenAPI spec. No network,
/// no filesystem access.
/// </summary>
[TestFixture]
public sealed class ItemPositionServiceTests : ServiceTestBase
{
    private ItemPositionService _sut = null!;

    private const int DocumentId = 42;
    private const int PositionId = 7;

    /// <summary>
    /// Creates a fresh <see cref="ItemPositionService" /> per test, bound to the
    /// <see cref="ServiceTestBase.ConnectionHandler" /> substitute.
    /// </summary>
    [SetUp]
    public void CreateSut()
    {
        _sut = new ItemPositionService(ConnectionHandler);
    }

    /// <summary>
    /// Get calls <see cref="IBexioConnectionHandler.GetAsync{TResult}" /> once with the
    /// expected collection path that includes <c>kb_position_article</c>, for each supported
    /// document type per the OpenAPI <c>kb_document_type</c> enum.
    /// </summary>
    [TestCase(KbDocumentType.Offer)]
    [TestCase(KbDocumentType.Order)]
    [TestCase(KbDocumentType.Invoice)]
    public async Task Get_CallsGetAsync_WithCorrectPositionTypePath(string documentType)
    {
        var response = new ApiResult<List<PositionArticle>> { IsSuccess = true, Data = [] };
        ConnectionHandler
            .GetAsync<List<PositionArticle>>(Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.Get(documentType, DocumentId);

        await ConnectionHandler.Received(1).GetAsync<List<PositionArticle>>(
            $"2.0/{documentType}/{DocumentId}/kb_position_article",
            null,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Get returns the <see cref="ApiResult{T}" /> produced by the connection handler.
    /// </summary>
    [Test]
    public async Task Get_ReturnsApiResult()
    {
        var response = new ApiResult<List<PositionArticle>> { IsSuccess = true, Data = [] };
        ConnectionHandler
            .GetAsync<List<PositionArticle>>(Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(response);

        var result = await _sut.Get(KbDocumentType.Invoice, DocumentId);

        Assert.That(result, Is.SameAs(response));
    }

    /// <summary>
    /// GetById calls <see cref="IBexioConnectionHandler.GetAsync{TResult}" /> with the
    /// expected single-position path that includes the position id.
    /// </summary>
    [Test]
    public async Task GetById_CallsGetAsync_WithPositionIdInPath()
    {
        var response = new ApiResult<PositionArticle> { IsSuccess = true };
        ConnectionHandler
            .GetAsync<PositionArticle>(Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.GetById(KbDocumentType.Invoice, DocumentId, PositionId);

        await ConnectionHandler.Received(1).GetAsync<PositionArticle>(
            $"2.0/{KbDocumentType.Invoice}/{DocumentId}/kb_position_article/{PositionId}",
            null,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// GetById returns the <see cref="ApiResult{T}" /> produced by the connection handler.
    /// </summary>
    [Test]
    public async Task GetById_ReturnsApiResult()
    {
        var response = new ApiResult<PositionArticle> { IsSuccess = true };
        ConnectionHandler
            .GetAsync<PositionArticle>(Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(response);

        var result = await _sut.GetById(KbDocumentType.Invoice, DocumentId, PositionId);

        Assert.That(result, Is.SameAs(response));
    }

    /// <summary>
    /// Create calls <see cref="IBexioConnectionHandler.PostAsync{TResult,TCreate}" /> with the
    /// collection path and the supplied payload.
    /// </summary>
    [Test]
    public async Task Create_CallsPostAsync_WithCollectionPathAndPayload()
    {
        var payload = new PositionArticleCreate(Amount: "1.000000", UnitPrice: "50.00", ArticleId: 10);
        var response = new ApiResult<PositionArticle> { IsSuccess = true };
        ConnectionHandler
            .PostAsync<PositionArticle, PositionArticleCreate>(Arg.Any<PositionArticleCreate>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.Create(KbDocumentType.Invoice, DocumentId, payload);

        await ConnectionHandler.Received(1).PostAsync<PositionArticle, PositionArticleCreate>(
            payload,
            $"2.0/{KbDocumentType.Invoice}/{DocumentId}/kb_position_article",
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Create returns the <see cref="ApiResult{T}" /> produced by the connection handler.
    /// </summary>
    [Test]
    public async Task Create_ReturnsApiResult()
    {
        var payload = new PositionArticleCreate(Amount: "1.000000", UnitPrice: "10.00", ArticleId: 1);
        var response = new ApiResult<PositionArticle> { IsSuccess = true };
        ConnectionHandler
            .PostAsync<PositionArticle, PositionArticleCreate>(Arg.Any<PositionArticleCreate>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(response);

        var result = await _sut.Create(KbDocumentType.Invoice, DocumentId, payload);

        Assert.That(result, Is.SameAs(response));
    }

    /// <summary>
    /// Update calls <see cref="IBexioConnectionHandler.PostAsync{TResult,TCreate}" /> with the
    /// single-position path (including the position id) and the supplied payload. Bexio uses
    /// HTTP <c>POST</c> for position updates rather than <c>PUT</c>.
    /// </summary>
    [Test]
    public async Task Update_CallsPostAsync_WithSinglePathAndPayload()
    {
        var payload = new PositionArticleCreate(Amount: "2.000000", UnitPrice: "99.00");
        var response = new ApiResult<PositionArticle> { IsSuccess = true };
        ConnectionHandler
            .PostAsync<PositionArticle, PositionArticleCreate>(Arg.Any<PositionArticleCreate>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.Update(KbDocumentType.Invoice, DocumentId, PositionId, payload);

        await ConnectionHandler.Received(1).PostAsync<PositionArticle, PositionArticleCreate>(
            payload,
            $"2.0/{KbDocumentType.Invoice}/{DocumentId}/kb_position_article/{PositionId}",
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Delete calls <see cref="IBexioConnectionHandler.Delete" /> with the single-position path.
    /// </summary>
    [Test]
    public async Task Delete_CallsDelete_WithSinglePath()
    {
        var response = new ApiResult<object> { IsSuccess = true };
        ConnectionHandler
            .Delete(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.Delete(KbDocumentType.Invoice, DocumentId, PositionId);

        await ConnectionHandler.Received(1).Delete(
            $"2.0/{KbDocumentType.Invoice}/{DocumentId}/kb_position_article/{PositionId}",
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Delete returns the <see cref="ApiResult{T}" /> produced by the connection handler.
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
