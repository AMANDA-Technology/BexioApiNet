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
using BexioApiNet.Abstractions.Models.Banking.BankAccounts.Views;
using BexioApiNet.Models;
using BexioApiNet.Services.Connectors.Banking;

namespace BexioApiNet.UnitTests.Banking;

/// <summary>
/// Offline unit tests for <see cref="BankAccountService"/>. The service is a
/// thin pass-through over <see cref="IBexioConnectionHandler.GetAsync{T}"/> —
/// it never auto-pages, so verification focuses on path, query parameter, and
/// pass-through semantics.
/// </summary>
[TestFixture]
public sealed class BankAccountServiceTests : ServiceTestBase
{
    private const string ExpectedPath = "3.0/banking/accounts";

    /// <summary>
    /// With no parameters the service hits <c>3.0/banking/accounts</c> exactly
    /// once with a <see langword="null"/> query parameter and returns the
    /// connection handler's <see cref="ApiResult{T}"/> as-is.
    /// </summary>
    [Test]
    public async Task Get_WithNoParams_CallsGetAsyncOnceWithExpectedPath()
    {
        var expected = new ApiResult<List<BankAccountGet>>
        {
            IsSuccess = true,
            Data = [NewBankAccount(1)]
        };
        ConnectionHandler
            .GetAsync<List<BankAccountGet>>(Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

        var service = new BankAccountService(ConnectionHandler);

        var result = await service.Get();

        Assert.That(result, Is.SameAs(expected));
        await ConnectionHandler.Received(1).GetAsync<List<BankAccountGet>>(
            ExpectedPath,
            null,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// When a <see cref="QueryParameterBankAccount"/> is supplied, its inner
    /// <see cref="QueryParameter"/> instance is forwarded to the connection
    /// handler verbatim — the service must not rewrap or substitute it.
    /// </summary>
    [Test]
    public async Task Get_WithQueryParameter_PassesQueryParameterToConnectionHandler()
    {
        var queryParameter = new QueryParameterBankAccount(Limit: 25, Offset: 75);
        ConnectionHandler
            .GetAsync<List<BankAccountGet>>(Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new ApiResult<List<BankAccountGet>> { IsSuccess = true, Data = [] }));

        var service = new BankAccountService(ConnectionHandler);

        await service.Get(queryParameter);

        await ConnectionHandler.Received(1).GetAsync<List<BankAccountGet>>(
            ExpectedPath,
            queryParameter.QueryParameter,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// A <see cref="QueryParameterBankAccount"/> with no values supplied must produce
    /// a <see langword="null"/> <see cref="QueryParameter"/> so the request URI does
    /// not get spurious empty query parameters appended.
    /// </summary>
    [Test]
    public void QueryParameterBankAccount_WithNoValues_ProducesNullQueryParameter()
    {
        var queryParameter = new QueryParameterBankAccount();

        Assert.That(queryParameter.QueryParameter, Is.Null);
    }

    /// <summary>
    /// A <see cref="QueryParameterBankAccount"/> with only <c>Limit</c> emits just
    /// the <c>limit</c> entry — <c>offset</c> must not be added when the caller
    /// did not supply it.
    /// </summary>
    [Test]
    public void QueryParameterBankAccount_WithOnlyLimit_OmitsOffset()
    {
        var queryParameter = new QueryParameterBankAccount(Limit: 10);

        Assert.That(queryParameter.QueryParameter, Is.Not.Null);
        Assert.That(queryParameter.QueryParameter!.Parameters, Has.Count.EqualTo(1));
        Assert.That(queryParameter.QueryParameter.Parameters, Contains.Key("limit"));
        Assert.That(queryParameter.QueryParameter.Parameters["limit"], Is.EqualTo(10));
    }

    /// <summary>
    /// A <see cref="QueryParameterBankAccount"/> with both <c>Limit</c> and
    /// <c>Offset</c> emits both parameters under the keys expected by Bexio
    /// (<c>limit</c>, <c>offset</c>).
    /// </summary>
    [Test]
    public void QueryParameterBankAccount_WithLimitAndOffset_EmitsBoth()
    {
        var queryParameter = new QueryParameterBankAccount(Limit: 10, Offset: 5);

        Assert.That(queryParameter.QueryParameter, Is.Not.Null);
        Assert.That(queryParameter.QueryParameter!.Parameters, Has.Count.EqualTo(2));
        Assert.That(queryParameter.QueryParameter.Parameters["limit"], Is.EqualTo(10));
        Assert.That(queryParameter.QueryParameter.Parameters["offset"], Is.EqualTo(5));
    }

    /// <summary>
    /// The service must never invoke <c>FetchAll</c>: <see cref="BankAccountService"/>
    /// does not implement auto-paging even when <c>autoPage</c> is set.
    /// </summary>
    [Test]
    public async Task Get_WithAutoPage_DoesNotCallFetchAll()
    {
        var response = new ApiResult<List<BankAccountGet>>
        {
            IsSuccess = true,
            Data = [NewBankAccount(1)]
        };
        ConnectionHandler
            .GetAsync<List<BankAccountGet>>(Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(response));

        var service = new BankAccountService(ConnectionHandler);

        await service.Get(autoPage: true);

        await ConnectionHandler.DidNotReceive().FetchAll<BankAccountGet>(
            Arg.Any<int>(),
            Arg.Any<int>(),
            Arg.Any<string>(),
            Arg.Any<QueryParameter?>(),
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// The cancellation token supplied by the caller must be forwarded to the
    /// connection handler so cooperative cancellation flows end-to-end.
    /// </summary>
    [Test]
    public async Task Get_ForwardsCancellationTokenToConnectionHandler()
    {
        using var cts = new CancellationTokenSource();
        ConnectionHandler
            .GetAsync<List<BankAccountGet>>(Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new ApiResult<List<BankAccountGet>> { IsSuccess = true, Data = [] }));

        var service = new BankAccountService(ConnectionHandler);

        await service.Get(cancellationToken: cts.Token);

        await ConnectionHandler.Received(1).GetAsync<List<BankAccountGet>>(
            ExpectedPath,
            null,
            cts.Token);
    }

    /// <summary>
    /// A failing <see cref="ApiResult{T}"/> from the connection handler must
    /// surface to the caller untouched — the service may not swallow errors.
    /// </summary>
    [Test]
    public async Task Get_ReturnsApiResultFromConnectionHandlerUnchanged()
    {
        var apiError = new ApiError(ErrorCode: 403, Message: "forbidden", Errors: new object());
        var response = new ApiResult<List<BankAccountGet>>
        {
            IsSuccess = false,
            ApiError = apiError,
            StatusCode = System.Net.HttpStatusCode.Forbidden,
            Data = null
        };
        ConnectionHandler
            .GetAsync<List<BankAccountGet>>(Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(response));

        var service = new BankAccountService(ConnectionHandler);

        var result = await service.Get();

        Assert.That(result, Is.SameAs(response));
    }

    /// <summary>
    /// <c>GetById</c> hits <c>3.0/banking/accounts/{id}</c> exactly once with a
    /// <see langword="null"/> query parameter and returns the connection
    /// handler's <see cref="ApiResult{T}"/> as-is.
    /// </summary>
    [Test]
    public async Task GetById_WithId_CallsGetAsyncOnceWithExpectedPath()
    {
        const int id = 42;
        var expected = new ApiResult<BankAccountGet>
        {
            IsSuccess = true,
            Data = NewBankAccount(id)
        };
        ConnectionHandler
            .GetAsync<BankAccountGet>(Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

        var service = new BankAccountService(ConnectionHandler);

        var result = await service.GetById(id);

        Assert.That(result, Is.SameAs(expected));
        await ConnectionHandler.Received(1).GetAsync<BankAccountGet>(
            $"{ExpectedPath}/{id}",
            null,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// The cancellation token supplied to <c>GetById</c> must be forwarded to
    /// the connection handler so cooperative cancellation flows end-to-end.
    /// </summary>
    [Test]
    public async Task GetById_ForwardsCancellationTokenToConnectionHandler()
    {
        const int id = 7;
        using var cts = new CancellationTokenSource();
        ConnectionHandler
            .GetAsync<BankAccountGet>(Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new ApiResult<BankAccountGet> { IsSuccess = true, Data = NewBankAccount(id) }));

        var service = new BankAccountService(ConnectionHandler);

        await service.GetById(id, cts.Token);

        await ConnectionHandler.Received(1).GetAsync<BankAccountGet>(
            $"{ExpectedPath}/{id}",
            null,
            cts.Token);
    }

    /// <summary>
    /// A failing <see cref="ApiResult{T}"/> from the connection handler must
    /// surface to the <c>GetById</c> caller untouched.
    /// </summary>
    [Test]
    public async Task GetById_ReturnsApiResultFromConnectionHandlerUnchanged()
    {
        const int id = 99;
        var apiError = new ApiError(ErrorCode: 404, Message: "not found", Errors: new object());
        var response = new ApiResult<BankAccountGet>
        {
            IsSuccess = false,
            ApiError = apiError,
            StatusCode = System.Net.HttpStatusCode.NotFound,
            Data = null
        };
        ConnectionHandler
            .GetAsync<BankAccountGet>(Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(response));

        var service = new BankAccountService(ConnectionHandler);

        var result = await service.GetById(id);

        Assert.That(result, Is.SameAs(response));
    }

    private static BankAccountGet NewBankAccount(int id) =>
        new(
            Id: id,
            Name: $"Bank {id}",
            Owner: "owner",
            OwnerAddress: "addr",
            OwnerHouseNumber: "1",
            OwnerZip: "0000",
            OwnerCity: "city",
            OwnerCountryCode: "CH",
            BcNr: "bc",
            BankName: "bank",
            BankNr: "bnr",
            BankAccountNr: "ba",
            IbanNr: "iban",
            CurrencyId: null,
            AccountId: null,
            Remarks: "",
            QrInvoiceIban: "",
            InvoiceMode: null,
            IsEsr: null,
            EsrBesrId: null,
            EsrPostAccountNr: null,
            EsrPaymentForText: null,
            EsrInFavourOfText: null,
            Type: "bank");
}
