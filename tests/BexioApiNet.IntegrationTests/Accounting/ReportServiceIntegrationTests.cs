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

using BexioApiNet.Models;
using BexioApiNet.Services.Connectors.Accounting;

namespace BexioApiNet.IntegrationTests.Accounting;

/// <summary>
/// Integration tests covering the <see cref="ReportService" /> entry points against WireMock
/// stubs. Verifies the path composed from <see cref="ReportConfiguration" />
/// (<c>3.0/accounting/journal</c>) reaches the handler correctly and that the expected
/// HTTP verb and query parameters are used.
/// </summary>
public sealed class ReportServiceIntegrationTests : IntegrationTestBase
{
    private const string JournalPath = "/3.0/accounting/journal";

    /// <summary>
    /// <c>ReportService.GetJournal()</c> must issue a <c>GET</c> request against
    /// <c>/3.0/accounting/journal</c> and return a successful <c>ApiResult</c> when the
    /// server returns an empty array.
    /// </summary>
    [Test]
    public async Task ReportService_GetJournal_SendsGetRequest()
    {
        Server
            .Given(Request.Create().WithPath(JournalPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("[]"));

        var service = new ReportService(ConnectionHandler);

        var result = await service.GetJournal(cancellationToken: TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(JournalPath));
        });
    }

    /// <summary>
    /// <c>ReportService.GetJournal</c> with a <see cref="QueryParameterJournal" /> must
    /// forward the <c>from</c> and <c>to</c> query parameters onto the URL.
    /// </summary>
    [Test]
    public async Task ReportService_GetJournal_WithDateRange_AppendsQueryParameters()
    {
        Server
            .Given(Request.Create().WithPath(JournalPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("[]"));

        var service = new ReportService(ConnectionHandler);

        var result = await service.GetJournal(
            new QueryParameterJournal(
                From: new DateOnly(2024, 1, 1),
                To: new DateOnly(2024, 12, 31)),
            cancellationToken: TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(JournalPath));
            Assert.That(request.RawQuery, Does.Contain("from=2024-01-01"));
            Assert.That(request.RawQuery, Does.Contain("to=2024-12-31"));
        });
    }
}
