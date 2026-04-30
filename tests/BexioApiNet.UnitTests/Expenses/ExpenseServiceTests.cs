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
using BexioApiNet.Abstractions.Models.Expenses.Expenses;
using BexioApiNet.Abstractions.Models.Expenses.Expenses.Enums;
using BexioApiNet.Abstractions.Models.Expenses.Expenses.Views;
using BexioApiNet.Models;
using BexioApiNet.Services.Connectors.Expenses;

namespace BexioApiNet.UnitTests.Expenses;

/// <summary>
/// Offline unit tests for <see cref="ExpenseService"/>. Each test asserts that the service
/// forwards its calls to <see cref="IBexioConnectionHandler"/> with the expected
/// verb, path, payload and query parameters, and returns the handler's <see cref="ApiResult{T}"/>
/// unchanged. No network, no filesystem.
/// </summary>
[TestFixture]
[Category("Unit")]
public sealed class ExpenseServiceTests : ServiceTestBase
{
    private const string ExpectedEndpoint = "4.0/expenses";
    private const string ExpectedDocNumberEndpoint = "4.0/expenses/documentnumbers";

    private ExpenseService _sut = null!;

    /// <summary>
    /// Creates a fresh <see cref="ExpenseService"/> bound to the
    /// <see cref="ServiceTestBase.ConnectionHandler"/> substitute before each test.
    /// </summary>
    [SetUp]
    public void CreateSut()
    {
        _sut = new ExpenseService(ConnectionHandler);
    }

    /// <summary>
    /// Get with no query parameter forwards a <see langword="null"/> <see cref="QueryParameter"/>
    /// to <see cref="IBexioConnectionHandler.GetAsync{TResult}"/> at the expenses collection path.
    /// </summary>
    [Test]
    public async Task Get_WithNoParams_CallsGetAsync()
    {
        var response = new ApiResult<ExpenseListResponse>
        {
            IsSuccess = true,
            Data = new ExpenseListResponse([], new ExpensePaging(1, 100, 1, 0))
        };
        ConnectionHandler
            .GetAsync<ExpenseListResponse>(Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.Get();

        await ConnectionHandler.Received(1).GetAsync<ExpenseListResponse>(
            ExpectedEndpoint,
            null,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Get forwards the supplied <see cref="QueryParameterExpense.QueryParameter"/>
    /// to <see cref="IBexioConnectionHandler.GetAsync{TResult}"/>.
    /// </summary>
    [Test]
    public async Task Get_WithQueryParameter_ForwardsQueryParameter()
    {
        var queryParameter = new QueryParameterExpense(limit: 50, page: 2);
        ConnectionHandler
            .GetAsync<ExpenseListResponse>(Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(new ApiResult<ExpenseListResponse>
            {
                IsSuccess = true,
                Data = new ExpenseListResponse([], new ExpensePaging(2, 50, 0, 0))
            });

        await _sut.Get(queryParameter);

        await ConnectionHandler.Received(1).GetAsync<ExpenseListResponse>(
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
        var response = new ApiResult<ExpenseListResponse>
        {
            IsSuccess = true,
            Data = new ExpenseListResponse([], new ExpensePaging(1, 100, 1, 0))
        };
        ConnectionHandler
            .GetAsync<ExpenseListResponse>(Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(response);

        var result = await _sut.Get();

        result.ShouldBeSameAs(response);
    }

    /// <summary>
    /// GetById calls <see cref="IBexioConnectionHandler.GetAsync{TResult}"/> with the
    /// expense id appended to the endpoint root and a <see langword="null"/> query parameter.
    /// </summary>
    [Test]
    public async Task GetById_CallsGetAsync_WithIdInPath()
    {
        var id = Guid.NewGuid();
        string? capturedPath = null;
        ConnectionHandler
            .GetAsync<Expense>(
                Arg.Do<string>(path => capturedPath = path),
                Arg.Any<QueryParameter?>(),
                Arg.Any<CancellationToken>())
            .Returns(new ApiResult<Expense> { IsSuccess = true });

        await _sut.GetById(id);

        capturedPath.ShouldBe($"{ExpectedEndpoint}/{id}");
        await ConnectionHandler.Received(1).GetAsync<Expense>(
            $"{ExpectedEndpoint}/{id}",
            null,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// GetDocNumbers routes to the document-number validation endpoint at
    /// <c>/4.0/expenses/documentnumbers</c> and forwards the caller's
    /// <c>document_no</c> as a query parameter.
    /// </summary>
    [Test]
    public async Task GetDocNumbers_CallsGetAsync_WithDocNumberPath()
    {
        const string documentNo = "AB-1234";
        QueryParameter? capturedQueryParameter = null;
        ConnectionHandler
            .GetAsync<ExpenseDocumentNumberResponse>(
                Arg.Any<string>(),
                Arg.Do<QueryParameter?>(q => capturedQueryParameter = q),
                Arg.Any<CancellationToken>())
            .Returns(new ApiResult<ExpenseDocumentNumberResponse>
            {
                IsSuccess = true,
                Data = new ExpenseDocumentNumberResponse(true, null)
            });

        await _sut.GetDocNumbers(documentNo);

        await ConnectionHandler.Received(1).GetAsync<ExpenseDocumentNumberResponse>(
            ExpectedDocNumberEndpoint,
            Arg.Any<QueryParameter?>(),
            Arg.Any<CancellationToken>());
        capturedQueryParameter.ShouldNotBeNull();
        capturedQueryParameter!.Parameters["document_no"].ShouldBe(documentNo);
    }

    /// <summary>
    /// Create forwards the <see cref="ExpenseCreate"/> payload and the endpoint path
    /// to <see cref="IBexioConnectionHandler.PostAsync{TResult,TCreate}"/>.
    /// </summary>
    [Test]
    public async Task Create_CallsPostAsync()
    {
        var payload = BuildCreatePayload();
        ConnectionHandler
            .PostAsync<Expense, ExpenseCreate>(Arg.Any<ExpenseCreate>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new ApiResult<Expense> { IsSuccess = true });

        await _sut.Create(payload);

        await ConnectionHandler.Received(1).PostAsync<Expense, ExpenseCreate>(
            payload,
            ExpectedEndpoint,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Actions posts the <see cref="ExpenseActionRequest"/> to
    /// <c>/4.0/expenses/{id}/actions</c> via <see cref="IBexioConnectionHandler.PostAsync{TResult,TCreate}"/>.
    /// </summary>
    [Test]
    public async Task Actions_CallsPostAsync_WithIdInPath()
    {
        var id = Guid.NewGuid();
        var action = new ExpenseActionRequest(ExpenseAction.DUPLICATE);
        string? capturedPath = null;
        ConnectionHandler
            .PostAsync<Expense, ExpenseActionRequest>(
                Arg.Any<ExpenseActionRequest>(),
                Arg.Do<string>(path => capturedPath = path),
                Arg.Any<CancellationToken>())
            .Returns(new ApiResult<Expense> { IsSuccess = true });

        await _sut.Actions(id, action);

        capturedPath.ShouldBe($"{ExpectedEndpoint}/{id}/actions");
        await ConnectionHandler.Received(1).PostAsync<Expense, ExpenseActionRequest>(
            action,
            $"{ExpectedEndpoint}/{id}/actions",
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Update calls <see cref="IBexioConnectionHandler.PutAsync{TResult,TUpdate}"/> (PUT, not POST)
    /// with the expense id appended to the endpoint root.
    /// </summary>
    [Test]
    public async Task Update_CallsPutAsync_WithIdInPath()
    {
        var id = Guid.NewGuid();
        var payload = BuildUpdatePayload();
        string? capturedPath = null;
        ConnectionHandler
            .PutAsync<Expense, ExpenseUpdate>(
                Arg.Any<ExpenseUpdate>(),
                Arg.Do<string>(path => capturedPath = path),
                Arg.Any<CancellationToken>())
            .Returns(new ApiResult<Expense> { IsSuccess = true });

        await _sut.Update(id, payload);

        capturedPath.ShouldBe($"{ExpectedEndpoint}/{id}");
        await ConnectionHandler.Received(1).PutAsync<Expense, ExpenseUpdate>(
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
        const ExpenseBookingStatus status = ExpenseBookingStatus.DONE;
        string? capturedPath = null;
        ConnectionHandler
            .PutAsync<Expense, object?>(
                Arg.Any<object?>(),
                Arg.Do<string>(path => capturedPath = path),
                Arg.Any<CancellationToken>())
            .Returns(new ApiResult<Expense> { IsSuccess = true });

        await _sut.UpdateBookings(id, status);

        capturedPath.ShouldBe($"{ExpectedEndpoint}/{id}/bookings/{status}");
        await ConnectionHandler.Received(1).PutAsync<Expense, object?>(
            null,
            $"{ExpectedEndpoint}/{id}/bookings/{status}",
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Delete calls <see cref="IBexioConnectionHandler.Delete"/> with the expense id
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

        capturedPath.ShouldBe($"{ExpectedEndpoint}/{id}");
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
        var response = new ApiResult<Expense> { IsSuccess = true };
        ConnectionHandler
            .PostAsync<Expense, ExpenseCreate>(Arg.Any<ExpenseCreate>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(response);

        var result = await _sut.Create(payload);

        result.ShouldBeSameAs(response);
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

        result.ShouldBeSameAs(response);
    }

    /// <summary>
    /// The cancellation token supplied by the caller of <c>Get</c> must be forwarded
    /// to the connection handler so cooperative cancellation flows end-to-end.
    /// </summary>
    [Test]
    public async Task Get_ForwardsCancellationTokenToConnectionHandler()
    {
        using var cts = new CancellationTokenSource();
        ConnectionHandler
            .GetAsync<ExpenseListResponse>(Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(new ApiResult<ExpenseListResponse>
            {
                IsSuccess = true,
                Data = new ExpenseListResponse([], new ExpensePaging(1, 100, 0, 0))
            });

        await _sut.Get(cancellationToken: cts.Token);

        await ConnectionHandler.Received(1).GetAsync<ExpenseListResponse>(
            ExpectedEndpoint,
            null,
            cts.Token);
    }

    private static ExpenseCreate BuildCreatePayload() =>
        new(
            PaidOn: new DateOnly(2026, 4, 1),
            Amount: 100m,
            CurrencyCode: "CHF",
            AttachmentIds: [],
            SupplierId: 1,
            Title: "Sample expense",
            Address: new ExpenseAddress("Acme Ltd.", ExpenseAddressType.COMPANY));

    private static ExpenseUpdate BuildUpdatePayload() =>
        new(
            PaidOn: new DateOnly(2026, 4, 1),
            CurrencyCode: "CHF",
            Amount: 100m,
            AttachmentIds: [],
            SupplierId: 1,
            DocumentNo: "LR-12345",
            Title: "Sample expense",
            Address: new ExpenseAddress("Acme Ltd.", ExpenseAddressType.COMPANY));
}
