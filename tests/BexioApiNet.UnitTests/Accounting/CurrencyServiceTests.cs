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

using BexioApiNet.Abstractions.Models.Accounting.Currencies;
using BexioApiNet.Abstractions.Models.Api;
using BexioApiNet.Models;
using BexioApiNet.Services.Connectors.Accounting;

namespace BexioApiNet.UnitTests.Accounting;

/// <summary>
/// Offline unit tests for <see cref="CurrencyService"/>. Currency is a simple
/// list endpoint — the connector forwards the request and result, optionally
/// auto-paging through a <c>X-Total-Count</c> header.
/// </summary>
[TestFixture]
public sealed class CurrencyServiceTests : ServiceTestBase
{
    private const string ExpectedPath = "3.0/currencies";

    /// <summary>
    /// With no parameters the service hits <c>3.0/currencies</c> exactly once
    /// with a <see langword="null"/> query parameter and returns the connection
    /// handler's <see cref="ApiResult{T}"/> as-is.
    /// </summary>
    [Test]
    public async Task Get_WithNoParams_CallsGetAsyncOnceWithExpectedPath()
    {
        var expected = new ApiResult<List<Currency>>
        {
            IsSuccess = true,
            Data = [new Currency(Id: 1, Name: "CHF", RoundFactor: 0.05)]
        };
        ConnectionHandler
            .GetAsync<List<Currency>>(Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

        var service = new CurrencyService(ConnectionHandler);

        var result = await service.Get();

        Assert.That(result, Is.SameAs(expected));
        await ConnectionHandler.Received(1).GetAsync<List<Currency>>(
            ExpectedPath,
            null,
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
            .GetAsync<List<Currency>>(Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new ApiResult<List<Currency>> { IsSuccess = true, Data = [] }));

        var service = new CurrencyService(ConnectionHandler);

        await service.Get(cancellationToken: cts.Token);

        await ConnectionHandler.Received(1).GetAsync<List<Currency>>(
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
        var apiError = new ApiError(ErrorCode: 401, Message: "unauthorized", Errors: new object());
        var response = new ApiResult<List<Currency>>
        {
            IsSuccess = false,
            ApiError = apiError,
            StatusCode = System.Net.HttpStatusCode.Unauthorized,
            Data = null
        };
        ConnectionHandler
            .GetAsync<List<Currency>>(Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(response));

        var service = new CurrencyService(ConnectionHandler);

        var result = await service.Get();

        Assert.That(result, Is.SameAs(response));
    }
}
