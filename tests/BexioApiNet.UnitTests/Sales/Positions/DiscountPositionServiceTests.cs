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
/// Offline unit tests for <see cref="DiscountPositionService"/>. Each test verifies that the
/// service forwards its calls to <see cref="IBexioConnectionHandler"/> with the expected arguments
/// — in particular the <c>kb_position_discount</c> URL segment — and returns the handler's
/// result unchanged. Includes parametrized cases over the three supported document types
/// (<c>kb_offer</c>, <c>kb_order</c>, <c>kb_invoice</c>) per the OpenAPI spec. No network,
/// no filesystem access.
/// </summary>
[TestFixture]
public sealed class DiscountPositionServiceTests : ServiceTestBase
{
    private const int DocumentId = 1;
    private const int PositionId = 10;

    private DiscountPositionService _sut = null!;

    /// <summary>
    /// Creates a fresh <see cref="DiscountPositionService"/> per test, bound to the
    /// <see cref="ServiceTestBase.ConnectionHandler"/> substitute.
    /// </summary>
    [SetUp]
    public void CreateSut()
    {
        _sut = new DiscountPositionService(ConnectionHandler);
    }

    /// <summary>
    /// GetAll calls <see cref="IBexioConnectionHandler.GetAsync{TResult}"/> once with the
    /// expected list endpoint path and a null query parameter, for each supported document type.
    /// </summary>
    [TestCase(KbDocumentType.Offer)]
    [TestCase(KbDocumentType.Order)]
    [TestCase(KbDocumentType.Invoice)]
    public async Task GetAll_CallsGetAsync_WithExpectedPath(string documentType)
    {
        var response = new ApiResult<List<PositionDiscount>> { IsSuccess = true, Data = [] };
        ConnectionHandler
            .GetAsync<List<PositionDiscount>>(Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.GetAll(documentType, DocumentId);

        await ConnectionHandler.Received(1).GetAsync<List<PositionDiscount>>(
            $"2.0/{documentType}/{DocumentId}/kb_position_discount",
            null,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// GetAll returns the <see cref="ApiResult{T}"/> produced by the connection handler.
    /// </summary>
    [Test]
    public async Task GetAll_ReturnsApiResult()
    {
        var response = new ApiResult<List<PositionDiscount>> { IsSuccess = true, Data = [] };
        ConnectionHandler
            .GetAsync<List<PositionDiscount>>(Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
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
        var response = new ApiResult<PositionDiscount> { IsSuccess = true };
        ConnectionHandler
            .GetAsync<PositionDiscount>(Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.GetById(KbDocumentType.Invoice, DocumentId, PositionId);

        await ConnectionHandler.Received(1).GetAsync<PositionDiscount>(
            $"2.0/{KbDocumentType.Invoice}/{DocumentId}/kb_position_discount/{PositionId}",
            null,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// GetById returns the <see cref="ApiResult{T}"/> produced by the connection handler.
    /// </summary>
    [Test]
    public async Task GetById_ReturnsApiResult()
    {
        var response = new ApiResult<PositionDiscount> { IsSuccess = true };
        ConnectionHandler
            .GetAsync<PositionDiscount>(Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
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
        var payload = new PositionDiscountCreate(Text: "10% discount", IsPercentual: true, Value: "10.000000");
        var response = new ApiResult<PositionDiscount> { IsSuccess = true };
        ConnectionHandler
            .PostAsync<PositionDiscount, PositionDiscountCreate>(Arg.Any<PositionDiscountCreate>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.Create(KbDocumentType.Invoice, DocumentId, payload);

        await ConnectionHandler.Received(1).PostAsync<PositionDiscount, PositionDiscountCreate>(
            payload,
            $"2.0/{KbDocumentType.Invoice}/{DocumentId}/kb_position_discount",
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Create returns the <see cref="ApiResult{T}"/> produced by the connection handler.
    /// </summary>
    [Test]
    public async Task Create_ReturnsApiResult()
    {
        var payload = new PositionDiscountCreate(Text: "5%", IsPercentual: true, Value: "5.000000");
        var response = new ApiResult<PositionDiscount> { IsSuccess = true };
        ConnectionHandler
            .PostAsync<PositionDiscount, PositionDiscountCreate>(Arg.Any<PositionDiscountCreate>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
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
        var payload = new PositionDiscountCreate(Text: "5% discount", IsPercentual: true, Value: "5.000000");
        var response = new ApiResult<PositionDiscount> { IsSuccess = true };
        ConnectionHandler
            .PostAsync<PositionDiscount, PositionDiscountCreate>(Arg.Any<PositionDiscountCreate>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.Update(KbDocumentType.Invoice, DocumentId, PositionId, payload);

        await ConnectionHandler.Received(1).PostAsync<PositionDiscount, PositionDiscountCreate>(
            payload,
            $"2.0/{KbDocumentType.Invoice}/{DocumentId}/kb_position_discount/{PositionId}",
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
            $"2.0/{KbDocumentType.Invoice}/{DocumentId}/kb_position_discount/{PositionId}",
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
