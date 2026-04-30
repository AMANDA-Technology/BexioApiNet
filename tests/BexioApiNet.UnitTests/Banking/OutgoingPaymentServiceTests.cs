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
using BexioApiNet.Abstractions.Models.Banking.OutgoingPayments;
using BexioApiNet.Abstractions.Models.Banking.OutgoingPayments.Enums;
using BexioApiNet.Abstractions.Models.Banking.OutgoingPayments.Views;
using BexioApiNet.Models;
using BexioApiNet.Services.Connectors.Banking;

namespace BexioApiNet.UnitTests.Banking;

/// <summary>
/// Offline unit tests for <see cref="OutgoingPaymentService"/>. Each test asserts that the
/// service forwards its calls to <see cref="IBexioConnectionHandler"/> with the expected
/// verb, path, payload, and query parameters, and returns the handler's
/// <see cref="ApiResult{T}"/> unchanged. No network, no filesystem.
/// </summary>
[TestFixture]
public sealed class OutgoingPaymentServiceTests : ServiceTestBase
{
    private const string ExpectedEndpoint = "4.0/purchase/outgoing-payments";

    private OutgoingPaymentService _sut = null!;

    /// <summary>
    /// Creates a fresh <see cref="OutgoingPaymentService"/> bound to the
    /// <see cref="ServiceTestBase.ConnectionHandler"/> substitute before each test.
    /// </summary>
    [SetUp]
    public void CreateSut()
    {
        _sut = new OutgoingPaymentService(ConnectionHandler);
    }

    /// <summary>
    /// Get forwards the caller's <see cref="QueryParameterOutgoingPayment.QueryParameter"/>
    /// to <see cref="IBexioConnectionHandler.GetAsync{TResult}"/> exactly once at the
    /// <c>4.0/purchase/outgoing-payments</c> path.
    /// </summary>
    [Test]
    public async Task Get_CallsGetAsyncWithExpectedPathAndQueryParameter()
    {
        var queryParameter = new QueryParameterOutgoingPayment(Guid.NewGuid());
        var response = new ApiResult<OutgoingPaymentListResponse>
        {
            IsSuccess = true,
            Data = new OutgoingPaymentListResponse([], new OutgoingPaymentPaging(1, 100, 1, 0))
        };
        ConnectionHandler
            .GetAsync<OutgoingPaymentListResponse>(
                Arg.Any<string>(),
                Arg.Any<QueryParameter?>(),
                Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.Get(queryParameter);

        await ConnectionHandler.Received(1).GetAsync<OutgoingPaymentListResponse>(
            ExpectedEndpoint,
            queryParameter.QueryParameter,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Get returns the <see cref="ApiResult{T}"/> produced by the connection handler
    /// unchanged — the service may not rewrap or mutate the result.
    /// </summary>
    [Test]
    public async Task Get_ReturnsApiResultFromConnectionHandler()
    {
        var queryParameter = new QueryParameterOutgoingPayment(Guid.NewGuid());
        var response = new ApiResult<OutgoingPaymentListResponse>
        {
            IsSuccess = true,
            Data = new OutgoingPaymentListResponse([], new OutgoingPaymentPaging(1, 100, 1, 0))
        };
        ConnectionHandler
            .GetAsync<OutgoingPaymentListResponse>(
                Arg.Any<string>(),
                Arg.Any<QueryParameter?>(),
                Arg.Any<CancellationToken>())
            .Returns(response);

        var result = await _sut.Get(queryParameter);

        Assert.That(result, Is.SameAs(response));
    }

    /// <summary>
    /// Get forwards the caller-supplied cancellation token to the connection handler so
    /// cooperative cancellation flows end-to-end.
    /// </summary>
    [Test]
    public async Task Get_ForwardsCancellationToken()
    {
        using var cts = new CancellationTokenSource();
        var queryParameter = new QueryParameterOutgoingPayment(Guid.NewGuid());
        ConnectionHandler
            .GetAsync<OutgoingPaymentListResponse>(
                Arg.Any<string>(),
                Arg.Any<QueryParameter?>(),
                Arg.Any<CancellationToken>())
            .Returns(new ApiResult<OutgoingPaymentListResponse>
            {
                IsSuccess = true,
                Data = new OutgoingPaymentListResponse([], new OutgoingPaymentPaging(1, 100, 1, 0))
            });

        await _sut.Get(queryParameter, cts.Token);

        await ConnectionHandler.Received(1).GetAsync<OutgoingPaymentListResponse>(
            ExpectedEndpoint,
            queryParameter.QueryParameter,
            cts.Token);
    }

    /// <summary>
    /// <see cref="QueryParameterOutgoingPayment"/> always emits the mandatory <c>bill_id</c>
    /// query parameter even when no other parameters are set — the Bexio v4.0 spec requires it.
    /// </summary>
    [Test]
    public void QueryParameterOutgoingPayment_BillIdAlwaysEmitted()
    {
        var billId = Guid.NewGuid();
        var queryParameter = new QueryParameterOutgoingPayment(billId);

        Assert.That(queryParameter.QueryParameter, Is.Not.Null);
        Assert.That(queryParameter.QueryParameter.Parameters, Contains.Key("bill_id"));
        Assert.That(queryParameter.QueryParameter.Parameters["bill_id"], Is.EqualTo(billId));
    }

    /// <summary>
    /// <see cref="QueryParameterOutgoingPayment"/> emits all four optional parameters
    /// (<c>limit</c>, <c>page</c>, <c>order</c>, <c>sort</c>) under the keys defined by Bexio
    /// when the caller supplies them.
    /// </summary>
    [Test]
    public void QueryParameterOutgoingPayment_EmitsAllOptionalParameters()
    {
        var billId = Guid.NewGuid();

        var queryParameter = new QueryParameterOutgoingPayment(
            billId,
            limit: 50,
            page: 2,
            order: "desc",
            sort: "execution_date");

        Assert.That(queryParameter.QueryParameter.Parameters, Has.Count.EqualTo(5));
        Assert.That(queryParameter.QueryParameter.Parameters["bill_id"], Is.EqualTo(billId));
        Assert.That(queryParameter.QueryParameter.Parameters["limit"], Is.EqualTo(50));
        Assert.That(queryParameter.QueryParameter.Parameters["page"], Is.EqualTo(2));
        Assert.That(queryParameter.QueryParameter.Parameters["order"], Is.EqualTo("desc"));
        Assert.That(queryParameter.QueryParameter.Parameters["sort"], Is.EqualTo("execution_date"));
    }

    /// <summary>
    /// GetById calls <see cref="IBexioConnectionHandler.GetAsync{TResult}"/> with the
    /// payment id appended to the endpoint root and a <see langword="null"/> query parameter.
    /// </summary>
    [Test]
    public async Task GetById_CallsGetAsyncWithIdInPath()
    {
        var id = Guid.NewGuid();
        string? capturedPath = null;
        ConnectionHandler
            .GetAsync<OutgoingPayment>(
                Arg.Do<string>(path => capturedPath = path),
                Arg.Any<QueryParameter?>(),
                Arg.Any<CancellationToken>())
            .Returns(new ApiResult<OutgoingPayment> { IsSuccess = true });

        await _sut.GetById(id);

        Assert.That(capturedPath, Is.EqualTo($"{ExpectedEndpoint}/{id}"));
        await ConnectionHandler.Received(1).GetAsync<OutgoingPayment>(
            $"{ExpectedEndpoint}/{id}",
            null,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// GetById returns the <see cref="ApiResult{T}"/> from the connection handler unchanged.
    /// </summary>
    [Test]
    public async Task GetById_ReturnsApiResultFromConnectionHandler()
    {
        var id = Guid.NewGuid();
        var response = new ApiResult<OutgoingPayment> { IsSuccess = true };
        ConnectionHandler
            .GetAsync<OutgoingPayment>(
                Arg.Any<string>(),
                Arg.Any<QueryParameter?>(),
                Arg.Any<CancellationToken>())
            .Returns(response);

        var result = await _sut.GetById(id);

        Assert.That(result, Is.SameAs(response));
    }

    /// <summary>
    /// GetById forwards the caller-supplied cancellation token to the connection handler
    /// so cooperative cancellation flows end-to-end.
    /// </summary>
    [Test]
    public async Task GetById_ForwardsCancellationToken()
    {
        using var cts = new CancellationTokenSource();
        var id = Guid.NewGuid();
        ConnectionHandler
            .GetAsync<OutgoingPayment>(
                Arg.Any<string>(),
                Arg.Any<QueryParameter?>(),
                Arg.Any<CancellationToken>())
            .Returns(new ApiResult<OutgoingPayment> { IsSuccess = true });

        await _sut.GetById(id, cts.Token);

        await ConnectionHandler.Received(1).GetAsync<OutgoingPayment>(
            $"{ExpectedEndpoint}/{id}",
            null,
            cts.Token);
    }

    /// <summary>
    /// Create forwards the <see cref="OutgoingPaymentCreate"/> payload and endpoint
    /// path to <see cref="IBexioConnectionHandler.PostAsync{TResult,TCreate}"/>.
    /// </summary>
    [Test]
    public async Task Create_CallsPostAsyncWithPayloadAndPath()
    {
        var payload = BuildCreatePayload();
        var response = new ApiResult<OutgoingPayment> { IsSuccess = true };
        ConnectionHandler
            .PostAsync<OutgoingPayment, OutgoingPaymentCreate>(
                Arg.Any<OutgoingPaymentCreate>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.Create(payload);

        await ConnectionHandler.Received(1).PostAsync<OutgoingPayment, OutgoingPaymentCreate>(
            payload,
            ExpectedEndpoint,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Create returns the <see cref="ApiResult{T}"/> from the connection handler unchanged.
    /// </summary>
    [Test]
    public async Task Create_ReturnsApiResultFromConnectionHandler()
    {
        var payload = BuildCreatePayload();
        var response = new ApiResult<OutgoingPayment> { IsSuccess = true };
        ConnectionHandler
            .PostAsync<OutgoingPayment, OutgoingPaymentCreate>(
                Arg.Any<OutgoingPaymentCreate>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>())
            .Returns(response);

        var result = await _sut.Create(payload);

        Assert.That(result, Is.SameAs(response));
    }

    /// <summary>
    /// Create forwards the caller-supplied cancellation token to the connection handler.
    /// </summary>
    [Test]
    public async Task Create_ForwardsCancellationToken()
    {
        using var cts = new CancellationTokenSource();
        var payload = BuildCreatePayload();
        ConnectionHandler
            .PostAsync<OutgoingPayment, OutgoingPaymentCreate>(
                Arg.Any<OutgoingPaymentCreate>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>())
            .Returns(new ApiResult<OutgoingPayment> { IsSuccess = true });

        await _sut.Create(payload, cts.Token);

        await ConnectionHandler.Received(1).PostAsync<OutgoingPayment, OutgoingPaymentCreate>(
            payload,
            ExpectedEndpoint,
            cts.Token);
    }

    /// <summary>
    /// Update calls <see cref="IBexioConnectionHandler.PutAsync{TResult,TUpdate}"/> with the
    /// <see cref="OutgoingPaymentUpdate"/> payload and the endpoint root. The target payment
    /// id travels in the request body (<see cref="OutgoingPaymentUpdate.PaymentId"/>) — the
    /// Bexio v4.0 PUT endpoint has no <c>{id}</c> segment in the URL.
    /// </summary>
    [Test]
    public async Task Update_CallsPutAsyncWithPayloadAndRootPath()
    {
        var payload = BuildUpdatePayload();
        string? capturedPath = null;
        ConnectionHandler
            .PutAsync<OutgoingPayment, OutgoingPaymentUpdate>(
                Arg.Any<OutgoingPaymentUpdate>(),
                Arg.Do<string>(path => capturedPath = path),
                Arg.Any<CancellationToken>())
            .Returns(new ApiResult<OutgoingPayment> { IsSuccess = true });

        await _sut.Update(payload);

        Assert.That(capturedPath, Is.EqualTo(ExpectedEndpoint));
        await ConnectionHandler.Received(1).PutAsync<OutgoingPayment, OutgoingPaymentUpdate>(
            payload,
            ExpectedEndpoint,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Update returns the <see cref="ApiResult{T}"/> from the connection handler unchanged.
    /// </summary>
    [Test]
    public async Task Update_ReturnsApiResultFromConnectionHandler()
    {
        var payload = BuildUpdatePayload();
        var response = new ApiResult<OutgoingPayment> { IsSuccess = true };
        ConnectionHandler
            .PutAsync<OutgoingPayment, OutgoingPaymentUpdate>(
                Arg.Any<OutgoingPaymentUpdate>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>())
            .Returns(response);

        var result = await _sut.Update(payload);

        Assert.That(result, Is.SameAs(response));
    }

    /// <summary>
    /// Delete calls <see cref="IBexioConnectionHandler.Delete"/> with the payment id
    /// appended to the endpoint root.
    /// </summary>
    [Test]
    public async Task Delete_CallsConnectionHandlerDeleteWithIdInPath()
    {
        var id = Guid.NewGuid();
        string? capturedPath = null;
        ConnectionHandler
            .Delete(
                Arg.Do<string>(path => capturedPath = path),
                Arg.Any<CancellationToken>())
            .Returns(new ApiResult<object> { IsSuccess = true });

        await _sut.Delete(id);

        Assert.That(capturedPath, Is.EqualTo($"{ExpectedEndpoint}/{id}"));
        await ConnectionHandler.Received(1).Delete(
            $"{ExpectedEndpoint}/{id}",
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Delete returns the <see cref="ApiResult{T}"/> from the connection handler unchanged.
    /// </summary>
    [Test]
    public async Task Delete_ReturnsApiResultFromConnectionHandler()
    {
        var id = Guid.NewGuid();
        var response = new ApiResult<object> { IsSuccess = true };
        ConnectionHandler
            .Delete(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(response);

        var result = await _sut.Delete(id);

        Assert.That(result, Is.SameAs(response));
    }

    /// <summary>
    /// Delete forwards the caller-supplied cancellation token to the connection handler.
    /// </summary>
    [Test]
    public async Task Delete_ForwardsCancellationToken()
    {
        using var cts = new CancellationTokenSource();
        var id = Guid.NewGuid();
        ConnectionHandler
            .Delete(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new ApiResult<object> { IsSuccess = true });

        await _sut.Delete(id, cts.Token);

        await ConnectionHandler.Received(1).Delete(
            $"{ExpectedEndpoint}/{id}",
            cts.Token);
    }

    private static OutgoingPaymentCreate BuildCreatePayload() =>
        new(
            BillId: Guid.NewGuid(),
            PaymentType: OutgoingPaymentType.IBAN,
            ExecutionDate: new DateOnly(2026, 4, 20),
            Amount: 100.00m,
            CurrencyCode: "CHF",
            ExchangeRate: 1m,
            SenderBankAccountId: 1,
            IsSalaryPayment: false);

    private static OutgoingPaymentUpdate BuildUpdatePayload() =>
        new(
            PaymentId: Guid.NewGuid(),
            ExecutionDate: new DateOnly(2026, 4, 20),
            Amount: 150.00m,
            IsSalaryPayment: false);
}
