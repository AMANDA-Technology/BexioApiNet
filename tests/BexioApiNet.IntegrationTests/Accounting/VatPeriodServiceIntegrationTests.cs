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

using BexioApiNet.Services.Connectors.Accounting;

namespace BexioApiNet.IntegrationTests.Accounting;

/// <summary>
///     Integration tests covering the <see cref="VatPeriodService" /> entry points against WireMock
///     stubs. Verifies the path composed from <see cref="VatPeriodConfiguration" />
///     (<c>3.0/accounting/vat_periods</c>) reaches the handler correctly and that the expected
///     HTTP verbs are used.
/// </summary>
public sealed class VatPeriodServiceIntegrationTests : IntegrationTestBase
{
    private const string VatPeriodsPath = "/3.0/accounting/vat_periods";

    private const string VatPeriodResponse = """
                                             {
                                                 "id": 1,
                                                 "start": "2024-01-01",
                                                 "end": "2024-03-31",
                                                 "type": "quarter",
                                                 "status": "open",
                                                 "closed_at": null
                                             }
                                             """;

    /// <summary>
    ///     <c>VatPeriodService.Get()</c> must issue a <c>GET</c> request against
    ///     <c>/3.0/accounting/vat_periods</c> and return a successful <c>ApiResult</c> when the
    ///     server returns an empty array.
    /// </summary>
    [Test]
    public async Task VatPeriodService_Get_SendsGetRequest()
    {
        Server
            .Given(Request.Create().WithPath(VatPeriodsPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("[]"));

        var service = new VatPeriodService(ConnectionHandler);

        var result = await service.Get(cancellationToken: TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(VatPeriodsPath));
        });
    }

    /// <summary>
    ///     <c>VatPeriodService.GetById</c> must issue a <c>GET</c> request that includes the
    ///     target id in the URL path and surface the returned vat period on success.
    /// </summary>
    [Test]
    public async Task VatPeriodService_GetById_SendsGetRequestWithIdInPath()
    {
        const int id = 1;
        var expectedPath = $"{VatPeriodsPath}/{id}";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(VatPeriodResponse));

        var service = new VatPeriodService(ConnectionHandler);

        var result = await service.GetById(id, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data!.Id, Is.EqualTo(id));
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
        });
    }
}
