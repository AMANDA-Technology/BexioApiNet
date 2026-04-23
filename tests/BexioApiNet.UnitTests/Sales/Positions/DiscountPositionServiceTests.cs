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
using BexioApiNet.Models;
using BexioApiNet.Services.Connectors.Sales.Positions;

namespace BexioApiNet.UnitTests.Sales.Positions;

/// <summary>
/// Offline unit tests for <see cref="DiscountPositionService"/>. Each test verifies that the
/// service forwards its calls to <see cref="IBexioConnectionHandler"/> with the expected arguments
/// and returns the handler's result unchanged. No network, no filesystem access.
/// </summary>
[TestFixture]
public sealed class DiscountPositionServiceTests : ServiceTestBase
{
    private const string DocumentType = "kb_invoice";
    private const int DocumentId = 1;
    private const int PositionId = 10;
    private const string ExpectedListPath = $"2.0/{DocumentType}/1/kb_position_discount";
    private const string ExpectedSinglePath = $"2.0/{DocumentType}/1/kb_position_discount/10";

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
    /// expected list endpoint path and a null query parameter.
    /// </summary>
    [Test]
    public async Task GetAll_CallsGetAsync_WithExpectedPath()
    {
        var response = new ApiResult<List<PositionDiscount>> { IsSuccess = true, Data = [] };
        ConnectionHandler
            .GetAsync<List<PositionDiscount>>(Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.GetAll(DocumentType, DocumentId);

        await ConnectionHandler.Received(1).GetAsync<List<PositionDiscount>>(
            ExpectedListPath,
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

        var result = await _sut.GetAll(DocumentType, DocumentId);

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
        string? capturedPath = null;
        ConnectionHandler
            .GetAsync<PositionDiscount>(Arg.Do<string>(p => capturedPath = p), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.GetById(DocumentType, DocumentId, PositionId);

        Assert.That(capturedPath, Is.EqualTo(ExpectedSinglePath));
    }

    /// <summary>
    /// Create calls <see cref="IBexioConnectionHandler.PostAsync{TResult,TCreate}"/> with the
    /// payload and the expected list endpoint path.
    /// </summary>
    [Test]
    public async Task Create_CallsPostAsync_WithExpectedPath()
    {
        var payload = new PositionDiscount { Text = "10% discount", IsPercentual = true, Value = "10.000000" };
        var response = new ApiResult<PositionDiscount> { IsSuccess = true };
        ConnectionHandler
            .PostAsync<PositionDiscount, PositionDiscount>(Arg.Any<PositionDiscount>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.Create(DocumentType, DocumentId, payload);

        await ConnectionHandler.Received(1).PostAsync<PositionDiscount, PositionDiscount>(
            payload,
            ExpectedListPath,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Update calls <see cref="IBexioConnectionHandler.PostAsync{TResult,TUpdate}"/> (not PUT)
    /// with the position id in the path — Bexio uses POST for position updates.
    /// </summary>
    [Test]
    public async Task Update_CallsPostAsync_WithIdInPath()
    {
        var payload = new PositionDiscount { Text = "5% discount", IsPercentual = true, Value = "5.000000" };
        var response = new ApiResult<PositionDiscount> { IsSuccess = true };
        string? capturedPath = null;
        ConnectionHandler
            .PostAsync<PositionDiscount, PositionDiscount>(Arg.Any<PositionDiscount>(), Arg.Do<string>(p => capturedPath = p), Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.Update(DocumentType, DocumentId, PositionId, payload);

        Assert.That(capturedPath, Is.EqualTo(ExpectedSinglePath));
    }

    /// <summary>
    /// Delete calls <see cref="IBexioConnectionHandler.Delete"/> with the expected path including
    /// the position id.
    /// </summary>
    [Test]
    public async Task Delete_CallsDelete_WithIdInPath()
    {
        var response = new ApiResult<object> { IsSuccess = true };
        string? capturedPath = null;
        ConnectionHandler
            .Delete(Arg.Do<string>(p => capturedPath = p), Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.Delete(DocumentType, DocumentId, PositionId);

        Assert.That(capturedPath, Is.EqualTo(ExpectedSinglePath));
    }
}
