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
///     Integration tests covering the <see cref="TaxService" /> entry points against WireMock stubs.
///     Verifies the path composed from <see cref="TaxConfiguration" /> (<c>3.0/taxes</c>)
///     reaches the handler correctly and that the expected HTTP verbs are used.
/// </summary>
public sealed class TaxServiceIntegrationTests : IntegrationTestBase
{
    private const string TaxesPath = "/3.0/taxes";

    private const string TaxResponse = """
                                       {
                                           "id": 1,
                                           "uuid": "abc-123",
                                           "name": "MwSt 8.1%",
                                           "code": "UN81",
                                           "digit": "312",
                                           "type": "sales_tax",
                                           "account_id": 2200,
                                           "tax_settlement_type": "none",
                                           "value": 8.1,
                                           "net_tax_value": null,
                                           "start_year": 2024,
                                           "end_year": null,
                                           "is_active": true,
                                           "display_name": "MwSt 8.1% (312)",
                                           "start_month": 1,
                                           "end_month": null
                                       }
                                       """;

    /// <summary>
    ///     <c>TaxService.Get()</c> must issue a <c>GET</c> request against <c>/3.0/taxes</c>
    ///     and return a successful <c>ApiResult</c> when the server returns an empty array.
    /// </summary>
    [Test]
    public async Task TaxService_Get_SendsGetRequest()
    {
        Server
            .Given(Request.Create().WithPath(TaxesPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("[]"));

        var service = new TaxService(ConnectionHandler);

        var result = await service.Get(cancellationToken: TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(TaxesPath));
        });
    }

    /// <summary>
    ///     <c>TaxService.GetById</c> must issue a <c>GET</c> request that includes the target id
    ///     in the URL path and surface the returned tax on success.
    /// </summary>
    [Test]
    public async Task TaxService_GetById_SendsGetRequestWithIdInPath()
    {
        const int id = 1;
        var expectedPath = $"{TaxesPath}/{id}";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(TaxResponse));

        var service = new TaxService(ConnectionHandler);

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

    /// <summary>
    ///     <c>TaxService.Delete</c> must issue a <c>DELETE</c> request that includes the target id
    ///     in the URL path.
    /// </summary>
    [Test]
    public async Task TaxService_Delete_SendsDeleteRequest()
    {
        const int id = 1;
        var expectedPath = $"{TaxesPath}/{id}";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingDelete())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("{\"success\":true}"));

        var service = new TaxService(ConnectionHandler);

        var result = await service.Delete(id, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("DELETE"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
        });
    }
}
