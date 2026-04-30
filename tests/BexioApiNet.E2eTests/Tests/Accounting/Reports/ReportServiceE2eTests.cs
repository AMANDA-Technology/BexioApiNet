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

using BexioApiNet.Abstractions.Enums.Api;
using BexioApiNet.Models;
using BexioApiNet.Services.Connectors.Accounting;

namespace BexioApiNet.E2eTests.Tests.Accounting.Reports;

/// <summary>
/// Live end-to-end test for <see cref="ReportService"/>. Skipped automatically
/// when <c>BexioApiNet__BaseUri</c> / <c>BexioApiNet__JwtToken</c> are not set.
///
/// Note: <see cref="IReportService"/> is not yet exposed on <c>IBexioApiClient</c>
/// — DI wire-up is handled by sub-issue #49. The test therefore instantiates
/// <see cref="ReportService"/> directly using the credentials from the env vars.
/// The Bexio v3 OpenAPI spec only exposes <c>GET /3.0/accounting/journal</c>
/// under the Reports tag (balance_sheet and profit_loss are present in the spec
/// as path keys but contain no operations).
/// </summary>
public class ReportServiceE2eTests : BexioE2eTestBase
{
    /// <summary>
    /// Fetches the accounting journal and verifies the request succeeded
    /// and the handler returned a non-null list of entries. When the journal
    /// has data, asserts every entry against the OpenAPI <c>v3JournalResponse</c>
    /// schema.
    /// </summary>
    [Test]
    public async Task GetJournal_ReturnsSuccessfulApiResult()
    {
        Assert.That(BexioApiClient, Is.Not.Null);

        var baseUri = Environment.GetEnvironmentVariable("BexioApiNet__BaseUri");
        var jwtToken = Environment.GetEnvironmentVariable("BexioApiNet__JwtToken");

        using var connectionHandler = new BexioConnectionHandler(
            new BexioConfiguration
            {
                BaseUri = baseUri!,
                JwtToken = jwtToken!,
                AcceptHeaderFormat = ApiAcceptHeaders.JsonFormatted
            });

        var service = new ReportService(connectionHandler);

        var res = await service.GetJournal();

        Assert.That(res, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(res.IsSuccess, Is.True);
            Assert.That(res.ApiError, Is.Null);
            Assert.That(res.Data, Is.Not.Null);
        });

        foreach (var entry in res.Data!)
        {
            Assert.Multiple(() =>
            {
                Assert.That(entry.Id, Is.GreaterThan(0), "v3JournalResponse.id must be a positive integer");
                Assert.That(entry.Date, Is.Not.EqualTo(default(DateTime)));
            });
        }
    }

    /// <summary>
    /// Fetches the accounting journal with a date range filter and verifies the request
    /// succeeded — exercises the <c>from</c> / <c>to</c> query parameters at runtime.
    /// </summary>
    [Test]
    public async Task GetJournal_WithDateRange_ReturnsSuccessfulApiResult()
    {
        Assert.That(BexioApiClient, Is.Not.Null);

        var baseUri = Environment.GetEnvironmentVariable("BexioApiNet__BaseUri");
        var jwtToken = Environment.GetEnvironmentVariable("BexioApiNet__JwtToken");

        using var connectionHandler = new BexioConnectionHandler(
            new BexioConfiguration
            {
                BaseUri = baseUri!,
                JwtToken = jwtToken!,
                AcceptHeaderFormat = ApiAcceptHeaders.JsonFormatted
            });

        var service = new ReportService(connectionHandler);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var oneYearAgo = today.AddYears(-1);

        var res = await service.GetJournal(new QueryParameterJournal(From: oneYearAgo, To: today));

        Assert.That(res, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(res.IsSuccess, Is.True);
            Assert.That(res.ApiError, Is.Null);
            Assert.That(res.Data, Is.Not.Null);
        });
    }
}
