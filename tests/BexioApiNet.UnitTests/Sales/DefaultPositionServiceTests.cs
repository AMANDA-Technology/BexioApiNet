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
using BexioApiNet.Abstractions.Models.Sales.Positions;
using BexioApiNet.Abstractions.Models.Sales.Positions.Views;
using BexioApiNet.Models;
using BexioApiNet.Services.Connectors.Sales.Positions;

namespace BexioApiNet.UnitTests.Sales;

/// <summary>
/// Offline unit tests for <see cref="DefaultPositionService" />. Each test verifies that the
/// service delegates to <see cref="IBexioConnectionHandler" /> with the correct endpoint path
/// — in particular that <c>kb_position_custom</c> appears in every path — and that the handler
/// result is returned unchanged. No network, no filesystem access.
/// </summary>
[TestFixture]
public sealed class DefaultPositionServiceTests : ServiceTestBase
{
    private DefaultPositionService _sut = null!;

    private const string DocumentType = "kb_offer";
    private const int DocumentId = 99;
    private const int PositionId = 3;
    private const string ExpectedCollectionPath = "2.0/kb_offer/99/kb_position_custom";
    private const string ExpectedSinglePath = "2.0/kb_offer/99/kb_position_custom/3";

    /// <summary>
    /// Creates a fresh <see cref="DefaultPositionService" /> per test, bound to the
    /// <see cref="ServiceTestBase.ConnectionHandler" /> substitute.
    /// </summary>
    [SetUp]
    public void CreateSut()
    {
        _sut = new DefaultPositionService(ConnectionHandler);
    }

    /// <summary>
    /// Get calls <see cref="IBexioConnectionHandler.GetAsync{TResult}" /> once with the
    /// expected collection path that includes <c>kb_position_custom</c>.
    /// </summary>
    [Test]
    public async Task Get_CallsGetAsync_WithCorrectPositionTypePath()
    {
        var response = new ApiResult<List<PositionCustom>> { IsSuccess = true, Data = [] };
        ConnectionHandler
            .GetAsync<List<PositionCustom>>(Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.Get(DocumentType, DocumentId);

        await ConnectionHandler.Received(1).GetAsync<List<PositionCustom>>(
            ExpectedCollectionPath,
            null,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Get returns the <see cref="ApiResult{T}" /> produced by the connection handler.
    /// </summary>
    [Test]
    public async Task Get_ReturnsApiResult()
    {
        var response = new ApiResult<List<PositionCustom>> { IsSuccess = true, Data = [] };
        ConnectionHandler
            .GetAsync<List<PositionCustom>>(Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(response);

        var result = await _sut.Get(DocumentType, DocumentId);

        Assert.That(result, Is.SameAs(response));
    }

    /// <summary>
    /// GetById calls <see cref="IBexioConnectionHandler.GetAsync{TResult}" /> with the
    /// expected single-position path that includes the position id.
    /// </summary>
    [Test]
    public async Task GetById_CallsGetAsync_WithPositionIdInPath()
    {
        var response = new ApiResult<PositionCustom> { IsSuccess = true };
        ConnectionHandler
            .GetAsync<PositionCustom>(Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.GetById(DocumentType, DocumentId, PositionId);

        await ConnectionHandler.Received(1).GetAsync<PositionCustom>(
            ExpectedSinglePath,
            null,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Create calls <see cref="IBexioConnectionHandler.PostAsync{TResult,TCreate}" /> with the
    /// collection path and the supplied payload.
    /// </summary>
    [Test]
    public async Task Create_CallsPostAsync_WithCollectionPathAndPayload()
    {
        var payload = new PositionCustomCreate(Amount: "1.000000", Text: "Custom service fee", UnitPrice: "250.00");
        var response = new ApiResult<PositionCustom> { IsSuccess = true };
        ConnectionHandler
            .PostAsync<PositionCustom, PositionCustomCreate>(Arg.Any<PositionCustomCreate>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.Create(DocumentType, DocumentId, payload);

        await ConnectionHandler.Received(1).PostAsync<PositionCustom, PositionCustomCreate>(
            payload,
            ExpectedCollectionPath,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Update calls <see cref="IBexioConnectionHandler.PostAsync{TResult,TCreate}" /> with the
    /// single-position path (including the position id) and the supplied payload.
    /// </summary>
    [Test]
    public async Task Update_CallsPostAsync_WithSinglePathAndPayload()
    {
        var payload = new PositionCustomCreate(Amount: "2.000000", Text: "Updated service fee", UnitPrice: "300.00");
        var response = new ApiResult<PositionCustom> { IsSuccess = true };
        ConnectionHandler
            .PostAsync<PositionCustom, PositionCustomCreate>(Arg.Any<PositionCustomCreate>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.Update(DocumentType, DocumentId, PositionId, payload);

        await ConnectionHandler.Received(1).PostAsync<PositionCustom, PositionCustomCreate>(
            payload,
            ExpectedSinglePath,
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

        await _sut.Delete(DocumentType, DocumentId, PositionId);

        await ConnectionHandler.Received(1).Delete(
            ExpectedSinglePath,
            Arg.Any<CancellationToken>());
    }
}
