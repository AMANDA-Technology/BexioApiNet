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
using BexioApiNet.Abstractions.Models.Purchases.Bills;
using BexioApiNet.Abstractions.Models.Purchases.Bills.Enums;
using BexioApiNet.Abstractions.Models.Purchases.Bills.Views;
using BexioApiNet.Models;
using BexioApiNet.Services.Connectors.Purchases;

namespace BexioApiNet.UnitTests.Purchases;

/// <summary>
/// Offline unit tests for <see cref="BillService"/>. Each test asserts that the service
/// forwards its calls to <see cref="IBexioConnectionHandler"/> with the expected
/// verb, path, payload and query parameters, and returns the handler's <see cref="ApiResult{T}"/>
/// unchanged. No network, no filesystem.
/// </summary>
[TestFixture]
public sealed class BillServiceTests : ServiceTestBase
{
    private const string ExpectedEndpoint = "4.0/purchase/bills";
    private const string ExpectedDocNumberEndpoint = "4.0/purchase/documentnumbers/bills";

    private BillService _sut = null!;

    /// <summary>
    /// Creates a fresh <see cref="BillService"/> bound to the
    /// <see cref="ServiceTestBase.ConnectionHandler"/> substitute before each test.
    /// </summary>
    [SetUp]
    public void CreateSut()
    {
        _sut = new BillService(ConnectionHandler);
    }

    /// <summary>
    /// Get with no query parameter forwards a <see langword="null"/> <see cref="QueryParameter"/>
    /// to <see cref="IBexioConnectionHandler.GetAsync{TResult}"/> at the bills collection path.
    /// </summary>
    [Test]
    public async Task Get_WithNoParams_CallsGetAsync()
    {
        var response = new ApiResult<BillListResponse>
        {
            IsSuccess = true,
            Data = new BillListResponse([], new BillPaging(1, 100, 1, 0))
        };
        ConnectionHandler
            .GetAsync<BillListResponse>(Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.Get();

        await ConnectionHandler.Received(1).GetAsync<BillListResponse>(
            ExpectedEndpoint,
            null,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Get forwards the supplied <see cref="QueryParameterBill.QueryParameter"/>
    /// to <see cref="IBexioConnectionHandler.GetAsync{TResult}"/>.
    /// </summary>
    [Test]
    public async Task Get_WithQueryParameter_ForwardsQueryParameter()
    {
        var queryParameter = new QueryParameterBill(limit: 50, status: "TODO");
        ConnectionHandler
            .GetAsync<BillListResponse>(Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(new ApiResult<BillListResponse>
            {
                IsSuccess = true,
                Data = new BillListResponse([], new BillPaging(1, 50, 0, 0))
            });

        await _sut.Get(queryParameter);

        await ConnectionHandler.Received(1).GetAsync<BillListResponse>(
            ExpectedEndpoint,
            queryParameter.QueryParameter,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Get returns the <see cref="ApiResult{T}"/> from the connection handler unchanged.
    /// </summary>
    [Test]
    public async Task Get_ReturnsApiResultFromConnectionHandler()
    {
        var response = new ApiResult<BillListResponse>
        {
            IsSuccess = true,
            Data = new BillListResponse([], new BillPaging(1, 100, 1, 0))
        };
        ConnectionHandler
            .GetAsync<BillListResponse>(Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(response);

        var result = await _sut.Get();

        Assert.That(result, Is.SameAs(response));
    }

    /// <summary>
    /// GetById calls <see cref="IBexioConnectionHandler.GetAsync{TResult}"/> with the
    /// bill id appended to the endpoint root and a <see langword="null"/> query parameter.
    /// </summary>
    [Test]
    public async Task GetById_CallsGetAsync_WithIdInPath()
    {
        var id = Guid.NewGuid();
        string? capturedPath = null;
        ConnectionHandler
            .GetAsync<Bill>(
                Arg.Do<string>(path => capturedPath = path),
                Arg.Any<QueryParameter?>(),
                Arg.Any<CancellationToken>())
            .Returns(new ApiResult<Bill> { IsSuccess = true });

        await _sut.GetById(id);

        Assert.That(capturedPath, Is.EqualTo($"{ExpectedEndpoint}/{id}"));
        await ConnectionHandler.Received(1).GetAsync<Bill>(
            $"{ExpectedEndpoint}/{id}",
            null,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// GetDocNumbers routes to the document-number validation endpoint at
    /// <c>/4.0/purchase/documentnumbers/bills</c> and forwards the caller's
    /// <c>document_no</c> as a query parameter.
    /// </summary>
    [Test]
    public async Task GetDocNumbers_CallsGetAsync_WithDocNumberPath()
    {
        const string documentNo = "AB-1234";
        QueryParameter? capturedQueryParameter = null;
        ConnectionHandler
            .GetAsync<BillDocumentNumberResponse>(
                Arg.Any<string>(),
                Arg.Do<QueryParameter?>(q => capturedQueryParameter = q),
                Arg.Any<CancellationToken>())
            .Returns(new ApiResult<BillDocumentNumberResponse>
            {
                IsSuccess = true,
                Data = new BillDocumentNumberResponse(true, null)
            });

        await _sut.GetDocNumbers(documentNo);

        await ConnectionHandler.Received(1).GetAsync<BillDocumentNumberResponse>(
            ExpectedDocNumberEndpoint,
            Arg.Any<QueryParameter?>(),
            Arg.Any<CancellationToken>());
        Assert.That(capturedQueryParameter, Is.Not.Null);
        Assert.That(capturedQueryParameter!.Parameters["document_no"], Is.EqualTo(documentNo));
    }

    /// <summary>
    /// Create forwards the <see cref="BillCreate"/> payload and the endpoint path
    /// to <see cref="IBexioConnectionHandler.PostAsync{TResult,TCreate}"/>.
    /// </summary>
    [Test]
    public async Task Create_CallsPostAsync()
    {
        var payload = BuildCreatePayload();
        ConnectionHandler
            .PostAsync<Bill, BillCreate>(Arg.Any<BillCreate>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new ApiResult<Bill> { IsSuccess = true });

        await _sut.Create(payload);

        await ConnectionHandler.Received(1).PostAsync<Bill, BillCreate>(
            payload,
            ExpectedEndpoint,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Actions posts the <see cref="BillActionRequest"/> to
    /// <c>/4.0/purchase/bills/{id}/actions</c> via <see cref="IBexioConnectionHandler.PostAsync{TResult,TCreate}"/>.
    /// </summary>
    [Test]
    public async Task Actions_CallsPostAsync_WithIdInPath()
    {
        var id = Guid.NewGuid();
        var action = new BillActionRequest(BillAction.DUPLICATE);
        string? capturedPath = null;
        ConnectionHandler
            .PostAsync<Bill, BillActionRequest>(
                Arg.Any<BillActionRequest>(),
                Arg.Do<string>(path => capturedPath = path),
                Arg.Any<CancellationToken>())
            .Returns(new ApiResult<Bill> { IsSuccess = true });

        await _sut.Actions(id, action);

        Assert.That(capturedPath, Is.EqualTo($"{ExpectedEndpoint}/{id}/actions"));
        await ConnectionHandler.Received(1).PostAsync<Bill, BillActionRequest>(
            action,
            $"{ExpectedEndpoint}/{id}/actions",
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Update calls <see cref="IBexioConnectionHandler.PutAsync{TResult,TUpdate}"/> (PUT, not POST)
    /// with the bill id appended to the endpoint root.
    /// </summary>
    [Test]
    public async Task Update_CallsPutAsync_WithIdInPath()
    {
        var id = Guid.NewGuid();
        var payload = BuildUpdatePayload();
        string? capturedPath = null;
        ConnectionHandler
            .PutAsync<Bill, BillUpdate>(
                Arg.Any<BillUpdate>(),
                Arg.Do<string>(path => capturedPath = path),
                Arg.Any<CancellationToken>())
            .Returns(new ApiResult<Bill> { IsSuccess = true });

        await _sut.Update(id, payload);

        Assert.That(capturedPath, Is.EqualTo($"{ExpectedEndpoint}/{id}"));
        await ConnectionHandler.Received(1).PutAsync<Bill, BillUpdate>(
            payload,
            $"{ExpectedEndpoint}/{id}",
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// UpdateBookings calls <see cref="IBexioConnectionHandler.PutAsync{TResult,TUpdate}"/> with
    /// the target status in the path. The request body is <see langword="null"/> because the
    /// Bexio endpoint takes the target status from the URL only.
    /// </summary>
    [Test]
    public async Task UpdateBookings_CallsPutAsync_WithBookingsPath()
    {
        var id = Guid.NewGuid();
        const BillBookingStatus status = BillBookingStatus.BOOKED;
        string? capturedPath = null;
        ConnectionHandler
            .PutAsync<Bill, object?>(
                Arg.Any<object?>(),
                Arg.Do<string>(path => capturedPath = path),
                Arg.Any<CancellationToken>())
            .Returns(new ApiResult<Bill> { IsSuccess = true });

        await _sut.UpdateBookings(id, status);

        Assert.That(capturedPath, Is.EqualTo($"{ExpectedEndpoint}/{id}/bookings/{status}"));
        await ConnectionHandler.Received(1).PutAsync<Bill, object?>(
            null,
            $"{ExpectedEndpoint}/{id}/bookings/{status}",
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Delete calls <see cref="IBexioConnectionHandler.Delete"/> with the bill id
    /// appended to the endpoint root.
    /// </summary>
    [Test]
    public async Task Delete_CallsConnectionHandlerDelete_WithIdInPath()
    {
        var id = Guid.NewGuid();
        string? capturedPath = null;
        ConnectionHandler
            .Delete(Arg.Do<string>(path => capturedPath = path), Arg.Any<CancellationToken>())
            .Returns(new ApiResult<object> { IsSuccess = true });

        await _sut.Delete(id);

        Assert.That(capturedPath, Is.EqualTo($"{ExpectedEndpoint}/{id}"));
        await ConnectionHandler.Received(1).Delete(
            $"{ExpectedEndpoint}/{id}",
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Create returns the <see cref="ApiResult{T}"/> from the connection handler unchanged.
    /// </summary>
    [Test]
    public async Task Create_ReturnsApiResultFromConnectionHandler()
    {
        var payload = BuildCreatePayload();
        var response = new ApiResult<Bill> { IsSuccess = true };
        ConnectionHandler
            .PostAsync<Bill, BillCreate>(Arg.Any<BillCreate>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(response);

        var result = await _sut.Create(payload);

        Assert.That(result, Is.SameAs(response));
    }

    /// <summary>
    /// Delete returns the <see cref="ApiResult{T}"/> from the connection handler unchanged.
    /// </summary>
    [Test]
    public async Task Delete_ReturnsApiResultFromConnectionHandler()
    {
        var response = new ApiResult<object> { IsSuccess = true };
        ConnectionHandler
            .Delete(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(response);

        var result = await _sut.Delete(Guid.NewGuid());

        Assert.That(result, Is.SameAs(response));
    }

    private static BillCreate BuildCreatePayload() =>
        new(
            SupplierId: 1,
            ContactPartnerId: 2,
            CurrencyCode: "CHF",
            Address: new BillAddress("Acme Ltd.", BillAddressType.COMPANY),
            BillDate: new DateOnly(2026, 4, 1),
            DueDate: new DateOnly(2026, 5, 1),
            ManualAmount: false,
            ItemNet: true,
            LineItems: [new BillLineItem(Position: 0, Amount: 100m)],
            Discounts: [],
            AttachmentIds: []);

    private static BillUpdate BuildUpdatePayload() =>
        new(
            SupplierId: 1,
            ContactPartnerId: 2,
            CurrencyCode: "CHF",
            Address: new BillAddress("Acme Ltd.", BillAddressType.COMPANY),
            BillDate: new DateOnly(2026, 4, 1),
            DueDate: new DateOnly(2026, 5, 1),
            ManualAmount: false,
            ItemNet: true,
            SplitIntoLineItems: false,
            LineItems: [new BillLineItem(Position: 0, Amount: 100m)],
            Discounts: [],
            AttachmentIds: []);
}
