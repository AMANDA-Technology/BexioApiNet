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

using BexioApiNet.Abstractions.Models.Accounting.Reports;
using BexioApiNet.Abstractions.Models.Api;
using BexioApiNet.Models;
using BexioApiNet.Services.Connectors.Accounting;

namespace BexioApiNet.UnitTests.Accounting;

/// <summary>
/// Offline unit tests for <see cref="ReportService"/>. Verifies the connector
/// builds the right request path, forwards query parameters and cancellation
/// tokens, and returns the connection handler's <see cref="ApiResult{T}"/>
/// unchanged.
/// </summary>
[TestFixture]
public sealed class ReportServiceTests : ServiceTestBase
{
    private const string ExpectedPath = "3.0/accounting/journal";

    /// <summary>
    /// With no parameters the service hits <c>3.0/accounting/journal</c> exactly
    /// once with a <see langword="null"/> query parameter and returns the
    /// connection handler's <see cref="ApiResult{T}"/> as-is.
    /// </summary>
    [Test]
    public async Task GetJournal_WithNoParams_CallsGetAsyncOnceWithExpectedPath()
    {
        var expected = new ApiResult<List<Journal>>
        {
            IsSuccess = true,
            Data = [NewJournal(1)]
        };
        ConnectionHandler
            .GetAsync<List<Journal>>(Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

        var service = new ReportService(ConnectionHandler);

        var result = await service.GetJournal();

        Assert.That(result, Is.SameAs(expected));
        await ConnectionHandler.Received(1).GetAsync<List<Journal>>(
            ExpectedPath,
            null,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Supplied query parameters must be forwarded to the connection handler
    /// so date-range / account-uuid filters reach the Bexio endpoint.
    /// </summary>
    [Test]
    public async Task GetJournal_WithQueryParameters_ForwardsThemToConnectionHandler()
    {
        var query = new QueryParameterJournal(
            From: new DateOnly(2026, 1, 1),
            To: new DateOnly(2026, 12, 31),
            AccountUuid: "d591c997-5e88-486b-8fca-48dfd984d45d",
            Limit: 500,
            Offset: 0);
        ConnectionHandler
            .GetAsync<List<Journal>>(Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new ApiResult<List<Journal>> { IsSuccess = true, Data = [] }));

        var service = new ReportService(ConnectionHandler);

        await service.GetJournal(query);

        await ConnectionHandler.Received(1).GetAsync<List<Journal>>(
            ExpectedPath,
            query.QueryParameter,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// The cancellation token supplied by the caller must be forwarded to the
    /// connection handler so cooperative cancellation flows end-to-end.
    /// </summary>
    [Test]
    public async Task GetJournal_ForwardsCancellationTokenToConnectionHandler()
    {
        using var cts = new CancellationTokenSource();
        ConnectionHandler
            .GetAsync<List<Journal>>(Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new ApiResult<List<Journal>> { IsSuccess = true, Data = [] }));

        var service = new ReportService(ConnectionHandler);

        await service.GetJournal(cancellationToken: cts.Token);

        await ConnectionHandler.Received(1).GetAsync<List<Journal>>(
            ExpectedPath,
            null,
            cts.Token);
    }

    /// <summary>
    /// A failing <see cref="ApiResult{T}"/> from the connection handler must
    /// surface to the caller untouched — the service may not swallow errors.
    /// </summary>
    [Test]
    public async Task GetJournal_ReturnsApiResultFromConnectionHandlerUnchanged()
    {
        var apiError = new ApiError(ErrorCode: 404, Message: "not found", Errors: new object());
        var response = new ApiResult<List<Journal>>
        {
            IsSuccess = false,
            ApiError = apiError,
            StatusCode = System.Net.HttpStatusCode.NotFound,
            Data = null
        };
        ConnectionHandler
            .GetAsync<List<Journal>>(Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(response));

        var service = new ReportService(ConnectionHandler);

        var result = await service.GetJournal();

        Assert.That(result, Is.SameAs(response));
    }

    private static Journal NewJournal(int id) =>
        new(
            Id: id,
            RefId: 13,
            RefUuid: $"uuid-{id}",
            RefClass: "KbInvoice",
            Date: new DateTime(2026, 2, 17, 0, 0, 0, DateTimeKind.Utc),
            DebitAccountId: 77,
            CreditAccountId: 139,
            Description: "E2E description",
            Amount: 328.25m,
            CurrencyId: 1,
            CurrencyFactor: 1m,
            BaseCurrencyId: 1,
            BaseCurrencyAmount: 328.25m);
}
