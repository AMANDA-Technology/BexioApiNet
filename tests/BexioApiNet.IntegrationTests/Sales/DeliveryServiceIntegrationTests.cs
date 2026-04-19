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

using BexioApiNet.Services.Connectors.Sales;

namespace BexioApiNet.IntegrationTests.Sales;

/// <summary>
/// Integration tests covering the full surface of <see cref="DeliveryService"/> against
/// WireMock stubs. Verifies the path composed from <see cref="DeliveryConfiguration"/>
/// (<c>2.0/kb_delivery</c>) reaches the handler correctly, that the expected HTTP verbs are
/// used (including <c>POST</c>-without-body for the <c>/issue</c> action endpoint), and that
/// the <see cref="Abstractions.Models.Sales.Deliveries.Delivery"/> payload is deserialized
/// with the expected snake_case field names.
/// </summary>
public sealed class DeliveryServiceIntegrationTests : IntegrationTestBase
{
    private const string DeliveriesPath = "/2.0/kb_delivery";

    private const string DeliveryResponse = """
        {
            "id": 1,
            "document_nr": "LS-1000",
            "title": "Test delivery",
            "contact_id": 42,
            "contact_sub_id": null,
            "user_id": 1,
            "logopaper_id": null,
            "language_id": null,
            "bank_account_id": null,
            "currency_id": 1,
            "header": null,
            "footer": null,
            "total_gross": "100.00",
            "total_net": "92.00",
            "total_taxes": "8.00",
            "total": "100.00",
            "total_rounding_difference": 0.0,
            "mwst_type": 0,
            "mwst_is_net": true,
            "is_valid_from": "2026-04-01",
            "contact_address": null,
            "delivery_address_type": 0,
            "delivery_address": null,
            "kb_item_status_id": 10,
            "api_reference": null,
            "viewed_by_client_at": null,
            "updated_at": "2026-04-01 12:00:00",
            "taxs": [],
            "positions": []
        }
        """;

    /// <summary>
    /// <c>DeliveryService.Get()</c> must issue a <c>GET</c> request against
    /// <c>/2.0/kb_delivery</c> and return a successful <c>ApiResult</c> when the server
    /// returns an empty array.
    /// </summary>
    [Test]
    public async Task DeliveryService_Get_SendsGetRequest()
    {
        Server
            .Given(Request.Create().WithPath(DeliveriesPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("[]"));

        var service = new DeliveryService(ConnectionHandler);

        var result = await service.Get(cancellationToken: TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(DeliveriesPath));
        });
    }

    /// <summary>
    /// <c>DeliveryService.GetById</c> must issue a <c>GET</c> request that includes the target
    /// id in the URL path and surface the returned delivery on success.
    /// </summary>
    [Test]
    public async Task DeliveryService_GetById_SendsGetRequest()
    {
        const int id = 1;
        var expectedPath = $"{DeliveriesPath}/{id}";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(DeliveryResponse));

        var service = new DeliveryService(ConnectionHandler);

        var result = await service.GetById(id, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data!.Id, Is.EqualTo(id));
            Assert.That(result.Data.DocumentNr, Is.EqualTo("LS-1000"));
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
        });
    }

    /// <summary>
    /// <c>DeliveryService.Issue</c> must send a body-less <c>POST</c> against
    /// <c>/2.0/kb_delivery/{id}/issue</c>.
    /// </summary>
    [Test]
    public async Task DeliveryService_Issue_SendsPostRequest_ToIssuePath()
    {
        const int id = 1;
        var expectedPath = $"{DeliveriesPath}/{id}/issue";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("{\"success\":true}"));

        var service = new DeliveryService(ConnectionHandler);

        var result = await service.Issue(id, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("POST"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
        });
    }
}
