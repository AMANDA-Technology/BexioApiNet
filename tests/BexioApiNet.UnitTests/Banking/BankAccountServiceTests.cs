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

    private static BankAccountGet NewBankAccount(int id) =>
        new(
            Id: id,
            Name: $"Bank {id}",
            Owner: "owner",
            OwnerAddress: "addr",
            OwnerZip: "0000",
            OwnerCity: "city",
            BcNr: "bc",
            BankName: "bank",
            BankNr: "bnr",
            BankAccountNr: "ba",
            IbanNr: "iban",
            CurrencyId: null,
            AccountId: null,
            Remarks: "",
            InvoiceMode: "",
            QrInvoiceIban: "",
            Type: "");
}
