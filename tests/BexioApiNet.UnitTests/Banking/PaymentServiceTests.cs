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
using BexioApiNet.Abstractions.Models.Banking.Payments;
using BexioApiNet.Abstractions.Models.Banking.Payments.Enums;
using BexioApiNet.Abstractions.Models.Banking.Payments.Views;
using BexioApiNet.Models;
using BexioApiNet.Services.Connectors.Banking;

namespace BexioApiNet.UnitTests.Banking;

/// <summary>
/// Offline unit tests for <see cref="PaymentService"/>. Each test verifies that the service
/// forwards its calls to <see cref="IBexioConnectionHandler"/> with the expected verb, path,
/// and payload, and that the <see cref="ApiResult{T}"/> produced by the handler flows back
/// to the caller unchanged.
/// </summary>
[TestFixture]
public sealed class PaymentServiceTests : ServiceTestBase
{
    private const string ExpectedEndpoint = "4.0/banking/payments";

    private PaymentService _sut = null!;

    /// <summary>
    /// Creates a fresh <see cref="PaymentService"/> per test, bound to the
    /// <see cref="ServiceTestBase.ConnectionHandler"/> substitute.
    /// </summary>
    [SetUp]
    public void CreateSut()
    {
        _sut = new PaymentService(ConnectionHandler);
    }

    /// <summary>
    /// Get (no parameters) calls <see cref="IBexioConnectionHandler.GetAsync{TResult}"/> once
    /// with the expected endpoint and a <see langword="null"/> query parameter.
    /// </summary>
    [Test]
    public async Task Get_WithNoParams_CallsGetAsyncWithExpectedPath()
    {
        var expected = new ApiResult<List<Payment>?> { IsSuccess = true, Data = [] };
        ConnectionHandler
            .GetAsync<List<Payment>?>(Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(expected);

        var result = await _sut.Get();

        Assert.That(result, Is.SameAs(expected));
        await ConnectionHandler.Received(1).GetAsync<List<Payment>?>(
            ExpectedEndpoint,
            null,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Get forwards the inner <see cref="QueryParameter"/> from the supplied
    /// <see cref="QueryParameterPayment"/> to the connection handler verbatim.
    /// </summary>
    [Test]
    public async Task Get_WithQueryParameter_PassesQueryParameterThrough()
    {
        var queryParameter = new QueryParameterPayment(Page: 2, PerPage: 50, FilterBy: "status=open");
        ConnectionHandler
            .GetAsync<List<Payment>?>(Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(new ApiResult<List<Payment>?> { IsSuccess = true, Data = [] });

        await _sut.Get(queryParameter);

        await ConnectionHandler.Received(1).GetAsync<List<Payment>?>(
            ExpectedEndpoint,
            queryParameter.QueryParameter,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Get forwards the caller-supplied cancellation token to the connection handler
    /// so cooperative cancellation flows end-to-end.
    /// </summary>
    [Test]
    public async Task Get_ForwardsCancellationToken()
    {
        using var cts = new CancellationTokenSource();
        ConnectionHandler
            .GetAsync<List<Payment>?>(Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(new ApiResult<List<Payment>?> { IsSuccess = true, Data = [] });

        await _sut.Get(cancellationToken: cts.Token);

        await ConnectionHandler.Received(1).GetAsync<List<Payment>?>(
            ExpectedEndpoint,
            null,
            cts.Token);
    }

    /// <summary>
    /// GetById calls <see cref="IBexioConnectionHandler.GetAsync{TResult}"/> with the
    /// path composed from the endpoint root and the payment UUID.
    /// </summary>
    [Test]
    public async Task GetById_PathContainsPaymentId()
    {
        var paymentId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        string? capturedPath = null;
        ConnectionHandler
            .GetAsync<Payment>(
                Arg.Do<string>(path => capturedPath = path),
                Arg.Any<QueryParameter?>(),
                Arg.Any<CancellationToken>())
            .Returns(new ApiResult<Payment> { IsSuccess = true });

        await _sut.GetById(paymentId);

        Assert.That(capturedPath, Is.EqualTo($"{ExpectedEndpoint}/{paymentId}"));
        await ConnectionHandler.Received(1).GetAsync<Payment>(
            Arg.Any<string>(),
            null,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// GetById returns the <see cref="ApiResult{T}"/> produced by the connection handler
    /// without any additional processing.
    /// </summary>
    [Test]
    public async Task GetById_ReturnsApiResultFromConnectionHandler()
    {
        var paymentId = Guid.NewGuid();
        var response = new ApiResult<Payment> { IsSuccess = true };
        ConnectionHandler
            .GetAsync<Payment>(Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(response);

        var result = await _sut.GetById(paymentId);

        Assert.That(result, Is.SameAs(response));
    }

    /// <summary>
    /// Create forwards the payload and the expected endpoint path to
    /// <see cref="IBexioConnectionHandler.PostAsync{TResult,TCreate}"/>.
    /// </summary>
    [Test]
    public async Task Create_CallsPostAsync()
    {
        var payload = BuildCreatePayload();
        var response = new ApiResult<Payment> { IsSuccess = true };
        ConnectionHandler
            .PostAsync<Payment, PaymentCreate>(
                Arg.Any<PaymentCreate>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>())
            .Returns(response);

        var result = await _sut.Create(payload);

        Assert.That(result, Is.SameAs(response));
        await ConnectionHandler.Received(1).PostAsync<Payment, PaymentCreate>(
            payload,
            ExpectedEndpoint,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Cancel POSTs to <c>{id}/cancel</c> via
    /// <see cref="IBexioConnectionHandler.PostActionAsync{TResult}"/> — no request body is sent
    /// and the endpoint suffix must be <c>cancel</c>.
    /// </summary>
    [Test]
    public async Task Cancel_CallsPostActionAsyncOnCancelPath()
    {
        var paymentId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        string? capturedPath = null;
        ConnectionHandler
            .PostActionAsync<Payment>(
                Arg.Do<string>(path => capturedPath = path),
                Arg.Any<CancellationToken>())
            .Returns(new ApiResult<Payment> { IsSuccess = true });

        await _sut.Cancel(paymentId);

        Assert.That(capturedPath, Is.EqualTo($"{ExpectedEndpoint}/{paymentId}/cancel"));
        await ConnectionHandler.Received(1).PostActionAsync<Payment>(
            Arg.Any<string>(),
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Update forwards the payload and the expected endpoint path with the payment UUID to
    /// <see cref="IBexioConnectionHandler.PutAsync{TResult,TUpdate}"/>.
    /// </summary>
    [Test]
    public async Task Update_CallsPutAsync()
    {
        var paymentId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        var payload = new PaymentUpdate(Amount: 42.5m);
        string? capturedPath = null;
        ConnectionHandler
            .PutAsync<Payment, PaymentUpdate>(
                Arg.Any<PaymentUpdate>(),
                Arg.Do<string>(path => capturedPath = path),
                Arg.Any<CancellationToken>())
            .Returns(new ApiResult<Payment> { IsSuccess = true });

        await _sut.Update(paymentId, payload);

        Assert.That(capturedPath, Is.EqualTo($"{ExpectedEndpoint}/{paymentId}"));
        await ConnectionHandler.Received(1).PutAsync<Payment, PaymentUpdate>(
            payload,
            Arg.Any<string>(),
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Delete calls <see cref="IBexioConnectionHandler.Delete"/> with the path composed from
    /// the endpoint root and the payment UUID, and returns the handler's result unchanged.
    /// </summary>
    [Test]
    public async Task Delete_CallsConnectionHandlerDeleteOnIdPath()
    {
        var paymentId = Guid.Parse("44444444-4444-4444-4444-444444444444");
        string? capturedPath = null;
        var response = new ApiResult<object> { IsSuccess = true };
        ConnectionHandler
            .Delete(
                Arg.Do<string>(path => capturedPath = path),
                Arg.Any<CancellationToken>())
            .Returns(response);

        var result = await _sut.Delete(paymentId);

        Assert.That(result, Is.SameAs(response));
        Assert.That(capturedPath, Is.EqualTo($"{ExpectedEndpoint}/{paymentId}"));
    }

    private static PaymentCreate BuildCreatePayload() =>
        new(
            Type: PaymentType.iban,
            AccountId: Guid.Parse("55555555-5555-5555-5555-555555555555"),
            Recipient: new PaymentRecipient(
                Name: "Alice Example",
                Iban: "CH9300762011623852957",
                Address: new PaymentAddress(
                    StreetName: "Bahnhofstrasse",
                    HouseNumber: "1",
                    Zip: "8001",
                    City: "Zurich",
                    CountryCode: "CH")),
            Amount: 100m,
            Currency: "CHF",
            ExecutionDate: new DateOnly(2026, 5, 1),
            IsSalary: false);
}
